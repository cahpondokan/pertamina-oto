using System.Text.RegularExpressions;

namespace Pertamina.Oto.Distribution;

// Separate wire contract: never serve this document to schema-1 clients.
internal sealed record DistributionArtifact(string FileName, long Size, string Sha256);
internal sealed record DistributionRelease(int Schema, string Product, string ProductVersion, string Version,
    DateTimeOffset PublishedAt, string Channel, string Architecture, bool RuntimeBundled,
    int UpdaterProtocol, int MinimumUpdaterBaseline, string ManifestSha256, long ExpandedBytes,
    string[] Notes, DistributionArtifact OnlineInstaller, DistributionArtifact OfflineInstaller,
    DistributionArtifact UpdatePayload, string? ReleaseNotesJson = null, string? ReleaseNotesSha256 = null)
{
    internal const int Protocol = 2;
    internal const int Baseline = 1;
    internal static readonly Uri LatestUri = new(ReleaseInfo.Origin + "/api/releases/windows-x64/baseline-1/latest");

    internal void Validate()
    {
        if (Schema != 2 || Product != "PERTAMINA OTO" || Architecture != "x64" || !RuntimeBundled
            || UpdaterProtocol != Protocol || MinimumUpdaterBaseline != Baseline
            || ProductVersion is null || !Regex.IsMatch(ProductVersion, @"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant)
            || !Hash(ManifestSha256) || ExpandedBytes is <= 0 or > 2L * 1024 * 1024 * 1024)
            throw new InvalidDataException("Kontrak baseline distribusi tidak didukung.");
        Artifact(OnlineInstaller, $"PERTAMINA-OTO-OnlineSetup-x64-{Version}.exe");
        Artifact(OfflineInstaller, $"PERTAMINA-OTO-Setup-x64-{Version}.exe");
        Artifact(UpdatePayload, $"PERTAMINA-OTO-Windows-x64-{Version}.zip");
        if (OnlineInstaller.Size >= UpdatePayload.Size) throw new InvalidDataException("Installer online bukan paket tipis.");
        ToReleaseInfo().Validate();
        if ((ReleaseNotesJson is null) != (ReleaseNotesSha256 is null) || (ReleaseNotesSha256 is not null && !Hash(ReleaseNotesSha256)))
            throw new InvalidDataException("Ikatan catatan rilis tidak valid.");
        if (ReleaseNotesJson is not null && !Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ReleaseNotesJson))).Equals(ReleaseNotesSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Hash catatan rilis tidak cocok.");
    }

    internal ReleaseInfo ToReleaseInfo() => new(1, Version, PublishedAt, Channel, UpdatePayload.FileName,
        UpdatePayload.Size, UpdatePayload.Sha256, Notes, OfflineInstaller.FileName, OfflineInstaller.Size,
        OfflineInstaller.Sha256, ProductVersion, ReleaseNotesJson, ReleaseNotesSha256, true, false);

    internal static bool Hash(string? value) => value is not null && Regex.IsMatch(value, "^[A-Fa-f0-9]{64}$", RegexOptions.CultureInvariant);
    private static void Artifact(DistributionArtifact? artifact, string expected)
    {
        if (artifact is null || artifact.FileName != expected || artifact.Size is <= 0 or > 2L * 1024 * 1024 * 1024 || !Hash(artifact.Sha256))
            throw new InvalidDataException("Artefak distribusi tidak valid.");
    }
}
