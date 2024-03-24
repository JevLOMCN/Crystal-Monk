using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.MirDatabase;

namespace Server.MirObjects.Monsters
{
    public class BearMinotaurLrod : MonsterObject
    {
        public long PullTime;
        public long WaveAttackTime;

        public byte Stage
        {
            get
            {
                return
                    PercentHealth >= 90 ? (byte)0 :
                    PercentHealth >= 60 && PercentHealth < 90 ? (byte)1 : 
                    PercentHealth >= 40 && PercentHealth < 60 ? (byte)2 :
                    PercentHealth >= 20 && PercentHealth < 40 ? (byte)3 :
                    PercentHealth >= 10 && PercentHealth < 20 ? (byte)4 : (byte)5;
            }

        }

        public BearMinotaurLrod(MonsterInfo info) : base(info)
        {

        }

        public override void RefreshAll()
        {
            base.RefreshAll();
            switch(Stage)
            {
                case 0:
                default:
                    break;
                case 1:
                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 5);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 5);
                    MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 5);
                    MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 5);
                    MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 5);
                    MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 5);
                    MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 5);
                    MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 5);
                    MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 5);
                    MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 5);
                    Accuracy = (byte)Math.Min(ushort.MaxValue, Accuracy + 2);
                    Agility = (byte)Math.Min(ushort.MaxValue, Agility + 1);
                    AttackSpeed = Math.Max(400, AttackSpeed - 100);
                    MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 100);
                    break;
                case 2:
                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 10);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 10);
                    MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 10);
                    MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 10);
                    MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 8);
                    MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 8);
                    MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 8);
                    MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 8);
                    MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 8);
                    MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 8);
                    Accuracy = (byte)Math.Min(ushort.MaxValue, Accuracy + 2);
                    Agility = (byte)Math.Min(ushort.MaxValue, Agility + 1);
                    AttackSpeed = Math.Max(400, AttackSpeed - 200);
                    MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 200);
                    break;
                case 3:
                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 15);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 15);
                    MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 15);
                    MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 15);
                    MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 12);
                    MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 12);
                    MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 12);
                    MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 12);
                    MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 12);
                    MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 12);
                    Accuracy = (byte)Math.Min(ushort.MaxValue, Accuracy + 2);
                    Agility = (byte)Math.Min(ushort.MaxValue, Agility + 1);
                    AttackSpeed = Math.Max(400, AttackSpeed - 300);
                    MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 300);
                    break;
                case 4:
                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 10);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 10);
                    MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 10);
                    MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 10);
                    MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 20);
                    MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 20);
                    MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 20);
                    MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 20);
                    MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 20);
                    MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 20);
                    Accuracy = (byte)Math.Min(ushort.MaxValue, Accuracy + 5);
                    Agility = (byte)Math.Max(ushort.MinValue, Agility - 5);
                    AttackSpeed = Math.Max(400, AttackSpeed - 400);
                    MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 400);
                    break;
                case 5:
                    MinAC = (ushort)Math.Min(ushort.MaxValue, MinAC + 5);
                    MaxAC = (ushort)Math.Min(ushort.MaxValue, MaxAC + 5);
                    MinMAC = (ushort)Math.Min(ushort.MaxValue, MinMAC + 5);
                    MaxMAC = (ushort)Math.Min(ushort.MaxValue, MaxMAC + 5);
                    MinDC = (ushort)Math.Min(ushort.MaxValue, MinDC + 25);
                    MaxDC = (ushort)Math.Min(ushort.MaxValue, MaxDC + 25);
                    MinMC = (ushort)Math.Min(ushort.MaxValue, MinMC + 25);
                    MaxMC = (ushort)Math.Min(ushort.MaxValue, MaxMC + 25);
                    MinSC = (ushort)Math.Min(ushort.MaxValue, MinSC + 25);
                    MaxSC = (ushort)Math.Min(ushort.MaxValue, MaxSC + 25);
                    Accuracy = (byte)Math.Min(ushort.MaxValue, Accuracy + 10);
                    Agility = (byte)Math.Max(ushort.MinValue, Agility - 10);
                    AttackSpeed = Math.Max(400, AttackSpeed - 500);
                    MoveSpeed = (ushort)Math.Max(400, MoveSpeed - 500);
                    break;
            }
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;
            RefreshAll();
            if (InAttackRange() && CanAttack)
            {
                Attack();
                return;
            }
            else if (CanAttack && Envir.Time > PullTime)
            {
                PullTime = Envir.Time + Settings.Second * Envir.Random.Next(5, 15);
                List<MapObject> objects = FindAllTargets(12, CurrentLocation, false);
                for (int i = 0; i < objects.Count; i++)
                {
                    MirDirection dir = Functions.DirectionFromPoint(objects[i].CurrentLocation, CurrentLocation);
                    objects[i].Pushed(this, dir, Functions.MaxDistance(CurrentLocation, objects[i].CurrentLocation));
                    objects[i].Attacked(this, GetAttackPower(MinMC, MaxMC));
                }
                Broadcast(new ServerPackets.ObjectEffect { Direction = Direction, TargetID = ObjectID, ObjectID = ObjectID, Effect = SpellEffect.BearFlameSpin });
                Broadcast(new ServerPackets.ObjectAttack { Direction = Direction, Location = CurrentLocation, ObjectID = ObjectID });
                AttackTime = Envir.Time + AttackSpeed;
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
