#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal static void PrepareMergeEmptyAudit(string output) => PrepareCenteredAudit(output);

    internal async Task AuditMergeEmptyAsync(string output)
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
        async Task Capture(string name)
        {
            await Settle();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        try
        {
            var tool = ToolCatalog.All.Single(t => t.Id == "merge");
            foreach (var width in new[] { 1200, 800, 660 })
            {
                if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 900));
                ShowEmptyTool(tool); await Settle();
                Check(MergeEmptyActive && ReferenceEquals(mergeEmptyDropSurface, standardPageScroll) && standardPageScroll!.AllowDrop, $"{width}: work viewport accepts drop outside card");
                Check(emptyPanel!.Child is StackPanel card && card.Children.OfType<Image>().Count() == 1, $"{width}: single packaged Biblo image");
                Check(!IsTextPasteTarget(Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject), $"{width}: paste ready without click");
                Check(standardPageScroll!.ScrollableHeight <= 1, $"{width}: empty page fits viewport");
                var stack = (StackPanel)emptyPanel.Child;
                var buttons = stack.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<Button>()).ToArray();
                var pasteButton = buttons.Single(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Tempel file");
                var pickButton = buttons.Single(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Pilih PDF");
                Check(((StackPanel)pasteButton.Content).Children.Count == 2 && ((StackPanel)pasteButton.Content).Children.OfType<TextBlock>().Single().Text == "Tempel file", $"{width}: paste contains icon and label only, no permanent shortcut");
                var pasteTip = (ToolTip)ToolTipService.GetToolTip(pasteButton);
                Check((string)pasteTip.Content == "Ctrl+V", $"{width}: native paste tooltip is exact Ctrl+V");
                var pickTip = (ToolTip)ToolTipService.GetToolTip(pickButton);
                Check((string)pickTip.Content == "Ctrl+O", $"{width}: native picker tooltip is exact Ctrl+O");
                Check(pickButton.KeyboardAccelerators.Count == 0 && pasteButton.KeyboardAccelerators.Count == 0, $"{width}: buttons do not duplicate global shortcuts");
                Check(Math.Abs(pickButton.ActualWidth - pasteButton.ActualWidth) < 1 && pickButton.ActualHeight >= 44 && pasteButton.ActualHeight >= 44, $"{width}: equal-width accessible controls");
                Check(stack.Children.OfType<TextBlock>().Single(t => t.Text.StartsWith("Pilih minimal", StringComparison.Ordinal)).Text == "Pilih minimal 2 PDF untuk digabungkan.", $"{width}: minimum requirement once under heading");
                Check(emptyNotice!.Text.Length == 0 && emptyNotice.Visibility == Visibility.Collapsed, $"{width}: no duplicate idle notice");
                Check(mergeEmptyDropHint!.Text == "atau seret PDF ke area ini", $"{width}: single drop instruction");
                var originalHeight = emptyPanel.ActualHeight;
                SetMergeEmptyDragHighlight(true); await Settle();
                Check(mergeEmptyDropHint.Text == "Lepaskan untuk menambahkan PDF" && Math.Abs(emptyPanel.ActualHeight - originalHeight) < 1, $"{width}: drag feedback without layout shift");
                await Capture("merge-empty-drag-" + width);
                SetMergeEmptyDragHighlight(false);
                await Capture("merge-empty-" + width);
                if (width == 1200)
                {
                    pickTip.IsOpen = true; await Settle();
                    Check(pickTip.IsOpen, "native picker tooltip opens programmatically");
                    await Capture("merge-empty-pick-tooltip"); pickTip.IsOpen = false;
                    pasteTip.IsOpen = true; await Settle();
                    Check(pasteTip.IsOpen, "native tooltip opens programmatically");
                    await Capture("merge-empty-paste-tooltip"); pasteTip.IsOpen = false;
                }
                ShowHome(); await Settle();
                Check(mergeEmptyDropSurface is null && !MergeEmptyActive, $"{width}: navigation detaches drop handlers");
            }
            ShowEmptyTool(tool);
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter maximized) maximized.Maximize();
            await Settle(); await Capture("merge-empty-maximized");
            Check(standardPageScroll!.ScrollableHeight <= 1, "maximized: page fits viewport");
            var a = Path.Combine(output, "synthetic-first.pdf"); var b = Path.Combine(output, "synthetic-second.pdf");
            WriteHistoryAuditFixture(a, 595, 842); WriteHistoryAuditFixture(b, 420, 595);
            var first = await StorageFile.GetFileFromPathAsync(a); var second = await StorageFile.GetFileFromPathAsync(b);
            var textPath = Path.Combine(output, "unsupported.txt"); File.WriteAllText(textPath, "synthetic");
            ShowEmptyTool(tool); await Settle();
            await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(textPath)]);
            Check(MergeEmptyActive && emptyNotice!.Text == "Belum ada file dengan format yang sesuai.", "unsupported intake stays on empty Merge");
            await AcceptEmptyInputsAsync([first, first, second]); await Settle();
            Check(files.Count == 2 && files.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 2 && selected?.Id == "merge", "multi-file intake removes duplicates and opens Merge");
            Check(mergeEmptyDropSurface is null, "intake navigation detaches viewport handlers");
            ShowEmptyTool(tool); await Settle();
            pickingMergeEmptyInput = true;
            try { await PickEmptyToolAsync(); Check(MergeEmptyActive && pickingMergeEmptyInput, "Merge picker reentry returns without another dialog"); }
            finally { pickingMergeEmptyInput = false; }
            report.Add("SKIP clipboard mutation: preserve all user clipboard formats; physical paste remains to be tested.");
            Check(!acceptingEmptyInput && !pastingDocument, "intake guards released");
            if (Environment.GetCommandLineArgs().Contains("--interactive-merge-empty", StringComparer.Ordinal))
            {
                report.Add("INTERACTIVE candidate ready; no clipboard mutation. Close by writing interactive.done in evidence directory.");
                File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
                for (var attempt = 0; attempt < 240 && !File.Exists(Path.Combine(output, "interactive.done")); attempt++) await Task.Delay(1000);
            }
        }
        catch (Exception error) { report.Add("ERROR " + error); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = 1; }
        finally { DetachMergeEmptyDrop(); }
    }
}
#endif
