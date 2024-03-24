using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Client.MirScenes;
using Client.MirNetwork;
using S = ServerPackets;
using C = ClientPackets;
using Client.MirMagic;
using Client.MirSounds;
using Client.MirScenes.Dialogs;

namespace Client.MirObjects
{
    public class UserObject : PlayerObject
    {
        public uint Id;

        public ushort HP, MaxHP, MP, MaxMP;

        public ushort MinAC, MaxAC,
                   MinMAC, MaxMAC,
                   MinDC, MaxDC,
                   MinMC, MaxMC,
                   MinSC, MaxSC;

        public short Accuracy, Agility;
        public sbyte ASpeed, Luck;
        public int AttackSpeed;

        public ushort CurrentHandWeight, MaxHandWeight,
                      CurrentWearWeight, MaxWearWeight;
        public ushort CurrentBagWeight, MaxBagWeight;
        public long Experience, MaxExperience;
        public byte LifeOnHit;

        public bool TradeLocked;
        public uint TradeGoldAmount;
        public bool AllowTrade;

        public bool RentalGoldLocked;
        public bool RentalItemLocked;
        public uint RentalGoldAmount;

        public bool HasTeleportRing, HasProtectionRing, HasRevivalRing, HasClearRing,
            HasMuscleRing, HasParalysisRing, HasFireRing, HasHealRing, HasProbeNecklace, HasSkillNecklace, NoDuraLoss,
            HasBlinkSkill;

        public short MagicResist, PoisonResist, HealthRecovery, SpellRecovery, PoisonRecovery, CriticalRate, CriticalDamage, Holy, Freezing, PoisonAttack, HpDrainRate;
        public BaseStats CoreStats = new BaseStats(0);


        public UserItem[] Inventory = new UserItem[46], Equipment = new UserItem[15], Trade = new UserItem[10];
        public int BeltIdx = 6;
        public bool HasExpandedStorage = false;
        public DateTime ExpandedStorageExpiryTime;

        public List<ClientMagic> Magics = new List<ClientMagic>();
        public List<ItemSets> ItemSets = new List<ItemSets>();
        public List<EquipmentSlot> MirSet = new List<EquipmentSlot>();

        public List<ClientIntelligentCreature> IntelligentCreatures = new List<ClientIntelligentCreature>();//IntelligentCreature
        public IntelligentCreatureType SummonedCreatureType = IntelligentCreatureType.None;//IntelligentCreature
        public bool CreatureSummoned;//IntelligentCreature
        public int PearlCount = 0;

        public List<ClientQuestProgress> CurrentQuests = new List<ClientQuestProgress>();
        public List<int> CompletedQuests = new List<int>();
        public List<ClientMail> Mail = new List<ClientMail>();

        public ClientMagic NextMagic;
        public Point NextMagicLocation;
        public MapObject NextMagicObject;
        public MirDirection NextMagicDirection;
        public QueuedAction QueuedAction;

        public bool IsGameMaster;

        public long LastSendAttackTime;
        public long LastSetActionTime;

        public UserObject(uint objectID) : base(objectID)
        {
        }

        public void Load(S.UserInformation info)
        {
            Id = info.RealId;
            Name = info.Name;
            Settings.LoadTrackedQuests(info.Name);
            NameColour = info.NameColour;
            GuildName = info.GuildName;
            GuildRankName = info.GuildRank;
            Class = info.Class;
            Gender = info.Gender;
            Level = info.Level;

            CurrentLocation = info.Location;
            MapLocation = info.Location;
            GameScene.Scene.MapControl.AddObject(this);

            Direction = info.Direction;
            Hair = info.Hair;

            HP = info.HP;
            MP = info.MP;

            Experience = info.Experience;
            MaxExperience = info.MaxExperience;

            LevelEffects = info.LevelEffects;

            Inventory = info.Inventory;
            Equipment = info.Equipment;

            HasExpandedStorage = info.HasExpandedStorage;
            ExpandedStorageExpiryTime = info.ExpandedStorageExpiryTime;

            Magics = info.Magics;
          //  for (int i = 0; i < Magics.Count; i++ )
          //  {
           //     if (Magics[i].CastTime > 0)
          //          Magics[i].CastTime = Magics[i].CastTime;
          //  }

            IntelligentCreatures = info.IntelligentCreatures;//IntelligentCreature
            SummonedCreatureType = info.SummonedCreatureType;//IntelligentCreature
            CreatureSummoned = info.CreatureSummoned;//IntelligentCreature
            HumUp = info.HumUp;

            UpdateAnimCtrl();

            BindAllItems();

            RefreshStats();

            SetAction();
        }

        public int GetMagicDelay(ClientMagic magic)
        {
            long extraDelay = 0;
            if (magic.Spell == Spell.SlashingBurst && Buffs.Any(x => x == BuffType.ImmortalSkin))
                extraDelay = 30000;
            else if (magic.Spell == Spell.Bisul && magic.Level == 4)
                return 300000;

            return (int)(magic.Delay + extraDelay);
        }

        public override void SetLibraries()
        {
            base.SetLibraries();
        }

        public override void SetEffects()
        {
            base.SetEffects();
        }

