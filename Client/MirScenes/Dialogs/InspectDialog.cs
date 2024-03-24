using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C = ClientPackets;

namespace Client.MirScenes.Dialogs
{
    public sealed class InspectDialog : MirImageControl
    {
        public static UserItem[] Equipment = new UserItem[14];
        public static uint InspectID;

        public string Name;
        public string GuildName;
        public string GuildRank;
        public MirClass Class;
        public MirGender Gender;
        public byte Hair;
        public ushort Level;
        public string LoverName;

        public MirButton CloseButton, GroupButton, FriendButton, MailButton, TradeButton, LoverButton;
        public MirImageControl CharacterPage, ClassImage;
        public MirLabel NameLabel;
        public MirLabel GuildLabel, LoverLabel;

        public MirItemCell
            WeaponCell,
            ArmorCell,
            HelmetCell,
            TorchCell,
            NecklaceCell,
            BraceletLCell,
            BraceletRCell,
            RingLCell,
            RingRCell,
            AmuletCell,
            BeltCell,
            BootsCell,
            StoneCell,
            MountCell,
            TransformCell,
            AmuletUpCell;

        public bool Looks_HumUp = false;

        public InspectDialog()
        {
            Index = 430;
            Library = Libraries.Prguse;
            Location = new Point(536, 0);
            Movable = true;
            Sort = true;

            CharacterPage = new MirImageControl
            {
                Index = 340,
                Parent = this,
                Library = Libraries.Prguse,
                Location = new Point(8, 70),
            };
            CharacterPage.AfterDraw += (o, e) =>
            {
                //CharacterPage.Index = 379;
                if (Looks_HumUp)
                    DrawHumUp();
                else
                    DrawNormal();
            };


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

            GroupButton = new MirButton
            {
                HoverIndex = 432,
                Index = 431,
                Location = new Point(75, 357),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 433,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Invite to Group"),
            };
            GroupButton.Click += (o, e) =>
            {

                if (GroupDialog.GroupList.Count >= Globals.MaxGroup)
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr("Your group already has the maximum number of members.", ChatType.System);
                    return;
                }
                if (GroupDialog.GroupList.Count > 0 && GroupDialog.GroupList[0] != MapObject.User.Name)
                {

                    GameScene.Scene.ChatDialog.ReceiveChatTr("You are not the leader of your group.", ChatType.System);
                }

                Network.Enqueue(new C.AddMember { Name = Name });
                return;
            };

            FriendButton = new MirButton
            {
                HoverIndex = 435,
                Index = 434,
                Location = new Point(105, 357),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 436,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Add to Friends List"),
            };
            FriendButton.Click += (o, e) =>
            {
                Network.Enqueue(new C.AddFriend { Name = Name, Blocked = false });
            };

            MailButton = new MirButton
            {
                HoverIndex = 438,
                Index = 437,
                Location = new Point(135, 357),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 439,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Send Mail"),
            };
            MailButton.Click += (o, e) => GameScene.Scene.MailComposeLetterDialog.ComposeMail(Name);

            TradeButton = new MirButton
            {
                HoverIndex = 524,
                Index = 523,
                Location = new Point(165, 357),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 525,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Trade"),
            };
            TradeButton.Click += (o, e) => Network.Enqueue(new C.TradeRequest());

            NameLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                Location = new Point(50, 12),
                Size = new Size(190, 20),
                NotControl = true
            };
            NameLabel.Click += (o, e) =>
            {
                GameScene.Scene.ChatDialog.ChatTextBox.SetFocus();
                GameScene.Scene.ChatDialog.ChatTextBox.Text = string.Format("/{0} ", Name);
                GameScene.Scene.ChatDialog.ChatTextBox.Visible = true;
                GameScene.Scene.ChatDialog.ChatTextBox.TextBox.SelectionLength = 0;
                GameScene.Scene.ChatDialog.ChatTextBox.TextBox.SelectionStart = Name.Length + 2;

            };
            LoverButton = new MirButton
            {
                Index = 604,
                Location = new Point(17, 17),
                Library = Libraries.Prguse,
                Parent = this,
                Sound = SoundList.None
            };

            GuildLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                Location = new Point(50, 33),
                Size = new Size(190, 30),
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


            WeaponCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Weapon,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(123, 7),
            };

            ArmorCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Armour,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(163, 7),
            };

            HelmetCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Helmet,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 7),
            };


            TorchCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Torch,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 134),
            };

            NecklaceCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Necklace,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 98),
            };

            BraceletLCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.BraceletL,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(8, 170),
            };
            BraceletRCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.BraceletR,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 170),
            };
            RingLCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.RingL,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(8, 206),
            };
            RingRCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.RingR,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 206),
            };

            AmuletCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Amulet,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(8, 242),
            };

            BootsCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Boots,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(48, 242),
            };
            BeltCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Belt,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(88, 242),
            };

            StoneCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Stone,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(128, 242),
            };

            MountCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Mount,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(203, 62),
            };

            TransformCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.Transform,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(10, 134),
            };

            AmuletUpCell = new MirItemCell
            {
                ItemSlot = (int)EquipmentSlot.AmuletUp,
                GridType = MirGridType.Inspect,
                Parent = CharacterPage,
                Location = new Point(10, 98),
            };

        }

        public bool HasClassWeapon //stupple
        {
            get
            {
                if (WeaponCell == null || WeaponCell.Item == null)
                      return true;

                return Functions.HasClassWeapon(Class, WeaponCell.Item.Info.Shape);
            }
        }

        public void RefreshEquipmentStat()
        {
            short WeaponTransform = 0;
            short ArmourTransform = 0;
            Looks_HumUp = false;

            for (int i = 0; i < Equipment.Length; i++)
            {
                UserItem temp = Equipment[i];
                if (temp == null || temp.Info == null) continue;

                ItemInfo RealItem = Functions.GetRealItem(temp.Info, Level, Class, GameScene.ItemInfoList);

                if (RealItem.Type == ItemType.Armour)
                {
                    ArmourTransform = temp.Transform;
                    if (RealItem.NeedHumUp)
                        Looks_HumUp = true;
                }
                if (RealItem.Type == ItemType.Weapon)
                {
                    WeaponTransform = temp.Transform;
                    if (RealItem.NeedHumUp)
                        Looks_HumUp = true;
                }
            }

            if (WeaponTransform > 0 && ArmourTransform > 0)
            {
                Looks_HumUp = true;
            }
        }

        private void RefreshHumUp()
        {
            if (!Looks_HumUp)
            {
                switch (Class)
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
                switch (Class)
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

        public void RefreshInferface()
        {
            int offSet = Gender == MirGender.Male ? 0 : 1;

            CharacterPage.Index = 340 + offSet;

            RefreshHumUp();

            NameLabel.Text = Name;
            GuildLabel.Text = GuildName + " " + GuildRank;
            if (LoverName != "")
            {
                LoverButton.Visible = true;
                LoverButton.Hint = LoverName;
            }
            else
                LoverButton.Visible = false;
        }

        private void DrawHumUp()
        {
            MLibrary JobStaetItems = Libraries.StateItems;
            if (HasClassWeapon)
            {
                switch (Class)
                {
                    case MirClass.Warrior:
                        JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsWarM : Libraries.StateItemsWarW;
                        Libraries.Prguse.Draw(330 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Wizard:
                        JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsWizM : Libraries.StateItemsWizW;
                        Libraries.Prguse.Draw(332 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Taoist:
                        JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsTaoM : Libraries.StateItemsTaoW;
                        Libraries.Prguse.Draw(334 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Assassin:
                        JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsAssM : Libraries.StateItemsAssW;
                        Libraries.Prguse.Draw(336 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                    case MirClass.Archer:
                        JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsArcM : Libraries.StateItemsArcW;
                        Libraries.Prguse.Draw(320 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
                        break;
                }
            }
            else
            {
                JobStaetItems = Gender == MirGender.Male ? Libraries.StateItemsComM : Libraries.StateItemsComW;
                Libraries.Prguse.Draw(320 + (Gender == MirGender.Male ? 0 : 1), new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
            }

            if (JobStaetItems == null) return;

            if (ArmorCell.Item != null)
            {
                if (ArmorCell.Item.Info.Effect == 1 || ArmorCell.Item.Info.Effect == 2)
                {
                    int wingOffset = 0;

                    if (GameScene.User.HasClassWeapon)
                    {
                        switch (GameScene.User.Class)
                        {
                            case MirClass.Warrior:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 10 : 20;
                                break;
                            case MirClass.Wizard:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 12 : 22;
                                break;
                            case MirClass.Taoist:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 14 : 24;
                                break;
                            case MirClass.Assassin:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 16 : 26;
                                break;
                            case MirClass.Archer:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 18 : 28;
                                break;
                            default:
                                wingOffset = ArmorCell.Item.Info.Effect == 1 ? 2 : 4;
                                break;
                        }
                    }
                    else
                    {
                        wingOffset = GameScene.User.WingEffect == 1 ? 18 : 28;

                    }

                    //stupple hum stop
                    int genderOffset = Gender == MirGender.Male ? 0 : 1;

                    Libraries.Prguse2.DrawBlendEx(1200 + wingOffset + genderOffset, DisplayLocation, Color.White, true, 1F);
                }

                JobStaetItems.Draw(ArmorCell.Item.Image, DisplayLocation, Color.White, true, 1F);
            }

            if (WeaponCell.Item != null && WeaponCell.Item.LookHumUp())
            {
                JobStaetItems.Draw(WeaponCell.Item.Image, DisplayLocation, Color.White, true, 1F);
            }

            if (HelmetCell.Item != null)
                JobStaetItems.Draw(HelmetCell.Item.Info.Image, DisplayLocation, Color.White, true, 1F);
            else
            {
                int hair = 461 + Hair + (Gender == MirGender.Male ? 0 : 40);
                Libraries.Prguse.Draw(hair, new Point(DisplayLocation.X, DisplayLocation.Y), Color.White, true, 1F);
            }
        }

        private void DrawNormal()
        {
            if (Libraries.StateItems == null) return;

            ItemInfo RealItem = null;

            if (ArmorCell.Item != null)
            {
                int WingEffect = ArmorCell.Item.Info.Effect;

                if (ArmorCell.Item.Info.Effect == 1 || WingEffect == 2)
                {
                    int wingOffset = WingEffect == 1 ? 2 : 4;

                    if ((byte)Class < 6)
                    {
                        wingOffset = WingEffect == 1 ? 2 : 4;
                    }
                    else
                    {
                        wingOffset = WingEffect == 1 ? 18 : 28;
                    }

                    int genderOffset = Gender == MirGender.Male ? 0 : 1;

                    Libraries.Prguse2.DrawBlendEx(1200 + wingOffset + genderOffset, new Point(DisplayLocation.X, DisplayLocation.Y - 20), Color.White, true, 1F);
                }

                Libraries.StateItems.Draw(ArmorCell.Item.Info.Image, new Point(DisplayLocation.X + 0, DisplayLocation.Y + -20), Color.White, true, 1F);
            }

            if (WeaponCell.Item != null)
            {
                Libraries.StateItems.Draw(WeaponCell.Item.Info.Image, new Point(DisplayLocation.X, DisplayLocation.Y - 20), Color.White, true, 1F);
            }

            if (HelmetCell.Item != null)
                Libraries.StateItems.Draw(HelmetCell.Item.Info.Image, new Point(DisplayLocation.X, DisplayLocation.Y - 20), Color.White, true, 1F);
            else
            {
                int hair = 441 + Hair + (Class == MirClass.Assassin ? 20 : 0) + (Gender == MirGender.Male ? 0 : 40);

                int offSetX = Class == MirClass.Assassin ? (Gender == MirGender.Male ? 6 : 4) : 0;
                int offSetY = Class == MirClass.Assassin ? (Gender == MirGender.Male ? 25 : 18) : 0;

                Libraries.Prguse.Draw(hair, new Point(DisplayLocation.X + offSetX, DisplayLocation.Y + offSetY - 20), Color.White, true, 1F);
            }
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
        }
    }
}
