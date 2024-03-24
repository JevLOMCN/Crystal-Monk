using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HumanMonster : MonsterObject
    {
        private List<UserMagic> MagicList = new List<UserMagic>();

        public long  MeteorBlizzTime, FireWallTime, RepulseTime, _RageTime, PoisonCloudTime, SoulShieldTime, BlessedArmourTime, CurseTime;
        public long CastTime, SpellCastTime;
        public long NextMagicShieldTime, NextRageTime, NextFlamingSwordTime, NextSoulShieldTime, NextBlessedArmourTime, NextCurseTime;
        public bool Casting = false;
        public MirClass mobsClass;
        public MirGender mobsGender;
        public short weapon, armour;
        public byte wing, hair, light;
        public uint MP = 65535;
        public bool Summoned;
        public bool BoRecallComplete;

        public HumanMonster(MonsterInfo info)
            : base(info)
        {
            GetHumanInfo();
            Direction = MirDirection.Down;
            Summoned = true;
            switch (mobsClass)
            {
                case MirClass.Warrior:
                    MP = 1000;
                    break;
                case MirClass.Wizard:
                    MP = 2500;
                    break;
                case MirClass.Taoist:
                    MP = 1750;
                    break;
                case MirClass.Monk:
                    MP = 1000;
                    break;
            }

            Spell[] Magics = new Spell[] {Spell.ThunderBolt, Spell.SoulFireBall, Spell.Poisoning, Spell.SummonShinsu, Spell.IceStorm,
            Spell.DoubleSlash,  Spell.SummonShinsu, Spell.JinGangGunFa};
            foreach (Spell spell in Magics)
            {
                UserMagic magic = new UserMagic(spell);
                magic.Level = 3;
                MagicList.Add(magic);
            }
        }

        public UserMagic GetMagic(Spell spell)
        {
            for (int i = 0; i < MagicList.Count; i++)
            {
                UserMagic magic = MagicList[i];
                if (magic.Spell != spell) continue;
                return magic;
            }

            return null;
        }

        protected override bool InAttackRange()
        {
            if (Target.CurrentMap != CurrentMap) return false;

            switch (mobsClass)
            {
                case MirClass.Warrior:
                case MirClass.Assassin:
                    return Target.CurrentLocation != CurrentLocation && Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

                case MirClass.Taoist:
                case MirClass.Wizard:
                    return Target.CurrentLocation != CurrentLocation && Functions.InRange(CurrentLocation, Target.CurrentLocation, 11);

                case MirClass.Monk:
                    return Target.CurrentLocation != CurrentLocation && Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            }

            return false;
        }

        public int GetHitCount()
        {
            int count = 0;
            if (Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) <= 2)
                if (Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) == 1)
                {
                    Point tmmp = Functions.PointMove(CurrentLocation, Direction, 2);
                    if (CurrentMap.ValidPoint(tmmp))
                    {
                        var cellObjects = CurrentMap.GetCellObjects(tmmp);
                        if (cellObjects != null)
                            for (int i = 0; i < cellObjects.Count; i++)
                                if ((cellObjects[i].Race == ObjectType.Player ||
                                    cellObjects[i].Race == ObjectType.Monster) && 
                                    cellObjects[i].IsAttackTarget(this))
                                        count = 2;
                    }
                }
                else
                    count = 1;
            return count;
        }

        public long MPRegenTime;
        protected override void ProcessRegen()
        {
            if (Envir.Time > MPRegenTime)
            {
                int MPRegen = 10;
                if (MP + MPRegen > uint.MaxValue)
                    MP = uint.MaxValue;
                else
                    MP += (uint)MPRegen;
                MPRegenTime = Envir.Time + Settings.Second * 3;
            }
            base.ProcessRegen();
        }

        private void WarrorAttackTarget()
        {
            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            int closeTargets = targets.Count;
            if (InAttackRange())
            {
                if (Envir.Time > NextFlamingSwordTime && MP >= 45)
                {
                    MP -= 45;
                    NextFlamingSwordTime = Envir.Time + Settings.Second * Envir.Random.Next(15, 25);
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.FlamingSword, Level = 3 });
                    PerformFlamingSword();
                    return;
                }
                else if (Envir.Time > NextRageTime && MP >= 18 && Envir.Time > _RageTime)
                {
                    MP -= 18;
                    _RageTime = Envir.Time + Settings.Second * 20;
                    NextRageTime = Envir.Time + Settings.Second + Envir.Random.Next(30, 45);
                    Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                    PerformRage();
                    return;
                }
                else if (MP >= 9 && closeTargets <= 1)
                {
                    MP -= 9;
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.TwinDrakeBlade, Level = 3 });
                    PerformTwinDrakeBlade();
                    return;
                }
                else if (MP >= 6 && closeTargets >= 2)
                {
                    if (Envir.Random.Next(0, 10) >= 5
                        && MP >= 8)
                    {
                        MP -= 8;
                        Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.CrossHalfMoon, Level = 3 });
                        PerformCrossHalfMoon();
                        return;
                    }
                    else
                    {
                        MP -= 6;
                        Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.HalfMoon, Level = 3 });
                        PerformHalfmoon();
                        return;
                    }
                }
                else if (GetHitCount() == 2)
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.Thrusting, Level = 3 });
                    PerformThrusting();
                    return;
                }
                else if (Functions.InRange(CurrentLocation, Target.CurrentLocation, 1))
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                    // Attack();
                    int damage = GetAttackPower(MinDC, MaxDC);
                    if (damage == 0)
                        return;

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.AC, true);
                    ActionList.Add(action);
                    return;
                }
            }
        }

        private void TaoistAttackTarget()
        {
            if (!BoRecallComplete)
            {
                MP -= 50;
                CallDragion();
                BoRecallComplete = true;
            }
            else if (Target.PoisonList.Count < 2 && MP >= 6)
            {
                MP -= 6;
                PerformPoisoning();
            }
            else if (MP >= 8)
            {
                MP -= 6;
                PerformSoulFireBall();
            }
        }

        private void CallDragion()
        {
            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.SummonShinsu, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });

            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y - 2, Settings.ShinsuName);
            if (mob != null)
            {
                mob.Owner = this;
                mob.Master = this;
                mob.PetLevel = 4;
                mob.MaxPetLevel = 7;
                Pets.Add(mob);
            }
        }

        private void WizardAttackTarget()
        {
            if (MagicShield && Envir.Time > MagicShieldTime)
                MagicShield = false;

            if (!Casting)
            {
                //  Favour MagicShield
                if (!MagicShield && Envir.Time > NextMagicShieldTime && MP >= 28)
                {
                    MP -= 28;
                    Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation,
                    Spell = Spell.MagicShield, Level = 3});
                    PerformMagicShield();
                    return;
                }
                else if (Envir.Time > SpellCastTime && MP >= 20)
                {
                    MP -= 20;
                    SpellCastTime = Envir.Time + 1500;
                    PerformThunderBolt();
                    return;
                }
                else
                {
                    MP -= 48;
                    PerforeIceStorm();
                    return;
                }
            }
            else if (Envir.Time > CastTime)
                Casting = false;
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
            switch (mobsClass)
            {
                case MirClass.Warrior:
                    WarrorAttackTarget();
                    break;

                case MirClass.Wizard:
                    WizardAttackTarget();
                    break;

                case MirClass.Taoist:
                    TaoistAttackTarget();
                    break;

                case MirClass.Assassin:
                    AssassinAttackTarget();
                    break;

                case MirClass.Monk:
                    MonkAttackTarget();
                    break;
            }
        }

        private void AssassinAttackTarget()
        {
            PerformDoubleFlash();
        }

        private void MonkAttackTarget()
        {

        }

        private void PerformDoubleFlash()
        {
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.DoubleSlash, Level = 3 });

            int damage = GetReducedAttackPower(MinDC, MaxDC);
            damage = (int)(1.3 * GetAttackPower(MinDC, MaxDC));

            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 100, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);

            action = new DelayedAction(DelayedType.Damage, Envir.Time + 400, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        public void GetHumanInfo()
        {
            if (Settings.HumanMon != null && Settings.HumanMon.Count > 0)
            {
                for (int i = 0; i < Settings.HumanMon.Count; i++)
                {
                    if (Settings.HumanMon[i].HumansName.ToLower() == Info.Name.ToLower())
                    {
                        mobsClass = Settings.HumanMon[i].MobsClass;
                        mobsGender = Settings.HumanMon[i].MobsGender;
                        weapon = Settings.HumanMon[i].Weapon;
                        armour = Settings.HumanMon[i].Armour;
                        wing = Settings.HumanMon[i].Wing;
                        hair = Settings.HumanMon[i].Hair;
                        light = Settings.HumanMon[i].Light;
                        return;
                    }
                }
            }

            SMain.EnqueueDebugging(string.Format("Could not find {0}", Info.Name));
        }

        public void PerformFireBall()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);            

            int damage = GetAttackPower(MinMC, MaxMC);
            if (damage == 0)
                return;

            if (Envir.Random.Next(Settings.MagicResistWeight) >= Target.MagicResist)
            {
                int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                ActionList.Add(action);
                Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.FireBall, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });
            }
            if (Target.Dead)
                FindTarget();
        }

        public void PerformThunderBolt()
        {
            if (Target == null || !Target.IsAttackTarget(this)) return;
            UserMagic magic = GetMagic(Spell.ThunderBolt);
            if (magic == null) return;

            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.ThunderBolt, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            if (Target.Undead) damage = (int)(damage * 1.5F);
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, Target);
            ActionList.Add(action);
        }

        public void PerformRepulse()
        {
            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)            
                if (targets[i].IsAttackTarget(this))                
                    targets[i].Pushed(this, Functions.DirectionFromPoint(targets[i].CurrentLocation, targets[i].Back), 4);
            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.Repulsion, Cast = true, Level = 3 });
        }

        public void PerforeIceStorm()
        {
            if (Target == null || !Target.IsAttackTarget(this)) return;
            UserMagic magic = GetMagic(Spell.IceStorm);
            if (magic == null) return;

            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.IceStorm, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, Target);
            ActionList.Add(action);
        }

        public void PerformMagicShield()
        {
            NextMagicShieldTime = Envir.Time + Settings.Second * 45;
            MagicShieldTime = Envir.Time + Settings.Second * 20;
            MagicShield = true;
            MagicShieldLv = 3;
            CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MagicShieldUp }, CurrentLocation);

            AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
        }
        
        #region Warrior
        public void PerformFlamingSword()
        {
            int damage = GetAttackPower(MinDC * 2, MaxDC * 2);
            Target.Attacked(this, damage, DefenceType.AC);
        }

        public void PerformTwinDrakeBlade()
        {
            int damage = GetReducedAttackPower(MinDC, MaxDC);
            Target.Attacked(this, damage, DefenceType.ACAgility);
            damage = GetReducedAttackPower(MinDC, MaxDC);
            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 800, Target, damage, DefenceType.ACAgility);
            ActionList.Add(action);
        }

        public int GetReducedAttackPower(int min, int max)
        {
            int damage = GetAttackPower(min, max);
            float tmp = damage / 0.75f;
            damage = (int)tmp;
            return damage;
        }

        public void PerformHalfmoon()
        {
            MirDirection dir = Functions.PreviousDir(Direction);
            for (int i = 0; i < 3; i++)
            {
                Point location = Functions.PointMove(CurrentLocation, dir, 1);
                if (!CurrentMap.ValidPoint(location))
                    continue;
                var cellObjects = CurrentMap.GetCellObjects(location);
                if (
                    cellObjects != null)
                {
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        if (cellObjects[x].Race == ObjectType.Player ||
                            cellObjects[x].Race == ObjectType.Monster)
                        {
                            if (cellObjects[x].IsAttackTarget(this))
                            {
                                int damage = GetAttackPower(MinDC, MaxDC);
                                cellObjects[x].Attacked(this, damage, DefenceType.ACAgility);
                            }
                        }
                    }
                }
                dir = Functions.NextDir(dir);
            }
        }

        public void PerformCrossHalfMoon()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            List<MapObject> targets = FindAllTargets(1, CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)
                if (targets[i].IsAttackTarget(this))
                    targets[i].Attacked(this, damage, DefenceType.ACAgility);
        }

        public void PerformThrusting()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0)
                return;

            for (int i = 1; i <= 2; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                if (target == Target.CurrentLocation)
                {
                    if (Target.Attacked(this, damage, DefenceType.MACAgility) > 0 && Envir.Random.Next(8) == 0)
                    {
                        if (Envir.Random.Next(Settings.PoisonResistWeight) >= Target.PoisonResist)
                        {
                            int poison = GetAttackPower(MinSC, MaxSC);

                            Target.ApplyPoison(new Poison
                            {
                                Owner = this,
                                Duration = 5,
                                PType = PoisonType.Green,
                                Value = poison,
                                TickSpeed = 2000
                            }, this);
                        }
                    }
                }
                else
                {
                    if (!CurrentMap.ValidPoint(target))
                        continue;

                    var cellObjects = CurrentMap.GetCellObjects(target);
                    if (cellObjects == null)
                        continue;

                    for (int o = 0; o < cellObjects.Count; o++)
                    {
                        MapObject ob = cellObjects[o];
                        if (ob.Race == ObjectType.Monster || ob.Race == ObjectType.Player)
                        {
                            if (!ob.IsAttackTarget(this))
                                continue;

                            if (ob.Attacked(this, damage, DefenceType.MACAgility) > 0 && Envir.Random.Next(8) == 0)
                            {
                                if (Envir.Random.Next(Settings.PoisonResistWeight) >= Target.PoisonResist)
                                {
                                    int poison = GetAttackPower(MinSC, MaxSC);

                                    ob.ApplyPoison(new Poison
                                    {
                                        Owner = this,
                                        Duration = 5,
                                        PType = PoisonType.Green,
                                        Value = poison,
                                        TickSpeed = 2000
                                    }, this);
                                }
                            }
                        }
                        else
                            continue;

                        break;
                    }
                }
            }
       //     Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Direction = Direction, Effect = SpellEffect.HumanThrusting });
        }

        public void PerformRage()
        {
        }
        #endregion

        #region Taoist
        public void PerformSoulFireBall()
        {
            if (Target == null || !Target.IsAttackTarget(this)) return;
            UserMagic magic = GetMagic(Spell.SoulFireBall);
            if (magic == null) return;

            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.SoulFireBall, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });

            int damage = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Ste

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, Target);
            ActionList.Add(action);
        }

        public void PerformPoisoning()
        {
            if (Target == null || !Target.IsAttackTarget(this)) return;
            UserMagic magic = GetMagic(Spell.Poisoning);
            if (magic == null) return;

            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.Poisoning, TargetID = Target.ObjectID, Target = Target.CurrentLocation, Cast = true, Level = 3 });

            int power = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            PoisonType type = Envir.Random.Next(2) == 0 ? PoisonType.Green : PoisonType.Red;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, power, Target, type);
            ActionList.Add(action);
        }

        public void PerformCurse()
        {
            List<MapObject> targets = FindAllTargets(4, Target.CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)            
                if (targets[i].IsAttackTarget(this))                
                    if (Envir.Random.Next(5) == 0)                    
                        targets[i].ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = 5, Owner = this, Value = 10, TickSpeed = 2000 }, Owner = this, true, true);
        }

        public void PerformSoulShield()
        {
            List<MapObject> targets = FindAllTargets(4, CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].Race == ObjectType.Monster &&
                    targets[i].Master == null)
                {
                    targets[i].AddBuff(new Buff { Type = BuffType.SoulShield, Caster = this, ExpireTime = Envir.Time + Settings.Second * 10, ObjectID = targets[i].ObjectID, Values = new int[] { 10 } });
                }
            }
        }

        public void PerformSoulArmour()
        {
            List<MapObject> targets = FindAllTargets(4, CurrentLocation, false);
            for (int i = 0; i < targets.Count; i++)            
                if (targets[i].Race == ObjectType.Monster &&
                    targets[i].Master == null)               
                    targets[i].AddBuff(new Buff { Type = BuffType.BlessedArmour, Caster = this, ExpireTime = Envir.Time + Settings.Second * 10, ObjectID = targets[i].ObjectID, Values = new int[] { 10 } });            
        }
        #endregion

        public override void Spawned()
        {
            base.Spawned();
            Summoned = false;
        }
        public override void Die()
        {
            if (Dead)
                return;

            HP = 0;
            Dead = true;

            for (int i = Pets.Count - 1; i >= 0; i--)
            {
                if (Pets[i].Dead) continue;
                Pets[i].Die();
            }

            DeadTime = Envir.Time + DeadDelay;

            Broadcast(new S.ObjectDied { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = (byte)( Master != null ? 1 : 0 ) });

            if (EXPOwner != null && Master == null && EXPOwner.Race == ObjectType.Player)
                EXPOwner.WinExp(Experience, Info.Level);

            if (Respawn != null)
                Respawn.Count--;

            if (Master == null)
                Drop();

            PoisonList.Clear();
            Envir.MonsterCount--;
            CurrentMap.MonsterCount--;
        }

        protected override void MoveTo(Point location)
        {
            if (CurrentLocation == location) return;

            bool inRange = Functions.InRange(location, CurrentLocation, 1);

            if (inRange)
            {
                if (!CurrentMap.ValidPoint(location)) return;
                var cellObjects = CurrentMap.GetCellObjects(location);
                if (cellObjects != null)
                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject ob = cellObjects[i];
                        if (!ob.Blocking) continue;
                        return;
                    }
            }

            MirDirection dir = Functions.DirectionFromPoint(CurrentLocation, location);

            if (Run(dir)) return;

            switch (Envir.Random.Next(2)) //No favour
            {
                case 0:
                    for (int i = 0; i < 7; i++)
                    {
                        dir = Functions.NextDir(dir);

                        if (Walk(dir))
                            return;
                    }
                    break;
                default:
                    for (int i = 0; i < 7; i++)
                    {
                        dir = Functions.PreviousDir(dir);

                        if (Walk(dir))
                            return;
                    }
                    break;
            }
        }


        public override Packet GetInfo()
        {
            return new S.ObjectPlayer
            {
                ObjectID = ObjectID,
                Name = Name,
                NameColour = NameColour,
                Class = mobsClass,
                Gender = mobsGender,
                Location = CurrentLocation,
                Direction = Direction,
                Hair = hair,
                Weapon = weapon,
                Armour = armour,
                Light = light,
                Poison = CurrentPoison,
                Dead = Dead,
                Hidden = Hidden,
                Effect = SpellEffect.None,
                WingEffect = wing,
                Extra = Summoned,
                TransformType = -1
            };
        }
    }
}