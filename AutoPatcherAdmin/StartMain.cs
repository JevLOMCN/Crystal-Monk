using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoPatcherAdmin
{
    public partial class StartMain : Form
    {
        public StartMain()
        {
            InitializeComponent();
        }

        private void ButtonClient_Click(object sender, EventArgs e)
        {
            AMain form = new AMain();

            form.ShowDialog();
        }

        private void ButtonServer_Click(object sender, EventArgs e)
        {
            SMain form = new SMain();
            form.ShowDialog();
        }
    }
}
