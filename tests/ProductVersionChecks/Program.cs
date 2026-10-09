using System.Security.Cryptography;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;

var output = Path.GetFullPath(args.Single()); Directory.CreateDirectory(output);
var report = new List<string>();
void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); report.Add("PASS " + label); }
void Json(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value));
const string first = "20261001-120000-aaaaaaaa", second = "20261001-120001-bbbbbbbb";
var running = ApplicationVersion.Running();
Check(running.Display == System.Xml.Linq.XDocument.Load(Path.Combine(AppContext.BaseDirectory, "../../../../../eng/ProductRelease.props")).Descendants("OtoProductVersion").Single().Value + " · Pengembangan", "embedded running binary product/channel");
Check(running.BuildId is null, "unpackaged binary has no invented package ID");
var fixture = Path.Combine(output, "metadata"); Directory.CreateDirectory(fixture);
Check(ApplicationVersion.ReadDirectory(fixture, first).Display.Contains(first), "missing legacy metadata transparent fallback");
Json(Path.Combine(fixture, "build-info.json"), new LocalAppUpdate.Release(first, ""));
Check(!ApplicationVersion.ReadDirectory(fixture).Known, "legacy package never mislabeled current product");
Check(ApplicationVersion.ReadDirectory(fixture).Display == "Versi produk belum tercatat · Build " + first, "legacy build visible as fallback");
Json(Path.Combine(fixture, "build-info.json"), new LocalAppUpdate.Release(first, "", running.ProductVersion, "Pengembangan"));
Check(ApplicationVersion.ReadDirectory(fixture, first).Display == running.Display, "new package own product identity");
Check(!ApplicationVersion.ReadDirectory(fixture, second).Known, "mismatched staged metadata rejected");
File.WriteAllText(Path.Combine(fixture, "build-info.json"), "broken");
Check(!ApplicationVersion.ReadDirectory(fixture, first).Known, "malformed metadata safe fallback");
foreach (var bad in new[] { "01.1.0", "0.1", "1.0.0\nsecret", "C:/private.pdf", "0.1.0<script>" })
    Check(!new ApplicationVersion.Identity(bad, "Pengembangan", first).Known, "invalid product suppressed " + report.Count);
Check(!new ApplicationVersion.Identity("0.1.0", "secret\nchannel", first).Known, "untrusted channel suppressed");
Check(ApplicationVersion.UpdateDescription(running, new(running.ProductVersion, "Pengembangan", second)).StartsWith("Pembaruan build", StringComparison.Ordinal), "same product new build explained");
Check(!ApplicationVersion.UpdateDescription(running, new(running.ProductVersion == "0.2.0" ? "0.3.0" : "0.2.0", "Pengembangan", second)).StartsWith("Pembaruan build", StringComparison.Ordinal), "minor release separate description");
var feed = Path.Combine(output, "feed"); Directory.CreateDirectory(feed);
void Publish(string build)
{
    var package = Path.Combine(feed, build); var app = Path.Combine(package, "app"); Directory.CreateDirectory(Path.Combine(app, "worker"));
    File.WriteAllText(Path.Combine(app, "Pertamina.Oto.Desktop.App.exe"), "inert fixture");
    File.WriteAllText(Path.Combine(app, "worker/Pertamina.Oto.LocalPdf.Worker.exe"), "inert fixture");
    Json(Path.Combine(app, "build-info.json"), new LocalAppUpdate.Release(build, "", running.ProductVersion, "Pengembangan"));
    var entries = Directory.GetFiles(app, "*", SearchOption.AllDirectories).Select(path => new LocalAppUpdate.Entry(Path.GetRelativePath(app, path).Replace('\\','/'), new FileInfo(path).Length, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))))).ToArray();
    var manifestPath = Path.Combine(package, "manifest.json"); Json(manifestPath, new LocalAppUpdate.Manifest(build, entries, running.ProductVersion, "Pengembangan"));
    Json(Path.Combine(feed, "latest.json"), new LocalAppUpdate.Release(build, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))), running.ProductVersion, "Pengembangan"));
}
var installed = Path.Combine(output, "installed"); Publish(first); LocalAppUpdate.InstallFrom(installed, feed);
Publish(second); var available = LocalAppUpdate.InspectFeed(feed);
Check(ApplicationVersion.Available(available.Manifest, available.Package).Display == running.Display, "verified available package identity");
Check(LocalAppUpdate.InstallFrom(installed, feed) == second, "same product newer build installs");
Check(ApplicationVersion.ReadDirectory(Path.Combine(installed, "versions", second), second).Display == running.Display, "staged directory owns its identity");
Check(ApplicationVersion.Running() == running, "current pointer cannot relabel running binary");
Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(installed, "previous.json")).Version == first, "rollback identity retained");
Publish(first);
try { LocalAppUpdate.InstallFrom(installed, feed); throw new InvalidOperationException("older build accepted"); }
catch (InvalidDataException) { report.Add("PASS older timestamp rejected despite same product"); }
Check(LocalAppUpdate.VersionAt(installed) == second, "rejection preserves active pointer");
File.AppendAllText(Path.Combine(available.Package, "app/build-info.json"), " ");
try { ApplicationVersion.Available(available.Manifest, available.Package); throw new InvalidOperationException("tampered identity accepted"); }
catch (InvalidDataException) { report.Add("PASS available identity requires manifest hash"); }
Check(UpdateTests.Run(Path.Combine(output, "legacy-updater")) == 0, "existing legacy updater regressions");
File.WriteAllLines(Path.Combine(output, "results.txt"), report); Console.WriteLine($"{report.Count} PASS; legacy updater 13 PASS");
