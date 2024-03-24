using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OmaWitchDoctor : MonsterObject
    {

        public static Point[] DamagePoint = { new Point(0, -6), new Point(5, -5), new Point(8, 0), new Point(5, 5), new Point(0, 8), new Point(-5, 5), new Point(-8, 0), new Point(-6, -4) };
        public byte AttackRange = 6;

        protected internal OmaWitchDoctor(MonsterInfo info)
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

            if (!CanAttack)
                return;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            if (!ranged)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
                int damage = GetAttackPower(MinMC, MaxMC);
                if (damage == 0) return;
                Target.Attacked(this, damage, DefenceType.MACAgility);
            }
            else
            {
                if (!InDamagePointsRange(Target.CurrentLocation))
                {
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

                    MirDirection dir = Functions.PreviousDir(Direction);
                    Point target;
                    List<MapObject>cellObjects =  null;

                    for (int i = 0; i < 4; i++)
                    {
                        target = Functions.PointMove(CurrentLocation, Direction, 1);
                        dir = Functions.NextDir(Direction);
                        if (target == Front) continue;

                        if (!CurrentMap.ValidPoint(target)) continue;

                        cellObjects = CurrentMap.GetCellObjects(target);

                        if (cellObjects == null) continue;

                        for (int o = 0; o < cellObjects.Count; o++)
                        {
                            MapObject ob = cellObjects[o];
                            if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                            if (!ob.IsAttackTarget(this)) continue;

                            ob.Attacked(this, MinDC, DefenceType.MACAgility);

                            if (Envir.Random.Next(5) == 0)
                            {
                                ob.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = Envir.Random.Next(1, 4), TickSpeed = 1000 }, this);
                            }

                            break;
                        }
                    }
                    int damage = GetAttackPower(MinMC, MaxMC);
                    if (damage == 0) return;

                    DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1000, Target, damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
                else
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });
                    int damage = GetAttackPower(MinMC, MaxMC);
                    if (damage == 0) return;
                    DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1000, Target, damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
            }

            ShockTime = 0;
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
        }

        private bool InDamagePointsRange(Point pt)
        {
            for (int i=0; i<DamagePoint.Length; ++i)
            {
                if (Functions.InRange(new Point(CurrentLocation.X + DamagePoint[i].X, CurrentLocation.Y + DamagePoint[i].Y), pt, 2))
                    return true;
            }

            return false;
        }
    }
}
