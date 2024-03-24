using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLin6 : MonsterObject
    {
        private long SpellTime;
        private long SpellTime1;

        protected internal DogYoLin6(MonsterInfo info)
            : base(info)
        {
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.DogYoLin6,
                Value = damage,
                ExpireTime = Envir.Time + 20000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = new Point(x, y),
                CurrentMap = CurrentMap,
                StartTime = Envir.Time,
            };

            CurrentMap.AddObject(ob);
            ob.Spawned();
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            if (Envir.Time > SpellTime && Envir.Random.Next(5) == 0)
            {
                SpellTime = Envir.Time + 20000;
                RangeAttack4();
            }
            else if (Envir.Time > SpellTime1 && Envir.Random.Next(5) == 0)
            {
                SpellTime1 = Envir.Time + 15000;
                Target.AddBuff(new Buff { Type = BuffType.DogYoLin6, Caster = this, ExpireTime = Envir.Time + 5000, Visible = true, TickSpeed = 1000 });
                RangeAttack3();
            }
            else if (Envir.Random.Next(3) == 0)
            {
                Point location = Functions.PointMove(CurrentLocation, Direction, 4);
                if (!CurrentMap.ValidPoint(location)) return;

                var cellObjects = CurrentMap.GetCellObjects(location);
                bool blocked = false;
                if (cellObjects != null)
                {
                    for (int c = 0; c < cellObjects.Count; c++)
                    {
                        MapObject ob = cellObjects[c];
                        if (!ob.Blocking) continue;
                        blocked = true;
                        if ((cellObjects == null) || blocked) break;
                    }
                }

                // blocked telpo cancel
                if (blocked) return;

                CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
                RemoveObjects(Direction, 1);
                CurrentLocation = location;
                Broadcast(new S.ObjectDash { ObjectID = ObjectID, Direction = Direction, Location = location });

                CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
                AddObjects(Direction, 1);

                ActionTime = Envir.Time + 3000;
                AttackTime = Envir.Time + AttackSpeed;
            }
            else if (Envir.Random.Next(2) == 0)
            {
                RangeAttack();
            }
            else
                Attack1();
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            switch (type)
            {
                case 0:
                    LineAttack(4, damage, DefenceType.MAC, 0);
                    break;

                case 1:
                    RegenMonsterByName1(Settings.DogYoLin6, target.CurrentLocation.X, target.CurrentLocation.Y);
                    break;

                case 2:
                    break;

                case 3:
                    SpwanShiled(damage, target.CurrentLocation.X, target.CurrentLocation.Y);
                    break;
            }
        }
    }
}
