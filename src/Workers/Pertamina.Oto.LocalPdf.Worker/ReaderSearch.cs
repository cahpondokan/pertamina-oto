using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

// Read-only extraction, deliberately separate from the original-text editing admission rules.
// Lives in the bounded worker: query/content is returned over the pipe, never saved or logged.
internal static class ReaderSearch
{
    private const int MaxPageCharacters = 250_000;
    private const int MaxHits = 100;
    private static readonly HashSet<string> StandardFonts = new(StringComparer.Ordinal)
    { "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique", "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic", "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique" };
    private sealed record FontMap(int Width, Dictionary<string, string> Codes);

    internal static PdfReply Execute(PdfRequest request)
    {
        var query = Normalize(request.Text ?? "").Trim();
        if (query.Length is < 1 or > 200 || query.Contains('\0')) throw new InputException("Isi pencarian 1–200 karakter.");
        if (request.Inputs.Length != 1) throw new InputException("Pencarian memerlukan satu PDF lokal.");
        var path = Path.GetFullPath(request.Inputs[0]);
        if (path.StartsWith(@"\\", StringComparison.Ordinal) || !Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || new FileInfo(path).Length is <= 0 or > 104857600) throw new InputException("Pilih satu PDF lokal maksimal 100 MB.");
        // Modify admission enforces owner credentials even though no document is saved.
        // Password-protected search is not supported in this first version.
        using var document = PdfReader.Open(path, "", PdfDocumentOpenMode.Modify);
        if (document.SecuritySettings.IsEncrypted)
            throw new InputException("Pencarian PDF terlindungi belum didukung. Mode baca tetap tersedia.");
        if (document.PageCount is < 1 or > 1000) throw new InputException("Batas pencarian: 1–1000 halaman.");
        var hits = new List<PdfSearchHit>(); var textPages = 0; var partialPages = 0; var truncated = false; var boxCount = 0;
        for (var pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
        {
            var (text, partial, positions) = ExtractPage(document.Pages[pageIndex]);
            if (text.Any(c => c != '\0' && !char.IsWhiteSpace(c))) textPages++;
            if (partial) partialPages++;
            for (var offset = 0; offset <= text.Length - query.Length;)
            {
                var match = text.IndexOf(query, offset, StringComparison.OrdinalIgnoreCase);
                if (match < 0) break;
                if (hits.Count == MaxHits) { truncated = true; break; }
                var start = Math.Max(0, match - 30); var end = Math.Min(text.Length, match + Math.Min(query.Length, 80) + 30);
                // Bounded snippets keep JSON below the client pipe response ceiling.
                var boxes = MatchBoxes(Enumerable.Range(match, query.Length).Where(i => !char.IsWhiteSpace(text[i])).Select(i => positions[i]).ToArray());
                if (boxCount + (boxes?.Length ?? 0) > 256) boxes = null;
                boxCount += boxes?.Length ?? 0;
                hits.Add(new(pageIndex, (start > 0 ? "…" : "") + text[start..end].Replace('\0', ' ') + (end < text.Length ? "…" : ""),
                    hits.Count, match - start + (start > 0 ? 1 : 0), Math.Min(query.Length, end - match), boxes));
                offset = match + query.Length;
            }
        }
        return new(true, "Pencarian selesai.", document.PageCount, Search: new(hits.ToArray(), textPages, partialPages, truncated));
    }

    private static PdfSearchBox[]? MatchBoxes(PdfSearchBox?[] glyphs)
    {
        if (glyphs.Any(g => g is null)) return null;
        var boxes = new List<PdfSearchBox>();
        foreach (var box in glyphs.Select(g => g!).Distinct())
        {
            if (boxes.Count > 0)
            {
                var last = boxes[^1];
                if (Math.Abs(last.Y - box.Y) < .00001 && Math.Abs(last.Height - box.Height) < .00001 &&
                    (Math.Abs(last.X + last.Width - box.X) < .005 || Math.Abs(box.X + box.Width - last.X) < .005))
                { var x = Math.Min(last.X, box.X); boxes[^1] = last with { X = x, Width = Math.Max(last.X + last.Width, box.X + box.Width) - x }; continue; }
                if (Math.Abs(last.X - box.X) < .00001 && Math.Abs(last.Width - box.Width) < .00001 &&
                    (Math.Abs(last.Y + last.Height - box.Y) < .005 || Math.Abs(box.Y + box.Height - last.Y) < .005))
                { var y = Math.Min(last.Y, box.Y); boxes[^1] = last with { Y = y, Height = Math.Max(last.Y + last.Height, box.Y + box.Height) - y }; continue; }
            }
            boxes.Add(box);
        }
        // Keep anonymous-pipe replies bounded for pathological multiline queries.
        return boxes.Count is > 0 and <= 16 ? boxes.ToArray() : null;
    }

    private static (string Text, bool Partial, List<PdfSearchBox?> Positions) ExtractPage(PdfPage page)
    {
        var text = new StringBuilder(); var partial = false; var positions = new List<PdfSearchBox?>();
        var geometry = new ReaderSearchGeometry(page);
        void Append(string value, PdfSearchBox? box = null)
        {
            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c) && text.Length > 0 && text[^1] == ' ') continue;
                text.Append(char.IsWhiteSpace(c) ? ' ' : c); positions.Add(box);
            }
        }
        try
        {
            var stream = page.Contents.CreateSingleContent().Stream;
            if (stream.Value.Length > 4 * 1024 * 1024 || stream.UnfilteredValue.Length > 8 * 1024 * 1024) return ("", true, []);
            var resources = page.Resources;
            var fonts = resources.Elements.GetDictionary("/Font");
            var maps = new Dictionary<string, FontMap?>();
            var content = ContentReader.ReadContent(page);
            if (content.Count > 100_000) return ("", true, []);
            var font = ""; var stack = new Stack<string>(); var inside = false;
            if (page.Elements.GetArray("/Annots") is { Elements.Count: > 0 }) partial = true;
            void Boundary() { if (text.Length > 0 && text[^1] != ' ') Append(" "); }
            void Unknown() { partial = true; Append("\0"); }
            void Add(CString value)
            {
                if (!inside) { Unknown(); return; }
                if (!maps.TryGetValue(font, out var map))
                {
                    var dictionary = font.StartsWith('/') ? fonts?.Elements.GetDictionary(font) : null;
                    maps[font] = map = dictionary is null ? null : ReadFont(dictionary);
                }
                if (map is null || value.Value.Length % map.Width != 0) { geometry.UnknownAdvance(); Unknown(); return; }
                for (var i = 0; i < value.Value.Length; i += map.Width)
                {
                    if (text.Length >= MaxPageCharacters) { Unknown(); return; }
                    var raw = value.Value.Substring(i, map.Width); var box = geometry.Glyph(raw);
                    if (map.Codes.TryGetValue(raw, out var decoded)) Append(decoded, box);
                    else Unknown();
                }
            }
            foreach (var item in content)
            {
                if (text.Length >= MaxPageCharacters) { partial = true; break; }
                if (item is not COperator op) { Unknown(); continue; }
                var operands = op.Operands;
                geometry.Operator(op);
                switch (op.Name)
                {
                    case "q": if (stack.Count >= 256) return (text.ToString(), true, positions); stack.Push(font); break;
                    case "Q": font = stack.Count > 0 ? stack.Pop() : ""; break;
                    case "BT": inside = true; Boundary(); break;
                    case "ET": inside = false; Boundary(); break;
                    case "Tf" when operands.Count == 2 && operands[0] is CName name: font = name.Name; break;
                    case "Tm" or "Td" or "TD" or "T*": Boundary(); break;
                    case "Tj" when operands.Count == 1 && operands[0] is CString single: Add(single); break;
                    case "TJ" when operands.Count == 1 && operands[0] is CArray array:
                        foreach (var piece in array)
                        {
                            if (piece is CString value) Add(value);
                            else if (piece is CInteger or CReal)
                            {
                                var adjustment = piece is CInteger integer ? integer.Value : ((CReal)piece).Value;
                                geometry.Adjust(adjustment); if (adjustment <= -120) Boundary();
                            }
                        }
                        break;
                    case "'" or "\"":
                        Boundary(); if (operands.Count > 0 && operands[^1] is CString quoted) Add(quoted); else Unknown(); break;
                    // Forms, images, ActualText and inherited graphics-state fonts need broader extraction.
                    // Insert a hard boundary so missing text cannot create a false cross-gap match.
                    case "gs":
                        var graphics = geometry.GraphicsState(op);
                        // Preserve the established search boundary/snippet offsets across gs.
                        Append("\0");
                        if (!graphics.Safe) partial = true;
                        if (graphics.FontUnknown) font = "";
                        break;
                    case "Do" or "BI" or "ID" or "EI" or "BDC": Unknown(); break;
                    case "Tj" or "TJ" or "Tf": Unknown(); break;
                }
            }
        }
        catch (Exception) { partial = true; } // A failed page never proves that it contains no matches.
        return (text.ToString(), partial, positions);
    }

    private static string Normalize(string text)
    {
        var output = new StringBuilder(text.Length); var space = false;
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c)) { if (!space) output.Append(' '); space = true; }
            else { output.Append(c); space = false; }
        }
        return output.ToString();
    }

    private static FontMap? ReadFont(PdfDictionary font)
    {
        if (font.Elements.ContainsKey("/ToUnicode")) return ReadCMap(font.Elements.GetDictionary("/ToUnicode"));
        var subtype = font.Elements.GetName("/Subtype");
        if (subtype is not ("/Type1" or "/TrueType")) return null;
        if (font.Elements["/Encoding"] is PdfDictionary or PdfSharp.Pdf.Advanced.PdfReference) return null;
        var encoding = font.Elements.GetName("/Encoding");
        var baseFont = font.Elements.GetName("/BaseFont").TrimStart('/');
        var standard = encoding == "/StandardEncoding" || (string.IsNullOrEmpty(encoding) &&
            StandardFonts.Contains(baseFont));
        if (encoding != "/WinAnsiEncoding" && !standard) return null;
        var map = new Dictionary<string, string>();
        for (var c = 32; c <= 126; c++) map[((char)c).ToString()] = ((char)c).ToString();
        if (standard) { map["'"] = "\u2019"; map["`"] = "\u2018"; }
        else
        {
            for (var c = 160; c <= 255; c++) map[((char)c).ToString()] = ((char)c).ToString();
            const string cp1252 = "€\0‚ƒ„…†‡ˆ‰Š‹Œ\0Ž\0\0‘’“”•–—˜™š›œ\0žŸ";
            for (var i = 0; i < cp1252.Length; i++) if (cp1252[i] != '\0') map[((char)(128 + i)).ToString()] = cp1252[i].ToString();
        }
        return new(1, map);
    }

    private static FontMap? ReadCMap(PdfDictionary? unicode)
    {
        if (unicode?.Stream is null) return null;
        var bytes = unicode.Stream.UnfilteredValue;
        if (bytes.Length > 1024 * 1024) return null;
        var cmap = Encoding.ASCII.GetString(bytes);
        if (cmap.Contains("usecmap", StringComparison.Ordinal)) return null;
        var codes = new Dictionary<string, string>(); var width = 0; var invalid = false;
        MatchCollection Matches(string input, string pattern) => Regex.Matches(input, pattern, RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
        var strictUnicode = new UnicodeEncoding(true, false, true);
        void Add(string source, string target)
        {
            if (source.Length is not (2 or 4) || target.Length is 0 or > 64 || target.Length % 4 != 0) { invalid = true; return; }
            if (width != 0 && width != source.Length / 2) { invalid = true; return; }
            width = source.Length / 2;
            var key = Encoding.Latin1.GetString(Convert.FromHexString(source));
            codes[key] = strictUnicode.GetString(Convert.FromHexString(target));
        }
        foreach (Match block in Matches(cmap, @"beginbfchar(.*?)endbfchar"))
            foreach (Match pair in Matches(block.Groups[1].Value, @"<([0-9a-fA-F]+)>\s*<([0-9a-fA-F]+)>")) Add(pair.Groups[1].Value, pair.Groups[2].Value);
        foreach (Match block in Matches(cmap, @"beginbfrange(.*?)endbfrange"))
            foreach (Match range in Matches(block.Groups[1].Value, @"<([0-9a-fA-F]+)>\s*<([0-9a-fA-F]+)>\s*(<([0-9a-fA-F]+)>|\[([^\]]*)\])"))
            {
                var first = range.Groups[1].Value; var last = range.Groups[2].Value;
                if (first.Length is not (2 or 4) || first.Length != last.Length) { invalid = true; continue; }
                var start = int.Parse(first, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var end = int.Parse(last, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (end < start || end - start > 4096) { invalid = true; continue; }
                if (range.Groups[4].Success)
                {
                    var target = Convert.FromHexString(range.Groups[4].Value);
                    if (target.Length is 0 or > 32 || target.Length % 2 != 0) { invalid = true; continue; }
                    for (var n = start; n <= end; n++)
                    {
                        Add(n.ToString(first.Length == 2 ? "X2" : "X4", CultureInfo.InvariantCulture), Convert.ToHexString(target));
                        for (var b = target.Length - 1; b >= 0; b--) { if (++target[b] != 0) break; }
                    }
                }
                else
                {
                    var targets = Matches(range.Groups[5].Value, @"<([0-9a-fA-F]+)>");
                    if (targets.Count != end - start + 1) { invalid = true; continue; }
                    for (var i = 0; i < targets.Count; i++) Add((start + i).ToString(first.Length == 2 ? "X2" : "X4", CultureInfo.InvariantCulture), targets[i].Groups[1].Value);
                }
            }
        return invalid || width == 0 || codes.Count == 0 ? null : new(width, codes);
    }
}
