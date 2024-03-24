using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class RhinoPriest : MonsterObject
    {
        public RhinoPriest(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            if (CurrentAction == MirAction.AttackRange2)
                frameAction = MirAction.AttackRange1;
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RhinoPriest], 376 + (int)Direction * 9, 9, 9 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RhinoPriest], 448 + (int)Direction * 7, 7, 7 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;
            }

            return true;
        }
    }
}
