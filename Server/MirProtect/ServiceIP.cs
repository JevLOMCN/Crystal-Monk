using Server.MirNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server
{
    class ServiceIP
    {
        public static string BlackFile = @"./Configs/IPBlackList.txt";

        public static List<string> List = new List<string>();
        public static void Load()
        {
            List.Clear();
            if (File.Exists(BlackFile))
            {
                List.AddRange(File.ReadAllLines(BlackFile));
                foreach (var item in List)
                {
                    SMain.Enqueue("Black IP:" + item);
                }
                SMain.Enqueue("BlackList load. Count:" + List.Count);
            }
            else
            {
                SMain.Enqueue(BlackFile + "not found");
            }
        }

        public static void Reload()
        {
            Load();

            KickBlack();
        }

        public static bool ContailIP(string ip)
        {
            foreach (var item in List)
            {
                if (string.Compare(ip, item) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static void KickBlack()
        {
            List<MirConnection> ConnList = new List<MirConnection>();
            lock (SMain.Envir.Connections)
                ConnList.AddRange(SMain.Envir.Connections);

            for (int i = 0; i < ConnList.Count; i++)
            {
                MirConnection conn = ConnList[i];
                if (ContailIP(conn.IPAddress))
                {
                    SMain.Enqueue("已阻止当前连接。IP:" + conn.IPAddress + "RecvPktCount:" + conn.NetStat.RecvPktCount + "RecvDataSize:" + conn.NetStat.RecvDataSize);
                    conn.Disconnecting = true;
                }
            }
        }

        public static void AddIP(string IP)
        {
            List.Add(IP);
            SMain.Enqueue("已增加到黑名单.IP:" + IP);

            KickBlack();
        }
    }
}
