using Server.MirDatabase;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OrcWizard : MonsterObject
    {
        public long FearTime;
        public byte AttackRange = 6;

        protected internal OrcWizard(MonsterInfo info)
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
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });


            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);

            if (Target.Dead)
                FindTarget();

        }

        protected void Heal()
        {

            var targets = FindAllNearby(8, CurrentLocation);
            targets.RemoveAll(x => !x.IsFriendlyTarget(this) || x.Race != ObjectType.Monster || x.Health == x.MaxHealth || x.Dead);

            ShockTime = 0;

            if (targets.Count == 0) return;

            var T = (MonsterObject)targets[Envir.Random.Next(targets.Count)];

            Direction = Functions.DirectionFromPoint(CurrentLocation, T.CurrentLocation);
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = T.ObjectID ,Type = 1});


            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            int heal = GetAttackPower(MinSC, MaxSC);
            if (heal == 0) return;

            T.ChangeHP(heal);

        }

        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;
          
            if (InAttackRange() && Envir.Time < FearTime)
            {
                if (Envir.Random.Next(100) < 20)
                    Heal();
                else
                    Attack();
                return;
            }

            FearTime = Envir.Time + 5000;

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            int dist = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation);

            if (dist >= AttackRange)
                MoveTo(Target.CurrentLocation);
            else
            {
                MirDirection dir = Functions.DirectionFromPoint(Target.CurrentLocation, CurrentLocation);

                if (Walk(dir)) return;

                switch (Envir.Random.Next(2)) //No favour
                {
                    case 0:
                        for (int i = 0; i < 7; i++)
                        {
                            dir = Functions.NextDir(dir);

                            if (Walk(dir))
                                return;
                        }
                        break;
                    default:
                        for (int i = 0; i < 7; i++)
                        {
                            dir = Functions.PreviousDir(dir);

                            if (Walk(dir))
                                return;
                        }
                        break;
                }
                
            }
        }
    }
}
