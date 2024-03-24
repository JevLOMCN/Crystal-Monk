using System.IO;
using System;
using Client.MirSounds;
using System.Windows.Forms;
using System.Reflection;

namespace Client
{
    class Settings
    {
        public const long CleanDelay = 600000;
        public static int ScreenWidth = 800, ScreenHeight = 600;
        private static InIReader Reader = new InIReader(@".\Mir2Test.ini");

        private static bool _useTestConfig;
        public static bool UseTestConfig
        {
            get
            {
                return _useTestConfig;
            }
            set 
            {
                if (value == true)
                {
                    Reader = new InIReader(@".\Mir2Test.ini");
                }
                _useTestConfig = value;
            }
        }

        public static InIReader AssistReader;

        public const string DataPath = @".\Data\",
                            MapPath = @".\Map\",
                            SoundPath = @".\Sound\",
                            ExtraDataPath = @".\Data\Extra\",
                            ShadersPath = @".\Data\Shaders\",
                            MonsterPath = @".\Data\Monster\",
                            GatePath = @".\Data\Gate\",
                            FlagPath = @".\Data\Flag\",
                            NPCPath = @".\Data\NPC\",
                            CArmourPath = @".\Data\CArmour\",
                            CWeaponPath = @".\Data\CWeapon\",
                            CWeaponEffectPath = @".\Data\CWeaponEffect\",
                            CHairPath = @".\Data\CHair\",
                            AArmourPath = @".\Data\AArmour\",
                            AWeaponPath = @".\Data\AWeapon\",
                            AHairPath = @".\Data\AHair\",
                            ARArmourPath = @".\Data\ARArmour\",
                            ARWeaponPath = @".\Data\ARWeapon\",
                            ARHairPath = @".\Data\ARHair\",
                            CHumEffectPath = @".\Data\CHumEffect\",
                            AHumEffectPath = @".\Data\AHumEffect\",
                            ARHumEffectPath = @".\Data\ARHumEffect\",
                            MountPath = @".\Data\Mount\",
                            FishingPath = @".\Data\Fishing\",
                            PetsPath = @".\Data\Pet\",
                            TransformPath = @".\Data\Transform\",
                            TransformMountsPath = @".\Data\TransformRide2\",
                            TransformEffectPath = @".\Data\TransformEffect\",
                            TransformWeaponEffectPath = @".\Data\TransformWeaponEffect\",

                            MonkArmourPath = @".\Data\MonkArmour\",
                            MonkWeaponPath = @".\Data\MonkWeapon\",
                            MonkHairPath = @".\Data\MonkHair\",
                            MonkHumEffectPath = @".\Data\MonkHumEffect\",
                            MonkWeaponEffectPath = @".\Data\MonkWeaponEffect\",

                            //stupple HUMUP stared
                            UpCArmourPath = @".\Data\HumUp\UpCArmour\",
                            UpCWeaponPath = @".\Data\HumUp\UpCWeapon\",
                            UpCHairPath = @".\Data\HumUp\UpCHair\",
                            UpCHumEffectPath = @".\Data\HumUp\UpCHumEffect\",

                            UpWarArmourPath = @".\Data\HumUp\UpWarArmour\",
                            UpWarWeaponPath = @".\Data\HumUp\UpWarWeapon\",
                            UpWarHairPath = @".\Data\HumUp\UpWarHair\",
                            UpWarHumEffectPath = @".\Data\HumUp\UpWarHumEffect\",

                            UpWizArmourPath = @".\Data\HumUp\UpWizArmour\",
                            UpWizWeaponPath = @".\Data\HumUp\UpWizWeapon\",
                            UpWizHairPath = @".\Data\HumUp\UpWizHair\",
                            UpWizHumEffectPath = @".\Data\HumUp\UpWizHumEffect\",

                            UpTaoArmourPath = @".\Data\HumUp\UpTaoArmour\",
                            UpTaoWeaponPath = @".\Data\HumUp\UpTaoWeapon\",
                            UpTaoHairPath = @".\Data\HumUp\UpTaoHair\",
                            UpTaoHumEffectPath = @".\Data\HumUp\UpTaoHumEffect\",

