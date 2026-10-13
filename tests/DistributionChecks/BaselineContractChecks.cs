using System.Net;
using System.Text;
using System.Text.Json;
using Pertamina.Oto.Distribution;

internal static class BaselineContractChecks
{
    internal static void VerifyCandidate(string directory)
    {
        var release = JsonSerializer.Deserialize<DistributionRelease>(File.ReadAllText(Path.Combine(directory, "distribution-v2.json")), JsonSerializerOptions.Web)
            ?? throw new InvalidDataException("catalog");
        release.Validate();
        foreach (var artifact in new[] { release.OnlineInstaller, release.OfflineInstaller, release.UpdatePayload })
        {
            using var stream = File.OpenRead(Path.Combine(directory, artifact.FileName));
            if (stream.Length != artifact.Size || !Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Artifact bytes mismatch.");
        }
        Console.WriteLine("PASS actual schema2 catalog and all three artifact lengths/SHA256");
    }
    internal static async Task RunAsync(string output)
    {
        const string version = "20261011-104936-27c39aeb";
        var release = new DistributionRelease(2, "PERTAMINA OTO", "0.2.4", version,
            new DateTimeOffset(2026, 10, 11, 3, 49, 36, TimeSpan.Zero), "development", "x64", true, 2, 1,
            new string('a', 64), 1500000000, ["Fixture kandidat sintetis; belum diterbitkan."],
            new($"PERTAMINA-OTO-OnlineSetup-x64-{version}.exe", 150000, new string('b', 64)),
            new($"PERTAMINA-OTO-Setup-x64-{version}.exe", 300000000, new string('c', 64)),
            new($"PERTAMINA-OTO-Windows-x64-{version}.zip", 299000000, new string('d', 64)));
        var results = new List<string>();
        void Pass(string label) => results.Add("PASS " + label);
        void Reject(DistributionRelease candidate, string label)
        {
            try { candidate.Validate(); } catch (InvalidDataException) { Pass(label); return; }
            throw new InvalidOperationException(label);
        }
        release.Validate(); Pass("schema2 coherent artifacts");
        Reject(release with { Schema = 1 }, "schema1 not accepted as baseline");
        Reject(release with { UpdaterProtocol = 3 }, "wrong protocol");
        Reject(release with { MinimumUpdaterBaseline = 2 }, "future baseline");
        Reject(release with { Architecture = "arm64" }, "wrong architecture");
        Reject(release with { RuntimeBundled = false }, "global runtime dependency rejected");
        Reject(release with { ManifestSha256 = "bad" }, "manifest hash required");
        Reject(release with { Version = "20260230-104936-27c39aeb" }, "invalid immutable identity");
        Reject(release with { OnlineInstaller = release.OnlineInstaller with { FileName = "../setup.exe" } }, "artifact path rejected");
        Reject(release with { UpdatePayload = release.UpdatePayload with { Size = 0 } }, "zero size rejected");
        Reject(release with { OnlineInstaller = release.OnlineInstaller with { Size = release.UpdatePayload.Size } }, "full payload disguised as thin installer rejected");
        Reject(release with { ReleaseNotesJson = "{}", ReleaseNotesSha256 = new string('a', 64) }, "notes hash mismatch");
        using var client = new HttpClient(new Handler(request =>
        {
            if (request.RequestUri != DistributionRelease.LatestUri) throw new InvalidOperationException("wrong fixed endpoint");
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(release, JsonSerializerOptions.Web), Encoding.UTF8, "application/json") };
        }));
        var fetched = await OnlineReleaseClient.FetchBaselineAsync(client, CancellationToken.None);
        if (fetched?.ManifestSha256 != release.ManifestSha256) throw new InvalidOperationException("binding lost");
        Pass("new fixed HTTPS endpoint roundtrip retains manifest binding");
        using var unavailable = new HttpClient(new Handler(_ => new(HttpStatusCode.NoContent)));
        if (await OnlineReleaseClient.FetchBaselineAsync(unavailable, CancellationToken.None) is not null) throw new InvalidOperationException("204");
        Pass("unavailable feed remains unknown");
        using var redirected = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { RequestMessage = new(HttpMethod.Get, "https://example.invalid/latest") }));
        try { await OnlineReleaseClient.FetchBaselineAsync(redirected, CancellationToken.None); throw new InvalidOperationException("redirect accepted"); }
        catch (InvalidDataException) { Pass("cross-origin redirect rejected"); }
        File.WriteAllLines(Path.Combine(output, "baseline-contract-results.txt"), results);
        File.WriteAllText(Path.Combine(output, "baseline-fixture.json"), JsonSerializer.Serialize(release, JsonSerializerOptions.Web));
    }
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(response(request));
    }
}
