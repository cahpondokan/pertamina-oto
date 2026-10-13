#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pertamina.Oto.Distribution;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditOnlineUpdateAsync()
    {
        var liveIndex = Array.IndexOf(Environment.GetCommandLineArgs(), "--live-transition");
        if (liveIndex >= 0) { await AuditLiveOnlineTransitionAsync(liveIndex); return; }
        var openIndex = Array.IndexOf(Environment.GetCommandLineArgs(), "--live-open");
        if (openIndex >= 0) { await AuditLiveOpenAsync(openIndex); return; }
        var output = Path.Combine(AppContext.BaseDirectory, "online-update-audit"); Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child; }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(300); root.UpdateLayout(); }
        async Task Capture(string name)
        {
            await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync((UIElement)Content); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
        }
        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 900)); await Settle();
            Check(!onlineUpdateBanner.IsOpen && !onlineUpdateTimer.IsEnabled, "audit starts without public network polling or visible banner");
            ShowSettings(); await Settle();
            var toggle = Walk(root).OfType<ToggleSwitch>().Single(x => AutomationProperties.GetName(x) == "Notifikasi update saat OTO berjalan");
            Check(Walk(root).OfType<Button>().Any(x => AutomationProperties.GetName(x) == "Update"), "manual online update action appears in native settings");
            Check(Walk(root).OfType<Button>().Any(x => AutomationProperties.GetName(x) == "Update lokal"), "explicit local bootstrap action retained");
            Check(Walk(root).OfType<Button>().Any(x => AutomationProperties.GetName(x) == "Periksa update"), "primary update entry retained");
            var release = new ReleaseInfo(1, "20261001-180000-aaaaaaaa", DateTimeOffset.UtcNow, "development", "PERTAMINA-OTO-Windows-x64-20261001-180000-aaaaaaaa.zip", 100, new string('a', 64), ["Fixture notifikasi"]);
            ShowOnlineRelease(release); await Settle();
            Check(onlineUpdateBanner.IsOpen && onlineUpdateBanner.IsClosable && onlineUpdateBanner.Title == "Pembaruan OTO tersedia", "new release shows a dismissible nonmodal banner");
            Check(onlineUpdateBanner.Message.Contains(release.Version, StringComparison.Ordinal) && onlineUpdateBanner.ActionButton is Button, "banner exposes build and explicit download action");
            ShowOnlineRelease(release with { ProductVersion = ApplicationVersion.Running().ProductVersion }); await Settle();
            Check(onlineUpdateBanner.Message.Contains("Pembaruan build untuk versi produk yang sama: " + ApplicationVersion.Running().Display, StringComparison.Ordinal)
                && !onlineUpdateBanner.Message.Contains(release.Version, StringComparison.Ordinal), "known online product hides technical build and explains same version update");
            var shell = (Grid)Content;
            Check(Grid.GetRow(root) == 2 && Grid.GetRow(onlineUpdateBanner) == 1 && shell.RowDefinitions[1].Height.IsAuto, "banner occupies its own row without covering document workspace");
            await Capture("01-notification-settings");
            toggle.IsOn = true; toggle.IsOn = false; await Settle();
            Check(!onlineUpdatesEnabled && !onlineUpdateBanner.IsOpen, "disabling notifications closes banner immediately");
            Check(!System.Text.Json.JsonSerializer.Deserialize<OnlinePreference>(File.ReadAllText(OnlinePreferencePath))!.Enabled, "notification preference persists to isolated fixture");
            toggle.IsOn = true; await Settle();
            Check(onlineUpdatesEnabled, "notifications can be enabled again");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(980, 760)); await Settle(); ShowOnlineRelease(release); await Capture("02-compact-notification");
            Check(onlineUpdateBanner.ActualWidth <= shell.ActualWidth && root.ActualHeight > 400, "compact window retains space for normal workspace");
            var togglePosition = toggle.TransformToVisual(shell).TransformPoint(new Windows.Foundation.Point());
            Check(togglePosition.X >= 0 && togglePosition.X + toggle.ActualWidth <= shell.ActualWidth, "notification toggle remains fully visible in compact settings");
            static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
            manualUpdateFixtureRoot = Path.Combine(output, "fixture-install"); Directory.CreateDirectory(manualUpdateFixtureRoot);
            manualUpdateFixtureBuild = "20261001-180000-aaaaaaaa";
            File.WriteAllText(Path.Combine(manualUpdateFixtureRoot, "current.json"), System.Text.Json.JsonSerializer.Serialize(new LocalAppUpdate.Release(manualUpdateFixtureBuild, new string('a',64))));
            async Task Scenario(ReleaseInfo? fixture, string expected)
            {
                using var client = new System.Net.Http.HttpClient(new OnlineAuditHandler((_, _) => Task.FromResult(new System.Net.Http.HttpResponseMessage(fixture is null ? System.Net.HttpStatusCode.NoContent : System.Net.HttpStatusCode.OK) { Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(fixture, System.Text.Json.JsonSerializerOptions.Web), System.Text.Encoding.UTF8, "application/json") })));
                manualUpdateFixtureClient = client;
                var task = CheckManualOnlineUpdateAsync();
                var dialog = await WaitDialog(d => d.Title as string == expected); Check(dialog.Title as string == expected, "native manual state: " + expected);
                if (expected == "Update tersedia")
                {
                    var consentText = string.Join("\n", Walk(dialog).OfType<TextBlock>().Select(t => t.Text));
                    var selectedNotes = fixture is null ? null : ReleaseNotes.Online(fixture);
                    Check(consentText.Contains(" MB", StringComparison.Ordinal) && (selectedNotes is null
                        ? consentText.Contains(ReleaseNotes.Missing, StringComparison.Ordinal)
                        : selectedNotes.Releases[0].Summary.All(s => consentText.Contains(s, StringComparison.Ordinal))), "consent shows size and exact notes or honest legacy fallback");
                    Check(Walk(dialog).OfType<Button>().Any(b => AutomationProperties.GetName(b) == "Lihat detail perubahan"), "consent exposes detail action");
                }
                Invoke(Walk(dialog).OfType<Button>().Single(b => b.Name == "CloseButton")); await task;
                Check(LocalAppUpdate.VersionAt(manualUpdateFixtureRoot) == manualUpdateFixtureBuild && !checkingUpdate && !messageOpen, "dismissal preserves active fixture and resets busy state");
            }
            await Scenario(release, "Sudah terbaru");
            await Scenario(null, "Rilis belum tersedia");
            await Scenario(release with { Version = "20261002-180000-bbbbbbbb", FileName = "PERTAMINA-OTO-Windows-x64-20261002-180000-bbbbbbbb.zip" }, "Update tersedia");
            // These inert legacy-transport fixtures retain the entire real history,
            // but not indentation that inflates the schema-1 16 KiB wire budget.
            using var bundledNotes = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets/release-notes.json")));
            var bundledJson = System.Text.Json.JsonSerializer.Serialize(bundledNotes.RootElement);
            var bundledHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(bundledJson)));
            await Scenario(release with { Version = "20261002-180000-bbbbbbbb", FileName = "PERTAMINA-OTO-Windows-x64-20261002-180000-bbbbbbbb.zip",
                ProductVersion = ApplicationVersion.Running().ProductVersion, ReleaseNotesJson = bundledJson, ReleaseNotesSha256 = bundledHash }, "Update tersedia");
            using (var client = new System.Net.Http.HttpClient(new OnlineAuditHandler(async (_, token) => { await Task.Delay(Timeout.Infinite, token); return new System.Net.Http.HttpResponseMessage(); })))
            {
                manualUpdateFixtureClient = client; var task = CheckManualOnlineUpdateAsync();
                var dialog = await WaitDialog(d => d.Title as string == "Update aplikasi"); Check(dialog.Title as string == "Update aplikasi", "native checking status visible");
                Invoke(Walk(dialog).OfType<Button>().Single(b => b.Name == "CloseButton"));
                dialog = await WaitDialog(d => d.Title as string == "Update dibatalkan"); Check(dialog.Title as string == "Update dibatalkan", "native checking cancellation visible");
                Invoke(Walk(dialog).OfType<Button>().Single(b => b.Name == "CloseButton")); await task;
            }
            // Inert binaries exercise the actual UI->stream->extract->hash->atomic installer path.
            // This handler never contacts HTTPS and is deliberately not internet A->B evidence.
            manualUpdateFixtureDataRoot = Path.Combine(output, "isolated-data");
            restartFixtureRoot = manualUpdateFixtureRoot;
            var fixtureRun = Path.Combine(output, "transfer-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fixtureRun);
            manualUpdateFixtureRoot = Path.Combine(fixtureRun, "installed"); Directory.CreateDirectory(manualUpdateFixtureRoot);
            restartFixtureRoot = manualUpdateFixtureRoot;
            File.WriteAllText(Path.Combine(manualUpdateFixtureRoot, "current.json"), System.Text.Json.JsonSerializer.Serialize(new LocalAppUpdate.Release(manualUpdateFixtureBuild, new string('a',64))));
            var sourcePreference = Path.Combine(manualUpdateFixtureRoot, "update-source.json");
            File.WriteAllText(sourcePreference, "synthetic local preference sentinel");
            var document = Path.Combine(fixtureRun, "synthetic-document.pdf"); File.WriteAllText(document, "%PDF-1.4\nsynthetic preservation sentinel\n%%EOF");
            var settings = Path.Combine(fixtureRun, "settings.json"); File.WriteAllText(settings, "synthetic settings sentinel");
            var preserved = new[] { document, settings, sourcePreference }.ToDictionary(p => p, File.ReadAllBytes);
            var nativeStatuses = new List<string>(); manualUpdateFixtureStatus = nativeStatuses.Add;
            const string nextBuild = "20261002-180000-bbbbbbbb";
            byte[] Package(bool damaged)
            {
                var feed = Path.Combine(fixtureRun, Guid.NewGuid().ToString("N"), "feed");
                var package = Path.Combine(feed, nextBuild); Directory.CreateDirectory(Path.Combine(package, "app/worker"));
                var entries = new List<LocalAppUpdate.Entry>();
                foreach (var name in new[] { "Pertamina.Oto.Desktop.App.exe", "worker/Pertamina.Oto.LocalPdf.Worker.exe" })
                {
                    var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(2 * 1024 * 1024);
                    File.WriteAllBytes(Path.Combine(package, "app", name), bytes);
                    entries.Add(new(name, bytes.Length, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))));
                }
                Directory.CreateDirectory(Path.Combine(package, "app/Assets"));
                foreach (var item in new[] { ("Assets/release-notes.json", bundledJson), ("build-info.json", System.Text.Json.JsonSerializer.Serialize(new LocalAppUpdate.Release(nextBuild, "", ApplicationVersion.Running().ProductVersion, "Pengembangan"))) })
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(item.Item2); File.WriteAllBytes(Path.Combine(package, "app", item.Item1), bytes);
                    entries.Add(new(item.Item1, bytes.Length, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))));
                }
                var manifest = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new LocalAppUpdate.Manifest(nextBuild, entries.ToArray(), ApplicationVersion.Running().ProductVersion, "Pengembangan"));
                File.WriteAllBytes(Path.Combine(package, "manifest.json"), manifest);
                File.WriteAllText(Path.Combine(feed, "latest.json"), System.Text.Json.JsonSerializer.Serialize(new LocalAppUpdate.Release(nextBuild, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(manifest)), ApplicationVersion.Running().ProductVersion, "Pengembangan")));
                if (damaged) File.WriteAllText(Path.Combine(package, "app/Pertamina.Oto.Desktop.App.exe"), "corrupt synthetic payload");
                using var memory = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memory, System.IO.Compression.ZipArchiveMode.Create, true))
                    foreach (var path in Directory.GetFiles(feed, "*", SearchOption.AllDirectories))
                    { using var item = archive.CreateEntry("feed/" + Path.GetRelativePath(feed, path).Replace('\\','/'), System.IO.Compression.CompressionLevel.NoCompression).Open(); using var input = File.OpenRead(path); input.CopyTo(item); }
                return memory.ToArray();
            }
            async Task<ContentDialog> WaitDialog(Func<ContentDialog,bool> match)
            {
                for (var i = 0; i < 200; i++)
                { await Task.Delay(25); root.UpdateLayout(); var found = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Walk(p.Child)).OfType<ContentDialog>().FirstOrDefault(d => match(d) && Walk(d).OfType<Button>().Any(b => b.Name == "CloseButton" && b.IsLoaded)); if (found is not null) return found; }
                throw new TimeoutException("Native update state not reached");
            }
            void Dismiss(ContentDialog dialog) => Invoke(Walk(dialog).OfType<Button>().Single(b => b.Name == "CloseButton"));
            async Task Transfer(string mode)
            {
                nativeStatuses.Clear(); var zip = Package(mode == "payload");
                var offered = release with { Version = nextBuild, FileName = "PERTAMINA-OTO-Windows-x64-" + nextBuild + ".zip", Size = zip.Length,
                    Sha256 = mode == "hash" ? new string('0',64) : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(zip)), ProductVersion = ApplicationVersion.Running().ProductVersion,
                    ReleaseNotesJson = bundledJson, ReleaseNotesSha256 = bundledHash };
                using var client = new System.Net.Http.HttpClient(new OnlineAuditHandler((request, _) => Task.FromResult(request.RequestUri == ReleaseInfo.LatestUri
                    ? new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(offered, System.Text.Json.JsonSerializerOptions.Web), System.Text.Encoding.UTF8, "application/json") }
                    : new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new System.Net.Http.StreamContent(new OnlineAuditStream(zip, mode == "network")) })));
                manualUpdateFixtureClient = client;
                var before = File.ReadAllBytes(Path.Combine(manualUpdateFixtureRoot!, "current.json"));
                using var locked = mode == "lock" ? new FileStream(Path.Combine(manualUpdateFixtureRoot!, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None) : null;
                var task = CheckManualOnlineUpdateAsync();
                var consent = await WaitDialog(d => d.Title as string == "Update tersedia");
                Invoke(Walk(consent).OfType<Button>().Single(b => b.Name == "PrimaryButton"));
                var downloading = await WaitDialog(d => d.Title as string == "Mengunduh dan memasang update" && nativeStatuses.Any(s => s.Contains('%')));
                Check(downloading.Content is TextBlock { Text: var text } && text.Contains('%'), "native streamed percentage visible: " + mode);
                if (mode == "cancel") Dismiss(downloading);
                var terminal = await WaitDialog(d => mode == "success" ? ReferenceEquals(d, updateCompletion) : d.Title as string == (mode == "cancel" ? "Update dibatalkan" : "Update belum berhasil"));
                Check(true, "native terminal state reached: " + mode);
                if (mode == "success")
                {
                    Check(LocalAppUpdate.VersionAt(manualUpdateFixtureRoot!) == nextBuild, "native consent activates verified inert B");
                    Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(manualUpdateFixtureRoot!, "previous.json")).Version == manualUpdateFixtureBuild, "native install retains previous A pointer");
                    Check(nativeStatuses.Contains("Memverifikasi isi paket…") && nativeStatuses.Contains("Memasang update…"), "native validation and installation status reported");
                    toolDrafts["watermark"] = new([document], "", "UNSAVED", "hasil-watermark.pdf", 0, 20);
                    Invoke(Walk(terminal).OfType<Button>().Single(b => b.Name == "PrimaryButton"));
                    var guard = await WaitDialog(d => Walk(d).OfType<TextBlock>().Any(t => t.Text == "Simpan perubahan dahulu"));
                    Check(RestartHasSession && !allowClose, "native online completion restart protects unsaved synthetic draft");
                    Dismiss(guard); await task;
                    Check(toolDrafts["watermark"].Text == "UNSAVED", "cancel restart review preserves exact draft");
                    toolDrafts.Clear();
                    var restart = RestartForUpdateAsync(nextBuild);
                    var fallback = await WaitDialog(d => d.Title as string == "Aplikasi tetap terbuka");
                    Check(!allowClose && fallback.Content as string is { } copy && copy.Contains("shortcut", StringComparison.Ordinal), "missing launcher gives safe save/normal close/reopen fallback");
                    Dismiss(fallback); await restart;
                }
                else
                {
                    Check(before.SequenceEqual(File.ReadAllBytes(Path.Combine(manualUpdateFixtureRoot!, "current.json"))), "native failure/cancel preserves byte-exact pointer: " + mode);
                    Check(!File.Exists(Path.Combine(manualUpdateFixtureRoot!, "previous.json")), "failed native transfer does not fabricate previous pointer: " + mode);
                    Dismiss(terminal); await task;
                }
                Check(preserved.All(p => p.Value.SequenceEqual(File.ReadAllBytes(p.Key))), "native transfer preserves synthetic document/settings/local source: " + mode);
                Check(!checkingUpdate && !messageOpen && !allowClose, "native transfer resets busy state and keeps session: " + mode);
            }
            await Transfer("cancel"); await Transfer("network"); await Transfer("hash"); await Transfer("payload"); await Transfer("lock"); await Transfer("success");
            File.WriteAllText(Path.Combine(output, "fixture-scope.txt"), "Native WinUI and actual updater code, inert random binaries with in-memory HTTP handler. No public internet download, binary launch or user installation. Not actual HTTPS A-to-B evidence.");
            manualUpdateFixtureClient = null; manualUpdateFixtureRoot = null; manualUpdateFixtureBuild = null;
            manualUpdateFixtureDataRoot = null; manualUpdateFixtureStatus = null; restartFixtureRoot = null;
        }
        catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
        File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
    }

    private async Task AuditLiveOnlineTransitionAsync(int index)
    {
        var args = Environment.GetCommandLineArgs(); var report = new List<string>(); string? evidence = null;
        void Check(bool value, string label) { if (!value) throw new InvalidDataException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child; }
        static void Invoke(ContentDialog dialog, string name) => ((IInvokeProvider)new ButtonAutomationPeer(Walk(dialog).OfType<Button>().Single(b => b.Name == name)).GetPattern(PatternInterface.Invoke)).Invoke();
        var statuses = new List<string>();
        async Task<ContentDialog> Wait(Func<ContentDialog, bool> match, Task operation)
        {
            var until = DateTimeOffset.UtcNow.AddMinutes(21);
            while (DateTimeOffset.UtcNow < until)
            {
                await Task.Delay(10);
                var dialog = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Walk(p.Child)).OfType<ContentDialog>().FirstOrDefault(d => match(d) && Walk(d).OfType<Button>().Any(b => b.Name == "CloseButton" && b.IsLoaded));
                if (dialog is not null) return dialog;
                if (operation.IsCompleted) { await operation; throw new InvalidDataException("Expected native state not reached"); }
                var failure = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Walk(p.Child)).OfType<ContentDialog>().FirstOrDefault(d => d.Title as string == "Update belum berhasil" && Walk(d).OfType<Button>().Any(b => b.Name == "CloseButton" && b.IsLoaded));
                if (failure is not null) { Invoke(failure, "CloseButton"); await operation; throw new InvalidDataException("Live update reported failure"); }
            }
            throw new TimeoutException("Live native update exceeded bounded deadline");
        }
        try
        {
            if (index + 3 >= args.Length) throw new InvalidDataException("Live mode requires isolated install root, expected B and accepted ZIP hash");
            var install = Path.GetFullPath(args[index + 1]); var expectedB = args[index + 2]; var expectedHash = args[index + 3];
            LocalAppUpdate.ValidateVersion(expectedB);
            if (!System.Text.RegularExpressions.Regex.IsMatch(expectedHash, "^[A-Fa-f0-9]{64}$")) throw new InvalidDataException("Invalid accepted ZIP hash");
            // Discover workspace boundary from the actual A installation, never from user LocalAppData.
            var boundary = Directory.GetParent(install)?.Parent;
            if (boundary is null || boundary.Name != "online-update-finalization" || boundary.Parent?.Name != "artifacts"
                || !Directory.GetParent(install)!.Name.StartsWith("live-", StringComparison.Ordinal) || !Guid.TryParseExact(Directory.GetParent(install)!.Name[5..], "N", out _) || Path.GetFileName(install) != "installation")
                throw new InvalidDataException("Live root must be artifacts/online-update-finalization/live-*/installation");
            _ = LocalAppUpdate.SafePath(boundary.FullName, Path.GetRelativePath(boundary.FullName, install));
            var runningA = RunningBuildLabel(); LocalAppUpdate.ValidateVersion(runningA);
            using (var marker = System.Text.Json.JsonDocument.Parse(File.ReadAllText(LocalAppUpdate.SafePath(install, "live-audit-marker.json"))))
            {
                var entry = marker.RootElement; var created = entry.GetProperty("CreatedUtc").GetDateTimeOffset();
                if (entry.GetProperty("Schema").GetInt32() != 1 || entry.GetProperty("BuildA").GetString() != runningA || entry.GetProperty("BuildB").GetString() != expectedB
                    || !expectedHash.Equals(entry.GetProperty("ZipSha256").GetString(), StringComparison.OrdinalIgnoreCase)
                    || DateTimeOffset.UtcNow - created > TimeSpan.FromMinutes(15) || created - DateTimeOffset.UtcNow > TimeSpan.FromMinutes(1)
                    || File.Exists(Path.Combine(install, "live-audit-consumed.json"))) throw new InvalidDataException("Fresh, exact, unused live test marker required");
            }
            var active = LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "current.json"));
            Check(active.Version == runningA && string.CompareOrdinal(expectedB[..15], runningA[..15]) > 0, "genuine running A matches isolated current pointer and B is newer");
            var actual = Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar);
            Check(actual.Equals(LocalAppUpdate.SafePath(install, "versions/" + runningA), StringComparison.OrdinalIgnoreCase), "native A process runs from isolated installed A payload");
            var manifestPath = LocalAppUpdate.SafePath(install, "audit-A-manifest.json");
            using (var stream = File.OpenRead(manifestPath)) Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).Equals(active.ManifestSha256, StringComparison.OrdinalIgnoreCase), "A audit manifest bound to immutable current pointer");
            var manifest = LocalAppUpdate.Read<LocalAppUpdate.Manifest>(manifestPath);
            Check(manifest.Version == runningA, "A manifest build identity matches real running binary directory");
            foreach (var entry in manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(actual, entry.Path), entry);
            Check(true, "all " + manifest.Files.Length + " real A payloads verified before live network");
            evidence = Directory.GetParent(install)!.FullName;
            var data = LocalAppUpdate.SafePath(evidence, "isolated-data");
            if (Directory.Exists(data) || File.Exists(data) || File.Exists(Path.Combine(evidence, "native-live-results.txt")) || File.Exists(Path.Combine(evidence, "native-live-progress.txt")) || File.Exists(Path.Combine(evidence, "live-catalog.json")))
                throw new IOException("Fresh native data/results required; previous evidence is never overwritten");
            using (var consumed = new FileStream(LocalAppUpdate.SafePath(install, "live-audit-consumed.json"), FileMode.CreateNew)) System.Text.Json.JsonSerializer.Serialize(consumed, new { BuildA = runningA, BuildB = expectedB });
            Directory.CreateDirectory(data);
            var document = Path.Combine(data, "synthetic-preservation.pdf"); WriteHistoryAuditFixture(document);
            restartAuditPreference = Path.Combine(data, "preferences.json");
            File.WriteAllText(restartAuditPreference, System.Text.Json.JsonSerializer.Serialize(new FolderPreference(data)));
            libraryTestPath = Path.Combine(data, "library.json");
            File.WriteAllText(libraryTestPath, System.Text.Json.JsonSerializer.Serialize(new SavedLibrary(1, ["merge"], [new SavedJob(Guid.Parse("11111111-1111-1111-1111-111111111111"), DateTimeOffset.UtcNow, "merge", [document], "Synthetic live update history")])));
            favorites.Clear(); savedJobs.Clear(); LoadFolderPreference(); LoadLibrary();
            var source = LocalAppUpdate.SafePath(install, "update-source.json");
            if (!File.Exists(source)) throw new InvalidDataException("Runner must seed explicit trusted local source before native live mode");
            var sentinels = new[] { document, restartAuditPreference, libraryTestPath, source }.ToDictionary(p => p, File.ReadAllBytes);
            var oldPointer = File.ReadAllBytes(Path.Combine(install, "current.json"));
            manualUpdateFixtureRoot = install; manualUpdateFixtureDataRoot = data; restartFixtureRoot = install;
            manualUpdateFixtureStatus = statuses.Add; // Real running build and real OnlineHttp are deliberately retained.
            Check(manualUpdateFixtureBuild is null && manualUpdateFixtureClient is null, "live UI retains real build identity and fixed-origin no-redirect HTTP client");
            var offered = await OnlineReleaseClient.FetchAsync(OnlineHttp, CancellationToken.None) ?? throw new InvalidDataException("No live metadata");
            Check(offered.Version == expectedB && offered.Sha256.Equals(expectedHash, StringComparison.OrdinalIgnoreCase), "real HTTPS offer matches accepted immutable B ZIP identity");
            File.WriteAllText(Path.Combine(evidence, "live-catalog.json"), System.Text.Json.JsonSerializer.Serialize(offered));
            ShowSettings();
            async Task<ContentDialog> Start(bool cancel)
            {
                statuses.Clear(); var operation = CheckManualOnlineUpdateAsync();
                var consent = await Wait(d => d.Title as string == "Update tersedia", operation);
                Check(consent.Content as string is { } body && body.Contains(expectedB, StringComparison.Ordinal), "actual native consent shows accepted B before install action");
                Invoke(consent, "PrimaryButton");
                if (cancel)
                {
                    var download = await Wait(d => d.Title as string == "Mengunduh dan memasang update" && statuses.Any(s => s.Contains('%') && !s.Contains("100%", StringComparison.Ordinal)), operation);
                    Check(true, "actual HTTPS native percentage observed before cancellation"); Invoke(download, "CloseButton");
                    var cancelled = await Wait(d => d.Title as string == "Update dibatalkan", operation); Invoke(cancelled, "CloseButton"); await operation;
                    Check(oldPointer.SequenceEqual(File.ReadAllBytes(Path.Combine(install, "current.json"))), "actual native download cancellation preserves byte-exact A pointer");
                    Check(sentinels.All(s => s.Value.SequenceEqual(File.ReadAllBytes(s.Key))), "actual native cancellation preserves synthetic document/settings/local source");
                    return cancelled;
                }
                var completion = await Wait(d => ReferenceEquals(d, updateCompletion), operation);
                Check(LocalAppUpdate.VersionAt(install) == expectedB, "actual native HTTPS install activates exact B");
                var completedPackages = Directory.GetFiles(Path.Combine(data, "update-downloads"), "package.zip", SearchOption.AllDirectories).Where(p => new FileInfo(p).Length == offered.Size).ToArray();
                var acceptedPackages = completedPackages.Where(p => { using var stream = File.OpenRead(p); return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).Equals(expectedHash, StringComparison.OrdinalIgnoreCase); }).ToArray();
                Check(acceptedPackages.Length == 1, "actual native success staging ZIP hash equals accepted publisher hash after second metadata fetch");
                File.WriteAllText(Path.Combine(evidence, "native-downloaded-package.json"), System.Text.Json.JsonSerializer.Serialize(new { Build = expectedB, ZipSha256 = expectedHash, ZipSize = offered.Size, RelativePath = Path.GetRelativePath(evidence, acceptedPackages[0]) }));
                Check(statuses.Any(s => s.Contains('%')) && statuses.Contains("Memverifikasi isi paket…") && statuses.Contains("Memasang update…"), "actual native progress/validation/install states reported");
                Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "previous.json")).Version == runningA, "actual native install retains previous A pointer");
                toolDrafts["watermark"] = new([document], "", "UNSAVED-LIVE-SYNTHETIC", "hasil-watermark.pdf", 0, 20);
                Invoke(completion, "PrimaryButton");
                var guard = await Wait(d => Walk(d).OfType<TextBlock>().Any(t => t.Text == "Simpan perubahan dahulu"), operation);
                Check(RestartHasSession && !allowClose, "actual live online completion protects unsaved synthetic work");
                Invoke(guard, "CloseButton"); await operation;
                Check(toolDrafts["watermark"].Text == "UNSAVED-LIVE-SYNTHETIC", "cancel native restart review retains exact unsaved draft");
                toolDrafts.Clear();
                Check(sentinels.All(s => s.Value.SequenceEqual(File.ReadAllBytes(s.Key))), "actual live install preserves synthetic document/preferences/history/local source bytes");
                foreach (var entry in manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(actual, entry.Path), entry);
                Check(true, "all real A payloads unchanged after actual B activation");
                File.WriteAllLines(Path.Combine(evidence, "native-live-progress.txt"), statuses);
                return completion;
            }
            await Start(true); await Start(false);
            Check(!checkingUpdate && !messageOpen && !allowClose, "native live flow completes without closing old session or changing user installation");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        finally
        {
            manualUpdateFixtureRoot = null; manualUpdateFixtureDataRoot = null; manualUpdateFixtureStatus = null; restartFixtureRoot = null;
            if (evidence is not null && !File.Exists(Path.Combine(evidence, "native-live-results.txt"))) File.WriteAllLines(Path.Combine(evidence, "native-live-results.txt"), report);
            else Console.Error.WriteLine(string.Join(Environment.NewLine, report));
        }
    }

    private async Task AuditLiveOpenAsync(int index)
    {
        var args = Environment.GetCommandLineArgs(); var report = new List<string>(); string? evidence = null;
        void Check(bool value, string label) { if (!value) throw new InvalidDataException(label); report.Add("PASS " + label); }
        try
        {
            if (index + 3 >= args.Length) throw new InvalidDataException("Live open requires isolated root, expected B and manifest hash");
            var install = Path.GetFullPath(args[index + 1]); var expected = args[index + 2]; var expectedHash = args[index + 3];
            LocalAppUpdate.ValidateVersion(expected);
            var run = Directory.GetParent(install); var boundary = run?.Parent;
            if (boundary is null || boundary.Name != "online-update-finalization" || boundary.Parent?.Name != "artifacts" || run is null || !run.Name.StartsWith("live-", StringComparison.Ordinal)
                || !Guid.TryParseExact(run.Name[5..], "N", out _) || Path.GetFileName(install) != "installation") throw new InvalidDataException("Invalid live-open root");
            _ = LocalAppUpdate.SafePath(boundary.FullName, Path.GetRelativePath(boundary.FullName, install));
            var active = LocalAppUpdate.Read<LocalAppUpdate.Release>(LocalAppUpdate.SafePath(install, "current.json"));
            Check(active.Version == expected && RunningBuildLabel() == expected && active.ManifestSha256.Equals(expectedHash, StringComparison.OrdinalIgnoreCase), "actual B running identity/current pointer/accepted manifest agree");
            Check(Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar).Equals(LocalAppUpdate.SafePath(install, "versions/" + expected), StringComparison.OrdinalIgnoreCase), "actual B process path is isolated installed B");
            if (!File.Exists(LocalAppUpdate.SafePath(install, "live-audit-consumed.json")) || File.Exists(Path.Combine(run.FullName, "native-live-open-results.txt"))) throw new IOException("Completed fresh live transition required before B open");
            var manifestPath = LocalAppUpdate.SafePath(install, "audit-B-manifest.json");
            using (var stream = File.OpenRead(manifestPath)) Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).Equals(expectedHash, StringComparison.OrdinalIgnoreCase), "B audit manifest hash matches accepted B");
            var manifest = LocalAppUpdate.Read<LocalAppUpdate.Manifest>(manifestPath);
            Check(manifest.Version == expected, "accepted B manifest identity matches actual process");
            foreach (var entry in manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(AppContext.BaseDirectory, entry.Path), entry);
            Check(true, "all " + manifest.Files.Length + " actual B payloads verified before settings read");
            var data = LocalAppUpdate.SafePath(run.FullName, "isolated-data");
            restartAuditPreference = LocalAppUpdate.SafePath(data, "preferences.json"); libraryTestPath = LocalAppUpdate.SafePath(data, "library.json");
            var document = LocalAppUpdate.SafePath(data, "synthetic-preservation.pdf"); var source = LocalAppUpdate.SafePath(install, "update-source.json");
            var sentinels = new[] { restartAuditPreference, libraryTestPath, document, source }.ToDictionary(p => p, File.ReadAllBytes);
            evidence = run.FullName;
            favorites.Clear(); savedJobs.Clear(); defaultFolder = null; LoadFolderPreference(); LoadLibrary();
            Check(defaultFolder == data, "actual installed B loads prior isolated output-folder preference");
            Check(favorites.Count == 1 && favorites.Contains("merge"), "actual installed B loads prior isolated favorite");
            Check(savedJobs.Count == 1 && savedJobs[0].Id == Guid.Parse("11111111-1111-1111-1111-111111111111") && savedJobs[0].Tool == "merge" && savedJobs[0].Paths.SequenceEqual([document]), "actual installed B loads exact synthetic history job/path/identity");
            ShowSettings(); await Task.Delay(250); root.UpdateLayout(); ShowHome(); await Task.Delay(250); root.UpdateLayout();
            Check(root.ActualWidth > 400 && root.ActualHeight > 400 && !onlineUpdateTimer.IsEnabled, "actual B renders native settings/home with audit network polling disabled");
            Check(sentinels.All(s => s.Value.SequenceEqual(File.ReadAllBytes(s.Key))), "actual B settings/history read leaves source/document/JSON bytes unchanged");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        finally
        {
            if (evidence is not null && !File.Exists(Path.Combine(evidence, "native-live-open-results.txt"))) File.WriteAllLines(Path.Combine(evidence, "native-live-open-results.txt"), report);
            else Console.Error.WriteLine(string.Join(Environment.NewLine, report));
        }
    }
}
internal sealed class OnlineAuditHandler(Func<System.Net.Http.HttpRequestMessage, CancellationToken, Task<System.Net.Http.HttpResponseMessage>> send) : System.Net.Http.HttpMessageHandler
{ protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request,CancellationToken cancellationToken) => send(request,cancellationToken); }
internal sealed class OnlineAuditStream(byte[] bytes, bool disconnect = false) : MemoryStream(bytes)
{
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        if (disconnect && Position >= Length / 2) throw new IOException("Synthetic connection interrupted mid-stream");
        return await base.ReadAsync(buffer[..Math.Min(buffer.Length, 32768)], cancellationToken);
    }
}
#endif
