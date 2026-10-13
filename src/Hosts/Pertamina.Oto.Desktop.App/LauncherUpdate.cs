using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Pertamina.Oto.Desktop.App;

// Same-user, verified package maintenance. Caller owns maintenance then update locks.
// Never closes an app or changes current.json; the caller commits both changes together.
internal static class LauncherUpdate
{
    internal const string BundleFolder = "restart-launcher";
    internal const string CapabilityFile = "restart-capability.json";
    internal static readonly string[] Files = ["PERTAMINA-OTO.exe", "PERTAMINA-OTO.dll", "PERTAMINA-OTO.deps.json", "PERTAMINA-OTO.runtimeconfig.json"];
    private static readonly string[] RuntimeFiles = ["coreclr.dll", "hostfxr.dll", "hostpolicy.dll", "System.Private.CoreLib.dll"];
    internal sealed record Capability(int Protocol, LocalAppUpdate.Entry[] Files);

    // Protocol 2 adds a private .NET runtime, flat inside the stable launcher directory.
    // Never allow current.json, locks, arbitrary directories or application data as targets.
    internal static bool IsOwnedFile(string name) => Files.Contains(name, StringComparer.Ordinal) ||
        (!name.Contains('/') && !name.Contains('\\') && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) ||
        name is "createdump.exe" or "Microsoft.NETCore.App.deps.json" or "LICENSE.txt" or "LICENSE.TXT" or "THIRD-PARTY-NOTICES.TXT";

    internal static Capability Validate(string source)
    {
        var capability = LocalAppUpdate.Read<Capability>(LocalAppUpdate.SafePath(source, CapabilityFile));
        if (capability.Protocol is not (1 or 2) || capability.Files is not { Length: > 0 and <= 300 } ||
            capability.Files.Any(f => f is null || f.Path is null || !IsOwnedFile(f.Path)) ||
            capability.Files.Select(f => f.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != capability.Files.Length ||
            !Files.All(name => capability.Files.Any(f => f.Path == name)) ||
            (capability.Protocol == 1 && capability.Files.Length != 4) ||
            (capability.Protocol == 2 && !RuntimeFiles.All(name => capability.Files.Any(f => f.Path == name))) ||
            capability.Files.Sum(f => f.Length) > 256L * 1024 * 1024 ||
            capability.Files.Any(f => f.Length <= 0 || f.Length > 16 * 1024 * 1024 || f.Sha256 is null || !Regex.IsMatch(f.Sha256, "^[a-fA-F0-9]{64}$", RegexOptions.CultureInvariant)))
            throw new InvalidDataException("Paket launcher tidak lengkap atau tidak valid.");
        foreach (var entry in capability.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(source, entry.Path), entry);
        return capability;
    }

    internal static string? BundledSource(string app, LocalAppUpdate.Manifest manifest)
    {
        var entries = manifest.Files.Where(f => f.Path.StartsWith(BundleFolder + "/", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (entries.Length == 0) return null; // Legacy packages remain readable.
        // Exact names and manifest binding, not just a self-declared capability hash.
        var source = LocalAppUpdate.SafePath(app, BundleFolder);
        var capability = Validate(source);
        var expected = capability.Files.Select(f => f.Path).Append(CapabilityFile).Select(n => BundleFolder + "/" + n).ToArray();
        if (!entries.Select(f => f.Path).Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal)))
            throw new InvalidDataException("Manifest launcher tidak lengkap.");
        foreach (var entry in entries) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(app, entry.Path), entry);
        return source;
    }

