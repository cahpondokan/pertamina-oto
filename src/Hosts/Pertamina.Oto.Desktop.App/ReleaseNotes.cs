using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pertamina.Oto.Desktop.App;

// Plain text only. No URLs, markup parser, remote execution or latest-feed lookup.
internal static class ReleaseNotes
{
    internal const int MaxBytes = 65536;
    private static readonly JsonSerializerOptions Options = new() { MaxDepth = 8, UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };
    internal sealed record Entry(string ProductVersion, string Date, string[] Summary,
        string[] Features, string[] Improvements, string[] Fixes, string[] Limitations);
    internal sealed record Catalog(int Schema, string ProductVersion, Entry[] Releases);
    internal sealed record Viewed(int Schema, string[] Versions);
    internal const string Missing = "Catatan perubahan untuk paket ini belum tersedia. Versi dan pemeriksaan integritas tetap berlaku.";
    internal static Catalog? Parse(string? json, string? product)
    {
        try
        {
            if (!ApplicationVersion.ValidProduct(product) || json is null || Encoding.UTF8.GetByteCount(json) > MaxBytes) return null;
            var value = JsonSerializer.Deserialize<Catalog>(json, Options);
            if (value is null || value.Schema != 1 || value.ProductVersion != product || value.Releases is not { Length: > 0 and <= 24 }
                || value.Releases[0].ProductVersion != product || value.Releases.Select(r => r.ProductVersion).Distinct().Count() != value.Releases.Length) return null;
            foreach (var entry in value.Releases)
            {
                if (!ApplicationVersion.ValidProduct(entry.ProductVersion) || Compare(entry.ProductVersion, product!) > 0
                    || !DateOnly.TryParseExact(entry.Date, "yyyy-MM-dd", out _)
                    || !Texts(entry.Summary, 5, 240) || entry.Summary.Length < 3
                    || !Texts(entry.Features, 12, 500) || !Texts(entry.Improvements, 12, 500)
                    || !Texts(entry.Fixes, 12, 500) || !Texts(entry.Limitations, 12, 500)) return null;
            }
            return value;
        }
        catch (Exception error) when (error is JsonException or ArgumentException or NullReferenceException or OverflowException) { return null; }
    }
    private static bool Texts(string[]? values, int count, int length) => values is not null && values.Length <= count
        && values.All(v => !string.IsNullOrWhiteSpace(v) && v.Length <= length && !v.Any(char.IsControl));
    internal static int Compare(string a, string b)
    {
        var left = a.Split('.').Select(int.Parse).ToArray(); var right = b.Split('.').Select(int.Parse).ToArray();
        for (var i = 0; i < 3; i++) { var comparison = left[i].CompareTo(right[i]); if (comparison != 0) return comparison; }
        return 0;
    }
    internal static Catalog? Read(string directory, string? product)
    {
        try
        {
            var path = LocalAppUpdate.SafePath(directory, "Assets/release-notes.json");
            if (!File.Exists(path) || new FileInfo(path).Length > MaxBytes) return null;
            return Parse(File.ReadAllText(path), product);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException) { return null; }
    }
    internal static Catalog? Available(LocalAppUpdate.Manifest manifest, string package, ApplicationVersion.Identity identity)
    {
        var entry = manifest.Files.SingleOrDefault(f => f.Path == "Assets/release-notes.json");
        if (entry is null || entry.Length > MaxBytes) return null;
        LocalAppUpdate.Verify(LocalAppUpdate.SafePath(package, "app/Assets/release-notes.json"), entry);
        return Read(Path.Combine(package, "app"), identity.ProductVersion);
    }
    internal static Catalog? Online(Pertamina.Oto.Distribution.ReleaseInfo release)
    {
        if (release.ReleaseNotesJson is null || release.ReleaseNotesSha256 is null) return null;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(release.ReleaseNotesJson)));
        return hash.Equals(release.ReleaseNotesSha256, StringComparison.OrdinalIgnoreCase) ? Parse(release.ReleaseNotesJson, release.ProductVersion) : null;
    }
    internal static Viewed ReadViewed(string path)
    {
        try
        {
            var safe = LocalAppUpdate.SafePath(Path.GetDirectoryName(path)!, Path.GetFileName(path));
            if (!File.Exists(safe) || new FileInfo(safe).Length > 8192) return new(1, []);
            var value = JsonSerializer.Deserialize<Viewed>(File.ReadAllText(safe));
            return value is { Schema: 1, Versions.Length: <= 128 } && value.Versions.All(ApplicationVersion.ValidProduct) ? value : new(1, []);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidDataException) { return new(1, []); }
    }
    internal static void MarkViewed(string path, string product)
    {
        if (!ApplicationVersion.ValidProduct(product)) throw new InvalidDataException("Versi catatan tidak valid.");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var safe = LocalAppUpdate.SafePath(Path.GetDirectoryName(path)!, Path.GetFileName(path));
        var versions = ReadViewed(safe).Versions.Append(product).Distinct().TakeLast(128).ToArray();
        var temporary = safe + "." + Guid.NewGuid().ToString("N") + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new Viewed(1, versions)));
        File.Move(temporary, safe, true);
    }
    // current must be THIS running binary; a prepared pointer is not successful startup.
    internal static bool ShouldShow(ApplicationVersion.Identity running, string? currentBuild,
        ApplicationVersion.Identity previous, Viewed viewed, Catalog? notes)
    {
        if (!running.Known || !ApplicationVersion.ValidBuild(running.BuildId) || currentBuild != running.BuildId || notes is null
            || viewed.Versions.Any(v => Compare(v, running.ProductVersion!) >= 0)) return false;
        if (viewed.Versions.Length > 0) return true;
        return previous.Known && ApplicationVersion.ValidBuild(previous.BuildId) && previous.BuildId != running.BuildId
            && Compare(running.ProductVersion!, previous.ProductVersion!) > 0;
    }
}
