using System.Diagnostics;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;

if (args.Length == 3 && args[0] == "--restart-ready")
{
    var mode = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixture-mode.txt"));
    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "starts.txt"), "started\n");
    if (mode == "crash") return 1;
    var accepted = await RestartProtocol.AcceptAsync(args[1], args[2], args[2]);
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "result.txt"), accepted ? "COMMIT" : "CANCEL");
    return accepted ? 0 : 1;
}
if (args.Length == 4 && args[0] == "--fixture-parent")
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
    try
    {
        using var ready = await RestartProtocol.PrepareAsync(args[1], args[2], timeout.Token);
        if (!ready.NextRunning) return 2;
        File.WriteAllText(Path.Combine(args[1], "old-ready.txt"), "new running before old exit");
        if (args[3] == "cancel") { File.WriteAllText(Path.Combine(args[1], "old-preserved.txt"), "draft retained"); return 0; }
        await ready.CommitCloseAsync();
        File.WriteAllText(Path.Combine(args[1], "old-exit.txt"), "orderly exit"); return 0;
    }
    catch (Exception) { File.WriteAllText(Path.Combine(args[1], "old-preserved.txt"), "work retained on failure"); return args[3] == "fail" ? 0 : 1; }
}
if (args.Length != 2) return 2;
var evidence = Path.GetFullPath(args[0]); var launcherOutput = Path.GetFullPath(args[1]); Directory.CreateDirectory(evidence);
const string version = "20261001-110100-bbbbbbbb";
var report = new List<string>();
void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); report.Add("PASS " + label); }
async Task Reject(Func<Task> action, string label) { try { await action(); } catch (Exception) { report.Add("PASS " + label); return; } throw new InvalidOperationException(label); }
string Fixture(string name, string mode)
{
    var root = Path.Combine(evidence, name + " space"); Directory.CreateDirectory(root);
    foreach (var path in Directory.GetFiles(launcherOutput, "PERTAMINA-OTO.*")) File.Copy(path, Path.Combine(root, Path.GetFileName(path)));
    RestartProtocol.WriteCapability(root);
    var target = Path.Combine(root, "versions", version); Directory.CreateDirectory(target);
    foreach (var path in Directory.GetFiles(AppContext.BaseDirectory, "RestartChecks.*")) File.Copy(path, Path.Combine(target, Path.GetFileName(path)));
    File.Copy(Path.Combine(AppContext.BaseDirectory, "RestartChecks.exe"), Path.Combine(target, "Pertamina.Oto.Desktop.App.exe"));
    File.WriteAllText(Path.Combine(root, "current.json"), JsonSerializer.Serialize(new LocalAppUpdate.Release(version, "")));
    File.WriteAllText(Path.Combine(target, "build-info.json"), JsonSerializer.Serialize(new LocalAppUpdate.Release(version, "")));
    File.WriteAllText(Path.Combine(target, "fixture-mode.txt"), mode); return root;
}
async Task Parent(string root, string mode)
{
    var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "RestartChecks.exe")) { UseShellExecute = false, CreateNoWindow = true };
    foreach (var arg in new[] { "--fixture-parent", root, version, mode }) start.ArgumentList.Add(arg);
    using var process = Process.Start(start)!; using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    await process.WaitForExitAsync(timeout.Token); Check(process.ExitCode == 0, "isolated parent " + mode);
}
try
{
    await Reject(() => RestartProtocol.PrepareAsync(evidence, version, CancellationToken.None), "missing launcher capability blocks before launch");
    var tamper = Fixture("tamper", "ready"); File.AppendAllText(Path.Combine(tamper, "PERTAMINA-OTO.dll"), "tamper");
    await Reject(() => RestartProtocol.PrepareAsync(tamper, version, CancellationToken.None), "changed launcher rejected");
    var mismatch = Fixture("mismatch", "ready"); File.WriteAllText(Path.Combine(mismatch, "current.json"), JsonSerializer.Serialize(new LocalAppUpdate.Release("20261001-110200-cccccccc", "")));
    await Reject(() => RestartProtocol.PrepareAsync(mismatch, version, CancellationToken.None), "changed pointer rejected before launch");
    Check(!await RestartProtocol.AcceptAsync(Guid.NewGuid().ToString("N"), version, "20261001-110000-aaaaaaaa"), "new running build must match requested version");
    await Reject(() => Task.FromResult(RestartProtocol.PipeName("../invalid")), "invalid pipe token rejected");
    var good = Fixture("success", "ready"); await Parent(good, "success");
    var target = Path.Combine(good, "versions", version);
    for (var i = 0; i < 100 && !File.Exists(Path.Combine(target, "result.txt")); i++) await Task.Delay(50);
    Check(File.ReadAllText(Path.Combine(target, "result.txt")) == "COMMIT", "new fixture committed only after exact old process exited");
    Check(File.Exists(Path.Combine(good, "old-ready.txt")) && File.Exists(Path.Combine(good, "old-exit.txt")), "old exited orderly after new startup acknowledgement");
    Check(File.ReadAllLines(Path.Combine(target, "starts.txt")).Length == 1, "exactly one new instance with space-containing paths");
    var migrated = Fixture("legacy-repair", "ready");
    var bundle = Path.Combine(migrated, "versions", version, LauncherUpdate.BundleFolder); Directory.CreateDirectory(bundle);
    foreach (var name in LauncherUpdate.Files.Append(LauncherUpdate.CapabilityFile)) File.Copy(Path.Combine(migrated, name), Path.Combine(bundle, name));
    File.Delete(Path.Combine(migrated, LauncherUpdate.CapabilityFile));
    await Parent(migrated, "success");
    LauncherUpdate.Validate(migrated);
    var migratedResult = Path.Combine(migrated, "versions", version, "result.txt");
    for (var i = 0; i < 100 && !File.Exists(migratedResult); i++) await Task.Delay(50);
    Check(File.ReadAllText(migratedResult) == "COMMIT", "legacy update transition repairs launcher from exact installed bundle and restarts");
    var cancelled = Fixture("cancel", "ready"); await Parent(cancelled, "cancel");
    target = Path.Combine(cancelled, "versions", version);
    for (var i = 0; i < 100 && !File.Exists(Path.Combine(target, "result.txt")); i++) await Task.Delay(50);
    Check(File.ReadAllText(Path.Combine(target, "result.txt")) == "CANCEL" && File.Exists(Path.Combine(cancelled, "old-preserved.txt")), "cancel after readiness preserves old work and new exits itself");
    var crash = Fixture("crash", "crash"); await Parent(crash, "fail");
    Check(File.Exists(Path.Combine(crash, "old-preserved.txt")) && !File.Exists(Path.Combine(crash, "old-exit.txt")), "failed new startup never requests old exit");
    var locked = Fixture("locked", "ready");
    using (var gate = new FileStream(Path.Combine(locked, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) await Parent(locked, "fail");
    Check(!File.Exists(Path.Combine(locked, "versions", version, "starts.txt")), "update writer lock blocks restart child");
}
catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); }
File.WriteAllLines(Path.Combine(evidence, "test-results.txt"), report);
return report.Any(r => r.StartsWith("FAIL", StringComparison.Ordinal)) ? 1 : 0;
