using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class KingHydrax : MonsterObject
    {
        public KingHydrax(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingHydrax], 368 + (int)Direction * 6, 6, 6 * Frame.Interval, this, CMain.Time + Frame.Interval, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingHydrax], 416, 9, 9 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingHydrax], 425 + (int)Direction * 6, 6, 6 * Frame.Interval, this, 0, true));
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.Die:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingHydrax], 543 + (int)Direction * 7, 7, 7 * Frame.Interval, this, CMain.Time + 3 * Frame.Interval, true));
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
                                missile = CreateProjectile(473, Libraries.Monsters[(ushort)Monster.KingHydrax], true, 4, 50, 0);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KingHydrax], 537, 6, 900, missile.Target));
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
