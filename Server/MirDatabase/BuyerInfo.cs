using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.MirDatabase
{
    public class BuyerInfo
    {
        public ulong ID;
        public int CharacterIndex;
        public int ItemIndex;
        public DateTime CreateDate;
        public bool Completed;

        public BuyerInfo(BinaryReader reader, int version, int customversion)
        {
            ID = reader.ReadUInt64();
            CharacterIndex = reader.ReadInt32();
            ItemIndex = reader.ReadInt32();
            CreateDate = DateTime.FromBinary(reader.ReadInt64());
            Completed = reader.ReadBoolean();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(ID);
            writer.Write(CharacterIndex);
            writer.Write(ItemIndex);
            writer.Write(CreateDate.ToBinary());
            writer.Write(Completed);
        }
    }
}
