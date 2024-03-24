using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MirObjects.Monsters
{
    public class CrystalBeast : MonsterObject
    {
        /// <summary>
        /// Variables to be used for Slaves
        /// </summary>
        public string SlaveName = Settings.CrystalBeastSlave;
        public bool spawn75 = false;
        public bool spawn50 = false;
        public bool spawn25 = false;

        public int DebuffDuration; //seconds
        public long debuffTime;
        public int[] DebuffAmount = new int[]//10
        {
            0,//hp0
            0,//mp1
            0,//dc2
            0,//mc3
            0,//sc4
            0,//ac5
            0,//mac6
            0,//crit dmg7
            0,//reflect8
            0,//hp drain9
        };//5 stats
        public int StealDuration;
        public int[] StolenStats = new int[]//9
        {
            0,//hp0
            0,//dc1
            0,//mc2
            0,//sc3
            0,//ac4
            0,//mac5
            0,//crit dmg6
            0,//reflect7
            0,//hp drain8
        };

        List<MonsterObject> slaves = new List<MonsterObject>();

        public CrystalBeast(MonsterInfo info)
            :base(info)
        {
            DebuffDuration = Envir.Random.Next(1, 10);
            StealDuration = DebuffDuration;
            DebuffAmount[0] = Envir.Random.Next(50, 200);//hp
            StolenStats[0] = DebuffAmount[0];
            DebuffAmount[1] = Envir.Random.Next(50, 200);//mp
            DebuffAmount[2] = Envir.Random.Next(10, 40);//DC
            StolenStats[2] = DebuffAmount[2];
            DebuffAmount[3] = Envir.Random.Next(10, 40);//MC
            StolenStats[3] = DebuffAmount[3];
            DebuffAmount[4] = Envir.Random.Next(10, 40);//SC
            StolenStats[4] = DebuffAmount[4];
            DebuffAmount[5] = Envir.Random.Next(10, 40);//AC
            StolenStats[5] = DebuffAmount[5];
            DebuffAmount[6] = Envir.Random.Next(10, 40);//AMC
            StolenStats[6] = DebuffAmount[6];
            DebuffAmount[7] = Envir.Random.Next(10, 40);//crit dmg
            DebuffAmount[8] = Envir.Random.Next(10, 40);//reflect
            DebuffAmount[9] = Envir.Random.Next(10, 40);//HP drain
        }

        /// <summary>
        /// Override the die method in order to kill the slaves on the CrystalBeasts death.
        /// </summary>
        public override void Die()
        {
            for (int i = 0; i < slaves.Count; i++)
                slaves[i].Die();
            base.Die();
        }

        /// <summary>
        /// Override the change HP in order to perform the spawns
        /// </summary>
        /// <param name="amount"></param>
        public override void ChangeHP(int amount)
        {
            //  Healths between 51% & 75% and the 3/4 spawn hasn't been made yet.
            if (PercentHealth > 50 && PercentHealth <= 75 &&
                !spawn75)
                spawn75 = SpawnMinion();
            //  Healths between 26% & 50% and the 1/2 spawn hasn't been made yet.
            if (PercentHealth > 25 && PercentHealth <= 50 &&
                !spawn50)
                spawn50 = SpawnMinion();
            //  Healths between 0% and 25% and the 1/4 spawn hasn't been made yet.
            if (PercentHealth <= 25 &&
                !spawn25)
                spawn25 = SpawnMinion();
            //  Keep the base to ensure the monster still loses HP, wouldn't want an immortal Boss now would we? :)
            base.ChangeHP(amount);
        }

        /// <summary>
        /// This will be used for the Blizzard Attack, it'll Channnel the attack
        /// </summary>
        public long BlizzardChannelTime = 2000;
        /// <summary>
        /// This is used for knowing when to use it again.
        /// </summary>
        public long BlizzardAttackTime;
        /// <summary>
        /// This will be used in order to know if we've cast Blizzard yet
        /// </summary>
        public bool BlizzardCast;
        /// <summary>
        /// This will be used in order to not perform any other action until Blizzard is complete.
        /// </summary>
        public bool Casting;
        /// <summary>
        /// This is used to know when Channeling has finished
        /// </summary>
        public long BlizzardCastingTime;

        /// <summary>
        /// Here will wee Perform the Blizzard attack.
        /// </summary>
        public void PerformBlizzard()
        {
            //  Blizzard is Channeling & has finished Channeling
            if (BlizzardCast &&
                Envir.Time > BlizzardCastingTime)
            {
                //  Broadcast the Packets for the Attack frames & Effects.
                Broadcast(new ServerPackets.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                Broadcast(new ServerPackets.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.BlizzardCast });
                //  Get every target around the main target within a range of 4
                List<MapObject> targets = FindAllTargets(4, Target.CurrentLocation, false);
                //  Cycle through every target in that list
                for (int i = 0; i < targets.Count; i++)
                {
                    //  Each spot will have different damage than others
                    int damage = GetAttackPower(MinMC, MaxMC);
                    //  Create a new Action type. (Created MonsterMagic for you)
                    // TODO
                  //  DelayedAction action = new DelayedAction(DelayedType.MonsterMagic, Envir.Time + 500, this, Spell.CrystalBeastBlizz, damage, Target.CurrentLocation);
                    //  Add the action to the current map
                //    CurrentMap.ActionList.Add(action);
                    //  Apply new damage values
                    damage = GetAttackPower(MinMC, MaxMC);
                    //  Attack the target as the first hit.
                    targets[i].Attacked(this, damage, DefenceType.MAC);
                }
                //  Blizzard is no longer Cast
                BlizzardCast = false;
                //  Blizzard is no longer Channeling.
                Casting = false;
            }
            //  Blizzard hasn't been cast
            else if (!BlizzardCast)
            {
                //  We're now Chanelling
                Casting = true;
                //  We've now Cast Blizzard
                BlizzardCast = true;
                //  Broadcast the Packet for the channeling effect
                Broadcast(new ServerPackets.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.CastingBlizzard });
                //  Set the Casting time to 5 seconds
                BlizzardCastingTime = Envir.Time + BlizzardChannelTime;
            }
        }


        /// <summary>
        /// Method to spawn other monsters
        /// </summary>
        /// <returns></returns>
        public bool SpawnMinion()
        {
            MonsterObject mob = null;
            //  Get the monster to spawn as slave by name defined
            mob = GetMonster(Envir.GetMonsterInfo(SlaveName));
            //  Ensure we're not trying to spawn an invalid monster
            if (mob == null)
                return false;
            //  Get a random point on the map within 6 spots from current location
            Point randomLocation = GetRandomPoint(14, 6, CurrentMap);

            //  Spawn the monster at the random location
            if (!mob.Spawn(CurrentMap, randomLocation))
                //  If we can't spawn the monster at the random location, spawn it on the CrystalBeast
                mob.Spawn(CurrentMap, CurrentLocation);
            //  Ensure the target isn't null or dead
            if (Target == null || Target.Dead)
                //  Find a new target if the conditions are met
                FindTarget();
            //  Slaves will target the CrystalBeasts target.
            mob.Target = Target;
            mob.ActionTime = Envir.Time + 2000;
            //  Add the Slave the the custom slave list so we can destroy it if the CrystalBeast dies (see the Die override)
            slaves.Add(mob);
            return true;
        }

        /// <summary>
        /// Ensure we set a time so it's not using Slash Attack all the time
        /// </summary>
        public long SlashAttackTime;

        /// <summary>
        /// Perform Slash attack to one target, this will deal 25% more damage than any other
        /// </summary>
        public void PerformSlashAttack()
        {
            if (!Target.Dead)
            {
                //  Get the damage from Min/Max DC
                int damage = GetAttackPower(MinDC, MaxDC);
                // Get a 25% boost
                int tmp = damage * 100 / 25;
                // Apply that 25% boost onto the Damage
                damage += tmp;
                // Attack the target with the 25% damage boost and ignore Agility
                Target.Attacked(this, damage, DefenceType.AC);
                // Send the Packet to client and play the Slash Animation.
                Broadcast(new ServerPackets.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Type = 2, Location = CurrentLocation });
            }
        }

        /// <summary>
        /// If the Target isn't in range for melee hit, we shall perform a RangedAttack on it.
        /// </summary>
        public void PerformRangeAttack()
        {
            //  Number of Columns we're going to hit
            int col = 3;
            //  Number of Rows we'll hit
            int row = 4;
            //  Define a Point Array variable
            Point[] loc = new Point[col];
            //  Populate the Point Array variable with the three spaces in front of the mob.
            loc[0] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 1);
            loc[1] = Functions.PointMove(CurrentLocation, Direction, 1);
            loc[2] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 1);
            //  Cycle through the columns first.
            for (int i = 0; i < col; i++)
            {
                // The start point will change to the next column each cycle
                Point startPoint = loc[i];
                //  Cycle through the rows
                for (int j = 0; j < row; j++)
                {
                    //  Get the hit point
                    Point hitPoint = Functions.PointMove(startPoint, Direction, j);
                    //  Check if the Hit point is valid
                    if (!CurrentMap.ValidPoint(hitPoint))
                        continue;
                    //  Get the Map Cell
                    var cellObjects = CurrentMap.GetCellObjects(hitPoint);
                    //  Check the Map cell has objects to hit
                    if (cellObjects == null)
                        continue;
                    //  Cycle through the Objects within that cell
                    for (int k = 0; k < cellObjects.Count; k++)
                    {
                        //  Assign the Object as a Map Object
                        MapObject target = cellObjects[k];
                        //  Switch between the MapObjects race.
                        switch (target.Race)
                        {
                            //  We'll only hit Players or Monsters
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                {
                                    //  Check if the target is a valid target
                                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) continue;
                                        //  Get the damage for each target in order to provide diverse damage between each target
                                        int damage = GetAttackPower(MinMC, MaxMC);
                                        //  Attack the target, the damage will depend on the targets MAC + Agil
                                        target.Attacked(this, damage, DefenceType.MACAgility);
                                        Broadcast(new ServerPackets.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.CrystalBeastBlast, DelayTime = 800 });
                                }
                                break;
                        }
                    }
                }
            }
            //  Send Range attack packet to the client in order to perform the frames & the Scatter animation.
            Broadcast(new ServerPackets.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Type = 1, Location = CurrentLocation });
        }

        /// <summary>
        /// We need to define a long variable so we can ensure the SpinAttack isn't going to happen every attack.
        /// </summary>
        public long SpinAttackTime;
        /// <summary>
        /// Will attack all targets around it's self three times.
        /// </summary>
        public void PerformSpinAttack()
        {
            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsAttackTarget(this))
                {
                    //Damage will be different on each hit
                    int damage = GetAttackPower(MinDC, MaxDC);
                    if (Envir.Time > debuffTime)
                    {
                        // TODO
                    //    targets[i].AddBuff(new Buff { Caster = this, ExpireTime = Envir.Time + (DebuffDuration * Settings.Minute), ObjectID = targets[i].ObjectID, Type = BuffType.MobDebuff, Values = DebuffAmount });
                        targets[i].ApplyPoison(new Poison { Duration = Envir.Random.Next(3, 5), Owner = this, PType = PoisonType.Frozen, TickSpeed = 1000, Value = GetAttackPower(MinDC, MaxDC) });
                   //     AddBuff(new Buff { Caster = this, Values = StolenStats, ExpireTime = Envir.Time + (StealDuration * Settings.Minute), Type = BuffType.MobDebuff, ObjectID = ObjectID });
                        DebuffDuration = Envir.Random.Next(1, 10);
                        StealDuration = DebuffDuration;
                        DebuffAmount[0] = Envir.Random.Next(50, 200);//hp
                        StolenStats[0] = DebuffAmount[0];
                        DebuffAmount[1] = Envir.Random.Next(50, 200);//mp
                        DebuffAmount[2] = Envir.Random.Next(10, 40);//DC
                        StolenStats[2] = DebuffAmount[2];
                        DebuffAmount[3] = Envir.Random.Next(10, 40);//MC
                        StolenStats[3] = DebuffAmount[3];
                        DebuffAmount[4] = Envir.Random.Next(10, 40);//SC
                        StolenStats[4] = DebuffAmount[4];
                        DebuffAmount[5] = Envir.Random.Next(10, 40);//AC
                        StolenStats[5] = DebuffAmount[5];
                        DebuffAmount[6] = Envir.Random.Next(10, 40);//AMC
                        StolenStats[6] = DebuffAmount[6];
                        DebuffAmount[7] = Envir.Random.Next(10, 40);//crit dmg
                        DebuffAmount[8] = Envir.Random.Next(10, 40);//reflect
                        DebuffAmount[9] = Envir.Random.Next(10, 40);//HP drain
                        debuffTime = Envir.Time + (DebuffDuration * Settings.Minute);
                    }
                    targets[i].Attacked(this, damage, DefenceType.ACAgility);
                    damage = GetAttackPower(MinDC, MaxDC);
                    //  Use DelayedAction to allow multiple hits
                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + Settings.Second * 1, targets[i], damage, DefenceType.ACAgility);
                    ActionList.Add(action);
                    damage = GetAttackPower(MinDC, MaxDC);
                    action = new DelayedAction(DelayedType.Damage, Envir.Time + Settings.Second * 2, targets[i], damage, DefenceType.ACAgility);
                    ActionList.Add(action);
                    damage = GetAttackPower(MinDC, MaxDC);
                    action = new DelayedAction(DelayedType.Damage, Envir.Time + Settings.Second * 3, targets[i], damage, DefenceType.ACAgility);
                    ActionList.Add(action);
                }
            }
            //  Broadcast the ActionType on the client (the Attack Frames) Type = Attack2
            Broadcast(new ServerPackets.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });
        }

        /// <summary>
        /// A bit like Halfmoon but hits 1 less target.
        /// </summary>
        public void PerformSliceAttack()
        {
            //  Get the Point to the Left of where the Crystal Beast is facing
            MirDirection dir = Functions.PreviousDir(Direction);
            //  Cycle directions 4 times (4 spots to hit)
            for (int i = 0; i < 4; i++)
            {
                //  Get the Location from the Direction defined
                Point loc = Functions.PointMove(CurrentLocation, dir, 1);
                //  Set the next Direction in order for the Location to be used next
                dir = Functions.NextDir(dir);
                //  Ensure the Location is valid
                if (!CurrentMap.ValidPoint(loc))
                    continue;
                //  Get the Cell
                var cellObjects = CurrentMap.GetCellObjects(loc);
                //  Ensure the Cell has objects
                if (cellObjects == null)
                    continue;
                //  Cycle through the objects on the cell.
                for (int o = 0; o < cellObjects.Count; o++)
                {
                    //  Assign the object as a MapObject
                    MapObject ob = cellObjects[o];
                    //  Ensure the object is a player or monster
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster)
                        continue;
                    //  Ensure we're allow to attack the target
                    if (!ob.IsAttackTarget(this))
                        continue;
                    //  Set the damage for each target so it's not the same damage for each target
                    int damage = GetAttackPower(MinMC, MaxMC);
                    //  Attack the target
                    ob.Attacked(this, damage, DefenceType.ACAgility);
                    break;
                }
            }
            //  Broadcast the ActionType on the client (the Attack Frames) Type = Attack1
            Broadcast(new ServerPackets.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
        }


        /// <summary>
        /// Override the ProcessTarget in order to setup the AI.
        /// </summary>
        protected override void ProcessTarget()
        {
            //  Ensure we're not trying to attack and invalid Target (Dead or non existent)
            if (Target == null || Target.Dead)
            {
                FindTarget();
                return;
            }
            //  Get the Direction to face and use on the ObjectAttack Packet
            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            //  Ensure we're using the Attack time (attack Speed)
            if (Envir.Time > AttackTime)
            {
                //  If we've casted (Channeled) Blizzard, we will perform the actual Blizzard
                if (BlizzardCast && Casting)
                {
                    PerformBlizzard();
                    BlizzardAttackTime = Envir.Time + Settings.Second * 35;
                }
                //  Don't Perform any other action if we're channeling Blizzard
                if (!Casting)
                {
                    //  If we haven't casted Blizzard & we're now ablt to perform it, start Channeling.
                    if (Envir.Time > BlizzardAttackTime &&
                        !BlizzardCast)
                    {
                        PerformBlizzard();
                    }
                    //  Current time has to be larger than the SpinAttack time.
                    if (Envir.Time > SpinAttackTime)
                    {
                        //  Perform the Spin Attack
                        PerformSpinAttack();
                        //  Set the SpinAttack time to 30 seconds
                        SpinAttackTime = Envir.Time + Settings.Second * 30;
                    }
                    else if (InAttackRange() &&
                             Envir.Time > SlashAttackTime)
                    {
                        PerformSlashAttack();
                        //Slash attack is done every 15 seconds.
                        SlashAttackTime = Envir.Time + Settings.Second * 15;
                    }
                    //  If we cannot use SpinAttack, ensure we're within range of the Target and perform the SliceAttack
                    else if (InAttackRange())
                        PerformSliceAttack();
                    //  If we're not in range, we'll perform a Ranged Attack.
                    else
                        PerformRangeAttack();
                }
                //  Set the Attack time to the Attack Speed
                AttackTime = Envir.Time + AttackSpeed;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }
            //  If we're not in Range, move to the target
            if (!InAttackRange())
                MoveTo(Target.CurrentLocation);
            //  If the targets invalid or dead, find a new one.
            if (Target == null || Target.Dead)
            {
                FindTarget();
                return;
            }
            //  Move to the Target (it'll check if in range)
            MoveTo(Target.CurrentLocation);
        }
    }
}
