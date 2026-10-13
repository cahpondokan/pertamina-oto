#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal static void PrepareSavedResultAudit(string output)
    {
        PrepareCenteredAudit(output);
        restartAuditPreference = Path.Combine(output, "preferences.json");
        File.WriteAllText(restartAuditPreference, System.Text.Json.JsonSerializer.Serialize(new { DefaultFolder = output }));
    }

    internal async Task AuditSavedResultViewportAsync(string output)
    {
        var report = new List<string>();
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
            if (!ok) throw new InvalidOperationException(label);
        }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(350); root.UpdateLayout(); }
        async Task Capture(string name)
        {
            await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        Button Button(string label) => Descendants(latestResultCard!).OfType<Button>().Single(b => AutomationProperties.GetName(b) == label);
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task WaitPreview()
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                root.UpdateLayout();
                if (Descendants(latestResultCard!).OfType<Image>().Single(i => i.Name != "SavedMergeBiblo").Source is not null) return;
                await Task.Delay(100);
            }
            Check(false, "saved preview did not load within bounded wait");
        }
        void Fit(string label)
        {
            Check(standardPageScroll!.ScrollableHeight <= 1, label + " no page scrolling: " + standardPageScroll.ScrollableHeight);
            Check(latestResultCard!.ActualHeight > 200, label + " usable bounded result card");
            foreach (var button in Descendants(latestResultCard).OfType<Button>())
            {
                var rect = button.TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, button.ActualWidth, button.ActualHeight));
                Check(rect.Left >= -1 && rect.Right <= root.ActualWidth + 1 && rect.Top >= -1 && rect.Bottom <= root.ActualHeight + 1,
                    label + " visible action " + AutomationProperties.GetName(button));
                Check(button.ActualHeight >= 44, label + " 44 DIP action " + AutomationProperties.GetName(button));
            }
        }
        try
        {
            selected = ToolCatalog.All.Single(t => t.Id == "merge");
            var a = Path.Combine(output, "synthetic-source.pdf"); WriteHistoryAuditFixture(a);
            var path = Path.Combine(output, "Laporan_gabungan.pdf");
            var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(a)));
            var merged = await WorkerClient.RunAsync(new("merge", [a, a], path), CancellationToken.None);
            Check(merged.Success && merged.Pages == 6, "synthetic saved result generated through existing merge worker");
            var savedHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            foreach (var size in new[] { (660, 810), (1000, 850), (1400, 1000) })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(size.Item1, size.Item2));
                ShowUnifiedResult([path], "6 halaman · Hasil sintetis", ShowHome); await Settle(); await Task.Delay(650); await Settle();
                await WaitPreview(); await Settle();
                Fit("window " + size.Item1 + "x" + size.Item2);
                var close = Button("Tutup dokumen");
                var closeColors = (ResourceDictionary)close.Resources.ThemeDictionaries["Default"];
                Check(close.Content is TextBlock { Text: "×  Tutup dokumen" }
                    && ((Microsoft.UI.Xaml.Media.SolidColorBrush)closeColors["ButtonBackground"]).Color == Windows.UI.Color.FromArgb(255, 255, 241, 242)
                    && ((Microsoft.UI.Xaml.Media.SolidColorBrush)closeColors["ButtonBorderBrush"]).Color == Windows.UI.Color.FromArgb(255, 245, 182, 189)
                    && ((Microsoft.UI.Xaml.Media.SolidColorBrush)closeColors["ButtonForeground"]).Color == Windows.UI.Color.FromArgb(255, 180, 35, 53),
                    "saved close matches loaded Merge blush styling " + size.Item1);
                Check(closeColors.ContainsKey("ButtonBackgroundPointerOver") && closeColors.ContainsKey("ButtonBackgroundPressed")
                    && closeColors.ContainsKey("ButtonBackgroundDisabled") && close.Resources.ThemeDictionaries.ContainsKey("HighContrast"),
                    "saved close preserves native pointer pressed disabled highcontrast states " + size.Item1);
                var biblo = Descendants(latestResultCard!).OfType<Image>().Single(i => i.Name == "SavedMergeBiblo");
                Check(biblo.Source is BitmapImage source && source.UriSource?.LocalPath == Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", "028-gabungkan-pdf.png"),
                    "canonical Merge Biblo at " + size.Item1);
                Check(biblo.ActualWidth >= 72 && biblo.ActualWidth <= 85 && biblo.ActualHeight >= 72 && biblo.ActualHeight <= 85,
                    "compact Merge Biblo at " + size.Item1);
                Check(Descendants(latestResultCard!).OfType<Image>().Single(i => i.Name != "SavedMergeBiblo").Source is not null, "preview loaded at " + size.Item1);
                foreach (var label in new[] { "Buka hasil", "Cetak hasil", "Tampilkan di folder", "←  Kembali edit", "Tutup dokumen", "Salin ringkasan" })
                    Check(Button(label).Focus(FocusState.Keyboard), "keyboard focus " + size.Item1 + " " + label);
                await Capture("result-" + size.Item1);
            }
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter max) max.Maximize();
            await Settle(); Fit("maximized"); await Capture("result-maximized");
            var longPath = Path.Combine(output, new string('L', 112) + ".pdf"); File.Copy(path, longPath, true);
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 810));
            selected = ToolCatalog.All.Single(t => t.Id == "merge");
            ShowUnifiedResult([longPath], new string('d', 700) + Environment.NewLine + "Detail baris kedua", ShowHome); await Settle(); Fit("long name and pending detail");
            await WaitPreview(); await Settle(); Fit("long name and detail"); await Capture("result-long-name");
            Check(Descendants(latestResultCard!).OfType<TextBlock>().Any(t => t.Text == longPath && t.IsTextSelectionEnabled), "full path accessible in inline details");
            Check(!Descendants(latestResultCard!).OfType<Button>().Any(b => AutomationProperties.GetName(b) == "Detail hasil"), "redundant detail popup removed");
            Check(Button("Salin ringkasan").IsEnabled, "inline copy summary command remains available");
            report.Add("SKIP clipboard mutation: copy action is outside this scope and is not invoked.");
            Fit("inline details"); await Capture("result-details");
            var printOpened = false;
            auditPrintPrinters = () => [];
            auditPrintDialog = async dialog =>
            {
                await Task.Delay(200);
                printOpened = Descendants(dialog).OfType<TextBlock>().Any(t => t.Text == Path.GetFileName(longPath));
                dialog.Hide();
            };
            Invoke(Button("Cetak hasil"));
            for (var i = 0; i < 60 && !printOpened; i++) await Task.Delay(100);
            Check(printOpened, "existing print dialog receives exact saved result; no print submission");
            auditPrintPrinters = null; auditPrintDialog = null;
            var backCalled = false; var closeCalled = false;
            selected = ToolCatalog.All.Single(t => t.Id == "merge");
            ShowUnifiedResult([path], "Hasil sintetis", () => backCalled = true, restart: () => closeCalled = true);
            await Settle(); Invoke(Button("←  Kembali edit")); Invoke(Button("Tutup dokumen")); await Settle();
            Check(backCalled && closeCalled, "back and close callbacks preserved");
            Invoke(Button("Buka hasil"));
            for (var i = 0; i < 80 && pdfReader?.Visible != true; i++) await Task.Delay(100);
            Check(pdfReader?.Visible == true && pdfReader.ReaderSource == path, "existing reader opens exact saved output");
            ShowHome(); await Settle();
            selected = ToolCatalog.All.Single(t => t.Id == "merge");
            ShowUnifiedResult([Path.Combine(output, "missing.pdf")], "Hasil sintetis", ShowHome); await Settle();
            Invoke(Button("Buka hasil")); await Settle(); Fit("missing result notice");
            Check(Descendants(latestResultCard!).OfType<TextBlock>().Any(t => t.Visibility == Visibility.Visible && t.Text.Contains("File hasil tidak ditemukan", StringComparison.Ordinal)), "missing output notice visible without growing page");
            await Capture("result-error");
            ShowHome(); await Settle(); Check(latestResultCard?.IsLoaded != true, "leaving result releases viewport sizing");
            ShowUnifiedResult([path], "Hasil PDF umum", ShowHome); await Settle();
            Check(!Descendants(latestResultCard!).OfType<Image>().Any(i => i.Name == "SavedMergeBiblo"), "non-Merge result preserves generic success header");
            Check(Descendants(latestResultCard!).OfType<Image>().Single().Source is not null, "non-Merge preview remains loaded");
            Fit("non-Merge result");
            selected = ToolCatalog.All.Single(t => t.Id == "merge");
            ShowUnifiedResult([path], "6 halaman · Hasil sintetis", ShowHome); await Settle(); Fit("return to result"); await Capture("result-final");
            Check(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(a))) == hash && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) == savedHash,
                "source and saved result SHA256 unchanged by UI actions");
        }
        catch (Exception error)
        {
            report.Add("FAIL " + error); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = 1;
            try { await Capture("failure"); } catch (Exception) { /* Preserve the original assertion failure. */ }
        }
        finally { auditPrintPrinters = null; auditPrintDialog = null; }
    }
}
#endif