    internal static Transaction? Begin(string root, string source, Action<int>? afterReplace = null, CancellationToken cancellation = default)
    {
        var capability = Validate(source);
        var names = capability.Files.Select(f => f.Path).Append(CapabilityFile).ToArray();
        foreach (var name in names) LocalAppUpdate.SafePath(root, name);
        if (Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar).Equals(Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Sumber launcher harus terpisah.");
        try
        {
            var installed = Validate(root);
            if (installed.Files.OrderBy(f => f.Path, StringComparer.Ordinal).SequenceEqual(capability.Files.OrderBy(f => f.Path, StringComparer.Ordinal))) return null;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or System.Text.Json.JsonException) { /* Legacy/mismatched launcher needs verified replacement. */ }
        var launcher = LocalAppUpdate.SafePath(root, Files[0]);
        foreach (var process in Process.GetProcessesByName("PERTAMINA-OTO"))
        {
            using (process)
            {
                try
                {
                    if (!process.HasExited && string.Equals(process.MainModule?.FileName, launcher, StringComparison.OrdinalIgnoreCase))
                        throw new IOException("Launcher masih digunakan; coba kembali setelah selesai.");
                }
                catch (InvalidOperationException) { /* Process exited during observation. */ }
            }
        }
        cancellation.ThrowIfCancellationRequested();
        // A missing capability must not recopy an unchanged private runtime (~80 MB).
        // Verify exact bytes before omitting a target; changed files still use the same
        // transactional backup/rollback and final full validation as a fresh install.
        var changed = new List<string>();
        foreach (var entry in capability.Files)
        {
            cancellation.ThrowIfCancellationRequested();
            try { LocalAppUpdate.Verify(LocalAppUpdate.SafePath(root, entry.Path), entry, cancellation); }
            catch (Exception error) when (error is IOException or InvalidDataException) { changed.Add(entry.Path); }
        }
        names = changed.Append(CapabilityFile).ToArray();
        var backup = LocalAppUpdate.SafePath(root, "launcher-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(backup, "before"));
        Directory.CreateDirectory(Path.Combine(backup, "after"));
        var transaction = new Transaction(root, backup);
        try
        {
            foreach (var name in names)
            {
                cancellation.ThrowIfCancellationRequested();
                var target = LocalAppUpdate.SafePath(root, name);
                if (Directory.Exists(target)) throw new IOException("Lokasi launcher bukan file.");
                if (File.Exists(target)) File.Copy(target, Path.Combine(backup, "before", name));
                File.Copy(LocalAppUpdate.SafePath(source, name), Path.Combine(backup, "after", name));
            }
            var stagedCapability = LocalAppUpdate.Read<Capability>(Path.Combine(backup, "after", CapabilityFile));
            if (stagedCapability.Protocol != capability.Protocol || stagedCapability.Files is null || !stagedCapability.Files.SequenceEqual(capability.Files))
                throw new InvalidDataException("Kapabilitas launcher berubah selama persiapan.");
            // Bind staged copies to the validated source capability too.
            foreach (var entry in capability.Files.Where(f => changed.Contains(f.Path, StringComparer.Ordinal))) LocalAppUpdate.Verify(Path.Combine(backup, "after", entry.Path), entry, cancellation);
            cancellation.ThrowIfCancellationRequested();
            foreach (var name in names)
            {
                var target = LocalAppUpdate.SafePath(root, name);
                var staged = Path.Combine(backup, "after", name);
                if (File.Exists(target)) File.Replace(staged, target, null); else File.Move(staged, target);
                transaction.Replaced.Add(name);
                afterReplace?.Invoke(transaction.Replaced.Count);
            }
            Validate(root);
            return transaction;
        }
        catch { transaction.Dispose(); throw; }
    }

    internal sealed class Transaction(string root, string backup) : IDisposable
    {
        internal List<string> Replaced { get; } = [];
        private bool committed;
        internal void Commit() => committed = true;
        public void Dispose()
        {
            if (committed) return;
            foreach (var name in Replaced.AsEnumerable().Reverse())
            {
                var target = LocalAppUpdate.SafePath(root, name);
                var saved = Path.Combine(backup, "before", name);
                if (File.Exists(saved))
                {
                    var pending = LocalAppUpdate.SafePath(root, name + ".rollback-" + Guid.NewGuid().ToString("N"));
                    File.Copy(saved, pending);
                    if (File.Exists(target)) File.Replace(pending, target, null); else File.Move(pending, target);
                }
                else File.Delete(target); // Only this transaction's newly created, validated owned file.
            }
            committed = true;
        }
    }
}
