using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Downloads;

// A separate catalog binds the frozen baseline contract; legacy routes keep schema 1.
internal sealed class BaselineCatalog(IConfiguration configuration, IWebHostEnvironment environment)
{
    private readonly string root = Path.GetFullPath(configuration["Distribution:Root"] ?? Path.Combine(environment.ContentRootPath, "releases"));
    private readonly object gate = new();
    private string? verifiedStamp;
    internal bool Present => File.Exists(Path.Combine(root, "distribution-v2.json"));

    internal DistributionRelease? Current()
    {
        lock (gate)
        {
            try
            {
                var metadata = new FileInfo(Safe("distribution-v2.json"));
                if (!metadata.Exists || metadata.Length is <= 0 or > 98304) return null;
                var candidate = JsonSerializer.Deserialize<DistributionRelease>(File.ReadAllText(metadata.FullName), ReleaseCatalog.JsonOptions);
                if (candidate is null) return null;
                candidate.Validate();
                var artifacts = new[] { candidate.OnlineInstaller, candidate.OfflineInstaller, candidate.UpdatePayload };
                var files = artifacts.Select(a => new FileInfo(Safe(a.FileName))).ToArray();
                if (files.Where((file, index) => !file.Exists || file.Length != artifacts[index].Size).Any()) return null;
                var currentStamp = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(metadata.FullName))) + string.Join(";", files.Select(file => $"{file.LastWriteTimeUtc.Ticks}:{file.Length}"));
                if (currentStamp != verifiedStamp)
                {
                    for (var i = 0; i < files.Length; i++)
                    {
                        using var stream = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        if (!Convert.ToHexString(SHA256.HashData(stream)).Equals(artifacts[i].Sha256, StringComparison.OrdinalIgnoreCase)) return null;
                    }
                    verifiedStamp = currentStamp;
                }
                return candidate;
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
            var artifact = new[] { release.OnlineInstaller, release.OfflineInstaller, release.UpdatePayload }.FirstOrDefault(a => a.FileName == fileName);
            if (artifact is null) return null;
            FileStream? stream = null;
            try
            {
                stream = new FileStream(Safe(fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Length == artifact.Size && Convert.ToHexString(SHA256.HashData(stream)).Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase))
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
}
