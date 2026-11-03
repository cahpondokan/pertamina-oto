using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.UI.ViewManagement;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // UI returns intent only. File validation, save/review and draft replacement belong to the caller.
    private async Task<string[]?> ChooseTransitionDocumentsAsync(ToolCard target,
        IReadOnlyList<TransitionDocument> documents, string context, string notice, bool multiple)
    {
        var view = await ShowFeatureTransitionDialogAsync(() => BuildTransitionDocuments(target, documents, context, notice, multiple));
        return view?.DocumentsResult;
    }

    private async Task<string?> ChooseTransitionActionAsync(ToolCard target, string title, string description,
        IReadOnlyList<TransitionAction> actions, string? notice = null)
    {
        var view = await ShowFeatureTransitionDialogAsync(() => BuildTransitionActions(target, title, description, actions, notice));
        return view?.ActionResult;
    }

    private sealed class TransitionChoiceView
    {
        internal required Border Frame;
        internal required ToggleButton Choice;
        internal required TextBlock Title;
        internal required TextBlock Detail;
        internal TextBlock? PathHint;
        internal TextBlock? Problem;
        internal Border? Badge;
        internal TextBlock? BadgeText;
        internal int SourceIndex;
        internal string SearchText = "";
    }

    private sealed class TransitionDialogView
    {
        internal required ContentDialog Dialog;
        internal required Grid Layout;
        internal required StackPanel Body;
        internal required ScrollViewer Scroll;
        internal required Button Primary;
        internal required Button Cancel;
        internal required Button Close;
        internal required TextBlock Summary;
        internal required Image Biblo;
        internal required TransitionPalette Palette;
        internal readonly List<TransitionChoiceView> Choices = [];
        internal TextBox? Search;
        internal CheckBox? SelectVisible;
        internal CheckBox? Acknowledgment;
        internal string[]? DocumentsResult;
        internal string? ActionResult;
        internal Action? Confirm;
        internal Action? Refresh;
        internal Action? Reflow;
        internal Action? Cleanup;
#if DEBUG
        internal Func<VirtualKey, bool>? KeyDown;
        internal Func<VirtualKey, bool>? KeyUp;
        internal Action<VirtualKey>? PrimeOpeningKey;
#endif
    }

#if DEBUG
    private Action<TransitionDialogView>? auditFeatureTransitionOpened;
