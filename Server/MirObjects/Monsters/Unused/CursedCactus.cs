using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.MirDatabase;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CursedCactus :MonsterObject
    {
        public byte AttackRange = 8;
        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }

        public CursedCactus(MonsterInfo info) : base(info)
        {

        }

        protected override bool CanMove { get { return false; } }

        public override void Turn(MirDirection dir)
        {
            
        }

        public override bool Walk(MirDirection dir)
        {
            return false;
        }

        protected override void ProcessRoam()
        {
            
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
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (Envir.Random.Next(Settings.MagicResistWeight) >= Target.MagicResist)
            {
                int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);
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
    }
}
