using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    class WizardAnimCtrl : PlayerAnimCtrl
    {
        bool IsAltAnim(MirAction CurrentAction)
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
                        return true;
                }
            }

            return false;
        }

        public int SetWeaponLibrariesIndex()
        {
            return Weapon - 3 * 100;
        }

        public override void SetLibraries(MirAction CurrentAction)
        {
            if (!HumUp)
                base.SetLibraries(CurrentAction);
            else
                SetHumUpLibraries(CurrentAction);
        }

        private void SetHumUpLibraries(MirAction CurrentAction)
        {
            Armour = Armour % 100;
            AltAnim = IsHumUpAltAnim(CurrentAction);

            #region Armours
            if (AltAnim)
            {
                BodyLibrary = Armour < Libraries.UpWizArmours.Length ? Libraries.UpWizArmours[Armour] : Libraries.UpWizArmours[0];
                HairLibrary = Hair < Libraries.UpWizHair.Length ? Libraries.UpWizHair[Hair] : null;

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
                        fixArmour = (fixArmour - 1) * 5 + 2;
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
                int Index = Weapon - Functions.UpWizardWeaponNum * 100;

                if (Index < 0)
                    WeaponLibrary1 = null;
                else
                    WeaponLibrary1 = Index < Libraries.UpWizWeapons.Length ? Libraries.UpWizWeapons[Index] : null;
                WeaponLibrary2 = null;
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
                WingLibrary = (WingEffect - 1) < (AltAnim ? Libraries.UpWizHumEffect.Length : Libraries.UpCHumEffect.Length) ? (AltAnim ? Libraries.UpWizHumEffect[WingEffect - 1] : Libraries.UpCHumEffect[WingEffect - 1]) : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 720 : 1112;
            HairOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 720 : 1112;
            WeaponOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 720 : 600;
            WingOffset = Gender == MirGender.Male ? 0 : AltAnim ? 720 : 1112;
            MountOffset = 0;
            #endregion
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
                        return true;
                }
            }
            return false;
        }
    }
}
