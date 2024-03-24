using Server.MirDatabase;
using System.Collections.Generic;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CatRestlessJar : MonsterObject
    {
        protected override bool CanMove { get { return false; } }

        protected internal CatRestlessJar(MonsterInfo info)
            : base(info)
        {
            Direction = MirDirection.Up;
        }

        protected override bool InAttackRange()
        {
            return InAttackRange(6);
        }

        public override void Turn(MirDirection dir)
        {
        }

        public override bool Walk(MirDirection dir) 
        { 
            return false; 
        }

        protected override void Attack()
        {
            if (!CanAttack || Target == null) return;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            bool range = !Functions.InRange(Target.CurrentLocation, CurrentLocation, 1);
            if (range)
            {
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

                int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step
                DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action); ;
            }
            else if (Envir.Random.Next(5) == 0)
            {
                ShockTime = 0;
                List<MapObject> targets = FindAllTargets(7, CurrentLocation, false);
                if (targets.Count == 0) return;

                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, TargetID = Target.ObjectID, Location = CurrentLocation });
                if (targets.Count != 0)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, targets[i], damage, DefenceType.MACAgility);
                        ActionList.Add(action); ;
                    }
                }
            }
            else
            {
                List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
                if (targets.Count == 0) return;

                ShockTime = 0;

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Attacked(this, GetAttackPower(MinDC, MaxDC), DefenceType.ACAgility);

                    if (Envir.Random.Next(6) == 0)
                    {
                        targets[i].Pushed(this, Functions.DirectionFromPoint(CurrentLocation, targets[i].CurrentLocation), Envir.Random.Next(3) + 1);
                    }
                }
            }

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
        }   

        public override void ApplyPoison(Poison p, MapObject Caster = null, bool NoResist = false, bool ignoreDefence = true) { }
    }
}
