using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun1 : MonsterObject
    {
        public bool Visible;
        public long VisibleTime;

        protected override bool CanAttack
        {
            get
            {
                return Visible && base.CanAttack;
            }
        }
        protected override bool CanMove { get { return false; } }
        public override bool Blocking
        {
            get
            {
                return Visible && base.Blocking;
            }
        }

        protected internal KunLun1(MonsterInfo info)
            : base(info)
        {
            Visible = false;
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        protected override void ProcessAI()
        {
            if (!Dead && Envir.Time > VisibleTime)
            {
                VisibleTime = Envir.Time + 2000;

                bool find = FindNearby(7);

                if (!Visible && find)
                {
                    Visible = true;
                    CellTime = Envir.Time + 500;
                    Broadcast(GetInfo());
                    Broadcast(new S.ObjectShow { ObjectID = ObjectID });
                    ActionTime = Envir.Time + 1000;
                }

                if (Visible && !find)
                {
                    Visible = false;
                    VisibleTime = Envir.Time + 3000;

                    Broadcast(new S.ObjectHide { ObjectID = ObjectID });

                    SetHP(MaxHP);
                }
            }

            base.ProcessAI();
        }


        public override void Turn(MirDirection dir)
        {
        }

        public override bool Walk(MirDirection dir) { return false; }

        public override bool IsAttackTarget(MonsterObject attacker)
        {
            return Visible && base.IsAttackTarget(attacker);
        }
        public override bool IsAttackTarget(PlayerObject attacker)
        {
            return Visible && base.IsAttackTarget(attacker);
        }

        protected override void ProcessRoam() { }

        protected override void ProcessSearch()
        {
            if (Visible)
                base.ProcessSearch();
        }

        public override Packet GetInfo()
        {
            if (!Visible) return null;

            return base.GetInfo();
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            switch (type)
            {
                case 0:
                    target.Attacked(this, damage, DefenceType.AC);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.KunLun1, EffectType = 0 });
                    break;

                case 1:
                    target.Attacked(this, damage, DefenceType.MAC);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.KunLun1, EffectType = 1 });
                    if (Envir.Random.Next(5) == 0)
                        target.ApplyPoison(new Poison { Owner = this, Duration = 50, PType = PoisonType.Green, Value = damage / 15, TickSpeed = 1000 }, this);
                    break;
            }
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged || Envir.Random.Next(4) == 0)
                RangeAttack();
            else
                Attack1();
        }
    }
}
