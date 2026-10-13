using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Pertamina.Oto.Desktop.App;

[SupportedOSPlatform("windows")]
internal static class PdfAssociation
{
    internal const string AppName = "PERTAMINA OTO";
    internal const string ProgId = "PertaminaOTO.Pdf";
    internal const string HelperName = "PertaminaOTO.PdfOpener.exe";
    internal const string Capabilities = @"Software\PertaminaOTO\Capabilities";
    internal static readonly Uri SettingsUri = new("ms-settings:defaultapps?registeredAppUser=" + Uri.EscapeDataString(AppName));
    private static readonly string[] HelperFiles = [HelperName, "PertaminaOTO.PdfOpener.dll", "PertaminaOTO.PdfOpener.deps.json", "PertaminaOTO.PdfOpener.runtimeconfig.json"];
    internal sealed record Value(string Key, string Name, object Data, RegistryValueKind Kind = RegistryValueKind.String);

    internal static string Register()
    {
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "portable-profile.json")) || Directory.Exists(Path.Combine(AppContext.BaseDirectory, "portable-profile.json")))
            throw new InvalidOperationException("Pembuka PDF default tidak tersedia dalam mode portable.");
        var root = LocalAppUpdate.InstallRoot;
        // An installed version is required; development artifact paths never become the default opener.
        var version = LocalAppUpdate.VersionAt(root); LocalAppUpdate.ValidateVersion(version);
        if (!File.Exists(LocalAppUpdate.SafePath(root, "versions/" + version + "/Pertamina.Oto.Desktop.App.exe")))
            throw new InvalidOperationException("Instalasi lokal belum tersedia.");
        var helper = PrepareHelper(root, Path.Combine(AppContext.BaseDirectory, "pdf-opener"));
        WriteRegistration(Registry.CurrentUser, Registration(helper));
        SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero); // SHCNE_ASSOCCHANGED, no default takeover.
        return helper;
    }

    internal static string PrepareHelper(string root, string source)
    {
        var files = HelperFiles.Select(name =>
        {
            var path = LocalAppUpdate.SafePath(source, name);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length is <= 0 or > 16 * 1024 * 1024) throw new InvalidDataException("Peluncur PDF tidak valid.");
            return new LocalAppUpdate.Entry(name, stream.Length, Convert.ToHexString(SHA256.HashData(stream)));
        }).ToArray();
        var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', files.Select(f => f.Sha256))))).ToLowerInvariant();
        var destination = LocalAppUpdate.SafePath(root, "pdf-open/" + id);
        Directory.CreateDirectory(destination);
        using var gate = new FileStream(LocalAppUpdate.SafePath(root, "pdf-association.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        foreach (var file in files)
        {
            var path = LocalAppUpdate.SafePath(destination, file.Path);
            if (!File.Exists(path))
            {
                var pending = LocalAppUpdate.SafePath(destination, file.Path + ".pending-" + Guid.NewGuid().ToString("N"));
                File.Copy(LocalAppUpdate.SafePath(source, file.Path), pending, false); LocalAppUpdate.Verify(pending, file);
                File.Move(pending, path, false);
            }
            LocalAppUpdate.Verify(path, file);
        }
        return LocalAppUpdate.SafePath(destination, HelperName);
    }

    internal static Value[] Registration(string helper)
    {
        if (!Path.IsPathFullyQualified(helper) || helper.Contains('"') || helper.StartsWith(@"\\", StringComparison.Ordinal)) throw new ArgumentException("Lokasi peluncur tidak valid.");
        var command = $"\"{helper}\" \"%1\"";
        var icon = $"\"{helper}\",0";
        var prog = @"Software\Classes\" + ProgId;
        var application = @"Software\Classes\Applications\" + HelperName;
        return
        [
            new(prog, "", "Dokumen PDF PERTAMINA OTO"), new(prog, "FriendlyTypeName", "Dokumen PDF PERTAMINA OTO"),
            new(prog + @"\DefaultIcon", "", icon), new(prog + @"\shell\open\command", "", command),
            new(prog + @"\Application", "ApplicationName", AppName), new(prog + @"\Application", "ApplicationIcon", icon),
            new(application, "FriendlyAppName", AppName), new(application + @"\DefaultIcon", "", icon),
            new(application + @"\shell\open\command", "", command), new(application + @"\SupportedTypes", ".pdf", ""),
            new(@"Software\Classes\.pdf\OpenWithProgids", ProgId, Array.Empty<byte>(), RegistryValueKind.None),
            new(Capabilities, "ApplicationName", AppName), new(Capabilities, "ApplicationDescription", "Buka dan baca PDF lokal dengan PERTAMINA OTO."),
            new(Capabilities, "ApplicationIcon", icon), new(Capabilities + @"\FileAssociations", ".pdf", ProgId),
            new(@"Software\RegisteredApplications", AppName, Capabilities)
        ];
    }

    // A supplied base key lets integration tests use an isolated, inert HKCU subtree.
    internal static void WriteRegistration(RegistryKey baseKey, IEnumerable<Value> values)
    {
        foreach (var value in values)
        {
            using var key = baseKey.CreateSubKey(value.Key, writable: true) ?? throw new IOException("Pendaftaran Windows gagal.");
            key.SetValue(value.Name, value.Data, value.Kind);
        }
    }

    internal enum DefaultStatus { Oto, Other, Unknown }
    internal static DefaultStatus QueryStatus()
    {
        try
        {
            var buffer = new char[512]; uint length = (uint)buffer.Length;
            if (AssocQueryString(0, 20, ".pdf", null, buffer, ref length) != 0) return DefaultStatus.Unknown;
            return ClassifyDefault(new string(buffer).TrimEnd('\0'));
        }
        catch (Exception) { return DefaultStatus.Unknown; }
    }
    internal static DefaultStatus ClassifyDefault(string? selected) => string.IsNullOrWhiteSpace(selected) ? DefaultStatus.Unknown
        : string.Equals(selected, ProgId, StringComparison.OrdinalIgnoreCase) || string.Equals(selected, @"Applications\" + HelperName, StringComparison.OrdinalIgnoreCase) ? DefaultStatus.Oto : DefaultStatus.Other;

    internal static string Status()
    {
        try
        {
            var status = QueryStatus();
            if (status == DefaultStatus.Unknown) return "Status pilihan Windows belum dapat dibaca. Periksa melalui tombol di samping.";
            if (status == DefaultStatus.Oto) return "PERTAMINA OTO sudah menjadi pembuka PDF default.";
            using var apps = Registry.CurrentUser.OpenSubKey(@"Software\RegisteredApplications");
            return apps?.GetValue(AppName) as string == Capabilities
                ? "OTO sudah terdaftar. Pilih OTO untuk .pdf pada pengaturan Windows."
                : "Daftarkan OTO agar muncul dalam daftar pembuka PDF Windows.";
        }
        catch (Exception) { return "Status pilihan Windows belum dapat dibaca. Periksa melalui tombol di samping."; }
    }

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);

    [DllImport("shlwapi.dll", EntryPoint = "AssocQueryStringW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int AssocQueryString(uint flags, uint str, string association, string? extra, [Out] char[] output, ref uint length);
}
