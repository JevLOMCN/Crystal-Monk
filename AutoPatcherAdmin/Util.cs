using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoPatcherAdmin
{
    class Util
    {
        public delegate void SaveLogHandler(string text);

        public static event SaveLogHandler savelog;

        public static bool InExcludeList(List<FileInformation> OldList, string fileName)
        {
            if (OldList != null)
            {
                for (int i = 0; i < OldList.Count; ++i)
                {
                    if (fileName == OldList[i].FileName)
                        return false;
                }
            }

            foreach (var item in Settings.ExcludeList)
            {
                if (item.StartsWith("*") && fileName.EndsWith(item.Substring(1)))
                    return true;

                if (fileName.StartsWith(item))
                    return true;

                if (fileName.EndsWith(item)) return true;
            }

            return false;
        }

        public static bool InExcludeList(string fileName)
        {
            return InExcludeList(null, fileName);
        }

        public static void CreateDirectoryIfNeeded(string directory)
        {
            string fulldirectory = "";
            char[] splitChar = { '\\' };
            string[] DirectoryList = directory.Split(splitChar);

            foreach (string directoryCheck in DirectoryList)
            {
                fulldirectory += directoryCheck + "\\";
                try
                {
                    if (!Directory.Exists(fulldirectory))
                        Directory.CreateDirectory(fulldirectory);
                }
                catch (Exception ex)
                {
                    SaveLog(ex.ToString());
                }
            }
        }

        public static void MoveFile(string src, string dst)
        {
            byte[] raw = File.ReadAllBytes(src);
            CreateDirectoryIfNeeded(Path.GetDirectoryName(dst));
            File.Delete(dst);
            FileStream fs = new FileStream(dst, FileMode.Create);
            fs.Write(raw, 0, raw.Length);
            fs.Flush();
            fs.Close();
        }

        public static void SaveLog(string text)
        {
            File.AppendAllText(@".\log.txt",
                   string.Format("[{0}] {1}{2}", DateTime.Now, text, Environment.NewLine));

            savelog(text);
        }

        public static List<string> Diff(string currentServerPath, string lastServerPath)
        {
            List<string> output = new List<string>();
            string[] lastFiles = Directory.GetFiles(Settings.LastServerPath, "*.*", SearchOption.AllDirectories);
            string[] currentFiles = Directory.GetFiles(Settings.CurrentServerPath, "*.*", SearchOption.AllDirectories);
            string currentRoot = currentServerPath.Replace("/", "\\");
            string lastRoot = lastServerPath.Replace("/", "\\");

            for (int i = 0; i < currentFiles.Length; ++i)
            {
                string fileName = currentFiles[i].Substring(currentRoot.Length + 1);
                if (InExcludeList(fileName))
                    continue;

                bool diff = true;
                for (int j = 0; j < lastFiles.Length; ++j)
                {
                    string fileName1 = lastFiles[j].Substring(lastRoot.Length + 1);
                    if (fileName.Equals(fileName1))
                    {
                        if (IsSameFile(lastFiles[j], currentFiles[i]))
                        {
                            diff = false;
                        }
                        else
                        {
                            SaveLog(string.Format("diff: {0} {1}", fileName, fileName1));
                            diff = true;
                        }

                        break;
                    }
                }

                if (diff)
                {
                    output.Add(fileName);
                }
            }

            return output;
        }

        private static bool IsSameFile(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2))
                return false;

            return BytesEuals(File.ReadAllBytes(file1), File.ReadAllBytes(file2));
            //FileInfo info1 = new FileInfo(file1);
            //FileInfo info2 = new FileInfo(file2);

            //return info1.CreationTime != info2.CreationTime;
        }

        private static bool BytesEuals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }
    }
}
