using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirSounds;
using C = ClientPackets;
using S = ServerPackets;
using System.Threading;
using System.IO;

namespace Client.MirScenes
{
    public class SelectScene : MirScene
    {
        public MirImageControl Background, Title;
        private NewCharacterDialog _character;

        public MirLabel ServerLabel;
        public MirAnimatedControl CharacterDisplay;
        public MirButton StartGameButton, NewCharacterButton, DeleteCharacterButton, CreditsButton, ExitGame;
        public CharacterButton[] CharacterButtons;
        public MirLabel LastAccessLabel, LastAccessLabelLabel;
        public List<SelectInfo> Characters = new List<SelectInfo>();
        private int _selected;

        public SelectScene(List<SelectInfo> characters)
        {
            SoundManager.PlaySound(SoundList.SelectMusic, true);
            Disposing += (o, e) => SoundManager.StopSound(SoundList.SelectMusic);

            Characters = characters;
            SortList();

            KeyPress +=SelectScene_KeyPress;

            Background = new MirImageControl
            {
                Index = 64,
                Library = Libraries.Prguse,
                Parent = this,
            };

            Title = new MirImageControl
            {
                Index = 40,
                Library = Libraries.Title,
                Parent = this,
                Location = new Point(364, 12)
            };

            ServerLabel = new MirLabel
            {
                Location = new Point(322, 44),
                Parent = Background,
                Size = new Size(155, 17),
                Text = Network.ServerInfo.Name,// "Legend of Mir 2",
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            };

            StartGameButton = new MirButton
            {
                Enabled = false,
                HoverIndex = 341,
                Index = 340,
                Library = Libraries.Title,
                Location = new Point(110, 568),
                Parent = Background,
                PressedIndex = 342,
            };
            StartGameButton.Click += (o, e) => StartGame();

            NewCharacterButton = new MirButton
                {
                    HoverIndex = 344,
                    Index = 343,
                    Library = Libraries.Title,
                    Location = new Point(230, 568),
                    Parent = Background,
                    PressedIndex = 345,
                };
            NewCharacterButton.Click += (o, e) => _character = new NewCharacterDialog { Parent = this };

            DeleteCharacterButton = new MirButton
            {
                HoverIndex = 347,
                Index = 346,
                Library = Libraries.Title,
                Location = new Point(350, 568),
                Parent = Background,
                PressedIndex = 348
            };
            DeleteCharacterButton.Click += (o, e) => DeleteCharacter();


            CreditsButton = new MirButton
            {
                HoverIndex = 350,
                Index = 349,
                Library = Libraries.Title,
                Location = new Point(470, 568),
                Parent = Background,
                PressedIndex = 351
            };

            ExitGame = new MirButton
            {
                HoverIndex = 353,
                Index = 352,
                Library = Libraries.Title,
                Location = new Point(590, 568),
                Parent = Background,
                PressedIndex = 354
            };
            ExitGame.Click += (o, e) => Program.Form.Close();


            CharacterDisplay = new MirAnimatedControl
            {
                Animated = true,
                AnimationCount = 16,
                AnimationDelay = 250,
                FadeIn = true,
                FadeInDelay = 75,
                FadeInRate = 0.1F,
                Index = 220,
                Library = Libraries.ChrSel,
                Location = new Point(200, 300),
                Parent = Background,
                UseOffSet = true,
                Visible = false
            };
            CharacterDisplay.AfterDraw += (o, e) =>
            {
                // if (_selected >= 0 && _selected < Characters.Count && characters[_selected].Class == MirClass.Wizard)
                Libraries.ChrSel.DrawBlendEx(CharacterDisplay.Index + 560, CharacterDisplay.DisplayLocationWithoutOffSet, Color.White, true);
            };

            CharacterButtons = new CharacterButton[4];

            CharacterButtons[0] = new CharacterButton
            {
                Location = new Point(447, 122),
                Parent = Background,
                Sound = SoundList.ButtonA,
            };
            CharacterButtons[0].Click += (o,e) =>
            {
                if (characters.Count <= 0) return;

                _selected = 0;
                UpdateInterface();
            };

            CharacterButtons[1] = new CharacterButton
            {
                Location = new Point(447, 226),
                Parent = Background,
                Sound = SoundList.ButtonA,
            };
            CharacterButtons[1].Click += (o, e) =>
            {
                if (characters.Count <= 1) return;
                _selected = 1;
                UpdateInterface();
            };

            CharacterButtons[2] = new CharacterButton
            {
                Location = new Point(447, 330),
                Parent = Background,
                Sound = SoundList.ButtonA,
            };
            CharacterButtons[2].Click += (o, e) =>
            {
                if (characters.Count <= 2) return;

                _selected = 2;
                UpdateInterface();
            };

            CharacterButtons[3] = new CharacterButton
            {
                Location = new Point(447, 434),
                Parent = Background,
                Sound = SoundList.ButtonA,
            };
            CharacterButtons[3].Click += (o, e) =>
            {
                if (characters.Count <= 3) return;

                _selected = 3;
                UpdateInterface();
            };

            LastAccessLabel = new MirLabel
            {
                Location = new Point(140, 509),
                Parent = Background,
                Size = new Size(180, 21),
                DrawFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter,
                Border = true,
            };
            LastAccessLabelLabel = new MirLabel
                {
                    Location = new Point(-100, -1),
                    Parent = LastAccessLabel,
                    Text = CMain.Tr("Last Online:"),
                    Size = new Size(100, 21),
                    DrawFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter,
                    Border = true,
                };
            UpdateInterface();
        }

