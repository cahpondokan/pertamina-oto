using Pertamina.Oto.Desktop.App;

internal static class InstallPackage
{
    internal static readonly string[] LauncherFiles = ["PERTAMINA-OTO.exe", "PERTAMINA-OTO.dll", "PERTAMINA-OTO.deps.json", "PERTAMINA-OTO.runtimeconfig.json"];
    private static readonly string[] UninstallerFiles = ["Uninstall-OTO.exe", "Uninstall-OTO.exe.config"];

    internal static void Validate(string package)
    {
        var feed = LocalAppUpdate.SafePath(package, "feed");
        var (release, manifest, source) = LocalAppUpdate.InspectFeed(feed);
        // Verify the entire input before copying a launcher or touching the active pointer.
        foreach (var entry in manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(source, "app/" + entry.Path), entry);
        LauncherUpdate.BundledSource(LocalAppUpdate.SafePath(source, "app"), manifest);
        var launchers = LocalAppUpdate.Read<LocalAppUpdate.Entry[]>(LocalAppUpdate.SafePath(package, "launcher-manifest.json"));
        if (File.Exists(LocalAppUpdate.SafePath(package, "launcher/restart-capability.json")))
        {
            var capability = LauncherUpdate.Validate(LocalAppUpdate.SafePath(package, "launcher"));
            if (!launchers.OrderBy(e => e.Path, StringComparer.Ordinal).SequenceEqual(capability.Files.OrderBy(e => e.Path, StringComparer.Ordinal)))
                throw new InvalidDataException("Manifest launcher tidak cocok.");
        }
        else if (launchers.Length != LauncherFiles.Length || !launchers.Select(e => e.Path).Order().SequenceEqual(LauncherFiles.Order()))
            throw new InvalidDataException("Launcher tidak lengkap.");
        foreach (var entry in launchers) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(package, "launcher/" + entry.Path), entry);
        ValidateUninstaller(package);
    }

    internal static bool ValidateUninstaller(string package)
    {
        var metadata = LocalAppUpdate.SafePath(package, "uninstall-manifest.json");
        var exe = LocalAppUpdate.SafePath(package, "uninstall/Uninstall-OTO.exe");
        var config = LocalAppUpdate.SafePath(package, "uninstall/Uninstall-OTO.exe.config");
        if (!File.Exists(metadata) && !File.Exists(exe) && !File.Exists(config)) return false; // Legacy package.
        var entries = LocalAppUpdate.Read<LocalAppUpdate.Entry[]>(metadata);
        if (entries.Length != 2 || !entries.Select(e => e.Path).Order(StringComparer.Ordinal).SequenceEqual(UninstallerFiles))
            throw new InvalidDataException("Paket uninstall tidak lengkap.");
        foreach (var entry in entries) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(package, "uninstall/" + entry.Path), entry);
        return true;
    }

    internal static void RegisterUninstall(string package)
    {
        if (!ValidateUninstaller(package)) throw new InvalidDataException("Paket uninstall tidak tersedia.");
        var exe = LocalAppUpdate.SafePath(package, "uninstall/Uninstall-OTO.exe");
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe, "--register")
        { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Path.GetDirectoryName(exe)! }) ?? throw new IOException("Pendaftaran uninstall gagal.");
        process.WaitForExit();
        if (process.ExitCode != 0) throw new IOException("Pendaftaran uninstall gagal.");
    }

    internal static string Install(string package, string destination)
    {
        Validate(package);
        if (File.Exists(LocalAppUpdate.SafePath(destination, "uninstall-pending.json")))
            throw new IOException("Uninstall sebelumnya belum selesai. Selesaikan uninstall sebelum memasang ulang OTO.");
        var feed = LocalAppUpdate.SafePath(package, "feed");
        var launchers = LocalAppUpdate.Read<LocalAppUpdate.Entry[]>(LocalAppUpdate.SafePath(package, "launcher-manifest.json"));
        _ = LocalAppUpdate.SafePath(destination, "setup.lock");
        Directory.CreateDirectory(destination);
        using var gate = new FileStream(LocalAppUpdate.SafePath(destination, "setup.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var (_, manifest, source) = LocalAppUpdate.InspectFeed(feed);
        // New packages install app + manifest-bound launcher in one rollback-protected transaction.
        if (LauncherUpdate.BundledSource(LocalAppUpdate.SafePath(source, "app"), manifest) is not null)
            return LocalAppUpdate.InstallFrom(destination, feed);
        // Legacy packages never replace an existing launcher with an older one.
        foreach (var name in LauncherFiles)
        {
            var target = LocalAppUpdate.SafePath(destination, name);
            if (File.Exists(target))
            {
                if (!File.Exists(LocalAppUpdate.SafePath(destination, "current.json")))
                    LocalAppUpdate.Verify(target, launchers.Single(e => e.Path == name));
                continue;
            }
            File.Copy(LocalAppUpdate.SafePath(package, "launcher/" + name), target, false);
        }
        return LocalAppUpdate.InstallFrom(destination, feed);
    }
}
