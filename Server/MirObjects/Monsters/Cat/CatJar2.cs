using Server.MirDatabase;
using System.Collections.Generic;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CatJar2 : MonsterObject
    {
        public bool SpawnedMobs { get; set; }

        public long SpawnDelay;

        protected override bool CanMove { get { return false; } }

        protected internal CatJar2(MonsterInfo info)
            : base(info)
        {
            Direction = MirDirection.Up;
            SpawnDelay = Envir.Time + 120 * Settings.Second;
        }

        public override void Turn(MirDirection dir)
        {
        }
        public override bool Walk(MirDirection dir) 
        { 
            return false; 
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 5);
        }

        protected override void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.ACAgility, 0);
            ActionList.Add(action);

            if (Envir.Random.Next(25) == 0)
            {
                Target.ApplyPoison(new Poison { PType = PoisonType.Frozen, Duration = 5, TickSpeed = 1000 }, this);
            }
        }


        protected override void Attack()
        {
            if (!CanAttack || Target == null) return;

            if (Envir.Random.Next(8) == 1 && SpawnedMobs == false && Envir.Time > SpawnDelay)
            {
                var mob = GetMonster(Envir.GetMonsterInfo(Settings.Jar2Mob));
                SpawnDelay = Envir.Time + 60 * Settings.Second;

                if (mob != null)
                {
                    if (!mob.Spawn(CurrentMap, Front))
                        mob.Spawn(CurrentMap, CurrentLocation);

                    mob.Target = Target;
                    mob.ActionTime = Envir.Time + 2000;
                    SlaveList.Add(mob);
                }

                mob = GetMonster(Envir.GetMonsterInfo(Settings.Jar2Mob));

                if (mob != null)
                {
                    if (!mob.Spawn(CurrentMap, Back))
                        mob.Spawn(CurrentMap, CurrentLocation);

                    mob.Target = Target;
                    mob.ActionTime = Envir.Time + 2000;
                    SlaveList.Add(mob);
                }

                SpawnedMobs = true;
            }
            else
            {
                ShockTime = 0;
                bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
                if (ranged)
                {
                    if (Envir.Random.Next(5) == 0)
                        RangeAttack();
                    else
                        MoveTo(Target.CurrentLocation);
                }
                else
                {
                    Attack1();
                }
            }

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
        }   
    }
}
