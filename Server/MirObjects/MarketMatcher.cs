using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirObjects
{
    public class MarketMatcher
    {
        public string MatchName = string.Empty;
        public ItemType MatchType = ItemType.Nothing;
        public ItemGrade GradeType = ItemGrade.None;
        public short MinShapes;
        public short MaxShapes = short.MaxValue;
        public RequiredClass RequiredClass = RequiredClass.NoLimit;
    }
}
