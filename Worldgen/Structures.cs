using Microsoft.Xna.Framework;
using Remnants.Items.Accessories;
using Remnants.Items.Tools;
using Remnants.Items.Weapons;
using Remnants.Tiles;
using Remnants.Tiles.Blocks;
using Remnants.Tiles.Objects;
using Remnants.Tiles.Objects.Decoration;
using Remnants.Tiles.Objects.Furniture;
using Remnants.Tiles.Objects.Hazards;
using Remnants.Tiles.Plants;
using Remnants.Walls;
using Remnants.Walls.Vanity;
using Remnants.Walls.Parallax;
using Remnants.Walls.dev;
using StructureHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Remnants.Worldgen.NewPrimaryBiomes;
using static Remnants.Worldgen.NewSecondaryBiomes;
using MassiveSky;

namespace Remnants.Worldgen
{
    public class NewMicrodungeons : GenPass
    {
        public NewMicrodungeons(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            Configs Mine = Configs.Instance;
            BiomeMap biomes = ModContent.GetInstance<BiomeMap>();

            progress.Message = "Building microdungeons";
            int structureCount;

            int uniqueStructures = 6;
            int progressCounter = 0;

            structureCount = 0; // MARBLE BATHHOUSE
            while (structureCount < (Main.maxTilesX) / 1050)
            {
                progress.Set((progressCounter + (structureCount / (float)(Main.maxTilesX / 1050))) / (float)uniqueStructures);

                #region spawnconditions
                Structures.Dungeon temple = new Structures.Dungeon(0, 0, 3, structureCount == 0 ? 4 : structureCount == 1 ? 2 : Math.Max(WorldGen.genRand.Next(2, 5), WorldGen.genRand.Next(2, 4)), 18, 12, 3);
                temple.X = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.425f), (int)(Main.maxTilesX * 0.575f) - temple.area.Width);
                temple.Y = (MarbleCave.Y + 1) * biomes.scale - temple.area.Height;

                bool[] validTiles = TileID.Sets.Factory.CreateBoolSet(false, TileID.Marble, TileID.Obsidian);

                bool valid = true;
                if (!GenVars.structures.CanPlace(temple.area, validTiles, 10))
                {
                    valid = false;
                }
                //else if (!Structures.InsideBiome(temple.area, BiomeID.Marble))
                //{
                //    valid = false;
                //}
                #endregion

                if (valid)
                {
                    GenVars.structures.AddProtectedStructure(temple.area, 10);

                    for (int j = 0; j < 20; j += 3)
                    {
                        WorldGen.TileRunner(temple.area.Left - 7, temple.area.Bottom + 2 + j, WorldGen.genRand.Next(2, 7) * 2 + j / 2, 1, TileID.Marble, true, 0, 0, false, false);
                        WorldGen.TileRunner(temple.area.Right + 7, temple.area.Bottom + 2 + j, WorldGen.genRand.Next(2, 7) * 2 + j / 2, 1, TileID.Marble, true, 0, 0, false, false);
                    }

                    #region structure
                    bool[] marble = TileID.Sets.Factory.CreateBoolSet(false, TileID.Marble);
                    WGTools.Terraform(new Vector2(temple.area.Left - 5, temple.area.Bottom - 3), 5, marble);
                    WGTools.Terraform(new Vector2(temple.area.Right + 5, temple.area.Bottom - 3), 5, marble);

                    #region rooms
                    int roomCount;

                    for (int i = 0; i < temple.grid.Width; i++)
                    {
                        for (int j = 1; j < temple.grid.Height; j++)
                        {
                            temple.AddMarker(i, j);
                        }
                    }
                    int width = structureCount == 2 ? 2 : WorldGen.genRand.Next(1, temple.grid.Width + 1);
                    int pos = WorldGen.genRand.Next(0, temple.grid.Width - width);
                    for (int i = pos; i < pos + width; i++)
                    {
                        temple.AddMarker(i, 0);
                    }

                    //temple.AddMarker(0, 0, 1);
                    //roomCount = (temple.grid.Height - 1) * temple.grid.Width / 2;
                    //while (roomCount > 0)
                    //{
                    //    temple.targetCell.X = WorldGen.genRand.Next(0, temple.grid.Width);
                    //    temple.targetCell.Y = WorldGen.genRand.Next(0, temple.grid.Height);
                    //    if (roomCount < temple.grid.Height)
                    //    {
                    //        temple.targetCell.Y = roomCount;
                    //    }

                    //    if (!temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1))
                    //    {
                    //        temple.AddMarker(temple.targetCell.X, temple.targetCell.Y, 1);

                    //        roomCount--;
                    //    }
                    //}

                    //roomCount = temple.grid.Height * temple.grid.Width / 8;
                    //while (roomCount > 0)
                    //{
                    //    temple.targetCell.X = WorldGen.genRand.Next(0, temple.grid.Width - 1);
                    //    temple.targetCell.Y = WorldGen.genRand.Next(0, temple.grid.Height);

                    //    if (!temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && !temple.FindMarker(temple.targetCell.X + 1, temple.targetCell.Y + 1))
                    //    {
                    //        temple.AddMarker(temple.targetCell.X, temple.targetCell.Y); temple.AddMarker(temple.targetCell.X, temple.targetCell.Y + 1);
                    //        temple.AddMarker(temple.targetCell.X, temple.targetCell.Y, 2);

                    //        if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1))
                    //        {
                    //            temple.AddMarker(temple.targetCell.X, temple.targetCell.Y + 1, 1);
                    //        }

                    //        Generator.GenerateMultistructureRandom("Structures/common/thermalrig/solid", temple.roomPos, ModContent.GetInstance<Remnants>());

                    //        roomCount--;
                    //    }
                    //}

                    roomCount = 0;
                    while (roomCount < temple.grid.Height - 1)
                    {
                        temple.targetCell.X = WorldGen.genRand.Next(0, temple.grid.Width);
                        temple.targetCell.Y = roomCount + 1;

                        if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && temple.FindMarker(temple.targetCell.X, temple.targetCell.Y - 1))
                        {
                            temple.AddMarker(temple.targetCell.X, temple.targetCell.Y, 1);

                            roomCount++;
                        }
                    }

                    #endregion

                    for (temple.targetCell.Y = temple.grid.Height - 1; temple.targetCell.Y >= 0; temple.targetCell.Y--)
                    {
                        while (true)
                        {
                            temple.targetCell.X = WorldGen.genRand.Next(0, temple.grid.Width);

                            if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && !temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1))
                            {
                                Generator.GenerateMultistructureSpecific("Structures/common/MarbleBathhouse/room", temple.roomPos, ModContent.GetInstance<Remnants>(), 2);
                                temple.AddMarker(temple.targetCell.X, temple.targetCell.Y, 2);

                                int chestIndex = WorldGen.PlaceChest(temple.roomPos.X + 9, temple.roomPos.Y + temple.room.Height - 1, TileID.Dressers, style: 27);

                                var itemsToAdd = new List<(int type, int stack)>();

                                Structures.GenericLoot(chestIndex, itemsToAdd);

                                Structures.FillChest(chestIndex, itemsToAdd);

                                break;
                            }
                        }

