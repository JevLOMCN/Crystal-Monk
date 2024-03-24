using System;
using Server.MirDatabase;
using Server.MirEnvir;

using S = ServerPackets;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using Server.MirNetwork;
using System.Collections.Generic;
using Server.MirProtect;

namespace Server.MirObjects
{
    public class CommandMgr
    {
        protected static Envir Envir
        {
            get { return SMain.Envir; }
        }

        private PlayerObject p;

        private bool IsGM
        {
            get { return p.IsGM; }
        }

        public CommandMgr(PlayerObject p)
        {
            this.p = p;
        }

        private uint CharPage;

        public void ReceiveChat(string text, ChatType type)
        {
            p.ReceiveChat(text, type);
        }

        public static string ConvertToFileSize(double fileSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (fileSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fileSize = fileSize / 1024;
            }
            return string.Format("{0:0.#} {1}", fileSize, sizes[order]);
        }

        private void AnalyseDrop(string[] parts)
        {
            MonsterInfo info = Envir.GetMonsterInfo(parts[1]);
            if (info == null)
            {
                p.ReceiveChat(string.Format("找不到 {0}", parts[1]), ChatType.System);
                return;
            }

            float total = 0, potion = 0, equip = 0, commonEquip = 0F, book = 0F;
            for (int i = 0; i < info.Drops.Count; ++i)
            {
                DropInfo info2 = info.Drops[i][0];
                if (info2 == null || info2.Item == null)
                    continue;

                float rate = 1F / info2.Chance;
                total += rate;

                switch (info2.Item.Type)
                {
                    case ItemType.Potion:
                        potion += rate;
                        break;

                    case ItemType.Book:
                        book += rate;
                        break;

                    case ItemType.Armour:
                    case ItemType.Bracelet:
                    case ItemType.Necklace:
                    case ItemType.Ring:
                    case ItemType.Belt:
                    case ItemType.Boots:
                        if (info2.Item.Grade > ItemGrade.Common)
                            equip += rate;
                        else
                            commonEquip += rate;
                        break;
                }
            }
            p.ReceiveChat(string.Format("总爆率: {0} 药水掉落 {1} 普通装备 {2} 装备掉落 {3} 书籍掉落 {4}", total, potion, commonEquip, equip, book), ChatType.System);
        }

