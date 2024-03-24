using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLin8 : MonsterObject
    {
        protected internal DogYoLin8(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool CanMove
        {
            get { return false; }
        }

        protected override void ProcessTarget()
        {
            if (!CanAttack)
                return;
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            MagicAreaAttack(2, damage, CurrentLocation);
        }

 
    }
}
