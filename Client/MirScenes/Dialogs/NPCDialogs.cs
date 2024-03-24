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
using System.Diagnostics;

namespace Client.MirScenes.Dialogs
{
    public sealed class NPCDialog : MirImageControl
    {

        public class MyMirLabel : MirLabel
        {
            public short LineIndex;
        }

        public static Regex R = new Regex(@"<(.*?/\@.*?)>");
        public static Regex C = new Regex(@"{(.*?/.*?)}");

        public MirButton CloseButton, UpButton, DownButton, PositionBar, QuestButton;
        public List<MyMirLabel> TextLabels;
        public List<MyMirLabel> TextButtons;

        public MirLabel NameLabel;

        Font font = new Font(Settings.FontName, 9F);

        public List<string> CurrentLines = new List<string>();
        private int _index = 0;
        public int MaximumLines = 8;

        public NPCDialog()
        {
            Index = 995;
            Library = Libraries.Prguse;

            TextLabels = new List<MyMirLabel>();
            TextButtons = new List<MyMirLabel>();

            MouseWheel += NPCDialog_MouseWheel;

            Sort = true;

            NameLabel = new MirLabel
            {
                Text = "",
                Parent = this,
                Font = new Font(Settings.FontName, 10F, FontStyle.Bold),
                ForeColour = Color.BurlyWood,
                Location = new Point(30, 6),
                AutoSize = true
            };

            UpButton = new MirButton
            {
                Index = 197,
                HoverIndex = 198,
                PressedIndex = 199,
                Library = Libraries.Prguse2,
                Parent = this,
                Size = new Size(16, 14),
                Location = new Point(417, 34),
                Sound = SoundList.ButtonA,
                Visible = false
            };
            UpButton.Click += (o, e) =>
            {
                if (_index <= 0) return;

                _index--;

                NewText(CurrentLines, false);
                UpdatePositionBar();
            };

            DownButton = new MirButton
            {
                Index = 207,
                HoverIndex = 208,
                Library = Libraries.Prguse2,
                PressedIndex = 209,
                Parent = this,
                Size = new Size(16, 14),
                Location = new Point(417, 175),
                Sound = SoundList.ButtonA,
                Visible = false
            };
            DownButton.Click += (o, e) =>
            {
                if (_index + MaximumLines >= CurrentLines.Count) return;

                _index++;

                NewText(CurrentLines, false);
                UpdatePositionBar();
            };

            PositionBar = new MirButton
            {
                Index = 205,
                HoverIndex = 206,
                PressedIndex = 206,
                Library = Libraries.Prguse2,
                Location = new Point(417, 47),
                Parent = this,
                Movable = true,
                Sound = SoundList.None,
                Visible = false
            };
            PositionBar.OnMoving += PositionBar_OnMoving;

            QuestButton = new MirAnimatedButton()
            {
                Animated = true,
                AnimationCount = 10,
                Loop = true,
                AnimationDelay = 130,

                Index = 530,
                HoverIndex = 284,
                PressedIndex = 286,
                Library = Libraries.Title,
                Parent = this,
                Size = new Size(96, 25),
                Location = new Point((440 - 96) / 2, 224 - 30),
                Sound = SoundList.ButtonA,
                Visible = false
            };

            QuestButton.Click += (o, e) => GameScene.Scene.QuestListDialog.Toggle();

            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(413, 3),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };
            CloseButton.Click += (o, e) => Hide();

            MirButton helpButton = new MirButton
            {
                Index = 257,
                HoverIndex = 258,
                PressedIndex = 259,
                Library = Libraries.Prguse2,
                Parent = this,
                Location = new Point(390, 3),
                Sound = SoundList.ButtonA,
            };
            helpButton.Click += (o, e) => GameScene.Scene.HelpDialog.DisplayPage("Purchasing");
        }

