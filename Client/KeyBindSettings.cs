using System.IO;
using System;
using Client.MirSounds;
using System.Windows.Forms;
using System.Collections.Generic;


namespace Client
{

    public enum KeybindOptions : int
    {
        Bar1Skill1 = 0,
        Bar1Skill2,
        Bar1Skill3,
        Bar1Skill4,
        Bar1Skill5,
        Bar1Skill6,
        Bar1Skill7,
        Bar1Skill8,
        Bar2Skill1,
        Bar2Skill2,
        Bar2Skill3,
        Bar2Skill4,
        Bar2Skill5,
        Bar2Skill6,
        Bar2Skill7,
        Bar2Skill8,
        Inventory,
        Inventory2,
        Equipment,
        Equipment2,
        Skills,
        Skills2,
        Creature,
        MountWindow,
        Mount,
        Fishing,
        Skillbar,
        Mentor,
        Relationship,
        Friends,
        Guilds,
        GameShop,
        Quests,
        Closeall,
        Options,
        Options2,
        Group,
        Belt,
        BeltFlip,
        Pickup,
        Belt1,
        Belt1Alt,
        Belt2,
        Belt2Alt,
        Belt3,
        Belt3Alt,
        Belt4,
        Belt4Alt,
        Belt5,
        Belt5Alt,
        Belt6,
        Belt6Alt,
        Logout,
        Exit,
        CreaturePickup,
        CreatureAutoPickup,
        Minimap,
        Bigmap,
        Trade,
        Rental,
        ChangeAttackmode,
        AttackmodePeace,
        AttackmodeGroup,
        AttackmodeGuild,
        AttackmodeEnemyguild,
        AttackmodeRedbrown,
        AttackmodeAll,
        ChangePetmode,
        PetmodeBoth,
        PetmodeMoveonly,
        PetmodeAttackonly,
        PetmodeNone,
        Help,
        Autorun,
        Cameramode,
        Screenshot,
        DropView,
        TargetDead,
        Ranking,
        AddGroupMember,
        Assist,
        SwitchSkillBar,
    }

    public class KeyBind
    {
        public KeybindOptions function = KeybindOptions.Bar1Skill1;
        public string Text;

        private KeyInfo _defaultKey;

        private KeyInfo _customkey;

        public KeyInfo DefaultKey
        {
            get
            {
                return _defaultKey;
            }
            set
            {
                _defaultKey = value;
                bool flag = _customkey == null;
                if (flag)
                {
                    _customkey = new KeyInfo(_defaultKey);
                }
            }
        }

        public KeyInfo CutomKey
        {
            get
            {
                bool flag = _customkey == null;
                if (flag)
                {
                    _customkey = new KeyInfo(_defaultKey);
                }
                return _customkey;
            }
            set
            {
                _customkey = value;
            }
        }
        public bool CanOverlap
        {
            get
            {
                KeybindOptions keybindOptions = function;
                bool result;
                switch (keybindOptions)
                {
                    case KeybindOptions.Bar1Skill1:
                    case KeybindOptions.Bar1Skill2:
                    case KeybindOptions.Bar1Skill3:
                    case KeybindOptions.Bar1Skill4:
                    case KeybindOptions.Bar1Skill5:
                    case KeybindOptions.Bar1Skill6:
                    case KeybindOptions.Bar1Skill7:
                    case KeybindOptions.Bar1Skill8:
                    case KeybindOptions.Bar2Skill1:
                    case KeybindOptions.Bar2Skill2:
                    case KeybindOptions.Bar2Skill3:
                    case KeybindOptions.Bar2Skill4:
                    case KeybindOptions.Bar2Skill5:
                    case KeybindOptions.Bar2Skill6:
                    case KeybindOptions.Bar2Skill7:
                    case KeybindOptions.Bar2Skill8:
                        break;
                    default:
                        switch (keybindOptions)
                        {
                            case KeybindOptions.Belt1Alt:
                            case KeybindOptions.Belt2Alt:
                            case KeybindOptions.Belt3Alt:
                            case KeybindOptions.Belt4Alt:
                            case KeybindOptions.Belt5Alt:
                            case KeybindOptions.Belt6Alt:
                                return true;
                            case KeybindOptions.Belt2:
                            case KeybindOptions.Belt3:
                            case KeybindOptions.Belt4:
                            case KeybindOptions.Belt5:
                            case KeybindOptions.Belt6:
                                break;
                            default:
                                break;
                        }
                        result = false;
                        return result;
                }
                result = true;
                return result;
            }
        }
    }


