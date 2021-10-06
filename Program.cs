using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace map_reader
{
    class Program
    {
        static void Main(string[] args)
        {
            //I don't know how to do it is part better yet

            string path;

            Console.WriteLine("Enter map folder (or .bmp) name:");
            string mapName = Console.ReadLine();

            if (Directory.Exists(@$"maps\{mapName}"))
            {
                path = Directory.GetCurrentDirectory() + @$"\maps\{mapName}";
            }
            else
            {
                Console.WriteLine("Enter absolute path (without name):");
                path = Console.ReadLine() + @$"\{mapName}";
            }

            Console.WriteLine("1 - generate .bmp\n2 - generate .map");
            int key = Int32.Parse(Console.ReadLine());

            switch (key)
            {
                case 1:
                    MapToImage(path);
                    MapToImage(path, "BiomeMap");
                    MapToImage(path, "HeightMap");
                    break;
                case 2:
                    ImageToMap(path);
                    break;
                default:
                    Console.WriteLine("Exit");
                    break;
            }

        }
        public static void MapToImage(string path, string key = "NormalMap")
        {
            int x = 0;
            int y = 0;

            GetXandY(path, ref x, ref y);

            //two Dictionaries:
            //key-tile pair - generate from header
            //tile-color pair - matching biomes with color

            Dictionary<int, string> keyTilePair = GetKeyTilePair(path);
            Dictionary<string, Color> tileColorPair = new Dictionary<string, Color>
            {
                ["TileForestTropical"] =    Color.FromArgb(0xB8, 0xC3, 0x85),
                ["TileWaterSea"] =          Color.FromArgb(0x43, 0x95, 0xEA),
                ["TileBarren"] =            Color.FromArgb(0xFF, 0xBA, 0x66),
                ["TileSaltFlats"] =         Color.FromArgb(0xA5, 0x8F, 0x73),
                ["TileLakeShore"] =         Color.FromArgb(0xDE, 0xB9, 0x74),
                ["TileRuins"] =             Color.FromArgb(0xAE, 0xA6, 0x9B),
                ["TileWaterLake"] =         Color.FromArgb(0x84, 0xD7, 0xE7),
                ["TileBeachBoreal"] =       Color.FromArgb(0xEE, 0xD5, 0x97),
                ["TileForestBoreal"] =      Color.FromArgb(0x8C, 0xB2, 0x7A),
                ["TileRoads"] =             Color.FromArgb(0x65, 0x77, 0x7C),
                ["TileRocky"] =             Color.FromArgb(0xC8, 0xA8, 0x82),
                ["TileVolcanic"] =          Color.FromArgb(0x8C, 0x80, 0x73),
                ["TileClay"] =              Color.FromArgb(0xE2, 0x88, 0x5C),
                ["TileWaterSeaBridge"] =    Color.FromArgb(0x65, 0x7C, 0x7C),
                ["TileLava"] =              Color.FromArgb(0xC0, 0x00, 0x00),
                ["TileAlien"] =             Color.FromArgb(0x53, 0x5C, 0x6E),
                ["TileMeadows"] =           Color.FromArgb(0xA1, 0xBB, 0x44),
                ["TileBeachTemperate"] =    Color.FromArgb(0xEE, 0xD5, 0x97),
                ["TileForestTemperate"] =   Color.FromArgb(0xC5, 0xD6, 0x8F),
                ["TileSwamp"] =             Color.FromArgb(0x94, 0x9F, 0x57),
                ["TileWaterLakeBridge"] =   Color.FromArgb(0x65, 0x78, 0x7C),
                ["Cliff"] =                 Color.FromArgb(0x8A, 0x62, 0x32)
            };

            string pathToDataIndex = $@"{path}\ChunkDataIndex.data";
            FileStream chunkDataIndex = new FileStream(pathToDataIndex, FileMode.Open);

            List<int> offsetX = new List<int>(); //{ 0, 0, 0, 0, 10, 10, 10, 10, 20, 20, 20, 20 };
            List<int> offsetY = new List<int>(); //{ 0, 10, 20, 30, 0, 10, 20, 30, 0, 10, 20, 30 };

            GenerateOffset(x, y, ref offsetX, ref offsetY);

            Color[,] mapMatrix = new Color[x, y];

            for (int v = 0; v < x * y / 100; v++)
            {
                string pathToChunk = @$"{path}\Chunks\{GetChunkHash(chunkDataIndex)}";
                FileStream chunk = new FileStream(pathToChunk, FileMode.Open);

                byte[] chunkArr = new byte[200];
                chunk.Read(chunkArr);

                chunk.Dispose();

                int byteOffset = 0;

                for (int i = 0 + offsetX[v]; i < 10 + offsetX[v]; i++)
                {
                    for (int j = 0 + offsetY[v]; j < 10 + offsetY[v]; j++)
                    {
                        switch (key)
                        {
                            case "NormalMap": //map image like in game
                                mapMatrix[i, j] = (chunkArr[byteOffset + 1] & (0x80 | 0x40)) == (0x40) ? tileColorPair["Cliff"] : tileColorPair[keyTilePair[chunkArr[byteOffset]]];
                                break;
                            case "BiomeMap": //biome map 
                                mapMatrix[i, j] = tileColorPair[keyTilePair[chunkArr[byteOffset]]];
                                break;
                            case "HeightMap": //height map
                                var temp = (chunkArr[byteOffset + 1] & ~(0x80 | 0x40)) * 4;
                                mapMatrix[i, j] = Color.FromArgb(temp, temp, temp);
                                break;
                        }
                        byteOffset += 2;
                    }
                }
            }

            chunkDataIndex.Dispose();

            Bitmap bmp = new Bitmap(x, y);

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    bmp.SetPixel(i, y - j - 1, mapMatrix[i, j]);
                }
            }

            bmp.Save(@$"{path}{key}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //bmp.Save(@$"{path}{key}.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void ImageToMap(string path)
        {
            Bitmap mapBiome = new Bitmap($@"{path}BiomeMap.bmp");
            Bitmap mapHeight = new Bitmap($@"{path}HeightMap.bmp");

            int x = mapBiome.Width;
            int y = mapBiome.Height;

            byte[,] cliffMatrix = new byte[x, y];

            for (int i = 1; i < x - 1; i++)
            {
                for (int j = 1; j < y - 1; j++)
                {
                    if (mapHeight.GetPixel(i - 1, j).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i + 1, j).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i, j - 1).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i, j + 1).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i - 1, j - 1).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i + 1, j + 1).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i + 1, j - 1).R == mapHeight.GetPixel(i, j).R + 4 ||
                        mapHeight.GetPixel(i - 1, j + 1).R == mapHeight.GetPixel(i, j).R + 4)

                    {
                        cliffMatrix[i, j] = 0x40;
                    }
                }
            }

            Random random = new Random();

            /*for (int j = 0; j < y; j++)
            {
                for (int i = 1; i < x - 2; i ++)
                {

                    if (cliffMatrix[i - 1, j] == 0x40 &&
                        cliffMatrix[i, j] == 0x40 &&
                        cliffMatrix[i + 1, j] == 0x40 &&
                        cliffMatrix[i + 2, j] == 0x40 &&

                        cliffMatrix[i - 2, j] != 0x80 &&
                        cliffMatrix[i + 3, j] != 0x80)
                    {
                        cliffMatrix[i, j] = 0x80;
                        cliffMatrix[i + 1, j] = 0x80;
                    }
                }
            }*/

            List<int> offsetX = new List<int>();//{ 0, 0, 0, 0, 10, 10, 10, 10, 20, 20, 20, 20 };
            List<int> offsetY = new List<int>();//{ 0, 10, 20, 30, 0, 10, 20, 30, 0, 10, 20, 30 };

            GenerateOffset(x, y, ref offsetX, ref offsetY);

            Dictionary<Color, string> ColorTilePair = new Dictionary<Color, string>
            {
                [Color.FromArgb(0xB8, 0xC3, 0x85)] = "TileForestTropical",
                [Color.FromArgb(0x43, 0x95, 0xEA)] = "TileWaterSea",
                [Color.FromArgb(0xFF, 0xBA, 0x66)] = "TileBarren",
                [Color.FromArgb(0xA5, 0x8F, 0x73)] = "TileSaltFlats",
                [Color.FromArgb(0xDE, 0xB9, 0x74)] = "TileLakeShore",
                [Color.FromArgb(0xAE, 0xA6, 0x9B)] = "TileRuins",
                [Color.FromArgb(0x84, 0xD7, 0xE7)] = "TileWaterLake",
                [Color.FromArgb(0xEE, 0xD5, 0x97)] = "TileBeachBoreal",
                [Color.FromArgb(0x8C, 0xB2, 0x7A)] = "TileForestBoreal",
                [Color.FromArgb(0x65, 0x77, 0x7C)] = "TileRoads",
                [Color.FromArgb(0xC8, 0xA8, 0x82)] = "TileRocky",
                [Color.FromArgb(0x8C, 0x80, 0x73)] = "TileVolcanic",
                [Color.FromArgb(0xE2, 0x88, 0x5C)] = "TileClay",
                [Color.FromArgb(0x65, 0x7C, 0x7C)] = "TileWaterSeaBridge",
                [Color.FromArgb(0xC0, 0x00, 0x00)] = "TileLava",
                [Color.FromArgb(0x53, 0x5C, 0x6E)] = "TileAlien",
                [Color.FromArgb(0xA1, 0xBB, 0x44)] = "TileMeadows",
                [Color.FromArgb(0xEE, 0xD5, 0x97)] = "TileBeachTemperate",
                [Color.FromArgb(0xC5, 0xD6, 0x8F)] = "TileForestTemperate",
                [Color.FromArgb(0x94, 0x9F, 0x57)] = "TileSwamp",
                [Color.FromArgb(0x65, 0x78, 0x7C)] = "TileWaterLakeBridge"
                //[Color.FromArgb(0x8A, 0x62, 0x32)] = "Cliff"
            };

            Dictionary<string, int> TileKeyPair = new Dictionary<string, int>
            {
                ["TileForestTropical"] = 0,
                ["TileWaterSea"] = 1,
                ["TileBarren"] = 2,
                ["TileSaltFlats"] = 3,
                ["TileLakeShore"] = 4,
                ["TileRuins"] = 5,
                ["TileWaterLake"] = 6,
                ["TileBeachBoreal"] = 7,
                ["TileForestBoreal"] = 8,
                ["TileRoads"] = 9,
                ["TileRocky"] = 10,
                ["TileVolcanic"] = 11,
                ["TileClay"] = 12,
                ["TileWaterSeaBridge"] = 13,
                ["TileLava"] = 14,
                ["TileAlien"] = 15,
                ["TileMeadows"] = 16,
                ["TileBeachTemperate"] = 17,
                ["TileForestTemperate"] = 18,
                ["TileSwamp"] = 19,
                ["TileWaterLakeBridge"] = 20
                //["Cliff"] = 21
            };

            Directory.CreateDirectory(@$"{path}Gen");
            Directory.CreateDirectory(@$"{path}Gen/Chunks");

            FileStream chunkDataIndex = new FileStream(@$"{path}Gen/ChunkDataIndex.data", FileMode.OpenOrCreate);

            byte[] chunkArr = new byte[200];

            for (int v = 0; v < x * y / 100; v++)
            {
                int byteOffset = 0;
                string chunkString = "";

                for (int i = 0 + offsetX[v]; i < 10 + offsetX[v]; i++)
                {
                    for (int j = y - 1 - offsetY[v]; j >= y - 10 - offsetY[v]; j--)
                    {
                        chunkArr[byteOffset] = Convert.ToByte(TileKeyPair[ColorTilePair[mapBiome.GetPixel(i, j)]]);
                        //chunkArr[byteOffset + 1] = Convert.ToByte((mapHeight.GetPixel(i, j).R / 4) | cliffMatrix[i, j]);
                        chunkArr[byteOffset + 1] = Convert.ToByte((mapHeight.GetPixel(i, j).R / 4));
                        chunkString += $"{chunkArr[byteOffset]}{chunkArr[byteOffset + 1]}";
                        byteOffset += 2;
                    }
                }

                uint chunkHash = (uint)chunkString.GetHashCode();

                foreach (byte a in BitConverter.GetBytes(chunkHash))
                {
                    chunkDataIndex.WriteByte(a);
                }

                if (!File.Exists(@$"{path}Gen/Chunks/{chunkHash}"))
                {
                    File.WriteAllBytes(@$"{path}Gen/Chunks/{chunkHash}", chunkArr);
                }
            }
            chunkDataIndex.Close();

            StreamWriter Header = new StreamWriter(@$"{path}Gen/Header.yaml");
            Header.WriteLine("SerializerVersion: 1");
            Header.WriteLine("WorldChunkSize: 10");
            Header.WriteLine("WorldOffset:");
            Header.WriteLine("  X: 10000");
            Header.WriteLine("  Y: 10000");
            Header.WriteLine("WorldSize:");
            Header.WriteLine($"  X: {x}");
            Header.WriteLine($"  Y: {y}");
            Header.WriteLine("ProtoTileBinding:");
            foreach (var pair in TileKeyPair)
            {
                Header.WriteLine($"  {pair.Value}: AtomicTorch.CBND.CoreMod.Tiles.{pair.Key}");
            }
            Header.Close();
        }

        public static string GetChunkHash(FileStream chunkDataIndex)
        {
            byte[] tempArr = new byte[4];
            const int count = 4;

            chunkDataIndex.Read(tempArr, 0, count);
            uint hash = BitConverter.ToUInt32(tempArr);

            return Convert.ToString(hash);
        }
        public static Dictionary<int, string> GetKeyTilePair(string path)
        {
            Dictionary<int, string> keyTilePair = new Dictionary<int, string>();
            string pathToHeader = @$"{path}\Header.yaml";
            string[] lines = File.ReadAllLines(pathToHeader);

            bool itsReadable = false;

            foreach (var line in lines)
            {
                if (line[0] != ' ' && itsReadable == true)
                {
                    itsReadable = false;
                    break;
                }

                if (itsReadable)
                {
                    var temp = line.Trim().Split(": AtomicTorch.CBND.CoreMod.Tiles.");
                    keyTilePair.Add(Int32.Parse(temp[0]), temp[1]);
                }

                if (line.Trim() == "ProtoTileBinding:")
                {
                    itsReadable = true;
                }
            }
            return keyTilePair;
        }
        public static void GetXandY(string path, ref int x, ref int y)
        {
            string pathToHeader = @$"{path}\Header.yaml";
            string[] lines = File.ReadAllLines(pathToHeader);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "WorldSize:")
                {
                    x = Int32.Parse(lines[i + 1].Trim(' ', ':', 'X', 'Y'));
                    y = Int32.Parse(lines[i + 2].Trim(' ', ':', 'X', 'Y'));
                }
            }
        }
        public static void GenerateOffset(int x, int y, ref List<int> offsetX, ref List<int> offsetY)
        {
            for (int i = 0; i < x; i += 10)
            {
                for (int j = 0; j < y / 10; j++)
                {
                    offsetX.Add(i);
                }
            }

            for (int i = 0; i < x / 10; i++)
            {
                for (int j = 0; j < y; j += 10)
                {
                    offsetY.Add(j);
                }
            }
        }
    }
}
