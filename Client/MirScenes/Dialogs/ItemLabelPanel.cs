using Client.MirControls;
using Client.MirGraphics;
using Client.MirObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MirScenes.Dialogs
{
    public class ItemLabelPanel
    {
        private UserItem HoverItem;
        public InspectDialog InspectDialog;
        public Dictionary<int, ItemInfo> ItemInfoList;
        public MirControl ItemLabel;

        public MirControl NameInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            MirLabel nameLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.FromKnownColor(item.Info.NameColor), // GradeNameColor(HoverItem.Info.Grade),
                Location = new Point(4, 4),
                OutLine = true,
                Parent = ItemLabel,
                Text = HoverItem.Info.Grade != ItemGrade.None ? HoverItem.Info.FriendlyName :
                (HoverItem.Info.Type == ItemType.Pets && HoverItem.Info.Shape == 26 && HoverItem.Info.Effect != 7) ? CMain.Tr("WonderDrug") : HoverItem.Info.FriendlyName,
            };

            if (HoverItem.RefineAdded > 0)
                nameLabel.Text = "(*)" + nameLabel.Text;

            if (HoverItem.Transform > 0)
            {
                int type = HoverItem.Transform / 100;
                switch (type)
                {
                    case 4:
                        if (!nameLabel.Text.StartsWith("碧血"))
                            nameLabel.Text = "碧血" + nameLabel.Text;
                        break;

                    case 5:
                        if (!nameLabel.Text.StartsWith("虹玄"))
                            nameLabel.Text = "虹玄" + nameLabel.Text;
                        break;

                    case 6:
                        if (!nameLabel.Text.StartsWith("翊仙"))
                            nameLabel.Text = "翊仙" + nameLabel.Text;
                        break;

                    case 7:
                        if (!nameLabel.Text.StartsWith("飞燕"))
                            nameLabel.Text = "飞燕" + nameLabel.Text;
                        break;

                    case 8:
                        if (!nameLabel.Text.StartsWith("暗鬼"))
                            nameLabel.Text = "暗鬼" + nameLabel.Text;
                        break;

                    default:
                        nameLabel.Text = "幻化" + nameLabel.Text;
                        break;
                }
            }

#if DEBUG
            nameLabel.Text += item.Transform + " " + item.TransformImage;
#endif

            ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, nameLabel.DisplayRectangle.Right + 4),
                Math.Max(ItemLabel.Size.Height + 2, nameLabel.DisplayRectangle.Bottom + 2));


            if (HoverItem.Info.Grade != ItemGrade.None)
            {
                MirLabel gradeLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.FromKnownColor(item.Info.NameColor), // GradeNameColor(HoverItem.Info.Grade),
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Tr(HoverItem.Info.Grade.ToString()) 
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, gradeLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height + 2, gradeLabel.DisplayRectangle.Bottom + 2));
            }

            string text = "";

            if (HoverItem.Info.StackSize > 1)
            {
                text += string.Format(CMain.Tr(" Count {0}"), HoverItem.Count);
            }

            if (HoverItem.Info.Durability > 0)
            {
                switch (HoverItem.Info.Type)
                {
                    case ItemType.Amulet:
                        text += string.Format(CMain.Tr(" Usage {0}/{1}"), HoverItem.CurrentDura, HoverItem.MaxDura);
                        break;
                    case ItemType.Ore:
                        text += string.Format(CMain.Tr(" Purity {0}"), Math.Round(HoverItem.CurrentDura / 1000M));
                        break;
                    case ItemType.Meat:
                        text += string.Format(CMain.Tr(" Quality {0}"), Math.Round(HoverItem.CurrentDura / 1000M));
                        break;
                    case ItemType.Mount:
                        text += string.Format(CMain.Tr(" Loyalty {0} / {1}"), HoverItem.CurrentDura, HoverItem.MaxDura);
                        break;
                    case ItemType.Food:
                        text += string.Format(CMain.Tr(" Nutrition {0}"), HoverItem.CurrentDura);
                        break;
                    case ItemType.Gem:
                        break;
                    case ItemType.Potion:
                        break;
                    case ItemType.Pets:
                        if (HoverItem.Info.Shape == 26)//WonderDrug
                        {
                            string strTime = Functions.PrintTimeSpanFromSeconds((HoverItem.CurrentDura * 3600), false);
                            text += "\n" + string.Format(" Duration {0}", strTime);
                        }
                        break;
                    default:
                        text += string.Format(CMain.Tr(" Durability {0}/{1}"), Math.Round(HoverItem.CurrentDura / 1000M),
                                                   Math.Round(HoverItem.MaxDura / 1000M));
                        break;
                }
            }

            String WedRingName = "";
            if (HoverItem.WeddingRing == -1)
            {
                WedRingName = CMain.Tr(HoverItem.Info.Type.ToString());
                //"\n" + CMain.Tr("W ") + HoverItem.Weight + text;
            }
            else
            {
                WedRingName = CMain.Tr("Wedding Ring");
               // "\n" + CMain.Tr("W ") + HoverItem.Weight + text;
            }


            MirLabel etcLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = Color.White,
                Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                OutLine = true,
                Parent = ItemLabel,
                Text = WedRingName
            };

            ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, etcLabel.DisplayRectangle.Right + 4),
                Math.Max(ItemLabel.Size.Height + 2, etcLabel.DisplayRectangle.Bottom + 2));

            QuickCreateLabel(CMain.Tr("W ") + HoverItem.Weight + text, Color.White);

            ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, etcLabel.DisplayRectangle.Right + 4),
            Math.Max(ItemLabel.Size.Height + 2, etcLabel.DisplayRectangle.Bottom + 2));

            #region OUTLINE
            MirControl outLine = new MirControl
            {
                BackColour = Color.FromArgb(255, 50, 50, 50),
                Border = true,
                BorderColour = Color.Gray,
                NotControl = true,
                Parent = ItemLabel,
                Opacity = 0.4F,
                Location = new Point(0, 0)
            };
            outLine.Size = ItemLabel.Size;
            #endregion

            return outLine;
        }

        private void QuickCreateLabel(string text, Color color)
        {
            MirLabel label = new MirLabel
            {
                AutoSize = true,
                ForeColour = color,
                Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                OutLine = true,
                Parent = ItemLabel,
                Text = text
            };

            ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, label.DisplayRectangle.Right + 4),
                Math.Max(ItemLabel.Size.Height, label.DisplayRectangle.Bottom));
        }

        public MirControl AttackInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            bool fishingItem = false;

            switch (HoverItem.Info.Type)
            {
                case ItemType.Hook:
                case ItemType.Float:
                case ItemType.Bait:
                case ItemType.Finder:
                case ItemType.Reel:
                    fishingItem = true;
                    break;
                case ItemType.Weapon:
                    if (HoverItem.Info.Shape == 49 || HoverItem.Info.Shape == 50)
                        fishingItem = true;
                    break;
                case ItemType.Pets:
                    if (HoverItem.Info.Shape == 26) return null;
                    break;
                default:
                    fishingItem = false;
                    break;
            }

            int count = 0;
            int minValue = 0;
            int maxValue = 0;
            int addValue = 0;
            string text = "";

            #region Dura gem
            minValue = realItem.Durability;

            if (minValue > 0 && realItem.Type == ItemType.Gem)
            {
                count++;
                text = string.Format(CMain.Tr("Adds {0}Durability"), minValue / 1000);
                MirLabel DuraLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DuraLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DuraLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DC
            minValue = realItem.MinDC;
            maxValue = realItem.MaxDC;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.DC : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("DC + {0}~{1} (+{2})") : CMain.Tr("DC + {0}~{1}"), minValue, maxValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}DC"), minValue + maxValue + addValue);
                MirLabel DCLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("DC + {0}~{1}", minValue, maxValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DCLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DCLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MC

            minValue = realItem.MinMC;
            maxValue = realItem.MaxMC;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.MC : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("MC + {0}~{1} (+{2})") : CMain.Tr("MC + {0}~{1}"), minValue, maxValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}MC"), minValue + maxValue + addValue);
                MirLabel MCLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("MC + {0}~{1}", minValue, maxValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MCLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MCLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region SC

            minValue = realItem.MinSC;
            maxValue = realItem.MaxSC;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.SC : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("SC + {0}~{1} (+{2})") : CMain.Tr("SC + {0}~{1}"), minValue, maxValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}SC"), minValue + maxValue + addValue);
                MirLabel SCLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("SC + {0}~{1}", minValue, maxValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, SCLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, SCLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region LUCK / SUCCESS

            minValue = realItem.Luck;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.Luck : 0;

            if (minValue != 0 || addValue != 0)
            {
                count++;

                if (realItem.Type == ItemType.Pets && realItem.Shape == 28)
                {
                    text = CMain.Format("BagWeight + {0}% ", minValue + addValue);
                }
                else if (realItem.Type == ItemType.Potion && realItem.Shape == 4)
                {
                    text = CMain.Format("Exp + {0}% ", minValue + addValue);
                }
                else
                {
                    text = string.Format(minValue + addValue > 0 ? CMain.Tr("Luck + {0}") : CMain.Tr("Curse + {0}"), Math.Abs(minValue + addValue));
                }

                MirLabel LUCKLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, LUCKLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, LUCKLabel.DisplayRectangle.Bottom));
            }

            #endregion



            #region ACC

            minValue = realItem.Accuracy;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.Accuracy : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("Accuracy: + {0} (+{1})") : CMain.Tr("Accuracy: + {0}"), minValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}Accuracy"), minValue + maxValue + addValue);
                MirLabel ACCLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Accuracy + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, ACCLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, ACCLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region HOLY

            minValue = realItem.Holy;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel HOLYLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Holy + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Holy: + {0} (+{1})") : CMain.Tr("Holy: + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, HOLYLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, HOLYLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region ASPEED

            minValue = realItem.AttackSpeed;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.AttackSpeed : 0;

            if (minValue != 0 || maxValue != 0 || addValue != 0)
            {
                string plus = (addValue + minValue < 0) ? "" : "+";

                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                {
                    string negative = "+";
                    if (addValue < 0) negative = "";
                    text = string.Format(addValue != 0 ? CMain.Tr("A.Speed: ") + plus + "{0} ({2}{1})" : CMain.Tr("A.Speed: ") + plus + "{0}", minValue + addValue, addValue, negative);
                    //text = string.Format(addValue > 0 ? "A.Speed: + {0} (+{1})" : "A.Speed: + {0}", minValue + addValue, addValue);
                }
                else
                    text = string.Format(CMain.Tr("Adds {0}A.Speed"), minValue + maxValue + addValue);
                MirLabel ASPEEDLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("A.Speed + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, ASPEEDLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, ASPEEDLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region FREEZING

            minValue = realItem.Freezing;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.Freezing : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("Freezing: + {0} (+{1})") : CMain.Tr("Freezing: + {0}"), minValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}Freezing"), minValue + maxValue + addValue);
                MirLabel FREEZINGLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Freezing + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, FREEZINGLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, FREEZINGLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region POISON

            minValue = realItem.PoisonAttack;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.PoisonAttack : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("Poison: + {0} (+{1})") : CMain.Tr("Poison: + {0}"), minValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0}Poison"), minValue + maxValue + addValue);
                MirLabel POISONLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Poison + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, POISONLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, POISONLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region CRITICALRATE / FLEXIBILITY

            minValue = realItem.CriticalRate;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.CriticalRate : 0;

            if ((minValue > 0 || maxValue > 0 || addValue > 0) && (realItem.Type != ItemType.Gem))
            {
                count++;
                MirLabel CRITICALRATELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Critical Chance + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Critical Chance: + {0} (+{1})") : CMain.Tr("Critical Chance: + {0}"), minValue + addValue, addValue)
                };

                if (fishingItem)
                {
                    CRITICALRATELabel.Text = string.Format(addValue > 0 ? CMain.Tr("Flexibility: + {0} (+{1})") : CMain.Tr("Flexibility: + {0}"), minValue + addValue, addValue);
                }

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, CRITICALRATELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, CRITICALRATELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region CRITICALDAMAGE

            minValue = realItem.CriticalDamage;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.CriticalDamage : 0;

            if ((minValue > 0 || maxValue > 0 || addValue > 0) && (realItem.Type != ItemType.Gem))
            {
                count++;
                MirLabel CRITICALDAMAGELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Critical Damage + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Critical Damage: + {0} (+{1})") : CMain.Tr("Critical Damage: + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, CRITICALDAMAGELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, CRITICALDAMAGELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region Reflect

            minValue = realItem.Reflect;
            maxValue = 0;
            addValue = 0;

            if ((minValue > 0 || maxValue > 0 || addValue > 0) && (realItem.Type != ItemType.Gem))
            {
                count++;
                MirLabel ReflectLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Reflect chance: {0}"), minValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, ReflectLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, ReflectLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region Hpdrain

            minValue = realItem.HpDrainRate;
            maxValue = 0;
            addValue = 0;

            if ((minValue > 0 || maxValue > 0 || addValue > 0) && (realItem.Type != ItemType.Gem))
            {
                count++;
                MirLabel HPdrainLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("HP Drain Rate: {0}%"), minValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, HPdrainLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, HPdrainLabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }

        public MirControl DefenceInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            bool fishingItem = false;

            switch (HoverItem.Info.Type)
            {
                case ItemType.Hook:
                case ItemType.Float:
                case ItemType.Bait:
                case ItemType.Finder:
                case ItemType.Reel:
                    fishingItem = true;
                    break;
                case ItemType.Weapon:
                    if (HoverItem.Info.Shape == 49 || HoverItem.Info.Shape == 50)
                        fishingItem = true;
                    break;
                case ItemType.Pets:
                    if (HoverItem.Info.Shape == 26) return null;
                    break;
                default:
                    fishingItem = false;
                    break;
            }

            int count = 0;
            int minValue = 0;
            int maxValue = 0;
            int addValue = 0;

            string text = "";
            #region AC

            minValue = realItem.MinAC;
            maxValue = realItem.MaxAC;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.AC : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("AC + {0}~{1} (+{2})") : CMain.Tr("AC + {0}~{1}"), minValue, maxValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0} AC"), minValue + maxValue + addValue);
                MirLabel ACLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("AC + {0}~{1}", minValue, maxValue + addValue)
                    Text = text
                };

                if (fishingItem)
                {
                    if (HoverItem.Info.Type == ItemType.Float)
                    {
                        ACLabel.Text = string.Format(CMain.Tr("Nibble Chance + ") + (addValue > 0 ? "{0}~{1}% (+{2})" : "{0}~{1}%"), minValue, maxValue + addValue);
                    }
                    else if (HoverItem.Info.Type == ItemType.Finder)
                    {
                        ACLabel.Text = string.Format(CMain.Tr("Finder Increase + ") + (addValue > 0 ? "{0}~{1}% (+{2})" : "{0}~{1}%"), minValue, maxValue + addValue);
                    }
                    else
                    {
                        ACLabel.Text = string.Format(CMain.Tr("Success Chance + ") + (addValue > 0 ? "{0}% (+{1})" : "{0}%"), maxValue, maxValue + addValue);
                    }
                }

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, ACLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, ACLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAC

            minValue = realItem.MinMAC;
            maxValue = realItem.MaxMAC;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.MAC : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                {
                    if (addValue > 0)
                        text = string.Format(CMain.Tr("MAC + {0}~{1} (+{2})"), minValue, maxValue + addValue, addValue);
                    else
                        text = string.Format(CMain.Tr("MAC + {0}~{1}"), minValue, maxValue + addValue);

                }
                else
                    text = string.Format(CMain.Tr("Adds {0} MAC"), minValue + maxValue + addValue);
                MirLabel MACLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("MAC + {0}~{1}", minValue, maxValue + addValue)
                    Text = text
                };

                if (fishingItem)
                {
                    MACLabel.Text = string.Format(CMain.Tr("AutoReel Chance + {0}%"), maxValue + addValue);
                }

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MACLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MACLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXHP

            minValue = realItem.HP;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.HP : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXHPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format(realItem.Type == ItemType.Potion ? "HP + {0} Recovery" : "MAXHP + {0}", minValue + addValue)
                    Text = realItem.Type == ItemType.Potion ?
                    string.Format(addValue > 0 ? CMain.Tr("HP + {0} Recovery (+{1})") : CMain.Tr("HP + {0} Recovery"), minValue + addValue, addValue)
                    : string.Format(addValue > 0 ? CMain.Tr("Max HP + {0} (+{1})") : CMain.Tr("Max HP + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXHPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXHPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXMP

            minValue = realItem.MP;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.MP : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXMPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format(realItem.Type == ItemType.Potion ? "MP + {0} Recovery" : "MAXMP + {0}", minValue + addValue)
                    Text = realItem.Type == ItemType.Potion ?
                    string.Format(addValue > 0 ? CMain.Tr("MP + {0} Recovery (+{1})") : CMain.Tr("MP + {0} Recovery"), minValue + addValue, addValue)
                    : string.Format(addValue > 0 ? CMain.Tr("Max MP + {0} (+{1})") : CMain.Tr("Max MP + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXMPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXMPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXHPRATE

            minValue = realItem.HPrate;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXHPRATELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Max HP + {0}%"), minValue + addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXHPRATELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXHPRATELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXMPRATE

            minValue = realItem.MPrate;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXMPRATELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Max MP + {0}%"), minValue + addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXMPRATELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXMPRATELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXACRATE

            minValue = realItem.MaxAcRate;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXACRATE = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Max AC + {0}%"), minValue + addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXACRATE.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXACRATE.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAXMACRATE

            minValue = realItem.MaxMacRate;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MAXMACRATELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Max MAC + {0}%"), minValue + addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAXMACRATELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAXMACRATELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region HEALTH_RECOVERY

            minValue = realItem.HealthRecovery;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.HealthRecovery : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel HEALTH_RECOVERYLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("HealthRecovery + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Health Recovery + {0} (+{1})") : CMain.Tr("Health Recovery + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, HEALTH_RECOVERYLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, HEALTH_RECOVERYLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MANA_RECOVERY

            minValue = realItem.SpellRecovery;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.ManaRecovery : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel MANA_RECOVERYLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("ManaRecovery + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Mana Recovery + {0} (+{1})") : CMain.Tr("Mana Recovery + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MANA_RECOVERYLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MANA_RECOVERYLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region POISON_RECOVERY

            minValue = realItem.PoisonRecovery;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.PoisonRecovery : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel POISON_RECOVERYabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Poison Recovery + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Poison Recovery + {0} (+{1})") : CMain.Tr("Poison Recovery + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, POISON_RECOVERYabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, POISON_RECOVERYabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region AGILITY

            minValue = realItem.Agility;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.Agility : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel AGILITYLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Agility + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Agility + {0} (+{1})") : CMain.Tr("Agility + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, AGILITYLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, AGILITYLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region STRONG

            minValue = realItem.Strong;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.Strong : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel STRONGLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Strong + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Strong + {0} (+{1})") : CMain.Tr("Strong + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, STRONGLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, STRONGLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region POISON_RESIST

            minValue = realItem.PoisonResist;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.PoisonResist : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("Poison Resist + {0} (+{1})") : CMain.Tr("Poison Resist + {0}"), minValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0} Poison Resist"), minValue + maxValue + addValue);
                MirLabel POISON_RESISTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Poison Resist + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, POISON_RESISTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, POISON_RESISTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region MAGIC_RESIST

            minValue = realItem.MagicResist;
            maxValue = 0;
            addValue = (!HoverItem.Info.NeedIdentify || HoverItem.Identified) ? HoverItem.MagicResist : 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                if (HoverItem.Info.Type != ItemType.Gem)
                    text = string.Format(addValue > 0 ? CMain.Tr("Magic Resist + {0} (+{1})") : CMain.Tr("Magic Resist + {0}"), minValue + addValue, addValue);
                else
                    text = string.Format(CMain.Tr("Adds {0} Magic Resist"), minValue + maxValue + addValue);
                MirLabel MAGIC_RESISTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Magic Resist + {0}", minValue + addValue)
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, MAGIC_RESISTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, MAGIC_RESISTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }

        public void Process()
        {
            if (ItemLabel != null && !ItemLabel.IsDisposed)
            {
                ItemLabel.BringToFront();

                int x = CMain.MPoint.X + 15, y = CMain.MPoint.Y;
                if (x + ItemLabel.Size.Width > Settings.ScreenWidth)
                    x = Settings.ScreenWidth - ItemLabel.Size.Width;

                if (y + ItemLabel.Size.Height > Settings.ScreenHeight)
                    y = Settings.ScreenHeight - ItemLabel.Size.Height;
                ItemLabel.Location = new Point(x, y);
            }
        }

        public MirControl WeightInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;
            int minValue = 0;
            int maxValue = 0;
            int addValue = 0;

            #region HANDWEIGHT

            minValue = realItem.HandWeight;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel HANDWEIGHTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Hand Weight + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Hand Weight + {0} (+{1})") : CMain.Tr("Hand Weight + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, HANDWEIGHTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, HANDWEIGHTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region WEARWEIGHT

            minValue = realItem.WearWeight;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel WEARWEIGHTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Wear Weight + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Wear Weight + {0} (+{1})") : CMain.Tr("Wear Weight + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, WEARWEIGHTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, WEARWEIGHTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region BAGWEIGHT

            minValue = realItem.BagWeight;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel BAGWEIGHTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    //Text = string.Format("Bag Weight + {0}", minValue + addValue)
                    Text = string.Format(addValue > 0 ? CMain.Tr("Bag Weight + {0} (+{1})") : CMain.Tr("Bag Weight + {0}"), minValue + addValue, addValue)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, BAGWEIGHTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, BAGWEIGHTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region FASTRUN
            minValue = realItem.CanFastRun == true ? 1 : 0;
            maxValue = 0;
            addValue = 0;

            if (minValue > 0 || maxValue > 0 || addValue > 0)
            {
                count++;
                MirLabel BAGWEIGHTLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Instant Run"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, BAGWEIGHTLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, BAGWEIGHTLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region TIME & RANGE
            minValue = 0;
            maxValue = 0;
            addValue = 0;

            if (HoverItem.Info.Type == ItemType.Potion && HoverItem.Info.Durability > 0)
            {
                count++;
                MirLabel TNRLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = addValue > 0 ? Color.Cyan : Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Time : {0}"), Functions.PrintTimeSpanFromSeconds(HoverItem.Info.Durability * 60))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, TNRLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, TNRLabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }
        public MirControl AwakeInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;

            #region AWAKENAME
            if (HoverItem.Awake.getAwakeLevel() > 0)
            {
                count++;
                MirLabel AWAKENAMELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = GameScene.GradeNameColor(HoverItem.Info.Grade),
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("{0} Awakening({1})"), CMain.Tr(HoverItem.Awake.type.ToString()), HoverItem.Awake.getAwakeLevel())
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, AWAKENAMELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, AWAKENAMELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region AWAKE_TOTAL_VALUE
            if (HoverItem.Awake.getAwakeValue() > 0)
            {
                count++;
                MirLabel AWAKE_TOTAL_VALUELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(realItem.Type != ItemType.Armour ? "{0} + {1}-{2}" : CMain.Tr("MAX {0} + {1}"), CMain.Tr(HoverItem.Awake.type.ToString()), HoverItem.Awake.getAwakeValue(), HoverItem.Awake.getAwakeValue())
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, AWAKE_TOTAL_VALUELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, AWAKE_TOTAL_VALUELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region AWAKE_LEVEL_VALUE
            if (HoverItem.Awake.getAwakeLevel() > 0)
            {
                count++;
                for (int i = 0; i < HoverItem.Awake.getAwakeLevel(); i++)
                {
                    MirLabel AWAKE_LEVEL_VALUELabel = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = string.Format(realItem.Type != ItemType.Armour ? CMain.Tr("Level {0} : {1} + {2}-{3}") : CMain.Tr("Level {0} : MAX {1} + {2}-{3}"), i + 1, CMain.Tr(HoverItem.Awake.type.ToString()), HoverItem.Awake.getAwakeLevelValue(i), HoverItem.Awake.getAwakeLevelValue(i))
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, AWAKE_LEVEL_VALUELabel.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, AWAKE_LEVEL_VALUELabel.DisplayRectangle.Bottom));
                }
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }
        public MirControl NeedInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;

            #region LEVEL
            if (realItem.RequiredAmount > 0)
            {
                count++;
                string text;
                Color colour = Color.White;
                switch (realItem.RequiredType)
                {
                    case RequiredType.Level:
                        text = string.Format(CMain.Tr("Required Level : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.Level < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxAC:
                        text = string.Format(CMain.Tr("Required AC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MaxAC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxMAC:
                        text = string.Format(CMain.Tr("Required MAC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MaxMAC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxDC:
                        text = string.Format(CMain.Tr("Required DC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MaxDC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxMC:
                        text = string.Format(CMain.Tr("Required MC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MaxMC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxSC:
                        text = string.Format(CMain.Tr("Required SC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MaxSC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MaxLevel:
                        text = string.Format(CMain.Tr("Maximum Level : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.Level > realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MinAC:
                        text = string.Format(CMain.Tr("Required Base AC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MinAC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MinMAC:
                        text = string.Format(CMain.Tr("Required Base MAC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MinMAC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MinDC:
                        text = string.Format(CMain.Tr("Required Base DC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MinDC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MinMC:
                        text = string.Format(CMain.Tr("Required Base MC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MinMC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    case RequiredType.MinSC:
                        text = string.Format(CMain.Tr("Required Base SC : {0}"), realItem.RequiredAmount);
                        if (MapObject.User.MinSC < realItem.RequiredAmount)
                            colour = Color.Red;
                        break;
                    default:
                        text = "Unknown Type Required";
                        break;
                }

                MirLabel LEVELLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, LEVELLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, LEVELLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region CLASS
            if (realItem.RequiredClass != RequiredClass.None)
            {
                count++;
                Color colour = Color.White;

                switch (MapObject.User.Class)
                {
                    case MirClass.Warrior:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Warrior))
                            colour = Color.Red;
                        break;
                    case MirClass.Wizard:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Wizard))
                            colour = Color.Red;
                        break;
                    case MirClass.Taoist:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Taoist))
                            colour = Color.Red;
                        break;
                    case MirClass.Assassin:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Assassin))
                            colour = Color.Red;
                        break;
                    case MirClass.Archer:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Archer))
                            colour = Color.Red;
                        break;
                    case MirClass.Monk:
                        if (!realItem.RequiredClass.HasFlag(RequiredClass.Monk))
                            colour = Color.Red;
                        break;
                }

                MirLabel CLASSLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Class Required : {0}"), CMain.Tr(realItem.RequiredClass.ToString()))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, CLASSLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, CLASSLabel.DisplayRectangle.Bottom));
        }

            #endregion
            #region HUMUP
            if (realItem.NeedHumUp)
            {
                count++;
                Color colour = Color.White;
                if (realItem.NeedHumUp && !MapObject.User.HumUp)
                    colour = Color.Red;

                MirLabel RequireLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Tr("HumUp Required")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, RequireLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, RequireLabel.DisplayRectangle.Bottom));
            }

            if (realItem.Type == ItemType.Book && realItem.Effect > 0)
            {
                count++;
                Color colour = Color.White;
                Spell spell = (Spell)realItem.Effect;
                if (MapObject.User.GetMagic(spell) == null)
                    colour = Color.Red;

                MirLabel RequireLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = colour,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = "前置技能:" + CMain.Tr(spell.ToString())
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, RequireLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, RequireLabel.DisplayRectangle.Bottom));
            }
            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }
        public MirControl BindInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;

            #region DONT_DEATH_DROP

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontDeathdrop))
            {
                count++;
                MirLabel DONT_DEATH_DROPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't drop on death")
                    )
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_DEATH_DROPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_DEATH_DROPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_DROP

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontDrop))
            {
                count++;
                MirLabel DONT_DROPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't drop"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_DROPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_DROPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_UPGRADE

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontUpgrade))
            {
                count++;
                MirLabel DONT_UPGRADELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't upgrade"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_UPGRADELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_UPGRADELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_SELL

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontSell))
            {
                count++;
                MirLabel DONT_SELLLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't sell"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_SELLLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_SELLLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_TRADE

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontTrade))
            {
                count++;
                MirLabel DONT_TRADELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't trade"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_TRADELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_TRADELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_STORE

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontStore))
            {
                count++;
                MirLabel DONT_STORELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't store"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_STORELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_STORELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_REPAIR

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DontRepair))
            {
                count++;
                MirLabel DONT_REPAIRLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't repair"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_REPAIRLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_REPAIRLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_SPECIALREPAIR

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.NoSRepair))
            {
                count++;
                MirLabel DONT_REPAIRLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Can't special repair"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_REPAIRLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_REPAIRLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region BREAK_ON_DEATH

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.BreakOnDeath))
            {
                count++;
                MirLabel DONT_REPAIRLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Breaks on death"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_REPAIRLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_REPAIRLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region DONT_DESTROY_ON_DROP

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.DestroyOnDrop))
            {
                count++;
                MirLabel DONT_DODLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Destroyed when dropped"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, DONT_DODLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, DONT_DODLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region NoWeddingRing

            if (HoverItem.Info.Bind != BindMode.none && HoverItem.Info.Bind.HasFlag(BindMode.NoWeddingRing))
            {
                count++;
                MirLabel No_WedLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Cannot be a weddingring"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, No_WedLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, No_WedLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region BIND_ON_EQUIP

            if ((HoverItem.Info.Bind.HasFlag(BindMode.BindOnEquip)) & HoverItem.SoulBoundId == -1)
            {
                count++;
                MirLabel BOELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Soulbinds on equip"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, BOELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, BOELabel.DisplayRectangle.Bottom));
            }
            else if (HoverItem.SoulBoundId != -1)
            {
                count++;
                MirLabel BOELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Tr("Soulbound to: ") + GameScene.Scene.GetUserName((uint)HoverItem.SoulBoundId)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, BOELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, BOELabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region CURSED

            if ((!HoverItem.Info.NeedIdentify || HoverItem.Identified) && HoverItem.Cursed)
            {
                count++;
                MirLabel CURSEDLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Cursed"))
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, CURSEDLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, CURSEDLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region Gems

            if (HoverItem.Info.Type == ItemType.Gem)
            {
                #region UseOn text
                count++;
                string Text = "";
                if (HoverItem.Info.Unique == SpecialItemMode.None)
                {
                    Text = CMain.Tr("Cannot be used on any item.");
                }
                else
                {
                    Text = CMain.Tr("Can be used on: ");
                }
                MirLabel GemUseOn = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = Text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GemUseOn.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, GemUseOn.DisplayRectangle.Bottom));
                #endregion
                #region Weapon text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Paralize))
                {
                    MirLabel GemWeapon = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Weapon")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GemWeapon.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, GemWeapon.DisplayRectangle.Bottom));
                }
                #endregion
                #region Armour text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Teleport))
                {
                    MirLabel GemArmour = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Armour")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GemArmour.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, GemArmour.DisplayRectangle.Bottom));
                }
                #endregion
                #region Helmet text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Clearring))
                {
                    MirLabel Gemhelmet = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Helmet")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemhelmet.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemhelmet.DisplayRectangle.Bottom));
                }
                #endregion
                #region Necklace text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Protection))
                {
                    MirLabel Gemnecklace = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Necklace")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemnecklace.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemnecklace.DisplayRectangle.Bottom));
                }
                #endregion
                #region Bracelet text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Revival))
                {
                    MirLabel GemBracelet = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Bracelet")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GemBracelet.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, GemBracelet.DisplayRectangle.Bottom));
                }
                #endregion
                #region Ring text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Muscle))
                {
                    MirLabel GemRing = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Ring")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GemRing.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, GemRing.DisplayRectangle.Bottom));
                }
                #endregion
                #region Amulet text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Flame))
                {
                    MirLabel Gemamulet = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Amulet")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemamulet.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemamulet.DisplayRectangle.Bottom));
                }
                #endregion
                #region Belt text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Healing))
                {
                    MirLabel Gembelt = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Belt")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gembelt.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gembelt.DisplayRectangle.Bottom));
                }
                #endregion
                #region Boots text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Probe))
                {
                    MirLabel Gemboots = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Boots")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemboots.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemboots.DisplayRectangle.Bottom));
                }
                #endregion
                #region Stone text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.Skill))
                {
                    MirLabel Gemstone = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Stone")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemstone.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemstone.DisplayRectangle.Bottom));
                }
                #endregion
                #region Torch text
                count++;
                if (HoverItem.Info.Unique.HasFlag(SpecialItemMode.NoDuraLoss))
                {
                    MirLabel Gemtorch = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = Color.White,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = CMain.Tr("-Candle")
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, Gemtorch.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, Gemtorch.DisplayRectangle.Bottom));
                }
                #endregion
            }

            #endregion

            #region CANTAWAKEN

            if (!HoverItem.Info.CanAwakening && (HoverItem.Info.Type != ItemType.Gem))
            {
                count++;
                MirLabel CANTAWAKENINGLabel = new MirLabel
               {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Format("Can't awaken")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, CANTAWAKENINGLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, CANTAWAKENINGLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region EXPIRE

            if (HoverItem.ExpireInfo != null)
            {
                double remainingSeconds = (HoverItem.ExpireInfo.ExpiryDate - DateTime.Now).TotalSeconds;

                count++;
                MirLabel EXPIRELabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Yellow,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = remainingSeconds > 0 ? string.Format(CMain.Tr("Expires in {0}"), Functions.PrintTimeSpanFromSeconds(remainingSeconds)) : CMain.Tr("Expired")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, EXPIRELabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, EXPIRELabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (HoverItem.RentalInformation?.RentalLocked == false)
            {

                count++;
                MirLabel OWNERLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.DarkKhaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Tr("Item rented from: ") + HoverItem.RentalInformation.OwnerName
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, OWNERLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, OWNERLabel.DisplayRectangle.Bottom));

                double remainingTime = (HoverItem.RentalInformation.ExpiryDate - DateTime.Now).TotalSeconds;

                count++;
                MirLabel RENTALLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Khaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = remainingTime > 0 ? string.Format(CMain.Tr("Rental expires in: {0}"), Functions.PrintTimeSpanFromSeconds(remainingTime)) : CMain.Tr("Rental expired")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, RENTALLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, RENTALLabel.DisplayRectangle.Bottom));
            }
            else if (HoverItem.RentalInformation?.RentalLocked == true && HoverItem.RentalInformation.ExpiryDate > DateTime.Now)
            {
                count++;
                var remainingTime = (HoverItem.RentalInformation.ExpiryDate - DateTime.Now).TotalSeconds;
                var RentalLockLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.DarkKhaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = remainingTime > 0 ? string.Format(CMain.Tr("Rental lock expires in: {0}"), Functions.PrintTimeSpanFromSeconds(remainingTime)) : CMain.Tr("Rental lock expired")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, RentalLockLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, RentalLockLabel.DisplayRectangle.Bottom));
            }

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }
        public MirControl OverlapInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;


            #region GEM

            if (realItem.Type == ItemType.Gem)
            {
                string text = "";

                switch (realItem.Shape)
                {
                    case 1:
                        text = "Hold CTRL and left click to repair weapons.";
                        break;
                    case 2:
                        text = "Hold CTRL and left click to repair armour\nand accessory items.";
                        break;
                    case 3:
                    case 4:
                        text = "Hold CTRL and left click to combine with an item.";
                        break;
                }

                text = CMain.Tr(text);
                count++;
                MirLabel GEMLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = text
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, GEMLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, GEMLabel.DisplayRectangle.Bottom));
            }

            #endregion

            #region SPLITUP

            if (realItem.StackSize > 1 && realItem.Type != ItemType.Gem)
            {
                count++;
                MirLabel SPLITUPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.White,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = string.Format(CMain.Tr("Max Combine Count : {0}\nShift + Left click to split the stack"), realItem.StackSize)
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, SPLITUPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, SPLITUPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }
        public MirControl StoryInfoLabel(UserItem item, bool Inspect = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

            int count = 0;

            #region TOOLTIP

            if (realItem.Type == ItemType.Pets && realItem.Shape == 26)//Dynamic wonderDrug
            {
                string strTime = Functions.PrintTimeSpanFromSeconds((HoverItem.CurrentDura * 3600), false);
                switch ((int)realItem.Effect)
                {
                    case 0://exp low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase experience gained by {0}% for {1}."), HoverItem.Luck + realItem.Luck, strTime);
                        break;
                    case 1://drop low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase droprate by {0}% for {1}."), HoverItem.Luck + realItem.Luck, strTime);
                        break;
                    case 2://hp low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase MaxHP +{0} for {1}."), HoverItem.HP + realItem.HP, strTime);
                        break;
                    case 3://mp low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase MaxMP +{0} for {1}."), HoverItem.MP + realItem.MP, strTime);
                        break;
                    case 4://ac low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase AC {0}-{0} for {1}."), HoverItem.AC + realItem.MaxAC, strTime);
                        break;
                    case 5://amc low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase AMC {0}-{0} for {1}."), HoverItem.MAC + realItem.MaxAC, strTime);
                        break;
                    case 6://speed low/med/high
                        HoverItem.Info.ToolTip = string.Format(CMain.Tr("Increase AttackSpeed by {0} for {1}."), HoverItem.AttackSpeed + realItem.AttackSpeed, strTime);
                        break;
                }
            }

            if (realItem.Type == ItemType.Scroll && realItem.Shape == 7)//Credit Scroll
            {
                HoverItem.Info.ToolTip = string.Format(CMain.Tr("Adds {0} Credits to your Account."), HoverItem.Info.Price);
            }

            if (!string.IsNullOrEmpty(HoverItem.Info.ToolTip))
            {
                count++;

                MirLabel IDLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.DarkKhaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = CMain.Tr("Item Description:")
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, IDLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, IDLabel.DisplayRectangle.Bottom));

                MirLabel TOOLTIPLabel = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Khaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = HoverItem.Info.ToolTip
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, TOOLTIPLabel.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, TOOLTIPLabel.DisplayRectangle.Bottom));
            }

            #endregion

            if (count > 0)
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }

        public MirControl SocketInfoLabel(UserItem item, bool Inspect = false, bool hero = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, GameScene.ItemInfoList);
            if (realItem.Type == ItemType.Gem || item.RuneSlots.Length <= 0)
                return ItemLabel;

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            int scount = 0;
            string temp = "";

            temp = string.Format("({0}) Sockets", item.RuneSlots.Length);
            MirLabel socketLabel = new MirLabel
            {
                AutoSize = true,
                ForeColour = GameScene.GradeNameColor(HoverItem.Info.Grade),
                Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                OutLine = true,
                Parent = ItemLabel,
                Text = temp
            };
            ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, socketLabel.DisplayRectangle.Right + 4),
                Math.Max(ItemLabel.Size.Height, socketLabel.DisplayRectangle.Bottom + 4));

            for (int i = 0; i < item.RuneSlots.Length; i++)
            {
                Sockets sock = item.RuneSlots[i];
                if (sock != null)
                {
                    MirImageControl imgTest = new MirImageControl
                    {
                        Parent = ItemLabel,
                        Library = Libraries.EdensEliteInter,
                        Index = GetSocketItemIndex(sock.SocketItemType),
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        DrawImage = true,
                        NotControl = true,
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, imgTest.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, imgTest.DisplayRectangle.Bottom + 4));

                    temp = string.Format("{0}", sock.SocketedItem.FriendlyName);
                    MirLabel runeLabel = new MirLabel
                    {
                        AutoSize = true,
                        ForeColour = GameScene.GradeNameColor(HoverItem.Info.Grade),
                        Location = new Point(imgTest.Location.X + imgTest.Size.Width + 4, imgTest.Location.Y),
                        OutLine = true,
                        Parent = ItemLabel,
                        Text = temp
                    };
                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, runeLabel.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, runeLabel.DisplayRectangle.Bottom + 4));
                    scount++;
                }
                else
                {
                    MirImageControl imgTest = new MirImageControl
                    {
                        Parent = ItemLabel,
                        Library = Libraries.EdensEliteInter,
                        Index = 2,
                        Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                        DrawImage = true,
                        NotControl = true,
                    };

                    ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, imgTest.DisplayRectangle.Right + 4),
                        Math.Max(ItemLabel.Size.Height, imgTest.DisplayRectangle.Bottom + 4));
                }
            }

            if (scount > 0)
            {
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }

            return ItemLabel;
        }

        private int GetSocketItemIndex(SocketType socketItemType)
        {
            switch (socketItemType)
            {
                case SocketType.DamageIncreasePvE:
                case SocketType.DamageIncreasePvP:
                case SocketType.DamageReductionPvE:
                case SocketType.DamageReductionPvP:
                    return 3;
                case SocketType.DestructionBonus:
                case SocketType.MagicBonus:
                case SocketType.SpiritBonus:
                    return 5;
                case SocketType.Enrage:
                case SocketType.PinPoint:
                case SocketType.IronWall:
                case SocketType.Evasive:
                case SocketType.SpeedyMagician:
                    return 4;
                case SocketType.MeleeDamageBonus:
                case SocketType.MagicDamageBonus:
                case SocketType.SpiritualBonus:
                    return 6;
                default:
                    return 2;
            }
        }

        public void DisposeItemLabel()
        {
            if (ItemLabel != null && !ItemLabel.IsDisposed)
                ItemLabel.Dispose();
            ItemLabel = null;
        }

        public MirControl RuneStatLavel(UserItem item, bool Inspect = false, bool hero = false)
        {
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;

            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);
            string statDescription = string.Format("");
            int count = 0;
            //  Passive & New stat runes
            if (realItem.Type == ItemType.RuneStone &&
                realItem.Shape >= 0 &&
                realItem.Shape < 255)
            {
                switch ((SocketType)realItem.Shape)
                {
                    case SocketType.DamageReductionPvE:
                        statDescription = string.Format("PvE Damage Reduction {0}%", realItem.MaxMAC);
                        break;
                    case SocketType.DamageReductionPvP:
                        statDescription = string.Format("PvP Damage Reduction {0}%", realItem.MaxAC);
                        break;
                    case SocketType.DamageIncreasePvE:
                        statDescription = string.Format("PvE Damage Increase {0}%", realItem.MinAC);
                        break;
                    case SocketType.DamageIncreasePvP:
                        statDescription = string.Format("PvE Damage Increase {0}%", realItem.MinMAC);
                        break;
                    case SocketType.MeleeDamageBonus:
                        statDescription = string.Format("Melee Damage Bonus {0}%", realItem.MaxAcRate);
                        break;
                    case SocketType.MagicDamageBonus:
                        statDescription = string.Format("Magic Damage Bonus {0}%", realItem.MaxMacRate);
                        break;
                    case SocketType.SpiritualBonus:
                        statDescription = string.Format("Spiritual Damage Bonus {0}%", realItem.Holy);
                        break;
                    //  Flat bonus
                    case SocketType.HealthBonus:
                        statDescription = string.Format("HP + {0}", realItem.HP);
                        break;
                    case SocketType.ManaBonus:
                        statDescription = string.Format("MP + {0}", realItem.MP);
                        break;
                    //  % Based
                    case SocketType.HealthRegenBonus:
                        statDescription = string.Format("HP Recovery + {0}", realItem.HPrate);
                        break;
                    case SocketType.ManaRegenBonus:
                        statDescription = string.Format("MP Recovery + {0}", realItem.MPrate);
                        break;
                    //  Flat bonus
                    case SocketType.DestructionBonus:
                        statDescription = string.Format("DC + {0}-{1}", realItem.MinDC, realItem.MaxDC);
                        break;
                    case SocketType.MagicBonus:
                        statDescription = string.Format("MC + {0}-{1}", realItem.MinMC, realItem.MaxMC);
                        break;
                    case SocketType.SpiritBonus:
                        statDescription = string.Format("SC + {0}-{1}", realItem.MinSC, realItem.MaxSC);
                        break;
                    //  Small Accuracy Boost
                    case SocketType.PinPoint:
                        statDescription = string.Format("Chance to trigger : {0}%\r\nAccuracy + {1}\r\nMax DC + {2}\r\nCrit Rate + {3}\r\nCrit Damage + {4}\r\nDuration : {5}\r\nCool-Down : {6}",
                            realItem.Weight, realItem.Accuracy, realItem.MaxDC, realItem.CriticalRate, realItem.CriticalDamage, realItem.Durability, realItem.RequiredAmount);
                        break;
                    //  Small Agility Boost
                    case SocketType.Evasive:
                        statDescription = string.Format("Chance to trigger : {0}%\r\nMax AC + {1}\r\nMax AMC + {2}\r\nAgility + {3}\r\nDuration : {4}\r\nCool-Down : {5}",
                            realItem.Weight, realItem.MaxAC, realItem.MaxMAC, realItem.Agility, realItem.Durability, realItem.RequiredAmount);
                        break;
                    //  Small Attack Speed & Crit bonus.
                    case SocketType.Enrage:
                        statDescription = string.Format("Chance to trigger : {0}%\r\nA.Speed + {1}\r\nMax DC + {2}\r\nCrit Rate + {3}\r\nCrit Damage + {4}\r\nDuration : {5}\r\nCool-Down : {6}",
                            realItem.Weight, realItem.AttackSpeed, realItem.MaxDC, realItem.CriticalRate, realItem.CriticalDamage, realItem.Durability, realItem.RequiredAmount);
                        break;
                    //  Decent Defense.
                    case SocketType.IronWall:
                        statDescription = string.Format("Chance to trigger : {0}%\r\nAC + {1}-{2}\r\nMAC + {1}-{2}\r\nHP + {3}\r\nAgility + {4}\r\nDuration : {5}\r\nCool-Down : {6}",
                            realItem.Weight, realItem.MinAC, realItem.MaxAC, realItem.HP, realItem.Agility, realItem.Durability, realItem.RequiredAmount);
                        break;
                    //  Speedy magician
                    case SocketType.SpeedyMagician:
                        statDescription = string.Format("Chance to trigger : {0}%\r\nCool-Down reduction : {1}%\r\nDuration : {2}\r\nCool-Down : {3}",
                            realItem.Weight, realItem.MinMC, realItem.Durability, realItem.RequiredAmount);
                        break;
                }
                if (statDescription.Length > 0)
                    count = 1;
            }
            //  Stat type rune
            else
            {
                if (realItem.MinAC > 0 ||
                    realItem.MaxAC > 0)
                {
                    statDescription += realItem.MinAC > 0 ? string.Format("AC {0}", realItem.MinAC) : "0";
                    statDescription += realItem.MaxAC > 0 ? string.Format(" - {0}", realItem.MaxAC) : "0";
                }
                if (realItem.MinMAC > 0 ||
                    realItem.MaxMAC > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MinMAC > 0 ? string.Format("MAC {0}", realItem.MinMAC) : "0";
                    statDescription += realItem.MaxMAC > 0 ? string.Format(" - {0}", realItem.MaxMAC) : "0";
                }
                if (realItem.MinDC > 0 ||
                    realItem.MaxDC > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MinDC > 0 ? string.Format("DC {0}", realItem.MinDC) : "0";
                    statDescription += realItem.MaxDC > 0 ? string.Format(" - {0}", realItem.MaxDC) : "0";
                }
                if (realItem.MinMC > 0 ||
                    realItem.MaxMC > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MinMC > 0 ? string.Format("MC {0}", realItem.MinMC) : "0";
                    statDescription += realItem.MaxMC > 0 ? string.Format(" - {0}", realItem.MaxMC) : "0";
                }
                if (realItem.MinSC > 0 ||
                    realItem.MaxSC > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MinSC > 0 ? string.Format("SC {0}", realItem.MinSC) : "0";
                    statDescription += realItem.MaxSC > 0 ? string.Format(" - {0}", realItem.MaxSC) : "0";
                }
                if (realItem.Accuracy > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.Accuracy > 0 ? string.Format("Accuracy {0}", realItem.Accuracy) : "";
                }
                if (realItem.Agility > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.Agility > 0 ? string.Format("Agility {0}", realItem.Agility) : "";
                }
                if (realItem.HP > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.HP > 0 ? string.Format("HP {0}", realItem.HP) : "";
                }
                if (realItem.MP > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MP > 0 ? string.Format("MP {0}", realItem.MP) : "";
                }
                if (realItem.HPrate > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.HPrate > 0 ? string.Format("HP Recovery {0}%", realItem.HPrate) : "";
                }
                if (realItem.MPrate > 0)
                {
                    if (statDescription.Length > 0)
                        statDescription += "\r\n";
                    statDescription += realItem.MPrate > 0 ? string.Format("MP Recovery {0}%", realItem.MPrate) : "";
                }
                if (statDescription.Length > 0)
                    count = 1;
            }

            if (count > 0)
            {
                MirLabel RuneDescription = new MirLabel
                {
                    AutoSize = true,
                    ForeColour = Color.Khaki,
                    Location = new Point(4, ItemLabel.DisplayRectangle.Bottom),
                    OutLine = true,
                    Parent = ItemLabel,
                    Text = statDescription
                };

                ItemLabel.Size = new Size(Math.Max(ItemLabel.Size.Width, RuneDescription.DisplayRectangle.Right + 4),
                    Math.Max(ItemLabel.Size.Height, RuneDescription.DisplayRectangle.Bottom));

                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height + 4);

                #region OUTLINE
                MirControl outLine = new MirControl
                {
                    BackColour = Color.FromArgb(255, 50, 50, 50),
                    Border = true,
                    BorderColour = Color.Gray,
                    NotControl = true,
                    Parent = ItemLabel,
                    Opacity = 0.4F,
                    Location = new Point(0, 0)
                };
                outLine.Size = ItemLabel.Size;
                #endregion

                return outLine;
            }
            else
            {
                ItemLabel.Size = new Size(ItemLabel.Size.Width, ItemLabel.Size.Height - 4);
            }
            return null;
        }


        public void CreateItemLabel(UserItem item, bool Inspect = false)
        {
            if (item == null)
            {
                DisposeItemLabel();
                HoverItem = null;
                return;
            }

            if (item != HoverItem)
            {
                DisposeItemLabel();
            }

            if (item == HoverItem && ItemLabel != null && !ItemLabel.IsDisposed) return;
            ushort level = Inspect ? InspectDialog.Level : MapObject.User.Level;
            MirClass job = Inspect ? InspectDialog.Class : MapObject.User.Class;
            HoverItem = item;
            ItemInfo realItem = Functions.GetRealItem(item.Info, level, job, ItemInfoList);

            ItemLabel = new MirControl
            {
                BackColour = Color.FromArgb(255, 50, 50, 50),
                Border = true,
                BorderColour = Color.Gray,
                DrawControlTexture = true,
                NotControl = true,
                Parent = GameScene.Scene,
                Opacity = 0.7F,
                //  Visible = false
            };

            //Name Info Label
            MirControl[] outlines = new MirControl[10];
            outlines[0] = NameInfoLabel(item, Inspect);
            //Attribute Info1 Label - Attack Info
            outlines[1] = AttackInfoLabel(item, Inspect);
            //Attribute Info2 Label - Defence Info
            outlines[2] = DefenceInfoLabel(item, Inspect);
            //Attribute Info3 Label - Weight Info
            outlines[3] = WeightInfoLabel(item, Inspect);
            //Awake Info Label
            outlines[4] = AwakeInfoLabel(item, Inspect);
            //need Info Label
            outlines[5] = NeedInfoLabel(item, Inspect);
            //Bind Info Label
            outlines[6] = BindInfoLabel(item, Inspect);
            //Overlap Info Label
            outlines[7] = OverlapInfoLabel(item, Inspect);
            //Story Label
            outlines[8] = StoryInfoLabel(item, Inspect);

            outlines[9] = SocketInfoLabel(item, Inspect);

            foreach (var outline in outlines)
            {
                if (outline != null)
                {
                    outline.Size = new Size(ItemLabel.Size.Width, outline.Size.Height);
                }
            }

            //ItemLabel.Visible = true;
        }
    }
}
