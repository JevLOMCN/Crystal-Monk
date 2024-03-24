using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    class PlainMob : MonsterObject
    {
        public PlainMob(MonsterInfo info) : base(info)
        {

        }

        protected override bool InAttackRange()
        {
            if (Target == null) return false;
            return Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) <= 14;
        }

        public void RangeAttack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            Target.Attacked(this, damage, DefenceType.ACAgility);
            //  Range attacks send the objects ID so the client can use it within it's effects
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Target = Target.CurrentLocation, TargetID = Target.ObjectID, Type = 0, Location = CurrentLocation, Direction = Direction });
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            Target.Attacked(this, damage, DefenceType.ACAgility);
            //  Range attacks send the objects ID so the client can use it within it's effects
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Target = Target.CurrentLocation, TargetID = Target.ObjectID, Type = 0, Location = CurrentLocation, Direction = Direction });
        }

        public void Attack2()
        {

        }
        public void Attack3()
        {

        }
        public void Attack4()
        {

        }
        public void Attack5()
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


            int randy = Envir.Random.Next(5);
            if (randy == 0)
                RangeAttack2(); //  Since it's a range attack, we should move it from Attack1
            //else if (randy == 1)
                //RangeAttack2();
            //else if (randy == 2)
                //Attack2();
            //else if (randy == 3)
                //Attack3();
            //else if (randy == 4)
                //Attack4();
            //else if (randy == 5)
                //Attack5();

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
            ShockTime = 0;
            return;
            /*
            Point target = Functions.PointMove(CurrentLocation, Direction, 2);

            bool range = false;

            if (CurrentMap.ValidPoint(target))
            {
                var cellObjects = CurrentMap.GetCellObjects(target);
                if (cellObjects != null)
                    for (int o = 0; o < cellObjects.Count; o++)
                    {
                        MapObject ob = cellObjects[o];
                        if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player) continue;
                        if (!ob.IsAttackTarget(this)) continue;
                        range = true;
                        break;
                    }
            }


            if (range || Envir.Random.Next(5) == 0)
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            else
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            LineAttack(2, range);


            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
            ShockTime = 0;*/
        }

        private void LineAttack(int distance, bool range)
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            for (int i = 1; i <= distance; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (target == Target.CurrentLocation)
                    Target.Attacked(this, damage, range ? DefenceType.MACAgility : DefenceType.ACAgility);
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
                            ob.Attacked(this, damage, range ? DefenceType.MACAgility : DefenceType.ACAgility);
                        }
                        else continue;

                        break;
                    }
                }
            }
        }
    }
}
