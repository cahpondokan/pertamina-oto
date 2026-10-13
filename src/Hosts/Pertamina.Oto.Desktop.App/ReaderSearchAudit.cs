#if DEBUG
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class PdfReaderView
    {
        private static void WriteSearchPositionFixture(string path, bool rotated = false, string? customStream = null, string? customFont = null, bool opaqueState = false)
        {
            var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "",
                customFont ?? "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Italic >>" };
            var pages = new List<int>();
            var streams = customStream is not null ? [customStream] : new[] {
                "BT /F1 18 Tf 60 750 Td (Synthetic search geometry) Tj ET BT /F1 12 Tf 60 600 Td (Limitation PDG Reached) Tj 0 -120 Td (Service Life Limitation) Tj ET",
                "BT /F2 24 Tf 70 650 Td (Limit bold) Tj ET BT /F3 16 Tf 70 400 Td (Limit italic) Tj ET",
                "BT /F1 14 Tf 100 700 Td (Limit large paper) Tj ET" };
            for (var i = 0; i < streams.Length; i++)
            {
                pages.Add(objects.Count + 1);
                var box = i == 2 ? "[0 0 842 1191]" : "[0 0 595 842]";
                var crop = rotated && i == 1 ? "/CropBox [20 30 575 812] /Rotate 90" : "";
                var graphics = opaqueState ? " /ExtGState << /GS5 << /Type /ExtGState /BM /Normal /CA 1 >> /GS8 << /Type /ExtGState /BM /Normal /ca 1 >> >>" : "";
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox {box} {crop} /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R >>{graphics} >> /Contents {objects.Count + 2} 0 R >>");
                objects.Add($"<< /Length {streams[i].Length} >>\nstream\n{streams[i]}\nendstream");
            }
            objects[1] = $"<< /Type /Pages /Count {pages.Count} /Kids [{string.Join(' ', pages.Select(p => p + " 0 R"))}] >>";
            var pdf = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
            for (var i = 0; i < objects.Count; i++) { offsets.Add(pdf.Length); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
            var xref = pdf.Length; pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            File.WriteAllBytes(path, Encoding.ASCII.GetBytes(pdf.ToString()));
        }

        private async Task AuditSearchPositionsAsync(string output, Action<bool, string> check, Func<Task> settle, Func<string, Task> capture)
        {
            await AuditOpaqueSearchAsync(output, check, settle, capture);
            var single = Path.Combine(output, "search-two-limit-one-page.pdf");
            WriteSearchPositionFixture(single, customStream: "BT /F1 12 Tf 60 600 Td (Limitation PDG Reached) Tj 0 -120 Td (Service Life Limitation) Tj ET");
            await OpenAsync(single); RefreshSession(false); await settle(); fit = false;
            viewport.ChangeView(null, 0, 1.39f, true); await settle(); searchInput.Text = "Limit"; await SearchAsync(); await settle();
            check(searchHits.Length == 2 && searchStatus.Text.StartsWith("Hasil 1 dari 2 · Halaman 1", StringComparison.Ordinal), "one-page screenshot-equivalent fixture reports first of exactly two occurrences");
            // Reconstruct the confirmed old row assignment only inside this isolated audit.
            Grid.SetRow(searchPanel, 2); await settle();
            var oldSession = sessionPicker.TransformToVisual(View).TransformBounds(new Rect(0, 0, sessionPicker.ActualWidth, sessionPicker.ActualHeight));
            var oldResults = searchPanel.TransformToVisual(View).TransformBounds(new Rect(0, 0, searchPanel.ActualWidth, searchPanel.ActualHeight));
            check(oldSession.Top < oldResults.Bottom && oldResults.Top < oldSession.Bottom, "old shared-row placement reproduces actual session/search intersection");
            await capture("00-search-old-row-overlap-reconstructed"); Grid.SetRow(searchPanel, 3); await settle();
            await capture("20-search-one-page-two-hits");
            SelectSearchResult(1); await searchNavigation; await settle();
            check(searchStatus.Text.StartsWith("Hasil 2 dari 2 · Halaman 1", StringComparison.Ordinal) && Math.Abs(viewport.ZoomFactor - 1.39) < .005, "one-page second occurrence counter and zoom remain correct");
            await capture("21-search-one-page-second");
            var fixture = Path.Combine(output, "search-position-synthetic.pdf"); WriteSearchPositionFixture(fixture);
            var hash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
            owner.homeFiles.Clear(); owner.homeFiles.Add(fixture); owner.homeFiles.Add(path!);
            await OpenAsync(fixture); RefreshSession(false); await settle();
            fit = false; viewport.ChangeView(null, 0, 1.39f, true); await settle();
            searchInput.Text = "Limit"; await SearchEnterAsync(false); await settle();
            check(searchHits.Length == 5 && searchHits.All(h => h.Boxes is { Length: > 0 }), "five positioned hits across regular bold italic and mixed paper pages");
            check(Math.Abs(viewport.ZoomFactor - 1.39) < .005, "search retains user zoom 139 percent");
            Rect Bounds(FrameworkElement element) => element.TransformToVisual(View).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            void Layout(string label)
            {
                var session = Bounds(sessionPicker); var results = Bounds(searchPanel); var reading = Bounds(workspace);
                check(sessionPicker.Visibility == Visibility.Visible && session.Bottom <= results.Top + .1 && results.Bottom <= reading.Top + .1,
                    label + " session picker search results and reading viewport do not intersect");
            }
            Layout("desktop");
            var first = searchHits[0].Boxes![0]; var second = searchHits[1].Boxes![0];
            check(first.Y < second.Y && first.Width < .1 && second.Width < .1, "same page Limit matches have distinct tight glyph ranges");
            check(pageSlots[0].SearchOverlay.Children.Count == 2 && pageSlots[0].SearchOverlay.IsHitTestVisible == false, "real PDF overlay has both occurrences and passes input through");
            var markers = pageSlots[0].SearchOverlay.Children.OfType<Border>().ToArray();
            check(markers[0].BorderThickness.Left == 1 && markers[1].BorderThickness.Left == 0, "active orange marker uses outline and other marker has no outline");
            await capture("13-search-limit-139-first");
            ((IInvokeProvider)new ButtonAutomationPeer(searchNext).GetPattern(PatternInterface.Invoke)).Invoke(); await searchNavigation; await settle();
            var secondTop = second.Y * pageSlots[0].Image.Height * viewport.ZoomFactor;
            check(searchResults.SelectedIndex == 1 && pageIndex == 0 && secondTop >= viewport.VerticalOffset
                && secondTop < viewport.VerticalOffset + viewport.ViewportHeight, "next button reveals second occurrence with only necessary same-page scroll");
            check(searchStatus.Text.StartsWith("Hasil 2 dari 5 · Halaman 1", StringComparison.Ordinal), "active occurrence counter displays index count and page");
            await capture("14-search-limit-139-second");
            await SearchEnterAsync(true); await settle();
            var firstTop = first.Y * pageSlots[0].Image.Height * viewport.ZoomFactor;
            check(searchResults.SelectedIndex == 0 && firstTop >= viewport.VerticalOffset
                && firstTop < viewport.VerticalOffset + viewport.ViewportHeight, "Shift Enter reveals first occurrence without unnecessary offset restoration");
            await SearchEnterAsync(false); await settle(); check(searchResults.SelectedIndex == 1, "same-query Enter advances without rerunning search");
            searchResults.SelectedIndex = 2; await searchNavigation; await settle();
            check(pageIndex == 1 && Math.Abs(viewport.ZoomFactor - 1.39) < .005, "clicked bold hit changes page while preserving zoom");
            await capture("15-search-bold");
            searchResults.SelectedIndex = 3; await searchNavigation; await settle(); await capture("16-search-italic");
            foreach (var factor in new[] { .85f, 1.15f })
            {
                Zoom(factor); await settle();
                var marker = pageSlots[1].SearchOverlay.Children.OfType<Border>().Last();
                var box = searchHits[3].Boxes![0];
                check(Math.Abs(Canvas.GetLeft(marker) / pageSlots[1].Image.Width - box.X) < .000001 && Math.Abs(Canvas.GetTop(marker) / pageSlots[1].Image.Height - box.Y) < .000001, "zoom changes retain normalized glyph marker positions");
            }
            owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 720)); await settle(); Layout("compact");
            check(workspace.ActualHeight > 150, "compact search retains reading space"); await capture("17-search-compact");
            fit = true; Fit(); await Rendering; await settle(); await capture("18-search-fit");
            FocusSearch(); check(searchInput.FocusState != FocusState.Unfocused, "Ctrl F target focuses search textbox");
            ClearSearch(); check(pageSlots.All(p => p.SearchOverlay.Children.Count == 0), "close query clears every page overlay");
            searchInput.Text = "Limit"; var pending = SearchAsync(); searchInput.Text = "new-query"; await pending;
            check(searchHits.Length == 0 && pageSlots.All(p => p.SearchOverlay.Children.Count == 0), "rapid query replacement rejects old highlights");
            var rotated = Path.Combine(output, "search-rotation-crop.pdf"); WriteSearchPositionFixture(rotated, true);
            owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000)); await OpenAsync(rotated); await settle();
            searchInput.Text = "Limit"; await SearchAsync(); searchResults.SelectedIndex = 2; await searchNavigation; await settle();
            check(searchHits[2].Boxes is { Length: > 0 } && pageSlots[1].Image.Width > pageSlots[1].Image.Height, "cropped clockwise rotated page has positioned results in displayed orientation");
            await capture("19-search-rotation-crop");
            var noMetrics = Path.Combine(output, "search-no-metrics.pdf");
            WriteSearchPositionFixture(noMetrics, customStream: "BT /F1 12 Tf 60 600 Td (Limit) Tj ET", customFont: "<< /Type /Font /Subtype /TrueType /BaseFont /Synthetic /Encoding /WinAnsiEncoding /FontDescriptor << /Type /FontDescriptor >> >>");
            await OpenAsync(noMetrics); searchInput.Text = "Limit"; await SearchAsync(); await settle();
            check(searchHits.Length == 1 && searchStatus.Text.Contains("posisi tepat belum tersedia", StringComparison.Ordinal) && pageSlots.All(p => p.SearchOverlay.Children.Count == 0), "no metrics shows truthful fallback with no fake overlay");
            var scan = Path.Combine(output, "search-scan.pdf"); WriteSearchPositionFixture(scan, customStream: "/Scan Do");
            await OpenAsync(scan); searchInput.Text = "Limit"; await SearchAsync();
            check(searchHits.Length == 0 && searchStatus.Text.Contains("perlu OCR", StringComparison.Ordinal), "scan-only search explains OCR requirement without running OCR");
            var partial = Path.Combine(output, "search-partial.pdf"); WriteSearchPositionFixture(partial, customStream: "BT /F1 12 Tf 60 600 Td (Limit) Tj ET /Logo Do");
            await OpenAsync(partial); searchInput.Text = "Limit"; await SearchAsync();
            check(searchHits.Length == 1 && searchHits[0].Boxes is { Length: > 0 } && searchStatus.Text.Contains("belum lengkap", StringComparison.Ordinal) && !searchStatus.Text.Contains("font", StringComparison.OrdinalIgnoreCase), "unsupported object warning retains supported positioned text without accusing fonts");
            var capped = Path.Combine(output, "search-cap.pdf"); WriteSearchPositionFixture(capped, customStream: "BT /F1 12 Tf 60 600 Td (" + string.Join(' ', Enumerable.Repeat("Limit", 110)) + ") Tj ET");
            await OpenAsync(capped); searchInput.Text = "Limit"; await SearchAsync();
            check(searchHits.Length == 100 && searchStatus.Text.Contains("hasil terpotong", StringComparison.Ordinal), "active result counter retains truncation warning");
            var afterHash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
            check(hash.SequenceEqual(afterHash), "positioned search overlays preserve source SHA256");
            ClearSearch();
            await AuditSearchFlickerAsync(output, check, settle);
        }

        private async Task AuditOpaqueSearchAsync(string output, Action<bool, string> check, Func<Task> settle, Func<string, Task> capture)
        {
            var fixture = Path.Combine(output, "search-opaque-state.pdf");
            WriteSearchPositionFixture(fixture, customStream: "/GS5 gs BT /F2 18 Tf 1 0 0 1 124 680 Tm (Alamat) Tj ET /GS8 gs BT /F2 20 Tf 1 0 0 1 118 360 Tm (Alamat) Tj ET", opaqueState: true);
            var privateInput = Environment.GetEnvironmentVariable("OTO_SEARCH_PRIVATE_PDF");
            var inputs = privateInput is not null && libraryTestPath is not null ? new[] { privateInput, fixture } : new[] { fixture };
            foreach (var input in inputs)
            {
            var privateSource = input != fixture;
            var originalHash = SHA256.HashData(await File.ReadAllBytesAsync(input));
            try
            {
            foreach (var width in new[] { 1586, 720 })
            foreach (var zoom in new[] { .5f, 1f, 2f })
            {
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 1000));
                await OpenAsync(input); await settle(); fit = false;
                viewport.ChangeView(null, 0, zoom, true); await settle();
                searchInput.Text = "alamat"; await SearchAsync(); await settle();
                check(searchHits.Length == 2 && searchHits.All(h => h.Boxes is { Length: 1 }), "opaque-state native search has two positioned labels");
                var markers = pageSlots[0].SearchOverlay.Children.OfType<Border>().ToArray();
                check(markers.Length == 2 && markers[0].BorderThickness.Left == 1 && markers[1].BorderThickness.Left == 0, "opaque-state first active orange and other yellow marker");
                if (!privateSource) await capture(FormattableString.Invariant($"opaque-{width}-{zoom * 100:0}-first"));
                await SearchEnterAsync(false); await settle();
                check(searchResults.SelectedIndex == 1 && markers[1].BorderThickness.Left == 1 && markers[0].BorderThickness.Left == 0, "opaque-state Enter switches active occurrence");
                if (!privateSource) await capture(FormattableString.Invariant($"opaque-{width}-{zoom * 100:0}-second"));
                await SearchEnterAsync(true); await settle();
                check(searchResults.SelectedIndex == 0, "opaque-state Shift Enter returns first occurrence");
                ((IInvokeProvider)new ButtonAutomationPeer(searchNext).GetPattern(PatternInterface.Invoke)).Invoke(); await searchNavigation; await settle();
                check(searchResults.SelectedIndex == 1, "opaque-state next button selects second occurrence");
                ((IInvokeProvider)new ButtonAutomationPeer(searchPrevious).GetPattern(PatternInterface.Invoke)).Invoke(); await searchNavigation; await settle();
                check(searchResults.SelectedIndex == 0, "opaque-state previous button selects first occurrence");
                searchResults.SelectedIndex = 1; await searchNavigation; await settle();
                check(markers[1].BorderThickness.Left == 1 && markers.SequenceEqual(pageSlots[0].SearchOverlay.Children.OfType<Border>()) && Math.Abs(viewport.ZoomFactor - zoom) < .005, "opaque-state click retains overlay cache and zoom");
                foreach (var (marker, box) in markers.Zip(searchHits.Select(h => h.Boxes![0])))
                    check(Math.Abs(Canvas.GetLeft(marker) / pageSlots[0].Image.Width - box.X) < .000001 && Math.Abs(Canvas.GetTop(marker) / pageSlots[0].Image.Height - box.Y) < .000001, "opaque-state normalized marker stays at label across size and zoom");
            }
            ClearSearch();
            var finalHash = SHA256.HashData(await File.ReadAllBytesAsync(input));
            check(originalHash.SequenceEqual(finalHash), privateSource ? "private original native search preserves source bytes" : "opaque-state fixture preserves source bytes");
            }
            finally
            {
                if (privateSource)
                {
                    owner.savedJobs.RemoveAll(job => job.Paths.Contains(input, StringComparer.OrdinalIgnoreCase));
                    owner.SaveLibrary();
                }
            }
            }
        }

        private async Task AuditSearchFlickerAsync(string output, Action<bool, string> check, Func<Task> settle)
        {
            var baseline = Environment.GetCommandLineArgs().Contains("--flicker-baseline", StringComparer.Ordinal);
            void Stage(string stage) => File.WriteAllText(Path.Combine(output, "flicker-stage.txt"), stage);
            Stage("fixture");
            var fixture = Path.Combine(output, "flicker.pdf");
            var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>" };
            var pages = new List<int>();
            for (var i = 0; i < 12; i++)
            {
                pages.Add(objects.Count + 1);
                var width = i % 3 == 0 ? 595 : i % 3 == 1 ? 842 : 1191;
                var height = i % 3 == 0 ? 842 : i % 3 == 1 ? 1191 : 1684;
                var content = $"BT /F1 12 Tf 60 {height - 120} Td (Signal first) Tj 0 -90 Td (Signal nearby) Tj 0 -380 Td (Signal distant) Tj ET";
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /Font << /F1 3 0 R >> >> /Contents {objects.Count + 2} 0 R >>");
                objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
            }
            objects[1] = $"<< /Type /Pages /Count {pages.Count} /Kids [{string.Join(' ', pages.Select(p => p + " 0 R"))}] >>";
            var pdf = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
            for (var i = 0; i < objects.Count; i++) { offsets.Add(pdf.Length); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
            var xref = pdf.Length; pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            File.WriteAllBytes(fixture, Encoding.ASCII.GetBytes(pdf.ToString()));
            var hash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
            await OpenAsync(fixture); await settle(); searchInput.Text = "Signal"; await SearchAsync(); await settle();
            check(searchHits.Length == 36, "flicker fixture has 36 hits across A4 A3 A2 pages");
            var traces = new List<object>();
            foreach (var width in new[] { 1586, 720 })
            foreach (var zoom in new[] { 1f, 1.25f, 2f })
            {
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 900)); await settle(); fit = false;
                viewport.ChangeView(null, null, zoom, true); await settle();
                searchResults.SelectedIndex = 0; await searchNavigation; await settle(); FocusSearch();
                foreach (var target in new[] { 1, 2, 3, 30, 35, 0 })
                {
                    var beforeThumbnails = thumbnails.Children.ToArray();
                    var beforeMarkers = pageSlots.SelectMany(p => p.SearchOverlay.Children).ToArray();
                    var beforeKey = thumbnailKey;
                    var renderCount = pageRenderRequests; var evictionCount = bitmapEvictions;
                    var thumbnailCount = thumbnailRenderRequests; var rebuildCount = thumbnailRebuilds;
                    var workerCount = searchWorkerRuns;
                    var events = new List<double>(); var frames = new List<object>();
                    var clock = System.Diagnostics.Stopwatch.StartNew();
                    double peakPixels = 0; var emptyFrames = 0; var blankThumbnailFrames = 0;
                    void Observe(object? sender, object e)
                    {
                        var pixels = pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width);
                        peakPixels = Math.Max(peakPixels, pixels);
                        if (pageSlots.All(p => p.Image.Source is null)) emptyFrames++;
                        var thumbImages = thumbnails.Children.OfType<Button>().Select(b => b.Content).OfType<StackPanel>().SelectMany(p => p.Children).OfType<Microsoft.UI.Xaml.Controls.Image>().ToArray();
                        if (thumbImages.Length > 0 && thumbImages.All(p => p.Source is null)) blankThumbnailFrames++;
                        frames.Add(new { ms = clock.Elapsed.TotalMilliseconds, offset = viewport.VerticalOffset,
                        zoom = viewport.ZoomFactor, selected = searchResults.SelectedIndex, page = pageIndex,
                        bitmapPages = pageSlots.Select((p, i) => (p, i)).Where(p => p.p.Image.Source is not null).Select(p => p.i).ToArray(),
                        rasterPixels = pixels,
                        thumbnails = thumbnails.Children.Count,
                        sameThumbnails = beforeThumbnails.SequenceEqual(thumbnails.Children), sameMarkers = beforeMarkers.SequenceEqual(pageSlots.SelectMany(p => p.SearchOverlay.Children)) });
                    }
                    void Changed(object? sender, ScrollViewerViewChangedEventArgs e) { if (!e.IsIntermediate) events.Add(viewport.VerticalOffset); }
                    viewport.ViewChanged += Changed; Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += Observe;
                    var name = $"{(baseline ? "before" : "after")}-{width}-{zoom * 100:0}-{target}";
                    try
                    {
                        searchResults.SelectedIndex = target; var pending = searchNavigation;
                        // Sample consecutive native content frames while navigation is in flight, without Settle.
                        for (var frame = 0; frame < 6; frame++)
                        {
                            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap(); await bitmap.RenderAsync(owner.root);
                            var bytes = await bitmap.GetPixelsAsync();
                            var file = await (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + $"-{frame}.png", Windows.Storage.CreationCollisionOption.FailIfExists);
                            using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                            var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);
                            encoder.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
                                (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
                            if (pending.IsCompleted && frame >= 2) break;
                            await Task.Delay(16);
                        }
                        await pending; await settle();
                    }
                    finally { viewport.ViewChanged -= Changed; Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= Observe; }
                    traces.Add(new { name, events, frames, elapsedMs = clock.Elapsed.TotalMilliseconds,
                        pageRenders = pageRenderRequests - renderCount, evictions = bitmapEvictions - evictionCount,
                        thumbnailRenders = thumbnailRenderRequests - thumbnailCount, thumbnailRebuilds = thumbnailRebuilds - rebuildCount,
                        workerRuns = searchWorkerRuns - workerCount, peakPixels, emptyFrames, blankThumbnailFrames });
                    if (!baseline)
                    {
                        check(events.Distinct().Count() <= 1, name + " has at most one direct viewport destination");
                        check(Math.Abs(viewport.ZoomFactor - zoom) < .005 && searchInput.FocusState != FocusState.Unfocused, name + " preserves zoom and search focus");
                        check(beforeMarkers.SequenceEqual(pageSlots.SelectMany(p => p.SearchOverlay.Children)), name + " retains highlight element identities");
                        if (beforeKey == thumbnailKey)
                            check(beforeThumbnails.SequenceEqual(thumbnails.Children) && thumbnailRenderRequests == thumbnailCount,
                                name + " retains thumbnail identities with no thumbnail renders in same group");
                        check(searchWorkerRuns == workerCount, name + " does not rerun the search worker");
                        check(emptyFrames == 0 && blankThumbnailFrames == 0, name + " never blanks all page or thumbnail images in observed native frames");
                        check(peakPixels <= 16_000_000, name + " bounds transient retained destination and visible raster requests");
                        check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000,
                            name + " settles within retained raster budget");
                    }
                }
            }
            File.WriteAllText(Path.Combine(output, "flicker-transitions.json"), System.Text.Json.JsonSerializer.Serialize(traces));
            Stage("transitions complete");
            if (!baseline)
            {
                var workerCount = searchWorkerRuns;
                Stage("rapid Enter");
                marks.Add(0); bookmarksOnly = true; var pendingThumbnails = DrawThumbnailsAsync(generation);
                bookmarksOnly = false; await DrawThumbnailsAsync(generation); await pendingThumbnails;
                check(thumbnailKey?.StartsWith("pages:", StringComparison.Ordinal) == true && thumbnails.Children.OfType<Button>().Count(b => b.Tag is int) == 8,
                    "rapid thumbnail tab reversal rejects stale bookmarks replacement");
                marks.Remove(0);
                var requests = new List<Task>();
                for (var i = 0; i < 45; i++) requests.Add(SearchEnterAsync(false));
                await Task.WhenAll(requests); await settle();
                check(searchResults.SelectedIndex == 9 && searchStatus.Text.StartsWith("Hasil 10 dari 36", StringComparison.Ordinal), "45 rapid Enter presses wrap and settle on latest occurrence");
                check(searchWorkerRuns == workerCount && searchMarkers[9].All(m => m.BorderThickness.Left == 1), "rapid Enter retains worker results and latest active marker");
                searchResults.SelectedIndex = 0; await searchNavigation; await SearchEnterAsync(true); await settle();
                Stage("query change");
                check(searchResults.SelectedIndex == 35, "Shift Enter wraps from first to last occurrence");
                searchResults.SelectedIndex = 0; var stale = searchNavigation; searchInput.Text = "replacement"; await stale; await settle();
                check(searchHits.Length == 0 && searchStatus.Text.Length == 0 && pageSlots.All(p => p.SearchOverlay.Children.Count == 0), "query change invalidates pending navigation and markers");
                searchInput.Text = "Signal"; await SearchAsync(); await settle();
                Stage("close search");
                searchResults.SelectedIndex = 35; stale = searchNavigation; ClearSearch(); await stale; await settle();
                check(searchHits.Length == 0 && searchPanel.Visibility == Visibility.Collapsed, "close search rejects pending navigation");
                check(pageSlots.Sum(p => p.RenderWidth * (double)p.RenderWidth * p.Image.Height / p.Image.Width) <= 8_000_000,
                    "close search releases prepared destination outside visible range");
                searchInput.Text = "Signal"; await SearchAsync(); await settle();
                Stage("zoom during navigation");
                searchResults.SelectedIndex = 35; stale = searchNavigation;
                viewport.ChangeView(null, null, 1.25f, true); await stale; await settle();
                check(Math.Abs(viewport.ZoomFactor - 1.25) < .005, "zoom change during navigation rejects stale captured scale");
                viewport.ChangeView(null, null, 2f, true); await settle();
                searchResults.SelectedIndex = 0; stale = searchNavigation; Deactivate(); await stale;
                Stage("resume");
                check(pageSlots.All(p => p.Image.Source is null), "deactivation rejects navigation raster commits");
                check(thumbnailDocument is null && overlayPages is null, "deactivation drops thumbnail and overlay cache owners");
                Resume(); await Rendering; await settle();
                check(picture.Source is not null && Math.Abs(viewport.ZoomFactor - 2) < .005, "resume restores valid raster and retained zoom after search cancellation");
                var other = Path.Combine(output, "flicker-other.pdf"); WriteSearchPositionFixture(other);
                Stage("document change");
                searchResults.SelectedIndex = 35; stale = searchNavigation; await OpenAsync(other); await stale; await settle();
                check(path == other && searchHits.Length == 0 && pageSlots.Length == 3 && picture.Source is not null, "document change rejects old navigation output");
                searchInput.Text = "missing-word"; await SearchAsync(); workerCount = searchWorkerRuns;
                Stage("cached no result");
                await SearchEnterAsync(false); await SearchAsync();
                check(searchHits.Length == 0 && searchWorkerRuns == workerCount && searchStatus.Text.Contains("Tidak ditemukan", StringComparison.Ordinal), "completed no-result query is cached without worker reruns");
                ClearSearch(); searchInput.Text = "Limit"; workerCount = searchWorkerRuns;
                Stage("pending query");
                var pendingSearch = SearchAsync(); await SearchEnterAsync(false); await SearchAsync(); await pendingSearch; await settle();
                check(searchWorkerRuns == workerCount + 1 && searchHits.Length == 5, "same query while worker is pending launches exactly one search");
                ClearSearch(); searchInput.Text = "Limit"; pendingSearch = SearchAsync(); CancelSearch(); await pendingSearch; await settle();
                Stage("worker cancelled");
                check(searchHits.Length == 0 && searchStatus.Text.Contains("dibatalkan", StringComparison.Ordinal), "worker cancellation rejects late search results");
            }
            var finalHash = SHA256.HashData(await File.ReadAllBytesAsync(fixture));
            check(hash.SequenceEqual(finalHash), "flicker transitions preserve source SHA256");
            ClearSearch();
            Stage("complete");
        }
    }
}
#endif
