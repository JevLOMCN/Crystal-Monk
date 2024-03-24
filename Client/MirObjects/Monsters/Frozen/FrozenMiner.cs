using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class FrozenMiner : MonsterObject
    {
        public FrozenMiner(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            Frames.Frames.TryGetValue(frameAction, out Frame);
            FrameIndex = 0;

            if (Frame == null)
            {
                CMain.SaveError(string.Format("{0} Frame not found ", CurrentAction));
                return false;
            }

            FrameInterval = Frame.Interval;

            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenMiner], 432 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenMiner], 462 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + 4 * Frame.Interval, true));
                    break;

            }

            return true;
        }
    }
}
