using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
using System.Linq;

namespace Server.MirObjects.Monsters
{
    public class SecretKnight : MonsterObject
    {
        protected internal SecretKnight(MonsterInfo info)
            : base(info)
        {
        }

        protected override void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            target.Attacked(this, damage, defence);
            Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.SecretKnight, EffectType = 0 });
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
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, defence);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.SecretKnight, EffectType = 1 });

                    if (Envir.Random.Next(5) == 0)
                        target.ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Slow, TickSpeed = 1000 }, this);
                    break;

                case 1:
                    AddBuff(new Buff { Type = BuffType.SecretKnight, Caster = this, ExpireTime = Envir.Time + 8000, Values = new int[] { }, Visible = true });
                    break;
            }
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 6);
        }

        public override void Die()
        {
            CallDragon();
            base.Die();
        }

        private void CallDragon()
        {
            MonsterObject mob = RegenMonsterByName(CurrentMap, CurrentLocation.X, CurrentLocation.Y, Settings.SecretKnight, 500);
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;


            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged)
            {
                if (Envir.Random.Next(5) == 0)
                    MoveTo(Target.CurrentLocation);
                else
                {
                    Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                    ActionTime = Envir.Time + 500;
                    AttackTime = Envir.Time + AttackSpeed;
                    RangeAttack();
                }
            }
            else
            {
                Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                if (Envir.Random.Next(3) == 0)
                {
                    bool shield = Buffs.Any(x => x.Type == BuffType.SecretKnight);
                    if (!shield)
                    {
                        RangeAttack2();
                    }
                }
                else
                {
                    Attack1();
                }
            }
        }
    }
}