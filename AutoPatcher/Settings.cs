using System;
using System.Windows.Forms;

namespace AutoPatcher
{
    public static class Settings
    {
        private static readonly InIReader Reader = new InIReader(@".\Mir2Test.ini");

        public static string Host = @""; //ftp://212.67.209.184
        public static string PatchFileName = @"PList.gz";

        public static bool NeedLogin  =false;
        public static string Login = string.Empty;
        public static string Password = string.Empty;
        public static bool AllowCleanUp = true;

        public static string Client;
        public static bool AutoStart;

        public static void Load()
        {

            Host = Reader.ReadString("Launcher", "Host", Host);
            PatchFileName = Reader.ReadString("Launcher", "PatchFile", PatchFileName);

            NeedLogin = Reader.ReadBoolean("Launcher", "NeedLogin", NeedLogin);
            Login = Reader.ReadString("Launcher", "Login", Login);
            Password = Reader.ReadString("Launcher", "Password", Password);

            AllowCleanUp = Reader.ReadBoolean("Launcher", "AllowCleanUp", AllowCleanUp);
            AutoStart = Reader.ReadBoolean("Launcher", "AutoStart", AutoStart);

            Client = Application.StartupPath + "\\";
            if (!Host.EndsWith("/")) Host += "/";
            if (Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) Host = Host.Insert(0, "http://");
        }
    }
}
