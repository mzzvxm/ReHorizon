/**
 ******************************************************************************
 * Velocity : Xbox 360 Modding Tool Project                               *
 ******************************************************************************
 * Original implementation by hetelek.                                        *
 * Adapted for ReHorizon by mzzvxm, 2026.                                     *
 ******************************************************************************
 *
 * @modified mzzvxm, 2026 - Ported and adapted for ReHorizon
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using Horizon.Functions;
using Horizon.Properties;
using XContent;
using XboxDataBaseFile;
using XProfile;

namespace Horizon.Forms
{
    internal sealed class ProfileCreatorWizard : Office2007Form
    {
        private const uint DashboardTitleId = 0xFFFE07D1;
        private const ulong AvatarImageRecordId = 0x8007;

        private readonly LabelX lblPageTitle;
        private readonly LabelX lblPageDescription;
        private readonly PictureBox picAvatar;
        private readonly Panel pageHost;
        private readonly ButtonX cmdBack;
        private readonly ButtonX cmdNext;
        private readonly ButtonX cmdCancel;

        private readonly Panel pageConsoleType;
        private readonly Panel pageGamertag;
        private readonly Panel pageGamerpic;
        private readonly Panel pageAvatar;
        private readonly Panel pageSavePath;

        private readonly RadioButton rdoRetail;
        private readonly RadioButton rdoDevkit;
        private readonly RadioButton rdoAvatarMale;
        private readonly RadioButton rdoAvatarFemale;
        private readonly TextBoxX txtGamertag;
        private readonly ListView lstGamerpics;
        private readonly ImageList gamerpicImages;
        private readonly ButtonX cmdImportImage;
        private readonly TextBoxX txtSavePath;
        private readonly ButtonX cmdBrowsePath;

        private int pageIndex;
        private ulong profileId;

        internal string CreatedProfilePath { get; private set; }

        internal ProfileCreatorWizard()
        {
            Text = "Avatar Creator";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            DoubleBuffered = true;
            ClientSize = new Size(760, 470);
            BackColor = Color.White;

            PanelEx root = new PanelEx();
            root.Dock = DockStyle.Fill;
            root.CanvasColor = Color.White;
            root.Style.BackColor1.Color = Color.White;
            root.Style.BackColor2.Color = Color.White;
            root.Style.GradientAngle = 0;
            Controls.Add(root);

            lblPageTitle = new LabelX();
            lblPageTitle.Location = new Point(24, 14);
            lblPageTitle.Size = new Size(710, 30);
            lblPageTitle.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Bold);
            lblPageTitle.ForeColor = Color.FromArgb(0, 66, 186);
            lblPageTitle.Text = "Avatar Creator";
            root.Controls.Add(lblPageTitle);

            lblPageDescription = new LabelX();
            lblPageDescription.Location = new Point(24, 48);
            lblPageDescription.Size = new Size(710, 36);
            lblPageDescription.WordWrap = true;
            root.Controls.Add(lblPageDescription);

            picAvatar = new PictureBox();
            picAvatar.Location = new Point(24, 92);
            picAvatar.Size = new Size(160, 280);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.Image = loadWizardAvatarImage();
            root.Controls.Add(picAvatar);

            pageHost = new Panel();
            pageHost.Location = new Point(196, 92);
            pageHost.Size = new Size(538, 280);
            pageHost.BackColor = Color.White;
            root.Controls.Add(pageHost);

            cmdBack = createActionButton("Back");
            cmdBack.Location = new Point(422, 420);
            cmdBack.Click += cmdBack_Click;
            root.Controls.Add(cmdBack);

            cmdNext = createActionButton("Next");
            cmdNext.Location = new Point(534, 420);
            cmdNext.Click += cmdNext_Click;
            root.Controls.Add(cmdNext);

            cmdCancel = createActionButton("Cancel");
            cmdCancel.Location = new Point(646, 420);
            cmdCancel.Click += cmdCancel_Click;
            root.Controls.Add(cmdCancel);

            pageConsoleType = new Panel();
            pageConsoleType.Dock = DockStyle.Fill;
            pageConsoleType.BackColor = Color.White;
            rdoRetail = new RadioButton();
            rdoRetail.Location = new Point(4, 8);
            rdoRetail.Size = new Size(500, 44);
            rdoRetail.Checked = true;
            rdoRetail.Text = "Retail - Any Xbox 360 profile that can or could connect to LIVE.";
            pageConsoleType.Controls.Add(rdoRetail);

            rdoDevkit = new RadioButton();
            rdoDevkit.Location = new Point(4, 62);
            rdoDevkit.Size = new Size(500, 44);
            rdoDevkit.Text = "Development Kit - Dev account format for XDK/Dev environments.";
            pageConsoleType.Controls.Add(rdoDevkit);

            pageGamertag = new Panel();
            pageGamertag.Dock = DockStyle.Fill;
            pageGamertag.BackColor = Color.White;
            txtGamertag = new TextBoxX();
            txtGamertag.Location = new Point(4, 8);
            txtGamertag.Size = new Size(520, 22);
            txtGamertag.MaxLength = 15;
            txtGamertag.WatermarkText = "Gamertag";
            txtGamertag.TextChanged += txtGamertag_TextChanged;
            pageGamertag.Controls.Add(txtGamertag);

            LabelX lblGamertagHint = new LabelX();
            lblGamertagHint.Location = new Point(4, 40);
            lblGamertagHint.Size = new Size(520, 80);
            lblGamertagHint.WordWrap = true;
            lblGamertagHint.Text = "A gamertag must be 1-15 chars, start with a letter, and contain only letters, numbers, and single spaces.";
            pageGamertag.Controls.Add(lblGamertagHint);

            pageGamerpic = new Panel();
            pageGamerpic.Dock = DockStyle.Fill;
            pageGamerpic.BackColor = Color.White;
            gamerpicImages = new ImageList();
            gamerpicImages.ColorDepth = ColorDepth.Depth32Bit;
            gamerpicImages.ImageSize = new Size(64, 64);

            lstGamerpics = new ListView();
            lstGamerpics.Location = new Point(4, 8);
            lstGamerpics.Size = new Size(520, 228);
            lstGamerpics.View = View.LargeIcon;
            lstGamerpics.MultiSelect = false;
            lstGamerpics.HideSelection = false;
            lstGamerpics.LargeImageList = gamerpicImages;
            lstGamerpics.SelectedIndexChanged += lstGamerpics_SelectedIndexChanged;
            pageGamerpic.Controls.Add(lstGamerpics);

            cmdImportImage = createActionButton("Import Image...");
            cmdImportImage.Location = new Point(398, 244);
            cmdImportImage.Size = new Size(126, 30);
            cmdImportImage.Click += cmdImportImage_Click;
            pageGamerpic.Controls.Add(cmdImportImage);

            pageAvatar = new Panel();
            pageAvatar.Dock = DockStyle.Fill;
            pageAvatar.BackColor = Color.White;

            LabelX lblAvatarHint = new LabelX();
            lblAvatarHint.Location = new Point(4, 8);
            lblAvatarHint.Size = new Size(520, 24);
            lblAvatarHint.Text = "Choose the starting avatar for this profile.";
            pageAvatar.Controls.Add(lblAvatarHint);

            PictureBox picMale = new PictureBox();
            picMale.Location = new Point(78, 40);
            picMale.Size = new Size(125, 190);
            picMale.SizeMode = PictureBoxSizeMode.Zoom;
            picMale.Image = loadAvatarGenderPreview(false);
            pageAvatar.Controls.Add(picMale);

            PictureBox picFemale = new PictureBox();
            picFemale.Location = new Point(330, 40);
            picFemale.Size = new Size(125, 190);
            picFemale.SizeMode = PictureBoxSizeMode.Zoom;
            picFemale.Image = loadAvatarGenderPreview(true);
            pageAvatar.Controls.Add(picFemale);

            rdoAvatarMale = new RadioButton();
            rdoAvatarMale.Location = new Point(108, 242);
            rdoAvatarMale.Size = new Size(70, 24);
            rdoAvatarMale.Checked = true;
            rdoAvatarMale.Text = "Male";
            pageAvatar.Controls.Add(rdoAvatarMale);

            rdoAvatarFemale = new RadioButton();
            rdoAvatarFemale.Location = new Point(358, 242);
            rdoAvatarFemale.Size = new Size(78, 24);
            rdoAvatarFemale.Text = "Female";
            pageAvatar.Controls.Add(rdoAvatarFemale);

            pageSavePath = new Panel();
            pageSavePath.Dock = DockStyle.Fill;
            pageSavePath.BackColor = Color.White;
            txtSavePath = new TextBoxX();
            txtSavePath.Location = new Point(4, 8);
            txtSavePath.Size = new Size(418, 22);
            txtSavePath.TextChanged += txtSavePath_TextChanged;
            pageSavePath.Controls.Add(txtSavePath);

            cmdBrowsePath = createActionButton("Browse...");
            cmdBrowsePath.Location = new Point(428, 6);
            cmdBrowsePath.Size = new Size(96, 26);
            cmdBrowsePath.Click += cmdBrowsePath_Click;
            pageSavePath.Controls.Add(cmdBrowsePath);

            LabelX lblSaveHint = new LabelX();
            lblSaveHint.Location = new Point(4, 40);
            lblSaveHint.Size = new Size(520, 120);
            lblSaveHint.WordWrap = true;
            lblSaveHint.Text = "Horizon will create a new profile package from scratch.\n\nIf \"FFFE07D1.gpd\" is not found in Horizon's executable folder, you will be prompted to select a dashboard GPD template.";
            pageSavePath.Controls.Add(lblSaveHint);

            loadBuiltInGamerpics();
            generateNewProfileId();
            showPage(0);
        }

        private static ButtonX createActionButton(string text)
        {
            ButtonX button = new ButtonX();
            button.AccessibleRole = AccessibleRole.PushButton;
            button.ColorTable = eButtonColor.OrangeWithBackground;
            button.FocusCuesEnabled = false;
            button.Shape = new RoundRectangleShapeDescriptor();
            button.Size = new Size(104, 30);
            button.Style = eDotNetBarStyle.StyleManagerControlled;
            button.Text = text;
            return button;
        }

        private static Image loadWizardAvatarImage()
        {
            string assetsDirectory = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileManager"),
                "Assets");
            string avatarPath = Path.Combine(assetsDirectory, "avatarJumping.png");
            if (File.Exists(avatarPath))
            {
                try
                {
                    using (Image img = Image.FromFile(avatarPath))
                        return new Bitmap(img);
                }
                catch
                {
                }
            }
            return Resources.User;
        }

        private static Image loadAvatarGenderPreview(bool female)
        {
            string assetsDirectory = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileManager"),
                "Assets");
            string fileName = female ? "avatarFemale.png" : "avatarMale.png";
            string previewPath = Path.Combine(assetsDirectory, fileName);

            if (File.Exists(previewPath))
            {
                try
                {
                    using (Image img = Image.FromFile(previewPath))
                        return new Bitmap(img);
                }
                catch
                {
                }
            }

            return female ? Resources.UserSingle : Resources.User;
        }

        private void loadBuiltInGamerpics()
        {
            gamerpicImages.Images.Clear();
            lstGamerpics.Items.Clear();

            if (loadVelocityGamerpics())
                return;

            Image[] defaultImages = new Image[]
            {
                Resources.Logo64,
                Resources.User,
                Resources.UserSingle,
                Resources.Smile,
                Resources.Star,
                Resources.Heart,
                Resources.Gamerscore,
                Resources.Trophy,
                Resources.Console,
                Resources.Lock,
                Resources.Gear,
                Resources.GreenDot,
                Resources.RedDot,
                Resources.GrayDot
            };

            for (int i = 0; i < defaultImages.Length; i++)
                addGamerpic(defaultImages[i], "Pic " + (i + 1).ToString(CultureInfo.InvariantCulture));

            if (lstGamerpics.Items.Count > 0)
                lstGamerpics.Items[0].Selected = true;
        }

        private bool loadVelocityGamerpics()
        {
            // Built-in avatar creator gamerpic assets are sourced from Velocity (hetelek)
            // and adapted for ReHorizon.
            string assetsDirectory = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileManager"),
                "Assets");
            string[] velocityNames = new string[]
            {
                "20000.png", "20001.png", "20002.png", "20003.png", "20004.png", "20005.png", "20006.png",
                "20007.png", "20008.png", "20009.png", "2000A.png", "2000B.png", "2000C.png"
            };

            bool loadedAny = false;
            for (int i = 0; i < velocityNames.Length; i++)
            {
                string filePath = Path.Combine(assetsDirectory, velocityNames[i]);
                if (!File.Exists(filePath))
                    continue;

                try
                {
                    using (Image source = Image.FromFile(filePath))
                        addGamerpic(new Bitmap(source), Path.GetFileNameWithoutExtension(velocityNames[i]).ToUpperInvariant());
                    loadedAny = true;
                }
                catch
                {
                }
            }

            if (loadedAny && lstGamerpics.Items.Count > 0)
                lstGamerpics.Items[0].Selected = true;

            return loadedAny;
        }

        private void addGamerpic(Image image, string key)
        {
            if (image == null)
                return;

            Bitmap normalized = new Bitmap(64, 64);
            Graphics g = Graphics.FromImage(normalized);
            g.Clear(Color.Transparent);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, 64, 64);
            g.Dispose();

            gamerpicImages.Images.Add(key, normalized);
            ListViewItem item = new ListViewItem(String.Empty, gamerpicImages.Images.Count - 1);
            item.Tag = key;
            lstGamerpics.Items.Add(item);
        }

        private void generateNewProfileId()
        {
            byte[] rand = new byte[6];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(rand);

            ulong part0 = (ulong)(ushort)((rand[0] << 8) | rand[1]);
            ulong part1 = ((ulong)(ushort)((rand[2] << 8) | rand[3])) << 16;
            ulong part2 = ((ulong)(ushort)((rand[4] << 8) | rand[5])) << 32;

            profileId = 0xE000000000000000UL | part2 | part1 | part0;

            string defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            txtSavePath.Text = Path.Combine(defaultDirectory, profileId.ToString("X16"));
        }

        private void showPage(int index)
        {
            pageIndex = index;
            pageHost.Controls.Clear();

            switch (pageIndex)
            {
                case 0:
                    lblPageTitle.Text = "Console Type";
                    lblPageDescription.Text = "Choose which console type this profile will be for.";
                    pageHost.Controls.Add(pageConsoleType);
                    break;

                case 1:
                    lblPageTitle.Text = "Gamertag";
                    lblPageDescription.Text = "Choose the gamertag for the new profile.";
                    pageHost.Controls.Add(pageGamertag);
                    break;

                case 2:
                    lblPageTitle.Text = "Gamerpicture";
                    lblPageDescription.Text = "Choose the gamerpicture for the new profile.";
                    pageHost.Controls.Add(pageGamerpic);
                    break;

                case 3:
                    lblPageTitle.Text = "Avatar";
                    lblPageDescription.Text = "Choose the starting avatar for the new profile.";
                    pageHost.Controls.Add(pageAvatar);
                    break;

                default:
                    lblPageTitle.Text = "Save Path";
                    lblPageDescription.Text = "Choose where Horizon should save the created profile.";
                    pageHost.Controls.Add(pageSavePath);
                    break;
            }

            cmdBack.Enabled = pageIndex > 0;
            cmdNext.Text = pageIndex == 4 ? "Create" : "Next";
            updateNextButtonEnabled();
        }

        private void updateNextButtonEnabled()
        {
            string validationError;
            cmdNext.Enabled = validateCurrentPage(out validationError, false);
        }

        private void cmdBack_Click(object sender, EventArgs e)
        {
            if (pageIndex > 0)
                showPage(pageIndex - 1);
        }

        private void cmdNext_Click(object sender, EventArgs e)
        {
            string validationError;
            if (!validateCurrentPage(out validationError, true))
            {
                if (validationError.Length != 0)
                    UI.errorBox(validationError);
                return;
            }

            if (pageIndex < 4)
            {
                showPage(pageIndex + 1);
                return;
            }

            string createError;
            if (!createProfile(out createError))
            {
                UI.errorBox(createError);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtGamertag_TextChanged(object sender, EventArgs e)
        {
            bool valid = isValidGamertag(txtGamertag.Text);
            txtGamertag.ForeColor = valid || txtGamertag.TextLength == 0
                ? SystemColors.WindowText
                : Color.FromArgb(255, 1, 1);
            updateNextButtonEnabled();
        }

        private void cmdImportImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Gamerpicture";
            ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Image imported = Image.FromFile(ofd.FileName))
                        addGamerpic(imported, "Custom" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                    lstGamerpics.Items[lstGamerpics.Items.Count - 1].Selected = true;
                    updateNextButtonEnabled();
                }
                catch (Exception ex)
                {
                    UI.errorBox("Could not import image.\n\n" + ex.Message);
                }
            }
        }

        private void lstGamerpics_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateNextButtonEnabled();
        }

        private void txtSavePath_TextChanged(object sender, EventArgs e)
        {
            updateNextButtonEnabled();
        }

        private void cmdBrowsePath_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Avatar Creator";
            sfd.FileName = profileId.ToString("X16");
            sfd.Filter = "All Files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                txtSavePath.Text = sfd.FileName;
                updateNextButtonEnabled();
            }
        }

        private bool validateCurrentPage(out string error, bool strict)
        {
            error = String.Empty;

            switch (pageIndex)
            {
                case 1:
                    if (!isValidGamertag(txtGamertag.Text))
                    {
                        error = "Enter a valid gamertag (1-15 chars, first char must be a letter, no double spaces, only letters/numbers/spaces).";
                        return false;
                    }
                    return true;

                case 2:
                    if (lstGamerpics.SelectedIndices.Count != 1)
                    {
                        if (strict)
                            error = "Select a gamerpicture.";
                        return false;
                    }
                    return true;

                case 4:
                    if (txtSavePath.Text.Trim().Length == 0)
                    {
                        error = "Choose a save path for the profile.";
                        return false;
                    }
                    return true;
            }

            return true;
        }

        private static bool isValidGamertag(string gamertag)
        {
            if (gamertag == null)
                return false;

            gamertag = gamertag.Trim();
            if (gamertag.Length == 0 || gamertag.Length > 15)
                return false;

            if (!Char.IsLetter(gamertag[0]))
                return false;

            char previous = '\0';
            for (int i = 0; i < gamertag.Length; i++)
            {
                char c = gamertag[i];
                if (c == ' ' && previous == ' ')
                    return false;
                if (!Char.IsLetterOrDigit(c) && c != ' ')
                    return false;
                previous = c;
            }

            return true;
        }

        private bool createProfile(out string error)
        {
            error = String.Empty;
            string outputPath = txtSavePath.Text.Trim();
            string gamertag = txtGamertag.Text.Trim();
            bool devkit = rdoDevkit.Checked;

            string dashboardGpdPath = resolveDashboardGpdPath();
            if (dashboardGpdPath == null)
            {
                error = "A dashboard GPD (FFFE07D1.gpd) is required to create a complete profile.";
                return false;
            }

            try
            {
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!String.IsNullOrEmpty(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                if (File.Exists(outputPath))
                {
                    if (UI.messageBox("A file already exists at this path.\n\nDo you want to overwrite it?",
                        "Overwrite File?", MessageBoxIcon.Question, MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button3)
                        != DialogResult.Yes)
                    {
                        error = "Profile creation was cancelled.";
                        return false;
                    }

                    File.Delete(outputPath);
                }

                byte[] gamerpic64 = selectedGamerpicAsPng(64);
                byte[] gamerpic32 = selectedGamerpicAsPng(32);
                string pictureKey = buildPictureKey();
                bool femaleAvatar = rdoAvatarFemale.Checked;

                XContentPackage package = new XContentPackage();
                package.CreatePackage(outputPath, createProfileMetadata(gamerpic64, gamertag));

                byte[] accountData = buildAccountData(gamertag, devkit);
                package.StfsContentPackage.CreateFileFromArray("Account", accountData);

                byte[] dashboardGpd = patchDashboardGpd(dashboardGpdPath, pictureKey, femaleAvatar);
                package.StfsContentPackage.CreateFileFromArray("FFFE07D1.gpd", dashboardGpd);

                package.StfsContentPackage.CreateFileFromArray("tile_64.png", gamerpic64);
                package.StfsContentPackage.CreateFileFromArray("tile_32.png", gamerpic32);
                package.StfsContentPackage.CreateFileFromArray("pp_64.png", gamerpic64);
                package.StfsContentPackage.CreateFileFromArray("pp_32.png", gamerpic32);

                package.Flush();
                package.Header.Metadata.ContentSize = (ulong)(package.IO.Stream.Length - package.StfsContentPackage.VolumeExtension.BackingFileOffset);
                package.Save(true);
                package.CloseIO(true);

                string validationError;
                if (!validateCreatedProfile(outputPath, out validationError))
                    throw new InvalidOperationException(validationError);

                cacheCreatedProfile(outputPath);
                CreatedProfilePath = outputPath;
                return true;
            }
            catch (Exception ex)
            {
                error = "An error occurred while creating the profile.\n\n" + ex.ToString();
                return false;
            }
        }

        private static string resolveDashboardGpdPath()
        {
            string executablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string localTemplate = Path.Combine(executablePath, "FFFE07D1.gpd");
            if (File.Exists(localTemplate))
                return localTemplate;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Dashboard GPD (FFFE07D1.gpd)";
            ofd.Filter = "GPD Files (*.gpd)|*.gpd|All Files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
                return ofd.FileName;
            return null;
        }

        private static bool validateCreatedProfile(string profilePath, out string error)
        {
            error = String.Empty;
            XContentPackage package = new XContentPackage();
            try
            {
                if (!package.LoadPackage(profilePath, false))
                {
                    error = "The created file could not be opened as an Xbox package.";
                    return false;
                }

                if (package.Header.Metadata.ContentType != ContentTypes.Profile)
                {
                    error = "The created package is not marked as a profile.";
                    return false;
                }

                if (package.StfsContentPackage == null
                    || package.StfsContentPackage.GetDirectoryEntryIndex("Account") == -1
                    || package.StfsContentPackage.GetDirectoryEntryIndex("FFFE07D1.gpd") == -1)
                {
                    error = "The created profile is missing required files (Account / FFFE07D1.gpd).";
                    return false;
                }

                try
                {
                    new XProfileAccount(package.StfsContentPackage.ExtractFileToArray("Account"));
                }
                catch (Exception ex)
                {
                    error = "The created Account file is invalid.\n\n" + ex.Message;
                    return false;
                }

                return true;
            }
            finally
            {
                package.CloseIO(true);
            }
        }

        private XContentMetadata createProfileMetadata(byte[] gamerpic, string gamertag)
        {
            XContentMetadata meta = new XContentMetadata();
            meta.ContentType = ContentTypes.Profile;
            meta.ContentMetadataVersion = 2;
            meta.ExecutionId.TitleId = DashboardTitleId;
            meta.Creator = profileId;
            meta.TitleName = "Xbox 360 Dashboard";
            meta.Publisher = "Microsoft";
            string displayName = String.IsNullOrEmpty(gamertag) ? profileId.ToString("X16") : gamertag.Trim();
            meta.SetAllDisplayNames(displayName);
            meta.SetAllDescriptions("Xbox 360 Profile");
            meta.DescriptionEx = String.Empty;
            meta.DisplayNameEx = String.Empty;
            meta.SetThumbnail(gamerpic);
            meta.SetTitleThumbnail(Resources.Console.ToByteArray());
            meta.DeviceId = new byte[20];
            meta.ConsoleId = new byte[5];
            meta.Reserved2 = new byte[44];
            meta.TransferFlags.bTransferFlags = 0x40;
            meta.AvatarAssetData.AssetId = new byte[16];
            meta.AvatarAssetData.Reserved = new byte[11];
            meta.MediaData.SeriesId = new byte[16];
            meta.MediaData.SeasonId = new byte[16];
            return meta;
        }

        private byte[] selectedGamerpicAsPng(int size)
        {
            int selectedIndex = lstGamerpics.SelectedIndices.Count == 0 ? 0 : lstGamerpics.SelectedIndices[0];
            Image source = gamerpicImages.Images[selectedIndex];
            Bitmap scaled = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(scaled);
            g.Clear(Color.Transparent);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, size, size);
            g.Dispose();
            return scaled.ToByteArray();
        }

        private string buildPictureKey()
        {
            int selected = lstGamerpics.SelectedIndices.Count == 0 ? 0 : lstGamerpics.SelectedIndices[0];
            string id = Math.Max(0, Math.Min(15, selected)).ToString("X");

            ListViewItem selectedItem = lstGamerpics.SelectedItems.Count == 0 ? null : lstGamerpics.SelectedItems[0];
            if (selectedItem != null && selectedItem.Tag != null)
            {
                string tag = selectedItem.Tag.ToString();
                if (!String.IsNullOrEmpty(tag) && tag.Length == 5
                    && tag.StartsWith("2000", StringComparison.OrdinalIgnoreCase))
                    id = tag.Substring(4, 1).ToUpperInvariant();
            }

            return String.Format("FFFE07D10002000{0}0001000{0}", id);
        }

        private byte[] buildAccountData(string gamertag, bool devkit)
        {
            byte[] zeroedAccount = new byte[0x17C];
            EndianReader reader = new EndianReader(new MemoryStream(zeroedAccount), EndianType.BigEndian);
            XProfileAccount.XamAccountInfo info = new XProfileAccount.XamAccountInfo(reader);
            reader.Close();

            info.GamerTag = gamertag;
            info.XuidOnline = profileId;
            info.CachedUserFlags = 0;
            info.Passcode = new byte[4];
            info.OnlineKey = new byte[16];
            info.OnlineDomain = String.Empty;
            info.OnlineKerberosRealm = String.Empty;
            info.UserPassportMembername = String.Empty;
            info.UserPassportPassword = String.Empty;
            info.OwnerPassportMembername = String.Empty;
            info.XboxLiveEnabled = !devkit;
            info.Recovering = false;
            info.PasswordProtected = false;

            byte[] encrypted = HorizonCrypt.XeKeysObfuscate(1, info.ToArray(), devkit);
            XProfileAccount account = new XProfileAccount(encrypted);
            account.Info.GamerTag = gamertag;
            account.Info.XuidOnline = profileId;
            account.Info.OnlineDomain = String.Empty;
            account.Info.OnlineKerberosRealm = String.Empty;
            account.Info.UserPassportMembername = String.Empty;
            account.Info.UserPassportPassword = String.Empty;
            account.Info.OwnerPassportMembername = String.Empty;
            account.Info.XboxLiveEnabled = !devkit;
            account.Info.Recovering = false;
            account.Info.PasswordProtected = false;
            account.DeveloperAccount = devkit;
            return account.Save();
        }

        private static byte[] patchDashboardGpd(string templatePath, string pictureKey, bool femaleAvatar)
        {
            byte[] originalData = File.ReadAllBytes(templatePath);
            EndianIO io = null;
            DataFile dataFile = null;

            try
            {
                io = new EndianIO(new MemoryStream(originalData), EndianType.BigEndian, true);
                io.Open();

                dataFile = new DataFile(io);
                dataFile.Read();

                // Keep the same baseline behavior from Velocity creator:
                // patch gamer picture key fields, and avatar payload based on gender choice.
                string normalizedKey = (pictureKey ?? String.Empty).ToLowerInvariant();
                trySetUnicodeSetting(dataFile, XProfileIds.XPROFILE_GAMERCARD_PICTURE_KEY, normalizedKey + "\0");
                trySetUnicodeSetting(dataFile, XProfileIds.XPROFILE_GAMERCARD_PERSONAL_PICTURE, normalizedKey + "\0");

                byte[] avatarImage = loadAvatarGenderImageBytes(femaleAvatar);
                if (avatarImage != null && avatarImage.Length != 0)
                    upsertImageEntry(dataFile, AvatarImageRecordId, avatarImage);

                if (femaleAvatar)
                {
                    byte[] femaleAvatarInfo = loadFemaleAvatarInfoBytes();
                    if (femaleAvatarInfo != null && femaleAvatarInfo.Length != 0)
                        upsertBinarySetting(dataFile, XProfileIds.XPROFILE_GAMERCARD_AVATAR_INFO_1, femaleAvatarInfo);
                }

                return dataFile.ToArray();
            }
            catch
            {
                // Some dashboard GPD variants parse differently in Horizon.
                // If patching fails, keep the original GPD so profile creation still works.
                return originalData;
            }
            finally
            {
                if (dataFile != null)
                {
                    try
                    {
                        dataFile.Dispose();
                    }
                    catch
                    {
                    }
                }
                else if (io != null)
                {
                    try
                    {
                        io.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static byte[] loadAvatarGenderImageBytes(bool femaleAvatar)
        {
            string assetsDirectory = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileManager"),
                "Assets");
            string fileName = femaleAvatar ? "avatarFemale.png" : "avatarMale.png";
            string imagePath = Path.Combine(assetsDirectory, fileName);

            if (File.Exists(imagePath))
            {
                try
                {
                    using (Image img = Image.FromFile(imagePath))
                        return new Bitmap(img).ToByteArray();
                }
                catch
                {
                }
            }

            return (femaleAvatar ? (Image)Resources.UserSingle : Resources.User).ToByteArray();
        }

        private static byte[] loadFemaleAvatarInfoBytes()
        {
            string assetsDirectory = Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileManager"),
                "Assets");
            string binaryPath = Path.Combine(assetsDirectory, "femaleAvatar.bin");
            if (!File.Exists(binaryPath))
                return null;

            try
            {
                return File.ReadAllBytes(binaryPath);
            }
            catch
            {
                return null;
            }
        }

        private static bool trySetUnicodeSetting(DataFile dataFile, XProfileIds settingId, string value)
        {
            if (dataFile == null)
                return false;

            byte[] settingValue = UnicodeEncoding.BigEndianUnicode.GetBytes(value ?? String.Empty);
            DataFileId id = new DataFileId()
            {
                Namespace = XboxDataBaseFile.Namespace.SETTINGS,
                Id = (ulong)settingId
            };

            byte[] current = null;
            try
            {
                current = dataFile.ReadRecord(id);
            }
            catch
            {
                return false;
            }

            // Match Velocity behavior: do not create missing settings while creating profile.
            if (current == null || current.Length == 0)
                return false;

            SettingRecord record = new SettingRecord();
            if (!record.Read(current))
                return false;

            record.settingId = (uint)settingId;
            record.settingType = 0x04;
            if (record.Unk2 == null || record.Unk2.Length != 7)
                record.Unk2 = new byte[7];
            if (record.Reserved == null || record.Reserved.Length != 4)
                record.Reserved = new byte[4];
            record.pbData = 0;
            record.cbData = (uint)settingValue.Length;
            record.varData = settingValue;

            try
            {
                dataFile.Upsert(id, record.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool upsertBinarySetting(DataFile dataFile, XProfileIds settingId, byte[] value)
        {
            if (dataFile == null || value == null)
                return false;

            DataFileId id = new DataFileId()
            {
                Namespace = XboxDataBaseFile.Namespace.SETTINGS,
                Id = (ulong)settingId
            };

            SettingRecord record = new SettingRecord();
            byte[] current = null;
            try
            {
                current = dataFile.ReadRecord(id);
            }
            catch
            {
            }

            if (current == null || !record.Read(current))
            {
                record = new SettingRecord((uint)settingId, 0x06);
                record.Unk1 = 0;
                record.Unk2 = new byte[7];
            }

            record.settingId = (uint)settingId;
            record.settingType = 0x06;
            if (record.Unk2 == null || record.Unk2.Length != 7)
                record.Unk2 = new byte[7];
            record.cbData = (uint)value.Length;
            record.pbData = 0;
            record.varData = value;

            try
            {
                dataFile.Upsert(id, record.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool upsertImageEntry(DataFile dataFile, ulong imageId, byte[] imageData)
        {
            if (dataFile == null || imageData == null || imageData.Length == 0)
                return false;

            try
            {
                dataFile.Upsert(new DataFileId()
                {
                    Namespace = XboxDataBaseFile.Namespace.IMAGES,
                    Id = imageId
                }, imageData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void cacheCreatedProfile(string profilePath)
        {
            XContentPackage package = new XContentPackage();
            try
            {
                if (!package.LoadPackage(profilePath, false))
                    return;

                if (package.StfsContentPackage == null
                    || package.StfsContentPackage.GetDirectoryEntryIndex("Account") == -1)
                    return;

                XProfileAccount account = new XProfileAccount(package.StfsContentPackage.ExtractFileToArray("Account"));
                Horizon.ProfileManager.addProfileToCache(package, account);
            }
            catch { }
            finally
            {
                package.CloseIO(true);
            }
        }
    }
}
