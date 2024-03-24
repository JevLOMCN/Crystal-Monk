using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;

namespace Server.MirObjects.Monsters
{
    class SecretQueen : MonsterObject
    {
        public SecretQueen(MonsterInfo info) : base(info)
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

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (HP < MaxHP * 0.5 && Envir.Random.Next(15) == 0)
                CallDragon();
            else if (Envir.Random.Next(1) == 0)
            {
                Point location = Functions.PointMove(CurrentLocation, Direction, 7);
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
                return;
            }
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack();
            else
                Attack1();

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            List<MapObject> targets;
            switch (type)
            {
                case 0:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Slow, Value = damage, TickSpeed = 1000 }, this);
                        }
                    }
                    break;
                case 1:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(5) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 50, PType = PoisonType.Green, Value = damage / 20, TickSpeed = 1000 }, this);
                    }
                    break;
                case 2:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(6) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 50, PType = PoisonType.Paralysis, Value = damage, TickSpeed = 1000 }, this);
                    }
                    break;
            }
        }

        private void CallDragon()
        {
            MonsterObject mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X + 3, CurrentLocation.Y + 3);

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X + 3, CurrentLocation.Y - 3);

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X - 3, CurrentLocation.Y + 3);

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X - 3, CurrentLocation.Y - 3);
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, Target, damage, DefenceType.MACAgility);
            ActionList.Add(action);

            Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.SecretQueen});
        }
    }
}
