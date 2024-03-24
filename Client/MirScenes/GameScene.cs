using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirSounds;
using Microsoft.DirectX.Direct3D;
using Font = System.Drawing.Font;
using S = ServerPackets;
using C = ClientPackets;
using Effect = Client.MirObjects.Effect;

using Client.MirScenes.Dialogs;
using System.Drawing.Imaging;
using Client.MirMagic;
using System.Diagnostics;
using Client.MirObjects.Monsters;

namespace Client.MirScenes
{
    public sealed class GameScene : MirScene
    {
        public static GameScene Scene;

        public static UserObject User
        {
            get { return MapObject.User; }
            set { MapObject.User = value; }
        }

        public static long MoveTime, AttackTime, NextRunTime, LogTime, LastRunTime;
        public static bool CanMove, CanRun;

        public MapControl MapControl;
        public MainDialog MainDialog;
        public ChatDialog ChatDialog;
        public ChatControlBar ChatControl;
        public InventoryDialog InventoryDialog;
        public CharacterDialog CharacterDialog;
        public CraftDialog CraftDialog;
        public StorageDialog StorageDialog;
        public BeltDialog BeltDialog;
        public MiniMapDialog MiniMapDialog;
        public InspectDialog InspectDialog;
        public OptionDialog OptionDialog;
        public MenuDialog MenuDialog;
        public NPCDialog NPCDialog;
        public NPCGoodsDialog NPCGoodsDialog;
        public NPCDropDialog NPCDropDialog;
        public NPCAwakeDialog NPCAwakeDialog;
        public HelpDialog HelpDialog;
        public MountDialog MountDialog;
        public FishingDialog FishingDialog;
        public FishingStatusDialog FishingStatusDialog;
        public RefineDialog RefineDialog;
        public GroupInfoDialog GroupInfoDialog;

        public GroupDialog GroupDialog;
        public GuildDialog GuildDialog;

        public BigMapDialog BigMapDialog;
        public TrustMerchantDialog TrustMerchantDialog;
        public CharacterDuraPanel CharacterDuraPanel;
        public DuraStatusDialog DuraStatusPanel;
        public TradeDialog TradeDialog;
        public GuestTradeDialog GuestTradeDialog;

        //public SkillBarDialog SkillBarDialog;
        public List<SkillBarDialog> SkillBarDialogs = new List<SkillBarDialog>();
        public ChatOptionDialog ChatOptionDialog;
        public ChatNoticeDialog ChatNoticeDialog;

        public QuestListDialog QuestListDialog;
        public QuestDetailDialog QuestDetailDialog;
        public QuestDiaryDialog QuestLogDialog;
        public QuestTrackingDialog QuestTrackingDialog;

        public RankingDialog RankingDialog;

        public MailListDialog MailListDialog;
        public MailComposeLetterDialog MailComposeLetterDialog;
        public MailComposeParcelDialog MailComposeParcelDialog;
        public MailReadLetterDialog MailReadLetterDialog;
        public MailReadParcelDialog MailReadParcelDialog;

        public IntelligentCreatureDialog IntelligentCreatureDialog;
        public IntelligentCreatureOptionsDialog IntelligentCreatureOptionsDialog;
        public IntelligentCreatureOptionsGradeDialog IntelligentCreatureOptionsGradeDialog;

        public FriendDialog FriendDialog;
        public MemoDialog MemoDialog;
        public RelationshipDialog RelationshipDialog;
        public MentorDialog MentorDialog;
        public GameShopDialog GameShopDialog;

        public ReportDialog ReportDialog;

        public ItemRentingDialog ItemRentingDialog;
        public ItemRentDialog ItemRentDialog;
        public GuestItemRentingDialog GuestItemRentingDialog;
        public GuestItemRentDialog GuestItemRentDialog;
        public ItemRentalDialog ItemRentalDialog;

        public ItemLabelPanel ItemLabelPanel;

        public BuffDialog BuffsDialog;
        public BuffDialog DebuffsDialog;

        public AssistDialog AssistDialog;
        public SocketingDialog SocketingDialog;

        //not added yet
        public KeyboardLayoutDialog KeyboardLayoutDialog;

        public static Dictionary<int, ItemInfo> ItemInfoList = new Dictionary<int, ItemInfo>();
        public static List<UserId> UserIdList = new List<UserId>();
        public static List<ChatItem> ChatItemList = new List<ChatItem>();
        public static List<ClientQuestInfo> QuestInfoList = new List<ClientQuestInfo>();
        public static List<GameShopItem> GameShopInfoList = new List<GameShopItem>();
        public static List<ClientRecipeInfo> RecipeInfoList = new List<ClientRecipeInfo>();

        public List<Buff> Buffs = new List<Buff>();

        public static UserItem[] Storage = new UserItem[80];
        public static UserItem[] GuildStorage = new UserItem[112];
        public static UserItem[] Refine = new UserItem[16];
        public static UserItem HoverItem;
        public static bool HoverItemInspect;
        public static MirItemCell SelectedCell;
        public static EquipmentSlot LastEquipmentSlot;
        public static UserItem[] CraftEndResult = new UserItem[1];

        public static bool PickedUpGold;
        public MirControl MailLabel, MemoLabel, GuildBuffLabel;
        public static long UseItemTime, PickUpTime, DropViewTime, TargetDeadTime;
        public static uint Gold, Credit;
        public static long InspectTime;
        public bool ShowReviveMessage;

        public bool NewMail;
        public int NewMailCounter = 0;

        public AttackMode AMode;
        public PetMode PMode;
        public LightSetting Lights;

        public static long NPCTime;
        public static uint NPCID;
        public static float NPCRate;
        public static uint DefaultNPCID;

        public long ToggleTime;

        public static long SpellTime;

        public long PingTime;
        public long NextPing = 10000;

        public MirLabel[] OutputLines = new MirLabel[10];
        public List<OutPutMessage> OutputMessages = new List<OutPutMessage>();

        public long OutputDelay;

        public AssistHelper AssistHelper = new AssistHelper();

        public static bool NextTimeFireHit;
        public static long LastFireHitTick;
        public static bool NextTimeDaMoHit;
        public static long LastDamoHitTick;
        public static bool Closed;
        public static bool Closing;

        public GameScene()
        {
            MapControl.AutoRun = false;
            MapControl.AutoHit = false;

            Scene = this;
            BackColour = Color.Transparent;
            MoveTime = CMain.Time;

            KeyDown += GameScene_KeyDown;

            MainDialog = new MainDialog { Parent = this };
            ChatDialog = new ChatDialog { Parent = this };
            ChatControl = new ChatControlBar { Parent = this };
            InventoryDialog = new InventoryDialog { Parent = this };
            CharacterDialog = new CharacterDialog { Parent = this, Visible = false };
            BeltDialog = new BeltDialog { Parent = this };
            StorageDialog = new StorageDialog { Parent = this, Visible = false };
            CraftDialog = new CraftDialog { Parent = this, Visible = false };
            MiniMapDialog = new MiniMapDialog { Parent = this };
            InspectDialog = new InspectDialog { Parent = this, Visible = false };
            OptionDialog = new OptionDialog { Parent = this, Visible = false };
            MenuDialog = new MenuDialog { Parent = this, Visible = false };
            NPCDialog = new NPCDialog { Parent = this, Visible = false };
            NPCGoodsDialog = new NPCGoodsDialog { Parent = this, Visible = false };
            NPCDropDialog = new NPCDropDialog { Parent = this, Visible = false };
            NPCAwakeDialog = new NPCAwakeDialog { Parent = this, Visible = false };

            HelpDialog = new HelpDialog { Parent = this, Visible = false };

            MountDialog = new MountDialog { Parent = this, Visible = false };
            FishingDialog = new FishingDialog { Parent = this, Visible = false };
            FishingStatusDialog = new FishingStatusDialog { Parent = this, Visible = false };
            
            GroupDialog = new GroupDialog { Parent = this, Visible = false };
            GuildDialog = new GuildDialog { Parent = this, Visible = false };
            GroupInfoDialog = new GroupInfoDialog { Parent = this, Visible = false };

            BigMapDialog = new BigMapDialog { Parent = this, Visible = false };
            TrustMerchantDialog = new TrustMerchantDialog { Parent = this, Visible = false };
            CharacterDuraPanel = new CharacterDuraPanel { Parent = this, Visible = false };
            DuraStatusPanel = new DuraStatusDialog { Parent = this, Visible = true };
            TradeDialog = new TradeDialog { Parent = this, Visible = false };
            GuestTradeDialog = new GuestTradeDialog { Parent = this, Visible = false };

            //SkillBarDialog = new SkillBarDialog { Parent = this, Visible = false };
            SkillBarDialog Bar1 = new SkillBarDialog { Parent = this, Visible = false, BarIndex = 0, BindIndex = 0 };
            SkillBarDialogs.Add(Bar1);
            SkillBarDialog Bar2 = new SkillBarDialog { Parent = this, Visible = false, BarIndex = 1, BindIndex = 1 };
            SkillBarDialogs.Add(Bar2);
            ChatOptionDialog = new ChatOptionDialog { Parent = this, Visible = false };
            ChatNoticeDialog = new ChatNoticeDialog { Parent = this, Visible = false };

            QuestListDialog = new QuestListDialog { Parent = this, Visible = false };
            QuestDetailDialog = new QuestDetailDialog {Parent = this, Visible = false};
            QuestTrackingDialog = new QuestTrackingDialog { Parent = this, Visible = false };
            QuestLogDialog = new QuestDiaryDialog {Parent = this, Visible = false};

            RankingDialog = new RankingDialog { Parent = this, Visible = false };

            MailListDialog = new MailListDialog { Parent = this, Visible = false };
            MailComposeLetterDialog = new MailComposeLetterDialog { Parent = this, Visible = false };
            MailComposeParcelDialog = new MailComposeParcelDialog { Parent = this, Visible = false };
            MailReadLetterDialog = new MailReadLetterDialog { Parent = this, Visible = false };
            MailReadParcelDialog = new MailReadParcelDialog { Parent = this, Visible = false };

            IntelligentCreatureDialog = new IntelligentCreatureDialog { Parent = this, Visible = false };
            IntelligentCreatureOptionsDialog = new IntelligentCreatureOptionsDialog { Parent = this, Visible = false };
            IntelligentCreatureOptionsGradeDialog = new IntelligentCreatureOptionsGradeDialog { Parent = this, Visible = false };

            RefineDialog = new RefineDialog { Parent = this, Visible = false };
            RelationshipDialog = new RelationshipDialog { Parent = this, Visible = false };
            FriendDialog = new FriendDialog { Parent = this, Visible = false };
            MemoDialog = new MemoDialog { Parent = this, Visible = false };
            MentorDialog = new MentorDialog { Parent = this, Visible = false };
            GameShopDialog = new GameShopDialog { Parent = this, Visible = false };

            ReportDialog = new ReportDialog { Parent = this, Visible = false };

            ItemRentingDialog = new ItemRentingDialog { Parent = this, Visible = false };
            ItemRentDialog = new ItemRentDialog { Parent = this, Visible = false };
            GuestItemRentingDialog = new GuestItemRentingDialog { Parent = this, Visible = false };
            GuestItemRentDialog = new GuestItemRentDialog { Parent = this, Visible = false };
            ItemRentalDialog = new ItemRentalDialog { Parent = this, Visible = false };

            BuffsDialog = new BuffDialog {Parent = this, Visible = true};
            DebuffsDialog = new BuffDialog { Parent = this, Visible = true};
            DebuffsDialog.Location = new Point(Settings.ScreenWidth - 170, 130);

            AssistDialog = new AssistDialog { Parent = this, Visible = false };
            SocketingDialog = new SocketingDialog { Parent = this, Visible = false };

            //not added yet
            KeyboardLayoutDialog = new KeyboardLayoutDialog { Parent = this, Visible = false };

            for (int i = 0; i < OutputLines.Length; i++)
                OutputLines[i] = new MirLabel
                {
                    AutoSize = true,
                    BackColour = Color.Transparent,
                    Font = new Font(Settings.FontName, 8F),
                    ForeColour = Color.LimeGreen,
                    Location = new Point(20, 25 + i * 15),
                    OutLine = true,
                };

        }

        public void CreateItemLabel(UserItem showItem, bool Inspect = false)
        {
            if (ItemLabelPanel == null)
            {
                ItemLabelPanel = new ItemLabelPanel();
                ItemLabelPanel.InspectDialog = InspectDialog;
                ItemLabelPanel.ItemInfoList = ItemInfoList;
            }

            ItemLabelPanel.CreateItemLabel(showItem, Inspect);
        }

        public void OutputMessageTr(string message, OutputMessageType type = OutputMessageType.Normal)
        {
            message = CMain.Tr(message);
            OutputMessage(message, type);
        }

        public void OutputMessage(string message, OutputMessageType type = OutputMessageType.Normal)
        {
            OutputMessages.Add(new OutPutMessage { Message = message, ExpireTime = CMain.Time + 5000, Type = type });
            if (OutputMessages.Count > 10)
                OutputMessages.RemoveAt(0);
        }

