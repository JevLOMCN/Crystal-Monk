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
using Client.MirScenes;
using Client;

namespace Client.MirScenes.Dialogs
{
    public sealed class CraftDialog : MirImageControl
    {
        public static UserItem RecipeItem;
        public static UserItem[] IngredientSlots = new UserItem[16];

        public List<KeyValuePair<MirItemCell, ulong>> Selected = new List<KeyValuePair<MirItemCell, ulong>>();

        public MirItemCell[] Grid;
        public static UserItem[] ShadowItems = new UserItem[16];
        public MirLabel CraftTips;
        public MirButton CraftButton, CloseButton;

        public CraftDialog()
        {
            Index = 1002;
            Library = Libraries.Prguse;
            Location = new Point(0, 0);
            Sort = true;
            BeforeDraw += CraftDialog_BeforeDraw;
            Movable = true;

            Grid = new MirItemCell[4 * 4];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    int idx = 4 * y + x;
                    Grid[idx] = new MirItemCell
                    {
                        ItemSlot = idx,
                        GridType = MirGridType.Craft,
                        Library = Libraries.Items,
                        Parent = this,
                        Size = new Size(34, 32),
                        Location = new Point(x * 34 + 12 + x, y * 32 + 37 + y),
                        Border = true,
                        BorderColour = Color.Lime
                    };
                    Grid[idx].Click += Grid_Click;
                }
            }

            CraftTips = new MirLabel { AutoSize = true, Location = new Point(28, 8), Parent = this, Text = "制作" };

            CloseButton = new MirButton
            {
                Index = 360,
                HoverIndex = 361,
                PressedIndex = 362,
                Location = new Point(139, 3),
                Library = Libraries.Prguse2,
                Parent = this,
                Sound = SoundList.ButtonA,
            };
            CloseButton.Click += (o, e) => Hide();

            CraftButton = new MirButton
            {
                HoverIndex = 337,
                Index = 336,
                Location = new Point(41, 177),
                Library = Libraries.Title,
                Parent = this,
                PressedIndex = 338,
                Sound = SoundList.ButtonA,
                GrayScale = true,
                Enabled = false
            };
            CraftButton.Click += (o, e) => CraftItem();
        }

        void CraftDialog_BeforeDraw(object sender, EventArgs e)
        {
            if (!GameScene.Scene.InventoryDialog.Visible)
            {
                Hide();
                return;
            }
        }


        private void Grid_Click(object sender, EventArgs e)
        {
            MirItemCell cell = (MirItemCell)sender;

            if (cell == null || cell.ShadowItem == null)
                return;

            if (GameScene.SelectedCell == null || GameScene.SelectedCell.GridType != MirGridType.Inventory || GameScene.SelectedCell.Locked)
                return;

            if (GameScene.SelectedCell.Item.Info != cell.ShadowItem.Info || cell.Item != null)
                return;


            cell.Item = GameScene.SelectedCell.Item;

            Selected.Add(new KeyValuePair<MirItemCell, ulong>(GameScene.SelectedCell, GameScene.SelectedCell.Item.UniqueID));

            GameScene.SelectedCell.Locked = true;
            GameScene.SelectedCell = null;

            RefreshCraftCells(RecipeItem);
        }

        public void Hide()
        {
            if (!Visible) return;

            Visible = false;

            ResetCells();
        }

        public void Show()
        {
            Visible = true;

            Location = new Point(GameScene.Scene.InventoryDialog.Location.X, GameScene.Scene.InventoryDialog.Location.Y + 236);
        }

        private void CraftItem()
        {
            if (RecipeItem == null) return;

            uint max = 99;
            for (int i = 0; i < Grid.Length; i++)
            {
                if (Grid[i] == null || Grid[i].Item == null) continue;

                uint temp = Grid[i].Item.Count / Grid[i].ShadowItem.Count;

                if (temp < max) max = temp;
            }

            if (max > RecipeItem.Info.StackSize)
                max = RecipeItem.Info.StackSize;

            //TODO - Check Max slots spare against slots to be used (stacksize/quantity)
            //TODO - GetMaxItemGain

            if (RecipeItem.Weight > (MapObject.User.MaxBagWeight - MapObject.User.CurrentBagWeight))
            {
                GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have enough weight.", ChatType.System);
                return;
            }

            if (max > 1)
            {
                MirAmountBox amountBox = new MirAmountBox(CMain.Tr("Craft Amount:"), RecipeItem.Info.Image, max, 0, max);

                amountBox.OKButton.Click += (o, e) =>
                {
                    if (amountBox.Amount > 0)
                    {
                        if (!HasCraftItems(RecipeItem, amountBox.Amount))
                        {
                            GameScene.Scene.ChatDialog.ReceiveChatTr("You do not have the required ingredients.", ChatType.System);
                            return;
                        }

                        Network.Enqueue(new C.CraftItem { UniqueID = RecipeItem.UniqueID, Count = amountBox.Amount, Slots = Selected.Select(x => x.Key.ItemSlot).ToArray() });
                    }
                };

                amountBox.Show();
            }
            else
            {
                Network.Enqueue(new C.CraftItem { UniqueID = RecipeItem.UniqueID, Count = 1, Slots = Selected.Select(x => x.Key.ItemSlot).ToArray() });
            }
        }

        private bool HasCraftItems(UserItem item, uint count)
        {
            for (int i = 0; i < Grid.Length; i++)
            {
                if (Grid[i].ShadowItem == null) continue;

                if (Grid[i].Item == null || Grid[i].Item.Count < (Grid[i].ShadowItem.Count * count)) return false;
            }

            return true;
        }

        public void ResetCells()
        {
            RecipeItem = null;
            for (int j = 0; j < Grid.Length; j++)
            {
                IngredientSlots[j] = null;
                ShadowItems[j] = null;
            }

            for (int i = 0; i < Selected.Count; i++)
            {
                Selected[i].Key.Locked = false;
            }

            Selected.Clear();
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

        public void RefreshCraftCells(UserItem selectedItem)
        {
            RecipeItem = selectedItem;

            CraftButton.Enabled = true;
            CraftButton.GrayScale = false;

            ClientRecipeInfo recipe = GameScene.RecipeInfoList.SingleOrDefault(x => x.Item.ItemIndex == selectedItem.ItemIndex);

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                if (i >= IngredientSlots.Length) break;

                ShadowItems[i] = recipe.Ingredients[i];

                bool needItem = Grid[i].Item == null || Grid[i].Item.Count < Grid[i].ShadowItem.Count;

                if (needItem)
                {
                    CraftButton.Enabled = false;
                    CraftButton.GrayScale = true;
                }
            }

            CraftTips.Text = string.Format("制作成功率:{0}%", recipe.SuccessRate);
            CraftTips.ForeColour = recipe.SuccessRate < 100 ? Color.Red : Color.Lime;
        }
    }
}