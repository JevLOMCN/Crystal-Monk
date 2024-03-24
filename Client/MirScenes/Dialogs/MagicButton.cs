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

namespace Client.MirScenes.Dialogs
{
    public sealed class MagicButton : MirControl
    {
        public MirImageControl LevelImage, ExpImage;
        public MirButton SkillButton;
        public MirLabel LevelLabel, NameLabel, ExpLabel, KeyLabel;
        public ClientMagic Magic;
        public MirAnimatedControl CoolDown;

        public MagicButton()
        {
            Size = new Size(231, 33);

            SkillButton = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.MagIcon2,
                Parent = this,
                Location = new Point(36, 0),
                Sound = SoundList.ButtonA,
            };
            SkillButton.Click += (o, e) => new AssignKeyPanel(Magic);

            LevelImage = new MirImageControl
            {
                Index = 516,
                Library = Libraries.Title,
                Location = new Point(73, 7),
                Parent = this,
                NotControl = true,
            };

            ExpImage = new MirImageControl
            {
                Index = 517,
                Library = Libraries.Title,
                Location = new Point(73, 19),
                Parent = this,
                NotControl = true,
            };

            LevelLabel = new MirLabel
            {
                AutoSize = true,
                Parent = this,
                Location = new Point(88, 2),
                NotControl = true,
            };

            NameLabel = new MirLabel
            {
                AutoSize = true,
                Parent = this,
                Location = new Point(109, 2),
                NotControl = true,
            };

            ExpLabel = new MirLabel
            {
                AutoSize = true,
                Parent = this,
                Location = new Point(109, 15),
                NotControl = true,
            };

            KeyLabel = new MirLabel
            {
                AutoSize = true,
                Parent = this,
                Location = new Point(2, 2),
                NotControl = true,
            };

            CoolDown = new MirAnimatedControl
            {
                Library = Libraries.Prguse2,
                Parent = this,
                Location = new Point(36, 0),
                NotControl = true,
                UseOffSet = true,
                Loop = false,
                Animated = false,
                Opacity = 0.6F
            };
        }

        public void Update(ClientMagic magic)
        {
            Magic = magic;

            NameLabel.Text = Magic.Name;

            LevelLabel.Text = Magic.Level.ToString();
            switch (Magic.Level)
            {
                case 0:
                    ExpLabel.Text = string.Format("{0}/{1}", Magic.Experience, Magic.Need1);
                    break;
                case 1:
                    ExpLabel.Text = string.Format("{0}/{1}", Magic.Experience, Magic.Need2);
                    break;
                case 2:
                    ExpLabel.Text = string.Format("{0}/{1}", Magic.Experience, Magic.Need3);
                    break;
                case 3:
                    if (Magic.Level4 != 127)
                        ExpLabel.Text = string.Format("{0}/{1}", Magic.Experience, Magic.Need4);
                    else
                        ExpLabel.Text = "-";
                    break;
                case 4:
                    ExpLabel.Text = "-";
                    break;

            }

            if (Magic.Key > 8)
            {
                int key = Magic.Key % 8;

                KeyLabel.Text = string.Format("CTRL" + Environment.NewLine + "F{0}", key != 0 ? key : 8);
            }
            else if (Magic.Key > 0)
                KeyLabel.Text = string.Format("F{0}", Magic.Key);
            else
                KeyLabel.Text = string.Empty;


            SkillButton.Index = Magic.Icon * 2;
            SkillButton.PressedIndex = Magic.Icon * 2 + 1;

            SetDelay();
        }

        public void SetDelay()
        {
            if (Magic == null) return;

            int totalFrames = 34;

            long timeLeft = UserObject.User.GetMagicTimeLeft(Magic.Spell);

            if (timeLeft < 100 || (CoolDown != null && CoolDown.Animated)) return;

            int delayPerFrame = (int)(UserObject.User.GetMagicDelay(Magic) / totalFrames);
            int startFrame = totalFrames - (int)(timeLeft / delayPerFrame);

            if (UserObject.User.IsMagicInCD(Magic.Spell))
            {
                CoolDown.Dispose();

                CoolDown = new MirAnimatedControl
                {
                    Index = 1290 + startFrame,
                    AnimationCount = (totalFrames - startFrame),
                    AnimationDelay = delayPerFrame,
                    Library = Libraries.Prguse2,
                    Parent = this,
                    Location = new Point(36, 0),
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
