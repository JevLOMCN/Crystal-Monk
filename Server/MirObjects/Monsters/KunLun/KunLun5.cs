using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun5 : MonsterObject
    {
        private long SpellTime;

        protected internal KunLun5(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
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
                    SpellObject ob = new SpellObject
                    {
                        Spell = Spell.KunLun5,
                        Value = damage,
                        ExpireTime = Envir.Time + 8000,
                        TickSpeed = 1000,
                        Caster = this,
                        CurrentLocation = new Point(target.CurrentLocation.X, target.CurrentLocation.Y),
                        CurrentMap = CurrentMap,
                        StartTime = Envir.Time + 800,
                    };

                    CurrentMap.AddObject(ob);
                    ob.Spawned();
                    break;

                case 1:
                    SpwanShiled(4 * damage, CurrentLocation.X - 2, CurrentLocation.Y - 2);
                    SpwanShiled(4 * damage, CurrentLocation.X - 2, CurrentLocation.Y + 2);
                    SpwanShiled(4 * damage, CurrentLocation.X + 2, CurrentLocation.Y - 2);
                    SpwanShiled(4 * damage, CurrentLocation.X + 2, CurrentLocation.Y + 2);
                    break;
            }
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.KunLun5Shield,
                Value = damage,
                ExpireTime = Envir.Time + 8000,
                TickSpeed = 1000,
                Caster = this,
                CurrentLocation = new Point(x, y),
                CurrentMap = CurrentMap,
                StartTime = Envir.Time + 800,
            };

            CurrentMap.AddObject(ob);
            ob.Spawned();
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
            if (ranged && Envir.Random.Next(5) == 0)
            {
                RangeAttack();
                ActionTime = Envir.Time + 1800;
                AttackTime = Envir.Time + AttackSpeed;
            }
            else if (Envir.Time > SpellTime && Envir.Random.Next(3) == 0)
            {
                RangeAttack2();
                SpellTime = Envir.Time + 9000;
                ActionTime = Envir.Time + 1800;
                AttackTime = Envir.Time + AttackSpeed;
            }
            else if (!ranged)
            {
                Attack1();
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
            }
            else
                MoveTo(Target.CurrentLocation);
        }
    }
}
