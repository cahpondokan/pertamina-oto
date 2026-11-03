using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oto.WebDiagnostics;

// Separate from authenticated enterprise audit. All browser facts are untrusted
// reports, never evidence of a person's identity or an authoritative saved file.
internal static class DiagnosticsContract
{
    internal const string Route = "/api/diagnostics/events";
    internal const int MaxBody = 12288;
    internal static readonly HashSet<string> Features = new("shell merge organize split images-pdf numbers watermark remove extract rotate crop".Split(' '), StringComparer.Ordinal);
    internal static readonly HashSet<string> Stages = new("input process save runtime cleanup preview cloud".Split(' '), StringComparer.Ordinal);
    internal static readonly HashSet<string> Events = new("started succeeded failed cancelled download_requested save_succeeded save_cancelled save_failed runtime_error".Split(' '), StringComparer.Ordinal);
    internal static readonly HashSet<string> Codes = new("NONE UNKNOWN CANCELLED WORKER TIMEOUT VERIFY INVALID ENCRYPTED LIMIT SESSION_LIMIT TYPE SINGLE MERGE EMPTY SPLIT PARTITION RANGE OPTIONS TEXT LAYOUT IMAGE_FORMAT IMAGE_LIMIT IMAGE_BROWSER FILE_COUNT TOTAL_BYTES STORAGE_BROWSER STORAGE UNSUPPORTED UNSUPPORTED_FORM UNSUPPORTED_SIGNATURE UNSUPPORTED_ATTACHMENT UNSUPPORTED_ACTION UNSUPPORTED_ANNOTATIONS UNSUPPORTED_TAGGED UNSAFE_LINK LINK_TARGET ANNOTATION_OPERATION TAGGED_STRUCTURE TAGGED_OPERATION TAGGED_MIXED TAGGED_CONFLICT CLOUD_CONFIG CLOUD_LOAD CLOUD_AUTH CLOUD_DOWNLOAD CLOUD_RESPONSE CLOUD_CHANGED TYPE_ERROR RANGE_ERROR REFERENCE_ERROR SYNTAX_ERROR QUOTA NOT_ALLOWED NOT_READABLE SECURITY NETWORK SERVER_ERROR".Split(' '), StringComparer.Ordinal);
    private static readonly HashSet<string> Browsers = new(["edge", "firefox", "chromium", "safari", "other"], StringComparer.Ordinal);
    private static readonly HashSet<string> Fields = new("id processId event feature stage code elapsedMs occurredAt clientVersion browser".Split(' '), StringComparer.Ordinal);

    internal static List<DiagnosticEvent>? Parse(ReadOnlyMemory<byte> bytes, bool historical = false)
    {
        try
        {
            using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 4 });
            var root = doc.RootElement;
            if (!ExactFields(root, new HashSet<string>(["version", "events"], StringComparer.Ordinal)) ||
                !root.GetProperty("version").TryGetInt32(out var version) || version != 1) return null;
            var items = root.GetProperty("events");
            if (items.ValueKind != JsonValueKind.Array || items.GetArrayLength() is < 1 or > 16) return null;
            List<DiagnosticEvent> result = [];
            foreach (var item in items.EnumerateArray())
            {
                if (!ExactFields(item, Fields)) return null;
                string S(string name) => item.GetProperty(name).GetString() ?? "";
                var id = S("id"); var process = S("processId"); var eventName = S("event");
                var feature = S("feature"); var stage = S("stage"); var code = S("code"); var browser = S("browser");
                if (!Uuid(id) || !Uuid(process) || !Events.Contains(eventName) || !Features.Contains(feature) ||
                    !Stages.Contains(stage) || !Codes.Contains(code) || !Browsers.Contains(browser) || S("clientVersion") != "web-audit-1" ||
                    !item.GetProperty("elapsedMs").TryGetInt32(out var elapsed) || elapsed is < 0 or > 3600000 ||
                    !item.GetProperty("occurredAt").TryGetInt64(out var occurred) ||
                    occurred is < 0 or > 4102444800000 ||
                    (!historical && Math.Abs((decimal)occurred - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) > 86400000)) return null;
                var failed = eventName is "failed" or "save_failed" or "runtime_error";
                var cancelled = eventName is "cancelled" or "save_cancelled";
                if (failed && code is "NONE" or "CANCELLED" || cancelled && code != "CANCELLED" || !failed && !cancelled && code != "NONE") return null;
                if (eventName.StartsWith("save_", StringComparison.Ordinal) || eventName == "download_requested") { if (stage != "save") return null; }
                if (eventName == "runtime_error" && (stage != "runtime" || feature != "shell")) return null;
                result.Add(new(id, process, eventName, feature, stage, code, elapsed, occurred, "web-audit-1", browser));
            }
            return result;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException or OverflowException) { return null; }
    }

    private static bool ExactFields(JsonElement value, HashSet<string> fields)
    {
        if (value.ValueKind != JsonValueKind.Object) return false;
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject()) if (!fields.Contains(property.Name) || !seen.Add(property.Name)) return false;
        return seen.Count == fields.Count;
    }
    private static bool Uuid(string value) => value.Length == 36 && value[14] == '4' && "89ab".Contains(value[19], StringComparison.Ordinal) && Guid.TryParseExact(value, "D", out _) && !value.Any(char.IsAsciiLetterUpper);
}

