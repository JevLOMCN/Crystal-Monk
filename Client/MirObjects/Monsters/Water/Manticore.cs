using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class Manticore : MonsterObject
    {
        public Manticore(uint objectID) : base(objectID)
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

                //   case MirAction.Turn:
                //   Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Manticore], 496 , 9, 9 * Frame.Interval, this, CMain.Time + 2 * Frame.Interval, true));
                //     break;

                case MirAction.Attack1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Manticore], 505 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Manticore], 536 + (int)Direction * 7, 7, 7 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Manticore], 592, 9, 9 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
