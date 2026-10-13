using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pertamina.Oto.Desktop.App;

// Local development feed only: hashes detect damaged packages, not publisher identity.
// The configured feed must remain a trusted directory controlled by the developer/user.
internal static class LocalAppUpdate
{
    internal sealed record Source(string Folder);
    internal sealed record Release(string Version, string ManifestSha256, string? ProductVersion = null, string? Channel = null);
    internal sealed record Entry(string Path, long Length, string Sha256);
    internal sealed record Manifest(string Version, Entry[] Files, string? ProductVersion = null, string? Channel = null);
    internal static string InstallRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PertaminaOTO");
    internal static string VersionAt(string root) => Read<Release>(Path.Combine(root, "current.json")).Version;
    internal static T Read<T>(string path)
    {
        if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new InvalidDataException("Metadata update terlalu besar.");
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? throw new InvalidDataException("Metadata update tidak valid.");
    }
    internal static void ValidateVersion(string version)
    {
        if (!Regex.IsMatch(version, "^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$", RegexOptions.CultureInvariant)) throw new InvalidDataException("ID versi tidak valid.");
    }
    internal static string SafePath(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(':') || relative.Split('/', '\\').Any(s => s is "" or "." or ".." || s.EndsWith('.') || s.EndsWith(' ')))
            throw new InvalidDataException("Lokasi paket tidak valid.");
        var full = Path.GetFullPath(Path.Combine(root, relative));
        if (!full.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Lokasi paket di luar folder.");
        for (var item = full; item is not null; item = Path.GetDirectoryName(item))
            if ((File.Exists(item) || Directory.Exists(item)) && (File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Link folder tidak diizinkan untuk update.");
        return full;
    }
    internal static void Verify(string path, Entry entry, CancellationToken cancellationToken = default)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[65536];
        while (true) { cancellationToken.ThrowIfCancellationRequested(); var count = file.Read(buffer); if (count == 0) break; hash.AppendData(buffer, 0, count); }
        if (file.Length != entry.Length || !Convert.ToHexString(hash.GetHashAndReset()).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Paket update rusak atau berubah. Versi aktif tetap aman.");
    }
    internal static (Release Release, Manifest Manifest, string Package) Inspect(string root)
    {
        var source = Read<Source>(Path.Combine(root, "update-source.json")).Folder;
        return InspectFeed(source);
    }
    internal static (Release Release, Manifest Manifest, string Package) InspectFeed(string source)
    {
        if (!Path.IsPathFullyQualified(source) || source.StartsWith(@"\\", StringComparison.Ordinal)) throw new InvalidDataException("Update ini memerlukan folder lokal.");
        var release = Read<Release>(SafePath(source, "latest.json")); ValidateVersion(release.Version);
        var package = SafePath(source, release.Version);
        var manifestPath = SafePath(package, "manifest.json");
        using var locked = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (locked.Length > 4 * 1024 * 1024 || !Convert.ToHexString(SHA256.HashData(locked)).Equals(release.ManifestSha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Manifest update tidak valid.");
        locked.Position = 0;
        var manifest = JsonSerializer.Deserialize<Manifest>(locked) ?? throw new InvalidDataException("Manifest kosong.");
        if (manifest.Version != release.Version || manifest.Files is not { Length: > 0 and < 10000 } || manifest.Files.Select(f => f.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != manifest.Files.Length || manifest.Files.Any(f => f.Length < 0 || f.Length > 1024L * 1024 * 1024) || manifest.Files.Sum(f => f.Length) > 2L * 1024 * 1024 * 1024)
            throw new InvalidDataException("Isi paket update tidak valid.");
        if (!manifest.Files.Any(f => f.Path == "Pertamina.Oto.Desktop.App.exe") || !manifest.Files.Any(f => f.Path == "worker/Pertamina.Oto.LocalPdf.Worker.exe")) throw new InvalidDataException("Paket aplikasi tidak lengkap.");
        foreach (var file in manifest.Files) SafePath(package, "app/" + file.Path);
        return (release, manifest, package);
    }
    internal static string Install(string root) => InstallFrom(root, null);
    // Explicit extracted feed is used only by the user-run download installer.
    // It never replaces the existing local update-source preference.
    internal static string InstallFrom(string root, string? feed, CancellationToken cancellationToken = default) => InstallSelected(root, feed, null, null, cancellationToken);
    internal static string InstallSelected(string root, string? feed, string? expectedVersion, string? expectedManifestSha256, CancellationToken cancellationToken = default)
    {
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "portable-profile.json")) || Directory.Exists(Path.Combine(AppContext.BaseDirectory, "portable-profile.json")))
            throw new InvalidOperationException("Mode portable tidak memasang update ke instalasi lokal. Ekstrak ZIP baru ke folder terpisah.");
        Directory.CreateDirectory(root);
        using var maintenance = new FileStream(SafePath(root, "launcher-maintenance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        using var gate = new FileStream(SafePath(root, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var (release, manifest, package) = feed is null ? Inspect(root) : InspectFeed(feed);
        if (expectedVersion is not null && release.Version != expectedVersion || expectedManifestSha256 is not null && !release.ManifestSha256.Equals(expectedManifestSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Paket yang dipilih berubah. Periksa update kembali sebelum memasang.");
        var currentPath = Path.Combine(root, "current.json");
        var sameVersion = File.Exists(currentPath) && VersionAt(root) == release.Version;
        if (!sameVersion && feed is not null && File.Exists(currentPath))
        {
            var active = VersionAt(root); ValidateVersion(active);
            if (string.CompareOrdinal(release.Version[..15], active[..15]) <= 0)
                throw new InvalidDataException("Versi terpasang lebih baru atau setara. Paket ini tidak dipasang.");
        }
        var versions = SafePath(root, "versions"); Directory.CreateDirectory(versions);
        var target = SafePath(versions, release.Version);
        if (!Directory.Exists(target))
        {
            var staging = SafePath(versions, "staging-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(staging);
            foreach (var entry in manifest.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var source = SafePath(package, "app/" + entry.Path); var destination = SafePath(staging, entry.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var output = new FileStream(destination, FileMode.CreateNew))
                {
                    var buffer = new byte[65536]; long copied = 0;
                    while (true) { cancellationToken.ThrowIfCancellationRequested(); var count = input.Read(buffer); if (count == 0) break; copied += count; if (copied > entry.Length) throw new InvalidDataException("Paket melebihi ukuran manifest."); output.Write(buffer, 0, count); }
                }
                Verify(destination, entry, cancellationToken);
            }
            Directory.Move(staging, target);
        }
        foreach (var entry in manifest.Files) { cancellationToken.ThrowIfCancellationRequested(); Verify(SafePath(target, entry.Path), entry, cancellationToken); }
        var launcherSource = LauncherUpdate.BundledSource(target, manifest);
        using var launcherUpdate = launcherSource is null ? null : LauncherUpdate.Begin(root, launcherSource, cancellation: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (sameVersion) { launcherUpdate?.Commit(); return release.Version; }
        var pending = SafePath(root, "current-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(pending, JsonSerializer.Serialize(release));
        if (File.Exists(currentPath)) File.Replace(pending, currentPath, Path.Combine(root, "previous.json"));
        else File.Move(pending, currentPath);
        launcherUpdate?.Commit();
        return release.Version;
    }
}
