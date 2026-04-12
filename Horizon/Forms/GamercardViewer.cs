using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Horizon.Functions;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace Horizon.Forms
{
    public partial class GamercardViewer : DevComponents.DotNetBar.Office2007RibbonForm
    {
        public GamercardViewer()
        {
            InitializeComponent();
            MdiParent = Main.mainForm;

            // FORÇA O USO DO TLS 1.2 E IGNORA CERTIFICADOS INVÁLIDOS (Mesma correção do TitleIDFinder)
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private static string[,] goodNames =
        {
            { "cheater912", "Cheater912" },
            { "unknown v2", "Unknown" },
            { "modified", "N v The Master" },
            { "ttg sean", "TTG SEAN" },
            { "zrueda", "Zrueda" },
            { "nookie", "NookiesaMillion" },
            { "clk", "Squaill" }
        };

        // URL base antiga mantida apenas para referência interna
        internal static string baseUrl = "http://gamercard.xbox.com";
        private string currentGamertag;

        // Atualizado para a URL moderna de perfis do Xbox Live
        private string lastURL = "https://account.xbox.com/en-us/profile?gamertag=Cheater912";

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            string searchTag = txtGamertag.Text.Trim();
            if (searchTag.Length == 0)
            {
                UI.messageBox("Enter a gamertag!\n\nDigite uma gamertag!", "No Gamertag", MessageBoxIcon.Warning);
            }
            else
            {
                bool isGood = false;
                for (int x = 0; x < goodNames.Length / 2; x++)
                {
                    if (goodNames[x, 0] == searchTag.ToLower())
                    {
                        searchTag = goodNames[x, 1];
                        isGood = true;
                        break;
                    }
                }

                currentGamertag = searchTag;
                cmdGamertag.Text = currentGamertag;
                lastURL = "https://account.xbox.com/en-us/profile?gamertag=" + searchTag;

                // Novas URLs seguras para buscar os avatares (Mantido ativo pela Microsoft)
                string avatarBaseUrl = "https://avatar-ssl.xboxlive.com/avatar/" + searchTag;

                try
                {
                    // Carrega as imagens de forma assíncrona para não travar a UI
                    pbGamerpic.LoadAsync(avatarBaseUrl + "/avatarpic-l.png");
                    pbAvatarSmall.LoadAsync(avatarBaseUrl + "/avatarpic-s.png");
                    pbAvatar.LoadAsync(avatarBaseUrl + "/avatar-body.png");

                    // Como o gamercard.xbox.com está morto, renderizamos um "Gamercard" local em HTML dentro do WebBrowser antigo
                    string localHtmlGamercard = @"
                    <html>
                    <body style='margin:0;padding:0;background-color:#EBEBEB;font-family:""Segoe UI"",Tahoma,Arial;text-align:center;'>
                        <div style='padding-top:25px;'>
                            <h2 style='color:#333;margin:0;padding:0;font-size:18px;'>" + searchTag + @"</h2>
                            <br>
                            <span style='background-color:#769F28;color:white;padding:3px 8px;font-size:12px;font-weight:bold;border-radius:3px;'>Xbox Live</span>
                            <br><br>
                            <span style='font-size:10px;color:#999;'>Legacy Mode</span>
                        </div>
                    </body>
                    </html>";

                    wbGamercard.Dispose();
                    wbGamercard = new WebBrowser();
                    SuspendLayout();
                    wbGamercard.AllowNavigation = false;
                    wbGamercard.AllowWebBrowserDrop = false;
                    wbGamercard.IsWebBrowserContextMenuEnabled = false;
                    wbGamercard.Location = new Point(11, 90);
                    wbGamercard.MinimumSize = new Size(20, 20);
                    wbGamercard.ScriptErrorsSuppressed = true;
                    wbGamercard.ScrollBarsEnabled = false;
                    wbGamercard.Size = new Size(200, 135);
                    wbGamercard.WebBrowserShortcutsEnabled = false;

                    // Injeta o HTML diretamente, sem precisar navegar para uma URL externa
                    wbGamercard.DocumentText = localHtmlGamercard;

                    Controls.Add(this.wbGamercard);
                    ResumeLayout(false);
                    rbGamercardViewer.Refresh();

                    if (isGood)
                        Main.doFlashColors();
                }
                catch
                {
                    doesNotExist();
                }
            }
        }

        private void doesNotExist()
        {
            UI.messageBox("This gamertag doesn't exist or could not be loaded!\n\nEsta gamertag não existe ou não pôde ser carregada!", "Invalid Gamertag", MessageBoxIcon.Exclamation);
        }

        private static string splitHtml(string doc, string start, string end)
        {
            return doc.Split(new string[] { start }, StringSplitOptions.None)[1].Split(new string[] { end }, StringSplitOptions.None)[0];
        }

        private void txtGamertag_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Enter)
                cmdSearch_Click(sender, e);
        }

        private void cmdGamertag_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(lastURL);
            }
            catch
            {

            }
        }

        private void cmdExtractAllImages_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Proteção para evitar travamentos caso as imagens ainda estejam baixando ou falhem
                    if (pbGamerpic.Image != null)
                        pbGamerpic.Image.Save(Path.Combine(fbd.SelectedPath, currentGamertag + " - Gamerpic.png"));

                    if (pbAvatarSmall.Image != null)
                        pbAvatarSmall.Image.Save(Path.Combine(fbd.SelectedPath, currentGamertag + " - AvatarPic.png"));

                    if (pbAvatar.Image != null)
                        pbAvatar.Image.Save(Path.Combine(fbd.SelectedPath, currentGamertag + " - Avatar.png"));

                    UI.messageBox("Images saved successfully!\n\nImagens salvas com sucesso!", "Saved", MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    UI.messageBox("Error saving images:\n" + ex.Message, "Error", MessageBoxIcon.Error);
                }
            }
        }
    }
}