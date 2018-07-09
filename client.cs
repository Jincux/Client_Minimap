//Minimap
// jincux 9789

exec("./clientcmd.cs");
exec("./types.cs");
exec("./dev.cs");

if(!$Minimap::defaults) {
  $Minimap::chunkSize = 16;
  $Minimap::follow = "player"; // "player", "snap", "fixed"
  $Minimap::follow::snapsize = 100;
  $Minimap::follow::position = "0 0"; // for fixed position

  $Minimap::defaults = true;
}

function mRound(%n) {
  %floor = mFloor(%n);
  if(%n - %floor < 0.5) {
    return %floor;
  } else {
    return %floor+1;
  }
}

function vectorFloor(%vec3) {
  return mFloor(getWord(%vec3, 0)) SPC mFloor(getWord(%vec3, 1)) SPC mFloor(getWord(%vec3, 2));
}


function vectorCeil(%vec3) {
  return mCeil(getWord(%vec3, 0)) SPC mCeil(getWord(%vec3, 1)) SPC mCeil(getWord(%vec3, 2));
}

function vector2Round(%vec) {
  return mRound(getWord(%vec, 0)) SPC mRound(getWord(%vec, 1));
}



function Minimap_render_loop() {
    cancel($Minimap::RenderSch);
    MinimapViewport.render();
    $Minimap::RenderSch = schedule(200, MinimapViewport, "Minimap_render_loop");
}

function Minimap::_addToChunks(%id) {
  Minimap::_removeFromChunks(%id);

  %min = $Minimap::Position[%id];
  %max = vectorAdd($Minimap::Position[%id], $Minimap::Extent[%id]);

  %minChunk = vectorFloor(vectorScale(%min, 1/$Minimap::chunkSize));
  %maxChunk = vectorCeil(vectorScale(%max, 1/$Minimap::chunkSize));

  %minChunkX = getWord(%minChunk, 0);
  %minChunkY = getWord(%minChunk, 1);
  %maxChunkX = getWord(%maxChunk, 0);
  %maxChunkY = getWord(%maxChunk, 1);

  for(%x = getWord(%minChunk, 0); %x < getWord(%maxChunk, 0); %x++) {
    for(%y = getWord(%minChunk, 1); %y < getWord(%maxChunk, 1); %y++) {
      %idx = $Minimap::chunkSize[%x, %y]+0;
      $Minimap::chunkSize[%x, %y]++;
      $Minimap::chunkId[%x, %y, %idx] = %id;

      $Minimap::chunksIn[%id] = $Minimap::chunksIn[%id] TAB %x SPC %y;
    }
  }

  if(%minChunkX < $Minimap::minChunkX) {
    $Minimap::minChunkX = %minChunkX;
    $Minimap::forceUpdate = true;
  }

  if(%minChunkY < $Minimap::minChunkY) {
    $Minimap::minChunkY = %minChunkY;
    $Minimap::forceUpdate = true;
  }

  if(%maxChunkX > $Minimap::maxChunkX) {
    $Minimap::maxChunkX = %maxChunkX;
    $Minimap::forceUpdate = true;
  }

  if(%maxChunkY > $Minimap::maxChunkY) {
    $Minimap::maxChunkY = %maxChunkY;
    $Minimap::forceUpdate = true;
  }
}

function Minimap::_removeFromChunks(%id) {
  for(%i = 0; %i < getFieldCount($Minimap::chunksIn[%id]); %i++) {
    %x = getWord(getField($Minimap::chunksIn[%id], %i), 0);
    %y = getWord(getField($Minimap::chunksIn[%id], %i), 1);

    %found = false;
    for(%cidx = 0; %cidx < $Minimap::chunkSize[%x, %y]; %cidx++) {
      if($Minimap::chunkId[%x, %y, %cidx] == %id) {
        %found = true;
      }

      if(%found) {
        $Minimap::chunkId[%x, %y, %cidx] = $Minimap::chunkId[%x, %y, %cidx+1];
      }
    }
    $Minimap::chunkSize[%x, %y]--;
  }
  $Minimap::chunksIn[%id] = "";
}

