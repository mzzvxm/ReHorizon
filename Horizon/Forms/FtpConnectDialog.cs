using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Horizon.Forms
{
    internal sealed class FtpConnectDialog : Form
    {
        private readonly TextBox txtHost = new TextBox();
        private readonly NumericUpDown numPort = new NumericUpDown();
        private readonly TextBox txtUser = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtRoots = new TextBox();
        private readonly CheckBox ckShowPassword = new CheckBox();
        private readonly Label lblStatus = new Label();
        private readonly ProgressBar progress = new ProgressBar();
        private readonly Button cmdOk = new Button();
        private readonly Button cmdCancel = new Button();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private bool closingAfterSuccess;

        internal FtpConnectDialog()
        {
            Text = "Connect FTP";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(410, 275);

            Label lblHost = new Label() { Left = 12, Top = 16, Width = 120, Text = "Host / IP" };
            txtHost.Left = 12;
            txtHost.Top = 34;
            txtHost.Width = 286;

            Label lblPort = new Label() { Left = 308, Top = 16, Width = 90, Text = "Port" };
            numPort.Left = 308;
            numPort.Top = 34;
            numPort.Width = 90;
            numPort.Minimum = 1;
            numPort.Maximum = 65535;
            numPort.Value = 21;

            Label lblUser = new Label() { Left = 12, Top = 66, Width = 120, Text = "Username" };
            txtUser.Left = 12;
            txtUser.Top = 84;
            txtUser.Width = 188;
            txtUser.Text = "xbox";

            Label lblPassword = new Label() { Left = 210, Top = 66, Width = 120, Text = "Password" };
            txtPassword.Left = 210;
            txtPassword.Top = 84;
            txtPassword.Width = 188;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "xbox";

            ckShowPassword.Left = 210;
            ckShowPassword.Top = 110;
            ckShowPassword.Width = 120;
            ckShowPassword.Text = "Show password";
            ckShowPassword.CheckedChanged += new EventHandler(ckShowPassword_CheckedChanged);

            Label lblRoots = new Label() { Left = 12, Top = 138, Width = 210, Text = "Roots (comma-separated)" };
            txtRoots.Left = 12;
            txtRoots.Top = 156;
            txtRoots.Width = 386;
            txtRoots.Text = "Hdd1:,Usb0:,Usb1:";

            Label lblHint = new Label();
            lblHint.Left = 12;
            lblHint.Top = 184;
            lblHint.Width = 386;
            lblHint.Height = 18;
            lblHint.Text = "Ex.: IP 192.168.0.15, porta 21, roots Hdd1:, Usb0:, Usb1:";

            lblStatus.Left = 12;
            lblStatus.Top = 208;
            lblStatus.Width = 386;
            lblStatus.Height = 18;
            lblStatus.Text = "Ready to connect.";

            progress.Left = 12;
            progress.Top = 230;
            progress.Width = 386;
            progress.Height = 12;
            progress.Style = ProgressBarStyle.Blocks;

            cmdOk.Text = "Connect";
            cmdOk.Left = 242;
            cmdOk.Top = 248;
            cmdOk.Width = 75;
            cmdOk.DialogResult = DialogResult.OK;

            cmdCancel.Text = "Cancel";
            cmdCancel.Left = 323;
            cmdCancel.Top = 248;
            cmdCancel.Width = 75;
            cmdCancel.DialogResult = DialogResult.Cancel;

            AcceptButton = cmdOk;
            CancelButton = cmdCancel;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            Controls.Add(lblHost);
            Controls.Add(txtHost);
            Controls.Add(lblPort);
            Controls.Add(numPort);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(ckShowPassword);
            Controls.Add(lblRoots);
            Controls.Add(txtRoots);
            Controls.Add(lblHint);
            Controls.Add(lblStatus);
            Controls.Add(progress);
            Controls.Add(cmdOk);
            Controls.Add(cmdCancel);
        }

        internal Func<FtpConnectDialog, string> ConnectHandler { get; set; }

        internal string Host
        {
            get { return txtHost.Text.Trim(); }
        }

        internal int Port
        {
            get { return (int)numPort.Value; }
        }

        internal string Username
        {
            get { return txtUser.Text.Trim(); }
        }

        internal string Password
        {
            get { return txtPassword.Text; }
        }

        internal string[] Roots
        {
            get
            {
                string[] values = txtRoots.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < values.Length; i++)
                    values[i] = values[i].Trim();
                return values;
            }
        }

        internal void SetStatus(string status)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { SetStatus(status); });
                return;
            }
            lblStatus.Text = status;
        }

        internal string StatusText
        {
            get { return lblStatus.Text; }
        }

        private void ckShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !ckShowPassword.Checked;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtHost.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (closingAfterSuccess || DialogResult != DialogResult.OK)
                return;

            e.Cancel = true;
            if (worker.IsBusy)
                return;

            if (Host.Length == 0)
            {
                SetStatus("Enter a host or IP address.");
                return;
            }

            if (ConnectHandler == null)
            {
                SetStatus("No FTP handler was configured.");
                return;
            }

            ToggleBusy(true);
            SetStatus("Trying to connect...");
            worker.RunWorkerAsync();
        }

        private void ToggleBusy(bool busy)
        {
            txtHost.Enabled = !busy;
            numPort.Enabled = !busy;
            txtUser.Enabled = !busy;
            txtPassword.Enabled = !busy;
            txtRoots.Enabled = !busy;
            ckShowPassword.Enabled = !busy;
            cmdOk.Enabled = !busy;
            cmdCancel.Enabled = !busy;
            progress.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = ConnectHandler == null ? "No FTP handler was configured." : ConnectHandler(this);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleBusy(false);

            if (e.Error != null)
            {
                SetStatus("Connection failed: " + e.Error.Message);
                return;
            }

            string error = e.Result as string;
            if (!string.IsNullOrEmpty(error))
            {
                SetStatus(error);
                return;
            }

            SetStatus("Connected successfully.");
            closingAfterSuccess = true;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
