using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class CannibalTentacles : MonsterObject
    {
        public CannibalTentacles(uint objectID) : base(objectID)
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
                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CannibalTentacles], 408 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CannibalTentacles], 488 + (int)Direction * 9, 9, 9 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
