using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirSounds;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Effect = Client.MirObjects.Effect;
using S = ServerPackets;
using C = ClientPackets;
using System.IO;

namespace Client.MirScenes.Dialogs
{
    public sealed class MapControl : MirControl
    {
        public static UserObject User
        {
            get { return MapObject.User; }
            set { MapObject.User = value; }
        }

        public static List<MapObject> Objects = new List<MapObject>();

        public const int CellWidth = 48;
        public const int CellHeight = 32;

        // 屏幕网格的中心
        public static int OffSetX;
        public static int OffSetY;

        // 屏幕网格中心的+4
        public static int ViewRangeX;
        public static int ViewRangeY;

        public static Point MapLocation
        {
            get { return GameScene.User == null ? Point.Empty : new Point(MouseLocation.X / CellWidth - OffSetX, MouseLocation.Y / CellHeight - OffSetY).Add(GameScene.User.CurrentLocation); }
        }

        public static MouseButtons MapButtons;
        public static Point MouseLocation;
        public long InputDelay;
        public static long NextAction;

        public CellInfo[,] M2CellInfo;
        public List<Door> Doors = new List<Door>();
        public int Width, Height;

        public string FileName = String.Empty;
        public string Title = String.Empty;
        public ushort MiniMap, BigMap, Music, SetMusic;
        public LightSetting Lights;
        public bool Lightning, Fire;
        public byte MapDarkLight;
        public long LightningTime, FireTime;

        public bool FloorValid, LightsValid;

        private Texture _floorTexture, _lightTexture;
        private Surface _floorSurface, _lightSurface;

        private static bool _awakeningAction;
        public static bool AwakeningAction
        {
            get { return _awakeningAction; }
            set
            {
                if (_awakeningAction == value) return;
                _awakeningAction = value;
            }
        }

        private static bool _autoRun;
        public static bool AutoRun
        {
            get { return _autoRun; }
            set
            {
                if (_autoRun == value) return;
                _autoRun = value;
                if (GameScene.Scene != null)
                    GameScene.Scene.ChatDialog.ReceiveChat(value ? CMain.Tr("[AutoRun: On]") : CMain.Tr("[AutoRun: Off]"), ChatType.Hint);
            }
        }
        public static bool AutoHit;

        public int AnimationCount;

        public static List<Effect> Effects = new List<Effect>();

        //-----------------------------------------------------------------------------------
        private bool _autoPath;
        public bool AutoPath
        {
            get
            {
                return _autoPath;
            }
            set
            {
                if (_autoPath == value) return;
                _autoPath = value;

                if (!_autoPath)
                    CurrentPath = null;

                if (GameScene.Scene != null)
                    GameScene.Scene.ChatDialog.ReceiveChat(value ? CMain.Tr("[AutoPath: On]") : CMain.Tr("[AutoPath: Off]"), ChatType.Hint);
            }
        }

        public PathFinder PathFinder = null;
        public List<Node> CurrentPath = null;
        //----------------------------------------------------------------------------------

        public MapControl()
        {
            MapButtons = MouseButtons.None;

            OffSetX = Settings.ScreenWidth / 2 / CellWidth;
            OffSetY = Settings.ScreenHeight / 2 / CellHeight - 1;

            ViewRangeX = OffSetX + 6;
            ViewRangeY = OffSetY + 5;

            Size = new Size(Settings.ScreenWidth, Settings.ScreenHeight);
            DrawControlTexture = true;
            BackColour = Color.Black;

            MouseDown += OnMouseDown;
            MouseMove += (o, e) =>
            {
                MouseLocation = e.Location;
                MouseLocation.Y = MouseLocation.Y * Settings.ScreenHeight / Program.Form.ClientSize.Height;
                MouseLocation.X = MouseLocation.X * Settings.ScreenWidth / Program.Form.ClientSize.Width;
            };
            Click += OnMouseClick;
        }

        public void LoadMap()
        {
            GameScene.Scene.NPCDialog.Hide();
            Objects.Clear();
        //    Effects.Clear();
            Doors.Clear();

            if (User != null)
                Objects.Add(User);

            MapObject.MouseObject = null;
            MapObject.TargetObject = null;
            MapObject.MagicObject = null;
            MapObject.GlobalDisplayOffset = new Point(0, 0);

            if (!File.Exists(FileName))
            {
                CMain.TaskDownload(FileName, () =>
                {
                    Reload();
                });
            }

            MapReader Map = new MapReader(FileName);
            M2CellInfo = Map.MapCells;
            Width = Map.Width;
            Height = Map.Height;

            try
            {
                if (SetMusic != Music)
                {
                    if (SoundManager.Device != null)
                        SoundManager.Device.Dispose();
                    SoundManager.Create();
                    SoundManager.PlayMusic(Music, true);
                }
            }
            catch (Exception)
            {
                // Do nothing. index was not valid.
            }

            SetMusic = Music;
            SoundList.Music = Music;

            PathFinder = new PathFinder(this);
        }

        public void Reload()
        {
            MapReader Map = new MapReader(FileName);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (M2CellInfo[x, y].CellObjects != null && x < Map.Width && y < Map.Height)
                        Map.MapCells[x, y].CellObjects = M2CellInfo[x, y].CellObjects;
                }

            M2CellInfo = Map.MapCells;
            Width = Map.Width;
            Height = Map.Height;
            PathFinder = new PathFinder(this);
        }

        public void Process()
        {
            Processdoors();
            User.Process();

            for (int i = Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = Objects[i];
                if (ob == User) continue;
                //  if (ob.ActionFeed.Count > 0 || ob.Effects.Count > 0 || GameScene.CanMove || CMain.Time >= ob.NextMotion)
                ob.Process();
            }

            for (int i = Effects.Count - 1; i >= 0; i--)
                Effects[i].Process();

            if (MapObject.TargetObject != null && MapObject.TargetObject is MonsterObject && MapObject.TargetObject.AI == 64)
                MapObject.TargetObject = null;
            if (MapObject.MagicObject != null && MapObject.MagicObject is MonsterObject && MapObject.MagicObject.AI == 64)
                MapObject.MagicObject = null;

            CheckInput();

            MapObject bestmouseobject = null;
            for (int y = MapLocation.Y + 2; y >= MapLocation.Y - 2; y--)
            {
                if (y >= Height) continue;
                if (y < 0) break;
                for (int x = MapLocation.X + 2; x >= MapLocation.X - 2; x--)
                {
                    if (x >= Width) continue;
                    if (x < 0) break;
                    CellInfo cell = M2CellInfo[x, y];
                    if (cell.CellObjects == null) continue;

                    for (int i = cell.CellObjects.Count - 1; i >= 0; i--)
                    {
                        MapObject ob = cell.CellObjects[i];
                        if (ob == MapObject.User || !ob.MouseOver(CMain.MPoint)) continue;

                        if (MapObject.MouseObject != ob)
                        {
                            if (ob.Dead)
                            {
                                if (!Settings.TargetDead && GameScene.TargetDeadTime <= CMain.Time) continue;

                                bestmouseobject = ob;
                                //continue;
                            }
                            MapObject.MouseObject = ob;
                            Redraw();
                        }
                        if (bestmouseobject != null && MapObject.MouseObject == null)
                        {
                            MapObject.MouseObject = bestmouseobject;
                            Redraw();
                        }
                        return;
                    }
                }
            }


            if (MapObject.MouseObject != null)
            {
                MapObject.MouseObject = null;
                Redraw();
            }
        }

