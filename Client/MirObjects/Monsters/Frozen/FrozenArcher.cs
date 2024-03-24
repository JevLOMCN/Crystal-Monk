using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Client.MirGraphics;
using Client.MirScenes;

namespace Client.MirObjects.Monsters
{
    class FrozenArcher : MonsterObject
    {
        public FrozenArcher(uint objectID) : base(objectID)
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
                                missile = CreateProjectile(264, Libraries.Monsters[(ushort)Monster.FrozenArcher], true, 5, 50, 0);

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
    }
}
