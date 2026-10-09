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
    internal async Task AuditReaderUiAsync()
    {
        ShowPdfReader(); await pdfReader!.AuditAsync();
    }
    private sealed partial class PdfReaderView
    {
        internal async Task AuditEmbeddingAsync(Action<bool, string> check, Func<Task>? captureBoundary = null)
        {
            async Task Scroll(double offset)
            {
                viewport.ChangeView(null, offset, null, true);
                for (var i = 0; i < 30; i++)
                {
                    View.UpdateLayout(); await Task.Delay(100);
                    if (Math.Abs(viewport.VerticalOffset - offset) < 2) break;
                    viewport.ChangeView(null, offset, null, true);
                }
                RefreshScroll(force: true); await Rendering;
            }
            check(previewOnly && Visible && document is not null, $"embedded shared reader loaded (visible={Visible}, loaded={View.IsLoaded}, pages={document?.PageCount}, notice={notice.Text})");
            check(viewport.ZoomMode == ZoomMode.Enabled && !View.AllowDrop, "read preview native pinch wiring and no document replacement drop");
            await GoAsync(0);
            var zoom = viewport.ZoomFactor;
            await Scroll((pageSlots[1].Top + 20) * zoom);
            check(pageIndex == 1 && pageInput.Text == "2", "embedded forward boundary updates active page number");
            check(pageSlots[0].Image.Source is not null && pageSlots[1].Image.Source is not null, "embedded neighboring page rasters rendered together");
            await Scroll((pageSlots[1].Top - 100) * zoom);
            check(pageIndex == 0 && pageInput.Text == "1", "embedded reverse boundary updates page number");
            if (captureBoundary is not null) await captureBoundary();
            check(Math.Abs(viewport.ZoomFactor - zoom) < .005, "embedded scrolling retains zoom");
            await GoAsync(2);
            check(pageIndex == 2 && pageInput.Text == "3" && picture.Source is not null, "embedded page jump uses same scroll surface");
            check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000, "embedded retained raster request budget bounded");
            check(thumbnails.Children.OfType<Button>().Any(b => b.BorderThickness.Left == 2), "embedded active thumbnail follows viewport");
            var pending = GoAsync(0); Deactivate(); await pending;
            check(pageSlots.All(p => p.Image.Source is null), "embedded cancel rejects stale render and clears rasters");
            Resume(); await Rendering;
            check(picture.Source is not null, "embedded reader resumes after cancellation");
        }

        private static void WriteContinuousScrollFixture(string target, int count)
        {
            using var stream = new FileStream(target, FileMode.CreateNew, FileAccess.Write);
            void Write(string value) => stream.Write(System.Text.Encoding.ASCII.GetBytes(value));
            var offsets = new List<long> { 0 };
            void Object(string value) { offsets.Add(stream.Position); Write($"{offsets.Count - 1} 0 obj\n{value}\nendobj\n"); }
            Write("%PDF-1.4\n");
            Object("<< /Type /Catalog /Pages 2 0 R >>");
            Object($"<< /Type /Pages /Count {count} /Kids [{string.Join(" ", Enumerable.Range(0, count).Select(i => $"{i + 5} 0 R"))}] >>");
            Object("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            var text = "BT /F1 18 Tf 40 780 Td (Synthetic continuous scroll fixture) Tj ET\n";
            Object($"<< /Length {text.Length} >>\nstream\n{text}endstream");
            for (var i = 0; i < count; i++) Object("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents 4 0 R >>");
            var xref = stream.Position;
            Write($"xref\n0 {offsets.Count}\n0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1)) Write(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            Write($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        }

        internal async Task AuditAsync()
        {
            var output = Path.Combine(AppContext.BaseDirectory, "reader-ui-audit-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(output);
            var report = new List<string>();
            void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); report.Add("PASS " + label); }
            async Task Settle()
            {
                owner.root.UpdateLayout(); await Task.Delay(250); owner.root.UpdateLayout();
                for (var i = 0; i < 3; i++) { var pending = Rendering; await pending; if (pending == Rendering) break; }
            }
            async Task Capture(string name)
            {
                await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(owner.root); var bytes = await bitmap.GetPixelsAsync();
                var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.FailIfExists);
                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
            }
            try
            {
                if (Environment.GetCommandLineArgs().Contains("--landing-layout-only"))
                {
                    // Exercise the actual Loaded thumbnail path, not just empty
                    // session buttons. Native reentrancy requires a real recent PDF.
                    var recentFixture = Path.Combine(output, "Recent synthetic PDF for storage broker regression.pdf");
                    WriteHistoryAuditFixture(recentFixture);
                    var recentHash = SHA256.HashData(await File.ReadAllBytesAsync(recentFixture));
                    owner.savedJobs.Clear();
                    owner.savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now, "reader", [recentFixture], "Synthetic crash regression"));
                    var callsBefore = Volatile.Read(ref previewStorageBackgroundCalls);
                    if (owner.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
                    owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1040));
                    foreach (var width in new[] { 1920, 640, 480, 900, 640 })
                    foreach (var sessions in new[] { 0, 2, 20 })
                    foreach (var status in new[] { PdfAssociation.DefaultStatus.Other, PdfAssociation.DefaultStatus.Oto, PdfAssociation.DefaultStatus.Unknown })
                    {
                        var label = $"landing {width} sessions={sessions} default={status}";
                        File.WriteAllText(Path.Combine(output, "stage.txt"), label);
                        owner.root.Width = width; owner.root.Height = 650;
                        owner.homeFiles.Clear(); owner.chosenFiles.Clear();
                        for (var i = 0; i < sessions; i++) owner.homeFiles.Add(Path.Combine(output, $"Dokumen sesi sintetis dengan nama panjang {i}.pdf"));
                        landingDefaultTestStatus = status;
                        owner.ShowHistory(); await Settle();
                        owner.ShowPdfReader(false); RefreshLandingDefault(); await Settle();
                        Check(Visible && document is null && landing!.Visibility == Visibility.Visible, label + " visible after history");
                        Check(readerChrome.All(e => e.Visibility == Visibility.Collapsed), label + " document controls remain hidden");
                        Check(landingSessions.Children.OfType<Button>().Count() == sessions, label + " session actions retained");
                        var bounds = landingOpen!.TransformToVisual(landing).TransformBounds(new Windows.Foundation.Rect(0, 0, landingOpen.ActualWidth, landingOpen.ActualHeight));
                        Check(bounds.Left >= 0 && bounds.Right <= landing!.ActualWidth + 1, label + " open action inside viewport");
                        for (var attempt = 0; attempt < 60 && !Descendants(recentLanding).OfType<Image>().Any(i => i.Source is not null); attempt++)
                            await Task.Delay(100);
                        Check(Descendants(recentLanding).OfType<Image>().Any(i => i.Source is not null), label + " real recent thumbnail rendered after Loaded");
                        Check(Volatile.Read(ref previewStorageUnsafeCalls) == 0 && Volatile.Read(ref previewStorageBackgroundCalls) > callsBefore,
                            label + " storage broker invoked on background MTA, never XAML STA");
                        if (sessions == 2 && status == PdfAssociation.DefaultStatus.Other) await Capture($"narrow-landing-{width}-{report.Count}");
                        File.WriteAllLines(Path.Combine(output, "audit-current.txt"), report);
                    }
                    var recentAfterHash = SHA256.HashData(await File.ReadAllBytesAsync(recentFixture));
                    Check(recentHash.SequenceEqual(recentAfterHash), "recent thumbnail source bytes preserved");
                    var unavailable = false;
                    try { await OpenPreviewStorageAsync(Path.Combine(output, "missing-storage-fixture.pdf")); }
                    catch (Exception) { unavailable = true; }
                    Check(unavailable, "missing preview file propagates to existing safe fallback");
                    report.Add("NOT_TESTED physical DPI, multi-monitor, touch gestures and screen-reader UAT.");
                    return;
                }
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000)); await Settle();
                Check(Visible && document is null && !edit.IsEnabled && !more.IsEnabled, "empty reader has no document actions");
                Check(count.Text.Length == 0 && pageInput.Text.Length == 0 && zoomControls.Percent.Text == "—", "empty reader and shared preview show no fictitious page zero or zoom");
                await Capture("01-empty");
                landingDefaultTestStatus = PdfAssociation.DefaultStatus.Other; RefreshLandingDefault();
                Check(landingDefaultButton!.Visibility == Visibility.Visible && defaultHeading.Text == "Jadikan OTO pembuka PDF utama", "not-default offer remains explicit Windows choice");
                await Capture("landing-default-offer");
                RefreshLandingDefault();
                Check(landingDefaultButton.Visibility == Visibility.Visible, "return without default change retains offer without success claim");
                landingDefaultTestStatus = PdfAssociation.DefaultStatus.Unknown; RefreshLandingDefault();
                Check(landingDefaultStatus.Text.Contains("belum dapat diverifikasi", StringComparison.Ordinal), "unknown default displayed honestly");
                landingDefaultTestStatus = PdfAssociation.DefaultStatus.Oto; RefreshLandingDefault();
                Check(landingDefaultButton.Visibility == Visibility.Collapsed && defaultHeading.Text == "OTO adalah pembuka PDF default Anda.", "verified default has compact status and no nag");
                landingDefaultTestStatus = null; RefreshLandingDefault();
                Check(landing?.Visibility == Visibility.Visible && readerChrome.All(e => e.Visibility == Visibility.Collapsed), "landing hides reader rail toolbar zoom and page zero");
                Check(PdfAssociation.ClassifyDefault(PdfAssociation.ProgId) == PdfAssociation.DefaultStatus.Oto && PdfAssociation.ClassifyDefault(@"Applications\" + PdfAssociation.HelperName) == PdfAssociation.DefaultStatus.Oto, "effective OTO ProgIDs verified");
                Check(PdfAssociation.ClassifyDefault("Other.Pdf") == PdfAssociation.DefaultStatus.Other && PdfAssociation.ClassifyDefault(null) == PdfAssociation.DefaultStatus.Unknown, "other and unknown default status distinct");
                foreach (var size in new[] { new Windows.Graphics.SizeInt32(1366, 768), new Windows.Graphics.SizeInt32(1920, 1080), new Windows.Graphics.SizeInt32(900, 640) })
                {
                    landingDefaultTestStatus = PdfAssociation.DefaultStatus.Other; RefreshLandingDefault();
                    owner.AppWindow.Resize(size); await Settle();
                    Check(landingOpen!.ActualWidth >= 170 && landingOpen.ActualHeight >= 46, "landing CTA target " + size.Width);
                    var defaultBounds = landingDefaultButton!.TransformToVisual(landing).TransformBounds(new Windows.Foundation.Rect(0, 0, landingDefaultButton.ActualWidth, landingDefaultButton.ActualHeight));
                    Check(defaultBounds.X >= 0 && defaultBounds.Right <= landing!.ActualWidth, "default CTA within viewport " + size.Width);
                    await Capture("landing-" + size.Width);
                }
                landingDefaultTestStatus = null; RefreshLandingDefault();
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000)); await Settle();
                var fixture = Path.Combine(output, "reader-synthetic.pdf"); WriteHistoryAuditFixture(fixture);
                var hash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
                await OpenAsync(fixture); await Rendering; await Settle();
                Check(document is not null && picture.Source is not null, "local PDF rendered inside native reader");
                Check(thumbnails.Children.OfType<Button>().Count() == document!.PageCount, "real page thumbnails created for small document");
                Check(picture.ActualWidth * viewport.ZoomFactor <= viewport.ActualWidth + 2 && picture.ActualHeight * viewport.ZoomFactor <= viewport.ActualHeight + 2, "initial document fits canvas horizontally and vertically");
                Check(edit.IsEnabled && organize.IsEnabled && more.IsEnabled, "document actions enabled after valid open");
                Check(viewport.ZoomMode == ZoomMode.Enabled, "native pinch zoom enabled");
                Check(ZoomController.TryPercent("125%", .1, 5, out var parsed) && parsed == 1.25, "percent input accepts valid finite preset");
                foreach (var invalid in new[] { "NaN", "Infinity", "0", "-10", "501%", "oops" })
                    Check(!ZoomController.TryPercent(invalid, .1, 5, out _), "percent rejects " + invalid);
                zoomControls.Command(0); await Settle(); zoomControls.Command(1); await Settle(); zoomControls.Command(-1); await Settle();
                Check(Math.Abs(viewport.ZoomFactor - 1) < .0001, "zoom plus minus round trip is symmetric");
                Check(zoomControls.Percent.Text == "100%" && zoomControls.Toolbar.Children.OfType<ComboBox>().Count() == 1 && !zoomControls.Toolbar.Children.OfType<TextBlock>().Any(t => t.Text.Contains('%')), "single active percentage control");
                fit = true; Fit(); await Rendering; await Settle();
                Check(Descendants(View).OfType<Button>().All(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b))), "all reader buttons have accessible names");
                await Capture("02-reader-desktop");
                Check(viewport.ScrollableHeight > picture.Height * viewport.ZoomFactor, "scroll extent covers the complete document rather than one page");
                async Task ScrollTo(double offset)
                {
                    viewport.ChangeView(null, offset, null, true);
                    // ChangeView is asynchronous; await the requested native offset rather than assuming 250 ms.
                    for (var attempt = 0; attempt < 30; attempt++)
                    {
                        await Settle();
                        if (Math.Abs(viewport.VerticalOffset - Math.Clamp(offset, 0, viewport.ScrollableHeight)) < .01 && !settingZoom) break;
                        viewport.ChangeView(null, Math.Clamp(offset, 0, viewport.ScrollableHeight), null, true);
                    }
                    await Rendering; await Settle();
                }
                await ScrollTo((pageSlots[1].Top - 160) * viewport.ZoomFactor);
                Check(pageSlots[0].Image.Source is not null && pageSlots[1].Image.Source is not null, "both adjacent pages remain rendered across the scroll boundary");
                await Capture("09-continuous-page-boundary");
                await ScrollTo((pageSlots[1].Top + 40) * viewport.ZoomFactor);
                Check(pageIndex == 1 && pageInput.Text == "2" && previous.IsEnabled && next.IsEnabled, "scrolling downward advances page number and navigation controls");
                await ScrollTo(0);
                Check(pageIndex == 0 && !previous.IsEnabled, "scrolling upward returns to the first page without a page-button click");
                Zoom(2); await Settle(); var scrollZoom = viewport.ZoomFactor;
                await ScrollTo(120);
                Check(pageIndex == 0 && viewport.VerticalOffset > 100, "zoomed page scrolls inside the page without premature navigation");
                await ScrollTo((pageSlots[1].Top + 40) * scrollZoom);
                Check(pageIndex == 1 && Math.Abs(viewport.ZoomFactor - scrollZoom) < .001, $"cross-page scrolling preserves the selected zoom (page={pageIndex}, zoom={viewport.ZoomFactor}, expected={scrollZoom}, setting={settingZoom}, offset={viewport.VerticalOffset})");
                await ScrollTo(double.MaxValue);
                Check(pageIndex == 2 && !next.IsEnabled && viewport.VerticalOffset <= viewport.ScrollableHeight + 1, "document end clamps safely on the last page");
                Check(pageSlots[0].Image.Source is null && pageSlots[2].Image.Source is not null, "distant page bitmap is released while the visible page remains rendered");
                Check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000, "retained page raster requests stay within the pixel budget");
                await ScrollTo(-100);
                Check(pageIndex == 0 && viewport.VerticalOffset == 0 && picture.Source is not null, $"document start clamps and revisiting a released page renders it again (page={pageIndex}, offset={viewport.VerticalOffset}, source={picture.Source is not null}, setting={settingZoom})");
                viewport.ChangeView(null, pageSlots[1].Top * scrollZoom, null, true);
                await ScrollTo(pageSlots[2].Top * scrollZoom);
                Check(pageIndex == 2 && picture == pageSlots[2].Image && picture.Source is not null, "rapid scroll settles on the latest visible page");
                fit = true; await GoAsync(0); await Settle(); await Rendering;
                var before = viewport.ZoomFactor; Zoom(1.15f); await Settle(); Check(viewport.ZoomFactor > before, "zoom changes rendered document scale");
                fit = true; await GoAsync(1); await Settle(); await Rendering; Check(pageIndex == 1 && picture.Source is not null, "next page renders correctly");
                Check(fit, "fit mode survives page navigation and native view events");
                pageInput.Text = "9999"; await ReadPageInputAsync(); Check(pageIndex == 1 && notice.Text.Contains("jangkauan", StringComparison.Ordinal), "invalid page does not change selection");
                marks.Add(1); bookmarksOnly = true; await DrawThumbnailsAsync(generation); Check(thumbnails.Children.OfType<Button>().Count() == 1, "session bookmark filters thumbnails");
                bookmarksOnly = false;
                await OpenAsync(Path.Combine(output, "missing.pdf")); Check(path == fixture && picture.Source is not null, "missing file preserves previous document");
                await OpenAsync(Path.ChangeExtension(fixture, ".txt")); Check(path == fixture && notice.Text.Contains("lokal", StringComparison.Ordinal), "non-PDF rejected without losing previous document");
                await AcceptAsync([]); Check(notice.Text.Contains("satu", StringComparison.Ordinal), "empty or multiple input rejected clearly");
                await GoAsync(0); await Settle();
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 720)); await Settle(); await Rendering; await Capture("03-reader-compact");
                Check(workspace.ActualHeight > 200, "compact reader retains document viewport");
                Check(fit && picture.ActualHeight * viewport.ZoomFactor <= viewport.ActualHeight + 2, $"fit mode adapts continuous pages to the compact viewport (fit={fit}, zoom={viewport.ZoomFactor}, fitted={fittedZoom}, setting={settingZoom}, page={picture.ActualHeight}, viewport={viewport.ActualHeight})");
                owner.ShowHome(); await Settle(); Check(!Visible && picture.Source is null, "navigation cancels rendering and releases displayed bitmap");
                owner.ShowPdfReader(); await Settle(); await Rendering;
                await Capture("return-diagnostic");
                Check(Visible && picture.Source is not null, $"return restores reader document (visible={Visible}, loaded={View.IsLoaded}, pdf={document is not null}, notice={notice.Text})");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000)); await Settle();
                await UseToolAsync("edit"); Check(owner.liveEditor?.HasDocument == true && owner.liveEditor.Visible, "edit action opens actual existing editor");
                owner.ShowPdfReader(); await Settle(); await Rendering;
                var afterHash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
                Check(hash.SequenceEqual(afterHash), "reading and editor handoff preserve original bytes");
                var target = await StorageFolder.GetFolderFromPathAsync(output);
                var copy1 = await CopyAsync(fixture, target); var copy2 = await CopyAsync(fixture, target);
                Check(copy1.Path != copy2.Path && copy1.Path != fixture, "save copies generate unique names without overwrite");
                var copyHash = SHA256.HashData(await File.ReadAllBytesAsync(copy1.Path));
                Check(hash.SequenceEqual(copyHash), "saved copy retains exact source bytes");
                await Capture("04-reader-restored");
                searchInput.Text = "pompa"; await SearchAsync(); await Settle();
                Check(searchHits.Length == 3 && searchResults.Items.Count == 3 && pageIndex == 0, $"search finds real text on all three pages and opens first result (hits={searchHits.Length}, items={searchResults.Items.Count}, page={pageIndex}, status={searchStatus.Text})");
                Check(searchStatus.Text.Contains("3/3", StringComparison.Ordinal) && searchCancel.Visibility == Visibility.Collapsed, "search reports page coverage and completes progress state");
                SelectSearchResult(1); await searchNavigation; Check(pageIndex == 1, "next search result opens correct page");
                SelectSearchResult(-1); await searchNavigation; Check(pageIndex == 0, "previous search result opens correct page");
                SelectSearchResult(-1); await searchNavigation; Check(pageIndex == 2, "search result navigation wraps safely");
                await Capture("05-reader-search");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 720)); await Settle(); await Capture("06-reader-search-compact");
                Check(workspace.ActualHeight > 150 && searchInput.ActualWidth >= 100, "compact search retains readable controls and document area");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000)); await Settle();
                searchInput.Text = "tidakada123"; await SearchAsync();
                Check(searchHits.Length == 0 && searchStatus.Text.Contains("teks yang dapat dibaca", StringComparison.Ordinal), "no-match message describes extraction coverage");
                searchInput.Text = "pompa"; var pendingSearch = SearchAsync(); CancelSearch(); await pendingSearch;
                Check(searchHits.Length == 0 && searchStatus.Text.Contains("dibatalkan", StringComparison.Ordinal), "cancelled worker cannot publish late search results");
                var pendingEdit = SearchAsync(); searchInput.Text = "baru"; await pendingEdit;
                Check(searchHits.Length == 0 && searchPanel.Visibility == Visibility.Collapsed, "query changes invalidate in-flight results");
                searchInput.Text = "pompa"; var pendingNavigation = SearchAsync(); owner.ShowHome(); await pendingNavigation; await Settle();
                Check(searchHits.Length == 0 && !Visible, "navigation cancels search without reopening reader");
                owner.ShowPdfReader(); await Settle(); await Rendering;
                var pendingReplacement = SearchAsync(); await OpenAsync(copy1.Path); await pendingReplacement;
                Check(path == copy1.Path && searchHits.Length == 0, "replacing document invalidates previous search results");
                await OpenAsync(fixture); searchInput.Text = "pompa"; await SearchAsync();
                await OpenAsync(Path.Combine(output, "still-missing.pdf"));
                Check(path == fixture && searchHits.Length == 3, "failed document open retains valid search results");
                var searchedHash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
                Check(hash.SequenceEqual(searchedHash), "search preserves source bytes");
                ClearSearch(); Check(searchHits.Length == 0 && searchPanel.Visibility == Visibility.Collapsed, "closing search clears in-memory snippets");
                await AuditSearchPositionsAsync(output, Check, Settle, Capture);
                owner.ShowSettings(); await Settle(); await Capture("07-settings-default-pdf");
                Check(Descendants(owner.root).OfType<Button>().Any(b => AutomationProperties.GetName(b) == "Jadikan PERTAMINA OTO pembuka PDF default"), "settings exposes accessible default PDF action");
                Check(owner.pdfDefaultStatus?.IsLoaded == true && owner.pdfDefaultStatus.Text.Length > 0, "default PDF status displayed without changing Windows associations");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 720)); await Capture("08-settings-default-pdf-compact");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000));
                var explorerFixture = Path.Combine(output, "Berkas lokal é & (1).pdf"); File.Copy(fixture, explorerFixture);
                await owner.OpenPdfReaderAsync(explorerFixture); await Rendering; await Settle();
                Check(Visible && path == explorerFixture && picture.Source is not null, "Explorer activation target opens spaced Unicode path directly in reader");
                foreach (var (width, height, label) in new[] { (842, 1191, "A3"), (1191, 1684, "A2"), (1684, 1191, "A2 landscape") })
                {
                    var large = Path.Combine(output, $"page-{width}-{height}.pdf"); WriteHistoryAuditFixture(large, width, height);
                    await OpenAsync(large); await Rendering; await Settle();
                    Check(path == large && picture.Source is not null && Math.Abs(picture.Width / picture.Height - (double)width / height) < .001, label + " renders at correct aspect ratio");
                    Check(picture.ActualWidth * viewport.ZoomFactor <= viewport.ActualWidth + 2 && picture.ActualHeight * viewport.ZoomFactor <= viewport.ActualHeight + 2, label + " fits viewport");
                }
                var mixed = Path.Combine(output, "mixed-sizes.pdf");
                var merged = await WorkerClient.RunAsync(new Pertamina.Oto.LocalPdf.PdfRequest("merge", [fixture, Path.Combine(output, "page-1191-1684.pdf"), Path.Combine(output, "page-1684-1191.pdf")], mixed), CancellationToken.None);
                Check(merged.Success, "mixed-size nine-page synthetic fixture produced by existing worker");
                var retiredPages = pageSlots;
                await OpenAsync(mixed); await Rendering; await Settle();
                Check(pageSlots.Length == 9 && retiredPages.All(p => p.Image.Source is null), "document replacement releases every prior page bitmap");
                Zoom(1.4f); await Settle();
                await ScrollTo(pageSlots[3].Top * viewport.ZoomFactor + 40);
                Check(pageIndex == 3 && Math.Abs(picture.Width / picture.Height - 1191d / 1684) < .001, "scroll follows actual offsets across mixed paper sizes");
                await GoAsync(8); await Settle(); await Rendering;
                Check(pageIndex == 8 && picture.Source is not null && Math.Abs(picture.Width / picture.Height - 1684d / 1191) < .001 && !next.IsEnabled, "jump reaches the final shorter landscape page in a mixed document");
                Check(pageSlots.Take(6).All(p => p.Image.Source is null), "longer document does not retain distant page images");
                await Capture("10-continuous-mixed-sizes");
                await ScrollTo(0);
                Check(pageIndex == 0 && picture.Source is not null, "scroll returns from the mixed-size document end to the first page");
                var thousand = Path.Combine(output, "thousand-pages.pdf");
                WriteContinuousScrollFixture(thousand, 1000);
                var thousandHash = SHA256.HashData(await File.ReadAllBytesAsync(thousand));
                await OpenAsync(thousand); await Rendering; await Settle();
                Check(pageSlots.Length == 1000 && pageSlots.Count(p => p.Image.Source is not null) <= 4, "1000-page document renders only the viewport and neighboring pages");
                await GoAsync(999); await Rendering; await Settle();
                Check(pageIndex == 999 && picture.Source is not null && pageSlots.Take(997).All(p => p.Image.Source is null), "1000-page end navigation evicts distant bitmaps");
                Check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000, "1000-page retained rasters obey the pixel budget");
                await Capture("11-thousand-page-end");
                var staleRender = GoAsync(500);
                owner.ShowHome(); await staleRender; await Settle();
                Check(pageSlots.All(p => p.Image.Source is null), "navigation during render prevents stale page images from returning");
                owner.ShowPdfReader(); await Settle(); await Rendering;
                await OpenAsync(fixture); await Rendering; await Settle();
                Check(pageSlots.Length == 3 && picture.Source is not null, "small document replaces the 1000-page surface safely");
                var thousandAfterHash = SHA256.HashData(await File.ReadAllBytesAsync(thousand));
                Check(thousandHash.SequenceEqual(thousandAfterHash), "1000-page scrolling preserves source bytes");
                owner.sidebarCollapsed = false; owner.ShowPdfReader(); await Settle();
                Check(!owner.sidebarCollapsed, "reader preserves expanded sidebar preference");
                owner.readerFocus = true; owner.LayoutCards();
                Check(!owner.sidebarCollapsed && owner.root.ColumnDefinitions[0].Width.Value == 72, "focus compacts layout without changing preference");
                owner.readerFocus = false; owner.LayoutCards();
                Check(owner.root.ColumnDefinitions[0].Width.Value == 232, "leaving focus restores expanded sidebar");
                fit = false; viewport.ChangeView(null, 120, 1.2f, true); await Settle();
                var beforeScroll = viewport.VerticalOffset; var beforeZoom = viewport.ZoomFactor; var beforePage = pageIndex;
                owner.ShowHome(); await Settle(); owner.ShowPdfReader(); await Settle(); await Rendering;
                Check(path == fixture && Math.Abs(viewport.VerticalOffset - beforeScroll) < 2 && Math.Abs(viewport.ZoomFactor - beforeZoom) < .005 && pageIndex == beforePage, "navigation restores document page scroll and zoom");
                owner.homeFiles.Clear(); owner.chosenFiles.Clear(); owner.homeFiles.Add(fixture); owner.homeFiles.Add(copy1.Path); owner.chosenFiles.Add(copy1.Path);
                RefreshSession(); await Settle();
                Check(path == fixture && sessionPicker.Items.Count == 2, "manual document wins over changed session selection");
                Release(); RefreshSession(); await Settle(); await Rendering;
                Check(path == copy1.Path, "one selected session PDF opens automatically");
                Release(); owner.chosenFiles.Clear(); RefreshSession(); await Settle();
                Check(document is null && sessionPicker.Items.Count == 2, "ambiguous session stays empty with explicit document choice");
                sessionPicker.SelectedIndex = 0; await Settle(); await Rendering;
                Check(path == fixture, "session picker opens the exact chosen document");
                SetIdentity(fixture, true);
                Check(metadata.Text.Contains("Hasil pengolahan"), "processed result identity displayed");
                await Capture("12-session-flow");
                Release(); UpdateControls(); RefreshLandingRecent(); await Settle();
                Check(landing!.Visibility == Visibility.Visible && recentLanding.Children.Count <= 3, "last document release returns to landing with bounded real history");
                Check(owner.savedJobs.Any(j => j.Tool == "reader" && j.Paths.Contains(fixture)), "successful reader open persists real metadata");
                await Capture("landing-recent");
                var missing = Path.Combine(output, "missing-local.pdf");
                owner.savedJobs.Insert(0, new(Guid.NewGuid(), DateTimeOffset.Now, "reader", [missing], "")); RefreshLandingRecent(); await Settle();
                Check(Descendants(recentLanding).OfType<TextBlock>().Any(t => t.Text.Contains("File tidak ditemukan", StringComparison.Ordinal)), "missing recent PDF labelled without deleting history");
                await Capture("landing-missing");
                report.Add("LIMIT manual UAT: physical mouse wheel/touchpad/pinch, picker, clipboard/drop, save dialog, recycle confirmation and fullscreen; continuous scrolling verified through native ScrollViewer offsets");
                report.Add("LIMIT search excludes OCR, protected PDF, forms/annotations and unsupported font encodings; unsupported metrics/skew uses explicit page-only fallback; printing not implemented; physical shortcuts, multi-monitor/DPI and screen-reader UAT remain manual");
            }
            catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
            finally { Release(); await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); }
        }
    }
}
#endif
