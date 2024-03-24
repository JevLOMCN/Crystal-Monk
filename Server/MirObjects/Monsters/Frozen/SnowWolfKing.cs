using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    class SnowWolfKing : MonsterObject
    {
        public SnowWolfKing(MonsterInfo info) : base(info)
        {
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            if (HP < MaxHP * 0.8 && Envir.Random.Next(15) == 0)
                CallDragon();
            else if (Envir.Random.Next(8) == 0)
            {
                List<MapObject> targets = FindAllTargets(10, CurrentLocation);
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                    TeleportOut(targets[i].Back, true, 8);

                    ActionTime = Envir.Time + 3500;
                    AttackTime = Envir.Time + 3500;
                    return;
                }
            }
            else if (Envir.Random.Next(15) == 0)
                RangeAttack3();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack2();
            else if (Envir.Random.Next(5) == 0)
                RangeAttack();
            else
                Attack1();

            ActionTime = Envir.Time + 800;
            AttackTime = Envir.Time + AttackSpeed;
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
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this) || targets[i].CurrentMap != CurrentMap || targets[i].Node == null) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 7, PType = PoisonType.Slow, Value = damage, TickSpeed = 1000 }, this);
                        }
                    }
                    break;
                case 1:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(5) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 3 + Envir.Random.Next(50), PType = PoisonType.Green, Value = damage / 20, TickSpeed = 1000 }, this);
                    }
                    break;
                case 2:
                    if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;
                    target.Attacked(this, damage, DefenceType.MAC);
                    Broadcast(new S.ObjectEffect { ObjectID = target.ObjectID, Effect = SpellEffect.SnowWolfKing, EffectType = 1 });
                    if (Envir.Random.Next(6) == 0)
                    {
                        target.ApplyPoison(new Poison { Owner = this, Duration = 3 + Envir.Random.Next(5), PType = PoisonType.Paralysis, Value = damage, TickSpeed = 1000 }, this);
                    }
                    break;
            }
        }

        private void CallDragon()
        {
            MonsterObject mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X + 3, CurrentLocation.Y + 3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.SnowWolfKing });

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X + 3, CurrentLocation.Y - 3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.SnowWolfKing });

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X - 3, CurrentLocation.Y + 3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.SnowWolfKing });

            mob = RegenMonsterByName1(Settings.SnowWolfKing, CurrentLocation.X - 3, CurrentLocation.Y -3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.SnowWolfKing });
        }
    }
}
