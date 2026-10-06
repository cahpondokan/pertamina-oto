using Pertamina.Oto.Desktop.App;

internal static class InstallPackage
{
    internal static readonly string[] LauncherFiles = ["PERTAMINA-OTO.exe", "PERTAMINA-OTO.dll", "PERTAMINA-OTO.deps.json", "PERTAMINA-OTO.runtimeconfig.json"];

    internal static void Validate(string package)
    {
        var feed = LocalAppUpdate.SafePath(package, "feed");
        var (release, manifest, source) = LocalAppUpdate.InspectFeed(feed);
        // Verify the entire input before copying a launcher or touching the active pointer.
        foreach (var entry in manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(source, "app/" + entry.Path), entry);
        LauncherUpdate.BundledSource(LocalAppUpdate.SafePath(source, "app"), manifest);
        var launchers = LocalAppUpdate.Read<LocalAppUpdate.Entry[]>(LocalAppUpdate.SafePath(package, "launcher-manifest.json"));
        if (launchers.Length != LauncherFiles.Length || !launchers.Select(e => e.Path).Order().SequenceEqual(LauncherFiles.Order()))
            throw new InvalidDataException("Launcher tidak lengkap.");
        foreach (var entry in launchers) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(package, "launcher/" + entry.Path), entry);
    }

    internal static string Install(string package, string destination)
    {
        Validate(package);
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
