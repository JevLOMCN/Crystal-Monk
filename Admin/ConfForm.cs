using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Admin
{
    public partial class ConfForm : Form
    {
        public ConfForm(string name = "NewFile")
        {
            InitializeComponent();

            textBoxName.Text = name;
            textBoxUserName.Text = Settings.UserName;
            textBoxPassword.Text = Settings.Password;
            textBoxIP.Text = Settings.ServerIP;
            textBoxPort.Text = Settings.ServerPort.ToString();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            Settings.ServerIP = textBoxIP.Text;
            short.TryParse(textBoxPort.Text, out Settings.ServerPort);
            Settings.UserName = textBoxUserName.Text;
            Settings.Password = textBoxPassword.Text;

            Settings.Save(textBoxName.Text);
            Dispose();
        }
    }
}
