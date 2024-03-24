using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class Nadz : MonsterObject
    {
        protected internal Nadz(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.ACAgility, 0);
            ActionList.Add(action);

            if (Envir.Random.Next(15) == 0)
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

            if (Envir.Random.Next(5) == 0)
            {
                RangeAttack();
            }
            else
            {
                Attack1();
            }
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
                    target.Attacked(this, damage, DefenceType.AC);

                    break;

                case 1:
                    targets = FindAllTargets(1, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (!targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                    }
                    break;
            }
        }
    }
}
