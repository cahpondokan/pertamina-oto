using System.Text.RegularExpressions;

namespace Pertamina.Oto.Desktop.App;

// Product identity is optional on legacy packages. Never infer it from current/latest.
internal static class ApplicationVersion
{
    internal sealed record Identity(string? ProductVersion, string? Channel, string? BuildId)
    {
        internal bool Known => ValidProduct(ProductVersion) && ValidChannel(Channel);
        internal string Display => Known ? ProductVersion + " · " + Channel
            : ValidBuild(BuildId) ? "Versi produk belum tercatat · Build " + BuildId : "Build pengembangan";
    }
    internal static bool ValidProduct(string? value) => value is not null && Regex.IsMatch(value, @"^(0|[1-9][0-9]{0,5})\.(0|[1-9][0-9]{0,5})\.(0|[1-9][0-9]{0,5})$", RegexOptions.CultureInvariant);
    internal static bool ValidChannel(string? value) => value is "Pengembangan" or "Stabil";
    internal static bool ValidBuild(string? value) => value is not null && Regex.IsMatch(value, "^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$", RegexOptions.CultureInvariant);
    internal static Identity ReadDirectory(string directory, string? expectedBuild = null)
    {
        try
        {
            var info = LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(directory, "build-info.json"));
            if (!ValidBuild(info.Version) || expectedBuild is not null && info.Version != expectedBuild) return new(null, null, expectedBuild);
            return new(info.ProductVersion, info.Channel, info.Version);
        }
        catch (Exception) { return new(null, null, expectedBuild); }
    }
    internal static Identity Running()
    {
        var packaged = ReadDirectory(AppContext.BaseDirectory);
        // Embedded assembly metadata identifies this binary even before packaging.
        var attributes = typeof(ApplicationVersion).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
            .Cast<System.Reflection.AssemblyMetadataAttribute>().ToArray();
        return new(attributes.FirstOrDefault(a => a.Key == "OtoProductVersion")?.Value,
            attributes.FirstOrDefault(a => a.Key == "OtoChannel")?.Value, packaged.BuildId);
    }
    internal static Identity Available(LocalAppUpdate.Manifest manifest, string package)
    {
        var entry = manifest.Files.FirstOrDefault(f => f.Path == "build-info.json");
        if (entry is null) return new(null, null, manifest.Version);
        LocalAppUpdate.Verify(LocalAppUpdate.SafePath(package, "app/build-info.json"), entry);
        return ReadDirectory(Path.Combine(package, "app"), manifest.Version);
    }
    internal static string UpdateDescription(Identity running, Identity next) => running.Known && next.Known && running.ProductVersion == next.ProductVersion
        ? "Pembaruan build untuk versi produk yang sama: " + next.Display + "."
        : "Pembaruan tersedia: " + next.Display + ".";
}
