using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OrcGeneral : MonsterObject
    {
        protected internal OrcGeneral(MonsterInfo info)
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

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            int damage;

           if (Envir.Random.Next(5) == 1)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

                LineAttack(3);

            }           
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });

                damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, Target, damage, DefenceType.AC);
                ActionList.Add(action);
            }

            if (Target.Dead)
                FindTarget();

        }


        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (Envir.Random.Next(10) == 1 && InAttackRange(5))
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });
                Nuke();
                return;
            }

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

            if ( Envir.Random.Next(7) == 0 && Target.CurrentMap == CurrentMap)
                Teleport(Target.CurrentMap, Target.Back);
            else
                MoveTo(Target.CurrentLocation);
        }

        private void Nuke()
        {
            List<MapObject> targets = FindAllTargets(5, CurrentLocation);
            if (targets.Count == 0) return;

            var damage = GetAttackPower(MinMC, MaxMC);

            for (int i = 0; i < targets.Count; i++)
            {
                Target = targets[i];

                var dist = Functions.MaxDistance(Target.CurrentLocation, CurrentLocation);

                if (Target == null || !Target.IsAttackTarget(this) || Target.CurrentMap != CurrentMap || Target.Node == null) continue;

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, Target, damage, DefenceType.MAC);
                ActionList.Add(action);
            }
        }


        private void LineAttack(int distance)
        {

            int damage = (int)(1.25f * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            for (int i = 1; i <= distance; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (target == Target.CurrentLocation)
                {
                    Target.Attacked(this, damage, DefenceType.ACAgility);
                }
                else
                {
                    if (!CurrentMap.ValidPoint(target)) continue;

                    var cellObjects = CurrentMap.GetCellObjects(target);
                    if (cellObjects == null) continue;

                    for (int o = 0; o < cellObjects.Count; o++)
                    {
                        MapObject ob = cellObjects[o];
                        if (ob.Race == ObjectType.Monster || ob.Race == ObjectType.Player)
                        {
                            if (!ob.IsAttackTarget(this)) continue;

                            ob.Attacked(this, damage, DefenceType.ACAgility);
                        }
                        else continue;

                        break;
                    }

                }
            }
        }


    }
}
