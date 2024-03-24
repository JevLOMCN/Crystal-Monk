using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class BlackDragon_Mob : MonsterObject
    {
        /// <summary>
        /// Check if the target is in cone range
        /// </summary>
        public bool InConeRange
        {
            get { return Functions.InRange(CurrentLocation, Target.CurrentLocation, 4); }
        }
        /// <summary>
        /// Check if the target is in slash range
        /// </summary>
        public bool InSlashRange
        {
            get { return Functions.InRange(CurrentLocation, Target.CurrentLocation, 1); }
        }

        /// <summary>
        /// Get the current Stage of the monster.
        /// </summary>
        public int Stage
        {
            get
            {
                return
                PercentHealth >= 75 ? 0 :
                PercentHealth >= 50 && PercentHealth < 75 ? 1 :
                PercentHealth >= 25 && PercentHealth < 50 ? 2 : 3;
            }
        }

        public long ConeAttackTime;
        public long SlashAttackTime;

        /// <summary>
        /// Get the points in the shape of a cone
        /// </summary>
        /// <returns></returns>
        public List<Point> GetConePoints()
        {
            List<Point> targetPoints = new List<Point>
            {
                Functions.PointMove(CurrentLocation, Direction, 1),//   0
                Functions.PointMove(CurrentLocation, Direction, 2),//   1
                Functions.PointMove(CurrentLocation, Direction, 3),//   2
                Functions.PointMove(CurrentLocation, Direction, 4)//    3
            };
            MirDirection dir = Direction;
            dir = Functions.PreviousDir(dir);
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 1));//4
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 2));//5
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 3));//6
            targetPoints.Add(Functions.PointMove(targetPoints[4], Direction, 1));//7
            targetPoints.Add(Functions.PointMove(targetPoints[4], Direction, 2));//8
            targetPoints.Add(Functions.PointMove(targetPoints[4], Direction, 3));//9
            dir = Direction;
            dir = Functions.NextDir(dir);
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 1));//10
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 2));//11
            targetPoints.Add(Functions.PointMove(CurrentLocation, dir, 3));//12
            targetPoints.Add(Functions.PointMove(targetPoints[10], Direction, 1));//13
            targetPoints.Add(Functions.PointMove(targetPoints[10], Direction, 2));//14
            targetPoints.Add(Functions.PointMove(targetPoints[10], Direction, 3));//15
            return targetPoints;
        }

        public bool SpawnSlaves()
        {
            int spawnCount = 0;
            int count = Math.Min(8, 40 - SlaveList.Count);
            for (int i = 0; i < count; i++)
            {
                MonsterObject mob = null;
                switch (Envir.Random.Next(4))
                {
                    case 0:
                        mob = GetMonster(Envir.GetMonsterInfo(Settings.LordMonster1));
                        break;
                    case 1:
                        mob = GetMonster(Envir.GetMonsterInfo(Settings.LordMonster2));
                        break;
                    case 2:
                        mob = GetMonster(Envir.GetMonsterInfo(Settings.LordMonster3));
                        break;
                    case 3:
                        mob = GetMonster(Envir.GetMonsterInfo(Settings.LordMonster4));
                        break;
                }

                if (mob == null) continue;

                if (!mob.Spawn(CurrentMap, Front))
                    mob.Spawn(CurrentMap, CurrentLocation);

                mob.Target = Target;
                mob.ActionTime = Envir.Time + 2000;
                SlaveList.Add(mob);
                spawnCount++;
            }
            return spawnCount > 0;
        }

        public BlackDragon_Mob(MonsterInfo info) : base(info)
        {

        }

        public void BDAttack()
        {
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
            #region Slash Attack
            if (InSlashRange && Envir.Time > SlashAttackTime && Envir.Random.Next(5) == 0)
            {
                bool attacked = false;
                if (Target != null)
                {
                    MapObject _origTarget = Target;
                    //  Attempt to find a weaker target
                    // TODO
                 //   Target = FindWeakTarget(CurrentLocation, 3);
                    if (Target == null)
                        Target = _origTarget;
                    Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                    if (Target.Attacked(this, GetAttackPower(MinSC, MaxSC), DefenceType.None) > 0)
                    {
                        attacked = true;
                        if (Envir.Random.Next(5) == 0)
                            Target.ApplyPoison(new Poison { PType = PoisonType.Bleeding, Duration = Envir.Random.Next(3, 8), Owner = this, TickSpeed = 1000, Value = Envir.Random.Next(25, 50) });
                    }
                }
                if (attacked)
                {
                    SlashAttackTime = Envir.Time + 2500;
                    Broadcast(new S.ObjectAttack { Type = 0, Direction = Direction, Location = CurrentLocation, ObjectID = ObjectID });
                    //Broadcast(new S.Chat { Message = string.Format("Slash"), Type = ChatType.System });
                    return;
                }
            }
            #endregion
            #region Cone Attack
            else if (InConeRange && Envir.Time > ConeAttackTime)
            {
                List<Point> conePoints = GetConePoints();
                bool attacked = false;
                for (int i = 0; i < conePoints.Count; i++)
                {
                    if (!CurrentMap.ValidPoint(conePoints[i]))
                        continue;
                    var cellObjects = CurrentMap.GetCellObjects(conePoints[i]);
                    if (cellObjects == null || cellObjects.Count <= 0)
                        continue;
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        MapObject ob = cellObjects[x];
                        switch (ob.Race)
                        {
                            case ObjectType.Player:
                            case ObjectType.Monster:
                                {
                                    if (ob.IsAttackTarget(this))
                                    {
                                        if (ob.Attacked(this, GetAttackPower(MinMC, MaxMC), DefenceType.Agility) > 0)
                                        {
                                            attacked = true;
                                            if (Envir.Random.Next(5) == 0)
                                                Target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = Envir.Random.Next(3, 8), Owner = this, TickSpeed = 1000, Value = GetAttackPower(MinMC, MaxMC) });
                                            else if (Envir.Random.Next(10) == 0)
                                                Target.ApplyPoison(new Poison { PType = PoisonType.Frozen, Duration = Envir.Random.Next(3, 8), Owner = this, TickSpeed = 1000, Value = GetAttackPower(MinMC, MaxMC) });
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                if (attacked)
                {
                    //  Only set the cone attack time if it succesfully hit a target
                    ConeAttackTime = Envir.Time + 5000;
                    Broadcast(new S.ObjectAttack { Type = 1, Direction = Direction, Location = CurrentLocation, ObjectID = ObjectID, Spell = Spell.None });
                    //Broadcast(new S.Chat { Message = string.Format("Cone"), Type = ChatType.System });
                    return;
                }

            }
            #endregion
            else if (Stage == 1 || Stage == 3)
            {
                if (Envir.Random.Next(5) == 0)
                {
                    if (!SpawnSlaves())
                        Attack();
                }
                else
                    Attack();

                return;
            }
            else
                Attack();
            return;
        }

        protected override void Attack()
        {
            if (BindingShotCenter) ReleaseBindingShot();
            //Broadcast(new S.Chat { Message = string.Format("Attack"), Type = ChatType.System });
            ShockTime = 0;

            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }


            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

            int damage = GetAttackPower(MinDC, MaxDC);

            if (damage == 0) return;

            Target.Attacked(this, damage);
        }

        protected override void ProcessTarget()
        {
            // TODO
          //  if (IsRetreating)
            //    return;
            if (Target == null || !CanAttack) return;

            if (InAttackRange() || (Envir.Time > ConeAttackTime && InConeRange && Envir.Random.Next(5) == 0))
            {
                BDAttack();
                if (Target.Dead)
                    FindTarget();
                return;
            }
            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }
            if (Target != null &&
                !Target.Dead)
                MoveTo(Target.CurrentLocation);
            else
                FindTarget();
        }
    }
}
