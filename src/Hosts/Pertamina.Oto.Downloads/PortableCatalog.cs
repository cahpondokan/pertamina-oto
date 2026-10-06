using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pertamina.Oto.Downloads;

internal sealed record PortableRelease(int Schema, string Version, string ProductVersion, string Channel,
    string FileName, long Size, string Sha256, DateTimeOffset PublishedAt, string Runtime, string EntryPoint)
{
    internal void Validate()
    {
        if (Schema != 1 || Channel != "development" || ProductVersion != "0.1.0" ||
            Version is null || !Regex.IsMatch(Version, @"\A[0-9]{8}-[0-9]{6}-[a-f0-9]{8}\z") ||
            FileName != $"PERTAMINA-OTO-Portable-Windows-x64-{Version}.zip" ||
            Size is <= 0 or > 2147483648 || Sha256 is null || !Regex.IsMatch(Sha256, @"\A[A-Fa-f0-9]{64}\z") ||
            Runtime is not ("self-contained" or "dotnet-10-x64-required") ||
            EntryPoint != "PERTAMINA-OTO.exe" || PublishedAt == default)
            throw new InvalidDataException("Invalid portable release metadata.");
    }
    public string DownloadPath => "/portable-downloads/" + FileName;
}

// Independent catalog: portable bytes must never replace the installer update feed.
internal sealed class PortableCatalog(IConfiguration configuration, IWebHostEnvironment environment)
{
    private readonly string root = Path.GetFullPath(configuration["Distribution:PortableRoot"] ??
        Path.Combine(environment.ContentRootPath, "portable-releases"));
    private readonly object gate = new();
    private PortableRelease? cached;
    private (DateTime Metadata, long MetadataSize, DateTime Package, long PackageSize) stamp;

    internal PortableRelease? Current()
    {
        lock (gate)
        {
        try
        {
            var metadata = new FileInfo(Safe("current.json"));
            if (!metadata.Exists || metadata.Length is <= 0 or > 16384) return null;
            var value = JsonSerializer.Deserialize<PortableRelease>(File.ReadAllText(metadata.FullName), ReleaseCatalog.JsonOptions);
            if (value is null) return null;
            value.Validate();
            var package = new FileInfo(Safe(value.FileName));
            if (!package.Exists || package.Length != value.Size) return null;
            var currentStamp = (metadata.LastWriteTimeUtc, metadata.Length, package.LastWriteTimeUtc, package.Length);
            if (cached == value && stamp == currentStamp) return value;
            using var stream = new FileStream(Safe(value.FileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!Matches(stream, value)) return null;
            cached = value; stamp = currentStamp; return value;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or ArgumentException)
        { return null; }
        }
    }

    internal FileStream? Open(string fileName)
    {
        var value = Current();
        if (value is null || value.FileName != fileName) return null;
        FileStream? stream = null;
        try
        {
            stream = new FileStream(Safe(fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (Matches(stream, value)) { stream.Position = 0; return stream; }
            stream.Dispose(); return null;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException)
        { stream?.Dispose(); return null; }
    }

    private static bool Matches(FileStream stream, PortableRelease value) => stream.Length == value.Size &&
        Convert.ToHexString(SHA256.HashData(stream)).Equals(value.Sha256, StringComparison.OrdinalIgnoreCase);

    private string Safe(string name)
    {
        if (name != Path.GetFileName(name) || name.Contains(':') || name.Contains('\\') || name is "." or "..") throw new InvalidDataException();
        var path = Path.Combine(root, name);
        for (var part = path; part is not null; part = Path.GetDirectoryName(part))
            if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException();
        return path;
    }
}
