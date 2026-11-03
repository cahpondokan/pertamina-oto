using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Pertamina.Oto.Desktop.App;

// Current-user local IPC only. A PDF activation cannot issue commands or upload files.
// Version directories have distinct identities, so an update never forwards into old code.
internal sealed class ReaderActivation : IDisposable
{
    private readonly string pipeName;
    private readonly Mutex mutex;
    private readonly bool ownsMutex;
    private readonly CancellationTokenSource stopping = new();
    private Task? listener;
    internal bool IsPrimary => ownsMutex;
    internal Action<string>? Diagnostic { get; set; }

    internal ReaderActivation(string identity)
    {
        pipeName = "OTO.Reader." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..32];
        mutex = new Mutex(true, "Local\\" + pipeName, out ownsMutex);
    }

    internal static bool IsNormalLaunch(string[] args) => args.Length == 0 ||
        args.Length == 1 && ValidPath(args[0]) || args.Length == 2 && args[0] == "--open-reader" && ValidPath(args[1]);

    internal static string? LaunchPath(string[] args) => args.Length == 2 ? args[1] : args.Length == 1 ? args[0] : null;

    internal static bool ValidPath(string? path) => path is { Length: > 0 and <= 32760 } &&
        !path.Any(c => char.IsControl(c)) && Path.IsPathFullyQualified(path) &&
        !path.StartsWith(@"\\", StringComparison.Ordinal) && Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    internal void Start(Func<string?, Task<bool>> receive)
    {
        if (!ownsMutex || listener is not null) throw new InvalidOperationException();
        listener = ListenAsync(receive);
    }

    private async Task ListenAsync(Func<string?, Task<bool>> receive)
    {
        while (!stopping.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await server.WaitForConnectionAsync(stopping.Token).ConfigureAwait(false);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token);
                timeout.CancelAfter(TimeSpan.FromSeconds(8));
                var lengthBytes = new byte[4];
                await server.ReadExactlyAsync(lengthBytes, timeout.Token).ConfigureAwait(false);
                var length = BitConverter.ToInt32(lengthBytes);
                if (length is < 2 or > 131072) continue;
                var data = new byte[length];
                await server.ReadExactlyAsync(data, timeout.Token).ConfigureAwait(false);
                var request = JsonSerializer.Deserialize<Request>(data);
                var valid = request is { Version: 1 } && (request.Path is null || ValidPath(request.Path));
                var accepted = valid && await receive(request!.Path).ConfigureAwait(false);
                await server.WriteAsync(new byte[] { accepted ? (byte)1 : (byte)0 }, timeout.Token).ConfigureAwait(false);
            }
            catch (Exception error) when (error is IOException or OperationCanceledException or JsonException or UnauthorizedAccessException)
            { Diagnostic?.Invoke("receive:" + error.GetType().Name + ":" + error.HResult); }
        }
    }

    internal async Task<bool> ForwardAsync(string? path, CancellationToken cancellation = default)
    {
        if (path is not null && !ValidPath(path)) return false;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            await client.ConnectAsync(timeout.Token).ConfigureAwait(false);
            // Transfer this user-initiated launch's foreground permission to the
            // exact same-user receiver, never to every process (ASFW_ANY).
            if (GetNamedPipeServerProcessId(client.SafePipeHandle, out var receiverPid))
                _ = AllowSetForegroundWindow(receiverPid);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new Request(1, path));
            await client.WriteAsync(BitConverter.GetBytes(bytes.Length), timeout.Token).ConfigureAwait(false);
            await client.WriteAsync(bytes, timeout.Token).ConfigureAwait(false);
            var response = new byte[1]; await client.ReadExactlyAsync(response, timeout.Token).ConfigureAwait(false);
            return response[0] == 1;
        }
        catch (Exception error) when (error is IOException or OperationCanceledException or UnauthorizedAccessException)
        { Diagnostic?.Invoke("send:" + error.GetType().Name + ":" + error.HResult); return false; }
    }

    public void Dispose()
    {
        stopping.Cancel();
        if (ownsMutex) mutex.ReleaseMutex();
        mutex.Dispose();
        // Listener uses the token until its asynchronous pipe wait is cancelled.
    }

    private sealed record Request(int Version, string? Path);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint serverProcessId);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(uint processId);
}
