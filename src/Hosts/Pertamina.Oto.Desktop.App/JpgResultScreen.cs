using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Pertamina.Oto.LocalPdf;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private UIElement[] jpgPreviousContent = [];
    private StackPanel? jpgProgressPanel;

    private void ShowJpgProgress()
    {
        jpgPreviousContent = body.Children.ToArray();
        foreach (var item in jpgPreviousContent) item.Visibility = Visibility.Collapsed;
        jpgProgressPanel = new StackPanel { Spacing = 24, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 70, 0, 40), MaxWidth = 650 };
        jpgProgressPanel.Children.Add(new TextBlock { Text = "Mengubah PDF ke JPG…", FontSize = 28, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });
        jpgProgressPanel.Children.Add(new ProgressRing { IsActive = true, Width = 56, Height = 56 });
        var detail = Copy("", 14); detail.TextAlignment = TextAlignment.Center;
        detail.SetBinding(TextBlock.TextProperty, new Binding { Source = status, Path = new PropertyPath("Text"), Mode = BindingMode.OneWay });
        jpgProgressPanel.Children.Add(detail);
        var stop = new Button { Content = "Batalkan", HorizontalAlignment = HorizontalAlignment.Center };
        stop.Click += (_, _) => { active?.Cancel(); stop.IsEnabled = false; stop.Content = "Membatalkan…"; };
        jpgProgressPanel.Children.Add(stop); body.Children.Add(jpgProgressPanel);
    }

    private void FinishJpgScreen(PdfReply? reply)
    {
        if (jpgProgressPanel is not null) body.Children.Remove(jpgProgressPanel);
        jpgProgressPanel = null;
        foreach (var item in jpgPreviousContent) item.Visibility = Visibility.Visible;
        jpgPreviousContent = [];
        if (reply is not { Success: true } || resultPath is null) return;
        var paths = reply.Pages > 1 ? Directory.GetFiles(Path.GetDirectoryName(resultPath)!, "*.jpg").OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray() : new[] { resultPath };
        ShowCurrentResult(paths, $"{reply.Pages} gambar · {reply.Bytes / 1024d:N1} KB");
    }
}
