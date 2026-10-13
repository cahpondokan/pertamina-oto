using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Oto.Setup
{
    internal sealed class SetupOperations : ISetupOperations, IDisposable
    {
        private string folder;
        public bool RuntimeBundled { get { return PayloadInfo.RuntimeBundled; } }
        internal static string InstalledLauncher { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PertaminaOTO", "PERTAMINA-OTO.exe"); } }
        public string DetectRuntime() { return RuntimeRequirement.Detect(); }

        internal static Stream Payload()
        { return Assembly.GetExecutingAssembly().GetManifestResourceStream("OTO.Payload.zip") ?? throw new InvalidDataException("Paket OTO tidak ditemukan. Unduh ulang installer."); }
        internal static void VerifyPayload()
        {
            using (var payload = Payload())
            using (var sha = SHA256.Create())
            {
                if (!string.Equals(BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", ""), PayloadInfo.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Paket OTO rusak. Unduh ulang installer dari oto.ayani.id.");
            }
        }
        internal static string SafeEntry(string root, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || Path.IsPathRooted(name) || name.Contains(':') || name.Split('/', '\\').Any(x => x.Length == 0 || x == "." || x == ".." || x.EndsWith(".", StringComparison.Ordinal) || x.EndsWith(" ", StringComparison.Ordinal)))
                throw new InvalidDataException("Lokasi berkas paket tidak sesuai.");
            var full = Path.GetFullPath(Path.Combine(root, name));
            if (!full.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Lokasi paket keluar dari folder pemasangan.");
            NoLinks(full); return full;
        }
        internal static void NoLinks(string path)
        {
            for (var part = Path.GetFullPath(path); part != null; part = Path.GetDirectoryName(part))
                if ((File.Exists(LongPath(part)) || Directory.Exists(LongPath(part))) && (File.GetAttributes(LongPath(part)) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Folder pemasangan menggunakan tautan yang tidak didukung.");
        }
        private static string LongPath(string path)
        {
            var full = Path.GetFullPath(path);
            return full.StartsWith(@"\\?\", StringComparison.Ordinal) ? full : full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full.Substring(2) : @"\\?\" + full;
        }
        internal static string CreatePrivateFolder()
        {
            var path = Path.Combine(Path.GetTempPath(), "OTO-Setup-" + Guid.NewGuid().ToString("N")); NoLinks(path);
            var security = new DirectorySecurity(); security.SetAccessRuleProtection(true, false);
            foreach (var sid in new[] { WindowsIdentity.GetCurrent().User, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) })
                security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.CreateDirectory(path, security); return path;
        }
        internal static void Extract(Stream input, string destination, long expectedBytes, IProgress<SetupProgress> progress, CancellationToken cancellation)
        {
            using (var zip = new ZipArchive(input, ZipArchiveMode.Read, true))
            {
                if (zip.Entries.Count == 0 || zip.Entries.Count > 10000 || zip.Entries.Sum(e => e.Length) != expectedBytes || expectedBytes > 2L * 1024 * 1024 * 1024)
                    throw new InvalidDataException("Ukuran paket OTO tidak sesuai.");
                var fileNames = new System.Collections.Generic.HashSet<string>(zip.Entries.Select(e => e.FullName.TrimEnd('/')), StringComparer.OrdinalIgnoreCase);
                foreach (var entry in zip.Entries)
                {
                    var name = entry.FullName;
                    if (name.Contains('\\') || name.Any(c => char.IsControl(c) || "<>\"|?*".Contains(c))
                        || name.Split('/').Any(Reserved) || (entry.ExternalAttributes >> 16 & 0xF000) == 0xA000
                        || (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0
                        || entry.Length > 1024L * 1024 * 1024 || (entry.Length > 1024 * 1024 && entry.Length / Math.Max(1, entry.CompressedLength) > 1000))
                        throw new InvalidDataException("Isi paket tidak aman.");
                    for (var parent = Path.GetDirectoryName(name)?.Replace('\\', '/'); !string.IsNullOrEmpty(parent); parent = Path.GetDirectoryName(parent)?.Replace('\\', '/'))
                        if (fileNames.Contains(parent)) throw new InvalidDataException("Lokasi file paket bertabrakan.");
                }
                var entries = zip.Entries.Select(e => SafeEntry(destination, e.FullName)).ToArray();
                if (entries.Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Length) throw new InvalidDataException("Paket berisi nama file ganda.");
                long extracted = 0;
                for (var i = 0; i < zip.Entries.Count; i++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    var entry = zip.Entries[i]; var path = entries[i];
                    Directory.CreateDirectory(LongPath(Path.GetDirectoryName(path)));
                    using (var source = entry.Open())
                    using (var target = new FileStream(LongPath(path), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920]; int count; long fileBytes = 0;
                        while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellation.ThrowIfCancellationRequested(); fileBytes += count; extracted += count;
                            if (fileBytes > entry.Length || extracted > expectedBytes) throw new InvalidDataException("Isi paket OTO melampaui batas.");
                            target.Write(buffer, 0, count);
                        }
                        if (fileBytes != entry.Length) throw new InvalidDataException("Isi paket OTO tidak lengkap.");
                    }
                    progress.Report(new SetupProgress("Menyiapkan OTO", "Membaca paket aplikasi yang sudah disertakan di installer…", (int)(extracted * 100 / Math.Max(1, expectedBytes)), true));
                }
            }
        }
        private static bool Reserved(string part)
        {
            var stem = part.Split('.')[0];
            return stem.Equals("CON", StringComparison.OrdinalIgnoreCase) || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase)
                || stem.Equals("AUX", StringComparison.OrdinalIgnoreCase) || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase)
                || (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) && "0123456789¹²³".Contains(stem[3]));
        }

        public Task PrepareAsync(IProgress<SetupProgress> progress, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                cancellation.ThrowIfCancellationRequested(); VerifyPayload();
                if (!RuntimeRequirement.SupportedWindows()) throw new InvalidOperationException("Installer ini memerlukan Windows 10 versi 2004 atau lebih baru, dengan prosesor x64.");
                if (!RuntimeRequirement.HasDiskSpace(PayloadInfo.ExpandedBytes, RuntimeBundled || DetectRuntime() != null)) throw new IOException("Ruang penyimpanan belum cukup. Kosongkan ruang pada drive aplikasi dan folder sementara, lalu coba kembali.");
                folder = CreatePrivateFolder();
                using (var payload = Payload()) Extract(payload, folder, PayloadInfo.ExpandedBytes, progress, cancellation);
            }, cancellation);
        }
        public async Task DownloadRuntimeAsync(IProgress<SetupProgress> progress, CancellationToken cancellation)
        {
            if (RuntimeBundled) throw new InvalidOperationException("Paket ini tidak boleh mengunduh runtime terpisah.");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, CheckCertificateRevocationList = true })
            using (var client = new HttpClient(handler))
            {
                try { await TrustedRuntime.DownloadAsync(client, Path.Combine(folder, "Microsoft-dotnet-runtime.exe"), progress, cancellation).ConfigureAwait(false); }
                catch (Exception) when (cancellation.IsCancellationRequested) { throw new OperationCanceledException(cancellation); }
            }
        }
        public Task<int> InstallRuntimeAsync()
        {
            if (RuntimeBundled) throw new InvalidOperationException("Paket ini tidak boleh memasang runtime terpisah.");
            return Task.Run(() =>
            {
                var file = Path.Combine(folder, "Microsoft-dotnet-runtime.exe");
                // Keep the verified executable locked against replacement until its process exits.
                using (var locked = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    TrustedRuntime.VerifyHash(locked, RuntimeRequirement.Sha512);
                    TrustedRuntime.VerifyMicrosoftSignature(file);
                    return Run(new ProcessStartInfo(file, "/install /passive /norestart") { UseShellExecute = true, Verb = "runas", WorkingDirectory = folder });
                }
            });
        }
        public Task<int> InstallAppAsync()
        { return Task.Run(() => Run(new ProcessStartInfo(Path.Combine(folder, "Setup.exe"), "--install-silent") { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = folder })); }

        private static int Run(ProcessStartInfo start)
        {
            try { using (var process = Process.Start(start)) { if (process == null) throw new IOException("Pemasang belum dapat dibuka."); process.WaitForExit(); return process.ExitCode; } }
            catch (Win32Exception e) when (e.NativeErrorCode == 1223) { return 1223; }
        }
        internal static void CleanPrivateFolder(string path)
        {
            var full = Path.GetFullPath(path); var parent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
            if (!string.Equals(Path.GetDirectoryName(full), parent, StringComparison.OrdinalIgnoreCase) || !System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(full), "^OTO-Setup-[a-f0-9]{32}$")) throw new InvalidDataException("Folder pembersihan tidak sesuai.");
            NoLinks(full); CheckTree(LongPath(full)); Directory.Delete(LongPath(full), true);
        }
        private static void CheckTree(string path)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(path))
            { if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Tautan tidak dihapus."); if (Directory.Exists(entry)) CheckTree(entry); }
        }
        public void Dispose()
        {
            if (folder == null) return;
            try { CleanPrivateFolder(folder); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is InvalidDataException) { /* Retain only this private staging directory if cleanup is unsafe or files are still in use. */ }
            folder = null;
        }
    }
}
