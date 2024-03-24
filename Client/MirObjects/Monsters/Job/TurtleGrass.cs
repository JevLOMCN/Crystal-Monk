using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class TurtleGrass : MonsterObject
    {
        public TurtleGrass(uint objectID)
            : base(objectID)
        {

        }

        protected override void SetAttack1Action()
        {
            Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Turtlegrass], 360 + (int)Direction * 6, 6, Frame.Count * Frame.Interval, this));
        }
    }
}
