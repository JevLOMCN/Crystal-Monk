using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects
{
    public class SpellObject : MapObject
    {
        public override ObjectType Race
        {
            get { return ObjectType.Spell; }
        }

        public override string Name { get; set; }
        public override int CurrentMapIndex { get; set; }
        public override Point CurrentLocation
        {
            get;
            set;
        }
        public override MirDirection Direction { get; set; }
        public override ushort Level { get; set; }
        public override bool Blocking
        {
            get
            {
                return false;
            }
        }

        public long TickTime, StartTime;
        public MapObject Caster;
        public int Value, TickSpeed;
        public Spell Spell;
        public Point CastLocation;
        public bool Show, Decoration;

        //ExplosiveTrap
        public bool DetonatedTrap;
        public int TrapIndex;

        //Portal
        public Map ExitMap;
        public Point ExitCoord;

        public int Range = 0;
        public int CasterChannelingId;

        public override uint Health
        {
            get { throw new NotSupportedException(); }
        }
        public override uint MaxHealth
        {
            get { throw new NotSupportedException(); }
        }

        public override void Spawned()
        {
            base.Spawned();
            ++Envir.SpellObjectCount;
        }

        public override void Process()
        {
            if (Decoration) return;

            if (Caster != null && Caster.Node == null) Caster = null;

            if (Envir.Time > ExpireTime || ((Spell == Spell.FireWall || Spell == Spell.Portal
                || Spell == Spell.ExplosiveTrap || Spell == Spell.Reincarnation || Spell == Spell.HealingCircle || Spell == Spell.HealingCircle2
                || Spell == Spell.MoonMist || Spell == Spell.MoonMist2
                ) && Caster == null) || (Spell == Spell.TrapHexagon && Target != null) || (Spell == Spell.Trap && Target != null))
            {
                if (Spell == Spell.TrapHexagon && Target != null || Spell == Spell.Trap && Target != null)
                {
                    MonsterObject ob = (MonsterObject)Target;

                    if (Envir.Time < ExpireTime && ob.ShockTime != 0) return;
                }

                if (Spell == Spell.Reincarnation && Caster != null && Caster is PlayerObject)
                {
                    PlayerObject player = (PlayerObject)Caster;
                    player.ReincarnationReady = true;
                    player.ReincarnationExpireTime = Envir.Time + 6000;
                }

                CurrentMap.RemoveObject(this);
                Despawn();
                return;
            }

            if (Spell == Spell.Reincarnation && !((PlayerObject)Caster).ActiveReincarnation)
            {
                CurrentMap.RemoveObject(this);
                Despawn();
                return;
            }

            if (CasterChannelingId != 0 && (Caster == null || Caster.Node == null || CasterChannelingId != Caster.ChannelingId))
            {
                CurrentMap.RemoveObject(this);
                Despawn();
                return;
            }

            if (Envir.Time < TickTime) return;
            TickTime = Envir.Time + TickSpeed;

            if (Envir.Time >= StartTime)
            {
                for (int i = -Range; i < Range + 1; i++)
                {
                    for (int j = -Range; j < Range + 1; j++)
                    {
                        int x = CurrentLocation.X + i;
                        int y = CurrentLocation.Y + j;
                        if (!CurrentMap.ValidPoint(x, y)) continue;
                        List<MapObject> cellObjects = CurrentMap.GetCellObjects(x, y);
                        if (cellObjects == null)
                            continue;

                        for (int k = 0; k < cellObjects.Count; k++)
                            ProcessTimeEvent(cellObjects[k]);
                    }
                }
            }

            if ((Spell == Spell.MapLava) || (Spell == Spell.MapLightning)) Value = 0;
        }

        public void ProcessTimeEvent(MapObject ob)
        {
            switch (Spell)
            {
                case Spell.FireWall:
                case Spell.KunLun5:
                case Spell.KunLun12:
                case Spell.DogYoLin6:
                case Spell.DogYoLinPosionCloud:
                case Spell.DogYoLinPosionRain:
                case Spell.MoreaKing:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;

                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;

                case Spell.KunLun11:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;

                    if (!ob.IsAttackTarget(Caster)) return;
                    int times = (int)(TickTime - StartTime) / TickSpeed;
                    ob.Attacked(Caster, times * Value, DefenceType.MAC, false);
                    break;

                case Spell.HealingCircle:
                case Spell.HealingCircle2:
                    {
                        if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster)
                            return;
                        if (ob.Dead)
                            return;
                        if (Caster != null && ob.ObjectID == Caster.ObjectID) return;

                        if (ob.IsAttackTarget(Caster))
                        {
                            if (!ob.IsAttackTarget(Caster)) return;
                            ob.Attacked(Caster, Value, DefenceType.MAC, false);
                            Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.HealingCircle2 });
                        }
                        else if (Caster.IsFriendlyTarget(ob))
                        {
                            if (ob.Health >= ob.MaxHealth) return;
                            ob.HealAmount = (ushort)Math.Min(ushort.MaxValue, ob.HealAmount + Value);
                            ob.OperateTime = 0;
                            Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.HealingCircle });
                        }
                    }
                    break;
                case Spell.Healing: //SafeZone
                    if (ob.Race != ObjectType.Player && (ob.Race != ObjectType.Monster || ob.Master == null || ob.Master.Race != ObjectType.Player)) return;
                    if (ob.Dead || ob.HealAmount != 0 || ob.PercentHealth == 100) return;

                    ob.HealAmount += 25;
                    Broadcast(new S.ObjectEffect { ObjectID = ob.ObjectID, Effect = SpellEffect.Healing });
                    break;
                case Spell.PoisonCloud:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;

                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, (int)(0.9F * Value), DefenceType.MAC, false);
                    if (!ob.Dead)
                        ob.ApplyPoison(new Poison
                        {
                            Duration = 15,
                            Owner = Caster,
                            PType = PoisonType.Green,
                            TickSpeed = 2000,
                            Value = Value / 20
                        }, Caster, false);
                    break;
                case Spell.Blizzard:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    if (!ob.Dead && Envir.Random.Next(8) == 0)
                        ob.ApplyPoison(new Poison
                        {
                            Duration = 5 + Envir.Random.Next(Caster.Freezing),
                            Owner = Caster,
                            PType = PoisonType.Slow,
                            TickSpeed = 2000,
                        }, Caster);
                    break;
                case Spell.MeteorStrike:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;

                case Spell.MapFire:
                case Spell.BloodBoom:
                case Spell.SandStorm:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;
                case Spell.MapLava:
                    if (ob is PlayerObject)
                    {
                        PlayerObject pOb = (PlayerObject)ob;
                        if (pOb.Account.AdminAccount && pOb.Observer)
                            return;
                    }
                    break;
                case Spell.MapLightning:
                    if (ob is PlayerObject)
                    {
                        PlayerObject pOb = (PlayerObject)ob;
                        if (pOb.Account.AdminAccount && pOb.Observer)
                            return;
                    }
                    break;
                case Spell.MapQuake1:
                case Spell.MapQuake2:
                    if (Value == 0) return;
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    ob.Struck(Value, DefenceType.MAC);
                    break;

                case Spell.Portal:
                    if (ob.Race != ObjectType.Player) return;
                    if (Caster != ob && (Caster == null || (Caster.GroupMembers == null) || (!Caster.GroupMembers.Contains((PlayerObject)ob)))) return;

                    if (ExitMap == null) return;

                    MirDirection dir = ob.Direction;

                    Point newExit = Functions.PointMove(ExitCoord, dir, 1);

                    if (!ExitMap.ValidPoint(newExit)) return;

                    ob.Teleport(ExitMap, newExit, false);

                    Value = Value - 1;

                    if (Value < 1)
                    {
                        ExpireTime = Envir.Time;
                        return;
                    }

                    break;
            }
        }

        public void ProcessSpell(MapObject ob)
        {
            switch (Spell)
            {
                case Spell.FireWall:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;

                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;

                case Spell.Healing: //SafeZone
                    if (ob.Race != ObjectType.Player && (ob.Race != ObjectType.Monster || ob.Master == null || ob.Master.Race != ObjectType.Player)) return;
                    if (ob.Dead || ob.HealAmount != 0 || ob.PercentHealth == 100) return;

                    ob.HealAmount += 25;
                    Broadcast(new S.ObjectEffect {ObjectID = ob.ObjectID, Effect = SpellEffect.Healing});
                    break;
                case Spell.PoisonCloud:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;              

                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, (int)(0.9F*Value), DefenceType.MAC, false);
                    if (!ob.Dead)
                        ob.ApplyPoison(new Poison
                        {
                            Duration = 15,
                            Owner = Caster,
                            PType = PoisonType.Green,
                            TickSpeed = 2000,
                            Value = Value / 20
                        }, Caster, false);
                    break;
                case Spell.Blizzard:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    if (!ob.Dead && Envir.Random.Next(8) == 0)
                        ob.ApplyPoison(new Poison
                        {
                            Duration = 5 + Envir.Random.Next(Caster.Freezing),
                            Owner = Caster,
                            PType = PoisonType.Slow,
                            TickSpeed = 2000,
                        }, Caster);
                    break;
                case Spell.MeteorStrike:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                    ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;

                case Spell.MapFire:
                case Spell.BloodBoom:
                case Spell.SandStorm:
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    if (!ob.IsAttackTarget(Caster)) return;
                  ob.Attacked(Caster, Value, DefenceType.MAC, false);
                    break;
                case Spell.MapLava:
                    if (ob is PlayerObject)
                    {
                        PlayerObject pOb = (PlayerObject)ob;
                        if (pOb.Account.AdminAccount && pOb.Observer)
                            return;
                    }
                    break;
                case Spell.MapLightning:
                    if (ob is PlayerObject)
                    {
                        PlayerObject pOb = (PlayerObject)ob;
                        if (pOb.Account.AdminAccount && pOb.Observer)
                            return;
                    }
                    break;
                case Spell.MapQuake1:
                case Spell.MapQuake2:
                    if (Value == 0) return;
                    if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) return;
                    if (ob.Dead) return;
                    ob.Struck(Value, DefenceType.MAC);
                    break;

                case Spell.Portal:
                    if (ob.Race != ObjectType.Player) return;
                    if (Caster != ob && (Caster == null || (Caster.GroupMembers == null) || (!Caster.GroupMembers.Contains((PlayerObject)ob)))) return;

                    if (ExitMap == null) return;

                    MirDirection dir = ob.Direction;

                    Point newExit = Functions.PointMove(ExitCoord, dir, 1);

                    if (!ExitMap.ValidPoint(newExit)) return;

                    ob.Teleport(ExitMap, newExit, false);

                    Value = Value - 1;

                    if(Value < 1)
                    {
                        ExpireTime = Envir.Time;
                        return;
                    }
                    
                    break;
            }
        }

        public override void SetOperateTime()
        {
            long time = Envir.Time + 2000;

            if (TickTime < time && TickTime > Envir.Time)
                time = TickTime;

            if (OwnerTime < time && OwnerTime > Envir.Time)
                time = OwnerTime;

            if (ExpireTime < time && ExpireTime > Envir.Time)
                time = ExpireTime;

            if (PKPointTime < time && PKPointTime > Envir.Time)
                time = PKPointTime;

            if (LastHitTime < time && LastHitTime > Envir.Time)
                time = LastHitTime;

            if (EXPOwnerTime < time && EXPOwnerTime > Envir.Time)
                time = EXPOwnerTime;

            if (BrownTime < time && BrownTime > Envir.Time)
                time = BrownTime;

            for (int i = 0; i < ActionList.Count; i++)
            {
                if (ActionList[i].Time >= time && ActionList[i].Time > Envir.Time) continue;
                time = ActionList[i].Time;
            }

            for (int i = 0; i < PoisonList.Count; i++)
            {
                if (PoisonList[i].TickTime >= time && PoisonList[i].TickTime > Envir.Time) continue;
                time = PoisonList[i].TickTime;
            }

            for (int i = 0; i < Buffs.Count; i++)
            {
                if (Buffs[i].ExpireTime >= time && Buffs[i].ExpireTime > Envir.Time) continue;
                time = Buffs[i].ExpireTime;
            }

            if (OperateTime <= Envir.Time || time < OperateTime)
                OperateTime = time;
        }

        public override void Process(DelayedAction action)
        {
            throw new NotSupportedException();
        }
        public override bool IsAttackTarget(PlayerObject attacker)
        {
            throw new NotSupportedException();
        }
        public override bool IsAttackTarget(MonsterObject attacker)
        {
            throw new NotSupportedException();
        }
        public override int Attacked(PlayerObject attacker, int damage, DefenceType type = DefenceType.ACAgility, bool damageWeapon = true, bool isReflectDamage = false)
        {
            throw new NotSupportedException();
        }
        public override int Attacked(MonsterObject attacker, int damage, DefenceType type = DefenceType.ACAgility)
        {
            throw new NotSupportedException();
        }

        public override int Struck(int damage, DefenceType type = DefenceType.ACAgility)
        {
            throw new NotSupportedException();
        }
        public override bool IsFriendlyTarget(PlayerObject ally)
        {
            throw new NotSupportedException();
        }
        public override bool IsFriendlyTarget(MonsterObject ally)
        {
            throw new NotSupportedException();
        }
        public override void ReceiveChat(string text, ChatType type)
        {
            throw new NotSupportedException();
        }

        public override Packet GetInfo()
        {
            switch (Spell)
            {
                case Spell.Healing:
                    return null;
                case Spell.PoisonCloud:
                case Spell.Blizzard:
                case Spell.MeteorStrike:
                case Spell.HealingCircle:
                case Spell.HealingCircle2:
                    if (!Show)
                        return null;

                    return new S.ObjectSpell
                    {
                        ObjectID = ObjectID,
                        Location = CastLocation,
                        Spell = Spell,
                        Direction = Direction
                    };
                case Spell.ExplosiveTrap:
                    if (!Show && !DetonatedTrap)
                        return null;
                    return new S.ObjectSpell
                    {
                        ObjectID = ObjectID,
                        Location = CurrentLocation,
                        Spell = Spell,
                        Direction = Direction,
                        Param = DetonatedTrap
                    };

                default:
                    return new S.ObjectSpell
                    {
                        ObjectID = ObjectID,
                        Location = CurrentLocation,
                        Spell = Spell,
                        Direction = Direction
                    };
            }

        }

        public override void ApplyPoison(Poison p, MapObject Caster = null, bool NoResist = false, bool ignoreDefence = true)
        {
            throw new NotSupportedException();
        }
        public override void Die()
        {
            throw new NotSupportedException();
        }
        public override int Pushed(MapObject pusher, MirDirection dir, int distance)
        {
            throw new NotSupportedException();
        }
        public override void SendHealth(PlayerObject player)
        {
            throw new NotSupportedException();
        }

        private void Reincarnation(PlayerObject player)
        {
            if (player != null && Caster != null && Caster.Node != null)
            {
                player.ActiveReincarnation = false;
                player.Enqueue(new S.CancelReincarnation { });
            }
        }

        public void ExplosiveTrap(PlayerObject player)
        {
            if (player != null && Caster != null)
            {
                if (!DetonatedTrap)
                {
                    if (CurrentMap.ValidPoint(CurrentLocation.X, CurrentLocation.Y))
                    {
                        var cellObjects = CurrentMap.GetCellObjects(new Point(CurrentLocation.X, CurrentLocation.Y));
                        if (cellObjects != null)
                        {
                            for (int k = 0; k < cellObjects.Count; k++)
                            {
                                MapObject ob = cellObjects[k];
                                if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster)
                                    continue;

                                if (!ob.IsAttackTarget(Caster)) continue;
                                ob.Attacked(Caster, Value, DefenceType.MAC, false);
                            }
                        }
                    }
                    DetonatedTrap = true;
                    player.ArcherTrapObjectsArray[TrapIndex] = null;
                    Broadcast(new S.MapEffect { Effect = SpellEffect.ExplosiveTrap, Location = CurrentLocation });
                    ExpireTime = 0;
                }

            }
        }

        private void Portal(PlayerObject player)
        {
            if (player == null)
                return;

            if (player.PortalObjectsArray[0] == this)
            {
                player.PortalObjectsArray[0] = null;

                if (player.PortalObjectsArray[1] != null)
                {
                    player.PortalObjectsArray[1].ExpireTime = 0;
                    player.PortalObjectsArray[1].Process();
                }
            }
            else
            {
                player.PortalObjectsArray[1] = null;
            }
        }

        public override void Despawn()
        {
            base.Despawn();
            --Envir.SpellObjectCount;

            PlayerObject player = null;
            if (Caster is PlayerObject)
            {
                player = (PlayerObject)Caster;

            }

            switch (Spell)
            {
                case Spell.Reincarnation:
                    Reincarnation(player);
                    break;

                case Spell.ExplosiveTrap:
                    ExplosiveTrap(player);
                    break;

                case Spell.Portal:
                    Portal(player);
                    break;

                case Spell.KunLun5Shield:
                    Explosive(1);
                    break;

                case Spell.KunLun14:
                case Spell.KunLun141:
                    Explosive(2);
                    break;
            }
        }

        public override void BroadcastInfo()
        {
            if ((Spell != Spell.ExplosiveTrap) || (Caster == null))
                base.BroadcastInfo();
            Packet p;
            if (CurrentMap == null) return;

            for (int i = CurrentMap.Players.Count - 1; i >= 0; i--)
            {
                PlayerObject player = CurrentMap.Players[i];
                if (Functions.InRange(CurrentLocation, player.CurrentLocation, Globals.DataRange))
                {
                    if ((Caster == null) || (player == null)) continue;
                    if ((player == Caster) || (player.IsFriendlyTarget(Caster)))
                    {
                        p = GetInfo();
                        if (p != null)
                            player.Enqueue(p);
                    }
                }
            }
        }

        private void Explosive(int area)
        {
            if (Caster != null)
                ((MonsterObject)Caster).MagicAreaAttack(area, Value, CurrentLocation);
        }
    }
}
