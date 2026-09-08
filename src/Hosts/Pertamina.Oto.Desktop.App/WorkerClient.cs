using System.Diagnostics;
using System.Text.Json;
using Pertamina.Oto.LocalPdf;

namespace Pertamina.Oto.Desktop.App;

internal static class WorkerClient
{
    internal static async Task<PdfReply> RunAsync(PdfRequest request, CancellationToken cancellationToken)
    {
        var finalPath = Path.GetFullPath(request.Output);
        if (File.Exists(finalPath) || Directory.Exists(finalPath))
            return new(false, "Nama hasil sudah ada. Pilih nama baru; aplikasi tidak menimpa file.");
        if (request.Inputs.Any(input => string.Equals(Path.GetFullPath(input), finalPath, StringComparison.OrdinalIgnoreCase)))
            return new(false, "Lokasi hasil tidak boleh sama dengan file sumber.");
        var stage = Path.Combine(Path.GetDirectoryName(finalPath)!, ".oto-" + Guid.NewGuid().ToString("N") + ".tmp");
        var sources = new List<FileStream>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        var token = timeout.Token;
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "worker", "Pertamina.Oto.LocalPdf.Worker.exe"),
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = AppContext.BaseDirectory
            }
        };
        try
        {
            // FileShare.Read denies writes/deletes, including aliases, throughout processing/publication.
            foreach (var input in request.Inputs.Distinct(StringComparer.OrdinalIgnoreCase))
                sources.Add(new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read));
            if (sources.Sum(s => s.Length) > 100L * 1024 * 1024)
                return new(false, "Total file dibatasi 100 MB pada versi pengembangan.");
            token.ThrowIfCancellationRequested();
            if (!process.Start()) return new(false, "Mesin lokal gagal dimulai.");
            using var kill = token.Register(() => Stop(process));
            // Passwords go through the anonymous pipe, never command-line arguments/log files.
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request with { Output = stage }).AsMemory(), token);
            process.StandardInput.Close();
            var response = ReadBoundedAsync(process.StandardOutput, token);
            var errors = DrainAsync(process.StandardError, token);
            while (!process.HasExited)
            {
                if (process.WorkingSet64 > 1024L * 1024 * 1024)
                {
                    Stop(process);
                    return new(false, "Proses dihentikan karena melewati batas memori versi pengembangan.");
                }
                await Task.Delay(100, token);
                process.Refresh();
            }
            await errors;
            var reply = JsonSerializer.Deserialize<PdfReply>(await response);
            if (process.ExitCode != 0 || reply is not { Success: true })
                return reply is { Success: false } ? reply : new(false, "Mesin PDF berhenti; sumber tidak diubah.");
            var output = new FileInfo(stage);
            if (!output.Exists || output.Length <= 0 || output.Length != reply.Bytes || output.Length > 250L * 1024 * 1024)
                return new(false, "Hasil tidak memenuhi validasi ukuran; tidak diterbitkan.");
            token.ThrowIfCancellationRequested();
            File.Move(stage, finalPath, false); // Atomic new-name publication, on the same volume.
            // Cancellation after a successful move must not fabricate a cancelled outcome.
            return reply;
        }
        catch (OperationCanceledException)
        {
            Stop(process);
            return new(false, cancellationToken.IsCancellationRequested
                ? "Proses dibatalkan. Sumber tidak diubah."
                : "Proses melewati batas 2 menit dan dihentikan. Sumber tidak diubah.");
        }
        catch (Exception)
        {
            Stop(process);
            return new(false, "Proses gagal. Periksa akses file, ruang disk, dan nama hasil. Sumber tidak diubah.");
        }
        finally
        {
            Stop(process);
            foreach (var source in sources) source.Dispose();
            try { if (File.Exists(stage)) File.Delete(stage); }
            catch (IOException) { /* Only the unique stage can remain; never delete final/source. */ }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void Stop(Process process)
    {
        try { if (!process.HasExited) { process.Kill(true); process.WaitForExit(3000); } }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception) { }
    }

    private static async Task<string> ReadBoundedAsync(StreamReader reader, CancellationToken token)
    {
        var buffer = new char[8192];
        var count = 0;
        while (count < buffer.Length)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(count), token);
            if (read == 0) return new string(buffer, 0, count);
            count += read;
        }
        throw new InvalidDataException("Worker response exceeded limit.");
    }

    private static async Task DrainAsync(StreamReader reader, CancellationToken token)
    {
        var buffer = new char[1024];
        while (await reader.ReadAsync(buffer, token) != 0) { }
    }
}
