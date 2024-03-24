using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes.Dialogs;

namespace Client.MirObjects.Monsters
{
    class DarkWraith : MonsterObject
    {
        public DarkWraith(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 720 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 750 + (int)Direction * 5, 5, 5 * Frame.Interval, CurrentLocation, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkWraith], 796 + (int)Direction * 5, 5, 5 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(360 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(392 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(440 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(504 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(584 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(616 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.DarkWraith].DrawBlend(648 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
