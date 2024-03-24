using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;

namespace Server.MirObjects.Monsters
{
    public class Taganda : MonsterObject
    {
        public long SwingAttackTime;
        public byte SwingRange = 3;
        public long Enragetime;
        public long ExplosionTime;
        public byte ExplosionRange = 8;
        public int EnrageStages
        {
            get {
                return
                    PercentHealth >= 90 ? 0 :
                    PercentHealth >= 80 && PercentHealth < 90 ? 1 :
                    PercentHealth >= 70 && PercentHealth < 80 ? 2 :
                    PercentHealth >= 60 && PercentHealth < 70 ? 3 :
                    PercentHealth >= 50 && PercentHealth < 60 ? 4 :
                    PercentHealth >= 40 && PercentHealth < 50 ? 5 :
                    PercentHealth >= 30 && PercentHealth < 40 ? 6 :
                    PercentHealth >= 20 && PercentHealth < 30 ? 7 :
                    PercentHealth >= 10 && PercentHealth < 20 ? 8 :
                    PercentHealth >= 5 && PercentHealth < 10 ? 9 : 10;
                    }
        }

        public Taganda(MonsterInfo info) : base(info)
        {

        }

        public void DoExplosion()
        {
            if (!Functions.InRange(CurrentLocation, Target.CurrentLocation, ExplosionRange))
                return;
            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
            /*
            DelayedAction action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 1);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 2);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 1300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 3);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 1800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 4);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 2300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 5);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 2800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 6);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 3300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 7);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 3800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 8);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 4300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 9);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 4800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 10);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 5300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 11);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 5800, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 12);
            CurrentMap.ActionList.Add(action);
            action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 6300, this, Spell.MobFireWall, GetAttackPower(MinMC, MaxMC), CurrentLocation, 1, 13);
            CurrentMap.ActionList.Add(action);
            Broadcast(new S.ObjectAttack { Type = 2, Direction = Direction, Location = CurrentLocation, ObjectID = ObjectID });
            */
        }

        public void DoSwingAttack()
        {
            if (!Functions.InRange(CurrentLocation, Target.CurrentLocation, SwingRange))
                return;
            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            Point[] locs = new Point[10];
            locs[0] = Functions.PointMove(CurrentLocation, Direction, 1);
            locs[1] = Functions.PointMove(CurrentLocation, Direction, 2);
            locs[2] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 1);
            locs[3] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 2);
            locs[4] = Functions.PointMove(locs[2], Direction, 1);
            locs[5] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 1);
            locs[6] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 2);
            locs[7] = Functions.PointMove(locs[5], Direction, 1);
            MirDirection dir = Functions.NextDir(Direction);
            dir = Functions.NextDir(dir);
            locs[8] = Functions.PointMove(locs[0], dir, 2);
            locs[9] = Functions.PointMove(locs[0], dir, 3);
            for (int i = 0; i < locs.Length; i++)
            {
                if (!CurrentMap.ValidPoint(locs[i]))
                    continue;
                var cellObjects = CurrentMap.GetCellObjects(locs[i]);
                if (
                    cellObjects != null &&
                    cellObjects.Count > 0)
                {
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        switch(cellObjects[x].Race)
                        {
                            case ObjectType.Player:
                            case ObjectType.Monster:
                                if (!cellObjects[x].IsAttackTarget(this))
                                    continue;
                                int delay = Functions.MaxDistance(CurrentLocation, cellObjects[x].CurrentLocation) * 50 + 500; //50 MS per Step
                                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, cellObjects[x], GetAttackPower(MinDC, MaxDC), DefenceType.Agility);
                                ActionList.Add(action);
                                break;
                        }
                    }
                }
            }
            Broadcast(new S.ObjectRangeAttack { Type = 0, Direction = Direction, Location = CurrentLocation, ObjectID = ObjectID, TargetID = Target.ObjectID });
        }

        public void DoEnrage()
        {
            Enragetime = Envir.Time + (Envir.Random.Next(15, 30) * Settings.Second);
        }

        public override void RefreshAll()
        {
            base.RefreshAll();
            if (Envir.Time < Enragetime)
            {
                AttackSpeed = Math.Max(400, AttackSpeed - 300);
                MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 300);
                MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 15);
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 15);
            }
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;
            if (CanAttack && Envir.Time > AttackTime)
            {
                
                if (Envir.Time >= Enragetime + 10000 &&
                Envir.Random.Next(0, 25 - EnrageStages) == 0)
                {
                    DoEnrage();
                    DoExplosion();
                    return;
                }
                
                if (Envir.Time >= SwingAttackTime + 5000 &&
                    Envir.Random.Next(0, 15 - EnrageStages) == 0)
                {
                    DoSwingAttack();
                    return;
                }
                
                if (Functions.InRange(CurrentLocation, Target.CurrentLocation, 1))
                {
                    Attack();
                    return;
                }
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
