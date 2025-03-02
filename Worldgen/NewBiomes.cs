using Microsoft.Xna.Framework;
using Remnants;
using Remnants.Tiles;
using Remnants.Tiles.Blocks;
using Remnants.Walls.Vanity;
using Remnants.Worldgen;
using StructureHelper;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Remnants.Worldgen.NewPrimaryBiomes;

namespace Remnants.Worldgen
{
    #region BioMap
    public class BiomeID
    {
        public const int None = 0;

        public const int Tundra = 1;

        public const int Jungle = 2;

        public const int Desert = 3;

        public const int Underworld = 4;

        public const int Corruption = 5;

        public const int Crimson = 6;

        public const int Clouds = 7;

        public const int Glowshroom = 8;

        public const int Marble = 9;

        public const int Granite = 10;

        public const int Hive = 11;

        public const int GemCave = 12;

        public const int Beach = 13;

        public const int OceanCave = 14;

        public const int Aether = 15;

        public const int AshForest = 16;

        public const int Obsidian = 17;

        public const int Toxic = 18;

        public const int Abysm = 101;
    }
    public class BiomeMap : ModSystem
    {
        public int[,] biomeMap;

        public int scale => 50;
        public int width => Main.maxTilesX / scale;
        public int height => Main.maxTilesY / scale;

        public FastNoiseLite blendingNoise = new FastNoiseLite();

        private float[,] blendingX;
        private float[,] blendingY;
        private int blendDistance => ModContent.GetInstance<Client>().ExperimentalWorldgen ? 0 : 40;

        public FastNoiseLite materialsNoise = new FastNoiseLite();

        public float[,] materials;

        internal class NewBiomeMapSetup : GenPass
        {
            public NewBiomeMapSetup(string name, float loadWeight) : base(name, loadWeight)
            {
            }
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Setting up biome map";

                BiomeMap biomes = ModContent.GetInstance<BiomeMap>();

                biomes.biomeMap = new int[biomes.width, biomes.height];

                biomes.blendingNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                biomes.blendingNoise.SetSeed(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
                biomes.blendingNoise.SetFrequency(0.0125f);
                biomes.blendingNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
                //biomes.blending.SetFractalOctaves(3);
                biomes.blendingNoise.SetFractalLacunarity(2.25f);

                biomes.blendingX = new float[Main.maxTilesX, Main.maxTilesY];
                biomes.blendingY = new float[Main.maxTilesX, Main.maxTilesY];

                biomes.materialsNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                biomes.materialsNoise.SetSeed(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
                biomes.materialsNoise.SetFrequency(0.025f);
                biomes.materialsNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
                //biomes.materialsNoise.SetFractalLacunarity(2.25f);

                biomes.materials = new float[Main.maxTilesX, Main.maxTilesY];

                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    progress.Set((float)y / Main.maxTilesY);

                    for (int x = 0; x < Main.maxTilesX; x++)
                    {
                        biomes.blendingX[x, y] = biomes.blendingNoise.GetNoise(x, y * 2 + 999);
                        biomes.blendingY[x, y] = biomes.blendingNoise.GetNoise(x + 999, y * 2);

                        biomes.materials[x, y] = biomes.materialsNoise.GetNoise(x, y * 2);
                    }
                }

                //for (int y = 0; y <= (int)(Main.worldSurface * 0.4 / scale); y++)
                //{
                //    for (int x = 0; x < width; x++)
                //    {
                //        AddBiome(x, y, "heaven");
                //    }
                //}
                //for (int y = 0; y < biomes.height - 4; y++)
                //{
                //    for (int x = 0; x <= 6; x++)
                //    {
                //        biomes.AddBiome(x, y, BiomeID.Beach);
                //    }
                //    for (int x = biomes.width - 7; x < biomes.width; x++)
                //    {
                //        biomes.AddBiome(x, y, BiomeID.Beach);
                //    }
                //}

                if (GenVars.dungeonSide == 1)
                {
                    Tundra.X = (int)(biomes.width * (ModContent.GetInstance<Client>().ExperimentalWorldgen ? 0.7f : 0.65f));// WorldGen.genRand.Next((int)(biomes.width * 0.65), (int)(biomes.width * 0.7));
                }
                else
                {
                    Tundra.X = (int)(biomes.width * (ModContent.GetInstance<Client>().ExperimentalWorldgen ? 0.3f : 0.35f));//WorldGen.genRand.Next((int)(biomes.width * 0.3), (int)(biomes.width * 0.35));
                }

                float tundraCorruptionDistance = 0.225f;
                bool tundraCorruptionSwap = WorldGen.genRand.NextBool(2); //GenVars.dungeonSide == 1;

                Tundra.X = biomes.width / 2;
                Tundra.X += (int)(biomes.width * tundraCorruptionDistance * (GenVars.dungeonSide != 1 ? -1 : 1));
                Tundra.X += (int)(biomes.width * 0.05f * (!tundraCorruptionSwap ? -1 : 1));

                Corruption.X = biomes.width / 2;
                Corruption.X += (int)(biomes.width * tundraCorruptionDistance * (Tundra.X < biomes.width / 2 ? -1 : 1));
                Corruption.X += (int)(biomes.width * 0.075f * (tundraCorruptionSwap ? -1 : 1));

                float jungleDesertDistance = 0.25f;// WorldGen.genRand.NextFloat(0.275f, 0.325f);
                bool jungleDesertSwap = WorldGen.genRand.NextBool(2); //GenVars.dungeonSide == 1;

                Jungle.Center = biomes.width / 2;
                Jungle.Center += (int)(biomes.width * jungleDesertDistance * (GenVars.dungeonSide == 1 ? -1 : 1));
                Jungle.Center += (int)(biomes.width * 0.05f * (!jungleDesertSwap ? -1 : 1));

                Desert.X = biomes.width / 2;
                Desert.X += (int)(biomes.width * jungleDesertDistance * (Jungle.Center < biomes.width / 2 ? -1 : 1));
                Desert.X += (int)(biomes.width * 0.1f * (jungleDesertSwap ? -1 : 1));

                for (int y = biomes.height - 4; y < biomes.height; y++)
                {
                    for (int x = 0; x < biomes.width; x++)
                    {
                        biomes.AddBiome(x, y, BiomeID.Underworld);
                    }
                }
                for (int y = biomes.height - 6; y < biomes.height - 4; y++)
                {
                    for (int x = 0; x < biomes.width; x++)
                    {
                        biomes.AddBiome(x, y, BiomeID.Obsidian);
                    }
                }

                for (int y = 0; y < biomes.height - 6; y++)
                {
                    for (int x = 0; x <= 6; x++)
                    {
                        biomes.AddBiome(x, y, BiomeID.Beach);
                    }

                    for (int x = biomes.width - 7; x < biomes.width; x++)
                    {
                        biomes.AddBiome(x, y, BiomeID.Beach);
                    }
                }
            }
        }

        public void AddBiome(int i, int j, int type)
        {
            biomeMap[i, j] = type;
        }

        public int FindBiome(float x, float y, bool blending = true)
        {
            if (!blending)
            {
                //if ((int)y >= Main.maxTilesY - 200)
                //{
                //    return null;
                //}
                int i = (int)MathHelper.Clamp(x, 20, Main.maxTilesX - 20);
                int j = (int)MathHelper.Clamp(y, 20, Main.maxTilesY - 20);
                return biomeMap[(int)(i / scale), (int)(j / scale)];
            }
            else
            {
                int i = (int)MathHelper.Clamp(x + blendingX[(int)x, (int)y] * blendDistance, 20, Main.maxTilesX - 20);
                int j = (int)MathHelper.Clamp(y + blendingY[(int)x, (int)y] * blendDistance, 20, Main.maxTilesY - 20);
                return biomeMap[(int)(i / scale), (int)(j / scale)];
            }
        }

        public int FindLayer(int x, int y)
        {
            return (int)MathHelper.Clamp(y + blendingY[(int)x, (int)y] * blendDistance, 20, Main.maxTilesY - 20) / scale;
        }

        public bool UpdatingBiome(float x, float y, bool[] biomesToUpdate, int type)
        {
            return biomesToUpdate[type] && FindBiome(x, y) == type;
        }

        public int skyLayer => (int)(Main.worldSurface * 0.4) / scale;
        public int surfaceLayer => (int)(Main.worldSurface + 25) / scale;
        public int caveLayer => (int)(Main.rockLayer + 25) / scale;
        public int lavaLayer => GenVars.lavaLine / scale - 1;

        public void UpdateMap(int[] biomes, GenerationProgress progress)
        {
            progress.Message = "Updating biome map";

            FastNoiseLite caves1 = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            FastNoiseLite caves2 = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            FastNoiseLite caves3 = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));

            FastNoiseLite fleshcaves = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            fleshcaves.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            //fleshcaves.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);
            fleshcaves.SetFrequency(0.04f);
            fleshcaves.SetFractalType(FastNoiseLite.FractalType.PingPong);
            fleshcaves.SetFractalGain(0.8f);
            fleshcaves.SetFractalWeightedStrength(0.25f);
            fleshcaves.SetFractalPingPongStrength(1.5f);

            FastNoiseLite fossils = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            fossils.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            fossils.SetFrequency(0.1f);
            fossils.SetFractalType(FastNoiseLite.FractalType.Ridged);

            FastNoiseLite roots = new FastNoiseLite();
            roots.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            roots.SetFrequency(0.1f);
            roots.SetFractalType(FastNoiseLite.FractalType.FBm);
            roots.SetFractalOctaves(3);

            FastNoiseLite background = new FastNoiseLite();

            bool[] biomesToUpdate = new bool[129];
            for (int i = 0; i < biomes.Length; i++)
            {
                biomesToUpdate[biomes[i]] = true;
            }

