using System;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.MirNetwork;
using C = ClientPackets;
using Client.MirScenes;

namespace Client.MirScenes.Dialogs
{
    public sealed class RefineDialog : MirImageControl
    {
        public MirItemCell[] Grid;
        public MirButton RefineButton;

        public RefineDialog()
        {
            Index = 1002;
            Library = Libraries.Prguse;
            Location = new Point(0, 225);
            Sort = true;

            MirImageControl TitleLabel = new MirImageControl
            {
                Index = 18,
                Library = Libraries.Title,
                Location = new Point(28, 8),
                Parent = this
            };


            Grid = new MirItemCell[4 * 4];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int idx = 4 * y + x;
                    Grid[idx] = new MirItemCell
                    {
                        ItemSlot = idx,
                        GridType = MirGridType.Refine,
                        Library = Libraries.Items,
                        Parent = this,
                        Size = new Size(34, 32),
                        Location = new Point(x * 34 + 12 + x, y * 32 + 37 + y),
                    };
                }
            }
        }

        public void Hide()
        {
            Visible = false;
            RefineCancel();
        }

        public void Show()
        {
            Visible = true;
        }

        public void RefineCancel()
        {
            Network.Enqueue(new C.RefineCancel());
        }

        public void RefineReset()
        {
            for (int i = 0; i < Grid.Length; i++)
                Grid[i].Item = null;
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

