using Client.MirGraphics;
using Client.MirScenes;
using System;
using System.Drawing;

namespace Client.MirObjects.Monsters
{
    class Jar2 : MonsterObject
    {
        public Jar2(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.Jar2], 624 + (int)Direction * 8, 8, 8 * Frame.Interval, this, 0, true));
                    break;
                case MirAction.AttackRange1:
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
                                missile = CreateProjectile(1273, Libraries.Monsters[(ushort)Monster.SeedingsGeneral], true, 4, 50, 0);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        if (missile.Target.CurrentAction == MirAction.Dead) return;
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SeedingsGeneral], 1137, 8, 1200, missile.Target));
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

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.Standing:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(312 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(392 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.Attack1:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(392 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(440 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(520 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.Jar2].DrawBlend(544 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}

