﻿using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class TurtleGrass : MonsterObject
    {
        protected internal TurtleGrass(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);

            if (Envir.Random.Next(4) == 1)
            {
                if (Envir.Random.Next(4) == 1)
                    Target.Pushed(this, Direction, 1 + Envir.Random.Next(2));
                Target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = 3, TickSpeed = 1000 }, this);
            }
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

           if (Envir.Random.Next(5) == 0)
            {
                RangeAttack();
            }           
            else
            {
                Attack1();
            }
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            MirDirection dir = Functions.PreviousDir(Direction);
            for (int i = 0; i < 4; i++)
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