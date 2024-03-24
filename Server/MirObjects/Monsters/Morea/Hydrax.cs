using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class Hydrax : MonsterObject
    {
        protected internal Hydrax(MonsterInfo info)
            : base(info)
        {
        }

        protected override void RangeAttack()
        {
            int damage = (int)(1.2 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
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

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            target.Attacked(this, damage, DefenceType.MAC);

            if (Envir.Random.Next(5) == 0)
            {
                target.ApplyPoison(new Poison { Owner = this, Duration = 12, PType = PoisonType.Green, Value = damage / 20, TickSpeed = 1000 }, this);
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Hydrax });
            }
        }
    }
}
