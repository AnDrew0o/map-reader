# map-reader
À program for convert a Cryofall .map file into a picture (.bmp)
or convert two .bmp (one - biome map, another - height map, you need both it) to .map

1. To create a picture from a map, you must to unzip map.

To create a map from the pictures, you must name it NameBiomeMap.bmp(for biome map) and Name2.bmp(for height map) 
You can set "Name" by yourself. 

2. You can't use any images. 
For Biome map you must use only this colors:
	TileForestTropical -    #B8C385
	TileWaterSea -          #4395EA
	TileBarren -            #FFBA66
	TileSaltFlats -         #A58F73
	TileLakeShore -         #DEB974
	TileRuins -             #AEA69B
	TileWaterLake -         #84D7E7
	TileBeachBoreal -       #EED597
	TileForestBoreal -      #8CB27A
	TileRocky -             #C8A882
	TileVolcanic -          #8C8073
	TileClay -              #E2885C
	TileLava -              #C00000
	TileAlien -             #535C6E
	TileMeadows -           #A1BB44
	TileBeachTemperate -    #EED597
	TileForestTemperate -   #C5D68F
	TileSwamp -             #949F57
	TileRoads -             #65777C
	TileWaterLakeBridge -   #65787C
	TileWaterSeaBridge -    #657C7C

For Height map it's a bit more difficult
	The generating of the height map depends on the value of the height byte, and since heights can only be from 1 to 63 (but not all are used),
	I multiply the value of the byte by 4 (so that the colors are more different, and the value was not more than 255), and fill all 3 channels (RGB) with this value.
	so you can use colors like (4, 4, 4), (64, 64, 64) or (252, 252, 252)
	In general, 1 < n < 63. your rgb color is (4 * n, 4 * n, 4 * n)
	The difference of nearby heights should not be more than 1.
	For example nearby colors should be (40, 40, 40) and (44, 44, 44).
	If you don't do it, the Cryofall editor will simply add intermediate heights, but it will not be what you expect.

	I will change it in the future, probably.
	And I want to make a 2D generator.

3. À little bit about drawing a map in a graphical editor
	Drawing a height map can be difficult, but you can first draw biomes,
	and then in a new layer draw the heights attached to the biomes (you can do this with many things, but not with all)

4. CryoFall map data specification (*.map) by ai_enabled:

	The whole world is split into chunks of 10x10 tiles. Each chunk contains 
	an array of tiles. Each tile is just two bytes: first is the tile prototype 
	index (for the lookup table ProtoTileBinding in Header.yaml file),second is 
	the height index and cliff/slope flag (more on this below).
	Chunk filename is just a hashcode (32-bit unsigned integer). 
	To determine location for each chunk an index file is used (ChunkDataIndex.data). 
	It contains an array of chunk hashcodes (32-bit unsigned integer). 
	So its size is the world size multiplied by 4 bytes. 
	World size and origin offset are encoded in Header.yaml as well as plenty of other data.

	Format for tile height index—it's a single byte so there are 8 bits:
	— 2^6 (mask 0x40) determines whether it's a cliff
	— 2^7 (mask 0x80) determines whether it's a slope
	— this leaves us with 6 bytes to represent the height level (so there are 2^6 - 1 = 63 height levels).

