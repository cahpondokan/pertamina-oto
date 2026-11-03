#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditReaderTabsAsync(string output)
    {
        ShowPdfReader(false);
        await pdfReader!.AuditTabsAsync(Path.GetFullPath(output));
    }

    private sealed partial class PdfReaderView
    {
        internal async Task AuditTabsAsync(string output)
        {
            Directory.CreateDirectory(output);
            var report = new List<string>();
            var main = owner;
            void Log(string line) { report.Add(line); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
            void Check(bool ok, string label) { Log((ok ? "PASS " : "FAIL ") + label); if (!ok) throw new InvalidOperationException(label); }
            async Task Settle(MainWindow w)
            {
                w.root.UpdateLayout(); if (w.pdfReader is { } activeReader) await activeReader.WaitLoadedAsync(); await Task.Delay(200); w.root.UpdateLayout();
                for (var i = 0; i < 5 && w.pdfReader is { } r; i++) { var pending = r.Rendering; await pending; if (pending == r.Rendering) break; }
            }
            async Task Capture(MainWindow w, string name)
            {
                await Settle(w);
                var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync((FrameworkElement)w.Content);
                var bytes = await bitmap.GetPixelsAsync();
                var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray());
                await encoder.FlushAsync();
            }
            try
            {
                var first = Path.Combine(output, "A synthetic.pdf"); var second = Path.Combine(output, "B synthetic.pdf");
                WriteContinuousScrollFixture(first, 6); WriteContinuousScrollFixture(second, 4);
                var hashA = SHA256.HashData(await File.ReadAllBytesAsync(first)); var hashB = SHA256.HashData(await File.ReadAllBytesAsync(second));
                await main.OpenPdfReaderAsync(first); await Settle(main);
                var a = main.pdfReader!;
                Check(a.document?.PageCount == 6 && main.Readers.Count() == 1, "first PDF reuses empty tab");
                Check(ReferenceEquals(Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(main.root.XamlRoot), a.viewport), "opened PDF focuses reading surface for immediate arrow navigation");
                await a.GoAsync(2); a.fit = false; ApplyViewportZoom(a.viewport, 1.3f); await Settle(main);
                a.searchInput.Text = "Synthetic"; await a.SearchAsync(); await Settle(main);
                await a.GoAsync(2); await Settle(main);
                var pageA = a.pageIndex; var zoomA = a.viewport.ZoomFactor; var offsetA = a.viewport.VerticalOffset;
                Check(a.searchHits.Length > 0, "first PDF has independent local search results");
                await main.OpenPdfReaderAsync(second); await Settle(main);
                var b = main.pdfReader!;
                Check(a != b && main.Readers.Count() == 2 && a.ReaderSource == first && b.ReaderSource == second, "second PDF adds independent tab without replacing first");
                Check(a.View.Visibility == Visibility.Collapsed && a.suspended && a.pageSlots[pageA].Image.Source is not null,
                    "inactive recent tab retains its visible page without rendering in the background");
                Check(warmReaders.Count <= 2 && warmReaders.Sum(r => r.RetainedRasterBytes) <= WarmReaderBudget,
                    "inactive raster cache obeys global count and decoded-byte limits");
                Check(main.readerSurface.Children.Contains(a.View) && main.readerSurface.Children.Contains(b.View) &&
                    main.FindReaderTab(a)!.Content is null && main.FindReaderTab(b)!.Content is null,
                    "tab contents stay mounted in one persistent reading surface");
                var retainedPageA = a.pageSlots[pageA].Image.Source;
                Check(b.pageIndex == 0 && b.searchHits.Length == 0, "new tab has its own page and search");
                await b.NavigateReaderKeyAsync(1); await Settle(main);
                Check(b.pageIndex == 1, "Down route advances one page");
                await b.NavigateReaderKeyAsync(-1); await Settle(main);
                Check(b.pageIndex == 0, "Up route returns one page");
                await b.NavigateReaderKeyAsync(-1); Check(b.pageIndex == 0, "Up stops at first page");
                await b.GoAsync(3); await b.NavigateReaderKeyAsync(1); Check(b.pageIndex == 3, "Down stops at last page");
                b.FocusReading(); Check(b.ReaderKeyAllowed(b.viewport), "arrow navigation allowed on reading surface");
                Check(!b.ReaderKeyAllowed(b.pageInput) && !b.ReaderKeyAllowed(b.searchInput) && !b.ReaderKeyAllowed(b.searchResults), "page/search inputs and result list retain native arrows");
                main.messageOpen = true; Check(!b.ReaderKeyAllowed(b.viewport), "modal prevents reader keyboard navigation"); main.messageOpen = false;
                main.CycleReaderTab(-1); await Settle(main);
                Check(main.pdfReader == a && a.pageIndex == pageA && Math.Abs(a.viewport.ZoomFactor - zoomA) < .01 && Math.Abs(a.viewport.VerticalOffset - offsetA) < 3, "tab switch restores page zoom and scroll");
                Check(ReferenceEquals(retainedPageA, a.pageSlots[pageA].Image.Source), "warm tab reuses the same decoded current page");
                Check(a.searchInput.Text == "Synthetic" && a.searchHits.Length > 0, "tab switch preserves query and search results");
                Check(main.integratedWindowHeader is not null && main.readerTabs!.Parent == main.integratedWindowHeader && !main.historyPage.Children.Contains(main.readerTabs), "document tabs belong to persistent window chrome");
                main.ShowHome(); await Settle(main);
                Check(main.readerTabs!.IsLoaded && main.readerTabs.ActualWidth > 0 && !main.ReaderRouteVisible && main.Readers.Count() == 2, "Beranda keeps both document tabs available");
                await Capture(main, "00-home-with-tabs");
                main.ActivateReaderTabFromHeader(a); await Settle(main);
                Check(main.ReaderRouteVisible && main.pdfReader == a && a.pageIndex == pageA && Math.Abs(a.viewport.ZoomFactor - zoomA) < .01 && Math.Abs(a.viewport.VerticalOffset - offsetA) < 3, "same active tab restores reader from Beranda without losing position");
                Check(a.searchInput.Text == "Synthetic" && a.searchHits.Length > 0, "Beranda round trip retains search results");
                main.ShowHome(); await Settle(main); main.ActivateReaderTabFromHeader(b); await Settle(main);
                Check(main.ReaderRouteVisible && main.pdfReader == b && main.Readers.Count() == 2, "different tab restores correct document from Beranda without duplicate");
                main.ActivateReaderTabFromHeader(a); await Settle(main);
                Check(a.integratedToolbar?.Visibility == Visibility.Visible && a.searchInput.PlaceholderText == "Cari isi PDF", "compact document toolbar uses tooltip-only search shortcut");
                Check(!Descendants(a.integratedToolbar!).OfType<TextBlock>().Any(t => t.Text is "Semua alat" or "Alat lainnya" or "Tutup dokumen" or "Buka PDF"), "reader toolbar excludes duplicate hub and tab actions");
                foreach (var width in new[] { 1480, 1024, 800, 600 })
                {
                    main.AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 900)); await Settle(main);
                    foreach (var element in new FrameworkElement[] { a.searchInput, a.pageInput, a.zoomControls.Percent, a.print, a.edit, a.more })
                    {
                        var bounds = element.TransformToVisual(a.View).TransformBounds(new Windows.Foundation.Rect(0,0,element.ActualWidth,element.ActualHeight));
                        Check(element.IsLoaded && bounds.X >= -1 && bounds.Right <= a.View.ActualWidth + 1 && bounds.Y >= 0 && bounds.Bottom <= a.View.ActualHeight,
                            $"header {width}px keeps {element.GetType().Name} reachable");
                    }
                    await Capture(main,$"header-{width}");
                }
                main.AppWindow.Resize(new Windows.Graphics.SizeInt32(1480,960)); await Settle(main);
                await Capture(main, "01-two-tabs");
                await main.OpenPdfReaderAsync(first); await Settle(main);
                Check(main.Readers.Count() == 2 && main.pdfReader == a, "opening identical file activates its existing tab");
                await main.OpenPdfReaderAsync(Path.Combine(output, "missing.pdf")); await Settle(main);
                Check(main.Readers.Count() == 2 && main.pdfReader == a && a.document is not null, "failed open restores active document and removes failed extra tab");
                var beforeWindows = readerWindows.Count;
                main.DetachReaderTab(b);
                var detached = readerWindows.Single(w => w != main); await Settle(detached); await Settle(main);
                Check(readerWindows.Count == beforeWindows + 1 && detached.pdfReader == b && b.owner == detached, "detach transfers same session to another native window");
                Check(main.Readers.Single() == a && detached.Readers.Single() == b && b.pageIndex == 3, "both windows independently retain document and page");
                Check(b.View.XamlRoot != a.View.XamlRoot && b.Visible && a.Visible, "documents render in two distinct live window roots");
                detached.AppWindow.Move(new Windows.Graphics.PointInt32(main.AppWindow.Position.X + 450, main.AppWindow.Position.Y + 30));
                detached.AppWindow.Resize(new Windows.Graphics.SizeInt32(760, 720)); await Settle(detached);
                await Capture(detached, "02-detached-window"); await Capture(main, "03-original-window");
                await b.NavigateReaderKeyAsync(-1); Check(b.pageIndex == 2 && a.pageIndex == pageA, "navigation in detached window leaves original document unchanged");
                Check(main.savedJobs.Any(j => j.Paths.Contains(first)) && main.savedJobs.Any(j => j.Paths.Contains(second)), "history retains files opened across tabs");
                detached.MoveReaderTab(b, main); await Settle(main);
                Check(readerWindows.Count == beforeWindows && b.owner == main && main.Readers.Count() == 2, "join transfers tab back and closes empty detached window");
                Check(b.pageIndex == 2 && b.document is not null && a.document is not null, "joining preserves both sessions");
                main.CloseReaderTab(b); await Settle(main);
                Check(b.disposed && b.sourceLock is null && main.Readers.Single() == a, "closing one tab releases its resources and retains other tab");
                using (new FileStream(second, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                Check(true, "closed PDF file lock released");
                var transient = main.AddReaderTab(); await Settle(main);
                var loading = main.OpenPdfReaderAsync(second); main.CloseReaderTab(transient); await loading; await Settle(main);
                Check(a.document is not null && !a.disposed, "closing while open request starts preserves existing session");
                foreach (var extra in main.Readers.Where(r => r != a).ToArray()) main.CloseReaderTab(extra);
                await Settle(main);
                main.DetachReaderTab(a); var closeWindow = readerWindows.Single(w => w != main); await Settle(closeWindow);
                Log($"STATE before reader close visible={closeWindow.pdfReader?.Visible} loaded={closeWindow.pdfReader?.View.IsLoaded} nonReaderBusy={closeWindow.NonReaderBusy} pending={closeWindow.RestartPendingWork().Count} readerCount={closeWindow.Readers.Count()} attached={closeWindow.historyPage.Children.Contains(closeWindow.readerTabs)}");
                await closeWindow.ConfirmCloseAsync().WaitAsync(TimeSpan.FromSeconds(5)); await Task.Delay(100);
                Check(closeWindow.readerWindowClosed && !closeWindow.messageOpen && readerWindows.Contains(main), "reader window exits directly without closing another window");
                var afterA = SHA256.HashData(await File.ReadAllBytesAsync(first)); var afterB = SHA256.HashData(await File.ReadAllBytesAsync(second));
                Check(hashA.SequenceEqual(afterA) && hashB.SequenceEqual(afterB), "source hashes unchanged by all reading and tab actions");
                Check(main.RestartPendingWork().Count == 0, "reading produces no unsaved tool draft");
                await main.OpenPdfReaderAsync(first); await Settle(main);
                main.toolDrafts["watermark"] = new([first], "", "DRAFT", "hasil-watermark.pdf", 0, 20);
                var closing = main.ConfirmCloseAsync(); await Task.Delay(250);
                Check(!main.readerWindowClosed && main.messageOpen && main.RestartPendingWork().Count == 1, "reader exit retains confirmation for actual hidden tool draft");
                var dialog = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(main.root.XamlRoot)
                    .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().Single();
                dialog.Hide(); await closing.WaitAsync(TimeSpan.FromSeconds(5));
                Check(!main.allowClose && main.toolDrafts["watermark"].Text == "DRAFT", "dismissing close retains hidden tool draft");
                main.toolDrafts.Remove("watermark"); main.pdfReader!.CloseReaderDocument(); await Settle(main);
                await Capture(main, "04-empty-reader");
                report.Add("LIMIT physical multi-monitor DPI/drag, mouse/keyboard hardware, Windows association picker and screen-reader UAT remain manual. No network used by these reader/IPC checks.");
            }
            catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
            finally
            {
                foreach (var w in readerWindows.Where(w => w != main).ToArray()) { w.allowClose = true; w.Close(); }
                await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report);
            }
        }
    }
}
#endif
