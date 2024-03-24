using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects
{
    public class SockService : BaseService
    {
        public bool CanSocketItem(UserItem item, byte slot, UserItem usingItem)
        {
            if (item == null)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 5 });
                return false;
            }

            if (usingItem == null)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 5 });
                return false;
            }
            //  Get the runes Item Info
            ItemInfo runeItem = Envir.GetItemInfo(usingItem.ItemIndex);
            if (runeItem == null)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 5 });
                return false;
            }

            if (item.RuneSlots.Length == 0)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 6 });
                return false;
            }

            bool hasSlot = false;
            for (int i = 0; i < item.RuneSlots.Length; i++)
            {
                if (item.RuneSlots[i] != null)
                    continue;

                hasSlot = true;
            }

            if (!hasSlot)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 6 });
                return false;
            }

            bool canSocket = true;
            for (int i = 0; i < item.RuneSlots.Length; i++)
            {
                if (item.RuneSlots[i] == null)
                    continue;

                //  Get the current socket occupying that item
                ItemInfo mainItem = Envir.GetItemInfo(item.RuneSlots[i].SocketedItem.ItemIndex);
                //  Check if the shapes match
                if (mainItem.Shape == runeItem.Shape)
                    canSocket = false;
            }
            if (!canSocket)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 10 });
                return false;
            }
            //  Same Slot
            Sockets socket = item.RuneSlots.Where(sock => sock != null && sock.Slot == slot).FirstOrDefault();
            if (socket != null)
            {
                Enqueue(new S.AddRuneResult { RuneResult = 11 });
                return false;
            }
            return true;
        }

        public void SocketItem(ulong item, byte slot, ulong usingItem)
        {
            UserItem _item = null;
            int idxItem = -1;
            int idxUsingItem = -1;
            UserItem _usingItem = null;

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                if (Info.Equipment[i] == null)
                    continue;
                if (Info.Equipment[i].UniqueID == item)
                {
                    _item = Info.Equipment[i];
                    idxItem = i;
                }
            }
            if (_item == null)
            {
                for (int i = 0; i < Info.Inventory.Length; i++)
                {
                    if (Info.Inventory[i] == null)
                        continue;
                    if (Info.Inventory[i].UniqueID == item)
                    {
                        _item = Info.Inventory[i];
                        idxItem = i;
                    }
                }
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                if (Info.Inventory[i] == null)
                    continue;
                if (Info.Inventory[i].UniqueID == usingItem)
                {
                    _usingItem = Info.Inventory[i];
                    idxUsingItem = i;
                }
            }

            if (!CanSocketItem(_item, slot, _usingItem))
                return;

            if (_item != null)
            {
                Sockets socket = new Sockets
                {
                    Slot = slot,
                    SocketedItem = _usingItem
                };
                ItemInfo temp2 = Envir.GetItemInfo(_usingItem.ItemIndex);
                socket.SocketItemType = (SocketType)temp2.Shape;
                socket.CanRemove = true;
                for (int i = 0; i < _item.RuneSlots.Length; ++i)
                {
                    if (_item.RuneSlots[i] == null)
                    {
                        _item.RuneSlots[i] = socket;
                        break;
                    }
                }

                Info.Inventory[idxUsingItem] = null;
                Enqueue(new S.DeleteItem { UniqueID = usingItem, Count = 1 });
                Enqueue(new S.AddRuneResult { RuneResult = 0, Item = socket.SocketedItem, slot = socket.Slot });
                Enqueue(new S.RefreshItem { Item = _item });
                RefreshStats();
            }
        }

        public void RemoveSocket(MirGridType grid, ulong id, int to)
        {
            S.RemoveRune p = new S.RemoveRune { Grid = grid, UniqueID = id, To = to };
            UserItem[] array;
            switch (grid)
            {
                case MirGridType.Inventory:
                    array = Info.Inventory;
                    break;
                default:
                    return;
            }

            if (to < 0 || to >= array.Length)
                return;

            UserItem temp = null;
            UserItem runeFound = null;
            UserItem runeHolder = null;
            int index = -1;

            for (int i = 0; i < Info.Equipment.Length; i++)
            {
                temp = Info.Equipment[i];
                if (temp == null)
                    continue;
                if (temp.RuneSlots.Length <= 0)
                    continue;

                for (int x = 0; x < temp.RuneSlots.Length; x++)
                {
                    if (temp.RuneSlots[x] != null && temp.RuneSlots[x].SocketedItem.UniqueID == id)
                    {
                        runeFound = temp.RuneSlots[x].SocketedItem;
                        temp.RuneSlots[x] = null;
                        runeHolder = temp;
                        index = x;
                        break;
                    }
                }
            }

            for (int i = 0; i < Info.Inventory.Length; i++)
            {
                temp = Info.Inventory[i];
                if (temp == null)
                    continue;
                if (temp.RuneSlots.Length <= 0)
                    continue;

                for (int x = 0; x < temp.RuneSlots.Length; x++)
                {
                    if (temp.RuneSlots[x] != null && temp.RuneSlots[x].SocketedItem.UniqueID == id)
                    {
                        runeFound = temp.RuneSlots[x].SocketedItem;
                        temp.RuneSlots[x] = null;
                        runeHolder = temp;
                        index = x;
                        break;
                    }
                }
            }

            if (runeFound == null)
            {
                return;
            }

            if (array[to] == null)
            {
                array[to] = runeFound;
                Enqueue(p);
                RefreshStats();
                Broadcast(GetUpdateInfo());
                Enqueue(new S.RefreshItem { Item = runeHolder });
                Report.ItemMoved("RemoveItem", temp, MirGridType.SockeRune, grid, index, to);
                return;
            }

            Enqueue(p);
        }

    }
}