        public void RefreshStats()
        {
            RefreshLevelStats();
            RefreshBagWeight();
            RefreshEquipmentStats();
            RefreshItemSetStats();
            RefreshMirSetStats();
            RefreshSkills();
            RefreshBuffs();
            RefreshMountStats();
            RefreshGuildBuffs();
            //RefreshFishingStats();

            SetLibraries();
            SetEffects();
            
            if (this == User && Light < 3) Light = 3;
            AttackSpeed = 1400 - ((ASpeed * 40) + Math.Min(370, (Level * 14)));
            if (AttackSpeed < 400) AttackSpeed = 400;

            PercentHealth = (byte)(HP / (float)MaxHP * 100);

            GameScene.Scene.Redraw();
        }
        private void RefreshLevelStats()
        {
            MaxHP = 0; MaxMP = 0;
            MinAC = 0; MaxAC = 0;
            MinMAC = 0; MaxMAC = 0;
            MinDC = 0; MaxDC = 0;
            MinMC = 0; MaxMC = 0;
            MinSC = 0; MaxSC = 0;


            //Other Stats;
            MaxBagWeight = 0;
            MaxWearWeight = 0;
            MaxHandWeight = 0;
            ASpeed = 0;
            Luck = 0;
            Light = 0;
            LifeOnHit = 0;
            HpDrainRate = 0;
            MagicResist = 0;
            PoisonResist = 0;
            HealthRecovery = 0;
            SpellRecovery = 0;
            PoisonRecovery = 0;
            Holy = 0;
            Freezing = 0;
            PoisonAttack = 0;
            
            Accuracy = CoreStats.StartAccuracy;
            Agility = CoreStats.StartAgility;
            CriticalRate = CoreStats.StartCriticalRate;
            CriticalDamage = CoreStats.StartCriticalDamage;

            MinAC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MinAc > 0 ? Level / CoreStats.MinAc : 0);
            MaxAC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MaxAc > 0 ? Level / CoreStats.MaxAc : 0);
            MinMAC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MinMac > 0 ? Level / CoreStats.MinMac : 0);
            MaxMAC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MaxMac > 0 ? Level / CoreStats.MaxMac : 0);
            MinDC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MinDc > 0 ? Level / CoreStats.MinDc : 0);
            MaxDC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MaxDc > 0 ? Level / CoreStats.MaxDc : 0);
            MinMC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MinMc > 0 ? Level / CoreStats.MinMc : 0);
            MaxMC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MaxMc > 0 ? Level / CoreStats.MaxMc : 0);
            MinSC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MinSc > 0 ? Level / CoreStats.MinSc : 0);
            MaxSC = (ushort)Math.Min(ushort.MaxValue, CoreStats.MaxSc > 0 ? Level / CoreStats.MaxSc : 0);
            CriticalRate = (byte)Math.Min(byte.MaxValue, CoreStats.CritialRateGain > 0 ? CriticalRate + (Level / CoreStats.CritialRateGain) : CriticalRate);
            CriticalDamage = (byte)Math.Min(byte.MaxValue, CoreStats.CriticalDamageGain > 0 ? CriticalDamage + (Level / CoreStats.CriticalDamageGain) : CriticalDamage);
            MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, 50 + Level / CoreStats.BagWeightGain * Level);
            MaxWearWeight = (ushort)Math.Min(ushort.MaxValue, 15 + Level / CoreStats.WearWeightGain * Level);
            MaxHandWeight = (ushort)Math.Min(ushort.MaxValue, 12 + Level / CoreStats.HandWeightGain * Level);

            switch (Class)
            {
                case MirClass.Warrior:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 11 + (Level * 3.5F) + (Level * CoreStats.MpGainRate));
                    break;
                case MirClass.Wizard:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 13 + ((Level / 5F + 2F) * 2.2F * Level) + (Level * CoreStats.MpGainRate));
                    break;
                case MirClass.Taoist:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, (13 + Level / 8F * 2.2F * Level) + (Level * CoreStats.MpGainRate));
                    break;
                case MirClass.Assassin:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 12 + ((3 + Level / 12F) * Level) + (Level * CoreStats.MpGainRate));
                    break;
                case MirClass.Archer:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 12 + (5 + Level / 10F) * Level + (Level * CoreStats.MpGainRate));
                    break;
                case MirClass.Monk:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / CoreStats.HpGain + CoreStats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 11 + 3.3F * Level + (Level * CoreStats.MpGainRate));
                    break;
            }
            MaxHealth = MaxHP;
        }

        public bool IsMagicInCD(Spell spell)
        {
            return GetMagicTimeLeft(spell) > 0;
        }

        public int GetMagicTimeLeft(Spell spell)
        {
            ClientMagic magic = GetMagic(spell);
            return GetMagicTimeLeft(magic);
        }

        public int GetMagicTimeLeft(ClientMagic magic)
        { 
            if (magic == null || magic.CastTime <= 0)
                return 0;

            return Math.Max(0, (int)(magic.CastTime + GetMagicDelay(magic) - CMain.Time));
        }

        private void RefreshBagWeight()
        {
            CurrentBagWeight = 0;

            for (int i = 0; i < Inventory.Length; i++)
            {
                UserItem item = Inventory[i];
                if (item != null)
                    CurrentBagWeight = (ushort)Math.Min(ushort.MaxValue, CurrentBagWeight + item.Weight);
            }
        }
        private void RefreshEquipmentStats()
        {
            Weapon = -1;
			WeaponEffect = 0;
			Armour = 0;
            WingEffect = 0;
            MountType = -1;

            CurrentWearWeight = 0;
            CurrentHandWeight = 0;

            HasTeleportRing = false;
            HasProtectionRing = false;
            HasMuscleRing = false;
            HasParalysisRing = false;
            HasProbeNecklace = false;
            HasSkillNecklace = false;
            NoDuraLoss = false;
            FastRun = false;
            short Macrate = 0, Acrate = 0, HPrate = 0, MPrate = 0;

            int WeaponTransform = 0;
            int ArmourTransform = 0;
            LookHumUp = false;
            ItemSets.Clear();
            MirSet.Clear();

            for (int i = 0; i < Equipment.Length; i++)
            {
                UserItem temp = Equipment[i];
                if (temp == null) continue;

                ItemInfo RealItem = Functions.GetRealItem(temp.Info, Level, Class, GameScene.ItemInfoList);

                if (RealItem.Type == ItemType.Weapon || RealItem.Type == ItemType.Torch)
                    CurrentHandWeight = (ushort)Math.Min(ushort.MaxValue, CurrentHandWeight + temp.Weight);
                else
                    CurrentWearWeight = (ushort)Math.Min(ushort.MaxValue, CurrentWearWeight + temp.Weight);

                if (temp.CurrentDura == 0 && RealItem.Durability > 0) continue;

                MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + RealItem.MinAC + temp.Awake.getAC());
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + RealItem.MaxAC + temp.AC + temp.Awake.getAC());
                MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + RealItem.MinMAC + temp.Awake.getMAC());
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + RealItem.MaxMAC + temp.MAC + temp.Awake.getMAC());

                MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + RealItem.MinDC + temp.Awake.getDC());
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + RealItem.MaxDC + temp.DC + temp.Awake.getDC());
                MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + RealItem.MinMC + temp.Awake.getMC());
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + RealItem.MaxMC + temp.MC + temp.Awake.getMC());
                MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + RealItem.MinSC + temp.Awake.getSC());
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + RealItem.MaxSC + temp.SC + temp.Awake.getSC());

                Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + RealItem.Accuracy + temp.Accuracy);
                Agility = (byte)Math.Min(byte.MaxValue, Agility + RealItem.Agility + temp.Agility);

                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + RealItem.HP + temp.HP + temp.Awake.getHPMP());
                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + RealItem.MP + temp.MP + temp.Awake.getHPMP());

                ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + temp.AttackSpeed + RealItem.AttackSpeed)));
                Luck = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, Luck + temp.Luck + RealItem.Luck)));

                MaxBagWeight = (ushort)Math.Max(ushort.MinValue, (Math.Min(ushort.MaxValue, MaxBagWeight + RealItem.BagWeight)));
                MaxWearWeight = (ushort)Math.Max(ushort.MinValue, (Math.Min(ushort.MaxValue, MaxWearWeight + RealItem.WearWeight)));
                MaxHandWeight = (ushort)Math.Max(ushort.MinValue, (Math.Min(ushort.MaxValue, MaxHandWeight + RealItem.HandWeight)));
                HPrate = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, HPrate + RealItem.HPrate));
                MPrate = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, MPrate + RealItem.MPrate));
                Acrate = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, Acrate + RealItem.MaxAcRate));
                Macrate = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, Macrate + RealItem.MaxMacRate));
                MagicResist = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, MagicResist + temp.MagicResist + RealItem.MagicResist)));
                PoisonResist = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, PoisonResist + temp.PoisonResist + RealItem.PoisonResist)));
                HealthRecovery = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, HealthRecovery + temp.HealthRecovery + RealItem.HealthRecovery)));
                SpellRecovery = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, SpellRecovery + temp.ManaRecovery + RealItem.SpellRecovery)));
                PoisonRecovery = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, PoisonRecovery + temp.PoisonRecovery + RealItem.PoisonRecovery)));
                CriticalRate = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, CriticalRate + temp.CriticalRate + RealItem.CriticalRate)));
                CriticalDamage = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, CriticalDamage + temp.CriticalDamage + RealItem.CriticalDamage)));
                Holy = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, Holy + RealItem.Holy)));
                Freezing = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, Freezing + temp.Freezing + RealItem.Freezing)));
                PoisonAttack = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, PoisonAttack + temp.PoisonAttack + RealItem.PoisonAttack)));
                HpDrainRate = (byte)Math.Max(100, Math.Min(byte.MaxValue, HpDrainRate + RealItem.HpDrainRate));

                for (int x=0; x< temp.RuneSlots.Length; ++x)
                {
                    if (temp.RuneSlots[x] == null)
                        continue;

                    RefreshRuneStats(temp.RuneSlots[x].SocketedItem);
                }

                if (RealItem.Light > Light) Light = RealItem.Light;
                if (RealItem.Unique != SpecialItemMode.None)
                {
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Paralize)) HasParalysisRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Teleport)) HasTeleportRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Clearring)) HasClearRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Protection)) HasProtectionRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Revival)) HasRevivalRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Muscle)) HasMuscleRing = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Probe)) HasProbeNecklace = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Skill)) HasSkillNecklace = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.NoDuraLoss)) NoDuraLoss = true;
                }

                if (RealItem.CanFastRun)
                {
                    FastRun = true;
                }

                if (RealItem.Type == ItemType.Armour)
                {
                    ArmourTransform = temp.Transform;
                    Armour = RealItem.Shape;
                    WingEffect = RealItem.Effect;
                    if (RealItem.NeedHumUp)
                        LookHumUp = true;
                }
				if (RealItem.Type == ItemType.Weapon)
				{
                    WeaponTransform = temp.Transform;
					Weapon = RealItem.Shape;
					WeaponEffect = RealItem.Effect;
                    if (RealItem.NeedHumUp)
                        LookHumUp = true;
                }

				if (RealItem.Type == ItemType.Mount)
                    MountType = RealItem.Shape;

                if (RealItem.Set == ItemSet.None) continue;

                ItemSets itemSet = ItemSets.Where(set => set.Set == RealItem.Set && !set.Type.Contains(RealItem.Type) && !set.SetComplete).FirstOrDefault();

                if (itemSet != null)
                {
                    itemSet.Type.Add(RealItem.Type);
                    itemSet.Count++;
                }
                else
                {
                    ItemSets.Add(new ItemSets { Count = 1, Set = RealItem.Set, Type = new List<ItemType> { RealItem.Type } });
                }

                //Mir Set
                if (RealItem.Set == ItemSet.Mir)
                {
                    if (!MirSet.Contains((EquipmentSlot)i))
                        MirSet.Add((EquipmentSlot)i);
                }
            }

            MaxHP = (ushort)Math.Min(ushort.MaxValue, (((double)HPrate / 100) + 1) * MaxHP);
            MaxMP = (ushort)Math.Min(ushort.MaxValue, (((double)MPrate / 100) + 1) * MaxMP);
            MaxAC = (ushort)Math.Min(ushort.MaxValue, (((double)Acrate / 100) + 1) * MaxAC);
            MaxMAC = (ushort)Math.Min(ushort.MaxValue, (((double)Macrate / 100) + 1) * MaxMAC);

            if (HasMuscleRing)
            {
                MaxBagWeight = (ushort)(MaxBagWeight * 2);
                MaxWearWeight = Math.Min(ushort.MaxValue, (ushort)(MaxWearWeight * 2));
                MaxHandWeight = Math.Min(ushort.MaxValue, (ushort)(MaxHandWeight * 2));
            }

            if (WeaponTransform > 0 && ArmourTransform > 0)
            {
                LookHumUp = true;
            }

            if (LookHumUp)
            {
                if (WeaponTransform > 0)
                    Weapon = WeaponTransform;

                if (ArmourTransform > 0)
                    Armour = ArmourTransform;
            }
        }

        private void RefreshRuneStats(UserItem item)
        {

        }

        private void RefreshItemSetStats()
        {
            foreach (var s in ItemSets)
            {
                if ((s.Set == ItemSet.Smash) && (s.Type.Contains(ItemType.Ring)) && (s.Type.Contains(ItemType.Bracelet)))
                    ASpeed = (sbyte)Math.Min(sbyte.MaxValue, ASpeed + 2);
                if ((s.Set == ItemSet.Purity) && (s.Type.Contains(ItemType.Ring)) && (s.Type.Contains(ItemType.Bracelet)))
                    Holy = Math.Min(byte.MaxValue, (byte)(Holy + 3));
                if ((s.Set == ItemSet.HwanDevil) && (s.Type.Contains(ItemType.Ring)) && (s.Type.Contains(ItemType.Bracelet)))
                {
                    MaxWearWeight = (ushort)Math.Min(ushort.MaxValue, MaxWearWeight + 5);
                    MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 20);
                }

                if (!s.SetComplete) continue;
                switch (s.Set)
                {
                    case ItemSet.Mundane:
                        CriticalRate = (short)Math.Min(short.MaxValue, CriticalRate + 2);
                        break;
                    case ItemSet.NokChi:
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 50);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 5);
                        break;
                    case ItemSet.TaoProtect:
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 30);
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 30);
                       // Reflect = (byte)Math.Min(byte.MaxValue, Reflect + 5);
                        break;
                    case ItemSet.RedOrchid:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + 2);
                        HpDrainRate = (byte)Math.Min(byte.MaxValue, HpDrainRate + 10);
                        break;
                    case ItemSet.RedFlower:
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 50);
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP - 25);
                        break;
                    case ItemSet.Smash:
                        MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 1);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 3);
                        ASpeed = (sbyte)Math.Min(sbyte.MaxValue, ASpeed + 2);
                        break;
                    case ItemSet.HwanDevil:
                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 1);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 2);
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 15);
                        break;
                    case ItemSet.Purity:
                        MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 1);
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 2);
                        Holy = (byte)Math.Min(ushort.MaxValue, Holy + 3);
                        break;
                    case ItemSet.FiveString:
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + ((MaxHP / 100) * 30));
                        MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 2);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 2);
                        break;
                    case ItemSet.Spirit:
                        MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 2);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 5);
                        ASpeed = (sbyte)Math.Min(sbyte.MaxValue, ASpeed + 2);
                        break;
                    case ItemSet.Bone:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 2);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                        break;
                    case ItemSet.Bug:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                        PoisonResist = (byte)Math.Min(byte.MaxValue, PoisonResist + 1);
                        break;
                    case ItemSet.WhiteGold:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 2);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 2);
                        break;
                    case ItemSet.WhiteGoldH:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 3);
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 30);
                        ASpeed = (sbyte)Math.Min(sbyte.MaxValue, ASpeed + 2);
                        break;
                    case ItemSet.RedJade:
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 2);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 2);
                        break;
                    case ItemSet.RedJadeH:
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 2);
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 40);
                        Agility = (byte)Math.Min(byte.MaxValue, Agility + 2);
                        break;
                    case ItemSet.Nephrite:
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 2);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                        break;
                    case ItemSet.NephriteH:
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 2);
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 15);
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 20);
                        Holy = (byte)Math.Min(byte.MaxValue, Holy + 1);
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + 1);
                        break;
                    case ItemSet.Whisker1:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 25);
                        break;
                    case ItemSet.Whisker2:
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 17);
                        break;
                    case ItemSet.Whisker3:
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 17);
                        break;
                    case ItemSet.Whisker4:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 20);
                        break;
                    case ItemSet.Whisker5:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 17);
                        break;
                    case ItemSet.Hyeolryong:
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 2);
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 15);
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 20);
                        Holy = (byte)Math.Min(byte.MaxValue, Holy + 1);
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + 1);
                        break;
                    case ItemSet.Monitor:
                        MagicResist = (byte)Math.Min(byte.MaxValue, MagicResist + 1);
                        PoisonResist = (byte)Math.Min(byte.MaxValue, PoisonResist + 1);
                        break;
                    case ItemSet.Oppressive:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
                        Agility = (byte)Math.Min(byte.MaxValue, Agility + 1);
                        break;
                }
            }
        }

        private void RefreshMirSetStats()
        {
            if (MirSet.Count() == 10)
            {
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 70);
                Luck = (sbyte)Math.Min(sbyte.MaxValue, Luck + 2);
                ASpeed = (sbyte)Math.Min(sbyte.MaxValue, ASpeed + 2);
                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + 70);
                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + 80);
                MagicResist = (byte)Math.Min(byte.MaxValue, MagicResist + 6);
                PoisonResist = (byte)Math.Min(byte.MaxValue, PoisonResist + 6);
            }

            if (MirSet.Contains(EquipmentSlot.RingL) && MirSet.Contains(EquipmentSlot.RingR))
            {
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
            }
            if (MirSet.Contains(EquipmentSlot.BraceletL) && MirSet.Contains(EquipmentSlot.BraceletR))
            {
                MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 1);
                MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 1);
            }
            if ((MirSet.Contains(EquipmentSlot.RingL) | MirSet.Contains(EquipmentSlot.RingR)) && (MirSet.Contains(EquipmentSlot.BraceletL) | MirSet.Contains(EquipmentSlot.BraceletR)) && MirSet.Contains(EquipmentSlot.Necklace))
            {
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
                MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 30);
                MaxWearWeight = (ushort)Math.Min(ushort.MaxValue, MaxWearWeight + 17);
            }
            if (MirSet.Contains(EquipmentSlot.RingL) && MirSet.Contains(EquipmentSlot.RingR) && MirSet.Contains(EquipmentSlot.BraceletL) && MirSet.Contains(EquipmentSlot.BraceletR) && MirSet.Contains(EquipmentSlot.Necklace))
            {
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 1);
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 1);
                MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + 20);
                MaxWearWeight = (ushort)Math.Min(ushort.MaxValue, MaxWearWeight + 10);
            }
            if (MirSet.Contains(EquipmentSlot.Armour) && MirSet.Contains(EquipmentSlot.Helmet) && MirSet.Contains(EquipmentSlot.Weapon))
            {
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 2);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                Agility = (byte)Math.Min(byte.MaxValue, Agility + 1);
            }
            if (MirSet.Contains(EquipmentSlot.Armour) && MirSet.Contains(EquipmentSlot.Boots) && MirSet.Contains(EquipmentSlot.Belt))
            {
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                MaxHandWeight = (ushort)Math.Min(ushort.MaxValue, MaxHandWeight + 17);
            }
            if (MirSet.Contains(EquipmentSlot.Armour) && MirSet.Contains(EquipmentSlot.Boots) && MirSet.Contains(EquipmentSlot.Belt) && MirSet.Contains(EquipmentSlot.Helmet) && MirSet.Contains(EquipmentSlot.Weapon))
            {
                MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 1);
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 1);
                MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 1);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 1);
                MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 1);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 1);
                MaxHandWeight = (ushort)Math.Min(ushort.MaxValue, MaxHandWeight + 17);
            }
        }

        private void RefreshSkills()
        {
            for (int i = 0; i < Magics.Count; i++)
            {
                ClientMagic magic = Magics[i];
                switch (magic.Spell)
                {
                    case Spell.Fencing:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level * 3 + 3);
                        break;
                    case Spell.Slaying:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level);
                        break;
                    case Spell.FatalSword:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level + 1);
                    //    Agility = (byte)Math.Min(ushort.MaxValue, Agility + magic.Level + 1);
                        break;
                    case Spell.Focus:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level + 1);
                        break;
                    case Spell.SpiritSword:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level + 1);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + MaxSC * (magic.Level + 1) * 0.1F);
                        break;
                    case Spell.JiBenGunFa:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level * 2 + 2);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + (magic.Level + 1) * 1);
                        break;
                }
            }
        }
        private void RefreshBuffs()
        {
            IsGameMaster = false;
            NoMove = false;
            Hidden2 = false;

            for (int i = 0; i < GameScene.Scene.Buffs.Count; i++)
            {
                Buff buff = GameScene.Scene.Buffs[i];

                switch (buff.Type)
                {
                    case BuffType.Haste:
                    case BuffType.Fury:
                        ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + buff.Values[0])));
                        break;
                    case BuffType.ImmortalSkin:
                        break;
                    case BuffType.DogYoLin7:
                        NoMove = true;
                        break;
                    case BuffType.MoonMist:
                        Hidden2 = true;
                        break;
                    case BuffType.SwiftFeet:
                        Sprint = true;
                        break;
                    case BuffType.LightBody:
                        Agility = (byte)Math.Min(ushort.MaxValue, Agility + buff.Values[0]);
                        break;
                    case BuffType.SoulShield:
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        break;
                    case BuffType.BlessedArmour:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        break;
                    case BuffType.UltimateEnhancer:
                        if (Class == MirClass.Wizard || Class == MirClass.Archer)
                        {
                            MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[0]);
                        }
                        else if (Class == MirClass.Taoist)
                        {
                            MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + buff.Values[0]);
                        }
                        else
                        {
                            MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[0]);
                        }
                        break;
                    case BuffType.ProtectionField:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        break;
                    case BuffType.ProtectionField1:
                        if (buff.Values.Length < 1) return;
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        break;
                    case BuffType.Rage:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[0]);
                        break;
                    case BuffType.CounterAttack:
                        //ReflectRate = (byte)Math.Min(byte.MaxValue, ReflectRate + 50);
                     //   MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[0]);
                     //   MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + buff.Values[0]);
                        break;
                    case BuffType.Curse:
                        ushort rMaxDC = (ushort)(((int)MaxDC / 100) * buff.Values[0]);
                        ushort rMaxMC = (ushort)(((int)MaxMC / 100) * buff.Values[0]);
                        ushort rMaxSC = (ushort)(((int)MaxSC / 100) * buff.Values[0]);
                        byte rASpeed = (byte)(((int)ASpeed / 100) * buff.Values[0]);

                        MaxDC = (ushort)Math.Max(ushort.MinValue, MaxDC - rMaxDC);
                        MaxMC = (ushort)Math.Max(ushort.MinValue, MaxMC - rMaxMC);
                        MaxSC = (ushort)Math.Max(ushort.MinValue, MaxSC - rMaxSC);
                        ASpeed = (sbyte)Math.Min(sbyte.MaxValue, (Math.Max(sbyte.MinValue, ASpeed - rASpeed)));
                        break;

                    case BuffType.HumUp://stupple
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxHP + buff.Values[0]));
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxMP + buff.Values[1]));
                        HealthRecovery = (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, HealthRecovery + buff.Values[2]));
                        SpellRecovery = (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, SpellRecovery + buff.Values[3]));
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxBagWeight + buff.Values[4]));
                        break;
                    case BuffType.MagicBooster:
                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + buff.Values[0]);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[0]);
                        break;

                    case BuffType.Knapsack:
                    case BuffType.BagWeight:
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, MaxBagWeight + buff.Values[0]);
                        break;

                    case BuffType.Impact:
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[0]);
                        break;
                    case BuffType.Magic:
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[0]);
                        break;
                    case BuffType.Taoist:
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + buff.Values[0]);
                        break;
                    case BuffType.Storm:
                        ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + buff.Values[0])));
                        break;
                    case BuffType.HealthAid:
                    case BuffType.Healing2:
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + buff.Values[0]);
                        break;
                    case BuffType.ManaAid:
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + buff.Values[0]);
                        break;
                    case BuffType.Defence:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        break;
                    case BuffType.MagicDefence:
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        break;
                    case BuffType.GameMaster:
                        IsGameMaster = true;
                        break;
                    case BuffType.Luck:
                        Luck = (sbyte)Math.Min(sbyte.MaxValue, Luck + buff.Values[0]);
                        MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + buff.Values[1]);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[1]);

                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + buff.Values[2]);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[2]);

                        MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + buff.Values[3]);
                        MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + buff.Values[3]);

                        MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[4]);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[4]);

                        if (buff.Values.Length > 5)
                            Agility = (short)Math.Min(short.MaxValue, Agility + buff.Values[5]);
                        if (buff.Values.Length > 6)
                            Accuracy = (short)Math.Min(short.MaxValue, Accuracy + buff.Values[6]);
                        if (buff.Values.Length > 7)
                            MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + buff.Values[7]);
                        break;

                    case BuffType.CounterAttack1:
                        MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[0]);
                        MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + buff.Values[0]);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        //ReflectRate = (byte)Math.Min(byte.MaxValue, ReflectRate + buff.Values[1]);
                        break;

                    case BuffType.WonderDrug:
                        switch (buff.Values[0])
                        {
                            case 2:
                                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + buff.Values[1]);
                                break;
                            case 3:
                                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + buff.Values[1]);
                                break;
                            case 4:
                                MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[1]);
                                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[1]);
                                break;
                            case 5:
                                MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + buff.Values[1]);
                                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[1]);
                                break;
                            case 6:
                                ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + buff.Values[1])));
                                break;
                        }
                        break;
                }

            }
        }

        public void RefreshMountStats()
        {
            UserItem MountItem = Equipment[(int)EquipmentSlot.Mount];

            if (MountItem == null) return;

            UserItem[] Slots = MountItem.Slots;

            for (int i = 0; i < Slots.Length; i++)
            {
                UserItem temp = Slots[i];
                if (temp == null) continue;

                ItemInfo RealItem = Functions.GetRealItem(temp.Info, Level, Class, GameScene.ItemInfoList);

                CurrentWearWeight = (ushort)Math.Min(ushort.MaxValue, CurrentWearWeight + temp.Weight);

                if (temp.CurrentDura == 0 && temp.Info.Durability > 0) continue;

                MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + RealItem.MinAC);
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + RealItem.MaxAC + temp.AC);
                MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + RealItem.MinMAC);
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + RealItem.MaxMAC + temp.MAC);

                MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + RealItem.MinDC);
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + RealItem.MaxDC + temp.DC);
                MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + RealItem.MinMC);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + RealItem.MaxMC + temp.MC);
                MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + RealItem.MinSC);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + RealItem.MaxSC + temp.SC);

                Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + RealItem.Accuracy + temp.Accuracy);
                Agility = (byte)Math.Min(byte.MaxValue, Agility + RealItem.Agility + temp.Agility);

                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + RealItem.HP + temp.HP);
                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + RealItem.MP + temp.MP);

                ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + temp.AttackSpeed + RealItem.AttackSpeed)));
                Luck = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, Luck + temp.Luck + RealItem.Luck)));
            }
        }

        public void RefreshGuildBuffs()
        {
            if (User != this) return;
            if (GameScene.Scene.GuildDialog == null) return;
            for (int i = 0; i < GameScene.Scene.GuildDialog.EnabledBuffs.Count; i++)
            {
                GuildBuff Buff = GameScene.Scene.GuildDialog.EnabledBuffs[i];
                if (Buff == null) continue;
                if (!Buff.Active) continue;
                if (Buff.Info == null)
                Buff.Info = GameScene.Scene.GuildDialog.FindGuildBuffInfo(Buff.Id);
                if (Buff.Info == null) continue;
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + Buff.Info.BuffAc);
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + Buff.Info.BuffMac);
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + Buff.Info.BuffDc);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + Buff.Info.BuffMc);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + Buff.Info.BuffSc);
                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + Buff.Info.BuffMaxHp);
                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + Buff.Info.BuffMaxMp);
                HealthRecovery = (byte)Math.Min(byte.MaxValue, HealthRecovery + Buff.Info.BuffHpRegen);
                SpellRecovery = (byte)Math.Min(byte.MaxValue, SpellRecovery + Buff.Info.BuffMPRegen);
            }
        }

        public void BindAllItems()
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] == null) continue;
                GameScene.Bind(Inventory[i]);
            }

            for (int i = 0; i < Equipment.Length; i++)
            {
                if (Equipment[i] == null) continue;
                GameScene.Bind(Equipment[i]);
            }
        }


        public ClientMagic GetMagic(Spell spell)
        {
            for (int i = 0; i < Magics.Count; i++)
            {
                ClientMagic magic = Magics[i];
                if (magic.Spell != spell) continue;
                return magic;
            }

            return null;
        }


        public void GetMaxGain(UserItem item)
        {
            if (CurrentBagWeight + item.Weight <= MaxBagWeight && FreeSpace(Inventory) > 0) return;

            uint min = 0;
            uint max = item.Count;

            if (CurrentBagWeight >= MaxBagWeight)
            {

            }

            if (item.Info.Type == ItemType.Amulet)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    UserItem bagItem = Inventory[i];

                    if (bagItem == null || bagItem.Info != item.Info) continue;

                    if (bagItem.Count + item.Count <= bagItem.Info.StackSize)
                    {
                        item.Count = max;
                        return;
                    }
                    item.Count = bagItem.Info.StackSize - bagItem.Count;
                    min += item.Count;
                    if (min >= max)
                    {
                        item.Count = max;
                        return;
                    }
                }

                if (min == 0)
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr(FreeSpace(Inventory) == 0 ? "You do not have enough space." : "You do not have enough weight.", ChatType.System);

                    item.Count = 0;
                    return;
                }

                item.Count = min;
                return;
            }

            if (CurrentBagWeight + item.Weight > MaxBagWeight)
            {
                item.Count = (uint)(Math.Max((MaxBagWeight - CurrentBagWeight), uint.MinValue) / item.Info.Weight);
                max = item.Count;
                if (item.Count == 0)
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough weight.", ChatType.System);
                    return;
                }
            }

            if (item.Info.StackSize > 1)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    UserItem bagItem = Inventory[i];

                    if (bagItem == null) return;
                    if (bagItem.Info != item.Info) continue;

                    if (bagItem.Count + item.Count <= bagItem.Info.StackSize)
                    {
                        item.Count = max;
                        return;
                    }

                    item.Count = bagItem.Info.StackSize - bagItem.Count;
                    min += item.Count;
                    if (min >= max)
                    {
                        item.Count = max;
                        return;
                    }
                }

                if (min == 0)
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough space.", ChatType.System);
                    item.Count = 0;
                }
            }
            else
            {
                GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough space.", ChatType.System);
                item.Count = 0;
            }

        }
        private int FreeSpace(UserItem[] array)
        {
            int count = 0;
            for (int i = 0; i < array.Length; i++)
                count++;
            return count;
        }

        public override void SetAction()
        {
            if (QueuedAction != null )
            {
                if ((ActionFeed.Count == 0) || (ActionFeed.Count == 1 && NextAction.Action == MirAction.Stance))
                {
                    ActionFeed.Clear();
                    ActionFeed.Add(QueuedAction);
                    QueuedAction = null;
                }
            }

            base.SetAction();
        }
        public override void ProcessFrames()
        {
            bool clear = CMain.Time >= NextMotion;

            base.ProcessFrames();

            if (clear)
            {
                QueuedAction = null;
            }

            if ((CurrentAction == MirAction.Standing
                || CurrentAction == MirAction.MountStanding
                || CurrentAction == MirAction.Stance
                || CurrentAction == MirAction.Stance2
                || CurrentAction == MirAction.DashFail)
                && (QueuedAction != null
                || NextAction != null))
                SetAction();
         //   else if (QueuedAction != null)
        //        CMain.SaveDebug(string.Format("动作存在，没有执行 {0}", CurrentAction));
        }

        public void ClearMagic()
        {
            NextMagic = null;
            NextMagicDirection = 0;
            NextMagicLocation = Point.Empty;
            NextMagicObject = null;
        }

        public override void DrawHealthNum()
        {
            if (!Settings.ShowHealth)
                return;

            CreateHealthNum();

            if (HealthBarLabel == null || HealthBarLabel.IsDisposed || Dead) return;

            if (Level > 0 && Settings.ShowLevel)
                HealthBarLabel.Text = String.Format("{0}/{1}/Z{2}", HP, MaxHP, Level);
            else
                HealthBarLabel.Text = String.Format("{0}/{1}", HP, MaxHP);

            HealthBarLabel.Location = new Point(DisplayRectangle.X + (48 - HealthBarLabel.Size.Width) / 2, DisplayRectangle.Y - (65 + HealthBarLabel.Size.Height) - (Dead ? 35 : 0));
            HealthBarLabel.Draw();
        }

        public UserItem GetPoison(int count, byte shape = 0)
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                UserItem item = Inventory[i];
                if (item != null && item.Info.Type == ItemType.Amulet && item.Count >= count)
                {
                    if (shape == 0)
                    {
                        if (item.Info.Shape == 1 || item.Info.Shape == 2)
                            return item;
                    }
                    else
                    {
                        if (item.Info.Shape == shape)
                            return item;
                    }
                }
            }

            return null;
        }

        public UserItem GetAmulet(int count, int shape = 0)
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                UserItem item = Inventory[i];
                if (item != null && item.Info.Type == ItemType.Amulet && item.Info.Shape == shape && item.Count >= count)
                    return item;
            }

            return null;
        }

        public bool IsMagicToggle(Spell spell)
        {
            ClientMagic clientMagic = GetMagic(spell);
            return clientMagic != null && clientMagic.Toggle;
        }

        public void CloseSpell(Spell spell)
        {
            ClientMagic clientMagic = GetMagic(spell);
            if (clientMagic != null)
                clientMagic.Toggle = false;
        }

        public bool CanRun(MirDirection dir)
        {
            if (InTrapRock) return false;
            if (CurrentBagWeight > MaxBagWeight) return false;

            MapControl MapControl = GameScene.Scene.MapControl;
            if (CanWalk(dir) && MapControl.EmptyCell(Functions.PointMove(CurrentLocation, dir, 2)))
            {
                if (RidingMount || Sprint && !Sneaking)
                {
                    return MapControl.EmptyCell(Functions.PointMove(CurrentLocation, dir, 3));
                }

                return true;
            }

            return false;
        }

        public bool CanWalk(MirDirection dir)
        {
            return GameScene.Scene.MapControl.EmptyCell(Functions.PointMove(CurrentLocation, dir, 1)) && !InTrapRock && !NoMove;
        }

        public bool CanRideAttack()
        {
            if (RidingMount)
            {
                UserItem item = Equipment[(int)EquipmentSlot.Mount];
                if (item == null || item.Slots.Length < 4 || item.Slots[(int)MountSlot.Bells] == null) return false;
            }

            return true;
        }

        public bool CheckMagicMP(ClientMagic magic)
        {
            return magic.BaseCost + magic.LevelCost * magic.Level <= MP;
        }

        protected override void AutoSpell()
        {
            Spell = Spell.None;
            ClientMagic magic;
            if (!RidingMount)
            {
                if (IsMagicToggle(Spell.Slaying) && TargetObject != null)
                    Spell = Spell.Slaying;

                if (IsMagicToggle(Spell.Thrusting))// && GameScene.Scene.MapControl.HasTarget(Functions.PointMove(CurrentLocation, Direction, 2)))
                    Spell = Spell.Thrusting;

                if (IsMagicToggle(Spell.HalfMoon))
                {
                    if (TargetObject != null && GameScene.Scene.MapControl.CanHalfMoon(CurrentLocation, Direction))
                    {
                        magic = GetMagic(Spell.HalfMoon);
                        if (magic != null && CheckMagicMP(magic))
                            Spell = Spell.HalfMoon;
                    }
                }

                if (IsMagicToggle(Spell.CrossHalfMoon))
                {
                    if (TargetObject != null && GameScene.Scene.MapControl.CanCrossHalfMoon(CurrentLocation))
                    {
                        magic = GetMagic(Spell.CrossHalfMoon);
                        if (magic != null && CheckMagicMP(magic))
                            Spell = Spell.CrossHalfMoon;
                    }
                }

                if (IsMagicToggle(Spell.DoubleSlash))
                {
                    magic = GetMagic(Spell.DoubleSlash);
                    if (magic != null && CheckMagicMP(magic))
                        Spell = Spell.DoubleSlash;
                }


                if (IsMagicToggle(Spell.TwinDrakeBlade) && TargetObject != null)
                {
                    magic = GetMagic(Spell.TwinDrakeBlade);
                    if (magic != null && CheckMagicMP(magic))
                        Spell = Spell.TwinDrakeBlade;
                }

                if (IsMagicToggle(Spell.FlamingSword))
                {
                    if (TargetObject != null && Functions.InRange(CurrentLocation, TargetObject.CurrentLocation, 1))
                    {
                        magic = GetMagic(Spell.FlamingSword);
                        if (magic != null)
                            Spell = Spell.FlamingSword;
                    }
                }

                if (IsMagicToggle(Spell.LuoHanGunFa))
                {
                    if (GameScene.Scene.MapControl.HasTargetDir(CurrentLocation, Direction, 3))
                    {
                        CurrentMagic = MagicMgr.Get(Spell.LuoHanGunFa);
                        Spell = Spell.LuoHanGunFa;
                    }
                }

                if (IsMagicToggle(Spell.DaMoGunFa))
                {
                    if (TargetObject != null)
                    {
                        magic = GetMagic(Spell.DaMoGunFa);
                        if (magic != null)
                        {
                            Spell = Spell.DaMoGunFa;
                            CurrentMagic = MagicMgr.Get(Spell.DaMoGunFa);
                        }
                    }
                }
            }

            Network.Enqueue(new C.Attack { Direction = Direction, Spell = Spell });

            if (Spell == Spell.Slaying || Spell == Spell.FlamingSword || Spell == Spell.DaMoGunFa || Spell == Spell.TwinDrakeBlade)
                CloseSpell(Spell);

            if (Spell == Spell.FlamingSword)
                GameScene.NextTimeFireHit = false;
            else if (Spell == Spell.DaMoGunFa)
                GameScene.NextTimeDaMoHit = false;

            magic = GetMagic(Spell);

            if (magic != null) SpellLevel = magic.Level;

            GameScene.AttackTime = CMain.Time + AttackSpeed;
            MapControl.NextAction = CMain.Time + 2500;// 2500;

#if DEBUG 
         //   CMain.SaveDebug(string.Format("发送攻击消息完毕  {0} {1} {2} 攻击间隔 {3} {4}", CMain.Time, GameScene.AttackTime, MapControl.NextAction, CMain.Time - LastSendAttackTime, AttackSpeed));
#endif
            LastSendAttackTime = CMain.Time;
        }
    }
}

