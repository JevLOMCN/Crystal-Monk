using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Threading.Tasks;


namespace Server.MirObjects.Monsters
{
    public class Lord : MonsterObject
    {
        // Attack Range
        public byte AttackRange = 15;
        public long nukeTime;
        public bool nukeTrigger = false;

        public int Stage
        {
            get
            {
                return
                  PercentHealth >= 75 ? 1 :
                  PercentHealth >= 50 && PercentHealth < 75 ? 2 :
                  PercentHealth >= 25 && PercentHealth < 50 ? 3 : 4;
            }
        }
        protected internal Lord(MonsterInfo info)
            : base(info)
        {
        }
        //Checks if target is in Attack Range
        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }
        public override void RefreshAll()
        {
            base.RefreshAll();
            //  50 ~ 74
            if (Stage == 2)
            {
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 900);
                MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 900);
            }
            //  0 ~ 25
            if (Stage == 4)
            {
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 900);
                MinAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 900);
            }


        }


        //Main Attack method
        protected override void Attack()
        {
            if (Target == null ||
                Target.Dead)
            {
                // TODO
          //      FindWeakTarget(CurrentLocation, 12);
                return;
            }

            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }
            RefreshAll();
            StageAttack();
            
        }

        public void Attack0(int damage)
        {
            if (Target == null ||
                Target.Dead)
                return;
            Target.Attacked(this, damage, DefenceType.MAC);
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Type = 0, Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation), Target = Target.CurrentLocation, TargetID = Target.ObjectID, Location = CurrentLocation });
        }


        protected void StageAttack()
        {// Much easier :P
            if (Target == null ||
                Target.Dead)
                return;
            switch (Stage)
            {
                case 1:
                    {
                        bool ranged = Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
                        Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);


                        if (ranged)
                        {
                            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
                            int damage = GetAttackPower(MinSC, MaxSC);


                            List<MapObject> Attacktargets = FindAllTargets(4, CurrentLocation, false);
                            for (int i = 0; i < Attacktargets.Count; i++)
                            {
                                Attacktargets[i].Attacked(this, damage, DefenceType.AC);
                                if (Envir.Random.Next(20) > 18)
                                {
                                    Attacktargets[i].ApplyPoison(new Poison
                                    {
                                        Owner = this,
                                        Duration = 6,
                                        Value = damage,
                                        PType = PoisonType.Frozen,
                                        TickSpeed = 1000,
                                    }, this);
                                }
                            }
                            
                        }
                        else
                        {
                            if (Envir.Random.Next(10) > 7)
                            {
                                SpawnSlaves();
                            }
                            int damage = GetAttackPower(MinDC, MaxDC);
                            Attack0(damage);
                        }
                    }
                    break;
                case 2:
                    {
                        bool ranged = Functions.InRange(CurrentLocation, Target.CurrentLocation, 3);
                        Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);


                        if (ranged)
                        {
                            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
                            int damage = GetAttackPower(MinMC, MaxMC);


                            List<MapObject> Attacktargets = FindAllTargets(4, CurrentLocation, false);
                            for (int i = 0; i < Attacktargets.Count; i++)
                            {
                                Attacktargets[i].Attacked(this, damage, DefenceType.AC);
                                if (Envir.Random.Next(20) > 18)
                                {
                                    Attacktargets[i].ApplyPoison(new Poison
                                    {
                                        Owner = this,
                                        Duration = 6,
                                        Value = damage,
                                        PType = PoisonType.Bleeding,
                                        TickSpeed = 1000,
                                    }, this);
                                }
                            }
                        }

                        else
                        {
                            if (Envir.Random.Next(10) > 7)
                            {
                                SpawnSlaves();
                            }
                            int damage = GetAttackPower(MinDC, MaxDC);
                            Attack0(damage);
                        }
                    }
                    break;
                case 3:
                    {
                        bool ranged = Functions.InRange(CurrentLocation, Target.CurrentLocation, 3);
                        Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);


                        if (ranged)
                        {
                            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
                            int damage = GetAttackPower(MinSC, MaxSC);


                            List<MapObject> Attacktargets = FindAllTargets(4, CurrentLocation, false);
                            for (int i = 0; i < Attacktargets.Count; i++)
                            {
                                Attacktargets[i].Attacked(this, damage, DefenceType.AC);
                                if (Envir.Random.Next(20) > 18)
                                {
                                    // TODO
                                    /*
                                    Attacktargets[i].ApplyPoison(new Poison
                                    {
                                        Owner = this,
                                        Duration = 6,
                                        Value = damage,
                                        PType = PoisonType.Burning,
                                        TickSpeed = 1000,
                                    }, this);
                                    */
                                }
                            }
                        }

                        else
                        {
                            if (Envir.Random.Next(10) > 7)
                            {
                                SpawnSlaves();
                            }
                            //  Change its target, then attack
                            int damage = GetAttackPower(MinDC, MaxDC);
                            Attack0(damage);
                        }
                    }
                    break;
                case 4:
                    {
                        bool ranged = Functions.InRange(CurrentLocation, Target.CurrentLocation, 3);
                        Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);


                        if (ranged)
                        {
                            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
                            int damage = GetAttackPower(MinMC, MaxMC);


                            List<MapObject> Attacktargets = FindAllTargets(3, CurrentLocation, false);
                            for (int i = 0; i < Attacktargets.Count; i++)
                            {
                                Attacktargets[i].Attacked(this, damage, DefenceType.AC);
                                if (Envir.Random.Next(20) > 18)
                                {
                                    Attacktargets[i].ApplyPoison(new Poison
                                    {
                                        Owner = this,
                                        Duration = 6,
                                        Value = damage,
                                        PType = PoisonType.Paralysis,
                                        TickSpeed = 1000,
                                    }, this);
                                }
                            }
                        }

                        else
                        {
                            if (Envir.Random.Next(10) > 7)
                            {
                                SpawnSlaves();
                            }
                            //MoveTo(Target.CurrentLocation);
                            //  Change its target, then attack
                            int damage = GetAttackPower(MinDC, MaxDC);
                            Attack0(damage);
                        }
                    }
                    break;
            }

            // TODO
           // Target = FindWeakTarget(CurrentLocation, 12, MirClass.NONE, false);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
        }


        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack &&
                Envir.Time > AttackTime)
            {
                Attack();
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

        private void SpawnSlaves()
        {
            int count = Math.Min(8, 40 - SlaveList.Count);

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

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
            }
        }
    }
}