internal sealed record DiagnosticEvent(string Id, string ProcessId, string Event, string Feature, string Stage, string Code, int ElapsedMs, long OccurredAt, string ClientVersion, string Browser);
internal sealed record StoredDiagnostic(DateTimeOffset ReceivedAtUtc, string Source, string ServerVersion, DiagnosticEvent Data);

internal sealed class DiagnosticsStore : IDisposable
{
    private const long MaxFileBytes = 16 * 1024 * 1024;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<string, string> fingerprints = new(StringComparer.Ordinal);
    private readonly string? directory;
    private readonly string serverVersion;
    private bool loaded;
    private readonly bool readerGroup;
    private long windowStart;
    private int windowRequests;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    internal bool Enabled { get; }
    internal string LastState { get; private set; } = "disabled";

    public DiagnosticsStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Enabled = configuration["WebDiagnostics:Enabled"] == "true";
        readerGroup = configuration["WebDiagnostics:ReaderGroup"] == "true";
        serverVersion = configuration["WebDiagnostics:Release"] ?? "web-audit-20261027";
        if (serverVersion.Length > 80 || serverVersion.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '.' and not '_'))
            throw new InvalidOperationException("Invalid diagnostics release identifier.");
        if (!Enabled) return;
        directory = configuration["WebDiagnostics:Directory"];
        if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory))
            throw new InvalidOperationException("Diagnostics requires an absolute private directory outside the application.");
        directory = Path.GetFullPath(directory);
        var root = Path.GetFullPath(environment.ContentRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var web = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(root, "wwwroot")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if ((directory + Path.DirectorySeparatorChar).StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            (directory + Path.DirectorySeparatorChar).StartsWith(web, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Diagnostics storage must be outside the application and webroot.");
        LastState = "ready";
    }

    // Global bounded budget, no IP/device/session tracking. No queue of waiting requests.
    internal bool Admit()
    {
        lock (fingerprints)
        {
            var minute = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;
            if (minute != windowStart) { windowStart = minute; windowRequests = 0; }
            return ++windowRequests <= 60;
        }
    }

    internal async Task<int> AppendAsync(IReadOnlyList<DiagnosticEvent> events, string source = "anonymous-client", CancellationToken cancellationToken = default)
    {
        if (!Enabled) return 503;
        if (!await gate.WaitAsync(0, cancellationToken).ConfigureAwait(false)) return 429;
        try
        {
            var path = Path.Combine(directory!, "events.jsonl");
            CheckLinks(directory!);
            if (OperatingSystem.IsWindows()) Directory.CreateDirectory(directory!);
            else Directory.CreateDirectory(directory!, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | (readerGroup ? UnixFileMode.GroupRead | UnixFileMode.GroupExecute : 0));
            CheckLinks(path);
            var options = new FileStreamOptions { Mode = FileMode.OpenOrCreate, Access = FileAccess.ReadWrite, Share = FileShare.Read, Options = FileOptions.Asynchronous };
            if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | (readerGroup ? UnixFileMode.GroupRead : 0);
            await using var stream = new FileStream(path, options);
            if (stream.Length > MaxFileBytes) { LastState = "capacity"; return 503; }
            if (!loaded)
            {
                fingerprints.Clear();
                using var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, leaveOpen: true);
                while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
                {
                    var row = JsonSerializer.Deserialize<StoredDiagnostic>(line, Json) ?? throw new JsonException();
                    if (!fingerprints.TryAdd(row.Data.Id, Fingerprint(row.Data)) || fingerprints.Count > 32768) throw new JsonException();
                }
                // Never append to an incomplete tail left by an interrupted write.
                if (stream.Length > 0) { stream.Position = stream.Length - 1; if (stream.ReadByte() != '\n') throw new JsonException(); }
                loaded = true;
            }
            Dictionary<string, string> pending = new(StringComparer.Ordinal);
            List<StoredDiagnostic> rows = [];
            foreach (var item in events)
            {
                var hash = Fingerprint(item);
                if (fingerprints.TryGetValue(item.Id, out var existing) || pending.TryGetValue(item.Id, out existing))
                { if (hash != existing) return 409; continue; }
                pending.Add(item.Id, hash); rows.Add(new(DateTimeOffset.UtcNow, source, serverVersion, item));
            }
            if (rows.Count == 0) return 204;
            var bytes = Encoding.UTF8.GetBytes(string.Concat(rows.Select(row => JsonSerializer.Serialize(row, Json) + "\n")));
            if (stream.Length + bytes.Length > MaxFileBytes || fingerprints.Count + rows.Count > 32768) { LastState = "capacity"; return 503; }
            var originalLength = stream.Length;
            stream.Position = originalLength;
            try
            {
                await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }
            catch { stream.SetLength(originalLength); throw; }
            foreach (var entry in pending) fingerprints.Add(entry.Key, entry.Value);
            LastState = "ready";
            return 204;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or OperationCanceledException)
        { loaded = false; LastState = "unavailable"; return 503; }
        finally { gate.Release(); }
    }
    private static string Fingerprint(DiagnosticEvent value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value, Json)));
    private static void CheckLinks(string path)
    {
        for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Diagnostics links are not allowed.");
    }
    public void Dispose() => gate.Dispose();
}

