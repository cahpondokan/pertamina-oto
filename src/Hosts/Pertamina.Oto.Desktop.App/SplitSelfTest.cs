using System.Security.Cryptography;
using System.Text;
using Windows.Data.Pdf;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal static class SplitSelfTest
{
    private static readonly int[] FixedSizes = [3, 3, 2];
    internal static async Task RunAsync()
    {
        var report = new List<string>();
        var directory = Path.Combine(AppContext.BaseDirectory, "split-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        void Reject(Action action, string label)
        {
            try { action(); } catch (ArgumentException) { report.Add("PASS " + label); return; }
            throw new InvalidOperationException(label);
        }
        try
        {
            var source = Path.Combine(directory, "Contoh-8-halaman.pdf"); CreateSample(source);
            var sourcePdf = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(source));
            var hash = SHA256.HashData(File.ReadAllBytes(source));
            var ranges = new[] { new SplitRange(1, 3), new SplitRange(5, 7) };
            var custom = SplitPlan.Create(8, SplitMode.Custom, ranges, 3, "", false);
            Check(custom.Length == 2 && custom[1].SequenceEqual(new[] { 5, 6, 7 }), "custom groups omit unselected pages");
            var fixedPlan = SplitPlan.Create(8, SplitMode.Fixed, ranges, 3, "", false);
            Check(fixedPlan.Select(group => group.Length).SequenceEqual(FixedSizes), "fixed groups keep remainder");
            Check(SplitPlan.Create(8, SplitMode.AllPages, ranges, 3, "", false).Length == 8, "extract all produces eight groups");
            Check(SplitPlan.ParseSelection("7, 1, 3–5, 3", 8).SequenceEqual(new[] { 1, 3, 4, 5, 7 }), "selection normalized and deduplicated in source order");
            var combined = SplitPlan.Create(8, SplitMode.Custom, ranges, 3, "", true);
            Check(combined.Length == 1 && combined[0].SequenceEqual(new[] { 1, 2, 3, 5, 6, 7 }), "combined custom preserves range order");
            Reject(() => SplitPlan.ParseSelection("0", 8), "reject page zero");
            Reject(() => SplitPlan.ParseSelection("9", 8), "reject missing page");
            Reject(() => SplitPlan.ParseSelection("3-2", 8), "reject reversed range");
            Reject(() => SplitPlan.ParseSelection("1,", 8), "reject trailing separator");
            Reject(() => SplitPlan.ParseSelection("", 8), "reject empty selection");
            Reject(() => SplitPlan.Create(8, SplitMode.Fixed, ranges, 0, "", false), "reject zero fixed size");
            Reject(() => SplitPlan.Create(1000, SplitMode.Custom, [new(1, 1000), new(1, 2)], 1, "", true), "reject output exceeding 1000 pages");
            var export = await SplitExport.RunAsync(source, directory, "", fixedPlan, CancellationToken.None);
            Check(export.Success && export.Files.Length == 3, "real worker fixed split direct PDF files");
            for (var i = 0; i < export.Files.Length; i++)
            {
                var pdf = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(export.Files[i]));
                Check(pdf.PageCount == fixedPlan[i].Length, "reopen output count " + i);
                for (var j = 0; j < pdf.PageCount; j++)
                {
                    using var page = pdf.GetPage((uint)j);
                    using var original = sourcePdf.GetPage((uint)(fixedPlan[i][j] - 1));
                    Check(Math.Abs(page.Size.Width - original.Size.Width) < 1, "source page identity " + i + "/" + j);
                }
            }
            var again = await SplitExport.RunAsync(source, directory, "", fixedPlan, CancellationToken.None);
            Check(again.Success && Path.GetDirectoryName(again.Files[0]) != Path.GetDirectoryName(export.Files[0]), "existing folder preserved by unique suffix");
            var single = await SplitExport.RunAsync(source, directory, "", combined, CancellationToken.None);
            Check(single.Success && Path.GetDirectoryName(single.Files[0]) == directory, "combined direct PDF at destination");
            var combinedPdf = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(single.Files[0]));
            Check(combinedPdf.PageCount == 6, "combined PDF reopens with six pages");
            var singleAgain = await SplitExport.RunAsync(source, directory, "", combined, CancellationToken.None);
            Check(singleAgain.Success && singleAgain.Files[0] != single.Files[0] && File.Exists(single.Files[0]), "existing PDF preserved by unique suffix");
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            var stopped = await SplitExport.RunAsync(source, directory, "", fixedPlan, cancelled.Token);
            Check(!stopped.Success && stopped.Files.Length == 0, "pre-cancel has no published output");
            using var midway = new CancellationTokenSource();
            var interrupted = await SplitExport.RunAsync(source, directory, "", fixedPlan, midway.Token, (done, _) => { if (done == 1) midway.Cancel(); });
            Check(!interrupted.Success && interrupted.Files.Length == 0, "mid-batch cancel does not publish partial results");
            var invalid = await SplitExport.RunAsync(source, directory, "", [[1], [99]], CancellationToken.None);
            Check(!invalid.Success && invalid.Files.Length == 0, "later group failure does not publish earlier group");
            Check(Directory.GetDirectories(directory, ".oto-split-*").Length == 0, "no staging remnants after success cancellation or failure");
            Check(hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "source SHA256 unchanged");
            report.Add("SAMPLE " + source);
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "split-selftest.txt"), report);
    }

    private static void CreateSample(string path)
    {
        var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R 4 0 R 5 0 R 6 0 R 7 0 R 8 0 R 9 0 R 10 0 R] /Count 8 >>" };
        for (var i = 1; i <= 8; i++) objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {200 + i * 10} 300] /Resources << /Font << /F1 11 0 R >> >> /Contents {11 + i} 0 R >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        for (var i = 1; i <= 8; i++)
        {
            var stream = $"BT /F1 22 Tf 25 210 Td (Halaman {i}) Tj ET";
            objects.Add($"<< /Length {stream.Length} >>\nstream\n{stream}\nendstream");
        }
        var text = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (var i = 0; i < objects.Count; i++) { offsets.Add(text.Length); text.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        var xref = text.Length; text.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) text.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
        text.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        File.WriteAllText(path, text.ToString(), Encoding.ASCII);
    }
}
