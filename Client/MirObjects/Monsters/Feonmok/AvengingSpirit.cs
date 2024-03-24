using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class AvengingSpirit : MonsterObject
    {
        public AvengingSpirit(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.AvengingSpirit], 280 + (int)Direction * 8, 8, 8 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.AvengingSpirit], 344 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.AvengingSpirit], 442 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
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
                                missile = CreateProjectile(368, Libraries.Monsters[(ushort)Monster.AvengingSpirit], true, 4, 50, 0);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.AvengingSpirit], 432, 10, 1500, missile.Target));
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
