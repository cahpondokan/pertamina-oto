using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private void BuildMergeCompactHeader(ToggleButton favorite)
    {
        // Heading has already applied navigation/history state. Replace its two text rows only.
        body.Children.RemoveAt(1);
        body.Children.RemoveAt(0);
        var header = new Grid { ColumnSpacing = 16 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock { Text = "Gabung PDF", FontSize = 28,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        title.Children.Add(Copy("Tambahkan dokumen, atur urutan, lalu gabungkan.", 14));
        header.Children.Add(title);
        favorite.VerticalAlignment = VerticalAlignment.Center;
        favorite.MinHeight = 40;
        Grid.SetColumn(favorite, 1); header.Children.Add(favorite);
        body.Children.Add(header);
        var summary = new Grid { ColumnSpacing = 16, HorizontalAlignment = HorizontalAlignment.Stretch };
        summary.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        summary.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        summary.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var info = new FontIcon { Glyph = "\uE946", FontSize = 18, VerticalAlignment = VerticalAlignment.Center };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAccessibilityView(info, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        summary.Children.Add(info);
        var summaryText = new TextBlock { Text = "Versi pengembangan · Periksa hasil sebelum digunakan.",
            TextWrapping = TextWrapping.Wrap, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(summaryText, 1); summary.Children.Add(summaryText);
        var label = new TextBlock { Text = "Lihat batasan", FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(label, 2); summary.Children.Add(label);
        var details = new StackPanel { Spacing = 6 };
        details.Children.Add(Copy("Gunakan salinan dokumen uji. Tata letak, form, bookmark, tautan, dan tanda tangan digital belum dijamin tetap sama.", 13));
        details.Children.Add(Copy("Batas pemrosesan: 100 MB · 1000 halaman", 12));
        var limits = new Expander { Header = summary, Content = details, IsExpanded = false,
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Background = Brush(255, 248, 234), BorderBrush = Brush(248, 213, 146),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
        var colors = new ResourceDictionary();
        colors["ExpanderHeaderBackground"] = Brush(255, 248, 234);
        colors["ExpanderHeaderBorderBrush"] = Brush(248, 213, 146);
        colors["ExpanderHeaderBorderPointerOverBrush"] = Brush(229, 180, 100);
        colors["ExpanderHeaderBorderPressedBrush"] = Brush(210, 155, 70);
        colors["ExpanderContentBackground"] = Brush(255, 252, 245);
        limits.Resources.ThemeDictionaries["Default"] = colors;
        limits.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(limits,
            "Versi pengembangan · Periksa hasil sebelum digunakan. Lihat batasan");
        body.Children.Add(limits);
    }
}
