using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.Distribution;

internal static class BaselinePackageChecks
{
    internal static async Task<int> RunAsync(string[] args)
    {
        if (args.Length != 4) throw new ArgumentException("--baseline-packages <new evidence> <A zip> <B distribution directory>");
        var evidence = Path.GetFullPath(args[1]);
        var allowed = Path.GetFullPath("artifacts/distribution-refresh-app-20261011") + Path.DirectorySeparatorChar;
        if (!evidence.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) || Directory.Exists(evidence)) throw new InvalidDataException("Fresh owned evidence required.");
        Directory.CreateDirectory(evidence);
        var results = new List<string>();
        void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); results.Add("PASS " + label); }
        try
        {
            var catalog = JsonSerializer.Deserialize<DistributionRelease>(File.ReadAllText(Path.Combine(args[3], "distribution-v2.json")), JsonSerializerOptions.Web) ?? throw new InvalidDataException("catalog");
            catalog.Validate(); var release = catalog.ToReleaseInfo();
            long baselineExpanded;
            using (var archive = System.IO.Compression.ZipFile.OpenRead(args[2])) baselineExpanded = archive.Entries.Sum(entry => entry.Length);
            var required = Math.Max(2 * baselineExpanded, baselineExpanded + 2 * catalog.ExpandedBytes + release.Size) + 256L * 1024 * 1024;
            Check(new DriveInfo(Path.GetPathRoot(evidence)!).AvailableFreeSpace >= required, "real-package peak disk budget available before extraction");
            var baseline = Path.Combine(evidence, "baseline"); OnlineUpdatePackage.Extract(args[2], baseline, CancellationToken.None);
            var first = LocalAppUpdate.InspectFeed(Path.Combine(baseline, "feed"));
            var root = Path.Combine(evidence, "installation"); Directory.CreateDirectory(root);
            LocalAppUpdate.InstallSelected(root, Path.Combine(baseline, "feed"), first.Release.Version, first.Release.ManifestSha256);
            Check(LocalAppUpdate.VersionAt(root) == first.Release.Version, "real private-runtime baseline A activated in isolated installation");
            File.WriteAllText(Path.Combine(evidence, "baseline-storage.json"), JsonSerializer.Serialize(new { OriginalZip = Path.GetFullPath(args[2]), Sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(args[2]))), ExpandedBytes = baselineExpanded, RequiredBytes = required, RemovedOnly = baseline, Reason = "reproducible extracted staging after verified A activation; original ZIP and isolated installation retained" }));
            Directory.Delete(baseline, recursive: true);
            var initial = File.ReadAllBytes(Path.Combine(root, "current.json"));
            var sentinel = Path.Combine(evidence, "synthetic-user-data.txt"); File.WriteAllText(sentinel, "synthetic settings/history/document sentinel");
            Check(release.IsNewerThan(first.Release.Version), "genuinely generated later B immutable build offered");
            using var http = new HttpClient(new Handler(request =>
            {
                if (request.RequestUri != new Uri(ReleaseInfo.Origin + "/downloads/" + release.FileName)) throw new InvalidDataException("origin");
                return new(HttpStatusCode.OK) { Content = new StreamContent(File.OpenRead(Path.Combine(args[3], release.FileName))), RequestMessage = request };
            }));
            async Task Reject(Func<Task> action, string label)
            { try { await action(); } catch (Exception e) when (e is InvalidDataException or IOException or OperationCanceledException) { Check(File.ReadAllBytes(Path.Combine(root, "current.json")).AsSpan().SequenceEqual(initial), label + " retains A pointer"); return; } throw new InvalidOperationException(label); }
            await Reject(() => OnlineUpdatePackage.DownloadAsync(http, release with { Sha256 = new string('0', 64) }, Path.Combine(evidence, "corrupt.zip"), null, CancellationToken.None), "real payload corruption");
            File.Delete(Path.Combine(evidence, "corrupt.zip"));
            using (var cancellation = new CancellationTokenSource())
            { cancellation.Cancel(); await Reject(() => OnlineUpdatePackage.DownloadAsync(http, release, Path.Combine(evidence, "cancel.zip"), null, cancellation.Token), "download cancellation"); }
            var zip = Path.Combine(evidence, "verified-B.zip"); await OnlineUpdatePackage.DownloadAsync(http, release, zip, null, CancellationToken.None);
            var extracted = Path.Combine(evidence, "next"); OnlineUpdatePackage.Extract(zip, extracted, CancellationToken.None);
            var feed = Path.Combine(extracted, "feed"); var candidate = LocalAppUpdate.InspectFeed(feed);
            Check(candidate.Release.Version == catalog.Version && candidate.Release.ManifestSha256.Equals(catalog.ManifestSha256, StringComparison.OrdinalIgnoreCase), "real downloaded B manifest binds schema2 offer");
            foreach (var entry in candidate.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(candidate.Package, "app/" + entry.Path), entry);
            Check(true, "all actual B payload sizes and hashes verified");
            var app = Path.Combine(candidate.Package, "app");
            BaselineUpdateAdmission.Validate(app, candidate.Manifest);
            Check(true, "five actual private-runtime contracts and launcher protocol2 admitted");
            var runtimePath = Path.Combine(app, "Pertamina.Oto.Desktop.App.runtimeconfig.json");
            var originalRuntime = File.ReadAllBytes(runtimePath);
            try
            {
                File.WriteAllText(runtimePath, "{\"runtimeOptions\":{\"framework\":{\"name\":\"Microsoft.NETCore.App\",\"version\":\"10.0.12\"}}}");
                var forged = candidate.Manifest with { Files = candidate.Manifest.Files.Select(entry => entry.Path == "Pertamina.Oto.Desktop.App.runtimeconfig.json" ? entry with { Length = new FileInfo(runtimePath).Length, Sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(runtimePath))) } : entry).ToArray() };
                await Reject(() => Task.Run(() => BaselineUpdateAdmission.Validate(app, forged)), "self-declared runtime bundled with global-runtime config");
            }
            finally { File.WriteAllBytes(runtimePath, originalRuntime); }
            await Reject(() => Task.Run(() => LocalAppUpdate.InstallSelected(root, feed, release.Version, new string('0', 64))), "wrong offered manifest");
            using (var gate = new FileStream(Path.Combine(root, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                await Reject(() => Task.Run(() => LocalAppUpdate.InstallSelected(root, feed, release.Version, catalog.ManifestSha256)), "concurrent installer");
            LocalAppUpdate.InstallSelected(root, feed, release.Version, catalog.ManifestSha256);
            Check(LocalAppUpdate.VersionAt(root) == release.Version, "new baseline A to later B activates actual private-runtime payload");
            Check(File.ReadAllBytes(Path.Combine(root, "previous.json")).AsSpan().SequenceEqual(initial), "exact previous A pointer retained");
            Check(Directory.Exists(Path.Combine(root, "versions", first.Release.Version)) && File.ReadAllText(sentinel) == "synthetic settings/history/document sentinel", "prior real application and user sentinel retained");
            var installed = Path.Combine(root, "versions", release.Version);
            foreach (var entry in candidate.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(installed, entry.Path), entry);
            Check(true, "installed B exact manifest verification");
            var start = new ProcessStartInfo(Path.Combine(installed, "Pertamina.Oto.Desktop.App.exe"), "--audit-startup-window") { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = installed };
            start.Environment["OTO_ISOLATED_NATIVE_AUDIT"] = "1";
            start.Environment["DOTNET_ROOT"] = Path.Combine(evidence, "nonexistent-runtime");
            start.Environment["DOTNET_ROOT_X64"] = Path.Combine(evidence, "nonexistent-runtime");
            start.Environment["DOTNET_HOST_TRACE"] = "1";
            start.Environment["DOTNET_HOST_TRACEFILE"] = Path.Combine(evidence, "B-private-runtime.host.txt");
            using var child = Process.Start(start) ?? throw new IOException("startup");
            await child.WaitForExitAsync();
            var lines = File.ReadAllLines(Path.Combine(installed, "startup-window-audit", "audit.txt"));
            Check(child.ExitCode == 0 && lines.Count(line => line.StartsWith("PASS ", StringComparison.Ordinal)) >= 15 && !lines.Any(line => line.StartsWith("FAIL ", StringComparison.Ordinal)), "actual installed B native startup/render 15 checks");
            Check(File.ReadAllText(Path.Combine(evidence, "B-private-runtime.host.txt")).Contains(Path.Combine(installed, "coreclr.dll"), StringComparison.OrdinalIgnoreCase), "actual B loads app-private CoreCLR with invalid system runtime roots");
            foreach (var entry in candidate.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(installed, entry.Path), entry);
            Check(true, "original installed B payloads unchanged after native startup");
            File.WriteAllLines(Path.Combine(evidence, "results.txt"), results);
            File.WriteAllText(Path.Combine(evidence, "summary.json"), JsonSerializer.Serialize(new { Status = "PASS", A = first.Release.Version, B = release.Version, Checks = results.Count, Transport = "in-memory HTTP handler streaming actual package bytes; not public HTTPS", NativePid = child.Id, DllSha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.Combine(installed, "Pertamina.Oto.Desktop.App.dll")))) }));
            return 0;
        }
        catch (Exception e) { results.Add("FAIL " + e); File.WriteAllLines(Path.Combine(evidence, "results.txt"), results); return 1; }
    }
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(response(request)); }
}
