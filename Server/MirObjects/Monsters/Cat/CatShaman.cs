using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CatShaman : MonsterObject
    {
        public CatShaman(MonsterInfo info)
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

            if (!CanAttack) return;

            if (Envir.Random.Next(4) == 0)
            {
                RangeAttack();
            }
            else if (Envir.Random.Next(4) == 0)
            {
                RangeAttack2();
            }
            else
            {
                Attack1();
            }

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 300;
        }

        protected override void RangeAttack()
        {
            int damage = (int)(1.5 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = (int)(1.5 * GetAttackPower(MinDC, MaxDC));
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            switch (type)
            {
                case 0:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);

                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.CatShaman });
                    break;

                case 1:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(6) == 0)
                    {
                        // TODO 禁足
                        Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.CatShaman, EffectType = 1 });
                        target.ApplyPoison(new Poison { Owner = this, Duration = 2, PType = PoisonType.Stun, Value = damage, TickSpeed = 1000 }, this);
                    }
                    break;
            }
        }
    }
}
