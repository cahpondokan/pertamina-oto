using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;

internal static class LauncherDeliveryChecks
{
    internal static int Run(string evidence)
    {
        Directory.CreateDirectory(evidence);
        var results = new List<string>();
        void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); results.Add("PASS " + label); }
        void Reject(Action action, string label)
        {
            try { action(); } catch (Exception e) when (e is IOException or InvalidDataException or JsonException or OperationCanceledException) { results.Add("PASS " + label); return; }
            throw new InvalidOperationException(label);
        }
        string Hash(string file) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
        void Json(string file, object value) => File.WriteAllText(file, JsonSerializer.Serialize(value));
        void Bundle(string directory, string content)
        {
            Directory.CreateDirectory(directory);
            var entries = new List<LocalAppUpdate.Entry>();
            foreach (var name in LauncherUpdate.Files)
            {
                var file = Path.Combine(directory, name); File.WriteAllText(file, content + name);
                entries.Add(new(name, new FileInfo(file).Length, Hash(file)));
            }
            Json(Path.Combine(directory, LauncherUpdate.CapabilityFile), new LauncherUpdate.Capability(1, entries.ToArray()));
        }
        string Feed(string name, string version)
        {
            var feed = Path.Combine(evidence, name, "feed");
            var app = Path.Combine(feed, version, "app"); Directory.CreateDirectory(Path.Combine(app, "worker"));
            File.WriteAllText(Path.Combine(app, "Pertamina.Oto.Desktop.App.exe"), version);
            File.WriteAllText(Path.Combine(app, "worker", "Pertamina.Oto.LocalPdf.Worker.exe"), version);
            Bundle(Path.Combine(app, LauncherUpdate.BundleFolder), version);
            var files = Directory.GetFiles(app, "*", SearchOption.AllDirectories).Select(f => new LocalAppUpdate.Entry(Path.GetRelativePath(app, f).Replace('\\', '/'), new FileInfo(f).Length, Hash(f))).ToArray();
            var manifest = Path.Combine(feed, version, "manifest.json"); Json(manifest, new LocalAppUpdate.Manifest(version, files));
            Json(Path.Combine(feed, "latest.json"), new LocalAppUpdate.Release(version, Hash(manifest)));
            return feed;
        }
        const string first = "20261002-210000-aaaaaaaa", next = "20261002-220000-bbbbbbbb";
        try
        {
            var feed = Feed("first", first); var newer = Feed("next", next);
            var root = Path.Combine(evidence, "legacy install"); Directory.CreateDirectory(root);
            Bundle(root, "old"); File.Delete(Path.Combine(root, LauncherUpdate.CapabilityFile));
            File.WriteAllText(Path.Combine(root, "sentinel.txt"), "keep data");
            Check(LocalAppUpdate.InstallFrom(root, feed) == first, "legacy launcher migrates during verified app install");
            LauncherUpdate.Validate(root);
            Check(File.ReadAllText(Path.Combine(root, "PERTAMINA-OTO.dll")).StartsWith(first, StringComparison.Ordinal), "new capability matches actual launcher");
            Check(Directory.GetDirectories(root, "launcher-backup-*").Length == 1, "old launcher backup retained");
            Check(File.ReadAllText(Path.Combine(root, "sentinel.txt")) == "keep data", "user data retained");
            var pointer = File.ReadAllBytes(Path.Combine(root, "current.json"));
            var oldHash = Hash(Path.Combine(root, "PERTAMINA-OTO.dll"));
            var source = Path.Combine(newer, next, "app", LauncherUpdate.BundleFolder);
            Reject(() => { using var transaction = LauncherUpdate.Begin(root, source, afterReplace: n => { if (n == 2) throw new IOException("injected file failure"); }); }, "partial copy failure rolls back");
            Check(Hash(Path.Combine(root, "PERTAMINA-OTO.dll")) == oldHash, "rollback restores exact prior launcher"); LauncherUpdate.Validate(root);
            using (var locked = new FileStream(Path.Combine(root, "current.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                Reject(() => LocalAppUpdate.InstallFrom(root, newer), "pointer commit failure rolls launcher back");
            Check(Hash(Path.Combine(root, "PERTAMINA-OTO.dll")) == oldHash && pointer.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(root, "current.json"))), "launcher and pointer both retained on failed commit");
            using (var locked = new FileStream(Path.Combine(root, "launcher-maintenance.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Reject(() => LocalAppUpdate.InstallFrom(root, newer), "concurrent launcher maintenance rejected");
            using (var locked = new FileStream(Path.Combine(root, "update.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Reject(() => LocalAppUpdate.InstallFrom(root, newer), "concurrent updater rejected");
            Reject(() => LocalAppUpdate.InstallFrom(root, newer, new CancellationToken(true)), "cancelled install retains launcher and pointer");
            Check(LocalAppUpdate.InstallFrom(root, newer) == next, "subsequent update migrates launcher and app together");
            Check(LocalAppUpdate.VersionAt(root) == next && LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(root, "previous.json")).Version == first, "old application remains recoverable");
            File.Delete(Path.Combine(root, LauncherUpdate.CapabilityFile));
            Check(LocalAppUpdate.InstallFrom(root, newer) == next, "same-version repair restores missing capability"); LauncherUpdate.Validate(root);
            var backups = Directory.GetDirectories(root, "launcher-backup-*").Length;
            LocalAppUpdate.InstallFrom(root, newer);
            Check(Directory.GetDirectories(root, "launcher-backup-*").Length == backups, "identical bundle is a no-op");
            Reject(() => LocalAppUpdate.InstallFrom(root, feed), "older release cannot downgrade launcher");
            var broken = Path.Combine(evidence, "tampered"); Bundle(broken, "tamper"); File.AppendAllText(Path.Combine(broken, "PERTAMINA-OTO.dll"), "damage");
            Reject(() => { using var t = LauncherUpdate.Begin(root, broken); }, "corrupt bundle rejected before mutation");
            Check(LocalAppUpdate.VersionAt(root) == next, "bad bundle preserves active version");
            var fresh = Path.Combine(evidence, "fresh"); Check(LocalAppUpdate.InstallFrom(fresh, feed) == first, "fresh installation includes restart capability"); LauncherUpdate.Validate(fresh);
            var package = Path.Combine(evidence, "first"); Bundle(Path.Combine(package, "launcher"), "ignored outer launcher");
            Json(Path.Combine(package, "launcher-manifest.json"), LauncherUpdate.Validate(Path.Combine(package, "launcher")).Files);
            var installer = Path.Combine(evidence, "installer");
            Check(InstallPackage.Install(package, installer) == first, "installer uses manifest-bound launcher instead of unrelated outer bundle");
            LauncherUpdate.Validate(installer);
            Check(File.ReadAllText(Path.Combine(installer, "PERTAMINA-OTO.dll")).StartsWith(first, StringComparison.Ordinal), "installer and online update use identical verified launcher");
            var legacy = Path.Combine(evidence, "legacy rollback"); Directory.CreateDirectory(legacy);
            Bundle(legacy, "legacy"); File.Delete(Path.Combine(legacy, LauncherUpdate.CapabilityFile));
            var legacyHash = Hash(Path.Combine(legacy, "PERTAMINA-OTO.dll"));
            Reject(() => { using var t = LauncherUpdate.Begin(legacy, source, afterReplace: n => { if (n == 5) throw new IOException("after capability"); }); }, "failure after capability commit rolls back legacy migration");
            Check(!File.Exists(Path.Combine(legacy, LauncherUpdate.CapabilityFile)) && Hash(Path.Combine(legacy, "PERTAMINA-OTO.dll")) == legacyHash, "legacy rollback removes only newly created capability and restores old bytes");
            using (var locked = new FileStream(Path.Combine(legacy, "PERTAMINA-OTO.dll"), FileMode.Open, FileAccess.Read, FileShare.Read))
                Reject(() => { using var t = LauncherUpdate.Begin(legacy, source); }, "locked launcher file refuses replacement and rolls back earlier files");
            Check(Hash(Path.Combine(legacy, "PERTAMINA-OTO.dll")) == legacyHash, "locked file failure preserves launcher");
            var partial = Path.Combine(evidence, "partial capability"); Bundle(partial, "partial");
            var originalCapability = LauncherUpdate.Validate(partial);
            Json(Path.Combine(partial, LauncherUpdate.CapabilityFile), originalCapability with { Files = originalCapability.Files[..3] });
            Reject(() => { using var t = LauncherUpdate.Begin(legacy, partial); }, "partial capability rejected before mutation");
            var linked = Path.Combine(evidence, "linked launcher"); Bundle(linked, "links");
            File.Delete(Path.Combine(linked, "PERTAMINA-OTO.dll"));
            File.CreateSymbolicLink(Path.Combine(linked, "PERTAMINA-OTO.dll"), Path.Combine(source, "PERTAMINA-OTO.dll"));
            Reject(() => { using var t = LauncherUpdate.Begin(legacy, linked); }, "source reparse link rejected");
            var (_, manifest, folder) = LocalAppUpdate.InspectFeed(feed);
            Reject(() => LauncherUpdate.BundledSource(Path.Combine(folder, "app"), manifest with { Files = manifest.Files.Where(f => f.Path != "restart-launcher/restart-capability.json").ToArray() }), "partial manifest binding rejected");
            Check(Directory.GetFiles(root, "*.pending*").Length == 0, "no partial launcher files exposed as final files");
        }
        catch (Exception error) { results.Add("FAIL " + error); }
        File.WriteAllLines(Path.Combine(evidence, "results.txt"), results);
        return results.Any(r => r.StartsWith("FAIL", StringComparison.Ordinal)) ? 1 : 0;
    }
}
