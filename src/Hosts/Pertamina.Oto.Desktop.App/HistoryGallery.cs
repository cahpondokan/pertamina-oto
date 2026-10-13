using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private double historyScale = .6;
    private bool historyDetails = true;
    private string historySearch = "";
    private int historyFilter;
    private int historySort;
    private Grid? historyShell;
    private Grid? historyPreview;
    private Border? historyPreviewFrame;
    private Grid? historyLibrary;
    private ScrollViewer? historyViewport;
    private Canvas? historyCanvas;
    private TextBlock? historyNotice;
    private TextBox? historySearchBox;
    private Action? resizeHistory;
    private Action? drawHistory;
    private string? historySelectedPath;
    private bool historyExpanded;
    private readonly List<HistoryRow> historyRows = [];

    private sealed class HistoryRow
    {
        internal required SavedJob Job { get; init; }
        internal required string Path { get; init; }
        internal required Border Frame { get; init; }
        internal required Grid Columns { get; init; }
        internal required Grid Content { get; init; }
        internal required Image Picture { get; init; }
        internal required StackPanel Names { get; init; }
        internal required TextBlock Name { get; init; }
        internal required TextBlock Detail { get; init; }
        internal required TextBlock Inline { get; init; }
        internal required TextBlock Pages { get; init; }
        internal required TextBlock Size { get; init; }
        internal required TextBlock Date { get; init; }
        internal required TextBlock Placeholder { get; init; }
        internal required Button More { get; init; }
        internal bool Loading { get; set; }
        internal bool Attempted { get; set; }
        internal double Top { get; set; }
    }

    private void CloseHistoryPreview()
    {
        historyReaderRequest++;
        disposeHistoryReader?.Invoke(); disposeHistoryReader = null;
        historyPreview?.Children.Clear();
        if (historyPreviewFrame is not null) historyPreviewFrame.Visibility = Visibility.Collapsed;
        historyExpanded = false; historySelectedPath = null;
        resizeHistory?.Invoke(); UpdateHistorySelection();
    }

    private void UpdateHistorySelection()
    {
        foreach (var row in historyRows)
        {
            var selectedRow = string.Equals(row.Path, historySelectedPath, StringComparison.OrdinalIgnoreCase);
            row.Frame.Background = selectedRow ? Brush(232, 242, 255) : Brush(255, 255, 255);
            row.Frame.BorderBrush = selectedRow ? Brush(8, 102, 232) : Brush(235, 239, 245);
            row.Frame.BorderThickness = selectedRow ? new Thickness(3, 0, 0, 0) : !historyDetails ? new Thickness(1) : new Thickness(0, 0, 0, 1);
        }
    }

    private void ResizeHistoryPanels()
    {
        if (historyShell is null || historyLibrary is null || historyPreviewFrame is null) return;
        var preview = historyPreviewFrame.Visibility == Visibility.Visible;
        var hideList = preview && (historyExpanded || historyShell.ActualWidth < 650);
        historyLibrary.Visibility = hideList ? Visibility.Collapsed : Visibility.Visible;
        historyShell.ColumnDefinitions[0].Width = hideList ? new GridLength(0)
            : preview ? new GridLength(Math.Clamp(historyShell.ActualWidth * .36, 300, 460)) : new GridLength(1, GridUnitType.Star);
        historyShell.ColumnDefinitions[1].Width = preview ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }

    private void BuildHistoryGallery()
    {
        BeginScreen("Riwayat", "Dokumen terakhir Anda"); body.Children.Clear();
        var galleryGeneration = historyNavigationGeneration;
        historyPage.Children.Clear(); historyPage.RowDefinitions.Clear();
        historySelectedPath = null; historyExpanded = false; historyRows.Clear();
        foreach (var height in new[] { GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto })
            historyPage.RowDefinitions.Add(new RowDefinition { Height = height });
        var header = new Grid { Padding = new Thickness(24, 18, 24, 18), MinHeight = 94, BorderBrush = Brush(229, 235, 242), BorderThickness = new Thickness(0, 0, 0, 1), ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new ColumnDefinition()); header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var heading = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        heading.Children.Add(HistoryText("Riwayat", 26, true)); heading.Children.Add(HistoryText("Dokumen terakhir Anda", 13, muted: true)); header.Children.Add(heading);
        var headerActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 22, VerticalAlignment = VerticalAlignment.Center };
        var offline = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, VerticalAlignment = VerticalAlignment.Center };
        offline.Children.Add(new Border { Width = 7, Height = 7, CornerRadius = new CornerRadius(4), Background = Brush(32, 162, 76) });
        offline.Children.Add(HistoryText("Offline", 12)); headerActions.Children.Add(offline);
        var open = HistoryButton("plus", "Buka PDF", true, true); open.Click += async (_, _) => await PickHistoryPdfAsync(); headerActions.Children.Add(open);
        Grid.SetColumn(headerActions, 1); header.Children.Add(headerActions); ResponsiveLayout.Header(header, heading, headerActions, 480); historyPage.Children.Add(header);

        historyShell = new Grid(); historyShell.ColumnDefinitions.Add(new ColumnDefinition()); historyShell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        historyLibrary = new Grid { BorderBrush = Brush(229, 235, 242), BorderThickness = new Thickness(0, 0, 1, 0) };
        foreach (var height in new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto })
            historyLibrary.RowDefinitions.Add(new RowDefinition { Height = height });
        var toolbar = new Grid { Padding = new Thickness(16, 14, 16, 14), ColumnSpacing = 10, RowSpacing = 8, BorderBrush = Brush(229, 235, 242), BorderThickness = new Thickness(0, 0, 0, 1) };
        toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < 4; i++) toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = i == 0 ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
        var search = new TextBox { PlaceholderText = "Cari dokumen…", Text = historySearch, FontSize = 13, MinWidth = 80, MinHeight = 36, CornerRadius = new CornerRadius(6), Padding = new Thickness(32, 7, 8, 7), BorderBrush = Brush(223, 230, 239) };
        historySearchBox = search; AutomationProperties.SetName(search, "Cari nama dokumen");
        var searchHost = new Grid(); searchHost.Children.Add(search); var searchIcon = HistoryIcon("search", 16); searchIcon.HorizontalAlignment = HorizontalAlignment.Left; searchIcon.Margin = new Thickness(10, 0, 0, 0); searchHost.Children.Add(searchIcon); toolbar.Children.Add(searchHost);
        var filters = AllToolsFilter.Concat(ToolCatalog.All.Where(t => savedJobs.Any(j => j.Tool == t.Id)).Select(CatalogName)).ToArray();
        var filter = new ComboBox { ItemsSource = filters, SelectedIndex = Math.Clamp(historyFilter, 0, filters.Length - 1), MaxWidth = 145, MinWidth = 100, MinHeight = 36, FontSize = 12, CornerRadius = new CornerRadius(6) };
        var sort = new ComboBox { ItemsSource = new[] { "Terbaru", "Terlama", "Nama A–Z" }, SelectedIndex = historySort, MinWidth = 100, MinHeight = 36, FontSize = 12, CornerRadius = new CornerRadius(6) };
        AutomationProperties.SetName(filter, "Filter alat"); AutomationProperties.SetName(sort, "Urutan dokumen");
        Grid.SetColumn(filter, 1); toolbar.Children.Add(filter); Grid.SetColumn(sort, 2); toolbar.Children.Add(sort);
        var views = new StackPanel { Orientation = Orientation.Horizontal, Background = Brush(244, 247, 251), CornerRadius = new CornerRadius(6) };
        var cardsButton = HistoryButton("layout-grid", "Tampilan kartu"); var detailsButton = HistoryButton("list", "Tampilan detail"); views.Children.Add(cardsButton); views.Children.Add(detailsButton);
        Grid.SetColumn(views, 3); toolbar.Children.Add(views); historyLibrary.Children.Add(toolbar);

        var columnHeadings = HistoryColumns(); columnHeadings.Padding = new Thickness(24, 10, 24, 10); columnHeadings.BorderBrush = Brush(229, 235, 242); columnHeadings.BorderThickness = new Thickness(0, 0, 0, 1);
        var labels = new[] { "Nama dokumen", "Halaman", "Ukuran", "Waktu proses" };
        for (var i = 0; i < labels.Length; i++) { var label = HistoryText(labels[i], 12, muted: true); Grid.SetColumn(label, i); columnHeadings.Children.Add(label); }
        Grid.SetRow(columnHeadings, 1); historyLibrary.Children.Add(columnHeadings);
        historyCanvas = new Canvas { Background = Brush(255, 255, 255), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        historyViewport = new ScrollViewer { Content = historyCanvas, ZoomMode = ZoomMode.Enabled, MinZoomFactor = .5f, MaxZoomFactor = 2f, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top, HorizontalScrollMode = ScrollMode.Disabled, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetRow(historyViewport, 2); historyLibrary.Children.Add(historyViewport);
        var footer = new Grid { Padding = new Thickness(20, 12, 20, 12), BorderBrush = Brush(229, 235, 242), BorderThickness = new Thickness(0, 1, 0, 0) };
        footer.ColumnDefinitions.Add(new ColumnDefinition()); footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var count = HistoryText("", 11, muted: true); footer.Children.Add(count);
        var hint = HistoryText("Cubit untuk ukuran kartu", 11, muted: true); Grid.SetColumn(hint, 1); footer.Children.Add(hint); Grid.SetRow(footer, 3); historyLibrary.Children.Add(footer);
        toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        historyPreview = new Grid(); historyPreviewFrame = new Border { Child = historyPreview, Background = Brush(230, 234, 240), Visibility = Visibility.Collapsed };
        historyShell.Children.Add(historyLibrary); Grid.SetColumn(historyPreviewFrame, 1); historyShell.Children.Add(historyPreviewFrame); Grid.SetRow(historyShell, 1); historyPage.Children.Add(historyShell);
        historyNotice = HistoryText(libraryNotice, 12, muted: true); historyNotice.Margin = new Thickness(20, 8, 20, 8); historyNotice.Visibility = string.IsNullOrEmpty(libraryNotice) ? Visibility.Collapsed : Visibility.Visible;
        AutomationProperties.SetLiveSetting(historyNotice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite); Grid.SetRow(historyNotice, 2); historyPage.Children.Add(historyNotice);

        var groups = new List<(TextBlock Label, List<HistoryRow> Rows)>();
        var arranging = false;
        void Arrange()
        {
            if (arranging || galleryGeneration != historyNavigationGeneration) return; arranging = true;
            try
            {
                ResizeHistoryPanels();
                var width = Math.Max(180, historyViewport.ActualWidth - 16);
                var details = historyDetails;
                var wide = width >= 730 && details;
                var compact = historyPreviewFrame.Visibility == Visibility.Visible;
                var wrapTools = historyLibrary.ActualWidth < (compact ? 360 : 530);
                toolbar.RowSpacing = wrapTools ? 8 : 0;
                toolbar.Padding = compact ? new Thickness(12) : new Thickness(16, 14, 16, 14);
                sort.Visibility = Visibility.Visible;
                Grid.SetRow(sort, wrapTools ? 2 : 0); Grid.SetColumn(sort, wrapTools ? 0 : 2); Grid.SetColumnSpan(sort, wrapTools ? 4 : 1);
                Grid.SetColumnSpan(searchHost, wrapTools ? 4 : 1); Grid.SetRow(filter, wrapTools ? 1 : 0); Grid.SetColumn(filter, wrapTools ? 0 : 1);
                Grid.SetRow(views, wrapTools ? 1 : 0); views.HorizontalAlignment = HorizontalAlignment.Right;
                offline.Visibility = historyPage.ActualWidth < 500 ? Visibility.Collapsed : Visibility.Visible;
                hint.Visibility = compact || width < 400 ? Visibility.Collapsed : Visibility.Visible;
                columnHeadings.Visibility = wide ? Visibility.Visible : Visibility.Collapsed;
                cardsButton.Background = !details ? Brush(224, 237, 255) : Brush(244, 247, 251);
                detailsButton.Background = details ? Brush(224, 237, 255) : Brush(244, 247, 251);
                AutomationProperties.SetItemStatus(cardsButton, !details ? "Aktif" : ""); AutomationProperties.SetItemStatus(detailsButton, details ? "Aktif" : "");
                historyCanvas.Width = width;
                // Reserve the requested thumbnail width plus its existing
                // in-card space, so native zoom doesn't shrink it at commit.
                var cardWidth = Math.Clamp(Math.Max(190 * historyScale, 150 * historyScale + 48), 140, Math.Max(140, width - 24));
                var columns = details ? 1 : Math.Max(1, (int)((width - 12) / (cardWidth + 12)));
                if (!details) cardWidth = (width - 12 * (columns + 1)) / columns;
                var y = 0d;
                foreach (var group in groups)
                {
                    group.Label.Width = Math.Max(80, width - 32); Canvas.SetLeft(group.Label, 20); Canvas.SetTop(group.Label, y + 12); y += 40;
                    for (var i = 0; i < group.Rows.Count; i++)
                    {
                        var row = group.Rows[i]; var height = details ? 88 : 230 * historyScale + 70;
                        row.Frame.Width = details ? width - 12 : cardWidth; row.Frame.Height = height;
                        row.Frame.Padding = new Thickness(10, 10, 8, 10);
                        row.Columns.ColumnSpacing = wide ? 12 : details ? 4 : 0;
                        row.Columns.ColumnDefinitions[4].Width = details ? new GridLength(36) : new GridLength(0);
                        Grid.SetColumn(row.More, details ? 4 : 0);
                        row.More.HorizontalAlignment = HorizontalAlignment.Right;
                        row.More.VerticalAlignment = details ? VerticalAlignment.Center : VerticalAlignment.Top;
                        row.Top = y + i / columns * (height + (details ? 0 : 12));
                        Canvas.SetTop(row.Frame, row.Top); Canvas.SetLeft(row.Frame, details ? 6 : 12 + i % columns * (cardWidth + 12));
                        for (var c = 1; c <= 3; c++) row.Columns.ColumnDefinitions[c].Width = wide ? columnHeadings.ColumnDefinitions[c].Width : new GridLength(0);
                        row.Pages.Visibility = row.Size.Visibility = row.Date.Visibility = wide ? Visibility.Visible : Visibility.Collapsed;
                        row.Inline.Visibility = wide ? Visibility.Collapsed : Visibility.Visible;
                        row.Name.MaxLines = 2; row.Detail.MaxLines = 1; row.Inline.MaxLines = 1;
                        row.Picture.Width = details ? 44 : Math.Max(70, Math.Min(cardWidth - 48, 150 * historyScale)); row.Picture.Height = details ? 60 : 205 * historyScale;
                        row.Content.ColumnDefinitions[0].Width = details ? new GridLength(56) : new GridLength(1, GridUnitType.Star);
                        row.Content.ColumnDefinitions[1].Width = details ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
                        Grid.SetColumn(row.Names, details ? 1 : 0); Grid.SetRow(row.Names, details ? 0 : 1);
                        row.Names.Margin = new Thickness(0, details ? 0 : 8, 0, 0);
                    }
                    if (group.Rows.Count > 0) y += Math.Ceiling(group.Rows.Count / (double)columns) * ((details ? 88 : 230 * historyScale + 70) + (details ? 0 : 12));
                }
                historyCanvas.Height = Math.Max(y + 12, 160); UpdateHistorySelection(); QueueHistoryThumbnails();
            }
            finally { arranging = false; }
        }
        resizeHistory = Arrange;
        var resetting = false;
        var manipulating = false;
        var resetRetries = 0;
        var settleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        HistoryRow? gestureAnchor = null; double gestureY = 0;
        ZoomController? cardZoom = null;
        void Scale(double scale)
        {
            if (Math.Abs(historyViewport.ZoomFactor - 1) > .0001 && !resetting)
            {
                gestureAnchor = historyRows.FirstOrDefault(r => (r.Top + r.Frame.Height) * historyViewport.ZoomFactor >= historyViewport.VerticalOffset);
                gestureY = gestureAnchor is null ? 0 : gestureAnchor.Top * historyViewport.ZoomFactor - historyViewport.VerticalOffset;
                historyScale = Math.Clamp(scale, .5, 1.8); resetting = true; resetRetries = 0;
                historyViewport.ChangeView(0, historyViewport.VerticalOffset / historyViewport.ZoomFactor, 1, true);
                settleTimer.Stop(); settleTimer.Start(); return;
            }
            var anchor = historyRows.FirstOrDefault(r => r.Top + r.Frame.Height >= historyViewport.VerticalOffset);
            var offset = anchor is null ? 0 : historyViewport.VerticalOffset - anchor.Top;
            historyScale = Math.Clamp(scale, .5, 1.8); Arrange(); historyCanvas.UpdateLayout();
            if (anchor is not null) historyViewport.ChangeView(0, Math.Max(0, anchor.Top + offset), null, true);
            historyViewport.MinZoomFactor = (float)(.5 / historyScale); historyViewport.MaxZoomFactor = (float)(1.8 / historyScale);
            cardZoom?.Sync();
        }
        cardZoom = new ZoomController(historyViewport, "Ukuran kartu", .5, 1.8, () => resetting ? historyScale : historyScale * historyViewport.ZoomFactor, Scale);
        Grid.SetRow(cardZoom.Toolbar, 3); Grid.SetColumnSpan(cardZoom.Toolbar, 4); toolbar.Children.Add(cardZoom.Toolbar);
        cardsButton.Click += (_, _) => { historyDetails = false; historyViewport.ZoomMode = ZoomMode.Enabled; Arrange(); cardZoom.Toolbar.Visibility = Visibility.Visible; };
        detailsButton.Click += (_, _) => { historyDetails = true; historyViewport.ZoomMode = ZoomMode.Disabled; Arrange(); cardZoom.Toolbar.Visibility = Visibility.Collapsed; };
        cardZoom.Toolbar.Visibility = historyDetails ? Visibility.Collapsed : Visibility.Visible;
        historyViewport.ZoomMode = historyDetails ? ZoomMode.Disabled : ZoomMode.Enabled;
        historyViewport.MinZoomFactor = (float)(.5 / historyScale); historyViewport.MaxZoomFactor = (float)(1.8 / historyScale);
        void CompleteCardZoom()
        {
            if (manipulating) return;
            if (historyDetails) { resetting = false; if (Math.Abs(historyViewport.ZoomFactor - 1) > .001) historyViewport.ChangeView(null, null, 1, true); return; }
            if (resetting)
            {
                if (Math.Abs(historyViewport.ZoomFactor - 1) < .001)
                {
                    resetting = false; Scale(historyScale);
                    if (gestureAnchor is not null) historyViewport.ChangeView(0, Math.Max(0, gestureAnchor.Top - gestureY), null, true);
                    cardZoom.Sync(force: true);
                }
                else if (resetRetries++ < 3) { historyViewport.ChangeView(0, historyViewport.VerticalOffset / historyViewport.ZoomFactor, 1, true); settleTimer.Start(); }
                return;
            }
            if (Math.Abs(historyViewport.ZoomFactor - 1) < .005) return;
            resetting = true; resetRetries = 0; var factor = historyViewport.ZoomFactor;
            gestureAnchor = historyRows.FirstOrDefault(r => (r.Top + r.Frame.Height) * factor >= historyViewport.VerticalOffset);
            gestureY = gestureAnchor is null ? 0 : gestureAnchor.Top * factor - historyViewport.VerticalOffset;
            historyScale = Math.Clamp(historyScale * factor, .5, 1.8);
            historyViewport.ChangeView(0, historyViewport.VerticalOffset / factor, 1, true);
            settleTimer.Stop(); settleTimer.Start();
        }
        settleTimer.Tick += (_, _) => { settleTimer.Stop(); CompleteCardZoom(); };
        historyViewport.DirectManipulationStarted += (_, _) => { manipulating = true; settleTimer.Stop(); };
        historyViewport.DirectManipulationCompleted += (_, _) => { manipulating = false; CompleteCardZoom(); };
        historyViewport.Unloaded += (_, _) => settleTimer.Stop();
        historyViewport.ViewChanged += (_, e) =>
        {
            QueueHistoryThumbnails(); cardZoom.Sync(force: true);
            if (!e.IsIntermediate && !manipulating) CompleteCardZoom();
            settleTimer.Stop(); if (!manipulating) settleTimer.Start();
        };
        historyCanvas.PointerWheelChanged += (_, e) =>
        {
            if ((Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & Windows.UI.Core.CoreVirtualKeyStates.Down) == 0) return;
            e.Handled = true; if (!historyDetails) cardZoom.Command(e.GetCurrentPoint(historyViewport).Properties.MouseWheelDelta > 0 ? 1 : -1);
        };
        historyViewport.SizeChanged += (_, _) => Arrange(); historyShell.SizeChanged += (_, _) => Arrange();
        void Draw()
        {
            if (galleryGeneration != historyNavigationGeneration) return;
            historyRows.Clear(); groups.Clear(); historyCanvas.Children.Clear();
            var matches = savedJobs.Where(j => j.Paths.Any(p => Path.GetFileName(p).Contains(search.Text, StringComparison.OrdinalIgnoreCase)) &&
                (filter.SelectedIndex <= 0 || ToolCatalog.All.Any(t => t.Id == j.Tool && CatalogName(t) == filter.SelectedItem as string)));
            matches = sort.SelectedIndex switch { 1 => matches.OrderBy(j => j.When), 2 => matches.OrderBy(j => Path.GetFileName(j.Paths[0]), StringComparer.CurrentCultureIgnoreCase), _ => matches.OrderByDescending(j => j.When) };
            string? previousDay = null;
            foreach (var job in matches)
            {
                var day = job.When.LocalDateTime.Date;
                var label = sort.SelectedIndex == 2 ? "Semua dokumen" : day == DateTime.Today ? "Hari ini" : day == DateTime.Today.AddDays(-1) ? "Kemarin" : day.ToString("dd MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("id-ID"));
                if (label != previousDay) { var groupLabel = HistoryText(label, 12, muted: true); groups.Add((groupLabel, [])); historyCanvas.Children.Add(groupLabel); previousDay = label; }
                var row = CreateHistoryRow(job); historyRows.Add(row); groups[^1].Rows.Add(row); historyCanvas.Children.Add(row.Frame);
            }
            count.Text = $"{historyRows.Count} pekerjaan";
            if (historyRows.Count == 0)
            {
                var empty = HistoryText(savedJobs.Count == 0 ? "Belum ada riwayat. Buka PDF atau mulai mengolah dokumen." : "Tidak ada dokumen yang cocok.", 14, muted: true);
                empty.Margin = new Thickness(20, 28, 20, 0); empty.MaxWidth = 340; historyCanvas.Children.Add(empty);
            }
            Arrange();
        }
        drawHistory = Draw;
        search.TextChanged += (_, _) => { historySearch = search.Text; Draw(); };
        filter.SelectionChanged += (_, _) => { historyFilter = filter.SelectedIndex; Draw(); };
        sort.SelectionChanged += (_, _) => { historySort = sort.SelectedIndex; Draw(); };
        SetHistoryPageVisible(true); Draw();
        historyPage.Loaded -= HistoryPageLoaded; historyPage.Loaded += HistoryPageLoaded;
    }

    private void HistoryPageLoaded(object sender, RoutedEventArgs e)
    {
        historyPage.Loaded -= HistoryPageLoaded; resizeHistory?.Invoke();
    }

    private HistoryRow CreateHistoryRow(SavedJob job)
    {
        var path = job.Paths[0]; var tool = ToolCatalog.All.FirstOrDefault(t => t.Id == job.Tool);
        var toolLabel = tool is null ? "Dokumen" : CatalogName(tool);
        var columnsGrid = HistoryColumns();
        var content = new Grid(); content.ColumnDefinitions.Add(new ColumnDefinition()); content.ColumnDefinitions.Add(new ColumnDefinition());
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var picture = new Image { Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var pictureHost = new Grid { BorderBrush = Brush(223, 230, 239), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(3), Padding = new Thickness(2), Background = Brush(255, 255, 255), HorizontalAlignment = HorizontalAlignment.Center }; var placeholder = HistoryText("PDF", 11, true, true); placeholder.HorizontalAlignment = HorizontalAlignment.Center;
        pictureHost.Children.Add(placeholder); pictureHost.Children.Add(picture); content.Children.Add(pictureHost);
        var names = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        var name = HistoryText(Path.GetFileName(path), 13, true); name.TextTrimming = TextTrimming.CharacterEllipsis;
        var detail = HistoryText(job.When.LocalDateTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " · " + toolLabel + (job.Paths.Length > 1 ? $" · {job.Paths.Length} hasil" : ""), 11, muted: true); detail.TextTrimming = TextTrimming.CharacterEllipsis;
        var inline = HistoryText("Memuat detail…", 11, muted: true); inline.TextTrimming = TextTrimming.CharacterEllipsis;
        names.Children.Add(name); names.Children.Add(inline); names.Children.Add(detail); content.Children.Add(names);
        var view = new Button { Content = content, Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent), BorderThickness = new Thickness(0), Padding = new Thickness(0), HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Center };
        AutomationProperties.SetName(view, "Preview " + Path.GetFileName(path)); ToolTipService.SetToolTip(view, Path.GetFileName(path));
        view.Click += async (_, _) => await OpenInternalPdfAsync(path);
        columnsGrid.Children.Add(view);
        var pagesText = HistoryText("—", 12, muted: true); var size = HistoryText("—", 12, muted: true);
        try { if (File.Exists(path)) size.Text = HistoryFileSize(new FileInfo(path).Length); } catch (Exception error) when (error is IOException or UnauthorizedAccessException) { LocalDiagnostics.Record("history-metadata", error); }
        var date = HistoryText(job.When.LocalDateTime.ToString("dd MMM · HH:mm", System.Globalization.CultureInfo.GetCultureInfo("id-ID")), 12, muted: true);
        Grid.SetColumn(pagesText, 1); Grid.SetColumn(size, 2); Grid.SetColumn(date, 3); columnsGrid.Children.Add(pagesText); columnsGrid.Children.Add(size); columnsGrid.Children.Add(date);
        var more = HistoryButton("ellipsis-vertical", "Tindakan untuk " + Path.GetFileName(path));
        var menu = new MenuFlyout();
        var preview = new MenuFlyoutItem { Text = "Preview dokumen", IsEnabled = File.Exists(path) }; preview.Click += async (_, _) => await OpenInternalPdfAsync(path); menu.Items.Add(preview);
        var folderAction = new MenuFlyoutItem { Text = "Buka folder hasil" }; folderAction.Click += async (_, _) => await OpenHistoryFolderAsync(path); menu.Items.Add(folderAction);
        foreach (var extra in job.Paths.Skip(1)) { var result = new MenuFlyoutItem { Text = Path.GetFileName(extra) }; result.Click += async (_, _) => await OpenInternalPdfAsync(extra); menu.Items.Add(result); }
        menu.Items.Add(new MenuFlyoutSeparator());
        var remove = new MenuFlyoutItem { Text = "Hapus catatan (file tetap aman)" };
        remove.Click += (_, _) => RemoveHistoryJob(job);
        menu.Items.Add(remove); more.Flyout = menu; Grid.SetColumn(more, 4); columnsGrid.Children.Add(more);
        var frame = new Border { Child = columnsGrid, CornerRadius = new CornerRadius(5) };
        var row = new HistoryRow { Job = job, Path = path, Frame = frame, Columns = columnsGrid, Content = content, Picture = picture, Names = names, Name = name, Detail = detail, Inline = inline, Pages = pagesText, Size = size, Date = date, Placeholder = placeholder, More = more };
        if (!File.Exists(path)) { inline.Text = "File tidak ditemukan"; view.IsEnabled = false; placeholder.Text = "?"; row.Attempted = true; }
        frame.Loaded += (_, _) => QueueHistoryThumbnails(); return row;
    }

    private void QueueHistoryThumbnails()
    {
        if (historyViewport is null || historyPage.Visibility != Visibility.Visible) return;
        foreach (var row in historyRows)
            if (!row.Attempted && !row.Loading && row.Frame.IsLoaded && row.Top + row.Frame.Height >= historyViewport.VerticalOffset - 100 && row.Top < historyViewport.VerticalOffset + historyViewport.ActualHeight + 150)
                _ = LoadHistoryThumbnailAsync(row);
    }

    private void RemoveHistoryJob(SavedJob job)
    {
        savedJobs.Remove(job); SaveLibrary();
        if (job.Paths.Contains(historySelectedPath)) CloseHistoryPreview();
        drawHistory?.Invoke();
        SetHistoryNotice(string.IsNullOrEmpty(libraryNotice) ? "Catatan dihapus. File asli tetap tersimpan." : libraryNotice);
    }

    private static async Task LoadHistoryThumbnailAsync(HistoryRow row)
    {
        row.Loading = true;
        try
        {
            var info = HistoryText(""); var count = await RenderPreviewAsync(row.Path, 0, row.Picture, info, 320);
            row.Attempted = row.Picture.IsLoaded;
            if (!row.Picture.IsLoaded) return;
            row.Pages.Text = count > 0 ? count.ToString(System.Globalization.CultureInfo.InvariantCulture) : "—";
            row.Inline.Text = count > 0 ? $"{count} halaman · {row.Size.Text}" : "Preview belum tersedia · " + row.Size.Text;
            row.Placeholder.Visibility = row.Picture.Source is null ? Visibility.Visible : Visibility.Collapsed;
        }
        finally { row.Loading = false; }
    }

    private async Task PickHistoryPdfAsync()
    {
        if (active is not null || messageOpen) return;
        var generation = historyNavigationGeneration;
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker(); picker.FileTypeFilter.Add(".pdf");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync();
            if (file is not null && generation == historyNavigationGeneration) await OpenInternalPdfAsync(file.Path);
        }
        catch (Exception error) { LocalDiagnostics.Record("reader-picker", error); SetHistoryNotice("Pemilih file belum dapat dibuka. Coba kembali."); }
    }
}
