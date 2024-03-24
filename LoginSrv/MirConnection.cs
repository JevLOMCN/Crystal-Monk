using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using C = ClientPackets;
using S = ServerPackets;
using LoginSrv;

namespace Server.MirNetwork
{
    public class MirConnection
    {
        public readonly int SessionID;
        public readonly string IPAddress;

        private TcpClient _client;
        private ConcurrentQueue<Packet> _receiveList;
        private ConcurrentQueue<Packet> _sendList;

        private bool _disconnecting;
        public bool Connected;
        public bool Disconnecting
        {
            get { return _disconnecting; }
            set
            {
                if (_disconnecting == value) return;
                _disconnecting = value;
            }
        }
        public readonly long TimeConnected;
        public long TimeDisconnected, TimeOutTime;

        byte[] _rawData = new byte[0];
        public bool StorageSent;


        public MirConnection(int sessionID, TcpClient client)
        {
            SessionID = sessionID;
            IPAddress = client.Client.RemoteEndPoint.ToString().Split(':')[0];

            _client = client;
            _client.NoDelay = true;

            TimeConnected = 0;
            TimeOutTime = 0;


            _receiveList = new ConcurrentQueue<Packet>();
            _sendList = new ConcurrentQueue<Packet>();
            //_sendList.Enqueue(new S.Connected());

            Connected = true;
            BeginReceive();
        }

        private void BeginReceive()
        {
            if (!Connected) return;

            byte[] rawBytes = new byte[8 * 1024];

            try
            {
                _client.Client.BeginReceive(rawBytes, 0, rawBytes.Length, SocketFlags.None, ReceiveData, rawBytes);
            }
            catch
            {
                Disconnecting = true;
            }
        }
        private void ReceiveData(IAsyncResult result)
        {
            if (!Connected) return;

            int dataRead;

            try
            {
                dataRead = _client.Client.EndReceive(result);
            }
            catch
            {
                Disconnecting = true;
                return;
            }

            if (dataRead == 0)
            {
                Disconnecting = true;
                return;
            }

            byte[] rawBytes = result.AsyncState as byte[];

            byte[] temp = _rawData;
            _rawData = new byte[dataRead + temp.Length];
            Buffer.BlockCopy(temp, 0, _rawData, 0, temp.Length);
            Buffer.BlockCopy(rawBytes, 0, _rawData, temp.Length, dataRead);

            Packet p;
            while ((p = Packet.ReceivePacket(_rawData, out _rawData)) != null)
                _receiveList.Enqueue(p);

            BeginReceive();
        }
        private void BeginSend(List<byte> data)
        {
            if (!Connected || data.Count == 0) return;

            //Interlocked.Add(ref Network.Sent, data.Count);

            try
            {
                _client.Client.BeginSend(data.ToArray(), 0, data.Count, SocketFlags.None, SendData, Disconnecting);
            }
            catch
            {
                Disconnecting = true;
            }
        }
        private void SendData(IAsyncResult result)
        {
            try
            {
                _client.Client.EndSend(result);
            }
            catch
            { }
        }

        public void Enqueue(Packet p)
        {
            if (_sendList != null && p != null)
                _sendList.Enqueue(p);
        }

        public void Process()
        {
            if (_client == null || Disconnecting)
            {
                Disconnect(20);
                return;
            }

            while (!_receiveList.IsEmpty && !Disconnecting)
            {
                Packet p;
                if (!_receiveList.TryDequeue(out p)) continue;
                ProcessPacket(p);
            }

            if (_sendList == null || _sendList.Count <= 0) return;

            List<byte> data = new List<byte>();
            while (!_sendList.IsEmpty)
            {
                Packet p;
                if (!_sendList.TryDequeue(out p) || p == null) continue;
                data.AddRange(p.GetPacketBytes());
            }

            BeginSend(data);
        }

        private void ProcessPacket(Packet p)
        {
            if (p.Index != (short)ClientPacketIds.ServerList)
                return;

            S.ServerList serverList = new S.ServerList();

            for (int i = 0; i < LForm.ServerInfo.Count; ++i)
            {
                GameServerInfo info = new GameServerInfo()
                {
                    Name = LForm.ServerInfo[i].Name,
                    RegionID = LForm.ServerInfo[i].RegionID,
                    Ip = LForm.ServerInfo[i].Ip,
                    Port = LForm.ServerInfo[i].Port,
                };
    
                serverList.servers.Add(info);
            }
            Enqueue(serverList);
        }

        public void Disconnect(byte reason)
        {
            lock (LForm.Connections)
                LForm.Connections.Remove(this);

            if (_client != null) _client.Client.Dispose();
            _client = null;
        }
    }
}
