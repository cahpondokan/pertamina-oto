using System.Globalization;
using System.IO.Compression;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal sealed class InputException(string message) : Exception(message);

internal static class PdfOperations
{
    private const long MaxBytes = 100 * 1024 * 1024;
    private const int MaxPages = 1000;

    internal static PdfReply Execute(PdfRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Inputs is null || request.Inputs.Length is < 1 or > 50)
            throw new InputException("Pilih 1–50 file, total maksimal 100 MB.");
        var output = Path.GetFullPath(request.Output);
        if (File.Exists(output) || Directory.Exists(output) || request.Inputs.Any(p =>
            string.Equals(Path.GetFullPath(p), output, StringComparison.OrdinalIgnoreCase)))
            throw new InputException("Hasil harus memakai nama baru; sumber dan file existing tidak boleh ditimpa.");
        if (request.Inputs.Sum(p => new FileInfo(p).Length) > MaxBytes)
            throw new InputException("Total input versi pengembangan dibatasi 100 MB.");
        if (request.Text.Length > 200 || request.Password.Length > 127 || request.NewPassword.Length > 127)
            throw new InputException("Teks terlalu panjang (maksimal 200 karakter; kata sandi 127).");
        if (request.Tool is not ("merge" or "images") && request.Inputs.Length != 1)
            throw new InputException("Alat ini memerlukan tepat satu PDF.");
        if (request.Tool == "merge" && request.Inputs.Length < 2)
            throw new InputException("Merge memerlukan minimal dua PDF.");

        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        var pages = request.Tool switch
        {
            "merge" => Merge(request),
            "images" => Images(request),
            "extract" or "remove" or "organize" => Select(request),
            "split" => Split(request),
            "rotate" or "crop" or "numbers" or "watermark" or "compress" or "protect" or "unlock" => Modify(request),
            _ => throw new InputException("Mesin alat ini belum tersedia; tidak ada hasil simulasi.")
        };
        return new(true, request.Tool == "compress"
            ? "Selesai. Kompresi lossless; ukuran tidak selalu lebih kecil."
            : "Proses selesai; hasil berhasil dibaca ulang.", pages, new FileInfo(output).Length);
    }

    internal static int[] ParsePages(string specification, int count, bool allowDuplicates = false)
    {
        if (count is < 1 or > MaxPages) throw new InputException("PDF harus berisi 1–1000 halaman.");
        if (string.IsNullOrWhiteSpace(specification)) return Enumerable.Range(0, count).ToArray();
        var pages = new List<int>();
        foreach (var token in specification.Split(',', StringSplitOptions.TrimEntries))
        {
            var bounds = token.Split('-', StringSplitOptions.TrimEntries);
            if (bounds.Length is < 1 or > 2 ||
                !int.TryParse(bounds[0], NumberStyles.None, CultureInfo.InvariantCulture, out var first) ||
                first < 1 || first > count)
                throw new InputException("Format halaman: 1,3-5,2. Nomor harus ada dalam dokumen.");
            var last = first;
            if (bounds.Length == 2 && (!int.TryParse(bounds[1], NumberStyles.None, CultureInfo.InvariantCulture, out last)
                || last < first || last > count))
                throw new InputException("Rentang halaman tidak valid. Contoh: 1,3-5.");
            for (var page = first; page <= last; page++)
            {
                if (!allowDuplicates && pages.Contains(page - 1))
                    throw new InputException("Nomor halaman berulang. Duplikasi hanya tersedia pada Organize.");
                pages.Add(page - 1);
                if (pages.Count > MaxPages) throw new InputException("Hasil dibatasi 1000 halaman.");
            }
        }
        return pages.ToArray();
    }

    private static PdfDocument Open(string path, string password, PdfDocumentOpenMode mode)
    {
        // Import mode alone can admit a user password. Modify mode explicitly
        // enforces owner credentials; no save is performed during this check.
        if (mode == PdfDocumentOpenMode.Import)
        {
            using var permissionCheck = PdfReader.Open(path, password, PdfDocumentOpenMode.Modify);
        }
        var document = PdfReader.Open(path, password, mode);
        if (document.PageCount is < 1 or > MaxPages)
        {
            document.Dispose();
            throw new InputException("PDF harus berisi 1–1000 halaman.");
        }
        return document;
    }

    private static int Merge(PdfRequest request)
    {
        using var output = new PdfDocument();
        foreach (var path in request.Inputs)
        {
            using var input = Open(path, request.Password, PdfDocumentOpenMode.Import);
            if (output.PageCount + input.PageCount > MaxPages)
                throw new InputException("Hasil dibatasi 1000 halaman.");
            foreach (var page in input.Pages) output.AddPage(page);
        }
        return SaveValidated(output, request.Output);
    }

    private static int Select(PdfRequest request)
    {
        using var input = Open(request.Inputs[0], request.Password, PdfDocumentOpenMode.Import);
        if (string.IsNullOrWhiteSpace(request.Pages) && request.Tool == "remove")
            throw new InputException("Isi halaman yang akan dihapus.");
        var pages = ParsePages(request.Pages, input.PageCount, request.Tool == "organize");
        if (request.Tool == "remove") pages = Enumerable.Range(0, input.PageCount).Except(pages).ToArray();
        if (pages.Length == 0) throw new InputException("Minimal satu halaman harus tersisa.");
        using var output = new PdfDocument();
        foreach (var page in pages) output.AddPage(input.Pages[page]);
        return SaveValidated(output, request.Output);
    }

    private static int Split(PdfRequest request)
    {
        using var input = Open(request.Inputs[0], request.Password, PdfDocumentOpenMode.Import);
        var pages = ParsePages(request.Pages, input.PageCount);
        // ZIP is one atomic publication, avoiding partially saved multi-file results.
        using var stream = new FileStream(request.Output, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach (var index in pages)
        {
            using var document = new PdfDocument();
            document.AddPage(input.Pages[index]);
            using var buffer = new MemoryStream();
            document.Save(buffer, false);
            buffer.Position = 0;
            using (var verification = PdfReader.Open(buffer, PdfDocumentOpenMode.Import))
                if (verification.PageCount != 1) throw new InputException("Validasi hasil gagal.");
            buffer.Position = 0;
            var entry = zip.CreateEntry(string.Create(CultureInfo.InvariantCulture, $"halaman-{index + 1:D4}.pdf"));
            using var entryStream = entry.Open();
            buffer.CopyTo(entryStream);
        }
        return pages.Length;
    }

    private static int Images(PdfRequest request)
    {
        using var output = new PdfDocument();
        foreach (var path in request.Inputs)
        {
            var extension = Path.GetExtension(path);
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new InputException("Pilih gambar JPG atau PNG.");
            using var image = XImage.FromFile(path);
            if ((long)image.PixelWidth * image.PixelHeight > 40_000_000)
                throw new InputException("Gambar dibatasi 40 megapiksel.");
            var page = output.AddPage();
            page.Width = XUnit.FromPoint(595.28);
            page.Height = XUnit.FromPoint(841.89);
            using var graphics = XGraphics.FromPdfPage(page);
            var scale = Math.Min(555.28 / image.PointWidth, 801.89 / image.PointHeight);
            var width = image.PointWidth * scale;
            var height = image.PointHeight * scale;
            graphics.DrawImage(image, (595.28 - width) / 2, (841.89 - height) / 2, width, height);
        }
        return SaveValidated(output, request.Output);
    }

    private static int Modify(PdfRequest request)
    {
        using var document = Open(request.Inputs[0], request.Password, PdfDocumentOpenMode.Modify);
        var selected = ParsePages(request.Pages, document.PageCount);
        if (request.Tool == "rotate" && request.Rotation is not (90 or 180 or 270))
            throw new InputException("Rotasi harus 90, 180, atau 270 derajat.");
        if (request.Tool == "watermark" && string.IsNullOrWhiteSpace(request.Text))
            throw new InputException("Isi teks watermark terlebih dahulu.");
        if (request.Tool == "crop" && (!double.IsFinite(request.Margin) || request.Margin <= 0))
            throw new InputException("Margin potong harus lebih besar dari 0 pt.");
        foreach (var index in selected)
        {
            var page = document.Pages[index];
            switch (request.Tool)
            {
                case "rotate": page.Rotate = (page.Rotate + request.Rotation) % 360; break;
                case "crop":
                    var box = page.EffectiveCropBoxReadOnly;
                    if (request.Margin * 2 >= Math.Min(box.Width, box.Height))
                        throw new InputException("Margin potong terlalu besar untuk halaman.");
                    page.CropBox = new PdfRectangle(new XPoint(box.X1 + request.Margin, box.Y1 + request.Margin),
                        new XPoint(box.X2 - request.Margin, box.Y2 - request.Margin));
                    break;
                case "numbers":
                case "watermark":
                    using (var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                    {
                        var number = request.Tool == "numbers";
                        var text = number ? string.Create(CultureInfo.InvariantCulture, $"{index + 1} / {document.PageCount}") : request.Text;
                        var font = new XFont("Arial", number ? 10 : 32);
                        graphics.DrawString(text, font, number ? XBrushes.Black : XBrushes.LightGray,
                            new XRect(20, number ? page.Height.Point - 30 : page.Height.Point / 2 - 30,
                                page.Width.Point - 40, 40), XStringFormats.Center);
                    }
                    break;
            }
        }
        document.Options.CompressContentStreams = true;
        if (request.Tool == "protect")
        {
            if (request.NewPassword.Length < 8) throw new InputException("Gunakan kata sandi baru minimal 8 karakter.");
            document.SecuritySettings.UserPassword = request.NewPassword;
            document.SecuritySettings.OwnerPassword = request.NewPassword;
            document.SecurityHandler.SetEncryptionToV5();
        }
        if (request.Tool == "unlock")
        {
            document.SecurityHandler.SetEncryptionToNoneAndResetPasswords();
        }
        return SaveValidated(document, request.Output,
            request.Tool == "protect" ? request.NewPassword : request.Tool == "unlock" ? "" : request.Password);
    }

    private static int SaveValidated(PdfDocument document, string path, string password = "")
    {
        var count = document.PageCount;
        using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            document.Save(output, false);
        using var reopened = PdfReader.Open(path, password, PdfDocumentOpenMode.Import);
        if (reopened.PageCount != count) throw new InputException("Validasi jumlah halaman hasil gagal.");
        return count;
    }
}
