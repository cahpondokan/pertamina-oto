using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.Distribution;

internal static class FinalizationChecks
{
    private static readonly JsonSerializerOptions EvidenceJson = new() { WriteIndented = true };
    internal static async Task<int> RunAsync(string[] args)
    {
        var results = new List<string>();
        var evidence = Path.GetFullPath(args[1]);
        var allowed = Path.GetFullPath(Path.Combine("artifacts", "online-update-finalization")) + Path.DirectorySeparatorChar;
        if (!evidence.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Evidence must be a new folder under artifacts/online-update-finalization.");
        if (Directory.Exists(evidence) || File.Exists(evidence)) throw new IOException("Use a fresh evidence folder; prior runs are retained.");
        LocalAppUpdate.SafePath(Path.GetFullPath("artifacts/online-update-finalization"), Path.GetRelativePath(allowed, evidence));
        Directory.CreateDirectory(evidence);
        void Check(bool value, string label) { if (!value) throw new InvalidDataException(label); results.Add("PASS " + label); }
        static string Hash(string file) { using var stream = File.OpenRead(file); return Convert.ToHexString(SHA256.HashData(stream)); }
        static string Payload(string root, string version) => LocalAppUpdate.SafePath(root, "versions/" + version);
        (LocalAppUpdate.Release Release, LocalAppUpdate.Manifest Manifest, string Package) InspectZip(string zip, string destination, string expected)
        {
            OnlineUpdatePackage.Extract(zip, destination, CancellationToken.None);
            var candidate = LocalAppUpdate.InspectFeed(Path.Combine(destination, "feed"));
            Check(candidate.Release.Version == expected, "full ZIP immutable build matches expected " + expected);
            foreach (var entry in candidate.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(candidate.Package, "app/" + entry.Path), entry);
            Check(true, "full ZIP " + candidate.Manifest.Files.Length + " payload sizes/hashes verified " + expected);
            return candidate;
        }
        async Task StartNative(string root, string version)
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Native WinUI startup requires Windows");
            var app = Payload(root, version);
            using var metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(app, "build-info.json")));
            Check(metadata.RootElement.GetProperty("Version").GetString() == version, "installed startup build-info identity " + version);
            var exe = Path.Combine(app, "Pertamina.Oto.Desktop.App.exe");
            var start = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = app, WindowStyle = ProcessWindowStyle.Hidden };
            start.ArgumentList.Add("--audit-startup-window");
            using var process = Process.Start(start) ?? throw new IOException("Native startup failed");
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await process.WaitForExitAsync(deadline.Token); // No kill/forced closure on timeout.
            Check(process.ExitCode == 0, "actual installed native process exits cleanly " + version);
            var audit = File.ReadAllLines(Path.Combine(app, "startup-window-audit", "audit.txt"));
            Check(audit.Count(x => x.StartsWith("PASS ", StringComparison.Ordinal)) >= 15 && !audit.Any(x => x.StartsWith("FAIL", StringComparison.Ordinal)), "actual installed native startup/render/reader 15 checks " + version);
            File.WriteAllText(Path.Combine(evidence, "startup-" + version + ".json"), JsonSerializer.Serialize(new { Build = version, RelativeExecutable = Path.GetRelativePath(evidence, exe), Sha256 = Hash(exe), process.Id, process.ExitCode, NativePass = audit.Count(x => x.StartsWith("PASS ", StringComparison.Ordinal)) }, EvidenceJson));
        }
        try
        {
            var zipA = Path.GetFullPath(args[2]); var versionA = args[3]; LocalAppUpdate.ValidateVersion(versionA);
            var candidateA = InspectZip(zipA, Path.Combine(evidence, "package-A"), versionA);
            File.WriteAllText(Path.Combine(evidence, "package-A.json"), JsonSerializer.Serialize(new { Build = versionA, ZipSha256 = Hash(zipA), ZipSize = new FileInfo(zipA).Length, candidateA.Release.ManifestSha256, Payloads = candidateA.Manifest.Files.Length }));
            if (args[0] == "--full-package") return 0;
            if (args[0] == "--bootstrap-startup")
            {
                if (!Path.GetFileName(evidence).StartsWith("live-", StringComparison.Ordinal) || !Guid.TryParseExact(Path.GetFileName(evidence)[5..], "N", out _)) throw new InvalidDataException("Bootstrap evidence directory must be live-<GUID N format>");
                var bootstrapInstall = Path.Combine(evidence, "installation");
                Check(LocalAppUpdate.InstallFrom(bootstrapInstall, Path.Combine(evidence, "package-A", "feed")) == versionA, "actual full ZIP A bootstraps isolated installation before public activation");
                await StartNative(bootstrapInstall, versionA);
                foreach (var entry in candidateA.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(Payload(bootstrapInstall, versionA), entry.Path), entry);
                Check(true, "all " + candidateA.Manifest.Files.Length + " isolated A payloads unchanged after actual native startup");
                File.WriteAllText(Path.Combine(evidence, "scope.txt"), "Actual publisher full ZIP A, bounded extraction and complete payload verification, legitimate isolated A install and real A WinUI startup/render/reader 15 checks. No public HTTP or B activation; no user installation mutation. Fresh live integration must use a new GUID.");
                return 0;
            }
            var versionB = args[4]; LocalAppUpdate.ValidateVersion(versionB);
            var expectedZipHash = args[5];
            using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false });
            var online = await OnlineReleaseClient.FetchAsync(client, CancellationToken.None) ?? throw new InvalidDataException("No live release");
            Check(online.Version == versionB && online.Sha256.Equals(expectedZipHash, StringComparison.OrdinalIgnoreCase), "real HTTPS offered B and ZIP hash match publisher accepted candidate");
            var install = Path.Combine(evidence, "installation");
            Check(LocalAppUpdate.InstallFrom(install, Path.Combine(evidence, "package-A", "feed")) == versionA, "legitimate full ZIP A bootstrap into isolated installation");
            await StartNative(install, versionA);
            if (args[0] == "--native-live")
            {
                if (!Path.GetFileName(evidence).StartsWith("live-", StringComparison.Ordinal) || !Guid.TryParseExact(Path.GetFileName(evidence)[5..], "N", out _)) throw new InvalidDataException("Native evidence directory must be live-<GUID N format>");
                if (args.Length < 7) throw new InvalidDataException("Native mode requires publisher accepted B ZIP path");
                var acceptedBZip = Path.GetFullPath(args[6]);
                Check(Hash(acceptedBZip).Equals(expectedZipHash, StringComparison.OrdinalIgnoreCase), "local publisher accepted B ZIP hash pinned independently");
                var acceptedB = InspectZip(acceptedBZip, Path.Combine(evidence, "accepted-B"), versionB);
                File.Copy(Path.Combine(candidateA.Package, "manifest.json"), Path.Combine(install, "audit-A-manifest.json"));
                File.WriteAllText(Path.Combine(install, "update-source.json"), JsonSerializer.Serialize(new LocalAppUpdate.Source(Path.Combine(evidence, "package-A", "feed"))));
                File.WriteAllText(Path.Combine(install, "live-audit-marker.json"), JsonSerializer.Serialize(new { Schema = 1, CreatedUtc = DateTimeOffset.UtcNow, BuildA = versionA, BuildB = versionB, ZipSha256 = expectedZipHash }));
                var start = new ProcessStartInfo(Path.Combine(Payload(install, versionA), "Pertamina.Oto.Desktop.App.exe")) { UseShellExecute = false, WorkingDirectory = Payload(install, versionA), WindowStyle = ProcessWindowStyle.Hidden };
                start.Environment["OTO_RESTART_NATIVE_FIXTURE"] = "1"; // Existing pre-constructor storage isolation; no restart is requested.
                foreach (var argument in new[] { "--audit-online-update", "--live-transition", install, versionB, expectedZipHash }) start.ArgumentList.Add(argument);
                using var process = Process.Start(start) ?? throw new IOException("Native live process failed to launch");
                using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(25));
                await process.WaitForExitAsync(deadline.Token);
                Check(process.ExitCode == 0, "actual isolated native A live update process exits cleanly");
                var liveReport = File.ReadAllLines(Path.Combine(evidence, "native-live-results.txt"));
                Check(liveReport.Count(l => l.StartsWith("PASS ", StringComparison.Ordinal)) >= 20 && !liveReport.Any(l => l.StartsWith("FAIL", StringComparison.Ordinal)), "actual native HTTPS check/consent/progress/cancel/install/draft checks pass");
                Check(LocalAppUpdate.VersionAt(install) == versionB && LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "previous.json")).Version == versionA, "actual native activated B with previous A retained");
                Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "current.json")).ManifestSha256.Equals(acceptedB.Release.ManifestSha256, StringComparison.OrdinalIgnoreCase), "native installed B manifest matches publisher accepted B manifest");
                foreach (var entry in acceptedB.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(Payload(install, versionB), entry.Path), entry);
                Check(true, "all " + acceptedB.Manifest.Files.Length + " native installed B payloads independently match accepted manifest");
                File.Copy(Path.Combine(acceptedB.Package, "manifest.json"), Path.Combine(install, "audit-B-manifest.json"));
                var nativeData = Path.Combine(evidence, "isolated-data");
                var nativeSentinels = Directory.GetFiles(nativeData, "*", SearchOption.TopDirectoryOnly).ToDictionary(p => p, File.ReadAllBytes);
                var open = new ProcessStartInfo(Path.Combine(Payload(install, versionB), "Pertamina.Oto.Desktop.App.exe")) { UseShellExecute = false, WorkingDirectory = Payload(install, versionB), WindowStyle = ProcessWindowStyle.Hidden };
                open.Environment["OTO_RESTART_NATIVE_FIXTURE"] = "1";
                foreach (var argument in new[] { "--audit-online-update", "--live-open", install, versionB, acceptedB.Release.ManifestSha256 }) open.ArgumentList.Add(argument);
                using (var opened = Process.Start(open) ?? throw new IOException("Native B live-open failed"))
                {
                    using var openDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(60)); await opened.WaitForExitAsync(openDeadline.Token);
                    Check(opened.ExitCode == 0, "actual installed B opens with prior isolated settings/history");
                }
                var openReport = File.ReadAllLines(Path.Combine(evidence, "native-live-open-results.txt"));
                Check(openReport.Count(l => l.StartsWith("PASS ", StringComparison.Ordinal)) >= 10 && !openReport.Any(l => l.StartsWith("FAIL", StringComparison.Ordinal)), "actual B reads expected preference/favorite/history and renders unchanged data");
                await StartNative(install, versionB);
                Check(nativeSentinels.All(s => s.Value.SequenceEqual(File.ReadAllBytes(s.Key))), "actual B startup preserves native isolated preferences/library/document");
                foreach (var entry in candidateA.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(Payload(install, versionA), entry.Path), entry);
                Check(true, "all A payloads retained after actual native B startup");
                File.WriteAllText(Path.Combine(evidence, "scope.txt"), "Actual native UI workflow invokes updater with real fixed HTTPS and isolated installation: check, consent, progress, cancel, verify/install, completion/draft protection; actual installed B WinUI startup and genuine directory build-info. Programmatic native integration, not physical user laptop UAT. No user installation/launcher touched.");
                return 0;
            }
            var data = Path.Combine(evidence, "isolated-data"); Directory.CreateDirectory(data);
            var document = Path.Combine(data, "synthetic-document.pdf"); File.WriteAllText(document, "%PDF-1.4\nsynthetic-preservation-sentinel\n%%EOF");
            var preferences = Path.Combine(data, "preferences.json"); File.WriteAllText(preferences, "{\"syntheticSetting\":\"retained\"}");
            var localSource = Path.Combine(install, "update-source.json"); File.WriteAllText(localSource, JsonSerializer.Serialize(new LocalAppUpdate.Source(Path.Combine(evidence, "package-A", "feed"))));
            var sentinels = new[] { document, preferences, localSource }.ToDictionary(p => p, File.ReadAllBytes);
            var oldPointer = File.ReadAllBytes(Path.Combine(install, "current.json"));
            using (var cancel = new CancellationTokenSource())
            {
                try { await OnlineUpdatePackage.DownloadAsync(client, online, Path.Combine(evidence, "cancelled-real.zip"), new CancelProgress(cancel), cancel.Token); throw new InvalidDataException("Real download did not cancel"); }
                catch (OperationCanceledException) { Check(true, "real HTTPS streaming cancel after first progress callback"); }
            }
            Check(oldPointer.SequenceEqual(File.ReadAllBytes(Path.Combine(install, "current.json"))), "real download cancellation retains byte-exact A pointer");
            var downloaded = Path.Combine(evidence, "downloaded-real-B.zip"); var statuses = new List<string>();
            await OnlineUpdatePackage.DownloadAsync(client, online, downloaded, new CollectProgress(statuses), CancellationToken.None);
            Check(Hash(downloaded).Equals(expectedZipHash, StringComparison.OrdinalIgnoreCase) && new FileInfo(downloaded).Length == online.Size, "real HTTPS full ZIP B length/SHA256 match accepted catalog");
            var candidateB = InspectZip(downloaded, Path.Combine(evidence, "package-B"), versionB);
            using (var locked = new FileStream(Path.Combine(install, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                try { LocalAppUpdate.InstallFrom(install, Path.Combine(evidence, "package-B", "feed")); throw new InvalidDataException("Concurrent install not refused"); }
                catch (IOException) { Check(true, "real verified B install refuses held single-writer lock"); }
            }
            Check(oldPointer.SequenceEqual(File.ReadAllBytes(Path.Combine(install, "current.json"))), "real B failure retains byte-exact A pointer");
            Check(LocalAppUpdate.InstallFrom(install, Path.Combine(evidence, "package-B", "feed")) == versionB, "real HTTPS B payload activates isolated installation");
            Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "previous.json")).Version == versionA, "real activation retains previous A pointer");
            foreach (var entry in candidateB.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(Payload(install, versionB), entry.Path), entry);
            await StartNative(install, versionB);
            foreach (var entry in candidateA.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(Payload(install, versionA), entry.Path), entry);
            Check(true, "all A binaries retained unchanged after B startup");
            Check(sentinels.All(s => s.Value.SequenceEqual(File.ReadAllBytes(s.Key))), "synthetic document/preferences/local-source byte-exact preservation");
            File.WriteAllLines(Path.Combine(evidence, "download-progress.txt"), statuses);
            File.WriteAllText(Path.Combine(evidence, "live-catalog.json"), JsonSerializer.Serialize(online, EvidenceJson));
            File.WriteAllText(Path.Combine(evidence, "scope.txt"), "Actual fixed-origin HTTPS metadata/download, actual full-package core updater install, actual installed A/B WinUI startup with synthetic-only audits. Core HTTPS + native startup; not ordinary-button full native clickthrough or user laptop UAT. User installation untouched. Synthetic external preference/document sentinels are preservation checks, not migrated real preferences.");
            return 0;
        }
        catch (Exception error) { results.Add("FAIL " + error.GetType().Name + ": " + error.Message); return 1; }
        finally { File.WriteAllLines(Path.Combine(evidence, "results.txt"), results); Console.WriteLine($"Finalization: {results.Count(x => x.StartsWith("PASS ", StringComparison.Ordinal))} PASS; {results.Count(x => x.StartsWith("FAIL", StringComparison.Ordinal))} FAIL"); }
    }
    private sealed class CancelProgress(CancellationTokenSource cancel) : IProgress<string>
    { public void Report(string value) => cancel.Cancel(); }
    private sealed class CollectProgress(List<string> output) : IProgress<string>
    { public void Report(string value) => output.Add(value); }
}