                            UpAssArmourPath = @".\Data\HumUp\UpAssArmour\",
                            UpAssWeaponRPath = @".\Data\HumUp\UpAssWeaponR\",
                            UpAssWeaponLPath = @".\Data\HumUp\UpAssWeaponL\",
                            UpAssHairPath = @".\Data\HumUp\UpAssHair\",
                            UpAssHumEffectPath = @".\Data\HumUp\UpAssHumEffect\",

                            UpArcArmourPath = @".\Data\HumUp\UpArcArmour\",
                            UpArcWeaponPath = @".\Data\HumUp\UpArcWeapon\",
                            UpArcWeaponSPath = @".\Data\HumUp\UpArcWeaponS\",
                            UpArcHairPath = @".\Data\HumUp\UpArcHair\",
                            UpArcHumEffectPath = @".\Data\HumUp\UpArcHumEffect\",


                            UpMountPath = @".\Data\HumUp\UpMount\",
                            UpFishingPath = @".\Data\HumUp\UpFishing\";
                            //STupple Humup end

        //Logs
        public static bool LogErrors = true;
        public static bool LogChat = true;
        public static int RemainingErrorLogs = 100;

        //Graphics
        public static bool FullScreen = false, TopMost = true;
        public static string FontName = "Tahoma"; //"MS Sans Serif"
        public static bool FPSCap = true;
        public static int MaxFPS = 100;
        public static int Resolution = 1024;
        public static bool DebugMode = false;

        //Network
        public static bool UseConfig = false;
        public static string IPAddress = "127.0.01";
        public static int Port = 7000;
        public const int TimeOut = 5000;

        //Sound
        public static int SoundOverLap = 3;
        private static byte _volume = 100;
        public static byte Volume
        {
            get { return _volume; }
            set
            {
                if (_volume == value) return;

                _volume = (byte) (value > 100 ? 100 : value);

                if (_volume == 0)
                    SoundManager.Vol = -10000;
                else 
                    SoundManager.Vol = (int)(-3000 + (3000 * (_volume / 100M)));
            }
        }

        private static byte _musicVolume = 100;
        public static byte MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                if (_musicVolume == value) return;

                _musicVolume = (byte)(value > 100 ? 100 : value);

