using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HornedMage : MonsterObject
    {
        protected internal HornedMage(MonsterInfo info)
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

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, Target, damage, DefenceType.ACAgility, 0);
            ActionList.Add(action);

            Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.HornedMage, EffectType = 0 });
            Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.HornedMage, EffectType = 1 });
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
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
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);


            if (Envir.Random.Next(5) == 0)
            {
                RangeAttack2();
            }
            else if (Envir.Random.Next(5) == 0)
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

            switch (type)
            {
                case 0:
                    List<MapObject> targets = FindAllTargets(3, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (!targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
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
