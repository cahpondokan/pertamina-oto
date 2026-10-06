using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PdfSharp.Pdf;

namespace Pertamina.Oto.LocalPdf;

internal static class ReaderSearchTests
{
    internal static int Run()
    {
        var output = Path.Combine(Path.GetTempPath(), "oto-reader-search-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(output);
        var count = 0;
        void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); count++; Console.WriteLine("PASS " + label); }
        const string normal = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>";
        const string winAnsi = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>";
        string Fixture(string name, string[] streams, string font = normal, string? cmap = null, string? graphics = null, bool indirect = false)
        {
            var path = Path.Combine(output, name + ".pdf"); WritePdf(path, streams, font, cmap, graphics, indirect); return path;
        }
        PdfReply Search(string path, string query) => PdfOperations.Execute(new("reader-search", [path], "", Text: query));
        var source = Fixture("source", ["BT /F1 12 Tf 40 700 Td (Alpha Pertamina OTO) Tj ET", "BT /F1 12 Tf 40 700 Td (PERTAMINA kedua pertamina) Tj ET", ""]);
        var hash = SHA256.HashData(File.ReadAllBytes(source));
        var reply = Search(source, "pErTaMiNa");
        Check(reply.Search is { Hits.Length: 3, TextPages: 2, PartialPages: 0 } && reply.Search.Hits.Select(h => h.Page).SequenceEqual([0, 1, 1]), "case insensitive ordered cross-page results and repeated hits");
        Check(reply.Search!.Hits.All(h => h.Boxes is { Length: > 0 }) && reply.Search.Hits.Select(h => h.Id).Distinct().Count() == 3, "each supported occurrence has geometry and stable identity");
        var samePage = reply.Search.Hits.Where(h => h.Page == 1).ToArray();
        Check(samePage[0].Boxes![0].X < samePage[1].Boxes![0].X, "repeated words on one line have distinct glyph coordinates");
        Check(samePage.All(h => h.Snippet.Substring(h.SnippetMatch, h.MatchLength).Equals("pertamina", StringComparison.OrdinalIgnoreCase)), "snippet emphasis references its own occurrence");
        var limits = Fixture("limit-two", ["BT /F1 12 Tf 60 600 Td (Limitation PDG Reached) Tj 0 -80 Td (Service Life Limitation) Tj ET"]);
        var limitHits = Search(limits, "Limit").Search!.Hits;
        Check(limitHits.Length == 2 && limitHits.All(h => h.Boxes is { Length: 1 }) && limitHits[0].Boxes![0].Y < limitHits[1].Boxes![0].Y, "two Limit substrings on same page retain different line positions");
        Check(Math.Abs(limitHits[0].Boxes![0].X * 595 - 60) < .001 && Math.Abs(limitHits[0].Boxes![0].Width * 595 - (556 + 222 + 833 + 222 + 278) * .012) < .001, "Limit highlight uses exact Helvetica advances rather than full line or string fraction");
        var scaled = Fixture("scaled", ["q 2 0 0 2 10 20 cm BT /F1 18 Tf 30 250 Td (Limit) Tj ET Q"]);
        var scaledBox = Search(scaled, "Limit").Search!.Hits[0].Boxes![0];
        Check(Math.Abs(scaledBox.X * 595 - 70) < .001 && Math.Abs(scaledBox.Width * 595 - 2111 * .036) < .001, "graphics transform and mixed font size scale each glyph advance");
        foreach (var rotation in new[] { 0, 90, 180, 270 })
        {
            var rotatedPath = Fixture("crop-rotate-" + rotation, ["BT /F1 12 Tf 60 600 Td (Limit) Tj ET"]);
            using (var pdf = PdfSharp.Pdf.IO.PdfReader.Open(rotatedPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
            { pdf.Pages[0].CropBox = new PdfRectangle(new PdfSharp.Drawing.XRect(20, 30, 555, 782)); pdf.Pages[0].Rotate = rotation; pdf.Save(rotatedPath); }
            var box = Search(rotatedPath, "Limit").Search!.Hits[0].Boxes![0];
            var expectedX = rotation switch { 0 => 40d / 555, 90 => (600 - 12 * .207 - 30) / 782, 180 => 1 - (40 + 2111 * .012) / 555, _ => 1 - (600 + 12 * .718 - 30) / 782 };
            Check(Math.Abs(box.X - expectedX) < .000001, "crop and rotation " + rotation + " follow page display transform");
        }
        var noGeometry = Fixture("no-metrics", ["BT /F1 12 Tf 40 700 Td (Limit) Tj ET"], "<< /Type /Font /Subtype /TrueType /BaseFont /Synthetic /Encoding /WinAnsiEncoding /FontDescriptor << /Type /FontDescriptor >> >>");
        Check(Search(noGeometry, "Limit").Search is { Hits.Length: 1 } noPosition && noPosition.Hits[0].Boxes is null, "missing metrics preserves text with honest no-position fallback");
        var skew = Fixture("skew", ["BT /F1 12 Tf 1 .2 0 1 40 700 Tm (Limit) Tj ET"]);
        Check(Search(skew, "Limit").Search!.Hits[0].Boxes is null, "unsupported skew does not fabricate highlight");
        const string labels = "BT /F1 18 Tf 124 680 Td (Alamat) Tj 0 -320 Td (Alamat) Tj ET";
        foreach (var indirect in new[] { false, true })
        {
            var benign = Fixture("opaque-" + indirect, ["/GS gs " + labels], graphics: "<< /Type /ExtGState /CA 1 /ca 1 /BM /Normal >>", indirect: indirect);
            var result = Search(benign, "alamat").Search!;
            Check(result is { Hits.Length: 2, PartialPages: 0 } && result.Hits.All(h => h.Boxes is { Length: 1 }), "opaque normal graphics state direct/indirect retains two exact positions " + indirect);
            Check(result.Hits[0].Boxes![0].Y < result.Hits[1].Boxes![0].Y && Math.Abs(result.Hits[0].Boxes![0].X * 595 - 124) < .001, "opaque state preserves separate label geometry " + indirect);
        }
        foreach (var graphics in new[] { "<< /ca 0 >>", "<< /ca .5 >>", "<< /CA .5 >>", "<< /BM /Multiply >>", "<< /SMask /None >>", "<< /TR /Identity >>", "<< /Unknown 1 >>", "<< /ca (1) >>", "<< /Type /Invalid >>", "42", "<< /Type /ExtGState /CA 1 /ca 1 /BM /Normal /LW 1 >>" })
        {
            var unsafePath = Fixture("unsafe-" + count, ["/GS gs " + labels], graphics: graphics);
            Check(Search(unsafePath, "alamat").Search is { Hits.Length: 2, PartialPages: 1 } unsafeResult && unsafeResult.Hits.All(h => h.Boxes is null), "unsupported graphics state retains text with no fabricated box " + count);
        }
        var missingState = Fixture("missing-state", ["/Missing gs " + labels]);
        Check(Search(missingState, "alamat").Search is { Hits.Length: 2, PartialPages: 1 } missingResult && missingResult.Hits.All(h => h.Boxes is null), "missing graphics state falls back even after Tf");
        var explicitWidths = "<< /Type /Font /Subtype /TrueType /BaseFont /ABCDEF+SyntheticBold /Encoding /WinAnsiEncoding /FirstChar 32 /LastChar 126 /Widths [" + string.Join(' ', Enumerable.Repeat("600", 95)) + "] /FontDescriptor << /Type /FontDescriptor /Ascent 750 /Descent -250 >> >>";
        var metricsState = Fixture("state-explicit-widths", ["/GS gs " + labels], font: explicitWidths, graphics: "<< /CA 1 /ca 1 /BM /Normal >>");
        var metricBox = Search(metricsState, "alamat").Search!.Hits[0].Boxes![0];
        Check(Math.Abs(metricBox.Width * 595 - 64.8) < .001 && Math.Abs(metricBox.Height * 842 - 18) < .001, "subset TrueType explicit widths and descriptor metrics survive opaque state");
        var fontState = Fixture("state-font", ["BT /F1 12 Tf 40 700 Td /GS gs (Limit) Tj ET BT /F1 12 Tf 40 600 Td (Limit) Tj ET"], graphics: "<< /Font [3 0 R 24] >>");
        Check(Search(fontState, "Limit").Search is { Hits.Length: 1, PartialPages: 1 } fontResult && fontResult.Hits[0].Boxes is null, "unsupported ExtGState font invalidates decoder until explicit font switch without repairing geometry");
        var restoredFont = Fixture("state-font-restore", ["BT /F1 12 Tf 40 700 Td q /GS gs (Hidden) Tj Q 1 0 0 1 40 600 Tm (Limit) Tj ET"], graphics: "<< /Font [3 0 R 24] >>");
        Check(Search(restoredFont, "Limit").Search is { Hits.Length: 1, PartialPages: 1 } restoredFontResult && restoredFontResult.Hits[0].Boxes is { Length: 1 }, "Q restores both font decoder and geometry after unsupported ExtGState font");
        var nested = Fixture("state-restore", ["BT /F1 12 Tf 40 700 Td q q /GS gs (Limit) Tj Q Q 0 -80 Td (Limit) Tj ET"], graphics: "<< /ca .5 >>");
        var restored = Search(nested, "Limit").Search!;
        Check(restored is { Hits.Length: 2, PartialPages: 1 } && restored.Hits[0].Boxes is null && restored.Hits[1].Boxes is { Length: 1 }, "nested q Q restores safe inherited graphics state");
        var inheritedUnsafe = Fixture("inherited-unsafe", ["1 .2 0 1 0 0 cm q /GS gs " + labels + " Q " + labels], graphics: "<< /ca 1 >>");
        Check(Search(inheritedUnsafe, "alamat").Search!.Hits.All(h => h.Boxes is null), "benign state and restore do not repair inherited unsafe transform");
        var clipping = Fixture("clip-state", ["0 0 10 10 re W n /GS gs " + labels], graphics: "<< /ca 1 >>");
        Check(Search(clipping, "alamat").Search!.Hits.All(h => h.Boxes is null), "benign state cannot override unproven clipping");
        foreach (var clipPath in new[] { "0 0 595 842 re W n", "q 0 0 595 842 re W* n 100 200 400 600 re W n Q", "q 0 0 10 10 re W n Q" })
        {
            var fullClip = Fixture("rect-clip-" + count, [clipPath + " /GS gs " + labels], graphics: "<< /ca 1 >>");
            Check(Search(fullClip, "alamat").Search!.Hits.All(h => h.Boxes is { Length: 1 }), "proven rectangle clipping and Q preserve contained labels " + count);
        }
        foreach (var clipPath in new[] { "0 0 m 595 842 l W n", "0 0 595 842 re W 0 0 m n", "0 0 595 842 re 0 0 10 10 re W* n" })
        {
            var unknownClip = Fixture("unknown-clip-" + count, [clipPath + " /GS gs " + labels], graphics: "<< /ca 1 >>");
            Check(Search(unknownClip, "alamat").Search!.Hits.All(h => h.Boxes is null), "complex or changed clip cannot fabricate box " + count);
        }
        var delayedClip = Fixture("delayed-clip", ["q 0 0 10 10 re W Q n /GS gs " + labels], graphics: "<< /ca 1 >>");
        Check(Search(delayedClip, "alamat").Search!.Hits.All(h => h.Boxes is null), "path-end clipping after Q remains effective because path is not saved graphics state");
        Check(Search(source, "missing").Search is { Hits.Length: 0, TextPages: 2 }, "no match distinguishes text from blank pages");
        Check(Search(source, "Pertamina\t OTO").Search is { Hits.Length: 1 }, "whitespace in phrase normalized");
        Check(Search(source, "A.*").Search is { Hits.Length: 0 }, "query is literal not regular expression");
        var operators = Fixture("operators", ["BT /F1 12 Tf 40 700 Td [(PER) 20 (TAMINA) -250 (OTO)] TJ T* (kedua) ' 0 0 (ketiga) \" ET"]);
        Check(Search(operators, "pertamina oto").Search is { Hits.Length: 1 }, "TJ kerning joins glyphs and inserts word gap");
        Check(Search(operators, "kedua ketiga").Search is { Hits.Length: 1 }, "single and double quote text operators searched");
        var latin = Fixture("winansi", ["BT /F1 12 Tf (Caf\u00e9 \u0080) Tj ET"], winAnsi);
        Check(Search(latin, "café €").Search is { Hits.Length: 1 }, "WinAnsi extended Latin and euro decoded");
        const string composite = "<< /Type /Font /Subtype /Type0 /BaseFont /Synthetic /Encoding /Identity-H /DescendantFonts [<< /Type /Font /Subtype /CIDFontType2 /BaseFont /Synthetic /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> >>] /ToUnicode 4 0 R >>";
        const string cmap = "begincmap 1 begincodespacerange <0000> <FFFF> endcodespacerange 4 beginbfchar <0001> <0041> <0002> <00660069> <0003> <4E2D> <0004> <D83DDE00> endbfchar 1 beginbfrange <0005> <0007> <0042> endbfrange 1 beginbfrange <0008> <0009> [<0065> <0066>] endbfrange endcmap";
        var unicode = Fixture("unicode", ["BT /F1 12 Tf 40 700 Td <000100020003000400050006000700080009> Tj ET"], composite, cmap);
        Check(Search(unicode, "Afi中😀BCDef").Search is { Hits.Length: 1, PartialPages: 0 }, "ToUnicode bfchar ranges arrays ligatures and surrogate pairs");
        var ligature = Search(unicode, "fi").Search!.Hits[0];
        Check(ligature.Boxes is { Length: 1 } && Math.Abs(ligature.Boxes[0].Width * 595 - 12) < .001, "ToUnicode multi-character ligature maps to one source glyph width");
        var missingGlyph = Fixture("unknown-code", ["BT /F1 12 Tf <000100FF0005> Tj ET"], composite, cmap);
        Check(Search(missingGlyph, "AB").Search is { Hits.Length: 0, PartialPages: 1 }, "unknown glyph blocks fabricated cross-gap match");
        var mixedWidths = Fixture("mixed-width", ["BT /F1 12 Tf <0001> Tj ET"], composite, "begincmap 2 beginbfchar <01> <0041> <0001> <0042> endbfchar endcmap");
        Check(Search(mixedWidths, "A").Search is { Hits.Length: 0, PartialPages: 1 }, "variable code width reported unsupported");
        var invalidUnicode = Fixture("invalid-unicode", ["BT /F1 12 Tf <0001> Tj ET"], composite, "begincmap 1 beginbfchar <0001> <D800> endbfchar endcmap");
        Check(Search(invalidUnicode, "x").Search is { Hits.Length: 0, PartialPages: 1 }, "invalid Unicode page does not claim complete coverage");
        var differences = Fixture("differences", ["BT /F1 12 Tf (ABC) Tj ET"], "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding << /BaseEncoding /WinAnsiEncoding /Differences [65 /Z] >> >>");
        Check(Search(differences, "ABC").Search is { Hits.Length: 0, PartialPages: 1 }, "custom font differences not guessed as ASCII");
        var gap = Fixture("form-gap", ["BT /F1 12 Tf (Alpha) Tj /Form Do (Beta) Tj ET"]);
        Check(Search(gap, "AlphaBeta").Search is { Hits.Length: 0, PartialPages: 1 } && Search(gap, "Alpha").Search!.Hits.Length == 1, "unsupported XObject disclosed while preserving supported text");
        var scan = Fixture("scan-only", ["/Scan Do"]);
        Check(Search(scan, "text").Search is { Hits.Length: 0, TextPages: 0, PartialPages: 1 }, "image-only page reports no extracted text");
        var capped = Fixture("many", ["BT /F1 12 Tf (" + string.Join(' ', Enumerable.Repeat("hit", 110)) + ") Tj ET"]);
        var cappedResult = Search(capped, "hit");
        Check(cappedResult.Search is { Hits.Length: 100, Truncated: true }, "large result set capped with explicit truncation");
        Check(JsonSerializer.Serialize(cappedResult).Length < 131072, "reply remains below client pipe limit");
        var only100 = Fixture("exact-cap", ["BT /F1 12 Tf (" + string.Join(' ', Enumerable.Repeat("hit", 100)) + ") Tj ET"]);
        Check(Search(only100, "hit").Search is { Hits.Length: 100, Truncated: false }, "exactly 100 hits is not incorrectly marked truncated");
        var deep = Fixture("deep-state", [string.Concat(Enumerable.Repeat("q ", 258)) + "BT /F1 12 Tf (text) Tj ET"]);
        Check(Search(deep, "text").Search is { PartialPages: 1 }, "graphics stack bounded");
        foreach (var query in new[] { "", " \t", new string('x', 201), "bad\0query" })
        {
            var rejected = false; try { Search(source, query); } catch (InputException) { rejected = true; }
            Check(rejected, "invalid query rejected");
        }
        var encrypted = Path.Combine(output, "encrypted.pdf");
        using (var pdf = new PdfDocument()) { pdf.AddPage(); pdf.SecuritySettings.OwnerPassword = "synthetic-owner"; pdf.SecuritySettings.UserPassword = "synthetic-user"; pdf.SecurityHandler.SetEncryptionToV5(); pdf.Save(encrypted); }
        var protectedRejected = false; try { Search(encrypted, "anything"); } catch (Exception) { protectedRejected = true; }
        Check(protectedRejected, "protected PDF cannot bypass credentials");
        var invalid = Path.Combine(output, "invalid.pdf"); File.WriteAllText(invalid, "not a PDF");
        var invalidRejected = false; try { Search(invalid, "anything"); } catch (Exception) { invalidRejected = true; }
        Check(invalidRejected, "corrupt PDF rejected");
        var sentinel = Path.Combine(output, "existing.txt"); File.WriteAllText(sentinel, "keep");
        PdfOperations.Execute(new("reader-search", [source], sentinel, Text: "alpha"));
        Check(File.ReadAllText(sentinel) == "keep" && hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "read-only search preserves source bytes and existing output path");
        Console.WriteLine($"Search tests: {count} PASS"); return 0;
    }

    private static void WritePdf(string path, string[] streams, string font, string? cmap, string? graphics, bool indirect)
    {
        var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "", font };
        if (cmap is not null) objects.Add($"<< /Length {Encoding.Latin1.GetByteCount(cmap)} >>\nstream\n{cmap}\nendstream");
        var graphicsResource = "";
        if (graphics is not null)
        {
            if (indirect) { objects.Add(graphics); graphics = (objects.Count).ToString(System.Globalization.CultureInfo.InvariantCulture) + " 0 R"; }
            graphicsResource = " /ExtGState << /GS " + graphics + " >>";
        }
        var pages = new List<int>();
        foreach (var stream in streams)
        {
            pages.Add(objects.Count + 1);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >>{graphicsResource} >> /Contents {objects.Count + 2} 0 R >>");
            objects.Add($"<< /Length {Encoding.Latin1.GetByteCount(stream)} >>\nstream\n{stream}\nendstream");
        }
        objects[1] = $"<< /Type /Pages /Kids [{string.Join(' ', pages.Select(p => p + " 0 R"))}] /Count {pages.Count} >>";
        var pdf = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (var i = 0; i < objects.Count; i++) { offsets.Add(Encoding.Latin1.GetByteCount(pdf.ToString())); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        var xref = Encoding.Latin1.GetByteCount(pdf.ToString()); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
        pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n"); File.WriteAllBytes(path, Encoding.Latin1.GetBytes(pdf.ToString()));
    }
}
