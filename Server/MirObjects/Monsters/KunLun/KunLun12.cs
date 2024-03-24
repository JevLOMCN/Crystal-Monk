using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun12 : MonsterObject
    {
        private long SpellTime1;
        protected internal KunLun12(MonsterInfo info)
            : base(info)
        {
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.KunLun12,
                Value = damage,
                ExpireTime = Envir.Time + 10000,
                TickSpeed = 1000,
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
                case 0:
                    targets = FindAllTargets(5, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        SpwanShiled(damage, targets[i].CurrentLocation.X, targets[i].CurrentLocation.Y);
                    }
                    break;

                case 1:
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

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
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

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged && Envir.Random.Next(3) == 0 && Envir.Time > SpellTime1)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                SpellTime1 = Envir.Time + 12000;
                RangeAttack();
            }
            else
            {
                if (ranged)
                {
                    MoveTo(Target.CurrentLocation);
                }
                else
                {

                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                    Attack1();
                }
            }
        }
    }
}
