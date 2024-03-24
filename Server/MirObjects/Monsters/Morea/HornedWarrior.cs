using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HornedWarrior : MonsterObject
    {
        protected internal HornedWarrior(MonsterInfo info)
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

           if (Envir.Random.Next(3) == 0)
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
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            switch (type)
            {
                case 0:
                    if (Envir.Random.Next(4) == 1)
                    {
                        if (Envir.Random.Next(4) == 1)
                            target.Pushed(this, Direction, 1 + Envir.Random.Next(2));
                        target.ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = 3, TickSpeed = 1000 }, this);
                    }
                    break;

                case 1:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(15) == 1)
                        target.Pushed(this, Direction, 1 + Envir.Random.Next(2));
                    break;
            }
        }
    }
}