                if (_musicVolume == 0)
                    SoundManager.MusicVol = -10000;
                else
                    SoundManager.MusicVol = (int)(-3000 + (3000 * (_musicVolume / 100M)));
            }
        }

        //Game
        public static string AccountID = "",
                             Password = "";

        public static bool
            SkillMode = false,
            SkillBar = true,
            //SkillSet = true,
            Effect = true,
            LevelEffect = true,
            DropView = true,
            NameView = true,
            HPView = true,
            TransparentChat = false,
            DuraView = false,
            TargetDead = false,
            ExpandedBuffWindow = true;

        public static int[,] SkillbarLocation = new int[2, 2] { { 0, 0 }, { 216, 0 }  };

        //Quests
        public static int[] TrackedQuests = new int[5];

        //Chat
        public static bool
            ShowNormalChat = true,
            ShowYellChat = true,
            ShowWhisperChat = true,
            ShowLoverChat = true,
            ShowMentorChat = true,
            ShowGroupChat = true,
            ShowGuildChat = true;

        //Filters
        public static bool
            FilterNormalChat = false,
            FilterWhisperChat = false,
            FilterShoutChat = false,
            FilterSystemChat = false,
            FilterLoverChat = false,
            FilterMentorChat = false,
            FilterGroupChat = false,
            FilterGuildChat = false;


        //AutoPatcher
        public static bool P_Patcher = true;
        public static string P_Host = @"http://" + IPAddress + "/patch/";
        public static string P_PatchFileName = @"PList.gz";
        public static bool P_NeedLogin = false;
        public static string P_Login = string.Empty;
        public static string P_Password = string.Empty;
        public static string P_ServerName = string.Empty;
        public static string P_Client = Application.StartupPath + "\\";
        public static bool P_AutoStart = false;

        // assist
        [InI("Assist")]
        public static bool FreeShift = true;

        [InI("Assist")]
        public static bool StruckShield = false;

        [InI("Assist")]
        public static bool SmartCrsHit = false;

        [InI("Assist")]
        public static bool SmartFireHit = true;

        [InI("Assist")]
        public static bool SmartSheild = false;

        [InI("Assist")]
        public static bool SmartChangeSign = true;

        [InI("Assist")]
        public static bool SmartChangePoison = true;

        [InI("Assist")]
        public static bool SmartDaMo = true;

        [InI("Assist")]
        public static bool SmartElementalBarrier = true;

        [InI("Assist")]
        public static bool SmartElementalBarrier1 = true;

        [InI("Assist")]
        public static bool SmartProtect = true;

        [InI("Assist")]
        public static bool AutoPick = true;

        [InI("Assist")]
        public static bool SpaceThrusting = false;

        [InI("Assist")]
        public static bool ShowLevel = false;

        [InI("Assist")]
        public static bool ShowTransform = true;

        [InI("Assist")]
        public static bool ShowGuildName = true;

        [InI("Assist")]
        public static int NumBackCityHP = 50;

        [InI("Assist")]
        public static int UseItemInterval = 3000;

        [InI("Assist")]
        public static bool ShowPing = true;

        [InI("Assist")]
        public static bool ShowHealth = true;

        [InI("Assist")]
        public static string PercentItem0 = "金创药";

        [InI("Assist")]
        public static string PercentItem1 = "魔法药";

        [InI("Assist")]
        public static string PercentItem2 = "回城";

        [InI("Assist")]
        public static int ProtectPercent0 = 50;

        [InI("Assist")]
        public static int ProtectPercent1 = 50;

        [InI("Assist")]
        public static int ProtectPercent2 = 5;

        [InI("Assist")]
        public static bool ShowGroupInfo = true;

        [InI("Assist")]
        public static bool ShowHeal = true;

        [InI("Assist")]
        public static bool ShowDamage = true;

        [InI("Assist")]
        public static bool ShowMonsterName = true;

        [InI("Assist")]
        public static bool HideDead = false;

        [InI("Assist")]
        public static bool HideSystem2 = false;

        [InI("Debug", false)]
        public static bool DrawFloor = true;

        [InI("Debug", false)]
        public static bool DrawBack = true;

        [InI("Debug", false)]
        public static bool DrawMiddle = true;

        [InI("Debug", false)]
        public static bool DrawFront = true;

        [InI("Debug", false)]
        public static bool DrawShandaAnim = true;

        [InI("Debug", false)]
        public static bool DrawMir3Middle = true;

        [InI("Debug", false)]
        public static bool DrawFrontLayer = true;

        [InI("Debug", false)]
        public static bool BorderView = true;

        [InI("BackDownload")]
        public static bool BackDownload = true;
    
     

        public static bool CheckFileName = false;

        public static void Load()
        {
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(MapPath)) Directory.CreateDirectory(MapPath);
            if (!Directory.Exists(SoundPath)) Directory.CreateDirectory(SoundPath);

            //Graphics
            FullScreen = Reader.ReadBoolean("Graphics", "FullScreen", FullScreen);
            TopMost = Reader.ReadBoolean("Graphics", "AlwaysOnTop", TopMost);
            FPSCap = Reader.ReadBoolean("Graphics", "FPSCap", FPSCap);
            Resolution = Reader.ReadInt32("Graphics", "Resolution", Resolution);
            DebugMode = Reader.ReadBoolean("Graphics", "DebugMode", DebugMode);

            //Network
            UseConfig = Reader.ReadBoolean("Network", "UseConfig", UseConfig);
            if (UseConfig)
            {
                IPAddress = Reader.ReadString("Network", "IPAddress", IPAddress);
                Port = Reader.ReadInt32("Network", "Port", Port);
            }

            //Logs
            LogErrors = Reader.ReadBoolean("Logs", "LogErrors", LogErrors);
            LogChat = Reader.ReadBoolean("Logs", "LogChat", LogChat);

            //Sound
            Volume = Reader.ReadByte("Sound", "Volume", Volume);
            SoundOverLap = Reader.ReadInt32("Sound", "SoundOverLap", SoundOverLap);
            MusicVolume = Reader.ReadByte("Sound", "Music", MusicVolume);

            //Game
            AccountID = Reader.ReadString("Game", "AccountID", AccountID);
            Password = Reader.ReadString("Game", "Password", Password);

            SkillMode = Reader.ReadBoolean("Game", "SkillMode", SkillMode);
            SkillBar = Reader.ReadBoolean("Game", "SkillBar", SkillBar);
            //SkillSet = Reader.ReadBoolean("Game", "SkillSet", SkillSet);
            Effect = Reader.ReadBoolean("Game", "Effect", Effect);
            LevelEffect = Reader.ReadBoolean("Game", "LevelEffect", Effect);
            DropView = Reader.ReadBoolean("Game", "DropView", DropView);
            NameView = Reader.ReadBoolean("Game", "NameView", NameView);
            HPView = Reader.ReadBoolean("Game", "HPMPView", HPView);
            FontName = Reader.ReadString("Game", "FontName", FontName);
            TransparentChat = Reader.ReadBoolean("Game", "TransparentChat", TransparentChat);
            TargetDead = Reader.ReadBoolean("Game", "TargetDead", TargetDead);
            ExpandedBuffWindow = Reader.ReadBoolean("Game", "ExpandedBuffWindow", ExpandedBuffWindow);
            DuraView = Reader.ReadBoolean("Game", "DuraWindow", DuraView);

            for (int i = 0; i < SkillbarLocation.Length / 2; i++)
            {
                SkillbarLocation[i, 0] = Reader.ReadInt32("Game", "Skillbar" + i.ToString() + "X", SkillbarLocation[i, 0]);
                SkillbarLocation[i, 1] = Reader.ReadInt32("Game", "Skillbar" + i.ToString() + "Y", SkillbarLocation[i, 1]);
            }

            //Chat
            ShowNormalChat = Reader.ReadBoolean("Chat", "ShowNormalChat", ShowNormalChat);
            ShowYellChat = Reader.ReadBoolean("Chat", "ShowYellChat", ShowYellChat);
            ShowWhisperChat = Reader.ReadBoolean("Chat", "ShowWhisperChat", ShowWhisperChat);
            ShowLoverChat = Reader.ReadBoolean("Chat", "ShowLoverChat", ShowLoverChat);
            ShowMentorChat = Reader.ReadBoolean("Chat", "ShowMentorChat", ShowMentorChat);
            ShowGroupChat = Reader.ReadBoolean("Chat", "ShowGroupChat", ShowGroupChat);
            ShowGuildChat = Reader.ReadBoolean("Chat", "ShowGuildChat", ShowGuildChat);

            //Filters
            FilterNormalChat = Reader.ReadBoolean("Filter", "FilterNormalChat", FilterNormalChat);
            FilterWhisperChat = Reader.ReadBoolean("Filter", "FilterWhisperChat", FilterWhisperChat);
            FilterShoutChat = Reader.ReadBoolean("Filter", "FilterShoutChat", FilterShoutChat);
            FilterSystemChat = Reader.ReadBoolean("Filter", "FilterSystemChat", FilterSystemChat);
            FilterLoverChat = Reader.ReadBoolean("Filter", "FilterLoverChat", FilterLoverChat);
            FilterMentorChat = Reader.ReadBoolean("Filter", "FilterMentorChat", FilterMentorChat);
            FilterGroupChat = Reader.ReadBoolean("Filter", "FilterGroupChat", FilterGroupChat);
            FilterGuildChat = Reader.ReadBoolean("Filter", "FilterGuildChat", FilterGuildChat);

            //AutoPatcher
            P_Patcher = Reader.ReadBoolean("Launcher", "Enabled", P_Patcher);