        private void ProcessOuput()
        {
            for (int i = 0; i < OutputMessages.Count; i++)
            {
                if (CMain.Time >= OutputMessages[i].ExpireTime)
                    OutputMessages.RemoveAt(i);
            }

            for (int i = 0; i < OutputLines.Length; i++)
            {
                if (OutputMessages.Count > i)
                {
                    Color color;
                    switch (OutputMessages[i].Type)
                    {
                        case OutputMessageType.Quest:
                            color = Color.Gold;
                            break;
                        case OutputMessageType.Guild:
                            color = Color.DeepPink;
                            break;
                        default:
                            color = Color.LimeGreen;
                            break;
                    }

                    OutputLines[i].Text = OutputMessages[i].Message;
                    OutputLines[i].ForeColour = color;
                    OutputLines[i].Visible = true;
                }
                else
                {
                    OutputLines[i].Text = string.Empty;
                    OutputLines[i].Visible = false;
                }
            }
        }
        private void GameScene_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Alt || e.KeyCode == Keys.F10)
                e.SuppressKeyPress = true;
            foreach (KeyBind KeyCheck in CMain.InputKeys.Keylist)
            {
                if (KeyCheck.CutomKey.Key == Keys.None)
                    continue;
                if (KeyCheck.CutomKey.Key != e.KeyCode)
                    continue;
                if ((KeyCheck.CutomKey.RequireAlt != 2) && (KeyCheck.CutomKey.RequireAlt != (CMain.Alt ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireShift != 2) && (KeyCheck.CutomKey.RequireShift != (CMain.Shift ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireCtrl != 2) && (KeyCheck.CutomKey.RequireCtrl != (CMain.Ctrl ? 1 : 0)))
                    continue;
                if ((KeyCheck.CutomKey.RequireTilde != 2) && (KeyCheck.CutomKey.RequireTilde != (CMain.Tilde ? 1 : 0)))
                    continue;
                //now run the real code
                switch (KeyCheck.function)
                {
                    case KeybindOptions.Bar1Skill1: UseSpell(1); break;
                    case KeybindOptions.Bar1Skill2: UseSpell(2); break;
                    case KeybindOptions.Bar1Skill3: UseSpell(3); break;
                    case KeybindOptions.Bar1Skill4: UseSpell(4); break;
                    case KeybindOptions.Bar1Skill5: UseSpell(5); break;
                    case KeybindOptions.Bar1Skill6: UseSpell(6); break;
                    case KeybindOptions.Bar1Skill7: UseSpell(7); break;
                    case KeybindOptions.Bar1Skill8: UseSpell(8); break;
                    case KeybindOptions.Bar2Skill1: UseSpell(9); break;
                    case KeybindOptions.Bar2Skill2: UseSpell(10); break;
                    case KeybindOptions.Bar2Skill3: UseSpell(11); break;
                    case KeybindOptions.Bar2Skill4: UseSpell(12); break;
                    case KeybindOptions.Bar2Skill5: UseSpell(13); break;
                    case KeybindOptions.Bar2Skill6: UseSpell(14); break;
                    case KeybindOptions.Bar2Skill7: UseSpell(15); break;
                    case KeybindOptions.Bar2Skill8: UseSpell(16); break;
                    case KeybindOptions.Inventory:
                    case KeybindOptions.Inventory2:
                        if (!InventoryDialog.Visible) InventoryDialog.Show();
                        else InventoryDialog.Hide();
                        break;
                    case KeybindOptions.Equipment:
                    case KeybindOptions.Equipment2:
                        if (!CharacterDialog.Visible || !CharacterDialog.CharacterPage.Visible)
                        {
                            CharacterDialog.Show();
                            CharacterDialog.ShowCharacterPage();
                        }
                        else CharacterDialog.Hide();
                        break;
                    case KeybindOptions.Skills:
                    case KeybindOptions.Skills2:
                        if (!CharacterDialog.Visible || !CharacterDialog.SkillPage.Visible)
                        {
                            CharacterDialog.Show();
                            CharacterDialog.ShowSkillPage();
                        }
                        else CharacterDialog.Hide();
                        break;
                    case KeybindOptions.Creature:
                        if (!IntelligentCreatureDialog.Visible) IntelligentCreatureDialog.Show();
                        else IntelligentCreatureDialog.Hide();
                        break;
                    case KeybindOptions.MountWindow:
                        if (!MountDialog.Visible) MountDialog.Show();
                        else MountDialog.Hide();
                        break;

                    case KeybindOptions.GameShop:
                        if (!GameShopDialog.Visible) GameShopDialog.Show();
                        else GameShopDialog.Hide();
                        break;
                    case KeybindOptions.Fishing:
                        if (!FishingDialog.Visible) FishingDialog.Show();
                        else FishingDialog.Hide();
                        break;
                    case KeybindOptions.Skillbar:
                        UpdateSkillBar();
                        break;
                    case KeybindOptions.Mount:
                        if (GameScene.Scene.MountDialog.CanRide())
                            GameScene.Scene.MountDialog.Ride();
                        break;
                    case KeybindOptions.Mentor:
                        if (!MentorDialog.Visible) MentorDialog.Show();
                        else MentorDialog.Hide();
                        break;
                    case KeybindOptions.Relationship:
                        if (!RelationshipDialog.Visible) RelationshipDialog.Show();
                        else RelationshipDialog.Hide();
                        break;
                    case KeybindOptions.Friends:
                        if (!FriendDialog.Visible) FriendDialog.Show();
                        else FriendDialog.Hide();
                        break;
                    case KeybindOptions.Guilds:
                        if (!GuildDialog.Visible) GuildDialog.Show();
                        else
                            GuildDialog.Hide();
                        break;

                    case KeybindOptions.Ranking:
                        if (!RankingDialog.Visible) RankingDialog.Show();
                        else RankingDialog.Hide();
                        break;

                    case KeybindOptions.Quests:
                        if (!QuestLogDialog.Visible) QuestLogDialog.Show();
                        else QuestLogDialog.Hide();
                        break;

                    case KeybindOptions.Exit:
                        QuitGame();
                        return;

                    case KeybindOptions.Closeall:
                        InventoryDialog.Hide();
                        CharacterDialog.Hide();
                        OptionDialog.Hide();
                        MenuDialog.Hide();
                        if (NPCDialog.Visible) NPCDialog.Hide();
                        HelpDialog.Hide();
                        KeyboardLayoutDialog.Hide();
                        RankingDialog.Hide();
                        IntelligentCreatureDialog.Hide();
                        IntelligentCreatureOptionsDialog.Hide();
                        IntelligentCreatureOptionsGradeDialog.Hide();
                        MountDialog.Hide();
                        FishingDialog.Hide();
                        FriendDialog.Hide();
                        RelationshipDialog.Hide();
                        MentorDialog.Hide();
                        GameShopDialog.Hide();
                        GroupDialog.Hide();
                        GuildDialog.Hide();
                        InspectDialog.Hide();
                        StorageDialog.Hide();
                        TrustMerchantDialog.Hide();
                        //CharacterDuraPanel.Hide();
                        QuestListDialog.Hide();
                        QuestDetailDialog.Hide();
                        QuestLogDialog.Hide();
                        NPCAwakeDialog.Hide();
                        RefineDialog.Hide();
                        BigMapDialog.Visible = false;
                        if (FishingStatusDialog.bEscExit) FishingStatusDialog.Cancel();
                        MailComposeLetterDialog.Hide();
                        MailComposeParcelDialog.Hide();
                        MailListDialog.Hide();
                        MailReadLetterDialog.Hide();
                        MailReadParcelDialog.Hide();
                        ItemRentalDialog.Visible = false;

                        if (ItemLabelPanel != null)
                            ItemLabelPanel.DisposeItemLabel();
                        break;
                    case KeybindOptions.Options:
                    case KeybindOptions.Options2:
                        if (!OptionDialog.Visible) OptionDialog.Show();
                        else OptionDialog.Hide();
                        break;
                    case KeybindOptions.Group:
                        if (!GroupDialog.Visible) GroupDialog.Show();
                        else GroupDialog.Hide();
                        break;
                    case KeybindOptions.Belt:
                        if (!BeltDialog.Visible) BeltDialog.Show();
                        else BeltDialog.Hide();
                        break;
                    case KeybindOptions.BeltFlip:
                        BeltDialog.Flip();
                        break;
                    case KeybindOptions.Pickup:
                        if (CMain.Time > PickUpTime)
                        {
                            PickUpTime = CMain.Time + 200;
                            Network.Enqueue(new C.PickUp());
                        }
                        break;
                    case KeybindOptions.Belt1:
                    case KeybindOptions.Belt1Alt:
                        BeltDialog.Grid[0].UseItem();
                        break;
                    case KeybindOptions.Belt2:
                    case KeybindOptions.Belt2Alt:
                        BeltDialog.Grid[1].UseItem();
                        break;
                    case KeybindOptions.Belt3:
                    case KeybindOptions.Belt3Alt:
                        BeltDialog.Grid[2].UseItem();
                        break;
                    case KeybindOptions.Belt4:
                    case KeybindOptions.Belt4Alt:
                        BeltDialog.Grid[3].UseItem();
                        break;
                    case KeybindOptions.Belt5:
                    case KeybindOptions.Belt5Alt:
                        BeltDialog.Grid[4].UseItem();
                        break;
                    case KeybindOptions.Belt6:
                    case KeybindOptions.Belt6Alt:
                        BeltDialog.Grid[5].UseItem();
                        break;
                    case KeybindOptions.Logout:
                        {
                            LogOut();
                            e.Handled = true;
                        }
                        break;
                    case KeybindOptions.Minimap:
                        MiniMapDialog.Toggle();
                        break;
                    case KeybindOptions.Bigmap:
                        BigMapDialog.Toggle();
                        break;
                    case KeybindOptions.Trade:
                        Network.Enqueue(new C.TradeRequest());
                        break;
                    case KeybindOptions.Rental:
                        //ItemRentalDialog.Toggle();
                        break;
                    case KeybindOptions.ChangePetmode:
                        switch (PMode)
                        {
                            case PetMode.Both:
                                Network.Enqueue(new C.ChangePMode { Mode = PetMode.MoveOnly });
                                return;
                            case PetMode.MoveOnly:
                                Network.Enqueue(new C.ChangePMode { Mode = PetMode.AttackOnly });
                                return;
                            case PetMode.AttackOnly:
                                Network.Enqueue(new C.ChangePMode { Mode = PetMode.None });
                                return;
                            case PetMode.None:
                                Network.Enqueue(new C.ChangePMode { Mode = PetMode.Both });
                                return;
                        }
                        break;
                    case KeybindOptions.PetmodeBoth:
                        Network.Enqueue(new C.ChangePMode { Mode = PetMode.Both });
                        return;
                    case KeybindOptions.PetmodeMoveonly:
                        Network.Enqueue(new C.ChangePMode { Mode = PetMode.MoveOnly });
                        return;
                    case KeybindOptions.PetmodeAttackonly:
                        Network.Enqueue(new C.ChangePMode { Mode = PetMode.AttackOnly });
                        return;
                    case KeybindOptions.PetmodeNone:
                        Network.Enqueue(new C.ChangePMode { Mode = PetMode.None });
                        return;
                    case KeybindOptions.CreatureAutoPickup://semiauto!
                        Network.Enqueue(new C.IntelligentCreaturePickup { MouseMode = false, Location = MapControl.MapLocation });
                        break;
                    case KeybindOptions.CreaturePickup:
                        Network.Enqueue(new C.IntelligentCreaturePickup { MouseMode = true, Location = MapControl.MapLocation });
                        break;
                    case KeybindOptions.ChangeAttackmode:
                        switch (AMode)
                        {
                            case AttackMode.Peace:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Group });
                                return;
                            case AttackMode.Group:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Guild });
                                return;
                            case AttackMode.Guild:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.EnemyGuild });
                                return;
                            case AttackMode.EnemyGuild:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.RedBrown });
                                return;
                            case AttackMode.RedBrown:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.All });
                                return;
                            case AttackMode.All:
                                Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Peace });
                                return;
                        }
                        break;
                    case KeybindOptions.AttackmodePeace:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Peace });
                        return;
                    case KeybindOptions.AttackmodeGroup:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Group });
                        return;
                    case KeybindOptions.AttackmodeGuild:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.Guild });
                        return;
                    case KeybindOptions.AttackmodeEnemyguild:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.EnemyGuild });
                        return;
                    case KeybindOptions.AttackmodeRedbrown:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.RedBrown });
                        return;
                    case KeybindOptions.AttackmodeAll:
                        Network.Enqueue(new C.ChangeAMode { Mode = AttackMode.All });
                        return;

                    case KeybindOptions.Help:
                        if (!HelpDialog.Visible) HelpDialog.Show();
                        else HelpDialog.Hide();
                        break;
                    case KeybindOptions.Autorun:
                        MapControl.AutoRun = !MapControl.AutoRun;
                        break;
                    case KeybindOptions.Cameramode:

                        if (!MainDialog.Visible)
                        {
                            MainDialog.Show();
                            ChatDialog.Show();
                            BeltDialog.Show();
                            ChatControl.Show();
                            MiniMapDialog.Show();
                            CharacterDuraPanel.Show();
                            DuraStatusPanel.Show();
                        }
                        else
                        {
                            MainDialog.Hide();
                            ChatDialog.Hide();
                            BeltDialog.Hide();
                            ChatControl.Hide();
                            MiniMapDialog.Hide();
                            CharacterDuraPanel.Hide();
                            DuraStatusPanel.Hide();
                        }
                        break;
                    case KeybindOptions.DropView:
                        if (CMain.Time > DropViewTime)
                            DropViewTime = CMain.Time + 5000;
                        break;
                    case KeybindOptions.TargetDead:
                        if (CMain.Time > TargetDeadTime)
                            TargetDeadTime = CMain.Time + 5000;
                        break;
                    case KeybindOptions.AddGroupMember:
                        if (MapObject.MouseObject == null) break;
                        if (MapObject.MouseObject.Race != ObjectType.Player) break;

                        GameScene.Scene.GroupDialog.AddMember(MapObject.MouseObject.Name);
                        break;

                    case KeybindOptions.Assist:
                        if (AssistDialog.Visible)
                            AssistDialog.Hide();
                        else
                            AssistDialog.Show();
                        break;

                    case KeybindOptions.SwitchSkillBar:
                        if (!Settings.SkillMode)
                            return;

                        if (SkillBarDialogs[0].BindIndex == 0)
                            SkillBarDialogs[0].BindIndex = 1;
                        else
                            SkillBarDialogs[0].BindIndex = 0;

                        SkillBarDialogs[0].Update();

                        ChatDialog.ReceiveChat(string.Format(CMain.Tr("Use skill bar {0}"), SkillBarDialogs[0].BindIndex + 1), ChatType.System);
                        break;
                }
            }
        }

        public void UseSpell(ClientMagic magic)
        {
            if (magic == null) return;
            int cost;

            MagicConf magicConf = CMain.MagicConf.GetConf(magic.Spell);
            if (magicConf == null || magicConf.IsPassive)
                return;

            if (magicConf.NeedToggle)
            {
                if (magicConf.ToggleInverval > 0 && CMain.Time < ToggleTime)
                    return;

                ToggleTime = CMain.Time + magicConf.ToggleInverval;

                if (!magicConf.CostMP)
                {
                    magic.Toggle = !magic.Toggle;
                    ChatDialog.ReceiveChatTr(magic.Toggle ? String.Format(CMain.Tr("Use {0}."), CMain.Tr(magic.Spell.ToString()))
                    : String.Format(CMain.Tr("Do not use {0}."), CMain.Tr(magic.Spell.ToString())), ChatType.Hint);
                    Network.Enqueue(new C.SpellToggle { Spell = magic.Spell, CanUse = magic.Toggle });
                }
                else
                {
                    cost = magic.Level * magic.LevelCost + magic.BaseCost;
                    if (cost > MapObject.User.MP)
                    {
                        Scene.OutputMessage(CMain.Tr("Not Enough Mana to cast."));
                        return;
                    }

                    if (magicConf.Sound > 0)
                    {
                        SoundManager.PlaySound(20000 + (ushort)magic.Spell * 10);
                    }

                    if (magicConf.CastEffect > 0)
                    {
                        EffectConf effectConf = CMain.MagicConf.GetEffectConf(magicConf.CastEffect);
                        if (effectConf != null)
                            User.Effects.Add(new Effect(Libraries.Magic2, 210, 6, 500, User));
                    }

                    magic.Toggle = true;
                    Network.Enqueue(new C.SpellToggle { Spell = magic.Spell, CanUse = magic.Toggle });
                }
            }
            else
            {
                User.NextMagic = magic;
                User.NextMagicLocation = MapControl.MapLocation;
                User.NextMagicObject = MapObject.MouseObject;
                User.NextMagicDirection = MapControl.MouseDirection();
            }

        }

        public void OutputMessageIfPrepared(string message)
        {
            if (CMain.Time >= OutputDelay)
            {
                OutputDelay = CMain.Time + 1000;
                GameScene.Scene.OutputMessage(message);
            }
        }

        public void UseSpell(int key)
        {
            if (User.RidingMount || User.Fishing) return;

            if (!User.HasClassWeapon && User.Weapon >= 0)
            {
                ChatDialog.ReceiveChat(CMain.Tr("You must be wearing a suitable weapon to perform this skill"), ChatType.System);
                return;
            }

            if (CMain.Time < User.ChannelingTime || CMain.Time < User.ReincarnationStopTime) return;

            ClientMagic magic = null;

            if (key < 8)
            {
                SkillBarDialogs[0].UseSpell(key);
            }
            else
            {
                SkillBarDialogs[1].UseSpell(key - 8);
            }
        }

        public void UseSpell(Spell spell)
        {
            if (User.RidingMount || User.Fishing) return;

            ClientMagic magic = User.GetMagic(spell);
            if (magic == null)
                return;

            if (!User.HasClassWeapon && User.Weapon >= 0)
            {
                ChatDialog.ReceiveChat(CMain.Tr("You must be wearing a suitable weapon to perform this skill"), ChatType.System);
                return;
            }

            if (CMain.Time < User.ChannelingTime || CMain.Time < User.ReincarnationStopTime) return;

            UseSpell(magic);
        }

        public void QuitGame()
        {
            if (Closing)
            {
                Closed = true;
                Program.Form.Close();
                return;
            }

            Closing = true;
            MirMessageBox messageBox = new MirMessageBox(CMain.Tr("Do you want to quit Legend of Mir?"), MirMessageBoxButtons.YesNo);
            messageBox.YesButton.Click += (o, e) =>
            {
                AssistHelper.Save();
                Closed = true;
                Program.Form.Close();
            };

            messageBox.NoButton.Click += (o, e) =>
            {
                Closing = false;
            };

            messageBox.Show();
        }

        public void LogOut()
        {
            MirMessageBox messageBox = new MirMessageBox(CMain.Tr("Do you want to log out of Legend of Mir?"), MirMessageBoxButtons.YesNo);
            messageBox.YesButton.Click += (o, e) =>
            {
                AssistHelper.Save();
                Network.Enqueue(new C.LogOut());
                Enabled = false;
            };
            messageBox.Show();
        }

        protected internal override void DrawControl()
        {
            if (MapControl != null && !MapControl.IsDisposed)
                MapControl.DrawControl();

            base.DrawControl();


            if (PickedUpGold || (SelectedCell != null && SelectedCell.Item != null))
            {
                int image = PickedUpGold ? 116 : SelectedCell.Item.Image;
                Size imgSize = Libraries.Items.GetTrueSize(image);
                Point p = CMain.MPoint.Add(-imgSize.Width / 2, -imgSize.Height / 2);

                if (p.X + imgSize.Width >= Settings.ScreenWidth)
                    p.X = Settings.ScreenWidth - imgSize.Width;

                if (p.Y + imgSize.Height >= Settings.ScreenHeight)
                    p.Y = Settings.ScreenHeight - imgSize.Height;

                Libraries.Items.Draw(image, p.X, p.Y);
            }

            for (int i = 0; i < OutputLines.Length; i++)
                OutputLines[i].Draw();
        }
        public override void Process()
        {
            if (MapControl == null || User == null)
                return;

            if (CMain.Time >= MoveTime)
            {
                MoveTime += 100; //Move Speed
                CanMove = true;
                MapControl.AnimationCount++;
                MapControl.TextureValid = false;
            }
            else
                CanMove = false;


            if (CMain.Time >= NextPing)
            {
                NextPing = CMain.Time + 20000;
                Network.Enqueue(new C.KeepAlive() { Time = CMain.Time });
            }

            if (HoverItem != null)
            {
                CreateItemLabel(HoverItem, HoverItemInspect);
            }
            else
            {
                DisposeItemLabel();
            }

            if (ItemLabelPanel != null)
                ItemLabelPanel.Process();

            if (MailLabel != null && !MailLabel.IsDisposed)
            {
                MailLabel.BringToFront();

                int x = CMain.MPoint.X + 15, y = CMain.MPoint.Y;
                if (x + MailLabel.Size.Width > Settings.ScreenWidth)
                    x = Settings.ScreenWidth - MailLabel.Size.Width;

                if (y + MailLabel.Size.Height > Settings.ScreenHeight)
                    y = Settings.ScreenHeight - MailLabel.Size.Height;
                MailLabel.Location = new Point(x, y);
            }

            if (MemoLabel != null && !MemoLabel.IsDisposed)
            {
                MemoLabel.BringToFront();

                int x = CMain.MPoint.X + 15, y = CMain.MPoint.Y;
                if (x + MemoLabel.Size.Width > Settings.ScreenWidth)
                    x = Settings.ScreenWidth - MemoLabel.Size.Width;

                if (y + MemoLabel.Size.Height > Settings.ScreenHeight)
                    y = Settings.ScreenHeight - MemoLabel.Size.Height;
                MemoLabel.Location = new Point(x, y);
            }

            if (GuildBuffLabel != null && !GuildBuffLabel.IsDisposed)
            {
                GuildBuffLabel.BringToFront();

                int x = CMain.MPoint.X + 15, y = CMain.MPoint.Y;
                if (x + GuildBuffLabel.Size.Width > Settings.ScreenWidth)
                    x = Settings.ScreenWidth - GuildBuffLabel.Size.Width;

                if (y + GuildBuffLabel.Size.Height > Settings.ScreenHeight)
                    y = Settings.ScreenHeight - GuildBuffLabel.Size.Height;
                GuildBuffLabel.Location = new Point(x, y);
            }

            if (!User.Dead) ShowReviveMessage = false;

            if (ShowReviveMessage && CMain.Time > User.DeadTime && User.CurrentAction == MirAction.Dead)
            {
                ShowReviveMessage = false;
                MirMessageBox messageBox = new MirMessageBox(CMain.Tr("You have died, Do you want to revive in town?"), MirMessageBoxButtons.YesNo, false);

                messageBox.YesButton.Click += (o, e) =>
                {
                    if (User.Dead) Network.Enqueue(new C.TownRevive());
                };

                messageBox.AfterDraw += (o, e) =>
                {
                    if (!User.Dead) messageBox.Dispose();
                };

                messageBox.Show();
            }

            BuffsDialog.Process();
            DebuffsDialog.Process();

            MapControl.Process();
            MainDialog.Process();
            InventoryDialog.Process();
            GameShopDialog.Process();
            MiniMapDialog.Process();
            GroupInfoDialog.Process();
            foreach (SkillBarDialog Bar in Scene.SkillBarDialogs)
                Bar.Process();

            DialogProcess();

            ProcessOuput();

            AssistHelper.Process();
        }

        public void UpdateSkillBar()
        {
            if (Settings.SkillBar)
            {
                if (Settings.SkillMode)
                {
                    Scene.SkillBarDialogs[0].Show();
                    Scene.SkillBarDialogs[1].Hide();
                }
                else
                {
                    Scene.SkillBarDialogs[0].BindIndex = 0;
                    Scene.SkillBarDialogs[0].Show();
                    Scene.SkillBarDialogs[1].BindIndex = 1;
                    Scene.SkillBarDialogs[1].Show();
                }
            }
            else
            {
                foreach (SkillBarDialog Bar in Scene.SkillBarDialogs)
                    Bar.Hide();
            }

            int barWidth = Scene.SkillBarDialogs[0].TrueSize.Width;
            int barHeight = Scene.SkillBarDialogs[0].TrueSize.Height;
            for (int i = 0; i < Scene.SkillBarDialogs.Count; i++)
            {
                if (i * 2 > Settings.SkillbarLocation.Length) break;

                if (Settings.SkillbarLocation[i, 0] > Settings.ScreenWidth - barWidth) 
                {
                    Settings.SkillbarLocation[i, 0] = Settings.ScreenWidth - barWidth;
                }
                if (Settings.SkillbarLocation[i, 1] > Settings.ScreenHeight - barHeight)
                {
                    Settings.SkillbarLocation[i, 1] = Settings.ScreenHeight - barHeight;
                }

                if (Settings.SkillbarLocation[i, 0] < 0)
                    Settings.SkillbarLocation[i, 0] = 0;
                if (Settings.SkillbarLocation[i, 1] < 0)
                    Settings.SkillbarLocation[i, 1] = 0;

                Scene.SkillBarDialogs[i].Location = new Point(Settings.SkillbarLocation[i, 0], Settings.SkillbarLocation[i, 1]);
            }
        }

        public void DialogProcess()
        {
            if (Settings.DuraView)
                CharacterDuraPanel.Show();
            else
                CharacterDuraPanel.Hide();
        }

        public override void ProcessPacket(Packet p)
        {
            switch (p.Index)
            {
                case (short)ServerPacketIds.KeepAlive:
                    KeepAlive((S.KeepAlive)p);
                    break;
                case (short)ServerPacketIds.MapInformation: //MapInfo
                    MapInformation((S.MapInformation)p);
                    break;
                case (short)ServerPacketIds.UserInformation:
                    UserInformation((S.UserInformation)p);
                    break;
                case (short)ServerPacketIds.UserLocation:
                    UserLocation((S.UserLocation)p);
                    break;
                case (short)ServerPacketIds.ObjectPlayer:
                    ObjectPlayer((S.ObjectPlayer)p);
                    break;
                case (short)ServerPacketIds.ObjectRemove:
                    ObjectRemove((S.ObjectRemove)p);
                    break;
                case (short)ServerPacketIds.ObjectTurn:
                    ObjectTurn((S.ObjectTurn)p);
                    break;
                case (short)ServerPacketIds.ObjectWalk:
                    ObjectWalk((S.ObjectWalk)p);
                    break;
                case (short)ServerPacketIds.ObjectRun:
                    ObjectRun((S.ObjectRun)p);
                    break;
                case (short)ServerPacketIds.Chat:
                    ReceiveChat((S.Chat)p);
                    break;
                case (short)ServerPacketIds.ObjectChat:
                    ObjectChat((S.ObjectChat)p);
                    break;
                case (short)ServerPacketIds.MoveItem:
                    MoveItem((S.MoveItem)p);
                    break;
                case (short)ServerPacketIds.EquipItem:
                    EquipItem((S.EquipItem)p);
                    break;
                case (short)ServerPacketIds.MergeItem:
                    MergeItem((S.MergeItem)p);
                    break;
                case (short)ServerPacketIds.RemoveItem:
                    RemoveItem((S.RemoveItem)p);
                    break;
                case (short)ServerPacketIds.RemoveSlotItem:
                    RemoveSlotItem((S.RemoveSlotItem)p);
                    break;
                case (short)ServerPacketIds.TakeBackItem:
                    TakeBackItem((S.TakeBackItem)p);
                    break;
                case (short)ServerPacketIds.StoreItem:
                    StoreItem((S.StoreItem)p);
                    break;
                case (short)ServerPacketIds.DepositRefineItem:
                    DepositRefineItem((S.DepositRefineItem)p);
                    break;
                case (short)ServerPacketIds.RetrieveRefineItem:
                    RetrieveRefineItem((S.RetrieveRefineItem)p);
                    break;
                case (short)ServerPacketIds.RefineCancel:
                    RefineCancel((S.RefineCancel)p);
                    break;
                case (short)ServerPacketIds.RefineItem:
                    RefineItem((S.RefineItem)p);
                    break;
                case (short)ServerPacketIds.DepositTradeItem:
                    DepositTradeItem((S.DepositTradeItem)p);
                    break;
                case (short)ServerPacketIds.RetrieveTradeItem:
                    RetrieveTradeItem((S.RetrieveTradeItem)p);
                    break;
                case (short)ServerPacketIds.SplitItem:
                    SplitItem((S.SplitItem)p);
                    break;
                case (short)ServerPacketIds.SplitItem1:
                    SplitItem1((S.SplitItem1)p);
                    break;
                case (short)ServerPacketIds.UseItem:
                    UseItem((S.UseItem)p);
                    break;
                case (short)ServerPacketIds.DropItem:
                    DropItem((S.DropItem)p);
                    break;
                case (short)ServerPacketIds.PlayerUpdate:
                    PlayerUpdate((S.PlayerUpdate)p);
                    break;
                case (short)ServerPacketIds.PlayerInspect:
                    PlayerInspect((S.PlayerInspect)p);
                    break;
                case (short)ServerPacketIds.LogOutSuccess:
                    LogOutSuccess((S.LogOutSuccess)p);
                    break;
                case (short)ServerPacketIds.LogOutFailed:
                    LogOutFailed((S.LogOutFailed)p);
                    break;
                case (short)ServerPacketIds.TimeOfDay:
                    TimeOfDay((S.TimeOfDay)p);
                    break;
                case (short)ServerPacketIds.ChangeAMode:
                    ChangeAMode((S.ChangeAMode)p);
                    break;
                case (short)ServerPacketIds.ChangePMode:
                    ChangePMode((S.ChangePMode)p);
                    break;
                case (short)ServerPacketIds.ObjectItem:
                    ObjectItem((S.ObjectItem)p);
                    break;
                case (short)ServerPacketIds.ObjectGold:
                    ObjectGold((S.ObjectGold)p);
                    break;
                case (short)ServerPacketIds.GainedItem:
                    GainedItem((S.GainedItem)p);
                    break;
                case (short)ServerPacketIds.GainedGold:
                    GainedGold((S.GainedGold)p);
                    break;
                case (short)ServerPacketIds.LoseGold:
                    LoseGold((S.LoseGold)p);
                    break;
                case (short)ServerPacketIds.GainedCredit:
                    GainedCredit((S.GainedCredit)p);
                    break;
                case (short)ServerPacketIds.LoseCredit:
                    LoseCredit((S.LoseCredit)p);
                    break;
                case (short)ServerPacketIds.ObjectMonster:
                    ObjectMonster((S.ObjectMonster)p);
                    break;
                case (short)ServerPacketIds.ObjectAttack:
                    ObjectAttack((S.ObjectAttack)p);
                    break;
                case (short)ServerPacketIds.Struck:
                    Struck((S.Struck)p);
                    break;
                case (short)ServerPacketIds.DamageIndicator:
                    DamageIndicator((S.DamageIndicator)p);
                    break;
                case (short)ServerPacketIds.ObjectStruck:
                    ObjectStruck((S.ObjectStruck)p);
                    break;
                case (short)ServerPacketIds.DuraChanged:
                    DuraChanged((S.DuraChanged)p);
                    break;
                case (short)ServerPacketIds.HealthChanged:
                    HealthChanged((S.HealthChanged)p);
                    break;
                case (short)ServerPacketIds.DeleteItem:
                    DeleteItem((S.DeleteItem)p);
                    break;
                case (short)ServerPacketIds.Death:
                    Death((S.Death)p);
                    break;
                case (short)ServerPacketIds.ObjectDied:
                    ObjectDied((S.ObjectDied)p);
                    break;
                case (short)ServerPacketIds.ColourChanged:
                    ColourChanged((S.ColourChanged)p);
                    break;
                case (short)ServerPacketIds.ObjectColourChanged:
                    ObjectColourChanged((S.ObjectColourChanged)p);
                    break;
                case (short)ServerPacketIds.ObjectGuildNameChanged:
                    ObjectGuildNameChanged((S.ObjectGuildNameChanged)p);
                    break;
                case (short)ServerPacketIds.GainExperience:
                    GainExperience((S.GainExperience)p);
                    break;
                case (short)ServerPacketIds.LevelChanged:
                    LevelChanged((S.LevelChanged)p);
                    break;
                case (short)ServerPacketIds.ObjectLeveled:
                    ObjectLeveled((S.ObjectLeveled)p);
                    break;
                case (short)ServerPacketIds.ObjectHarvest:
                    ObjectHarvest((S.ObjectHarvest)p);
                    break;
                case (short)ServerPacketIds.ObjectHarvested:
                    ObjectHarvested((S.ObjectHarvested)p);
                    break;
                case (short)ServerPacketIds.ObjectNpc:
                    ObjectNPC((S.ObjectNPC)p);
                    break;
                case (short)ServerPacketIds.NPCResponse:
                    NPCResponse((S.NPCResponse)p);
                    break;
                case (short)ServerPacketIds.ObjectHide:
                    ObjectHide((S.ObjectHide)p);
                    break;
                case (short)ServerPacketIds.ObjectShow:
                    ObjectShow((S.ObjectShow)p);
                    break;
                case (short)ServerPacketIds.Poisoned:
                    Poisoned((S.Poisoned)p);
                    break;
                case (short)ServerPacketIds.ObjectPoisoned:
                    ObjectPoisoned((S.ObjectPoisoned)p);
                    break;
                case (short)ServerPacketIds.MapChanged:
                    MapChanged((S.MapChanged)p);
                    break;
                case (short)ServerPacketIds.ObjectTeleportOut:
                    ObjectTeleportOut((S.ObjectTeleportOut)p);
                    break;
                case (short)ServerPacketIds.ObjectTeleportIn:
                    ObjectTeleportIn((S.ObjectTeleportIn)p);
                    break;
                case (short)ServerPacketIds.TeleportIn:
                    TeleportIn();
                    break;
                case (short)ServerPacketIds.NPCGoods:
                    NPCGoods((S.NPCGoods)p);
                    break;
                case (short)ServerPacketIds.NPCSell:
                    NPCSell();
                    break;
                case (short)ServerPacketIds.NPCRepair:
                    NPCRepair((S.NPCRepair)p);
                    break;
                case (short)ServerPacketIds.NPCSRepair:
                    NPCSRepair((S.NPCSRepair)p);
                    break;
                case (short)ServerPacketIds.NPCRefine:
                    NPCRefine((S.NPCRefine)p);
                    break;
                case (short)ServerPacketIds.NPCCheckRefine:
                    NPCCheckRefine((S.NPCCheckRefine)p);
                    break;
                case (short)ServerPacketIds.NPCCollectRefine:
                    NPCCollectRefine((S.NPCCollectRefine)p);
                    break;
                case (short)ServerPacketIds.NPCReplaceWedRing:
                    NPCReplaceWedRing((S.NPCReplaceWedRing)p);
                    break;
                case (short)ServerPacketIds.NPCStorage:
                    NPCStorage();
                    break;
                case (short)ServerPacketIds.NPCRequestInput:
                    NPCRequestInput((S.NPCRequestInput)p);
                    break;
                case (short)ServerPacketIds.SellItem:
                    SellItem((S.SellItem)p);
                    break;
                case (short)ServerPacketIds.CraftItem:
                    CraftItem((S.CraftItem)p);
                    break;
                case (short)ServerPacketIds.RepairItem:
                    RepairItem((S.RepairItem)p);
                    break;
                case (short)ServerPacketIds.ItemRepaired:
                    ItemRepaired((S.ItemRepaired)p);
                    break;
                case (short)ServerPacketIds.NewMagic:
                    NewMagic((S.NewMagic)p);
                    break;
                case (short)ServerPacketIds.MagicLeveled:
                    MagicLeveled((S.MagicLeveled)p);
                    break;
                case (short)ServerPacketIds.Magic:
                    Magic((S.Magic)p);
                    break;
                case (short)ServerPacketIds.MagicDelay:
                    MagicDelay((S.MagicDelay)p);
                    break;
                case (short)ServerPacketIds.MagicCast:
                    MagicCast((S.MagicCast)p);
                    break;
                case (short)ServerPacketIds.ObjectMagic:
                    ObjectMagic((S.ObjectMagic)p);
                    break;
                case (short)ServerPacketIds.ObjectEffect:
                    ObjectEffect((S.ObjectEffect)p);
                    break;
                case (short)ServerPacketIds.RangeAttack:
                    RangeAttack((S.RangeAttack)p);
                    break;
                case (short)ServerPacketIds.Pushed:
                    Pushed((S.Pushed)p);
                    break;
                case (short)ServerPacketIds.ObjectPushed:
                    ObjectPushed((S.ObjectPushed)p);
                    break;
                case (short)ServerPacketIds.ObjectName:
                    ObjectName((S.ObjectName)p);
                    break;
                case (short)ServerPacketIds.UserStorage:
                    UserStorage((S.UserStorage)p);
                    break;
                case (short)ServerPacketIds.SwitchGroup:
                    SwitchGroup((S.SwitchGroup)p);
                    break;
                case (short)ServerPacketIds.DeleteGroup:
                    DeleteGroup();
                    break;
                case (short)ServerPacketIds.DeleteMember:
                    DeleteMember((S.DeleteMember)p);
                    break;
                case (short)ServerPacketIds.GroupInvite:
                    GroupInvite((S.GroupInvite)p);
                    break;
                case (short)ServerPacketIds.AddMember:
                    AddMember((S.AddMember)p);
                    break;
                case (short)ServerPacketIds.Revived:
                    Revived();
                    break;
                case (short)ServerPacketIds.ObjectRevived:
                    ObjectRevived((S.ObjectRevived)p);
                    break;
                case (short)ServerPacketIds.SpellToggle:
                    SpellToggle((S.SpellToggle)p);
                    break;
                case (short)ServerPacketIds.ObjectHealth:
                    ObjectHealth((S.ObjectHealth)p);
                    break;
                case (short)ServerPacketIds.MapEffect:
                    MapEffect((S.MapEffect)p);
                    break;
                case (short)ServerPacketIds.ObjectRangeAttack:
                    ObjectRangeAttack((S.ObjectRangeAttack)p);
                    break;
                case (short)ServerPacketIds.AddBuff:
                    AddBuff((S.AddBuff)p);
                    break;
                case (short)ServerPacketIds.RemoveBuff:
                    RemoveBuff((S.RemoveBuff)p);
                    break;
                case (short)ServerPacketIds.ObjectHidden:
                    ObjectHidden((S.ObjectHidden)p);
                    break;
                case (short)ServerPacketIds.RefreshItem:
                    RefreshItem((S.RefreshItem)p);
                    break;
                case (short)ServerPacketIds.ObjectSpell:
                    ObjectSpell((S.ObjectSpell)p);
                    break;
                case (short)ServerPacketIds.UserDash:
                    UserDash((S.UserDash)p);
                    break;
                case (short)ServerPacketIds.ObjectDash:
                    ObjectDash((S.ObjectDash)p);
                    break;
                case (short)ServerPacketIds.UserDashFail:
                    UserDashFail((S.UserDashFail)p);
                    break;
                case (short)ServerPacketIds.ObjectDashFail:
                    ObjectDashFail((S.ObjectDashFail)p);
                    break;
                case (short)ServerPacketIds.NPCConsign:
                    NPCConsign();
                    break;
                case (short)ServerPacketIds.NPCMarket:
                    NPCMarket((S.NPCMarket)p);
                    break;
                case (short)ServerPacketIds.NPCMarketPage:
                    NPCMarketPage((S.NPCMarketPage)p);
                    break;
                case (short)ServerPacketIds.ConsignItem:
                    ConsignItem((S.ConsignItem)p);
                    break;
                case (short)ServerPacketIds.MarketFail:
                    MarketFail((S.MarketFail)p);
                    break;
                case (short)ServerPacketIds.MarketSuccess:
                    MarketSuccess((S.MarketSuccess)p);
                    break;
                case (short)ServerPacketIds.ObjectSitDown:
                    ObjectSitDown((S.ObjectSitDown)p);
                    break;
                case (short)ServerPacketIds.InTrapRock:
                    S.InTrapRock packetdata = (S.InTrapRock)p;
                    User.InTrapRock = packetdata.Trapped;
                    break;
                case (short)ServerPacketIds.RemoveMagic:
                    RemoveMagic((S.RemoveMagic)p);
                    break;
                case (short)ServerPacketIds.BaseStatsInfo:
                    BaseStatsInfo((S.BaseStatsInfo)p);
                    break;
                case (short)ServerPacketIds.UserName:
                    UserName((S.UserName)p);
                    break;
                case (short)ServerPacketIds.ChatItemStats:
                    ChatItemStats((S.ChatItemStats)p);
                    break;
                case (short)ServerPacketIds.GuildInvite:
                    GuildInvite((S.GuildInvite)p);
                    break;
                case (short)ServerPacketIds.GuildMemberChange:
                    GuildMemberChange((S.GuildMemberChange)p);
                    break;
                case (short)ServerPacketIds.GuildNoticeChange:
                    GuildNoticeChange((S.GuildNoticeChange)p);
                    break;
                case (short)ServerPacketIds.GuildStatus:
                    GuildStatus((S.GuildStatus)p);
                    break;
                case (short)ServerPacketIds.GuildExpGain:
                    GuildExpGain((S.GuildExpGain)p);
                    break;
                case (short)ServerPacketIds.GuildNameRequest:
                    GuildNameRequest((S.GuildNameRequest)p);
                    break;
                case (short)ServerPacketIds.GuildStorageGoldChange:
                    GuildStorageGoldChange((S.GuildStorageGoldChange)p);
                    break;
                case (short)ServerPacketIds.GuildStorageItemChange:
                    GuildStorageItemChange((S.GuildStorageItemChange)p);
                    break;
                case (short)ServerPacketIds.GuildStorageList:
                    GuildStorageList((S.GuildStorageList)p);
                    break;
                case (short)ServerPacketIds.GuildRequestWar:
                    GuildRequestWar((S.GuildRequestWar)p);
                    break;
                case (short)ServerPacketIds.DefaultNPC:
                    DefaultNPC((S.DefaultNPC)p);
                    break;
                case (short)ServerPacketIds.NPCUpdate:
                    NPCUpdate((S.NPCUpdate)p);
                    break;
                case (short)ServerPacketIds.NPCImageUpdate:
                    NPCImageUpdate((S.NPCImageUpdate)p);
                    break;
                case (short)ServerPacketIds.MarriageRequest:
                    MarriageRequest((S.MarriageRequest)p);
                    break;
                case (short)ServerPacketIds.DivorceRequest:
                    DivorceRequest((S.DivorceRequest)p);
                    break;
                case (short)ServerPacketIds.MentorRequest:
                    MentorRequest((S.MentorRequest)p);
                    break;
                case (short)ServerPacketIds.TradeRequest:
                    TradeRequest((S.TradeRequest)p);
                    break;
                case (short)ServerPacketIds.TradeAccept:
                    TradeAccept((S.TradeAccept)p);
                    break;
                case (short)ServerPacketIds.TradeGold:
                    TradeGold((S.TradeGold)p);
                    break;
                case (short)ServerPacketIds.TradeItem:
                    TradeItem((S.TradeItem)p);
                    break;
                case (short)ServerPacketIds.TradeConfirm:
                    TradeConfirm();
                    break;
                case (short)ServerPacketIds.TradeCancel:
                    TradeCancel((S.TradeCancel)p);
                    break;
                case (short)ServerPacketIds.MountUpdate:
                    MountUpdate((S.MountUpdate)p);
                    break;
                case (short)ServerPacketIds.TransformUpdate:
                    TransformUpdate((S.TransformUpdate)p);
                    break;
                case (short)ServerPacketIds.EquipSlotItem:
                    EquipSlotItem((S.EquipSlotItem)p);
                    break;
                case (short)ServerPacketIds.FishingUpdate:
                    FishingUpdate((S.FishingUpdate)p);
                    break;
                case (short)ServerPacketIds.ChangeQuest:
                    ChangeQuest((S.ChangeQuest)p);
                    break;
                case (short)ServerPacketIds.CompleteQuest:
                    CompleteQuest((S.CompleteQuest)p);
                    break;
                case (short)ServerPacketIds.ShareQuest:
                    ShareQuest((S.ShareQuest)p);
                    break;
                case (short)ServerPacketIds.GainedQuestItem:
                    GainedQuestItem((S.GainedQuestItem)p);
                    break;
                case (short)ServerPacketIds.DeleteQuestItem:
                    DeleteQuestItem((S.DeleteQuestItem)p);
                    break;
                case (short)ServerPacketIds.CancelReincarnation:
                    User.ReincarnationStopTime = 0;
                    break;
                case (short)ServerPacketIds.RequestReincarnation:
                    if (!User.Dead) return;
                    RequestReincarnation();
                    break;
                case (short)ServerPacketIds.UserBackStep:
                    UserBackStep((S.UserBackStep)p);
                    break;
                case (short)ServerPacketIds.ObjectBackStep:
                    ObjectBackStep((S.ObjectBackStep)p);
                    break;
                case (short)ServerPacketIds.UserDashAttack:
                    UserDashAttack((S.UserDashAttack)p);
                    break;
                case (short)ServerPacketIds.ObjectDashAttack:
                    ObjectDashAttack((S.ObjectDashAttack)p);
                    break;
                case (short)ServerPacketIds.UserAttackMove://Warrior Skill - SlashingBurst
                    UserAttackMove((S.UserAttackMove)p);
                    break;
                case (short)ServerPacketIds.CombineItem:
                    CombineItem((S.CombineItem)p);
                    break;
                case (short)ServerPacketIds.ItemUpgraded:
                    ItemUpgraded((S.ItemUpgraded)p);
                    break;
                case (short)ServerPacketIds.SetConcentration:
                    SetConcentration((S.SetConcentration)p);
                    break;
                case (short)ServerPacketIds.SetObjectConcentration:
                    SetObjectConcentration((S.SetObjectConcentration)p);
                    break;
                case (short)ServerPacketIds.SetElemental:
                    SetElemental((S.SetElemental)p);
                    break;
                case (short)ServerPacketIds.SetObjectElemental:
                    SetObjectElemental((S.SetObjectElemental)p);
                    break;
                case (short)ServerPacketIds.RemoveDelayedExplosion:
                    RemoveDelayedExplosion((S.RemoveDelayedExplosion)p);
                    break;
                case (short)ServerPacketIds.ObjectDeco:
                    ObjectDeco((S.ObjectDeco)p);
                    break;
                case (short)ServerPacketIds.ObjectSneaking:
                    ObjectSneaking((S.ObjectSneaking)p);
                    break;
                case (short)ServerPacketIds.ObjectLevelEffects:
                    ObjectLevelEffects((S.ObjectLevelEffects)p);
                    break;
                case (short)ServerPacketIds.SetBindingShot:
                    SetBindingShot((S.SetBindingShot)p);
                    break;
                case (short)ServerPacketIds.SendOutputMessage:
                    SendOutputMessage((S.SendOutputMessage)p);
                    break;
                case (short)ServerPacketIds.NPCAwakening:
                    NPCAwakening();
                    break;
                case (short)ServerPacketIds.NPCDisassemble:
                    NPCDisassemble();
                    break;
                case (short)ServerPacketIds.NPCDowngrade:
                    NPCDowngrade();
                    break;
                case (short)ServerPacketIds.NPCReset:
                    NPCReset();
                    break;
                case (short)ServerPacketIds.AwakeningNeedMaterials:
                    AwakeningNeedMaterials((S.AwakeningNeedMaterials)p);
                    break;
                case (short)ServerPacketIds.AwakeningLockedItem:
                    AwakeningLockedItem((S.AwakeningLockedItem)p);
                    break;
                case (short)ServerPacketIds.Awakening:
                    Awakening((S.Awakening)p);
                    break;
                case (short)ServerPacketIds.ReceiveMail:
                    ReceiveMail((S.ReceiveMail)p);
                    break;
                case (short)ServerPacketIds.MailLockedItem:
                    MailLockedItem((S.MailLockedItem)p);
                    break;
                case (short)ServerPacketIds.MailSent:
                    MailSent((S.MailSent)p);
                    break;
                case (short)ServerPacketIds.MailSendRequest:
                    MailSendRequest((S.MailSendRequest)p);
                    break;
                case (short)ServerPacketIds.ParcelCollected:
                    ParcelCollected((S.ParcelCollected)p);
                    break;
                case (short)ServerPacketIds.MailCost:
                    MailCost((S.MailCost)p);
                    break;
                case (short)ServerPacketIds.ResizeInventory:
                    ResizeInventory((S.ResizeInventory)p);
                    break;
                case (short)ServerPacketIds.ResizeStorage:
                    ResizeStorage((S.ResizeStorage)p);
                    break;
                case (short)ServerPacketIds.NewIntelligentCreature:
                    NewIntelligentCreature((S.NewIntelligentCreature)p);
                    break;
                case (short)ServerPacketIds.UpdateIntelligentCreatureList:
                    UpdateIntelligentCreatureList((S.UpdateIntelligentCreatureList)p);
                    break;
                case (short)ServerPacketIds.IntelligentCreatureEnableRename:
                    IntelligentCreatureEnableRename((S.IntelligentCreatureEnableRename)p);
                    break;
                case (short)ServerPacketIds.IntelligentCreaturePickup:
                    IntelligentCreaturePickup((S.IntelligentCreaturePickup)p);
                    break;
                case (short)ServerPacketIds.NPCPearlGoods:
                    NPCPearlGoods((S.NPCPearlGoods)p);
                    break;
                case (short)ServerPacketIds.FriendUpdate:
                    FriendUpdate((S.FriendUpdate)p);
                    break;
                case (short)ServerPacketIds.LoverUpdate:
                    LoverUpdate((S.LoverUpdate)p);
                    break;
                case (short)ServerPacketIds.MentorUpdate:
                    MentorUpdate((S.MentorUpdate)p);
                    break;
                case (short)ServerPacketIds.GuildBuffList:
                    GuildBuffList((S.GuildBuffList)p);
                    break;
                case (short)ServerPacketIds.GameShopInfo:
                    GameShopUpdate((S.GameShopInfo)p);
                    break;
                case (short)ServerPacketIds.GameShopStock:
                    GameShopStock((S.GameShopStock)p);
                    break;
                case (short)ServerPacketIds.Rankings:
                    Rankings((S.Rankings)p);
                    break;
                case (short)ServerPacketIds.Opendoor:
                    Opendoor((S.Opendoor)p);
                    break;
                case (short)ServerPacketIds.GetRentedItems:
                    RentedItems((S.GetRentedItems) p);
                    break;
                case (short)ServerPacketIds.ItemRentalRequest:
                    ItemRentalRequest((S.ItemRentalRequest)p);
                    break;
                case (short)ServerPacketIds.ItemRentalFee:
                    ItemRentalFee((S.ItemRentalFee)p);
                    break;
                case (short)ServerPacketIds.ItemRentalPeriod:
                    ItemRentalPeriod((S.ItemRentalPeriod)p);
                    break;
                case (short)ServerPacketIds.DepositRentalItem:
                    DepositRentalItem((S.DepositRentalItem)p);
                    break;
                case (short)ServerPacketIds.RetrieveRentalItem:
                    RetrieveRentalItem((S.RetrieveRentalItem)p);
                    break;
                case (short)ServerPacketIds.UpdateRentalItem:
                    UpdateRentalItem((S.UpdateRentalItem)p);
                    break;
                case (short)ServerPacketIds.CancelItemRental:
                    CancelItemRental((S.CancelItemRental)p);
                    break;
                case (short)ServerPacketIds.ItemRentalLock:
                    ItemRentalLock((S.ItemRentalLock)p);
                    break;
                case (short)ServerPacketIds.ItemRentalPartnerLock:
                    ItemRentalPartnerLock((S.ItemRentalPartnerLock)p);
                    break;
                case (short)ServerPacketIds.CanConfirmItemRental:
                    CanConfirmItemRental((S.CanConfirmItemRental)p);
                    break;
                case (short)ServerPacketIds.ConfirmItemRental:
                    ConfirmItemRental((S.ConfirmItemRental)p);
                    break;
                case (short)ServerPacketIds.RearrangeInventory:
                    RearrangeInventory((S.RearrangeInventory)p);
                    break;
                case (short)ServerPacketIds.EEOpenSocketDialog:
                    if (!SocketingDialog.Visible)
                        SocketingDialog.Show();
                    break;
                case (short)ServerPacketIds.EERuneSocketResult:
                    SocketingDialog.DisplayResult((S.AddRuneResult)p);
                    break;
                case (short)ServerPacketIds.EERemoveRune:
                    SocketingDialog.RemoveRune((S.RemoveRune)p);
                    break;
                case (short)ServerPacketIds.ChangeNameRequest:
                    ChangeNameRequest((S.ChangeNameRequest)p);
                    break;
                default:
                    base.ProcessPacket(p);
                    break;
            }
        }

        private void RearrangeInventory(S.RearrangeInventory p)
        {
            User.Inventory = p.Inventory;
            for (int i = 0; i < User.Inventory.Length; i++)
            {
                if (User.Inventory[i] == null) continue;
                GameScene.Bind(User.Inventory[i]);
            }

            InventoryDialog.RefreshInventory();
        }

        public void DisposeItemLabel()
        {
            if (ItemLabelPanel != null)
            {
                ItemLabelPanel.DisposeItemLabel();
                ItemLabelPanel = null;
            }
        }

        private void KeepAlive(S.KeepAlive p)
        {
            if (p.Time == 0) return;
            PingTime = (CMain.Time - p.Time);


        }
        private void MapInformation(S.MapInformation p)
        {
            if (MapControl != null && !MapControl.IsDisposed)
                MapControl.Dispose();
            MapControl = new MapControl { FileName = Path.Combine(Settings.MapPath, p.FileName + ".map"), Title = p.Title, MiniMap = p.MiniMap, BigMap = p.BigMap, Lights = p.Lights, Lightning = p.Lightning, Fire = p.Fire, MapDarkLight = p.MapDarkLight, Music = p.Music };
            MapControl.LoadMap();
            InsertControl(0, MapControl);
        }
        private void UserInformation(S.UserInformation p)
        {
            User = new UserObject(p.ObjectID);
            User.Load(p);
            MainDialog.PModeLabel.Visible = User.Class == MirClass.Wizard || User.Class == MirClass.Taoist;
            Gold = p.Gold;
            Credit = p.Credit;

            InventoryDialog.RefreshInventory();
            UpdateSkillBar();

            AssistHelper.Init();
        }
        private void UserLocation(S.UserLocation p)
        {
            MapControl.NextAction = 0;
            if (User.CurrentLocation == p.Location && User.Direction == p.Direction)
            {
                if (Settings.AutoPick && CMain.Time > GameScene.PickUpTime)
                {
                    GameScene.PickUpTime = CMain.Time + 200;

                    CellInfo cInfo = GameScene.Scene.MapControl.M2CellInfo[User.CurrentLocation.X, User.CurrentLocation.Y];
                    if (cInfo.CellObjects != null)
                    {
                        for (int c = 0; c < cInfo.CellObjects.Count; c++)
                        {
                            ItemObject ob = cInfo.CellObjects[c] as ItemObject;
                            if (ob == null) continue;

                            if (!AssistHelper.NeedPick(ob.Name))
                                continue;

                            Network.Enqueue(new C.PickUp());
                        }
                    }
                }
                return;
            }

            if (Settings.DebugMode)
            {
                CMain.SaveError("UserLocation");
            }

            MapControl.RemoveObject(User);
            User.CurrentLocation = p.Location;
            User.MapLocation = p.Location;
            MapControl.AddObject(User);

            MapControl.FloorValid = false;
            MapControl.InputDelay = CMain.Time + 400;

            if (User.Dead) return;

            User.ClearMagic();
            User.QueuedAction = null;

            for (int i = User.ActionFeed.Count - 1; i >= 0; i--)
            {
                if (User.ActionFeed[i].Action == MirAction.Pushed) continue;
                User.ActionFeed.RemoveAt(i);
            }

            User.SetAction();
        }
        private void ReceiveChat(S.Chat p)
        {
            ChatDialog.ReceiveChat(p.Message, p.Type);
        }
        private void ObjectPlayer(S.ObjectPlayer p)
        {
            PlayerObject player = new PlayerObject(p.ObjectID);
            player.Load(p);
        }
        private void ObjectRemove(S.ObjectRemove p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Remove();
            }
        }
        private void ObjectTurn(S.ObjectTurn p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Standing, Direction = p.Direction, Location = p.Location });
                return;
            }
        }
        private void ObjectWalk(S.ObjectWalk p)
        {
            if (p.ObjectID == User.ObjectID)
            {
                return;
            }

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Walking, Direction = p.Direction, Location = p.Location });
                return;
            }
        }
        private void ObjectRun(S.ObjectRun p)
        {
            if (p.ObjectID == User.ObjectID)
            {
                return;
            }

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Running, Direction = p.Direction, Location = p.Location });
                return;
            }
        }
        private void ObjectChat(S.ObjectChat p)
        {
            ChatDialog.ReceiveChat(p.Text, p.Type);

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Chat(p.Text);
                return;
            }

        }
        private void MoveItem(S.MoveItem p)
        {
            MirItemCell toCell, fromCell;

            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];
                    break;
                case MirGridType.Storage:
                    fromCell = StorageDialog.Grid[p.From];
                    break;
                case MirGridType.Trade:
                    fromCell = TradeDialog.Grid[p.From];
                    break;
                case MirGridType.Refine:
                    fromCell = RefineDialog.Grid[p.From];
                    break;
                default:
                    return;
            }

            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];
                    break;
                case MirGridType.Storage:
                    toCell = StorageDialog.Grid[p.To];
                    break;
                case MirGridType.Trade:
                    toCell = TradeDialog.Grid[p.To];
                    break;
                case MirGridType.Refine:
                    toCell = RefineDialog.Grid[p.To];
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (p.Grid == MirGridType.Trade)
                TradeDialog.ChangeLockState(false);

            if (!p.Success) return;

            UserItem i = fromCell.Item;
            fromCell.Item = toCell.Item;
            toCell.Item = i;

            User.RefreshStats();
            CharacterDuraPanel.GetCharacterDura();
        }
        private void EquipItem(S.EquipItem p)
        {
            MirItemCell fromCell;

            MirItemCell toCell = CharacterDialog.Grid[p.To];

            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    fromCell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);
                    break;
                case MirGridType.Storage:
                    fromCell = StorageDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;

            UserItem i = fromCell.Item;
            fromCell.Item = toCell.Item;
            toCell.Item = i;
            CharacterDuraPanel.UpdateCharacterDura(i);
            User.RefreshStats();
        }
        private void EquipSlotItem(S.EquipSlotItem p)
        {
            MirItemCell fromCell;
            MirItemCell toCell;

            switch (p.GridTo)
            {
                case MirGridType.Mount:
                    toCell = MountDialog.Grid[p.To];
                    break;
                case MirGridType.Fishing:
                    toCell = FishingDialog.Grid[p.To];
                    break;
                default:
                    return;
            }

            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    fromCell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);
                    break;
                case MirGridType.Storage:
                    fromCell = StorageDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);
                    break;
                default:
                    return;
            }

            //if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;

            UserItem i = fromCell.Item;
            fromCell.Item = null;
            toCell.Item = i;
            User.RefreshStats();
        }

        private void CombineItem(S.CombineItem p)
        {
            MirItemCell fromCell = InventoryDialog.GetCell(p.IDFrom) ?? BeltDialog.GetCell(p.IDFrom);
            MirItemCell toCell = InventoryDialog.GetCell(p.IDTo) ?? BeltDialog.GetCell(p.IDTo);

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (p.Destroy) toCell.Item = null;

            if (!p.Success) return;

            if (fromCell.Item.Count > 1 )
                fromCell.Item.Count--;
            else
                fromCell.Item = null;

            User.RefreshStats();

            if (toCell.Item != null)
                GameScene.Scene.InventoryDialog.DisplayItemGridEffect(toCell.Item.UniqueID, 1);
        }

        private void MergeItem(S.MergeItem p)
        {
            MirItemCell toCell, fromCell;

            switch (p.GridFrom)
            {
                case MirGridType.Inventory:
                    fromCell = InventoryDialog.GetCell(p.IDFrom) ?? BeltDialog.GetCell(p.IDFrom);
                    break;
                case MirGridType.Storage:
                    fromCell = StorageDialog.GetCell(p.IDFrom);
                    break;
                case MirGridType.Equipment:
                    fromCell = CharacterDialog.GetCell(p.IDFrom);
                    break;
                case MirGridType.Trade:
                    fromCell = TradeDialog.GetCell(p.IDFrom);
                    break;
                case MirGridType.Fishing:
                    fromCell = FishingDialog.GetCell(p.IDFrom);
                    break;
                default:
                    return;
            }

            switch (p.GridTo)
            {
                case MirGridType.Inventory:
                    toCell = InventoryDialog.GetCell(p.IDTo) ?? BeltDialog.GetCell(p.IDTo);
                    break;
                case MirGridType.Storage:
                    toCell = StorageDialog.GetCell(p.IDTo);
                    break;
                case MirGridType.Equipment:
                    toCell = CharacterDialog.GetCell(p.IDTo);
                    break;
                case MirGridType.Trade:
                    toCell = TradeDialog.GetCell(p.IDTo);
                    break;
                case MirGridType.Fishing:
                    toCell = FishingDialog.GetCell(p.IDTo);
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (p.GridFrom == MirGridType.Trade || p.GridTo == MirGridType.Trade)
                TradeDialog.ChangeLockState(false);

            if (!p.Success) return;
            if (fromCell.Item.Count <= toCell.Item.Info.StackSize - toCell.Item.Count)
            {
                toCell.Item.Count += fromCell.Item.Count;
                fromCell.Item = null;
            }
            else
            {
                fromCell.Item.Count -= toCell.Item.Info.StackSize - toCell.Item.Count;
                toCell.Item.Count = toCell.Item.Info.StackSize;
            }

            User.RefreshStats();
        }
        private void RemoveItem(S.RemoveItem p)
        {
            MirItemCell toCell;

            int index = -1;

            for (int i = 0; i < MapObject.User.Equipment.Length; i++)
            {
                if (MapObject.User.Equipment[i] == null || MapObject.User.Equipment[i].UniqueID != p.UniqueID) continue;
                index = i;
                break;
            }

            MirItemCell fromCell = CharacterDialog.Grid[index];


            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];
                    break;
                case MirGridType.Storage:
                    toCell = StorageDialog.Grid[p.To];
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            CharacterDuraPanel.GetCharacterDura();
            User.RefreshStats();
        }
        private void RemoveSlotItem(S.RemoveSlotItem p)
        {
            MirItemCell fromCell;
            MirItemCell toCell;

            int index = -1;

            switch (p.Grid)
            {
                case MirGridType.Mount:
                    fromCell = MountDialog.GetCell(p.UniqueID);
                    break;
                case MirGridType.Fishing:
                    fromCell = FishingDialog.GetCell(p.UniqueID);
                    break;
                default:
                    return;
            }

            switch (p.GridTo)
            {
                case MirGridType.Inventory:
                    toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];
                    break;
                case MirGridType.Storage:
                    toCell = StorageDialog.Grid[p.To];
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            CharacterDuraPanel.GetCharacterDura();
            User.RefreshStats();
        }
        private void TakeBackItem(S.TakeBackItem p)
        {
            MirItemCell fromCell = StorageDialog.Grid[p.From];

            MirItemCell toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
            CharacterDuraPanel.GetCharacterDura();
        }
        private void StoreItem(S.StoreItem p)
        {
            MirItemCell fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];

            MirItemCell toCell = StorageDialog.Grid[p.To];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }
        private void DepositRefineItem(S.DepositRefineItem p)
        {
            MirItemCell fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];

            MirItemCell toCell = RefineDialog.Grid[p.To];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }

        private void RetrieveRefineItem(S.RetrieveRefineItem p)
        {
            MirItemCell fromCell = RefineDialog.Grid[p.From];
            MirItemCell toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }

        private void RefineCancel(S.RefineCancel p)
        {
            RefineDialog.RefineReset();  
        }

        private void RefineItem(S.RefineItem p)
        {
            RefineDialog.RefineReset();
            for (int i = 0; i < User.Inventory.Length; i++)
            {
                if (User.Inventory[i] != null && User.Inventory[i].UniqueID == p.UniqueID)
                {
                    User.Inventory[i] = null;
                    break;
                }
            }
            NPCDialog.Hide();
        }


        private void DepositTradeItem(S.DepositTradeItem p)
        {
            MirItemCell fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];

            MirItemCell toCell = TradeDialog.Grid[p.To];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;
            TradeDialog.ChangeLockState(false);

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }
        private void RetrieveTradeItem(S.RetrieveTradeItem p)
        {
            MirItemCell fromCell = TradeDialog.Grid[p.From];
            MirItemCell toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];

            if (toCell == null || fromCell == null) return;

            toCell.Locked = false;
            fromCell.Locked = false;
            TradeDialog.ChangeLockState(false);

            if (!p.Success) return;
            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }
        private void SplitItem(S.SplitItem p)
        {
            Bind(p.Item);

            UserItem[] array;
            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    array = MapObject.User.Inventory;
                    break;
                case MirGridType.Storage:
                    array = Storage;
                    break;
                default:
                    return;
            }

            if (p.Grid == MirGridType.Inventory && (p.Item.Info.Type == ItemType.Potion || p.Item.Info.Type == ItemType.Scroll || p.Item.Info.Type == ItemType.Amulet || (p.Item.Info.Type == ItemType.Script && p.Item.Info.Effect == 1)))
            {
                if (p.Item.Info.Type == ItemType.Potion || p.Item.Info.Type == ItemType.Scroll || (p.Item.Info.Type == ItemType.Script && p.Item.Info.Effect == 1))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (array[i] != null) continue;
                        array[i] = p.Item;
                        User.RefreshStats();
                        return;
                    }
                }
                else if (p.Item.Info.Type == ItemType.Amulet)
                {
                    for (int i = 4; i < GameScene.User.BeltIdx; i++)
                    {
                        if (array[i] != null) continue;
                        array[i] = p.Item;
                        User.RefreshStats();
                        return;
                    }
                }
            }

            for (int i = GameScene.User.BeltIdx; i < array.Length; i++)
            {
                if (array[i] != null) continue;
                array[i] = p.Item;
                User.RefreshStats();
                return;
            }

            for (int i = 0; i < GameScene.User.BeltIdx; i++)
            {
                if (array[i] != null) continue;
                array[i] = p.Item;
                User.RefreshStats();
                return;
            }
        }

        private void SplitItem1(S.SplitItem1 p)
        {
            MirItemCell cell;

            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);
                    break;
                case MirGridType.Storage:
                    cell = StorageDialog.GetCell(p.UniqueID);
                    break;
                default:
                    return;
            }

            if (cell == null) return;

            cell.Locked = false;

            if (!p.Success) return;
            cell.Item.Count -= p.Count;
            User.RefreshStats();
        }
        private void UseItem(S.UseItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);

            if (cell == null) return;

            cell.Locked = false;

            if (!p.Success) return;
            if (cell.Item.Count > 1) cell.Item.Count--;
            else cell.Item = null;
            User.RefreshStats();
        }
        private void DropItem(S.DropItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);

            if (cell == null) return;

            cell.Locked = false;

            if (!p.Success) return;

            if (p.Count == cell.Item.Count)
                cell.Item = null;
            else
                cell.Item.Count -= p.Count;

            User.RefreshStats();
        }


        private void MountUpdate(S.MountUpdate p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                if (MapControl.Objects[i].ObjectID != p.ObjectID) continue;

                PlayerObject player = MapControl.Objects[i] as PlayerObject;
                if (player != null)
                {
                    player.MountUpdate(p);
                }
                break;
            }

            if (p.ObjectID != User.ObjectID) return;

            CanRun = false;

            User.RefreshStats();

            GameScene.Scene.MountDialog.RefreshDialog();
            GameScene.Scene.Redraw();
        }

        private void TransformUpdate(S.TransformUpdate p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                if (MapControl.Objects[i].ObjectID != p.ObjectID) continue;

                PlayerObject player = MapControl.Objects[i] as PlayerObject;
                if (player != null)
                {
                    player.TransformType = p.TransformType;
                    player.SetLibraries();
                }
                break;
            }
        }

        private void FishingUpdate(S.FishingUpdate p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                if (MapControl.Objects[i].ObjectID != p.ObjectID) continue;

                PlayerObject player = MapControl.Objects[i] as PlayerObject;
                if (player != null)
                {
                    player.FishingUpdate(p);
                }
                break;
            }

            if (p.ObjectID != User.ObjectID) return;

            GameScene.Scene.FishingStatusDialog.ProgressPercent = p.ProgressPercent;
            GameScene.Scene.FishingStatusDialog.ChancePercent = p.ChancePercent;

            GameScene.Scene.FishingStatusDialog.ChanceLabel.Text = string.Format("{0}%", GameScene.Scene.FishingStatusDialog.ChancePercent);

            if (p.Fishing)
                GameScene.Scene.FishingStatusDialog.Show();
            else
                GameScene.Scene.FishingStatusDialog.Hide();

            Redraw();
        }

        private void CompleteQuest(S.CompleteQuest p)
        {
            if (User == null)
            {
                CMain.SaveError("User is null");
                return;
            }

            User.CompletedQuests = p.CompletedQuests;
        }

        private void ShareQuest(S.ShareQuest p)
        {
            ClientQuestInfo quest = GameScene.QuestInfoList.FirstOrDefault(e => e.Index == p.QuestIndex);
            
            if (quest == null) return;

            MirMessageBox messageBox = new MirMessageBox(string.Format("{0} would like to share a quest with you. Do you accept?", p.SharerName), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.AcceptQuest { NPCIndex = 0, QuestIndex = quest.Index });

            messageBox.Show();
        }

        private void ChangeQuest(S.ChangeQuest p)
        {
            switch(p.QuestState)
            {
                case QuestState.Add:
                    User.CurrentQuests.Add(p.Quest);
                    BindQuest(p.Quest);

                    if (Settings.TrackedQuests.Contains(p.Quest.Id))
                    {
                        GameScene.Scene.QuestTrackingDialog.AddQuest(p.Quest, true);
                    }
                    else if (p.TrackQuest)
                    {
                        GameScene.Scene.QuestTrackingDialog.AddQuest(p.Quest);
                    }


                    break;
                case QuestState.Update:
                    for (int i = 0; i < User.CurrentQuests.Count; i++)
                    {
                        if (User.CurrentQuests[i].Id != p.Quest.Id) continue;

                        User.CurrentQuests[i] = p.Quest;
                    }

                    foreach (ClientQuestProgress quest in User.CurrentQuests)
                        BindQuest(quest);

                    GameScene.Scene.QuestTrackingDialog.DisplayQuests();
                    break;
                case QuestState.Remove:

                    for (int i = User.CurrentQuests.Count - 1; i >= 0; i--)
                    {
                        if (User.CurrentQuests[i].Id != p.Quest.Id) continue;

                        User.CurrentQuests.RemoveAt(i);
                    }

                    GameScene.Scene.QuestTrackingDialog.RemoveQuest(p.Quest);

                    break;
            }

            if (Scene.QuestListDialog.Visible)
            {
                Scene.QuestListDialog.DisplayInfo();
            }

            if (Scene.QuestLogDialog.Visible)
            {
                Scene.QuestLogDialog.DisplayQuests();
            }
        }

        private void PlayerUpdate(S.PlayerUpdate p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                if (MapControl.Objects[i].ObjectID != p.ObjectID) continue;

                PlayerObject player = MapControl.Objects[i] as PlayerObject;
                if (player != null) player.Update(p);
                return;
            }
        }
        private void PlayerInspect(S.PlayerInspect p)
        {
            InspectDialog.Equipment = p.Equipment;
            for (int i = 0; i < InspectDialog.Equipment.Length; i++)
            {
                UserItem temp = InspectDialog.Equipment[i];
                if (temp != null)
                    Bind(temp);
            }

            InspectDialog.Name = p.Name;
            InspectDialog.GuildName = p.GuildName;
            InspectDialog.GuildRank = p.GuildRank;
            InspectDialog.Class = p.Class;
            InspectDialog.Gender = p.Gender;
            InspectDialog.Hair = p.Hair;
            InspectDialog.Level = p.Level;
            InspectDialog.LoverName = p.LoverName;

            InspectDialog.RefreshEquipmentStat();
            InspectDialog.RefreshInferface();
            InspectDialog.Show();
        }
        private void LogOutSuccess(S.LogOutSuccess p)
        {
            for (int i = 0; i <= 3; i++)//Fix for orbs sound
                SoundManager.StopSound(20000 + 126 * 10 + 5 + i);

            User = null;
            if (Settings.Resolution != 800)
                CMain.SetResolution(800, 600);
            ActiveScene = new SelectScene(p.Characters);

            Dispose();
        }
        private void LogOutFailed(S.LogOutFailed p)
        {
            Enabled = true;
            ChatDialog.ReceiveChatTr("Not allow login out.", ChatType.System);
        }

        private void TimeOfDay(S.TimeOfDay p)
        {
            Lights = p.Lights;
            switch (Lights)
            {
                case LightSetting.Day:
                case LightSetting.Normal:
                    MiniMapDialog.LightSetting.Index = 2093;
                    break;
                case LightSetting.Dawn:
                    MiniMapDialog.LightSetting.Index = 2095;
                    break;
                case LightSetting.Evening:
                    MiniMapDialog.LightSetting.Index = 2094;
                    break;
                case LightSetting.Night:
                    MiniMapDialog.LightSetting.Index = 2092;
                    break;
            }
        }
        private void ChangeAMode(S.ChangeAMode p)
        {
            AMode = p.Mode;

            switch (p.Mode)
            {
                case AttackMode.Peace:
                    ChatDialog.ReceiveChatTr("[Attack Mode: Peaceful]", ChatType.Hint);
                    break;
                case AttackMode.Group:
                    ChatDialog.ReceiveChatTr("[Attack Mode: Group]", ChatType.Hint);
                    break;
                case AttackMode.Guild:
                    ChatDialog.ReceiveChatTr("[Attack Mode: Guild]", ChatType.Hint);
                    break;
                case AttackMode.EnemyGuild:
                    ChatDialog.ReceiveChatTr("[Attack Mode: Enemy Guild]", ChatType.Hint);
                    break;
                case AttackMode.RedBrown:
                    ChatDialog.ReceiveChatTr("[Attack Mode: Red+Brown]", ChatType.Hint);
                    break;
                case AttackMode.All:
                    ChatDialog.ReceiveChatTr("[Attack Mode: All]", ChatType.Hint);
                    break;
            }

            MainDialog.OnAModeChanged();
        }

        private void ChangePMode(S.ChangePMode p)
        {
            PMode = p.Mode;
            switch (p.Mode)
            {
                case PetMode.Both:
                    ChatDialog.ReceiveChatTr("[Pet Mode: Attack and Move]", ChatType.Hint);
                    break;
                case PetMode.MoveOnly:
                    ChatDialog.ReceiveChatTr("[Pet Mode: Do Not Attack]", ChatType.Hint);
                    break;
                case PetMode.AttackOnly:
                    ChatDialog.ReceiveChatTr("[Pet Mode: Do Not Move]", ChatType.Hint);
                    break;
                case PetMode.None:
                    ChatDialog.ReceiveChatTr("[Pet Mode: Do Not Attack or Move]", ChatType.Hint);
                    break;
            }

            MainDialog.PModeLabel.Visible = true;
            MainDialog.OnPModeChanged();
        }

        private void ObjectItem(S.ObjectItem p)
        {
            ItemObject ob = new ItemObject(p.ObjectID);
            ob.Load(p);
            /*
            string[] Warnings = new string[] {"HeroNecklace","AdamantineNecklace","8TrigramWheel","HangMaWheel","BaekTaGlove","SpiritReformer","BokMaWheel","BoundlessRing","ThunderRing","TaeGukRing","OmaSpiritRing","NobleRing"};
            if (Warnings.Contains(p.Name))
            {
                ChatDialog.ReceiveChat(string.Format("{0} at {1}", p.Name, p.Location), ChatType.Hint);
            }
            */
        }
        private void ObjectGold(S.ObjectGold p)
        {
            ItemObject ob = new ItemObject(p.ObjectID);
            ob.Load(p);
        }
        private void GainedItem(S.GainedItem p)
        {
            Bind(p.Item);
            AddItem(p.Item);
            User.RefreshStats();

            OutputMessage(string.Format(CMain.Tr("You gained {0}."), p.Item.FriendlyName));
        }
        private void GainedQuestItem(S.GainedQuestItem p)
        {
            Bind(p.Item);
            AddQuestItem(p.Item);
        }

        private void GainedGold(S.GainedGold p)
        {
            if (p.Gold == 0) return;

            Gold += p.Gold;
            SoundManager.PlaySound(SoundList.Gold);
            OutputMessage(string.Format(CMain.Tr("You gained {0:###,###,###} Gold."), p.Gold));
        }
        private void LoseGold(S.LoseGold p)
        {
            Gold -= p.Gold;
            SoundManager.PlaySound(SoundList.Gold);
        }
        private void GainedCredit(S.GainedCredit p)
        {
            if (p.Credit == 0) return;

            Credit += p.Credit;
            SoundManager.PlaySound(SoundList.Gold);
            OutputMessage(string.Format(CMain.Tr("You gained {0:###,###,###} Credit."), p.Credit));
        }
        private void LoseCredit(S.LoseCredit p)
        {
            Credit -= p.Credit;
            SoundManager.PlaySound(SoundList.Gold);
        }
        private void ObjectMonster(S.ObjectMonster p)
        {
            MonsterObject mob;
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID == p.ObjectID)
                {
                    mob = (MonsterObject)ob;
                    mob.Load(p, true);
                    return;
                }
            }

            mob = MonsterObject.GetMonster(p.ObjectID, p.Image);
            mob.Load(p);
        }

        private void ObjectAttack(S.ObjectAttack p)
        {
            if (p.ObjectID == User.ObjectID) return;

            QueuedAction action = null;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                if (ob.Race == ObjectType.Player)
                {
                    action = new QueuedAction { Action = MirAction.Attack1, Direction = p.Direction, Location = p.Location, Params = new List<object>() }; //FAR Close up attack
                }
                else
                {
                    switch (p.Type)
                    {
                        default:
                            {
                                action = new QueuedAction { Action = MirAction.Attack1, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                                break;
                            }
                        case 1:
                            {
                                action = new QueuedAction { Action = MirAction.Attack2, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                                break;
                            }
                        case 2:
                            {
                                action = new QueuedAction { Action = MirAction.Attack3, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                                break;
                            }
                        case 3:
                            {
                                action = new QueuedAction { Action = MirAction.Attack4, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                                break;
                            }
                    }
                }
                action.Params.Add(p.Spell);
                action.Params.Add(p.Level);
                ob.ActionFeed.Add(action);
                return;
            }
        }
        private void Struck(S.Struck p)
        {
            LogTime = CMain.Time + Globals.LogDelay;

            NextRunTime = CMain.Time + 2500;
            User.CancelChannelling();

            User.ClearMagic();
            if (User.ReincarnationStopTime > CMain.Time)
                Network.Enqueue(new C.CancelReincarnation {});

            MirDirection dir = User.Direction;
            Point location = User.CurrentLocation;

            for (int i = 0; i < User.ActionFeed.Count; i++)
                if (User.ActionFeed[i].Action == MirAction.Struck) return;


            if (User.ActionFeed.Count > 0)
            {
                dir = User.ActionFeed[User.ActionFeed.Count - 1].Direction;
                location = User.ActionFeed[User.ActionFeed.Count - 1].Location;
            }

            if (User.Buffs.Any(a => a == BuffType.EnergyShield))
            {
                for (int j = 0; j < User.Effects.Count; j++)
                {
                    BuffEffect effect = null;
                    effect = User.Effects[j] as BuffEffect;

                    if (effect != null && effect.BuffType == BuffType.EnergyShield)
                    {
                        effect.Clear();
                        effect.Remove();

                        User.Effects.Add(effect = new BuffEffect(Libraries.Magic2, 1890, 6, 600, User, true, BuffType.EnergyShield) { Repeat = false });
                        SoundManager.PlaySound(20000 + (ushort)Spell.EnergyShield * 10 + 1);
                        
                        effect.Complete += (o, e) =>
                        {
                            User.Effects.Add(new BuffEffect(Libraries.Magic2, 1900, 2, 800, User, true, BuffType.EnergyShield) { Repeat = true });
                        };


                        break;
                    }
                }
            }

            QueuedAction action = new QueuedAction { Action = MirAction.Struck, Direction = dir, Location = location, Params = new List<object>() };
            action.Params.Add(p.AttackerID);
            User.ActionFeed.Add(action);
            User.StruckTime = CMain.Time + 500;

        }
        private void ObjectStruck(S.ObjectStruck p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                if (ob.SkipFrames) return;
                if (ob.ActionFeed.Count > 0 && ob.ActionFeed[ob.ActionFeed.Count - 1].Action == MirAction.Struck) return;

                if (ob.Race == ObjectType.Player)
                    ((PlayerObject)ob).CancelChannelling();
                QueuedAction action = new QueuedAction { Action = MirAction.Struck, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                action.Params.Add(p.AttackerID);
                ob.ActionFeed.Add(action);

                if (ob.Buffs.Any(a => a == BuffType.EnergyShield))
                {
                    for (int j = 0; j < ob.Effects.Count; j++)
                    {
                        BuffEffect effect = null;
                        effect = ob.Effects[j] as BuffEffect;

                        if (effect != null && effect.BuffType == BuffType.EnergyShield)
                        {
                            effect.Clear();
                            effect.Remove();

                            ob.Effects.Add(effect = new BuffEffect(Libraries.Magic2, 1890, 6, 600, ob, true, BuffType.EnergyShield) { Repeat = false });
                            SoundManager.PlaySound(20000 + (ushort)Spell.EnergyShield * 10 + 1);

                            effect.Complete += (o, e) =>
                            {
                                ob.Effects.Add(new BuffEffect(Libraries.Magic2, 1900, 2, 800, ob, true, BuffType.EnergyShield) { Repeat = true });
                            };

                            break;
                        }
                    }
                }

                return;
            }
        }

        private void DamageIndicator(S.DamageIndicator p)
        {
            if (!Settings.ShowDamage)
                return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject obj = MapControl.Objects[i];
                if (obj.ObjectID != p.ObjectID) continue;

                switch (p.Type)
                {
                    case DamageType.Hit: //add damage level colours
                        obj.Damages.Add(new Damage(p.Damage.ToString("#,##0"), 1000, obj.Race == ObjectType.Player ? Color.Red : Color.White, 50));
                        break;
                    case DamageType.Miss:
                        obj.Damages.Add(new Damage(CMain.Tr("Miss"), 1200, obj.Race == ObjectType.Player ? Color.LightCoral : Color.LightGray, 50));
                        break;
                    case DamageType.Critical:
                        obj.Damages.Add(new Damage(p.Damage.ToString("#,##0"), 1000, obj.Race == ObjectType.Player ? Color.DarkRed : Color.Yellow, 50, 10F) { Offset = 15 });
                        break;
                    case DamageType.Invalid:
                        obj.Damages.Add(new Damage(CMain.Tr("Invalid"), 1000, obj.Race == ObjectType.Player ? Color.LightCoral : Color.LightGray, 50) { Offset = 15 });
                        break;
                    case DamageType.Heal:
                        if (Settings.ShowHeal)
                            obj.Damages.Add(new Damage("+" + p.Damage.ToString("#,##0"), 1000, obj.Race == ObjectType.Player ? Color.Lime : Color.Lime, 50));
                        break;
                }
            }
        }

        private void DuraChanged(S.DuraChanged p)
        {
            UserItem item = null;
            for (int i = 0; i < User.Inventory.Length; i++)
                if (User.Inventory[i] != null && User.Inventory[i].UniqueID == p.UniqueID)
                {
                    item = User.Inventory[i];
                    break;
                }

            if (item == null)
                for (int i = 0; i < User.Equipment.Length; i++)
                {
                    if (User.Equipment[i] != null && User.Equipment[i].UniqueID == p.UniqueID)
                    {
                        item = User.Equipment[i];
                        break;
                    }
                    if (User.Equipment[i] != null && User.Equipment[i].Slots != null)
                    {
                        for (int j = 0; j < User.Equipment[i].Slots.Length; j++)
                        {
                            if (User.Equipment[i].Slots[j] != null && User.Equipment[i].Slots[j].UniqueID == p.UniqueID)
                            {
                                item = User.Equipment[i].Slots[j];
                                break;
                            }
                        }

                        if (item != null) break;
                    }
                }

            if (item == null) return;

            item.CurrentDura = p.CurrentDura;

            if (item.CurrentDura == 0)
            {
                User.RefreshStats();
                switch (item.Info.Type)
                {
                    case ItemType.Mount:
                        ChatDialog.ReceiveChat(CMain.Format("{0} is no longer loyal to you.", item.Info.Name), ChatType.System);
                        break;
                    default:
                        ChatDialog.ReceiveChat(CMain.Format("{0}'s dura has dropped to 0.", item.Info.Name), ChatType.System);
                        break;
                }
                
            }

            if (HoverItem == item)
            {
                DisposeItemLabel();
                CreateItemLabel(item);
            }

            CharacterDuraPanel.UpdateCharacterDura(item);
        }
        private void HealthChanged(S.HealthChanged p)
        {
            User.HP = p.HP;
            User.MP = p.MP;

            User.PercentHealth = (byte)(User.HP / (float)User.MaxHP * 100);
        }

        private void DeleteQuestItem(S.DeleteQuestItem p)
        {
            for (int i = 0; i < User.Inventory.Length; i++)
            {
                UserItem item = User.Inventory[i];

                if (item == null || item.UniqueID != p.UniqueID) continue;

                if (item.Count == p.Count)
                    User.Inventory[i] = null;
                else
                    item.Count -= p.Count;
                break;
            } 
        }

        private void DeleteItem(S.DeleteItem p)
        {
            for (int i = 0; i < User.Inventory.Length; i++)
            {
                UserItem item = User.Inventory[i];

                if (item == null || item.UniqueID != p.UniqueID) continue;

                if (item.Count == p.Count)
                    User.Inventory[i] = null;
                else
                    item.Count -= p.Count;
                break;
            }

            for (int i = 0; i < User.Equipment.Length; i++)
            {
                UserItem item = User.Equipment[i];

                if (item != null && item.Slots.Length > 0)
                {
                    for (int j = 0; j < item.Slots.Length; j++)
                    {
                        UserItem slotItem = item.Slots[j];

                        if (slotItem == null || slotItem.UniqueID != p.UniqueID) continue;

                        if (slotItem.Count == p.Count)
                            item.Slots[j] = null;
                        else
                            slotItem.Count -= p.Count;
                        break;
                    }
                }

                if (item == null || item.UniqueID != p.UniqueID) continue;

                if (item.Count == p.Count)
                    User.Equipment[i] = null;
                else
                    item.Count -= p.Count;
                break;
            }

            for (int i = 0; i < Storage.Length; i++)
            {
                var item = Storage[i];
                if (item == null || item.UniqueID != p.UniqueID) continue;


                if (item.Count == p.Count)
                    User.Inventory[i] = null;
                else
                    item.Count -= p.Count;
                break;
            }

            User.RefreshStats();
        }
        private void Death(S.Death p)
        {
            User.Dead = true;

            User.ActionFeed.Add(new QueuedAction { Action = MirAction.Die, Direction = p.Direction, Location = p.Location });
            ShowReviveMessage = true;

            LogTime = 0;
        }
        private void ObjectDied(S.ObjectDied p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                switch(p.Type)
                {
                    default:
                        ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Die, Direction = p.Direction, Location = p.Location });
                        ob.Dead = true;
                        break;
                    case 1:
                        MapControl.Effects.Add(new Effect(Libraries.Magic2, 690, 10, 1000, ob.CurrentLocation));
                        ob.Remove();
                        break;
                    case 2:
                        SoundManager.PlaySound(20000 + (ushort)Spell.DarkBody * 10 + 1);
                        MapControl.Effects.Add(new Effect(Libraries.Magic2, 2600, 10, 1200, ob.CurrentLocation));
                        ob.Remove();
                        break;
                }
                return;
            }
        }
        private void ColourChanged(S.ColourChanged p)
        {
            User.NameColour = p.NameColour;
        }
        private void ObjectColourChanged(S.ObjectColourChanged p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.NameColour = p.NameColour;
                return;
            }
        }

        private void ObjectGuildNameChanged(S.ObjectGuildNameChanged p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                PlayerObject obPlayer = (PlayerObject)ob;
                obPlayer.GuildName = p.GuildName;
                return;
            }
        }
        private void GainExperience(S.GainExperience p)
        {
            OutputMessage(string.Format(CMain.Tr("Experience Gained {0}."), p.Amount));
            MapObject.User.Experience += p.Amount;
        }
        private void LevelChanged(S.LevelChanged p)
        {
            User.Level = p.Level;
            User.Experience = p.Experience;
            User.MaxExperience = p.MaxExperience;
            User.RefreshStats();
            OutputMessage(CMain.Tr("Level Increased!"));
            User.Effects.Add(new Effect(Libraries.Magic2, 1200, 20, 2000, User));
            SoundManager.PlaySound(SoundList.LevelUp);
            ChatDialog.ReceiveChat(CMain.Tr("Congratulations! You have leveled up. Your HP and MP have been restored."), ChatType.LevelUp); 
        }
        private void ObjectLeveled(S.ObjectLeveled p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Effects.Add(new Effect(Libraries.Magic2, 1180, 16, 2500, ob));
                SoundManager.PlaySound(SoundList.LevelUp);
                return;
            }
        }
        private void ObjectHarvest(S.ObjectHarvest p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Harvest, Direction = ob.Direction, Location = ob.CurrentLocation });
                return;
            }
        }
        private void ObjectHarvested(S.ObjectHarvested p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Skeleton, Direction = ob.Direction, Location = ob.CurrentLocation });
                return;
            }
        }
        private void ObjectNPC(S.ObjectNPC p)
        {
            NPCObject ob = new NPCObject(p.ObjectID);
            ob.Load(p);
        }
        private void NPCResponse(S.NPCResponse p)
        {
            NPCTime = 0;
            NPCDialog.NewText(p.Page);

            if (p.Page.Count > 0)
                NPCDialog.Show();
            else
                NPCDialog.Hide();

            NPCGoodsDialog.Hide();
            // BuyBackDialog.Hide();
            NPCDropDialog.Hide();
            StorageDialog.Hide();
        }
        private void NPCUpdate(S.NPCUpdate p)
        {
            GameScene.NPCID = p.NPCID; //Updates the client with the correct NPC ID if it's manually called from the client
        }

        private void NPCImageUpdate(S.NPCImageUpdate p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID || ob.Race != ObjectType.Merchant) continue;

                NPCObject npc = (NPCObject)ob;
                npc.Image = p.Image;
                npc.Colour = p.Colour;

                npc.LoadLibrary();
                return;
            }
        }
        private void DefaultNPC(S.DefaultNPC p)
        {
            GameScene.DefaultNPCID = p.ObjectID; //Updates the client with the correct Default NPC ID
        }


        private void ObjectHide(S.ObjectHide p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Hide, Direction = ob.Direction, Location = ob.CurrentLocation });
                return;
            }
        }
        private void ObjectShow(S.ObjectShow p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Show, Direction = ob.Direction, Location = ob.CurrentLocation });
                return;
            }
        }
        private void Poisoned(S.Poisoned p)
        {
            User.Poison = p.Poison;
            if (p.Poison.HasFlag(PoisonType.Stun) || p.Poison.HasFlag(PoisonType.Frozen) || p.Poison.HasFlag(PoisonType.Paralysis) || p.Poison.HasFlag(PoisonType.LRParalysis))
            {
                    User.ClearMagic();
            }
        }
        private void ObjectPoisoned(S.ObjectPoisoned p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Poison = p.Poison;

                ob.PoisonChanged();
                return;
            }
        }
        private void MapChanged(S.MapChanged p)
        {
            MapControl.FileName = Path.Combine(Settings.MapPath, p.FileName + ".map");
            MapControl.Title = p.Title;
            MapControl.MiniMap = p.MiniMap;
            MapControl.BigMap = p.BigMap;
            MapControl.Lights = p.Lights;
            MapControl.MapDarkLight = p.MapDarkLight;
            MapControl.Music = p.Music;
            MapControl.LoadMap();
            MapControl.NextAction = 0;

            User.CurrentLocation = p.Location;
            User.MapLocation = p.Location;
            MapControl.AddObject(User);
            
            User.Direction = p.Direction;

            User.QueuedAction = null;
            User.ActionFeed.Clear();
            User.ClearMagic();
            User.SetAction();

            GameScene.CanRun = false;

            MapControl.FloorValid = false;
            MapControl.InputDelay = CMain.Time + 400;
        }
        private void ObjectTeleportOut(S.ObjectTeleportOut p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                Effect effect = null;

                bool playDefaultSound = true;

                switch (p.Type)
                {
                    case 1: //Yimoogi
                        {
                            effect = new Effect(Libraries.Magic2, 1300, 10, 500, ob);
                            break;
                        }
                    case 2: //RedFoxman
                        {
                            effect = new Effect(Libraries.Monsters[(ushort)Monster.RedFoxman], 243, 10, 500, ob);
                            break;
                        }
                    case 4: //MutatedManWorm
                        {
                            effect = new Effect(Libraries.Monsters[(ushort)Monster.MutatedManworm], 272, 6, 500, ob);

                            SoundManager.PlaySound(((ushort)Monster.MutatedManworm) * 10 + 7);
                            playDefaultSound = false;
                            break;
                        }
                    case 5: //WitchDoctor
                        {
                            effect = new Effect(Libraries.Monsters[(ushort)Monster.WitchDoctor], 328, 20, 1000, ob);
                            break;
                        }
                    case 6: //TurtleKing
                        {
                            effect = new Effect(Libraries.Monsters[(ushort)Monster.TurtleKing], 946, 10, 500, ob);
                            break;
                        }
                    case 7: //HornCommander
                        {
                           // effect = new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 928, 10, 500, ob);
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 928, 10, 1000, ob.CurrentLocation, 0, false));
                            break;
                        }
                    case 8: //HornCommander
                        {
                            // effect = new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 928, 10, 500, ob);
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 561, 10, 1000, ob.CurrentLocation, 0, false));
                            break;
                        }
                    default:
                        {
                            effect = new Effect(Libraries.Magic, 250, 10, 500, ob);
                            break;
                        }
                }

                if (effect != null)
                {
                    effect.Complete += (o, e) => ob.Remove();
                    ob.Effects.Add(effect);
                }

                if(playDefaultSound)
                    SoundManager.PlaySound(SoundList.Teleport);

                return;
            }
        }
        private void ObjectTeleportIn(S.ObjectTeleportIn p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                bool playDefaultSound = true;

                switch (p.Type)
                {
                    case 1: //Yimoogi
                        {
                            ob.Effects.Add(new Effect(Libraries.Magic2, 1310, 10, 500, ob));
                            break;
                        }
                    case 2: //RedFoxman
                        {
                            ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RedFoxman], 253, 10, 500, ob));
                            break;
                        }
                    case 4: //MutatedManWorm
                        {
                            ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.MutatedManworm], 278, 7, 500, ob));

                            SoundManager.PlaySound(((ushort)Monster.MutatedManworm) * 10 + 7);
                            playDefaultSound = false;
                            break;
                        }
                    case 5: //WitchDoctor
                        {
                            ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.WitchDoctor], 348, 20, 1000, ob));
                            break;
                        }
                    case 6: //TurtleKing
                        {
                            ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TurtleKing], 956, 10, 500, ob));
                            break;
                        }
                    case 7: //HornCommander
                        {
                            break;
                        }
                    case 8: //SnowWolfKing
                        {
                            // effect = new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 928, 10, 500, ob);
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 571, 10, 1000, ob.CurrentLocation, 0, false));
                            break;
                        }
                    default:
                        {
                            ob.Effects.Add(new Effect(Libraries.Magic, 260, 10, 500, ob));
                            break;
                        }
                }

                if(playDefaultSound)
                    SoundManager.PlaySound(SoundList.Teleport);

                return;
            }

            
        }
        private void TeleportIn()
        {
            User.Effects.Add(new Effect(Libraries.Magic, 260, 10, 500, User));
            SoundManager.PlaySound(SoundList.Teleport);
        }
        private void NPCGoods(S.NPCGoods p)
        {
            for (int i = 0; i < p.List.Count; i++)
            {
                p.List[i].Info = GetInfo(p.List[i].ItemIndex);
            }

            NPCRate = p.Rate;
            if (!NPCDialog.Visible) return;
            NPCGoodsDialog.usePearls = false;
            NPCGoodsDialog.NewGoods(p.List);
            NPCGoodsDialog.UpdatePanelType(p.Type);
            NPCGoodsDialog.Show();


        }
        private void NPCSell()
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Sell;
            NPCDropDialog.Show();
        }
        private void NPCRepair(S.NPCRepair p)
        {
            NPCRate = p.Rate;
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Repair;
            NPCDropDialog.Show();
        }
        private void NPCStorage()
        {
            if (NPCDialog.Visible)
                StorageDialog.Show();
        }
        private void NPCRequestInput(S.NPCRequestInput p)
        {
            MirInputBox inputBox = new MirInputBox("Please enter the required information.");

            inputBox.OKButton.Click += (o1, e1) =>
            {
                Network.Enqueue(new C.NPCConfirmInput { Value = inputBox.InputTextBox.Text, NPCID = p.NPCID, PageName = p.PageName });
                inputBox.Dispose();
            };
            inputBox.Show();
        }

        private void NPCSRepair(S.NPCSRepair p)
        {
            NPCRate = p.Rate;
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.SpecialRepair;
            NPCDropDialog.Show();
        }

        private void NPCRefine(S.NPCRefine p)
        {
            NPCRate = p.Rate;
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Refine;
            if (p.Refining)
            {
                NPCDropDialog.Hide();
                NPCDialog.Hide();
            }
            else
                NPCDropDialog.Show();
        }

        private void NPCCheckRefine(S.NPCCheckRefine p)
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.CheckRefine;
            NPCDropDialog.Show();
        }

        private void NPCCollectRefine(S.NPCCollectRefine p)
        {
            if (!NPCDialog.Visible) return;
            NPCDialog.Hide();
        }

        private void NPCReplaceWedRing(S.NPCReplaceWedRing p)
        {
            if (!NPCDialog.Visible) return;
            NPCRate = p.Rate;
            NPCDropDialog.PType = PanelType.ReplaceWedRing;
            NPCDropDialog.Show();
        }


        private void SellItem(S.SellItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);

            if (cell == null) return;

            cell.Locked = false;

            if (!p.Success) return;

            if (p.Count == cell.Item.Count)
                cell.Item = null;
            else
                cell.Item.Count -= p.Count;

            User.RefreshStats();
        }
        private void RepairItem(S.RepairItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);

            if (cell == null) return;

            cell.Locked = false;
        }
        private void CraftItem(S.CraftItem p)
        {
            if (!p.Success)
            {
                ChatDialog.ReceiveChatTr("Craft failed!", ChatType.System);
                return;
            }

            for (int i = 0; i < CraftDialog.Selected.Count; i++)
            {
                MirItemCell cell = CraftDialog.Selected[i].Key;
                ulong oldItem = CraftDialog.Selected[i].Value;

                if (cell.Item == null || cell.Item.UniqueID != oldItem)
                {
                    MirItemCell gridCell = CraftDialog.GetCell(oldItem);

                    if (gridCell != null)
                        gridCell.Item = null;
                    cell.Locked = false;
                }
            }

            CraftDialog.RefreshCraftCells(CraftDialog.RecipeItem);

            User.RefreshStats();
        }
        private void ItemRepaired(S.ItemRepaired p)
        {
            UserItem item = null;
            for (int i = 0; i < User.Inventory.Length; i++)
                if (User.Inventory[i] != null && User.Inventory[i].UniqueID == p.UniqueID)
                {
                    item = User.Inventory[i];
                    break;
                }

            if (item == null)
                for (int i = 0; i < User.Equipment.Length; i++)
                    if (User.Equipment[i] != null && User.Equipment[i].UniqueID == p.UniqueID)
                    {
                        item = User.Equipment[i];
                        break;
                    }

            if (item == null) return;

            item.MaxDura = p.MaxDura;
            item.CurrentDura = p.CurrentDura;

            if (HoverItem == item)
            {
                DisposeItemLabel();
                CreateItemLabel(item);
            }
        }

        private void ItemUpgraded(S.ItemUpgraded p)
        {
            UserItem item = null;
            for (int i = 0; i < User.Inventory.Length; i++)
                if (User.Inventory[i] != null && User.Inventory[i].UniqueID == p.Item.UniqueID)
                {
                    item = User.Inventory[i];
                    break;
                }

            if (item == null) return;

            item.DC = p.Item.DC;
            item.MC = p.Item.MC;
            item.SC = p.Item.SC;

            item.AC = p.Item.AC;
            item.MAC = p.Item.MAC;
            item.MaxDura = p.Item.MaxDura;

            item.AttackSpeed = p.Item.AttackSpeed;
            item.Agility = p.Item.Agility;
            item.Accuracy = p.Item.Accuracy;
            item.PoisonAttack = p.Item.PoisonAttack;
            item.Freezing = p.Item.Freezing;
            item.MagicResist = p.Item.MagicResist;
            item.PoisonResist = p.Item.PoisonResist;
            item.RefinedValue = p.Item.RefinedValue;
            item.RefineAdded = p.Item.RefineAdded;
            

            GameScene.Scene.InventoryDialog.DisplayItemGridEffect(item.UniqueID, 0);

            //MirAnimatedControl anim = new MirAnimatedControl
            //{
            //    Animated = true,
            //    AnimationCount = 9,
            //    DisplayLocation = GameScene.Scene.InventoryDialog
            //};

            if (HoverItem == item)
            {
                DisposeItemLabel();
                CreateItemLabel(item);
            }
        }

        private void NewMagic(S.NewMagic p)
        {
            ClientMagic magic = p.Magic;

            User.Magics.Add(magic);
            User.RefreshStats();
            foreach (SkillBarDialog Bar in SkillBarDialogs)
                Bar.Update();
        }

        private void RemoveMagic(S.RemoveMagic p)
        {
            User.Magics.RemoveAt(p.PlaceId);
            User.RefreshStats();
            foreach (SkillBarDialog Bar in SkillBarDialogs)
                Bar.Update();
        }

        private void MagicLeveled(S.MagicLeveled p)
        {
            for (int i = 0; i < User.Magics.Count; i++)
            {
                ClientMagic magic = User.Magics[i];
                if (magic.Spell != p.Spell) continue;

                if (magic.Level != p.Level)
                {
                    magic.Level = p.Level;
                    User.RefreshStats();
                }

                magic.Experience = p.Experience;
                break;
            }


        }
        private void Magic(S.Magic p)
        {
            User.Spell = p.Spell;
            User.Cast = p.Cast;
            User.TargetID = p.TargetID;
            User.TargetPoint = p.Target;
            User.SpellLevel = p.Level;

            if (!p.Cast) return;

            ClientMagic magic = User.GetMagic(p.Spell);
            magic.CastTime = CMain.Time;
        }

        private void MagicDelay(S.MagicDelay p)
        {
            ClientMagic magic = User.GetMagic(p.Spell);
            magic.Delay = p.Delay;
        }

        private void MagicCast(S.MagicCast p)
        {
            ClientMagic magic = User.GetMagic(p.Spell);
            magic.CastTime = CMain.Time;
        }

        private void ObjectMagic(S.ObjectMagic p)
        {
            if (p.SelfBroadcast == false && p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                QueuedAction action = new QueuedAction { Action = MirAction.Spell, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                action.Params.Add(p.Spell);
                action.Params.Add(p.TargetID);
                action.Params.Add(p.Target);
                action.Params.Add(p.Cast);
                action.Params.Add(p.Level);
                action.Params.Add(p.Param);

                ob.ActionFeed.Add(action);
                return;
            }

        }
        private void ObjectEffect(S.ObjectEffect p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                PlayerObject player;
                switch (p.Effect)
                {
                    case SpellEffect.FatalSword:
                        ob.Effects.Add(new Effect(Libraries.Magic2, 1940, 4, 400, ob));
                        SoundManager.PlaySound(20000 + (ushort)Spell.FatalSword * 10);
                        break;
                    case SpellEffect.StormEscape:
                        ob.Effects.Add(new Effect(Libraries.Magic3, 610, 10, 600, ob));
                        SoundManager.PlaySound(SoundList.Teleport);
                        break;
                    case SpellEffect.ShiBuYiSha:
                        ob.Effects.Add(new Effect(Libraries.Magic4, 438, 8, 600, ob) { Blend = true });
                        SoundManager.PlaySound(SoundList.Teleport);
                        break;
                    case SpellEffect.Teleport:
                        ob.Effects.Add(new Effect(Libraries.Magic, 1600, 10, 600, ob));
                        SoundManager.PlaySound(SoundList.Teleport);
                        break;
                    case SpellEffect.Healing:
                        SoundManager.PlaySound(20000 + (ushort)Spell.Healing * 10 + 1);
                        ob.Effects.Add(new Effect(Libraries.Magic, 370, 10, 800, ob));
                        break;
                    case SpellEffect.HealingCircle:
                        ob.Effects.Add(new Effect(Libraries.Magic3, 650, 10, 10 * 80, ob));
                        break;
                    case SpellEffect.HealingCircle2:
                        ob.Effects.Add(new Effect(Libraries.Magic3, 660, 10, 10 * 80, ob));
                        break;
                    case SpellEffect.RedMoonEvil:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RedMoonEvil], 32, 6, 400, ob) { Blend = false });
                        break;
                    case SpellEffect.TwinDrakeBlade:
                        ob.Effects.Add(new Effect(Libraries.Magic2, 380, 6, 800, ob));
                        break;
                   case SpellEffect.MPEater:
                        for (int j = MapControl.Objects.Count - 1; j >= 0; j--)
                        {
                            MapObject ob2 = MapControl.Objects[j];
                            if (ob2.ObjectID == p.EffectType)
                            {
                                ob2.Effects.Add(new Effect(Libraries.Magic2, 2411, 19, 1900, ob2));
                                break;
                            }
                        }
                        ob.Effects.Add(new Effect(Libraries.Magic2, 2400, 9, 900, ob));
                        SoundManager.PlaySound(20000 + (ushort)Spell.FatalSword * 10);
                        break;
                    case SpellEffect.Bleeding:
                        ob.Effects.Add(new Effect(Libraries.Magic3, 60, 3, 400, ob));
                        break;
                    case SpellEffect.Hemorrhage:
                        SoundManager.PlaySound(20000 + (ushort)Spell.Hemorrhage * 10);
                        ob.Effects.Add(new Effect(Libraries.Magic3, 0, 4, 400, ob));
                        ob.Effects.Add(new Effect(Libraries.Magic3, 28, 6, 600, ob));
                        ob.Effects.Add(new Effect(Libraries.Magic3, 46, 8, 800, ob));
                        break;
                    case SpellEffect.MagicShieldUp:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ShieldEffect != null)
                        {
                            player.ShieldEffect.Clear();
                            player.ShieldEffect.Remove();
                        }

                        player.MagicShield = true;
                        player.Effects.Add(player.ShieldEffect = new Effect(Libraries.Magic, 3890, 3, 600, ob) { Repeat = true });
                        break;
                    case SpellEffect.MagicShieldDown:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ShieldEffect != null)
                        {
                            player.ShieldEffect.Clear();
                            player.ShieldEffect.Remove();
                        }
                        player.ShieldEffect = null;
                        player.MagicShield = false;
                        break;
                    case SpellEffect.GreatFoxSpirit:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.GreatFoxSpirit], 375 + (CMain.Random.Next(3) * 20), 20, 1400, ob));
                        SoundManager.PlaySound(((ushort)Monster.GreatFoxSpirit * 10) + 5);
                        break;
                    case SpellEffect.Entrapment:
                        ob.Effects.Add(new Effect(Libraries.Magic2, 1010, 10, 1500, ob));
                        ob.Effects.Add(new Effect(Libraries.Magic2, 1020, 8, 1200, ob));
                        break;
                    case SpellEffect.Critical:
                        //ob.Effects.Add(new Effect(Libraries.CustomEffects, 0, 12, 60, ob));
                        break;
                    case SpellEffect.Reflect:
                        ob.Effects.Add(new Effect(Libraries.Magic, 3480, 10, 1500, ob));
                        ob.Effects.Add(new Effect(Libraries.Magic3, 140, 10, 1500, ob));
                        break;
                    case SpellEffect.ElementalBarrierUp:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ElementalBarrierEffect != null)
                        {
                            player.ElementalBarrierEffect.Clear();
                            player.ElementalBarrierEffect.Remove();
                        }

                        player.ElementalBarrier = true;
                        player.Effects.Add(player.ElementalBarrierEffect = new Effect(Libraries.Magic3, 1890, 10, 2000, ob) { Repeat = true });
                        break;
                    case SpellEffect.ElementalBarrierDown:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ElementalBarrierEffect != null)
                        {
                            player.ElementalBarrierEffect.Clear();
                            player.ElementalBarrierEffect.Remove();
                        }
                        player.ElementalBarrierEffect = null;
                        player.ElementalBarrier = false;
                        player.Effects.Add(player.ElementalBarrierEffect = new Effect(Libraries.Magic3, 1910, 7, 1400, ob));
                        SoundManager.PlaySound(20000 + 131 * 10 + 5);
                        break;

                    case SpellEffect.MoreaShield1Up:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 468, 6, 900, ob));
                        break;
                    case SpellEffect.MoreaShield1Down:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 474, 3, 450, ob));
                        break;
                    case SpellEffect.MoreaShield2Up:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 477, 7, 1050, ob));
                        break;
                    case SpellEffect.MoreaShield2Down:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 484, 4, 600, ob));
                        break;
                    case SpellEffect.MoreaShield3Up:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 488, 6, 900, ob));
                        break;
                    case SpellEffect.MoreaShield3Down:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedArcher], 494, 3, 450, ob));
                        break;
                    case SpellEffect.Bear:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Bear], 312, 9, 900, ob));
                        break;
                    case SpellEffect.ElementalBarrierUp1:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ElementalBarrierEffect1 != null)
                        {
                            player.ElementalBarrierEffect1.Clear();
                            player.ElementalBarrierEffect1.Remove();
                        }

                        player.ElementalBarrier1 = true;
                        player.Effects.Add(player.ElementalBarrierEffect1 = new Effect(Libraries.Magic3, 1890, 10, 2000, ob) { Repeat = true });
                        break;
                    case SpellEffect.ElementalBarrierDown1:
                        if (ob.Race != ObjectType.Player) return;
                        player = (PlayerObject)ob;
                        if (player.ElementalBarrierEffect1 != null)
                        {
                            player.ElementalBarrierEffect1.Clear();
                            player.ElementalBarrierEffect1.Remove();
                        }
                        player.ElementalBarrierEffect1 = null;
                        player.ElementalBarrier1 = false;
                        player.Effects.Add(player.ElementalBarrierEffect1 = new Effect(Libraries.Magic3, 1910, 7, 1400, ob));
                        SoundManager.PlaySound(20000 + 131 * 10 + 5);
                        break;
                    case SpellEffect.MoonMist:
                        MapControl.Effects.Add(new Effect(Libraries.Magic3, 680, 34, 34*80, ob.CurrentLocation, 0, true));
                        break;
                    case SpellEffect.DelayedExplosion:
                        int effectid = DelayedExplosionEffect.GetOwnerEffectID(ob.ObjectID);
                        if (effectid < 0)
                        {
                            ob.Effects.Add(new DelayedExplosionEffect(Libraries.Magic3, 1590, 8, 1200, ob, true, 0, 0));
                        }
                        else if (effectid >= 0)
                        {
                            if (DelayedExplosionEffect.effectlist[effectid].stage < p.EffectType)
                            {
                                DelayedExplosionEffect.effectlist[effectid].Remove();
                                ob.Effects.Add(new DelayedExplosionEffect(Libraries.Magic3, 1590 + ((int)p.EffectType * 10), 8, 1200, ob, true, (int)p.EffectType, 0));
                            }
                        }

                        //else
                        //    ob.Effects.Add(new DelayedExplosionEffect(Libraries.Magic3, 1590 + ((int)p.EffectType * 10), 8, 1200, ob, true, (int)p.EffectType, 0));
                        break;

                    case SpellEffect.AwakeningSuccess:
                        {
                            Effect ef = new Effect(Libraries.Magic3, 900, 16, 1600, ob, CMain.Time + p.DelayTime);
                            ef.Played += (o, e) => SoundManager.PlaySound(50002);
                            ef.Complete += (o, e) => MapControl.AwakeningAction = false;
                            ob.Effects.Add(ef);
                            ob.Effects.Add(new Effect(Libraries.Magic3, 840, 16, 1600, ob, CMain.Time + p.DelayTime) { Blend = false });
                        }
                        break;
                    case SpellEffect.AwakeningFail:
                        {
                            Effect ef = new Effect(Libraries.Magic3, 920, 9, 900, ob, CMain.Time + p.DelayTime);
                            ef.Played += (o, e) => SoundManager.PlaySound(50003);
                            ef.Complete += (o, e) => MapControl.AwakeningAction = false;
                            ob.Effects.Add(ef);
                            ob.Effects.Add(new Effect(Libraries.Magic3, 860, 9, 900, ob, CMain.Time + p.DelayTime) { Blend = false });
                        }
                        break;
                    case SpellEffect.AwakeningHit:
                        {
                            Effect ef = new Effect(Libraries.Magic3, 880, 5, 500, ob, CMain.Time + p.DelayTime);
                            ef.Played += (o, e) => SoundManager.PlaySound(50001);
                            ob.Effects.Add(ef);
                            ob.Effects.Add(new Effect(Libraries.Magic3, 820, 5, 500, ob, CMain.Time + p.DelayTime) { Blend = false });
                        }
                        break;
                    case SpellEffect.AwakeningMiss:
                        {
                            Effect ef = new Effect(Libraries.Magic3, 890, 5, 500, ob, CMain.Time + p.DelayTime);
                            ef.Played += (o, e) => SoundManager.PlaySound(50000);
                            ob.Effects.Add(ef);
                            ob.Effects.Add(new Effect(Libraries.Magic3, 830, 5, 500, ob, CMain.Time + p.DelayTime) { Blend = false });
                        }
                        break;
                    case SpellEffect.TurtleKing:
                        {
                            Effect ef = new Effect(Libraries.Monsters[(ushort)Monster.TurtleKing], CMain.Random.Next(2) == 0 ? 922 : 934, 12, 1200, ob);
                            ef.Played += (o, e) => SoundManager.PlaySound(20000 + (ushort)Spell.HellFire * 10 + 1);
                            ob.Effects.Add(ef);
                        }
                        break;
                    case SpellEffect.Behemoth:
                        {
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Behemoth], 788, 10, 1500, ob.CurrentLocation));
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Behemoth], 778, 10, 1500, ob.CurrentLocation, 0, true) { Blend = false });
                        }
                        break;
                    case SpellEffect.Stunned:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StoningStatue], 632, 10, 1000, ob)
                        {
                            Repeat = p.Time > 0,
                            RepeatUntil = p.Time > 0 ? CMain.Time + p.Time : 0
                        });
                        break;
                    case SpellEffect.IcePillar:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IcePillar], 18, 8, 800, ob));
                        break;

                    case SpellEffect.TianLeiZhen:
                        ob.Effects.Add(new Effect(Libraries.Magic4, 429, 8, 1200, ob, 0, true));
                        break;

                    case SpellEffect.CatBigBossThunder:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.GeneralJinmYo], 512, 9, 900, ob, 0, true));
                                    break;

                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.GeneralJinmYo], 522, 7, 700, ob, 0, true));
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.CatBigBossShield:
                        {
                            CatBigBoss boss = ((CatBigBoss)ob);
                            switch (p.EffectType)
                            {
                                case 0:
                                    boss.ShieldEffect = new Effect(Libraries.Monsters[(ushort)Monster.GeneralJinmYo], 529, 7, 900, ob, 0, true);
                                    boss.ShieldEffect.Repeat = true;
                                    boss.ShieldEffect.Rate = 0.6F;
                                    ob.Effects.Add(boss.ShieldEffect);
                                    break;

                                case 1:
                                    if (boss.ShieldEffect != null)
                                    {
                                        boss.ShieldEffect.Clear();
                                        boss.ShieldEffect.Remove();
                                    }
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.TreeQueen:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 120, 15, 1500, ob, 0) { Blend = false });
                        break;

                    case SpellEffect.KillerPlant:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KillerPlant], 1224, 10, 1000, ob, 0, true));
                        break;

                    case SpellEffect.HornedCommander:
                        switch(p.EffectType)
                        {
                            case 0:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1224, 10, 1000, ob, 0, true));
                                break;
                        }
                        break;

                    case SpellEffect.SnowWolfKing:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 571, 10, 1600, ob, CMain.Time + p.DelayTime, true));
                                    break;

                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 605, 10, 1600, ob, CMain.Time + p.DelayTime, true));
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.CreeperPlant:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CreeperPlant], 250, 6, 6 * 150, ob, 0, true));
                                    break;

                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CreeperPlant], 256, 10, 1000, ob, 0, true));
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.AvengerPlant:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.AvengerPlant], 248, 6, 900, ob, 0, true));
                        break;

                     case SpellEffect.ClawBeast:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ClawBeast], 568, 7, 1050, ob, 0, true));
                        break;

                     case SpellEffect.Hydrax:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Hydrax], 240, 5, 900, ob, 0, true));
                        break;

                    //110
                    case SpellEffect.HornedMage:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Basiloid], 0, 9, 9 * 150, ob, CMain.Time + 400, false));
                                    break;
                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Basiloid], 9, 6, 900, ob, CMain.Time + 800, false));
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.FloatingRock:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FloatingRock], 226, 10, 1500, ob, 0, true));
                        break;

                    case SpellEffect.FrozenAxeman:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenAxeman], 552, 6, 900, ob, 0, true));
                                    break;
                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenAxeman], 582, 6, 900, ob, 0, true));
                                    break;
                                case 2:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenAxeman], 612, 6, 900, ob, 0, true));
                                    break;
                            }
                        }
                        break;

                    case SpellEffect.FrozenMagician:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenMagician], 872, 6, 900, ob, 0, true));
                        break;
                    // 112
                    case SpellEffect.FrozenKnight:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenKnight], 456, 10, 1500, ob, 0, true));
                        break;

                                  // 176
                       case SpellEffect.IcePhantom:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IcePhantom], 682, 8, 1200, ob, 0, true));
                        break;

                                         // 190
                       case SpellEffect.BlackTortoise:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.BlackTortoise], 398, 6, 1050, ob, 0, true));
                        break;

                                                  // 190
                    //   case SpellEffect.BlackTortoise:
                  //      ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.BlackTortoise], 398, 6, 1050, ob, 0, true));
                  //      break;

                      // 191
                       case SpellEffect.DragonWarrior:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonWarrior], 529, 7, 1050, ob, 0, true));
                        break;

                          // 192
                       case SpellEffect.DragonArcher:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonArcher], 664, 6, 900, ob, 0, true));
                        break;

                              // 171
                       case SpellEffect.FrozenMiner:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenMiner], 456, 6, 900, ob, 0, true));
                        break;

                                     // 172
                    //   case SpellEffect.FrozenMiner:
                    //    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenMiner], 502, 6, 900, ob, 0, true));
                   //     break;
                   
                                        // 179
                    //   case SpellEffect.FrozenMiner:
                    //    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IceCrystalSoldier], 464, 6, 900, ob, 0, true));
                    //    break;

                                        // 180
                       case SpellEffect.IceCrystalSoldier:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IceCrystalSoldier], 476, 8, 1200, ob, 0, true));
                        break;

                        // 181
                       case SpellEffect.DarkWraith:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 744, 6, 900, ob, 0, true));
                        break;

                    // 182
                    // case SpellEffect.DarkWraith:
                    //   ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 790, 6, 900, ob, 0, true));
                    //   break;

                    // 183
                    //   case SpellEffect.DarkWraith:
                    //   ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 836, 6, 900, ob, 0, true));
                    //   break;

                    case SpellEffect.SnowWarrior:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWarrior], 400, 7, 1050, ob, 0, true));
                        break;

                    case SpellEffect.SnowMouse:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowMouse], 288, 8, 1200, ob, 0, true));
                        break;

                    // 203
                    //  case SpellEffect.DarkWraith:
                    //  ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowAssassin], 324, 5, 750, ob, 0, true));
                    //   break;

                    case SpellEffect.SnowAssassin:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowAssassin], 320, 5, 750, ob, 0, true));
                                    break;
                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowAssassin], 357, 6, 900, ob, 0, true));
                                    break;
                            }
                        }
                        break;


                    case SpellEffect.SecretWarrior2:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior2], 384, 5, 750, ob, 0, true));
                        break;
                    // 207
                    case SpellEffect.SecretWarrior3:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior3], 304, 6, 900, ob, 0, true));
                                    break;
                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior3], 342, 6, 900, ob, 0, true));
                                    break;
                            }
                        }
                        break;

                                                 // 210
                       case SpellEffect.SecretWarrior4:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior4], 320, 7, 1050, ob, 0, true));
                                    break;
                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior4], 374,7, 1050, ob, 0, true));
                                    break;
                            }
                        }
                        break;

                       // 212
                       case SpellEffect.SecretKnight:
                        switch (p.EffectType)
                        {
                            case 0:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 416, 6, 600, ob, 0, true));
                                break;
                            case 1:
                                // 213
                                MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 470, 15, 15*150, ob.CurrentLocation, 0, true));
                                break;

                                // 214
                            case 2:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 504, 8, 8*150, ob, 0, true));
                                break;

                        }
                        break;

                         case SpellEffect.SecretWarrior8:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior8], 392, 6, 6*150, ob, 0, true));
                        break;

                    case SpellEffect.SecretPage4:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretPaper4], 299, 8, 8 * 150, ob, 0, true));
                        break;

                         case SpellEffect.SecretJudge:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretJudge], 374, 14, 14*150, ob, 0, true));
                        break;

                    case SpellEffect.SecretWizard:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWizard], 438, 27, 27 * 150, ob, 0, true));
                        break;


                    case SpellEffect.SecretQueen:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 465, 6, 600, ob, 0, true));
                        break;
                    #region new
                    case SpellEffect.MutantWeb:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FlamingMutant], 320, 10, 800, ob));
                        break;

                    case SpellEffect.MutantWebHold:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FlamingMutant], 330, 10, 800, ob)
                        {
                            Repeat = p.Time > 0,
                            RepeatUntil = p.Time > 0 ? CMain.Time + p.Time : 0
                        });
                        break;
                    case SpellEffect.FlyingStatueCast:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FlyingStatue], 314, 20, 800, ob));
                        break;
                    case SpellEffect.FlyingStatueGreen:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FlyingStatue], 352, 10, 800, ob));
                        break;
                    case SpellEffect.FlyingStatuePurple:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FlyingStatue], 334, 10, 800, ob));
                        break;

                    case SpellEffect.KingGuardHold:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingGuard], 753, 10, 800, ob));
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingGuard], 763, 10, 800, ob));
                        break;
                    case SpellEffect.KingGuardSmash:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingGuard], 773, 10, 800, ob));
                        break;

                    case SpellEffect.StoningStatusFire:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StoningStatue], 657, 10, 800, ob));
                        break;

                    case SpellEffect.CatShaman:
                        {
                            switch (p.EffectType)
                            {
                                case 0:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CatShaman], 728, 12, 1800, ob, 0, true));
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CatShaman], 740, 6, 800, ob, 0, true));
                                    break;

                                case 1:
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CatShaman], 746, 8, 1200, ob, 0, true));
                                    ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CatShaman], 754, 7, 1050, ob, 0, true));
                                    break;
                            }
                        }
                        break;
                    #endregion

                    case SpellEffect.KunLun1:
                        switch (p.EffectType)
                        {
                            case 0:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun1], 560, 6, 900, ob, 0, false) { Blend = false });
                                break;
                            case 1:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun1], 570, 5, 5 * 150, ob, 0, false) { Blend = false }); break; }
                        break;

                    case SpellEffect.KunLun2:
                        switch (p.EffectType)
                        {
                            case 0:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun2], 650, 7, 1050, ob, 0, true));
                                break;
                            case 1:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun2], 740, 10, 10 * 150, ob, 0, true));
                                break;
                        }
                        break;

                    case SpellEffect.KunLun3:
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun3], 640, 10, 1000, ob, 0, true));
                        break;

                    case SpellEffect.DogYoLin7:
                        switch (p.EffectType)
                        {
                            case 0:
                                ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1490, 11, 1500, ob, 0, true));
                                break;
                            case 1:
                                MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1540, 10, 10 * 150, ob.CurrentLocation, 0, true));
                                break;
                        }

                        break;
                }
                return;
            }

        }

        private void HumUpPlayer(S.HumUpPlayer p)//stupple
        {
            PlayerObject player = (PlayerObject)GameScene.Scene.MapControl.FindObject(p.ObjectID, p.Location.X, p.Location.Y);
            player.Class = p.Class;
        }


        private void RangeAttack(S.RangeAttack p)
        {
            User.TargetID = p.TargetID;
            User.TargetPoint = p.Target;
            User.Spell = p.Spell;
        }

        private void Pushed(S.Pushed p)
        {
            User.ActionFeed.Add(new QueuedAction { Action = MirAction.Pushed, Direction = p.Direction, Location = p.Location });
        }
        private void ObjectPushed(S.ObjectPushed p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Pushed, Direction = p.Direction, Location = p.Location });

                return;
            }
        }
        private void ObjectName(S.ObjectName p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Name = p.Name;
                return;
            }
        }
        private void UserStorage(S.UserStorage p)
        {
            if(Storage.Length != p.Storage.Length)
            {
                Array.Resize(ref Storage, p.Storage.Length);
            }

            Storage = p.Storage;

            for (int i = 0; i < Storage.Length; i++)
            {
                if (Storage[i] == null) continue;
                Bind(Storage[i]);
            }
        }
        private void SwitchGroup(S.SwitchGroup p)
        {
            GroupDialog.AllowGroup = p.AllowGroup;
        }
        private void DeleteGroup()
        {
            GroupDialog.GroupList.Clear();
            ChatDialog.ReceiveChat(CMain.Tr("You have left the group."), ChatType.Group);
        }
        private void DeleteMember(S.DeleteMember p)
        {
            GroupDialog.GroupList.Remove(p.Name);
            ChatDialog.ReceiveChat(string.Format("-{0} has left the group.", p.Name), ChatType.Group);
        }
        private void GroupInvite(S.GroupInvite p)
        {
            MirMessageBox messageBox = new MirMessageBox(CMain.Format("Do you want to group with {0}?", p.Name), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.GroupInvite { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => Network.Enqueue(new C.GroupInvite { AcceptInvite = false });

            messageBox.Show();
        }
        private void AddMember(S.AddMember p)
        {
            GroupDialog.GroupList.Add(p.Name);
            ChatDialog.ReceiveChat(CMain.Format("-{0} has joined the group.", p.Name), ChatType.Group);
        }
        private void Revived()
        {
            User.SetAction();
            User.Dead = false;
            User.Effects.Add(new Effect(Libraries.Magic2, 1220, 20, 2000, User));
            SoundManager.PlaySound(SoundList.Revive);
        }
        private void ObjectRevived(S.ObjectRevived p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                if (p.Effect)
                {
                    ob.Effects.Add(new Effect(Libraries.Magic2, 1220, 20, 2000, ob));
                    SoundManager.PlaySound(SoundList.Revive);
                }
                ob.Dead = false;
                ob.ActionFeed.Clear();
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Revive, Direction = ob.Direction, Location = ob.CurrentLocation });
                return;
            }
        }
        private void SpellToggle(S.SpellToggle p)
        {
            ClientMagic clientMagic = User.GetMagic(p.Spell);
            if (clientMagic == null)
                return;

            clientMagic.Toggle = p.CanUse;

            MagicConf magicConf = CMain.MagicConf.GetConf(p.Spell);
            if (magicConf != null && magicConf.CanUsedTips != null && magicConf.CanUsedTips.Length > 0)
                ChatDialog.ReceiveChat(clientMagic.Toggle ? CMain.Tr(magicConf.CanUsedTips) : CMain.Tr(magicConf.CannotUsedTips), ChatType.Hint);

            if (p.Spell == Spell.FlamingSword)
            {
                GameScene.NextTimeFireHit = p.CanUse;
                if (p.CanUse)
                    GameScene.LastFireHitTick = CMain.Time;
            }
            else if (p.Spell == Spell.DaMoGunFa)
            {
                GameScene.NextTimeDaMoHit = p.CanUse;
                if (p.CanUse)
                    GameScene.LastDamoHitTick = CMain.Time;
            }
        }

        private void ObjectHealth(S.ObjectHealth p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.PercentHealth = p.Percent;
                ob.HealthTime = CMain.Time + p.Expire * 1000;
                ob.Health = p.Health;
                ob.MaxHealth = p.MaxHealth;
                return;
            }
        }
        private void MapEffect(S.MapEffect p)
        {
            switch (p.Effect)
            {
                case SpellEffect.Mine:
                    SoundManager.PlaySound(10091);
                    Effect HitWall = new Effect(Libraries.Effect, 8 * p.Value, 3, 240, p.Location) { Light = 0 };
                    MapControl.Effects.Add(HitWall);
                    break;

                case SpellEffect.ExplosiveTrap:
                    MapControl.Effects.Add(new Effect(Libraries.Magic3, 1570, 9, 900, p.Location, 0, true));
                    MirSounds.SoundManager.PlaySound(20000 + 124 * 10 + 5);//Boom for all players in range
                    break;

                case SpellEffect.HornedCommander:
                    switch (p.Value)
                    {
                        case 0:
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1190, 9, 900, p.Location, 0, true));
                            break;

                        case 1:
                            MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1066, 12, 1200, p.Location, 0, true));
                            break;
                    }
                    break;
            }
        }
        private void ObjectRangeAttack(S.ObjectRangeAttack p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                QueuedAction action = null;
                if (ob.Race == ObjectType.Player)
                {
                    switch (p.Type)
                    {
                        default:
                            {
                                action = new QueuedAction { Action = MirAction.AttackRange1, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                                break;
                            }
                    }
                }
                else
                {
                    action = new QueuedAction { Action = MirAction.AttackRange1 + p.Type, Direction = p.Direction, Location = p.Location, Params = new List<object>() };
                }
                action.Params.Add(p.TargetID);
                action.Params.Add(p.Target);
                action.Params.Add(p.Spell);
                ob.ActionFeed.Add(action);
                return;
            }
        }

        private void ShowMentalState(Buff buff)
        {
            if (buff.Type == BuffType.MentalState)
            {
                switch (buff.Values[0])
                {
                    case 0:
                        ChatDialog.ReceiveChat("Mentalstate: Agressive.", ChatType.Hint);
                        break;
                    case 1:
                        ChatDialog.ReceiveChat("Mentalstate: Trick shot.", ChatType.Hint);
                        break;
                    case 2:
                        ChatDialog.ReceiveChat("Mentalstate: Group mode.", ChatType.Hint);
                        break;
                }

            }
        }

        private bool IsBuff(BuffType type)
        {
            switch(type)
            {
                case BuffType.MoreaBlood:
                case BuffType.MoreaBlood2:
                case BuffType.MoreaBlood3:
                case BuffType.MoreaBlood8:
                case BuffType.KunLun14:
                    return false;
            }
            return true;
        }

        private void AddBuff(S.AddBuff p)
        {
            Buff buff = new Buff { Type = p.Type, Caster = p.Caster, Expire = CMain.Time + p.Expire, Values = p.Values, Infinite = p.Infinite, ObjectID = p.ObjectID, Visible = p.Visible };

            if (buff.ObjectID == User.ObjectID)
            {
                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].Type != buff.Type) continue;

                    Buffs[i] = buff;
                    User.RefreshStats();
                    ShowMentalState(buff);
                    return;
                }
                
                Buffs.Add(buff);
                if (IsBuff(p.Type))
                    BuffsDialog.CreateBuff(buff);
                else
                    DebuffsDialog.CreateBuff(buff);
                User.RefreshStats();
                ShowMentalState(buff);               
            }

            if (!buff.Visible || buff.ObjectID <= 0) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != buff.ObjectID) continue;
                if ((ob is PlayerObject) || (ob is MonsterObject))
                {
                    if (!ob.Buffs.Contains(buff.Type))
                    {
                        ob.Buffs.Add(buff.Type);
                    }

                    ob.AddBuffEffect(buff.Type);
                    return;
                }
            }
        }
        private void RemoveBuff(S.RemoveBuff p)
        {
            for (int i = 0; i < Buffs.Count; i++)
            {
                if (Buffs[i].Type != p.Type || User.ObjectID != p.ObjectID) continue;

                switch (Buffs[i].Type)
                {
                    case BuffType.SwiftFeet:
                        User.Sprint = false;
                        break;
                }

                Buffs.RemoveAt(i);
                if (IsBuff(p.Type))
                    BuffsDialog.RemoveBuff(p.Type);
                else
                    DebuffsDialog.RemoveBuff(p.Type);
            }

            if (User.ObjectID == p.ObjectID)
                User.RefreshStats();

            if (p.ObjectID <= 0) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];

                if (ob.ObjectID != p.ObjectID) continue;

                ob.Buffs.Remove(p.Type);
                ob.RemoveBuffEffect(p.Type);
                return;
            }
        }

        private void ObjectHidden(S.ObjectHidden p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                ob.Hidden = p.Hidden;
                return;
            }
        }
        private void ObjectSneaking(S.ObjectSneaking p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
               // ob.SneakingActive = p.SneakingActive;
                return;
            }
        }

        private void ObjectLevelEffects(S.ObjectLevelEffects p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID || ob.Race != ObjectType.Player) continue;

                PlayerObject temp = (PlayerObject)ob;

                temp.LevelEffects = p.LevelEffects;

                temp.SetEffects();
                return;
            }
        }

        private void RefreshItem(S.RefreshItem p)
        {
            Bind(p.Item);

            if (SelectedCell != null && SelectedCell.Item.UniqueID == p.Item.UniqueID)
                SelectedCell = null;

            if (HoverItem != null && HoverItem.UniqueID == p.Item.UniqueID)
            {
                DisposeItemLabel();
                CreateItemLabel(p.Item);
            }

            for (int i = 0; i < User.Inventory.Length; i++)
                if (User.Inventory[i] != null && User.Inventory[i].UniqueID == p.Item.UniqueID)
                {
                    User.Inventory[i] = p.Item;
                    User.RefreshStats();
                    return;
                }

            for (int i = 0; i < User.Equipment.Length; i++)
                if (User.Equipment[i] != null && User.Equipment[i].UniqueID == p.Item.UniqueID)
                {
                    User.Equipment[i] = p.Item;
                    User.RefreshStats();
                    return;
                }


        }
        private void ObjectSpell(S.ObjectSpell p)
        {
            SpellObject ob = new SpellObject(p.ObjectID);
            ob.Load(p);
        }
        private void ObjectDeco(S.ObjectDeco p)
        {
            DecoObject ob = new DecoObject(p.ObjectID);
            ob.Load(p);
        }

        private void UserDash(S.UserDash p)
        {
            if (User.Direction == p.Direction && User.CurrentLocation == p.Location)
            {
                MapControl.NextAction = 0;
                return;
            }
            MirAction action = User.CurrentAction == MirAction.DashL ? MirAction.DashR : MirAction.DashL;
            for (int i = User.ActionFeed.Count - 1; i >= 0; i--)
            {
                if (User.ActionFeed[i].Action == MirAction.DashR)
                {
                    action = MirAction.DashL;
                    break;
                }
                if (User.ActionFeed[i].Action == MirAction.DashL)
                {
                    action = MirAction.DashR;
                    break;
                }
            }

            User.ActionFeed.Add(new QueuedAction { Action = action, Direction = p.Direction, Location = p.Location });

        }
        private void UserDashFail(S.UserDashFail p)
        {
            MapControl.NextAction = 0;
            User.ActionFeed.Add(new QueuedAction { Action = MirAction.DashFail, Direction = p.Direction, Location = p.Location });
        }
        private void ObjectDash(S.ObjectDash p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                MirAction action = MirAction.DashL;

                if (ob.ActionFeed.Count > 0 && ob.ActionFeed[ob.ActionFeed.Count - 1].Action == action)
                    action = MirAction.DashR;

                ob.ActionFeed.Add(new QueuedAction { Action = action, Direction = p.Direction, Location = p.Location });

                return;
            }
        }
        private void ObjectDashFail(S.ObjectDashFail p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.DashFail, Direction = p.Direction, Location = p.Location });

                return;
            }
        }
        private void UserBackStep(S.UserBackStep p)
        {
            if (User.Direction == p.Direction && User.CurrentLocation == p.Location)
            {
                MapControl.NextAction = 0;
                return;
            }
            User.ActionFeed.Add(new QueuedAction { Action = MirAction.Jump, Direction = p.Direction, Location = p.Location });
        }
        private void ObjectBackStep(S.ObjectBackStep p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                ((PlayerObject)ob).JumpDistance = p.Distance;

                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.Jump, Direction = p.Direction, Location = p.Location });

                return;
            }
        }
        private void UserDashAttack(S.UserDashAttack p)
        {
            if (User.Direction == p.Direction && User.CurrentLocation == p.Location)
            {
                MapControl.NextAction = 0;
                return;
            }
            //User.JumpDistance = p.Distance;
            User.ActionFeed.Add(new QueuedAction { Action = MirAction.DashAttack, Direction = p.Direction, Location = p.Location });
        }
        private void ObjectDashAttack(S.ObjectDashAttack p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                ((PlayerObject)ob).JumpDistance = p.Distance;

                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.DashAttack, Direction = p.Direction, Location = p.Location });

                return;
            }
        }
        private void UserAttackMove(S.UserAttackMove p)//Warrior Skill - SlashingBurst
        {
            MapControl.NextAction = 0;
            if (User.CurrentLocation == p.Location && User.Direction == p.Direction) return;

            MapControl.RemoveObject(User);
            User.CurrentLocation = p.Location;
            User.MapLocation = p.Location;
            MapControl.AddObject(User);

            MapControl.FloorValid = false;
            MapControl.InputDelay = CMain.Time + 400;

            if (User.Dead) return;

            User.ClearMagic();
            User.QueuedAction = null;

            for (int i = User.ActionFeed.Count - 1; i >= 0; i--)
            {
                if (User.ActionFeed[i].Action == MirAction.Pushed) continue;
                User.ActionFeed.RemoveAt(i);
            }

            User.SetAction();

            User.ActionFeed.Add(new QueuedAction { Action = MirAction.Standing, Direction = p.Direction, Location = p.Location });
        }

        private void SetConcentration(S.SetConcentration p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                User.Concentrating = p.Enabled;
                User.ConcentrateInterrupted = p.Interrupted;
                if (p.Enabled && !p.Interrupted)
                {
                    int idx = InterruptionEffect.GetOwnerEffectID(User.ObjectID);
                    if (idx < 0)
                    {
                        //    InterruptionEffect.effectlist[idx] = new InterruptionEffect(Libraries.Magic3, 1860, 8, 8 * 100, User, true);
                        //else
                        User.Effects.Add(new InterruptionEffect(Libraries.Magic3, 1860, 8, 8 * 100, User, true));
                        SoundManager.PlaySound(20000 + 129 * 10);
                    }
                }
                break;
            }
        }
        private void SetObjectConcentration(S.SetObjectConcentration p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                ((PlayerObject)ob).Concentrating = p.Enabled;
                ((PlayerObject)ob).ConcentrateInterrupted = p.Interrupted;

                if (p.Enabled && !p.Interrupted)
                {
                    //int idx = InterruptionEffect.GetOwnerEffectID(ob.ObjectID);
                    //if (idx < 0)
                    //    InterruptionEffect.effectlist[idx] = new InterruptionEffect(Libraries.Magic3, 1860, 8, 8 * 100, ob, true);
                    if (((PlayerObject)ob).ConcentratingEffect == null)
                    {
                        ((PlayerObject)ob).Effects.Add(((PlayerObject)ob).ConcentratingEffect = new InterruptionEffect(Libraries.Magic3, 1860, 8, 8 * 100, ob, true));
                        SoundManager.PlaySound(20000 + 129 * 10);
                    }
                }
                break;
            }
        }

        private void SetElemental(S.SetElemental p)
        {
            if (User.ObjectID != p.ObjectID) return;

          //  User.HasElements = p.Enabled;
            User.ElementsLevel = (int)p.Value;
            int elementType = (int)p.ElementType;
            int maxExp = (int)p.ExpLast;

            if (p.Enabled && p.ElementType > 0)
                User.Effects.Add(new ElementsEffect(Libraries.Magic3, 1630 + ((elementType - 1) * 10), 10, 10 * 100, User, true, 1 + (elementType - 1), maxExp, (elementType == 4 || elementType == 3) ? true : false));
        }
        private void SetObjectElemental(S.SetObjectElemental p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

 //               ((PlayerObject)ob).HasElements = p.Enabled;
                ((PlayerObject)ob).ElementCasted = p.Casted;
                ((PlayerObject)ob).ElementsLevel = (int)p.Value;
                int elementType = (int)p.ElementType;
                int maxExp = (int)p.ExpLast;

                if (p.Enabled && p.ElementType > 0)
                    ((PlayerObject)ob).Effects.Add(new ElementsEffect(Libraries.Magic3, 1630 + ((elementType - 1) * 10), 10, 10 * 100, ob, true, 1 + (elementType - 1), maxExp));
            }
        }

        private void RemoveDelayedExplosion(S.RemoveDelayedExplosion p)
        {
            //if (p.ObjectID == User.ObjectID) return;

            int effectid = DelayedExplosionEffect.GetOwnerEffectID(p.ObjectID);
            if (effectid >= 0)
                DelayedExplosionEffect.effectlist[effectid].Remove();
        }

        private void SetBindingShot(S.SetBindingShot p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                if (ob.Race != ObjectType.Monster) continue;

                TrackableEffect NetCast = new TrackableEffect(new Effect(Libraries.MagicC, 0, 8, 700, ob));
                NetCast.EffectName = "BindingShotDrop";

                //TrackableEffect NetDropped = new TrackableEffect(new Effect(Libraries.ArcherMagic, 7, 1, 1000, ob, CMain.Time + 600) { Repeat = true, RepeatUntil = CMain.Time + (p.Value - 1500) });
                TrackableEffect NetDropped = new TrackableEffect(new Effect(Libraries.MagicC, 7, 1, 1000, ob) { Repeat = true, RepeatUntil = CMain.Time + (p.Value - 1500) });
                NetDropped.EffectName = "BindingShotDown";

                TrackableEffect NetFall = new TrackableEffect(new Effect(Libraries.MagicC, 8, 8, 700, ob));
                NetFall.EffectName = "BindingShotFall";

                NetDropped.Complete += (o1, e1) =>
                {
                    SoundManager.PlaySound(20000 + 130 * 10 + 6);//sound M130-6
                    ob.Effects.Add(NetFall);
                };
                NetCast.Complete += (o, e) =>
                {
                    SoundManager.PlaySound(20000 + 130 * 10 + 5);//sound M130-5
                    ob.Effects.Add(NetDropped);
                };
                ob.Effects.Add(NetCast);
                break;
            }
        }

        private void SendOutputMessage(S.SendOutputMessage p)
        {
            OutputMessage(p.Message, p.Type);
        }

        private void NPCConsign()
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Consign;
            NPCDropDialog.Show();
        }
        private void NPCMarket(S.NPCMarket p)
        {
            for (int i = 0; i < p.Listings.Count; i++)
                Bind(p.Listings[i].Item);

            TrustMerchantDialog.SearchTextBox.Text = "";
            TrustMerchantDialog.Show();
            TrustMerchantDialog.UserMode = p.UserMode;
            TrustMerchantDialog.Listings = p.Listings;
            TrustMerchantDialog.Page = 0;
            TrustMerchantDialog.PageCount = p.Pages;
            TrustMerchantDialog.UpdateInterface();
        }
        private void NPCMarketPage(S.NPCMarketPage p)
        {
            if (!TrustMerchantDialog.Visible) return;

            for (int i = 0; i < p.Listings.Count; i++)
                Bind(p.Listings[i].Item);

            TrustMerchantDialog.Listings.AddRange(p.Listings);
            TrustMerchantDialog.Page = (TrustMerchantDialog.Listings.Count - 1) / 10;
            TrustMerchantDialog.UpdateInterface();
        }
        private void ConsignItem(S.ConsignItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID) ?? BeltDialog.GetCell(p.UniqueID);

            if (cell == null) return;

            cell.Locked = false;

            if (!p.Success) return;

            cell.Item = null;

            User.RefreshStats();
        }
        private void MarketFail(S.MarketFail p)
        {
            TrustMerchantDialog.MarketTime = 0;
            switch (p.Reason)
            {
                case 0:
                    MirMessageBox.ShowTr("You cannot use the TrustMerchant when dead.");
                    break;
                case 1:
                    MirMessageBox.ShowTr("You cannot buy from the TrustMerchant without using.");
                    break;
                case 2:
                    MirMessageBox.ShowTr("This item has already been sold.");
                    break;
                case 3:
                    MirMessageBox.ShowTr("This item has Expired and cannot be brought.");
                    break;
                case 4:
                    MirMessageBox.ShowTr("You do not have enough gold to buy this item.");
                    break;
                case 5:
                    MirMessageBox.ShowTr("You do not have enough weight or space spare to buy this item.");
                    break;
                case 6:
                    MirMessageBox.ShowTr("You cannot buy your own items.");
                    break;
                case 7:
                    MirMessageBox.ShowTr("You are too far away from the Trust Merchant.");
                    break;
                case 8:
                    MirMessageBox.ShowTr("You cannot hold enough gold to get your sale");
                    break;
            }

        }
        private void MarketSuccess(S.MarketSuccess p)
        {
            TrustMerchantDialog.MarketTime = 0;
            MirMessageBox.Show(p.Message);
        }
        private void ObjectSitDown(S.ObjectSitDown p)
        {
            if (p.ObjectID == User.ObjectID) return;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;
                if (ob.Race != ObjectType.Monster) continue;
                ob.SitDown = p.Sitting;
                ob.ActionFeed.Add(new QueuedAction { Action = MirAction.SitDown, Direction = p.Direction, Location = p.Location });
                return;
            }
        }

        private void BaseStatsInfo(S.BaseStatsInfo p)
        {
            User.CoreStats = p.Stats;
            User.RefreshStats();
        }

        private void UserName(S.UserName p)
        {
            for (int i = 0; i < UserIdList.Count; i++)
                if (UserIdList[i].Id == p.Id)
                {
                    UserIdList[i].UserName = p.Name;
                    break;
                }
            DisposeItemLabel();
            HoverItem = null;
        }

        private void ChatItemStats(S.ChatItemStats p)
        {
            for (int i = 0; i < ChatItemList.Count; i++)
                if (ChatItemList[i].ID == p.ChatItemId)
                {
                    ChatItemList[i].ItemStats = p.Stats;
                    ChatItemList[i].RecievedTick = CMain.Time;
                }
        }

        private void GuildInvite(S.GuildInvite p)
        {
            MirMessageBox messageBox = new MirMessageBox(CMain.Format("Do you want to join the {0} guild?", p.Name), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.GuildInvite { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => Network.Enqueue(new C.GuildInvite { AcceptInvite = false });

            messageBox.Show();
        }

        private void ChangeNameRequest(S.ChangeNameRequest p)
        {
            MirInputBox inputBox = new MirInputBox(CMain.Tr("请输入名字"));
            inputBox.OKButton.Click += (o, e) =>
            {
                if (string.IsNullOrEmpty(inputBox.InputTextBox.Text) || inputBox.InputTextBox.Text.Contains('\\'))
                {
                    ChatDialog.ReceiveChat("你不能使用\\", ChatType.System);
                    inputBox.InputTextBox.Text = "";
                }

                if (!NameChecker.CheckCharacterName(inputBox.InputTextBox.Text))
                {
                    ChatDialog.ReceiveChat("名字不合法", ChatType.System);
                    return;
                }

                Network.Enqueue(new C.ChangeNameReturn { Name = inputBox.InputTextBox.Text });
                inputBox.Dispose();
            };
            inputBox.Show();
        }

        private void GuildNameRequest(S.GuildNameRequest p)
        {
            MirInputBox inputBox = new MirInputBox(CMain.Tr("Please enter a guild name, length must be 3~20 characters."));
            inputBox.InputTextBox.TextBox.KeyPress += (o, e) =>
            {
              //  if (!NameChecker.CheckGuildName(inputBox.InputTextBox.Text))
              //  {
                //    e.Handled = true;
                 //   return;
               // }
                //string Allowed = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                //        string Allowed = "^[A-Za-z0-9-\u4E00-\u9FA5]{" + Globals.MinCharacterNameLength + "," + Globals.MaxCharacterNameLength + "}$";
                //        if (!NameChecker.CheckCharacterName(inputBox.InputTextBox.Text) && e.KeyChar != (char)Keys.Back)
                //            e.Handled = true;
                //if (!Allowed.Contains(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                //    e.Handled = true;
            };
            inputBox.OKButton.Click += (o, e) =>
            {
                if (inputBox.InputTextBox.Text.Contains('\\'))
                {
                    ChatDialog.ReceiveChat("You cannot use the \\ sign in a guildname!", ChatType.System);
                    inputBox.InputTextBox.Text = "";
                }

                if (!NameChecker.CheckGuildName(inputBox.InputTextBox.Text))
                {
                    ChatDialog.ReceiveChat(CMain.Tr("Please enter a guild name, length must be 3~20 characters."), ChatType.System);
                    return;
                }

                Network.Enqueue(new C.GuildNameReturn { Name = inputBox.InputTextBox.Text });
                inputBox.Dispose();
            };
            inputBox.Show();
        }

        private void GuildRequestWar(S.GuildRequestWar p)
        {
            MirInputBox inputBox = new MirInputBox("Please enter the guild you would like to go to war with.");

            inputBox.OKButton.Click += (o, e) =>
            {
                Network.Enqueue(new C.GuildWarReturn { Name = inputBox.InputTextBox.Text });
                inputBox.Dispose();
            };
            inputBox.Show();
        }

        private void GuildNoticeChange(S.GuildNoticeChange p)
        {
            if (p.update == -1)
                GuildDialog.NoticeChanged = true;
            else
                GuildDialog.NoticeChange(p.notice);
        }
        private void GuildMemberChange(S.GuildMemberChange p)
        {
            switch (p.Status)
            {
                case 0: // logged of
                    GuildDialog.MemberStatusChange(p.Name, false);
                    break;
                case 1: // logged on
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} logged on."), p.Name), ChatType.Guild);
                    GuildDialog.MemberStatusChange(p.Name, true);
                    break;
                case 2://new member
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} joined guild."), p.Name), ChatType.Guild);
                    GuildDialog.MemberCount++;
                    GuildDialog.MembersChanged = true;
                    break;
                case 3://kicked member
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} got removed from the guild."), p.Name), ChatType.Guild);
                    GuildDialog.MemberCount--;
                    GuildDialog.MembersChanged = true;
                    break;
                case 4://member left
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} left the guild."), p.Name), ChatType.Guild);
                    GuildDialog.MembersChanged = true;
                    break;
                case 5://rank change (name or different rank)
                    GuildDialog.MembersChanged = true;
                    break;
                case 6: //new rank
                    if (p.Ranks.Count > 0)
                        GuildDialog.NewRankRecieved(p.Ranks[0]);
                    break;
                case 7: //rank option changed
                    if (p.Ranks.Count > 0)
                        GuildDialog.RankChangeRecieved(p.Ranks[0]);
                    break;
                case 8: //my rank changed
                    if (p.Ranks.Count > 0)
                        GuildDialog.MyRankChanged(p.Ranks[0]);
                    break;
                case 255:
                    GuildDialog.NewMembersList(p.Ranks);
                    break;
            }
        }

        private void GuildStatus(S.GuildStatus p)
        {
            if ((User.GuildName == "") && (p.GuildName != ""))
            {
                GuildDialog.NoticeChanged = true;
                GuildDialog.MembersChanged = true;
            }
            if (p.GuildName == "")
            {
                GuildDialog.Hide();
            }

            if ((User.GuildName == p.GuildName) && (GuildDialog.Level < p.Level))
            {
                //guild leveled
            }
            bool GuildChange = User.GuildName != p.GuildName;
            User.GuildName = p.GuildName;
            User.GuildRankName = p.GuildRankName;
            GuildDialog.Level = p.Level;
            GuildDialog.Experience = p.Experience;
            GuildDialog.MaxExperience = p.MaxExperience;
            GuildDialog.Gold = p.Gold;
            GuildDialog.SparePoints = p.SparePoints;
            GuildDialog.MemberCount = p.MemberCount;
            GuildDialog.MaxMembers = p.MaxMembers;
            GuildDialog.Voting = p.Voting;
            GuildDialog.ItemCount = p.ItemCount;
            GuildDialog.BuffCount = p.BuffCount;
            GuildDialog.StatusChanged(p.MyOptions);
            GuildDialog.MyRankId = p.MyRankId;
            GuildDialog.UpdateMembers();
            //reset guildbuffs
            if (GuildChange)
            {
                GuildDialog.EnabledBuffs.Clear();
                GuildDialog.UpdateActiveStats();
                RemoveBuff(new S.RemoveBuff { ObjectID = User.ObjectID, Type = BuffType.Guild });
                User.RefreshStats();
            }
        }

        private void GuildExpGain(S.GuildExpGain p)
        {
            //OutputMessage(string.Format("Guild Experience Gained {0}.", p.Amount));
            GuildDialog.Experience += p.Amount;
        }

        private void GuildStorageGoldChange(S.GuildStorageGoldChange p)
        {
            switch (p.Type)
            {
                case 0:
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} donated {1} gold to guild funds."), p.Name, p.Amount), ChatType.Guild);
                    GuildDialog.Gold += p.Amount;
                    break;
                case 1:
                    ChatDialog.ReceiveChat(String.Format(CMain.Tr("{0} retrieved {1} gold from guild funds."), p.Name, p.Amount), ChatType.Guild);
                    if (GuildDialog.Gold > p.Amount)
                        GuildDialog.Gold -= p.Amount;
                    else
                        GuildDialog.Gold = 0;
                    break;
                case 2:
                    if (GuildDialog.Gold > p.Amount)
                        GuildDialog.Gold -= p.Amount;
                    else
                        GuildDialog.Gold = 0;
                    break;
                case 3:
                    GuildDialog.Gold += p.Amount;
                    break;
            }
        }

        private void GuildStorageItemChange(S.GuildStorageItemChange p)
        {
            MirItemCell fromCell = null;
            MirItemCell toCell = null;
            switch (p.Type)
            {
                case 0://store
                    toCell = GuildDialog.StorageGrid[p.To];

                    if (toCell == null) return;

                    toCell.Locked = false;
                    toCell.Item = p.Item.Item;
                    Bind(toCell.Item);
                    if (p.User != User.Id) return;
                    fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];
                    fromCell.Locked = false;
                    if (fromCell != null)
                        fromCell.Item = null;
                    User.RefreshStats();
                    break;
                case 1://retrieve
                    fromCell = GuildDialog.StorageGrid[p.From];

                    if (fromCell == null) return;
                    fromCell.Locked = false;

                    if (p.User != User.Id)
                    {
                        fromCell.Item = null;
                        return;
                    }
                    toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];
                    if (toCell == null) return;
                    toCell.Locked = false;
                    toCell.Item = fromCell.Item;
                    fromCell.Item = null;
                    break;

                case 2:
                    toCell = GuildDialog.StorageGrid[p.To];
                    fromCell = GuildDialog.StorageGrid[p.From];

                    if (toCell == null || fromCell == null) return;

                    toCell.Locked = false;
                    fromCell.Locked = false;
                    fromCell.Item = toCell.Item;
                    toCell.Item = p.Item.Item;
                    
                    Bind(toCell.Item);
                    if (fromCell.Item != null) 
                        Bind(fromCell.Item);
                    break;
                case 3://failstore
                    fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];
                    toCell = GuildDialog.StorageGrid[p.To];

                    if (toCell == null || fromCell == null) return;

                    toCell.Locked = false;
                    fromCell.Locked = false;
                    break;
                case 4://failretrieve
                    toCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];
                    fromCell = GuildDialog.StorageGrid[p.To];

                    if (toCell == null || fromCell == null) return;

                    toCell.Locked = false;
                    fromCell.Locked = false;
                    break;
                case 5://failmove
                    fromCell = GuildDialog.StorageGrid[p.To];
                    toCell = GuildDialog.StorageGrid[p.From];

                    if (toCell == null || fromCell == null) return;

                    GuildDialog.StorageGrid[p.From].Locked = false;
                    GuildDialog.StorageGrid[p.To].Locked = false;
                    break;
            }
        }
        private void GuildStorageList(S.GuildStorageList p)
        {
            for (int i = 0; i < p.Items.Length; i++)
            {
                if (i >= GuildDialog.StorageGrid.Length) break;
                if (p.Items[i] == null)
                {
                    GuildDialog.StorageGrid[i].Item = null;
                    continue;
                }
                GuildDialog.StorageGrid[i].Item = p.Items[i].Item;
                Bind(GuildDialog.StorageGrid[i].Item);
            }
        }

        private void MarriageRequest(S.MarriageRequest p)
        {
            MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("{0} has asked for your hand in marriage."), p.Name), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.MarriageReply { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => { Network.Enqueue(new C.MarriageReply { AcceptInvite = false }); messageBox.Dispose(); };

            messageBox.Show();
        }

        private void DivorceRequest(S.DivorceRequest p)
        {
            MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("{0} has requested a divorce"), p.Name), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.DivorceReply { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => { Network.Enqueue(new C.DivorceReply { AcceptInvite = false }); messageBox.Dispose(); };

            messageBox.Show();
        }

        private void MentorRequest(S.MentorRequest p)
        {
            MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("{0} (Level {1}) has requested you teach him the ways of the {2}."), p.Name, p.Level, GameScene.User.Class.ToString()), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.MentorReply { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => { Network.Enqueue(new C.MentorReply { AcceptInvite = false }); messageBox.Dispose(); };

            messageBox.Show();
        }

        private bool UpdateGuildBuff(GuildBuff buff, bool Remove = false)
        {
            for (int i = 0; i < GuildDialog.EnabledBuffs.Count; i++)
            {
                if (GuildDialog.EnabledBuffs[i].Id == buff.Id)
                {
                    if (Remove)
                    {
                        GuildDialog.EnabledBuffs.RemoveAt(i);
                    }
                    else
                        GuildDialog.EnabledBuffs[i] = buff;
                    return true;
                }
            }
            return false;
        }

        private void GuildBuffList(S.GuildBuffList p)
        {
            //getting the list of all guildbuffs on server?
            if (p.GuildBuffs.Count > 0)
                GuildDialog.GuildBuffInfos.Clear();
            for (int i = 0; i < p.GuildBuffs.Count; i++)
            {
                GuildDialog.GuildBuffInfos.Add(p.GuildBuffs[i]);
            }
            //getting the list of all active/removedbuffs?
            for (int i = 0; i < p.ActiveBuffs.Count; i++)
            {
                //if (p.ActiveBuffs[i].ActiveTimeRemaining > 0)
                //    p.ActiveBuffs[i].ActiveTimeRemaining = Convert.ToInt32(CMain.Time / 1000) + (p.ActiveBuffs[i].ActiveTimeRemaining * 60);
                if (UpdateGuildBuff(p.ActiveBuffs[i], p.Remove == 1)) continue;
                if (!(p.Remove == 1))
                {
                    GuildDialog.EnabledBuffs.Add(p.ActiveBuffs[i]);
                    //CreateGuildBuff(p.ActiveBuffs[i]);
                }
            }

            for (int i = 0; i < GuildDialog.EnabledBuffs.Count; i++)
            {
                if (GuildDialog.EnabledBuffs[i].Info == null)
                {
                    GuildDialog.EnabledBuffs[i].Info = GuildDialog.FindGuildBuffInfo(GuildDialog.EnabledBuffs[i].Id);
                }
            }

            Buff buff = Buffs.FirstOrDefault(e => e.Type == BuffType.Guild);

            if (GuildDialog.EnabledBuffs.Any(e => e.Active))
            {
                if (buff == null)
                {
                    buff = new Buff { Type = BuffType.Guild, ObjectID = User.ObjectID, Caster = "Guild", Infinite = true };

                    Buffs.Add(buff);
                    BuffsDialog.CreateBuff(buff);
                }

                GuildDialog.UpdateActiveStats();
            }
            else
            {
                RemoveBuff(new S.RemoveBuff { ObjectID = User.ObjectID, Type = BuffType.Guild });
            }

            User.RefreshStats();
        }

        private void TradeRequest(S.TradeRequest p)
        {
            MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("Player {0} has requested to trade with you."), p.Name), MirMessageBoxButtons.YesNo);

            messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.TradeReply { AcceptInvite = true });
            messageBox.NoButton.Click += (o, e) => { Network.Enqueue(new C.TradeReply { AcceptInvite = false }); messageBox.Dispose(); };

            messageBox.Show();
        }
        private void TradeAccept(S.TradeAccept p)
        {
            GuestTradeDialog.GuestName = p.Name;
            TradeDialog.TradeAccept();
        }
        private void TradeGold(S.TradeGold p)
        {
            GuestTradeDialog.GuestGold = p.Amount;
            TradeDialog.ChangeLockState(false);
            TradeDialog.RefreshInterface();
        }
        private void TradeItem(S.TradeItem p)
        {
            GuestTradeDialog.GuestItems = p.TradeItems;
            TradeDialog.ChangeLockState(false);
            TradeDialog.RefreshInterface();
        }
        private void TradeConfirm()
        {
            TradeDialog.TradeReset();
        }
        private void TradeCancel(S.TradeCancel p)
        {
            if (p.Unlock)
            {
                TradeDialog.ChangeLockState(false);
            }
            else
            {
                TradeDialog.TradeReset();

                MirMessageBox messageBox = new MirMessageBox("Deal cancelled.\r\nTo deal correctly you must face the other party.", MirMessageBoxButtons.OK);
                messageBox.Show();
            }
        }
        private void NPCAwakening()
        {
            if (NPCAwakeDialog.Visible != true)
                NPCAwakeDialog.Show();
        }
        private void NPCDisassemble()
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Disassemble;
            NPCDropDialog.Show();
        }
        private void NPCDowngrade()
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Downgrade;
            NPCDropDialog.Show();
        }
        private void NPCReset()
        {
            if (!NPCDialog.Visible) return;
            NPCDropDialog.PType = PanelType.Reset;
            NPCDropDialog.Show();
        }
        private void AwakeningNeedMaterials(S.AwakeningNeedMaterials p)
        {
            NPCAwakeDialog.SetNeedItems(p.Materials, p.MaterialsCount);
        }
        private void AwakeningLockedItem(S.AwakeningLockedItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID);
            if (cell != null)
                cell.Locked = p.Locked;
        }
        private void Awakening(S.Awakening p)
        {
            if (NPCAwakeDialog.Visible)
                NPCAwakeDialog.Hide();
            if (InventoryDialog.Visible)
                InventoryDialog.Hide();

            MirItemCell cell = InventoryDialog.GetCell((ulong)p.removeID);
            if (cell != null)
            {
                cell.Locked = false;
                cell.Item = null;
            }

            for (int i = 0; i < InventoryDialog.Grid.Length; i++)
            {
                if (InventoryDialog.Grid[i].Locked == true)
                {
                    InventoryDialog.Grid[i].Locked = false;

                    //if (InventoryDialog.Grid[i].Item.UniqueID == (ulong)p.removeID)
                    //{
                    //    InventoryDialog.Grid[i].Item = null;
                    //}
                }
            }

            for (int i = 0; i < NPCAwakeDialog.ItemsIdx.Length; i++)
            {
                NPCAwakeDialog.ItemsIdx[i] = 0;
            }

            MirMessageBox messageBox = null;

            switch (p.result)
            {
                case -4:
                    messageBox = new MirMessageBox(CMain.Tr("You have not supplied enough materials."), MirMessageBoxButtons.OK);
                    MapControl.AwakeningAction = false;
                    break;
                case -3:
                    messageBox = new MirMessageBox(CMain.Tr("You do not have enough gold."), MirMessageBoxButtons.OK);
                    MapControl.AwakeningAction = false;
                    break;
                case -2:
                    messageBox = new MirMessageBox(CMain.Tr("Awakening already at maximum level."), MirMessageBoxButtons.OK);
                    MapControl.AwakeningAction = false;
                    break;
                case -1:
                    messageBox = new MirMessageBox(CMain.Tr("Cannot awaken this item."), MirMessageBoxButtons.OK);
                    MapControl.AwakeningAction = false;
                    break;
                case 0:
                    //messageBox = new MirMessageBox("Upgrade Failed.", MirMessageBoxButtons.OK);
                    break;
                case 1:
                    //messageBox = new MirMessageBox("Upgrade Success.", MirMessageBoxButtons.OK);
                    break;

            }

            if (messageBox != null) messageBox.Show();
        }

        private void ReceiveMail(S.ReceiveMail p)
        {
            NewMail = false;
            NewMailCounter = 0;
            User.Mail.Clear();

            User.Mail = p.Mail.OrderByDescending(e => !e.Locked).ThenByDescending(e => e.DateSent).ToList();

            foreach(ClientMail mail in User.Mail)
            {
                foreach(UserItem itm in mail.Items)
                {
                    Bind(itm);
                }
            }

            //display new mail received
            if (User.Mail.Any(e => e.Opened == false))
            {
                NewMail = true;
            }

            GameScene.Scene.MailListDialog.UpdateInterface();
        }

        private void MailLockedItem(S.MailLockedItem p)
        {
            MirItemCell cell = InventoryDialog.GetCell(p.UniqueID);
            if (cell != null)
                cell.Locked = p.Locked;
        }

        private void MailSendRequest(S.MailSendRequest p)
        {
            MirInputBox inputBox = new MirInputBox("Please enter the name of the person you would like to mail.");

            inputBox.OKButton.Click += (o1, e1) =>
            {
                GameScene.Scene.MailComposeParcelDialog.ComposeMail(inputBox.InputTextBox.Text);
                GameScene.Scene.InventoryDialog.Show();

                //open letter dialog, pass in name
                inputBox.Dispose();
            };

            inputBox.Show();
        }

        private void MailSent(S.MailSent p)
        {
            for (int i = 0; i < InventoryDialog.Grid.Length; i++)
            {
                if (InventoryDialog.Grid[i].Locked == true)
                {
                    InventoryDialog.Grid[i].Locked = false;
                }
            }

            for (int i = 0; i < BeltDialog.Grid.Length; i++)
            {
                if (BeltDialog.Grid[i].Locked == true)
                {
                    BeltDialog.Grid[i].Locked = false;
                }
            }

            GameScene.Scene.MailComposeParcelDialog.Hide();
            GameScene.Scene.ChatDialog.ReceiveChat("Mail send successful.", ChatType.System);
        }

        private void ParcelCollected(S.ParcelCollected p)
        {
            switch(p.Result)
            {
                case -1:
                    MirMessageBox messageBox = new MirMessageBox(string.Format(CMain.Tr("No parcels to collect.")), MirMessageBoxButtons.OK);
                    messageBox.Show();
                    break;
                case 0:
                    messageBox = new MirMessageBox(string.Format(CMain.Tr("All parcels have been collected.")), MirMessageBoxButtons.OK);
                    messageBox.Show();
                    break;
                case 1:
                    GameScene.Scene.MailReadParcelDialog.Hide();
                    break;
            }
        }

        private void ResizeInventory(S.ResizeInventory p)
        {
            Array.Resize(ref User.Inventory, p.Size);
            InventoryDialog.RefreshInventory2();
        }

        private void ResizeStorage(S.ResizeStorage p)
        {
            Array.Resize(ref Storage, p.Size);
            User.HasExpandedStorage = p.HasExpandedStorage;
            User.ExpandedStorageExpiryTime = p.ExpiryTime;

            StorageDialog.RefreshStorage2();
        }

        private void MailCost(S.MailCost p)
        {
            if(GameScene.Scene.MailComposeParcelDialog.Visible)
            {
                if (p.Cost > 0)
                    SoundManager.PlaySound(SoundList.Gold);

                GameScene.Scene.MailComposeParcelDialog.ParcelCostLabel.Text = p.Cost.ToString();
            }
        }

        public void AddQuestItem(UserItem item)
        {
            Redraw();

            if (item.Info.StackSize > 1) //Stackable
            {
                for (int i = 0; i < User.Inventory.Length; i++)
                {
                    UserItem temp = User.Inventory[i];
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

            for (int i = 0; i < User.Inventory.Length; i++)
            {
                if (User.Inventory[i] != null) continue;
                User.Inventory[i] = item;
                return;
            }
        }

        private void RequestReincarnation()
        {
            if (CMain.Time > User.DeadTime && User.CurrentAction == MirAction.Dead)
            {
                MirMessageBox messageBox = new MirMessageBox(CMain.Tr("Would you like to be revived?"), MirMessageBoxButtons.YesNo);

                messageBox.YesButton.Click += (o, e) => Network.Enqueue(new C.AcceptReincarnation());

                messageBox.Show();
            }
        }

        private void NewIntelligentCreature(S.NewIntelligentCreature p)
        {
            User.IntelligentCreatures.Add(p.Creature);

            MirInputBox inputBox = new MirInputBox("Please give your creature a name.");
            inputBox.InputTextBox.Text = GameScene.User.IntelligentCreatures[User.IntelligentCreatures.Count-1].CustomName;
            inputBox.OKButton.Click += (o1, e1) =>
            {
                if (IntelligentCreatureDialog.Visible) IntelligentCreatureDialog.Update();//refresh changes
                GameScene.User.IntelligentCreatures[User.IntelligentCreatures.Count - 1].CustomName = inputBox.InputTextBox.Text;
                Network.Enqueue(new C.UpdateIntelligentCreature { Creature = GameScene.User.IntelligentCreatures[User.IntelligentCreatures.Count - 1] });
                inputBox.Dispose();
            };
            inputBox.Show();
        }

        private void UpdateIntelligentCreatureList(S.UpdateIntelligentCreatureList p)
        {
            User.CreatureSummoned = p.CreatureSummoned;
            User.SummonedCreatureType = p.SummonedCreatureType;
            User.PearlCount = p.PearlCount;
            if (p.CreatureList.Count != User.IntelligentCreatures.Count)
            {
                User.IntelligentCreatures.Clear();
                for (int i = 0; i < p.CreatureList.Count; i++)
                    User.IntelligentCreatures.Add(p.CreatureList[i]);

                for (int i = 0; i < IntelligentCreatureDialog.CreatureButtons.Length; i++)
                    IntelligentCreatureDialog.CreatureButtons[i].Clear();

                IntelligentCreatureDialog.Hide();
            }
            else
            {
                for (int i = 0; i < p.CreatureList.Count; i++)
                {
                    User.IntelligentCreatures[i] = p.CreatureList[i];

                    if (User.IntelligentCreatures[i].ExpireTime != -9999)
                        User.IntelligentCreatures[i].ExpireTime += CMain.Time / 1000;
                    if (User.IntelligentCreatures[i].PetType == User.SummonedCreatureType)
                    {
                        User.IntelligentCreatures[i].MaintainFoodTime += CMain.Time / 1000;
                        User.IntelligentCreatures[i].BlackstoneTime -= CMain.Time / 1000;
                    }
                }
                if (IntelligentCreatureDialog.Visible) IntelligentCreatureDialog.Update();
            }
        }

        private void IntelligentCreatureEnableRename(S.IntelligentCreatureEnableRename p)
        {
            IntelligentCreatureDialog.CreatureRenameButton.Visible = true;
            if (IntelligentCreatureDialog.Visible) IntelligentCreatureDialog.Update();
        }

        private void IntelligentCreaturePickup(S.IntelligentCreaturePickup p)
        {
            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != p.ObjectID) continue;

                MonsterObject monOb = (MonsterObject)ob;

                if (monOb != null) monOb.PlayPickupSound();
            }
        }

        private void NPCPearlGoods(S.NPCPearlGoods p)
        {
            for (int i = 0; i < p.List.Count; i++)
            {
                p.List[i].Info = GetInfo(p.List[i].ItemIndex);
            }

            NPCRate = p.Rate;
            if (!NPCDialog.Visible) return;
            NPCGoodsDialog.usePearls = true;
            NPCGoodsDialog.NewGoods(p.List);
            NPCGoodsDialog.Show();
        }

        private void FriendUpdate(S.FriendUpdate p)
        {
            GameScene.Scene.FriendDialog.Friends = p.Friends;

            if (GameScene.Scene.FriendDialog.Visible)
            {
                GameScene.Scene.FriendDialog.Update(false);
            }
        }

        private void LoverUpdate(S.LoverUpdate p)
        {
            GameScene.Scene.RelationshipDialog.LoverName = p.Name;
            GameScene.Scene.RelationshipDialog.Date = p.Date;
            GameScene.Scene.RelationshipDialog.MapName = p.MapName;
            GameScene.Scene.RelationshipDialog.MarriedDays = p.MarriedDays;
            GameScene.Scene.RelationshipDialog.UpdateInterface();
        }

        private void MentorUpdate(S.MentorUpdate p)
        {
            GameScene.Scene.MentorDialog.MentorName = p.Name;
            GameScene.Scene.MentorDialog.MentorLevel = p.Level;
            GameScene.Scene.MentorDialog.MentorOnline = p.Online;
            GameScene.Scene.MentorDialog.MenteeEXP = p.MenteeEXP;

            GameScene.Scene.MentorDialog.UpdateInterface();
        }

        private void GameShopUpdate(S.GameShopInfo p)
        {
            p.Item.Stock = p.StockLevel;
            GameShopInfoList.Add(p.Item);
            if (p.Item.Date > DateTime.Now.AddDays(-7)) GameShopDialog.New.Visible = true;
        }

        private void GameShopStock(S.GameShopStock p)
        {
            for (int i = 0; i < GameShopInfoList.Count; i++)
            {
                if (GameShopInfoList[i].GIndex == p.GIndex)
                    {
                    if (p.StockLevel == 0) GameShopInfoList.Remove(GameShopInfoList[i]);
                    else GameShopInfoList[i].Stock = p.StockLevel;

                    if (GameShopDialog.Visible) GameShopDialog.UpdateShop();
                    }
            }
        }
        public void AddItem(UserItem item)
        {
            Redraw();

            if (item.Info.StackSize > 1) //Stackable
            {
                for (int i = 0; i < User.Inventory.Length; i++)
                {
                    UserItem temp = User.Inventory[i];
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
                for (int i = 0; i < User.BeltIdx - 2; i++)
                {
                    if (User.Inventory[i] != null) continue;
                    User.Inventory[i] = item;
                    return;
                }
            }
            else if (item.Info.Type == ItemType.Amulet)
            {
                for (int i = 4; i < User.BeltIdx; i++)
                {
                    if (User.Inventory[i] != null) continue;
                    User.Inventory[i] = item;
                    return;
                }
            }
            else
            {
                for (int i = User.BeltIdx; i < User.Inventory.Length; i++)
                {
                    if (User.Inventory[i] != null) continue;
                    User.Inventory[i] = item;
                    return;
                }
            }

            for (int i = 0; i < User.Inventory.Length; i++)
            {
                if (User.Inventory[i] != null) continue;
                User.Inventory[i] = item;
                return;
            }
        }

        public static void Bind(UserItem item)
        {
            if (!ItemInfoList.ContainsKey(item.ItemIndex))
                return;

            item.Info = ItemInfoList[item.ItemIndex];

            item.SetSlotSize();

            for (int s = 0; s < item.Slots.Length; s++)
            {
                if (item.Slots[s] == null) continue;

                Bind(item.Slots[s]);
            }

            for (int s = 0; s < item.RuneSlots.Length; s++)
            {
                if (item.RuneSlots[s] == null) continue;

                Bind(item.RuneSlots[s].SocketedItem);
            }
        }

        public static void BindQuest(ClientQuestProgress quest)
        {
            for (int i = 0; i < QuestInfoList.Count; i++)
            {
                if (QuestInfoList[i].Index != quest.Id) continue;

                quest.QuestInfo = QuestInfoList[i];

                return;
            }
        }

        public static Color GradeNameColor(ItemGrade grade)
        {
            switch (grade)
            {
                case ItemGrade.Common:
                    return Color.Yellow;
                case ItemGrade.Rare:
                    return Color.DeepSkyBlue;
                case ItemGrade.Legendary:
                    return Color.DarkOrange;
                case ItemGrade.Mythical:
                    return Color.Plum;
                default:
                    return Color.Yellow;
            }
        }

 
        public void DisposeMailLabel()
        {
            if (MailLabel != null && !MailLabel.IsDisposed)
                MailLabel.Dispose();
            MailLabel = null;
        }
        public void DisposeMemoLabel()
        {
            if (MemoLabel != null && !MemoLabel.IsDisposed)
                MemoLabel.Dispose();
            MemoLabel = null;
        }
        public void DisposeGuildBuffLabel()
        {
            if (GuildBuffLabel != null && !GuildBuffLabel.IsDisposed)
                GuildBuffLabel.Dispose();
            GuildBuffLabel = null;
        }

   
        public void CreateMailLabel(ClientMail mail)
        {
            if (mail == null)
            {
                DisposeMailLabel();
                return;
            }

            if (MailLabel != null && !MailLabel.IsDisposed) return;

            MailLabel = new MirControl
            {
                BackColour = Color.FromArgb(255, 50, 50, 50),
                Border = true,
                BorderColour = Color.Gray,
                DrawControlTexture = true,
                NotControl = true,
                Parent = this,
                Opacity = 0.7F
            };

            MirLabel nameLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.Yellow,
                Location = new Point(4, 4),
                OutLine = true,
                Parent = MailLabel,
                Text = mail.SenderName
            };

            MailLabel.Size = new Size(Math.Max(MailLabel.Size.Width, nameLabel.DisplayRectangle.Right + 4),
                Math.Max(MailLabel.Size.Height, nameLabel.DisplayRectangle.Bottom));

            MirLabel dateLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.White,
                Location = new Point(4, MailLabel.DisplayRectangle.Bottom),
                OutLine = true,
                Parent = MailLabel,
                Text = "Date Sent : " + mail.DateSent.ToString("dd/MM/yy H:mm:ss")
            };

            MailLabel.Size = new Size(Math.Max(MailLabel.Size.Width, dateLabel.DisplayRectangle.Right + 4),
                Math.Max(MailLabel.Size.Height, dateLabel.DisplayRectangle.Bottom));

            if (mail.Gold > 0)
            {
                MirLabel goldLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.White,
                    Location = new Point(4, MailLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = MailLabel,
                    Text = CMain.Tr("Gold: " + mail.Gold)
                };

                MailLabel.Size = new Size(Math.Max(MailLabel.Size.Width, goldLabel.DisplayRectangle.Right + 4),
                Math.Max(MailLabel.Size.Height, goldLabel.DisplayRectangle.Bottom));
            }

            MirLabel openedLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.Red,
                Location = new Point(4, MailLabel.DisplayRectangle.Bottom),
                OutLine = true,
                Parent = MailLabel,
                Text = mail.Opened ? CMain.Tr("[Old]") : CMain.Tr("[New]")
            };

            MailLabel.Size = new Size(Math.Max(MailLabel.Size.Width, openedLabel.DisplayRectangle.Right + 4),
            Math.Max(MailLabel.Size.Height, openedLabel.DisplayRectangle.Bottom));
        }
        public void CreateMemoLabel(ClientFriend friend)
        {
            if (friend == null)
            {
                DisposeMemoLabel();
                return;
            }

            if (MemoLabel != null && !MemoLabel.IsDisposed) return;

            MemoLabel = new MirControl
            {
                BackColour = Color.FromArgb(255, 50, 50, 50),
                Border = true,
                BorderColour = Color.Gray,
                DrawControlTexture = true,
                NotControl = true,
                Parent = this,
                Opacity = 0.7F
            };

            MirLabel memoLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.White,
                Location = new Point(4, 4),
                OutLine = true,
                Parent = MemoLabel,
                Text = Functions.StringOverLines(friend.Memo, 5, 20)
            };

            MemoLabel.Size = new Size(Math.Max(MemoLabel.Size.Width, memoLabel.DisplayRectangle.Right + 4),
                Math.Max(MemoLabel.Size.Height, memoLabel.DisplayRectangle.Bottom + 4));
        }

        public static ItemInfo GetInfo(int index)
        {
            if (ItemInfoList.ContainsKey(index))
                return ItemInfoList[index];

            return null;
        }

        public string GetUserName(uint id)
        {
            for (int i = 0; i < UserIdList.Count; i++)
            {
                UserId who = UserIdList[i];
                if (id == who.Id)
                    return who.UserName;
            }
            Network.Enqueue(new C.RequestUserName { UserID = id });
            UserIdList.Add(new UserId() { Id = id, UserName = CMain.Tr("Unknown") });
            return "";
        }

        public class OutPutMessage
        {
            public string Message;
            public long ExpireTime;
            public OutputMessageType Type;
        }

        public void Rankings(S.Rankings p)
        {
            RankingDialog.RecieveRanks(p.Listings, p.RankType, p.MyRank);
        }

        public void Opendoor(S.Opendoor p)
        {
            MapControl.OpenDoor(p.DoorIndex, p.Close);
        }

        private void RentedItems(S.GetRentedItems p)
        {
            ItemRentalDialog.ReceiveRentedItems(p.RentedItems);
        }

        private void ItemRentalRequest(S.ItemRentalRequest p)
        {
            if (!p.Renting)
            {
                GuestItemRentDialog.SetGuestName(p.Name);
                ItemRentingDialog.OpenItemRentalDialog();
            }
            else
            {
                GuestItemRentingDialog.SetGuestName(p.Name);
                ItemRentDialog.OpenItemRentDialog();
            }

            ItemRentalDialog.Visible = false;
        }

        private void ItemRentalFee(S.ItemRentalFee p)
        {
            GuestItemRentDialog.SetGuestFee(p.Amount);
            ItemRentDialog.RefreshInterface();
        }

        private void ItemRentalPeriod(S.ItemRentalPeriod p)
        {
            GuestItemRentingDialog.GuestRentalPeriod = p.Days;
            ItemRentingDialog.RefreshInterface();
        }

        private void DepositRentalItem(S.DepositRentalItem p)
        {
            var fromCell = p.From < User.BeltIdx ? BeltDialog.Grid[p.From] : InventoryDialog.Grid[p.From - User.BeltIdx];
            var toCell = ItemRentingDialog.ItemCell;

            if (toCell == null || fromCell == null)
                return;
 
            toCell.Locked = false;
            fromCell.Locked = false;
         
            if (!p.Success)
                return;

            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();

            if (ItemRentingDialog.RentalPeriod == 0)
                ItemRentingDialog.InputRentalPeroid();
        }

        private void RetrieveRentalItem(S.RetrieveRentalItem p)
        {
            var fromCell = ItemRentingDialog.ItemCell;
            var toCell = p.To < User.BeltIdx ? BeltDialog.Grid[p.To] : InventoryDialog.Grid[p.To - User.BeltIdx];

            if (toCell == null || fromCell == null)
                return;

            toCell.Locked = false;
            fromCell.Locked = false;

            if (!p.Success)
                return;

            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            User.RefreshStats();
        }

        private void UpdateRentalItem(S.UpdateRentalItem p)
        {
            GuestItemRentingDialog.GuestLoanItem = p.LoanItem;
            ItemRentDialog.RefreshInterface();
        }

        private void CancelItemRental(S.CancelItemRental p)
        {
            User.RentalGoldLocked = false;
            User.RentalItemLocked = false;

            ItemRentingDialog.Reset();
            ItemRentDialog.Reset();

            var messageBox = new MirMessageBox(CMain.Tr("Item rental cancelled.\r\n") +
                                               CMain.Tr("To complete item rental please face the other party throughout the transaction."));
            messageBox.Show();
        }

        private void ItemRentalLock(S.ItemRentalLock p)
        {
            if (!p.Success)
                return;
            
            User.RentalGoldLocked = p.GoldLocked;
            User.RentalItemLocked = p.ItemLocked;

            if (User.RentalGoldLocked)
                ItemRentDialog.Lock();
            else if (User.RentalItemLocked)
                ItemRentingDialog.Lock();
        }

        private void ItemRentalPartnerLock(S.ItemRentalPartnerLock p)
        {
            if (p.GoldLocked)
                GuestItemRentDialog.Lock();
            else if (p.ItemLocked)
                GuestItemRentingDialog.Lock();
        }

        private void CanConfirmItemRental(S.CanConfirmItemRental p)
        {
            ItemRentingDialog.EnableConfirmButton();
        }

        private void ConfirmItemRental(S.ConfirmItemRental p)
        {
            User.RentalGoldLocked = false;
            User.RentalItemLocked = false;

            ItemRentingDialog.Reset();
            ItemRentDialog.Reset();
        }

        public void DebugOutPut(string msg)
        {
            if (!Settings.DebugMode)
                return;

            ChatDialog.ReceiveChat(msg, ChatType.Hint);
        }

        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Scene = null;
                User = null;

                MoveTime = 0;
                AttackTime = 0;
                NextRunTime = 0;
                LastRunTime = 0;
                CanMove = false;
                CanRun = false;

                MapControl = null;
                MainDialog = null;
                ChatDialog = null;
                ChatControl = null;
                InventoryDialog = null;
                CharacterDialog = null;
                StorageDialog = null;
                BeltDialog = null;
                MiniMapDialog = null;
                InspectDialog = null;
                OptionDialog = null;
                MenuDialog = null;
                NPCDialog = null;
                QuestDetailDialog = null;
                QuestListDialog = null;
                QuestLogDialog = null;
                QuestTrackingDialog = null;
                GameShopDialog = null;
                MentorDialog = null;

                RelationshipDialog = null;
                CharacterDuraPanel = null;
                DuraStatusPanel = null;

                HoverItem = null;
                SelectedCell = null;
                PickedUpGold = false;

                UseItemTime = 0;
                PickUpTime = 0;
                InspectTime = 0;

                DisposeItemLabel();

                AMode = 0;
                PMode = 0;
                Lights = 0;

                NPCTime = 0;
                NPCID = 0;
                DefaultNPCID = 0;

                for (int i = 0; i < OutputLines.Length; i++)
                    if (OutputLines[i] != null && OutputLines[i].IsDisposed)
                        OutputLines[i].Dispose();

                OutputMessages.Clear();
                OutputMessages = null;
            }

            base.Dispose(disposing);
        }

        #endregion

    }

}

