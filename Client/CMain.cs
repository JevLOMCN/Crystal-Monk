using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirScenes;
using Client.MirSounds;
using Microsoft.DirectX.Direct3D;
using Font = System.Drawing.Font;
using Client.MirScenes.Dialogs;
using Client.MirMagic;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Client.MirObjects;

namespace Client
{
    public partial class CMain : Form
    {
        class DownloadTask
        {
            public string FileName;
            public Action Action;
        }

        public static MirControl DebugBaseLabel, HintBaseLabel, DownloadBaseLabel;
        public static MirLabel DebugTextLabel, HintTextLabel, DownLoadLabel;
        public static Graphics Graphics;
        public static Point MPoint;

        public readonly static Stopwatch Timer = Stopwatch.StartNew();
        public readonly static DateTime StartTime = DateTime.Now;
        public static long Time, OldTime;
        public static DateTime Now { get { return StartTime.AddMilliseconds(Time); } }
        public static readonly Random Random = new Random();

        public static bool DebugOverride;

        private static long _fpsTime;
        private static int _fps;
        public static int FPS;
        public static int DrawTextCallTimes;

        public static bool Shift, Alt, Ctrl, Tilde;
        public static KeyBindSettings InputKeys = new KeyBindSettings();
        public static Language lan = new Language("language.txt");
        public static MagicConfMgr MagicConf = new MagicConfMgr();

        private static Task _task;
        private static ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        private static Queue<DownloadTask> _downloadQueue = new Queue<DownloadTask>();
        private static HashSet<String> _downloadFiles = new HashSet<string>();
        private static volatile bool isDownloading;

        private static Action action;

        public CMain()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            MouseClick += CMain_MouseClick;
            MouseDown += CMain_MouseDown;
            MouseUp += CMain_MouseUp;
            MouseMove += CMain_MouseMove;
            MouseDoubleClick += CMain_MouseDoubleClick;
            KeyPress += CMain_KeyPress;
            KeyDown += CMain_KeyDown;
            KeyUp += CMain_KeyUp;
            Deactivate += CMain_Deactivate;
            MouseWheel += CMain_MouseWheel;


            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            FormBorderStyle = Settings.FullScreen ? FormBorderStyle.None : FormBorderStyle.FixedDialog;

            Graphics = CreateGraphics();
            Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Graphics.CompositingQuality = CompositingQuality.HighQuality;
            Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Graphics.TextContrast = 0;
        }

        private void CMain_Load(object sender, EventArgs e)
        {

            try
            {
                ClientSize = new Size(Settings.ScreenWidth, Settings.ScreenHeight);

                DXManager.Create();
                SoundManager.Create();
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                try
                {
                    UpdateTime();
                    UpdateEnviroment();
                    RenderEnvironment();
                }
                catch (Exception ex)
                {
                    SaveError(string.Format("{0}, {1}, {2}", DXManager.TextureList.Count, MapObject.LabelList.Count, ex.ToString()));
                }
            }
        }

        private static void CMain_Deactivate(object sender, EventArgs e)
        {
            MapControl.MapButtons = MouseButtons.None;
            Shift = false;
            Alt = false;
            Ctrl = false;
            Tilde = false;
        }

