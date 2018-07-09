function clientCmdMinimapClear() {
  Minimap::clear();
}

function clientCmdMinimapShow(%color) {

}

function clientCmdMinimapScale(%scale) {

}

function clientCmdMinimapRectangle(%uuid, %x, %y, %h, %w, %color) {
  Minimap::createRectangle(%uuid, %x, %y, %h, %w, %color);
}

function clientCmdMinimapSetLevel(%uuid, %level) {
  %id = $Minimap::Id[%uuid];
  $Minimap::Level[%id] = %level;
}

function clientCmdMinimapShowLevel(%level) {

}
