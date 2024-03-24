using Server.MirDatabase;
using System.Collections.Generic;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CatJar1 : MonsterObject
    {
        public bool SpawnedMobs { get; set; }

        public long SpawnDelay;

        protected override bool CanMove { get { return false; } }

        protected internal CatJar1(MonsterInfo info)
            : base(info)
        {
            Direction = MirDirection.Up;
            SpawnDelay = Envir.Time + 120 * Settings.Second;
        }

        protected override void Attack() { }

        public override void Turn(MirDirection dir)
        {
        }
        public override bool Walk(MirDirection dir) 
        { 
            return false; 
        }

        protected override void ProcessTarget()
        {


            if (!CanAttack || Target == null) return;

            if (Envir.Random.Next(8) == 1 && SpawnedMobs == false && Envir.Time > SpawnDelay)
            {
                var mob = GetMonster(Envir.GetMonsterInfo(Settings.Jar1Mob));
                SpawnDelay = Envir.Time + 60 * Settings.Second;

                if (mob != null)
                {
                    if (!mob.Spawn(CurrentMap, Front))
                        mob.Spawn(CurrentMap, CurrentLocation);

                    mob.Target = Target;
                    mob.ActionTime = Envir.Time + 2000;
                    SlaveList.Add(mob);
                }

                mob = GetMonster(Envir.GetMonsterInfo(Settings.Jar1Mob));

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
                List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
                if (targets.Count == 0) return;

                ShockTime = 0;

                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                for (int i = 0; i < targets.Count; i++)
                {
                    Target = targets[i];

                    if (Envir.Random.Next(5) == 0)
                    {
                        MirDirection pushdir = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                        Target.Pushed(this, pushdir, 1 + Envir.Random.Next(3));
                    }
                }

            }

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

        }   

    }
}
