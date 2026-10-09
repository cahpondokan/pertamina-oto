using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal static class PrintPlan
{
    internal static (uint Width, uint Height) RasterSize(double width, double height, int maxWidth = 4096, int maxEdge = 4096, double pixels = 16_000_000)
    {
        if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0 || !double.IsFinite(width * height))
            throw new InvalidOperationException("Ukuran halaman PDF tidak valid.");
        var scale = Math.Min(maxWidth / width, Math.Min(maxEdge / Math.Max(width, height), Math.Sqrt(pixels / (width * height))));
        return ((uint)Math.Max(1, width * scale), (uint)Math.Max(1, height * scale));
    }
    internal static int[] Pages(string mode, string custom, int count, int current)
    {
        if (count < 1 || count > 10000) throw new InvalidOperationException("PDF harus memiliki 1–10.000 halaman.");
        if (mode == "Semua halaman") return Enumerable.Range(0, count).ToArray();
        if (mode == "Halaman saat ini") return [Math.Clamp(current, 0, count - 1)];
        if (custom.Length > 2048 || string.IsNullOrWhiteSpace(custom)) throw new InvalidOperationException("Isi rentang, misalnya 1, 3–5.");
        var result = new SortedSet<int>();
        foreach (var part in custom.Replace('–', '-').Split(','))
        {
            var range = part.Trim().Split('-');
            if (range.Length > 2 || !int.TryParse(range[0], out var first) || first < 1 || first > count) throw new InvalidOperationException("Rentang halaman tidak valid.");
            var last = first;
            if (range.Length == 2 && (!int.TryParse(range[1], out last) || last < first || last > count)) throw new InvalidOperationException("Rentang halaman tidak valid.");
            for (var i = first; i <= last; i++) result.Add(i - 1);
        }
        return result.ToArray();
    }
    internal static (double X, double Y, double W, double H) Placement(double w, double h, PrintDevice.Bounds b, bool actual)
    {
        var pw = w * b.DpiX / 96; var ph = h * b.DpiY / 96;
        var scale = actual ? 1 : Math.Min(b.PrintableWidth / pw, b.PrintableHeight / ph);
        return (b.Left + (b.PrintableWidth - pw * scale) / 2, b.Top + (b.PrintableHeight - ph * scale) / 2, pw * scale, ph * scale);
    }
    internal static async Task<int> SendAsync(PdfDocument pdf, PrintDevice device, int[] pages, int copies, bool actual, Action<int> progress, string? output, CancellationToken token, Action<int>? jobStarted = null)
    {
        if (copies < 1 || copies > 99 || pages.Length == 0 || pages.Length * copies > 10000) throw new InvalidOperationException("Maksimal 99 salinan dan 10.000 lembar per pekerjaan.");
        var dc = device.CreateContext(); var started = false;
        try
        {
            var b = PrintDevice.Geometry(dc);
            var info = new PrintDevice.DocInfo { Size = Marshal.SizeOf<PrintDevice.DocInfo>(), Name = "PERTAMINA OTO · Dokumen PDF", Output = output };
            token.ThrowIfCancellationRequested();
            var job = PrintDevice.StartDoc(dc, ref info); if (job <= 0) throw new InvalidOperationException("Windows tidak menerima pekerjaan cetak."); started = true; jobStarted?.Invoke(job);
            var completed = 0;
            for (var copy = 0; copy < copies; copy++)
            {
              foreach (var index in pages)
            {
                token.ThrowIfCancellationRequested();
                using var page = pdf.GetPage((uint)index);
                var p = Placement(page.Size.Width, page.Size.Height, b, actual);
                // One raster at a time; bound at 16M pixels / 4096 per edge. PDF renderer uses white background.
                var (width, height) = RasterSize(page.Size.Width, page.Size.Height);
                using var stream = new InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = width, DestinationHeight = height, BackgroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255) });
                token.ThrowIfCancellationRequested(); stream.Seek(0);
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var pixels = (await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage)).DetachPixelData();
                if (decoder.PixelWidth != width || decoder.PixelHeight != height || pixels.Length != (long)width * height * 4)
                    throw new InvalidOperationException("Raster halaman tidak sesuai ukuran yang diminta.");
                var bitmap = new PrintDevice.BitmapInfo { Size = 40, Width = (int)width, Height = -(int)height, Planes = 1, Bits = 32 };
                if (PrintDevice.StartPage(dc) <= 0) throw new InvalidOperationException("Driver gagal memulai halaman.");
                var rows = PrintDevice.StretchDIBits(dc, (int)Math.Round(p.X - b.Left), (int)Math.Round(p.Y - b.Top), (int)Math.Round(p.W), (int)Math.Round(p.H), 0, 0, (int)width, (int)height, pixels, ref bitmap, 0, 0x00CC0020);
                if (rows == 0 || rows == -1 || PrintDevice.EndPage(dc) <= 0) throw new InvalidOperationException("Driver gagal menerima halaman.");
                progress(++completed);
            }
              // Start each collated copy on the front of a sheet when duplex has an odd page count.
              if (copy + 1 < copies && device.Duplex && device.DuplexMode > 1 && pages.Length % 2 != 0)
              {
                  token.ThrowIfCancellationRequested();
                  if (PrintDevice.StartPage(dc) <= 0 || PrintDevice.EndPage(dc) <= 0) throw new InvalidOperationException("Driver gagal memisahkan salinan dua sisi.");
              }
            }
            token.ThrowIfCancellationRequested();
            if (PrintDevice.EndDoc(dc) <= 0) throw new InvalidOperationException("Pekerjaan tidak berhasil dikirim."); started = false; return job;
        }
        finally { if (started) _ = PrintDevice.AbortDoc(dc); PrintDevice.DeleteDC(dc); }
    }
}
