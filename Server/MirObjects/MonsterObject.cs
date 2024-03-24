using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Server.MirDatabase;
using Server.MirEnvir;
using Server.MirObjects.Monsters;
using S = ServerPackets;

namespace Server.MirObjects
{
    public class MonsterObject : MapObject
    {

        public static MonsterObject RegenMonsterByName(Map currentMap, int x, int y, string name, int delay = 0)
        {
            MonsterObject mob = GetMonster(Envir.GetMonsterInfo(name));
            if (mob == null)
                return null;

            DelayedAction action = new DelayedAction(DelayedType.Spawn, Envir.Time + delay, mob, new Point(x, y));
            currentMap.ActionList.Add(action);

            return mob;
        }

        public static MonsterObject GetMonster(MonsterInfo info)
        {
            if (info == null) return null;

            switch (info.AI)
            {
                case 1:
                case 2:
                    return new Deer(info);
                case 3:
                    return new Tree(info);
                case 4:
                    return new SpittingSpider(info);
                case 5:
                    return new CannibalPlant(info);
                case 6:
                    return new Guard(info);
                case 7:
                    return new CaveMaggot(info);
                case 8:
                    return new AxeSkeleton(info);
                case 9:
                    return new HarvestMonster(info);
                case 10:
                    return new FlamingWooma(info);
                case 11:
                    return new WoomaTaurus(info);
                case 12:
                    return new BugBagMaggot(info); // 角蝇
                case 13:
                    return new RedMoonEvil(info); //赤月恶魔
                case 14:
                    return new EvilCentipede(info); // 触龙神
                case 15:
                    return new ZumaMonster(info); // 祖玛雕像
                case 16:
                    return new RedThunderZuma(info); // 祖玛赤雷
                case 17:
                    return new ZumaTaurus(info);// 祖玛雕像
                case 18:
                    return new Shinsu(info);
                case 19:
                    return new KingScorpion(info); //邪恶毒蛇
                case 20:
                    return new DarkDevil(info);   // 黑天
                case 21:
                    return new IncarnatedGhoul(info); //尸王
                case 22:
                    return new IncarnatedZT(info); //悲月角虫
                case 23:
                    return new BoneFamiliar(info);
                case 24:
                    return new DigOutZombie(info);//僵尸
                case 25:
                    return new RevivingZombie(info);
                case 26:
                    return new ShamanZombie(info);
                case 27:
                    return new Khazard(info);
                case 28:
                    return new ToxicGhoul(info);
                case 29:
                    return new BoneSpearman(info);
                case 30:
                    return new BoneLord(info);
                case 31:
                    return new RightGuard(info);
                case 32:
                    return new LeftGuard(info);
                case 33:
                    return new MinotaurKing(info);
                case 34:
                    return new FrostTiger(info);
                case 35:
                    return new SandWorm(info);
                case 36:
                    return new Yimoogi(info);
                case 37:
                    return new CrystalSpider(info);
                case 38:
                    return new HolyDeva(info);
                case 39:
                    return new RootSpider(info);
                case 40:
                    return new BombSpider(info);
                case 41:
                case 42:
                    return new YinDevilNode(info);
                case 43:
                    return new OmaKing(info);
                case 44:
                    return new BlackFoxman(info);
                case 45:
                    return new RedFoxman(info);
                case 46:
                    return new WhiteFoxman(info);
                case 47:
                    return new TrapRock(info);
                case 48:
                    return new GuardianRock(info);
                case 49:
                    return new ThunderElement(info);
                case 50:
                    return new GreatFoxSpirit(info);
                case 51:
                    return new HedgeKekTal(info);
                case 52:
                    return new EvilMir(info);
                case 53:
                    return new EvilMirBody(info);
                case 54:
                    return new DragonStatue(info);
                case 55:
                    return new HumanWizard(info);
                case 56:
                    return new Trainer(info);
                case 57:
                    return new TownArcher(info);
                case 58:
                    return new Guard(info);
                case 59:
                    return new HumanAssassin(info);
                case 60:
                    return new VampireSpider(info);
                case 61:
                    return new SpittingToad(info);
                case 62:
                    return new SnakeTotem(info);
                case 63:
                    return new CharmedSnake(info);
                case 64:
                    return new IntelligentCreatureObject(info);
                case 65:
                    return new MutatedManworm(info);
                case 66:
                    return new CrazyManworm(info);
                case 67:
                    return new DarkDevourer(info);
                case 68:
                    return new Football(info);
                case 69:
                    return new PoisonHugger(info);
                case 70:
                    return new Hugger(info);
                case 71:
                    return new Behemoth(info);      // 怨恶  暗黑酋长
                case 72:
                    return new FinialTurtle(info);
                case 73:
                    return new TurtleKing(info);
                case 74:
                    return new LightTurtle(info);
                case 75:
                    return new WitchDoctor(info);
                case 76:
                    return new HellSlasher(info);
                case 77:
                    return new HellPirate(info);
                case 78:
                    return new HellCannibal(info);
                case 79:
                    return new HellKeeper(info);
                case 80:
                    return new ConquestArcher(info);
                case 81:
                    return new Gate(info);
                case 82:
                    return new Wall(info);
                case 83:
                    return new Tornado(info);
                case 84:
                    return new WingedTigerLord(info);

                case 85:
                    return new IceGate(info);

                case 86:
                    return new ManectricClaw(info);
                case 87:
                    return new ManectricBlest(info);
                case 88:
                    return new ManectricKing(info);
                case 89:
                    return new IcePillar(info);
                case 90:
                    return new TrollBomber(info);
                case 91:
                    return new TrollKing(info);
                case 92:
                    return new FlameSpear(info);
                case 93:
                    return new FlameMage(info);
                case 94:
                    return new FlameScythe(info);
                case 95:
                    return new FlameAssassin(info);
                case 96:
                    return new FlameQueen(info);
                case 97:
                    return new HellKnight(info);
                case 98:
                    return new HellLord(info);
                case 99:
                    return new HellBomb(info);
                case 100:
                    return new VenomSpider(info);

                case 101:
                    return new DeathWolf(info);


                case 102:
                    return new FrozenMiner(info);  // unused
                case 103:
                    return new FrozenAxeman(info);// unused
                case 104:
                    return new FrozenMagician(info);// unused
                case 105:
                    return new OrcCommander(info);// unused
                case 106:
                    return new OrcMutant(info);// unused
                case 107:
                    return new OrcGeneral(info);// unused

                case 108:
                    return new SandDragon(info);// unused
                case 109:
                    return new OrcWizard(info);// unused
                case 110:
                    return new OrcWithAnimal(info);// unused
                case 111:
                    return new TucsonMage(info);
                case 112:
                    return new TucsonWarrior(info);
                case 113:
                    return new Armadillo(info);
                case 114:
                    return new ArmadilloElder(info);
                case 115:
                    return new SandSnail(info);
                case 116:
                    return new CannibalTentacles(info);// unused
                case 117:
                    return new TucsonGeneral(info);   // 异秘

                case 118:
                    return new CatJar1(info);
                case 119:
                    return new CatJar2(info);
                case 120:
                    return new CatRestlessJar(info);
                case 121:
                    return new BeastKing(info);  // 野兽王？
                case 122:
                    return new TrollStoner(info);// unused
                case 123:
                    return new SnowFlowerQueen(info);
                case 124:
                    return new SnowFlower(info);
                case 125:
                    return new SnowMouse(info);

                case 127:
                    return new TreeQueen(info);
                case 128:
                    return new KillerPlant(info);
                case 135:
                    return new ShellFighter(info);
                case 136:
                    return new HornedSorceror(info);
                case 137:
                    return new HornedCommander(info);
                case 138:
                    return new Kirin(info);
                case 139:
                    return new SnowWolfKing(info);
                case 140:
                    return new SecretQueen(info);
                case 141:
                    return new SeedingsGeneral(info);
                case 142:
                    return new TucsonEgg(info);
                case 143:
                    return new TucsonPlagued(info);

                case 144:
                    return new GasToad(info);
                case 145:
                    return new SwampWarrior(info);
                case 146:
                    return new AssassinBird(info);
                case 147:
                    return new RhinoWarrior(info);
                case 148:
                    return new RhinoPriest(info);
                case 149:
                    return new SwampSlime(info);

                case 150:
                    return new OmaSlasher(info);
                case 151:
                    return new OmaBlest(info);
                case 152:
                    return new OmaCannibal(info);
                case 153:
                    return new OmaAssassin(info);
                case 154:
                    return new OmaMage(info);
                case 155:
                    return new OmaWitchDoctor(info);

                case 156:
                    return new LightningBead(info);// unused
                case 157:
                    return new HealingBead(info);// unused
                case 158:
                    return new PowerUpBead(info);// unused
                case 159:
                    return new DarkOmaKing(info);// unused


                case 160:
                    return new Lord(info);// unused
                case 161:
                    return new BlackDragon_Mob(info);// unused
                case 162:
                    return new FalconLord(info);// unused
                case 163:
                    return new BearMinotaurLrod(info);// unused
                case 164:
                    return new Taganda(info);//MC (wave) DC (swing & Normal)// unused
                case 165:
                    return new NumaMage(info);//MC// unused
                case 166:
                    return new CursedCactus(info);//DC// unused

                case 180:
                    return new DarkWraith(info);// unused
                case 181:
                    return new DarkSpirit(info);// unused
                case 182:
                    return new CrystalBeast(info); // 冰雪守护神
                case 183:
                    return new FlyingStatue(info);
                case 184:
                    return new StrayCat(info);
                case 185:
                    return new BlackHammerCat(info);
                case 186:
                    return new CatShaman(info);
                case 187:
                    return new IceGuard(info);// unused
                case 188:
                    return new ElementGuard(info);// unused
                case 189:
                    return new KingGuard(info);// unused
                case 190:
                    return new HumanMonster(info);
                case 191:
                    return new MirKing(info);// unused

                case 192: return new CatBigBoss(info);
                case 200: return new Runaway(info);
                case 201: return new TalkingMonster(info);
                case 253: return new FlamingMutant(info);
                case 254: return new StoningStatue(info);
                case 255: return new AncientBringer(info);

                case 304: return new RockGuard(info);
                case 305: return new MudWarrior(info);
                case 306: return new SmallPot(info);

                case 322: return new Mandrill(info);
                case 323: return new PlagueCrab(info);
                case 324: return new CreeperPlant(info);
                case 325: return new FloatingWraith(info);
                case 326: return new ArmedPlant(info);
                case 327: return new AvengerPlant(info);
                case 328: return new Nadz(info);
                case 329: return new AvengingSpirit(info);
                case 330: return new AvengingWarrior(info);
                case 331: return new AxePlant(info);
                case 332: return new WoodBox(info);
                case 333: return new ClawBeast(info);

                case 335: return new SackWarrior(info);
                case 336: return new WereTiger(info);
                case 337: return new KingHydrax(info);
                case 338: return new Hydrax(info);
                case 339: return new HornedMage(info);
                //  case 340: return new Basiloid(info);
                case 341: return new HornedArcher(info);
                case 342: return new ColdArcher(info);
                case 343: return new HornedWarrior(info);
                case 344: return new FloatingRock(info);
                case 345: return new ScalyBeast(info);
                case 346: return new HornedSorceror(info);

                case 347: return new BoulderSpirit(info);
                case 348: return new HornedCommander(info);
                case 349: return new MoreaWind(info);



                // 玄月林case 349: return new MoonStone(info);
                case 350: return new SunStone(info);
                //case 351: return new LightningStone(info);
                case 352: return new TurtleGrass(info);
                case 353: return new Mantree(info);
                case 354: return new Bear(info);
                //case 355: return new Leopard(info);
                case 356: return new ChieftainArcher(info);
                case 357: return new ChieftainSword(info);
                //case 358: return new StoningSpider(info);
                case 359: return new VampireSpider(info);
                case 360: return new SpittingToad(info);
                case 361: return new SnakeTotem(info);
                case 362: return new CharmedSnake(info);



                // 雪原case 363: return new FrozenSoldier(info);
                case 364: return new FrozenFighter(info);
                case 365: return new FrozenArcher(info);
                case 366: return new FrozenKnight(info);
                case 367: return new FrozenGolem(info);
                case 368: return new IcePhantom(info);
                case 369: return new SnowWolf(info);
                case 370: return new SnowWolfKing(info);

                case 371: return new WaterDragon(info);
                case 372: return new BlackTortoise(info);
                case 373: return new Manticore(info);
                case 374: return new DragonWarrior(info);
                case 375: return new DragonArcher(info);
                case 376: return new Kirin(info);
                //   case 377: return new Guard3(info);
                //   case 378: return new ArcherGuard3(info);
                //   case 379: return new Bunny2(info);

                // 冰川
                case 380: return new FrozenMiner(info);
                case 381: return new FrozenAxeman(info);
                case 382: return new FrozenMagician(info);
                case 383: return new SnowYeti(info);
                case 384: return new IceCrystalSoldier(info);
                case 385: return new DarkWraith(info);
                case 386: return new DarkSpirit(info);
                case 387: return new CrystalBeast(info);

                // 冰原
                case 403: return new SnowFlower(info);
                case 404: return new SnowMouse(info);
                case 405: return new SnowSnail(info);
                case 406: return new SnowWarrior(info);
                case 407: return new SnowArchor(info);
                case 408: return new SnowAssassin(info);
                case 409: return new SnowFlowerQueen(info);

                // 地宫
                case 410: return new SecretWarrior2(info);
                case 411: return new SecretWarrior3(info);
                case 412: return new SecretWarrior4(info);
                case 413: return new SecretKnight(info);
                case 414: return new SecretPaper1(info);
                case 415: return new SecretPaper2(info);
                case 416: return new SecretPaper3(info);
                case 417: return new SecretPaper4(info);
                case 418: return new SecretJudge(info);
                case 419: return new SecretWizard(info);
                case 420: return new SecretWarrior8(info);
                case 421: return new SecretQueen(info);

                // 昆仑
                case 450: return new KunLun0(info);                case 451: return new KunLun1(info);
                case 452: return new KunLun2(info);
                case 453: return new KunLun3(info);
                case 454: return new KunLun4(info);
                case 455: return new KunLun5(info);
                case 456: return new KunLun6(info);
                case 457: return new KunLun7(info);
                case 458: return new KunLun8(info);
                case 459: return new KunLun9(info);
                case 460: return new KunLun10(info);
                case 461: return new KunLun11(info);
                case 462: return new KunLun12(info);
                case 463: return new KunLun13(info);
                case 464: return new KunLun14(info);

                // 毒妖林
                case 465: return new DogYoLin0(info);
                case 466: return new DogYoLin1(info);
                case 467: return new DogYoLin2(info);
                case 468: return new DogYoLin3(info);
                case 469: return new DogYoLin4(info);
                case 470: return new DogYoLin5(info);
                case 471: return new DogYoLin6(info);
                case 472: return new DogYoLin7(info);
                case 473: return new DogYoLin8(info);
                case 474: return new DogYoLinDoor(info);

                default:
                    return new MonsterObject(info);
            }
        }

