using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private readonly Canvas editorSurface = new();
        private ZoomController? editorZoom;
        private readonly DispatcherTimer editorScrollTimer = new() { Interval = TimeSpan.FromMilliseconds(80) };
        private Canvas[] editorSheets = [];
        private Image[] editorImages = [];
        private uint[] editorRenderWidths = [];
        private double[] editorTops = [];
        private bool initialEditorFit;
        private bool scrollingEditor;
        private bool editorScrollPending;
        private bool editorZoomIntermediate;
        private float editorObservedZoom;
        private (int First, int Last) editorRetainedRange = (-1, -1);
        private string inactiveDraftState = "";

        private void RefreshInactiveDrafts()
        {
            var state = State;
            if (state == inactiveDraftState) return;
            inactiveDraftState = state;
            var activeSheet = sheet; var activeImage = background; var activePage = page;
            try
            {
                for (var i = 0; i < editorSheets.Length; i++)
                {
                    if (i == activePage) continue;
                    sheet = editorSheets[i]; background = editorImages[i]; page = i; Refresh(true);
                }
            }
            finally { sheet = activeSheet; background = activeImage; page = activePage; }
        }

        private void InstallEditorPages()
        {
            editorScrollTimer.Stop(); editorSurface.Children.Clear(); inactiveDraftState = "";
            editorZoomIntermediate = false; editorRetainedRange = (-1, -1);
            editorObservedZoom = viewport.ZoomFactor;
            editorSheets = new Canvas[pageSizes.Length]; editorImages = new Image[pageSizes.Length]; editorTops = new double[pageSizes.Length];
            editorRenderWidths = new uint[pageSizes.Length];
            editorSurface.Width = pageSizes.Max(s => s.Width); double top = 0;
            for (var i = 0; i < pageSizes.Length; i++)
            {
                var size = pageSizes[i]; var index = i;
                var canvas = new Canvas { Width = size.Width, Height = size.Height, Background = Brush(255, 255, 255) };
                var image = new Image { Width = size.Width, Height = size.Height, IsHitTestVisible = false };
                canvas.Children.Add(image); editorSheets[i] = canvas; editorImages[i] = image; editorTops[i] = top;
                Canvas.SetTop(canvas, top); Canvas.SetLeft(canvas, (editorSurface.Width - size.Width) / 2); editorSurface.Children.Add(canvas);
                canvas.PointerPressed += async (_, e) =>
                {
                    if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Touch) return;
                    if (page != index) { e.Handled = true; await RenderAsync(index, true, false); return; }
                    Pressed(canvas, e);
                };
                canvas.PointerMoved += Moved; canvas.PointerReleased += Released;
                canvas.PointerCanceled += (_, _) => CancelGesture(); canvas.PointerCaptureLost += (_, _) => CancelGesture();
                top += size.Height + 24;
            }
            editorSurface.Height = top; initialEditorFit = true;
            editorScrollTimer.Tick -= EditorScrollTick; editorScrollTimer.Tick += EditorScrollTick;
            ActivateEditorPage(0);
        }

        private void ActivateEditorPage(int index)
        {
            sheet.IsHitTestVisible = false;
            sheet = editorSheets[index]; background = editorImages[index]; sheet.IsHitTestVisible = true;
            // Inactive pages retain their draft visuals but route no object gestures to the active page.
            for (var i = 0; i < editorSheets.Length; i++)
                foreach (var child in editorSheets[i].Children) child.IsHitTestVisible = i == index && child != editorImages[i];
            foreach (var canvas in editorSheets) canvas.IsHitTestVisible = true;
        }

        private void QueueEditorScroll()
        {
            if (pdf is null || editorTops.Length == 0) return;
            editorScrollPending = true;
            var last = editorTops.Length - 1;
            editorSurface.Height = editorTops[last] + Math.Max(pageSizes[last].Height, viewport.ActualHeight / viewport.ZoomFactor);
            editorScrollTimer.Stop(); editorScrollTimer.Start();
        }

        private int EditorPageAt(double y)
        {
            var index = Array.BinarySearch(editorTops, y);
            return index >= 0 ? index : Math.Clamp(~index - 1, 0, editorTops.Length - 1);
        }

        private async void EditorScrollTick(object? sender, object e)
        {
            editorScrollTimer.Stop();
            if (editorZoomIntermediate) { editorScrollTimer.Start(); return; }
            if (scrollingEditor) return;
            if (Working || !Visible || pdf is null || editorTops.Length == 0 || drawing || drawingShape || dragging is not null) { if (Visible && pdf is not null) QueueEditorScroll(); return; }
            scrollingEditor = true;
            editorScrollPending = false;
            try
            {
                var top = viewport.VerticalOffset / viewport.ZoomFactor;
                var target = EditorPageAt(top + Math.Min(80, viewport.ActualHeight / viewport.ZoomFactor / 4));
                if (target != page) await RenderAsync(target, true, false);
                if (target != page) { viewport.ChangeView(null, editorTops[page] * viewport.ZoomFactor, null, true); return; }
                var first = Math.Max(0, target - 1);
                var last = Math.Min(editorTops.Length - 1, EditorPageAt(top + viewport.ActualHeight / viewport.ZoomFactor) + 1);
                last = Math.Min(last, first + 11);
                var document = pdf;
                for (var i = 0; i < editorImages.Length; i++)
                    if ((i < first || i > last) && (i < editorRetainedRange.First || i > editorRetainedRange.Last))
                    { editorImages[i].Source = null; editorRenderWidths[i] = 0; }
                var pixels = 6_000_000d / (last - first + 1);
                var scale = viewport.ZoomFactor; var images = editorImages;
                for (var i = first; i <= last; i++)
                {
                    var ratio = pageSizes[i].Height / pageSizes[i].Width;
                    var requested = pageSizes[i].Width * scale * (View.XamlRoot?.RasterizationScale ?? 1);
                    var width = (uint)Math.Max(1, Math.Min(Math.Min(Math.Max(320, requested), 3200 / Math.Max(1, ratio)), Math.Sqrt(pixels / ratio)));
                    if (editorImages[i].Source is not null && editorRenderWidths[i] == width) continue;
                    using var pdfPage = document!.GetPage((uint)i); using var stream = new InMemoryRandomAccessStream();
                    await pdfPage.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = width });
                    var bitmap = new BitmapImage(); stream.Seek(0); await bitmap.SetSourceAsync(stream);
                    if (!Visible || editorZoomIntermediate || pdf != document || images != editorImages || Math.Abs(scale - viewport.ZoomFactor) > .001) return;
                    editorImages[i].Source = bitmap; editorRenderWidths[i] = width;
                }
                for (var i = 0; i < editorImages.Length; i++) if (i < first || i > last) { editorImages[i].Source = null; editorRenderWidths[i] = 0; }
                editorRetainedRange = (first, last);
            }
            catch (Exception error) { LocalDiagnostics.Record("editor-scroll", error); notice.Text = "Halaman belum dapat dirender. Draf tetap tersedia."; }
            finally { scrollingEditor = false; if (editorScrollPending) QueueEditorScroll(); }
        }
    }
}
