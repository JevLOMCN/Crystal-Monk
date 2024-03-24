using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class IcePhantom : MonsterObject
    {
        protected internal IcePhantom(MonsterInfo info)
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

            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            if (targets.Count == 0) return;

            for (int i = 0; i < targets.Count; i++)
                targets[i].Attacked(this, damage, DefenceType.MACAgility);
        }

        public override void Die()
        {
            ActionList.Add(new DelayedAction(DelayedType.Die, Envir.Time + 500));
            base.Die();
        }

        protected override void CompleteDeath(IList<object> data)
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;
            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            if (targets.Count == 0) return;

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].Attacked(this, damage, DefenceType.ACAgility) <= 0) continue;

                if (Envir.Random.Next(5) == 0)
                    targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Stun, TickSpeed = 1000 }, this);
            }
        }
    }
}