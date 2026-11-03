using System.Text;
using System.Text.Json;
using Oto.WebDiagnostics;

namespace Oto.Monitor;

internal sealed class MonitorData(string directory)
{
    internal const long Capacity = 16 * 1024 * 1024;
    private readonly object gate = new();
    private DateTime lastWrite;
    private long size = -1;
    private List<StoredDiagnostic> rows = [];
    private int rejected;
    private int duplicates;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal IResult Overview(int days, string feature, string outcome, string process, int page)
    {
        if ((feature.Length > 0 && !DiagnosticsContract.Features.Contains(feature)) || (outcome is not "" and not "errors" and not "success" and not "cancelled") ||
            (process.Length > 0 && !Guid.TryParseExact(process, "D", out _))) return Results.BadRequest();
        lock (gate)
        {
            var missing = false;
            try
            {
                var path = Path.Combine(directory, "events.jsonl"); PrivateFiles.Check(path);
                var info = new FileInfo(path);
                if (!info.Exists) { rows = []; size = 0; rejected = 0; duplicates = 0; missing = true; lastWrite = default; }
                else if (info.Length > Capacity) return Results.Json(new { code = "CAPACITY", message = "Kapasitas log terlampaui. Pengelola perlu memeriksa penyimpanan." }, statusCode: 503);
                else if (info.Length != size || info.LastWriteTimeUtc != lastWrite)
                {
                    var snapshotLength = info.Length;
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var bytes = new byte[(int)snapshotLength]; stream.ReadExactly(bytes);
                    var lines = Encoding.UTF8.GetString(bytes).Split('\n');
                    List<StoredDiagnostic> parsed = []; HashSet<string> seen = new(StringComparer.Ordinal); var bad = 0; var dup = 0;
                    // An in-flight tail is never parsed or shown; the next refresh retries it.
                    for (var i = 0; i < lines.Length - 1; i++)
                    {
                        var row = ParseRow(lines[i]);
                        if (row is null) { bad++; continue; }
                        if (!seen.Add(row.Data.Id)) { dup++; continue; }
                        parsed.Add(row);
                        if (parsed.Count > 32768) return Results.Json(new { code = "CAPACITY", message = "Batas jumlah catatan terlampaui." }, statusCode: 503);
                    }
                    if (lines[^1].Length > 0) bad++;
                    rows = parsed; rejected = bad; duplicates = dup; size = snapshotLength; lastWrite = info.LastWriteTimeUtc;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { return Results.Json(new { code = "STORAGE", message = "Penyimpanan log belum dapat dibaca. Data sebelumnya mungkin sudah tidak mutakhir." }, statusCode: 503); }
            var now = DateTimeOffset.UtcNow;
            var selected = rows.Where(row => (days == 0 || row.ReceivedAtUtc >= now.AddDays(-days)) && (feature.Length == 0 || row.Data.Feature == feature) && (process.Length == 0 || row.Data.ProcessId == process)).ToList();
            var totalBeforeOutcome = selected.Count;
            selected = selected.Where(row => outcome switch {
                "errors" => IsError(row.Data.Event), "success" => row.Data.Event is "succeeded" or "save_succeeded",
                "cancelled" => row.Data.Event is "cancelled" or "save_cancelled", _ => true
            }).OrderByDescending(row => row.ReceivedAtUtc).ThenByDescending(row => row.Data.Id, StringComparer.Ordinal).ToList();
            var errors = selected.Where(row => IsError(row.Data.Event)).GroupBy(row => new { row.Data.Feature, row.Data.Stage, row.Data.Code, Release = row.ServerVersion, row.Data.Browser })
                .Select(group => new { group.Key.Feature, group.Key.Stage, group.Key.Code, group.Key.Release, group.Key.Browser, Count = group.Count() }).OrderByDescending(group => group.Count).Take(100);
            var pending = selected.GroupBy(row => new { row.Data.ProcessId, row.Data.Stage }).Count(group => group.Any(row => row.Data.Event == "started") && !group.Any(row => row.Data.Event is "succeeded" or "failed" or "cancelled"));
            var pages = Math.Max(1, (selected.Count + 49) / 50); page = Math.Min(page, pages);
            var last = rows.Count > 0 ? rows.Max(row => row.ReceivedAtUtc) : (DateTimeOffset?)null;
            var buckets = Enumerable.Range(0, 24).Select(index => {
                var end = now.AddHours(index - 23); var start = end.AddHours(-1);
                return new { start, events = selected.Count(row => row.ReceivedAtUtc >= start && row.ReceivedAtUtc < end), errors = selected.Count(row => row.ReceivedAtUtc >= start && row.ReceivedAtUtc < end && IsError(row.Data.Event)) };
            });
            return Results.Json(new {
                generatedAtUtc = now, lastReceivedAtUtc = last, state = missing ? "waiting" : "readable", stale = last is null || now - last > TimeSpan.FromMinutes(15),
                storageBytes = Math.Max(0, size), capacityBytes = Capacity, recordCount = rows.Count, recordLimit = 32768,
                capacityWarning = size >= Capacity * 0.8 || rows.Count >= 26214, rejectedRows = rejected, duplicateRows = duplicates,
                events = selected.Count, totalBeforeOutcome,
                successfulProcesses = selected.Count(row => row.Data.Event == "succeeded" && row.Data.Stage == "process"),
                errorEvents = selected.Count(row => IsError(row.Data.Event)), startedWithoutTerminal = pending,
                downloadsRequested = selected.Count(row => row.Data.Event == "download_requested"),
                savesConfirmed = selected.Count(row => row.Data.Event == "save_succeeded"),
                features = selected.GroupBy(row => row.Data.Feature).Select(group => new { feature = group.Key, events = group.Count(), errors = group.Count(row => IsError(row.Data.Event)) }).OrderByDescending(group => group.events),
                errors, trend = buckets, recent = selected.Skip((page - 1) * 50).Take(50), page, pages
            }, Json);
        }
    }
    private static bool IsError(string name) => name is "failed" or "save_failed" or "runtime_error";

    private static StoredDiagnostic? ParseRow(string line)
    {
        try
        {
            if (line.Length > 2048) return null;
            using var doc = JsonDocument.Parse(line, new JsonDocumentOptions { MaxDepth = 4 }); var root = doc.RootElement;
            var names = root.EnumerateObject().Select(property => property.Name).ToArray();
            if (names.Length != 4 || names.Distinct(StringComparer.Ordinal).Count() != 4 || names.Any(name => name is not "receivedAtUtc" and not "source" and not "serverVersion" and not "data")) return null;
            var source = root.GetProperty("source").GetString(); var release = root.GetProperty("serverVersion").GetString();
            if (source is not "anonymous-client" and not "server" || string.IsNullOrEmpty(release) || release.Length > 80 || release.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '.' and not '_') ||
                !root.GetProperty("receivedAtUtc").TryGetDateTimeOffset(out var received) || received.Year is < 2020 or > 2100) return null;
            var data = DiagnosticsContract.Parse(Encoding.UTF8.GetBytes("{\"version\":1,\"events\":[" + root.GetProperty("data").GetRawText() + "]}"), historical: true);
            return data?.Count == 1 ? new(received, source, release, data[0]) : null;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException) { return null; }
    }
}
