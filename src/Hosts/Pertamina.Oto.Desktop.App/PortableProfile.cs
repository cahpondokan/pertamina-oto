using System.Text.Json;

namespace Pertamina.Oto.Desktop.App;

// A package marker applies even when the host EXE is opened directly.
internal static class PortableProfile
{
    internal const string MarkerName = "portable-profile.json";
    internal static bool IsPortable => File.Exists(Path.Combine(AppContext.BaseDirectory, MarkerName)) || Directory.Exists(Path.Combine(AppContext.BaseDirectory, MarkerName));
    internal static string DataRoot => IsPortable
        ? ResolveDataRoot(AppContext.BaseDirectory)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PertaminaOTO");

    internal static string ResolveDataRoot(string appDirectory)
    {
        var app = Path.GetFullPath(appDirectory).TrimEnd(Path.DirectorySeparatorChar);
        var marker = Path.Combine(app, MarkerName);
        RejectLinks(marker);
        if (new FileInfo(marker).Length > 256) throw new InvalidDataException("Penanda portable tidak valid.");
        using var json = JsonDocument.Parse(File.ReadAllText(marker));
        if (json.RootElement.ValueKind != JsonValueKind.Object || !json.RootElement.TryGetProperty("schema", out var schema)
            || schema.ValueKind != JsonValueKind.Number || !schema.TryGetInt32(out var version) || version != 1
            || !json.RootElement.TryGetProperty("dataFolder", out var folder) || folder.ValueKind != JsonValueKind.String || folder.GetString() != "Data")
            throw new InvalidDataException("Penanda portable tidak valid.");
        var parent = Path.GetDirectoryName(app) ?? throw new InvalidDataException("Folder portable tidak valid.");
        var data = Path.Combine(parent, "Data");
        RejectLinks(data);
        return data;
    }

    internal static void ValidateStartup()
    {
        if (!IsPortable) return;
        var data = DataRoot;
        Directory.CreateDirectory(data);
        RejectLinks(data);
        var probe = Path.Combine(data, ".write-check-" + Guid.NewGuid().ToString("N"));
        using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
    }

    private static void RejectLinks(string path)
    {
        for (string? item = path; item is not null; item = Path.GetDirectoryName(item))
            if ((File.Exists(item) || Directory.Exists(item)) && (File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Folder portable tidak boleh memakai link atau junction.");
    }
}
