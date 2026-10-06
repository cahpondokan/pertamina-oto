using System.Diagnostics;
using Pertamina.Oto.Desktop.App;

try
{
    var root = AppContext.BaseDirectory;
    if (args.Length == 2 && args[0] == "--maintain-launcher")
    {
        var destination = Path.GetFullPath(args[1]);
        LocalAppUpdate.ValidateVersion(LocalAppUpdate.VersionAt(destination));
        using var maintenance = new FileStream(LocalAppUpdate.SafePath(destination, "launcher-maintenance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        using var gate = new FileStream(LocalAppUpdate.SafePath(destination, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        using var update = LauncherUpdate.Begin(destination, root);
        update?.Commit();
        return 0;
    }
    if (args.Length == 1 && args[0] == "--write-restart-capability") { RestartProtocol.WriteCapability(root); return 0; }
    if (args.Length == 5 && args[0] == "--restart")
        return await RestartProtocol.LaunchAsync(root, args[1], args[2], int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture), long.Parse(args[4], System.Globalization.CultureInfo.InvariantCulture));
    if (args.Length != 0 && args[0] == "--restart") return 1;
    if (args.Length == 2 && args[0] == "--self-test") return UpdateTests.Run(Path.GetFullPath(args[1]));
    if (args.Length == 2 && args[0] == "--install") { LocalAppUpdate.Install(Path.GetFullPath(args[1])); return 0; }
    var version = LocalAppUpdate.VersionAt(root); LocalAppUpdate.ValidateVersion(version);
    var exe = LocalAppUpdate.SafePath(root, "versions/" + version + "/Pertamina.Oto.Desktop.App.exe");
    var start = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exe)! };
    Process.Start(start);
    return 0;
}
catch (Exception error)
{
#if DEBUG
    if (Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1") File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "launcher-fixture-error.txt"), error.GetType().Name + " " + error.HResult.ToString("X8", System.Globalization.CultureInfo.InvariantCulture));
#endif
    // No document names or exception details in launcher diagnostics.
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "launcher-error.txt"), "Aplikasi belum dapat dibuka. Periksa current.json dan folder versions. Versi sebelumnya tersedia melalui previous.json. Kode: " + error.HResult.ToString("X8", System.Globalization.CultureInfo.InvariantCulture));
    return 1;
}
