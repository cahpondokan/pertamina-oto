#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditZoomSmoothnessAsync()
    {
        ShowPdfReader(); await pdfReader!.AuditSmoothnessAsync();
    }
    private sealed partial class PdfReaderView
    {
        internal async Task AuditSmoothnessAsync()
        {
            var output = Path.Combine(AppContext.BaseDirectory, "zoom-smoothness"); Directory.CreateDirectory(output);
            var report = new List<string>(); var traces = new List<object>();
            var baseline = Environment.GetCommandLineArgs().Contains("--zoom-baseline", StringComparer.Ordinal);
            void Check(bool value, string name) { if (!value && !baseline) throw new InvalidOperationException(name); report.Add((value ? "PASS " : "FAIL ") + name); }
            async Task Settle() { for (var i = 0; i < 8; i++) { View.UpdateLayout(); await Task.Delay(100); } await Rendering; }
            async Task Capture(string name)
            {
                var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(View); var data = (await bitmap.GetPixelsAsync()).ToArray();
                var folder = await StorageFolder.GetFolderFromPathAsync(output); var file = await folder.CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, data); await encoder.FlushAsync();
            }
            try
            {
                var fixture = Path.Combine(output, "mixed.pdf"); WriteZoomSurfaceFixture(fixture);
                var sourceHash = System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(fixture));
                await OpenAsync(fixture); await Settle();
                foreach (var windowWidth in new[] { 1586, 720 })
                foreach (var startZoom in new[] { .5f, 1f, 1.25f, 2f })
                {
                    owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, 900)); await Settle();
                    fit = false; viewport.ChangeView(null, pageSlots[5].Top * startZoom + 180, startZoom, true); await Settle();
                    var frames = 0; var blank = 0; var lost = 0; var renders = pageRenderRequests; var evictions = bitmapEvictions; var thumbs = thumbnailRebuilds;
                    var retained = pageSlots.Where(p => p.Image.Source is not null).ToArray(); var identities = pageSlots.Select(p => p.Frame).ToArray();
                    var centerBefore = (viewport.VerticalOffset + viewport.ActualHeight / 2) / viewport.ZoomFactor;
                    var scaleChangingRenders = 0; var peakPixels = 0d;
                    var intermediate = false; var lastRenderCount = pageRenderRequests;
                    var blankSamples = new List<string>();
                    void Changing(object? sender, ScrollViewerViewChangedEventArgs e) => intermediate = e.IsIntermediate;
                    void Observe(object? sender, object e)
                    {
                        frames++; var top = viewport.VerticalOffset / viewport.ZoomFactor; var bottom = top + viewport.ActualHeight / viewport.ZoomFactor;
                        var visible = pageSlots.Where(p => p.Top + p.Image.Height > top && p.Top < bottom).ToArray();
                        if (visible.Length > 0 && visible.All(p => p.Image.Source is null))
                        {
                            blank++;
                            if (blankSamples.Count < 20) blankSamples.Add(FormattableString.Invariant($"z={viewport.ZoomFactor},offset={viewport.VerticalOffset},page={PageAt(top)},range={renderedRange},sources={string.Join(',', pageSlots.Select((p, i) => (p, i)).Where(x => x.p.Image.Source is not null).Select(x => x.i))}"));
                        }
                        if (retained.Any(p => p.Top + p.Image.Height > top && p.Top < bottom && p.Image.Source is null)) lost++;
                        peakPixels = Math.Max(peakPixels, pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width));
                        if (intermediate) scaleChangingRenders += pageRenderRequests - lastRenderCount;
                        lastRenderCount = pageRenderRequests;
                    }
                    viewport.ViewChanged += Changing;
                    CompositionTarget.Rendering += Observe;
                    try
                    {
                        ZoomController.Route(viewport, 1);
                        for (var i = 0; i < 5; i++) { await Task.Delay(20); await Capture($"{windowWidth}-{startZoom:0.##}-{i}"); }
                        await Settle();
                        Check(Math.Abs((viewport.VerticalOffset + viewport.ActualHeight / 2) / viewport.ZoomFactor - centerBefore) < 3, $"button center anchor {windowWidth}/{startZoom}");
                        for (var i = 0; i < 8; i++) { ZoomController.Route(viewport, i % 2 == 0 ? -1 : 1); await Task.Delay(25); }
                        await Settle();
                        // Model an anchored native zoom, not a simultaneous
                        // arbitrary page jump caused by unspecified offsets.
                        var nativeX = (viewport.HorizontalOffset + viewport.ActualWidth / 2) / viewport.ZoomFactor;
                        var nativeY = (viewport.VerticalOffset + viewport.ActualHeight / 2) / viewport.ZoomFactor;
                        viewport.ChangeView(Math.Max(0, nativeX * startZoom - viewport.ActualWidth / 2),
                            Math.Max(0, nativeY * startZoom - viewport.ActualHeight / 2), startZoom, false); await Settle();
                        traces.Add(new { windowWidth, startZoom, frames, blank, lost, Renders = pageRenderRequests - renders, Evictions = bitmapEvictions - evictions, scaleChangingRenders, peakPixels, blankSamples });
                        Check(frames > 0 && blank == 0 && lost == 0, $"retained visible pages never clear during burst/native zoom {windowWidth}/{startZoom}");
                        Check(pageSlots.Select(p => p.Frame).SequenceEqual(identities) && thumbnailRebuilds == thumbs, $"page and thumbnail identities stable {windowWidth}/{startZoom}");
                        Check(scaleChangingRenders == 0, $"no raster work at intermediate native scale {windowWidth}/{startZoom}");
                        Check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000, $"steady raster budget {windowWidth}/{startZoom}");
                    }
                    finally { CompositionTarget.Rendering -= Observe; viewport.ViewChanged -= Changing; }
                }
                foreach (var preset in new[] { "50%", "100%", "125%", "200%", "500%" })
                {
                    zoomControls.Percent.SelectedItem = preset; await Settle();
                    Check(ZoomController.TryPercent(preset, .1, 5, out var target) && Math.Abs(viewport.ZoomFactor - target) < .001,
                        "editable preset uses actual scale " + preset);
                    Check(pageSlots.Any(p => p.Image.Source is not null), "preset retains decoded page " + preset);
                }
                ZoomController.Route(viewport, 0); await Settle(); FitPreview(false); await Rendering; await Settle();
                Check(Math.Abs(viewport.ZoomFactor - FitFactor()) < .001, "fit page retains truthful percentage");
                FitPreview(true); await Rendering; await Settle();
                Check(Math.Abs(viewport.ZoomFactor - FitFactor()) < .001, "fit width retains truthful percentage");
                var finalHash = System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(fixture));
                Check(sourceHash.SequenceEqual(finalHash), "zoom leaves source PDF bytes unchanged");
                report.Add("NOT_TESTED physical pinch, monitor DPI, user PDF and every GPU frame; native ChangeView and sampled WinUI captures only.");
            }
            catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
            finally { await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); await File.WriteAllTextAsync(Path.Combine(output, "transitions.json"), System.Text.Json.JsonSerializer.Serialize(traces)); }
        }
        private static void WriteZoomSurfaceFixture(string target)
        {
            var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /XObject /Subtype /Image /Width 2 /Height 2 /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /ASCIIHexDecode /Length 25 >>\nstream\nFF000000FF000000FFFFFFFF>\nendstream" };
            var pages = new List<int>();
            for (var i = 0; i < 30; i++)
            {
                var w = i % 3 == 0 ? 595 : i % 3 == 1 ? 842 : 1191; var h = i % 3 == 0 ? 842 : i % 3 == 1 ? 1191 : 1684;
                if (i % 4 == 3) (w, h) = (h, w);
                var content = new System.Text.StringBuilder();
                for (var y = 30; y < h - 20; y += 65)
                    content.Append(System.Globalization.CultureInfo.InvariantCulture, $"0.9 0.94 1 rg 15 {y} {w - 30} 28 re f 0 0 0 rg BT /F1 16 Tf 30 {y + 8} Td (Synthetic page {i + 1} - row {y}) Tj ET\n");
                content.Append(System.Globalization.CultureInfo.InvariantCulture, $"q 120 0 0 120 {w - 150} {h - 160} cm /Im Do Q\n");
                pages.Add(objects.Count + 1);
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {w} {h}] /Resources << /Font << /F1 3 0 R >> /XObject << /Im 4 0 R >> >> /Contents {objects.Count + 2} 0 R >>");
                objects.Add($"<< /Length {content.Length} >>\nstream\n{content}endstream");
            }
            objects[1] = $"<< /Type /Pages /Count {pages.Count} /Kids [{string.Join(' ', pages.Select(p => p + " 0 R"))}] >>";
            var pdf = new System.Text.StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
            for (var i = 0; i < objects.Count; i++) { offsets.Add(pdf.Length); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
            var xref = pdf.Length; pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            File.WriteAllBytes(target, System.Text.Encoding.ASCII.GetBytes(pdf.ToString()));
        }
    }
}
#endif
