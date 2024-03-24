using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLin5 : MonsterObject
    {
        protected internal DogYoLin5(MonsterInfo info)
            : base(info)
        {
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.DogYoLinPosionCloud,
                Value = damage,
                ExpireTime = Envir.Time + 20000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = new Point(x, y),
                CurrentMap = CurrentMap,
                StartTime = Envir.Time,
            };

            CurrentMap.AddObject(ob);
            ob.Spawned();
        }

        private void SpwanShiled1(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.DogYoLinPosionRain,
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
                    SpwanShiled1(damage, CurrentLocation.X, CurrentLocation.Y);

                    targets = FindAllTargets(5, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        SpwanShiled(damage, targets[i].CurrentLocation.X, targets[i].CurrentLocation.Y);
                    }
                    break;
                case 1:
                    targets = FindAllTargets(5, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        targets[i].AddBuff(new Buff { Type = BuffType.DogYoLin5, Caster = this, ExpireTime = Envir.Time + 8000, Visible = true, TickSpeed = 1000 });
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


            if (HP < MaxHP / 2 && Envir.Random.Next(5) == 0)
            {
                ActionTime = Envir.Time + 10000;
                AttackTime = Envir.Time + 10000;
                RangeAttack();
            }
            else if (Envir.Random.Next(5) == 0)
            {
                RangeAttack2();
            }
            else
            {
                Attack1();
            }
        }
    }
}
