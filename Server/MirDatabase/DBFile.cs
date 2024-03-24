using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MirObjects
{
    class DBFile
    {
        public int NextAccountID, NextCharacterID;
        public ulong NextUserItemID, NextAuctionID, NextMailID;
        public List<AccountInfo> AccountList = new List<AccountInfo>();
        public LinkedList<AuctionInfo> Auctions = new LinkedList<AuctionInfo>();
        public int GuildCount, NextGuildID;
        public List<GuildObject> GuildList = new List<GuildObject>();
        public List<MailInfo> Mail = new List<MailInfo>();
        public Dictionary<int, int> GameshopLog = new Dictionary<int, int>();

        public void load(string filepath)
        {
            using (FileStream stream = File.OpenRead(filepath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int LoadCustomVersion = 0;
                int LoadVersion = reader.ReadInt32();
                if (LoadVersion > 57)
                    LoadCustomVersion = reader.ReadInt32();
                NextAccountID = reader.ReadInt32();
                NextCharacterID = reader.ReadInt32();
                NextUserItemID = reader.ReadUInt64();

                if (LoadVersion > 27)
                {
                    GuildCount = reader.ReadInt32();
                    NextGuildID = reader.ReadInt32();
                }

                int count = reader.ReadInt32();
                AccountList.Clear();
                for (int i = 0; i < count; i++)
                {
                    AccountList.Add(new AccountInfo(reader));
                }

                if (LoadVersion < 7) return;

                foreach (AuctionInfo auction in Auctions)
                    auction.CharacterInfo.AccountInfo.Auctions.Remove(auction);
                Auctions.Clear();

                if (LoadVersion >= 8)
                    NextAuctionID = reader.ReadUInt64();

                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    AuctionInfo auction = new AuctionInfo(reader, LoadVersion, LoadCustomVersion);

                    Auctions.AddLast(auction);
                    auction.CharacterInfo.AccountInfo.Auctions.AddLast(auction);
                }

                if (LoadVersion == 7)
                {
                    foreach (AuctionInfo auction in Auctions)
                    {
                        if (auction.Sold && auction.Expired) auction.Expired = false;

                        auction.AuctionID = ++NextAuctionID;
                    }
                }

                if (LoadVersion > 43)
                {
                    NextMailID = reader.ReadUInt64();

                    Mail.Clear();

                    count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        Mail.Add(new MailInfo(reader, LoadVersion, LoadCustomVersion));
                    }
                }

                if (LoadVersion >= 63)
                {
                    int logCount = reader.ReadInt32();
                    for (int i = 0; i < logCount; i++)
                    {
                        GameshopLog.Add(reader.ReadInt32(), reader.ReadInt32());
                    }
                }
            }
        }
    }
}
