using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static string CategoryName(string group) => group switch
    {
        "Organize PDF" => "Atur halaman", "Optimize PDF" => "Optimalkan",
        "Convert to PDF" => "Ubah ke PDF", "Convert from PDF" => "Ubah dari PDF",
        "PDF Security" => "Keamanan PDF", "PDF Intelligence" => "Asisten dokumen", _ => group
    };

    private static string CatalogName(ToolCard tool) => tool.Id switch
    {
        "remove" => "Hapus halaman", "extract" => "Ambil halaman", "scan" => "Scan ke PDF",
        "repair" => "Perbaiki PDF", "word-pdf" => "Word ke PDF", "ppt-pdf" => "PowerPoint ke PDF",
        "excel-pdf" => "Excel ke PDF", "html-pdf" => "HTML ke PDF", "pdf-jpg" => "PDF ke JPG",
        "pdf-word" => "PDF ke Word", "pdf-ppt" => "PDF ke PowerPoint", "pdf-excel" => "PDF ke Excel",
        "pdfa" => "PDF ke PDF/A", "numbers" => "Nomor halaman", "crop" => "Potong halaman",
        "edit" => "Edit isi PDF", "forms" => "Formulir PDF", "unlock" => "Buka kunci PDF",
        "protect" => "Kunci PDF", "sign" => "Tanda tangan", "redact" => "Redaksi permanen",
        "compare" => "Bandingkan PDF", "summary" => "Ringkas PDF", "translate" => "Terjemahkan PDF",
        "markdown" => "PDF ke Markdown", _ => DisplayTool(tool)
    };

    private Border BuildCategory(string group, ToolCard[] tools)
    {
        var color = group switch
        {
            "Organize PDF" => Brush(106, 55, 210), "Optimize PDF" => Brush(20, 125, 63),
            "Convert to PDF" => Brush(0, 94, 215), "Convert from PDF" => Brush(181, 29, 68),
            "Edit PDF" => Brush(162, 93, 0), "PDF Security" => Brush(185, 26, 67), _ => Brush(0, 117, 131)
        };
        var content = new StackPanel { Spacing = 4 };
        content.Children.Add(new Border { Padding = new Thickness(16), Background = Brush(242, 245, 251),
            Child = new TextBlock { Text = $"{CategoryName(group)} ({tools.Length})", FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = color, TextWrapping = TextWrapping.Wrap } });
        foreach (var tool in tools)
        {
            var row = new Grid { ColumnSpacing = 4, Margin = new Thickness(6, 0, 6, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var label = new Grid { ColumnSpacing = 10 };
            label.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            label.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            label.Children.Add(tool.Id == "merge" ? MergeBiblo(28) : new FontIcon { Glyph = tool.Glyph, FontSize = 22,
                FontFamily = new FontFamily("Segoe Fluent Icons"), Foreground = tool.Available ? color : Brush(103, 116, 135) });
            var text = new TextBlock { Text = CatalogName(tool), FontSize = 14, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(text, 1); label.Children.Add(text);
            var button = new Button { Content = label, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, MinHeight = 46, Padding = new Thickness(8),
                Background = tool.Available ? Brush(255, 255, 255) : Brush(243, 245, 248), BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(8), Foreground = tool.Available ? Brush(16, 35, 65) : Brush(94, 107, 128) };
            AutomationProperties.SetName(button, CatalogName(tool) + (tool.Available ? ", aktif fungsi dasar" : ", belum aktif; lihat penjelasan"));
            ToolTipService.SetToolTip(button, tool.Description);
            button.Click += (_, _) => OpenHomeTool(tool); row.Children.Add(button);
            FrameworkElement trailing;
            if (tool.Available)
            {
                var favorite = new ToggleButton { Content = favorites.Contains(tool.Id) ? "★" : "☆", IsChecked = favorites.Contains(tool.Id),
                    Background = Brush(255, 255, 255), BorderThickness = new Thickness(0), MinWidth = 32, VerticalAlignment = VerticalAlignment.Center };
                AutomationProperties.SetName(favorite, "Favorit " + CatalogName(tool));
                favorite.Click += (_, _) => { if (favorite.IsChecked == true) favorites.Add(tool.Id); else favorites.Remove(tool.Id); SaveLibrary(); favorite.Content = favorite.IsChecked == true ? "★" : "☆"; };
                trailing = favorite;
            }
            else trailing = new TextBlock { Text = "◷ Belum aktif", FontSize = 11, Foreground = Brush(94, 107, 128), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 8, 0) };
            Grid.SetColumn(trailing, 1); row.Children.Add(trailing); content.Children.Add(row);
        }
        return new Border { Child = content, Padding = new Thickness(0, 0, 0, 8), CornerRadius = new CornerRadius(12),
            Background = Brush(255, 255, 255), BorderBrush = Brush(222, 228, 238), BorderThickness = new Thickness(1) };
    }
}
