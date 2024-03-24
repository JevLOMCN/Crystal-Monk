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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C = ClientPackets;
using Client.MirObjects;

namespace Client.MirScenes.Dialogs
{
    public sealed class NPCGoodsDialog : MirImageControl
    {
        public int StartIndex;
        public UserItem SelectedItem;

        public List<UserItem> Goods = new List<UserItem>();
        public MirGoodsCell[] Cells;
        public MirButton BuyButton, CloseButton;
        public MirImageControl BuyLabel;

        public MirButton UpButton, DownButton, PositionBar;

        public bool usePearls = false;//pearl currency

        public PanelType PType;

        public NPCGoodsDialog()
        {
            Index = 1000;
            Library = Libraries.Prguse;
            Location = new Point(0, 224);
            Cells = new MirGoodsCell[8];
            Sort = true;

            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new MirGoodsCell
                {
                    Parent = this,
                    Location = new Point(10, 34 + i * 33),
                    Sound = SoundList.ButtonC,
                };
                Cells[i].Click += (o, e) =>
                {
                    SelectedItem = ((MirGoodsCell)o).Item;
                    Update();

                    if (PType == PanelType.Craft)
                    {
                        GameScene.Scene.CraftDialog.ResetCells();
                        GameScene.Scene.CraftDialog.RefreshCraftCells(SelectedItem);

                        if (GameScene.Scene.CraftDialog.Visible) ;
                        else
                            GameScene.Scene.CraftDialog.Show();
                    }
                };
                Cells[i].MouseWheel += NPCGoodsPanel_MouseWheel;
                Cells[i].DoubleClick += (o, e) => BuyItem();
            }

            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(216, 3),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };
            CloseButton.Click += (o, e) => Hide();

            BuyButton = new MirButton
            {
                HoverIndex = 313,
                Index = 312,
                Location = new Point(77, 304),
                Library = Libraries.Title,
                Parent = this,
                PressedIndex = 314,
                Sound = SoundList.ButtonA,
            };
            BuyButton.Click += (o, e) => BuyItem();

            BuyLabel = new MirImageControl
            {
                Index = 27,
                Library = Libraries.Title,
                Parent = this,
                Location = new Point(20, 9),
            };


            UpButton = new MirButton
            {
                Index = 197,
                HoverIndex = 198,
                Library = Libraries.Prguse2,
                Location = new Point(218, 35),
                Parent = this,
                PressedIndex = 199,
                Sound = SoundList.ButtonA
            };
            UpButton.Click += (o, e) =>
            {
                if (StartIndex == 0) return;
                StartIndex--;
                Update();
            };

            DownButton = new MirButton
            {
                Index = 207,
                HoverIndex = 208,
                Library = Libraries.Prguse2,
                Location = new Point(218, 284),
                Parent = this,
                PressedIndex = 209,
                Sound = SoundList.ButtonA
            };
            DownButton.Click += (o, e) =>
            {
                if (Goods.Count <= 8) return;

                if (StartIndex == Goods.Count - 8) return;
                StartIndex++;
                Update();
            };

