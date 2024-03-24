using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace AutoPatcher
{
    public partial class AMain : Form
    {
        long _totalBytes, _completedBytes, _currentBytes;
        private int _fileCount, _currentCount;

        private FileInformation _currentFile;
        public bool Completed, Checked, ErrorFound;
        
        public List<FileInformation> OldList;
        public Queue<FileInformation> DownloadList;

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        private Thread _workThread;

        public AMain()
        {
            InitializeComponent();
        }

        public void Start()
        {

            OldList = new List<FileInformation>();
            DownloadList = new Queue<FileInformation>();

            byte[] data = Download(Settings.PatchFileName);

            if (data != null)
            {
                using (MemoryStream stream = new MemoryStream(data))
                using (BinaryReader reader = new BinaryReader(stream))
                    ParseOld(reader);
            }
            else
            {
            //    MessageBox.Show("更新列表获取失败.");
                Completed = true;
                return;
            }

            _fileCount = OldList.Count;
            for (int i = 0; i < OldList.Count; i++)
                CheckFile(OldList[i]);

            Checked = true;
            _fileCount = 0;
            _currentCount = 0;


            _fileCount = DownloadList.Count;
            BeginDownload();
        }

        private void BeginDownload()
        {
            if (DownloadList == null) return;

            if (DownloadList.Count == 0)
            {
                DownloadList = null;
                _currentFile = null;
                Completed = true;

                CleanUp();
                return;
            }

            Process[] processes = Process.GetProcessesByName("Client");
            if (processes.Length > 0)
            {
                string patcherPath = Application.StartupPath + @"\Client.exe";

                for (int i = 0; i < processes.Length; i++)
                    if (processes[i].MainModule.FileName == patcherPath)
                        processes[i].Kill();
            }


            _currentFile = DownloadList.Dequeue();

            Download(_currentFile);
        }

        private void CleanUp()
        {

        }

        public bool NeedFile(string fileName)
        {
            for (int i = 0; i < OldList.Count; i++)
            {
                if (fileName.EndsWith(OldList[i].FileName))
                    return true;
            }

            return false;
        }


        public void ParseOld(BinaryReader reader)
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
                OldList.Add(new FileInformation(reader));
        }

        public void CheckFile(FileInformation old)
        {
            FileInformation info = GetFileInformation(Settings.Client + (old.FileName == "AutoPatcher.gz" ? "AutoPatcher.exe" : old.FileName));
            _currentCount++;

            if (info == null || old.Length != info.Length || old.Creation != info.Creation)
            {
                DownloadList.Enqueue(old);
                _totalBytes += old.Length;
            }
        }

        public void Download(FileInformation info)
        {
            string fileName = info.FileName.Replace(@"\", "/");

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (o, e) =>
                    {
                        _currentBytes = e.BytesReceived;
                    };
                    client.DownloadDataCompleted += (o, e) =>
                    {
                        if (e.Error != null)
                        {
                            File.AppendAllText(@".\Error.txt",
                                   string.Format("[{0}] {1}{2}", DateTime.Now, info.FileName + " 下载失败. (" + e.Error.Message + ")", Environment.NewLine));
                            ErrorFound = true;
                        }
                        else
                        {
                            _currentCount++;
                            _completedBytes += _currentBytes;
                            _currentBytes = 0;
                            _stopwatch.Stop();

                            if (!Directory.Exists(Settings.Client + Path.GetDirectoryName(info.FileName)))
                                Directory.CreateDirectory(Settings.Client + Path.GetDirectoryName(info.FileName));

                            if (info.FileName.EndsWith("AutoPatcher.exe"))
                            {
                                File.WriteAllBytes(Settings.Client + info.FileName + ".gz", e.Result);
                                File.SetLastWriteTime(Settings.Client + info.FileName + ".gz", info.Creation);
                            }
                            else
                            {
                                File.WriteAllBytes(Settings.Client + info.FileName, e.Result);
                                File.SetLastWriteTime(Settings.Client + info.FileName, info.Creation);
                            }
                        }
                        BeginDownload();
                    };

                    if (Settings.NeedLogin) client.Credentials = new NetworkCredential(Settings.Login, Settings.Password);


                    _stopwatch = Stopwatch.StartNew();
                    client.DownloadDataAsync(new Uri(Settings.Host + "/" + fileName));
                }
            }
            catch
            {
                MessageBox.Show(string.Format("找不到文件: {0}", fileName));
            }
        }

        public byte[] Download(string fileName)
        {
            fileName = fileName.Replace(@"\", "/");

            if (fileName != "AutoPatcher.gz" && fileName != "PList.gz")
                fileName += Path.GetExtension(fileName);

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    return client.DownloadData(Settings.Host + fileName);
                }
            }
            catch
            {
                return null;
            }
        }

        public FileInformation GetFileInformation(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            FileInfo info = new FileInfo(fileName);
            return new FileInformation
            {
                FileName = fileName.Remove(0, Settings.Client.Length),
                Length = (int)info.Length,
                Creation = info.LastWriteTime
            };
        }

        private void AMain_Load(object sender, EventArgs e)
        {
            PlayButton.Enabled = false;
            _workThread = new Thread(Start) { IsBackground = true };
            _workThread.Start();
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Completed)
                {
                    if (Settings.AutoStart)
                    {
                        Play();
                        return;
                    }
                    ActionLabel.Text = "完成.";
                    SizeLabel.Text = "完成.";
                    FileLabel.Text = "完成.";
                    SpeedLabel.Text = "完成.";
                    progressBar1.Value = 100;
                    progressBar2.Value = 100;
                    PlayButton.Enabled = true;
                    InterfaceTimer.Enabled = false;

                    if (ErrorFound) MessageBox.Show("有文件下载失败, 详情请查看Error.txt.", "下载失败.");
                    ErrorFound = false;
                    return;
                }

                ActionLabel.Text = !Checked ? string.Format("检查文件... {0}/{1}", _currentCount, _fileCount) : string.Format("下载... {0}/{1}", _currentCount, _fileCount);
                SizeLabel.Text = string.Format("{0:#,##0} MB / {1:#,##0} MB", (_completedBytes + _currentBytes) * 1.0 / 1024 / 1024, _totalBytes * 1.0 / 1024 / 1024);

                if (_currentFile != null)
                {
                    FileLabel.Text = string.Format("{0}, ({1:#,##0} KB) / ({2:#,##0} KB)", _currentFile.FileName, _currentBytes / 1024, _currentFile.Length / 1024);
                    SpeedLabel.Text = (_currentBytes / 1024F / _stopwatch.Elapsed.TotalSeconds).ToString("#,##0.##") + "KB/s";
                    progressBar2.Value = (int)(100 * _currentBytes / _currentFile.Compressed);
                }
                if (_totalBytes > 0)
                    progressBar1.Value = (int)(100 * (_completedBytes + _currentBytes) / _totalBytes);
            }
            catch
            {
                
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            Play();
        }

        public void Play()
        {
            if (File.Exists(Settings.Client + "Client.exe"))
            {
                string fileName = Application.StartupPath + "\\" + "Client.exe";
                Process.Start(fileName, "-noupdate");
            //    ProcessStartInfo info = new ProcessStartInfo(Settings.Client + "Client.exe") { WorkingDirectory = Settings.Client };
             //   Process.Start(info);
            }
            else
                MessageBox.Show(Settings.Client + "找不到Client.exe.", "找不到Client.exe.");
            Close();
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
