using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class FrozenGolem : MonsterObject
    {
        public FrozenGolem(uint objectID) : base(objectID)
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
                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenGolem], 463, 10, 100 * Frame.Interval, this, CMain.Time + 1200, false));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FrozenGolem], 456, 7, 7 * Frame.Interval, this, CMain.Time+1200, true));
                    break;

            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.FrozenGolem].DrawBlend(264 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.FrozenGolem].DrawBlend(296 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.FrozenGolem].DrawBlend(344 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.FrozenGolem].DrawBlend(408 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.FrozenGolem].DrawBlend(432 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
