using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Monsters
{
    class TAssassinM2 : MonsterObject
    {
        private MLibrary HairLibrary, WeaponLibrary1, WeaponLibrary2;

        public int WeaponEffect, ArmourOffSet, HairOffSet, WeaponOffSet;

        public TAssassinM2(uint objectID)
            : base(objectID)
        {

        }

        public void SetLibrary()
        {

        }

        public override void Draw()
        {
            DrawBehindEffects(Settings.Effect);

            float oldOpacity = DXManager.Opacity;
            if (Hidden && !DXManager.Blending) DXManager.SetOpacity(0.5F);

            if (BodyLibrary == null || Frame == null) return;

            if (HairLibrary != null)
                HairLibrary.Draw(DrawFrame + HairOffSet, DrawLocation, DrawColour, true);

            if (!DXManager.Blending && Frame.Blend)
                BodyLibrary.DrawBlend(DrawFrame, DrawLocation, DrawColour, true);
            else
                BodyLibrary.Draw(DrawFrame + ArmourOffSet, DrawLocation, DrawColour, true);

            if (WeaponLibrary1 != null)
                WeaponLibrary1.Draw(DrawFrame + WeaponOffSet, DrawLocation, DrawColour, true); //original

            if (WeaponLibrary2 != null)
                WeaponLibrary2.Draw(DrawFrame + WeaponOffSet, DrawLocation, DrawColour, true);

            DXManager.SetOpacity(oldOpacity);
        }
    }
}
