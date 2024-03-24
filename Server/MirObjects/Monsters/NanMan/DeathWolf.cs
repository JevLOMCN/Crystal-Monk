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
    public class DeathWolf : MonsterObject
    {
        protected internal DeathWolf(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && (Functions.InTheLine(CurrentLocation, Target.CurrentLocation, 4));
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            if (Functions.InRange(CurrentLocation, Target.CurrentLocation, 1))
            {
                int damage = GetAttackPower(MinDC, MaxDC);
                Target.Attacked(this, damage, DefenceType.AC);
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
            }
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });
                LineAttack(4);
            }

            ShockTime = 0;
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + (AttackSpeed);
        }

        private void LineAttack(int distance, bool push = false)
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500;

            for (int i = distance; i >= 1; i--)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (!CurrentMap.ValidPoint(target)) continue;

                var cellObjects = CurrentMap.GetCellObjects(target);
                if (cellObjects == null) continue;

                for (int o = 0; o < cellObjects.Count; o++)
                {
                    MapObject ob = cellObjects[o];
                    if (ob.Race == ObjectType.Monster || ob.Race == ObjectType.Player)
                    {
                        if (!ob.IsAttackTarget(this)) continue;

                        if (push)
                        {
                            ob.Pushed(this, Direction, distance - 1);
                        }

                        ob.Attacked(this, damage, DefenceType.ACAgility);

                    }
                    else continue;

                    break;
                }
            }
        }
    }
}