        private void SelectScene_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            if (StartGameButton.Enabled)
                StartGame();
            e.Handled = true;
        }


        public void SortList()
        {
            if (Characters != null)
                Characters.Sort((c1, c2) => c2.LastAccess.CompareTo(c1.LastAccess));
        }


        public void StartGame()
        {
            if (!Libraries.Loaded)
            {
                int progress = Libraries.Progress > 100 ? 100 : Libraries.Progress;

                MirMessageBox message = new MirMessageBox(string.Format(CMain.Tr("Please wait, The game is still loading... {0:##0}%"), progress / (double)Libraries.Count * 100), MirMessageBoxButtons.Cancel);

                message.BeforeDraw += (o, e) => message.Label.Text = string.Format(CMain.Tr("Please wait, The game is still loading... {0:##0}%"), progress / (double)Libraries.Count * 100);

                message.AfterDraw += (o, e) =>
                {
                    if (!Libraries.Loaded) return;
                    message.Dispose();
                    StartGame();
                };

                message.Show();

                return;
            }
            StartGameButton.Enabled = false;

            Network.Enqueue(new C.StartGame
            {
                CharacterIndex = Characters[_selected].Index
            });

            if (!Directory.Exists("./Configs/")) Directory.CreateDirectory("./Configs/");
            Settings.AssistReader = new InIReader("./Configs/" + Characters[_selected].Name + ".txt");
            InIAttribute.ReadInI<Settings>(Settings.AssistReader);
        }

        public override void Process()
        {
            

        }
        public override void ProcessPacket(Packet p)
        {
            switch (p.Index)
            {
                case (short)ServerPacketIds.NewCharacter:
                    NewCharacter((S.NewCharacter)p);
                    break;
                case (short)ServerPacketIds.NewCharacterSuccess:
                    NewCharacter((S.NewCharacterSuccess)p);
                    break;
                case (short)ServerPacketIds.DeleteCharacter:
                    DeleteCharacter((S.DeleteCharacter)p);
                    break;
                case (short)ServerPacketIds.DeleteCharacterSuccess:
                    DeleteCharacter((S.DeleteCharacterSuccess)p);
                    break;
                case (short)ServerPacketIds.StartGame:
                    StartGame((S.StartGame)p);
                    break;
                case (short)ServerPacketIds.StartGameBanned:
                    StartGame((S.StartGameBanned)p);
                    break;
                case (short)ServerPacketIds.StartGameDelay:
                    StartGame((S.StartGameDelay) p);
                    break;
                default:
                    base.ProcessPacket(p);
                    break;
            }
        }

