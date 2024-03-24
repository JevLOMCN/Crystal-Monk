using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class DogYoLin6 : MonsterObject
    {
        public DogYoLin6(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            if (CurrentAction == MirAction.DashL || CurrentAction == MirAction.DashR)
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
                case MirAction.Attack1:
                    break;

                    // 转
                case MirAction.AttackRange1:
                //    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 920, 10, 10 * Frame.Interval, this, 0, false));
                    break;

                    // 
                case MirAction.AttackRange2:
                    break;

                    // 大砍
                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 730 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 810 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, CMain.Time + 6 * Frame.Interval, true));
                    break;

                case MirAction.DashL:
                case MirAction.DashR:
                    // 0 1 2 3 4 5 6
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 720, 8, 8 * Frame.Interval, this, 0, false));
                    break;

                case MirAction.AttackRange4:
                 //   Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 900 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
