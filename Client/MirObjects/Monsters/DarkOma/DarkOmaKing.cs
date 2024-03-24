using Client.MirGraphics;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class DarkOmaKing : MonsterObject
    {
        public DarkOmaKing(uint objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkOmaKing], 1568 + (int)Direction * 4, 4, 4 * Frame.Interval, this));
                    break;
         

                case MirAction.AttackRange1:
                  MapObject ob = MapControl.GetObject(TargetID);
                    if (ob != null)
                    {
                        ob.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkOmaKing], 1715, 13, 900, ob));
                    }
                    break;

                case MirAction.Attack2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkOmaKing], 1600, 30, 30 * Frame.Interval, this));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DarkOmaKing], 1702, 13, 900, this));
                    break;
            }

            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(784 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(864 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(912 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1568 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack2:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(984 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1600 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack3:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1392 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1702 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1256 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1320 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1464 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.DarkOmaKing].DrawBlend(1488 + FrameIndex + (int)Direction * 9, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
