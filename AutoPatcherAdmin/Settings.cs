using System.IO;

namespace AutoPatcherAdmin
{
    public static class Settings
    {
        private static readonly InIReader Reader = new InIReader(@".\PatchAdmin.ini");
        private static readonly string ignorePath = @".\patch.ignore";

        public static string ClientPath = @".\..\client_packet";
        public static string OutPutPath = @".\output";
        public static bool AllowCleanUp = true;
        public static string DownloadUrl = @"http://127.0.0.1/patch/";
        public static string[] ExcludeList = new string[] { "Thumbs.db", "Client.pdb", "Client.vshost.exe", "Client.exe.config", "Client.instr.pdb", "Client.vshost.exe.config" };


        public static string CurrentServerPath = @".\..\client_packet";
        public static string LastServerPath = @".\..\client_packet";

        public static void Load()
        {
            ClientPath = Reader.ReadString("AutoPatcher", "ClientPath", ClientPath);
            OutPutPath = Reader.ReadString("AutoPatcher", "OutPutPath", OutPutPath);
            DownloadUrl = Reader.ReadString("AutoPatcher", "DownloadUrl", DownloadUrl);
            AllowCleanUp = Reader.ReadBoolean("AutoPatcher", "AllowCleanUp", AllowCleanUp);

            if (File.Exists(ignorePath))
                ExcludeList = File.ReadAllLines(ignorePath);

            CurrentServerPath = Reader.ReadString("AutoPatcher", "CurrentServerPath", CurrentServerPath);
            LastServerPath = Reader.ReadString("AutoPatcher", "LastServerPath", LastServerPath);
        }

        public static void Save()
        {
            Reader.Write("AutoPatcher", "ClientPath", ClientPath);
            Reader.Write("AutoPatcher", "OutPutPath", OutPutPath);
            Reader.Write("AutoPatcher", "DownloadUrl", DownloadUrl);
            Reader.Write("AutoPatcher", "AllowCleanUp", AllowCleanUp);
            Reader.Write("AutoPatcher", "CurrentServerPath", CurrentServerPath);
            Reader.Write("AutoPatcher", "LastServerPath", LastServerPath);
        }
    }
}
