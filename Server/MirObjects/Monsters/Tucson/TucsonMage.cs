using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class TucsonMage : MonsterObject
    {
        protected internal TucsonMage(MonsterInfo info)
            : base(info)
        {
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 3);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            int col = 3;
            int row = 3;
            Point[] loc = new Point[col]; //0 = left 1 = center 2 = right
            loc[0] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 1);
            loc[1] = Functions.PointMove(CurrentLocation, Direction, 1);
            loc[2] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 1);

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
                                    target.Attacked(this, damage, DefenceType.MAC);

                                    if (Envir.Random.Next(Settings.PoisonResistWeight) >= target.PoisonResist)
                                    {
                                        if (Envir.Random.Next(25) == 0)
                                        {
                                            target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = 3, TickSpeed = 1000 }, this);
                                        }
                                    }

                                }
                                break;
                        }
                    }
                }
            }
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
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (Envir.Random.Next(5) == 0)
            {
                RangeAttack();
            }
            else
            {
                Attack1();
            }

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
        }
    }
}