        public void ProcessCommand(string[] parts)
        {
            switch (parts[0].ToUpper())
            {
                case "ONLINECOUNT":
                    string AvailMem = string.Format("{0}", ConvertToFileSize(new ComputerInfo().AvailablePhysicalMemory));
                    ReceiveChat(string.Format("Online Count:{0} AvailableMem:{1}", Envir.PlayerCount, AvailMem), ChatType.Hint);
                    break;

                case "QUERYINVENTORY":
                    QueryInventory(parts);
                    break;

                case "QUERYSTORAGE":
                    QueryStorage(parts);
                    break;

                case "QUERYEQUIP":
                    QueryEquipment(parts);
                    break;

                case "QUERYALLCHARACTER":
                    QueryAllCharacter(parts);
                    break;

                case "DELETEINVENTORY":
                    DeleteInventory(parts);
                    break;

                case "DELETESTORAGE":
                    DeleteStorage(parts);
                    break;

                case "DELETEEQUIPMENT":
                    DeleteEquipment(parts);
                    break;

                case "QUERYCHARACTER":
                    QueryCharacter(parts);
                    break;

                case "QUERYONLINE":
                    QueryOnline(parts);
                    break;

                case "REMOVECREDIT":
                    RemoveCredit(parts);
                    break;

                case "REMOVEGOLD":
                    RemoveGold(parts);
                    break;

                case "RELOADSHOP":
                    {
                        if (!IsGM) return;
                        Envir.LoadGameShopList();
                        ReceiveChat("GameShopReload", ChatType.Hint);
                    }
                    break;

                case "HUMUP"://stupple
                    if (!IsGM) return;
                    PlayerObject player = p;
                    if (parts.Length > 1)
                        player = Envir.GetPlayer(parts[1]);
                    player.Humup();
                    break;

                case "RELOADITEMS":
                    if (!IsGM) return;
                    foreach (var newInfo in SMain.EditEnvir.ItemInfoList.Values)
                    {
                        ItemInfo info = Envir.GetItemInfo(newInfo.Index);
                        if (info != null)
                            info.CopyFrom(newInfo);
                    }
                    for (int i = 0; i < SMain.Envir.Connections.Count; i++)
                    {
                        MirConnection conn = SMain.Envir.Connections[i];
                        if (conn != null)
                            conn.SentItemInfo.Clear();
                    }
                    ReceiveChat("Items Reloaded.", ChatType.Hint);

                    break;

                case "RELOADMONSTERS":
                    if (!IsGM) return;
                    for (int i = 0; i < SMain.EditEnvir.MonsterInfoList.Count; i++)
                    {
                        MonsterInfo newInfo = SMain.EditEnvir.MonsterInfoList[i];
                        MonsterInfo info = Envir.GetMonsterInfo(newInfo.Index);

                        if (info != null)
                            info.CopyFrom(newInfo);
                    }

                    ReceiveChat("Monsters Reloaded.", ChatType.Hint);
                    break;
                case "RELOADIPBLACKLIST":
                    if (!IsGM) return;
                    ServiceIP.Reload();
                    ReceiveChat("IPBlackList Reloaded.", ChatType.Hint);
                    break;
                case "RELOADPROTECT":
                    if (!IsGM) return;
                    ProtectSettings.Load();
                    ReceiveChat("ProtectSettings Reloaded", ChatType.Hint);
                    break;
                case "ADDIPBLACKLIST":
                    if (!IsGM) return;
                    if (parts.Length < 2) return;
                    ServiceIP.AddIP(parts[1]);
                    ReceiveChat("add black ip:" + parts[1], ChatType.Hint);
                    break;

                case "DUMPALLCONN":
                    if (!IsGM) return;
                    DumpAllConn();
                    break;
                case "CLOSEPROTECT":
                    if (!IsGM) return;
                    ServiceNetStat.Close = true;
                    ReceiveChat("close protect", ChatType.Hint);
                    break;
                case "OPENPROTECT":
                    if (!IsGM) return;
                    ServiceNetStat.Close = false;
                    ReceiveChat("open protect", ChatType.Hint);
                    break;

                case "掉落":
                    if (parts.Length < 2) return;

                    if (Envir.DropMap.ContainsKey(parts[1]))
                        p.ReceiveChat(Envir.DropMap[parts[1]], ChatType.System);
                    else
                        p.ReceiveChat(string.Format("找不到该物品 {0}", parts[1]), ChatType.System);
                    break;

                case "ANALYSEDROP":
                    if (!IsGM) return;
                    if (parts.Length < 1) return;
                    AnalyseDrop(parts);
                    break;

                case "CLEARREGION":
                    ClearRegion(parts);
                    break;

                case "转移元宝":
                    TransferAccounts(parts);
                    break;

                case "DUMPMAP":
                    DumpMap();
                    break;

                case "RELOADREGION":
                    ReloadRegion();
                    break;

                case "TOGGLESLOWLOG":
                    if (!IsGM) return;
                    Envir.ToggleSlowFrameLog = !Envir.ToggleSlowFrameLog;
                    break;

                default:
                    break;
            }
        }

        private void DumpAllConn()
        {
            if (!IsGM) return;

            for (int i = 0; i < SMain.Envir.Connections.Count; i++)
            {
                MirConnection conn = SMain.Envir.Connections[i];

                string name = conn.Player != null ? conn.Player.Name : "NULL";
                long onlineTime = (SMain.Envir.Time - conn.TimeConnected) / 1000;
                NetStat netStat = conn.NetStat;
                string text = string.Format("IPAddress:{0} NickName:{1} OnlineTime:{2} sec RecvPktCount:{3} RecvDataSize:{4} RecvNumPerSec:{5}", conn.IPAddress, name, onlineTime, netStat.RecvPktCount, netStat.RecvDataSize, netStat.RecvNumPerSec);

                SMain.Enqueue(text);
                ReceiveChat(text, ChatType.Hint);
            }
        }

        private void ClearRegion(string[] parts)
        {
            if (!IsGM) return;
            if (parts.Length < 1) return;

            byte regionID;
            byte.TryParse(parts[1], out regionID);

            int count = 0;
            for (int i = Envir.AccountList.Count - 1; i >= 0; --i)
            {
                if (Envir.AccountList[i].RegionID != regionID)
                    continue;

                SMain.Enqueue(string.Format("Remove account {0}", Envir.AccountList[i].AccountID));
                Envir.AccountList.RemoveAt(i);
                ++count;
            }

            List<AuctionInfo> list = new List<AuctionInfo>();
            foreach (AuctionInfo auction in Envir.Auctions)
            {
                if (auction.CharacterInfo != null
                    && auction.CharacterInfo.AccountInfo != null
                    && auction.CharacterInfo.AccountInfo.RegionID != regionID)
                    continue;

                list.Add(auction);
            }

            foreach (AuctionInfo auction in list)
            {
                Envir.Auctions.Remove(auction);
            }
            SMain.Enqueue(string.Format("Remove AuctionInfo {0}", list.Count));
            ReceiveChat(string.Format("Remove AuctionInfo {0}", list.Count), ChatType.Hint);

            Envir.GuildList.RemoveAll(x =>
            {
                CharacterInfo info = Envir.GetCharacterInfo(x.Ranks[0].Members[0].Id);
                if (info.AccountInfo.RegionID == regionID)
                {
                    string m = string.Format("Remove guildobject:{0}", x.Name);
                    SMain.Enqueue(m);
                    ReceiveChat(m, ChatType.Hint);
                    return true;
                }
                return false;
            }
            );

            string message = string.Format("Remove account:{0}", count);
            ReceiveChat(message, ChatType.Hint);
        }

