using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.Distribution;

var output = Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
var report = new List<string>();
void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); report.Add("PASS " + label); }
string Json(object value) => JsonSerializer.Serialize(value);
var product = ApplicationVersion.Running().ProductVersion!;
var differentProduct = product == "0.0.0" ? "0.0.1" : "0.0.0";
var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Hosts/Pertamina.Oto.Desktop.App/Assets/release-notes.json"));
var raw = File.ReadAllText(source); var catalog = ReleaseNotes.Parse(raw, product)!;
const string oldBuild = "20261004-120000-aaaaaaaa", newBuild = "20261004-140000-bbbbbbbb";
var running = new ApplicationVersion.Identity(product, "Pengembangan", newBuild);
var previous = new ApplicationVersion.Identity("0.1.0", "Pengembangan", oldBuild);
Check(catalog is not null, "authoritative version equals current catalog");
Check(ReleaseNotes.Parse(raw, differentProduct) is null, "wrong selected product rejects notes");
Check(ReleaseNotes.Parse(null, product) is null, "legacy missing notes fallback");
Check(ReleaseNotes.Parse("broken", product) is null, "malformed JSON fallback");
Check(ReleaseNotes.Parse(new string(' ', 65537), product) is null, "byte limit");
var unknownSchema = System.Text.Json.Nodes.JsonNode.Parse(raw)!;
unknownSchema["Schema"] = 2;
Check(ReleaseNotes.Parse(unknownSchema.ToJsonString(), product) is null, "unknown schema");
var unknownStructure = System.Text.Json.Nodes.JsonNode.Parse(raw)!;
unknownStructure["Extra"] = 1;
Check(ReleaseNotes.Parse(unknownStructure.ToJsonString(), product) is null, "unknown structure");
var first = catalog!.Releases[0];
foreach (var invalid in new[] {
    catalog with { Releases = [] }, catalog with { Releases = [first, first] },
    catalog with { Releases = [first with { Date = "not-date" }] },
    catalog with { Releases = [first with { Summary = ["one"] }] },
    catalog with { Releases = [first with { Summary = ["a", "b", "private\ntext"] }] },
    catalog with { Releases = [first with { Fixes = [new string('a', 501)] }] },
    catalog with { Releases = [first with { Limitations = null! }] },
    catalog with { Releases = [first with { ProductVersion = "9.9.9" }] },
    catalog with { Releases = Enumerable.Repeat(first,25).ToArray() }
}) Check(ReleaseNotes.Parse(Json(invalid), product) is null, "negative structure/bounds " + report.Count);
var escaped = catalog with { Releases = [first with { Fixes = ["<script>literal</script> & \"quotes\""] }] };
Check(ReleaseNotes.Parse(Json(escaped), product)!.Releases[0].Fixes[0] == escaped.Releases[0].Fixes[0], "escaping roundtrip is inert text");
var empty = new ReleaseNotes.Viewed(1, []);
Check(!ReleaseNotes.ShouldShow(running, newBuild, new(null,null,null), empty, catalog), "clean install never claims updated");
Check(ReleaseNotes.ShouldShow(running, newBuild, previous, empty, catalog), "bootstrap successful upgrade");
Check(!ReleaseNotes.ShouldShow(previous, newBuild, previous, empty, catalog), "staged pointer cannot show new running version");
Check(!ReleaseNotes.ShouldShow(running, oldBuild, previous, empty, catalog), "failed or cancelled activation");
Check(!ReleaseNotes.ShouldShow(running, newBuild, running with { BuildId = oldBuild }, empty, catalog), "same product technical rebuild");
Check(!ReleaseNotes.ShouldShow(running, newBuild, previous, new(1,[product]), catalog), "reinstall and repeat startup seen once");
Check(!ReleaseNotes.ShouldShow(previous, oldBuild, running, new(1,[product]), catalog), "rollback no upgrade claim");
Check(ReleaseNotes.ShouldShow(running, newBuild, previous with { ProductVersion = "0.0.1" }, empty, catalog), "skipped releases only known current catalog");
Check(!ReleaseNotes.ShouldShow(running, newBuild, previous, empty, null), "missing notes not consumed by panel");
var ledger = Path.Combine(output,"state/viewed.json");
Check(ReleaseNotes.ReadViewed(ledger).Versions.Length == 0, "no writes before display success");
ReleaseNotes.MarkViewed(ledger, product); ReleaseNotes.MarkViewed(ledger, product);
Check(ReleaseNotes.ReadViewed(ledger).Versions.SequenceEqual(new[]{product}), "atomic persisted per-version dedup");
Check(!ReleaseNotes.ShouldShow(running,newBuild,previous,ReleaseNotes.ReadViewed(ledger),catalog), "displayed marker suppresses future panels");
File.WriteAllText(ledger,"broken"); Check(ReleaseNotes.ReadViewed(ledger).Versions.Length == 0,"malformed ledger recovery");
File.WriteAllText(ledger,new string(' ',8193)); Check(ReleaseNotes.ReadViewed(ledger).Versions.Length == 0,"bounded ledger");
var app = Path.Combine(output,"package/app"); Directory.CreateDirectory(Path.Combine(app,"Assets"));
File.Copy(source,Path.Combine(app,"Assets/release-notes.json"));
var bytes = File.ReadAllBytes(source); var hash = Convert.ToHexString(SHA256.HashData(bytes));
var manifest = new LocalAppUpdate.Manifest(newBuild,[new("Assets/release-notes.json",bytes.Length,hash)],product,"Pengembangan");
var local = ReleaseNotes.Available(manifest,Path.Combine(output,"package"),running);
var online = new ReleaseInfo(1,newBuild,DateTimeOffset.UtcNow,"development",$"PERTAMINA-OTO-Windows-x64-{newBuild}.zip",1,new string('A',64),first.Summary,ProductVersion:product,ReleaseNotesJson:raw,ReleaseNotesSha256:hash);
online.Validate();
Check(Json(local!) == Json(ReleaseNotes.Online(online)!),"exact local-online presenter model parity");
Check(ReleaseNotes.Online(online with { ReleaseNotesSha256 = new string('0',64) }) is null,"online hash mismatch honest fallback");
Check(ReleaseNotes.Online(online with { ProductVersion = differentProduct }) is null,"online chosen identity mismatch fallback");
Check(ReleaseNotes.Online(online with { ReleaseNotesJson = null }) is null,"legacy online compatible");
Check(Json(ReleaseNotes.Read(app,product)!) == Json(catalog),"offline history reads package only");
File.AppendAllText(Path.Combine(app,"Assets/release-notes.json")," ");
try { ReleaseNotes.Available(manifest,Path.Combine(output,"package"),running); throw new InvalidOperationException("tamper accepted"); }
catch (InvalidDataException) { report.Add("PASS local manifest binds notes bytes"); }
Check(ReleaseNotes.Read(output,product) is null,"missing offline file fallback");
// Real installer fixtures: selected release must remain identical across confirmation and install.
var feed = Path.Combine(output,"selected-feed"); var payload = Path.Combine(feed,newBuild,"app");
Directory.CreateDirectory(Path.Combine(payload,"worker"));
File.WriteAllText(Path.Combine(payload,"Pertamina.Oto.Desktop.App.exe"),"INERT fixture");
File.WriteAllText(Path.Combine(payload,"worker/Pertamina.Oto.LocalPdf.Worker.exe"),"INERT fixture");
var entries = Directory.GetFiles(payload,"*",SearchOption.AllDirectories).Select(p=>new LocalAppUpdate.Entry(Path.GetRelativePath(payload,p).Replace('\\','/'),new FileInfo(p).Length,Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))))).ToArray();
var manifestPath = Path.Combine(feed,newBuild,"manifest.json"); File.WriteAllText(manifestPath,Json(new LocalAppUpdate.Manifest(newBuild,entries)));
var manifestHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath)));
File.WriteAllText(Path.Combine(feed,"latest.json"),Json(new LocalAppUpdate.Release(newBuild,manifestHash)));
var install = Path.Combine(output,"selected-install");
foreach (var selected in new[]{(oldBuild,manifestHash),(newBuild,new string('0',64))})
{
    try { LocalAppUpdate.InstallSelected(install,feed,selected.Item1,selected.Item2); throw new InvalidOperationException("changed selection accepted"); }
    catch(InvalidDataException) { report.Add("PASS changed selected identity rejected before activation "+report.Count); }
    Check(!File.Exists(Path.Combine(install,"current.json")),"changed selection leaves pointer unchanged "+report.Count);
}
Check(LocalAppUpdate.InstallSelected(install,feed,newBuild,manifestHash)==newBuild,"unchanged selected package installs through existing protocol");
Check(Encoding.UTF8.GetByteCount(Json(online))<16384,"new catalog fits unchanged online metadata bound");
File.WriteAllLines(Path.Combine(output,"results.txt"),report); Console.WriteLine($"{report.Count} PASS");