#if DEBUG
            P_Host = Reader.ReadString("Launcher", "Host", P_Host);
#endif
            P_PatchFileName = Reader.ReadString("Launcher", "PatchFile", P_PatchFileName);
            P_NeedLogin = Reader.ReadBoolean("Launcher", "NeedLogin", P_NeedLogin);
            P_Login = Reader.ReadString("Launcher", "Login", P_Login);
            P_Password = Reader.ReadString("Launcher", "Password", P_Password);
            P_AutoStart = Reader.ReadBoolean("Launcher", "AutoStart", P_AutoStart);
            P_ServerName = Reader.ReadString("Launcher", "ServerName", P_ServerName);

            if (!P_Host.EndsWith("/")) P_Host += "/";
            if (P_Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) P_Host = P_Host.Insert(0, "http://");
        }

        public static void Save()
        {
            //Graphics
            Reader.Write("Graphics", "FullScreen", FullScreen);
            Reader.Write("Graphics", "AlwaysOnTop", TopMost);
            Reader.Write("Graphics", "FPSCap", FPSCap);
            Reader.Write("Graphics", "Resolution", Resolution);
            Reader.Write("Graphics", "DebugMode", DebugMode);

            //Sound
            Reader.Write("Sound", "Volume", Volume);
            Reader.Write("Sound", "Music", MusicVolume);

            //Game
            Reader.Write("Game", "AccountID", AccountID);
            Reader.Write("Game", "Password", Password);
            Reader.Write("Game", "SkillMode", SkillMode);
            Reader.Write("Game", "SkillBar", SkillBar);
            //Reader.Write("Game", "SkillSet", SkillSet);
            Reader.Write("Game", "Effect", Effect);
            Reader.Write("Game", "LevelEffect", LevelEffect);
            Reader.Write("Game", "DropView", DropView);
            Reader.Write("Game", "NameView", NameView);
            Reader.Write("Game", "HPMPView", HPView);
            Reader.Write("Game", "FontName", FontName);
            Reader.Write("Game", "TransparentChat", TransparentChat);
            Reader.Write("Game", "TargetDead", TargetDead);
            Reader.Write("Game", "ExpandedBuffWindow", ExpandedBuffWindow);
            Reader.Write("Game", "DuraWindow", DuraView);

            for (int i = 0; i < SkillbarLocation.Length / 2; i++)
            {
                Reader.Write("Game", "Skillbar" + i.ToString() + "X", SkillbarLocation[i, 0]);
                Reader.Write("Game", "Skillbar" + i.ToString() + "Y", SkillbarLocation[i, 1]);
            }

            //Chat
            Reader.Write("Chat", "ShowNormalChat", ShowNormalChat);
            Reader.Write("Chat", "ShowYellChat", ShowYellChat);
            Reader.Write("Chat", "ShowWhisperChat", ShowWhisperChat);
            Reader.Write("Chat", "ShowLoverChat", ShowLoverChat);
            Reader.Write("Chat", "ShowMentorChat", ShowMentorChat);
            Reader.Write("Chat", "ShowGroupChat", ShowGroupChat);
            Reader.Write("Chat", "ShowGuildChat", ShowGuildChat);

            //Filters
            Reader.Write("Filter", "FilterNormalChat", FilterNormalChat);
            Reader.Write("Filter", "FilterWhisperChat", FilterWhisperChat);
            Reader.Write("Filter", "FilterShoutChat", FilterShoutChat);
            Reader.Write("Filter", "FilterSystemChat", FilterSystemChat);
            Reader.Write("Filter", "FilterLoverChat", FilterLoverChat);
            Reader.Write("Filter", "FilterMentorChat", FilterMentorChat);
            Reader.Write("Filter", "FilterGroupChat", FilterGroupChat);
            Reader.Write("Filter", "FilterGuildChat", FilterGuildChat);

            //AutoPatcher
            Reader.Write("Launcher", "Enabled", P_Patcher);
#if DEBUG
            Reader.Write("Launcher", "Host", P_Host);
#endif
            Reader.Write("Launcher", "PatchFile", P_PatchFileName);
            Reader.Write("Launcher", "NeedLogin", P_NeedLogin);
            Reader.Write("Launcher", "Login", P_Login);
            Reader.Write("Launcher", "Password", P_Password);
            Reader.Write("Launcher", "ServerName", P_ServerName);
            Reader.Write("Launcher", "AutoStart", P_AutoStart);
            Reader.Save();

            if (AssistReader != null)
            {
                InIAttribute.WriteInI<Settings>(AssistReader);
                AssistReader.Save();
            }
        }

        public static void LoadTrackedQuests(string Charname)
        {
            //Quests
            for (int i = 0; i < TrackedQuests.Length; i++)
            {
                TrackedQuests[i] = Reader.ReadInt32("Q-" + Charname, "Quest-" + i.ToString(), -1);
            }
        }

        public static void SaveTrackedQuests(string Charname)
        {
            //Quests
            for (int i = 0; i < TrackedQuests.Length; i++)
            {
                Reader.Write("Q-" + Charname, "Quest-" + i.ToString(), TrackedQuests[i]);
            }
        }
    }
}
