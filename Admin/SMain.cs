using System;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.IO;
using S = ServerPackets;
using C = ClientPackets;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace Admin
{
    public partial class SMain : Form
    {
        public static LoginScene LoginScene;
        private static readonly ConcurrentQueue<string> MessageLog = new ConcurrentQueue<string>();
        private static readonly ConcurrentQueue<string> DebugLog = new ConcurrentQueue<string>();
        private static readonly ConcurrentQueue<string> ChatLog = new ConcurrentQueue<string>();

        public readonly static Stopwatch Timer = Stopwatch.StartNew();
        public static long Time;

        public DataAnalysic DataAnalysic;

        public static long NextSyncTime;
        public static string SyncFileName;
        public static uint SyncFileLength;

        public static string[] funcs = { "每日活跃", "每日注册", "流失人数" };

        public SMain()
        {
            InitializeComponent();

            AutoResize();

            LoginScene = new LoginScene();
            DataAnalysic = new DataAnalysic();
  
        }

        private void AutoResize()
        {
        }

        public static void Enqueue(Exception ex)
        {
            if (MessageLog.Count < 100)
            MessageLog.Enqueue(String.Format("[{0}]: {1} - {2}" + Environment.NewLine, DateTime.Now, ex.TargetSite, ex));
        }

        public static void EnqueueDebugging(string msg)
        {
                            //               String.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, msg));
        }

        public static void EnqueueChat(string msg)
        {
        }

        public static void Enqueue(string msg)
        {
            MessageLog.Enqueue(String.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, msg));
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Time = Timer.ElapsedMilliseconds;

                Network.Process();

                while (!MessageLog.IsEmpty)
                {
                    string message;

                    if (!MessageLog.TryDequeue(out message)) continue;

                    LogTextBox.AppendText(message);
                }

                if (Time > NextSyncTime && SyncLogCheckBox.Checked)
                    SyncFile();

                ProcessPlayersOnlineTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SyncFile()
        {
            Enqueue("开始同部文件" + SyncFileName);

            long lStartPos = 0;
            if (File.Exists(SyncFileName))
            {
                using (FileStream fileStream = File.OpenWrite(SyncFileName))
                {
                    lStartPos = fileStream.Length;
                }
            }

            Network.Enqueue(new C.DownloadFile() { FileName = SyncFileName, Start = (uint)lStartPos });

            if (SyncFileLength == 0 || lStartPos + 255 < SyncFileLength)
            {
                NextSyncTime = Time + 5 * 1000;
            }
            else
                NextSyncTime = Time + Settings.SyncInvertal * 60 * 1000;
        }

        private void SMain_FormClosing(object sender, FormClosingEventArgs e)
        {
    //        Envir.Stop();
        }

        public static void SaveError(string ex)
        {
            try
            {
                Enqueue(ex);
            }
            catch
            {
            }
        }

        private void SMain_Load(object sender, EventArgs e)
        {
            try
            {
                string text = File.ReadAllText(Application.StartupPath + "./recentopen.txt");
                string[] arr = text.Split(' ');
                OpenProject(arr[0], arr[1]);

                AnlysicComboBox.DataSource = funcs;
            }
            catch (Exception )
            {
                Enqueue("open file error");
            }
        }

        private void queryOnlineBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@onlinecount");
        }

        private void queryBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@querycharacter " + textBoxNickName.Text);
        }

        private void giveGoldBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@givegold " + textBoxNickName.Text + " " + textBoxGold.Text);
        }

        private void giveCreditBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@givecredit " + textBoxNickName.Text + " " + textBoxCredit.Text);
        }

        private void rmGoldBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@removegold " + textBoxNickName.Text + " " + textBoxRmGold.Text);
        }

        private void rmCreditBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@removecredit " + textBoxNickName.Text + " " + textBoxRmCredit.Text);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfForm confForm = new ConfForm();
            confForm.ShowDialog();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "打开";
            fdlg.Filter = "All files（*.*）|*.ini";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                OpenProject(fdlg.FileName, fdlg.SafeFileName);
            }
        }

        private void OpenProject(string fileName, string safeFileName)
        {
            InIReader Reader = new InIReader(fileName);

            Text = "Admin. " + safeFileName;

            Settings.ReadIni<Settings>(Reader);

            Network.ServerInfo = new GameServerInfo();
            Network.ServerInfo.Ip = Settings.ServerIP;
            Network.ServerInfo.Port = Settings.ServerPort;
            Network.Connect();

            File.WriteAllText(Application.StartupPath + "./recentopen.txt", fileName + " " + safeFileName);
        }

        private void configToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ConfForm confForm = new ConfForm(Settings.Name);
            confForm.ShowDialog();
        }

        private void CalculateLevelCost(DirectoryInfo dir)
        {
            Regex reg = new Regex("(\\d\\d:\\d\\d:\\d\\d), .*, Levelled to (\\d+), , ");

            for (int i = 0; i < dir.GetFiles().Length; ++i)
            {
                FileInfo fileInfo = dir.GetFiles()[i];
                string[] lines = File.ReadAllLines(fileInfo.FullName);

                for (int j=0; j<lines.Length; ++j)
                {
                    Match match = reg.Match(lines[j]);
                    if (!match.Success)
                        continue;

                    string hms = match.Groups[1].Value;
                    string level = match.Groups[2].Value;

                    string date = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                    DateTime dateTime = Convert.ToDateTime(date + @" " + hms);
                    Enqueue(string.Format("{0} {1}", dateTime, level));
                }
            }
        }

        private void DailyActive()
        {
            DirectoryInfo Dir = new DirectoryInfo(DataAnalysic.SelectedPath);

            DirectoryInfo[] DirSub = Dir.GetDirectories();

            Enqueue("玩家总数：" + DirSub.Length);

            Dictionary<DateTime, int> daily = new Dictionary<DateTime, int>();
            for (int i = 0; i < DirSub.Length; ++i)
            {
                DirectoryInfo[] udirs = DirSub[i].GetDirectories();
                Dictionary<DateTime, int> tmp = new Dictionary<DateTime, int>();
                for (int j = 0; j < udirs.Length; ++j)
                {
                    FileInfo[] files = udirs[j].GetFiles();
                    for (int k = 0; k < files.Length; ++k)
                    {
                        string name = files[k].Name.Substring(0, files[k].Name.Length - 4);
                        DateTime date = DateTime.Parse(name);
                        HashAdd(tmp, date, 1);
                    }
                }

                foreach (var k in tmp.Keys)
                    HashAdd(daily, k, 1);
            }

            var list = daily.Keys.ToList();
            list.Sort();

            mainChart.Series.Clear();
            //new 一个叫做【Strength】的系列
            Series Strength = new Series("活跃度");

            Strength.ChartType = SeriesChartType.Column;
            foreach (var key in list)
            {
                Enqueue(string.Format("Active {0}, {1}", key, daily[key]));
                Strength.Points.AddXY(key.ToString("m"), daily[key]);
            }
            mainChart.Series.Add(Strength);
        }

        private void DailyRegister()
        {
            DirectoryInfo Dir = new DirectoryInfo(DataAnalysic.SelectedPath);
            DirectoryInfo[] DirSub = Dir.GetDirectories();

            Enqueue("玩家总数：" + DirSub.Length);

            Dictionary<DateTime, int> register = new Dictionary<DateTime, int>();
            for (int i = 0; i < DirSub.Length; ++i)
            {
                DirectoryInfo[] udirs = DirSub[i].GetDirectories();
                DateTime early = DateTime.MaxValue;

                for (int j = 0; j < udirs.Length; ++j)
                {
                    FileInfo[] files = udirs[j].GetFiles();
                    for (int k = 0; k < files.Length; ++k)
                    {
                        DateTime date = DateTime.Parse(files[k].Name.Substring(0, files[k].Name.Length - files[k].Extension.Length));
                        if (date < early)
                            early = date;
                    }
                }

                HashAdd(register, early, 1);
            }

            var list = register.Keys.ToList();
            list.Sort();

            mainChart.Series.Clear();
            //new 一个叫做【Strength】的系列
            Series Strength = new Series("注册");

            Strength.ChartType = SeriesChartType.Column;
            foreach (var key in list)
            {
                Strength.Points.AddXY(key.ToString("m"), register[key]);
            }
            mainChart.Series.Add(Strength);
        }

        private void DailyLose()
        {
            DirectoryInfo Dir = new DirectoryInfo(DataAnalysic.SelectedPath);
            DirectoryInfo[] DirSub = Dir.GetDirectories();

            Enqueue("玩家总数：" + DirSub.Length);

            Dictionary<DateTime, List<string>> lose = new Dictionary<DateTime, List<string>>();
            for (int i = 0; i < DirSub.Length; ++i)
            {
                DirectoryInfo[] udirs = DirSub[i].GetDirectories();
                DateTime late = DateTime.MinValue;

                for (int j = 0; j < udirs.Length; ++j)
                {
                    FileInfo[] files = udirs[j].GetFiles();
                    for (int k = 0; k < files.Length; ++k)
                    {
                        DateTime date = DateTime.Parse(files[k].Name.Substring(0, files[k].Name.Length - files[k].Extension.Length));
                        if (date > late)
                            late = date;
                    }
                }

                if (late.Date == DateTime.Now.Date)
                    continue;

                if (!lose.ContainsKey(late))
                    lose[late] = new List<string>();

                lose[late].Add(DirSub[i].Name);
            }

            var list = lose.Keys.ToList();
            list.Sort();

            mainChart.Series.Clear();
            //new 一个叫做【Strength】的系列
            Series Strength = new Series("流失");

            Strength.ChartType = SeriesChartType.Column;
            foreach (var key in list)
            {
                Strength.Points.AddXY(key.ToString("m"), lose[key].Count);
                foreach(var un in lose[key])
                {
                    Enqueue(string.Format("Lose {0}, {1}", key, un));
                }
            }
            mainChart.Series.Add(Strength);
        }

        private void HashAdd(Dictionary<DateTime, int> dict, DateTime name, int v)
        {
            if (dict.ContainsKey(name))
                dict[name] = dict[name] + 1;
            else
                dict[name] = 1;
        }

        private void scanBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择目录";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataAnalysic.SelectedPath = dialog.SelectedPath;
                dirTextBox.Text = dialog.SelectedPath;
            }
        }

        private void AnlysicComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (AnlysicComboBox.SelectedIndex)
            {
                case 0:
                    DailyActive();
                    break;

                case 1:
                    DailyRegister();
                    break;

                case 2:
                    DailyLose();
                    break;
            }

        }

        private void LevelBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@level " + textBoxNickName.Text + " " + LevelTextBox.Text);
        }

        public void ProcessPlayersOnlineTab()
        {
            if (OnlinePlayersListView.Items.Count != LoginScene.OnlinePlayers.Count)
            {
                OnlinePlayersListView.Items.Clear();

                for (int i = 0; i < LoginScene.OnlinePlayers.Count; i++)
                {
                    SelectInfo character = LoginScene.OnlinePlayers[i];

                    ListViewItem ListItem = new ListViewItem(character.Index.ToString()) { Tag = character };
                    ListItem.SubItems.Add(character.Name);
                    ListItem.SubItems.Add(character.Level.ToString());

                    OnlinePlayersListView.Items.Add(ListItem);
                }
            }
        }

        private void QueryOnlineBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@QUERYONLINE");
        }

        private void FileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            SyncFileName = FileNameTextBox.Text;
        }

        private void TodayLogButton_Click(object sender, EventArgs e)
        {
            FileNameTextBox.Text = @"./Logs/Log (" + DateTime.Now.Date.ToString("dd-MM-yyyy") + ").txt";
            SyncFileName = FileNameTextBox.Text;
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            FileNameTextBox.Text = @"./Logs/Log (" + monthCalendar1.SelectionStart.ToString("dd-MM-yyyy") + ").txt";
            SyncFileName = FileNameTextBox.Text;
        }

        private void reloaddrops_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@reloaddrops");
        }

        private void ReloadItemsButton_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@reloaditems");
        }

        private void ReloadMonstersBtn_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@reloadmonsters");
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Network.EnqueueChat("@reloadregion");
        }
    }
}