    public class KeyBindSettings
    {
        private static InIReader Reader = new InIReader(@".\KeyBinds.ini");
        public List<KeyBind> Keylist = new List<KeyBind>();
        public KeyBindSettings()
        {
            New();
            if (!File.Exists(@".\KeyBinds.ini"))
            {
                Save();
                return;
            }
            Load();
        }

        public void ResetKey()
        {
            foreach (KeyBind current in Keylist)
            {
                current.CutomKey = new KeyInfo(current.DefaultKey);
            }
        }

        public void Load()
        {
            foreach (KeyBind Inputkey in Keylist)
            {
                Inputkey.CutomKey.RequireAlt = Reader.ReadByte(Inputkey.function.ToString(), "RequireAlt", Inputkey.CutomKey.RequireAlt);
                Inputkey.CutomKey.RequireShift = Reader.ReadByte(Inputkey.function.ToString(), "RequireShift", Inputkey.CutomKey.RequireShift);
                Inputkey.CutomKey.RequireTilde = Reader.ReadByte(Inputkey.function.ToString(), "RequireTilde", Inputkey.CutomKey.RequireTilde);
                Inputkey.CutomKey.RequireCtrl = Reader.ReadByte(Inputkey.function.ToString(), "RequireCtrl", Inputkey.CutomKey.RequireCtrl);
                string Input = Reader.ReadString(Inputkey.function.ToString(), "RequireKey", Inputkey.CutomKey.Key.ToString());
                Enum.TryParse(Input, out Inputkey.CutomKey.Key);
                
            }
        }

        public void Save()
        {
            Reader.Write("Guide", "01", "RequireAlt,RequireShift,RequireTilde,RequireCtrl");
            Reader.Write("Guide", "02", "have 3 options: 0/1/2");
            Reader.Write("Guide", "03", "0 < you cannot have this key pressed to use the function");
            Reader.Write("Guide", "04", "1 < you have to have this key pressed to use this function");
            Reader.Write("Guide", "05", "2 < it doesnt matter if you press this key to use this function");
            Reader.Write("Guide", "06", "by default just use 2, unless you have 2 functions on the same key");
            Reader.Write("Guide", "07", "example: change attack mode (ctrl+h) and help (h)");
            Reader.Write("Guide", "08", "if you set either of those to requireshift 2, then they wil both work at the same time or not work");
            Reader.Write("Guide", "09", "");
            Reader.Write("Guide", "10", "To get the value for RequireKey look at:");
            Reader.Write("Guide", "11", "https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx");
        
            foreach (KeyBind Inputkey in Keylist)
            {
                KeyBindSettings.Reader.Write(Inputkey.function.ToString(), "RequireAlt", Inputkey.CutomKey.RequireAlt);
                KeyBindSettings.Reader.Write(Inputkey.function.ToString(), "RequireShift", Inputkey.CutomKey.RequireShift);
                KeyBindSettings.Reader.Write(Inputkey.function.ToString(), "RequireTilde", Inputkey.CutomKey.RequireTilde);
                KeyBindSettings.Reader.Write(Inputkey.function.ToString(), "RequireCtrl", Inputkey.CutomKey.RequireCtrl);
                KeyBindSettings.Reader.Write(Inputkey.function.ToString(), "RequireKey", Inputkey.CutomKey.Key.ToString());
            }

            Reader.Save();
        }

