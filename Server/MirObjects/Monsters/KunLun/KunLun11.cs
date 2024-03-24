using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun11 : MonsterObject
    {
        protected internal KunLun11(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.KunLun11,
                Value = damage,
                ExpireTime = Envir.Time + 1500,
                TickSpeed = 500,
                Caster = this,
                CurrentLocation = new Point(x, y),
                CurrentMap = CurrentMap,
                StartTime = Envir.Time,
            };

            CurrentMap.AddObject(ob);
            ob.Spawned();
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            int type = (int)data[3];

            List<MapObject> targets;
            switch (type)
            {
                case 1:
                    targets = FindAllTargets(5, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        SpwanShiled(damage, targets[i].CurrentLocation.X, targets[i].CurrentLocation.Y);
                    }
                    break;

                case 2:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 50, PType = PoisonType.Green, Value = damage / 15, TickSpeed = 1000 }, this);
                    }
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
            if (ranged)
            {
                if (Envir.Random.Next(3) == 0)
                    RangeAttack2();
                else if (Envir.Random.Next(3) == 0)
                    RangeAttack();
            }
            else
                Attack1();
        }
    }
}
