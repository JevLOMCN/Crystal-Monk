using Client.MirGraphics;
using System;
using System.Drawing;

namespace Client.MirObjects.Monsters
{
    class SeedingsGeneral : MonsterObject
    {
        public SeedingsGeneral(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1144 + (int)Direction * 6, 6, 6 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1072 + (int)Direction * 9, 9, 9 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1264, 9, 9 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1192 + (int)Direction * 9, 9, 9 * Frame.Interval, this, 0, true));
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1273 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1337, 8, 8 * Frame.Interval, this, 0, true));
                    break;
            }


            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.SitDown:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(536 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(568 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(600 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.Running:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(656 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(694 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(766 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(838 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange3:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(902 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(974 + FrameIndex + (int)Direction * 2, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.SeedingsGeneral].DrawBlend(1000 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}