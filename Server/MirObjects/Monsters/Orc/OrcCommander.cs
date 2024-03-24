using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class OrcCommander : MonsterObject
    {
        protected internal OrcCommander(MonsterInfo info)
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

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            int damage;

           if (Envir.Random.Next(5) == 1)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                damage = GetAttackPower(MinMC, MaxMC);
                if (damage == 0) return;

                HalfmoonAttack(damage);

            }           
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });

                damage = GetAttackPower(MinDC, MaxDC);
                if (damage == 0) return;

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, Target, damage, DefenceType.AC);
                ActionList.Add(action);
            }

            if (Target.Dead)
                FindTarget();

        }


        private void HalfmoonAttack(int damage)
        {
            MirDirection dir = Functions.PreviousDir(Direction);
            for (int i = 0; i < 3; i++)
            {
                Point location = Functions.PointMove(CurrentLocation, dir, 1);
                if (!CurrentMap.ValidPoint(location))
                    continue;
                var cellObjects = CurrentMap.GetCellObjects(location);
                if (
                    cellObjects != null)
                {
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        if (cellObjects[x].Race == ObjectType.Player ||
                            cellObjects[x].Race == ObjectType.Monster)
                        {
                            if (cellObjects[x].IsAttackTarget(this))
                            {
                                cellObjects[x].Attacked(this, damage, DefenceType.MAC);
                            }
                        }
                    }
                }
                dir = Functions.NextDir(dir);
            }
        }

     
    }
}