            int startY = 40;
            if (!biomesToUpdate[BiomeID.Clouds])
            {
                if (!biomesToUpdate[BiomeID.Tundra] && !biomesToUpdate[BiomeID.Jungle] && !biomesToUpdate[BiomeID.Desert] && !biomesToUpdate[BiomeID.Corruption] && !biomesToUpdate[BiomeID.Crimson])
                {
                    startY = (int)Main.worldSurface - blendDistance * 2;
                    if (biomesToUpdate[BiomeID.Beach])
                    {
                        startY -= 100;
                    }
                }
                //else if (biomes.Contains(BiomeID.Underworld) || biomes.Contains("sulfurlayer"))
                //{
                //    startY = Main.maxTilesY - 200 - blendDistance;
                //}
            }

            //for (int j = -1; j <= 1; j++)
            //{
            //    for (int i = -1; i <= 1; i++)
            //    {

            //    }
            //}

            bool lunarVeil = ModLoader.TryGetMod("Stellamod", out Mod lv);

            for (float y = startY; y < Main.maxTilesY - 40; y++)
            {
                progress.Set((y - startY) / (Main.maxTilesY - 20 - startY));

                for (float x = 40; x < Main.maxTilesX - 40; x++)
                {
                    Tile tile = WGTools.Tile(x, y);

                    int i = (int)MathHelper.Clamp(x + blendingX[(int)x, (int)y] * blendDistance, 20, Main.maxTilesX - 20);
                    int j = (int)MathHelper.Clamp(y + blendingY[(int)x, (int)y] * blendDistance, 20, Main.maxTilesY - 20);

                    int layer = (j / scale);

                    bool beach = (i / scale <= 6 || i / scale >= width - 7);
                    bool underground = layer >= surfaceLayer;
                    bool sky = layer < skyLayer;

                    #region custom
                    if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Hive))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        caves1.SetFrequency(0.02f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalOctaves(3);
                        //caves.SetFractalGain(1);
                        caves1.SetFractalLacunarity(2);
                        //caves.SetFractalWeightedStrength(-0.45f);
                        caves1.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
                        caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Sub);

                        caves2.SetNoiseType(FastNoiseLite.NoiseType.Value);
                        caves2.SetFrequency(0.015f);
                        caves2.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves2.SetFractalOctaves(3);
                        caves2.SetFractalLacunarity(2);

                        float _caves = caves1.GetNoise(x, y * 2);
                        float _background = caves1.GetNoise(x + 999, y * 2 + 999);

                        tile.TileType = TileID.Hive;
                        if (_caves < -0.75f)
                        {
                            tile.HasTile = true;
                            if (_caves < -0.85f)
                            {
                                if (WorldGen.genRand.NextBool(25))
                                {
                                    tile.TileType = TileID.JungleGrass;
                                }
                                else tile.TileType = TileID.Mud;
                            }
                            tile.Slope = SlopeType.Solid;
                        }
                        else tile.HasTile = false;
                        if (_background < -0.75f)
                        {
                            tile.WallType = WallID.HiveUnsafe;
                        }
                        else tile.WallType = 0;

