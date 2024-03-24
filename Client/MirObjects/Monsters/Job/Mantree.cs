using Client.MirGraphics;
using Client.MirScenes;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class ManTree : MonsterObject
    {
        public ManTree(uint objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Mantree], 472 + 4 * (int)Direction, 4, 4 * Frame.Interval, this,
                        CMain.Time + 4 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Mantree], 504 + (int)Direction * 2, 2, Frame.Count * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Mantree], 520, 8, 8 * Frame.Interval, this, CMain.Time + 1000)
                    {
                        OffsetLocation = Functions.PointMove(new Point(0, 0), Direction, 1)
                    });
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Mantree], 504 + (int)Direction * 2, 2, Frame.Count * Frame.Interval, this));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Mantree], 520, 8, 8 * Frame.Interval, this, CMain.Time + 1000)
                    {
                        OffsetLocation = Functions.PointMove(new Point(0, 0), Direction, 1)
                    });
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(528 + FrameIndex + 4 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(560 + FrameIndex + 6 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(608 + FrameIndex + 6 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(656 + FrameIndex + 2 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(672 + FrameIndex + 10 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(752 + FrameIndex + 6 * (int)Direction, DrawLocation, Color.White, true);
                    break;

                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.Mantree].DrawBlend(800 + FrameIndex + 4 * (int)Direction, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
