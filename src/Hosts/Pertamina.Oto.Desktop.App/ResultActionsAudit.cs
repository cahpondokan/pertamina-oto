#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    internal async Task AuditResultActionsAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "result-actions-audit"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "isolated-library.json"); libraryUnreadable = false;
        var report = new List<string>();
        void Check(bool pass, string label) { if (!pass) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Items(DependencyObject parent)
        {
            yield return parent;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                foreach (var child in Items(VisualTreeHelper.GetChild(parent, i))) yield return child;
        }
        Button[] Buttons(string label) => Items(root).OfType<Button>().Where(b => AutomationProperties.GetName(b) == label).ToArray();
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task Capture(string name)
        {
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
        }
        async Task Panel(string[] paths) { ShowUnifiedResult(paths, "Synthetic saved result", ShowHome); root.UpdateLayout(); await Task.Delay(800); }
        async Task Read(Button button, string expected)
        {
            Invoke(button);
            for (var i = 0; i < 150 && pdfReader?.ReaderSource != expected; i++) await Task.Delay(100);
            Check(pdfReader?.Visible == true && pdfReader.ReaderSource == expected, "OTO reader opens exact output " + Path.GetFileName(expected));
        }
        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000));
            var folder = Path.Combine(output, "Reveal fixture Ω hasil"); Directory.CreateDirectory(folder);
            var first = Path.Combine(folder, "Hasil Ω & [01].pdf"); var second = Path.Combine(folder, "Hasil Ω & [02].pdf");
            var original = Path.Combine(output, "synthetic-original.pdf"); WriteHistoryAuditFixture(original);
            File.Copy(original, first, true); File.Copy(original, second, true);
            var before = new[] { original, first, second }.Select(p => SHA256.HashData(File.ReadAllBytes(p))).ToArray();
            homeFiles.Add(original); chosenFiles.Add(original);
            await Panel([first]);
            Check(Buttons("Buka hasil").Length == 1 && Buttons("Tampilkan di folder").Length == 1, "single result has one open and one exact reveal action");
            Check(!Items(root).OfType<Button>().Any(b => AutomationProperties.GetName(b).Contains("Buka folder", StringComparison.Ordinal) || AutomationProperties.GetName(b).StartsWith('▱')), "duplicate folder button removed");
            await Capture("single-result"); await Read(Buttons("Buka hasil")[0], first);
            Check(chosenFiles.Contains(original) && homeFiles.Contains(original), "reading result preserves original source selection");
            await Panel([first, second]);
            Check(Buttons("Buka hasil").Length == 2 && Buttons("Tampilkan di folder").Length == 2 && Buttons("Cetak hasil ini").Length == 2, "multiple results expose separate actions per file");
            await Capture("multiple-results"); await Read(Buttons("Buka hasil")[1], second);
            var printChecked = false; string? printFailure = null;
            auditPrintPrinters = () => [];
            auditPrintDialog = async dialog =>
            {
                try
                {
                    for (var i = 0; i < 30 && !Items(dialog).OfType<TextBlock>().Any(t => t.Text == Path.GetFileName(second)); i++) { dialog.UpdateLayout(); await Task.Delay(100); }
                    Check(Items(dialog).OfType<TextBlock>().Any(t => t.Text == Path.GetFileName(second)), "print dialog receives selected second result"); printChecked = true;
                }
                catch (InvalidOperationException error) { printFailure = error.Message; }
                finally { dialog.Hide(); }
            };
            await Panel([first, second]); Invoke(Buttons("Cetak hasil ini")[1]);
            for (var i = 0; i < 150 && !printChecked && printFailure is null; i++) await Task.Delay(100);
            Check(printChecked && printFailure is null, "existing print dialog opened without submitting a job: " + printFailure);
            for (var i = 0; i < 100 && messageOpen; i++) await Task.Delay(100);
            var missing = Path.Combine(folder, "Missing Ω.pdf"); await Panel([missing]);
            foreach (var label in new[] { "Buka hasil", "Cetak hasil", "Tampilkan di folder" })
            {
                Invoke(Buttons(label)[0]); await Task.Delay(100);
                Check(Items(root).OfType<TextBlock>().Any(t => t.Text.Contains("File hasil tidak ditemukan", StringComparison.Ordinal)), "missing output visibly rejected by " + label);
            }
            await Capture("missing-output");
            var nonPdf = Path.Combine(folder, "Hasil Ω.txt"); File.WriteAllText(nonPdf, "Synthetic result fixture"); await Panel([nonPdf]);
            Check(Buttons("↗  Buka file").Length == 1 && Buttons("Cetak hasil").Length == 0 && Buttons("Tampilkan di folder").Length == 1, "non-PDF open/reveal retained without print capability");
            foreach (var pair in new[] { original, first, second }.Select((p, i) => (p, i))) Check(SHA256.HashData(File.ReadAllBytes(pair.p)).SequenceEqual(before[pair.i]), "source/output hash preserved " + Path.GetFileName(pair.p));
            foreach (var stage in new[] { "single", "multiple-second" })
            {
                await Panel(stage == "single" ? [first] : [first, second]);
                Invoke(Buttons("Tampilkan di folder")[stage == "single" ? 0 : 1]);
                await File.WriteAllTextAsync(Path.Combine(output, "reveal-" + stage + "-ready.txt"), stage == "single" ? first : second);
                // Keep only this isolated fixture alive for native Explorer inspection.
                var inspected = Path.Combine(output, "inspection-" + stage + "-complete.txt");
                for (var i = 0; i < 600 && !File.Exists(inspected); i++) await Task.Delay(500);
                Check(File.Exists(inspected), "native inspection handoff completed for " + stage);
            }
            report.Add("EXTERNAL_EVIDENCE_REQUIRED Explorer selected_elements/screenshot for each stage; handoff markers/API return alone are insufficient.");
        }
        catch (Exception error) { report.Add("FAIL " + error.Message); Environment.ExitCode = 1; }
        finally { auditPrintDialog = null; auditPrintPrinters = null; libraryTestPath = null; await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); }
    }
}
#endif
