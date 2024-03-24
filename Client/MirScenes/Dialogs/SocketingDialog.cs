using Client.MirControls;
using Client.MirGraphics;
using Client.MirScenes;
using System.Drawing;
using Client.MirNetwork;
using S = ServerPackets;
using C = ClientPackets;
using Client.MirSounds;
using System;

namespace Client.MirScenes.Dialogs
{
    public class EE_Mob_Active_Effects
    {
        //  0   =   Poison
        //  1   =   Buff
        public sbyte EffectType;
        public PoisonType PType;
        public BuffType BType;
    }

    public sealed class SocketingDialog : MirImageControl
    {
        public MirItemCell Item;
        public MirImageControl ItemImage;
        public MirItemCell[] ItemCells = new MirItemCell[3];
        public MirImageControl[] RuneSlots = new MirImageControl[3];

        public MirLabel CanRemove;

        public static Sockets[] Runes = new Sockets[3];
        public MirImageControl SocketsContainer;
        public MirButton CloseBtn, AplyBtn;

        public SocketingDialog()
        {
            Index = 143;
            Library = Libraries.EdensEliteInter;
            Location = new Point(Settings.ScreenWidth / 2 - this.Size.Width / 2, Settings.ScreenHeight / 2 - this.Size.Height / 2);
            Sort = true;
            Movable = true;

            CloseBtn = new MirButton
            {
                Parent = this,
                Library = Libraries.Prguse2,
                Location = new Point(236, 0),
                Index = 7,
                HoverIndex = 8,
                PressedIndex = 9,
                Sound = SoundList.ButtonA,
                Hint = "Exit"
            };
            CloseBtn.Click += (o, e) => Hide();

            AplyBtn = new MirButton
            {
                Parent = this,
                Library = Libraries.CustomTitle,
                Location = new Point(160, 20),
                Index = 712,
                HoverIndex = 713,
                PressedIndex = 714,
                Sound = SoundList.ButtonA,
                Hint = "Forge"
            };
            AplyBtn.Click += (o, e) => Hide();

            SocketsContainer = new MirImageControl
            {
                Index = 28,
                Library = Libraries.EdensEliteInter,
                Location = new Point(12, 56),
                Parent = this,
            };

            RuneSlots[0] = new MirImageControl
            {
                Index = 989,
                Library = Libraries.Prguse,
                Parent = this,
                Location = new Point(100, 18),
            };
            //  Main Item
            Item = new MirItemCell
            {
                BorderColour = Color.OrangeRed,
                GridType = MirGridType.SocketItem,
                Library = Libraries.Items,
                Parent = RuneSlots[0],
                Location = new Point(2, 2),
                ItemSlot = 0,
                Hint = CMain.Tr("Place your Item here.")
            };
            ItemImage = new MirImageControl
            {
                Index = 16,
                Library = Libraries.Prguse2,
                Parent = SocketsContainer,
                Location = new Point(32, 9),
                Visible = false
            };

            for (int i = 0; i < 3; i++)
            {
                RuneSlots[i] = new MirImageControl
                {
                    Index = 16,
                    Library = Libraries.Prguse2,
                    Parent = SocketsContainer,
                    Location = new Point(32 + i * 64, 9),
                    Visible = false
                };
                ItemCells[i] = new MirItemCell
                {
                    BorderColour = Color.Aqua,
                    GridType = MirGridType.SockeRune,
                    Library = Libraries.Items,
                    Parent = RuneSlots[i],
                    Location = new Point(2, 2),
                    ItemSlot = i,
                    Visible = false,
                    Hint = CMain.Tr("Place your Rune here.")
                };
            }
        }

