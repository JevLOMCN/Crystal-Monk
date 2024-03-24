using Client.MirScenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MirObjects
{
    public class Buff
    {
        public BuffType Type;
        public string Caster;
        public bool Visible;
        public uint ObjectID;
        public long Expire;
        public int[] Values;
        public bool Infinite;

        public override string ToString()
        {
            string text = string.Empty;

            switch (Type)
            {
                //magic
                case BuffType.TemporalFlux:
                    text = string.Format(CMain.Tr("Temporal Flux\nIncreases cost of next Teleport by: {0} MP.\n"), (int)(MapObject.User.MaxMP * 0.3F));
                    break;
                case BuffType.Hiding:
                    text = CMain.Tr("Hiding\nInvisible to many monsters.\n");
                    break;
                case BuffType.Haste:
                    text = string.Format(CMain.Tr("Haste\nIncreases Attack Speed by: {0}.\n"), Values[0]);
                    break;
                case BuffType.SwiftFeet:
                    text = string.Format(CMain.Tr("Swift Feet\nIncreases Move Speed by: {0}.\n"), Values[0]);
                    break;
                case BuffType.Fury:
                    text = string.Format(CMain.Tr("Fury\nIncreases Attack Speed by: {0}.\n"), Values[0]);
                    break;
                case BuffType.LightBody:
                    text = string.Format(CMain.Tr("Light Body\nIncreases Agility by: {0}.\n"), Values[0]);
                    break;
                case BuffType.SoulShield:
                    text = string.Format(CMain.Tr("Soul Shield\nIncreases MAC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.BlessedArmour:
                    text = string.Format(CMain.Tr("Blessed Armour\nIncreases AC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.ProtectionField:
                    text = string.Format(CMain.Tr("Protection Field\nIncreases AC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.ProtectionField1:
                    text = string.Format(CMain.Tr("Protection Field1\nIncreases AC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.Rage:
                    text = string.Format(CMain.Tr("Rage\nIncreases DC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.ImmortalSkin:
                    text = string.Format(CMain.Tr("ImmortalSkin\nDamage Reduce by: {0}%.\nDecreases DC by: {1}%.\n"), Values[0], Values[1]);
                    break;
                case BuffType.StormEscape:
                    text = CMain.Tr("Increase next magic damage.\n");
                    break;
                case BuffType.CounterAttack:
                    text = string.Format(CMain.Tr("Counter Attack\nIncreases Reflect by: {0}.\n"), Values[0]);
                    break;
                case BuffType.CounterAttack1:
                    text = string.Format(CMain.Tr("Counter Attack\nIncreases AC/MAC by: {0}-{1}.\n"), Values[0], Values[0]);
                    break;
                case BuffType.UltimateEnhancer:
                    if (GameScene.User.Class == MirClass.Wizard || GameScene.User.Class == MirClass.Archer)
                    {
                        text = string.Format(CMain.Tr("Ultimate Enhancer\nIncreases MC by: 0-{0}.\n"), Values[0]);
                    }
                    else if (GameScene.User.Class == MirClass.Taoist)
                    {
                        text = string.Format(CMain.Tr("Ultimate Enhancer\nIncreases SC by: 0-{0}.\n"), Values[0]);
                    }
                    else
                    {
                        text = string.Format(CMain.Tr("Ultimate Enhancer\nIncreases DC by: 0-{0}.\n"), Values[0]);
                    }
                    break;
                case BuffType.EnergyShield:
                    text = string.Format(CMain.Tr("Energy Shield\n{0}% chance to gain {1} HP when attacked\n"), Math.Round((1 / (decimal)Values[0]) * 100), Values[1]);
                    break;
                case BuffType.Curse:
                    text = string.Format(CMain.Tr("Cursed\nDecreases DC/MC/SC/ASpeed by: {0}%.\n"), Values[0]);
                    break;
                case BuffType.MoonLight:
                    text = CMain.Tr("Moon Light\nInvisible to players and many\nmonsters when at a distance.\n");
                    break;
                case BuffType.MoonMist:
                    text = CMain.Tr("Moon Light\nInvisible to players and many\nmonsters when at a distance.\n");
                    break;
                case BuffType.DarkBody:
                    text = CMain.Tr("Dark Body\nInvisible to many monsters and able to move.\n");
                    break;
                case BuffType.VampireShot:
                    text = string.Format(CMain.Tr("Vampire Shot\nGives you a vampiric ability\nthat can be released with\ncertain skills.\n"), Values[0]);
                    break;
                case BuffType.PoisonShot:
                    text = string.Format(CMain.Tr("Poison Shot\nGives you a poison ability\nthat can be released with\ncertain skills.\n"), Values[0]);
                    break;
                case BuffType.Concentration:
                    text = CMain.Tr("Concentrating\nIncreases chance on element extraction.\n");
                    break;
                case BuffType.MentalState:
                    switch (Values[0])
                    {
                        case 0:
                            text = string.Format(CMain.Tr("Agressive (Full damage)\nCan't shoot over walls.\n"), Values[0]);
                            break;
                        case 1:
                            text = string.Format(CMain.Tr("Trick shot (Minimal damage)\nCan shoot over walls.\n"), Values[0]);
                            break;
                        case 2:
                            text = string.Format(CMain.Tr("Group Mode (Medium damage)\nDon't steal agro.\n"), Values[0]);
                            break;
                    }
                    break;
                case BuffType.MagicBooster:
                    text = string.Format(CMain.Tr("Magic Booster\nIncreases MC by: {0}-{0}.\nIncreases consumption by {1}%.\n"), Values[0], Values[1]);
                    break;
                case BuffType.MagicShield:
                    text = string.Format(CMain.Tr("Magic Shield\nReduces damage by {0}%.\n"), (Values[0] + 2) * 10);
                    break;

                //special
                case BuffType.GameMaster:
                    GMOptions options = (GMOptions)Values[0];
                    text = CMain.Tr("GameMaster\n");

                    if (options.HasFlag(GMOptions.GameMaster)) text += CMain.Tr("-Invisible\n");
                    if (options.HasFlag(GMOptions.Superman)) text += CMain.Tr("-Superman\n");
                    if (options.HasFlag(GMOptions.Observer)) text += CMain.Tr("-Observer\n");
                    break;
                case BuffType.General:
                    text = string.Format(CMain.Tr("Mirian Advantage\nExpRate increased by {0}%\n"), Values[0]);

                    if (Values.Length > 1)
                        text += string.Format(CMain.Tr("DropRate increased by {0}%\n"), Values[1]);
                    if (Values.Length > 2)
                        text += string.Format(CMain.Tr("GoldRate increased by {0}%\n"), Values[2]);
                    break;
                case BuffType.Exp:
                    text = string.Format(CMain.Tr("Exp Rate\nIncreased by {0}%\n"), Values[0]);
                    break;
                case BuffType.Exp1:
                    text = string.Format(CMain.Tr("Exp Rate\nIncreased by {0}%\n"), Values[0]);
                    break;
                case BuffType.Gold:
                    text = string.Format(CMain.Tr("Gold Rate\nIncreased by {0}%\n"), Values[0]);
                    break;
                case BuffType.Drop:
                    text = string.Format(CMain.Tr("Drop Rate\nIncreased by {0}%\n"), Values[0]);
                    break;
                case BuffType.BagWeight:
                    text = string.Format(CMain.Tr("Bag Weight\nIncreases BagWeight by: {0}.\n"), Values[0]);
                    break;
                case BuffType.RelationshipEXP:
                    text = string.Format(CMain.Tr("Love is in the Air\nExpRate increased by: {0}%.\n"), Values[0]);
                    break;
                case BuffType.Mentee:
                    text = string.Format(CMain.Tr("In Training\nLearn skill points twice as quick.\nExpRate increased by: {0}%.\n"), Values[0]);
                    break;
                case BuffType.Bisul:
                    text = string.Format(CMain.Tr("skill no guilding\n"), Values[0]);
                    break;
                case BuffType.Mentor:
                    text = string.Format(CMain.Tr("Mentorship Empowerment\nDamage to monsters increased by {0}%.\n"), Values[0]);
                    break;
                case BuffType.Guild:
                    text = string.Format(CMain.Tr("Guild Charge\n"));
                    text += GameScene.Scene.GuildDialog.ActiveStats;
                    break;
                case BuffType.Rested:
                    text = string.Format(CMain.Tr("Rested\nIncreases Exp Rate by {0}%\n"), Values[0]);
                    break;

                //stats
                case BuffType.Impact:
                    text = string.Format(CMain.Tr("Impact\nIncreases DC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.Magic:
                    text = string.Format(CMain.Tr("Magic\nIncreases MC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.Taoist:
                    text = string.Format(CMain.Tr("Taoist\nIncreases SC by: 0-{0}.\n"), Values[0]);
                    break;
                case BuffType.Storm:
                    text = string.Format(CMain.Tr("Storm\nIncreases A.Speed by: {0}.\n"), Values[0]);
                    break;
                case BuffType.HealthAid:
                    text = string.Format(CMain.Tr("Health Aid\nIncreases HP by: {0}.\n"), Values[0]);
                    break;
                case BuffType.Healing2:
                    text = string.Format(CMain.Tr("Healing2\nIncreases HP by: {0}.\n"), Values[0]);
                    break;
                case BuffType.ManaAid:
                    text = string.Format(CMain.Tr("Mana Aid\nIncreases MP by: {0}.\n"), Values[0]);
                    break;
                case BuffType.Defence:
                    text = string.Format(CMain.Tr("Defence\nIncreases Max AC by: {0}-{0}.\n"), Values[0]);
                    break;
                case BuffType.MagicDefence:
                    text = string.Format(CMain.Tr("Magic Defence\nIncreases Max MAC by: {0}-{0}.\n"), Values[0]);
                    break;
                case BuffType.WonderDrug:
                    text = string.Format(CMain.Tr("Wonder Drug\n"));
                    switch (Values[0])
                    {
                        case 0:
                            text += string.Format(CMain.Tr("Increases Exp Rate by {0}%\n"), Values[1]);
                            break;
                        case 1:
                            text += string.Format(CMain.Tr("Increases Drop Rate by {0}%\n"), Values[1]);
                            break;
                        case 2:
                            text += string.Format(CMain.Tr("Increases HP by: {0}.\n"), Values[1]);
                            break;
                        case 3:
                            text += string.Format(CMain.Tr("Increases MP by: {0}.\n"), Values[1]);
                            break;
                        case 4:
                            text += string.Format(CMain.Tr("Increases Max AC by: {0}-{0}.\n"), Values[1]);
                            break;
                        case 5:
                            text += string.Format(CMain.Tr("Increases Max MAC by: {0}-{0}.\n"), Values[1]);
                            break;
                        case 6:
                            text += string.Format(CMain.Tr("Increases A.Speed by: {0}.\n"), Values[1]);
                            break;
                    }
                    break;
                case BuffType.Knapsack:
                    text = string.Format(CMain.Tr("Knapsack\nIncreases BagWeight by: {0}.\n"), Values[0]);
                    break;

                case BuffType.HumUp:
                    text += string.Format(CMain.Tr("-MaxHp: {0}\n"), Values[0]);
                    text += string.Format(CMain.Tr("-MaxMp: {0}\n"), Values[1]);
                    text += string.Format(CMain.Tr("-HealthRecovery: {0}\n"), Values[2]);
                    text += string.Format(CMain.Tr("-SpellRecovery: {0}\n"), Values[3]);
                    text += string.Format(CMain.Tr("-MaxBagWeight: {0}\n"), Values[4]);
                    break;

                case BuffType.Luck:
                    text += string.Format(CMain.Tr("-Luck: {0}\n"), Values[0]);
                    text += string.Format(CMain.Tr("-DC: {0}\n"), Values[1]);
                    text += string.Format(CMain.Tr("-MC: {0}\n"), Values[2]);
                    if (Values[3] > 0)
                        text += string.Format(CMain.Tr("-SC: {0}\n"), Values[3]);
                    if (Values[4] > 0)
                        text += string.Format(CMain.Tr("-AC: {0}\n"), Values[4]);
                    if (Values.Length > 5 && Values[5] > 0)
                        text += string.Format(CMain.Tr("-Agility: {0}\n"), Values[5]);
                    if (Values.Length > 6 && Values[6] > 0)
                        text += string.Format(CMain.Tr("-Accuracy: {0}\n"), Values[6]);
                    if (Values.Length > 7 && Values[7] > 0)
                        text += string.Format(CMain.Tr("-MaxHP: {0}\n"), Values[7]);
                    break;
                case BuffType.MoreaBlood:
                case BuffType.MoreaBlood2:
                case BuffType.MoreaBlood3:
                case BuffType.MoreaBlood8:
                    break;

            }

            text += string.Format(CMain.Tr("Expire: {0}"), Infinite ? CMain.Tr("Never") : Functions.PrintTimeSpanFromSeconds(Math.Round((Expire - CMain.Time) / 1000D)));

            if (Caster.Length > 0) text += string.Format(CMain.Tr("\nCaster: {0}"), Caster);

            return text;
        }

    }
}
