using System;
using System.Windows.Forms;

namespace Client
{
    public class KeyInfo
    {
        public byte RequireCtrl;

        public byte RequireShift;

        public byte RequireAlt;

        public byte RequireTilde;

        public Keys Key;

        public KeyInfo()
        {
            RequireCtrl = 0;
            RequireShift = 0;
            RequireAlt = 0;
            RequireTilde = 0;
            Key = Keys.None;
        }

        public KeyInfo(KeyInfo info)
        {
            RequireCtrl = info.RequireCtrl;
            RequireShift = info.RequireShift;
            RequireAlt = info.RequireAlt;
            RequireTilde = info.RequireTilde;
            Key = info.Key;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                (RequireCtrl == 1) ? "Ctrl + " : "",
                (RequireShift == 1) ? "Shift + " : "",
                (RequireAlt == 1) ? "Alt + " : "",
                (RequireTilde == 1) ? "Tilde + " : "",
                 Key.ToString()
            });
        }

        public void Reset(bool isOverlap)
        {
            if (isOverlap)
            {
                RequireCtrl = 2;
                RequireShift = 2;
                RequireAlt = 2;
                RequireTilde = 2;
            }
            else
            {
                RequireCtrl = 0;
                RequireShift = 0;
                RequireAlt = 0;
                RequireTilde = 0;
            }
            Key = Keys.None;
        }
    }
}
