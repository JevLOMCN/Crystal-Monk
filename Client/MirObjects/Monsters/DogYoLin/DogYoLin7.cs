using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class DogYoLin7 : MonsterObject
    {
        public DogYoLin7(uint objectID) : base(objectID)
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
                  //  Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1520, 10, 10 * Frame.Interval, this, 0, true));
                  //  Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1530, 10, 10 * Frame.Interval, this, 0, true));
                 //   Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1540, 10, 10 * Frame.Interval, this, 0, true));
                  //  Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1550, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange2:
                    break;

                case MirAction.AttackRange3:
                    break;

                case MirAction.AttackRange4:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1570 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, CMain.Time + 6 * Frame.Interval, true));
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
                                missile = CreateProjectile(1450, Libraries.Monsters[(ushort)Monster.DogYoLin7], true, 6, 30, -6);
                                missile.Blend = false;
                                missile = CreateProjectile(1480, Libraries.Monsters[(ushort)Monster.DogYoLin7], true, 6, 30, -6);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1460, 10, 1500, missile.Target));
                                    };
                                }
                            }
                            break;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }

        protected override void ProcessAttackRange4Frame()
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
                        case 6:
                            {
                                missile = CreateProjectile(1660, Libraries.Monsters[(ushort)Monster.DogYoLin7], true, 6, 50, 4);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1830, 10, 1500, missile.Target));
                                    };
                                }

                                missile = CreateProjectile(1660, Libraries.Monsters[(ushort)Monster.DogYoLin7], true, 6, 50, 4);
                                missile.Start = CMain.Time + 300;
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1830, 10, 1500, missile.Target));
                                    };
                                }
                            }
                            NextMotion += 700;
                            return;
                    }
                    NextMotion += FrameInterval;
                }
            }
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(720 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(800 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(880 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(960 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(1040 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange3:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(1120 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange4:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(1200 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(1280 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.DogYoLin7].DrawBlend(1360 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
