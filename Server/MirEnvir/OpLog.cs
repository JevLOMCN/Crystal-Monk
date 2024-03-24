using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirEnvir
{
    public class OpLog
    {
        public int TopOnlineNum;
        public int TotalOnlineTime;
        public string TopOnlineTime;
        public int NewAccountNum;
        public int NewCharactorNum;
        public HashSet<string> DAU = new HashSet<string>();

        internal void LogOnline(int count)
        {
            if (count > TopOnlineNum)
            {
                TopOnlineNum = count;
                TopOnlineTime = DateTime.Now.ToString();
            }

            TotalOnlineTime += count;
        }

        internal void ProcessNewDay()
        {
            TopOnlineNum = 0;
            TopOnlineTime = DateTime.Now.ToString();

            DAU.Clear();
            TotalOnlineTime = 0;
        }
    }
}
