using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    class CatBigBoss : MonsterObject
    {
        public List<MonsterObject> Slaves = new List<MonsterObject>();
        public long ShieldTime;
        public bool HasThunderShield;

        public CatBigBoss(MonsterInfo info)
            : base(info)
        {

        }

        protected override bool InAttackRange()
        {
            return InAttackRange(7);
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.Damage, Envir.Time + 1300, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }


        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (HasThunderShield)
                damage = (int)(damage * 1.5);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

            List<MapObject> targets = FindAllTargets(1, Target.CurrentLocation, false);
            if (targets.Count != 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, targets[i], damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
            }
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (HasThunderShield)
                damage = (int)(damage * 1.5);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

            List<MapObject> targets = FindAllTargets(2, Target.CurrentLocation, false);
            if (targets.Count != 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 2000, targets[i], damage, DefenceType.MACAgility);
                    ActionList.Add(action);

                    if (Envir.Random.Next(6) == 0)
                        targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Stun, Value = damage, TickSpeed = 2000 }, this);
                }
                Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.CatBigBossThunder, EffectType = 0, DelayTime = 2000 });
            }
        }

        protected override void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (HasThunderShield)
                damage = (int)(damage * 1.5);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });

            List<MapObject> targets = FindAllTargets(8, CurrentLocation, false);

            if (targets.Count != 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, targets[i], damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                    Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.CatBigBossThunder, EffectType = 1, DelayTime = 2000 });
                }
            }
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3});

            CallSlave();
        }

        protected void CallSlave()
        {
            if (SlaveList.Count > 5)
                return;

            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y - 2, Settings.CatSlave1);
            if (mob != null)
            {
                mob.Owner = this; ;
                SlaveList.Add(mob);
            }

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y + 2, Settings.CatSlave2);
            if (mob != null)
            {
                mob.Owner = this;
                SlaveList.Add(mob);
            }
        }

        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (Envir.Time > ShieldTime && HasThunderShield)
            {
                HasThunderShield = false;
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.CatBigBossShield, EffectType = 1});
            }

            if (InAttackRange())
            {
                Attack();
                if (Target.Dead)
                    FindTarget();

                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);

            SlaveList.RemoveAll(e => (e.Dead || e.Node == null));
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
            
            if (HP < MaxHP * 0.6 && !HasThunderShield && Envir.Random.Next(4) == 0)
            {
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.CatBigBossShield });
                HasThunderShield = true;
                ShieldTime = Envir.Time + 8000;
            }
            else if (HP < MaxHP * 0.7 && Envir.Random.Next(8) == 0)
                RangeAttack4();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack3();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else
            {
                bool range = !Functions.InRange(Target.CurrentLocation, CurrentLocation, 1);
                if (range)
                {
                    RangeAttack2();
                }
                else
                {
                    if (Envir.Random.Next(5) == 0)
                        RangeAttack();
                    else
                        Attack1();
                }
            }


            ActionTime = Envir.Time + 800;
            AttackTime = Envir.Time + AttackSpeed;
        }
    }
}
