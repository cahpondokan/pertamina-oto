namespace Pertamina.Oto.Desktop.App;

internal sealed record ToolCard(string Id, string Name, string Group, string Glyph, string Description, bool Available = false);

internal static class ToolCatalog
{
    internal static readonly ToolCard[] All =
    [
        new("merge", "Merge PDF", "Organize PDF", "\uE8B5", "Gabungkan file sesuai urutan daftar.", true),
        new("split", "Split PDF", "Organize PDF", "\uE8A7", "Pilih rentang atau halaman, periksa preview, lalu simpan sebagai PDF baru.", true),
        new("remove", "Remove pages", "Organize PDF", "\uE74D", "Hapus halaman yang Anda tentukan dari salinan hasil.", true),
        new("extract", "Extract pages", "Organize PDF", "\uE8A5", "Ambil halaman terpilih, sesuai urutan yang ditulis.", true),
        new("organize", "Organize PDF", "Organize PDF", "\uE8FD", "Urutkan, duplikasi, atau sisihkan halaman melalui nomor halaman.", true),
        new("scan", "Scan to PDF", "Organize PDF", "\uE8FE", "Belum aktif: koneksi scanner dan alur akuisisi perangkat belum diimplementasikan."),
        new("compress", "Compress PDF", "Optimize PDF", "\uE74A", "Kompresi stream lossless; tidak menurunkan kualitas gambar, ukuran bisa tetap/bertambah.", true),
        new("repair", "Repair PDF", "Optimize PDF", "\uE90F", "Belum aktif: mesin pemulihan dan validasi file rusak belum terintegrasi."),
        new("ocr", "OCR PDF", "Optimize PDF", "\uE8D2", "Belum aktif: OCR lokal dan model bahasa belum terintegrasi."),
        new("images", "JPG to PDF", "Convert to PDF", "\uE91B", "Susun JPG/PNG menjadi PDF A4, satu gambar per halaman.", true),
        new("word-pdf", "Word to PDF", "Convert to PDF", "\uE8A5", "Konversi DOCX A4 portrait lewat Microsoft Word lokal berlisensi. Hasil divalidasi dan sumber tetap utuh. A3/Letter/landscape/DOC/DOCM belum didukung.", true),
        new("ppt-pdf", "PowerPoint to PDF", "Convert to PDF", "\uE8F9", "Belum aktif: konverter Office lokal belum tersedia pada aplikasi."),
        new("excel-pdf", "Excel to PDF", "Convert to PDF", "\uE80A", "Belum aktif: konverter Office lokal belum tersedia pada aplikasi."),
        new("html-pdf", "HTML to PDF", "Convert to PDF", "\uE774", "Belum aktif: renderer HTML offline dengan pemblokiran resource eksternal belum diintegrasikan."),
        new("pdf-jpg", "PDF to JPG", "Convert from PDF", "\uEB9F", "Ubah halaman PDF menjadi JPG langsung; beberapa halaman disimpan dalam folder. Resolusi dan encoding mengikuti preset kualitas JPG.", true),
        new("pdf-word", "PDF to Word", "Convert from PDF", "\uE8A5", "Belum aktif: rekonstruksi teks dan tata letak Word belum diimplementasikan."),
        new("pdf-ppt", "PDF to PowerPoint", "Convert from PDF", "\uE8F9", "Belum aktif: ekspor slide belum diimplementasikan."),
        new("pdf-excel", "PDF to Excel", "Convert from PDF", "\uE80A", "Belum aktif: ekstraksi tabel dan ekspor workbook belum diimplementasikan."),
        new("pdfa", "PDF to PDF/A", "Convert from PDF", "\uE8B7", "Belum aktif: konversi dan validator konformitas PDF/A belum tersedia."),
        new("rotate", "Rotate PDF", "Edit PDF", "\uE7AD", "Putar halaman terpilih sebesar 90, 180, atau 270 derajat.", true),
        new("numbers", "Add page numbers", "Edit PDF", "\uE8D2", "Tambahkan nomor halaman di bagian bawah halaman terpilih.", true),
        new("watermark", "Add watermark", "Edit PDF", "\uE8D3", "Tambahkan teks abu-abu di tengah halaman terpilih.", true),
        new("crop", "Crop PDF", "Edit PDF", "\uE7A8", "Perkecil area tampil dengan margin. Bukan penghapusan data/redaksi.", true),
        new("edit", "Edit PDF", "Edit PDF", "\uE70F", "Edit teks asli yang didukung, tambah teks/gambar/bentuk, lalu simpan PDF baru.", true),
        new("forms", "PDF Forms", "Edit PDF", "\uE73A", "Belum aktif: editor dan pengisian field formulir belum terintegrasi."),
        new("unlock", "Unlock PDF", "PDF Security", "\uE785", "Hapus enkripsi dengan kata sandi pemilik yang sah. Bukan pemecah sandi.", true),
        new("protect", "Protect PDF", "PDF Security", "\uE72E", "Enkripsi PDF dengan kata sandi pembuka baru (AES-256).", true),
        new("sign", "Sign PDF", "PDF Security", "\uED63", "Belum aktif: penempatan tanda tangan visual belum diimplementasikan; tidak menjanjikan tanda tangan digital/PKI."),
        new("redact", "Redact PDF", "PDF Security", "\uE7C9", "Belum aktif: penghapusan permanen konten harus teruji; kotak hitam bukan redaksi aman."),
        new("compare", "Compare PDF", "PDF Security", "\uE8AB", "Belum aktif: pembandingan visual dan konten belum terintegrasi."),
        new("summary", "AI Summarizer", "PDF Intelligence", "\uE945", "Belum aktif: model AI lokal belum tersedia. Tidak ada pengiriman ke API publik."),
        new("translate", "Translate PDF", "PDF Intelligence", "\uE8C1", "Belum aktif: model terjemahan lokal belum tersedia."),
        new("markdown", "PDF to Markdown", "PDF Intelligence", "\uE943", "Belum aktif: ekstraksi struktur dokumen belum terintegrasi.")
    ];
}
