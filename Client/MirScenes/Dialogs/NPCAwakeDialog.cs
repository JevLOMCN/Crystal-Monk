using System;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.MirNetwork;
using C = ClientPackets;
using System.Text.RegularExpressions;

namespace Client.MirScenes.Dialogs
{
    public sealed class NPCAwakeDialog : MirImageControl
    {

        public MirButton UpgradeButton, CloseButton;
        public MirItemCell[] ItemCells = new MirItemCell[7];
        public MirDropDownBox SelectAwakeType;
        public AwakeType CurrentAwakeType = AwakeType.None;
        public MirLabel GoldLabel, NeedItemLabel1, NeedItemLabel2;

        public static UserItem[] Items = new UserItem[7];
        public static int[] ItemsIdx = new int[7];

        public NPCAwakeDialog()
        {
            Index = 710;
            Library = Libraries.Title;
            Location = new Point(0, 0);
            Sort = true;
            Movable = true;

            GoldLabel = new MirLabel
            {
                AutoSize = true,
                Location = new Point(112, 354),
                Parent = this,
                NotControl = true,
            };

            NeedItemLabel1 = new MirLabel
            {
                AutoSize = true,
                Location = new Point(67, 317),//
                Parent = this,
                NotControl = true,
            };

            NeedItemLabel2 = new MirLabel
            {
                AutoSize = true,
                Location = new Point(192, 317),//(155, 316),
                Parent = this,
                NotControl = true,
            };

            UpgradeButton = new MirButton
            {
                HoverIndex = 713,
                Index = 712,
                Location = new Point(115, 391), //new Point(181, 135),
                Library = Libraries.Title,
                Parent = this,
                PressedIndex = 714,
                Sound = SoundList.ButtonA,
            };
            UpgradeButton.Click += (o, e) => Awakening();

            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(284, 4),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };
            CloseButton.Click += (o, e) => Hide();

            ItemCells[0] = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(202, 91),
                ItemSlot = 0,
            };
            //ItemCells[0].AfterDraw += (o, e) => ItemCell_AfterDraw();
            //ItemCells[0].Click += (o, e) => ItemCell_Click();

