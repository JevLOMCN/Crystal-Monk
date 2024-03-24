using Client.MirGraphics;
using Client.MirScenes;
using System;
using System.Drawing;

namespace Client.MirObjects.Monsters
{
 class RestlessJar : MonsterObject
    {
        public RestlessJar(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RestlessJar], 384, 7, 7 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RestlessJar], 391 + (int)Direction * 10, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RestlessJar], 471, 5, 5 * Frame.Interval, this, 0, true)); // 
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
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            {
                                missile = CreateProjectile(476, Libraries.Monsters[(ushort)Monster.RestlessJar], true, 2, 50, 0);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        if (missile.Target.CurrentAction == MirAction.Dead) return;
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.RestlessJar], 508, 7, 1050, missile.Target));
                                        PlaySound(3497);
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

