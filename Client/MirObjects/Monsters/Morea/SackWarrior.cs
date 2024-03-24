using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class SackWarrior : MonsterObject
    {
        public SackWarrior(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SackWarrior], 344 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SackWarrior], 368 + (int)Direction * 2, 2, 2 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SackWarrior], 384, 9, 9 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SackWarrior], 393, 10, 10 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
