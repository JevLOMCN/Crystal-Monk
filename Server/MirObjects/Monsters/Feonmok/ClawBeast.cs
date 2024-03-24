using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class ClawBeast : MonsterObject
    {
        protected internal ClawBeast(MonsterInfo info)
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

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

           if (Envir.Random.Next(3) == 0)
            {
                RangeAttack();
            }           
            else
            {
                Attack1();
            }
        }

        protected override void CompleteAttack(IList<object> data)
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
                if (cellObjects != null) {
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        MapObject ob = cellObjects[x];
                        if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player)
                            continue;

                        if (ob.IsAttackTarget(this))
                        {
                            ob.Attacked(this, damage, DefenceType.MAC);
                            if (Envir.Random.Next(5) == 0)
                            {
                                ob.ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Paralysis, Value = damage, TickSpeed = 1000 }, this);
                                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ClawBeast});
                            }
                        }
                    }
                }
                dir = Functions.NextDir(dir);
            }
        }
    }
}
