using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class KunLun14 : MonsterObject
    {
        public KunLun14(uint objectID) : base(objectID)
        {
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
                                if (Stage == 0)
                                {
                                    missile = CreateProjectile(1210, Libraries.Monsters[(ushort)Monster.KunLun14], true, 3, 50, 7);
                                    if (missile.Target != null)
                                    {
                                        missile.Complete += (o, e) =>
                                        {
                                            missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1370, 10, 1500, missile.Target));
                                        };

                                    }
                                }
                                else
                                {
                                    missile = CreateProjectile(1690, Libraries.Monsters[(ushort)Monster.KunLun14], true, 6, 50, -6);
                                    if (missile.Target != null)
                                    {
                                        missile.Complete += (o, e) =>
                                        {
                                            missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1700, 10, 1500, missile.Target));
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

            if (Stage == 0)
            {
                NormalStage(action);
            }
            else
            {
                CrayzeStage(action);
            }
            return true;
        }

        protected override void DrawBlendEffects()
        {
            switch (CurrentAction)
            {
                case MirAction.AttackRange3:
                    Libraries.Monsters[(ushort)Monster.Bear].DrawBlend(1430 + 20 * (int)Direction + FrameIndex, DrawLocation, Color.White, true);
                    break;
            }
        }

        private void NormalStage(QueuedAction action)
        {
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    break;

                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange2:

                    break;

                case MirAction.Attack2:
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1430 + 20 * (int)Direction, 20, 20 * Frame.Interval, this, 0, true));
                    Stage = 1;
                    Frames = FrameSet.Monsters[243 + Stage];
                    break;
            }
        }

        private void CrayzeStage(QueuedAction action)
        {
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    break;

                case MirAction.AttackRange1:
                    TargetID = (uint)action.Params[0];
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1600 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1720 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1800, 15, 15 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1820, 15, 15 * Frame.Interval, this, CMain.Time + 9 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1860 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1980 + 10 * (int)Direction, 10, 10 * Frame.Interval, this, 0, true));
                    break;
            }
        }

    }
}