                        WGTools.Tile(x, y).LiquidType = 2;
                        if (WorldGen.genRand.NextBool(20))
                        {
                            WGTools.Tile(x, y).LiquidAmount = 255;
                        }
                        else WGTools.Tile(x, y).LiquidAmount = 0;
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.OceanCave))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFrequency(0.04f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalOctaves(3);
                        //caves.SetFractalGain(1);
                        caves1.SetFractalLacunarity(2);
                        //caves.SetFractalWeightedStrength(-0.45f);
                        caves1.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
                        caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Sub);

                        if (layer > surfaceLayer)
                        {
                            SetDefaultValues(caves2);

                            caves2.SetNoiseType(FastNoiseLite.NoiseType.Value);
                            caves2.SetFrequency(0.02f);
                            caves2.SetFractalType(FastNoiseLite.FractalType.FBm);
                            caves2.SetFractalOctaves(3);

                            float _caves = caves1.GetNoise(x, y * 2);
                            float _size = (caves3.GetNoise(x, y * 2) / 2) + 1;

                            if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass || tile.TileType == TileID.ClayBlock || tile.TileType == TileID.Stone || tile.TileType == TileID.Silt)
                            {
                                if (MaterialBlend(x, y, frequency: 2) < -0.15f)
                                {
                                    if (WorldGen.genRand.NextBool(10))
                                    {
                                        tile.TileType = TileID.ArgonMoss;
                                    }
                                    else tile.TileType = TileID.Stone;
                                }
                                else if (MaterialBlend(x, y, frequency: 2) <= 0.15f)
                                {
                                    tile.TileType = TileID.Coralstone;// Stone;
                                }
                                else tile.TileType = TileID.Sand;
                            }
                            if (_caves + _size * 0.2f < 0.2f)
                            {
                                tile.HasTile = true;
                                tile.Slope = SlopeType.Solid;
                            }
                            else tile.HasTile = false;
                            //if (MaterialBlend(x, y, true, 2) <= -0.2f)
                            //{
                            //    tile.HasTile = true;
                            //    WGTools.GetTile(x, y).TileType = TileID.Coralstone;
                            //}
                            WGTools.Tile(x, y).LiquidType = 0;
                            WGTools.Tile(x, y).LiquidAmount = 255;

                            if (!WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                            {
                                if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Stone)
                                {
                                    WGTools.Tile(x - 1, y - 1).TileType = TileID.ArgonMoss;
                                }
                            }
                        }
                        else
                        {
                            tile.HasTile = false;
                            WGTools.Tile(x, y).LiquidType = 0;
                            if (y >= Main.worldSurface - 60)
                            {
                                WGTools.Tile(x, y).LiquidAmount = 255;
                            }
                        }
                        if (layer >= surfaceLayer - 1)
                        {
                            float _background = caves1.GetNoise(x + 999, y * 2 + 999);

                            if (_background < 0 && layer >= surfaceLayer)
                            {
                                if (MaterialBlend(x, y, frequency: 4) >= 0f)
                                {
                                    if (x < Main.maxTilesX / 2)
                                    {
                                        tile.WallType = WallID.RocksUnsafe4;
                                    }
                                    else tile.WallType = WallID.RocksUnsafe3;
                                }
                                else tile.WallType = WallID.HallowUnsafe4;
                            }
                            else tile.WallType = 0;
                        }
                    }
                    //else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Flesh))
                    //{
                    //    caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                    //    //caves.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);
                    //    caves1.SetFrequency(0.04f);
                    //    caves1.SetFractalType(FastNoiseLite.FractalType.PingPong);
                    //    caves1.SetFractalGain(0.8f);
                    //    caves1.SetFractalWeightedStrength(0.25f);
                    //    caves1.SetFractalPingPongStrength(1.5f);

                    //    if (MaterialBlend(x, y) < -0.7f)
                    //    {
                    //        tile.TileType = (ushort)ModContent.TileType<hardstone>();
                    //    }
                    //    else tile.TileType = (ushort)ModContent.TileType<flesh>();

                    //    float _caves = fleshcaves.GetNoise(x, y + ((float)Math.Cos(x / 40) * 20));
                    //    if (_caves > -0.25f)
                    //    {
                    //        tile.HasTile = false;
                    //        tile.LiquidAmount = 0;
                    //    }
                    //    else tile.HasTile = true;

                    //    if (_caves > 0.15f)
                    //    {
                    //        tile.WallType = 0;
                    //    }
                    //    else
                    //    {
                    //        if (MaterialBlend(x, y) < -0.7f)
                    //        {
                    //            tile.WallType = (ushort)ModContent.WallType<hardstonewall>();
                    //        }
                    //        else tile.WallType = (ushort)ModContent.WallType<fleshwall>();
                    //    }
                    //    //if (background.GetNoise(x, y * 1.5f + ((float)Math.Cos(x / 60) * 20)) > 0.1f)
                    //    //{
                    //    //    if (tile.type == ModContent.TileType<hardstone>())
                    //    //    {
                    //    //        tile.wall = (ushort)ModContent.WallType<hardstonewall>();
                    //    //    }
                    //    //    else tile.wall = (ushort)ModContent.WallType<fleshwall>();
                    //    //}
                    //    //else tile.wall = 0;
                    //}
                    #endregion
                    #region surface
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Tundra))
                    {
                        if (layer >= caveLayer && layer < caveLayer + 2 * (Main.maxTilesY / 1200f) && lunarVeil)
                        {
                            if (lv.TryFind<ModTile>("AbyssalDirt", out ModTile aDirt))
                            {
                                tile.TileType = aDirt.Type;
                            }
                        }
                        else if (tile.TileType == TileID.Silt)
                        {
                            tile.TileType = TileID.Slush;
                        }
                        else if (MaterialBlend(x, y, frequency: 2) >= 0.2f)
                        {
                            tile.TileType = TileID.SnowBlock;
                        }
                        else
                        {
                            tile.TileType = TileID.IceBlock;

                            if (layer < surfaceLayer)
                            {
                                for (int k = 1; k <= WorldGen.genRand.Next(4, 7); k++)
                                {
                                    if (!WGTools.Tile(x, y - k).HasTile)
                                    {
                                        tile.TileType = TileID.SnowBlock;
                                        break;
                                    }
                                }
                            }
                        }

                        //if (layer < surfaceLayer - 1 - (2 * Main.maxTilesY / 1200f * ModContent.GetInstance<Client>().TerrainAmplitude) || layer >= surfaceLayer)
                        //{
                        //    if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass || tile.TileType == TileID.ClayBlock || tile.TileType == TileID.Sand)
                        //    {
                        //        tile.TileType = TileID.SnowBlock;
                        //    }
                        //    else if (tile.TileType == TileID.Stone)
                        //    {
                        //        tile.TileType = TileID.IceBlock;
                        //    }
                        //    else if (tile.TileType == TileID.Silt)
                        //    {
                        //        tile.TileType = TileID.Slush;
                        //    }
                        //    if (tile.WallType == WallID.DirtUnsafe || tile.WallType == WallID.Cave6Unsafe)
                        //    {
                        //        tile.WallType = WallID.SnowWallUnsafe;
                        //    }
                        //}

                        if (tile.WallType == WallID.DirtUnsafe || tile.WallType == WallID.Cave6Unsafe)
                        {
                            tile.WallType = WallID.SnowWallUnsafe;
                        }

                        SetDefaultValues(caves1);
                        SetDefaultValues(caves2);
                        SetDefaultValues(caves3);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFractalType(FastNoiseLite.FractalType.PingPong);
                        caves1.SetFractalOctaves(3);
                        caves1.SetFrequency(0.0125f);

                        caves2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves2.SetFrequency(0.025f);
                        caves2.SetFractalType(FastNoiseLite.FractalType.FBm);

                        caves3.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves3.SetFrequency(0.075f);
                        caves3.SetFractalType(FastNoiseLite.FractalType.FBm);

                        if (layer >= surfaceLayer)
                        {
                            caves1.SetFractalPingPongStrength(caves2.GetNoise(x, y * 2) + 2);
                            float _caves = caves1.GetNoise(x, y * 2);
                            float _size = (caves3.GetNoise(x, y * 2) / 2) + 1;

                            if (_caves < -_size * 0.1f)
                            {
                                tile.HasTile = false;
                                if (WorldGen.genRand.NextBool(50) && y < GenVars.lavaLine)
                                {
                                    tile.LiquidAmount = 255;
                                }
                            }
                            else
                            {
                                tile.HasTile = true;
                            }


                            if (_caves + 1 > _size * 0.6f)
                            {
                                if (MaterialBlend(x, y, frequency: 2) >= 0.2f)
                                {
                                    tile.WallType = WallID.SnowWallUnsafe;
                                }
                                else tile.WallType = WallID.IceUnsafe;
                            }
                            else if (y > Main.worldSurface)
                            {
                                tile.WallType = 0;
                            }
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Jungle))
                    {
                        SetDefaultValues(caves1);
                        SetDefaultValues(caves2);
                        SetDefaultValues(caves3);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFractalType(FastNoiseLite.FractalType.PingPong);
                        caves1.SetFractalOctaves(3);
                        caves1.SetFrequency(0.0125f);
                        //caves.SetFractalGain(0.25f);

                        caves2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves2.SetFrequency(0.025f);
                        caves2.SetFractalType(FastNoiseLite.FractalType.FBm);

                        caves3.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves3.SetFrequency(0.075f);
                        caves3.SetFractalType(FastNoiseLite.FractalType.FBm);

                        background.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        background.SetFrequency(0.04f);
                        background.SetFractalType(FastNoiseLite.FractalType.FBm);
                        background.SetFractalOctaves(2);
                        background.SetFractalWeightedStrength(-1);
                        background.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
                        background.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);

                        if (MaterialBlend(x, y, frequency: 2) >= -0.2f)
                        {
                            if (tile.TileType == TileID.Grass || WorldGen.genRand.NextBool(25))
                            {
                                tile.TileType = TileID.JungleGrass;
                            }
                            else tile.TileType = TileID.Mud;
                        }
                        else
                        {
                            tile.TileType = TileID.Stone;

                            if (layer < surfaceLayer)
                            {
                                for (int k = 1; k <= WorldGen.genRand.Next(4, 7); k++)
                                {
                                    if (!WGTools.Tile(x, y - k).HasTile)
                                    {
                                        tile.TileType = TileID.Mud;
                                        break;
                                    }
                                }
                            }
                        }

                        if (layer >= surfaceLayer && !beach)
                        {
                            caves1.SetFractalPingPongStrength(caves2.GetNoise(x, y * 2) + 2);
                            float _caves = caves1.GetNoise(x, y * 2);
                            float _size = (caves3.GetNoise(x, y * 2) / 2) + 1;

                            if (_caves > -_size * 0.1f)
                            {
                                tile.HasTile = false;
                                if (WorldGen.genRand.NextBool(50) && y < GenVars.lavaLine)
                                {
                                    tile.LiquidAmount = 255;
                                }
                            }
                            else
                            {
                                tile.HasTile = true;
                            }

                            if (_caves < _size * 0.4f)
                            {
                                if (MaterialBlend(x, y, frequency: 2) >= -0.2f)
                                {
                                    tile.WallType = WallID.JungleUnsafe;
                                }
                                else tile.WallType = WallID.JungleUnsafe3; //layer >= lavaLayer ? WallID.LavaUnsafe2 :
                            }
                            else if (y > Main.worldSurface)
                            {
                                tile.WallType = 0;
                            }
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Desert))
                    {
                        SetDefaultValues(caves1);
                        SetDefaultValues(caves2);
                        SetDefaultValues(caves3);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFrequency(0.03f / 2);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalLacunarity(1.75f);
                        caves1.SetFractalGain(0.8f);
                        caves2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves2.SetFrequency(0.03f / 2);
                        caves2.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves3.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves3.SetFrequency(0.015f / 2);
                        caves3.SetFractalType(FastNoiseLite.FractalType.FBm);

                        background.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        background.SetFrequency(0.1f);
                        background.SetFractalType(FastNoiseLite.FractalType.None);
                        background.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
                        background.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);

                        if (layer >= surfaceLayer)
                        {
                            float _tunnels = caves1.GetNoise(x, y * 2);
                            //float _nests = nests.GetNoise(x, y + ((float)Math.Cos(x / 60) * 20)) * ((nests2.GetNoise(x, y + ((float)Math.Cos(x / 60) * 20)) + 1) / 2);
                            float _fossils = ((fossils.GetNoise(x, y * 2) + 1) / 2) * 0.4f;

                            float _background = ((background.GetNoise(x, y * 2) + 1) / 2) * 0.3f;


                            float _size = (caves2.GetNoise(x, y * 2 + ((float)Math.Cos(x / 60) * 20)) + 1) / 2 / 4;
                            float _offset = caves3.GetNoise(x, y * 2 + ((float)Math.Cos(x / 60) * 20));

                            //if (MaterialBlend(x, y, frequency: 2) <= 0.2f)
                            //{
                            //    WGTools.Tile(x, y).TileType = TileID.HardenedSand;
                            //}
                            //else tile.TileType = TileID.Sand;
                            tile.HasTile = true;
                            tile.Slope = 0;
                            tile.LiquidAmount = 0;
                            if (_tunnels + 0.1f + _fossils < _offset - _size || _tunnels - 0.1f - _fossils > _offset + _size)
                            {
                                tile.TileType = TileID.DesertFossil;
                                tile.WallType = WallID.DesertFossil;
                            }
                            else if (_tunnels + 0.1f > _offset - _size && _tunnels - 0.1f < _offset + _size)
                            {
                                //if (gems.GetNoise(x * 2, y * 2) > 0.4f)
                                //{
                                //    tile.TileType = (ushort)ModContent.TileType<sandstoneamber>();
                                //}
                                //else
                                //{

                                //}
                                tile.TileType = TileID.Sandstone;
                                tile.WallType = WallID.Sandstone;

                                if (_tunnels > _offset - _size && _tunnels < _offset + _size && !beach)
                                {
                                    tile.HasTile = false;
                                    tile.LiquidAmount = 0;

                                    if (_tunnels - 0.05f - _background > _offset - _size && _tunnels + 0.05f + _background < _offset + _size)
                                    {
                                        tile.WallType = WallID.HardenedSand;
                                    }
                                }
                            }
                            else
                            {
                                tile.TileType = TileID.HardenedSand;
                                tile.WallType = WallID.HardenedSand;
                            }
                        }
                        else
                        {
                            if (MaterialBlend(x, y, frequency: 2) <= 0)
                            {
                                WGTools.Tile(x, y).TileType = TileID.HardenedSand;
                            }
                            else tile.TileType = TileID.Sand;

                            if (layer == surfaceLayer - 1)
                            {
                                tile.HasTile = true;
                                tile.TileType = TileID.HardenedSand;
                                tile.WallType = WallID.HardenedSand;
                            }
                            else tile.LiquidAmount = 0;
                            if (tile.HasTile)// && y > Terrain.Middle)
                            {
                                int var = 1;
                                //if (y / biomes.scale >= (int)Main.worldSurface / biomes.scale - 1 || dunes.GetNoise(x, y + 2) > 0)
                                //{
                                //    var = 2;
                                //}
                                for (int k = -var; k <= var; k++)
                                {
                                    if (!WGTools.Tile(x + k, y + 1).HasTile)
                                    {
                                        WGTools.Tile(x + k, y + 1).TileType = TileID.Sand;
                                        WGTools.Tile(x + k, y + 1).HasTile = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Corruption))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        caves1.SetFrequency(0.015f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.Ridged);
                        caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
                        caves1.SetFractalOctaves(3);
                        caves1.SetFractalGain(0.75f);
                        caves1.SetFractalWeightedStrength(0.5f);

                        if (!Main.wallDungeon[tile.WallType])
                        {
                            float _size = 1;// (caves2.GetNoise(x * 2, y) + 1) / 2;
                            float _caves = caves1.GetNoise(x, y) / 2;
                            //float _offset = caves3.GetNoise(x, y);

                            float thing = (_caves) / (1 + 1 / 2);

                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(Corruption.orbX, !WorldGen.crimson ? Corruption.orbYPrimary : Corruption.orbYSecondary));
                            thing += MathHelper.Clamp((1 - dist / (Main.maxTilesX / 4200f * 48)) * 2, 0, 1);

                            bool ug = layer >= surfaceLayer - 1;
                            if (ug || tile.WallType != 0)
                            {
                                tile.HasTile = true;
                            }

                            if (thing > 0.25f - _size / (underground ? 2.5f : 3))//thing > _offset - _size / (ug ? 2 : 3) && thing < _offset + _size / (ug ? 2 : 3) && !sky)
                            {
                                tile.TileType = TileID.Ebonstone;
                                if (ug || tile.WallType != 0)
                                {
                                    tile.WallType = WallID.EbonstoneUnsafe;
                                }
                                if (thing > 0) //thing > _offset - _size / 5 && thing < _offset + _size / 5)
                                {
                                    tile.HasTile = false;

                                    if (ug && thing > 0.2f)
                                    {
                                        tile.WallType = WallID.CorruptionUnsafe2;
                                    }
                                }
                            }
                            else
                            {
                                tile.TileType = TileID.Dirt;
                                if (ug)
                                {
                                    tile.WallType = WallID.DirtUnsafe;
                                }
                            }

                            if (!ug && WGTools.Tile(x - 1, y - 1).TileType == TileID.Dirt && !WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.CorruptGrass;
                            }
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Crimson))
                    {
                        SetDefaultValues(caves1);

                        //caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        //caves1.SetFrequency(0.015f);
                        //caves1.SetFractalType(FastNoiseLite.FractalType.Ridged);
                        //caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
                        //caves1.SetFractalOctaves(3);
                        //caves1.SetFractalGain(0.75f);
                        //caves1.SetFractalWeightedStrength(0.5f);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                        caves1.SetFrequency(0.01f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.Ridged);
                        caves1.SetFractalOctaves(4);
                        //caves1.SetFractalGain(0.6f);
                        caves1.SetFractalWeightedStrength(0.5f);

                        if (!Main.wallDungeon[tile.WallType])
                        {
                            float _size = 1;// (caves2.GetNoise(x, y) + 1) / 2 + 0.25f;
                            //caves1.SetFractalPingPongStrength(caves3.GetNoise(x, y) / 2 + 2);
                            float _caves = caves1.GetNoise(x, y);// / 2;
                            //float _offset = caves3.GetNoise(x, y) + 0.5f;

                            float thing = (_caves) / (1 + 1 / 2);

                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(Corruption.orbX, WorldGen.crimson ? Corruption.orbYPrimary : Corruption.orbYSecondary));
                            thing += MathHelper.Clamp((1 - dist / (Main.maxTilesX / 4200f * 48)) * 4, 0, 1);

                            bool ug = layer >= surfaceLayer - 1;
                            if (ug || tile.WallType != 0)
                            {
                                tile.HasTile = true;
                            }
                            if (!sky)
                            {
                                //if (thing > _offset - _size / (underground ? 2 : 3) && thing < _offset + _size / (underground ? 2 : 3) && !sky)
                                if (thing > 0.5f - _size / (underground ? 1.75f : 2.25f))
                                {
                                    tile.TileType = TileID.Crimstone;
                                    if (ug || tile.WallType != 0)
                                    {
                                        tile.WallType = WallID.CrimstoneUnsafe;
                                        //tile.WallColor = PaintID.DeepRedPaint;
                                    }
                                    if (thing > 0.225f)
                                    {
                                        tile.HasTile = false;

                                        //if (ug && thing > 0.75f)
                                        //{
                                        //    tile.WallType = WallID.CrimsonUnsafe3;
                                        //    //tile.WallColor = PaintID.DeepRedPaint;
                                        //}
                                    }
                                }
                                else
                                {
                                    tile.TileType = TileID.Dirt;
                                    if (ug)
                                    {
                                        tile.WallType = WallID.DirtUnsafe;
                                    }
                                }
                            }

                            if (!underground && WGTools.Tile(x - 1, y - 1).TileType == TileID.Dirt && !WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.CrimsonGrass;
                            }
                        }
                    }

                    #endregion
                    #region underground
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Glowshroom))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        caves1.SetFrequency(0.015f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalGain(0.7f);
                        caves1.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
                        caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);

                        if (MaterialBlend(x + WorldGen.genRand.Next(-1, 2), y + WorldGen.genRand.Next(-1, 2), true, 2) <= -0.3f)
                        {
                            WGTools.Tile(x, y).TileType = TileID.MushroomBlock;
                        }
                        else
                        {
                            if (tile.TileType == TileID.Grass || WorldGen.genRand.NextBool(25) || FindBiome(x + 1, y + 1) != BiomeID.Glowshroom && !WGTools.Solid(x + 1, y + 1))
                            {
                                tile.TileType = TileID.MushroomGrass;
                            }
                            else tile.TileType = TileID.Mud;
                        }

                        float _caves = caves1.GetNoise(x, y * 2); //+((float)Math.Cos(x / 60) * 20)

                        if (_caves < -0.275f)
                        {
                            tile.HasTile = false;
                            if (WorldGen.genRand.NextBool(25))
                            {
                                tile.LiquidAmount = 255;
                            }
                            else tile.LiquidAmount = 0;
                        }
                        else
                        {
                            tile.HasTile = true;
                            tile.Slope = 0;
                        }

                        if (!WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                        {
                            if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Mud || WGTools.Tile(x - 1, y - 1).TileType == TileID.MushroomBlock)
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.MushroomGrass;
                            }
                        }

                        if (caves1.GetNoise(x + 999, y * 2 + 999) > -0.2f)
                        {
                            tile.WallType = WallID.MushroomUnsafe;
                        }
                        else tile.WallType = 0;
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.GemCave))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFrequency(0.02f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);

                        float _caves = caves1.GetNoise(x, y * 2);

                        int gemType = RemTile.GetGemType(j);

                        ushort gemBlock = gemType == 5 ? TileID.Diamond : gemType == 4 ? TileID.Ruby : gemType == 3 ? TileID.Emerald : gemType == 2 ? TileID.Sapphire : gemType == 1 ? TileID.Topaz : TileID.Amethyst;
                        ushort gemWall = gemType == 5 ? WallID.DiamondUnsafe : gemType == 4 ? WallID.RubyUnsafe : gemType == 3 ? WallID.EmeraldUnsafe : gemType == 2 ? WallID.SapphireUnsafe : gemType == 1 ? WallID.TopazUnsafe : WallID.AmethystUnsafe;

                        if (_caves < -0.1f || _caves > 0.1f)
                        {
                            tile.HasTile = false;
                            tile.LiquidAmount = 0;
                        }
                        else
                        {
                            tile.HasTile = true;
                            tile.Slope = 0;
                        }

                        WGTools.Tile(x, y).TileType = TileID.Stone;

                        if (!WGTools.SurroundingTilesActive(x - 1, y - 1))
                        {
                            if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Stone && WGTools.Tile(x - 1, y - 2).TileType != TileID.GemSaplings && WorldGen.genRand.NextBool(3))
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = gemBlock;
                            }
                        }

                        if (_caves > -0.3f && _caves < 0.3f)
                        {
                            tile.WallType = gemWall;
                        }
                        else tile.WallType = 0;
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Obsidian))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        caves1.SetFrequency(0.02f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalGain(0.75f);
                        caves1.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Manhattan);
                        caves1.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Sub);

                        if (lunarVeil && lv.TryFind<ModTile>("CindersparkDirt", out ModTile csDirt))
                        {
                            tile.TileType = csDirt.Type;
                        }
                        else if (MaterialBlend(x, y, frequency: 2) >= -0.1f)
                        {
                            tile.TileType = TileID.Obsidian;
                            tile.WallType = WallID.ObsidianBackUnsafe;
                        }
                        else
                        {
                            tile.TileType = TileID.Ash;
                            tile.WallType = WallID.LavaUnsafe1;
                        }

                        float _caves = caves1.GetNoise(x, y * 2); //+((float)Math.Cos(x / 60) * 20)


                        if (_caves > -0.7f)
                        {
                            tile.HasTile = false;
                            tile.LiquidAmount = 0;

                            if (_caves > -0.55f)
                            {
                                tile.WallType = 0;
                            }
                        }
                        else
                        {
                            tile.HasTile = true;
                            tile.Slope = 0;
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Marble))
                    {
                        tile.TileType = TileID.Marble;
                        tile.HasTile = true;

                        tile.LiquidType = 0;

                        if (FindBiome(x, y - 1) == BiomeID.Marble && FindBiome(x, y + 1) == BiomeID.Marble)
                        {
                            tile.WallType = WallID.MarbleUnsafe;
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Granite))
                    {
                        tile.TileType = TileID.Granite;
                        tile.HasTile = true;

                        tile.LiquidType = 0;

                        if (FindBiome(x - 1, y) == BiomeID.Granite && FindBiome(x + 1, y) == BiomeID.Granite)
                        {
                            tile.WallType = WallID.GraniteUnsafe;
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Toxic))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Value);
                        caves1.SetFrequency(0.02f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.FBm);
                        caves1.SetFractalGain(0.6f);

                        if (MaterialBlend(x, y, true) <= -0.25f)
                        {
                            WGTools.Tile(x, y).TileType = (ushort)ModContent.TileType<ToxicWaste>();
                        }
                        else WGTools.Tile(x, y).TileType = TileID.Stone;

                        float _caves = caves1.GetNoise(x, y * 2);

                        if (_caves < -0.05f)
                        {
                            tile.HasTile = false;
                            if (WorldGen.genRand.NextBool(15))
                            {
                                tile.LiquidAmount = 255;
                            }
                            else tile.LiquidAmount = 0;
                        }
                        else
                        {
                            tile.HasTile = true;
                            tile.Slope = 0;
                        }

                        if (caves1.GetNoise(x + 999, y * 2 + 999) < -0.1f)
                        {
                            tile.WallType = WallID.JungleUnsafe3;
                        }

                        if (!WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                        {
                            if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Stone)
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.KryptonMoss;
                            }
                            else if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Mud)
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.JungleGrass;
                            }
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Underworld) || UpdatingBiome(x, y, biomesToUpdate, BiomeID.AshForest))
                    {
                        background.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                        background.SetFrequency(0.025f);
                        background.SetFractalType(FastNoiseLite.FractalType.Ridged);
                        background.SetFractalWeightedStrength(-0.5f);
                        background.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
                        background.SetFractalOctaves(2);

                        tile.HasTile = true;

                        if (tile.TileType != ModContent.TileType<Hardstone>())
                        {
                            if (layer >= height - 1)
                            {
                                tile.TileType = (ushort)ModContent.TileType<Hardstone>();
                                tile.HasTile = true;
                            }

                            else tile.TileType = TileID.Ash;
                        }
                    }
                    else if (UpdatingBiome(x, y, biomesToUpdate, BiomeID.Aether))
                    {
                        SetDefaultValues(caves1);

                        caves1.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                        caves1.SetFrequency(0.015f);
                        caves1.SetFractalType(FastNoiseLite.FractalType.Ridged);

                        float _caves = caves1.GetNoise(x, y * 2);


                        tile.LiquidType = LiquidID.Shimmer;

                        if (_caves < 0.6f)
                        {
                            tile.HasTile = false;
                            if (WorldGen.genRand.NextBool(5))
                            {
                                tile.LiquidAmount = 255;
                            }
                            else tile.LiquidAmount = 0;
                        }
                        else
                        {
                            tile.HasTile = true;
                            tile.Slope = 0;
                        }


                        if (_caves >= 0.85f)
                        {
                            WGTools.Tile(x, y).TileType = TileID.ShimmerBlock;
                        }
                        else WGTools.Tile(x, y).TileType = TileID.Stone;

                        if (!WGTools.SurroundingTilesActive(x - 1, y - 1))
                        {
                            if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Stone)
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.VioletMoss;
                            }
                        }

                        if (caves1.GetNoise(x + 999, y * 2 + 999) > 0.6f)
                        {
                            tile.WallType = WallID.HallowUnsafe1;
                            tile.WallColor = PaintID.PurplePaint;
                        }
                        else tile.WallType = 0;
                    }
                    #endregion

                    if (y >= Main.maxTilesY - 20)
                    {
                        break;
                    }
                    
                    if (biomesToUpdate[BiomeID.Beach] && beach)
                    {
                        tile.WallType = 0;
                        if (layer > surfaceLayer - 1)
                        {
                            tile.HasTile = true;
                        }
                        if (layer >= surfaceLayer)
                        {
                            if ((i / scale) == 0 || (i / scale) == width - 1)
                            {
                                tile.TileType = (ushort)ModContent.TileType<Hardstone>();
                            }
                            else if (tile.TileType == TileID.ArgonMoss)
                            {
                                tile.TileType = TileID.Stone;
                            }
                            else if (tile.TileType == TileID.JungleGrass || tile.TileType == TileID.MushroomGrass)
                            {
                                tile.TileType = TileID.Mud;
                            }
                        }
                        else
                        {
                            if (MaterialBlend(x, y, frequency: 2) <= 0)
                            {
                                WGTools.Tile(x, y).TileType = TileID.HardenedSand;
                            }
                            else tile.TileType = TileID.Sand;

                            if (WGTools.Tile(x, y).WallType == WallID.DirtUnsafe || WGTools.SurroundingTilesActive(x, y))
                            {
                                tile.WallType = WallID.HardenedSand;
                            }
                        }
                    }
                }
            }
        }

        public float MaterialBlend(float x, float y, bool flip = false, float frequency = 1)
        {
            x *= frequency;
            y *= frequency;
            if (frequency > 1)
            {
                x %= Main.maxTilesX;
                y %= Main.maxTilesY;
            }
            //double noiseX = 0;
            //double noiseY = 0;
            //float multiplier = 1;
            //for (int i = 0; i < materialsX.Length; i++)
            //{
            //    noiseX += Math.Sin((x + WorldGen.genRand.NextFloat(-0.5f, 0.5f)) * materialsX[i] * frequency * multiplier) / materialsX.Length;
            //    noiseY += Math.Sin((y + WorldGen.genRand.NextFloat(-0.5f, 0.5f)) * materialsY[i] * frequency * multiplier) / materialsY.Length;
            //    multiplier *= 1.75f;
            //}
            if (flip ? WorldGen.InWorld((int)x, Main.maxTilesY - (int)y) : WorldGen.InWorld((int)x, (int)y))
            {
                return materials[(int)x, flip ? Main.maxTilesY - (int)y : (int)y];
            }
            else return 0;
        }

        private void SetDefaultValues(FastNoiseLite noise)
        {
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFractalGain(0.5f);
            noise.SetFractalLacunarity(2);
            noise.SetFractalWeightedStrength(0);
            noise.SetFractalPingPongStrength(2);
            noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
            noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);
            noise.SetCellularJitter(1);
        }
    }
    #endregion

    #region Primary
    public class NewPrimaryBiomes : GenPass
    {
        public NewPrimaryBiomes(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        internal class Tundra
        {
            public static int X;
            public static int Y;
            public static float Size;
            public static float HeightMultiplier;
        }

        internal class Jungle
        {
            public static int Center;
            public static int Size;
        }

        internal class Desert
        {
            public static int X;
            public static int Y;
            public static int Size;
        }

        internal class Corruption
        {
            public static int X;
            public static int Y;
            public static int Size;
            public static float heightMultiplier;

            public static int orbX;
            public static int orbYPrimary => (int)Main.rockLayer;
            public static int orbYSecondary => Main.maxTilesY - 300 - (orbYPrimary - (int)Main.worldSurface);

            public static void CreateOrb(bool alternate)
            {
                int orbY = alternate ? orbYSecondary : orbYPrimary;

                int radius = 48;

                FastNoiseLite noise = new FastNoiseLite();
                noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                noise.SetFrequency(0.1f);
                noise.SetFractalType(FastNoiseLite.FractalType.None);

                FastNoiseLite noise2 = new FastNoiseLite();
                noise2.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                noise2.SetFrequency(0.2f);
                noise2.SetFractalType(FastNoiseLite.FractalType.None);

                bool crimson = WorldGen.crimson ^ alternate;

                for (int j = (int)(orbY - radius * 1.5f); j <= orbY + radius * 1.5f; j++)
                {
                    for (int i = (int)(orbX - radius * 1.5f); i <= orbX + radius * 1.5f; i++)
                    {
                        float distance = Vector2.Distance(new Vector2(i, j), new Vector2(orbX, orbY)) + noise.GetNoise(i, j) * 10;

                        Tile tile = Main.tile[i, j];

                        if (distance < 16 * Main.maxTilesX / 4200f)
                        {
                            tile.TileType = crimson ? TileID.FleshBlock : TileID.LesionBlock;
                            tile.WallType = crimson ? WallID.CrimsonUnsafe3 : WallID.CorruptionUnsafe3;

                            tile.WallColor = crimson ? PaintID.DeepRedPaint : PaintID.OrangePaint;
                        }

                        if (distance < 12 * Main.maxTilesX / 4200f)
                        {
                            if (noise2.GetNoise(i, j) > -0.7f)
                            {
                                tile.HasTile = true;
                            }
                            else
                            {
                                tile.HasTile = false;

                                if (crimson && noise2.GetNoise(i, j) < -0.9f)
                                {
                                    tile.WallType = WallID.CrimsonUnsafe2;
                                }
                            }

                            tile.LiquidAmount = 51;
                        }
                        else if (distance < 16 * Main.maxTilesX / 4200f)
                        {
                            tile.HasTile = true;
                        }
                        else if (distance < 20 * Main.maxTilesX / 4200f)
                        {
                            tile.TileType = crimson ? TileID.Crimstone : TileID.Ebonstone;
                            tile.HasTile = true;
                        }
                    }
                }

                int count = (int)(8 * Main.maxTilesX / 4200f);
                while (count > 0)
                {
                    int x = orbX + (int)(WorldGen.genRand.NextFloat(-12, 12) * Main.maxTilesX / 4200f);
                    int y = orbY + (int)(WorldGen.genRand.NextFloat(-12, 12) * Main.maxTilesX / 4200f);

                    bool valid = true;

                    for (int j = y - 1; j <= y + 2; j++)
                    {
                        for (int i = x - 1; i <= x + 2; i++)
                        {
                            if (WGTools.Tile(i, j).HasTile)
                            {
                                valid = false;
                            }
                        }
                    }
                    for (int j = y - 3; j <= y + 4; j++)
                    {
                        for (int i = x - 3; i <= x + 4; i++)
                        {
                            if (WGTools.Tile(i, j).HasTile && WGTools.Tile(i, j).TileType == TileID.ShadowOrbs)
                            {
                                valid = false;
                            }
                        }
                    }

                    if (valid)
                    {
                        for (int j = y; j <= y + 1; j++)
                        {
                            for (int i = x; i <= x + 1; i++)
                            {
                                Tile tile = Main.tile[i, j];

                                tile.TileType = TileID.ShadowOrbs;
                                tile.HasTile = true;
                                tile.TileFrameX = (short)((i - x) * 18);
                                tile.TileFrameY = (short)((j - y) * 18);
                                if (WorldGen.crimson ^ alternate)
                                {
                                    tile.TileFrameX += 18 * 2;
                                }
                            }
                        }
                        count--;
                    }
                }
            }
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();
            FastNoiseLite noise;

            bool lunarVeil = ModLoader.TryGetMod("Stellamod", out Mod lv);

            #region tundra
            Tundra.Y = (int)Main.worldSurface / biomes.scale;
            Tundra.Size = biomes.width / 12.5f;
            Tundra.HeightMultiplier = (int)(RemWorld.lavaLevel) / biomes.scale / Tundra.Size;

            noise = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.2f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite thinice = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            thinice.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            thinice.SetFrequency(0.2f);
            thinice.SetFractalType(FastNoiseLite.FractalType.FBm);


            for (int j = 0; j < Math.Min(GenVars.lavaLine / biomes.scale - 1, biomes.height - 4); j++)
            {
                for (int i = 0; i < biomes.width; i++)
                {
                    if (noise.GetNoise(i, j) <= (1 - Vector2.Distance(new Vector2(Tundra.X, MathHelper.Clamp(j, 0, Tundra.Y)), new Vector2(i, (j - Tundra.Y) / Tundra.HeightMultiplier + Tundra.Y)) / (Tundra.Size * (j < biomes.surfaceLayer ? 0.75f : 1))) * 2)
                    {
                        biomes.AddBiome(i, j, BiomeID.Tundra);
                    }
                }
            }
            #endregion

            #region jungle
            progress.Message = "Adding mud";

            Jungle.Size = biomes.width / 9;

            noise = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.2f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            for (int j = 1; j < biomes.height; j++)
            {
                for (int i = 6; i < biomes.width - 6; i++)
                {
                    Vector2 point = new Vector2(Jungle.Center, j);
                    float _size = Jungle.Size;
                    if (biomes.biomeMap[i, j] != BiomeID.Obsidian && (biomes.biomeMap[i, j] != BiomeID.Beach || j >= biomes.surfaceLayer && j < biomes.caveLayer))
                    {
                        if (j < biomes.surfaceLayer && (i > Jungle.Center && i < Desert.X || i > Desert.X && i < Jungle.Center) || noise.GetNoise(i, j) <= (1 - Vector2.Distance(point, new Vector2(i, j)) / _size) * 2)
                        {
                            if (j < biomes.height - 4)
                            {
                                if (ModContent.GetInstance<Client>().ExperimentalWorldgen && j > biomes.lavaLayer)
                                {
                                    biomes.AddBiome(i, j, BiomeID.Toxic);
                                }
                                else biomes.AddBiome(i, j, BiomeID.Jungle);
                            }
                            else biomes.AddBiome(i, j, BiomeID.AshForest);
                        }
                    }
                }
            }
            #endregion

            #region desert
            progress.Message = "Adding sand";

            Desert.Y = biomes.surfaceLayer;

            Desert.Size = biomes.width / 20;

            int bottom = biomes.lavaLayer;

            GenVars.UndergroundDesertLocation = new Rectangle((Desert.X - Desert.Size) * biomes.scale, (biomes.surfaceLayer - 1) * biomes.scale, (Desert.Size * 2 + 1) * biomes.scale, (bottom + Desert.Size - biomes.surfaceLayer + 1) * biomes.scale);
            GenVars.UndergroundDesertLocation.Height = (int)(GenVars.UndergroundDesertLocation.Height * 1.25f);
            GenVars.structures.AddStructure(GenVars.UndergroundDesertLocation);

            noise = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.2f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            for (int j = biomes.surfaceLayer / 2; j < biomes.height - 4; j++)
            {
                for (int i = 7; i < biomes.width - 7; i++)
                {
                    Vector2 point = new Vector2(Desert.X, MathHelper.Clamp(j, 1, bottom));
                    if (noise.GetNoise(i, j) <= (1 - Vector2.Distance(point, new Vector2(i, j)) / Desert.Size) * 2)
                    {
                        biomes.AddBiome(i, j, BiomeID.Desert);
                    }
                }
            }

            #region chasm
            //FastNoiseLite chasm = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            //chasm.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            //chasm.SetFrequency(0.04f);
            //chasm.SetFractalType(FastNoiseLite.FractalType.FBm);
            //chasm.SetFractalOctaves(5);

            //for (int y = 1; y < WorldGen.UndergroundDesertLocation.Bottom; y++)
            //{
            //    for (int x = WorldGen.UndergroundDesertLocation.Left; x <= WorldGen.UndergroundDesertLocation.Right; x++)
            //    {
            //        float _caves = chasm.GetNoise(x * 2, y) * 1.5f;

            //        Vector2 point = new Vector2(WorldGen.UndergroundDesertLocation.X + WorldGen.UndergroundDesertLocation.Width / 2, y);
            //        float threshold = (Vector2.Distance(new Vector2(x, y), point) / (biomes.scale / 2)) * 2 - 1 + 0.5f;

            //        if (biomes.FindBiome(x, y) == BiomeID.Desert && _caves > threshold - 0.4f)
            //        {
            //            WGTools.GetTile(x, y).TileType = TileID.HardenedSand;
            //            if (_caves > threshold)
            //            {
            //                WGTools.GetTile(x, y).HasTile = false;
            //            }
            //        }

            //        if (biomes.FindBiome(x, y) == "desertunderground")
            //        {
            //            if (WGTools.GetTile(x, y).TileType == TileID.Sand && !WGTools.SurroundingTilesActive(x, y))
            //            {
            //                WGTools.GetTile(x, y).TileType = TileID.Sandstone;
            //                WGTools.GetTile(x, y).WallType = WallID.Sandstone;
            //            }
            //        }
            //    }
            //}
            #endregion

            #endregion

            #region corruption
            Corruption.Y = (int)Main.worldSurface / biomes.scale;

            Corruption.orbX = (int)((Corruption.X + 0.5f) * biomes.scale);

            Corruption.Size = biomes.width / 42;

            noise = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.2f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            for (int j = biomes.skyLayer; j < biomes.height - 6; j++)
            {
                for (int i = 0; i < biomes.width; i++)
                {
                    Vector2 point = new Vector2(Corruption.X, MathHelper.Clamp(j, 1, biomes.caveLayer));
                    if (noise.GetNoise(i, j) <= (1 - Vector2.Distance(point, new Vector2(i, j)) / Corruption.Size) * 2)
                    {
                        if (WorldGen.crimson)
                        {
                            biomes.AddBiome(i, j, BiomeID.Crimson);
                        }
                        else biomes.AddBiome(i, j, BiomeID.Corruption);
                    }

                    point = new Vector2(Corruption.X, MathHelper.Clamp(j, (Main.maxTilesY - 300 - (int)(Main.rockLayer - Main.worldSurface)) / biomes.scale - 1, biomes.height - 6));
                    if (noise.GetNoise(i, j) <= (1 - Vector2.Distance(point, new Vector2(i, j)) / Corruption.Size) * 2)
                    {
                        if (!WorldGen.crimson)
                        {
                            biomes.AddBiome(i, j, BiomeID.Crimson);
                        }
                        else biomes.AddBiome(i, j, BiomeID.Corruption);
                    }
                }
            }

            #endregion

            biomes.UpdateMap(new int[] { BiomeID.Tundra, BiomeID.Jungle, BiomeID.Desert, BiomeID.Corruption, BiomeID.Crimson, BiomeID.Underworld, BiomeID.AshForest, BiomeID.Obsidian, BiomeID.Beach, BiomeID.Toxic, BiomeID.Abysm }, progress);

            #region corruption
            progress.Message = "Incubating infection";
            for (int k = 0; k < 10; k++)
            {
                for (int y = 40; y < Main.worldSurface; y++)
                {
                    for (int x = (Corruption.X - Corruption.Size - 1) * biomes.scale; x <= (Corruption.X + Corruption.Size + 2) * biomes.scale; x++)
                    {
                        if (!WGTools.SurroundingTilesActive(x, y) && (WGTools.Tile(x, y).WallType == WallID.EbonstoneUnsafe || WGTools.Tile(x, y).WallType == WallID.CrimstoneUnsafe))
                        {
                            int adjacentWalls = 0;

                            if (WGTools.Tile(x + 1, y).WallType != 0 || WGTools.Tile(x + 1, y).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x, y + 1).WallType != 0 || WGTools.Tile(x, y + 1).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x - 1, y).WallType != 0 || WGTools.Tile(x - 1, y).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x, y - 1).WallType != 0 || WGTools.Tile(x, y - 1).HasTile)
                            {
                                adjacentWalls++;
                            }

                            if (k == 9)
                            {
                                if (adjacentWalls < 3)
                                {
                                    WGTools.Tile(x, y).WallType = (ushort)ModContent.WallType<Walls.dev.nothing>();
                                }
                            }
                            else if (adjacentWalls < 4 && WorldGen.genRand.NextBool(4 / (4 - adjacentWalls)))
                            {
                                WGTools.Tile(x, y).WallType = (ushort)ModContent.WallType<Walls.dev.nothing>();
                            }
                        }
                    }
                }

                for (int y = 40; y < Main.worldSurface; y++)
                {
                    for (int x = (Corruption.X - Corruption.Size - 1) * biomes.scale; x <= (Corruption.X + Corruption.Size + 2) * biomes.scale; x++)
                    {
                        if (WGTools.Tile(x, y).WallType == (ushort)ModContent.WallType<Walls.dev.nothing>())
                        {
                            WGTools.Tile(x, y).WallType = 0;
                        }
                    }
                }
            }

            Corruption.CreateOrb(false);
            Corruption.CreateOrb(true);

            int structureCount = Main.maxTilesX / 210;
            while (structureCount > 0)
            {
                #region spawnconditions
                int x = WorldGen.genRand.Next((Corruption.X - Corruption.Size) * biomes.scale, (Corruption.X + Corruption.Size) * biomes.scale);
                int y = structureCount > Main.maxTilesX / 420 ? WorldGen.genRand.Next((int)Main.worldSurface, (int)(Main.maxTilesY - 300 - Main.worldSurface) / 2 + (int)Main.worldSurface) : WorldGen.genRand.Next((int)(Main.maxTilesY - 300 - Main.worldSurface) / 2 + (int)Main.worldSurface, Main.maxTilesY - 300);

                bool valid = true;

                if (WGTools.Tile(x, y).TileType == TileID.DemonAltar || Main.wallDungeon[WGTools.Tile(x, y).WallType])
                {
                    valid = false;
                }
                else if (biomes.FindBiome(x, y) != BiomeID.Corruption && biomes.FindBiome(x, y) != BiomeID.Crimson)
                {
                    valid = false;
                }
                else if (WGTools.Tile(x, y + 1).TileType != TileID.Ebonstone && WGTools.Tile(x, y + 1).TileType != TileID.Crimstone)
                {
                    valid = false;
                }
                #endregion

                if (valid)
                {
                    WorldGen.PlaceObject(x, y, TileID.DemonAltar, style: biomes.FindBiome(x, y) == BiomeID.Crimson ? 1 : 0);

                    if (WGTools.Tile(x, y).TileType == TileID.DemonAltar)
                    {
                        structureCount--;
                    }
                }
            }
            #endregion

            progress.Message = "Cleaning up ground";

            for (int y = 40; y < Main.worldSurface; y++)
            {
                progress.Set((y - 40) / (Main.worldSurface - 40));

                for (int x = 40; x < Main.maxTilesX - 40; x++)
                {
                    Tile tile = Main.tile[x, y];

                    if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand)
                    {
                        if (biomes.FindLayer(x, y) <= biomes.surfaceLayer)
                        {
                            if (tile.TileType == TileID.HardenedSand)
                            {
                                for (int i = 1; i <= WorldGen.genRand.Next(4, 7); i++)
                                {
                                    if (!WGTools.Tile(x, y - i).HasTile)
                                    {
                                        tile.TileType = TileID.Sand;
                                        break;
                                    }
                                }
                            }
                            else if (tile.TileType == TileID.Sand)
                            {
                                for (int i = 1; i <= WorldGen.genRand.Next(4, 7); i++)
                                {
                                    if (!WGTools.Tile(x, y + i).HasTile)
                                    {
                                        tile.TileType = TileID.HardenedSand;
                                        break;
                                    }
                                }
                            }

                            if (WGTools.Tile(x, y).WallType == WallID.DirtUnsafe || WGTools.SurroundingTilesActive(x, y))
                            {
                                tile.WallType = WallID.HardenedSand;
                            }
                        }
                    }
                }
            }

            for (int k = 0; k < 10; k++)
            {
                for (int y = 40; y < Main.worldSurface; y++)
                {
                    for (int x = (Desert.X - Desert.Size - 1) * biomes.scale; x <= (Desert.X + Desert.Size + 2) * biomes.scale; x++)
                    {
                        if (!WGTools.SurroundingTilesActive(x, y) && WGTools.Tile(x, y).WallType == WallID.HardenedSand)
                        {
                            int adjacentWalls = 0;

                            if (WGTools.Tile(x + 1, y).WallType != 0 || WGTools.Tile(x + 1, y).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x, y + 1).WallType != 0 || WGTools.Tile(x, y + 1).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x - 1, y).WallType != 0 || WGTools.Tile(x - 1, y).HasTile)
                            {
                                adjacentWalls++;
                            }
                            if (WGTools.Tile(x, y - 1).WallType != 0 || WGTools.Tile(x, y - 1).HasTile)
                            {
                                adjacentWalls++;
                            }

                            if (k == 9)
                            {
                                if (adjacentWalls < 3)
                                {
                                    WGTools.Tile(x, y).WallType = (ushort)ModContent.WallType<Walls.dev.nothing>();
                                }
                            }
                            else if (adjacentWalls < 4 && WorldGen.genRand.NextBool(4 / (4 - adjacentWalls)))
                            {
                                WGTools.Tile(x, y).WallType = (ushort)ModContent.WallType<Walls.dev.nothing>();
                            }
                        }
                    }
                }

                for (int y = 40; y < Main.worldSurface; y++)
                {
                    for (int x = (Desert.X - Desert.Size - 1) * biomes.scale; x <= (Desert.X + Desert.Size + 2) * biomes.scale; x++)
                    {
                        if (WGTools.Tile(x, y).WallType == (ushort)ModContent.WallType<Walls.dev.nothing>())
                        {
                            WGTools.Tile(x, y).WallType = 0;
                        }
                    }
                }
            }

            #region objects
            int area = GenVars.UndergroundDesertLocation.Width * GenVars.UndergroundDesertLocation.Height;

            int objects = area / 800;
            while (objects > 0)
            {
                int x = WorldGen.genRand.Next(GenVars.UndergroundDesertLocation.Left, GenVars.UndergroundDesertLocation.Right + 1);
                int y = WorldGen.genRand.Next((int)Main.worldSurface, GenVars.UndergroundDesertLocation.Bottom + 1);

                if (biomes.FindBiome(x, y) == BiomeID.Desert && WGTools.Tile(x, y + 1).HasTile && !WGTools.Tile(x + 1, y).HasTile && WGTools.Tile(x, y + 1).TileType == TileID.Sandstone)
                {
                    WorldGen.PlaceObject(x, y, TileID.AntlionLarva, style: Main.rand.Next(3));
                    if (Framing.GetTileSafely(x, y).TileType == TileID.AntlionLarva)
                    {
                        objects--;
                    }
                }
            }
            #endregion

            #region underworld
            progress.Message = "Creating the underworld";

            FastNoiseLite terrain = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            terrain.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            terrain.SetFrequency(0.03f);
            terrain.SetFractalType(FastNoiseLite.FractalType.FBm);
            terrain.SetFractalOctaves(3);
            //terrain.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
            //terrain.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);

            FastNoiseLite elevation = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            elevation.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            elevation.SetFrequency(0.01f);
            elevation.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite distance = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            distance.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            distance.SetFrequency(0.06f);
            distance.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite background = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            background.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            background.SetFrequency(0.01f);
            background.SetFractalType(FastNoiseLite.FractalType.Ridged);
            background.SetFractalWeightedStrength(-0.5f);
            background.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
            background.SetFractalOctaves(2);

            for (float y = Main.maxTilesY - 250; y < Main.maxTilesY - 21; y++)
            {
                progress.Set((y - (Main.maxTilesY - 200)) / 200);

                for (float x = 20; x < Main.maxTilesX - 20; x++)
                {
                    if (biomes.FindBiome(x, y) == BiomeID.Underworld || biomes.FindBiome(x, y) == BiomeID.AshForest)
                    {
                        Vector2 point = new Vector2(x, Main.maxTilesY - 150 + elevation.GetNoise(x, y) * 30 + WorldGen.genRand.Next(2));
                        float threshold;

                        if (y > point.Y)
                        {
                            threshold = MathHelper.Clamp(1 - Vector2.Distance(new Vector2(x, y), point) / (50 + distance.GetNoise(x, y) * 50), 0, 1);
                        }
                        else threshold = MathHelper.Clamp(1 - Vector2.Distance(new Vector2(x, y), point) / (50 + distance.GetNoise(x, y) * 50), 0, 1);

                        float _terrain = terrain.GetNoise(x, y * 2);
                        if (_terrain < threshold - 0.25f && WGTools.Tile(x, y).TileType != ModContent.TileType<Hardstone>())
                        {
                            WGTools.Tile(x, y).HasTile = false;
                            if (WGTools.Tile(x, y).WallType == WallID.ObsidianBackUnsafe)
                            {
                                WGTools.Tile(x, y).WallType = 0;
                            }
                            if (y <= point.Y - 40 && y >= point.Y - 50 || y > point.Y + 40 + distance.GetNoise(x, y) * 20)
                            {
                                WGTools.Tile(x, y).LiquidAmount = 255;
                            }
                            //float _background = background.GetNoise(x, y);
                        }

                        if (_terrain / 2 > threshold - 0.5f && WGTools.Tile(x, y).WallType == 0)
                        {
                            if (biomes.FindBiome(x, y) == BiomeID.AshForest)
                            {
                                WGTools.Tile(x, y).WallType = WallID.LavaUnsafe4;
                            }
                            else WGTools.Tile(x, y).WallType = WallID.LavaUnsafe3;
                        }

                        if (biomes.FindBiome(x, y) == BiomeID.AshForest && !WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                        {
                            if (WGTools.Tile(x - 1, y - 1).TileType == TileID.Ash)
                            {
                                WGTools.Tile(x - 1, y - 1).TileType = TileID.AshGrass;
                            }
                        }

                        //else if (y > point.Y)
                        //{
                        //    WGTools.GetTile(x, y).active(true);
                        //    //if (WGTools.GetTile(x, y).wall == 0 && WGTools.SurroundingTilesActive(x - 1, y - 1, true))
                        //    //{
                        //    //    WGTools.GetTile(x, y).wall = WallID.LavaUnsafe1;
                        //    //}
                        //}

                        //if (WGTools.Tile(x, y).WallType == WallID.ObsidianBackUnsafe && WGTools.Tile(x, y).HasTile == false && WorldGen.genRand.NextBool(2))
                        //{
                        //    int adjacentTiles = 0;
                        //    if (WGTools.Tile(x + 1, y).HasTile && WGTools.Tile(x + 1, y).TileType == TileID.Obsidian)
                        //    {
                        //        adjacentTiles++;
                        //    }
                        //    if (WGTools.Tile(x, y + 1).HasTile && WGTools.Tile(x, y + 1).TileType == TileID.Obsidian)
                        //    {
                        //        adjacentTiles++;
                        //    }
                        //    if (WGTools.Tile(x - 1, y).HasTile && WGTools.Tile(x - 1, y).TileType == TileID.Obsidian)
                        //    {
                        //        adjacentTiles++;
                        //    }
                        //    if (WGTools.Tile(x, y - 1).HasTile && WGTools.Tile(x, y - 1).TileType == TileID.Obsidian)
                        //    {
                        //        adjacentTiles++;
                        //    }

                        //    if (adjacentTiles >= 1)
                        //    {
                        //        WorldGen.PlaceTile((int)x, (int)y, TileID.ExposedGems);
                        //    }
                        //}
                    }

                    WGTools.Tile(x, y).LiquidType = 1;
                }
            }
            #endregion
        }

        public bool TundraLeft()
        {
            int tundraLeft = 0;
            int tundraRight = 0;

            for (int x = 1; x < Main.maxTilesX; x++)
            {
                if (Framing.GetTileSafely(x, (int)Main.worldSurface).TileType == TileID.SnowBlock || Framing.GetTileSafely(x, (int)Main.worldSurface).TileType == TileID.IceBlock)
                {
                    tundraLeft = x;
                    break;
                }
            }
            for (int x = Main.maxTilesX; x > 1; x--)
            {
                if (Framing.GetTileSafely(x, (int)Main.worldSurface).TileType == TileID.SnowBlock || Framing.GetTileSafely(x, (int)Main.worldSurface).TileType == TileID.IceBlock)
                {
                    tundraRight = x;
                    break;
                }
            }

            Tundra.X = (tundraLeft + tundraRight) / 2;

            return Tundra.X < Main.maxTilesX * 0.5 ? true : false;
        }
    }
    #endregion

    #region Secondary
    public class NewSecondaryBiomes : GenPass
    {
        public NewSecondaryBiomes(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        internal class Hive
        {
            public static int X;
            public static int Y;
            public static float Size;
        }

        internal class MarbleCave
        {
            public static int Y;
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();

            progress.Message = "Adding minibiomes";

            #region glowshroom
            FastNoiseLite glowshroom = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            glowshroom.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            glowshroom.SetFractalType(FastNoiseLite.FractalType.None);
            glowshroom.SetFrequency(0.1f);

            FastNoiseLite gemcaves = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            gemcaves.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            gemcaves.SetFractalType(FastNoiseLite.FractalType.FBm);
            gemcaves.SetFrequency(0.15f);
            gemcaves.SetFractalOctaves(1);

            Main.tileSolid[TileID.MushroomBlock] = true;
            #endregion

            #region marblecave
            int marbleCaveLeft = (int)(biomes.width * 0.4f);
            int marbleCaveRight = (int)(biomes.width * 0.6f);
            MarbleCave.Y = Math.Min(biomes.lavaLayer, biomes.height - 8 - Main.maxTilesY / 600);
            #endregion

            #region hive
            Main.tileSolid[TileID.Hive] = true;
            Main.tileSolid[TileID.BeeHive] = false;

            //if (Jungle.Center > Desert.X)
            //{
            //    //X = WorldGen.genRand.Next((int)(biomes.width * 0.85), (int)(biomes.width * 0.9));
            //    Hive.X = GenVars.UndergroundDesertLocation.Right / biomes.scale + (Jungle.Center);
            //}
            //else
            //{
            //    //X = WorldGen.genRand.Next((int)(biomes.width * 0.1), (int)(biomes.width * 0.15));
            //    Hive.X = GenVars.UndergroundDesertLocation.Left / biomes.scale + (Jungle.Center);
            //}
            //Hive.X /= 2;

            Hive.X = Jungle.Center;
            if (Jungle.Center > Desert.X)
            {
                Hive.X -= (int)(Jungle.Size * 0.75f);
            }
            else Hive.X += (int)(Jungle.Size * 0.75f);

            Hive.Size = biomes.width / 32;
            Hive.Y = (int)(Main.rockLayer / biomes.scale + Hive.Size);

            FastNoiseLite noise = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.2f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            for (int j = 1; j < biomes.height - 4; j++)
            {
                for (int i = 0; i < biomes.width; i++)
                {
                    if (noise.GetNoise(i, j) <= (1 - Vector2.Distance(new Vector2(Hive.X, Hive.Y), new Vector2(i, j)) / Hive.Size) * 2)
                    {
                        biomes.AddBiome(i, j, BiomeID.Hive);
                    }
                }
            }
            #endregion

            #region aether
            Main.tileSolid[TileID.ShimmerBlock] = true;

            GenVars.shimmerPosition.X = GenVars.dungeonSide == 1 ? Main.maxTilesX - 150 : 150;
            GenVars.shimmerPosition.Y = GenVars.lavaLine;
            #endregion

            FastNoiseLite growth = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            growth.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            growth.SetFrequency(0.005f);
            growth.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite flesh = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            flesh.SetNoiseType(FastNoiseLite.NoiseType.Value);
            flesh.SetFrequency(0.25f);

            FastNoiseLite meadows = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            meadows.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            meadows.SetFrequency(0.5f);
            meadows.SetFractalType(FastNoiseLite.FractalType.FBm);
            meadows.SetFractalOctaves(3);

            bool thorium = ModLoader.TryGetMod("ThoriumMod", out Mod mod) || ModLoader.TryGetMod("Aequus", out Mod mod2);

            for (int i = 1; i < biomes.width - 1; i++)
            {
                for (int j = 0; j < biomes.height - 5; j++)
                {
                    if (j >= biomes.surfaceLayer)
                    {
                        if (i <= 5 && i > 0 || i >= biomes.width - 6 && i < biomes.width - 1)
                        {
                            bool jungleSide = GenVars.dungeonSide == 1 && i <= 5 || GenVars.dungeonSide != 1 && i >= biomes.width - 6;
                            bool thoriumCompat = jungleSide && thorium;

                            if (!thoriumCompat && j < biomes.caveLayer - 1 && j > biomes.surfaceLayer)
                            {
                                biomes.AddBiome(i, j, BiomeID.OceanCave);
                            }
                            else if (jungleSide && j > biomes.caveLayer + (thorium ? 2 : 0) && j < biomes.height - 7)
                            {
                                biomes.AddBiome(i, j, BiomeID.Aether);
                            }
                        }
                        else if (i > 6 && i < biomes.width - 7)
                        {
                            if (i >= Tundra.X - 1 && i <= Tundra.X + 1)
                            {
                                biomes.AddBiome(i, j, BiomeID.Granite);
                            }
                            else if (j >= MarbleCave.Y - 1 && j <= MarbleCave.Y + 1 && i >= marbleCaveLeft && i <= marbleCaveRight)
                            {
                                biomes.AddBiome(i, j, BiomeID.Marble);
                            }
                            else if (biomes.biomeMap[i, j] == BiomeID.None)
                            {
                                if (j >= biomes.caveLayer && glowshroom.GetNoise(i, j * 2) < -0.95f && j < biomes.lavaLayer)
                                {
                                    biomes.AddBiome(i, j, BiomeID.Glowshroom);
                                }
                                else if (j >= biomes.caveLayer && gemcaves.GetNoise(i, j * 2) > 0.65f)
                                {
                                    biomes.AddBiome(i, j, BiomeID.GemCave);
                                }
                            }
                        }
                    }

                    if (GenVars.dungeonSide != 1 || !thorium)
                    {
                        biomes.AddBiome(1, biomes.surfaceLayer, BiomeID.OceanCave); biomes.AddBiome(1, biomes.surfaceLayer - 1, BiomeID.OceanCave);
                    }
                    if (GenVars.dungeonSide == 1 || !thorium)
                    {
                        biomes.AddBiome(biomes.width - 2, biomes.surfaceLayer, BiomeID.OceanCave); biomes.AddBiome(biomes.width - 2, biomes.surfaceLayer - 1, BiomeID.OceanCave);
                    }
                }
            }

            #region growth
            //int growthSize = biomes.width / 16;
            //int growthY = (int)(RemWorld.lavaLevel / biomes.scale) - growthSize / 2;
            //int growthX = Tundra.X;

            //for (int j = (int)Main.worldSurface / biomes.scale + 2; j < WorldGen.lavaLine / biomes.scale; j++)
            //{
            //    for (int i = 0; i < biomes.width; i++)
            //    {
            //        float num = Vector2.Distance(new Vector2(growthX, growthY), new Vector2(i + (j - growthY) / 2, j)) / growthSize;

            //        if ((growth.GetNoise(i * biomes.scale, j * biomes.scale) + 1) / 2 >= num)
            //        {
            //            biomes.AddBiome(i, j, "growth");
            //        }
            //    }
            //}
            #endregion

            biomes.UpdateMap(new int[] { BiomeID.Glowshroom, BiomeID.Marble, BiomeID.Granite, BiomeID.Aether, BiomeID.Hive, BiomeID.GemCave, BiomeID.OceanCave }, progress);

            progress.Message = "Carving marble";

            Vector2 point;
            float threshold;

            FastNoiseLite terrain = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            terrain.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            terrain.SetFrequency(0.02f);
            terrain.SetFractalType(FastNoiseLite.FractalType.FBm);
            terrain.SetFractalOctaves(4);

            for (int y = (int)Main.rockLayer; y < GenVars.lavaLine + 50; y++)
            {
                progress.Set((y - (Main.maxTilesY - 200)) / 200);

                for (int x = (int)(Main.maxTilesX * 0.35f) - 100; x < (int)(Main.maxTilesX * 0.65f) + 100; x++)
                {
                    if (biomes.FindBiome(x, y) == BiomeID.Marble)
                    {
                        point = new Vector2(MathHelper.Clamp(x, Main.maxTilesX * 0.4f + 50, Main.maxTilesX * 0.6f - 50), (MarbleCave.Y + 0.5f) * biomes.scale);

                        threshold = MathHelper.Clamp(1 - Vector2.Distance(new Vector2(x, y), point) / 80, 0, 1) * 2 - 1;

                        if (terrain.GetNoise(x * 3, y) * 2f < threshold)
                        {
                            WGTools.Tile(x, y).HasTile = false;

                            if (y > point.Y + 32)
                            {
                                WGTools.Tile(x, y).LiquidAmount = 153;
                            }
                            else WGTools.Tile(x, y).LiquidAmount = 0;
                        }
                        if (terrain.GetNoise(x * 3, y) * 3f + 0.35f < threshold)
                        {
                            WGTools.Tile(x, y).WallType = 0;
                        }
                    }
                }
            }

            progress.Message = "Carving granite";

            terrain = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            terrain.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            terrain.SetFrequency(0.03f);
            terrain.SetFractalType(FastNoiseLite.FractalType.FBm);
            terrain.SetFractalOctaves(2);

            for (int y = (int)Main.worldSurface - 25; y < Main.maxTilesY - 175; y++)
            {
                progress.Set((y - (Main.maxTilesY - 200)) / 200);

                for (int x = 400; x < Main.maxTilesX - 400; x++)
                {
                    if (biomes.FindBiome(x, y) == BiomeID.Granite)
                    {
                        point = new Vector2((Tundra.X + 0.5f) * biomes.scale, MathHelper.Clamp(y, (int)Main.worldSurface + 50, Main.maxTilesY - 350));
                        threshold = MathHelper.Clamp((1 - Vector2.Distance(new Vector2(x, y), point) / 75) * 1.5f, 0, 1);

                        float _terrain = terrain.GetNoise(x, y * 2) * 1.5f;

                        if (_terrain + 0.5f < threshold * 2 - 1)
                        {
                            WGTools.Tile(x, y).HasTile = false;

                            if (Vector2.Distance(new Vector2(x, y), point) > 35)
                            {
                                WGTools.Tile(x, y).LiquidAmount = 255;
                            }
                            else WGTools.Tile(x, y).LiquidAmount = 0;
                        }

                        point.Y = MathHelper.Clamp(y, (int)Main.worldSurface + 50, Main.maxTilesY - 450);
                        threshold = MathHelper.Clamp((1 - Vector2.Distance(new Vector2(x, y), point) / 75) * 1.5f, 0, 1);

                        if (_terrain + 1 < threshold * 2 - 1)
                        {
                            WGTools.Tile(x, y).WallType = 0;
                        }
                    }
                }
            }
        }
    }
    #endregion
}