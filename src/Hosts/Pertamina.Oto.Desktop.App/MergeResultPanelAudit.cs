#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal static void PrepareMergePanelAudit(string output)
    {
        PrepareCenteredAudit(output);
        restartAuditPreference = Path.Combine(output, "isolated-preferences.json");
        File.WriteAllText(restartAuditPreference, System.Text.Json.JsonSerializer.Serialize(new { DefaultFolder = output }));
    }
    internal async Task AuditMergeResultPanelAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string label) { report.Add((ok ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); if (!ok) throw new InvalidOperationException(label); }
        async Task Capture(string name)
        {
            root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        try
        {
            var a = Path.Combine(output, "synthetic-first.pdf"); var b = Path.Combine(output, "synthetic-second.pdf");
            WriteHistoryAuditFixture(a, 595, 842); WriteHistoryAuditFixture(b, 420, 595);
            var hashes = new[] { a, b }.Select(p => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)))).ToArray();
            folder = output; location.Text = "Folder hasil: " + folder;
            homeFiles.Add(a); chosenFiles.Add(a);
            ShowTool(ToolCatalog.All.Single(t => t.Id == "merge"));
            await AcceptMergeFilesAsync([await StorageFile.GetFileFromPathAsync(a)]);
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1100));
            Check(files.Count == 1 && !run.IsEnabled, "one PDF keeps native primary action disabled");
            Check(mergeCancelButton?.Visibility == Visibility.Collapsed, "cancel hidden while idle");
            await Capture("one-pdf");
            await AcceptMergeFilesAsync([await StorageFile.GetFileFromPathAsync(b)]);
            Check(files.Count == 2 && run.IsEnabled, "two PDF readiness retains original predicate");
            Check(mergeReadyTitle?.Text == "2 PDF siap digabungkan.", "correct readiness count");
            await Capture("ready");
            var button = Descendants(toolOptions).OfType<Button>().Single(x => AutomationProperties.GetName(x) == "Gabungkan PDF");
            Check(button.Content is TextBlock { Text: "Gabungkan PDF", TextAlignment: TextAlignment.Center }
                && !Descendants(button).OfType<Image>().Any(), "primary action contains only centered text without Biblo or placeholder");
            Check(button.HorizontalContentAlignment == HorizontalAlignment.Center
                && button.VerticalContentAlignment == VerticalAlignment.Center, "primary action content centered without icon gap");
            Check(Descendants(mergeInput!).OfType<Image>().Any()
                && Descendants(toolWorkspace).OfType<StackPanel>().Any(panel =>
                    panel.Children.OfType<TextBlock>().Any(text => text.Text == "Hasil gabungan")
                    && panel.Children.OfType<Image>().Any()), "workspace and result headers retain Biblo");
            Check(button.IsEnabled && button.MinHeight >= 44 && button.Focus(FocusState.Keyboard), "native primary action binding and keyboard focus");
            await Capture("keyboard-focus");
            var filenameInput = Descendants(toolOptions).OfType<TextBox>().Single(x => AutomationProperties.GetName(x) == "Nama file hasil");
            filenameInput.Text = new string('a', 112) + ".pdf";
            Check(outputName.Text.Length == 116, "long full filename retained with single pdf suffix");
            Check(filenameInput.MinHeight >= 44 && filenameInput.MaxLength == outputName.MaxLength, "scoped filename control keeps length limit and native click target");
            folder = Path.Combine(output, "folder-with-a-long-name-for-native-panel-validation", "another-long-destination-segment");
            Directory.CreateDirectory(folder); location.Text = "Folder hasil: " + folder;
            var expander = toolOptions.Children.OfType<Expander>().Single();
            Check(!expander.IsExpanded, "password collapsed by default");
            expander.IsExpanded = true; await Capture("long-name-folder-password");
            expander.IsExpanded = false;
            filenameInput.Text = "invalid/name.pdf"; await ExecuteAsync();
            Check(status.Text.StartsWith("Masukkan nama file yang valid", StringComparison.Ordinal) && active is null, "existing filename validation rejects invalid path before worker");
            UpdateMergeReady();
            Check(mergeReadiness?.Visibility == Visibility.Collapsed, "error retained without misleading ready card");
            await Capture("error");
            RefreshMergeTiles();
            Check(mergeReadyTitle?.Text == "Periksa nama file hasil.", "invalid filename does not advertise a ready result after list refresh");
            filenameInput.Text = new string('a', 112) + ".pdf";
            using (var cancellation = new CancellationTokenSource())
            {
                active = cancellation; cancel.IsEnabled = true; run.IsEnabled = false; SetMergeProcessing(true);
                Check(mergeCancelButton?.Visibility == Visibility.Visible && mergeReadiness?.Visibility == Visibility.Collapsed, "busy exposes cancel and hides ready card");
                Check(Descendants(mergeProcessing!).OfType<Image>().Any(i => i.Source is BitmapImage { UriSource: not null } image && image.UriSource.LocalPath.EndsWith("028-gabungkan-pdf.png", StringComparison.OrdinalIgnoreCase)), "busy contains canonical Biblo bitmap");
                Check(Descendants(mergeProcessing!).OfType<ProgressBar>().Single().IsIndeterminate, "busy does not invent percent progress");
                await Capture("busy-cancel");
                cancellation.Cancel(); Check(cancellation.IsCancellationRequested, "existing cancellation token usable");
                active = null; cancel.IsEnabled = false; SetMergeProcessing(false); RefreshMergeTiles();
                Check(mergeProcessing is null && mergeInput!.Visibility == Visibility.Visible, "cancel recovery restores input workspace");
            }
            foreach (var size in new[] { (660, 810), (1000, 900), (1400, 1100) })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(size.Item1, size.Item2)); await Capture("size-" + size.Item1);
                Check(button.ActualWidth > 100, "primary action measured at window pixel width " + size.Item1);
                standardPageScroll?.ChangeView(null, standardPageScroll.ScrollableHeight, null, true);
                await Capture("size-" + size.Item1 + "-bottom");
                standardPageScroll?.ChangeView(null, 0, null, true);
            }
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter max) max.Maximize();
            await Capture("maximized");
            outputName.Text = "native-panel-merge.pdf";
            var destination = Path.Combine(folder, outputName.Text);
            await ExecuteAsync();
            Check(resultPath == destination && File.Exists(destination), "existing ExecuteAsync processes and saves directly to selected folder and name");
            var pdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(destination));
            Check(pdf.PageCount == 6, "saved merge output reopens with all six fixture pages");
            var sourceA = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(a));
            var sourceB = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(b));
            using (var first = pdf.GetPage(0)) using (var second = pdf.GetPage(3))
            using (var originalA = sourceA.GetPage(0)) using (var originalB = sourceB.GetPage(0))
                Check(first.Size == originalA.Size && second.Size == originalB.Size, "output follows document order");
            Check(new[] { a, b }.Select(p => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)))).SequenceEqual(hashes), "both source SHA256 hashes unchanged");
            homeFiles.Clear(); homeFiles.Add(a); chosenFiles.Clear(); chosenFiles.Add(a);
            ShowTool(ToolCatalog.All.Single(t => t.Id == "merge"));
            await AcceptMergeFilesAsync([await StorageFile.GetFileFromPathAsync(a), await StorageFile.GetFileFromPathAsync(b)]);
            outputName.Text = "cancelled-merge.pdf";
            var cancelledPath = Path.Combine(folder!, outputName.Text);
            var executing = ExecuteAsync();
            Check(active is not null, "real merge enters cancellable busy state");
            active!.Cancel(); await executing;
            Check(!File.Exists(cancelledPath) && status.Text.Contains("dibatalkan", StringComparison.Ordinal), "real worker cancellation leaves no final result");
            Check(mergeReadiness?.Visibility == Visibility.Collapsed, "cancellation message remains visible without ready card");
            RefreshMergeTiles();
            outputName.Text = "Laporan_gabungan.pdf";
            await Capture("final-ready");
            Check(!report.Any(x => x.StartsWith("FAIL", StringComparison.Ordinal)), "all scoped native checks passed");
        }
        catch (Exception e) { report.Add("FAIL " + e); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = 1; }
    }
}
#endif
