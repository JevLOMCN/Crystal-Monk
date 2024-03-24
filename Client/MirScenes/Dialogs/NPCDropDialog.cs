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

namespace Client.MirScenes.Dialogs
{
    public sealed class NPCDropDialog : MirImageControl
    {

        public readonly MirButton ConfirmButton, HoldButton;
        public readonly MirItemCell ItemCell;
        public MirItemCell OldCell;
        public readonly MirLabel InfoLabel;
        public PanelType PType;

        public static UserItem TargetItem;
        public bool Hold;


        public NPCDropDialog()
        {
            Index = 392;
            Library = Libraries.Prguse;
            Location = new Point(264, 224);
            Sort = true;

            Click += NPCDropPanel_Click;

            HoldButton = new MirButton
            {
                HoverIndex = 294,
                Index = 293,
                Location = new Point(114, 36),
                Library = Libraries.Title,
                Parent = this,
                PressedIndex = 295,
                Sound = SoundList.ButtonA,
            };
            HoldButton.Click += (o, e) => Hold = !Hold;

            ConfirmButton = new MirButton
            {
                HoverIndex = 291,
                Index = 290,
                Location = new Point(114, 62),
                Library = Libraries.Title,
                Parent = this,
                PressedIndex = 292,
                Sound = SoundList.ButtonA,
            };
            ConfirmButton.Click += (o, e) => Confirm();

            InfoLabel = new MirLabel
            {
                AutoSize = true,
                Location = new Point(30, 10),
                Parent = this,
                NotControl = true,
            };

            ItemCell = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.DropPanel,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(38, 72),
            };
            ItemCell.Click += (o, e) => ItemCell_Click();

            BeforeDraw += NPCDropPanel_BeforeDraw;
            AfterDraw += NPCDropPanel_AfterDraw;
        }

        private void NPCDropPanel_AfterDraw(object sender, EventArgs e)
        {
            if (Hold)
                Libraries.Title.Draw(295, 114 + DisplayLocation.X, 36 + DisplayLocation.Y);
        }

        private void NPCDropPanel_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;

            if (me == null) return;
            int x = me.X - DisplayLocation.X;
            int y = me.Y - DisplayLocation.Y;