internal static class DiagnosticsHosting
{
    internal static bool IsIngestion(HttpRequest request) => HttpMethods.IsPost(request.Method) && request.Path.Equals(DiagnosticsContract.Route, StringComparison.Ordinal);

    internal static void MapWebDiagnostics(this WebApplication app)
    {
        app.MapPost(DiagnosticsContract.Route, async (HttpContext context, DiagnosticsStore store) =>
        {
            if (!store.Enabled) return Results.StatusCode(404);
            var request = context.Request;
            if (request.QueryString.HasValue || request.Headers["X-Oto-Diagnostics"] != "1" ||
                !Uri.TryCreate(request.Headers.Origin.ToString(), UriKind.Absolute, out var origin) ||
                !string.Equals(origin.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase) ||
                (origin.Scheme != "https" && !(origin.Scheme == "http" && origin.IsLoopback)) ||
                (request.Headers["Sec-Fetch-Site"].Count > 0 && request.Headers["Sec-Fetch-Site"] != "same-origin")) return Results.StatusCode(403);
            if (!string.Equals(request.ContentType?.Split(';')[0].Trim(), "application/json", StringComparison.OrdinalIgnoreCase) || request.Headers.ContentEncoding.Count != 0)
                return Results.StatusCode(415);
            if (request.ContentLength > DiagnosticsContract.MaxBody) return Results.StatusCode(413);
            if (!store.Admit()) { context.Response.Headers.RetryAfter = "60"; return Results.StatusCode(429); }
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromSeconds(3));
                var bytes = new byte[DiagnosticsContract.MaxBody + 1]; var count = 0;
                while (count < bytes.Length)
                {
                    var read = await request.Body.ReadAsync(bytes.AsMemory(count), timeout.Token).ConfigureAwait(false);
                    if (read == 0) break; count += read;
                }
                if (count > DiagnosticsContract.MaxBody) return Results.StatusCode(413);
                var events = DiagnosticsContract.Parse(bytes.AsMemory(0, count));
                return events is null ? Results.StatusCode(400) : Results.StatusCode(await store.AppendAsync(events, cancellationToken: timeout.Token).ConfigureAwait(false));
            }
            catch (Exception e) when (e is IOException or OperationCanceledException or BadHttpRequestException) { return Results.StatusCode(400); }
        });
        app.MapGet("/privasi-web", () => Results.Content(PrivacyHtml, "text/html; charset=utf-8"));
    }

    internal static async Task SafeServerError(HttpContext context, DiagnosticsStore store)
    {
        // No path, request body, exception text or stack is retained.
        if (store.Enabled && store.Admit())
        {
            var id = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
            await store.AppendAsync([new(id, id, "runtime_error", "shell", "runtime", "SERVER_ERROR", 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "web-audit-1", "other")], "server").ConfigureAwait(false);
        }
        if (!context.Response.HasStarted) { context.Response.StatusCode = 500; await context.Response.WriteAsync("Terjadi gangguan aplikasi. Silakan coba kembali.").ConfigureAwait(false); }
    }

    private const string PrivacyHtml = """
        <!doctype html><html lang="id"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Privasi OTO Web</title>
        <link rel="stylesheet" href="/css/site.css"><main class="main"><h1>Privasi OTO Web</h1>
        <p>OTO Web dapat digunakan tanpa akun. PDF dan gambar diproses di perangkat Anda; isi, nama dan lokasi file tidak dikirim melalui diagnostik.</p>
        <h2>Catatan aktivitas dan gangguan</h2><p>Ketika diagnostik diaktifkan pengelola, aplikasi mengirim ID acak per proses, waktu, fitur, tahapan, hasil, durasi, versi aplikasi, keluarga browser, dan kode kesalahan yang telah ditentukan ke server OTO. Tidak ada cookie analitik, identitas pengunjung tetap, isi dokumen, teks tanda air, rentang halaman, URL, pesan kesalahan atau stack mentah di dalam catatan ini.</p>
        <p>Catatan digunakan untuk menelusuri gangguan. ID proses tidak mengidentifikasi orang. Catatan dari browser merupakan laporan klien yang belum terverifikasi; unduhan diminta tidak berarti file sudah tersimpan.</p>
        <p>Antrean diagnostik dibatasi di memori tab. Pengiriman yang gagal dicoba kembali secara terbatas dan dapat hilang saat tab ditutup. Gangguan diagnostik tidak menghalangi pemrosesan PDF lokal. Catatan disimpan di direktori privat pengelola dengan batas kapasitas, tanpa akses baca publik atau penghapusan otomatis.</p>
        <p>Server jaringan tetap menerima alamat IP untuk melayani koneksi. Aplikasi diagnostik tidak menyimpannya; log infrastruktur server dikelola terpisah.</p><p><a href="/">Kembali ke alat PDF</a></p></main></html>
        """;
}
