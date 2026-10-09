using System.Globalization;
using System.Text.RegularExpressions;

namespace Pertamina.Oto.Distribution;

internal sealed record ReleaseInfo(int Schema, string Version, DateTimeOffset PublishedAt, string Channel,
    string FileName, long Size, string Sha256, string[] Notes,
    string? InstallerFileName = null, long? InstallerSize = null, string? InstallerSha256 = null,
    string? ProductVersion = null, string? ReleaseNotesJson = null, string? ReleaseNotesSha256 = null)
{
    internal const string Origin = "https://oto.ayani.id";
    internal static readonly Uri LatestUri = new(Origin + "/api/releases/windows-x64/latest");
    internal static readonly Uri DownloadPage = new(Origin + "/#unduh");
    internal string DownloadPath => "/downloads/" + (InstallerFileName ?? FileName);
    internal long DownloadSize => InstallerSize ?? Size;
    internal string DownloadSha256 => InstallerSha256 ?? Sha256;

    internal static bool ValidVersion(string? value) => value is not null
        && Regex.IsMatch(value, "^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$", RegexOptions.CultureInvariant)
        && DateTime.TryParseExact(value[..15], "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    internal bool IsNewerThan(string installed) => ValidVersion(installed)
        && string.CompareOrdinal(Version[..15], installed[..15]) > 0;

    internal void Validate()
    {
        if (Schema != 1 || !ValidVersion(Version) || Channel != "development"
            || FileName != $"PERTAMINA-OTO-Windows-x64-{Version}.zip"
            || Size is <= 0 or > 2L * 1024 * 1024 * 1024
            || Sha256 is null || !Regex.IsMatch(Sha256, "^[A-Fa-f0-9]{64}$", RegexOptions.CultureInvariant)
            || PublishedAt < new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) || PublishedAt > DateTimeOffset.UtcNow.AddDays(1)
            || Notes is not { Length: > 0 and <= 8 } || Notes.Any(n => string.IsNullOrWhiteSpace(n) || n.Length > 240 || n.Any(char.IsControl)))
            throw new InvalidDataException("Metadata rilis tidak valid.");
        if (ReleaseNotesJson is not null && System.Text.Encoding.UTF8.GetByteCount(ReleaseNotesJson) > 65536)
            throw new InvalidDataException("Catatan rilis terlalu besar.");
        if (InstallerFileName is not null || InstallerSize is not null || InstallerSha256 is not null)
        {
            if (InstallerFileName != $"PERTAMINA-OTO-Setup-x64-{Version}.exe"
                || InstallerSize is null or <= 0 or > 2L * 1024 * 1024 * 1024
                || InstallerSha256 is null || !Regex.IsMatch(InstallerSha256, "^[A-Fa-f0-9]{64}$", RegexOptions.CultureInvariant))
                throw new InvalidDataException("Metadata installer tidak valid.");
        }
    }
}
