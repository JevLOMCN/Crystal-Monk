using Client.MirControls;
using Client.MirGraphics;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MirScenes.Dialogs
{
    public sealed class BeltDialog : MirImageControl
    {
        public MirLabel[] Key = new MirLabel[6];
        public MirButton CloseButton, RotateButton;
        public MirItemCell[] Grid;

        public BeltDialog()
        {
            Index = 1932;
            Library = Libraries.Prguse;
            Movable = true;
            Sort = true;
            Visible = true;
            Location = new Point(GameScene.Scene.MainDialog.Location.X + 230, Settings.ScreenHeight - 150);

            BeforeDraw += BeltPanel_BeforeDraw;

            for (int i = 0; i < Key.Length; i++)
            {
                Key[i] = new MirLabel
                {
                    Parent = this,
                    Size = new Size(26, 14),
                    Location = new Point(8 + i * 35, 2),
                    Text = (i + 1).ToString()
                };
            }

            RotateButton = new MirButton
            {
                HoverIndex = 1927,
                Index = 1926,
                Location = new Point(222, 3),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 1928,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Rotate")
            };
            RotateButton.Click += (o, e) => Flip();

            CloseButton = new MirButton
            {
                HoverIndex = 1924,
                Index = 1923,
                Location = new Point(222, 19),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 1925,
                Sound = SoundList.ButtonA,
                Hint = CMain.Tr("Close (") + CMain.InputKeys.GetKey(KeybindOptions.Belt) + ")"
            };
            CloseButton.Click += (o, e) => Hide();

            Grid = new MirItemCell[6];

            for (int x = 0; x < 6; x++)
            {
                Grid[x] = new MirItemCell
                {
                    ItemSlot = x,
                    Size = new Size(32, 32),
                    GridType = MirGridType.Inventory,
                    Library = Libraries.Items,
                    Parent = this,
                    Location = new Point(x * 35 + 12, 3),
                };
            }

        }

        private void BeltPanel_BeforeDraw(object sender, EventArgs e)
        {
            //if Transparent return

            if (Libraries.Prguse != null)
                Libraries.Prguse.Draw(Index + 1, DisplayLocation, Color.White, false, 0.5F);
        }

        public void Hide()
        {
            Visible = false;
        }

        public void Show()
        {
            Visible = true;
        }

        public void Flip()
        {
            //0,70 LOCATION
            if (Index == 1932)
            {
                Index = 1944;
                Location = new Point(0, 200);

                for (int x = 0; x < 6; x++)
                    Grid[x].Location = new Point(3, x * 35 + 12);

                CloseButton.Index = 1935;
                CloseButton.HoverIndex = 1936;
                CloseButton.Location = new Point(3, 222);
                CloseButton.PressedIndex = 1937;

                RotateButton.Index = 1938;
                RotateButton.HoverIndex = 1939;
                RotateButton.Location = new Point(19, 222);
                RotateButton.PressedIndex = 1940;

            }
            else
            {
                Index = 1932;
                Location = new Point(GameScene.Scene.MainDialog.Location.X + 230, Settings.ScreenHeight - 150);

                for (int x = 0; x < 6; x++)
                    Grid[x].Location = new Point(x * 35 + 12, 3);

                CloseButton.Index = 1923;
                CloseButton.HoverIndex = 1924;
                CloseButton.Location = new Point(222, 19);
                CloseButton.PressedIndex = 1925;

                RotateButton.Index = 1926;
                RotateButton.HoverIndex = 1927;
                RotateButton.Location = new Point(222, 3);
                RotateButton.PressedIndex = 1928;
            }

            for (int i = 0; i < Key.Length; i++)
            {
                Key[i].Location = (Index != 1932) ? new Point(-1, 11 + i * 35) : new Point(8 + i * 35, 2);
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
    }
}
