using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using Horizon.Functions;

namespace Horizon.Forms
{
    internal sealed class FtpConnectDialog : Office2007Form
    {
        private readonly TextBoxX txtHost = new TextBoxX();
        private readonly NumericUpDown numPort = new NumericUpDown();
        private readonly TextBoxX txtUser = new TextBoxX();
        private readonly TextBoxX txtPassword = new TextBoxX();
        private readonly TextBoxX txtRoots = new TextBoxX();
        private readonly ComboBoxEx cboTransferMode = new ComboBoxEx();
        private readonly CheckBoxX ckShowPassword = new CheckBoxX();
        private readonly LabelX lblStatus = new LabelX();
        private readonly ProgressBar progress = new ProgressBar();
        private readonly ButtonX cmdOk = new ButtonX();
        private readonly ButtonX cmdCancel = new ButtonX();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private bool closingAfterSuccess;
        private string statusText = "Ready to connect.";

        internal FtpConnectDialog()
        {
            Text = "Connect FTP";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 392);
            Font = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Regular, GraphicsUnit.Point, 0);

            PanelEx root = new PanelEx();
            root.Dock = DockStyle.Fill;
            root.ColorSchemeStyle = eDotNetBarStyle.StyleManagerControlled;
            root.Style.BackColor1.ColorSchemePart = eColorSchemePart.PanelBackground;
            root.Style.BackColor2.ColorSchemePart = eColorSchemePart.PanelBackground2;
            root.Style.Border = eBorderType.SingleLine;
            root.Style.BorderColor.ColorSchemePart = eColorSchemePart.PanelBorder;
            root.Style.ForeColor.ColorSchemePart = eColorSchemePart.PanelText;
            root.Style.GradientAngle = 90;
            Controls.Add(root);

