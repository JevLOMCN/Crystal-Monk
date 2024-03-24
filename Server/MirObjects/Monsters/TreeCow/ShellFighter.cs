using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class ShellFighter : MonsterObject
    {
        protected internal ShellFighter(MonsterInfo info)
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
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (Envir.Random.Next(5) == 0)
                RangeAttack4();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack3();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack();
            else
                Attack1();

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
                    targets = FindAllTargets(5, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(6) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Stun, Value = damage, TickSpeed = 1000 }, this);
                    }
                    break;

                case 2:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 50, PType = PoisonType.Green, Value = damage/15, TickSpeed = 1000 }, this);
                    }
                    break;
            }
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

            LineAttack(5, damage , DefenceType.MAC, 50);
        }

        protected override void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected override void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time, Target, damage, DefenceType.MACAgility, 3);
            ActionList.Add(action);
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3 });

            List<MapObject> targets = FindAllTargets(5, CurrentLocation);

            for (int i = 0; i < targets.Count; i++)
            {
                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1200, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                action = new DelayedAction(DelayedType.Damage, Envir.Time + 1500, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                action = new DelayedAction(DelayedType.Damage, Envir.Time + 2000, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                action = new DelayedAction(DelayedType.Damage, Envir.Time + 2500, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                action = new DelayedAction(DelayedType.Damage, Envir.Time + 3000, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);

                targets[i].Attacked(this, damage, DefenceType.MAC);
                if (Envir.Random.Next(5) == 0)
                    targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Slow, Value = damage, TickSpeed = 2000 }, this);
            }
        }
    }
}
