using Pertamina.Oto.Desktop.App;

var root = Path.GetFullPath(args[0]);
Directory.CreateDirectory(root);
var lines = new List<string>();
void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); lines.Add("PASS " + label); }
var app = Path.Combine(root, "fixture", "app");
Directory.CreateDirectory(app);
var marker = Path.Combine(app, PortableProfile.MarkerName);
File.WriteAllText(marker, "{\"schema\":1,\"dataFolder\":\"Data\"}");
Check(PortableProfile.ResolveDataRoot(app) == Path.Combine(root, "fixture", "Data"), "profile belongs to extracted package");
foreach (var value in new[] { "{", "{}", "[]", "{\"schema\":2147483648,\"dataFolder\":\"Data\"}", "{\"schema\":\"1\",\"dataFolder\":\"Data\"}", "{\"schema\":2,\"dataFolder\":\"Data\"}", "{\"schema\":1,\"dataFolder\":\"../escape\"}" })
{
    File.WriteAllText(marker, value);
    try { PortableProfile.ResolveDataRoot(app); Check(false, "invalid marker rejected"); }
    catch (Exception error) when (error is InvalidDataException or System.Text.Json.JsonException) { Check(true, "invalid marker rejected " + lines.Count); }
}
File.Delete(marker); Directory.CreateDirectory(marker);
try { PortableProfile.ResolveDataRoot(app); Check(false, "directory marker rejected"); }
catch (Exception error) when (error is IOException or UnauthorizedAccessException) { Check(true, "directory marker rejected"); }
// An inert linked updater uses its own process directory, never the user's install.
var processMarker = Path.Combine(AppContext.BaseDirectory, PortableProfile.MarkerName);
if (File.Exists(processMarker) || Directory.Exists(processMarker)) throw new InvalidOperationException("Test output must start clean.");
Check(!PortableProfile.IsPortable, "installed mode unchanged without marker");
File.WriteAllText(processMarker, "{\"schema\":1,\"dataFolder\":\"Data\"}");
try
{
    Check(PortableProfile.IsPortable, "direct executable marker recognized");
    PortableProfile.ValidateStartup();
    Check(Directory.Exists(PortableProfile.DataRoot), "writable isolated Data startup");
    var target = Path.Combine(root, "must-not-install");
    try { LocalAppUpdate.InstallFrom(target, "unused"); Check(false, "portable install blocked"); }
    catch (InvalidOperationException) { Check(!Directory.Exists(target), "portable install blocked before mutation"); }
}
finally { File.Delete(processMarker); }
File.WriteAllLines(Path.Combine(root, "profile-results.txt"), lines);
Console.WriteLine(string.Join(Environment.NewLine, lines));
