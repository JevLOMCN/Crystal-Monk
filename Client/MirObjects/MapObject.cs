using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirScenes;
using Client.MirSounds;
using Client.MirScenes.Dialogs;

namespace Client.MirObjects
{
    public abstract class MapObject
    {
        public static Font ChatFont = new Font(Settings.FontName, 10F);
        public static List<MirLabel> LabelList = new List<MirLabel>();
        public static List<MirLabel> DamageLabelList = new List<MirLabel>();

        public static UserObject User;
        public static MapObject MouseObject, TargetObject, MagicObject;
        public abstract ObjectType Race { get; }
        public abstract bool Blocking { get; }

        public uint ObjectID;
        public string Name = string.Empty;
        public Point CurrentLocation, MapLocation;
        public MirDirection Direction;
        public bool Dead, Hidden, SitDown, Sneaking;
        public PoisonType Poison;
        public long DeadTime;
        public ushort AI;
        public bool InTrapRock;

        public bool Blend = true;

        public uint Health, MaxHealth;

        public byte PercentHealth;
        public long HealthTime;
        public bool NoMove;

        public List<QueuedAction> ActionFeed = new List<QueuedAction>();
        public QueuedAction NextAction
        {
            get { return ActionFeed.Count > 0 ? ActionFeed[0] : null; }
        }

        public List<Effect> Effects = new List<Effect>();
        public List<BuffType> Buffs = new List<BuffType>();

        public Color DrawColour = Color.White, NameColour = Color.White, LightColour = Color.White;
        public MirLabel NameLabel, ChatLabel, GuildLabel, HealthBarLabel;
        public long ChatTime;
        public int DrawFrame, DrawWingFrame;
        public Point DrawLocation, Movement, FinalDrawLocation, OffSetMove;
        public Rectangle DisplayRectangle;
        public int Light, DrawY;
        public long NextMotion, NextMotion2;
        public MirAction CurrentAction;
        public bool SkipFrames;
        
        //Sound
        public int StruckWeapon;

        public MirLabel TempLabel;

        public List<Damage> Damages = new List<Damage>();

        public static Point GlobalDisplayOffset = new Point();

        public Effect MoreaEffect;

        protected Point GlobalDisplayLocationOffset
        {
            get { return new Point(-GlobalDisplayOffset.X, -GlobalDisplayOffset.Y); }
        }

        protected MapObject(uint objectID)
        {
            ObjectID = objectID;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != ObjectID) continue;
                ob.Remove();
            }

