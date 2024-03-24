using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class MirKing : FlamingWooma
    {
        private long _teleTime, _madTime/*, _FireTime*/, _CrazyWalk;
        private const long TeleDelay = 10000, FireDelay = 20000;
        private byte _stage = 7;

        protected internal MirKing(MonsterInfo info)
            : base(info)
        {
        }

        protected override void ProcessAI()
        {
            if (Dead) return;

            if (_madTime > 0 && Envir.Time > _madTime)
            {
                _madTime = 0;
                RefreshAll();
            }


            if (Envir.Time > _teleTime)
            {
                _teleTime = Envir.Time + TeleDelay;


                if (Envir.Random.Next(2) == 1 && Target != null && Target.IsAttackTarget(this) && Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) > 4)
                {
                    Target = null;
                    TeleportRandom(10, 0);
                }
            }

            if (_madTime > Envir.Time && Envir.Time > _CrazyWalk && Target != null) // TODO && InAttackRange(2))
            {
                _CrazyWalk = Envir.Time + 750;
                MirDirection dir = Direction;

                if (Envir.Random.Next(100) > 50)
                    dir = Functions.NextDir(dir);
                else
                    dir = Functions.PreviousDir(dir);

                Walk(dir);

            }


            if (MaxHP >= 7)
            {
                byte stage = (byte)(HP / (MaxHP / 7));

                if (stage < _stage)
                {
                    _madTime = Envir.Time + 10000;
                    MoveSpeed = 400;
                    AttackSpeed = 500;
                }
                _stage = stage;
            }


            base.ProcessAI();
        }
    }
}
