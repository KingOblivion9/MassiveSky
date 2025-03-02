using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Linq;
using MassiveSky;
using Remnants;
using Remnants.Walls;
using Remnants.Walls.Parallax;
using static Remnants.Worldgen.NewPrimaryBiomes;
using static Remnants.Worldgen.BiomeMap;
using Remnants.Walls.Vanity;
using Remnants.Worldgen;
using StructureHelper;
using Terraria.DataStructures;
using Remnants.Tiles.Blocks;


namespace Remnants.Worldgen
{
    public class SkyWorld : ModSystem
    {
        public static Rectangle World => new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);

        public static int whisperingMazeY;
        public static int whisperingMazeX;
        public static bool sightedWard = false;

        public static int mushroomTiles;
        public static int hiveTiles;
        public static int marbleTiles;
        public static int graniteTiles;
        public static int pyramidTiles;
        public static int oceanCaveTiles;
        public static int gardenTiles;

        public Rectangle skyJungle;

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int genIndex;

            #region terrain
            InsertPass(tasks, new NewBiomeMapSetup("Biome Map Setup", 1), FindIndex(tasks, "[R] Biome Map Setup") + 1);
            InsertPass(tasks, new LargeTerrain("Terrain Improvement", 1), FindIndex(tasks, "[R] Terrain Improvement"), true);
            #endregion

