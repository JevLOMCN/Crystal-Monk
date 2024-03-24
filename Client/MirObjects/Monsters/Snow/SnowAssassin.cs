﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;

namespace Client.MirObjects.Monsters
{
    class SnowAssassin : MonsterObject
    {
        public SnowAssassin(uint objectID) : base(objectID)
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
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowAssassin], 325 + (int)Direction *4, 4, 4 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }
    }
}
