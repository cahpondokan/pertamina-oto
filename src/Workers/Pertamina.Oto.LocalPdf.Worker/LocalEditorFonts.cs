using System.Buffers.Binary;
using PdfSharp.Fonts;

namespace Pertamina.Oto.LocalPdf;

// Reads only admitted local font filenames. Never downloads or bundles Windows fonts.
internal sealed class LocalEditorFonts : IFontResolver
{
    private static readonly Dictionary<string, (string File, string PostScript)> Faces = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Arial"] = ("arial.ttf", "ArialMT"), ["Arial Bold"] = ("arialbd.ttf", "Arial-BoldMT"),
        ["Arial Italic"] = ("ariali.ttf", "Arial-ItalicMT"), ["Arial Bold Italic"] = ("arialbi.ttf", "Arial-BoldItalicMT"),
        ["Times New Roman"] = ("times.ttf", "TimesNewRomanPSMT"), ["Times New Roman Bold"] = ("timesbd.ttf", "TimesNewRomanPS-BoldMT"),
        ["Times New Roman Italic"] = ("timesi.ttf", "TimesNewRomanPS-ItalicMT"), ["Times New Roman Bold Italic"] = ("timesbi.ttf", "TimesNewRomanPS-BoldItalicMT"),
        ["Calibri"] = ("calibri.ttf", "Calibri"), ["Calibri Bold"] = ("calibrib.ttf", "Calibri-Bold"),
        ["Calibri Italic"] = ("calibrii.ttf", "Calibri-Italic"), ["Calibri Bold Italic"] = ("calibriz.ttf", "Calibri-BoldItalic"),
        ["Cambria Bold"] = ("cambriab.ttf", "Cambria-Bold"), ["Cambria Italic"] = ("cambriai.ttf", "Cambria-Italic"),
        ["Cambria Bold Italic"] = ("cambriaz.ttf", "Cambria-BoldItalic"),
        ["Verdana"] = ("verdana.ttf", "Verdana"), ["Verdana Bold"] = ("verdanab.ttf", "Verdana-Bold"),
        ["Verdana Italic"] = ("verdanai.ttf", "Verdana-Italic"), ["Verdana Bold Italic"] = ("verdanaz.ttf", "Verdana-BoldItalic"),
        ["Aptos"] = ("aptos.ttf", "Aptos"), ["Aptos Bold"] = ("aptos-bold.ttf", "Aptos-Bold"),
        ["Aptos Italic"] = ("aptos-italic.ttf", "Aptos-Italic"), ["Aptos Bold Italic"] = ("aptos-bold-italic.ttf", "Aptos-BoldItalic")
    };
    internal static void Initialize()
    {
        if (GlobalFontSettings.FallbackFontResolver is null) GlobalFontSettings.FallbackFontResolver = new LocalEditorFonts();
    }
    internal static string[] Available() => Faces.Keys.Where(face => Read(face) is not null).ToArray();
    internal static string Match(string pdfName)
    {
        var name = pdfName.Split('+')[^1];
        static string Normalize(string value) => value.Replace(" ", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
        return Faces.FirstOrDefault(pair => (Normalize(pair.Value.PostScript).Equals(Normalize(name), StringComparison.OrdinalIgnoreCase) || Normalize(pair.Key).Equals(Normalize(name), StringComparison.OrdinalIgnoreCase)) && Read(pair.Key) is not null).Key ?? "";
    }
    private static byte[]? Read(string face)
    {
        if (!Faces.TryGetValue(face, out var entry)) return null;
        foreach (var directory in new[] { Environment.GetFolderPath(Environment.SpecialFolder.Fonts), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts") })
        {
            var path = Path.Combine(directory, entry.File);
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length is < 12 or > 20000000) continue;
                var bytes = File.ReadAllBytes(path);
                if (BinaryPrimitives.ReadUInt32BigEndian(bytes) != 0x00010000) continue; // Standalone TrueType only; TTC/CFF/variable fonts are not silently substituted.
                var count = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(4));
                if (12 + count * 16 > bytes.Length) continue;
                for (var i = 0; i < count; i++)
                {
                    var table = 12 + i * 16;
                    if (System.Text.Encoding.ASCII.GetString(bytes, table, 4) != "OS/2") continue;
                    var offset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(table + 8));
                    if (offset > bytes.Length - 10) break;
                    var permissions = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan((int)offset + 8));
                    // Restrictive/preview-only/no-subsetting/bitmap-only permissions fail closed.
                    if ((permissions & 0x0306) == 0) return bytes;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return null;
    }
    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        => familyName.StartsWith("OTO-LOCAL:", StringComparison.Ordinal) && Read(familyName[10..]) is not null ? new FontResolverInfo(familyName[10..]) : null;
    public byte[]? GetFont(string faceName) => Read(faceName);
}
