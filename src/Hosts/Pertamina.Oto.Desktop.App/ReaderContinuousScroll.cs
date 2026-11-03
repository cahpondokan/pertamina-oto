using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class PdfReaderView
    {
        private sealed record PageSlot(double Top, Image Image, Border Frame)
        {
            internal uint RenderWidth { get; set; }
            internal Canvas SearchOverlay { get; } = new() { IsHitTestVisible = false };
        }

        private readonly Canvas pageSurface = new();
        // Applying ChangeView inside SizeChanged can use the previous viewport metrics.
        // Let the native layout settle before refitting; one timer coalesces a resize drag.
        private readonly DispatcherTimer resizeTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };
        private readonly DispatcherTimer zoomRenderTimer = new() { Interval = TimeSpan.FromMilliseconds(180) };
        private float renderedZoom;
        private bool readerZoomIntermediate;
        private PageSlot[] pageSlots = [];
        private float fittedZoom = 1;
        private (int First, int Last, int Page) renderedRange = (-1, -1, -1);
        private (int First, int Last) retainedRange = (-1, -1);
#if DEBUG
        private int pageRenderRequests;
        private int bitmapEvictions;
#endif

        private static PageSlot[] MeasurePages(PdfDocument pdf)
        {
            var pages = new PageSlot[pdf.PageCount]; double top = 0;
            for (uint i = 0; i < pdf.PageCount; i++)
            {
                using var page = pdf.GetPage(i);
                var width = page.Size.Width; var height = page.Size.Height;
                var ratio = height / width;
                if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0 || !double.IsFinite(ratio) || ratio is < .05 or > 20)
                    throw new IOException("Unsupported page dimensions");
                var image = new Image { Width = width, Height = height, Stretch = Stretch.Uniform };
                AutomationProperties.SetName(image, $"Halaman PDF {i + 1}");
                var layers = new Grid(); layers.Children.Add(image);
                var frame = new Border { Width = width, Height = height, Background = Brush(255, 255, 255), Child = layers };
                pages[i] = new PageSlot(top, image, frame); layers.Children.Add(pages[i].SearchOverlay); top += height + 24;
            }
            return pages;
        }

        private void InstallPages(PageSlot[] pages)
        {
            ClearPageBitmaps(); pageSlots = pages; pageSurface.Children.Clear();
            thumbnails.Children.Clear();
            pageSurface.Width = pages.Max(p => p.Image.Width);
            foreach (var slot in pages)
            {
                Canvas.SetTop(slot.Frame, slot.Top);
                Canvas.SetLeft(slot.Frame, (pageSurface.Width - slot.Image.Width) / 2);
                pageSurface.Children.Add(slot.Frame);
            }
            picture = pages[0].Image;
            UpdatePageExtent(viewport.ZoomFactor);
        }

        private void UpdatePageExtent(float zoom)
        {
            if (pageSlots.Length == 0) return;
            var last = pageSlots[^1];
            // Keep even a short final page reachable at the top of the viewport.
            pageSurface.Height = last.Top + Math.Max(last.Image.Height, viewport.ActualHeight / zoom);
        }

        private void SelectPage(int index)
        {
            pageIndex = index; picture = pageSlots[index].Image;
            pageInput.Text = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            UpdateControls(); PageSelected?.Invoke(picture);
        }

        private int PageAt(double offset)
        {
            var low = 0; var high = pageSlots.Length - 1;
            while (low < high)
            {
                var middle = (low + high + 1) / 2;
                if (pageSlots[middle].Top <= offset + .5) low = middle; else high = middle - 1;
            }
            return low;
        }

        private void OnReaderViewChanged(ScrollViewerViewChangedEventArgs e)
        {
            // Collapsed tabs and native restoration can report zero offsets or
            // transient zoom. Those notifications must not replace reading state.
            if (!Visible || suspended) return;
            zoomControls.Sync();
            if (settingZoom) return;
            zoomRenderTimer.Stop();
            zoomRenderTimer.Tick -= ZoomRenderSettled; zoomRenderTimer.Tick += ZoomRenderSettled; zoomRenderTimer.Start();
            readerZoomIntermediate = e.IsIntermediate;
            if (Math.Abs(viewport.ZoomFactor - fittedZoom) > Math.Max(.0001, fittedZoom * .005)) fit = false;
            UpdatePageExtent(viewport.ZoomFactor);
            // Retain the valid surface throughout native zoom. Scroll-only motion
            // can still load neighbors, but scale changes render after settling.
            if (e.IsIntermediate && Math.Abs(renderedZoom - viewport.ZoomFactor) > .000001) return;
            RefreshScroll();
        }

        private void ZoomRenderSettled(object? sender, object e)
        {
            zoomRenderTimer.Stop();
            if (!Visible || suspended || settingZoom) return;
            if (readerZoomIntermediate) { zoomRenderTimer.Start(); return; }
            if (Math.Abs(renderedZoom - viewport.ZoomFactor) < .005) return;
            if (!Rendering.IsCompleted) { zoomRenderTimer.Start(); return; }
            renderedZoom = viewport.ZoomFactor; RefreshScroll(force: true);
        }

        private void RefreshScroll(bool force = false)
        {
            if (settingZoom || suspended || !Visible || document is null || pageSlots.Length == 0) return;
            var top = viewport.VerticalOffset / viewport.ZoomFactor;
            var page = PageAt(top);
            if (page != pageIndex) SelectPage(page);
            var first = Math.Max(0, page - 1);
            var last = Math.Min(pageSlots.Length - 1, PageAt(top + viewport.ActualHeight / viewport.ZoomFactor) + 1);
            var range = (first, last, page);
            if (!force && range == renderedRange) return;
            renderedRange = range;
            Rendering = RenderVisiblePagesAsync(first, last, ++generation);
        }

        private async Task RenderVisiblePagesAsync(int first, int last, int ticket, bool prepareNavigation = false, Func<bool>? stillCurrent = null)
        {
            var pdf = document!; var pages = pageSlots;
            var scale = viewport.ZoomFactor;
            bool Current() => ticket == generation && ReferenceEquals(pdf, document) && ReferenceEquals(pages, pageSlots)
                && Visible && !suspended && Math.Abs(scale - viewport.ZoomFactor) < .001 && (stillCurrent?.Invoke() ?? true);
            if (!Current()) return;
            // Across the retained page images, cap the requested decoded pixels at 8 million.
            // Very small/zoomed-out pages share that budget instead of retaining every full-size bitmap.
            var pixelsPerPage = 8_000_000d / (last - first + 1);
            // At most the last completed range and the new range survive a
            // cancelled request. Never evict the old valid range before decode.
            for (var i = 0; i < pages.Length; i++)
                if ((i < first || i > last) && (i < retainedRange.First || i > retainedRange.Last))
                {
#if DEBUG
                    if (pages[i].Image.Source is not null) bitmapEvictions++;
#endif
                    pages[i].Image.Source = null; pages[i].RenderWidth = 0;
                }
            try
            {
                foreach (var index in Enumerable.Range(first, last - first + 1).OrderBy(i => Math.Abs(i - pageIndex)))
                {
                    var slot = pages[index]; var ratio = slot.Image.Height / slot.Image.Width;
                    var requested = slot.Image.Width * scale * (View.XamlRoot?.RasterizationScale ?? 1);
                    var width = (uint)Math.Max(1, Math.Min(Math.Min(Math.Max(320, requested), 3200 / Math.Max(1, ratio)), Math.Sqrt(pixelsPerPage / ratio)));
                    if (slot.Image.Source is not null && slot.RenderWidth == width) continue;
                    await RenderGate.WaitAsync();
                    try
                    {
                        if (!Current()) return;
                        using var page = pdf.GetPage((uint)index); using var stream = new InMemoryRandomAccessStream();
#if DEBUG
                        pageRenderRequests++;
#endif
                        await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = width });
                        stream.Seek(0); var bitmap = new BitmapImage(); await bitmap.SetSourceAsync(stream);
                        if (!Current()) return;
                        slot.Image.Source = bitmap; slot.RenderWidth = width;
                    }
                    finally { RenderGate.Release(); }
                }
                if (!Current() || prepareNavigation) return;
                renderedZoom = scale;
                retainedRange = (first, last);
                for (var i = 0; i < pages.Length; i++)
                    if (i < first || i > last)
                    {
#if DEBUG
                        if (pages[i].Image.Source is not null) bitmapEvictions++;
#endif
                        pages[i].Image.Source = null; pages[i].RenderWidth = 0;
                    }
                await DrawThumbnailsAsync(ticket);
            }
            catch (Exception error) { if (ticket == generation) Failure("reader-render", error); }
        }

        private long RetainedRasterBytes => pageSlots.Sum(slot => RasterBytes(slot.Image)) +
            thumbnails.Children.OfType<Button>().Select(button => button.Content).OfType<StackPanel>()
                .SelectMany(panel => panel.Children.OfType<Image>()).Sum(RasterBytes);

        private static long RasterBytes(Image image) => image.Source is BitmapSource bitmap
            ? (long)bitmap.PixelWidth * bitmap.PixelHeight * 4 : 0;

        private void TrimInactivePageBitmaps()
        {
            if (pageSlots.Length == 0) return;
            var top = savedVertical / savedZoom;
            var first = PageAt(top);
            var last = PageAt(top + viewport.ActualHeight / savedZoom);
            for (var i = 0; i < pageSlots.Length; i++)
                if (i < first || i > last) { pageSlots[i].Image.Source = null; pageSlots[i].RenderWidth = 0; }
            retainedRange = (first, last);
            // Resume explicitly refreshes neighbors. Keep the retained range
            // identifiable if another tab switch cancels a pending restore.
            renderedRange = (first, last, pageIndex);
        }

        private void ClearPageBitmaps()
        {
            zoomRenderTimer.Stop(); renderedZoom = viewport.ZoomFactor;
            readerZoomIntermediate = false;
            thumbnailDocument = null; thumbnailKey = null;
            overlayPages = null; overlayHits = null; searchMarkers = []; activeMarker = -1;
            foreach (var page in pageSlots) { page.Image.Source = null; page.RenderWidth = 0; }
            picture.Source = null; renderedRange = (-1, -1, -1);
            retainedRange = (-1, -1);
        }
    }
}