            InsertPass(tasks, new NewMicrodungeons("Microdungeons", 0), FindIndex(tasks, "[R] Microdungeons"), true);
        }

        public static void InsertPass(List<GenPass> tasks, GenPass item, int index, bool replace = false)
        {
            if (replace)
            {
                RemovePass(tasks, index);
            }
            if (index != -1)
            {
                tasks.Insert(index, item);
            }

        }

        public static void RemovePass(List<GenPass> tasks, int index, bool destroy = false)
        {
            if (index != -1)
            {
                if (destroy)
                {
                    tasks.RemoveAt(index);
                }
                else tasks[index].Disable();
            }
        }

        public static int FindIndex(List<GenPass> tasks, string value)
        {
            return tasks.FindIndex(genpass => genpass.Name.Equals(value));
        }

        public static Tile Tile(int x, int y)
        {
            return Framing.GetTileSafely(x, y);
        }
    }

    public class LargeTerrain : GenPass
    {
        public LargeTerrain(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        private static float scaleX => Main.maxTilesX / 4200f;
        private static float scaleY => Main.maxTilesY / 1200f;

        public static int Minimum => (int)(Main.worldSurface - 60 - 150 * scaleY * ModContent.GetInstance<Client>().TerrainAmplitude);
        public static int Maximum => (int)(Main.worldSurface - 60);
        public static int Middle => (Minimum + Maximum) / 2;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();
            Configs RatioChange = Configs.Instance;
            int SkyChange = (int)RatioChange.RatioIncrease;

            if (RatioChange.AutoRatio)
            {
                double Auto = (double)(Main.maxTilesY * 0.15);
                Main.worldSurface += Auto;
                Main.rockLayer += Auto + 30;
            }
            else
            {
                Main.worldSurface += SkyChange;
                Main.rockLayer += SkyChange + 30;
            }

            #region terrain
            progress.Message = "Generating world terrain";

            FastNoiseLite altitude = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            altitude.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            altitude.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
            altitude.SetCellularJitter(0f);
            altitude.SetFractalType(FastNoiseLite.FractalType.FBm);
            altitude.SetFrequency(0.003f / scaleX);
            altitude.SetFractalOctaves(3);
            altitude.SetFractalLacunarity(3);
            altitude.SetFractalGain(0.75f);
            float[] altitudes = new float[Main.maxTilesX];
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                altitudes[i] = altitude.GetNoise(i, 0);// ((int)((altitude.GetNoise(i, 0) * (Maximum - Minimum)) / 12f) * 12) / (Maximum - Minimum);
            }

            FastNoiseLite roughness = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            roughness.SetNoiseType(FastNoiseLite.NoiseType.Value);
            roughness.SetFrequency(0.015f / scaleX);
            roughness.SetFractalType(FastNoiseLite.FractalType.FBm);
            roughness.SetFractalOctaves(5);

            for (float y = MathHelper.Clamp(Minimum - (Maximum - Minimum) / 2, 0, (int)Main.worldSurface * 0.05f); y <= Main.maxTilesY - 200; y++)
            {
                progress.Set(y / (Main.maxTilesY - 200));

                for (float x = 40; x < Main.maxTilesX - 40; x++)
                {
                    Tile tile = Main.tile[(int)x, (int)y];

                    if (y < Main.worldSurface)
                    {
                        float beachMultiplier = MathHelper.Clamp(Vector2.Distance(new Vector2(x, 0), new Vector2(MathHelper.Clamp(x, 0, 350), 0)) / (150 * scaleX), 0, 1) * MathHelper.Clamp(Vector2.Distance(new Vector2(x, 0), new Vector2(MathHelper.Clamp(x, Main.maxTilesX - 350, Main.maxTilesX), 0)) / (150 * scaleX), 0, 1);
                        float mountainX = Tundra.X * biomes.scale + biomes.scale / 2;
                        float mountainMultiplier = MathHelper.Clamp(MathHelper.Distance(x, mountainX) / (200 * scaleX), 0, 1);
                        float mountainMultiplier2 = MathHelper.Clamp((MathHelper.Distance(x, mountainX) / (300 * scaleX)), 0, 1);
                        float valleyX = Jungle.Center * biomes.scale + biomes.scale / 2;
                        float valleyMultiplier = MathHelper.Clamp((MathHelper.Distance(x, valleyX) / (100 * scaleX)) - 0.5f, 0, 1);
                        float valleyMultiplier2 = MathHelper.Clamp((MathHelper.Distance(x, valleyX) / (150 * scaleX)) - 0.5f, 0, 1);

                        float _altitude = 0;
                        for (int i = (int)(x - 10 * scaleX); i <= x + 10 * scaleX; i++)
                        {
                            _altitude += altitudes[(int)MathHelper.Clamp(i, 0, Main.maxTilesX - 1)];
                        }
                        _altitude /= 1 + 20 * scaleX;
                        _altitude -= 1f;
                        _altitude *= beachMultiplier;
                        _altitude *= 0.5f;
                        _altitude += 1f;

                        if (ModContent.GetInstance<Client>().IceMountain)
                        {
                            _altitude -= 1f;
                            _altitude *= mountainMultiplier2;
                            _altitude += 1f;

                            _altitude += 0.5f;
                            _altitude *= mountainMultiplier;
                            _altitude -= 0.5f;
                        }

                        if (ModContent.GetInstance<Client>().JungleValley)
                        {
                            _altitude -= 0.5f;
                            _altitude *= valleyMultiplier2;
                            _altitude += 0.5f;

                            _altitude -= 1f;
                            _altitude *= valleyMultiplier;
                            _altitude += 1f;
                        }

                        float _roughness = roughness.GetNoise(x, y * 2) / 0.8f;
                        _roughness *= 0.5f + beachMultiplier;
                        _roughness += 1f;
                        _roughness /= 2;
                        //_bumps *= (y - Minimum) / (Maximum - Minimum);

                        float _terrain = _altitude * 0.85f + _roughness * 0.2f;
                        _terrain -= 1 - 0.5f / ModContent.GetInstance<Client>().TerrainAmplitude;
                        _terrain *= MathHelper.Clamp(Vector2.Distance(new Vector2(x, 0), new Vector2(MathHelper.Clamp(x, Main.maxTilesX / 2 - 50 * scaleX, Main.maxTilesX / 2 + 50 * scaleX), 0)) / (100 * scaleX), 0.25f, 1);
                        _terrain += 1 - 0.5f / ModContent.GetInstance<Client>().TerrainAmplitude;

                        float threshold = _terrain * (Maximum - Minimum) + Minimum;
                        threshold -= (int)Main.worldSurface;
                        threshold *= MathHelper.Clamp(MathHelper.Distance(x, MathHelper.Clamp(x, 0, 125)) / 125, 0.15f, 1);
                        threshold *= MathHelper.Clamp(MathHelper.Distance(x, MathHelper.Clamp(x, Main.maxTilesX - 125, Main.maxTilesX)) / 125, 0.15f, 1);
                        threshold += (int)Main.worldSurface;

                        if (y >= threshold) // + Math.Sin((y * MathHelper.TwoPi) / 10) * 2
                        {
                            tile.HasTile = true;
                        }
                        else
                        {
                            tile.HasTile = false;
                            if (y >= (int)Main.worldSurface - 60)
                            {
                                tile.LiquidAmount = 255;
                            }
                        }
                    }
                    else tile.HasTile = true;

                    float blendLow = (int)Main.worldSurface;
                    float blendHigh = (int)Main.rockLayer;
                    float num = MathHelper.Clamp((y - blendLow) / (blendHigh - blendLow) * 2 - 1, -1, 1);

                    int layer = biomes.FindLayer((int)x, (int)y);

                    if (layer >= biomes.caveLayer && biomes.MaterialBlend(x, y, true) <= -0.3f)
                    {
                        tile.TileType = TileID.Silt;
                    }
                    else if (biomes.MaterialBlend(x, y, frequency: 2) <= (layer >= biomes.caveLayer ? 0.2f : -0.2f) || layer >= biomes.lavaLayer)
                    {
                        tile.TileType = TileID.Stone;
                    }
                    else if (layer < biomes.surfaceLayer && biomes.MaterialBlend(x, y, true) >= 0.2f)
                    {
                        tile.TileType = TileID.ClayBlock;
                    }
                    else if (layer >= biomes.surfaceLayer && layer < biomes.caveLayer && biomes.MaterialBlend(x, y, frequency: 2) >= 0.3f && y > Main.worldSurface + 5)
                    {
                        tile.TileType = TileID.Sand;
                    }
                    else tile.TileType = TileID.Dirt;

                    if (tile.TileType == TileID.Stone || tile.TileType == TileID.ClayBlock)
                    {
                        for (int i = 1; i <= WorldGen.genRand.Next(4, 7); i++)
                        {
                            if (!WGTools.Tile(x, y - i).HasTile)
                            {
                                tile.TileType = TileID.Dirt;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private void DoBlend(float x, float y, int type, int chance, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (WorldGen.genRand.NextBool(chance * 10))
                {
                    WorldGen.TileRunner((int)x + WorldGen.genRand.Next(-20, 21), (int)y + WorldGen.genRand.Next(-20, 21), WorldGen.genRand.Next(4, 25), 5, type, false, WorldGen.genRand.NextFloat(-10, 10), WorldGen.genRand.NextFloat(-10, 10));
                }
            }
        }
    }
    public class NewCaves : GenPass
    {
        public NewCaves(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Weathering caves";

            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();

            float scale = 1.25f;// * (2 / 1.5f);
            float heightMultiplier = 2;

            float transit1 = (int)Main.worldSurface - 60;
            float transit2 = (int)Main.rockLayer;

            FastNoiseLite caves = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            caves.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            caves.SetFrequency(0.01f / scale);
            caves.SetFractalType(FastNoiseLite.FractalType.FBm);
            caves.SetFractalOctaves(3);

            FastNoiseLite caveSize = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            caveSize.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            caveSize.SetFrequency(0.01f / scale);
            caveSize.SetFractalType(FastNoiseLite.FractalType.FBm);
            caveSize.SetFractalGain(0.7f);
            caveSize.SetFractalOctaves(4);

            FastNoiseLite caveOffset = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            caveOffset.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            caveOffset.SetFrequency(0.01f / scale);
            caveOffset.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite caveRoughness = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            caveRoughness.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            caveRoughness.SetFrequency(0.04f / scale);
            caveRoughness.SetFractalType(FastNoiseLite.FractalType.FBm);
            FastNoiseLite caveRoughness2 = new FastNoiseLite(WorldGen.genRand.Next(int.MinValue, int.MaxValue));
            caveRoughness2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            caveRoughness2.SetFrequency(0.01f / scale);
            caveRoughness2.SetFractalType(FastNoiseLite.FractalType.FBm);

            FastNoiseLite background = new FastNoiseLite();
            background.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            background.SetFrequency(0.04f);
            background.SetFractalType(FastNoiseLite.FractalType.FBm);
            background.SetFractalOctaves(2);
            background.SetFractalWeightedStrength(-1);
            background.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
            background.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);

            for (float y = 40; y < Main.maxTilesY - 100; y++)
            {
                progress.Set((y - ((int)Main.worldSurface - 40)) / (Main.maxTilesY - 200 - ((int)Main.worldSurface - 40)));

                //float divider = -((y - transit1) / (transit2 - transit1)) + 1;
                //float intensity = 0.6f;
                //if (divider < 1) {
                //    divider = 1;
                //}
                //if (divider > 1 + intensity) {
                //    divider = 1 + intensity;
                //}
                //divider = (divider - 1) * intensity + 1;
                for (float x = 325; x < Main.maxTilesX - 325; x++)
                {
                    if (WGTools.Tile(x, y).HasTile && biomes.FindLayer((int)x, (int)y) >= biomes.surfaceLayer)
                    {
                        //float transit = MathHelper.Clamp((y - transit1) / (transit2 - transit1), 0, 1);// / 2 + 0.5f;

                        float mult = 2;

                        float threshold = 1;
                        //threshold -= MathHelper.Clamp((1 - Vector2.Distance(new Vector2(x, y * 2.5f), new Vector2(1, ((int)Main.worldSurface - 85) * 2.5f)) / 500) * 3, 0, 1);
                        //threshold -= MathHelper.Clamp((1 - Vector2.Distance(new Vector2(x, y * 2.5f), new Vector2(Main.maxTilesX, ((int)Main.worldSurface - 85) * 2.5f)) / 500) * 3, 0, 1);

                        float _size = (caveSize.GetNoise(x * mult, y * mult * heightMultiplier) * 0.8f + 0.25f) * threshold;
                        float _caves = caves.GetNoise(x * mult, y * mult * heightMultiplier);
                        float _roughness = caveRoughness.GetNoise(x * mult, y * mult * heightMultiplier) * ((caveRoughness2.GetNoise(x * mult, y * mult * heightMultiplier) + 1) / 2);
                        float _offset = caveOffset.GetNoise(x * mult, y * mult * heightMultiplier);

                        float _overall = (_caves + _roughness / 3) / (1 + 1 / 2);

                        if (biomes.FindLayer((int)x, (int)y) < biomes.caveLayer)
                        {
                            if (_overall - _offset < -_size / 1 || _overall - _offset > _size / 1)
                            {
                                WGTools.Tile(x, y).HasTile = false;
                            }


                            if (background.GetNoise(x, y * 2) > -0.5f)// || WGTools.SurroundingTilesActive(x, y, true))
                            {
                                WGTools.Tile(x, y).WallType = WGTools.Tile(x, y).TileType == TileID.Stone ? WallID.RocksUnsafe1 : WallID.Cave6Unsafe;
                            }
                        }
                        else
                        {
                            if (_overall - _offset > -_size / 2 && _overall - _offset < _size / 2)
                            {
                                WGTools.Tile(x, y).HasTile = false;
                            }

                            if (biomes.FindLayer((int)x, (int)y) < biomes.height - 6)
                            {
                                if (_overall * 4 - _offset < -_size || _overall * 4 - _offset > _size)
                                {
                                    WGTools.Tile(x, y).WallType = WGTools.Tile(x, y).TileType == TileID.Dirt ? WallID.Cave6Unsafe : biomes.FindLayer((int)x, (int)y) >= biomes.lavaLayer ? WallID.Cave8Unsafe : WallID.RocksUnsafe1;
                                }
                            }
                        }


                        if (y > GenVars.lavaLine)
                        {
                            WGTools.Tile(x, y).LiquidType = LiquidID.Lava;
                        }
                    }
                }
            }

            progress.Message = "Flooding the caves";

            int structureCount = Main.maxTilesX * (Main.maxTilesY - 200 - (int)Main.rockLayer) / 50000;
            while (structureCount > 0)
            {
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 300);

                if (!WGTools.Tile(x, y).HasTile && WGTools.Tile(x, y).LiquidAmount != 255 && biomes.FindBiome(x, y) != BiomeID.Jungle)
                {
                    int radius = WorldGen.genRand.Next(25, 50);

                    for (int j = Math.Max(y - radius, (int)Main.rockLayer); j <= Math.Min(y + radius, Main.maxTilesY - 300); j++)
                    {
                        for (int i = x - radius; i <= x + radius; i++)
                        {
                            if (Vector2.Distance(new Vector2(i, j), new Vector2(x, y)) < radius && WorldGen.InWorld(i, j))
                            {
                                WGTools.Tile(i, j).LiquidAmount = 255;
                            }
                        }
                    }

                    structureCount--;
                }
            }
        }
    }

    public class Ores : GenPass
    {
        public Ores(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Scattering minerals";

            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();

            int[] blocksToReplace = new int[] { TileID.Dirt, TileID.Grass, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Stone, TileID.Ebonstone, TileID.Crimstone, TileID.SnowBlock, TileID.IceBlock, TileID.Mud, TileID.JungleGrass, TileID.MushroomGrass, TileID.Sand, TileID.HardenedSand, TileID.Sandstone };

            int rarity;
            int type;

            for (int y = 40; y < Main.maxTilesY - 40; y++)
            {
                progress.Set(((float)y - 40) / (Main.maxTilesY - 40 - 40));

                for (int x = 40; x < Main.maxTilesX - 40; x++)
                {
                    Tile tile = Main.tile[x, y];

                    if (biomes.FindBiome(x, y, false) != BiomeID.Hive)
                    {
                        float radius = 3;
                        while (radius < 15)
                        {
                            if (WorldGen.genRand.NextBool(3))
                            {
                                break;
                            }
                            else radius++;
                        }

                        if (WorldGen.SolidTile3(x, y) && !Main.wallDungeon[WGTools.Tile(x, y).WallType] && WGTools.Tile(x, y).WallType != ModContent.WallType<GardenBrickWall>() && WGTools.Tile(x, y).WallType != ModContent.WallType<undergrowth>() && WGTools.Tile(x, y).WallType != WallID.LivingWoodUnsafe && WGTools.Tile(x, y).WallType != ModContent.WallType<forgottentomb>() && WGTools.Tile(x, y).WallType != ModContent.WallType<TombBrickWallUnsafe>() && biomes.FindBiome(x, y) != BiomeID.GemCave)
                        {
                            if (y > Main.worldSurface * 0.5f)
                            {
                                if (biomes.FindBiome(x, y) == BiomeID.Underworld || biomes.FindBiome(x, y) == BiomeID.AshForest)
                                {
                                    if (y >= Main.maxTilesY - 140)
                                    {
                                        rarity = 10;
                                        OreVein(x, y, WorldGen.genRand.Next(16, 24), rarity, TileID.Hellstone, new int[] { TileID.Ash, TileID.AshGrass, TileID.Obsidian }, 5, 0.55f, 6, 3);
                                    }
                                }
                                else if (x >= 350 && x <= Main.maxTilesX - 350)
                                {
                                    if (y < Main.worldSurface && biomes.FindBiome(x, y) == BiomeID.Desert)
                                    {
                                        rarity = 50;

                                        AddFossil(x, y, rarity);
                                    }
                                    if (y > Main.worldSurface && (biomes.FindBiome(x, y) == BiomeID.Corruption || biomes.FindBiome(x, y) == BiomeID.Crimson))
                                    {
                                        rarity = 5;

                                        type = biomes.FindBiome(x, y) == BiomeID.Crimson ? TileID.Crimtane : TileID.Demonite;

                                        OreVein(x, y, WorldGen.genRand.Next(12, 18), rarity, type, blocksToReplace, 5, 0.55f, 6, 3);
                                    }
                                    else
                                    {
                                        #region tin/copper
                                        if (y < GenVars.lavaLine)
                                        {
                                            rarity = y > Main.worldSurface && y < Main.rockLayer ? 10 : 20;

                                            type = biomes.FindBiome(x, y, false) == BiomeID.Jungle || biomes.FindBiome(x, y, false) == BiomeID.Desert ? TileID.Tin : TileID.Copper;

                                            OreVein(x, y, WorldGen.genRand.Next(12, 18), rarity, type, blocksToReplace, 3, 0.5f, 5, 3);
                                        }
                                        #endregion

                                        #region iron/lead
                                        if (y > Main.worldSurface)
                                        {
                                            rarity = y > Main.rockLayer && y < GenVars.lavaLine ? 20 : 40;

                                            type = biomes.FindBiome(x, y, false) == BiomeID.Jungle || biomes.FindBiome(x, y, false) == BiomeID.Desert ? TileID.Lead : TileID.Iron;

                                            OreVein(x, y, WorldGen.genRand.Next(16, 24), rarity, type, blocksToReplace, 3, 0.5f, 5, 3);
                                        }
                                        #endregion

                                        #region silver/tungsten
                                        if (y > Main.rockLayer)
                                        {
                                            rarity = y > GenVars.lavaLine ? 20 : 40;

                                            type = biomes.FindBiome(x, y) == BiomeID.Jungle || biomes.FindBiome(x, y, false) == BiomeID.Desert ? TileID.Tungsten : TileID.Silver;

                                            OreVein(x, y, WorldGen.genRand.Next(12, 18), rarity, type, blocksToReplace, 3, 0.5f, 5, 3);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                #region gold/platinum
                                if (biomes.FindBiome(x, y) != BiomeID.Tundra)
                                {
                                    rarity = 5;

                                    type = biomes.FindBiome(x, y) == BiomeID.Jungle || biomes.FindBiome(x, y, false) == BiomeID.Desert ? TileID.Platinum : TileID.Gold;

                                    OreVein(x, y, WorldGen.genRand.Next(12, 18), rarity, type, blocksToReplace, 3, 0.5f, 5, 3);
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
        }

        private void OreVein(int structureX, int structureY, int size, float rarity, int type, int[] blocksToReplace, int steps, float weight = 0.5f, int birthLimit = 4, int deathLimit = 4)
        {
            if (ModContent.GetInstance<Client>().OreFrequency == 0) { return; }

            rarity /= ModContent.GetInstance<Client>().OreFrequency;
            if (WorldGen.genRand.NextBool((int)(rarity * 100)) && !Main.wallDungeon[WGTools.Tile(structureX, structureY).WallType])
            {
                int width = size;
                int height = size;

                structureX -= width / 2;
                structureY -= height / 2;

                bool[,] cellMap = new bool[width, height];
                bool[,] cellMapNew = cellMap;

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        if (WorldGen.genRand.NextFloat(0, 1) <= weight)
                        {
                            cellMap[x, y] = true;
                        }
                        else cellMap[x, y] = false;
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    if (WGTools.Tile(structureX, structureY + y).HasTile)
                    {
                        cellMap[0, y] = false;
                    }
                    if (WGTools.Tile(structureX + width - 1, structureY + y).HasTile)
                    {
                        cellMap[width - 1, y] = false;
                    }
                }
                for (int x = 0; x < width; x++)
                {
                    if (WGTools.Tile(structureX + x, structureY).HasTile)
                    {
                        cellMap[x, 0] = false;
                    }
                    if (WGTools.Tile(structureX + x, structureY + height - 1).HasTile)
                    {
                        cellMap[x, height - 1] = false;
                    }
                }

                cellMapNew = cellMap;

                for (int i = 0; i < steps; i++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            bool left = cellMap[x - 1, y];
                            bool right = cellMap[x + 1, y];
                            bool top = cellMap[x, y - 1];
                            bool bottom = cellMap[x, y + 1];
                            bool topleft = cellMap[x - 1, y - 1];
                            bool topright = cellMap[x + 1, y - 1];
                            bool bottomleft = cellMap[x - 1, y + 1];
                            bool bottomright = cellMap[x + 1, y + 1];
                            int adjacentTiles = 0;
                            if (left) { adjacentTiles++; }
                            if (right) { adjacentTiles++; }
                            if (top) { adjacentTiles++; }
                            if (bottom) { adjacentTiles++; }
                            if (topleft) { adjacentTiles++; }
                            if (topright) { adjacentTiles++; }
                            if (bottomleft) { adjacentTiles++; }
                            if (bottomright) { adjacentTiles++; }

                            if (cellMap[x, y] == false && adjacentTiles > birthLimit)
                            {
                                cellMapNew[x, y] = true;
                            }
                            if (cellMap[x, y] && adjacentTiles < deathLimit)
                            {
                                cellMapNew[x, y] = false;
                            }
                        }
                    }
                    cellMap = cellMapNew;
                }

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        if (cellMap[x, y])
                        {
                            Tile tile = WGTools.Tile(x + structureX, y + structureY);
                            if (tile.HasTile && blocksToReplace.Contains(tile.TileType) && (WGTools.Solid(x + structureX, y + structureY - 1) || structureY > Main.worldSurface))
                            {
                                tile.TileType = (ushort)type;
                            }
                        }
                    }
                }
            }
        }

        private void AddFossil(int x, int y, float rarity)
        {
            if (ModContent.GetInstance<Client>().OreFrequency == 0) { return; }

            rarity /= ModContent.GetInstance<Client>().OreFrequency;
            if (WorldGen.genRand.NextBool((int)(rarity * 100)))
            {
                int lifetime = WorldGen.genRand.Next(12, 25);

                Vector2 position = new Vector2(x, y);
                Vector2 velocity = WorldGen.genRand.NextVector2Circular(1, 1);

                float cooldown = 0;

                while (lifetime > 0)
                {
                    Tile tile = WGTools.Tile(position.X, position.Y);
                    if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand)
                    {
                        WGTools.Tile(position.X, position.Y).TileType = TileID.FossilOre;
                    }

                    position += new Vector2(velocity.X, velocity.Y);

                    velocity += Main.rand.NextVector2Circular(10f, 10f) * 0.1f;
                    if (velocity.Length() > 1)
                    {
                        velocity = Vector2.Normalize(velocity) * 1;
                    }

                    cooldown -= velocity.Length();
                    if (cooldown <= 0)
                    {
                        FossilRib(position, Vector2.Normalize(velocity).RotatedBy(-MathHelper.PiOver2));
                        FossilRib(position, Vector2.Normalize(velocity).RotatedBy(MathHelper.PiOver2));
                        cooldown += 3;
                    }

                    lifetime--;
                }
            }
        }

        private void FossilRib(Vector2 position, Vector2 velocity)
        {
            int lifetime = WorldGen.genRand.Next(3, 6);

            while (lifetime > 0)
            {
                Tile tile = WGTools.Tile(position.X, position.Y);
                if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand)
                {
                    WGTools.Tile(position.X, position.Y).TileType = TileID.FossilOre;
                }

                position += new Vector2(velocity.X, velocity.Y);


                lifetime--;
            }
        }
    }
}