using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Linq;

namespace Server.MirObjects.Monsters
{
    public class KunLun8 : MonsterObject
    {
        private long NextBuffTime;

        protected internal KunLun8(MonsterInfo info)
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

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            if (!Buffs.Any(x => x.Type == BuffType.KunLun8) && Envir.Time > NextBuffTime)
            {
                ActionTime = Envir.Time + 1800;
                AttackTime = Envir.Time + 1800;
                NextBuffTime = Envir.Time + 60000;
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
                AddBuff(new Buff { Type = BuffType.KunLun8, Caster = this, ExpireTime = Envir.Time + 40000, Visible = true });
                return;
            }

            if (Envir.Random.Next(3) == 0)
                RangeAttack();
            else
                Attack1();
        }
    }
}
