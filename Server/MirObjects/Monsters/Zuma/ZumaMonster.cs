using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;


namespace Server.MirObjects.Monsters
{
    class ZumaMonster : MonsterObject
    {
        public bool Stoned = true;
        public bool AvoidFireWall = true;

        protected override bool CanMove
        {
            get
            {
                return base.CanMove && !Stoned;
            }
        }
        protected override bool CanAttack
        {
            get
            {
                return base.CanAttack && !Stoned;
            }
        }


        protected internal ZumaMonster(MonsterInfo info) : base(info)
        {
        }

        public override int Pushed(MapObject pusher, MirDirection dir, int distance)
        {
            return Stoned ? 0 : base.Pushed(pusher, dir, distance);
        }

        public override void ApplyPoison(Poison p, MapObject Caster = null, bool NoResist = false, bool ignoreDefence = true)
        {
            if (Stoned) return;

            base.ApplyPoison(p, Caster, NoResist, ignoreDefence);
        }
        public override void AddBuff(Buff b)
        {
            if (Stoned) return;

            base.AddBuff(b);
        }
        
        public override bool IsFriendlyTarget(PlayerObject ally)
        {
            if (Stoned) return false;

            return base.IsFriendlyTarget(ally);
        }

        protected override void ProcessAI()
        {
            if (!Dead && Envir.Time > ActionTime)
            {
                bool stoned = !FindNearby(2);
                
                if (Stoned && !stoned)
                {
                    Wake();
                    WakeAll(14);
                }
            }

            base.ProcessAI();
        }
        public void Wake()
        {
            if (!Stoned) return;

            Stoned = false;
            Broadcast(new S.ObjectShow { ObjectID = ObjectID });
            ActionTime = Envir.Time + 1000;
        }
        public void WakeAll(int dist)
        {

            for (int y = CurrentLocation.Y - dist; y <= CurrentLocation.Y + dist; y++)
            {
                if (y < 0) continue;
                if (y >= CurrentMap.Height) break;

                for (int x = CurrentLocation.X - dist; x <= CurrentLocation.X + dist; x++)
                {
                    if (x < 0) continue;
                    if (x >= CurrentMap.Width) break;

                    var cellObjects = CurrentMap.GetCellObjects(x, y);

                    if (cellObjects == null) continue;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        ZumaMonster target = cellObjects[i] as ZumaMonster;
                        if (target == null || !target.Stoned) continue;
                        target.Wake();
                        target.Target = Target;
                    }
                }
            }

        }
        public override bool IsAttackTarget(MonsterObject attacker)
        {
            return !Stoned && base.IsAttackTarget(attacker);
        }
        public override bool IsAttackTarget(PlayerObject attacker)
        {
            return !Stoned && base.IsAttackTarget(attacker);
        }

        public override bool Walk(MirDirection dir)
        {
            if (!CanMove) return false;

            Point location = Functions.PointMove(CurrentLocation, dir, 1);

            if (!CurrentMap.ValidPoint(location)) return false;

            var cellObjects = CurrentMap.GetCellObjects(location);

            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (AvoidFireWall && ob.Race == ObjectType.Spell)
                        if (((SpellObject)ob).Spell == Spell.FireWall) return false;

                    if (!ob.Blocking) continue;

                    return false;
                }

            CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);

            Direction = dir;
            RemoveObjects(dir, 1);
            CurrentLocation = location;
            CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
            AddObjects(dir, 1);

            if (Hidden)
            {
                Hidden = false;

                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].Type != BuffType.Hiding) continue;

                    Buffs[i].ExpireTime = 0;
                    break;
                }
            }


            CellTime = Envir.Time + 500;
            ActionTime = Envir.Time + 300;
            MoveTime = Envir.Time + MoveSpeed;

            InSafeZone = CurrentMap.GetSafeZone(CurrentLocation) != null;

            Broadcast(new S.ObjectWalk { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            cellObjects = CurrentMap.GetCellObjects(CurrentLocation.X, CurrentLocation.Y);
            if (cellObjects == null) return true;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                if (cellObjects[i].Race != ObjectType.Spell) continue;
                SpellObject ob = (SpellObject)cellObjects[i];

                ob.ProcessSpell(this);
                //break;
            }

            return true;
        }


        public override Packet GetInfo()
        {
            return new S.ObjectMonster
            {
                ObjectID = ObjectID,
                Name = Name,
                NameColour = NameColour,
                Location = CurrentLocation,
                Image = Info.Image,
                Direction = Direction,
                Effect = Info.Effect,
                AI = Info.AI,
                Light = Info.Light,
                Dead = Dead,
                Skeleton = Harvested,
                Poison = CurrentPoison,
                Hidden = Hidden,
                Extra = Stoned,
            };
        }
    }
}
