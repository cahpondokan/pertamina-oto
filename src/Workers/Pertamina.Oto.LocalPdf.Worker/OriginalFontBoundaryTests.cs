using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal static class OriginalFontBoundaryTests
{
    internal static int Run(string directory)
    {
        var count = 0;
        void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); count++; Console.WriteLine("PASS " + label); }
        string Fixture(string name, string fontName = "Helvetica", string tail = "T* ET", Action<PdfDictionary>? change = null)
        {
            var path = Path.Combine(directory, name + ".pdf");
            using var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var font = new PdfDictionary(pdf);
            font.Elements.SetName("/Type", "/Font"); font.Elements.SetName("/Subtype", "/Type1");
            font.Elements.SetName("/BaseFont", "/" + fontName); font.Elements.SetName("/Encoding", "/WinAnsiEncoding");
            change?.Invoke(font);
            var fonts = new PdfDictionary(pdf); fonts.Elements["/F1"] = font;
            page.Resources.Elements["/Font"] = fonts;
            page.Contents.ReplaceContent(ContentReader.ReadContent(Encoding.ASCII.GetBytes("BT /F1 12 Tf 1 0 0 1 40 700 Tm (BUDI SANTOSO) Tj " + tail)));
            pdf.Save(path); return path;
        }
        EditorTextRun[] Runs(string file) => PdfOperations.Execute(new("edit-text-check", [file], "unused", Rotation: 0)).TextRuns!;
        foreach (var name in new[] { "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique", "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic", "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique" })
        {
            var source = Fixture("standard-" + name, name); var runs = Runs(source);
            Check(runs.Length == 1 && runs[0].Text == "BUDI SANTOSO", "standard WinAnsi and terminal T-star: " + name);
            var run = runs[0]; var output = Path.Combine(directory, "changed-" + name + ".pdf");
            PdfOperations.Execute(new("edit", [source], output, Marks: [new(0, "replace-text", run.X, run.Y, run.Width, run.Height, "BUDI", run.Size, SourceId: run.Id, OriginalText: run.Text)]));
            Check(Runs(output) is [var edited] && edited.Text == "BUDI" && edited.Font == run.Font && edited.X == run.X && edited.Y == run.Y, "standard font original replacement reopens: " + name);
        }
        foreach (var name in new[] { "Symbol", "ZapfDingbats", "CustomFont", "AAAAAA+Helvetica" })
            Check(Runs(Fixture("reject-" + name, name)).Length == 0, "nonstandard Type1 rejected: " + name);
        Check(Runs(Fixture("custom-widths", change: f => f.Elements["/Widths"] = new PdfArray())).Length == 0, "standard font override is not assigned built-in metrics");
        Check(Runs(Fixture("missing-encoding", change: f => f.Elements.Remove("/Encoding"))).Length == 0, "standard built-in encoding not guessed as WinAnsi");
        Check(Runs(Fixture("encoding-dictionary", change: f => f.Elements["/Encoding"] = new PdfDictionary())).Length == 0, "custom encoding dictionary rejected without reader exception");
        var adjacent = Fixture("dependent", tail: "(FOLLOW) Tj ET");
        var adjacentRuns = Runs(adjacent);
        Check(adjacentRuns.Length == 2 && adjacentRuns[1].X > adjacentRuns[0].X, "adjacent fragments track original text advance");
        var multiline = Fixture("line-follow", tail: "16 TL T* (FOLLOW) Tj T* (LAST) Tj ET");
        var lineRuns = Runs(multiline);
        Check(lineRuns.Length == 3 && lineRuns[1].X == lineRuns[0].X && lineRuns[1].Y == lineRuns[0].Y + 16 && lineRuns[2].Y == lineRuns[1].Y + 16, "T-star follows leading and original line origin");
        foreach (var source in new[] { adjacent, multiline })
        {
            var before = Runs(source); var run = before[0];
            foreach (var local in new[] { "", "Arial" })
            foreach (var text in new[] { "BUDI", "" })
            {
                var output = Path.Combine(directory, "position-preserved-" + count + ".pdf");
                PdfOperations.Execute(new("edit", [source], output, Marks: [new(0, "replace-text", run.X, run.Y, run.Width, run.Height, text, run.Size, SourceId: run.Id, OriginalText: run.Text, LocalFont: local)]));
                var after = Runs(output);
                Check(before.Skip(1).All(b => after.Any(a => a.Text == b.Text && Math.Abs(a.X - b.X) < .001 && Math.Abs(a.Y - b.Y) < .001 && a.Font == b.Font)), "shorter/deleted original and local replacements retain following fragments and lines");
            }
        }
        var relative = Runs(Fixture("relative-line", tail: "20 -16 TD (FOLLOW) Tj T* (LAST) Tj ET"));
        Check(relative.Length == 3 && relative[1].X == relative[0].X + 20 && relative[2].X == relative[1].X && relative[2].Y == relative[1].Y + 16, "TD uses line matrix and establishes subsequent leading");
        var spaced = Runs(Fixture("word-spaced", tail: "0 -16 Td 3 Tw (BUDI SANTOSO) Tj ET"));
        Check(spaced.Length == 2 && Math.Abs(spaced[1].Width - spaced[0].Width - 3) < .001, "single-byte space width includes Tw");
        Check(Runs(Fixture("rotated-text", tail: "1 1 0 1 40 600 Tm (FOLLOW) Tj ET")).Length == 1, "unsupported skew stays excluded after line support");
        var mixed = Fixture("mixed-adjacent", tail: "/F2 10 Tf (FOLLOW) Tj 14 TL T* (LAST) Tj ET");
        var mixedSource = Path.Combine(directory, "mixed-font-source.pdf");
        using (var pdf = PdfReader.Open(mixed, PdfDocumentOpenMode.Modify))
        {
            var f2 = new PdfDictionary(pdf); f2.Elements.SetName("/Type", "/Font"); f2.Elements.SetName("/Subtype", "/Type1");
            f2.Elements.SetName("/BaseFont", "/Times-Bold"); f2.Elements.SetName("/Encoding", "/WinAnsiEncoding");
            pdf.Pages[0].Resources.Elements.GetDictionary("/Font")!.Elements["/F2"] = f2;
            pdf.Save(mixedSource);
        }
        var mixedRuns = Runs(mixedSource);
        var mixedRun = mixedRuns[0]; var mixedOutput = Path.Combine(directory, "mixed-local-edited.pdf");
        PdfOperations.Execute(new("edit", [mixedSource], mixedOutput, Marks: [new(0, "replace-text", mixedRun.X, mixedRun.Y, mixedRun.Width, mixedRun.Height, "BUDI", mixedRun.Size, SourceId: mixedRun.Id, OriginalText: mixedRun.Text, LocalFont: "Arial")]));
        var mixedAfter = Runs(mixedOutput);
        Check(mixedRuns.Length == 3 && mixedAfter.Length == 3 && mixedRuns.Skip(1).All(b => mixedAfter.Any(a => a.Text == b.Text && Math.Abs(a.X - b.X) < .001 && Math.Abs(a.Y - b.Y) < .001 && a.Font == b.Font)), "local replacement preserves mixed-font neighbor and original following line when old font has no glyphs left");
        var mapped = Fixture("mapped-simple", change: f =>
        {
            f.Elements.SetName("/Subtype", "/TrueType"); f.Elements.Remove("/Encoding");
            f.Elements.SetInteger("/FirstChar", 32);
            var widths = new PdfArray(); for (var i = 32; i <= 126; i++) widths.Elements.Add(new PdfInteger(600));
            f.Elements["/Widths"] = widths;
            var descriptor = new PdfDictionary(); var program = new PdfDictionary(f.Owner); program.CreateStream([0, 1, 0, 0]);
            f.Owner.Internals.AddObject(program);
            descriptor.Elements["/FontFile2"] = program.Reference!; f.Elements["/FontDescriptor"] = descriptor;
            var cmap = new PdfDictionary(f.Owner); cmap.CreateStream(Encoding.ASCII.GetBytes("1 beginbfrange <20> <7E> <0020> endbfrange"));
            f.Owner.Internals.AddObject(cmap); f.Elements["/ToUnicode"] = cmap.Reference!;
        });
        // Synthetic font resource tests exercise decoding only; no claim it is a renderable TTF.
        Check(Runs(mapped).Length == 1, "embedded simple font code mapping admitted without encoding substitution");
        foreach (var kind in new[] { "no-map", "no-program", "no-widths", "duplicate-code", "duplicate-unicode" })
        {
            var path = Path.Combine(directory, "mapped-" + kind + ".pdf");
            using (var pdf = PdfReader.Open(mapped, PdfDocumentOpenMode.Modify))
            {
                var f = pdf.Pages[0].Resources.Elements.GetDictionary("/Font")!.Elements.GetDictionary("/F1")!;
                if (kind == "no-map") f.Elements.Remove("/ToUnicode");
                if (kind == "no-program") f.Elements.Remove("/FontDescriptor");
                if (kind == "no-widths") f.Elements.Remove("/Widths");
                if (kind.StartsWith("duplicate-", StringComparison.Ordinal))
                    f.Elements.GetDictionary("/ToUnicode")!.Stream.Value = Encoding.ASCII.GetBytes(kind == "duplicate-code"
                        ? "2 beginbfchar <42> <0042> <42> <0043> endbfchar 1 beginbfrange <20> <7E> <0020> endbfrange"
                        : "2 beginbfchar <42> <0055> <55> <0055> endbfchar");
                pdf.Save(path);
            }
            Check(Runs(path).Length == 0, "ambiguous/incomplete simple font rejected: " + kind);
        }
        return count;
    }
}
