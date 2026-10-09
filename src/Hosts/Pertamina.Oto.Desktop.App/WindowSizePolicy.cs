using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Pertamina.Oto.Desktop.App;

// Native presenter constraints handle sizing; no ordinary-resize correction loop.
internal sealed partial class WindowSizePolicy
{
    private readonly MainWindow window;
    private readonly FrameworkElement shell;
    private readonly DisplayAreaWatcher displays;
    private (uint Dpi, RectInt32 Work)? environment;
    internal WindowMinimum Minimum { get; private set; }

    internal WindowSizePolicy(MainWindow window, FrameworkElement shell)
    {
        this.window = window; this.shell = shell;
        displays = DisplayArea.CreateWatcher();
        displays.Updated += (_, _) => Refresh();
        displays.Added += (_, _) => Refresh();
        displays.Removed += (_, _) => Refresh();
        window.AppWindow.Changed += (_, _) => Refresh();
        shell.Loaded += (_, _) =>
        {
            if (shell.XamlRoot is not null) shell.XamlRoot.Changed += (_, _) => Refresh();
            Refresh();
        };
        window.Closed += (_, _) => displays.Stop();
        Refresh(); displays.Start();
    }

    private void Refresh()
    {
        if (window.AppWindow.Presenter is not OverlappedPresenter presenter) return;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var dpi = GetDpiForWindow(hwnd);
        if (dpi == 0) return;
        var work = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
        var next = (dpi, work);
        if (environment is { } current && current.Dpi == dpi && current.Work.Equals(work)) return;
        var previous = environment;
        // Remove transient maximize/minimize style bits when measuring restored chrome.
        var style = unchecked((uint)GetWindowLongPtr(hwnd, -16).ToInt64()) & ~0x21000000u;
        var extended = unchecked((uint)GetWindowLongPtr(hwnd, -20).ToInt64());
        var frame = new NativeRect();
        if (!AdjustWindowRectExForDpi(ref frame, style, false, extended, dpi)) return;
        Minimum = WindowSizeBounds.Calculate(dpi / 96d, frame.Right - frame.Left, frame.Bottom - frame.Top, work.Width, work.Height);
        // Cache before setting presenter properties: their change event can be synchronous.
        environment = next;
        presenter.PreferredMinimumWidth = Minimum.Width;
        presenter.PreferredMinimumHeight = Minimum.Height;
        if (previous is null || presenter.State != OverlappedPresenterState.Restored) return;
        // Once on an actual environment change, fit the restored window to this work area.
        var size = window.AppWindow.Size; var position = window.AppWindow.Position;
        var width = Math.Clamp(size.Width, Minimum.Width, work.Width);
        var height = Math.Clamp(size.Height, Minimum.Height, work.Height);
        var x = Math.Clamp(position.X, work.X, work.X + work.Width - width);
        var y = Math.Clamp(position.Y, work.Y, work.Y + work.Height - height);
        if (width != size.Width || height != size.Height || x != position.X || y != position.Y)
            window.AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll")] private static partial uint GetDpiForWindow(nint hwnd);
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static partial nint GetWindowLongPtr(nint hwnd, int index);
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AdjustWindowRectExForDpi(ref NativeRect rect, uint style, [MarshalAs(UnmanagedType.Bool)] bool menu, uint extended, uint dpi);
}