            ItemCells[1] = new MirItemCell //Required
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(31, 316),
                ItemSlot = 1,
                Enabled = false,

            };

            ItemCells[2] = new MirItemCell //Required
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(155, 316),
                ItemSlot = 2,
                Enabled = false,
            };

            ItemCells[3] = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(175, 199),
                ItemSlot = 3,
            };

            ItemCells[4] = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(230, 199),
                ItemSlot = 4,
            };

            ItemCells[5] = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(175, 256),
                ItemSlot = 5,
            };

            ItemCells[6] = new MirItemCell
            {
                BorderColour = Color.Lime,
                GridType = MirGridType.AwakenItem,
                Library = Libraries.Items,
                Parent = this,
                Location = new Point(230, 256),
                ItemSlot = 6,
            };

            SelectAwakeType = new MirDropDownBox()
            {
                Parent = this,
                Location = new Point(35, 141),
                Size = new Size(109, 14),
                ForeColour = Color.White,
                Visible = true,
                Enabled = true,
            };
            SelectAwakeType.ValueChanged += (o, e) => OnAwakeTypeSelect(SelectAwakeType._WantedIndex);
        }

        public void ItemCellClear()
        {
            if (ItemCells[1].Item != null)
            {
                ItemCells[1].Item = null;
            }
            if (ItemCells[2].Item != null)
            {
                ItemCells[2].Item = null;
            }


            NeedItemLabel2.Text = "";
            NeedItemLabel1.Text = "";
            GoldLabel.Text = "";
        }

        public void ItemCell_Click()
        {
            ItemCellClear();
            SelectAwakeType.Items.Clear();

            if (Items[0] == null)
            {
                SelectAwakeType.Items.Add(CMain.Tr("Select Upgrade Item."));
                SelectAwakeType.SelectedIndex = SelectAwakeType.Items.Count - 1;
                CurrentAwakeType = AwakeType.None;
            }
            else
            {
                if (Items[0].Awake.getAwakeLevel() == 0)
                {
                    SelectAwakeType.Items.Add(CMain.Tr("Select Upgrade Type."));
                    if (Items[0].Info.Type == ItemType.Weapon)
                    {
                        SelectAwakeType.Items.Add(CMain.Tr("Bravery Glyph"));
                        SelectAwakeType.Items.Add(CMain.Tr("Magic Glyph"));
                        SelectAwakeType.Items.Add(CMain.Tr("Soul Glyph"));
                    }
                    else if (Items[0].Info.Type == ItemType.Helmet)
                    {
                        SelectAwakeType.Items.Add(CMain.Tr("Protection Glyph"));
                        SelectAwakeType.Items.Add(CMain.Tr("EvilSlayer Glyph"));
                    }
                    else
                    {
                        SelectAwakeType.Items.Add(CMain.Tr("Body Glyph"));
                    }
                }
                else
                {
                    SelectAwakeType.Items.Add(CMain.Tr(getAwakeTypeText(Items[0].Awake.type)));
                    if (CurrentAwakeType != Items[0].Awake.type)
                    {
                        CurrentAwakeType = Items[0].Awake.type;
                        OnAwakeTypeSelect(0);
                    }
                }
            }
        }

        public string getAwakeTypeText(AwakeType type)
        {
            string typeName = "";
            switch (type)
            {
                case AwakeType.DC:
                    typeName = "Bravery Glyph";
                    break;
                case AwakeType.MC:
                    typeName = "Magic Glyph";
                    break;
                case AwakeType.SC:
                    typeName = "Soul Glyph";
                    break;
                case AwakeType.AC:
                    typeName = "Protection Glyph";
                    break;
                case AwakeType.MAC:
                    typeName = "EvilSlayer Glyph";
                    break;
                case AwakeType.HPMP:
                    typeName = "Body Glyph";
                    break;
                default:
                    typeName = "Select Upgrade Item.";
                    break;
            }
            return typeName;
        }

        public AwakeType getAwakeType(string typeName)
        {
            AwakeType type = AwakeType.None;
            if (typeName == CMain.Tr("Bravery Glyph"))
            {
                type = AwakeType.DC;
            }
            else if (typeName == CMain.Tr("Magic Glyph"))
            {
                type = AwakeType.MC;
            }
            else if (typeName == CMain.Tr("Soul Glyph"))
            {
                type = AwakeType.SC;
            }
            else if (typeName == CMain.Tr("Protection Glyph"))
            {
                type = AwakeType.AC;
            }
            else if (typeName == CMain.Tr("EvilSlayer Glyph"))
            {
                type = AwakeType.MAC;
            }
            else if (typeName == CMain.Tr("Body Glyph"))
            {
                type = AwakeType.HPMP;
            }
            else
            {
                type = AwakeType.None;
            }

            return type;
        }

        public void OnAwakeTypeSelect(int Index)
        {
            SelectAwakeType.SelectedIndex = Index;

            AwakeType type = getAwakeType(SelectAwakeType.Items[SelectAwakeType.SelectedIndex]);
            CurrentAwakeType = type;
            if (type != AwakeType.None)
            {
                Network.Enqueue(new C.AwakeningNeedMaterials { UniqueID = Items[0].UniqueID, Type = type });
            }
        }

        public void SetNeedItems(ItemInfo[] Materials, byte[] MaterialsCount)
        {
            if (MaterialsCount[0] != 0)
            {
                ItemCells[1].Item = new UserItem(Materials[0]);
                ItemCells[1].Item.Count = MaterialsCount[0];
                NeedItemLabel1.Text = Regex.Replace(ItemCells[1].Item.Info.Name, @"[\d-]", string.Empty) + CMain.Tr("\nQuantity: ") + MaterialsCount[0].ToString();
            }
            else
            {
                ItemCells[1].Item = null;
                NeedItemLabel1.Text = "";
            }

            if (MaterialsCount[1] != 0)
            {
                ItemCells[2].Item = new UserItem(Materials[1]);
                ItemCells[2].Item.Count = MaterialsCount[1];
                NeedItemLabel2.Text = Regex.Replace(ItemCells[2].Item.Info.Name, @"[\d-]", string.Empty) + CMain.Tr("\nQuantity:") + MaterialsCount[1].ToString();
            }
            else
            {
                ItemCells[2].Item = null;
                NeedItemLabel2.Text = "";
            }

            if (ItemCells[0].Item != null)
                GoldLabel.Text = ItemCells[0].Item.AwakeningPrice().ToString();
        }

        public bool CheckNeedMaterials()
        {
            int maxEqual = (Items[1] == null || Items[2] == null) ? 1 : 2;
            int equal = 0;
            for (int i = 1; i < 3; i++)
            {
                if (Items[i] == null) continue;
                for (int j = 3; j < 5; j++)
                {
                    if (Items[j] == null) continue;
                    if (Items[i].Info.Name == Items[j].Info.Name &&
                        Items[i].Count <= Items[j].Count)
                        equal++;
                }
            }
            return equal >= maxEqual;
        }

        public void Awakening()
        {
            if (CheckNeedMaterials())
            {
                AwakeType type = getAwakeType(SelectAwakeType.Items[SelectAwakeType.SelectedIndex]);
                ulong materialUID = 0;
                if (Items[5] != null)
                    materialUID = Items[5].UniqueID;

                if (Items[6] != null) 
                    materialUID = Items[6].UniqueID;

                if (type != AwakeType.None)
                {
                    Network.Enqueue(new C.Awakening { UniqueID = Items[0].UniqueID, Type = type, MaterialUniqueID = materialUID });
                    MapControl.AwakeningAction = true;
                }
            }
        }

        public void Hide()
        {
            foreach (var item in ItemCells)
            {
                if (item.Item != null)
                {
                    Network.Enqueue(new C.AwakeningLockedItem { UniqueID = item.Item.UniqueID, Locked = false });
                    item.Item = null;
                }
            }

            for (int i = 0; i < ItemsIdx.Length; i++)
            {
                ItemsIdx[i] = 0;
            }

            ItemCell_Click();
            Visible = false;
        }

        public void Show()
        {
            Visible = true;

            GameScene.Scene.InventoryDialog.Location = new Point(Size.Width + 5, 0);
            GameScene.Scene.InventoryDialog.Show();
        }
    }
}


