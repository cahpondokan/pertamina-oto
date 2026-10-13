using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Pertamina.Oto.Desktop.App;

// Same-user LOCAL handshake. No document paths, shell commands or kill operations.
internal static class RestartProtocol
{
    private static readonly string[] LauncherFiles = ["PERTAMINA-OTO.exe", "PERTAMINA-OTO.dll", "PERTAMINA-OTO.deps.json", "PERTAMINA-OTO.runtimeconfig.json"];
    private static void TraceFixture(string root, string stage)
    {
#if DEBUG
        if (Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1") File.AppendAllText(Path.Combine(root, "restart-fixture-stages-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture) + ".txt"), stage + Environment.NewLine);
#endif
    }
    internal sealed record Capability(int Protocol, LocalAppUpdate.Entry[] Files, int MaintenanceProtocol = 1);
    internal static string PipeName(string token)
    {
        if (!Guid.TryParseExact(token, "N", out _)) throw new InvalidDataException("Invalid restart token.");
        return "oto-restart-" + token;
    }
    internal static NamedPipeServerStream Server(string token) => new(PipeName(token), PipeDirection.InOut, 1,
        PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
    internal static StreamReader Reader(Stream pipe) => new(pipe, Encoding.UTF8, false, 1024, leaveOpen: true);
    internal static StreamWriter Writer(Stream pipe) => new(pipe, new UTF8Encoding(false), 1024, leaveOpen: true) { AutoFlush = true };
    internal static string ValidateLauncher(string root)
    {
        LauncherUpdate.Validate(root);
        return LocalAppUpdate.SafePath(root, "PERTAMINA-OTO.exe");
    }
    internal static void WriteCapability(string root)
    {
        var bundled = File.Exists(Path.Combine(root, "coreclr.dll"));
        var names = bundled ? Directory.GetFiles(root).Select(Path.GetFileName).OfType<string>().Where(LauncherUpdate.IsOwnedFile).Order(StringComparer.Ordinal).ToArray() : LauncherFiles;
        var entries = names
            .Select(name => { var file = LocalAppUpdate.SafePath(root, name); using var stream = File.OpenRead(file);
                return new LocalAppUpdate.Entry(name, stream.Length, Convert.ToHexString(SHA256.HashData(stream))); }).ToArray();
        File.WriteAllText(Path.Combine(root, "restart-capability.json"), System.Text.Json.JsonSerializer.Serialize(new Capability(bundled ? 2 : 1, entries)));
        LauncherUpdate.Validate(root);
    }
    internal sealed class Prepared(NamedPipeServerStream pipe, Process launcher, Process next) : IDisposable
    {
        internal bool NextRunning => !next.HasExited && !launcher.HasExited;
        internal async Task CommitCloseAsync()
        { using var writer = Writer(pipe); await writer.WriteLineAsync("EXIT"); }
        // Launcher proceeds only after this exact old process exits normally.
        public void Dispose() { pipe.Dispose(); launcher.Dispose(); next.Dispose(); }
    }
    internal static async Task<Prepared> PrepareAsync(string root, string expected, CancellationToken cancellation)
    {
        LocalAppUpdate.ValidateVersion(expected);
        using var maintenance = new FileStream(LocalAppUpdate.SafePath(root, "launcher-maintenance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        // Older updater binaries copy the new app bundle but cannot migrate the root launcher.
        // Repair only from the exact installed target, under the same lock ordering as updates.
        using (var gate = new FileStream(LocalAppUpdate.SafePath(root, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            if (LocalAppUpdate.VersionAt(root) != expected) throw new InvalidDataException("Update berubah; periksa kembali.");
            var nextDirectory = LocalAppUpdate.SafePath(root, "versions/" + expected);
            var bundle = LocalAppUpdate.SafePath(nextDirectory, LauncherUpdate.BundleFolder);
            if (Directory.Exists(bundle))
            {
                if (LocalAppUpdate.Read<LocalAppUpdate.Release>(LocalAppUpdate.SafePath(nextDirectory, "build-info.json")).Version != expected)
                    throw new InvalidDataException("Identitas aplikasi baru tidak cocok.");
                using var repair = LauncherUpdate.Begin(root, bundle, cancellation: cancellation);
                cancellation.ThrowIfCancellationRequested();
                repair?.Commit();
            }
        }
        var launcherPath = ValidateLauncher(root);
        if (LocalAppUpdate.VersionAt(root) != expected) throw new InvalidDataException("Update berubah; periksa kembali.");
        var token = Guid.NewGuid().ToString("N"); var pipe = Server(token); Process? launcher = null; Process? next = null;
        try
        {
            using var current = Process.GetCurrentProcess();
            var start = new ProcessStartInfo(launcherPath) { UseShellExecute = false, WorkingDirectory = root, CreateNoWindow = true };
            foreach (var value in new[] { "--restart", expected, token, current.Id.ToString(CultureInfo.InvariantCulture), current.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture) }) start.ArgumentList.Add(value);
            launcher = Process.Start(start) ?? throw new IOException("Launcher tidak dimulai.");
            await pipe.WaitForConnectionAsync(cancellation);
            using var reader = Reader(pipe);
            var line = await reader.ReadLineAsync(cancellation);
            TraceFixture(root, "old received startup reply");
            if (line is null || !line.StartsWith("READY ", StringComparison.Ordinal) || !int.TryParse(line.AsSpan(6), CultureInfo.InvariantCulture, out var id)) throw new IOException("Startup belum dikonfirmasi.");
            next = Process.GetProcessById(id);
            TraceFixture(root, "old found new process");
            var exe = LocalAppUpdate.SafePath(root, "versions/" + expected + "/Pertamina.Oto.Desktop.App.exe");
            if (next.HasExited) throw new IOException("Instance baru sudah keluar.");
            if (launcher.HasExited) throw new IOException("Launcher sudah keluar.");
            if (!string.Equals(next.MainModule?.FileName, exe, StringComparison.OrdinalIgnoreCase)) throw new IOException("Path instance baru tidak cocok.");
            TraceFixture(root, "old verified new process path and liveness");
            return new Prepared(pipe, launcher, next);
        }
        catch { pipe.Dispose(); launcher?.Dispose(); next?.Dispose(); throw; }
    }
    internal static async Task<bool> AcceptAsync(string token, string expected, string runningVersion)
    {
        try
        {
            LocalAppUpdate.ValidateVersion(expected);
            if (expected != runningVersion) return false;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            using var pipe = new NamedPipeClientStream(".", PipeName(token), PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            await pipe.ConnectAsync(timeout.Token);
            using var reader = Reader(pipe); using var writer = Writer(pipe);
            await writer.WriteLineAsync("READY " + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            return await reader.ReadLineAsync(timeout.Token) == "COMMIT";
        }
        catch (Exception error) when (error is IOException or OperationCanceledException or InvalidDataException or UnauthorizedAccessException) { return false; }
    }
    internal static async Task<int> LaunchAsync(string root, string expected, string token, int oldPid, long oldStarted)
    {
        using var old = Process.GetProcessById(oldPid);
        if (old.HasExited || old.StartTime.ToUniversalTime().Ticks != oldStarted) throw new InvalidDataException("Parent mismatch.");
        using var gate = new FileStream(Path.Combine(root, "update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        LocalAppUpdate.ValidateVersion(expected);
        if (LocalAppUpdate.VersionAt(root) != expected) throw new InvalidDataException("Version mismatch.");
        var exe = LocalAppUpdate.SafePath(root, "versions/" + expected + "/Pertamina.Oto.Desktop.App.exe");
        var info = LocalAppUpdate.Read<LocalAppUpdate.Release>(LocalAppUpdate.SafePath(root, "versions/" + expected + "/build-info.json"));
        if (info.Version != expected) throw new InvalidDataException("Build mismatch.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var parent = new NamedPipeClientStream(".", PipeName(token), PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        await parent.ConnectAsync(timeout.Token); // Never start a child without an available parent.
        TraceFixture(root, "launcher parent connected");
        var childToken = Guid.NewGuid().ToString("N"); using var childPipe = Server(childToken);
        var start = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exe)! };
        foreach (var value in new[] { "--restart-ready", childToken, expected }) start.ArgumentList.Add(value);
#if DEBUG
        if (Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1") start.ArgumentList.Add("--audit-restart-child");
#endif
        using var child = Process.Start(start) ?? throw new IOException("New instance failed.");
        TraceFixture(root, "launcher new started");
        var connection = childPipe.WaitForConnectionAsync(timeout.Token);
        if (await Task.WhenAny(connection, child.WaitForExitAsync(timeout.Token)) != connection) throw new IOException("New instance exited before startup.");
        await connection;
        TraceFixture(root, "launcher new connected");
        using var childReader = Reader(childPipe); using var childWriter = Writer(childPipe); using var parentWriter = Writer(parent); using var parentReader = Reader(parent);
        if (await childReader.ReadLineAsync(timeout.Token) != "READY " + child.Id.ToString(CultureInfo.InvariantCulture) || child.HasExited) throw new IOException("New startup not ready.");
        TraceFixture(root, "launcher new ready");
        await parentWriter.WriteLineAsync("READY " + child.Id.ToString(CultureInfo.InvariantCulture));
        if (await parentReader.ReadLineAsync(timeout.Token) != "EXIT") throw new IOException("Old session cancelled restart.");
        TraceFixture(root, "launcher old consent received");
        await old.WaitForExitAsync(timeout.Token);
        TraceFixture(root, "launcher old exited");
        if (child.HasExited) throw new IOException("New instance exited.");
        await childWriter.WriteLineAsync("COMMIT");
        return 0;
        // On failure/disconnect the new instance exits itself; the old one is never killed.
    }
}