        void NPCDialog_MouseWheel(object sender, MouseEventArgs e)
        {
            int count = e.Delta / SystemInformation.MouseWheelScrollDelta;

            if (_index == 0 && count >= 0) return;
            if (_index == CurrentLines.Count - 1 && count <= 0) return;
            if (CurrentLines.Count <= MaximumLines) return;

            _index -= count;

            if (_index < 0) _index = 0;
            if (_index + MaximumLines > CurrentLines.Count - 1) _index = CurrentLines.Count - MaximumLines;

            NewText(CurrentLines, false);

            UpdatePositionBar();
        }

        void PositionBar_OnMoving(object sender, MouseEventArgs e)
        {
            int x = 417;
            int y = PositionBar.Location.Y;

            if (y >= 155) y = 155;
            if (y <= 47) y = 47;

            int location = y - 47;
            int interval = 108 / (CurrentLines.Count - MaximumLines);

            double yPoint = location / interval;

            _index = Convert.ToInt16(Math.Floor(yPoint));

            NewText(CurrentLines, false);

            PositionBar.Location = new Point(x, y);
        }

        private void UpdatePositionBar()
        {
            if (CurrentLines.Count <= MaximumLines) return;

            int interval = 108 / (CurrentLines.Count - MaximumLines);

            int x = 417;
            int y = 48 + (_index * interval);

            if (y >= 155) y = 155;
            if (y <= 47) y = 47;

            PositionBar.Location = new Point(x, y);
        }


        public void NewText(List<string> lines, bool resetIndex = true)
        {
            if (resetIndex)
            {
                _index = 0;
                CurrentLines = lines;
                UpdatePositionBar();

                if (lines.Count > MaximumLines)
                {
                    Index = 385;
                    UpButton.Visible = true;
                    DownButton.Visible = true;
                    PositionBar.Visible = true;
                }
                else
                {
                    Index = 384;
                    UpButton.Visible = false;
                    DownButton.Visible = false;
                    PositionBar.Visible = false;
                }

                NewContent();
            }

            for (int i = 0; i < TextLabels.Count; ++i)
            {
                if (TextLabels[i].LineIndex >= _index && TextLabels[i].LineIndex < _index + MaximumLines)
                {
                    TextLabels[i].Visible = true;
                    TextLabels[i].Location = new Point(TextLabels[i].Location.X, 34 + (TextLabels[i].LineIndex - _index) * 20);
                }
                else
                {
                    TextLabels[i].Visible = false;
                }
            }

            for (int i = 0; i < TextButtons.Count; ++i)
            {
                if (TextButtons[i].LineIndex >= _index && TextButtons[i].LineIndex < _index + MaximumLines)
                {
                    TextButtons[i].Visible = true;
                    TextButtons[i].Location = new Point(TextButtons[i].Location.X, 34 + (TextButtons[i].LineIndex - _index) * 20);
                }
                else
                {
                    TextButtons[i].Visible = false;
                }
            }
        }


        private void NewButton(string text, string key, Point p, short LineIndex)
        {
            key = string.Format("[{0}]", key);

            MyMirLabel temp = new MyMirLabel
            {
                AutoSize = true,
                Visible = true,
                Parent = this,
                Location = p,
                Text = text,
                ForeColour = Color.Yellow,
                Sound = SoundList.ButtonC,
                Font = font,
                LineIndex = LineIndex
            };
            //Fontstyle.Underline;

            temp.MouseEnter += (o, e) => temp.ForeColour = Color.Red;
            temp.MouseLeave += (o, e) => temp.ForeColour = Color.Yellow;
            temp.MouseDown += (o, e) => temp.ForeColour = Color.Yellow;
            temp.MouseUp += (o, e) => temp.ForeColour = Color.Red;

            temp.Click += (o, e) =>
            {
                if (key == "[@Exit]")
                {
                    Hide();
                    return;
                }

                if (CMain.Time <= GameScene.NPCTime) return;

                GameScene.NPCTime = CMain.Time + 5000;
                Network.Enqueue(new C.CallNPC { ObjectID = GameScene.NPCID, Key = key });
            };
            temp.MouseWheel += NPCDialog_MouseWheel;

            TextButtons.Add(temp);
        }

