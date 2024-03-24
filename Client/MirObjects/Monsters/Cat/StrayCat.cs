using Client.MirGraphics;
using System;
namespace Client.MirObjects.Monsters
{
    class StrayCat : MonsterObject
    {
        public StrayCat(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StrayCat], 528 + (int)Direction * 5, 5, 5 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StrayCat], 568 + (int)Direction * 2, 2, 2 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StrayCat], 632 + (int)Direction * 12, 12, 12 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StrayCat], 584 + (int)Direction * 6, 6, 6 * Frame.Interval, this, CMain.Time + 800, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.StrayCat], 728 + (int)Direction * 10, 10, 10 * Frame.Interval, this, CMain.Time + 1000, true));
                    break;
            }


            return true;
        }
    }

}