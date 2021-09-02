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
            string path;
            //string mapName = "Cryofall";

            Console.WriteLine("Enter map folder name:");
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
            //string path = @$"C:\Users\AnDrew0o\source\repos\map reader\maps\{mapName}";

            MapToImage(path);
            MapToImage(path, 1);
            MapToImage(path, 2);
        }
        public static void MapToImage(string path, int key = 0)
        {
            int x = 0;
            int y = 0;

            GetXandY(path, ref x, ref y);

            Dictionary<int, string> keyTilePair = GetKeyTilePair(path);
            Dictionary<string, Color> tileColorPair = new Dictionary<string, Color>
            {
                ["TileForestTropical"] = Color.FromArgb(0xB8, 0xC3, 0x85),
                ["TileWaterSea"] = Color.FromArgb(0x43, 0x95, 0xEA),
                ["TileBarren"] = Color.FromArgb(0xFF, 0xBA, 0x66),
                ["TileSaltFlats"] = Color.FromArgb(0xA5, 0x8F, 0x73),
                ["TileLakeShore"] = Color.FromArgb(0xDE, 0xB9, 0x74),
                ["TileRuins"] = Color.FromArgb(0xAE, 0xA6, 0x9B),
                ["TileWaterLake"] = Color.FromArgb(0x84, 0xD7, 0xE7),
                ["TileBeachBoreal"] = Color.FromArgb(0xEE, 0xD5, 0x97),
                ["TileForestBoreal"] = Color.FromArgb(0x8C, 0xB2, 0x7A),
                ["TileRoads"] = Color.FromArgb(0x65, 0x77, 0x7C),
                ["TileRocky"] = Color.FromArgb(0xC8, 0xA8, 0x82),
                ["TileVolcanic"] = Color.FromArgb(0x8C, 0x80, 0x73),
                ["TileClay"] = Color.FromArgb(0xE2, 0x88, 0x5C),
                ["TileWaterSeaBridge"] = Color.FromArgb(0x65, 0x77, 0x7C),
                ["TileLava"] = Color.FromArgb(0xC0, 0x00, 0x00),
                ["TileAlien"] = Color.FromArgb(0x53, 0x5C, 0x6E),
                ["TileMeadows"] = Color.FromArgb(0xA1, 0xBB, 0x44),
                ["TileBeachTemperate"] = Color.FromArgb(0xEE, 0xD5, 0x97),
                ["TileForestTemperate"] = Color.FromArgb(0xC5, 0xD6, 0x8F),
                ["TileSwamp"] = Color.FromArgb(0x94, 0x9F, 0x57),
                ["TileWaterLakeBridge"] = Color.FromArgb(0x65, 0x77, 0x7C),
                ["Cliff"] = Color.FromArgb(0x8A, 0x62, 0x32)
            };

            string pathToDataIndex = $@"{path}\ChunkDataIndex.data";
            FileStream chunkDataIndex = new FileStream(pathToDataIndex, FileMode.Open);

            List<int> offsetX = new List<int>();//{ 0, 0, 0, 0, 10, 10, 10, 10, 20, 20, 20, 20 };
            List<int> offsetY = new List<int>();//{ 0, 10, 20, 30, 0, 10, 20, 30, 0, 10, 20, 30 };

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
                            case 0:
                                mapMatrix[i, j] = (chunkArr[byteOffset + 1] & (0x80 | 0x40)) == (0x40) ? tileColorPair["Cliff"] : tileColorPair[keyTilePair[chunkArr[byteOffset]]];
                                break;
                            case 1:
                                mapMatrix[i, j] = tileColorPair[keyTilePair[chunkArr[byteOffset]]];
                                break;
                            case 2:
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
