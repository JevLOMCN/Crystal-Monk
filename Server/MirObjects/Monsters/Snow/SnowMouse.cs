using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class SnowMouse : HarvestMonster
    {
        protected internal SnowMouse(MonsterInfo info)
            : base(info)
        {
        }

        protected override void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            target.Attacked(this, damage, DefenceType.MAC);
            if (Envir.Random.Next(3) == 0)
            Target.ApplyPoison(new Poison
            {
                Owner = this,
                Duration = 5,
                PType = PoisonType.Slow,
                TickSpeed = 1000
            }, this);

            Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.SnowMouse });
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

            Attack1();
        }
    }
}