        private void NewCharacter(S.NewCharacter p)
        {
            _character.OKButton.Enabled = true;
            
            switch (p.Result)
            {
                case 0:
                    MirMessageBox.Show(CMain.Tr("Creating new characters is currently disabled."));
                    _character.Dispose();
                    break;
                case 1:
                    MirMessageBox.Show(CMain.Tr("Your Character Name is not acceptable."));
                    _character.NameTextBox.SetFocus();
                    break;
                case 2:
                    MirMessageBox.Show(CMain.Tr("The gender you selected does not exist.\n Contact a GM for assistance."));
                    break;
                case 3:
                    MirMessageBox.Show(CMain.Tr("The class you selected does not exist.\n Contact a GM for assistance."));
                    break;
                case 4:
                    MirMessageBox.Show(CMain.Tr("You cannot make anymore then ") + Globals.MaxCharacterCount + CMain.Tr(" Characters."));
                    _character.Dispose();
                    break;
                case 5:
                    MirMessageBox.Show(CMain.Tr("A Character with this name already exists."));
                    _character.NameTextBox.SetFocus();
                    break;
            }


        }
        private void NewCharacter(S.NewCharacterSuccess p)
        {
            _character.Dispose();
            MirMessageBox.Show(CMain.Tr("Your character was created successfully."));
            
            Characters.Insert(0, p.CharInfo);
            _selected = 0;
            UpdateInterface();
        }

        private void DeleteCharacter()
        {
            if (_selected < 0 || _selected >= Characters.Count) return;

            MirMessageBox message = new MirMessageBox(CMain.Format(CMain.Tr("Are you sure you want to Delete the character {0}?"), Characters[_selected].Name), MirMessageBoxButtons.YesNo);
            int index = Characters[_selected].Index;

            message.YesButton.Click += (o, e) =>
            {
                DeleteCharacterButton.Enabled = false;
                Network.Enqueue(new C.DeleteCharacter { CharacterIndex = index });
            };

            message.Show();
        }

        private void DeleteCharacter(S.DeleteCharacter p)
        {
            DeleteCharacterButton.Enabled = true;
            switch (p.Result)
            {
                case 0:
                    MirMessageBox.Show(CMain.Tr("Deleting characters is currently disabled."));
                    break;
                case 1:
                    MirMessageBox.Show(CMain.Tr("The character you selected does not exist.\n Contact a GM for assistance."));
                    break;
            }
        }
        private void DeleteCharacter(S.DeleteCharacterSuccess p)
        {
            DeleteCharacterButton.Enabled = true;
            MirMessageBox.Show(CMain.Tr("Your character was deleted successfully."));

            for (int i = 0; i < Characters.Count; i++)
                if (Characters[i].Index == p.CharacterIndex)
                {
                    Characters.RemoveAt(i);
                    break;
                }

            UpdateInterface();
        }

        private void StartGame(S.StartGameDelay p)
        {
            StartGameButton.Enabled = true;

            long time = CMain.Time + p.Milliseconds;

            MirMessageBox message = new MirMessageBox(string.Format("You cannot log onto this character for another {0} seconds.", Math.Ceiling(p.Milliseconds/1000M)));

            message.BeforeDraw += (o, e) => message.Label.Text = string.Format("You cannot log onto this character for another {0} seconds.", Math.Ceiling((time - CMain.Time)/1000M));
                

            message.AfterDraw += (o, e) =>
            {
                if (CMain.Time <= time) return;
                message.Dispose();
                StartGame();
            };

            message.Show();
        }
        public void StartGame(S.StartGameBanned p)
        {
            StartGameButton.Enabled = true;

            TimeSpan d = p.ExpiryDate - CMain.Now;
            MirMessageBox.Show(string.Format("This account is banned.\n\nReason: {0}\nExpiryDate: {1}\nDuration: {2:#,##0} Hours, {3} Minutes, {4} Seconds", p.Reason,
                                             p.ExpiryDate, Math.Floor(d.TotalHours), d.Minutes, d.Seconds));
        }
        public void StartGame(S.StartGame p)
        {
            StartGameButton.Enabled = true;

            if (p.Resolution < Settings.Resolution || Settings.Resolution == 0) Settings.Resolution = p.Resolution;

            if (p.Resolution < 1024 || Settings.Resolution < 1024) Settings.Resolution = 800;
            else if (p.Resolution < 1366 || Settings.Resolution < 1280) Settings.Resolution = 1024;
            else if (p.Resolution < 1366 || Settings.Resolution < 1366) Settings.Resolution = 1280;//not adding an extra setting for 1280 on server cause well it just depends on the aspect ratio of your screen
            else if (p.Resolution >= 1366 && Settings.Resolution >= 1366) Settings.Resolution = 1366;

            switch (p.Result)
            {
                case 0:
                    MirMessageBox.Show(CMain.Tr("Starting the game is currently disabled."));
                    break;
                case 1:
                    MirMessageBox.Show(CMain.Tr("You are not logged in."));
                    break;
                case 2:
                    MirMessageBox.Show(CMain.Tr("Your character could not be found."));
                    break;
                case 3:
                    MirMessageBox.Show(CMain.Tr("No active map and/or start point found."));
                    break;
                case 4:
                    if (Settings.Resolution == 1024)
                        CMain.SetResolution(1024, 768);
                    else if (Settings.Resolution == 1280)
                        CMain.SetResolution(1280, 800);
                    else if (Settings.Resolution == 1366)
                        CMain.SetResolution(1366, 768);
                    ActiveScene = new GameScene();
                    Dispose();
                    break;
            }
        }
        private void UpdateInterface()
        {
            for (int i = 0; i < CharacterButtons.Length; i++)
            {
                CharacterButtons[i].Selected = i == _selected;
                CharacterButtons[i].Update(i >= Characters.Count ? null : Characters[i]);
            }

            if (_selected >= 0 && _selected < Characters.Count)
            {
                CharacterDisplay.Visible = true;
                //CharacterDisplay.Index = ((byte)Characters[_selected].Class + 1) * 20 + (byte)Characters[_selected].Gender * 280; 

                switch ((MirClass)Characters[_selected].Class)
                {
                    case MirClass.Warrior:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 20 : 300; //220 : 500;
                        break;
                    case MirClass.Wizard:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 40 : 320; //240 : 520;
                        break;
                    case MirClass.Taoist:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 60 : 340; //260 : 540;
                        break;
                    case MirClass.Assassin:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 80 : 360; //280 : 560;
                        break;
                    case MirClass.Archer:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 100 : 140; //160 : 180;
                        break;
                    case MirClass.Monk:
                        CharacterDisplay.Index = 1140; //Monk (no female monk)
                        break;
                }

                LastAccessLabel.Text = Characters[_selected].LastAccess == DateTime.MinValue ? CMain.Tr("Never") : Characters[_selected].LastAccess.ToString();
                LastAccessLabel.Visible = true;
                LastAccessLabelLabel.Visible = true;
                StartGameButton.Enabled = true;
            }
            else
            {
                CharacterDisplay.Visible = false;
                LastAccessLabel.Visible = false;
                LastAccessLabelLabel.Visible = false;
                StartGameButton.Enabled = false;
            }
        }


