using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MirObjects.Monsters
{
    public class FalconLord : MonsterObject
    {
        /// <summary>
        /// 'Fly' behind player(s)
        /// </summary>
        public long TeleportTime;
        /// <summary>
        /// Push player(s)
        /// </summary>
        public long WindTime;
        /// <summary>
        /// 'Flying'
        /// </summary>
        public long ImmortalTime = 0;
        
        public int PushRange = 3;

        public int TeleportRange = 9;

        public long FinalTeleportTime = 0;

        public bool IsTeleporting { get { return Envir.Time < FinalTeleportTime; } }

        public bool Immortal { get { return Envir.Time < ImmortalTime; } }

        public FalconLord(MonsterInfo info) : base(info)
        {

        }

        public override int Attacked(MonsterObject attacker, int damage, DefenceType type = DefenceType.ACAgility)
        {
            if (!Immortal)
                return base.Attacked(attacker, damage, type);
            else
                return 0;
        }

        public override int Attacked(PlayerObject attacker, int damage, DefenceType type = DefenceType.ACAgility, bool damageWeapon = true, bool isReflectDamage = false)
        {
            if (!Immortal)
                return base.Attacked(attacker, damage, type, damageWeapon);
            else
                return 0;
        }

        public override int Struck(int damage, DefenceType type = DefenceType.ACAgility)
        {
            if (!Immortal)
                return base.Struck(damage, type);
            else
                return 0;
        }


        protected override void ProcessRegen()
        {
            if (!Immortal)
                base.ProcessRegen();
        }


        public void WindAttack()
        {
            List<MapObject> objects = FindAllTargets(PushRange, Target.CurrentLocation, false);
            for (int i = 0; i < objects.Count; i++)
            {

                int damage = GetAttackPower(MinMC, MaxMC);
                DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, objects[i], damage, DefenceType.ACAgility);
                ActionList.Add(action);
                objects[i].Pushed(this, Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation), 4);
            }
            Broadcast(new ServerPackets.ObjectAttack { ObjectID = ObjectID, Location = CurrentLocation, Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation) });
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;
            if (IsTeleporting)
                return;
            if (Envir.Random.Next(20) == 0 && !Immortal)
            {
                ImmortalTime = Envir.Time + Settings.Second * Envir.Random.Next(20, 60);
                Broadcast(new ServerPackets.ObjectEffect { ObjectID = ObjectID, TargetID = ObjectID, Effect = SpellEffect.FalconShield, Time = (uint)(ImmortalTime - Envir.Time) });
            }
            List<MapObject> targetCount = FindAllTargets(TeleportRange, Target.CurrentLocation, false);
            for (int i = targetCount.Count - 1; i >= 0; i--)
                if (targetCount[i].Race != ObjectType.Player)
                    targetCount.RemoveAt(i);
            if (Envir.Random.Next(5) == 0 &&
                Envir.Time > WindTime &&
                Functions.InRange(CurrentLocation, Target.CurrentLocation, PushRange) && CanAttack)
            {
                WindTime = Envir.Time + Settings.Second * Envir.Random.Next(5, 15);
                WindAttack();
                AttackTime = Envir.Time + AttackSpeed;
                return;
            }
            else if (!Immortal &&
                !IsTeleporting &&
                Envir.Random.Next(targetCount.Count == 0 ? 20 :
                                  targetCount.Count > 0 && targetCount.Count <= 5 ? 5 :
                                  targetCount.Count > 5 && targetCount.Count <= 10 ? 15 :
                                  targetCount.Count > 10 && targetCount.Count <= 20 ? 10 : 5) == 0 &&
                Envir.Time > TeleportTime &&
                Functions.InRange(CurrentLocation, Target.CurrentLocation, TeleportRange) && CanAttack)
            {
                TeleportTime = Envir.Time + Settings.Second * Envir.Random.Next(8, 18);
                //  Do Teleporting
                List<MapObject> objects = FindAllTargets(TeleportRange, CurrentLocation, false);
                if (objects != null &&
                    objects.Count > 0)
                {
                    for (int i = objects.Count - 1; i >= 0; i--)
                        if (objects[i].Race != ObjectType.Player)
                            objects.RemoveAt(i);
                    long finalAttackTime = Envir.Time + 1000;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        
                        DelayedAction action = new DelayedAction(DelayedType.Magic, finalAttackTime, objects[i], GetAttackPower(MinMC, MaxMC), DefenceType.MACAgility, (int)0);
                        finalAttackTime += 1000;
                        ActionList.Add(action);
                    }
                    FinalTeleportTime = finalAttackTime;
                }
                AttackTime = Envir.Time + AttackSpeed;
                return;
                
            }
            else if (InAttackRange() && CanAttack)
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
    }
}
