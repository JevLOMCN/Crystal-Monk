﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace Client.MirGraphics
{
    public static class Libraries
    {
        public static bool Loaded;
        public static int Count, Progress;

        public static readonly MLibrary
            ChrSel = new MLibrary(Settings.DataPath + "ChrSel"),
            Prguse = new MLibrary(Settings.DataPath + "Prguse"),
            Prguse2 = new MLibrary(Settings.DataPath + "Prguse2"),
            Prguse3 = new MLibrary(Settings.DataPath + "Prguse3"),
            BuffIcon = new MLibrary(Settings.DataPath + "BuffIcon"),
            Help = new MLibrary(Settings.DataPath + "Help"),
            MiniMap = new MLibrary(Settings.DataPath + "MMap"),
            Title = new MLibrary(Settings.DataPath + "Title"),
            MagIcon = new MLibrary(Settings.DataPath + "MagIcon"),
            MagIcon2 = new MLibrary(Settings.DataPath + "MagIcon2"),
            Magic = new MLibrary(Settings.DataPath + "Magic"),
            Magic2 = new MLibrary(Settings.DataPath + "Magic2"),
            Magic3 = new MLibrary(Settings.DataPath + "Magic3"),
            Effect = new MLibrary(Settings.DataPath + "Effect"),
            MagicC = new MLibrary(Settings.DataPath + "MagicC"),
            Magic4 = new MLibrary(Settings.DataPath + "Magic4"),
            GuildSkill = new MLibrary(Settings.DataPath + "GuildSkill"),

            EdensEliteInter = new MLibrary(Settings.DataPath + "ee-inter"),
            CustomTitle = new MLibrary(Settings.DataPath + "custom-title");

        public static readonly MLibrary
            Background = new MLibrary(Settings.DataPath + "Background");



        public static readonly MLibrary
            Dragon = new MLibrary(Settings.DataPath + "Dragon");

        //Map
        public static readonly MLibrary[] MapLibs = new MLibrary[400];

        //Items
        public static readonly MLibrary
            Items = new MLibrary(Settings.DataPath + "Items"),
            StateItems = new MLibrary(Settings.DataPath + "StateItem"),
            StateItemsArcM = new MLibrary(Settings.DataPath + "StateItem_Archer_Man"),
            StateItemsArcW = new MLibrary(Settings.DataPath + "StateItem_Archer_Woman"),
            StateItemsAssM = new MLibrary(Settings.DataPath + "StateItem_Assassin_Man"),
            StateItemsAssW = new MLibrary(Settings.DataPath + "StateItem_Assassin_Woman"),
            StateItemsComM = new MLibrary(Settings.DataPath + "StateItem_Common_Man"),
            StateItemsComW = new MLibrary(Settings.DataPath + "StateItem_Common_Woman"),
            StateItemsTaoM = new MLibrary(Settings.DataPath + "StateItem_Monk_Man"),
            StateItemsTaoW = new MLibrary(Settings.DataPath + "StateItem_Monk_Woman"),
            StateItemsWarM = new MLibrary(Settings.DataPath + "StateItem_Warrior_Man"),
            StateItemsWarW = new MLibrary(Settings.DataPath + "StateItem_Warrior_Woman"),
            StateItemsWizM = new MLibrary(Settings.DataPath + "StateItem_Wizard_Man"),
            StateItemsWizW = new MLibrary(Settings.DataPath + "StateItem_Wizard_Woman"),
            FloorItems = new MLibrary(Settings.DataPath + "DNItems");

        //Deco
        public static readonly MLibrary
            Deco = new MLibrary(Settings.DataPath + "Deco");

        public static readonly MLibrary[] CArmours = new MLibrary[42],
                                          CWeapons = new MLibrary[58],
										  CWeaponEffect = new MLibrary[67],
										  CHair = new MLibrary[9],
                                          CHumEffect = new MLibrary[6],
                                          AArmours = new MLibrary[17],
                                          AWeaponsL = new MLibrary[15],
                                          AWeaponsR = new MLibrary[15],
                                          AHair = new MLibrary[9],
                                          AHumEffect = new MLibrary[3],
                                          ARArmours = new MLibrary[17],
                                          ARWeapons = new MLibrary[20],
                                          ARWeaponsS = new MLibrary[20],
                                          ARHair = new MLibrary[9],
                                          ARHumEffect = new MLibrary[3],
                                          Monsters = new MLibrary[506],
                                          Gates = new MLibrary[2],
                                          Flags = new MLibrary[12],
                                          Mounts = new MLibrary[12],
                                          NPCs = new MLibrary[200],
                                          Fishing = new MLibrary[2],
                                          Pets = new MLibrary[14],


                                          //humup //stupple
                                          UpCArmours = new MLibrary[25],
                                          UpCWeapons = new MLibrary[100],
                                          UpCHair = new MLibrary[9],
                                          UpCHumEffect = new MLibrary[3],

                                          UpWarArmours = new MLibrary[9],
                                          UpWarWeapons = new MLibrary[6],
                                          UpWarHair = new MLibrary[9],
                                          UpWarHumEffect = new MLibrary[3],

                                          UpWizArmours = new MLibrary[9],
                                          UpWizWeapons = new MLibrary[6],
                                          UpWizHair = new MLibrary[9],
                                          UpWizHumEffect = new MLibrary[3],

                                          UpTaoArmours = new MLibrary[9],
                                          UpTaoWeapons = new MLibrary[6],
                                          UpTaoHair = new MLibrary[9],
                                          UpTaoHumEffect = new MLibrary[3],

                                          UpAssArmours = new MLibrary[9],
                                          UpAssWeaponsR = new MLibrary[10],
                                          UpAssWeaponsL = new MLibrary[10],
                                          UpAssHair = new MLibrary[9],
                                          UpAssHumEffect = new MLibrary[3],

                                          UpArcArmours = new MLibrary[9],
                                          UpArcWeapons = new MLibrary[9],
                                          UpArcWeaponsS = new MLibrary[9],
                                          UpArcHair = new MLibrary[9],
                                          UpArcHumEffect = new MLibrary[3],

                                          UpMounts = new MLibrary[5],
                                          UpFishingM = new MLibrary[2],
                                          UpFishingF = new MLibrary[2],



                                          Transform = new MLibrary[28],
                                          TransformMounts = new MLibrary[28],
                                          TransformEffect = new MLibrary[2],
                                          TransformWeaponEffect = new MLibrary[1],

                                          //Monk
                                          MonkArmours = new MLibrary[14],
                                          MonkWeapons = new MLibrary[17],
                                          MonkHair = new MLibrary[0], //no monk specific hair
                                          MonkHumEffects = new MLibrary[3],
                                          MonkWeaponEffects = new MLibrary[0]; //none at the moment

        static Libraries()
        {
            //Wiz/War/Tao
            for (int i = 0; i < CArmours.Length; i++)
                CArmours[i] = new MLibrary(Settings.CArmourPath + i.ToString("00"));

            for (int i = 0; i < CHair.Length; i++)
                CHair[i] = new MLibrary(Settings.CHairPath + i.ToString("00"));

            for (int i = 0; i < CWeapons.Length; i++)
                CWeapons[i] = new MLibrary(Settings.CWeaponPath + i.ToString("00"));

			for (int i = 0; i < CWeaponEffect.Length; i++)
				CWeaponEffect[i] = new MLibrary(Settings.CWeaponEffectPath + i.ToString("00"));

			for (int i = 0; i < CHumEffect.Length; i++)
                CHumEffect[i] = new MLibrary(Settings.CHumEffectPath + i.ToString("00"));

            //Assassin
            for (int i = 0; i < AArmours.Length; i++)
                AArmours[i] = new MLibrary(Settings.AArmourPath + i.ToString("00"));

            for (int i = 0; i < AHair.Length; i++)
                AHair[i] = new MLibrary(Settings.AHairPath + i.ToString("00"));

            for (int i = 0; i < AWeaponsL.Length; i++)
                AWeaponsL[i] = new MLibrary(Settings.AWeaponPath + i.ToString("00") + " L");

            for (int i = 0; i < AWeaponsR.Length; i++)
                AWeaponsR[i] = new MLibrary(Settings.AWeaponPath + i.ToString("00") + " R");

            for (int i = 0; i < AHumEffect.Length; i++)
                AHumEffect[i] = new MLibrary(Settings.AHumEffectPath + i.ToString("00"));

            //Archer
            for (int i = 0; i < ARArmours.Length; i++)
                ARArmours[i] = new MLibrary(Settings.ARArmourPath + i.ToString("00"));

            for (int i = 0; i < ARHair.Length; i++)
                ARHair[i] = new MLibrary(Settings.ARHairPath + i.ToString("00"));

            for (int i = 0; i < ARWeapons.Length; i++)
                ARWeapons[i] = new MLibrary(Settings.ARWeaponPath + i.ToString("00"));

            for (int i = 0; i < ARWeaponsS.Length; i++)
                ARWeaponsS[i] = new MLibrary(Settings.ARWeaponPath + i.ToString("00") + " S");

            for (int i = 0; i < ARHumEffect.Length; i++)
                ARHumEffect[i] = new MLibrary(Settings.ARHumEffectPath + i.ToString("00"));
            //HumUp started   //stupple
            for (int i = 0; i < UpCArmours.Length; i++)
                UpCArmours[i] = new MLibrary(Settings.UpCArmourPath + i.ToString("00"));

            for (int i = 0; i < UpCWeapons.Length; i++)
                UpCWeapons[i] = new MLibrary(Settings.UpCWeaponPath + i.ToString("00"));

            for (int i = 0; i < UpCHair.Length; i++)
                UpCHair[i] = new MLibrary(Settings.UpCHairPath + i.ToString("00"));

            for (int i = 0; i < UpCHumEffect.Length; i++)
                UpCHumEffect[i] = new MLibrary(Settings.UpCHumEffectPath + i.ToString("00"));


            for (int i = 0; i < UpWarArmours.Length; i++)
                UpWarArmours[i] = new MLibrary(Settings.UpWarArmourPath + i.ToString("00"));

            for (int i = 0; i < UpWarWeapons.Length; i++)
                UpWarWeapons[i] = new MLibrary(Settings.UpWarWeaponPath + i.ToString("00"));

            for (int i = 0; i < UpWarHair.Length; i++)
                UpWarHair[i] = new MLibrary(Settings.UpWarHairPath + i.ToString("00"));

            for (int i = 0; i < UpWarHumEffect.Length; i++)
                UpWarHumEffect[i] = new MLibrary(Settings.UpWarHumEffectPath + i.ToString("00"));


            for (int i = 0; i < UpWizArmours.Length; i++)
                UpWizArmours[i] = new MLibrary(Settings.UpWizArmourPath + i.ToString("00"));

            for (int i = 0; i < UpWizWeapons.Length; i++)
                UpWizWeapons[i] = new MLibrary(Settings.UpWizWeaponPath + i.ToString("00"));

            for (int i = 0; i < UpWizHair.Length; i++)
                UpWizHair[i] = new MLibrary(Settings.UpWizHairPath + i.ToString("00"));

            for (int i = 0; i < UpWizHumEffect.Length; i++)
                UpWizHumEffect[i] = new MLibrary(Settings.UpWizHumEffectPath + i.ToString("00"));


            for (int i = 0; i < UpTaoArmours.Length; i++)
                UpTaoArmours[i] = new MLibrary(Settings.UpTaoArmourPath + i.ToString("00"));

            for (int i = 0; i < UpTaoWeapons.Length; i++)
                UpTaoWeapons[i] = new MLibrary(Settings.UpTaoWeaponPath + i.ToString("00"));

            for (int i = 0; i < UpTaoHair.Length; i++)
                UpTaoHair[i] = new MLibrary(Settings.UpTaoHairPath + i.ToString("00"));

            for (int i = 0; i < UpTaoHumEffect.Length; i++)
                UpTaoHumEffect[i] = new MLibrary(Settings.UpTaoHumEffectPath + i.ToString("00"));


            for (int i = 0; i < UpAssArmours.Length; i++)
                UpAssArmours[i] = new MLibrary(Settings.UpAssArmourPath + i.ToString("00"));

            for (int i = 0; i < UpAssWeaponsR.Length; i++)
                UpAssWeaponsR[i] = new MLibrary(Settings.UpAssWeaponRPath + i.ToString("00"));

            for (int i = 0; i < UpAssWeaponsL.Length; i++)
                UpAssWeaponsL[i] = new MLibrary(Settings.UpAssWeaponLPath + i.ToString("00"));

            for (int i = 0; i < UpAssHair.Length; i++)
                UpAssHair[i] = new MLibrary(Settings.UpAssHairPath + i.ToString("00"));

            for (int i = 0; i < UpAssHumEffect.Length; i++)
                UpAssHumEffect[i] = new MLibrary(Settings.UpAssHumEffectPath + i.ToString("00"));


            for (int i = 0; i < UpArcArmours.Length; i++)
                UpArcArmours[i] = new MLibrary(Settings.UpArcArmourPath + i.ToString("00"));

            for (int i = 0; i < UpArcWeapons.Length; i++)
                UpArcWeapons[i] = new MLibrary(Settings.UpArcWeaponPath + i.ToString("00"));

            for (int i = 0; i < UpArcWeaponsS.Length; i++)
                UpArcWeaponsS[i] = new MLibrary(Settings.UpArcWeaponSPath + i.ToString("00"));

            for (int i = 0; i < UpArcHair.Length; i++)
                UpArcHair[i] = new MLibrary(Settings.UpArcHairPath + i.ToString("00"));

            for (int i = 0; i < UpArcHumEffect.Length; i++)
                UpArcHumEffect[i] = new MLibrary(Settings.UpArcHumEffectPath + i.ToString("00"));

            for (int i = 0; i < UpMounts.Length; i++)
                UpMounts[i] = new MLibrary(Settings.UpMountPath + i.ToString("00"));

            for (int i = 0; i < UpFishingM.Length; i++)
                UpFishingM[i] = new MLibrary(Settings.UpFishingPath + i.ToString("00M"));

            for (int i = 0; i < UpFishingF.Length; i++)
                UpFishingF[i] = new MLibrary(Settings.UpFishingPath + i.ToString("00F"));
            ///Hum End //stupple 
            //Other
            for (int i = 0; i < Monsters.Length; i++)
                Monsters[i] = new MLibrary(Settings.MonsterPath + i.ToString("000"));

            for (int i = 0; i < Gates.Length; i++)
                Gates[i] = new MLibrary(Settings.GatePath + i.ToString("00"));

            for (int i = 0; i < Flags.Length; i++)
                Flags[i] = new MLibrary(Settings.FlagPath + i.ToString("00"));

            for (int i = 0; i < NPCs.Length; i++)
                NPCs[i] = new MLibrary(Settings.NPCPath + i.ToString("00"));

            for (int i = 0; i < Mounts.Length; i++)
                Mounts[i] = new MLibrary(Settings.MountPath + i.ToString("00"));

            for (int i = 0; i < Fishing.Length; i++)
                Fishing[i] = new MLibrary(Settings.FishingPath + i.ToString("00"));

            for (int i = 0; i < Pets.Length; i++)
                Pets[i] = new MLibrary(Settings.PetsPath + i.ToString("00"));

            for (int i = 0; i < Transform.Length; i++)
                Transform[i] = new MLibrary(Settings.TransformPath + i.ToString("00"));

            for (int i = 0; i < TransformMounts.Length; i++)
                TransformMounts[i] = new MLibrary(Settings.TransformMountsPath + i.ToString("00"));

            for (int i = 0; i < TransformEffect.Length; i++)
                TransformEffect[i] = new MLibrary(Settings.TransformEffectPath + i.ToString("00"));

            for (int i = 0; i < TransformWeaponEffect.Length; i++)
                TransformWeaponEffect[i] = new MLibrary(Settings.TransformWeaponEffectPath + i.ToString("00"));


            //Monk
            for (int i = 0; i < MonkArmours.Length; i++)
                MonkArmours[i] = new MLibrary(Settings.MonkArmourPath + i.ToString("00"));

            for (int i = 0; i < MonkWeapons.Length; i++)
                MonkWeapons[i] = new MLibrary(Settings.MonkWeaponPath + i.ToString("00"));

            for (int i = 0; i < MonkHair.Length; i++)
                MonkHair[i] = new MLibrary(Settings.MonkHairPath + i.ToString("00"));

            for (int i = 0; i < MonkHumEffects.Length; i++)
                MonkHumEffects[i] = new MLibrary(Settings.MonkHumEffectPath + i.ToString("00"));

            for (int i = 0; i < MonkWeaponEffects.Length; i++)
                MonkWeaponEffects[i] = new MLibrary(Settings.MonkWeaponEffectPath + i.ToString("00"));

            #region Maplibs
            //wemade mir2 (allowed from 0-99)
            MapLibs[0] = new MLibrary(Settings.DataPath + "Map\\WemadeMir2\\Tiles");
            MapLibs[1] = new MLibrary(Settings.DataPath + "Map\\WemadeMir2\\Smtiles");
            MapLibs[2] = new MLibrary(Settings.DataPath + "Map\\WemadeMir2\\Objects");
            for (int i = 2; i < 27; i++)
            {
                MapLibs[i + 1] = new MLibrary(Settings.DataPath + "Map\\WemadeMir2\\Objects" + i.ToString());
            }
            //shanda mir2 (allowed from 100-199)
            MapLibs[100] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\Tiles");
            for (int i = 1; i < 10; i++)
            {
                MapLibs[100 + i] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\Tiles" + (i + 1));
            }
            MapLibs[110] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\SmTiles");
            for (int i = 1; i < 10; i++)
            {
                MapLibs[110 + i] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\SmTiles" + (i + 1));
            }
            MapLibs[120] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\Objects");
            for (int i = 1; i < 31; i++)
            {
                MapLibs[120 + i] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\Objects" + (i + 1));
            }
            MapLibs[190] = new MLibrary(Settings.DataPath + "Map\\ShandaMir2\\AniTiles1");
            //wemade mir3 (allowed from 200-299)
            string[] Mapstate = { "", "wood\\", "sand\\", "snow\\", "forest\\"};
            for (int i = 0; i < Mapstate.Length; i++)
            {
                MapLibs[200 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Tilesc");
                MapLibs[201 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Tiles30c");
                MapLibs[202 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Tiles5c");
                MapLibs[203 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Smtilesc");
                MapLibs[204 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Housesc");
                MapLibs[205 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Cliffsc");
                MapLibs[206 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Dungeonsc");
                MapLibs[207 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Innersc");
                MapLibs[208 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Furnituresc");
                MapLibs[209 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Wallsc");
                MapLibs[210 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "smObjectsc");
                MapLibs[211 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Animationsc");
                MapLibs[212 +(i*15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Object1c");
                MapLibs[213 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\WemadeMir3\\" + Mapstate[i] + "Object2c");
            }
            Mapstate = new string[] { "", "wood", "sand", "snow", "forest"};
            //shanda mir3 (allowed from 300-399)
            for (int i = 0; i < Mapstate.Length; i++)
            {
                MapLibs[300 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Tilesc" + Mapstate[i]);
                MapLibs[301 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Tiles30c" + Mapstate[i]);
                MapLibs[302 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Tiles5c" + Mapstate[i]);
                MapLibs[303 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Smtilesc" + Mapstate[i]);
                MapLibs[304 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Housesc" + Mapstate[i]);
                MapLibs[305 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Cliffsc" + Mapstate[i]);
                MapLibs[306 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Dungeonsc" + Mapstate[i]);
                MapLibs[307 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Innersc" + Mapstate[i]);
                MapLibs[308 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Furnituresc" + Mapstate[i]);
                MapLibs[309 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Wallsc" + Mapstate[i]);
                MapLibs[310 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "smObjectsc" + Mapstate[i]);
                MapLibs[311 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Animationsc" + Mapstate[i]);
                MapLibs[312 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Object1c" + Mapstate[i]);
                MapLibs[313 + (i * 15)] = new MLibrary(Settings.DataPath + "Map\\ShandaMir3\\" + "Object2c" + Mapstate[i]);
            }
            #endregion

            LoadLibraries();

            Thread thread = new Thread(LoadGameLibraries) { IsBackground = true };
            thread.Start();
        }

        static void LoadLibraries()
        {
            ChrSel.Initialize();
            Progress++;

            Prguse.Initialize();
            Progress++;

            Prguse2.Initialize();
            Progress++;

            Prguse3.Initialize();
            Progress++;

            Title.Initialize();
            Progress++;

            EdensEliteInter.Initialize();
            Progress++;

            CustomTitle.Initialize();
            Progress++;
        }

        private static void LoadGameLibraries()
        {
            Count = MapLibs.Length + Monsters.Length + Gates.Length + NPCs.Length + CArmours.Length +
                CHair.Length + CWeapons.Length + CWeaponEffect.Length + AArmours.Length + AHair.Length + AWeaponsL.Length + AWeaponsR.Length +
                ARArmours.Length + ARHair.Length + ARWeapons.Length + ARWeaponsS.Length +
                CHumEffect.Length + AHumEffect.Length + ARHumEffect.Length + Mounts.Length + Fishing.Length + Pets.Length +
                Transform.Length + TransformMounts.Length + TransformEffect.Length + TransformWeaponEffect.Length +
                MonkArmours.Length + MonkHair.Length + MonkHumEffects.Length + MonkWeaponEffects.Length + MonkWeapons.Length + 
                17;

            Dragon.Initialize();
            Progress++;

            BuffIcon.Initialize();
            Progress++;

            Help.Initialize();
            Progress++;

            MiniMap.Initialize();
            Progress++;

            MagIcon.Initialize();
            Progress++;
            MagIcon2.Initialize();
            Progress++;

            Magic.Initialize();
            Progress++;
            Magic2.Initialize();
            Progress++;
            Magic3.Initialize();
            Progress++;
            MagicC.Initialize();
            Progress++;

            Effect.Initialize();
            Progress++;

            GuildSkill.Initialize();
            Progress++;

            Background.Initialize();
            Progress++;

            Deco.Initialize();
            Progress++;

            Items.Initialize();
            Progress++;
            StateItems.Initialize();
            Progress++;
            FloorItems.Initialize();
            Progress++;

            for (int i = 0; i < MapLibs.Length; i++)
            {
                if (MapLibs[i] == null)
                    MapLibs[i] = new MLibrary("");
                else
                    MapLibs[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < Monsters.Length; i++)
            {
                Monsters[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < Gates.Length; i++)
            {
                Gates[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < NPCs.Length; i++)
            {
                NPCs[i].Initialize();
                Progress++;
            }


            for (int i = 0; i < CArmours.Length; i++)
            {
                CArmours[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < CHair.Length; i++)
            {
                CHair[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < CWeapons.Length; i++)
            {
                CWeapons[i].Initialize();
                Progress++;
            }

			for (int i = 0; i < CWeaponEffect.Length; i++)
			{
				CWeaponEffect[i].Initialize();
				Progress++;
			}

			for (int i = 0; i < AArmours.Length; i++)
            {
                AArmours[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < AHair.Length; i++)
            {
                AHair[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < AWeaponsL.Length; i++)
            {
                AWeaponsL[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < AWeaponsR.Length; i++)
            {
                AWeaponsR[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < ARArmours.Length; i++)
            {
                ARArmours[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < ARHair.Length; i++)
            {
                ARHair[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < ARWeapons.Length; i++)
            {
                ARWeapons[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < ARWeaponsS.Length; i++)
            {
                ARWeaponsS[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < CHumEffect.Length; i++)
            {
                CHumEffect[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < AHumEffect.Length; i++)
            {
                AHumEffect[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < ARHumEffect.Length; i++)
            {
                ARHumEffect[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < Mounts.Length; i++)
            {
                Mounts[i].Initialize();
                Progress++;
            }


            for (int i = 0; i < Fishing.Length; i++)
            {
                Fishing[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < Pets.Length; i++)
            {
                Pets[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < Transform.Length; i++)
            {
                Transform[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < TransformEffect.Length; i++)
            {
                TransformEffect[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < TransformWeaponEffect.Length; i++)
            {
                TransformWeaponEffect[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < TransformMounts.Length; i++)
            {
                TransformMounts[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < MonkArmours.Length; i++)
            {
                MonkArmours[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < MonkWeapons.Length; i++)
            {
                MonkWeapons[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < MonkHair.Length; i++)
            {
                MonkHair[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < MonkWeaponEffects.Length; i++)
            {
                MonkWeaponEffects[i].Initialize();
                Progress++;
            }

            for (int i = 0; i < MonkHumEffects.Length; i++)
            {
                MonkHumEffects[i].Initialize();
                Progress++;
            }

            Magic4.Initialize();
            Progress++;

            Loaded = true;
        }

    }

    public sealed class MLibrary
    {
        private const string Extention = ".Lib";
        public const int LibVersion = 2;

        private readonly string _fileName;

        private MImage[] _images;
        private int[] _indexList;
        private int _count;
        private bool _initialized;

        private BinaryReader _reader;
        private FileStream _fStream;

        private bool _isDownloadAsync;

        public MLibrary(string filename)
        {
            _fileName = Path.ChangeExtension(filename, Extention);
        }

        public static string GetFileRealName(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            FileInfo[] fileInfos = info.Directory.GetFiles();
            for (int i = 0; i < fileInfos.Length; ++i)
            {
                if (fileInfos[i].Name.ToUpper() == info.Name.ToUpper())
                    return fileInfos[i].Name;
            }
            return null;
        }

        public static void RepairFileName(string filePath)
        {
            string realName = GetFileRealName(filePath);
            if (!filePath.EndsWith(realName))
            {
                CMain.SaveError(string.Format("file name sensitivity {0} {1}", filePath, realName));
                Directory.Move(filePath, filePath + "n");
                Directory.Move(filePath + "n", filePath);
            }
        }

        public void Initialize()
        {
            int CurrentVersion = 0;

            if (!File.Exists(_fileName))
            {
                return;
            }
            try
            {
                if (Settings.CheckFileName)
                    RepairFileName(_fileName);

                _fStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
                _reader = new BinaryReader(_fStream);
                CurrentVersion = _reader.ReadInt32();
                if (CurrentVersion != LibVersion)
                {
                    //cant use a directx based error popup cause it could be the lib file containing the interface is invalid :(
                    System.Windows.Forms.MessageBox.Show("Wrong version, expecting lib version: " + _fileName + " " + LibVersion.ToString() + " found version: " + CurrentVersion.ToString() + ".", _fileName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
                    System.Windows.Forms.Application.Exit();
                    return;
                }
                _count = _reader.ReadInt32();
                _images = new MImage[_count];
                _indexList = new int[_count];

                for (int i = 0; i < _count; i++)
                    _indexList[i] = _reader.ReadInt32();

                _initialized = true;
            }
            catch (Exception ex)
            {
                CMain.SaveError(ex.ToString());
            }
        }

        private bool CheckImage(int index)
        {
            if (!_initialized)
            {
                if (File.Exists(_fileName))
                {
                    Initialize();
                }
                else
                {
                    CMain.TaskDownload(_fileName, () =>
                    {
                        Initialize();
                    });
                }
            }

            if (_images == null || index < 0 || index >= _images.Length)
                return false;

            if (_images[index] == null)
            {
                _fStream.Position = _indexList[index];
                _images[index] = new MImage(_reader);
            }
            MImage mi = _images[index];
            if (!mi.TextureValid)
            {
                if ((mi.Width == 0) || (mi.Height == 0))
                    return false;
                _fStream.Seek(_indexList[index] + 17, SeekOrigin.Begin);
                mi.CreateTexture(_reader);
            }

            return true;
        }

        public Point GetOffSet(int index)
        {
            if (!_initialized) Initialize();

            if (_images == null || index < 0 || index >= _images.Length)
                return Point.Empty;

            if (_images[index] == null)
            {
                _fStream.Seek(_indexList[index], SeekOrigin.Begin);
                _images[index] = new MImage(_reader);
            }

            return new Point(_images[index].X, _images[index].Y);
        }
        public Size GetSize(int index)
        {
            if (!_initialized) Initialize();
            if (_images == null || index < 0 || index >= _images.Length)
                return Size.Empty;

            if (_images[index] == null)
            {
                _fStream.Seek(_indexList[index], SeekOrigin.Begin);
                _images[index] = new MImage(_reader);
            }

            return new Size(_images[index].Width, _images[index].Height);
        }
        public Size GetTrueSize(int index)
        {
            if (!_initialized)
                Initialize();

            if (_images == null || index < 0 || index >= _images.Length)
                return Size.Empty;

            if (_images[index] == null)
            {
                _fStream.Position = _indexList[index];
                _images[index] = new MImage(_reader);
            }
            MImage mi = _images[index];
            if (mi.TrueSize.IsEmpty)
            {
                if (!mi.TextureValid)
                {
                    if ((mi.Width == 0) || (mi.Height == 0))
                        return Size.Empty;

                    _fStream.Seek(_indexList[index] + 17, SeekOrigin.Begin);
                    mi.CreateTexture(_reader);
                }
                return mi.GetTrueSize();
            }
            return mi.TrueSize;
        }

        public void Draw(int index, int x, int y)
        {
            if (x >= Settings.ScreenWidth || y >= Settings.ScreenHeight)
                return;

            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (x + mi.Width < 0 || y + mi.Height < 0)
                return;

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, new PointF(x, y), Color.White);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }
        public void Draw(int index, Point point, Color colour, bool offSet = false)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, colour);

            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void Draw(int index, Point point, Color colour, bool offSet, float opacity)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            float oldOpacity = DXManager.Opacity;
            DXManager.SetOpacity(opacity);

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, colour);
            DXManager.SetOpacity(oldOpacity);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void DrawBlend(int index, Point point, Color colour, bool offSet = false, float rate = 1)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            bool oldBlend = DXManager.Blending;
            DXManager.SetBlend(true, rate);

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, colour);

            DXManager.SetBlend(oldBlend);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void DrawBlendEx(int index, Point point, Color colour, bool offSet = false, float rate = 1)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            bool oldBlend = DXManager.Blending;
            BlendMode oldBlendMode = DXManager.BlendingMode;
            float oldBlendRate = DXManager.BlendingRate;
            if (rate >= 1)
            {
                DXManager.SetBlend(true, rate, BlendMode.INVLIGHT);
            }
            else
            {
                DXManager.SetBlend(true, rate);
            }

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, colour);

            DXManager.SetBlend(oldBlend, oldBlendRate, oldBlendMode);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void Draw(int index, Rectangle section, Point point, Color colour, bool offSet)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);


            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            if (section.Right > mi.Width)
                section.Width -= section.Right - mi.Width;

            if (section.Bottom > mi.Height)
                section.Height -= section.Bottom - mi.Height;

            DXManager.Sprite.Draw2D(mi.Image, section, section.Size, point, colour);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }
        public void Draw(int index, Rectangle section, Point point, Color colour, float opacity)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];


            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            if (section.Right > mi.Width)
                section.Width -= section.Right - mi.Width;

            if (section.Bottom > mi.Height)
                section.Height -= section.Bottom - mi.Height;

            float oldOpacity = DXManager.Opacity;
            DXManager.SetOpacity(opacity);

            DXManager.Sprite.Draw2D(mi.Image, section, section.Size, point, colour);

            DXManager.SetOpacity(oldOpacity);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }
        public void Draw(int index, Point point, Size size, Color colour)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + size.Width < 0 || point.Y + size.Height < 0)
                return;

            DXManager.Sprite.Draw2D(mi.Image, new Rectangle(Point.Empty, new Size(mi.Width, mi.Height)), size, point, colour);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void DrawTinted(int index, Point point, Color colour, Color Tint, bool offSet = false)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            if (offSet) point.Offset(mi.X, mi.Y);

            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;
            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, colour);

            if (mi.HasMask)
            {
                DXManager.Sprite.Draw2D(mi.MaskImage, Point.Empty, 0, point, Tint);
            }

            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public void DrawUp(int index, int x, int y)
        {
            if (x >= Settings.ScreenWidth)
                return;

            if (!CheckImage(index))
                return;

            MImage mi = _images[index];
            y -= mi.Height;
            if (y >= Settings.ScreenHeight)
                return;
            if (x + mi.Width < 0 || y + mi.Height < 0)
                return;


            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, new PointF(x, y), Color.White);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }
        public void DrawUpBlend(int index, Point point)
        {
            if (!CheckImage(index))
                return;

            MImage mi = _images[index];

            point.Y -= mi.Height;


            if (point.X >= Settings.ScreenWidth || point.Y >= Settings.ScreenHeight || point.X + mi.Width < 0 || point.Y + mi.Height < 0)
                return;

            bool oldBlend = DXManager.Blending;
            DXManager.SetBlend(true, 1);

            DXManager.Sprite.Draw2D(mi.Image, Point.Empty, 0, point, Color.White);

            DXManager.SetBlend(oldBlend);
            mi.CleanTime = CMain.Time + Settings.CleanDelay;
        }

        public bool VisiblePixel(int index, Point point, bool accuate)
        {
            if (!CheckImage(index))
                return false;

            if (accuate)
                return _images[index].VisiblePixel(point);

            int accuracy = 2;

            for (int x = -accuracy; x <= accuracy; x++)
                for (int y = -accuracy; y <= accuracy; y++)
                    if (_images[index].VisiblePixel(new Point(point.X + x, point.Y + y)))
                        return true;

            return false;
        }

    }

    public sealed class MImage
    {
        public short Width, Height, X, Y, ShadowX, ShadowY;
        public byte Shadow;
        public int Length;

        public bool TextureValid;
        public Texture Image;
        //layer 2:
        public short MaskWidth, MaskHeight, MaskX, MaskY;
        public int MaskLength;

        public Texture MaskImage;
        public Boolean HasMask;

        public long CleanTime;
        public Size TrueSize;

        public unsafe byte* Data;


        public MImage(BinaryReader reader)
        {

            //read layer 1
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            ShadowX = reader.ReadInt16();
            ShadowY = reader.ReadInt16();
            Shadow = reader.ReadByte();
            Length = reader.ReadInt32();

            //check if there's a second layer and read it
            HasMask = ((Shadow >> 7) == 1) ? true : false;
            if (HasMask)
            {
                reader.ReadBytes(Length);
                MaskWidth = reader.ReadInt16();
                MaskHeight = reader.ReadInt16();
                MaskX = reader.ReadInt16();
                MaskY = reader.ReadInt16();
                MaskLength = reader.ReadInt32();
            }
        }

        public unsafe void CreateTexture(BinaryReader reader)
        {

            int w = Width;// + (4 - Width % 4) % 4;
            int h = Height;// + (4 - Height % 4) % 4;
            GraphicsStream stream = null;

            Image = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            stream = Image.LockRectangle(0, LockFlags.Discard);
            Data = (byte*)stream.InternalDataPointer;

            byte[] decomp = DecompressImage(reader.ReadBytes(Length));

            stream.Write(decomp, 0, decomp.Length);

            stream.Dispose();
            Image.UnlockRectangle(0);

            if (HasMask)
            {
                reader.ReadBytes(12);
                w = Width;// + (4 - Width % 4) % 4;
                h = Height;// + (4 - Height % 4) % 4;

                MaskImage = new Texture(DXManager.Device, w, h, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                stream = MaskImage.LockRectangle(0, LockFlags.Discard);

                decomp = DecompressImage(reader.ReadBytes(Length));

                stream.Write(decomp, 0, decomp.Length);

                stream.Dispose();
                MaskImage.UnlockRectangle(0);
            }

            DXManager.TextureList.Add(this);
            TextureValid = true;
            Image.Disposing += (o, e) =>
            {
                TextureValid = false;
                Image = null;
                MaskImage = null;
                Data = null;
                DXManager.TextureList.Remove(this);
            };


            CleanTime = CMain.Time + Settings.CleanDelay;
        }
        public unsafe bool VisiblePixel(Point p)
        {
            if (p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height)
                return false;

            int w = Width;

            bool result = false;
            if (Data != null)
            {
                int x = p.X;
                int y = p.Y;
                
                int index = (y * (w << 2)) + (x << 2);
                
                byte col = Data[index];

                if (col == 0) return false;
                else return true;
            }
            return result;
        }

        public Size GetTrueSize()
        {
            if (TrueSize != Size.Empty) return TrueSize;

            int l = 0, t = 0, r = Width, b = Height;

            bool visible = false;
            for (int x = 0; x < r; x++)
            {
                for (int y = 0; y < b; y++)
                {
                    if (!VisiblePixel(new Point(x, y))) continue;

                    visible = true;
                    break;
                }

                if (!visible) continue;

                l = x;
                break;
            }

            visible = false;
            for (int y = 0; y < b; y++)
            {
                for (int x = l; x < r; x++)
                {
                    if (!VisiblePixel(new Point(x, y))) continue;

                    visible = true;
                    break;

                }
                if (!visible) continue;

                t = y;
                break;
            }

            visible = false;
            for (int x = r - 1; x >= l; x--)
            {
                for (int y = 0; y < b; y++)
                {
                    if (!VisiblePixel(new Point(x, y))) continue;

                    visible = true;
                    break;
                }

                if (!visible) continue;

                r = x + 1;
                break;
            }

            visible = false;
            for (int y = b - 1; y >= t; y--)
            {
                for (int x = l; x < r; x++)
                {
                    if (!VisiblePixel(new Point(x, y))) continue;

                    visible = true;
                    break;

                }
                if (!visible) continue;

                b = y + 1;
                break;
            }

            TrueSize = Rectangle.FromLTRB(l, t, r, b).Size;

            return TrueSize;
        }

        private static byte[] DecompressImage(byte[] image)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(image), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }


}