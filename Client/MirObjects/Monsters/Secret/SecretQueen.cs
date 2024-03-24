using Client.MirGraphics;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class SecretQueen : MonsterObject
    {
        public SecretQueen(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();

            MirAction frameAction = CurrentAction;
            if (CurrentAction == MirAction.DashL || CurrentAction == MirAction.DashR)
                frameAction = MirAction.AttackRange2;

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
                // 砍
                case MirAction.Attack1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 424 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + Frame.Interval, true));

                 //   Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 637, 8, 8 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 471 + (int)Direction * 10, 10, 10 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.DashL:
                case MirAction.DashR:
                    // 0 1 2 3 4 5 6
                    for (int i = 0; i < 3; i++)
                    {
                        Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 615, 22, 22 * Frame.Interval,
                            Functions.PointMove(CurrentLocation, Direction, -(6 - i * 2)), CMain.Time + 100 + 200 * i, true)
                        );
                    }
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretQueen], 551 + (int)Direction * 8, 8, 8 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.AttackRange3:
                    break;
            }

            return true;
        }
    }
}
