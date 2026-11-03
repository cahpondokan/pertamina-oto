using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private void FitSavedResultViewport(Border card, StackPanel original, Grid preview, ResponsiveWrapPanel secondary,
        StackPanel details, TextBlock notice, TextBlock footer, bool isMergeResult = false, bool isOrganizerResult = false)
    {
        var heading = new StackPanel { Spacing = 6 };
        var title = original.Children[0]; var subtitle = original.Children[1];
        var expander = original.Children.OfType<Expander>().Single(); expander.Content = null;
        original.Children.Clear(); heading.Children.Add(title); heading.Children.Add(subtitle);
        if (title is TextBlock caption)
        {
            caption.Text = "PDF berhasil disimpan"; caption.FontSize = 26;
            caption.Foreground = Brush(16, 40, 77); caption.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
        }
        var success = new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(24), Background = Brush(0, 139, 94),
            Child = new TextBlock { Text = "✓", FontSize = 28, Foreground = Brush(255, 255, 255), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
        FrameworkElement resultIcon = success;
        if (isMergeResult || isOrganizerResult)
        {
            var biblo = isOrganizerResult ? OrganizerBiblo(80) : MergeBiblo(80);
            biblo.Name = isOrganizerResult ? "SavedOrganizerBiblo" : "SavedMergeBiblo";
            AutomationProperties.SetName(biblo, isOrganizerResult ? "Biblo Atur halaman PDF" : "Biblo Gabungkan PDF");
            if (isOrganizerResult && subtitle is TextBlock resultSubtitle) resultSubtitle.Text = "Susunan halaman tersimpan sebagai PDF baru.";
            var badge = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(12),
                Background = Brush(0, 139, 94), BorderBrush = Brush(255, 255, 255), BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom,
                Child = new TextBlock { Text = "✓", FontSize = 15, Foreground = Brush(255, 255, 255),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
            AutomationProperties.SetName(badge, "PDF berhasil disimpan");
            resultIcon = new Grid { Width = 80, Height = 80, Children = { biblo, badge } };
        }
        heading.VerticalAlignment = VerticalAlignment.Center;
        var titleGroup = new Grid { ColumnSpacing = 16 };
        titleGroup.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); titleGroup.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        titleGroup.Children.Add(resultIcon); Grid.SetColumn(heading, 1); titleGroup.Children.Add(heading);
        secondary.Spacing = 8;
        var summary = details.Children.OfType<TextBlock>().Single();
        var separator = summary.Text.LastIndexOf('\n');
        var fullPath = separator >= 0 ? summary.Text[(separator + 1)..] : summary.Text;
        summary.Text = fullPath; summary.FontSize = 14; summary.MaxLines = 3;
        summary.TextTrimming = TextTrimming.CharacterEllipsis; ToolTipService.SetToolTip(summary, fullPath);
        AutomationProperties.SetName(summary, "Lokasi lengkap file: " + fullPath);
        var detailTitle = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = {
            new FontIcon { Glyph = "\uE946", FontSize = 16 }, HistoryText("Detail hasil", 16, true) } };
        details.Children.Insert(0, detailTitle);
        var pathLabel = HistoryText("Lokasi lengkap file", 13, muted: true); details.Children.Insert(1, pathLabel);
        var copyButton = details.Children.OfType<Button>().Single();
        copyButton.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = {
            new FontIcon { Glyph = "\uE8C8", FontSize = 16 }, new TextBlock { Text = "Salin ringkasan" } } };
        var detailCard = new Border { Child = details, Background = Brush(247, 250, 253), BorderBrush = Brush(220, 229, 241),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(16) };
        var info = preview.Children.OfType<StackPanel>().Single(); info.Children.Insert(info.Children.Count - 1, detailCard);
        preview.SizeChanged += (_, _) =>
        {
            var compact = preview.ActualWidth < 650;
            details.Spacing = compact ? 6 : 10; detailCard.Padding = new Thickness(compact ? 10 : 16);
            summary.FontSize = compact ? 12 : 14; summary.MaxLines = compact ? 2 : 3;
            pathLabel.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        };
        foreach (var command in secondary.Children.OfType<Button>())
        {
            command.MinHeight = 44;
            var label = AutomationProperties.GetName(command);
            if (label is "Tutup dokumen" or "Detail hasil") command.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = {
                new FontIcon { Glyph = label == "Tutup dokumen" ? "\uE711" : "\uE946", FontSize = 16 }, new TextBlock { Text = label } } };
            if ((isMergeResult || isOrganizerResult) && label == "Tutup dokumen") StyleMergeDocumentCloseButton(command);
        }
        var header = new Grid { ColumnSpacing = 16, RowSpacing = 12 };
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) }); header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        header.RowDefinitions.Add(new() { Height = GridLength.Auto }); header.RowDefinitions.Add(new() { Height = GridLength.Auto });
        header.Children.Add(titleGroup); Grid.SetColumn(secondary, 1); header.Children.Add(secondary);
        secondary.HorizontalAlignment = HorizontalAlignment.Right;
        header.SizeChanged += (_, _) =>
        {
            var compact = header.ActualWidth < 850;
            Grid.SetColumnSpan(titleGroup, compact ? 2 : 1); Grid.SetColumn(secondary, compact ? 0 : 1);
            Grid.SetRow(secondary, compact ? 1 : 0); Grid.SetColumnSpan(secondary, compact ? 2 : 1);
        };
        notice.MaxLines = 2; notice.TextTrimming = TextTrimming.CharacterEllipsis;
        void UpdateNotice() => notice.Visibility = string.IsNullOrEmpty(notice.Text) ? Visibility.Collapsed : Visibility.Visible;
        var noticeToken = notice.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) =>
        { UpdateNotice(); ToolTipService.SetToolTip(notice, notice.Text); });
        UpdateNotice();
        var fitted = new Grid { Padding = new Thickness(20), RowSpacing = 16 };
        fitted.RowDefinitions.Add(new() { Height = GridLength.Auto });
        fitted.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
        fitted.RowDefinitions.Add(new() { Height = GridLength.Auto });
        fitted.Children.Add(header); Grid.SetRow(preview, 1); fitted.Children.Add(preview);
        Grid.SetRow(notice, 2); fitted.Children.Add(notice);
        card.CornerRadius = new CornerRadius(14); footer.Text = "";
        card.Child = fitted; card.Margin = new Thickness(0);
        var scroll = standardPageScroll;
        if (scroll is null) return;
        void Resize()
        {
            if (!card.IsLoaded || scroll.ActualHeight <= 0) return;
            var height = scroll.ActualHeight - body.Padding.Top - body.Padding.Bottom - body.Spacing - Math.Max(16, footer.ActualHeight);
            card.Height = Math.Max(0, height);
        }
        void SizeChanged(object sender, SizeChangedEventArgs e) => Resize();
        card.Loaded += (_, _) => { Resize(); scroll.ChangeView(null, 0, null, true); };
        scroll.SizeChanged += SizeChanged; footer.SizeChanged += SizeChanged; body.SizeChanged += SizeChanged;
        card.Unloaded += (_, _) =>
        {
            scroll.SizeChanged -= SizeChanged; footer.SizeChanged -= SizeChanged; body.SizeChanged -= SizeChanged;
            notice.UnregisterPropertyChangedCallback(TextBlock.TextProperty, noticeToken);
        };
    }
}
