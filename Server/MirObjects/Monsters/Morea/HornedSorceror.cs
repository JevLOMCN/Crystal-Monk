using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HornedSorceror : MonsterObject
    {
        protected internal HornedSorceror(MonsterInfo info)
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

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (Envir.Random.Next(5) == 0)
                RangeAttack4();
            else if (Envir.Random.Next(15) == 0)
                RangeAttack3();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack();
            else
                Attack1();

            ActionTime = Envir.Time + 800;
            AttackTime = Envir.Time + AttackSpeed;
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
                case 1:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Frozen, Value = damage, TickSpeed = 1000 }, this);
                        }
                    }
                    break;
            }
        }

        protected override void RangeAttack()
        {
            int damage = 2 * GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

            LineAttack(3, damage, DefenceType.MAC, 50);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3 });

            LineAttack(2, damage);
            MoveForward(3, 200);
        }

        protected override void RangeAttack3()
        {
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });

            SpellObject spellObj = new SpellObject
            {
                Spell = Spell.SandStorm,
                Value = Envir.Random.Next(MinDC, MaxDC),
                ExpireTime = Envir.Time + 7000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = Target.CurrentLocation,
                CurrentMap = CurrentMap,
                Range = 1
            };

            DelayedAction action = new DelayedAction(DelayedType.Spawn, Envir.Time + 300, spellObj);
            CurrentMap.ActionList.Add(action);
        }
    }
}