        public void Hide()
        {
            if (Visible)
            {
                Visible = false;
                #region Clear everything
                Item.Item = null;
                ItemImage.Index = 0;
                
                for (int i = 0; i < RuneSlots.Length; i++)
                    RuneSlots[i].Visible = false;
                for (int i = 0; i < ItemCells.Length; i++)
                {
                    ItemCells[i].Item = null;
                    if (i > 0)
                    {
                        ItemCells[i].Visible = false;
                        ItemCells[i].Locked = false;
                    }
                }
                #endregion
                #region Unlock any locked cells
                for (int i = 0; i < GameScene.Scene.InventoryDialog.Grid.Length; i++)
                    if (GameScene.Scene.InventoryDialog.Grid[i].Locked)
                        GameScene.Scene.InventoryDialog.Grid[i].Locked = false;
                for (int i = 0; i < GameScene.Scene.CharacterDialog.Grid.Length; i++)
                    if (GameScene.Scene.CharacterDialog.Grid[i].Locked)
                        GameScene.Scene.CharacterDialog.Grid[i].Locked = false;
                #endregion
            }
        }

        public void Show()
        {
            if (!Visible)
                Visible = true;
        }

        public void DisplayResult(S.AddRuneResult info)
        {
            ItemInfo temp = null;
            if (info == null)
                return;

            switch (info.RuneResult)
            {
                case 0:         //  Add Succ
                    {
                        ItemCells[info.slot].Item = info.Item;
                        temp = GameScene.GetInfo(info.Item.ItemIndex);
                        ItemCells[info.slot].Item.Info = temp;
                        ItemCells[info.slot].Index = temp.Image;
                    }
                    break;
                case 1:         //  Remove Succ
                    {
                        ItemCells[info.slot].Item = info.Item;
                        temp = GameScene.GetInfo(info.Item.ItemIndex);
                        ItemCells[info.slot].Item.Info = temp;
                        ItemCells[info.slot].Index = temp.Image;
                    }
                    break;
                case 10:
                    GameScene.Scene.ChatDialog.ReceiveChatTr("Can socket same type item", ChatType.Hint);
                    break;
                default:
                    GameScene.Scene.ChatDialog.ReceiveChat("add rune error " + info.RuneResult, ChatType.Hint);
                    break;
            }
        }

        public void ClearRuneSockets()
        {
       
            for (int i=0; i< 3; i++)
            {
                ItemCells[i].Item = null;
                Runes[i] = null;
            }
        }

        public void PopulateRuneCells(UserItem item)
        {
            ClearRuneSockets();
            if (item == null)
                return;

            Item.Item = item;
            Item.Index = item.Info.Image;

            if (item.RuneSlots == null)
                return;

            for (int i = 0; i < item.RuneSlots.Length; i++)
            {
                SetSlotItem(i, item.RuneSlots[i] != null ? item.RuneSlots[i].SocketedItem : null);
            }
        }

        public void RemoveRune(S.RemoveRune p)
        {
            MirItemCell toCell;
            MirItemCell fromCell;

            int index = -1;

            for (int i = 0; i < ItemCells.Length; i++)
            {
                if (ItemCells[i] == null)
                    continue;
                if (ItemCells[i].Item == null)
                    continue;
                if (ItemCells[i].Item.UniqueID != p.UniqueID)
                    continue;
                index = i;
                break;
            }
            fromCell = ItemCells[index];



            switch (p.Grid)
            {
                case MirGridType.Inventory:
                    toCell = GameScene.Scene.InventoryDialog.Grid[p.To - GameScene.User.BeltIdx];
                    break;
                default:
                    return;
            }

            if (toCell == null || fromCell == null)
                return;

            toCell.Locked = false;
            fromCell.Locked = false;

            toCell.Item = fromCell.Item;
            fromCell.Item = null;
            GameScene.Scene.CharacterDuraPanel.GetCharacterDura();
            GameScene.User.RefreshStats();
        }

        public void SetSlotItem(int itemSlot, UserItem item)
        {
            RuneSlots[itemSlot].Visible = true;
            ItemCells[itemSlot].Visible = true;
            if (item == null)
                return;

            ItemInfo temp = GameScene.GetInfo(item.ItemIndex);
            item.Info = temp;

            ItemCells[itemSlot].Index = temp.Image;
            ItemCells[itemSlot].Item = item;
        }
    }
}