            if (new Rectangle(20, 55, 75, 75).Contains(x, y))
                ItemCell_Click();
        }

        private void Confirm()
        {
            if (TargetItem == null) return;

            switch (PType)
            {
                case PanelType.Sell:
                    if (TargetItem.Info.Bind.HasFlag(BindMode.DontSell))
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat("Cannot sell this item.", ChatType.System);
                        return;
                    }
                    if (GameScene.Gold + TargetItem.Price() / 2 <= uint.MaxValue)
                    {
                        Network.Enqueue(new C.SellItem { UniqueID = TargetItem.UniqueID, Count = TargetItem.Count });
                        TargetItem = null;
                        return;
                    }
                    GameScene.Scene.ChatDialog.ReceiveChat("Cannot carry anymore gold.", ChatType.System);
                    break;
                case PanelType.Repair:
                    if (TargetItem.Info.Bind.HasFlag(BindMode.DontRepair))
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat("Cannot repair this item.", ChatType.System);
                        return;
                    }
                    if (GameScene.Gold >= TargetItem.RepairPrice() * GameScene.NPCRate)
                    {
                        Network.Enqueue(new C.RepairItem { UniqueID = TargetItem.UniqueID });
                        TargetItem = null;
                        return;
                    }
                    GameScene.Scene.ChatDialog.ReceiveChat("You do not have enough gold.", ChatType.System);
                    break;
                case PanelType.SpecialRepair:
                    if ((TargetItem.Info.Bind.HasFlag(BindMode.DontRepair)) || (TargetItem.Info.Bind.HasFlag(BindMode.NoSRepair)))
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat("Cannot repair this item.", ChatType.System);
                        return;
                    }
                    if (GameScene.Gold >= (TargetItem.RepairPrice() * 3) * GameScene.NPCRate)
                    {
                        Network.Enqueue(new C.SRepairItem { UniqueID = TargetItem.UniqueID });
                        TargetItem = null;
                        return;
                    }
                    GameScene.Scene.ChatDialog.ReceiveChat("You do not have enough gold.", ChatType.System);
                    break;
                case PanelType.Consign:
                    if (TargetItem.Info.Bind.HasFlag(BindMode.DontStore))
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat("Cannot store this item.", ChatType.System);
                        return;
                    }
                    MirAmountBox box = new MirAmountBox("Consignment Price:", TargetItem.Image, Globals.MaxConsignment, Globals.MinConsignment)
                    {
                        InputTextBox = { Text = string.Empty },
                        Amount = 0
                    };

                    box.Show();
                    box.OKButton.Click += (o, e) =>
                    {
                        Network.Enqueue(new C.ConsignItem { UniqueID = TargetItem.UniqueID, Price = box.Amount });
                        TargetItem = null;
                    };
                    return;
                case PanelType.Disassemble:
                    Network.Enqueue(new C.DisassembleItem { UniqueID = TargetItem.UniqueID });
                    break;
                case PanelType.Downgrade:
                    Network.Enqueue(new C.DowngradeAwakening { UniqueID = TargetItem.UniqueID });
                    break;
                case PanelType.Reset:
                    if (TargetItem.Info.NeedIdentify == false)
                    {
                        Network.Enqueue(new C.ResetAddedItem { UniqueID = TargetItem.UniqueID });
                    }
                    break;
                case PanelType.Refine:

                    for (int i = 0; i < GameScene.Scene.RefineDialog.Grid.Length; i++)
                    {
                        if (GameScene.Scene.RefineDialog.Grid[i].Item != null)
                        {
                            if (GameScene.Gold >= ((TargetItem.Info.RequiredAmount * 10) * GameScene.NPCRate))
                            {
                                Network.Enqueue(new C.RefineItem { UniqueID = TargetItem.UniqueID });
                                TargetItem = null;
                                return;
                            }
                            GameScene.Scene.ChatDialog.ReceiveChat(String.Format("You don't have enough gold to refine your {0}.", TargetItem.FriendlyName), ChatType.System);
                            return;
                        }

                    }
                    GameScene.Scene.ChatDialog.ReceiveChat(String.Format("You haven't deposited any items to refine your {0} with.", TargetItem.FriendlyName), ChatType.System);
                    break;
                case PanelType.CheckRefine:

                    if (TargetItem.RefineAdded == 0)
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat(String.Format("Your {0} hasn't been refined so it doesn't need checking.", TargetItem.FriendlyName), ChatType.System);
                        return;
                    }
                    Network.Enqueue(new C.CheckRefine { UniqueID = TargetItem.UniqueID });
                    break;

                case PanelType.ReplaceWedRing:

                    if (TargetItem.Info.Type != ItemType.Ring)
                    {
                        GameScene.Scene.ChatDialog.ReceiveChat(String.Format("{0} isn't a ring.", TargetItem.FriendlyName), ChatType.System);
                        return;
                    }

                    Network.Enqueue(new C.ReplaceWedRing { UniqueID = TargetItem.UniqueID });
                    break;
            }


            TargetItem = null;
            OldCell.Locked = false;
            OldCell = null;
        }

        private void ItemCell_Click()
        {
            if (OldCell != null)
            {
                OldCell.Locked = false;
                TargetItem = null;
                OldCell = null;
            }

            if (GameScene.SelectedCell != null && PType == PanelType.Disassemble)
            {
                if (GameScene.SelectedCell.Item.Info.Grade != ItemGrade.None &&
                    GameScene.SelectedCell.Item.Info.Type != ItemType.Awakening)
                {
                    TargetItem = GameScene.SelectedCell.Item;
                    OldCell = GameScene.SelectedCell;
                    OldCell.Locked = true;
                    GameScene.SelectedCell = null;
                    return;
                }
            }

            if (GameScene.SelectedCell != null && PType == PanelType.Downgrade)
            {
                if (GameScene.SelectedCell.Item.Awake.getAwakeLevel() != 0)
                {
                    TargetItem = GameScene.SelectedCell.Item;
                    OldCell = GameScene.SelectedCell;
                    OldCell.Locked = true;
                    GameScene.SelectedCell = null;
                    return;
                }
            }

            if (GameScene.SelectedCell != null && PType == PanelType.Reset)
            {
                if (GameScene.SelectedCell.Item.IsAdded)
                {
                    TargetItem = GameScene.SelectedCell.Item;
                    OldCell = GameScene.SelectedCell;
                    OldCell.Locked = true;
                    GameScene.SelectedCell = null;
                    return;
                }
            }

            if (GameScene.SelectedCell != null && (PType == PanelType.Disassemble || PType == PanelType.Downgrade || PType == PanelType.Reset))
            {
                GameScene.SelectedCell.Locked = false;
                GameScene.SelectedCell = null;
                return;
            }

            //////////////////////////////////////

            if (GameScene.SelectedCell == null || GameScene.SelectedCell.GridType != MirGridType.Inventory ||
                (PType != PanelType.Sell && PType != PanelType.Consign && GameScene.SelectedCell.Item != null && GameScene.SelectedCell.Item.Info.Durability == 0))
                return;
            /*
            if (GameScene.SelectedCell.Item != null && (GameScene.SelectedCell.Item.Info.StackSize > 1 && GameScene.SelectedCell.Item.Count > 1))
            {
                MirAmountBox amountBox = new MirAmountBox("Sell Amount:", GameScene.SelectedCell.Item.Image, GameScene.SelectedCell.Item.Count);

                amountBox.OKButton.Click += (o, a) =>
                {
                    TargetItem = GameScene.SelectedCell.Item.Clone();
                    TargetItem.Count = amountBox.Amount;

                    OldCell = GameScene.SelectedCell;
                    OldCell.Locked = true;
                    GameScene.SelectedCell = null;
                    if (Hold) Confirm();
                };

                amountBox.Show();
            }
            else
            {
                TargetItem = GameScene.SelectedCell.Item;
                OldCell = GameScene.SelectedCell;
                OldCell.Locked = true;
                GameScene.SelectedCell = null;
                if (Hold) Confirm();
            }
            */
            TargetItem = GameScene.SelectedCell.Item;
            OldCell = GameScene.SelectedCell;
            OldCell.Locked = true;
            GameScene.SelectedCell = null;
            if (Hold) Confirm();
        }

        private void NPCDropPanel_BeforeDraw(object sender, EventArgs e)
        {
            string text;

            HoldButton.Visible = true;

            Index = 351;
            Library = Libraries.Prguse2;
            Location = new Point(264, 224);

            ConfirmButton.HoverIndex = 291;
            ConfirmButton.Index = 290;
            ConfirmButton.PressedIndex = 292;
            ConfirmButton.Location = new Point(114, 62);

            InfoLabel.Location = new Point(30, 10);

            ItemCell.Location = new Point(38, 72);

            switch (PType)
            {
                case PanelType.Sell:
                    text = CMain.Tr("Sale: ");
                    break;
                case PanelType.Repair:
                    text = CMain.Tr("Repair: ");
                    break;
                case PanelType.SpecialRepair:
                    text = CMain.Tr("S. Repair: ");
                    break;
                case PanelType.Consign:
                    InfoLabel.Text = CMain.Tr("Consignment: ");
                    return;
                case PanelType.Disassemble:
                    text = CMain.Tr("Item will be Destroyed\n\n\n\n\n\n\n\n         ");
                    HoldButton.Visible = false;
                    Index = 711;
                    Library = Libraries.Title;
                    Location = new Point(234, 224);
                    ConfirmButton.HoverIndex = 716;
                    ConfirmButton.Index = 715;
                    ConfirmButton.PressedIndex = 717;
                    ConfirmButton.Location = new Point(62, 190);
                    InfoLabel.Location = new Point(44, 60);
                    ItemCell.Location = new Point(83, 94);
                    break;
                case PanelType.Downgrade:
                    text = CMain.Tr("Downgrade: ");
                    HoldButton.Visible = false;
                    break;
                case PanelType.Reset:
                    text = CMain.Tr("Reset: ");
                    HoldButton.Visible = false;
                    break;
                case PanelType.Refine:
                    text = CMain.Tr("Refine: ");
                    HoldButton.Visible = false;
                    ConfirmButton.Visible = true;
                    GameScene.Scene.RefineDialog.Show();
                    break;
                case PanelType.CheckRefine:
                    text = CMain.Tr("Check Refine");
                    HoldButton.Visible = false;
                    ConfirmButton.Visible = true;
                    break;
                case PanelType.ReplaceWedRing:
                    text = CMain.Tr("Replace: ");
                    HoldButton.Visible = false;
                    ConfirmButton.Visible = true;
                    break;

                default: return;

            }

            if (TargetItem != null)
            {

                switch (PType)
                {
                    case PanelType.Sell:
                        text += (TargetItem.Price() / 2).ToString();
                        break;
                    case PanelType.Repair:
                        text += (TargetItem.RepairPrice() * GameScene.NPCRate).ToString();
                        break;
                    case PanelType.SpecialRepair:
                        text += ((TargetItem.RepairPrice() * 3) * GameScene.NPCRate).ToString();
                        break;
                    case PanelType.Disassemble:
                        text += TargetItem.DisassemblePrice().ToString();
                        break;
                    case PanelType.Downgrade:
                        text += TargetItem.DowngradePrice().ToString();
                        break;
                    case PanelType.Reset:
                        text += TargetItem.ResetPrice().ToString();
                        break;
                    case PanelType.Refine:
                        text += ((TargetItem.Info.RequiredAmount * 10) * GameScene.NPCRate).ToString();
                        break;
                    case PanelType.ReplaceWedRing:
                        text += ((TargetItem.Info.RequiredAmount * 10) * GameScene.NPCRate).ToString();
                        break;
                    default: return;
                }

                text += " Gold";
            }

            InfoLabel.Text = text;
        }

        public void Hide()
        {
            if (OldCell != null)
            {
                OldCell.Locked = false;
                TargetItem = null;
                OldCell = null;
            }
            Visible = false;
        }
        public void Show()
        {
            Hold = false;
            GameScene.Scene.InventoryDialog.Show();
            Visible = true;
        }
    }
}