        public void New()
        {
            KeyBind InputKey;
            InputKey = new KeyBind{ Text = "1号技能栏1", function = KeybindOptions.Bar1Skill1, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F1 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏2", function = KeybindOptions.Bar1Skill2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F2 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏3", function = KeybindOptions.Bar1Skill3, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F3 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏4", function = KeybindOptions.Bar1Skill4, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F4 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏5", function = KeybindOptions.Bar1Skill5, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F5 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏6", function = KeybindOptions.Bar1Skill6, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F6 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏7", function = KeybindOptions.Bar1Skill7, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F7 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "1号技能栏8", function = KeybindOptions.Bar1Skill8, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 0, Key = Keys.F8 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏1", function = KeybindOptions.Bar2Skill1, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F1 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏2", function = KeybindOptions.Bar2Skill2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F2 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏3", function = KeybindOptions.Bar2Skill3, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F3 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏4", function = KeybindOptions.Bar2Skill4, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F4 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏5", function = KeybindOptions.Bar2Skill5, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F5 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏6", function = KeybindOptions.Bar2Skill6, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F6 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏7", function = KeybindOptions.Bar2Skill7, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F7 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "2号技能栏8", function = KeybindOptions.Bar2Skill8, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 0, RequireCtrl = 1, Key = Keys.F8 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "仓库1", function = KeybindOptions.Inventory, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.F9 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "仓库2", function = KeybindOptions.Inventory2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.I } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "装备栏1", function = KeybindOptions.Equipment, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.F10 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "装备栏2", function = KeybindOptions.Equipment2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.C } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "技能栏1", function = KeybindOptions.Skills, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.F11 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "技能栏2", function = KeybindOptions.Skills2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.S } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物", function = KeybindOptions.Creature, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.E } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "坐骑", function = KeybindOptions.MountWindow, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.J } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "骑马", function = KeybindOptions.Mount, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.M } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "钓鱼", function = KeybindOptions.Fishing, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.N } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "技能栏", function = KeybindOptions.Skillbar, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.R } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "师徒", function = KeybindOptions.Mentor, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.W } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "情侣", function = KeybindOptions.Relationship, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.L } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "好友", function = KeybindOptions.Friends, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.F } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "公会", function = KeybindOptions.Guilds, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 0, Key = Keys.G } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "商店", function = KeybindOptions.GameShop, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Y } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "任务", function = KeybindOptions.Quests, DefaultKey = new KeyInfo {RequireAlt = 0, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Q } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "关闭所有", function = KeybindOptions.Closeall, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Escape } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "设置", function = KeybindOptions.Options, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.F12 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "设置2", function = KeybindOptions.Options2, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.O } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "组队", function = KeybindOptions.Group, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.P } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "", function = KeybindOptions.Belt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 0, Key = Keys.Z } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "技能1", function = KeybindOptions.BeltFlip, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 1, Key = Keys.Z } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "拾取", function = KeybindOptions.Pickup, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Tab } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏1", function = KeybindOptions.Belt1, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D1 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏1", function = KeybindOptions.Belt1Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad1 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏2", function = KeybindOptions.Belt2, DefaultKey = new KeyInfo {RequireAlt = 0, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D2 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏2", function = KeybindOptions.Belt2Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad2 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏3", function = KeybindOptions.Belt3, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D3 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏3", function = KeybindOptions.Belt3Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad3 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏4", function = KeybindOptions.Belt4, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D4 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏4", function = KeybindOptions.Belt4Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad4 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏5", function = KeybindOptions.Belt5, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D5 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏5", function = KeybindOptions.Belt5Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad5 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏6", function = KeybindOptions.Belt6, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D6 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "快捷栏6", function = KeybindOptions.Belt6Alt, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.NumPad6 } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "小退", function = KeybindOptions.Logout, DefaultKey = new KeyInfo {RequireAlt = 1, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.X } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "大退", function = KeybindOptions.Exit, DefaultKey = new KeyInfo {RequireAlt = 1, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Q } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物自动拾取", function = KeybindOptions.CreaturePickup, DefaultKey = new KeyInfo {RequireAlt = 0, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.X } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物手动拾取", function = KeybindOptions.CreatureAutoPickup, DefaultKey = new KeyInfo {RequireAlt = 1, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.A } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "小地图", function = KeybindOptions.Minimap, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.V } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "大地图", function = KeybindOptions.Bigmap, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.B } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "交易", function = KeybindOptions.Trade, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.T } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "租赁", function = KeybindOptions.Rental, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 0, Key = Keys.A } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "改变攻击模式", function = KeybindOptions.ChangeAttackmode, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 1, Key = Keys.H } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "和平攻击", function = KeybindOptions.AttackmodePeace, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None }};
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "组队攻击", function = KeybindOptions.AttackmodeGroup, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "行会攻击", function = KeybindOptions.AttackmodeGuild, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "善恶攻击", function = KeybindOptions.AttackmodeEnemyguild, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "红名攻击", function = KeybindOptions.AttackmodeRedbrown, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "全体攻击", function = KeybindOptions.AttackmodeAll, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "改变宠物模式", function = KeybindOptions.ChangePetmode, DefaultKey = new KeyInfo {RequireAlt = 0, RequireShift = 0, RequireTilde = 2, RequireCtrl = 1, Key = Keys.A } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物所有模式", function = KeybindOptions.PetmodeBoth, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物移动模式", function = KeybindOptions.PetmodeMoveonly, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物攻击模式", function = KeybindOptions.PetmodeAttackonly, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "宠物无模式", function = KeybindOptions.PetmodeNone, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.None }};
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "帮助", function = KeybindOptions.Help, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 0, RequireTilde = 2, RequireCtrl = 2, Key = Keys.H } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "自动行走", function = KeybindOptions.Autorun, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.D } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "镜头模式", function = KeybindOptions.Cameramode, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Insert } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "截屏", function = KeybindOptions.Screenshot, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.PrintScreen } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "掉落显示", function = KeybindOptions.DropView, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Tab } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "死亡显示", function = KeybindOptions.TargetDead, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 1, Key = Keys.ControlKey } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "排行榜", function = KeybindOptions.Ranking, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.K } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "组队", function = KeybindOptions.AddGroupMember, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 1, Key = Keys.G } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "辅助", function = KeybindOptions.Assist, DefaultKey = new KeyInfo {RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.U } };
            Keylist.Add(InputKey);
            InputKey = new KeyBind { Text = "切换技能栏", function = KeybindOptions.SwitchSkillBar, DefaultKey = new KeyInfo { RequireAlt = 2, RequireShift = 2, RequireTilde = 2, RequireCtrl = 2, Key = Keys.Oem3 } };
            Keylist.Add(InputKey);
        }

        public string GetKey(KeybindOptions Option)
        {
            string output = "";
            for (int i = 0; i < Keylist.Count; i++ )
            {
                if (Keylist[i].function == Option)
                {
                    if (CMain.InputKeys.Keylist[i].CutomKey.Key == Keys.None) return output;
                    if (CMain.InputKeys.Keylist[i].CutomKey.RequireAlt == 1)
                        output = "Alt";
                    if (CMain.InputKeys.Keylist[i].CutomKey.RequireCtrl == 1)
                        output = output != "" ? output + "\nCtrl" : "Ctrl";
                    if (CMain.InputKeys.Keylist[i].CutomKey.RequireShift == 1)
                        output = output != "" ? output + "\nShift" : "Shift";
                    if (CMain.InputKeys.Keylist[i].CutomKey.RequireTilde == 1)
                        output = output != "" ? output + "\n~" : "~";

                    output = output != "" ? output + "\n" + CMain.InputKeys.Keylist[i].CutomKey.Key.ToString() : CMain.InputKeys.Keylist[i].CutomKey.Key.ToString();
                    return output;
                }
            }
            return "";
        }

        public KeyBind HasKey(KeyBind keyBind)
        {
            KeyBind result;
            foreach (KeyBind current in Keylist)
            {
                bool flag = keyBind == current;
                if (!flag)
                {
                    bool flag2 = current.CutomKey.ToString() == keyBind.CutomKey.ToString();
                    if (flag2)
                    {
                        result = current;
                        return result;
                    }
                }
            }
            result = null;
            return result;
        }

    }

    
}
