using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Pertamina.Oto.LocalPdf;

internal static class WorkerTests
{
    internal static int Run()
    {
        // Synthetic-only smoke tests. Keep artifacts for independent review.
        var root = Path.Combine(Path.GetTempPath(), "oto-functional-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var first = Path.Combine(root, "source.pdf");
        using (var document = new PdfDocument())
        {
            for (var i = 0; i < 3; i++)
                document.AddPage().Width = PdfSharp.Drawing.XUnit.FromPoint(400 + i * 10);
            document.Save(first);
        }
        var hash = SHA256.HashData(File.ReadAllBytes(first));
        var tests = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("FAIL " + name);
            Console.WriteLine("PASS " + name);
            tests++;
        }
        string Output(string name) => Path.Combine(root, name + ".pdf");
        PdfReply Run(PdfRequest request) => PdfOperations.Execute(request);
        Check(Run(new("merge", [first, first], Output("merge"))).Pages == 6, "merge repeated source: six pages");
        Check(Run(new("merge", [first, first], Output("merge-rotated"), FileRotations: [90, 270])).Pages == 6, "merge rotated sources");
        using (var merged = PdfReader.Open(Output("merge-rotated"), PdfDocumentOpenMode.Import))
            Check(Enumerable.Range(0, 6).All(i => merged.Pages[i].Rotate == (i < 3 ? 90 : 270)), "merge rotates every page in each source");
        foreach (var angles in new int[][] { [90], [45, 0] })
        {
            var denied = false;
            try { Run(new("merge", [first, first], Output("bad-merge-angle"), FileRotations: angles)); }
            catch (InputException) { denied = true; }
            Check(denied && !File.Exists(Output("bad-merge-angle")), "invalid merge rotations rejected");
        }
        Check(Run(new("extract", [first], Output("extract"), "3,1")).Pages == 2, "extract selected pages");
        using (var extracted = PdfReader.Open(Output("extract"), PdfDocumentOpenMode.Import))
            Check(extracted.Pages[0].Width.Point == 420 && extracted.Pages[1].Width.Point == 400, "extract order");
        Check(Run(new("remove", [first], Output("remove"), "2")).Pages == 2, "remove page");
        Check(Run(new("organize", [first], Output("organize"), "3,1,1")).Pages == 3, "organize including duplicate");
        Check(Run(new("organize", [first, first], Output("layout"), Layout: [new(1, 2, 90), new(0, 0), new(1, 2, 180)])).Pages == 3, "visual layout multi-source duplicate");
        using (var arranged = PdfReader.Open(Output("layout"), PdfDocumentOpenMode.Import))
            Check(arranged.Pages[0].Width.Point == 420 && arranged.Pages[1].Width.Point == 400
                && arranged.Pages[0].Rotate == 90 && arranged.Pages[2].Rotate == 180, "visual layout order and independent rotations");
        Check(Run(new("organize", [first, first, first], Output("blank-layout"), Layout: [new(2, 2, 270), new(-1, -1, 90), new(0, 0)])).Pages == 3, "organizer three sources and blank insertion");
        using (var blankLayout = PdfReader.Open(Output("blank-layout"), PdfDocumentOpenMode.Import))
        {
            Check(blankLayout.Pages[0].Width.Point == 420 && blankLayout.Pages[0].Rotate == 270 && blankLayout.Pages[2].Width.Point == 400, "organizer reordered sources preserve geometry");
            Check(Math.Abs(blankLayout.Pages[1].Width.Point - 595) < 2 && Math.Abs(blankLayout.Pages[1].Height.Point - 842) < 2 && blankLayout.Pages[1].Rotate == 90, "blank A4 dimensions and rotation");
        }
        Check(Run(new("organize", [first], Output("only-blank"), Layout: [new(-1, -1)])).Pages == 1, "remove all originals and retain blank");
        foreach (var invalidLayout in new PagePlacement[][] { [], [new(2, 0)], [new(0, -1)], [new(0, 3)], [new(0, 0, 45)], [new(-1, 0)], [new(-1, -1, 45)], [new(-2, -1)] })
        {
            var deniedLayout = false;
            try { Run(new("organize", [first], Output("invalid-layout"), Layout: invalidLayout)); }
            catch (InputException) { deniedLayout = true; }
            Check(deniedLayout && !File.Exists(Output("invalid-layout")), "invalid visual layout rejected without output");
        }
        Run(new("rotate", [first], Output("rotate"), "2", Rotation: 90));
        using (var rotated = PdfReader.Open(Output("rotate"), PdfDocumentOpenMode.Import))
            Check(rotated.Pages[0].Rotate == 0 && rotated.Pages[1].Rotate == 90, "rotate only selected page");
        Run(new("crop", [first], Output("crop"), "1", Margin: 20));
        using (var cropped = PdfReader.Open(Output("crop"), PdfDocumentOpenMode.Import))
            Check(cropped.Pages[0].CropBox.Width == 360, "crop geometry");
        Check(Run(new("numbers", [first], Output("numbers"))).Pages == 3, "page numbers save/reopen");
        Check(Run(new("watermark", [first], Output("watermark"), Text: "SYNTHETIC")).Pages == 3, "watermark save/reopen");
        Check(Run(new("compress", [first], Output("compress"))).Pages == 3, "lossless compression save/reopen");
        Run(new("protect", [first], Output("protect"), NewPassword: "synthetic-only-password"));
        var rejected = false;
        try { using var denied = PdfReader.Open(Output("protect"), "wrong", PdfDocumentOpenMode.Import); }
        catch (PdfReaderException) { rejected = true; }
        Check(rejected, "protected result rejects wrong password");
        Check(Run(new("unlock", [Output("protect")], Output("unlock"), Password: "synthetic-only-password")).Pages == 3, "unlock with authorized password");
        using (var unlocked = PdfReader.Open(Output("unlock"), PdfDocumentOpenMode.Import))
            Check(unlocked.PageCount == 3, "unlocked opens without password");
        var zipPath = Path.Combine(root, "split.zip");
        Run(new("split", [first], zipPath));
        using (var zip = ZipFile.OpenRead(zipPath)) Check(zip.Entries.Count == 3, "split ZIP contains three PDFs");
        rejected = false;
        try { Run(new("remove", [first], Output("empty"), "1-3")); }
        catch (InputException) { rejected = true; }
        Check(rejected && !File.Exists(Output("empty")), "remove all rejected without output");
        rejected = false;
        try { Run(new("compress", [first], first)); }
        catch (InputException) { rejected = true; }
        Check(rejected, "source overwrite rejected");
        foreach (var invalid in new[] { "0", "4", "2-1", "1,1", "a", "1-999999999" })
        {
            rejected = false;
            try { PdfOperations.ParsePages(invalid, 3); }
            catch (InputException) { rejected = true; }
            Check(rejected, "invalid selection " + invalid);
        }
        Check(hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(first))), "source SHA256 unchanged");
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{tests} checks passed. Synthetic artifacts: {root}"));
        return 0;
    }
}