            PositionBar = new MirButton
            {
                Index = 205,
                HoverIndex = 206,
                Library = Libraries.Prguse2,
                Location = new Point(218, 49),
                Parent = this,
                PressedIndex = 206,
                Movable = true,
                Sound = SoundList.None
            };
            PositionBar.OnMoving += PositionBar_OnMoving;
            PositionBar.MouseUp += (o, e) => Update();
        }

        private void BuyItem()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.Info.StackSize > 1)
            {
                uint tempCount = SelectedItem.Count;
                uint maxQuantity = SelectedItem.Info.StackSize;

                SelectedItem.Count = maxQuantity;

                if (usePearls)//pearl currency
                {
                    if (SelectedItem.Price() > GameScene.User.PearlCount)
                    {
                        maxQuantity = GameScene.Gold / (SelectedItem.Price() / SelectedItem.Count);
                        if (maxQuantity == 0)
                        {
                            GameScene.Scene.ChatDialog.ReceiveChat("You do not have enough Pearls.", ChatType.System);
                            return;
                        }
                    }
                }

                else if (SelectedItem.Price() > GameScene.Gold)
                {
                    maxQuantity = GameScene.Gold / (SelectedItem.Price() / SelectedItem.Count);
                    if (maxQuantity == 0)
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat("You do not have enough Gold.", ChatType.System);
                        return;
                    }
                }

                MapObject.User.GetMaxGain(SelectedItem);

                if (SelectedItem.Count > tempCount)
                {
                    SelectedItem.Count = tempCount;
                }

                if (SelectedItem.Count == 0) return;

                MirAmountBox amountBox = new MirAmountBox(CMain.Tr("Purchase Amount:"), SelectedItem.Image, maxQuantity, 0, SelectedItem.Count);

                amountBox.OKButton.Click += (o, e) =>
                {
                    if (amountBox.Amount > 0)
                    {
                        Network.Enqueue(new C.BuyItem { ItemIndex = SelectedItem.UniqueID, Count = amountBox.Amount, Type = PanelType.Buy });
                    }
                };

                amountBox.Show();
            }
            else
            {
                if (SelectedItem.Info.Price > GameScene.Gold)
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough gold.", ChatType.System);
                    return;
                }

                if (SelectedItem.Weight > (MapObject.User.MaxBagWeight - MapObject.User.CurrentBagWeight))
                {
                    GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough weight.", ChatType.System);
                    return;
                }

                for (int i = 0; i < MapObject.User.Inventory.Length; i++)
                {
                    if (MapObject.User.Inventory[i] == null) break;
                    if (i == MapObject.User.Inventory.Length - 1)
                    {
                        GameScene.Scene.ChatDialog.ReceiveChatTr("You cannot purchase any more items.", ChatType.System);
                        return;
                    }
                }


                Network.Enqueue(new C.BuyItem { ItemIndex = SelectedItem.UniqueID, Count = 1, Type = PanelType.Buy });
            }
        }

        private void NPCGoodsPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            int count = e.Delta / SystemInformation.MouseWheelScrollDelta;

            if (StartIndex == 0 && count >= 0) return;
            if (StartIndex == Goods.Count - 1 && count <= 0) return;

            StartIndex -= count;
            Update();
        }
        private void Update()
        {
            if (StartIndex > Goods.Count - 8) StartIndex = Goods.Count - 8;
            if (StartIndex <= 0) StartIndex = 0;

            if (Goods.Count > 8)
            {
                PositionBar.Visible = true;
                int h = 233 - PositionBar.Size.Height;
                h = (int)((h / (float)(Goods.Count - 8)) * StartIndex);
                PositionBar.Location = new Point(218, 49 + h);
            }
            else
                PositionBar.Visible = false;


            for (int i = 0; i < 8; i++)
            {
                if (i + StartIndex >= Goods.Count)
                {
                    Cells[i].Visible = false;
                    continue;
                }
                Cells[i].Visible = true;

                Cells[i].Item = Goods[i + StartIndex];
                Cells[i].Border = SelectedItem != null && Cells[i].Item == SelectedItem;
                Cells[i].usePearls = usePearls;//pearl currency
            }
        }

        private void PositionBar_OnMoving(object sender, MouseEventArgs e)
        {
            const int x = 218;
            int y = PositionBar.Location.Y;
            if (y >= 282 - PositionBar.Size.Height) y = 282 - PositionBar.Size.Height;
            if (y < 49) y = 49;

            int h = 233 - PositionBar.Size.Height;
            h = (int)Math.Round(((y - 49) / (h / (float)(Goods.Count - 8))));

            PositionBar.Location = new Point(x, y);

            if (h == StartIndex) return;
            StartIndex = h;
            Update();
        }

        public void UpdatePanelType(PanelType type)
        {
            PType = type;

            if (PType == PanelType.Buy)
            {
                BuyButton.Index = 312;
                BuyButton.HoverIndex = 313;
                BuyButton.PressedIndex = 314;

                BuyLabel.Index = 27;
                BuyButton.Visible = true;
            }
            else if (PType == PanelType.Craft)
            {
                BuyLabel.Index = 12;
                BuyButton.Visible = false;

                GameScene.Scene.CraftDialog.Show();
            }
        }

        public void NewGoods(List<UserItem> list)
        {
            Goods.Clear();
            StartIndex = 0;
            SelectedItem = null;

            foreach (UserItem item in list)
            {
                //item.CurrentDura = item.Info.Durability;
                //item.MaxDura = item.Info.Durability;
                Goods.Add(item);
            }

            Update();
        }



        public void Hide()
        {
            Visible = false;
            if (GameScene.Scene.CraftDialog.Visible)
                GameScene.Scene.CraftDialog.Hide();
        }
        public void Show()
        {
            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i].Recipe = PType == PanelType.Craft;
            }

            Visible = true;

            GameScene.Scene.InventoryDialog.Show();
        }
    }

}