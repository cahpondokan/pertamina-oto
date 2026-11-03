using System.Globalization;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;

namespace Pertamina.Oto.LocalPdf;

internal static partial class PdfOperations
{
    // Fail closed for unsupported encodings/layouts. This changes the original Tj operand,
    // never paints a rectangle over the old text. No external/native dependency is required.
    private sealed record TextCandidate(EditorTextRun Run, COperator Operator, Dictionary<char, double> Widths, Dictionary<char, string> Encoding, double Scale, TextState State, bool PreserveAdvance, double LineX, double LineY, double TextX, double TextA, double TextD);
    private sealed record SourceFont(string Name, Dictionary<string, char> Decode, Dictionary<char, string> Encode, Dictionary<char, double> Widths, int CodeBytes)
    {
        internal string? Read(string raw)
        {
            if (raw.Length % CodeBytes != 0) return null;
            var chars = new List<char>();
            for (var i = 0; i < raw.Length; i += CodeBytes)
            {
                if (!Decode.TryGetValue(raw.Substring(i, CodeBytes), out var c)) return null;
                chars.Add(c);
            }
            return new string(chars.ToArray());
        }
    }

    private static Dictionary<int, char> ReadUnicodeMap(PdfDictionary font, int codeBytes)
    {
        var result = new Dictionary<int, char>();
        var ambiguous = false;
        var unicode = font.Elements.GetDictionary("/ToUnicode");
        if (unicode?.Stream is null) return result;
        var bytes = unicode.Stream.UnfilteredValue;
        if (bytes.Length > 1024 * 1024) return result;
        var cmap = System.Text.Encoding.ASCII.GetString(bytes);
        if (cmap.Contains("usecmap", StringComparison.Ordinal)) return result;
        MatchCollection Matches(string value, string pattern) => Regex.Matches(value, pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline, TimeSpan.FromSeconds(1));
        void Add(string source, string target)
        {
            if (source.Length != codeBytes * 2 || target.Length != 4) return;
            var cid = int.Parse(source, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var scalar = int.Parse(target, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (IsSupportedSourceCharacter(scalar))
            {
                if (result.TryGetValue(cid, out var previous) && previous != (char)scalar) ambiguous = true;
                result[cid] = (char)scalar;
            }
        }
        foreach (Match block in Matches(cmap, @"beginbfchar(.*?)endbfchar"))
            foreach (Match pair in Matches(block.Groups[1].Value, @"<([0-9A-Fa-f]+)>\s*<([0-9A-Fa-f]+)>")) Add(pair.Groups[1].Value, pair.Groups[2].Value);
        foreach (Match block in Matches(cmap, @"beginbfrange(.*?)endbfrange"))
            foreach (Match range in Matches(block.Groups[1].Value, @"<([0-9A-Fa-f]+)>\s*<([0-9A-Fa-f]+)>\s*(<([0-9A-Fa-f]+)>|\[([^\]]*)\])"))
            {
                if (range.Groups[1].Length != codeBytes * 2 || range.Groups[2].Length != codeBytes * 2) continue;
                var start = int.Parse(range.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var end = int.Parse(range.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (end < start || end - start > 4096) continue;
                if (range.Groups[4].Success && range.Groups[4].Length == 4)
                {
                    var target = int.Parse(range.Groups[4].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    for (var cid = start; cid <= end; cid++)
                        if (IsSupportedSourceCharacter(target + cid - start))
                            Add(cid.ToString(codeBytes == 2 ? "X4" : "X2", CultureInfo.InvariantCulture), (target + cid - start).ToString("X4", CultureInfo.InvariantCulture));
                }
                else if (range.Groups[5].Success)
                {
                    var values = Matches(range.Groups[5].Value, @"<([0-9A-Fa-f]+)>");
                    if (values.Count != end - start + 1) continue;
                    for (var i = 0; i < values.Count; i++) Add((start + i).ToString(codeBytes == 2 ? "X4" : "X2", CultureInfo.InvariantCulture), values[i].Groups[1].Value);
                }
            }
        return ambiguous ? [] : result;
    }

    private static bool IsSupportedSourceCharacter(int scalar) => scalar is >= 32 and <= 126 or >= 160 and <= 591 or >= 8192 and <= 8303;

    private static SourceFont? ReadSourceFont(PdfDictionary font, HashSet<string> shown)
    {
        if (font.Elements["/Encoding"] is { } encodingItem && encodingItem is not PdfName) return null;
        var subtype = font.Elements.GetName("/Subtype"); var cidFont = subtype == "/Type0";
        if (subtype == "/Type1") return ReadStandardSourceFont(font);
        // A simple embedded TrueType font may use its built-in code mapping. Preserve
        // those exact source codes; ToUnicode describes text, not a WinAnsi substitute.
        var encoding = font.Elements.GetName("/Encoding");
        var mappedEmbedded = !font.Elements.ContainsKey("/Encoding") &&
            font.Elements.GetDictionary("/ToUnicode")?.Stream is not null &&
            font.Elements.GetDictionary("/FontDescriptor")?.Elements.GetDictionary("/FontFile2")?.Stream is not null;
        if (cidFont ? encoding != "/Identity-H" : subtype != "/TrueType" || (encoding != "/WinAnsiEncoding" && !mappedEmbedded)) return null;
        var codeBytes = cidFont ? 2 : 1;
        var map = font.Elements.ContainsKey("/ToUnicode") ? ReadUnicodeMap(font, codeBytes)
            : cidFont ? [] : Enumerable.Range(32, 95).ToDictionary(i => i, i => (char)i);
        var descendant = cidFont ? font.Elements.GetArray("/DescendantFonts")?.Elements.GetDictionary(0) : null;
        if (cidFont && descendant?.Elements.GetName("/Subtype") != "/CIDFontType2") return null;
        var decode = new Dictionary<string, char>(); var encode = new Dictionary<char, string>(); var widths = new Dictionary<char, double>();
        var usedCodes = new HashSet<string>();
        foreach (var text in shown)
            if (text.Length % codeBytes == 0)
                for (var i = 0; i < text.Length; i += codeBytes) usedCodes.Add(text.Substring(i, codeBytes));
        double CidWidth(int cid)
        {
            var w = descendant!.Elements.GetArray("/W");
            if (w is not null)
                for (var i = 0; i < w.Elements.Count;)
                {
                    var first = w.Elements.GetInteger(i++); if (i >= w.Elements.Count) break;
                    if (w.Elements[i] is PdfArray array)
                    {
                        i++; if (cid >= first && cid - first < array.Elements.Count) return array.Elements.GetReal(cid - first);
                    }
                    else
                    {
                        var last = w.Elements.GetInteger(i++); if (i >= w.Elements.Count) break;
                        var value = w.Elements.GetReal(i++); if (cid >= first && cid <= last) return value;
                    }
                }
            return descendant.Elements.ContainsKey("/DW") ? descendant.Elements.GetReal("/DW") : 1000;
        }
        var simpleWidths = cidFont ? null : font.Elements.GetArray("/Widths"); var firstChar = cidFont ? 0 : font.Elements.GetInteger("/FirstChar");
        foreach (var (code, character) in map)
        {
            var raw = codeBytes == 2 ? new string([(char)(code >> 8), (char)(code & 255)]) : ((char)code).ToString();
            if (!usedCodes.Contains(raw)) continue; // An embedded subset may omit unreferenced glyphs.
            var index = code - firstChar;
            var width = cidFont ? CidWidth(code) : simpleWidths is not null && index >= 0 && index < simpleWidths.Elements.Count ? simpleWidths.Elements.GetReal(index) : 0;
            if (!double.IsFinite(width) || width <= 0 || width > 2000) continue;
            decode[raw] = character;
            // Ambiguous Unicode-to-glyph mapping cannot be silently inverted.
            if (encode.TryGetValue(character, out var previousCode) && previousCode != raw) return null;
            encode[character] = raw; widths[character] = width;
        }
        return new(font.Elements.GetName("/BaseFont").TrimStart('/'), decode, encode, widths, codeBytes);
    }
    private sealed record TextState(double A = 1, double D = 1, double E = 0, double F = 0,
        string Font = "", double Size = 0, double Spacing = 0, double WordSpacing = 0, double Horizontal = 100, double Rise = 0, int Render = 0, bool Safe = true, double Leading = 0);

    private static PdfReply InspectOriginalText(PdfRequest request)
    {
        if (request.Inputs.Length != 1 || new FileInfo(request.Inputs[0]).Length > MaxBytes)
            throw new InputException("Pilih satu PDF maksimal 100 MB.");
        using var document = Open(request.Inputs[0], "", PdfDocumentOpenMode.Modify);
        ValidateEditorDocument(document);
        var index = request.Rotation; // Explicit zero-based page for this internal operation.
        if (index < 0 || index >= document.PageCount) throw new InputException("Halaman tidak valid.");
        var (_, candidates, explanation) = ReadOriginalText(document.Pages[index]);
        var runs = candidates.Select(c => c.Run).ToArray();
        return new(true, runs.Length == 0
            ? explanation
            : "Pilih teks berbingkai pada halaman atau dari daftar. Font dan posisi asli dipertahankan." + explanation, TextRuns: runs, LocalFonts: LocalEditorFonts.Available());
    }

    private static (CSequence Content, List<TextCandidate> Candidates, string Explanation) ReadOriginalText(PdfPage page)
    {
        if (page.Contents.CreateSingleContent().Stream.Value.Length > 4 * 1024 * 1024)
            throw new InputException("Isi halaman terlalu kompleks untuk edit teks langsung.");
        var content = ContentReader.ReadContent(page);
        var operators = content.Cast<CObject>().OfType<COperator>().ToArray();
        if (operators.Length != content.Count || operators.Any(o => o.Name is "BI" or "ID" or "EI" or "BDC" or "BMC" or "EMC"))
            return (content, [], "Edit teks dibatasi: halaman berisi tagged content atau gambar inline yang belum aman ditulis ulang."); // Do not serialize unsupported content.
        var fonts = page.Resources.Elements.GetDictionary("/Font");
        if (fonts is null) return (content, [], "Tidak ditemukan font teks langsung pada halaman. Teks mungkin berupa gambar, outline, atau objek terpisah; OCR hanya diperlukan untuk gambar.");
        var available = new Dictionary<string, HashSet<string>>();
        var fontName = ""; var fontStack = new Stack<string>();
        foreach (var op in operators)
        {
            if (op.Name == "q") fontStack.Push(fontName);
            if (op.Name == "Q") fontName = fontStack.Count > 0 ? fontStack.Pop() : "";
            if (op.Name == "Tf" && op.Operands.Count == 2 && op.Operands[0] is CName name) fontName = name.Name;
            if (!available.TryGetValue(fontName, out var chars)) available[fontName] = chars = [];
            if (op.Name == "Tj" && op.Operands.Count == 1 && op.Operands[0] is CString text)
                chars.Add(text.Value);
            if (op.Name == "TJ" && op.Operands.Count == 1 && op.Operands[0] is CArray array)
                foreach (var item in array.OfType<CString>()) chars.Add(item.Value);
        }
        // A restored font can be used solely for a numeric TJ advance after a local
        // replacement. It still needs its state tracked even without painted glyphs.
        var sourceFonts = available.Where(kv => kv.Key.StartsWith('/') && fonts.Elements.GetDictionary(kv.Key) is not null)
            .ToDictionary(kv => kv.Key, kv => ReadSourceFont(fonts.Elements.GetDictionary(kv.Key)!, kv.Value));
        var result = new List<TextCandidate>(); var state = new TextState(); var stack = new Stack<TextState>();
        var rejectedFont = 0; var rejectedLayout = 0;
        // ISO 32000-1 9.4: text advances change Tm, never the text line matrix.
        // Keep the line origin separately so T*, Td and TD survive earlier edits.
        double tx = 0, ty = 0, lx = 0, ly = 0, ta = 1, td = 1;
        var positioned = false; var inside = false; var matrixSafe = true;
        for (var id = 0; id < operators.Length; id++)
        {
            var op = operators[id]; var args = op.Operands;
            double N(int n) => args[n] switch { CInteger i => i.Value, CReal r => r.Value, _ => double.NaN };
            switch (op.Name)
            {
                case "q": stack.Push(state); break;
                case "Q": state = stack.Count > 0 ? stack.Pop() : new TextState(Safe: false); break;
                case "cm" when args.Count == 6:
                    state = state with { A = state.A * N(0), D = state.D * N(3), E = state.A * N(4) + state.E, F = state.D * N(5) + state.F,
                        Safe = state.Safe && N(1) == 0 && N(2) == 0 }; break;
                case "gs":
                    var gsName = args.Count == 1 && args[0] is CName gn ? gn.Name : "";
                    var gs = page.Resources.Elements.GetDictionary("/ExtGState")?.Elements.GetDictionary(gsName);
                    if (gs is null || gs.Elements.ContainsKey("/Font")) state = state with { Safe = false };
                    break;
                case "BT": inside = true; positioned = true; matrixSafe = true; lx = ly = tx = ty = 0; ta = td = 1; break;
                case "ET": inside = false; break;
                case "Tf" when args.Count == 2 && args[0] is CName fn: state = state with { Font = fn.Name, Size = N(1) }; break;
                case "Tc": state = state with { Spacing = N(0) }; break;
                case "Tw": state = state with { WordSpacing = N(0) }; break;
                case "TL": state = state with { Leading = N(0) }; break;
                case "Tz": state = state with { Horizontal = N(0) }; break;
                case "Ts": state = state with { Rise = N(0) }; break;
                case "Tr": state = state with { Render = (int)N(0) }; break;
                case "Tm" when args.Count == 6:
                    ta = N(0); td = N(3); lx = tx = N(4); ly = ty = N(5); matrixSafe = N(1) == 0 && N(2) == 0; positioned = matrixSafe; break;
                case "Td" or "TD" when args.Count == 2:
                    if (op.Name == "TD") state = state with { Leading = -N(1) };
                    lx += ta * N(0); ly += td * N(1); tx = lx; ty = ly; positioned = matrixSafe; break;
                case "T*":
                    ly -= td * state.Leading; tx = lx; ty = ly; positioned = matrixSafe && double.IsFinite(ly); break;
                case "'" or "\"": positioned = false; matrixSafe = false; break;
                case "TJ" or "Tj":
                    var previousCount = result.Count;
                    var pieces = args.Count == 1 ? args[0] is CString single ? new CObject[] { single } : args[0] is CArray array ? array.ToArray() : [] : [];
                    var strings = pieces.OfType<CString>().ToArray();
                    var raw = string.Concat(strings.Select(s => s.Value));
                    var adjustment = pieces.Sum(p => p is CInteger integer ? integer.Value : p is CReal real ? real.Value : 0);
                    var advanced = false;
                    if (inside && positioned && state.Safe && state.Render == 0 && double.IsFinite(state.Spacing) && Math.Abs(state.Spacing) <= 1 && double.IsFinite(state.WordSpacing) && Math.Abs(state.WordSpacing) <= 72 && state.Horizontal == 100 && state.Rise == 0 &&
                        state.Size is >= 1 and <= 72 && state.A * ta > 0 && state.D * td > 0 && pieces.Length > 0 && pieces.All(p => p is CString or CInteger or CReal) &&
                        raw.Length <= 400 && strings.All(s => s.CStringType is CStringType.String or CStringType.HexString))
                    {
                        if (sourceFonts.TryGetValue(state.Font, out var font) && font is not null && font.Read(raw) is { Length: <= 200 } decoded)
                        {
                            if (decoded.All(font.Widths.ContainsKey))
                            {
                                var scale = state.Size * state.A * ta / 1000;
                                // Tw applies only to the single-byte code 32, not to a Unicode
                                // space encoded as a multibyte CID or another source character code.
                                var wordSpaces = font.CodeBytes == 1 ? raw.Count(c => c == ' ') : 0;
                                var advance = (decoded.Sum(c => font.Widths[c]) - adjustment) * state.Size / 1000 + decoded.Length * state.Spacing + wordSpaces * state.WordSpacing;
                                var width = advance * state.A * ta;
                                var size = state.Size * state.D * td;
                                var x = state.A * tx + state.E; var baseline = page.Height.Point - (state.D * ty + state.F);
                                var top = baseline - size; var height = size * 1.3;
                                if (decoded.Length > 0 && new[] { x, top, width, height, size }.All(double.IsFinite) && x >= 0 && top >= 0 && width > 0 && x + width <= page.Width.Point && top + height <= page.Height.Point && result.Count < 100)
                                    result.Add(new(new(id, decoded, font.Name, x, top, width, height, size, new string(font.Widths.Keys.Order().ToArray()), LocalEditorFonts.Match(font.Name)), op, font.Widths, font.Encode, scale, state, !HasIndependentFollowingPosition(operators, id), lx, ly, tx, ta, td));
                                tx += advance * ta;
                                advanced = double.IsFinite(tx);
                            }
                        }
                    }
                    if (raw.Length > 0 && result.Count == previousCount)
                    {
                        if (!sourceFonts.TryGetValue(state.Font, out var rejected) || rejected is null || rejected.Read(raw) is null) rejectedFont++;
                        else rejectedLayout++;
                    }
                    positioned = advanced; break;
            }
        }
        var reasons = new List<string>();
        if (rejectedFont > 0) reasons.Add($"{rejectedFont} bagian: encoding/pemetaan atau metrik font belum didukung");
        if (rejectedLayout > 0) reasons.Add($"{rejectedLayout} bagian: posisi, transformasi, atau batas teks belum aman");
        var explanation = reasons.Count > 0 ? string.Join("; ", reasons) + "." : "";
        return (content, result, result.Count > 0 ? (explanation.Length > 0 ? " Dilewati: " + explanation : "")
            : explanation.Length > 0 ? "Tidak ada teks yang dapat diedit. " + explanation
            : "Tidak ditemukan operator teks langsung. Halaman mungkin berupa gambar, outline, atau objek terpisah.");
    }

    private static bool HasIndependentFollowingPosition(COperator[] ops, int index)
    {
        // Td/TD position relative to the line matrix, not the advanced text matrix.
        for (var i = index + 1; i < ops.Length; i++)
        {
            if (ops[i].Name is "Td" or "TD" or "Tm" or "T*" or "ET") return true;
            if (ops[i].Name is "Tj" or "TJ" or "'" or "\"") return false;
        }
        return false;
    }

    private static void ReplaceOriginalText(PdfDocument document, EditorMark[] edits)
    {
        if (edits.GroupBy(m => (m.Page, m.SourceId)).Any(g => g.Count() != 1)) throw new InputException("Perubahan teks berulang.");
        foreach (var group in edits.GroupBy(m => m.Page))
        {
            if (group.Key < 0 || group.Key >= document.PageCount) throw new InputException("Halaman teks tidak valid.");
            var page = document.Pages[group.Key]; var (content, candidates, _) = ReadOriginalText(page);
            var replacements = new Dictionary<COperator, CSequence>();
            foreach (var edit in group)
            {
                var candidate = candidates.SingleOrDefault(c => c.Run.Id == edit.SourceId);
                if (candidate is null || candidate.Run.Text != edit.OriginalText) throw new InputException("Teks sumber berubah atau formatnya belum didukung. Buka ulang dokumen.");
                if (!string.IsNullOrEmpty(edit.LocalFont))
                {
                    replacements[candidate.Operator] = BuildLocalReplacement(page, candidate, edit);
                    continue;
                }
                if (edit.Text is null || edit.Text.Length > 200 || edit.Text.Any(c => !candidate.Widths.ContainsKey(c)))
                    throw new InputException("Karakter pengganti tidak tersedia pada font PDF ini. Font asli tidak diganti diam-diam.");
                var words = edit.Text.Count(c => candidate.Encoding[c] == " ");
                var width = edit.Text.Sum(c => candidate.Widths[c]) * candidate.Scale + (edit.Text.Length * candidate.State.Spacing + words * candidate.State.WordSpacing) * candidate.Scale * 1000 / candidate.State.Size;
                if (width > candidate.Run.Width + .1) throw new InputException("Teks pengganti lebih lebar dari area asli. Gunakan teks lebih singkat agar tata letak tetap aman.");
                var operand = new CString { Value = string.Concat(edit.Text.Select(c => candidate.Encoding[c])), CStringType = CStringType.String };
                if (candidate.PreserveAdvance)
                {
                    // Preserve the original endpoint for the next fragment, including a font
                    // change mid-line. No overlay, guessed Tm or alteration of the line matrix.
                    var sequence = ContentReader.ReadContent(System.Text.Encoding.ASCII.GetBytes("[] TJ\n"));
                    var array = (CArray)((COperator)sequence[0]).Operands[0];
                    array.Add(operand);
                    array.Add(new CReal { Value = (width - candidate.Run.Width) / candidate.Scale });
                    replacements[candidate.Operator] = sequence;
                }
                else
                {
                    candidate.Operator.Operands.Clear();
                    if (candidate.Operator.Name == "TJ") { var array = new CArray(); array.Add(operand); candidate.Operator.Operands.Add(array); }
                    else candidate.Operator.Operands.Add(operand);
                }
            }
            var finalContent = new CSequence();
            foreach (var item in content)
                if (item is COperator op && replacements.TryGetValue(op, out var replacement)) finalContent.Add(replacement);
                else finalContent.Add(item);
            page.Contents.ReplaceContent(finalContent);
        }
    }
}
