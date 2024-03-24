using Client.MirGraphics;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class HornedSorceror : MonsterObject
    {
        public HornedSorceror(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedSorceror], 536 + (int)Direction * 2, 2, 4 * Frame.Interval, this, CMain.Time + 400, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedSorceror], 552 + (int)Direction * 9, 9, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedSorceror], 624, 10, 10 * Frame.Interval, this, 0, true));   
                    break;

                case MirAction.AttackRange3:
                    break;

                case MirAction.AttackRange4:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedSorceror], 644 + (int)Direction * 5, 5, Frame.Count * Frame.Interval, CurrentLocation, 0, true));
                    break;

            }

            return true;
        }

    }
}
