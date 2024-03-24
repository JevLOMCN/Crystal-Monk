using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLin7 : MonsterObject
    {
        private long SpellTime;
        protected internal DogYoLin7(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            List<MapObject> targets;
            switch (type)
            {
                case 0:
                    target.Attacked(this, damage, DefenceType.MAC);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.DogYoLin7 });
                    break;

                case 1:
                    AddBuff(new Buff { Type = BuffType.DogYoLinShield, Caster = this, ExpireTime = Envir.Time + 10000, Visible = true, TickSpeed = 1000, Values = new int[] { } });
                    break;

                case 2:
                    targets = FindAllTargets(5, target.CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        targets[i].AddBuff(new Buff { Type = BuffType.DogYoLin7, Caster = this, ExpireTime = Envir.Time + 5000, Visible = true, TickSpeed = 1000 });
                    }
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.DogYoLin7, EffectType = 1 });
                    break;

                case 3:
                    target.Attacked(this, damage, DefenceType.MAC);
                    break;
            }
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3 });
            int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay, Target, damage, DefenceType.MACAgility, 3);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + delay + 600, Target, damage, DefenceType.MACAgility, 3);
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

            if (Envir.Time > SpellTime && Envir.Random.Next(5) == 0)
            {
                SpellTime = Envir.Time + 15000;
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                RangeAttack2();
            }
            else if (ranged)
            {
                if (Envir.Random.Next(5) == 0)
                    RangeAttack();
                else if (Envir.Random.Next(5) == 0)
                    RangeAttack3();
                else if (Envir.Random.Next(3) == 0)
                    MoveTo(Target.CurrentLocation);
                else 
                    RangeAttack4();
            }
           
            else
            {
                Attack1();
            }
        }
    }
}
