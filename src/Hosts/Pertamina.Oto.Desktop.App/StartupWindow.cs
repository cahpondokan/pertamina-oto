using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool startupWindowPrepared;
    private WindowSizePolicy? windowSizePolicy;

    internal void PrepareStartupWindow()
    {
        // Only the first launch prepares the window, before Activate shows it.
        // No activation, navigation, resize or focus handler reapplies this state.
        if (startupWindowPrepared) return;
        startupWindowPrepared = true;
        windowSizePolicy = new WindowSizePolicy(this, (Microsoft.UI.Xaml.FrameworkElement)Content);
        var work = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
        var minimum = windowSizePolicy.Minimum;
        // Center the effective rectangle, including emergency work-area caps.
        var width = Math.Min(work.Width, Math.Max(minimum.Width, Math.Min(1480, (int)(work.Width * 0.9))));
        var height = Math.Min(work.Height, Math.Max(minimum.Height, Math.Min(960, (int)(work.Height * 0.9))));
        AppWindow.MoveAndResize(new RectInt32(work.X + (work.Width - width) / 2,
            work.Y + (work.Height - height) / 2, width, height));
        // Keep the default overlapped presenter: native caption, taskbar, and
        // minimize/restore/resize remain available. Windows chooses maximized bounds.
        if (AppWindow.Presenter is OverlappedPresenter presenter) presenter.Maximize();
    }
}
