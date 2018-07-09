function Minimap::createGrid(%spacing_x, %spacing_y, %offset_x, %offset_y, %color) {
  $Minimap::isGrid = true;
  $Minimap::Grid::Spacing = %spacing_x SPC %spacing_y;
  $Minimap::Grid::Offset  = %offset_x SPC %offset_y;
}

function Minimap::createRectangle(%uuid, %x, %y, %h, %w, %color) {
  %id = Minimap::clearUUID(%uuid);

  $Minimap::ID[%uuid]   = %id;
  $Minimap::UUID[%id]   = %uuid;

  $Minimap::Type[%id]     = "rect";
  $Minimap::TypeId[%id]   = 1;
  $Minimap::Position[%id] = %x SPC %y;
  $Minimap::Extent[%id]   = %h SPC %w;
  $Minimap::Color[%id]    = %color;

  $Minimap::NeedsUpdate[%id] = true;

  Minimap::_addToChunks(%id);
}

function Minimap::createText(%uuid, %x, %y, %text, %color) {
  %id = Minimap::clearUUID(%uuid);

  $Minimap::UUID[%uuid]   = %id;

  $Minimap::Type[%id]     = "text";
  $Minimap::TypeId[%id]   = 2;
  $Minimap::Position[%id] = %x SPC %y;
  $Minimap::Extent[%id]   = "100 16";
  $Minimap::Text[%id]    = %text;

  Minimap::_addToChunks(%id);
}

function Minimap::createIcon(%uuid, %x, %y, %icon, %waypoint) {
  %id = Minimap::clearUUID(%uuid);

  $Minimap::UUID[%uuid]   = %id;

  $Minimap::Type[%id]     = "icon";
  $Minimap::TypeId[%id]   = 3;
  $Minimap::Position[%id] = %x SPC %y;
  $Minimap::Extent[%id]   = "16 16";
  $Minimap::Icon[%id]     = %icon;
  $Minimap::Waypoint[%id] = %waypoint;

  Minimap::_addToChunks(%id);


}

function Minimap::createImage(%uuid, %x, %y, %imageId, %color) {
  %id = Minimap::clearUUID(%uuid);

  $Minimap::UUID[%uuid]   = %id;

  $Minimap::Type[%id]     = "icon";
  $Minimap::TypeId[%id]   = 4;
  $Minimap::Position[%id] = %x SPC %y;
  $Minimap::Extent[%id]     = %h SPC %w;
  $Minimap::Color[%id]    = %color;

  Minimap::_addToChunks(%id);
}
