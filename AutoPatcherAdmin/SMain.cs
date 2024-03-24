using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoPatcherAdmin
{
    public partial class SMain : Form
    {
        public List<FileInformation> OldList, NewList;
        public Queue<FileInformation> OutputList;
        public string packetName;

        public SMain()
        {
            InitializeComponent();

            TextBoxCurrent.Text = Settings.CurrentServerPath;
            TextBoxLastVersion.Text = Settings.LastServerPath;
            TextBoxOutput.Text = Settings.OutPutPath;
            Util.savelog += (string text) =>
            {
                LogTextBox.AppendText(string.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, text));
            };
        }

        public FileInformation GetFileInformation(string fileName)
        {
            FileInfo info = new FileInfo(fileName);

            FileInformation file = new FileInformation
            {
                FileName = fileName.Remove(0, Settings.ClientPath.Length),
                Length = (int)info.Length,
                Creation = info.LastWriteTime
            };

            return file;
        }

        private void GeneratePacket(string root, List<string> files, string output)
        {
            foreach (string file in files)
            {
                string fileName = Path.Combine(root, file);
                if (!File.Exists(fileName))
                {
                    Util.SaveLog(string.Format("not find file {0}", file));
                    continue;
                }

                string dstFileName = Path.Combine(output, file);
                    
                Util.MoveFile(fileName, dstFileName);
            }
        }

        private void TextBoxLastVersion_DragDrop(object sender, DragEventArgs e)
        {
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                TextBoxLastVersion.Text = f;
            }
        }

        private void TextBoxCurrent_DragDrop(object sender, DragEventArgs e)
        {
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                TextBoxCurrent.Text = f;
            }
        }

        private void TextBoxLastVersion_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void TextBoxCurrent_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            ProcessButton.Enabled = false;
            Settings.CurrentServerPath = TextBoxCurrent.Text;
            Settings.LastServerPath = TextBoxLastVersion.Text;

            DateTime dt = DateTime.Now;
            packetName = "mir-server-" + dt.ToString(@"yyyy-MM-dd-HHmmss");

            OldList = new List<FileInformation>();
            NewList = new List<FileInformation>();
            OutputList = new Queue<FileInformation>();

            List<string> diffFiles = Util.Diff(Settings.CurrentServerPath, Settings.LastServerPath);
            Refresh();

            GeneratePacket(Settings.CurrentServerPath, diffFiles, Path.Combine(Settings.OutPutPath, packetName));

            Util.SaveLog(string.Format("生成完成 {0}", packetName));
        }

    }
}
