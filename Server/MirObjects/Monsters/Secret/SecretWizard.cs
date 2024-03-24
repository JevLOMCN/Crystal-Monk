using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class SecretWizard : MonsterObject
    {
        protected internal SecretWizard(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 2);
            ActionList.Add(action);

            Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.SecretWizard });
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            switch (type)
            {
                case 0:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    break;

                case 1:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, (int)(1.2 * damage), DefenceType.MAC);
                    if (Envir.Random.Next(5) == 0)
                        target.ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Slow, Value = damage, TickSpeed = 1000 }, this);
                    break;

                case 2:

                    List<MapObject> targets = FindAllTargets(2, target.CurrentLocation, false);
                    if (targets.Count == 0) return;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        targets[i].Attacked(this, damage, DefenceType.MACAgility);
                    }
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