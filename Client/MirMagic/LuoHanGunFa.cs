using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.MirObjects;
using Client.MirScenes;
using Client.MirNetwork;

using S = ServerPackets;
using C = ClientPackets;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirMagic
{
    class LuoHanGunFa : BaseMagic
    {
        public override void OnMagicBegin(PlayerObject player)
        {
          
        }

        public override void OnDrawEffect(PlayerObject player, MirAction action)
        {
            if (action != MirAction.Attack1)
                return;

            MirDirection Direction = player.Direction;
            int SpellLevel = player.SpellLevel;
            int FrameIndex = player.FrameIndex;
            Point DrawLocation = player.DrawLocation;
            Libraries.Magic4.DrawBlendEx((int)Direction * 6 + FrameIndex, DrawLocation, Color.White, true);
        }
    }
}
