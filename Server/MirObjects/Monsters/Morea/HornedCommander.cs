using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HornedCommander : MonsterObject
    {
        private bool Shield = false;
        private long CallDelayTime;

        protected internal HornedCommander(MonsterInfo info)
            : base(info)
        {
        }

        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (InAttackRange())
            {
                Attack();
                if (Target.Dead)
                    FindTarget();

                return;
            }

            if (Envir.Random.Next(4) == 0)
            {
                RangeAttack3();
                ActionTime = Envir.Time + 3000;
                AttackTime = Envir.Time + AttackSpeed;
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);
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

            if (HP < MaxHP / 2 && Envir.Random.Next(5) == 0)
            {
                List<MapObject> targets = FindAllTargets(10, CurrentLocation);
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                    SpwanShiled(damage, targets[i].CurrentLocation.X, targets[i].CurrentLocation.Y);
                }
            }
          
            if (Envir.Random.Next(5) == 0)
            {
                RangeAttack6();
                ActionTime = Envir.Time + 6500;
                AttackTime = Envir.Time + 6500;
                return;
            }
            else if (Envir.Random.Next(10) == 0)
            {
                RangeAttack4();
            }
            else if (HP < MaxHP / 2 && Envir.Random.Next(3) == 0 && !Shield)
            {
                RangeAttack5();
                ActionTime = Envir.Time + 20000;
                AttackTime = Envir.Time + 20000;
                Shield = true;

                AddBuff(new Buff { Type = BuffType.Defence, Caster = this, ExpireTime = Envir.Time + 20000, Values = new int[] { 500 } });
                AddBuff(new Buff { Type = BuffType.MagicDefence, Caster = this, ExpireTime = Envir.Time + 20000, Values = new int[] { 500 } });
                return;
            }
            else if (Envir.Time > CallDelayTime && Envir.Random.Next(10) == 0)
            {
                CallDelayTime = Envir.Time + 30000;
                RangeAttack7();
            }
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack();
            else
                Attack1();

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.MoreaKing,
                Value = damage,
                ExpireTime = Envir.Time + 10000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = new Point(x, y),
                CurrentMap = CurrentMap,
                StartTime = Envir.Time + 800,
                Range = 3
            };

            CurrentMap.AddObject(ob);
            ob.Spawned();
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
                    targets = FindAllTargets(1, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Stun, Value = damage, TickSpeed = 1000 }, this);
                        }
                    }
                    break;

                case 1:
                    targets = FindAllTargets(4, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.HornedCommander });
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 1, PType = PoisonType.Frozen, Value = damage, TickSpeed = 1000 }, this);
                        }
                    }
                    break;

                case 3:
                    targets = FindAllTargets(4, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                    }
                    break;

                case 4:
                    MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y + 1, Settings.MoreaKnight);
                    if (mob != null)
                        mob.Owner = this; ;

                    break;

                case 5:
                    for (int j = 0; j < 3; j++)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Point location = new Point(CurrentLocation.X + (j - 1) * 7,
                                                     CurrentLocation.Y + (i - 1) * 7);

                            targets = FindAllTargets(4, location);
                            for (int k = 0; k < targets.Count; k++)
                            {
                                if (targets[k] == null || !targets[k].IsAttackTarget(this) || targets[k].CurrentMap != CurrentMap || targets[k].Node == null) continue;
                                targets[k].Attacked(this, damage, DefenceType.MAC);
                            }

                            Broadcast(new S.MapEffect { Location = location, Effect = SpellEffect.HornedCommander, Value = 1 });
                        }
                    }
                    break;


                case 6:
                    CallDragon();
                    break;
            }
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            LineAttack(7, damage, DefenceType.ACAgility, 100);
        }

        protected override void RangeAttack()
        {
            int damage = 2 * GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = (int)(GetAttackPower(MinDC, MaxDC) * 2.5);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 3500, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void RangeAttack3()
        {
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });
            Point location = Functions.PointMove(Target.CurrentLocation, Target.Direction, 1);

            TeleportOut(location, true, 7);
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 3500, Target, damage, DefenceType.MACAgility, 3);
            ActionList.Add(action);
        }

        private void RangeAttack5()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 4 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1500, Target, damage, DefenceType.MACAgility, 4);
            ActionList.Add(action);
        }

        private void CallDragon()
        {
            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y + 3, Settings.MoreaStone);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X + 4, CurrentLocation.Y, Settings.MoreaWind);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X - 3, CurrentLocation.Y, Settings.MoreaStone);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y - 4, Settings.MoreaWind);
            if (mob != null)
                mob.Owner = this;
        }

        private void RangeAttack6()
        {
            int damage = 3 * GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 5 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1500, Target, damage, DefenceType.MACAgility, 5);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 3000, Target, damage, DefenceType.MACAgility, 5);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 4500, Target, damage, DefenceType.MACAgility, 5);
            ActionList.Add(action);
        }

        private void RangeAttack7()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 6 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.MACAgility, 6);
            ActionList.Add(action);

        }
    }
}