        public static MapObject GetObject(uint targetID)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                MapObject ob = Objects[i];
                if (ob.ObjectID != targetID) continue;
                return ob;
            }
            return null;
        }

        public override void Draw()
        {
            //Do nothing.
        }

        protected override void CreateTexture()
        {
            if (User == null) return;

            if (!FloorValid)
                DrawFloor();

            if (ControlTexture != null && !ControlTexture.Disposed && Size != TextureSize)
                ControlTexture.Dispose();

            if (ControlTexture == null || ControlTexture.Disposed)
            {
                DXManager.ControlList.Add(this);
                ControlTexture = new Texture(DXManager.Device, Size.Width, Size.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                ControlTexture.Disposing += ControlTexture_Disposing;
                TextureSize = Size;
            }

            Surface oldSurface = DXManager.CurrentSurface;
            Surface surface = ControlTexture.GetSurfaceLevel(0);

            if (User.Dead)
                DXManager.SetGrayscale(true);
            else
                DXManager.SetGrayscale(false);

            DXManager.SetSurface(surface);
            DXManager.Device.Clear(ClearFlags.Target, BackColour, 0, 0);

            DrawBackground();

            if (FloorValid)
                DXManager.Sprite.Draw2D(_floorTexture, Point.Empty, 0F, Point.Empty, Color.White);

            DrawObjects();

            //Render Death, 

            LightSetting setting = Lights == LightSetting.Normal ? GameScene.Scene.Lights : Lights;
            if (setting != LightSetting.Day)
                DrawLights(setting);

            if (Settings.DropView || GameScene.DropViewTime > CMain.Time)
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    ItemObject ob = Objects[i] as ItemObject;
                    if (ob == null) continue;

                    if (!ob.MouseOver(MouseLocation))
                        ob.DrawName();
                }
            }

            if (MapObject.MouseObject != null && !(MapObject.MouseObject is ItemObject))
                MapObject.MouseObject.DrawName();

            int offSet = 0;
            for (int i = 0; i < Objects.Count; i++)
            {
                ItemObject ob = Objects[i] as ItemObject;
                if (ob == null) continue;

                if (!ob.MouseOver(MouseLocation)) continue;
                ob.DrawName(offSet);
                offSet -= ob.NameLabel.Size.Height + (ob.NameLabel.Border ? 1 : 0);
            }

            if (MapObject.User.MouseOver(MouseLocation))
                MapObject.User.DrawName();

            DXManager.SetSurface(oldSurface);
            surface.Dispose();

            TextureValid = true;
        }
        protected internal override void DrawControl()
        {
            if (!DrawControlTexture)
                return;

            if (!TextureValid)
                CreateTexture();

            if (ControlTexture == null || ControlTexture.Disposed)
                return;

            float oldOpacity = DXManager.Opacity;

            DXManager.SetOpacity(Opacity);
            DXManager.Sprite.Draw2D(ControlTexture, Point.Empty, 0F, Point.Empty, Color.White);
            DXManager.SetOpacity(oldOpacity);

            CleanTime = CMain.Time + Settings.CleanDelay;
        }

        private void DrawFloor()
        {
            if (!Settings.DrawFloor)
                return;

            if (_floorTexture == null || _floorTexture.Disposed)
            {
                _floorTexture = new Texture(DXManager.Device, Settings.ScreenWidth, Settings.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                _floorTexture.Disposing += FloorTexture_Disposing;
                _floorSurface = _floorTexture.GetSurfaceLevel(0);
            }


            Surface oldSurface = DXManager.CurrentSurface;

            DXManager.SetSurface(_floorSurface);
            DXManager.Device.Clear(ClearFlags.Target, Color.Empty, 0, 0); //Color.Black

            int index;
            int drawY, drawX;

            int centerX = User.Movement.X + MapObject.GlobalDisplayOffset.X / CellWidth;
            int centerY = User.Movement.Y + MapObject.GlobalDisplayOffset.Y / CellHeight;
            int offsetX = -MapObject.GlobalDisplayOffset.X % CellWidth;
            int offsetY = -MapObject.GlobalDisplayOffset.Y % CellHeight;

            if (Settings.DrawBack)
                DrawBack(centerX, centerY, offsetX, offsetY);
           
            if (Settings.DrawMiddle)
                DrawMiddle(centerX, centerY, offsetX, offsetY);

            if (Settings.DrawFront)
                DrawFront(centerX, centerY, offsetX, offsetY);

            DXManager.SetSurface(oldSurface);

            FloorValid = true;
        }

        private void DrawFront(int centerX, int centerY, int offsetX, int offsetY)
        {
            for (int y = centerY - ViewRangeY; y <= centerY + ViewRangeY + 5; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                int drawY = (y - centerY + OffSetY) * CellHeight + User.OffSetMove.Y + offsetY; //Moving OffSet

                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int drawX = (x - centerX + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X + offsetX; //Moving OffSet

                    int index = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;
                    if (index == -1) continue;
                    int fileIndex = M2CellInfo[x, y].FrontIndex;
                    if (fileIndex == -1) continue;
                    Size s = Libraries.MapLibs[fileIndex].GetSize(index);
                    if (fileIndex == 200) continue; //fixes random bad spots on old school 4.map
                    if (M2CellInfo[x, y].DoorIndex > 0)
                    {
                        Door DoorInfo = GetDoor(M2CellInfo[x, y].DoorIndex);
                        if (DoorInfo == null)
                        {
                            DoorInfo = new Door() { index = M2CellInfo[x, y].DoorIndex, DoorState = 0, ImageIndex = 0, LastTick = CMain.Time };
                            Doors.Add(DoorInfo);
                        }
                        else
                        {
                            if (DoorInfo.DoorState != 0)
                            {
                                index += (DoorInfo.ImageIndex + 1) * M2CellInfo[x, y].DoorOffset;//'bad' code if you want to use animation but it's gonna depend on the animation > has to be custom designed for the animtion
                            }
                        }
                    }

                    if (index < 0 || ((s.Width != CellWidth || s.Height != CellHeight) && ((s.Width != CellWidth * 2) || (s.Height != CellHeight * 2)))) continue;
                    Libraries.MapLibs[fileIndex].Draw(index, drawX, drawY);
                }
            }
        }

        private void DrawMiddle(int centerX, int centerY, int offsetX, int offsetY)
        {
            for (int y = centerY - ViewRangeY; y <= centerY + ViewRangeY + 5; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                int drawY = (y - centerY + OffSetY) * CellHeight + User.OffSetMove.Y + offsetY; //Moving OffSet

                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int drawX = (x - centerX + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X + offsetX; //Moving OffSet

                    int index = M2CellInfo[x, y].MiddleImage - 1;

                    if ((index < 0) || (M2CellInfo[x, y].MiddleIndex == -1)) continue;
                    if (M2CellInfo[x, y].MiddleIndex > 199)
                    {//mir3 mid layer is same level as front layer not real middle + it cant draw index -1 so 2 birds in one stone :p
                        Size s = Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].GetSize(index);

                        if (s.Width != CellWidth || s.Height != CellHeight) continue;
                    }
                    Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].Draw(index, drawX, drawY);
                }
            }
        }

        private void DrawBack(int centerX, int centerY, int offsetX, int offsetY)
        {
            for (int y = centerY - ViewRangeY; y <= centerY + ViewRangeY; y++)
            {
                if (y <= 0 || y % 2 == 1) continue;
                if (y >= Height) break;
                int drawY = (y - centerY + OffSetY) * CellHeight + User.OffSetMove.Y + offsetY; //Moving OffSet

                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x <= 0 || x % 2 == 1) continue;
                    if (x >= Width) break;
                    int drawX = (x - centerX + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X + offsetX; //Moving OffSet
                    if ((M2CellInfo[x, y].BackImage == 0) || (M2CellInfo[x, y].BackIndex == -1)) continue;
                    int index = (M2CellInfo[x, y].BackImage & 0x1FFFF) - 1;
                    Libraries.MapLibs[M2CellInfo[x, y].BackIndex].Draw(index, drawX, drawY);
                }
            }
        }

        private void DrawBackground()
        {
            string cleanFilename = FileName.Replace(Settings.MapPath, "");

            if (cleanFilename.StartsWith("ID1") || cleanFilename.StartsWith("ID2"))
            {
                Libraries.Background.Draw(10, 0, 0); //mountains
            }
            else if (cleanFilename.StartsWith("ID3_013"))
            {
                Libraries.Background.Draw(22, 0, 0); //desert
            }
            else if (cleanFilename.StartsWith("ID3_015"))
            {
                Libraries.Background.Draw(23, 0, 0); //greatwall
            }
            else if (cleanFilename.StartsWith("ID3_023") || cleanFilename.StartsWith("ID3_025"))
            {
                Libraries.Background.Draw(21, 0, 0); //village entrance
            }
        }

        private void DrawObjects()
        {
            int centerX = User.Movement.X + MapObject.GlobalDisplayOffset.X / CellWidth;
            int centerY = User.Movement.Y + MapObject.GlobalDisplayOffset.Y / CellHeight;
            int offsetX = -MapObject.GlobalDisplayOffset.X % CellWidth;
            int offsetY = -MapObject.GlobalDisplayOffset.Y % CellHeight;

            for (int y = centerY - ViewRangeY; y <= centerY + ViewRangeY + 25; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    M2CellInfo[x, y].DrawDeadObjects();
                }
            }

            for (int y = centerY  - ViewRangeY; y <= centerY + ViewRangeY + 25; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                int drawY = (y - centerY + OffSetY + 1) * CellHeight + User.OffSetMove.Y + offsetY;

                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int drawX = (x - centerX + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X + offsetX;
                    int index;
                    byte animation;
                    bool blend;
                    Size s;
                    #region Draw shanda's tile animation layer
                    index = M2CellInfo[x, y].TileAnimationImage;
                    animation = M2CellInfo[x, y].TileAnimationFrames;
                    if ((index > 0) & (animation > 0))
                    {
                        index--;
                        int animationoffset = M2CellInfo[x, y].TileAnimationOffset ^ 0x2000;
                        index += animationoffset * (AnimationCount % animation);
                        if (Settings.DrawShandaAnim)
                             Libraries.MapLibs[190].DrawUp(index, drawX, drawY);
                    }

                    #endregion

                    #region Draw mir3 middle layer
                    if ((M2CellInfo[x, y].MiddleIndex > 199) && (M2CellInfo[x, y].MiddleIndex != -1))
                    {
                        index = M2CellInfo[x, y].MiddleImage - 1;
                        if (index > 0)
                        {
                            animation = M2CellInfo[x, y].MiddleAnimationFrame;
                            blend = false;
                            if ((animation > 0) && (animation < 255))
                            {
                                if ((animation & 0x0f) > 0)
                                {
                                    blend = true;
                                    animation &= 0x0f;
                                }
                                if (animation > 0)
                                {
                                    byte animationTick = M2CellInfo[x, y].MiddleAnimationTick;
                                    index += (AnimationCount % (animation + (animation * animationTick))) / (1 + animationTick);

                                    if (blend && (animation == 10 || animation == 8)) //diamond mines, abyss blends
                                    {
                                        Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUpBlend(index, new Point(drawX, drawY));
                                    }
                                    else
                                    {
                                        Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUp(index, drawX, drawY);
                                    }
                                }
                            }
                            s = Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].GetSize(index);
                            if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != (CellWidth * 2) || s.Height != (CellHeight * 2)) && !blend)
                            {
                                if (Settings.DrawMir3Middle)
                                    Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUp(index, drawX, drawY);
                            }
                        }
                    }
                    #endregion

                    #region Draw front layer
                    index = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;

                    if (index < 0) continue;

                    int fileIndex = M2CellInfo[x, y].FrontIndex;
                    if (fileIndex == -1) continue;
                    animation = M2CellInfo[x, y].FrontAnimationFrame;

                    if ((animation & 0x80) > 0)
                    {
                        blend = true;
                        animation &= 0x7F;
                    }
                    else
                        blend = false;


                    if (animation > 0)
                    {
                        byte animationTick = M2CellInfo[x, y].FrontAnimationTick;
                        index += (AnimationCount % (animation + (animation * animationTick))) / (1 + animationTick);
                    }


                    if (M2CellInfo[x, y].DoorIndex > 0)
                    {
                        Door DoorInfo = GetDoor(M2CellInfo[x, y].DoorIndex);
                        if (DoorInfo == null)
                        {
                            DoorInfo = new Door() { index = M2CellInfo[x, y].DoorIndex, DoorState = 0, ImageIndex = 0, LastTick = CMain.Time };
                            Doors.Add(DoorInfo);
                        }
                        else
                        {
                            if (DoorInfo.DoorState != 0)
                            {
                                index += (DoorInfo.ImageIndex + 1) * M2CellInfo[x, y].DoorOffset;//'bad' code if you want to use animation but it's gonna depend on the animation > has to be custom designed for the animtion
                            }
                        }
                    }

                    s = Libraries.MapLibs[fileIndex].GetSize(index);
                    if (s.Width == CellWidth && s.Height == CellHeight && animation == 0) continue;
                    if ((s.Width == CellWidth * 2) && (s.Height == CellHeight * 2) && (animation == 0)) continue;

                    if (Settings.DrawFrontLayer)
                    {
                        if (blend)
                        {
                            if ((fileIndex > 99) & (fileIndex < 199))
                                Libraries.MapLibs[fileIndex].DrawBlendEx(index, new Point(drawX, drawY - (3 * CellHeight)), Color.White, true);
                            else
                                Libraries.MapLibs[fileIndex].DrawBlendEx(index, new Point(drawX, drawY - s.Height), Color.White, (index >= 2723 && index <= 2732));
                        }
                        else
                            Libraries.MapLibs[fileIndex].Draw(index, drawX, drawY - s.Height);
                    }
                    
                    #endregion
                }

                for (int x = centerX - ViewRangeX; x <= centerX + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    M2CellInfo[x, y].DrawObjects();
                }
            }

            DXManager.Sprite.Flush();
            float oldOpacity = DXManager.Opacity;
            DXManager.SetOpacity(0.4F);

            //MapObject.User.DrawMount();

            MapObject.User.DrawBody();

            if ((MapObject.User.Direction == MirDirection.Up) ||
                (MapObject.User.Direction == MirDirection.UpLeft) ||
                (MapObject.User.Direction == MirDirection.UpRight) ||
                (MapObject.User.Direction == MirDirection.Right) ||
                (MapObject.User.Direction == MirDirection.Left))
            {
                MapObject.User.DrawHead();
                MapObject.User.DrawWings();
            }
            else
            {
                MapObject.User.DrawWings();
                MapObject.User.DrawHead();
            }

            DXManager.SetOpacity(oldOpacity);

            if (MapObject.MouseObject != null && !MapObject.MouseObject.Dead && MapObject.MouseObject != MapObject.TargetObject && MapObject.MouseObject.Blend) //Far
                MapObject.MouseObject.DrawBlend();

            if (MapObject.TargetObject != null)
                MapObject.TargetObject.DrawBlend();

            for (int i = 0; i < Objects.Count; i++)
            {
                oldOpacity = DXManager.Opacity;
                Objects[i].DrawEffects(Settings.Effect);

                if (Settings.NameView && !(Objects[i] is ItemObject) && !Objects[i].Dead)
                {
                    if (Objects[i] is MonsterObject)
                    {
                        if (Settings.ShowMonsterName)
                            Objects[i].DrawName();

                    }
                    else
                        Objects[i].DrawName();
                }

                Objects[i].DrawChat();
                Objects[i].DrawHealth();
                Objects[i].DrawPoison();
                Objects[i].DrawHealthNum();

                Objects[i].DrawDamages();
            }

            if (!Settings.Effect) return;

            for (int i = Effects.Count - 1; i >= 0; i--)
                Effects[i].Draw();
        }

        private void DrawLights(LightSetting setting)
        {
            if (DXManager.Lights == null || DXManager.Lights.Count == 0) return;

            if (_lightTexture == null || _lightTexture.Disposed)
            {
                _lightTexture = new Texture(DXManager.Device, Settings.ScreenWidth, Settings.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                _lightTexture.Disposing += FloorTexture_Disposing;
                _lightSurface = _lightTexture.GetSurfaceLevel(0);
            }

            Surface oldSurface = DXManager.CurrentSurface;
            DXManager.SetSurface(_lightSurface);

            #region Night Lights
            Color Darkness = Color.Black;
            switch (MapDarkLight)
            {
                case 1:
                    Darkness = Color.FromArgb(255, 20, 20, 20);
                    break;
                case 2:
                    Darkness = Color.LightSlateGray;
                    break;
                case 3:
                    Darkness = Color.SkyBlue;
                    break;
                case 4:
                    Darkness = Color.Goldenrod;
                    break;
                default:
                    Darkness = Color.Black;
                    break;
            }

            DXManager.Device.Clear(ClearFlags.Target, setting == LightSetting.Night ? Darkness : Color.FromArgb(255, 50, 50, 50), 0, 0);
            #endregion

            int light;
            Point p;
            DXManager.SetBlend(true);
            DXManager.Device.RenderState.SourceBlend = Blend.SourceAlpha;

            #region Object Lights (Player/Mob/NPC)
            for (int i = 0; i < Objects.Count; i++)
            {
                MapObject ob = Objects[i];
                if (ob.Light > 0 && (!ob.Dead || ob == MapObject.User || ob.Race == ObjectType.Spell))
                {

                    light = ob.Light;
                    int LightRange = light % 15;
                    if (LightRange >= DXManager.Lights.Count)
                        LightRange = DXManager.Lights.Count - 1;

                    p = ob.DrawLocation;

                    Color lightColour = ob.LightColour;

                    if (ob.Race == ObjectType.Player)
                    {
                        switch (light / 15)
                        {
                            case 0://no light source
                                lightColour = Color.FromArgb(255, 60, 60, 60);
                                break;
                            case 1:
                                lightColour = Color.FromArgb(255, 120, 120, 120);
                                break;
                            case 2://Candle
                                lightColour = Color.FromArgb(255, 180, 180, 180);
                                break;
                            case 3://Torch
                                lightColour = Color.FromArgb(255, 240, 240, 240);
                                break;
                            default://Peddler Torch
                                lightColour = Color.FromArgb(255, 255, 255, 255);
                                break;
                        }
                    }
                    else if (ob.Race == ObjectType.Merchant)
                    {
                        lightColour = Color.FromArgb(255, 120, 120, 120);
                    }

                    if (DXManager.Lights[LightRange] != null && !DXManager.Lights[LightRange].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[LightRange].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[LightRange].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Sprite.Draw2D(DXManager.Lights[LightRange], PointF.Empty, 0, p, lightColour);
                    }

                }
                #region Object Effect Lights
                if (!Settings.Effect) continue;
                for (int e = 0; e < ob.Effects.Count; e++)
                {
                    Effect effect = ob.Effects[e];
                    if (!effect.Blend || CMain.Time < effect.Start || (!(effect is Missile) && effect.Light < ob.Light)) continue;

                    light = effect.Light;

                    p = effect.DrawLocation;

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Sprite.Draw2D(DXManager.Lights[light], PointF.Empty, 0, p, effect.LightColour);
                    }

                }
                #endregion
            }
            #endregion

            #region Map Effect Lights
            if (Settings.Effect)
            {
                for (int e = 0; e < Effects.Count; e++)
                {
                    Effect effect = Effects[e];
                    if (!effect.Blend || CMain.Time < effect.Start) continue;

                    light = effect.Light;
                    if (light == 0) continue;

                    p = effect.DrawLocation;

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Sprite.Draw2D(DXManager.Lights[light], PointF.Empty, 0, p, Color.White);
                    }
                }
            }
            #endregion

            #region Map Lights
            for (int y = MapObject.User.Movement.Y - ViewRangeY - 24; y <= MapObject.User.Movement.Y + ViewRangeY + 24; y++)
            {
                if (y < 0) continue;
                if (y >= Height) break;
                for (int x = MapObject.User.Movement.X - ViewRangeX - 24; x < MapObject.User.Movement.X + ViewRangeX + 24; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int imageIndex = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;
                    if (M2CellInfo[x, y].Light <= 0 || M2CellInfo[x, y].Light >= 10) continue;
                    if (M2CellInfo[x, y].Light == 0) continue;

                    Color lightIntensity;

                    light = (M2CellInfo[x, y].Light % 10) * 3;

                    switch (M2CellInfo[x, y].Light / 10)
                    {
                        case 1:
                            lightIntensity = Color.FromArgb(255, 255, 255, 255);
                            break;
                        case 2:
                            lightIntensity = Color.FromArgb(255, 120, 180, 255);
                            break;
                        case 3:
                            lightIntensity = Color.FromArgb(255, 255, 180, 120);
                            break;
                        case 4:
                            lightIntensity = Color.FromArgb(255, 22, 160, 5);
                            break;
                        default:
                            lightIntensity = Color.FromArgb(255, 255, 255, 255);
                            break;
                    }

                    int fileIndex = M2CellInfo[x, y].FrontIndex;

                    p = new Point(
                        (x + OffSetX - MapObject.User.Movement.X) * CellWidth + MapObject.User.OffSetMove.X,
                        (y + OffSetY - MapObject.User.Movement.Y) * CellHeight + MapObject.User.OffSetMove.Y + 32);


                    if (M2CellInfo[x, y].FrontAnimationFrame > 0)
                        p.Offset(Libraries.MapLibs[fileIndex].GetOffSet(imageIndex));

                    if (light >= DXManager.Lights.Count)
                        light = DXManager.Lights.Count - 1;

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2) + 10, -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Sprite.Draw2D(DXManager.Lights[light], PointF.Empty, 0, p, lightIntensity);
                    }
                }
            }
            #endregion

            DXManager.SetBlend(false);
            DXManager.SetSurface(oldSurface);

            DXManager.Device.RenderState.SourceBlend = Blend.DestinationColor;
            DXManager.Device.RenderState.DestinationBlend = Blend.BothInvSourceAlpha;

            DXManager.Sprite.Draw2D(_lightTexture, PointF.Empty, 0, PointF.Empty, Color.White);
            DXManager.Sprite.End();
            DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
        }

        private static void OnMouseClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            if (me == null) return;

            if (AwakeningAction) return;
            switch (me.Button)
            {
                case MouseButtons.Left:
                    {
                        AutoRun = false;
                        if (MapObject.MouseObject == null) return;
                        NPCObject npc = MapObject.MouseObject as NPCObject;
                        if (npc != null)
                        {
                            GameScene.Scene.NPCDialog.Hide();

                            if (CMain.Time <= GameScene.NPCTime && npc.ObjectID == GameScene.NPCID) return;

                            GameScene.NPCTime = CMain.Time + 5000;
                            GameScene.NPCID = npc.ObjectID;
                            Network.Enqueue(new C.CallNPC { ObjectID = npc.ObjectID, Key = "[@Main]" });
                        }

                        MonsterObject mon = MapObject.MouseObject as MonsterObject;
                        if (mon != null && mon.AI == 201)
                        {
                            GameScene.Scene.NPCDialog.Hide();

                            if (CMain.Time <= GameScene.NPCTime) return;

                            GameScene.NPCTime = CMain.Time + 5000;

                            Network.Enqueue(new C.TalkMonsterNPC { ObjectID = mon.ObjectID });
                        }
                    }
                    break;
                case MouseButtons.Right:
                    {
                        AutoRun = false;
                        if (MapObject.MouseObject == null) return;
                        PlayerObject player = MapObject.MouseObject as PlayerObject;
                        if (player == null || player == User || !CMain.Ctrl) return;
                        if (CMain.Time <= GameScene.InspectTime && player.ObjectID == InspectDialog.InspectID) return;

                        GameScene.InspectTime = CMain.Time + 500;
                        InspectDialog.InspectID = player.ObjectID;
                        Network.Enqueue(new C.Inspect { ObjectID = player.ObjectID });
                    }
                    break;
                case MouseButtons.Middle:
                    AutoRun = !AutoRun;
                    break;
            }
        }

        private static void DropItem()
        {
            if (GameScene.SelectedCell.GridType != MirGridType.Inventory)
            {
                GameScene.SelectedCell = null;
                return;
            }

            MirItemCell cell = GameScene.SelectedCell;
            if (cell.Item.Info.Bind.HasFlag(BindMode.DontDrop))
            {
                MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("You cannot drop {0}"), cell.Item.Name), MirMessageBoxButtons.OK);
                messageBox.Show();
                GameScene.SelectedCell = null;
                return;
            }
            if (cell.Item.Count == 1)
            {
                if (cell.Item.Info.Grade > ItemGrade.Common)
                {
                    MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("Are you sure you want to drop {0}?"), cell.Item.FriendlyName), MirMessageBoxButtons.YesNo);

                    messageBox.YesButton.Click += (o, a) =>
                    {
                        Network.Enqueue(new C.DropItem { UniqueID = cell.Item.UniqueID, Count = 1 });
                        cell.Locked = true;
                    };
                    messageBox.Show();
                }
                else
                {
                    Network.Enqueue(new C.DropItem { UniqueID = cell.Item.UniqueID, Count = 1 });
                    cell.Locked = true;
                }
            }
            else
            {
                MirAmountBox amountBox = new MirAmountBox(CMain.Tr("Drop Amount:"), cell.Item.Info.Image, cell.Item.Count);

                amountBox.OKButton.Click += (o, a) =>
                {
                    if (amountBox.Amount <= 0) return;
                    Network.Enqueue(new C.DropItem
                    {
                        UniqueID = cell.Item.UniqueID,
                        Count = amountBox.Amount
                    });

                    cell.Locked = true;
                };

                amountBox.Show();
            }
            GameScene.SelectedCell = null;
        }

        private static void OnMouseDown(object sender, MouseEventArgs e)
        {
            MapButtons |= e.Button;
            GameScene.CanRun = false;

            if (AwakeningAction) return;

            if (e.Button != MouseButtons.Left) return;

            if (GameScene.SelectedCell != null)
            {
                DropItem();
                MapButtons &= ~e.Button;
                return;
            }
            if (GameScene.PickedUpGold)
            {
                MirAmountBox amountBox = new MirAmountBox(CMain.Tr("Drop Amount:"), 116, GameScene.Gold);

                amountBox.OKButton.Click += (o, a) =>
                {
                    if (amountBox.Amount > 0)
                    {
                        Network.Enqueue(new C.DropGold { Amount = amountBox.Amount });
                    }
                };

                amountBox.Show();
                GameScene.PickedUpGold = false;
            }

            if (MapObject.MouseObject != null && !MapObject.MouseObject.Dead && !(MapObject.MouseObject is ItemObject) &&
                !(MapObject.MouseObject is NPCObject) && !(MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 64)
                 && !(MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 70))
            {
                MapObject.TargetObject = MapObject.MouseObject;
                if (MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI != 6)
                    MapObject.MagicObject = MapObject.TargetObject;
            }
            else
                MapObject.TargetObject = null;
        }

        private bool ProcessAutoPath()
        {
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                AutoPath = false;
                return true;
            }

            Node currentNode = CurrentPath.SingleOrDefault(x => User.CurrentLocation == x.Location);
            if (currentNode != null)
            {
                while (true)
                {
                    Node first = CurrentPath.First();
                    CurrentPath.Remove(first);

                    if (first == currentNode)
                        break;
                }
            }

            if (CurrentPath.Count > 0)
            {
                MirDirection dir = Functions.DirectionFromPoint(User.CurrentLocation, CurrentPath.First().Location);

                if (!User.CanWalk(dir))
                {
                    CurrentPath = PathFinder.FindPath(MapObject.User.CurrentLocation, CurrentPath.Last().Location);
                    return true;
                }

                if (GameScene.CanRun && User.CanRun(dir) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && CurrentPath.Count > 1)
                {
                    int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                    Node upcomingStep = CurrentPath.SingleOrDefault(x => Functions.PointMove(User.CurrentLocation, dir, distance) == x.Location);
                    if (upcomingStep != null)
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, distance) };
                        return true;
                    }
                }

                if (User.CanWalk(dir))
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, 1) };

                    return true;
                }
            }

            return false;
        }

        private bool ProcessAutoAttack()
        {
            GameScene.LogTime = CMain.Time + Globals.LogDelay;
            MirDirection direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation);

            if (User.Class == MirClass.Archer && User.HasClassWeapon && !User.RidingMount && !User.Fishing)//ArcherTest - non aggressive targets (player / pets)
            {
                if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                {
                    if (CMain.Time > GameScene.AttackTime)
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.AttackRange1, Direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation), Location = User.CurrentLocation, Params = new List<object>() };
                        User.QueuedAction.Params.Add(MapObject.TargetObject != null ? MapObject.TargetObject.ObjectID : (uint)0);
                        User.QueuedAction.Params.Add(MapObject.TargetObject.CurrentLocation);
                        // MapObject.TargetObject = null; //stop constant attack when close up
                    }
                }
                else
                {
                    GameScene.Scene.OutputMessageIfPrepared(CMain.Tr("Target is too far."));
                }
                //  return;
            }

            else if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 1)
                || (Settings.SpaceThrusting && HasTarget(Functions.PointMove(User.CurrentLocation, direction, 2)) 
                && User.IsMagicToggle(Spell.Thrusting)
                && !User.IsMagicToggle(Spell.CrossHalfMoon)
                && !User.IsMagicToggle(Spell.HalfMoon)
                && !User.IsMagicToggle(Spell.TwinDrakeBlade)))
            {
                if (CMain.Time > GameScene.AttackTime && User.CanRideAttack())
                {
          //          CMain.SaveDebug(string.Format("加入攻击动作 攻击时间 {0} 当前时间 {1} 下一次Motion时间 {2}", GameScene.AttackTime, CMain.Time, User.NextMotion));
                    User.QueuedAction = new QueuedAction { Action = MirAction.Attack1, Direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation), Location = User.CurrentLocation };
                    return true;
                }
            }

            return false;
        }

        private void ProcessAutoRun(MirDirection direction)
        {
            if (GameScene.CanRun && User.CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint))) //slow remove
            {
                int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                bool fail = false;
                for (int i = 1; i <= distance; i++)
                {
                    if (!CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                        fail = true;
                }
                if (!fail)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, distance) };
                    return;
                }
            }
            if ((User.CanWalk(direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                return;
            }
            if (direction != User.Direction)
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
            }
        }

        private bool OnLeftButtonDown(MirDirection direction)
        {
            if (MapObject.MouseObject is NPCObject || (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != User)) return false;
            if (MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 70) return false;

            if (CMain.Alt && !User.RidingMount)
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Harvest, Direction = direction, Location = User.CurrentLocation };
                return true;
            }

            if (CMain.Shift)
            {
                if (CMain.Time > GameScene.AttackTime && User.CanRideAttack()) //ArcherTest - shift click
                {
                    MapObject target = null;
                    if (MapObject.MouseObject is MonsterObject || MapObject.MouseObject is PlayerObject) target = MapObject.MouseObject;

                    if (User.Class == MirClass.Archer && User.HasClassWeapon && !User.RidingMount)
                    {
                        if (target != null)
                        {
                            if (!Functions.InRange(MapObject.MouseObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                            {
                                GameScene.Scene.OutputMessageIfPrepared("Target is too far.");
                                return true;
                            }
                        }

                        User.QueuedAction = new QueuedAction { Action = MirAction.AttackRange1, Direction = MouseDirection(), Location = User.CurrentLocation, Params = new List<object>() };
                        User.QueuedAction.Params.Add(target != null ? target.ObjectID : (uint)0);
                        User.QueuedAction.Params.Add(Functions.PointMove(User.CurrentLocation, MouseDirection(), 9));
                        return true;
                    }

                    //stops double slash from being used without empty hand or assassin weapon (otherwise bugs on second swing)
                    if (User.IsMagicToggle(Spell.DoubleSlash) && (!User.HasClassWeapon && User.Weapon > -1)) return true;

                    User.QueuedAction = new QueuedAction { Action = MirAction.Attack1, Direction = direction, Location = User.CurrentLocation };
                }
                return true;
            }

            if (MapObject.MouseObject is MonsterObject && User.Class == MirClass.Archer && MapObject.TargetObject != null && !MapObject.TargetObject.Dead && User.HasClassWeapon && !User.RidingMount) //ArcherTest - range attack
            {
                if (Functions.InRange(MapObject.MouseObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                {
                    if (CMain.Time > GameScene.AttackTime)
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.AttackRange1, Direction = direction, Location = User.CurrentLocation, Params = new List<object>() };
                        User.QueuedAction.Params.Add(MapObject.TargetObject.ObjectID);
                        User.QueuedAction.Params.Add(MapObject.TargetObject.CurrentLocation);
                    }
                }
                else
                {
                    GameScene.Scene.OutputMessageIfPrepared("Target is too far.");
                }
                return true;
            }

            if (MapLocation == User.CurrentLocation)
            {
                if (CMain.Time > GameScene.PickUpTime)
                {
                    GameScene.PickUpTime = CMain.Time + 200;
                    Network.Enqueue(new C.PickUp());
                }
                return true;
            }

            //mine
            if (!ValidPoint(Functions.PointMove(User.CurrentLocation, direction, 1)))
            {
                if ((MapObject.User.Equipment[(int)EquipmentSlot.Weapon] != null) && (MapObject.User.Equipment[(int)EquipmentSlot.Weapon].Info.CanMine))
                {
                    if (direction != User.Direction)
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                        return true;
                    }
                    AutoHit = true;
                    return true;
                }
            }
            if ((User.CanWalk(direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                return true;
            }
            if (direction != User.Direction)
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                return true;
            }

            if (CanFish(direction))
            {
                User.FishingTime = CMain.Time;
                Network.Enqueue(new C.FishingCast { CastOut = true });
                return true;
            }
            return false;
        }

        private bool WalkToTargetPosition()
        {
            int x = (MouseLocation.X + MapObject.GlobalDisplayOffset.X) / CellWidth + GameScene.User.CurrentLocation.X;
            int y = (MouseLocation.Y + MapObject.GlobalDisplayOffset.Y) / CellWidth + GameScene.User.CurrentLocation.Y;

            CurrentPath = GameScene.Scene.MapControl.PathFinder.FindPath(User.CurrentLocation, new Point(x, y));
            AutoPath = true;
            GameScene.Scene.ChatDialog.ReceiveChat(string.Format("{0} {1}", x, y), ChatType.Hint);
            return true;
        }

        private bool OnRightButtonDown(MirDirection direction)
        {
            if (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != User && CMain.Ctrl) return false;

            if (Functions.InRange(MapLocation, User.CurrentLocation, 2))
            {
                if (direction != User.Direction)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                }
                return true;
            }

            GameScene.CanRun = User.FastRun ? true : GameScene.CanRun;

            if (GameScene.CanRun && User.CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint))) //slow removed
            {
                int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                bool fail = false;
                for (int i = 0; i <= distance; i++)
                {
                    if (!CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                        fail = true;
                }
                if (!fail)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, User.RidingMount || (User.Sprint && !User.Sneaking) ? 3 : 2) };
                    return true;
                }
            }
            if ((User.CanWalk(direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                return true;
            }
            if (direction != User.Direction)
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                return true;
            }

            return false;
        }

        private void CheckInput()
        {
            if (AwakeningAction) return;

            if ((MouseControl == this) && (MapButtons != MouseButtons.None)) AutoHit = false;//mouse actions stop mining even when frozen!
            if (!User.CanRideAttack()) AutoHit = false;

            if (CMain.Time < InputDelay || User.Poison.HasFlag(PoisonType.Paralysis) || User.Poison.HasFlag(PoisonType.LRParalysis) || User.Poison.HasFlag(PoisonType.Frozen) || User.Fishing) return;

            if (User.NextMagic != null && !User.RidingMount)
            {
                UseMagic(User.NextMagic);
                return;
            }

            if (CMain.Time < User.ReincarnationStopTime)
            {
                return;
            }

            if (AutoPath)
            {
                if (MapButtons == MouseButtons.Left)
                {
                    AutoPath = false;
                    return;
                }

                ProcessAutoPath();
            }

            if (MapObject.TargetObject != null && !MapObject.TargetObject.Dead)
            {
                if (((MapObject.TargetObject.Name.EndsWith(")") || MapObject.TargetObject is PlayerObject) && (CMain.Shift || Settings.FreeShift)) ||
                    (!MapObject.TargetObject.Name.EndsWith(")") && MapObject.TargetObject is MonsterObject))
                {
                    if (ProcessAutoAttack())
                        return;
                }
            }

            if (AutoHit && !User.RidingMount)
            {
                if (CMain.Time > GameScene.AttackTime)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Mine, Direction = User.Direction, Location = User.CurrentLocation };
                    return;
                }
            }


            MirDirection direction;
            if (MouseControl == this)
            {
                direction = MouseDirection();
                if (AutoRun)
                {
                    ProcessAutoRun(direction);
                    return;
                }

                switch (MapButtons)
                {
                    case MouseButtons.Left:
                        if (OnLeftButtonDown(direction))
                            return;
                        break;
                    case MouseButtons.Right:
                        if (OnRightButtonDown(direction))
                            return;
                        break;
                }
            }

            ProcessTarget();
        }

        private void ProcessTarget()
        {
            if (MapObject.TargetObject == null || MapObject.TargetObject.Dead) return;

            if (MapObject.TargetObject.Name.EndsWith(")") || (MapObject.TargetObject is PlayerObject))
            {
                if (!CMain.Shift && !Settings.FreeShift)
                    return;
            }

            if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 1)) return;
            if (User.Class == MirClass.Archer && User.HasClassWeapon && (MapObject.TargetObject is MonsterObject || MapObject.TargetObject is PlayerObject)) return; //ArcherTest - stop walking

            MirDirection direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation);
            // 隔位刺杀
            if (Settings.SpaceThrusting && User.IsMagicToggle(Spell.Thrusting) && HasTarget(Functions.PointMove(User.CurrentLocation, direction, 2))) return;

            if (GameScene.CanRun && User.CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint)))
            {
                int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                bool fail = false;
                for (int i = 1; i <= distance; i++)
                {
                    if (!CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                        fail = true;
                }
                if (!fail)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, distance) };
                    return;
                }
            }
            else
            {
                if (User.CanWalk(direction))
                    User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
            }
        }

        public void UseMagic(ClientMagic magic)
        {
            if (CMain.Time < GameScene.SpellTime || User.Poison.HasFlag(PoisonType.Stun))
            {
                User.ClearMagic();
                return;
            }

          
            if (UserObject.User.IsMagicInCD(magic.Spell))
            {
                GameScene.Scene.OutputMessageIfPrepared(string.Format(CMain.Tr("You cannot cast {0} for another {1} seconds."), CMain.Tr(magic.Spell.ToString()), UserObject.User.GetMagicTimeLeft(magic.Spell) / 1000 + 1));
                User.ClearMagic();
                return;
            }

            int cost = magic.Level * magic.LevelCost + magic.BaseCost;

            if (magic.Spell == Spell.Teleport || magic.Spell == Spell.Blink || magic.Spell == Spell.StormEscape || magic.Spell == Spell.StormEscape1)
            {
                for (int i = 0; i < GameScene.Scene.Buffs.Count; i++)
                {
                    if (GameScene.Scene.Buffs[i].Type != BuffType.TemporalFlux) continue;
                    cost += (int)(User.MaxMP * 0.3F);
                }
            }

            if (cost > MapObject.User.MP)
            {
                GameScene.Scene.OutputMessageIfPrepared(CMain.Tr("Not Enough Mana to cast."));
                User.ClearMagic();
                return;
            }

            //bool isTargetSpell = true;

            MapObject target = null;

            //Targeting
            switch (magic.Spell)
            {
                case Spell.FireBall:
                case Spell.GreatFireBall:
                case Spell.ElectricShock:
                case Spell.Poisoning:
                case Spell.ThunderBolt:
                case Spell.FlameDisruptor:
                case Spell.SoulFireBall:
                case Spell.TurnUndead:
                case Spell.FrostCrunch:
                case Spell.Vampirism:
                case Spell.Revelation:
                case Spell.Entrapment:
                case Spell.EntrapSwordSecret:
                case Spell.Hallucination:
                case Spell.DarkBody:
                case Spell.Taunt:
                case Spell.Plague:
                case Spell.HealingCircle:
                case Spell.HealingCircle2:
                case Spell.GreateFireBallSecret:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }

                    if (target == null) target = MapObject.MagicObject;

                    if (target != null && target.Race == ObjectType.Monster) MapObject.MagicObject = target;
                    break;
                case Spell.StraightShot:
                case Spell.DoubleShot:
                case Spell.ElementalShot:
                case Spell.DelayedExplosion:
                case Spell.DelayedExplosion2:
                case Spell.BindingShot:
                case Spell.VampireShot:
                case Spell.PoisonShot:
                case Spell.CrippleShot:
                case Spell.NapalmShot:
                case Spell.NapalmShot2:
                case Spell.SummonVampire:
                case Spell.SummonToad:
                case Spell.SummonSnakes:
                    if (!User.HasClassWeapon)
                    {
                        GameScene.Scene.OutputMessage(CMain.Tr("You must be wearing a bow to perform this skill."));
                        User.ClearMagic();
                        return;
                    }
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }

                    if (target == null) target = MapObject.MagicObject;

                    if (target != null) MapObject.MagicObject = target;     
                    break;
                case Spell.Purification:
                case Spell.Healing:
                case Spell.UltimateEnhancer:
                case Spell.EnergyShield:
                case Spell.PetEnhancer:
                case Spell.Healing2:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }

                    if (target == null) target = User;
                    break;
                case Spell.FireBang:
                case Spell.MassHiding:
                case Spell.FireWall:
                case Spell.TrapHexagon:
                case Spell.CatTongue:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }
                    break;
                case Spell.PoisonCloud:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }
                    break;
                case Spell.Blizzard:
                case Spell.MeteorStrike:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }
                    break;
                case Spell.Reincarnation:
                    if (User.NextMagicObject != null)
                    {
                        if (User.NextMagicObject.Dead && User.NextMagicObject.Race == ObjectType.Player)
                            target = User.NextMagicObject;
                    }
                    break;
                case Spell.Trap:
                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }
                    break;
                case Spell.FlashDash:
                case Spell.FlashDash2:
                    if (User.GetMagic(magic.Spell).Level <= 1 && User.IsDashAttack() == false)
                    {
                        User.ClearMagic();
                        return;
                    }
                    break;
                    
                default:
                    break;
            }

            MirDirection dir = (target == null || target == User) ? User.NextMagicDirection : Functions.DirectionFromPoint(User.CurrentLocation, target.CurrentLocation);

            Point location = target != null ? target.CurrentLocation : User.NextMagicLocation;

            if (magic.Spell == Spell.FlashDash || magic.Spell == Spell.FlashDash2)
                dir = User.Direction;

            if ((magic.Range != 0) && (!Functions.InRange(User.CurrentLocation, location, magic.Range)))
            {
                GameScene.Scene.OutputMessageIfPrepared(CMain.Tr("Target is too far."));
                User.ClearMagic();
                return;
            }

            GameScene.Scene.AssistHelper.PrevSendUseMagic(magic);

            GameScene.LogTime = CMain.Time + Globals.LogDelay;

            User.QueuedAction = new QueuedAction { Action = MirAction.Spell, Direction = dir, Location = User.CurrentLocation, Params = new List<object>() };
            User.QueuedAction.Params.Add(magic.Spell);
            User.QueuedAction.Params.Add(target != null ? target.ObjectID : 0);
            User.QueuedAction.Params.Add(location);
            User.QueuedAction.Params.Add(magic.Level);
         //   User.ClearMagic();
        }

        public bool HasTargetDir(Point currentLocation, MirDirection direction, int len)
        {
            for(int i=0; i<len; i++)
            {
                if (HasTarget(Functions.PointMove(currentLocation, direction, i+1)))
                    return true;
            }

            return false;
        }

        public static MirDirection MouseDirection(float ratio = 45F) //22.5 = 16
        {
            Point p = new Point(MouseLocation.X / CellWidth, MouseLocation.Y / CellHeight);
            if (Functions.InRange(new Point(OffSetX, OffSetY), p, 2))
                return Functions.DirectionFromPoint(new Point(OffSetX, OffSetY), p);

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = MouseLocation;
            float bc = (float)Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;
            angle += ratio / 2;
            if (angle > 360) angle -= 360;

            return (MirDirection)(angle / ratio);
        }

        public static int Direction16(Point source, Point destination)
        {
            PointF c = new PointF(source.X, source.Y);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF(destination.X, destination.Y);
            float bc = (float)Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (destination.X < c.X) angle = 360 - angle;
            angle += 11.25F;
            if (angle > 360) angle -= 360;

            return (int)(angle / 22.5F);
        }

        public static double Distance(PointF p1, PointF p2)
        {
            double x = p2.X - p1.X;
            double y = p2.Y - p1.Y;
            return Math.Sqrt(x * x + y * y);
        }

        public bool EmptyCell(Point p)
        {
            if ((M2CellInfo[p.X, p.Y].BackImage & 0x20000000) != 0 || (M2CellInfo[p.X, p.Y].FrontImage & 0x8000) != 0) // + (M2CellInfo[P.X, P.Y].FrontImage & 0x7FFF) != 0)
                return false;

            for (int i = 0; i < Objects.Count; i++)
            {
                MapObject ob = Objects[i];

                if (ob.CurrentLocation == p && ob.Blocking)
                    return false;
            }

            return true;
        }

        public bool CheckDoorOpen(Point p)
        {
            if (M2CellInfo[p.X, p.Y].DoorIndex == 0) return true;
            Door DoorInfo = GetDoor(M2CellInfo[p.X, p.Y].DoorIndex);
            if (DoorInfo == null) return false;//if the door doesnt exist then it isnt even being shown on screen (and cant be open lol)
            if ((DoorInfo.DoorState == 0) || (DoorInfo.DoorState == 3))
            {
                Network.Enqueue(new C.Opendoor() { DoorIndex = DoorInfo.index });
                return false;
            }
            if ((DoorInfo.DoorState == 2) && (DoorInfo.LastTick + 4000 > CMain.Time))
            {
                Network.Enqueue(new C.Opendoor() { DoorIndex = DoorInfo.index });
            }
            return true;
        }

        public bool CanFish(MirDirection dir)
        {
            if (!GameScene.User.HasFishingRod || GameScene.User.FishingTime + 1000 > CMain.Time) return false;
            if (GameScene.User.CurrentAction != MirAction.Standing) return false;
            if (GameScene.User.Direction != dir) return false;
            if (GameScene.User.TransformType >= 6 && GameScene.User.TransformType <= 9) return false;

            Point point = Functions.PointMove(User.CurrentLocation, dir, 3);

            if (!M2CellInfo[point.X, point.Y].FishingCell) return false;

            return true;
        }

        public bool CanFly(Point target)
        {
            Point location = User.CurrentLocation;
            while (location != target)
            {
                MirDirection dir = Functions.DirectionFromPoint(location, target);

                location = Functions.PointMove(location, dir, 1);

                if (location.X < 0 || location.Y < 0 || location.X >= GameScene.Scene.MapControl.Width || location.Y >= GameScene.Scene.MapControl.Height) return false;

                if (!GameScene.Scene.MapControl.ValidPoint(location)) return false;
            }

            return true;
        }


        public bool ValidPoint(Point p)
        {
            //GameScene.Scene.ChatDialog.ReceiveChat(string.Format("cell: {0}", (M2CellInfo[p.X, p.Y].BackImage & 0x20000000)), ChatType.Hint);
            return (M2CellInfo[p.X, p.Y].BackImage & 0x20000000) == 0;
        }
        public bool HasTarget(Point p)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                MapObject ob = Objects[i];

                if (ob.CurrentLocation == p && ob.Blocking)
                    return true;
            }
            return false;
        }
        public bool CanHalfMoon(Point p, MirDirection d)
        {
            d = Functions.PreviousDir(d);
            for (int i = 0; i < 4; i++)
            {
                if (HasTarget(Functions.PointMove(p, d, 1))) return true;
                d = Functions.NextDir(d);
            }
            return false;
        }
        public bool CanCrossHalfMoon(Point p)
        {
            MirDirection dir = MirDirection.Up;
            for (int i = 0; i < 8; i++)
            {
                if (HasTarget(Functions.PointMove(p, dir, 1))) return true;
                dir = Functions.NextDir(dir);
            }
            return false;
        }

        private void FloorTexture_Disposing(object sender, EventArgs e)
        {
            FloorValid = false;
            _floorTexture = null;

            if (_floorSurface != null && !_floorSurface.Disposed)
                _floorSurface.Dispose();
            _floorSurface = null;
        }
        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Objects.Clear();

                MapButtons = 0;
                MouseLocation = Point.Empty;
                InputDelay = 0;
                NextAction = 0;

                M2CellInfo = null;
                Width = 0;
                Height = 0;

                FileName = String.Empty;
                Title = String.Empty;
                MiniMap = 0;
                BigMap = 0;
                Lights = 0;
                FloorValid = false;
                LightsValid = false;
                MapDarkLight = 0;
                Music = 0;

                if (_floorSurface != null && !_floorSurface.Disposed)
                    _floorSurface.Dispose();


                if (_lightSurface != null && !_lightSurface.Disposed)
                    _lightSurface.Dispose();

                AnimationCount = 0;
                Effects.Clear();
            }

            base.Dispose(disposing);
        }

        #endregion



        public void RemoveObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].RemoveObject(ob);
        }
        public void AddObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].AddObject(ob);
        }
        public MapObject FindObject(uint ObjectID, int x, int y)
        {
            if (x > Width || y > Height)
                return null;

            return M2CellInfo[x, y].FindObject(ObjectID);
        }
        public void SortObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].Sort();
        }

        public Door GetDoor(byte Index)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i].index == Index)
                    return Doors[i];
            }
            return null;
        }
        public void Processdoors()
        {
            long Now = CMain.Time;
            for (int i = 0; i < Doors.Count; i++)
            {
                if ((Doors[i].DoorState == 1) || (Doors[i].DoorState == 3))
                {
                    if (Doors[i].LastTick + 50 < CMain.Time)
                    {
                        Doors[i].LastTick = CMain.Time;
                        Doors[i].ImageIndex++;
                        if (Doors[i].ImageIndex == 1)//change the 1 if you want to actualy animate doors opening/closing
                        {
                            Doors[i].ImageIndex = 0;
                            Doors[i].DoorState = (byte)(++Doors[i].DoorState % 4);
                        }
                        FloorValid = false;
                    }
                }
                if (Doors[i].DoorState == 2)
                {
                    if (Doors[i].LastTick + 5000 < CMain.Time)
                    {
                        Doors[i].LastTick = CMain.Time;
                        Doors[i].DoorState = 3;
                        FloorValid = false;
                    }
                }
            }
        }
        public void OpenDoor(byte Index, bool Closed)
        {
            Door Info = GetDoor(Index);
            if (Info == null) return;
            Info.DoorState = (byte)(Closed ? 3 : Info.DoorState == 2 ? 2 : 1);
            Info.ImageIndex = 0;
            Info.LastTick = CMain.Time;
        }
    }
}
