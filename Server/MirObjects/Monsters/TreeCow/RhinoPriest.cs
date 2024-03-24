using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

// A1R1R2
namespace Server.MirObjects.Monsters
{
    class RhinoPriest : MonsterObject
    {
        protected internal RhinoPriest(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            if (Envir.Random.Next(5) == 0)
            {
                RangeAttack2();
            }
            else if (Envir.Random.Next(5) == 0)
            {
                RangeAttack();
            }
            else
            {
                Attack1();
            }
        }
    }
}
