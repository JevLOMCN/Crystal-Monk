using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class WereTiger : MonsterObject
    {
        protected internal WereTiger(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);

        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);

            if (Envir.Random.Next(4) == 1)
               Target.Pushed(this, Direction, 1 + Envir.Random.Next(2));
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
                RangeAttack();
            }           
            else
            {
                Attack1();
            }
        }
    }
}
