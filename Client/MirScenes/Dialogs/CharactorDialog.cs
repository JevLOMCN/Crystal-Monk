using Client.MirControls;
using Client.MirGraphics;
using Client.MirObjects;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.MirScenes.Dialogs
{
    public sealed class CharacterDialog : MirImageControl
    {
        public MirButton CloseButton, CharacterButton, StatusButton, StateButton, SkillButton;
        public MirImageControl CharacterPage, StatusPage, StatePage, SkillPage, ClassImage;

        public MirLabel NameLabel, GuildLabel, LoverLabel;
        public MirLabel ACLabel, MACLabel, DCLabel, MCLabel, SCLabel, HealthLabel, ManaLabel;
        public MirLabel CritRLabel, CritDLabel, LuckLabel, AttkSpdLabel, AccLabel, AgilLabel;
        public MirLabel ExpPLabel, BagWLabel, WearWLabel, HandWLabel, MagicRLabel, PoisonRecLabel, HealthRLabel, ManaRLabel, PoisonResLabel, HolyTLabel, FreezeLabel, PoisonAtkLabel;
        public MirLabel HeadingLabel, StatLabel;
        public MirButton NextButton, BackButton;

        public MirItemCell[] Grid;
        public MagicButton[] Magics;

        public int StartIndex;

        public CharacterDialog()
        {
            Index = 504;
            Library = Libraries.Title;
            Location = new Point(Settings.ScreenWidth - 264, 0);
            Movable = true;
            Sort = true;

            BeforeDraw += (o, e) => RefreshInterface();

            CharacterPage = new MirImageControl
            {
                Index = 340,
                Parent = this,
                Library = Libraries.Prguse,
                Location = new Point(8, 90),
            };
            CharacterPage.AfterDraw += (o, e) =>
            {
                if (Libraries.StateItems == null) return;
                MLibrary JobStaetItems = Libraries.StateItems;
                if (JobStaetItems == null) return;

                if (!GameScene.User.LookHumUp || MapControl.User.Class == MirClass.Monk)
                {
                    DrawBodyItem();
                    DrawWeaponItem();
                    DrawHeadItem();
                }
                else
                {
                    DrawHumUp();
                }
            };

            StatusPage = new MirImageControl
            {
                Index = 506,
                Parent = this,
                Library = Libraries.Title,
                Location = new Point(8, 90),
                Visible = false,
            };
            StatusPage.BeforeDraw += (o, e) =>
            {
                ACLabel.Text = string.Format("{0}-{1}", MapObject.User.MinAC, MapObject.User.MaxAC);
                MACLabel.Text = string.Format("{0}-{1}", MapObject.User.MinMAC, MapObject.User.MaxMAC);
                DCLabel.Text = string.Format("{0}-{1}", MapObject.User.MinDC, MapObject.User.MaxDC);
                MCLabel.Text = string.Format("{0}-{1}", MapObject.User.MinMC, MapObject.User.MaxMC);
                SCLabel.Text = string.Format("{0}-{1}", MapObject.User.MinSC, MapObject.User.MaxSC);
                HealthLabel.Text = string.Format("{0}/{1}", MapObject.User.HP, MapObject.User.MaxHP);
                ManaLabel.Text = string.Format("{0}/{1}", MapObject.User.MP, MapObject.User.MaxMP);
                CritRLabel.Text = string.Format("{0}%", MapObject.User.CriticalRate);
                CritDLabel.Text = string.Format("{0}", MapObject.User.CriticalDamage);
                AttkSpdLabel.Text = string.Format("{0}", MapObject.User.ASpeed);
                AccLabel.Text = string.Format("+{0}", MapObject.User.Accuracy);
                AgilLabel.Text = string.Format("+{0}", MapObject.User.Agility);
                LuckLabel.Text = string.Format("{0}", MapObject.User.Luck);
            };

            StatePage = new MirImageControl
            {
                Index = 507,
                Parent = this,
                Library = Libraries.Title,
                Location = new Point(8, 90),
                Visible = false
            };
            StatePage.BeforeDraw += (o, e) =>
            {
                ExpPLabel.Text = string.Format("{0}/{1}", MapObject.User.Experience, (double)MapObject.User.MaxExperience);
                BagWLabel.Text = string.Format("{0}/{1}", MapObject.User.CurrentBagWeight, MapObject.User.MaxBagWeight);
                WearWLabel.Text = string.Format("{0}/{1}", MapObject.User.CurrentWearWeight, MapObject.User.MaxWearWeight);
                HandWLabel.Text = string.Format("{0}/{1}", MapObject.User.CurrentHandWeight, MapObject.User.MaxHandWeight);
                MagicRLabel.Text = string.Format("+{0}", MapObject.User.MagicResist);
                PoisonResLabel.Text = string.Format("+{0}", MapObject.User.PoisonResist);
                HealthRLabel.Text = string.Format("+{0}", MapObject.User.HealthRecovery);
                ManaRLabel.Text = string.Format("+{0}", MapObject.User.SpellRecovery);
                PoisonRecLabel.Text = string.Format("+{0}", MapObject.User.PoisonRecovery);
                HolyTLabel.Text = string.Format("+{0}", MapObject.User.Holy);
                FreezeLabel.Text = string.Format("+{0}", MapObject.User.Freezing);
                PoisonAtkLabel.Text = string.Format("+{0}", MapObject.User.PoisonAttack);
            };


            SkillPage = new MirImageControl
            {
                Index = 508,
                Parent = this,
                Library = Libraries.Title,
                Location = new Point(8, 90),
                Visible = false
            };


            CharacterButton = new MirButton
            {
                Index = 500,
                Library = Libraries.Title,
                Location = new Point(8, 70),
                Parent = this,
                PressedIndex = 500,
                Size = new Size(64, 20),
                Sound = SoundList.ButtonA,
            };
            CharacterButton.Click += (o, e) => ShowCharacterPage();
            StatusButton = new MirButton
            {
                Library = Libraries.Title,
                Location = new Point(70, 70),
                Parent = this,
                PressedIndex = 501,
                Size = new Size(64, 20),
                Sound = SoundList.ButtonA
            };
            StatusButton.Click += (o, e) => ShowStatusPage();

            StateButton = new MirButton
            {
                Library = Libraries.Title,
                Location = new Point(132, 70),
                Parent = this,
                PressedIndex = 502,
                Size = new Size(64, 20),
                Sound = SoundList.ButtonA
            };
            StateButton.Click += (o, e) => ShowStatePage();

            SkillButton = new MirButton
            {
                Library = Libraries.Title,
                Location = new Point(194, 70),
                Parent = this,
                PressedIndex = 503,
                Size = new Size(64, 20),
                Sound = SoundList.ButtonA
            };
            SkillButton.Click += (o, e) => ShowSkillPage();

            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(241, 3),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };
            CloseButton.Click += (o, e) => Hide();

            NameLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Parent = this,
                Location = new Point(0, 12),
                Size = new Size(264, 20),
                NotControl = true,
            };
            GuildLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Parent = this,
                Location = new Point(0, 33),
                Size = new Size(264, 30),
                NotControl = true,
            };
            ClassImage = new MirImageControl
            {
                Index = 100,
                Library = Libraries.Prguse,
                Location = new Point(15, 33),
                Parent = this,
                NotControl = true,
            };

            Grid = new MirItemCell[Enum.GetNames(typeof(EquipmentSlot)).Length];

            Grid[(int)EquipmentSlot.Weapon] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Weapon,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(123, 7),
            };


            Grid[(int)EquipmentSlot.Armour] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Armour,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(163, 7),
            };


            Grid[(int)EquipmentSlot.Helmet] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Helmet,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 7),
            };



            Grid[(int)EquipmentSlot.Torch] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Torch,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 134),
            };


            Grid[(int)EquipmentSlot.Necklace] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Necklace,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 98),
            };


            Grid[(int)EquipmentSlot.BraceletL] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.BraceletL,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(10, 170),
            };

            Grid[(int)EquipmentSlot.BraceletR] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.BraceletR,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 170),
            };

            Grid[(int)EquipmentSlot.RingL] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.RingL,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(10, 206),
            };

            Grid[(int)EquipmentSlot.RingR] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.RingR,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 206),
            };


            Grid[(int)EquipmentSlot.Amulet] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Amulet,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(10, 242),
            };


            Grid[(int)EquipmentSlot.Boots] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Boots,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(48, 242),
            };

            Grid[(int)EquipmentSlot.Belt] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Belt,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(88, 242),
            };


            Grid[(int)EquipmentSlot.Stone] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Stone,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(128, 242),
            };

            Grid[(int)EquipmentSlot.Mount] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Mount,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(205, 62),
            };

            Grid[(int)EquipmentSlot.Transform] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Transform,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(10, 134),
            };

            Grid[(int)EquipmentSlot.AmuletUp] = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.AmuletUp,
                GridType = MirGridType.Equipment,
                Parent = CharacterPage,
                Location = new Point(10, 98),
            };

            // STATS I
            HealthLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 20),
                NotControl = true,
                Text = "0-0",
            };

            ManaLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 38),
                NotControl = true,
                Text = "0-0",
            };

            ACLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 56),
                NotControl = true,
                Text = "0-0",
            };

            MACLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 74),
                NotControl = true,
                Text = "0-0",
            };
            DCLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 92),
                NotControl = true,
                Text = "0-0"
            };
            MCLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 110),
                NotControl = true,
                Text = "0/0"
            };
            SCLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 128),
                NotControl = true,
                Text = "0/0"
            };
            //Breezer - New Labels
            CritRLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 146),
                NotControl = true
            };
            CritDLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 164),
                NotControl = true
            };
            AttkSpdLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 182),
                NotControl = true
            };
            AccLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 200),
                NotControl = true
            };
            AgilLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 218),
                NotControl = true
            };
            LuckLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatusPage,
                Location = new Point(126, 236),
                NotControl = true
            };
            // STATS II 
            ExpPLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 20),
                NotControl = true,
                Text = "0-0",
            };

            BagWLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 38),
                NotControl = true,
                Text = "0-0",
            };

            WearWLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 56),
                NotControl = true,
                Text = "0-0",
            };

            HandWLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 74),
                NotControl = true,
                Text = "0-0",
            };
            MagicRLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 92),
                NotControl = true,
                Text = "0-0"
            };
            PoisonResLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 110),
                NotControl = true,
                Text = "0/0"
            };
            HealthRLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 128),
                NotControl = true,
                Text = "0/0"
            };
            //Breezer
            ManaRLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 146),
                NotControl = true
            };
            PoisonRecLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 164),
                NotControl = true
            };
            HolyTLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 182),
                NotControl = true
            };
            FreezeLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 200),
                NotControl = true
            };
            PoisonAtkLabel = new MirLabel
            {
                AutoSize = true,
                Parent = StatePage,
                Location = new Point(126, 218),
                NotControl = true
            };

            Magics = new MagicButton[7];

            for (int i = 0; i < Magics.Length; i++)
                Magics[i] = new MagicButton { Parent = SkillPage, Visible = false, Location = new Point(8, 8 + i * 33) };

            NextButton = new MirButton
            {
                Index = 396,
                Location = new Point(140, 250),
                Library = Libraries.Prguse,
                Parent = SkillPage,
                PressedIndex = 397,
                Sound = SoundList.ButtonA,
            };
            NextButton.Click += (o, e) =>
            {
                if (StartIndex + 7 >= MapObject.User.Magics.Count) return;

                StartIndex += 7;
                RefreshInterface();

                ClearCoolDowns();
            };

            BackButton = new MirButton
            {
                Index = 398,
                Location = new Point(90, 250),
                Library = Libraries.Prguse,
                Parent = SkillPage,
                PressedIndex = 399,
                Sound = SoundList.ButtonA,
            };
            BackButton.Click += (o, e) =>
            {
                if (StartIndex - 7 < 0) return;

                StartIndex -= 7;
                RefreshInterface();

                ClearCoolDowns();
            };
        }

        public void Hide()
        {
            if (!Visible) return;
            Visible = false;
        }

        public void Show()
        {
            if (Visible) return;
            Visible = true;

            ClearCoolDowns();
        }

        public void ShowCharacterPage()
        {
            CharacterPage.Visible = true;
            StatusPage.Visible = false;
            StatePage.Visible = false;
            SkillPage.Visible = false;
            CharacterButton.Index = 500;
            StatusButton.Index = -1;
            StateButton.Index = -1;
            SkillButton.Index = -1;
        }

        private void ShowStatusPage()
        {
            CharacterPage.Visible = false;
            StatusPage.Visible = true;
            StatePage.Visible = false;
            SkillPage.Visible = false;
            CharacterButton.Index = -1;
            StatusButton.Index = 501;
            StateButton.Index = -1;
            SkillButton.Index = -1;
        }

        private void ShowStatePage()
        {
            CharacterPage.Visible = false;
            StatusPage.Visible = false;
            StatePage.Visible = true;
            SkillPage.Visible = false;
            CharacterButton.Index = -1;
            StatusButton.Index = -1;
            StateButton.Index = 502;
            SkillButton.Index = -1;
        }

        public void ShowSkillPage()
        {
            CharacterPage.Visible = false;
            StatusPage.Visible = false;
            StatePage.Visible = false;
            SkillPage.Visible = true;
            CharacterButton.Index = -1;
            StatusButton.Index = -1;
            StateButton.Index = -1;
            SkillButton.Index = 503;
            StartIndex = 0;

            ClearCoolDowns();
        }

        private void DrawBodyItem()
        {
            if (Grid[(int)EquipmentSlot.Armour].Item == null)
                return;

            ItemInfo RealItem = null;
            if (GameScene.User.WingEffect == 1 || GameScene.User.WingEffect == 2)
            {
                int wingOffset = 0;
                wingOffset = GameScene.User.WingEffect == 1 ? 2 : 4;

                int genderOffset = MapObject.User.Gender == MirGender.Male ? 0 : 1;

                Libraries.Prguse2.DrawBlend(1200 + wingOffset + genderOffset, DisplayLocation, Color.White, true, 1F);
            }

            RealItem = Functions.GetRealItem(Grid[(int)EquipmentSlot.Armour].Item.Info, MapObject.User.Level, MapObject.User.Class, GameScene.ItemInfoList);
            Libraries.StateItems.Draw(RealItem.Image, DisplayLocation, Color.White, true, 1F);
        }

        private void ClearCoolDowns()
        {
            for (int i = 0; i < Magics.Length; i++)
            {
                Magics[i].CoolDown.Dispose();
            }
        }

        private void RefreshHumUp(MirClass job)
        {
            if (!MapControl.User.LookHumUp || job == MirClass.Monk)
            {
                switch (job)
                {
                    case MirClass.Warrior:
                        ClassImage.Index = 100;// + offSet * 5;
                        break;
                    case MirClass.Wizard:
                        ClassImage.Index = 101;// + offSet * 5;
                        break;
                    case MirClass.Taoist:
                        ClassImage.Index = 102;// + offSet * 5;
                        break;
                    case MirClass.Assassin:
                        ClassImage.Index = 103;// + offSet * 5;
                        break;
                    case MirClass.Archer:
                        ClassImage.Index = 104;// + offSet * 5;
                        break;
                    case MirClass.Monk:
                        ClassImage.Index = 110;
                        break;
                }
            }
            else
            {
                switch (job)
                {
                    case MirClass.Warrior:
                        ClassImage.Index = 100;
                        CharacterPage.Index = 379;
                        break;
                    case MirClass.Wizard:
                        ClassImage.Index = 101;
                        CharacterPage.Index = 379;
                        break;
                    case MirClass.Taoist:
                        ClassImage.Index = 102;
                        CharacterPage.Index = 379;
                        break;
                    case MirClass.Assassin:
                        ClassImage.Index = 103;
                        CharacterPage.Index = 379;
                        break;
                    case MirClass.Archer:
                        ClassImage.Index = 104;
                        CharacterPage.Index = 379;
                        break;
                    case MirClass.Monk:
                        ClassImage.Index = 110;
                        break;
                }
            }
        }

        private void RefreshInterface()
        {
            int offSet = MapObject.User.Gender == MirGender.Male ? 0 : 1;

            Index = 504;// +offSet;
            CharacterPage.Index = 340 + offSet;

            RefreshHumUp(MapObject.User.Class);

            NameLabel.Text = MapObject.User.Name;
            GuildLabel.Text = MapObject.User.GuildName + " " + MapObject.User.GuildRankName;

            for (int i = 0; i < Magics.Length; i++)
            {
                if (i + StartIndex >= MapObject.User.Magics.Count)
                {
                    Magics[i].Visible = false;
                    continue;
                }

                Magics[i].Visible = true;
                Magics[i].Update(MapObject.User.Magics[i + StartIndex]);
            }
        }

        public MirItemCell GetCell(ulong id)
        {
            for (int i = 0; i < Grid.Length; i++)
            {
                if (Grid[i].Item == null || Grid[i].Item.UniqueID != id) continue;
                return Grid[i];
            }
            return null;
        }

        private void DrawWeaponItem()
        {
            ItemInfo RealItem = null;
            if (Grid[(int)EquipmentSlot.Weapon].Item != null)
            {
                RealItem = Functions.GetRealItem(Grid[(int)EquipmentSlot.Weapon].Item.Info, MapObject.User.Level, MapObject.User.Class, GameScene.ItemInfoList);
                Libraries.StateItems.Draw(RealItem.Image, DisplayLocation, Color.White, true, 1F);
            }
        }

        private void DrawHeadItem()
        {
            if (Grid[(int)EquipmentSlot.Helmet].Item != null)
                Libraries.StateItems.Draw(Grid[(int)EquipmentSlot.Helmet].Item.Info.Image, DisplayLocation, Color.White, true, 1F);
            else
            {
                if (MapObject.User.Class == MirClass.Monk)
                    return;

                int hair = 441 + MapObject.User.Hair + (MapObject.User.Class == MirClass.Assassin ? 20 : 0) + (MapObject.User.Gender == MirGender.Male ? 0 : 40);

                int offSetX = MapObject.User.Class == MirClass.Assassin ? (MapObject.User.Gender == MirGender.Male ? 6 : 4) : 0;
                int offSetY = MapObject.User.Class == MirClass.Assassin ? (MapObject.User.Gender == MirGender.Male ? 25 : 18) : 0;

                Libraries.Prguse.Draw(hair, new Point(DisplayLocation.X + offSetX, DisplayLocation.Y + offSetY), Color.White, true, 1F);
            }
        }

        private void DrawHumUp()
        {
            MLibrary JobStaetItems = null;

            if (GameScene.User.HasClassWeapon)
            {
                switch (GameScene.User.Class)
                {
                    case MirClass.Warrior:
                        JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsWarM : Libraries.StateItemsWarW;
                        Libraries.Prguse.Draw(330 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Wizard:
                        JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsWizM : Libraries.StateItemsWizW;
                        Libraries.Prguse.Draw(332 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Taoist:
                        JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsTaoM : Libraries.StateItemsTaoW;
                        Libraries.Prguse.Draw(334 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Assassin:
                        JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsAssM : Libraries.StateItemsAssW;
                        Libraries.Prguse.Draw(336 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Archer:
                        JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsArcM : Libraries.StateItemsArcW;
                        Libraries.Prguse.Draw(320 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                }
            }
            else
            {
                JobStaetItems = MapObject.User.Gender == MirGender.Male ? Libraries.StateItemsComM : Libraries.StateItemsComW;
                Libraries.Prguse.Draw(320 + (MapObject.User.Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
            }

            if (JobStaetItems == null) return;

            if (Grid[(int)EquipmentSlot.Armour].Item != null)
            {
                if (GameScene.User.WingEffect == 1 || GameScene.User.WingEffect == 2)
                {
                    int wingOffset = 0;

                    if (GameScene.User.HasClassWeapon)
                    {
                        switch (GameScene.User.Class)
                        {
                            case MirClass.Warrior:
                                wingOffset = GameScene.User.WingEffect == 1 ? 10 : 20;
                                break;
                            case MirClass.Wizard:
                                wingOffset = GameScene.User.WingEffect == 1 ? 12 : 22;
                                break;
                            case MirClass.Taoist:
                                wingOffset = GameScene.User.WingEffect == 1 ? 14 : 24;
                                break;
                            case MirClass.Assassin:
                                wingOffset = GameScene.User.WingEffect == 1 ? 16 : 26;
                                break;
                            case MirClass.Archer:
                                wingOffset = GameScene.User.WingEffect == 1 ? 18 : 28;
                                break;
                            default:
                                wingOffset = GameScene.User.WingEffect == 1 ? 2 : 4;
                                break;
                        }
                    }
                    else
                    {
                        wingOffset = GameScene.User.WingEffect == 1 ? 18 : 28;

                    }

                    //stupple hum stop
                    int genderOffset = MapObject.User.Gender == MirGender.Male ? 0 : 1;

                    Libraries.Prguse2.DrawBlend(1200 + wingOffset + genderOffset, DisplayLocation, Color.White, true, 1F);
                }

                JobStaetItems.Draw(Grid[(int)EquipmentSlot.Armour].Item.Image, DisplayLocation, Color.White, true, 1F);
            }

            UserItem weapon = Grid[(int)EquipmentSlot.Weapon].Item;
            if (weapon != null && weapon.LookHumUp())
            {
                JobStaetItems.Draw(weapon.Image, DisplayLocation, Color.White, true, 1F);
            }

            if (Grid[(int)EquipmentSlot.Helmet].Item != null)
            {
                JobStaetItems.Draw(Grid[(int)EquipmentSlot.Helmet].Item.Info.Image, DisplayLocation, Color.White, true, 1F);
            }
            else
            {
                int hair = 461 + MapObject.User.Hair + (MapObject.User.Gender == MirGender.Male ? 0 : 40);

                Libraries.Prguse.Draw(hair, new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
            }
        }

    }
}