            LabelX lblTitle = new LabelX();
            lblTitle.Text = "FTP Console Connection";
            lblTitle.Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);
            lblTitle.Location = new Point(16, 10);
            lblTitle.Size = new Size(484, 26);
            root.Controls.Add(lblTitle);

            LabelX lblSubtitle = new LabelX();
            lblSubtitle.Text = "Connect Horizon to your console and map Content from Hdd1, Usb0, Usb1, and OnBoardMU.";
            lblSubtitle.WordWrap = true;
            lblSubtitle.Location = new Point(16, 34);
            lblSubtitle.Size = new Size(484, 30);
            root.Controls.Add(lblSubtitle);

            LabelX lblHost = new LabelX();
            lblHost.Text = "Host / IP";
            lblHost.Location = new Point(16, 68);
            lblHost.Size = new Size(120, 18);
            root.Controls.Add(lblHost);

            txtHost.Location = new Point(16, 88);
            txtHost.Size = new Size(330, 22);
            txtHost.Border.Class = "TextBoxBorder";
            txtHost.Border.CornerType = eCornerType.Square;
            root.Controls.Add(txtHost);

            LabelX lblPort = new LabelX();
            lblPort.Text = "Port";
            lblPort.Location = new Point(358, 68);
            lblPort.Size = new Size(100, 18);
            root.Controls.Add(lblPort);

            numPort.Location = new Point(358, 88);
            numPort.Size = new Size(142, 22);
            numPort.Minimum = 1;
            numPort.Maximum = 65535;
            numPort.Value = 21;
            root.Controls.Add(numPort);

            LabelX lblUser = new LabelX();
            lblUser.Text = "Username";
            lblUser.Location = new Point(16, 122);
            lblUser.Size = new Size(120, 18);
            root.Controls.Add(lblUser);

            txtUser.Location = new Point(16, 142);
            txtUser.Size = new Size(236, 22);
            txtUser.Border.Class = "TextBoxBorder";
            txtUser.Border.CornerType = eCornerType.Square;
            txtUser.Text = "xboxftp";
            root.Controls.Add(txtUser);

            LabelX lblPassword = new LabelX();
            lblPassword.Text = "Password";
            lblPassword.Location = new Point(264, 122);
            lblPassword.Size = new Size(120, 18);
            root.Controls.Add(lblPassword);

            txtPassword.Location = new Point(264, 142);
            txtPassword.Size = new Size(236, 22);
            txtPassword.Border.Class = "TextBoxBorder";
            txtPassword.Border.CornerType = eCornerType.Square;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "xboxftp";
            root.Controls.Add(txtPassword);

            ckShowPassword.Text = "Show password";
            ckShowPassword.Location = new Point(264, 168);
            ckShowPassword.Size = new Size(140, 22);
            ckShowPassword.Style = eDotNetBarStyle.StyleManagerControlled;
            ckShowPassword.CheckedChanged += new EventHandler(ckShowPassword_CheckedChanged);
            root.Controls.Add(ckShowPassword);

            LabelX lblTransferMode = new LabelX();
            lblTransferMode.Text = "Transfer mode";
            lblTransferMode.Location = new Point(16, 176);
            lblTransferMode.Size = new Size(120, 18);
            root.Controls.Add(lblTransferMode);

            cboTransferMode.DrawMode = DrawMode.OwnerDrawFixed;
            cboTransferMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTransferMode.Style = eDotNetBarStyle.StyleManagerControlled;
            cboTransferMode.ItemHeight = 16;
            cboTransferMode.Location = new Point(16, 196);
            cboTransferMode.Size = new Size(236, 22);
            cboTransferMode.Items.AddRange(new object[]
            {
                "Auto (Passive -> Active)",
                "Passive only",
                "Active only"
            });
            cboTransferMode.SelectedIndex = 0;
            root.Controls.Add(cboTransferMode);

            LabelX lblRoots = new LabelX();
            lblRoots.Text = "Roots to scan (comma-separated)";
            lblRoots.Location = new Point(16, 226);
            lblRoots.Size = new Size(236, 18);
            root.Controls.Add(lblRoots);

            txtRoots.Location = new Point(16, 246);
            txtRoots.Size = new Size(484, 22);
            txtRoots.Border.Class = "TextBoxBorder";
            txtRoots.Border.CornerType = eCornerType.Square;
            txtRoots.Text = "Hdd1,Usb0,Usb1,OnBoardMU";
            root.Controls.Add(txtRoots);

            LabelX lblHint = new LabelX();
            lblHint.Text = "Tip: Host 192.168.0.x, port 21, user/pass xbox. Horizon auto-maps each root to its Content folder.";
            lblHint.WordWrap = true;
            lblHint.Location = new Point(16, 272);
            lblHint.Size = new Size(484, 32);
            root.Controls.Add(lblHint);

            lblStatus.Location = new Point(16, 306);
            lblStatus.Size = new Size(484, 18);
            lblStatus.Text = statusText;
            root.Controls.Add(lblStatus);

            progress.Location = new Point(16, 328);
            progress.Size = new Size(484, 12);
            progress.Style = ProgressBarStyle.Blocks;
            root.Controls.Add(progress);

            cmdOk.Text = "Connect";
            cmdOk.Size = new Size(98, 28);
            cmdOk.Location = new Point(298, 350);
            cmdOk.Style = eDotNetBarStyle.StyleManagerControlled;
            cmdOk.Click += new EventHandler(cmdOk_Click);
            root.Controls.Add(cmdOk);

            cmdCancel.Text = "Cancel";
            cmdCancel.Size = new Size(98, 28);
            cmdCancel.Location = new Point(402, 350);
            cmdCancel.Style = eDotNetBarStyle.StyleManagerControlled;
            cmdCancel.Click += new EventHandler(cmdCancel_Click);
            root.Controls.Add(cmdCancel);

            AcceptButton = cmdOk;
            CancelButton = cmdCancel;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        internal Func<FtpConnectDialog, string> ConnectHandler { get; set; }

        internal string Host
        {
            get { return ReadUiValue(delegate { return txtHost.Text.Trim(); }, string.Empty); }
        }

        internal int Port
        {
            get { return ReadUiValue(delegate { return (int)numPort.Value; }, 21); }
        }

        internal string Username
        {
            get { return ReadUiValue(delegate { return txtUser.Text.Trim(); }, string.Empty); }
        }

        internal string Password
        {
            get { return ReadUiValue(delegate { return txtPassword.Text; }, string.Empty); }
        }

        internal string[] Roots
        {
            get
            {
                string rawRoots = ReadUiValue(delegate { return txtRoots.Text; }, string.Empty);
                string[] values = rawRoots.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < values.Length; i++)
                    values[i] = values[i].Trim();
                return values;
            }
        }

        internal FtpClient.TransferMode SelectedTransferMode
        {
            get
            {
                int selectedIndex = ReadUiValue(delegate { return cboTransferMode.SelectedIndex; }, 0);
                if (selectedIndex == 1)
                    return FtpClient.TransferMode.Passive;
                if (selectedIndex == 2)
                    return FtpClient.TransferMode.Active;
                return FtpClient.TransferMode.Auto;
            }
        }

        private T ReadUiValue<T>(Func<T> getter, T fallback)
        {
            if (getter == null)
                return fallback;

            try
            {
                if (InvokeRequired)
                {
                    object value = Invoke(getter);
                    if (value is T)
                        return (T)value;
                    return fallback;
                }

                return getter();
            }
            catch (ObjectDisposedException)
            {
                return fallback;
            }
            catch (InvalidOperationException)
            {
                return fallback;
            }
        }

        internal void SetStatus(string status)
        {
            statusText = status ?? string.Empty;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { lblStatus.Text = statusText; });
                return;
            }

            lblStatus.Text = statusText;
        }

        internal string StatusText
        {
            get { return statusText; }
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
            if (!closingAfterSuccess && worker.IsBusy)
                e.Cancel = true;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
                return;

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
                return;

            if (Host.Length == 0)
            {
                SetStatus("Enter a host or IP address.");
                txtHost.Focus();
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
            cboTransferMode.Enabled = !busy;
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
