using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLin4 : MonsterObject
    {
        protected internal DogYoLin4(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            switch (type)
            {
                case 0:
                    target.Attacked(this, damage, DefenceType.MAC);
                    break;

                case 1:
                    target.Attacked(this, damage, DefenceType.MAC);
                    target.Pushed(this, Direction, 6);
                    break;
      
            }
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

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged)
            {
                if (Envir.Random.Next(6) == 0)
                    MoveTo(Target.CurrentLocation);
                else
                    RangeAttack();
            }
            else if (Envir.Random.Next(3) == 0)
            {
                int damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
                DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 300, Target, damage, DefenceType.MACAgility, 1);
                ActionList.Add(action);
            }
            else
            {
                Attack1();
            }
        }
    }
}