#endif

    private async Task<TransitionDialogView?> ShowFeatureTransitionDialogAsync(Func<TransitionDialogView> build)
    {
        // Do not take ownership of a guard that belongs to a different modal.
        if (messageOpen) return null;
        var opener = FocusManager.GetFocusedElement(root.XamlRoot) as Control;
        TransitionDialogView? view = null;
        messageOpen = true;
        try
        {
            view = build();
            await view.Dialog.ShowAsync();
            return view;
        }
        finally
        {
            view?.Cleanup?.Invoke();
            messageOpen = false;
            if (view?.DocumentsResult is null && view?.ActionResult is null && opener?.IsLoaded == true)
                opener.Focus(FocusState.Programmatic);
        }
    }

    private sealed class TransitionPalette
    {
        internal bool HighContrast;
        internal readonly SolidColorBrush Surface = Brush(255, 255, 255);
        internal readonly SolidColorBrush Ink = Brush(15, 35, 72);
        internal readonly SolidColorBrush Muted = Brush(82, 99, 130);
        internal readonly SolidColorBrush Line = Brush(210, 217, 228);
        internal readonly SolidColorBrush Blue = Brush(0, 86, 235);
        internal readonly SolidColorBrush BlueHover = Brush(0, 69, 195);
        internal readonly SolidColorBrush BluePressed = Brush(0, 53, 153);
        internal readonly SolidColorBrush OnBlue = Brush(255, 255, 255);
        internal readonly SolidColorBrush Selected = Brush(233, 243, 255);
        internal readonly SolidColorBrush NeutralHover = Brush(241, 245, 250);
        internal readonly SolidColorBrush NeutralPressed = Brush(225, 233, 244);
        internal readonly SolidColorBrush Disabled = Brush(235, 238, 243);
        internal readonly SolidColorBrush DisabledInk = Brush(96, 108, 128);
        internal readonly SolidColorBrush Warning = Brush(141, 43, 24);
        internal readonly SolidColorBrush WarningFill = Brush(255, 244, 234);

        internal void Update(bool highContrast)
        {
            HighContrast = highContrast;
            var settings = new UISettings();
            var ink = settings.GetColorValue(UIColorType.Foreground);
            var surface = settings.GetColorValue(UIColorType.Background);
            void Set(SolidColorBrush brush, byte r, byte g, byte b, bool inverse = false) =>
                brush.Color = highContrast ? (inverse ? surface : ink) : Windows.UI.Color.FromArgb(255, r, g, b);
            Set(Surface, 255, 255, 255, true); Set(Ink, 15, 35, 72); Set(Muted, 82, 99, 130);
            Set(Line, 210, 217, 228); Set(Blue, 0, 86, 235); Set(BlueHover, 0, 69, 195);
            Set(BluePressed, 0, 53, 153); Set(OnBlue, 255, 255, 255, true);
            Set(Selected, 233, 243, 255, true); Set(NeutralHover, 241, 245, 250, true);
            Set(NeutralPressed, 225, 233, 244, true); Set(Disabled, 235, 238, 243, true);
            Set(DisabledInk, 96, 108, 128); Set(Warning, 141, 43, 24); Set(WarningFill, 255, 244, 234, true);
        }
    }

    private static TextBlock TransitionText(string text, TransitionPalette palette, double size = 14, bool bold = false) => new()
    {
        Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Foreground = palette.Ink,
        FontWeight = bold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
    };

    private static string TransitionToolName(ToolCard target) => target.Id == "edit" ? "Edit PDF" : CatalogName(target);

    // Existing approved collection. Agent 0 copies these assets into Assets/Biblo during integration.
    private static string TransitionBibloAsset(string toolId) => toolId switch
    {
        "merge" => "028-gabungkan-pdf.png", "organize" => "031-atur-ulang-halaman-pdf.png",
        "edit" => "018-sunting-pdf.png", "split" => "030-pisahkan-pdf.png",
        "remove" => "032-hapus-halaman-pdf.png", "extract" => "033-ekstrak-halaman-pdf.png",
        "rotate" => "034-putar-halaman-pdf.png", "compress" => "040-kompres-pdf.png",
        "images" => "005-gambar-ke-pdf.png", "word-pdf" => "063-word-ke-pdf.png",
        "pdf-jpg" => "093-pdf-ke-jpg.png", "numbers" => "023-tambahkan-nomor-halaman.png",
        "watermark" => "022-tambah-tanda-air.png", "crop" => "025-potong-pdf.png",
        "protect" => "047-lindungi-pdf.png", "unlock" => "048-buka-kunci-pdf.png",
        "scan" => "003-buat-pdf-dengan-kamera.png", "repair" => "043-perbaiki-pdf.png",
        "ocr" => "042-pdf-ocr.png", "ppt-pdf" => "064-powerpoint-ke-pdf.png",
        "excel-pdf" => "065-excel-ke-pdf.png", "html-pdf" => "004-halaman-web-ke-pdf.png",
        "pdf-word" => "089-pdf-ke-word.png", "pdf-ppt" => "090-pdf-ke-powerpoint.png",
        "pdf-excel" => "091-pdf-ke-excel.png", "pdfa" => "046-pdf-ke-pdf-a.png",
        "forms" => "020-isi-pdf.png", "sign" => "049-tanda-tangani-pdf.png",
        "redact" => "050-hitamkan-pdf.png", "compare" => "054-bandingkan-pdf.png",
        "markdown" => "107-pdf-ke-markdown.png", _ => "059-studio-pdf-desktop-oto.png"
    };

    private static Button TransitionButton(string name, string label, TransitionPalette p, bool primary = false)
    {
        var button = new Button
        {
            Name = name, Content = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
            MinHeight = 46, MinWidth = 44, Padding = new Thickness(16, 10, 16, 10),
            CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center,
            UseSystemFocusVisuals = true, IsTabStop = true
        };
        // Override every state explicitly: the neutral cancel cannot inherit the global accent style.
        foreach (var (state, fill) in new[]
        {
            ("", primary ? p.Blue : p.Surface), ("PointerOver", primary ? p.BlueHover : p.NeutralHover),
            ("Pressed", primary ? p.BluePressed : p.NeutralPressed), ("Disabled", p.Disabled)
        })
        {
            button.Resources["ButtonBackground" + state] = fill;
            button.Resources["ButtonBorderBrush" + state] = primary ? fill : p.Line;
            button.Resources["ButtonForeground" + state] = state == "Disabled" ? p.DisabledInk : primary ? p.OnBlue : p.Ink;
        }
        AutomationProperties.SetName(button, label);
        return button;
    }

    private TransitionDialogView BuildTransitionShell(ToolCard target, string title, string description)
    {
        var accessibility = new AccessibilitySettings();
        var palette = new TransitionPalette(); palette.Update(accessibility.HighContrast);
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, RequestedTheme = ElementTheme.Light,
            DefaultButton = ContentDialogButton.None };
        AutomationProperties.SetName(dialog, title);
        dialog.Resources["ContentDialogBackground"] = palette.Surface;
        dialog.Resources["ContentDialogForeground"] = palette.Ink;
        dialog.Resources["ContentDialogCornerRadius"] = new CornerRadius(14);
        var layout = new Grid { RowSpacing = 18, Background = palette.Surface };
        layout.RowDefinitions.Add(new() { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
        layout.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var header = new Grid { ColumnSpacing = 14 };
        header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        var biblo = new Image { Name = "FeatureTransitionBiblo", Width = 90, Height = 90,
            Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center,
            Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", TransitionBibloAsset(target.Id)))) };
        AutomationProperties.SetAccessibilityView(biblo, AccessibilityView.Raw);
        header.Children.Add(biblo);
        var heading = new StackPanel { Spacing = 7, VerticalAlignment = VerticalAlignment.Center };
        var titleText = TransitionText(title, palette, 26, true);
        heading.Children.Add(titleText);
        var subtitle = TransitionText(description, palette, 15); subtitle.Foreground = palette.Muted;
        heading.Children.Add(subtitle); Grid.SetColumn(heading, 1); header.Children.Add(heading);
        var close = TransitionButton("FeatureTransitionClose", "Tutup", palette);
        close.Content = new FontIcon { Glyph = "\uE8BB", FontSize = 17 };
        close.Width = 44; close.Height = 44; close.Padding = new Thickness(8); close.VerticalAlignment = VerticalAlignment.Top;
        AutomationProperties.SetName(close, "Tutup dialog"); ToolTipService.SetToolTip(close, "Tutup (Esc)");
        Grid.SetColumn(close, 2); header.Children.Add(close); layout.Children.Add(header);
        var body = new StackPanel { Spacing = 12 };
        var scroll = new ScrollViewer { Content = body, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollMode = ScrollMode.Enabled, IsTabStop = false };
        AutomationProperties.SetName(scroll, "Pilihan perpindahan fitur");
        Grid.SetRow(scroll, 1); layout.Children.Add(scroll);
        var summary = TransitionText("", palette, 13); summary.Foreground = palette.Muted;
        summary.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetLiveSetting(summary, AutomationLiveSetting.Polite);
        var cancelButton = TransitionButton("FeatureTransitionCancel", "Batal", palette);
        var primary = TransitionButton("FeatureTransitionPrimary", "Lanjutkan", palette, true); primary.IsEnabled = false;
        var footer = new Grid { ColumnSpacing = 10, RowSpacing = 10,
            Padding = new Thickness(0, 16, 0, 0), BorderThickness = new Thickness(0, 1, 0, 0), BorderBrush = palette.Line };
        footer.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        footer.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        footer.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        footer.RowDefinitions.Add(new() { Height = GridLength.Auto });
        footer.RowDefinitions.Add(new() { Height = GridLength.Auto });
        footer.Children.Add(summary); footer.Children.Add(cancelButton); footer.Children.Add(primary);
        Grid.SetRow(footer, 2); layout.Children.Add(footer); dialog.Content = layout;
        var view = new TransitionDialogView { Dialog = dialog, Layout = layout, Body = body, Scroll = scroll,
            Primary = primary, Cancel = cancelButton, Close = close, Summary = summary, Biblo = biblo, Palette = palette };
        var openingKeys = new HashSet<VirtualKey>();
        var heldKeys = new HashSet<VirtualKey>();
        Action? pending = null;
        var closing = false;
        var cancelRequested = false;
        var attached = false;
        var contrastAttached = false;
        void Finish(Action? commit)
        {
            if (closing) return;
            if (commit is null) cancelRequested = true;
            if (cancelRequested) commit = null;
            if (heldKeys.Count > 0) { pending = () => Finish(commit); return; }
            closing = true; commit?.Invoke(); dialog.Hide();
        }
        void RequestConfirm()
        {
            if (!primary.IsEnabled || openingKeys.Count > 0) return;
            Finish(() => view.Confirm?.Invoke());
        }
        bool KeyDown(VirtualKey key)
        {
            if (key is not (VirtualKey.Enter or VirtualKey.Space or VirtualKey.Escape)) return false;
            heldKeys.Add(key);
            if (key == VirtualKey.Escape) { cancelRequested = true; pending = () => Finish(null); return true; }
            return openingKeys.Contains(key);
        }
        bool KeyUp(VirtualKey key)
        {
            if (key is not (VirtualKey.Enter or VirtualKey.Space or VirtualKey.Escape)) return false;
            heldKeys.Remove(key);
            var consumed = openingKeys.Remove(key) || key == VirtualKey.Escape || pending is not null;
            if (heldKeys.Count == 0 && pending is { } complete)
            {
                pending = null;
                DispatcherQueue.TryEnqueue(() => { if (!closing) complete(); });
            }
            return consumed;
        }
        dialog.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler((_, e) =>
        { if (KeyDown(e.Key)) e.Handled = true; }), true);
        dialog.AddHandler(UIElement.PreviewKeyUpEvent, new KeyEventHandler((_, e) =>
        { if (KeyUp(e.Key)) e.Handled = true; }), true);
        primary.Click += (_, _) => RequestConfirm();
        cancelButton.Click += (_, _) => Finish(null); close.Click += (_, _) => Finish(null);
        void Bound()
        {
            layout.Width = Math.Max(100, Math.Min(688, root.ActualWidth - 96));
            layout.MaxHeight = Math.Max(120, root.ActualHeight - 96);
            dialog.Resources["ContentDialogMinWidth"] = Math.Min(280, layout.Width);
            dialog.Resources["ContentDialogMaxWidth"] = layout.Width + 48;
            dialog.Resources["ContentDialogMaxHeight"] = Math.Max(160, root.ActualHeight - 32);
            var narrow = layout.Width < 580;
            biblo.Width = biblo.Height = narrow ? 64 : 90;
            titleText.FontSize = narrow ? 23 : 26;
            Grid.SetColumnSpan(summary, narrow ? 3 : 1); Grid.SetRow(summary, 0);
            Grid.SetRow(cancelButton, narrow ? 1 : 0); Grid.SetRow(primary, narrow ? 1 : 0);
            Grid.SetColumn(cancelButton, 1); Grid.SetColumn(primary, 2);
            primary.MaxWidth = Math.Max(100, layout.Width * .59);
            cancelButton.MaxWidth = Math.Max(80, layout.Width * .28);
            // A finite scroll height keeps choices scrollable and both footer actions reachable.
            header.Measure(new Windows.Foundation.Size(layout.Width, double.PositiveInfinity));
            footer.Measure(new Windows.Foundation.Size(layout.Width, double.PositiveInfinity));
            scroll.MaxHeight = Math.Max(60, layout.MaxHeight - header.DesiredSize.Height - footer.DesiredSize.Height - 36);
        }
        void Resize(object sender, SizeChangedEventArgs e) => Bound();
        view.Reflow = Bound;
        void ThemeChanged(AccessibilitySettings sender, object args) => DispatcherQueue.TryEnqueue(() =>
        { if (!attached) return; palette.Update(sender.HighContrast); view.Refresh?.Invoke(); });
        view.Cleanup = () =>
        {
            if (attached) { root.SizeChanged -= Resize; attached = false; }
            if (contrastAttached)
            {
                try { accessibility.HighContrastChanged -= ThemeChanged; }
                catch (System.Runtime.InteropServices.COMException error) { LocalDiagnostics.Record("transition-contrast-unsubscribe", error); }
                contrastAttached = false;
            }
            pending = null; heldKeys.Clear(); openingKeys.Clear();
#if DEBUG
            view.KeyDown = null; view.KeyUp = null; view.PrimeOpeningKey = null;
#endif
        };
        Bound();
        dialog.Opened += (_, _) =>
        {
            attached = true; root.SizeChanged += Resize;
            // Unpackaged Windows hosts can expose the current setting while refusing
            // its event subscription (ERROR_NOT_FOUND). Keep the readable palette.
            try { accessibility.HighContrastChanged += ThemeChanged; contrastAttached = true; }
            catch (System.Runtime.InteropServices.COMException error) { LocalDiagnostics.Record("transition-contrast-subscribe", error); }
            // Ignore any activation key already held when the modal obtains focus.
            foreach (var key in new[] { VirtualKey.Enter, VirtualKey.Space, VirtualKey.Escape })
                if ((Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key) & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0)
                { openingKeys.Add(key); heldKeys.Add(key); }
            Bound(); cancelButton.Focus(FocusState.Programmatic);
            DispatcherQueue.TryEnqueue(() => { if (attached && !closing) cancelButton.Focus(FocusState.Programmatic); });
#if DEBUG
            view.KeyDown = KeyDown; view.KeyUp = KeyUp;
            view.PrimeOpeningKey = key => { openingKeys.Add(key); heldKeys.Add(key); };
            auditFeatureTransitionOpened?.Invoke(view);
#endif
        };
        dialog.Closed += (_, _) => view.Cleanup?.Invoke();
        return view;
    }

    private static Border TransitionNotice(string text, TransitionPalette p, bool warning = false)
    {
        var copy = TransitionText(text, p); copy.Foreground = warning ? p.Warning : p.Ink;
        var icon = new FontIcon { Glyph = warning ? "\uE7BA" : "\uE946", FontSize = 22,
            Foreground = warning ? p.Warning : p.Blue, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        AutomationProperties.SetAccessibilityView(icon, AccessibilityView.Raw);
        var row = new Grid { ColumnSpacing = 12 };
        row.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(icon); Grid.SetColumn(copy, 1); row.Children.Add(copy);
        return new Border { Child = row, Padding = new Thickness(14), CornerRadius = new CornerRadius(8),
            Background = warning ? p.WarningFill : p.Selected, BorderBrush = warning ? p.Warning : p.Line,
            BorderThickness = new Thickness(1) };
    }

    private static void PaintTransitionChoice(TransitionChoiceView row, TransitionPalette p)
    {
        var chosen = row.Choice.IsChecked == true;
        row.Frame.Background = chosen ? p.Selected : p.Surface;
        row.Frame.BorderBrush = chosen ? p.Blue : p.Line;
        // Thickness stays constant, avoiding a layout jump when navigating the radio group.
        row.Title.Foreground = row.Choice.IsEnabled ? p.Ink : p.DisabledInk;
        row.Detail.Foreground = p.Muted;
        if (row.PathHint is not null) row.PathHint.Foreground = p.Muted;
        if (row.Problem is not null) row.Problem.Foreground = p.Warning;
        if (row.Badge is not null && row.BadgeText is not null)
        {
            var color = OrganizerColor(row.SourceIndex);
            row.Badge.Background = p.HighContrast ? p.Ink : color;
            // Source colors are stable; text contrast is computed independently of source identity.
            double Linear(byte value) { var n = value / 255d; return n <= .04045 ? n / 12.92 : Math.Pow((n + .055) / 1.055, 2.4); }
            var luminance = .2126 * Linear(color.Color.R) + .7152 * Linear(color.Color.G) + .0722 * Linear(color.Color.B);
            row.BadgeText.Foreground = p.HighContrast ? p.Surface : luminance > .179 ? Brush(0, 0, 0) : Brush(255, 255, 255);
        }
    }

    private static TransitionChoiceView TransitionChoice(string name, string title, string detail,
        TransitionPalette p, bool multiple, string group)
    {
        ToggleButton choice = multiple ? new CheckBox() : new RadioButton { GroupName = group };
        choice.Name = name; choice.MinHeight = 76; choice.MinWidth = 44;
        choice.HorizontalAlignment = HorizontalAlignment.Stretch; choice.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        choice.Padding = new Thickness(12); choice.FlowDirection = FlowDirection.RightToLeft;
        choice.UseSystemFocusVisuals = true; choice.IsTabStop = true;
        var words = new StackPanel { Spacing = 4 };
        var heading = TransitionText(title, p, 16, true);
        var explanation = TransitionText(detail, p, 13);
        words.Children.Add(heading); words.Children.Add(explanation);
        var grid = new Grid { ColumnSpacing = 12, FlowDirection = FlowDirection.LeftToRight };
        grid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(words, 1); grid.Children.Add(words); choice.Content = grid;
        AutomationProperties.SetName(choice, title); AutomationProperties.SetHelpText(choice, detail);
        var frame = new Border { Child = choice, BorderThickness = new Thickness(1.5), CornerRadius = new CornerRadius(9),
            BorderBrush = p.Line, Background = p.Surface, HorizontalAlignment = HorizontalAlignment.Stretch };
        return new TransitionChoiceView { Frame = frame, Choice = choice, Title = heading, Detail = explanation };
    }

    private TransitionDialogView BuildTransitionDocuments(ToolCard target, IReadOnlyList<TransitionDocument> documents,
        string context, string notice, bool multiple)
    {
        // Snapshot the supplied list so caller-side list edits cannot change a visible decision.
        var candidates = documents.ToArray();
        var name = TransitionToolName(target);
        var view = BuildTransitionShell(target, "Buka di " + name,
            multiple ? "Pilih dokumen yang ingin dibuka." : "Pilih satu dokumen yang ingin dibuka.");
        var p = view.Palette;
        ((TextBlock)view.Primary.Content).Text = "Buka di " + name;
        AutomationProperties.SetName(view.Primary, "Buka di " + name);
        var origin = TransitionText(string.IsNullOrWhiteSpace(context) ? "Dokumen tersedia" : context, p, 16, true);
        view.Body.Children.Add(origin);
        view.Body.Children.Add(TransitionText(candidates.Length + " dokumen", p, 13));
        var duplicateNames = candidates.GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var search = new TextBox { Name = "FeatureTransitionSearch", PlaceholderText = "Cari nama dokumen atau lokasi", MinHeight = 40,
            Visibility = candidates.Length > 6 ? Visibility.Visible : Visibility.Collapsed };
        AutomationProperties.SetName(search, "Cari dokumen"); view.Search = search; view.Body.Children.Add(search);
        var all = new CheckBox { Name = "FeatureTransitionSelectVisible", Content = "Pilih semua yang terlihat", MinHeight = 44,
            Visibility = multiple && candidates.Length > 1 ? Visibility.Visible : Visibility.Collapsed };
        AutomationProperties.SetName(all, "Pilih semua dokumen yang terlihat dan dapat dibuka");
        view.SelectVisible = all; view.Body.Children.Add(all);
        var group = "TransitionDocuments-" + Guid.NewGuid().ToString("N");
        var rows = new StackPanel { Spacing = 9 }; view.Body.Children.Add(rows);
        var empty = TransitionText(candidates.Length == 0 ? "Tidak ada dokumen untuk dipilih." : "Tidak ada dokumen yang cocok. Ubah atau hapus pencarian.", p);
        empty.Name = "FeatureTransitionEmpty"; rows.Children.Add(empty);
        var syncing = false;
        void Refresh()
        {
            if (syncing) return;
            syncing = true;
            try
            {
                var query = search.Text.Trim();
                foreach (var row in view.Choices)
                {
                    row.Frame.Visibility = query.Length == 0 || row.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase)
                        ? Visibility.Visible : Visibility.Collapsed;
                    PaintTransitionChoice(row, p);
                }
                var selectedRows = view.Choices.Where(r => r.Choice.IsEnabled && r.Choice.IsChecked == true).ToArray();
                var hidden = selectedRows.Count(r => r.Frame.Visibility == Visibility.Collapsed);
                var validVisible = view.Choices.Where(r => r.Choice.IsEnabled && r.Frame.Visibility == Visibility.Visible).ToArray();
                var selectedVisible = validVisible.Count(r => r.Choice.IsChecked == true);
                all.IsEnabled = validVisible.Length > 0;
                all.IsChecked = selectedVisible == 0 ? false : selectedVisible == validVisible.Length ? true : null;
                empty.Visibility = view.Choices.All(r => r.Frame.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
                view.Summary.Text = selectedRows.Length + " dokumen dipilih" + (hidden > 0 ? " · " + hidden + " di luar pencarian" : "");
                view.Primary.IsEnabled = selectedRows.Length > 0 && (multiple || selectedRows.Length == 1);
                view.Reflow?.Invoke();
            }
            finally { syncing = false; }
        }
        for (var i = 0; i < candidates.Length; i++)
        {
            var document = candidates[i];
            var label = string.IsNullOrWhiteSpace(document.Name) ? Path.GetFileName(document.Path) : document.Name;
            if (string.IsNullOrWhiteSpace(label)) label = "Dokumen tanpa nama";
            var problem = document.Problem;
            if ((string.IsNullOrWhiteSpace(document.Path) || !Path.IsPathFullyQualified(document.Path)) && string.IsNullOrWhiteSpace(problem))
                problem = "Lokasi dokumen tidak tersedia.";
            var row = TransitionChoice("FeatureTransitionDocument" + i, label, document.Detail, p, multiple, group);
            row.SourceIndex = document.SourceIndex;
            row.Choice.IsEnabled = string.IsNullOrWhiteSpace(problem);
            var source = string.IsNullOrWhiteSpace(document.SourceKey) ? OrganizerSourceCode(Math.Max(0, document.SourceIndex)) : document.SourceKey;
            var badgeText = TransitionText(source, p, 16, true); badgeText.TextAlignment = TextAlignment.Center;
            badgeText.VerticalAlignment = VerticalAlignment.Center;
            var badge = new Border { Child = badgeText, MinWidth = 38, MaxWidth = 88, MinHeight = 42,
                Padding = new Thickness(6), CornerRadius = new CornerRadius(8), VerticalAlignment = VerticalAlignment.Center };
            AutomationProperties.SetAccessibilityView(badge, AccessibilityView.Raw);
            row.Badge = badge; row.BadgeText = badgeText;
            var grid = (Grid)row.Choice.Content; grid.Children.Add(badge);
            var words = (StackPanel)grid.Children[0];
            grid.ColumnDefinitions.Insert(1, new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(words, 2);
            var fileMark = new StackPanel { Spacing = 2, Width = 36, VerticalAlignment = VerticalAlignment.Center };
            fileMark.Children.Add(new FontIcon { Glyph = "\uE8A5", FontSize = 23, Foreground = p.Muted });
            var extension = Path.GetExtension(document.Path).ToUpperInvariant() switch
            {
                ".PDF" => "PDF", ".DOCX" => "DOCX", ".JPG" or ".JPEG" => "JPG", ".PNG" => "PNG", _ => "FILE"
            };
            var fileType = TransitionText(extension, p, 10, true); fileType.TextAlignment = TextAlignment.Center;
            fileMark.Children.Add(fileType); AutomationProperties.SetAccessibilityView(fileMark, AccessibilityView.Raw);
            Grid.SetColumn(fileMark, 1); grid.Children.Add(fileMark);
            if (duplicateNames.Contains(document.Name))
            {
                var pathHint = TransitionText(document.Path, p, 12);
                // Full local path is visible for duplicate names, never copied into telemetry or audit logs.
                pathHint.Name = "FeatureTransitionPath" + i; words.Children.Add(pathHint); row.PathHint = pathHint;
            }
            if (!string.IsNullOrWhiteSpace(problem))
            {
                var problemText = TransitionText("Tidak dapat dipilih · " + problem, p, 13);
                words.Children.Add(problemText); row.Problem = problemText;
            }
            AutomationProperties.SetName(row.Choice, source + " · " + label);
            AutomationProperties.SetHelpText(row.Choice, document.Detail + (duplicateNames.Contains(document.Name) ? ". " + document.Path : "")
                + (string.IsNullOrWhiteSpace(problem) ? "" : ". Tidak dapat dipilih: " + problem));
            ToolTipService.SetToolTip(row.Choice, document.Path);
            row.SearchText = string.Join(" ", label, document.Path, document.Detail, source);
            row.Choice.Checked += (_, _) => Refresh(); row.Choice.Unchecked += (_, _) => Refresh();
            view.Choices.Add(row); rows.Children.Add(row.Frame);
        }
        if (!string.IsNullOrWhiteSpace(notice)) view.Body.Children.Add(TransitionNotice(notice, p));
        var invalidCount = view.Choices.Count(r => !r.Choice.IsEnabled);
        if (invalidCount > 0) view.Body.Children.Add(TransitionNotice(invalidCount + " dokumen tidak dapat dipilih. Periksa keterangan pada dokumen tersebut.", p, true));
        var reassurance = TransitionText("Pilihan ini tidak mengubah file asli.", p, 13); reassurance.Foreground = p.Muted;
        view.Body.Children.Add(reassurance);
        search.TextChanged += (_, _) => Refresh();
        void ChangeVisibleSelection(object sender, RoutedEventArgs args)
        {
            if (syncing) return;
            var visible = view.Choices.Where(r => r.Choice.IsEnabled && r.Frame.Visibility == Visibility.Visible).ToArray();
            var select = !visible.All(r => r.Choice.IsChecked == true);
            syncing = true;
            try { foreach (var row in visible) row.Choice.IsChecked = select; }
            finally { syncing = false; }
            Refresh();
        }
        // UI Automation Toggle providers and keyboard input use the same state-change path.
        all.Checked += ChangeVisibleSelection; all.Unchecked += ChangeVisibleSelection; all.Indeterminate += ChangeVisibleSelection;
        view.Confirm = () => view.DocumentsResult = candidates.Where((_, i) => view.Choices[i].Choice.IsEnabled
            && view.Choices[i].Choice.IsChecked == true).Select(d => d.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        view.Refresh = Refresh;
        syncing = true;
        var initial = view.Choices.Where(r => r.Choice.IsEnabled);
        foreach (var row in multiple ? initial : candidates.Length == 1 ? initial : []) row.Choice.IsChecked = true;
        syncing = false; Refresh();
        return view;
    }

    private TransitionDialogView BuildTransitionActions(ToolCard target, string title, string description,
        IReadOnlyList<TransitionAction> actions, string? notice)
    {
        var options = actions.ToArray();
        var view = BuildTransitionShell(target, title, description);
        var p = view.Palette;
        var group = "TransitionActions-" + Guid.NewGuid().ToString("N");
        var warning = TransitionNotice("Tindakan ini dapat mengganti atau membuang perubahan yang belum disimpan. Periksa pilihan sebelum melanjutkan.", p, true);
        warning.Visibility = Visibility.Collapsed;
        var acknowledgment = new CheckBox { Name = "FeatureTransitionAcknowledge", MinHeight = 44,
            HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = TransitionText("Saya memahami perubahan yang belum disimpan dapat dibuang.", p), Visibility = Visibility.Collapsed };
        AutomationProperties.SetName(acknowledgment, "Saya memahami perubahan yang belum disimpan dapat dibuang");
        view.Acknowledgment = acknowledgment;
        var selectedIndex = -1;
        void Refresh()
        {
            var index = view.Choices.FindIndex(r => r.Choice.IsChecked == true && r.Choice.IsEnabled);
            var selectionChanged = selectedIndex != index;
            if (selectionChanged) { selectedIndex = index; acknowledgment.IsChecked = false; }
            var action = index < 0 ? null : options[index];
            var destructive = action?.Destructive == true;
            warning.Visibility = acknowledgment.Visibility = destructive ? Visibility.Visible : Visibility.Collapsed;
            var label = action?.Title ?? "Lanjutkan";
            ((TextBlock)view.Primary.Content).Text = label; AutomationProperties.SetName(view.Primary, label);
            view.Primary.IsEnabled = action is not null && (!destructive || acknowledgment.IsChecked == true);
            view.Summary.Text = action is null ? "Pilih tindakan untuk melanjutkan" : destructive
                ? acknowledgment.IsChecked == true ? "Penggantian draf telah Anda setujui" : "Centang persetujuan di atas untuk melanjutkan"
                : "1 tindakan dipilih";
            foreach (var row in view.Choices) PaintTransitionChoice(row, p);
            view.Reflow?.Invoke();
            if (selectionChanged && destructive)
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (!view.Dialog.IsLoaded || acknowledgment.Visibility != Visibility.Visible) return;
                    // Reveal the required consent after a destructive choice, including
                    // short windows. Do not move keyboard focus or enable confirmation.
                    view.Layout.UpdateLayout();
                    view.Scroll.ChangeView(null, view.Scroll.ScrollableHeight, null, true);
                });
        }
        for (var i = 0; i < options.Length; i++)
        {
            var action = options[i];
            var detail = action.Description + (action.Destructive ? "\nPerubahan yang belum disimpan dapat dibuang." : "");
            var row = TransitionChoice("FeatureTransitionAction" + i, action.Title, detail, p, false, group);
            row.Choice.IsEnabled = !string.IsNullOrWhiteSpace(action.Id);
            row.Choice.Checked += (_, _) => Refresh(); row.Choice.Unchecked += (_, _) => Refresh();
            view.Choices.Add(row); view.Body.Children.Add(row.Frame);
        }
        if (!string.IsNullOrWhiteSpace(notice)) view.Body.Children.Add(TransitionNotice(notice, p));
        if (options.Length == 0) view.Body.Children.Add(TransitionText("Tidak ada tindakan yang tersedia. Tutup dialog untuk kembali.", p));
        view.Body.Children.Add(warning); view.Body.Children.Add(acknowledgment);
        acknowledgment.Checked += (_, _) => Refresh(); acknowledgment.Unchecked += (_, _) => Refresh();
        view.Confirm = () => { if (selectedIndex >= 0 && view.Primary.IsEnabled) view.ActionResult = options[selectedIndex].Id; };
        view.Refresh = Refresh; Refresh();
        return view;
    }
}
