using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

// A1R1
namespace Server.MirObjects.Monsters
{
    class RhinoWarrior : MonsterObject
    {
        protected internal RhinoWarrior(MonsterInfo info)
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

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            if (Envir.Random.Next(4) == 0)
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
