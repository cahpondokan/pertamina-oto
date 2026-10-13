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
    private static readonly int[] OrganizerTransferAuditWidths = [1400, 660];
    internal async Task AuditOrganizerTransferAsync(string output)
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
        async Task<ContentDialog> Popup()
        {
            for (var i = 0; i < 100; i++)
            {
                var dialog = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
                    .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().FirstOrDefault();
                if (dialog is not null) { await Settle(); return dialog; }
                await Task.Delay(25);
            }
            throw new InvalidOperationException("Transfer popup did not open");
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task Choose(string name)
        {
            var dialog = await Popup();
            Invoke(Descendants(dialog).OfType<Button>().Single(b => b.Name == name));
        }
        var a = Path.Combine(output, "synthetic-first.pdf");
        var b = Path.Combine(output, "synthetic-second.pdf");
        var saved = Path.Combine(output, "existing-saved.pdf");
        WriteHistoryAuditFixture(a, 595, 842); WriteHistoryAuditFixture(b, 420, 595); WriteHistoryAuditFixture(saved, 595, 842);
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        var originalHashes = new[] { Hash(a), Hash(b), Hash(saved) };
        openingEmptyInput = true;
        try { ShowTool(ToolCatalog.All.Single(t => t.Id == "merge")); } finally { openingEmptyInput = false; }
        folder = output;
        await AcceptMergeFilesAsync([await StorageFile.GetFileFromPathAsync(a), await StorageFile.GetFileFromPathAsync(b)]);
        outputName.Text = "retained-merge.pdf"; mergeAngles[a] = 90;
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
        foreach (var width in OrganizerTransferAuditWidths)
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 1000)); await Settle();
            var view = BuildOrganizerTransferConfirmation(); var showing = view.Dialog.ShowAsync();
            try
            {
                await Popup();
                Check(view.Dialog.PrimaryButtonText.Length == 0 && view.Dialog.CloseButtonText.Length == 0 && view.Dialog.SecondaryButtonText.Length == 0,
                    width + ": only two custom actions, no duplicate footer");
                Check(view.Return.FocusState != FocusState.Unfocused && view.Result == ContentDialogResult.None,
                    width + ": safe focus and default intent");
                Check(Descendants(view.Dialog).OfType<Button>().Count(x => x.Name is "OrganizerTransferReplace" or "OrganizerTransferReturn") == 2,
                    width + ": exactly two native actions");
                foreach (var button in new[] { view.Replace, view.Return })
                {
                    Check(button.ActualWidth >= 44 && button.ActualHeight >= 44 && button.ActualWidth <= root.ActualWidth,
                        width + ": " + button.Name + " reachable hit target");
                    Check(AutomationProperties.GetName(button).Length > 0 && button.UseSystemFocusVisuals && button.IsTabStop,
                        width + ": " + button.Name + " keyboard accessibility");
                }
                var biblo = Descendants(view.Dialog).OfType<Image>().Single(i => i.Name == "OrganizerTransferBiblo");
                Check(((BitmapImage)biblo.Source).PixelWidth > 0 && biblo.ActualWidth > 0, width + ": existing Biblo 031 decoded");
                var background = Descendants(view.Dialog).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
                Check(background.ActualWidth <= root.ActualWidth && background.ActualHeight <= root.ActualHeight,
                    width + ": modal fits viewport");
                var rendered = new RenderTargetBitmap(); await rendered.RenderAsync(background);
                var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync("transfer-" + width + ".png", CreationCollisionOption.ReplaceExisting);
                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rendered.PixelWidth,
                    (uint)rendered.PixelHeight, 96, 96, (await rendered.GetPixelsAsync()).ToArray()); await encoder.FlushAsync();
                Invoke(view.Return); await showing;
                Check(view.Result == ContentDialogResult.None && files.SequenceEqual(new[] { a, b }), width + ": return only returns intent, keeps inputs");
            }
            finally { view.Dialog.Hide(); await showing; }
        }
        var oldPage = OrganizerPage.Create(0, 0, 180);
        pageLayout.Clear(); pageLayout.Add(oldPage); organizerSources.Clear(); organizerSources.Add(saved);
        layoutUndo.Push([oldPage]); layoutRedo.Push([oldPage]);
        var undo = layoutUndo.Count; var redo = layoutRedo.Count;
        var cancel = OpenMergeOrganizerAsync(); await Choose("OrganizerTransferReturn"); await cancel;
        Check(pageLayout.SequenceEqual(new[] { oldPage }) && organizerSources.SequenceEqual(new[] { saved }) && layoutUndo.Count == undo && layoutRedo.Count == redo,
            "cancel preserves existing arrangement, sources and undo/redo");
        Check(files.SequenceEqual(new[] { a, b }) && outputName.Text == "retained-merge.pdf" && !messageOpen && !importing,
            "cancel retains Merge name/inputs and releases guards");
        importing = true; await OpenMergeOrganizerAsync(); importing = false;
        Check(pageLayout.SequenceEqual(new[] { oldPage }) && !messageOpen, "busy guard does not replace arrangement");
        messageOpen = true; await OpenMergeOrganizerAsync(); messageOpen = false;
        Check(pageLayout.SequenceEqual(new[] { oldPage }), "reentrant modal guard does not replace arrangement");
        files.Clear(); files.Add(a); files.Add(Path.Combine(output, "missing.pdf"));
        var missing = OpenMergeOrganizerAsync(); await Choose("OrganizerTransferReplace"); await missing;
        Check(pageLayout.SequenceEqual(new[] { oldPage }) && organizerSources.SequenceEqual(new[] { saved }) && layoutUndo.Count == undo && layoutRedo.Count == redo,
            "failed input loading retains old arrangement and history");
        Check(!messageOpen && !importing, "failed input loading releases modal/import guards");
        files.Clear(); files.Add(a); files.Add(b);
        var replace = OpenMergeOrganizerAsync(); await Choose("OrganizerTransferReplace"); await replace; await Settle();
        Check(pageLayout.Count == 6 && pageLayout.Take(3).Select(p => p.Page).SequenceEqual(Enumerable.Range(0, 3))
            && pageLayout.Take(3).All(p => p.Source == 0 && p.Rotation == 90)
            && pageLayout.Skip(3).Select(p => p.Page).SequenceEqual(Enumerable.Range(0, 3))
            && pageLayout.Skip(3).All(p => p.Source == 1 && p.Rotation == 0),
            "explicit replacement transfers source order/pages/rotation");
        Check(organizerSources.SequenceEqual(new[] { a, b }) && layoutUndo.Count == 0 && layoutRedo.Count == 0 && !messageOpen && !importing,
            "explicit replacement commits sources and releases guards");
        Check(Hash(a) == originalHashes[0] && Hash(b) == originalHashes[1] && Hash(saved) == originalHashes[2],
            "source PDFs and already saved PDF remain byte-identical");
        report.Add("LIMIT physical key-up/overlap, alternate DPI, screen reader and live high contrast are not asserted by this native fixture.");
        File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
    }
}
#endif
