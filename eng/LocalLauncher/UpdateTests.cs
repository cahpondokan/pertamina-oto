using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;

internal static class UpdateTests
{
    internal static int Run(string root)
    {
        Directory.CreateDirectory(root);
        var feed = Path.Combine(root, "feed"); var install = Path.Combine(root, "installed"); Directory.CreateDirectory(feed); Directory.CreateDirectory(install);
        var results = new List<string>();
        void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); results.Add("PASS " + label); }
        void Reject(Action action, string label) { try { action(); } catch (Exception) { results.Add("PASS " + label); return; } throw new InvalidOperationException(label); }
        void Json(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value));
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        void Publish(string version)
        {
            var package = Path.Combine(feed, version); Directory.CreateDirectory(Path.Combine(package, "app", "worker"));
            var files = new List<LocalAppUpdate.Entry>();
            foreach (var path in new[] { "Pertamina.Oto.Desktop.App.exe", "worker/Pertamina.Oto.LocalPdf.Worker.exe" })
            { var target = Path.Combine(package, "app", path); File.WriteAllText(target, version); files.Add(new(path, new FileInfo(target).Length, Hash(target))); }
            var manifest = Path.Combine(package, "manifest.json"); Json(manifest, new LocalAppUpdate.Manifest(version, files.ToArray()));
            Json(Path.Combine(feed, "latest.json"), new LocalAppUpdate.Release(version, Hash(manifest)));
        }
        try
        {
            Json(Path.Combine(install, "update-source.json"), new LocalAppUpdate.Source(feed));
            const string first = "20260928-100000-aaaaaaaa", second = "20260928-110000-bbbbbbbb";
            Publish(first); Check(LocalAppUpdate.Install(install) == first, "initial install verified");
            Check(LocalAppUpdate.Install(install) == first, "same version no-op");
            Publish(second);
            var binary = Path.Combine(feed, second, "app", "Pertamina.Oto.Desktop.App.exe"); File.AppendAllText(binary, "tamper");
            Reject(() => LocalAppUpdate.Install(install), "damaged payload rejected"); Check(LocalAppUpdate.VersionAt(install) == first, "failed update retains active version");
            File.WriteAllText(binary, second);
            Check(LocalAppUpdate.Install(install) == second, "new version activated");
            Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(install, "previous.json")).Version == first, "previous pointer retained");
            Check(Directory.Exists(Path.Combine(install, "versions", first)), "previous binaries retained");
            Reject(() => LocalAppUpdate.SafePath(install, "../outside.exe"), "traversal rejected");
            Reject(() => LocalAppUpdate.SafePath(install, "C:/outside.exe"), "absolute path rejected");
            Reject(() => LocalAppUpdate.SafePath(install, "app.exe:stream"), "alternate stream rejected");
            Reject(() => LocalAppUpdate.ValidateVersion("../bad"), "invalid version rejected");
            using (var gate = new FileStream(Path.Combine(install, "update.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Reject(() => LocalAppUpdate.Install(install), "concurrent installer rejected");
            File.AppendAllText(Path.Combine(feed, second, "manifest.json"), "bad");
            Reject(() => LocalAppUpdate.Inspect(install), "damaged manifest rejected");
            File.WriteAllLines(Path.Combine(root, "test-results.txt"), results); return 0;
        }
        catch (Exception error) { results.Add("FAIL " + error.GetType().Name); File.WriteAllLines(Path.Combine(root, "test-results.txt"), results); return 1; }
    }
}
