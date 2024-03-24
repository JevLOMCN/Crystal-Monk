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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C = ClientPackets;

namespace Client.MirScenes.Dialogs
{
    public sealed class ChatDialog : MirImageControl
    {
        public List<ChatHistory> FullHistory = new List<ChatHistory>();
        public List<ChatHistory> History = new List<ChatHistory>();
        public List<MirLabel> ChatLines = new List<MirLabel>();

        public MirButton HomeButton, UpButton, EndButton, DownButton, PositionBar;
        public MirImageControl CountBar;
        public MirTextBox ChatTextBox;
        public Font ChatFont = new Font(Settings.FontName, 8F);
        public string LastPM = string.Empty;

        public int StartIndex, LineCount = 4, WindowSize;
        public string ChatPrefix = "";

        public bool Transparent;

        public string InputText;

        public static string[] CompleteText =
        {
            "@info ",
            "@make ",
            "@mapmove ",
            "@querycharacter ",
            "@queryinventory ",
            "@querystorage ",
            "@queryequipment ",
            "@queryonline",
            "@deleteinventory ",
            "@deletestorage ",
            "@deleteequipment ",
            "@querycharacter ",
            "@queryallcharacter ",
            "@onlinecount ",
            "@givegold ",
            "@givecredit ",
            ""
        };

        public static int index;

        public ChatDialog()
        {
            Index = Settings.Resolution != 800 ? 2221 : 2201;
            Library = Libraries.Prguse;
            Location = new Point(GameScene.Scene.MainDialog.Location.X + 230, Settings.ScreenHeight - 97);
            PixelDetect = true;

            KeyPress += ChatPanel_KeyPress;
            KeyDown += ChatPanel_KeyDown;
            MouseWheel += ChatPanel_MouseWheel;

            ChatTextBox = new MirTextBox
            {
                BackColour = Color.DarkGray,
                ForeColour = Color.Black,
                Parent = this,
                Size = new Size(Settings.Resolution != 800 ? 627 : 403, 13),
                Location = new Point(1, 54),
                MaxLength = Globals.MaxChatLength,
                Visible = false,
                Font = ChatFont,
            };
            ChatTextBox.TextBox.KeyPress += ChatTextBox_KeyPress;
            ChatTextBox.TextBox.KeyDown += ChatTextBox_KeyDown;
            ChatTextBox.TextBox.KeyUp += ChatTextBox_KeyUp;
            ChatTextBox.TextBox.AcceptsTab = true;

            HomeButton = new MirButton
            {
                Index = 2018,
                HoverIndex = 2019,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 618 : 394, 1),
                Parent = this,
                PressedIndex = 2020,
                Sound = SoundList.ButtonA,
            };
            HomeButton.Click += (o, e) =>
            {
                if (StartIndex == 0) return;
                StartIndex = 0;
                Update();
            };


            UpButton = new MirButton
            {
                Index = 2021,
                HoverIndex = 2022,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 618 : 394, 9),
                Parent = this,
                PressedIndex = 2023,
                Sound = SoundList.ButtonA,
            };
            UpButton.Click += (o, e) =>
            {
                if (StartIndex == 0) return;
                StartIndex--;
                Update();
            };


