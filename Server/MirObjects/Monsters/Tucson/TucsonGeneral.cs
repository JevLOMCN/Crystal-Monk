using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class TucsonGeneral : MonsterObject
    {
        public long NukeDelay = Envir.Time;

        protected internal TucsonGeneral(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
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


            int dist = Functions.Distance(CurrentLocation, Target.CurrentLocation);
            if (dist > 2 && Envir.Random.Next(3) == 0)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;

                int damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
                DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.MACAgility, 1);
                ActionList.Add(action);
            }
            else if (dist < 2 && Envir.Random.Next(4) == 0)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                RangeAttack();
            }
            else if (dist < 2 && HP < MaxHP / 1.25F && NukeDelay < Envir.Time)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                NukeDelay = Envir.Time + 20 * Settings.Second;
                RangeAttack3();
            }
            else if (dist < 2)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                Attack1();
            }
            else
            {
                MoveTo(Target.CurrentLocation);
            }
        }

        protected override void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];

            HalfMoonAttack(damage, DefenceType.None);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            List<MapObject> targets;
            switch (type)
            {
                case 0:
                    targets = FindAllTargets(3, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(8) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Frozen, TickSpeed = 2000 }, this);
                        else if (Envir.Random.Next(4) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Slow, TickSpeed = 2000 }, this);
                    }
                    break;

                case 1:
                    targets = FindAllTargets(2, target.CurrentLocation, false);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(3) == 1)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Stun, TickSpeed = 2000 }, this);
                    }
                    break;

                case 2:
                    targets = FindAllTargets(4, CurrentLocation, false);

                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);

                        targets[i].Pushed(this, Direction, 3);
                    }
                    break;
                default:
                    break;

            }
        }
    }
}
