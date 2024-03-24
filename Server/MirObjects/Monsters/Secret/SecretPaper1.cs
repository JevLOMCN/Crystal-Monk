using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class SecretPaper1 : MonsterObject
    {
        private bool HasCall;
        protected internal SecretPaper1(MonsterInfo info)
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

            if (!HasCall && Envir.Random.Next(2) == 0)
            {
                MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X + 2, CurrentLocation.Y, Settings.SecretPaper1);
                if (mob != null)
                    mob.Owner = this;

                HasCall = true;
            }
        }
    }
}