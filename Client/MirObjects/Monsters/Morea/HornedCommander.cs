using Client.MirGraphics;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class HornedCommander : MonsterObject
    {
        public HornedCommander(uint objectID) : base(objectID)
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
                // 砍
                case MirAction.Attack1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1142 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + Frame.Interval * 3, true));
                    break;

                // 砍
                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 784 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + Frame.Interval*3, true));
                    break;

                // 转
                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 808 + (int)Direction * 7, 7, 7 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 864, 8, 8 * Frame.Interval, this, CMain.Time + 5 * Frame.Interval, false));
                    break;

                // 传送
                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 872 + (int)Direction * 7, 7, 7 * Frame.Interval, this, 0, true));
                    break;

                // 炸
                case MirAction.AttackRange4:
                    break;

                // 开盾
                case MirAction.AttackRange5:
                    FrameIntervals.Add(5, 18500);
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1166, 7, 7 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1173, 17, 17 * Frame.Interval, this, CMain.Time + 500) { Repeat = true, RepeatUntil = CMain.Time + 18500});
                    break;

                // 举锤子，地刺
                case MirAction.AttackRange6:
                    FrameIntervals.Add(3, 5500);
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1010 + (int)Direction * 2, 2, 2 * Frame.Interval, this, CMain.Time + 5 * Frame.Interval, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1026 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, false) { Repeat = true, RepeatUntil = CMain.Time + 5500});

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 938 + (int)Direction * 9, 7, 7 * Frame.Interval, this, CMain.Time + 5500, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1078 + (int)Direction * 8, 8, 8 * Frame.Interval, this, CMain.Time + 6000, true));
                    break;

                case MirAction.AttackRange7:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1199, 9, 9 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
