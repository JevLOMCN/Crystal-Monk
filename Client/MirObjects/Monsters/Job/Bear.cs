using Client.MirGraphics;
using Client.MirScenes;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class Bear : MonsterObject
    {
        public Bear(uint objectID)
            : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Bear], 321 + (int)Direction * 4, 4, Frame.Count * Frame.Interval, this, CMain.Time + 2 * Frame.Interval));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Bear], 353, 9, Frame.Count * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
