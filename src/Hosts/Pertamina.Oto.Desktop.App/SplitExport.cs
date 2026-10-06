using Pertamina.Oto.LocalPdf;

namespace Pertamina.Oto.Desktop.App;

internal sealed record SplitExportResult(bool Success, string Message, string[] Files, bool CleanupPending = false);

internal static class SplitExport
{
    internal static async Task<SplitExportResult> RunAsync(string source, string folder, string password, int[][] groups,
        CancellationToken token, Action<int, int>? progress = null)
    {
        string? stage = null;
        var cleanupPending = false;
        SplitExportResult result;
        try
        {
            source = Path.GetFullPath(source); folder = Path.GetFullPath(folder);
            if (source.StartsWith(@"\\", StringComparison.Ordinal) || folder.StartsWith(@"\\", StringComparison.Ordinal) || !Directory.Exists(folder))
                throw new IOException();
            if (groups.Length == 0 || groups.Any(group => group.Length == 0 || group.Any(page => page is < 1 or > 1000)) || groups.Sum(group => group.Length) > 1000)
                throw new ArgumentException("Kelompok halaman tidak valid.", nameof(groups));
            token.ThrowIfCancellationRequested();
            using var sourceLock = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (sourceLock.Length is <= 0 or > 104857600) throw new IOException();
            var stem = Path.GetFileNameWithoutExtension(source);
            if (stem.Length > 70) stem = stem[..70];
            stage = Path.Combine(folder, ".oto-split-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stage);
            var outputs = new List<string>(); long bytes = 0;
            for (var i = 0; i < groups.Length; i++)
            {
                token.ThrowIfCancellationRequested(); progress?.Invoke(i, groups.Length);
                var output = Path.Combine(stage, $"{stem}_bagian-{i + 1:D2}.pdf");
                var reply = await WorkerClient.RunAsync(new PdfRequest("organize", [source], output,
                    Password: password, Layout: groups[i].Select(page => new PagePlacement(0, page - 1)).ToArray()), token);
                if (!reply.Success) { token.ThrowIfCancellationRequested(); throw new SplitFailure(reply.Message); }
                if (reply.Pages != groups[i].Length) throw new IOException();
                bytes += reply.Bytes;
                if (bytes > 250L * 1024 * 1024) throw new IOException();
                outputs.Add(output);
            }
            token.ThrowIfCancellationRequested();
            string[] saved;
            if (groups.Length == 1)
            {
                var target = Unique(folder, stem + "_bagian-01", ".pdf");
                File.Move(outputs[0], target, false); saved = [target];
            }
            else
            {
                var target = Unique(folder, stem + "_pisah", "");
                Directory.Move(stage, target); stage = null;
                saved = outputs.Select(path => Path.Combine(target, Path.GetFileName(path))).ToArray();
            }
            progress?.Invoke(groups.Length, groups.Length);
            result = new(true, $"{groups.Length} PDF berhasil disimpan.", saved);
        }
        catch (OperationCanceledException) { result = new(false, "Dibatalkan. File asli tetap aman; tidak ada hasil parsial yang diterbitkan.", []); }
        catch (SplitFailure error) { result = new(false, error.Message, []); }
        catch (Exception) { result = new(false, "Pemisahan gagal. Periksa PDF, izin folder, dan ruang disk. File asli tidak diubah.", []); }
        finally
        {
            // Only this invocation's unique staging directory; never recurse or touch published results.
            if (stage is not null)
            {
                try { foreach (var path in Directory.GetFiles(stage)) File.Delete(path); Directory.Delete(stage); }
                catch (Exception) { cleanupPending = true; }
            }
        }
        return result with { CleanupPending = cleanupPending };
    }

    private static string Unique(string folder, string stem, string extension)
    {
        for (var i = 0; i < 10000; i++)
        {
            var path = Path.Combine(folder, stem + (i == 0 ? "" : $" ({i + 1})") + extension);
            if (!File.Exists(path) && !Directory.Exists(path)) return path;
        }
        throw new IOException();
    }
    private sealed class SplitFailure(string message) : Exception(message);
}