        private void NewColour(string text, string colour, Point p, short LineIndex)
        {
            Color textColour = Color.FromName(colour);

            MyMirLabel temp = new MyMirLabel
            {
                AutoSize = true,
                Visible = true,
                Parent = this,
                Location = p,
                Text = text,
                ForeColour = textColour,
                Font = font,
                LineIndex = LineIndex
            };
            temp.MouseWheel += NPCDialog_MouseWheel;

            TextButtons.Add(temp);
        }

        public void CheckQuestButtonDisplay()
        {
            NameLabel.Text = string.Empty;

            QuestButton.Visible = false;

            NPCObject npc = (NPCObject)MapControl.GetObject(GameScene.NPCID);
            if (npc != null)
            {
                string[] nameSplit = npc.Name.Split('_');
                NameLabel.Text = nameSplit[0];

                if (npc.GetAvailableQuests().Any())
                    QuestButton.Visible = true;
            }
        }

        public void Hide()
        {
            Visible = false;
            GameScene.Scene.NPCGoodsDialog.Hide();
            GameScene.Scene.NPCDropDialog.Hide();
            GameScene.Scene.NPCAwakeDialog.Hide();
            GameScene.Scene.RefineDialog.Hide();
            GameScene.Scene.StorageDialog.Hide();
            GameScene.Scene.TrustMerchantDialog.Hide();
            GameScene.Scene.InventoryDialog.Location = new Point(0, 0);
        }

        public void Show()
        {
            GameScene.Scene.InventoryDialog.Location = new Point(Size.Width + 5, 0);
            Visible = true;

            CheckQuestButtonDisplay();
        }

        private void NewContent()
        {
            for (int i = 0; i < TextLabels.Count; ++i)
                TextLabels[i].Dispose();

            for (int i = 0; i < TextButtons.Count; ++i)
                TextButtons[i].Dispose();

            TextLabels.Clear();
            TextButtons.Clear();
            for (int i = 0; i < CurrentLines.Count; ++i)
            {
                MyMirLabel TextLabel = new MyMirLabel
                {
                    Font = font,
                    DrawFormat = TextFormatFlags.WordBreak,
                    Visible = true,
                    Parent = this,
                    Size = new Size(420, 20),
                    Location = new Point(20, 34 + (i - _index) * 20),
                    NotControl = true,
                    LineIndex = (short)i
                };

                TextLabels.Add(TextLabel);

                string currentLine = CurrentLines[i];

                List<Match> matchList = R.Matches(currentLine).Cast<Match>().ToList();
                matchList.AddRange(C.Matches(currentLine).Cast<Match>());

                int oldLength = currentLine.Length;

                foreach (Match match in matchList.OrderBy(o => o.Index).ToList())
                {
                    int offSet = oldLength - currentLine.Length;

                    Capture capture = match.Groups[1].Captures[0];
                    string[] values = capture.Value.Split('/');
                    currentLine = currentLine.Remove(capture.Index - 1 - offSet, capture.Length + 2).Insert(capture.Index - 1 - offSet, values[0]);
                    string text = currentLine.Substring(0, capture.Index - 1 - offSet) + " ";
                    Size size = TextRenderer.MeasureText(CMain.Graphics, text, TextLabel.Font, TextLabel.Size, TextFormatFlags.TextBoxControl);

                    if (R.Match(match.Value).Success)
                        NewButton(values[0], values[1], TextLabel.Location.Add(new Point(size.Width - 10, 0)), (short)i);

                    if (C.Match(match.Value).Success)
                        NewColour(values[0], values[1], TextLabel.Location.Add(new Point(size.Width - 10, 0)), (short)i);
                }

                TextLabel.Text = currentLine;
                TextLabel.MouseWheel += NPCDialog_MouseWheel;

            }
        }
    }
}
