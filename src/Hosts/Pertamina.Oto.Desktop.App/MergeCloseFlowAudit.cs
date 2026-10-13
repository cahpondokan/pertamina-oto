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
    internal async Task AuditMergeCloseFlowAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label);
            File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
            if (!ok) throw new InvalidOperationException(label);
        }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(200); root.UpdateLayout(); }
        async Task Capture(UIElement element, string name)
        {
            await Settle();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(element);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        ContentDialog Popup() => VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
            .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().First();
        void Invoke(string name)
        {
            var button = Descendants(Popup()).OfType<Button>().Single(b => b.Name == name);
            ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        }
        var tool = ToolCatalog.All.Single(t => t.Id == "merge");
        var a = Path.Combine(output, "synthetic-first.pdf");
        var b = Path.Combine(output, "synthetic-second.pdf");
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        async Task Open(string name, bool missing = false)
        {
            openingEmptyInput = true;
            try { ShowTool(tool); } finally { openingEmptyInput = false; }
            folder = output;
            await AcceptMergeFilesAsync([await StorageFile.GetFileFromPathAsync(a), await StorageFile.GetFileFromPathAsync(b)]);
            if (missing) { files.Remove(b); files.Add(Path.Combine(output, "missing.pdf")); }
            outputName.Text = name; await Settle();
        }
        try
        {
            WriteHistoryAuditFixture(a, 595, 842); WriteHistoryAuditFixture(b, 420, 595);
            var hashes = new[] { Hash(a), Hash(b) };
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
            foreach (var width in new[] { 1400, 660 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 1100));
                await Open("retained.pdf");
                foreach (var line in await AuditMergeWorkspaceRevisionAsync()) Check(line.StartsWith("PASS ", StringComparison.Ordinal), width + ": " + line);
                var limits = body.Children.OfType<Expander>().Single();
                limits.IsExpanded = true; await Capture(root, "limits-expanded-" + width); limits.IsExpanded = false;
                foreach (var line in AuditMergeCloseButton()) Check(line.StartsWith("PASS ", StringComparison.Ordinal), width + ": " + line);
                await Capture(root, "loaded-" + width);
                foreach (var line in await AuditMergeCloseConfirmationAsync(async (dialog, name) =>
                {
                    var background = Descendants(dialog).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
                    await Capture(background, name + "-" + width);
                })) if (line.StartsWith("PASS ", StringComparison.Ordinal)) Check(true, width + ": " + line);
            }
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1100));
            await Open("retained.pdf");
            await AuditAppExitRetentionAsync(Check);
            var actionButtons = Descendants(mergeCards[a]).OfType<Button>().ToArray();
            ((IInvokeProvider)new ButtonAutomationPeer(actionButtons[0]).GetPattern(PatternInterface.Invoke)).Invoke();
            Check(mergeAngles[a] == 90 && files.SequenceEqual(new[] { a, b }), "hover rotate updates only session angle and preserves order");
            var openedSource = false;
            auditReadDialog = _ => { openedSource = true; return Task.CompletedTask; };
            try
            {
                ((IInvokeProvider)new ButtonAutomationPeer(actionButtons[2]).GetPattern(PatternInterface.Invoke)).Invoke();
                for (var attempt = 0; attempt < 100 && (!openedSource || messageOpen); attempt++) await Task.Delay(100);
            }
            finally { auditReadDialog = null; }
            Check(openedSource && !messageOpen && files.Count == 2, "hover enlarge uses internal source preview and retains session");
            ((IInvokeProvider)new ButtonAutomationPeer(actionButtons[1]).GetPattern(PatternInterface.Invoke)).Invoke(); await Settle();
            Check(files.SequenceEqual(new[] { b }) && File.Exists(a), "hover remove excludes input only and preserves source");
            files.Insert(0, a); mergeAngles[a] = 0; RefreshMergeTiles();
            var keep = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseReturn"); await keep;
            Check(files.Count == 2 && outputName.Text == "retained.pdf" && !mergeCloseSaveInProgress && !messageOpen, "return retains session, name and releases close guards");
            var save = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseSave"); await save; await Settle();
            var destination = Path.Combine(output, "retained.pdf");
            Check(File.Exists(destination) && files.Count == 0 && resultPath is null && MergeEmptyActive && !RestartHasSession, "actual close-save persists result then clears only session and returns to empty Merge");
            var banner = body.Children.OfType<Border>().Single(b => b.Tag is MergeClosedNoticeState);
            var notice = (MergeClosedNoticeState)banner.Tag;
            Check(notice.SavedPath == destination && !mergeCloseSaveInProgress && !messageOpen, "success banner keeps exact immutable saved path after result cleared");
            var pdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(destination));
            Check(pdf.PageCount == 6, "close-save output reopens with six pages");
            var sourceA = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(a));
            var sourceB = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(b));
            using (var first = pdf.GetPage(0)) using (var second = pdf.GetPage(3))
            using (var originalA = sourceA.GetPage(0)) using (var originalB = sourceB.GetPage(0))
                Check(first.Size == originalA.Size && second.Size == originalB.Size, "close-save preserves document order");
            await Capture(root, "closed-saved-success");
            var previewOpened = false;
            auditReadDialog = _ => { previewOpened = true; return Task.CompletedTask; };
            try { await notice.OpenAsync(); } finally { auditReadDialog = null; }
            Check(previewOpened && resultPath is null, "success Buka hasil uses captured path and internal preview");
            var savedHash = Hash(destination);
            await Open("discarded.pdf");
            var discard = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseDiscard"); await discard; await Settle();
            Check(files.Count == 0 && MergeEmptyActive && !body.Children.OfType<Border>().Any(x => x.Tag is MergeClosedNoticeState)
                && !File.Exists(Path.Combine(output, "discarded.pdf")) && Hash(destination) == savedHash, "discard closes without success claim and keeps previous saved output");
            await Open("failed.pdf", missing: true);
            var failure = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseSave"); await failure; await Settle();
            Check(files.Count == 2 && outputName.Text == "failed.pdf" && !MergeEmptyActive && !mergeCloseSaveInProgress
                && !messageOpen && !File.Exists(Path.Combine(output, "failed.pdf")), "failed close-save retains input/settings and no success");
            await Capture(root, "failed-retained");
            var clearFailed = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseDiscard"); await clearFailed;
            await Open("cancelled.pdf");
            var cancelled = CloseDocumentsAsync(); await Settle(); Invoke("MergeCloseSave");
            for (var attempt = 0; attempt < 200 && active is null && !cancelled.IsCompleted; attempt++) await Task.Delay(1);
            Check(active is not null && mergeCloseSaveInProgress, "actual close-save enters cancellable processing mode");
            active!.Cancel(); await cancelled; await Settle();
            Check(files.Count == 2 && outputName.Text == "cancelled.pdf" && !mergeCloseSaveInProgress && !messageOpen
                && !File.Exists(Path.Combine(output, "cancelled.pdf")) && !MergeEmptyActive, "cancelled close-save retains draft and source settings");
            Check(new[] { Hash(a), Hash(b) }.SequenceEqual(hashes) && Hash(destination) == savedHash, "all source and prior saved hashes unchanged across save/discard/fail/cancel");
            await AuditMergeCloseProcessingAsync(Path.Combine(output, "processing"));
            Check(true, "four-mode processing layout/token audit passed");
            await AuditMergeClosedSavedNoticeAsync(Path.Combine(output, "notice"));
            Check(Environment.ExitCode == 0, "saved-notice path/missing/preview/responsive audit passed");
            Check(!mergeCloseSaveInProgress, "mode flag reset after audits");
            report.Add("LIMIT native programmatic input, current DPI only; physical hover/keyboard/high contrast toggle/Explorer/network-disabled UAT not claimed.");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        finally { File.WriteAllLines(Path.Combine(output, "audit.txt"), report); auditReadDialog = null; }
    }
}
#endif
