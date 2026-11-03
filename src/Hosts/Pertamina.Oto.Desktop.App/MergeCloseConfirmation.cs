using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Windows.UI.ViewManagement;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // TASK2 baseline: both owned files absent; HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c.
    // Custom action results are returned only after ContentDialog.ShowAsync completes.
    private sealed class MergeCloseDialogView
    {
        internal required ContentDialog Dialog;
        internal required Button Save;
        internal required Button Discard;
        internal required Button Return;
        internal ContentDialogResult Result;
#if DEBUG
        internal Func<VirtualKey, bool>? OrganizerKeyDown;
        internal Func<VirtualKey, bool>? OrganizerKeyUp;
#endif
    }
#if DEBUG
    private Action<MergeCloseDialogView>? auditOrganizerCloseOpened;
#endif

    private async Task<ContentDialogResult> ShowMergeCloseConfirmationAsync(bool organizer = false)
    {
        var view = BuildMergeCloseConfirmation(organizer);
        await view.Dialog.ShowAsync();
        return view.Result;
    }

    private MergeCloseDialogView BuildMergeCloseConfirmation(bool organizer = false)
    {
        var highContrast = new AccessibilitySettings().HighContrast;
        var system = new UISettings();
        var foreground = highContrast ? new SolidColorBrush(system.GetColorValue(UIColorType.Foreground)) : Brush(15, 35, 72);
        var background = highContrast ? new SolidColorBrush(system.GetColorValue(UIColorType.Background)) : Brush(255, 255, 255);
        var blue = highContrast ? foreground : Brush(0, 89, 190);
        var red = highContrast ? foreground : Brush(133, 17, 34);
        TextBlock Text(string value, double size, bool bold = false) => new()
        {
            Text = value, FontSize = size, TextWrapping = TextWrapping.Wrap,
            Foreground = foreground, FontWeight = bold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
        };
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, RequestedTheme = ElementTheme.Light,
            DefaultButton = ContentDialogButton.None };
        AutomationProperties.SetName(dialog, "Tutup dokumen?");
        dialog.Resources["ContentDialogBackground"] = background;
        dialog.Resources["ContentDialogCornerRadius"] = new CornerRadius(14);
        var content = new StackPanel { Spacing = 16 };
        var header = new Grid { ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var words = new StackPanel { Spacing = 10 };
        words.Children.Add(Text("Tutup dokumen?", 28, true));
        words.Children.Add(Text(organizer ? "Susunan halaman belum disimpan." : "Hasil gabungan belum disimpan.", 19, true));
        words.Children.Add(Text("Proses dan simpan hasil sebelum menutup, atau tutup tanpa menyimpan.", 15));
        header.Children.Add(words);
        var biblo = organizer ? OrganizerBiblo(88) : MergeBiblo(88); biblo.VerticalAlignment = VerticalAlignment.Top;
        AutomationProperties.SetAccessibilityView(biblo, AccessibilityView.Raw);
        Grid.SetColumn(biblo, 1); header.Children.Add(biblo); content.Children.Add(header);

        Button Option(string name, string label, string detail, string glyph, SolidColorBrush color, SolidColorBrush fill)
        {
            var grid = new Grid { ColumnSpacing = 12 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var icon = new FontIcon { Glyph = glyph, FontSize = 28, Foreground = color };
            AutomationProperties.SetAccessibilityView(icon, AccessibilityView.Raw); grid.Children.Add(icon);
            var copy = new StackPanel { Spacing = 6 };
            var title = Text(label, 17, true); title.Foreground = color; copy.Children.Add(title); copy.Children.Add(Text(detail, 14));
            Grid.SetColumn(copy, 1); grid.Children.Add(copy);
            var button = new Button { Name = name, Content = grid, MinWidth = 44, MinHeight = 100,
                HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16), CornerRadius = new CornerRadius(8), Background = fill,
                Foreground = color, BorderBrush = color, BorderThickness = new Thickness(1.5) };
            AutomationProperties.SetName(button, label); AutomationProperties.SetHelpText(button, detail);
            return button;
        }
        var save = Option("MergeCloseSave", "Proses dan simpan", organizer ? "Simpan susunan sebagai PDF baru." : "Gabungkan PDF, lalu simpan hasilnya.", "\uE74E", blue,
            highContrast ? background : Brush(234, 244, 255));
        var discard = Option("MergeCloseDiscard", "Tutup tanpa menyimpan", "Tutup dokumen tanpa menyimpan perubahan.", "\uE8BB", red,
            highContrast ? background : Brush(255, 238, 241));
        var options = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
        options.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        options.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        options.Children.Add(save); options.Children.Add(discard);
        void Reflow()
        {
            var narrow = content.MaxWidth < 560;
            Grid.SetColumnSpan(save, narrow ? 2 : 1); Grid.SetColumnSpan(discard, narrow ? 2 : 1);
            Grid.SetColumn(discard, narrow ? 0 : 1); Grid.SetRow(discard, narrow ? 1 : 0);
        }
        content.Children.Add(options);
        content.Children.Add(new Border { Background = highContrast ? background : Brush(234, 244, 255),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(14),
            Child = Text("File asli dan hasil yang sudah disimpan tetap tersedia.", 14) });
        var back = new Button { Name = "MergeCloseReturn", MinHeight = 48, MinWidth = 44,
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = blue, Foreground = background, BorderBrush = blue, CornerRadius = new CornerRadius(8),
            Content = new TextBlock { Text = "Kembali mengedit", TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center,
                FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold }, TabIndex = 0 };
        AutomationProperties.SetName(back, "Kembali mengedit"); save.TabIndex = 1; discard.TabIndex = 2;
        content.Children.Add(back);
        var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        dialog.Content = scroll;
        var view = new MergeCloseDialogView { Dialog = dialog, Save = save, Discard = discard, Return = back };
        var closing = false;
        var heldReturnKeys = new HashSet<VirtualKey>();
        var returnRequested = false;
        void Finish(ContentDialogResult result)
        {
            if (closing) return;
            if (organizer && heldReturnKeys.Count > 0) { returnRequested = true; return; }
            closing = true; view.Result = result; dialog.Hide();
        }
        save.Click += (_, _) => Finish(ContentDialogResult.Primary);
        discard.Click += (_, _) => Finish(ContentDialogResult.Secondary);
        back.Click += (_, _) => Finish(ContentDialogResult.None);
        // Enter remains safe even after keyboard navigation; Space explicitly activates a focused option.
        if (!organizer) dialog.PreviewKeyDown += (_, e) =>
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Escape) { e.Handled = true; Finish(ContentDialogResult.None); }
        };
        else
        {
            bool HandleOrganizerKeyDown(VirtualKey key)
            {
                if (key is not (VirtualKey.Enter or VirtualKey.Escape)) return false;
                heldReturnKeys.Add(key); returnRequested = true;
                return true;
            }
            bool HandleOrganizerKeyUp(VirtualKey key)
            {
                if (key is not (VirtualKey.Enter or VirtualKey.Escape)) return false;
                heldReturnKeys.Remove(key);
                if (returnRequested && heldReturnKeys.Count == 0)
                    DispatcherQueue.TryEnqueue(() => { if (heldReturnKeys.Count == 0) Finish(ContentDialogResult.None); });
                return true;
            }
            dialog.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler((_, e) =>
            {
                if (HandleOrganizerKeyDown(e.Key)) e.Handled = true;
            }), true);
            dialog.AddHandler(UIElement.PreviewKeyUpEvent, new KeyEventHandler((_, e) =>
            {
                if (HandleOrganizerKeyUp(e.Key)) e.Handled = true;
            }), true);