        public override ObjectType Race
        {
            get { return ObjectType.Monster; }
        }

        public MonsterInfo Info;
        public MapRespawn Respawn;

        public override string Name
        {
            get { return Master == null ? Info.GameName : string.Format("{0}({1})", Info.GameName, Master.Name); }
            set { throw new NotSupportedException(); }
        }

        public override int CurrentMapIndex { get; set; }
        public override Point CurrentLocation { get; set; }
        public override sealed MirDirection Direction { get; set; }
        public override ushort Level
        {
            get { return Info.Level; }
            set { throw new NotSupportedException(); }
        }

        public override sealed AttackMode AMode
        {
            get
            {
                return base.AMode;
            }
            set
            {
                base.AMode = value;
            }
        }
        public override sealed PetMode PMode
        {
            get
            {
                return base.PMode;
            }
            set
            {
                base.PMode = value;
            }
        }

        public override uint Health
        {
            get { return HP; }
        }

        public override uint MaxHealth
        {
            get { return MaxHP; }
        }

        public uint HP, MaxHP;
        public ushort MoveSpeed;

        public virtual uint Experience
        {
            get { return Info.Experience; }
        }
        public int DeadDelay
        {
            get
            {
                switch (Info.AI)
                {
                    case 81:
                    case 82:
                        return int.MaxValue;
                    case 252:
                        return 5000;
                    default:
                        return 180000;
                }
            }
        }
        public const int RegenDelay = 10000, EXPOwnerDelay = 5000, SearchDelay = 3000, RoamDelay = 1000, HealDelay = 600, RevivalDelay = 2000;
        public long ActionTime, MoveTime, AttackTime, RegenTime, DeadTime, SearchTime, RoamTime, HealTime;
        public long ShockTime, RageTime, HallucinationTime;
        public bool BindingShotCenter, PoisonStopRegen = true;

        public byte PetLevel;
        public uint PetExperience;
        public byte MaxPetLevel;
        public long TameTime;
        public long StruckTime;

        public int RoutePoint;
        public bool Waiting;

        public List<MonsterObject> SlaveList = new List<MonsterObject>();
        public List<RouteInfo> Route = new List<RouteInfo>();

        public override bool Blocking
        {
            get
            {
                return !Dead;
            }
        }
        protected virtual bool CanRegen
        {
            get { return Envir.Time >= RegenTime; }
        }
        protected virtual bool CanMove
        {
            get
            {
                return !Dead && Envir.Time > MoveTime && Envir.Time > StruckTime && Envir.Time > ActionTime && Envir.Time > ShockTime &&
                       (Master == null || Master.PMode == PetMode.MoveOnly || Master.PMode == PetMode.Both) && (CurrentPoison == PoisonType.None 
                       || (!CurrentPoison.HasFlag(PoisonType.Paralysis) 
                       && !CurrentPoison.HasFlag(PoisonType.LRParalysis) 
                       && !CurrentPoison.HasFlag(PoisonType.Stun)
                       && !CurrentPoison.HasFlag(PoisonType.Frozen)));
            }
        }
        protected virtual bool CanAttack
        {
            get
            {
                return !Dead && Envir.Time > AttackTime && Envir.Time > StruckTime && Envir.Time > ActionTime &&
                     (Master == null || Master.PMode == PetMode.AttackOnly || Master.PMode == PetMode.Both || !CurrentMap.Info.NoFight) && !CurrentPoison.HasFlag(PoisonType.Paralysis)
                       && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Stun) && !CurrentPoison.HasFlag(PoisonType.Frozen);
            }
        }

        protected internal MonsterObject(MonsterInfo info)
        {
            Info = info;

            Undead = Info.Undead;
            AutoRev = info.AutoRev;
            CoolEye = info.CoolEye > Envir.Random.Next(100);
            Direction = (MirDirection)Envir.Random.Next(8);

            AMode = AttackMode.All;
            PMode = PetMode.Both;

            RegenTime = Envir.Random.Next(RegenDelay) + Envir.Time;
            SearchTime = Envir.Random.Next(SearchDelay) + Envir.Time;
            RoamTime = Envir.Random.Next(RoamDelay) + Envir.Time;

            PoisonResist = info.PoisonResist;
        }
        public bool Spawn(Map temp, Point location)
        {
            if (!temp.ValidPoint(location)) return false;

            CurrentMap = temp;
            CurrentLocation = location;

            CurrentMap.AddObject(this);

            RefreshAll();
            SetHP(MaxHP);

            Spawned();
            Envir.MonsterCount++;
            CurrentMap.MonsterCount++;
            return true;
        }
        public bool Spawn(MapRespawn respawn, Point location)
        {
            Respawn = respawn;

            CurrentLocation = location;

            respawn.Map.AddObject(this);

            CurrentMap = respawn.Map;

            if (Respawn.Route.Count > 0)
                Route.AddRange(Respawn.Route);

            RefreshAll();
            SetHP(MaxHP);

            Spawned();
            Respawn.Count++;
            respawn.Map.MonsterCount++;
            Envir.MonsterCount++;
            return true;
        }

