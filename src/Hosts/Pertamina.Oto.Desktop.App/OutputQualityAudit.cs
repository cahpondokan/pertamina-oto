#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditQualityAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "quality-audit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "quality-audit-path.txt"), directory);
        var report = new List<string>();
        void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        async Task Capture(string name)
        {
            root.UpdateLayout(); await Task.Delay(200);
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var pixels = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", CreationCollisionOption.FailIfExists);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
        }
        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000));
            outputQuality = "medium";
            var options = BuildQualityOptions(true);
            var segment = (Panel)options.Children[1];
            var buttons = segment.Children.OfType<ToggleButton>().ToArray();
            Check(buttons.Length == 3 && buttons[1].IsChecked == true && buttons.Count(b => b.IsChecked == true) == 1, "three segments default medium");
            Check(buttons.All(b => b.IsTabStop && AutomationProperties.GetName(b).StartsWith("Kualitas ", StringComparison.Ordinal)), "keyboard focus and accessible toggle names");
            buttons[2].IsChecked = true;
            Check(outputQuality == "high" && buttons.Count(b => b.IsChecked == true) == 1, "accessible checked state updates pipeline and remains exclusive");
            buttons[2].IsChecked = false;
            Check(buttons[2].IsChecked == true && outputQuality == "high", "selected segment cannot leave an empty selection");
            buttons[1].IsChecked = true;
            Check(options.Children.OfType<ToggleButton>().Single().Content.ToString()!.StartsWith("Asli", StringComparison.Ordinal), "original encoding bypass available");
            Check(!BuildQualityOptions(false).Children.OfType<ToggleButton>().Any(), "JPG has no misleading original PDF quality option");
            var source = Path.Combine(directory, "synthetic-photo.png");
            const uint width = 3000, height = 2000;
            var pixels = new byte[width * height * 4]; var random = new Random(42);
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var x = i / 4 % (int)width; var y = i / 4 / (int)width;
                pixels[i] = (byte)(x * 255 / width); pixels[i + 1] = (byte)(y * 255 / height);
                pixels[i + 2] = (byte)random.Next(256); pixels[i + 3] = 255;
            }
            using (var file = new FileStream(source, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var stream = file.AsRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, width, height, 96, 96, pixels); await encoder.FlushAsync();
            }
            var hash = SHA256.HashData(await File.ReadAllBytesAsync(source));
            var sizes = new List<long>();
            foreach (var quality in new[] { "low", "medium", "high", "original" })
            {
                var target = Path.Combine(directory, quality + ".pdf");
                var reply = await ExportImagesWithQualityAsync(new("images", [source], target, Quality: quality), CancellationToken.None);
                Check(reply.Success && reply.Pages == 1, quality + " image PDF validates by reopening");
                sizes.Add(reply.Bytes); report.Add("MEASURE " + quality + " PDF bytes=" + reply.Bytes);
                var pdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(target));
                Check(pdf.PageCount == 1, quality + " Windows PDF reopen");
            }
            Check(sizes.Distinct().Count() == 4 && sizes[0] < sizes[1] && sizes[1] < sizes[2], "photo-like fixture preset encoding changes results");
            var pdfSource = Path.Combine(directory, "high.pdf");
            var pdfHash = SHA256.HashData(await File.ReadAllBytesAsync(pdfSource));
            var edges = new List<uint>();
            foreach (var quality in new[] { "low", "medium", "high" })
            {
                var target = Path.Combine(directory, quality + ".jpg");
                Check((await ExportJpgAsync(new("pdf-jpg", [pdfSource], target, Rotation: 0, Quality: quality), CancellationToken.None)).Success, quality + " JPG export");
                using var stream = await (await StorageFile.GetFileFromPathAsync(target)).OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                edges.Add(Math.Max(decoder.PixelWidth, decoder.PixelHeight));
                Check(decoder.PixelWidth > 0 && decoder.PixelHeight > 0, quality + " JPEG reopens");
                report.Add("MEASURE " + quality + " JPG edge=" + edges[^1] + " bytes=" + new FileInfo(target).Length);
            }
            Check(edges[0] < edges[1] && edges[1] < edges[2], "JPG preset resolution increases");
            var textPdf = Path.Combine(directory, "text.pdf");
            var content = "BT /F1 18 Tf 40 780 Td (Quality fixture selectable text) Tj ET\n";
            var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Count 1 /Kids [3 0 R] >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {content.Length} >>\nstream\n{content}endstream" };
            var text = new System.Text.StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
            for (var i = 0; i < objects.Length; i++) { offsets.Add(text.Length); text.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
            var xref = text.Length; text.Append("xref\n0 6\n0000000000 65535 f \n");
            foreach (var offset in offsets) text.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            text.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            File.WriteAllText(textPdf, text.ToString(), System.Text.Encoding.ASCII);
            var textHash = SHA256.HashData(File.ReadAllBytes(textPdf));
            foreach (var quality in new[] { "low", "medium", "high" })
                Check((await ExportJpgAsync(new("pdf-jpg", [textPdf], Path.Combine(directory, "text-" + quality + ".jpg"), Rotation: 0, Quality: quality), CancellationToken.None)).Success, quality + " text PDF JPG export");
            var compressed = Path.Combine(directory, "text-lossless.pdf");
            Check((await WorkerClient.RunAsync(new("compress", [textPdf], compressed), CancellationToken.None)).Success, "lossless text PDF compression");
            var search = await WorkerClient.RunAsync(new("reader-search", [compressed], Path.Combine(directory, "search-unused.pdf"), Text: "selectable"), CancellationToken.None);
            Check(search.Success && search.Search?.Hits.Length > 0, "compressed PDF retains searchable text");
            Check(textHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(textPdf))), "text PDF source hash unchanged");
            Check(!(await ExportImagesWithQualityAsync(new("images", [source], Path.Combine(directory, "low.pdf"), Quality: "low"), CancellationToken.None)).Success, "existing output rejected");
            Check(!(await ExportImagesWithQualityAsync(new("images", [source], Path.Combine(directory, "invalid.pdf"), Quality: "bogus"), CancellationToken.None)).Success, "unknown quality rejected");
            Check(!(await ExportJpgAsync(new("pdf-jpg", [pdfSource], Path.Combine(directory, "invalid.jpg"), Quality: "bogus"), CancellationToken.None)).Success, "unknown JPG quality rejected");
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            Check(!(await ExportImagesWithQualityAsync(new("images", [source], Path.Combine(directory, "cancel.pdf"), Quality: "high"), cancellation.Token)).Success, "cancelled image conversion rejected");
            Check(!File.Exists(Path.Combine(directory, "cancel.pdf")), "cancel leaves no result");
            Check(!(await WorkerClient.RunAsync(new("compress", [pdfSource], Path.Combine(directory, "lossy.pdf"), Quality: "low"), CancellationToken.None)).Success, "worker rejects unsupported lossy PDF request");
            Check(hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))) && pdfHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(pdfSource))), "image and PDF source hashes unchanged");
            Check(Directory.GetDirectories(directory, ".oto-quality-*").Length == 0, "staged image data removed");
            ShowTool(ToolCatalog.All.Single(t => t.Id == "images")); await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(source)]); await Capture("images-quality");
            Check(selected?.Id == "images" && outputQuality == "medium", "real image intake defaults medium");
            ShowTool(ToolCatalog.All.Single(t => t.Id == "pdf-jpg")); await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(pdfSource)]); await Capture("jpg-quality");
            Check(selected?.Id == "pdf-jpg" && outputQuality == "medium", "real JPG intake defaults medium");
            ShowTool(ToolCatalog.All.Single(t => t.Id == "compress")); await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(pdfSource)]); await Capture("compress-lossless");
            await TestJpgExportAsync();
            Check(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "jpg-selftest.txt")).All(l => l.StartsWith("PASS", StringComparison.Ordinal)), "JPG range rotation cancellation source regression");
        }
        catch (Exception ex) { report.Add("FAIL " + ex.GetType().Name + ": " + ex.Message); }
        File.WriteAllLines(Path.Combine(directory, "audit.txt"), report);
    }
}
#endif
