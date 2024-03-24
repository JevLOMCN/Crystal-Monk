﻿using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

// A1PR1
namespace Server.MirObjects.Monsters
{
    class GasToad : MonsterObject
    {
        protected internal GasToad(MonsterInfo info)
            : base(info)
        {
        }

        protected override void RangeAttack()
        {
            int damage = (int)(1.2 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.Attacked(this, damage, defence);
            target.ApplyPoison(new Poison { PType = PoisonType.Green, Duration = 15, Value = damage / 10, TickSpeed = 2000 }, this);
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
    }
}
