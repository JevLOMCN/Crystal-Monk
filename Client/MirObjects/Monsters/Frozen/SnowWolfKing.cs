using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.MirGraphics;
using System.Drawing;

namespace Client.MirObjects.Monsters
{
    class SnowWolfKing : MonsterObject
    {
        public SnowWolfKing(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 456 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + Frame.Interval * 4, true));
                    break;

                // 砍
                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 480, 9, 9 * Frame.Interval, this, CMain.Time + Frame.Interval * 1, true));
                    break;

                // 转
                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 489, 9, 9 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 561, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowWolfKing], 581 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + Frame.Interval * 1, true));
                    break;
            }

            return true;
        }
    }
}
