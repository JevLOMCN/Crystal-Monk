using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes.Dialogs;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class SecretJudge : MonsterObject
    {
        public SecretJudge(uint objectID) : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            if (frameAction == MirAction.AttackRange2)
                frameAction = MirAction.AttackRange1;
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
                    TargetID = (uint)action.Params[0];
                    MapControl.Effects.Add(new Effect(Libraries.Magic2, 1790 + (int)Direction * 10, 10, 10 * FrameInterval, CurrentLocation));
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretJudge], 365, 9, 9 * Frame.Interval, this, 0, true));
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
                                missile = CreateProjectile(410, Libraries.Magic2, true, 4, 30, 6);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWizard], 392, 6, 600, missile.Target) { Blend = true });
                                    };
                                }

                            }
                            break;
                        case 6:
                            {
                                missile = CreateProjectile(410, Libraries.Magic2, true, 4, 30, 6);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWizard], 392, 6, 600, missile.Target) { Blend = true });
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
