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
        Check(Runs(Fixture("dependent", tail: "(FOLLOW) Tj ET")).Length == 0, "dependent horizontal text advance rejected");
        Check(Runs(Fixture("line-follow", tail: "T* (FOLLOW) Tj ET")).Length == 0, "nonterminal line advance remains conservative");
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
