using System.Globalization;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;

namespace Pertamina.Oto.LocalPdf;

internal static partial class PdfOperations
{
    private static CSequence BuildLocalReplacement(PdfPage page, TextCandidate candidate, EditorMark edit)
    {
        if (!LocalEditorFonts.Available().Contains(edit.LocalFont, StringComparer.Ordinal) || edit.Text is null || edit.Text.Length > 200 || edit.Text.Any(c => c < 32 || c > 126))
            throw new InputException("Font lokal tidak tersedia/izin embedding tidak sesuai, atau karakter di luar Latin dasar. Tidak ada font diganti otomatis.");
        if (edit.Text.Length == 0) return ContentReader.ReadContent(Encoding.ASCII.GetBytes(candidate.PreserveAdvance
            ? "[() " + (-candidate.Run.Width / candidate.Scale).ToString("R", CultureInfo.InvariantCulture) + "] TJ\n" : "() Tj\n"));
        // PDFsharp prepares a properly embedded Unicode font and encoded operand. We transplant
        // only Tf/Tj at the original position: original clipping, color, CTM and layering remain.
        // The temporary drawing stream is discarded by ReplaceOriginalText's final ReplaceContent.
        using (var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
        {
            var font = new XFont("OTO-LOCAL:" + edit.LocalFont, candidate.State.Size, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
            var width = graphics.MeasureString(edit.Text, font).Width * candidate.Scale * 1000 / candidate.State.Size;
            if (edit.FitText && width > candidate.Run.Width)
            {
                var fittedSize = candidate.State.Size * candidate.Run.Width / width;
                if (fittedSize * candidate.Run.Size / candidate.State.Size < 6) throw new InputException("Teks terlalu panjang: ukuran hasil akan kurang dari 6 pt. Pendekkan teks agar tetap terbaca.");
                font = new XFont("OTO-LOCAL:" + edit.LocalFont, fittedSize, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
                width = graphics.MeasureString(edit.Text, font).Width * candidate.Scale * 1000 / candidate.State.Size;
            }
            if (width > candidate.Run.Width + .1)
                throw new InputException("Teks pengganti lebih lebar dari area asli. Pendekkan teks; perluasan kotak/baris untuk teks asli belum tersedia.");
            graphics.DrawString(edit.Text, font, XBrushes.Black, 0, 100);
        }
        var generated = ContentReader.ReadContent(page).OfType<COperator>().ToArray();
        var tf = generated.Last(o => o.Name == "Tf");
        var tj = generated.Last(o => o.Name is "Tj" or "TJ");
        var result = ContentReader.ReadContent(Encoding.ASCII.GetBytes("0 Tc\n0 Tw\n")); result.Add(tf); result.Add(tj);
        result.Add(ContentReader.ReadContent(Encoding.ASCII.GetBytes(candidate.State.Font + " " + candidate.State.Size.ToString("R", CultureInfo.InvariantCulture) + " Tf\n" + candidate.State.Spacing.ToString("R", CultureInfo.InvariantCulture) + " Tc\n" + candidate.State.WordSpacing.ToString("R", CultureInfo.InvariantCulture) + " Tw\n")));
        if (candidate.PreserveAdvance)
        {
            // Embedded PDF widths are rounded from native font metrics. Restore both
            // the original line matrix and the exact original text endpoint instead
            // of deriving the next fragment's position from MeasureString.
            string N(double value) => value.ToString("R", CultureInfo.InvariantCulture);
            var advance = (candidate.TextX - candidate.LineX) / candidate.TextA * 1000 / candidate.State.Size + candidate.Run.Width / candidate.Scale;
            result.Add(ContentReader.ReadContent(Encoding.ASCII.GetBytes(N(candidate.TextA) + " 0 0 " + N(candidate.TextD) + " " + N(candidate.LineX) + " " + N(candidate.LineY) + " Tm\n[" + N(-advance) + "] TJ\n")));
        }
        return result;
    }
}
