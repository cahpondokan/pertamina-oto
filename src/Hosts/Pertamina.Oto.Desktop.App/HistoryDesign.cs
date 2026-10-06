using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Action? disposeHistoryReader;
    private int historyNavigationGeneration;

    private void SetHistoryPageVisible(bool visible)
    {
        if (!visible)
        {
            pdfReader?.Deactivate();
            historyNavigationGeneration++;
            disposeHistoryReader?.Invoke(); disposeHistoryReader = null;
        }
        historyPage.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (visible)
            foreach (var item in navigation) item.Background = AutomationProperties.GetName(item) == "Riwayat" ? Brush(23, 104, 212) : Brush(7, 29, 61);
        if (standardPageScroll is not null) standardPageScroll.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
        if (standardPageHeader is not null) standardPageHeader.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private static TextBlock HistoryText(string text, double size = 13, bool strong = false, bool muted = false) => new()
    {
        Text = text, FontSize = size, FontWeight = strong ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal,
        Foreground = muted ? Brush(82, 100, 123) : Brush(23, 43, 70), TextWrapping = TextWrapping.Wrap,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static Image HistoryIcon(string name, double size = 18) => new()
    {
        Source = new SvgImageSource(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "History", name + ".svg"))),
        Width = size, Height = size, IsHitTestVisible = false
    };

    private static Button HistoryButton(string icon, string label, bool text = false, bool primary = false)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(HistoryIcon(primary && icon == "plus" ? "plus-white" : icon));
        if (text) content.Children.Add(new TextBlock { Text = label, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content, MinWidth = 36, MinHeight = 36, Padding = new Thickness(text ? 12 : 8, 8, text ? 12 : 8, 8),
            Background = primary ? Brush(8, 102, 232) : Brush(255, 255, 255),
            Foreground = primary ? Brush(255, 255, 255) : Brush(23, 43, 70),
            BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(6), VerticalAlignment = VerticalAlignment.Center
        };
        AutomationProperties.SetName(button, label); ToolTipService.SetToolTip(button, label); return button;
    }

    private static Grid HistoryColumns()
    {
        var grid = new Grid { ColumnSpacing = 12 };
        foreach (var width in new[] { new GridLength(1, GridUnitType.Star), new GridLength(70), new GridLength(85), new GridLength(130), new GridLength(36) })
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        return grid;
    }

    private static string HistoryFileSize(long length) => length >= 1024 * 1024
        ? (length / (1024d * 1024)).ToString("0.#", System.Globalization.CultureInfo.GetCultureInfo("id-ID")) + " MB"
        : (length / 1024d).ToString("0.#", System.Globalization.CultureInfo.GetCultureInfo("id-ID")) + " KB";

    private async Task OpenHistoryFolderAsync(string path)
    {
        try { await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)!)); }
        catch (Exception error) { LocalDiagnostics.Record("history-folder", error); SetHistoryNotice("Folder tidak ditemukan. File mungkin sudah dipindahkan."); }
    }

    private void SetHistoryNotice(string text)
    {
        if (historyNotice is null) return;
        historyNotice.Text = text; historyNotice.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
    }
}
