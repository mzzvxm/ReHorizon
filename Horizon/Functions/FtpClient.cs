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
        internal enum TransferMode
        {
            Auto = 0,
            Passive = 1,
            Active = 2
        }

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
        private readonly TransferMode transferMode;

        internal FtpClient(string host, int port, string username, string password)
            : this(host, port, username, password, TransferMode.Auto)
        {
        }

        internal FtpClient(string host, int port, string username, string password, TransferMode transferMode)
        {
            this.host = (host ?? string.Empty).Trim();
            if (this.host.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                this.host = this.host.Substring(6);
            this.host = this.host.Trim('/').Trim();
            this.port = port <= 0 ? 21 : port;
            this.transferMode = transferMode;
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
            builder.Path = remotePath.Length == 0 ? "/" : "/" + remotePath;
            return builder.Uri;
        }

        internal static string NormalizePath(string remotePath)
        {
            if (string.IsNullOrEmpty(remotePath))
                return string.Empty;

            remotePath = remotePath.Replace('\\', '/').Trim();
            while (remotePath.StartsWith("/", StringComparison.Ordinal))
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

        private static void AddPathCandidate(List<string> candidates, string candidate)
        {
            candidate = NormalizePath(candidate);
            for (int i = 0; i < candidates.Count; i++)
                if (string.Equals(candidates[i], candidate, StringComparison.OrdinalIgnoreCase))
                    return;
            candidates.Add(candidate);
        }

        private static List<string> BuildPathCandidates(string remotePath, bool directoryPath)
        {
            string normalized = NormalizePath(remotePath);
            List<string> candidates = new List<string>();

            AddPathCandidate(candidates, normalized);

            string withoutSlash = normalized.TrimEnd('/');
            AddPathCandidate(candidates, withoutSlash);

            if (directoryPath && withoutSlash.Length != 0)
                AddPathCandidate(candidates, withoutSlash + "/");

            string withoutColon = withoutSlash.TrimEnd(':');
            if (withoutColon.Length != withoutSlash.Length)
            {
                AddPathCandidate(candidates, withoutColon);
                if (directoryPath && withoutColon.Length != 0)
                    AddPathCandidate(candidates, withoutColon + "/");

                AddPathCandidate(candidates, withoutColon + ":");
                if (directoryPath && withoutColon.Length != 0)
                    AddPathCandidate(candidates, withoutColon + ":/");
            }
            else if (withoutSlash.Length != 0 && withoutSlash.IndexOf('/') < 0)
            {
                AddPathCandidate(candidates, withoutSlash + ":");
                if (directoryPath)
                    AddPathCandidate(candidates, withoutSlash + ":/");
            }

            int firstSeparator = withoutSlash.IndexOf('/');
            if (firstSeparator > 0 && firstSeparator < withoutSlash.Length - 1)
            {
                string firstSegment = withoutSlash.Substring(0, firstSeparator);
                string remainingPath = withoutSlash.Substring(firstSeparator + 1);
                string firstWithoutColon = firstSegment.TrimEnd(':');
                bool hadColon = firstWithoutColon.Length != firstSegment.Length;

                if (hadColon)
                {
                    AddPathCandidate(candidates, firstWithoutColon + "/" + remainingPath);
                    if (directoryPath)
                        AddPathCandidate(candidates, firstWithoutColon + "/" + remainingPath + "/");
                }
                else
                {
                    AddPathCandidate(candidates, firstSegment + ":/" + remainingPath);
                    if (directoryPath)
                        AddPathCandidate(candidates, firstSegment + ":/" + remainingPath + "/");
                }
            }

            if (candidates.Count == 0)
                candidates.Add(string.Empty);

            return candidates;
        }

        private bool[] GetTransferModes()
        {
            switch (transferMode)
            {
                case TransferMode.Passive:
                    return new bool[] { true };
                case TransferMode.Active:
                    return new bool[] { false };
                default:
                    return new bool[] { true, false };
            }
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
            request.Timeout = 12000;
            request.ReadWriteTimeout = 20000;
            return request;
        }

        private static bool IsFatalRetryError(WebException ex)
        {
            if (ex == null)
                return false;

            if (ex.Status == WebExceptionStatus.NameResolutionFailure
                || ex.Status == WebExceptionStatus.ProxyNameResolutionFailure
                || ex.Status == WebExceptionStatus.TrustFailure
                || ex.Status == WebExceptionStatus.SecureChannelFailure)
                return true;

            FtpWebResponse response = ex.Response as FtpWebResponse;
            return response != null && response.StatusCode == FtpStatusCode.NotLoggedIn;
        }

        private static bool IsListDetailsUnavailable(WebException ex)
        {
            FtpWebResponse response = ex == null ? null : ex.Response as FtpWebResponse;
            if (response == null)
                return false;

            switch (response.StatusCode)
            {
                case FtpStatusCode.CommandSyntaxError:
                case FtpStatusCode.CommandNotImplemented:
                case FtpStatusCode.ArgumentSyntaxError:
                case FtpStatusCode.ActionNotTakenFileUnavailable:
                case FtpStatusCode.ActionNotTakenFilenameNotAllowed:
                    return true;
            }

            return false;
        }

        private T ExecuteWithFallback<T>(string remotePath, bool directoryPath, Func<bool, string, T> operation)
        {
            WebException lastError = null;
            List<string> pathCandidates = BuildPathCandidates(remotePath, directoryPath);
            bool[] modes = GetTransferModes();

            for (int modeIndex = 0; modeIndex < modes.Length; modeIndex++)
            {
                bool passive = modes[modeIndex];
                for (int pathIndex = 0; pathIndex < pathCandidates.Count; pathIndex++)
                {
                    try
                    {
                        return operation(passive, pathCandidates[pathIndex]);
                    }
                    catch (WebException ex)
                    {
                        lastError = ex;
                        if (IsFatalRetryError(ex))
                            throw;
                    }
                }
            }

            if (lastError != null)
                throw lastError;

            throw new WebException("FTP request failed.");
        }

        internal bool DirectoryExists(string remotePath)
        {
            return ExecuteWithFallback<bool>(remotePath, true, delegate (bool passive, string candidatePath)
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, candidatePath, passive).GetResponse())
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

        private bool TryListDirectoryDetails(string remotePath, bool passive, out List<Entry> entries)
        {
            entries = null;
            try
            {
                entries = new List<Entry>();
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath, passive).GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        Entry entry = ParseDetailedLine(remotePath, line);
                        if (entry != null)
                            entries.Add(entry);
                    }
                }
                return true;
            }
            catch (WebException ex)
            {
                if (IsListDetailsUnavailable(ex))
                    return false;
                throw;
            }
        }

        private static bool RawNameIndicatesDirectory(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
                return false;

            string trimmed = rawName.Trim();
            return trimmed.EndsWith("/", StringComparison.Ordinal)
                || trimmed.EndsWith("\\", StringComparison.Ordinal)
                || trimmed.EndsWith(":", StringComparison.Ordinal);
        }

        private bool TryGetFileSize(string remotePath, bool passive, out long size)
        {
            size = 0;
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.GetFileSize, remotePath, passive).GetResponse())
                {
                    if (response.ContentLength >= 0)
                        size = response.ContentLength;
                }
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        private bool TryListDirectoryCommand(string remotePath, bool passive, out int itemCount, out string firstItem)
        {
            itemCount = 0;
            firstItem = null;
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, remotePath, passive).GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        if (firstItem == null)
                            firstItem = line;
                        itemCount++;
                    }
                }
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        private bool PathIsDirectory(string remotePath, bool passive)
        {
            string normalized = NormalizePath(remotePath).TrimEnd('/');
            if (normalized.Length == 0)
                return true;

            int itemCount;
            string firstItem;
            if (TryListDirectoryCommand(normalized + "/", passive, out itemCount, out firstItem))
                return true;

            long size;
            if (TryGetFileSize(normalized, passive, out size))
                return false;

            if (TryListDirectoryCommand(normalized, passive, out itemCount, out firstItem))
            {
                if (itemCount > 1)
                    return true;

                if (itemCount == 1)
                {
                    string childName = NormalizeEntryName(firstItem);
                    string leafName = NormalizeEntryName(normalized);
                    if (!string.Equals(childName, leafName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private List<Entry> ListDirectoryNames(string remotePath, bool passive)
        {
            List<Entry> entries = new List<Entry>();
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.ListDirectory, remotePath, passive).GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    while (!reader.EndOfStream)
                    {
                        string rawName = reader.ReadLine();
                        if (string.IsNullOrEmpty(rawName))
                            continue;

                        Entry parsedFromList = ParseDetailedLine(remotePath, rawName);
                        if (parsedFromList != null)
                        {
                            entries.Add(parsedFromList);
                            continue;
                        }

                        string name = NormalizeEntryName(rawName);
                        if (name.Length == 0 || name == "." || name == "..")
                            continue;

                        string fullPath = BuildEntryPath(remotePath, rawName);
                        bool isDirectory = RawNameIndicatesDirectory(rawName) || PathIsDirectory(fullPath, passive);
                        long size = 0;
                        if (!isDirectory)
                            TryGetFileSize(fullPath, passive, out size);

                        entries.Add(new Entry()
                        {
                            Name = name,
                            FullPath = fullPath,
                            IsDirectory = isDirectory,
                            Size = size,
                            Modified = DateTime.MinValue
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = ex.Response as FtpWebResponse;
                if (response != null && (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable || response.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed))
                {
                    return entries;
                }
                throw;
            }

            return entries;
        }

        internal List<Entry> ListDirectory(string remotePath)
        {
            return ExecuteWithFallback<List<Entry>>(remotePath, true, delegate (bool passive, string candidatePath)
            {
                List<Entry> entries;
                if (TryListDirectoryDetails(candidatePath, passive, out entries))
                {
                    if (entries.Count != 0)
                        return entries;

                    return ListDirectoryNames(candidatePath, passive);
                }

                return ListDirectoryNames(candidatePath, passive);
            });
        }

        private static Entry ParseDetailedLine(string parentPath, string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
                return null;

            // O FallbackLine foi injetado para agir caso nenhum dos 3 padrões engessados funcione.
            Entry entry = ParseUnixLine(parentPath, trimmed) ?? ParseDosLine(parentPath, trimmed) ?? ParseMachineLine(parentPath, trimmed) ?? ParseFallbackLine(parentPath, trimmed);
            if (entry == null || entry.Name == "." || entry.Name == "..")
                return null;
            return entry;
        }

        private static Entry ParseFallbackLine(string parentPath, string line)
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;

            bool isDir = line.StartsWith("d", StringComparison.OrdinalIgnoreCase) || line.IndexOf("<DIR>", StringComparison.OrdinalIgnoreCase) >= 0;

            int nameStartIndex = parts.Length - 1;
            for (int i = parts.Length - 2; i >= 0; i--)
            {
                // Procura o último token que seja um relógio/data
                if (parts[i].Contains(":") || (parts[i].Contains("/") && char.IsDigit(parts[i][0])) || (parts[i].Contains("-") && char.IsDigit(parts[i][0])))
                {
                    nameStartIndex = i + 1;
                    break;
                }
            }

            if (nameStartIndex < parts.Length - 1 && parts[nameStartIndex].Equals("<DIR>", StringComparison.OrdinalIgnoreCase))
                nameStartIndex++;

            string rawName = parts[nameStartIndex];
            for (int i = nameStartIndex + 1; i < parts.Length; i++)
                rawName += " " + parts[i];

            string name = NormalizeEntryName(rawName);
            if (name.Length == 0 || name == "." || name == "..") return null;

            long size = 0;
            if (!isDir)
            {
                for (int i = nameStartIndex - 1; i >= 0; i--)
                {
                    if (long.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out size))
                        break;
                }
            }

            return new Entry()
            {
                Name = name,
                FullPath = BuildEntryPath(parentPath, rawName),
                IsDirectory = isDir,
                Size = size,
                Modified = DateTime.MinValue
            };
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
                    if (value.Equals("dir", StringComparison.OrdinalIgnoreCase)
                        || value.Equals("cdir", StringComparison.OrdinalIgnoreCase)
                        || value.Equals("pdir", StringComparison.OrdinalIgnoreCase))
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

            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
                return null;

            bool isDirectory = line[0] == 'd';

            int dateStartIndex = -1;
            for (int i = 3; i < parts.Length - 1; i++)
            {
                string p = parts[i].ToLower();
                if (p.Contains(":") || p.Contains("/") || p.Contains("-") ||
                    p == "jan" || p == "feb" || p == "mar" || p == "apr" || p == "may" || p == "jun" ||
                    p == "jul" || p == "aug" || p == "sep" || p == "oct" || p == "nov" || p == "dec")
                {
                    dateStartIndex = i;
                    break;
                }
            }

            int nameIndex = -1;
            long size = 0;

            if (dateStartIndex != -1)
            {
                long.TryParse(parts[dateStartIndex - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out size);

                int tokensToSkip = 1;
                for (int i = dateStartIndex + 1; i < parts.Length; i++)
                {
                    string p = parts[i];
                    if (p.Contains(":") || p.Contains("/") || p.Contains("-") || (p.Length == 4 && int.TryParse(p, out _)) || (p.Length <= 2 && int.TryParse(p, out _)))
                    {
                        tokensToSkip++;
                    }
                    else
                    {
                        break;
                    }
                }

                nameIndex = dateStartIndex + tokensToSkip;
            }
            else
            {
                nameIndex = parts.Length > 8 ? 8 : parts.Length - 1;
                long.TryParse(parts[parts.Length > 8 ? 4 : 3], NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
            }

            if (nameIndex >= parts.Length) return null;

            string rawName = parts[nameIndex];
            for (int i = nameIndex + 1; i < parts.Length; i++)
            {
                rawName += " " + parts[i];
            }

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
                IsDirectory = isDirectory,
                Size = size,
                Modified = DateTime.MinValue
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
            ExecuteWithFallback<object>(remotePath, false, delegate (bool passive, string candidatePath)
            {
                using (FtpWebResponse response = (FtpWebResponse)CreateRequest(WebRequestMethods.Ftp.DownloadFile, candidatePath, passive).GetResponse())
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