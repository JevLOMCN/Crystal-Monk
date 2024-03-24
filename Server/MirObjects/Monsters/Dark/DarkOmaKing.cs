using Server.MirDatabase;
using System.Collections.Generic;
using System.Drawing;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DarkOmaKing : MonsterObject
    {
        protected virtual byte AttackRange
        {
            get
            {
                return 8;
            }
        }

        public byte _stage { get; private set; } = 7;

        protected internal DarkOmaKing(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
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
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;


            int damage = (int)((Target.MaxHealth * 0.10F));
            if (!ranged)
            {
                if (Envir.Random.Next(10) == 1)
                {
                    ActionTime = Envir.Time + 1000;
                    damage = 1 + (int)((Target.MaxHealth * 0.10F));
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                    AoeAttack(2,MinDC + damage, 300, CurrentLocation);
                    AoeAttack(2, MinDC + damage, 650, CurrentLocation);
                    AoeAttack(2, MinDC + damage, 1050, CurrentLocation);
                    return;
                }

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                damage = 1 + (int)((Target.MaxHealth * 0.15F));

                if (damage == 0) return;
                Target.Attacked(this, MinMC + damage, DefenceType.MACAgility);
            }
            else
            {
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID , Type = 0});
                AttackTime = Envir.Time + AttackSpeed + 500;

                damage = (int)((Target.MaxHealth * 0.10F));
                 if (damage == 0) return;


                AoeAttack(1, MinDC + damage, 300,Target.CurrentLocation);
            }


            if (Target.Dead)
                FindTarget();

        }



        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
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



        protected override void ProcessAI()
        {
            if (Dead) return;

            if (MaxHP >= 7 && Target != null)
            {
                byte stage = (byte)(HP / (MaxHP / 7));

                if (stage < _stage)
                {
                    ActionTime = Envir.Time + 400;
                    int damage = 1 + (int)((Target.MaxHealth * 0.45F));
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

                    AoeAttack(3, MinSC + damage, 300, CurrentLocation);
                }
                _stage = stage;
            }


            base.ProcessAI();
        }
    


}
}
