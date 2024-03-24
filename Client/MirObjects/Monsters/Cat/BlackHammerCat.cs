using Client.MirGraphics;
using System;
using System.Drawing;

namespace Client.MirObjects.Monsters
{
    class BlackHammerCat : MonsterObject
    {
        public BlackHammerCat(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StainHammerCat], 240 + (int)Direction * 4, 4, 4 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StainHammerCat], 272 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + 800, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.BlackHammerCat], 648 + (int)Direction * 11, 11, Frame.Count * Frame.Interval, this, CMain.Time + 200));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.BlackHammerCat], 736 + (int)Direction * 8, 8, Frame.Count * Frame.Interval, this, CMain.Time + 800));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(336 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(368 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(416 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(472 + FrameIndex + (int)Direction * 12, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(568 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.BlackHammerCat].DrawBlendEx(592 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
            }
        }
    }

}