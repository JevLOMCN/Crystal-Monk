using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Collections.Generic;

//A1R1C
namespace Server.MirObjects.Monsters
{
    class SwampWarrior : MonsterObject
    {
        protected internal SwampWarrior(MonsterInfo info)
            : base(info)
        {
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            if (targets.Count == 0) return;

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].Attacked(this, damage, DefenceType.ACAgility) <= 0) continue;
                if (Envir.Random.Next(3) == 0)
                {
                    targets[i].ApplyPoison(new Poison { Owner = this, Duration = 15, PType = PoisonType.Green, Value = GetAttackPower(MinDC, MaxDC) / 20, TickSpeed = 2000 }, this);
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
    }
}
