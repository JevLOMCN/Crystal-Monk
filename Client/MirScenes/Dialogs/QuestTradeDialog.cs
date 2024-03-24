using System;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
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
    public sealed class GuestTradeDialog : MirImageControl
    {
        public MirItemCell[] GuestGrid;
        public static UserItem[] GuestItems = new UserItem[10];
        public string GuestName;
        public uint GuestGold;

        public MirLabel GuestNameLabel, GuestGoldLabel;

        public MirButton ConfirmButton;

        public GuestTradeDialog()
        {
            Index = 390;
            Library = Libraries.Prguse;
            Movable = true;
            Size = new Size(204, 152);
            Location = new Point((Settings.ScreenWidth / 2) + 10, Settings.ScreenHeight - 350);
            Sort = true;

            #region Host labels
            GuestNameLabel = new MirLabel
            {
                Parent = this,
                Location = new Point(0, 10),
                Size = new Size(204, 14),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                NotControl = true,
            };

            GuestGoldLabel = new MirLabel
            {
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Font = new Font(Settings.FontName, 8F),
                Location = new Point(35, 123),
                Parent = this,
                Size = new Size(90, 15),
                Sound = SoundList.Gold,
                NotControl = true,
            };
            #endregion

            #region Grids
            GuestGrid = new MirItemCell[5 * 2];

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    GuestGrid[2 * x + y] = new MirItemCell
                    {
                        ItemSlot = 2 * x + y,
                        GridType = MirGridType.GuestTrade,
                        Parent = this,
                        Location = new Point(x * 36 + 10 + x, y * 32 + 39 + y),
                    };
                }
            }
            #endregion
        }

        public void RefreshInterface()
        {
            GuestNameLabel.Text = GuestName;
            GuestGoldLabel.Text = string.Format("{0:###,###,##0}", GuestGold);

            for (int i = 0; i < GuestItems.Length; i++)
            {
                if (GuestItems[i] == null) continue;
                GameScene.Bind(GuestItems[i]);
            }

            Redraw();
        }

        public void TradeReset()
        {
            for (int i = 0; i < GuestItems.Length; i++)
                GuestItems[i] = null;

            GuestName = string.Empty;
            GuestGold = 0;

            Hide();
        }


        public void Hide()
        {
            Visible = false;
        }

        public void Show()
        {
            Visible = true;
        }
    }
}

