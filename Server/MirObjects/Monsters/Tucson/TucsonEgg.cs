using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class TucsonEgg : MonsterObject
    {
        protected internal TucsonEgg(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool CanMove { get { return false; } }
        public override void Turn(MirDirection dir) { }
        public override bool Walk(MirDirection dir) { return false; }

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

            CallDragon();
            Die();
        }

        private void CallDragon()
        {
            RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y, Settings.TucsonEgg);
        }
    }
}
