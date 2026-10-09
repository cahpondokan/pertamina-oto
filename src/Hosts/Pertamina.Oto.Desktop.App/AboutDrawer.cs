using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Grid? aboutOverlay;
    private Border? aboutPanel;
    private Control? aboutOpener;
    private Button? aboutSettingsButton;
    private Button BuildAboutSettingsButton()
    {
        aboutSettingsButton = ActionButton("Tentang PERTAMINA OTO", () => ShowAboutDrawer(aboutSettingsButton));
        return aboutSettingsButton;
    }
    private readonly List<Button> aboutButtons = [];
    private readonly List<(Control Control, bool Enabled)> aboutDisabled = [];
    private TextBlock? aboutCopyStatus;

    // Read this executable's immutable build metadata, never latest/current pointers.
    private static string SafeRunningVersion()
    {
        return ApplicationVersion.Running().Display;
    }
    private static string SanitizeRunningVersion(string value)
    {
        return Pertamina.Oto.Distribution.ReleaseInfo.ValidVersion(value)
            ? value : "Build pengembangan";
    }
    private static string AboutPlatform() => "Windows Desktop · " + RuntimeInformation.ProcessArchitecture;
    private static string SafeApplicationInfo() => "PERTAMINA OTO\nOffice Tools Organizer\nVersi berjalan: " + SafeRunningVersion()
        + "\nID build: " + SanitizeRunningVersion(RunningBuildLabel())
        + "\nChannel: " + (ApplicationVersion.Running().Channel ?? "Belum tercatat") + "\nPlatform: " + AboutPlatform() + "\nRuntime: .NET " + Environment.Version
        + "\nDeployment: desktop tanpa paket (unpackaged)";

    private void ShowAboutDrawer(Control? opener = null)
    {
        if (aboutOverlay is not null || messageOpen) return;
        messageOpen = true; // Existing menu/paste flows respect this modal boundary.
        aboutOpener = opener ?? FocusManager.GetFocusedElement(root.XamlRoot) as Control;
        aboutButtons.Clear(); aboutDisabled.Clear();
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child; }
        aboutDisabled.AddRange(Walk((DependencyObject)Content).OfType<Control>().Select(control => (control, control.IsEnabled)));
        foreach (var saved in aboutDisabled) saved.Control.IsEnabled = false;
        aboutOverlay = new Grid(); Grid.SetColumnSpan(aboutOverlay, 2);
        var backdrop = new Border { Background = new SolidColorBrush(Windows.UI.Color.FromArgb(95, 15, 31, 51)) };
        backdrop.PointerPressed += (_, e) => { CloseAboutDrawer(); e.Handled = true; };
        aboutOverlay.Children.Add(backdrop);
        var layout = new Grid { Padding = new Thickness(32, 26, 32, 22), RowSpacing = 20 };
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var header = new Grid(); header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(new TextBlock { Text = "Tentang aplikasi", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        var close = AboutButton("Tutup Tentang aplikasi", "\uE711", () => { CloseAboutDrawer(); return Task.CompletedTask; }, true);
        Grid.SetColumn(close, 1); header.Children.Add(close); layout.Children.Add(header);
        var content = new StackPanel { Spacing = 20 };
        var brand = new Grid { ColumnSpacing = 18 }; brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var logo = new Image { Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "oto.png"))), Width = 70, Height = 70, Stretch = Stretch.Uniform };
        AutomationProperties.SetName(logo, "Logo resmi PERTAMINA OTO");
        brand.Children.Add(new Border { Width = 80, Height = 80, Background = Brush(11, 32, 59), CornerRadius = new CornerRadius(12), Child = logo });
        var name = new StackPanel { Spacing = 5 };
        name.Children.Add(new TextBlock { Text = "PERTAMINA OTO", FontSize = 21, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        name.Children.Add(new TextBlock { Text = "Office Tools Organizer", FontSize = 16, TextWrapping = TextWrapping.Wrap });
        name.Children.Add(Copy("Baca dan olah dokumen secara lokal.", 13));
        name.Children.Add(new Border { Background = Brush(224, 237, 255), CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Left, Child = new TextBlock { Text = ApplicationVersion.Running().Channel ?? "Belum tercatat", FontSize = 12, Foreground = Brush(23, 104, 212) } });
        Grid.SetColumn(name, 1); brand.Children.Add(name); content.Children.Add(brand);
        content.Children.Add(AboutLine());
        var metadata = new StackPanel { Spacing = 14 };
        metadata.Children.Add(AboutMetadata("Versi berjalan", SafeRunningVersion()));
        metadata.Children.Add(AboutMetadata("Platform", AboutPlatform())); content.Children.Add(metadata);
        content.Children.Add(AboutLine());
        content.Children.Add(AboutIconText("\uE777", "Pembaruan", "Dari folder rilis lokal"));
        var update = AboutButton("Periksa update", null, async () => { CloseAboutDrawer(); await CheckLocalUpdateAsync(); });
        update.Background = Brush(23, 104, 212); update.Foreground = Brush(255, 255, 255); content.Children.Add(update);
        content.Children.Add(AboutIconText("\uE72E", "Dokumen tetap di perangkat", "Tanpa unggah untuk pemrosesan lokal."));
        content.Children.Add(AboutLine());
        var links = new StackPanel { Spacing = 2 };
        links.Children.Add(AboutButton("Riwayat versi", "\uE81C", async () => { CloseAboutDrawer(); await ShowReleaseHistoryAsync(); }, linkRow: true));
        links.Children.Add(AboutButton("Panduan penggunaan", "\uE736", () => { CloseAboutDrawer(); ShowHelp(); return Task.CompletedTask; }, linkRow: true));
        links.Children.Add(AboutButton("Laporkan masalah", "\uE7BA", () => { CloseAboutDrawer(); ShowIssueReport(); return Task.CompletedTask; }, linkRow: true));
        links.Children.Add(AboutButton("Lisensi komponen", "\uE8A5", async () => { CloseAboutDrawer(); await ShowAboutLicensesAsync(); }, linkRow: true));
        links.Children.Add(AboutButton("Detail teknis", "\uE943", async () => { CloseAboutDrawer(); await MenuMessageAsync("Detail teknis", SafeApplicationInfo()); }, linkRow: true));
        content.Children.Add(links);
        var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetRow(scroll, 1); layout.Children.Add(scroll);
        var footer = new StackPanel { Spacing = 8 };
        footer.Children.Add(AboutButton("Salin info aplikasi", "\uE8C8", CopyAboutInfoAsync));
        footer.Children.Add(Copy("Tanpa isi dokumen atau lokasi file pribadi.", 12));
        aboutCopyStatus = Copy("", 12); AutomationProperties.SetLiveSetting(aboutCopyStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite); footer.Children.Add(aboutCopyStatus);
        Grid.SetRow(footer, 2); layout.Children.Add(footer);
        aboutPanel = new Border { Width = Math.Min(550, root.ActualWidth), HorizontalAlignment = HorizontalAlignment.Right, Background = Brush(255, 255, 255), Child = layout, BorderThickness = new Thickness(1, 0, 0, 0), BorderBrush = Brush(221, 229, 241) };
        AutomationProperties.SetName(aboutPanel, "Tentang aplikasi"); aboutOverlay.Children.Add(aboutPanel);
        aboutOverlay.KeyDown += AboutKeyDown;
        root.SizeChanged += ResizeAboutDrawer; root.Children.Add(aboutOverlay);
        aboutOverlay.Loaded += (_, _) => close.Focus(FocusState.Programmatic);
        close.Focus(FocusState.Programmatic);
    }
    private Task CopyAboutInfoAsync()
    {
        try { var data = new DataPackage(); data.SetText(SafeApplicationInfo()); Clipboard.SetContent(data); aboutCopyStatus!.Text = "Info aplikasi disalin."; }
        catch (Exception) { aboutCopyStatus!.Text = "Clipboard tidak tersedia. Coba kembali."; }
        return Task.CompletedTask;
    }
    private void ResizeAboutDrawer(object sender, SizeChangedEventArgs e) { if (aboutPanel is not null) aboutPanel.Width = Math.Min(550, root.ActualWidth); }
    private void AboutKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var reverse = (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;
        if (HandleAboutKey(e.Key, reverse)) e.Handled = true;
    }
    private bool HandleAboutKey(VirtualKey key, bool reverse)
    {
        if (key == VirtualKey.Escape) { CloseAboutDrawer(); return true; }
        if (key == VirtualKey.Tab)
        {
            MoveAboutFocus(reverse); return true;
        }
        return false;
    }
    private void MoveAboutFocus(bool reverse)
    {
        var current = FocusManager.GetFocusedElement(root.XamlRoot);
        var index = aboutButtons.FindIndex(b => ReferenceEquals(b, current));
        aboutButtons[(index + (reverse ? -1 : 1) + aboutButtons.Count) % aboutButtons.Count].Focus(FocusState.Keyboard);
    }
    private void CloseAboutDrawer()
    {
        if (aboutOverlay is null) return;
        root.SizeChanged -= ResizeAboutDrawer; root.Children.Remove(aboutOverlay); aboutOverlay = null; aboutPanel = null;
        messageOpen = false;
        foreach (var saved in aboutDisabled) saved.Control.IsEnabled = saved.Enabled;
        aboutDisabled.Clear(); aboutButtons.Clear();
        aboutOpener?.Focus(FocusState.Programmatic); aboutOpener = null;
    }
    private Button AboutButton(string title, string? glyph, Func<Task> action, bool iconOnly = false, bool linkRow = false)
    {
        var button = new Button { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = iconOnly ? HorizontalAlignment.Center : HorizontalAlignment.Stretch,
            Padding = new Thickness(12, 11, 12, 11), MinHeight = 44, Background = Brush(255, 255, 255), BorderBrush = Brush(221, 229, 241) };
        AutomationProperties.SetName(button, title);
        if (glyph is null) { button.Content = title; button.HorizontalContentAlignment = HorizontalAlignment.Center; }
        else if (iconOnly) button.Content = new FontIcon { Glyph = glyph, FontSize = 18 };
        else
        {
            var row = new Grid { ColumnSpacing = 14 }; row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.Children.Add(new FontIcon { Glyph = glyph, FontSize = 20 });
            var label = new TextBlock { Text = title, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(label, 1); row.Children.Add(label); button.Content = row;
            if (linkRow)
            {
                button.BorderThickness = new Thickness(0, 0, 0, 1); button.CornerRadius = new CornerRadius(0);
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
                var arrow = new FontIcon { Glyph = "\uE76C", FontSize = 12 }; Grid.SetColumn(arrow, 2); row.Children.Add(arrow);
            }
        }
        button.Click += async (_, _) => await action(); aboutButtons.Add(button); return button;
    }
    private static Border AboutLine() => new() { Height = 1, Background = Brush(221, 229, 241) };
    private static Grid AboutMetadata(string label, string value)
    {
        var row = new Grid { ColumnSpacing = 12 }; row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap }); var text = new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true }; Grid.SetColumn(text, 1); row.Children.Add(text);
        row.SizeChanged += (_, _) => row.ColumnDefinitions[0].Width = new GridLength(row.ActualWidth < 360 ? 110 : 165); return row;
    }
    private static Grid AboutIconText(string glyph, string title, string detail)
    {
        var row = new Grid { ColumnSpacing = 16 }; row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(new FontIcon { Glyph = glyph, FontSize = 26 }); var text = new StackPanel { Spacing = 4 };
        text.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap }); text.Children.Add(Copy(detail, 13)); Grid.SetColumn(text, 1); row.Children.Add(text); return row;
    }
    private async Task ShowAboutLicensesAsync()
    {
        if (messageOpen) return;
        string inventory;
        try { inventory = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "About-component-notices.txt")); }
        catch (Exception)
        { await MenuMessageAsync("Lisensi komponen", "Inventaris lisensi build ini tidak dapat dibaca. Periksa paket aplikasi lokal."); return; }
        messageOpen = true;
        try
        {
            var scroll = new ScrollViewer { Content = new TextBlock { Text = inventory, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true }, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Lisensi komponen", Content = scroll, CloseButtonText = "Tutup" };
            ResponsiveLayout.BoundDialog(dialog, scroll, root); await dialog.ShowAsync();
        }
        catch (Exception error) { LocalDiagnostics.Record("about-licenses", error); }
        finally { messageOpen = false; }
    }
}
