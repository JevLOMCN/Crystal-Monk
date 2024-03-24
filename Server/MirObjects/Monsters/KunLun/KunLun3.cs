using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun3 : MonsterObject
    {
        protected internal KunLun3(MonsterInfo info)
            : base(info)
        {
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
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.KunLun3, EffectType = 0 });
                    break;

                case 1:
                    target.Attacked(this, damage, DefenceType.MAC);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.KunLun3, EffectType = 1 });
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

            if (Envir.Random.Next(3) == 0)
                Attack1();
            else
                RangeAttack();
        }
    }
}
