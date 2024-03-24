using Client.MirGraphics;
using System;
namespace Client.MirObjects.Monsters
{
    class Jar1 : MonsterObject
    {
        public Jar1(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Jar1], 130 + (int)Direction * 3, 3, 3 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}

