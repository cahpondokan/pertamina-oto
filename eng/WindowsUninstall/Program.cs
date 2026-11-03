using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("PERTAMINA OTO Uninstall")]
[assembly: AssemblyProduct("PERTAMINA OTO")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8")]

namespace Oto.Uninstall
{
    internal static class Program
    {
        internal const string Exe = "Uninstall-OTO.exe";
        internal const string Key = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PertaminaOTO";
        internal static string Root { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PertaminaOTO"); } }
        internal static readonly Regex VersionId = new Regex(@"^\d{8}-\d{6}-[a-f0-9]{8}$", RegexOptions.CultureInvariant);
        internal sealed class Release { public string Version { get; set; } public string ProductVersion { get; set; } }
        internal sealed class Journal { public string Version { get; set; } public string[] Files { get; set; } public string[] Directories { get; set; } }
        internal sealed class Capability { public CapabilityFile[] Files { get; set; } }
        internal sealed class CapabilityFile { public string Path { get; set; } }
        internal sealed class Plan
        {
            internal readonly List<string> Files = new List<string>();
            internal readonly List<string> Directories = new List<string>();
        }

        [STAThread]
        private static int Main(string[] args)
        {
            AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false);
            AppContext.SetSwitch("Switch.System.IO.BlockLongPaths", false);
            Application.EnableVisualStyles();
            try
            {
                if (args.Length == 2 && args[0] == "--self-test") return Tests.Run(Path.GetFullPath(args[1]));
                if (args.Length == 1 && args[0] == "--register") { Register(Root, Registry.CurrentUser); return 0; }
                if (args.Length == 1 && args[0] == "--repair")
                {
                    Register(Root, Registry.CurrentUser);
                    MessageBox.Show("PERTAMINA OTO sudah didaftarkan. Buka kembali Pengaturan Windows > Aplikasi > Aplikasi terinstal.", "PERTAMINA OTO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                if (args.Length == 1 && args[0] == "--uninstall")
                {
                    ValidateInstallation(Root, true);
                    // Run outside the application directory so the uninstaller can remove itself.
                    var stage = Path.Combine(Path.GetTempPath(), "OTO-Uninstall-" + Guid.NewGuid().ToString("N"));
                    NoLinks(stage); Directory.CreateDirectory(stage);
                    var staged = Path.Combine(stage, Exe);
                    File.Copy(Assembly.GetExecutingAssembly().Location, staged);
                    File.Copy(Assembly.GetExecutingAssembly().Location + ".config", staged + ".config");
                    Process.Start(new ProcessStartInfo(staged, "--confirm-uninstall") { UseShellExecute = false, WorkingDirectory = stage });
                    return 0;
                }
                if (args.Length != 1 || args[0] != "--confirm-uninstall") return 1;
                // No caller-supplied root, silent deletion flag, elevation, or process termination.
                if (Path.GetFullPath(Assembly.GetExecutingAssembly().Location).StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return 1;
                ValidateInstallation(Root, true);
                if (MessageBox.Show("Hapus PERTAMINA OTO dan versi aplikasi sebelumnya dari akun Windows ini?\n\nSimpan pekerjaan dan tutup semua jendela OTO terlebih dahulu.\n\nDokumen hasil, riwayat, dan pengaturan di luar folder pemasangan tetap disimpan.", "Uninstall PERTAMINA OTO", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return 0;
                Remove(Root, Registry.CurrentUser, true);
                MessageBox.Show("PERTAMINA OTO telah dihapus. Dokumen, riwayat, dan pengaturan pribadi tetap disimpan.", "PERTAMINA OTO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }
            catch (Exception error)
            {
                // Safe diagnostic code only; never log document paths or exception contents.
                if (args.Length == 1 && args[0] == "--register") return 1;
                MessageBox.Show("Proses belum selesai. Simpan pekerjaan dan tutup OTO, lalu coba lagi. Jika tetap gagal, folder pemasangan perlu diperiksa.\nKode: " + error.HResult.ToString("X8", CultureInfo.InvariantCulture), "PERTAMINA OTO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 1;
            }
        }

        internal static void NoLinks(string path)
        {
            var full = Path.GetFullPath(path);
            if (full.StartsWith(@"\\", StringComparison.Ordinal) || full.IndexOf('"') >= 0) throw new InvalidDataException();
            for (var part = full; part != null; part = Path.GetDirectoryName(part))
                if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException();
        }
        internal static string Child(string root, string relative)
        {
            if (string.IsNullOrEmpty(relative) || Path.IsPathRooted(relative) || relative.Contains(':') || relative.Split('/', '\\').Any(p => p.Length == 0 || p == "." || p == ".." || p.EndsWith(".", StringComparison.Ordinal) || p.EndsWith(" ", StringComparison.Ordinal))) throw new InvalidDataException();
            var full = Path.GetFullPath(Path.Combine(root, relative));
            if (!full.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException();
            NoLinks(full); return full;
        }
        internal static Release ReadRelease(string file)
        {
            if (new FileInfo(file).Length > 65536) throw new InvalidDataException();
            var value = new JavaScriptSerializer().Deserialize<Release>(File.ReadAllText(file));
            if (value == null || value.Version == null || !VersionId.IsMatch(value.Version)) throw new InvalidDataException();
            return value;
        }
        internal static Release ValidateInstallation(string root, bool allowPending = false)
        {
            NoLinks(root);
            var release = ReadRelease(Child(root, "current.json"));
            var pending = Child(root, "uninstall-pending.json");
            if (allowPending && File.Exists(pending)) { ReadJournal(root, release.Version); return release; }
            if (!File.Exists(Child(root, "PERTAMINA-OTO.exe")) || !File.Exists(Child(root, "versions/" + release.Version + "/Pertamina.Oto.Desktop.App.exe"))) throw new InvalidDataException();
            return release;
        }
        internal static string Hash(string file)
        { using (var sha = SHA256.Create()) using (var stream = File.OpenRead(file)) return BitConverter.ToString(sha.ComputeHash(stream)); }

        internal static void Register(string root, RegistryKey registry)
        {
            var release = ValidateInstallation(root);
            using (var gate = new FileStream(Child(root, "setup.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                foreach (var suffix in new[] { "", ".config" })
                {
                    var source = Assembly.GetExecutingAssembly().Location + suffix;
                    var target = Child(root, Exe + suffix);
                    if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase) || File.Exists(target) && Hash(source) == Hash(target)) continue;
                    var pending = Child(root, Exe + suffix + ".pending-" + Guid.NewGuid().ToString("N"));
                    File.Copy(source, pending, false);
                    if (Hash(pending) != Hash(source)) throw new IOException();
                    if (File.Exists(target)) File.Replace(pending, target, null); else File.Move(pending, target);
                }
                using (var key = registry.CreateSubKey(Key))
                {
                    key.SetValue("DisplayName", "PERTAMINA OTO");
                    key.SetValue("DisplayVersion", release.ProductVersion != null && Regex.IsMatch(release.ProductVersion, @"^\d+\.\d+\.\d+$") ? release.ProductVersion : release.Version);
                    key.SetValue("Publisher", "PERTAMINA OTO");
                    key.SetValue("InstallLocation", Path.GetFullPath(root));
                    key.SetValue("DisplayIcon", "\"" + Child(root, "PERTAMINA-OTO.exe") + "\",0");
                    key.SetValue("UninstallString", "\"" + Child(root, Exe) + "\" --uninstall");
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    key.SetValue("SystemComponent", 0, RegistryValueKind.DWord);
                    key.Flush();
                }
            }
        }

        internal static Plan BuildPlan(string root)
        {
            var release = ValidateInstallation(root, true);
            if (File.Exists(Child(root, "uninstall-pending.json"))) return ReadJournal(root, release.Version);
            var plan = new Plan();
            var versions = Child(root, "versions");
            foreach (var directory in Directory.GetDirectories(versions))
            {
                var name = Path.GetFileName(directory);
                if (!VersionId.IsMatch(name)) continue; // Unknown/staging folders remain available for recovery.
                var exe = Child(root, "versions/" + name + "/Pertamina.Oto.Desktop.App.exe");
                if (!File.Exists(exe)) throw new InvalidDataException();
                AddTree(root, directory, plan);
            }
            foreach (var directory in Directory.GetDirectories(root))
            {
                var name = Path.GetFileName(directory);
                if (Regex.IsMatch(name, @"^launcher-backup-[a-f0-9]{32}$") || name == "pdf-open") AddTree(root, directory, plan);
            }
            // Root files are deliberately allowlisted. Unknown files/folders are retained.
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PERTAMINA-OTO.exe", "PERTAMINA-OTO.dll", "PERTAMINA-OTO.deps.json", "PERTAMINA-OTO.runtimeconfig.json", "createdump.exe", "Microsoft.NETCore.App.deps.json", "LICENSE.TXT", "THIRD-PARTY-NOTICES.TXT", "restart-capability.json", "update-source.json", "launcher-error.txt" };
            var capabilityPath = Child(root, "restart-capability.json");
            if (File.Exists(capabilityPath))
            {
                if (new FileInfo(capabilityPath).Length > 262144) throw new InvalidDataException();
                var capability = new JavaScriptSerializer().Deserialize<Capability>(File.ReadAllText(capabilityPath));
                if (capability == null || capability.Files == null || capability.Files.Length > 300) throw new InvalidDataException();
                foreach (var entry in capability.Files)
                {
                    if (entry == null || entry.Path == null || entry.Path.Contains('/') || entry.Path.Contains('\\') || !(names.Contains(entry.Path) || entry.Path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException();
                    Child(root, entry.Path); names.Add(entry.Path);
                }
            }
            foreach (var file in Directory.GetFiles(root))
                if (names.Contains(Path.GetFileName(file))) { NoLinks(file); plan.Files.Add(file); }
            return plan;
        }
        private static Plan ReadJournal(string root, string version)
        {
            var file = Child(root, "uninstall-pending.json");
            if (new FileInfo(file).Length > 16 * 1024 * 1024) throw new InvalidDataException();
            var journal = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 }.Deserialize<Journal>(File.ReadAllText(file));
            if (journal == null || journal.Version != version || journal.Files == null || journal.Directories == null || journal.Files.Length > 100000 || journal.Directories.Length > 100000) throw new InvalidDataException();
            var plan = new Plan();
            foreach (var path in journal.Files) plan.Files.Add(Child(root, path));
            foreach (var path in journal.Directories) plan.Directories.Add(Child(root, path));
            return plan;
        }
        private static void AddTree(string root, string directory, Plan plan)
        {
            NoLinks(directory);
            if (!Path.GetFullPath(directory).StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException();
            foreach (var file in Directory.GetFiles(directory)) { NoLinks(file); plan.Files.Add(file); }
            foreach (var child in Directory.GetDirectories(directory)) AddTree(root, child, plan);
            plan.Directories.Add(directory); // Children before parents; never recursive delete.
        }
        internal static void CheckProcesses(string root)
        {
            foreach (var name in new[] { "PERTAMINA-OTO", "Pertamina.Oto.Desktop.App", "Pertamina.Oto.LocalPdf.Worker", "Pertamina.Oto.WordPdf.Worker", "PertaminaOTO.PdfOpener" })
                foreach (var process in Process.GetProcessesByName(name)) using (process)
                {
                    try
                    {
                        if (!process.HasExited && process.MainModule.FileName.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new IOException("OTO is running.");
                    }
                    catch (InvalidOperationException) { /* exited during inspection */ }
                }
        }
        internal static void Remove(string root, RegistryKey registry, bool checkProcesses)
        {
            var release = ValidateInstallation(root, true);
            if (checkProcesses) CheckProcesses(root);
            using (var key = registry.OpenSubKey(Key))
                if (key != null && !string.Equals(key.GetValue("InstallLocation") as string, root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException();
            var locks = new List<FileStream>();
            var lockNames = new[] { "setup.lock", "launcher-maintenance.lock", "update.lock", "pdf-association.lock" };
            try
            {
                foreach (var name in lockNames) locks.Add(new FileStream(Child(root, name), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None));
                var plan = BuildPlan(root);
                // Refuse before deleting anything when a payload file is in use.
                foreach (var file in plan.Files) if (File.Exists(file)) using (File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                var journal = Child(root, "uninstall-pending.json");
                if (!File.Exists(journal))
                {
                    var content = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 }.Serialize(new Journal { Version = release.Version, Files = plan.Files.Select(p => p.Substring(root.Length + 1)).ToArray(), Directories = plan.Directories.Select(p => p.Substring(root.Length + 1)).ToArray() });
                    using (var writer = new StreamWriter(new FileStream(journal, FileMode.CreateNew, FileAccess.Write, FileShare.None))) writer.Write(content);
                }
                foreach (var file in plan.Files) { NoLinks(file); if (File.Exists(file)) File.Delete(file); }
                foreach (var dir in plan.Directories) { NoLinks(dir); if (Directory.Exists(dir)) Directory.Delete(dir, false); }
                RemoveAssociations(root, registry);
                if (checkProcesses) RemoveShortcuts(root);
                // Retain both uninstall program and its registration until payload cleanup succeeds.
                registry.DeleteSubKeyTree(Key, false);
                foreach (var name in new[] { "current.json", "previous.json", "uninstall-pending.json", Exe + ".config", Exe }) { var path = Child(root, name); if (File.Exists(path)) File.Delete(path); }
            }
            finally { foreach (var gate in locks) gate.Dispose(); }
            foreach (var name in lockNames) { var path = Child(root, name); if (File.Exists(path)) File.Delete(path); }
            var versions = Child(root, "versions");
            if (Directory.Exists(versions) && !Directory.EnumerateFileSystemEntries(versions).Any()) Directory.Delete(versions, false);
            if (!Directory.EnumerateFileSystemEntries(root).Any()) Directory.Delete(root, false);
        }
        private static void RemoveAssociations(string root, RegistryKey registry)
        {
            const string prog = @"Software\Classes\PertaminaOTO.Pdf";
            const string app = @"Software\Classes\Applications\PertaminaOTO.PdfOpener.exe";
            bool owned = false;
            foreach (var path in new[] { prog, app })
            {
                using (var command = registry.OpenSubKey(path + @"\shell\open\command"))
                    owned = (command == null ? "" : command.GetValue("") as string ?? "").StartsWith("\"" + root + @"\pdf-open\", StringComparison.OrdinalIgnoreCase);
                if (owned) registry.DeleteSubKeyTree(path, false);
                if (path == prog && owned)
                {
                    using (var openWith = registry.OpenSubKey(@"Software\Classes\.pdf\OpenWithProgids", true)) if (openWith != null) openWith.DeleteValue("PertaminaOTO.Pdf", false);
                    registry.DeleteSubKeyTree(@"Software\PertaminaOTO\Capabilities", false);
                    using (var apps = registry.OpenSubKey(@"Software\RegisteredApplications", true)) if (apps != null && (apps.GetValue("PERTAMINA OTO") as string) == @"Software\PertaminaOTO\Capabilities") apps.DeleteValue("PERTAMINA OTO", false);
                }
            }
            // Windows owns .pdf UserChoice; never delete or rewrite it.
        }
        private static void RemoveShortcuts(string root)
        {
            foreach (var folder in new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Environment.GetFolderPath(Environment.SpecialFolder.Programs) })
            {
                var shortcut = Path.Combine(folder, "PERTAMINA OTO.lnk");
                if (!File.Exists(shortcut)) continue;
                NoLinks(shortcut);
                dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                dynamic link = shell.CreateShortcut(shortcut);
                if (string.Equals((string)link.TargetPath, Path.Combine(root, "PERTAMINA-OTO.exe"), StringComparison.OrdinalIgnoreCase)) File.Delete(shortcut);
            }
        }
    }
}
