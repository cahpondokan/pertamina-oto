using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;

internal static class PortableLauncher
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            Verify(root);
            if (args.Length == 1 && args[0] == "--verify") return 0;
            if (args.Length != 0) throw new InvalidDataException();
            if (!Environment.Is64BitOperatingSystem || Environment.OSVersion.Version.Build < 19041) throw new InvalidDataException();
            if (MessageBox.Show("PERTAMINA OTO 0.1.0 · Pengembangan\n\nUji pengembangan publik, bukan Production/stabil. Gunakan salinan dokumen tidak sensitif. Baca MULAI-DI-SINI.txt dan folder lisensi terlebih dahulu.\n\nOTO belum ditandatangani. Portable tidak menghindari Smart App Control atau kebijakan keamanan. Jangan menonaktifkan perlindungan.\n\nLanjutkan uji dengan batasan ini?", "PERTAMINA OTO", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return 0;
            var start = new ProcessStartInfo(Path.Combine(root, "app", "Pertamina.Oto.Desktop.App.exe")) { UseShellExecute = false, WorkingDirectory = Path.Combine(root, "app") };
            string runtime = Path.Combine(root, "runtime");
            if (Directory.Exists(runtime))
            {
                start.EnvironmentVariables["DOTNET_ROOT"] = runtime;
                start.EnvironmentVariables["DOTNET_ROOT_X64"] = runtime;
                start.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";
            }
            using (var child = Process.Start(start))
            {
                if (child == null || child.WaitForExit(5000)) throw new InvalidOperationException();
            }
            return 0;
        }
        catch (Exception error)
        {
            if (args.Length == 1 && args[0] == "--verify")
            {
                string evidence = Environment.GetEnvironmentVariable("OTO_PORTABLE_VERIFY_EVIDENCE");
                if (!String.IsNullOrEmpty(evidence))
                    try { File.WriteAllText(evidence, error.GetType().FullName + " | " + error.HResult.ToString("X8") + " | " + (error.TargetSite == null ? "" : error.TargetSite.Name)); } catch (Exception) { }
                return 1;
            }
            MessageBox.Show("Paket belum dapat dimulai. Ekstrak seluruh ZIP ke folder lokal. File wajib mungkin hilang/berubah atau prasyarat Windows/.NET belum tersedia. Baca MULAI-DI-SINI.txt. Jika diblokir keamanan, hentikan dan hubungi pengelola/IT; jangan menonaktifkan perlindungan.", "PERTAMINA OTO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static void Verify(string root)
    {
        string manifestPath = Path.Combine(root, "package-manifest.json");
        RejectLinks(manifestPath);
        if (new FileInfo(WindowsPath(manifestPath)).Length > 4 * 1024 * 1024) throw new InvalidDataException();
        var json = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
        var manifest = json.Deserialize<Manifest>(File.ReadAllText(WindowsPath(manifestPath)));
        if (manifest == null || manifest.Schema != 1 || manifest.Channel != "Pengembangan" || manifest.Files == null || manifest.Files.Length < 3 || manifest.Files.Length > 10000) throw new InvalidDataException();
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.Files)
        {
            if (entry == null || String.IsNullOrEmpty(entry.Path) || Path.IsPathRooted(entry.Path) || entry.Path.Contains(":") || entry.Length < 0 || entry.Length > 1024L * 1024 * 1024) throw new InvalidDataException();
            foreach (string segment in entry.Path.Split('/', '\\')) if (segment == "" || segment == "." || segment == ".." || segment.EndsWith(".") || segment.EndsWith(" ")) throw new InvalidDataException();
            if (!paths.Add(entry.Path.Replace('\\', '/'))) throw new InvalidDataException();
            string path = Path.GetFullPath(Path.Combine(root, entry.Path));
            if (!path.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException();
            RejectLinks(path);
            using (var stream = new FileStream(WindowsPath(path), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha = SHA256.Create())
                if (stream.Length != entry.Length || BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "") != entry.Sha256) throw new InvalidDataException();
        }
        foreach (string required in new[] { "app/Pertamina.Oto.Desktop.App.exe", "app/portable-profile.json", "MULAI-DI-SINI.txt", "PERTAMINA-OTO.exe" }) if (!paths.Contains(required)) throw new InvalidDataException();
    }

    private static void RejectLinks(string path)
    {
        for (string item = path; item != null; item = Path.GetDirectoryName(item))
            if ((File.Exists(WindowsPath(item)) || Directory.Exists(WindowsPath(item))) && (File.GetAttributes(WindowsPath(item)) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException();
    }
    private static string WindowsPath(string path)
    {
        string full = Path.GetFullPath(path);
        return full.StartsWith(@"\\?\", StringComparison.Ordinal) ? full : full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full.Substring(2) : @"\\?\" + full;
    }
    public sealed class Manifest { public int Schema { get; set; } public string Channel { get; set; } public Entry[] Files { get; set; } }
    public sealed class Entry { public string Path { get; set; } public long Length { get; set; } public string Sha256 { get; set; } }
}
