using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class ChieftainArcher : MonsterObject
    {
        private const byte AttackRange = 6;

        protected internal ChieftainArcher(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            switch (Envir.Random.Next(8))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    {
                        Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 0 });

                        int damage = GetAttackPower(MinDC, MinDC);
                        if (damage == 0) return;

                        int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                        DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.ACAgility, 0);
                        ActionList.Add(action);
                    }
                    break;

                case 4:
                case 5:
                case 6:
                case 7:
                    {
                        Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

                        int damage = GetAttackPower(MinDC, MinDC);
                        if (damage == 0) return;

                        int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                        DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.ACAgility, 1);
                        ActionList.Add(action);

                    }
                    break;

                    //{
                      //  Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });
                      //  int damage = GetAttackPower(MinDC, MinDC);
                      //  if (damage == 0) return;

                      //  int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                      //  DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.ACAgility, 2);
                      //  ActionList.Add(action);

                   // }
            }
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.Attacked(this, damage, defence);

            switch (type)
            {
                case 1:
                    if (Envir.Random.Next(10) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Bleeding, Value = GetAttackPower(MinDC, MinDC), TickSpeed = 1000 }, this);
                    }
                    break;

                case 2:
                    LineAttack(4);
                    break;
            }
        }

        private void LineAttack(int distance)
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            for (int i = 1; i <= distance; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (Target != null && target == Target.CurrentLocation)
                    Target.Attacked(this, damage, DefenceType.None);
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

                            ob.Attacked(this, (int)(damage * (0.9F + 0.1 * i)), DefenceType.None);
                        }
                        else continue;

                        break;
                    }
                }
            }
        }
    }
}