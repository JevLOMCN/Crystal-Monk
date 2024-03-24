using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Client.MirGraphics;
using Client.MirScenes;
using S = ServerPackets;
using Client.MirScenes.Dialogs;

namespace Client.MirObjects
{
    class SpellObject : MapObject
    {
        public MLibrary BodyLibrary;

        public override ObjectType Race
        {
            get { return ObjectType.Spell; }
        }

        public override bool Blocking
        {
            get { return false; }
        }

        public Spell Spell;
        public int FrameCount, FrameInterval, FrameIndex;
        public bool Repeat;
        public float Rate = 1F;

        public PlayerObject owner;

        public SpellObject(uint objectID) : base(objectID)
        {
        }

        public void Load(S.ObjectSpell info)
        {
            CurrentLocation = info.Location;
            MapLocation = info.Location;
            GameScene.Scene.MapControl.AddObject(this);
            Spell = info.Spell;
            Direction = info.Direction;
            Repeat = true;

            switch (Spell)
            {
                case Spell.TrapHexagon:
                    BodyLibrary = Libraries.Magic;
                    DrawFrame = 1390;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    break;
                case Spell.FireWall:
                    BodyLibrary = Libraries.Magic;
                    DrawFrame = 1630;
                    FrameInterval = 120;
                    FrameCount = 6;
                    Light = 3;
                    Blend = true;
                    break;
                case Spell.HealingCircle:
                case Spell.HealingCircle2:
                    BodyLibrary = Libraries.Magic3;
                    DrawFrame = 630;
                    FrameInterval = 120;
                    FrameCount = 10;
                    Rate = 1F;
                    Light = 3;
                    Blend = true;
                    break;
                case Spell.PoisonCloud:
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 1650;
                    FrameInterval = 120;
                    FrameCount = 20;
                    Light = 3;
                    Blend = true;
                    break;
                case Spell.DigOutZombie:
                    BodyLibrary = (ushort)Monster.DigOutZombie < Libraries.Monsters.Count() ? Libraries.Monsters[(ushort)Monster.DigOutZombie] : Libraries.Magic;
                    DrawFrame = 304 + (byte) Direction;
                    FrameCount = 0;
                    Blend = false;
                    break;
                case Spell.Blizzard:
                    CurrentLocation.Y = Math.Max(0, CurrentLocation.Y - 20);
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 1550;
                    FrameInterval = 100;
                    FrameCount = 30;
                    Light = 3;
                    Blend = true;
                    Repeat = false;
                    break;
                case Spell.MeteorStrike:
                    Point location = new Point(CurrentLocation.X, CurrentLocation.Y - 20);
                    Effects.Add(new Effect(Libraries.Magic2, 1610, 30, 3000, location, 0) { Light = 3, Blend = true, Rate = 1.0F});
                    Effects.Add(new Effect(Libraries.Magic2, 1610, 30, 3000, location, CMain.Time + 1200) { Light = 3, Blend = true, Rate = 1.0F });
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 1600;
                    FrameInterval = 80;
                    FrameCount = 10;
                    Rate = 1.0F;
                    Light = 3;
                    Repeat = false;
                    break;

                case Spell.Rubble:
                    if (Direction == 0)
                        BodyLibrary = null;
                    else
                    {
                        BodyLibrary = Libraries.Effect;
                        DrawFrame = 64 + Math.Min(4, (int)(Direction - 1));
                        FrameCount = 1;
                        FrameInterval = 10000;
                    }
                    break;
                case Spell.Reincarnation:
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 1680;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Light = 1;
                    Blend = true;
                    Repeat = true;
                    break;
                case Spell.ExplosiveTrap:
                    BodyLibrary = Libraries.Magic3;
                    DrawFrame = 1560;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Repeat = true;
                    //Light = 1;
                    Blend = true;
                    break;
                case Spell.Trap:
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 2360;
                    FrameInterval = 100;
                    FrameCount = 8;
                    Blend = true;
                    break;
                case Spell.MapLightning:
                    MapControl.Effects.Add(new Effect(Libraries.Dragon, 400 + (CMain.Random.Next(3) * 10), 5, 600, CurrentLocation));
                    MirSounds.SoundManager.PlaySound(8301);
                    break;
                case Spell.MapLava:
                    MapControl.Effects.Add(new Effect(Libraries.Dragon, 440, 20, 1600, CurrentLocation) { Blend = false });
                    MapControl.Effects.Add(new Effect(Libraries.Dragon, 470, 10, 800, CurrentLocation));
                    MirSounds.SoundManager.PlaySound(8302);
                    break;
                case Spell.MapQuake1:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HellLord], 27, 12, 1200, CurrentLocation) { Blend = false });
                    break;
                case Spell.MapQuake2:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HellLord], 39, 13, 1300, CurrentLocation) { Blend = false });
                    break;

                case Spell.StunderBall:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun5];
                    DrawFrame = 700;
                    FrameInterval = 100;
                    FrameCount = 9;
                    Blend = true;
                    
                    break;

                case Spell.MoreaKing:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.HornedCommander];
                    DrawFrame = 1190;
                    FrameInterval = 100;
                    FrameCount = 9;
                    Blend = false;
                    Repeat = false;
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1198, 1, 100, CurrentLocation, CMain.Time + 900) { Repeat = true, Blend = false });
                    break;

                case Spell.MapFire:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.ChieftainSword];
                    DrawFrame = 992;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = true;
                    break;

                case Spell.SandStorm:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.HornedSorceror];
                    DrawFrame = 634;
                    FrameInterval = 100;
                    FrameCount = 10;
                  //  Blend = true;
                    Repeat = true;
                    break;

                case Spell.BloodBoom:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.ChieftainSword];
                    DrawFrame = 1066;
                    FrameInterval = 80;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = true;
                    Rate = 1.0F;

                    break;

                case Spell.Portal:
                    BodyLibrary = Libraries.Magic2;
                    DrawFrame = 2360;
                    FrameInterval = 100;
                    FrameCount = 8;
                    Blend = true;
                    break;

                case Spell.KunLun5:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun5];
                    DrawFrame = 660;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    break;

                case Spell.KunLun5Shield:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun5];
                    DrawFrame = 690;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = false;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun5], 700, 9, 900, CurrentLocation,
                    CMain.Time + 300) { Repeat = true});
                    break;

                case Spell.KunLun11:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun11];
                    DrawFrame = 760;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = false;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun11], 760, 10, 1000, CurrentLocation, CMain.Time + 500, true));
                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun11], 770, 10, 1000, CurrentLocation, CMain.Time + 1000, true));
                    break;

                case Spell.KunLun12:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun12];
                    DrawFrame = 750;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = false;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun12], 760, 10, 1000, CurrentLocation, CMain.Time + 300, true)
                    { Repeat = true });
                    break;

                case Spell.KunLun14:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun14];
                    DrawFrame = 1390;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = false;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1400, 10, 1500, CurrentLocation, CMain.Time + 300, true)
                    { Repeat = true });
                    break;
                case Spell.KunLun141:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.KunLun14];
                    DrawFrame = 1940;
                    FrameInterval = 100;
                    FrameCount = 10;
                    Blend = true;
                    Repeat = false;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1950, 3, 300, CurrentLocation, CMain.Time + 300, true)
                    { Repeat = true });
                    break;

                case Spell.DogYoLinPosionCloud:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.DogYoLin5];
                    DrawFrame = 720;
                    FrameInterval = 100;
                    FrameCount = 7;
                    Blend = true;
                    Repeat = true;

                    break;

                case Spell.DogYoLinPosionRain:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.DogYoLin5];
                    DrawFrame = 730;
                    FrameInterval = 100;
                    FrameCount = 11;
                    Blend = true;
                    Repeat = true;

                    break;

                case Spell.DogYoLin6:
                    BodyLibrary = Libraries.Monsters[(ushort)Monster.DogYoLin6];
                    DrawFrame = 900;
                    FrameInterval = 100;
                    FrameCount = 5;
                    Blend = false;
                    Repeat = true;

                    Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.DogYoLin6], 920, 5, 500, this, 0, true){ Repeat = true  });
                    break;

            }


            NextMotion = CMain.Time + FrameInterval;
            NextMotion -= NextMotion % 100;
        }

        public override void Remove()
        {
            switch (Spell)
            {
                case Spell.KunLun5Shield:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun5], 710, 10, 1000, CurrentLocation));
                    break;
                case Spell.KunLun12:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun5], 770, 4, 400, CurrentLocation));
                    break;
                case Spell.KunLun14:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1410, 10, 1000, CurrentLocation));
                    break;
                case Spell.KunLun141:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1960, 10, 1000, CurrentLocation));
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.KunLun14], 1970, 10, 1000, CurrentLocation));
                    break;
                case Spell.MoreaKing:
                    MapControl.Effects.Add(new Effect(Libraries.Monsters[(ushort)Monster.HornedCommander], 1199, 9, 900, CurrentLocation));
                    break;
            }

            base.Remove();
        }

        public override void Process()
        {
            if (CMain.Time >= NextMotion)
            {
                if (++FrameIndex >= FrameCount && Repeat)
                    FrameIndex = 0;
                NextMotion = CMain.Time + FrameInterval;
            }

            DrawLocation = new Point((CurrentLocation.X - User.Movement.X + MapControl.OffSetX) * MapControl.CellWidth, (CurrentLocation.Y - User.Movement.Y + MapControl.OffSetY) * MapControl.CellHeight);
            DrawLocation.Offset(GlobalDisplayLocationOffset);
            DrawLocation.Offset(User.OffSetMove);

            for (int i = Effects.Count - 1; i >= 0; i--)
                Effects[i].Process();
        }

        private void DrawSelf()
        {
            if (FrameIndex >= FrameCount && !Repeat)
            {
                return;
            }
            if (BodyLibrary == null)
            {
                return;
            }

            if (Blend)
                BodyLibrary.DrawBlendEx(DrawFrame + FrameIndex, DrawLocation, DrawColour, true, Rate);
            else
                BodyLibrary.Draw(DrawFrame + FrameIndex, DrawLocation, DrawColour, true);
        }

        public override void Draw()
        {
            DrawSelf();

            for (int i = Effects.Count - 1; i >= 0; i--)
                Effects[i].Draw();
        }

        public override bool MouseOver(Point p)
        {
            return false;
        }

        public override void DrawBehindEffects(bool effectsEnabled)
        {
        }

        public override void DrawEffects(bool effectsEnabled)
        { 
        }
    }
}
