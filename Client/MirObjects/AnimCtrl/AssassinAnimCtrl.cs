using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    class AssassinAnimCtrl : PlayerAnimCtrl
    {
        bool IsAltAnim(MirAction CurrentAction)
        {
            if (HasClassWeapon || Weapon < 0)
            {
                 switch (CurrentAction)
                {
                    case MirAction.Standing:
                    case MirAction.Stance:
                    case MirAction.Walking:
                    case MirAction.Running:
                    case MirAction.Die:
                    case MirAction.Struck:
                    case MirAction.Attack1:
                    case MirAction.Attack2:
                    case MirAction.Attack3:
                    case MirAction.Attack4:
                    case MirAction.Sneek:
                    case MirAction.Spell:
                    case MirAction.DashAttack:
                        return true;
                }
            }

            return CurrentAction == MirAction.Jump;
        }

        public override bool IsHumUpAltAnim(MirAction CurrentAction)
        {
            if (HasClassWeapon)
            {
                switch (CurrentAction)
                {
                    case MirAction.Standing:
                    case MirAction.Walking:
                    case MirAction.Running:
                    case MirAction.Stance:
                    case MirAction.Stance2:
                    case MirAction.Attack1:
                    case MirAction.Attack2:
                    case MirAction.Attack3:
                    case MirAction.Attack4:
                    case MirAction.Spell:
                    case MirAction.Harvest:
                    case MirAction.Struck:
                    case MirAction.Die:
                    case MirAction.Dead:
                    case MirAction.Revive:
                    case MirAction.DashL:
                    case MirAction.DashR:
                    case MirAction.DashAttack:
                    case MirAction.Sneek:
                        return true;
                }
            }
            return false;
        }

        public override void SetLibraries(MirAction CurrentAction)
        {
            if (!HumUp)
                SetNormalLibraries(CurrentAction);
            else
                SetHumUpLibraries(CurrentAction);
        }

        public int SetWeaponLibrariesIndex()
        {
            return Weapon - 100;
        }

        private void SetHumUpLibraries(MirAction CurrentAction)
        {
            Armour = Armour % 100;
            AltAnim = IsHumUpAltAnim(CurrentAction);

            if (AltAnim)
            {
                BodyLibrary = Armour < Libraries.UpAssArmours.Length ? Libraries.UpAssArmours[Armour] : Libraries.UpAssArmours[0];
                HairLibrary = Hair < Libraries.UpAssHair.Length ? Libraries.UpAssHair[Hair] : null;

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
                        fixArmour = (fixArmour - 1) * 5 + 4;
                        break;
                    default:
                        fixArmour = fixArmour + 16;
                        break;
                }

                BodyLibrary = fixArmour < Libraries.UpCArmours.Length ? Libraries.UpCArmours[fixArmour] : Libraries.UpCArmours[0];
                HairLibrary = Hair < Libraries.UpCHair.Length ? Libraries.UpCHair[Hair] : null;

            }

            #region Weapons
            if (HasClassWeapon)
            {
                int Index = Weapon - Functions.UpAssassinWeaponNum * 100;
                if (Index < 0)
                {
                    WeaponLibrary1 = null;
                    WeaponLibrary2 = null;
                }
                else
                {
                    WeaponLibrary1 = Index < Libraries.UpAssWeaponsR.Length ? Libraries.UpAssWeaponsR[Index] : null;
                    WeaponLibrary2 = Index < Libraries.UpAssWeaponsL.Length ? Libraries.UpAssWeaponsL[Index] : null;
                }
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
                WingLibrary = (WingEffect - 1) < (AltAnim ? Libraries.UpAssHumEffect.Length : Libraries.UpCHumEffect.Length) ? (AltAnim ? Libraries.UpAssHumEffect[WingEffect - 1] : Libraries.UpCHumEffect[WingEffect - 1]) : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 912 : 1112;
            HairOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 912 : 1112;
            WeaponOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 912 : 600;
            WingOffset = Gender == MirGender.Male ? 0 : AltAnim ? 912 : 1112;
            MountOffset = 0;
            #endregion
        }

        private void SetNormalLibraries(MirAction CurrentAction)
        {
            bool altAnim = IsAltAnim(CurrentAction);

            if (altAnim)
            {
                switch (Armour)
                {
                    case 9: //heaven
                    case 10: //mir
                    case 11: //oma
                        BodyLibrary = Armour + 3 < Libraries.AArmours.Length ? Libraries.AArmours[Armour + 3] : Libraries.AArmours[0];
                        break;

                    case 12: //spirit
                        BodyLibrary = Armour + 4 < Libraries.AArmours.Length ? Libraries.AArmours[Armour + 4] : Libraries.AArmours[0];
                        break;

                    case 19:
                        BodyLibrary = Armour - 3 < Libraries.AArmours.Length ? Libraries.AArmours[Armour - 3] : Libraries.AArmours[0];
                        break;

                    case 20:
                    case 21:
                    case 22:
                    case 23: //red bone
                    case 24:
                        BodyLibrary = Armour - 17 < Libraries.AArmours.Length ? Libraries.AArmours[Armour - 17] : Libraries.AArmours[0];
                        break;

                    case 28:
                    case 29:
                    case 30:
                        BodyLibrary = Armour - 20 < Libraries.AArmours.Length ? Libraries.AArmours[Armour - 20] : Libraries.AArmours[0];
                        break;

                    case 34:
                        BodyLibrary = Armour - 23 < Libraries.AArmours.Length ? Libraries.AArmours[Armour - 23] : Libraries.AArmours[0];
                        break;

                    default:
                        BodyLibrary = Armour < Libraries.AArmours.Length ? Libraries.AArmours[Armour] : Libraries.AArmours[0];
                        break;
                }

                HairLibrary = Hair < Libraries.AHair.Length ? Libraries.AHair[Hair] : null;
            }
            else
            {
                BodyLibrary = Armour < Libraries.CArmours.Length ? Libraries.CArmours[Armour] : Libraries.CArmours[0];
                HairLibrary = Hair < Libraries.CHair.Length ? Libraries.CHair[Hair] : null;
            }

            #region Weapons
            if (HasClassWeapon)
            {
                int Index = Weapon - 100;

                WeaponLibrary1 = Index < Libraries.AWeaponsL.Length ? Libraries.AWeaponsR[Index] : null;
                WeaponLibrary2 = Index < Libraries.AWeaponsR.Length ? Libraries.AWeaponsL[Index] : null;
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
                if (altAnim)
                    WingLibrary = (WingEffect - 1) < Libraries.AHumEffect.Length ? Libraries.AHumEffect[WingEffect - 1] : null;
                else
                    WingLibrary = (WingEffect - 1) < Libraries.CHumEffect.Length ? Libraries.CHumEffect[WingEffect - 1] : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : altAnim ? 512 : 808;
            HairOffSet = Gender == MirGender.Male ? 0 : altAnim ? 512 : 808;
            WeaponOffSet = Gender == MirGender.Male ? 0 : altAnim ? 512 : 416;
            WingOffset = Gender == MirGender.Male ? 0 : altAnim ? 544 : 1112;
            MountOffset = 0;
            #endregion
        }
    }
}
