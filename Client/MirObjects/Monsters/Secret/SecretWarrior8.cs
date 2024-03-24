using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class SecretWarrior8 : MonsterObject
    {
        public SecretWarrior8(uint objectID) : base(objectID)
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
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior8], 480 + (int)Direction * 2, 2, 2 * Frame.Interval, this, CMain.Time + 2 * Frame.Interval, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior8], 398 + (int)Direction * 6, 6, 6 * Frame.Interval, this, 0, true));
                    TargetID = (uint)action.Params[0];
                    break;

                case MirAction.AttackRange2:
                    //Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior8], 502, 12, 12 * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretJudge], 367, 7, 7 * Frame.Interval, this, 0, true));
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
                                missile = CreateProjectile(446, Libraries.Monsters[(ushort)Monster.SecretWarrior8], true, 5, 50, -5);
                                if (missile.Target != null)
                                {
                                    missile.Complete += (o, e) =>
                                    {
                                        if (missile.Target.CurrentAction == MirAction.Dead) return;
                                        missile.Target.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.SecretWarrior8], 451, 6, 900, missile.Target));
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
