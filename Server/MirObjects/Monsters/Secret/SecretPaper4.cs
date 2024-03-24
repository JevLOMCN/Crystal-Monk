using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class SecretPaper4 : MonsterObject
    {
        protected internal SecretPaper4(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 6);
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

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged)
            {
                if (Envir.Random.Next(5) == 0)
                    MoveTo(Target.CurrentLocation);
                else
                {
                    Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                    ActionTime = Envir.Time + 500;
                    AttackTime = Envir.Time + AttackSpeed;
                    RangeAttack();
                }
            }
            else if (Envir.Random.Next(5) == 0)
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                RangeAttack();
            }
            else
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
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
            target.Attacked(this, damage, DefenceType.MAC);
            Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.SecretPage4 });
        }
    }
}