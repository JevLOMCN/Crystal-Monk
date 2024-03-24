﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class DogYoLin4 : MonsterObject
    {
        public DogYoLin4(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin4], 560, 8, 8 * Frame.Interval, this, 0, false));
                    break;

                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin4], 780 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;
                
                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin4], 860 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
        }

        protected override void ProcessAttackRange1Frame()
        {
            if (CMain.Time >= NextMotion)
            {
                GameScene.Scene.MapControl.TextureValid = false;

                if (SkipFrames) UpdateFrame();

                if (UpdateFrame() >= Frame.Count)
                {
                    FrameIndex = Frame.Count - 1;
                    SetAction();
                }
                else
                {
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            {
                                missile = CreateProjectile(760, Libraries.Monsters[(ushort)Monster.DogYoLin4], true, 6, 50, -6);
                                missile.Blend = false;

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin4], 770, 10, 1500, missile.Target));
                                    };
                                }

                                
                                missile = CreateProjectile(580, Libraries.Monsters[(ushort)Monster.DogYoLin4], true, 5, 50, 5);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin4], 740, 10, 1500, missile.Target));
                                    };
                                }
                                

                      
                            }
                            break;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }
    }


}
