using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Server.MirDatabase;
using Server.MirObjects;
using S = ServerPackets;

namespace Server.MirEnvir
{
    public class Map
    {
        private static Envir Envir
        {
            get { return SMain.Envir; }
        }
        
        public MapInfo Info;

        public int Width, Height;
        public List<Point> WalkableCells;

        public CellAttribute[,] CellAttrs;
        public Dictionary<int, sbyte> FishingAttrs = new Dictionary<int, sbyte>();
        public MineSet[,] Mine;

        public List<MapObject>[,] MapObjects;
        public Dictionary<int, Door> Doors = new Dictionary<int, Door>();

        public byte[,] StonesLeft;
        public long[,] LastRegenTick;


        public long LightningTime, FireTime, InactiveTime;
        public int MonsterCount;
        public int InactiveCount = 10;

        public List<NPCObject> NPCs = new List<NPCObject>();
        public List<PlayerObject> Players = new List<PlayerObject>();
        public MapRespawn[] Respawns;
        public List<DelayedAction> ActionList = new List<DelayedAction>();
        public LinkedList<MapObject> Objects = new LinkedList<MapObject>();

        public List<ConquestObject> Conquest = new List<ConquestObject>();
        public ConquestObject tempConquest;
        public byte Region;

        public long ReportTime;

        LinkedListNode<MapObject> Current;

        public Map(MapInfo info)
        {
            Info = info;
        }

        public Door AddDoor(byte DoorIndex, Point location)
        {
            DoorIndex = (byte)(DoorIndex & 0x7F);
            foreach (var Info in Doors.Values)
                if (Info.index == DoorIndex)
                    return Info;
            Door DoorInfo = new Door() { index = DoorIndex, X = location.X, Y = location.Y };
            Doors.Add(location.X  + location.Y * Width, DoorInfo);
            return DoorInfo;
        }
        
        public bool OpenDoor(byte DoorIndex)
        {
            foreach (var Info in Doors.Values)
                if (Info.index == DoorIndex)
                {
                    Info.DoorState = 2;
                    Info.LastTick = Envir.Time;
                    return true;
                }
            return false;
        }

        private byte FindType(byte[] input)
        {
            //c# custom map format
            if ((input[2] == 0x43) && (input[3] == 0x23))
            {
                return 100;
            }
            //wemade mir3 maps have no title they just start with blank bytes
            if (input[0] == 0)
                return 5;
            //shanda mir3 maps start with title: (C) SNDA, MIR3.
            if ((input[0] == 0x0F) && (input[5] == 0x53) && (input[14] == 0x33))
                return 6;

            //wemades antihack map (laby maps) title start with: Mir2 AntiHack
            if ((input[0] == 0x15) && (input[4] == 0x32) && (input[6] == 0x41) && (input[19] == 0x31))
                return 4;

            //wemades 2010 map format i guess title starts with: Map 2010 Ver 1.0
            if ((input[0] == 0x10) && (input[2] == 0x61) && (input[7] == 0x31) && (input[14] == 0x31))
                return 1;

            //shanda's 2012 format and one of shandas(wemades) older formats share same header info, only difference is the filesize
            if ((input[4] == 0x0F) && (input[18] == 0x0D) && (input[19] == 0x0A))
            {
                int W = input[0] + (input[1] << 8);
                int H = input[2] + (input[3] << 8);
                if (input.Length > (52 + (W * H * 14)))
                    return 3;
                else
                    return 2;
            }

            //3/4 heroes map format (myth/lifcos i guess)
            if ((input[0] == 0x0D) && (input[1] == 0x4C) && (input[7] == 0x20) && (input[11] == 0x6D))
                return 7;
            return 0;
        }

        private void LoadMapCellsv0(byte[] fileBytes)
        {
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 12
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.

                    offSet += 2;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //No Floor Tile.

                    offSet += 4;

                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));

