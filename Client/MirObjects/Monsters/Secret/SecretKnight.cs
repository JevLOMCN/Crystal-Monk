using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class SecretKnight : MonsterObject
    {
        public SecretKnight(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 384 + (int)Direction * 4, 4, 4 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 422 + (int)Direction * 6, 6, 6 * Frame.Interval, this, 0, true));
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 475, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretKnight], 485, 11, 11 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
