using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class TucsonPlagued : MonsterObject
    {
        protected internal TucsonPlagued(MonsterInfo info)
            : base(info)
        {
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1600, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);

            if (Envir.Random.Next(6) == 1)
            {
                Target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = 3, TickSpeed = 1000 }, this);
            }
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

            if (Envir.Random.Next(3) == 0)
            {
                RangeAttack();
            }
            else if (Envir.Random.Next(3) == 0)
            {
                RangeAttack2();
            }          
            else
            {
                Attack1();
            }
        }
    }
}
