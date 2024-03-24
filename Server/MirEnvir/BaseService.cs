using Server.MirDatabase;
using Server.MirEnvir;
using Server.MirNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirObjects
{
    public class BaseService
    {
        protected static Envir Envir
        {
            get { return SMain.Envir; }
        }

        public PlayerObject Player;

        public CharacterInfo Info
        {
            get { return Player.Info; }
        }

        public Reporting Report
        {
            get { return Player.Report; }
        }

        public MirConnection Connection
        {
            get {return Player.Connection;}
        }

        public void Broadcast(Packet p)
        {
            Player.Broadcast(p);
        }

        public void RefreshStats()
        {
            Player.RefreshStats();
        }

        public void Enqueue(Packet p)
        {
            Player.Enqueue(p);
        }

        public Packet GetUpdateInfo()
        {
            return Player.GetUpdateInfo();
        }
    }
}
