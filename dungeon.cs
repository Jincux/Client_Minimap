// dungeon minimap

function colorIdToRGB(%id) {
  %color  = getColorIDTable(%id);

  if(strpos(%color, ".") >= 0) {
    %color = vectorScale(%color, 255) SPC (getWord(%color, 3)*255);
  }

  return mFloor(getWord(%color, 0)) SPC mFloor(getWord(%color, 1)) SPC mFloor(getWord(%color, 2)) SPC mFloor(getWord(%color, 3));
}

package MinimapDungeon {
  function serverCmdStartDungeon(%cl) {
    if(%cl.isAdmin) {
      commandToAll('MinimapClear');
    }

    return parent::serverCmdStartDungeon(%cl);
  }

  function createDungeon(%scriptObject, %ignoreDoor, %saferoom, %iteration) {
    %color_floor = colorIdToRGB($Level[%scriptObject.level @ "FloorColor"]);
    %color_wall  = colorIdToRGB($Level[%scriptObject.level @ "WallColor"]);
    %color_door  = colorIdToRGB($Level[%scriptObject.level @ "DoorColor"]);

    %min = (16*%scriptObject.x)-8 SPC (16*%scriptObject.y)-8;

    %corner[0] = !%scriptObject.merge[1] || !%scriptObject.merge[2];
    %corner[1] = !%scriptObject.merge[2] || !%scriptObject.merge[3];
    %corner[2] = !%scriptObject.merge[3] || !%scriptObject.merge[4];
    %corner[3] = !%scriptObject.merge[4] || !%scriptObject.merge[1];

    for(%c = 0; %c < ClientGroup.getCount(); %c++) {
      %client = ClientGroup.getObject(%c);
      commandToClient(%client, 'MinimapRectangle', %scriptObject @ "_floor", // uuid
                                                   getWord(%min, 0), getWord(%min, 1), // position
                                                   16, 16, //extent
                                                   %color_floor); // color

      for(%i = 0; %i < 4; %i++) {
        // walls
        if(!%scriptObject.merge[%i+1]) {
          %width = 14;
          %offset = 1;

          if(%corner[(%i-1) % 4]) {
            %width++;
            %offset--;
          }

          if(%corner[%i]) {
            %width++;
          }

          switch(%i) {
            case 2: // North, so 1 on y
              %position = (getWord(%min, 0)+%offset) SPC getWord(%min, 1);
              %wextent   = %width SPC 1;

            case 1:
              %position = (getWord(%min, 0)+15) SPC getWord(%min, 1)+%offset;
              %wextent   = 1 SPC %width;

            case 0:
              %position = (getWord(%min, 0)+%offset) SPC getWord(%min, 1)+15;
              %wextent   = %width SPC 1;

            case 3:
              %position = getWord(%min, 0) SPC getWord(%min, 1)+%offset;
              %wextent   = 1 SPC %width;
          }

          %c[0] = "255 0 0 255";
          %c[1] = "0 255 0 255";
          %c[2] = "0 0 255 255";
          %c[3] = "0 255 255 255";

          commandToClient(%client, 'MinimapRectangle', %scriptObject @ "_wall" @ %i, // uuid
                                                       getWord(%position, 0), getWord(%position, 1), // position
                                                       getWord(%wextent, 0), getWord(%wextent, 1), //extent
                                                       %color_wall); // color
        }

        // doors
        if(%scriptObject.door[%i+1] && !%scriptObject.merge[%i+1]) {
          %width = 3;
          %offset = 6.5;

          switch(%i) {
            case 2: // North, so 1 on y
              %position = (getWord(%min, 0)+%offset) SPC getWord(%min, 1);
              %wextent   = %width SPC 1;

            case 1:
              %position = (getWord(%min, 0)+15) SPC getWord(%min, 1)+%offset;
              %wextent   = 1 SPC %width;

            case 0:
              %position = (getWord(%min, 0)+%offset) SPC getWord(%min, 1)+15;
              %wextent   = %width SPC 1;

            case 3:
              %position = getWord(%min, 0) SPC getWord(%min, 1)+%offset;
              %wextent   = 1 SPC %width;
          }

          %c[0] = "255 0 0 255";
          %c[1] = "0 255 0 255";
          %c[2] = "0 0 255 255";
          %c[3] = "0 255 255 255";

          commandToClient(%client, 'MinimapRectangle', %scriptObject @ "_door" @ %i, // uuid
                                                       getWord(%position, 0), getWord(%position, 1), // position
                                                       getWord(%wextent, 0), getWord(%wextent, 1), //extent
                                                       %color_door); // color
        }
      }



    }

    return parent::createDungeon(%scriptObject, %ignoreDoor, %saferoom, %iteration);
  }
};
activatePackage(MinimapDungeon);
