using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Pertamina.Oto.Desktop.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(callback =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            _ = new DesktopApplication();
        });
    }
}

public sealed partial class DesktopApplication : Application
{
    private Window? window;

    internal DesktopApplication()
    {
        RequestedTheme = ApplicationTheme.Light;
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        window.Activate();
    }
}
