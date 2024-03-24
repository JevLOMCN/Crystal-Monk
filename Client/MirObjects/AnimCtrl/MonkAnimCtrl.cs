using Client.MirGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.MirObjects.Job
{
    class MonkAnimCtrl : PlayerAnimCtrl
    {
        public override bool IsHumUpAltAnim(MirAction CurrentAction)
        {
            return false;
        }

        bool IsAltAnim(MirAction CurrentAction)
        {
            return CurrentAction == MirAction.JumpDown;
        }

        public override void SetLibraries(MirAction CurrentAction)
        {
            if (MountType > -1 && RidingMount && ShowMount)
            {
                BodyLibrary = Libraries.CArmours[2];
            }
            else
            {
                switch (Armour)
                {
                    case 9: //heaven
                    case 10: //mir
                    case 11: //oma
                    case 29:
                        if (!IsAltAnim(CurrentAction))
                        {
                            BodyLibrary = Armour < Libraries.CArmours.Length ? Libraries.CArmours[Armour] : Libraries.CArmours[0];
                        }
                        else
                        {
                            BodyLibrary = Libraries.MonkArmours[12];
                        }
                        break;
                    case 12: 
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        // 9-12
                        int RealArmours = Armour - 3;
                        BodyLibrary = RealArmours < Libraries.MonkArmours.Length ? Libraries.MonkArmours[RealArmours] : Libraries.MonkArmours[0];
                        break;
                  
                    default:
                        BodyLibrary = Armour < Libraries.MonkArmours.Length ? Libraries.MonkArmours[Armour] : Libraries.MonkArmours[0];
                        break;
                }

            }

            if (HasClassWeapon)
            {
                int Index = Weapon - 300;

                WeaponLibrary1 = Index < Libraries.MonkWeapons.Length ? Libraries.MonkWeapons[Index] : null;
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

            WeaponLibrary2 = null;

            if (WingEffect > 0)
            {
                switch (Armour)
                {
                    case 9: //heaven
                    case 10: //mir
                    case 11: //oma
                        WingLibrary = (WingEffect - 1) < Libraries.CHumEffect.Length ? Libraries.CHumEffect[WingEffect - 1] : null;
                        break;
                    default:
                        WingLibrary = (WingEffect - 1) < Libraries.MonkHumEffects.Length ? Libraries.MonkHumEffects[WingEffect - 1] : null;
                        break;
               }
            }

        }

    }
}
