using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class HellPirate : MonsterObject
    {
        protected internal HellPirate(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }
            
            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            if (Envir.Random.Next(3) > 0)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
            }
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                MirDirection dir = Functions.PreviousDir(Direction);
                Point target;
                List<MapObject> cellObjects =  null;

                for (int i = 0; i < 8; i++)
                {
                    target = Functions.PointMove(CurrentLocation, dir, 1);
                    dir = Functions.NextDir(dir);
                    if (target == Front) continue;

                    if (!CurrentMap.ValidPoint(target)) continue;

                    cellObjects = CurrentMap.GetCellObjects(target);

                    if (cellObjects == null) continue;

                    for (int o = 0; o < cellObjects.Count; o++)
                    {
                        MapObject ob = cellObjects[o];
                        if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                        if (!ob.IsAttackTarget(this)) continue;

                        ob.Attacked(this, MinDC, DefenceType.Agility);
                        break;
                    }
                }
            }

            ShockTime = 0;
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Target.Attacked(this, damage, DefenceType.ACAgility);
        }   
    }
}
