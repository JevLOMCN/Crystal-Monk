using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class TucsonGeneral : MonsterObject
    {
        public TucsonGeneral(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TucsonGeneral], 504 + (int)Direction * 5, 5, 5 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TucsonGeneral], 544, 8, 8 * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange3:
                    Effect ef = new Effect(Libraries.Monsters[(ushort)Monster.TucsonGeneral], 745, 17, 17 * Frame.Interval, this, 0, true);
                    ef.OffsetLocation = Functions.PointMove(new Point(0, 0), Direction, 1);
                    Effects.Add(ef);
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
                                missile = CreateProjectile(592, Libraries.Monsters[(ushort)Monster.TucsonGeneral], true, 9, 50, 0);

                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        if (missile.Target.CurrentAction == MirAction.Dead) return;
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.TucsonGeneral], 736, 9, 150 * 9, missile.Target));
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
