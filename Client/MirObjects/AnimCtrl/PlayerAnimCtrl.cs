using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    public abstract class PlayerAnimCtrl
    {
        public MLibrary WeaponLibrary1, WeaponEffectLibrary1, WeaponLibrary2, HairLibrary, WingLibrary, MountLibrary;
        public MLibrary BodyLibrary;
        public int Armour, Hair, Weapon, WeaponEffect, WingEffect;
        public int ArmourOffSet, HairOffSet, WeaponOffSet, WingOffset, MountOffset;

        public MirGender Gender;
        public bool HumUp;
        public bool HasClassWeapon;

        public bool AltAnim;

        public int MountType;
        public bool RidingMount;
        public bool ShowMount;

        public abstract bool IsHumUpAltAnim(MirAction CurrentAction);

        public virtual void SetLibraries(MirAction CurrentAction)
        {
            #region Armours
            BodyLibrary = Armour < Libraries.CArmours.Length ? Libraries.CArmours[Armour] : Libraries.CArmours[0];
            HairLibrary = Hair < Libraries.CHair.Length ? Libraries.CHair[Hair] : null;
            #endregion

            #region Weapons

            if (Weapon >= 0)
            {
                WeaponLibrary1 = Weapon < Libraries.CWeapons.Length ? Libraries.CWeapons[Weapon] : null;
                if (WeaponEffect > 0)
                    WeaponEffectLibrary1 = WeaponEffect < Libraries.CWeaponEffect.Length ? Libraries.CWeaponEffect[WeaponEffect] : null;
                else
                    WeaponEffectLibrary1 = null;
            }
            else
            {
                WeaponLibrary1 = null;
                WeaponEffectLibrary1 = null;
                WeaponLibrary2 = null;
            }

            #endregion

            #region WingEffects
            if (WingEffect > 0 && WingEffect < 100)
            {
                WingLibrary = (WingEffect - 1) < Libraries.CHumEffect.Length ? Libraries.CHumEffect[WingEffect - 1] : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : 808;
            HairOffSet = Gender == MirGender.Male ? 0 : 808;
            WeaponOffSet = Gender == MirGender.Male ? 0 : 416;
            WingOffset = Gender == MirGender.Male ? 0 : 840;
            MountOffset = 0;
            #endregion
        }
    }
}
