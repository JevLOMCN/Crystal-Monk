using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoPatcherAdmin
{
    public partial class AMain : Form
    {
        public const string PatchFileName = @"PList.gz";

        public string packetName = "";

        public List<FileInformation> OldList, NewList;
        public Queue<FileInformation> UploadList;
        private Stopwatch _stopwatch = Stopwatch.StartNew();

        long _totalBytes, _completedBytes;

        public AMain()
        {
            InitializeComponent();

            ClientTextBox.Text = Settings.ClientPath;
            OutputTextBox.Text = Settings.OutPutPath;
            DowloadTextBox.Text = Settings.DownloadUrl;
            AllowCleanCheckBox.Checked = Settings.AllowCleanUp;
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessButton.Enabled = false;
                Settings.ClientPath = ClientTextBox.Text;
                Settings.OutPutPath = OutputTextBox.Text;
                Settings.AllowCleanUp = AllowCleanCheckBox.Checked;
                Settings.DownloadUrl = DowloadTextBox.Text;

                DateTime dt = DateTime.Now;
                packetName = "mir-" + dt.ToString(@"yyyy-MM-dd-HHmmss");

                OldList = new List<FileInformation>();
                NewList = new List<FileInformation>();
                UploadList = new Queue<FileInformation>();

                // 下载文件信息列表
                byte[] data = Download(PatchFileName);

                if (data != null)
                {
                    using (MemoryStream stream = new MemoryStream(data))
                    using (BinaryReader reader = new BinaryReader(stream))
                        ParseOld(reader);
                }
                else
                {
                    SaveLog("No patch in server");
                }

                SaveLog("Checking Files...");
                Refresh();
                
                // 获取新的文件信息列表
                CheckFiles();

                for (int i = 0; i < NewList.Count; i++)
                {
                    FileInformation info = NewList[i];
                    if (NeedUpdate(info))
                    {
                        // 比对时间，大小
                        UploadList.Enqueue(info);
                    }
                    else
                    {
                        for (int o = 0; o < OldList.Count; o++)
                        {
                            if (OldList[o].FileName != info.FileName) continue;
                            NewList[i] = OldList[o];
                            break;
                        }
                    }
                }

                //   BeginUpload();
                //  移动到生成目录
                new Task(GeneratePacket).Start();
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString());
                ProcessButton.Enabled = true;
                SaveLog("Error...");
            }
        }

        private void GeneratePacket()
        {
            try
            {
                if (UploadList == null) return;

                int count = 0;
                foreach (FileInformation info in UploadList)
                {
                    MoveFile(info, File.ReadAllBytes(Settings.ClientPath + (info.FileName == "AutoPatcher.gz" ? "AutoPatcher.exe" : info.FileName)));
                    _totalBytes += info.Length;
                    SaveLog(string.Format("文件： {0}/{1} {2} Creation:{3} Size:{4}KB", ++count, UploadList.Count, info.FileName, info.Creation, info.Length / 1024));
                }
                SaveLog(string.Format("总共更新 {0} 个文件 {1:#,##0}MB", UploadList.Count, _totalBytes / 1024));

                MoveFile(new FileInformation { FileName = PatchFileName }, CreateNew());
                UploadList = null;
                SaveLog("Complete...");
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                SaveLog("Error...");
            }
            finally
            {
                ProcessButton.Enabled = true;
            }
        }

        public void MoveFile(FileInformation info, byte[] raw, bool retry = true)
        {
            _completedBytes += info.Length;
            string fileName = info.FileName.Replace(@"\", "/");

       //     if (fileName != "AutoPatcher.gz" && fileName != "PList.gz")
         //       fileName += ".gz";

        //    byte[] data = Compress(raw);

            string filePath = Settings.OutPutPath;
            filePath = filePath.Replace(@"\", "/");
            if (!filePath.EndsWith("/"))
                filePath += "/";

            filePath += packetName + "/";

            filePath += fileName;

            Util.CreateDirectoryIfNeeded(Path.GetDirectoryName(filePath));
            File.Delete(filePath);
            FileStream fs = new FileStream(filePath, FileMode.Create);
            fs.Write(raw, 0, raw.Length);
            fs.Flush();
            fs.Close();
        }


        public bool NeedFile(string fileName)
        {
            for (int i = 0; i < NewList.Count; i++)
            {
                if (fileName.EndsWith(NewList[i].FileName) && !InExcludeList(NewList[i].FileName))
                    return true;
            }

            return false;
        }

        public void ParseOld(BinaryReader reader)
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                FileInformation fileInfo = new FileInformation(reader);
                OldList.Add(fileInfo);
                SaveLog(string.Format("Old:{0}/{1} {2}", i + 1, count, fileInfo.FileName));
            }
        }

        public byte[] CreateNew()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(NewList.Count);
                for (int i = 0; i < NewList.Count; i++)
                {
                    NewList[i].Save(writer);
                    SaveLog(string.Format("{0}：{1}/{2} {3} Creation:{4} Size:{5}KB", PatchFileName, i+1, NewList.Count, NewList[i].FileName, NewList[i].Creation, NewList[i].Length/ 1024));
                }

                return stream.ToArray();
            }
        }

        public void SaveLog(string text)
        {
             File.AppendAllText(@".\log.txt",
                    string.Format("[{0}] {1}{2}", DateTime.Now, text, Environment.NewLine));
            LogTextBox.AppendText(string.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, text));
        }

        public void CheckFiles()
        {
            string[] files = Directory.GetFiles(Settings.ClientPath, "*.*" ,SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                FileInformation info = GetFileInformation(files[i]);
                if (InExcludeList(info.FileName)) continue;
                NewList.Add(info);
            }

            for (int i = 0; i < OldList.Count; ++i)
            {
                if (InExcludeList(OldList[i].FileName)) continue;
                bool find = false;
                for (int j=0; j<NewList.Count; ++j)
                {
                    if (NewList[j].FileName == OldList[i].FileName)
                        find = true;
                }

                if (!find)
                    NewList.Add(OldList[i]);
            }
        }

        public bool InExcludeList(string fileName)
        {
            return Util.InExcludeList(OldList, fileName);
        }

        public bool NeedUpdate(FileInformation info)
        {
            for (int i = 0; i < OldList.Count; i++)
            {
                FileInformation old = OldList[i];
                if (old.FileName != info.FileName) continue;

                if (old.Length != info.Length) return true;
                if (old.Creation != info.Creation) return true;

                return false;
            }
            return true;
        }

        public FileInformation GetFileInformation(string fileName)
        {
            FileInfo info = new FileInfo(fileName);

            FileInformation file =  new FileInformation
                {
                    FileName = fileName.Remove(0, Settings.ClientPath.Length),
                    Length = (int) info.Length,
                    Creation = info.LastWriteTime
                };

            if (file.FileName == "AutoPatcher.exe")
                file.FileName = "AutoPatcher.gz";

            return file;
        }


        public byte[] Download(string fileName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    return client.DownloadData(Settings.DownloadUrl + "/" + fileName);
                }
            }
            catch
            {
                return null;
            }
        }
       
        private void AMain_Load(object sender, EventArgs e)
        {

        }
    }

    public class FileInformation
    {
        public string FileName; //Relative.
        public int Length, Compressed;
        public DateTime Creation;

        public FileInformation()
        {

        }
        public FileInformation(BinaryReader reader)
        {
            FileName = reader.ReadString();
            Length = reader.ReadInt32();
            Compressed = reader.ReadInt32();
            Creation = DateTime.FromBinary(reader.ReadInt64());
        }
        public void Save(BinaryWriter writer)
        {
            writer.Write(FileName);
            writer.Write(Length);
            writer.Write(Compressed);
            writer.Write(Creation.ToBinary());
        }
    }
}
