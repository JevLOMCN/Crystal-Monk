using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class DragonWarrior : MonsterObject
    {
        public DragonWarrior(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonWarrior], 552 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 4 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonWarrior], 576 + (int)Direction * 7, 7, 7 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonWarrior], 632 + (int)Direction * 4, 4, 4 * Frame.Interval, this, CMain.Time + 2 * Frame.Interval, true));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DragonWarrior], 504 + (int)Direction * 6, 6, 6 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
