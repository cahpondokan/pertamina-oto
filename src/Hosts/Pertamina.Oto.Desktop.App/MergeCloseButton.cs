using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Border BuildMergeDocumentSection(StackPanel inputArea)
    {
        var card = Section("Dokumen", "Pilih salinan uji. File asli tidak ditimpa.", inputArea);
        var content = (StackPanel)card.Child;
        var title = (TextBlock)content.Children[0];
        content.Children.RemoveAt(0);
        var heading = new Grid { ColumnSpacing = 12 };
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        title.VerticalAlignment = VerticalAlignment.Center;
        heading.Children.Add(title);
        var close = DocumentCloseButton();
        close.Name = "MergeDocumentCloseButton";
        StyleMergeDocumentCloseButton(close);
        close.HorizontalAlignment = HorizontalAlignment.Right;
        close.VerticalAlignment = VerticalAlignment.Top;
        close.HorizontalContentAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(close, 1); heading.Children.Add(close);
        // Keep the existing execution gate responsible for busy enablement.
        editable.Add(close);
        content.Children.Insert(0, heading);
        return card;
    }

    // Shared visual treatment only: callers retain their existing close action and state.
    private static void StyleMergeDocumentCloseButton(Button close)
    {
        close.Content = new TextBlock { Text = "×  Tutup dokumen", TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center };
        close.MinHeight = 44; close.MinWidth = 44;
        close.Padding = new Thickness(14, 8, 14, 8);
        close.CornerRadius = new CornerRadius(8);
        close.IsTabStop = true; close.UseSystemFocusVisuals = true;
        AutomationProperties.SetName(close, "Tutup dokumen");
        // Default-theme overrides leave the native HighContrast theme resources intact.
        var colors = new ResourceDictionary();
        colors["ButtonBackground"] = Brush(255, 241, 242);
        colors["ButtonBorderBrush"] = Brush(245, 182, 189);
        colors["ButtonForeground"] = Brush(180, 35, 53);
        colors["ButtonBackgroundPointerOver"] = Brush(255, 228, 231);
        colors["ButtonBorderBrushPointerOver"] = Brush(210, 93, 108);
        colors["ButtonForegroundPointerOver"] = Brush(150, 24, 42);
        colors["ButtonBackgroundPressed"] = Brush(254, 205, 211);
        colors["ButtonBorderBrushPressed"] = Brush(180, 35, 53);
        colors["ButtonForegroundPressed"] = Brush(127, 29, 45);
        colors["ButtonBackgroundDisabled"] = Brush(245, 245, 245);
        colors["ButtonBorderBrushDisabled"] = Brush(210, 210, 210);
        colors["ButtonForegroundDisabled"] = Brush(120, 120, 120);
        close.Resources.ThemeDictionaries["Default"] = colors;
        close.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
    }
}
