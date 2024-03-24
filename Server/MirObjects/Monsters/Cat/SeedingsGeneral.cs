using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class SeedingsGeneral : MonsterObject
    {
        public SeedingsGeneral(MonsterInfo info)
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

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (Envir.Random.Next(3) == 0)
            {
                RangeAttack();
            }
            else if (Envir.Random.Next(3) == 0)
            {
                RangeAttack2();
            }
            else if (Envir.Random.Next(3) == 0)
            {
                RangeAttack3();
            }
            else
            {
                Attack1();
            }

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
        }

        protected override void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });
            LineAttack(3, damage, DefenceType.MACAgility, 100);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            List<MapObject> targets;
            switch (type)
            {
                case 0:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(5) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 30, PType = PoisonType.Red, Value = damage / 20, TickSpeed = 1000 }, this);
                    }
                    break;

                case 1:
                    targets = FindAllTargets(1, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (!targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(6) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 12, PType = PoisonType.Slow, Value = damage / 20, TickSpeed = 1000 }, this);
                        }
                    }
                    break;

            }
        }
   }
}