        private void QueryInventory(string[] parts)
        {
            if (!IsGM) return;
            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null)
                return;

            ReceiveChat(Envir.Tr("--Inventory Info--"), ChatType.System2);
            for (int i = 0; i < info.Inventory.Length; ++i)
            {
                UserItem userItem = info.Inventory[i];
                if (userItem == null)
                    continue;

                string message = string.Format("Slot:{0} Name:{1}", i, userItem.Name);
                ReceiveChat(message, ChatType.System2);
            }
        }

        private void QueryStorage(string[] parts)
        {
            if (!IsGM) return;

            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null || info.AccountInfo == null)
            {
                ReceiveChat("Account info is null", ChatType.System2);
                return;
            }

            AccountInfo accountInfo = info.AccountInfo;
            ReceiveChat(Envir.Tr("--Storage Info--"), ChatType.System2);
            for (int i = 0; i < accountInfo.Storage.Length; ++i)
            {
                UserItem userItem = accountInfo.Storage[i];
                if (userItem == null)
                    continue;

                string message = string.Format("Slot:{0} Name:{1}", i, userItem.Name);
                ReceiveChat(message, ChatType.System2);
            }
        }

        private void QueryEquipment(string[] parts)
        {
            if (!IsGM) return;
            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null)
                return;

            ReceiveChat(Envir.Tr("--Equipment Info--"), ChatType.System2);
            for (int i = 0; i < info.Equipment.Length; ++i)
            {
                UserItem userItem = info.Equipment[i];
                if (userItem == null)
                    continue;

                string message = string.Format("Slot:{0} Name:{1}", i, userItem.Name);
                ReceiveChat(message, ChatType.System2);
            }
        }

        private void DeleteInventory(string[] parts)
        {
            if (!IsGM) return;
            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null)
                return;

            int slot = -1;
            if (!int.TryParse(parts[2], out slot))
                return;

            if (slot < 0 || slot >= info.Inventory.Length)
                return;

            ReceiveChat(string.Format("Begin delete inventory {0}", info.Inventory[slot].Name != null ? info.Inventory[slot].Name : "NULL"), ChatType.System2);
            info.Inventory[slot] = null;
            ReceiveChat("DeleteInventory success", ChatType.System2);
        }

        private void DeleteStorage(string[] parts)
        {
            if (!IsGM) return;
            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null || info.AccountInfo == null)
            {
                ReceiveChat("Account info is null", ChatType.System2);
                return;
            }

            AccountInfo accountInfo = info.AccountInfo;

            int slot = -1;
            if (!int.TryParse(parts[2], out slot))
                return;

            if (slot < 0 || slot >= accountInfo.Storage.Length)
                return;

            ReceiveChat(string.Format("Begin delete storage {0}", accountInfo.Storage[slot].Name != null ? accountInfo.Storage[slot].Name : "NULL"), ChatType.System2);
            accountInfo.Storage[slot] = null;
            ReceiveChat("Delete storage success", ChatType.System2);
        }

        private void DeleteEquipment(string[] parts)
        {
            if (!IsGM && parts.Length > 0) return;
            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null)
                return;

            int slot = -1;
            if (!int.TryParse(parts[2], out slot))
                return;

            if (slot < 0 || slot >= info.Equipment.Length)
                return;

            ReceiveChat(string.Format("Begin delete equipment {0}", info.Equipment[slot].Name != null ? info.Equipment[slot].Name : "NULL"), ChatType.System2);
            info.Equipment[slot] = null;
            ReceiveChat("Delete equipment success", ChatType.System2);
        }

        private void QueryAllCharacter(string[] parts)
        {
            if (!IsGM) return;

            ReceiveChat(Envir.Tr("--Character Info--"), ChatType.System2);

            uint page = CharPage;
            if (parts.Length > 1)
                uint.TryParse(parts[1], out page);
            uint count = 30;

            S.OnlinePlayers packet = new S.OnlinePlayers();
            for (uint i = 0; i < count; i++)
            {
                long cur = (i + page) % Envir.CharacterList.Count;
                if (cur >= Envir.CharacterList.Count)
                    break;

                CharacterInfo characterInfo = Envir.CharacterList[(int)cur];
                packet.list.Add(characterInfo.ToSelectInfo());
            }
            CharPage = page + count;
        }

        private void QueryCharacter(string[] parts)
        {
            if (!IsGM) return;

            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null || info.AccountInfo == null)
            {
                ReceiveChat("Account info is null", ChatType.System2);
                return;
            }

            ReceiveChat(string.Format(Envir.Tr("Account:{0} Name:{1}, Region:{2}, Level:{3}, X:{4}, Y:{5} Credit:{6} Gold:{7}"),
                  info.AccountInfo.AccountID,
                  info.Name,
                  info.AccountInfo.RegionID,
                  info.Level,
                  info.CurrentLocation.X,
                  info.CurrentLocation.Y,
                  info.AccountInfo.Credit,
                  info.AccountInfo.Gold),
                  ChatType.System2);
        }

        private void RemoveGold(string[] parts)
        {
            if (!IsGM) return; 

            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null || info.AccountInfo == null)
            {
                ReceiveChat("Account info is null", ChatType.System2);
                return;
            }

            uint count = 0;
            if (parts.Length > 1)
                uint.TryParse(parts[2], out count);

            if (info.AccountInfo.Gold < count)
                count = info.AccountInfo.Gold;

            info.AccountInfo.Gold -= count;

            PlayerObject player = Envir.GetPlayer(parts[1]); 
            if (player != null)
                player.Enqueue(new S.LoseGold { Gold = count});
        }

        private void RemoveCredit(string[] parts)
        {
            if (!IsGM) return;

            CharacterInfo info = Envir.GetCharacterInfo(parts[1]);
            if (info == null || info.AccountInfo == null)
            {
                ReceiveChat("Account info is null", ChatType.System2);
                return;
            }

            uint count = 0;
            if (parts.Length > 1)
                uint.TryParse(parts[2], out count);

            if (info.AccountInfo.Credit < count)
                count = info.AccountInfo.Credit;

            info.AccountInfo.Credit -= count;

            PlayerObject player = Envir.GetPlayer(parts[1]);
            if (player != null)
                player.Enqueue(new S.LoseCredit { Credit = count });
        }

        private void QueryOnline(string[] parts)
        {
            if (!IsGM) return;

            ReceiveChat(Envir.Tr("--Online Character Info--"), ChatType.System2);

            uint page = CharPage;
            if (parts.Length > 1)
                uint.TryParse(parts[1], out page);

            S.OnlinePlayers packet = new S.OnlinePlayers();
            for (uint i = 0; i < Envir.Players.Count; i++)
            {
                PlayerObject obj = Envir.Players[(int)i];

                CharacterInfo characterInfo = obj.Info;
                packet.list.Add(characterInfo.ToSelectInfo());
            }

            p.Enqueue(packet);
        }

        private void TransferAccounts(string[] parts)
        {
            if (parts.Length < 4)
            {
                p.ReceiveChat("请输入用户名 区服 数量! 格式：@转移元宝 张三 2 1000", ChatType.System);
                return;
            }

            byte region;
            byte.TryParse(parts[2], out region);

            uint amount = CharPage;
            uint.TryParse(parts[3], out amount);

            AccountInfo info = Envir.GetAccount(parts[1], region);
            if (info == null)
            {
                p.ReceiveChat("找不到该玩家!", ChatType.System);
                return;
            }

            if (p.Account.Credit < amount)
            {
                p.ReceiveChat("你的货币不足!", ChatType.System);
                return;
            }

            p.Account.Credit -= amount;
            p.Enqueue(new S.LoseCredit { Credit = amount });
            info.Credit += amount;

            for (int i = 0; i < info.Characters.Count; ++i)
            {
                PlayerObject player = Envir.GetPlayer((uint)info.Characters[i].Index);
                if (player != null)
                {
                    player.Enqueue(new S.GainedCredit { Credit = amount });
                }
            }
            p.ReceiveChat(string.Format("元宝转移成功! 账号:{0}, 区:{1}, 数量:{2}", parts[1], parts[2], parts[3]), ChatType.System);
            p.Report.CreditChanged("TransferCredit", amount, p.Account.Credit, true, parts[1] + parts[2]);
        }

        private void DumpMap()
        {
            if (!IsGM)
                return;

            string str = p.CurrentMap.DumpInfo();
            p.ReceiveChat(str, ChatType.System);
            SMain.EnqueueDebugging(str);
        }

        private void ReloadRegion()
        {
            Settings.LoadRegionList();
            p.ReceiveChat("RegionList reloaded", ChatType.Hint);
        }
    }
}