using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal static partial class PdfOperations
{
    private static PdfReply InspectEditorDocument(PdfRequest request)
    {
        if (request.Inputs.Length != 1 || new FileInfo(request.Inputs[0]).Length > MaxBytes)
            throw new InputException("Pilih satu PDF, maksimal 100 MB.");
        using var document = Open(request.Inputs[0], "", PdfDocumentOpenMode.Modify);
        ValidateEditorDocument(document);
        return new(true, "Dokumen siap diedit.", document.PageCount, PageSizes: document.Pages.Cast<PdfSharp.Pdf.PdfPage>().Select(p => new EditorPageSize(p.Width.Point, p.Height.Point)).ToArray());
    }

    private static void ValidateEditorDocument(PdfSharp.Pdf.PdfDocument document)
    {
        if (document.Internals.Catalog.Elements.ContainsKey("/AcroForm") || document.SecuritySettings.IsEncrypted || document.Internals.Catalog.Elements.ContainsKey("/Perms"))
            throw new InputException("Editor belum mendukung PDF formulir, bertanda tangan digital, atau terenkripsi.");
        foreach (var page in document.Pages)
        {
            if (page.Rotate != 0 || page.MediaBox.X1 != 0 || page.MediaBox.Y1 != 0 ||
                (page.Elements.ContainsKey("/CropBox") && !page.CropBox.Equals(page.MediaBox)) ||
                (page.Elements.ContainsKey("/UserUnit") && page.Elements.GetReal("/UserUnit") != 1))
                throw new InputException("Editor belum mendukung PDF dengan rotasi atau area potong khusus.");
            if (page.Width.Point is < 40 or > 2250 || page.Height.Point is < 40 or > 2250)
                throw new InputException("Ukuran halaman belum didukung editor.");
        }
    }

    // Original replacements modify content operands; additional marks append independently.
    private static int AddEditorMarks(PdfRequest request)
    {
        if (!string.IsNullOrEmpty(request.Password)) throw new InputException("Editor belum mendukung PDF terenkripsi.");
        if (request.Marks is not { Length: > 0 and <= 100 } marks)
            throw new InputException("Tambahkan 1–100 objek sebelum menyimpan.");
        using var document = Open(request.Inputs[0], request.Password, PdfDocumentOpenMode.Modify);
        ValidateEditorDocument(document);
        ReplaceOriginalText(document, marks.Where(m => m.Kind == "replace-text").ToArray());
        foreach (var mark in marks)
        {
            if (mark.Kind == "replace-text") continue;
            if (mark is null || mark.Page < 0 || mark.Page >= document.PageCount ||
                !new[] { mark.X, mark.Y, mark.Width, mark.Height, mark.Size }.All(double.IsFinite) ||
                mark.X < 0 || mark.Y < 0 || mark.Width <= 0 || mark.Height <= 0 || mark.Size is < 1 or > 72)
                throw new InputException("Posisi atau ukuran objek tidak valid.");
            var page = document.Pages[mark.Page];
            if (page.Rotate != 0 || page.MediaBox.X1 != 0 || page.MediaBox.Y1 != 0 ||
                (page.Elements.ContainsKey("/CropBox") && !page.CropBox.Equals(page.MediaBox)))
                throw new InputException("Editor awal belum mendukung halaman diputar atau dipotong. Tidak ada hasil diterbitkan.");
            if (mark.X + mark.Width > page.Width.Point || mark.Y + mark.Height > page.Height.Point)
                throw new InputException("Objek melewati tepi halaman. Perkecil atau pindahkan objek.");
            if (mark.Color is null || mark.Color.Length != 7 || mark.Color[0] != '#' ||
                !uint.TryParse(mark.Color.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                throw new InputException("Warna objek tidak valid.");
            var color = XColor.FromArgb((int)(rgb >> 16), (int)((rgb >> 8) & 255), (int)(rgb & 255));
            using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            var pen = new XPen(color, mark.Size);
            switch (mark.Kind)
            {
                case "text":
                    if (string.IsNullOrWhiteSpace(mark.Text) || mark.Text.Length > 200 || mark.Text.Any(c => c < 32 || c > 126))
                        throw new InputException("Teks tambahan saat ini mendukung satu baris huruf Latin dasar, maksimal 200 karakter.");
                    var font = new XFont("Arial", mark.Size);
                    var measured = graphics.MeasureString(mark.Text, font);
                    if (measured.Width > mark.Width || measured.Height > mark.Height)
                        throw new InputException("Teks melebihi kotaknya. Perbesar kotak atau kecilkan ukuran font.");
                    graphics.DrawString(mark.Text, font, new XSolidBrush(color), new XRect(mark.X, mark.Y, mark.Width, mark.Height), XStringFormats.TopLeft);
                    break;
                // Keep strokes inside the bounding box, matching XAML shape rendering.
                case "rectangle":
                case "ellipse":
                    if (mark.Size >= Math.Min(mark.Width, mark.Height)) throw new InputException("Ketebalan garis melebihi ukuran bentuk.");
                    var inset = mark.Size / 2;
                    if (mark.Kind == "rectangle") graphics.DrawRectangle(pen, mark.X + inset, mark.Y + inset, mark.Width - mark.Size, mark.Height - mark.Size);
                    else graphics.DrawEllipse(pen, mark.X + inset, mark.Y + inset, mark.Width - mark.Size, mark.Height - mark.Size);
                    break;
                case "image":
                    if (mark.ImageIndex < 1 || mark.ImageIndex >= request.Inputs.Length)
                        throw new InputException("Gambar tambahan tidak ditemukan.");
                    var imagePath = request.Inputs[mark.ImageIndex];
                    if (new FileInfo(imagePath).Length > 10 * 1024 * 1024 || Path.GetExtension(imagePath).ToLowerInvariant() is not (".png" or ".jpg" or ".jpeg"))
                        throw new InputException("Gunakan gambar JPG/PNG maksimal 10 MB.");
                    using (var image = XImage.FromFile(imagePath))
                    {
                        if ((long)image.PixelWidth * image.PixelHeight > 20000000) throw new InputException("Gambar dibatasi 20 megapiksel.");
                        graphics.DrawImage(image, mark.X, mark.Y, mark.Width, mark.Height);
                    }
                    break;
                case "line":
                    if (mark.Points is null) graphics.DrawLine(pen, mark.X, mark.Y, mark.X + mark.Width, mark.Y + mark.Height);
                    else
                    {
                        if (mark.Points is not { Length: 2 } endpoints || endpoints.Any(p => p is null || !double.IsFinite(p.X) || !double.IsFinite(p.Y) || p.X < 0 || p.Y < 0 || p.X > mark.Width || p.Y > mark.Height))
                            throw new InputException("Titik garis tidak valid.");
                        graphics.DrawLine(pen, mark.X + endpoints[0].X, mark.Y + endpoints[0].Y, mark.X + endpoints[1].X, mark.Y + endpoints[1].Y);
                    }
                    break;
                case "pen":
                    if (mark.Points is not { Length: >= 2 and <= 300 } points || points.Any(p => p is null ||
                        !double.IsFinite(p.X) || !double.IsFinite(p.Y) || p.X < 0 || p.Y < 0 || p.X > mark.Width || p.Y > mark.Height))
                        throw new InputException("Goresan tidak valid atau terlalu panjang.");
                    graphics.DrawLines(pen, points.Select(p => new XPoint(mark.X + p.X, mark.Y + p.Y)).ToArray());
                    break;
                default: throw new InputException("Jenis objek belum didukung. Teks asli tidak diubah.");
            }
        }
        return SaveValidated(document, request.Output);
    }
}
