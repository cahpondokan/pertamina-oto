using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed class PreviewRaster
    {
        internal string Path = "", Password = "";
        internal uint Page, Width;
        internal int Generation;
        internal bool Refreshing;
    }
    private static readonly ConditionalWeakTable<Image, PreviewRaster> previewRasters = new();

    private static async Task RefreshVisiblePreviewRastersAsync(ScrollViewer viewport)
    {
        if (!viewport.IsLoaded || viewport.Content is not DependencyObject content) return;
        var scale = viewport.ZoomFactor;
        var images = Descendants(content).OfType<Image>().Where(i => i.IsLoaded && previewRasters.TryGetValue(i, out _))
            .Where(i => { var point = i.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()); return point.Y + i.ActualHeight * viewport.ZoomFactor >= 0 && point.Y < viewport.ActualHeight && point.X + i.ActualWidth * viewport.ZoomFactor >= 0 && point.X < viewport.ActualWidth; }).Take(24).ToArray();
        var pixels = 8_000_000d / Math.Max(1, images.Length);
        foreach (var image in images)
        {
            if (!viewport.IsLoaded || !image.IsLoaded) return;
            var state = previewRasters.GetValue(image, _ => new());
            var ratio = image.Source is BitmapImage bitmap && bitmap.PixelWidth > 0 ? bitmap.PixelHeight / (double)bitmap.PixelWidth : 1.414;
            var width = (uint)Math.Max(1, Math.Min(Math.Min(2400, Math.Max(240, image.ActualWidth * viewport.ZoomFactor * (image.XamlRoot?.RasterizationScale ?? 1))), Math.Sqrt(pixels / Math.Max(.05, ratio))));
            if (state.Refreshing || Math.Abs((double)state.Width - width) < 32) continue;
            state.Refreshing = true;
            try { await RenderPreviewAsync(state.Path, state.Page, image, Copy("", 12), width, state.Password,
                () => viewport.IsLoaded && Math.Abs(scale - viewport.ZoomFactor) < .001); }
            finally { state.Refreshing = false; }
        }
    }
}
