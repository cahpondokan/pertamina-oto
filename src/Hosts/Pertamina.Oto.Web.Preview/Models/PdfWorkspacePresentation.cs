namespace Pertamina.Oto.Web.Preview.Models;

// Presentation ports shared by the locally processed PDF workspaces.
public sealed record PdfFlowViewModel(string Prefix, string Action, bool Multiple = false);
public sealed record PdfHeadingViewModel(string Title, string Description, string Icon, PdfCapacityViewModel Capacity);
public sealed record PdfDropViewModel(string Id, string PickId, string Title, string Description, string Button, string Limit, string Icon, string InputLabel = "PDF");

// Tool-specific copy for one shared shell. Worker admission remains authoritative.
public sealed record PdfToolPresentation(string EmptyTitle, string Description, string Action)
{
    public static PdfToolPresentation For(WorkspaceViewModel tool) => tool.Id switch
    {
        "organize" => new("Setiap halaman, sesuai urutan Anda", "Susun, putar, dan rapikan halaman dalam satu PDF baru.", "Susun"),
        "split" => new("Setiap bagian, jadi PDF tersendiri", "Pilih satu PDF, lalu tentukan bagian yang akan dipisahkan.", "Pisahkan"),
        "extract" => Pdf("Ambil halaman yang Anda perlukan", "Pilih halaman sumber untuk disimpan menjadi PDF baru.", "Ekstrak"),
        "remove" => Pdf("Simpan hanya halaman yang diperlukan", "Tentukan halaman yang akan dikeluarkan dari PDF baru.", "Hapus"),
        "rotate" => Pdf("Arah halaman, sesuai kebutuhan Anda", "Pilih halaman dan sudut putar untuk memperbaiki orientasinya.", "Putar"),
        "numbers" => Pdf("Nomor halaman yang rapi dan berurutan", "Atur posisi, angka awal, dan tampilan nomor halaman.", "Nomori"),
        "watermark" => Pdf("Beri identitas pada dokumen Anda", "Tambahkan tanda air teks dengan tampilan yang Anda pilih.", "Tandai"),
        "crop" => Pdf("Sesuaikan bingkai halaman Anda", "Atur tepi area yang terlihat pada halaman PDF.", "Potong"),
        "images-pdf" => new("Foto dan gambar, jadi satu PDF", "Susun gambar, lalu pilih ukuran halaman dan margin PDF.", "Konversi"),
        _ => throw new InvalidOperationException("No shared presentation for this tool.")
    };
    private static PdfToolPresentation Pdf(string title, string description, string action) =>
        new(title, description, action);
}
