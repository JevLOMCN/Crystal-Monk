using Server.MirDatabase;
using S = ServerPackets;
using System.Drawing;
using Server.MirEnvir;
using System.Collections.Generic;

namespace Server.MirObjects.Monsters
{
    public class KillerPlant : MonsterObject
    {
        protected internal KillerPlant(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return InAttackRange(7);
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

            if (HP < MaxHP / 2 && Envir.Random.Next(15) == 0)
            {
                RangeAttack4();
            }
            else
            {
                bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
                if (ranged)
                {
                    if (Envir.Random.Next(5) == 0)
                        RangeAttack3();
                    else if (Envir.Random.Next(4) == 0)
                        RangeAttack2();
                    else 
                        MoveTo(Target.CurrentLocation);
                }
                else
                {
                    if (Envir.Random.Next(5) == 0)
                        RangeAttack3();
                    else if (Envir.Random.Next(5) == 0)
                        RangeAttack2();
                    else if (Envir.Random.Next(5) == 0)
                        RangeAttack();
                    else
                        Attack1();
                }
            }
  
            ActionTime = Envir.Time + 400;
            AttackTime = Envir.Time + AttackSpeed;

        }

        private void CallDragon()
        {
            int rand = Envir.Random.Next(4);
            string name;
            switch(rand)
            {
                case 0:
                    name = Settings.KillerPlant1;
                    break;

                case 1:
                    name = Settings.KillerPlant2;
                    break;

                case 2:
                    name = Settings.KillerPlant3;
                    break;

                default:
                    name = Settings.KillerPlant4;
                    break;
            }

            MonsterObject mob = RegenMonsterByName1(name, CurrentLocation.X + 3, CurrentLocation.Y);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.KillerPlant });
            mob = RegenMonsterByName1(name, CurrentLocation.X - 3, CurrentLocation.Y);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.KillerPlant });
            mob = RegenMonsterByName1(name, CurrentLocation.X, CurrentLocation.Y + 3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.KillerPlant });
            mob = RegenMonsterByName1(name, CurrentLocation.X, CurrentLocation.Y - 3);
            Broadcast(new S.ObjectEffect { ObjectID = mob.ObjectID, Effect = SpellEffect.KillerPlant });
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
                    if (Target == null || !Target.IsAttackTarget(this)) break;
                    Target.Attacked(this, damage, DefenceType.MAC);
                    if (Envir.Random.Next(3) == 0)
                        Target.Pushed(this, Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation), Envir.Random.Next(3) + 2);

                    break;

                case 1:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(5) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Slow, Value = damage, TickSpeed = 2000 }, this);
                    }
                    break;

                case 2:
                    targets = FindAllTargets(2, CurrentLocation);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(6) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Paralysis, Value = damage, TickSpeed = 2000 }, this);
                    }
                    break;

                case 3:
                    targets = FindAllTargets(5, CurrentLocation);

                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i] == null || !targets[i].IsAttackTarget(this)) continue;
                        targets[i].Attacked(this, damage, DefenceType.MAC);
                        if (Envir.Random.Next(6) == 0)
                            targets[i].ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Stun, Value = damage, TickSpeed = 2000 }, this);
                    }
                    break;

                case 4:
                    CallDragon();
                    break;
            }

        }
    }
}