function Minimap::viewport() {
  %vp = new GuiSwatchCtrl(MinimapViewport) {
    position = "0 0";
    extent = "128 128";

    viewportScale = 1;
    viewportCenter = "0 0";

    color = "240 240 240 128";
  };

  %cont = new GuiSwatchCtrl(MinimapViewportContainer) {
    position = "0 0";
    extent = "128 128";
    color = "0 0 0 0";
  };

  %vp.container = %cont;
  %vp.add(%cont);

  if(!isObject(MinimapViewportGroup)) {
    new SimSet(MinimapViewportGroup);
  }

  MinimapViewportGroup.add(%vp);
}

function Minimap::enablePlayerIcon(%bool) {
  $Minimap::PlayerIcon = %bool;
}

function Minimap::clear() {
  for(%i = 0; %i < $Minimap::Items; %i++) {
    %uuid = $Minimap::UUID[%i];

    Minimap::_removeFromChunks(%i);

    $Minimap::ID[%uuid] = "";
    $Minimap::UUID[%i]  = "";

    $Minimap::Type[%i]  = "";
  }
  $Minimap::Items = 0;

  $Minimap::minChunkX = 0;
  $Minimap::minChunkY = 0;
  $Minimap::maxChunkX = 0;
  $Minimap::maxChunkY = 0;

  MinimapViewport.container.deleteAll();
}

function Minimap::clearUUID(%uuid) {
  if(strlen(%id = $Minimap::ID[%uuid]) != 0) {
    $Minimap::Type[%id] = "";
    return %id;
  } else {
    %id = $Minimap::Items+0;
    $Minimap::Items++;
    return %id;
  }
}
function Minimap::getChunkRange(%worldPos, %viewportExtent, %scale) {
  %worldChunk = vectorScale(%worldPos, 1/$Minimap::chunkSize);
  %radius = vectorScale(%viewportExtent, 1/(%scale*2*$Minimap::chunkSize));
  %min    = vectorFloor(vectorSub(%worldChunk, %radius));
  %max    = vectorCeil(vectorAdd(%worldChunk, %radius));
  return getWords(%min, 0, 1) SPC getWords(%max, 0, 1);
}

function Minimap::getObjectsInChunkRange(%worldPos, %viewportExtent, %scale, %forceUpdate) {
  %range = Minimap::getChunkRange(%worldPos, %viewportExtent, %scale);

  %xmin = getWord(%range, 0);
  %xmax = getWord(%range, 2);
  %ymin = getWord(%range, 1);
  %ymax = getWord(%range, 3);

  %renderList = "";
  for(%x = %xmin; %x < %xmax; %x++) {
    for(%y = %ymin; %y < %ymax; %y++) {

      %ct = $Minimap::chunkSize[%x, %y];
      for(%c = 0; %c < %ct; %c++) {
        %id = $Minimap::chunkId[%x, %y, %c];
        if($Minimap::NeedsUpdate[%id] || %forceUpdate)
          %renderList = %renderList SPC %id;
      }

    }
  }

  return %renderList;
}

function Minimap::getRenderObjectsInRange(%worldPos, %viewportExtent, %scale) {
  %range = Minimap::getChunkRange(%worldPos, %viewportExtent, %scale);

  %xmin = getWord(%range, 0);
  %xmax = getWord(%range, 2);
  %ymin = getWord(%range, 1);
  %ymax = getWord(%range, 3);

  %renderList = "";
  for(%x = %xmin; %x < %xmax; %x++) {
    for(%y = %ymin; %y < %ymax; %y++) {

      %ct = $Minimap::chunkSize[%x, %y];
      for(%c = 0; %c < %ct; %c++) {
        %id = $Minimap::chunkId[%x, %y, %c];
        %obj = $Minimap::GuiObj[%id];
        if(isObject(%obj))
          %renderList = %renderList SPC $Minimap::GuiObj[%id];
        else
          $Minimap::NeedsUpdate[%id] = true;
      }

    }
  }

  return %renderList;
}

