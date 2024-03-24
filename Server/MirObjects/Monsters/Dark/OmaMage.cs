﻿using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OmaMage : MonsterObject
    {
        private const byte AttackRange = 6;

        protected internal OmaMage(MonsterInfo info)
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

                int damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                Target.Attacked(this, damage, DefenceType.ACAgility);
            }
            else
            {
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

                int damage = GetAttackPower(MinMC, MaxMC);
                if (damage == 0) return;

                int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                if (Envir.Random.Next(Settings.PoisonResistWeight) >= Target.PoisonResist)
                {
                    if (Envir.Random.Next(30) == 0)
                    {
                        // TODO
                       // Target.ApplyPoison(new Poison { Owner = this, PType = PoisonType.Burning, Duration = GetAttackPower(MinMC, MaxMC), TickSpeed = 1000 }, this);
                    }
                    if (Envir.Random.Next(25) == 0)
                    {
                        Target.ApplyPoison(new Poison { Owner = this, PType = PoisonType.Red, Duration = GetAttackPower(MinDC, MaxDC), TickSpeed = 1000 }, this);
                    }
                    if (Envir.Random.Next(24) == 0)
                    {
                        Target.ApplyPoison(new Poison { Owner = this, PType = PoisonType.Green, Duration = GetAttackPower(MinSC, MaxSC), TickSpeed = 1000 }, this);
                    }
                }
            }


            if (Target.Dead)
                FindTarget();

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

            MoveTo(Target.CurrentLocation);

        }
    }
}