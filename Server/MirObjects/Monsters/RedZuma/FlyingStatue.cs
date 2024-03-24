using Server.MirDatabase;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class FlyingStatue : MonsterObject
    {
        public long PoisonCastTime;

        protected virtual byte AttackRange
        {
            get
            {
                return 8;
            }
        }

        public FlyingStatue(MonsterInfo info)
            : base(info)
        {

        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }


        protected override void ProcessTarget()
        {

            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
                Attack();
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);
        }


        protected override void Attack()
        {
            if (Target == null)
                return;

            if (Envir.Time > PoisonCastTime)
            {
                int damage = GetAttackPower(MinDC, MaxDC);
                if (Target.Attacked(this, damage, DefenceType.None) > 0)
                {
                    int value = GetAttackPower(MinSC, MaxSC);
                    if (Envir.Random.Next(2) == 1)
                    {
                        Target.ApplyPoison(new Poison { Duration = Envir.Random.Next(5), Owner = this, PType = PoisonType.Frozen, TickSpeed = 2000, Value = value / 15 }, this);
                        Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Direction = Direction, Effect = SpellEffect.FlyingStatuePurple });
                    }
                    else
                    {
                        Target.ApplyPoison(new Poison { Duration = 5 + Envir.Random.Next(5), Owner = this, PType = PoisonType.Green, TickSpeed = 1000, Value = value / 15 + Envir.Random.Next(10) }, this);
                        Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Direction = Direction, Effect = SpellEffect.FlyingStatueGreen });
                    }
                }

                PoisonCastTime = Envir.Time + 8000;
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            }
            else
            {
                int damage = GetAttackPower(MinDC, MaxDC);
                if (Target.Attacked(this, damage, DefenceType.None) > 0)
                {
                    Broadcast(new S.ObjectEffect { ObjectID = Target.ObjectID, Effect = SpellEffect.FlyingStatueCast, Direction = Direction });
                    ActionTime = Envir.Time + 1000;
                    AttackTime = Envir.Time + AttackSpeed + 500;
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                }
            }
        }
    }
}
