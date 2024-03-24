using Server.MirDatabase;
using Server.MirEnvir;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using S = ServerPackets;

namespace Server.MirObjects
{
    public class BuffV2
    {
        protected static Envir Envir
        {
            get { return SMain.Envir; }
        }

        public uint ObjectID;
        public MapObject Caster;
        /// <summary>
        /// 施法者
        /// </summary>
        public MapObject Owner;
        public BuffType Type;
        public object[] Values;
        /// <summary>
        /// 到期时间
        /// </summary>
        public long Expire ;
        /// <summary>
        /// 时钟速度
        /// </summary>
        public long TickSpeed;
        /// <summary>
        /// 时钟时间
        /// </summary>
        public long TickTime;
        public bool Dead;

        public void Process()
        {
            if (Envir.Time <= TickTime)
                return;

            Tick();
            TickTime = Envir.Time + TickSpeed;
        }

        public virtual void Tick()
        {
            switch (Type)
            {
                case BuffType.DamageHalo:
                    DamageHalo();
                    break;
            }
        }

        public void DamageHalo()
        {
            PlayerObject player = (PlayerObject)Owner;
            int damageBase = Owner.GetAttackPower(Owner.MinSC, Owner.MaxSC);
            UserMagic magic = player.GetMagic(Spell.TianLeiZhen);
            if (magic == null)
                return;

            int value = magic.GetDamage(damageBase);
            Point location = Owner.CurrentLocation;
            Map map = Owner.CurrentMap;

            int cost = magic.Info.BaseCost + magic.Info.LevelCost * magic.Level;
            if (cost > player.MP)
                return;

            player.ChangeMP(-cost);
            player.Enqueue(new S.ObjectEffect { ObjectID = player.ObjectID, Effect = SpellEffect.TianLeiZhen});
            player.Broadcast(new S.ObjectEffect { ObjectID = player.ObjectID, Effect = SpellEffect.TianLeiZhen});
            player.LevelMagic(magic);

            for (int y = location.Y - 1; y <= location.Y + 1; y++)
            {
                if (y < 0) continue;
                if (y >= map.Height) break;

                for (int x = location.X - 1; x <= location.X + 1; x++)
                {
                    if (x < 0) continue;
                    if (x >= map.Width) break;

                    List<MapObject> cellObjects = map.GetCellObjects(x, y);

                    if (cellObjects == null) continue;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject target = cellObjects[i];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (!target.IsAttackTarget(player)) break;

                                target.Attacked(player, value, DefenceType.MAC, false);
                                break;
                        }
                    }
                }
            }
        }
    }
}
