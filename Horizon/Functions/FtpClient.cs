using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Horizon.Library.Systems.FATX;

namespace Horizon.Functions
{
    internal sealed class FtpClient
    {
        internal sealed class Entry
        {
            internal string Name;
            internal string FullPath;
            internal bool IsDirectory;
            internal long Size;
            internal DateTime Modified = DateTime.MinValue;
        }

        private readonly string host;
        private readonly int port;
        private readonly NetworkCredential credentials;

        internal FtpClient(string host, int port, string username, string password)
        {
            this.host = (host ?? string.Empty).Trim();
            if (this.host.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                this.host = this.host.Substring(6);
            this.host = this.host.Trim('/').Trim();
            this.port = port <= 0 ? 21 : port;
            credentials = new NetworkCredential(username ?? "anonymous", password ?? string.Empty);
        }

        internal string Host
        {
            get { return "ftp://" + host + ":" + port.ToString(CultureInfo.InvariantCulture); }
        }

        private Uri BuildUri(string remotePath)
        {
            UriBuilder builder = new UriBuilder("ftp", host, port);
            remotePath = NormalizePath(remotePath);
            builder.Path = "/" + remotePath;
            return builder.Uri;
        }

        internal static string NormalizePath(string remotePath)
        {
            if (string.IsNullOrEmpty(remotePath))
                return string.Empty;

            remotePath = remotePath.Replace('\\', '/').Trim();
            while (remotePath.StartsWith("/"))
                remotePath = remotePath.Substring(1);
            return remotePath;
        }

        internal static string Combine(string parentPath, string childName)
        {
            parentPath = NormalizePath(parentPath);
            childName = NormalizePath(childName);
            if (parentPath.Length == 0)
                return childName;
            if (childName.Length == 0)
                return parentPath;
            return parentPath.TrimEnd('/') + "/" + childName;
        }

        private static string NormalizeEntryName(string rawName)
        {
            rawName = NormalizePath(rawName).TrimEnd('/');
            if (rawName.Length == 0)
                return string.Empty;

            int separator = rawName.LastIndexOf('/');
            if (separator >= 0)
                rawName = rawName.Substring(separator + 1);

            return rawName.Trim().TrimEnd(':');
        }

        private static string BuildEntryPath(string parentPath, string rawName)
        {
            string normalizedRaw = NormalizePath(rawName).TrimEnd('/');
            if (normalizedRaw.Length == 0)
                return NormalizePath(parentPath);

            if (normalizedRaw.IndexOf('/') >= 0)
                return normalizedRaw;

            return Combine(parentPath, normalizedRaw);
        }

        private FtpWebRequest CreateRequest(string method, string remotePath, bool passive)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(BuildUri(remotePath));
            request.Method = method;
            request.Credentials = credentials;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.UsePassive = passive;
            request.Proxy = null;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 15000;
            return request;
        }

        private T WithPassiveFallback<T>(Func<bool, T> action)
        {
            try
            {
                return action(true);
            }
            catch (WebException)
            {
                return action(false);
            }
        }

