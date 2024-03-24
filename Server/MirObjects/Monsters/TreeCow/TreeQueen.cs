using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;
using System;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class TreeQueen : MonsterObject
    {
        protected override bool CanMove { get { return false; } }

        public bool RecallComplete;

        protected internal TreeQueen(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return InAttackRange(10);
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
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 2);
            if (HP < MaxHP * 2 / 3 && !RecallComplete)
            {
                CallDragon();
                RecallComplete = true;
            }
            else if ((ranged) || Envir.Random.Next(5) == 0)
                RangeAttack3();
            else if (Envir.Random.Next(4) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(4) == 0)
                RangeAttack();
            else
                Attack1();

            ActionTime = Envir.Time + 800;
            AttackTime = Envir.Time + AttackSpeed;

        }

        private void CallDragon()
        {
            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X + Envir.Random.Next(5) + 1, CurrentLocation.Y, Settings.TreeQueen1);
            if (mob != null)
            {
                mob.Owner = this; ;
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y + Envir.Random.Next(5) + 1, Settings.TreeQueen1);
            if (mob != null)
            {
                mob.Owner = this; ;
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X + Envir.Random.Next(5) + 1, CurrentLocation.Y + Envir.Random.Next(5) + 1, Settings.TreeQueen1);
            if (mob != null)
            {
                mob.Owner = this; ;
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X - Envir.Random.Next(5) - 1, CurrentLocation.Y, Settings.TreeQueen1);
            if (mob != null)
            {
                mob.Owner = this; ;
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y - Envir.Random.Next(5) - 1, Settings.TreeQueen2);
            if (mob != null)
            {
                mob.Owner = this; ;
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X - Envir.Random.Next(5) - 1, CurrentLocation.Y - Envir.Random.Next(5) - 1, Settings.TreeQueen2);
            if (mob != null)
            {
                mob.Owner = this; ;
            }
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
                Attack();
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
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
                    targets = FindAllTargets(3, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                    }
                    break;

                case 1:
                    targets = FindAllTargets(3, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(6) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Paralysis, Value = damage, TickSpeed = 2000 }, this);
                    }
                    break;

                case 2:
                    targets = FindAllTargets(1, CurrentLocation, false);

                    if (targets.Count != 0)
                    {
                        for (int i = 0; i < targets.Count; i++)
                        {
                            if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                            targets[i].Pushed(this, Functions.DirectionFromPoint(CurrentLocation, targets[i].CurrentLocation), Envir.Random.Next(3) + 2);
                        }
                    }
                    break;

                case 3:
                    targets = FindAllTargets(10, CurrentLocation);

                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage * 2, DefenceType.MAC);
                        Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.TreeQueen });
                        if (Envir.Random.Next(6) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Stun, Value = damage,
                                TickSpeed = 2000 }, this);
                    }
                    break;
            }

        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action); ;
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 2);
            ActionList.Add(action);
        }

        protected override void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 3);
            ActionList.Add(action);
        }
    }
}