                    offSet += 3;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));
                }
        }
        
        private void LoadMapCellsv1(byte[] fileBytes)
        {
            int offSet = 21;

            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            Width = w ^ xor;
            Height = h ^ xor;
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 54;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    int v = BitConverter.ToInt32(fileBytes, offSet);
                    if (((BitConverter.ToInt32(fileBytes, offSet) ^ 0xAA38AA38) & 0x20000000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.

                    offSet += 6;
                    if (((BitConverter.ToInt16(fileBytes, offSet) ^ xor) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //No Floor Tile.

                    offSet += 2;
                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));
                    offSet += 5;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));

                    offSet += 1;
                }
        }

        private void LoadMapCellsv2(byte[] fileBytes)
        {
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 14
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //No Floor Tile.

                    offSet += 2;
                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));
                    offSet += 5;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));

                    offSet += 2;
                }
        }

        private void LoadMapCellsv3(byte[] fileBytes)
        {
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 36
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //No Floor Tile.

                    offSet += 2;
                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));
                    offSet += 12;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));

                    offSet += 17;
                }
        }

        private void LoadMapCellsv4(byte[] fileBytes)
        {
            int offSet = 31;
            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            Width = w ^ xor;
            Height = h ^ xor;
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 64;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 12
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.

                    offSet += 2;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.

                    offSet += 4;
                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));
                    offSet += 6;
                }
        }

        private void LoadMapCellsv5(byte[] fileBytes)
        {
            int offSet = 22;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 28 + (3 * ((Width / 2) + (Width % 2)) * (Height / 2));
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 14
                    if ((fileBytes[offSet] & 0x01) != 1)
                        CellAttrs[x , y] = CellAttribute.HighWall;
                    else if ((fileBytes[offSet] & 0x02) != 2)
                        CellAttrs[x , y] = CellAttribute.LowWall;
                    offSet += 13;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));
                }
        }

        private void LoadMapCellsv6(byte[] fileBytes)
        {
            int offSet = 16;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 40;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 20
                    if ((fileBytes[offSet] & 0x01) != 1)
                        CellAttrs[x , y] = CellAttribute.HighWall;
                    else if ((fileBytes[offSet] & 0x02) != 2)
                        CellAttrs[x , y] = CellAttribute.LowWall;
                    offSet += 20;
                }
        }

        private void LoadMapCellsv7(byte[] fileBytes)
        {
            int offSet = 21;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 4;
            Height = BitConverter.ToInt16(fileBytes, offSet);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offSet = 54;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 15
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.
                    offSet += 6;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.
                    //offSet += 2;
                    offSet += 2;
                    if (fileBytes[offSet] > 0)
                        AddDoor(fileBytes[offSet], new Point(x, y));
                    offSet += 4;

                    byte light = fileBytes[offSet++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));

                    offSet += 2;
                }
        }

        private void LoadMapCellsV100(byte[] Bytes)
        {
            int offset = 4;
            if ((Bytes[0] != 1) || (Bytes[1] != 0)) return;//only support version 1 atm
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            CellAttrs = new CellAttribute[Width , Height];
            MapObjects = new List<MapObject>[Width , Height];

            offset = 8;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    offset += 2;
                    if ((BitConverter.ToInt32(Bytes, offset) & 0x20000000) != 0)
                        CellAttrs[x , y] = CellAttribute.HighWall; //Can Fire Over.
                    offset += 10;
                    if ((BitConverter.ToInt16(Bytes, offset) & 0x8000) != 0)
                        CellAttrs[x , y] = CellAttribute.LowWall; //Can't Fire Over.

                    offset += 2;
                    if (Bytes[offset] > 0)
                        AddDoor(Bytes[offset], new Point(x, y));
                    offset += 11;

                    byte light = Bytes[offset++];

                    if (light >= 100 && light <= 119)
                        FishingAttrs.Add(x + y * Width, (sbyte)(light - 100));
                }
                
        }

        public static void RepairFileName(string filePath)
        {
            string realName = GetFileRealName(filePath);
            if (!filePath.EndsWith(realName))
            {
                SMain.Enqueue(string.Format("file name sensitivity {0} {1}", filePath, realName));
                Directory.Move(filePath, filePath + "n");
                Directory.Move(filePath + "n", filePath);
            }
        }

        public static string GetFileRealName(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            FileInfo[] fileInfos = info.Directory.GetFiles();
            for (int i = 0; i < fileInfos.Length; ++i)
            {
                if (fileInfos[i].Name.ToUpper() == info.Name.ToUpper())
                    return fileInfos[i].Name;
            }
            return null;
        }

        public bool Load()
        {
            try
            {
                string fileName = Path.Combine(Settings.MapPath, Info.FileName + ".map");

 //               File.AppendAllText("MapList.txt",
   //                           string.Format("{0}" + Environment.NewLine, Info.FileName + ".map"));
              //  RepairFileName(fileName);

                if (File.Exists(fileName))
                {
                    byte[] fileBytes = File.ReadAllBytes(fileName);
                    switch(FindType(fileBytes))
                    {
                        case 0:
                            LoadMapCellsv0(fileBytes);
                            break;
                        case 1:
                            LoadMapCellsv1(fileBytes);
                            break;
                        case 2:
                            LoadMapCellsv2(fileBytes);
                            break;
                        case 3:
                            LoadMapCellsv3(fileBytes);
                            break;
                        case 4:
                            LoadMapCellsv4(fileBytes);
                            break;
                        case 5:
                            LoadMapCellsv5(fileBytes);
                            break;
                        case 6:
                            LoadMapCellsv6(fileBytes);
                            break;
                        case 7:
                            LoadMapCellsv7(fileBytes);
                            break;
                        case 100:
                            LoadMapCellsV100(fileBytes);
                            break;
                    }

                    Respawns = new MapRespawn[Info.Respawns.Count];
                    for (int i = 0; i < Info.Respawns.Count; i++)
                    {
                        MapRespawn info = new MapRespawn(Info.Respawns[i]);
                        if (info.Monster == null) continue;
                        info.Map = this;
                        Respawns[i] = info;
                        Envir.TotalMonsterCount += info.Info.Count;

                        if ((info.Info.SaveRespawnTime) && (info.Info.RespawnTicks != 0))
                            SMain.Envir.SavedSpawns.Add(info);
                    }

                    for (int i = 0; i < Info.NPCs.Count; i++)
                    {
                        NPCInfo info = Info.NPCs[i];
                        if (!ValidPoint(info.Location))
                        {
                            SMain.Enqueue(string.Format("NPC create failed {0} {1}", Info.Title, info.Name));
                          //  continue;
                        }

                        AddObject(new NPCObject(info, this));
                    }

                    for (int i = 0; i < Info.SafeZones.Count; i++)
                        CreateSafeZone(Info.SafeZones[i]);
                    CreateMine();
                    return true;
                }
            }
            catch (Exception ex)
            {
                SMain.Enqueue(ex);
            }

            SMain.Enqueue("Failed to Load Map: " + Info.FileName + " Title: " + Info.Title);
            return false;
        }

        private void CreateSafeZone(SafeZoneInfo info)
        {
            if (Settings.SafeZoneBorder)
            {
                for (int y = info.Location.Y - info.Size; y <= info.Location.Y + info.Size; y++)
                {
                    if (y < 0) continue;
                    if (y >= Height) break;
                    for (int x = info.Location.X - info.Size; x <= info.Location.X + info.Size; x += Math.Abs(y - info.Location.Y) == info.Size ? 1 : info.Size * 2)
                    {
                        if (x < 0) continue;
                        if (x >= Width) break;
                        if (!CanWalk(x, y)) continue;

                        SpellObject spell = new SpellObject
                        {
                            ExpireTime = long.MaxValue,
                            Spell = Spell.TrapHexagon,
                            TickSpeed = int.MaxValue,
                            CurrentLocation = new Point(x, y),
                            CurrentMap = this,
                            Decoration = true
                        };

                        AddObject(x, y, spell);

                        spell.Spawned();
                    }
                }
            }

            if (Settings.SafeZoneHealing)
            {
                for (int y = info.Location.Y - info.Size; y <= info.Location.Y + info.Size; y++)
                {
                    if (y < 0) continue;
                    if (y >= Height) break;
                    for (int x = info.Location.X - info.Size; x <= info.Location.X + info.Size; x++)
                    {
                        if (x < 0) continue;
                        if (x >= Width) break;
                        if (!CanWalk(x, y)) continue;

                        SpellObject spell = new SpellObject
                            {
                                ExpireTime = long.MaxValue,
                                Value = 25,
                                TickSpeed = 2000,
                                Spell = Spell.Healing,
                                CurrentLocation = new Point(x, y),
                                CurrentMap = this
                            };

                        AddObject(x, y, spell);

                        spell.Spawned();
                    }
                }
            }
        }

        public void AddObject(int x, int y, MapObject obj)
        {
            if (MapObjects[x, y] == null)
                MapObjects[x, y] = new List<MapObject>();
            
            MapObjects[x, y].Add(obj);
        }

        public void RemoveObject(int x, int y, MapObject obj)
        {
            if (MapObjects[x, y] != null)
            {
                if (!MapObjects[x, y].Remove(obj))
                {
                    SMain.Enqueue(string.Format("Remove failed {0} [{1}] [{2}] [{3}] [{4}]", Info.FileName, obj.Name, obj.ObjectID, x, y));

                    for (int i=0; i<Width; ++i)
                    for (int j=0; j<Height; ++j)
                    {
                        if (MapObjects[i, j] == null)
                            continue;

                            foreach (var o in MapObjects[i, j])
                            {
                                if (o == obj || o.ObjectID == obj.ObjectID)
                                    SMain.Enqueue(string.Format("Remove failed {0} [{1}] [{2}] [{3}] [{4}] [{5}]", Info.FileName, obj.Name, obj.ObjectID, i, j, o == obj));
                            }
                    }
                }
         
                if (MapObjects[x, y].Count == 0)
                    MapObjects[x, y] = null;
            }
        }

        public string DumpInfo()
        {
            int count = 0;
            for (int i = 0; i < Width; ++i)
            { 
                for (int j = 0; j < Height; ++j)
                {
                    if (MapObjects[i, j] == null)
                        continue;

                    foreach (var obj in MapObjects[i, j])
                    {
                        SMain.Enqueue(string.Format("{0} {1} {2}", i, j, obj.ObjectID));
                    }
                    count += MapObjects[i, j].Count;
                }
            }

            return string.Format("MapObjects count {0}, Objects count {1}", count, Objects.Count);
        }

        private void CreateMine()
        {
            if ((Info.MineIndex == 0) && (Info.MineZones.Count == 0)) return;
            Mine = new MineSet[Width, Height];
            StonesLeft = new byte[Width, Height];
            LastRegenTick = new long[Width, Height];
            if ((Info.MineIndex != 0) && (Settings.MineSetList.Count > Info.MineIndex - 1))
            {
                Settings.MineSetList[Info.MineIndex - 1].SetDrops(Envir.ItemInfoList);
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        Mine[i, j] = Settings.MineSetList[Info.MineIndex - 1];
            }
            if (Info.MineZones.Count > 0)
            {
                for (int i = 0; i < Info.MineZones.Count; i++)
                {
                    MineZone Zone = Info.MineZones[i];
                    if (Zone.Mine != 0)
                        Settings.MineSetList[Zone.Mine - 1].SetDrops(Envir.ItemInfoList);
                    if (Settings.MineSetList.Count < Zone.Mine) continue;
                    for (int x = Zone.Location.X - Zone.Size; x < Zone.Location.X + Zone.Size; x++)
                        for (int y = Zone.Location.Y - Zone.Size; y < Zone.Location.Y + Zone.Size; y++)
                        {
                            if ((x < 0) || (x >= Width) || (y < 0) || (y >= Height)) continue;
                            if (Zone.Mine == 0)
                                Mine[x, y] = null;
                            else
                                Mine[x, y] = Settings.MineSetList[Zone.Mine - 1];
                        }
                }
            }
        }

        public bool ValidPoint(Point location)
        {
            return location.X >= 0 && location.X < Width && location.Y >= 0 && location.Y < Height && CanWalk(location.X, location.Y);
        }
        public bool ValidPoint(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height && CanWalk(x, y);
        }

        public bool CheckDoorOpen(Point location)
        {
            if (!Doors.ContainsKey(location.X + Width * location.Y)) return true;
            if (Doors[location.X + Width * location.Y].DoorState != 2) return false;
            return true;
        }

        private void ProcessLight()
        {
            if ((Info.Lightning) && Envir.Time > LightningTime)
            {
                LightningTime = Envir.Time + Envir.Random.Next(3000, 15000);
                for (int i = Players.Count - 1; i >= 0; i--)
                {
                    PlayerObject player = Players[i];
                    Point Location;
                    if (Envir.Random.Next(4) == 0)
                    {
                        Location = player.CurrentLocation;
                    }
                    else
                        Location = new Point(player.CurrentLocation.X - 10 + Envir.Random.Next(20), player.CurrentLocation.Y - 10 + Envir.Random.Next(20));

                    if (!ValidPoint(Location)) continue;

                    SpellObject Lightning = null;
                    Lightning = new SpellObject
                    {
                        Spell = Spell.MapLightning,
                        Value = Envir.Random.Next(Info.LightningDamage),
                        ExpireTime = Envir.Time + (1000),
                        TickSpeed = 500,
                        Caster = null,
                        CurrentLocation = Location,
                        CurrentMap = this,
                        Direction = MirDirection.Up
                    };
                    AddObject(Lightning);
                    Lightning.Spawned();
                }
            }
        }

        private void ProcessFire()
        {
            if ((Info.Fire) && Envir.Time > FireTime)
            {
                FireTime = Envir.Time + Envir.Random.Next(3000, 15000);
                for (int i = Players.Count - 1; i >= 0; i--)
                {
                    PlayerObject player = Players[i];
                    Point Location;
                    if (Envir.Random.Next(4) == 0)
                    {
                        Location = player.CurrentLocation;

                    }
                    else
                        Location = new Point(player.CurrentLocation.X - 10 + Envir.Random.Next(20), player.CurrentLocation.Y - 10 + Envir.Random.Next(20));

                    if (!ValidPoint(Location)) continue;

                    SpellObject Lightning = null;
                    Lightning = new SpellObject
                    {
                        Spell = Spell.MapLava,
                        Value = Envir.Random.Next(Info.FireDamage),
                        ExpireTime = Envir.Time + (1000),
                        TickSpeed = 500,
                        Caster = null,
                        CurrentLocation = Location,
                        CurrentMap = this,
                        Direction = MirDirection.Up
                    };
                    AddObject(Lightning);
                    Lightning.Spawned();
                }
            }
        }

        private void ProcessDoors()
        {
            if (Players.Count <= 0)
                return;

            foreach (var en in Doors)
            {
                var Info = en.Value;
                if ((Info.DoorState == 2) && (Info.LastTick + 5000 < Envir.Time))
                {
                    Info.DoorState = 0;
                    //broadcast that door is closed
                    Broadcast(new S.Opendoor() { DoorIndex = Info.index, Close = true }, new Point(Info.X, Info.Y));
                }
            }
        }

        public void Process()
        {
            int ItemCount = 0;
            if (!Inactive())
            {
                int processCount = 0;
                if (Current == null)
                    Current = Objects.First;
                while (Current != null)
                {
                    LinkedListNode<MapObject> next = Current.Next;
                    if (Current.Value is ItemObject)
                        ++ItemCount;

                    if (Envir.Time > Current.Value.OperateTime)
                    {
                        try
                        {
                            Current.Value.Process();
                            ++processCount;
                        }
                        catch (Exception ex)
                        {
                            Envir.LogException(ex, Current.Value.Name);
                        }
                        Current.Value.SetOperateTime();
                        Envir.ProcessRealCount++;
                    }
                    Current = next;
                    if (processCount > 100)
                        break;
                }
            }

            ProcessRespawns();
            //process doors
            ProcessDoors();
            ProcessLight();
            ProcessFire();

            for (int i = 0; i < ActionList.Count; i++)
            {
                if (Envir.Time < ActionList[i].Time) continue;
                Process(ActionList[i]);
                ActionList.RemoveAt(i);
            }

            if (InactiveTime < Envir.Time)
            {
                if (Players.Count <= 0 && ItemCount <= 0)
                {
                    InactiveTime = Envir.Time + Settings.Minute;
                    bool isActive = Inactive();
                    InactiveCount++;
                    if (!isActive && Inactive())
                    {
                        SMain.Enqueue(string.Format("Map {0} RegionId {1} sleep ", Info.Title, Region));
                    }
                }
                else
                {
                    if (Inactive())
                        SMain.Enqueue(string.Format("Map {0}  RegionId {1} wakeup", Info.Title, Region));
                    InactiveCount = 0;
                }
            }
        }

        private void ProcessRespawns()
        {
            for (int i = 0; i < Respawns.Length; i++)
            {
                MapRespawn respawn = Respawns[i];
                RespawnInfo respawnInfo = respawn.Info;
                if ((respawnInfo.RespawnTicks != 0) && (Envir.RespawnTick.CurrentTickcounter < respawn.NextSpawnTick)) continue;
                if ((respawnInfo.RespawnTicks == 0) && (Envir.Time < respawn.RespawnTime)) continue;
                if (respawn.Count >= respawnInfo.Count) continue;

                int count = respawnInfo.Count - respawn.Count;

                bool Success = true;
                for (int c = 0; c < count; c++)
                {
                    if (!respawn.Spawn())
                        Success = false;
                    else
                        ++Envir.RespawnCount;
                }

                if (Success)
                {
                    long delay = Math.Max(1, respawnInfo.Delay - respawnInfo.RandomDelay + Envir.Random.Next(respawnInfo.RandomDelay * 2));
                    respawn.RespawnTime = Envir.Time + (delay * Settings.Minute) + Envir.Random.Next(Settings.Minute);
                    if (respawnInfo.RespawnTicks != 0)
                    {
                        respawn.NextSpawnTick = Envir.RespawnTick.CurrentTickcounter + (ulong)respawnInfo.RespawnTicks;
                        if (respawn.NextSpawnTick > long.MaxValue)//since nextspawntick is ulong this simple thing allows an easy way of preventing the counter from overflowing
                            respawn.NextSpawnTick -= long.MaxValue;
                    }
                }
                else
                {
                    respawn.RespawnTime = Envir.Time + Envir.Random.Next(Settings.Minute); // each time it fails to spawn, give it a 1 minute cooldown
                }
            }
        }

        public void Process(DelayedAction action)
        {
            switch (action.Type)
            {
                case DelayedType.Magic:
                    CompleteMagic(action.Params);
                    break;
                case DelayedType.Teleport:
                    CompleteTeleport(action.Params);
                    break;
                case DelayedType.Spawn:
                    MapObject obj = (MapObject)action.Params[0];

                    switch(obj.Race)
                    {
                        case ObjectType.Monster:
                            {
                                MonsterObject mob = (MonsterObject)action.Params[0];
                                mob.Spawn(this, (Point)action.Params[1]);
                                if (action.Params.Length > 2) ((MonsterObject)action.Params[2]).SlaveList.Add(mob);
                            }
                            break;
                        case ObjectType.Spell:
                            {
                                SpellObject spell = (SpellObject)action.Params[0];
                                AddObject(spell);
                                spell.Spawned();
                            }
                            break;
                    }
                    break;
            }
        }

        private void CompleteTeleport(IList<object> data)
        {
            if (data.Count < 4)
                return;

            MapObject mapObject = (MapObject)data[0];
            Point location = (Point)data[1];
            bool effects = (bool)data[2];
            byte effectnumber = (byte)data[3];

            mapObject.TeleportIn(location, effects, effectnumber);
        }

        private void CompleteMagic(IList<object> data)
        {
            bool train = false;
            PlayerObject player = (PlayerObject)data[0];
            UserMagic magic = (UserMagic)data[1];

            if (player == null || player.Info == null) return;

            int value, value2;
            Point location;
            MirDirection dir;
            MonsterObject monster;
            Point front;
            List<MapObject> cellObjects = null;

            switch (magic.Spell)
            {

                #region HellFire

                case Spell.HellFire:
                    value = (int)data[2];
                    dir = (MirDirection)data[4];
                    location = Functions.PointMove((Point)data[3], dir, 1);
                    int count = (int)data[5] - 1;

                    if (!ValidPoint(location)) return;

                    if (count > 0)
                    {
                        DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 100, player, magic, value, location, dir, count);
                        ActionList.Add(action);
                    }

                    List<MapObject> objects = GetCellObjects(location.X, location.Y);
                    if (objects == null) return;

                    for (int i = 0; i < objects.Count; i++)
                    {
                        MapObject target = objects[i];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (target.IsAttackTarget(player))
                                {
                                    if (target.Attacked(player, value, DefenceType.MAC, false) > 0)
                                        player.LevelMagic(magic);
                                    return;
                                }
                                break;
                        }
                    }
                    break;

                #endregion

                #region SummonSkeleton, SummonShinsu, SummonHolyDeva, ArcherSummons

                case Spell.SummonSkeleton:
                case Spell.SummonShinsu:
                case Spell.SummonHolyDeva:
                case Spell.SummonVampire:
                case Spell.SummonToad:
                case Spell.SummonSnakes:
                    monster = (MonsterObject)data[2];
                    front = (Point)data[3];

                    if (monster.Master.Dead) return;

                    if (ValidPoint(front))
                        monster.Spawn(this, front);
                    else
                        monster.Spawn(player.CurrentMap, player.CurrentLocation);

                    monster.Master.Pets.Add(monster);
                    break;

                #endregion

                #region FireBang, IceStorm
                    //todo:法师技能修改:冰咆哮和爆裂火焰
                case Spell.IceStorm:
                case Spell.FireBang:
                    value = (int)data[2];
                    location = (Point)data[3];

                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            List<MapObject> mapObjects = GetCellObjects(x, y);
                            if (mapObjects == null) continue;

                            for (int i = 0; i < mapObjects.Count; i++)
                            {
                                MapObject target = mapObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (target.IsAttackTarget(player))
                                        {
                                            if (target.Attacked(player, value, DefenceType.MAC, false) > 0)
                                                train = true;
                                        }
                                        break;
                                }
                            }

                        }

                    }

                    break;

                #endregion

                #region MassHiding

                case Spell.MassHiding:
                    value = (int)data[2];
                    location = (Point)data[3];

                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            List<MapObject> mapObjects = GetCellObjects(x, y);

                            if (mapObjects == null) continue;

                            for (int i = 0; i < mapObjects.Count; i++)
                            {
                                MapObject target = mapObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (target.IsFriendlyTarget(player))
                                        {
                                            for (int b = 0; b < target.Buffs.Count; b++)
                                                if (target.Buffs[b].Type == BuffType.Hiding) return;

                                            target.AddBuff(new Buff { Type = BuffType.Hiding, Caster = player, ExpireTime = Envir.Time + value * 1000 });
                                            target.OperateTime = 0;
                                            train = true;
                                        }
                                        break;
                                }
                            }

                        }

                    }

                    break;

                #endregion

                #region SoulShield, BlessedArmour

                case Spell.SoulShield:
                case Spell.BlessedArmour:
                    value = (int)data[2];
                    location = (Point)data[3];
                    BuffType type = magic.Spell == Spell.SoulShield ? BuffType.SoulShield : BuffType.BlessedArmour;

                    for (int y = location.Y - 3; y <= location.Y + 3; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 3; x <= location.X + 3; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (target.IsFriendlyTarget(player))
                                        {
                                            target.AddBuff(new Buff { Type = type, Caster = player, ExpireTime = Envir.Time + value * 1000, Values = new int[]{ target.Level / 7 + 4 } });
                                            target.OperateTime = 0;
                                            train = true;
                                        }
                                        break;
                                }
                            }

                        }

                    }

                    break;

                #endregion

                #region FireWall

                case Spell.FireWall:
                    value = (int)data[2];
                    location = (Point)data[3];

                    player.LevelMagic(magic);

                    if (ValidPoint(location))
                    {
                        cellObjects = GetCellObjects(location);

                        bool cast = true;
                        if (cellObjects != null)
                            for (int o = 0; o < cellObjects.Count; o++)
                            {
                                MapObject target = cellObjects[o];
                                if (target.Race != ObjectType.Spell || ((SpellObject)target).Spell != Spell.FireWall) continue;

                                cast = false;
                                break;
                            }

                        if (cast)
                        {
                            SpellObject ob = new SpellObject
                                {
                                    Spell = Spell.FireWall,
                                    Value = value,
                                    ExpireTime = Envir.Time + (10 + value / 2) * 1000,
                                    TickSpeed = 2000,
                                    Caster = player,
                                    CurrentLocation = location,
                                    CurrentMap = this,
                                };
                            AddObject(ob);
                            ob.Spawned();
                        }
                    }

                    dir = MirDirection.Up;
                    for (int i = 0; i < 4; i++)
                    {
                        location = Functions.PointMove((Point)data[3], dir, 1);
                        dir += 2;

                        if (!ValidPoint(location)) continue;

                        cellObjects = GetCellObjects(location);
                        bool cast = true;

                        if (cellObjects != null)
                            for (int o = 0; o < cellObjects.Count; o++)
                            {
                                MapObject target = cellObjects[o];
                                if (target.Race != ObjectType.Spell || ((SpellObject)target).Spell != Spell.FireWall) continue;

                                cast = false;
                                break;
                            }

                        if (!cast) continue;

                        SpellObject ob = new SpellObject
                        {
                            Spell = Spell.FireWall,
                            Value = value,
                            ExpireTime = Envir.Time + (10 + value / 2) * 1000,
                            TickSpeed = 2000,
                            Caster = player,
                            CurrentLocation = location,
                            CurrentMap = this,
                        };
                        AddObject(ob);
                        ob.Spawned();
                    }

                    break;

                #endregion

                #region Lightning

                case Spell.Lightning:
                    value = (int)data[2];
                    location = (Point)data[3];
                    dir = (MirDirection)data[4];

                    for (int i = 0; i < 8; i++)
                    {
                        location = Functions.PointMove(location, dir, 1);

                        if (!ValidPoint(location)) continue;

                        cellObjects = GetCellObjects(location);

                        if (cellObjects == null) continue;

                        for (int o = 0; o < cellObjects.Count; o++)
                        {
                            MapObject target = cellObjects[o];
                            if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) continue;

                            if (!target.IsAttackTarget(player)) continue;
                            if (target.Attacked(player, value, DefenceType.MAC, false) > 0)
                                train = true;
                            break;
                        }
                    }

                    break;

                #endregion

                #region HeavenlySword

                case Spell.HeavenlySword:
                    value = (int)data[2];
                    location = (Point)data[3];
                    dir = (MirDirection)data[4];

                    for (int i = 0; i < 3; i++)
                    {
                        location = Functions.PointMove(location, dir, 1);

                        if (!ValidPoint(location)) continue;

                        cellObjects = GetCellObjects(location);

                        if (cellObjects == null) continue;

                        for (int o = 0; o < cellObjects.Count; o++)
                        {
                            MapObject target = cellObjects[o];
                            if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) continue;

                            if (!target.IsAttackTarget(player)) continue;
                            if (target.Attacked(player, value, DefenceType.AC, false) > 0)
                                train = true;
                            break;
                        }
                    }

                    break;

                #endregion

                #region MassHealing

                case Spell.MassHealing:
                    value = (int)data[2];
                    location = (Point)data[3];

                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (target.IsFriendlyTarget(player))
                                        {
                                            if (target.Health >= target.MaxHealth) continue;
                                            target.HealAmount = (ushort)Math.Min(ushort.MaxValue, target.HealAmount + value);
                                            target.OperateTime = 0;
                                            train = true;
                                        }
                                        break;
                                }
                            }

                        }

                    }

                    break;

                #endregion

                #region ThunderStorm
                case Spell.ThunderStorm:
                case Spell.FlameField:
                case Spell.NapalmShot:
                    value = (int)data[2];
                    location = (Point)data[3];
                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (!target.IsAttackTarget(player)) break;

                                        if (target.Attacked(player, magic.Spell == Spell.ThunderStorm && !target.Undead ? value / 10 : value, DefenceType.MAC, false) <= 0)
                                        {
                                            if (target.Undead)
                                            {
                                                target.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = magic.Level + 2, TickSpeed = 1000 }, player);
                                            }
                                            break;
                                        }

                                        train = true;
                                        break;
                                }
                            }

                        }
                    }

                    break;

                case Spell.StormEscape:
                case Spell.StormEscape1:
                    value = (int)data[2];
                    location = (Point)data[3];
                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (!target.IsAttackTarget(player)) break;

                                        if (target.Attacked(player, value, DefenceType.MAC, false) >= 0)
                                        {
                                            target.ApplyPoison(new Poison { PType = PoisonType.LRParalysis, Duration = Math.Max(3, SMain.Envir.Random.Next(magic.Level + 1)), TickSpeed = 1000 }, player);
                                        }

                                        train = true;
                                        break;
                                }
                            }

                        }
                    }

                    break;

                #endregion

                case Spell.NapalmShot2:
                    value = (int)data[2];
                    location = (Point)data[3];

                    bool hasVampBuff = (player.Buffs.Where(ex => ex.Type == BuffType.VampireShot).ToList().Count() > 0);
                    bool hasPoisonBuff = (player.Buffs.Where(ex => ex.Type == BuffType.PoisonShot).ToList().Count() > 0);
                    if (hasVampBuff || hasPoisonBuff)
                        value = (int)1.3F * value;

                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (!target.IsAttackTarget(player)) break;

                                        if (hasPoisonBuff || hasVampBuff)
                                            target.Attacked(player, 2 * value, DefenceType.MAC, false);
                                        else
                                            target.Attacked(player, value, DefenceType.MAC, false);

                                        if (hasPoisonBuff && Envir.Random.Next(5) == 0)//Poison Effect 
                                        {
                                            target.ApplyPoison(new Poison
                                            {
                                                Duration = (value * 2) + (magic.Level + 1) * 7,
                                                Owner = player,
                                                PType = PoisonType.Green,
                                                TickSpeed = 2000,
                                                Value = value / 15 + magic.Level + 1 + Envir.Random.Next(player.PoisonAttack)
                                            }, player);
                                            target.OperateTime = 0;
                                        }

                                        train = true;
                                        break;
                                }
                            }

                            if (hasVampBuff)//Vampire Effect
                            {
                                if (player.VampAmount == 0) player.VampTime = Envir.Time + 1000;
                                player.VampAmount += (ushort)(value * (magic.Level + 1) * 0.25F);
                            }

                            if (hasVampBuff)//Vampire Effect
                            {
                                //cancel out buff
                                player.AddBuff(new Buff { Type = BuffType.VampireShot, Caster = player, ExpireTime = Envir.Time + 1000, Values = new int[] { value }, Visible = true, ObjectID = player.ObjectID });
                            }
                            if (hasPoisonBuff)//Poison Effect
                            {
                                //cancel out buff
                                player.AddBuff(new Buff { Type = BuffType.PoisonShot, Caster = player, ExpireTime = Envir.Time + 1000, Values = new int[] { value }, Visible = true, ObjectID = player.ObjectID });
                            }
                        }
                    }

                    break;
                #region LionRoar

                case Spell.LionRoar:
                    location = (Point)data[2];

                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                if (target.Race != ObjectType.Monster) continue;
                                //Only targets
                                if (!target.IsAttackTarget(player) || player.Level + 3 < target.Level) continue;
                                target.ApplyPoison(new Poison { PType = PoisonType.LRParalysis, Duration = magic.Level + 2, TickSpeed = 1000 }, player);
                                target.OperateTime = 0;
                                train = true;
                            }

                        }

                    }

                    break;

                #endregion

                #region PoisonCloud

                case Spell.PoisonCloud:
                    // 毒云第一段伤害高
                    value = (int)data[2];
                    location = (Point)data[3];
                    byte bonusdmg = (byte)data[4];
                    train = true;
                    bool show = true;

                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            bool cast = true;
                            if (cellObjects != null)
                                for (int o = 0; o < cellObjects.Count; o++)
                                {
                                    MapObject target = cellObjects[o];
                                    if (target.Race != ObjectType.Spell || ((SpellObject)target).Spell != Spell.PoisonCloud) continue;

                                    cast = false;
                                    break;
                                }

                            if (!cast) continue;

                            SpellObject ob = new SpellObject
                                {
                                    Spell = Spell.PoisonCloud,
                                    Value = value + bonusdmg,
                                    ExpireTime = Envir.Time + 6000,
                                    TickSpeed = 1000,
                                    Caster = player,
                                    CurrentLocation = new Point(x, y),
                                    CastLocation = location,
                                    Show = show,
                                    CurrentMap = this,
                                };

                            show = false;

                            AddObject(ob);
                            ob.Spawned();
                        }
                    } 

                    break;

                #endregion

                #region IceThrust

                case Spell.IceThrust:
                    {
                        location = (Point)data[2];
                        MirDirection direction = (MirDirection)data[3];

                        int nearDamage = (int)data[4];
                        int farDamage = (int)data[5];

                        int col = 3;
                        int row = 3;

                        Point[] loc = new Point[col]; //0 = left 1 = center 2 = right
                        loc[0] = Functions.PointMove(location, Functions.PreviousDir(direction), 1);
                        loc[1] = Functions.PointMove(location, direction, 1);
                        loc[2] = Functions.PointMove(location, Functions.NextDir(direction), 1);

                        for (int i = 0; i < col; i++)
                        {
                            Point startPoint = loc[i];
                            for (int j = 0; j < row; j++)
                            {
                                Point hitPoint = Functions.PointMove(startPoint, direction, j);

                                if (!ValidPoint(hitPoint)) continue;

                                cellObjects = GetCellObjects(hitPoint);
                                 
                                if (cellObjects == null) continue;

                                for (int k = 0; k < cellObjects.Count; k++)
                                {
                                    MapObject target = cellObjects[k];
                                    switch (target.Race)
                                    {
                                        case ObjectType.Monster:
                                        case ObjectType.Player:
                                            if (target.IsAttackTarget(player))
                                            {
                                                //Only targets
                                                if (target.Attacked(player, j <= 1 ? nearDamage : farDamage, DefenceType.MAC, false) > 0)
                                                {
                                                    if (player.Level + (target.Race == ObjectType.Player ? 2 : 10) >= target.Level && Envir.Random.Next(target.Race == ObjectType.Player ? 100 : 20) <= magic.Level)
                                                    {
                                                        target.ApplyPoison(new Poison
                                                        {
                                                            Owner = player,
                                                            Duration = target.Race == ObjectType.Player ? 4 : 5 + Envir.Random.Next(5),
                                                            PType = PoisonType.Slow,
                                                            TickSpeed = 1000,
                                                        }, player);
                                                        target.OperateTime = 0;
                                                    }

                                                    if (player.Level + (target.Race == ObjectType.Player ? 2 : 10) >= target.Level && Envir.Random.Next(target.Race == ObjectType.Player ? 100 : 40) <= magic.Level)
                                                    {
                                                        target.ApplyPoison(new Poison
                                                        {
                                                            Owner = player,
                                                            Duration = target.Race == ObjectType.Player ? 2 : 5 + Envir.Random.Next(player.Freezing),
                                                            PType = PoisonType.Frozen,
                                                            TickSpeed = 1000,
                                                        }, player);
                                                        target.OperateTime = 0;
                                                    }

                                                    train = true;
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    break;

                #endregion

                #region SlashingBurst

                case Spell.SlashingBurst:
                    value = (int)data[2];
                    location = (Point)data[3];
                    dir = (MirDirection)data[4];
                    count = (int)data[5];

                    for (int i = 0; i < count; i++)
                    {
                        location = Functions.PointMove(location, dir, 1);

                        if (!ValidPoint(location)) break;

                        cellObjects = GetCellObjects(location);

                        if (cellObjects == null) break;

                        for (int o = 0; o < cellObjects.Count; o++)
                        {
                            MapObject target = cellObjects[o];
                            if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) continue;

                            if (!target.IsAttackTarget(player)) continue;
                            if (target.Attacked(player, value, DefenceType.AC, false) > 0)
                                train = true;
                            break;
                        }
                    }

                    break;

                #endregion
                #region SlashingBurst
                case Spell.XiangLongGunFa:
                    value = (int)data[2];
                    location = (Point)data[3];
                    dir = (MirDirection)data[4];
                    count = (int)data[5];

                    for (int i = 0; i < count; i++)
                    {
                        location = Functions.PointMove(location, dir, 1);

                        if (!ValidPoint(location)) continue;

                        cellObjects = GetCellObjects(location);

                        if (cellObjects == null) continue;

                        for (int o = 0; o < cellObjects.Count; o++)
                        {
                            MapObject target = cellObjects[o];
                            if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) continue;

                            if (!target.IsAttackTarget(player)) continue;
                            if (target.Attacked(player, value, DefenceType.AC, false) > 0)
                                train = true;
                            break;
                        }
                    }
                    break;

                #endregion

                #region Mirroring

                case Spell.Mirroring:
                    monster = (MonsterObject)data[2];
                    front = (Point)data[3];
                    bool finish = (bool)data[4];

                    if (finish)
                    {
                        monster.Die();
                        return;
                    };

                    if (ValidPoint(front))
                        monster.Spawn(this, front);
                    else
                        monster.Spawn(player.CurrentMap, player.CurrentLocation);
                    break;

                #endregion

                #region Blizzard

                case Spell.Blizzard:
                    {
                        value = (int)data[2];
                        location = (Point)data[3];
                        int channelingId = (int)data[4];
                        if (channelingId != player.ChannelingId)
                            return;

                        train = true;
                        show = true;

                        for (int y = location.Y - 2; y <= location.Y + 2; y++)
                        {
                            if (y < 0) continue;
                            if (y >= Height) break;

                            for (int x = location.X - 2; x <= location.X + 2; x++)
                            {
                                if (x < 0) continue;
                                if (x >= Width) break;

                                cellObjects = GetCellObjects(x, y);

                                bool cast = true;
                                if (cellObjects != null)
                                    for (int o = 0; o < cellObjects.Count; o++)
                                    {
                                        MapObject target = cellObjects[o];
                                        if (target.Race != ObjectType.Spell || ((SpellObject)target).Spell != Spell.Blizzard) continue;

                                        cast = false;
                                        break;
                                    }

                                if (!cast) continue;

                                SpellObject ob = new SpellObject
                                {
                                    Spell = Spell.Blizzard,
                                    Value = value,
                                    ExpireTime = Envir.Time + 3000,
                                    TickSpeed = 1000,
                                    Caster = player,
                                    CurrentLocation = new Point(x, y),
                                    CastLocation = location,
                                    Show = show,
                                    CurrentMap = this,
                                    StartTime = Envir.Time + 800,
                                    CasterChannelingId = channelingId
                                };

                                show = false;

                                AddObject(ob);
                                ob.Spawned();
                            }
                        }
                    }
                    break;

                #endregion

                #region MeteorStrike

                case Spell.MeteorStrike:
                    {
                        value = (int)data[2];
                        location = (Point)data[3];
                        int channelingId = (int)data[4];
                        if (channelingId != player.ChannelingId)
                            return;

                        train = true;
                        show = true;

                        for (int y = location.Y - 2; y <= location.Y + 2; y++)
                        {
                            if (y < 0) continue;
                            if (y >= Height) break;

                            for (int x = location.X - 2; x <= location.X + 2; x++)
                            {
                                if (x < 0) continue;
                                if (x >= Width) break;

                                cellObjects = GetCellObjects(x, y);

                                bool cast = true;
                                if (cellObjects != null)
                                    for (int o = 0; o < cellObjects.Count; o++)
                                    {
                                        MapObject target = cellObjects[o];
                                        if (target.Race != ObjectType.Spell || ((SpellObject)target).Spell != Spell.MeteorStrike) continue;

                                        cast = false;
                                        break;
                                    }

                                if (!cast) continue;

                                SpellObject ob = new SpellObject
                                {
                                    Spell = Spell.MeteorStrike,
                                    Value = value,
                                    ExpireTime = Envir.Time + 5000,
                                    TickSpeed = 1000,
                                    Caster = player,
                                    CurrentLocation = new Point(x, y),
                                    CastLocation = location,
                                    Show = show,
                                    CurrentMap = this,
                                    StartTime = Envir.Time + 800,
                                    CasterChannelingId = channelingId
                                };

                                show = false;

                                AddObject(ob);
                                ob.Spawned();
                            }
                        }
                    }
                    break;

                #endregion

                #region TrapHexagon

                case Spell.TrapHexagon:
                    value = (int)data[2];
                    location = (Point)data[3];

                    MonsterObject centerTarget = null;

                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];

                                if (y == location.Y && x == location.X && target.Race == ObjectType.Monster)
                                {
                                    centerTarget = (MonsterObject)target;
                                }
                                
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                        if (target == null || !target.IsAttackTarget(player) || target.Node == null || target.Level > player.Level + 2) continue;

                                        MonsterObject mobTarget = (MonsterObject)target;

                                        if (centerTarget == null) centerTarget = mobTarget;

                                        mobTarget.ShockTime = Envir.Time + value;
                                        mobTarget.Target = null;
                                        break;
                                }
                            }

                        }
                    }

                    if (centerTarget == null) return;

                    for (byte i = 0; i < 8; i += 2)
                    {
                        Point startpoint = Functions.PointMove(location, (MirDirection)i, 2);
                        for (byte j = 0; j <= 4; j += 4)
                        {
                            MirDirection spawndirection = i == 0 || i == 4 ? MirDirection.Right : MirDirection.Up;
                            Point spawnpoint = Functions.PointMove(startpoint, spawndirection + j, 1);
                            if (spawnpoint.X <= 0 || spawnpoint.X > centerTarget.CurrentMap.Width) continue;
                            if (spawnpoint.Y <= 0 || spawnpoint.Y > centerTarget.CurrentMap.Height) continue;
                            SpellObject ob = new SpellObject
                            {
                                Spell = Spell.TrapHexagon,
                                ExpireTime = Envir.Time + value,
                                TickSpeed = 100,
                                Caster = player,
                                CurrentLocation = spawnpoint,
                                CastLocation = location,
                                CurrentMap = centerTarget.CurrentMap,
                                Target = centerTarget,
                            };

                            centerTarget.CurrentMap.AddObject(ob);
                            ob.Spawned();
                        }
                    }

                    train = true;

                    break;

                #endregion

                #region Curse

                case Spell.Curse:
                    value = (int)data[2];           //伤害
                    location = (Point)data[3];      //位置
                    value2 = (int)data[4];          //9
                    type = BuffType.Curse;

                    for (int y = location.Y - 3; y <= location.Y + 3; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 3; x <= location.X + 3; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);
                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        if (Envir.Random.Next(10) >= 4) continue;

                                        //Only targets
                                        if (target.IsAttackTarget(player))
                                        {
                                            target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = target.Race == ObjectType.Player ? value2 : value,
                                                TickSpeed = 1000, Value = value2 }, player);
                                            target.AddBuff(new Buff { Type = type, Caster = player, ExpireTime = Envir.Time + value * 1000, Values = new int[]{ value2 } });
                                            target.OperateTime = 0;
                                            train = true;
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    break;

                #endregion

                #region ExplosiveTrap

                case Spell.ExplosiveTrap:
                    value = (int)data[2];
                    front = (Point)data[3];

                    if (ValidPoint(front))
                    {
                        cellObjects = GetCellObjects(front);

                        bool cast = true;
                        if (cellObjects != null)
                            for (int o = 0; o < cellObjects.Count; o++)
                            {
                                MapObject target = cellObjects[o];
                                if (target.Race != ObjectType.Spell || (((SpellObject)target).Spell != Spell.FireWall && ((SpellObject)target).Spell != Spell.ExplosiveTrap)) continue;

                                cast = false;
                                break;
                            }

                        if (cast)
                        {
                            player.LevelMagic(magic);
                            for (int i = 0; i < 9; i++)
                            {
                                SpellObject ob = new SpellObject
                                {
                                    Spell = Spell.ExplosiveTrap,
                                    Value = value,
                                    ExpireTime = Envir.Time + 15000,
                                    TickSpeed = 1000,
                                    Caster = player,
                                    CurrentLocation = new Point(front.X - 1 + i % 3, front.Y - 1 + i / 3),
                                    CurrentMap = this,
                                    TrapIndex = i,
                                    Show = i == 4
                                };
                                AddObject(ob);
                                ob.Spawned();
                                player.ArcherTrapObjectsArray[i] = ob;
                            }
                        }
                    }
                    break;

                #endregion

                #region HealingCircle
                case Spell.HealingCircle:
                case Spell.HealingCircle2:
                    value = (int)data[2];
                    location = (Point)data[3];
                    long duration = (long)data[4];
                    train = true;
                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0)
                            continue;
                        if (y >= Height)
                            break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0)
                                continue;
                            if (x >= Width)
                                break;

                            if (!CanWalk(x, y))
                                continue;

                            SpellObject ob = new SpellObject
                            {
                                Spell = magic.Spell,
                                Value = value,
                                ExpireTime = Envir.Time + duration,
                                TickSpeed = 3000,
                                Caster = player,
                                CurrentLocation = new Point(x, y),
                                CastLocation = location,
                                Show = x == location.X && y == location.Y,
                                CurrentMap = this,
                            };

                            show = false;

                            AddObject(ob);
                            ob.Spawned();
                        }
                    }
                    break;
                #endregion

                #region Trap

                case Spell.Trap:
                    value = (int)data[2];
                    location = (Point)data[3];
                    MonsterObject selectTarget = null;

                    if (!ValidPoint(location)) break;

                    cellObjects = GetCellObjects(location);

                    if (cellObjects == null) break;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject target = cellObjects[i];
                        if (target.Race == ObjectType.Monster)
                        {
                            selectTarget = (MonsterObject)target;

                            if (selectTarget == null || !selectTarget.IsAttackTarget(player) || selectTarget.Node == null || selectTarget.Level >= player.Level + 2) continue;
                            selectTarget.ShockTime = Envir.Time + value;
                            selectTarget.Target = null;
                            break;
                        }
                    }

                    if (selectTarget == null) return;

                    if (location.X <= 0 || location.X > selectTarget.CurrentMap.Width) break;
                    if (location.Y <= 0 || location.Y > selectTarget.CurrentMap.Height) break;
                    SpellObject spellOb = new SpellObject
                    {
                        Spell = Spell.Trap,
                        ExpireTime = Envir.Time + value,
                        TickSpeed = 100,
                        Caster = player,
                        CurrentLocation = location,
                        CastLocation = location,
                        CurrentMap = selectTarget.CurrentMap,
                        Target = selectTarget,
                    };

                    selectTarget.CurrentMap.AddObject(spellOb);
                    spellOb.Spawned();

                    train = true;
                    break;

                #endregion

                #region OneWithNature

                case Spell.OneWithNature:
                    value = (int)data[2];
                    location = (Point)data[3];

                    hasVampBuff = (player.Buffs.Where(ex => ex.Type == BuffType.VampireShot).ToList().Count() > 0);
                    hasPoisonBuff = (player.Buffs.Where(ex => ex.Type == BuffType.PoisonShot).ToList().Count() > 0);

                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (!target.IsAttackTarget(player) || target.Dead) break;

                                        //knockback
                                        //int distance = 1 + Math.Max(0, magic.Level - 1) + Envir.Random.Next(2);
                                        //dir = Functions.DirectionFromPoint(location, target.CurrentLocation);
                                        //if(target.Level < player.Level)
                                        //    target.Pushed(player, dir, distance);// <--crashes server somehow?

                                        if (target.Attacked(player, value, DefenceType.MAC, false) <= 0) break;

                                        if (hasVampBuff)//Vampire Effect
                                        {
                                            if (player.VampAmount == 0) player.VampTime = Envir.Time + 1000;
                                            player.VampAmount += (ushort)(value * (magic.Level + 1) * 0.25F);
                                        }
                                        if (hasPoisonBuff)//Poison Effect
                                        {
                                            target.ApplyPoison(new Poison
                                            {
                                                Duration = (value * 2) + (magic.Level + 1) * 7,
                                                Owner = player,
                                                PType = PoisonType.Green,
                                                TickSpeed = 2000,
                                                Value = value / 15 + magic.Level + 1 + Envir.Random.Next(player.PoisonAttack)
                                            }, player);
                                            target.OperateTime = 0;
                                        }
                                        train = true;
                                        break;
                                }
                            }

                        }
                    }

                    if (hasVampBuff)//Vampire Effect
                    {
                        //cancel out buff
                        player.AddBuff(new Buff { Type = BuffType.VampireShot, Caster = player, ExpireTime = Envir.Time + 1000, Values = new int[]{ value }, Visible = true, ObjectID = player.ObjectID });
                    }
                    if (hasPoisonBuff)//Poison Effect
                    {
                        //cancel out buff
                        player.AddBuff(new Buff { Type = BuffType.PoisonShot, Caster = player, ExpireTime = Envir.Time + 1000, Values = new int[]{ value }, Visible = true, ObjectID = player.ObjectID });
                    }
                    break;

                #endregion

                #region Portal

                case Spell.Portal:                  
                    value = (int)data[2];
                    location = (Point)data[3];
                    value2 = (int)data[4];

                    spellOb = new SpellObject
                    {
                        Spell = Spell.Portal,
                        Value = value2,
                        ExpireTime = Envir.Time + value * 1000,
                        TickSpeed = 2000,
                        Caster = player,
                        CurrentLocation = location,
                        CurrentMap = this,
                    };

                    if (player.PortalObjectsArray[0] == null)
                    {
                        player.PortalObjectsArray[0] = spellOb;
                    }
                    else
                    {
                        player.PortalObjectsArray[1] = spellOb;
                        player.PortalObjectsArray[1].ExitMap = player.PortalObjectsArray[0].CurrentMap;
                        player.PortalObjectsArray[1].ExitCoord = player.PortalObjectsArray[0].CurrentLocation;

                        player.PortalObjectsArray[0].ExitMap = player.PortalObjectsArray[1].CurrentMap;
                        player.PortalObjectsArray[0].ExitCoord = player.PortalObjectsArray[1].CurrentLocation;
                    }

                    AddObject(spellOb);
                    spellOb.Spawned();
                    train = true;
                    break;

                #endregion

                #region DelayedExplosion

                case Spell.DelayedExplosion:
                    value = (int)data[2];
                    location = (Point)data[3];
                    MapObject ob1 = (MapObject)data[4];
                    bool inflect = (bool)data[5];

                    List<MapObject> targets = new List<MapObject>();
                    for (int y = location.Y - 1; y <= location.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 1; x <= location.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (target.IsAttackTarget(player))
                                        {
                                            if (target != ob1)
                                                targets.Add(target);
                                            if (target.Attacked(player, value, DefenceType.MAC, false) > 0)
                                                train = false;//wouldnt want to make the skill give twice the points
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    short inflectCnt = 0;
                    if (inflect && targets.Count > 0)
                    {
                        for (int i = 0; i < targets.Count; ++i)
                        {
                            MapObject ob2 = targets[i];
                            if (ob2 == null)
                                continue;

                            ++inflectCnt;
                            ob2.ApplyPoison(new Poison
                            {
                                Duration = 6,
                                Owner = player,
                                PType = PoisonType.DelayedExplosion,
                                TickSpeed = 1500,
                                Value = value,
                                Infect = true
                            }, player);
                            if (inflectCnt == 3)
                                break;
                        }
                    }

                    break;

                #endregion

                #region BattleCry

                case Spell.BattleCry:
                    location = (Point)data[2];

                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= Width) break;

                            cellObjects = GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject target = cellObjects[i];
                                if (target.Race != ObjectType.Monster) continue;

                                if (magic.Level == 0)
                                {
                                    if (Envir.Random.Next(60) >= 4) continue;
                                }
                                else if (magic.Level == 1)
                                {
                                    if (Envir.Random.Next(45) >= 3) continue;
                                }
                                else if (magic.Level == 2)
                                {
                                    if (Envir.Random.Next(30) >= 2) continue;
                                }
                                else if (magic.Level == 3)
                                {
                                    if (Envir.Random.Next(15) >= 1) continue;
                                }

                                if (((MonsterObject)target).Info.CoolEye == 100) continue;
                                target.Target = player;
                                target.OperateTime = 0;
                                train = true;
                            }
                        }
                    }
                    break;

                #endregion

                #region JinGangGunFa
                case Spell.JinGangGunFa:
                    {
                        value = (int)data[2];
                        location = (Point)data[3];
                        dir = (MirDirection)data[4];

                        if (Envir.Random.Next(0, 100) <= (1 + player.Luck))
                            value += value;//crit should do something like double dmg, not double max dc dmg!

                        MirDirection old = dir;
                        dir = Functions.PreviousDir(dir);

                        DefenceType defence = DefenceType.MACAgility;
                        for (int i = 0; i < 4; i++)
                        {
                            Point target = Functions.PointMove(location, dir, 1);
                            if (dir == old)
                                defence = DefenceType.None;
                            dir = Functions.NextDir(dir);

                            if (!ValidPoint(target)) continue;

                            cellObjects = GetCellObjects(target);

                            if (cellObjects == null) continue;

                            for (int o = 0; o < cellObjects.Count; o++)
                            {
                                MapObject ob = cellObjects[o];
                                if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                                if (!ob.IsAttackTarget(player)) continue;

                                ob.Attacked(player, value, defence, false);
                                train = true;
                                break;
                            }
                        }
                    }

                    break;

                #endregion

                #region MoonMist

                case Spell.MoonMist:
                case Spell.MoonMist2:
                    {
                        value = (int)data[2];
                        location = (Point)data[3];
                        for (int y = location.Y - 2; y <= location.Y + 2; y++)
                        {
                            if (y < 0) continue;
                            if (y >= Height) break;

                            for (int x = location.X - 2; x <= location.X + 2; x++)
                            {
                                if (x < 0) continue;
                                if (x >= Width) break;

                                cellObjects = GetCellObjects(x, y);

                                if (cellObjects == null) continue;

                                for (int i = 0; i < cellObjects.Count; i++)
                                {
                                    MapObject target = cellObjects[i];
                                    switch (target.Race)
                                    {
                                        case ObjectType.Monster:
                                        case ObjectType.Player:
                                            //Only targets
                                            if (!target.IsAttackTarget(player)) break;

                                            target.Attacked(player, value, DefenceType.AC, false);

                                            if (magic.Spell == Spell.MoonMist2)
                                                target.ApplyPoison(new Poison { PType = PoisonType.LRParalysis, Duration = Math.Max(3, SMain.Envir.Random.Next(magic.Level + 1)), TickSpeed = 1000 }, player);
                                            break;
                                    }
                                }

                            }
                        }
                    }

                    break;

                #endregion

            }

            if (train)
                player.LevelMagic(magic);

        }

        public List<MapObject> GetCellObjects(Point pt)
        {
            return GetCellObjects(pt.X, pt.Y);
        }

        public List<MapObject> GetCellObjects(int x, int y)
        {
            return MapObjects[x, y];
        }

        public void AddObject(MapObject ob)
        {
            if (ob.Race == ObjectType.Player)
            {
                Players.Add((PlayerObject)ob);
                InactiveTime = Envir.Time;
            }
            if (ob.Race == ObjectType.Merchant)
                NPCs.Add((NPCObject)ob);

            AddObject(ob.CurrentLocation.X, ob.CurrentLocation.Y, ob);
        }

        public void RemoveObject(MapObject ob)
        {
            if (ob.Race == ObjectType.Player) Players.Remove((PlayerObject)ob);
            if (ob.Race == ObjectType.Merchant) NPCs.Remove((NPCObject)ob);

            RemoveObject(ob.CurrentLocation.X, ob.CurrentLocation.Y, ob);
        }

        public SafeZoneInfo GetSafeZone(Point location)
        {
            for (int i = 0; i < Info.SafeZones.Count; i++)
            {
                SafeZoneInfo szi = Info.SafeZones[i];
                if (Functions.InRange(szi.Location, location, szi.Size))
                    return szi;
            }
            return null;
        }

        public ConquestObject GetConquest(Point location)
        {
            for (int i = 0; i < Conquest.Count; i++)
            {
                ConquestObject swi = Conquest[i];

                if ((swi.Info.FullMap || Functions.InRange(swi.Info.Location, location, swi.Info.Size)) && swi.WarIsOn)
                    return swi;
            }
            return null;
        }

        public void Broadcast(Packet p, Point location)
        {
            if (p == null) return;

            for (int i = Players.Count - 1; i >= 0; i--)
            {
                PlayerObject player = Players[i];

                if (Functions.InRange(location, player.CurrentLocation, Globals.DataRange))
                    player.Enqueue(p);
                    
            }
        }

        public void BroadcastNPC(Packet p, Point location)
        {
            if (p == null) return;

            for (int i = Players.Count - 1; i >= 0; i--)
            {
                PlayerObject player = Players[i];

                if (Functions.InRange(location, player.CurrentLocation, Globals.DataRange))
                    player.Enqueue(p);

            }
        }


        public void Broadcast(Packet p, Point location, PlayerObject Player)
        {
            if (p == null) return;

            if (Functions.InRange(location, Player.CurrentLocation, Globals.DataRange))
            {
                Player.Enqueue(p);
            }    
        }

        public bool Inactive(int count = 5)
        {
            //temporary test for server speed. Stop certain processes if no players.
            if (InactiveCount > count) return true;

            return false;
        }

        public bool CanWalk(int x, int y)
        {
            return CellAttrs[x , y] == CellAttribute.Walk;
        }
        public bool CanWalk(Point location)
        {
            return CanWalk(location.X, location.Y);
        }

        // TODO
        internal List<MapObject> GetAllObjects()
        {
            List<MapObject> list = new List<MapObject>();
            foreach (List<MapObject> mapObjects in MapObjects)
            {
                if (mapObjects != null)
                list.AddRange(mapObjects);
            }

            return list;
        }

        internal bool CanFishing(int x, int y)
        {
            return FishingAttrs.ContainsKey(x + y * Width);
        }

        internal sbyte GetFishingAttribute(int x, int y)
        {
            if (FishingAttrs.ContainsKey(x + y * Width))
                return FishingAttrs[x + y * Width];

            return 0;
        }

        internal MineSet GetMineSpot(int x, int y)
        {
            return Mine[x, y];
        }

        internal void ReduceStonesLeft(int x, int y)
        {
            StonesLeft[x, y] -= 1;
        }

        internal byte GetStoneLeft(int x, int y)
        {
            if (x < Width && y < Height && x >=0 && y >= 0)
                return StonesLeft[x, y];

            return 0;
        }

        internal void ProcessMineRegen(MineSet Mine, int x, int y)
        {
            if (x < Width && y < Height && x >= 0 && y >= 0)
            {
                if (Envir.Time > LastRegenTick[x, y])
                {
                    LastRegenTick[x, y] = Envir.Time + Mine.SpotRegenRate * 60 * 1000;
                    StonesLeft[x, y] = (byte)Envir.Random.Next(Mine.MaxStones);
                }
            }
        }
    }

    public class MapRespawn
    {
        public RespawnInfo Info;
        public MonsterInfo Monster;
        public Map Map;
        public int Count;
        public long RespawnTime;
        public ulong NextSpawnTick;

        public List<RouteInfo> Route;

        public MapRespawn(RespawnInfo info)
        {
            Info = info;
            Monster = SMain.Envir.GetMonsterInfo(info.MonsterIndex);

            LoadRoutes();
        }

        public bool Spawn()
        {
            if (Map == null)
                return false;

            Point point;
            point = new Point(Info.Location.X + SMain.Envir.Random.Next(-Info.Spread, Info.Spread + 1),
                                        Info.Location.Y + SMain.Envir.Random.Next(-Info.Spread, Info.Spread + 1));

            if (!Map.ValidPoint(point)) return false;

            MonsterObject ob = MonsterObject.GetMonster(Monster);
            if (ob == null) return true;
            return ob.Spawn(this, point);
        }

        public void LoadRoutes()
        {
            Route = new List<RouteInfo>();

            if (string.IsNullOrEmpty(Info.RoutePath)) return;

            string fileName = Path.Combine(Settings.RoutePath, Info.RoutePath + ".txt");

            if (!File.Exists(fileName)) return;

            List<string> lines = File.ReadAllLines(fileName).ToList();

            foreach (string line in lines)
            {
                RouteInfo info = RouteInfo.FromText(line);

                if (info == null) continue;

                Route.Add(info);
            }
        }
    }
}
