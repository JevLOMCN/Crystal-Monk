using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Client;
using System.Net.Sockets;
using C = ClientPackets;
using S = ServerPackets;
using Client.MirNetwork;

namespace Launcher
{
    public partial class AMain : Form
    {
        public bool Completed;
        
        private Stopwatch _stopwatch = Stopwatch.StartNew();

        public Thread _workThread;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private S.ServerList serverlist;

        private Config ConfigForm = new Config();

        public AMain()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(1, 0, 0);
            TransparencyKey = Color.FromArgb(1, 0, 0);
        }

        public static void SaveError(string ex)
        {
            try
            {
                if (Settings.RemainingErrorLogs-- > 0)
                {
                    File.AppendAllText(@".\Error.txt",
                                       string.Format("[{0}] {1}{2}", DateTime.Now, ex, Environment.NewLine));
                }

                Network.Enqueue(new C.ReportIssue {Image=new byte[0], Message = ex});
            }
            catch
            {
            }
        }

        public void Start()
        {
            Completed = true;
        }

        private void AMain_Load(object sender, EventArgs e)
        {
          //  if (Settings.P_BrowserAddress != "") Main_browser.Navigate(new Uri(Settings.P_BrowserAddress));

            Launch_pb.Enabled = false;
            ProgressCurrent_pb.Width = 5;
            TotalProg_pb.Width = 5;
            Version_label.Text = "Version " + Application.ProductVersion;

            if (Settings.P_ServerName != String.Empty)
            {
                Name_label.Visible = true;
                Name_label.Text = Settings.P_ServerName;
            }
            _workThread = new Thread(Start) { IsBackground = true };
            _workThread.Start();
        }

        private void Launch_pb_Click(object sender, EventArgs e)
        {
            Launch();
        }

        private void Launch()
        {
            if (serverlist.servers.Count == 0)
            {
                MessageBox.Show("Could not find server!");
                return;
            }

            if (serverTreeView.SelectedNode == null)
            {
                Network.ServerInfo = serverlist.servers[0];
            }
            else
            {
                for (int i = 0; i < serverlist.servers.Count; i++)
                {
                    if (String.CompareOrdinal(serverlist.servers[i].Name, serverTreeView.SelectedNode.Text) == 0)
                        Network.ServerInfo = serverlist.servers[i];
                }
            }

            if (Network.ServerInfo == null)
            {
                MessageBox.Show("Could not find server1!");
                return;
            }

            if (ConfigForm.Visible) ConfigForm.Visible = false;
            Program.Form = new CMain();
            Program.Form.Text = Network.ServerInfo.Name;
            Program.Form.Closed += (s, args) => this.Close();
            Program.Form.Show();
            Program.PForm.Hide();
        }

