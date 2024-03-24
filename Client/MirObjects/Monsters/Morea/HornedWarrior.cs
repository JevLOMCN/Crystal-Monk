using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class HornedWarrior : MonsterObject
    {
        public HornedWarrior(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedWarrior], 752 + (int)Direction * 2, 2, 2 * Frame.Interval, this, CMain.Time + 4 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedWarrior], 768 + (int)Direction * 8, 8, 8 * Frame.Interval, this, CMain.Time + 8 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedWarrior], 832 + (int)Direction * 10, 10, 10 * Frame.Interval, this, CMain.Time + 4 * Frame.Interval, true));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(376 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(408 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(456 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(520 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(592 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(656 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.HornedWarrior].DrawBlend(680 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
