using System.Collections.Generic;
using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class DogYoLinDoor : MonsterObject
    {
            public override void Turn(MirDirection dir) { }

        public override bool Walk(MirDirection dir) { return false; }

        protected override void ProcessRoam() { }

        protected override bool CanMove
        {
            get { return false; }
        }

        protected override bool CanAttack
        {
            get { return false; }
        }

        protected List<BlockingObject> BlockingObjects = new List<BlockingObject>();
        protected Point[] BlockArray;

        protected internal DogYoLinDoor(MonsterInfo info)
            : base(info)
        {
            switch (info.Effect)
            {
                case 1:
                    BlockArray = new Point[]
                    {
                        new Point(-2, -2),
                        new Point(-2, -1),
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

        public override void Die()
        {
            for (int i = 0; i < BlockingObjects.Count; ++i)
            {
                if (BlockingObjects[i].Dead) continue;
                BlockingObjects[i].Die();
            }
            base.Die();
        }
    }
}
