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
    public sealed class AssignKeyPanel : MirImageControl
    {
        public MirButton SaveButton, NoneButton;

        public MirLabel TitleLabel;
        public MirImageControl MagicImage;
        public MirButton[] FKeys;

        public ClientMagic Magic;
        public byte Key;

        public AssignKeyPanel(ClientMagic magic)
        {
            Magic = magic;
            Key = magic.Key;

            Modal = true;
            Index = 710;
            Library = Libraries.Prguse;
            Location = Center;
            Parent = GameScene.Scene;
            Visible = true;

            MagicImage = new MirImageControl
            {
                Location = new Point(16, 16),
                Index = magic.Icon * 2,
                Library = Libraries.MagIcon2,
                Parent = this,
            };

            TitleLabel = new MirLabel
            {
                Location = new Point(49, 17),
                Parent = this,
                Size = new Size(230, 32),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak,
                Text = CMain.Format("Select the Key for: {0}", magic.Name)
            };

            NoneButton = new MirButton
            {
                Index = 287, //154
                HoverIndex = 288,
                PressedIndex = 289,
                Library = Libraries.Title,
                Parent = this,
                Location = new Point(284, 64),
            };
            NoneButton.Click += (o, e) => Key = 0;

            SaveButton = new MirButton
            {
                Library = Libraries.Title,
                Parent = this,
                Location = new Point(284, 101),
                Index = 156,
                HoverIndex = 157,
                PressedIndex = 158,
            };
            SaveButton.Click += (o, e) =>
            {
                for (int i = 0; i < MapObject.User.Magics.Count; i++)
                {
                    if (MapObject.User.Magics[i].Key == Key)
                        MapObject.User.Magics[i].Key = 0;
                }

                Network.Enqueue(new C.MagicKey { Spell = Magic.Spell, Key = Key });
                Magic.Key = Key;
                foreach (SkillBarDialog Bar in GameScene.Scene.SkillBarDialogs)
                    Bar.Update();

                Dispose();
            };


            FKeys = new MirButton[16];

            FKeys[0] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(17, 58),
                Sound = SoundList.ButtonA,
                Text = "F1"
            };
            FKeys[0].Click += (o, e) => Key = 1;

            FKeys[1] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(49, 58),
                Sound = SoundList.ButtonA,
                Text = "F2"
            };
            FKeys[1].Click += (o, e) => Key = 2;

            FKeys[2] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(81, 58),
                Sound = SoundList.ButtonA,
                Text = "F3"
            };
            FKeys[2].Click += (o, e) => Key = 3;

            FKeys[3] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(113, 58),
                Sound = SoundList.ButtonA,
                Text = "F4"
            };
            FKeys[3].Click += (o, e) => Key = 4;

            FKeys[4] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(150, 58),
                Sound = SoundList.ButtonA,
                Text = "F5"
            };
            FKeys[4].Click += (o, e) => Key = 5;

            FKeys[5] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(182, 58),
                Sound = SoundList.ButtonA,
                Text = "F6",
            };
            FKeys[5].Click += (o, e) => Key = 6;

            FKeys[6] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(214, 58),
                Sound = SoundList.ButtonA,
                Text = "F7"
            };
            FKeys[6].Click += (o, e) => Key = 7;

            FKeys[7] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(246, 58),
                Sound = SoundList.ButtonA,
                Text = "F8"
            };
            FKeys[7].Click += (o, e) => Key = 8;


            FKeys[8] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(17, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F1"
            };
            FKeys[8].Click += (o, e) => Key = 9;

            FKeys[9] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(49, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F2"
            };
            FKeys[9].Click += (o, e) => Key = 10;

            FKeys[10] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(81, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F3"
            };
            FKeys[10].Click += (o, e) => Key = 11;

            FKeys[11] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(113, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F4"
            };
            FKeys[11].Click += (o, e) => Key = 12;

            FKeys[12] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(150, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F5"
            };
            FKeys[12].Click += (o, e) => Key = 13;

            FKeys[13] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(182, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F6"
            };
            FKeys[13].Click += (o, e) => Key = 14;

            FKeys[14] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(214, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F7"
            };
            FKeys[14].Click += (o, e) => Key = 15;

            FKeys[15] = new MirButton
            {
                Index = 0,
                PressedIndex = 1,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(246, 95),
                Sound = SoundList.ButtonA,
                Text = "Ctrl" + Environment.NewLine + "F8"
            };
            FKeys[15].Click += (o, e) => Key = 16;

            BeforeDraw += AssignKeyPanel_BeforeDraw;
        }

        private void AssignKeyPanel_BeforeDraw(object sender, EventArgs e)
        {
            for (int i = 0; i < FKeys.Length; i++)
            {
                FKeys[i].Index = 1656;
                FKeys[i].HoverIndex = 1657;
                FKeys[i].PressedIndex = 1658;
                FKeys[i].Visible = true;
            }

            if (Key == 0 || Key > FKeys.Length) return;

            FKeys[Key - 1].Index = 1658;
            FKeys[Key - 1].HoverIndex = 1658;
            FKeys[Key - 1].PressedIndex = 1658;
        }
    }


}
