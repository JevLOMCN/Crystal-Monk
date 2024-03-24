using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirProtect
{
    public class ProtectSettings
    {
        [InI("ProtectSettings")]
        public static int WarnDataRead = 1024;
        [InI("ProtectSettings")]
        public static int WarnRecvNumPerSec = 50;
        [InI("ProtectSettings")]
        public static bool AutoAddBlackList = true;

        public static void Load()
        {
            InIReader reader = new InIReader("./Configs/ProtectSettings.ini");
            InIAttribute.ReadInI<ProtectSettings>(reader);

            SMain.Enqueue(string.Format("WarnDataRead:{0}, WarnRecvNumPerSec:{1} AutoAddBlackList：{2}", WarnDataRead, WarnRecvNumPerSec, AutoAddBlackList));
        }
    }
}
