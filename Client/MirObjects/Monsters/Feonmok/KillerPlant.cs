using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class KillerPlant : MonsterObject
    {
        public KillerPlant(uint objectID)
            : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KillerPlant], 1168 + (int)Direction * 4, 4, 4 * Frame.Interval, this));
                    break;

                case MirAction.Attack2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KillerPlant], 1200, 14, 14 * Frame.Interval, this));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KillerPlant], 1214, 10, 10 * Frame.Interval, this));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KillerPlant], 1236, 13, 13 * Frame.Interval, this));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(584 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(664 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(728 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(784 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(888 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange3:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(952 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange4:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(1008 + FrameIndex + (int)Direction * 7, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(1064 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.KillerPlant].DrawBlend(1088 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
