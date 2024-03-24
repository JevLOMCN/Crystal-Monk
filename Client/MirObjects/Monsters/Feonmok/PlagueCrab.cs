using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class PlagueCrab : MonsterObject
    {
        public PlagueCrab(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            switch (CurrentAction)
            {
                case MirAction.AttackRange1:
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
                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.PlagueCrab], 488 + (int)Direction * 7, 7, 7 * Frame.Interval, this, 0, true));
                    TargetID = (uint)action.Params[0];
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.PlagueCrab].DrawBlend(248 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.PlagueCrab].DrawBlend(280 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.PlagueCrab].DrawBlend(328 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.PlagueCrab].DrawBlend(392 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.PlagueCrab].DrawBlend(416 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
