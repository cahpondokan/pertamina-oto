using System.Globalization;
using System.Text.Json;

namespace Pertamina.Oto.Web.Preview.Models;

public sealed record PdfCapacityViewModel(string Summary, string Detail, string Limit, string Support);

// Server-rendered copy and browser workers read the same embedded/static JSON.
// No JavaScript evaluation, user configuration or document data is involved.
public static class PdfCapacity
{
    private static readonly JsonElement Data = Load();
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("id-ID");
    private static JsonElement Load()
    {
        using var stream = typeof(PdfCapacity).Assembly.GetManifestResourceStream("Oto.Pdf.Capacity.json")
            ?? throw new InvalidOperationException("Capacity manifest missing.");
        using var json = JsonDocument.Parse(stream);
        if (json.RootElement.GetProperty("version").GetInt32() != 1) throw new InvalidOperationException("Capacity schema unsupported.");
        return json.RootElement.Clone();
    }
    private static long N(JsonElement item, string key) => item.GetProperty(key).GetInt64();
    private static string Num(long value) => value.ToString("N0", Culture);
    private static string Bytes(long value) => value >= 1_000_000_000 && value % 1_000_000_000 == 0
        ? $"{Num(value / 1_000_000_000)} GB"
        : value % 1_048_576 == 0 ? $"{Num(value / 1_048_576)} MiB" : $"{Num(value)} byte";
    private static JsonElement Bounded => Data.GetProperty("bounded");
    public static string SplitParts => $"{Num(N(Data, "splitMinimumParts"))}–{Num(N(Data.GetProperty("split"), "outputs"))} bagian";
    public static string SessionLimit => $"{Num(N(Bounded, "files"))} file sumber, {Bytes(N(Bounded, "totalBytes"))} input dan {Bytes(N(Bounded, "outputBytes"))} hasil";
    public static string Timeout => $"{Num(N(Bounded, "timeout") / 1000)} detik";
    public static string UnitNote => "MiB = 1.048.576 byte; GB = 1.000.000.000 byte. Batas penerimaan bukan jaminan setiap dokumen dapat diproses; struktur PDF dan kemampuan perangkat tetap diperiksa.";

    public static PdfCapacityViewModel For(string tool)
    {
        if (!Data.GetProperty("tools").TryGetProperty(tool, out var key)) throw new InvalidOperationException("Tool has no admitted input capacity.");
        var profile = Data.GetProperty("profiles").GetProperty(key.GetString()!);
        var budgetKey = profile.GetProperty("budget").GetString()!;
        var budget = Data.GetProperty(budgetKey);
        var multiple = profile.GetProperty("multiple").GetBoolean();
        var images = profile.GetProperty("kind").GetString() == "images";
        var count = multiple ? N(budget, "files") : 1;
        var kind = images ? "gambar" : "PDF";
        var summary = $"{Num(count)} {kind} · {Bytes(multiple ? N(budget, "totalBytes") : N(budget, "fileBytes"))}{(multiple ? " total" : "")}";
        if (budgetKey == "merge")
        {
            var limit = $"{Num(N(profile, "minimum"))}–{Num(count)} PDF · Total maks. {Bytes(N(budget, "totalBytes"))} ({Num(N(budget, "totalBytes"))} byte)";
            var support = $"{limit}. Tidak ada batas halaman tetap. Kapasitas tetap bergantung ruang penyimpanan dan kompleksitas dokumen: maksimum {Num(N(budget, "inputObjects"))} objek dari seluruh input dan {Num(N(budget, "objects"))} objek per PDF, metadata parser {Bytes(N(budget, "metadataBytes"))} per file, serta batas dekompresi/struktur keamanan yang ada. Proses dihentikan setelah {Num(N(budget, "timeout") / 60000)} menit.";
            return new(summary, "Batas penerimaan · bergantung kompleksitas PDF", limit, support);
        }
        var detail = multiple && budgetKey == "bounded" ? $"{Bytes(N(budget, "fileBytes"))}/{(images ? "gambar" : "file")} · " : "";
        detail += images ? $"{Num(N(Data.GetProperty("images"), "totalPixels") / 1_000_000)} juta piksel total" : $"{Num(N(budget, "pages"))} halaman{(multiple ? " total" : "")}";
        var full = summary + " · " + detail;
        if (budgetKey is "split" or "local")
        {
            summary = multiple ? $"Maks. {Num(count)} {kind} · {Bytes(N(budget, "totalBytes"))} total" : $"1 PDF · Maks. {Bytes(N(budget, "fileBytes"))}";
            var parser = Data.GetProperty("merge");
            var fileExtra = images
                ? $" Batas gambar tetap {Num(N(Data.GetProperty("images"), "pixels") / 1_000_000)} juta piksel per gambar, {Num(N(Data.GetProperty("images"), "dimension"))} piksel per sisi, {Bytes(N(parser, "metadataBytes"))} metadata header dan {Bytes(N(Bounded, "outputBytes"))} total gambar JPEG setelah normalisasi. Dekoder gambar browser juga memakai RAM."
                : $" Batas struktur tetap berlaku: {Num(N(parser, "objects"))} objek per PDF, {Num(N(parser, "inputObjects"))} objek seluruh input dan {Bytes(N(parser, "metadataBytes"))} metadata parser per PDF, beserta pemeriksaan dekompresi.";
            var support = $"{summary} ({Num(N(budget, "totalBytes"))} byte per pekerjaan), {detail}; hasil {(budgetKey == "split" ? SplitParts : "1 PDF")} dengan total maks. {Bytes(N(budget, "outputBytes"))}. File PDF dibaca bertahap; hasil ditulis ke penyimpanan sementara lokal browser (OPFS), bukan server. Ruang yang diperlukan mengikuti ukuran hasil, tanpa menyediakan 4 GB terlebih dahulu.{fileExtra} Proses dibatasi {Num(N(budget, "timeout") / 60000)} menit. Alat PDF selain Gabungkan PDF menjalankan satu proses pada satu waktu untuk situs ini.";
            return new(summary, detail, summary + " · " + detail, support);
        }
        var extra = images ? $" JPG/PNG. Maksimum {Num(N(Data.GetProperty("images"), "pixels") / 1_000_000)} juta piksel per gambar dan {Num(N(Data.GetProperty("images"), "dimension"))} piksel per sisi." : "";
        return new(summary, detail, (images ? "JPG / PNG · " : "") + full, full + "." + extra);
    }
}
