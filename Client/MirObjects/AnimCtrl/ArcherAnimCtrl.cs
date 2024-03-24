using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    class ArcherAnimCtrl : PlayerAnimCtrl
    {
        bool IsAltAnim(MirAction CurrentAction)
        {
            if (HasClassWeapon)
            {
                switch (CurrentAction)
                {
                    case MirAction.Walking:
                    case MirAction.Running:
                    case MirAction.AttackRange1:
                    case MirAction.AttackRange2:
                        return true;
                }
            }

            return CurrentAction == MirAction.Jump;
        }

        public override void SetLibraries(MirAction CurrentAction)
        {
            if (!HumUp)
                SetNormalLibraries(CurrentAction);
            else
                SetHumUpLibraries(CurrentAction);
        }

        public void SetNormalLibraries(MirAction CurrentAction)
        {
            AltAnim = IsAltAnim(CurrentAction);

            #region Armours
            if (AltAnim)
            {
                switch (Armour)
                {
                    case 9: //heaven
                    case 10: //mir
                    case 11: //oma
                    case 12: //spirit
                        BodyLibrary = Armour + 1 < Libraries.ARArmours.Length ? Libraries.ARArmours[Armour + 1] : Libraries.ARArmours[0];
                        break;

                    case 19:
                        BodyLibrary = Armour - 5 < Libraries.ARArmours.Length ? Libraries.ARArmours[Armour - 5] : Libraries.ARArmours[0];
                        break;

                    case 29:
                    case 30:
                        BodyLibrary = Armour - 14 < Libraries.ARArmours.Length ? Libraries.ARArmours[Armour - 14] : Libraries.ARArmours[0];
                        break;

                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                        BodyLibrary = Armour - 32 < Libraries.ARArmours.Length ? Libraries.ARArmours[Armour - 32] : Libraries.ARArmours[0];
                        break;

                    default:
                        BodyLibrary = Armour < Libraries.ARArmours.Length ? Libraries.ARArmours[Armour] : Libraries.ARArmours[0];
                        break;
                }

                HairLibrary = Hair < Libraries.ARHair.Length ? Libraries.ARHair[Hair] : null;
            }
            else
            {
                BodyLibrary = Armour < Libraries.CArmours.Length ? Libraries.CArmours[Armour] : Libraries.CArmours[0];
                HairLibrary = Hair < Libraries.CHair.Length ? Libraries.CHair[Hair] : null;
            }
            #endregion

            #region Weapons
            if (HasClassWeapon)
            {
                int Index = Weapon - 200;

                if (AltAnim)
                    WeaponLibrary2 = Index < Libraries.ARWeaponsS.Length ? Libraries.ARWeaponsS[Index] : null;
                else
                    WeaponLibrary2 = Index < Libraries.ARWeapons.Length ? Libraries.ARWeapons[Index] : null;

                WeaponLibrary1 = null;
            }
            else
            {
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
            }
            #endregion

            #region WingEffects
            if (WingEffect > 0 && WingEffect < 100)
            {
                if (AltAnim)
                    WingLibrary = (WingEffect - 1) < Libraries.ARHumEffect.Length ? Libraries.ARHumEffect[WingEffect - 1] : null;
                else
                    WingLibrary = (WingEffect - 1) < Libraries.CHumEffect.Length ? Libraries.CHumEffect[WingEffect - 1] : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 352 : 808;
            HairOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 352 : 808;
            WeaponOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 352 : 416;
            WingOffset = Gender == MirGender.Male ? 0 : AltAnim ? 352 : 840;
            MountOffset = 0;
            #endregion
        }

        private void SetHumUpLibraries(MirAction CurrentAction)
        {
            Armour = Armour % 100;
            AltAnim = IsHumUpAltAnim(CurrentAction);

            #region Armours
            if (AltAnim)
            {
                BodyLibrary = Armour < Libraries.UpArcArmours.Length ? Libraries.UpArcArmours[Armour] : Libraries.UpArcArmours[0];
                HairLibrary = Hair < Libraries.UpArcHair.Length ? Libraries.UpArcHair[Hair] : null;

            }
            else
            {
                int fixArmour = Armour;
                switch (Armour)
                {
                    case 0:
                        fixArmour = 0;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        fixArmour = (fixArmour - 1) * 5 + 5;
                        break;
                    default:
                        fixArmour = fixArmour + 16;
                        break;
                }

                BodyLibrary = fixArmour < Libraries.UpCArmours.Length ? Libraries.UpCArmours[fixArmour] : Libraries.UpCArmours[0];
                HairLibrary = Hair < Libraries.UpCHair.Length ? Libraries.UpCHair[Hair] : null;

            }
            #endregion

            #region Weapons
            if (HasClassWeapon)
            {
                int Index = Weapon - Functions.UpArcherWeaponNum * 100;

                if (Index >= 0)
                {
                    if (AltAnim)
                        WeaponLibrary2 = Index < Libraries.UpArcWeapons.Length ? Libraries.UpArcWeapons[Index] : Libraries.UpArcWeapons[0];
                    else
                        WeaponLibrary2 = Index < Libraries.UpArcWeaponsS.Length ? Libraries.UpArcWeaponsS[Index] : Libraries.UpArcWeaponsS[0];
                }
                else
                    WeaponLibrary2 = null;

                WeaponLibrary1 = null;
            }
            else
            {
                if (Weapon > 0)
                    WeaponLibrary1 = Weapon < Libraries.UpCWeapons.Length ? Libraries.UpCWeapons[Weapon] : null;
                else
                    WeaponLibrary1 = null;

                WeaponLibrary2 = null;
            }
            #endregion

            #region WingEffects
            if (WingEffect > 0 && WingEffect < 100)
            {
                WingLibrary = (WingEffect - 1) < (AltAnim ? Libraries.UpArcHumEffect.Length : Libraries.UpCHumEffect.Length) ? (AltAnim ? Libraries.UpArcHumEffect[WingEffect - 1] : Libraries.UpCHumEffect[WingEffect - 1]) : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 384 : 1112;
            HairOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 384 : 1112;
            WeaponOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 384 : 600;
            WingOffset = Gender == MirGender.Male ? 0 : AltAnim ? 384 : 1112;
            MountOffset = 0;
            #endregion
        }

        public override bool IsHumUpAltAnim(MirAction CurrentAction)
        {
            if (HasClassWeapon)
            {
                switch (CurrentAction)
                {
                    case MirAction.Walking:
                    case MirAction.Running:
                    case MirAction.AttackRange1:
                    case MirAction.AttackRange2:
                    case MirAction.AttackRange3:
                    case MirAction.Jump:
                        return true;
                }
            }
            return false;
        }
    }
}
