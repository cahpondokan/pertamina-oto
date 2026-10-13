#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditZoomAsync(bool interactive)
    {
        var output = Path.Combine(AppContext.BaseDirectory, "zoom-audit"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "library.json"); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear();
        var report = new List<string>();
        void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); report.Add("PASS " + name); }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(600); root.UpdateLayout(); }
        async Task Capture(string name)
        {
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root); var pixels = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray()); await encoder.FlushAsync();
        }
        try
        {
            var source = Path.Combine(output, "synthetic.pdf"); WriteHistoryAuditFixture(source);
            var hash = SHA256.HashData(File.ReadAllBytes(source));
            homeFiles.Add(source); chosenFiles.Add(source);
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1366, 768)); await Settle();
            foreach (var id in new[] { "merge", "split", "remove", "extract", "organize" })
            {
                previewScale = 1;
                if (id == "organize") { ShowOrganizer(); await AddOrganizerSourcesAsync(); }
                else
                {
                    openingEmptyInput = true; try { ShowTool(ToolCatalog.All.First(t => t.Id == id)); } finally { openingEmptyInput = false; }
                    files.Clear(); files.Add(source); await Settle();
                    if (id == "merge")
                    {
                        for (var i = 0; i < 8; i++) { var copy = Path.Combine(output, "copy-" + i + ".pdf"); File.Copy(source, copy, true); files.Add(copy); }
                        RefreshMergeTiles();
                    }
                    else if (id == "split") { await RefreshSplitSourceAsync(); splitMode = SplitMode.Fixed; splitStep = 1; UpdateSplitPlan(); RefreshSplitDesign(); }
                    else if (id == "remove") await RefreshRemoveSourceAsync();
                    else await RefreshExtractSourceAsync();
                }
                await Settle(); await Task.Delay(1500);
                var viewport = Descendants(body).OfType<ScrollViewer>().First(v => v.ZoomMode == ZoomMode.Enabled);
                var order = pageLayout.Select(p => p.Id).ToArray(); var inputs = files.ToArray();
                var selections = Descendants(viewport).OfType<CheckBox>().Select(c => (Box: c, Selected: c.IsChecked)).ToArray();
                Check(Descendants(viewport).OfType<Image>().Any(i => i.Source is not null), id + " real synthetic page thumbnails rendered");
                Check(ZoomController.Route(viewport, 0), id + " active viewport routing"); await Settle();
                ZoomController.Route(viewport, 1); await Settle(); ZoomController.Route(viewport, -1); await Settle();
                Check(Math.Abs(previewScale - 1) < .0001, id + " symmetric plus minus");
                var livePercent = true; var intermediate = 0;
                var zoomImage = Descendants(viewport).OfType<Image>().First(i => i.IsLoaded && i.Source is not null);
                var nativeSizes = new List<double>(); var blankFrames = 0;
                void Frame(object? sender, object e)
                {
                    if (zoomImage.IsLoaded) nativeSizes.Add(zoomImage.ActualWidth * viewport.ZoomFactor);
                    if (!Descendants(viewport).OfType<Image>().Any(i => i.IsLoaded && i.Source is not null)) blankFrames++;
                }
                void Observe(object? sender, ScrollViewerViewChangedEventArgs e)
                {
                    if (!e.IsIntermediate || previewScale != 1 || viewport.ZoomFactor <= 1.001) return;
                    intermediate++;
                    var field = Descendants(body).OfType<ComboBox>().First(c => AutomationProperties.GetName(c) == "Ukuran kartu dalam persen");
                    livePercent &= ZoomController.TryPercent(field.Text, .5, 2, out var value) && Math.Abs(value - viewport.ZoomFactor) < .00011;
                }
                CompositionTarget.Rendering += Frame;
                viewport.ViewChanged += Observe; viewport.ChangeView(null, null, 1.5f, false); await Settle();
                for (var attempt = 0; attempt < 30 && (Math.Abs(previewScale - 1.5) > .01 || Math.Abs(viewport.ZoomFactor - 1) > .001); attempt++) { await Task.Delay(100); root.UpdateLayout(); }
                viewport.ViewChanged -= Observe;
                CompositionTarget.Rendering -= Frame;
                await File.WriteAllTextAsync(Path.Combine(output, id + "-native-transition.json"), System.Text.Json.JsonSerializer.Serialize(new { nativeSizes, blankFrames }));
                Check(blankFrames == 0 && nativeSizes.Count > 0, id + " sampled native transition retains images");
                Check(nativeSizes.Zip(nativeSizes.Skip(1), (a, b) => b >= a - 2).All(x => x), id + " native commit has no backward size flash");
                Check(livePercent && intermediate > 0, id + " animated native intermediate percentage follows real scale");
                Check(Math.Abs(previewScale - 1.5) < .01 && Math.Abs(viewport.ZoomFactor - 1) < .001, id + $" native zoom settles into reflow once (scale={previewScale}, native={viewport.ZoomFactor})");
                Check(order.SequenceEqual(pageLayout.Select(p => p.Id)) && inputs.SequenceEqual(files), id + " zoom retains input and page order");
                Check(selections.All(c => c.Box.IsChecked == c.Selected), id + " zoom retains page selections");
                var percent = Descendants(body).OfType<ComboBox>().First(c => AutomationProperties.GetName(c) == "Ukuran kartu dalam persen");
                Check(percent.Text == "150%", id + " percentage follows settled native scale");
                for (var i = 0; i < 10; i++) ZoomController.Route(viewport, 1);
                await Settle(); Check(previewScale == 2 && !Descendants(body).OfType<Button>().First(b => AutomationProperties.GetName(b) == "Perbesar kartu").IsEnabled, id + " upper clamp disables plus");
                ZoomController.Route(viewport, 0); await Settle();
                if (id == "merge" && viewport.Content is GridView populated && viewport.ScrollableHeight > 200)
                {
                    viewport.ChangeView(null, 100, null, true); await Settle();
                    var anchor = Enumerable.Range(0, populated.Items.Count).Select(populated.ContainerFromIndex).OfType<FrameworkElement>()
                        .First(item => item.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y + item.ActualHeight >= 0);
                    var y = anchor.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y;
                    ZoomController.Route(viewport, 1); await Settle();
                    Check(Math.Abs(anchor.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y - y) < 3, "merge reflow preserves visible item anchor within 3 DIP");
                    ZoomController.Route(viewport, 0); await Settle();
                }
                foreach (var size in new[] { new Windows.Graphics.SizeInt32(1366, 768), new Windows.Graphics.SizeInt32(1920, 1080) })
                { AppWindow.Resize(size); await Settle(); await Capture(id + "-" + size.Width); }
                ZoomController.Route(viewport, 0); await Settle();
            }
            BeginScreen("Preview gambar", "Synthetic rotated preview"); SetHistoryPageVisible(false); body.Children.Clear();
            var rotated = SinglePagePreviewPanel(source, angle: 90); body.Children.Add(rotated); await Settle(); await Task.Delay(1000);
            var imageViewport = Descendants(rotated).OfType<ScrollViewer>().First(v => v.ZoomMode == ZoomMode.Enabled);
            Check(Descendants(rotated).OfType<Image>().Any(i => i.Source is not null), "rotated preview rendered");
            ZoomController.Route(imageViewport, 1); await Settle(); Check(imageViewport.ZoomFactor > 1, "rotated preview uses native document zoom"); await Capture("rotated-preview");
            var pixels = new byte[640 * 480 * 4];
            for (var i = 0; i < pixels.Length; i += 4) { var stripe = (i / 4 / 640) % 40 < 8; pixels[i] = 255; pixels[i + 1] = stripe ? (byte)120 : (byte)245; pixels[i + 2] = stripe ? (byte)40 : (byte)245; pixels[i + 3] = 255; }
            foreach (var extension in new[] { "png", "jpg" })
            {
                var imageFile = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync("synthetic." + extension, CreationCollisionOption.ReplaceExisting);
                using (var stream = await imageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(extension == "png" ? BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, 640, 480, 96, 96, pixels); await encoder.FlushAsync();
                }
                body.Children.Clear(); var panel = SinglePagePreviewPanel(imageFile.Path, angle: 90); body.Children.Add(panel); await Settle(); await Task.Delay(500);
                var view = Descendants(panel).OfType<ScrollViewer>().First(v => v.ZoomMode == ZoomMode.Enabled);
                Check(Descendants(panel).OfType<Image>().Any(i => i.Source is not null), extension + " synthetic image source rendered");
                ZoomController.Route(view, 0); await Settle(); ZoomController.Route(view, 1); await Settle(); ZoomController.Route(view, -1); await Settle();
                Check(Math.Abs(view.ZoomFactor - 1) < .001, extension + " rotated image zoom round trip"); await Capture(extension + "-rotated");
            }
            Check(SHA256.HashData(File.ReadAllBytes(source)).SequenceEqual(hash), "all zoom paths leave source SHA256 unchanged");
            report.Add("NOT TESTED hardware pinch/multitouch and screen reader; native ChangeView is a state regression, not physical gesture evidence.");
            if (interactive) { await OpenPdfReaderAsync(source); await Settle(); }
        }
        catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
        finally { await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); }
    }
}
#endif
