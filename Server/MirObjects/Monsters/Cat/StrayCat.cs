using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class StrayCat : MonsterObject
    {
        public StrayCat(MonsterInfo info)
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

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (!CanAttack) return;

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

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
        }

        protected override void RangeAttack()
        {
            int damage = (int)(1.5 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = (int)(1.5 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            List<MapObject> targets;
            switch (type)
            {
                case 0:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    break;

                case 1:
                    targets = FindAllTargets(1, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                    }
                    break;
            }
        }
    }
}
