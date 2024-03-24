using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class ClawBeast : MonsterObject
    {
        public ClawBeast(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            if (frameAction == MirAction.AttackRange1)
                frameAction = MirAction.Attack1;
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ClawBeast], 504 + (int)Direction * 8, 8, 8 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.ClawBeast].DrawBlend(256 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.ClawBeast].DrawBlend(288 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.ClawBeast].DrawBlend(336 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.ClawBeast].DrawBlend(416 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.ClawBeast].DrawBlend(440 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
