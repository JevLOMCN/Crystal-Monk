using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;

namespace Server.MirObjects.Monsters
{
    //          frame.Frames.Add(MirAction.Attack1, new Frame(96, 8, 0, 100));
    //        frame.Frames.Add(MirAction.Attack2, new Frame(160, 9, 0, 100));
    //        frame.Frames.Add(MirAction.Attack3, new Frame(232, 10, 0, 100));
    //        frame.Frames.Add(MirAction.AttackRange1, new Frame(312, 10, 0, 200));
    //        frame.Frames.Add(MirAction.AttackRange2, new Frame(384, 9, 0, 200));

    public class ChieftainSword : MonsterObject
    {
        protected bool HasBomb;
        protected long FireCD;

        protected bool Transparent;
        private bool Transparent1;

        public ChieftainSword(MonsterInfo info)
            : base(info)
        {
        }

        public override bool Blocking
        {
            get { return !Transparent && !Transparent1; }
        }

        private void SpawnFireWall()
        {
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (j == 1 && i == 1)
                        continue;

                    Point location = new Point(CurrentLocation.X + (j - 1) * 7,
                                             CurrentLocation.Y + (i - 1) * 7);

                    SpellObject spellObj = null;

                    spellObj = new SpellObject
                    {
                        Spell = Spell.MapFire,
                        Value = Envir.Random.Next(MinDC, MaxDC),
                        ExpireTime = Envir.Time + 7000,
                        TickSpeed = 1000,
                        Caster = this,
                        CurrentLocation = location,
                        CurrentMap = CurrentMap,
                        Range = 3
                    };

                    DelayedAction action = new DelayedAction(DelayedType.Spawn, Envir.Time + 300, spellObj);
                    CurrentMap.ActionList.Add(action);
                }
            }
        }

        protected override void RangeAttack()
        {
            int damage = GetAttackPower(MinMC, MaxMC) * 4;
            if (damage == 0) return;
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            AttackTime = Envir.Time + AttackSpeed + 500;

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        protected override void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

            SpellObject spellObj = new SpellObject
            {
                Spell = Spell.BloodBoom,
                Value = Envir.Random.Next(MinDC, MaxDC),
                ExpireTime = Envir.Time + 6000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = CurrentLocation,
                CurrentMap = CurrentMap,
                Range = 1,
            };

            DelayedAction action = new DelayedAction(DelayedType.Spawn, Envir.Time + 1000, spellObj);
            CurrentMap.ActionList.Add(action);

            ActionTime = Envir.Time + 7000;
            AttackTime = Envir.Time + 7000;
        }

        protected override void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 3 });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);

            MoveForward(3, 500);
        }

        private void RangeAttack5()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 5 });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        private void RangeAttack6()
        {
            Broadcast(GetInfo());
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 7 });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        private void RangeAttack9()
        {
            Transparent = false;
            Broadcast(GetInfo());
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 8 });

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 1000, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        private void RangeAttack7()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 6 });
            Transparent = true;
        }

        private void CallDragon()
        {
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 4 });

            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X + 2, CurrentLocation.Y, Settings.JobKing1);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y-2, Settings.JobKing2);
            if (mob != null)
                mob.Owner = this; ;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y + 2, Settings.JobKing3);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y, Settings.JobKing4);
            if (mob != null)
                mob.Owner = this;

            mob = RegenMonsterByName(CurrentMap, CurrentLocation.X - 2, CurrentLocation.Y, Settings.JobKing3);
            if (mob != null)
                mob.Owner = this;
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

            if (Transparent)
            {
                if (Transparent1)
                {
                    RangeAttack6();
                    Transparent1 = false;
                    ActionTime = Envir.Time + 5000;
                    AttackTime = Envir.Time + AttackSpeed;
                    return;
                }
                else
                {
                    RangeAttack9();
                }
            }
            else if (Envir.Random.Next(10) == 0)
            {
                RangeAttack7();
                Transparent1 = true;
                ActionTime = Envir.Time + 5000;
                AttackTime = Envir.Time + AttackSpeed;
                return;
            }
            else if (Envir.Random.Next(4) == 0 && HP < MaxHP * 0.5)
            {
                CallDragon();
            }
            else if (Envir.Random.Next(10) == 0)
            {
                RangeAttack3();
                return;
            }
            else if (Envir.Random.Next(15) == 0)
            {
                SpawnFireWall();
            }
            else if (Envir.Random.Next(10) == 0)
            {
                RangeAttack4();
            }
            else if (Envir.Random.Next(5) == 0)
            {
                RangeAttack5();
            }
            else if (Envir.Random.Next(5) == 0)
            {
                RangeAttack();
            }
            else
            {
                Attack1();
            }

            ActionTime = Envir.Time + 800;
            AttackTime = Envir.Time + AttackSpeed;
        }
    }
}
