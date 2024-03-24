using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class TreeQueen : MonsterObject
    {
        public TreeQueen(uint objectID)
            : base(objectID)
        {

        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                case MirAction.AttackRange1:
                case MirAction.AttackRange2:
                case MirAction.AttackRange3:
                    PlaySound(3616);
                    frameAction = MirAction.Attack1;
                    break;
            }

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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 35, 22, 22 * Frame.Interval, this, 0, false));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 57, 9, 9 * Frame.Interval, this, 0, true) { Blend = false });
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 66, 9, 9 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 75, 15, 15 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 90, 16, 16 * Frame.Interval, this, CMain.Time + 800, false) { Blend = false}
                    );
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TreeQueen], 106, 14, 14 * Frame.Interval, this, CMain.Time + 800, false)
                    );
                    break;
            }

            return true;
        }
    }
}
