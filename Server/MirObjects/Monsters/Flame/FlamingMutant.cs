using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class FlamingMutant : MonsterObject
    {
        private const byte AttackRange = 8;
        public long WebTime;
        protected internal FlamingMutant(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }

        protected override void Attack()
        {

            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            if (!ranged)
            {

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].IsAttackTarget(this) &&
                        !targets[i].Dead)
                    {
                        int damage = GetAttackPower(MinDC, MaxDC);
                        if (damage == 0)
                            return;
                        DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, targets[i], damage, DefenceType.ACAgility);
                        ActionList.Add(action);
                        damage = GetAttackPower(MinDC, MaxDC);
                        targets[i].Attacked(this, damage, DefenceType.ACAgility);
                    }
                }
            }
            else
            {
                if (Envir.Random.Next(5) == 0)
                {
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

                    int damage = GetAttackPower(MinMC, MaxMC);
                    if (damage == 0) return;

                    int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 20 + 500; //50 MS per Step

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
                else
                {
                    MoveTo(Target.CurrentLocation);
                }

            }


            if (Target.Dead)
                FindTarget();

        }

        public void PerformWebAttack()
        {
            List<MapObject> targets = FindAllTargets(1, Target.CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsAttackTarget(this) &&
                    !targets[i].Dead)
                {
                    targets[i].ApplyPoison(new Poison { Duration = 5, Owner = this, PType = PoisonType.Slow, TickSpeed = 1000, Value = 10 }, this);
                    Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Direction = Direction, Effect = SpellEffect.MutantWebHold, Time = 5 });
                }
            }
            Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Direction = Direction, Effect = SpellEffect.MutantWeb });
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
                if (Envir.Time > WebTime)
                {
                    PerformWebAttack();
                    WebTime = Envir.Time + Settings.Second * 15;
                }
                Attack();
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);

        }
    }
}

