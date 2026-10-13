using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Owned by the empty view, never by the cleared document session.
    private sealed record MergeClosedNoticeState(string SavedPath, Button Open, Button Folder,
        TextBlock Status, Func<Task> OpenAsync, Action Reveal);

    private void ShowMergeClosedSavedNotice(string savedPath)
    {
        if (emptyTool?.Id != "merge" || emptyPanel is null || !body.Children.Contains(emptyPanel)) return;
        // The caller has validated and saved this result before clearing the session.
        var immutablePath = Path.GetFullPath(savedPath);
        var highContrast = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast;
        var colors = new Windows.UI.ViewManagement.UISettings();
        var foreground = highContrast
            ? new SolidColorBrush(colors.UIElementColor(Windows.UI.ViewManagement.UIElementType.WindowText))
            : Brush(16, 77, 51);
        var text = new StackPanel { Spacing = 3 };
        var title = Copy("Hasil gabungan berhasil disimpan", 16);
        title.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; title.Foreground = foreground;
        AutomationProperties.SetLiveSetting(title, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        var titleRow = new Grid { ColumnSpacing = 8 };
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition());
        var check = new FontIcon { Glyph = "\uE73E", FontSize = 20, Foreground = foreground };
        AutomationProperties.SetAccessibilityView(check, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        titleRow.Children.Add(check); Grid.SetColumn(title, 1); titleRow.Children.Add(title);
        text.Children.Add(titleRow);
        var name = Copy(Path.GetFileName(immutablePath), 13); name.Foreground = foreground;
        name.MaxLines = 1; name.TextTrimming = TextTrimming.CharacterEllipsis;
        ToolTipService.SetToolTip(name, name.Text); text.Children.Add(name);
        var directory = Copy(Path.GetDirectoryName(immutablePath) ?? "", 12); directory.Foreground = foreground;
        directory.MaxLines = 1; directory.TextTrimming = TextTrimming.CharacterEllipsis;
        ToolTipService.SetToolTip(directory, directory.Text); text.Children.Add(directory);
        var statusLine = Copy("", 13); statusLine.Foreground = foreground; statusLine.Visibility = Visibility.Collapsed;
        AutomationProperties.SetLiveSetting(statusLine, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        void Failure(string message) { statusLine.Text = message; statusLine.Visibility = Visibility.Visible; }
        var content = new StackPanel { Spacing = 6 };
        var header = new Grid { ColumnSpacing = 12, RowSpacing = 8 };
        header.ColumnDefinitions.Add(new ColumnDefinition());
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(text);
        var actions = new ResponsiveWrapPanel { Spacing = 8 };
        var open = new Button { Content = "Buka hasil", MinWidth = 104, MinHeight = 44 };
        var folderButton = new Button { Content = "Buka folder", MinWidth = 104, MinHeight = 44 };
        AutomationProperties.SetName(open, "Buka hasil"); AutomationProperties.SetName(folderButton, "Buka folder");
        actions.Children.Add(open); actions.Children.Add(folderButton);
        Grid.SetColumn(actions, 1); header.Children.Add(actions);
        ResponsiveLayout.Header(header, text, actions, 540);
        content.Children.Add(header); content.Children.Add(statusLine);
        var banner = new Border { Child = content, MaxWidth = 760, Padding = new Thickness(14, 10, 14, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch, CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(1),
            Background = highContrast ? new SolidColorBrush(colors.UIElementColor(Windows.UI.ViewManagement.UIElementType.Window)) : Brush(234, 249, 240),
            BorderBrush = highContrast ? foreground : Brush(135, 209, 166) };
        bool Available()
        {
            if (!body.Children.Contains(banner) || !MergeEmptyActive || active is not null || importing || messageOpen) return false;
            if (File.Exists(immutablePath)) { statusLine.Text = ""; statusLine.Visibility = Visibility.Collapsed; return true; }
            Failure("File hasil tidak ditemukan. File mungkin telah dipindahkan atau dihapus."); return false;
        }
        var opening = false;
        async Task Open()
        {
            if (opening || !Available()) return;
            opening = true;
            try { await ShowReadPreviewDialogAsync(immutablePath); }
            catch (Exception) { Failure("Hasil tidak dapat dibuka. Periksa lokasi file."); }
            finally { opening = false; }
        }
        void Reveal()
        {
            if (!Available()) return;
            try { ResultFileReveal.Select(immutablePath); }
            catch (Exception) { Failure("Folder hasil tidak dapat dibuka. Periksa lokasi file."); }
        }
        banner.Tag = new MergeClosedNoticeState(immutablePath, open, folderButton, statusLine, Open, Reveal);
        open.Click += async (_, _) => await Open(); folderButton.Click += (_, _) => Reveal();
        foreach (var previous in body.Children.OfType<Border>().Where(b => b.Tag is MergeClosedNoticeState).ToArray()) body.Children.Remove(previous);
        body.Children.Insert(body.Children.IndexOf(emptyPanel), banner);
        // Keep the existing input focus, shortcuts and drop surface intact.
    }
}
