using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class FrozenSoldier : MonsterObject
    {
        protected internal FrozenSoldier(MonsterInfo info)
            : base(info)
        {
        }
    }
}