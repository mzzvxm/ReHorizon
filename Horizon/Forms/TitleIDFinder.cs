using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Horizon.Functions;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Horizon.Forms
{
    public partial class TitleIDFinder : DevComponents.DotNetBar.Office2007RibbonForm
    {
        public TitleIDFinder()
        {
            InitializeComponent();
            MdiParent = Main.mainForm;

            // Força TLS 1.2 e ignora certificados inválidos
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length == 0)
                UI.messageBox("Enter something to search for!", "No Text", MessageBoxIcon.Error);
            else
            {
                if (txtSearch.Text.ToLower() == "modded warfare")
                    Main.doFlash();
                listGames.Items.Clear();
                foreach (ListViewItem title in doSearchTitle(txtSearch.Text))
                    listGames.Items.Add(title);
            }
        }

        private static string fixTitleName(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Limpeza nativa sem depender de WebUtility, previne erros no build
            string fixedStr = input.Replace("â„¢", String.Empty)
                                   .Replace("Â", String.Empty)
                                   .Replace("&amp;", "&&")
                                   .Replace("&#34;", "\"")
                                   .Replace("&quot;", "\"")
                                   .Replace("&#39;", "'");

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fixedStr.ToLower());
        }

        internal static List<ListViewItem> doSearchTitle(string search)
        {
            List<ListViewItem> titleList = new List<ListViewItem>();
            Dictionary<string, ListViewItem> finalItems = new Dictionary<string, ListViewItem>();
            string searchLower = search.ToLower();

            // 1. BUSCA ONLINE (Usado APENAS para descobrir os Title IDs)
            List<string> onlineIds = new List<string>();
            Dictionary<string, string> onlineNamesFallback = new Dictionary<string, string>();
            Dictionary<string, string> onlineCoversFallback = new Dictionary<string, string>();

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    wc.Headers.Add("Accept", "*/*");
                    wc.Headers.Add("hx-request", "true");
                    wc.Headers.Add("hx-target", "searchResults");

                    string searchUrl = "https://xboxgamer.pics/hx-search?searchInput=" + Uri.EscapeDataString(search);
                    string html = wc.DownloadString(searchUrl);

                    MatchCollection matches = Regex.Matches(html, @"id=\""([A-Fa-f0-9]{8})\""[\s\S]*?<img[^>]*data-src=\""([^\""]+)\""[^>]*>[\s\S]*?<h3[^>]*>(.*?)<\/h3>", RegexOptions.IgnoreCase);

                    foreach (Match m in matches)
                    {
                        string titleId = m.Groups[1].Value.ToUpper();
                        string coverUrl = m.Groups[2].Value;
                        string gameName = m.Groups[3].Value.Trim();

                        if (!onlineIds.Contains(titleId))
                        {
                            onlineIds.Add(titleId);
                            onlineNamesFallback[titleId] = gameName;
                            onlineCoversFallback[titleId] = coverUrl;
                        }
                    }
                }
            }
            catch { }

            // 2. BUSCA NO TITLES.JSON E SUBSTITUIÇÃO (A Prioridade para os Metadados: Nome e Capa)
            try
            {
                string jsonPath = Path.Combine(Application.StartupPath, "titles.json");
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    MatchCollection matches = Regex.Matches(jsonContent, @"\""([^\""]+)\""\s*:\s*\{([^}]+)\}", RegexOptions.IgnoreCase);

                    foreach (Match m in matches)
                    {
                        string gameName = m.Groups[1].Value;
                        string innerData = m.Groups[2].Value;

                        Match idMatch = Regex.Match(innerData, @"\""title_id\""\s*:\s*\""([^\""]+)\""", RegexOptions.IgnoreCase);
                        string titleId = idMatch.Success ? idMatch.Groups[1].Value.ToUpper() : "";

                        if (string.IsNullOrEmpty(titleId)) continue;

                        bool matchesText = gameName.ToLower().Contains(searchLower);
                        bool matchesOnline = onlineIds.Contains(titleId);

                        if (matchesText || matchesOnline)
                        {
                            Match coverMatch = Regex.Match(innerData, @"\""cover\""\s*:\s*\""([^\""]+)\""", RegexOptions.IgnoreCase);
                            string coverUrl = coverMatch.Success ? coverMatch.Groups[1].Value : "";

                            ListViewItem game = new ListViewItem(fixTitleName(gameName));
                            game.SubItems.Add(titleId);
                            game.Tag = coverUrl;

                            if (!finalItems.ContainsKey(titleId))
                            {
                                finalItems.Add(titleId, game);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.messageBox("Error parsing JSON database:\n\n" + ex.Message, "Error", MessageBoxIcon.Error);
            }

            // 3. FALLBACK DE SEGURANÇA (Se o Title ID existe no site, mas NÃO está no seu JSON)
            foreach (string id in onlineIds)
            {
                if (!finalItems.ContainsKey(id))
                {
                    ListViewItem game = new ListViewItem(fixTitleName(onlineNamesFallback[id]));
                    game.SubItems.Add(id);
                    game.Tag = onlineCoversFallback[id];
                    finalItems.Add(id, game);
                }
            }

            titleList = finalItems.Values.ToList();

            if (titleList.Count == 0)
            {
                UI.messageBox("No titles found online or in the local database.\n\nNenhum jogo encontrado online ou na base de dados local.", "Not Found", MessageBoxIcon.Information);
            }

            return titleList;
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Char.IsLetter(e.KeyChar) && Char.IsDigit(e.KeyChar) && Char.IsWhiteSpace(e.KeyChar) && Char.IsControl(e.KeyChar);
            if ((Keys)e.KeyChar == Keys.Enter)
                cmdSearch_Click(sender, e);
        }

        // Funções protetoras contra crash de Área de Transferência (Clipboard)
        private void copyTextToClipboard(string text)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return;
                }
                catch { System.Threading.Thread.Sleep(50); } // Aguarda 50ms e tenta novamente
            }
        }

        private void copyImageToClipboard(Image img)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Clipboard.SetImage(img);
                    return;
                }
                catch { System.Threading.Thread.Sleep(50); } // Aguarda 50ms e tenta novamente
            }
        }

        private void pbGameImage_Click(object sender, EventArgs e)
        {
            if (pbGameImage.Image != null)
                copyImageToClipboard(pbGameImage.Image);
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            if (listGames.SelectedItems.Count == 1)
            {
                try { Clipboard.Clear(); } catch { } // Tenta limpar o clipboard com segurança

                // Usa a nova função segura em vez do Clipboard direto
                copyTextToClipboard(listGames.SelectedItems[0].SubItems[1].Text);

                string imageLocation = listGames.SelectedItems[0].Tag as string;

                if (!string.IsNullOrEmpty(imageLocation))
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(state => {
                        try
                        {
                            string finalUrl = imageLocation.Contains("download.xbox.com") ? imageLocation.Replace("https://", "http://") : imageLocation;

                            using (WebClient wc = new WebClient())
                            {
                                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                                byte[] imgBytes = wc.DownloadData(finalUrl);

                                using (MemoryStream ms = new MemoryStream(imgBytes))
                                {
                                    Image img = Image.FromStream(ms);
                                    this.Invoke((MethodInvoker)delegate {
                                        pbGameImage.ImageLocation = "";
                                        pbGameImage.Image = img;
                                    });
                                }
                            }
                        }
                        catch
                        {
                            this.Invoke((MethodInvoker)delegate {
                                pbGameImage.ImageLocation = "";
                                pbGameImage.Image = Properties.Resources.TitleIdFinder_Default;
                            });
                        }
                    });
                }
                else
                {
                    pbGameImage.ImageLocation = "";
                    pbGameImage.Image = Properties.Resources.TitleIdFinder_Default;
                }
            }
        }
    }
}