        internal bool DirectoryExists(string remotePath)
        {
            return WithPassiveFallback<bool>(delegate(bool passive)
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, remotePath, passive).GetResponse())
                    return true;
            });
        }

        internal bool TryDirectoryExists(string remotePath, out string error)
        {
            error = null;
            try
            {
                return DirectoryExists(remotePath);
            }
            catch (WebException ex)
            {
                error = GetFriendlyError(ex);
                return false;
            }
        }

        internal bool TryListDirectory(string remotePath, out List<Entry> entries, out string error)
        {
            entries = null;
            error = null;
            try
            {
                entries = ListDirectory(remotePath);
                return true;
            }
            catch (WebException ex)
            {
                error = GetFriendlyError(ex);
                return false;
            }
        }

        internal List<Entry> ListDirectory(string remotePath)
        {
            return WithPassiveFallback<List<Entry>>(delegate (bool passive)
            {
                // [NOVA ADIÇÃO] Força a barra no final apenas na hora de listar, garantindo que o Xbox entre na pasta
                string pathForListing = remotePath;
                if (!string.IsNullOrEmpty(pathForListing) && !pathForListing.EndsWith("/"))
                    pathForListing += "/";

                List<Entry> entries = new List<Entry>();
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectoryDetails, pathForListing, passive).GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        Entry entry = ParseDetailedLine(remotePath, line); // Passa o remotePath original
                        if (entry != null)
                            entries.Add(entry);
                    }
                }

                if (entries.Count == 0)
                {
                    using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, pathForListing, passive).GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                    {
                        while (!reader.EndOfStream)
                        {
                            string rawName = reader.ReadLine();
                            if (string.IsNullOrEmpty(rawName))
                                continue;

                            string name = NormalizeEntryName(rawName);
                            string fullPath = BuildEntryPath(remotePath, rawName); // Passa o remotePath original
                            if (name.Length == 0)
                                continue;

                            entries.Add(new Entry()
                            {
                                Name = name,
                                FullPath = fullPath,
                                IsDirectory = PathIsDirectory(fullPath, passive)
                            });
                        }
                    }
                }

                return entries;
            });
        }

        private bool PathIsDirectory(string remotePath, bool passive)
        {
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, remotePath, passive).GetResponse())
                    return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        private static Entry ParseDetailedLine(string parentPath, string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
                return null;

            Entry entry = ParseUnixLine(parentPath, trimmed) ?? ParseDosLine(parentPath, trimmed) ?? ParseMachineLine(parentPath, trimmed);
            if (entry == null || entry.Name == "." || entry.Name == "..")
                return null;
            return entry;
        }

        private static Entry ParseMachineLine(string parentPath, string line)
        {
            int separator = line.LastIndexOf(';');
            if (separator <= 0 || separator >= line.Length - 1)
                return null;

            string factsPart = line.Substring(0, separator + 1);
            string rawName = line.Substring(separator + 1).Trim();
            string name = NormalizeEntryName(rawName);
            if (name.Length == 0)
                return null;

            bool? isDirectory = null;
            long size = 0;
            DateTime modified = DateTime.MinValue;
            string[] facts = factsPart.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string fact in facts)
            {
                int equalIndex = fact.IndexOf('=');
                if (equalIndex <= 0 || equalIndex >= fact.Length - 1)
                    continue;

                string key = fact.Substring(0, equalIndex).Trim();
                string value = fact.Substring(equalIndex + 1).Trim();
                if (key.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Equals("dir", StringComparison.OrdinalIgnoreCase) || value.Equals("cdir", StringComparison.OrdinalIgnoreCase) || value.Equals("pdir", StringComparison.OrdinalIgnoreCase))
                        isDirectory = true;
                    else if (value.Equals("file", StringComparison.OrdinalIgnoreCase))
                        isDirectory = false;
                }
                else if (key.Equals("size", StringComparison.OrdinalIgnoreCase))
                    long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
                else if (key.Equals("modify", StringComparison.OrdinalIgnoreCase))
                {
                    DateTime parsedModified;
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedModified)
                        || DateTime.TryParseExact(value, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedModified))
                        modified = parsedModified;
                }
            }

            if (!isDirectory.HasValue)
                return null;

            return new Entry()
            {
                Name = name,
                FullPath = BuildEntryPath(parentPath, rawName),
                IsDirectory = isDirectory.Value,
                Size = size,
                Modified = modified
            };
        }

        private static Entry ParseUnixLine(string parentPath, string line)
        {
            if (line.Length < 10 || (line[0] != 'd' && line[0] != '-' && line[0] != 'l'))
                return null;

            string[] parts = line.Split(new char[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 9)
                return null;

            long size;
            long.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
            DateTime modified = DateTime.MinValue;
            DateTime.TryParse(parts[5] + " " + parts[6] + " " + parts[7], CultureInfo.InvariantCulture, DateTimeStyles.None, out modified);

            string rawName = parts[8];
            int linkSeparator = rawName.IndexOf(" -> ", StringComparison.Ordinal);
            if (linkSeparator >= 0)
                rawName = rawName.Substring(0, linkSeparator);

            string name = NormalizeEntryName(rawName);
            if (name.Length == 0)
                return null;

            return new Entry()
            {
                Name = name,
                FullPath = BuildEntryPath(parentPath, rawName),
                IsDirectory = line[0] == 'd',
                Size = size,
                Modified = modified
            };
        }

        private static Entry ParseDosLine(string parentPath, string line)
        {
            if (line.Length < 17 || line[2] != '-' || line[5] != '-')
                return null;

            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                return null;

            bool isDirectory = string.Equals(parts[2], "<DIR>", StringComparison.OrdinalIgnoreCase);
            long size = 0;
            if (!isDirectory)
                long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out size);

            DateTime modified = DateTime.MinValue;
            DateTime.TryParse(parts[0] + " " + parts[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out modified);

            string rawName = line.Substring(line.IndexOf(parts[3], StringComparison.Ordinal)).Trim();
            string name = NormalizeEntryName(rawName);
            if (name.Length == 0)
                return null;

            return new Entry()
            {
                Name = name,
                FullPath = BuildEntryPath(parentPath, rawName),
                IsDirectory = isDirectory,
                Size = size,
                Modified = modified
            };
        }

        internal EndianIO DownloadFileToIO(string remotePath)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "__horizon_ftp_" + Guid.NewGuid().ToString("N") + ".tmp");
            DownloadFile(remotePath, tempPath);
            return new EndianIO(tempPath, EndianType.BigEndian, true);
        }

        private void DownloadFile(string remotePath, string localPath)
        {
            WithPassiveFallback<object>(delegate(bool passive)
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.DownloadFile, remotePath, passive).GetResponse())
                using (Stream input = response.GetResponseStream())
                using (FileStream output = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[0x8000];
                    int read;
                    while (input != null && (read = input.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, read);
                }
                return null;
            });
        }

        internal static string GetFriendlyError(WebException ex)
        {
            if (ex == null)
                return "Unknown FTP error.";

            FtpWebResponse response = ex.Response as FtpWebResponse;
            if (response != null)
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.StatusDescription.Trim(), (int)response.StatusCode);

            return ex.Message;
        }
    }
}
