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
    public sealed class SkillBarDialog : MirImageControl
    {
        private readonly MirButton _switchBindsButton;

        public bool AltBind;
        public bool HasSkill = false;
        public byte BarIndex;
        public byte BindIndex;

        public MirImageControl[] Cells = new MirImageControl[8];
        public MirLabel[] KeyNameLabels = new MirLabel[8];
        public MirLabel BindNumberLabel = new MirLabel();
        public long[] LastAutoTime = new long[8];
        public bool[] IsAutoSpell = new bool[8];

        public MirAnimatedControl[] CoolDowns = new MirAnimatedControl[8];

        public SkillBarDialog()
        {
            Index = 2190;
            Library = Libraries.Prguse;
            Movable = true;
            Sort = true;
            Location = new Point(0, BarIndex * 20);
            Visible = true;

            BeforeDraw += MagicKeyDialog_BeforeDraw;

            _switchBindsButton = new MirButton
            {
                Index = 2247,
                Library = Libraries.Prguse,
                Parent = this,
                Sound = SoundList.ButtonA,
                Size = new Size(16, 28),
                Location = new Point(0, 0)
            };
            _switchBindsButton.Click += (o, e) =>
            {
                if (!Settings.SkillMode)
                    return;

                if (BindIndex == 0)
                    BindIndex = 1;
                else
                    BindIndex = 0;
                Update();
            };

            for (var i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new MirImageControl
                {
                    Index = -1,
                    Library = Libraries.MagIcon,
                    Parent = this,
                    Location = new Point(i * 25 + 15, 3),
                    BorderColour = Color.Lime,
                };
                int j = i + 1;
                Cells[i].Click += (o, e) =>
                    {
                        MouseEventArgs eventArgs = (MouseEventArgs)e;
                        if (eventArgs.Button == MouseButtons.Right)
                        {
                            IsAutoSpell[j-1] = !IsAutoSpell[j-1];
                            Cells[j-1].Border = IsAutoSpell[j-1];
                        }
                        else
                            UseSpell(j);
                    };

                CoolDowns[i] = new MirAnimatedControl
                {
                    Library = Libraries.Prguse2,
                    Parent = this,
                    Location = new Point(i * 25 + 15, 3),
                    NotControl = true,
                    UseOffSet = true,
                    Loop = false,
                    Animated = false,
                    Opacity = 0.6F
                };
            }

            BindNumberLabel = new MirLabel
            {
                Text = "1",
                Font = new Font(Settings.FontName, 8F),
                ForeColour = Color.White,
                Parent = this,
                Location = new Point(0, 1),
                Size = new Size(10, 25),
                NotControl = true
            };

            for (var i = 0; i < KeyNameLabels.Length; i++)
            {
                KeyNameLabels[i] = new MirLabel
                {
                    Text = "F" + (i + 1),
                    Font = new Font(Settings.FontName, 8F),
                    ForeColour = Color.White,
                    Parent = this,
                    Location = new Point(i * 25 + 13, 0),
                    Size = new Size(25, 25),
                    NotControl = true
                };
            }
            OnMoving += SkillBar_OnMoving;
        }

        private void SkillBar_OnMoving(object sender, MouseEventArgs e)
        {
            if (BarIndex * 2 >= Settings.SkillbarLocation.Length) return;
            Settings.SkillbarLocation[BarIndex, 0] = this.Location.X;
            Settings.SkillbarLocation[BarIndex, 1] = this.Location.Y;
        }

        private string GetKey(int barindex, int i)
        {
            //KeybindOptions Type = KeybindOptions.Bar1Skill1;
            if ((barindex == 0) && (i == 1))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill1);
            if ((barindex == 0) && (i == 2))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill2);
            if ((barindex == 0) && (i == 3))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill3);
            if ((barindex == 0) && (i == 4))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill4);
            if ((barindex == 0) && (i == 5))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill5);
            if ((barindex == 0) && (i == 6))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill6);
            if ((barindex == 0) && (i == 7))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill7);
            if ((barindex == 0) && (i == 8))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar1Skill8);
            if ((barindex == 1) && (i == 1))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill1);
            if ((barindex == 1) && (i == 2))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill2);
            if ((barindex == 1) && (i == 3))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill3);
            if ((barindex == 1) && (i == 4))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill4);
            if ((barindex == 1) && (i == 5))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill5);
            if ((barindex == 1) && (i == 6))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill6);
            if ((barindex == 1) && (i == 7))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill7);
            if ((barindex == 1) && (i == 8))
                return CMain.InputKeys.GetKey(KeybindOptions.Bar2Skill8);
            return "";
        }
                    
        void MagicKeyDialog_BeforeDraw(object sender, EventArgs e)
        {
            Libraries.Prguse.Draw(2193, new Point(DisplayLocation.X + 12, DisplayLocation.Y), Color.White, true, 0.5F);
        }

        public void Update()
        {
            HasSkill = false;
            foreach (var m in GameScene.User.Magics)
            {
                if ((m.Key < (BindIndex * 8)+1) || (m.Key > ((BindIndex + 1) * 8)+1)) continue;
                HasSkill = true;
            }
            if (!Visible) return;
            Index = 2190;
            _switchBindsButton.Index = 2247;
            BindNumberLabel.Text = (BindIndex + 1).ToString();
            BindNumberLabel.Location = new Point(0, 1);

            for (var i = 1; i <= 8; i++)
            {
                Cells[i - 1].Index = -1;

                int offset = BindIndex * 8;
                string key = GetKey(BindIndex, i);
                KeyNameLabels[i - 1].Text = key;

                foreach (var m in GameScene.User.Magics)
                {
                    if (m.Key != i + offset) continue;
                    HasSkill = true;
                    ClientMagic magic = MapObject.User.GetMagic(m.Spell);
                    if (magic == null) continue;

                    Cells[i - 1].Index = magic.Icon*2;
                    Cells[i - 1].Hint = string.Format(CMain.Tr("{0}\nMP: {1}\nCooldown: {2}\nKey: {3}"), magic.Name,
                        (magic.BaseCost + (magic.LevelCost * magic.Level)), Functions.PrintTimeSpanFromMilliSeconds(magic.Delay), key);

                    KeyNameLabels[i - 1].Text = "";
                }

                CoolDowns[i - 1].Dispose();
            }
        }

        public void Process()
        {
            ProcessSkillDelay();
            ProcessAutoSpell();
        }

        private void ProcessSkillDelay()
        {
            if (!Visible) return;

            int offset = BindIndex * 8;

            for (int i = 0; i < Cells.Length; i++)
            {
                foreach (var magic in GameScene.User.Magics)
                {
                    if (magic.Key != i + offset + 1) continue;

                    int totalFrames = 22;
                    long timeLeft = GameScene.User.GetMagicTimeLeft(magic);

                    if (timeLeft < 100 || (CoolDowns[i] != null && CoolDowns[i].Animated))
                    {
                        if (timeLeft > 0)
                            CoolDowns[i].Dispose();
                        else
                            continue;
                    }

                    int delayPerFrame = (int)(GameScene.User.GetMagicDelay(magic) / totalFrames);
                    int startFrame = totalFrames - (int)(timeLeft / delayPerFrame);

                    if (GameScene.User.GetMagicTimeLeft(magic) > 0)
                    {
                        CoolDowns[i].Dispose();

                        CoolDowns[i] = new MirAnimatedControl
                        {
                            Index = 1260 + startFrame,
                            AnimationCount = (totalFrames - startFrame),
                            AnimationDelay = delayPerFrame,
                            Library = Libraries.Prguse2,
                            Parent = this,
                            Location = new Point(i * 25 + 15, 3),
                            NotControl = true,
                            UseOffSet = true,
                            Loop = false,
                            Animated = true,
                            Opacity = 0.6F
                        };
                    }
                }
            }
        }

        public void Show()
        {
            Visible = true;
            Update();
        }

        public void Hide()
        {
            if (!Visible) return;
            Visible = false;
        }

        public void UseSpell(int key)
        {
            ClientMagic magic = FindMagic(key);
            if (magic != null)
                GameScene.Scene.UseSpell(magic);
        }

        private void ProcessAutoSpell()
        {
            for (int i = 0; i < 8; i++)
            {
                if (!IsAutoSpell[i])
                    continue;

                if (CMain.Time - LastAutoTime[i] < 1000)
                    continue;

                ClientMagic magic = FindMagic(i+1);
                if (magic == null || MapControl.User.IsMagicInCD(magic.Spell))
                    continue;

                GameScene.Scene.UseSpell(magic);
                LastAutoTime[i] = CMain.Time;
            }
        }

        private ClientMagic FindMagic(int key)
        {
            int bindKey = BindIndex * 8 + key;
            for (int i = 0; i < GameScene.User.Magics.Count; i++)
            {
                if (GameScene.User.Magics[i].Key != bindKey) continue;
                return GameScene.User.Magics[i];
            }

            return null;
        }

    }
}
