﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Server.MirDatabase;
using Server.MirEnvir;
using Server.MirNetwork;
using S = ServerPackets;
using Server.MirObjects.Monsters;
using System.IO;

namespace Server.MirObjects
{
    public sealed class PlayerObject : MapObject
    {
        public string GMPassword = Settings.GMPassword;
        public bool IsGM, GMNeverDie, GMGameMaster, EnableGroupRecall, EnableGuildInvite, AllowMarriage, AllowLoverRecall, AllowMentor, HasMapShout, HasServerShout;

        public bool HasUpdatedBaseStats = true;

        public long LastRecallTime, LastRevivalTime, LastTeleportTime, LastProbeTime, MenteeEXP; // LastProbeTime 探测

        public short Looks_Armour = 0, Looks_Weapon = -1, Looks_WeaponEffect = 0;
        public byte Looks_Wings = 0;

        public bool WarZone = false;

        public override ObjectType Race
        {
            get { return ObjectType.Player; }
        }

        public CharacterInfo Info;
        public AccountInfo Account;
        public MirConnection Connection;
        public Reporting Report;
        public CommandMgr CommandMgr;

        public override string Name
        {
            get { return Info.Name; }
            set { /*Check if Name exists.*/ }
        }

        public override int CurrentMapIndex
        {
            get { return Info.CurrentMapIndex; }
            set { Info.CurrentMapIndex = value; }
        }
        public override Point CurrentLocation
        {
            get { return Info.CurrentLocation; }
            set { Info.CurrentLocation = value; }
        }
        public override MirDirection Direction
        {
            get { return Info.Direction; }
            set { Info.Direction = value; }
        }
        public override ushort Level
        {
            get { return Info.Level; }
            set { Info.Level = value; }
        }

        public override uint Health
        {
            get { return HP; }
        }
        public override uint MaxHealth
        {
            get { return MaxHP; }
        }

        public ushort HP
        {
            get { return Info.HP; }
            set { Info.HP = value; }
        }
        public ushort MP
        {
            get { return Info.MP; }
            set { Info.MP = value; }
        }

        public ushort MaxHP, MaxMP;

        public override AttackMode AMode
        {
            get { return Info.AMode; }
            set { Info.AMode = value; }
        }
        public override PetMode PMode
        {
            get { return Info.PMode; }
            set { Info.PMode = value; }
        }

        public long Experience
        {
            set { Info.Experience = value; }
            get { return Info.Experience; }
        }
        public long MaxExperience;
        public byte LifeOnHit;
        public byte HpDrainRate;
        public float HpDrain = 0;

        public float ExpRateOffset = 0;

        public bool NewMail = false;

        public override int PKPoints
        {
            get { return Info.PKPoints; }
            set { Info.PKPoints = value; }
        }

        public byte Hair
        {
            get { return Info.Hair; }
            set { Info.Hair = value; }
        }

        internal byte GetRegionID()
        {
            return Account.RegionID;
        }

        public MirClass Class
        {
            get { return Info.Class; }
        }
        public MirGender Gender
        { get { return Info.Gender; } }

        public int BindMapIndex
        {
            get { return Info.BindMapIndex; }
            set { Info.BindMapIndex = value; }
        }
        public Point BindLocation
        {
            get { return Info.BindLocation; }
            set { Info.BindLocation = value; }
        }

        public bool RidingMount;
        public MountInfo Mount
        {
            get { return Info.Mount; }
        }
        public short MountType
        {
            get { return Mount.MountType; }
            set { Mount.MountType = value; }
        }

        public short TransformType;

        public int FishingChance, FishingChanceCounter, FishingProgressMax, FishingProgress, FishingAutoReelChance = 0, FishingNibbleChance = 0;
        public bool Fishing, FishingAutocast, FishFound, FishFirstFound;
        public bool NoMove;

        public bool CanMove
        {
            get { return !Dead && Envir.Time >= ActionTime && !Fishing && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen) && !NoMove; }
        }
        public bool CanWalk
        {
            get { return !Dead && Envir.Time >= ActionTime && !InTrapRock && !Fishing && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen); }
        }
        public bool CanRun
        {
            get { return !Dead && Envir.Time >= ActionTime && (_stepCounter > 0 || FastRun) && (!Sneaking || ActiveSwiftFeet) && CurrentBagWeight <= MaxBagWeight && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen); }
        }
        public bool CanAttack
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && Envir.Time >= AttackTime && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen) && Mount.CanAttack && !Fishing;
            }
        }

        public bool CanRegen
        {
            get { return Envir.Time >= RegenTime && _runCounter == 0; }
        }
        private bool CanCast
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && Envir.Time >= SpellTime && !CurrentPoison.HasFlag(PoisonType.Stun) &&
                    !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.Frozen) && Mount.CanAttack && !Fishing;
            }
        }

        public const long TurnDelay = 350, MoveDelay = 600, HarvestDelay = 350, RegenDelay = 10000, PotDelay = 250, HealDelay = 600, DuraDelay = 10000, VampDelay = 500, LoyaltyDelay = 1000, FishingCastDelay = 750, FishingDelay = 200, CreatureTimeLeftDelay = 1000, ItemExpireDelay = 60000, MovementDelay = 2000;
        public long ActionTime, RunTime, RegenTime, PotTime, HealTime, AttackTime, StruckTime, TorchTime, DuraTime, DecreaseLoyaltyTime, IncreaseLoyaltyTime, ChatTime, ShoutTime, SpellTime, VampTime, SearchTime, FishingTime, LogTime, FishingFoundTime, CreatureTimeLeftTicker, StackingTime, ItemExpireTime, RestedTime, MovementTime;

        public byte ChatTick;

        public bool ElementalBarrier;
        public byte ElementalBarrierLv;
        public long ElementalBarrierTime;

        public bool ElementalBarrier1;
        public byte ElementalBarrierLv1;
        public long ElementalBarrierTime1;

        public bool HasElemental;
        public int ElementsLevel;

        private bool _concentrating;
        public bool Concentrating
        {
            get
            {
                return _concentrating;
            }
            set
            {
                if (_concentrating == value) return;
                _concentrating = value;
            }

        }
        public bool ConcentrateInterrupted;
        public long ConcentrateInterruptTime;

        public bool Stacking;

        public IntelligentCreatureType SummonedCreatureType = IntelligentCreatureType.None;
        public bool CreatureSummoned;

        public LevelEffects LevelEffects = LevelEffects.None;

        private int _stepCounter, _runCounter, _fishCounter, _restedCounter;

        public MapObject[] ArcherTrapObjectsArray = new MapObject[9];
        public SpellObject[] PortalObjectsArray = new SpellObject[2];

        public NPCObject DefaultNPC
        {
            get
            {
                return SMain.Envir.DefaultNPC;
            }
        }

        public uint NPCID;
        public NPCPage NPCPage;
        public Dictionary<NPCSegment, bool> NPCSuccess = new Dictionary<NPCSegment, bool>();
        public bool NPCDelayed;
        public List<string> NPCSpeech = new List<string>();
        public Map NPCMoveMap;
        public Point NPCMoveCoord;
        public string NPCInputStr;

        public MarketMatcher MarketMatcher = new MarketMatcher();

        public bool UserMatch;
        public int PageSent;
        public List<AuctionInfo> Search = new List<AuctionInfo>();
        public List<ItemSets> ItemSets = new List<ItemSets>();
        public List<EquipmentSlot> MirSet = new List<EquipmentSlot>();

        public bool FatalSword, Slaying, FlamingSword, MPEater, Hemorrhage, CounterAttack;
        public bool DaMoGunFa;
        public int MPEaterCount, HemorrhageAttackCount;
        public long FlamingSwordTime, CounterAttackTime, DaMoGunFaTime;
        public bool ActiveReincarnation, ActiveSwiftFeet, ReincarnationReady;
        public PlayerObject ReincarnationTarget, ReincarnationHost;
        public long ReincarnationExpireTime;
        public byte ReflectRate;
        public bool UnlockCurse = false;
        public bool FastRun = false;
        public bool CanGainExp = true;

        public bool CanCreateGuild = false;
        public GuildObject MyGuild = null;
        public Rank MyGuildRank = null;
        public GuildObject PendingGuildInvite = null;
        public bool GuildNoticeChanged = true; //set to false first time client requests notice list, set to true each time someone in guild edits notice
        public bool GuildMembersChanged = true;//same as above but for members
        public bool GuildCanRequestItems = true;
        public bool RequestedGuildBuffInfo = false;
        public override bool Blocking
        {
            get
            {
                return !Dead && !Observer;
            }
        }
        public bool AllowGroup
        {
            get { return Info.AllowGroup; }
            set { Info.AllowGroup = value; }
        }

        public bool AllowTrade
        {
            get { return Info.AllowTrade; }
            set { Info.AllowTrade = value; }
        }


        public bool GameStarted { get; set; }

        public bool HasTeleportRing, HasProtectionRing, HasRevivalRing;
        public bool HasMuscleRing, HasClearRing, HasParalysisRing, HasProbeNecklace;

        public PlayerObject MarriageProposal;
        public PlayerObject DivorceProposal;
        public PlayerObject MentorRequest;

        public PlayerObject GroupInvitation;
        public PlayerObject TradeInvitation;

        public PlayerObject TradePartner = null;
        public bool TradeLocked = false;
        public uint TradeGoldAmount = 0;

        public PlayerObject ItemRentalPartner = null;
        public UserItem ItemRentalDepositedItem = null;
        public uint ItemRentalFeeAmount = 0;
        public uint ItemRentalPeriodLength = 0;
        public bool ItemRentalFeeLocked = false;
        public bool ItemRentalItemLocked = false;

        private long LastRankUpdate = Envir.Time;

        public bool Looks_HumUp;

        public List<QuestProgressInfo> CurrentQuests
        {
            get { return Info.CurrentQuests; }
        }

        public List<int> CompletedQuests
        {
            get { return Info.CompletedQuests; }
        }

        public byte AttackBonus, MineRate, GemRate, FishRate, CraftRate, SkillNeckBoost;

        public Dictionary<string, BaseService> services = new Dictionary<string, BaseService>();

        public PlayerObject(CharacterInfo info, MirConnection connection)
        {
            if (info.Player != null)
                throw new InvalidOperationException(Envir.Tr("Player.Info not Null."));


            info.Player = this;
            info.Mount = new MountInfo(this);

            Connection = connection;
            Info = info;
            Account = Connection.Account;

            Report = new Reporting(this);
            CommandMgr = new CommandMgr(this);

            if (Account.AdminAccount)
            {
                IsGM = true;
                SMain.Enqueue(string.Format(Envir.Tr("{0} is now a GM"), Name));
            }

            if (Level == 0) NewCharacter();

            if (Info.GuildIndex != -1)
            {
                MyGuild = Envir.GetGuild(Info.GuildIndex);
            }
            RefreshStats();

            if (HP == 0)
            {
                SetHP(MaxHP);
                SetMP(MaxMP);

                CurrentLocation = BindLocation;
                CurrentMapIndex = BindMapIndex;

                if (Info.PKPoints >= 200)
                {
                    Map temp = Envir.GetMapByNameAndInstance(Settings.PKTownMapName, GetRegionID(), 1);
                    Point tempLocation = new Point(Settings.PKTownPositionX, Settings.PKTownPositionY);

                    if (temp != null && temp.ValidPoint(tempLocation))
                    {
                        CurrentMapIndex = temp.Info.Index;
                        CurrentLocation = tempLocation;
                    }
                }
            }
        }

        public T GetService<T>() where T : BaseService
        {
            Type objType = typeof(T);
            return (T)services[objType.Name];
        }

        public void StopGame(byte reason)
        {
            if (Node == null) return;

            for (int i = 0; i < Pets.Count; i++)
            {
                MonsterObject pet = Pets[i];

                if (pet.Info.AI == 64)//IntelligentCreature
                {
                    //dont save Creatures they will miss alot of AI-Info when they get spawned on login
                    UnSummonIntelligentCreature(((IntelligentCreatureObject)pet).PetType, false);
                    continue;
                }

                pet.Master = null;

                if (!pet.Dead)
                {
                    try
                    {
                        Info.Pets.Add(new PetInfo(pet) { Time = Envir.Time });

                        Envir.MonsterCount--;
                        pet.CurrentMap.MonsterCount--;

                        pet.CurrentMap.RemoveObject(pet);
                        pet.Despawn();
                    }
                    catch
                    {
                        SMain.EnqueueDebugging(Name + " Pet logout was null on logout : " + pet != null ? pet.Name : "" + " " + pet.CurrentMap != null ? pet.CurrentMap.Info.FileName : "");
                    }
                }
            }
            Pets.Clear();

            for (int i = 0; i < Info.Magics.Count; i++)
            {
                if (Info.Magics[i] != null)
                    continue;

                if (Envir.Time < (Info.Magics[i].CastTime + Info.Magics[i].GetDelay()))
                    Info.Magics[i].CastTime = Info.Magics[i].GetDelay() + Info.Magics[i].CastTime - Envir.Time;
                else
                    Info.Magics[i].CastTime = 0;
            }

            if (MyGuild != null) MyGuild.PlayerLogged(this, false);
            Envir.Players.Remove(this);
            CurrentMap.RemoveObject(this);
            Despawn();

            if (GroupMembers != null)
            {
                GroupMembers.Remove(this);
                RemoveGroupBuff();

                if (GroupMembers.Count > 1)
                {
                    Packet p = new S.DeleteMember { Name = Name };

                    for (int i = 0; i < GroupMembers.Count; i++)
                        GroupMembers[i].Enqueue(p);
                }
                else
                {
                    GroupMembers[0].Enqueue(new S.DeleteGroup());
                    GroupMembers[0].GroupMembers = null;
                }
                GroupMembers = null;
            }

            for (int i = 0; i < Buffs.Count; i++)
            {
                Buff buff = Buffs[i];
                if (buff.Infinite) continue;
                if (buff.Type == BuffType.Curse) continue;

                buff.Caster = null;
                if (!buff.Paused) buff.ExpireTime -= Envir.Time;

                Info.Buffs.Add(buff);
            }
            Buffs.Clear();

            for (int i = 0; i < PoisonList.Count; i++)
            {
                Poison poison = PoisonList[i];
                poison.Owner = null;
                poison.TickTime -= Envir.Time;

                Info.Poisons.Add(poison);
            }

            PoisonList.Clear();

            for (int i = 0; i < NPCVar.Count; i++)
            {
                Info.NPCVar.Add(NPCVar[i]);
            }
            NPCVar.Clear();

            TradeCancel();
            CancelItemRental();
            RefineCancel();
            LogoutRelationship();
            LogoutMentor();

            string logReason = LogOutReason(reason);

            SMain.Enqueue(logReason);

            Fishing = false;

            Info.LastIP = Connection.IPAddress;
            Info.LastDate = Envir.Now;

            Report.Disconnected(logReason);
            Report.ForceSave();

            CleanUp();
        }

        private string LogOutReason(byte reason)
        {
            switch (reason)
            {
                //0-10 are 'senddisconnect to client'
                case 0:
                    return string.Format("{0} Has logged out. Reason: Server closed", Name);
                case 1:
                    return string.Format("{0} Has logged out. Reason: Double login", Name);
                case 2:
                    return string.Format("{0} Has logged out. Reason: Chat message too long", Name);
                case 3:
                    return string.Format("{0} Has logged out. Reason: Server crashed", Name);
                case 4:
                    return string.Format("{0} Has logged out. Reason: Kicked by admin", Name);
                case 5:
                    return string.Format("{0} Has logged out. Reason: Maximum connections reached", Name);
                case 10:
                    return string.Format("{0} Has logged out. Reason: Wrong client version", Name);
                case 20:
                    return string.Format("{0} Has logged out. Reason: User gone missing / disconnected", Name);
                case 21:
                    return string.Format("{0} Has logged out. Reason: Connection timed out", Name);
                case 22:
                    return string.Format("{0} Has logged out. Reason: User closed game", Name);
                case 23:
                    return string.Format("{0} Has logged out. Reason: User returned to select char", Name);
                default:
                    return string.Format("{0} Has logged out. Reason: Unknown", Name);
            }
        }

        private void NewCharacter()
        {
            if (Envir.StartPoints.Count == 0) return;

            SetBind();

            Level = 1;
            Hair = (byte)SMain.Envir.Random.Next(0, 9);


            for (int i = 0; i < Envir.StartItems.Count; i++)
            {
                ItemInfo info = Envir.StartItems[i];
                if (!CorrectStartItem(info)) continue;

                AddItem(Envir.CreateFreshItem(info));
            }

        }

        public long GetDelayTime(long original)
        {
            if (CurrentPoison.HasFlag(PoisonType.Slow))
            {
                return original * 2;
            }
            return original;
        }

        public override void Process()
        {
            if (Connection == null || Node == null || Info == null) return;

            if (GroupInvitation != null && GroupInvitation.Node == null)
                GroupInvitation = null;

            if (MagicShield && Envir.Time > MagicShieldTime)
            {
                MagicShield = false;
                MagicShieldLv = 0;
                MagicShieldTime = 0;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MagicShieldDown }, CurrentLocation);
                RemoveBuff(BuffType.MagicShield);
            }

            if (ElementalBarrier && Envir.Time > ElementalBarrierTime)
            {
                ElementalBarrier = false;
                ElementalBarrierLv = 0;
                ElementalBarrierTime = 0;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierDown }, CurrentLocation);
            }

            if (ElementalBarrier1 && Envir.Time > ElementalBarrierTime1)
            {
                ElementalBarrier1 = false;
                ElementalBarrierLv1 = 0;
                ElementalBarrierTime1 = 0;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierDown1 }, CurrentLocation);
            }

            if (CellTime + 700 < Envir.Time) _stepCounter = 0;

            if (Sneaking) CheckSneakRadius();

            if (FlamingSword && Envir.Time >= FlamingSwordTime)
            {
                FlamingSword = false;
                Enqueue(new S.SpellToggle { Spell = Spell.FlamingSword, CanUse = false });
            }

            if (DaMoGunFa && Envir.Time >= DaMoGunFaTime)
            {
                DaMoGunFa = false;
                Enqueue(new S.SpellToggle { Spell = Spell.DaMoGunFa, CanUse = false });
            }

            if (CounterAttack && Envir.Time >= CounterAttackTime)
            {
                CounterAttack = false;
            }

            if (ReincarnationReady && Envir.Time >= ReincarnationExpireTime)
            {
                ReincarnationReady = false;
                ActiveReincarnation = false;
                ReincarnationTarget = null;
                ReceiveChatTr("Reincarnation failed.", ChatType.System);
            }
            if ((ReincarnationReady || ActiveReincarnation) && (ReincarnationTarget == null || !ReincarnationTarget.Dead))
            {
                ReincarnationReady = false;
                ActiveReincarnation = false;
                ReincarnationTarget = null;
            }

            if (Envir.Time > RunTime && _runCounter > 0)
            {
                RunTime = Envir.Time + 1500;
                _runCounter--;
            }

            if (Settings.RestedPeriod > 0)
            {
                if (Envir.Time > RestedTime)
                {
                    _restedCounter = InSafeZone ? _restedCounter + 1 : _restedCounter;

                    if (_restedCounter > 0)
                    {
                        int count = _restedCounter / (Settings.RestedPeriod * 60);

                        GiveRestedBonus(count);
                    }

                    RestedTime = Envir.Time + Settings.Second;
                }
            }

            if (Stacking && Envir.Time > StackingTime)
            {
                Stacking = false;

                for (int i = 0; i < 8; i++)
                {
                    if (Pushed(this, (MirDirection)i, 1) == 1) break;
                }
            }

            if (NewMail)
            {
                ReceiveChatTr("New mail has arrived.", ChatType.System);

                GetMail();
            }

            if (Account.ExpandedStorageExpiryDate < Envir.Now && Account.HasExpandedStorage)
            {
                Account.HasExpandedStorage = false;
                ReceiveChatTr("Expanded storage has expired.", ChatType.System);
                Enqueue(new S.ResizeStorage { Size = Account.Storage.Length, HasExpandedStorage = Account.HasExpandedStorage, ExpiryTime = Account.ExpandedStorageExpiryDate });
            }

            if (Envir.Time > IncreaseLoyaltyTime && Mount.HasMount)
            {
                IncreaseLoyaltyTime = Envir.Time + (LoyaltyDelay * 60);
                IncreaseMountLoyalty(1);
            }

            if (Envir.Time > FishingTime && Fishing)
            {
                _fishCounter++;
                UpdateFish();
            }

            if (Envir.Time > ItemExpireTime)
            {
                ItemExpireTime = Envir.Time + ItemExpireDelay;

                ProcessItems();
            }

            for (int i = Pets.Count() - 1; i >= 0; i--)
            {
                MonsterObject pet = Pets[i];
                if (pet.Dead) Pets.Remove(pet);
            }

            ProcessBuffs();
            ProcessInfiniteBuffs();
            ProcessRegen();
            ProcessPoison();
            ProcessBuffV2s();

            RefreshCreaturesTimeLeft();

            ProcessDura();

            base.Process();

            RefreshNameColour();

        }

        public void ProcessDura()
        {
            UserItem item;
            if (Envir.Time > TorchTime)
            {
                TorchTime = Envir.Time + 10000;
                item = Info.Equipment[(int)EquipmentSlot.Torch];
                if (item != null)
                {
                    DamageItem(item, 5);

                    if (item.CurrentDura == 0)
                    {
                        Info.Equipment[(int)EquipmentSlot.Torch] = null;
                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                        RefreshStats();
                    }
                }
            }

            if (Envir.Time > DuraTime)
            {
                DuraTime = Envir.Time + DuraDelay;

                for (int i = 0; i < Info.Equipment.Length; i++)
                {
                    item = Info.Equipment[i];
                    if (item == null || !item.DuraChanged) continue; // || item.Info.Type == ItemType.Mount
                    item.DuraChanged = false;
                    Enqueue(new S.DuraChanged { UniqueID = item.UniqueID, CurrentDura = item.CurrentDura });
                }
            }
        }

        public override void SetOperateTime()
        {
            OperateTime = Envir.Time;
        }

        private void ProcessBuffs()
        {
            bool refresh = false;

            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                Buff buff = Buffs[i];

                if (Envir.Time <= buff.ExpireTime || buff.Infinite || buff.Paused) continue;

                Buffs.RemoveAt(i);
                Enqueue(new S.RemoveBuff { Type = buff.Type, ObjectID = ObjectID });

                if (buff.Visible)
                    Broadcast(new S.RemoveBuff { Type = buff.Type, ObjectID = ObjectID });

                switch (buff.Type)
                {
                    case BuffType.MoonLight:
                    case BuffType.Hiding:
                    case BuffType.DarkBody:
                        if (!HasClearRing) Hidden = false;
                        Sneaking = false;
                        for (int j = 0; j < Buffs.Count; j++)
                        {
                            switch (Buffs[j].Type)
                            {
                                case BuffType.Hiding:
                                case BuffType.MoonLight:
                                case BuffType.DarkBody:
                                    if (Buffs[j].Type != buff.Type)
                                        Buffs[j].ExpireTime = 0;
                                    break;
                            }
                        }
                        break;
                    case BuffType.MoonMist:
                        Hidden2 = false;
                        BroadcastInfo();
                        break;
                    case BuffType.Concentration:
                        ConcentrateInterrupted = false;
                        ConcentrateInterruptTime = 0;
                        Concentrating = false;
                        UpdateConcentration();
                        break;
                    case BuffType.SwiftFeet:
                        ActiveSwiftFeet = false;
                        break;
                    case BuffType.ImmortalSkin:
                        if (buff.Values.Length > 2 && buff.Values[2] > 0)
                        {
                            VampAmount = (ushort)buff.Values[2];
                        }
                        break;
                }

                refresh = true;
            }

            if (Concentrating && !ConcentrateInterrupted && (ConcentrateInterruptTime != 0))
            {
                //check for reenable
                if (ConcentrateInterruptTime <= SMain.Envir.Time)
                {
                    ConcentrateInterruptTime = 0;
                    UpdateConcentration();//Update & send to client
                }
            }

            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                Buff buff = Buffs[i];
                if (Envir.Time < buff.TickTime)
                    continue;

                switch (Buffs[i].Type)
                {
                    case BuffType.MoreaBlood:
                    case BuffType.MoreaBlood2:
                    case BuffType.MoreaBlood3:
                    case BuffType.MoreaBlood8:
                    case BuffType.KunLun14:
                        PoisonDamage(-buff.Values[0], buff.Caster);
                        buff.TickTime += buff.TickSpeed;
                        break;
                }

                if (Dead)
                    break;
            }

            if (refresh) RefreshStats();
        }
        private void ProcessInfiniteBuffs()
        {
            bool hiding = false;
            bool isGM = false;
            bool mentalState = false;

            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                Buff buff = Buffs[i];

                if (!buff.Infinite) continue;

                bool removeBuff = false;

                switch (buff.Type)
                {
                    case BuffType.Hiding:
                        hiding = true;
                        if (!HasClearRing) removeBuff = true;
                        break;
                    case BuffType.MentalState:
                        mentalState = true;
                        break;
                    case BuffType.GameMaster:
                        isGM = true;
                        if (!IsGM) removeBuff = true;
                        break;
                }

                if (removeBuff)
                {
                    Buffs.RemoveAt(i);
                    Enqueue(new S.RemoveBuff { Type = buff.Type, ObjectID = ObjectID });

                    switch (buff.Type)
                    {
                        case BuffType.Hiding:
                            Hidden = false;
                            break;
                    }
                }
            }

            if (HasClearRing && !hiding)
            {
                AddBuff(new Buff { Type = BuffType.Hiding, Caster = this, ExpireTime = Envir.Time + 100, Infinite = true });
            }

            if (GetMagic(Spell.MentalState) != null && !mentalState)
            {
                AddBuff(new Buff { Type = BuffType.MentalState, Caster = this, ExpireTime = Envir.Time + 100, Values = new int[] { Info.MentalState }, Infinite = true });
            }

            if (IsGM && !isGM)
            {
                AddBuff(new Buff { Type = BuffType.GameMaster, Caster = this, ExpireTime = Envir.Time + 100, Values = new int[] { 0 }, Infinite = true, Visible = Settings.GameMasterEffect });
            }
        }
        private void ProcessRegen()
        {
            if (Dead) return;

            int healthRegen = 0, manaRegen = 0;

            if (CanRegen)
            {
                RegenTime = Envir.Time + RegenDelay;


                if (HP < MaxHP)
                {
                    healthRegen += (int)(MaxHP * 0.03F) + 1;
                    healthRegen += (int)(healthRegen * ((double)HealthRecovery / Settings.HealthRegenWeight));
                }

                if (MP < MaxMP)
                {
                    manaRegen += (int)(MaxMP * 0.03F) + 1;
                    manaRegen += (int)(manaRegen * ((double)SpellRecovery / Settings.ManaRegenWeight));
                }
            }

            if (Envir.Time > PotTime)
            {
                //PotTime = Envir.Time + Math.Max(50,Math.Min(PotDelay, 600 - (Level * 10)));
                PotTime = Envir.Time + PotDelay;
                int PerTickRegen = 5 + (Level / 10);

                if (PotHealthAmount > PerTickRegen)
                {
                    healthRegen += PerTickRegen;
                    PotHealthAmount -= (ushort)PerTickRegen;
                }
                else
                {
                    healthRegen += PotHealthAmount;
                    PotHealthAmount = 0;
                }

                if (PotManaAmount > PerTickRegen)
                {
                    manaRegen += PerTickRegen;
                    PotManaAmount -= (ushort)PerTickRegen;
                }
                else
                {
                    manaRegen += PotManaAmount;
                    PotManaAmount = 0;
                }
            }

            if (Envir.Time > HealTime)
            {
                HealTime = Envir.Time + HealDelay;

                int incHeal = (Level / 10) + (HealAmount / 10);
                if (HealAmount > (5 + incHeal))
                {
                    healthRegen += (5 + incHeal);
                    HealAmount -= (ushort)Math.Min(HealAmount, 5 + incHeal);
                }
                else
                {
                    healthRegen += HealAmount;
                    HealAmount = 0;
                }
            }

            if (Envir.Time > VampTime)
            {
                VampTime = Envir.Time + VampDelay;

                ushort incHeal = (ushort)(VampAmount / 8);
                if (incHeal > 10)
                {
                    healthRegen += incHeal;
                    VampAmount -= incHeal;
                }
                else
                {
                    healthRegen += VampAmount;
                    VampAmount = 0;
                }
            }

            if (healthRegen > 0)
            {
                BroadcastDamageIndicator(DamageType.Heal, healthRegen);
                ChangeHP(healthRegen);
            }
            if (HP == MaxHP)
            {
                PotHealthAmount = 0;
                HealAmount = 0;
            }

            if (manaRegen > 0) ChangeMP(manaRegen);
            if (MP == MaxMP) PotManaAmount = 0;
        }

        private void ProcessBuffV2s()
        {
            for (int i = BuffV2s.Count - 1; i >= 0; i--)
            {
                BuffV2 buff = BuffV2s[i];
                if (buff.Expire != -1 && Envir.Time > buff.Expire)
                {
                    BuffV2s.RemoveAt(i);
                    continue;
                } 

                BuffV2s[i].Process();
            }
        }

        private void ProcessPoison()
        {
            PoisonType type = PoisonType.None;
            ArmourRate = 1F;
            DamageRate = 1F;

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (Dead) return;

                Poison poison = PoisonList[i];

                if (poison.Owner != null && poison.Owner.Node == null)
                {
                    PoisonList.RemoveAt(i);
                    continue;
                }

                if (Envir.Time > poison.TickTime)
                {
                    poison.Time++;
                    poison.TickTime = Envir.Time + poison.TickSpeed;

                    if (poison.Time >= poison.Duration)
                        PoisonList.RemoveAt(i);

                    if (poison.PType == PoisonType.Green || poison.PType == PoisonType.Bleeding || poison.PType == PoisonType.Morea)
                    {
                        LastHitter = poison.Owner;
                        LastHitTime = Envir.Time + 10000;

                        if (poison.PType == PoisonType.Bleeding)
                        {
                            Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Bleeding, EffectType = 0 });
                        }

                        //ChangeHP(-poison.Value);
                        PoisonDamage(-poison.Value, poison.Owner);

                        if (Dead) break;
                        RegenTime = Envir.Time + RegenDelay;
                    }
                    if (poison.PType == PoisonType.DelayedExplosion)
                    {
                        if (Envir.Time > ExplosionInflictedTime) ExplosionInflictedStage++;

                        if (!ProcessDelayedExplosion(poison))
                        {
                            if (Dead) break;

                            ExplosionInflictedStage = 0;
                            ExplosionInflictedTime = 0;
                            PoisonList.RemoveAt(i);
                            continue;
                        }
                    }
                }

                switch (poison.PType)
                {
                    case PoisonType.Red:
                        ArmourRate -= 0.10F;
                        break;
                    case PoisonType.Stun:
                        DamageRate += 0.20F;
                        break;
                }
                type |= poison.PType;
                /*
                if ((int)type < (int)poison.PType)
                    type = poison.PType;
                */
            }

            if (type == CurrentPoison) return;

            Enqueue(new S.Poisoned { Poison = type });
            Broadcast(new S.ObjectPoisoned { ObjectID = ObjectID, Poison = type });

            CurrentPoison = type;
        }
        private bool ProcessDelayedExplosion(Poison poison)
        {
            if (Dead) return false;

            if (ExplosionInflictedStage == 0)
            {
                Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 0 });
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 0 });
                return true;
            }
            if (ExplosionInflictedStage == 1)
            {
                if (Envir.Time > ExplosionInflictedTime)
                    ExplosionInflictedTime = poison.TickTime + 1500;
                Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 1 });
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 1 });
                return true;
            }
            if (ExplosionInflictedStage == 2)
            {
                Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 2 });
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 2 });
                if (poison.Owner != null)
                {
                    switch (poison.Owner.Race)
                    {
                        case ObjectType.Player:
                            PlayerObject caster = (PlayerObject)poison.Owner;
                            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time, poison.Owner, caster.GetMagic(Spell.DelayedExplosion), poison.Value, this.CurrentLocation, this, poison.Infect);
                            CurrentMap.ActionList.Add(action);
                            //Attacked((PlayerObject)poison.Owner, poison.Value, DefenceType.MAC, false);
                            break;
                        case ObjectType.Monster://this is in place so it could be used by mobs if one day someone chooses to
                            Attacked((MonsterObject)poison.Owner, poison.Value, DefenceType.MAC);
                            break;

                    }

                    LastHitter = poison.Owner;
                }
                return false;
            }
            return false;
        }

        private void ProcessItems()
        {
            for (var i = 0; i < Info.Inventory.Length; i++)
            {
                var item = Info.Inventory[i];

                if (item?.ExpireInfo?.ExpiryDate <= Envir.Now)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} has just expired from your inventory."), item.Info.FriendlyName), ChatType.Hint);
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    Info.Inventory[i] = null;
                    UpdateQuestItem(item.Info);

                    continue;
                }

                if (item?.RentalInformation?.RentalLocked != true ||
                    !(item?.RentalInformation?.ExpiryDate <= Envir.Now))
                    continue;

                ReceiveChat(string.Format("The rental lock has been removed from {0}.", item.Info.FriendlyName), ChatType.Hint);
                item.RentalInformation = null;
            }

            for (var i = 0; i < Info.Equipment.Length; i++)
            {
                var item = Info.Equipment[i];

                if (item?.ExpireInfo?.ExpiryDate <= Envir.Now)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} has just expired from your equipment."), item.Info.FriendlyName), ChatType.Hint);
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    Info.Equipment[i] = null;
                    UpdateQuestItem(item.Info);

                    continue;
                }

                if (item?.RentalInformation?.RentalLocked != true ||
                    !(item?.RentalInformation?.ExpiryDate <= Envir.Now))
                    continue;

                ReceiveChat(string.Format("The rental lock has been removed from {0}.", item.Info.FriendlyName), ChatType.Hint);
                item.RentalInformation = null;
            }

            for (int i = 0; i < Info.AccountInfo.Storage.Length; i++)
            {
                var item = Info.AccountInfo.Storage[i];
                if (item?.ExpireInfo?.ExpiryDate <= Envir.Now)
                {
                    ReceiveChat($"{item.Info.FriendlyName} has just expired from your storage.", ChatType.Hint);
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    Info.AccountInfo.Storage[i] = null;

                    continue;
                }
            }
        }

        public override void Process(DelayedAction action)
        {
            if (action.FlaggedToRemove)
                return;

            switch (action.Type)
            {
                case DelayedType.Magic:
                    CompleteMagic(action.Params);
                    break;
                case DelayedType.Damage:
                    CompleteAttack(action.Params);
                    break;
                case DelayedType.MapMovement:
                    CompleteMapMovement(action.Params);
                    break;
                case DelayedType.Mine:
                    CompleteMine(action.Params);
                    break;
                case DelayedType.NPC:
                    CompleteNPC(action.Params);
                    break;
                case DelayedType.Poison:
                    CompletePoison(action.Params);
                    break;
                case DelayedType.DamageIndicator:
                    CompleteDamageIndicator(action.Params);
                    break;
                case DelayedType.MagicSpell:
                    CompleteMagicSpell(action.Params);
                    break;
            }
        }

        private void CompleteMagicSpell(IList<object> data)
        {
            UserMagic magic = (UserMagic)data[0];
            int spellingId = (int)data[4];
            int delay = (int)data[3];
            int value = (int)data[1];
            MapObject target = (MapObject)data[2];
            if (spellingId != SpellingId)
                return;

            SpellingId = 0;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, value, target);
            ActionList.Add(action);
        }

        private void SetHP(ushort amount)
        {
            if (HP == amount) return;

            HP = amount <= MaxHP ? amount : MaxHP;
            HP = GMNeverDie ? MaxHP : HP;

            if (!Dead && HP == 0) Die();

            //HealthChanged = true;
            Enqueue(new S.HealthChanged { HP = HP, MP = MP });
            BroadcastHealthChange();
        }
        private void SetMP(ushort amount)
        {
            if (MP == amount) return;
            //was info.MP
            MP = amount <= MaxMP ? amount : MaxMP;
            MP = GMNeverDie ? MaxMP : MP;

            // HealthChanged = true;
            Enqueue(new S.HealthChanged { HP = HP, MP = MP });
            BroadcastHealthChange();
        }

        public void ChangeHP(int amount)
        {
            //if (amount < 0) amount = (int)(amount * PoisonRate);

            if (HasProtectionRing && MP > 0 && amount < 0)
            {
                ChangeMP(amount);
                return;
            }

            ushort value = (ushort)Math.Max(ushort.MinValue, Math.Min(MaxHP, HP + amount));

            if (value == HP) return;

            HP = value;
            HP = GMNeverDie ? MaxHP : HP;

            if (!Dead && HP == 0) Die();

            // HealthChanged = true;
            Enqueue(new S.HealthChanged { HP = HP, MP = MP });
            BroadcastHealthChange();
        }
        //use this so you can have mobs take no/reduced poison damage
        public void PoisonDamage(int amount, MapObject Attacker)
        {
            ChangeHP(amount);
        }
        public void ChangeMP(int amount)
        {
            ushort value = (ushort)Math.Max(ushort.MinValue, Math.Min(MaxMP, MP + amount));

            if (value == MP) return;

            MP = value;
            MP = GMNeverDie ? MaxMP : MP;

            // HealthChanged = true;
            Enqueue(new S.HealthChanged { HP = HP, MP = MP });
            BroadcastHealthChange();
        }
        public override void Die()
        {
            if (HasRevivalRing && Envir.Time > LastRevivalTime)
            {
                LastRevivalTime = Envir.Time + 300000;

                for (var i = (int)EquipmentSlot.RingL; i <= (int)EquipmentSlot.RingR; i++)
                {
                    var item = Info.Equipment[i];

                    if (item == null) continue;
                    if (!(item.Info.Unique.HasFlag(SpecialItemMode.Revival)) || item.CurrentDura < 1000) continue;
                    SetHP(MaxHP);
                    item.CurrentDura = (ushort)(item.CurrentDura - 1000);
                    Enqueue(new S.DuraChanged { UniqueID = item.UniqueID, CurrentDura = item.CurrentDura });
                    RefreshStats();
                    ReceiveChatTr("You have been given a second chance at life", ChatType.System);
                    return;
                }
            }

            if (LastHitter != null && LastHitter.Race == ObjectType.Player)
            {
                PlayerObject hitter = (PlayerObject)LastHitter;

                if (AtWar(hitter) || WarZone)
                {
                    hitter.ReceiveChat(string.Format(Envir.Tr("You've been protected by the law")), ChatType.System);
                }
                else if (Envir.Time > BrownTime && PKPoints < 200)
                {
                    UserItem weapon = hitter.Info.Equipment[(byte)EquipmentSlot.Weapon];

                    hitter.PKPoints = Math.Min(int.MaxValue, LastHitter.PKPoints + 100);
                    hitter.ReceiveChat(string.Format(Envir.Tr("You have murdered {0}"), Name), ChatType.System);
                    ReceiveChat(string.Format(Envir.Tr("You have been murdered by {0}"), LastHitter.Name), ChatType.System);

                    if (weapon != null && weapon.Luck > (Settings.MaxLuck * -1) && Envir.Random.Next(4) == 0)
                    {
                        weapon.Luck--;
                        hitter.ReceiveChatTr("Your weapon has been cursed.", ChatType.System);
                        hitter.Enqueue(new S.RefreshItem { Item = weapon });
                    }
                }
            }

            UnSummonIntelligentCreature(SummonedCreatureType);

            for (int i = Pets.Count - 1; i >= 0; i--)
            {
                if (Pets[i].Dead) continue;
                Pets[i].Die();
            }

            if (MagicShield)
            {
                MagicShield = false;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MagicShieldDown }, CurrentLocation);
            }
            if (ElementalBarrier)
            {
                ElementalBarrier = false;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierDown }, CurrentLocation);
            }
            if (ElementalBarrier1)
            {
                ElementalBarrier1 = false;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierDown1 }, CurrentLocation);
            }

            if (PKPoints > 200)
                RedDeathDrop(LastHitter);
            else if (!InSafeZone)
                DeathDrop(LastHitter);

            HP = 0;
            Dead = true;

            LogTime = Envir.Time;
            BrownTime = Envir.Time;

            Enqueue(new S.Death { Direction = Direction, Location = CurrentLocation });
            Broadcast(new S.ObjectDied { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            for (int i = 0; i < Buffs.Count; i++)
            {
                if (Buffs[i].Type == BuffType.Curse)
                {
                    Buffs.RemoveAt(i);
                    break;
                }
            }

            PoisonList.Clear();
            InTrapRock = false;

            CallDefaultNPC(DefaultNPCType.Die);

            Report.Died(CurrentMap.Info.FileName);
        }

        private void DeathDrop(MapObject killer)
        {
            var pkbodydrop = true;

            if (CurrentMap == null || CurrentMap.Info == null || CurrentMap.Info.NoDropPlayer && Race == ObjectType.Player)
                return;

            if ((killer == null) || ((pkbodydrop) || (killer.Race != ObjectType.Player)))
            {
                for (var i = 0; i < Info.Equipment.Length; i++)
                {
                    var item = Info.Equipment[i];

                    if (item == null)
                        continue;

                    if (item.Info.Bind.HasFlag(BindMode.DontDeathdrop))
                        continue;

                    // TODO: Check this.
                    if (item.WeddingRing != -1 && Info.Equipment[(int)EquipmentSlot.RingL].UniqueID == item.UniqueID)
                        continue;

                    if (((killer == null) || ((killer != null) && (killer.Race != ObjectType.Player))))
                    {
                        if (item.Info.Bind.HasFlag(BindMode.BreakOnDeath))
                        {
                            Info.Equipment[i] = null;
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                            ReceiveChat($"Your {item.FriendlyName} shattered upon death.", ChatType.System2);
                            Report.ItemChanged("Death Drop", item, item.Count, 1);
                        }
                    }
                    if (ItemSets.Any(set => set.Set == ItemSet.Spirit && !set.SetComplete))
                    {
                        if (item.Info.Set == ItemSet.Spirit)
                        {
                            Info.Equipment[i] = null;
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                            Report.ItemChanged("Death Drop", item, item.Count, 1);
                        }
                    }

                    if (item.Count > 1)
                    {
                        var percent = Envir.RandomomRange(10, 8);
                        var count = (uint)Math.Ceiling(item.Count / 10F * percent);

                        if (count > item.Count || count <= 0)
                            continue;

                        var temp2 = Envir.CreateFreshItem(item.Info);
                        temp2.Count = count;

                        if (!DropItem(temp2, Settings.DropRange, true))
                            continue;

                        if (count == item.Count)
                            Info.Equipment[i] = null;

                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = count });
                        item.Count -= count;

                        Report.ItemChanged("Death Drop", item, count, 1);
                    }
                    else if (Envir.Random.Next(30) == 0)
                    {
                        if (Envir.ReturnRentalItem(item, item.RentalInformation?.OwnerName, Info))
                        {
                            Info.Equipment[i] = null;
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                            ReceiveChat($"You died and {item.Info.FriendlyName} has been returned to it's owner.", ChatType.Hint);
                            Report.ItemMailed("Death Dropped Rental Item", item, 1, 1);

                            continue;
                        }

                        if (!DropItem(item, Settings.DropRange, true))
                            continue;

                        if (item.Info.GlobalDropNotify)
                            foreach (var player in Envir.Players)
                            {
                                if (player.GetRegionID() != GetRegionID())
                                    continue;

                                player.ReceiveChat(string.Format(Envir.Tr("{0} {1} {2} has dropped {3}."), CurrentMap.Region, CurrentMap.Info.Title, Name,
                                    item.FriendlyName), ChatType.System2);
                            }

                        Info.Equipment[i] = null;
                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                        Report.ItemChanged("Death Drop", item, item.Count, 1);
                    }
                }

            }

            for (var i = 0; i < Info.Inventory.Length; i++)
            {
                var item = Info.Inventory[i];

                if (item == null)
                    continue;

                if (item.Info.Bind.HasFlag(BindMode.DontDeathdrop))
                    continue;

                if (item.WeddingRing != -1)
                    continue;

                if (item.Count > 1)
                {
                    var percent = Envir.RandomomRange(10, 8);

                    if (percent == 0)
                        continue;

                    var count = (uint)Math.Ceiling(item.Count / 10F * percent);

                    if (count > item.Count)
                        throw new ArgumentOutOfRangeException();

                    var temp2 = Envir.CreateFreshItem(item.Info);
                    temp2.Count = count;

                    if (!DropItem(temp2, Settings.DropRange, true))
                        continue;

                    if (count == item.Count)
                        Info.Inventory[i] = null;

                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = count });
                    item.Count -= count;
                    UpdateQuestItem(item.Info);
                    Report.ItemChanged("DeathDrop", item, count, 1);
                }
                else if (Envir.Random.Next(10) == 0)
                {
                    if (Envir.ReturnRentalItem(item, item.RentalInformation?.OwnerName, Info))
                    {
                        Info.Inventory[i] = null;
                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                        UpdateQuestItem(item.Info);

                        ReceiveChat($"You died and {item.Info.FriendlyName} has been returned to has been returned to it's owner.", ChatType.Hint);
                        Report.ItemMailed("Death Dropped Rental Item", item, 1, 1);

                        continue;
                    }

                    if (!DropItem(item, Settings.DropRange, true))
                        continue;

                    if (item.Info.GlobalDropNotify)
                        foreach (var player in Envir.Players)
                        {
                            player.ReceiveChat(string.Format(Envir.Tr("{0} {1} {2} has dropped {3}."), CurrentMap.Region, CurrentMap.Info.Title, Name,
                                  item.FriendlyName), ChatType.System2);
                        }

                    Info.Inventory[i] = null;
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    UpdateQuestItem(item.Info);

                    Report.ItemChanged("DeathDrop", item, item.Count, 1);
                }
            }

            RefreshStats();
        }
        private void RedDeathDrop(MapObject killer)
        {
            if (killer == null || killer.Race != ObjectType.Player)
            {
                for (var i = 0; i < Info.Equipment.Length; i++)
                {
                    var item = Info.Equipment[i];

                    if (item == null)
                        continue;

                    if (item.Info.Bind.HasFlag(BindMode.DontDeathdrop))
                        continue;

                    // TODO: Check this.
                    if ((item.WeddingRing != -1) && (Info.Equipment[(int)EquipmentSlot.RingL].UniqueID == item.UniqueID))
                        continue;

                    if (item.Info.Bind.HasFlag(BindMode.BreakOnDeath))
                    {
                        Info.Equipment[i] = null;
                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                        ReceiveChat($"Your {item.FriendlyName} shattered upon death.", ChatType.System2);
                        Report.ItemChanged("RedDeathDrop", item, item.Count, 1);
                    }

                    if (item.Count > 1)
                    {
                        var percent = Envir.RandomomRange(10, 4);
                        var count = (uint)Math.Ceiling(item.Count / 10F * percent);

                        if (count > item.Count || count <= 0)
                            continue;

                        var temp2 = Envir.CreateFreshItem(item.Info);
                        temp2.Count = count;

                        if (!DropItem(temp2, Settings.DropRange, true))
                            continue;

                        if (count == item.Count)
                            Info.Equipment[i] = null;

                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = count });
                        item.Count -= count;
                        UpdateQuestItem(item.Info);

                        Report.ItemChanged("RedDeathDrop", item, count, 1);
                    }
                    else if (Envir.Random.Next(10) == 0)
                    {
                        if (Envir.ReturnRentalItem(item, item.RentalInformation?.OwnerName, Info))
                        {
                            Info.Equipment[i] = null;
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                            ReceiveChat($"You died and {item.Info.FriendlyName} has been returned to it's owner.", ChatType.Hint);
                            Report.ItemMailed("Death Dropped Rental Item", item, 1, 1);

                            continue;
                        }

                        if (!DropItem(item, Settings.DropRange, true))
                            continue;

                        if (item.Info.GlobalDropNotify)
                            foreach (var player in Envir.Players)
                            {
                                if (player.GetRegionID() != GetRegionID())
                                    continue;

                                player.ReceiveChat(string.Format(Envir.Tr("{0} {1} {2} has dropped {3}."), CurrentMap.Region, CurrentMap.Info.Title, Name,
                                  item.FriendlyName), ChatType.System2);
                            }

                        Info.Equipment[i] = null;
                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                        Report.ItemChanged("RedDeathDrop", item, item.Count, 1);
                    }
                }

            }

            for (var i = 0; i < Info.Inventory.Length; i++)
            {
                var item = Info.Inventory[i];

                if (item == null)
                    continue;

                if (item.Info.Bind.HasFlag(BindMode.DontDeathdrop))
                    continue;

                if (item.WeddingRing != -1)
                    continue;

                if (Envir.ReturnRentalItem(item, item.RentalInformation?.OwnerName, Info))
                {
                    Info.Inventory[i] = null;
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    UpdateQuestItem(item.Info);

                    ReceiveChat($"You died and {item.Info.FriendlyName} has been returned to it's owner.", ChatType.Hint);
                    Report.ItemMailed("Death Dropped Rental Item", item, 1, 1);

                    continue;
                }

                if (!DropItem(item, Settings.DropRange, true))
                    continue;

                if (item.Info.GlobalDropNotify)
                    foreach (var player in Envir.Players)
                    {
                        player.ReceiveChat(string.Format(Envir.Tr("{0} {1} {2} has dropped {3}."), CurrentMap.Region, CurrentMap.Info.Title, Name,
                                  item.FriendlyName), ChatType.System2);
                    }

                Info.Inventory[i] = null;
                Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                UpdateQuestItem(item.Info);

                Report.ItemChanged("RedDeathDrop", item, item.Count, 1);
            }

            RefreshStats();
        }

        public void DownloadFile(string fileName, int start)
        {
            if (!IsGM)
                return;

            if (!File.Exists(fileName))
                ReceiveChat("Can not find file " + fileName, ChatType.Hint);

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                if (start < fileStream.Length)
                {
                    byte[] data = new byte[255];
                    fileStream.Seek(start, SeekOrigin.Begin);
                    int len = fileStream.Read(data, 0, 255);

                    Enqueue(new S.DownloadFile() { FileName = fileName, FileLength = (uint)fileStream.Length, Start = start, Data = data });
                }
                else
                {
                    ReceiveChat("File start large then file lenght " + fileName, ChatType.Hint);
                }
            }
        }

        public override void WinExp(uint amount, uint targetLevel = 0)
        {
            int expPoint;

            if (Level < targetLevel + 10 || !Settings.ExpMobLevelDifference)
            {
                expPoint = (int)amount;
            }
            else
            {
                expPoint = (int)amount - (int)Math.Round(Math.Max(amount / 15, 1) * ((double)Level - (targetLevel + 10)));
            }

            if (expPoint <= 0) expPoint = 1;

            expPoint = (int)(expPoint * Settings.ExpRate);

            //party
            float[] partyExpRate = { 1F, 1.3F, 1.6F, 1.8F, 2.2F, 2.5F, 2.8F, 3.1F, 3.3F, 3.5F, 3.7F, 4.0F };

            if (GroupMembers != null)
            {
                int sumLevel = 0;
                int nearCount = 0;
                for (int i = 0; i < GroupMembers.Count; i++)
                {
                    PlayerObject player = GroupMembers[i];

                    if (player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        sumLevel += player.Level;
                        nearCount++;
                    }
                }

                if (nearCount > partyExpRate.Length-1) nearCount = partyExpRate.Length-1;

                for (int i = 0; i < GroupMembers.Count; i++)
                {
                    PlayerObject player = GroupMembers[i];
                    if (player.CurrentMap == CurrentMap &&
                        Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                    {
                        player.GainExp((uint)(expPoint * partyExpRate[nearCount] * player.Level / sumLevel));
                    }
                }
            }
            else
                GainExp((uint)expPoint);
        }

        public void GainExp(uint amount)
        {
            if (!CanGainExp) return;

            if (amount == 0) return;

            if (Info.Married != 0)
            {
                Buff buff = Buffs.Where(e => e.Type == BuffType.RelationshipEXP).FirstOrDefault();
                if (buff != null)
                {
                    CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);
                    PlayerObject player = Envir.GetPlayer(Lover.Name);
                    if (player != null && player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                    {
                        amount += ((amount / 100) * (uint)Settings.LoverEXPBonus);
                    }
                }
            }

            if (Info.Mentor != 0 && !Info.isMentor)
            {
                Buff buffMentor = Buffs.Where(e => e.Type == BuffType.Mentee).FirstOrDefault();
                if (buffMentor != null)
                {
                    CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);
                    PlayerObject player = Envir.GetPlayer(Mentor.Name);
                    if (player != null && player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                    {
                        amount += ((amount / 100) * (uint)Settings.MentorExpBoost);
                    }
                }
            }

            if (ExpRateOffset > 0)
                amount += (uint)(amount * (ExpRateOffset / 100));
            if (Info.Mentor != 0 && !Info.isMentor)
                MenteeEXP += (amount / 100) * Settings.MenteeExpBank;

            Experience += amount;

            Enqueue(new S.GainExperience { Amount = amount });


            for (int i = 0; i < Pets.Count; i++)
            {
                MonsterObject monster = Pets[i];
                if (monster.CurrentMap == CurrentMap && Functions.InRange(monster.CurrentLocation, CurrentLocation, Globals.DataRange) && !monster.Dead)
                    monster.PetExp(amount);
            }

            if (MyGuild != null)
                MyGuild.GainExp(amount);

            if (Experience < MaxExperience) return;
            if (Level >= ushort.MaxValue) return;

            //Calculate increased levels
            var experience = Experience;

            while (experience >= MaxExperience)
            {
                Level++;
                experience -= MaxExperience;

                RefreshLevelStats();

                if (Level >= ushort.MaxValue) break;
            }

            Experience = experience;

            LevelUp();

            if (IsGM) return;
            if ((LastRankUpdate + 3600 * 1000) > Envir.Time)
            {
                LastRankUpdate = Envir.Time;
                SMain.Envir.CheckRankUpdate(GetRegionID(), Info);
            }
        }

        public void LevelUp()
        {
            RefreshStats();
            SetHP(MaxHP);
            SetMP(MaxMP);

            CallDefaultNPC(DefaultNPCType.LevelUp);

            Enqueue(new S.LevelChanged { Level = Level, Experience = Experience, MaxExperience = MaxExperience });
            Broadcast(new S.ObjectLeveled { ObjectID = ObjectID });

            if (Info.Mentor != 0 && !Info.isMentor)
            {
                CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);
                if ((Mentor != null) && ((Info.Level + Settings.MentorLevelGap) > Mentor.Level))
                    MentorBreak();
            }

            for (int i = CurrentMap.NPCs.Count - 1; i >= 0; i--)
            {
                if (Functions.InRange(CurrentMap.NPCs[i].CurrentLocation, CurrentLocation, Globals.DataRange))
                    CurrentMap.NPCs[i].CheckVisible(this);
            }
            Report.Levelled(Level);
            if (IsGM) return;
            SMain.Envir.CheckRankUpdate(GetRegionID(), Info);
        }

        private static int FreeSpace(IList<UserItem> array)
        {
            int count = 0;

            for (int i = 0; i < array.Count; i++)
                if (array[i] == null) count++;

            return count;
        }

        private void AddQuestItem(UserItem item)
        {
            if (item.Info.StackSize > 1) //Stackable
            {
                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem temp = Info.Inventory[i];
                    if (temp == null || item.Info != temp.Info || temp.Count >= temp.Info.StackSize) continue;

                    if (item.Count + temp.Count <= temp.Info.StackSize)
                    {
                        temp.Count += item.Count;
                        return;
                    }
                    item.Count -= temp.Info.StackSize - temp.Count;
                    temp.Count = temp.Info.StackSize;
                }
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] != null) continue;
                Info.Inventory[i] = item;

                return;
            }
        }

        private void AddItem(UserItem item)
        {
            if (item.Info.StackSize > 1) //Stackable
            {
                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem temp = Info.Inventory[i];
                    if (temp == null || item.Info != temp.Info || temp.Count >= temp.Info.StackSize) continue;

                    if (item.Count + temp.Count <= temp.Info.StackSize)
                    {
                        temp.Count += item.Count;
                        return;
                    }
                    item.Count -= temp.Info.StackSize - temp.Count;
                    temp.Count = temp.Info.StackSize;
                }
            }

            if (item.Info.Type == ItemType.Potion || item.Info.Type == ItemType.Scroll || (item.Info.Type == ItemType.Script && item.Info.Effect == 1))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Info.Inventory[i] != null) continue;
                    Info.Inventory[i] = item;
                    return;
                }
            }
            else if (item.Info.Type == ItemType.Amulet)
            {
                for (int i = 4; i < 6; i++)
                {
                    if (Info.Inventory[i] != null) continue;
                    Info.Inventory[i] = item;
                    return;
                }
            }
            else
            {
                for (int i = 6; i < Info.Inventory.Length; i++)
                {
                    if (Info.Inventory[i] != null) continue;
                    Info.Inventory[i] = item;
                    return;
                }
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] != null) continue;
                Info.Inventory[i] = item;
                return;
            }
        }

        private bool CorrectStartItem(ItemInfo info)
        {
            switch (Class)
            {
                case MirClass.Warrior:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Warrior)) return false;
                    break;
                case MirClass.Wizard:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Wizard)) return false;
                    break;
                case MirClass.Taoist:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Taoist)) return false;
                    break;
                case MirClass.Assassin:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Assassin)) return false;
                    break;
                case MirClass.Archer:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Archer)) return false;
                    break;
                case MirClass.Monk:
                    if (!info.RequiredClass.HasFlag(RequiredClass.Monk)) return false;
                    break;
               default:
                    return false;
            }

            switch (Gender)
            {
                case MirGender.Male:
                    if (!info.RequiredGender.HasFlag(RequiredGender.Male)) return false;
                    break;
                case MirGender.Female:
                    if (!info.RequiredGender.HasFlag(RequiredGender.Female)) return false;
                    break;
                default:
                    return false;
            }

            return true;
        }
        public void CheckItemInfo(ItemInfo info, bool dontLoop = false)
        {
            if ((dontLoop == false) && (info.ClassBased | info.LevelBased)) //send all potential data so client can display it
            {
                foreach (var item in Envir.ItemInfoList.Values)
                {
                    if ((item != info) && (item.Name.StartsWith(info.Name)))
                        CheckItemInfo(item, true);
                }
            }

            if (Connection.SentItemInfo.Contains(info)) return;
            Enqueue(new S.NewItemInfo { Info = info });
            Connection.SentItemInfo.Add(info);
        }
        public void CheckItem(UserItem item)
        {
            CheckItemInfo(item.Info);

            for (int i = 0; i < item.Slots.Length; i++)
            {
                if (item.Slots[i] == null) continue;

                CheckItemInfo(item.Slots[i].Info);
            }

            for (int i = 0; i < item.RuneSlots.Length; i++)
            {
                if (item.RuneSlots[i] == null || item.RuneSlots[i].SocketedItem == null) continue;

                CheckItemInfo(item.RuneSlots[i].SocketedItem.Info);
            }
        }
        public void CheckQuestInfo(QuestInfo info)
        {
            if (Connection.SentQuestInfo.Contains(info)) return;
            Enqueue(new S.NewQuestInfo { Info = info.CreateClientQuestInfo() });
            Connection.SentQuestInfo.Add(info);
        }

        public void CheckRecipeInfo(RecipeInfo info)
        {
            if (Connection.SentRecipeInfo.Contains(info)) return;

            CheckItemInfo(info.Item.Info);

            foreach (var ingredient in info.Ingredients)
            {
                CheckItemInfo(ingredient.Info);
            }

            Enqueue(new S.NewRecipeInfo { Info = info.CreateClientRecipeInfo() });
            Connection.SentRecipeInfo.Add(info);
        }

        private void SetBind()
        {
            SafeZoneInfo szi = Envir.StartPoints[Envir.Random.Next(Envir.StartPoints.Count)];

            BindMapIndex = szi.Info.Index;
            BindLocation = szi.Location;
        }
        public void StartGame()
        {
            Map temp = Envir.GetMap(CurrentMapIndex, GetRegionID());

            if (temp != null && temp.Info.NoReconnect)
            {
                Map temp1 = Envir.GetMapByNameAndInstance(temp.Info.NoReconnectMap, GetRegionID());
                if (temp1 != null)
                {
                    temp = temp1;
                    CurrentLocation = GetRandomPoint(40, 0, temp);
                }
            }

            if (temp == null || !temp.ValidPoint(CurrentLocation))
            {
                temp = Envir.GetMap(BindMapIndex, GetRegionID());

                if (temp == null || !temp.ValidPoint(BindLocation))
                {
                    SetBind();
                    temp = Envir.GetMap(BindMapIndex, GetRegionID());

                    if (temp == null || !temp.ValidPoint(BindLocation))
                    {
                        StartGameFailed();
                        return;
                    }
                }
                CurrentMapIndex = BindMapIndex;
                CurrentLocation = BindLocation;
            }
            temp.AddObject(this);
            CurrentMap = temp;
            Envir.Players.Add(this);

            StartGameSuccess();

            //Call Login NPC
            CallDefaultNPC(DefaultNPCType.Login);

            //Call Daily NPC
            if (Info.NewDay)
            {
                CallDefaultNPC(DefaultNPCType.Daily);
            }
        }
        private void StartGameSuccess()
        {
            Connection.Stage = GameStage.Game;
            for (int i = 0; i < Info.Magics.Count; i++)
            {
                if (Info.Magics[i].CastTime == 0) continue;
                long TimeSpend = Info.Magics[i].GetDelay() - Info.Magics[i].CastTime;
                if (TimeSpend < 0)
                {
                    Info.Magics[i].CastTime = 0;
                    continue;
                    //avoid having server owners lower the delays and bug it :p
                }
                Info.Magics[i].CastTime = Envir.Time > TimeSpend ? Envir.Time - TimeSpend : 0;
            }
            Enqueue(new S.StartGame { Result = 4, Resolution = Settings.AllowedResolution });
            ReceiveChatTr("Welcome to the Legend of Mir 2 Crystal Server.", ChatType.Hint);

            if (Settings.TestServer)
            {
                ReceiveChat("Game is currently in test mode.", ChatType.Hint);
                Chat("@GAMEMASTER");
            }

            if (Info.GuildIndex != -1)
            {
                //MyGuild = Envir.GetGuild(Info.GuildIndex);
                if (MyGuild == null)
                {
                    Info.GuildIndex = -1;
                    ReceiveChat("You have been removed from the guild.", ChatType.System);
                }
                else
                {
                    MyGuildRank = MyGuild.FindRank(Info.Name);
                    if (MyGuildRank == null)
                    {
                        MyGuild = null;
                        Info.GuildIndex = -1;
                        ReceiveChat("You have been removed from the guild.", ChatType.System);
                    }
                }
            }

            Spawned();

            SetLevelEffects();

            GetItemInfo();
            GetMapInfo();
            GetUserInfo();
            GetQuestInfo();
            GetRecipeInfo();

            GetCompletedQuests();

            GetMail();
            GetFriends();
            GetRelationship();

            if ((Info.Mentor != 0) && (Info.MentorDate.AddDays(Settings.MentorLength) < DateTime.Now))
                MentorBreak();
            else
                GetMentor();

            CheckConquest();

            GetGameShop();

            for (int i = 0; i < CurrentQuests.Count; i++)
            {
                CurrentQuests[i].ResyncTasks();
                SendUpdateQuest(CurrentQuests[i], QuestState.Add);
            }

            Enqueue(new S.BaseStatsInfo { Stats = Settings.ClassBaseStats[Class] });
            GetObjectsPassive();
            Enqueue(new S.TimeOfDay { Lights = Envir.Lights });
            Enqueue(new S.ChangeAMode { Mode = AMode });
            //if (Class == MirClass.Wizard || Class == MirClass.Taoist)//why could an war, sin, archer not have pets?
            Enqueue(new S.ChangePMode { Mode = PMode });
            Enqueue(new S.SwitchGroup { AllowGroup = AllowGroup });

            Enqueue(new S.DefaultNPC { ObjectID = DefaultNPC.ObjectID });

            Enqueue(new S.GuildBuffList() { GuildBuffs = Settings.Guild_BuffList });
            RequestedGuildBuffInfo = true;

            if (Info.Thrusting) Enqueue(new S.SpellToggle { Spell = Spell.Thrusting, CanUse = true });
            if (Info.HalfMoon) Enqueue(new S.SpellToggle { Spell = Spell.HalfMoon, CanUse = true });
            if (Info.CrossHalfMoon) Enqueue(new S.SpellToggle { Spell = Spell.CrossHalfMoon, CanUse = true });
            if (Info.DoubleSlash) Enqueue(new S.SpellToggle { Spell = Spell.DoubleSlash, CanUse = true });

            UserMagic magic = GetMagic(Spell.PoisonShot);
            if (magic != null)
                Enqueue(new S.SpellToggle { Spell = Spell.PoisonShot, CanUse = magic.Toggle });

            for (int i = 0; i < Info.Pets.Count; i++)
            {
                PetInfo info = Info.Pets[i];

                MonsterObject monster = MonsterObject.GetMonster(Envir.GetMonsterInfo(info.MonsterIndex));

                if (monster == null) continue;

                monster.PetLevel = info.Level;
                monster.MaxPetLevel = info.MaxPetLevel;
                monster.PetExperience = info.Experience;

                monster.Master = this;
                Pets.Add(monster);

                monster.RefreshAll();
                if (!monster.Spawn(CurrentMap, Back))
                    monster.Spawn(CurrentMap, CurrentLocation);

                monster.SetHP(info.HP);

                if (!Settings.PetSave)
                {
                    if (info.Time < 1 || (Envir.Time > info.Time + (Settings.PetTimeOut * Settings.Minute))) monster.Die();
                }
            }

            Info.Pets.Clear();

            for (int i = 0; i < Info.Buffs.Count; i++)
            {
                Buff buff = Info.Buffs[i];
                buff.ExpireTime += Envir.Time;
                buff.Paused = false;

                AddBuff(buff);
            }

            Info.Buffs.Clear();

            for (int i = 0; i < Info.Poisons.Count; i++)
            {
                Poison poison = Info.Poisons[i];
                poison.TickTime += Envir.Time;
                //poison.Owner = this;

                ApplyPoison(poison, poison.Owner);
            }

            Info.Poisons.Clear();

            for (int i = 0; i < Info.NPCVar.Count; i++)
            {
                NPCVar.Add(Info.NPCVar[i]);
            }
            Info.NPCVar.Clear();

            if (MyGuild != null)
            {
                MyGuild.PlayerLogged(this, true);
                if (MyGuild.BuffList.Count > 0)
                    Enqueue(new S.GuildBuffList() { ActiveBuffs = MyGuild.BuffList });
            }

            if (TransformType > -1)
                Enqueue(new S.TransformUpdate { ObjectID = ObjectID, TransformType = TransformType });

            if (InSafeZone && Info.LastDate > DateTime.MinValue)
            {
                double totalMinutes = (Envir.Now - Info.LastDate).TotalMinutes;

                _restedCounter = (int)(totalMinutes * 60);
            }

            if (Info.Mail.Count > Settings.MailCapacity)
            {
                ReceiveChatTr("Your mailbox is overflowing.", ChatType.System);
            }

            Report.Connected(Connection.IPAddress);

            SMain.Enqueue(string.Format(Envir.Tr("{0} region {1} has connected."), Info.Name, GetRegionID()));

            if (!IsGM)
            {
                LastRankUpdate = Envir.Time;
                SMain.Envir.CheckRankUpdate(GetRegionID(), Info);
            }

            // HumUp
            if (Info.HumUp)
                AddHumUpBuff();
        }

        public void ReceiveChatTr(string msg, ChatType hint)
        {
            msg = Envir.Tr(msg);
            ReceiveChat(msg, hint);
        }

        private void StartGameFailed()
        {
            Enqueue(new S.StartGame { Result = 3 });
            CleanUp();
        }

        public void SetLevelEffects()
        {
            LevelEffects = LevelEffects.None;

            if (Info.Flags[990]) LevelEffects |= LevelEffects.Mist;
            if (Info.Flags[991]) LevelEffects |= LevelEffects.RedDragon;
            if (Info.Flags[992]) LevelEffects |= LevelEffects.BlueDragon;
        }
        public void GiveRestedBonus(int count)
        {
            if (count > 0)
            {
                Buff buff = Buffs.FirstOrDefault(e => e.Type == BuffType.Rested);

                long existingTime = 0;
                if (buff != null)
                {
                    existingTime = buff.ExpireTime - Envir.Time;
                }

                long duration = ((Settings.RestedBuffLength * Settings.Minute) * count) + existingTime;
                long maxDuration = (Settings.RestedBuffLength * Settings.Minute) * Settings.RestedMaxBonus;

                if (duration > maxDuration) duration = maxDuration;

                AddBuff(new Buff { Type = BuffType.Rested, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { Settings.RestedExpBonus } });
                _restedCounter = 0;
                ReceiveChat(string.Format(Envir.Tr("Give rested bouns {0}"), count), ChatType.Hint);
            }
        }

        public void Revive(uint hp, bool effect)
        {
            if (!Dead) return;

            Dead = false;
            SetHP((ushort)hp);

            CurrentMap.RemoveObject(this);
            Broadcast(new S.ObjectRemove { ObjectID = ObjectID });

            CurrentMap = this.CurrentMap;
            CurrentLocation = this.CurrentLocation;

            CurrentMap.AddObject(this);

            Enqueue(new S.MapChanged
            {
                FileName = CurrentMap.Info.FileName,
                Title = CurrentMap.Info.Title + "-" + CurrentMap.Region,
                MiniMap = CurrentMap.Info.MiniMap,
                BigMap = CurrentMap.Info.BigMap,
                Lights = CurrentMap.Info.Light,
                Location = CurrentLocation,
                Direction = Direction,
                MapDarkLight = CurrentMap.Info.MapDarkLight,
                Music = CurrentMap.Info.Music
            });

            GetObjects();

            Enqueue(new S.Revived());
            Broadcast(new S.ObjectRevived { ObjectID = ObjectID, Effect = effect });

            Fishing = false;
            Enqueue(GetFishInfo());
        }
        public void TownRevive()
        {
            if (!Dead) return;

            Map temp = Envir.GetMap(BindMapIndex, GetRegionID());
            Point bindLocation = BindLocation;

            if (Info.PKPoints >= 200)
            {
                temp = Envir.GetMapByNameAndInstance(Settings.PKTownMapName, GetRegionID(), 1);
                bindLocation = new Point(Settings.PKTownPositionX, Settings.PKTownPositionY);

                if (temp == null)
                {
                    temp = Envir.GetMap(BindMapIndex, GetRegionID());
                    bindLocation = BindLocation;
                }
            }

            if (temp == null || !temp.ValidPoint(bindLocation)) return;

            Dead = false;
            SetHP(MaxHP);
            SetMP(MaxMP);
            RefreshStats();

            CurrentMap.RemoveObject(this);
            Broadcast(new S.ObjectRemove { ObjectID = ObjectID });

            CurrentMap = temp;
            CurrentLocation = bindLocation;

            CurrentMap.AddObject(this);

            Enqueue(new S.MapChanged
            {
                FileName = CurrentMap.Info.FileName,
                Title = CurrentMap.Info.Title + "-" + CurrentMap.Region,
                MiniMap = CurrentMap.Info.MiniMap,
                BigMap = CurrentMap.Info.BigMap,
                Lights = CurrentMap.Info.Light,
                Location = CurrentLocation,
                Direction = Direction,
                MapDarkLight = CurrentMap.Info.MapDarkLight,
                Music = CurrentMap.Info.Music
            });

            GetObjects();
            Enqueue(new S.Revived());
            Broadcast(new S.ObjectRevived { ObjectID = ObjectID, Effect = true });


            InSafeZone = true;
            Fishing = false;
            Enqueue(GetFishInfo());
        }

        private void GetItemInfo()
        {
            UserItem item;
            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                item = Info.Inventory[i];
                if (item == null) continue;

                CheckItem(item);
            }

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                item = Info.Equipment[i];

                if (item == null) continue;

                CheckItem(item);
            }
        }
        private void GetUserInfo()
        {
            string guildname = MyGuild != null ? MyGuild.Name : "";
            string guildrank = MyGuild != null ? MyGuildRank.Name : "";
            S.UserInformation packet = new S.UserInformation
            {
                ObjectID = ObjectID,
                RealId = (uint)Info.Index,
                Name = Name,
                GuildName = guildname,
                GuildRank = guildrank,
                NameColour = GetNameColour(this),
                Class = Class,
                Gender = Gender,
                Level = Level,
                Location = CurrentLocation,
                Direction = Direction,
                Hair = Hair,
                HP = HP,
                MP = MP,

                Experience = Experience,
                MaxExperience = MaxExperience,

                LevelEffects = LevelEffects,

                Inventory = new UserItem[Info.Inventory.Length],
                Equipment = new UserItem[Info.Equipment.Length],
                Gold = Account.Gold,
                Credit = Account.Credit,
                HasExpandedStorage = Account.ExpandedStorageExpiryDate > Envir.Now ? true : false,
                ExpandedStorageExpiryTime = Account.ExpandedStorageExpiryDate
            };

            //Copy this method to prevent modification before sending packet information.
            for (int i = 0; i < Info.Magics.Count; i++)
                packet.Magics.Add(Info.Magics[i].CreateClientMagic());

            Info.Inventory.CopyTo(packet.Inventory, 0);
            Info.Equipment.CopyTo(packet.Equipment, 0);

            //IntelligentCreature
            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
                packet.IntelligentCreatures.Add(Info.IntelligentCreatures[i].CreateClientIntelligentCreature());
            packet.SummonedCreatureType = SummonedCreatureType;
            packet.CreatureSummoned = CreatureSummoned;
            packet.HumUp = Info.HumUp;

            Enqueue(packet);
        }
        private void GetMapInfo()
        {
            Enqueue(new S.MapInformation
            {
                FileName = CurrentMap.Info.FileName,
                Title = CurrentMap.Info.Title + "-" + CurrentMap.Region,
                MiniMap = CurrentMap.Info.MiniMap,
                Lights = CurrentMap.Info.Light,
                BigMap = CurrentMap.Info.BigMap,
                Lightning = CurrentMap.Info.Lightning,
                Fire = CurrentMap.Info.Fire,
                MapDarkLight = CurrentMap.Info.MapDarkLight,
                Music = CurrentMap.Info.Music,
            });
        }

        private void GetQuestInfo()
        {
            for (int i = 0; i < Envir.QuestInfoList.Count; i++)
            {
                CheckQuestInfo(Envir.QuestInfoList[i]);
            }
        }
        private void GetRecipeInfo()
        {
            for (int i = 0; i < Envir.RecipeInfoList.Count; i++)
            {
                CheckRecipeInfo(Envir.RecipeInfoList[i]);
            }
        }
        private void GetObjects()
        {
            for (int y = CurrentLocation.Y - Globals.DataRange; y <= CurrentLocation.Y + Globals.DataRange; y++)
            {
                if (y < 0) continue;
                if (y >= CurrentMap.Height) break;

                for (int x = CurrentLocation.X - Globals.DataRange; x <= CurrentLocation.X + Globals.DataRange; x++)
                {
                    if (x < 0) continue;
                    if (x >= CurrentMap.Width) break;
                    if (x < 0 || x >= CurrentMap.Width) continue;

                    var cellObjects = CurrentMap.GetCellObjects(x, y);

                    if (cellObjects == null) continue;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject ob = cellObjects[i];

                        //if (ob.Race == ObjectType.Player && ob.Observer) continue;

                        ob.Add(this);
                    }
                }
            }
        }
        private void GetObjectsPassive()
        {
            for (int y = CurrentLocation.Y - Globals.DataRange; y <= CurrentLocation.Y + Globals.DataRange; y++)
            {
                if (y < 0) continue;
                if (y >= CurrentMap.Height) break;

                for (int x = CurrentLocation.X - Globals.DataRange; x <= CurrentLocation.X + Globals.DataRange; x++)
                {
                    if (x < 0) continue;
                    if (x >= CurrentMap.Width) break;
                    if (x < 0 || x >= CurrentMap.Width) continue;

                    var cellObjects = CurrentMap.GetCellObjects(x, y);

                    if (cellObjects == null) continue;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject ob = cellObjects[i];
                        if (ob == this) continue;

                        if (ob.Race == ObjectType.Deco)
                        {
                            var tt = 0;

                            tt++;
                        }
                        //if (ob.Race == ObjectType.Player && ob.Observer) continue;
                        if (ob.Race == ObjectType.Player)
                        {
                            PlayerObject Player = (PlayerObject)ob;
                            Enqueue(Player.GetInfoEx(this));
                        }
                        else if (ob.Race == ObjectType.Spell)
                        {
                            SpellObject obSpell = (SpellObject)ob;
                            if ((obSpell.Spell != Spell.ExplosiveTrap) || (IsFriendlyTarget(obSpell.Caster)))
                                Enqueue(ob.GetInfo());
                        }
                        else if (ob.Race == ObjectType.Merchant)
                        {
                            NPCObject NPC = (NPCObject)ob;

                            NPC.CheckVisible(this);

                            if (NPC.VisibleLog[Info.Index] && NPC.Visible) Enqueue(ob.GetInfo());
                        }
                        else
                        {
                            Enqueue(ob.GetInfo());
                        }

                        if (ob.Race == ObjectType.Player || ob.Race == ObjectType.Monster)
                            ob.SendHealth(this);
                    }
                }
            }
        }

        #region Refresh Stats

        public void RefreshStats()
        {
            if (!HasUpdatedBaseStats)
            {
                Enqueue(new S.BaseStatsInfo { Stats = Settings.ClassBaseStats[(MirClass)Class] });
                HasUpdatedBaseStats = true;
            }
            RefreshLevelStats();
            RefreshBagWeight();
            RefreshEquipmentStats();
            RefreshItemSetStats();
            RefreshMirSetStats();
            RefreshSkills();
            RefreshBuffs();
            RefreshStatCaps();
            RefreshMountStats();
            RefreshGuildBuffs();

            //Location Stats ?

            if (HP > MaxHP) SetHP(MaxHP);
            if (MP > MaxMP) SetMP(MaxMP);

            // 800
            AttackSpeed = 1400 - ((ASpeed * 40) + Math.Min(370, (Level * 14)));

            if (AttackSpeed < 400) AttackSpeed = 400;
        }

        private void RefreshLevelStats()
        {
            MaxExperience = Level < Settings.ExperienceList.Count ? Settings.ExperienceList[Level - 1] : 0;
            MaxHP = 0; MaxMP = 0;
            MinAC = 0; MaxAC = 0;
            MinMAC = 0; MaxMAC = 0;
            MinDC = 0; MaxDC = 0;
            MinMC = 0; MaxMC = 0;
            MinSC = 0; MaxSC = 0;

            BaseStats stats = Settings.ClassBaseStats[Class];
            Accuracy = stats.StartAccuracy;
            Agility = stats.StartAgility;
            CriticalRate = stats.StartCriticalRate;
            CriticalDamage = stats.StartCriticalDamage;
            //Other Stats;
            MaxBagWeight = 0;
            MaxWearWeight = 0;
            MaxHandWeight = 0;
            ASpeed = 0;
            Luck = 0;
            LifeOnHit = 0;
            HpDrainRate = 0;
            ReflectRate = 0;
            MagicResist = 0;
            PoisonResist = 0;
            HealthRecovery = 0;
            SpellRecovery = 0;
            PoisonRecovery = 0;
            Holy = 0;
            Freezing = 0;
            PoisonAttack = 0;

            ExpRateOffset = 0;
            ItemDropRateOffset = 0;
            MineRate = 0;
            GemRate = 0;
            FishRate = 0;
            CraftRate = 0;
            GoldDropRateOffset = 0;

            AttackBonus = 0;

            DamageReduceRate = 0;

            MinAC = (ushort)Math.Min(ushort.MaxValue, stats.MinAc > 0 ? Level / stats.MinAc : 0);
            MaxAC = (ushort)Math.Min(ushort.MaxValue, stats.MaxAc > 0 ? Level / stats.MaxAc : 0);
            MinMAC = (ushort)Math.Min(ushort.MaxValue, stats.MinMac > 0 ? Level / stats.MinMac : 0);
            MaxMAC = (ushort)Math.Min(ushort.MaxValue, stats.MaxMac > 0 ? Level / stats.MaxMac : 0);
            MinDC = (ushort)Math.Min(ushort.MaxValue, stats.MinDc > 0 ? Level / stats.MinDc : 0);
            MaxDC = (ushort)Math.Min(ushort.MaxValue, stats.MaxDc > 0 ? Level / stats.MaxDc : 0);
            MinMC = (ushort)Math.Min(ushort.MaxValue, stats.MinMc > 0 ? Level / stats.MinMc : 0);
            MaxMC = (ushort)Math.Min(ushort.MaxValue, stats.MaxMc > 0 ? Level / stats.MaxMc : 0);
            MinSC = (ushort)Math.Min(ushort.MaxValue, stats.MinSc > 0 ? Level / stats.MinSc : 0);
            MaxSC = (ushort)Math.Min(ushort.MaxValue, stats.MaxSc > 0 ? Level / stats.MaxSc : 0);
            CriticalRate = (byte)Math.Min(byte.MaxValue, stats.CritialRateGain > 0 ? CriticalRate + (Level / stats.CritialRateGain) : CriticalRate);
            CriticalDamage = (byte)Math.Min(byte.MaxValue, stats.CriticalDamageGain > 0 ? CriticalDamage + (Level / stats.CriticalDamageGain) : CriticalDamage);

            MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, (50 + Level / stats.BagWeightGain * Level));
            MaxWearWeight = (ushort)Math.Min(ushort.MaxValue, 15 + Level / stats.WearWeightGain * Level);
            MaxHandWeight = (ushort)Math.Min(ushort.MaxValue, 12 + Level / stats.HandWeightGain * Level);
            switch (Class)
            {
                case MirClass.Warrior:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 11 + (Level * 3.5F) + (Level * stats.MpGainRate));
                    break;
                case MirClass.Wizard:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 13 + ((Level / 5F + 2F) * 2.2F * Level) + (Level * stats.MpGainRate));
                    break;
                case MirClass.Taoist:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 13 + Level / 8F * 2.2F * Level + (Level * stats.MpGainRate));
                    break;
                case MirClass.Assassin:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 12 + (3 + Level / 12F) * Level + (Level * stats.MpGainRate));
                    break;
                case MirClass.Archer:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 12 + (5 + Level / 10F) * Level + (Level * stats.MpGainRate));
                    break;
                case MirClass.Monk:
                    MaxHP = (ushort)Math.Min(ushort.MaxValue, 14 + (Level / stats.HpGain + stats.HpGainRate + Level / 20F) * Level);
                    MaxMP = (ushort)Math.Min(ushort.MaxValue, 11 + 3.3F * Level + (Level * stats.MpGainRate));
                    break;
            }

        }

        private void RefreshBagWeight()
        {
            CurrentBagWeight = 0;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];
                if (item != null)
                    CurrentBagWeight = (ushort)Math.Min(ushort.MaxValue, CurrentBagWeight + item.Weight);
            }
        }

        private void RefreshEquipmentStats()
        {
            short OldLooks_Weapon = Looks_Weapon;
            short OldLooks_WeaponEffect = Looks_WeaponEffect;
            short OldLooks_Armour = Looks_Armour;
            short Old_MountType = MountType;
            byte OldLooks_Wings = Looks_Wings;
            byte OldLight = Light;
            short Old_TransformType = TransformType;
            short WeaponTransform = 0;
            short ArmourTransform = 0;

            Looks_Armour = 0;
            Looks_Weapon = -1;
            Looks_WeaponEffect = 0;
            Looks_Wings = 0;
            Light = 0;
            CurrentWearWeight = 0;
            CurrentHandWeight = 0;
            MountType = -1;
            TransformType = -1;

            HasTeleportRing = false;
            HasProtectionRing = false;
            HasRevivalRing = false;
            HasClearRing = false;
            HasMuscleRing = false;
            HasParalysisRing = false;
            HasProbeNecklace = false;
            SkillNeckBoost = 1;
            FastRun = false;

            List<string> skillsToAdd = null;
            List<string> skillsToRemove = null;
            short Macrate = 0, Acrate = 0, HPrate = 0, MPrate = 0;
            ItemSets.Clear();
            MirSet.Clear();

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                UserItem temp = Info.Equipment[i];
                if (temp == null) continue;
                ItemInfo RealItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);
                if (RealItem.Type == ItemType.Weapon || RealItem.Type == ItemType.Torch)
                    CurrentHandWeight = (ushort)Math.Min(byte.MaxValue, CurrentHandWeight + temp.Weight);
                else
                    CurrentWearWeight = (ushort)Math.Min(byte.MaxValue, CurrentWearWeight + temp.Weight);

                if (temp.CurrentDura == 0 && temp.Info.Durability > 0) continue;


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
                MaxWearWeight = (ushort)Math.Max(ushort.MinValue, (Math.Min(byte.MaxValue, MaxWearWeight + RealItem.WearWeight)));
                MaxHandWeight = (ushort)Math.Max(ushort.MinValue, (Math.Min(byte.MaxValue, MaxHandWeight + RealItem.HandWeight)));
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
                ReflectRate = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, ReflectRate + RealItem.Reflect)));
                HpDrainRate = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, HpDrainRate + RealItem.HpDrainRate)));

                for (int x = 0; x < temp.RuneSlots.Length; ++x)
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
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Flame))
                    {
                        if (skillsToAdd == null)
                            skillsToAdd = new List<string>();
                        if (skillsToRemove == null)
                            skillsToRemove =  new List<string> { Settings.HealRing, Settings.FireRing, Settings.BlinkSkill };

                        skillsToAdd.Add(Settings.FireRing);
                        skillsToRemove.Remove(Settings.FireRing);
                    }
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Healing))
                    {
                        if (skillsToAdd == null)
                            skillsToAdd = new List<string>();
                        if (skillsToRemove == null)
                            skillsToRemove =  new List<string> { Settings.HealRing, Settings.FireRing, Settings.BlinkSkill };

                        skillsToAdd.Add(Settings.HealRing);
                        skillsToRemove.Remove(Settings.HealRing);
                    }
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Probe)) HasProbeNecklace = true;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Skill)) SkillNeckBoost = 3;
                    if (RealItem.Unique.HasFlag(SpecialItemMode.Blink))
                    {
                        if (skillsToAdd == null)
                            skillsToAdd = new List<string>();
                        if (skillsToRemove == null)
                            skillsToRemove =  new List<string> { Settings.HealRing, Settings.FireRing, Settings.BlinkSkill };

                        skillsToAdd.Add(Settings.BlinkSkill);
                        skillsToRemove.Remove(Settings.BlinkSkill);
                    }
                }
                if (RealItem.CanFastRun)
                {
                    FastRun = true;
                }

                if (RealItem.Type == ItemType.Armour)
                {
                    ArmourTransform = temp.Transform;
                    Looks_Armour = RealItem.Shape;
                    Looks_Wings = RealItem.Effect;
                    if (RealItem.NeedHumUp)
                        Looks_HumUp = true;
                }

                if (RealItem.Type == ItemType.Weapon)
                {
                    WeaponTransform = temp.Transform;
                    Looks_Weapon = RealItem.Shape;
                    Looks_WeaponEffect = RealItem.Effect;
                    if (RealItem.NeedHumUp)
                        Looks_HumUp = true;
                }

                if (RealItem.Type == ItemType.Mount)
                {
                    MountType = RealItem.Shape;
                    //RealItem.Effect;
                }

                if (RealItem.Set != ItemSet.None)
                {
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

                if (RealItem.Type == ItemType.Transform)
                    TransformType = RealItem.Shape;
            }

            if ((WeaponTransform > 0 && ArmourTransform > 0))
            {
                Looks_HumUp = true;
            }

            if (Looks_HumUp)
            {
                if (WeaponTransform > 0)
                    Looks_Weapon = WeaponTransform;

                if (ArmourTransform > 0)
                    Looks_Armour = ArmourTransform;
            }

            MaxHP = (ushort)Math.Min(ushort.MaxValue, (((double)HPrate / 100) + 1) * MaxHP);
            MaxMP = (ushort)Math.Min(ushort.MaxValue, (((double)MPrate / 100) + 1) * MaxMP);
            MaxAC = (ushort)Math.Min(ushort.MaxValue, (((double)Acrate / 100) + 1) * MaxAC);
            MaxMAC = (ushort)Math.Min(ushort.MaxValue, (((double)Macrate / 100) + 1) * MaxMAC);

            AddTempSkills(skillsToAdd);
            RemoveTempSkills(skillsToRemove);

            if (HasMuscleRing)
            {
                MaxBagWeight = (ushort)(MaxBagWeight * 2);
                MaxWearWeight = Math.Min(ushort.MaxValue, (ushort)(MaxWearWeight * 2));
                MaxHandWeight = Math.Min(ushort.MaxValue, (ushort)(MaxHandWeight * 2));
            }
            if ((OldLooks_Armour != Looks_Armour) || (OldLooks_Weapon != Looks_Weapon) || (OldLooks_WeaponEffect != Looks_WeaponEffect) || (OldLooks_Wings != Looks_Wings) || (OldLight != Light))
            {
                Broadcast(GetUpdateInfo());

                if ((OldLooks_Weapon == 49 || OldLooks_Weapon == 50) && (Looks_Weapon != 49 && Looks_Weapon != 50))
                {
                    Enqueue(GetFishInfo());
                }
            }

            if (Old_MountType != MountType)
            {
                RefreshMount(false);
            }

            if (Old_TransformType != TransformType)
            {
                Enqueue(new S.TransformUpdate { ObjectID = ObjectID, TransformType = TransformType } );
                Broadcast(new S.TransformUpdate { ObjectID = ObjectID, TransformType = TransformType });
            }
        }

        public void ChangeEquip(List<string> param)
        {
            int type;
            if (!int.TryParse(param[0], out type)) return;
            short transform;
            ushort image;
            if (!short.TryParse(param[1], out transform)) return;
            if (!ushort.TryParse(param[2], out image)) return;

            if (type == 0)
            {
                UserItem item = Info.Equipment[(int)EquipmentSlot.Weapon];
                if (item != null)
                {
                    item.Transform = transform;
                    item.TransformImage = image;
                    Enqueue(new S.RefreshItem { Item = item });
                    ReceiveChatTr("Transform weapon successful!", ChatType.Hint);
                }
                else
                {
                    ReceiveChatTr("Transform weapon failed!", ChatType.Hint);
                }
            }
            else
            {
                UserItem item = Info.Equipment[(int)EquipmentSlot.Armour];
                if (item != null)
                {
                    item.Transform = transform;
                    item.TransformImage = image;
                    Enqueue(new S.RefreshItem { Item = item });
                    ReceiveChatTr("Transform armour successful!", ChatType.Hint);
                }
                else
                {
                    ReceiveChatTr("Transform armour failed!", ChatType.Hint);
                }
            }

            RefreshStats();
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
                        ReflectRate = (byte)Math.Min(byte.MaxValue, ReflectRate + 5);
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
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + (((double)MaxHP / 100) * 30));
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
                        ASpeed = (sbyte)Math.Min(int.MaxValue, ASpeed + 2);
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
                ASpeed = (sbyte)Math.Min(int.MaxValue, ASpeed + 2);
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

        public void RefreshStatCaps()
        {
            MagicResist = Math.Min(Settings.MaxMagicResist, MagicResist);
            PoisonResist = Math.Min(Settings.MaxPoisonResist, PoisonResist);
            CriticalRate = Math.Min(Settings.MaxCriticalRate, CriticalRate);
            CriticalDamage = Math.Min(Settings.MaxCriticalDamage, CriticalDamage);
            Freezing = Math.Min(Settings.MaxFreezing, Freezing);
            PoisonAttack = Math.Min(Settings.MaxPoisonAttack, PoisonAttack);
            HealthRecovery = Math.Min(Settings.MaxHealthRegen, HealthRecovery);
            PoisonRecovery = Math.Min(Settings.MaxPoisonRecovery, PoisonRecovery);
            SpellRecovery = Math.Min(Settings.MaxManaRegen, SpellRecovery);
            HpDrainRate = Math.Min(Settings.MaxHpDrainRate, HpDrainRate);
            ReflectRate = Math.Min(Settings.MaxReflect, ReflectRate);
        }

        public void RefreshMountStats()
        {
            if (!Mount.HasMount) return;

            UserItem[] Slots = Mount.Slots;

            for (int i = 0; i < Slots.Length; i++)
            {
                UserItem temp = Slots[i];
                if (temp == null) continue;

                ItemInfo RealItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

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

        #endregion

        private void AddTempSkills(IEnumerable<string> skillsToAdd)
        {
            if (skillsToAdd == null)
                return;

            foreach (var skill in skillsToAdd)
            {
                Spell spelltype;
                bool hasSkill = false;

                if (!Enum.TryParse(skill, out spelltype)) return;

                for (var i = Info.Magics.Count - 1; i >= 0; i--)
                    if (Info.Magics[i].Spell == spelltype) hasSkill = true;

                if (hasSkill) continue;

                var magic = new UserMagic(spelltype) { IsTempSpell = true };
                Info.Magics.Add(magic);
                Enqueue(magic.GetInfo());
            }
        }
        private void RemoveTempSkills(IEnumerable<string> skillsToRemove)
        {
            if (skillsToRemove == null)
                return;

            foreach (var skill in skillsToRemove)
            {
                Spell spelltype;
                if (!Enum.TryParse(skill, out spelltype)) return;

                for (var i = Info.Magics.Count - 1; i >= 0; i--)
                {
                    if (!Info.Magics[i].IsTempSpell || Info.Magics[i].Spell != spelltype) continue;

                    Info.Magics.RemoveAt(i);
                    Enqueue(new S.RemoveMagic { PlaceId = i });
                }
            }
        }

        private void RefreshSkills()
        {
            for (int i = 0; i < Info.Magics.Count; i++)
            {
                UserMagic magic = Info.Magics[i];
                switch (magic.Spell)
                {
                    case Spell.Fencing:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level * 3 + 3);
                        break;
                    case Spell.Slaying:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level);
                        break;
                    case Spell.Focus:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level + 1);
                        break;
                    case Spell.FatalSword:
                        Accuracy = (byte)Math.Min(byte.MaxValue, Accuracy + magic.Level + 1);
                        //    Agility = (byte)Math.Min(ushort.MaxValue, Agility + magic.Level + 1);
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
            NoMove = false;
            for (int i = 0; i < Buffs.Count; i++)
            {
                Buff buff = Buffs[i];

                if (buff.Values == null || buff.Values.Length < 1 || buff.Paused) continue;

                switch (buff.Type)
                {
                    case BuffType.Haste:
                    case BuffType.Fury:
                        ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + buff.Values[0])));
                        break;
                    case BuffType.ImmortalSkin:
                        DamageReduceRate = (byte)buff.Values[0];
                        MaxDC = (ushort)Math.Max(0, (MaxDC * (100 - buff.Values[1]) / 100));
                        break;
                    case BuffType.DogYoLin7:
                        NoMove = true;
                        break;
                    case BuffType.SwiftFeet:
                        ActiveSwiftFeet = true;
                        break;
                    case BuffType.LightBody:
                        Agility = (byte)Math.Min(byte.MaxValue, Agility + buff.Values[0]);
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
                        ReflectRate = (byte)Math.Min(byte.MaxValue, ReflectRate + buff.Values[0]);
                        break;

                    case BuffType.CounterAttack1:
                        MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[0]);
                        MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + buff.Values[0]);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        ReflectRate = (byte)Math.Min(byte.MaxValue, ReflectRate + buff.Values[1]);
                        break;

                    case BuffType.HumUp://stupple
                        MaxHP = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxHP + buff.Values[0]));
                        MaxMP = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxMP + buff.Values[1]));
                        HealthRecovery = (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, HealthRecovery + buff.Values[2]));
                        SpellRecovery = (byte)Math.Min(byte.MaxValue, Math.Max(byte.MinValue, SpellRecovery + buff.Values[3]));
                        MaxBagWeight = (ushort)Math.Min(ushort.MaxValue, Math.Max(ushort.MinValue, MaxBagWeight + buff.Values[4]));
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
                    case BuffType.MagicBooster:
                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + buff.Values[0]);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[0]);
                        break;

                    case BuffType.General:
                        ExpRateOffset = (float)Math.Min(float.MaxValue, ExpRateOffset + buff.Values[0]);

                        if (buff.Values.Length > 1)
                            ItemDropRateOffset = (float)Math.Min(float.MaxValue, ItemDropRateOffset + buff.Values[1]);
                        if (buff.Values.Length > 2)
                            GoldDropRateOffset = (float)Math.Min(float.MaxValue, GoldDropRateOffset + buff.Values[2]);
                        break;
                    case BuffType.Rested:
                    case BuffType.Exp:
                    case BuffType.Exp1:
                        ExpRateOffset = (float)Math.Min(float.MaxValue, ExpRateOffset + buff.Values[0]);
                        break;
                    case BuffType.Drop:
                        ItemDropRateOffset = (float)Math.Min(float.MaxValue, ItemDropRateOffset + buff.Values[0]);
                        break;
                    case BuffType.Gold:
                        GoldDropRateOffset = (float)Math.Min(float.MaxValue, GoldDropRateOffset + buff.Values[0]);
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
                    case BuffType.WonderDrug:
                        switch (buff.Values[0])
                        {
                            case 0:
                                ExpRateOffset = (float)Math.Min(float.MaxValue, ExpRateOffset + buff.Values[1]);
                                break;
                            case 1:
                                ItemDropRateOffset = (float)Math.Min(float.MaxValue, ItemDropRateOffset + buff.Values[1]);
                                break;
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
        public void RefreshGuildBuffs()
        {
            if (MyGuild == null) return;
            if (MyGuild.BuffList.Count == 0) return;
            for (int i = 0; i < MyGuild.BuffList.Count; i++)
            {
                GuildBuff Buff = MyGuild.BuffList[i];
                if ((Buff.Info == null) || (!Buff.Active)) continue;
                MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + Buff.Info.BuffAc);
                MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + Buff.Info.BuffMac);
                MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + Buff.Info.BuffDc);
                MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + Buff.Info.BuffMc);
                MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + Buff.Info.BuffSc);
                AttackBonus = (byte)Math.Min(byte.MaxValue, AttackBonus + Buff.Info.BuffAttack);
                MaxHP = (ushort)Math.Min(ushort.MaxValue, MaxHP + Buff.Info.BuffMaxHp);
                MaxMP = (ushort)Math.Min(ushort.MaxValue, MaxMP + Buff.Info.BuffMaxMp);
                MineRate = (byte)Math.Min(byte.MaxValue, MineRate + Buff.Info.BuffMineRate);
                GemRate = (byte)Math.Min(byte.MaxValue, GemRate + Buff.Info.BuffGemRate);
                FishRate = (byte)Math.Min(byte.MaxValue, FishRate + Buff.Info.BuffFishRate);
                ExpRateOffset = (float)Math.Min(float.MaxValue, ExpRateOffset + Buff.Info.BuffExpRate);
                CraftRate = (byte)Math.Min(byte.MaxValue, CraftRate + Buff.Info.BuffCraftRate); //needs coding
                SkillNeckBoost = (byte)Math.Min(byte.MaxValue, SkillNeckBoost + Buff.Info.BuffSkillRate);
                HealthRecovery = (byte)Math.Min(byte.MaxValue, HealthRecovery + Buff.Info.BuffHpRegen);
                SpellRecovery = (byte)Math.Min(byte.MaxValue, SpellRecovery + Buff.Info.BuffMPRegen);
                ItemDropRateOffset = (float)Math.Min(float.MaxValue, ItemDropRateOffset + Buff.Info.BuffDropRate);
                GoldDropRateOffset = (float)Math.Min(float.MaxValue, GoldDropRateOffset + Buff.Info.BuffGoldRate);
            }
        }
        public void RefreshNameColour()
        {
            Color colour = Color.White;

            if (PKPoints >= 200)
                colour = Color.Red;
            else if (WarZone)
            {
                if (MyGuild == null)
                    colour = Color.Green;
                else
                    colour = Color.Blue;
            }
            else if (Envir.Time < BrownTime)
                colour = Color.SaddleBrown;
            else if (PKPoints >= 100)
                colour = Color.Yellow;

            if (colour == NameColour) return;

            NameColour = colour;
            if ((MyGuild == null) || (!MyGuild.IsAtWar()))
                Enqueue(new S.ColourChanged { NameColour = NameColour });

            BroadcastColourChange();
        }

        public Color GetNameColour(PlayerObject player)
        {
            if (player == null) return NameColour;

            if (WarZone)
            {
                if (MyGuild == null)
                    return Color.Green;
                else
                {
                    if (player.MyGuild == null)
                        return Color.Orange;
                    if (player.MyGuild == MyGuild)
                        return Color.Blue;
                    else
                        return Color.Orange;
                }
            }

            if (MyGuild != null)
                if (MyGuild.IsAtWar())
                    if (player.MyGuild == MyGuild)
                        return Color.Blue;
                    else
                        if (MyGuild.IsEnemy(player.MyGuild))
                        return Color.Orange;
            return NameColour;
        }

        public void BroadcastColourChange()
        {
            if (CurrentMap == null) return;

            for (int i = CurrentMap.Players.Count - 1; i >= 0; i--)
            {
                PlayerObject player = CurrentMap.Players[i];
                if (player == this) continue;

                if (Functions.InRange(CurrentLocation, player.CurrentLocation, Globals.DataRange))
                    player.Enqueue(new S.ObjectColourChanged { ObjectID = ObjectID, NameColour = GetNameColour(player) });
            }
        }

        public override void BroadcastInfo()
        {
            Packet p;
            if (CurrentMap == null) return;

            for (int i = CurrentMap.Players.Count - 1; i >= 0; i--)
            {
                PlayerObject player = CurrentMap.Players[i];
                if (player == this) continue;

                if (Functions.InRange(CurrentLocation, player.CurrentLocation, Globals.DataRange))
                {
                    p = GetInfoEx(player);
                    if (p != null)
                        player.Enqueue(p);
                }
            }
        }

        private void Kill(string[] parts)
        {
            if (!IsGM) return;

            PlayerObject player;
            if (parts.Length >= 2)
            {
                player = Envir.GetPlayer(parts[1]);

                if (player == null)
                {
                    ReceiveChat(string.Format(Envir.Tr("Could not find {0}"), parts[0]), ChatType.System);
                    return;
                }
                if (!player.GMNeverDie) player.Die();
            }
            else
            {
                if (!CurrentMap.ValidPoint(Front)) return;

                var cellObjects = CurrentMap.GetCellObjects(Front);

                if (cellObjects == null) return;

                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];

                    switch (ob.Race)
                    {
                        case ObjectType.Player:
                        case ObjectType.Monster:
                            if (ob.Dead) continue;
                            ob.EXPOwner = this;
                            ob.ExpireTime = Envir.Time + MonsterObject.EXPOwnerDelay;
                            ob.Die();
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        private void Restore(string[] parts)
        {
            if (!IsGM || parts.Length < 2) return;
            CharacterInfo data = Envir.GetCharacterInfo(parts[1]);

            if (data == null)
            {
                ReceiveChat(string.Format(Envir.Tr("Player {0} was not found"), parts[1]), ChatType.System);
                return;
            }

            if (!data.Deleted) return;
            data.Deleted = false;

            ReceiveChat(string.Format(Envir.Tr("Player {0} has been restored by"), data.Name), ChatType.System);
            SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been restored by {1}"), data.Name, Name));
        }

        private void ChangeGender(string[] parts)
        {
            if (!IsGM && !Settings.TestServer) return;

            CharacterInfo data = parts.Length < 2 ? Info : Envir.GetCharacterInfo(parts[1]);

            if (data == null) return;

            switch (data.Gender)
            {
                case MirGender.Male:
                    data.Gender = MirGender.Female;
                    break;
                case MirGender.Female:
                    data.Gender = MirGender.Male;
                    break;
            }

            ReceiveChat(string.Format(Envir.Tr("Player {0} has been changed to {1}"), data.Name, data.Gender), ChatType.System);
            SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been changed to {1} by {2}"), data.Name, data.Gender, Name));

            if (data.Player != null)
                data.Player.Connection.LogOut();
        }

        private void ChangeLevel(string[] parts)
        {
            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
            PlayerObject player;
            ushort level;
            ushort old;
            if (parts.Length >= 3)
            {
                if (!IsGM) return;

                if (ushort.TryParse(parts[2], out level))
                {
                    if (level == 0) return;
                    player = Envir.GetPlayer(parts[1]);
                    if (player == null) return;
                    old = player.Level;
                    player.Level = level;
                    player.LevelUp();

                    ReceiveChat(string.Format(Envir.Tr("Player {0} has been Leveled {1} -> {2}."), player.Name, old, player.Level), ChatType.System);
                    SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been Leveled {1} -> {2} by {3}"), player.Name, old, player.Level, Name));
                    return;
                }
            }
            else
            {
                if (parts[1] == "-1")
                {
                    parts[1] = ushort.MaxValue.ToString();
                }

                if (ushort.TryParse(parts[1], out level))
                {
                    if (level == 0) return;
                    old = Level;
                    Level = level;
                    LevelUp();

                    ReceiveChat(string.Format(Envir.Tr("Leveled {0} -> {1}."), old, Level), ChatType.System);
                    SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been Leveled {1} -> {2} by {3}"), Name, old, Level, Name));
                    return;
                }
            }

            ReceiveChat(Envir.Tr("Could not level player"), ChatType.System);
        }

        private void Make(string[] parts)
        {
            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

            ItemInfo iInfo = Envir.GetItemInfo(parts[1]);
            if (iInfo == null) return;

            uint count = 1;
            if (parts.Length >= 3 && !uint.TryParse(parts[2], out count))
                count = 1;

            UserItem item;
            var tempCount = count;

            while (count > 0)
            {
                if (iInfo.StackSize >= count)
                {
                    item = Envir.CreateDropItem(iInfo);
                    item.Count = count;

                    if (CanGainItem(item, false)) GainItem(item);

                    return;
                }
                item = Envir.CreateDropItem(iInfo);
                item.Count = iInfo.StackSize;
                count -= iInfo.StackSize;

                if (!CanGainItem(item, false)) return;
                GainItem(item);
            }

            ReceiveChat(string.Format(Envir.Tr("{0} x{1} has been created."), iInfo.Name, tempCount), ChatType.System);
            SMain.Enqueue(string.Format(Envir.Tr("Player {0} has attempted to Create {1} x{2}"), Name, iInfo.Name, tempCount));
        }

        private void GiveGold(string[] parts)
        {
            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
            PlayerObject player;
            uint count;
            player = this;

            if (parts.Length > 2)
            {
                if (!IsGM) return;

                if (!uint.TryParse(parts[2], out count)) return;
                player = Envir.GetPlayer(parts[1]);

                if (player == null)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[1]), ChatType.System);
                    return;
                }
            }

            else if (!uint.TryParse(parts[1], out count)) return;

            if (count + player.Account.Gold >= uint.MaxValue)
                count = uint.MaxValue - player.Account.Gold;

            player.GainGold(count);
            string str = string.Format(Envir.Tr("Player {0} has been given {1} gold"), player.Name, count);
            SMain.Enqueue(str);
            ReceiveChat(str, ChatType.System);
        }

        private void GroupCall()
        {
            if (GroupMembers == null || GroupMembers[0] != this || Dead)
                return;

            if (CurrentMap.Info.NoRecall)
            {
                ReceiveChat(Envir.Tr("You cannot recall people on this map"), ChatType.System);
                return;
            }

            if (Envir.Time < LastRecallTime)
            {
                ReceiveChat(string.Format(Envir.Tr("You cannot recall for another {0} seconds"), (LastRecallTime - Envir.Time) / 1000), ChatType.System);
                return;
            }

            if (ItemSets.Any(set => set.Set == ItemSet.Recall && set.SetComplete))
            {
                LastRecallTime = Envir.Time + 180000;
                for (var i = 1; i < GroupMembers.Count(); i++)
                {
                    if (GroupMembers[i].EnableGroupRecall)
                        GroupMembers[i].Teleport(CurrentMap, CurrentLocation);
                    else
                        GroupMembers[i].ReceiveChat(Envir.Tr("A recall was attempted without your permission"),
                            ChatType.System);
                }
            }
        }

        private void RecallMember(string[] parts)
        {
            if (GroupMembers == null || GroupMembers[0] != this)
            {
                ReceiveChat(Envir.Tr("You are not a group leader."), ChatType.System);
                return;
            }

            if (Dead)
            {
                ReceiveChat(Envir.Tr("You cannot recall when you are dead."), ChatType.System);
                return;
            }

            if (CurrentMap.Info.NoRecall)
            {
                ReceiveChat(Envir.Tr("You cannot recall people on this map"), ChatType.System);
                return;
            }

            if (Envir.Time < LastRecallTime)
            {
                ReceiveChat(string.Format(Envir.Tr("You cannot recall for another {0} seconds"), (LastRecallTime - Envir.Time) / 1000), ChatType.System);
                return;
            }
            if (ItemSets.Any(set => set.Set == ItemSet.Recall && set.SetComplete))
            {
                if (parts.Length < 2) return;
                PlayerObject player = Envir.GetPlayer(parts[1]);

                if (player == null || !IsMember(player) || this == player)
                {
                    ReceiveChat((string.Format(Envir.Tr("Player {0} could not be found"), parts[1])), ChatType.System);
                    return;
                }
                if (!player.EnableGroupRecall)
                {
                    player.ReceiveChat(Envir.Tr("A recall was attempted without your permission"),
                            ChatType.System);
                    ReceiveChat((string.Format(Envir.Tr("{0} is blocking grouprecall"), player.Name)), ChatType.System);
                    return;
                }
                LastRecallTime = Envir.Time + 60000;

                if (!player.Teleport(CurrentMap, Front))
                    player.Teleport(CurrentMap, CurrentLocation);
            }
            else
            {
                ReceiveChat(Envir.Tr("You cannot recall without a recallset."), ChatType.System);
                return;
            }
        }

        private void RecallLover()
        {
            if (Info.Married == 0)
            {
                ReceiveChat(Envir.Tr("You're not married."), ChatType.System);
                return;
            }

            if (Dead)
            {
                ReceiveChat(Envir.Tr("You can't recall when you are dead."), ChatType.System);
                return;
            }

            if (CurrentMap.Info.NoRecall)
            {
                ReceiveChat(Envir.Tr("You cannot recall people on this map"), ChatType.System);
                return;
            }

            if (Info.Equipment[(int)EquipmentSlot.RingL] == null)
            {
                ReceiveChat(Envir.Tr("You need to be wearing a Wedding Ring for recall."), ChatType.System);
                return;
            }


            if (Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing == Info.Married)
            {
                CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);

                if (Lover == null) return;

                PlayerObject player = Envir.GetPlayer(Lover.Name);

                if (player == null)
                {
                    ReceiveChat((string.Format(Envir.Tr("{0} is not online."), Lover.Name)), ChatType.System);
                    return;
                }

                if (player.Dead)
                {
                    ReceiveChat(Envir.Tr("You can't recall a dead player."), ChatType.System);
                    return;
                }

                if (player.Info.Equipment[(int)EquipmentSlot.RingL] == null)
                {
                    player.ReceiveChat((string.Format(Envir.Tr("You need to wear a Wedding Ring for recall."), Lover.Name)), ChatType.System);
                    ReceiveChat((string.Format(Envir.Tr("{0} Isn't wearing a Wedding Ring."), Lover.Name)), ChatType.System);
                    return;
                }

                if (player.Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing != player.Info.Married)
                {
                    player.ReceiveChat((string.Format(Envir.Tr("You need to wear a Wedding Ring on your left finger for recall."), Lover.Name)), ChatType.System);
                    ReceiveChat((string.Format(Envir.Tr("{0} Isn't wearing a Wedding Ring."), Lover.Name)), ChatType.System);
                    return;
                }

                if (!player.AllowLoverRecall)
                {
                    player.ReceiveChat(Envir.Tr("A recall was attempted without your permission"),
                            ChatType.System);
                    ReceiveChat((string.Format(Envir.Tr("{0} is blocking Lover Recall."), player.Name)), ChatType.System);
                    return;
                }

                if ((Envir.Time < LastRecallTime) && (Envir.Time < player.LastRecallTime))
                {
                    ReceiveChat(string.Format(Envir.Tr("You cannot recall for another {0} seconds"), (LastRecallTime - Envir.Time) / 1000), ChatType.System);
                    return;
                }

                LastRecallTime = Envir.Time + 60000;
                player.LastRecallTime = Envir.Time + 60000;

                if (!player.Teleport(CurrentMap, Front))
                    player.Teleport(CurrentMap, CurrentLocation);
            }
            else
            {
                ReceiveChat(Envir.Tr("You cannot recall your lover without wearing a wedding ring"), ChatType.System);
                return;
            }
        }

        private void DeleteSkill(string[] parts)
        {
            if ((!IsGM) || parts.Length < 2) return;
            Spell skill1;

            if (!Enum.TryParse(parts.Length > 2 ? parts[2] : parts[1], true, out skill1)) return;

            if (skill1 == Spell.None) return;

            PlayerObject player;
            if (parts.Length > 2)
            {
                if (!IsGM) return;
                player = Envir.GetPlayer(parts[1]);

                if (player == null)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} was not found!"), parts[1]), ChatType.System);
                    return;
                }
            }
            else
                player = this;

            if (player == null) return;

            var magics = new UserMagic(skill1);
            bool removed = false;

            for (var i = player.Info.Magics.Count - 1; i >= 0; i--)
            {
                if (player.Info.Magics[i].Spell != skill1) continue;

                player.Info.Magics.RemoveAt(i);
                player.Enqueue(new S.RemoveMagic { PlaceId = i });
                removed = true;
            }

            if (removed)
            {
                ReceiveChat(string.Format(Envir.Tr("You have deleted skill {0} from player {1}"), skill1.ToString(), player.Name), ChatType.Hint);
                player.ReceiveChat(string.Format(Envir.Tr("{0} has been removed from you."), skill1), ChatType.Hint);
            }
            else ReceiveChat(string.Format(Envir.Tr("Unable to delete skill, skill not found")), ChatType.Hint);
        }

        private void ProcessCommand(string message)
        {
            //Command
            message = message.Remove(0, 1);
            string[] parts = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return;

            PlayerObject player;
            CharacterInfo data;
            String hintstring;
            UserItem item;

            String cmd = parts[0].ToUpper();
            if (cmd.Equals(Settings.AllowTrade))
            {
                AllowTrade = !AllowTrade;

                if (AllowTrade)
                    ReceiveChat(Envir.Tr("You are now allowing trade"), ChatType.System);
                else
                    ReceiveChat(Envir.Tr("You are no longer allowing trade"), ChatType.System);
            }
            else if (cmd.Equals(Settings.AllowGuild))
            {
                EnableGuildInvite = !EnableGuildInvite;
                hintstring = EnableGuildInvite ? Envir.Tr("Guild invites enabled.") : Envir.Tr("Guild invites disabled.");
                ReceiveChat(hintstring, ChatType.Hint);
            }
            else if (cmd.Equals(Settings.LeaveGuild))
            {
                    if (MyGuild == null) return;
                    if (MyGuildRank == null) return;
                    if (MyGuild.IsAtWar())
                    {
                        ReceiveChat(Envir.Tr("Cannot leave guild whilst at war."), ChatType.System);
                        return;
                    }

                    MyGuild.DeleteMember(this, Name);
            }
            else if (cmd.Equals(Settings.EnableGroupRecall))
            {
                EnableGroupRecall = !EnableGroupRecall;
                hintstring = EnableGroupRecall ? Envir.Tr("Group Recall Enabled.") : Envir.Tr("Group Recall Disabled.");
                ReceiveChat(hintstring, ChatType.Hint);
            }
            else if (cmd.Equals(Settings.GroupCall))
            {
                GroupCall();
            }
            else if (cmd.Equals(Settings.RecallMember))
            {
                RecallMember(parts);
            }
            else if (cmd.Equals(Settings.RecallLover))
            {
                RecallLover();
            }

            switch (parts[0].ToUpper())
            {
                case "LOGIN":
                    //GMLogin = true;
                    //ReceiveChat(Envir.Tr("Please type the GM Password."), ChatType.Hint);
                    return;

                case "KILL":
                    Kill(parts);
                    break;

                case "RESTORE":
                    Restore(parts);
                    break;

                case "CHANGEGENDER":
                    ChangeGender(parts);
                    break;

                case "LEVEL":
                    ChangeLevel(parts);
                    break;

                case "MAKE":
                    Make(parts);
                    break;

                case "CLEARBUFFS":
                    foreach (var buff in Buffs)
                    {
                        buff.Infinite = false;
                        buff.ExpireTime = 0;
                    }
                    break;

                case "CLEARBAG":
                    if (!IsGM && !Settings.TestServer) return;
                    player = this;

                    if (parts.Length >= 2)
                        player = Envir.GetPlayer(parts[1]);

                    if (player == null) return;
                    for (int i = 0; i < player.Info.Inventory.Length; i++)
                    {
                        item = player.Info.Inventory[i];
                        if (item == null) continue;

                        player.Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                        player.Info.Inventory[i] = null;
                        UpdateQuestItem(item.Info);
                    }
                    player.RefreshStats();
                    break;

                case "SUPERMAN":
                    if (!IsGM && !Settings.TestServer) return;

                    GMNeverDie = !GMNeverDie;

                    hintstring = GMNeverDie ? "Invincible Mode." : "Normal Mode.";
                    ReceiveChat(hintstring, ChatType.Hint);
                    UpdateGMBuff();
                    break;

                case "GAMEMASTER":
                    if (!IsGM && !Settings.TestServer) return;

                    GMGameMaster = !GMGameMaster;

                    hintstring = GMGameMaster ? "GameMaster Mode." : "Normal Mode.";
                    ReceiveChat(hintstring, ChatType.Hint);
                    UpdateGMBuff();
                    break;

                case "OBSERVER":
                    if (!IsGM) return;
                    Observer = !Observer;

                    hintstring = Observer ? "Observer Mode." : "Normal Mode.";
                    ReceiveChat(hintstring, ChatType.Hint);
                    UpdateGMBuff();
                    break;

                case "RECALL":
                    if (!IsGM) return;

                    if (parts.Length < 2) return;
                    player = Envir.GetPlayer(parts[1]);

                    if (player == null) return;

                    player.Teleport(CurrentMap, Front);
                    break;


                case "TIME":
                    ReceiveChat(string.Format(Envir.Tr("The time is : {0}"), DateTime.Now.ToString("hh:mm tt")), ChatType.System);
                    break;

                case "ROLL":
                    int diceNum = Envir.Random.Next(5) + 1;

                    if (GroupMembers == null) { return; }

                    for (int i = 0; i < GroupMembers.Count; i++)
                    {
                        PlayerObject playerSend = GroupMembers[i];
                        playerSend.ReceiveChat(string.Format(Envir.Tr("{0} has rolled a {1}"), Name, diceNum), ChatType.Group);
                    }
                    break;

                case "MAP":
                    var mapName = CurrentMap.Info.FileName;
                    var mapTitle = CurrentMap.Info.Title;
                    ReceiveChat((string.Format(Envir.Tr("You are currently in {0}. Map ID: {1}"), mapTitle, mapName)), ChatType.System);
                    break;

                case "SAVEPLAYER":
                    if (!IsGM) return;

                    if (parts.Length < 2) return;

                    CharacterInfo tempInfo = null;

                    System.IO.Directory.CreateDirectory(Envir.Tr("Character Backups"));

                    for (int i = 0; i < Envir.AccountList.Count; i++)
                    {
                        for (int j = 0; j < Envir.AccountList[i].Characters.Count; j++)
                        {
                            if (String.Compare(Envir.AccountList[i].Characters[j].Name, parts[1], StringComparison.OrdinalIgnoreCase) != 0) continue;

                            tempInfo = Envir.AccountList[i].Characters[j];
                            break;
                        }
                    }

                    using (System.IO.FileStream stream = System.IO.File.Create(string.Format(Envir.Tr("Character Backups/{0}"), tempInfo.Name)))
                    {
                        using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
                        {
                            tempInfo.Save(writer);
                        }
                    }

                    break;

                case "LOADPLAYER":
                    if (!IsGM) return;

                    if (parts.Length < 2) return;

                    tempInfo = null;

                    System.IO.Directory.CreateDirectory(Envir.Tr("Character Backups"));

                    for (int i = 0; i < Envir.AccountList.Count; i++)
                    {
                        for (int j = 0; j < Envir.AccountList[i].Characters.Count; j++)
                        {
                            if (String.Compare(Envir.AccountList[i].Characters[j].Name, parts[1], StringComparison.OrdinalIgnoreCase) != 0) continue;

                            tempInfo = Envir.AccountList[i].Characters[j];

                            using (System.IO.FileStream stream = System.IO.File.OpenRead(string.Format(Envir.Tr("Character Backups/{0}"), tempInfo.Name)))
                            {
                                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                                {
                                    CharacterInfo tt = new CharacterInfo(reader);

                                    if (Envir.AccountList[i].Characters[j].Index != tt.Index)
                                    {
                                        ReceiveChat(Envir.Tr("Player name was matched however IDs did not. Likely due to player being recreated. Player not restored"), ChatType.System);
                                        return;
                                    }

                                    Envir.AccountList[i].Characters[j] = tt;
                                }
                            }
                        }
                    }

                    Envir.BeginSaveAccounts();
                    break;

                case "MOVE":
                    if (!IsGM && !HasTeleportRing && !Settings.TestServer) return;
                    if (!IsGM && CurrentMap.Info.NoPosition)
                    {
                        ReceiveChat(Envir.Tr("You cannot position move on this map"), ChatType.System);
                        return;
                    }
                    if (Envir.Time < LastTeleportTime)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You cannot teleport for another {0} seconds"), (LastTeleportTime - Envir.Time) / 1000), ChatType.System);
                        return;
                    }

                    int x, y;

                    if (parts.Length <= 2 || !int.TryParse(parts[1], out x) || !int.TryParse(parts[2], out y))
                    {
                        if (!IsGM)
                            LastTeleportTime = Envir.Time + 10000;
                        TeleportRandom(200, 0);
                        return;
                    }
                    if (!IsGM)
                        LastTeleportTime = Envir.Time + 10000;
                    Teleport(CurrentMap, new Point(x, y));
                    break;

                case "MAPMOVE":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                    var instanceID = 1; x = 0; y = 0;

                    if (parts.Length == 3 || parts.Length == 5)
                        int.TryParse(parts[2], out instanceID);

                    if (instanceID < 1) instanceID = 1;

                    var map = SMain.Envir.GetMapByNameAndInstance(parts[1], GetRegionID(), instanceID);
                    if (map == null)
                    {
                        ReceiveChat((string.Format(Envir.Tr("Map {0}:[{1}] could not be found"), parts[1], instanceID)), ChatType.System);
                        return;
                    }

                    if (parts.Length == 4 || parts.Length == 5)
                    {
                        int.TryParse(parts[parts.Length - 2], out x);
                        int.TryParse(parts[parts.Length - 1], out y);
                    }

                    switch (parts.Length)
                    {
                        case 2:
                            ReceiveChat(TeleportRandom(200, 0, map) ? (string.Format(Envir.Tr("Moved to Map {0}"), map.Info.FileName)) :
                                (string.Format(Envir.Tr("Failed movement to Map {0}"), map.Info.FileName)), ChatType.System);
                            break;
                        case 3:
                            ReceiveChat(TeleportRandom(200, 0, map) ? (string.Format(Envir.Tr("Moved to Map {0}:[{1}]"), map.Info.FileName, instanceID)) :
                                (string.Format(Envir.Tr("Failed movement to Map {0}:[{1}]"), map.Info.FileName, instanceID)), ChatType.System);
                            break;
                        case 4:
                            ReceiveChat(Teleport(map, new Point(x, y)) ? (string.Format(Envir.Tr("Moved to Map {0} at {1}:{2}"), map.Info.FileName, x, y)) :
                                (string.Format(Envir.Tr("Failed movement to Map {0} at {1}:{2}"), map.Info.FileName, x, y)), ChatType.System);
                            break;
                        case 5:
                            ReceiveChat(Teleport(map, new Point(x, y)) ? (string.Format(Envir.Tr("Moved to Map {0}:[{1}] at {2}:{3}"), map.Info.FileName, instanceID, x, y)) :
                                (string.Format(Envir.Tr("Failed movement to Map {0}:[{1}] at {2}:{3}"), map.Info.FileName, instanceID, x, y)), ChatType.System);
                            break;
                    }
                    break;

                case "GOTO":
                    if (!IsGM) return;

                    if (parts.Length < 2) return;
                    player = Envir.GetPlayer(parts[1]);

                    if (player == null) return;

                    Teleport(player.CurrentMap, player.CurrentLocation);
                    break;

                case "MOB":
                    if (!IsGM && !Settings.TestServer) return;
                    if (parts.Length < 2)
                    {
                        ReceiveChat(Envir.Tr("Not enough parameters to spawn monster"), ChatType.System);
                        return;
                    }

                    MonsterInfo mInfo = Envir.GetMonsterInfo(parts[1]);
                    if (mInfo == null)
                    {
                        ReceiveChat((string.Format("Monster {0} does not exist", parts[1])), ChatType.System);
                        return;
                    }

                    uint count = 1;
                    if (parts.Length >= 3 && IsGM)
                        if (!uint.TryParse(parts[2], out count)) count = 1;

                    for (int i = 0; i < count; i++)
                    {
                        MonsterObject monster = MonsterObject.GetMonster(mInfo);
                        if (monster == null) return;
                        monster.Spawn(CurrentMap, Front);
                    }

                    ReceiveChat(string.Format("Monster {0} x{1} has been spawned.", mInfo.Name, count), ChatType.System);
                    break;

                case "RECALLMOB":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    MonsterInfo mInfo2 = Envir.GetMonsterInfo(parts[1]);
                    if (mInfo2 == null) return;

                    count = 1;
                    byte petlevel = 0;

                    if (parts.Length > 2)
                        if (!uint.TryParse(parts[2], out count) || count > 50) count = 1;

                    if (parts.Length > 3)
                        if (!byte.TryParse(parts[3], out petlevel) || petlevel > 7) petlevel = 0;

                    if (!IsGM && Pets.Count > 4) return;

                    for (int i = 0; i < count; i++)
                    {
                        MonsterObject monster = MonsterObject.GetMonster(mInfo2);
                        if (monster == null) return;
                        monster.PetLevel = petlevel;
                        monster.Master = this;
                        monster.MaxPetLevel = 7;
                        monster.Direction = Direction;
                        monster.ActionTime = Envir.Time + 1000;
                        monster.Spawn(CurrentMap, Front);
                        Pets.Add(monster);
                    }

                    ReceiveChat((string.Format(Envir.Tr("Pet {0} x{1} has been recalled."), mInfo2.Name, count)), ChatType.System);
                    break;

                case "RELOADDROPS":
                    if (!IsGM) return;
                    Envir.DropMap.Clear();
                    foreach (var t in Envir.MonsterInfoList)
                        t.LoadDrops();
                    ReceiveChat(Envir.Tr("Drops Reloaded."), ChatType.Hint);
                    break;

                case "RELOADNPCS":
                    if (!IsGM) return;

                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        CurrentMap.NPCs[i].LoadInfo(true);
                    }

                    DefaultNPC.LoadInfo(true);
                    SMain.Envir.MonsterNPC.LoadInfo(true);
                    SMain.Envir.RobotNPC.LoadInfo(true);

                    ReceiveChat(Envir.Tr("NPCs Reloaded."), ChatType.Hint);
                    break;

                case "GIVEGOLD":
                    GiveGold(parts);
                    break;

                case "GIVEPEARLS":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    player = this;

                    if (parts.Length > 2)
                    {
                        if (!IsGM) return;

                        if (!uint.TryParse(parts[2], out count)) return;
                        player = Envir.GetPlayer(parts[1]);

                        if (player == null)
                        {
                            ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[1]), ChatType.System);
                            return;
                        }
                    }

                    else if (!uint.TryParse(parts[1], out count)) return;

                    if (count + player.Info.PearlCount >= int.MaxValue)
                        count = (uint)(int.MaxValue - player.Info.PearlCount);

                    player.IntelligentCreatureGainPearls((int)count);
                    if (count > 1)
                        SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been given {1} pearls"), player.Name, count));
                    else
                        SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been given {1} pearl"), player.Name, count));
                    break;
                case "GIVECREDIT":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    player = this;

                    if (parts.Length > 2)
                    {
                        if (!IsGM) return;

                        if (!uint.TryParse(parts[2], out count)) return;
                        player = Envir.GetPlayer(parts[1]);

                        if (player == null)
                        {
                            ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[1]), ChatType.System);
                            return;
                        }
                    }

                    else if (!uint.TryParse(parts[1], out count)) return;

                    if (count + player.Account.Credit >= uint.MaxValue)
                        count = uint.MaxValue - player.Account.Credit;

                    player.GainCredit(count);
                    string str = string.Format(Envir.Tr("Player {0} has been given {1} credit"), player.Name, count);
                    SMain.Enqueue(str);
                    ReceiveChat(str, ChatType.System);
                    break;
                case "GIVESKILL":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                    byte spellLevel = 0;

                    player = this;
                    Spell skill;

                    if (!Enum.TryParse(parts.Length > 3 ? parts[2] : parts[1], true, out skill)) return;

                    if (skill == Spell.None) return;

                    spellLevel = byte.TryParse(parts.Length > 3 ? parts[3] : parts[2], out spellLevel) ? Math.Min((byte)3, spellLevel) : (byte)0;

                    if (parts.Length > 3)
                    {
                        if (!IsGM) return;

                        player = Envir.GetPlayer(parts[1]);

                        if (player == null)
                        {
                            ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[1]), ChatType.System);
                            return;
                        }
                    }

                    var magic = new UserMagic(skill) { Level = spellLevel };

                    if (player.Info.Magics.Any(e => e.Spell == skill))
                    {
                        player.Info.Magics.FirstOrDefault(e => e.Spell == skill).Level = spellLevel;
                        player.ReceiveChat(string.Format(Envir.Tr("Spell {0} changed to level {1}"), skill.ToString(), spellLevel), ChatType.Hint);
                        return;
                    }
                    else
                    {
                        player.ReceiveChat(string.Format(Envir.Tr("You have learned {0} at level {1}"), skill.ToString(), spellLevel), ChatType.Hint);

                        if (player != this)
                        {
                            ReceiveChat(string.Format(Envir.Tr("{0} has learned {1} at level {2}"), player.Name, skill.ToString(), spellLevel), ChatType.Hint);
                        }

                        player.Info.Magics.Add(magic);
                    }

                    player.Enqueue(magic.GetInfo());
                    player.RefreshStats();
                    break;

                case "FIND":
                    if (!IsGM && !HasProbeNecklace) return;

                    if (Envir.Time < LastProbeTime)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You cannot search for another {0} seconds"), (LastProbeTime - Envir.Time) / 1000), ChatType.System);
                        return;
                    }

                    if (parts.Length < 2) return;
                    player = Envir.GetPlayer(parts[1]);

                    if (player == null)
                    {
                        ReceiveChat(parts[1] + " is not online", ChatType.System);
                        return;
                    }
                    if (player.CurrentMap == null) return;
                    if (!IsGM)
                        LastProbeTime = Envir.Time + 180000;
                    ReceiveChat((string.Format(Envir.Tr("{0} is located at {1} ({2},{3})"), player.Name, player.CurrentMap.Info.Title, player.CurrentLocation.X, player.CurrentLocation.Y)), ChatType.System);
                    break;

                case "CREATEGUILD":

                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    player = parts.Length < 3 ? this : Envir.GetPlayer(parts[1]);

                    if (player == null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[1]), ChatType.System);
                        return;
                    }
                    if (player.MyGuild != null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("Player {0} is already in a guild."), player.Name), ChatType.System);
                        return;
                    }

                    String gName = parts.Length < 3 ? parts[1] : parts[2];
                    if ((gName.Length < 3) || (gName.Length > 20))
                    {
                        ReceiveChat(Envir.Tr("Guildname is restricted to 3-20 characters."), ChatType.System);
                        return;
                    }
                    GuildObject guild = Envir.GetGuild(gName);
                    if (guild != null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("Guild {0} already exists."), gName), ChatType.System);
                        return;
                    }
                    player.CanCreateGuild = true;
                    if (player.CreateGuild(gName))
                        ReceiveChat(string.Format(Envir.Tr("Successfully created guild {0}"), gName), ChatType.System);
                    else
                        ReceiveChat(Envir.Tr("Failed to create guild"), ChatType.System);
                    player.CanCreateGuild = false;
                    break;


                case "TRIGGER":
                    if (!IsGM) return;
                    if (parts.Length < 2) return;

                    if (parts.Length >= 3)
                    {
                        player = Envir.GetPlayer(parts[2]);

                        if (player == null)
                        {
                            ReceiveChat(string.Format(Envir.Tr("Player {0} was not found."), parts[2]), ChatType.System);
                            return;
                        }

                        player.CallDefaultNPC(DefaultNPCType.Trigger, parts[1]);
                        return;
                    }

                    foreach (var pl in Envir.Players)
                    {
                        pl.CallDefaultNPC(DefaultNPCType.Trigger, parts[1]);
                    }

                    break;

                case "RIDE":
                    if (MountType > -1)
                    {
                        RidingMount = !RidingMount;

                        RefreshMount();
                    }
                    else
                        ReceiveChat(Envir.Tr("You haven't a mount..."), ChatType.System);

                    ChatTime = 0;
                    break;
                case "SETFLAG":
                    if (!IsGM && !Settings.TestServer) return;

                    if (parts.Length < 2) return;

                    int tempInt = 0;

                    if (!int.TryParse(parts[1], out tempInt)) return;

                    if (tempInt > Info.Flags.Length - 1) return;

                    Info.Flags[tempInt] = !Info.Flags[tempInt];

                    for (int f = CurrentMap.NPCs.Count - 1; f >= 0; f--)
                    {
                        if (Functions.InRange(CurrentMap.NPCs[f].CurrentLocation, CurrentLocation, Globals.DataRange))
                            CurrentMap.NPCs[f].CheckVisible(this);
                    }

                    break;

                case "LISTFLAGS":
                    if (!IsGM && !Settings.TestServer) return;

                    for (int i = 0; i < Info.Flags.Length; i++)
                    {
                        if (Info.Flags[i] == false) continue;

                        ReceiveChat("Flag " + i, ChatType.Hint);
                    }
                    break;

                case "CLEARFLAGS":
                    if (!IsGM && !Settings.TestServer) return;

                    player = parts.Length > 1 && IsGM ? Envir.GetPlayer(parts[1]) : this;

                    if (player == null)
                    {
                        ReceiveChat(parts[1] + " is not online", ChatType.System);
                        return;
                    }

                    for (int i = 0; i < player.Info.Flags.Length; i++)
                    {
                        player.Info.Flags[i] = false;
                    }
                    break;
                case "CLEARMOB":
                    if (!IsGM) return;

                    if (parts.Length > 1)
                    {
                        map = Envir.GetMapByNameAndInstance(parts[1], GetRegionID());

                        if (map == null) return;

                    }
                    else
                    {
                        map = CurrentMap;
                    }

                        var cellObjects = map.GetAllObjects();
                    if (cellObjects != null)
                    {

                        int obCount = cellObjects.Count();

                        for (int m = 0; m < obCount; m++)
                        {
                            MapObject ob = cellObjects[m];

                            if (ob.Race != ObjectType.Monster) continue;
                            if (ob.Dead) continue;
                            ob.Die();
                        }
                    }
                    break;

                case "CHANGECLASS": //@changeclass [Player] [Class]
                    if (!IsGM && !Settings.TestServer) return;

                    data = parts.Length <= 2 || !IsGM ? Info : Envir.GetCharacterInfo(parts[1]);

                    if (data == null) return;

                    MirClass mirClass;

                    if (!Enum.TryParse(parts[parts.Length - 1], true, out mirClass) || data.Class == mirClass) return;

                    data.Class = mirClass;

                    ReceiveChat(string.Format(Envir.Tr("Player {0} has been changed to {1}"), data.Name, data.Class), ChatType.System);
                    SMain.Enqueue(string.Format(Envir.Tr("Player {0} has been changed to {1} by {2}"), data.Name, data.Class, Name));

                    if (data.Player != null)
                        data.Player.Connection.LogOut();
                    break;

                case "DIE":
                    LastHitter = null;
                    Die();
                    break;
                case "HAIR":
                    if (!IsGM && !Settings.TestServer) return;

                    if (parts.Length < 2)
                    {
                        Info.Hair = (byte)SMain.Envir.Random.Next(0, 9);
                    }
                    else
                    {
                        byte tempByte = 0;

                        byte.TryParse(parts[1], out tempByte);

                        Info.Hair = tempByte;
                    }
                    break;

                case "DECO": //TEST CODE
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    ushort tempShort = 0;

                    ushort.TryParse(parts[1], out tempShort);

                    DecoObject decoOb = new DecoObject
                    {
                        Image = tempShort,
                        CurrentMap = CurrentMap,
                        CurrentLocation = CurrentLocation,
                    };

                    CurrentMap.AddObject(decoOb);
                    decoOb.Spawned();

                    Enqueue(decoOb.GetInfo());
                    break;

                case "ADJUSTPKPOINT":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                    if (parts.Length > 2)
                    {
                        if (!IsGM) return;

                        player = Envir.GetPlayer(parts[1]);

                        if (player == null) return;


                        int.TryParse(parts[2], out tempInt);
                    }
                    else
                    {
                        player = this;
                        int.TryParse(parts[1], out tempInt);
                    }

                    player.PKPoints = tempInt;

                    break;

                case "AWAKENING":
                    {
                        if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                        ItemType type;

                        if (!Enum.TryParse(parts[1], true, out type)) return;

                        AwakeType awakeType;

                        if (!Enum.TryParse(parts[2], true, out awakeType)) return;

                        foreach (UserItem temp in Info.Equipment)
                        {
                            if (temp == null) continue;

                            ItemInfo realItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

                            if (realItem.Type == type)
                            {
                                Awake awake = temp.Awake;
                                bool[] isHit;
                                int result = awake.UpgradeAwake(temp, awakeType, out isHit);
                                switch (result)
                                {
                                    case -1:
                                        ReceiveChat(string.Format(Envir.Tr("{0} : Condition Error."), temp.Name), ChatType.System);
                                        break;
                                    case 0:
                                        ReceiveChat(string.Format(Envir.Tr("{0} : Upgrade Failed."), temp.Name), ChatType.System);
                                        break;
                                    case 1:
                                        ReceiveChat(string.Format(Envir.Tr("{0} : AWAKE Level {1}, value {2}~{3}."), temp.Name, awake.getAwakeLevel(), awake.getAwakeValue(), awake.getAwakeValue()), ChatType.System);
                                        Packet p = new S.RefreshItem { Item = temp };
                                        Enqueue(p);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case "REMOVEAWAKENING":
                    {
                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        ItemType type;

                        if (!Enum.TryParse(parts[1], true, out type)) return;

                        foreach (UserItem temp in Info.Equipment)
                        {
                            if (temp == null) continue;

                            ItemInfo realItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

                            if (realItem.Type == type)
                            {
                                Awake awake = temp.Awake;
                                int result = awake.RemoveAwake();
                                switch (result)
                                {
                                    case 0:
                                        ReceiveChat(string.Format(Envir.Tr("{0} : Remove failed Level 0"), temp.Name), ChatType.System);
                                        break;
                                    case 1:
                                        ReceiveChat(string.Format(Envir.Tr("{0} : Remove success. Level {1}"), temp.Name, temp.Awake.getAwakeLevel()), ChatType.System);
                                        Packet p = new S.RefreshItem { Item = temp };
                                        Enqueue(p);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    break;

                case "STARTWAR":
                    if (!IsGM) return;
                    if (parts.Length < 2) return;

                    GuildObject enemyGuild = Envir.GetGuild(parts[1]);

                    if (MyGuild == null)
                    {
                        ReceiveChat(Envir.Tr("You are not in a guild."), ChatType.System);
                    }

                    if (MyGuild.Ranks[0] != MyGuildRank)
                    {
                        ReceiveChat(Envir.Tr("You must be a leader to start a war."), ChatType.System);
                        return;
                    }

                    if (enemyGuild == null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("Could not find guild {0}."), parts[1]), ChatType.System);
                        return;
                    }

                    if (MyGuild == enemyGuild)
                    {
                        ReceiveChat(Envir.Tr("Cannot go to war with your own guild."), ChatType.System);
                        return;
                    }

                    if (MyGuild.WarringGuilds.Contains(enemyGuild))
                    {
                        ReceiveChat(Envir.Tr("Already at war with this guild."), ChatType.System);
                        return;
                    }

                    if (MyGuild.GoToWar(enemyGuild))
                    {
                        ReceiveChat(string.Format(Envir.Tr("You started a war with {0}."), parts[1]), ChatType.System);
                        enemyGuild.SendMessage(string.Format(Envir.Tr("{0} has started a war"), MyGuild.Name), ChatType.System);
                    }

                    break;

                case "ADDINVENTORY":
                    {
                        int openLevel = (int)((Info.Inventory.Length - 46) / 4);
                        uint openGold = (uint)(1000000 + openLevel * 1000000);
                        if (Account.Gold >= openGold)
                        {
                            Account.Gold -= openGold;
                            Enqueue(new S.LoseGold { Gold = openGold });
                            Enqueue(new S.ResizeInventory { Size = Info.ResizeInventory() });
                            ReceiveChat(Envir.Tr("Inventory size increased."), ChatType.System);
                        }
                        else
                        {
                            ReceiveChat(Envir.Tr("Not enough gold."), ChatType.System);
                        }
                        ChatTime = 0;
                    }
                    break;

                case "ADDSTORAGE":
                    {
                        TimeSpan addedTime = new TimeSpan(30, 0, 0, 0);
                        uint cost = 1000000;

                        if (Account.Gold >= cost)
                        {
                            Account.Gold -= cost;
                            Account.HasExpandedStorage = true;

                            if (Account.ExpandedStorageExpiryDate > Envir.Now)
                            {
                                Account.ExpandedStorageExpiryDate = Account.ExpandedStorageExpiryDate + addedTime;
                                ReceiveChat(Envir.Tr("Expanded storage time extended, expires on: ") + Account.ExpandedStorageExpiryDate.ToString(), ChatType.System);
                            }
                            else
                            {
                                Account.ExpandedStorageExpiryDate = Envir.Now + addedTime;
                                ReceiveChat(Envir.Tr("Storage expanded, expires on: ") + Account.ExpandedStorageExpiryDate.ToString(), ChatType.System);
                            }

                            Enqueue(new S.LoseGold { Gold = cost });
                            Enqueue(new S.ResizeStorage { Size = Account.ExpandStorage(), HasExpandedStorage = Account.HasExpandedStorage, ExpiryTime = Account.ExpandedStorageExpiryDate });
                        }
                        else
                        {
                            ReceiveChat(Envir.Tr("Not enough gold."), ChatType.System);
                        }
                        ChatTime = 0;
                    }
                    break;

                case "INFO":
                    {
                        if (!IsGM && !Settings.TestServer) return;

                        MapObject ob = null;

                        if (parts.Length < 2)
                        {
                            Point target = Functions.PointMove(CurrentLocation, Direction, 1);
                            cellObjects = CurrentMap.GetCellObjects(target);

                            if (cellObjects == null || cellObjects.Count < 1) return;

                            ob = cellObjects[0];
                        }
                        else
                        {
                            ob = Envir.GetPlayer(parts[1]);
                        }

                        if (ob == null) return;

                        switch (ob.Race)
                        {
                            case ObjectType.Player:
                                PlayerObject plOb = (PlayerObject)ob;
                                ReceiveChat(Envir.Tr("--Player Info--"), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("Name : {0}, Level : {1}, X : {2}, Y : {3} Credit {4} Gold {5}"), plOb.Name, plOb.Level, plOb.CurrentLocation.X, plOb.CurrentLocation.Y, plOb.Info.AccountInfo.Credit, plOb.Info.AccountInfo.Gold), ChatType.System2);
                                break;
                            case ObjectType.Monster:
                                MonsterObject monOb = (MonsterObject)ob;
                                ReceiveChat(Envir.Tr("--Monster Info--"), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("ID : {0}, Name : {1}"), monOb.Info.Index, monOb.Name), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("Level : {0}, X : {1}, Y : {2}"), monOb.Level, monOb.CurrentLocation.X, monOb.CurrentLocation.Y), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("HP : {0}, MinDC : {1}, MaxDC : {1}"), monOb.Info.HP, monOb.MinDC, monOb.MaxDC), ChatType.System2);
                                break;
                            case ObjectType.Merchant:
                                NPCObject npcOb = (NPCObject)ob;
                                ReceiveChat(Envir.Tr("--NPC Info--"), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("ID : {0}, Name : {1}"), npcOb.Info.Index, npcOb.Name), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("X : {0}, Y : {1}"), ob.CurrentLocation.X, ob.CurrentLocation.Y), ChatType.System2);
                                ReceiveChat(string.Format(Envir.Tr("File : {0}"), npcOb.Info.FileName), ChatType.System2);
                                break;
                        }
                    }
                    break;

                case "CLEARQUESTS":
                    if (!IsGM && !Settings.TestServer) return;

                    player = parts.Length > 1 && IsGM ? Envir.GetPlayer(parts[1]) : this;

                    if (player == null)
                    {
                        ReceiveChat(parts[1] + " is not online", ChatType.System);
                        return;
                    }

                    foreach (var quest in player.CurrentQuests)
                    {
                        SendUpdateQuest(quest, QuestState.Remove);
                    }

                    player.CurrentQuests.Clear();

                    player.CompletedQuests.Clear();
                    player.GetCompletedQuests();

                    break;

                case "SETQUEST":
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                    player = parts.Length > 3 && IsGM ? Envir.GetPlayer(parts[3]) : this;

                    if (player == null)
                    {
                        ReceiveChat(parts[3] + " is not online", ChatType.System);
                        return;
                    }

                    int questid = 0;
                    int questState = 0;

                    int.TryParse(parts[1], out questid);
                    int.TryParse(parts[2], out questState);

                    if (questid < 1) return;

                    var activeQuest = player.CurrentQuests.FirstOrDefault(e => e.Index == questid);

                    //remove from active list
                    if (activeQuest != null)
                    {
                        player.SendUpdateQuest(activeQuest, QuestState.Remove);
                        player.CurrentQuests.Remove(activeQuest);
                    }

                    switch (questState)
                    {
                        case 0: //cancel
                            if (player.CompletedQuests.Contains(questid))
                                player.CompletedQuests.Remove(questid);
                            break;
                        case 1: //complete
                            if (!player.CompletedQuests.Contains(questid))
                                player.CompletedQuests.Add(questid);
                            break;
                    }

                    player.GetCompletedQuests();
                    break;

                case "CREATEMAPINSTANCE": //TEST CODE
                    if (!IsGM || parts.Length < 2) return;

                    map = SMain.Envir.GetMapByNameAndInstance(parts[1], GetRegionID());

                    if (map == null)
                    {
                        ReceiveChat(string.Format("Map {0} does not exist", parts[1]), ChatType.System);
                        return;
                    }

                    MapInfo mapInfo = map.Info;
                    mapInfo.CreateInstance();
                    ReceiveChat(string.Format(Envir.Tr("Map instance created for map {0}"), mapInfo.FileName), ChatType.System);
                    break;
                case "STARTCONQUEST":
                    //Needs some work, but does job for now.
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                    int ConquestID;

                    if (parts.Length < 1)
                    {
                        ReceiveChat(string.Format(Envir.Tr("The Syntax is /StartConquest [ConquestID]")), ChatType.System);
                        return;
                    }

                    if (MyGuild == null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You need to be in a guild to start a War")), ChatType.System);
                        return;
                    }

                    else if (!int.TryParse(parts[1], out ConquestID)) return;

                    ConquestObject tempConq = Envir.Conquests.FirstOrDefault(t => t.Info.Index == ConquestID);

                    if (tempConq != null)
                    {
                        tempConq.StartType = ConquestType.Forced;
                        tempConq.WarIsOn = !tempConq.WarIsOn;
                        tempConq.AttackerID = MyGuild.Guildindex;
                    }
                    else return;
                    ReceiveChat(string.Format(Envir.Tr("{0} War Started."), tempConq.Info.Name), ChatType.System);
                    SMain.Enqueue(string.Format(Envir.Tr("{0} War Started."), tempConq.Info.Name));
                    break;
                case "RESETCONQUEST":
                    //Needs some work, but does job for now.
                    if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                    int ConquestNum;

                    if (parts.Length < 1)
                    {
                        ReceiveChat(string.Format(Envir.Tr("The Syntax is /ResetConquest [ConquestID]")), ChatType.System);
                        return;
                    }

                    if (MyGuild == null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You need to be in a guild to start a War")), ChatType.System);
                        return;
                    }

                    else if (!int.TryParse(parts[1], out ConquestNum)) return;

                    ConquestObject ResetConq = Envir.Conquests.FirstOrDefault(t => t.Info.Index == ConquestNum);

                    if (ResetConq != null && !ResetConq.WarIsOn)
                    {
                        ResetConq.Reset();
                    }
                    else
                    {
                        ReceiveChat(Envir.Tr("Conquest not found or War is currently on."), ChatType.System);
                        return;
                    }
                    ReceiveChat(string.Format(Envir.Tr("{0} has been reset."), ResetConq.Info.Name), ChatType.System);
                    break;
                case "GATES":

                    if (MyGuild == null || MyGuild.Conquest == null || !MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank) || MyGuild.Conquest.WarIsOn)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You don't have access to control any gates at the moment.")), ChatType.System);
                        return;
                    }

                    bool OpenClose = false;

                    if (parts.Length > 1)
                    {
                        string openclose = parts[1];

                        if (openclose.ToUpper() == "CLOSE") OpenClose = true;
                        else if (openclose.ToUpper() == "OPEN") OpenClose = false;
                        else
                        {
                            ReceiveChat(string.Format(Envir.Tr("You must type /Gates Open or /Gates Close.")), ChatType.System);
                            return;
                        }

                        for (int i = 0; i < MyGuild.Conquest.GateList.Count; i++)
                            if (MyGuild.Conquest.GateList[i].Gate != null && !MyGuild.Conquest.GateList[i].Gate.Dead)
                                if (OpenClose)
                                    MyGuild.Conquest.GateList[i].Gate.CloseDoor();
                                else
                                    MyGuild.Conquest.GateList[i].Gate.OpenDoor();
                    }
                    else
                    {
                        for (int i = 0; i < MyGuild.Conquest.GateList.Count; i++)
                            if (MyGuild.Conquest.GateList[i].Gate != null && !MyGuild.Conquest.GateList[i].Gate.Dead)
                                if (!MyGuild.Conquest.GateList[i].Gate.Closed)
                                {
                                    MyGuild.Conquest.GateList[i].Gate.CloseDoor();
                                    OpenClose = true;
                                }
                                else
                                {
                                    MyGuild.Conquest.GateList[i].Gate.OpenDoor();
                                    OpenClose = false;
                                }
                    }

                    if (OpenClose)
                        ReceiveChat(string.Format(Envir.Tr("The gates at {0} have been closed."), MyGuild.Conquest.Info.Name), ChatType.System);
                    else
                        ReceiveChat(string.Format(Envir.Tr("The gates at {0} have been opened."), MyGuild.Conquest.Info.Name), ChatType.System);
                    break;

                case "CHANGEFLAG":
                    if (MyGuild == null || MyGuild.Conquest == null || !MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank) || MyGuild.Conquest.WarIsOn)
                    {
                        ReceiveChat(string.Format(Envir.Tr("You don't have access to change any flags at the moment.")), ChatType.System);
                        return;
                    }

                    ushort flag = (ushort)Envir.Random.Next(12);

                    if (parts.Length > 1)
                    {
                        ushort temp;

                        ushort.TryParse(parts[1], out temp);

                        if (temp <= 11) flag = temp;
                    }

                    MyGuild.FlagImage = (ushort)(1000 + flag);

                    for (int i = 0; i < MyGuild.Conquest.FlagList.Count; i++)
                    {
                        MyGuild.Conquest.FlagList[i].UpdateImage();
                    }

                    break;
                case "CHANGEFLAGCOLOUR":
                    {
                        if (MyGuild == null || MyGuild.Conquest == null || !MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank) || MyGuild.Conquest.WarIsOn)
                        {
                            ReceiveChat(string.Format(Envir.Tr("You don't have access to change any flags at the moment.")), ChatType.System);
                            return;
                        }

                        byte r1 = (byte)Envir.Random.Next(255);
                        byte g1 = (byte)Envir.Random.Next(255);
                        byte b1 = (byte)Envir.Random.Next(255);

                        if (parts.Length > 3)
                        {
                            byte.TryParse(parts[1], out r1);
                            byte.TryParse(parts[2], out g1);
                            byte.TryParse(parts[3], out b1);
                        }

                        MyGuild.FlagColour = Color.FromArgb(255, r1, g1, b1);

                        for (int i = 0; i < MyGuild.Conquest.FlagList.Count; i++)
                        {
                            MyGuild.Conquest.FlagList[i].UpdateColour();
                        }
                    }
                    break;
                case "REVIVE":
                    if (!IsGM) return;

                    if (parts.Length < 2)
                    {
                        RefreshStats();
                        SetHP(MaxHP);
                        SetMP(MaxMP);
                        Revive(MaxHealth, true);
                    }
                    else
                    {
                        player = Envir.GetPlayer(parts[1]);
                        if (player == null) return;
                        player.Revive(MaxHealth, true);
                    }
                    break;
                case "DELETESKILL":
                    DeleteSkill(parts);
                    break;

                default:
                    break;
            }

            CommandMgr.ProcessCommand(parts);

            foreach (string command in Envir.CustomCommands)
            {
                if (string.Compare(parts[0], command, true) != 0) continue;
                CallDefaultNPC(DefaultNPCType.CustomCommand, parts[0]);
            }
        }

        public void Chat(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            SMain.EnqueueChat(string.Format("{0}: {1}", Name, message));

            if (Info.ChatBanned)
            {
                if (Info.ChatBanExpiryDate > DateTime.Now)
                {
                    ReceiveChat(Envir.Tr("You are currently banned from chatting."), ChatType.System);
                    return;
                }

                Info.ChatBanned = false;
            }
            else
            {
                if (ChatTime > Envir.Time)
                {
                    if (ChatTick >= 5 & !IsGM)
                    {
                        Info.ChatBanned = true;
                        Info.ChatBanExpiryDate = DateTime.Now.AddMinutes(5);
                        ReceiveChat(Envir.Tr("You have been banned from chatting for 5 minutes."), ChatType.System);
                        return;
                    }

                    ChatTick++;
                }
                else
                    ChatTick = 0;

                ChatTime = Envir.Time + 2000;
            }

            string[] parts;

            message = message.Replace("$pos", Functions.PointToString(CurrentLocation));


            Packet p;
            if (message.StartsWith("/"))
            {
                //Private Message
                message = message.Remove(0, 1);
                parts = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) return;

                PlayerObject player = Envir.GetPlayer(parts[0]);

                if (player == null)
                {
                    IntelligentCreatureObject creature = GetCreatureByName(parts[0]);
                    if (creature != null)
                    {
                        creature.ReceiveChat(message.Remove(0, parts[0].Length), ChatType.WhisperIn);
                        return;
                    }
                    ReceiveChat(string.Format(Envir.Tr("Could not find {0}."), parts[0]), ChatType.System);
                    return;
                }

                if (player.Info.Friends.Any(e => e.Info == Info && e.Blocked))
                {
                    ReceiveChat(Envir.Tr("Player is not accepting your messages."), ChatType.System);
                    return;
                }

                if (Info.Friends.Any(e => e.Info == player.Info && e.Blocked))
                {
                    ReceiveChat(Envir.Tr("Cannot message player whilst they are on your blacklist."), ChatType.System);
                    return;
                }

                ReceiveChat(string.Format("/{0}", message), ChatType.WhisperOut);
                player.ReceiveChat(string.Format("{0}=>{1}", Name, message.Remove(0, parts[0].Length)), ChatType.WhisperIn);
            }
            else if (message.StartsWith("!!"))
            {
                if (GroupMembers == null) return;
                //Group
                message = String.Format("{0}:{1}", Name, message.Remove(0, 2));

                p = new S.ObjectChat { ObjectID = ObjectID, Text = message, Type = ChatType.Group };

                for (int i = 0; i < GroupMembers.Count; i++)
                    GroupMembers[i].Enqueue(p);
            }
            else if (message.StartsWith("!~"))
            {
                if (MyGuild == null) return;

                //Guild
                message = message.Remove(0, 2);
                MyGuild.SendMessage(String.Format("{0}: {1}", Name, message));

            }
            else if (message.StartsWith("!#"))
            {
                //Mentor Message
                message = message.Remove(0, 2);
                parts = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) return;

                if (Info.Mentor == 0) return;

                CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);
                PlayerObject player = Envir.GetPlayer(Mentor.Name);

                if (player == null)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} isn't online."), Mentor.Name), ChatType.System);
                    return;
                }

                ReceiveChat(string.Format("{0}: {1}", Name, message), ChatType.Mentor);
                player.ReceiveChat(string.Format("{0}: {1}", Name, message), ChatType.Mentor);
            }
            else if (message.StartsWith("!"))
            {
                //Shout
                if (Envir.Time < ShoutTime)
                {
                    ReceiveChat(string.Format(Envir.Tr("You cannot shout for another {0} seconds."), Math.Ceiling((ShoutTime - Envir.Time) / 1000D)), ChatType.System);
                    return;
                }
                if (Level < 8 && (!HasMapShout && !HasServerShout))
                {
                    ReceiveChat(Envir.Tr("You need to be level 8 before you can shout."), ChatType.System);
                    return;
                }

                ShoutTime = Envir.Time + 10000;
                message = String.Format("(!){0}:{1}", Name, message.Remove(0, 1));

                if (HasMapShout)
                {
                    p = new S.Chat { Message = message, Type = ChatType.Shout2 };
                    HasMapShout = false;

                    for (int i = 0; i < CurrentMap.Players.Count; i++)
                    {
                        CurrentMap.Players[i].Enqueue(p);
                    }
                    return;
                }
                else if (HasServerShout)
                {
                    p = new S.Chat { Message = message, Type = ChatType.Shout3 };
                    HasServerShout = false;

                    for (int i = 0; i < Envir.Players.Count; i++)
                    {
                        Envir.Players[i].Enqueue(p);
                    }
                    return;
                }
                else
                {
                    p = new S.Chat { Message = message, Type = ChatType.Shout };

                    //Envir.Broadcast(p);
                    for (int i = 0; i < CurrentMap.Players.Count; i++)
                    {
                        if (!Functions.InRange(CurrentLocation, CurrentMap.Players[i].CurrentLocation, Globals.DataRange * 2)) continue;
                        CurrentMap.Players[i].Enqueue(p);
                    }
                }

            }
            else if (message.StartsWith(":)"))
            {
                //Relationship Message
                message = message.Remove(0, 2);
                parts = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) return;

                if (Info.Married == 0) return;

                CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);
                PlayerObject player = Envir.GetPlayer(Lover.Name);

                if (player == null)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} isn't online."), Lover.Name), ChatType.System);
                    return;
                }

                ReceiveChat(string.Format("{0}: {1}", Name, message), ChatType.Relationship);
                player.ReceiveChat(string.Format("{0}: {1}", Name, message), ChatType.Relationship);
            }
            else if (message.StartsWith("@!"))
            {
                if (!IsGM) return;

                message = String.Format("(*){0}:{1}", Name, message.Remove(0, 2));

                p = new S.Chat { Message = message, Type = ChatType.Announcement };

                Envir.Broadcast(p);
            }
            else if (message.StartsWith("@"))
            {
                ProcessCommand(message);
            }
            else
            {
                message = String.Format("{0}:{1}", CurrentMap.Info.NoNames ? "?????" : Name, message);

                p = new S.ObjectChat { ObjectID = ObjectID, Text = message, Type = ChatType.Normal };

                Enqueue(p);
                Broadcast(p);
            }
        }

        public void Turn(MirDirection dir)
        {
            _stepCounter = 0;

            if (CanMove)
            {
                ActionTime = Envir.Time + GetDelayTime(TurnDelay);

                Direction = dir;
                if (CheckMovement(CurrentLocation)) return;

                SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

                if (szi != null)
                {
                    BindLocation = szi.Location;
                    BindMapIndex = CurrentMapIndex;
                    InSafeZone = true;
                }
                else
                    InSafeZone = false;

                var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
                if (cellObjects != null)
                {
                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        if (cellObjects[i].Race != ObjectType.Spell) continue;
                        SpellObject ob = (SpellObject)cellObjects[i];

                        ob.ProcessSpell(this);
                        //break;
                    }
                }

                if (TradePartner != null)
                    TradeCancel();

                if (ItemRentalPartner != null)
                    CancelItemRental();

                Broadcast(new S.ObjectTurn { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            }

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
        }
        public void Harvest(MirDirection dir)
        {
            if (!CanMove)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            ActionTime = Envir.Time + HarvestDelay;

            Direction = dir;

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
            Broadcast(new S.ObjectHarvest { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            Point front = Front;
            bool send = false;
            for (int d = 0; d <= 1; d++)
            {
                for (int y = front.Y - d; y <= front.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = front.X - d; x <= front.X + d; x += Math.Abs(y - front.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;
                        if (!CurrentMap.ValidPoint(x, y)) continue;

                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            if (ob.Race != ObjectType.Monster || !ob.Dead || ob.Harvested) continue;

                            if (ob.EXPOwner != null && ob.EXPOwner != this && !IsMember(ob))
                            {
                                send = true;
                                continue;
                            }

                            if (ob.Harvest(this)) return;
                        }
                    }
                }
            }

            if (send)
                ReceiveChat(Envir.Tr("You do not own any nearby carcasses."), ChatType.System);
        }

        public void CancelSpelling()
        {
            ChannelingTime = 0;
            ChannelingId = 0;
            SpellingId = 0;
        }

        public void Walk(MirDirection dir)
        {

            if (!CanMove || !CanWalk)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            Point location = Functions.PointMove(CurrentLocation, dir, 1);

            if (!CurrentMap.ValidPoint(location))
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            if (!CurrentMap.CheckDoorOpen(location))
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }


            var cellObjects = CurrentMap.GetCellObjects(location);
            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];

                    if (ob.Race == ObjectType.Merchant)
                    {
                        NPCObject NPC = (NPCObject)ob;
                        if (!NPC.Visible || !NPC.VisibleLog[Info.Index]) continue;
                    }
                    else
                        if (!ob.Blocking || ob.CellTime >= Envir.Time) continue;

                    Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                    return;
                }

            if (Concentrating)
            {
                if (ConcentrateInterrupted)
                    ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                else
                {
                    ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                    ConcentrateInterrupted = true;
                    UpdateConcentration();//Update & send to client
                }
            }

            if (Hidden && !HasClearRing)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.Hiding:
                            Buffs[i].ExpireTime = 0;
                            break;
                    }
                }
            }

            Direction = dir;
            if (CheckMovement(location)) return;

            CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
            RemoveObjects(dir, 1);

            CurrentLocation = location;
            CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
            AddObjects(dir, 1);

            _stepCounter++;

            SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

            if (szi != null)
            {
                BindLocation = szi.Location;
                BindMapIndex = CurrentMapIndex;
                InSafeZone = true;
            }
            else
                InSafeZone = false;


            CheckConquest();

            CancelSpelling();

            CellTime = Envir.Time + 500;
            ActionTime = Envir.Time + GetDelayTime(MoveDelay);

            if (TradePartner != null)
                TradeCancel();

            if (ItemRentalPartner != null)
                CancelItemRental();

            if (RidingMount) DecreaseMountLoyalty(1);

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
            Broadcast(new S.ObjectWalk { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });


            cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
            if (cellObjects != null)
            {
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    if (cellObjects[i].Race != ObjectType.Spell) continue;
                    SpellObject ob = (SpellObject)cellObjects[i];

                    ob.ProcessSpell(this);
                    //break;
                }
            }

        }
        public void Run(MirDirection dir)
        {
            var steps = RidingMount || ActiveSwiftFeet && !Sneaking ? 3 : 2;

            if (!CanMove || !CanWalk || !CanRun)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            if (Concentrating)
            {
                if (ConcentrateInterrupted)
                    ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                else
                {
                    ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                    ConcentrateInterrupted = true;
                    UpdateConcentration();//Update & send to client
                }
            }

            if (TradePartner != null)
                TradeCancel();

            if (ItemRentalPartner != null)
                CancelItemRental();

            if (Hidden && !HasClearRing && !Sneaking)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.Hiding:
                        case BuffType.MoonLight:
                        case BuffType.DarkBody:
                            Buffs[i].ExpireTime = 0;
                            break;
                    }
                }
            }

            Direction = dir;
            Point location = Functions.PointMove(CurrentLocation, dir, 1);
            for (int j = 1; j <= steps; j++)
            {
                location = Functions.PointMove(CurrentLocation, dir, j);
                if (!CurrentMap.ValidPoint(location))
                {
                    Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                    return;
                }
                if (!CurrentMap.CheckDoorOpen(location))
                {
                    Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                    return;
                }
                var cellObjects = CurrentMap.GetCellObjects(location);

                if (cellObjects != null)
                {
                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject ob = cellObjects[i];

                        if (ob.Race == ObjectType.Merchant)
                        {
                            NPCObject NPC = (NPCObject)ob;
                            if (!NPC.Visible || !NPC.VisibleLog[Info.Index]) continue;
                        }
                        else
                            if (!ob.Blocking || ob.CellTime >= Envir.Time) continue;

                        Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                        return;
                    }


                }
                if (CheckMovement(location)) return;

            }
            if (RidingMount && !Sneaking)
            {
                DecreaseMountLoyalty(2);
            }

            Direction = dir;

            CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
            RemoveObjects(dir, steps);

            Point OldLocation = CurrentLocation;
            CurrentLocation = location;
            CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
            AddObjects(dir, steps);


            SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

            if (szi != null)
            {
                BindLocation = szi.Location;
                BindMapIndex = CurrentMapIndex;
                InSafeZone = true;
            }
            else
                InSafeZone = false;


            CheckConquest();



            CellTime = Envir.Time + 500;
            ActionTime = Envir.Time + GetDelayTime(MoveDelay);

            if (!RidingMount)
                _runCounter++;

            if (_runCounter > 10)
            {
                _runCounter -= 8;
                ChangeHP(-1);
            }

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
            Broadcast(new S.ObjectRun { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });


            for (int j = 1; j <= steps; j++)
            {
                location = Functions.PointMove(OldLocation, dir, j);
                var cellObjects = CurrentMap.GetCellObjects(location);
                if (cellObjects == null) continue;
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    if (cellObjects[i].Race != ObjectType.Spell) continue;
                    SpellObject ob = (SpellObject)cellObjects[i];

                    ob.ProcessSpell(this);
                    //break;
                }
            }

            CancelSpelling();
        }

        public override int Pushed(MapObject pusher, MirDirection dir, int distance)
        {
            int result = 0;
            MirDirection reverse = Functions.ReverseDirection(dir);
            for (int i = 0; i < distance; i++)
            {
                Point location = Functions.PointMove(CurrentLocation, dir, 1);

                if (!CurrentMap.ValidPoint(location)) return result;

                var cellObjects = CurrentMap.GetCellObjects(location);

                bool stop = false;
                if (cellObjects != null)
                    for (int c = 0; c < cellObjects.Count; c++)
                    {
                        MapObject ob = cellObjects[c];
                        if (!ob.Blocking) continue;
                        stop = true;
                    }
                if (stop) break;

                CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);

                Direction = reverse;
                RemoveObjects(dir, 1);
                CurrentLocation = location;
                CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
                AddObjects(dir, 1);

                if (TradePartner != null)
                    TradeCancel();

                if (ItemRentalPartner != null)
                    CancelItemRental();

                Enqueue(new S.Pushed { Direction = Direction, Location = CurrentLocation });
                Broadcast(new S.ObjectPushed { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

                result++;
            }

            if (result > 0)
            {
                if (Concentrating)
                {
                    if (ConcentrateInterrupted)
                        ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                    else
                    {
                        ConcentrateInterruptTime = SMain.Envir.Time + 3000;// needs adjusting
                        ConcentrateInterrupted = true;
                        UpdateConcentration();//Update & send to client
                    }
                }

                var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
                if (cellObjects != null)
                {
                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        if (cellObjects[i].Race != ObjectType.Spell) continue;
                        SpellObject ob = (SpellObject)cellObjects[i];

                        ob.ProcessSpell(this);
                        //break;
                    }
                }
            }

            ActionTime = Envir.Time + 500;
            return result;
        }

        public void RangeAttack(MirDirection dir, Point location, uint targetID)
        {
            LogTime = Envir.Time + Globals.LogDelay;

            if (!HasArchorClassWeapon())
                return;

            if (Functions.InRange(CurrentLocation, location, Globals.MaxAttackRange) == false) return;

            MapObject target = null;

            if (targetID == ObjectID)
                target = this;
            else if (targetID > 0)
                target = FindObject(targetID, 10);

            if (target != null && target.Dead) return;

            if (target != null && target.Race != ObjectType.Monster && target.Race != ObjectType.Player) return;

            Direction = dir;

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });

            UserMagic magic;
            Spell spell = Spell.None;

            if (target != null && !CanFly(target.CurrentLocation) && (Info.MentalState != 1))
            {
                target = null;
                targetID = 0;
            }

            if (target != null)
            {
                magic = GetMagic(Spell.PoisonShot);
                if (magic != null && magic.Toggle)
                {
                    int cost = magic.Info.BaseCost + magic.Info.LevelCost * magic.Level;
                    if (cost < MP)
                    {
                        ChangeMP(-cost);

                        PoisonArrowShot(target, magic);
                        spell = Spell.PoisonShot;
                    }
                }
                else
                {
                    magic = GetMagic(Spell.Focus);
                    if (magic != null)
                        LevelMagic(magic);

                    int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);//计算距离
                    int damage = GetAttackPower(MinDC, MaxDC); //基础伤害(修改为计算攻击)
                    damage = ApplyDistanceBonus(damage, distance);//range boost //距离伤害系数,越远伤害越高.
                    damage = ApplyArcherState(damage); //精神加成

                    if (target.CurrentLocation != location)
                        location = target.CurrentLocation;

                    int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500 + 50; //50 MS per Step

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, target, damage, DefenceType.ACAgility, true);
                    ActionList.Add(action);
                }
            }
            else
                targetID = 0;

            Enqueue(new S.RangeAttack { TargetID = targetID, Target = location, Spell = spell });
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = targetID, Target = location, Spell = spell });

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + 550;
            RegenTime = Envir.Time + RegenDelay;
        }

        private void Thrusting(Spell spell, ref Point target, MirDirection dir, int damageBase)
        {
            if (spell == Spell.Thrusting)
            {
                target = Functions.PointMove(target, dir, 1);

                if (!CurrentMap.ValidPoint(target)) return;

                var cellObjects = CurrentMap.GetCellObjects(target);

                if (cellObjects == null) return;

                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                    if (!ob.IsAttackTarget(this)) continue;

                    UserMagic magic = GetMagic(spell);
                    int damageFinal = magic.GetDamage(damageBase);
                    ob.Attacked(this, damageFinal, DefenceType.Agility, false);
                    break;
                }
            }
        }

        private void HalfMoon(Spell spell, MirDirection dir, int damageBase)
        {
            if (spell != Spell.HalfMoon)
                return;

            dir = Functions.PreviousDir(dir);

            UserMagic magic = GetMagic(spell);
            int damageFinal = magic.GetDamage(damageBase);
            for (int i = 0; i < 4; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, dir, 1);
                dir = Functions.NextDir(dir);
                if (target == Front) continue;

                if (!CurrentMap.ValidPoint(target)) continue;

                var cellObjects = CurrentMap.GetCellObjects(target);

                if (cellObjects == null) continue;

                for (int o = 0; o < cellObjects.Count; o++)
                {
                    MapObject ob = cellObjects[o];
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                    if (!ob.IsAttackTarget(this)) continue;

                    ob.Attacked(this, damageFinal, DefenceType.Agility, false);
                    break;
                }
            }
        }

        private void CrossHalfMoon(Spell spell, int damageBase, ref MirDirection dir)
        {
            if (spell != Spell.CrossHalfMoon)
                return;

            UserMagic magic = GetMagic(spell);
            int damageFinal = magic.GetDamage(damageBase);
            for (int i = 0; i < 8; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, dir, 1);
                dir = Functions.NextDir(dir);
                if (target == Front) continue;

                if (!CurrentMap.ValidPoint(target)) continue;

                var cellObjects = CurrentMap.GetCellObjects(target);

                if (cellObjects == null) continue;

                for (int o = 0; o < cellObjects.Count; o++)
                {
                    MapObject ob = cellObjects[o];
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                    if (!ob.IsAttackTarget(this)) continue;

                    ob.Attacked(this, damageFinal, DefenceType.Agility, false);
                    break;
                }
            }
        }

        private void Mine(Point target)
        {
            UserItem weapon = Info.Equipment[(int)EquipmentSlot.Weapon];
            if (weapon == null) return;
            if (!weapon.Info.CanMine || weapon.CurrentDura <= 0) return;
            
            if (CurrentMap.Mine == null) return;
            MineSet Mine = CurrentMap.GetMineSpot(target.X, target.Y);
            byte StonesLeft = CurrentMap.GetStoneLeft(target.X, target.Y);
             
            if (Mine == null) return;
            if (StonesLeft > 0)
            {
                CurrentMap.ReduceStonesLeft(target.X, target.Y);
                if (Envir.Random.Next(100) <= (Mine.HitRate + (Info.Equipment[(int)EquipmentSlot.Weapon].Info.Accuracy + Info.Equipment[(int)EquipmentSlot.Weapon].Accuracy) * 10))
                {
                    //create some rubble on the floor (or increase whats there)
                    SpellObject Rubble = null;
                    var minecellObjects = CurrentMap.GetCellObjects(CurrentLocation);
                    if (minecellObjects != null)
                    {
                        for (int i = 0; i < minecellObjects.Count; i++)
                        {
                            if (minecellObjects[i].Race != ObjectType.Spell) continue;
                            SpellObject ob = (SpellObject)minecellObjects[i];

                            if (ob.Spell != Spell.Rubble) continue;
                            Rubble = ob;
                            Rubble.ExpireTime = Envir.Time + (5 * 60 * 1000);
                            break;
                        }
                    }
                    if (Rubble == null)
                    {
                        Rubble = new SpellObject
                        {
                            Spell = Spell.Rubble,
                            Value = 1,
                            ExpireTime = Envir.Time + (5 * 60 * 1000),
                            TickSpeed = 2000,
                            Caster = null,
                            CurrentLocation = CurrentLocation,
                            CurrentMap = this.CurrentMap,
                            Direction = MirDirection.Up
                        };
                        CurrentMap.AddObject(Rubble);
                        Rubble.Spawned();
                    }
                    if (Rubble != null)
                        ActionList.Add(new DelayedAction(DelayedType.Mine, Envir.Time + 400, Rubble));
                    //check if we get a payout
                    if (Envir.Random.Next(100) <= (Mine.DropRate + (MineRate * 5)))
                    {
                        GetMinePayout(Mine);
                    }
                    DamageItem(Info.Equipment[(int)EquipmentSlot.Weapon], 5 + Envir.Random.Next(15));
                }
            }
            else
            {
                CurrentMap.ProcessMineRegen(Mine, target.X, target.Y);
            }
        }

        private void LuoHanGunFa(Spell spell, MirDirection dir, Point target, int damageBase)
        {
            if (spell != Spell.LuoHanGunFa)
                return;

            for (int j = 1; j < 3; j++)
            {
                if (j > 0)
                    target = Functions.PointMove(target, dir, j);

                if (!CurrentMap.ValidPoint(target)) continue;

                var cellObjects = CurrentMap.GetCellObjects(target);

                if (cellObjects == null) continue;

                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                    if (!ob.IsAttackTarget(this)) continue;

                    UserMagic magic = GetMagic(spell);
                    int damageFinal = magic.GetDamage(damageBase);
                    ob.Attacked(this, damageFinal, DefenceType.ACAgility, false);
                    break;
                }
            }
        }

        private void JinGangGunFa(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, Direction);
            CurrentMap.ActionList.Add(action);
        }

        private void ProcessMPEnter(ref int damageFinal, int damageBase, ref DefenceType defence, MapObject ob)
        {
            UserMagic magic = GetMagic(Spell.MPEater);
            if (magic == null)
                return;

            int baseCount = 1 + Accuracy / 2;
            int maxCount = baseCount + magic.Level * 5;
            MPEaterCount += Envir.Random.Next(baseCount, maxCount);
            if (MPEater)
            {
                LevelMagic(magic);
                damageFinal = magic.GetDamage(damageBase);
                defence = DefenceType.ACAgility;

                S.ObjectEffect p = new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.MPEater, EffectType = ObjectID };
                CurrentMap.Broadcast(p, ob.CurrentLocation);

                int addMp = 5 * (magic.Level + Accuracy / 4);

                if (ob.Race == ObjectType.Player)
                {
                    ((PlayerObject)ob).ChangeMP(-addMp);
                }

                ChangeMP(addMp);
                MPEaterCount = 0;
                MPEater = false;
            }
            else if (!MPEater && 100 <= MPEaterCount) MPEater = true;
        }

        private void ProcessHemorrhage(ref int damageFinal, int damageBase, MapObject ob)
        {
            UserMagic magic = GetMagic(Spell.Hemorrhage);
            if (magic == null)
                return;

            HemorrhageAttackCount += Envir.Random.Next(1, 1 + magic.Level * 2);
            if (Hemorrhage)
            {
                damageFinal = magic.GetDamage(damageBase);
                LevelMagic(magic);
                S.ObjectEffect ef = new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.Hemorrhage };

                CurrentMap.Broadcast(ef, ob.CurrentLocation);
                long calcDuration = magic.Level * 2 + Luck / 6;

                ob.ApplyPoison(new Poison
                {
                    Duration = (calcDuration <= 0) ? 1 : calcDuration,
                    Owner = this,
                    PType = PoisonType.Bleeding,
                    TickSpeed = 1000,
                    Value = MaxDC + 1
                }, this);

                ob.OperateTime = 0;
                HemorrhageAttackCount = 0;
                Hemorrhage = false;
            }
            else if (!Hemorrhage && 55 <= HemorrhageAttackCount) Hemorrhage = true; 
        }

        private void ProcessAreaAttack(Spell spell, Point target, MirDirection dir, int damageBase)
        {
            switch (spell)
            {
                case Spell.Thrusting:
                    Thrusting(spell, ref target, dir, damageBase);
                    break;
                case Spell.HalfMoon:
                    HalfMoon(spell, dir, damageBase);
                    break;
                case Spell.CrossHalfMoon:
                    CrossHalfMoon(spell, damageBase, ref dir);
                    break;
                case Spell.None:
                    Mine(target);
                    break;
                case Spell.LuoHanGunFa:
                    LuoHanGunFa(spell, dir, target, damageBase);
                    break;
            }
        }

        private void ProcessAttackDamage(Spell spell, ref int damageFinal, int damageBase, ref DefenceType defence, MapObject ob)
        {
            UserMagic magic;
            DelayedAction action;
            switch (spell)
            {
                case Spell.Slaying:
                    magic = GetMagic(Spell.Slaying);
                    damageFinal = magic.GetDamage(damageBase);
                    LevelMagic(magic);
                    break;
                case Spell.DoubleSlash:
                    magic = GetMagic(Spell.DoubleSlash);
                    if (defence == DefenceType.MACAgility) defence = DefenceType.ACAgility;
                    damageFinal = magic.GetDamage(damageBase);
                    action = new DelayedAction(DelayedType.Damage, Envir.Time + 550, ob, damageFinal, defence, false);
                    ActionList.Add(action);
                    LevelMagic(magic);
                    break;
                case Spell.Thrusting:
                    magic = GetMagic(Spell.Thrusting);
                    LevelMagic(magic);
                    break;
                case Spell.LuoHanGunFa:
                    magic = GetMagic(Spell.LuoHanGunFa);
                    damageFinal = magic.GetDamage(damageBase);
                    LevelMagic(magic);
                    break;
                case Spell.HalfMoon:
                    magic = GetMagic(Spell.HalfMoon);
                    LevelMagic(magic);
                    break;
                case Spell.CrossHalfMoon:
                    magic = GetMagic(Spell.CrossHalfMoon);
                    LevelMagic(magic);
                    break;
                case Spell.TwinDrakeBlade:
                    magic = GetMagic(Spell.TwinDrakeBlade);
                    damageFinal = magic.GetDamage(damageBase);
                    action = new DelayedAction(DelayedType.Damage, Envir.Time + 400, ob, damageFinal, DefenceType.ACAgility, false);
                    ActionList.Add(action);
                    LevelMagic(magic);

                    magic.Toggle = false;
                    if ((((ob.Race != ObjectType.Player) || Settings.PvpCanResistPoison) && (Envir.Random.Next(Settings.PoisonAttackWeight) >= ob.PoisonResist)) 
                         && Envir.Random.Next(ob.Race == ObjectType.Player ? 40 : 20) <= magic.Level + 1)
                    {
                        ob.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = ob.Race == ObjectType.Player ? 2 : magic.Level, TickSpeed = 1000 }, this);
                        ob.Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.TwinDrakeBlade });
                    }

                    break;
                case Spell.FlamingSword:
                    magic = GetMagic(Spell.FlamingSword);
                    damageFinal = magic.GetDamage(damageBase);
                    FlamingSword = false;
                    defence = DefenceType.AC;
                    //action = new DelayedAction(DelayedType.Damage, Envir.Time + 400, ob, damage, DefenceType.Agility, true);
                    //ActionList.Add(action);
                    LevelMagic(magic);
                    break;

                case Spell.DaMoGunFa:
                    magic = GetMagic(Spell.DaMoGunFa);
                    damageFinal = magic.GetDamage(damageBase);
                    DaMoGunFa = false;
                    defence = DefenceType.AC;
                    LevelMagic(magic);
                    break;
            }
        }

        private void ProcessAttackSpell(ref Spell spell, ref byte level)
        {
            UserMagic magic;
            if (RidingMount)
            {
                spell = Spell.None;
            }

            switch (spell)
            {
                case Spell.Slaying:
                    if (!Slaying)
                        spell = Spell.None;
                    else
                    {
                        magic = GetMagic(Spell.Slaying);
                        level = magic.Level;
                    }

                    Slaying = false;
                    break;
                case Spell.DoubleSlash:
                    magic = GetMagic(spell);
                    if (magic == null || magic.Info.BaseCost + (magic.Level * magic.Info.LevelCost) > MP)
                    {
                        spell = Spell.None;
                        break;
                    }
                    level = magic.Level;
                    ChangeMP(-(magic.Info.BaseCost + magic.Level * magic.Info.LevelCost));
                    break;
                case Spell.Thrusting:
                case Spell.FlamingSword:
                    magic = GetMagic(spell);
                    if ((magic == null) || (!FlamingSword && (spell == Spell.FlamingSword)))
                    {
                        spell = Spell.None;
                        break;
                    }
                    level = magic.Level;
                    break;
                case Spell.LuoHanGunFa:
                case Spell.DaMoGunFa:
                    magic = GetMagic(spell);
                    if (magic == null || (!DaMoGunFa && spell == Spell.DaMoGunFa))
                        spell = Spell.None;
                    break;
                case Spell.HalfMoon:
                case Spell.CrossHalfMoon:
                case Spell.FlashDash:
                case Spell.FlashDash2:
                    magic = GetMagic(spell);
                    if (magic == null || magic.Info.BaseCost + (magic.Level * magic.Info.LevelCost) > MP)
                    {
                        spell = Spell.None;
                        break;
                    }
                    level = magic.Level;
                    ChangeMP(-(magic.Info.BaseCost + magic.Level * magic.Info.LevelCost));
                    break;
                case Spell.TwinDrakeBlade:
                    magic = GetMagic(spell);
                    if (!magic.Toggle || magic == null || magic.Info.BaseCost + magic.Level * magic.Info.LevelCost > MP)
                    {
                        spell = Spell.None;
                        break;
                    }
                    level = magic.Level;
                    ChangeMP(-(magic.Info.BaseCost + magic.Level * magic.Info.LevelCost));
                    break;
                default:
                    spell = Spell.None;
                    break;
            }
        }

        public void Attack(MirDirection dir, Spell spell)
        {
            LogTime = Envir.Time + Globals.LogDelay;

            bool MoonLightAttack = false;
            bool DarkBodyAttack = false;
            bool MoonMistAttack = false;
            bool MoonMistAttack2 = false;

            if (!CanAttack)
            {
                switch (spell)
                {
                    case Spell.Slaying:
                        Slaying = false;
                        break;
                }

                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            if (Hidden)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.MoonLight:
                        case BuffType.DarkBody:
                            MoonLightAttack = true;
                            DarkBodyAttack = true;
                            Buffs[i].ExpireTime = 0;
                            break;
                    
                    }
                }
            }
            if (Hidden2)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.MoonMist:
                            MoonMistAttack = true;
                            if (Buffs[i].Values[0] > 0)
                                MoonMistAttack2 = true;
                            Buffs[i].ExpireTime = 0;
                            break;
                    }
                }
            }

            byte level = 0;
            ProcessAttackSpell(ref spell, ref level);

            UserMagic magic;
            if (!Slaying)
            {
                magic = GetMagic(Spell.Slaying);

                if (magic != null && Envir.Random.Next(12) <= magic.Level)
                {
                    Slaying = true;
                    Enqueue(new S.SpellToggle { Spell = Spell.Slaying, CanUse = Slaying });
                }
            }

            Direction = dir;
            if (RidingMount) DecreaseMountLoyalty(3);

            Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = spell, Level = level });

            AttackTime = Envir.Time + AttackSpeed;
            ActionTime = Envir.Time + AttackSpeed;
            RegenTime = Envir.Time + RegenDelay;

            Point target = Functions.PointMove(CurrentLocation, dir, 1);

            //damabeBase = the original damage from your gear (+ bonus from moonlight and darkbody)
            int damageBase;
            if (spell != Spell.LuoHanGunFa && spell != Spell.DaMoGunFa)
                damageBase = GetAttackPower(MinDC, MaxDC);
            else
                damageBase = GetAttackPower(MinSC, MaxSC);
            //damageFinal = the damage you're gonna do with skills added
            int damageFinal;

            if (MoonLightAttack || DarkBodyAttack || MoonMistAttack)
            {
                magic = MoonLightAttack ? GetMagic(Spell.MoonLight) : GetMagic(Spell.DarkBody);

                if (magic != null)
                {
                    damageBase += magic.GetPower();
                }
            }

            bool isValidPoint = CurrentMap.ValidPoint(target);
            var cellObjects = CurrentMap.GetCellObjects(target);
            if (isValidPoint && cellObjects != null)
            {
                damageFinal = damageBase;//incase we're not using skills
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                    if (ob == null || ob.Node == null) continue;
                    if (!ob.IsAttackTarget(this)) continue;

                    //Only undead targets
                    if (ob.Undead)
                    {
                        damageBase = Math.Min(int.MaxValue, damageBase + Holy);
                        damageFinal = damageBase;//incase we're not using skills
                    }

                    DefenceType defence = DefenceType.ACAgility;
                    ProcessFatalSword(ref damageFinal, damageBase, ref defence, ob);
                    ProcessMPEnter(ref damageFinal, damageBase, ref defence, ob);
                    ProcessHemorrhage(ref damageFinal, damageBase, ob);

                    ProcessAttackDamage(spell, ref damageFinal, damageBase, ref defence, ob);

                    if (MoonMistAttack2)
                    {
                        if (Envir.Random.Next(3) == 0)
                            ob.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = 5, TickSpeed = 1000 }, this);
                    }

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, ob, damageFinal, defence, true);
                    ActionList.Add(action);
                    break;
                }
            }

            ProcessAreaAttack(spell, target, dir, damageBase);
        }

        public void ProcessFatalSword(ref int damageFinal, int damageBase, ref DefenceType defence, MapObject ob)
        {
            UserMagic magic = GetMagic(Spell.FatalSword);
            if (magic == null)
                return;

            if (FatalSword)
            {
                damageFinal = magic.GetDamage(damageBase);
                LevelMagic(magic);
                S.ObjectEffect p = new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.FatalSword };

                defence = DefenceType.Agility;
                CurrentMap.Broadcast(p, ob.CurrentLocation);

                FatalSword = false;
            }

            if (!FatalSword && Envir.Random.Next(10) == 0)
                FatalSword = true;
        }

        public void GetMinePayout(MineSet Mine)
        {
            if ((Mine.Drops == null) || (Mine.Drops.Count == 0)) return;
            if (FreeSpace(Info.Inventory) == 0) return;
            byte Slot = (byte)Envir.Random.Next(Mine.TotalSlots);
            for (int i = 0; i < Mine.Drops.Count; i++)
            {
                MineDrop Drop = Mine.Drops[i];
                if ((Drop.MinSlot <= Slot) && (Drop.MaxSlot >= Slot) && (Drop.Item != null))
                {
                    UserItem item = Envir.CreateDropItem(Drop.Item);
                    if (item.Info.Type == ItemType.Ore)
                    {
                        item.CurrentDura = (ushort)Math.Min(ushort.MaxValue, (Drop.MinDura + Envir.Random.Next(Math.Max(0, Drop.MaxDura - Drop.MinDura))) * 1000);
                        if ((Drop.BonusChance > 0) && (Envir.Random.Next(100) <= Drop.BonusChance))
                            item.CurrentDura = (ushort)Math.Min(ushort.MaxValue, item.CurrentDura + (Envir.Random.Next(Drop.MaxBonusDura) * 1000));
                    }

                    if (CheckGroupQuestItem(item)) continue;

                    if (CanGainItem(item, false))
                    {
                        GainItem(item);
                        Report.ItemChanged("MinePayout", item, item.Count, 2);
                    }
                    return;
                }
            }
        }

        public void Magic(Spell spell, MirDirection dir, uint targetID, Point location)
        {
            if (!CanCast)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            UserMagic magic = GetMagic(spell);

            if (magic == null)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            if ((location.X != 0) && (location.Y != 0) && magic.Info.Range != 0 && Functions.InRange(CurrentLocation, location, magic.Info.Range) == false) return;

            if (Hidden)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.MoonLight:
                        case BuffType.DarkBody:
                            Buffs[i].ExpireTime = 0;
                            break;
                    }
                }
            }

            AttackTime = Envir.Time + MoveDelay;
            SpellTime = Envir.Time + 1800; //Spell Delay

            if (spell != Spell.ShoulderDash)
                ActionTime = Envir.Time + MoveDelay;

            LogTime = Envir.Time + Globals.LogDelay;

            long delay = magic.GetDelay();
            if (magic.Spell == Spell.SlashingBurst && Buffs.Any(e => e.Type == BuffType.ImmortalSkin))
                delay += 30000;
            else if (magic.Spell == Spell.Bisul && magic.Level == 4)
                delay = 300000;

            if (magic != null && Envir.Time < (magic.CastTime + delay) && magic.CastTime > 0)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            int cost = magic.Info.BaseCost + magic.Info.LevelCost * magic.Level;
            if (spell == Spell.Plague)
                cost = 2 * MaxSC;

            if (spell == Spell.Teleport || spell == Spell.Blink || spell == Spell.StormEscape || spell == Spell.StormEscape1)
                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].Type != BuffType.TemporalFlux) continue;
                    cost += (int)(MaxMP * 0.3F);
                    break;
                }

            if (Buffs.Any(e => e.Type == BuffType.MagicBooster))
            {
                UserMagic booster = GetMagic(Spell.MagicBooster);

                if (booster != null)
                {
                    int penalty = (int)Math.Round((decimal)(cost / 100) * (6 + booster.Level));
                    cost += penalty;
                }
            }

            if (cost > MP)
            {
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });
                return;
            }

            RegenTime = Envir.Time + RegenDelay;
            ChangeMP(-cost);

            Direction = dir;
            if (spell != Spell.ShoulderDash && spell != Spell.BackStep && spell != Spell.PoisonShot && spell != Spell.FlashDash && spell != Spell.FlashDash2)
                Enqueue(new S.UserLocation { Direction = Direction, Location = CurrentLocation });

            MapObject target = null;

            if (targetID == ObjectID)
                target = this;
            else if (targetID > 0)
                target = FindObject(targetID, 10);

            bool cast = true;
            byte level = magic.Level;
            switch (spell)
            {
                case Spell.FireBall:
                case Spell.GreatFireBall:
                case Spell.FrostCrunch:
                    if (!Fireball(target, magic)) targetID = 0;
                    break;
                case Spell.Healing:
                    if (target == null)
                    {
                        target = this;
                        targetID = ObjectID;
                    }
                    Healing(target, magic);
                    break;
                case Spell.Healing2:
                    if (target == null)
                    {
                        target = this;
                        targetID = ObjectID;
                    }
                    Healing2(target, magic);
                    break;
                case Spell.Repulsion:
                case Spell.EnergyRepulsor:
                case Spell.FireBurst:
                    Repulsion(magic);
                    break;
                case Spell.ElectricShock:
                    ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, target as MonsterObject));
                    break;
                case Spell.Poisoning:
                    if (!Poisoning(target, magic)) cast = false;
                    break;
                case Spell.Taunt:
                    if (!Taunting(target, magic)) cast = false;
                    break;
                case Spell.HellFire:
                    HellFire(magic);
                    break;
                case Spell.ThunderBolt:
                    ThunderBolt(target, magic);
                    break;
                case Spell.SoulFireBall:
                    if (!SoulFireball(target, magic, out cast)) targetID = 0;
                    break;
                case Spell.SummonSkeleton:
                    SummonSkeleton(magic);
                    break;
                case Spell.Teleport:
                case Spell.Blink:
                    ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 200, magic, location));
                    break;
                case Spell.Hiding:
                    Hiding(magic);
                    break;
                case Spell.HealingCircle:
                    HealingCircle(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.HealingCircle2:
                    HealingCircle2(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.Haste:
                case Spell.LightBody:
                    ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic));
                    break;
                case Spell.Fury:
                    FurySpell(magic, out cast);
                    break;
                case Spell.ImmortalSkin:
                    ImmortalSkin(magic, out cast);
                    break;
                case Spell.ImmortalSkin1:
                    ImmortalSkin1(magic, out cast);
                    break;
                case Spell.FireBang:
                case Spell.IceStorm:
                    FireBang(magic, target == null ? location : target.CurrentLocation);
                    break;
                case Spell.MassHiding:
                    MassHiding(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.SoulShield:
                case Spell.BlessedArmour:
                    SoulShield(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.FireWall:
                    FireWall(magic, target == null ? location : target.CurrentLocation);
                    break;
                case Spell.Lightning:
                    Lightning(magic);
                    break;
                case Spell.HeavenlySword:
                    HeavenlySword(magic);
                    break;
                case Spell.MassHealing:
                    MassHealing(magic, target == null ? location : target.CurrentLocation);
                    break;
                case Spell.ShoulderDash:
                    ShoulderDash(magic);
                    return;
                case Spell.ThunderStorm:
                case Spell.FlameField:
                    ThunderStorm(magic);
                    if (spell == Spell.FlameField)
                        SpellTime = Envir.Time + 2500; //Spell Delay
                    break;
                case Spell.StormEscape:
                    StormEscape(magic, location);
                    break;
                case Spell.StormEscape1:
                    StormEscape1(magic, location);
                    break;
                case Spell.ShiBuYiSha:
                    ShiBuYiSha(magic, location);
                    break;
                case Spell.MagicShield:
                    ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, magic.GetPower(GetAttackPower(MinMC, MaxMC) + 15)));
                    break;
                case Spell.FlameDisruptor:
                    FlameDisruptor(target, magic);
                    break;
                case Spell.TurnUndead:
                    TurnUndead(target, magic);
                    break;
                case Spell.MagicBooster:
                    MagicBooster(magic);
                    break;
                case Spell.Vampirism:
                    Vampirism(target, magic);
                    break;
                case Spell.SummonShinsu:
                    SummonShinsu(magic);
                    break;
                case Spell.Purification:
                    if (target == null)
                    {
                        target = this;
                        targetID = ObjectID;
                    }
                    Purification(target, magic);
                    break;
                case Spell.LionRoar:
                case Spell.BattleCry:
                    CurrentMap.ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, CurrentLocation));
                    break;
                case Spell.Revelation:
                    Revelation(target, magic);
                    break;
                case Spell.PoisonCloud:
                    PoisonCloud(magic, location, out cast);
                    break;
                case Spell.Entrapment:
                    Entrapment(target, magic);
                    break;
                case Spell.EntrapSwordSecret:
                    EntrapSwordSecret(target, magic);
                    break;
                case Spell.BladeAvalanche:
                    BladeAvalanche(magic);
                    break;
                case Spell.JinGangGunFa:
                    JinGangGunFa(magic);
                    break;
                case Spell.SlashingBurst:
                    SlashingBurst(magic, out cast);
                    break;
                case Spell.XiangLongGunFa:
                    XiangLongGunFa(magic, out cast);
                    break;
                case Spell.Rage:
                    Rage(magic);
                    break;
                case Spell.Mirroring:
                    Mirroring(magic);
                    break;
                case Spell.Blizzard:
                    Blizzard(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.MeteorStrike:
                    MeteorStrike(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.IceThrust:
                    IceThrust(magic);
                    break;
                case Spell.ProtectionField:
                    ProtectionField(magic);
                    break;
                case Spell.ProtectionField1:
                    ProtectionField1(magic);
                    break;
                case Spell.PetEnhancer:
                    PetEnhancer(target, magic, out cast);
                    break;
                case Spell.TrapHexagon:
                    TrapHexagon(magic, target, out cast);
                    break;
                case Spell.Reincarnation:
                    Reincarnation(magic, target == null ? null : target as PlayerObject, out cast);
                    break;
                case Spell.Curse:
                    Curse(magic, target == null ? location : target.CurrentLocation, out cast);
                    break;
                case Spell.SummonHolyDeva:
                    SummonHolyDeva(magic);
                    break;
                case Spell.Hallucination:
                    Hallucination(target, magic);
                    break;
                case Spell.EnergyShield:
                    EnergyShield(target, magic, out cast);
                    break;
                case Spell.UltimateEnhancer:
                    UltimateEnhancer(target, magic, out cast);
                    break;
                case Spell.Plague:
                    Plague(magic, target, out cast);
                    break;
                case Spell.SwiftFeet:
                    SwiftFeet(magic, out cast);
                    break;
                case Spell.MoonLight:
                    MoonLight(magic);
                    break;
                case Spell.Trap:
                    Trap(magic, target, out cast);
                    break;
                case Spell.PoisonSword:
                    PoisonSword(magic);
                    break;
                case Spell.DarkBody:
                    DarkBody(target, magic);
                    break;
                case Spell.LuoHanZhen:
                    LuoHanZhen(target, magic);
                    break;
                case Spell.FlashDash:
                    FlashDash(magic, false);
                    return;
                case Spell.FlashDash2:
                    FlashDash(magic, true);
                    return;
                case Spell.CrescentSlash:
                    CrescentSlash(magic);
                    break;
                case Spell.StraightShot:
                    if (!StraightShot(target, magic)) targetID = 0;
                    break;
                case Spell.DoubleShot:
                    if (!DoubleShot(target, magic)) targetID = 0;
                    break;
                case Spell.BackStep:
                    BackStep(magic);
                    return;
                case Spell.ExplosiveTrap:
                    ExplosiveTrap(magic, Front);
                    return;
                case Spell.DelayedExplosion:
                    if (!DelayedExplosion(target, magic)) targetID = 0;
                    break;
                case Spell.DelayedExplosion2:
                    if (!DelayedExplosion2(target, magic)) targetID = 0;
                    break;
                case Spell.Concentration:
                    Concentration(magic);
                    break;
                case Spell.ElementalShot:
                    if (!ElementalShot(target, magic)) targetID = 0;
                    break;
                case Spell.ElementalBarrier:
                    {
                        int duration = (int)(5 + GetAttackPower(MinSC, MaxSC) * 0.5F);
                        ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, duration));
                    }
                    break;
                case Spell.ElementalBarrier1:
                    {
                        ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, magic.GetPower(GetAttackPower(MinDC, MaxDC))));
                    }
                    break;
                case Spell.BindingShot:
                    BindingShot(magic, target, out cast);
                    break;
                case Spell.SummonVampire:
                case Spell.SummonToad:
                case Spell.SummonSnakes:
                    ArcherSummon(magic, target, location);
                    break;
                case Spell.VampireShot:
                case Spell.CrippleShot:
                    SpecialArrowShot(target, magic);
                    break;
                case Spell.NapalmShot:
                    NapalmShot(target, magic);
                    break;
                case Spell.NapalmShot2:
                    NapalmShot2(target, magic);
                    SpellTime = Envir.Time + AttackSpeed;
                    return;
                case Spell.OneWithNature:
                    OneWithNature(target, magic);
                    break;
                //Custom Spells
                case Spell.Portal:
                    Portal(magic, location, out cast);
                    break;
                case Spell.CounterAttack:
                    CounterAttacking(magic);
                    break;
                case Spell.CounterAttack1:
                    CounterAttacking1(magic);
                    break;
                case Spell.MoonMist:
                case Spell.MoonMist2:
                    MoonMist(magic);
                    break;
                case Spell.CatTongue:
                    CatTongue(magic, target, out cast);
                    break;
                case Spell.GreateFireBallSecret:
                    GreateFireBallSecret(magic, target, out cast);
                    break;
                case Spell.Bisul:
                    Bisul(magic, target, cast);
                    break;
                default:
                    cast = false;
                    break;
            }

            if (cast)
            {
                magic.CastTime = Envir.Time;
            }

            Enqueue(new S.Magic { Spell = spell, TargetID = targetID, Target = location, Cast = cast, Level = level });
            Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = spell, TargetID = targetID, Target = location, Cast = cast, Level = level });
        }

        private void Healing2(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsFriendlyTarget(this)) return;

            int health = magic.GetDamage(GetAttackPower(MinSC, MaxSC)) + Level;
            int duration = (GetAttackPower(MinSC, MaxSC) + 25 + magic.Level * 25) * 1000;

            target.AddBuff(new Buff { Type = BuffType.Healing2, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { health }, Visible = true });
            LevelMagic(magic);
        }

        private void ShiBuYiSha(UserMagic magic, Point location)
        {
            int damage = magic.GetDamage(GetAttackPower(MinSC, MaxSC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, location);
            ActionList.Add(action);
        }

        private void Bisul(UserMagic magic, MapObject target, bool cast)
        {
            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic));
            cast = true;
        }

        private void MoonMist(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinDC, MaxDC));
            if (Buffs.Any(e => e.Type == BuffType.MoonMist)) return;

            if (magic.Spell == Spell.MoonMist)
                AddBuff(new Buff { Type = BuffType.MoonMist, Caster = this, ExpireTime = Envir.Time + (magic.Level * 3 + 10) * 1000, Visible = true, Values = new int[] { 0 } });
            else
                AddBuff(new Buff { Type = BuffType.MoonMist, Caster = this, ExpireTime = Envir.Time + (magic.Level * 3 + 15) * 1000, Visible = true, Values = new int[] { 1 } });

            CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MoonMist }, CurrentLocation);
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 2000, this, magic, damage, CurrentLocation);
            CurrentMap.ActionList.Add(action);
            LevelMagic(magic);
        }

        private void HealingCircle(UserMagic magic, Point location, out bool cast)
        {
            cast = false;
            UserItem item = GetAmulet(10);
            if (item == null) return;
            cast = true;

            int value = magic.GetDamage(GetAttackPower(MinSC, MaxSC)) + 40 * (magic.Level + 1);

            long duration = (magic.Level + 1) * 5000;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location, duration);
            CurrentMap.ActionList.Add(action);

            ConsumeItem(item, 10);
        }

        private void HealingCircle2(UserMagic magic, Point location, out bool cast)
        {
            cast = false;
            if (Buffs.Any(e => e.Type == BuffType.HealingCircle2)) return;

            UserItem item = GetAmulet(10);
            if (item == null) return;
            cast = true;

            int value = magic.GetDamage(GetAttackPower(MinSC, MaxSC)) + 40 * (magic.Level + 1);

            long duration = (magic.Level + 1) * 5000;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location, duration);
            CurrentMap.ActionList.Add(action);

            int factor = (int)(magic.Level * 10 + 20);
            AddBuff(new Buff { Type = BuffType.HealingCircle2, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { factor }, Visible = true });

            ConsumeItem(item, 10);
        }

        private void CounterAttacking(UserMagic magic)
        {
            if (CounterAttack || Envir.Time < CounterAttackTime) return;

            CounterAttack = true;
            CounterAttackTime = Envir.Time + 5000 + 250 * GetAttackPower(MinSC, MaxSC);
            AddBuff(new Buff { Type = BuffType.CounterAttack, Caster = this, ExpireTime = CounterAttackTime, Values = new int[] { 90 }, Visible = true });
            LevelMagic(magic);
        }

        private void CounterAttacking1(UserMagic magic)
        {
            if (CounterAttack || Envir.Time < CounterAttackTime) return;

            CounterAttack = true;
            CounterAttackTime = Envir.Time + 7000;
            AddBuff(new Buff { Type = BuffType.CounterAttack1, Caster = this, ExpireTime = CounterAttackTime, Values = new int[] { 11 + magic.Level * 3, 60 + magic.Level*10 }, Visible = true });
            LevelMagic(magic);
        }

        private bool Taunting(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return false;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, target);
            ActionList.Add(action);
            return true;
        }

        #region Elemental System
        private void Concentration(UserMagic magic)
        {
            int duration = 45 + (15 * magic.Level);
            int count = Buffs.Where(x => x.Type == BuffType.Concentration).ToList().Count();
            if (count > 0) return;

            AddBuff(new Buff { Type = BuffType.Concentration, Caster = this, ExpireTime = Envir.Time + duration * 1000, Values = new int[] { magic.Level } });

            LevelMagic(magic);

            ConcentrateInterruptTime = 0;
            ConcentrateInterrupted = false;
            Concentrating = true;
            UpdateConcentration();//Update & send to client

            OperateTime = 0;
        }
        public void UpdateConcentration()
        {
            Enqueue(new S.SetConcentration { ObjectID = ObjectID, Enabled = Concentrating, Interrupted = ConcentrateInterrupted });
            Broadcast(new S.SetObjectConcentration { ObjectID = ObjectID, Enabled = Concentrating, Interrupted = ConcentrateInterrupted });
        }
        private bool ElementalShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return false;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return false;

            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinDC, MaxDC));
            damage = ApplyDistanceBonus(damage, distance);//ElementalShot

            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);
            ActionList.Add(action);
            return true;
        }

        public void GatherElement()
        {
            UserMagic magic = GetMagic(Spell.Meditation);

            if (magic == null) return;

            int MeditationLvl = magic.Level;

            magic = GetMagic(Spell.Concentration);

            int ConcentrateLvl = magic != null ? magic.Level : -1;

            int MeditateChance = 0;
            int ConcentrateChance = 0;

            if (Concentrating && !ConcentrateInterrupted && ConcentrateLvl >= 0)
                ConcentrateChance = 1 + ConcentrateLvl;

            if (MeditationLvl >= 0)
            {
                MeditateChance = (8 - MeditationLvl);
                int rnd = Envir.Random.Next(10);
                if (rnd >= (MeditateChance - ConcentrateChance))
                {
                    ObtainElement(false);
                    LevelMagic(GetMagic(Spell.Meditation));
                }
            }
        }
        public void ObtainElement(bool cast)
        {
            int orbType = 0;
            int meditateLevel = 0;

            UserMagic spell = GetMagic(Spell.Meditation);

            if (spell == null)
            {
                ReceiveChat(Envir.Tr("Skill requires meditation."), ChatType.System);
                return;
            }

            meditateLevel = spell.Level;

            int maxOrbs = (int)Settings.OrbsExpList[Settings.OrbsExpList.Count - 1];

            if (cast)
            {
                ElementsLevel = (int)Settings.OrbsExpList[0];
                orbType = 1;
                if (Settings.GatherOrbsPerLevel)//Meditation Orbs per level
                    if (meditateLevel == 3)
                    {
                        Enqueue(new S.SetElemental { ObjectID = ObjectID, Enabled = true, Value = (uint)Settings.OrbsExpList[0], ElementType = 1, ExpLast = (uint)maxOrbs });
                        Broadcast(new S.SetObjectElemental { ObjectID = ObjectID, Enabled = true, Casted = true, Value = (uint)Settings.OrbsExpList[0], ElementType = 1, ExpLast = (uint)maxOrbs });
                        ElementsLevel = (int)Settings.OrbsExpList[1];
                        orbType = 2;
                    }

                HasElemental = true;
            }
            else
            {
                HasElemental = false;
                ElementsLevel++;

                if (Settings.GatherOrbsPerLevel)//Meditation Orbs per level
                    if (ElementsLevel > Settings.OrbsExpList[GetMagic(Spell.Meditation).Level])
                    {
                        HasElemental = true;
                        ElementsLevel = (int)Settings.OrbsExpList[GetMagic(Spell.Meditation).Level];
                        return;
                    }

                if (ElementsLevel >= Settings.OrbsExpList[0]) HasElemental = true;
                for (int i = 0; i <= Settings.OrbsExpList.Count - 1; i++)
                {
                    if (Settings.OrbsExpList[i] != ElementsLevel) continue;
                    orbType = i + 1;
                    break;
                }
            }

            Enqueue(new S.SetElemental { ObjectID = ObjectID, Enabled = HasElemental, Value = (uint)ElementsLevel, ElementType = (uint)orbType, ExpLast = (uint)maxOrbs });
            Broadcast(new S.SetObjectElemental { ObjectID = ObjectID, Enabled = HasElemental, Casted = cast, Value = (uint)ElementsLevel, ElementType = (uint)orbType, ExpLast = (uint)maxOrbs });
        }

        public int GetElementalOrbCount()
        {
            int OrbCount = 0;
            for (int i = Settings.OrbsExpList.Count - 1; i >= 0; i--)
            {
                if (ElementsLevel >= Settings.OrbsExpList[i])
                {
                    OrbCount = i + 1;
                    break;
                }
            }
            return OrbCount;
        }
        public int GetElementalOrbPower(bool defensive)
        {
            // if (!HasElemental) return 0;

            int orbCount = GetElementalOrbCount() - 1;
            if (defensive)
            {
                orbCount = Math.Min(Settings.OrbsDefList.Count - 1, orbCount);
                return (int)Settings.OrbsDefList[orbCount];
            }
            else
            {
                orbCount = Math.Min(Settings.OrbsDmgList.Count - 1, orbCount);
                return (int)Settings.OrbsDmgList[orbCount];
            }
        }
        #endregion

        #region Wizard Skills
        private bool Fireball(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);
            ActionList.Add(action);

            return true;
        }
        private void Repulsion(UserMagic magic)
        {
            bool result = false;
            for (int d = 0; d <= 1; d++)
            {
                for (int y = CurrentLocation.Y - d; y <= CurrentLocation.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = CurrentLocation.X - d; x <= CurrentLocation.X + d; x += Math.Abs(y - CurrentLocation.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;

                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; cellObjects != null && i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player) continue;

                            if (!ob.IsAttackTarget(this) || ob.Level >= Level) continue;

                            if (Envir.Random.Next(20) >= 6 + magic.Level * 3 + Level - ob.Level) continue;

                            int distance = 1 + Math.Max(0, magic.Level - 1) + Envir.Random.Next(2);
                            MirDirection dir = Functions.DirectionFromPoint(CurrentLocation, ob.CurrentLocation);

                            if (ob.Pushed(this, dir, distance) == 0) continue;

                            if (ob.Race == ObjectType.Player)
                            {
                                SafeZoneInfo szi = CurrentMap.GetSafeZone(ob.CurrentLocation);

                                if (szi != null)
                                {
                                    ((PlayerObject)ob).BindLocation = szi.Location;
                                    ((PlayerObject)ob).BindMapIndex = CurrentMapIndex;
                                    ob.InSafeZone = true;
                                }
                                else
                                    ob.InSafeZone = false;

                                ob.Attacked(this, magic.GetDamage(0), DefenceType.None, false);
                            }
                            result = true;
                        }
                    }
                }
            }

            if (result) LevelMagic(magic);
        }
        private void ElectricShock(MonsterObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;

            if (Envir.Random.Next(4 - magic.Level) > 0)
            {
                if (Envir.Random.Next(2) == 0) LevelMagic(magic);
                return;
            }

            LevelMagic(magic);

            if (target.Master == this)
            {
                target.ShockTime = Envir.Time + (magic.Level * 5 + 10) * 1000;
                target.Target = null;
                return;
            }

            if (Envir.Random.Next(2) > 0)
            {
                target.ShockTime = Envir.Time + (magic.Level * 5 + 10) * 1000;
                target.Target = null;
                return;
            }

            if (target.Level > Level + 2 || !target.Info.CanTame) return;

            if (Envir.Random.Next(Level + 20 + magic.Level * 5) <= target.Level + 10)
            {
                if (Envir.Random.Next(5) > 0 && target.Master == null)
                {
                    target.RageTime = Envir.Time + (Envir.Random.Next(20) + 10) * 1000;
                    target.Target = null;
                }
                return;
            }

            if (Pets.Count(t => !t.Dead) >= magic.Level + 2) return;
            int rate = (int)(target.MaxHP / 100);
            if (rate <= 2) rate = 2;
            else rate *= 2;

            if (Envir.Random.Next(rate) != 0) return;
            //else if (Envir.Random.Next(20) == 0) target.Die();

            if (target.Master != null)
            {
                target.SetHP(target.MaxHP / 10);
                target.Master.Pets.Remove(target);
            }
            else if (target.Respawn != null)
            {
                target.Respawn.Count--;
                Envir.MonsterCount--;
                CurrentMap.MonsterCount--;
                target.Respawn = null;
            }

            target.Master = this;
            //target.HealthChanged = true;
            target.BroadcastHealthChange();
            Pets.Add(target);
            target.Target = null;
            target.RageTime = 0;
            target.ShockTime = 0;
            target.OperateTime = 0;
            target.MaxPetLevel = (byte)(1 + magic.Level * 2);
            target.TameTime = Envir.Time + (Settings.Minute * 60 * 4);

            target.Broadcast(new S.ObjectName { ObjectID = target.ObjectID, Name = target.Name });
        }
        private void HellFire(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, Direction, 4);
            CurrentMap.ActionList.Add(action);

            if (magic.Level != 3) return;

            MirDirection dir = (MirDirection)(((int)Direction + 1) % 8);
            action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, dir, 4);
            CurrentMap.ActionList.Add(action);

            dir = (MirDirection)(((int)Direction - 1 + 8) % 8);
            action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, dir, 4);
            CurrentMap.ActionList.Add(action);
        }
        private void ThunderBolt(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            if (target.Undead) damage = (int)(damage * 1.5F);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, target);

            ActionList.Add(action);
        }
        private void Vampirism(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, target);

            ActionList.Add(action);
        }
        private void FireBang(UserMagic magic, Point location)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, location);
            CurrentMap.ActionList.Add(action);
        }
        private void FireWall(UserMagic magic, Point location)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, location);
            CurrentMap.ActionList.Add(action);
        }
        private void Lightning(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, Direction);
            CurrentMap.ActionList.Add(action);
        }
        private void TurnUndead(MapObject target, UserMagic magic)
        {
            if (target == null || target.Race != ObjectType.Monster || !target.Undead || !target.IsAttackTarget(this)) return;

            if (Envir.Random.Next(2) + Level - 1 <= target.Level)
            {
                target.Target = this;
                return;
            }

            int dif = Level - target.Level + 15;

            if (Envir.Random.Next(100) >= (magic.Level + 1 << 3) + dif)
            {
                target.Target = this;
                return;
            }

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, target);
            ActionList.Add(action);
        }
        private void FlameDisruptor(MapObject target, UserMagic magic)
        {
            if (target == null || (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) || !target.IsAttackTarget(this)) return;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            if (target.Race == ObjectType.Monster && !target.Undead) damage = (int)(damage * 1.5F);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, damage, target);

            ActionList.Add(action);
        }
        private void ThunderStorm(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation);
            CurrentMap.ActionList.Add(action);
        }

        private void StormEscape(UserMagic magic, Point location)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation);
            CurrentMap.ActionList.Add(action);
            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 750, magic, location));
        }
        private void StormEscape1(UserMagic magic, Point location)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation);
            CurrentMap.ActionList.Add(action);
            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 750, magic, location));
        }
        private void Mirroring(UserMagic magic)
        {
            MonsterObject monster;
            DelayedAction action;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.CloneName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, monster, Front, true);
                CurrentMap.ActionList.Add(action);
                return;
            }

            MonsterInfo info = Envir.GetMonsterInfo(Settings.CloneName);
            if (info == null) return;


            LevelMagic(magic);

            monster = MonsterObject.GetMonster(info);
            monster.Master = this;
            monster.ActionTime = Envir.Time + 1000;
            monster.RefreshNameColour(false);

            Pets.Add(monster);

            action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, monster, Front, false);
            CurrentMap.ActionList.Add(action);
        }
        private void Blizzard(UserMagic magic, Point location, out bool cast)
        {
            cast = false;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            if (Buffs.FirstOrDefault(x => x.Type == BuffType.Bisul) == null)
            {
                ChannelingTime = Envir.Time + Globals.BlizzardCastTime + Globals.BlizzardGuideTime - 1500;
                ChannelingId = ++NextChannelingId;
            }

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, location, ChannelingId);
            CurrentMap.ActionList.Add(action);
            cast = true;
        }
        private void MeteorStrike(UserMagic magic, Point location, out bool cast)
        {
            cast = false;

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            if (Buffs.FirstOrDefault(x => x.Type == BuffType.Bisul) == null)
            {
                ChannelingId = ++NextChannelingId;
                ChannelingTime = Envir.Time + Globals.BlizzardCastTime + Globals.BlizzardGuideTime;
            }

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + Globals.BlizzardCastTime, this, magic, damage, location, ChannelingId);
            CurrentMap.ActionList.Add(action);
            cast = true;
        }

        private void IceThrust(UserMagic magic)
        {
            int damageBase = GetAttackPower(MinMC, MaxMC);
            if (Envir.Random.Next(100) <= (1 + Luck))
                damageBase += damageBase;
            int damageFinish = magic.GetDamage(damageBase);

            Point location = Functions.PointMove(CurrentLocation, Direction, 1);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 1500, this, magic, location, Direction, damageFinish, (int)(damageFinish * 0.6));

            CurrentMap.ActionList.Add(action);
        }

        private void MagicBooster(UserMagic magic)
        {
            int bonus = 6 + magic.Level * 6;

            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, bonus));
        }

        #endregion

        #region Taoist Skills
        private void Healing(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsFriendlyTarget(this)) return;

            int health = magic.GetDamage(GetAttackPower(MinSC, MaxSC) * 2) + Level;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, health, target);

            ActionList.Add(action);
        }
        private bool Poisoning(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return false;

            UserItem item = GetPoison(1);
            if (item == null) return false;

            int power = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            PoisonType type = item.Info.Shape == 1 ? PoisonType.Green : PoisonType.Red;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, power, target, type);
            ActionList.Add(action);
            ConsumeItem(item, 1);
            return true;
        }
        private bool SoulFireball(MapObject target, UserMagic magic, out bool cast)
        {
            cast = false;
            UserItem item = GetAmulet(1);
            if (item == null) return false;
            cast = true;

            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int damage = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);

            ActionList.Add(action);
            ConsumeItem(item, 1);

            return true;
        }
        private void SummonSkeleton(UserMagic magic)
        {
            MonsterObject monster;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.SkeletonName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                monster.ActionList.Add(new DelayedAction(DelayedType.Recall, Envir.Time + 500));
                return;
            }

            if (Pets.Where(x => x.Race == ObjectType.Monster).Count() > 1) return;

            var cellObjects = CurrentMap.GetCellObjects(Front);
            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject target = cellObjects[i];
                    switch (target.Race)
                    {
                        case ObjectType.Merchant:
                            return;
                    }
                }

            UserItem item = GetAmulet(1);
            if (item == null) return;

            MonsterInfo info = Envir.GetMonsterInfo(Settings.SkeletonName);
            if (info == null) return;


            LevelMagic(magic);
            ConsumeItem(item, 1);

            monster = MonsterObject.GetMonster(info);
            monster.PetLevel = magic.Level;
            monster.Master = this;
            monster.MaxPetLevel = (byte)(4 + magic.Level);
            monster.ActionTime = Envir.Time + 1000;
            monster.RefreshNameColour(false);

            //Pets.Add(monster);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, monster, Front);
            CurrentMap.ActionList.Add(action);
        }
        private void Purification(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsFriendlyTarget(this)) return;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, target);

            ActionList.Add(action);
        }
        private void SummonShinsu(UserMagic magic)
        {
            MonsterObject monster;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.ShinsuName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                monster.ActionList.Add(new DelayedAction(DelayedType.Recall, Envir.Time + 500));
                return;
            }

            if (Pets.Where(x => x.Race == ObjectType.Monster).Count() > 1) return;
            var cellObjects = CurrentMap.GetCellObjects(Front);
            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject target = cellObjects[i];
                    switch (target.Race)
                    {
                        case ObjectType.Merchant:
                            return;
                    }
                }

            UserItem item = GetAmulet(5);
            if (item == null) return;

            MonsterInfo info = Envir.GetMonsterInfo(Settings.ShinsuName);
            if (info == null) return;


            LevelMagic(magic);
            ConsumeItem(item, 5);


            monster = MonsterObject.GetMonster(info);
            monster.PetLevel = magic.Level;
            monster.Master = this;
            monster.MaxPetLevel = (byte)(1 + magic.Level * 2);
            monster.Direction = Direction;
            monster.ActionTime = Envir.Time + 1000;

            //Pets.Add(monster);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, monster, Front);
            CurrentMap.ActionList.Add(action);
        }
        private void Hiding(UserMagic magic)
        {
            UserItem item = GetAmulet(1);
            if (item == null) return;

            ConsumeItem(item, 1);

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, GetAttackPower(MinSC, MaxSC) + (magic.Level + 1) * 5);
            ActionList.Add(action);

        }
        private void MassHiding(UserMagic magic, Point location, out bool cast)
        {
            cast = false;
            UserItem item = GetAmulet(1);
            if (item == null) return;
            cast = true;

            int delay = Functions.MaxDistance(CurrentLocation, location) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, GetAttackPower(MinSC, MaxSC) / 2 + (magic.Level + 1) * 2, location);
            CurrentMap.ActionList.Add(action);
        }
        private void SoulShield(UserMagic magic, Point location, out bool cast)
        {
            cast = false;
            UserItem item = GetAmulet(1);
            if (item == null) return;
            cast = true;

            int delay = Functions.MaxDistance(CurrentLocation, location) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, GetAttackPower(MinSC, MaxSC) * 2 + (magic.Level + 1) * 10, location);
            CurrentMap.ActionList.Add(action);

            ConsumeItem(item, 1);
        }
        private void MassHealing(UserMagic magic, Point location)
        {
            int value = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location);
            CurrentMap.ActionList.Add(action);
        }
        private void Revelation(MapObject target, UserMagic magic)
        {
            if (target == null) return;

            int value = GetAttackPower(MinSC, MaxSC) + magic.GetPower();

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, value, target);

            ActionList.Add(action);
        }
        private void PoisonCloud(UserMagic magic, Point location, out bool cast)
        {
            cast = false;

            UserItem amulet = GetAmulet(1);
            if (amulet == null) return;

            UserItem poison = GetPoison(1, 1);
            if (poison == null) return;

            int delay = Functions.MaxDistance(CurrentLocation, location) * 50 + 500; //50 MS per Step
            int damage = magic.GetDamage(GetAttackPower(MinSC, MaxSC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, damage, location, (byte)Envir.Random.Next(PoisonAttack));

            ConsumeItem(amulet, 1);
            ConsumeItem(poison, 1);

            CurrentMap.ActionList.Add(action);
            cast = true;
        }
        private void TrapHexagon(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;

            if (target == null || !target.IsAttackTarget(this) || !(target is MonsterObject)) return;
            if (target.Level > Level + 2) return;

            UserItem item = GetAmulet(1);
            Point location = target.CurrentLocation;

            if (item == null) return;

            LevelMagic(magic);
            uint duration = (uint)((magic.Level * 5 + 10) * 1000);
            int value = (int)duration;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location);
            CurrentMap.ActionList.Add(action);

            ConsumeItem(item, 1);
            cast = true;
        }
        private void Reincarnation(UserMagic magic, PlayerObject target, out bool cast)
        {
            cast = true;

            if (target == null || !target.Dead) return;

            // checks for amulet of revival
            UserItem item = GetAmulet(1, 3);
            if (item == null) return;

            if (!ActiveReincarnation && !ReincarnationReady)
            {
                cast = false;
                int CastTime = Math.Abs(((magic.Level + 1) * 1000) - 9000);
                ExpireTime = Envir.Time + CastTime;
                ReincarnationReady = true;
                ActiveReincarnation = true;
                ReincarnationTarget = target;
                ReincarnationExpireTime = ExpireTime + 5000;

                target.ReincarnationHost = this;

                SpellObject ob = new SpellObject
                {
                    Spell = Spell.Reincarnation,
                    ExpireTime = ExpireTime,
                    TickSpeed = 1000,
                    Caster = this,
                    CurrentLocation = CurrentLocation,
                    CastLocation = CurrentLocation,
                    Show = true,
                    CurrentMap = CurrentMap,
                };
                Packet p = new S.Chat { Message = string.Format(Envir.Tr("{0} is attempting to revive {1}"), Name, target.Name), Type = ChatType.Shout };

                for (int i = 0; i < CurrentMap.Players.Count; i++)
                {
                    if (!Functions.InRange(CurrentLocation, CurrentMap.Players[i].CurrentLocation, Globals.DataRange * 2)) continue;
                    CurrentMap.Players[i].Enqueue(p);
                }

                CurrentMap.AddObject(ob);
                ob.Spawned();
                ConsumeItem(item, 1);
                // chance of failing Reincarnation when casting
                if (Envir.Random.Next(30) > (1 + magic.Level) * 10)
                {
                    return;
                }

                DelayedAction action = new DelayedAction(DelayedType.Magic, ExpireTime, magic);

                ActionList.Add(action);
                return;
            }
            return;
        }
        private void SummonHolyDeva(UserMagic magic)
        {
            MonsterObject monster;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.AngelName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                monster.ActionList.Add(new DelayedAction(DelayedType.Recall, Envir.Time + 500));
                return;
            }

            if (Pets.Where(x => x.Race == ObjectType.Monster).Count() > 1) return;
            var cellObjects = CurrentMap.GetCellObjects(Front);
            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject target = cellObjects[i];
                    switch (target.Race)
                    {
                        case ObjectType.Merchant:
                            return;
                    }
                }

            UserItem item = GetAmulet(2);
            if (item == null) return;

            MonsterInfo info = Envir.GetMonsterInfo(Settings.AngelName);
            if (info == null) return;

            LevelMagic(magic);
            ConsumeItem(item, 2);

            monster = MonsterObject.GetMonster(info);
            monster.PetLevel = magic.Level;
            monster.Master = this;
            monster.MaxPetLevel = (byte)(1 + magic.Level * 2);
            monster.Direction = Direction;
            monster.ActionTime = Envir.Time + 1000;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 1500, this, magic, monster, Front);
            CurrentMap.ActionList.Add(action);
        }
        private void Hallucination(MapObject target, UserMagic magic)
        {
            if (target == null || target.Race != ObjectType.Monster || !target.IsAttackTarget(this)) return;

            int damage = 0;
            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, delay, magic, damage, target);

            ActionList.Add(action);
        }
        private void EnergyShield(MapObject target, UserMagic magic, out bool cast)
        {
            cast = false;

            if (target == null || !target.IsFriendlyTarget(this)) target = this; //offical is only party target

            int duration = 30 + 50 * magic.Level;
            int power = magic.GetPower(GetAttackPower(MinSC, MaxSC));
            int chance = 9 - (Luck / 3 + magic.Level);

            int[] values = { chance < 2 ? 2 : chance, power };

            switch (target.Race)
            {
                case ObjectType.Player:
                    //Only targets
                    if (target.IsFriendlyTarget(this))
                    {
                        target.AddBuff(new Buff { Type = BuffType.EnergyShield, Caster = this, ExpireTime = Envir.Time + duration * 1000, Visible = true, Values = values });
                        target.OperateTime = 0;
                        LevelMagic(magic);
                        cast = true;
                    }
                    break;
            }
        }
        private void UltimateEnhancer(MapObject target, UserMagic magic, out bool cast)
        {
            cast = false;

            if (target == null || !target.IsFriendlyTarget(this)) return;
            UserItem item = GetAmulet(1);
            if (item == null) return;

            long expiretime = GetAttackPower(MinSC, MaxSC) * 2 + (magic.Level + 1) * 10;
            int value = MaxSC >= 5 ? Math.Min(8, MaxSC / 5) : 1;

            switch (target.Race)
            {
                case ObjectType.Monster:
                case ObjectType.Player:
                    //Only targets
                    if (target.IsFriendlyTarget(this))
                    {
                        target.AddBuff(new Buff { Type = BuffType.UltimateEnhancer, Caster = this, ExpireTime = Envir.Time + expiretime * 1000, Values = new int[] { value } });
                        target.OperateTime = 0;
                        LevelMagic(magic);
                        ConsumeItem(item, 1);
                        cast = true;
                    }
                    break;
            }
        }
        private void Plague(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;

            if (target == null)
                return;
            UserItem item = GetAmulet(1);
            if (item == null) return;
            cast = true;

            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            PoisonType pType = PoisonType.None;

            UserItem itemp = GetPoison(1, 1);

            if (itemp != null)
                pType = PoisonType.Green;
            else
            {
                itemp = GetPoison(1, 2);

                if (itemp != null)
                    pType = PoisonType.Red;
            }

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, magic.GetDamage(MaxSC), target, pType);
            ActionList.Add(action);

            ConsumeItem(item, 1);
            if (itemp != null) ConsumeItem(itemp, 1);
        }
        private void Curse(UserMagic magic, Point location, out bool cast)
        {
            cast = false;
            UserItem item = GetAmulet(1);
            if (item == null) return;
            cast = true;

            ConsumeItem(item, 1);

            if (Envir.Random.Next(10 - ((magic.Level + 1) * 2)) > 2) return;

            int delay = Functions.MaxDistance(CurrentLocation, location) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, magic.GetDamage(GetAttackPower(MinSC, MaxSC)), location, 1 + ((magic.Level + 1) * 2));
            CurrentMap.ActionList.Add(action);
        }

        private void PetEnhancer(MapObject target, UserMagic magic, out bool cast)
        {
            cast = false;

            if (target == null || target.Race != ObjectType.Monster || !target.IsFriendlyTarget(this)) return;

            int duration = GetAttackPower(MinSC, MaxSC) + magic.GetPower();

            cast = true;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, duration, target);

            ActionList.Add(action);
        }
        #endregion

        #region Warrior Skills
        private void Entrapment(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, 0, target);
            ActionList.Add(action);
        }

        private void EntrapSwordSecret(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic, 0, target);
            ActionList.Add(action);
        }

        private void BladeAvalanche(UserMagic magic)
        {
            int damageBase = GetAttackPower(MinDC, MaxDC);
            if (Envir.Random.Next(0, 100) <= (1 + Luck))
                damageBase += damageBase;//crit should do something like double dmg, not double max dc dmg!
            int damageFinal = magic.GetDamage(damageBase);

            int col = 3;
            int row = 3;

            Point[] loc = new Point[col]; //0 = left 1 = center 2 = right
            loc[0] = Functions.PointMove(CurrentLocation, Functions.PreviousDir(Direction), 1);
            loc[1] = Functions.PointMove(CurrentLocation, Direction, 1);
            loc[2] = Functions.PointMove(CurrentLocation, Functions.NextDir(Direction), 1);
            bool train = false;
            for (int i = 0; i < col; i++)
            {
                Point startPoint = loc[i];
                for (int j = 0; j < row; j++)
                {
                    Point hitPoint = Functions.PointMove(startPoint, Direction, j);

                    if (!CurrentMap.ValidPoint(hitPoint)) continue;

                    var cellObjects = CurrentMap.GetCellObjects(hitPoint);

                    if (cellObjects == null) continue;

                    for (int k = 0; k < cellObjects.Count; k++)
                    {
                        MapObject target = cellObjects[k];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (target.IsAttackTarget(this))
                                {
                                    if (target.Attacked(this, j <= 1 ? damageFinal : (int)(damageFinal * 0.6), DefenceType.AC, false) > 0)
                                        train = true;
                                }
                                break;
                        }
                    }
                }
            }

            if (train)
                LevelMagic(magic);
        }
        private void ProtectionField(UserMagic magic)
        {
            int duration = 10000 + GetAttackPower(MinSC, MaxSC) * 800;
            int value = 18 + 4 * magic.Level;
            AddBuff(new Buff { Type = BuffType.ProtectionField, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { value } });
            OperateTime = 0;
            LevelMagic(magic);
        }

        private void ProtectionField1(UserMagic magic)
        {
            int duration = (45 + (15 * magic.Level)) * 1000;
            int value = (int)Math.Round(MaxAC * (0.2 + (0.03 * magic.Level)));

            AddBuff(new Buff { Type = BuffType.ProtectionField1, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { value } });
            OperateTime = 0;
            LevelMagic(magic);
        }

        private void Rage(UserMagic magic)
        {
            int count = Buffs.Where(x => x.Type == BuffType.Rage).ToList().Count();
            if (count > 0) return;

            //     int duration = 48 + (6 * magic.Level);
            //   int value = (int)Math.Round(MaxDC * (0.12 + (0.03 * magic.Level)));
            int duration = 18 + 6 * magic.Level;
            int value = 5 + magic.Level * 5;

            AddBuff(new Buff { Type = BuffType.Rage, Caster = this, ExpireTime = Envir.Time + duration * 1000, Values = new int[] { value } });
            OperateTime = 0;
            LevelMagic(magic);
        }
        private void ShoulderDash(UserMagic magic)
        {
            if (InTrapRock) return;
            if (!CanWalk) return;
            ActionTime = Envir.Time + MoveDelay;

            int dist = Envir.Random.Next(2) + magic.Level + 2;
            int travel = 0;
            bool wall = true;
            Point location = CurrentLocation;
            MapObject target = null;
            for (int i = 0; i < dist; i++)
            {
                location = Functions.PointMove(location, Direction, 1);

                if (!CurrentMap.ValidPoint(location)) break;

                var cellObjects = CurrentMap.GetCellObjects(location);

                bool blocking = false;

                if (InSafeZone) blocking = true;

                SafeZoneInfo szi = CurrentMap.GetSafeZone(location);

                if (szi != null)
                {
                    blocking = true;
                }

                if (cellObjects != null)
                {
                    for (int c = cellObjects.Count - 1; c >= 0; c--)
                    {
                        MapObject ob = cellObjects[c];
                        if (!ob.Blocking) continue;
                        wall = false;
                        if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player)
                        {
                            blocking = true;
                            break;
                        }

                        if (target == null && ob.Race == ObjectType.Player)
                            target = ob;

                        if (Envir.Random.Next(20) >= 6 + magic.Level * 3 + Level - ob.Level || !ob.IsAttackTarget(this) || ob.Level >= Level || ob.Pushed(this, Direction, 1) == 0)
                        {
                            if (target == ob)
                                target = null;
                            blocking = true;
                            break;
                        }

                        if (cellObjects == null) break;

                    }
                }

                if (blocking)
                {
                    if (magic.Level != 3) break;

                    Point location2 = Functions.PointMove(location, Direction, 1);

                    if (!CurrentMap.ValidPoint(location2)) break;

                    szi = CurrentMap.GetSafeZone(location2);

                    if (szi != null)
                    {
                        break;
                    }

                    cellObjects = CurrentMap.GetCellObjects(location2);

                    blocking = false;


                    if (cellObjects != null)
                    {
                        for (int c = cellObjects.Count - 1; c >= 0; c--)
                        {
                            MapObject ob = cellObjects[c];
                            if (!ob.Blocking) continue;
                            if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player)
                            {
                                blocking = true;
                                break;
                            }

                            if (!ob.IsAttackTarget(this) || ob.Level >= Level || ob.Pushed(this, Direction, 1) == 0)
                            {
                                blocking = true;
                                break;
                            }

                            if (cellObjects == null) break;
                        }
                    }

                    if (blocking) break;

                    cellObjects = CurrentMap.GetCellObjects(location);

                    if (cellObjects != null)
                    {
                        for (int c = cellObjects.Count - 1; c >= 0; c--)
                        {
                            MapObject ob = cellObjects[c];
                            if (!ob.Blocking) continue;
                            if (ob.Race != ObjectType.Monster && ob.Race != ObjectType.Player)
                            {
                                blocking = true;
                                break;
                            }

                            if (Envir.Random.Next(20) >= 6 + magic.Level * 3 + Level - ob.Level || !ob.IsAttackTarget(this) || ob.Level >= Level || ob.Pushed(this, Direction, 1) == 0)
                            {
                                blocking = true;
                                break;
                            }

                            if (cellObjects == null) break;
                        }
                    }

                    if (blocking) break;
                }

                travel++;
                CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
                RemoveObjects(Direction, 1);

                CurrentLocation = location;

                Enqueue(new S.UserDash { Direction = Direction, Location = location });
                Broadcast(new S.ObjectDash { ObjectID = ObjectID, Direction = Direction, Location = location });

                CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
                AddObjects(Direction, 1);
            }

            if (travel > 0 && !wall)
            {

                if (target != null) target.Attacked(this, magic.GetDamage(0), DefenceType.None, false);
                LevelMagic(magic);
            }

            if (travel > 0)
            {
                SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

                if (szi != null)
                {
                    BindLocation = szi.Location;
                    BindMapIndex = CurrentMapIndex;
                    InSafeZone = true;
                }
                else
                    InSafeZone = false;

                ActionTime = Envir.Time + (travel * MoveDelay / 2);

                var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
                if (cellObjects != null)
                {
                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        if (cellObjects[i].Race != ObjectType.Spell) continue;
                        SpellObject ob = (SpellObject)cellObjects[i];

                        if (ob.Spell != Spell.FireWall || !IsAttackTarget(ob.Caster)) continue;
                        Attacked(ob.Caster, ob.Value, DefenceType.MAC, false);
                        break;
                    }
                }
            }

            if (travel == 0 || wall && dist != travel)
            {
                if (travel > 0)
                {
                    Enqueue(new S.UserDash { Direction = Direction, Location = Front });
                    Broadcast(new S.ObjectDash { ObjectID = ObjectID, Direction = Direction, Location = Front });
                }
                else
                    Broadcast(new S.ObjectDash { ObjectID = ObjectID, Direction = Direction, Location = Front });

                Enqueue(new S.UserDashFail { Direction = Direction, Location = CurrentLocation });
                Broadcast(new S.ObjectDashFail { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                ReceiveChat(Envir.Tr("Not enough pushing Power."), ChatType.System);
            }


            magic.CastTime = Envir.Time;
            _stepCounter = 0;
            //ActionTime = Envir.Time + GetDelayTime(MoveDelay);

            Enqueue(new S.MagicCast { Spell = magic.Spell });

            CellTime = Envir.Time + 500;
        }

        private void SlashingBurst(UserMagic magic, out bool cast)
        {
            cast = true;

            SpellTime = Envir.Time + AttackSpeed;
            // damage
            int damageBase = GetAttackPower(MinDC, MaxDC);
            int damageFinal = magic.GetDamage(damageBase);

            // objects = this, magic, damage, currentlocation, direction, attackRange
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damageFinal, CurrentLocation, Direction, 4);
            CurrentMap.ActionList.Add(action);

            action = new DelayedAction(DelayedType.Magic, Envir.Time + 100, magic, CurrentLocation, Direction);
            ActionList.Add(action);
        
        }

        private void XiangLongGunFa(UserMagic magic, out bool cast)
        {
            cast = true;

            // damage
            int damageBase = GetAttackPower(MinSC, MaxSC);
            int damageFinal = magic.GetDamage(damageBase);

            // objects = this, magic, damage, currentlocation, direction, attackRange
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damageFinal, CurrentLocation, Direction, 1);
            CurrentMap.ActionList.Add(action);

            // telpo location
            Point location = Functions.PointMove(CurrentLocation, Direction, 2);

            if (!CurrentMap.ValidPoint(location)) return;

            var cellObjects = CurrentMap.GetCellObjects(location);

            bool blocked = false;
            if (cellObjects != null)
            {
                for (int c = 0; c < cellObjects.Count; c++)
                {
                    MapObject ob = cellObjects[c];
                    if (!ob.Blocking) continue;
                    blocked = true;
                    if ((cellObjects == null) || blocked) break;
                }
            }

            // blocked telpo cancel
            if (blocked) return;

            Teleport(CurrentMap, location, false);
        }

        private void FurySpell(UserMagic magic, out bool cast)
        {
            cast = true;

            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic));
        }
        private void ImmortalSkin(UserMagic magic, out bool cast)
        {
            cast = true;

            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic));
        }
        private void ImmortalSkin1(UserMagic magic, out bool cast)
        {
            cast = true;

            ActionList.Add(new DelayedAction(DelayedType.Magic, Envir.Time + 500, magic));
        }
        private void CounterAttackCast(UserMagic magic, MapObject target)
        {
            if (target == null || magic == null) return;

            if (!CounterAttack) return;

            int damageBase = GetAttackPower(MinDC, MaxDC);
            if (Envir.Random.Next(0, 100) <= Accuracy)
                damageBase += damageBase;//crit should do something like double dmg, not double max dc dmg!
            int damageFinal = magic.GetDamage(damageBase);


            MirDirection dir = Functions.ReverseDirection(target.Direction);
            Direction = dir;

            if (Functions.InRange(CurrentLocation, target.CurrentLocation, 1) == false) return;
            if (Envir.Random.Next(10) > magic.Level + 6) return;
            Enqueue(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.CounterAttack, TargetID = target.ObjectID, Target = target.CurrentLocation, Cast = true, Level = GetMagic(Spell.CounterAttack).Level, SelfBroadcast = true });
            DelayedAction action = new DelayedAction(DelayedType.Damage, AttackTime, target, damageFinal, DefenceType.AC, true);
            ActionList.Add(action);
            LevelMagic(magic);
            CounterAttack = false;
        }

        private void CounterAttackCast1(UserMagic magic, MapObject target)
        {
            if (target == null || magic == null) return;

            if (!CounterAttack) return;

            int damageBase = GetAttackPower(MinDC, MaxDC);
            if (Envir.Random.Next(0, 100) <= Accuracy)
                damageBase += damageBase;//crit should do something like double dmg, not double max dc dmg!
            int damageFinal = magic.GetDamage(damageBase);

            MirDirection dir = Functions.ReverseDirection(target.Direction);
            Direction = dir;

            if (Functions.InRange(CurrentLocation, target.CurrentLocation, 1) == false) return;
            if (Envir.Random.Next(10) > magic.Level + 6) return;
            Enqueue(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.CounterAttack1, TargetID = target.ObjectID, Target = target.CurrentLocation, Cast = true, Level = GetMagic(Spell.CounterAttack1).Level, SelfBroadcast = true });
            DelayedAction action = new DelayedAction(DelayedType.Damage, AttackTime, target, damageFinal, DefenceType.AC, true);
            ActionList.Add(action);
            LevelMagic(magic);
            CounterAttack = false;
        }
        #endregion

        #region Assassin Skills

        private void HeavenlySword(UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinDC, MaxDC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation, Direction);
            CurrentMap.ActionList.Add(action);
        }
        private void SwiftFeet(UserMagic magic, out bool cast)
        {
            cast = true;

            AddBuff(new Buff { Type = BuffType.SwiftFeet, Caster = this, ExpireTime = Envir.Time + 25000 + magic.Level * 5000, Values = new int[] { 1 }, Visible = true });
            LevelMagic(magic);
        }
        private void MoonLight(UserMagic magic)
        {
            for (int i = 0; i < Buffs.Count; i++)
                if (Buffs[i].Type == BuffType.MoonLight) return;

            AddBuff(new Buff { Type = BuffType.MoonLight, Caster = this, ExpireTime = Envir.Time + 15000 + magic.Level * 5000, Visible = true });
            LevelMagic(magic);
        }
        private void Trap(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;

            if (target == null || !target.IsAttackTarget(this) || !(target is MonsterObject)) return;
            if (target.Level >= Level + 2) return;

            Point location = target.CurrentLocation;

            LevelMagic(magic);
            uint duration = 60000;
            int value = (int)duration;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location);
            CurrentMap.ActionList.Add(action);
            cast = true;
        }
        private bool PoisonSword(UserMagic magic)
        {
            UserItem item = GetPoison(1);
            if (item == null) return false;

            Point hitPoint;
            List<MapObject>cellObjects =  null;
            MirDirection dir = Functions.PreviousDir(Direction);
            int power = magic.GetDamage(GetAttackPower(MinDC, MaxDC));

            for (int i = 0; i < 5; i++)
            {
                hitPoint = Functions.PointMove(CurrentLocation, dir, 1);
                dir = Functions.NextDir(dir);

                if (!CurrentMap.ValidPoint(hitPoint)) continue;
                cellObjects = CurrentMap.GetCellObjects(hitPoint);

                if (cellObjects == null) continue;

                for (int o = 0; o < cellObjects.Count; o++)
                {
                    MapObject target = cellObjects[o];
                    if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) continue;
                    if (target == null || !target.IsAttackTarget(this) || target.Node == null) continue;

                    target.ApplyPoison(new Poison
                    {
                        Duration = 3 + power / 10 + magic.Level * 3,
                        Owner = this,
                        PType = PoisonType.Green,
                        TickSpeed = 1000,
                        Value = power / 10 + magic.Level + 1 + Envir.Random.Next(PoisonAttack)
                    }, this);

                    target.OperateTime = 0;
                    break;
                }
            }

            LevelMagic(magic);
            ConsumeItem(item, 1);
            return true;
        }

        private void LuoHanZhen(MapObject target, UserMagic magic)
        {
            MonsterObject monster;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.MonkCloneName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                monster.Die();
            }

            RegenMonsterByName(Settings.MonkCloneName, CurrentLocation.X - 1, CurrentLocation.Y - 1);
            RegenMonsterByName(Settings.MonkCloneName, CurrentLocation.X - 1, CurrentLocation.Y + 1);
            RegenMonsterByName(Settings.MonkCloneName, CurrentLocation.X + 1, CurrentLocation.Y - 1);
            RegenMonsterByName(Settings.MonkCloneName, CurrentLocation.X + 1, CurrentLocation.Y + 1);

            LevelMagic(magic);
        }

        public MonsterObject RegenMonsterByName(string name, int x, int y)
        {
            MonsterObject mob = MonsterObject.GetMonster(Envir.GetMonsterInfo(name));
            if (mob == null)
                return null;

            mob.Spawn(CurrentMap, new Point(x, y));
            mob.Owner = this;
            mob.Master = this;
            mob.ActionTime = Envir.Time + 500;
            mob.RefreshNameColour(false);
            Pets.Add(mob);

            return mob;
        }

        private void DarkBody(MapObject target, UserMagic magic)
        {
            MonsterObject monster;
            for (int i = 0; i < Pets.Count; i++)
            {
                monster = Pets[i];
                if ((monster.Info.Name != Settings.AssassinCloneName) || monster.Dead) continue;
                if (monster.Node == null) continue;
                monster.Die();
                return;
            }

            MonsterInfo info = Envir.GetMonsterInfo(Settings.AssassinCloneName);
            if (info == null) return;

            if (target == null) return;

            LevelMagic(magic);

            monster = MonsterObject.GetMonster(info);
            monster.Master = this;
            monster.Direction = Direction;
            monster.ActionTime = Envir.Time + 500;
            monster.RefreshNameColour(false);
            monster.Target = target;
            Pets.Add(monster);

            monster.Spawn(CurrentMap, CurrentLocation);

            for (int i = 0; i < Buffs.Count; i++)
                if (Buffs[i].Type == BuffType.DarkBody) return;

            AddBuff(new Buff { Type = BuffType.DarkBody, Caster = this, ExpireTime = Envir.Time + (GetAttackPower(MinAC, MaxAC) + (magic.Level + 1) * 5) * 500, Visible = true });
            LevelMagic(magic);
        }
        private void CrescentSlash(UserMagic magic)
        {
            int damageBase = GetAttackPower(MinDC, MaxDC);
            if (Envir.Random.Next(0, 100) <= Accuracy)
                damageBase += damageBase; //crit should do something like double dmg, not double max dc dmg!
            int damageFinal = magic.GetDamage(damageBase);

            MirDirection backDir = Functions.ReverseDirection(Direction);
            MirDirection preBackDir = Functions.PreviousDir(backDir);
            MirDirection nextBackDir = Functions.NextDir(backDir);

            for (int i = 0; i < 8; i++)
            {
                MirDirection dir = (MirDirection)i;
                Point hitPoint = Functions.PointMove(CurrentLocation, dir, 1);

                if (dir != backDir && dir != preBackDir && dir != nextBackDir)
                {
                    if (!CurrentMap.ValidPoint(hitPoint)) continue;

                    var cellObjects = CurrentMap.GetCellObjects(hitPoint);

                    if (cellObjects == null) continue;

                    for (int j = 0; j < cellObjects.Count; j++)
                    {
                        MapObject target = cellObjects[j];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (target.IsAttackTarget(this))
                                {
                                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + AttackSpeed, target, damageFinal, DefenceType.AC, true);
                                    ActionList.Add(action);
                                }
                                break;
                        }
                    }
                    LevelMagic(magic);
                }
            }
        }

        private void FlashDash(UserMagic magic, bool force)
        {
            bool success = false;
            ActionTime = Envir.Time;

            int travel = 0;
            bool blocked = false;
            int jumpDistance = (magic.Level <= 1) ? 0 : 1;//3 max
            Point location = CurrentLocation;
            for (int i = 0; i < jumpDistance; i++)
            {
                location = Functions.PointMove(location, Direction, 1);
                if (!CurrentMap.ValidPoint(location)) break;

                var cellObjects = CurrentMap.GetCellObjects(location);
                if (cellObjects != null)
                {
                    for (int c = 0; c < cellObjects.Count; c++)
                    {
                        MapObject ob = cellObjects[c];
                        if (!ob.Blocking) continue;
                        blocked = true;
                        if ((cellObjects == null) || blocked) break;
                    }
                }
                if (blocked) break;
                travel++;
            }

            jumpDistance = travel;

            if (jumpDistance > 0)
            {
                location = Functions.PointMove(CurrentLocation, Direction, jumpDistance);
                CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
                RemoveObjects(Direction, 1);
                CurrentLocation = location;
                CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
                AddObjects(Direction, 1);
                Enqueue(new S.UserDashAttack { Direction = Direction, Location = location });
                Broadcast(new S.ObjectDashAttack { ObjectID = ObjectID, Direction = Direction, Location = location, Distance = jumpDistance });
            }
            else
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            }

            if (travel == 0) location = CurrentLocation;

            int attackDelay = (AttackSpeed - 120) <= 300 ? 300 : (AttackSpeed - 120);
            AttackTime = Envir.Time + attackDelay;
            SpellTime = Envir.Time + 300;

            location = Functions.PointMove(location, Direction, 1);
            if (CurrentMap.ValidPoint(location))
            {
                var cellObjects = CurrentMap.GetCellObjects(location);
                if (cellObjects != null)
                {
                    for (int c = 0; c < cellObjects.Count; c++)
                    {
                        MapObject ob = cellObjects[c];
                        switch (ob.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (ob.IsAttackTarget(this))
                                {
                                    DelayedAction action = new DelayedAction(DelayedType.Damage, AttackTime, ob, magic.GetDamage(GetAttackPower(MinDC, MaxDC)), DefenceType.AC, true);
                                    ActionList.Add(action);
                                    success = true;

                                    if (force)
                                    {
                                        ob.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = ob.Race == ObjectType.Player ? 2 : magic.Level, TickSpeed = 1000 }, this);
                                        ob.Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.TwinDrakeBlade });
                                    }
                                    else
                                    {
                                        if ((((ob.Race != ObjectType.Player) || Settings.PvpCanResistPoison) && (Envir.Random.Next(Settings.PoisonAttackWeight) >= ob.PoisonResist)) && (Envir.Random.Next(15) <= magic.Level + 1))
                                        {
                                            ob.ApplyPoison(new Poison { PType = PoisonType.Stun, Duration = ob.Race == ObjectType.Player ? 2 : magic.Level, TickSpeed = 1000 }, this);
                                            ob.Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.TwinDrakeBlade });

                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            if (success) //technicaly this makes flashdash lvl when it casts rather then when it hits (it wont lvl if it's not hitting!)
                LevelMagic(magic);

            magic.CastTime = Envir.Time;
            Enqueue(new S.MagicCast { Spell = magic.Spell });
        }
        #endregion

        #region Archer Skills

        private int ApplyArcherState(int damage)
        {
            UserMagic magic = GetMagic(Spell.MentalState);
            if (magic != null)
                LevelMagic(magic);
            int dmgpenalty = 100;
            switch (Info.MentalState)
            {
                case 1: //trickshot
                    dmgpenalty = 55 + (Info.MentalStateLvl * 5);
                    break;
                case 2: //group attack
                    dmgpenalty = 80;
                    break;
            }
            return (damage * dmgpenalty) / 100;
        }

        private bool StraightShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return false;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return false;
            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            damage = ApplyArcherState(damage);
            int delay = distance * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);

            ActionList.Add(action);

            return true;
        }
        private bool DoubleShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return false;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return false;
            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinDC, MaxDC));
            damage = ApplyDistanceBonus(damage, distance); 
            damage = ApplyArcherState(damage);
            int delay = distance * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);

            ActionList.Add(action);

            action = new DelayedAction(DelayedType.Magic, Envir.Time + delay + 50, magic, damage, target);

            ActionList.Add(action);

            return true;
        }

        private int ApplyDistanceBonus(int damage, int distance)
        {
            return (int)((1.0 + Math.Min(0.3, Math.Max(0, (distance - 1) * 0.5))) * damage);
        }

        private void BackStep(UserMagic magic)
        {
            ActionTime = Envir.Time;
            if (!CanWalk) return;

            int travel = 0;
            bool blocked = false;
            int jumpDistance = 2 + magic.Level;//3 max
            MirDirection jumpDir = Functions.ReverseDirection(Direction);
            Point location = CurrentLocation;
            for (int i = 0; i < jumpDistance; i++)
            {
                location = Functions.PointMove(location, jumpDir, 1);
                if (!CurrentMap.ValidPoint(location)) break;

                var cellObjects = CurrentMap.GetCellObjects(location);
                if (cellObjects != null)
                    for (int c = 0; c < cellObjects.Count; c++)
                    {
                        MapObject ob = cellObjects[c];
                        if (!ob.Blocking) continue;
                        blocked = true;
                        if ((cellObjects == null) || blocked) break;
                    }
                if (blocked) break;
                travel++;
            }

            jumpDistance = travel;
            if (jumpDistance > 0)
            {
                for (int i = 0; i < jumpDistance; i++)
                {
                    location = Functions.PointMove(CurrentLocation, jumpDir, 1);
                    CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);
                    RemoveObjects(jumpDir, 1);
                    CurrentLocation = location;
                    CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
                    AddObjects(jumpDir, 1);
                }
                Enqueue(new S.UserBackStep { Direction = Direction, Location = location });
                Broadcast(new S.ObjectBackStep { ObjectID = ObjectID, Direction = Direction, Location = location, Distance = jumpDistance });
                LevelMagic(magic);
            }
            else
            {
                Broadcast(new S.ObjectBackStep { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Distance = jumpDistance });
                ReceiveChat(Envir.Tr("Not enough jumping power."), ChatType.System);
            }

            if (++magic.Times == 3)
            {
                magic.Times = 0;
                magic.CastTime = Envir.Time;
                Enqueue(new S.MagicCast { Spell = magic.Spell });
            }

            CellTime = Envir.Time + 500;
            SpellTime = Envir.Time + 1000;
        }
        private bool DelayedExplosion(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int power = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, power, target);
            ActionList.Add(action);
            return true;
        }

        private bool DelayedExplosion2(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int power = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 2600; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, power, target);
            ActionList.Add(action);
            return true;
        }
        private void ExplosiveTrap(UserMagic magic, Point location)
        {
            bool hasTrapObject = false;
            for (int i = 0; i < 9; ++i)
            {
                if (ArcherTrapObjectsArray[i] != null)
                    hasTrapObject = true;
            }

            if (hasTrapObject)
            {
                ExplosiveTrapDetonated();

                Enqueue(new S.Magic { Spell = Spell.ExplosiveTrap, Target = location, Cast = true, Level = magic.Level});
                Broadcast(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = Spell.ExplosiveTrap,
                    Target = location, Cast = true, Level = magic.Level, Param = true });

                magic.CastTime = Envir.Time + 500;
                return;
            }

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, location);
            CurrentMap.ActionList.Add(action);

            Enqueue(new S.Magic { Spell = Spell.ExplosiveTrap, Target = location, Cast = true, Level = magic.Level });
            Broadcast(new S.ObjectMagic
            {
                ObjectID = ObjectID,
                Direction = Direction,
                Location = CurrentLocation,
                Spell = Spell.ExplosiveTrap,
                Target = location,
                Cast = true,
                Level = magic.Level,
            });

            SpellTime = Envir.Time + 500;
        }
        public void ExplosiveTrapDetonated()
        {
            SpellObject ArcherTrap;
            for (int j = 0; j < 9; j++)
            {
                if (ArcherTrapObjectsArray[j] == null)
                    continue;

                ArcherTrap = (SpellObject)ArcherTrapObjectsArray[j];
                ArcherTrap.ExplosiveTrap(this);
            }
        }
        public void DoKnockback(MapObject target, UserMagic magic)//ElementalShot - knockback
        {
            var cellObjects = CurrentMap.GetCellObjects(target.CurrentLocation);
            if (cellObjects == null) return;

            if (target.CurrentLocation.Y < 0 || target.CurrentLocation.Y >= CurrentMap.Height || target.CurrentLocation.X < 0 || target.CurrentLocation.X >= CurrentMap.Height) return;

            if (target.Race != ObjectType.Monster && target.Race != ObjectType.Player) return;
            if (!target.IsAttackTarget(this) || target.Level >= Level) return;

            if (Envir.Random.Next(20) >= 6 + magic.Level * 3 + ElementsLevel + Level - target.Level) return;
            int distance = 1 + Math.Max(0, magic.Level - 1) + Envir.Random.Next(2);
            MirDirection dir = Functions.DirectionFromPoint(CurrentLocation, target.CurrentLocation);

            target.Pushed(this, dir, distance);
        }
        public void BindingShot(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;

            if (target == null || !target.IsAttackTarget(this) || !(target is MonsterObject)) return;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return;
            if (target.Level > Level + 2) return;
            if (((MonsterObject)target).ShockTime >= Envir.Time) return;//Already shocked


            uint duration = (uint)((magic.Level * 5 + 10) * 1000);
            int value = (int)duration;
            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, value, target);
            ActionList.Add(action);

            cast = true;
        }

        public void PoisonArrowShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return;
            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinDC, MaxDC));

            damage = ApplyDistanceBonus(damage, distance);
            damage = ApplyArcherState(damage);

            int delay = distance * 50 + 500; //50 MS per Step
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);
            ActionList.Add(action);
        }

        public void SpecialArrowShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return;
            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);

            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            damage = ApplyArcherState(damage);

            int delay = distance * 50 + 500; //50 MS per Step

            SpellTime = Envir.Time + AttackSpeed;
            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);
            ActionList.Add(action);
        }
        public void NapalmShot(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return;

            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            damage = ApplyArcherState(damage);

            int delay = distance * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, damage, target.CurrentLocation);
            CurrentMap.ActionList.Add(action);
        }
        public void NapalmShot2(MapObject target, UserMagic magic)
        {
            if (target == null || !target.IsAttackTarget(this)) return;
            if ((Info.MentalState != 1) && !CanFly(target.CurrentLocation)) return;

            int distance = Functions.MaxDistance(CurrentLocation, target.CurrentLocation);
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));
            damage = ApplyArcherState(damage);

            int delay = distance * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, this, magic, damage, target.CurrentLocation);
            CurrentMap.ActionList.Add(action);
        }
        public void ArcherSummon(UserMagic magic, MapObject target, Point location)
        {
            if (target != null && target.IsAttackTarget(this))
                location = target.CurrentLocation;
            if (!CanFly(location)) return;

            uint duration = (uint)((magic.Level * 5 + 10) * 1000);
            int value = (int)duration;
            int delay = Functions.MaxDistance(CurrentLocation, location) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, value, location, target);
            ActionList.Add(action);
        }

        public void OneWithNature(MapObject target, UserMagic magic)
        {
            int damage = magic.GetDamage(GetAttackPower(MinMC, MaxMC));

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, damage, CurrentLocation);
            CurrentMap.ActionList.Add(action);
        }
        #endregion

        #region Custom

        private void Portal(UserMagic magic, Point location, out bool cast)
        {
            cast = false;

            if (!CurrentMap.ValidPoint(location)) return;

            if (PortalObjectsArray[1] != null && PortalObjectsArray[1].Node != null)
            {
                PortalObjectsArray[0].ExpireTime = 0;
                PortalObjectsArray[0].Process();
            }

            if (!CanFly(location)) return;

            int duration = 30 + (magic.Level * 30);
            int value = duration;
            int passthroughCount = (magic.Level * 2) - 1;

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, value, location, passthroughCount);
            CurrentMap.ActionList.Add(action);
            cast = true;
        }

        #endregion

        private void CheckSneakRadius()
        {
            if (!Sneaking) return;

            for (int y = CurrentLocation.Y - 3; y <= CurrentLocation.Y + 3; y++)
            {
                if (y < 0) continue;
                if (y >= CurrentMap.Height) break;

                for (int x = CurrentLocation.X - 3; x <= CurrentLocation.X + 3; x++)
                {
                    if (x < 0) continue;
                    if (x >= CurrentMap.Width) break;

                    var cellObjects = CurrentMap.GetCellObjects(x, y);
                    if (cellObjects == null) continue;

                    for (int i = 0; cellObjects != null && i < cellObjects.Count; i++)
                    {
                        MapObject ob = cellObjects[i];
                        if ((ob.Race != ObjectType.Player) || ob == this) continue;

                        SneakingActive = false;
                        return;
                    }
                }
            }

            SneakingActive = true;
        }

        private void CompleteMagic(IList<object> data)
        {
            UserMagic magic = (UserMagic)data[0];
            int value;
            MapObject target;
            Point location;
            MonsterObject monster;
            switch (magic.Spell)
            {
                #region FireBall, GreatFireBall, ThunderBolt, SoulFireBall, FlameDisruptor

                case Spell.FireBall:            // 火球
                case Spell.GreatFireBall:       // 大火球
                case Spell.ThunderBolt:         // 雷电术
                case Spell.SoulFireBall:        // 灵魂火符
                case Spell.FlameDisruptor:      // 灭天火
                case Spell.StraightShot:        // 落日箭法
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Attacked(this, value, DefenceType.MAC, false) > 0) LevelMagic(magic);
                    break;

                case Spell.GreateFireBallSecret:
                    value = (int)data[1];
                    target = (MapObject)data[2];
                    int damage = magic.GetDamage(value);
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Attacked(this, damage, DefenceType.MAC, false) > 0) LevelMagic(magic);

                    if (Envir.Random.Next(Math.Min(6, Math.Max(9 - Luck, 1))) == 0)
                        target.ApplyPoison(new Poison { Owner = this, Duration = 5 + magic.Level * 5, PType = PoisonType.Morea, Value = value / 3, TickSpeed = 1000 }, this);
                    break;

                case Spell.DoubleShot:          // 连珠箭法
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Attacked(this, value, DefenceType.ACAgility, false) > 0) LevelMagic(magic);
                    break;

                #endregion

                case Spell.CatTongue:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Attacked(this, value, DefenceType.AC, false) > 0) LevelMagic(magic);
                    if (Envir.Random.Next(2) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = target is PlayerObject ? 3 : 3, PType = PoisonType.Stun, Value = value, TickSpeed = 1000 }, this);
                    }
                    else if (Envir.Random.Next(2) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = target is PlayerObject ? 3 : 3, PType = PoisonType.Frozen, Value = value, TickSpeed = 1000 }, this);
                    }

                    break;

                case Spell.ShiBuYiSha:
                    value = (int)data[1];
                    location = (Point)data[2];

                    if (CurrentMap.Info.NoTeleport)
                    {
                        ReceiveChat((Envir.Tr("You cannot teleport on this map")), ChatType.System);
                        return;
                    }
                    if (!CurrentMap.ValidPoint(location) || !Teleport(CurrentMap, location, false)) return;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ShiBuYiSha }, CurrentLocation);

                    for (int y = location.Y - 2; y <= location.Y + 2; y++)
                    {
                        if (y < 0) continue;
                        if (y >= CurrentMap.Height) break;

                        for (int x = location.X - 2; x <= location.X + 2; x++)
                        {
                            if (x < 0) continue;
                            if (x >= CurrentMap.Width) break;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                target = cellObjects[i];
                                switch (target.Race)
                                {
                                    case ObjectType.Monster:
                                    case ObjectType.Player:
                                        //Only targets
                                        if (!target.IsAttackTarget(this)) break;

                                        target.Attacked(this, value, DefenceType.MAC, false);
                                        break;
                                }
                            }

                        }
                    }

                    LevelMagic(magic);
                    break;

                #region FrostCrunch
                case Spell.FrostCrunch:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Attacked(this, value, DefenceType.MAC, false) > 0)
                    {
                        //todo:法师技能修改:寒冰掌减速和冰冻概率
                        //if (Level + (target.Race == ObjectType.Player ? 2 : 10) >= target.Level && Envir.Random.Next(target.Race == ObjectType.Player ? 100 : 20) <= magic.Level)
                        if (Envir.Random.Next(20) <= (magic.Level * 3 + 1))//修改为技能等级0/1/2/3对应生效概率为5/20/35/50%
                        {
                            target.ApplyPoison(new Poison
                            {
                                Owner = this,
                                //Duration = target.Race == ObjectType.Player ? 4 : 5 + Envir.Random.Next(5),
                                Duration = target.Race == ObjectType.Player ? 1 : 3 + Envir.Random.Next(4),
                                PType = PoisonType.Slow,
                                TickSpeed = 1000,
                            }, this);
                            target.OperateTime = 0;
                        }

                        //if (Level + (target.Race == ObjectType.Player ? 2 : 10) >= target.Level && Envir.Random.Next(target.Race == ObjectType.Player ? 100 : 40) <= magic.Level)
                        if (Level + (target.Race == ObjectType.Player ? 2 : 100) >= target.Level && Envir.Random.Next(target.Race == ObjectType.Player ? 100 : 40) <= magic.Level)
                        {
                            target.ApplyPoison(new Poison
                            {
                                Owner = this,
                                Duration = target.Race == ObjectType.Player ? 2 : 5 + Envir.Random.Next(Freezing),
                                PType = PoisonType.Frozen,
                                TickSpeed = 1000,
                            }, this);
                            target.OperateTime = 0;
                        }

                        LevelMagic(magic);
                    }
                    break;

                #endregion

                #region Vampirism

                case Spell.Vampirism:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    value = target.Attacked(this, value, DefenceType.MAC, false);
                    if (value == 0) return;
                    LevelMagic(magic);
                    if (VampAmount == 0) VampTime = Envir.Time + 1000;
                    VampAmount += (ushort)(value * (magic.Level + 1) * 0.25F);
                    break;

                #endregion

                #region Healing

                case Spell.Healing:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsFriendlyTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Health >= target.MaxHealth) return;
                    target.HealAmount = (ushort)Math.Min(ushort.MaxValue, target.HealAmount + value);
                    target.OperateTime = 0;
                    LevelMagic(magic);
                    break;

                #endregion

                #region ElectricShock

                case Spell.ElectricShock:
                    monster = (MonsterObject)data[1];
                    if (monster == null || !monster.IsAttackTarget(this) || monster.CurrentMap != CurrentMap || monster.Node == null) return;
                    ElectricShock(monster, magic);
                    break;

                #endregion

                #region Poisoning

                case Spell.Poisoning:
                    value = (int)data[1];
                    target = (MapObject)data[2];
                    PoisonType PType = (PoisonType)data[3];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

                    switch (PType)
                    {
                        case PoisonType.Green:
                            int duration = Math.Max(10, (value * 2) + ((magic.Level + 1) * 7) - target.GetDefencePower(target.MinMAC, target.MaxMAC));
                            target.ApplyPoison(new Poison
                            {
                                Duration = duration,
                                Owner = this,
                                PType = PoisonType.Green,
                                TickSpeed = 2000,
                                Value = value / 15 + magic.Level + 1 + Envir.Random.Next(PoisonAttack)
                            }, this);
                            break;
                        case PoisonType.Red:
                            target.ApplyPoison(new Poison
                            {
                                Duration = (value * 2) + (magic.Level + 1) * 7,
                                Owner = this,
                                PType = PoisonType.Red,
                                TickSpeed = 2000,
                            }, this);
                            break;
                    }
                    target.OperateTime = 0;

                    LevelMagic(magic);
                    break;

                #endregion

                #region StormEscape
                case Spell.StormEscape:
                    location = (Point)data[1];
                    if (CurrentMap.Info.NoTeleport)
                    {
                        ReceiveChat((Envir.Tr("You cannot teleport on this map")), ChatType.System);
                        return;
                    }
                    if (!CurrentMap.ValidPoint(location) || Envir.Random.Next(4) >= magic.Level + 1 || !Teleport(CurrentMap, location, false)) return;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.StormEscape }, CurrentLocation);
                    AddBuff(new Buff { Type = BuffType.TemporalFlux, Caster = this, ExpireTime = Envir.Time + 30000 });
                    LevelMagic(magic);
                    break;

                case Spell.StormEscape1:
                    location = (Point)data[1];
                    if (CurrentMap.Info.NoTeleport)
                    {
                        ReceiveChat((Envir.Tr("You cannot teleport on this map")), ChatType.System);
                        return;
                    }
                    if (!CurrentMap.ValidPoint(location) || Envir.Random.Next(2) >= magic.Level + 1 || !Teleport(CurrentMap, location, false)) return;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.StormEscape }, CurrentLocation);
                    AddBuff(new Buff { Type = BuffType.TemporalFlux, Caster = this, ExpireTime = Envir.Time + 30000 });
                    AddBuff(new Buff { Type = BuffType.StormEscape, Caster = this, ExpireTime = Envir.Time + 20000 });
                    LevelMagic(magic);
                    break;
                #endregion


                #region Teleport
                case Spell.Teleport:
                    Map temp = Envir.GetMap(BindMapIndex, GetRegionID());
                    int mapSizeX = temp.Width / (magic.Level + 1);
                    int mapSizeY = temp.Height / (magic.Level + 1);

                    if (CurrentMap.Info.NoTeleport)
                    {
                        ReceiveChat((Envir.Tr("You cannot teleport on this map")), ChatType.System);
                        return;
                    }

                    for (int i = 0; i < 200; i++)
                    {
                        location = new Point(BindLocation.X + Envir.Random.Next(-mapSizeX, mapSizeX),
                                             BindLocation.Y + Envir.Random.Next(-mapSizeY, mapSizeY));

                        if (Teleport(temp, location)) break;
                    }

                    AddBuff(new Buff { Type = BuffType.TemporalFlux, Caster = this, ExpireTime = Envir.Time + 30000 });
                    LevelMagic(magic);

                    break;
                #endregion

                #region Blink

                case Spell.Blink:
                    {
                        location = (Point)data[1];
                        if (CurrentMap.Info.NoTeleport)
                        {
                            ReceiveChat((Envir.Tr("You cannot teleport on this map")), ChatType.System);
                            return;
                        }
                        if (Functions.InRange(CurrentLocation, location, magic.Info.Range) == false) return;
                        if (!CurrentMap.ValidPoint(location) || Envir.Random.Next(4) >= magic.Level + 1 || !Teleport(CurrentMap, location, false)) return;
                        CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Teleport }, CurrentLocation);
                        LevelMagic(magic);
                        AddBuff(new Buff { Type = BuffType.TemporalFlux, Caster = this, ExpireTime = Envir.Time + 30000 });
                    }
                    break;

                #endregion

                #region Hiding

                case Spell.Hiding:
                    for (int i = 0; i < Buffs.Count; i++)
                        if (Buffs[i].Type == BuffType.Hiding) return;

                    value = (int)data[1];
                    AddBuff(new Buff { Type = BuffType.Hiding, Caster = this, ExpireTime = Envir.Time + value * 1000 });
                    LevelMagic(magic);
                    break;

                #endregion

                #region Haste

                case Spell.Haste:
                    AddBuff(new Buff { Type = BuffType.Haste, Caster = this, ExpireTime = Envir.Time + (magic.Level + 1) * 30000, Values = new int[] { (magic.Level + 1) * 2 } });
                    LevelMagic(magic);
                    break;

                #endregion

                #region Fury

                case Spell.Fury:
                    AddBuff(new Buff { Type = BuffType.Fury, Caster = this, ExpireTime = Envir.Time + 60000 + magic.Level * 10000, Values = new int[] { 4 }, Visible = true });
                    LevelMagic(magic);
                    break;

                #endregion

                #region ImmortalSkin

                case Spell.ImmortalSkin:
                    {
                        int reduce = 50 - magic.Level * 10;
                        int duration = 50000;
                        AddBuff(new Buff { Type = BuffType.ImmortalSkin, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { 50, reduce, 0 }, Visible = true });

                        LevelMagic(magic);
                    }
                    break;
                #endregion

                case Spell.ImmortalSkin1:
                    {
                        int reduce = 50 - magic.Level * 10;
                        int duration = 50000;
                        int regen = 400 + magic.Level * 100 + Math.Max(0, Luck * 100);
                        AddBuff(new Buff { Type = BuffType.ImmortalSkin, Caster = this, ExpireTime = Envir.Time + duration, Values = new int[] { 60, reduce, regen }, Visible = true });

                        LevelMagic(magic);
                    }
                    break;

                #region LightBody

                case Spell.LightBody:
                    AddBuff(new Buff { Type = BuffType.LightBody, Caster = this, ExpireTime = Envir.Time + (magic.Level + 1) * 30000, Values = new int[] { magic.Level + 1 } });
                    LevelMagic(magic);
                    break;

                #endregion

                #region MagicShield

                case Spell.MagicShield:

                    if (MagicShield) return;
                    MagicShield = true;
                    MagicShieldLv = magic.Level;
                    MagicShieldTime = Envir.Time + (int)data[1] * 1000;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MagicShieldUp }, CurrentLocation);
                    AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
                    LevelMagic(magic);
                    break;

                #endregion

                #region TurnUndead

                case Spell.TurnUndead:
                    monster = (MonsterObject)data[1];
                    if (monster == null || !monster.IsAttackTarget(this) || monster.CurrentMap != CurrentMap || monster.Node == null) return;
                    monster.LastHitter = this;
                    monster.LastHitTime = Envir.Time + 5000;
                    monster.EXPOwner = this;
                    monster.EXPOwnerTime = Envir.Time + 5000;
                    monster.Die();
                    LevelMagic(magic);
                    break;

                #endregion

                #region MagicBooster

                case Spell.MagicBooster:
                    value = (int)data[1];

                    AddBuff(new Buff { Type = BuffType.MagicBooster, Caster = this, ExpireTime = Envir.Time + 60000, Values = new int[] { value, 6 + magic.Level }, Visible = true });
                    LevelMagic(magic);
                    break;

                #endregion

                case Spell.Bisul:
                    {
                        int duration = (10 + 5 * magic.Level + MaxMC / 10) * 1000;
                        AddBuff(new Buff { Type = BuffType.Bisul, Caster = this, ExpireTime = Envir.Time + duration, Visible = true });
                        LevelMagic(magic);
                    }
                    break;

                #region Purification

                case Spell.Purification:
                    target = (MapObject)data[1];

                    if (target == null || !target.IsFriendlyTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (Envir.Random.Next(4) > magic.Level || target.PoisonList.Count == 0) return;

                    target.ExplosionInflictedTime = 0;
                    target.ExplosionInflictedStage = 0;

                    for (int i = 0; i < target.Buffs.Count; i++)
                    {
                        if (target.Buffs[i].Type == BuffType.Curse)
                        {
                            target.Buffs.RemoveAt(i);
                            break;
                        }
                    }

                    target.PoisonList.Clear();
                    target.OperateTime = 0;

                    if (target.ObjectID == ObjectID)
                        Enqueue(new S.RemoveDelayedExplosion { ObjectID = target.ObjectID });
                    target.Broadcast(new S.RemoveDelayedExplosion { ObjectID = target.ObjectID });

                    LevelMagic(magic);
                    break;

                #endregion

                #region Revelation

                case Spell.Revelation:
                    value = (int)data[1];
                    target = (MapObject)data[2];
                    if (target == null || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (target.Race != ObjectType.Player && target.Race != ObjectType.Monster) return;
                    if (Envir.Random.Next(4) > magic.Level || Envir.Time < target.RevTime) return;

                    target.RevTime = Envir.Time + value * 1000;
                    target.OperateTime = 0;
                    target.BroadcastHealthChange();

                    LevelMagic(magic);
                    break;

                #endregion

                #region Reincarnation

                case Spell.Reincarnation:

                    if (ReincarnationReady)
                    {
                        ReincarnationTarget.Enqueue(new S.RequestReincarnation { });
                        LevelMagic(magic);
                        ReincarnationReady = false;
                    }
                    break;

                #endregion

                #region Entrapment

                case Spell.Entrapment:
                    {
                        value = (int)data[1];
                        target = (MapObject)data[2];
                        if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null || !(target.Race == ObjectType.Monster) ||
                            Functions.MaxDistance(CurrentLocation, target.CurrentLocation) > 7 || target.Level >= Level + 5 + Envir.Random.Next(8)) return;
                        MirDirection pulldirection = (MirDirection)((byte)(Direction - 4) % 8);
                        int pulldistance = 0;
                        if ((byte)pulldirection % 2 > 0)
                            pulldistance = Math.Max(0, Math.Min(Math.Abs(CurrentLocation.X - target.CurrentLocation.X), Math.Abs(CurrentLocation.Y - target.CurrentLocation.Y)));
                        else
                            pulldistance = pulldirection == MirDirection.Up || pulldirection == MirDirection.Down ? Math.Abs(CurrentLocation.Y - target.CurrentLocation.Y) - 2 : Math.Abs(CurrentLocation.X - target.CurrentLocation.X) - 2;

                        int levelgap = target.Race == ObjectType.Player ? Level - target.Level + 4 : Level - target.Level + 9;
                        if (Envir.Random.Next(30) >= ((magic.Level + 1) * 3) + levelgap) return;

                        int duration = target.Race == ObjectType.Player ? (int)Math.Round((magic.Level + 1) * 1.6) : (int)Math.Round((magic.Level + 1) * 0.8);
                        if (duration > 0) target.ApplyPoison(new Poison { PType = PoisonType.Paralysis, Duration = duration, TickSpeed = 1000 }, this);
                        CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.Entrapment }, target.CurrentLocation);
                        if (target.Pushed(this, pulldirection, pulldistance) > 0) LevelMagic(magic);
                    }
                    break;

                #endregion

                case Spell.EntrapSwordSecret:
                    {
                        value = (int)data[1];
                        target = (MapObject)data[2];
                        if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null || !(target.Race == ObjectType.Monster || target.Race == ObjectType.Player) ||
                            Functions.MaxDistance(CurrentLocation, target.CurrentLocation) > 7 || target.Level >= Level + 5 + Envir.Random.Next(8)) return;
                        MirDirection pulldirection = (MirDirection)((byte)(Direction - 4) % 8);
                        int pulldistance = 0;
                        if ((byte)pulldirection % 2 > 0)
                            pulldistance = Math.Max(0, Math.Min(Math.Abs(CurrentLocation.X - target.CurrentLocation.X), Math.Abs(CurrentLocation.Y - target.CurrentLocation.Y)));
                        else
                            pulldistance = pulldirection == MirDirection.Up || pulldirection == MirDirection.Down ? Math.Abs(CurrentLocation.Y - target.CurrentLocation.Y) - 2 : Math.Abs(CurrentLocation.X - target.CurrentLocation.X) - 2;

                        int levelgap = target.Race == ObjectType.Player ? Level - target.Level + 4 : Level - target.Level + 9;

                        int duration = target.Race == ObjectType.Player ? (int)Math.Round((magic.Level + 1) * 1.6) : (int)Math.Round((magic.Level + 1) * 0.8);
                        if (duration > 0) target.ApplyPoison(new Poison { PType = PoisonType.Paralysis, Duration = duration, TickSpeed = 1000 }, this);
                        CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.Entrapment }, target.CurrentLocation);
                        if (target.Pushed(this, pulldirection, pulldistance) > 0) LevelMagic(magic);
                    }
                    break;

                #region Hallucination
                // 有问题
                case Spell.Hallucination:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null ||
                        Functions.MaxDistance(CurrentLocation, target.CurrentLocation) > 7 || Envir.Random.Next(Level + 20 + magic.Level * 5) <= target.Level + 10) return;
                    UserItem item = GetAmulet(1);
                    if (item == null) return;

                    ((MonsterObject)target).HallucinationTime = Envir.Time + (Envir.Random.Next(20) + 10) * 1000;
                    target.Target = null;

                    ConsumeItem(item, 1);

                    LevelMagic(magic);
                    break;

                #endregion

                #region PetEnhancer

                case Spell.PetEnhancer:
                    value = (int)data[1];
                    MonsterObject pet = (MonsterObject)data[2];

                    int dcInc = (magic.Level + 2) * 2 + MaxSC / 5;
                    int acInc, mcInc = 0;
                    acInc = 4 + pet.Level * 2 + MaxSC / 5;
                    if (GetRegionID() == 2)
                    {
                        acInc = 4 + pet.MaxPetLevel * 2 + MaxSC / 5;
                        mcInc = 4 + pet.MaxPetLevel * 2 + MaxSC / 10;
                    }

                    pet.AddBuff(new Buff { Type = BuffType.PetEnhancer, Caster = this, ExpireTime = Envir.Time + value * 1000, Values = new int[] { dcInc, acInc, mcInc }, Visible = true });
                    LevelMagic(magic);
                    break;

                #endregion

                #region ElementalBarrier, ElementalShot

                case Spell.ElementalBarrier:
                    //destroy orbs
                    ElementsLevel = 0;
                    LevelMagic(magic);
                    //
                    ElementalBarrier = true;
                    ElementalBarrierLv = (byte)((int)magic.Level);//compensate for lower mc then wizard
                    ElementalBarrierTime = Envir.Time + ((int)data[1]) * 1000;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierUp }, CurrentLocation);
                    break;

                case Spell.ElementalBarrier1:

                    ElementsLevel = 0;
                    LevelMagic(magic);

                    ElementalBarrier1 = true;
                    ElementalBarrierLv1 = (byte)(magic.Level);//compensate for lower mc then wizard
                    ElementalBarrierTime1 = Envir.Time + ((int)data[1]) * 1000;
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.ElementalBarrierUp1 }, CurrentLocation);
                    break;

                case Spell.ElementalShot:

                    //todo:技能修改:弓箭手技能修改:万斤闪

                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null)
                    {
                        //destroy orbs
                        ElementsLevel = 0;
                        //    ObtainElement(false);//update and send to client
                        return;
                    }
                    if (target.Attacked(this, value, DefenceType.AC, false) > 0)
                        LevelMagic(magic);
                    DoKnockback(target, magic);//ElementalShot - Knockback

                    //destroy orbs
                    ElementsLevel = 0;
                    //      ObtainElement(false);//update and send to client
                    break;

                #endregion

                #region DelayedExplosion

                case Spell.DelayedExplosion:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

                    if (target.Attacked(this, value, DefenceType.MAC, false) > 0) LevelMagic(magic);

                    target.ApplyPoison(new Poison
                    {
                        Duration = 6,
                        Owner = this,
                        PType = PoisonType.DelayedExplosion,
                        TickSpeed = 1500,
                        Value = value
                    }, this);

                    target.OperateTime = 0;
                    LevelMagic(magic);
                    break;

                case Spell.DelayedExplosion2:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

                    if (target.Attacked(this, value, DefenceType.MAC, false) > 0) LevelMagic(magic);

                    target.ApplyPoison(new Poison
                    {
                        Duration = 6,
                        Owner = this,
                        PType = PoisonType.DelayedExplosion,
                        TickSpeed = 1500,
                        Value = value,
                        Infect = true
                    }, this);

                    target.OperateTime = 0;
                    LevelMagic(magic);
                    break;
                #endregion

                #region BindingShot

                case Spell.BindingShot:
                    value = (int)data[1];
                    target = (MapObject)data[2];

                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    if (((MonsterObject)target).ShockTime >= Envir.Time) return;//Already shocked

                    Point place = target.CurrentLocation;
                    MonsterObject centerTarget = null;

                    for (int y = place.Y - 1; y <= place.Y + 1; y++)
                    {
                        if (y < 0) continue;
                        if (y >= CurrentMap.Height) break;

                        for (int x = place.X - 1; x <= place.X + 1; x++)
                        {
                            if (x < 0) continue;
                            if (x >= CurrentMap.Width) break;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject targetob = cellObjects[i];

                                if (y == place.Y && x == place.X && targetob.Race == ObjectType.Monster)
                                {
                                    centerTarget = (MonsterObject)targetob;
                                }

                                switch (targetob.Race)
                                {
                                    case ObjectType.Monster:
                                        if (targetob == null || !targetob.IsAttackTarget(this) || targetob.Node == null || targetob.Level > this.Level + 2) continue;

                                        MonsterObject mobTarget = (MonsterObject)targetob;

                                        if (centerTarget == null) centerTarget = mobTarget;

                                        mobTarget.ShockTime = Envir.Time + value;
                                        mobTarget.Target = null;
                                        break;
                                }
                            }
                        }
                    }

                    if (centerTarget == null) return;

                    //only the centertarget holds the effect
                    centerTarget.BindingShotCenter = true;
                    centerTarget.Broadcast(new S.SetBindingShot { ObjectID = centerTarget.ObjectID, Enabled = true, Value = value });

                    LevelMagic(magic);
                    break;

                #endregion

                #region VampireShot, PoisonShot, CrippleShot
                case Spell.VampireShot:
                    CompleteVampireShot(magic, (int)data[1], (MapObject)data[2]);
                    break;

                case Spell.PoisonShot:
                    CompletePoisonShot(magic, (int)data[1], (MapObject)data[2]);
                    break;

                case Spell.CrippleShot:
                    CompleteCrippleShot(magic, (int)data[1], (MapObject)data[2]);
                    break;
                #endregion

                #region ArcherSummons
                case Spell.SummonVampire:
                case Spell.SummonToad:
                case Spell.SummonSnakes:
                    value = (int)data[1];
                    location = (Point)data[2];
                    target = (MapObject)data[3];

                    int SummonType = 0;
                    switch (magic.Spell)
                    {
                        case Spell.SummonVampire:
                            SummonType = 1;
                            break;
                        case Spell.SummonToad:
                            SummonType = 2;
                            break;
                        case Spell.SummonSnakes:
                            SummonType = 3;
                            break;
                    }
                    if (SummonType == 0) return;

                    for (int i = 0; i < Pets.Count; i++)
                    {
                        monster = Pets[i];
                        if ((monster.Info.Name != (SummonType == 1 ? Settings.VampireName : (SummonType == 2 ? Settings.ToadName : Settings.SnakeTotemName))) || monster.Dead) continue;
                        if (monster.Node == null) continue;
                        monster.ActionList.Add(new DelayedAction(DelayedType.Recall, Envir.Time + 500, target));
                        monster.Target = target;
                        return;
                    }

                    if (Pets.Where(x => x.Race == ObjectType.Monster).Count() > 1) return;
                    MonsterInfo info = Envir.GetMonsterInfo((SummonType == 1 ? Settings.VampireName : (SummonType == 2 ? Settings.ToadName : Settings.SnakeTotemName)));
                    if (info == null) return;

                    LevelMagic(magic);

                    monster = MonsterObject.GetMonster(info);
                    monster.PetLevel = magic.Level;
                    monster.Master = this;
                    monster.MaxPetLevel = (byte)(1 + magic.Level * 2);
                    monster.Direction = Direction;
                    monster.ActionTime = Envir.Time + 1000;
                    monster.Target = target;

                    if (SummonType == 1)
                        ((VampireSpider)monster).AliveTime = Envir.Time + ((magic.Level * 1500) + 15000);
                    if (SummonType == 2)
                        ((SpittingToad)monster).AliveTime = Envir.Time + ((magic.Level * 2000) + 25000);
                    if (SummonType == 3)
                        ((SnakeTotem)monster).AliveTime = Envir.Time + ((magic.Level * 1500) + 20000);

                    //Pets.Add(monster);

                    DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + 500, this, magic, monster, location);
                    CurrentMap.ActionList.Add(action);
                    break;
                #endregion

                #region Taunt
                case Spell.Taunt:
                    {
                        MapObject ob = (MapObject)data[1];
                        if (ob != null && ob.Race == ObjectType.Monster)
                        {
                            if (!ob.IsAttackTarget(this)) break;
                            if (ob.Hidden && (Level < ob.Level)) break;
                            ob.Target = this;
                            LevelMagic(magic);
                        }
                    }
                    break;
                    #endregion

                #region Plague

                case Spell.Plague:
                    value = (int)data[1];
                    target = (MapObject)data[2];
                    switch (target.Race)
                    {
                        case ObjectType.Monster:
                        case ObjectType.Player:
                            //Only targets
                            if (target.IsAttackTarget(this))
                            {
                                int chance = Envir.Random.Next(21 - magic.Level * 2);
                                int duration = (2 * (magic.Level + 1)) + (value / 8);
                                PoisonType poison;
                                if (new int[] { 0, 1, 3 }.Contains(chance)) //3 in 15 chances it'll slow
                                {
                                    poison = PoisonType.Slow;
                                    if (target.Race == ObjectType.Player)
                                        duration = magic.Level + 3;
                                }
                                else if (new int[] { 3, 4 }.Contains(chance)) //2 in 15 chances it'll freeze
                                {
                                    poison = PoisonType.Frozen;
                                    if (target.Race == ObjectType.Player)
                                        duration = magic.Level + 2;
                                }
                                else if (new int[] { 5, 6, 7 }.Contains(chance)) //5 in 15 chances it'll red/green
                                {
                                    poison = (PoisonType)data[3];
                                    duration = (2 * (magic.Level + 1)) + (value / 8);
                                }
                                else if (new int[] { 8, 9 }.Contains(chance))
                                {
                                    poison = PoisonType.Paralysis;
                                    if (target.Race == ObjectType.Player)
                                        duration = magic.Level + 2;
                                }
                                else //5 in 15 chances it'll do nothing
                                {
                                    poison = PoisonType.None;
                                }

                                int tempValue = 0;

                                if (poison != PoisonType.None)
                                {
                                    if (poison == PoisonType.Green)
                                        tempValue = value / 15 + magic.Level + 1;

                                    target.ApplyPoison(new Poison { PType = poison, Duration = duration, TickSpeed = 1000, Value = tempValue, Owner = this }, this, false, false);
                                }

                                target.Attacked(this, value * 2, DefenceType.MAC, false);

                                if (target.Race == ObjectType.Player)
                                {
                                    PlayerObject tempOb = (PlayerObject)target;
                                    tempOb.ChangeMP(Envir.Random.Next(MinSC, MaxSC));
                                }

                                LevelMagic(magic);
                            }
                            break;
                    }

                break;

                #endregion

                case Spell.SlashingBurst:
                    {
                        location = (Point)data[1];
                        MirDirection dir = (MirDirection)data[2];
                        // telpo location
                        location = Functions.PointMove(location, dir, 2);

                        if (!CurrentMap.ValidPoint(location)) return;

                        var cellObjects = CurrentMap.GetCellObjects(location);

                        bool blocked = false;
                        if (cellObjects != null)
                        {
                            for (int c = 0; c < cellObjects.Count; c++)
                            {
                                MapObject ob = cellObjects[c];
                                if (!ob.Blocking) continue;
                                blocked = true;
                                if ((cellObjects == null) || blocked) break;
                            }
                        }

                        // blocked telpo cancel
                        if (blocked) return;

                        Teleport(CurrentMap, location, false);
                    }
                    break;
            }
        }

        private void CompleteCrippleShot(UserMagic magic, int value, MapObject target)
        {
            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (target.Attacked(this, value, DefenceType.MAC, false) == 0) return;
            int buffTime = 5 + (5 * magic.Level);

            bool hasVampBuff = (Buffs.Where(x => x.Type == BuffType.VampireShot).ToList().Count() > 0);
            bool hasPoisonBuff = (Buffs.Where(x => x.Type == BuffType.PoisonShot).ToList().Count() > 0);
            if (hasVampBuff || hasPoisonBuff)
            {
                Point place = target.CurrentLocation;
                for (int y = place.Y - 1; y <= place.Y + 1; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;
                    for (int x = place.X - 1; x <= place.X + 1; x++)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;
                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;
                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject targetob = cellObjects[i];
                            if (targetob.Race != ObjectType.Monster && targetob.Race != ObjectType.Player) continue;
                            if (targetob == null || !targetob.IsAttackTarget(this) || targetob.Node == null) continue;
                            if (targetob.Dead) continue;

                            if (hasVampBuff)//Vampire Effect
                            {
                                //cancel out buff
                                AddBuff(new Buff { Type = BuffType.VampireShot, Caster = this, ExpireTime = Envir.Time + 1000, Values = new int[] { value }, Visible = true, ObjectID = this.ObjectID });

                                if (VampAmount == 0) VampTime = Envir.Time + 1000;
                                VampAmount += (ushort)(value * (magic.Level + 1) * 0.25F);
                            }
                            if (hasPoisonBuff)//Poison Effect
                            {
                                //cancel out buff
                                AddBuff(new Buff { Type = BuffType.PoisonShot, Caster = this, ExpireTime = Envir.Time + 1000, Values = new int[] { value }, Visible = true, ObjectID = this.ObjectID });

                                if (Envir.Random.Next(5) == 0)
                                {
                                    targetob.ApplyPoison(new Poison
                                    {
                                        Duration = 8,
                                        Owner = this,
                                        PType = PoisonType.Green,
                                        TickSpeed = 2000,
                                        Value = value / 10 + magic.Level + 1 + Envir.Random.Next(PoisonAttack)
                                    }, this);
                                    targetob.OperateTime = 0;
                                }
                            }
                            if (hasPoisonBuff || hasVampBuff)
                                target.Attacked(this, (int)(2.0F * value), DefenceType.MAC, false);
                            else
                                target.Attacked(this, value, DefenceType.MAC, false);

                        }
                    }
                }
            }
            LevelMagic(magic);
        }

        private void CompletePoisonShot(UserMagic magic, int value, MapObject target)
        {
            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (target.Attacked(this, value, DefenceType.AC, false) == 0) return;
            int buffTime = 5 + (5 * magic.Level);

          //  bool hasVampBuff = (Buffs.Where(x => x.Type == BuffType.VampireShot).ToList().Count() > 0);
            bool hasPoisonBuff = (Buffs.Where(x => x.Type == BuffType.PoisonShot).ToList().Count() > 0);
            if (!hasPoisonBuff && (Envir.Random.Next(20) >= 8))//40% chance
            {
                AddBuff(new Buff { Type = BuffType.PoisonShot, Caster = this, ExpireTime = Envir.Time + (buffTime * 1000), Values = new int[] { value }, Visible = true, ObjectID = this.ObjectID });
                BroadcastInfo();
            }

            LevelMagic(magic);
        }

        private void CompleteVampireShot(UserMagic magic, int value, MapObject target)
        {
            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (target.Attacked(this, value, DefenceType.MAC, false) == 0) return;

            int buffTime = 5 + (5 * magic.Level);
            bool hasVampBuff = Buffs.Any(x => x.Type == BuffType.VampireShot);
           // bool hasPoisonBuff = (Buffs.Where(x => x.Type == BuffType.PoisonShot).ToList().Count() > 0);
            if (!hasVampBuff)
            {
                AddBuff(new Buff { Type = BuffType.VampireShot, Caster = this, ExpireTime = Envir.Time + (buffTime * 1000), Values = new int[] { value }, Visible = true, ObjectID = this.ObjectID });
                BroadcastInfo();
            }

            LevelMagic(magic);
        }

        private void CompleteMine(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            if (target == null) return;
            target.Broadcast(new S.MapEffect { Effect = SpellEffect.Mine, Location = target.CurrentLocation, Value = (byte)Direction });
            //target.Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.Mine });
            if ((byte)target.Direction < 6)
                target.Direction++;
            target.Broadcast(target.GetInfo());
        }
        private void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            bool damageWeapon = (bool)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (target.Attacked(this, damage, defence, damageWeapon) <= 0) return;

            //Level Fencing / SpiritSword
            foreach (UserMagic magic in Info.Magics)
            {
                switch (magic.Spell)
                {
                    case Spell.Fencing:
                    case Spell.SpiritSword:
                    case Spell.Strong://todo:技能修改:增加被动:增强体质
                    case Spell.JiBenGunFa:
                        LevelMagic(magic);
                        break;
                }
            }
        }
        private void CompleteDamageIndicator(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            DamageType type = (DamageType)data[1];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.BroadcastDamageIndicator(type);
        }

        private void CompleteNPC(IList<object> data)
        {
            uint npcid = (uint)data[0];
            string page = (string)data[1];

            if (data.Count > 3)
            {
                Map map = (Map)data[2];
                Point coords = (Point)data[3];

                Teleport(map, coords);
            }

            NPCDelayed = true;

            if (page.Length > 0)
            {
                if (npcid == DefaultNPC.ObjectID)
                {
                    DefaultNPC.Call(this, page.ToUpper());
                }
                else
                {
                    // TODO: 注意修改
                    NPCObject obj = CurrentMap.Objects.FirstOrDefault(x => x.ObjectID == npcid) as NPCObject;

                    if (obj != null)
                        obj.Call(this, page);
                }

                CallNPCNextPage();
            }
        }
        private void CompletePoison(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            PoisonType pt = (PoisonType)data[1];
            SpellEffect sp = (SpellEffect)data[2];
            int duration = (int)data[3];
            int tickSpeed = (int)data[4];

            if (target == null) return;

            target.ApplyPoison(new Poison { PType = pt, Duration = duration, TickSpeed = tickSpeed }, this);
            target.Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = sp });
        }

        private UserItem GetAmulet(int count, int shape = 0)
        {
            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                UserItem item = Info.Equipment[i];
                if (item != null && item.Info.Type == ItemType.Amulet && item.Info.Shape == shape && item.Count >= count)
                    return item;
            }

            return null;
        }
        private UserItem GetPoison(int count, byte shape = 0)
        {
            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                UserItem item = Info.Equipment[i];
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
        private UserItem GetBait(int count)
        {
            UserItem item = Info.Equipment[(int)EquipmentSlot.Weapon];
            if (item == null || item.Info.Type != ItemType.Weapon || (item.Info.Shape != 49 && item.Info.Shape != 50)) return null;

            UserItem bait = item.Slots[(int)FishingSlot.Bait];

            if (bait == null || bait.Count < count) return null;

            return bait;
        }

        private UserItem GetFishingItem(FishingSlot type)
        {
            UserItem item = Info.Equipment[(int)EquipmentSlot.Weapon];
            if (item == null || item.Info.Type != ItemType.Weapon || (item.Info.Shape != 49 && item.Info.Shape != 50)) return null;

            UserItem fishingItem = item.Slots[(int)type];

            if (fishingItem == null) return null;

            return fishingItem;
        }
        private void DeleteFishingItem(FishingSlot type)
        {
            UserItem item = Info.Equipment[(int)EquipmentSlot.Weapon];
            if (item == null || item.Info.Type != ItemType.Weapon || (item.Info.Shape != 49 && item.Info.Shape != 50)) return;

            UserItem slotItem = Info.Equipment[(int)EquipmentSlot.Weapon].Slots[(int)type];

            Enqueue(new S.DeleteItem { UniqueID = slotItem.UniqueID, Count = 1 });
            Info.Equipment[(int)EquipmentSlot.Weapon].Slots[(int)type] = null;

            Report.ItemChanged("FishingConsumable", slotItem, 1, 1);
        }
        private void DamagedFishingItem(FishingSlot type, int lossDura)
        {
            UserItem item = GetFishingItem(type);

            if (item != null)
            {
                if (item.CurrentDura <= 0)
                {

                    DeleteFishingItem(type);
                }
                else
                {
                    DamageItem(item, lossDura, true);
                }
            }
        }

        public UserMagic GetMagic(Spell spell)
        {
            for (int i = 0; i < Info.Magics.Count; i++)
            {
                UserMagic magic = Info.Magics[i];
                if (magic.Spell != spell) continue;
                return magic;
            }

            return null;
        }

        public void LevelMagic(UserMagic magic)
        {
            byte exp = (byte)(Envir.Random.Next(3) + 1);

            if ((Settings.MentorSkillBoost) && (Info.Mentor != 0) && (Info.isMentor))
            {
                Buff buff = Buffs.Where(e => e.Type == BuffType.Mentee).FirstOrDefault();
                if (buff != null)
                {
                    CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);
                    PlayerObject player = Envir.GetPlayer(Mentor.Name);
                    if (player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                        if (SkillNeckBoost == 1) exp *= 2;
                }
            }

            exp *= SkillNeckBoost;

            if (Level == 65535) exp = byte.MaxValue;

            int oldLevel = magic.Level;

            switch (magic.Level)
            {
                case 0:
                    if (Level < magic.Info.Level1)
                        return;

                    magic.Experience += exp;
                    if (magic.Experience >= magic.Info.Need1)
                    {
                        magic.Level++;
                        magic.Experience = (ushort)(magic.Experience - magic.Info.Need1);
                        RefreshStats();
                    }
                    break;
                case 1:
                    if (Level < magic.Info.Level2)
                        return;

                    magic.Experience += exp;
                    if (magic.Experience >= magic.Info.Need2)
                    {
                        magic.Level++;
                        magic.Experience = (ushort)(magic.Experience - magic.Info.Need2);
                        RefreshStats();
                    }
                    break;
                case 2:
                    if (Level < magic.Info.Level3)
                        return;

                    magic.Experience += exp;
                    if (magic.Experience >= magic.Info.Need3)
                    {
                        magic.Level++;
                        magic.Experience = 0;
                        RefreshStats();
                    }
                    break;

                case 3:
                    if (Level < magic.Info.Level4 || !Info.HumUp)
                        return;

                    magic.Experience += exp;
                    if (magic.Experience >= magic.Info.Need4)
                    {
                        magic.Level++;
                        magic.Experience = 0;
                        RefreshStats();
                    }
                    break;
                default:
                    return;
            }

            if (oldLevel != magic.Level)
            {
                long delay = magic.GetDelay();
                Enqueue(new S.MagicDelay { Spell = magic.Spell, Delay = delay });
            }

            Enqueue(new S.MagicLeveled { Spell = magic.Spell, Level = magic.Level, Experience = magic.Experience });

        }

        public bool CheckMovement(Point location)
        {
            if (Envir.Time < MovementTime) return false;

            //Script triggered coords
            for (int s = 0; s < CurrentMap.Info.ActiveCoords.Count; s++)
            {
                Point activeCoord = CurrentMap.Info.ActiveCoords[s];

                if (activeCoord != location) continue;

                CallDefaultNPC(DefaultNPCType.MapCoord, CurrentMap.Info.FileName, activeCoord.X, activeCoord.Y);
            }

            //Map movements
            for (int i = 0; i < CurrentMap.Info.Movements.Count; i++)
            {
                MovementInfo info = CurrentMap.Info.Movements[i];

                if (info.Source != location) continue;

                if (info.NeedHole)
                {
                    var cellObjects = CurrentMap.GetCellObjects(location);

                    if (cellObjects == null ||
                        cellObjects.Where(ob => ob.Race == ObjectType.Spell).All(ob => ((SpellObject)ob).Spell != Spell.DigOutZombie))
                        continue;
                }

                if (info.ConquestIndex > 0)
                {
                    if (MyGuild == null || MyGuild.Conquest == null) continue;
                    if (MyGuild.Conquest.Info.Index != info.ConquestIndex) continue;
                }

                if (info.NeedMove) //use with ENTERMAP npc command
                {
                    NPCMoveMap = Envir.GetMap(info.MapIndex, GetRegionID());
                    NPCMoveCoord = info.Destination;
                    continue;
                }

                Map temp = Envir.GetMap(info.MapIndex, GetRegionID());

                if (temp == null || !temp.ValidPoint(info.Destination) || temp.Info.NoEnter)
                {
                    SMain.EnqueueDebugging(string.Format("temp no enter or info.destination invalid {0}", temp.Info.Title));
                    continue;
                }

                CurrentMap.RemoveObject(this);
                Broadcast(new S.ObjectRemove { ObjectID = ObjectID });

                CompleteMapMovement(temp, info.Destination, CurrentMap, CurrentLocation);
                return true;
            }

            return false;
        }
        private void CompleteMapMovement(params object[] data)
        {
            if (this == null) return;
            Map temp = (Map)data[0];
            Point destination = (Point)data[1];
            Map checkmap = (Map)data[2];
            Point checklocation = (Point)data[3];

            if (CurrentMap != checkmap || CurrentLocation != checklocation) return;

            bool mapChanged = temp != CurrentMap;

            CurrentMap = temp;
            CurrentLocation = destination;

            CurrentMap.AddObject(this);

            MovementTime = Envir.Time + MovementDelay;

            Enqueue(new S.MapChanged
            {
                FileName = CurrentMap.Info.FileName,
                Title = CurrentMap.Info.Title + "-" + CurrentMap.Region,
                MiniMap = CurrentMap.Info.MiniMap,
                BigMap = CurrentMap.Info.BigMap,
                Lights = CurrentMap.Info.Light,
                Location = CurrentLocation,
                Direction = Direction,
                MapDarkLight = CurrentMap.Info.MapDarkLight,
                Music = CurrentMap.Info.Music
            });

            if (RidingMount) RefreshMount();

            GetObjects();

            SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

            if (szi != null)
            {
                BindLocation = szi.Location;
                BindMapIndex = CurrentMapIndex;
                InSafeZone = true;
            }
            else
                InSafeZone = false;

            if (mapChanged)
            {
                CallDefaultNPC(DefaultNPCType.MapEnter, CurrentMap.Info.FileName);
            }

            if (Info.Married != 0)
            {
                CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);
                PlayerObject player = Envir.GetPlayer(Lover.Name);

                if (player != null) player.GetRelationship(false);
            }

            CheckConquest(true);
        }

        public override bool Teleport(Map temp, Point location, bool effects = true, byte effectnumber = 0)
        {
            Map oldMap = CurrentMap;
            Point oldLocation = CurrentLocation;

            bool mapChanged = temp != oldMap;

            if (!base.Teleport(temp, location, effects)) return false;

            Enqueue(new S.MapChanged
            {
                FileName = CurrentMap.Info.FileName,
                Title = CurrentMap.Info.Title + "-" + CurrentMap.Region,
                MiniMap = CurrentMap.Info.MiniMap,
                BigMap = CurrentMap.Info.BigMap,
                Lights = CurrentMap.Info.Light,
                Location = CurrentLocation,
                Direction = Direction,
                MapDarkLight = CurrentMap.Info.MapDarkLight,
                Music = CurrentMap.Info.Music
            });

            if (effects) Enqueue(new S.ObjectTeleportIn { ObjectID = ObjectID, Type = effectnumber });

            //Cancel actions
            if (TradePartner != null)
                TradeCancel();

            if (ItemRentalPartner != null)
                CancelItemRental();

            if (RidingMount) RefreshMount();
            CancelSpelling();

            GetObjectsPassive();

            SafeZoneInfo szi = CurrentMap.GetSafeZone(CurrentLocation);

            if (szi != null)
            {
                BindLocation = szi.Location;
                BindMapIndex = CurrentMapIndex;
                InSafeZone = true;
            }
            else
                InSafeZone = false;

            CheckConquest();

            Fishing = false;
            Enqueue(GetFishInfo());

            if (mapChanged)
            {
                CallDefaultNPC(DefaultNPCType.MapEnter, CurrentMap.Info.FileName);

                if (Info.Married != 0)
                {
                    CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);
                    PlayerObject player = Envir.GetPlayer(Lover.Name);

                    if (player != null) player.GetRelationship(false);
                }
            }

            if (CheckStacked())
            {
                StackingTime = Envir.Time + 1000;
                Stacking = true;
            }

            Report.MapChange("Teleported", oldMap.Info, CurrentMap.Info);

            return true;
        }
        public bool TeleportEscape(int attempts)
        {
            Map temp = Envir.GetMap(BindMapIndex, GetRegionID());

            for (int i = 0; i < attempts; i++)
            {
                Point location = new Point(BindLocation.X + Envir.Random.Next(-100, 100),
                                           BindLocation.Y + Envir.Random.Next(-100, 100));

                if (Teleport(temp, location)) return true;
            }

            return false;
        }

        private Packet GetMountInfo()
        {
            return new S.MountUpdate
            {
                ObjectID = ObjectID,
                RidingMount = RidingMount,
                MountType = MountType
            };
        }
        public Packet GetUpdateInfo()
        {
            UpdateConcentration();
            return new S.PlayerUpdate
            {
                ObjectID = ObjectID,
                Weapon = Looks_Weapon,
                WeaponEffect = Looks_WeaponEffect,
                Armour = Looks_Armour,
                Light = Light,
                WingEffect = Looks_Wings,
                LooksHumUp = Looks_HumUp
            };
        }

        public override Packet GetInfo()
        {
            //should never use this but i leave it in for safety
            if (Observer || Hidden2) return null;

            string gName = "";
            string conquest = "";
            if (MyGuild != null)
            {
                gName = MyGuild.Name;
                if (MyGuild.Conquest != null)
                {
                    conquest = "[" + MyGuild.Conquest.Info.Name + "]";
                    gName = gName + conquest;
                }

            }

            return new S.ObjectPlayer
            {
                ObjectID = ObjectID,
                Name = CurrentMap.Info.NoNames ? "?????" : Name,
                NameColour = NameColour,
                GuildName = CurrentMap.Info.NoNames ? "?????" : gName,
                GuildRankName = CurrentMap.Info.NoNames ? "?????" : MyGuildRank != null ? MyGuildRank.Name : "",
                Class = Class,
                Gender = Gender,
                Level = Level,
                Location = CurrentLocation,
                Direction = Direction,
                Hair = Hair,
                Weapon = Looks_Weapon,
                WeaponEffect = Looks_WeaponEffect,
                Armour = Looks_Armour,
                Light = Light,
                Poison = CurrentPoison,
                Dead = Dead,
                Hidden = Hidden,
                Effect = MagicShield ? SpellEffect.MagicShieldUp :
                ElementalBarrier ? SpellEffect.ElementalBarrierUp :
                ElementalBarrier1 ? SpellEffect.ElementalBarrierUp1 :
                SpellEffect.None,

                WingEffect = Looks_Wings,
                MountType = MountType,
                RidingMount = RidingMount,
                Fishing = Fishing,

                TransformType = TransformType,

                ElementOrbEffect = (uint)GetElementalOrbCount(),
                ElementOrbLvl = (uint)ElementsLevel,
                ElementOrbMax = (uint)Settings.OrbsExpList[Settings.OrbsExpList.Count - 1],

                Buffs = Buffs.Where(d => d.Visible).Select(e => e.Type).ToList(),

                LevelEffects = LevelEffects,

                LooksHumUp = Looks_HumUp
            };
        }
        public Packet GetInfoEx(PlayerObject player)
        {
            var p = (S.ObjectPlayer)GetInfo();

            if (p != null)
            {
                p.NameColour = GetNameColour(player);
            }

            return p;
        }

        public override bool IsAttackTarget(PlayerObject attacker)
        {
            if (attacker == null || attacker.Node == null) return false;
            if (Dead || InSafeZone || attacker.InSafeZone || attacker == this || GMGameMaster) return false;
            if (CurrentMap.Info.NoFight) return false;

            switch (attacker.AMode)
            {
                case AttackMode.All:
                    return true;
                case AttackMode.Group:
                    return GroupMembers == null || !GroupMembers.Contains(attacker);
                case AttackMode.Guild:
                    return MyGuild == null || MyGuild != attacker.MyGuild;
                case AttackMode.EnemyGuild:
                    return MyGuild != null && MyGuild.IsEnemy(attacker.MyGuild);
                case AttackMode.Peace:
                    return false;
                case AttackMode.RedBrown:
                    return PKPoints >= 200 || Envir.Time < BrownTime;
            }

            return true;
        }
        public override bool IsAttackTarget(MonsterObject attacker)
        {
            if (attacker == null || attacker.Node == null) return false;
            if (Dead || attacker.Master == this || GMGameMaster) return false;
            if (attacker.Info.AI == 6 || attacker.Info.AI == 58) return PKPoints >= 200;
            if (attacker.Master == null) return true;
            if (InSafeZone || attacker.InSafeZone || attacker.Master.InSafeZone) return false;

            if (LastHitter != attacker.Master && attacker.Master.LastHitter != this)
            {
                bool target = false;

                for (int i = 0; i < attacker.Master.Pets.Count; i++)
                {
                    if (attacker.Master.Pets[i].Target != this) continue;

                    target = true;
                    break;
                }

                if (!target)
                    return false;
            }

            switch (attacker.Master.AMode)
            {
                case AttackMode.All:
                    return true;
                case AttackMode.Group:
                    return GroupMembers == null || !GroupMembers.Contains(attacker.Master);
                case AttackMode.Guild:
                    return true;
                case AttackMode.EnemyGuild:
                    return false;
                case AttackMode.Peace:
                    return false;
                case AttackMode.RedBrown:
                    return PKPoints >= 200 || Envir.Time < BrownTime;
            }

            return true;
        }

        public override bool IsFriendlyTarget(PlayerObject ally)
        {
            if (ally == this) return true;

            switch (ally.AMode)
            {
                case AttackMode.Group:
                    return GroupMembers != null && GroupMembers.Contains(ally);
                case AttackMode.RedBrown:
                    return PKPoints < 200 & Envir.Time > BrownTime;
                case AttackMode.Guild:
                    return MyGuild != null && MyGuild == ally.MyGuild;
                case AttackMode.EnemyGuild:
                    return true;
            }
            return true;
        }
        public override bool IsFriendlyTarget(MonsterObject ally)
        {
            if (ally.Race != ObjectType.Monster) return false;
            if (ally.Master == null) return false;

            switch (ally.Master.Race)
            {
                case ObjectType.Player:
                    if (!ally.Master.IsFriendlyTarget(this)) return false;
                    break;
                case ObjectType.Monster:
                    return false;
            }

            return true;
        }
        public override int Attacked(PlayerObject attacker, int damage, DefenceType type = DefenceType.ACAgility, bool damageWeapon = true, bool isReflectDamage = false)
        {
            int armour = 0;
            bool critical = false;

            for (int i = 0; i < Buffs.Count; i++)
            {
                switch (Buffs[i].Type)
                {
                    case BuffType.MoonLight:
                    case BuffType.DarkBody:
                    case BuffType.MoonMist:
                        Buffs[i].ExpireTime = 0;
                        break;
                    case BuffType.EnergyShield:
                        int rate = Buffs[i].Values[0];

                        if (Envir.Random.Next(rate) == 0)
                        {
                            if (HP + ((ushort)Buffs[i].Values[1]) >= MaxHP)
                                SetHP(MaxHP);
                            else
                                ChangeHP(Buffs[i].Values[1]);
                        }
                        break;
                }
            }

            switch (type)
            {
                case DefenceType.ACAgility:
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.AC:
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.MACAgility:
                    if ((Settings.PvpCanResistMagic) && (Envir.Random.Next(Settings.MagicResistWeight) < MagicResist))
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.MAC:
                    if ((Settings.PvpCanResistMagic) && (Envir.Random.Next(Settings.MagicResistWeight) < MagicResist))
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.Agility:
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    break;
            }

            armour = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(armour * ArmourRate))));
            damage = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(damage * DamageRate))));

            if (damageWeapon)
                attacker.DamageWeapon();

            damage += attacker.AttackBonus;

            if ((type == DefenceType.AC || type == DefenceType.ACAgility) && Envir.Random.Next(100) < ReflectRate)
            {
                if (attacker.IsAttackTarget(this))
                {
                    int reflectDamage = damage * Settings.ReflectDamage / 100;
                    attacker.Attacked(this, reflectDamage, type, false, true);
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Reflect }, CurrentLocation);
                }
            }

            if (MagicShield)
                damage -= damage * (MagicShieldLv + 3) / 10;

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }
            
            if ((attacker.CriticalRate * Settings.CriticalRateWeight) > Envir.Random.Next(100))
            {
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Critical }, CurrentLocation);
                damage = Math.Min(int.MaxValue, damage + (int)Math.Floor(damage * ((double)attacker.CriticalDamage / Settings.CriticalDamageWeight)));
                critical = true;
            }

            if (MagicShield)
            {
                MagicShieldTime -= (damage - armour) * 60;
                AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
            }

            ElementalBarrierTime -= (damage - armour) * 70;

            if (ElementalBarrier1)
            {
                damage -= ((damage - armour) * (ElementalBarrierLv1 + 2) / 10);
                ElementalBarrierTime1 -= 3000;
                ElementalBarrierTime1 = Math.Max(0, ElementalBarrierTime1);
            }

            if (ElementalBarrier)
                damage -= (int)(damage * (ElementalBarrierLv * 1.5F + 2) / 10);//技能修改:金刚术技能防御数值修改

            if (DamageReduceRate > 0)
                damage -= damage * DamageReduceRate / 100;

            Buff buff = Buffs.FirstOrDefault(e => e.Type == BuffType.HealingCircle2);
            if (buff != null)
                damage -= (int)(damage * 1F * buff.Values[0] / 100);

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }

            if (attacker.LifeOnHit > 0)
                attacker.ChangeHP(attacker.LifeOnHit);

            if (attacker.HpDrainRate > 0 && !attacker.Dead)
            {
                byte rate = (type == DefenceType.AC || type == DefenceType.ACAgility) ? attacker.HpDrainRate : (byte)(attacker.HpDrainRate * 0.8);
                attacker.HpDrain += Math.Max(0, ((float)(damage - armour) / 100) * rate);
                if (attacker.HpDrain > 2)
                {
                    int HpGain = (int)Math.Floor(attacker.HpDrain);
                    attacker.ChangeHP(HpGain);
                    attacker.HpDrain -= HpGain;
                }
            }

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (PoisonList[i].PType != PoisonType.LRParalysis) continue;

                PoisonList.RemoveAt(i);
                OperateTime = 0;
            }


            LastHitter = attacker;
            LastHitTime = Envir.Time + 10000;
            RegenTime = Envir.Time + RegenDelay;
            LogTime = Envir.Time + Globals.LogDelay;

            if (Envir.Time > BrownTime && PKPoints < 200 && !AtWar(attacker) && !isReflectDamage)
                attacker.BrownTime = Envir.Time + Settings.Minute;

            ushort LevelOffset = (byte)(Level > attacker.Level ? 0 : Math.Min(10, attacker.Level - Level));

            if (attacker.HasParalysisRing && type != DefenceType.MAC && type != DefenceType.MACAgility && 1 == Envir.Random.Next(1, 15))
            {
                ApplyPoison(new Poison { PType = PoisonType.Paralysis, Duration = 5, TickSpeed = 1000 }, attacker);
            }
            if ((attacker.Freezing > 0) && (Settings.PvpCanFreeze) && type != DefenceType.MAC && type != DefenceType.MACAgility)
            {
                if ((Envir.Random.Next(Settings.FreezingAttackWeight) < attacker.Freezing) && (Envir.Random.Next(LevelOffset) == 0))
                    ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = Math.Min(10, (3 + Envir.Random.Next(attacker.Freezing))), TickSpeed = 1000 }, attacker);
            }

            if (attacker.PoisonAttack > 0 && type != DefenceType.MAC && type != DefenceType.MACAgility)
            {
                if ((Envir.Random.Next(Settings.PoisonAttackWeight) < attacker.PoisonAttack) && (Envir.Random.Next(LevelOffset) == 0))
                    ApplyPoison(new Poison { PType = PoisonType.Green, Duration = 5, TickSpeed = 1000, Value = Math.Min(10, 3 + Envir.Random.Next(attacker.PoisonAttack)) }, attacker);
            }

            attacker.GatherElement();

            DamageDura();
            CancelSpelling();

            ActiveReincarnation = false;

            Enqueue(new S.Struck { AttackerID = attacker.ObjectID });
            Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = attacker.ObjectID, Direction = Direction, Location = CurrentLocation });

            if (critical)
                BroadcastDamageIndicator(DamageType.Critical, armour - damage);
            else
                BroadcastDamageIndicator(DamageType.Hit, armour - damage);

            ChangeHP(armour - damage);
            return damage - armour;
        }
        public override int Attacked(MonsterObject attacker, int damage, DefenceType type = DefenceType.ACAgility)
        {
            int armour = 0;

            for (int i = 0; i < Buffs.Count; i++)
            {
                switch (Buffs[i].Type)
                {
                    case BuffType.MoonLight:
                    case BuffType.DarkBody:
                    case BuffType.MoonMist:
                        Buffs[i].ExpireTime = 0;
                        break;
                    case BuffType.EnergyShield:
                        int rate = Buffs[i].Values[0];

                        if (Envir.Random.Next(rate < 2 ? 2 : rate) == 0)
                        {
                            if (HP + ((ushort)Buffs[i].Values[1]) >= MaxHP)
                                SetHP(MaxHP);
                            else
                                ChangeHP(Buffs[i].Values[1]);
                        }
                        break;
                }
            }

            switch (type)
            {
                case DefenceType.ACAgility:
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.AC:
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.MACAgility:
                    if (Envir.Random.Next(Settings.MagicResistWeight) < MagicResist)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.MAC:
                    if (Envir.Random.Next(Settings.MagicResistWeight) < MagicResist)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.Agility:
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }                             
                    break;
            }

            if ((type == DefenceType.AC || type == DefenceType.ACAgility) && Envir.Random.Next(100) < ReflectRate)
            {
                if (attacker.IsAttackTarget(this))
                {
                    int reflectDamage = damage * Settings.ReflectDamage / 100;
                    attacker.Attacked(this, reflectDamage, type, false);
                    CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Reflect }, CurrentLocation);
                }
            }

            armour = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(armour * ArmourRate))));
            damage = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(damage * DamageRate))));

            if (MagicShield)
                damage -= damage * (MagicShieldLv + 2) / 10;

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }

            if (MagicShield)
            {
                MagicShieldTime -= 3000;
                AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
            }


            if (ElementalBarrier)
            {
                ElementalBarrierTime -= (damage - armour) * 70;
                damage -= (int)(damage * (ElementalBarrierLv * 1.5F + 3) / 10);//技能修改:金刚术技能防御数值修改
            }

            Buff buff = Buffs.FirstOrDefault(e => e.Type == BuffType.HealingCircle2);
            if (buff != null && buff.Values.Length > 0)
                damage -= (int)(damage * (buff.Values[0] * 1F) / 100);

            if (ElementalBarrier1)
            {
                damage -= ((damage - armour) * (ElementalBarrierLv1 + 2) / 10);
                ElementalBarrierTime1 -= 3000;
                ElementalBarrierTime1 = Math.Max(0, ElementalBarrierTime1);
            }

            if (DamageReduceRate > 0)
                damage -= damage * DamageReduceRate / 100;

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (PoisonList[i].PType != PoisonType.LRParalysis) continue;

                PoisonList.RemoveAt(i);
                OperateTime = 0;
            }

            LastHitter = attacker.Master ?? attacker;
            LastHitTime = Envir.Time + 10000;
            RegenTime = Envir.Time + RegenDelay;
            LogTime = Envir.Time + Globals.LogDelay;

            DamageDura();
            CancelSpelling();

            ActiveReincarnation = false;

            if (StruckTime < Envir.Time)
            {
                Enqueue(new S.Struck { AttackerID = attacker.ObjectID });
                Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = attacker.ObjectID, Direction = Direction, Location = CurrentLocation });
                StruckTime = Envir.Time + 500;
            }

            BroadcastDamageIndicator(DamageType.Hit, armour - damage);

            ChangeHP(armour - damage);
            return damage - armour;
        }
        public override int Struck(int damage, DefenceType type = DefenceType.ACAgility)
        {
            int armour = 0;
            if (Hidden)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    switch (Buffs[i].Type)
                    {
                        case BuffType.MoonLight:
                        case BuffType.DarkBody:
                        case BuffType.MoonMist:
                            Buffs[i].ExpireTime = 0;
                            break;
                    }
                }
            }

            switch (type)
            {
                case DefenceType.ACAgility:
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.AC:
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.MACAgility:
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.MAC:
                    armour = GetDefencePower(MinAC, MaxAC);
                    break;
                case DefenceType.Agility:
                    break;
            }

            armour = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(armour * ArmourRate))));
            damage = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(damage * DamageRate))));

            if (MagicShield)
                damage -= damage * (MagicShieldLv + 2) / 10;

            if (ElementalBarrier)
                damage -= (int)(damage * (ElementalBarrierLv + 1.5F + 3) / 10);

            if (ElementalBarrier1)
                damage -= damage * (ElementalBarrierLv1 + 1) / 10;

            if (armour >= damage) return 0;

            if (MagicShield)
            {
                MagicShieldTime -= (damage - armour) * 60;
                AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
            }

            ElementalBarrierTime -= (damage - armour) * 60;
            RegenTime = Envir.Time + RegenDelay;
            LogTime = Envir.Time + Globals.LogDelay;

            DamageDura();
            CancelSpelling();
            ActiveReincarnation = false;
            Enqueue(new S.Struck { AttackerID = 0 });
            Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = 0, Direction = Direction, Location = CurrentLocation });

            ChangeHP(armour - damage);
            return damage - armour;
        }
        public override void ApplyPoison(Poison p, MapObject Caster = null, bool NoResist = false, bool ignoreDefence = true)
        {
            if ((Caster != null) && (!NoResist))
                if (((Caster.Race != ObjectType.Player) || Settings.PvpCanResistPoison) && (Envir.Random.Next(Settings.PoisonResistWeight) < PoisonResist))
                    return;

            if (!ignoreDefence && (p.PType == PoisonType.Green))
            {
                int armour = GetDefencePower(MinMAC, MaxMAC);

                if (p.Value < armour)
                    p.PType = PoisonType.None;
                else
                    p.Value -= armour;
            }

            if (p.Owner != null && p.Owner.Race == ObjectType.Player && Envir.Time > BrownTime && PKPoints < 200)
                p.Owner.BrownTime = Envir.Time + Settings.Minute;

            p.Duration = Math.Max(0, p.Duration - PoisonRecovery);
            if (p.Duration == 0) return;
            if (p.PType == PoisonType.None) return;

            for (int i = 0; i < PoisonList.Count; i++)
            {
                if (PoisonList[i].PType != p.PType) continue;
                if ((PoisonList[i].PType == PoisonType.Green) && (PoisonList[i].Value > p.Value)) return;//cant cast weak poison to cancel out strong poison
                if ((PoisonList[i].PType != PoisonType.Green) && ((PoisonList[i].Duration - PoisonList[i].Time) > p.Duration)) return;//cant cast 1 second poison to make a 1minute poison go away!
                if ((PoisonList[i].PType == PoisonType.Frozen) || (PoisonList[i].PType == PoisonType.Slow) || (PoisonList[i].PType == PoisonType.Paralysis) || (PoisonList[i].PType == PoisonType.LRParalysis)) return;//prevents mobs from being perma frozen/slowed
                if (p.PType == PoisonType.DelayedExplosion) return;
                ReceiveChat(Envir.Tr("You have been poisoned."), ChatType.System2);
                PoisonList[i] = p;
                return;
            }

            if (p.PType == PoisonType.DelayedExplosion)
            {
                ExplosionInflictedTime = Envir.Time + 1500;
                Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion });
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion });
                ReceiveChat(Envir.Tr("You are a walking explosive."), ChatType.System);
            }
            else
                ReceiveChat(Envir.Tr("You have been poisoned."), ChatType.System2);

            PoisonList.Add(p);
        }

        public override void AddBuff(Buff b)
        {
            if (Buffs.Any(d => d.Infinite && d.Type == b.Type)) return; //cant overwrite infinite buff with regular buff

            base.AddBuff(b);

            string caster = b.Caster != null ? b.Caster.Name : string.Empty;

            if (b.Values == null) b.Values = new int[1];

            S.AddBuff addBuff = new S.AddBuff { Type = b.Type, Caster = caster, Expire = b.ExpireTime - Envir.Time, Values = b.Values, Infinite = b.Infinite, ObjectID = ObjectID, Visible = b.Visible };
            Enqueue(addBuff);

            if (b.Visible) Broadcast(addBuff);

            RefreshStats();
        }
        public void PauseBuff(Buff b)
        {
            if (b.Paused) return;

            b.ExpireTime = b.ExpireTime - Envir.Time;
            b.Paused = true;
            Enqueue(new S.RemoveBuff { Type = b.Type, ObjectID = ObjectID });
        }
        public void UnpauseBuff(Buff b)
        {
            if (!b.Paused) return;

            b.ExpireTime = b.ExpireTime + Envir.Time;
            b.Paused = false;
            Enqueue(new S.AddBuff { Type = b.Type, Caster = Name, Expire = b.ExpireTime - Envir.Time, Values = b.Values, Infinite = b.Infinite, ObjectID = ObjectID, Visible = b.Visible });
        }

        public void EquipSlotItem(MirGridType grid, ulong id, int to, MirGridType gridTo)
        {
            S.EquipSlotItem p = new S.EquipSlotItem { Grid = grid, UniqueID = id, To = to, GridTo = gridTo, Success = false };

            UserItem Item = null;

            switch (gridTo)
            {
                case MirGridType.Mount:
                    Item = Info.Equipment[(int)EquipmentSlot.Mount];
                    break;
                case MirGridType.Fishing:
                    Item = Info.Equipment[(int)EquipmentSlot.Weapon];
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            if (Item == null || Item.Slots == null)
            {
                Enqueue(p);
                return;
            }

            if (gridTo == MirGridType.Fishing && (Item.Info.Shape != 49 && Item.Info.Shape != 50))
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Item.Slots.Length)
            {
                Enqueue(p);
                return;
            }

            if (Item.Slots[to] != null)
            {
                Enqueue(p);
                return;
            }

            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }

                    if (Info.Equipment[to] != null &&
                        Info.Equipment[to].Info.Bind.HasFlag(BindMode.DontStore))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                default:
                    Enqueue(p);
                    return;
            }


            int index = -1;
            UserItem temp = null;

            for (int i = 0; i < array.Length; i++)
            {
                temp = array[i];
                if (temp == null || temp.UniqueID != id) continue;
                index = i;
                break;
            }

            if (temp == null || index == -1)
            {
                Enqueue(p);
                return;
            }

            if ((temp.SoulBoundId != -1) && (temp.SoulBoundId != Info.Index))
            {
                Enqueue(p);
                return;
            }


            if (CanUseItem(temp))
            {
                if (temp.Info.NeedIdentify && !temp.Identified)
                {
                    temp.Identified = true;
                    Enqueue(new S.RefreshItem { Item = temp });
                }
                //if ((temp.Info.BindOnEquip) && (temp.SoulBoundId == -1))
                //{
                //    temp.SoulBoundId = Info.Index;
                //    Enqueue(new S.RefreshItem { Item = temp });
                //}
                //if (UnlockCurse && Info.Equipment[to].Cursed)
                //    UnlockCurse = false;

                Item.Slots[to] = temp;
                array[index] = null;

                p.Success = true;
                Enqueue(p);
                RefreshStats();

                Report.ItemMoved("EquipSlotItem", temp, grid, gridTo, index, to);

                return;
            }

            Enqueue(p);
        }
        public void RemoveItem(MirGridType grid, ulong id, int to)
        {
            S.RemoveItem p = new S.RemoveItem { Grid = grid, UniqueID = id, To = to, Success = false };
            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            if (to < 0 || to >= array.Length) return;

            UserItem temp = null;
            int index = -1;

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                temp = Info.Equipment[i];
                if (temp == null || temp.UniqueID != id) continue;
                index = i;
                break;
            }

            if (temp == null || index == -1)
            {
                Enqueue(p);
                return;
            }

            if (temp.Cursed && !UnlockCurse)
            {
                Enqueue(p);
                return;
            }

            /*
            if (temp.WeddingRing != -1)
            {
                Enqueue(p);
                return;
            }
            */

            if (!CanRemoveItem(grid, temp)) return;

            if (temp.Cursed)
                UnlockCurse = false;

            if (array[to] == null)
            {
                Info.Equipment[index] = null;

                array[to] = temp;
                p.Success = true;
                Enqueue(p);
                RefreshStats();
                Broadcast(GetUpdateInfo());

                Report.ItemMoved("RemoveItem", temp, MirGridType.Equipment, grid, index, to);

                return;
            }

            Enqueue(p);
        }
        public void RemoveSlotItem(MirGridType grid, ulong id, int to, MirGridType gridTo)
        {
            S.RemoveSlotItem p = new S.RemoveSlotItem { Grid = grid, UniqueID = id, To = to, GridTo = gridTo, Success = false };
            UserItem[] array;
            switch (gridTo)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            if (to < 0 || to >= array.Length) return;

            UserItem temp = null;
            UserItem slotTemp = null;
            int index = -1;

            switch (grid)
            {
                case MirGridType.Mount:
                    temp = Info.Equipment[(int)EquipmentSlot.Mount];
                    break;
                case MirGridType.Fishing:
                    temp = Info.Equipment[(int)EquipmentSlot.Weapon];
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            if (temp == null || temp.Slots == null)
            {
                Enqueue(p);
                return;
            }

            if (grid == MirGridType.Fishing && (temp.Info.Shape != 49 && temp.Info.Shape != 50))
            {
                Enqueue(p);
                return;
            }

            for (int i = 0; i < temp.Slots.Length; i++)
            {
                slotTemp = temp.Slots[i];
                if (slotTemp == null || slotTemp.UniqueID != id) continue;
                index = i;
                break;
            }

            if (slotTemp == null || index == -1)
            {
                Enqueue(p);
                return;
            }

            if (slotTemp.Cursed && !UnlockCurse)
            {
                Enqueue(p);
                return;
            }

            if (slotTemp.WeddingRing != -1)
            {
                Enqueue(p);
                return;
            }

            if (!CanRemoveItem(gridTo, slotTemp)) return;

            temp.Slots[index] = null;

            if (slotTemp.Cursed)
                UnlockCurse = false;

            if (array[to] == null)
            {
                array[to] = slotTemp;
                p.Success = true;
                Enqueue(p);
                RefreshStats();
                Broadcast(GetUpdateInfo());

                Report.ItemMoved("RemoveSlotItem", temp, grid, gridTo, index, to);

                return;
            }

            Enqueue(p);
        }
        public void MoveItem(MirGridType grid, int from, int to)
        {
            S.MoveItem p = new S.MoveItem { Grid = grid, From = from, To = to, Success = false };
            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                case MirGridType.Trade:
                    array = Info.Trade;
                    TradeItem();
                    break;
                case MirGridType.Refine:
                    array = Info.Refine;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            if (from >= 0 && to >= 0 && from < array.Length && to < array.Length)
            {
                if (array[from] == null)
                {
                    Report.ItemError("MoveItem", grid, grid, from, to);
                    ReceiveChat(Envir.Tr("Item Move Error - Please report the item you tried to move and the time"), ChatType.System);
                    Enqueue(p);
                    return;
                }

                UserItem i = array[to];
                array[to] = array[from];

                Report.ItemMoved("MoveItem", array[to], grid, grid, from, to);

                array[from] = i;

                Report.ItemMoved("MoveItem", array[from], grid, grid, to, from);

                p.Success = true;
                Enqueue(p);
                return;
            }

            Enqueue(p);
        }

        class ItemComparable : IComparer<UserItem>
        {
            public int Compare(UserItem s1, UserItem s2)
            {
                if (s1 == null) return 1;
                if (s2 == null) return -1;
                return s1.Info.Index - s2.Info.Index;
            }
        }

        public void RearrangeInventory()
        {
            Array.Sort(Info.Inventory, 6, Info.Inventory.Length - 6, new ItemComparable());

            S.RearrangeInventory p = new S.RearrangeInventory
            {
                Inventory = Info.Inventory,
            };

            Enqueue(p);
        }

        public void StoreItem(int from, int to)
        {
            S.StoreItem p = new S.StoreItem { From = from, To = to, Success = false };

            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
            {
                Enqueue(p);
                return;
            }
            NPCObject ob = null;
            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                ob = CurrentMap.NPCs[i];
                break;
            }

            if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
            {
                Enqueue(p);
                return;
            }


            if (from < 0 || from >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Account.Storage.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Info.Inventory[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (temp.Info.Bind.HasFlag(BindMode.DontStore))
            {
                Enqueue(p);
                return;
            }

            if (temp.RentalInformation != null && temp.RentalInformation.BindingFlags.HasFlag(BindMode.DontStore))
            {
                Enqueue(p);
                return;
            }

            if (Account.Storage[to] == null)
            {
                Account.Storage[to] = temp;
                Info.Inventory[from] = null;
                RefreshBagWeight();
                UpdateQuestItem(temp.Info);

                Report.ItemMoved("StoreItem", temp, MirGridType.Inventory, MirGridType.Storage, from, to);

                p.Success = true;
                Enqueue(p);
                return;
            }
            Enqueue(p);
        }
        public void TakeBackItem(int from, int to)
        {
            S.TakeBackItem p = new S.TakeBackItem { From = from, To = to, Success = false };

            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
            {
                Enqueue(p);
                return;
            }
            NPCObject ob = null;
            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                ob = CurrentMap.NPCs[i];
                break;
            }

            if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
            {
                Enqueue(p);
                return;
            }


            if (from < 0 || from >= Account.Storage.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Account.Storage[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (temp.Weight + CurrentBagWeight > MaxBagWeight)
            {
                ReceiveChat(Envir.Tr("Too heavy to get back."), ChatType.System);
                Enqueue(p);
                return;
            }

            if (Info.Inventory[to] == null)
            {
                Info.Inventory[to] = temp;
                Account.Storage[from] = null;

                Report.ItemMoved("TakeBackStoreItem", temp, MirGridType.Storage, MirGridType.Inventory, from, to);

                p.Success = true;
                RefreshBagWeight();
                UpdateQuestItem(temp.Info);
                Enqueue(p);

                return;
            }
            Enqueue(p);
        }
        public void EquipItem(MirGridType grid, ulong id, int to)
        {
            S.EquipItem p = new S.EquipItem { Grid = grid, UniqueID = id, To = to, Success = false };

            if (Fishing)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Equipment.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                default:
                    Enqueue(p);
                    return;
            }


            int index = -1;
            UserItem temp = null;

            for (int i = 0; i < array.Length; i++)
            {
                temp = array[i];
                if (temp == null || temp.UniqueID != id) continue;
                index = i;
                break;
            }

            if (temp == null || index == -1)
            {
                Enqueue(p);
                return;
            }
            if ((Info.Equipment[to] != null) && (Info.Equipment[to].Cursed) && (!UnlockCurse))
            {
                Enqueue(p);
                return;
            }

            if ((temp.SoulBoundId != -1) && (temp.SoulBoundId != Info.Index))
            {
                Enqueue(p);
                return;
            }

            /*
            if (Info.Equipment[to] != null)
                if (Info.Equipment[to].WeddingRing != -1)
                {
                    Enqueue(p);
                    return;
                }
                */

            if (CanEquipItem(temp, to))
            {
                if (temp.Info.NeedIdentify && !temp.Identified)
                {
                    temp.Identified = true;
                    Enqueue(new S.RefreshItem { Item = temp });
                }
                if ((temp.Info.Bind.HasFlag(BindMode.BindOnEquip)) && (temp.SoulBoundId == -1))
                {
                    temp.SoulBoundId = Info.Index;
                    Enqueue(new S.RefreshItem { Item = temp });
                }

                if ((Info.Equipment[to] != null) && (Info.Equipment[to].Cursed) && (UnlockCurse))
                    UnlockCurse = false;

                array[index] = Info.Equipment[to];

                Report.ItemMoved("RemoveItem", temp, MirGridType.Equipment, grid, to, index);

                Info.Equipment[to] = temp;

                Report.ItemMoved("EquipItem", temp, grid, MirGridType.Equipment, index, to);

                p.Success = true;
                Enqueue(p);
                RefreshStats();

                //Broadcast(GetUpdateInfo());
                return;
            }
            Enqueue(p);
        }

        private void UsePotion(UserItem item, S.UseItem p)
        {
            switch (item.Info.Shape)
            {
                case 0: //NormalPotion
                    PotHealthAmount = (ushort)Math.Min(ushort.MaxValue, PotHealthAmount + item.Info.HP);
                    PotManaAmount = (ushort)Math.Min(ushort.MaxValue, PotManaAmount + item.Info.MP);
                    break;
                case 1: //SunPotion
                    ChangeHP(item.Info.HP);
                    ChangeMP(item.Info.MP);
                    break;
                case 2: //MysteryWater
                    if (UnlockCurse)
                    {
                        ReceiveChat(Envir.Tr("You can already unequip a cursed item."), ChatType.Hint);
                        Enqueue(p);
                        return;
                    }
                    ReceiveChat(Envir.Tr("You can now unequip a cursed item."), ChatType.Hint);
                    UnlockCurse = true;
                    break;
                case 3: //Buff
                    int time = item.Info.Durability;

                    if ((item.Info.MaxDC + item.DC) > 0)
                        AddBuff(new Buff { Type = BuffType.Impact, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MaxDC + item.DC } });

                    if ((item.Info.MaxMC + item.MC) > 0)
                        AddBuff(new Buff { Type = BuffType.Magic, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MaxMC + item.MC } });

                    if ((item.Info.MaxSC + item.SC) > 0)
                        AddBuff(new Buff { Type = BuffType.Taoist, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MaxSC + item.SC } });

                    if ((item.Info.AttackSpeed + item.AttackSpeed) > 0)
                        AddBuff(new Buff { Type = BuffType.Storm, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.AttackSpeed + item.AttackSpeed } });

                    if ((item.Info.HP + item.HP) > 0)
                        AddBuff(new Buff { Type = BuffType.HealthAid, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.HP + item.HP } });

                    if ((item.Info.MP + item.MP) > 0)
                        AddBuff(new Buff { Type = BuffType.ManaAid, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MP + item.MP } });

                    if ((item.Info.MaxAC + item.AC) > 0)
                        AddBuff(new Buff { Type = BuffType.Defence, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MaxAC + item.AC } });

                    if ((item.Info.MaxMAC + item.MAC) > 0)
                        AddBuff(new Buff { Type = BuffType.MagicDefence, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.MaxMAC + item.MAC } });
                    break;
                case 4: //Exp
                    time = item.Info.Durability;

                    AddBuff(new Buff { Type = BuffType.Exp, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Luck + item.Luck } });
                    break;

                case 5: //
                    time = item.Info.Durability;

                    AddBuff(new Buff { Type = BuffType.Luck, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute,
                        Values = new int[] { item.Info.Luck, item.Info.MaxDC, item.Info.MaxMC, item.Info.MaxSC, item.Info.MaxAC, item.Info.Accuracy, item.Info.Agility, item.Info.HP} });
                    break;
            }
        }

        public void UseItem(ulong id)
        {
            S.UseItem p = new S.UseItem { UniqueID = id, Success = false };

            UserItem item = null;
            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                item = Info.Inventory[i];
                if (item == null || item.UniqueID != id) continue;
                index = i;
                break;
            }

            if (item == null || index == -1 || !CanUseItem(item))
            {
                Enqueue(p);
                return;
            }

            if (item.Count <= 0)
            {
                SMain.Enqueue(string.Format("Count:{0}", item.Count));
                Enqueue(p);
                return;
            }

            if (Dead && !(item.Info.Type == ItemType.Scroll && item.Info.Shape == 6))
            {
                Enqueue(p);
                return;
            }

            switch (item.Info.Type)
            {
                case ItemType.Potion:
                    UsePotion(item, p);
                    break;
                case ItemType.Scroll:
                    UserItem temp;
                    switch ((SpecialItemShape)item.Info.Shape)
                    {
                        case SpecialItemShape.TeleportEscape: //DE
                            if (!TeleportEscape(20))
                            {
                                Enqueue(p);
                                return;
                            }
                            break;
                        case SpecialItemShape.Teleport: //TT
                            if (!Teleport(Envir.GetMap(BindMapIndex, GetRegionID()), BindLocation))
                            {
                                Enqueue(p);
                                return;
                            }
                            break;
                        case SpecialItemShape.TeleportRandom: //RT
                            if (!TeleportRandom(200, item.Info.Durability))
                            {
                                Enqueue(p);
                                return;
                            }
                            break;
                        case SpecialItemShape.BenedictionOil: //BenedictionOil
                            if (!TryLuckWeapon())
                            {
                                Enqueue(p);
                                return;
                            }
                            break;
                        case SpecialItemShape.RepairOil: //RepairOil
                            temp = Info.Equipment[(int)EquipmentSlot.Weapon];
                            if (temp == null || temp.MaxDura == temp.CurrentDura)
                            {
                                Enqueue(p);
                                return;
                            }
                            if (temp.Info.Bind.HasFlag(BindMode.DontRepair))
                            {
                                Enqueue(p);
                                return;
                            }
                            temp.MaxDura = (ushort)Math.Max(0, temp.MaxDura - Math.Min(5000, temp.MaxDura - temp.CurrentDura) / 30);

                            temp.CurrentDura = (ushort)Math.Min(temp.MaxDura, temp.CurrentDura + 5000);
                            temp.DuraChanged = false;

                            ReceiveChat(Envir.Tr("Your weapon has been partially repaired"), ChatType.Hint);
                            Enqueue(new S.ItemRepaired { UniqueID = temp.UniqueID, MaxDura = temp.MaxDura, CurrentDura = temp.CurrentDura });
                            break;
                        case SpecialItemShape.WarGodOil: //WarGodOil
                            temp = Info.Equipment[(int)EquipmentSlot.Weapon];
                            if (temp == null || temp.MaxDura == temp.CurrentDura)
                            {
                                Enqueue(p);
                                return;
                            }
                            if (temp.Info.Bind.HasFlag(BindMode.DontRepair) || (temp.Info.Bind.HasFlag(BindMode.NoSRepair)))
                            {
                                Enqueue(p);
                                return;
                            }
                            temp.CurrentDura = temp.MaxDura;
                            temp.DuraChanged = false;

                            ReceiveChat(Envir.Tr("Your weapon has been completely repaired"), ChatType.Hint);
                            Enqueue(new S.ItemRepaired { UniqueID = temp.UniqueID, MaxDura = temp.MaxDura, CurrentDura = temp.CurrentDura });
                            break;
                        case SpecialItemShape.ResurrectionScroll: //ResurrectionScroll
                            if (Dead)
                            {
                                MP = MaxMP;
                                Revive(MaxHealth, true);
                            }
                            break;
                        case SpecialItemShape.CreditScroll: //CreditScroll
                            if (item.Info.Price > 0)
                            {
                                GainCredit(item.Info.Price);
                                ReceiveChat(String.Format(Envir.Tr("{0} Credits have been added to your Account"), item.Info.Price), ChatType.Hint);
                            }
                            break;
                        case SpecialItemShape.MapShoutScroll: //MapShoutScroll
                            HasMapShout = true;
                            ReceiveChat(Envir.Tr("You have been given one free shout across your current map"), ChatType.Hint);
                            break;
                        case SpecialItemShape.ServerShoutScroll://ServerShoutScroll
                            HasServerShout = true;
                            ReceiveChat(Envir.Tr("You have been given one free shout across the server"), ChatType.Hint);
                            break;
                        case SpecialItemShape.GuildSkillScroll://GuildSkillScroll
                            MyGuild.NewBuff(item.Info.Effect, false);
                            break;
                        case SpecialItemShape.HomeTeleport://HomeTeleport
                            if (MyGuild != null && MyGuild.Conquest != null && !MyGuild.Conquest.WarIsOn && MyGuild.Conquest.PalaceMap != null && !TeleportRandom(200, 0, MyGuild.Conquest.PalaceMap))
                            {
                                Enqueue(p);
                                return;
                            }
                            break;
                        case SpecialItemShape.LotteryTicket://LotteryTicket                                                                                    
                            if (Envir.Random.Next(item.Info.Effect * 32) == 1) // 1st prize : 1,000,000
                            {
                                ReceiveChat(Envir.Tr("You won 1st Prize! Received 1,000,000 gold"), ChatType.Hint);
                                GainGold(1000000);
                            }
                            else if (Envir.Random.Next(item.Info.Effect * 16) == 1)  // 2nd prize : 200,000
                            {
                                ReceiveChat(Envir.Tr("You won 2nd Prize! Received 200,000 gold"), ChatType.Hint);
                                GainGold(200000);
                            }
                            else if (Envir.Random.Next(item.Info.Effect * 8) == 1)  // 3rd prize : 100,000
                            {
                                ReceiveChat(Envir.Tr("You won 3rd Prize! Received 100,000 gold"), ChatType.Hint);
                                GainGold(100000);
                            }
                            else if (Envir.Random.Next(item.Info.Effect * 4) == 1) // 4th prize : 10,000
                            {
                                ReceiveChat(Envir.Tr("You won 4th Prize! Received 10,000 gold"), ChatType.Hint);
                                GainGold(10000);
                            }
                            else if (Envir.Random.Next(item.Info.Effect * 2) == 1)  // 5th prize : 1,000
                            {
                                ReceiveChat(Envir.Tr("You won 5th Prize! Received 1,000 gold"), ChatType.Hint);
                                GainGold(1000);
                            }
                            else if (Envir.Random.Next(item.Info.Effect) == 1)  // 6th prize 500
                            {
                                ReceiveChat(Envir.Tr("You won 6th Prize! Received 500 gold"), ChatType.Hint);
                                GainGold(500);
                            }
                            else
                            {
                                ReceiveChat(Envir.Tr("You haven't won anything."), ChatType.Hint);
                            }
                            break;
                        case SpecialItemShape.RepairAllOil:
                            {
                                for (int i = 0; i < Info.Equipment.Length; ++i)
                                {
                                    temp = Info.Equipment[i];
                                    if (temp == null || temp.MaxDura == temp.CurrentDura)
                                        continue;

                                    if (temp.Info.Bind.HasFlag(BindMode.DontRepair))
                                        continue;

                                    temp.CurrentDura = temp.MaxDura;
                                    temp.DuraChanged = false;

                                    Enqueue(new S.ItemRepaired { UniqueID = temp.UniqueID, MaxDura = temp.MaxDura, CurrentDura = temp.CurrentDura });
                                }
                                ReceiveChat(Envir.Tr("Your equipment has been partially repaired"), ChatType.Hint);
                            }
                            break;

                        case SpecialItemShape.SuperBenedictionOil:
                            LuckWeapon();
                            break;
                    }
                    break;
                case ItemType.Book:
                    UserMagic magic = new UserMagic((Spell)item.Info.Shape);

                    if (magic.Info == null)
                    {
                        Enqueue(p);
                        return;
                    }

                    Info.Magics.Add(magic);
                    Enqueue(magic.GetInfo());
                    RefreshStats();
                    break;
                case ItemType.Script:
                    CallDefaultNPC(DefaultNPCType.UseItem, item.Info.Shape);
                    break;
                case ItemType.Food:
                    temp = Info.Equipment[(int)EquipmentSlot.Mount];
                    if (temp == null || temp.MaxDura == temp.CurrentDura)
                    {
                        Enqueue(p);
                        return;
                    }

                    switch (item.Info.Shape)
                    {
                        case 0:
                            temp.MaxDura = (ushort)Math.Max(0, temp.MaxDura - Math.Min(1000, temp.MaxDura - (temp.CurrentDura / 30)));
                            break;
                        case 1:
                            break;
                    }

                    temp.CurrentDura = (ushort)Math.Min(temp.MaxDura, temp.CurrentDura + item.CurrentDura);
                    temp.DuraChanged = false;

                    ReceiveChat(Envir.Tr("Your mount has been fed."), ChatType.Hint);
                    Enqueue(new S.ItemRepaired { UniqueID = temp.UniqueID, MaxDura = temp.MaxDura, CurrentDura = temp.CurrentDura });

                    RefreshStats();
                    break;
                case ItemType.Pets:
                    if (item.Info.Shape >= 20)
                    {
                        switch (item.Info.Shape)
                        {
                            case 20://Mirror
                                Enqueue(new S.IntelligentCreatureEnableRename());
                                break;
                            case 21://BlackStone
                                if (item.Count > 1) item.Count--;
                                else Info.Inventory[index] = null;
                                RefreshBagWeight();
                                p.Success = true;
                                Enqueue(p);
                                BlackstoneRewardItem();
                                return;
                            case 22://Nuts
                                if (CreatureSummoned)
                                    for (int i = 0; i < Pets.Count; i++)
                                    {
                                        if (Pets[i].Info.AI != 64) continue;
                                        if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;
                                        ((IntelligentCreatureObject)Pets[i]).maintainfoodTime = item.Info.Effect * Settings.Hour / 1000;
                                        UpdateCreatureMaintainFoodTime(SummonedCreatureType, 0);
                                        GetCreaturesInfo();
                                        break;
                                    }

                                break;
                            case 23://FairyMoss, FreshwaterClam, Mackerel, Cherry
                                if (CreatureSummoned)
                                    for (int i = 0; i < Pets.Count; i++)
                                    {
                                        if (Pets[i].Info.AI != 64) continue;
                                        if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;
                                        if (((IntelligentCreatureObject)Pets[i]).Fullness < 10000)
                                        {
                                            ((IntelligentCreatureObject)Pets[i]).IncreaseFullness(item.Info.Effect * 100);
                                        GetCreaturesInfo();
                                        }
                                        break;
                                    }
                                break;
                            case 24://WonderPill
                                if (CreatureSummoned)
                                    for (int i = 0; i < Pets.Count; i++)
                                    {
                                        if (Pets[i].Info.AI != 64) continue;
                                        if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;
                                        if (((IntelligentCreatureObject)Pets[i]).Fullness == 0)
                                        {
                                            ((IntelligentCreatureObject)Pets[i]).IncreaseFullness(100);
                                        GetCreaturesInfo();
                                        }
                                        break;
                                    }
                                break;
                            case 25://Strongbox
                                byte boxtype = item.Info.Effect;
                                if (item.Count > 1) item.Count--;
                                else Info.Inventory[index] = null;
                                RefreshBagWeight();
                                p.Success = true;
                                Enqueue(p);
                                StrongboxRewardItem(boxtype);
                                return;
                            case 26://Wonderdrugs
                                int time = item.Info.Durability;
                                switch (item.Info.Effect)
                                {
                                    case 0://exp low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.Luck + item.Luck } });
                                        break;
                                    case 1://drop low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.Luck + item.Luck } });
                                        break;
                                    case 2://hp low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.HP + item.HP } });
                                        break;
                                    case 3://mp low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.MP + item.MP } });
                                        break;
                                    case 4://ac-ac low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.MaxAC + item.AC } });
                                        break;
                                    case 5://mac-mac low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.MaxMAC + item.MAC } });
                                        break;
                                    case 6://speed low/med/high
                                        AddBuff(new Buff { Type = BuffType.WonderDrug, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Effect, item.Info.AttackSpeed + item.AttackSpeed } });
                                        break;
                                }
                                break;
                            case 27://FortuneCookies
                                break;
                            case 28://Knapsack
                                time = item.Info.Durability;
                                AddBuff(new Buff { Type = BuffType.Knapsack, Caster = this, ExpireTime = Envir.Time + time * Settings.Minute, Values = new int[] { item.Info.Luck + item.Luck } });
                                break;
                        }
                    }
                    else
                    {
                        int slotIndex = Info.IntelligentCreatures.Count;
                        UserIntelligentCreature petInfo = new UserIntelligentCreature((IntelligentCreatureType)item.Info.Shape, slotIndex, item.Info.Effect);
                        if (Info.CheckHasIntelligentCreature((IntelligentCreatureType)item.Info.Shape))
                        {
                            ReceiveChat(Envir.Tr("You already have this creature."), ChatType.Hint);
                            petInfo = null;
                        }

                        if (petInfo == null || slotIndex >= 10)
                        {
                            Enqueue(p);
                            return;
                        }

                        ReceiveChat(Envir.Tr("Obtained a new creature {" + petInfo.CustomName + "}."), ChatType.Hint);

                        Info.IntelligentCreatures.Add(petInfo);
                        Enqueue(petInfo.GetInfo());
                    }
                    break;
                default:
                    return;
            }

            if (item.Count > 1) item.Count--;
            else Info.Inventory[index] = null;
            RefreshBagWeight();

            Report.ItemChanged("UseItem", item, 1, 1);

            p.Success = true;
            Enqueue(p);
        }
        public void SplitItem(MirGridType grid, ulong id, uint count)
        {
            S.SplitItem1 p = new S.SplitItem1 { Grid = grid, UniqueID = id, Count = count, Success = false };
            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    array = Account.Storage;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            UserItem temp = null;


            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null || array[i].UniqueID != id) continue;
                temp = array[i];
                break;
            }

            if (temp == null || count >= temp.Count || count <= 0 || FreeSpace(array) == 0)
            {
                Enqueue(p);
                return;
            }

            temp.Count -= count;

            temp = Envir.CreateFreshItem(temp.Info);
            temp.Count = count;

            p.Success = true;
            Enqueue(p);
            Enqueue(new S.SplitItem { Item = temp, Grid = grid });

            if (grid == MirGridType.Inventory && (temp.Info.Type == ItemType.Potion || temp.Info.Type == ItemType.Scroll || temp.Info.Type == ItemType.Amulet || (temp.Info.Type == ItemType.Script && temp.Info.Effect == 1)))
            {
                if (temp.Info.Type == ItemType.Potion || temp.Info.Type == ItemType.Scroll || (temp.Info.Type == ItemType.Script && temp.Info.Effect == 1))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (array[i] != null) continue;
                        array[i] = temp;
                        RefreshBagWeight();
                        return;
                    }
                }
                else if (temp.Info.Type == ItemType.Amulet)
                {
                    for (int i = 4; i < 6; i++)
                    {
                        if (array[i] != null) continue;
                        array[i] = temp;
                        RefreshBagWeight();
                        return;
                    }
                }
            }

            for (int i = 6; i < array.Length; i++)
            {
                if (array[i] != null) continue;
                array[i] = temp;
                RefreshBagWeight();
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                if (array[i] != null) continue;
                array[i] = temp;
                RefreshBagWeight();
                return;
            }
        }

        public void MergeItem(MirGridType gridFrom, MirGridType gridTo, ulong fromID, ulong toID)
        {
            S.MergeItem p = new S.MergeItem { GridFrom = gridFrom, GridTo = gridTo, IDFrom = fromID, IDTo = toID, Success = false };

            UserItem[] arrayFrom;

            switch (gridFrom)
            {
                case MirGridType.Inventory:
                    arrayFrom = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    arrayFrom = Account.Storage;
                    break;
                case MirGridType.Equipment:
                    arrayFrom = Info.Equipment;
                    break;
                case MirGridType.Fishing:
                    if (Info.Equipment[(int)EquipmentSlot.Weapon] == null || (Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 49 && Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 50))
                    {
                        Enqueue(p);
                        return;
                    }
                    arrayFrom = Info.Equipment[(int)EquipmentSlot.Weapon].Slots;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            UserItem[] arrayTo;
            switch (gridTo)
            {
                case MirGridType.Inventory:
                    arrayTo = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.StorageKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Enqueue(p);
                        return;
                    }
                    NPCObject ob = null;
                    for (int i = 0; i < CurrentMap.NPCs.Count; i++)
                    {
                        if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                        ob = CurrentMap.NPCs[i];
                        break;
                    }

                    if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
                    {
                        Enqueue(p);
                        return;
                    }
                    arrayTo = Account.Storage;
                    break;
                case MirGridType.Equipment:
                    arrayTo = Info.Equipment;
                    break;
                case MirGridType.Fishing:
                    if (Info.Equipment[(int)EquipmentSlot.Weapon] == null || (Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 49 && Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 50))
                    {
                        Enqueue(p);
                        return;
                    }
                    arrayTo = Info.Equipment[(int)EquipmentSlot.Weapon].Slots;
                    break;
                default:
                    Enqueue(p);
                    return;
            }

            UserItem tempFrom = null;
            int index = -1;

            for (int i = 0; i < arrayFrom.Length; i++)
            {
                if (arrayFrom[i] == null || arrayFrom[i].UniqueID != fromID) continue;
                index = i;
                tempFrom = arrayFrom[i];
                break;
            }

            if (tempFrom == null || tempFrom.Info.StackSize == 1 || index == -1)
            {
                Enqueue(p);
                return;
            }


            UserItem tempTo = null;

            for (int i = 0; i < arrayTo.Length; i++)
            {
                if (arrayTo[i] == null || arrayTo[i].UniqueID != toID) continue;
                tempTo = arrayTo[i];
                break;
            }

            if (tempTo == null || tempTo.Info != tempFrom.Info || tempTo.Count == tempTo.Info.StackSize)
            {
                Enqueue(p);
                return;
            }

            if (tempTo.Info.Type != ItemType.Amulet && (gridFrom == MirGridType.Equipment || gridTo == MirGridType.Equipment))
            {
                Enqueue(p);
                return;
            }

            if (tempTo.Info.Type != ItemType.Bait && (gridFrom == MirGridType.Fishing || gridTo == MirGridType.Fishing))
            {
                Enqueue(p);
                return;
            }

            if (tempFrom.Count <= tempTo.Info.StackSize - tempTo.Count)
            {
                tempTo.Count += tempFrom.Count;
                arrayFrom[index] = null;
            }
            else
            {
                tempFrom.Count -= tempTo.Info.StackSize - tempTo.Count;
                tempTo.Count = tempTo.Info.StackSize;
            }

            UpdateQuestItem(tempFrom.Info);

            TradeUnlock();

            p.Success = true;
            Enqueue(p);
            RefreshStats();
        }
        public void CombineItem(ulong fromID, ulong toID)
        {
            S.CombineItem p = new S.CombineItem { IDFrom = fromID, IDTo = toID, Success = false };

            UserItem[] array = Info.Inventory;
            UserItem tempFrom = null;
            UserItem tempTo = null;
            int indexFrom = -1;
            int indexTo = -1;

            if (Dead)
            {
                Enqueue(p);
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null || array[i].UniqueID != fromID) continue;
                indexFrom = i;
                tempFrom = array[i];
                break;
            }

            if (tempFrom == null || indexFrom == -1)
            {
                Enqueue(p);
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null || array[i].UniqueID != toID) continue;
                indexTo = i;
                tempTo = array[i];
                break;
            }

            if (tempTo == null || indexTo == -1)
            {
                Enqueue(p);
                return;
            }

            if ((byte)tempTo.Info.Type < 1 || (byte)tempTo.Info.Type > 11)
            {
                Enqueue(p);
                return;
            }

            bool canRepair = false, canUpgrade = false;

            if (tempFrom.Info.Type != ItemType.Gem)
            {
                Enqueue(p);
                return;
            }

            switch (tempFrom.Info.Shape)
            {
                case 1: //BoneHammer
                case 2: //SewingSupplies
                case 5: //SpecialHammer
                case 6: //SpecialSewingSupplies

                    if (tempTo.Info.Bind.HasFlag(BindMode.DontRepair))
                    {
                        Enqueue(p);
                        return;
                    }

                    switch (tempTo.Info.Type)
                    {
                        case ItemType.Weapon:
                        case ItemType.Necklace:
                        case ItemType.Ring:
                        case ItemType.Bracelet:
                            if (tempFrom.Info.Shape == 1 || tempFrom.Info.Shape == 5)
                                canRepair = true;
                            break;
                        case ItemType.Armour:
                        case ItemType.Helmet:
                        case ItemType.Boots:
                        case ItemType.Belt:
                            if (tempFrom.Info.Shape == 2 || tempFrom.Info.Shape == 6)
                                canRepair = true;
                            break;
                        default:
                            canRepair = false;
                            break;
                    }

                    if (canRepair != true)
                    {
                        Enqueue(p);
                        return;
                    }

                    if (tempTo.CurrentDura == tempTo.MaxDura)
                    {
                        ReceiveChat(Envir.Tr("Item does not need to be repaired."), ChatType.Hint);
                        Enqueue(p);
                        return;
                    }
                    break;
                case 3: //gems
                case 4: //orbs
                    if (tempTo.Info.Bind.HasFlag(BindMode.DontUpgrade) || tempTo.Info.Unique != SpecialItemMode.None)
                    {
                        Enqueue(p);
                        return;
                    }

                    if (tempTo.RentalInformation != null && tempTo.RentalInformation.BindingFlags.HasFlag(BindMode.DontUpgrade))
                    {
                        Enqueue(p);
                        return;
                    }

                    if ((tempTo.GemCount >= tempFrom.Info.CriticalDamage) || (GetCurrentStatCount(tempFrom, tempTo) >= tempFrom.Info.HpDrainRate))
                    {
                        ReceiveChat(Envir.Tr("Item has already reached maximum added stats"), ChatType.Hint);
                        Enqueue(p);
                        return;
                    }

                    int successchance = tempFrom.Info.Reflect;

                    // Gem is only affected by the stat applied.
                    // Drop rate per gem won't work if gems add more than 1 stat, i.e. DC + 2 per gem.
                    if (Settings.GemStatIndependent)
                    {
                        StatType GemType = GetGemType(tempFrom);

                        switch (GemType)
                        {
                            case StatType.AC:
                                successchance *= (int)tempTo.AC;
                                break;

                            case StatType.MAC:
                                successchance *= (int)tempTo.MAC;
                                break;

                            case StatType.DC:
                                successchance *= (int)tempTo.DC;
                                break;

                            case StatType.MC:
                                successchance *= (int)tempTo.MC;
                                break;

                            case StatType.SC:
                                successchance *= (int)tempTo.SC;
                                break;

                            case StatType.ASpeed:
                                successchance *= (int)tempTo.AttackSpeed;
                                break;

                            case StatType.Accuracy:
                                successchance *= (int)tempTo.Accuracy;
                                break;

                            case StatType.Agility:
                                successchance *= (int)tempTo.Agility;
                                break;

                            case StatType.Freezing:
                                successchance *= (int)tempTo.Freezing;
                                break;

                            case StatType.PoisonAttack:
                                successchance *= (int)tempTo.PoisonAttack;
                                break;

                            case StatType.MagicResist:
                                successchance *= (int)tempTo.MagicResist;
                                break;

                            case StatType.PoisonResist:
                                successchance *= (int)tempTo.PoisonResist;
                                break;

                            // These attributes may not work as more than 1 stat is
                            // added per gem, i.e + 40 HP.

                            case StatType.HP:
                                successchance *= (int)tempTo.HP;
                                break;

                            case StatType.MP:
                                successchance *= (int)tempTo.MP;
                                break;

                            case StatType.HP_Regen:
                                successchance *= (int)tempTo.HealthRecovery;
                                break;

                            // I don't know if this conflicts with benes.
                            case StatType.Luck:
                                successchance *= (int)tempTo.Luck;
                                break;

                            case StatType.Strong:
                                successchance *= (int)tempTo.Strong;
                                break;

                            case StatType.PoisonRegen:
                                successchance *= (int)tempTo.PoisonRecovery;
                                break;


                            /*
                                 Currently not supported.
                                 Missing item definitions.

                                 case StatType.HP_Precent:
                                 case StatType.MP_Precent:
                                 case StatType.MP_Regen:
                                 case StatType.Holy:
                                 case StatType.Durability:


                            */
                            default:
                                successchance *= (int)tempTo.GemCount;
                                break;

                        }
                    }
                    // Gem is affected by the total added stats on the item.
                    else
                    {
                        successchance *= (int)tempTo.GemCount;
                    }

                    successchance = successchance >= tempFrom.Info.CriticalRate ? 0 : (tempFrom.Info.CriticalRate - successchance) + (GemRate * 5);

                    //check if combine will succeed
                    bool succeeded = Envir.Random.Next(100) < successchance;
                    canUpgrade = true;

                    byte itemType = (byte)tempTo.Info.Type;

                    if (!ValidGemForItem(tempFrom, itemType))
                    {
                        ReceiveChat(Envir.Tr("Invalid combination"), ChatType.Hint);
                        Enqueue(p);
                        return;
                    }

                    if ((tempFrom.Info.MaxDC + tempFrom.DC) > 0)
                    {
                        if (succeeded) tempTo.DC = (byte)Math.Min(byte.MaxValue, tempTo.DC + tempFrom.Info.MaxDC + tempFrom.DC);
                    }

                    else if ((tempFrom.Info.MaxMC + tempFrom.MC) > 0)
                    {
                        if (succeeded) tempTo.MC = (byte)Math.Min(byte.MaxValue, tempTo.MC + tempFrom.Info.MaxMC + tempFrom.MC);
                    }

                    else if ((tempFrom.Info.MaxSC + tempFrom.SC) > 0)
                    {
                        if (succeeded) tempTo.SC = (byte)Math.Min(byte.MaxValue, tempTo.SC + tempFrom.Info.MaxSC + tempFrom.SC);
                    }

                    else if ((tempFrom.Info.MaxAC + tempFrom.AC) > 0)
                    {
                        if (succeeded) tempTo.AC = (byte)Math.Min(byte.MaxValue, tempTo.AC + tempFrom.Info.MaxAC + tempFrom.AC);
                    }

                    else if ((tempFrom.Info.MaxMAC + tempFrom.MAC) > 0)
                    {
                        if (succeeded) tempTo.MAC = (byte)Math.Min(byte.MaxValue, tempTo.MAC + tempFrom.Info.MaxMAC + tempFrom.MAC);
                    }

                    else if ((tempFrom.Info.Durability) > 0)
                    {
                        if (succeeded) tempTo.MaxDura = (ushort)Math.Min(ushort.MaxValue, tempTo.MaxDura + tempFrom.MaxDura);
                    }

                    else if ((tempFrom.Info.AttackSpeed + tempFrom.AttackSpeed) > 0)
                    {
                        if (succeeded) tempTo.AttackSpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, tempTo.AttackSpeed + tempFrom.Info.AttackSpeed + tempFrom.AttackSpeed)));
                    }

                    else if ((tempFrom.Info.Agility + tempFrom.Agility) > 0)
                    {
                        if (succeeded) tempTo.Agility = (byte)Math.Min(byte.MaxValue, tempFrom.Info.Agility + tempTo.Agility + tempFrom.Agility);
                    }

                    else if ((tempFrom.Info.Accuracy + tempFrom.Accuracy) > 0)
                    {
                        if (succeeded) tempTo.Accuracy = (byte)Math.Min(byte.MaxValue, tempFrom.Info.Accuracy + tempTo.Accuracy + tempFrom.Accuracy);
                    }

                    else if ((tempFrom.Info.PoisonAttack + tempFrom.PoisonAttack) > 0)
                    {
                        if (succeeded) tempTo.PoisonAttack = (byte)Math.Min(byte.MaxValue, tempFrom.Info.PoisonAttack + tempTo.PoisonAttack + tempFrom.PoisonAttack);
                    }

                    else if ((tempFrom.Info.Freezing + tempFrom.Freezing) > 0)
                    {
                        if (succeeded) tempTo.Freezing = (byte)Math.Min(byte.MaxValue, tempFrom.Info.Freezing + tempTo.Freezing + tempFrom.Freezing);
                    }

                    else if ((tempFrom.Info.MagicResist + tempFrom.MagicResist) > 0)
                    {
                        if (succeeded) tempTo.MagicResist = (byte)Math.Min(byte.MaxValue, tempFrom.Info.MagicResist + tempTo.MagicResist + tempFrom.MagicResist);
                    }

                    else if ((tempFrom.Info.PoisonResist + tempFrom.PoisonResist) > 0)
                    {
                        if (succeeded) tempTo.PoisonResist = (byte)Math.Min(byte.MaxValue, tempFrom.Info.PoisonResist + tempTo.PoisonResist + tempFrom.PoisonResist);
                    }
                    else if ((tempFrom.Info.Luck + tempFrom.Luck) > 0)
                    {
                        if (succeeded) tempTo.Luck = (sbyte)Math.Min(sbyte.MaxValue, tempFrom.Info.Luck + tempTo.Luck + tempFrom.Luck);
                    }
                    else
                    {
                        ReceiveChat(Envir.Tr("Cannot combine these items."), ChatType.Hint);
                        Enqueue(p);
                        return;
                    }

                    if (!succeeded)
                    {

                        if ((tempFrom.Info.Shape == 3) && (Envir.Random.Next(15) < 3))
                        {
                            //item destroyed
                            ReceiveChat(Envir.Tr("Item has been destroyed."), ChatType.Hint);
                            Report.ItemChanged(Envir.Tr("CombineItem (Item Destroyed)"), Info.Inventory[indexTo], 1, 1);

                            Info.Inventory[indexTo] = null;
                            p.Destroy = true;
                        }
                        else
                        {
                            //upgrade has no effect
                            ReceiveChat(Envir.Tr("Upgrade has no effect."), ChatType.Hint);
                        }

                        canUpgrade = false;
                    }
                    break;
                default:
                    Enqueue(p);
                    return;
            }



            RefreshBagWeight();

            if (canRepair && Info.Inventory[indexTo] != null)
            {
                switch (tempTo.Info.Shape)
                {
                    case 1:
                    case 2:
                        {
                            tempTo.MaxDura = (ushort)Math.Max(0, Math.Min(tempTo.MaxDura, tempTo.MaxDura - 100 * Envir.Random.Next(10)));
                        }
                        break;
                    default:
                        break;
                }
                tempTo.CurrentDura = tempTo.MaxDura;
                tempTo.DuraChanged = false;

                ReceiveChat(Envir.Tr("Item has been repaired."), ChatType.Hint);
                Enqueue(new S.ItemRepaired { UniqueID = tempTo.UniqueID, MaxDura = tempTo.MaxDura, CurrentDura = tempTo.CurrentDura });
            }

            if (canUpgrade && Info.Inventory[indexTo] != null)
            {
                tempTo.GemCount++;
                ReceiveChat(Envir.Tr("Item has been upgraded."), ChatType.Hint);
                Enqueue(new S.ItemUpgraded { Item = tempTo });
            }

            if (tempFrom.Count > 1) tempFrom.Count--;
            else Info.Inventory[indexFrom] = null;

            Report.ItemCombined("CombineItem", tempFrom, tempTo, indexFrom, indexTo, MirGridType.Inventory);

            //item merged ok
            TradeUnlock();

            p.Success = true;
            Enqueue(p);
        }
        private bool ValidGemForItem(UserItem Gem, byte itemtype)
        {
            switch (itemtype)
            {
                case 1: //weapon
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Paralize))
                        return true;
                    break;
                case 2: //Armour
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Teleport))
                        return true;
                    break;
                case 4: //Helmet
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Clearring))
                        return true;
                    break;
                case 5: //necklace
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Protection))
                        return true;
                    break;
                case 6: //bracelet
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Revival))
                        return true;
                    break;
                case 7: //ring
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Muscle))
                        return true;
                    break;
                case 8: //amulet
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Flame))
                        return true;
                    break;
                case 9://belt
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Healing))
                        return true;
                    break;
                case 10: //boots
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Probe))
                        return true;
                    break;
                case 11: //stone
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.Skill))
                        return true;
                    break;
                case 12:///torch
                    if (Gem.Info.Unique.HasFlag(SpecialItemMode.NoDuraLoss))
                        return true;
                    break;
            }
            return false;
        }
        //Gems granting multiple stat types are not compatiable with this method.
        private StatType GetGemType(UserItem gem)
        {
            if ((gem.Info.MaxDC + gem.DC) > 0)
                return StatType.DC;

            else if ((gem.Info.MaxMC + gem.MC) > 0)
                return StatType.MC;

            else if ((gem.Info.MaxSC + gem.SC) > 0)
                return StatType.SC;

            else if ((gem.Info.MaxAC + gem.AC) > 0)
                return StatType.AC;

            else if ((gem.Info.MaxMAC + gem.MAC) > 0)
                return StatType.MAC;

            else if ((gem.Info.Durability) > 0)
                return StatType.Durability;

            else if ((gem.Info.AttackSpeed + gem.AttackSpeed) > 0)
                return StatType.ASpeed;

            else if ((gem.Info.Agility + gem.Agility) > 0)
                return StatType.Agility;

            else if ((gem.Info.Accuracy + gem.Accuracy) > 0)
                return StatType.Accuracy;

            else if ((gem.Info.PoisonAttack + gem.PoisonAttack) > 0)
                return StatType.PoisonAttack;

            else if ((gem.Info.Freezing + gem.Freezing) > 0)
                return StatType.Freezing;

            else if ((gem.Info.MagicResist + gem.MagicResist) > 0)
                return StatType.MagicResist;

            else if ((gem.Info.PoisonResist + gem.PoisonResist) > 0)
                return StatType.PoisonResist;

            else if ((gem.Info.Luck + gem.Luck) > 0)
                return StatType.Luck;

            else if ((gem.Info.PoisonRecovery + gem.PoisonRecovery) > 0)
                return StatType.PoisonRegen;

            else if ((gem.Info.HP + gem.HP) > 0)
                return StatType.HP;

            else if ((gem.Info.MP + gem.MP) > 0)
                return StatType.MP;

            else if ((gem.Info.HealthRecovery + gem.HealthRecovery) > 0)
                return StatType.HP_Regen;

            // These may be incomplete. Item definitions may be missing?

            else if ((gem.Info.HPrate) > 0)
                return StatType.HP_Percent;

            else if ((gem.Info.MPrate) > 0)
                return StatType.MP_Percent;

            else if ((gem.Info.SpellRecovery) > 0)
                return StatType.MP_Regen;

            else if ((gem.Info.Holy) > 0)
                return StatType.Holy;

            else if ((gem.Info.Strong + gem.Strong) > 0)
                return StatType.Strong;

            else if (gem.Info.HPrate > 0)
                return StatType.HP_Regen;

            else
                return StatType.Unknown;
        }
        //Gems granting multiple stat types are not compatible with this method.
        private int GetCurrentStatCount(UserItem gem, UserItem item)
        {
            if ((gem.Info.MaxDC + gem.DC) > 0)
                return item.DC;

            else if ((gem.Info.MaxMC + gem.MC) > 0)
                return item.MC;

            else if ((gem.Info.MaxSC + gem.SC) > 0)
                return item.SC;

            else if ((gem.Info.MaxAC + gem.AC) > 0)
                return item.AC;

            else if ((gem.Info.MaxMAC + gem.MAC) > 0)
                return item.MAC;

            else if ((gem.Info.Durability) > 0)
                return item.Info.Durability > item.MaxDura ? 0 : ((item.MaxDura - item.Info.Durability) / 1000);

            else if ((gem.Info.AttackSpeed + gem.AttackSpeed) > 0)
                return item.AttackSpeed;

            else if ((gem.Info.Agility + gem.Agility) > 0)
                return item.Agility;

            else if ((gem.Info.Accuracy + gem.Accuracy) > 0)
                return item.Accuracy;

            else if ((gem.Info.PoisonAttack + gem.PoisonAttack) > 0)
                return item.PoisonAttack;

            else if ((gem.Info.Freezing + gem.Freezing) > 0)
                return item.Freezing;

            else if ((gem.Info.MagicResist + gem.MagicResist) > 0)
                return item.MagicResist;

            else if ((gem.Info.PoisonResist + gem.PoisonResist) > 0)
                return item.PoisonResist;

            else if ((gem.Info.Luck + gem.Luck) > 0)
                return item.Luck;

            else if ((gem.Info.PoisonRecovery + gem.PoisonRecovery) > 0)
                return item.PoisonRecovery;

            else if ((gem.Info.HP + gem.HP) > 0)
                return item.HP;

            else if ((gem.Info.MP + gem.MP) > 0)
                return item.MP;

            else if ((gem.Info.HealthRecovery + gem.HealthRecovery) > 0)
                return item.HealthRecovery;

            // Definitions are missing for these.
            /*
            else if ((gem.Info.HPrate) > 0)
                return item.h

            else if ((gem.Info.MPrate) > 0)
                return 

            else if ((gem.Info.SpellRecovery) > 0)
                return 

            else if ((gem.Info.Holy) > 0)
                return 

            else if ((gem.Info.Strong + gem.Strong) > 0)
                return 

            else if (gem.Info.HPrate > 0)
                return
            */
            return 0;
        }
        public void DropItem(ulong id, uint count)
        {
            S.DropItem p = new S.DropItem { UniqueID = id, Count = count, Success = false };
            if (Dead)
            {
                Enqueue(p);
                return;
            }

            if (CurrentMap.Info.NoThrowItem)
            {
                ReceiveChat(Envir.Tr("You cannot drop items on this map"), ChatType.System);
                Enqueue(p);
                return;
            }

            UserItem temp = null;
            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                temp = Info.Inventory[i];
                if (temp == null || temp.UniqueID != id) continue;
                index = i;
                break;
            }

            if (temp == null || index == -1 || count > temp.Count)
            {
                Enqueue(p);
                return;
            }

            if (temp.Info.Bind.HasFlag(BindMode.DontDrop))
            {
                Enqueue(p);
                return;
            }

            if (temp.RentalInformation != null && temp.RentalInformation.BindingFlags.HasFlag(BindMode.DontDrop))
            {
                Enqueue(p);
                return;
            }

            if (temp.Count == count)
            {
                if (!temp.Info.Bind.HasFlag(BindMode.DestroyOnDrop))
                    if (!DropItem(temp))
                    {
                        Enqueue(p);
                        return;
                    }
                Info.Inventory[index] = null;
                UpdateQuestItem(temp.Info);
            }
            else
            {
                UserItem temp2 = Envir.CreateFreshItem(temp.Info);
                temp2.Count = count;
                if (!temp.Info.Bind.HasFlag(BindMode.DestroyOnDrop))
                    if (!DropItem(temp2))
                    {
                        Enqueue(p);
                        return;
                    }
                temp.Count -= count;
                UpdateQuestItem(temp2.Info);
            }
            p.Success = true;
            Enqueue(p);
            RefreshBagWeight();

            Report.ItemChanged("DropItem", temp, count, 1);
        }
        public void DropGold(uint gold)
        {
            if (Account.Gold < gold) return;

            ItemObject ob = new ItemObject(this, gold);

            if (!ob.Drop(5)) return;
            Account.Gold -= gold;
            Enqueue(new S.LoseGold { Gold = gold });
        }
        public void PickUp()
        {
            if (Dead)
            {
                //Send Fail
                return;
            }

            if (!SMain.Envir.CheckSameServer(GetRegionID(), CurrentMap.Region))
            {
                SMain.EnqueueDebugging(string.Format("no same region {0}", Name));
                return;
            }

            var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);

            bool sendFail = false;
            if (cellObjects == null)
                return;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                MapObject ob = cellObjects[i];

                if (ob.Race != ObjectType.Item) continue;

                if (ob.Owner != null && ob.Owner != this && !IsGroupMember(ob.Owner)) //Or Group member.
                {
                    sendFail = true;
                    continue;
                }
                ItemObject item = (ItemObject)ob;

                if (item.Item != null)
                {
                    if (!CanGainItem(item.Item)) continue;

                    if (item.Item.Info.ShowGroupPickup && IsGroupMember(this))
                        for (int j = 0; j < GroupMembers.Count; j++)
                            GroupMembers[j].ReceiveChat(Envir.Format("{0} Picked up: {1}", Name, item.Item.Name),
                                ChatType.System);

                    GainItem(item.Item);

                    Report.ItemChanged("PickUpItem", item.Item, item.Item.Count, 2);

                    CurrentMap.RemoveObject(ob);
                    ob.Despawn();

                    return;
                }

                if (!CanGainGold(item.Gold)) continue;

                GainGold(item.Gold);
                CurrentMap.RemoveObject(ob);
                ob.Despawn();
                return;
            }

            if (sendFail)
                ReceiveChat(Envir.Tr("Can not pick up, You do not own this item."), ChatType.System);

        }

        private bool IsGroupMember(MapObject player)
        {
            if (player.Race != ObjectType.Player) return false;
            return GroupMembers != null && GroupMembers.Contains(player);
        }

        public override bool CanGainGold(uint gold)
        {
            return (UInt64)gold + Account.Gold <= uint.MaxValue;
        }
        public override void WinGold(uint gold)
        {
            if (GroupMembers == null)
            {
                GainGold(gold);
                return;
            }

            uint count = 0;

            for (int i = 0; i < GroupMembers.Count; i++)
            {
                PlayerObject player = GroupMembers[i];
                if (player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                    count++;
            }

            if (count == 0 || count > gold)
            {
                GainGold(gold);
                return;
            }
            gold = gold / count;

            for (int i = 0; i < GroupMembers.Count; i++)
            {
                PlayerObject player = GroupMembers[i];
                if (player.CurrentMap == CurrentMap && Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) && !player.Dead)
                    player.GainGold(gold);
            }
        }
        public void GainGold(uint gold)
        {
            if (gold == 0) return;

            if (((UInt64)Account.Gold + gold) > uint.MaxValue)
                gold = uint.MaxValue - Account.Gold;

            Account.Gold += gold;

            Enqueue(new S.GainedGold { Gold = gold });
        }
        public void GainCredit(uint credit)
        {
            if (credit == 0) return;

            if (((UInt64)Account.Credit + credit) > uint.MaxValue)
                credit = uint.MaxValue - Account.Credit;

            Account.Credit += credit;

            Enqueue(new S.GainedCredit { Credit = credit });
        }

        public bool CanGainItem(UserItem item, bool useWeight = true)
        {
            if (item.Info.Type == ItemType.Amulet)
            {
                if (FreeSpace(Info.Inventory) > 0 && (CurrentBagWeight + item.Weight <= MaxBagWeight || !useWeight)) return true;

                uint count = item.Count;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem bagItem = Info.Inventory[i];

                    if (bagItem == null || bagItem.Info != item.Info) continue;

                    if (bagItem.Count + count <= bagItem.Info.StackSize) return true;

                    count -= bagItem.Info.StackSize - bagItem.Count;
                }

                return false;
            }

            if (useWeight && CurrentBagWeight + (item.Weight) > MaxBagWeight) return false;

            if (FreeSpace(Info.Inventory) > 0) return true;

            if (item.Info.StackSize > 1)
            {
                uint count = item.Count;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem bagItem = Info.Inventory[i];

                    if (bagItem.Info != item.Info) continue;

                    if (bagItem.Count + count <= bagItem.Info.StackSize) return true;

                    count -= bagItem.Info.StackSize - bagItem.Count;
                }
            }

            return false;
        }

        public bool CanGainItems(ItemInfo ItemInfo, uint ItemCount)
        {
            if (ItemCount < 1) return true;


            uint count = ItemCount;
            if (ItemInfo.StackSize > 1)
            {
                for (int u = 0; u < Info.Inventory.Length; u++)
                {
                    UserItem bagItem = Info.Inventory[u];

                    if (bagItem == null || bagItem.Info != ItemInfo) continue;

                    count -= bagItem.Info.StackSize - bagItem.Count;
                    break;
                }
            }

            if (FreeSpace(Info.Inventory) < (int)Math.Ceiling((double)count / ItemInfo.StackSize)) return false;
            int itemWeight = ItemInfo.Weight * (int)Math.Ceiling((double)count / ItemInfo.StackSize);
            if (CurrentBagWeight + (itemWeight) > MaxBagWeight) return false;
            return true;
        }

        public bool CanGainItems(UserItem[] items)
        {
            int itemCount = items.Count(e => e != null);
            uint itemWeight = 0;
            uint stackOffset = 0;

            if (itemCount < 1) return true;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;

                itemWeight += items[i].Weight;

                if (items[i].Info.StackSize > 1)
                {
                    uint count = items[i].Count;

                    for (int u = 0; u < Info.Inventory.Length; u++)
                    {
                        UserItem bagItem = Info.Inventory[u];

                        if (bagItem == null || bagItem.Info != items[i].Info) continue;

                        if (bagItem.Count + count > bagItem.Info.StackSize) stackOffset++;

                        break;
                    }
                }
            }

            if (CurrentBagWeight + (itemWeight) > MaxBagWeight) return false;
            if (FreeSpace(Info.Inventory) < itemCount + stackOffset) return false;

            return true;
        }

        public void GainItem(UserItem item)
        {
            //CheckItemInfo(item.Info);
            CheckItem(item);

            UserItem clonedItem = item.Clone();

            Enqueue(new S.GainedItem { Item = clonedItem }); //Cloned because we are probably going to change the amount.

            AddItem(item);
            UpdateQuestItem(item.Info);
            RefreshBagWeight();
        }

        public void GainItemMail(UserItem item, int reason)
        {
            string sender = "Bichon Administrator";
            string message = "You have been automatically sent an item \r\ndue to the following reason.\r\n";

            switch (reason)
            {
                case 1:
                    message = "Could not return item to bag after trade.";
                    break;
                case 2:
                    message = "Your loaned item has been returned.";
                    break;
                default:
                    message = "No reason provided.";
                    break;
            }

            //sent from player
            MailInfo mail = new MailInfo(Info.Index)
            {
                MailID = Envir.GenerateMailId(),
                Sender = sender,
                Message = message
            };

            mail.Items.Add(item);

            mail.Send();
        }

        private bool DropItem(UserItem item, int range = 1, bool DeathDrop = false)
        {
            ItemObject ob = new ItemObject(this, item, DeathDrop);

            if (!ob.Drop(range)) return false;

            if (item.Info.Type == ItemType.Meat)
                item.CurrentDura = (ushort)Math.Max(0, item.CurrentDura - 2000);

            return true;
        }
        private bool CanUseItem(UserItem item)
        {
            if (item == null) return false;

            switch (Gender)
            {
                case MirGender.Male:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Male))
                    {
                        ReceiveChat(Envir.Tr("You are not Female."), ChatType.System);
                        return false;
                    }
                    break;
                case MirGender.Female:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Female))
                    {
                        ReceiveChat(Envir.Tr("You are not Male."), ChatType.System);
                        return false;
                    }
                    break;
            }

            switch (Class)
            {
                case MirClass.Warrior:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Warrior))
                    {
                        ReceiveChat(Envir.Tr("Warriors cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
                case MirClass.Wizard:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Wizard))
                    {
                        ReceiveChat(Envir.Tr("Wizards cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
                case MirClass.Taoist:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Taoist))
                    {
                        ReceiveChat(Envir.Tr("Taoists cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
                case MirClass.Assassin:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Assassin))
                    {
                        ReceiveChat(Envir.Tr("Assassins cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
                case MirClass.Archer:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Archer))
                    {
                        ReceiveChat(Envir.Tr("Archer cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
                case MirClass.Monk:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Monk))
                    {
                        ReceiveChat(Envir.Tr("Monk cannot use this item."), ChatType.System);
                        return false;
                    }
                    break;
            }

            switch (item.Info.RequiredType)
            {
                case RequiredType.Level:
                    if (Level < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You are not a high enough level."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxAC:
                    if (MaxAC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough AC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxMAC:
                    if (MaxMAC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough MAC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxDC:
                    if (MaxDC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough DC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxMC:
                    if (MaxMC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough MC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxSC:
                    if (MaxSC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough SC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MaxLevel:
                    if (Level > item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You have exceeded the maximum level."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MinAC:
                    if (MinAC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough Base AC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MinMAC:
                    if (MinMAC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough Base MAC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MinDC:
                    if (MinDC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough Base DC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MinMC:
                    if (MinMC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough Base MC."), ChatType.System);
                        return false;
                    }
                    break;
                case RequiredType.MinSC:
                    if (MinSC < item.Info.RequiredAmount)
                    {
                        ReceiveChat(Envir.Tr("You do not have enough Base SC."), ChatType.System);
                        return false;
                    }
                    break;
            }

            switch (item.Info.Type)
            {
                case ItemType.Scroll:
                    switch (item.Info.Shape)
                    {
                        case 0:
                            if (CurrentMap.Info.NoEscape)
                            {
                                ReceiveChat(Envir.Tr("You cannot use Dungeon Escapes here"), ChatType.System);
                                return false;
                            }
                            break;
                        case 2:
                            if (CurrentMap.Info.NoRandom)
                            {
                                ReceiveChat(Envir.Tr("You cannot use Random Teleports here"), ChatType.System);
                                return false;
                            }
                            break;
                        case 6:
                            if (!Dead)
                            {
                                ReceiveChat(Envir.Tr("You cannot use Resurrection Scrolls whilst alive"), ChatType.Hint);
                                return false;
                            }
                            break;
                        case 10:
                            {
                                int skillId = item.Info.Effect;

                                if (MyGuild == null)
                                {
                                    ReceiveChat(Envir.Tr("You must be in a guild to use this skill"), ChatType.Hint);
                                    return false;
                                }
                                if (MyGuildRank != MyGuild.Ranks[0])
                                {
                                    ReceiveChat(Envir.Tr("You must be the guild leader to use this skill"), ChatType.Hint);
                                    return false;
                                }
                                GuildBuffInfo buffInfo = Envir.FindGuildBuffInfo(skillId);

                                if (buffInfo == null) return false;

                                if (MyGuild.BuffList.Any(e => e.Info.Id == skillId))
                                {
                                    ReceiveChat(Envir.Tr("Your guild already has this skill"), ChatType.Hint);
                                    return false;
                                }
                            }
                            break;
                    }
                    break;
                case ItemType.Potion:
                    if (CurrentMap.Info.NoDrug)
                    {
                        ReceiveChat(Envir.Tr("You cannot use Potions here"), ChatType.System);
                        return false;
                    }

                    switch (item.Info.Shape)
                    {
                        case 5:
                            if (Buffs.Any(x => x.Type == BuffType.Luck))
                            {
                                ReceiveChat("你已经获得该效果.", ChatType.System);
                                return false;
                            }
                            break;
                    }
                    break;

                case ItemType.Book:
                    if (item.Info.Effect > 0 && GetMagic((Spell)(item.Info.Effect)) == null)
                    {
                        ReceiveChat(Envir.Tr("You do not have a learn preposition skill."), ChatType.System);
                        return false;
                    }

                    if (Info.Magics.Any(t => t.Spell == (Spell)item.Info.Shape))
                    {
                        return false;
                    }
                    break;
                case ItemType.Saddle:
                case ItemType.Ribbon:
                case ItemType.Bells:
                case ItemType.Mask:
                case ItemType.Reins:
                    if (Info.Equipment[(int)EquipmentSlot.Mount] == null)
                    {
                        ReceiveChat(Envir.Tr("Can only be used with a mount"), ChatType.System);
                        return false;
                    }
                    break;
                case ItemType.Hook:
                case ItemType.Float:
                case ItemType.Bait:
                case ItemType.Finder:
                case ItemType.Reel:
                    if (Info.Equipment[(int)EquipmentSlot.Weapon] == null ||
                        (Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 49 && Info.Equipment[(int)EquipmentSlot.Weapon].Info.Shape != 50))
                    {
                        ReceiveChat(Envir.Tr("Can only be used with a fishing rod"), ChatType.System);
                        return false;
                    }
                    break;
                case ItemType.Pets:
                    switch (item.Info.Shape)
                    {
                        case 20://mirror rename creature
                            if (Info.IntelligentCreatures.Count == 0) return false;
                            break;
                        case 21://creature stone
                            break;
                        case 22://nuts maintain food levels
                            if (!CreatureSummoned)
                            {
                                ReceiveChat(Envir.Tr("Can only be used with a creature summoned"), ChatType.System);
                                return false;
                            }
                            break;
                        case 23://basic creature food
                            if (!CreatureSummoned)
                            {
                                ReceiveChat(Envir.Tr("Can only be used with a creature summoned"), ChatType.System);
                                return false;
                            }
                            else
                            {
                                for (int i = 0; i < Pets.Count; i++)
                                {
                                    if (Pets[i].Info.AI != 64) continue;
                                    if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;


                                    if (((IntelligentCreatureObject)Pets[i]).Fullness > 9900)
                                    {
                                        ReceiveChat(((IntelligentCreatureObject)Pets[i]).Name + " is not hungry", ChatType.System);
                                        return false;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case 24://wonderpill vitalize creature
                            if (!CreatureSummoned)
                            {
                                ReceiveChat(Envir.Tr("Can only be used with a creature summoned"), ChatType.System);
                                return false;
                            }
                            else
                            {
                                for (int i = 0; i < Pets.Count; i++)
                                {
                                    if (Pets[i].Info.AI != 64) continue;
                                    if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;


                                    if (((IntelligentCreatureObject)Pets[i]).Fullness > 0)
                                    {
                                        ReceiveChat(((IntelligentCreatureObject)Pets[i]).Name + " does not need to be vitalized", ChatType.System);
                                        return false;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case 25://Strongbox
                            break;
                        case 26://Wonderdrugs
                            break;
                        case 27://Fortunecookies
                            break;
                    }
                    break;
            }

            if (RidingMount && item.Info.Type != ItemType.Scroll && item.Info.Type != ItemType.Potion)
            {
                return false;
            }

            //if (item.Info.Type == ItemType.Book)
            //    for (int i = 0; i < Info.Magics.Count; i++)
            //        if (Info.Magics[i].Spell == (Spell)item.Info.Shape) return false;

            return true;
        }
        private bool CanEquipItem(UserItem item, int slot)
        {
            if (!Functions.CanWearItem((EquipmentSlot)slot, item.Info.Type))
                return false;

            switch (Gender)
            {
                case MirGender.Male:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Male))
                        return false;
                    break;
                case MirGender.Female:
                    if (!item.Info.RequiredGender.HasFlag(RequiredGender.Female))
                        return false;
                    break;
            }

            if (item.Info.NeedHumUp && !Info.HumUp)
            {
                return false;
            }

            switch (Class)
            {
                case MirClass.Warrior:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Warrior))
                        return false;
                    break;
                case MirClass.Wizard:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Wizard))
                        return false;
                    break;
                case MirClass.Taoist:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Taoist))
                        return false;
                    break;
                case MirClass.Assassin:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Assassin))
                        return false;
                    break;
                case MirClass.Monk:
                    if (!item.Info.RequiredClass.HasFlag(RequiredClass.Monk))
                        return false;
                    break;
            }

            switch (item.Info.RequiredType)
            {
                case RequiredType.Level:
                    if (Level < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxAC:
                    if (MaxAC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxMAC:
                    if (MaxMAC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxDC:
                    if (MaxDC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxMC:
                    if (MaxMC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxSC:
                    if (MaxSC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MaxLevel:
                    if (Level > item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MinAC:
                    if (MinAC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MinMAC:
                    if (MinMAC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MinDC:
                    if (MinDC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MinMC:
                    if (MinMC < item.Info.RequiredAmount)
                        return false;
                    break;
                case RequiredType.MinSC:
                    if (MinSC < item.Info.RequiredAmount)
                        return false;
                    break;
            }

            if (item.Info.Type == ItemType.Weapon || item.Info.Type == ItemType.Torch)
            {
                if (item.Weight - (Info.Equipment[slot] != null ? Info.Equipment[slot].Weight : 0) + CurrentHandWeight > MaxHandWeight)
                    return false;
            }
            else
                if (item.Weight - (Info.Equipment[slot] != null ? Info.Equipment[slot].Weight : 0) + CurrentWearWeight > MaxWearWeight)
                return false;

            if (RidingMount && item.Info.Type != ItemType.Torch)
            {
                return false;
            }

            return true;
        }
        private bool CanRemoveItem(MirGridType grid, UserItem item)
        {
            //Item  Stuck

            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                case MirGridType.Storage:
                    array = Account.Storage;
                    break;
                default:
                    return false;
            }

            if (RidingMount && item.Info.Type != ItemType.Torch)
            {
                return false;
            }

            return FreeSpace(array) > 0;
        }

        public bool CheckQuestItem(UserItem uItem, uint count)
        {
            foreach (var item in Info.Inventory.Where(item => item != null && item.Info == uItem.Info))
            {
                if (count > item.Count)
                {
                    count -= item.Count;
                    continue;
                }

                if (count > item.Count) continue;
                count = 0;
                break;
            }

            return count <= 0;
        }
        public bool CanGainQuestItem(UserItem item)
        {
            if (FreeSpace(Info.Inventory) > 0) return true;

            if (item.Info.StackSize > 1)
            {
                uint count = item.Count;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem bagItem = Info.Inventory[i];

                    if (bagItem.Info != item.Info) continue;

                    if (bagItem.Count + count <= bagItem.Info.StackSize) return true;

                    count -= bagItem.Info.StackSize - bagItem.Count;
                }
            }

            ReceiveChat(Envir.Tr("You cannot carry anymore quest items."), ChatType.System);

            return false;
        }
        public void GainQuestItem(UserItem item)
        {
            CheckItem(item);

            UserItem clonedItem = item.Clone();

            Enqueue(new S.GainedQuestItem { Item = clonedItem });

            AddQuestItem(item);


        }
        public void TakeQuestItem(ItemInfo uItem, uint count)
        {
            for (int o = 0; o < Info.Inventory.Length; o++)
            {
                UserItem item = Info.Inventory[o];
                if (item == null) continue;
                if (item.Info != uItem) continue;

                if (count > item.Count)
                {
                    Enqueue(new S.DeleteQuestItem { UniqueID = item.UniqueID, Count = item.Count });
                    Info.Inventory[o] = null;

                    count -= item.Count;
                    continue;
                }

                Enqueue(new S.DeleteQuestItem { UniqueID = item.UniqueID, Count = count });

                if (count == item.Count)
                    Info.Inventory[o] = null;
                else
                    item.Count -= count;
                break;
            }
        }

        private void DamageDura()
        {
            if (Info == null)
                return;

            for (int i = 0; i < Info.Equipment.Length; i++)
                DamageItem(Info.Equipment[i], Envir.Random.Next(1) + 1);
        }
        public void DamageWeapon()
        {
            DamageItem(Info.Equipment[(int)EquipmentSlot.Weapon], Envir.Random.Next(4) + 1);
        }
        private void DamageItem(UserItem item, int amount, bool isChanged = false)
        {
            if (item == null || item.CurrentDura == 0 || item.Info.Type == ItemType.Amulet) return;
            if ((item.WeddingRing == Info.Married) && (Info.Equipment[(int)EquipmentSlot.RingL].UniqueID == item.UniqueID)) return;

            if (item.Info.Unique.HasFlag(SpecialItemMode.NoDuraLoss))
                return;

            if (item.Info.Strong + item.Strong > 0) amount = Math.Max(1, amount - item.Info.Strong - item.Strong );
            item.CurrentDura = (ushort)Math.Max(ushort.MinValue, item.CurrentDura - amount);
            item.DuraChanged = true;

            if (item.CurrentDura > 0 && isChanged != true) return;
            Enqueue(new S.DuraChanged { UniqueID = item.UniqueID, CurrentDura = item.CurrentDura });

            item.DuraChanged = false;
            RefreshStats();
        }
        private void ConsumeItem(UserItem item, uint cost)
        {
            item.Count -= cost;
            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = cost });

            if (item.Count != 0) return;

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                if (Info.Equipment[i] != null && Info.Equipment[i].Slots.Length > 0)
                {
                    for (int j = 0; j < Info.Equipment[i].Slots.Length; j++)
                    {
                        if (Info.Equipment[i].Slots[j] != item) continue;
                        Info.Equipment[i].Slots[j] = null;
                        return;
                    }
                }

                if (Info.Equipment[i] != item) continue;
                Info.Equipment[i] = null;

                return;
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] != item) continue;
                Info.Inventory[i] = null;
                return;
            }
            //Item not found
        }

        private bool LuckWeapon()
        {
            var item = Info.Equipment[(int)EquipmentSlot.Weapon];

            if (item == null || item.Luck >= 7)
                return false;

            if (item.Info.Bind.HasFlag(BindMode.DontUpgrade))
                return false;

            if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.DontUpgrade))
                return false;

            Luck++;
            item.Luck++;
            Enqueue(new S.RefreshItem { Item = item });
            ReceiveChat(Envir.Tr("Luck dwells within your weapon."), ChatType.Hint);

            return true;
        }

        private bool TryLuckWeapon()
        {
            var item = Info.Equipment[(int)EquipmentSlot.Weapon];

            if (item == null || item.Luck >= 7)
                return false;

            if (item.Info.Bind.HasFlag(BindMode.DontUpgrade))
                return false;

            if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.DontUpgrade))
                return false;

            if (item.Luck > (Settings.MaxLuck * -1) && Envir.Random.Next(20) == 0)
            {
                Luck--;
                item.Luck--;
                Enqueue(new S.RefreshItem { Item = item });
                ReceiveChat(Envir.Tr("Curse dwells within your weapon."), ChatType.System);
            }
            else if (item.Luck <= 0 || Envir.Random.Next(10 * item.Luck) == 0)
            {
                Luck++;
                item.Luck++;
                Enqueue(new S.RefreshItem { Item = item });
                ReceiveChat(Envir.Tr("Luck dwells within your weapon."), ChatType.Hint);
            }
            else
            {
                ReceiveChat(Envir.Tr("No effect."), ChatType.Hint);
            }

            return true;
        }

        public void RequestUserName(uint id)
        {
            CharacterInfo Character = Envir.GetCharacterInfo((int)id);
            if (Character != null)
                Enqueue(new S.UserName { Id = (uint)Character.Index, Name = Character.Name });
        }
        public void RequestChatItem(ulong id)
        {
            //Enqueue(new S.ChatItemStats { ChatItemId = id, Stats = whatever });
        }
        public void Inspect(uint id)
        {
            if (ObjectID == id) return;

            PlayerObject player = CurrentMap.Players.SingleOrDefault(x => x.ObjectID == id || x.Pets.Count(y => y.ObjectID == id && y is Monsters.HumanWizard) > 0);

            if (player == null) return;
            Inspect(player.Info.Index);
        }
        public void Inspect(int id)
        {
            if (ObjectID == id) return;
            CharacterInfo player = Envir.GetCharacterInfo(id);
            if (player == null) return;
            CharacterInfo Lover = null;
            string loverName = "";
            if (player.Married != 0) Lover = Envir.GetCharacterInfo(player.Married);

            if (Lover != null)
                loverName = Lover.Name;

            for (int i = 0; i < player.Equipment.Length; i++)
            {
                UserItem u = player.Equipment[i];
                if (u == null) continue;

                CheckItem(u);
            }
            string guildname = "";
            string guildrank = "";
            GuildObject Guild = null;
            Rank GuildRank = null;
            if (player.GuildIndex != -1)
            {
                Guild = Envir.GetGuild(player.GuildIndex);
                if (Guild != null)
                {
                    GuildRank = Guild.FindRank(player.Name);
                    if (GuildRank == null)
                        Guild = null;
                    else
                    {
                        guildname = Guild.Name;
                        guildrank = GuildRank.Name;
                    }
                }
            }
            Enqueue(new S.PlayerInspect
            {
                Name = player.Name,
                Equipment = player.Equipment,
                GuildName = guildname,
                GuildRank = guildrank,
                Hair = player.Hair,
                Gender = player.Gender,
                Class = player.Class,
                Level = player.Level,
                LoverName = loverName
            });
        }
        public void RemoveObjects(MirDirection dir, int count)
        {
            switch (dir)
            {
                case MirDirection.Up:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.UpRight:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }

                    //Left Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange - count; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Right:
                    //Left Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.DownRight:
                    //Top Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }

                    //Left Block
                    for (int a = -Globals.DataRange + count; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Down:
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.DownLeft:
                    //Top Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }

                    //Right Block
                    for (int a = -Globals.DataRange + count; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Left:
                    for (int a = -Globals.DataRange; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
                case MirDirection.UpLeft:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }

                    //Right Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange - count; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Remove(this);
                            }
                        }
                    }
                    break;
            }
        }
        public void AddObjects(MirDirection dir, int count)
        {
            switch (dir)
            {
                case MirDirection.Up:
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.UpRight:
                    //Top Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }

                    //Right Block
                    for (int a = -Globals.DataRange + count; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Right:
                    for (int a = -Globals.DataRange; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.DownRight:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }

                    //Right Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange - count; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X + Globals.DataRange - b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Down:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.DownLeft:
                    //Bottom Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y + Globals.DataRange - a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }

                    //Left Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange - count; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.Left:
                    //Left Block
                    for (int a = -Globals.DataRange; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
                case MirDirection.UpLeft:
                    //Top Block
                    for (int a = 0; a < count; a++)
                    {
                        int y = CurrentLocation.Y - Globals.DataRange + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = -Globals.DataRange; b <= Globals.DataRange; b++)
                        {
                            int x = CurrentLocation.X + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }

                    //Left Block
                    for (int a = -Globals.DataRange + count; a <= Globals.DataRange; a++)
                    {
                        int y = CurrentLocation.Y + a;
                        if (y < 0 || y >= CurrentMap.Height) continue;

                        for (int b = 0; b < count; b++)
                        {
                            int x = CurrentLocation.X - Globals.DataRange + b;
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                ob.Add(this);
                            }
                        }
                    }
                    break;
            }
        }
        public override void Remove(PlayerObject player)
        {
            if (player == this) return;

            base.Remove(player);
            Enqueue(new S.ObjectRemove { ObjectID = player.ObjectID });
        }
        public override void Add(PlayerObject player)
        {
            if (player == this) return;

            //base.Add(player);
            Enqueue(player.GetInfoEx(this));
            player.Enqueue(GetInfoEx(player));

            player.SendHealth(this);
            SendHealth(player);
        }
        public override void Remove(MonsterObject monster)
        {
            Enqueue(new S.ObjectRemove { ObjectID = monster.ObjectID });
        }
        public override void Add(MonsterObject monster)
        {
            Enqueue(monster.GetInfo());

            monster.SendHealth(this);
        }
        public override void SendHealth(PlayerObject player)
        {
            if (!player.IsMember(this) && Envir.Time > RevTime) return;
            byte time = Math.Min(byte.MaxValue, (byte)Math.Max(5, (RevTime - Envir.Time) / 1000));
            player.Enqueue(new S.ObjectHealth { ObjectID = ObjectID, Percent = PercentHealth, Expire = time, Health = Health, MaxHealth = MaxHealth });
        }

        public override void ReceiveChat(string text, ChatType type)
        {
            Enqueue(new S.Chat { Message = text, Type = type });

            Report.ChatMessage(text);
        }

        public void ReceiveOutputMessage(string text, OutputMessageType type)
        {
            Enqueue(new S.SendOutputMessage { Message = text, Type = type });
        }

        private void CleanUp()
        {
            Connection.Player = null;
            Info.Player = null;
            Info.Mount = null;
            Connection = null;
            Account = null;
            Info = null;
        }

        public void Enqueue(Packet p)
        {
            if (Connection == null) return;
            Connection.Enqueue(p);
        }

        public void SpellToggle(Spell spell, bool use)
        {
            UserMagic magic;

            magic = GetMagic(spell);
            if (magic == null) return;

            int cost;
            switch (spell)
            {
                case Spell.Thrusting:
                    Info.Thrusting = use;
                    break;
                case Spell.LuoHanGunFa:
                    Info.LuoHanGunFa = use;
                    break;
                case Spell.HalfMoon:
                    Info.HalfMoon = use;
                    break;
                case Spell.CrossHalfMoon:
                    Info.CrossHalfMoon = use;
                    break;
                case Spell.DoubleSlash:
                    Info.DoubleSlash = use;
                    break;
                case Spell.TwinDrakeBlade:
                    magic = GetMagic(spell);
                    if (magic == null || magic.Toggle) return;
                    if (magic == null) return;
                    cost = magic.Info.BaseCost + magic.Level * magic.Info.LevelCost;
                    if (cost >= MP) return;
                    ChangeMP(-cost);
                    magic.Toggle = use;
                    Enqueue(new S.ObjectMagic { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Spell = spell });
                    break;
                case Spell.FlamingSword:
                    if (FlamingSword || Envir.Time < FlamingSwordTime) return;
                    magic = GetMagic(spell);
                    if (magic == null) return;
                    cost = magic.Info.BaseCost + magic.Level * magic.Info.LevelCost;
                    if (cost >= MP) return;

                    FlamingSword = true;
                    FlamingSwordTime = Envir.Time + 10000;
                    Enqueue(new S.SpellToggle { Spell = Spell.FlamingSword, CanUse = true });
                    ChangeMP(-cost);
                    break;

                case Spell.DaMoGunFa:
                    if (DaMoGunFa || Envir.Time < DaMoGunFaTime) return;
                    magic = GetMagic(spell);
                    if (magic == null) return;
                    cost = magic.Info.BaseCost + magic.Level * magic.Info.LevelCost;
                    if (cost >= MP) return;

                    DaMoGunFa = true;
                    DaMoGunFaTime = Envir.Time + 9000;
                    Enqueue(new S.SpellToggle { Spell = Spell.DaMoGunFa, CanUse = true });
                    ChangeMP(-cost);
                    break;

                case Spell.MentalState:
                    Info.MentalState = (byte)((Info.MentalState + 1) % 3);
                    for (int i = 0; i < Buffs.Count; i++)
                    {
                        if (Buffs[i].Type == BuffType.MentalState)
                        {
                            Buffs[i].Values[0] = Info.MentalState;
                            S.AddBuff addBuff = new S.AddBuff { Type = Buffs[i].Type, Caster = Buffs[i].Caster.Name, Expire = Buffs[i].ExpireTime - Envir.Time, Values = Buffs[i].Values, Infinite = Buffs[i].Infinite, ObjectID = ObjectID, Visible = Buffs[i].Visible };
                            Enqueue(addBuff);
                            break;
                        }
                    }
                    break;

                case Spell.TianLeiZhen:
                    magic.Toggle = use;
                    if (magic.Toggle)
                        AddBuffV2(new BuffV2 { Owner = this, Type = BuffType.DamageHalo, Caster = this, Expire = -1, TickSpeed = 1200, ObjectID = ObjectID });
                    else
                        RemoveBuffV2(BuffType.DamageHalo);
                    break;

                default:
                    magic.Toggle = use;
                    break;
            }
        }

        private void UpdateGMBuff()
        {
            if (!IsGM) return;
            for (int i = 0; i < Buffs.Count; i++)
            {
                if (Buffs[i].Type == BuffType.GameMaster)
                {
                    GMOptions options = GMOptions.None;

                    if (GMGameMaster) options |= GMOptions.GameMaster;
                    if (GMNeverDie) options |= GMOptions.Superman;
                    if (Observer) options |= GMOptions.Observer;

                    Buffs[i].Values[0] = (byte)options;
                    Enqueue(new S.AddBuff { Type = Buffs[i].Type, Caster = Buffs[i].Caster.Name, Expire = Buffs[i].ExpireTime - Envir.Time, Values = Buffs[i].Values, Infinite = Buffs[i].Infinite, ObjectID = ObjectID, Visible = Buffs[i].Visible });
                    break;
                }
            }
        }

        #region NPC

        public void CallDefaultNPC(DefaultNPCType type, params object[] value)
        {
            string key = string.Empty;

            switch (type)
            {
                case DefaultNPCType.Login:
                    key = "Login";
                    break;
                case DefaultNPCType.UseItem:
                    if (value.Length < 1) return;
                    key = string.Format("UseItem({0})", value[0]);
                    break;
                case DefaultNPCType.Trigger:
                    if (value.Length < 1) return;
                    key = string.Format("Trigger({0})", value[0]);
                    break;
                case DefaultNPCType.MapCoord:
                    if (value.Length < 3) return;
                    key = string.Format("MapCoord({0},{1},{2})", value[0], value[1], value[2]);
                    break;
                case DefaultNPCType.MapEnter:
                    if (value.Length < 1) return;
                    key = string.Format("MapEnter({0})", value[0]);
                    break;
                case DefaultNPCType.Die:
                    key = "Die";
                    break;
                case DefaultNPCType.LevelUp:
                    key = "LevelUp";
                    break;
                case DefaultNPCType.CustomCommand:
                    if (value.Length < 1) return;
                    key = string.Format("CustomCommand({0})", value[0]);
                    break;
                case DefaultNPCType.OnAcceptQuest:
                    if (value.Length < 1) return;
                    key = string.Format("OnAcceptQuest({0})", value[0]);
                    break;
                case DefaultNPCType.OnFinishQuest:
                    if (value.Length < 1) return;
                    key = string.Format("OnFinishQuest({0})", value[0]);
                    break;
                case DefaultNPCType.Daily:
                    key = "Daily";
                    Info.NewDay = false;
                    break;
                case DefaultNPCType.TalkMonster:
                    if (value.Length < 1) return;
                    key = string.Format("TalkMonster({0})", value[0]);
                    break;
                case DefaultNPCType.KillMonster:
                    if (value.Length < 1) return;
                    key = string.Format("KillMonster({0})", value[0]);
                    break;
            }

            key = string.Format("[@_{0}]", key);

            DelayedAction action = new DelayedAction(DelayedType.NPC, SMain.Envir.Time + 0, DefaultNPC.ObjectID, key);
            ActionList.Add(action);

            Enqueue(new S.NPCUpdate { NPCID = DefaultNPC.ObjectID });
        }

        public void CallDefaultNPC(uint objectID, string key)
        {
            if (DefaultNPC == null) return;
            DefaultNPC.Call(this, key.ToUpper());
            CallNPCNextPage();
            return;
        }

        public void CallNPC(uint objectID, string key)
        {
            if (Dead) return;

            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                NPCObject ob = CurrentMap.NPCs[i];
                if (ob.ObjectID != objectID) continue;
                if (!Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange)) return;
                ob.CheckVisible(this);

                if (!ob.VisibleLog[Info.Index] || !ob.Visible) return;

                ob.Call(this, key.ToUpper());
                break;
            }

            CallNPCNextPage();
        }
        private void CallNPCNextPage()
        {
            //process any new npc calls immediately
            for (int i = 0; i < ActionList.Count; i++)
            {
                if (ActionList[i].Type != DelayedType.NPC || ActionList[i].Time != -1) continue;
                var action = ActionList[i];

                ActionList.RemoveAt(i);

                CompleteNPC(action.Params);
            }
        }

        public void TalkMonster(uint objectID)
        {
            TalkingMonster talkMonster = FindObject(objectID, Globals.DataRange) as TalkingMonster;

            if (talkMonster == null) return;

            talkMonster.TalkingObjects.Add(this);

            CallDefaultNPC(DefaultNPCType.TalkMonster, talkMonster.Info.Name);
        }

        public void BuyItem(ulong index, uint count, PanelType type)
        {
            if (Dead) return;

            if (NPCPage == null ||
                !(String.Equals(NPCPage.Key, NPCObject.BuySellKey, StringComparison.CurrentCultureIgnoreCase) ||
                String.Equals(NPCPage.Key, NPCObject.BuyKey, StringComparison.CurrentCultureIgnoreCase) ||
                String.Equals(NPCPage.Key, NPCObject.BuyBackKey, StringComparison.CurrentCultureIgnoreCase) ||
                String.Equals(NPCPage.Key, NPCObject.BuyUsedKey, StringComparison.CurrentCultureIgnoreCase) ||
                String.Equals(NPCPage.Key, NPCObject.PearlBuyKey, StringComparison.CurrentCultureIgnoreCase))) return;

            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                NPCObject ob = CurrentMap.NPCs[i];
                if (ob.ObjectID != NPCID) continue;

                if (type == PanelType.Buy)
                {
                    ob.Buy(this, index, count);
                }
            }
        }
        public void CraftItem(ulong index, uint count, int[] slots)
        {
            if (Dead) return;

            if (NPCPage == null) return;

            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                NPCObject ob = CurrentMap.NPCs[i];
                if (ob.ObjectID != NPCID) continue;

                ob.Craft(this, index, count, slots);
            }
        }


        public void SellItem(ulong uniqueID, uint count)
        {
            S.SellItem p = new S.SellItem { UniqueID = uniqueID, Count = count };
            if (Dead || count == 0)
            {
                Enqueue(p);
                return;
            }

            if (NPCPage == null || !(String.Equals(NPCPage.Key, NPCObject.BuySellKey, StringComparison.CurrentCultureIgnoreCase) || String.Equals(NPCPage.Key, NPCObject.SellKey, StringComparison.CurrentCultureIgnoreCase)))
            {
                Enqueue(p);
                return;
            }

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                UserItem temp = null;
                int index = -1;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    temp = Info.Inventory[i];
                    if (temp == null || temp.UniqueID != uniqueID) continue;
                    index = i;
                    break;
                }

                if (temp == null || index == -1 || count > temp.Count)
                {
                    Enqueue(p);
                    return;
                }

                if (temp.Info.Bind.HasFlag(BindMode.DontSell))
                {
                    Enqueue(p);
                    return;
                }

                if (temp.RentalInformation != null && temp.RentalInformation.BindingFlags.HasFlag(BindMode.DontSell))
                {
                    Enqueue(p);
                    return;
                }

                if (ob.Types.Count != 0 && !ob.Types.Contains(temp.Info.Type))
                {
                    ReceiveChat(Envir.Tr("You cannot sell this item here."), ChatType.System);
                    Enqueue(p);
                    return;
                }

                if (temp.Info.StackSize > 1 && count != temp.Count)
                {
                    UserItem item = Envir.CreateFreshItem(temp.Info);
                    item.Count = count;

                    if (item.Price() / 2 + Account.Gold > uint.MaxValue)
                    {
                        Enqueue(p);
                        return;
                    }

                    temp.Count -= count;
                    temp = item;
                }
                else Info.Inventory[index] = null;

                ob.Sell(this, temp);
                p.Success = true;
                Enqueue(p);
                GainGold(temp.Price() / 2);
                RefreshBagWeight();

                return;
            }



            Enqueue(p);
        }
        public void RepairItem(ulong uniqueID, bool special = false)
        {
            Enqueue(new S.RepairItem { UniqueID = uniqueID });

            if (Dead) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.RepairKey, StringComparison.CurrentCultureIgnoreCase) && !special) || (!String.Equals(NPCPage.Key, NPCObject.SRepairKey, StringComparison.CurrentCultureIgnoreCase) && special)) return;

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                UserItem temp = null;
                int index = -1;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    temp = Info.Inventory[i];
                    if (temp == null || temp.UniqueID != uniqueID) continue;
                    index = i;
                    break;
                }

                if (temp == null || index == -1) return;

                if ((temp.Info.Bind.HasFlag(BindMode.DontRepair)) || (temp.Info.Bind.HasFlag(BindMode.NoSRepair) && special))
                {
                    ReceiveChat(Envir.Tr("You cannot Repair this item."), ChatType.System);
                    return;
                }

                if (ob.Types.Count != 0 && !ob.Types.Contains(temp.Info.Type))
                {
                    ReceiveChat(Envir.Tr("You cannot Repair this item here."), ChatType.System);
                    return;
                }

                uint cost = (uint)(temp.RepairPrice() * ob.PriceRate(this));

                uint baseCost = (uint)(temp.RepairPrice() * ob.PriceRate(this, true));

                if (cost > Account.Gold) return;

                Account.Gold -= cost;
                Enqueue(new S.LoseGold { Gold = cost });
                if (ob.Conq != null) ob.Conq.GoldStorage += (cost - baseCost);

                if (!special) temp.MaxDura = (ushort)Math.Max(0, temp.MaxDura - (temp.MaxDura - temp.CurrentDura) / 30);

                temp.CurrentDura = temp.MaxDura;
                temp.DuraChanged = false;

                Enqueue(new S.ItemRepaired { UniqueID = uniqueID, MaxDura = temp.MaxDura, CurrentDura = temp.CurrentDura });
                return;
            }
        }
        public void SendStorage()
        {
            if (Connection.StorageSent) return;
            Connection.StorageSent = true;

            for (int i = 0; i < Account.Storage.Length; i++)
            {
                UserItem item = Account.Storage[i];
                if (item == null) continue;
                //CheckItemInfo(item.Info);
                CheckItem(item);
            }

            Enqueue(new S.UserStorage { Storage = Account.Storage }); // Should be no alter before being sent.
        }

        #endregion

        #region Consignment

        public void ConsignItem(ulong uniqueID, uint price, byte currencyType)
        {
            S.ConsignItem p = new S.ConsignItem { UniqueID = uniqueID };
            if (price < Globals.MinConsignment || price > Globals.MaxConsignment || Dead)
            {
                Enqueue(p);
                return;
            }

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.ConsignKey, StringComparison.CurrentCultureIgnoreCase)
                && !String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase)))
            {
                Enqueue(p);
                return;
            }

            if (Account.Gold < Globals.ConsignmentCost)
            {
                Enqueue(p);
                return;
            }

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                UserItem temp = null;
                int index = -1;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    temp = Info.Inventory[i];
                    if (temp == null || temp.UniqueID != uniqueID) continue;
                    index = i;
                    break;
                }

                if (temp == null || index == -1)
                {
                    Enqueue(p);
                    return;
                }

                if (temp.Info.Bind.HasFlag(BindMode.DontSell))
                {
                    Enqueue(p);
                    return;
                }

                if (temp.RentalInformation != null && temp.RentalInformation.BindingFlags.HasFlag(BindMode.DontSell))
                {
                    Enqueue(p);
                    return;
                }

                //Check Max Consignment.

                AuctionInfo auction = new AuctionInfo
                {
                    AuctionID = Envir.GenerateAuctionId(),
                    CharacterIndex = Info.Index,
                    CharacterInfo = Info,
                    ConsignmentDate = Envir.Now,
                    Item = temp,
                    Price = price,
                    CurrencyType = currencyType,
                };

                Account.Auctions.AddLast(auction);
                Envir.Auctions.AddFirst(auction);

                p.Success = true;
                Enqueue(p);

                Report.ItemChanged("ConsignItem", temp, temp.Count, 1);

                Info.Inventory[index] = null;
                Account.Gold -= Globals.ConsignmentCost;
                Enqueue(new S.LoseGold { Gold = Globals.ConsignmentCost });
                RefreshBagWeight();
                UpdateQuestItem(temp.Info);
            }

            Enqueue(p);
        }
        public bool Match(AuctionInfo info, MarketMatcher matcher)
        {
            if (Envir.Now >= info.ConsignmentDate.AddDays(Globals.ConsignmentLength) && !info.Sold)
                info.Expired = true;

            // 一服多区支持 
            if (info.CharacterInfo == null || info.CharacterInfo.AccountInfo == null || !Envir.CheckSameServer(info.CharacterInfo.AccountInfo.RegionID, GetRegionID()))
                return false;

            return (UserMatch || !info.Expired && !info.Sold) && ((matcher.MatchType == ItemType.Nothing || info.Item.Info.Type == matcher.MatchType) 
                && (info.Item.Info.Shape >= matcher.MinShapes && info.Item.Info.Shape <= matcher.MaxShapes) 
                && (matcher.GradeType == ItemGrade.None || info.Item.Info.Grade == matcher.GradeType) &&
                (string.IsNullOrWhiteSpace(matcher.MatchName) || info.Item.Info.Name.Replace(" ", "").IndexOf(matcher.MatchName, StringComparison.OrdinalIgnoreCase) >= 0) 
                &&(matcher.RequiredClass == RequiredClass.NoLimit || info.Item.Info.RequiredClass.HasFlag(matcher.RequiredClass)));
        }
        public void MarketPage(int page)
        {
            if (Dead || Envir.Time < SearchTime) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase) && !String.Equals(NPCPage.Key, NPCObject.ConsignmentsKey, StringComparison.CurrentCultureIgnoreCase)) || page <= PageSent) return;

            SearchTime = Envir.Time + Globals.SearchDelay;

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                List<ClientAuction> listings = new List<ClientAuction>();

                for (int i = 0; i < 10; i++)
                {
                    if (i + page * 10 >= Search.Count) break;
                    listings.Add(Search[i + page * 10].CreateClientAuction(UserMatch));
                }

                for (int i = 0; i < listings.Count; i++)
                {
                    //CheckItemInfo(listings[i].Item.Info);
                    CheckItem(listings[i].Item);
                }

                PageSent = page;
                Enqueue(new S.NPCMarketPage { Listings = listings });
            }
        }
        public void GetMarket(MarketMatcher matcher)
        {
            Search.Clear();
            matcher.MatchName = matcher.MatchName.Replace(" ", "");
            PageSent = 0;
            LinkedListNode<AuctionInfo> current = UserMatch ? Account.Auctions.First : Envir.Auctions.First;

            while (current != null)
            {
                if (Match(current.Value, matcher)) Search.Add(current.Value);
                current = current.Next;
            }

            List<ClientAuction> listings = new List<ClientAuction>();

            for (int i = 0; i < 10; i++)
            {
                if (i >= Search.Count) break;
                listings.Add(Search[i].CreateClientAuction(UserMatch));
            }

            for (int i = 0; i < listings.Count; i++)
            {
                //CheckItemInfo(listings[i].Item.Info);
                CheckItem(listings[i].Item);
            }

            Enqueue(new S.NPCMarket { Listings = listings, Pages = (Search.Count - 1) / 10 + 1, UserMode = UserMatch });
        }

        public void MarketSearch(MarketMatcher matcher)
        {
            if (Dead || Envir.Time < SearchTime) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase) && !String.Equals(NPCPage.Key, NPCObject.ConsignmentsKey, StringComparison.CurrentCultureIgnoreCase))) return;

            SearchTime = Envir.Time + Globals.SearchDelay;

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                GetMarket(matcher);
            }
        }
        public void MarketRefresh()
        {
            if (Dead || Envir.Time < SearchTime) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase) && !String.Equals(NPCPage.Key, NPCObject.ConsignmentsKey, StringComparison.CurrentCultureIgnoreCase))) return;

            SearchTime = Envir.Time + Globals.SearchDelay;

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                GetMarket(MarketMatcher);
            }
        }
        public void MarketBuy(ulong auctionID, bool MailItems)
        {
            if (Dead)
            {
                Enqueue(new S.MarketFail { Reason = 0 });
                return;

            }

            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase))
            {
                Enqueue(new S.MarketFail { Reason = 1 });
                return;
            }

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                foreach (AuctionInfo auction in Envir.Auctions)
                {
                    if (auction.AuctionID != auctionID) continue;

                    if (auction.Sold)
                    {
                        Enqueue(new S.MarketFail { Reason = 2 });
                        return;
                    }

                    if (auction.Expired)
                    {
                        Enqueue(new S.MarketFail { Reason = 3 });
                        return;
                    }

                    if (auction.CurrencyType == 0)
                    {
                        if (auction.Price > Account.Gold)
                        {
                            Enqueue(new S.MarketFail { Reason = 4 });
                            return;
                        }
                    }
                    else
                    {
                        if (auction.Price > Account.Credit)
                        {
                            Enqueue(new S.MarketFail { Reason = 4 });
                            return;
                        }
                    }

                    if (!CanGainItem(auction.Item))
                    {
                        Enqueue(new S.MarketFail { Reason = 5 });
                        return;
                    }

                    if (Account.Auctions.Contains(auction))
                    {
                        Enqueue(new S.MarketFail { Reason = 6 });
                        return;
                    }

                    auction.Sold = true;
                    if (auction.CurrencyType == 0)
                    {
                        Account.Gold -= auction.Price;
                        Enqueue(new S.LoseGold { Gold = auction.Price });
                    }
                    else
                    {
                        Account.Credit -= auction.Price;
                        Enqueue(new S.LoseCredit { Credit = auction.Price });
                    }

                    if (MailItems)
                    {
                        List<UserItem> mailItem = new List<UserItem>();
                        mailItem.Add(auction.Item);
                        MailInfo mail = new MailInfo(Info.Index)
                        {
                            MailID = Envir.GenerateMailId(),
                            Sender = "Auctioneer",
                            Message = "Your item(s) are enclosed",
                            Items = mailItem,
                        };
                        mail.Send();
                    }
                    else
                        GainItem(auction.Item);

                    Report.ItemChanged("BuyMarketItem", auction.Item, auction.Item.Count, 2);

                    Envir.MessageAccount(auction.CharacterInfo.AccountInfo, string.Format(Envir.Tr("You Sold {0} for {1:#,##0} Gold"), auction.Item.FriendlyName, auction.Price), ChatType.Hint);
                    Enqueue(new S.MarketSuccess { Message = string.Format(Envir.Tr("You brought {0} for {1:#,##0} Gold"), auction.Item.FriendlyName, auction.Price) });
                    return;
                }
            }

            Enqueue(new S.MarketFail { Reason = 7 });
        }
        public void MarketGetBack(ulong auctionID)
        {
            if (Dead)
            {
                Enqueue(new S.MarketFail { Reason = 0 });
                return;

            }

            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.MarketKey, StringComparison.CurrentCultureIgnoreCase))
            {
                Enqueue(new S.MarketFail { Reason = 1 });
                return;
            }

            for (int n = 0; n < CurrentMap.NPCs.Count; n++)
            {
                NPCObject ob = CurrentMap.NPCs[n];
                if (ob.ObjectID != NPCID) continue;

                foreach (AuctionInfo auction in Account.Auctions)
                {
                    if (auction.AuctionID != auctionID) continue;

                    if (auction.Sold && auction.Expired)
                    {
                        SMain.Enqueue(string.Format(Envir.Tr("Auction both sold and Expired {0}"), Account.AccountID));
                        return;
                    }


                    if (!auction.Sold || auction.Expired)
                    {
                        if (!CanGainItem(auction.Item))
                        {
                            Enqueue(new S.MarketFail { Reason = 5 });
                            return;
                        }

                        Account.Auctions.Remove(auction);
                        Envir.Auctions.Remove(auction);
                        GainItem(auction.Item);
                        MarketSearch(MarketMatcher);

                        Report.ItemChanged("GetBackMarketItem", auction.Item, auction.Item.Count, 2);

                        return;
                    }

                    if (auction.CurrencyType == 0)
                    {
                        uint gold = (uint)Math.Max(0, auction.Price - auction.Price * Globals.Commission);
                        if (!CanGainGold(gold))
                        {
                            Enqueue(new S.MarketFail { Reason = 8 });
                            return;
                        }
                        GainGold(gold);
                        Enqueue(new S.MarketSuccess { Message = string.Format(Envir.Tr("You Sold {0} for {1:#,##0} Gold. \nEarnings: {2:#,##0} Gold.\nCommision: {3:#,##0} Gold.‎"), auction.Item.FriendlyName, auction.Price, gold, auction.Price - gold) });
                    }
                    else
                    {
                        uint credit = (uint)Math.Max(0, auction.Price - auction.Price * Globals.Commission);
                        GainCredit(credit);
                        Enqueue(new S.MarketSuccess { Message = string.Format(Envir.Tr("You Sold {0} for {1:#,##0} Credit. \nEarnings: {2:#,##0} Credit.\nCommision: {3:#,##0} Credit.‎"), auction.Item.FriendlyName, auction.Price, credit, auction.Price - credit) });
                    }

                    Account.Auctions.Remove(auction);
                    Envir.Auctions.Remove(auction);
                    MarketSearch(MarketMatcher);
                    return;
                }

            }

            Enqueue(new S.MarketFail { Reason = 7 });
        }

        #endregion

        #region Awakening

        public void Awakening(ulong uniqueID, AwakeType type, ulong materialUniquedID)
        {
            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.AwakeningKey, StringComparison.CurrentCultureIgnoreCase))
                return;

            if (type == AwakeType.None) return;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];
                if (item == null || item.UniqueID != uniqueID) continue;

                Awake awake = item.Awake;

                if (item.Info.Bind.HasFlag(BindMode.DontUpgrade))
                {
                    Enqueue(new S.Awakening { result = -1, removeID = -1 });
                    return;
                }

                if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.DontUpgrade))
                {
                    Enqueue(new S.Awakening { result = -1, removeID = -1 });
                    return;
                }

                if (!item.Info.CanAwakening)
                {
                    Enqueue(new S.Awakening { result = -1, removeID = -1 });
                    return;
                }

                if (awake.IsMaxLevel())
                {
                    Enqueue(new S.Awakening { result = -2, removeID = -1 });
                    return;
                }

                if (Info.AccountInfo.Gold < item.AwakeningPrice())
                {
                    Enqueue(new S.Awakening { result = -3, removeID = -1 });
                    return;
                }

                if (HasAwakeningNeedMaterials(item, type, materialUniquedID))
                {
                    Info.AccountInfo.Gold -= item.AwakeningPrice();
                    Enqueue(new S.LoseGold { Gold = item.AwakeningPrice() });

                    bool[] isHit;

                    switch (awake.UpgradeAwake(item, type, out isHit))
                    {
                        case -1:
                            Enqueue(new S.Awakening { result = -1, removeID = -1 });
                            break;
                        case 0:
                            AwakeningEffect(false, isHit);
                            Report.ItemChanged("Awake failed", Info.Inventory[i], 1, 1);
                            Info.Inventory[i] = null;
                            Enqueue(new S.Awakening { result = 0, removeID = (long)item.UniqueID });
                            break;
                        case 1:
                            Enqueue(new S.RefreshItem { Item = item });
                            AwakeningEffect(true, isHit);
                            Enqueue(new S.Awakening { result = 1, removeID = -1 });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void DowngradeAwakening(ulong UniqueID)
        {
            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.DowngradeKey, StringComparison.CurrentCultureIgnoreCase))
                return;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];
                if (item != null)
                {
                    if (item.UniqueID == UniqueID)
                    {
                        if (item.RentalInformation != null)
                        {
                            ReceiveChat($"Unable to downgrade {item.FriendlyName} as it belongs to {item.RentalInformation.OwnerName}", ChatType.System);
                            return;
                        }

                        if (Info.AccountInfo.Gold >= item.DowngradePrice())
                        {
                            Info.AccountInfo.Gold -= item.DowngradePrice();
                            Enqueue(new S.LoseGold { Gold = item.DowngradePrice() });

                            Awake awake = item.Awake;
                            int result = awake.RemoveAwake();
                            switch (result)
                            {
                                case 0:
                                    ReceiveChat(string.Format(Envir.Tr("{0} : Remove failed Level 0"), item.Name), ChatType.System);
                                    break;
                                case 1:
                                    ushort maxDura = (Envir.Random.Next(20) == 0) ? (ushort)(item.MaxDura - 1000) : item.MaxDura;
                                    if (maxDura < 1000) maxDura = 1000;

                                    Info.Inventory[i].CurrentDura = (Info.Inventory[i].CurrentDura >= maxDura) ? maxDura : Info.Inventory[i].CurrentDura;
                                    Info.Inventory[i].MaxDura = maxDura;
                                    ReceiveChat(string.Format(Envir.Tr("{0} : Remove success. Level {1}"), item.Name, item.Awake.getAwakeLevel()), ChatType.System);
                                    Enqueue(new S.RefreshItem { Item = item });
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void DisassembleItem(ulong UniqueID)
        {
            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.DisassembleKey, StringComparison.CurrentCultureIgnoreCase))
                return;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];

                if (item == null || item.UniqueID != UniqueID)
                    continue;

                if (item.Info.Bind.HasFlag(BindMode.UnableToDisassemble))
                {
                    ReceiveChat($"Unable to disassemble {item.FriendlyName}", ChatType.System);
                    return;
                }

                if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.UnableToDisassemble))
                {
                    ReceiveChat($"Unable to disassemble {item.FriendlyName} as it belongs to {item.RentalInformation.OwnerName}", ChatType.System);
                    return;
                }

                if (Info.AccountInfo.Gold >= item.DisassemblePrice())
                {
                    List<ItemInfo> dropList = new List<ItemInfo>();
                    foreach (DropInfo drop in Envir.AwakeningDrops)
                    {
                        if (drop.Item.Grade == item.Info.Grade - 1 ||
                            drop.Item.Grade == item.Info.Grade + 1)
                        {
                            if (Envir.Random.Next((drop.Chance <= 0) ? 1 : drop.Chance) == 0)
                            {
                                dropList.Add(drop.Item);
                            }
                        }

                        if (drop.Item.Grade == item.Info.Grade)
                        {
                            dropList.Add(drop.Item);
                        }
                    }

                    if (dropList.Count == 0) continue;

                    UserItem gainItem = Envir.CreateDropItem(dropList[Envir.Random.Next(dropList.Count)]);
                    if (gainItem == null) continue;
                    gainItem.Count = (uint)Envir.Random.Next((int)((((int)item.Info.Grade * item.Info.RequiredAmount) / 10) + item.Quality()));
                    if (gainItem.Count < 1) gainItem.Count = 1;

                    GainItem(gainItem);

                    Enqueue(new S.LoseGold { Gold = item.DisassemblePrice() });
                    Info.AccountInfo.Gold -= item.DisassemblePrice();

                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    Info.Inventory[i] = null;
                    UpdateQuestItem(item.Info);
                }
            }
        }

        public void ResetAddedItem(ulong UniqueID)
        {
            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.ResetKey, StringComparison.CurrentCultureIgnoreCase))
                return;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];
                if (item != null)
                {
                    if (item.UniqueID == UniqueID)
                    {
                        if (item.RentalInformation != null)
                        {
                            ReceiveChat($"Unable to reset {item.FriendlyName} as it belongs to {item.RentalInformation.OwnerName}", ChatType.System);
                            return;
                        }

                        if (Info.AccountInfo.Gold >= item.ResetPrice())
                        {
                            Info.AccountInfo.Gold -= item.ResetPrice();
                            Enqueue(new S.LoseGold { Gold = item.ResetPrice() });

                            UserItem newItem = new UserItem(item.Info);

                            ushort maxDura = (Envir.Random.Next(20) == 0) ? (ushort)(item.MaxDura - 1000) : item.MaxDura;
                            if (maxDura < 1000) maxDura = 1000;

                            newItem.UniqueID = item.UniqueID;
                            newItem.ItemIndex = item.ItemIndex;
                            newItem.CurrentDura = (item.CurrentDura >= maxDura) ? maxDura : item.CurrentDura;
                            newItem.MaxDura = maxDura;
                            newItem.Count = item.Count;
                            newItem.Slots = item.Slots;
                            newItem.Awake = item.Awake;

                            Info.Inventory[i] = newItem;

                            Enqueue(new S.RefreshItem { Item = Info.Inventory[i] });
                        }
                    }
                }
            }
        }

        public void AwakeningNeedMaterials(ulong UniqueID, AwakeType type)
        {
            if (type == AwakeType.None) return;

            foreach (UserItem item in Info.Inventory)
            {
                if (item != null)
                {
                    if (item.UniqueID == UniqueID)
                    {
                        Awake awake = item.Awake;

                        byte[] materialCount = new byte[2];
                        int idx = 0;
                        foreach (List<byte> material in Awake.AwakeMaterials[(int)type - 1])
                        {
                            byte materialRate = (byte)(Awake.AwakeMaterialRate[(int)item.Info.Grade - 1] * (float)awake.getAwakeLevel());
                            materialCount[idx] = material[(int)item.Info.Grade - 1];
                            materialCount[idx] += materialRate;
                            idx++;
                        }

                        ItemInfo[] materials = new ItemInfo[2];

                        foreach (ItemInfo info in Envir.ItemInfoList.Values)
                        {
                            if (item.Info.Grade == info.Grade &&
                                info.Type == ItemType.Awakening)
                            {
                                if (info.Shape == (short)type - 1)
                                {
                                    materials[0] = info;
                                }
                                else if (info.Shape == 100)
                                {
                                    materials[1] = info;
                                }
                            }
                        }

                        Enqueue(new S.AwakeningNeedMaterials { Materials = materials, MaterialsCount = materialCount });
                        break;
                    }
                }
            }
        }

        public void AwakeningEffect(bool isSuccess, bool[] isHit)
        {
            for (int i = 0; i < 5; i++)
            {
                Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = isHit[i] ? SpellEffect.AwakeningHit : SpellEffect.AwakeningMiss, EffectType = 0, DelayTime = (uint)(i * 500) });
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = isHit[i] ? SpellEffect.AwakeningHit : SpellEffect.AwakeningMiss, EffectType = 0, DelayTime = (uint)(i * 500) });
            }

            Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = isSuccess ? SpellEffect.AwakeningSuccess : SpellEffect.AwakeningFail, EffectType = 0, DelayTime = 2500 });
            Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = isSuccess ? SpellEffect.AwakeningSuccess : SpellEffect.AwakeningFail, EffectType = 0, DelayTime = 2500 });
        }

        public bool HasAwakeningNeedMaterials(UserItem item, AwakeType type, ulong materialUniquedID)
        {
            Awake awake = item.Awake;
            awake.extraSuccessRate = 0;
            byte[] materialCount = new byte[2];

            int idx = 0;
            foreach (List<byte> material in Awake.AwakeMaterials[(int)type - 1])
            {
                byte materialRate = (byte)(Awake.AwakeMaterialRate[(int)item.Info.Grade - 1] * (float)awake.getAwakeLevel());
                materialCount[idx] = material[(int)item.Info.Grade - 1];
                materialCount[idx] += materialRate;
                idx++;
            }

            byte[] currentCount = new byte[2] { 0, 0};

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem materialItem = Info.Inventory[i];
                if (materialItem == null)
                    continue;

                if (materialItem.Info.Grade == item.Info.Grade &&
                    materialItem.Info.Type == ItemType.Awakening)
                {
                    if (materialItem.Info.Shape == ((int)type - 1) &&
                        materialCount[0] - currentCount[0] != 0)
                    {
                        if (materialItem.Count <= materialCount[0] - currentCount[0])
                        {
                            currentCount[0] += (byte)materialItem.Count;
                        }
                        else if (materialItem.Count > materialCount[0] - currentCount[0])
                        {
                            currentCount[0] = (byte)(materialCount[0] - currentCount[0]);
                        }
                    }
                    else if ((SpecialItemShape)materialItem.Info.Shape == SpecialItemShape.AwakeMaterial &&
                        materialCount[1] - currentCount[1] != 0)
                    {
                        if (materialItem.Count <= materialCount[1] - currentCount[1])
                        {
                            currentCount[1] += (byte)materialItem.Count;
                        }
                        else if (materialItem.Count > materialCount[1] - currentCount[1])
                        {
                            currentCount[1] = (byte)(materialCount[1] - currentCount[1]);
                        }
                    }
                }
            }

            for (int i = 0; i < materialCount.Length; i++)
            {
                if (materialCount[i] != currentCount[i])
                {
                    Enqueue(new S.Awakening { result = -4, removeID = -1 });
                    return false;
                }
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] == null)
                    continue;

                if (Info.Inventory[i].Info.Type == ItemType.Awakening && (SpecialItemShape)Info.Inventory[i].Info.Shape == SpecialItemShape.SuccessRateUpItem)
                {
                    if (Info.Inventory[i].UniqueID == materialUniquedID)
                    {
                        Enqueue(new S.DeleteItem { UniqueID = Info.Inventory[i].UniqueID, Count = 1 });
                        Info.Inventory[i].Count -= 1;
                        if (Info.Inventory[i].Count <= 0 || Info.Inventory[i].Count > 1000000)
                            Info.Inventory[i] = null;

                        awake.extraSuccessRate = 100;
                        continue;
                    }
                }

                if (Info.Inventory[i].Info.Grade == item.Info.Grade &&
                    Info.Inventory[i].Info.Type == ItemType.Awakening)
                {
                    if (Info.Inventory[i].Info.Shape == ((int)type - 1) &&
                        currentCount[0] > 0)
                    {
                        if (Info.Inventory[i].Count <= currentCount[0])
                        {
                            Enqueue(new S.DeleteItem { UniqueID = Info.Inventory[i].UniqueID, Count = Info.Inventory[i].Count });
                            currentCount[0] -= (byte)Info.Inventory[i].Count;
                            UserItem temp = Info.Inventory[i];
                            Info.Inventory[i] = null;
                            UpdateQuestItem(temp.Info);
                        }
                        else if (Info.Inventory[i].Count > currentCount[0])
                        {
                            Enqueue(new S.DeleteItem { UniqueID = Info.Inventory[i].UniqueID, Count = (uint)currentCount[0] });
                            Info.Inventory[i].Count -= currentCount[0];
                            currentCount[0] = 0;
                            UpdateQuestItem(Info.Inventory[i].Info);
                        }
                    }
                    else if (Info.Inventory[i].Info.Shape == 100 &&
                        currentCount[1] > 0)
                    {
                        if (Info.Inventory[i].Count <= currentCount[1])
                        {
                            Enqueue(new S.DeleteItem { UniqueID = Info.Inventory[i].UniqueID, Count = Info.Inventory[i].Count });
                            currentCount[1] -= (byte)Info.Inventory[i].Count;
                            UserItem temp = Info.Inventory[i];
                            Info.Inventory[i] = null;
                            UpdateQuestItem(temp.Info);
                        }
                        else if (Info.Inventory[i].Count > currentCount[1])
                        {
                            Enqueue(new S.DeleteItem { UniqueID = Info.Inventory[i].UniqueID, Count = (uint)currentCount[1] });
                            Info.Inventory[i].Count -= currentCount[1];
                            currentCount[1] = 0;
                            UpdateQuestItem(Info.Inventory[i].Info);
                        }
                    }

                         }
            }
            return true;
        }

        #endregion

        #region Groups

        public void SwitchGroup(bool allow)
        {
            Enqueue(new S.SwitchGroup { AllowGroup = allow });

            if (AllowGroup == allow) return;
            AllowGroup = allow;

            if (AllowGroup || GroupMembers == null) return;

            RemoveGroupBuff();

            GroupMembers.Remove(this);
            Enqueue(new S.DeleteGroup());

            if (GroupMembers.Count > 1)
            {
                Packet p = new S.DeleteMember { Name = Name };

                for (int i = 0; i < GroupMembers.Count; i++)
                    GroupMembers[i].Enqueue(p);
            }
            else
            {
                GroupMembers[0].Enqueue(new S.DeleteGroup());
                GroupMembers[0].GroupMembers = null;
            }
            GroupMembers = null;
        }

        public void RemoveGroupBuff()
        {
            for (int i = 0; i < Buffs.Count; i++)
            {
                Buff buff = Buffs[i];

                if (buff.Type == BuffType.RelationshipEXP)
                {
                    CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);

                    if (Lover == null) continue;

                    PlayerObject LoverP = Envir.GetPlayer(Lover.Name);

                    RemoveBuff(BuffType.RelationshipEXP);

                    if (LoverP != null)
                    {
                        LoverP.RemoveBuff(BuffType.RelationshipEXP);
                    }
                }
                else if (buff.Type == BuffType.Mentee || buff.Type == BuffType.Mentor)
                {
                    CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);

                    if (Mentor == null) continue;

                    PlayerObject MentorP = Envir.GetPlayer(Mentor.Name);

                    RemoveBuff(buff.Type);

                    if (MentorP != null)
                    {
                        MentorP.RemoveBuff(buff.Type == BuffType.Mentee ? BuffType.Mentor : BuffType.Mentee);
                    }
                }
            }
        }
        public void AddMember(string name)
        {
            if (GroupMembers != null && GroupMembers[0] != this)
            {
                ReceiveChat(Envir.Tr("You are not the group leader."), ChatType.System);
                return;
            }

            if (GroupMembers != null && GroupMembers.Count >= Globals.MaxGroup)
            {
                ReceiveChat(Envir.Tr("Your group already has the maximum number of members."), ChatType.System);
                return;
            }

            PlayerObject player = Envir.GetPlayer(name);

            if (player == null)
            {
                ReceiveChat(name + Envir.Tr(" could not be found."), ChatType.System);
                return;
            }
            if (player == this)
            {
                ReceiveChat(Envir.Tr("You cannot group yourself."), ChatType.System);
                return;
            }

            if (!SMain.Envir.CheckSameServer(GetRegionID(), player.GetRegionID()))
            {
                ReceiveChat(name + Envir.Tr(" could not be found."), ChatType.System);
                return;
            }

            if (!player.AllowGroup)
            {
                ReceiveChat(name + Envir.Tr(" is not allowing group."), ChatType.System);
                return;
            }

            if (player.GroupMembers != null)
            {
                ReceiveChat(name + Envir.Tr(" is already in another group."), ChatType.System);
                return;
            }

            if (player.GroupInvitation != null)
            {
                ReceiveChat(name + Envir.Tr(" is already receiving an invite from another player."), ChatType.System);
                return;
            }

            SwitchGroup(true);
            player.Enqueue(new S.GroupInvite { Name = Name });
            player.GroupInvitation = this;

        }
        public void DelMember(string name)
        {
            if (GroupMembers == null)
            {
                ReceiveChat(Envir.Tr("You are not in a group."), ChatType.System);
                return;
            }
            if (GroupMembers[0] != this)
            {
                ReceiveChat(Envir.Tr("You are not the group leader."), ChatType.System);
                return;
            }

            PlayerObject player = null;

            for (int i = 0; i < GroupMembers.Count; i++)
            {
                if (String.Compare(GroupMembers[i].Name, name, StringComparison.OrdinalIgnoreCase) != 0) continue;
                player = GroupMembers[i];
                break;
            }


            if (player == null)
            {
                ReceiveChat(name + " is not in your group.", ChatType.System);
                return;
            }

            player.RemoveGroupBuff();

            GroupMembers.Remove(player);
            player.Enqueue(new S.DeleteGroup());

            if (GroupMembers.Count > 1)
            {
                Packet p = new S.DeleteMember { Name = player.Name };

                for (int i = 0; i < GroupMembers.Count; i++)
                    GroupMembers[i].Enqueue(p);
            }
            else
            {
                GroupMembers[0].Enqueue(new S.DeleteGroup());
                GroupMembers[0].GroupMembers = null;
            }
            player.GroupMembers = null;
        }
        public void GroupInvite(bool accept)
        {
            if (GroupInvitation == null)
            {
                ReceiveChat(Envir.Tr("You have not been invited to a group."), ChatType.System);
                return;
            }


            if (!accept)
            {
                GroupInvitation.ReceiveChat(Name + " has declined your group invite.", ChatType.System);
                GroupInvitation = null;
                return;
            }

            if (GroupMembers != null)
            {
                ReceiveChat(string.Format(Envir.Tr("You can no longer join {0}'s group"), GroupInvitation.Name), ChatType.System);
                GroupInvitation = null;
                return;
            }

            if (GroupInvitation.GroupMembers != null && GroupInvitation.GroupMembers[0] != GroupInvitation)
            {
                ReceiveChat(GroupInvitation.Name + " is no longer the group leader.", ChatType.System);
                GroupInvitation = null;
                return;
            }

            if (GroupInvitation.GroupMembers != null && GroupInvitation.GroupMembers.Count >= Globals.MaxGroup)
            {
                ReceiveChat(GroupInvitation.Name + "'s group already has the maximum number of members.", ChatType.System);
                GroupInvitation = null;
                return;
            }
            if (!GroupInvitation.AllowGroup)
            {
                ReceiveChat(GroupInvitation.Name + " is not on allow group.", ChatType.System);
                GroupInvitation = null;
                return;
            }
            if (GroupInvitation.Node == null)
            {
                ReceiveChat(GroupInvitation.Name + " no longer online.", ChatType.System);
                GroupInvitation = null;
                return;
            }

            if (GroupInvitation.GroupMembers == null)
            {
                GroupInvitation.GroupMembers = new List<PlayerObject> { GroupInvitation };
                GroupInvitation.Enqueue(new S.AddMember { Name = GroupInvitation.Name });
            }

            Packet p = new S.AddMember { Name = Name };
            GroupMembers = GroupInvitation.GroupMembers;
            GroupInvitation = null;

            for (int i = 0; i < GroupMembers.Count; i++)
            {
                PlayerObject member = GroupMembers[i];

                member.Enqueue(p);
                Enqueue(new S.AddMember { Name = member.Name });

                if (CurrentMap != member.CurrentMap || !Functions.InRange(CurrentLocation, member.CurrentLocation, Globals.DataRange)) continue;

                byte time = Math.Min(byte.MaxValue, (byte)Math.Max(5, (RevTime - Envir.Time) / 1000));

                member.Enqueue(new S.ObjectHealth { ObjectID = ObjectID, Percent = PercentHealth, Expire = time, Health = Health, MaxHealth = MaxHealth });
                Enqueue(new S.ObjectHealth { ObjectID = member.ObjectID, Percent = member.PercentHealth, Expire = time, Health = Health, MaxHealth = MaxHealth });

                for (int j = 0; j < member.Pets.Count; j++)
                {
                    MonsterObject pet = member.Pets[j];

                    Enqueue(new S.ObjectHealth { ObjectID = pet.ObjectID, Percent = pet.PercentHealth, Expire = time, Health = pet.Health, MaxHealth = pet.MaxHealth });
                }
            }

            GroupMembers.Add(this);

            //Adding Buff on for marriage
            if (GroupMembers != null)
                for (int i = 0; i < GroupMembers.Count; i++)
                {
                    PlayerObject player = GroupMembers[i];
                    if (Info.Married == player.Info.Index)
                    {
                        AddBuff(new Buff { Type = BuffType.RelationshipEXP, Caster = player, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.LoverEXPBonus } });
                        player.AddBuff(new Buff { Type = BuffType.RelationshipEXP, Caster = this, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.LoverEXPBonus } });
                    }
                    if (Info.Mentor == player.Info.Index)
                    {
                        if (Info.isMentor)
                        {
                            player.AddBuff(new Buff { Type = BuffType.Mentee, Caster = player, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.MentorExpBoost } });
                            AddBuff(new Buff { Type = BuffType.Mentor, Caster = this, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.MentorDamageBoost } });
                        }
                        else
                        {
                            AddBuff(new Buff { Type = BuffType.Mentee, Caster = player, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.MentorExpBoost } });
                            player.AddBuff(new Buff { Type = BuffType.Mentor, Caster = this, ExpireTime = Envir.Time * 1000, Infinite = true, Values = new int[] { Settings.MentorDamageBoost } });
                        }
                    }
                }



            for (int j = 0; j < Pets.Count; j++)
                Pets[j].BroadcastHealthChange();

            Enqueue(p);
        }

        #endregion

        #region Guilds

        public void CreateNewbieGuild(string GuildName)
        {
            if (Envir.GetGuild(GuildName) != null) return;
            //make the guild
            GuildObject guild = new GuildObject(this, GuildName) { Guildindex = ++Envir.NextGuildID };
            guild.Ranks[0].Members.Clear();
            guild.Membercount--;
            Envir.GuildList.Add(guild);
        }
        public bool CreateGuild(string GuildName)
        {
            if ((MyGuild != null) || (Info.GuildIndex != -1)) return false;
            if (Envir.GetGuild(GuildName) != null) return false;
            if (Info.Level < Settings.Guild_RequiredLevel)
            {
                ReceiveChat(String.Format(Envir.Tr("Your level is not high enough to create a guild, required: {0}"), Settings.Guild_RequiredLevel), ChatType.System);
                return false;
            }
            //check if we have the required items
            for (int i = 0; i < Settings.Guild_CreationCostList.Count; i++)
            {
                ItemVolume Required = Settings.Guild_CreationCostList[i];
                if (Required.Item == null)
                {
                    if (Info.AccountInfo.Gold < Required.Amount)
                    {
                        ReceiveChat(String.Format(Envir.Tr("Insufficient gold. Creating a guild requires {0} gold."), Required.Amount), ChatType.System);
                        return false;
                    }
                }
                else
                {
                    uint count = Required.Amount;
                    foreach (var item in Info.Inventory.Where(item => item != null && item.Info == Required.Item))
                    {
                        if ((Required.Item.Type == ItemType.Ore) && (item.CurrentDura / 1000 > Required.Amount))
                        {
                            count = 0;
                            break;
                        }
                        if (item.Count > count)
                            count = 0;
                        else
                            count = count - item.Count;
                        if (count == 0) break;
                    }
                    if (count != 0)
                    {
                        if (Required.Amount == 1)
                            ReceiveChat(String.Format(Envir.Tr("{0} is required to create a guild."), Required.Item.Name), ChatType.System);
                        else
                        {
                            if (Required.Item.Type == ItemType.Ore)
                                ReceiveChat(string.Format(Envir.Tr("{0} with purity {1} is recuired to create a guild."), Required.Item.Name, Required.Amount / 1000), ChatType.System);
                            else
                                ReceiveChat(string.Format(Envir.Tr("Insufficient {0}, you need {1} to create a guild."), Required.Item.Name, Required.Amount), ChatType.System);
                        }
                        return false;
                    }
                }
            }
            //take the required items
            for (int i = 0; i < Settings.Guild_CreationCostList.Count; i++)
            {
                ItemVolume Required = Settings.Guild_CreationCostList[i];
                if (Required.Item == null)
                {
                    if (Info.AccountInfo.Gold >= Required.Amount)
                    {
                        Info.AccountInfo.Gold -= Required.Amount;
                        Enqueue(new S.LoseGold { Gold = Required.Amount });
                    }
                }
                else
                {
                    uint count = Required.Amount;
                    for (int o = 0; o < Info.Inventory.Length; o++)
                    {
                        UserItem item = Info.Inventory[o];
                        if (item == null) continue;
                        if (item.Info != Required.Item) continue;

                        if ((Required.Item.Type == ItemType.Ore) && (item.CurrentDura / 1000 > Required.Amount))
                        {
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                            Info.Inventory[o] = null;
                            UpdateQuestItem(item.Info);
                            break;
                        }
                        if (count > item.Count)
                        {
                            Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                            Info.Inventory[o] = null;
                            count -= item.Count;
                            UpdateQuestItem(item.Info);
                            continue;
                        }

                        Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = count });
                        if (count == item.Count)
                            Info.Inventory[o] = null;
                        else
                            item.Count -= count;
                        UpdateQuestItem(item.Info);
                        break;
                    }
                }
            }
            RefreshStats();
            //make the guild
            GuildObject guild = new GuildObject(this, GuildName) { Guildindex = ++Envir.NextGuildID };
            Envir.GuildList.Add(guild);
            Info.GuildIndex = guild.Guildindex;
            MyGuild = guild;
            MyGuildRank = guild.FindRank(Name);
            GuildMembersChanged = true;
            GuildNoticeChanged = true;
            GuildCanRequestItems = true;
            //tell us we now have a guild
            BroadcastInfo();
            MyGuild.SendGuildStatus(this);
            return true;
        }
        public void EditGuildMember(string Name, string RankName, byte RankIndex, byte ChangeType)
        {
            if ((MyGuild == null) || (MyGuildRank == null))
            {
                ReceiveChat(Envir.Tr("You are not in a guild!"), ChatType.System);
                return;
            }
            switch (ChangeType)
            {
                case 0: //add member
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanRecruit))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to recruit new members!"), ChatType.System);
                        return;
                    }
                    if (Name == "") return;
                    PlayerObject player = Envir.GetPlayer(Name);
                    if (player == null || !Envir.CheckSameServer(player.GetRegionID(), GetRegionID()))
                    {
                        ReceiveChat(String.Format(Envir.Tr("{0} is not online!"), Name), ChatType.System);
                        return;
                    }
                    if ((player.MyGuild != null) || (player.MyGuildRank != null) || (player.Info.GuildIndex != -1))
                    {
                        ReceiveChat(String.Format(Envir.Tr("{0} is already in a guild!"), Name), ChatType.System);
                        return;
                    }
                    if (!player.EnableGuildInvite)
                    {
                        ReceiveChat(String.Format(Envir.Tr("{0} is disabling guild invites!"), Name), ChatType.System);
                        return;
                    }
                    if (player.PendingGuildInvite != null)
                    {
                        ReceiveChat(string.Format(Envir.Tr("{0} already has a guild invite pending."), Name), ChatType.System);
                        return;
                    }

                    if (MyGuild.IsAtWar())
                    {
                        ReceiveChat(Envir.Tr("Cannot recuit members whilst at war."), ChatType.System);
                        return;
                    }

                    player.Enqueue(new S.GuildInvite { Name = MyGuild.Name });
                    player.PendingGuildInvite = MyGuild;
                    break;
                case 1: //delete member
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanKick))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to remove members!"), ChatType.System);
                        return;
                    }
                    if (Name == "") return;

                    if (!MyGuild.DeleteMember(this, Name))
                    {
                        return;
                    }
                    break;
                case 2: //promote member (and it'll auto create a new rank at bottom if the index > total ranks!)
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to change other members rank!"), ChatType.System);
                        return;
                    }
                    if (Name == "") return;
                    MyGuild.ChangeRank(this, Name, RankIndex, RankName);
                    break;
                case 3: //change rank name
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to change ranks!"), ChatType.System);
                        return;
                    }
                    if ((RankName == "") || (RankName.Length < 3))
                    {
                        ReceiveChat(Envir.Tr("Rank name to short!"), ChatType.System);
                        return;
                    }
                    if (RankName.Contains("\\") || RankName.Length > 20)
                    {
                        return;
                    }
                    if (!MyGuild.ChangeRankName(this, RankName, RankIndex))
                        return;
                    break;
                case 4: //new rank
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to change ranks!"), ChatType.System);
                        return;
                    }
                    if (MyGuild.Ranks.Count > 254)
                    {
                        ReceiveChat(Envir.Tr("No more rank slots available."), ChatType.System);
                        return;
                    }
                    MyGuild.NewRank(this);
                    break;
                case 5: //change rank setting
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanChangeRank))
                    {
                        ReceiveChat(Envir.Tr("You are not allowed to change ranks!"), ChatType.System);
                        return;
                    }
                    int temp;

                    if (!int.TryParse(RankName, out temp))
                    {
                        return;
                    }
                    MyGuild.ChangeRankOption(this, RankIndex, temp, Name);
                    break;
            }
        }
        public void EditGuildNotice(List<string> notice)
        {
            if ((MyGuild == null) || (MyGuildRank == null))
            {
                ReceiveChat(Envir.Tr("You are not in a guild!"), ChatType.System);
                return;
            }
            if (!MyGuildRank.Options.HasFlag(RankOptions.CanChangeNotice))
            {

                ReceiveChat(Envir.Tr("You are not allowed to change the guild notice!"), ChatType.System);
                return;
            }
            if (notice.Count > 200)
            {
                ReceiveChat(Envir.Tr("Guild notice can not be longer then 200 lines!"), ChatType.System);
                return;
            }
            MyGuild.NewNotice(notice);
        }
        public void GuildInvite(bool accept)
        {
            if (PendingGuildInvite == null)
            {
                ReceiveChat(Envir.Tr("You have not been invited to a guild."), ChatType.System);
                return;
            }
            if (!accept) return;
            if (!PendingGuildInvite.HasRoom())
            {
                ReceiveChat(String.Format(Envir.Tr("{0} is full."), PendingGuildInvite.Name), ChatType.System);
                return;
            }
            PendingGuildInvite.NewMember(this);
            Info.GuildIndex = PendingGuildInvite.Guildindex;
            MyGuild = PendingGuildInvite;
            MyGuildRank = PendingGuildInvite.FindRank(Name);
            GuildMembersChanged = true;
            GuildNoticeChanged = true;
            //tell us we now have a guild
            BroadcastInfo();
            MyGuild.SendGuildStatus(this);
            PendingGuildInvite = null;
            EnableGuildInvite = false;
            GuildCanRequestItems = true;
            //refresh guildbuffs
            RefreshStats();
            if (MyGuild.BuffList.Count > 0)
                Enqueue(new S.GuildBuffList() { ActiveBuffs = MyGuild.BuffList });
        }
        public void RequestGuildInfo(byte Type)
        {
            if (MyGuild == null) return;
            if (MyGuildRank == null) return;
            switch (Type)
            {
                case 0://notice
                    if (GuildNoticeChanged)
                        Enqueue(new S.GuildNoticeChange() { notice = MyGuild.Notice });
                    GuildNoticeChanged = false;
                    break;
                case 1://memberlist
                    if (GuildMembersChanged)
                        Enqueue(new S.GuildMemberChange() { Status = 255, Ranks = MyGuild.Ranks });
                    break;
            }
        }
        public void GuildNameReturn(string Name)
        {
            if (Name == "") CanCreateGuild = false;
            if (!CanCreateGuild) return;
            if ((Name.Length < 3) || (Name.Length > 20))
            {
                ReceiveChat(Envir.Tr("Guild name too long."), ChatType.System);
                CanCreateGuild = false;
                return;
            }
            if (Name.Contains('\\'))
            {
                CanCreateGuild = false;
                return;
            }
            if (MyGuild != null)
            {
                ReceiveChat(Envir.Tr("You are already part of a guild."), ChatType.System);
                CanCreateGuild = false;
                return;
            }
            GuildObject guild = Envir.GetGuild(Name);
            if (guild != null)
            {
                ReceiveChat(string.Format(Envir.Tr("Guild {0} already exists."), Name), ChatType.System);
                CanCreateGuild = false;
                return;
            }

            CreateGuild(Name);
            CanCreateGuild = false;
        }
        public void GuildStorageGoldChange(Byte Type, uint Amount)
        {
            if ((MyGuild == null) || (MyGuildRank == null))
            {
                ReceiveChat(Envir.Tr("You are not part of a guild."), ChatType.System);
                return;
            }

            if (!InSafeZone)
            {
                ReceiveChat(Envir.Tr("You cannot use guild storage outside safezones."), ChatType.System);
                return;
            }

            if (Type == 0)//donate
            {
                if (Account.Gold < Amount)
                {
                    ReceiveChat(Envir.Tr("Insufficient gold."), ChatType.System);
                    return;
                }
                if ((MyGuild.Gold + (UInt64)Amount) > uint.MaxValue)
                {
                    ReceiveChat(Envir.Tr("Guild gold limit reached."), ChatType.System);
                    return;
                }
                Account.Gold -= Amount;
                MyGuild.Gold += Amount;
                Enqueue(new S.LoseGold { Gold = Amount });
                MyGuild.SendServerPacket(new S.GuildStorageGoldChange() { Type = 0, Name = Info.Name, Amount = Amount });
                MyGuild.NeedSave = true;
            }
            else
            {
                if (MyGuild.Gold < Amount)
                {
                    ReceiveChat(Envir.Tr("Insufficient gold."), ChatType.System);
                    return;
                }
                if (!CanGainGold(Amount))
                {
                    ReceiveChat(Envir.Tr("Gold limit reached."), ChatType.System);
                    return;
                }
                if (MyGuildRank.Index != 0)
                {
                    ReceiveChat(Envir.Tr("Insufficient rank."), ChatType.System);
                    return;
                }

                MyGuild.Gold -= Amount;
                GainGold(Amount);
                MyGuild.SendServerPacket(new S.GuildStorageGoldChange() { Type = 1, Name = Info.Name, Amount = Amount });
                MyGuild.NeedSave = true;
            }
        }
        public void GuildStorageItemChange(Byte Type, int from, int to)
        {
            S.GuildStorageItemChange p = new S.GuildStorageItemChange { Type = (byte)(3 + Type), From = from, To = to };
            if ((MyGuild == null) || (MyGuildRank == null))
            {
                Enqueue(p);
                ReceiveChat(Envir.Tr("You are not part of a guild."), ChatType.System);
                return;
            }

            if (!InSafeZone && Type != 3)
            {
                Enqueue(p);
                ReceiveChat(Envir.Tr("You cannot use guild storage outside safezones."), ChatType.System);
                return;
            }

            switch (Type)
            {
                case 0://store
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanStoreItem))
                    {
                        Enqueue(p);
                        ReceiveChat(Envir.Tr("You do not have permission to store items in guild storage."), ChatType.System);
                        return;
                    }
                    if (from < 0 || from >= Info.Inventory.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (to < 0 || to >= MyGuild.StoredItems.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (Info.Inventory[from] == null)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (Info.Inventory[from].Info.Bind.HasFlag(BindMode.DontStore))
                    {
                        Enqueue(p);
                        return;
                    }
                    if (Info.Inventory[from].RentalInformation != null && Info.Inventory[from].RentalInformation.BindingFlags.HasFlag(BindMode.DontStore))
                    {
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[to] != null)
                    {
                        ReceiveChat(Envir.Tr("Target slot not empty."), ChatType.System);
                        Enqueue(p);
                        return;
                    }
                    MyGuild.StoredItems[to] = new GuildStorageItem() { Item = Info.Inventory[from], UserId = Info.Index };
                    UserItem temp = Info.Inventory[from];
                    Info.Inventory[from] = null;
                    UpdateQuestItem(temp.Info);
                    RefreshBagWeight();
                    MyGuild.SendItemInfo(MyGuild.StoredItems[to].Item);
                    MyGuild.SendServerPacket(new S.GuildStorageItemChange() { Type = 0, User = Info.Index, Item = MyGuild.StoredItems[to], To = to, From = from });
                    MyGuild.NeedSave = true;
                    break;
                case 1://retrieve
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanRetrieveItem))
                    {

                        ReceiveChat(Envir.Tr("You do not have permission to retrieve items from guild storage."), ChatType.System);
                        return;
                    }
                    if (from < 0 || from >= MyGuild.StoredItems.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (to < 0 || to >= Info.Inventory.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (Info.Inventory[to] != null)
                    {
                        ReceiveChat(Envir.Tr("Target slot not empty."), ChatType.System);
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[from] == null)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (MaxBagWeight < CurrentBagWeight + MyGuild.StoredItems[from].Item.Weight)
                    {
                        ReceiveChat(Envir.Tr("Too overweight to retrieve item."), ChatType.System);
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[from].Item.Info.Bind.HasFlag(BindMode.DontStore))
                    {
                        Enqueue(p);
                        return;
                    }
                    Info.Inventory[to] = MyGuild.StoredItems[from].Item;
                    MyGuild.StoredItems[from] = null;
                    UpdateQuestItem(Info.Inventory[to].Info);
                    MyGuild.SendServerPacket(new S.GuildStorageItemChange() { Type = 1, User = Info.Index, To = to, From = from });
                    RefreshBagWeight();
                    MyGuild.NeedSave = true;
                    break;
                case 2: // Move Item
                    GuildStorageItem q = null;
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanStoreItem))
                    {
                        Enqueue(p);
                        ReceiveChat(Envir.Tr("You do not have permission to move items in guild storage."), ChatType.System);
                        return;
                    }
                    if (from < 0 || from >= MyGuild.StoredItems.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (to < 0 || to >= MyGuild.StoredItems.Length)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[from] == null)
                    {
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[from].Item.Info.Bind.HasFlag(BindMode.DontStore))
                    {
                        Enqueue(p);
                        return;
                    }
                    if (MyGuild.StoredItems[to] != null)
                    {
                        q = MyGuild.StoredItems[to];
                    }
                    MyGuild.StoredItems[to] = MyGuild.StoredItems[from];
                    if (q != null) MyGuild.StoredItems[from] = q;
                    else MyGuild.StoredItems[from] = null;

                    MyGuild.SendItemInfo(MyGuild.StoredItems[to].Item);

                    if (MyGuild.StoredItems[from] != null) MyGuild.SendItemInfo(MyGuild.StoredItems[from].Item);

                    MyGuild.SendServerPacket(new S.GuildStorageItemChange() { Type = 2, User = Info.Index, Item = MyGuild.StoredItems[to], To = to, From = from });
                    MyGuild.NeedSave = true;
                    break;
                case 3://request list
                    if (!GuildCanRequestItems) return;
                    GuildCanRequestItems = false;
                    for (int i = 0; i < MyGuild.StoredItems.Length; i++)
                    {
                        if (MyGuild.StoredItems[i] == null) continue;
                        UserItem item = MyGuild.StoredItems[i].Item;
                        if (item == null) continue;
                        //CheckItemInfo(item.Info);
                        CheckItem(item);
                    }
                    Enqueue(new S.GuildStorageList() { Items = MyGuild.StoredItems });
                    break;
            }

        }
        public void GuildWarReturn(string Name)
        {
            if (MyGuild == null || MyGuildRank != MyGuild.Ranks[0]) return;

            GuildObject enemyGuild = Envir.GetGuild(Name);

            if (enemyGuild == null)
            {
                ReceiveChat(string.Format(Envir.Tr("Could not find guild {0}."), Name), ChatType.System);
                return;
            }

            if (MyGuild == enemyGuild)
            {
                ReceiveChat(Envir.Tr("Cannot go to war with your own guild."), ChatType.System);
                return;
            }

            if (MyGuild.WarringGuilds.Contains(enemyGuild))
            {
                ReceiveChat(Envir.Tr("Already at war with this guild."), ChatType.System);
                return;
            }

            if (MyGuild.Gold < Settings.Guild_WarCost)
            {
                ReceiveChat(Envir.Tr("Not enough funds in guild bank."), ChatType.System);
                return;
            }

            if (MyGuild.GoToWar(enemyGuild))
            {
                ReceiveChat(string.Format(Envir.Tr("You started a war with {0}."), Name), ChatType.System);
                enemyGuild.SendMessage(string.Format(Envir.Tr("{0} has started a war"), MyGuild.Name), ChatType.System);

                MyGuild.Gold -= Settings.Guild_WarCost;
                MyGuild.SendServerPacket(new S.GuildStorageGoldChange() { Type = 2, Name = Info.Name, Amount = Settings.Guild_WarCost });
            }
        }

        public bool AtWar(PlayerObject attacker)
        {
            if (CurrentMap.Info.Fight) return true;

            if (MyGuild == null) return false;

            if (attacker == null || attacker.MyGuild == null) return false;

            if (!MyGuild.WarringGuilds.Contains(attacker.MyGuild)) return false;

            return true;
        }

        public void GuildBuffUpdate(byte Type, int Id)
        {
            if (MyGuild == null) return;
            if (MyGuildRank == null) return;
            if (Id < 0) return;
            switch (Type)
            {
                case 0://request info list
                    if (RequestedGuildBuffInfo) return;
                    Enqueue(new S.GuildBuffList() { GuildBuffs = Settings.Guild_BuffList });
                    break;
                case 1://buy the buff
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanActivateBuff))
                    {
                        ReceiveChat(Envir.Tr("You do not have the correct guild rank."), ChatType.System);
                        return;
                    }
                    GuildBuffInfo BuffInfo = Envir.FindGuildBuffInfo(Id);
                    if (BuffInfo == null)
                    {
                        ReceiveChat(Envir.Tr("Buff does not excist."), ChatType.System);
                        return;
                    }
                    if (MyGuild.GetBuff(Id) != null)
                    {
                        ReceiveChat(Envir.Tr("Buff already obtained."), ChatType.System);
                        return;
                    }
                    if ((MyGuild.Level < BuffInfo.LevelRequirement) || (MyGuild.SparePoints < BuffInfo.PointsRequirement)) return;//client checks this so it shouldnt be possible without a moded client :p
                    MyGuild.NewBuff(Id);
                    break;
                case 2://activate the buff
                    if (!MyGuildRank.Options.HasFlag(RankOptions.CanActivateBuff))
                    {
                        ReceiveChat(Envir.Tr("You do not have the correct guild rank."), ChatType.System);
                        return;
                    }
                    GuildBuff Buff = MyGuild.GetBuff(Id);
                    if (Buff == null)
                    {
                        ReceiveChat(Envir.Tr("Buff not obtained."), ChatType.System);
                        return;
                    }
                    if ((MyGuild.Gold < Buff.Info.ActivationCost) || (Buff.Active)) return;
                    MyGuild.ActivateBuff(Id);
                    break;
            }
        }

        #endregion

        #region Trading

        public void DepositTradeItem(int from, int to)
        {
            S.DepositTradeItem p = new S.DepositTradeItem { From = from, To = to, Success = false };

            if (from < 0 || from >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Trade.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Info.Inventory[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (temp.Info.Bind.HasFlag(BindMode.DontTrade))
            {
                Enqueue(p);
                return;
            }

            if (temp.RentalInformation != null && temp.RentalInformation.BindingFlags.HasFlag(BindMode.DontTrade))
            {
                Enqueue(p);
                return;
            }

            if (Info.Trade[to] == null)
            {
                Info.Trade[to] = temp;
                Info.Inventory[from] = null;
                RefreshBagWeight();
                TradeItem();

                Report.ItemMoved("DepositTradeItem", temp, MirGridType.Inventory, MirGridType.Trade, from, to);

                p.Success = true;
                Enqueue(p);

                UpdateQuestItem(temp.Info);
                return;
            }
            Enqueue(p);

        }
        public void RetrieveTradeItem(int from, int to)
        {
            S.RetrieveTradeItem p = new S.RetrieveTradeItem { From = from, To = to, Success = false };

            if (from < 0 || from >= Info.Trade.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Info.Trade[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (temp.Weight + CurrentBagWeight > MaxBagWeight)
            {
                ReceiveChat(Envir.Tr("Too heavy to get back."), ChatType.System);
                Enqueue(p);
                return;
            }

            if (Info.Inventory[to] == null)
            {
                Info.Inventory[to] = temp;
                Info.Trade[from] = null;

                p.Success = true;
                RefreshBagWeight();
                TradeItem();

                Report.ItemMoved("RetrieveTradeItem", temp, MirGridType.Trade, MirGridType.Inventory, from, to);
            }

            Enqueue(p);
        }

        public void TradeRequest()
        {
            if (TradePartner != null)
            {
                ReceiveChat(Envir.Tr("You are already trading."), ChatType.System);
                return;
            }

            Point target = Functions.PointMove(CurrentLocation, Direction, 1);
            var cellObjects = CurrentMap.GetCellObjects(target);
            PlayerObject player = null;

            if (cellObjects == null || cellObjects.Count < 1) return;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                MapObject ob = cellObjects[i];
                if (ob.Race != ObjectType.Player) continue;

                player = Envir.GetPlayer(ob.Name);
            }

            if (player == null)
            {
                ReceiveChat(string.Format(Envir.Tr("You must face someone to trade.")), ChatType.System);
                return;
            }

            if (player != null)
            {
                if (!Functions.FacingEachOther(Direction, CurrentLocation, player.Direction, player.CurrentLocation))
                {
                    ReceiveChat(string.Format(Envir.Tr("You must face someone to trade.")), ChatType.System);
                    return;
                }

                if (player == this)
                {
                    ReceiveChat(Envir.Tr("You cannot trade with your self."), ChatType.System);
                    return;
                }

                if (player.Dead || Dead)
                {
                    ReceiveChat(Envir.Tr("Cannot trade when dead"), ChatType.System);
                    return;
                }

                if (player.TradeInvitation != null)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} already has a trade invitation."), player.Info.Name), ChatType.System);
                    return;
                }

                if (!player.AllowTrade)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} is not allowing trade at the moment."), player.Info.Name), ChatType.System);
                    return;
                }

                if (!Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) || player.CurrentMap != CurrentMap)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} is not within trading range."), player.Info.Name), ChatType.System);
                    return;
                }

                if (player.TradePartner != null)
                {
                    ReceiveChat(string.Format(Envir.Tr("Player {0} is already trading."), player.Info.Name), ChatType.System);
                    return;
                }

                player.TradeInvitation = this;
                player.Enqueue(new S.TradeRequest { Name = Info.Name });
            }
        }
        public void TradeReply(bool accept)
        {
            if (TradeInvitation == null || TradeInvitation.Info == null)
            {
                TradeInvitation = null;
                return;
            }

            if (!accept)
            {
                TradeInvitation.ReceiveChat(string.Format(Envir.Tr("Player {0} has refused to trade."), Info.Name), ChatType.System);
                TradeInvitation = null;
                return;
            }

            if (TradePartner != null)
            {
                ReceiveChat(Envir.Tr("You are already trading."), ChatType.System);
                TradeInvitation = null;
                return;
            }

            if (TradeInvitation.TradePartner != null)
            {
                ReceiveChat(string.Format(Envir.Tr("Player {0} is already trading."), TradeInvitation.Info.Name), ChatType.System);
                TradeInvitation = null;
                return;
            }

            TradePartner = TradeInvitation;
            TradeInvitation.TradePartner = this;
            TradeInvitation = null;

            Enqueue(new S.TradeAccept { Name = TradePartner.Info.Name });
            TradePartner.Enqueue(new S.TradeAccept { Name = Info.Name });
        }
        public void TradeGold(uint amount)
        {
            TradeUnlock();

            if (TradePartner == null) return;

            if (Account.Gold < amount)
            {
                return;
            }

            TradeGoldAmount += amount;
            Account.Gold -= amount;

            Enqueue(new S.LoseGold { Gold = amount });
            TradePartner.Enqueue(new S.TradeGold { Amount = TradeGoldAmount });
        }
        public void TradeItem()
        {
            TradeUnlock();

            if (TradePartner == null) return;

            for (int i = 0; i < Info.Trade.Length; i++)
            {
                UserItem u = Info.Trade[i];
                if (u == null) continue;

                //TradePartner.CheckItemInfo(u.Info);
                TradePartner.CheckItem(u);
            }

            TradePartner.Enqueue(new S.TradeItem { TradeItems = Info.Trade });
        }

        public void TradeUnlock()
        {
            TradeLocked = false;

            if (TradePartner != null)
            {
                TradePartner.TradeLocked = false;
            }
        }

        public void TradeConfirm(bool confirm)
        {
            if (!confirm)
            {
                TradeLocked = false;
                return;
            }

            if (TradePartner == null)
            {
                TradeCancel();
                return;
            }

            if (!Functions.InRange(TradePartner.CurrentLocation, CurrentLocation, Globals.DataRange) || TradePartner.CurrentMap != CurrentMap ||
                !Functions.FacingEachOther(Direction, CurrentLocation, TradePartner.Direction, TradePartner.CurrentLocation))
            {
                TradeCancel();
                return;
            }

            TradeLocked = true;

            if (TradeLocked && !TradePartner.TradeLocked)
            {
                TradePartner.ReceiveChat(string.Format(Envir.Tr("Player {0} is waiting for you to confirm trade."), Info.Name), ChatType.System);
            }

            if (!TradeLocked || !TradePartner.TradeLocked) return;

            PlayerObject[] TradePair = new PlayerObject[2] { TradePartner, this };

            bool CanTrade = true;
            UserItem u;

            //check if both people can accept the others items
            for (int p = 0; p < 2; p++)
            {
                int o = p == 0 ? 1 : 0;

                if (!TradePair[o].CanGainItems(TradePair[p].Info.Trade))
                {
                    CanTrade = false;
                    TradePair[p].ReceiveChat(Envir.Tr("Trading partner cannot accept all items."), ChatType.System);
                    TradePair[p].Enqueue(new S.TradeCancel { Unlock = true });

                    TradePair[o].ReceiveChat(Envir.Tr("Unable to accept all items."), ChatType.System);
                    TradePair[o].Enqueue(new S.TradeCancel { Unlock = true });

                    return;
                }

                if (!TradePair[o].CanGainGold(TradePair[p].TradeGoldAmount))
                {
                    CanTrade = false;
                    TradePair[p].ReceiveChat(Envir.Tr("Trading partner cannot accept any more gold."), ChatType.System);
                    TradePair[p].Enqueue(new S.TradeCancel { Unlock = true });

                    TradePair[o].ReceiveChat(Envir.Tr("Unable to accept any more gold."), ChatType.System);
                    TradePair[o].Enqueue(new S.TradeCancel { Unlock = true });

                    return;
                }
            }

            //swap items
            if (CanTrade)
            {
                for (int p = 0; p < 2; p++)
                {
                    int o = p == 0 ? 1 : 0;

                    for (int i = 0; i < TradePair[p].Info.Trade.Length; i++)
                    {
                        u = TradePair[p].Info.Trade[i];

                        if (u == null) continue;

                        TradePair[o].GainItem(u);
                        TradePair[p].Info.Trade[i] = null;

                        Report.ItemMoved("TradeConfirm", u, MirGridType.Trade, MirGridType.Inventory, i, -99, string.Format("Trade from {0} to {1}", TradePair[p].Name, TradePair[o].Name));
                    }

                    if (TradePair[p].TradeGoldAmount > 0)
                    {
                        Report.GoldChanged("TradeConfirm", TradePair[p].TradeGoldAmount, Account.Gold, true, string.Format("Trade from {0} to {1}", TradePair[p].Name, TradePair[o].Name));

                        TradePair[o].GainGold(TradePair[p].TradeGoldAmount);
                        TradePair[p].TradeGoldAmount = 0;
                    }

                    TradePair[p].ReceiveChat(Envir.Tr("Trade successful."), ChatType.System);
                    TradePair[p].Enqueue(new S.TradeConfirm());

                    TradePair[p].TradeLocked = false;
                    TradePair[p].TradePartner = null;
                }
            }
        }
        public void TradeCancel()
        {
            TradeUnlock();

            if (TradePartner == null)
            {
                return;
            }

            PlayerObject[] TradePair = new PlayerObject[2] { TradePartner, this };

            for (int p = 0; p < 2; p++)
            {
                if (TradePair[p] != null)
                {
                    for (int t = 0; t < TradePair[p].Info.Trade.Length; t++)
                    {
                        UserItem temp = TradePair[p].Info.Trade[t];

                        if (temp == null) continue;

                        if (FreeSpace(TradePair[p].Info.Inventory) < 1)
                        {
                            TradePair[p].GainItemMail(temp, 1);
                            Report.ItemMailed("TradeCancel", temp, temp.Count, 1);

                            TradePair[p].Enqueue(new S.DeleteItem { UniqueID = temp.UniqueID, Count = temp.Count });
                            TradePair[p].Info.Trade[t] = null;
                            TradePair[p].UpdateQuestItem(temp.Info);
                            continue;
                        }

                        for (int i = 0; i < TradePair[p].Info.Inventory.Length; i++)
                        {
                            if (TradePair[p].Info.Inventory[i] != null) continue;

                            //Put item back in inventory
                            if (TradePair[p].CanGainItem(temp))
                            {
                                TradePair[p].RetrieveTradeItem(t, i);
                            }
                            else //Send item to mailbox if it can no longer be stored
                            {
                                TradePair[p].GainItemMail(temp, 1);
                                Report.ItemMailed("TradeCancel", temp, temp.Count, 1);

                                TradePair[p].Enqueue(new S.DeleteItem { UniqueID = temp.UniqueID, Count = temp.Count });
                            }

                            TradePair[p].UpdateQuestItem(temp.Info);
                            TradePair[p].Info.Trade[t] = null;

                            break;
                        }
                    }

                    //Put back deposited gold
                    if (TradePair[p].TradeGoldAmount > 0)
                    {
                        Report.GoldChanged("TradeCancel", TradePair[p].TradeGoldAmount, Account.Gold, false);

                        TradePair[p].GainGold(TradePair[p].TradeGoldAmount);
                        TradePair[p].TradeGoldAmount = 0;
                    }

                    TradePair[p].TradeLocked = false;
                    TradePair[p].TradePartner = null;

                    TradePair[p].Enqueue(new S.TradeCancel { Unlock = false });
                }
            }
        }

        #endregion

        #region Mounts

        public void RefreshMount(bool refreshStats = true)
        {
            if (RidingMount)
            {
                if (MountType < 0)
                {
                    RidingMount = false;
                }
                else if (!Mount.CanRide)
                {
                    RidingMount = false;
                    ReceiveChat(Envir.Tr("You must have a saddle to ride your mount"), ChatType.System);
                }
                else if (!Mount.CanMapRide)
                {
                    RidingMount = false;
                    ReceiveChat(Envir.Tr("You cannot ride on this map"), ChatType.System);
                }
                else if (!Mount.CanDungeonRide)
                {
                    RidingMount = false;
                    ReceiveChat(Envir.Tr("You cannot ride here without a bridle"), ChatType.System);
                }
            }
            else
            {
                RidingMount = false;
            }

            if (refreshStats)
                RefreshStats();

            Broadcast(GetMountInfo());
            Enqueue(GetMountInfo());
        }
        public void IncreaseMountLoyalty(int amount)
        {
            UserItem item = Info.Equipment[(int)EquipmentSlot.Mount];
            if (item != null && item.CurrentDura < item.MaxDura)
            {
                item.CurrentDura = (ushort)Math.Min(item.MaxDura, item.CurrentDura + amount);
                item.DuraChanged = false;
                Enqueue(new S.ItemRepaired { UniqueID = item.UniqueID, MaxDura = item.MaxDura, CurrentDura = item.CurrentDura });
            }
        }
        public void DecreaseMountLoyalty(int amount)
        {
            if (Envir.Time > DecreaseLoyaltyTime)
            {
                DecreaseLoyaltyTime = Envir.Time + (Mount.SlowLoyalty ? (LoyaltyDelay * 2) : LoyaltyDelay);
                UserItem item = Info.Equipment[(int)EquipmentSlot.Mount];
                if (item != null && item.CurrentDura > 0)
                {
                    DamageItem(item, amount);

                    if (item.CurrentDura == 0)
                    {
                        RefreshMount();
                    }
                }
            }
        }

        #endregion

        #region Fishing

        public void FishingCast(bool cast, bool cancel = false)
        {
            UserItem rod = Info.Equipment[(int)EquipmentSlot.Weapon];

            byte flexibilityStat = 0;
            sbyte successStat = 0;
            byte nibbleMin = 0, nibbleMax = 0;
            byte failedAddSuccessMin = 0, failedAddSuccessMax = 0;
            FishingProgressMax = Settings.FishingAttempts;//30;

            if (rod == null || (rod.Info.Shape != 49 && rod.Info.Shape != 50) || rod.CurrentDura <= 0)
            {
                Fishing = false;
                return;
            }

            Point fishingPoint = Functions.PointMove(CurrentLocation, Direction, 3);

            if (fishingPoint.X < 0 || fishingPoint.Y < 0 || CurrentMap.Width < fishingPoint.X || CurrentMap.Height < fishingPoint.Y)
            {
                Fishing = false;
                return;
            }

            if (!CurrentMap.CanFishing(fishingPoint.X, fishingPoint.Y))
            {
                Fishing = false;
                return;
            }

            flexibilityStat = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, flexibilityStat + rod.Info.CriticalRate)));
            successStat = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, successStat + rod.Info.MaxAC)));

            if (cast)
            {
                DamageItem(rod, 1, true);
            }

            UserItem hook = rod.Slots[(int)FishingSlot.Hook];

            if (hook == null)
            {
                ReceiveChat(Envir.Tr("You need a hook."), ChatType.System);
                return;
            }
            else
            {
                DamagedFishingItem(FishingSlot.Hook, 1);
            }

            foreach (UserItem temp in rod.Slots)
            {
                if (temp == null) continue;

                ItemInfo realItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

                switch (realItem.Type)
                {
                    case ItemType.Hook:
                        {
                            flexibilityStat = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, flexibilityStat + temp.CriticalRate + realItem.CriticalRate)));
                        }
                        break;
                    case ItemType.Float:
                        {
                            nibbleMin = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, nibbleMin + realItem.MinAC)));
                            nibbleMax = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, nibbleMax + realItem.MaxAC)));
                        }
                        break;
                    case ItemType.Bait:
                        {
                            successStat = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, successStat + realItem.MaxAC)));
                        }
                        break;
                    case ItemType.Finder:
                        {
                            failedAddSuccessMin = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, failedAddSuccessMin + realItem.MinAC)));
                            failedAddSuccessMax = (byte)Math.Max(byte.MinValue, (Math.Min(byte.MaxValue, failedAddSuccessMax + realItem.MaxAC)));
                        }
                        break;
                    case ItemType.Reel:
                        {
                            FishingAutoReelChance = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, FishingAutoReelChance + realItem.MaxMAC)));
                            successStat = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, successStat + realItem.MaxAC)));
                        }
                        break;
                    default:
                        break;
                }
            }
            FishingNibbleChance = 5 + Envir.Random.Next(nibbleMin, nibbleMax);

            if (cast) FishingChance = Settings.FishingSuccessStart + (int)successStat + (FishingChanceCounter != 0 ? Envir.Random.Next(failedAddSuccessMin, failedAddSuccessMax) : 0) + (FishingChanceCounter * Settings.FishingSuccessMultiplier); //10 //10
            if (FishingChanceCounter != 0) DamagedFishingItem(FishingSlot.Finder, 1);
            FishingChance += FishRate * 5;

            FishingChance = Math.Min(100, Math.Max(0, FishingChance));
            FishingNibbleChance = Math.Min(100, Math.Max(0, FishingNibbleChance));
            FishingAutoReelChance = Math.Min(100, Math.Max(0, FishingAutoReelChance));

            FishingTime = Envir.Time + FishingCastDelay + Settings.FishingDelay;

            if (cast)
            {
                if (Fishing) return;

                _fishCounter = 0;
                FishFound = false;

                UserItem item = GetBait(1);

                if (item == null)
                {
                    ReceiveChat(Envir.Tr("You need bait."), ChatType.System);
                    return;
                }

                ConsumeItem(item, 1);
                Fishing = true;
            }
            else
            {
                if (!Fishing)
                {
                    Enqueue(GetFishInfo());
                    return;
                }

                Fishing = false;

                if (FishingProgress > 99)
                {
                    FishingChanceCounter++;
                }

                if (FishFound)
                {
                    if (Level >= 33)
                        GainExp(Settings.FishingExpGain / 4);
                    else
                        GainExp(Settings.FishingExpGain);

                    int getChance = FishingChance + Envir.Random.Next(10, 24) + (FishingProgress > 50 ? flexibilityStat / 2 : 0);
                    getChance = Math.Min(100, Math.Max(0, getChance));

                    if (Envir.Random.Next(0, 100) <= getChance)
                    {
                        FishingChanceCounter = 0;

                        UserItem dropItem = null;

                        sbyte fishingAttr = CurrentMap.GetFishingAttribute(fishingPoint.X, fishingPoint.Y);
                        foreach (DropInfo drop in Envir.FishingDrops.Where(x => x.Type == fishingAttr))
                        {
                            int rate = (int)(drop.Chance / (Settings.DropRate));

                            if (EXPOwner != null && EXPOwner.ItemDropRateOffset > 0)
                                rate -= (int)(rate * (EXPOwner.ItemDropRateOffset / 100));

                            if (rate < 1) rate = 1;

                            if (Envir.Random.Next(rate) != 0) continue;

                            dropItem = Envir.CreateDropItem(drop.Item);
                            break;
                        }

                        if (dropItem == null)
                            ReceiveChat(Envir.Tr("Your fish got away!"), ChatType.System);
                        else if (FreeSpace(Info.Inventory) < 1)
                            ReceiveChat(Envir.Tr("You do not have enough space in your bag."), ChatType.System);
                        else
                        {
                            GainItem(dropItem);
                            Report.ItemChanged("FishedItem", dropItem, dropItem.Count, 2);
                        }

                        if (Envir.Random.Next(100 - Settings.FishingMobSpawnChance) == 0)
                        {
                            MonsterObject mob = MonsterObject.GetMonster(Envir.GetMonsterInfo(Settings.FishingMonster));

                            if (mob == null) return;

                            mob.Spawn(CurrentMap, Back);
                        }

                        DamagedFishingItem(FishingSlot.Reel, 1);

                        if (!FishingAutocast)
                            cancel = true;
                    }
                    else
                        ReceiveChat(Envir.Tr("Your fish got away!"), ChatType.System);
                }

                FishFound = false;
                FishFirstFound = false;
            }

            Enqueue(GetFishInfo());
            Broadcast(GetFishInfo());

            if (FishingAutocast && !cast && !cancel)
            {
                FishingTime = Envir.Time + (FishingCastDelay * 2);
                FishingFoundTime = Envir.Time;
                FishingAutoReelChance = 0;
                FishingNibbleChance = 0;
                FishFirstFound = false;

                FishingCast(true);
            }
        }
        public void FishingChangeAutocast(bool autoCast)
        {
            UserItem rod = Info.Equipment[(int)EquipmentSlot.Weapon];

            if (rod == null || (rod.Info.Shape != 49 && rod.Info.Shape != 50)) return;

            UserItem reel = rod.Slots[(int)FishingSlot.Reel];

            if (reel == null)
            {
                FishingAutocast = false;
                return;
            }

            FishingAutocast = autoCast;
        }
        public void UpdateFish()
        {
            if (FishFound != true && FishFirstFound != true)
            {
                FishFound = Envir.Random.Next(0, 100) <= FishingNibbleChance;
                FishingFoundTime = FishFound ? Envir.Time + 3000 : Envir.Time;

                if (FishFound)
                {
                    FishFirstFound = true;
                    DamagedFishingItem(FishingSlot.Float, 1);
                }
            }
            else
            {
                if (FishingAutoReelChance != 0 && Envir.Random.Next(0, 100) <= FishingAutoReelChance)
                {
                    FishingCast(false);
                }
            }

            if (FishingFoundTime < Envir.Time)
                FishFound = false;

            FishingTime = Envir.Time + FishingDelay;

            Enqueue(GetFishInfo());

            if (FishingProgress > 100)
            {
                FishingCast(false);
            }
        }
        Packet GetFishInfo()
        {
            FishingProgress = _fishCounter > 0 ? (int)(((decimal)_fishCounter / FishingProgressMax) * 100) : 0;

            return new S.FishingUpdate
            {
                ObjectID = ObjectID,
                Fishing = Fishing,
                ProgressPercent = FishingProgress,
                FishingPoint = Functions.PointMove(CurrentLocation, Direction, 3),
                ChancePercent = FishingChance,
                FoundFish = FishFound
            };
        }

        #endregion

        #region Quests

        public bool CheckAcceptQuest(int index, bool defaultNPC)
        {
            bool canAccept = true;

            if (CurrentQuests.Exists(e => e.Info.Index == index)) return false; //e.Info.NpcIndex == npcIndex && 

            QuestInfo info = Envir.QuestInfoList.FirstOrDefault(d => d.Index == index);

            if (!defaultNPC)
            {
                NPCObject npc = null;

                for (int i = CurrentMap.NPCs.Count - 1; i >= 0; i--)
                {
                    if (CurrentMap.NPCs[i].Info.Index != info.NpcIndex) continue;

                    if (!Functions.InRange(CurrentMap.NPCs[i].CurrentLocation, CurrentLocation, Globals.DataRange)) break;
                    npc = CurrentMap.NPCs[i];
                    break;
                }
                if (npc == null || !npc.VisibleLog[Info.Index] || !npc.Visible) return false;
            }

            if (!info.CanAccept(this))
            {
                canAccept = false;
            }

            if (CurrentQuests.Count >= Globals.MaxConcurrentQuests)
            {
                ReceiveChat(Envir.Tr("Maximum amount of quests already taken."), ChatType.System);
                return false;
            }

            if (CompletedQuests.Contains(index))
            {
                ReceiveChat(Envir.Tr("Quest has already been completed."), ChatType.System);
                return false;
            }

            //check previous chained quests have been completed
            QuestInfo tempInfo = info;
            while (tempInfo != null && tempInfo.RequiredQuest != 0)
            {
                if (!CompletedQuests.Contains(tempInfo.RequiredQuest))
                {
                    canAccept = false;
                    break;
                }

                tempInfo = Envir.QuestInfoList.FirstOrDefault(d => d.Index == tempInfo.RequiredQuest);
            }

            if (!canAccept)
            {
                ReceiveChat(Envir.Tr("Could not accept quest."), ChatType.System);
                return false;
            }

            return true;
        }

        public void AcceptQuest(int index, bool defaultNPC = false)
        {
            if (!CheckAcceptQuest(index, defaultNPC))
                return;

            QuestInfo info = Envir.QuestInfoList.FirstOrDefault(d => d.Index == index);
            if (info.CarryItems.Count > 0)
            {
                foreach (QuestItemTask carryItem in info.CarryItems)
                {
                    uint count = carryItem.Count;

                    while (count > 0)
                    {
                        UserItem item = SMain.Envir.CreateFreshItem(carryItem.Item);

                        if (item.Info.StackSize > count)
                        {
                            item.Count = count;
                            count = 0;
                        }
                        else
                        {
                            count -= item.Info.StackSize;
                            item.Count = item.Info.StackSize;
                        }

                        if (!CanGainQuestItem(item))
                        {
                            return;
                        }

                        GainQuestItem(item);

                        Report.ItemChanged("AcceptQuest", item, item.Count, 2);
                    }
                }
            }

            QuestProgressInfo quest = new QuestProgressInfo(index) { StartDateTime = DateTime.Now };

            CurrentQuests.Add(quest);
            SendUpdateQuest(quest, QuestState.Add, true);

            if (quest.ProcessItem(Info.Inventory))
                SendUpdateQuest(quest, QuestState.Update);

            CallDefaultNPC(DefaultNPCType.OnAcceptQuest, index);
        }

        public void UpdateQuestItem(ItemInfo itemInfo)
        {
            foreach (QuestProgressInfo quest in CurrentQuests.
               Where(e => e.ItemTaskCount.Count > 0).
               Where(e => e.ContainItem(itemInfo)))
            {
                if (quest.ProcessItem(Info.Inventory))
                    SendUpdateQuest(quest, QuestState.Update);
            }
        }

        public void FinishQuest(int questIndex, int selectedItemIndex = -1)
        {
            QuestProgressInfo quest = CurrentQuests.FirstOrDefault(e => e.Info.Index == questIndex);

            if (quest == null || !quest.Completed) return;

            NPCObject npc = null;

            for (int i = CurrentMap.NPCs.Count - 1; i >= 0; i--)
            {
                if (CurrentMap.NPCs[i].Info.Index != quest.Info.FinishNpcIndex) continue;

                if (!Functions.InRange(CurrentMap.NPCs[i].CurrentLocation, CurrentLocation, Globals.DataRange)) break;
                npc = CurrentMap.NPCs[i];
                break;
            }
            if (npc == null || !npc.VisibleLog[Info.Index] || !npc.Visible) return;

            List<UserItem> rewardItems = new List<UserItem>();

            foreach (var reward in quest.Info.FixedRewards)
            {
                uint count = reward.Count;

                UserItem rewardItem;

                while (count > 0)
                {
                    rewardItem = Envir.CreateFreshItem(reward.Item);
                    if (reward.Item.StackSize >= count)
                    {
                        rewardItem.Count = count;
                        count = 0;
                    }
                    else
                    {
                        rewardItem.Count = reward.Item.StackSize;
                        count -= reward.Item.StackSize;
                    }

                    rewardItems.Add(rewardItem);
                }
            }

            if (selectedItemIndex >= 0)
            {
                for (int i = 0; i < quest.Info.SelectRewards.Count; i++)
                {
                    if (selectedItemIndex != i) continue;

                    uint count = quest.Info.SelectRewards[i].Count;
                    UserItem rewardItem;

                    while (count > 0)
                    {
                        rewardItem = Envir.CreateFreshItem(quest.Info.SelectRewards[i].Item);
                        if (quest.Info.SelectRewards[i].Item.StackSize >= count)
                        {
                            rewardItem.Count = count;
                            count = 0;
                        }
                        else
                        {
                            rewardItem.Count = quest.Info.SelectRewards[i].Item.StackSize;
                            count -= quest.Info.SelectRewards[i].Item.StackSize;
                        }

                        rewardItems.Add(rewardItem);
                    }
                }
            }

            if (!CanGainItems(rewardItems.ToArray()))
            {
                ReceiveChat(Envir.Tr("Cannot hand in quest whilst bag is full."), ChatType.System);
                return;
            }

            if (quest.Info.Type != QuestType.Repeatable)
            {
                Info.CompletedQuests.Add(quest.Index);
                GetCompletedQuests();
            }

            CurrentQuests.Remove(quest);
            SendUpdateQuest(quest, QuestState.Remove);

            if (quest.Info.CarryItems.Count > 0)
            {
                foreach (QuestItemTask carryItem in quest.Info.CarryItems)
                {
                    TakeQuestItem(carryItem.Item, carryItem.Count);
                }
            }

            foreach (QuestItemTask iTask in quest.Info.ItemTasks)
            {
                TakeQuestItem(iTask.Item, Convert.ToUInt32(iTask.Count));
            }

            foreach (UserItem item in rewardItems)
            {
                GainItem(item);
            }

            GainGold(quest.Info.GoldReward);
            GainExp(quest.Info.ExpReward);
            GainCredit(quest.Info.CreditReward);

            CallDefaultNPC(DefaultNPCType.OnFinishQuest, questIndex);
        }
        public void AbandonQuest(int questIndex)
        {
            QuestProgressInfo quest = CurrentQuests.FirstOrDefault(e => e.Info.Index == questIndex);

            if (quest == null) return;

            CurrentQuests.Remove(quest);
            SendUpdateQuest(quest, QuestState.Remove);

        }
        public void ShareQuest(int questIndex)
        {
            bool shared = false;

            if (GroupMembers != null)
            {
                foreach (PlayerObject player in GroupMembers.
                    Where(player => player.CurrentMap == CurrentMap &&
                        Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) &&
                        !player.Dead && player != this))
                {
                    player.Enqueue(new S.ShareQuest { QuestIndex = questIndex, SharerName = Name });
                    shared = true;
                }
            }

            if (!shared)
            {
                ReceiveChat(Envir.Tr("Quest could not be shared with anyone."), ChatType.System);
            }
        }

        public void CheckGroupQuestKill(MonsterInfo mInfo)
        {
            if (GroupMembers != null)
            {
                foreach (PlayerObject player in GroupMembers.
                    Where(player => player.CurrentMap == CurrentMap &&
                        Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) &&
                        !player.Dead))
                {
                    player.CheckNeedQuestKill(mInfo);
                }
            }
            else
                CheckNeedQuestKill(mInfo);
        }
        public bool CheckGroupQuestItem(UserItem item, bool gainItem = true)
        {
            bool itemCollected = false;

            if (GroupMembers != null)
            {
                foreach (PlayerObject player in GroupMembers.
                    Where(player => player.CurrentMap == CurrentMap &&
                        Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) &&
                        !player.Dead))
                {
                    if (player.CheckNeedQuestItem(item, gainItem))
                    {
                        itemCollected = true;
                        player.Report.ItemChanged("WinQuestItem", item, item.Count, 2);
                    }
                }
            }
            else
            {
                if (CheckNeedQuestItem(item, gainItem))
                {
                    itemCollected = true;
                    Report.ItemChanged("WinQuestItem", item, item.Count, 2);
                }
            }

            return itemCollected;
        }

        public bool CheckNeedQuestItem(UserItem item, bool gainItem = true)
        {
            foreach (QuestProgressInfo quest in CurrentQuests.
                Where(e => e.ItemTaskCount.Count > 0).
                Where(e => e.NeedItem(item.Info)).
                Where(e => CanGainQuestItem(item)))
            {
                if (gainItem)
                {
                    GainQuestItem(item);
                    quest.ProcessItem(Info.Inventory);

                    Enqueue(new S.SendOutputMessage { Message = string.Format(Envir.Tr("You found {0}."), item.FriendlyName), Type = OutputMessageType.Quest });

                    SendUpdateQuest(quest, QuestState.Update);

                    Report.ItemChanged("WinQuestItem", item, item.Count, 2);
                }
                return true;
            }

            return false;
        }
        public bool CheckNeedQuestFlag(int flagNumber)
        {
            foreach (QuestProgressInfo quest in CurrentQuests.
                Where(e => e.FlagTaskSet.Count > 0).
                Where(e => e.NeedFlag(flagNumber)))
            {
                quest.ProcessFlag(Info.Flags);

                //Enqueue(new S.SendOutputMessage { Message = string.Format("Location visited."), Type = OutputMessageType.Quest });

                SendUpdateQuest(quest, QuestState.Update);
                return true;
            }

            return false;
        }
        public void CheckNeedQuestKill(MonsterInfo mInfo)
        {
            foreach (QuestProgressInfo quest in CurrentQuests.
                    Where(e => e.KillTaskCount.Count > 0).
                    Where(quest => quest.NeedKill(mInfo)))
            {
                quest.ProcessKill(mInfo);

                Enqueue(new S.SendOutputMessage { Message = string.Format(Envir.Tr("You killed {0}."), mInfo.GameName), Type = OutputMessageType.Quest });

                SendUpdateQuest(quest, QuestState.Update);
            }
        }

        public void SendUpdateQuest(QuestProgressInfo quest, QuestState state, bool trackQuest = false)
        {
            quest.CheckCompleted();

            Enqueue(new S.ChangeQuest
            {
                Quest = quest.CreateClientQuestProgress(),
                QuestState = state,
                TrackQuest = trackQuest
            });
        }

        public void GetCompletedQuests()
        {
            Enqueue(new S.CompleteQuest
            {
                CompletedQuests = CompletedQuests
            });
        }

        #endregion

        #region Mail

        public void SendMail(string name, string message)
        {
            CharacterInfo player = Envir.GetCharacterInfo(name);

            if (player == null)
            {
                ReceiveChat(string.Format(Envir.Tr("Could not find player {0}"), name), ChatType.System);
                return;
            }

            if (!Envir.CheckSameServer(player.AccountInfo.RegionID, GetRegionID()))
            {
                ReceiveChat(string.Format("不是同区玩家 {0}", name), ChatType.System);
                return;
            }

            if (player.Friends.Any(e => e.Info == Info && e.Blocked))
            {
                ReceiveChat(Envir.Tr("Player is not accepting your mail."), ChatType.System);
                return;
            }

            if (Info.Friends.Any(e => e.Info == player && e.Blocked))
            {
                ReceiveChat(Envir.Tr("Cannot mail player whilst they are on your blacklist."), ChatType.System);
                return;
            }

            if (player.Index == Info.Index)
            {
                ReceiveChat(Envir.Tr("Cannot send mail to yourself."), ChatType.System);
                return;
            }

            //sent from player
            MailInfo mail = new MailInfo(player.Index, true)
            {
                MailID = Envir.GenerateMailId(),
                Sender = Info.Name,
                Message = message,
                Gold = 0
            };

            mail.Send();
        }

        public void SendMail(string name, string message, uint gold, ulong[] items, bool stamped)
        {
            CharacterInfo player = Envir.GetCharacterInfo(name);

            if (player == null)
            {
                ReceiveChat(string.Format(Envir.Tr("Could not find player {0}"), name), ChatType.System);
                return;
            }

            if (!Envir.CheckSameServer(player.AccountInfo.RegionID, GetRegionID()))
            {
                ReceiveChat(string.Format("不是同区玩家 {0}", name), ChatType.System);
                return;
            }

            if (player.Index == Info.Index)
            {
                ReceiveChat(Envir.Tr("Cannot send mail to yourself."), ChatType.System);
                return;
            }

            bool hasStamp = false;
            uint totalGold = 0;
            uint parcelCost = GetMailCost(items, gold, stamped);

            totalGold = gold + parcelCost;

            if (Account.Gold < totalGold || Account.Gold < gold || gold > totalGold)
            {
                Enqueue(new S.MailSent { Result = -1 });
                return;
            }

            //Validate user has stamp
            if (stamped)
            {
                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem item = Info.Inventory[i];

                    if (item == null || item.Info.Type != ItemType.Nothing || item.Info.Shape != 1 || item.Count < 1) continue;

                    hasStamp = true;

                    if (item.Count > 1) item.Count--;
                    else Info.Inventory[i] = null;

                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = 1 });
                    UpdateQuestItem(item.Info);
                    break;
                }
            }

            List<UserItem> giftItems = new List<UserItem>();

            for (int j = 0; j < (hasStamp ? 5 : 1); j++)
            {
                if (items[j] < 1) continue;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    UserItem item = Info.Inventory[i];

                    if (item == null || items[j] != item.UniqueID) continue;

                    if (item.Info.Bind.HasFlag(BindMode.DontTrade))
                    {
                        ReceiveChat(string.Format(Envir.Tr("{0} cannot be mailed"), item.FriendlyName), ChatType.System);
                        return;
                    }

                    if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.DontTrade))
                    {
                        ReceiveChat(string.Format(Envir.Tr("{0} cannot be mailed"), item.FriendlyName), ChatType.System);
                        return;
                    }

                    giftItems.Add(item);

                    Info.Inventory[i] = null;
                    Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    UpdateQuestItem(item.Info);
                }
            }

            if (totalGold > 0)
            {
                Account.Gold -= totalGold;
                Enqueue(new S.LoseGold { Gold = totalGold });
            }

            //Create parcel
            MailInfo mail = new MailInfo(player.Index, true)
            {
                MailID = Envir.GenerateMailId(),
                Sender = Info.Name,
                Message = message,
                Gold = gold,
                Items = giftItems
            };

            mail.Send();
            mail.Collected = false;

            Enqueue(new S.MailSent { Result = 1 });
        }

        public void ReadMail(ulong mailID)
        {
            MailInfo mail = Info.Mail.SingleOrDefault(e => e.MailID == mailID);

            if (mail == null) return;

            mail.DateOpened = DateTime.Now;

            GetMail();
        }

        public void CollectMail(ulong mailID)
        {
            MailInfo mail = Info.Mail.SingleOrDefault(e => e.MailID == mailID);

            if (mail == null) return;

            if (!mail.Collected)
            {
                ReceiveChat(Envir.Tr("Mail must be collected from the post office."), ChatType.System);
                return;
            }

            if (mail.Items.Count > 0)
            {
                if (!CanGainItems(mail.Items.ToArray()))
                {
                    ReceiveChat(Envir.Tr("Cannot collect items when bag is full."), ChatType.System);
                    return;
                }

                for (int i = 0; i < mail.Items.Count; i++)
                {
                    GainItem(mail.Items[i]);
                    Report.ItemChanged("MailGet", mail.Items[i], mail.Items[i].Count, 2);
                }
            }

            if (mail.Gold > 0)
            {
                uint count = mail.Gold;

                if (count + Account.Gold >= uint.MaxValue)
                    count = uint.MaxValue - Account.Gold;

                GainGold(count);
            }

            mail.Items = new List<UserItem>();
            mail.Gold = 0;

            mail.Collected = true;

            Enqueue(new S.ParcelCollected { Result = 1 });

            GetMail();
        }

        public void DeleteMail(ulong mailID)
        {
            MailInfo mail = Info.Mail.SingleOrDefault(e => e.MailID == mailID);

            if (mail == null) return;

            Info.Mail.Remove(mail);

            GetMail();
        }

        public void LockMail(ulong mailID, bool lockMail)
        {
            MailInfo mail = Info.Mail.SingleOrDefault(e => e.MailID == mailID);

            if (mail == null) return;

            mail.Locked = lockMail;

            GetMail();
        }

        public uint GetMailCost(ulong[] items, uint gold, bool stamped)
        {
            uint cost = 0;

            if (!Settings.MailFreeWithStamp || !stamped)
            {
                if (gold > 0 && Settings.MailCostPer1KGold > 0)
                {
                    cost += (uint)Math.Floor((decimal)gold / 1000) * Settings.MailCostPer1KGold;
                }

                if (items != null && items.Length > 0 && Settings.MailItemInsurancePercentage > 0)
                {
                    for (int j = 0; j < (stamped ? 5 : 1); j++)
                    {
                        if (items[j] < 1) continue;

                        for (int i = 0; i < Info.Inventory.Length; i++)
                        {
                            UserItem item = Info.Inventory[i];

                            if (item == null || items[j] != item.UniqueID) continue;

                            cost += (uint)Math.Floor((double)item.Price() / 100 * Settings.MailItemInsurancePercentage);
                        }
                    }
                }
            }


            return cost;
        }

        public void GetMail()
        {
            List<ClientMail> mail = new List<ClientMail>();

            int start = (Info.Mail.Count - Settings.MailCapacity) > 0 ? (Info.Mail.Count - (int)Settings.MailCapacity) : 0;

            for (int i = start; i < Info.Mail.Count; i++)
            {
                foreach (UserItem itm in Info.Mail[i].Items)
                {
                    CheckItem(itm);
                }

                mail.Add(Info.Mail[i].CreateClientMail());
            }

            //foreach (MailInfo m in Info.Mail)
            //{
            //    foreach (UserItem itm in m.Items)
            //    {
            //        CheckItem(itm);
            //    }

            //    mail.Add(m.CreateClientMail());
            //}

            NewMail = false;

            Enqueue(new S.ReceiveMail { Mail = mail });
        }

        public int GetMailAwaitingCollectionAmount()
        {
            int count = 0;
            for (int i = 0; i < Info.Mail.Count; i++)
            {
                if (!Info.Mail[i].Collected) count++;
            }

            return count;
        }

        #endregion

        #region IntelligentCreatures

        public void SummonIntelligentCreature(IntelligentCreatureType pType)
        {
            if (pType == IntelligentCreatureType.None) return;

            if (Dead) return;

            if (CreatureSummoned == true || SummonedCreatureType != IntelligentCreatureType.None) return;

            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;

                MonsterInfo mInfo = Envir.GetMonsterInfo(Settings.IntelligentCreatureNameList[(byte)pType]);
                if (mInfo == null) return;

                byte petlevel = 0;//for future use

                MonsterObject monster = MonsterObject.GetMonster(mInfo);
                if (monster == null) return;
                monster.PetLevel = petlevel;
                monster.Master = this;
                monster.MaxPetLevel = 7;
                monster.Direction = Direction;
                monster.ActionTime = Envir.Time + 1000;
                ((IntelligentCreatureObject)monster).CustomName = Info.IntelligentCreatures[i].CustomName;
                ((IntelligentCreatureObject)monster).CreatureRules = new IntelligentCreatureRules
                {
                    MinimalFullness = Info.IntelligentCreatures[i].Info.MinimalFullness,
                    MousePickupEnabled = Info.IntelligentCreatures[i].Info.MousePickupEnabled,
                    MousePickupRange = Info.IntelligentCreatures[i].Info.MousePickupRange,
                    AutoPickupEnabled = Info.IntelligentCreatures[i].Info.AutoPickupEnabled,
                    AutoPickupRange = Info.IntelligentCreatures[i].Info.AutoPickupRange,
                    SemiAutoPickupEnabled = Info.IntelligentCreatures[i].Info.SemiAutoPickupEnabled,
                    SemiAutoPickupRange = Info.IntelligentCreatures[i].Info.SemiAutoPickupRange,
                    CanProduceBlackStone = Info.IntelligentCreatures[i].Info.CanProduceBlackStone
                };
                ((IntelligentCreatureObject)monster).ItemFilter = Info.IntelligentCreatures[i].Filter;
                ((IntelligentCreatureObject)monster).CurrentPickupMode = Info.IntelligentCreatures[i].petMode;
                ((IntelligentCreatureObject)monster).Fullness = Info.IntelligentCreatures[i].Fullness;
                ((IntelligentCreatureObject)monster).blackstoneTime = Info.IntelligentCreatures[i].BlackstoneTime;
                ((IntelligentCreatureObject)monster).maintainfoodTime = Info.IntelligentCreatures[i].MaintainFoodTime;

                if (!CurrentMap.ValidPoint(Front)) return;
                monster.Spawn(CurrentMap, Front);
                Pets.Add(monster);//make a new creaturelist ? 

                CreatureSummoned = true;
                SummonedCreatureType = pType;

                ReceiveChat((string.Format(Envir.Tr("Creature {0} has been summoned."), Info.IntelligentCreatures[i].CustomName)), ChatType.System);
                break;
            }
            //update client
            GetCreaturesInfo();
        }
        public void UnSummonIntelligentCreature(IntelligentCreatureType pType, bool doUpdate = true)
        {
            if (pType == IntelligentCreatureType.None) return;

            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != pType) continue;

                if (doUpdate) ReceiveChat((string.Format(Envir.Tr("Creature {0} has been dismissed."), 
                    ((IntelligentCreatureObject)Pets[i]).CustomName)), ChatType.System);

                Pets[i].Die();

                CreatureSummoned = false;
                SummonedCreatureType = IntelligentCreatureType.None;
                break;
            }
            //update client
            if (doUpdate) GetCreaturesInfo();
        }
        public void ReleaseIntelligentCreature(IntelligentCreatureType pType, bool doUpdate = true)
        {
            if (pType == IntelligentCreatureType.None) return;

            //remove creature
            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;

                if (doUpdate) ReceiveChat((string.Format(Envir.Tr("Creature {0} has been released."), Info.IntelligentCreatures[i].CustomName)), ChatType.System);

                Info.IntelligentCreatures.Remove(Info.IntelligentCreatures[i]);
                break;
            }

            //re-arange slots
            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
                Info.IntelligentCreatures[i].SlotIndex = i;

            //update client
            if (doUpdate) GetCreaturesInfo();
        }

        public void UpdateSummonedCreature(IntelligentCreatureType pType)
        {
            if (pType == IntelligentCreatureType.None) return;

            UserIntelligentCreature creatureInfo = null;
            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;

                creatureInfo = Info.IntelligentCreatures[i];
                break;
            }
            if (creatureInfo == null) return;

            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != pType) continue;

                ((IntelligentCreatureObject)Pets[i]).CustomName = creatureInfo.CustomName;
                ((IntelligentCreatureObject)Pets[i]).ItemFilter = creatureInfo.Filter;
                ((IntelligentCreatureObject)Pets[i]).CurrentPickupMode = creatureInfo.petMode;
                break;
            }
        }
        public void UpdateCreatureFullness(IntelligentCreatureType pType, int fullness)
        {
            if (pType == IntelligentCreatureType.None) return;

            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;
                Info.IntelligentCreatures[i].Fullness = fullness;
                break;
            }

        }

        public void UpdateCreatureBlackstoneTime(IntelligentCreatureType pType, long blackstonetime)
        {
            if (pType == IntelligentCreatureType.None) return;

            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;
                Info.IntelligentCreatures[i].BlackstoneTime = blackstonetime;
                break;
            }

            //update client
            //GetCreaturesInfo();
        }

        public void UpdateCreatureMaintainFoodTime(IntelligentCreatureType pType, long maintainfoodtime)
        {
            if (pType == IntelligentCreatureType.None) return;

            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
            {
                if (Info.IntelligentCreatures[i].PetType != pType) continue;
                Info.IntelligentCreatures[i].MaintainFoodTime = maintainfoodtime;
                break;
            }

            //update client
            //GetCreaturesInfo();
        }

        public void RefreshCreaturesTimeLeft()
        {
            if (Envir.Time > CreatureTimeLeftTicker)
            {
                //Make sure summoned vars are in correct state
                RefreshCreatureSummoned();

                //ExpireTime
                List<int> releasedPets = new List<int>();
                CreatureTimeLeftTicker = Envir.Time + CreatureTimeLeftDelay;
                for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
                {
                    if (Info.IntelligentCreatures[i].ExpireTime == -9999) continue;//permanent
                    Info.IntelligentCreatures[i].ExpireTime = Info.IntelligentCreatures[i].ExpireTime - 1;
                    if (Info.IntelligentCreatures[i].ExpireTime <= 0)
                    {
                        Info.IntelligentCreatures[i].ExpireTime = 0;
                        if (CreatureSummoned && SummonedCreatureType == Info.IntelligentCreatures[i].PetType)
                            UnSummonIntelligentCreature(SummonedCreatureType, false);//unsummon creature
                        releasedPets.Add(i);
                    }
                }
                for (int i = (releasedPets.Count - 1); i >= 0; i--)//start with largest value
                {
                    ReceiveChat((string.Format(Envir.Tr("Creature {0} has expired."), Info.IntelligentCreatures[releasedPets[i]].CustomName)), ChatType.System);
                    ReleaseIntelligentCreature(Info.IntelligentCreatures[releasedPets[i]].PetType, false);//release creature
                }

                if (CreatureSummoned && SummonedCreatureType != IntelligentCreatureType.None)
                {
                    for (int i = 0; i < Pets.Count; i++)
                    {
                        if (Pets[i].Info.AI != 64) continue;
                        if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;

                        ((IntelligentCreatureObject)Pets[i]).ProcessBlackStoneProduction();
                        ((IntelligentCreatureObject)Pets[i]).ProcessMaintainFoodBuff();
                        break;
                    }
                }

                //update client
            }
        }
        public void RefreshCreatureSummoned()
        {
            if (SummonedCreatureType == IntelligentCreatureType.None || !CreatureSummoned)
            {
                //make sure both are in the unsummoned state
                CreatureSummoned = false;
                SummonedCreatureType = IntelligentCreatureType.None;
                return;
            }
            bool petFound = false;
            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;
                petFound = true;
                break;
            }
            if (!petFound)
            {
                SMain.EnqueueDebugging(string.Format(Envir.Tr("{0}: SummonedCreature no longer exists?!?. {1}"), Name, SummonedCreatureType.ToString()));
                CreatureSummoned = false;
                SummonedCreatureType = IntelligentCreatureType.None;
            }
        }

        public void IntelligentCreaturePickup(bool mousemode, Point atlocation)
        {
            if (!CreatureSummoned) return;

            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;

                //((IntelligentCreatureObject)Pets[i]).MouseLocation = atlocation;
                ((IntelligentCreatureObject)Pets[i]).ManualPickup(mousemode, atlocation);
                break;
            }
        }

        public void IntelligentCreatureGainPearls(int amount)
        {
            Info.PearlCount += amount;
            if (Info.PearlCount > int.MaxValue) Info.PearlCount = int.MaxValue;
        }

        public void IntelligentCreatureLosePearls(int amount)
        {
            Info.PearlCount -= amount;
            if (Info.PearlCount < 0) Info.PearlCount = 0;
        }

        public void IntelligentCreatureProducePearl()
        {
            Info.PearlCount++;
        }
        public bool IntelligentCreatureProduceBlackStone()
        {
            ItemInfo iInfo = Envir.GetItemInfo(Settings.CreatureBlackStoneName);
            if (iInfo == null) return false;

            UserItem item = Envir.CreateDropItem(iInfo);
            item.Count = 1;

            if (!CanGainItem(item, false)) return false;

            GainItem(item);
            return true;
        }

        public void IntelligentCreatureSay(IntelligentCreatureType pType, string message)
        {
            if (!CreatureSummoned || message == "") return;
            if (pType != SummonedCreatureType) return;

            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != pType) continue;

                Enqueue(new S.ObjectChat { ObjectID = Pets[i].ObjectID, Text = message, Type = ChatType.Normal });
                return;
            }
        }

        public void StrongboxRewardItem(int boxtype)
        {
            int highRate = int.MaxValue;
            UserItem dropItem = null;

            foreach (DropInfo drop in Envir.StrongboxDrops)
            {
                int rate = (int)(Envir.Random.Next(0, drop.Chance) / Settings.DropRate);
                if (rate < 1) rate = 1;

                if (highRate > rate)
                {
                    highRate = rate;
                    dropItem = Envir.CreateFreshItem(drop.Item);
                }
            }

            if (dropItem == null)
            {
                ReceiveChat(Envir.Tr("Nothing found."), ChatType.System);
                return;
            }

            if (dropItem.Info.Type == ItemType.Pets && dropItem.Info.Shape == 26)
            {
                dropItem = CreateDynamicWonderDrug(boxtype, dropItem);
            }
            else
                dropItem = Envir.CreateDropItem(dropItem.Info);

            if (FreeSpace(Info.Inventory) < 1)
            {
                ReceiveChat(Envir.Tr("No more space."), ChatType.System);
                return;
            }

            if (dropItem != null) GainItem(dropItem);
        }

        public void BlackstoneRewardItem()
        {
            int highRate = int.MaxValue;
            UserItem dropItem = null;
            foreach (DropInfo drop in Envir.BlackstoneDrops)
            {
                int rate = (int)(Envir.Random.Next(0, drop.Chance) / Settings.DropRate); if (rate < 1) rate = 1;

                if (highRate > rate)
                {
                    highRate = rate;
                    dropItem = Envir.CreateDropItem(drop.Item);
                }
            }
            if (FreeSpace(Info.Inventory) < 1)
            {
                ReceiveChat(Envir.Tr("No more space."), ChatType.System);
                return;
            }
            if (dropItem != null) GainItem(dropItem);
        }

        private UserItem CreateDynamicWonderDrug(int boxtype, UserItem dropitem)
        {
            dropitem.CurrentDura = (ushort)1;//* 3600
            switch ((int)dropitem.Info.Effect)
            {
                case 0://exp low/med/high
                    dropitem.Luck = (sbyte)5;
                    if (boxtype > 0) dropitem.Luck = (sbyte)10;
                    if (boxtype > 1) dropitem.Luck = (sbyte)20;
                    break;
                case 1://drop low/med/high
                    dropitem.Luck = (sbyte)10;
                    if (boxtype > 0) dropitem.Luck = (sbyte)20;
                    if (boxtype > 1) dropitem.Luck = (sbyte)50;
                    break;
                case 2://hp low/med/high
                    dropitem.HP = (byte)50;
                    if (boxtype > 0) dropitem.HP = (byte)100;
                    if (boxtype > 1) dropitem.HP = (byte)200;
                    break;
                case 3://mp low/med/high
                    dropitem.MP = (byte)50;
                    if (boxtype > 0) dropitem.MP = (byte)100;
                    if (boxtype > 1) dropitem.MP = (byte)200;
                    break;
                case 4://ac low/med/high
                    dropitem.AC = (byte)1;
                    if (boxtype > 0) dropitem.AC = (byte)3;
                    if (boxtype > 1) dropitem.AC = (byte)5;
                    break;
                case 5://amc low/med/high
                    dropitem.MAC = (byte)1;
                    if (boxtype > 0) dropitem.MAC = (byte)3;
                    if (boxtype > 1) dropitem.MAC = (byte)5;
                    break;
                case 6://speed low/med/high
                    dropitem.AttackSpeed = (sbyte)2;
                    if (boxtype > 0) dropitem.AttackSpeed = (sbyte)3;
                    if (boxtype > 1) dropitem.AttackSpeed = (sbyte)4;
                    break;
            }
            //string dbg = String.Format(" Img: {0} Effect: {1} Dura: {2} Exp: {3} Drop: {3} HP: {4} MP: {5} AC: {6} MAC: {7} ASpeed: {8} BagWeight: {9}", dropitem.Image, dropitem.Info.Effect, dropitem.CurrentDura, dropitem.Luck, dropitem.HP, dropitem.MP, dropitem.AC, dropitem.MAC, dropitem.AttackSpeed, dropitem.Luck);
            //ReceiveChat(dropitem.Name + dbg, ChatType.System);
            return dropitem;
        }

        private IntelligentCreatureObject GetCreatureByName(string creaturename)
        {
            if (!CreatureSummoned || creaturename == "") return null;
            if (SummonedCreatureType == IntelligentCreatureType.None) return null;

            for (int i = 0; i < Pets.Count; i++)
            {
                if (Pets[i].Info.AI != 64) continue;
                if (((IntelligentCreatureObject)Pets[i]).PetType != SummonedCreatureType) continue;

                return ((IntelligentCreatureObject)Pets[i]);
            }
            return null;
        }

        private string CreateTimeString(double secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            string answer;
            if (t.TotalMinutes < 1.0)
            {
                answer = string.Format("{0}s", t.Seconds);
            }
            else if (t.TotalHours < 1.0)
            {
                answer = string.Format("{0}m", t.Minutes);
            }
            else if (t.TotalDays < 1.0)
            {
                answer = string.Format("{0}h {1:D2}m", (int)t.TotalHours, t.Minutes);
            }
            else // t.TotalDays >= 1.0
            {
                answer = string.Format("{0}d {1}h {2:D2}m", (int)t.TotalDays, (int)t.Hours, t.Minutes);
            }
            return answer;
        }

        private void GetCreaturesInfo()
        {
            if (Info.IntelligentCreatures.Count == 0)
                return;

            S.UpdateIntelligentCreatureList packet = new S.UpdateIntelligentCreatureList
            {
                CreatureSummoned = CreatureSummoned,
                SummonedCreatureType = SummonedCreatureType,
                PearlCount = Info.PearlCount,
            };

            for (int i = 0; i < Info.IntelligentCreatures.Count; i++)
                packet.CreatureList.Add(Info.IntelligentCreatures[i].CreateClientIntelligentCreature());

            Enqueue(packet);
        }


        #endregion

        #region Friends

        public void AddFriend(string name, bool blocked = false)
        {
            CharacterInfo info = Envir.GetCharacterInfo(name);

            if (info == null)
            {
                ReceiveChat(Envir.Tr("Player doesn't exist"), ChatType.System);
                return;
            }

            if (Name == name)
            {
                ReceiveChat(Envir.Tr("Cannot add yourself"), ChatType.System);
                return;
            }

            if (Info.Friends.Any(e => e.Index == info.Index))
            {
                ReceiveChat(Envir.Tr("Player already added"), ChatType.System);
                return;
            }

            FriendInfo friend = new FriendInfo(info, blocked);

            Info.Friends.Add(friend);

            GetFriends();
        }

        public void RemoveFriend(int index)
        {
            FriendInfo friend = Info.Friends.FirstOrDefault(e => e.Index == index);

            if (friend == null)
            {
                return;
            }

            Info.Friends.Remove(friend);

            GetFriends();
        }

        public void AddMemo(int index, string memo)
        {
            if (memo.Length > 200) return;

            FriendInfo friend = Info.Friends.FirstOrDefault(e => e.Index == index);

            if (friend == null)
            {
                return;
            }

            friend.Memo = memo;

            GetFriends();
        }

        public void GetFriends()
        {
            List<ClientFriend> friends = new List<ClientFriend>();

            foreach (FriendInfo friend in Info.Friends)
            {
                var info = friend.CreateClientFriend();
                if (info == null)
                    continue;

                friends.Add(info);
            }

            Enqueue(new S.FriendUpdate { Friends = friends });
        }

        #endregion

        #region Refining

        public void DepositRefineItem(int from, int to)
        {

            S.DepositRefineItem p = new S.DepositRefineItem { From = from, To = to, Success = false };

            if (NPCPage == null || !String.Equals(NPCPage.Key, NPCObject.RefineKey, StringComparison.CurrentCultureIgnoreCase))
            {
                Enqueue(p);
                return;
            }
            NPCObject ob = null;
            for (int i = 0; i < CurrentMap.NPCs.Count; i++)
            {
                if (CurrentMap.NPCs[i].ObjectID != NPCID) continue;
                ob = CurrentMap.NPCs[i];
                break;
            }

            if (ob == null || !Functions.InRange(ob.CurrentLocation, CurrentLocation, Globals.DataRange))
            {
                Enqueue(p);
                return;
            }


            if (from < 0 || from >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Refine.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Info.Inventory[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (Info.Refine[to] == null)
            {
                Info.Refine[to] = temp;
                Info.Inventory[from] = null;
                RefreshBagWeight();

                Report.ItemMoved("DepositRefineItems", temp, MirGridType.Inventory, MirGridType.Refine, from, to);

                p.Success = true;
                Enqueue(p);
                return;
            }
            Enqueue(p);

        }
        public void RetrieveRefineItem(int from, int to)
        {
            S.RetrieveRefineItem p = new S.RetrieveRefineItem { From = from, To = to, Success = false };

            if (from < 0 || from >= Info.Refine.Length)
            {
                Enqueue(p);
                return;
            }

            if (to < 0 || to >= Info.Inventory.Length)
            {
                Enqueue(p);
                return;
            }

            UserItem temp = Info.Refine[from];

            if (temp == null)
            {
                Enqueue(p);
                return;
            }

            if (temp.Weight + CurrentBagWeight > MaxBagWeight)
            {
                ReceiveChat(Envir.Tr("Too heavy to get back."), ChatType.System);
                Enqueue(p);
                return;
            }

            if (Info.Inventory[to] == null)
            {
                Info.Inventory[to] = temp;
                Info.Refine[from] = null;

                Report.ItemMoved("TakeBackRefineItems", temp, MirGridType.Refine, MirGridType.Inventory, from, to);

                p.Success = true;
                RefreshBagWeight();
                Enqueue(p);

                return;
            }
            Enqueue(p);
        }
        public void RefineCancel()
        {
            for (int t = 0; t < Info.Refine.Length; t++)
            {
                UserItem temp = Info.Refine[t];

                if (temp == null) continue;

                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    if (Info.Inventory[i] != null) continue;

                    //Put item back in inventory
                    if (CanGainItem(temp))
                    {
                        RetrieveRefineItem(t, i);
                    }
                    else //Drop item on floor if it can no longer be stored
                    {
                        if (DropItem(temp, Settings.DropRange))
                        {
                            Enqueue(new S.DeleteItem { UniqueID = temp.UniqueID, Count = temp.Count });
                        }
                    }

                    Info.Refine[t] = null;
                    UpdateQuestItem(temp.Info);
                    break;
                }
            }
        }
        public void RefineItem(ulong uniqueID)
        {
            Enqueue(new S.RepairItem { UniqueID = uniqueID }); //CHECK THIS.

            if (Dead) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.RefineKey, StringComparison.CurrentCultureIgnoreCase))) return;

            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] == null || Info.Inventory[i].UniqueID != uniqueID) continue;
                index = i;
                break;
            }

            if (index == -1) return;

            if (Info.Inventory[index].RefineAdded != 0)
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} needs to be checked before you can attempt to refine it again."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }

            if ((Info.Inventory[index].Info.Type != ItemType.Weapon) && (Settings.OnlyRefineWeapon))
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} can't be refined."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }

            if (Info.Inventory[index].Info.Bind.HasFlag(BindMode.DontUpgrade))
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} can't be refined."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }

            if (Info.Inventory[index].RentalInformation != null && Info.Inventory[index].RentalInformation.BindingFlags.HasFlag(BindMode.DontUpgrade))
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} can't be refined."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }


            if (index == -1) return;




            //CHECK GOLD HERE
            uint cost = (uint)((Info.Inventory[index].Info.RequiredAmount * 10) * Settings.RefineCost);

            if (cost > Account.Gold)
            {
                ReceiveChat(String.Format(Envir.Tr("You don't have enough gold to refine your {0}."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }

            Account.Gold -= cost;
            Enqueue(new S.LoseGold { Gold = cost });

            //START OF FORMULA

            Info.CurrentRefine = Info.Inventory[index];
            Info.Inventory[index] = null;
            Info.CollectTime = (Envir.Time + (Settings.RefineTime * Settings.Minute));
            Enqueue(new S.RefineItem { UniqueID = uniqueID });


            short OrePurity = 0;                // 纯度
            byte OreAmount = 0;                 // 矿石数量
            byte ItemAmount = 0;                // 物品数量
            short TotalDC = 0;
            short TotalMC = 0;
            short TotalSC = 0;
            short RequiredLevel = 0;
            short Durability = 0;
            short CurrentDura = 0;
            short AddedStats = 0;
            UserItem Ingredient;

            for (int i = 0; i < Info.Refine.Length; i++)
            {
                Ingredient = Info.Refine[i];

                if (Ingredient == null) continue;
                if (Ingredient.Info.Type == ItemType.Weapon)
                {
                    Info.Refine[i] = null;
                    continue;
                }

                if ((Ingredient.Info.MaxDC > 0) || (Ingredient.Info.MaxMC > 0) || (Ingredient.Info.MaxSC > 0))
                {
                    TotalDC += (short)(Ingredient.Info.MinDC + Ingredient.Info.MaxDC + Ingredient.DC);
                    TotalMC += (short)(Ingredient.Info.MinMC + Ingredient.Info.MaxMC + Ingredient.MC);
                    TotalSC += (short)(Ingredient.Info.MinSC + Ingredient.Info.MaxSC + Ingredient.SC);
                    RequiredLevel += Ingredient.Info.RequiredAmount;
                    if (Math.Round(Ingredient.MaxDura / 1000M) == Math.Round(Ingredient.Info.Durability / 1000M)) Durability++;
                    if (Math.Round(Ingredient.CurrentDura / 1000M) == Math.Round(Ingredient.MaxDura / 1000M)) CurrentDura++;
                    ItemAmount++;
                }

                if (Ingredient.Info.FriendlyName == Settings.RefineOreName)
                {
                    OrePurity += (short)Math.Round(Ingredient.CurrentDura / 1000M);
                    OreAmount++;
                }

                Info.Refine[i] = null;
            }

            if ((TotalDC == 0) && (TotalMC == 0) && (TotalSC == 0))
            {
                Info.CurrentRefine.RefinedValue = RefinedValue.None;
                Info.CurrentRefine.RefineAdded = Settings.RefineIncrease;
                if (Settings.RefineTime == 0)
                {
                    CollectRefine();
                }
                else
                {
                    ReceiveChat(String.Format(Envir.Tr("Your {0} is now being refined, please check back in {1} minute(s)."), Info.CurrentRefine.FriendlyName, Settings.RefineTime), ChatType.System);
                }
                return;
            }

            if (OreAmount == 0)
            {
                Info.CurrentRefine.RefinedValue = RefinedValue.None;
                Info.CurrentRefine.RefineAdded = Settings.RefineIncrease;
                if (Settings.RefineTime == 0)
                {
                    CollectRefine();
                }
                else
                {
                    ReceiveChat(String.Format(Envir.Tr("Your {0} is now being refined, please check back in {1} minute(s)."), Info.CurrentRefine.FriendlyName, Settings.RefineTime), ChatType.System);
                }
                return;
            }


            short RefineStat = 0;

            if ((TotalDC > TotalMC) && (TotalDC > TotalSC))
            {
                Info.CurrentRefine.RefinedValue = RefinedValue.DC;
                RefineStat = TotalDC;
            }

            if ((TotalMC > TotalDC) && (TotalMC > TotalSC))
            {
                Info.CurrentRefine.RefinedValue = RefinedValue.MC;
                RefineStat = TotalMC;
            }

            if ((TotalSC > TotalDC) && (TotalSC > TotalMC))
            {
                Info.CurrentRefine.RefinedValue = RefinedValue.SC;
                RefineStat = TotalSC;
            }

            Info.CurrentRefine.RefineAdded = Settings.RefineIncrease;


            int ItemSuccess = 0; //Chance out of 35%

            ItemSuccess += (RefineStat * 5) - Info.CurrentRefine.Info.RequiredAmount;
            ItemSuccess += 5;
            if (ItemSuccess > 10) ItemSuccess = 10;
            if (ItemSuccess < 0) ItemSuccess = 0; //10%


            if ((RequiredLevel / ItemAmount) > (Info.CurrentRefine.Info.RequiredAmount - 5)) ItemSuccess += 10; //20%
            if (Durability == ItemAmount) ItemSuccess += 10; //30%
            if (CurrentDura == ItemAmount) ItemSuccess += 5; //35%

            int OreSuccess = 0; //Chance out of 35%

            if (OreAmount >= ItemAmount) OreSuccess += 15; //15%
            if ((OrePurity / OreAmount) >= (RefineStat / ItemAmount)) OreSuccess += 15; //30%
            if (OrePurity == RefineStat) OreSuccess += 5; //35%

            int LuckSuccess = 0; //Chance out of 10%

            LuckSuccess = (Info.CurrentRefine.Luck + 5);
            if (LuckSuccess > 10) LuckSuccess = 10;
            if (LuckSuccess < 0) LuckSuccess = 0;


            int BaseSuccess = Settings.RefineBaseChance; //20% as standard

            int SuccessChance = (ItemSuccess + OreSuccess + LuckSuccess + BaseSuccess);

            AddedStats = (byte)(Info.CurrentRefine.DC + Info.CurrentRefine.MC + Info.CurrentRefine.SC);
            if (Info.CurrentRefine.Info.Type == ItemType.Weapon) AddedStats = (short)(AddedStats * Settings.RefineWepStatReduce);
            else AddedStats = (short)(AddedStats * Settings.RefineItemStatReduce);
            if (AddedStats > 50) AddedStats = 50;

            SuccessChance -= AddedStats;


            if (Envir.Random.Next(1, 100) > SuccessChance)
                Info.CurrentRefine.RefinedValue = RefinedValue.None;

            if (Envir.Random.Next(1, 100) < Settings.RefineCritChance)
                Info.CurrentRefine.RefineAdded = (byte)(Info.CurrentRefine.RefineAdded * Settings.RefineCritIncrease);

            //END OF FORMULA (SET REFINEDVALUE TO REFINEDVALUE.NONE) REFINEADDED SHOULD BE > 0

            if (Settings.RefineTime == 0)
            {
                CollectRefine();
            }
            else
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} is now being refined, please check back in {1} minute(s)."), Info.CurrentRefine.FriendlyName, Settings.RefineTime), ChatType.System);
            }
        }
        public void CollectRefine()
        {
            S.NPCCollectRefine p = new S.NPCCollectRefine { Success = false };

            if (Info.CurrentRefine == null)
            {
                ReceiveChat(Envir.Tr("You aren't currently refining any items."), ChatType.System);
                Enqueue(p);
                return;
            }

            if (Info.CollectTime > Envir.Time)
            {
                ReceiveChat(string.Format(Envir.Tr("Your {0} will be ready to collect in {1} minute(s)."), Info.CurrentRefine.FriendlyName, ((Info.CollectTime - Envir.Time) / Settings.Minute)), ChatType.System);
                Enqueue(p);
                return;
            }


            if (Info.CurrentRefine.Info.Weight + CurrentBagWeight > MaxBagWeight)
            {
                ReceiveChat(string.Format(Envir.Tr("Your {0} is too heavy to get back, try again after reducing your bag weight."), Info.CurrentRefine.FriendlyName), ChatType.System);
                Enqueue(p);
                return;
            }

            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] != null) continue;
                index = i;
                break;
            }

            if (index == -1)
            {
                ReceiveChat(String.Format(Envir.Tr("There isn't room in your bag for your {0}, make some space and try again."), Info.CurrentRefine.FriendlyName), ChatType.System);
                Enqueue(p);
                return;
            }

            ReceiveChat(String.Format(Envir.Tr("Your item has been returned to you.")), ChatType.System);
            p.Success = true;

            GainItem(Info.CurrentRefine);

            Info.CurrentRefine = null;
            Info.CollectTime = 0;
            Enqueue(p);
        }
        public void CheckRefine(ulong uniqueID)
        {
            //Enqueue(new S.RepairItem { UniqueID = uniqueID });

            if (Dead) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.RefineCheckKey, StringComparison.CurrentCultureIgnoreCase))) return;

            UserItem temp = null;

            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                temp = Info.Inventory[i];
                if (temp == null || temp.UniqueID != uniqueID) continue;
                index = i;
                break;
            }

            if (index == -1) return;

            if (Info.Inventory[index].RefineAdded == 0)
            {
                ReceiveChat(String.Format(Envir.Tr("{0} doesn't need to be checked as it hasn't been refined yet."), Info.Inventory[index].FriendlyName), ChatType.System);
                return;
            }


            if ((Info.Inventory[index].RefinedValue == RefinedValue.DC) && (Info.Inventory[index].RefineAdded > 0))
            {
                ReceiveChat(String.Format(Envir.Tr("Congratulations, your {0} now has +{1} extra DC."), Info.Inventory[index].FriendlyName, Info.Inventory[index].RefineAdded), ChatType.System);
                Info.Inventory[index].DC = (byte)Math.Min(byte.MaxValue, Info.Inventory[index].DC + Info.Inventory[index].RefineAdded);
                Info.Inventory[index].RefineAdded = 0;
                Info.Inventory[index].RefinedValue = RefinedValue.None;
            }
            else if ((Info.Inventory[index].RefinedValue == RefinedValue.MC) && (Info.Inventory[index].RefineAdded > 0))
            {
                ReceiveChat(String.Format(Envir.Tr("Congratulations, your {0} now has +{1} extra MC."), Info.Inventory[index].FriendlyName, Info.Inventory[index].RefineAdded), ChatType.System);
                Info.Inventory[index].MC = (byte)Math.Min(byte.MaxValue, Info.Inventory[index].MC + Info.Inventory[index].RefineAdded);
                Info.Inventory[index].RefineAdded = 0;
                Info.Inventory[index].RefinedValue = RefinedValue.None;
            }
            else if ((Info.Inventory[index].RefinedValue == RefinedValue.SC) && (Info.Inventory[index].RefineAdded > 0))
            {
                ReceiveChat(String.Format(Envir.Tr("Congratulations, your {0} now has +{1} extra SC."), Info.Inventory[index].FriendlyName, Info.Inventory[index].RefineAdded), ChatType.System);
                Info.Inventory[index].SC = (byte)Math.Min(byte.MaxValue, Info.Inventory[index].SC + Info.Inventory[index].RefineAdded);
                Info.Inventory[index].RefineAdded = 0;
                Info.Inventory[index].RefinedValue = RefinedValue.None;
            }
            else if ((Info.Inventory[index].RefinedValue == RefinedValue.None) && (Info.Inventory[index].RefineAdded > 0))
            {
                ReceiveChat(String.Format(Envir.Tr("Your {0} smashed into a thousand pieces upon testing."), Info.Inventory[index].FriendlyName), ChatType.System);
                Enqueue(new S.RefineItem { UniqueID = Info.Inventory[index].UniqueID });
                Info.Inventory[index] = null;
                return;
            }

            Enqueue(new S.ItemUpgraded { Item = Info.Inventory[index] });
            return;
        }

        #endregion

        #region Relationship

        public void NPCDivorce()
        {
            if (Info.Married == 0)
            {
                ReceiveChat(string.Format(Envir.Tr("You're not married.")), ChatType.System);
                return;
            }

            CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);
            PlayerObject Player = Envir.GetPlayer(Lover.Name);

            Buff buff = Buffs.Where(e => e.Type == BuffType.RelationshipEXP).FirstOrDefault();
            if (buff != null)
            {
                RemoveBuff(BuffType.RelationshipEXP);
                Player.RemoveBuff(BuffType.RelationshipEXP);
            }

            Info.Married = 0;
            Info.MarriedDate = DateTime.Now;

            if (Info.Equipment[(int)EquipmentSlot.RingL] != null)
            {
                Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing = -1;
                Enqueue(new S.RefreshItem { Item = Info.Equipment[(int)EquipmentSlot.RingL] });
            }


            GetRelationship(false);

            Lover.Married = 0;
            Lover.MarriedDate = DateTime.Now;
            if (Lover.Equipment[(int)EquipmentSlot.RingL] != null)
                Lover.Equipment[(int)EquipmentSlot.RingL].WeddingRing = -1;

            if (Player != null)
            {
                Player.GetRelationship(false);
                Player.ReceiveChat(string.Format(Envir.Tr("You've just been forcefully divorced")), ChatType.System);
                if (Player.Info.Equipment[(int)EquipmentSlot.RingL] != null)
                    Player.Enqueue(new S.RefreshItem { Item = Player.Info.Equipment[(int)EquipmentSlot.RingL] });
            }
        }

        public bool CheckMakeWeddingRing()
        {
            if (Info.Married == 0)
            {
                ReceiveChat(string.Format(Envir.Tr("You need to be married to make a Wedding Ring.")), ChatType.System);
                return false;
            }

            if (Info.Equipment[(int)EquipmentSlot.RingL] == null)
            {
                ReceiveChat(string.Format(Envir.Tr("You need to wear a ring on your left finger to make a Wedding Ring.")), ChatType.System);
                return false;
            }

            if (Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing != -1)
            {
                ReceiveChat(string.Format(Envir.Tr("You're already wearing a Wedding Ring.")), ChatType.System);
                return false;
            }

            if (Info.Equipment[(int)EquipmentSlot.RingL].Info.Bind.HasFlag(BindMode.NoWeddingRing))
            {
                ReceiveChat(string.Format(Envir.Tr("You cannot use this type of ring.")), ChatType.System);
                return false;
            }

            return true;
        }

        public void MakeWeddingRing()
        {
            if (CheckMakeWeddingRing())
            {
                Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing = Info.Married;
                Enqueue(new S.RefreshItem { Item = Info.Equipment[(int)EquipmentSlot.RingL] });
                ReceiveChat(string.Format(Envir.Tr("MakeWeddingRing successful.")), ChatType.System);
            }
        }

        public void ReplaceWeddingRing(ulong uniqueID)
        {
            if (Dead) return;

            if (NPCPage == null || (!String.Equals(NPCPage.Key, NPCObject.ReplaceWedRingKey, StringComparison.CurrentCultureIgnoreCase))) return;

            UserItem temp = null;
            UserItem CurrentRing = Info.Equipment[(int)EquipmentSlot.RingL];

            if (CurrentRing == null)
            {
                ReceiveChat(string.Format(Envir.Tr("You arn't wearing a  ring to upgrade.")), ChatType.System);
                return;
            }

            if (CurrentRing.WeddingRing == -1)
            {
                ReceiveChat(string.Format(Envir.Tr("You arn't wearing a Wedding Ring to upgrade.")), ChatType.System);
                return;
            }

            int index = -1;

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                temp = Info.Inventory[i];
                if (temp == null || temp.UniqueID != uniqueID) continue;
                index = i;
                break;
            }

            if (index == -1) return;

            temp = Info.Inventory[index];


            if (temp.Info.Type != ItemType.Ring)
            {
                ReceiveChat(string.Format(Envir.Tr("You can't replace a Wedding Ring with this item.")), ChatType.System);
                return;
            }

            if (!CanEquipItem(temp, (int)EquipmentSlot.RingL))
            {
                ReceiveChat(string.Format(Envir.Tr("You can't equip the item you're trying to use.")), ChatType.System);
                return;
            }

            if (temp.Info.Bind.HasFlag(BindMode.NoWeddingRing))
            {
                ReceiveChat(string.Format(Envir.Tr("You cannot use this type of ring.")), ChatType.System);
                return;
            }

            uint cost = (uint)((Info.Inventory[index].Info.RequiredAmount * 10) * Settings.ReplaceWedRingCost);

            if (cost > Account.Gold)
            {
                ReceiveChat(String.Format(Envir.Tr("You don't have enough gold to replace your Wedding Ring.")), ChatType.System);
                return;
            }

            Account.Gold -= cost;
            Enqueue(new S.LoseGold { Gold = cost });


            temp.WeddingRing = Info.Married;
            CurrentRing.WeddingRing = -1;

            Info.Equipment[(int)EquipmentSlot.RingL] = temp;
            Info.Inventory[index] = CurrentRing;

            Enqueue(new S.EquipItem { Grid = MirGridType.Inventory, UniqueID = temp.UniqueID, To = (int)EquipmentSlot.RingL, Success = true });

            Enqueue(new S.RefreshItem { Item = Info.Inventory[index] });
            Enqueue(new S.RefreshItem { Item = Info.Equipment[(int)EquipmentSlot.RingL] });

        }

        public void MarriageRequest()
        {
            if (Info.Married != 0)
            {
                ReceiveChat(string.Format(Envir.Tr("You're already married.")), ChatType.System);
                return;
            }

            if (Info.MarriedDate.AddDays(Settings.MarriageCooldown) > DateTime.Now)
            {
                ReceiveChat(string.Format(Envir.Tr("You can't get married again yet, there is a {0} day cooldown after a divorce."), Settings.MarriageCooldown), ChatType.System);
                return;
            }

            if (Info.Level < Settings.MarriageLevelRequired)
            {
                ReceiveChat(string.Format(Envir.Tr("You need to be at least level {0} to get married."), Settings.MarriageLevelRequired), ChatType.System);
                return;
            }

            Point target = Functions.PointMove(CurrentLocation, Direction, 1);
            var cellObjects = CurrentMap.GetCellObjects(target);
            PlayerObject player = null;

            if (cellObjects == null || cellObjects.Count < 1) return;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                MapObject ob = cellObjects[i];
                if (ob.Race != ObjectType.Player) continue;

                player = Envir.GetPlayer(ob.Name);
            }

            if (player != null)
            {
                if (!Functions.FacingEachOther(Direction, CurrentLocation, player.Direction, player.CurrentLocation))
                {
                    ReceiveChat(string.Format(Envir.Tr("You need to be facing each other to perform a marriage.")), ChatType.System);
                    return;
                }

                if (player.Level < Settings.MarriageLevelRequired)
                {
                    ReceiveChat(string.Format(Envir.Tr("Your lover needs to be at least level {0} to get married."), Settings.MarriageLevelRequired), ChatType.System);
                    return;
                }

                if (player.Info.MarriedDate.AddDays(Settings.MarriageCooldown) > DateTime.Now)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} can't get married again yet, there is a {1} day cooldown after divorce"), player.Name, Settings.MarriageCooldown), ChatType.System);
                    return;
                }

                if (!player.AllowMarriage)
                {
                    ReceiveChat(Envir.Tr("The person you're trying to propose to isn't allowing marriage requests."), ChatType.System);
                    return;
                }

                if (player == this)
                {
                    ReceiveChat(Envir.Tr("You cant marry yourself."), ChatType.System);
                    return;
                }

                if (player.Dead || Dead)
                {
                    ReceiveChat(Envir.Tr("You can't perform a marriage with a dead player."), ChatType.System);
                    return;
                }

                if (player.MarriageProposal != null)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} already has a marriage invitation."), player.Info.Name), ChatType.System);
                    return;
                }

                if (!Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) || player.CurrentMap != CurrentMap)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} is not within marriage range."), player.Info.Name), ChatType.System);
                    return;
                }

                if (player.Info.Married != 0)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} is already married."), player.Info.Name), ChatType.System);
                    return;
                }

                player.MarriageProposal = this;
                player.Enqueue(new S.MarriageRequest { Name = Info.Name });
            }
            else
            {
                ReceiveChat(string.Format(Envir.Tr("You need to be facing a player to request a marriage.")), ChatType.System);
                return;
            }
        }

        public void MarriageReply(bool accept)
        {
            if (MarriageProposal == null || MarriageProposal.Info == null)
            {
                MarriageProposal = null;
                return;
            }

            if (!accept)
            {
                MarriageProposal.ReceiveChat(string.Format(Envir.Tr("{0} has refused to marry you."), Info.Name), ChatType.System);
                MarriageProposal = null;
                return;
            }

            if (Info.Married != 0)
            {
                ReceiveChat(Envir.Tr("You are already married."), ChatType.System);
                MarriageProposal = null;
                return;
            }

            if (MarriageProposal.Info.Married != 0)
            {
                ReceiveChat(string.Format(Envir.Tr("{0} is already married."), MarriageProposal.Info.Name), ChatType.System);
                MarriageProposal = null;
                return;
            }


            MarriageProposal.Info.Married = Info.Index;
            MarriageProposal.Info.MarriedDate = DateTime.Now;

            Info.Married = MarriageProposal.Info.Index;
            Info.MarriedDate = DateTime.Now;

            GetRelationship(false);
            MarriageProposal.GetRelationship(false);

            MarriageProposal.ReceiveChat(string.Format(Envir.Tr("Congratulations, you're now married to {0}."), Info.Name), ChatType.System);
            ReceiveChat(String.Format(Envir.Tr("Congratulations, you're now married to {0}."), MarriageProposal.Info.Name), ChatType.System);

            MarriageProposal = null;
        }

        public void DivorceRequest()
        {

            if (Info.Married == 0)
            {
                ReceiveChat(string.Format(Envir.Tr("You're not married.")), ChatType.System);
                return;
            }


            Point target = Functions.PointMove(CurrentLocation, Direction, 1);
            var cellObjects = CurrentMap.GetCellObjects(target);
            PlayerObject player = null;

            if (cellObjects == null || cellObjects.Count < 1) return;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                MapObject ob = cellObjects[i];
                if (ob.Race != ObjectType.Player) continue;

                player = Envir.GetPlayer(ob.Name);
            }

            if (player == null)
            {
                ReceiveChat(string.Format(Envir.Tr("You need to be facing your lover to divorce them.")), ChatType.System);
                return;
            }

            if (player != null)
            {
                if (!Functions.FacingEachOther(Direction, CurrentLocation, player.Direction, player.CurrentLocation))
                {
                    ReceiveChat(string.Format(Envir.Tr("You need to be facing your lover to divorce them.")), ChatType.System);
                    return;
                }

                if (player == this)
                {
                    ReceiveChat(Envir.Tr("You can't divorce yourself."), ChatType.System);
                    return;
                }

                if (player.Dead || Dead)
                {
                    ReceiveChat(Envir.Tr("You can't divorce a dead player."), ChatType.System); //GOT TO HERE, NEED TO KEEP WORKING ON IT.
                    return;
                }

                if (player.Info.Index != Info.Married)
                {
                    ReceiveChat(string.Format(Envir.Tr("You aren't married to {0}"), player.Info.Name), ChatType.System);
                    return;
                }

                if (!Functions.InRange(player.CurrentLocation, CurrentLocation, Globals.DataRange) || player.CurrentMap != CurrentMap)
                {
                    ReceiveChat(string.Format(Envir.Tr("{0} is not within divorce range."), player.Info.Name), ChatType.System);
                    return;
                }

                player.DivorceProposal = this;
                player.Enqueue(new S.DivorceRequest { Name = Info.Name });
            }
            else
            {
                ReceiveChat(string.Format(Envir.Tr("You need to be facing your lover to divorce them.")), ChatType.System);
                return;
            }
        }

        public void DivorceReply(bool accept)
        {
            if (DivorceProposal == null || DivorceProposal.Info == null)
            {
                DivorceProposal = null;
                return;
            }

            if (!accept)
            {
                DivorceProposal.ReceiveChat(string.Format(Envir.Tr("{0} has refused to divorce you."), Info.Name), ChatType.System);
                DivorceProposal = null;
                return;
            }

            if (Info.Married == 0)
            {
                ReceiveChat(Envir.Tr("You aren't married so you don't require a divorce."), ChatType.System);
                DivorceProposal = null;
                return;
            }

            Buff buff = Buffs.Where(e => e.Type == BuffType.RelationshipEXP).FirstOrDefault();
            if (buff != null)
            {
                RemoveBuff(BuffType.RelationshipEXP);
                DivorceProposal.RemoveBuff(BuffType.RelationshipEXP);
            }

            DivorceProposal.Info.Married = 0;
            DivorceProposal.Info.MarriedDate = DateTime.Now;
            if (DivorceProposal.Info.Equipment[(int)EquipmentSlot.RingL] != null)
            {
                DivorceProposal.Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing = -1;
                DivorceProposal.Enqueue(new S.RefreshItem { Item = DivorceProposal.Info.Equipment[(int)EquipmentSlot.RingL] });
            }

            Info.Married = 0;
            Info.MarriedDate = DateTime.Now;
            if (Info.Equipment[(int)EquipmentSlot.RingL] != null)
            {
                Info.Equipment[(int)EquipmentSlot.RingL].WeddingRing = -1;
                Enqueue(new S.RefreshItem { Item = Info.Equipment[(int)EquipmentSlot.RingL] });
            }


            DivorceProposal.ReceiveChat(string.Format(Envir.Tr("You're now divorced"), Info.Name), ChatType.System);
            ReceiveChat(Envir.Tr("You're now divorced"), ChatType.System);

            GetRelationship(false);
            DivorceProposal.GetRelationship(false);
            DivorceProposal = null;
        }

        public void GetRelationship(bool CheckOnline = true)
        {
            if (Info.Married == 0)
            {
                Enqueue(new S.LoverUpdate { Name = "", Date = Info.MarriedDate, MapName = "", MarriedDays = 0 });
            }
            else
            {
                CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);

                PlayerObject player = Envir.GetPlayer(Lover.Name);

                if (player == null)
                    Enqueue(new S.LoverUpdate { Name = Lover.Name, Date = Info.MarriedDate, MapName = "", MarriedDays = (short)(DateTime.Now - Info.MarriedDate).TotalDays });
                else
                {
                    Enqueue(new S.LoverUpdate { Name = Lover.Name, Date = Info.MarriedDate, MapName = player.CurrentMap.Info.Title, MarriedDays = (short)(DateTime.Now - Info.MarriedDate).TotalDays });
                    if (CheckOnline)
                    {
                        player.GetRelationship(false);
                        player.ReceiveChat(String.Format(Envir.Tr("{0} has come online."), Info.Name), ChatType.System);
                    }
                }
            }
        }
        public void LogoutRelationship()
        {
            if (Info.Married == 0) return;
            CharacterInfo Lover = Envir.GetCharacterInfo(Info.Married);

            if (Lover == null)
            {
                SMain.EnqueueDebugging(Name + " is married but couldn't find marriage ID " + Info.Married);
                return;
            }

            PlayerObject player = Envir.GetPlayer(Lover.Name);
            if (player != null)
            {
                player.Enqueue(new S.LoverUpdate { Name = Info.Name, Date = player.Info.MarriedDate, MapName = "", MarriedDays = (short)(DateTime.Now - Info.MarriedDate).TotalDays });
                player.ReceiveChat(String.Format(Envir.Tr("{0} has gone offline."), Info.Name), ChatType.System);
            }
        }

        #endregion

        #region Mentorship

        public void MentorBreak(bool Force = false)
        {
            if (Info.Mentor == 0)
            {
                ReceiveChat(Envir.Tr("You don't currently have a Mentorship to cancel."), ChatType.System);
                return;
            }
            CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);
            PlayerObject Player = Envir.GetPlayer(Mentor.Name);

            if (Force)
            {
                Info.MentorDate = DateTime.Now.AddDays(Settings.MentorLength);
                ReceiveChat(String.Format(Envir.Tr("You now have a {0} day cooldown on starting a new Mentorship."), Settings.MentorLength), ChatType.System);
            }
            else
                ReceiveChat(Envir.Tr("Your Mentorship has now expired."), ChatType.System);

            if (Info.isMentor)
            {
                RemoveBuff(BuffType.Mentor);

                if (Player != null)
                {
                    Info.MentorExp += Player.MenteeEXP;
                    Player.MenteeEXP = 0;
                    Player.RemoveBuff(BuffType.Mentee);
                }
            }
            else
            {
                RemoveBuff(BuffType.Mentee);

                if (Player != null)
                {
                    Mentor.MentorExp += MenteeEXP;
                    MenteeEXP = 0;
                    Player.RemoveBuff(BuffType.Mentor);
                }
            }

            Info.Mentor = 0;
            GetMentor(false);


            if (Info.isMentor && Info.MentorExp > 0)
            {
                GainExp((uint)Info.MentorExp);
                Info.MentorExp = 0;
            }


            Mentor.Mentor = 0;


            if (Player != null)
            {
                Player.ReceiveChat(Envir.Tr("Your Mentorship has now expired."), ChatType.System);
                Player.GetMentor(false);
                if (Mentor.isMentor && Mentor.MentorExp > 0)
                {
                    Player.GainExp((uint)Mentor.MentorExp);
                    Info.MentorExp = 0;
                }
            }
            else
            {
                if (Mentor.isMentor && Mentor.MentorExp > 0)
                {
                    Mentor.Experience += Mentor.MentorExp;
                    Mentor.MentorExp = 0;
                }
            }

            Info.isMentor = false;
            Mentor.isMentor = false;
            Info.MentorExp = 0;
            Mentor.MentorExp = 0;
        }

        public void AddMentor(string Name)
        {

            if (Info.Mentor != 0)
            {
                ReceiveChat(Envir.Tr("You already have a Mentor."), ChatType.System);
                return;
            }

            if (Info.Name == Name)
            {
                ReceiveChat(Envir.Tr("You can't Mentor yourself."), ChatType.System);
                return;
            }

            if (Info.MentorDate > DateTime.Now)
            {
                ReceiveChat(Envir.Tr("You can't start a new Mentorship yet."), ChatType.System);
                return;
            }

            PlayerObject Mentor = Envir.GetPlayer(Name);

            if (Mentor == null)
            {
                ReceiveChat(String.Format(Envir.Tr("Can't find anybody by the name {0}."), Name), ChatType.System);
            }
            else
            {
                Mentor.MentorRequest = null;

                if (!Mentor.AllowMentor)
                {
                    ReceiveChat(String.Format(Envir.Tr("{0} is not allowing Mentor requests."), Mentor.Info.Name), ChatType.System);
                    return;
                }

                if (Mentor.Info.MentorDate > DateTime.Now)
                {
                    ReceiveChat(String.Format(Envir.Tr("{0} can't start another Mentorship yet."), Mentor.Info.Name), ChatType.System);
                    return;
                }

                if (Mentor.Info.Mentor != 0)
                {
                    ReceiveChat(String.Format(Envir.Tr("{0} is already a Mentor."), Mentor.Info.Name), ChatType.System);
                    return;
                }

            //    if (Info.Class != Mentor.Info.Class)
              //  {
                //    ReceiveChat(Envir.Tr("You can only be mentored by someone of the same Class."), ChatType.System);
                  //  return;
               // }
                if ((Info.Level + Settings.MentorLevelGap) > Mentor.Level)
                {
                    ReceiveChat(String.Format(Envir.Tr("You can only be mentored by someone who at least {0} level(s) above you."), Settings.MentorLevelGap), ChatType.System);
                    return;
                }

                Mentor.MentorRequest = this;
                Mentor.Enqueue(new S.MentorRequest { Name = Info.Name, Level = Info.Level });
                ReceiveChat(String.Format(Envir.Tr("Request Sent.")), ChatType.System);
            }

        }

        public void MentorReply(bool accept)
        {
            if (MentorRequest == null || MentorRequest.Info == null)
            {
                MentorRequest = null;
                return;
            }

            if (!accept)
            {
                MentorRequest.ReceiveChat(string.Format(Envir.Tr("{0} has refused to Mentor you."), Info.Name), ChatType.System);
                MentorRequest = null;
                return;
            }

            if (Info.Mentor != 0)
            {
                ReceiveChat(Envir.Tr("You already have a Student."), ChatType.System);
                return;
            }

            PlayerObject Student = Envir.GetPlayer(MentorRequest.Info.Name);
            MentorRequest = null;

            if (Student == null)
            {
                ReceiveChat(String.Format(Envir.Tr("{0} is no longer online."), Student.Name), ChatType.System);
                return;
            }
            else
            {
                if (Student.Info.Mentor != 0)
                {
                    ReceiveChat(String.Format(Envir.Tr("{0} already has a Mentor."), Student.Info.Name), ChatType.System);
                    return;
                }
                if (Info.Class != Student.Info.Class)
                {
                    ReceiveChat(Envir.Tr("You can only mentor someone of the same Class."), ChatType.System);
                    return;
                }
                if ((Info.Level - Settings.MentorLevelGap) < Student.Level)
                {
                    ReceiveChat(String.Format(Envir.Tr("You can only mentor someone who at least {0} level(s) below you."), Settings.MentorLevelGap), ChatType.System);
                    return;
                }

                Student.Info.Mentor = Info.Index;
                Student.Info.isMentor = false;
                Info.Mentor = Student.Info.Index;
                Info.isMentor = true;
                Student.Info.MentorDate = DateTime.Now;
                Info.MentorDate = DateTime.Now;

                ReceiveChat(String.Format(Envir.Tr("You're now the Mentor of {0}."), Student.Info.Name), ChatType.System);
                Student.ReceiveChat(String.Format(Envir.Tr("You're now being Mentored by {0}."), Info.Name), ChatType.System);
                GetMentor(false);
                Student.GetMentor(false);
            }
        }

        public void GetMentor(bool CheckOnline = true)
        {
            if (Info.Mentor == 0)
            {
                Enqueue(new S.MentorUpdate { Name = "", Level = 0, Online = false, MenteeEXP = 0 });
            }
            else
            {
                CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);

                PlayerObject player = Envir.GetPlayer(Mentor.Name);

                if (player == null)
                    Enqueue(new S.MentorUpdate { Name = Mentor.Name, Level = Mentor.Level, Online = false, MenteeEXP = Info.MentorExp });
                else
                {
                    Enqueue(new S.MentorUpdate { Name = Mentor.Name, Level = Mentor.Level, Online = true, MenteeEXP = Info.MentorExp });
                    if (CheckOnline)
                    {
                        player.GetMentor(false);
                        player.ReceiveChat(String.Format(Envir.Tr("{0} has come online."), Info.Name), ChatType.System);
                    }
                }
            }
        }

        public void LogoutMentor()
        {
            if (Info.Mentor == 0) return;

            CharacterInfo Mentor = Envir.GetCharacterInfo(Info.Mentor);

            if (Mentor == null)
            {
                SMain.EnqueueDebugging(Name + " is mentored but couldn't find mentor ID " + Info.Mentor);
                return;
            }

            PlayerObject player = Envir.GetPlayer(Mentor.Name);

            if (!Info.isMentor)
            {
                Mentor.MentorExp += MenteeEXP;
            }

            if (player != null)
            {
                player.Enqueue(new S.MentorUpdate { Name = Info.Name, Level = Info.Level, Online = false, MenteeEXP = Mentor.MentorExp });
                player.ReceiveChat(String.Format(Envir.Tr("{0} has gone offline."), Info.Name), ChatType.System);
            }
        }

        #endregion

        #region Gameshop

        public void GameShopStock(GameShopItem item)
        {
            int purchased;
            int StockLevel;

            if (item.iStock) //Invididual Stock
            {
                Info.GSpurchases.TryGetValue(item.Info.Index, out purchased);
            }
            else //Server Stock
            {
                Envir.GameshopLog.TryGetValue(item.Info.Index, out purchased);
            }

            if (item.Stock - purchased >= 0)
            {
                StockLevel = item.Stock - purchased;
                Enqueue(new S.GameShopStock { GIndex = item.Info.Index, StockLevel = StockLevel });
            }

        }

        public GameShopItem FindProduct(int GIndex)
        {
            List<GameShopItem> shopList = Envir.GameShopList;
            for (int i = 0; i < shopList.Count; i++)
            {
                if (shopList[i].GIndex == GIndex)
                {
                    return shopList[i];
                }
            }

            return null;
        }

        public void GameshopBuy(int GIndex, byte Quantity)
        {
            if (Quantity < 1 || Quantity > 99) return;

            GameShopItem Product = FindProduct(GIndex);
            if (Product == null)
            {
                ReceiveChat(Envir.Tr("You're trying to buy an item that isn't in the shop."), ChatType.System);
                SMain.EnqueueDebugging(Info.Name + " is trying to buy Something that doesn't exist.");
                return;
            }

            bool canAfford = false;
            uint CreditCost = 0;
            uint GoldCost = 0;

            if (((decimal)(Quantity * Product.Count) / Product.Info.StackSize) > 5) return;

            if (!CheckProductStock(Product, Quantity))
                return;

            SMain.EnqueueDebugging(Info.Name + " is trying to buy " + Product.Info.FriendlyName + " x " + Quantity + " - Stock is available");
            if (Product.CreditPrice * Quantity > 0 && Product.CreditPrice * Quantity < Account.Credit)
            {
                canAfford = true;
                CreditCost = (Product.CreditPrice * Quantity);
            }
            else
            { //Needs to attempt to pay with gold and credits
                if (Product.GoldPrice * Quantity > 0 && Account.Gold >= Product.GoldPrice * Quantity)
                {
                    GoldCost = Product.GoldPrice * Quantity;
                 //   CreditCost = Account.Credit;
                    canAfford = true;
                }
                else
                {
                    ReceiveChat(Envir.Tr("You don't have enough currency for your purchase."), ChatType.System);
                    SMain.EnqueueDebugging(Info.Name + " is trying to buy " + Product.Info.FriendlyName + " x " + Quantity + " - not enough currency.");
                    return;
                }
            }

            if (!canAfford)
                return;

            uint quantity = (Quantity * Product.Count);
            if (!CanGainItems(Product.Info, quantity))
            {
                ReceiveChat(Envir.Tr("Your inventory is full or out of weight."), ChatType.System);
                return;
            }

            SMain.EnqueueDebugging(Info.Name + " is trying to buy " + Product.Info.FriendlyName + " x " + Quantity + " - Has enough currency.");
            if (GoldCost > 0)
                Account.Gold -= GoldCost;

            Account.Credit -= CreditCost;

            Report.GoldChanged("GameShop", GoldCost, Account.Gold, true, Product.Info.FriendlyName);
            Report.CreditChanged("GameShop", CreditCost, Account.Credit, true, Product.Info.FriendlyName);

            if (GoldCost != 0) Enqueue(new S.LoseGold { Gold = GoldCost });
            if (CreditCost != 0) Enqueue(new S.LoseCredit { Credit = CreditCost });

            int Purchased;

            if (Product.iStock && Product.Stock != 0)
            {
                Info.GSpurchases.TryGetValue(Product.Info.Index, out Purchased);
                if (Purchased == 0)
                {
                    Info.GSpurchases[Product.GIndex] = Quantity;
                }
                else
                {
                    Info.GSpurchases[Product.GIndex] += Quantity;
                }
            }

            Purchased = 0;

            Envir.GameshopLog.TryGetValue(Product.Info.Index, out Purchased);
            if (Purchased == 0)
            {
                Envir.GameshopLog[Product.GIndex] = Quantity;
            }
            else
            {
                Envir.GameshopLog[Product.GIndex] += Quantity;
            }

            if (Product.Stock != 0) GameShopStock(Product);


            Report.ItemGSBought("GameShop", Product, Quantity, CreditCost, GoldCost);

            if (Product.Info.StackSize <= 1 || quantity == 1)
            {
                for (int i = 0; i < Quantity; i++)
                {
                    UserItem mailItem = Envir.CreateFreshItem(Envir.GetItemInfo(Product.Info.Index));
                    GainItem(mailItem);
                }
            }
            else
            {
                while (quantity > 0)
                {
                    UserItem mailItem = Envir.CreateFreshItem(Envir.GetItemInfo(Product.Info.Index));
                    mailItem.Count = 0;
                    for (int i = 0; i < mailItem.Info.StackSize; i++)
                    {
                        mailItem.Count++;
                        quantity--;
                        if (quantity == 0) break;
                    }
                    if (mailItem.Count == 0) break;

                    GainItem(mailItem);
                }
            }

            SMain.EnqueueDebugging(Info.Name + " is trying to buy " + Product.Info.FriendlyName + " x " + Quantity + " - Purchases Sent!");
        }

        public void GetGameShop()
        {
            int purchased;
            GameShopItem item = new GameShopItem();
            int StockLevel;

            for (int i = 0; i < Envir.GameShopList.Count; i++)
            {
                item = Envir.GameShopList[i];

                if (item.Stock != 0)
                {
                    if (item.iStock) //Individual Stock
                    {
                        Info.GSpurchases.TryGetValue(item.Info.Index, out purchased);
                    }
                    else //Server Stock
                    {
                        Envir.GameshopLog.TryGetValue(item.Info.Index, out purchased);
                    }

                    if (item.Stock - purchased >= 0)
                    {
                        StockLevel = item.Stock - purchased;
                        Enqueue(new S.GameShopInfo { Item = item, StockLevel = StockLevel });
                    }
                }
                else
                {
                    Enqueue(new S.GameShopInfo { Item = item, StockLevel = item.Stock });
                }
            }
        }

        #endregion

        #region ConquestWall
        public void CheckConquest(bool checkPalace = false)
        {
            if (CurrentMap.tempConquest == null && CurrentMap.Conquest != null)
            {
                ConquestObject swi = CurrentMap.GetConquest(CurrentLocation);
                if (swi != null)
                    EnterSabuk();
                else
                    LeaveSabuk();
            }
            else if (CurrentMap.tempConquest != null)
            {
                if (checkPalace && CurrentMap.Info.Index == CurrentMap.tempConquest.PalaceMap.Info.Index && CurrentMap.tempConquest.GameType == ConquestGame.CapturePalace)
                    CurrentMap.tempConquest.TakeConquest(this);

                EnterSabuk();
            }
        }
        public void EnterSabuk()
        {
            if (WarZone) return;
            WarZone = true;
            RefreshNameColour();
        }

        public void LeaveSabuk()
        {
            if (!WarZone) return;
            WarZone = false;
            RefreshNameColour();
        }
        #endregion

        private static int RANK_NUM = 7;
        private long[] LastRankRequest = new long[RANK_NUM];
        public void GetRanking(byte RankType)
        {
            if (RankType > RANK_NUM) return;
            if ((LastRankRequest[RankType] != 0) && ((LastRankRequest[RankType] + 300 * 1000) > Envir.Time)) return;
            LastRankRequest[RankType] = Envir.Time;
            Region region = Envir.GetRegion(GetRegionID());
            if (region == null)
                return;

            if (RankType == 0)
            {
                Enqueue(new S.Rankings { Listings = region.RankTop, RankType = RankType, MyRank = Info.Rank[0] });
            }
            else
            {
                Enqueue(new S.Rankings { Listings = region.RankClass[RankType - 1], RankType = RankType, MyRank = (byte)Class == (RankType - 1) ? Info.Rank[1] : 0 });
            }
        }

        public void Opendoor(byte Doorindex)
        {
            //todo: add check for sw doors
            if (CurrentMap.OpenDoor(Doorindex))
            {
                Enqueue(new S.Opendoor() { DoorIndex = Doorindex });
                Broadcast(new S.Opendoor() { DoorIndex = Doorindex });
            }
        }

        public void GetRentedItems()
        {
            Enqueue(new S.GetRentedItems { RentedItems = Info.RentedItems });
        }

        public void ItemRentalRequest()
        {
            if (Dead)
            {
                ReceiveChat(Envir.Tr("Unable to rent items while dead."), ChatType.System);
                return;
            }

            if (ItemRentalPartner != null)
            {
                ReceiveChat(Envir.Tr("You are already renting an item to another player."), ChatType.System);
                return;
            }

            var targetPosition = Functions.PointMove(CurrentLocation, Direction, 1);
            var cellObjects = CurrentMap.GetCellObjects(targetPosition);
            PlayerObject targetPlayer = null;

            if (cellObjects == null || cellObjects.Count < 1)
                return;

            foreach (var mapObject in cellObjects)
            {
                if (mapObject.Race != ObjectType.Player)
                    continue;

                targetPlayer = Envir.GetPlayer(mapObject.Name);
            }

            if (targetPlayer == null)
            {
                ReceiveChat(Envir.Tr("Face the player you would like to rent an item too."), ChatType.System);
                return;
            }

            if (Info.RentedItems.Count >= 3)
            {
                ReceiveChat(Envir.Tr("Unable to rent more than 3 items at a time."), ChatType.System);
                return;
            }

            if (targetPlayer.Info.HasRentedItem)
            {
                ReceiveChat($"{targetPlayer.Name} is unable to rent anymore items at this time.", ChatType.System);
                return;
            }

            if (!Functions.FacingEachOther(Direction, CurrentLocation, targetPlayer.Direction,
                targetPlayer.CurrentLocation))
            {
                ReceiveChat(Envir.Tr("Face the player you would like to rent an item too."), ChatType.System);
                return;
            }

            if (targetPlayer == this)
            {
                ReceiveChat(Envir.Tr("You are unable to rent items to yourself."), ChatType.System);
                return;
            }

            if (targetPlayer.Dead)
            {
                ReceiveChat($"Unable to rent items to {targetPlayer.Name} while dead.", ChatType.System);
                return;
            }

            if (!Functions.InRange(targetPlayer.CurrentLocation, CurrentLocation, Globals.DataRange)
                || targetPlayer.CurrentMap != CurrentMap)
            {
                ReceiveChat($"{targetPlayer.Name} is not within range.", ChatType.System);
                return;
            }

            if (targetPlayer.ItemRentalPartner != null)
            {
                ReceiveChat($"{targetPlayer.Name} is currently busy, try again soon.", ChatType.System);
                return;
            }

            ItemRentalPartner = targetPlayer;
            targetPlayer.ItemRentalPartner = this;

            Enqueue(new S.ItemRentalRequest { Name = targetPlayer.Name, Renting = false });
            ItemRentalPartner.Enqueue(new S.ItemRentalRequest { Name = Name, Renting = true });
        }

        public void SetItemRentalFee(uint amount)
        {
            if (ItemRentalFeeLocked)
                return;

            if (Account.Gold < amount)
                return;

            if (ItemRentalPartner == null)
                return;

            ItemRentalFeeAmount += amount;
            Account.Gold -= amount;

            Enqueue(new S.LoseGold { Gold = amount });
            ItemRentalPartner.Enqueue(new S.ItemRentalFee { Amount = amount });
        }

        public void SetItemRentalPeriodLength(uint days)
        {
            if (ItemRentalItemLocked)
                return;

            if (ItemRentalPartner == null)
                return;

            ItemRentalPeriodLength = days;
            ItemRentalPartner.Enqueue(new S.ItemRentalPeriod { Days = days });
        }

        public void DepositRentalItem(int from, int to)
        {
            var packet = new S.DepositRentalItem { From = from, To = to, Success = false };

            if (ItemRentalItemLocked)
            {
                Enqueue(packet);
                return;
            }

            if (from < 0 || from >= Info.Inventory.Length)
            {
                Enqueue(packet);
                return;
            }

            // TODO: Change this check.
            if (to < 0 || to >= 1)
            {
                Enqueue(packet);
                return;
            }

            var item = Info.Inventory[from];

            if (item == null)
            {
                Enqueue(packet);
                return;
            }

            if (item.RentalInformation?.RentalLocked == true)
            {
                ReceiveChat($"Unable to rent {item.FriendlyName} until {item.RentalInformation.ExpiryDate}", ChatType.System);
                Enqueue(packet);
                return;
            }

            if (item.Info.Bind.HasFlag(BindMode.UnableToRent))
            {
                ReceiveChat($"Unable to rent {item.FriendlyName}", ChatType.System);
                Enqueue(packet);
                return;
            }

            if (item.RentalInformation != null && item.RentalInformation.BindingFlags.HasFlag(BindMode.UnableToRent))
            {
                ReceiveChat($"Unable to rent {item.FriendlyName} as it belongs to {item.RentalInformation.OwnerName}", ChatType.System);
                Enqueue(packet);
                return;
            }

            if (ItemRentalDepositedItem == null)
            {
                ItemRentalDepositedItem = item;
                Info.Inventory[from] = null;

                packet.Success = true;
                RefreshBagWeight();
                UpdateRentalItem();
                Report.ItemMoved("DepositRentalItem", item, MirGridType.Inventory, MirGridType.Renting, from, to);
            }

            Enqueue(packet);
        }

        public void RetrieveRentalItem(int from, int to)
        {
            var packet = new S.RetrieveRentalItem { From = from, To = to, Success = false };

            // TODO: Change this check.
            if (from < 0 || from >= 1)
            {
                Enqueue(packet);
                return;
            }

            if (to < 0 || to >= Info.Inventory.Length)
            {
                Enqueue(packet);
                return;
            }

            var item = ItemRentalDepositedItem;

            if (item == null)
            {
                Enqueue(packet);
                return;
            }

            if (item.Weight + CurrentBagWeight > MaxBagWeight)
            {
                ReceiveChat(Envir.Tr("Item is too heavy to retrieve."), ChatType.System);
                Enqueue(packet);
                return;
            }

            if (Info.Inventory[to] == null)
            {
                Info.Inventory[to] = item;
                ItemRentalDepositedItem = null;

                packet.Success = true;
                RefreshBagWeight();
                UpdateRentalItem();
                Report.ItemMoved("RetrieveRentalItem", item, MirGridType.Renting, MirGridType.Inventory, from, to);
            }

            Enqueue(packet);
        }

        private void UpdateRentalItem()
        {
            if (ItemRentalPartner == null)
                return;

            if (ItemRentalDepositedItem != null)
                ItemRentalPartner.CheckItem(ItemRentalDepositedItem);

            ItemRentalPartner.Enqueue(new S.UpdateRentalItem { LoanItem = ItemRentalDepositedItem });
        }

        public void CancelItemRental()
        {
            if (ItemRentalPartner == null)
                return;

            ItemRentalRemoveLocks();

            var rentalPair = new[]  {
                ItemRentalPartner,
                this
            };

            // TODO: 任务物品被租赁
            for (var i = 0; i < 2; i++)
            {
                if (rentalPair[i] == null)
                    continue;

                if (rentalPair[i].ItemRentalDepositedItem != null)
                {
                    var item = rentalPair[i].ItemRentalDepositedItem;

                    if (FreeSpace(rentalPair[i].Info.Inventory) < 1)
                    {
                        rentalPair[i].GainItemMail(item, 1);
                        rentalPair[i].Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                        rentalPair[i].ItemRentalDepositedItem = null;

                        Report.ItemMailed(Envir.Tr("Cancel Item Rental"), item, item.Count, 1);

                        continue;
                    }

                    for (var j = 0; j < rentalPair[i].Info.Inventory.Length; j++)
                    {
                        if (rentalPair[i].Info.Inventory[j] != null)
                            continue;

                        if (rentalPair[i].CanGainItem(item))
                            rentalPair[i].RetrieveRentalItem(0, j);
                        else
                        {
                            rentalPair[i].GainItemMail(item, 1);
                            rentalPair[i].Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });

                            Report.ItemMailed("Cancel Item Rental", item, item.Count, 1);
                        }

                        rentalPair[i].ItemRentalDepositedItem = null;

                        break;
                    }


                }

                if (rentalPair[i].ItemRentalFeeAmount > 0)
                {
                    rentalPair[i].GainGold(rentalPair[i].ItemRentalFeeAmount);
                    rentalPair[i].ItemRentalFeeAmount = 0;

                    Report.GoldChanged("CancelItemRental", rentalPair[i].ItemRentalFeeAmount, Account.Gold, false);
                }

                rentalPair[i].ItemRentalPartner = null;
                rentalPair[i].Enqueue(new S.CancelItemRental());
            }
        }

        public void ItemRentalLockFee()
        {
            S.ItemRentalLock p = new S.ItemRentalLock { Success = false, GoldLocked = false, ItemLocked = false };

            if (ItemRentalFeeAmount > 0)
            {
                ItemRentalFeeLocked = true;
                p.GoldLocked = true;
                p.Success = true;

                ItemRentalPartner.Enqueue(new S.ItemRentalPartnerLock { GoldLocked = ItemRentalFeeLocked });
            }

            if (ItemRentalFeeLocked && ItemRentalPartner.ItemRentalItemLocked)
                ItemRentalPartner.Enqueue(new S.CanConfirmItemRental());
            else if (ItemRentalFeeLocked && !ItemRentalPartner.ItemRentalItemLocked)
                ItemRentalPartner.ReceiveChat($"{Name} has locked in the rental fee.", ChatType.System);

            Enqueue(p);
        }

        public void ItemRentalLockItem()
        {
            S.ItemRentalLock p = new S.ItemRentalLock { Success = false, GoldLocked = false, ItemLocked = false };

            if (ItemRentalDepositedItem != null)
            {
                ItemRentalItemLocked = true;
                p.ItemLocked = true;
                p.Success = true;

                ItemRentalPartner.Enqueue(new S.ItemRentalPartnerLock { ItemLocked = ItemRentalItemLocked });
            }

            if (ItemRentalItemLocked && ItemRentalPartner.ItemRentalFeeLocked)
                Enqueue(new S.CanConfirmItemRental());
            else if (ItemRentalItemLocked && !ItemRentalPartner.ItemRentalFeeLocked)
                ItemRentalPartner.ReceiveChat($"{Name} has locked in the rental item.", ChatType.System);


            Enqueue(p);
        }

        private void ItemRentalRemoveLocks()
        {
            ItemRentalFeeLocked = false;
            ItemRentalItemLocked = false;

            if (ItemRentalPartner == null)
                return;

            ItemRentalPartner.ItemRentalFeeLocked = false;
            ItemRentalPartner.ItemRentalItemLocked = false;
        }

        public void ChangeName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                ReceiveChat("名称不合法", ChatType.System);
                return;
            }

            if (!NameChecker.CheckCharacterName(name))
            {
                ReceiveChat("名称不合法", ChatType.System);
                return;
            }

            if ((!IsGM) && (Envir.DisabledCharNames.Contains(name.ToUpper())))
            {
                ReceiveChat("名称不合法", ChatType.System);
                return;
            }

            if (Envir.CharacterExists(name))
            {
                ReceiveChat("名称已被使用", ChatType.System);
                return;
            }

            UserItem item = GetScript(41, 1);
            if (item == null)
            {
                ReceiveChat("没有改名道具", ChatType.System);
                return;
            }
   
            ConsumeItem(item, 1);

            Info.Name = name;
            if (MyGuild != null)
                MyGuild.ChangeNickName(this, name);

            Envir.CheckRankUpdate(GetRegionID(), Info);
            ReceiveChat("恭喜你,修成昵称成功!请小退生效！", ChatType.System);
        }

        public void ConfirmItemRental()
        {
            if (ItemRentalPartner == null)
            {
                CancelItemRental();
                return;
            }

            if (Info.RentedItems.Count >= 3)
            {
                CancelItemRental();
                return;
            }

            if (ItemRentalPartner.Info.HasRentedItem)
            {
                CancelItemRental();
                return;
            }

            if (ItemRentalDepositedItem == null)
                return;

            if (ItemRentalPartner.ItemRentalFeeAmount <= 0)
                return;

            if (ItemRentalDepositedItem.Info.Bind.HasFlag(BindMode.UnableToRent))
                return;

            if (ItemRentalDepositedItem.RentalInformation != null &&
                ItemRentalDepositedItem.RentalInformation.BindingFlags.HasFlag(BindMode.UnableToRent))
                return;

            if (!Functions.InRange(ItemRentalPartner.CurrentLocation, CurrentLocation, Globals.DataRange)
                || ItemRentalPartner.CurrentMap != CurrentMap || !Functions.FacingEachOther(Direction, CurrentLocation,
                    ItemRentalPartner.Direction, ItemRentalPartner.CurrentLocation))
            {
                CancelItemRental();
                return;
            }

            if (!ItemRentalItemLocked && !ItemRentalPartner.ItemRentalFeeLocked)
                return;

            if (!ItemRentalPartner.CanGainItem(ItemRentalDepositedItem))
            {
                ReceiveChat($"{ItemRentalPartner.Name} is unable to receive the item.", ChatType.System);
                Enqueue(new S.CancelItemRental());

                ItemRentalPartner.ReceiveChat(Envir.Tr("Unable to accept the rental item."), ChatType.System);
                ItemRentalPartner.Enqueue(new S.CancelItemRental());

                return;
            }

            if (!CanGainGold(ItemRentalPartner.ItemRentalFeeAmount))
            {
                ReceiveChat(Envir.Tr("You are unable to receive any more gold."), ChatType.System);
                Enqueue(new S.CancelItemRental());

                ItemRentalPartner.ReceiveChat($"{Name} is unable to receive any more gold.", ChatType.System);
                ItemRentalPartner.Enqueue(new S.CancelItemRental());

                return;
            }

            var item = ItemRentalDepositedItem;
            item.RentalInformation = new RentalInformation
            {
                OwnerName = Name,
                ExpiryDate = DateTime.Now.AddDays(ItemRentalPeriodLength),
                BindingFlags = BindMode.DontDrop | BindMode.DontStore | BindMode.DontSell | BindMode.DontTrade | BindMode.UnableToRent | BindMode.DontUpgrade | BindMode.UnableToDisassemble
            };

            var itemRentalInformation = new ItemRentalInformation
            {
                ItemId = item.UniqueID,
                ItemName = item.FriendlyName,
                RentingPlayerName = ItemRentalPartner.Name,
                ItemReturnDate = item.RentalInformation.ExpiryDate,

            };

            Info.RentedItems.Add(itemRentalInformation);
            ItemRentalDepositedItem = null;

            ItemRentalPartner.GainItem(item);
            ItemRentalPartner.Info.HasRentedItem = true;
            ItemRentalPartner.ReceiveChat($"You have rented {item.FriendlyName} from {Name} until {item.RentalInformation.ExpiryDate}", ChatType.System);

            GainGold(ItemRentalPartner.ItemRentalFeeAmount);
            ReceiveChat($"Received {ItemRentalPartner.ItemRentalFeeAmount} gold for item rental.", ChatType.System);
            ItemRentalPartner.ItemRentalFeeAmount = 0;

            Enqueue(new S.ConfirmItemRental());
            ItemRentalPartner.Enqueue(new S.ConfirmItemRental());

            ItemRentalRemoveLocks();

            ItemRentalPartner.ItemRentalPartner = null;
            ItemRentalPartner = null;
        }

        #region Humup & Transform
        public void Humup()
        {
            foreach (var ob in Envir.Players)//// allow user to see whta going off
                ob.ReceiveChat(string.Format(Envir.Tr("Player {0} is hum up"), Name), ChatType.Announcement);

            Enqueue(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.HumUpEffect });
            Enqueue(new S.HumUpPlayer { ObjectID = ObjectID, Class = Class, Location = CurrentLocation });
            Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.HumUpEffect });
            Broadcast(new S.HumUpPlayer { ObjectID = ObjectID, Class = Class, Location = CurrentLocation });
            ReceiveChat(Envir.Tr("Please reconnect to the normal use."), ChatType.System);

            Info.HumUp = true;
            AddHumUpBuff();
        }

        public void AddHumUpBuff()
        {
            int[] values = {0,0,0,0,0 };
            switch(Class)
            {
                case MirClass.Warrior:
                    values = new int[] { 230, 130, 10, 10, 80 };
                    break;

                case MirClass.Wizard:
                    values = new int[] { 140, 210, 10, 10, 80 };
                    break;

                case MirClass.Taoist:
                    values = new int[] { 170, 180, 10, 10, 80 };
                    break;

                case MirClass.Assassin:
                    values = new int[] { 195, 155, 10, 10, 80 };
                    break;

                case MirClass.Archer:
                    values = new int[] { 160, 200, 10, 10, 80 };
                    break;

                case MirClass.Monk:
                    values = new int[] { 240, 100, 10, 10, 80 };
                    break;
            }

            AddBuff(new Buff { Type = BuffType.HumUp, Caster = this, ExpireTime = Envir.Time + 100, Infinite = true, Values = values });
        }

        private bool CheckProductStock(GameShopItem Product, byte Quantity)
        {
            if (Product.Stock == 0)
                return true;

            int purchased;
            if (Product.iStock) //Invididual Stock
            {
                Info.GSpurchases.TryGetValue(Product.Info.Index, out purchased);
            }
            else //Server Stock
            {
                Envir.GameshopLog.TryGetValue(Product.Info.Index, out purchased);
            }

            if (Product.Stock - purchased - Quantity >= 0)
            {
                return true;
            }
            else
            {
                ReceiveChat(Envir.Tr("You're trying to buy more of this item than is available."), ChatType.System);
                GameShopStock(Product);
                SMain.EnqueueDebugging(Info.Name + " is trying to buy " + Product.Info.FriendlyName + " x " + Quantity + " - Stock isn't available.");
                return false;
            }
        }

        private bool CatTongue(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;
            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int damage = magic.GetDamage(GetAttackPower(MinDC, MinDC));

            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step

            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time + delay, magic, damage, target);
            ActionList.Add(action);

            cast = true;
            return true;
        }

        private bool GreateFireBallSecret(UserMagic magic, MapObject target, out bool cast)
        {
            cast = false;
            if (target == null || !target.IsAttackTarget(this) || !CanFly(target.CurrentLocation)) return false;

            int value = GetAttackPower(MinMC, MaxMC);
            int delay = Functions.MaxDistance(CurrentLocation, target.CurrentLocation) * 50 + 500; //50 MS per Step
            SpellingId = ++NextChannelingId;

            DelayedAction action = new DelayedAction(DelayedType.MagicSpell, Envir.Time + 1200, magic, value, target, delay, SpellingId);
            ActionList.Add(action);

            cast = true;
            return true;
        }

        private bool HasArchorClassWeapon()
        {
            if (Info.Equipment[(int)EquipmentSlot.Weapon] == null) return false;
            ItemInfo RealItem = Functions.GetRealItem(Info.Equipment[(int)EquipmentSlot.Weapon].Info, Info.Level, Info.Class, Envir.ItemInfoList);
            int type = RealItem.Shape / 100;
            return type == 2 || type == 8;
        }

        private UserItem GetScript(int shape, int count)
        {
            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                UserItem item = Info.Inventory[i];
                if (item != null && item.Info.Type == ItemType.Script && item.Info.Shape == 41 && item.Count >= count)
                    return item;
            }
            return null;
        }

        #endregion
    }


}

