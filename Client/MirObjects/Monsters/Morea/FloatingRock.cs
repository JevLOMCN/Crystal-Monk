using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class FloatingRock : MonsterObject
    {
        public FloatingRock(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            if (frameAction == MirAction.AttackRange1)
                frameAction = MirAction.Attack1;
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FloatingRock], 160 + (int)Direction * 7, 7, 7 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FloatingRock], 216 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.FloatingRock], 152, 8, 8 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
