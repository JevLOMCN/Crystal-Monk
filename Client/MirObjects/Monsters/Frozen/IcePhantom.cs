using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class IcePhantom : MonsterObject
    {
        public IcePhantom(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IcePhantom], 640 + (int)Direction * 4, 4, 4 * Frame.Interval, this, CMain.Time + 2 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IcePhantom], 692 + (int)Direction * 10, 10, 10 * Frame.Interval, this, CMain.Time + 10 * Frame.Interval, false));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.IcePhantom], 672, 20, 20 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(320 + FrameIndex + (int)Direction * 4, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(352 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(400 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(472 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(536 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.IcePhantom].DrawBlend(560 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
