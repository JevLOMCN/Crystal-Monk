using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class ScalyBeast : MonsterObject
    {
        public ScalyBeast(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ScalyBeast], 345 + (int)Direction * 6, 6, 6 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    if (Direction != MirDirection.UpLeft)
                        Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ScalyBeast], 393 + (int)Direction * 8, 8, 8 * Frame.Interval, this, 0, true));
                    else
                        Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ScalyBeast], 393 + 6 * 8, 8, 8 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
