using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class WaterDragon : MonsterObject
    {
        public WaterDragon(uint objectID) : base(objectID)
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
                                missile = CreateProjectile(800, Libraries.Monsters[(ushort)Monster.WaterDragon], true, 6, 50, 0);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.WaterDragon], 896, 9, 9 * 150, missile.Target));
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
                    MapObject ob = null;
                    Missile missile;
                    switch (FrameIndex)
                    {
                        case 4:
                            {
                                missile = CreateProjectile(800, Libraries.Monsters[(ushort)Monster.WaterDragon], true, 6, 50, 0);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.WaterDragon], 905, 9, 9 * 150, missile.Target));
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
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(400 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.Walking:
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(464 + FrameIndex + (int)Direction * 6, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange1:
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(512 + FrameIndex + (int)Direction * 8, DrawLocation, Color.White, true);
                    break;
                case MirAction.AttackRange2:
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(576 + FrameIndex + (int)Direction * 10, DrawLocation, Color.White, true);
                    break;
                case MirAction.Struck:
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(656 + FrameIndex + (int)Direction * 3, DrawLocation, Color.White, true);
                    break;
                case MirAction.Die:
                    Libraries.Monsters[(ushort)Monster.WaterDragon].DrawBlend(680 + FrameIndex + (int)Direction * 15, DrawLocation, Color.White, true);
                    break;
            }
        }
    }
}