        public override void Spawned()
        {
            base.Spawned();
            ActionTime = Envir.Time + 2000;
            if (Info.HasSpawnScript && (SMain.Envir.MonsterNPC != null))
            {
                SMain.Envir.MonsterNPC.Call(this, string.Format("[@_SPAWN({0})]", Info.Index));
            }
        }

        protected virtual void RefreshBase()
        {
            MaxHP = Info.HP;
            MinAC = Info.MinAC;
            MaxAC = Info.MaxAC;
            MinMAC = Info.MinMAC;
            MaxMAC = Info.MaxMAC;
            MinDC = Info.MinDC;
            MaxDC = Info.MaxDC;
            MinMC = Info.MinMC;
            MaxMC = Info.MaxMC;
            MinSC = Info.MinSC;
            MaxSC = Info.MaxSC;
            Accuracy = Info.Accuracy;
            Agility = Info.Agility;

            MoveSpeed = Info.MoveSpeed;
            AttackSpeed = Info.AttackSpeed;
        }
        public virtual void RefreshAll()
        {
            RefreshBase();

            MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + PetLevel * 2);
            MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + PetLevel * 2);
            MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + PetLevel * 2);
            MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + PetLevel * 2);
            MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + PetLevel);
            MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + PetLevel);

            byte regionID = 1;
            if (Master != null && Master is PlayerObject)
            {
                PlayerObject p = (PlayerObject)(Master);
                regionID = p.GetRegionID();
            }

            if (Info.Name == Settings.SkeletonName || Info.Name == Settings.ShinsuName || Info.Name == Settings.AngelName)
            {
                MoveSpeed = (ushort)Math.Min(ushort.MaxValue, (Math.Max(ushort.MinValue, MoveSpeed - MaxPetLevel * 130)));
                AttackSpeed = (ushort)Math.Min(ushort.MaxValue, (Math.Max(ushort.MinValue, AttackSpeed - MaxPetLevel * 80)));

                if (Master != null)
                {
                    int sc = (int)(Master.GetAttackPower(Master.MinSC, Master.MaxSC) * 0.5F);

                    if (Info.Name == Settings.SkeletonName)
                    {
                        sc = (int)(Master.GetAttackPower(Master.MinSC, Master.MaxSC) * 0.2F);
                    }

                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + sc);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + sc);
                    if (Envir.IsNewVersion(regionID))
                    {
                        MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + Master.Level * 0.8);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + Master.Level * 0.8);
                        MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMC + sc);
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMC + sc);
                    }
                    else
                    {
                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + sc);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + sc);
                    }
                }
            }

            if (Info.Name == Settings.SkeletonName || Info.Name == Settings.ShinsuName)
            {
                MaxHP = (uint)Math.Min(uint.MaxValue, MaxHP + (3 + PetLevel) * PetLevel * 0.05 * MaxHP);
            }
            else
            {
                MaxHP = (uint)Math.Min(uint.MaxValue, MaxHP + PetLevel * 20);
            }

            if (Info.Name == Settings.SnakesName || Info.Name == Settings.SnakeTotemName || Info.Name == Settings.ToadName)
            {
                if (Master != null)
                {
                     int mc = (int)(Master.GetAttackPower(Master.MinMC, Master.MaxMC) * 0.5F);
                     MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + mc);
                     MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + mc);
                }
            }

            if (Info.Name == Settings.CloneName && Master != null)
            {
                MinMC = Master.MinMC;
                MaxMAC = Master.MaxMC;
            }

            if (MoveSpeed < 400) MoveSpeed = 400;

            if (Info.Name == Settings.ShinsuName || Info.Name == Settings.AngelName)
            {
                if (AttackSpeed < 500) AttackSpeed = 500;
            }
            else
            {
                if (AttackSpeed < 400) AttackSpeed = 400;
            }

            RefreshBuffs();
        }
        protected virtual void RefreshBuffs()
        {
            for (int i = 0; i < Buffs.Count; i++)
            {
                Buff buff = Buffs[i];

                if (buff.Values == null || buff.Values.Length < 1) continue;

                switch (buff.Type)
                {
                    case BuffType.Haste:
                        ASpeed = (sbyte)Math.Max(sbyte.MinValue, (Math.Min(sbyte.MaxValue, ASpeed + buff.Values[0])));
                        break;
                    case BuffType.SwiftFeet:
                        MoveSpeed = (ushort)Math.Max(ushort.MinValue, MoveSpeed + 100 * buff.Values[0]);
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
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[0]);
                        break;
                    case BuffType.Curse:
                        ushort rMaxDC = (ushort)(((float)MaxDC / 100) * buff.Values[0]);
                        ushort rMaxMC = (ushort)(((float)MaxMC / 100) * buff.Values[0]);
                        ushort rMaxSC = (ushort)(((float)MaxSC / 100) * buff.Values[0]);
                        sbyte rASpeed = (sbyte)(((float)ASpeed / 100) * buff.Values[0]);
                        ushort rMSpeed = (ushort)(((float)MoveSpeed / 100) * buff.Values[0]);

                        MaxDC = (ushort)Math.Max(ushort.MinValue, MaxDC - rMaxDC);
                        MaxMC = (ushort)Math.Max(ushort.MinValue, MaxMC - rMaxMC);
                        MaxSC = (ushort)Math.Max(ushort.MinValue, MaxSC - rMaxSC);
                        ASpeed = (sbyte)Math.Min(sbyte.MaxValue, (Math.Max(sbyte.MinValue, ASpeed - rASpeed)));
                        MoveSpeed = (ushort)Math.Max(ushort.MinValue, MoveSpeed - rMSpeed);
                        break;

                    case BuffType.PetEnhancer:
                        MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + buff.Values[0]);
                        MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + buff.Values[0]);
                        MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + buff.Values[1]);
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[1]);
                        MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + buff.Values[2]);
                        MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + buff.Values[2]);
                        break;

                    case BuffType.Defence:
                        MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + buff.Values[0]);
                        break;
                    case BuffType.MagicDefence:
                        MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + buff.Values[0]);
                        break;
                }

            }
        }
        public void RefreshNameColour(bool send = true)
        {
            if (ShockTime < Envir.Time) BindingShotCenter = false;

            Color colour = Color.White;

            switch (PetLevel)
            {
                case 1:
                    colour = Color.Aqua;
                    break;
                case 2:
                    colour = Color.Aquamarine;
                    break;
                case 3:
                    colour = Color.LightSeaGreen;
                    break;
                case 4:
                    colour = Color.SlateBlue;
                    break;
                case 5:
                    colour = Color.SteelBlue;
                    break;
                case 6:
                    colour = Color.Blue;
                    break;
                case 7:
                    colour = Color.Navy;
                    break;
            }

            if (Envir.Time < ShockTime)
                colour = Color.Peru;
            else if (Envir.Time < RageTime)
                colour = Color.Red;
            else if (Envir.Time < HallucinationTime)
                colour = Color.MediumOrchid;

            if (colour == NameColour || !send) return;

            NameColour = colour;

            Broadcast(new S.ObjectColourChanged { ObjectID = ObjectID, NameColour = NameColour });
        }

        public void SetHP(uint amount)
        {
            if (HP == amount) return;

            HP = amount <= MaxHP ? amount : MaxHP;

            if (!Dead && HP == 0) Die();

            //  HealthChanged = true;
            BroadcastHealthChange();
        }
        public virtual void ChangeHP(int amount)
        {

            uint value = (uint)Math.Max(uint.MinValue, Math.Min(MaxHP, HP + amount));

            if (value == HP) return;

            HP = value;

            if (!Dead && HP == 0) Die();

            // HealthChanged = true;
            BroadcastHealthChange();
        }

        //use this so you can have mobs take no/reduced poison damage
        public virtual void PoisonDamage(int amount, MapObject Attacker)
        {
            ChangeHP(amount);
        }


        public override bool Teleport(Map temp, Point location, bool effects = true, byte effectnumber = 0)
        {
            if (temp == null || !temp.ValidPoint(location)) return false;

            CurrentMap.RemoveObject(this);
            if (effects) Broadcast(new S.ObjectTeleportOut { ObjectID = ObjectID, Type = effectnumber });
            Broadcast(new S.ObjectRemove { ObjectID = ObjectID });

            CurrentMap.MonsterCount--;

            CurrentMap = temp;
            CurrentLocation = location;

            CurrentMap.MonsterCount++;

            InTrapRock = false;

            CurrentMap.AddObject(this);
            BroadcastInfo();

            if (effects) Broadcast(new S.ObjectTeleportIn { ObjectID = ObjectID, Type = effectnumber });

            BroadcastHealthChange();

            return true;
        }


        public override void Die()
        {
            if (Dead) return;

            HP = 0;
            Dead = true;

            DeadTime = Envir.Time + DeadDelay;

            Broadcast(new S.ObjectDied { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            if (Info != null || Info.HasDieScript && (SMain.Envir.MonsterNPC != null))
            {
                SMain.Envir.MonsterNPC.Call(this, string.Format("[@_DIE({0})]", Info.Index));
            }

            if (EXPOwner != null && Master == null && EXPOwner.Race == ObjectType.Player)
            {
                EXPOwner.WinExp(Experience, Level);

                PlayerObject playerObj = (PlayerObject)EXPOwner;
                playerObj.CheckGroupQuestKill(Info);

                playerObj.CallDefaultNPC(DefaultNPCType.KillMonster, Info.Index);
            }

            if (Respawn != null)
                Respawn.Count--;

            if (Master == null && EXPOwner != null)
                Drop();

            Master = null;

            PoisonList.Clear();
            Envir.MonsterCount--;
            if (CurrentMap != null)
                CurrentMap.MonsterCount--;
            else
                SMain.Enqueue(string.Format("Current Map is null {0}", Info.Name));
        }

        public void Revive(uint hp, bool effect)
        {
            if (!Dead) return;

            SetHP(hp);

            Dead = false;
            ActionTime = Envir.Time + RevivalDelay;

            Broadcast(new S.ObjectRevived { ObjectID = ObjectID, Effect = effect });

            if (Respawn != null)
                Respawn.Count++;

            Envir.MonsterCount++;
            CurrentMap.MonsterCount++;
        }

        public override int Pushed(MapObject pusher, MirDirection dir, int distance)
        {
            if (!Info.CanPush) return 0;
            //if (!CanMove) return 0; //stops mobs that can't move (like cannibalplants) from being pushed

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

                Broadcast(new S.ObjectPushed { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

                result++;
            }

            ActionTime = Envir.Time + 300 * result;
            MoveTime = Envir.Time + 500 * result;

            if (result > 0)
            {
                var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
                if (cellObjects == null) return result;

                for (int i = 0; i < cellObjects.Count; i++)
                {
                    if (cellObjects[i].Race != ObjectType.Spell) continue;
                    SpellObject ob = (SpellObject)cellObjects[i];

                    ob.ProcessSpell(this);
                    //break;
                }
            }

            return result;
        }

        protected virtual void Drop()
        {
            for (int i = 0; i < Info.Drops.Count; i++)
            {
                DropInfo drop = Info.Drops[i][0];

                if (drop.Item == null)
                    continue;

                if (Envir.IsNewVersion(CurrentMap.Region) && drop.Item.Name.Equals("罗汉金身"))
                    continue;

                int rate = (int)(drop.Chance / (Settings.DropRate));

                if (EXPOwner != null && EXPOwner.ItemDropRateOffset > 0)
                    rate  = (int)(rate / ((100F + EXPOwner.ItemDropRateOffset) / 100));

                if (rate < 1) rate = 1;

                if (Envir.Random.Next(rate) != 0) continue;

                if (Info.Drops[i].Count > 0)
                {
                    drop = Info.Drops[i][Envir.Random.Next(Info.Drops[i].Count)];
                }

                if (drop.Gold > 0)
                {
                    int lowerGoldRange = (int)(drop.Gold / 2);
                    int upperGoldRange = (int)(drop.Gold + drop.Gold / 2);

                    if (EXPOwner != null && EXPOwner.GoldDropRateOffset > 0)
                        lowerGoldRange += (int)(lowerGoldRange * (EXPOwner.GoldDropRateOffset / 100));

                    if (lowerGoldRange > upperGoldRange) lowerGoldRange = upperGoldRange;

                    int gold = Envir.Random.Next(lowerGoldRange, upperGoldRange);

                    if (gold <= 0) continue;

                    if (!DropGold((uint)gold)) return;
                }
                else
                {
                    UserItem item = Envir.CreateDropItem(drop.Item);

                    if (item == null) continue;

                    if (EXPOwner != null && EXPOwner.Race == ObjectType.Player)
                    {
                        PlayerObject ob = (PlayerObject)EXPOwner;

                        if (ob.CheckGroupQuestItem(item))
                        {
                            continue;
                        }
                    }

                    if (drop.QuestRequired) continue;
                    if (!DropItem(item)) return;
                }
            }
        }

        protected virtual bool DropItem(UserItem item)
        {
            if (CurrentMap.Info.NoDropMonster)
                return false;

            ItemObject ob = new ItemObject(this, item)
            {
                Owner = EXPOwner,
                OwnerTime = Envir.Time + Settings.Minute,
            };

            if (!item.Info.GlobalDropNotify)
                return ob.Drop(Settings.DropRange);

            foreach (var player in Envir.Players)
            {
                if (player.GetRegionID() != CurrentMap.Region)
                    continue;

                player.ReceiveChat(string.Format(Envir.Tr("{0} {1} {2} has dropped {3}."), CurrentMap.Region, CurrentMap.Info.Title, Name, item.FriendlyName),
                                 ChatType.System2);
            }

            return ob.Drop(Settings.DropRange);
        }

        protected virtual bool DropGold(uint gold)
        {
            if (EXPOwner != null && EXPOwner.CanGainGold(gold) && !Settings.DropGold)
            {
                EXPOwner.WinGold(gold);
                return true;
            }

            uint count = gold / Settings.MaxDropGold == 0 ? 1 : gold / Settings.MaxDropGold + 1;
            for (int i = 0; i < count; i++)
            {
                ItemObject ob = new ItemObject(this, i != count - 1 ? Settings.MaxDropGold : gold % Settings.MaxDropGold)
                {
                    Owner = EXPOwner,
                    OwnerTime = Envir.Time + Settings.Minute,
                };

                ob.Drop(Settings.DropRange);
            }

            return true;
        }

        public override void Process()
        {
            if (MagicShield && Envir.Time > MagicShieldTime)
            {
                MagicShield = false;
                MagicShieldLv = 0;
                MagicShieldTime = 0;
                CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.MagicShieldDown }, CurrentLocation);
                RemoveBuff(BuffType.MagicShield);
            }

            base.Process();

            RefreshNameColour();

            if (Target != null && (Target.CurrentMap != CurrentMap || !Target.IsAttackTarget(this) || !Functions.InRange(CurrentLocation, Target.CurrentLocation, Globals.DataRange)))
                Target = null;

            for (int i = SlaveList.Count - 1; i >= 0; i--)
                if (SlaveList[i].Dead || SlaveList[i].Node == null)
                    SlaveList.RemoveAt(i);

            if (Dead && Envir.Time >= DeadTime)
            {
                CurrentMap.RemoveObject(this);
                if (Master != null)
                {
                    Master.Pets.Remove(this);
                    Master = null;
                }

                Despawn();
                return;
            }

            if (Master != null && TameTime > 0 && Envir.Time >= TameTime)
            {
                Master.Pets.Remove(this);
                Master = null;
                Broadcast(new S.ObjectName { ObjectID = ObjectID, Name = Name });
            }

            ProcessAI();

            ProcessBuffs();
            ProcessRegen();
            ProcessPoison();


            /*   if (!HealthChanged) return;

               HealthChanged = false;

               BroadcastHealthChange();*/
        }

        public override void SetOperateTime()
        {
            long time = Envir.Time + 2000;

            if (DeadTime < time && DeadTime > Envir.Time)
                time = DeadTime;

            if (OwnerTime < time && OwnerTime > Envir.Time)
                time = OwnerTime;

            if (ExpireTime < time && ExpireTime > Envir.Time)
                time = ExpireTime;

            if (PKPointTime < time && PKPointTime > Envir.Time)
                time = PKPointTime;

            if (LastHitTime < time && LastHitTime > Envir.Time)
                time = LastHitTime;

            if (EXPOwnerTime < time && EXPOwnerTime > Envir.Time)
                time = EXPOwnerTime;

            if (SearchTime < time && SearchTime > Envir.Time)
                time = SearchTime;

            if (RoamTime < time && RoamTime > Envir.Time)
                time = RoamTime;


            if (ShockTime < time && ShockTime > Envir.Time)
                time = ShockTime;

            if (RegenTime < time && RegenTime > Envir.Time && Health < MaxHealth)
                time = RegenTime;

            if (RageTime < time && RageTime > Envir.Time)
                time = RageTime;

            if (HallucinationTime < time && HallucinationTime > Envir.Time)
                time = HallucinationTime;

            if (ActionTime < time && ActionTime > Envir.Time)
                time = ActionTime;

            if (MoveTime < time && MoveTime > Envir.Time)
                time = MoveTime;

            if (AttackTime < time && AttackTime > Envir.Time)
                time = AttackTime;

            if (HealTime < time && HealTime > Envir.Time && HealAmount > 0)
                time = HealTime;

            if (BrownTime < time && BrownTime > Envir.Time)
                time = BrownTime;

            for (int i = 0; i < ActionList.Count; i++)
            {
                if (ActionList[i].Time >= time && ActionList[i].Time > Envir.Time) continue;
                time = ActionList[i].Time;
            }

            for (int i = 0; i < PoisonList.Count; i++)
            {
                if (PoisonList[i].TickTime >= time && PoisonList[i].TickTime > Envir.Time) continue;
                time = PoisonList[i].TickTime;
            }

            for (int i = 0; i < Buffs.Count; i++)
            {
                if (Buffs[i].ExpireTime >= time && Buffs[i].ExpireTime > Envir.Time) continue;
                time = Buffs[i].ExpireTime;
            }


            if (OperateTime <= Envir.Time || time < OperateTime)
                OperateTime = time;
        }

        public override void Process(DelayedAction action)
        {
            switch (action.Type)
            {
                case DelayedType.Magic:
                    CompleteMagic(action.Params);
                    break;
                case DelayedType.Damage:
                    CompleteAttack(action.Params);
                    break;
                case DelayedType.RangeDamage:
                    CompleteRangeAttack(action.Params);
                    break;
                case DelayedType.Die:
                    CompleteDeath(action.Params);
                    break;
                case DelayedType.Recall:
                    PetRecall();
                    break;
                case DelayedType.Teleport:
                    DelayTeleport(action.Params);
                    break;
            }
        }
        private void DelayTeleport(IList<object> data)
        {
            if (data.Count < 3)
                return;

            Map map = (Map)data[0];
            Point location = (Point)data[1];
            bool effects = (bool)data[2];

            Teleport(CurrentMap, location, false);
        }

        private void CompleteMagic(IList<object> data)
        {
            UserMagic magic = (UserMagic)data[0];
            int value;
            MapObject target;
            switch (magic.Spell)
            {
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
                default:
                    {
                        value = (int)data[1];
                        target = (MapObject)data[2];

                        if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                        target.Attacked(this, value, DefenceType.MAC);
                    }
                    break;
            }
        }

        private void LevelMagic(UserMagic magic)
        {
        }

        public void PetRecall()
        {
            if (Master == null) return;
            if (!Teleport(Master.CurrentMap, Master.Back))
                Teleport(Master.CurrentMap, Master.CurrentLocation);
        }
        protected virtual void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.Attacked(this, damage, defence);
        }

        protected virtual void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.Attacked(this, damage, defence);
        }

        protected virtual void CompleteDeath(IList<object> data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ProcessRegen()
        {
            if (Dead) return;

            int healthRegen = 0;

            if (CanRegen)
            {
                RegenTime = Envir.Time + RegenDelay;


                if (HP < MaxHP)
                    healthRegen += (int)(MaxHP * 0.022F) + 1;
            }


            if (Envir.Time > HealTime)
            {
                HealTime = Envir.Time + HealDelay;

                if (HealAmount > 5)
                {
                    healthRegen += 5;
                    HealAmount -= 5;
                }
                else
                {
                    healthRegen += HealAmount;
                    HealAmount = 0;
                }
            }

            if (healthRegen > 0) ChangeHP(healthRegen);
            if (HP == MaxHP) HealAmount = 0;
        }
        protected virtual void ProcessPoison()
        {
            PoisonType type = PoisonType.None;
            ArmourRate = 1F;
            DamageRate = 1F;

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (Dead) return;

                Poison poison = PoisonList[i];
                //if (poison.Owner != null && poison.Owner.Node == null || Functions.Distance(CurrentLocation, poison.Owner.CurrentLocation) > Globals.DataRange)
                if ((poison.Owner != null && poison.Owner.Node == null))
                {
                    PoisonList.RemoveAt(i);
                    continue;
                }

                if (Envir.Time > poison.TickTime)
                {
                    poison.Time++;
                    poison.TickTime = Envir.Time + poison.TickSpeed;

                    if (poison.Time >= poison.Duration)
                    {
                        PoisonList.RemoveAt(i);
                    }

                    if (poison.PType == PoisonType.Green || poison.PType == PoisonType.Bleeding || poison.PType == PoisonType.Morea)
                    {
                        if (EXPOwner == null || EXPOwner.Dead)
                        {
                            EXPOwner = poison.Owner;
                            EXPOwnerTime = Envir.Time + EXPOwnerDelay;
                        }
                        else if (EXPOwner == poison.Owner
                            && poison.Owner.CurrentMap == CurrentMap 
                            && Functions.MaxDistance(poison.Owner.CurrentLocation, CurrentLocation) < Globals.DataRange)
                            EXPOwnerTime = Envir.Time + EXPOwnerDelay;

                        if (poison.PType == PoisonType.Bleeding)
                        {
                            Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Bleeding, EffectType = 0 });
                        }

                        //ChangeHP(-poison.Value);
                        PoisonDamage(-poison.Value, poison.Owner);
                        if (PoisonStopRegen)
                            RegenTime = Envir.Time + RegenDelay;
                    }

                    if (poison.PType == PoisonType.DelayedExplosion)
                    {
                        if (Envir.Time > ExplosionInflictedTime) ExplosionInflictedStage++;

                        if (!ProcessDelayedExplosion(poison))
                        {
                            ExplosionInflictedStage = 0;
                            ExplosionInflictedTime = 0;

                            if (Dead) break; //temp to stop crashing

                            PoisonList.RemoveAt(i);
                            continue;
                        }
                    }
                }

                switch (poison.PType)
                {
                    case PoisonType.Red:
                        ArmourRate -= 0.5F;
                        break;
                    case PoisonType.Stun:
                        DamageRate += 0.5F;
                        break;
                    case PoisonType.Slow:
                        MoveSpeed += Settings.SlowMoveSpeed;
                        AttackSpeed += Settings.SlowAttackSpeed;

                        if (poison.Time >= poison.Duration)
                        {
                            MoveSpeed = Info.MoveSpeed;
                            AttackSpeed = Info.AttackSpeed;
                        }
                        break;
                }
                type |= poison.PType;
                /*
                if ((int)type < (int)poison.PType)
                    type = poison.PType;
                 */
            }


            if (type == CurrentPoison) return;

            CurrentPoison = type;
            Broadcast(new S.ObjectPoisoned { ObjectID = ObjectID, Poison = type });
        }

        private bool ProcessDelayedExplosion(Poison poison)
        {
            if (Dead) return false;

            if (ExplosionInflictedStage == 0)
            {
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 0 });
                return true;
            }
            if (ExplosionInflictedStage == 1)
            {
                if (Envir.Time > ExplosionInflictedTime)
                    ExplosionInflictedTime = poison.TickTime + 2000;
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 1 });
                return true;
            }
            if (ExplosionInflictedStage == 2)
            {
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion, EffectType = 2 });
                if (poison.Owner != null)
                {
                    switch (poison.Owner.Race)
                    {
                        case ObjectType.Player:
                            PlayerObject caster = (PlayerObject)poison.Owner;
                            DelayedAction action = new DelayedAction(DelayedType.Magic, Envir.Time, caster, caster.GetMagic(Spell.DelayedExplosion), poison.Value, this.CurrentLocation, this, poison.Infect);
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


        private void ProcessBuffs()
        {
            bool refresh = false;
            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                Buff buff = Buffs[i];

                if (Envir.Time <= buff.ExpireTime) continue;

                Buffs.RemoveAt(i);

                if (buff.Visible)
                    Broadcast(new S.RemoveBuff { Type = buff.Type, ObjectID = ObjectID });

                switch (buff.Type)
                {
                    case BuffType.MoonLight:
                    case BuffType.Hiding:
                    case BuffType.DarkBody:
                    case BuffType.MoonMist:
                        Hidden = false;
                        break;
                }

                refresh = true;
            }

            if (refresh) RefreshAll();
        }
        protected virtual void ProcessAI()
        {
            if (Dead) return;

            if (Master != null)
            {
                if ((Master.PMode == PetMode.Both || Master.PMode == PetMode.MoveOnly))
                {
                    if (!Functions.InRange(CurrentLocation, Master.CurrentLocation, Globals.DataRange) || CurrentMap != Master.CurrentMap)
                        PetRecall();
                }

                if (Master.PMode == PetMode.MoveOnly || Master.PMode == PetMode.None)
                    Target = null;
            }

            ProcessSearch();
            ProcessRoam();
            ProcessTarget();
        }
        protected virtual void ProcessSearch()
        {
            if (Envir.Time < SearchTime) return;
            if (Master != null && (Master.PMode == PetMode.MoveOnly || Master.PMode == PetMode.None)) return;

            SearchTime = Envir.Time + SearchDelay;

            if (CurrentMap.Inactive(5)) return;

            //Stacking or Infront of master - Move
            bool stacking = CheckStacked();

            if (CanMove && ((Master != null && Master.Front == CurrentLocation) || stacking))
            {
                //Walk Randomly
                if (!Walk(Direction))
                {
                    MirDirection dir = Direction;

                    switch (Envir.Random.Next(3)) // favour Clockwise
                    {
                        case 0:
                            for (int i = 0; i < 7; i++)
                            {
                                dir = Functions.NextDir(dir);

                                if (Walk(dir))
                                    break;
                            }
                            break;
                        default:
                            for (int i = 0; i < 7; i++)
                            {
                                dir = Functions.PreviousDir(dir);

                                if (Walk(dir))
                                    break;
                            }
                            break;
                    }
                }
            }

            if (Target == null || Envir.Random.Next(3) == 0)
                FindTarget();
        }
        protected virtual void ProcessRoam()
        {
            if (Target != null || Envir.Time < RoamTime) return;

            if (ProcessRoute()) return;

            if (CurrentMap.Inactive(30)) return;

            if (Master != null)
            {
                MoveTo(Master.Back);
                return;
            }

            RoamTime = Envir.Time + RoamDelay;
            if (Envir.Random.Next(10) != 0) return;

            switch (Envir.Random.Next(3)) //Face Walk
            {
                case 0:
                    Turn((MirDirection)Envir.Random.Next(8));
                    break;
                default:
                    Walk(Direction);
                    break;
            }
        }
        protected virtual void ProcessTarget()
        {
            if (Target == null || !CanAttack) return;

            if (InAttackRange())
            {
                Attack();
                if (Target.Dead)
                    FindTarget();

                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);
        }
        protected virtual bool InAttackRange()
        {
            return InAttackRange(1);
        }

        protected bool InAttackRange(int range)
        {
            if (Target.CurrentMap != CurrentMap) return false;

            return Target.CurrentLocation != CurrentLocation && Functions.InRange(CurrentLocation, Target.CurrentLocation, range);
        }
        protected virtual void FindTarget()
        {
            //if (CurrentMap.Players.Count < 1) return;
            Map Current = CurrentMap;

            for (int d = 0; d <= Info.ViewRange; d++)
            {
                for (int y = CurrentLocation.Y - d; y <= CurrentLocation.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= Current.Height) break;

                    for (int x = CurrentLocation.X - d; x <= CurrentLocation.X + d; x += Math.Abs(y - CurrentLocation.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= Current.Width) break;
                        var cellObjects = Current.GetCellObjects(x, y);
                        if (cellObjects == null) continue;
                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            switch (ob.Race)
                            {
                                case ObjectType.Monster:
                                    if (!ob.IsAttackTarget(this)) continue;
                                    if (ob.Hidden && (!CoolEye || Level < ob.Level)) continue;
                                    if (ob.Hidden2) continue;
                                    if (this is TrapRock && ob.InTrapRock) continue;
                                    Target = ob;
                                    return;
                                case ObjectType.Player:
                                    PlayerObject playerob = (PlayerObject)ob;
                                    if (!ob.IsAttackTarget(this)) continue;
                                    if (playerob.GMGameMaster || ob.Hidden && (!CoolEye || Level < ob.Level) || Envir.Time < HallucinationTime) continue;

                                    Target = ob;

                                    if (Master != null)
                                    {
                                        for (int j = 0; j < playerob.Pets.Count; j++)
                                        {
                                            MonsterObject pet = playerob.Pets[j];

                                            if (!pet.IsAttackTarget(this)) continue;
                                            Target = pet;
                                            break;
                                        }
                                    }
                                    return;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool ProcessRoute()
        {
            if (Route.Count < 1) return false;

            RoamTime = Envir.Time + 500;

            if (CurrentLocation == Route[RoutePoint].Location)
            {
                if (Route[RoutePoint].Delay > 0 && !Waiting)
                {
                    Waiting = true;
                    RoamTime = Envir.Time + RoamDelay + Route[RoutePoint].Delay;
                    return true;
                }

                Waiting = false;
                RoutePoint++;
            }

            if (RoutePoint > Route.Count - 1) RoutePoint = 0;

            if (!CurrentMap.ValidPoint(Route[RoutePoint].Location)) return true;

            MoveTo(Route[RoutePoint].Location);

            return true;
        }

        protected virtual void MoveTo(Point location)
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

            if (Walk(dir)) return;

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

        public virtual void Turn(MirDirection dir)
        {
            if (!CanMove) return;

            Direction = dir;

            InSafeZone = CurrentMap.GetSafeZone(CurrentLocation) != null;

            var cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
            if (cellObjects == null)
                return;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                if (cellObjects[i].Race != ObjectType.Spell) continue;
                SpellObject ob = (SpellObject)cellObjects[i];

                ob.ProcessSpell(this);
                //break;
            }


            Broadcast(new S.ObjectTurn { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
        }

        public virtual bool Run(MirDirection dir)
        {
            if (!CanMove) return false;

            Point location = Functions.PointMove(CurrentLocation, dir, 2);

            if (!CurrentMap.ValidPoint(location)) return false;

            var cellObjects = CurrentMap.GetCellObjects(location);

            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (!ob.Blocking || Race == ObjectType.Creature) continue;

                    return false;
                }

            CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);

            Direction = dir;
            RemoveObjects(dir, 1);
            CurrentLocation = location;
            CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
            AddObjects(dir, 1);

            if (Hidden)
            {
                Hidden = false;

                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].Type != BuffType.Hiding) continue;

                    Buffs[i].ExpireTime = 0;
                    break;
                }
            }


            CellTime = Envir.Time + 500;
            ActionTime = Envir.Time + 300;
            MoveTime = Envir.Time + MoveSpeed;
            if (MoveTime > AttackTime)
                AttackTime = MoveTime;

            InSafeZone = CurrentMap.GetSafeZone(CurrentLocation) != null;

            Broadcast(new S.ObjectRun { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

            cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
            if (cellObjects == null)
                return true;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                if (cellObjects[i].Race != ObjectType.Spell) continue;
                SpellObject ob = (SpellObject)cellObjects[i];

                ob.ProcessSpell(this);
                //break;
            }

            return true;
        }

        public virtual bool Walk(MirDirection dir)
        {
            if (!CanMove) return false;

            Point location = Functions.PointMove(CurrentLocation, dir, 1);

            if (!CurrentMap.ValidPoint(location)) return false;

            var cellObjects = CurrentMap.GetCellObjects(location);

            if (cellObjects != null)
                for (int i = 0; i < cellObjects.Count; i++)
                {
                    MapObject ob = cellObjects[i];
                    if (!ob.Blocking || Race == ObjectType.Creature) continue;

                    return false;
                }

            CurrentMap.RemoveObject(CurrentLocation.X, CurrentLocation.Y, this);

            Direction = dir;
            RemoveObjects(dir, 1);
            CurrentLocation = location;
            CurrentMap.AddObject(CurrentLocation.X, CurrentLocation.Y, this);
            AddObjects(dir, 1);

            if (Hidden)
            {
                Hidden = false;

                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].Type != BuffType.Hiding) continue;

                    Buffs[i].ExpireTime = 0;
                    break;
                }
            }


            CellTime = Envir.Time + 500;
            ActionTime = Envir.Time + 300;
            MoveTime = Envir.Time + MoveSpeed;
            if (MoveTime > AttackTime)
                AttackTime = MoveTime;

            InSafeZone = CurrentMap.GetSafeZone(CurrentLocation) != null;

            Broadcast(new S.ObjectWalk { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });


            cellObjects = CurrentMap.GetCellObjects(CurrentLocation);
            if (cellObjects == null)
                return true;

            for (int i = 0; i < cellObjects.Count; i++)
            {
                if (cellObjects[i].Race != ObjectType.Spell) continue;
                SpellObject ob = (SpellObject)cellObjects[i];

                ob.ProcessSpell(this);
                //break;
            }

            return true;
        }
        protected virtual void Attack()
        {
            if (BindingShotCenter) ReleaseBindingShot();

            ShockTime = 0;

            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }


            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });


            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            int damage = GetAttackPower(MinDC, MaxDC);

            if (damage == 0) return;

            Target.Attacked(this, damage);
        }

        public void ReleaseBindingShot()
        {
            if (!BindingShotCenter) return;

            ShockTime = 0;
            Broadcast(GetInfo());//update clients in range (remove effect)
            BindingShotCenter = false;

            //the centertarget is escaped so make all shocked mobs awake (3x3 from center)
            Point place = CurrentLocation;
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
                        if (targetob == null || targetob.Node == null || targetob.Race != ObjectType.Monster) continue;
                        if (((MonsterObject)targetob).ShockTime == 0) continue;

                        //each centerTarget has its own effect which needs to be cleared when no longer shocked
                        if (((MonsterObject)targetob).BindingShotCenter) ((MonsterObject)targetob).ReleaseBindingShot();
                        else ((MonsterObject)targetob).ShockTime = 0;

                        break;
                    }
                }
            }
        }

        public bool FindNearby(int distance)
        {
            for (int d = 0; d <= distance; d++)
            {
                for (int y = CurrentLocation.Y - d; y <= CurrentLocation.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = CurrentLocation.X - d; x <= CurrentLocation.X + d; x += Math.Abs(y - CurrentLocation.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;
                        if (!CurrentMap.ValidPoint(x, y)) continue;
                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            switch (ob.Race)
                            {
                                case ObjectType.Monster:
                                case ObjectType.Player:
                                    if (!ob.IsAttackTarget(this)) continue;
                                    if (ob.Hidden && (!CoolEye || Level < ob.Level)) continue;
                                    if (ob.Hidden2) continue;
                                    if (ob.Race == ObjectType.Player)
                                    {
                                        PlayerObject player = ((PlayerObject)ob);
                                        if (player.GMGameMaster) continue;
                                    }
                                    return true;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }

            return false;
        }
        public bool FindFriendsNearby(int distance)
        {
            for (int d = 0; d <= distance; d++)
            {
                for (int y = CurrentLocation.Y - d; y <= CurrentLocation.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = CurrentLocation.X - d; x <= CurrentLocation.X + d; x += Math.Abs(y - CurrentLocation.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;
                        if (!CurrentMap.ValidPoint(x, y)) continue;
                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            switch (ob.Race)
                            {
                                case ObjectType.Monster:
                                case ObjectType.Player:
                                    if (ob == this || ob.Dead) continue;
                                    if (ob.IsAttackTarget(this)) continue;
                                    if (ob.Race == ObjectType.Player)
                                    {
                                        PlayerObject player = ((PlayerObject)ob);
                                        if (player.GMGameMaster) continue;
                                    }
                                    return true;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public List<MapObject> FindAllNearby(int dist, Point location, bool needSight = true)
        {
            List<MapObject> targets = new List<MapObject>();
            for (int d = 0; d <= dist; d++)
            {
                for (int y = location.Y - d; y <= location.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = location.X - d; x <= location.X + d; x += Math.Abs(y - location.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;

                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            switch (ob.Race)
                            {
                                case ObjectType.Monster:
                                case ObjectType.Player:
                                    targets.Add(ob);
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
            return targets;
        }

        protected List<MapObject> FindAllTargets(int dist, Point location, bool needSight = true)
        {
            List<MapObject> targets = new List<MapObject>();
            for (int d = 0; d <= dist; d++)
            {
                for (int y = location.Y - d; y <= location.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= CurrentMap.Height) break;

                    for (int x = location.X - d; x <= location.X + d; x += Math.Abs(y - location.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= CurrentMap.Width) break;

                        var cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null) continue;

                        for (int i = 0; i < cellObjects.Count; i++)
                        {
                            MapObject ob = cellObjects[i];
                            switch (ob.Race)
                            {
                                case ObjectType.Monster:
                                case ObjectType.Player:
                                    if (!ob.IsAttackTarget(this)) continue;
                                    if (ob.Hidden && (!CoolEye || Level < ob.Level) && needSight) continue;
                                    if (ob.Hidden2) continue;
                                    if (ob.Race == ObjectType.Player)
                                    {
                                        PlayerObject player = ((PlayerObject)ob);
                                        if (player.GMGameMaster) continue;
                                    }
                                    targets.Add(ob);
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
            return targets;
        }

        public override bool IsAttackTarget(PlayerObject attacker)
        {
            if (attacker == null || attacker.Node == null) return false;
            if (Dead) return false;
            if (Master == null) return true;
            if (attacker.AMode == AttackMode.Peace) return false;
            if (Master == attacker) return attacker.AMode == AttackMode.All;
            if (Master.Race == ObjectType.Player && (attacker.InSafeZone || InSafeZone)) return false;

            switch (attacker.AMode)
            {
                case AttackMode.Group:
                    return Master.GroupMembers == null || !Master.GroupMembers.Contains(attacker);
                case AttackMode.Guild:
                    {
                        if (!(Master is PlayerObject)) return false;
                        PlayerObject master = (PlayerObject)Master;
                        return master.MyGuild == null || master.MyGuild != attacker.MyGuild;
                    }
                case AttackMode.EnemyGuild:
                    {
                        if (!(Master is PlayerObject)) return false;
                        PlayerObject master = (PlayerObject)Master;
                        return (master.MyGuild != null && attacker.MyGuild != null) && master.MyGuild.IsEnemy(attacker.MyGuild);
                    }
                case AttackMode.RedBrown:
                    return Master.PKPoints >= 200 || Envir.Time < Master.BrownTime;
                default:
                    return true;
            }
        }
        public override bool IsAttackTarget(MonsterObject attacker)
        {
            if (attacker == null || attacker.Node == null) return false;
            if (Dead || attacker == this) return false;
            if (attacker.Race == ObjectType.Creature) return false;

            if (attacker.Info.AI == 6) // Guard
            {
                if (Info.AI != 1 && Info.AI != 2 && Info.AI != 3 && (Master == null || Master.PKPoints >= 200)) //Not Dear/Hen/Tree/Pets or Red Master 
                    return true;
            }
            else if (attacker.Info.AI == 58) // Tao Guard - attacks Pets
            {
                if (Info.AI != 1 && Info.AI != 2 && Info.AI != 3) //Not Dear/Hen/Tree
                    return true;
            }
            else if (Master != null) //Pet Attacked
            {
                if (Master == attacker)
                    return false;

                if (attacker.Master == null) //Wild Monster
                    return true;

                //Pet Vs Pet
                if (Master == attacker.Master)
                    return false;

                if (Envir.Time < ShockTime) //Shocked
                    return false;

                if (Master.Race == ObjectType.Player && attacker.Master.Race == ObjectType.Player && (Master.InSafeZone || attacker.Master.InSafeZone)) return false;
                if (Master.Race == ObjectType.Monster && attacker.Master.Race == ObjectType.Monster && (Master.InSafeZone || attacker.Master.InSafeZone)) return false;

                switch (attacker.Master.AMode)
                {
                    case AttackMode.Group:
                        if (Master.GroupMembers != null && Master.GroupMembers.Contains((PlayerObject)attacker.Master)) return false;
                        break;
                    case AttackMode.Guild:
                        break;
                    case AttackMode.EnemyGuild:
                        break;
                    case AttackMode.RedBrown:
                        if (attacker.Master.PKPoints < 200 || Envir.Time > attacker.Master.BrownTime) return false;
                        break;
                    case AttackMode.Peace:
                        return false;
                }

                for (int i = 0; i < Master.Pets.Count; i++)
                    if (Master.Pets[i].EXPOwner == attacker.Master) return true;

                for (int i = 0; i < attacker.Master.Pets.Count; i++)
                {
                    MonsterObject ob = attacker.Master.Pets[i];
                    if (ob == Target || ob.Target == this) return true;
                }

                return Master.LastHitter == attacker.Master;
            }
            else if (attacker.Master != null) //Pet Attacking Wild Monster
            {
                if (attacker.Master == this)
                    return false;

                if (Envir.Time < ShockTime) //Shocked
                    return false;

                for (int i = 0; i < attacker.Master.Pets.Count; i++)
                {
                    MonsterObject ob = attacker.Master.Pets[i];
                    if (ob == Target || ob.Target == this) return true;
                }

                if (Target == attacker.Master)
                    return true;
            }

            if (Envir.Time < attacker.HallucinationTime) return true;

            return Envir.Time < attacker.RageTime;
        }
        public override bool IsFriendlyTarget(PlayerObject ally)
        {
            if (Master == null) return false;
            if (Master == ally) return true;

            switch (ally.AMode)
            {
                case AttackMode.Group:
                    return Master.GroupMembers != null && Master.GroupMembers.Contains(ally);
                case AttackMode.Guild:
                    return false;
                case AttackMode.EnemyGuild:
                    return true;
                case AttackMode.RedBrown:
                    return Master.PKPoints < 200 & Envir.Time > Master.BrownTime;
            }
            return true;
        }

        public override bool IsFriendlyTarget(MonsterObject ally)
        {
            if (Master != null) return false;
            if (ally.Race != ObjectType.Monster) return false;
            if (ally.Master != null) return false;

            return true;
        }

        public override int Attacked(PlayerObject attacker, int damage, DefenceType type = DefenceType.ACAgility, bool damageWeapon = true, bool isReflectDamage = false)
        {
            if (Target == null && attacker.IsAttackTarget(this))
            {
                Target = attacker;
            }

            int armour = 0;
            bool critical = false;

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
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.MAC:
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

            if (MagicShield)
                damage -= damage * (MagicShieldLv + 2) / 10;

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }

            if (MagicShield)
            {
                MagicShieldTime -= (damage - armour) * 60;
                AddBuff(new Buff { Type = BuffType.MagicShield, Caster = this, ExpireTime = MagicShieldTime, Values = new int[] { MagicShieldLv } });
            }

            if ((attacker.CriticalRate * Settings.CriticalRateWeight) > Envir.Random.Next(100))
            {
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Critical });
                damage = Math.Min(int.MaxValue, damage + (int)Math.Floor(damage * ((double)attacker.CriticalDamage / Settings.CriticalDamageWeight)));
                critical = true;
            }

            if (attacker.LifeOnHit > 0)
                attacker.ChangeHP(attacker.LifeOnHit);

            if (Target != this && attacker.IsAttackTarget(this))
            {
                if (attacker.Info.MentalState == 2)
                {
                    if (Functions.MaxDistance(CurrentLocation, attacker.CurrentLocation) < (8 - attacker.Info.MentalStateLvl))
                        Target = attacker;
                }
                else
                    Target = attacker;
            }

            if (BindingShotCenter) ReleaseBindingShot();
            ShockTime = 0;

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (PoisonList[i].PType != PoisonType.LRParalysis) continue;

                PoisonList.RemoveAt(i);
                OperateTime = 0;
            }

            if (Master != null && Master != attacker)
                if (Envir.Time > Master.BrownTime && Master.PKPoints < 200)
                    attacker.BrownTime = Envir.Time + Settings.Minute;

            if (EXPOwner == null || EXPOwner.Dead)
                EXPOwner = attacker;

            if (EXPOwner == attacker)
                EXPOwnerTime = Envir.Time + EXPOwnerDelay;

            ushort LevelOffset = (ushort)(Level > attacker.Level ? 0 : Math.Min(10, attacker.Level - Level));

            if (attacker.HasParalysisRing && type != DefenceType.MAC && type != DefenceType.MACAgility && 1 == Envir.Random.Next(1, 15))
            {
                ApplyPoison(new Poison { PType = PoisonType.Paralysis, Duration = 5, TickSpeed = 1000 }, attacker);
            }

            if (attacker.Freezing > 0 && type != DefenceType.MAC && type != DefenceType.MACAgility)
            {
                if ((Envir.Random.Next(Settings.FreezingAttackWeight) < attacker.Freezing) && (Envir.Random.Next(LevelOffset) == 0))
                    ApplyPoison(new Poison { PType = PoisonType.Slow, Duration = Math.Min(10, (3 + Envir.Random.Next(attacker.Freezing))), TickSpeed = 1000 }, attacker);
            }

            if (attacker.PoisonAttack > 0 && type != DefenceType.MAC && type != DefenceType.MACAgility)
            {
                if ((Envir.Random.Next(Settings.PoisonAttackWeight) < attacker.PoisonAttack) && (Envir.Random.Next(LevelOffset) == 0))
                    ApplyPoison(new Poison { PType = PoisonType.Green, Duration = 5, TickSpeed = 1000, Value = Math.Min(10, 3 + Envir.Random.Next(attacker.PoisonAttack)) }, attacker);
            }

            Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = attacker.ObjectID, Direction = Direction, Location = CurrentLocation });

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

            attacker.GatherElement();

            if (attacker.Info.Mentor != 0 && attacker.Info.isMentor)
            {
                Buff buff = attacker.Buffs.Where(e => e.Type == BuffType.Mentor).FirstOrDefault();
                if (buff != null)
                {
                    CharacterInfo Mentee = Envir.GetCharacterInfo(attacker.Info.Mentor);
                    PlayerObject player = Envir.GetPlayer(Mentee.Name);
                    if (player.CurrentMap == attacker.CurrentMap && Functions.InRange(player.CurrentLocation, attacker.CurrentLocation, Globals.DataRange) && !player.Dead)
                    {
                        damage += ((damage / 100) * Settings.MentorDamageBoost);
                    }
                }
            }

            if (Master != null && Master != attacker && Master.Race == ObjectType.Player && Envir.Time > Master.BrownTime && Master.PKPoints < 200 && !((PlayerObject)Master).AtWar(attacker))
            {
                attacker.BrownTime = Envir.Time + Settings.Minute;
            }

            for (int i = 0; i < attacker.Pets.Count; i++)
            {
                MonsterObject ob = attacker.Pets[i];

                if (IsAttackTarget(ob) && (ob.Target == null)) ob.Target = this;
            }

            if (critical)
                BroadcastDamageIndicator(DamageType.Critical, armour - damage);
            else
                BroadcastDamageIndicator(DamageType.Hit, armour - damage);

            ChangeHP(armour - damage);
            return damage - armour;
        }

        public override int Attacked(MonsterObject attacker, int damage, DefenceType type = DefenceType.ACAgility)
        {
            if (Target == null && attacker.IsAttackTarget(this))
                Target = attacker;

            int armour = 0;

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
                    if (Envir.Random.Next(Agility + 1) > attacker.Accuracy)
                    {
                        BroadcastDamageIndicator(DamageType.Miss);
                        return 0;
                    }
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.MAC:
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

            armour = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(armour * ArmourRate))));
            damage = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(damage * DamageRate))));

            if (armour >= damage)
            {
                BroadcastDamageIndicator(DamageType.Invalid);
                return 0;
            }

            if (Target != this && attacker.IsAttackTarget(this))
                Target = attacker;

            if (BindingShotCenter) ReleaseBindingShot();
            ShockTime = 0;

            for (int i = PoisonList.Count - 1; i >= 0; i--)
            {
                if (PoisonList[i].PType != PoisonType.LRParalysis) continue;

                PoisonList.RemoveAt(i);
                OperateTime = 0;
            }

            if (attacker.Info.AI == 6 || attacker.Info.AI == 58)
                EXPOwner = null;

            else if (attacker.Master != null)
            {
                if (attacker.CurrentMap != attacker.Master.CurrentMap || !Functions.InRange(attacker.CurrentLocation, attacker.Master.CurrentLocation, Globals.DataRange))
                    EXPOwner = null;
                else
                {

                    if (EXPOwner == null || EXPOwner.Dead)
                        EXPOwner = attacker.Master;

                    if (EXPOwner == attacker.Master)
                        EXPOwnerTime = Envir.Time + EXPOwnerDelay;
                }

            }

            Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = attacker.ObjectID, Direction = Direction, Location = CurrentLocation });

            BroadcastDamageIndicator(DamageType.Hit, armour - damage);

            ChangeHP(armour - damage);
            return damage - armour;
        }

        public override int Struck(int damage, DefenceType type = DefenceType.ACAgility)
        {
            int armour = 0;

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
                    armour = GetDefencePower(MinMAC, MaxMAC);
                    break;
                case DefenceType.Agility:
                    break;
            }

            armour = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(armour * ArmourRate))));
            damage = (int)Math.Max(int.MinValue, (Math.Min(int.MaxValue, (decimal)(damage * DamageRate))));

            if (armour >= damage) return 0;
            Broadcast(new S.ObjectStruck { ObjectID = ObjectID, AttackerID = 0, Direction = Direction, Location = CurrentLocation });

            if (Settings.AllowStruck)
                StruckTime = Envir.Time + Settings.StruckDelay;
            ChangeHP(armour - damage);
            return damage - armour;
        }

        public override void ApplyPoison(Poison p, MapObject Caster = null, bool NoResist = false, bool ignoreDefence = true)
        {
            if (p.Owner != null && p.Owner.IsAttackTarget(this))
                Target = p.Owner;

            if (Master != null && p.Owner != null && p.Owner.Race == ObjectType.Player && p.Owner != Master)
            {
                if (Envir.Time > Master.BrownTime && Master.PKPoints < 200)
                    p.Owner.BrownTime = Envir.Time + Settings.Minute;
            }

            if (!ignoreDefence && (p.PType == PoisonType.Green))
            {
                int armour = GetDefencePower(MinMAC, MaxMAC);

                if (p.Value < armour)
                    p.PType = PoisonType.None;
                else
                    p.Value -= armour;
            }

            if (p.PType == PoisonType.None) return;

            for (int i = 0; i < PoisonList.Count; i++)
            {
                if (PoisonList[i].PType != p.PType) continue;
                if ((PoisonList[i].PType == PoisonType.Green) && (PoisonList[i].Value > p.Value)) return;//cant cast weak poison to cancel out strong poison
                if ((PoisonList[i].PType != PoisonType.Green) && ((PoisonList[i].Duration - PoisonList[i].Time) > p.Duration)) return;//cant cast 1 second poison to make a 1minute poison go away!
                if (p.PType == PoisonType.DelayedExplosion) return;
                if ((PoisonList[i].PType == PoisonType.Frozen) || (PoisonList[i].PType == PoisonType.Slow) || (PoisonList[i].PType == PoisonType.Paralysis) || (PoisonList[i].PType == PoisonType.LRParalysis)) return;//prevents mobs from being perma frozen/slowed
                PoisonList[i] = p;
                return;
            }

            if (p.PType == PoisonType.DelayedExplosion)
            {
                ExplosionInflictedTime = Envir.Time + 1500;
                Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.DelayedExplosion });
            }

            PoisonList.Add(p);
        }
        public override void AddBuff(Buff b)
        {
            if (Buffs.Any(d => d.Infinite && d.Type == b.Type)) return; //cant overwrite infinite buff with regular buff

            string caster = b.Caster != null ? b.Caster.Name : string.Empty;

            if (b.Values == null) b.Values = new int[1];

            S.AddBuff addBuff = new S.AddBuff { Type = b.Type, Caster = caster, Expire = b.ExpireTime - Envir.Time, Values = b.Values, Infinite = b.Infinite, ObjectID = ObjectID, Visible = b.Visible };

            if (b.Visible) Broadcast(addBuff);

            base.AddBuff(b);
            RefreshAll();
        }

        public override Packet GetInfo()
        {
            return new S.ObjectMonster
            {
                ObjectID = ObjectID,
                Name = Name,
                NameColour = NameColour,
                Location = CurrentLocation,
                Image = Info.Image,
                Direction = Direction,
                Effect = Info.Effect,
                AI = Info.AI,
                Light = Info.Light,
                Dead = Dead,
                Skeleton = Harvested,
                Poison = CurrentPoison,
                Hidden = Hidden,
                ShockTime = (ShockTime > 0 ? ShockTime - Envir.Time : 0),
                BindingShotCenter = BindingShotCenter
            };
        }

        public override void ReceiveChat(string text, ChatType type)
        {
            throw new NotSupportedException();
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                            if (x < 0 || x >= CurrentMap.Width) continue;

                            var cellObjects = CurrentMap.GetCellObjects(x, y);

                            if (cellObjects == null) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                MapObject ob = cellObjects[i];
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
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
                                if (ob.Race != ObjectType.Player) continue;
                                ob.Add(this);
                            }
                        }
                    }
                    break;
            }
        }

        public override void Add(PlayerObject player)
        {
            player.Enqueue(GetInfo());
            SendHealth(player);
        }

        public override void SendHealth(PlayerObject player)
        {
            if (!player.IsMember(Master) && !(player.IsMember(EXPOwner) && AutoRev) && Envir.Time > RevTime) return;
            byte time = Math.Min(byte.MaxValue, (byte)Math.Max(5, (RevTime - Envir.Time) / 1000));
            player.Enqueue(new S.ObjectHealth { ObjectID = ObjectID, Percent = PercentHealth, Expire = time, Health = Health, MaxHealth = MaxHealth });
        }

        public void PetExp(uint amount)
        {
            if (PetLevel >= MaxPetLevel) return;

            if (Info.Name == Settings.SkeletonName || Info.Name == Settings.ShinsuName || Info.Name == Settings.AngelName)
                amount *= 3;

            PetExperience += amount;

            if (PetExperience < (PetLevel + 1) * 20000) return;

            PetExperience = (uint)(PetExperience - ((PetLevel + 1) * 20000));
            PetLevel++;
            RefreshAll();
            OperateTime = 0;
            BroadcastHealthChange();
        }
        public override void Despawn()
        {
            SlaveList.Clear();
            base.Despawn();
        }

        protected void LineAttack(int distance, int damage, DefenceType defenceType = DefenceType.ACAgility, int speed = 0)
        {
            for (int i = 1; i <= distance; i++)
            {
                Point target = Functions.PointMove(CurrentLocation, Direction, i);

                int delay = speed * i;
                if (!CurrentMap.ValidPoint(target)) continue;

                var cellObjects = CurrentMap.GetCellObjects(target);
                if (cellObjects == null) continue;

                for (int o = 0; o < cellObjects.Count; o++)
                {
                    MapObject ob = cellObjects[o];
                    if (ob.Race == ObjectType.Monster || ob.Race == ObjectType.Player)
                    {
                        if (!ob.IsAttackTarget(this)) continue;

                        if (delay == 0)
                        {
                            ob.Attacked(this, damage, defenceType);
                        }
                        else
                        {
                            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, ob, damage, defenceType);
                            ActionList.Add(action);
                        }
                    }
                }
            }
        }

        public void MagicAreaAttack(int area, int damage, Point location)
        {
            List<MapObject> targets = FindAllTargets(area, location, false);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                targets[i].Attacked(this, damage, DefenceType.MAC);
            }
        }


        protected void AoeAttack(int area, int damage, int time, Point location)
        {
            List<MapObject> targets = FindAllTargets(area, location, false);

            if (targets.Count != 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + time, targets[i], damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
            }
        }

        public MonsterObject RegenMonsterByName1(string name, int x, int y)
        {
            MonsterObject mob = GetMonster(Envir.GetMonsterInfo(name));
            if (mob == null)
                return null;

            mob.Spawn(CurrentMap, new Point(x, y));
            mob.Owner = this;
            SlaveList.Add(mob);

            return mob;
        }

        public void MoveForward(int distance, int delay)
        {
            // telpo location
            Point location = Functions.PointMove(CurrentLocation, Direction, distance);

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

            if (delay > 0)
            {
                DelayedAction action = new DelayedAction(DelayedType.Teleport, Envir.Time + delay, CurrentMap, location, false);
                ActionList.Add(action);
            }
            else
            {
                Teleport(CurrentMap, location, true);
            }
        }

        protected virtual void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 400, Target, damage, DefenceType.ACAgility, 0);
            ActionList.Add(action);
        }

        protected virtual void RangeAttack()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 0);
            ActionList.Add(action);
        }

        protected virtual void RangeAttack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 1);
            ActionList.Add(action);
        }

        protected virtual void RangeAttack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 2 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 2);
            ActionList.Add(action);
        }

        protected virtual void RangeAttack4()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 3 });
            DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 800, Target, damage, DefenceType.MACAgility, 3);
            ActionList.Add(action);
        }

        protected void HalfMoonAttack(int damage, DefenceType defence)
        {
            MirDirection dir = Functions.PreviousDir(Direction);
            for (int i = 0; i < 3; i++)
            {
                Point location = Functions.PointMove(CurrentLocation, dir, 1);
                if (!CurrentMap.ValidPoint(location))
                    continue;
                var cellObjects = CurrentMap.GetCellObjects(location);
                if ( cellObjects != null)
                {
                    for (int x = 0; x < cellObjects.Count; x++)
                    {
                        if (cellObjects[x].Race == ObjectType.Player || cellObjects[x].Race == ObjectType.Monster)
                        {
                            if (cellObjects[x].IsAttackTarget(this))
                            {
                                cellObjects[x].Attacked(this, damage, defence);
                            }
                        }
                    }
                }
                dir = Functions.NextDir(dir);
            }
        }

        protected void IceCone(int nearDamage, int farDamage)
        {
            Point location = CurrentLocation;
            MirDirection direction = Direction;

            int col = 3;
            int row = 3;

            Point[] loc = new Point[col]; //0 = left 1 = center 2 = right
            loc[0] = Functions.PointMove(location, Functions.PreviousDir(direction), 1);
            loc[1] = Functions.PointMove(location, direction, 1);
            loc[2] = Functions.PointMove(location, Functions.NextDir(direction), 1);

            for (int i = 0; i < col; i++)
            {
                Point startPoint = loc[i];
                for (int j = 0; j < row; j++)
                {
                    Point hitPoint = Functions.PointMove(startPoint, direction, j);

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
                                if (target.IsAttackTarget(this))
                                    target.Attacked(this, j <= 1 ? nearDamage : farDamage, DefenceType.MAC, false);
                                break;
                        }
                    }
                }
            }
        }
    }
}
