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

namespace Client.MirScenes.Dialogs
{
    public sealed class MainDialog : MirImageControl
    {
        public static UserObject User
        {
            get { return MapObject.User; }
            set { MapObject.User = value; }
        }

        public MirImageControl ExperienceBar, WeightBar, LeftCap, RightCap;
        public MirButton GameShopButton, MenuButton, InventoryButton, CharacterButton, SkillButton, QuestButton, OptionButton;
        public MirControl HealthOrb;
        public MirLabel HealthLabel, ManaLabel, TopLabel, BottomLabel, LevelLabel, CharacterName, ExperienceLabel, GoldLabel, WeightLabel, SpaceLabel, AModeLabel, PModeLabel, SModeLabel, PingLabel;

        public bool HPOnly
        {
            get { return User != null && User.Class == MirClass.Warrior && User.Level < 26; }
        }

        public MainDialog()
        {
            Index = Settings.Resolution == 800 ? 0 : Settings.Resolution == 1024 ? 1 : 2;
            Library = Libraries.Prguse;
            Location = new Point(((Settings.ScreenWidth / 2) - (Size.Width / 2)), Settings.ScreenHeight - Size.Height);
            PixelDetect = true;

            PingLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.Yellow,
                OutLineColour = Color.Black,
                Parent = this,
                Location = new Point(Settings.Resolution != 800 ? 899 : 675, Settings.Resolution != 800 ? -443 : -265),
                Visible = true
            };

            LeftCap = new MirImageControl
            {
                Index = 12,
                Library = Libraries.Prguse,
                Location = new Point(-67, this.Size.Height - 96),
                Parent = this,
                Visible = false
            };
            RightCap = new MirImageControl
            {
                Index = 13,
                Library = Libraries.Prguse,
                Location = new Point(1024, this.Size.Height - 104),
                Parent = this,
                Visible = false
            };

            if (Settings.Resolution > 1024)
            {
                LeftCap.Visible = true;
                RightCap.Visible = true;
            }

            InventoryButton = new MirButton
            {
                HoverIndex = 1904,
                Index = 1903,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 96, 76),
                Parent = this,
                PressedIndex = 1905,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Inventory (") + CMain.InputKeys.GetKey(KeybindOptions.Inventory) + ")"
            };
            InventoryButton.Click += (o, e) =>
            {
                if (GameScene.Scene.InventoryDialog.Visible)
                    GameScene.Scene.InventoryDialog.Hide();
                else
                    GameScene.Scene.InventoryDialog.Show();
            };

