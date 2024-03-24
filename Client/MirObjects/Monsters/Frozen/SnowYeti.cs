using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class SnowYeti : MonsterObject
    {
        public SnowYeti(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowYeti], 504 + (int)Direction * 4, 4, 4 * Frame.Interval, this, CMain.Time + 5 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SnowYeti], 536 + (int)Direction * 3, 3, 3 * Frame.Interval, this, CMain.Time + 5 * Frame.Interval, true));
                    TargetID = (uint)action.Params[0];
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
                                missile = CreateProjectile(560, Libraries.Monsters[(ushort)Monster.SnowYeti], true, 6, 50, 0);
                            }
                            break;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }
    }
}
