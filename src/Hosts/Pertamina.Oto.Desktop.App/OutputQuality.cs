using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

// Reusable for raster output only. OCR, Office layout and PDF/A have separate contracts.
internal sealed record OutputQualityProfile(uint MaxEdge, double Scale, float JpegQuality)
{
    internal static OutputQualityProfile For(string value) => value switch
    {
        "low" => new(1200, 1, .55f),
        "medium" => new(2400, 2, .8f),
        "high" => new(3600, 3, .95f),
        "original" => new(uint.MaxValue, 1, 1),
        _ => throw new ArgumentException("Preset kualitas tidak valid.")
    };
}

internal sealed partial class MainWindow
{
    private string outputQuality = "medium";
    private StackPanel BuildQualityOptions(bool allowOriginal)
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(Copy(allowOriginal ? "Kualitas gambar dalam PDF" : "Kualitas JPG", 14));
        var segments = new ResponsiveWrapPanel { Spacing = 0 };
        var choices = new List<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>();
        var updating = false;
        void Select(string key, Microsoft.UI.Xaml.Controls.Primitives.ToggleButton chosen)
        {
            if (updating) return;
            updating = true;
            outputQuality = key;
            foreach (var choice in choices) choice.IsChecked = choice == chosen;
            updating = false;
        }
        foreach (var (key, label) in new[] { ("low", "Rendah"), ("medium", "Sedang"), ("high", "Tinggi") })
        {
            var button = new Microsoft.UI.Xaml.Controls.Primitives.ToggleButton { Content = label, IsChecked = outputQuality == key, MinWidth = 80, Padding = new Thickness(10), CornerRadius = new CornerRadius(0) };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, "Kualitas " + label);
            button.Click += (_, _) => Select(key, button);
            button.Checked += (_, _) => Select(key, button);
            button.Unchecked += (_, _) => { if (!updating && outputQuality == key) Select(key, button); };
            choices.Add(button);
            segments.Children.Add(button); editable.Add(button);
        }
        panel.Children.Add(segments);
        if (allowOriginal)
        {
            var original = new Microsoft.UI.Xaml.Controls.Primitives.ToggleButton { Content = "Asli · tanpa encoding ulang", IsChecked = outputQuality == "original" };
            original.Click += (_, _) => Select("original", original);
            original.Checked += (_, _) => Select("original", original);
            original.Unchecked += (_, _) => { if (!updating && outputQuality == "original") Select("original", original); };
            choices.Add(original);
            panel.Children.Add(original); editable.Add(original);
        }
        panel.Children.Add(Copy("Rendah: ukuran cenderung kecil, detail berkurang. Tinggi: detail lebih banyak, ukuran cenderung besar. Ukuran akhir bergantung isi.", 12));
        if (allowOriginal) panel.Children.Add(Copy("Preset mengubah gambar menjadi JPG dengan latar putih; transparansi diratakan. Pilih Asli untuk mempertahankannya.", 12));
        else panel.Children.Add(Copy("PDF sumber tetap asli. JPG adalah gambar tanpa teks yang dapat dipilih, tautan, atau formulir.", 12));
        return panel;
    }

    private static BitmapPropertySet JpegOptions(float quality) => new()
    {
        { "ImageQuality", new BitmapTypedValue(quality, Windows.Foundation.PropertyType.Single) }
    };

    private async Task<PdfReply> ExportImagesWithQualityAsync(PdfRequest request, CancellationToken token)
    {
        var locks = new List<FileStream>();
        string? stage = null;
        try
        {
            var profile = OutputQualityProfile.For(request.Quality);
            if (request.Inputs.Length is < 1 or > 50) return new(false, "Pilih 1–50 gambar.");
            long sourceBytes = 0;
            foreach (var path in request.Inputs)
            {
                if (Path.GetFullPath(path).StartsWith(@"\\", StringComparison.Ordinal) || !new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) return new(false, "Pilih JPG atau PNG lokal.");
                var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                locks.Add(source); sourceBytes += source.Length;
            }
            if (sourceBytes > 100L * 1024 * 1024) return new(false, "Total gambar maksimal 100 MB.");
            token.ThrowIfCancellationRequested();
            if (request.Quality == "original") return await WorkerClient.RunAsync(request, token);
            stage = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(request.Output))!, ".oto-quality-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stage);
            var converted = new List<string>();
            long stagedBytes = 0;
            foreach (var source in locks)
            {
                token.ThrowIfCancellationRequested();
                using var input = await (await Windows.Storage.StorageFile.GetFileFromPathAsync(source.Name)).OpenReadAsync().AsTask(token);
                var decoder = await BitmapDecoder.CreateAsync(input).AsTask(token);
                if (decoder.PixelWidth == 0 || decoder.PixelHeight == 0 || (long)decoder.PixelWidth * decoder.PixelHeight > 40_000_000) return new(false, "Gambar dibatasi 40 megapiksel.");
                var scale = Math.Min(1, profile.MaxEdge / (double)Math.Max(decoder.PixelWidth, decoder.PixelHeight));
                var width = (uint)Math.Max(1, Math.Round(decoder.PixelWidth * scale));
                var height = (uint)Math.Max(1, Math.Round(decoder.PixelHeight * scale));
                var transform = new BitmapTransform { ScaledWidth = width, ScaledHeight = height, InterpolationMode = BitmapInterpolationMode.Fant };
                var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.ColorManageToSRgb).AsTask(token);
                var pixels = data.DetachPixelData();
                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var alpha = pixels[i + 3];
                    for (var c = 0; c < 3; c++) pixels[i + c] = (byte)((pixels[i + c] * alpha + 255 * (255 - alpha) + 127) / 255);
                    pixels[i + 3] = 255;
                }
                var target = Path.Combine(stage, converted.Count + ".jpg");
                using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                using (var stream = output.AsRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream, JpegOptions(profile.JpegQuality)).AsTask(token);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, width, height, 96, 96, pixels);
                    await encoder.FlushAsync().AsTask(token);
                }
                stagedBytes += new FileInfo(target).Length;
                if (stagedBytes > 100L * 1024 * 1024) return new(false, "Hasil gambar sementara melebihi 100 MB.");
                converted.Add(target);
            }
            token.ThrowIfCancellationRequested();
            return await WorkerClient.RunAsync(request with { Inputs = converted.ToArray(), Quality = "original" }, token);
        }
        catch (OperationCanceledException) { return new(false, "Proses dibatalkan. Sumber tidak diubah."); }
        catch (Exception) { return new(false, "Konversi gambar gagal. Periksa gambar dan lokasi hasil. Sumber tidak diubah."); }
        finally
        {
            foreach (var source in locks) source.Dispose();
            if (stage is not null)
            {
                try { foreach (var file in Directory.GetFiles(stage)) File.Delete(file); Directory.Delete(stage); }
                catch (Exception) { status.Text = "File sementara mungkin tertinggal di folder hasil."; }
            }
        }
    }
}
