﻿using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class CatFire : MonsterObject
    {
        public CatFire(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.CatWidow], 248 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    break;
            }


            return true;
        }
    }

   
  

 

 


   

   
}