using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.UI.ViewManagement;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed class OrganizerTransferDialogView
    {
        internal required ContentDialog Dialog;
        internal required Button Replace;
        internal required Button Return;
        internal ContentDialogResult Result;
    }

    private async Task<ContentDialogResult> ShowOrganizerTransferConfirmationAsync()
    {
        var view = BuildOrganizerTransferConfirmation();
        await view.Dialog.ShowAsync();
        return view.Result;
    }

    private OrganizerTransferDialogView BuildOrganizerTransferConfirmation()
    {
        var highContrast = new AccessibilitySettings().HighContrast;
        var system = new UISettings();
        var foreground = highContrast ? new SolidColorBrush(system.GetColorValue(UIColorType.Foreground)) : Brush(15, 35, 72);
        var background = highContrast ? new SolidColorBrush(system.GetColorValue(UIColorType.Background)) : Brush(255, 255, 255);
        TextBlock Copy(string value, double size, bool bold = false) => new()
        {
            Text = value, FontSize = size, TextWrapping = TextWrapping.Wrap, Foreground = foreground,
            FontWeight = bold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
        };
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, RequestedTheme = ElementTheme.Light,
            DefaultButton = ContentDialogButton.None };
        AutomationProperties.SetName(dialog, "Ganti susunan halaman?");
        dialog.Resources["ContentDialogBackground"] = background;
        dialog.Resources["ContentDialogCornerRadius"] = new CornerRadius(14);
        var content = new StackPanel { Spacing = 18 };
        var header = new Grid { ColumnSpacing = 20 };
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        var words = new StackPanel { Spacing = 10 };
        words.Children.Add(Copy("Ganti susunan halaman?", 28, true));
        words.Children.Add(Copy("Dokumen Gabung PDF akan menggantikan susunan yang sedang terbuka di Atur halaman.", 16));
        header.Children.Add(words);
        var biblo = new Image { Name = "OrganizerTransferBiblo", Width = 112, Height = 112, Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", "031-atur-ulang-halaman-pdf.png"))) };
        AutomationProperties.SetAccessibilityView(biblo, AccessibilityView.Raw);
        Grid.SetColumn(biblo, 1); header.Children.Add(biblo); content.Children.Add(header);
        var warning = Copy("Susunan sebelumnya akan diganti. Simpan hasil yang diperlukan sebelum melanjutkan.", 15);
        warning.Foreground = highContrast ? foreground : Brush(133, 83, 15);
        content.Children.Add(new Border { Padding = new Thickness(14), CornerRadius = new CornerRadius(8),
            Background = highContrast ? background : Brush(255, 248, 231),
            BorderBrush = highContrast ? foreground : Brush(239, 211, 152), BorderThickness = new Thickness(1), Child = warning });
        var reassurance = Copy("File asli dan hasil yang sudah disimpan tetap aman.", 15);
        reassurance.Foreground = highContrast ? foreground : Brush(18, 104, 49);
        content.Children.Add(new Border { Padding = new Thickness(12), CornerRadius = new CornerRadius(8),
            Background = highContrast ? background : Brush(232, 250, 237), Child = reassurance });
        Button Action(string name, string label) => new()
        {
            Name = name, Content = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
            MinHeight = 52, MinWidth = 44, Padding = new Thickness(16, 12, 16, 12), CornerRadius = new CornerRadius(8),
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center,
            UseSystemFocusVisuals = true, IsTabStop = true
        };
        var replace = Action("OrganizerTransferReplace", "Gunakan dokumen ini");
        var back = Action("OrganizerTransferReturn", "Kembali ke Gabung PDF");
        var amber = new ResourceDictionary();
        amber["ButtonBackground"] = Brush(255, 248, 231);
        amber["ButtonBorderBrush"] = Brush(224, 180, 98);
        amber["ButtonForeground"] = Brush(133, 83, 15);
        amber["ButtonBackgroundPointerOver"] = Brush(255, 237, 194);
        amber["ButtonBorderBrushPointerOver"] = Brush(175, 120, 30);
        amber["ButtonForegroundPointerOver"] = Brush(113, 67, 10);
        amber["ButtonBackgroundPressed"] = Brush(251, 219, 153);
        amber["ButtonBorderBrushPressed"] = Brush(133, 83, 15);
        amber["ButtonForegroundPressed"] = Brush(94, 55, 8);
        replace.Resources.ThemeDictionaries["Default"] = amber;
        replace.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
        back.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
        AutomationProperties.SetName(replace, "Gunakan dokumen ini");
        AutomationProperties.SetHelpText(replace, "Ganti susunan Atur halaman dengan dokumen Gabung PDF saat ini.");
        AutomationProperties.SetName(back, "Kembali ke Gabung PDF");
        back.TabIndex = 0; replace.TabIndex = 1;
        var actions = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
        actions.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        actions.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        actions.RowDefinitions.Add(new() { Height = GridLength.Auto });
        actions.RowDefinitions.Add(new() { Height = GridLength.Auto });
        actions.Children.Add(replace); actions.Children.Add(back); content.Children.Add(actions);
        var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        dialog.Content = scroll;
        var view = new OrganizerTransferDialogView { Dialog = dialog, Replace = replace, Return = back };
        var closing = false;
        var heldReturnKeys = new HashSet<VirtualKey>();
        var returnRequested = false;
        void Finish(ContentDialogResult result)
        {
            if (closing) return;
            if (heldReturnKeys.Count > 0) { returnRequested = true; return; }
            closing = true; view.Result = result; dialog.Hide();
        }
        replace.Click += (_, _) => Finish(ContentDialogResult.Primary);
        back.Click += (_, _) => Finish(ContentDialogResult.None);
        // Keep focus in the modal until all safe-dismiss key events have been consumed.
        dialog.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler((_, e) =>
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Escape)
            { e.Handled = true; heldReturnKeys.Add(e.Key); returnRequested = true; }
        }), true);
        dialog.AddHandler(UIElement.PreviewKeyUpEvent, new KeyEventHandler((_, e) =>
        {
            if (e.Key is not (VirtualKey.Enter or VirtualKey.Escape)) return;
            e.Handled = true; heldReturnKeys.Remove(e.Key);
            if (!returnRequested || heldReturnKeys.Count > 0) return;
            DispatcherQueue.TryEnqueue(() => { if (heldReturnKeys.Count == 0) Finish(ContentDialogResult.None); });
        }), true);
        void Bound()
        {
            content.MaxWidth = Math.Max(80, Math.Min(680, root.ActualWidth - 96));
            scroll.MaxHeight = Math.Max(60, root.ActualHeight - 96);
            dialog.Resources["ContentDialogMinWidth"] = Math.Min(280, content.MaxWidth);
            dialog.Resources["ContentDialogMaxWidth"] = content.MaxWidth + 48;
            dialog.Resources["ContentDialogMaxHeight"] = Math.Max(100, root.ActualHeight - 32);
            var narrow = content.MaxWidth < 560;
            biblo.Width = biblo.Height = narrow ? 80 : 112;
            Grid.SetColumnSpan(replace, narrow ? 2 : 1); Grid.SetColumnSpan(back, narrow ? 2 : 1);
            Grid.SetColumn(back, narrow ? 0 : 1); Grid.SetRow(back, narrow ? 1 : 0);
        }
        void Resize(object sender, SizeChangedEventArgs e) => Bound();
        Bound();
        dialog.Opened += (_, _) => { root.SizeChanged += Resize; Bound(); back.Focus(FocusState.Programmatic); };
        dialog.Closed += (_, _) => root.SizeChanged -= Resize;
        return view;
    }
}
