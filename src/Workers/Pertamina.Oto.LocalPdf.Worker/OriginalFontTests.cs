using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal static class OriginalFontTests
{
    internal static int Run(string source, string directory)
    {
        var count = 0;
        void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); count++; Console.WriteLine("PASS " + name); }
        var tjSource = Path.Combine(directory, "tj-source.pdf");
        using (var pdf = PdfReader.Open(source, PdfDocumentOpenMode.Modify))
        {
            var content = ContentReader.ReadContent(pdf.Pages[0]); var changed = new CSequence();
            foreach (var item in content)
            {
                if (item is COperator { Name: "Tj" } op && op.Operands[0] is CString str)
                {
                    var arrayOp = (COperator)ContentReader.ReadContent(System.Text.Encoding.ASCII.GetBytes("[] TJ\n"))[0];
                    var array = (CArray)arrayOp.Operands[0];
                    array.Add(new CString { Value = str.Value[..2], CStringType = CStringType.String });
                    array.Add(new CInteger { Value = 20 });
                    array.Add(new CString { Value = str.Value[2..], CStringType = CStringType.String });
                    changed.Add(arrayOp);
                }
                else changed.Add(item);
            }
            pdf.Pages[0].Contents.ReplaceContent(changed); pdf.Save(tjSource);
        }
        var runs = PdfOperations.Execute(new("edit-text-check", [tjSource], "unused", Rotation: 0)).TextRuns!;
        Check(runs.Length == 2 && runs[0].Text == "PERTAMINA OTO - EDITOR TEST", "TJ strings and kerning decode as one editable run");
        var run = runs[0];
        var mark = new EditorMark(0, "replace-text", run.X, run.Y, run.Width, run.Height, "PERTAMINA OTO", run.Size, SourceId: run.Id, OriginalText: run.Text);
        var subsetOutput = Path.Combine(directory, "tj-subset.pdf");
        PdfOperations.Execute(new("edit", [tjSource], subsetOutput, Marks: [mark]));
        var subsetRuns = PdfOperations.Execute(new("edit-text-check", [subsetOutput], "unused", Rotation: 0)).TextRuns!;
        // Characters describes the page-wide used-glyph inventory, which legitimately shrinks after replacement.
        Check(subsetRuns[0].Text == mark.Text && subsetRuns[1] with { Characters = runs[1].Characters } == runs[1], "TJ replacement preserves all neighboring layout/text/font fields");
        using (var before = PdfReader.Open(tjSource, PdfDocumentOpenMode.Modify))
        using (var after = PdfReader.Open(subsetOutput, PdfDocumentOpenMode.Modify))
            Check(ContentReader.ReadContent(before.Pages[0])[runs[1].Id].ToString() == ContentReader.ReadContent(after.Pages[0])[runs[1].Id].ToString(), "untouched TJ operand and kerning remain identical");
        foreach (var face in LocalEditorFonts.Available())
        {
            var target = Path.Combine(directory, "local-font-" + face.Replace(' ', '-') + ".pdf");
            PdfOperations.Execute(new("edit", [tjSource], target, Marks: [mark with { Text = "Zqx 789", LocalFont = face }]));
            var edited = PdfOperations.Execute(new("edit-text-check", [target], "unused", Rotation: 0)).TextRuns!;
            Check(edited.Any(r => r.Text == "Zqx 789") && !edited.Any(r => r.Text == run.Text) && edited.Any(r => r.Text == runs[1].Text && r.X == runs[1].X && r.Y == runs[1].Y && r.Font == runs[1].Font), "new characters using explicit local font: " + face);
        }
        foreach (var invalid in new[] { mark with { LocalFont = "../times.ttf" }, mark with { LocalFont = "Missing Font" }, mark with { LocalFont = "Arial", Text = new string('W', 200) }, mark with { LocalFont = "Arial", Text = "Ω" } })
        {
            var failed = false; var output = Path.Combine(directory, "rejected-local-" + count + ".pdf");
            try { PdfOperations.Execute(new("edit", [tjSource], output, Marks: [invalid])); } catch (InputException) { failed = true; }
            Check(failed && !File.Exists(output), "invalid local font/text rejected without publication");
        }
        var fittedOutput = Path.Combine(directory, "fitted-local.pdf");
        PdfOperations.Execute(new("edit", [tjSource], fittedOutput, Marks: [mark with { LocalFont = "Arial", Text = new string('W', 30), FitText = true }]));
        var fitted = PdfOperations.Execute(new("edit-text-check", [fittedOutput], "unused", Rotation: 0)).TextRuns!.First();
        Check(fitted.Text == new string('W', 30) && fitted.Width <= run.Width + .1 && fitted.Size < run.Size && fitted.Size >= 6, "explicit fit option shrinks text within original region");
        var tinyRejected = false; var tinyOutput = Path.Combine(directory, "tiny-rejected.pdf");
        try { PdfOperations.Execute(new("edit", [tjSource], tinyOutput, Marks: [mark with { LocalFont = "Arial", Text = new string('W', 200), FitText = true }])); } catch (InputException) { tinyRejected = true; }
        Check(tinyRejected && !File.Exists(tinyOutput), "fit rejects unreadably small text without output");
        return count;
    }
}
