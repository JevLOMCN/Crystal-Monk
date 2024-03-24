using Server.MirNetwork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using S = ServerPackets;

namespace LoginSrv
{
    public partial class LForm : Form
    {
        private TcpListener _listener;
        public static List<GameServerInfo> ServerInfo = new List<GameServerInfo>();
        public static List<MirConnection> Connections = new List<MirConnection>();
        private static readonly ConcurrentQueue<string> DebugLog = new ConcurrentQueue<string>();
        private int _sessionID;
        public readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        private long lastTime;

        public LForm()
        {
            InitializeComponent();

            lastTime = Stopwatch.ElapsedMilliseconds;
            LoadServerInfo();
            StartTCPListener();
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            long Time = Stopwatch.ElapsedMilliseconds;
            lock (Connections)
            {
                for (int i = Connections.Count - 1; i >= 0; i--)
                    Connections[i].Process();
            }

            if (Time - lastTime > 1000 * 60 * 2)
            {
                lastTime = Time;
                EnqueueDebugging(string.Format("Connections count: {0}", Connections.Count));
            }

            while (!DebugLog.IsEmpty)
            {
                string message;

                if (!DebugLog.TryDequeue(out message)) continue;

                LogTextBox.AppendText(message);
            }
        }

        public static void EnqueueDebugging(string msg)
        {
            if (DebugLog.Count < 100)
                DebugLog.Enqueue(string.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, msg));
        }

        private void LoadServerInfo()
        {
            InIReader reader = new InIReader("./Configs/ServerList.ini");
            List<string> sections = reader.GetSections();
            for (int i = 0; i < sections.Count; ++i)
            {
                GameServerInfo info = new GameServerInfo();
                info.Name = sections[i];
                info.Ip = reader.ReadString(sections[i], "IP", "");
                info.RegionID = reader.ReadByte(sections[i], "RegionId", 1);
                info.Port = reader.ReadInt32(sections[i], "Port", 0);
                ServerInfo.Add(info);

                EnqueueDebugging(string.Format("服务器 [{0}]区服[{1}] [{2}], [{3}]", info.Name, info.RegionID, info.Ip, info.Port));
            }

            EnqueueDebugging(string.Format("load server info {0}", ServerInfo.Count));
        }

        private void StartTCPListener()
        {
            Packet.IsServer = true;
            _listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 6500);
            _listener.Start();
            _listener.BeginAcceptTcpClient(Connection, null);
        }

        private void Connection(IAsyncResult result)
        {
            if (!_listener.Server.IsBound) return;

            try
            {
                TcpClient tempTcpClient = _listener.EndAcceptTcpClient(result);
                lock (Connections)
                    Connections.Add(new MirConnection(++_sessionID, tempTcpClient));
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error Log (" + DateTime.Now.Date.ToString("dd-MM-yyyy") + ").txt",
                              String.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, ex.ToString()));
            }
            finally
            {
                while (Connections.Count >= 100)
                    Thread.Sleep(1);

                if (_listener.Server.IsBound)
                    _listener.BeginAcceptTcpClient(Connection, null);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


    }

}
