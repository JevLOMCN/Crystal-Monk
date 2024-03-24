using Server.MirDatabase;
using Server.MirEnvir;
using System.Collections.Generic;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public  class DarkWraith : MonsterObject
    {
        public DarkWraith(MonsterInfo info)
            : base(info)
        {
          
        }

        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (InAttackRange())
            {
                Attack();
                if (Target.Dead)
                    FindTarget();

                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            if (Envir.Random.Next(2) == 0)
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
                MoveForward(4, 500);
                ActionTime = Envir.Time + 600;
                AttackTime = Envir.Time + AttackSpeed;
            }
            else
            {
                MoveTo(Target.CurrentLocation);
            }
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
                RangeAttack2();
                MoveForward(4, 600);
            }
            else if (Envir.Random.Next(3) == 0)
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
