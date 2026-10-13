using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Downloads;

internal sealed class ReleaseCatalog(IConfiguration configuration, IWebHostEnvironment environment)
{
    private readonly string root = Path.GetFullPath(configuration["Distribution:Root"] ?? Path.Combine(environment.ContentRootPath, "releases"));
    private readonly object gate = new();
    private ReleaseInfo? cached;
    private (DateTime Catalog, long CatalogSize, DateTime Package, long PackageSize, DateTime Installer, long InstallerSize) stamp;

    internal ReleaseInfo? Current()
    {
        lock (gate)
        {
            try
            {
                var metadata = new FileInfo(Safe("current.json"));
                if (!metadata.Exists || metadata.Length is <= 0 or > 16384) return null;
                var candidate = JsonSerializer.Deserialize<ReleaseInfo>(File.ReadAllText(metadata.FullName), JsonOptions);
                if (candidate is null) return null;
                candidate.Validate();
                var package = new FileInfo(Safe(candidate.FileName));
                if (!package.Exists || package.Length != candidate.Size) return null;
                var installer = candidate.InstallerFileName is null ? null : new FileInfo(Safe(candidate.InstallerFileName));
                if (installer is not null && (!installer.Exists || installer.Length != candidate.InstallerSize)) return null;
                var currentStamp = (metadata.LastWriteTimeUtc, metadata.Length, package.LastWriteTimeUtc, package.Length, installer?.LastWriteTimeUtc ?? default, installer?.Length ?? 0);
                if (cached is not null && stamp == currentStamp && cached.Version == candidate.Version && cached.Sha256 == candidate.Sha256 && cached.InstallerSha256 == candidate.InstallerSha256) return candidate;
                using var stream = new FileStream(package.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (!Convert.ToHexString(SHA256.HashData(stream)).Equals(candidate.Sha256, StringComparison.OrdinalIgnoreCase)) return null;
                if (installer is not null)
                {
                    using var installerStream = new FileStream(installer.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (!Convert.ToHexString(SHA256.HashData(installerStream)).Equals(candidate.InstallerSha256, StringComparison.OrdinalIgnoreCase)) return null;
                }
                cached = candidate; stamp = currentStamp; return candidate;
            }
            catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or ArgumentException)
            { return null; }
        }
    }

    internal FileStream? Open(string fileName)
    {
        lock (gate)
        {
            var release = Current();
            if (release is null) return null;
            var isInstaller = string.Equals(fileName, release.InstallerFileName, StringComparison.Ordinal);
            if (!isInstaller && !string.Equals(fileName, release.FileName, StringComparison.Ordinal)) return null;
            var size = isInstaller ? release.InstallerSize : release.Size;
            var hash = isInstaller ? release.InstallerSha256 : release.Sha256;
            FileStream? stream = null;
            try
            {
                stream = new FileStream(Safe(fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Length == size && Convert.ToHexString(SHA256.HashData(stream)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                { stream.Position = 0; return stream; }
                stream.Dispose(); return null;
            }
            catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException) { stream?.Dispose(); return null; }
        }
    }

    private string Safe(string name)
    {
        if (name != Path.GetFileName(name) || name.Contains(':') || name.Contains('\\') || name is "." or "..") throw new InvalidDataException();
        var path = Path.Combine(root, name);
        for (var part = path; part is not null; part = Path.GetDirectoryName(part))
            if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException();
        return path;
    }

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
