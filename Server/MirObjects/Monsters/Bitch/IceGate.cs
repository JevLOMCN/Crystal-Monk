using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Server.MirDatabase;

namespace Server.MirObjects.Monsters
{
    public class IceGate : MonsterObject
    {
        protected override bool CanMove { get { return false; } }
        protected override bool CanAttack { get { return false; } }

        protected List<BlockingObject> BlockingObjects = new List<BlockingObject>();
        protected Point[] BlockArray;

        protected internal IceGate(MonsterInfo info)
            : base(info)
        {
            switch (info.Effect)
            {
                case 1:
                    BlockArray = new Point[]
                    {
                        new Point(-1, -1),
                        new Point(0, -1),
                        new Point(1, 1),
                        new Point(0, 1)
                    };
                    break;
            }
        }

        public override void Spawned()
        {
            base.Spawned();

            if (BlockArray == null) return;

            MonsterInfo bInfo = new MonsterInfo
            {
                HP = this.HP,
                Image = Monster.EvilMirBody,
                CanTame = false,
                CanPush = false,
                AutoRev = false
            };

            foreach (var block in BlockArray)
            {
                BlockingObject b = new BlockingObject(this, bInfo);
                BlockingObjects.Add(b);

                if (!b.Spawn(this.CurrentMap, new Point(this.CurrentLocation.X + block.X, this.CurrentLocation.Y + block.Y)))
                {
                    SMain.EnqueueDebugging(string.Format("{3} blocking mob not spawned at {0} {1}:{2}", CurrentMap.Info.FileName, block.X, block.Y, Info.Name));
                }
            }
        }

        public override void Turn(MirDirection dir) { }

        public override bool Walk(MirDirection dir) { return false; }

        protected override void ProcessRoam() { }

        public override Packet GetInfo()
        {
            return base.GetInfo();
        }

        public override int Attacked(PlayerObject attacker, int damage, DefenceType type = DefenceType.ACAgility, bool damageWeapon = true, bool isReflectDamage = false)
        {
            return base.Attacked(attacker, damage, type, damageWeapon);
        }

        public override void Die()
        {
            for (int i=0; i<BlockingObjects.Count; ++i)
            {
                if (BlockingObjects[i].Dead) continue;
                BlockingObjects[i].Die();
            }
            base.Die();
        }
    }
}
