#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
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
    // Parent runs this after integration, in the isolated audit preferences/session.
    internal async Task AuditMergeCloseProcessingAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool condition, string label)
        {
            report.Add((condition ? "PASS " : "FAIL ") + label);
            File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
            if (!condition) throw new InvalidOperationException(label);
        }
        async Task Capture(string name)
        {
            root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        var oldFolder = folder;
        var oldName = outputName.Text;
        try
        {
            ShowTool(ToolCatalog.All.Single(t => t.Id == "merge"));
            folder = Path.Combine(output, "synthetic-destination-" + new string('d', 80));
            outputName.Text = new string('n', 108) + ".pdf";
            // Display uses the final target supplied by ExecuteAsync, including collision suffix.
            var target = Path.Combine(folder, new string('n', 108) + " (2).pdf");
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            foreach (var closeSave in new[] { false, true })
            foreach (var size in new[] { (1400, 1100), (660, 810) })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(size.Item1, size.Item2));
                using var cancellation = new CancellationTokenSource();
                active = cancellation; cancel.IsEnabled = true;
                mergeCloseSaveInProgress = closeSave;
                SetMergeProcessing(true, target);
                await Capture((closeSave ? "close-save" : "ordinary") + "-" + size.Item1);
                var texts = Descendants(mergeProcessing!).OfType<TextBlock>().ToArray();
                Check(texts.Any(t => t.Text == "Sedang menggabungkan PDF…"), "processing heading present");
                Check(texts.Any(t => t.Text == (closeSave ? "Memproses dan menyimpan hasil di perangkat ini."
                    : $"Memproses {files.Count} PDF di perangkat Anda. Persentase proses belum tersedia.")), "mode-specific message retained");
                Check(Descendants(mergeProcessing!).OfType<ProgressBar>().Single().IsIndeterminate, "indeterminate without percentage or ETA");
                Check(Descendants(mergeProcessing!).OfType<Image>().Any(i => i.Source is BitmapImage { UriSource: not null } image
                    && image.UriSource.LocalPath.EndsWith("028-gabungkan-pdf.png", StringComparison.OrdinalIgnoreCase)), "canonical Merge Biblo");
                var destination = texts.SingleOrDefault(t => AutomationProperties.GetName(t).StartsWith("Nama file hasil:", StringComparison.Ordinal));
                Check(closeSave ? destination is not null && destination.Text.Contains(" (2).pdf", StringComparison.Ordinal)
                    && destination.TextTrimming == TextTrimming.CharacterEllipsis && destination.MaxLines == 2
                    && AutomationProperties.GetName(destination).Contains(folder, StringComparison.Ordinal) : destination is null,
                    "destination only close-save with final filename and full accessible folder");
                var stop = Descendants(mergeProcessing!).OfType<Button>().Single();
                Check(stop.ActualHeight >= 44 && stop.ActualWidth >= 44 && stop.IsEnabled && stop.Focus(FocusState.Keyboard), "cancel measured hit target and keyboard focus");
                cancel.IsEnabled = false; root.UpdateLayout();
                Check(!stop.IsEnabled, "cancel retains one-way enabled binding");
                cancel.IsEnabled = true; root.UpdateLayout();
                ((IInvokeProvider)new ButtonAutomationPeer(stop).GetPattern(PatternInterface.Invoke)).Invoke();
                await Task.Delay(50);
                Check(cancellation.IsCancellationRequested && !stop.IsEnabled && texts.Any(t => t.Text == "Membatalkan proses…"), "native cancel invokes existing token and clear cancelling status");
                active = null; cancel.IsEnabled = false; SetMergeProcessing(false);
                Check(mergeProcessing is null && mergeInput?.Visibility == Visibility.Visible, "processing recovery restores inputs");
            }
        }
        finally
        {
            active = null; cancel.IsEnabled = false; SetMergeProcessing(false);
            mergeCloseSaveInProgress = false; folder = oldFolder; outputName.Text = oldName;
        }
    }
}
#endif
