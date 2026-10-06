using System.Security.Cryptography;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal static class EditorTests
{
    internal static int Run()
    {
        LocalEditorFonts.Initialize();
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        var directory = Path.Combine(Path.GetTempPath(), "oto-editor-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var source = Path.Combine(directory, "source.pdf");
        using (var pdf = new PdfDocument())
        {
            for (var i = 0; i < 2; i++)
            {
                var page = pdf.AddPage(); page.Width = XUnit.FromPoint(595); page.Height = XUnit.FromPoint(842);
                using var graphics = XGraphics.FromPdfPage(page);
                graphics.DrawString(i == 0 ? "PERTAMINA OTO - EDITOR TEST" : "CONTROL PAGE - UNCHANGED", new XFont("Arial", 20), XBrushes.Black, 40, 60);
                graphics.DrawString("Original source content stays here.", new XFont("Arial", 12), XBrushes.Black, 40, 95);
            }
            pdf.Save(source);
        }
        var hash = SHA256.HashData(File.ReadAllBytes(source)); var count = 0;
        void Check(bool success, string label) { if (!success) throw new InvalidOperationException(label); Console.WriteLine("PASS " + label); count++; }
        var inspection = PdfOperations.Execute(new("edit-check", [source], Path.Combine(directory, "not-created.pdf")));
        Check(inspection.PageSizes is { Length: 2 } && inspection.PageSizes[0].Width == 595 && !File.Exists(Path.Combine(directory, "not-created.pdf")), "inspection point dimensions without output mutation");
        EditorMark[] marks = [new(0, "text", 40, 140, 300, 35, "EDITOR NEW TEXT", 16), new(0, "rectangle", 40, 200, 180, 80, Size: 3, Color: "#1668D4"), new(0, "ellipse", 250, 200, 120, 80, Size: 3), new(0, "line", 40, 340, 200, 40, Size: 2), new(0, "pen", 40, 430, 160, 40, Size: 3, Points: [new(0, 30), new(40, 0), new(80, 35), new(160, 10)])];
        var output = Path.Combine(directory, "edited.pdf");
        Check(PdfOperations.Execute(new("edit", [source], output, Marks: marks)).Pages == 2, "text shapes stroke output reopens with two pages");
        EditorMark[] drawnShapes = [new(0, "rectangle", 40, 140, 180, 100, Size: 2), new(0, "ellipse", 280, 140, 100, 100, Size: 2),
            new(0, "line", 40, 300, 180, 4, Size: 2, Points: [new(0, 0), new(180, 0)]),
            new(0, "line", 280, 300, 4, 100, Size: 2, Points: [new(0, 0), new(0, 100)]),
            new(0, "line", 40, 450, 180, 100, Size: 2, Points: [new(0, 100), new(180, 0)])];
        Check(PdfOperations.Execute(new("edit", [source], Path.Combine(directory, "drawn-shapes.pdf"), Marks: drawnShapes)).Pages == 2, "dragged shapes and directional lines save/reopen");
        foreach (var invalidPoints in new EditorPoint[][] { [new(0, 0)], [new(0, 0), new(300, 0)], [new(double.NaN, 0), new(100, 0)] })
        {
            var rejected = false; var bad = Path.Combine(directory, "invalid-line-" + count + ".pdf");
            try { PdfOperations.Execute(new("edit", [source], bad, Marks: [drawnShapes[2] with { Points = invalidPoints }])); } catch (InputException) { rejected = true; }
            Check(rejected && !File.Exists(bad), "invalid directional line rejected without output");
        }
        var invalid = new[] { marks[0] with { X = -1 }, marks[0] with { Text = "\u03A9" }, marks[0] with { Width = 2 }, marks[0] with { Page = 2 }, marks[0] with { Kind = "original-text" }, marks[0] with { X = double.NaN }, marks[0] with { Kind = "image", ImageIndex = 1 }, marks[0] with { Color = "bad" }, marks[4] with { Points = [new(0, 0), new(200, 0)] } };
        foreach (var mark in invalid)
        {
            var target = Path.Combine(directory, "invalid-" + count + ".pdf"); var rejected = false;
            try { PdfOperations.Execute(new("edit", [source], target, Marks: [mark])); } catch (InputException) { rejected = true; }
            Check(rejected && !File.Exists(target), "invalid edit rejected before file publication");
        }
        foreach (var kind in new[] { "rotate", "crop", "protect" })
        {
            var incompatible = Path.Combine(directory, kind + ".pdf"); PdfOperations.Execute(new(kind, [source], incompatible, NewPassword: "test-only"));
            var rejected = false;
            try { PdfOperations.Execute(new("edit", [incompatible], Path.Combine(directory, kind + "-edit.pdf"), Password: kind == "protect" ? "test-only" : "", Marks: marks)); }
            catch (InputException) { rejected = true; }
            Check(rejected, "unsupported " + kind + " document rejected");
        }
        var denied = false;
        try { PdfOperations.Execute(new("edit", [source], source, Marks: marks)); } catch (InputException) { denied = true; }
        Check(denied && hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "source overwrite denied and hash unchanged");
        var runs = PdfOperations.Execute(new("edit-text-check", [source], Path.Combine(directory, "inspect-only.pdf"), Rotation: 0)).TextRuns!;
        Check(runs.Length == 2 && runs[0].Text == "PERTAMINA OTO - EDITOR TEST" && runs[0].X == 40, "original text discovered with correct coordinates");
        var run = runs[0];
        var replacement = new EditorMark(0, "replace-text", run.X, run.Y, run.Width, run.Height, "PERTAMINA OTO", run.Size, SourceId: run.Id, OriginalText: run.Text);
        var replacedPath = Path.Combine(directory, "original-replaced.pdf");
        PdfOperations.Execute(new("edit", [source], replacedPath, Marks: [replacement]));
        var replacedRuns = PdfOperations.Execute(new("edit-text-check", [replacedPath], Path.Combine(directory, "inspect-after.pdf"), Rotation: 0)).TextRuns!;
        Check(replacedRuns[0].Text == "PERTAMINA OTO" && replacedRuns[1].Text == runs[1].Text && replacedRuns[1].X == runs[1].X && replacedRuns[1].Y == runs[1].Y, "original operand replaced while neighbor positioning retained");
        var removedPath = Path.Combine(directory, "original-removed.pdf");
        PdfOperations.Execute(new("edit", [source], removedPath, Marks: [replacement with { Text = "" }]));
        Check(PdfOperations.Execute(new("edit-text-check", [removedPath], Path.Combine(directory, "inspect-deleted.pdf"), Rotation: 0)).TextRuns!.Length == 1, "original text deletion removes operand content");
        foreach (var invalidReplacement in new[] { replacement with { Text = "Ω" }, replacement with { Text = new string('M', 100) }, replacement with { OriginalText = "stale" }, replacement with { SourceId = 9999 }, replacement with { Page = 99 } })
        {
            var badOutput = Path.Combine(directory, "invalid-original-" + count + ".pdf"); var rejected = false;
            try { PdfOperations.Execute(new("edit", [source], badOutput, Marks: [invalidReplacement])); } catch (InputException) { rejected = true; }
            Check(rejected && !File.Exists(badOutput), "unsafe original replacement rejected without output");
        }
        var duplicateDenied = false;
        try { PdfOperations.Execute(new("edit", [source], Path.Combine(directory, "duplicate-original.pdf"), Marks: [replacement, replacement])); } catch (InputException) { duplicateDenied = true; }
        Check(duplicateDenied, "duplicate original operand edits rejected");
        Check(hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "original source hash unchanged after text edits");
        var unicodeSource = Path.Combine(directory, "unicode-source.pdf");
        using (var pdf = new PdfDocument())
        {
            for (var i = 0; i < 2; i++)
            {
                var unicodePage = pdf.AddPage(); unicodePage.Width = XUnit.FromPoint(595); unicodePage.Height = XUnit.FromPoint(842);
                using var graphics = XGraphics.FromPdfPage(unicodePage);
                var font = new XFont("Arial", 20, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
                graphics.DrawString(i == 0 ? "PERTAMINA OTO - EDITOR TEST" : "CONTROL PAGE - UNCHANGED", font, XBrushes.Black, 40, 60);
                graphics.DrawString("Original source content stays here.", new XFont("Arial", 12, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode)), XBrushes.Black, 40, 95);
            }
            pdf.Save(unicodeSource);
        }
        var unicodeRuns = PdfOperations.Execute(new("edit-text-check", [unicodeSource], Path.Combine(directory, "cid-inspect.pdf"), Rotation: 0)).TextRuns!;
        Check(unicodeRuns.Length == 2 && unicodeRuns[0].Text == run.Text, "CID Identity-H text decoded through ToUnicode mapping");
        var unicodeOutput = Path.Combine(directory, "unicode-replaced.pdf");
        PdfOperations.Execute(new("edit", [unicodeSource], unicodeOutput, Marks: [replacement with { SourceId = unicodeRuns[0].Id }]));
        var unicodeResult = PdfOperations.Execute(new("edit-text-check", [unicodeOutput], Path.Combine(directory, "cid-verify.pdf"), Rotation: 0)).TextRuns!;
        Check(unicodeResult[0].Text == "PERTAMINA OTO" && unicodeResult[1].Text == unicodeRuns[1].Text, "CID replacement retains glyph encoding and neighbor text");
        foreach (var unsupported in new[] { "hidden", "skew", "tagged", "missing-cmap" })
        {
            var unsupportedPath = Path.Combine(directory, unsupported + ".pdf");
            using (var pdf = PdfReader.Open(unsupported == "missing-cmap" ? unicodeSource : source, PdfDocumentOpenMode.Modify))
            {
                var firstPage = pdf.Pages[0];
                if (unsupported == "missing-cmap")
                {
                    var fonts = firstPage.Resources.Elements.GetDictionary("/Font")!;
                    foreach (var fontName in fonts.Elements.Keys) fonts.Elements.GetDictionary(fontName)!.Elements.Remove("/ToUnicode");
                }
                else
                {
                    var prefix = unsupported == "hidden" ? "3 Tr\n" : unsupported == "skew" ? "1 0.2 0 1 0 0 cm\n" : "/Span BMC\n";
                    var content = firstPage.Contents.CreateSingleContent().Stream.UnfilteredValue;
                    firstPage.Contents.ReplaceContent(PdfSharp.Pdf.Content.ContentReader.ReadContent(System.Text.Encoding.ASCII.GetBytes(prefix).Concat(content).Concat(System.Text.Encoding.ASCII.GetBytes(unsupported == "tagged" ? "\nEMC\n" : "\n")).ToArray()));
                }
                pdf.Save(unsupportedPath);
            }
            Check(PdfOperations.Execute(new("edit-text-check", [unsupportedPath], Path.Combine(directory, unsupported + "-inspect.pdf"), Rotation: 0)).TextRuns!.Length == 0, "unsupported original layout rejected: " + unsupported);
        }
        count += OriginalFontTests.Run(unicodeSource, directory);
        count += OriginalFontBoundaryTests.Run(directory);
        Console.WriteLine($"{count} editor checks passed. Evidence: {directory}");
        return 0;
    }
}
