using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

// A1R1.2R2.3
namespace Server.MirObjects.Monsters
{
    class AssassinBird : MonsterObject
    {
        protected internal AssassinBird(MonsterInfo info)
            : base(info)
        {
        }

        protected override void RangeAttack()
        {
            int damage = (int)(1.2 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = (int)(1.3 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
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

            if (Envir.Random.Next(3) == 0)
            {
                RangeAttack2();
            }
            else if (Envir.Random.Next(3) == 0)
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
