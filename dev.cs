function createPlayMinimap() {
  if(!isObject(MinimapViewport)) {
    Minimap::viewport();
  }

  PlayGui.add(MinimapViewport);

  MinimapViewport.resize(1000, 50, 250, 250);
  Minimap_render_loop();
}

function minimap::registerChunkedGrid(%xmin, %ymin, %xmax, %ymax, %ct) {
  if(!%ct)
    %ct = 1;

  for(%x = %xmin; %x < %xmax; %x++) {
    for(%y = %ymin; %y < %ymax; %y++) {
      %color = mFloor(255*(%x-%xmin)/(%xmax-%xmin)) SPC mFloor(255*(%y-%ymin)/(%ymax-%ymin)) SPC "0 255";
      for(%c = 0; %c < %ct; %c++)
      Minimap::createRectangle("chunkgrid_" @ %x @ "_" @ %y @ "_" @ %c,
                               %x*$Minimap::chunkSize+1,
                               %y*$Minimap::chunkSize+1,
                               $Minimap::chunkSize-2,
                               $Minimap::chunkSize-2,
                               %color);
    }
  }
}
