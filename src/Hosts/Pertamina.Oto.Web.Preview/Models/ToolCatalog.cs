namespace Pertamina.Oto.Web.Preview.Models;

// Presentation capabilities only. Execution limits and validation stay in PDF workers.
public sealed record BrowserToolProfile(string InputKind, bool Multiple, bool Reorder, bool TransferSources)
{
    public static readonly BrowserToolProfile SinglePdf = new("pdf", false, false, true);
    public static readonly BrowserToolProfile Organizer = new("pdf", true, true, true);
    public static readonly BrowserToolProfile Merge = new("pdf", true, true, true);
    public static readonly BrowserToolProfile Images = new("images", true, true, false);
    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, System.Text.Json.JsonSerializerOptions.Web);
}
public sealed record ToolDefinition(string Id, string Title, string Description, string Category, string Icon, bool Preview = false, BrowserToolProfile? Browser = null);
public sealed record WorkspaceViewModel(string Id, string Title, string Description, string Icon)
{
    public BrowserToolProfile? Browser => ToolCatalog.Find(Id)?.Browser;
}

public static class ToolCatalog
{
    public static ToolDefinition? Find(string id) => All.FirstOrDefault(tool => tool.Id == id);
    public static bool BrowserEnabled(string id) => Find(id)?.Browser is not null;
    public static IReadOnlyList<ToolDefinition> All { get; } = new ToolDefinition[]
    {
        new("merge", "Gabungkan PDF", "Satukan dokumen, rapikan pekerjaan.", "Atur PDF", "028-gabungkan-pdf.png", true, BrowserToolProfile.Merge),
        new("organize", "Atur halaman PDF", "Susun, putar, dan atur setiap halaman.", "Atur PDF", "031-atur-ulang-halaman-pdf.png", true, BrowserToolProfile.Organizer),
        new("split", "Pisahkan PDF", "Ambil bagian yang Anda perlukan.", "Atur PDF", "030-pisahkan-pdf.png", Browser: BrowserToolProfile.SinglePdf),
        new("compress", "Kompres PDF", "Dokumen lebih kecil, mudah dibagikan.", "Optimalkan", "040-kompres-pdf.png"),
        new("edit", "Sunting PDF", "Tambahkan teks dan anotasi.", "Edit PDF", "018-sunting-pdf.png"),
        new("images-pdf", "Gambar ke PDF", "Jadikan foto satu dokumen PDF.", "Konversi", "005-gambar-ke-pdf.png", Browser: BrowserToolProfile.Images),
        new("pdf-images", "PDF ke gambar", "Simpan halaman sebagai gambar.", "Konversi", "092-pdf-ke-gambar.png"),
        new("sign", "Tanda tangan PDF", "Tambahkan tanda tangan visual.", "Keamanan", "049-tanda-tangani-pdf.png"),
        new("numbers", "Nomor halaman", "Penomoran yang rapi dan konsisten.", "Edit PDF", "023-tambahkan-nomor-halaman.png", Browser: BrowserToolProfile.SinglePdf),
        new("watermark", "Tanda air PDF", "Beri identitas pada dokumen.", "Edit PDF", "022-tambah-tanda-air.png", Browser: BrowserToolProfile.SinglePdf),
        new("remove", "Hapus halaman", "Keluarkan halaman yang tidak perlu.", "Atur PDF", "032-hapus-halaman-pdf.png", Browser: BrowserToolProfile.SinglePdf),
        new("extract", "Ekstrak halaman", "Simpan halaman pilihan sebagai PDF.", "Atur PDF", "033-ekstrak-halaman-pdf.png", Browser: BrowserToolProfile.SinglePdf),
        new("rotate", "Putar halaman", "Perbaiki orientasi dokumen Anda.", "Atur PDF", "034-putar-halaman-pdf.png", Browser: BrowserToolProfile.SinglePdf),
        new("protect", "Lindungi PDF", "Atur perlindungan dokumen.", "Keamanan", "047-lindungi-pdf.png"),
        new("ocr", "PDF OCR", "Ubah hasil pindai menjadi teks.", "Optimalkan", "042-pdf-ocr.png"),
        new("compare", "Bandingkan PDF", "Temukan perbedaan dua dokumen.", "Lihat PDF", "054-bandingkan-pdf.png"),
        new("reader", "Pembaca PDF", "Baca dan telusuri dokumen.", "Lihat PDF", "061-pembaca-pdf.png"),
        new("crop", "Potong halaman", "Sesuaikan area halaman PDF.", "Edit PDF", "025-potong-pdf.png", Browser: BrowserToolProfile.SinglePdf)
    };
}
