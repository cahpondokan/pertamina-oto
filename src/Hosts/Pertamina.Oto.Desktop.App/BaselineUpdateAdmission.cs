using System.Text.Json;

namespace Pertamina.Oto.Desktop.App;

internal static class BaselineUpdateAdmission
{
    internal static void Validate(string app, LocalAppUpdate.Manifest manifest)
    {
        var launcher = LauncherUpdate.BundledSource(app, manifest);
        if (launcher is null || LauncherUpdate.Validate(launcher).Protocol != 2)
            throw new InvalidDataException("Baseline memerlukan launcher runtime private protokol 2.");
        foreach (var prefix in new[] { "", "worker/", "word-worker/", "pdf-opener/", "restart-launcher/" })
            foreach (var name in new[] { "coreclr.dll", "hostfxr.dll", "hostpolicy.dll", "System.Private.CoreLib.dll" })
                if (!manifest.Files.Any(entry => entry.Path == prefix + name)) throw new InvalidDataException("Manifest runtime private tidak lengkap.");
        foreach (var path in new[] { "Pertamina.Oto.Desktop.App.runtimeconfig.json", "worker/Pertamina.Oto.LocalPdf.Worker.runtimeconfig.json", "word-worker/Pertamina.Oto.WordPdf.Worker.runtimeconfig.json", "pdf-opener/PertaminaOTO.PdfOpener.runtimeconfig.json", "restart-launcher/PERTAMINA-OTO.runtimeconfig.json" })
        {
            var entry = manifest.Files.SingleOrDefault(file => file.Path == path) ?? throw new InvalidDataException("Runtime contract missing.");
            LocalAppUpdate.Verify(LocalAppUpdate.SafePath(app, path), entry);
            if (entry.Length > 16384) throw new InvalidDataException("Runtime contract too large.");
            using var config = JsonDocument.Parse(File.ReadAllText(LocalAppUpdate.SafePath(app, path)));
            var options = config.RootElement.GetProperty("runtimeOptions");
            if (options.TryGetProperty("framework", out _) || options.TryGetProperty("frameworks", out _)
                || !options.TryGetProperty("includedFrameworks", out var included)
                || !included.EnumerateArray().Any(item => item.GetProperty("name").GetString() == "Microsoft.NETCore.App"
                    && System.Version.TryParse(item.GetProperty("version").GetString(), out var runtime) && runtime.Major == 10 && runtime.Minor == 0 && runtime.Build >= 12))
                throw new InvalidDataException("Paket baseline bergantung pada runtime global atau runtime belum diakui.");
        }
    }
}
