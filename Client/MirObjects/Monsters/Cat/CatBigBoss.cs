using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class CatBigBoss : MonsterObject
    {
        public Effect ShieldEffect;

        public CatBigBoss(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    break;

                case MirAction.AttackRange1:
                    break;

                case MirAction.AttackRange2:
                    PlaySound(3616);
                    break;

                case MirAction.AttackRange3:
                    PlaySound(3616);
                    frameAction = MirAction.AttackRange2;
                    break;

                case MirAction.AttackRange4:
                    frameAction = MirAction.AttackRange2;
                    break;
            }

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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.GeneralJinmYo], 416 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + 4 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.GeneralJinmYo], 456 + (int)Direction * 7, 7, Frame.Count * Frame.Interval, this, CMain.Time + 7* Frame.Interval, true));
                    break;
            }

            return true;
        }

    }
}
