using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    class WarriorAnimCtrl : PlayerAnimCtrl
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

        public override void SetLibraries(MirAction CurrentAction)
        {
            if (!HumUp)
                base.SetLibraries(CurrentAction);
            else
                setHumUpLibraries(CurrentAction);
        }

        private void setHumUpLibraries(MirAction CurrentAction)
        {
            Armour = Armour % 100;
            AltAnim = IsHumUpAltAnim(CurrentAction);
            if (AltAnim)
            {
                BodyLibrary = Armour < Libraries.UpWarArmours.Length ? Libraries.UpWarArmours[Armour] : Libraries.UpWarArmours[0];
                HairLibrary = Hair < Libraries.UpWarHair.Length ? Libraries.UpWarHair[Hair] : null;
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
                        fixArmour = (fixArmour - 1) * 5 + 1;
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
                int Index = Weapon - Functions.UpWarriorWeaponNum * 100;

                if (Index >= 0)
                    WeaponLibrary1 = Index >= 0 && Index < Libraries.UpWarWeapons.Length ? Libraries.UpWarWeapons[Index] : null;
                else
                    WeaponLibrary1 = null;
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
                WingLibrary = (WingEffect - 1) < (AltAnim ? Libraries.UpWarHumEffect.Length : Libraries.UpCHumEffect.Length) ? (AltAnim ? Libraries.UpWarHumEffect[WingEffect - 1] : Libraries.UpCHumEffect[WingEffect - 1]) : null;
            }
            #endregion

            #region Offsets
            ArmourOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 784 : 1112;
            HairOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 784 : 1112;
            WeaponOffSet = Gender == MirGender.Male ? 0 : AltAnim ? 784 : 600;
            WingOffset = Gender == MirGender.Male ? 0 : AltAnim ? 640 : 1112;
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
