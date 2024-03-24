using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class Kirin : MonsterObject
    {
        public Kirin(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Kirin], 776 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + Frame.Interval * 1, true));
                    break;

                // 砍
                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Kirin], 824 + (int)Direction * 7, 7, 7 * Frame.Interval, this, CMain.Time + Frame.Interval * 7, true));
                    break;

                // 转
                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Kirin], 880 + (int)Direction * 9, 9, 9 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(392 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                  //  Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(416 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);  //standing2?
                  //  Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(424 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);  //standing2?
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(496 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(544 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(600 + FrameIndex + (int)Direction * 12, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(696 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(744 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.Kirin].DrawBlend(768 + FrameIndex + (int)Direction, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