                        for (temple.targetCell.X = 0; temple.targetCell.X < temple.grid.Width; temple.targetCell.X++)
                        {
                            if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && !temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1) && !temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 2))
                            {
                                Generator.GenerateMultistructureSpecific("Structures/common/MarbleBathhouse/room", temple.roomPos, ModContent.GetInstance<Remnants>(), WorldGen.genRand.Next(2));
                            }

                            if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && (temple.targetCell.X == 0 || !temple.FindMarker(temple.targetCell.X - 1, temple.targetCell.Y)))
                            {
                                Generator.GenerateStructure("Structures/common/MarbleBathhouse/left", new Point16(temple.roomPos.X - 5, temple.roomPos.Y), ModContent.GetInstance<Remnants>());
                                WorldGen.PlaceTile(temple.roomPos.X - 6, temple.roomPos.Y, TileID.Platforms, style: 29);
                            }
                            if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y) && (temple.targetCell.X == temple.grid.Width - 1 || !temple.FindMarker(temple.targetCell.X + 1, temple.targetCell.Y)))
                            {
                                Generator.GenerateStructure("Structures/common/MarbleBathhouse/right", new Point16(temple.roomPos.X + temple.room.Width, temple.roomPos.Y), ModContent.GetInstance<Remnants>());
                                WorldGen.PlaceTile(temple.roomPos.X + temple.room.Width + 6, temple.roomPos.Y, TileID.Platforms, style: 29);
                            }

                            if (temple.targetCell.Y == temple.grid.Height - 1)
                            {
                                if (temple.targetCell.X == 0)
                                {
                                    WorldGen.PlaceTile(temple.roomPos.X - 6, temple.roomPos.Y + temple.room.Height, TileID.Platforms, style: 29);
                                }
                                if (temple.targetCell.X == temple.grid.Width - 1)
                                {
                                    WorldGen.PlaceTile(temple.roomPos.X + temple.room.Width + 6, temple.roomPos.Y + temple.room.Height, TileID.Platforms, style: 29);
                                }
                            }
                        }
                    }

                    for (temple.targetCell.Y = 0; temple.targetCell.Y < temple.grid.Height; temple.targetCell.Y++)
                    {
                        for (temple.targetCell.X = 0; temple.targetCell.X < temple.grid.Width; temple.targetCell.X++)
                        {
                            if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y))
                            {
                                if (temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1))
                                {
                                    Generator.GenerateStructure("Structures/common/MarbleBathhouse/ladder", temple.roomPos, ModContent.GetInstance<Remnants>());

                                    //if (temple.targetCell.Y < temple.grid.Height - 1 && temple.targetCell.X == 0)
                                    //{
                                    //    WGTools.Rectangle(temple.roomPos.X - 2, temple.roomPos.Y + 7, temple.roomPos.X, temple.roomPos.Y + 11, TileID.MarbleBlock);
                                    //    WGTools.Rectangle(temple.roomPos.X - 1, temple.roomPos.Y + 6, temple.roomPos.X - 1, temple.roomPos.Y + 12, TileID.Marble);
                                    //}
                                    //else if (temple.targetCell.Y < temple.grid.Height - 1 && temple.targetCell.X == temple.grid.Width - 1)
                                    //{
                                    //    WGTools.Rectangle(temple.roomPos.X + temple.room.Width, temple.roomPos.Y + 7, temple.roomPos.X + temple.room.Width + 2, temple.roomPos.Y + 11, TileID.MarbleBlock);
                                    //    WGTools.Rectangle(temple.roomPos.X + temple.room.Width + 1, temple.roomPos.Y + 6, temple.roomPos.X + temple.room.Width + 1, temple.roomPos.Y + 12, TileID.Marble);
                                    //}
                                }

                                if (temple.targetCell.X > 0 && temple.FindMarker(temple.targetCell.X - 1, temple.targetCell.Y) && !temple.FindMarker(temple.targetCell.X - 1, temple.targetCell.Y, 1) && !temple.FindMarker(temple.targetCell.X, temple.targetCell.Y, 1))
                                {
                                    WGTools.Rectangle(temple.roomPos.X - 2, temple.roomPos.Y + 4, temple.roomPos.X + 2, temple.roomPos.Y + temple.room.Height - 1, -1, WallID.MarbleBlock);

                                    WGTools.Rectangle(temple.roomPos.X - 2, temple.roomPos.Y + 3, temple.roomPos.X + 2, temple.roomPos.Y + 3, TileID.MarbleBlock);
                                    WGTools.Rectangle(temple.roomPos.X - 2, temple.roomPos.Y + temple.room.Height, temple.roomPos.X + 2, temple.roomPos.Y + temple.room.Height, TileID.MarbleBlock);

                                    WorldGen.PlaceObject(temple.roomPos.X, temple.roomPos.Y + 11, TileID.Pianos, style: 29);
                                    //WorldGen.PlaceObject(temple.roomPos.X, temple.roomPos.Y + 8, TileID.Painting3X3, style: 45);

                                    PlacePainting(temple.roomPos.X, temple.roomPos.Y + 7);
                                }
                            }
                        }
                    }

                    #region cleanup

                    for (int y = temple.area.Top - 4; y <= temple.area.Bottom + 50; y++)
                    {
                        for (int x = temple.area.Left - 6; x <= temple.area.Right + 6; x++)
                        {
                            Tile tile = Main.tile[x, y];

                            if (tile.WallType == ModContent.WallType<WoodLattice>())
                            {
                                tile.WallType = 0;
                                Main.tile[x, y - 1].WallType = 0;
                            }

                            if (y > temple.area.Bottom && biomes.FindBiome(x, y) == BiomeID.Marble)
                            {
                                if (!tile.HasTile)
                                {
                                    WorldGen.PlaceTile(x, y, TileID.Marble);
                                    tile.LiquidAmount = 0;
                                }
                            }
                        }
                    }

                    Structures.AddVariation(temple.area);
                    Structures.AddDecorations(temple.area);
                    #endregion

                    #region objects
                    int objects;

                    objects = 1;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(temple.area.Left, temple.area.Right);
                        int y = WorldGen.genRand.Next(temple.area.Top, temple.area.Bottom);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.Containers && WGTools.Tile(x, y + 1).TileType != TileID.Platforms && WGTools.Tile(x + 1, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y, 2))
                        {
                            int chestIndex = WorldGen.PlaceChest(x, y, style: 51, notNearOtherChests: true);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.Containers)
                            {
                                #region chestloot
                                var itemsToAdd = new List<(int type, int stack)>();

                                itemsToAdd.Add((structureCount % 2 == 0 ? ItemID.HermesBoots : ItemID.AncientChisel, 1));

                                Structures.GenericLoot(chestIndex, itemsToAdd, 2, new int[] { ItemID.TitanPotion });

                                Structures.FillChest(chestIndex, itemsToAdd);
                                #endregion

                                objects--;
                            }
                        }
                    }

                    //objects = temple.grid.Height * temple.grid.Width;
                    //while (objects > 0)
                    //{
                    //    int x = WorldGen.genRand.Next(temple.area.Left, temple.area.Right);
                    //    int y = WorldGen.genRand.Next(temple.area.Top, temple.area.Bottom + 2);

                    //    if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                    //    {
                    //        WorldGen.PlaceSmallPile(x, y, Main.rand.Next(6), 0);
                    //        if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                    //        {
                    //            objects--;
                    //        }
                    //    }
                    //}
                    //objects = temple.grid.Height * temple.grid.Width;
                    //while (objects > 0)
                    //{
                    //    int x = WorldGen.genRand.Next(temple.area.Left, temple.area.Right);
                    //    int y = WorldGen.genRand.Next(temple.area.Top, temple.area.Bottom + 2);

                    //    if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                    //    {
                    //        WorldGen.PlaceSmallPile(x, y, Main.rand.Next(28, 35), 0);
                    //        if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                    //        {
                    //            objects--;
                    //        }
                    //    }
                    //}
                    #endregion
                    #endregion

                    structureCount++;
                }
            }

            if (!Configs.Instance.DisableMinecarts)
            {
                progressCounter++;

                structureCount = 0; // MINECART RAIL
                while (structureCount < Main.maxTilesY / 150)
                {
                    progress.Set((progressCounter + (structureCount / (float)(Main.maxTilesY / 120))) / (float)uniqueStructures);

                    #region spawnconditions
                    Structures.Dungeon rail = new Structures.Dungeon(0, WorldGen.genRand.Next((int)Main.rockLayer, GenVars.lavaLine - 50), WorldGen.genRand.Next(15, 30) * (Main.maxTilesX / 4200), 2, 12, 6, 2);
                    rail.X = WorldGen.genRand.Next(400, Main.maxTilesX - 400 - rail.area.Width);// (structureCount < Main.maxTilesY / 240 ^ Tundra.X > biomes.width / 2) ? WorldGen.genRand.Next(400, Main.maxTilesX / 2 - rail.area.Width / 2) : WorldGen.genRand.Next(Main.maxTilesX / 2 - rail.area.Width / 2, Main.maxTilesX - 400 - rail.area.Width);
                    rail.X = (int)(rail.X / 4) * 4;

                    bool[] invalidTiles = TileID.Sets.Factory.CreateBoolSet(true, TileID.Ash, TileID.Ebonstone, TileID.Crimstone, TileID.LihzahrdBrick, TileID.LivingWood);

                    bool valid = true;
                    if (!GenVars.structures.CanPlace(rail.area, invalidTiles, 25))
                    {
                        valid = false;
                    }
                    //else if (Structures.AvoidsBiomes(rail.area, new int[] { BiomeID.Granite }) && structureCount < Main.maxTilesY / 600f || !Structures.AvoidsBiomes(rail.area, new int[] { BiomeID.Granite }) && structureCount >= Main.maxTilesY / 600f)
                    //{
                    //    valid = false;
                    //}
                    else if (!Structures.AvoidsBiomes(rail.area, new int[] { BiomeID.Tundra, BiomeID.Desert, BiomeID.Marble, BiomeID.Hive, BiomeID.GemCave, BiomeID.Toxic }))
                    {
                        valid = false;
                    }
                    else if (!Structures.AvoidsBiomes(rail.area, new int[] { BiomeID.Jungle }) ^ structureCount % 5 == 0)
                    {
                        valid = false;
                    }
                    else
                    {
                        for (int i = rail.area.Left; i <= rail.area.Right; i++)
                        {
                            for (int j = rail.area.Y - 100; j <= rail.area.Y + 100; j++)
                            {
                                if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.MinecartTrack)
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (!valid)
                            {
                                break;
                            }
                        }
                    }
                    #endregion

                    if (valid)
                    {
                        GenVars.structures.AddProtectedStructure(rail.area, 10);

                        #region structure
                        #region rooms

                        for (int i = rail.area.Left; i <= rail.area.Right; i++)
                        {
                            for (int j = rail.area.Y - 5; j <= rail.area.Y - 1; j++)
                            {
                                if (!TileID.Sets.IsBeam[Main.tile[i, j].TileType])
                                {
                                    WorldGen.KillTile(i, j);
                                }
                            }

                            WGTools.Terraform(new Vector2(i, rail.area.Y - 3), 5);
                            WorldGen.PlaceTile(i, rail.area.Y - 1, TileID.MinecartTrack);
                            WorldGen.TileFrame(i, rail.area.Y - 1);
                        }

                        bool hasStation = Structures.AvoidsBiomes(rail.area, new int[] { BiomeID.Desert, BiomeID.Granite });
                        int stationWidth = 2;
                        int stationX = WorldGen.genRand.Next(1, rail.grid.Width - stationWidth);
                        if (hasStation)
                        {
                            int ladderX = WorldGen.genRand.Next(stationX, stationX + stationWidth);

                            rail.targetCell.X = stationX;
                            rail.targetCell.Y = 0;

                            WGTools.Terraform(new Vector2(rail.room.Left, rail.room.Bottom - 3), 5);
                            WGTools.Terraform(new Vector2(rail.room.Right + rail.room.Width, rail.room.Bottom - 3), 5);

                            for (rail.targetCell.X = stationX; rail.targetCell.X < stationX + stationWidth; rail.targetCell.X++)
                            {
                                rail.AddMarker(rail.targetCell.X, 0);

                                if (rail.targetCell.X == ladderX)
                                {
                                    Generator.GenerateStructure("Structures/common/MinecartRail/ladder", new Point16(rail.roomPos.X, rail.roomPos.Y), ModContent.GetInstance<Remnants>());
                                }
                                else Generator.GenerateStructure("Structures/common/MinecartRail/room", new Point16(rail.roomPos.X, rail.roomPos.Y), ModContent.GetInstance<Remnants>());
                            }

                            rail.targetCell.X = stationX;
                            Generator.GenerateStructure("Structures/common/MinecartRail/wall", new Point16(rail.roomPos.X, rail.roomPos.Y), ModContent.GetInstance<Remnants>());
                            rail.targetCell.X = stationX + stationWidth;
                            Generator.GenerateStructure("Structures/common/MinecartRail/wall", new Point16(rail.roomPos.X, rail.roomPos.Y), ModContent.GetInstance<Remnants>());
                        }

                        int roomCount = rail.grid.Width / 10;
                        while (roomCount > 0)
                        {
                            rail.targetCell.X = WorldGen.genRand.Next(1, rail.grid.Width - 1);

                            if (!rail.FindMarker(rail.targetCell.X - 1, 0) && !rail.FindMarker(rail.targetCell.X, 0) && !rail.FindMarker(rail.targetCell.X + 1, 0) && !rail.FindMarker(rail.targetCell.X, 0, 1))
                            {
                                rail.AddMarker(rail.targetCell.X, 0, 1);

                                roomCount--;
                            }
                        }

                        for (rail.targetCell.Y = 0; rail.targetCell.Y < rail.grid.Height; rail.targetCell.Y++)
                        {
                            for (rail.targetCell.X = 0; rail.targetCell.X < rail.grid.Width; rail.targetCell.X++)
                            {
                                if (rail.FindMarker(rail.targetCell.X, rail.targetCell.Y) || rail.FindMarker(rail.targetCell.X, rail.targetCell.Y, 1))
                                {

                                }
                                else if (rail.targetCell.Y == 0 || rail.FindMarker(rail.targetCell.X, 0))
                                {
                                    Generator.GenerateStructure("Structures/common/MinecartRail/bottom", new Point16(rail.roomPos.X, rail.roomPos.Y), ModContent.GetInstance<Remnants>());

                                    for (int i = rail.roomPos.X; i <= rail.room.Right; i += 4)
                                    {
                                        WGTools.WoodenBeam(i, rail.roomPos.Y + 1);
                                    }
                                }
                            }
                        }

                        bool[] tiles = TileID.Sets.Factory.CreateBoolSet(false, TileID.WoodBlock, TileID.WoodenBeam, TileID.BorealBeam, TileID.RichMahoganyBeam, TileID.MushroomBeam, TileID.GraniteColumn);

                        for (rail.targetCell.X = 0; rail.targetCell.X < rail.grid.Width; rail.targetCell.X++)
                        {
                            if (rail.FindMarker(rail.targetCell.X, 0, 1))
                            {
                                for (int i = rail.roomPos.X + 1; i < rail.roomPos.X + rail.room.Width; i++)
                                {
                                    WorldGen.KillTile(i, rail.area.Y - 1);
                                }
                                WGTools.Tile(rail.roomPos.X, rail.area.Y - 1).TileFrameX = 1; WGTools.Tile(rail.roomPos.X + rail.room.Width, rail.area.Y - 1).TileFrameX = 1;

                                WGTools.Terraform(new Vector2(rail.roomPos.X + 6, rail.area.Y + 1), 6, tiles);
                                WGTools.Terraform(new Vector2(rail.roomPos.X + 6, rail.area.Y - 9), 8);
                            }
                        }

                        #endregion

                        #region cleanup
                        if (hasStation)
                        {
                            rail.targetCell.X = stationX;
                            rail.targetCell.Y = 0;

                            WGTools.Rectangle(rail.roomPos.X - 2, rail.room.Bottom, rail.roomPos.X - 1, rail.room.Bottom, TileID.Platforms, replace: false);
                            WGTools.Rectangle(rail.room.Right + rail.room.Width + 1, rail.room.Bottom, rail.room.Right + rail.room.Width + 2, rail.room.Bottom, TileID.Platforms, replace: false);

                            WGTools.PlaceObjectsInArea(rail.roomPos.X + 1, rail.room.Bottom - 1, rail.room.Right + rail.room.Width - 1, rail.room.Bottom - 1, biomes.FindBiome(rail.room.Right, rail.room.Bottom) == BiomeID.Glowshroom ? ModContent.TileType<Shroomcart>() : ModContent.TileType<Tiles.Objects.Minecart>());
                            //WGTools.PlaceObjectsInArea(rail.roomPos.X + 1, rail.room.Bottom - 1, rail.room.Right + rail.room.Width - 1, rail.room.Bottom - 1, TileID.GrandfatherClocks); 
                            WGTools.PlaceObjectsInArea(rail.roomPos.X + 1, rail.room.Bottom - 1, rail.room.Right + rail.room.Width - 1, rail.room.Bottom - 1, TileID.WorkBenches);
                            WGTools.PlaceObjectsInArea(rail.roomPos.X + 1, rail.room.Bottom - 1, rail.room.Right + rail.room.Width - 1, rail.room.Bottom - 1, TileID.Chairs);
                        }

                        Structures.AddDecorations(rail.area);
                        Structures.AddTheming(rail.area);
                        Structures.AddVariation(rail.area);
                        #endregion
                        #endregion

                        structureCount++;
                    }
                }
            }


            progressCounter++;

            structureCount = 0; // GRANITE TOWER
            int missingPiece = WorldGen.genRand.NextBool(3) ? ItemID.AncientCobaltLeggings : WorldGen.genRand.NextBool(2) ? ItemID.AncientCobaltBreastplate : ItemID.AncientCobaltHelmet;
            while (structureCount < Main.maxTilesY / 300)
            {
                progress.Set((progressCounter + (structureCount / (float)(Main.maxTilesY / 300))) / (float)uniqueStructures);

                #region spawnconditions
                Structures.Dungeon tower = new Structures.Dungeon(0, 0, 2, structureCount == 0 ? 5 : structureCount == 1 ? 3 : Math.Max(WorldGen.genRand.Next(4, 7), WorldGen.genRand.Next(4, 6)), 17, 18, 3);
                bool left = WorldGen.genRand.NextBool(2);
                tower.X = Tundra.X * biomes.scale + biomes.scale / 2 + (left ? -tower.area.Width - 35 : 35);
                tower.Y = WorldGen.genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 500 - tower.area.Height);

                bool[] validTiles = TileID.Sets.Factory.CreateBoolSet(true);

                bool valid = true;
                if (!GenVars.structures.CanPlace(tower.area, validTiles, 10))
                {
                    valid = false;
                }
                else if (tower.Y + tower.area.Height < Main.rockLayer && WorldGen.genRand.NextBool(2))
                {
                    valid = false;
                }
                #endregion

                if (valid)
                {
                    GenVars.structures.AddProtectedStructure(tower.area, 10);

                    WGTools.Rectangle(tower.area.Left - tower.room.Width * (left ? 1 : -1), tower.area.Top, tower.area.Right - tower.room.Width * (left ? 1 : -1), tower.area.Bottom, TileID.Granite);
                    Structures.FillEdges(tower.area.Left - tower.room.Width * (left ? 1 : -1), tower.area.Top, tower.area.Right - tower.room.Width * (left ? 1 : -1), tower.area.Bottom, TileID.Granite, false);

                    #region structure

                    #region rooms
                    for (tower.targetCell.Y = 0; tower.targetCell.Y < tower.grid.Height; tower.targetCell.Y++)
                    {
                        tower.AddMarker(left ? 1 : 0, tower.targetCell.Y);
                        WGTools.Terraform(new Vector2(left ? tower.area.Right + 6 : tower.area.Left - 7, tower.roomPos.Y + 11), 6.5f, scaleX: 1);
                    }
                    int height = WorldGen.genRand.Next(3, tower.grid.Height + 1);
                    int pos = WorldGen.genRand.Next(0, tower.grid.Height - height);
                    for (int j = pos; j < pos + height; j++)
                    {
                        tower.AddMarker(left ? 0 : 1, j);
                    }

                    int roomCount;

                    roomCount = 0;
                    while (roomCount < tower.grid.Height - 1)
                    {
                        tower.targetCell.X = WorldGen.genRand.Next(0, tower.grid.Width);
                        tower.targetCell.Y = roomCount + 1;

                        if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y) && tower.FindMarker(tower.targetCell.X, tower.targetCell.Y - 1))
                        {
                            tower.AddMarker(tower.targetCell.X, tower.targetCell.Y, 1);

                            roomCount++;
                        }
                    }

                    #endregion

                    for (tower.targetCell.Y = 0; tower.targetCell.Y < tower.grid.Height; tower.targetCell.Y++)
                    {
                        for (tower.targetCell.X = 0; tower.targetCell.X < tower.grid.Width; tower.targetCell.X++)
                        {
                            if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y) && !tower.FindMarker(tower.targetCell.X, tower.targetCell.Y, 1))
                            {
                                Generator.GenerateMultistructureSpecific("Structures/common/GraniteTower/room", tower.roomPos, ModContent.GetInstance<Remnants>(), !tower.FindMarker(tower.targetCell.X, tower.targetCell.Y - 1, 1) && tower.targetCell.Y > 0 && tower.FindMarker(tower.targetCell.X, tower.targetCell.Y - 1) ? 1 : 0);

                                if (WorldGen.genRand.NextBool(2))
                                {
                                    WorldGen.PlaceObject(tower.roomPos.X + 3, tower.roomPos.Y + tower.room.Height - 1, TileID.Chairs, style: 34, direction: 1);
                                    WorldGen.PlaceObject(tower.roomPos.X + 5, tower.roomPos.Y + tower.room.Height - 1, TileID.Tables, style: 33);
                                    WorldGen.PlaceObject(tower.roomPos.X + 5, tower.roomPos.Y + tower.room.Height - 3, TileID.Candles, style: 28);
                                    WorldGen.PlaceObject(tower.roomPos.X + 7, tower.roomPos.Y + tower.room.Height - 1, TileID.Chairs, style: 34, direction: -1);

                                    WorldGen.PlaceObject(tower.roomPos.X + 9, tower.roomPos.Y + tower.room.Height - 1, TileID.Chairs, style: 34, direction: 1);
                                    WorldGen.PlaceObject(tower.roomPos.X + 11, tower.roomPos.Y + tower.room.Height - 1, TileID.Tables, style: 33);
                                    WorldGen.PlaceObject(tower.roomPos.X + 11, tower.roomPos.Y + tower.room.Height - 3, TileID.Candles, style: 28);
                                    WorldGen.PlaceObject(tower.roomPos.X + 13, tower.roomPos.Y + tower.room.Height - 1, TileID.Chairs, style: 34, direction: -1);
                                }
                                else
                                {
                                    WorldGen.PlaceObject(tower.roomPos.X + 4, tower.roomPos.Y + tower.room.Height - 1, TileID.Beds, style: 29, direction: 1);
                                    WorldGen.PlaceObject(tower.roomPos.X + 11, tower.roomPos.Y + tower.room.Height - 1, TileID.Beds, style: 29, direction: -1);

                                    int chestIndex = WorldGen.PlaceChest(tower.roomPos.X + 8, tower.roomPos.Y + tower.room.Height - 1, TileID.Dressers, style: 26);
                                    var itemsToAdd = new List<(int type, int stack)>();
                                    Structures.GenericLoot(chestIndex, itemsToAdd);
                                    Structures.FillChest(chestIndex, itemsToAdd);
                                }
                            }
                        }
                    }

                    for (tower.targetCell.Y = 0; tower.targetCell.Y < tower.grid.Height; tower.targetCell.Y++)
                    {
                        for (tower.targetCell.X = 0; tower.targetCell.X < tower.grid.Width; tower.targetCell.X++)
                        {
                            if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y))
                            {
                                if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y, 1))
                                {
                                    Generator.GenerateStructure("Structures/common/GraniteTower/ladder", tower.roomPos, ModContent.GetInstance<Remnants>());
                                }

                                if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y) && (tower.targetCell.X == 0 || !tower.FindMarker(tower.targetCell.X - 1, tower.targetCell.Y)))
                                {
                                    if (!left)
                                    {
                                        Generator.GenerateStructure("Structures/common/GraniteTower/left", new Point16(tower.roomPos.X - 6, tower.roomPos.Y), ModContent.GetInstance<Remnants>());
                                        WorldGen.PlaceTile(tower.roomPos.X - 7, tower.roomPos.Y, TileID.Platforms, style: 28);
                                    }

                                    if (left || !tower.FindMarker(tower.targetCell.X - 1, tower.targetCell.Y))
                                    {
                                        WGTools.Rectangle(tower.roomPos.X - 2, tower.roomPos.Y + 12, tower.roomPos.X - 1, tower.roomPos.Y + tower.room.Height, TileID.GraniteBlock, WallID.GraniteBlock);
                                        WGTools.Rectangle(tower.roomPos.X - 1, tower.roomPos.Y + 13, tower.roomPos.X - 1, tower.roomPos.Y + tower.room.Height - 1, -1);
                                    }
                                }
                                if (tower.FindMarker(tower.targetCell.X, tower.targetCell.Y) && (tower.targetCell.X == tower.grid.Width - 1 || !tower.FindMarker(tower.targetCell.X + 1, tower.targetCell.Y)))
                                {
                                    if (left)
                                    {
                                        Generator.GenerateStructure("Structures/common/GraniteTower/right", new Point16(tower.roomPos.X + tower.room.Width, tower.roomPos.Y), ModContent.GetInstance<Remnants>());
                                        WorldGen.PlaceTile(tower.roomPos.X + tower.room.Width + 6, tower.roomPos.Y, TileID.Platforms, style: 28);
                                    }

                                    if (!left || !tower.FindMarker(tower.targetCell.X + 1, tower.targetCell.Y))
                                    {
                                        WGTools.Rectangle(tower.roomPos.X + tower.room.Width, tower.roomPos.Y + 12, tower.roomPos.X + tower.room.Width + 1, tower.roomPos.Y + tower.room.Height, TileID.GraniteBlock, WallID.GraniteBlock);
                                        WGTools.Rectangle(tower.roomPos.X + tower.room.Width, tower.roomPos.Y + 13, tower.roomPos.X + tower.room.Width, tower.roomPos.Y + tower.room.Height - 1, -1);
                                    }
                                }
                                if (tower.targetCell.Y == tower.grid.Height - 1)
                                {
                                    WGTools.Rectangle(tower.roomPos.X - (tower.targetCell.X == 0 ? 6 : 0), tower.roomPos.Y + tower.room.Height + 1, tower.roomPos.X + tower.room.Width + (tower.targetCell.X == tower.grid.Width - 1 ? 5 : -1), tower.roomPos.Y + tower.room.Height + 4, TileID.Granite);

                                    if (tower.targetCell.X == 0)
                                    {
                                        WorldGen.PlaceTile(tower.roomPos.X - 7, tower.roomPos.Y + tower.room.Height, TileID.Platforms, style: 28);
                                    }
                                    if (tower.targetCell.X == tower.grid.Width - 1)
                                    {
                                        WorldGen.PlaceTile(tower.roomPos.X + tower.room.Width + 6, tower.roomPos.Y + tower.room.Height, TileID.Platforms, style: 28);
                                    }
                                }
                            }
                        }
                    }

                    #region cleanup

                    //for (int y = temple.area.Top - 4; y <= temple.area.Bottom + 50; y++)
                    //{
                    //    for (int x = temple.area.Left - 6; x <= temple.area.Right + 6; x++)
                    //    {
                    //        Tile tile = Main.tile[x, y];
                    //    }
                    //}

                    Structures.AddVariation(tower.area);
                    Structures.AddDecorations(tower.area);
                    #endregion

                    #region objects
                    int objects;

                    objects = 1;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(tower.area.Left, tower.area.Right);
                        int y = WorldGen.genRand.Next(tower.area.Top, tower.area.Bottom);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.Containers && WGTools.Tile(x, y + 1).TileType != TileID.Platforms && WGTools.Tile(x + 1, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y, 2))
                        {
                            int chestIndex = WorldGen.PlaceChest(x, y, style: 50, notNearOtherChests: true);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.Containers)
                            {
                                #region chestloot
                                var itemsToAdd = new List<(int type, int stack)>();

                                int piece = structureCount % 4 == 3 ? ItemID.AncientCobaltLeggings : structureCount % 4 == 2 ? ItemID.AncientCobaltBreastplate : structureCount % 4 == 1 ? ItemID.AncientCobaltHelmet : ItemID.CelestialMagnet;
                                if (piece != missingPiece)
                                {
                                    itemsToAdd.Add((piece, 1));
                                }

                                Structures.GenericLoot(chestIndex, itemsToAdd, 2);

                                Structures.FillChest(chestIndex, itemsToAdd);
                                #endregion

                                objects--;
                            }
                        }
                    }
                    #endregion
                    #endregion

                    structureCount++;
                }
            }

            progressCounter++;

            structureCount = 0; // OVERGROWN CABIN
            while (structureCount < (Main.maxTilesX * Main.maxTilesY / 1200f) / (ModContent.GetInstance<Client>().ExperimentalWorldgen ? 840 : 420))
            {
                progress.Set((progressCounter + (structureCount / (float)(Main.maxTilesX * Main.maxTilesY / 1200f) / (ModContent.GetInstance<Client>().ExperimentalWorldgen ? 840 : 420))) / (float)uniqueStructures);

                #region spawnconditions
                Structures.Dungeon cabin = new Structures.Dungeon(WorldGen.genRand.Next(400, Main.maxTilesX - 400), 0, WorldGen.genRand.Next(2, 4), WorldGen.genRand.Next(1, 3), 12, 9, 3);

                cabin.Y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 200 - cabin.area.Height);

                bool valid = true;
                if (!GenVars.structures.CanPlace(cabin.area, 25))
                {
                    valid = false;
                }
                else if (!Structures.InsideBiome(cabin.area, BiomeID.Jungle))
                {
                    valid = false;
                }
                else if (cabin.Y > GenVars.lavaLine && WorldGen.genRand.NextBool(2))
                {
                    valid = false;
                }
                #endregion

                if (valid)
                {
                    GenVars.structures.AddProtectedStructure(cabin.area, 10);

                    #region structure
                    WGTools.Rectangle(cabin.area.Left, cabin.area.Top, cabin.area.Right, cabin.area.Bottom, -1);

                    WGTools.Terraform(new Vector2(cabin.area.Left, cabin.area.Bottom - 3), 5);
                    WGTools.Terraform(new Vector2(cabin.area.Right, cabin.area.Bottom - 3), 5);

                    WGTools.Rectangle(cabin.area.Left - 2, cabin.area.Bottom, cabin.area.Left - 1, cabin.area.Bottom, TileID.Platforms, replace: false);
                    WGTools.Rectangle(cabin.area.Right + 1, cabin.area.Bottom, cabin.area.Right + 2, cabin.area.Bottom, TileID.Platforms, replace: false);

                    #region rooms
                    int roomCount;

                    for (int i = 0; i < cabin.grid.Width; i++)
                    {
                        cabin.AddMarker(i, cabin.grid.Height - 1, 1);
                    }
                    if (cabin.grid.Height > 1)
                    {
                        int width = WorldGen.genRand.Next(1, cabin.grid.Width + 1);
                        int x = WorldGen.genRand.Next(0, cabin.grid.Width - width);
                        for (int i = x; i < x + width; i++)
                        {
                            cabin.AddMarker(i, cabin.grid.Height - 2, 1);
                        }
                    }

                    //AddMarker(0, 0, 1);
                    //roomCount = (cabin.grid.Height - 1) * cabin.grid.Width / 2;
                    //while (roomCount > 0)
                    //{
                    //    cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                    //    cabin.targetCell.Y = WorldGen.genRand.Next(0, cabin.grid.Height);
                    //    if (roomCount < cabin.grid.Height)
                    //    {
                    //        cabin.targetCell.Y = roomCount;
                    //    }

                    //    if (!FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                    //    {
                    //        AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 1);

                    //        roomCount--;
                    //    }
                    //}

                    //roomCount = cabin.grid.Height * cabin.grid.Width / 8;
                    //while (roomCount > 0)
                    //{
                    //    cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width - 1);
                    //    cabin.targetCell.Y = WorldGen.genRand.Next(0, cabin.grid.Height);

                    //    if (!FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && !FindMarker(cabin.targetCell.X + 1, cabin.targetCell.Y + 1))
                    //    {
                    //        AddMarker(cabin.targetCell.X, cabin.targetCell.Y); AddMarker(cabin.targetCell.X, cabin.targetCell.Y + 1);
                    //        AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 2);

                    //        if (FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                    //        {
                    //            AddMarker(cabin.targetCell.X, cabin.targetCell.Y + 1, 1);
                    //        }

                    //        Generator.GenerateMultistructureRandom("Structures/common/thermalrig/solid", roomPos, ModContent.GetInstance<Remnants>());

                    //        roomCount--;
                    //    }
                    //}

                    roomCount = 0;
                    while (roomCount < cabin.grid.Height - 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = roomCount + 1;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y - 1, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 2);

                            roomCount++;
                        }
                    }

                    roomCount = 0;
                    while (roomCount < 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = cabin.grid.Height - 1;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);

                            int index = WorldGen.genRand.Next(2);
                            Generator.GenerateMultistructureSpecific("Structures/common/OvergrownCabin/bathroom", cabin.roomPos, ModContent.GetInstance<Remnants>(), index);

                            PlacePainting(cabin.roomPos.X + 6, cabin.roomPos.Y + 4);

                            int chestIndex = WorldGen.PlaceChest(cabin.roomPos.X + (index == 0 ? 3 : 9), cabin.roomPos.Y + 8, TileID.Dressers, style: 2);

                            var itemsToAdd = new List<(int type, int stack)>();

                            itemsToAdd.Add((ItemID.FlareGun, 1));
                            itemsToAdd.Add((ItemID.Flare, WorldGen.genRand.Next(15, 30)));

                            Structures.GenericLoot(chestIndex, itemsToAdd);

                            Structures.FillChest(chestIndex, itemsToAdd);

                            roomCount++;
                        }
                    }

                    roomCount = 0;
                    while (roomCount < 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = 0;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);

                            int index = WorldGen.genRand.Next(2);
                            Generator.GenerateMultistructureSpecific("Structures/common/OvergrownCabin/bed", cabin.roomPos, ModContent.GetInstance<Remnants>(), index);

                            int chestIndex = WorldGen.PlaceChest(cabin.roomPos.X + (index == 0 ? 2 : 9), cabin.roomPos.Y + 8, style: 10);

                            var itemsToAdd = new List<(int type, int stack)>();

                            int[] specialItems = new int[5];
                            specialItems[0] = ItemID.AnkletoftheWind;
                            specialItems[1] = ItemID.FeralClaws;
                            specialItems[2] = ItemID.Boomstick;
                            specialItems[3] = ItemID.StaffofRegrowth;
                            specialItems[4] = ItemID.FiberglassFishingPole;

                            int specialItem = specialItems[structureCount % specialItems.Length];
                            itemsToAdd.Add((specialItem, 1));
                            if (specialItem == ItemID.Boomstick)
                            {
                                itemsToAdd.Add((ItemID.MusketBall, Main.rand.Next(30, 60)));
                            }

                            Structures.GenericLoot(chestIndex, itemsToAdd, 2);

                            Structures.FillChest(chestIndex, itemsToAdd);

                            roomCount++;
                        }
                    }

                    #endregion

                    for (cabin.targetCell.Y = cabin.grid.Height - 1; cabin.targetCell.Y >= 0; cabin.targetCell.Y--)
                    {
                        for (cabin.targetCell.X = 0; cabin.targetCell.X <= cabin.grid.Width; cabin.targetCell.X++)
                        {
                            if (cabin.targetCell.X < cabin.grid.Width)
                            {
                                if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && !cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 2))
                                {
                                    Generator.GenerateMultistructureRandom("Structures/common/OvergrownCabin/seat", cabin.roomPos, ModContent.GetInstance<Remnants>());
                                }
                                if (cabin.targetCell.Y == cabin.grid.Height - 1)
                                {
                                    Generator.GenerateStructure("Structures/common/OvergrownCabin/bottom", new Point16(cabin.roomPos.X, cabin.roomPos.Y + cabin.room.Height), ModContent.GetInstance<Remnants>());
                                }
                            }

                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && (cabin.targetCell.Y == 0 || !cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y - 1, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/OvergrownCabin/roof-middle", new Point16(cabin.roomPos.X, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X == 0 || (!cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X < cabin.grid.Width - 1 && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/OvergrownCabin/roof-left", new Point16(cabin.roomPos.X - 2, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                            if (cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X == cabin.grid.Width || (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X > 0 && cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/OvergrownCabin/roof-right", new Point16(cabin.roomPos.X, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                        }
                    }

                    for (cabin.targetCell.Y = cabin.grid.Height - 1; cabin.targetCell.Y >= 0; cabin.targetCell.Y--)
                    {
                        for (cabin.targetCell.X = 0; cabin.targetCell.X <= cabin.grid.Width; cabin.targetCell.X++)
                        {
                            if (cabin.targetCell.X < cabin.grid.Width && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 2))
                            {
                                Generator.GenerateMultistructureRandom("Structures/common/OvergrownCabin/ladder", cabin.roomPos, ModContent.GetInstance<Remnants>());
                            }

                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X == 0 || cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X == cabin.grid.Width || (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1)) || (!cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateMultistructureSpecific("Structures/common/OvergrownCabin/wall", new Point16(cabin.roomPos.X, cabin.roomPos.Y), ModContent.GetInstance<Remnants>(), cabin.targetCell.Y == cabin.grid.Height - 1 ? 0 : 1);
                            }
                        }
                    }

                    #region objects
                    int objects;

                    //objects = 1;
                    //while (objects > 0)
                    //{
                    //    int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                    //    int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom);

                    //    if (Framing.GetTileSafely(x, y).TileType != TileID.Containers && WGTools.Tile(x, y + 1).TileType == TileID.GrayBrick && WGTools.Tile(x + 1, y + 1).TileType == TileID.GrayBrick && WGTools.NoDoors(x, y, 2))
                    //    {
                    //        int chestIndex = WorldGen.PlaceChest(x, y, style: 1, notNearOtherChests: true);
                    //        if (Framing.GetTileSafely(x, y).TileType == TileID.Containers)
                    //        {
                    //            #region chestloot
                    //            var itemsToAdd = new List<(int type, int stack)>();

                    //            int[] specialItems = new int[3];
                    //            specialItems[0] = ItemID.HermesBoots;
                    //            specialItems[1] = ItemID.CloudinaBottle;
                    //            specialItems[2] = ItemID.MagicMirror;

                    //            int specialItem = specialItems[structureCount % specialItems.Length];
                    //            itemsToAdd.Add((specialItem, 1));

                    //            StructureTools.GenericLoot(chestIndex, itemsToAdd, 2);

                    //            StructureTools.FillChest(chestIndex, itemsToAdd);
                    //            #endregion

                    //            objects--;
                    //        }
                    //    }
                    //}

                    objects = cabin.grid.Height * cabin.grid.Width / 2;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.ClayPot && !WGTools.Tile(x, y - 1).HasTile && Framing.GetTileSafely(x, y + 1).TileType == TileID.RichMahogany && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceObject(x, y, TileID.ClayPot);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.ClayPot)
                            {
                                WorldGen.PlaceTile(x, y - 1, TileID.ImmatureHerbs, style: 1);
                                objects--;
                            }
                        }
                    }

                    objects = cabin.grid.Height * cabin.grid.Width;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceSmallPile(x, y, Main.rand.Next(6), 0);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                            {
                                objects--;
                            }
                        }
                    }
                    objects = cabin.grid.Height * cabin.grid.Width;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceSmallPile(x, y, Main.rand.Next(28, 35), 0);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                            {
                                objects--;
                            }
                        }
                    }
                    #endregion

                    #region cleanup
                    for (int y = cabin.area.Top - 4; y <= cabin.area.Bottom + 2; y++)
                    {
                        for (int x = cabin.area.Left - 2; x <= cabin.area.Right + 2; x++)
                        {
                            Tile tile = Main.tile[x, y];

                            if (y == cabin.area.Bottom + 1 && tile.HasTile && TileID.Sets.IsBeam[tile.TileType] && (!WGTools.Tile(x, y + 1).HasTile || WGTools.Tile(x, y + 1).TileType != TileID.RichMahoganyBeam))
                            {
                                int j = y;
                                WGTools.WoodenBeam(x, j);
                            }
                        }
                    }

                    Structures.AddDecorations(cabin.area);
                    Structures.AddTheming(cabin.area);
                    Structures.AddVariation(cabin.area);
                    #endregion
                    #endregion

                    structureCount++;
                }
            }

            progressCounter++;

            structureCount = 0; // BURIED CABIN
            while (structureCount < (Main.maxTilesX * Main.maxTilesY / 1200f) / 300)
            {
                progress.Set((progressCounter + (structureCount / (float)((Main.maxTilesX * Main.maxTilesY / 1200f) / 300))) / (float)uniqueStructures);

                #region spawnconditions
                Structures.Dungeon cabin = new Structures.Dungeon(0, 0, WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(1, 3), 8, 9, 3);

                cabin.X = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.1f), (int)(Main.maxTilesX * 0.9f) - cabin.area.Width);
                cabin.Y = (structureCount > (Main.maxTilesX * Main.maxTilesY / 1200) / 630) ? WorldGen.genRand.Next((int)Main.rockLayer, GenVars.lavaLine - cabin.area.Height) : WorldGen.genRand.Next(GenVars.lavaLine, Main.maxTilesY - 200 - cabin.area.Height);

                bool[] validTiles = TileID.Sets.Factory.CreateBoolSet(true, TileID.MushroomGrass, TileID.SnowBlock, TileID.IceBlock, TileID.Mud, TileID.JungleGrass, TileID.Sand, TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone, TileID.Ash, TileID.Marble, TileID.LihzahrdBrick);

                bool valid = true;
                if (!GenVars.structures.CanPlace(cabin.area, validTiles, 25))
                {
                    valid = false;
                }
                else if (!Structures.AvoidsBiomes(cabin.area, new int[] { BiomeID.Granite, BiomeID.Hive, BiomeID.Toxic, BiomeID.Obsidian }))
                {
                    valid = false;
                }
                #endregion

                if (valid)
                {
                    GenVars.structures.AddProtectedStructure(cabin.area, 10);

                    #region structure
                    WGTools.Rectangle(cabin.area.Left, cabin.area.Top, cabin.area.Right, cabin.area.Bottom, -1);

                    WGTools.Terraform(new Vector2(cabin.area.Left, cabin.area.Bottom - 3), 5);
                    WGTools.Terraform(new Vector2(cabin.area.Right, cabin.area.Bottom - 3), 5);

                    WGTools.Rectangle(cabin.area.Left - 2, cabin.area.Bottom, cabin.area.Left - 1, cabin.area.Bottom, TileID.Platforms, replace: false);
                    WGTools.Rectangle(cabin.area.Right + 1, cabin.area.Bottom, cabin.area.Right + 2, cabin.area.Bottom, TileID.Platforms, replace: false);

                    #region rooms
                    int roomCount;

                    for (int i = 0; i < cabin.grid.Width; i++)
                    {
                        cabin.AddMarker(i, cabin.grid.Height - 1, 1);
                    }
                    if (cabin.grid.Height > 1)
                    {
                        int width = WorldGen.genRand.Next(2, cabin.grid.Width + 1);
                        int x = WorldGen.genRand.Next(0, cabin.grid.Width - width);
                        for (int i = x; i < x + width; i++)
                        {
                            cabin.AddMarker(i, cabin.grid.Height - 2, 1);
                        }
                    }

                    //cabin.AddMarker(0, 0, 1);
                    //roomCount = (cabin.grid.Height - 1) * cabin.grid.Width / 2;
                    //while (roomCount > 0)
                    //{
                    //    cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                    //    cabin.targetCell.Y = WorldGen.genRand.Next(0, cabin.grid.Height);
                    //    if (roomCount < cabin.grid.Height)
                    //    {
                    //        cabin.targetCell.Y = roomCount;
                    //    }

                    //    if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                    //    {
                    //        cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 1);

                    //        roomCount--;
                    //    }
                    //}

                    //roomCount = cabin.grid.Height * cabin.grid.Width / 8;
                    //while (roomCount > 0)
                    //{
                    //    cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width - 1);
                    //    cabin.targetCell.Y = WorldGen.genRand.Next(0, cabin.grid.Height);

                    //    if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && !cabin.FindMarker(cabin.targetCell.X + 1, cabin.targetCell.Y + 1))
                    //    {
                    //        cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y); cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y + 1);
                    //        cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 2);

                    //        if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                    //        {
                    //            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y + 1, 1);
                    //        }

                    //        Generator.GenerateMultistructureRandom("Structures/common/thermalrig/solid", cabin.roomPos, ModContent.GetInstance<Remnants>());

                    //        roomCount--;
                    //    }
                    //}

                    roomCount = 0;
                    while (roomCount < cabin.grid.Height - 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = roomCount + 1;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y - 1, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y, 2);

                            roomCount++;
                        }
                    }

                    roomCount = 0;
                    while (roomCount < 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = 0;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);

                            int index = WorldGen.genRand.Next(2);
                            Generator.GenerateMultistructureSpecific("Structures/common/BuriedCabin/bed", cabin.roomPos, ModContent.GetInstance<Remnants>(), index);

                            int chestIndex = WorldGen.PlaceChest(cabin.roomPos.X + (index == 0 ? 2 : 6), cabin.roomPos.Y + 8, TileID.Dressers, style: biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Jungle || biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Hive ? 2 : biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Tundra ? 18 : 0);

                            var itemsToAdd = new List<(int type, int stack)>();

                            itemsToAdd.Add((ItemID.FlareGun, 1));
                            itemsToAdd.Add((ItemID.Flare, WorldGen.genRand.Next(15, 30)));

                            Structures.GenericLoot(chestIndex, itemsToAdd);

                            Structures.FillChest(chestIndex, itemsToAdd);

                            roomCount++;
                        }
                    }

                    roomCount = 0;
                    while (roomCount < 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = cabin.grid.Height - 1;

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);

                            Generator.GenerateStructure("Structures/common/BuriedCabin/blank", cabin.roomPos, ModContent.GetInstance<Remnants>());

                            WorldGen.PlaceObject(cabin.roomPos.X + 4, cabin.roomPos.Y + 5, TileID.Painting3X3, style: 45);

                            int chestIndex = 0;
                            int style = biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Jungle || biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Hive ? 10 : biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) == BiomeID.Tundra ? 11 : 1;
                            if (WorldGen.genRand.NextBool(2))
                            {
                                chestIndex = WorldGen.PlaceChest(cabin.roomPos.X + 2, cabin.roomPos.Y + 8, style: style);

                                //if (structureCount % 2 == 0)
                                //{
                                //    WorldGen.PlaceObject(cabin.roomPos.X + 5, cabin.roomPos.Y + 8, TileID.Anvils);
                                //}
                                //else
                                 WorldGen.PlaceObject(cabin.roomPos.X + 5, cabin.roomPos.Y + 8, TileID.SharpeningStation);
                            }
                            else
                            {
                                chestIndex = WorldGen.PlaceChest(cabin.roomPos.X + 5, cabin.roomPos.Y + 8, style: style);

                                //if (structureCount % 2 == 0)
                                //{
                                //    WorldGen.PlaceObject(cabin.roomPos.X + 2, cabin.roomPos.Y + 8, TileID.Anvils);
                                //}
                                //else
                                WorldGen.PlaceObject(cabin.roomPos.X + 3, cabin.roomPos.Y + 8, TileID.SharpeningStation);
                            }
                            //if (biomes.FindBiome(cabin.roomPos.X + 4, cabin.roomPos.Y + 8) != BiomeID.Jungle)
                            //{
                            //    WorldGen.PlaceTile(cabin.roomPos.X + 4, cabin.roomPos.Y + 8, TileID.MetalBars, style: 2);
                            //    WorldGen.PlaceTile(cabin.roomPos.X + 4, cabin.roomPos.Y + 7, TileID.MetalBars, style: 2);
                            //}

                            var itemsToAdd = new List<(int type, int stack)>();

                            int[] specialItems = new int[5];
                            specialItems[0] = ItemID.MagicMirror;
                            specialItems[1] = ItemID.CloudinaBottle;
                            specialItems[2] = ItemID.BandofRegeneration;
                            specialItems[3] = ItemID.Mace;
                            specialItems[4] = ItemID.TreasureMagnet;

                            int specialItem = specialItems[structureCount % specialItems.Length];
                            itemsToAdd.Add((specialItem, 1));

                            Structures.GenericLoot(chestIndex, itemsToAdd, 2);

                            Structures.FillChest(chestIndex, itemsToAdd);

                            roomCount++;
                        }
                    }

                    roomCount = 0;
                    while (roomCount < 1)
                    {
                        cabin.targetCell.X = WorldGen.genRand.Next(0, cabin.grid.Width);
                        cabin.targetCell.Y = WorldGen.genRand.Next(0, cabin.grid.Height);

                        if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1))
                        {
                            cabin.AddMarker(cabin.targetCell.X, cabin.targetCell.Y);

                            Generator.GenerateMultistructureRandom("Structures/common/BuriedCabin/books", cabin.roomPos, ModContent.GetInstance<Remnants>());

                            roomCount++;
                        }
                    }

                    #endregion

                    for (cabin.targetCell.Y = cabin.grid.Height - 1; cabin.targetCell.Y >= 0; cabin.targetCell.Y--)
                    {
                        for (cabin.targetCell.X = 0; cabin.targetCell.X <= cabin.grid.Width; cabin.targetCell.X++)
                        {
                            if (cabin.targetCell.X < cabin.grid.Width)
                            {
                                if (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && !cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 2))
                                {
                                    if (WorldGen.genRand.NextBool(2))
                                    {
                                        Generator.GenerateMultistructureSpecific("Structures/common/BuriedCabin/seat", cabin.roomPos, ModContent.GetInstance<Remnants>(), WorldGen.genRand.Next(1, 3));
                                        PlacePainting(cabin.roomPos.X + 4, cabin.roomPos.Y + 4);
                                    }
                                    else Generator.GenerateMultistructureSpecific("Structures/common/BuriedCabin/seat", cabin.roomPos, ModContent.GetInstance<Remnants>(), 0);
                                }
                                if (cabin.targetCell.Y == cabin.grid.Height - 1)
                                {
                                    Generator.GenerateStructure("Structures/common/BuriedCabin/bottom", new Point16(cabin.roomPos.X, cabin.roomPos.Y + cabin.room.Height), ModContent.GetInstance<Remnants>());
                                }
                            }

                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && (cabin.targetCell.Y == 0 || !cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y - 1, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/BuriedCabin/roof-middle", new Point16(cabin.roomPos.X, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X == 0 || (!cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X < cabin.grid.Width - 1 && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/BuriedCabin/roof-left", new Point16(cabin.roomPos.X - 2, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                            if (cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X == cabin.grid.Width || (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X > 0 && cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateStructure("Structures/common/BuriedCabin/roof-right", new Point16(cabin.roomPos.X, cabin.roomPos.Y - 4), ModContent.GetInstance<Remnants>());
                            }
                        }
                    }

                    for (cabin.targetCell.Y = cabin.grid.Height - 1; cabin.targetCell.Y >= 0; cabin.targetCell.Y--)
                    {
                        for (cabin.targetCell.X = 0; cabin.targetCell.X <= cabin.grid.Width; cabin.targetCell.X++)
                        {
                            if (cabin.targetCell.X < cabin.grid.Width && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 2))
                            {
                                Generator.GenerateMultistructureRandom("Structures/common/BuriedCabin/ladder", cabin.roomPos, ModContent.GetInstance<Remnants>());
                            }

                            if (cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.targetCell.X == 0 || cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.targetCell.X == cabin.grid.Width || (!cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1)) || (!cabin.FindMarker(cabin.targetCell.X - 1, cabin.targetCell.Y, 1) && cabin.FindMarker(cabin.targetCell.X, cabin.targetCell.Y, 1)))
                            {
                                Generator.GenerateMultistructureSpecific("Structures/common/BuriedCabin/wall", new Point16(cabin.roomPos.X, cabin.roomPos.Y), ModContent.GetInstance<Remnants>(), cabin.targetCell.Y == cabin.grid.Height - 1 ? 0 : 1);
                            }
                        }
                    }

                    #region objects
                    int objects;

                    //objects = 1;
                    //while (objects > 0)
                    //{
                    //    int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                    //    int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom);

                    //    if (Framing.GetTileSafely(x, y).TileType != TileID.Containers && WGTools.Tile(x, y + 1).TileType == TileID.GrayBrick && WGTools.Tile(x + 1, y + 1).TileType == TileID.GrayBrick && WGTools.NoDoors(x, y, 2))
                    //    {
                    //        int chestIndex = WorldGen.PlaceChest(x, y, style: 1, notNearOtherChests: true);
                    //        if (Framing.GetTileSafely(x, y).TileType == TileID.Containers)
                    //        {
                    //            #region chestloot
                    //            var itemsToAdd = new List<(int type, int stack)>();

                    //            int[] specialItems = new int[3];
                    //            specialItems[0] = ItemID.HermesBoots;
                    //            specialItems[1] = ItemID.CloudinaBottle;
                    //            specialItems[2] = ItemID.MagicMirror;

                    //            int specialItem = specialItems[structureCount % specialItems.Length];
                    //            itemsToAdd.Add((specialItem, 1));

                    //            StructureTools.GenericLoot(chestIndex, itemsToAdd, 2);

                    //            StructureTools.FillChest(chestIndex, itemsToAdd);
                    //            #endregion

                    //            objects--;
                    //        }
                    //    }
                    //}

                    objects = cabin.grid.Height * cabin.grid.Width / 2;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.ClayPot && !WGTools.Tile(x, y - 1).HasTile && Framing.GetTileSafely(x, y + 1).TileType == TileID.WoodBlock && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceObject(x, y, TileID.ClayPot);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.ClayPot)
                            {
                                WorldGen.PlaceTile(x, y - 1, TileID.ImmatureHerbs, style: 2);
                                objects--;
                            }
                        }
                    }

                    objects = cabin.grid.Height * cabin.grid.Width;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceSmallPile(x, y, Main.rand.Next(6), 0);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                            {
                                objects--;
                            }
                        }
                    }
                    objects = cabin.grid.Height * cabin.grid.Width;
                    while (objects > 0)
                    {
                        int x = WorldGen.genRand.Next(cabin.area.Left, cabin.area.Right);
                        int y = WorldGen.genRand.Next(cabin.area.Top, cabin.area.Bottom + 2);

                        if (Framing.GetTileSafely(x, y).TileType != TileID.SmallPiles && Framing.GetTileSafely(x, y + 1).TileType != TileID.Platforms && WGTools.NoDoors(x, y))
                        {
                            WorldGen.PlaceSmallPile(x, y, Main.rand.Next(28, 35), 0);
                            if (Framing.GetTileSafely(x, y).TileType == TileID.SmallPiles)
                            {
                                objects--;
                            }
                        }
                    }
                    #endregion

                    #region cleanup
                    for (int y = cabin.area.Top - 4; y <= cabin.area.Bottom + 2; y++)
                    {
                        for (int x = cabin.area.Left - 2; x <= cabin.area.Right + 2; x++)
                        {
                            Tile tile = Main.tile[x, y];

                            if (y == cabin.area.Bottom + 1 && tile.HasTile && TileID.Sets.IsBeam[tile.TileType] && (!WGTools.Tile(x, y + 1).HasTile || WGTools.Tile(x, y + 1).TileType != TileID.WoodenBeam))
                            {
                                int j = y;
                                WGTools.WoodenBeam(x, j);
                            }
                        }
                    }

                    Structures.AddDecorations(cabin.area);
                    Structures.AddTheming(cabin.area);
                    Structures.AddVariation(cabin.area);
                    #endregion
                    #endregion

                    structureCount++;
                }
            }

            progressCounter++;

            structureCount = 0; // MINING PLATFORM
            while (structureCount < (Main.maxTilesX * Main.maxTilesY / 1200f) / 175)
            {
                progress.Set((progressCounter + (structureCount / (float)((Main.maxTilesX * Main.maxTilesY / 1200f) / 175))) / (float)uniqueStructures);

                #region spawnconditions

                int x = structureCount < (Main.maxTilesX * Main.maxTilesY / 1200f) / (175 * 5) ? WorldGen.genRand.Next((int)(Main.maxTilesX * 0.4f), (int)(Main.maxTilesX * 0.6f)) : WorldGen.genRand.NextBool(2) ? WorldGen.genRand.Next((int)(Main.maxTilesX * 0.1f), (int)(Main.maxTilesX * 0.4f)) : WorldGen.genRand.Next((int)(Main.maxTilesX * 0.6f), (int)(Main.maxTilesX * 0.9f));
                int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 200);
                int height = Math.Max(WorldGen.genRand.Next(2, 7), WorldGen.genRand.Next(2, 7));
                Rectangle area = new Rectangle(x - 3, y - height * 6 - 8, 7, height * 6 + 8);

                bool[] validTiles = TileID.Sets.Factory.CreateBoolSet(true, TileID.Sand, TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone, TileID.Ash, TileID.LihzahrdBrick);

                bool valid = true;
                if (!GenVars.structures.CanPlace(area, validTiles, 25))
                {
                    valid = false;
                }
                else if (!Structures.AvoidsBiomes(area, new int[] { BiomeID.Glowshroom, BiomeID.Granite, BiomeID.Hive, BiomeID.Toxic, BiomeID.Obsidian }))
                {
                    valid = false;
                }
                else
                {
                    for (int j = y - height * 6 - 6; j <= y + 3; j++)
                    {
                        for (int i = x - 3; i <= x + 3; i++)
                        {
                            if (j < y - 1)
                            {
                                if (WGTools.Solid(i, j))
                                {
                                    valid = false;
                                }
                            }
                            else if (j > y)
                            {
                                if (!WGTools.Solid(i, j))
                                {
                                    valid = false;
                                }
                            }
                            else if (WGTools.Tile(i, j).LiquidAmount > 0 && WGTools.Tile(i, j).LiquidType == LiquidID.Lava)
                            {
                                valid = false;
                            }
                        }
                    }

                    if (valid)
                    {
                        int length = 0;
                        for (int i = x + 2; !WGTools.Solid(i, y - height * 6); i++)
                        {
                            length++;
                        }
                        for (int i = x - 2; !WGTools.Solid(i, y - height * 6); i--)
                        {
                            length++;
                        }
                        if (length > 40)
                        {
                            valid = false;
                        }
                    }
                }
                #endregion

                if (valid)
                {
                    GenVars.structures.AddProtectedStructure(area, 10);

                    #region structure
                    WGTools.Rectangle(x - 3, y, x + 3, y, TileID.WoodBlock);
                    for (int k = 0; k < height; k++)
                    {
                        y -= 6;
                        Generator.GenerateStructure("Structures/common/Platform/ladder", new Point16(x - 3, y), ModContent.GetInstance<Remnants>());
                    }
                    for (int i = x + 2; !WGTools.Solid(i, y); i++)
                    {
                        WGTools.Tile(i, y).TileType = biomes.FindBiome(i, y) == BiomeID.Glowshroom ? TileID.MushroomBlock : biomes.FindBiome(i, y) == BiomeID.Jungle ? TileID.RichMahogany : biomes.FindBiome(i, y) == BiomeID.Tundra ? TileID.BorealWood : TileID.WoodBlock;
                        WGTools.Tile(i, y).HasTile = true;
                    }
                    for (int i = x - 2; !WGTools.Solid(i, y); i--)
                    {
                        WGTools.Tile(i, y).TileType = biomes.FindBiome(i, y) == BiomeID.Glowshroom ? TileID.MushroomBlock : biomes.FindBiome(i, y) == BiomeID.Jungle ? TileID.RichMahogany : biomes.FindBiome(i, y) == BiomeID.Tundra ? TileID.BorealWood : TileID.WoodBlock;
                        WGTools.Tile(i, y).HasTile = true;
                    }

                    Generator.GenerateStructure("Structures/common/Platform/cabin", new Point16(x - 5, y - 8), ModContent.GetInstance<Remnants>());

                    #region cleanup

                    Structures.AddTheming(area);
                    Structures.AddVariation(area);
                    #endregion
                    #endregion

                    structureCount++;
                }
            }
        }

        private void PlacePainting(int x, int y)
        {
            int style2 = Main.rand.Next(10);

            if (style2 == 0)
            {
                style2 = 20;
            }
            else if (style2 == 1)
            {
                style2 = 21;
            }
            else if (style2 == 2)
            {
                style2 = 22;
            }
            else if (style2 == 3)
            {
                style2 = 24;
            }
            else if (style2 == 4)
            {
                style2 = 25;
            }
            else if (style2 == 5)
            {
                style2 = 26;
            }
            else if (style2 == 6)
            {
                style2 = 28;
            }
            else if (style2 == 7)
            {
                style2 = 33;
            }
            else if (style2 == 8)
            {
                style2 = 34;
            }
            else if (style2 == 9)
            {
                style2 = 35;
            }

            WorldGen.PlaceObject(x, y, TileID.Painting3X3, style: style2);
        }
    }
}