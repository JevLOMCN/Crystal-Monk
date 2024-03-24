using Client.MirControls;
using Client.MirGraphics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Client.MirScenes.Dialogs
{
    public sealed class KeyBindObject : MirControl
    {
        public MirLabel Title;

        public MirLabel DefaultKey;

        public MirButton AnySettingKey;

        private KeyInfo tempKeyInfo;

        private KeyBind _key;

        public KeyBind Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                Title.Text = _key.Text;
                RefreshKey();
            }
        }

        public KeyBindObject()
        {
            Title = new MirLabel
            {
                Text = "test",
                Location = new Point(0, 0),
                Parent = this,
                AutoSize = true,
                DrawFormat = TextFormatFlags.Default,
                Font = new Font(Settings.FontName, 9f),
                ForeColour = Color.White,
                NotControl = true
            };
            DefaultKey = new MirLabel
            {
                Text = "test",
                Location = new Point(210, 0),
                Parent = this,
                AutoSize = true,
                DrawFormat = TextFormatFlags.Default,
                Font = new Font(Settings.FontName, 9f),
                ForeColour = Color.White,
                NotControl = true
            };
            AnySettingKey = new MirButton
            {
                Parent = this,
                Location = new Point(340, 0),
                Library = Libraries.Prguse,
                Index = 1811,
                HoverIndex = 1810,
                PressedIndex = 1810
            };
            AnySettingKey.Click += new EventHandler(AnySettingKey_Clicked);
            AnySettingKey.CenterText = true;
        }

        public void SaveKey()
        {
            KeyBind equal = CMain.InputKeys.HasKey(Key);
            bool flag = equal != null;
            if (flag)
            {
                MirMessageBox mirMessageBox = new MirMessageBox(CMain.Format("[{0}]You are using the key[{1}] on the keyboard.\nDo you want to change it?", equal.Text, equal.CutomKey.ToString()), MirMessageBoxButtons.YesNo);
                mirMessageBox.Show();
                mirMessageBox.YesButton.Click += (o, e) =>
                {
                    equal.CutomKey.Reset(equal.CanOverlap);
                    GameScene.Scene.KeyboardLayoutDialog.RefreshKeys();
                };
                mirMessageBox.NoButton.Click += (o, e) =>
                {
                    CancelKey();
                };
            }
            else
            {
                RefreshKey();
            }
        }

        public void CancelKey()
        {
            Key.CutomKey = new KeyInfo(tempKeyInfo);
            tempKeyInfo = null;
            RefreshKey();
        }

        public void RefreshKey()
        {
            AnySettingKey.Index = 1811;
            DefaultKey.Text = string.Concat(new string[]
            {
                (_key.DefaultKey.RequireAlt == 1) ? "Alt + " : "",
                (_key.DefaultKey.RequireCtrl == 1) ? "Ctrl + " : "",
                (_key.DefaultKey.RequireShift == 1) ? "Shift + " : "",
                (_key.DefaultKey.RequireTilde == 1) ? "~ + " : "",
                 _key.DefaultKey.Key.ToString()
            });
            AnySettingKey.Text = string.Concat(new string[]
            {
                (_key.CutomKey.RequireAlt == 1) ? "Alt + " : "",
                (_key.CutomKey.RequireCtrl == 1) ? "Ctrl + " : "",
                (_key.CutomKey.RequireShift == 1) ? "Shift + " : "",
                (_key.CutomKey.RequireTilde == 1) ? "~ + " : "",
                 _key.CutomKey.Key.ToString()
            });
        }

        public void AnySettingKey_Clicked(object o, EventArgs e)
        {
            bool flag = GameScene.Scene.KeyboardLayoutDialog.ActiveKey != null;
            if (!flag)
            {
                tempKeyInfo = new KeyInfo(Key.CutomKey);
                GameScene.Scene.KeyboardLayoutDialog.PressKeyText.Visible = true;
                GameScene.Scene.KeyboardLayoutDialog.ActiveKey = this;
                GameScene.Scene.KeyboardLayoutDialog.InputKey.Text = string.Empty;
                GameScene.Scene.KeyboardLayoutDialog.InputKey.Visible = true;
                GameScene.Scene.KeyboardLayoutDialog.InputKey.SetFocus();
                GameScene.Scene.KeyboardLayoutDialog.InputKey.TextBox.ImeMode = ImeMode.Disable;
                AnySettingKey.Index = 1810;
                Key.CutomKey.Reset(Key.CanOverlap);
            }
        }

        public void Show()
        {
            bool visible = Visible;
            if (!visible)
            {
                Visible = true;
                AnySettingKey.Visible = true;
                Title.Visible = true;
                DefaultKey.Visible = true;
            }
        }

        public void Hide()
        {
            bool flag = !this.Visible;
            if (!flag)
            {
                Visible = false;
                AnySettingKey.Visible = false;
                Title.Visible = false;
                DefaultKey.Visible = false;
            }
        }

    }
}
