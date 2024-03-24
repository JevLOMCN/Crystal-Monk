using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirProtect
{
    public class ServiceProtect
    {
        public static void Load()
        {
            ProtectSettings.Load();
            ServiceIP.Load();
        }

        public static void Reload()
        {
            Load();
        }
    }
}
