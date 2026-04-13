using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Diagnostics;
using Horizon.Server;
using Horizon.Functions;

namespace Horizon.Forms
{
    public partial class About : Office2007RibbonForm
    {
        public About()
        {
            InitializeComponent();
            MdiParent = Main.mainForm;
            Tag = true;
            panelVersion.Text += Config.clientVersion;

        }

        private void About_FormClosing(object sender, FormClosingEventArgs e)
        {
            Tag = false;
        }

        private void btnMyGithub_Click(object sender, EventArgs e)
        {
            btnMyGithub.Enabled = false;
            Process.Start("https://github.com/mzzvxm/");
            btnMyGithub.Enabled = true;
        }

        private void btnSourceGithub_Click(object sender, EventArgs e)
        {
            btnSourceGithub.Enabled = false;
            Process.Start("https://github.com/unknownv2/");
            btnSourceGithub.Enabled = true;
        }

        int x = new int();
        private void pbLogo_Click(object sender, EventArgs e)
        {
            switch (x++)
            {
                case 3:
                    UI.messageBox("WtF iS YoU DOiN? STOP H4X0rZINgz PLZ!!!", "H4x0Rz", MessageBoxIcon.Exclamation);
                    break;
                case 8:
                    UI.messageBox("OMGz i WaRNeD YA!", "LOLz", MessageBoxIcon.Error);
                    this.Close();
                    break;
            }
        }
    }
}