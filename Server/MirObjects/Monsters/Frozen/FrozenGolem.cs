using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class FrozenGolem : MonsterObject
    {
        protected internal FrozenGolem(MonsterInfo info)
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

            Attack1();
        }

        public override void Die()
        {
            CallDragon();
            base.Die();
        }

        private void CallDragon()
        {
            RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y, Settings.FrozenGolem, 2500);
        }
    }
}