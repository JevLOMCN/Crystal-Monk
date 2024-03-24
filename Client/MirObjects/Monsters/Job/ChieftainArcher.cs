using Client.MirGraphics;
using Client.MirScenes;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class ChieftainArcher : MonsterObject
    {
        public ChieftainArcher(uint objectID)
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
                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange2:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange3:
                    TargetID = (uint)action.Params[0];
                    break;
            }

            return true;
        }

        // 312 5
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
                    if (FrameIndex == 2) PlaySwingSound();
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            if (MapControl.GetObject(TargetID) != null)
                            {
                                missile = CreateProjectile(312, Libraries.Monsters[(ushort)Monster.ChieftainArcher], true, 5, 30, 0);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainArcher], 392, 6, 600, missile.Target) { Blend = true });
                                    };
                                }
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
                    if (FrameIndex == 2) PlaySwingSound();
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            if (MapControl.GetObject(TargetID) != null)
                            {
                                missile = CreateProjectile(398, Libraries.Monsters[(ushort)Monster.ChieftainArcher], true, 5, 30, 0);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainArcher], 478, 6, 600, missile.Target) { Blend = true });
                                    };
                                }
                            }
                            break;
                    }

                    NextMotion += FrameInterval;
                }
            }
        }

        protected override void ProcessAttackRange3Frame()
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
                    if (FrameIndex == 2) PlaySwingSound();
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            missile = CreateProjectile(484, Libraries.Monsters[(ushort)Monster.ChieftainArcher], true, 5, 30, 0);
                            break;
                    }

                    NextMotion += FrameInterval;
                }
            }
        }


        // 
        // 392 6 爆炸
        // 398 10
        // 478 6 爆炸
        // 484 10
    }
}
