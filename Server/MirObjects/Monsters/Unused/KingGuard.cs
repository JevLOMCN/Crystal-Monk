using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MirObjects.Monsters
{
    public class KingGuard : MonsterObject
    {
        public long GroundSmashTime;
        public long HoldTime;
        public KingGuard(MonsterInfo info)
            : base(info)
        {

        }

        public void PerformGroundSmash()
        {

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
        }

        public void PerformHold()
        {

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
        }

        protected override void ProcessTarget()
        {
            if (Target == null)
                return;

            if (InAttackRange() && CanAttack)
            {
                if (Envir.Time > GroundSmashTime)
                {
                    PerformGroundSmash();
                    GroundSmashTime = Envir.Time + 10000;
                    return;
                }
                if (Envir.Time > HoldTime)
                {
                    PerformHold();
                    HoldTime = Envir.Time + 16000;
                    return;
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