function Minimap::getWorldCenterPoint() {
  //%centerPoint - the center to render about in world coordinates
  switch$($Minimap::follow) {
    case "player":
      %centerPoint = getWords(ServerConnection.getControlObject().getTransform(), 0, 2);

    case "snap":
      %centerPoint = getWords(ServerConnection.getControlObject().getTransform(), 0, 2);
      %x = mRound(getWord(%centerPoint, 0)/$Minimap::follow::snapsize) * $Minimap::follow::snapsize;
      %y = mRound(getWord(%centerPoint, 1)/$Minimap::follow::snapsize) * $Minimap::follow::snapsize;
      %centerPoint = %x SPC %y;

    case "fixed":
      %centerPoint = $Minimap::follow::position;
  }

  return %centerPoint;
}

function MinimapViewport::render(%this) {
  $mmrender_start = (getRealTime());

  %centerPoint = Minimap::getWorldCenterPoint();
  %renderCenter = vectorScale(%this.container.getGroup().extent, 0.5);

  //%this.container.position = mRound(getWord(%renderCenter, 0)+(($Minimap::minChunkX)*$Minimap::chunkSize-getWord(%centerPoint, 0))*%this.viewportScale) SPC mRound( getWord(%renderCenter, 1)+(($Minimap::minChunkY)*$Minimap::chunkSize+getWord(%centerPoint, 1))*%this.viewportScale);

  if($Minimap::forceUpdate) {
    %this.container.position = mRound(getWord(%renderCenter, 0)+(($Minimap::minChunkX)*$Minimap::chunkSize-getWord(%centerPoint, 0))*%this.viewportScale) SPC mRound( getWord(%renderCenter, 1)+(($Minimap::maxChunkY-$Minimap::minChunkY)*$Minimap::chunkSize+getWord(%centerPoint, 1))*%this.viewportScale);
    %this.container.extent = mRound(($Minimap::maxChunkX-$Minimap::minChunkX)*$Minimap::chunkSize*%this.viewportScale) SPC mRound(($Minimap::maxChunkY-$Minimap::minChunkY)*$Minimap::chunkSize*%this.viewportScale);
  }

  %containerOffset =  mRound((($Minimap::minChunkX)*$Minimap::chunkSize)) SPC mRound(((-$Minimap::maxChunkY)*$Minimap::chunkSize));

  if(%this.lastCenterpoint !$= %centerPoint || $Minimap::forceUpdate) {
    //%this.container.position = mRound(getWord(%renderCenter, 0)+(($Minimap::minChunkX)*$Minimap::chunkSize-getWord(%centerPoint, 0))*%this.viewportScale) SPC mRound( getWord(%renderCenter, 1)+(($Minimap::minChunkY)*$Minimap::chunkSize+getWord(%centerPoint, 1))*%this.viewportScale);
    %this.container.position = mRound(getWord(%renderCenter, 0)+(($Minimap::minChunkX)*$Minimap::chunkSize-getWord(%centerPoint, 0))*%this.viewportScale) SPC mRound( getWord(%renderCenter, 1)+(-($Minimap::maxChunkY)*$Minimap::chunkSize+getWord(%centerPoint, 1))*%this.viewportScale);
    %this.lastCenterpoint = %centerPoint;

    %vpMin = vectorSub(%centerPoint, vectorScale(%this.extent, 1/(%this.viewportScale*2)));
    %vpMax = vectorAdd(%centerPoint, vectorScale(%this.extent, 1/(%this.viewportScale*2)));
    %renderQueueCt = 0;

    %renderList = Minimap::getObjectsInChunkRange(%centerPoint, %this.extent, %this.viewportScale, $Minimap::forceUpdate);
    %guiObjects = Minimap::getRenderObjectsInRange(%centerPoint, %this.extent, %this.viewportScale);

    %guiCt = getWordCount(%guiObjects);
    for(%g = 0; %g < %guiCt; %g++) {
      %rendered[getWord(%guiObjects, %g)] = true;
    }

    if($Minimap::Verbose > 1) {
      echo("Render Prep: " @ getRealTime()-$mmrender_start);
      echo("Items:       " @ getWordCount(%renderList));
    }

    %renderCt = getWordCount(%renderList);

    for(%i = 0; %i < %renderCt; %i++) {
      %id = getWord(%renderList, %i);
      if(strlen($Minimap::Type[%id]) == 0)
        continue;

      %position = $Minimap::Position[%id];
      %extent   = $Minimap::Extent[%id];

      %min = %position;
      //%max = vectorAdd(%position, %extent);

      //%render = true;
      //if((getWord(%min, 0) > getWord(%vpMin, 0) && getWord(%min, 0) < getWord(%vpMax, 0)) ||
      //   (getWord(%max, 0) > getWord(%vpMin, 0) && getWord(%max, 0) < getWord(%vpMax, 0)) ||
      //   (getWord(%min, 0) < getWord(%vpMin, 0) && getWord(%max, 0) > getWord(%vpMax, 0)) ) {
        // x is in
      //  if((getWord(%min, 1) > getWord(%vpMin, 1) && getWord(%min, 1) < getWord(%vpMax, 1)) ||
      //     (getWord(%max, 1) > getWord(%vpMin, 1) && getWord(%max, 1) < getWord(%vpMax, 1)) ||
      //     (getWord(%min, 1) < getWord(%vpMin, 1) && getWord(%max, 1) > getWord(%vpMax, 1))) {
      //    // y is in
      //    %render = true;
      //  }
      //}

      //if(%render) {
      //  %renderQueue[%renderQueueCt] = %id;
      //  %renderQueueCt = 0;
      //} else {
      //  continue;
      //}

      %type = $Minimap::TypeId[%id];
      if(%type == 1) {
        if($Minimap::NeedsUpdate[%id] || $Minimap::forceUpdate) {
          %renderPosition = vectorScale(vectorSub(getWord(%position, 0) SPC -(getWord(%extent, 1)+getWord(%position, 1)), %containerOffset), %this.viewportScale);
          %renderPosition = mRound(getWord(%renderPosition, 0)) SPC mRound(getWord(%renderPosition, 1));

          %renderExtent = vectorScale(%extent, %this.viewportScale);
          %renderExtent = mRound(getWord(%renderExtent, 0)) SPC mRound(getWord(%renderExtent, 1));

          if(!isObject($Minimap::GuiObj[%id])) {
            %swatch = new GuiSwatchCtrl() {
              position = %renderPosition;
              extent = %renderExtent;
              color = $Minimap::Color[%id];
            };
            $Minimap::GuiObj[%id] = %swatch;
          } else {
            %swatch = $Minimap::GuiObj[%id];
            %swatch.position = %renderPosition;
            %swatch.extent = %renderExtent;
            %swatch.color = $Minimap::Color[%id];
          }
          %this.container.add(%swatch);
          %rendered[%swatch] = true;
          %updates++;
          $Minimap::NeedsUpdate[%id] = false;
        }
      } else if(%type == 2) {
        %centerDist = getWord(%position, 0) - getWord(%centerPoint, 0) SPC -getWord(%position, 1) - getWord(%centerPoint, 1);
        %renderOffset = (getWord(%centerDist, 0) * %this.viewportScale) SPC -((getWord(%centerDist, 1)) * %this.viewportScale);
        %renderOffset  = vectorSub(%renderOffset, vectorScale(%extent, 0.5));
        %renderPosition = vectorAdd(%renderCenter, %renderOffset);
        %renderPosition = mRound(getWord(%renderPosition, 0)) SPC mRound(getWord(%renderPosition, 1));

        if(!isObject($Minimap::GuiObj[%id])) {
          %text = new GuiTextCtrl() {
            profile = "GuiCenterTextProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = %renderPosition;
            extent = %extent;
            minExtent = %extent;
            enabled = "1";
            visible = "1";
            clipToParent = "1";
            text = $Minimap::Text[%id];
            maxLength = "255";
          };
          $Minimap::GuiObj[%id] = %text;
        } else {
          %text = $Minimap::GuiObj[%id];
          %text.position = %renderPosition;
        }
        %this.container.add(%text);
        %rendered[%text] = true;
      } else if(%type == 3) {
        %centerDist = vectorSub(%position, %centerPoint);
        %renderOffset = (getWord(%centerDist, 0) * %this.viewportScale) SPC -((getWord(%centerDist, 1)) * %this.viewportScale);
        %renderOffset  = vectorSub(%renderOffset, vectorScale(%extent, 0.5));
        %renderPosition = vectorAdd(%renderCenter, %renderOffset);
        %renderPosition = mRound(getWord(%renderPosition, 0)) SPC mRound(getWord(%renderPosition, 1));

        %icon = new GuiBitmapCtrl() {
          horizSizing = "center";
          vertSizing = "center";
          extent = "16 16";
          position = %renderPosition;
          bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ $Minimap::Icon[%id] @ ".png";
          visible = "1";
        };
        //%this.container.add(%icon);
        %rendered[%icon] = true;
      }
    }

    for(%i = 0; %i < %this.container.getCount(); %i++) {
      %obj = %this.container.getObject(%i);
      if(!%rendered[%obj]) {
        //%this.container.remove(%obj);
        //%obj.delete();
        //%i--;
        //%obj.visible = false;
      }
    }

  }

  if($Minimap::PlayerIcon) {
    // %renderCenter - the center of the viewport
    %renderCenter = vectorScale(%this.extent, 0.5);

    %transform = ServerConnection.getControlObject().getTransform();
    %z = getWord(%transform, 5);
    %w = getWord(%transform, 6);

    %rot = %z*%w;

    %quat = 2 * %rot/$pi;

    %quat += 0.5;
    if(%quat < 0)
      %quat += 4;

    switch(mfloor(%quat) % 4) {
      case 0:
        %dir = "up";

      case 1:
        %dir = "right";

      case 2:
        %dir = "down";

      case 3:
        %dir = "left";
    }

    if($Minimap::follow $= "fixed") {
      %position = vectorAdd(getWord(%transform, 0) SPC -getWord(%transform, 1), %renderCenter);
      %position = mRound(getWord(%position, 0))-8 SPC mRound(getWord(%position, 1))-8;
    } else if($Minimap::follow $= "player") {
      %position = mRound(getWord(%renderCenter, 0))-8 SPC mRound(getWord(%renderCenter, 1))-8;
    } else if($Minimap::follow $= "snap") {
      %relPosition = vectorSub(%transform, %centerPoint);
      %position = vectorAdd(getWord(%relPosition, 0) SPC -getWord(%relPosition, 1), %renderCenter);
      %position = mRound(getWord(%position, 0))-8 SPC mRound(getWord(%position, 1))-8;
    }

    if(!isObject($Minimap::Icon)) {
      %icon = new GuiBitmapCtrl() {
        horizSizing = "center";
        vertSizing = "center";
        extent = "16 16";
        position = %position;
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bullet_arrow_" @ %dir @ ".png";
        visible = "1";
      };

      $Minimap::Icon = %icon;
    } else {
      %icon = $Minimap::Icon;
      %icon.position = %position;
      %icon.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/bullet_arrow_" @ %dir @ ".png");
    }
    %this.add(%icon);
    %rendered[%icon] = true;
  }

  if($Minimap::isGrid) {

  }

  $Minimap::forceUpdate = false;

  if($Minimap::Verbose)
    echo("Render Time: " @ getRealTime()-$mmrender_start @ "ms, " @ %updates @ " updates");
}

//function MinimapViewport::onResize(%this, %x, %y, %h, %w) {
//  %this.render();
//}
