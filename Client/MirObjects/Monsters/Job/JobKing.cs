using Client.MirGraphics;
using Client.MirScenes;
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Client.MirSounds;

namespace Client.MirObjects
{

//天雨散花 1175 12
//光球  1170 5
//飞行  1122 6
//日闪  1082 5
//攻击  1002 8
//大爆炸 1066 10
//吸气   1076 5

// 攻击 1348 5

    class JobKing : MonsterObject
    {
        private long BombTime;
        private bool Bomb;

        public JobKing(uint objectID)
            : base(objectID)
        {
        }

        public override bool CalcActorFrame(QueuedAction action)
        {
            FrameIntervals.Clear();
            MirAction frameAction = CurrentAction;
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    PlaySound(4342);
                    break;

                case MirAction.AttackRange1:
                    PlaySound(4346);
                    break;

                case MirAction.AttackRange2:
                    PlaySound(4347);
                    break;

                case MirAction.AttackRange3:
                    FrameIntervals.Add(7, 6000);
                    PlaySound(4348);
                    break;

                case MirAction.AttackRange4:
                    PlaySound(4356);
                    break;

                case MirAction.AttackRange5:
                    PlaySound(4357);
                    break;

                case MirAction.AttackRange6:
                    PlaySound(4358);
                    break;

                case MirAction.AttackRange7:
                    PlaySound(4359);
                    break;

                case MirAction.AttackRange8:
                case MirAction.AttackRange9:
                    frameAction = MirAction.Attack1;
                    break;
            }

            Frames.Frames.TryGetValue(frameAction, out Frame);
            FrameIndex = 0;

            if (Frame == null)
            {
                CMain.SaveError(string.Format("{0} Frame not found ", CurrentAction));
                return false;
            }

            FrameInterval = Frame.Interval;

                    Effect eff;
            switch (CurrentAction)
            {
                case MirAction.Attack1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 752 + (int)Direction * 7, 7, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange1:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 808 + (int)Direction * 7, 7, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange2:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 864 + (int)Direction * 10, 10, Frame.Count * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 944 + (int)Direction * 10, 10, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange3:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1002 + (int)Direction * 8, 8, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange4:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1082 + (int)Direction * 5, 5, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange5:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1175, 12, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange6:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1122 + (int)Direction * 6, 6, Frame.Count * Frame.Interval, this, 0, true));
                    break;

                case MirAction.AttackRange7:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1187, 9, Frame.Count * Frame.Interval, this, 0, true));
                    Effects.Add(eff = new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1196 + (int)Direction * 9, 9, Frame.Count * Frame.Interval, this, 0, true));
                    eff.Complete += (o, e) => { Remove(); };
                    break;

                case MirAction.AttackRange8:
                    Hidden = true;
                    Effects.Add(eff = new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1268 + (int)Direction * 8, 8, Frame.Count * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1348 + (int)Direction * 5, 5, Frame.Count * Frame.Interval, this, 0, true));
                    eff.Complete += (o, e) => { Remove(); };
                    break;

                case MirAction.AttackRange9:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1268 + (int)Direction * 8, 8, Frame.Count * Frame.Interval, this, 0, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1348 + (int)Direction * 5, 5, Frame.Count * Frame.Interval, this, 0, true));

                    break;
            }


            return true;
        }


        protected override bool AfterFrameFinish()
        {
            if (CurrentAction == MirAction.AttackRange7)
            {
                Remove();
                return true;
            }

            return false;
        }
    }
}
