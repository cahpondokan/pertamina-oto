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
    private sealed class AppExitDialogView
    {
        internal required ContentDialog Dialog;
        internal required Button Exit;
        internal required Button Return;
        internal ContentDialogResult Result;
    }

    // This view only returns intent. ConfirmCloseAsync retains all authorization and busy guards.
    private async Task<ContentDialogResult> ShowAppExitConfirmationAsync()
    {
        var view = BuildAppExitConfirmation();
        await view.Dialog.ShowAsync();
        return view.Result;
    }

    private AppExitDialogView BuildAppExitConfirmation()
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
        AutomationProperties.SetName(dialog, "Keluar dari OTO?");
        dialog.Resources["ContentDialogBackground"] = background;
        dialog.Resources["ContentDialogCornerRadius"] = new CornerRadius(14);
        var content = new StackPanel { Spacing = 20 };
        var header = new Grid { ColumnSpacing = 20, RowSpacing = 12 };
        header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        header.RowDefinitions.Add(new() { Height = GridLength.Auto });
        header.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var biblo = new Image { Name = "AppExitBiblo", Width = 100, Height = 100, Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left,
            Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "BibloAppExit.png"))) };
        AutomationProperties.SetAccessibilityView(biblo, AccessibilityView.Raw);
        header.Children.Add(biblo);
        var words = new StackPanel { Spacing = 10 };
        words.Children.Add(Copy("Keluar dari OTO?", 28, true));
        words.Children.Add(Copy("Draf dan pilihan dokumen tidak disimpan otomatis.", 16));
        words.Children.Add(Copy("Simpan hasil yang diperlukan sebelum keluar.", 16));
        var reassurance = Copy("File asli dan hasil tersimpan tetap aman.", 15);
        reassurance.Foreground = highContrast ? foreground : Brush(18, 104, 49);
        words.Children.Add(new Border { Child = reassurance, Padding = new Thickness(12), CornerRadius = new CornerRadius(8),
            Background = highContrast ? background : Brush(232, 250, 237) });
        Grid.SetColumn(words, 1); header.Children.Add(words); content.Children.Add(header);
        Button Action(string name, string label) => new()
        {
            Name = name, Content = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
            MinHeight = 52, MinWidth = 44, Padding = new Thickness(16, 12, 16, 12), CornerRadius = new CornerRadius(8),
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center,
            UseSystemFocusVisuals = true, IsTabStop = true
        };
        var exit = Action("AppExitConfirm", "Keluar dari OTO");
        var back = Action("AppExitReturn", "Kembali ke aplikasi");
        var red = new ResourceDictionary();
        red["ButtonBackground"] = Brush(255, 241, 242);
        red["ButtonBorderBrush"] = Brush(245, 182, 189);
        red["ButtonForeground"] = Brush(180, 35, 53);
        red["ButtonBackgroundPointerOver"] = Brush(255, 228, 231);
        red["ButtonBorderBrushPointerOver"] = Brush(210, 93, 108);
        red["ButtonForegroundPointerOver"] = Brush(150, 24, 42);
        red["ButtonBackgroundPressed"] = Brush(254, 205, 211);
        red["ButtonBorderBrushPressed"] = Brush(180, 35, 53);
        red["ButtonForegroundPressed"] = Brush(127, 29, 45);
        red["ButtonBackgroundDisabled"] = Brush(245, 245, 245);
        red["ButtonBorderBrushDisabled"] = Brush(210, 210, 210);
        red["ButtonForegroundDisabled"] = Brush(120, 120, 120);
        exit.Resources.ThemeDictionaries["Default"] = red;
        exit.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
        back.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
        AutomationProperties.SetName(exit, "Keluar dari OTO");
        AutomationProperties.SetName(back, "Kembali ke aplikasi");
        back.TabIndex = 0; exit.TabIndex = 1;
        var actions = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
        actions.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        actions.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        actions.RowDefinitions.Add(new() { Height = GridLength.Auto });
        actions.RowDefinitions.Add(new() { Height = GridLength.Auto });
        actions.Children.Add(exit); actions.Children.Add(back); content.Children.Add(actions);
        var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        dialog.Content = scroll;
        var view = new AppExitDialogView { Dialog = dialog, Exit = exit, Return = back };
        var closing = false;
        var heldRetainKeys = new HashSet<VirtualKey>();
        var retainRequested = false;
        void Finish(ContentDialogResult result)
        {
            if (closing) return;
            // A click while a safe-dismiss key is held must also wait for release.
            if (heldRetainKeys.Count > 0) { retainRequested = true; return; }
            closing = true; view.Result = result; dialog.Hide();
        }
        exit.Click += (_, _) => Finish(ContentDialogResult.Primary);
        back.Click += (_, _) => Finish(ContentDialogResult.None);
        dialog.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler((_, e) =>
        {
            // Keep the dialog focused through key-up so the restored control cannot activate.
            if (e.Key is VirtualKey.Enter or VirtualKey.Escape)
            { e.Handled = true; heldRetainKeys.Add(e.Key); retainRequested = true; }
        }), true);
        dialog.AddHandler(UIElement.PreviewKeyUpEvent, new KeyEventHandler((_, e) =>
        {
            if (e.Key is not (VirtualKey.Enter or VirtualKey.Escape)) return;
            e.Handled = true;
            heldRetainKeys.Remove(e.Key);
            if (!retainRequested || heldRetainKeys.Count > 0) return;
            // Close after the complete routed key event has been consumed by the modal.
            DispatcherQueue.TryEnqueue(() =>
            {
                if (heldRetainKeys.Count == 0) Finish(ContentDialogResult.None);
            });
        }), true);
        void Bound()
        {
            content.MaxWidth = Math.Max(80, Math.Min(720, root.ActualWidth - 96));
            scroll.MaxHeight = Math.Max(60, root.ActualHeight - 96);
            dialog.Resources["ContentDialogMinWidth"] = Math.Min(280, content.MaxWidth);
            dialog.Resources["ContentDialogMaxWidth"] = content.MaxWidth + 48;
            dialog.Resources["ContentDialogMaxHeight"] = Math.Max(100, root.ActualHeight - 32);
            var narrow = content.MaxWidth < 480;
            biblo.Width = biblo.Height = narrow ? 64 : 100;
            Grid.SetColumn(words, narrow ? 0 : 1); Grid.SetRow(words, narrow ? 1 : 0);
            Grid.SetColumnSpan(words, narrow ? 2 : 1);
            Grid.SetColumnSpan(exit, narrow ? 2 : 1); Grid.SetColumnSpan(back, narrow ? 2 : 1);
            Grid.SetColumn(back, narrow ? 0 : 1); Grid.SetRow(back, narrow ? 1 : 0);
        }
        void Resize(object sender, SizeChangedEventArgs e) => Bound();
        Bound();
        dialog.Opened += (_, _) => { root.SizeChanged += Resize; Bound(); back.Focus(FocusState.Programmatic); };
        dialog.Closed += (_, _) => root.SizeChanged -= Resize;
        return view;
    }
}
