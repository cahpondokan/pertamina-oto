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
    internal static void PrepareMergeClosedSavedNoticeAudit(string output) => PrepareCenteredAudit(output);

    internal async Task AuditMergeClosedSavedNoticeAsync(string output)
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
            var source = Path.Combine(output, "synthetic-source.pdf");
            var saved = Path.Combine(output, "synthetic-saved.pdf");
            WriteHistoryAuditFixture(source, 595, 842); WriteHistoryAuditFixture(saved, 595, 842);
            string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            var originalHash = Hash(source); var savedHash = Hash(saved);
            foreach (var width in new[] { 1200, 800, 660 })
            {
                if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 900));
                ShowEmptyTool(tool); resultPath = null; ShowMergeClosedSavedNotice(saved); await Settle();
                var banner = body.Children.OfType<Border>().Single(b => b.Tag is MergeClosedNoticeState);
                var state = (MergeClosedNoticeState)banner.Tag;
                Check(resultPath is null && state.SavedPath == saved, $"{width}: immutable saved path survives cleared session");
                Check(body.Children.IndexOf(banner) < body.Children.IndexOf(emptyPanel!), $"{width}: notice above existing empty card");
                Check(state.Open.ActualHeight >= 44 && state.Folder.ActualHeight >= 44, $"{width}: accessible action hit targets");
                Check(AutomationProperties.GetName(state.Open) == "Buka hasil" && AutomationProperties.GetName(state.Folder) == "Buka folder", $"{width}: named actions");
                Check(banner.ActualWidth <= body.ActualWidth + 1 && state.Open.ActualWidth >= 104 && state.Folder.ActualWidth >= 104, $"{width}: responsive action fit");
                Check(standardPageScroll!.ScrollableHeight <= 180, $"{width}: bounded minimum viewport scrolling");
                Check(Hash(source) == originalHash && Hash(saved) == savedHash, $"{width}: source and saved result unchanged");
                await Capture("merge-closed-saved-" + width);
                if (width == 1200)
                {
                    var previewOpened = false;
                    auditReadDialog = _ => { previewOpened = true; return Task.CompletedTask; };
                    try { await state.OpenAsync(); }
                    finally { auditReadDialog = null; }
                    Check(previewOpened && resultPath is null && body.Children.Contains(banner), "saved result: immutable path opens internal preview and retains notice");
                }
                ShowHome(); await Settle();
                Check(!body.Children.Contains(banner), $"{width}: navigation removes view-bound notice");
                await state.OpenAsync(); state.Reveal();
                Check(state.Status.Text == "", $"{width}: detached action closures are inert");
            }
            ShowEmptyTool(tool); ShowMergeClosedSavedNotice(Path.Combine(output, "missing-saved-result.pdf")); await Settle();
            var missingBanner = body.Children.OfType<Border>().Single(b => b.Tag is MergeClosedNoticeState);
            var missing = (MergeClosedNoticeState)missingBanner.Tag;
            await missing.OpenAsync(); missing.Reveal();
            Check(body.Children.Contains(missingBanner) && missing.Status.Visibility == Visibility.Visible, "missing result: graceful message retains banner");
            await Settle(); await Capture("merge-closed-saved-missing");
            ShowEmptyTool(tool); await Settle();
            Check(!body.Children.OfType<Border>().Any(b => b.Tag is MergeClosedNoticeState), "fresh input view has no stale success");
            Check(Hash(source) == originalHash && Hash(saved) == savedHash, "failure actions leave synthetic source and result unchanged");
            report.Add("SKIP physical input, Explorer opening, high contrast toggle and real close-save lifecycle: integrator native verification required.");
            File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
        }
        catch (Exception) { report.Add("FAIL native notice audit interrupted; inspect failing label above."); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = 1; }
        finally { auditReadDialog = null; }
    }
}
#endif
