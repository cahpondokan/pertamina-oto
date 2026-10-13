using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.Distribution;

if (args.Length == 2 && args[0] == "--verify-v2") { BaselineContractChecks.VerifyCandidate(args[1]); return 0; }
if (args.Length == 2 && args[0] == "--validate-v2") { var candidate = JsonSerializer.Deserialize<DistributionRelease>(File.ReadAllText(args[1]), JsonSerializerOptions.Web) ?? throw new InvalidDataException("catalog"); candidate.Validate(); Console.WriteLine("PASS generated schema2 metadata before output commit"); return 0; }
var output = Path.GetFullPath(args.Single()); Directory.CreateDirectory(output);
var results = new List<string>();
void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); results.Add("PASS " + label); }
async Task Reject(Func<Task> action, string label)
{
    try { await action(); } catch (Exception e) when (e is IOException or InvalidDataException or HttpRequestException or JsonException or OperationCanceledException)
    { results.Add("PASS " + label); return; }
    throw new InvalidOperationException(label);
}
void Json(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value));
string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
ReleaseInfo Valid() => new(1, "20260930-180000-aaaaaaaa", DateTimeOffset.UtcNow, "development", "PERTAMINA-OTO-Windows-x64-20260930-180000-aaaaaaaa.zip", 123, new string('a', 64), ["Perbaikan OTO"]);
async Task<ReleaseInfo?> Fetch(string json, HttpStatusCode status = HttpStatusCode.OK, string media = "application/json", bool chunked = false)
{
    using var http = new HttpClient(new FakeHandler((request, _) =>
    {
        Check(request.RequestUri == ReleaseInfo.LatestUri && request.Method == HttpMethod.Get && request.RequestUri.Query.Length == 0, "request only to fixed HTTPS endpoint without user identifiers");
        HttpContent content = chunked ? new UnknownLengthContent(json) : new StringContent(json, Encoding.UTF8, media);
        content.Headers.ContentType = new(media);
        return Task.FromResult(new HttpResponseMessage(status) { Content = content });
    }));
    return await OnlineReleaseClient.FetchAsync(http, CancellationToken.None);
}
void Feed(string package, string version)
{
    var target = Path.Combine(package, "feed", version); Directory.CreateDirectory(Path.Combine(target, "app", "worker"));
    var entries = new List<LocalAppUpdate.Entry>();
    foreach (var name in new[] { "Pertamina.Oto.Desktop.App.exe", "worker/Pertamina.Oto.LocalPdf.Worker.exe" })
    { var file = Path.Combine(target, "app", name); File.WriteAllText(file, version); entries.Add(new(name, new FileInfo(file).Length, Hash(file))); }
    var manifest = Path.Combine(target, "manifest.json"); Json(manifest, new LocalAppUpdate.Manifest(version, entries.ToArray()));
    Json(Path.Combine(package, "feed", "latest.json"), new LocalAppUpdate.Release(version, Hash(manifest)));
}
try
{
    await BaselineContractChecks.RunAsync(output);
    var valid = Valid(); valid.Validate();
    Check(valid.IsNewerThan("20260930-170000-bbbbbbbb"), "newer timestamp offered");
    Check(!valid.IsNewerThan("20260930-180000-bbbbbbbb") && !valid.IsNewerThan("20261001-180000-bbbbbbbb") && !valid.IsNewerThan("Build pengembangan"), "same timestamp, downgrade and unknown build not offered");
    Check(!ReleaseInfo.ValidVersion("20260230-170000-bbbbbbbb") && !ReleaseInfo.ValidVersion("../bad"), "invalid date and traversal version rejected");
    var json = JsonSerializer.Serialize(valid, JsonSerializerOptions.Web);
    Check((await Fetch(json))?.Version == valid.Version, "valid release decoded");
    var installer = valid with { InstallerFileName = $"PERTAMINA-OTO-Setup-x64-{valid.Version}.exe", InstallerSize = 456, InstallerSha256 = new string('b', 64) };
    var installerJson = JsonSerializer.Serialize(installer, JsonSerializerOptions.Web);
    var decoded = await Fetch(installerJson);
    Check(decoded?.DownloadPath.EndsWith(".exe", StringComparison.Ordinal) == true && decoded.DownloadSize == 456 && decoded.DownloadSha256 == installer.InstallerSha256, "EXE metadata selects actual installer download");
    using (var legacy = JsonDocument.Parse(installerJson))
        Check(legacy.RootElement.GetProperty("schema").GetInt32() == 1 && legacy.RootElement.GetProperty("fileName").GetString() == valid.FileName && legacy.RootElement.GetProperty("sha256").GetString() == valid.Sha256, "legacy clients retain original valid ZIP contract");
    foreach (var invalid in new[] { installer with { InstallerFileName = "../../other.exe" }, installer with { InstallerSize = null }, installer with { InstallerSize = 0 }, installer with { InstallerSha256 = "bad" }, installer with { InstallerFileName = null } })
        await Reject(() => Fetch(JsonSerializer.Serialize(invalid, JsonSerializerOptions.Web)), "partial or unsafe EXE metadata rejected");
    Check(await Fetch("", HttpStatusCode.NoContent) is null, "no published release returns no notification");
    await Reject(() => Fetch("{"), "malformed metadata rejected");
    await Reject(() => Fetch("null"), "null metadata rejected");
    await Reject(() => Fetch(json, media: "text/html"), "HTML proxy response rejected");
    await Reject(() => Fetch(json, HttpStatusCode.ServiceUnavailable), "server failure rejected");
    await Reject(() => Fetch(json, HttpStatusCode.Redirect), "redirect response rejected");
    await Reject(() => Fetch(new string(' ', 17000) + json), "oversized declared metadata rejected");
    await Reject(() => Fetch(new string(' ', 17000) + json, chunked: true), "oversized chunked metadata rejected");
    foreach (var invalid in new[] { valid with { Schema = 2 }, valid with { FileName = "../../other.exe" }, valid with { Channel = "stable" }, valid with { Sha256 = "bad" }, valid with { Size = -1 }, valid with { Notes = [new string('x', 241)] }, valid with { Notes = ["line\nbreak"] }, valid with { PublishedAt = DateTimeOffset.UtcNow.AddDays(5) } })
        await Reject(() => Fetch(JsonSerializer.Serialize(invalid, JsonSerializerOptions.Web)), "untrusted release fields rejected");
    using (var disconnected = new HttpClient(new FakeHandler((_, _) => throw new HttpRequestException("offline"))))
        await Reject(async () => await OnlineReleaseClient.FetchAsync(disconnected, CancellationToken.None), "offline transport exits without installer side effects");
    using (var delayed = new HttpClient(new FakeHandler(async (_, token) => { await Task.Delay(Timeout.Infinite, token); return new HttpResponseMessage(); })))
    using (var cancellation = new CancellationTokenSource(50))
        await Reject(async () => await OnlineReleaseClient.FetchAsync(delayed, cancellation.Token), "window lifetime cancellation stops HTTP request");

    Check(UpdateTests.Run(Path.Combine(output, "local-updater")) == 0, "all 13 existing local updater regressions pass");
    Check(LauncherDeliveryChecks.Run(Path.Combine(output, "launcher-delivery")) == 0, "launcher migration, rollback, package and lock regressions pass");
    var package = Path.Combine(output, "package"); var install = Path.Combine(output, "installed");
    Directory.CreateDirectory(Path.Combine(package, "launcher"));
    var launchers = new List<LocalAppUpdate.Entry>();
    foreach (var name in InstallPackage.LauncherFiles)
    { var file = Path.Combine(package, "launcher", name); File.WriteAllText(file, "inert launcher"); launchers.Add(new(name, new FileInfo(file).Length, Hash(file))); }
    Json(Path.Combine(package, "launcher-manifest.json"), launchers);
    const string first = "20260930-180000-aaaaaaaa", second = "20261001-180000-bbbbbbbb";
    Feed(package, first); Check(InstallPackage.Install(package, install) == first, "download package first install activates verified files");
    Check(!File.Exists(Path.Combine(install, "update-source.json")), "download installation does not save extracted temporary feed");
    var preference = Path.Combine(install, "update-source.json"); File.WriteAllText(preference, "existing local preference");
    var launcher = Path.Combine(install, "PERTAMINA-OTO.exe"); var originalLauncher = Hash(launcher);
    Feed(package, second); Check(InstallPackage.Install(package, install) == second, "download package updates an existing installation");
    Check(File.ReadAllText(preference) == "existing local preference" && Hash(launcher) == originalLauncher, "existing local update preference and stable launcher preserved");
    Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "previous.json")).Version == first && Directory.Exists(Path.Combine(install, "versions", first)), "previous version remains recoverable");
    Feed(package, first); await Reject(() => Task.Run(() => InstallPackage.Install(package, install)), "older downloaded installer cannot downgrade");
    Check(LocalAppUpdate.VersionAt(install) == second, "downgrade refusal preserves active pointer");
    Feed(package, second); File.AppendAllText(Path.Combine(package, "feed", second, "app", "Pertamina.Oto.Desktop.App.exe"), "damaged");
    await Reject(() => Task.Run(() => InstallPackage.Install(package, install)), "damaged download payload rejected before activation");
    Check(LocalAppUpdate.VersionAt(install) == second, "corrupt downloaded package leaves active version unchanged");
    Console.WriteLine($"Distribution checks: {results.Count} PASS, plus 13 local updater checks.");
    File.WriteAllLines(Path.Combine(output, "test-results.txt"), results); return 0;
}
catch (Exception error)
{ results.Add("FAIL " + error); File.WriteAllLines(Path.Combine(output, "test-results.txt"), results); Console.Error.WriteLine(error); return 1; }

internal sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
{ protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken); }
internal sealed class UnknownLengthContent(string value) : HttpContent
{
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(Encoding.UTF8.GetBytes(value)).AsTask();
}