            MapControl.Objects.Add(this);
        }


        public virtual void Remove()
        {
            if (MouseObject == this) MouseObject = null;
            if (TargetObject == this) TargetObject = null;
            if (MagicObject == this) MagicObject = null;

            if (this == User.NextMagicObject)
                User.ClearMagic();

            MapControl.Objects.Remove(this);
            GameScene.Scene.MapControl.RemoveObject(this);

            if (ObjectID != GameScene.NPCID) return;

            GameScene.NPCID = 0;
            GameScene.Scene.NPCDialog.Hide();

            if (MoreaEffect != null)
            {
                MoreaEffect.Remove();
                MoreaEffect = null;
            }
        }

        public abstract void Process();
        public abstract void Draw();
        public abstract bool MouseOver(Point p);

        public void AddBuffEffect(BuffType type)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                if (!(Effects[i] is BuffEffect)) continue;
                if (((BuffEffect)(Effects[i])).BuffType == type) return;
            }

            PlayerObject ob = null;

            if (Race == ObjectType.Player)
            {
                ob = (PlayerObject)this;
            }

            BuffEffect effect;
            switch (type)
            {
                case BuffType.Fury:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 190, 7, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.ImmortalSkin:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 570, 5, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.SwiftFeet:
                    if (ob != null) ob.Sprint = true;
                    break;
                case BuffType.MoonLight:
                case BuffType.DarkBody:
                    if (ob != null) ob.Sneaking = true;
                    break;
                case BuffType.VampireShot:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 2110, 6, 1400, this, true, type) { Repeat = false });
                    break;
                case BuffType.PoisonShot:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 2310, 7, 1400, this, true, type) { Repeat = false });
                    break;
                case BuffType.EnergyShield:

                    Effects.Add(effect = new BuffEffect(Libraries.Magic2, 1880, 9, 900, this, true, type) { Repeat = false });
                    SoundManager.PlaySound(20000 + (ushort)Spell.EnergyShield * 10 + 0);

                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Magic2, 1900, 2, 800, this, true, type) { Repeat = true });
                    };
                    break;
                case BuffType.MagicBooster:
					Effects.Add(new BuffEffect(Libraries.Magic3, 90, 6, 1200, this, true, type) { Repeat = true });
                    break;
                case BuffType.PetEnhancer:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 230, 6, 1200, this, true, type) { Repeat = true });
                    break;
                case BuffType.Bisul:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 210, 7, 150*7, this, true, type) { Repeat = true });
                    break;
				case BuffType.GameMaster:
					Effects.Add(new BuffEffect(Libraries.CHumEffect[5], 0, 1416, 1200, this, true, type) { Repeat = true });
					break;
                case BuffType.HealingCircle2:
                    Effects.Add(effect = new BuffEffect(Libraries.Magic3, 980, 10, 1000, this, true, type) { Repeat = true });
                    break;
                case BuffType.KunLun8:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.KunLun8], 800, 10, 1000, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.KunLun8], 820, 7, 700, this, true, type) { Repeat = true });
                    };
                    break;

                case BuffType.SecretKnight:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.SecretKnight], 496, 8, 8*150, this, true, type) { Repeat = true });
                    break;

                case BuffType.KunLun14:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.KunLun14], 1830, 6, 6 * 150, this, true, type) { Repeat = true });
                    break;

                case BuffType.DogYoLin5:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin5], 750, 3, 3 * 150, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin5], 760, 7, 700, this, true, type) { Repeat = true});
                    };
                    break;

                case BuffType.DogYoLin6:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 930, 10, 10 * 150, this, true, type) { Repeat = true});
                    break;

                case BuffType.DogYoLin7:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1550, 10, 10 * 150, this, true, type) { Repeat = true});
                    break;

                case BuffType.DogYoLinShield:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1510, 3, 3 * 150, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1520, 5, 5 * 150, this, true, type) { Repeat = true });
                    };
                    break;
            }
        }
        public void RemoveBuffEffect(BuffType type)
        {
            PlayerObject ob = null;

            if (Race == ObjectType.Player)
            {
                ob = (PlayerObject)this;
            }

            for (int i = 0; i < Effects.Count; i++)
            {
                if (!(Effects[i] is BuffEffect)) continue;
                if (((BuffEffect)(Effects[i])).BuffType != type) continue;
                Effects[i].Repeat = false;
            }

            switch (type)
            {
                case BuffType.SwiftFeet:
                    if (ob != null) ob.Sprint = false;
                    break;
                case BuffType.MoonLight:
                case BuffType.DarkBody:
                    if (ob != null) ob.Sneaking = false;
                    break;
                case BuffType.HealingCircle2:
                    Effects.Add(new Effect(Libraries.Magic3, 990, 3, 300, this));
                    break;
                case BuffType.KunLun8:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun8], 810, 3, 300, this));
                    break;
                case BuffType.DogYoLin5:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin5], 770, 4, 400, this));
                    break;
                case BuffType.DogYoLinShield:
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin7], 1530, 2, 200, this));
                    break;
            }
        }

        public void Chat(string text)
        {
            if (ChatLabel != null && !ChatLabel.IsDisposed)
            {
                ChatLabel.Dispose();
                ChatLabel = null;
            }

            const int chatWidth = 200;
            List<string> chat = new List<string>();

            int index = 0;
            for (int i = 1; i < text.Length; i++)
                if (TextRenderer.MeasureText(CMain.Graphics, text.Substring(index, i - index), ChatFont).Width > chatWidth)
                {
                    chat.Add(text.Substring(index, i - index - 1));
                    index = i - 1;
                }
            chat.Add(text.Substring(index, text.Length - index));

            text = chat[0];
            for (int i = 1; i < chat.Count; i++)
                text += string.Format("\n{0}", chat[i]);

            ChatLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.White,
                OutLine = true,
                OutLineColour = Color.Black,
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Text = text,
            };
            ChatTime = CMain.Time + 5000;

        }

        public virtual void DrawHealthNum()
        {
            if (!Settings.ShowHealth)
                return;

            CreateHealthNum();

            if (HealthBarLabel == null || HealthBarLabel.IsDisposed || Dead) return;
            if (MaxHealth == 0)
                return;

            HealthBarLabel.Text = String.Format("{0}/{1}", Health, MaxHealth);
            HealthBarLabel.Location = new Point(DisplayRectangle.X + (48 - HealthBarLabel.Size.Width) / 2, DisplayRectangle.Y - (65 + HealthBarLabel.Size.Height) - (Dead ? 35 : 0));
            HealthBarLabel.Draw();
        }

        public virtual void DrawChat()
        {
            if (ChatLabel == null || ChatLabel.IsDisposed) return;

            if (CMain.Time > ChatTime)
            {
                ChatLabel.Dispose();
                ChatLabel = null;
                return;
            }

            ChatLabel.ForeColour = Dead ? Color.Gray : Color.White;
            ChatLabel.Location = new Point(DisplayRectangle.X + (48 - ChatLabel.Size.Width) / 2, DisplayRectangle.Y - (80 + ChatLabel.Size.Height) - (Dead ? 35 : 0));
            ChatLabel.Draw();
        }

        public virtual void CreateLabel()
        {
            NameLabel = null;

            for (int i = 0; i < LabelList.Count; i++)
            {
                if (LabelList[i].Text != Name || LabelList[i].ForeColour != NameColour) continue;
                NameLabel = LabelList[i];
                break;
            }


            if (NameLabel != null && !NameLabel.IsDisposed) return;

            NameLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = NameColour,
                OutLine = true,
                OutLineColour = Color.Black,
                Text = Name,
            };
            NameLabel.Disposing += (o, e) => LabelList.Remove(NameLabel);
            LabelList.Add(NameLabel);
        }

        public virtual void DrawName()
        {
            CreateLabel();

            if (NameLabel == null) return;
            
            NameLabel.Text = Name;
            NameLabel.Location = new Point(DisplayRectangle.X + (50 - NameLabel.Size.Width) / 2, DisplayRectangle.Y - (32 - NameLabel.Size.Height / 2) + (Dead ? 35 : 8)); //was 48 -
            NameLabel.Draw();
        }

        public virtual void DrawBlend()
        {
            DXManager.SetBlend(true, 0.3F); //0.8
            Draw();
            DXManager.SetBlend(false);
        }

        public void DrawDamages()
        {
            for (int i = Damages.Count - 1; i >= 0; i--)
            {
                Damage info = Damages[i];
                if (CMain.Time > info.ExpireTime)
                {
                    Damages.RemoveAt(i);
                }
                else
                {
                    info.Draw(DisplayRectangle.Location);
                }
            }
        }

        public void DrawHealth()
        {
            if (!Settings.ShowHealth)
            {
                if (CMain.Time >= HealthTime)
                {
                    if (Race == ObjectType.Monster && !Name.EndsWith(string.Format("({0})", User.Name)) && !GroupDialog.GroupList.Contains(Name)) return;
                    if (Race == ObjectType.Player && this != User && !GroupDialog.GroupList.Contains(Name)) return;
                    if (this == User && GroupDialog.GroupList.Count == 0) return;
                }
            }

            string name = Name;

            if (Name.Contains("(")) name = Name.Substring(Name.IndexOf("(") + 1, Name.Length - Name.IndexOf("(") - 2);

            if (Dead) return;
            if (Race != ObjectType.Player && Race != ObjectType.Monster) return;

            Libraries.Prguse2.Draw(0, DisplayRectangle.X + 8, DisplayRectangle.Y - 64);
            int index = 1;

            switch (Race)
            {
                case ObjectType.Player:
                    if (GroupDialog.GroupList.Contains(name)) index = 10;
                    break;
                case ObjectType.Monster:
                    if (GroupDialog.GroupList.Contains(name) || name == User.Name) index = 11;
                    break;
            }

            byte rate = PercentHealth;
            if (PercentHealth == 0 && MaxHealth == 0 && !Dead)
                rate = 100;

            Libraries.Prguse2.Draw(index, new Rectangle(0, 0, (int)(32 * rate / 100F), 4), new Point(DisplayRectangle.X + 8, DisplayRectangle.Y - 64), Color.White, false);
        }

        public void DrawPoison()
        {
            byte poisoncount = 0;
            if (Poison != PoisonType.None)
            {
                if (Poison.HasFlag(PoisonType.Green))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Green);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Red))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Red);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Bleeding))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.DarkRed);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Slow))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Purple);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Stun))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Yellow);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Frozen))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Blue);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Paralysis) || Poison.HasFlag(PoisonType.LRParalysis))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Gray);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.DelayedExplosion))
                {
                    DXManager.Sprite.Draw2D(DXManager.PoisonDotBackground, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 7 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 21)), Color.Black);
                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(DisplayRectangle.X + 8 + (poisoncount * 3)), (int)(DisplayRectangle.Y - 20)), Color.Orange);
                    poisoncount++;
                }

            }
        }

        public abstract void DrawBehindEffects(bool effectsEnabled);

        public abstract void DrawEffects(bool effectsEnabled);

        protected void CreateHealthNum()
        {
            if (HealthBarLabel != null) return;

            HealthBarLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.White,
                OutLine = true,
                OutLineColour = Color.Black,
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Text = "",
            };
        }

        public void PoisonChanged()
        {
            if (Poison.HasFlag(PoisonType.Morea))
            {
                if (MoreaEffect == null)
                {
                    MoreaEffect = new Effect(Libraries.Magic3, 4610, 8, 800, this, 0, true) { Repeat = true };
                    Effects.Add(MoreaEffect);
                }
            }
            else
            {
                if (MoreaEffect != null)
                {
                    MoreaEffect.Remove();
                    MoreaEffect = null;
                }
            }
        }

    }

}
