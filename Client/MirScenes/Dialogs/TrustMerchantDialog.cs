using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C = ClientPackets;
using Client.MirObjects;

namespace Client.MirScenes.Dialogs
{
    public sealed class TrustMerchantDialog : MirImageControl
    {
        public static bool UserMode = false;
        public static string[] CurrencyTypes = { "金币", "元宝" };

        public uint Amount = 0, MinAmount = Globals.MinConsignment, MaxAmount = Globals.MaxConsignment;

        public static long SearchTime, MarketTime;

        public MirTextBox SearchTextBox, PriceTextBox;
        public MirButton FindButton, RefreshButton, MailButton, BuyButton, CloseButton, NextButton, BackButton;
        public MirImageControl TitleLabel;
        public MirLabel ItemLabel, PriceLabel, SellerLabel, PageLabel;
        public MirLabel DateLabel, ExpireLabel;
        public MirLabel NameLabel, TotalPriceLabel, SplitPriceLabel;

        public MirItemCell ItemCell, tempCell;
        public static UserItem SellItemSlot;
        public MirButton SellItemButton;

        public List<ClientAuction> Listings = new List<ClientAuction>();

        public int Page, PageCount;
        public static AuctionRow Selected;
        public AuctionRow[] Rows = new AuctionRow[10];

        public MirButton UpButton, DownButton, PositionBar;
        public MirButton MarketButton, ConsignmentButton;
        public MirImageControl MarketPage, ConsignmentPage;

        public MirImageControl FilterBox, FilterBackground;
        public MirButton ShowAllButton, WeaponButton, DraperyItemsButton, AccessoriesItemsButton, ConsumableItemsButton;
        public MirButton EnhEquipButton, BooksButton, CraftingSystemButton, PetsItemButton, GradeButton;

        public MirLabel ShowAllLabel, WeaponLabel, DraperyItemsLabel, AccessoriesItemsLabel, ConsumableItemsLabel;
        public MirLabel EnhEquipLabel, BooksLabel, CraftingSystemLabel, PetsItemLabel, GButtonLabel;

        public MirButton ArmoursSubBtn, HelmetsSubBtn, BeltsSubBtn, BootsSubBtn, StonesSubBtn;// Drapery Items
        public MirButton NecklaceSubBtn, BraceletsSubBtn, RingsSubBtn;// Accessories Items
        public MirButton RecoveryPotionSubBtn, PowerUpSubBtn, ScrollSubBtn, ScriptSubBtn; //Consumable Items
        public MirButton GemSubBtn, OrbSubBtn, AwakeSubBtn; //Enhanced Equipment
        public MirButton[] BookSubBtn = new MirButton[6];
        public MirButton MaterialsSubBtn, FishSubBtn, MeatSubBtn, OreSubBtn; //Crafting System
        public MirButton NoveltyPetsSubBtn, NoveltyEquipmentSubBtn, MountsSubBtn, ReinsSubBtn, BellsSubBtn, RibbonSubBtn, MaskSubBtn; //Pets
        public MirButton CommonBtn, RareBtn, LegendaryBtn, MythicalBtn;//Item Grade

        public MirLabel ArmoursLabel, HelmetsLabel, BeltsLabel, BootsLabel, StonesLabel;// Drapery Items
        public MirLabel NecklaceLabel, BraceletsLabel, RingsLabel;// Accessories Items
        public MirLabel RecoveryPotionLabel, PowerUpLabel, ScrollLabel, ScriptLabel; //Consumable Items
        public MirLabel GemLabel, OrbLabel, AwakeLabel; //Enhanced Equipment
        public MirLabel[] BookSubLabel = new MirLabel[6];
        public MirLabel MaterialsLabel, FishLabel, MeatLabel, OreLabel; //Crafting System
        public MirLabel NoveltyPetsLabel, NoveltyEquipmentLabel, MountsLabel, ReinsLabel, BellsLabel, RibbonLabel, MaskLabel; //Pets
        public MirLabel CommonLabel, RareLabel, LegendaryLabel, MythicalLabel, UncommonLabel, SetLabel, UniqueLabel;//Item Grade

        public MirLabel totalGold;

        public MirDropDownBox CurrencyTypeBox;

        public TrustMerchantDialog()
        {
            Index = 786;
            Library = Libraries.Title;
            Sort = true;

            #region TrustMerchant Buttons

            MarketButton = new MirButton
            {
                Index = 789,
                PressedIndex = 788,
                Library = Libraries.Title,
                Location = new Point(9, 32),
                Parent = this,
            };
            MarketButton.Click += (o, e) =>
            {
                TMerchantDialog(0);
                if (tempCell != null)
                {
                    tempCell.Locked = false;
                    SellItemSlot = null;
                    tempCell = null;
                }
            };
            ConsignmentButton = new MirButton
            {
                Index = 791,
                PressedIndex = 790,
                Library = Libraries.Title,
                Location = new Point(105, 32),
                Parent = this,
                Visible = true,
            };
            ConsignmentButton.Click += (o, e) =>
            {
                TMerchantDialog(1);
            };
            CloseButton = new MirButton
            {
                Index = 360,
                HoverIndex = 361,
                PressedIndex = 362,
                Location = new Point(465, 3),
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            CloseButton.Click += (o, e) => Hide();

            #region Page Buttons & Label

            BackButton = new MirButton
            {
                Index = 240,
                HoverIndex = 241,
                PressedIndex = 242,
                Library = Libraries.Prguse2,
                Location = new Point(253, 420),
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            BackButton.Click += (o, e) =>
            {
                if (Page <= 0) return;

                Page--;
                UpdateInterface();
            };
            NextButton = new MirButton
            {
                Index = 243,
                HoverIndex = 244,
                PressedIndex = 245,
                Library = Libraries.Prguse2,
                Location = new Point(322, 420),
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            NextButton.Click += (o, e) =>
            {
                if (Page >= PageCount - 1) return;
                if (Page < (Listings.Count - 1) / 10)
                {
                    Page++;
                    UpdateInterface();
                    return;
                }
                Network.Enqueue(new C.MarketPage { Page = Page + 1 });
            };
            PageLabel = new MirLabel
            {
                Size = new Size(60, 20),
                Location = new Point(265, 421),
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Text = "0/0",
                NotControl = true,
                Parent = this,
            };

            #endregion
            #endregion

            MarketPage = new MirImageControl()
            {
                Index = 786,
                Library = Libraries.Title,
                Visible = false,
                Sort = true,
            };
            ConsignmentPage = new MirImageControl()
            {
                Index = 787,
                Library = Libraries.Title,
                Visible = false,
                Sort = true,
            };

            #region Filter Buttons

            ShowAllButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(1, 67),
                Parent = this,
            };
            ShowAllButton.Click += (o, e) => SwitchTab(0);
            ShowAllLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = ShowAllButton,
                Text = CMain.Tr("Show All Items"),
                NotControl = true,
            };
            WeaponButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ShowAllButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            WeaponButton.Click += (o, e) => SwitchTab(1);
            WeaponLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = WeaponButton,
                Text = CMain.Tr("Weapon Items"),
                NotControl = true,
            };
            DraperyItemsButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, WeaponButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            DraperyItemsButton.Click += (o, e) => SwitchTab(2);
            DraperyItemsLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = DraperyItemsButton,
                Text = CMain.Tr("Drapery Items"),
                NotControl = true,
            };

