using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S = ServerPackets;
using C = ClientPackets;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;

namespace Admin
{
    public class LoginScene
    {
        public List<SelectInfo> OnlinePlayers = new List<SelectInfo>();

        public void ProcessPacket(Packet p)
        {
            switch (p.Index)
            {
                case (short)ServerPacketIds.Connected:
                    Network.Connected = true;
                    SendVersion();
                    break;
                case (short)ServerPacketIds.ClientVersion:
                    ClientVersion((S.ClientVersion)p);
                    break;
                case (short)ServerPacketIds.LoginSuccess:
                    Login((S.LoginSuccess)p);
                    break;
                case (short)ServerPacketIds.Chat:
                    ReceiveChat((S.Chat)p);
                    break;
                case (short)ServerPacketIds.OnlinePlayers:
                    UpdateOnlinePlayers((S.OnlinePlayers)p);
                    break;
                case (short)ServerPacketIds.DownloadFile:
                    DownloadFile((S.DownloadFile)p);
                    break;
                default:
                    break;
            }
        }

        private void DownloadFile(S.DownloadFile p)
        {
            using (FileStream fs = new FileStream(p.FileName, FileMode.Append))
            {
                fs.Write(p.Data, 0, Math.Min((int)(p.FileLength - p.Start), p.Data.Length));
            }

            SMain.SyncFileLength = p.FileLength;
        }

        private void UpdateOnlinePlayers(S.OnlinePlayers p)
        {
            OnlinePlayers = p.list;
        }

        private void ReceiveChat(S.Chat p)
        {
            SMain.Enqueue(p.Message);
        }

        private void ClientVersion(S.ClientVersion p)
        {
            switch (p.Result)
            {
                case 0:
                    SMain.Enqueue("Wrong version, please update your game.\nGame will now Close");

                    Network.Disconnect();
                    break;
                case 1:
                    Network.Enqueue(new ClientPackets.Login { AccountID = Settings.UserName,
                    Password = Settings.Password, RegionId = 1 });
                    break;
            }
        }


        private void Login(S.LoginSuccess p)
        {
            SMain.Enqueue("LoginSuccess");

            if (p.Characters.Count > 0)
            {
                SMain.Enqueue(p.Characters[0].ToString());

                Network.Enqueue(new C.StartGame
                {
                    CharacterIndex = p.Characters[0].Index
                });
            }
        }

        private void SendVersion()
        {
            C.ClientVersion p = new C.ClientVersion();
            try
            {
                byte[] sum;
                using (MD5 md5 = MD5.Create())
                using (FileStream stream = File.OpenRead(Application.ExecutablePath))
                    sum = md5.ComputeHash(stream);

                p.VersionHash = sum;
                Network.Enqueue(p);
                SMain.Enqueue("send version");
            }
            catch (Exception ex)
            {
                SMain.Enqueue(ex.ToString());
            }
        }
    }
}