        #region Disposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Background = null;
                _character = null;

                ServerLabel = null;
                CharacterDisplay = null;
                StartGameButton = null;
                NewCharacterButton = null;
                DeleteCharacterButton = null; 
                CreditsButton = null;
                ExitGame = null;
                CharacterButtons = null;
                LastAccessLabel = null;LastAccessLabelLabel = null;
                Characters  = null;
                _selected = 0;
            }

            base.Dispose(disposing);
        }
        #endregion
        public sealed class NewCharacterDialog : MirImageControl
        {
            public MirImageControl TitleLabel;
            public MirAnimatedControl CharacterDisplay;

            public MirButton OKButton,
                             CancelButton,
                             WarriorButton,
                             WizardButton,
                             TaoistButton,
                             AssassinButton,
                             ArcherButton,
                             MonkButton,
                             MaleButton,
                             FemaleButton;

            public MirTextBox NameTextBox;

            public MirLabel Description;

            private MirClass _class;
            private MirGender _gender;

            #region Descriptions
            public const string WarriorDescription =
               "       战士是近身作战的专家，他孔武有力，使用重型武器统治战场，胆敢面对他的敌人都会在须臾间被大卸八块。战士可以迅速突入敌阵，以大开大合的招式掀起毁灭的风暴，收割大片弱小敌人的性命；在面对强大的敌人时，战士则召唤雷霆与火焰增强手中的武器，在巨力的加持下给胆敢挑战他的人开瓢。\n       他所向披靡，是战场上的绞肉机，给队友他带来胜利，给敌人则带去死亡。";

            public const string WizardDescription =
                "       法师是彻头彻尾的神秘主义者，他体质孱弱，依靠防御法术抵御攻击，然后根据战场局势选择恰当的法术，无边火海焚尽万物，霹雳闪电震慑鬼魅，极地冰霜冻结一切，空间法术神出鬼没……法师在战场上向来无往不利。\n       传说中强大的法师能够同时出现的不同的地方，拥有呼唤流星毁天灭地的伟大力量，有些地区把这样的法师当作神一样崇拜，虽然法师清楚自己并不是神，但依然被尊称为——法神。";

            public const string TaoistDescription =
                "       道士手持折扇，气质超然，挥挥手能令人重伤痊愈，谈笑间便叫敌身中剧毒，黄纸作符，能灼伤灵魂，真气鼓荡，能激发潜力。道士是当之无愧的中流砥柱，无论魔法还是刀剑，都能从容应对，亦能沟通天地，隐匿身形，或是使用仙家术法驱使鬼物仆役或是召唤上古凶兽前来护法。\n       悬壶济世，通晓阴阳，道士是传说中的仙人苗裔，具有神鬼莫测之能，众皆称其为——天尊。";

            public const string AssassinDescription =
                "       刺客翩若惊鸿，剑出游龙，疾风骤雨般的攻势，从四面八方带给敌人毫不掩饰的杀意。刺客为追求胜利无所不用其极。他剑术精湛，两把短刃迅猛诡秘，能从最不可思议的角度刺入敌人的要害；他精通毒物，最强壮的妖魔也要在他的剧毒之下饮恨；他善于利用环境，能够遁入阴影，在敌人毫无防备之时发动突袭，取敌性命。 \n       刺客半生与杀戮为伍，有关他的悬赏已是天文数字，然而敢打他主意的家伙都在路边水沟里寻找自己失落的脑袋。";

            public const string ArcherDescription =
                "       弓手的箭法如长虹贯日，能轻易于万军护卫之下取其大将性命。当敌人纷纷远离以为小命无虞时，突如其来的一箭会让敌人在惊愕中毙命；当敌人躲在障碍物之后，以为能够阻挡飞矢，天外飞仙般的一箭会让敌人带着疑问死去。弓手学究天人，不仅箭术通神，也通晓毒物，能够为箭矢淬毒，他在魔法一道也有不俗的造诣，能够加持箭矢，发射矫健的火龙或是噬血的魔箭，他甚至还擅长驯兽，能驱使野兽为自己服务。 \n       他的技艺如此高超，早已超出凡人的理解，追魂索命的箭矢是敌人永远无法摆脱的梦魇。";

            public const string MonkDescription =
                "       武僧本在寺庙中潜心精修不问世事，有一天当他欲入世渡人时，看到的却是横行的强盗，肆虐的妖魔，满目疮痍的大地，武僧遂发宏愿要以雷霆手段伏魔荡寇诛除妖邪。武僧修为高深，超凡入圣，他的肉体比厚重的铠甲更为强横，利刃连他的皮肤都无法划破，钝器则会被护身的气墙弹开，就连妖魔的攻击都无法突破他的防御，无论多么凶悍的敌人，都在武僧面前乖乖授首伏法，他高深的佛法勘破轮回，甚至能让队友起死回生，再战人间。 \n       佛有善目菩萨，亦有怒目金刚。";

            #endregion

            public NewCharacterDialog()
            {
                Index = 73;
                Library = Libraries.Prguse;
                Location = new Point((Settings.ScreenWidth - Size.Width)/2, (Settings.ScreenHeight - Size.Height)/2);
                Modal = true;

                TitleLabel = new MirImageControl
                    {
                        Index = 20,
                        Library = Libraries.Title,
                        Location = new Point(206, 11),
                        Parent = this,
                    };

                CancelButton = new MirButton
                    {
                        HoverIndex = 281,
                        Index = 280,
                        Library = Libraries.Title,
                        Location = new Point(425, 425),
                        Parent = this,
                        PressedIndex = 282
                    };
                CancelButton.Click += (o, e) => Dispose();


                OKButton = new MirButton
                    {
                        Enabled = false,
                        HoverIndex = 361,
                        Index = 360,
                        Library = Libraries.Title,
                        Location = new Point(160, 425),
                        Parent = this,
                        PressedIndex = 362,
                    };
                OKButton.Click += (o, e) => CreateCharacter();

                NameTextBox = new MirTextBox
                    {
                        Location = new Point(325, 268),
                        Parent = this,
                        Size = new Size(240, 20),
                        MaxLength = Globals.MaxCharacterNameLength
                    };
                NameTextBox.TextBox.KeyPress += TextBox_KeyPress;
                NameTextBox.TextBox.TextChanged += CharacterNameTextBox_TextChanged;
                NameTextBox.SetFocus();

                CharacterDisplay = new MirAnimatedControl
                    {
                        Animated = true,
                        AnimationCount = 16,
                        AnimationDelay = 250,
                        Index = 20,
                        Library = Libraries.ChrSel,
                        Location = new Point(120, 250),
                        Parent = this,
                        UseOffSet = true,
                    };
                CharacterDisplay.AfterDraw += (o, e) =>
                    {
                        if (_class == MirClass.Wizard)
                            Libraries.ChrSel.DrawBlendEx(CharacterDisplay.Index + 560, CharacterDisplay.DisplayLocationWithoutOffSet, Color.White, true);
                    };

                WarriorButton = new MirButton
                    {
                        HoverIndex = 2427,
                        Index = 2427,
                        Library = Libraries.Prguse,
                        Location = new Point(323, 296),
                        Parent = this,
                        PressedIndex = 2428,
                        Sound = SoundList.ButtonA,
                    };
                WarriorButton.Click += (o, e) =>
                    {
                        _class = MirClass.Warrior;
                        UpdateInterface();
                    };

                WizardButton = new MirButton
                    {
                        HoverIndex = 2430,
                        Index = 2429,
                        Library = Libraries.Prguse,
                        Location = new Point(373, 296),
                        Parent = this,
                        PressedIndex = 2431,
                        Sound = SoundList.ButtonA,
                    };
                WizardButton.Click += (o, e) =>
                    {
                        _class = MirClass.Wizard;
                        UpdateInterface();
                    };


                TaoistButton = new MirButton
                    {
                        HoverIndex = 2433,
                        Index = 2432,
                        Library = Libraries.Prguse,
                        Location = new Point(423, 296),
                        Parent = this,
                        PressedIndex = 2434,
                        Sound = SoundList.ButtonA,
                    };
                TaoistButton.Click += (o, e) =>
                    {
                        _class = MirClass.Taoist;
                        UpdateInterface();
                    };

                AssassinButton = new MirButton
                    {
                        HoverIndex = 2436,
                        Index = 2435,
                        Library = Libraries.Prguse,
                        Location = new Point(473, 296),
                        Parent = this,
                        PressedIndex = 2437,
                        Sound = SoundList.ButtonA,
                    };
                AssassinButton.Click += (o, e) =>
                    {
                        _class = MirClass.Assassin;
                        UpdateInterface();
                    };

                ArcherButton = new MirButton
                {
                    HoverIndex = 2439,
                    Index = 2438,
                    Library = Libraries.Prguse,
                    Location = new Point(523, 296),
                    Parent = this,
                    PressedIndex = 2440,
                    Sound = SoundList.ButtonA,
                };
                ArcherButton.Click += (o, e) =>
                {
                    _class = MirClass.Archer;
                    UpdateInterface();
                };

                //Monk
                MonkButton = new MirButton
                {
                    HoverIndex = 2448,
                    Index = 2447,
                    Library = Libraries.Prguse,
                    Location = new Point(523, 343),
                    Parent = this,
                    PressedIndex = 2449,
                    Sound = SoundList.ButtonA,
                };
                MonkButton.Click += (o, e) =>
                {
                    _class = MirClass.Monk;
                    _gender = MirGender.Male; //Monk is male only
                    UpdateInterface();
                };

                MaleButton = new MirButton
                    {
                        HoverIndex = 2421,
                        Index = 2421,
                        Library = Libraries.Prguse,
                        Location = new Point(323, 343),
                        Parent = this,
                        PressedIndex = 2422,
                        Sound = SoundList.ButtonA,
                    };
                MaleButton.Click += (o, e) =>
                    {
                        _gender = MirGender.Male;
                        UpdateInterface();
                    };

                FemaleButton = new MirButton
                    {
                        HoverIndex = 2424,
                        Index = 2423,
                        Library = Libraries.Prguse,
                        Location = new Point(373, 343),
                        Parent = this,
                        PressedIndex = 2425,
                        Sound = SoundList.ButtonA,
                    };
                FemaleButton.Click += (o, e) =>
                    {
                        _gender = MirGender.Female;
                        if (_class == MirClass.Monk)//added to stop female monks being created
                            _gender = MirGender.Male;
                        UpdateInterface();
                    };

                Description = new MirLabel
                {
                    Border = true,
                    Location = new Point(279, 70),
                    Parent = this,
                    Size = new Size(278, 187),
                    Text = WarriorDescription,
                    LineSpace = 3,
                    };
            }

            private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (sender == null) return;
                if (e.KeyChar != (char)Keys.Enter) return;
                e.Handled = true;

                if (OKButton.Enabled)
                    OKButton.InvokeMouseClick(null);
            }
            private void CharacterNameTextBox_TextChanged(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    OKButton.Enabled = false;
                    NameTextBox.Border = false;
                }
                else if (!NameChecker.CheckCharacterName(NameTextBox.Text))
                {
                    OKButton.Enabled = false;
                    NameTextBox.Border = true;
                    NameTextBox.BorderColour = Color.Red;
                }
                else
                {
                    OKButton.Enabled = true;
                    NameTextBox.Border = true;
                    NameTextBox.BorderColour = Color.Green;
                }
            }

            private void CreateCharacter()
            {
                OKButton.Enabled = false;

                Network.Enqueue(new C.NewCharacter
                    {
                        Name = NameTextBox.Text,
                        Class = _class,
                        Gender = _gender
                    });
            }

            private void UpdateInterface()
            {
                MaleButton.Index = 2420;
                FemaleButton.Index = 2423;

                WarriorButton.Index = 2426;
                WizardButton.Index = 2429;
                TaoistButton.Index = 2432;
                AssassinButton.Index = 2435;
                ArcherButton.Index = 2438;
                MonkButton.Index = 2447;

                switch (_gender)
                {
                    case MirGender.Male:
                        MaleButton.Index = 2421;
                        break;
                    case MirGender.Female:
                        FemaleButton.Index = 2424;
                        break;
                }

                switch (_class)
                {
                    case MirClass.Warrior:
                        WarriorButton.Index = 2427;
                        Description.Text = WarriorDescription;
                        CharacterDisplay.Index = (byte)_gender == 0 ? 20 : 300; //220 : 500;
                        break;
                    case MirClass.Wizard:
                        WizardButton.Index = 2430;
                        Description.Text = WizardDescription;
                        CharacterDisplay.Index = (byte)_gender == 0 ? 40 : 320; //240 : 520;
                        break;
                    case MirClass.Taoist:
                        TaoistButton.Index = 2433;
                        Description.Text = TaoistDescription;
                        CharacterDisplay.Index = (byte)_gender == 0 ? 60 : 340; //260 : 540;
                        break;
                    case MirClass.Assassin:
                        AssassinButton.Index = 2436;
                        Description.Text = AssassinDescription;
                        CharacterDisplay.Index = (byte)_gender == 0 ? 80 : 360; //280 : 560;
                        break;
                    case MirClass.Archer:
                        ArcherButton.Index = 2439;
                        Description.Text = ArcherDescription;
                        CharacterDisplay.Index = (byte)_gender == 0 ? 100 : 140; //160 : 180;
                        break;
                    case MirClass.Monk: //Monk
                        MonkButton.Index = 2447;
                        Description.Text = MonkDescription;
                        CharacterDisplay.Index = 1140;
                        break;
                }

                //CharacterDisplay.Index = ((byte)_class + 1) * 20 + (byte)_gender * 280;
            }

        }
        public sealed class CharacterButton : MirImageControl
        {
            public MirLabel NameLabel, LevelLabel, ClassLabel;
            public bool Selected;
            
            public CharacterButton()
            {
                Index = 44; //45 locked
                Library = Libraries.Prguse;
                Sound = SoundList.ButtonA;

                NameLabel = new MirLabel
                {
                    Location = new Point(107, 9),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(170, 18)
                };

                LevelLabel = new MirLabel
                {
                    Location = new Point(107, 28),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(30, 18)
                };

                ClassLabel = new MirLabel
                {
                    Location = new Point(178, 28),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(100, 18)
                };
            }

            public void Update(SelectInfo info)
            {
                if (info == null)
                {
                    Index = 44;
                    Library = Libraries.Prguse;
                    NameLabel.Text = string.Empty;
                    LevelLabel.Text = string.Empty;
                    ClassLabel.Text = string.Empty;

                    NameLabel.Visible = false;
                    LevelLabel.Visible = false;
                    ClassLabel.Visible = false;

                    return;
                }

                Library = Libraries.Title;

                byte offset = (byte)info.Class;
                Index = 658 + offset;

                if (Selected) Index += 6;

                NameLabel.Text = info.Name;
                LevelLabel.Text = info.Level.ToString();
                ClassLabel.Text = CMain.Tr(info.Class.ToString());
                
                NameLabel.Visible = true;
                LevelLabel.Visible = true;
                ClassLabel.Visible = true;
            }
        }
    }
}