            #region Drapery Filtering.

            ArmoursSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, DraperyItemsButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            ArmoursSubBtn.Click += (o, e) => SwitchTab(9);
            ArmoursLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = ArmoursSubBtn,
                Text = CMain.Tr("Amours"),
                NotControl = true,
            };
            HelmetsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ArmoursSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            HelmetsSubBtn.Click += (o, e) => SwitchTab(10);
            HelmetsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = HelmetsSubBtn,
                Text = CMain.Tr("Helmets"),
                NotControl = true,
            };
            BeltsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, HelmetsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            BeltsSubBtn.Click += (o, e) => SwitchTab(11);
            BeltsLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(16, 1),
                Parent = BeltsSubBtn,
                Text = CMain.Tr("Belts"),
                NotControl = true,
            };
            BootsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, BeltsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            BootsSubBtn.Click += (o, e) => SwitchTab(12);
            BootsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = BootsSubBtn,
                Text = CMain.Tr("Boots"),
                NotControl = true,
            };
            StonesSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, BootsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            StonesSubBtn.Click += (o, e) => SwitchTab(13);
            StonesLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = StonesSubBtn,
                Text = CMain.Tr("Stones"),
                NotControl = true,
            };
            #endregion

            AccessoriesItemsButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, DraperyItemsButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            AccessoriesItemsButton.Click += (o, e) => SwitchTab(3);
            AccessoriesItemsLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = AccessoriesItemsButton,
                Text = CMain.Tr("Accessory Items"),
                NotControl = true,
            };

            #region Accessories Items
            NecklaceSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, AccessoriesItemsButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            NecklaceSubBtn.Click += (o, e) => SwitchTab(14);
            NecklaceLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = NecklaceSubBtn,
                Text = CMain.Tr("Necklaces"),
                NotControl = true,
            };
            BraceletsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, NecklaceSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            BraceletsSubBtn.Click += (o, e) => SwitchTab(15);
            BraceletsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = BraceletsSubBtn,
                Text = CMain.Tr("Bracelets"),
                NotControl = true,
            };
            RingsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, BraceletsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            RingsSubBtn.Click += (o, e) => SwitchTab(16);
            RingsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = RingsSubBtn,
                Text = CMain.Tr("Rings"),
                NotControl = true,
            };
            #endregion

            ConsumableItemsButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, AccessoriesItemsButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            ConsumableItemsButton.Click += (o, e) => SwitchTab(4);
            ConsumableItemsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(2, 1),
                Parent = ConsumableItemsButton,
                Text = CMain.Tr("Cosumable Items"),
                NotControl = true,
            };

            #region Consumable Items
            RecoveryPotionSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ConsumableItemsButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            RecoveryPotionSubBtn.Click += (o, e) => SwitchTab(17);
            RecoveryPotionLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = RecoveryPotionSubBtn,
                Text = CMain.Tr("Recovery Pots"),
                NotControl = true,
            };
            PowerUpSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, RecoveryPotionSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            PowerUpSubBtn.Click += (o, e) => SwitchTab(18);
            PowerUpLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = PowerUpSubBtn,
                Text = CMain.Tr("Buff Pots"),
                NotControl = true,
            };
            ScrollSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, PowerUpSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            ScrollSubBtn.Click += (o, e) => SwitchTab(19);
            ScrollLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = ScrollSubBtn,
                Text = CMain.Tr("Scrolls / Oils"),
                NotControl = true,
            };
            ScriptSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ScrollSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            ScriptSubBtn.Click += (o, e) => SwitchTab(20);
            ScriptLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = ScriptSubBtn,
                Text = CMain.Tr("Misc Items"),
                NotControl = true,
            };
            #endregion

            EnhEquipButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ConsumableItemsButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            EnhEquipButton.Click += (o, e) => SwitchTab(5);
            EnhEquipLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(2, 1),
                Parent = EnhEquipButton,
                Text = CMain.Tr("Enhancment Items"),
                NotControl = true,
            };

            #region Enhancing Equipment
            GemSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, EnhEquipButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            GemSubBtn.Click += (o, e) => SwitchTab(21);
            GemLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = GemSubBtn,
                Text = CMain.Tr("Gems"),
                NotControl = true,
            };
            OrbSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, GemSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            OrbSubBtn.Click += (o, e) => SwitchTab(22);

            OrbLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = OrbSubBtn,
                Text = CMain.Tr("Orbs"),
                NotControl = true,
            };
            AwakeSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, OrbSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            AwakeSubBtn.Click += (o, e) => SwitchTab(23);
            AwakeLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = AwakeSubBtn,
                Text = CMain.Tr("Awakening"),
                NotControl = true,
            };
            #endregion

            BooksButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, EnhEquipButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            BooksButton.Click += (o, e) => SwitchTab(6);
            BooksLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = BooksButton,
                Text = CMain.Tr("Books"),
                NotControl = true,
            };

            #region Class Books
            for (int i=0; i< BookSubBtn.Length; ++i)
            {
                int y = i == 0 ? BooksButton.Location.Y + 20 : BookSubBtn[i - 1].Location.Y + 20;
                int index = i;
                BookSubBtn[i] = new MirButton
                {
                    Index = 917,
                    PressedIndex = 916,
                    HoverIndex = 916,
                    Library = Libraries.Prguse2,
                    Sound = SoundList.ButtonA,
                    Location = new Point(10, y),
                    Parent = this,
                    Visible = false,
                };
                BookSubBtn[i].Click += (o, e) => SwitchBookTab(index);
                BookSubLabel[i] = new MirLabel
                {
                    Size = new Size(100, 18),
                    Location = new Point(16, 1),
                    Parent = BookSubBtn[i],
                    Text = CMain.Tr(((MirClass)i).ToString()),
                    NotControl = true,
                };
            }

            #endregion

            CraftingSystemButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, BooksButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            CraftingSystemButton.Click += (o, e) => SwitchTab(7);
            CraftingSystemLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = CraftingSystemButton,
                Text = CMain.Tr("Crafting Items"),
                NotControl = true,
            };

            #region Crafting System (CraftingMaterials)
            MaterialsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, CraftingSystemButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            MaterialsSubBtn.Click += (o, e) => SwitchTab(29);
            MaterialsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = MaterialsSubBtn,
                Text = CMain.Tr("Materials"),
                NotControl = true,
            };
            FishSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, MaterialsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            FishSubBtn.Click += (o, e) => SwitchTab(30);
            FishLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = FishSubBtn,
                Text = CMain.Tr("Fish"),
                NotControl = true,
            };
            MeatSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, FishSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            MeatSubBtn.Click += (o, e) => SwitchTab(31);
            MeatLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = MeatSubBtn,
                Text = CMain.Tr("Meat"),
                NotControl = true,
            };
            OreSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, MeatSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            OreSubBtn.Click += (o, e) => SwitchTab(32);
            OreLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = OreSubBtn,
                Text = CMain.Tr("Ore"),
                NotControl = true,
            };
            #endregion

            PetsItemButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, CraftingSystemButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            PetsItemButton.Click += (o, e) => SwitchTab(8);
            PetsItemLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = PetsItemButton,
                Text = CMain.Tr("Pet Equipment"),
                NotControl = true,
            };

            #region Pets & Mounts
            NoveltyPetsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, PetsItemButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            NoveltyPetsSubBtn.Click += (o, e) => SwitchTab(33);
            NoveltyPetsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = NoveltyPetsSubBtn,
                Text = CMain.Tr("Novelty Pets"),
                NotControl = true,
            };
            NoveltyEquipmentSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, NoveltyPetsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            NoveltyEquipmentSubBtn.Click += (o, e) => SwitchTab(34);
            NoveltyEquipmentLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = NoveltyEquipmentSubBtn,
                Text = CMain.Tr("Novelty Equip"),
                NotControl = true,
            };
            MountsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, NoveltyEquipmentSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            MountsSubBtn.Click += (o, e) => SwitchTab(35);
            MountsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = MountsSubBtn,
                Text = CMain.Tr("Mounts"),
                NotControl = true,
            };
            ReinsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, MountsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            ReinsSubBtn.Click += (o, e) => SwitchTab(36);
            ReinsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = ReinsSubBtn,
                Text = CMain.Tr("Reins"),
                NotControl = true,
            };
            BellsSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, ReinsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            BellsSubBtn.Click += (o, e) => SwitchTab(37);
            BellsLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = BellsSubBtn,
                Text = CMain.Tr("Bells"),
                NotControl = true,
            };
            RibbonSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, BellsSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            RibbonSubBtn.Click += (o, e) => SwitchTab(38);
            RibbonLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = RibbonSubBtn,
                Text = CMain.Tr("Ribbons"),
                NotControl = true,
            };
            MaskSubBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, RibbonSubBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            MaskSubBtn.Click += (o, e) => SwitchTab(39);
            MaskLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = MaskSubBtn,
                Text = CMain.Tr("Mask"),
                NotControl = true,
            };
            #endregion

            GradeButton = new MirButton
            {
                Index = 915,
                PressedIndex = 914,
                HoverIndex = 914,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, PetsItemButton.Location.Y + 20),
                Parent = this,
                Visible = true,
            };
            GradeButton.Click += (o, e) => SwitchTab(40);
            GButtonLabel = new MirLabel
            {
                Size = new Size(99, 18),
                Location = new Point(2, 1),
                Parent = GradeButton,
                Text = CMain.Tr("Item Grades"),
                NotControl = true,
            };
            #region ItemGrades
            CommonBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, GradeButton.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            CommonBtn.Click += (o, e) => SwitchTab(41);
            CommonLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = CommonBtn,
                Text = CMain.Tr("Common"),
                NotControl = true,
            };
            RareBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, CommonBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            RareBtn.Click += (o, e) => SwitchTab(42);
            RareLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = RareBtn,
                Text = CMain.Tr("Rare"),
                NotControl = true,
            };
            LegendaryBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, RareBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            LegendaryBtn.Click += (o, e) => SwitchTab(43);
            LegendaryLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = LegendaryBtn,
                Text = CMain.Tr("Legendary"),
                NotControl = true,
            };
            MythicalBtn = new MirButton
            {
                Index = 917,
                PressedIndex = 916,
                HoverIndex = 916,
                Library = Libraries.Prguse2,
                Sound = SoundList.ButtonA,
                Location = new Point(10, LegendaryBtn.Location.Y + 20),
                Parent = this,
                Visible = false,
            };
            MythicalBtn.Click += (o, e) => SwitchTab(44);
            MythicalLabel = new MirLabel
            {
                Size = new Size(100, 18),
                Location = new Point(16, 1),
                Parent = MythicalBtn,
                Text = CMain.Tr("Mythical"),
                NotControl = true,
            };
            #endregion

            #endregion

            #region Market Buttons

            MailButton = new MirButton
            {
                Index = 437,
                HoverIndex = 438,
                PressedIndex = 439,
                Library = Libraries.Prguse,
                Location = new Point(202, 448),
                Sound = SoundList.ButtonA,
                Visible = false,
                Parent = this,
            };
            MailButton.Click += (o, e) =>
            {
                if (Selected == null || CMain.Time < MarketTime) return;

                MirMessageBox box = new MirMessageBox(string.Format("Are you sure you want to buy {0} for {1}?", Selected.Listing.Item.FriendlyName, Selected.Listing.Price), MirMessageBoxButtons.YesNo);
                box.YesButton.Click += (o1, e2) =>
                {
                    MarketTime = CMain.Time + 3000;
                    Network.Enqueue(new C.MarketBuy { AuctionID = Selected.Listing.AuctionID, MailItems = true });
                };
                box.Show();
            };
            RefreshButton = new MirButton
            {
                Index = 663,
                HoverIndex = 664,
                PressedIndex = 665,
                Library = Libraries.Prguse,
                Location = new Point(174, 448),
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            RefreshButton.Click += (o, e) =>
            {
                if (CMain.Time < SearchTime)
                {
                    GameScene.Scene.ChatDialog.ReceiveChat(string.Format(CMain.Tr("You can search again after {0} seconds."), Math.Ceiling((SearchTime - CMain.Time) / 1000D)), ChatType.System);
                    return;
                }
                SearchTime = CMain.Time + Globals.SearchDelay;
                SearchTextBox.Text = string.Empty;
                Network.Enqueue(new C.MarketRefresh());
            };
            BuyButton = new MirButton
            {
                Index = 703,
                HoverIndex = 704,
                PressedIndex = 705,
                Library = Libraries.Title,
                Location = new Point(300, 448),
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            BuyButton.Click += (o, e) =>
            {
                if (Selected == null || CMain.Time < MarketTime) return;

                if (UserMode)
                {
                    if (Selected.Listing.Seller == "For Sale")
                    {
                        MirMessageBox box = new MirMessageBox(string.Format(CMain.Tr(CMain.Tr("{0} has not sold, Are you sure you want to get it back?")), Selected.Listing.Item.FriendlyName), MirMessageBoxButtons.YesNo);
                        box.YesButton.Click += (o1, e2) =>
                        {
                            MarketTime = CMain.Time + 3000;
                            Network.Enqueue(new C.MarketGetBack { AuctionID = Selected.Listing.AuctionID });
                        };
                        box.Show();
                    }
                    else
                    {
                        MarketTime = CMain.Time + 3000;
                        Network.Enqueue(new C.MarketGetBack { AuctionID = Selected.Listing.AuctionID });
                    }

                }
                else
                {
                    MirMessageBox box = new MirMessageBox(string.Format(CMain.Tr("Are you sure you want to buy {0} for {1}?"), Selected.Listing.Item.FriendlyName, Selected.Listing.Price), MirMessageBoxButtons.YesNo);
                    box.YesButton.Click += (o1, e2) =>
                    {
                        MarketTime = CMain.Time + 3000;
                        Network.Enqueue(new C.MarketBuy { AuctionID = Selected.Listing.AuctionID, MailItems = false });
                    };
                    box.Show();
                }
            };
            #endregion

            #region Search

            SearchTextBox = new MirTextBox
            {
                Location = new Point(335, 35),
                Size = new Size(140, 1),
                MaxLength = 20,
                Parent = this,
                CanLoseFocus = true,
            };
            SearchTextBox.TextBox.KeyPress += SearchTextBox_KeyPress;
            SearchTextBox.TextBox.KeyUp += SearchTextBox_KeyUp;
            SearchTextBox.TextBox.KeyDown += SearchTextBox_KeyDown;
            FindButton = new MirButton
            {
                Index = 480,
                HoverIndex = 481,
                PressedIndex = 482,
                Library = Libraries.Title,
                Location = new Point(124, 448),
                Sound = SoundList.ButtonA,
                Parent = this,
            };
            FindButton.Click += (o, e) =>
            {
                if (String.IsNullOrEmpty(SearchTextBox.Text)) return;
                if (CMain.Time < SearchTime)
                {
                    GameScene.Scene.ChatDialog.ReceiveChat(string.Format("You can search again after {0} seconds.", Math.Ceiling((SearchTime - CMain.Time) / 1000D)), ChatType.System);
                    return;
                }

                SearchTime = CMain.Time + Globals.SearchDelay;
                Network.Enqueue(new C.MarketSearch
                {
                    Match = SearchTextBox.Text,
                });
            };

            #endregion

            #region Gold Label

            totalGold = new MirLabel
            {
                Size = new Size(100, 20),
                DrawFormat = TextFormatFlags.RightToLeft | TextFormatFlags.Right,

                Location = new Point(6, 452),
                Parent = this,
                NotControl = true,
                Font = new Font(Settings.FontName, 8F),
            };
            #endregion

            #region ItemCell

            ItemCell = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.TrustMerchant,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(47, 104),
                ItemSlot = 0,
                Visible = false
            };
            ItemCell.Click += (o, e) => ItemCell_Click();
            PriceTextBox = new MirTextBox
            {
                Location = new Point(15, 168),
                Size = new Size(100, 1),
                MaxLength = 20,
                Parent = this,
                CanLoseFocus = true,
                Visible = false,
                Font = new Font(Settings.FontName, 8F)
            };
            PriceTextBox.TextBox.TextChanged += TextBox_TextChanged;
            PriceTextBox.TextBox.KeyPress += MirInputBox_KeyPress;
            SellItemButton = new MirButton
            {
                Index = 700,
                PressedIndex = 702,
                HoverIndex = 701,
                Library = Libraries.Title,
                Sound = SoundList.ButtonA,
                Location = new Point(47, 206),
                Parent = this,
                Visible = false,
                Enabled = false
            };
            SellItemButton.Click += (o, e) =>
            {
                Network.Enqueue(new C.ConsignItem { UniqueID = SellItemSlot.UniqueID, Price = Amount, CurrencyType = (byte)CurrencyTypeBox.SelectedIndex });
                SellItemSlot = null;
                PriceTextBox.Text = null;
                SellItemButton.Enabled = false;
                TMerchantDialog(1);
            };
            #endregion

            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new AuctionRow
                {
                    Location = new Point(128, 83 + i * 33),
                    Parent = this,
                };
                Rows[i].Click += (o, e) =>
                {
                    Selected = (AuctionRow)o;
                    UpdateInterface();
                };
            }

            CurrencyTypeBox = new MirDropDownBox
            {
                Parent = this,
                Location = new Point(15, 188),
                Size = new Size(93, 15),
                ForeColour = Color.White,
                Visible = true,
                Enabled = true,
                BackColour = Color.FromArgb(255, 25, 25, 25),
                BorderColour = Color.FromArgb(255, 35, 35, 35),
            };
            
            CurrencyTypeBox.Items = CurrencyTypes.ToList();
            CurrencyTypeBox.SelectedIndex = 0;
            CurrencyTypeBox.ValueChanged += (o, e) => OnCurrencySelect(CurrencyTypeBox._WantedIndex);
        }

        private AuctionRow GetNextSelect()
        {
            bool find = false;
            for (int i = 0; i < 10; i++)
            {
                if (Rows[i] == Selected)
                {
                    find = true;
                    continue;
                }

                if (find)
                    return Rows[i];
            }

            return null;
        }

        private void SwitchBookTab(int index)
        {
            for (int i=0; i<BookSubBtn.Length; ++i)
            {
                BookSubBtn[i].Index = index != i ? 917 : 916;
            }

            switch ((MirClass)index)
            {
                case MirClass.Warrior:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Warrior });
                    break;
                case MirClass.Wizard:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Wizard });
                    break;
                case MirClass.Taoist:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Taoist });
                    break;
                case MirClass.Assassin:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Assassin });
                    break;
                case MirClass.Archer:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Archer });
                    break;
                case MirClass.Monk:
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false, RequiredClass = RequiredClass.Monk });
                    break;
            }
        }

        private void OnCurrencySelect(int Index)
        {
            if (Index >= CurrencyTypes.Length)
                Index = CurrencyTypes.Length - 1;

            CurrencyTypeBox.SelectedIndex = Index;
        }

        public void UpdateInterface()
        {
            PageLabel.Text = string.Format("{0}/{1}", Page + 1, PageCount);
            totalGold.Text = GameScene.Gold.ToString("###,###,##0");

            for (int i = 0; i < 10; i++)
                if (i + Page * 10 >= Listings.Count)
                {
                    Rows[i].Clear();
                    if (Rows[i] == Selected) Selected = null;
                }
                else
                {
                    bool update = false;
                    if (Rows[i] == Selected && Selected.Listing != Listings[i + Page * 10])
                        update = true;

                    Rows[i].Update(Listings[i + Page * 10]);
                    if (update)
                        Selected = Rows[i];
                }

            for (int i = 0; i < Rows.Length; i++)
                Rows[i].Border = Rows[i] == Selected;
        }
        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (CMain.Time < SearchTime)
            {
                GameScene.Scene.ChatDialog.ReceiveChat(string.Format(CMain.Tr("You can search again after {0} seconds."), Math.Ceiling((SearchTime - CMain.Time) / 1000D)), ChatType.System);
                return;
            }
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    if (string.IsNullOrEmpty(SearchTextBox.Text)) return;
                    SearchTime = CMain.Time + Globals.SearchDelay;
                    Network.Enqueue(new C.MarketSearch
                    {
                        Match = SearchTextBox.Text,
                    });
                    Program.Form.ActiveControl = null;
                    break;
                case (char)Keys.Escape:
                    e.Handled = true;
                    break;
            }
        }
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            CMain.Shift = e.Shift;
            CMain.Alt = e.Alt;
            CMain.Ctrl = e.Control;

            switch (e.KeyCode)
            {
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.Tab:
                case Keys.Escape:
                    CMain.CMain_KeyUp(sender, e);
                    break;
            }
        }
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            CMain.Shift = e.Shift;
            CMain.Alt = e.Alt;
            CMain.Ctrl = e.Control;

            switch (e.KeyCode)
            {
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.Tab:
                case Keys.Escape:
                    CMain.CMain_KeyDown(sender, e);
                    break;
            }
        }
        public void TMerchantDialog(byte TMDid)
        {
            MarketButton.Index = 789;
            ConsignmentButton.Index = 791;
            setdefault();

            switch (TMDid)
            {
                case 0:
                    Index = 786;
                    MarketButton.Index = 788;
                    BuyButton.Index = 703;
                    BuyButton.HoverIndex = 704;
                    BuyButton.PressedIndex = 705;
                    SwitchTab(0);
                    setvalues(0);
                    Network.Enqueue(new C.MarketSearch
                    {
                        Match = "",
                        Type = ItemType.Nothing,
                        Usermode = false
                    });
                    break;
                case 1:
                    Index = 787;
                    BuyButton.Index = 706;
                    BuyButton.HoverIndex = 707;
                    BuyButton.PressedIndex = 708;
                    ConsignmentButton.Index = 790;
                    setvalues(1);
                    Network.Enqueue(new C.MarketSearch
                    {
                        Match = "",
                        Type = ItemType.Nothing,
                        Usermode = true
                    });
                    break;
            }
        }

        private void setvalues(byte i)
        {
            switch (i)
            {
                case 0:
                    FindButton.Visible = true;
                    SellItemButton.Visible = false;
                    ShowAllButton.Visible = true;
                    WeaponButton.Visible = true;
                    DraperyItemsButton.Visible = true;
                    AccessoriesItemsButton.Visible = true;
                    ConsumableItemsButton.Visible = true;
                    EnhEquipButton.Visible = true;
                    BooksButton.Visible = true;
                    MailButton.Visible = true;
                    CraftingSystemButton.Visible = true;
                    PetsItemButton.Visible = true;
                    GradeButton.Visible = true;
                    PriceTextBox.Visible = false;
                    ItemCell.Visible = false;
                    SellItemButton.Visible = false;
                    SearchTextBox.Visible = true;
                    RefreshButton.Visible = true;
                    totalGold.Visible = true;
                    CurrencyTypeBox.Visible = false;
                    break;
                case 1:
                    MailButton.Visible = false;
                    ShowAllButton.Visible = false;
                    WeaponButton.Visible = false;
                    DraperyItemsButton.Visible = false;
                    AccessoriesItemsButton.Visible = false;
                    ConsumableItemsButton.Visible = false;
                    EnhEquipButton.Visible = false;
                    BooksButton.Visible = false;
                    CraftingSystemButton.Visible = false;
                    PetsItemButton.Visible = false;
                    GradeButton.Visible = false;
                    PriceTextBox.Visible = true;
                    PriceTextBox.Text = null;
                    ItemCell.Visible = true;
                    SellItemButton.Visible = true;
                    SellItemButton.Enabled = false;
                    FindButton.Visible = false;
                    SearchTextBox.Visible = false;
                    RefreshButton.Visible = false;
                    totalGold.Visible = false;
                    CurrencyTypeBox.Visible = true;
                    break;
            }
        }
        public void SwitchTab(byte STabid)
        {

            switch (STabid)
            {
                case 0:
                    setdefault();
                    ShowAllButton.Index = 914;
                    SetLocations(0);
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Nothing, Usermode = false });
                    break;
                case 1:
                    setdefault();
                    WeaponButton.Index = 914;
                    SetLocations(1);
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Weapon, Usermode = false });
                    break;
                case 2:
                    setdefault();
                    DraperyItemsButton.Index = 914;
                    ArmoursSubBtn.Visible = true;
                    HelmetsSubBtn.Visible = true;
                    BeltsSubBtn.Visible = true;
                    BootsSubBtn.Visible = true;
                    StonesSubBtn.Visible = true;
                    SetLocations(2);
                    break;
                case 3:
                    setdefault();
                    AccessoriesItemsButton.Index = 914;
                    NecklaceSubBtn.Visible = true;
                    BraceletsSubBtn.Visible = true;
                    RingsSubBtn.Visible = true;
                    SetLocations(3);
                    break;
                case 4:
                    setdefault();
                    ConsumableItemsButton.Index = 914;
                    RecoveryPotionSubBtn.Visible = true;
                    PowerUpSubBtn.Visible = true;
                    ScrollSubBtn.Visible = true;
                    ScriptSubBtn.Visible = true;
                    SetLocations(4);
                    break;
                case 5:
                    setdefault();
                    EnhEquipButton.Index = 914;
                    GemSubBtn.Visible = true;
                    OrbSubBtn.Visible = true;
                    AwakeSubBtn.Visible = true;
                    SetLocations(5);
                    break;
                case 6:
                    setdefault();
                    BooksButton.Index = 914;
                    for (int i = 0; i < BookSubBtn.Length; ++i)
                        BookSubBtn[i].Visible = true;
                    SetLocations(6);
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Book, Usermode = false });
                    break;
                case 7:
                    setdefault();
                    CraftingSystemButton.Index = 914;
                    MaterialsSubBtn.Visible = true;
                    FishSubBtn.Visible = true;
                    MeatSubBtn.Visible = true;
                    OreSubBtn.Visible = true;
                    SetLocations(7);
                    break;
                case 8:
                    setdefault();
                    PetsItemButton.Index = 914;
                    NoveltyPetsSubBtn.Visible = true;
                    NoveltyEquipmentSubBtn.Visible = true;
                    MountsSubBtn.Visible = true;
                    ReinsSubBtn.Visible = true;
                    BellsSubBtn.Visible = true;
                    RibbonSubBtn.Visible = true;
                    MaskSubBtn.Visible = true;
                    SetLocations(8);
                    break;
                case 9:
                    ArmoursSubBtn.Index = 916;
                    HelmetsSubBtn.Index = 917;
                    BeltsSubBtn.Index = 917;
                    BootsSubBtn.Index = 917;
                    StonesSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Armour, Usermode = false });
                    break;
                case 10:
                    ArmoursSubBtn.Index = 917;
                    HelmetsSubBtn.Index = 916;
                    BeltsSubBtn.Index = 917;
                    BootsSubBtn.Index = 917;
                    StonesSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Helmet, Usermode = false });
                    break;
                case 11:
                    ArmoursSubBtn.Index = 917;
                    HelmetsSubBtn.Index = 917;
                    BeltsSubBtn.Index = 916;
                    BootsSubBtn.Index = 917;
                    StonesSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Belt, Usermode = false });
                    break;
                case 12:
                    ArmoursSubBtn.Index = 917;
                    HelmetsSubBtn.Index = 917;
                    BeltsSubBtn.Index = 917;
                    BootsSubBtn.Index = 916;
                    StonesSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Boots, Usermode = false });
                    break;
                case 13:
                    ArmoursSubBtn.Index = 917;
                    HelmetsSubBtn.Index = 917;
                    BeltsSubBtn.Index = 917;
                    BootsSubBtn.Index = 917;
                    StonesSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Stone, Usermode = false });
                    break;
                case 14:
                    NecklaceSubBtn.Index = 916;
                    BraceletsSubBtn.Index = 917;
                    RingsSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Necklace, Usermode = false });
                    break;
                case 15:
                    NecklaceSubBtn.Index = 917;
                    BraceletsSubBtn.Index = 916;
                    RingsSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Bracelet, Usermode = false });
                    break;
                case 16:
                    NecklaceSubBtn.Index = 917;
                    BraceletsSubBtn.Index = 917;
                    RingsSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Ring, Usermode = false });
                    break;
                case 17:
                    RecoveryPotionSubBtn.Index = 916;
                    PowerUpSubBtn.Index = 917;
                    ScrollSubBtn.Index = 917;
                    ScriptSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Potion, Usermode = false, MaxShape = 2 });
                    break;
                case 18:
                    RecoveryPotionSubBtn.Index = 917;
                    PowerUpSubBtn.Index = 916;
                    ScrollSubBtn.Index = 917;
                    ScriptSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Potion, Usermode = false, MinShape = 3, MaxShape = 4 });
                    break;
                case 19:
                    RecoveryPotionSubBtn.Index = 917;
                    PowerUpSubBtn.Index = 917;
                    ScrollSubBtn.Index = 916;
                    ScriptSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Scroll, Usermode = false });
                    break;
                case 20:
                    RecoveryPotionSubBtn.Index = 917;
                    PowerUpSubBtn.Index = 917;
                    ScrollSubBtn.Index = 917;
                    ScriptSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Script, Usermode = false });
                    break;
                case 21:
                    GemSubBtn.Index = 916;
                    OrbSubBtn.Index = 917;
                    AwakeSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Gem, Usermode = false, MinShape = 3, MaxShape = 3 });
                    break;
                case 22:
                    GemSubBtn.Index = 917;
                    OrbSubBtn.Index = 916;
                    AwakeSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Gem, Usermode = false, MinShape = 4, MaxShape = 4 });
                    break;
                case 23:
                    GemSubBtn.Index = 917;
                    OrbSubBtn.Index = 917;
                    AwakeSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Awakening, Usermode = false });
                    break;
            
                case 29:
                    MaterialsSubBtn.Index = 916;
                    FishSubBtn.Index = 917;
                    MeatSubBtn.Index = 917;
                    OreSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.CraftingMaterial, Usermode = false });
                    break;
                case 30:
                    MaterialsSubBtn.Index = 917;
                    FishSubBtn.Index = 916;
                    MeatSubBtn.Index = 917;
                    OreSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Fish, Usermode = false });
                    break;
                case 31:
                    MaterialsSubBtn.Index = 917;
                    FishSubBtn.Index = 917;
                    MeatSubBtn.Index = 916;
                    OreSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Meat, Usermode = false });
                    break;
                case 32:
                    MaterialsSubBtn.Index = 917;
                    FishSubBtn.Index = 917;
                    MeatSubBtn.Index = 917;
                    OreSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Ore, Usermode = false });
                    break;
                case 33:
                    NoveltyPetsSubBtn.Index = 916;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Pets, Usermode = false, MinShape = 0, MaxShape = 13 });
                    break;
                case 34:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 916;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Pets, Usermode = false, MinShape = 20, MaxShape = 28 });
                    break;
                case 35:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 916;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Mount, Usermode = false });
                    break;
                case 36:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 916;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Reins, Usermode = false });
                    break;
                case 37:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 916;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Bells, Usermode = false });
                    break;
                case 38:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 916;
                    MaskSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Ribbon, Usermode = false });
                    break;
                case 39:
                    NoveltyPetsSubBtn.Index = 917;
                    NoveltyEquipmentSubBtn.Index = 917;
                    MountsSubBtn.Index = 917;
                    ReinsSubBtn.Index = 917;
                    BellsSubBtn.Index = 917;
                    RibbonSubBtn.Index = 917;
                    MaskSubBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Mask, Usermode = false });
                    break;
                case 40:
                    setdefault();
                    GradeButton.Index = 914;
                    CommonBtn.Visible = true;
                    RareBtn.Visible = true;
                    LegendaryBtn.Visible = true;
                    MythicalBtn.Visible = true;
                    SetLocations(9);
                    break;
                case 41:
                    CommonBtn.Index = 916;
                    RareBtn.Index = 917;
                    LegendaryBtn.Index = 917;
                    MythicalBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Grade = ItemGrade.Common, Usermode = false });
                    break;
                case 42:
                    CommonBtn.Index = 917;
                    RareBtn.Index = 916;
                    LegendaryBtn.Index = 917;
                    MythicalBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Grade = ItemGrade.Rare, Usermode = false });
                    break;
                case 43:
                    CommonBtn.Index = 917;
                    RareBtn.Index = 917;
                    LegendaryBtn.Index = 916;
                    MythicalBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Grade = ItemGrade.Legendary, Usermode = false });
                    break;
                case 44:
                    CommonBtn.Index = 917;
                    RareBtn.Index = 917;
                    LegendaryBtn.Index = 917;
                    MythicalBtn.Index = 916;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Grade = ItemGrade.Mythical, Usermode = false });
                    break;
                case 48:
                    MaterialsSubBtn.Index = 917;
                    FishSubBtn.Index = 917;
                    MeatSubBtn.Index = 917;
                    OreSubBtn.Index = 917;
                    Network.Enqueue(new C.MarketSearch { Match = SearchTextBox.Text, Type = ItemType.Nothing, Usermode = false });
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (uint.TryParse(PriceTextBox.Text, out Amount) && Amount >= MinAmount)
            {
                PriceTextBox.BorderColour = Color.Lime;

                if (Amount > MaxAmount)
                {
                    Amount = MaxAmount;
                    PriceTextBox.Text = MaxAmount.ToString();
                    PriceTextBox.TextBox.SelectionStart = PriceTextBox.Text.Length;
                    PriceTextBox.ForeColour = Color.Lime;
                    SellItemButton.Enabled = true;
                }
                if (Amount == MaxAmount)
                    PriceTextBox.BorderColour = Color.Orange;
                SellItemButton.Enabled = true;
                PriceTextBox.ForeColour = Color.Lime;
            }
            else
            {
                PriceTextBox.BorderColour = Color.Red;
                PriceTextBox.ForeColour = Color.Red;
                SellItemButton.Enabled = false;
            }
        }

        private void MirInputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ItemCell_Click()
        {
            if (tempCell != null)
            {
                tempCell.Locked = false;
                SellItemSlot = null;
                tempCell = null;
            }

            if (GameScene.SelectedCell == null || GameScene.SelectedCell.GridType != MirGridType.Inventory ||
                  GameScene.SelectedCell.Item != null && GameScene.SelectedCell.Item.Info.Durability < 0)
                return;

            SellItemSlot = GameScene.SelectedCell.Item;
            tempCell = GameScene.SelectedCell;
            tempCell.Locked = true;
            GameScene.SelectedCell = null;
            PriceTextBox.SetFocus();
        }

        public void setdefault()
        {
            ShowAllButton.Index = 915;
            WeaponButton.Index = 915;

            DraperyItemsButton.Index = 915;
            ArmoursSubBtn.Index = 917;
            ArmoursSubBtn.Visible = false;
            HelmetsSubBtn.Index = 917;
            HelmetsSubBtn.Visible = false;
            BeltsSubBtn.Index = 917;
            BeltsSubBtn.Visible = false;
            BootsSubBtn.Index = 917;
            BootsSubBtn.Visible = false;
            StonesSubBtn.Index = 917;
            StonesSubBtn.Visible = false;

            AccessoriesItemsButton.Index = 915;
            NecklaceSubBtn.Index = 917;
            NecklaceSubBtn.Visible = false;
            BraceletsSubBtn.Index = 917;
            BraceletsSubBtn.Visible = false;
            RingsSubBtn.Index = 917;
            RingsSubBtn.Visible = false;

            ConsumableItemsButton.Index = 915;
            RecoveryPotionSubBtn.Index = 917;
            RecoveryPotionSubBtn.Visible = false;
            PowerUpSubBtn.Index = 917;
            PowerUpSubBtn.Visible = false;
            ScrollSubBtn.Index = 917;
            ScrollSubBtn.Visible = false;
            ScriptSubBtn.Index = 917;
            ScriptSubBtn.Visible = false;

            EnhEquipButton.Index = 915;
            GemSubBtn.Index = 917;
            GemSubBtn.Visible = false;
            OrbSubBtn.Index = 917;
            OrbSubBtn.Visible = false;
            AwakeSubBtn.Index = 917;
            AwakeSubBtn.Visible = false;

            BooksButton.Index = 915;
            for (int i=0; i<BookSubBtn.Length; ++i)
            {
                BookSubBtn[i].Index = 917;
                BookSubBtn[i].Visible = false;
            }

            CraftingSystemButton.Index = 915;
            MaterialsSubBtn.Index = 917;
            MaterialsSubBtn.Visible = false;
            FishSubBtn.Index = 917;
            FishSubBtn.Visible = false;
            MeatSubBtn.Index = 917;
            MeatSubBtn.Visible = false;
            OreSubBtn.Index = 917;
            OreSubBtn.Visible = false;

            PetsItemButton.Index = 915;
            NoveltyPetsSubBtn.Index = 917;
            NoveltyPetsSubBtn.Visible = false;
            NoveltyEquipmentSubBtn.Index = 917;
            NoveltyEquipmentSubBtn.Visible = false;
            MountsSubBtn.Index = 917;
            MountsSubBtn.Visible = false;
            ReinsSubBtn.Index = 917;
            ReinsSubBtn.Visible = false;
            BellsSubBtn.Index = 917;
            BellsSubBtn.Visible = false;
            RibbonSubBtn.Index = 917;
            RibbonSubBtn.Visible = false;
            MaskSubBtn.Index = 917;
            MaskSubBtn.Visible = false;

            GradeButton.Index = 915;
            CommonBtn.Index = 917;
            CommonBtn.Visible = false;
            RareBtn.Index = 917;
            RareBtn.Visible = false;
            LegendaryBtn.Index = 917;
            LegendaryBtn.Visible = false;
            MythicalBtn.Index = 917;
            MythicalBtn.Visible = false;
        }

        public void SetLocations(int i)
        {
            switch (i)
            {
                case 0:
                case 1:
                case 9://8
                    {
                        ShowAllButton.Location = new Point(12, 67);
                        WeaponButton.Location = new Point(12, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }
                case 2:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, StonesSubBtn.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }
                case 3:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, RingsSubBtn.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }
                case 4:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ScriptSubBtn.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }
                case 5:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, AwakeSubBtn.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }
                case 6:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BookSubBtn[BookSubBtn.Length-1].Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }

                case 7:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, OreSubBtn.Location.Y + 20);
                        GradeButton.Location = new Point(10, PetsItemButton.Location.Y + 20);
                        break;
                    }

                case 8:
                    {
                        ShowAllButton.Location = new Point(10, 67);
                        WeaponButton.Location = new Point(10, ShowAllButton.Location.Y + 20);
                        DraperyItemsButton.Location = new Point(10, WeaponButton.Location.Y + 20);
                        AccessoriesItemsButton.Location = new Point(10, DraperyItemsButton.Location.Y + 20);
                        ConsumableItemsButton.Location = new Point(10, AccessoriesItemsButton.Location.Y + 20);
                        EnhEquipButton.Location = new Point(10, ConsumableItemsButton.Location.Y + 20);
                        BooksButton.Location = new Point(10, EnhEquipButton.Location.Y + 20);
                        CraftingSystemButton.Location = new Point(10, BooksButton.Location.Y + 20);
                        PetsItemButton.Location = new Point(10, CraftingSystemButton.Location.Y + 20);
                        GradeButton.Location = new Point(10, MaskSubBtn.Location.Y + 20);
                        break;
                    }
            }
        }

        public void Hide()
        {
            if (!Visible) return;
            Visible = false;
            Listings.Clear();
            if (tempCell != null)
            {
                tempCell.Locked = false;
                SellItemSlot = null;
                tempCell = null;
            }
        }
        public void Show()
        {
            if (Visible) return;
            Visible = true;
            TMerchantDialog(0);
            SwitchTab(0);
            UpdateInterface();
        }

        #region AuctionRow
        public sealed class AuctionRow : MirControl
        {
            public ClientAuction Listing = null;

            public MirLabel NameLabel, PriceLabel, SellerLabel, ExpireLabel;
            public MirImageControl IconImage, SelectedImage;
            public bool Selected = false;

            Size IconArea = new Size(36, 32);

            public AuctionRow()
            {
                Size = new Size(352, 31);
                Sound = SoundList.ButtonA;
                BorderColour = Color.FromArgb(255, 200, 100, 0);
                Location = new Point(2, 0);

                BeforeDraw += AuctionRow_BeforeDraw;

                NameLabel = new MirLabel
                {
                    AutoSize = true,
                    Size = new Size(140, 20),
                    Location = new Point(38, 7),
                    DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                    NotControl = true,
                    Parent = this,
                };

                PriceLabel = new MirLabel
                {
                    AutoSize = true,
                    Size = new Size(178, 20),
                    Location = new Point(165, 7),
                    DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                    NotControl = true,
                    Parent = this,
                };

                SellerLabel = new MirLabel
                {
                    AutoSize = true,
                    Size = new Size(148, 20),
                    Location = new Point(255, 0),
                    DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                    NotControl = true,
                    Parent = this,
                };

                IconImage = new MirImageControl
                {
                    Size = new Size(36, 32),
                    Location = new Point(1, 1),
                    Parent = this,
                };
                SelectedImage = new MirImageControl
                {
                    Size = new Size(339, 34),
                    Location = new Point(0, 0),
                    Border = true,
                    BorderColour = Color.FromArgb(255, 210, 100, 0),
                    NotControl = true,
                    Visible = false,
                    Parent = this,
                };
                ExpireLabel = new MirLabel
                {
                    AutoSize = true,
                    Location = new Point(255, 14),
                    Size = new Size(110, 22),
                    DrawFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter,
                    Parent = this,
                    NotControl = true,
                };
                UpdateInterface();
            }

            #endregion

            void AuctionRow_BeforeDraw(object sender, EventArgs e)
            {
                UpdateInterface();
            }
            public void UpdateInterface()
            {
                if (Listing == null) return;

                IconImage.Visible = true;

                if (Listing.Item.Count > 0)
                {
                    IconImage.Index = Listing.Item.Info.Image;
                    IconImage.Library = Libraries.Items;
                }
                else
                {
                    IconImage.Index = 540;
                    IconImage.Library = Libraries.Prguse;
                }

                IconImage.Location = new Point((IconArea.Width - IconImage.Size.Width) / 2, (IconArea.Height - IconImage.Size.Height) / 2);

                ExpireLabel.Visible = Listing != null;

                if (Listing == null) return;

                ExpireLabel.Text = string.Format("{0:dd/MM/yy HH:mm:ss}", Listing.ConsignmentDate.AddDays(Globals.ConsignmentLength));
            }
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                SelectedImage = null;
                IconImage = null;

                Selected = false;
            }
            public void Clear()
            {
                Visible = false;
                NameLabel.Text = string.Empty;
                PriceLabel.Text = string.Empty;
                SellerLabel.Text = string.Empty;
            }
            public void Update(ClientAuction listing)
            {
                Listing = listing;
                NameLabel.Text = Listing.Item.FriendlyName;
                PriceLabel.Text = Listing.Price.ToString("###,###,##0") + " "+ (Listing.CurrencyType == 0 ? CMain.Tr("Gold"): CMain.Tr("Credit"));

                NameLabel.ForeColour = GameScene.GradeNameColor(Listing.Item.Info.Grade);
                if (NameLabel.ForeColour == Color.Yellow)
                    NameLabel.ForeColour = Color.White;

                if (Listing.Price > 10000000) //10Mil
                    PriceLabel.ForeColour = Color.Red;
                else if (listing.Price > 1000000) //1Million
                    PriceLabel.ForeColour = Color.Orange;
                else if (listing.Price > 100000) //1Million
                    PriceLabel.ForeColour = Color.Green;
                else if (listing.Price > 10000) //1Million
                    PriceLabel.ForeColour = Color.DeepSkyBlue;
                else
                    PriceLabel.ForeColour = Color.White;

                SellerLabel.Text = Listing.Seller;

                if (UserMode)
                {
                    switch (Listing.Seller)
                    {
                        case "Sold":
                            SellerLabel.ForeColour = Color.Gold;
                            break;
                        case "Expired":
                            SellerLabel.ForeColour = Color.Red;
                            break;
                        default:
                            SellerLabel.ForeColour = Color.White;
                            break;
                    }
                }
                Visible = true;
            }
            protected override void OnMouseEnter()
            {
                if (Listing == null) return;

                base.OnMouseEnter();
                GameScene.HoverItem = Listing.Item;
            }
            protected override void OnMouseLeave()
            {
                if (Listing == null) return;

                base.OnMouseLeave();
                GameScene.HoverItem = null;
            }
        }
    }
}