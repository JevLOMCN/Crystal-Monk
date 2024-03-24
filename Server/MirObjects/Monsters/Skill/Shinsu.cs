﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    class Shinsu : MonsterObject
    {
        public bool Mode = false;
        public bool Summoned;
        public long ModeTime;

        protected override bool CanAttack
        {
            get
            {
                return base.CanAttack && Mode;
            }
        }

        protected internal Shinsu(MonsterInfo info) : base(info)
        {
            ActionTime = Envir.Time + 1000;
        }

        protected override void ProcessAI()
        {
            if (!Dead && Envir.Time > ActionTime)
            {
                if (Target != null) ModeTime = Envir.Time + 30000;

                if (!Mode && Envir.Time < ModeTime)
                {
                    Mode = true;
                    Broadcast(new S.ObjectShow { ObjectID = ObjectID });
                    ActionTime = Envir.Time + 1000;
                }
                else if (Mode && Envir.Time > ModeTime)
                {
                    Mode = false;
                    Broadcast(new S.ObjectHide { ObjectID = ObjectID });
                    ActionTime = Envir.Time + 1000;
                }
            }

            base.ProcessAI();
        }

        protected override bool InAttackRange()
        {
            if (Target.CurrentMap != CurrentMap) return false;
            if (Target.CurrentLocation == CurrentLocation) return false;

            int x = Math.Abs(Target.CurrentLocation.X - CurrentLocation.X);
            int y = Math.Abs(Target.CurrentLocation.Y - CurrentLocation.Y);

            if (x > 2 || y > 2) return false;

            return (x <= 1 && y <= 1) || (x == y || x % 2 == y % 2);
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this) || !CanAttack)
            {
                Target = null;
                return;
            }

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            ActionList.Add(new DelayedAction(DelayedType.Damage, Envir.Time + 500));

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
            ShockTime = 0;

            if (Target.Dead)
                FindTarget();
        }

        private void LineAttack(int distance)
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            DefenceType dType = DefenceType.None;
            byte RegionID = 1;
            if (Master != null && Master is PlayerObject)
            {
                PlayerObject p = (PlayerObject)Master;
                RegionID = p.GetRegionID();
            }

            if (Envir.IsNewVersion(RegionID))
            {
                dType = DefenceType.MAC;
            }

            for (int i = 1; i <= distance; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (Target != null && target == Target.CurrentLocation)
                    Target.Attacked(this, damage, dType);
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

                            ob.Attacked(this, damage, dType);
                        }
                        else continue;

                        break;
                    }
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();

            Summoned = true;
        }

        protected override void CompleteAttack(IList<object> data)
        {
            LineAttack(2);
        }

        public override Packet GetInfo()
        {
            return new S.ObjectMonster
                {
                    ObjectID = ObjectID,
                    Name = Name,
                    NameColour = NameColour,
                    Location = CurrentLocation,
                    Image = Mode ? Monster.Shinsu1 : Monster.Shinsu,
                    Direction = Direction,
                    Effect = Info.Effect,
                    AI = Info.AI,
                    Light = Info.Light,
                    Dead = Dead,
                    Skeleton = Harvested,
                    Poison = CurrentPoison,
                    Hidden = Hidden,
                    Extra = Summoned,
                };
        }
    }
}
