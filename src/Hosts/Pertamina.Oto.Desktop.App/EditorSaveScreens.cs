using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private ScrollViewer? saveScreen;
        private StackPanel ShowSaveScreen()
        {
            HideSaveScreen();
            foreach (var child in View.Children) child.Visibility = Visibility.Collapsed;
            var content = new StackPanel { Spacing = 20, Padding = new Thickness(16) };
            saveScreen = new ScrollViewer { Content = content, Background = Brush(245, 247, 251), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            Grid.SetRowSpan(saveScreen, 5); View.Children.Add(saveScreen);
            return content;
        }
        private void HideSaveScreen()
        {
            if (saveScreen is null) return;
            View.Children.Remove(saveScreen); saveScreen = null;
            foreach (var child in View.Children) child.Visibility = Visibility.Visible;
            resultButton.Visibility = lastResult is null ? Visibility.Collapsed : Visibility.Visible;
        }
        private void ShowSaving(CancellationTokenSource cancellation)
        {
            var content = ShowSaveScreen(); content.HorizontalAlignment = HorizontalAlignment.Center;
            content.Margin = new Thickness(0, 70, 0, 40); content.MaxWidth = 650;
            content.Children.Add(HistoryText("Menyimpan PDF…", 28, true));
            content.Children.Add(new ProgressRing { IsActive = true, Width = 56, Height = 56, HorizontalAlignment = HorizontalAlignment.Center });
            content.Children.Add(HistoryText("Menerapkan perubahan dan membuat file baru di perangkat ini. File asli tetap aman.", 14, muted: true));
            var cancel = HistoryButton("x", "Batalkan", true); cancel.HorizontalAlignment = HorizontalAlignment.Center;
            cancel.Click += (_, _) => { cancellation.Cancel(); cancel.IsEnabled = false; };
            content.Children.Add(cancel); cancel.Focus(FocusState.Programmatic);
        }
        private void ShowSaved(string target, string detail)
        {
            var content = ShowSaveScreen();
            owner.ShowUnifiedResult([target], detail, () => { HideSaveScreen(); Refresh(); viewport.Focus(FocusState.Programmatic); }, content,
                async () => { HideSaveScreen(); await CloseAsync(); });
        }
    }
}
