using System.IO.Compression;
using System.Security.Cryptography;

namespace Pertamina.Oto.Distribution;

// Caller uses a no-redirect HTTP handler. No cookies, document data or identity is sent.
internal static class OnlineUpdatePackage
{
    internal const long MaxExpandedBytes = 3L * 1024 * 1024 * 1024;
    internal static async Task DownloadAsync(HttpClient client, ReleaseInfo release, string destination,
        IProgress<string>? progress, CancellationToken cancellationToken)
    {
        release.Validate();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromMinutes(20));
        var uri = new Uri(ReleaseInfo.Origin + "/downloads/" + release.FileName);
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        if (response.RequestMessage?.RequestUri is { } final && final != uri)
            throw new InvalidDataException("Redirect update tidak diizinkan.");
        if (response.Content.Headers.ContentLength is { } length && length != release.Size)
            throw new InvalidDataException("Ukuran unduhan tidak cocok.");
        await using var input = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[65536]; long total = 0; var lastPercent = -1;
        while (true)
        {
            var count = await input.ReadAsync(buffer, deadline.Token).ConfigureAwait(false);
            if (count == 0) break;
            total += count;
            if (total > release.Size) throw new InvalidDataException("Unduhan melebihi ukuran rilis.");
            hash.AppendData(buffer, 0, count);
            await output.WriteAsync(buffer.AsMemory(0, count), deadline.Token).ConfigureAwait(false);
            var percent = (int)(total * 100 / release.Size);
            if (percent != lastPercent) { progress?.Report($"Mengunduh update… {percent}%"); lastPercent = percent; }
        }
        if (total != release.Size || !Convert.ToHexString(hash.GetHashAndReset()).Equals(release.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Hash atau ukuran unduhan tidak cocok.");
    }

    internal static void Extract(string zip, string destination, CancellationToken cancellationToken)
    {
        using var archive = ZipFile.OpenRead(zip);
        if (archive.Entries.Count is 0 or > 12000) throw new InvalidDataException("Jumlah isi ZIP tidak valid.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = entry.FullName;
            var directory = name.EndsWith('/');
            var canonical = directory ? name[..^1] : name;
            if (canonical.Length > 220 || canonical.Contains('\\') || canonical.Contains(':') || canonical.StartsWith('/')
                || canonical.Split('/').Any(p => p.Length == 0 || p is "." or ".." || p.EndsWith('.') || p.EndsWith(' ')
                    || p.Any(c => char.IsControl(c) || "<>\"|?*".Contains(c)) || Reserved(p))
                || !names.Add(canonical) || (entry.ExternalAttributes >> 16 & 0xF000) == 0xA000
                || (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Lokasi ZIP tidak aman atau ambigu.");
            if (!directory) files.Add(canonical);
            total = checked(total + entry.Length);
            if (entry.Length > 1024L * 1024 * 1024 || total > MaxExpandedBytes
                || (entry.Length > 1024 * 1024 && entry.Length / Math.Max(1, entry.CompressedLength) > 1000))
                throw new InvalidDataException("Isi ZIP terlalu besar.");
        }
        foreach (var name in names)
            for (var parent = Path.GetDirectoryName(name)?.Replace('\\', '/'); !string.IsNullOrEmpty(parent); parent = Path.GetDirectoryName(parent)?.Replace('\\', '/'))
                if (files.Contains(parent)) throw new InvalidDataException("Lokasi ZIP bertabrakan.");
        Directory.CreateDirectory(destination);
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.GetFullPath(Path.Combine(destination, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
            if (!path.StartsWith(Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Lokasi ZIP di luar staging.");
            for (var part = path; part is not null; part = Path.GetDirectoryName(part))
                if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Link staging tidak diizinkan.");
            if (entry.FullName.EndsWith('/')) { Directory.CreateDirectory(path); continue; }
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var input = entry.Open(); using var output = new FileStream(path, FileMode.CreateNew);
            var buffer = new byte[65536]; long copied = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested(); var count = input.Read(buffer);
                if (count == 0) break; copied += count;
                if (copied > entry.Length) throw new InvalidDataException("Isi ZIP melebihi ukuran.");
                output.Write(buffer, 0, count);
            }
            if (copied != entry.Length) throw new InvalidDataException("Isi ZIP tidak lengkap.");
        }
    }
    private static bool Reserved(string segment)
    {
        var stem = segment.Split('.')[0];
        return stem.Equals("CON", StringComparison.OrdinalIgnoreCase) || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("AUX", StringComparison.OrdinalIgnoreCase) || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase)
            || (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) && (stem[3] is >= '0' and <= '9' or '¹' or '²' or '³'));
    }
}
