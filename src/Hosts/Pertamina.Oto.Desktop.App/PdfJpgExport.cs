using Pertamina.Oto.LocalPdf;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private string? jpgResultFile;
    private async Task<PdfReply> ExportJpgAsync(PdfRequest request, CancellationToken token)
    {
        string? stage = null;
        try
        {
            var profile = OutputQualityProfile.For(request.Quality);
            if (request.Quality == "original") return new(false, "JPG memerlukan preset kualitas; PDF sumber tetap asli.");
            if (request.Inputs.Length != 1) return new(false, "Pilih satu PDF.");
            var source = Path.GetFullPath(request.Inputs[0]);
            var target = Path.GetFullPath(request.Output);
            if (source.StartsWith(@"\\", StringComparison.Ordinal) || target.StartsWith(@"\\", StringComparison.Ordinal)) return new(false, "Gunakan lokasi lokal.");
            if (File.Exists(target) || Directory.Exists(target) || string.Equals(source, target, StringComparison.OrdinalIgnoreCase)) return new(false, "Hasil sudah ada. Pilih nama baru.");
            using var sourceLock = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (sourceLock.Length is <= 0 or > 104857600) return new(false, "PDF harus berukuran maksimal 100 MB.");
            token.ThrowIfCancellationRequested();
            var file = await StorageFile.GetFileFromPathAsync(source);
            var pdf = await PdfDocument.LoadFromFileAsync(file, request.Password).AsTask(token);
            if (pdf.PageCount is 0 or > 1000) return new(false, "PDF harus berisi 1–1000 halaman.");
            var indices = new List<uint>();
            if (string.IsNullOrWhiteSpace(request.Pages)) indices.AddRange(Enumerable.Range(0, (int)pdf.PageCount).Select(i => (uint)i));
            else
            {
                foreach (var part in request.Pages.Split(','))
                {
                    var range = part.Trim().Split('-');
                    if (range.Length > 2 || !uint.TryParse(range[0], out var first)) return new(false, "Rentang halaman tidak valid.");
                    var last = first;
                    if (range.Length == 2 && !uint.TryParse(range[1], out last)) return new(false, "Rentang halaman tidak valid.");
                    if (first < 1 || last < first || last > pdf.PageCount) return new(false, "Halaman di luar rentang PDF.");
                    for (var p = first; p <= last; p++)
                    { if (indices.Contains(p - 1)) return new(false, "Halaman tidak boleh berulang."); indices.Add(p - 1); }
                }
            }
            var resultFolder = Path.Combine(Path.GetDirectoryName(target)!, Path.GetFileNameWithoutExtension(target));
            if (indices.Count > 1 && (Directory.Exists(resultFolder) || File.Exists(resultFolder))) return new(false, "Folder hasil sudah ada. Pilih nama baru.");
            stage = Path.Combine(Path.GetDirectoryName(target)!, ".oto-jpg-" + Guid.NewGuid().ToString("N") + ".tmp");
            Directory.CreateDirectory(stage);
            long totalBytes = 0;
            {
                {
                    for (var i = 0; i < indices.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        status.Text = $"Mengubah halaman {i + 1} dari {indices.Count} menjadi JPG…";
                        using var page = pdf.GetPage(indices[i]);
                        var scale = Math.Min(profile.Scale, profile.MaxEdge / Math.Max(page.Size.Width, page.Size.Height));
                        using var image = new InMemoryRandomAccessStream();
                        await page.RenderToStreamAsync(image, new PdfPageRenderOptions {
                            BitmapEncoderId = BitmapEncoder.PngEncoderId,
                            DestinationWidth = (uint)Math.Max(1, Math.Round(page.Size.Width * scale)),
                            DestinationHeight = (uint)Math.Max(1, Math.Round(page.Size.Height * scale)),
                            BackgroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255)
                        }).AsTask(token);
                        image.Seek(0);
                        var decoder = await BitmapDecoder.CreateAsync(image).AsTask(token);
                        if (decoder.PixelWidth == 0 || decoder.PixelHeight == 0) throw new InvalidDataException();
                        image.Seek(0);
                        using var rotated = new InMemoryRandomAccessStream();
                        {
                            var transform = new BitmapTransform { Rotation = request.Rotation switch { 90 => BitmapRotation.Clockwise90Degrees, 180 => BitmapRotation.Clockwise180Degrees, 270 => BitmapRotation.Clockwise270Degrees, _ => BitmapRotation.None } };
                            var pixels = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage).AsTask(token);
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, rotated, JpegOptions(profile.JpegQuality)).AsTask(token);
                            var swap = request.Rotation is 90 or 270;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, swap ? decoder.PixelHeight : decoder.PixelWidth, swap ? decoder.PixelWidth : decoder.PixelHeight, 96, 96, pixels.DetachPixelData());
                            await encoder.FlushAsync().AsTask(token); rotated.Seek(0);
                        }
                        var outputFile = Path.Combine(stage, Path.GetFileNameWithoutExtension(target) + $"-{i + 1:D3}.jpg");
                        using (var destination = new FileStream(outputFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        using (var input = rotated.AsStreamForRead()) await input.CopyToAsync(destination, token);
                        totalBytes += new FileInfo(outputFile).Length;
                        if (totalBytes > 250L * 1024 * 1024) throw new InvalidDataException("Output limit");
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            var rendered = Directory.GetFiles(stage).Order().ToArray();
            if (rendered.Length != indices.Count || rendered.Any(p => new FileInfo(p).Length == 0)) throw new InvalidDataException();
            if (indices.Count == 1) { File.Move(rendered[0], target, false); jpgResultFile = target; }
            else { Directory.Move(stage, resultFolder); stage = null; jpgResultFile = Path.Combine(resultFolder, Path.GetFileName(rendered[0])); }
            return new(true, "JPG tersimpan langsung: " + (indices.Count == 1 ? target : resultFolder), indices.Count, totalBytes);
        }
        catch (OperationCanceledException) { return new(false, "Ekspor dibatalkan. File sumber tidak diubah."); }
        catch (Exception) { return new(false, "Ekspor gagal. Periksa kata sandi, kondisi PDF, ruang disk, dan lokasi hasil. Sumber tidak diubah."); }
        finally
        {
            if (stage is not null) { try { foreach (var file in Directory.GetFiles(stage)) File.Delete(file); Directory.Delete(stage); } catch (Exception) { status.Text = "File sementara mungkin tertinggal di folder hasil."; } }
        }
    }

    internal async Task TestJpgExportAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "oto-jpg-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var report = new List<string>();
        try
        {
            var text = new System.Text.StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();
            var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R 4 0 R] /Count 2 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 300] /Resources << >> >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 300 200] /Resources << >> >>" };
            for (var i = 0; i < objects.Length; i++) { offsets.Add(text.Length); text.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
            var xref = text.Length;
            text.Append("xref\n0 5\n0000000000 65535 f \n");
            foreach (var offset in offsets) text.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            text.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size 5 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            var source = Path.Combine(root, "synthetic.pdf"); File.WriteAllText(source, text.ToString(), System.Text.Encoding.ASCII);
            var hash = System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source));
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); report.Add("PASS " + name); }
            var target = Path.Combine(root, "result.jpg");
            var request = new PdfRequest("pdf-jpg", [source], target, Quality: "medium");
            var reply = await ExportJpgAsync(request, CancellationToken.None);
            Check(reply.Success && reply.Pages == 2, "two-page export");
            var images = Directory.GetFiles(Path.Combine(root, "result"), "*.jpg").Order().ToArray();
            {
                Check(images.Length == 2, "direct JPG count");
                foreach (var entry in images)
                {
                    using var stream = File.OpenRead(entry);
                    Check(stream.ReadByte() == 255 && stream.ReadByte() == 216, "JPEG signature");
                }
                using var firstImage = new InMemoryRandomAccessStream();
                using (var entryStream = File.OpenRead(images[0]))
                { using var targetStream = firstImage.AsStreamForWrite(); await entryStream.CopyToAsync(targetStream); await targetStream.FlushAsync();
                    firstImage.Seek(0); var decoded = await BitmapDecoder.CreateAsync(firstImage);
                    Check(decoded.PixelWidth > decoded.PixelHeight && Math.Abs(decoded.PixelWidth / (double)decoded.PixelHeight - 1.5) < .02, "90-degree output dimensions"); }
            }
            Check(!(await ExportJpgAsync(request, CancellationToken.None)).Success, "existing target rejected");
            var single = Path.Combine(root, "single.jpg");
            Check((await ExportJpgAsync(request with { Output = single, Pages = "1" }, CancellationToken.None)).Success && File.Exists(single), "single page direct JPG");
            Check(!(await ExportJpgAsync(request with { Output = single, Pages = "1" }, CancellationToken.None)).Success, "existing JPG rejected");
            Check(!(await ExportJpgAsync(request with { Output = Path.Combine(root, "invalid.zip"), Pages = "3" }, CancellationToken.None)).Success, "invalid range rejected");
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            Check(!(await ExportJpgAsync(request with { Output = Path.Combine(root, "cancel.zip") }, cancelled.Token)).Success, "cancelled export rejected");
            Check(hash.SequenceEqual(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source))), "source unchanged");
            Check(Directory.GetFileSystemEntries(root, "*.tmp").Length == 0, "no staging leftovers");
        }
        catch (Exception ex) { report.Add("FAIL " + ex.GetType().Name + ": " + ex.Message); }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "jpg-selftest.txt"), report);
    }
}