            CharacterButton = new MirButton
            {
                HoverIndex = 1901,
                Index = 1900,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 119, 76),
                Parent = this,
                PressedIndex = 1902,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Character (") + CMain.InputKeys.GetKey(KeybindOptions.Equipment) + ")"
            };
            CharacterButton.Click += (o, e) =>
            {
                if (GameScene.Scene.CharacterDialog.Visible && GameScene.Scene.CharacterDialog.CharacterPage.Visible)
                    GameScene.Scene.CharacterDialog.Hide();
                else
                {
                    GameScene.Scene.CharacterDialog.Show();
                    GameScene.Scene.CharacterDialog.ShowCharacterPage();
                }
            };

            SkillButton = new MirButton
            {
                HoverIndex = 1907,
                Index = 1906,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 73, 76),
                Parent = this,
                PressedIndex = 1908,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Skills (") + CMain.InputKeys.GetKey(KeybindOptions.Skills) + ")"
            };
            SkillButton.Click += (o, e) =>
            {
                if (GameScene.Scene.CharacterDialog.Visible && GameScene.Scene.CharacterDialog.SkillPage.Visible)
                    GameScene.Scene.CharacterDialog.Hide();
                else
                {
                    GameScene.Scene.CharacterDialog.Show();
                    GameScene.Scene.CharacterDialog.ShowSkillPage();
                }
            };

            QuestButton = new MirButton
            {
                HoverIndex = 1910,
                Index = 1909,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 50, 76),
                Parent = this,
                PressedIndex = 1911,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Quests (") + CMain.InputKeys.GetKey(KeybindOptions.Quests) + ")"
            };
            QuestButton.Click += (o, e) =>
            {
                if (!GameScene.Scene.QuestLogDialog.Visible)
                    GameScene.Scene.QuestLogDialog.Show();
                else GameScene.Scene.QuestLogDialog.Hide();
            };

            OptionButton = new MirButton
            {
                HoverIndex = 1913,
                Index = 1912,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 27, 76),
                Parent = this,
                PressedIndex = 1914,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Options (") + CMain.InputKeys.GetKey(KeybindOptions.Options) + ")"
            };
            OptionButton.Click += (o, e) =>
            {
                if (!GameScene.Scene.OptionDialog.Visible)
                    GameScene.Scene.OptionDialog.Show();
                else GameScene.Scene.OptionDialog.Hide();
            };

            MenuButton = new MirButton
            {
                HoverIndex = 1961,
                Index = 1960,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 55, 35),
                Parent = this,
                PressedIndex = 1962,
                Sound = SoundList.ButtonC,
                Hint = CMain.Tr("Menu")
            };
            MenuButton.Click += (o, e) =>
            {
                if (!GameScene.Scene.MenuDialog.Visible) GameScene.Scene.MenuDialog.Show();
                else GameScene.Scene.MenuDialog.Hide();
            };

            GameShopButton = new MirButton
            {
                HoverIndex = 827,
                Index = 826,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 105, 35),
                Parent = this,
                PressedIndex = 828,
                Sound = SoundList.ButtonC,
                Hint = CMain.Tr("Game Shop (") + CMain.InputKeys.GetKey(KeybindOptions.GameShop) + ")"
            };
            GameShopButton.Click += (o, e) =>
            {
                if (!GameScene.Scene.GameShopDialog.Visible) GameScene.Scene.GameShopDialog.Show();
                else GameScene.Scene.GameShopDialog.Hide();
            };

            HealthOrb = new MirControl
            {
                Parent = this,
                Location = new Point(0, 30),
                NotControl = true,
            };

            HealthOrb.BeforeDraw += HealthOrb_BeforeDraw;

            HealthLabel = new MirLabel
            {
                AutoSize = true,
                Location = new Point(0, 27),
                Parent = HealthOrb
            };
            HealthLabel.SizeChanged += Label_SizeChanged;

            ManaLabel = new MirLabel
            {
                AutoSize = true,
                Location = new Point(0, 42),
                Parent = HealthOrb
            };
            ManaLabel.SizeChanged += Label_SizeChanged;

            TopLabel = new MirLabel
            {
                Size = new Size(85, 30),
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Location = new Point(9, 20),
                Parent = HealthOrb,
            };

            BottomLabel = new MirLabel
            {
                Size = new Size(85, 30),
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Location = new Point(9, 50),
                Parent = HealthOrb,
            };

            LevelLabel = new MirLabel
            {
                AutoSize = true,
                Parent = this,
                Location = new Point(5, 108)
            };

            CharacterName = new MirLabel
            {
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                Location = new Point(6, 120),
                Size = new Size(90, 16)
            };


            ExperienceBar = new MirImageControl
            {
                Index = Settings.Resolution != 800 ? 8 : 7,
                Library = Libraries.Prguse,
                Location = new Point(9, 143),
                Parent = this,
                DrawImage = false,
                NotControl = true,
            };
            ExperienceBar.BeforeDraw += ExperienceBar_BeforeDraw;

            ExperienceLabel = new MirLabel
            {
                AutoSize = true,
                Parent = ExperienceBar,
                NotControl = true,
            };

            GoldLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.VerticalCenter,
                Font = new Font(Settings.FontName, 8F),
                Location = new Point(this.Size.Width - 105, 119),
                Parent = this,
                Size = new Size(99, 13),
                Sound = SoundList.Gold,
            };
            GoldLabel.Click += (o, e) =>
            {
                if (GameScene.SelectedCell == null)
                    GameScene.PickedUpGold = !GameScene.PickedUpGold && GameScene.Gold > 0;
            };



            WeightBar = new MirImageControl
            {
                Index = 76,
                Library = Libraries.Prguse,
                Location = new Point(this.Size.Width - 105, 103),
                Parent = this,
                DrawImage = false,
                NotControl = true,
            };
            WeightBar.BeforeDraw += WeightBar_BeforeDraw;

            WeightLabel = new MirLabel
            {
                Parent = this,
                Location = new Point(this.Size.Width - 105, 101),
                Size = new Size(40, 14),
            };

            SpaceLabel = new MirLabel
            {
                Parent = this,
                Location = new Point(this.Size.Width - 30, 101),
                Size = new Size(26, 14),
            };


            AModeLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.Yellow,
                OutLineColour = Color.Black,
                Parent = this,
                Location = new Point(Settings.Resolution != 800 ? 899 : 675, Settings.Resolution != 800 ? -448 : -280),
            };

            PModeLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.Orange,
                OutLineColour = Color.Black,
                Parent = this,
                Location = new Point(230, 125),
                Visible = false
            };

            SModeLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.LimeGreen,
                OutLineColour = Color.Black,
                Parent = this,
                Location = new Point(Settings.Resolution != 800 ? 899 : 675, Settings.Resolution != 800 ? -463 : -295),
            };

        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public void OnAModeChanged()
        {
            switch (GameScene.Scene.AMode)
            {
                case AttackMode.Peace:
                    AModeLabel.Text = CMain.Tr("[Mode: Peaceful]");
                    break;
                case AttackMode.Group:
                    AModeLabel.Text = CMain.Tr("[Mode: Group]");
                    break;
                case AttackMode.Guild:
                    AModeLabel.Text = CMain.Tr("[Mode: Guild]");
                    break;
                case AttackMode.EnemyGuild:
                    AModeLabel.Text = CMain.Tr("[Mode: Enemy Guild]");
                    break;
                case AttackMode.RedBrown:
                    AModeLabel.Text = CMain.Tr("[Mode: Red/Brown]");
                    break;
                case AttackMode.All:
                    AModeLabel.Text = CMain.Tr("[Mode: Attack All]");
                    break;
            }

            AModeLabel.Text = CMain.Tr(AModeLabel.Text);
        }

        public void OnPModeChanged()
        {
            switch (GameScene.Scene.PMode)
            {
                case PetMode.Both:
                    PModeLabel.Text = CMain.Tr("[Pet: Attack and Move]");
                    break;
                case PetMode.MoveOnly:
                    PModeLabel.Text = CMain.Tr("[Pet: Do Not Attack]");
                    break;
                case PetMode.AttackOnly:
                    PModeLabel.Text = CMain.Tr("[Pet: Do Not Move]");
                    break;
                case PetMode.None:
                    PModeLabel.Text = CMain.Tr("[Pet: Do Not Attack or Move]");
                    break;
            }

            PModeLabel.Text = CMain.Tr(PModeLabel.Text);
        }

        public void OnPingChanged()
        {
            if (GameScene.Scene.PingTime < 100)
            {
                PingLabel.ForeColour = Color.Lime;
            }
            else if (GameScene.Scene.PingTime < 300)
            {
                PingLabel.ForeColour = Color.Yellow;
            }
            else
            {
                PingLabel.ForeColour = Color.Red;
            }

            PingLabel.Text = string.Format("Ping: {0}  FPS: {1}", GameScene.Scene.PingTime, CMain.FPS);
            PingLabel.Visible = Settings.ShowPing;
        }

        public void OnSkillModeChanged()
        {
            switch (Settings.SkillMode)
            {
                case true:
                    SModeLabel.Text = CMain.Tr("[Skill Mode: ~]");
                    break;
                case false:
                    SModeLabel.Text = CMain.Tr("[Skill Mode: Ctrl]");
                    break;
            }

            SModeLabel.Text = CMain.Tr(SModeLabel.Text);
        }

        public void Process()
        {
            OnSkillModeChanged();
            OnPingChanged();
            if (Settings.HPView)
            {
                HealthLabel.Text = string.Format("HP {0}/{1}", User.HP, User.MaxHP);
                ManaLabel.Text = HPOnly ? "" : string.Format("MP {0}/{1} ", User.MP, User.MaxMP);
                TopLabel.Text = string.Empty;
                BottomLabel.Text = string.Empty;
            }
            else
            {
                if (HPOnly)
                {
                    TopLabel.Text = string.Format("{0}\n" + "--", User.HP);
                    BottomLabel.Text = string.Format("{0}", User.MaxHP);
                }
                else
                {
                    TopLabel.Text = string.Format(" {0}    {1} \n" + "---------------", User.HP, User.MP);
                    BottomLabel.Text = string.Format(" {0}    {1} ", User.MaxHP, User.MaxMP);
                }
                HealthLabel.Text = string.Empty;
                ManaLabel.Text = string.Empty;
            }

            LevelLabel.Text = User.Level.ToString();
            ExperienceLabel.Text = string.Format("{0:#0.##%}", User.Experience / (double)User.MaxExperience);
            ExperienceLabel.Location = new Point((ExperienceBar.Size.Width / 2) - 20, -10);
            GoldLabel.Text = GameScene.Gold.ToString("###,###,##0");
            CharacterName.Text = User.Name;
            SpaceLabel.Text = User.Inventory.Count(t => t == null).ToString();
            WeightLabel.Text = (MapObject.User.MaxBagWeight - MapObject.User.CurrentBagWeight).ToString();
        }

        private void Label_SizeChanged(object sender, EventArgs e)
        {
            MirLabel l = sender as MirLabel;

            if (l == null) return;

            l.Location = new Point(50 - (l.Size.Width / 2), l.Location.Y);
        }

        private void HealthOrb_BeforeDraw(object sender, EventArgs e)
        {
            if (Libraries.Prguse == null) return;

            int height;
            if (User != null && User.HP != User.MaxHP)
                height = (int)(80 * User.HP / (float)User.MaxHP);
            else
                height = 80;

            if (height < 0) height = 0;
            if (height > 80) height = 80;

            int orbImage = 4;

            bool hpOnly = false;

            if (HPOnly)
            {
                hpOnly = true;
                orbImage = 6;
            }

            Rectangle r = new Rectangle(0, 80 - height, hpOnly ? 100 : 50, height);
            Libraries.Prguse.Draw(orbImage, r, new Point(((Settings.ScreenWidth / 2) - (Size.Width / 2)), HealthOrb.DisplayLocation.Y + 80 - height), Color.White, false);

            if (hpOnly) return;

            if (User != null && User.MP != User.MaxMP)
                height = (int)(80 * User.MP / (float)User.MaxMP);
            else
                height = 80;

            if (height < 0) height = 0;
            if (height > 80) height = 80;
            r = new Rectangle(51, 80 - height, 50, height);

            Libraries.Prguse.Draw(4, r, new Point(((Settings.ScreenWidth / 2) - (Size.Width / 2)) + 51, HealthOrb.DisplayLocation.Y + 80 - height), Color.White, false);
        }

        private void ExperienceBar_BeforeDraw(object sender, EventArgs e)
        {
            if (ExperienceBar.Library == null) return;
            if (MapObject.User == null) return;

            double percent = MapObject.User.Experience / (double)MapObject.User.MaxExperience;
            if (percent > 1) percent = 1;
            if (percent <= 0) return;

            Rectangle section = new Rectangle
            {
                Size = new Size((int)((ExperienceBar.Size.Width - 3) * percent), ExperienceBar.Size.Height)
            };

            ExperienceBar.Library.Draw(ExperienceBar.Index, section, ExperienceBar.DisplayLocation, Color.White, false);
        }

        private void WeightBar_BeforeDraw(object sender, EventArgs e)
        {
            if (WeightBar.Library == null) return;
            if (MapObject.User == null) return;
            double percent = MapObject.User.CurrentBagWeight / (double)MapObject.User.MaxBagWeight;
            if (percent > 1) percent = 1;
            if (percent <= 0) return;

            Rectangle section = new Rectangle
            {
                Size = new Size((int)((WeightBar.Size.Width - 2) * percent), WeightBar.Size.Height)
            };

            WeightBar.Library.Draw(WeightBar.Index, section, WeightBar.DisplayLocation, Color.White, false);
        }
    }
}