        public static void CMain_KeyDown(object sender, KeyEventArgs e)
        {
            Shift = e.Shift;
            Alt = e.Alt;
            Ctrl = e.Control;

            if (e.KeyCode == Keys.Oem8)
                CMain.Tilde = true;


            try
            {
                if (e.Alt && e.KeyCode == Keys.Enter)
                {
                    ToggleFullScreen();
                    return;
                }

                if (MirScene.ActiveScene != null)
                {
                    MirScene.ActiveScene.OnKeyDown(e);
                }
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        public static void CMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (Settings.FullScreen)
                Cursor.Clip = new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHeight);

            MPoint = Program.Form.PointToClient(Cursor.Position);
            Point oldPoint = MPoint;
            MPoint.Y = MPoint.Y * Settings.ScreenHeight / Program.Form.ClientSize.Height;
            MPoint.X = MPoint.X * Settings.ScreenWidth / Program.Form.ClientSize.Width;

            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_KeyUp(object sender, KeyEventArgs e)
        {
            Shift = e.Shift;
            Alt = e.Alt;
            Ctrl = e.Control;

            if (e.KeyCode == Keys.Oem8)
                CMain.Tilde = false;

            foreach (KeyBind KeyCheck in CMain.InputKeys.Keylist)
            {
                if (KeyCheck.function != KeybindOptions.Screenshot) continue;
                if (KeyCheck.CutomKey.Key != e.KeyCode)
                    continue;
                if ((KeyCheck.CutomKey.RequireAlt != 2) && (KeyCheck.CutomKey.RequireAlt != (Alt ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireShift != 2) && (KeyCheck.CutomKey.RequireShift != (Shift ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireCtrl != 2) && (KeyCheck.CutomKey.RequireCtrl != (Ctrl ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireTilde != 2) && (KeyCheck.CutomKey.RequireTilde != (Tilde ? 1 : 0)))
                    continue;
                Program.Form.CreateScreenShot();
                break;

            }
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnKeyUp(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnKeyPress(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseUp(object sender, MouseEventArgs e)
        {
            MapControl.MapButtons &= ~e.Button;
            if (!MapControl.MapButtons.HasFlag(MouseButtons.Right))
                GameScene.CanRun = false;

            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (Program.Form.ActiveControl is TextBox)
            {
                MirTextBox textBox = Program.Form.ActiveControl.Tag as MirTextBox;

                if (textBox != null && textBox.CanLoseFocus)
                    Program.Form.ActiveControl = null;
            }

            if (e.Button == MouseButtons.Right && (GameScene.SelectedCell != null || GameScene.PickedUpGold))
            {
                GameScene.SelectedCell = null;
                GameScene.PickedUpGold = false;
                return;
            }

            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseWheel(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void UpdateTime()
        {
            Time = Timer.ElapsedMilliseconds;
        }

        private static void UpdateEnviroment()
        {
            if (Time >= _fpsTime)
            {
                _fpsTime = Time + 1000;
                FPS = _fps;
                _fps = 0;
                DXManager.Clean(); // Clean once a second.
                CMain.SaveDebug(string.Format("DrawText {0}", DrawTextCallTimes));
            }
            else
                _fps++;

            while (queue.TryDequeue(out action))
                action();

            if (!isDownloading && _downloadQueue.Count > 0)
            {
                DownloadTask task = _downloadQueue.Dequeue();
                isDownloading = true;
                DownloadASync(task.FileName, task.Action);
            }

            Network.Process();

            if (MirScene.ActiveScene != null)
                MirScene.ActiveScene.Process();

            for (int i = 0; i < MirAnimatedControl.Animations.Count; i++)
                MirAnimatedControl.Animations[i].UpdateOffSet();

            for (int i = 0; i < MirAnimatedButton.Animations.Count; i++)
                MirAnimatedButton.Animations[i].UpdateOffSet();

            CreateHintLabel();
        }

        private static void RenderEnvironment()
        {
            try
            {
                if (DXManager.DeviceLost)
                {
                    DXManager.AttemptReset();
                    Thread.Sleep(1);
                    return;
                }
                else
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.CornflowerBlue, 0, 0);
                    DXManager.Device.BeginScene();
                    DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
                    DXManager.SetSurface(DXManager.MainSurface);

                    if (MirScene.ActiveScene != null)
                        MirScene.ActiveScene.Draw();

                    DXManager.Sprite.End();
                    DXManager.Device.EndScene();
                    DXManager.Device.Present();
                }
            }
            catch (DeviceLostException)
            {
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());

                DXManager.AttemptRecovery();
            }
        }

        private static void CreateDownloadLabel()
        {
            if (DownloadBaseLabel == null || DownloadBaseLabel.IsDisposed)
            {
                DownloadBaseLabel = new MirControl
                {
                    BackColour = Color.FromArgb(50, 50, 50),
                    Border = true,
                    BorderColour = Color.Black,
                    DrawControlTexture = true,
                    Location = new Point(5, 5),
                    NotControl = true,
                    Opacity = 0.5F,
                };
            }

            if (DownLoadLabel == null || DownLoadLabel.IsDisposed)
            {
                DownLoadLabel = new MirLabel
                {
                    AutoSize = true,
                    BackColour = Color.Transparent,
                    ForeColour = Color.Lime,
                    Parent = DownloadBaseLabel,
                };

                DownLoadLabel.SizeChanged += (o, e) => DownloadBaseLabel.Size = DownLoadLabel.Size;
            }

            DownLoadLabel.Text = "";
        }

        private static void CreateDebugLabel()
        {
            if (!Settings.DebugMode) return;

            if (DebugBaseLabel == null || DebugBaseLabel.IsDisposed)
            {
                DebugBaseLabel = new MirControl
                {
                    BackColour = Color.FromArgb(50, 50, 50),
                    Border = true,
                    BorderColour = Color.Black,
                    DrawControlTexture = true,
                    Location = new Point(5, 25),
                    NotControl = true,
                    Opacity = 0.5F
                };
            }

            if (DebugTextLabel == null || DebugTextLabel.IsDisposed)
            {
                DebugTextLabel = new MirLabel
                {
                    AutoSize = true,
                    BackColour = Color.Transparent,
                    ForeColour = Color.White,
                    Parent = DebugBaseLabel,
                };

                DebugTextLabel.SizeChanged += (o, e) => DebugBaseLabel.Size = DebugTextLabel.Size;
            }

            if (DebugOverride) return;

         /*   string text;
            if (MirControl.MouseControl != null)
            {
                text = string.Format("FPS: {0}", FPS);

                if (MirControl.MouseControl is MapControl)
                {
                    text += string.Format(", Co Ords: {0}", MapControl.MapLocation);

                    //text += "\r\n";

                    //var cell = GameScene.Scene.MapControl.M2CellInfo[MapControl.MapLocation.X, MapControl.MapLocation.Y];

                    //if (cell != null)
                    //{
                    //    text += string.Format("BackImage : {0}. BackIndex : {1}. MiddleImage : {2}. MiddleIndex {3}. FrontImage : {4}. FrontIndex : {5}", cell.BackImage, cell.BackIndex, cell.MiddleImage, cell.MiddleIndex, cell.FrontImage, cell.FrontIndex);
                    //}
                }

                if (MirScene.ActiveScene is GameScene)
                {
                    //text += "\r\n";
                    text += string.Format(", Objects: {0}", MapControl.Objects.Count);
                }
                if (MirObjects.MapObject.MouseObject != null)
                {
                    text += string.Format(", Target: {0}", MirObjects.MapObject.MouseObject.Name);
                }
                else
                {
                    text += string.Format(", Target: none");
                }
            }
            else
            {
                text = string.Format("FPS: {0}", FPS);
            }

            DebugTextLabel.Text = text;
            */
        }

        public static void SendDownloadMessage(string text)
        {
            if (DownloadBaseLabel == null || DownloadBaseLabel == null)
            {
                CreateDownloadLabel();
            }

            DownLoadLabel.Text = text;
        }

        public static void SendDebugMessage(string text)
        {
            if (!Settings.DebugMode) return;

            if (DebugBaseLabel == null || DebugTextLabel == null)
            {
                CreateDebugLabel();
            }

            DebugOverride = true;

            DebugTextLabel.Text = text;
        }

        private static void CreateHintLabel()
        {
            if (HintBaseLabel == null || HintBaseLabel.IsDisposed)
            {
                HintBaseLabel = new MirControl
                {
                    BackColour = Color.FromArgb(128, 128, 50),
                    Border = true,
                    DrawControlTexture = true,
                    BorderColour = Color.Yellow,
                    ForeColour = Color.Yellow,
                    Parent = MirScene.ActiveScene,
                    NotControl = true,
                    Opacity = 0.5F
                };
            }


            if (HintTextLabel == null || HintTextLabel.IsDisposed)
            {
                HintTextLabel = new MirLabel
                {
                    AutoSize = true,
                    BackColour = Color.Transparent,
                    ForeColour = Color.White,
                    Parent = HintBaseLabel,
                };

                HintTextLabel.SizeChanged += (o, e) => HintBaseLabel.Size = HintTextLabel.Size;
            }

            if (MirControl.MouseControl == null || string.IsNullOrEmpty(MirControl.MouseControl.Hint))
            {
                HintBaseLabel.Visible = false;
                return;
            }

            HintBaseLabel.Visible = true;

            HintTextLabel.Text = MirControl.MouseControl.Hint;

            Point point = MPoint.Add(-HintTextLabel.Size.Width, 20);

            if (point.X + HintBaseLabel.Size.Width >= Settings.ScreenWidth)
                point.X = Settings.ScreenWidth - HintBaseLabel.Size.Width - 1;
            if (point.Y + HintBaseLabel.Size.Height >= Settings.ScreenHeight)
                point.Y = Settings.ScreenHeight - HintBaseLabel.Size.Height - 1;

            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;

            HintBaseLabel.Location = point;
        }

        private static void ToggleFullScreen()
        {
            Settings.FullScreen = !Settings.FullScreen;

            Program.Form.FormBorderStyle = Settings.FullScreen ? FormBorderStyle.None : FormBorderStyle.FixedDialog;

            DXManager.Parameters.Windowed = !Settings.FullScreen;
            DXManager.Device.Reset(DXManager.Parameters);
            Program.Form.ClientSize = new Size(Settings.ScreenWidth, Settings.ScreenHeight);
        }//

        public void CreateScreenShot()
        {
            Point location = PointToClient(Location);

            location = new Point(-location.X, -location.Y);

            string text = string.Format("[{0} Server {1}] {2} {3:hh\\:mm\\:ss}",
                Settings.P_ServerName.Length > 0 ? Settings.P_ServerName : "Crystal",
                MapControl.User != null ? MapControl.User.Name : "",
                Now.ToShortDateString(),
                Now.TimeOfDay);

            using (Bitmap image = GetImage(Handle, new Rectangle(location, ClientSize)))
            using (Graphics graphics = Graphics.FromImage(image))
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 3, 10), sf);
                graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 4, 9), sf);
                graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 5, 10), sf);
                graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 4, 11), sf);
                graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.White, new Point((Settings.ScreenWidth / 2) + 4, 10), sf);//SandyBrown               

                string path = Path.Combine(Application.StartupPath, @"Screenshots\");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                int count = Directory.GetFiles(path, "*.png").Length;

                image.Save(Path.Combine(path, string.Format("Image {0}.Png", count)), ImageFormat.Png);
            }
        }

        public static void SaveError(string ex)
        {
            try
            {
                if (Settings.RemainingErrorLogs-- > 0)
                {
                    File.AppendAllText(@".\Error.txt",
                                       string.Format("[{0}] {1}{2}", Now, ex, Environment.NewLine));
                    Console.WriteLine(string.Format("[{0}] {1}{2}", Now, ex, Environment.NewLine));
                }
            }
            catch
            {
            }
        }

        public static void SaveDebug(string ex)
        {
            if (!Settings.DebugMode)
                return;

            Console.WriteLine(string.Format("{0} {1}", Now.ToString("yyyy-MM-dd hh:mm:ss fff"), ex));
            try
            {
             
                {
             //     File.AppendAllText(@".\Debug.txt",
            //                          string.Format("[{0}] {1}{2}", Now, ex, Environment.NewLine));
                }
            }
            catch
            {
            }
        }


        public static string Tr(string src)
        {
            return lan.Translate(src);
        }

        public static string Format(string format, params object[] args)
        {
            format = Tr(format);
            return string.Format(format, args);
        }

        public static void SetResolution(int width, int height)
        {
            if (Settings.ScreenWidth == width && Settings.ScreenHeight == height) return;

            DXManager.Device.Clear(ClearFlags.Target, Color.Black, 0, 0);
            DXManager.Device.Present();

            DXManager.Device.Dispose();

            Settings.ScreenWidth = width;
            Settings.ScreenHeight = height;
            Program.Form.ClientSize = new Size(width, height);

            DXManager.Create();
        }


        #region ScreenCapture

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr handle);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr handle);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr handle, int width, int height);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr handle, IntPtr handle2);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr handle, int destX, int desty, int width, int height,
                                         IntPtr handle2, int sourX, int sourY, int flag);
        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr handle);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr handle, IntPtr handle2);
        [DllImport("gdi32.dll")]
        public static extern int DeleteObject(IntPtr handle);

        public static Bitmap GetImage(IntPtr handle, Rectangle r)
        {
            IntPtr sourceDc = GetWindowDC(handle);
            IntPtr destDc = CreateCompatibleDC(sourceDc);

            IntPtr hBmp = CreateCompatibleBitmap(sourceDc, r.Width, r.Height);
            if (hBmp != IntPtr.Zero)
            {
                IntPtr hOldBmp = SelectObject(destDc, hBmp);
                BitBlt(destDc, 0, 0, r.Width, r.Height, sourceDc, r.X, r.Y, 0xCC0020); //0, 0, 13369376);
                SelectObject(destDc, hOldBmp);
                DeleteDC(destDc);
                ReleaseDC(handle, sourceDc);

                Bitmap bmp = Image.FromHbitmap(hBmp);


                DeleteObject(hBmp);

                return bmp;
            }

            return null;
        }
        #endregion

        #region Idle Check
        private static bool AppStillIdle
        {
            get
            {
                PeekMsg msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out PeekMsg msg, IntPtr hWnd, uint messageFilterMin,
                                               uint messageFilterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct PeekMsg
        {
            private readonly IntPtr hWnd;
            private readonly Message msg;
            private readonly IntPtr wParam;
            private readonly IntPtr lParam;
            private readonly uint time;
            private readonly Point p;
        }
        #endregion

        private void CMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GameScene.Scene != null && !GameScene.Closed)
            {
                e.Cancel = true;
                GameScene.Scene.QuitGame();
            }
        }

        public static void DecompressToFile(byte[] raw, string filePath)
        {
            using (GZipStream gStream = new GZipStream(new MemoryStream(raw), CompressionMode.Decompress))
            {
                using (var output = File.Create(filePath))
                {
                    gStream.CopyTo(output);
                }
            }
        }

        private static void DownloadASync(string fileName, Action action)
        {
            string url = Settings.P_Host + fileName;
            try
            {
                CMain.RunAction(() => { CMain.SendDownloadMessage(String.Format("正在下载[{0}],请耐心等待！", fileName)); });

                TimeSpan ts;
                using (WebClient client = new WebClient())
                {
                    if (Settings.P_NeedLogin) client.Credentials = new NetworkCredential(Settings.AccountID, Settings.Password);
        

                    DateTime beforDT = System.DateTime.Now;
                    String oldfilename = Settings.P_Client + fileName + "o";
                //    File.AppendAllText(@".\log.txt",
                  //           string.Format("[{0}] {1}{2}", DateTime.Now, url + " download begin", Environment.NewLine));

                    client.Proxy = WebRequest.DefaultWebProxy;
                    client.DownloadProgressChanged += (o, e) =>
                    {
                        ts = DateTime.Now.Subtract(beforDT);
                        int speed = (int)(e.BytesReceived / ts.TotalSeconds / 1000);
                        string str = String.Format("正在下载[{0}] 速度[{1}]kb/s 时间[{4}]s 进度[{2}]% 总共[{3}]kb", fileName, speed, e.ProgressPercentage, e.TotalBytesToReceive/1024, (int)ts.TotalSeconds);
                        CMain.RunAction(() => { CMain.SendDownloadMessage(str); });
                    };

                    client.DownloadFileCompleted += (o, e) =>
                    {
                        if (e.Error != null)
                        {
                            File.AppendAllText(@".\Error.txt",
                                   string.Format("[{0}] {1}{2}", DateTime.Now, fileName + " 下载失败. (" + e.Error.Message + ")", Environment.NewLine));
                        }
                        else
                        {
 //                           File.WriteAllBytes(Settings.P_Client + fileName, e.Result);
                            String newfilename = Settings.P_Client + fileName;
                            if (File.Exists(newfilename))
                                File.Delete(newfilename);
                            File.Move(oldfilename, newfilename);

                            RunAction(action);
                        }
                        CMain.RunAction(() => { CMain.SendDownloadMessage(""); });
                        isDownloading = false;
                    };

                    if (!Directory.Exists(Settings.P_Client + Path.GetDirectoryName(fileName)))
                         Directory.CreateDirectory(Settings.P_Client + Path.GetDirectoryName(fileName));

                    client.DownloadFileAsync(new Uri(url), oldfilename);
                }

            }
            catch (Exception e)
            {
                File.AppendAllText(@".\Error.txt",
                                  string.Format("[{0}] {1}{2}", DateTime.Now, url + " download error" + " " + e.ToString(), Environment.NewLine));
                CMain.RunAction(() => { CMain.SendDownloadMessage(""); });
            }
        }

        public static void TaskDownload(string _fileName, Action action)
        {
            if (!Settings.BackDownload)
                return;

            if (_downloadFiles.Contains(_fileName))
                return;

            _downloadFiles.Add(_fileName);
            DownloadTask task = new DownloadTask()
            {
                FileName = _fileName,
                Action = action
            };

            _downloadQueue.Enqueue(task);
        }

        private static void RunAction(Action action)
        {
            if (action == null)
                return;

            queue.Enqueue(action);
        }
    }
}