            EndButton = new MirButton
            {
                Index = 2027,
                HoverIndex = 2028,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 618 : 394, 45),
                Parent = this,
                PressedIndex = 2029,
                Sound = SoundList.ButtonA,
            };
            EndButton.Click += (o, e) =>
            {
                if (StartIndex == History.Count - 1) return;
                StartIndex = History.Count - 1;
                Update();
            };

            DownButton = new MirButton
            {
                Index = 2024,
                HoverIndex = 2025,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 618 : 394, 39),
                Parent = this,
                PressedIndex = 2026,
                Sound = SoundList.ButtonA,
            };
            DownButton.Click += (o, e) =>
            {
                if (StartIndex == History.Count - 1) return;
                StartIndex++;
                Update();
            };



            CountBar = new MirImageControl
            {
                Index = 2012,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 622 : 398, 16),
                Parent = this,
            };

            PositionBar = new MirButton
            {
                Index = 2015,
                HoverIndex = 2016,
                Library = Libraries.Prguse,
                Location = new Point(Settings.Resolution != 800 ? 619 : 395, 16),
                Parent = this,
                PressedIndex = 2017,
                Movable = true,
                Sound = SoundList.None,
            };
            PositionBar.OnMoving += PositionBar_OnMoving;
        }

        public void ReceiveChatTr(string v, ChatType system)
        {
            v = CMain.Tr(v);
            ReceiveChat(v, system);
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        private void ChatTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    if (!string.IsNullOrEmpty(ChatTextBox.Text))
                    {
                        string msg = ChatTextBox.Text;

                        HandleGMCmd(msg);
     

                        Network.Enqueue(new C.Chat
                        {
                            Message = msg
                        });

                        if (ChatTextBox.Text[0] == '/')
                        {
                            string[] parts = ChatTextBox.Text.Split(' ');
                            if (parts.Length > 0)
                                LastPM = parts[0];
                        }
                    }
                    ChatTextBox.Visible = false;
                    ChatTextBox.Text = string.Empty;
                    break;
                case (char)Keys.Escape:
                    e.Handled = true;
                    ChatTextBox.Visible = false;
                    ChatTextBox.Text = string.Empty;
                    break;

 
            }
        }

        void PositionBar_OnMoving(object sender, MouseEventArgs e)
        {
            int x = Settings.Resolution != 800 ? 619 : 395;
            int y = PositionBar.Location.Y;
            if (y >= 16 + CountBar.Size.Height - PositionBar.Size.Height) y = 16 + CountBar.Size.Height - PositionBar.Size.Height;
            if (y < 16) y = 16;

            int h = CountBar.Size.Height - PositionBar.Size.Height;
            h = (int)((y - 16) / (h / (float)(History.Count - 1)));

            if (h != StartIndex)
            {
                StartIndex = h;
                Update();
            }

            PositionBar.Location = new Point(x, y);
        }

        public void ReceiveChat(string text, ChatType type)
        {
            if (Settings.HideSystem2 && type == ChatType.System2)
                return;

            Color foreColour, backColour;

            switch (type)
            {
                case ChatType.Hint:
                    backColour = Color.White;
                    foreColour = Color.DarkGreen;
                    break;
                case ChatType.Announcement:
                    backColour = Color.Blue;
                    foreColour = Color.White;
                    GameScene.Scene.ChatNoticeDialog.ShowNotice(text);
                    break;
                case ChatType.Shout:
                    backColour = Color.Yellow;
                    foreColour = Color.Black;
                    break;
                case ChatType.Shout2:
                    backColour = Color.Green;
                    foreColour = Color.White;
                    break;
                case ChatType.Shout3:
                    backColour = Color.Purple;
                    foreColour = Color.White;
                    break;
                case ChatType.System:
                    backColour = Color.Red;
                    foreColour = Color.White;
                    break;
                case ChatType.System2:
                    backColour = Color.DarkRed;
                    foreColour = Color.White;
                    break;
                case ChatType.Group:
                    backColour = Color.White;
                    foreColour = Color.Brown;
                    break;
                case ChatType.WhisperOut:
                    foreColour = Color.CornflowerBlue;
                    backColour = Color.White;
                    break;
                case ChatType.WhisperIn:
                    foreColour = Color.DarkBlue;
                    backColour = Color.White;
                    break;
                case ChatType.Guild:
                    backColour = Color.White;
                    foreColour = Color.Green;
                    break;
                case ChatType.LevelUp:
                    backColour = Color.FromArgb(255, 225, 185, 250);
                    foreColour = Color.Blue;
                    break;
                case ChatType.Relationship:
                    backColour = Color.Transparent;
                    foreColour = Color.HotPink;
                    break;
                case ChatType.Mentor:
                    backColour = Color.White;
                    foreColour = Color.Purple;
                    break;
                default:
                    backColour = Color.White;
                    foreColour = Color.Black;
                    break;
            }

            int chatWidth = Settings.Resolution != 800 ? 614 : 390;
            List<string> chat = new List<string>();

            int index = 0;
            for (int i = 1; i < text.Length; i++)
                if (TextRenderer.MeasureText(CMain.Graphics, text.Substring(index, i - index), ChatFont).Width > chatWidth)
                {
                    chat.Add(text.Substring(index, i - index - 1));
                    index = i - 1;
                }
            chat.Add(text.Substring(index, text.Length - index));

            if (StartIndex == History.Count - LineCount)
                StartIndex += chat.Count;

            for (int i = 0; i < chat.Count; i++)
                FullHistory.Add(new ChatHistory { Text = chat[i], BackColour = backColour, ForeColour = foreColour, Type = type });

            Update();
        }

        public void Update()
        {
            History = new List<ChatHistory>();

            for (int i = 0; i < FullHistory.Count; i++)
            {
                switch (FullHistory[i].Type)
                {
                    case ChatType.Normal:
                        if (Settings.FilterNormalChat) continue;
                        break;
                    case ChatType.WhisperIn:
                    case ChatType.WhisperOut:
                        if (Settings.FilterWhisperChat) continue;
                        break;
                    case ChatType.Shout:
                        if (Settings.FilterShoutChat) continue;
                        break;
                    case ChatType.System:
                    case ChatType.System2:
                        if (Settings.FilterSystemChat) continue;
                        break;
                    case ChatType.Group:
                        if (Settings.FilterGroupChat) continue;
                        break;
                    case ChatType.Guild:
                        if (Settings.FilterGuildChat) continue;
                        break;
                }

                History.Add(FullHistory[i]);
            }

            for (int i = 0; i < ChatLines.Count; i++)
                ChatLines[i].Dispose();

            ChatLines.Clear();

            if (StartIndex >= History.Count) StartIndex = History.Count - 1;
            if (StartIndex < 0) StartIndex = 0;

            if (History.Count > 1)
            {
                int h = CountBar.Size.Height - PositionBar.Size.Height;
                h = (int)((h / (float)(History.Count - 1)) * StartIndex);
                PositionBar.Location = new Point(Settings.Resolution != 800 ? 619 : 395, 16 + h);
            }

            int y = 1;

            for (int i = StartIndex; i < History.Count; i++)
            {
                MirLabel temp = new MirLabel
                {
                    AutoSize = true,
                    BackColour = History[i].BackColour,
                    ForeColour = History[i].ForeColour,
                    Location = new Point(1, y),
                    OutLine = false,
                    Parent = this,
                    Text = History[i].Text,
                    Font = ChatFont,
                };
                temp.MouseWheel += ChatPanel_MouseWheel;
                ChatLines.Add(temp);

                temp.Click += (o, e) =>
                {
                    MirLabel l = o as MirLabel;
                    if (l == null) return;

                    string[] parts = l.Text.Split(':', ' ');
                    if (parts.Length == 0) return;

                    string name = parts[0];

                    ChatTextBox.SetFocus();
                    ChatTextBox.Text = name.StartsWith("/") ? name + " " : string.Format("/{0} ", name);
                    ChatTextBox.Visible = true;
                    ChatTextBox.TextBox.SelectionLength = 0;
                    ChatTextBox.TextBox.SelectionStart = ChatTextBox.Text.Length;
                };


                y += 13;
                if (i - StartIndex == LineCount - 1) break;
            }

        }

        private void ChatPanel_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (StartIndex == 0) return;
                    StartIndex--;
                    break;
                case Keys.Home:
                    if (StartIndex == 0) return;
                    StartIndex = 0;
                    break;
                case Keys.Down:
                    if (StartIndex == History.Count - 1) return;
                    StartIndex++;
                    break;
                case Keys.End:
                    if (StartIndex == History.Count - 1) return;
                    StartIndex = History.Count - 1;
                    break;
                case Keys.PageUp:
                    if (StartIndex == 0) return;
                    StartIndex -= LineCount;
                    break;
                case Keys.PageDown:
                    if (StartIndex == History.Count - 1) return;
                    StartIndex += LineCount;
                    break;
                default:
                    return;
            }
            Update();
            e.Handled = true;
        }
        private void ChatPanel_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '@':
                case '!':
                case ' ':
                case (char)Keys.Enter:
                    ChatTextBox.SetFocus();
                    if (e.KeyChar == '!') ChatTextBox.Text = "!";
                    if (e.KeyChar == '@') ChatTextBox.Text = "@";
                    if (ChatPrefix != "") ChatTextBox.Text = ChatPrefix;

                    ChatTextBox.Visible = true;
                    ChatTextBox.TextBox.SelectionLength = 0;
                    ChatTextBox.TextBox.SelectionStart = ChatTextBox.Text.Length;
                    e.Handled = true;
                    break;
                case '/':
                    ChatTextBox.SetFocus();
                    ChatTextBox.Text = LastPM + " ";
                    ChatTextBox.Visible = true;
                    ChatTextBox.TextBox.SelectionLength = 0;
                    ChatTextBox.TextBox.SelectionStart = ChatTextBox.Text.Length;
                    e.Handled = true;
                    break;
            }
        }
        private void ChatPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            int count = e.Delta / SystemInformation.MouseWheelScrollDelta;

            if (StartIndex == 0 && count >= 0) return;
            if (StartIndex == History.Count - 1 && count <= 0) return;

            StartIndex -= count;
            Update();
        }
        private void ChatTextBox_KeyUp(object sender, KeyEventArgs e)
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
                    CMain.CMain_KeyUp(sender, e);
                    break;

                case Keys.Tab:
                    e.Handled = true;
                    if (GameScene.User.IsGameMaster)
                    {
                        if (InputText == null || !ChatTextBox.Text.StartsWith(InputText.ToUpper()))
                        {
                            InputText = ChatTextBox.Text;
                            CompleteText[CompleteText.Length - 1] = InputText;
                        }

                        for (int i = 0; i < CompleteText.Length; ++i)
                        {
                            index = (index + 1) % CompleteText.Length;
                            if (CompleteText[index].ToUpper().StartsWith(InputText.ToUpper()))
                            {
                                ChatTextBox.Text = CompleteText[index];
                                ChatTextBox.TextBox.SelectionLength = 0;
                                ChatTextBox.TextBox.SelectionStart = ChatTextBox.Text.Length;
                                break;
                            }
                        }
                    }
                    break;

            }
        }
        private void ChatTextBox_KeyDown(object sender, KeyEventArgs e)
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
                    CMain.CMain_KeyDown(sender, e);
                    break;

            }
        }


        public void ChangeSize()
        {
            if (++WindowSize >= 3) WindowSize = 0;

            int y = DisplayRectangle.Bottom;
            switch (WindowSize)
            {
                case 0:
                    LineCount = 4;
                    Index = Settings.Resolution != 800 ? 2221 : 2201;
                    CountBar.Index = 2012;
                    DownButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 39);
                    EndButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 45);
                    ChatTextBox.Location = new Point(1, 54);
                    break;
                case 1:
                    LineCount = 7;
                    Index = Settings.Resolution != 800 ? 2224 : 2204;
                    CountBar.Index = 2013;
                    DownButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 39 + 48);
                    EndButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 45 + 48);
                    ChatTextBox.Location = new Point(1, 54 + 48);
                    break;
                case 2:
                    LineCount = 11;
                    Index = Settings.Resolution != 800 ? 2227 : 2207;
                    CountBar.Index = 2014;
                    DownButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 39 + 96);
                    EndButton.Location = new Point(Settings.Resolution != 800 ? 618 : 394, 45 + 96);
                    ChatTextBox.Location = new Point(1, 54 + 96);
                    break;
            }

            Location = new Point(Location.X, y - Size.Height);

            UpdateBackground();

            Update();
        }

        public void UpdateBackground()
        {
            int offset = Transparent ? 1 : 0;

            switch (WindowSize)
            {
                case 0:
                    Index = Settings.Resolution != 800 ? 2221 : 2201;
                    break;
                case 1:
                    Index = Settings.Resolution != 800 ? 2224 : 2204;
                    break;
                case 2:
                    Index = Settings.Resolution != 800 ? 2227 : 2207;
                    break;
            }

            Index -= offset;
        }

        private void HandleGMCmd(string msg)
        {
            if (msg.ToUpper() == "@LEVELEFFECT")
            {
                Settings.LevelEffect = !Settings.LevelEffect;
            }

            if (msg.ToUpper() == "@TARGETDEAD")
            {
                Settings.TargetDead = !Settings.TargetDead;
            }

#if DEBUG
            if (msg.ToUpper().StartsWith("@ARMOUR"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                    int.TryParse(param[1], out MapControl.User.Armour);
            }

            if (msg.ToUpper().StartsWith("@WEAPON"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                    int.TryParse(param[1], out MapControl.User.Weapon);
            }

            if (msg.ToUpper().StartsWith("@WEAPONEFFECT"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                    int.TryParse(param[1], out MapControl.User.WeaponEffect);
            }

            if (msg.ToUpper().StartsWith("@WEAPONEFFECT"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                    int.TryParse(param[1], out MapControl.User.WeaponEffect);
            }

            if (msg.ToUpper().StartsWith("@MAPDARKLIGHT"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                    byte.TryParse(param[1], out GameScene.Scene.MapControl.MapDarkLight);
            }

            if (msg.ToUpper().StartsWith("@ACTION"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                {
                    byte value = 0;
                    byte.TryParse(param[1], out value);
                    MonsterObject.DebugAction = (MirAction)value;
                }
            }

            if (msg.ToUpper().StartsWith("@LIGHT"))
            {
                string[] param = msg.Split(' ');
                if (param.Length > 1)
                {
                    byte value = 0;
                    byte.TryParse(param[1], out value);
                    GameScene.Scene.MapControl.Lights = (LightSetting)value;
                }
            }

            string[] args = msg.Split(' ');
            if (args.Length > 1)
                InIAttribute.SetIni<Settings>(args[0], args[1]);
#endif
        }

        public class ChatHistory
        {
            public string Text;
            public Color ForeColour, BackColour;
            public ChatType Type;
        }
    }
}
