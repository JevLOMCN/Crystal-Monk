using Client.MirGraphics;
using Client.MirScenes;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class ShellFighter : MonsterObject
    {
        public ShellFighter(uint objectID)
            : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ShellFighter], 592, 9, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ShellFighter], 744, 30, 30 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange4:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ShellFighter], 776, 30, 30 * Frame.Interval, this, 0, true));
                    break;
            }

            return true;
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
                    switch (FrameIndex)
                    {
                        case 4:
                            {
                                Missile missile = CreateProjectile(664, Libraries.Monsters[(ushort)Monster.ShellFighter], true, 5, 30, 7);
                                break;
                            }
                    }
                    NextMotion += FrameInterval;
                }
            }
        }


    }
}
