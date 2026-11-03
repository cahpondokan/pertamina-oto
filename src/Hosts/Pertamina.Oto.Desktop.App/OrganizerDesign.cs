using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private TextBlock? organizerCount, organizerReady;
    private Action? restoreOrganizerPresentation;
    private StackPanel? organizerCanvas;
    private ScrollViewer? organizerSourceViewport;
    private Button? organizerUndoButton, organizerRedoButton;

    private static Image OrganizerBiblo(double size) => new()
    {
        Width = size, Height = size, Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Center,
        Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", "031-atur-ulang-halaman-pdf.png")))
    };

    // Source index is append-only for the session (including excluded sources for Undo).
    private static string OrganizerSourceCode(int source)
    {
        var code = "";
        for (var n = source + 1; n > 0; n = (n - 1) / 26) code = (char)('A' + (n - 1) % 26) + code;
        return code;
    }

    private static string OrganizerOrigin(OrganizerPage page) => page.Source < 0
        ? "Kosong · A4" : $"{OrganizerSourceCode(page.Source)} · Hal. {page.Page + 1}";

    private static Border OrganizerFrame(UIElement content) => new()
    {
        Child = content, Padding = new Thickness(14), CornerRadius = new CornerRadius(10),
        Background = Brush(255, 255, 255), BorderBrush = Brush(172, 207, 252), BorderThickness = new Thickness(1)
    };

    private InfoBar BuildOrganizerStatus(FrameworkElement owner)
    {
        var notice = new InfoBar { Name = "OrganizerInputNotice", IsClosable = false,
            Severity = InfoBarSeverity.Warning, HorizontalAlignment = HorizontalAlignment.Stretch };
        long? subscription = null;
        void Update()
        {
            var normal = string.IsNullOrEmpty(status.Text) || status.Text.StartsWith("PDF ditambahkan.", StringComparison.Ordinal)
                || status.Text.StartsWith("Halaman kosong A4 ditambahkan.", StringComparison.Ordinal) || status.Text == "PDF hasil tersimpan."
                || status.Text.StartsWith("Siap digabungkan.", StringComparison.Ordinal)
                || status.Text.StartsWith("Tambahkan satu PDF lagi", StringComparison.Ordinal);
            notice.Message = normal ? "" : status.Text;
            notice.IsOpen = !normal;
            notice.Visibility = normal ? Visibility.Collapsed : Visibility.Visible;
        }
        void Subscribe() { subscription ??= status.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => Update()); Update(); }
        // A hidden notice must still observe the next failure. Its visible owner owns the
        // subscription, including when the existing editor is restored from the result view.
        Subscribe();
        owner.Loaded += (_, _) => Subscribe();
        owner.Unloaded += (_, _) => { if (subscription is { } token) status.UnregisterPropertyChangedCallback(TextBlock.TextProperty, token); subscription = null; };
        AutomationProperties.SetLiveSetting(notice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        return notice;
    }

    private static Action StyleOrganizerSaveButton(Button button)
    {
        var resources = button.Resources;
        var properties = new[] { FrameworkElement.StyleProperty, Control.BackgroundProperty, Control.ForegroundProperty };
        var previous = properties.Select(button.ReadLocalValue).ToArray();
        var colors = new ResourceDictionary();
        foreach (var (state, color) in new[] { ("", Brush(0, 103, 255)), ("PointerOver", Brush(0, 85, 210)), ("Pressed", Brush(9, 65, 150)) })
        {
            colors["AccentButtonBackground" + state] = color;
            colors["AccentButtonBorderBrush" + state] = color;
            colors["AccentButtonForeground" + state] = Brush(255, 255, 255);
        }
        colors["AccentButtonBackgroundDisabled"] = Brush(225, 230, 238);
        colors["AccentButtonBorderBrushDisabled"] = Brush(225, 230, 238);
        colors["AccentButtonForegroundDisabled"] = Brush(113, 123, 139);
        button.Resources = new ResourceDictionary();
        button.Resources.ThemeDictionaries["Default"] = colors;
        button.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
        button.ClearValue(Control.BackgroundProperty); button.ClearValue(Control.ForegroundProperty);
        button.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
        return () =>
        {
            button.Resources = resources;
            for (var i = 0; i < properties.Length; i++)
                if (previous[i] == DependencyProperty.UnsetValue) button.ClearValue(properties[i]); else button.SetValue(properties[i], previous[i]);
        };
    }

    private void BuildOrganizerDesign()
    {
        // Shared controls retain the presentation of the tool that owned them before this screen.
        var oldHeader = outputName.Header; var oldNameHeight = outputName.MinHeight;
        var oldSpacing = toolOptions.Spacing; var oldRunHeight = run.MinHeight; var oldRunAlignment = run.HorizontalAlignment;
        var restoreSaveStyle = StyleOrganizerSaveButton(run);
        restoreOrganizerPresentation = () =>
        {
            outputName.Header = oldHeader; outputName.MinHeight = oldNameHeight; toolOptions.Spacing = oldSpacing;
            run.MinHeight = oldRunHeight; run.HorizontalAlignment = oldRunAlignment;
            restoreSaveStyle();
        };
        var favorite = new ToggleButton { Content = "★ Favorit", IsChecked = favorites.Contains("organize") };
        favorite.Click += (_, _) => { if (favorite.IsChecked == true) favorites.Add("organize"); else favorites.Remove("organize"); SaveLibrary(); };
        var pageHeading = new StackPanel { Spacing = 4 };
        var title = body.Children[0]; var description = body.Children[1]; body.Children.Clear();
        pageHeading.Children.Add(title); pageHeading.Children.Add(description);
        var header = new Grid { ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); header.Children.Add(pageHeading);
        Grid.SetColumn(favorite, 1); header.Children.Add(favorite); body.Children.Add(header);
        var limits = new Expander { Header = "Versi pengembangan · Periksa hasil sebelum digunakan. · Lihat batasan",
            HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(255, 249, 230),
            BorderBrush = Brush(245, 209, 133), BorderThickness = new Thickness(1),
            Content = Copy("Maksimal 50 PDF lokal, 100 MB total dan 1000 halaman hasil. PDF terkunci atau rusak dapat ditolak. Halaman kosong menggunakan A4. Periksa hasil sebelum digunakan.", 13) };
        limits.Resources.ThemeDictionaries["Default"] = new ResourceDictionary
        {
            ["ExpanderHeaderBackground"] = Brush(255, 248, 234),
            ["ExpanderHeaderBorderBrush"] = Brush(248, 213, 146),
            ["ExpanderContentBackground"] = Brush(255, 252, 245)
        };
        limits.Resources.ThemeDictionaries["HighContrast"] = new ResourceDictionary();
        body.Children.Add(limits);
        var canvas = organizerCanvas = new StackPanel { Spacing = 10 };
        var heading = new Grid { ColumnSpacing = 10 };
        heading.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        heading.Children.Add(OrganizerBiblo(58));
        var words = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        words.Children.Add(HistoryText("Susunan halaman", 20, true));
        organizerCount = Copy("", 13); words.Children.Add(organizerCount); Grid.SetColumn(words, 1); heading.Children.Add(words);
        var close = DocumentCloseButton(); StyleMergeDocumentCloseButton(close); Grid.SetColumn(close, 2); heading.Children.Add(close);
        canvas.Children.Add(heading);
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        toolbar.Children.Add(HomeButton("Tambah PDF", "\uE710", async () => await PickOrganizerAsync(), true));
        organizerUndoButton = HomeButton("Undo", "\uE7A7", () => RestoreLayout(false));
        organizerRedoButton = HomeButton("Redo", "\uE7A6", () => RestoreLayout(true));
        ToolTipService.SetToolTip(organizerUndoButton, "Urungkan perubahan · Ctrl+Z");
        ToolTipService.SetToolTip(organizerRedoButton, "Ulangi perubahan · Ctrl+Y");
        toolbar.Children.Add(organizerUndoButton); toolbar.Children.Add(organizerRedoButton); canvas.Children.Add(toolbar);
        var hint = MergeLabel("\uE8A5", "Seret atau tempel PDF di area ini");
        canvas.Children.Add(new Border { Child = hint, Padding = new Thickness(12), BorderBrush = Brush(201, 218, 245), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(7) });
        canvas.Children.Add(BuildOrganizerStatus(canvas));
        layoutStatus.FontSize = 12; canvas.Children.Add(layoutStatus);
        canvas.Children.Add(OrganizerZoomHost());
        canvas.Children.Add(Copy("A/B = file sumber · Hal. = halaman asli · Urutan = posisi pada hasil", 12));
        canvas.AllowDrop = true;
        canvas.DragOver += (_, e) => { e.AcceptedOperation = active is null && !importing && organizerHeld is null && e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None; e.Handled = true; };
        canvas.Drop += async (_, e) =>
        {
            if (e.Handled || !e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            e.Handled = true; var deferral = e.GetDeferral();
            try { await AcceptOrganizerFilesAsync(await e.DataView.GetStorageItemsAsync()); }
            catch (Exception) { status.Text = "File tidak dapat ditambahkan."; }
            finally { deferral.Complete(); }
        };
        toolWorkspace.Children.Add(OrganizerFrame(canvas));

        var right = new StackPanel { Spacing = 8 };
        var sources = new StackPanel { Spacing = 6 };
        sources.Children.Add(HistoryText("File sumber", 18, true));
        organizerSourceViewport = new ScrollViewer { Content = sourceSummary, MaxHeight = 210, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        sources.Children.Add(organizerSourceViewport);
        sources.Children.Add(Copy("Keluarkan dari daftar tidak menghapus file asli.", 11));
        var sourceFrame = OrganizerFrame(sources); sourceFrame.Padding = new Thickness(10); right.Children.Add(sourceFrame);
        outputName.Header = MergeLabel("\uE8A5", "Nama file hasil");
        outputName.Text = organizerOutput; outputName.MinHeight = 44;
        toolOptions.Spacing = 6; toolOptions.Children.Add(outputName);
        toolOptions.Children.Add(BuildMergeOutputLocation(compact: true));
        password.Visibility = Visibility.Visible;
        toolOptions.Children.Add(new Expander { Header = MergeLabel("\uE72E", "Kata sandi sumber (opsional)"), Content = password, HorizontalAlignment = HorizontalAlignment.Stretch });
        organizerReady = Copy("", 13);
        toolOptions.Children.Add(new Border { Child = organizerReady, Padding = new Thickness(12), CornerRadius = new CornerRadius(8),
            Background = Brush(231, 249, 239), BorderBrush = Brush(158, 224, 187), BorderThickness = new Thickness(1) });
        run.Content = "Simpan PDF baru"; run.MinHeight = 48; run.HorizontalAlignment = HorizontalAlignment.Stretch;
        toolOptions.Children.Add(run);
        toolOptions.Children.Add(MergeLabel("\uE73E", "File asli tidak ditimpa.", true));
        right.Children.Add(MergeResultFrame(organizer: true)); toolWorkspace.Children.Add(right); body.Children.Add(toolWorkspace);
        editable.Add(outputName); editable.Add(password); editable.Add(pageGrid); editable.Add(close);
        foreach (var button in toolbar.Children.OfType<Button>()) editable.Add(button);
        // The page grid scrolls independently; narrow windows stack both panels in the page scroll.
        void Fit()
        {
            var available = (standardPageScroll?.ActualHeight ?? 800) - 350;
            pageGrid.MaxHeight = Math.Clamp(available, 280, 650);
        }
        void Resize(object sender, SizeChangedEventArgs e) => Fit();
        canvas.Loaded += (_, _) => { root.SizeChanged -= Resize; root.SizeChanged += Resize; Fit(); };
        canvas.Unloaded += (_, _) => root.SizeChanged -= Resize;
    }
}