#if DEBUG
            view.OrganizerKeyDown = HandleOrganizerKeyDown;
            view.OrganizerKeyUp = HandleOrganizerKeyUp;
#endif
        }
        void Bound()
        {
            content.MaxWidth = Math.Max(80, Math.Min(720, root.ActualWidth - 96));
            scroll.MaxHeight = Math.Max(60, root.ActualHeight - 96);
            dialog.Resources["ContentDialogMinWidth"] = Math.Min(280, content.MaxWidth);
            dialog.Resources["ContentDialogMaxWidth"] = content.MaxWidth + 48;
            dialog.Resources["ContentDialogMaxHeight"] = Math.Max(100, root.ActualHeight - 32);
            Reflow();
        }
        void Resize(object sender, SizeChangedEventArgs e) => Bound();
        Bound();
        dialog.Opened += (_, _) =>
        {
            root.SizeChanged += Resize; Bound(); back.Focus(FocusState.Programmatic);
#if DEBUG
            if (organizer) auditOrganizerCloseOpened?.Invoke(view);
#endif
        };
        dialog.Closed += (_, _) =>
        {
            root.SizeChanged -= Resize;
#if DEBUG
            view.OrganizerKeyDown = null; view.OrganizerKeyUp = null;
#endif
        };
        return view;
    }
}
