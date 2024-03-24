using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OrcMutant : MonsterObject
    {
        public bool Ranged;
        protected internal OrcMutant(MonsterInfo info)
            : base(info)
        {
        }

        protected virtual byte AttackRange
        {
            get
            {
                return 2;
            }
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

            if (Envir.Random.Next(10) == 1)
            {

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

                damage = GetAttackPower(MinMC, MaxMC);
                if (damage == 0) return;

                StunAndRepulse(damage);

            }
            else if (Envir.Random.Next(5) == 1 || Ranged)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                damage = (int)(1.5f * GetAttackPower(MinDC, MaxDC));
                if (damage == 0) return;

                AoeDmg(2, damage);

            }
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });

                damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                AoeDmg(1, damage);
            }

            if (Target.Dead)
                FindTarget();

        }

        private void Nuke()
        {
            List<MapObject> targets = FindAllTargets(7, CurrentLocation);
            if (targets.Count == 0) return;

           var damage = GetAttackPower(MinSC, MaxSC);

            for (int i = 0; i < targets.Count; i++)
            {
                Target = targets[i];

                var dist = Functions.MaxDistance(Target.CurrentLocation, CurrentLocation);
                var dmg = damage * (1 + (float)dist / 7);

                if (Target == null || !Target.IsAttackTarget(this) || Target.CurrentMap != CurrentMap || Target.Node == null) continue;

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, Target, (int)dmg, DefenceType.MAC);
                ActionList.Add(action);
            }
        }

        protected override void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (Envir.Random.Next(15) == 1 && HP <= MaxHP / 2)
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 3 });
                Nuke();
                return;
            }

            if (InAttackRange(AttackRange))
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

            if (Envir.Random.Next(7) == 0 && Target.CurrentMap == CurrentMap)
                Teleport(Target.CurrentMap, Target.Back);
            else
                MoveTo(Target.CurrentLocation);
        }

        private void StunAndRepulse(int dmg)
        {
            int col = 5;
            int row = 3;

            List<MapObject> TargetsList = new List<MapObject>();

            Point[] loc = new Point[col]; //0 = left 1 = center 2 = right
            loc[0] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 1);
            loc[1] = Functions.PointMove(CurrentLocation, Direction, 1);
            loc[2] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 1);
            loc[3] = Functions.PointMove(loc[0], Functions.PreviousDir(Direction), 1);
            loc[4] = Functions.PointMove(loc[2], Functions.NextDir(Direction), 1);

            for (int i = 0; i < col; i++)
            {
                Point startPoint = loc[i];
                for (int j = 0; j < row; j++)
                {
                    Point hitPoint = Functions.PointMove(startPoint, Direction, j);

                    if (!CurrentMap.ValidPoint(hitPoint)) continue;

                    var cellObjects = CurrentMap.GetCellObjects(hitPoint);

                    if (cellObjects == null) continue;

                    for (int k = 0; k < cellObjects.Count; k++)
                    {
                        MapObject target = cellObjects[k];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (target.IsAttackTarget(this))
                                {
                                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, target, dmg, DefenceType.MAC);
                                    ActionList.Add(action);

                                    TargetsList.Add(target);                                 
                                }
                                break;
                        }
                    }
                }
            }

            foreach (var t in TargetsList)
            {
                t.Pushed(this, Direction, 2 + Envir.Random.Next(4));
                if (Envir.Random.Next(Settings.PoisonResistWeight) >= t.PoisonResist)
                {
                    t.ApplyPoison(new Poison { PType = PoisonType.Paralysis, Duration = 3 + Envir.Random.Next(4), TickSpeed = 1000 }, this, true);
                }
            }

        }
    

        private void AoeDmg(int dist , int dmg)
        {
            List<MapObject> targets = FindAllTargets(dist, CurrentLocation);
            if (targets.Count == 0) return;

            for (int i = 0; i < targets.Count; i++)
            {
                Target = targets[i];

                if (Target == null || !Target.IsAttackTarget(this) || Target.CurrentMap != CurrentMap || Target.Node == null) continue;

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, Target, dmg, DefenceType.MAC);
                ActionList.Add(action);
            }
        }

     
    }
}
