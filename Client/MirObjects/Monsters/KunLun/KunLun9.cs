using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class KunLun9 : MonsterObject
    {
        public KunLun9(uint objectID) : base(objectID)
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
                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun9], 660 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
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
                                missile = CreateProjectile(480, Libraries.Monsters[(ushort)Monster.KunLun9], true, 10, 50, 0);
                            }
                            break;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }

        protected override void ProcessAttackRange2Frame()
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
                                missile = CreateProjectile(750, Libraries.Monsters[(ushort)Monster.KunLun9], true, 5, 50, 5);
                            }
                            break;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }
    }
}