        private void Close_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            Close();
        }

        private void Movement_panel_MouseClick(object sender, MouseEventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Movement_panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Movement_panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Launch_pb_MouseEnter(object sender, EventArgs e)
        {
            Launch_pb.Image = Client.Properties.Resources.Launch_Hover;
        }

        private void Launch_pb_MouseLeave(object sender, EventArgs e)
        {
            Launch_pb.Image = Client.Properties.Resources.Launch_Base1;
        }

        private void Close_pb_MouseEnter(object sender, EventArgs e)
        {
            Close_pb.Image = Client.Properties.Resources.Cross_Hover;
        }

        private void Close_pb_MouseLeave(object sender, EventArgs e)
        {
            Close_pb.Image = Client.Properties.Resources.Cross_Base;
        }

        private void Launch_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Client.Properties.Resources.Launch_Pressed;
        }

        private void Launch_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Client.Properties.Resources.Launch_Base1;
        }

        private void Close_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Client.Properties.Resources.Cross_Pressed;
        }

        private void Close_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Client.Properties.Resources.Cross_Base;
        }

        private void ProgressCurrent_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgEnd_pb.Location = new Point((ProgressCurrent_pb.Location.X + ProgressCurrent_pb.Width), 490);
            if (ProgressCurrent_pb.Width == 0) ProgEnd_pb.Visible = false;
            else ProgEnd_pb.Visible = true;
        }

        private void Config_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Client.Properties.Resources.Config_Pressed;
        }

        private void Config_pb_MouseEnter(object sender, EventArgs e)
        {
            Config_pb.Image = Client.Properties.Resources.Config_Hover;
        }

        private void Config_pb_MouseLeave(object sender, EventArgs e)
        {
            Config_pb.Image = Client.Properties.Resources.Config_Base;
        }

        private void Config_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Client.Properties.Resources.Config_Base;
        }

        private void Config_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Hide();
            else ConfigForm.Show(Program.PForm);
            ConfigForm.Location = new Point(Location.X + Config_pb.Location.X - 183, Location.Y + 36);
        }

        private void TotalProg_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgTotalEnd_pb.Location = new Point((TotalProg_pb.Location.X + TotalProg_pb.Width), 508);
            if (TotalProg_pb.Width == 0) ProgTotalEnd_pb.Visible = false;
            else ProgTotalEnd_pb.Visible = true;
        }

        private void Main_browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
          //  if (Main_browser.Url.AbsolutePath != "blank") Main_browser.Visible = true;
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Completed)
                {
                    ActionLabel.Text = "";
                    CurrentFile_label.Text = "Up to date.";
                    SpeedLabel.Text = "";
                    ProgressCurrent_pb.Width = 550;
                    TotalProg_pb.Width = 550;
                    CurrentFile_label.Visible = true;
                    CurrentPercent_label.Visible = true;
                    TotalPercent_label.Visible = true;
                    CurrentPercent_label.Text = "100%";
                    TotalPercent_label.Text = "100%";
                    InterfaceTimer.Enabled = false;
                    Launch_pb.Enabled = true;

                    RequestServerList();

                    return;
                }

                ActionLabel.Visible = true;
                SpeedLabel.Visible = true;
                CurrentFile_label.Visible = true;
                CurrentPercent_label.Visible = true;
                TotalPercent_label.Visible = true;
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private void AMain_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
        }

        private void ActionLabel_Click(object sender, EventArgs e)
        {
          //  LabelSwitch = !LabelSwitch;
        }

        private void Credit_label_Click(object sender, EventArgs e)
        {
            if (Credit_label.Text == "Powered by Crystal M2") Credit_label.Text = "Designed by Breezer";
            else Credit_label.Text = "Powered by Crystal M2";
        }

        private void AMain_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void RequestServerList()
        {
            TcpClient client = new TcpClient { NoDelay = true };
            try
            {
                client.Connect(Settings.IPAddress, 6500);
            }
            catch(Exception e)
            {
                MessageBox.Show("获取服务器列表失败");
                return;
            }

            NetworkStream ntwStream = client.GetStream();
            if (ntwStream.CanWrite)
            {
                ClientPackets.ServerList packet = new ClientPackets.ServerList();
                byte[] data = (byte[])packet.GetPacketBytes();
                ntwStream.Write(data, 0, data.Length);

                byte[] rawData = null;
                byte[] readData = new Byte[256];

                long lastTick = DateTime.Now.Ticks;
                while (true)
                {
                    Thread.Sleep(1);
                    Int32 dataRead = ntwStream.Read(readData, 0, readData.Length);

                    if (rawData == null)
                    {
                        rawData = new byte[readData.Length];
                        Buffer.BlockCopy(readData, 0, rawData, 0, dataRead);
                    }
                    else
                    {
                        byte[] temp = rawData;
                        rawData = new byte[dataRead + temp.Length];
                        Buffer.BlockCopy(temp, 0, rawData, 0, temp.Length);
                        Buffer.BlockCopy(readData, 0, rawData, temp.Length, dataRead);
                    }

                    int result = 0;
                    Packet p = Packet.ReceivePacket(rawData, out rawData, ref result);
                    if (p != null)
                    {
                        switch (p.Index)
                        {
                            case (short)ServerPacketIds.ServerList:
                                {
                                    serverlist = (S.ServerList)p;

                                    TreeNode node1 = new TreeNode("雪域归来");
                                    serverTreeView.Nodes.Add(node1);
                                    for (int j = 0; j < serverlist.servers.Count; ++j)
                                    {
                                        node1.Nodes.Add(serverlist.servers[j].Name);
                                    }
                                    serverTreeView.ExpandAll();
                                    Launch_pb.Enabled = true;
                                }
                                break;
                        }
                    }

                    if (Launch_pb.Enabled)
                        break;

                    long elapsed = DateTime.Now.Ticks - lastTick;
                    if (new TimeSpan(elapsed).TotalSeconds > 5)
                    {
                        MessageBox.Show("request server list time out.");
                        break;
                    }
                }

                ntwStream.Close();
                client.Close();
            }
        }
    }
}
