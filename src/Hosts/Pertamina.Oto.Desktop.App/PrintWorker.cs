using System.Diagnostics;
using System.Text.Json;
using Windows.Data.Pdf;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

// Child process contains PDF renderer/GDI driver failure and blocking spooler calls.
internal static class PrintWorker
{
    internal sealed record Request(string Snapshot, string Printer, byte[] Mode, int[] Pages, int Copies, bool Actual, string? Output);
    internal sealed record Reply(bool Success, int Job, string Status);
    // Whitelist reasons; driver/exception messages can contain document or environment data.
    internal static string SafeFailureReason(Exception error) => error switch
    {
        UnauthorizedAccessException => "Akses cetak ditolak.",
        IOException => "Berkas sementara atau layanan cetak tidak tersedia.",
        InvalidOperationException => "Windows atau driver tidak menerima seluruh pengiriman.",
        _ => "Proses cetak berhenti tanpa konfirmasi antrean."
    };
    internal static async Task<int> SubmitAsync(Request request, Action<int> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var directory = Path.Combine(Path.GetDirectoryName(request.Snapshot)!, "job-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var input = Path.Combine(directory, "request.json"); File.WriteAllText(input, JsonSerializer.Serialize(request));
        var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = AppContext.BaseDirectory };
        start.ArgumentList.Add("--print-worker"); start.ArgumentList.Add(input);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Proses cetak tidak dapat dimulai.");
        var stopped = false;
        try
        {
            var watch = Stopwatch.StartNew(); var lastProgress = -1;
            while (!process.HasExited)
            {
                if ((token.IsCancellationRequested || watch.Elapsed > TimeSpan.FromMinutes(10)) && !stopped)
                {
                    stopped = true; File.WriteAllText(Path.Combine(directory, "cancel"), "cancel");
                    if (!await Task.Run(() => process.WaitForExit(5000)))
                    { CancelQueue(request.Printer, directory); process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); }
                }
                var state = Path.Combine(directory, "progress");
                if (File.Exists(state)) try { if (int.TryParse(File.ReadAllText(state), out var n) && n != lastProgress) { lastProgress = n; progress(n); } } catch (IOException) { }
                await Task.Delay(100, CancellationToken.None);
            }
            if (stopped) { CancelQueue(request.Printer, directory); throw new OperationCanceledException("Pengiriman dihentikan.", token); }
            var result = Path.Combine(directory, "reply.json");
            if (process.ExitCode != 0 || !File.Exists(result)) { CancelQueue(request.Printer, directory); throw new InvalidOperationException("Proses cetak berhenti (exit " + process.ExitCode + ", status " + (File.Exists(result) ? JsonSerializer.Deserialize<Reply>(File.ReadAllText(result))?.Status : "no-reply") + "). Periksa antrean Windows."); }
            var reply = JsonSerializer.Deserialize<Reply>(File.ReadAllText(result));
            if (reply is not { Success: true }) throw new InvalidOperationException("Printer menolak pekerjaan. Periksa antrean Windows.");
            return reply.Job;
        }
        finally
        {
            if (!process.HasExited) { CancelQueue(request.Printer, directory); process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); }
            foreach (var name in new[] { "request.json", "reply.json", "progress", "job", "cancel" }) try { File.Delete(Path.Combine(directory, name)); } catch (IOException) { }
            try { Directory.Delete(directory, false); } catch (IOException) { }
        }
    }
    private static void CancelQueue(string printer, string directory)
    {
        try { if (int.TryParse(File.ReadAllText(Path.Combine(directory, "job")), out var job) && job > 0) PrintDevice.CancelJob(printer, job); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { LocalDiagnostics.Record("print-cancel-queue", error); }
    }
    internal static async Task<int> RunAsync(string input)
    {
        string? directory = null;
        try
        {
            if (new FileInfo(input).Length > 2 * 1024 * 1024) return 1;
            directory = Path.GetDirectoryName(Path.GetFullPath(input))!;
            var request = JsonSerializer.Deserialize<Request>(File.ReadAllText(input)) ?? throw new InvalidOperationException();
            if (request.Pages.Length is < 1 or > 10000 || request.Copies is < 1 or > 99 || request.Mode.Length is < 220 or > 1024 * 1024) return 1;
            // Hold immutable snapshot throughout native rendering, separately from the parent source lock.
            using var snapshotLock = new FileStream(request.Snapshot, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (snapshotLock.Length > 512L * 1024 * 1024) return 1;
            if (await PrintSecurity.HasEncryptionAsync(request.Snapshot) || request.Output is not null && File.Exists(request.Output)) throw new InvalidOperationException("Print input/output rejected.");
            var pdf = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(request.Snapshot));
            if (pdf.IsPasswordProtected || request.Pages.Any(p => p < 0 || p >= pdf.PageCount)) return 1;
            using var device = new PrintDevice(request.Printer); device.ImportMode(request.Mode);
            using var cancellation = new CancellationTokenSource();
            using var timer = new Timer(_ => { if (File.Exists(Path.Combine(directory, "cancel"))) cancellation.Cancel(); }, null, 0, 100);
            var job = await PrintPlan.SendAsync(pdf, device, request.Pages, request.Copies, request.Actual,
                n => File.WriteAllText(Path.Combine(directory, "progress"), n.ToString(System.Globalization.CultureInfo.InvariantCulture)), request.Output, cancellation.Token,
                id => File.WriteAllText(Path.Combine(directory, "job"), id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            File.WriteAllText(Path.Combine(directory, "reply.json"), JsonSerializer.Serialize(new Reply(true, job, "queued"))); return 0;
        }
        catch (Exception error) { LocalDiagnostics.Record("print-worker", error); if (directory is not null) File.WriteAllText(Path.Combine(directory, "reply.json"), JsonSerializer.Serialize(new Reply(false, 0, error.GetType().Name + ":" + error.HResult.ToString("X8", System.Globalization.CultureInfo.InvariantCulture)))); return 1; }
    }
}
