using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Border? mergeReadiness;
    private TextBlock? mergeReadyTitle;
    private TextBlock? mergeReadyDetail;
    private FontIcon? mergeReadyIcon;
    private Button? mergeCancelButton;

    private static StackPanel MergeFeatureLabel(string text, double size)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(MergeBiblo(size));
        row.Children.Add(new TextBlock { Text = text, FontSize = 14, TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center });
        return row;
    }

    private static StackPanel MergeLabel(string glyph, string text, bool safe = false)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(new FontIcon { Glyph = glyph, FontSize = 18, VerticalAlignment = VerticalAlignment.Center,
            Foreground = safe ? Brush(0, 126, 76) : Brush(0, 87, 208) });
        row.Children.Add(new TextBlock { Text = text, FontSize = safe ? 13 : 14, TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center });
        return row;
    }

    private Border MergeResultFrame(bool organizer = false)
    {
        var content = new StackPanel();
        var heading = new StackPanel { Spacing = organizer ? 0 : 8, Padding = new Thickness(organizer ? 10 : 20), Background = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 1), EndPoint = new Windows.Foundation.Point(1, 0),
            GradientStops = { new GradientStop { Color = Windows.UI.Color.FromArgb(255, 0, 72, 199), Offset = 0 },
                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 0, 113, 224), Offset = .85 },
                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 18, 182, 233), Offset = 1 } }
        } };
        var title = MergeFeatureLabel(organizer ? "Hasil susunan" : "Hasil gabungan", 40);
        if (organizer) { title.Children.RemoveAt(0); title.Children.Insert(0, OrganizerBiblo(40)); }
        title.Children.OfType<TextBlock>().Single().FontSize = 18;
        title.Children.OfType<TextBlock>().Single().FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
        foreach (var element in title.Children.OfType<Control>()) element.Foreground = Brush(255, 255, 255);
        title.Children.OfType<TextBlock>().Single().Foreground = Brush(255, 255, 255);
        var subtitle = new TextBlock { Text = "Atur nama dan lokasi file hasil.", FontSize = 13,
            TextWrapping = TextWrapping.Wrap, Foreground = Brush(255, 255, 255) };
        if (organizer)
        {
            var caption = title.Children.OfType<TextBlock>().Single(); title.Children.Remove(caption);
            title.Children.Add(new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center, Children = { caption, subtitle } });
        }
        heading.Children.Add(title);
        if (!organizer) heading.Children.Add(subtitle);
        content.Children.Add(heading);
        var form = new StackPanel { Padding = new Thickness(organizer ? 10 : 20) }; form.Children.Add(toolOptions);
        content.Children.Add(form);
        return new Border { Child = content, CornerRadius = new CornerRadius(14),
            Background = Brush(245, 249, 255), BorderBrush = Brush(172, 207, 252), BorderThickness = new Thickness(1) };
    }

    private StackPanel BuildMergeOutputLocation(bool compact = false)
    {
        var panel = new StackPanel { Spacing = compact ? 4 : 8 };
        panel.Children.Add(MergeLabel("\uE8B7", "Lokasi penyimpanan"));
        var name = new TextBlock { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = 1 };
        var path = new TextBlock { FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = 1 };
        var folderText = new StackPanel { Spacing = compact ? 0 : 4 }; folderText.Children.Add(name); folderText.Children.Add(path);
        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(new FontIcon { Glyph = "\uE8B7", FontSize = 24, Foreground = Brush(157, 96, 0) });
        Grid.SetColumn(folderText, 1); grid.Children.Add(folderText);
        var card = new Border { Child = grid, Padding = new Thickness(compact ? 8 : 12), Background = Brush(255, 247, 225),
            BorderBrush = Brush(245, 209, 133), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
        void Update()
        {
            name.Text = folder is null ? "Folder belum dipilih" : Path.GetFileName(Path.TrimEndingDirectorySeparator(folder));
            if (string.IsNullOrEmpty(name.Text)) name.Text = folder;
            path.Text = folder ?? "Pilih lokasi file hasil.";
            ToolTipService.SetToolTip(card, path.Text);
            AutomationProperties.SetName(card, "Lokasi penyimpanan: " + path.Text);
        }
        var token = location.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => Update());
        card.Unloaded += (_, _) => location.UnregisterPropertyChangedCallback(TextBlock.TextProperty, token);
        Update(); panel.Children.Add(card);
        var choose = ActionButton("Ubah folder", async () => await PickFolderAsync());
        choose.Content = MergeLabel("\uE8B7", "Ubah folder"); choose.MinHeight = compact ? 32 : 44;
        panel.Children.Add(choose);
        return panel;
    }

    private TextBox BuildMergeNameInput()
    {
        var input = new TextBox { Header = MergeLabel("\uE8A5", "Nama file hasil"), MinHeight = 44, MaxLength = outputName.MaxLength };
        input.SetBinding(TextBox.TextProperty, new Binding { Source = outputName, Path = new PropertyPath("Text"),
            Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        input.SetBinding(Control.IsEnabledProperty, new Binding { Source = outputName, Path = new PropertyPath("IsEnabled"), Mode = BindingMode.OneWay });
        input.TextChanged += (_, _) => UpdateMergePanelReadiness();
        AutomationProperties.SetName(input, "Nama file hasil");
        return input;
    }

    private Border BuildMergeReadiness()
    {
        mergeReadyTitle = new TextBlock { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
        mergeReadyDetail = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap };
        mergeReadyIcon = new FontIcon { FontSize = 22, VerticalAlignment = VerticalAlignment.Top };
        var text = new StackPanel { Spacing = 4 }; text.Children.Add(mergeReadyTitle); text.Children.Add(mergeReadyDetail);
        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(mergeReadyIcon); Grid.SetColumn(text, 1); grid.Children.Add(text);
        mergeReadiness = new Border { Child = grid, Padding = new Thickness(12), CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1) };
        var readiness = mergeReadiness;
        var token = status.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) =>
        {
            var expected = status.Text.StartsWith("Tambahkan satu PDF lagi", StringComparison.Ordinal)
                || status.Text.StartsWith("Siap digabungkan", StringComparison.Ordinal)
                || status.Text.StartsWith("Pilih lokasi hasil", StringComparison.Ordinal);
            readiness.Visibility = active is null && expected ? Visibility.Visible : Visibility.Collapsed;
        });
        readiness.Unloaded += (_, _) => status.UnregisterPropertyChangedCallback(TextBlock.TextProperty, token);
        return mergeReadiness;
    }

    private void UpdateMergePanelReadiness()
    {
        if (mergeReadiness is null || mergeReadyTitle is null || mergeReadyDetail is null || mergeReadyIcon is null) return;
        var name = outputName.Text.Trim();
        var validName = name.Length > 0 && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && name == Path.GetFileName(name) && name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        var ready = run.IsEnabled && validName;
        var expected = status.Text.StartsWith("Tambahkan satu PDF lagi", StringComparison.Ordinal)
            || status.Text.StartsWith("Siap digabungkan", StringComparison.Ordinal)
            || status.Text.StartsWith("Pilih lokasi hasil", StringComparison.Ordinal);
        mergeReadiness.Visibility = active is null && expected ? Visibility.Visible : Visibility.Collapsed;
        mergeReadiness.Background = ready ? Brush(231, 249, 239) : Brush(255, 247, 225);
        mergeReadiness.BorderBrush = ready ? Brush(158, 224, 187) : Brush(245, 209, 133);
        mergeReadyIcon.Glyph = ready ? "\uE73E" : "\uE946";
        mergeReadyIcon.Foreground = ready ? Brush(0, 126, 76) : Brush(157, 96, 0);
        mergeReadyTitle.Text = files.Count < 2 ? files.Count == 1 ? "Tambahkan satu PDF lagi." : "Tambahkan PDF untuk memulai."
            : folder is null ? "Pilih lokasi penyimpanan." : !validName ? "Periksa nama file hasil." : $"{files.Count} PDF siap digabungkan.";
        mergeReadyDetail.Text = files.Count < 2 ? "Minimal 2 PDF untuk digabungkan."
            : !validName ? "Nama file harus valid dan berakhiran .pdf." : "Urutan mengikuti daftar dokumen.";
    }

    private Button BuildMergeRunButton()
    {
        var action = new TextBlock { Text = "Gabungkan PDF", FontSize = 14,
            TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center };
        var button = new Button { Content = action, MinHeight = 48,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch, Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        button.SetBinding(Control.IsEnabledProperty, new Binding { Source = run, Path = new PropertyPath("IsEnabled"), Mode = BindingMode.OneWay });
        void UpdateLabel()
        {
            var highContrast = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast;
            action.Foreground = highContrast
                ? new SolidColorBrush(new Windows.UI.ViewManagement.UISettings().UIElementColor(Windows.UI.ViewManagement.UIElementType.HighlightText))
                : button.IsEnabled ? Brush(255, 255, 255) : Brush(113, 123, 139);
        }
        button.RegisterPropertyChangedCallback(Control.IsEnabledProperty, (_, _) => UpdateLabel());
        UpdateLabel();
        AutomationProperties.SetName(button, "Gabungkan PDF");
        button.Click += async (_, _) => await ExecuteAsync();
        return button;
    }

    private TextBlock BuildMergeStatus()
    {
        var text = new TextBlock { FontSize = 13, TextWrapping = TextWrapping.Wrap };
        text.SetBinding(TextBlock.TextProperty, new Binding { Source = status, Path = new PropertyPath("Text"), Mode = BindingMode.OneWay });
        void Update() => text.Visibility = status.Text.StartsWith("Tambahkan satu PDF lagi", StringComparison.Ordinal)
            || status.Text.StartsWith("Siap digabungkan", StringComparison.Ordinal)
            || status.Text.StartsWith("Pilih lokasi hasil", StringComparison.Ordinal) ? Visibility.Collapsed : Visibility.Visible;
        var token = status.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => Update());
        text.Unloaded += (_, _) => status.UnregisterPropertyChangedCallback(TextBlock.TextProperty, token);
        Update(); return text;
    }

    private HyperlinkButton BuildMergeOrganizerLink()
    {
        var link = new HyperlinkButton { Content = "Atur halaman satu per satu →", FontSize = 13, MinHeight = 44,
            HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(0, 8, 0, 8) };
        link.Click += async (_, _) => await OpenMergeOrganizerAsync();
        editable.Add(link); return link;
    }

    private Button BuildMergeCancelButton()
    {
        mergeCancelButton = new Button { Content = "Batalkan", MinHeight = 44, Visibility = Visibility.Collapsed };
        mergeCancelButton.SetBinding(Control.IsEnabledProperty, new Binding { Source = cancel, Path = new PropertyPath("IsEnabled"), Mode = BindingMode.OneWay });
        mergeCancelButton.Click += (_, _) => active?.Cancel();
        return mergeCancelButton;
    }
}
