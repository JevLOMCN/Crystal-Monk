using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class Mantree : MonsterObject
    {
        public Mantree(MonsterInfo info)
            : base(info)
        {

        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
            switch (type)
            {
                case 1:
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(5) == 0)
                        target.ApplyPoison(new Poison { Owner = this, Duration = 4, PType = PoisonType.Paralysis, TickSpeed = 1000 }, this);
                    break;

                case 2:
                    target.Attacked(this, damage, DefenceType.MAC);
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

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            DelayedAction action; 
            int damage = GetAttackPower(MinDC, MaxDC);
            switch (Envir.Random.Next(8))
            {
                case 0:
                case 1:
                case 2:
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                    if (damage == 0) return;

                    action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility);
                    ActionList.Add(action);
                    break;
                case 3:
                case 4:
                    RangeAttack();

                    break;
                case 5:
                case 6:
                case 7:
                    RangeAttack2();
                    break;
            }

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
            ShockTime = 0;
        }
    }
}
