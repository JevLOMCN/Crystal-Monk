using Server.MirNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirProtect
{
    class ServiceNetStat
    {
        public static bool Close;

        public static void AddStat(NetStat netStat, int dataRead)
        {
            if (Close)
            {
                return;
            }

            if (dataRead > ProtectSettings.WarnDataRead)
            {
                SMain.Enqueue(string.Format("消息报大小异常，可能遭受攻击 已添加到黑名单. IPAddress:{0} Length:{1}", netStat.IPAddress, dataRead));
                if (ProtectSettings.AutoAddBlackList)
                    ServiceIP.AddIP(netStat.IPAddress);
            }

            netStat.RecvDataSize += dataRead;

            if (SMain.Envir.Time != netStat.LastStatSec)
            {
                netStat.RecvNumPerSec = 0;
                netStat.LastStatSec = SMain.Envir.Time;
            }

            netStat.RecvNumPerSec++;
            if (netStat.RecvNumPerSec > ProtectSettings.WarnRecvNumPerSec)
            {
                SMain.Enqueue(string.Format("消息报频率异常，可能遭受攻击 已添加到黑名单. IPAddress:{0} RecvPktCount:{1}", netStat.IPAddress, netStat.RecvPktCount));
                if (ProtectSettings.AutoAddBlackList)
                    ServiceIP.AddIP(netStat.IPAddress);
            }

        }
    }
}
