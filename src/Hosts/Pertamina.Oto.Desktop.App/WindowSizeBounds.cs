namespace Pertamina.Oto.Desktop.App;

internal readonly record struct WindowMinimum(int Width, int Height, bool WorkAreaFallback);

internal static class WindowSizeBounds
{
    // Client DIP, excluding the native caption/border. See WINDOW_MINIMUM_SIZE.md.
    internal const int ClientWidth = 640;
    internal const int ClientHeight = 760;

    internal static WindowMinimum Calculate(double scale, int frameWidth, int frameHeight, int workWidth, int workHeight)
    {
        if (!double.IsFinite(scale) || scale <= 0 || frameWidth < 0 || frameHeight < 0 || workWidth <= 0 || workHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale));
        var width = checked((int)Math.Ceiling(ClientWidth * scale) + frameWidth);
        var height = checked((int)Math.Ceiling(ClientHeight * scale) + frameHeight);
        return new(Math.Min(width, workWidth), Math.Min(height, workHeight), width > workWidth || height > workHeight);
    }
}
