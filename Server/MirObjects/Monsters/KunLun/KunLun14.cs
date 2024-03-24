using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class KunLun14 : MonsterObject
    {
        private byte Stage;
        private long SpellTime;

        protected internal KunLun14(MonsterInfo info)
            : base(info)
        {
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
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                    }
                    break;

                case 1:
                    if (Stage == 0)
                    {
                        damage *= 5;
                        SpwanShiled(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                    }
                    else
                    {
                        targets = FindAllTargets(2, CurrentLocation, false);

                        if (targets.Count != 0)
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                                targets[i].AddBuff(new Buff { Type = BuffType.KunLun14, Caster = this, ExpireTime = Envir.Time + 8000, Visible = true, TickSpeed = 1000, Values = new int[] { damage } });
                            }
                        }

                    }
                    break;

                case 2:

                    if (Stage == 0)
                    {
                        Stage = 1;
                    }
                    else
                    {
                        damage *= 8;
                        SpwanShiled1(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled1(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled1(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled1(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                        SpwanShiled1(damage, CurrentLocation.X - 5 + Envir.Random.Next(10), CurrentLocation.Y - 5 + Envir.Random.Next(10));
                    }

                    break;
            }
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 7);
        }

        private void SpwanShiled(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.KunLun14,
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

        private void SpwanShiled1(int damage, int x, int y)
        {
            var ob = new SpellObject
            {
                Spell = Spell.KunLun141,
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

        private void NormalState()
        {
            if (HP < MaxHP / 2)
            {
                ActionTime = Envir.Time + 2500;
                AttackTime = Envir.Time + 2500;
                RangeAttack3();
                return;
            }

            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged && Envir.Random.Next(3) == 0)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                RangeAttack();
            }
            else if (Envir.Random.Next(5) == 0)
            {
                ActionTime = Envir.Time + 500;
                AttackTime = Envir.Time + AttackSpeed;
                RangeAttack2();
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

            if (Stage == 0)
            {
                NormalState();
            }
            else
            {
                CrayState();
            }
        }

        private void CrayState()
        {
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
            if (ranged && Envir.Random.Next(3) == 0)
            {
                MoveTo(Target.CurrentLocation);
                return;
            }

            ActionTime = Envir.Time + 1500;
            AttackTime = Envir.Time + AttackSpeed;
            if (Envir.Random.Next(3) == 0 && Envir.Time > SpellTime)
            {
                SpellTime = Envir.Time + 10000;
                RangeAttack3();
            }
            else if (Envir.Random.Next(4) == 0)
            {
                RangeAttack2();
            }
            else
            {
                RangeAttack();
            }
        }

        public override Packet GetInfo()
        {
            return new S.ObjectMonster
            {
                ObjectID = ObjectID,
                Name = Name,
                NameColour = NameColour,
                Location = CurrentLocation,
                Image = Info.Image,
                Direction = Direction,
                Effect = Info.Effect,
                AI = Info.AI,
                Light = Info.Light,
                Dead = Dead,
                Skeleton = Harvested,
                Poison = CurrentPoison,
                Hidden = Hidden,
                ShockTime = (ShockTime > 0 ? ShockTime - Envir.Time : 0),
                BindingShotCenter = BindingShotCenter,
                ExtraByte = Stage
            };
        }
    }
}
