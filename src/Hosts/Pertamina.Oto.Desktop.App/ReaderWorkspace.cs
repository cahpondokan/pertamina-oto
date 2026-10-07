using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private PdfReaderView? pdfReader;

    private void ShowPdfReader() => ShowPdfReader(true);
    private void ShowPdfReader(bool autoOpen)
    {
        if (active is not null || importing || messageOpen || liveEditor?.Working == true) return;
        BeginScreen("Pembaca PDF", "Baca dokumen lokal");
        SetHistoryPageVisible(true); selected = null; emptyTool = null;
        LayoutCards();
        historyPage.Children.Clear(); historyPage.RowDefinitions.Clear();
        pdfReader ??= new PdfReaderView(this);
        historyPage.Children.Add(pdfReader.View);
        pdfReader.RefreshSession(autoOpen);
        DispatcherQueue.TryEnqueue(() => { pdfReader.Resume(); LayoutCards(); });
        foreach (var item in navigation)
            item.Background = AutomationProperties.GetName(item) == "Pembaca PDF" ? Brush(23, 104, 212) : Brush(7, 29, 61);
    }

    internal async Task OpenPdfReaderAsync(string path, bool processed = false)
    {
        ShowPdfReader(autoOpen: false);
        await Task.Yield(); root.UpdateLayout();
        if (pdfReader is not null) await pdfReader.WaitLoadedAsync();
        if (pdfReader?.Visible == true) await pdfReader.OpenAsync(path);
        pdfReader?.SetIdentity(path, processed);
    }

    private sealed partial class PdfReaderView : IDisposable
    {
        private readonly MainWindow owner;
        internal Grid View { get; } = new() { Background = Brush(255, 255, 255) };
        private readonly bool previewOnly;
        internal bool Visible => View.IsLoaded && (previewOnly || owner.historyPage.Visibility == Visibility.Visible);
        internal Action<Image>? PageSelected;
        internal ScrollViewer ReadingViewport => viewport;
        internal Task NavigateAsync(int index) => GoAsync(index);
        internal PdfDocument? ReadingDocument => document;
        internal int ReadingPage => pageIndex;
        private readonly TextBlock title = HistoryText("Pembaca PDF", 18, true);
        private readonly TextBlock metadata = HistoryText("Buka PDF lokal untuk mulai membaca", 12, muted: true);
        private readonly TextBlock notice = HistoryText("", 12, muted: true);
        private Image picture = new() { Stretch = Stretch.Uniform };
        private readonly ScrollViewer viewport;
        private readonly Grid workspace = new();
        private readonly StackPanel thumbnails = new() { Spacing = 12, Padding = new Thickness(14) };
        private readonly StackPanel empty = new() { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, MaxWidth = 440 };
        private readonly TextBox pageInput = new() { Text = "1", Width = 54, MaxLength = 4, TextAlignment = TextAlignment.Center };
        private readonly TextBlock count = HistoryText("/ 0");
        private readonly ZoomController zoomControls;
        private bool fitWidth;
        private readonly Button previous = HistoryButton("chevron-left", "Halaman sebelumnya");
        private readonly Button next = HistoryButton("chevron-right", "Halaman berikutnya");
        private readonly Button edit = HistoryButton("editor-pen", "Edit PDF", true);
        private readonly Button organize = HistoryButton("layout-grid", "Atur halaman", true);
        private readonly Button bookmark = HistoryButton("favorite-star", "Tandai halaman");
        private readonly Button more = HistoryButton("ellipsis-vertical", "Pilihan dokumen");
        private readonly HashSet<int> marks = [];
        private readonly Grid canvas = new() { Background = Brush(230, 234, 240), Padding = new Thickness(24) };
        private PdfDocument? document;
        private FileStream? sourceLock;
        private string? path;
        private bool busy;
        private bool fit = true;
        private bool settingZoom;
        private bool bookmarksOnly;
        private int pageIndex;
        private int generation;
        private int thumbnailGeneration;
        private int openGeneration;
        internal Task Rendering { get; private set; } = Task.CompletedTask;

        internal PdfReaderView(MainWindow owner, bool previewOnly = false)
        {
            this.owner = owner; this.previewOnly = previewOnly;
            foreach (var height in new[] { GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto })
                View.RowDefinitions.Add(new RowDefinition { Height = height });
            var header = new Grid { Padding = new Thickness(24, 16, 24, 16), ColumnSpacing = 16, BorderBrush = Brush(222, 228, 237), BorderThickness = new Thickness(0, 0, 0, 1) };
            header.ColumnDefinitions.Add(new ColumnDefinition()); header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var titles = new StackPanel { Spacing = 4 }; title.MaxLines = 1; title.TextTrimming = TextTrimming.CharacterEllipsis;
            titles.Children.Add(title); titles.Children.Add(metadata); header.Children.Add(titles);
            var actions = new ResponsiveWrapPanel { Spacing = 8 };
            var open = HistoryButton("plus", "Buka PDF", true); open.Click += async (_, _) => await PickAsync();
            var menu = new MenuFlyout();
            if (!previewOnly)
            {
                var close = HistoryButton("x", "Tutup dokumen", true); close.Click += (_, _) => CloseReaderDocument();
                ToolTipService.SetToolTip(close, "Tutup sesi pembaca ini. Draf alat/editor lain tetap tersedia."); actions.Children.Add(close);
                View.SizeChanged += (_, _) => close.Visibility = View.ActualWidth < 720 ? Visibility.Collapsed : Visibility.Visible;
                var closeItem = new MenuFlyoutItem { Text = "Tutup dokumen" }; closeItem.Click += (_, _) => CloseReaderDocument(); menu.Items.Add(closeItem);
                var review = new MenuFlyoutItem { Text = "Tinjau pekerjaan belum disimpan" };
                review.Click += (_, _) => { var pending = owner.RestartPendingWork(); if (pending.Count > 0) pending[0].Review(); };
                menu.Items.Add(review);
            }
            edit.Background = Brush(8, 102, 232); edit.Foreground = Brush(255, 255, 255);
            edit.Click += async (_, _) => await UseToolAsync("edit");
            var save = new MenuFlyoutItem { Text = "Simpan salinan…" }; save.Click += async (_, _) => await SaveCopyAsync();
            var folder = new MenuFlyoutItem { Text = "Buka lokasi file" }; folder.Click += async (_, _) => await OpenFolderAsync();
            var remove = new MenuFlyoutItem { Text = "Hapus file…", Foreground = Brush(185, 28, 28) }; remove.Click += async (_, _) => await RecycleAsync();
            menu.Items.Add(save); menu.Items.Add(folder); menu.Items.Add(new MenuFlyoutSeparator()); menu.Items.Add(remove); more.Flyout = menu;
            var focus = HistoryButton("maximize2", "Mode fokus", true);
            focus.Click += (_, _) => { owner.readerFocus = !owner.readerFocus; focus.Background = owner.readerFocus ? Brush(222, 236, 255) : Brush(255, 255, 255); owner.LayoutCards(); };
            actions.Children.Add(focus); actions.Children.Add(open); actions.Children.Add(edit); actions.Children.Add(more);
            var other = new MenuFlyoutItem { Text = "Alat lainnya" };
            other.Click += (_, _) => { if (path is null || busy) return; if (!owner.homeFiles.Contains(path)) owner.homeFiles.Add(path); owner.chosenFiles.Clear(); owner.chosenFiles.Add(path); owner.ShowCatalog(); };
            menu.Items.Insert(0, other);
            Grid.SetColumn(actions, 1); header.Children.Add(actions); var layoutHeader = ResponsiveLayout.Header(header, titles, actions, 720, () => View.ActualHeight >= 400); View.Children.Add(header);
            var toolbar = new Grid { Padding = new Thickness(16, 8, 16, 8), ColumnSpacing = 16 };
            toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); toolbar.ColumnDefinitions.Add(new ColumnDefinition()); toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var tools = new ResponsiveWrapPanel { Spacing = 8 };
            var read = HistoryText("Baca", 14, true); read.Foreground = Brush(8, 102, 232); read.Margin = new Thickness(12, 0, 16, 0);
            tools.Children.Add(read); tools.Children.Add(organize); organize.Click += async (_, _) => await UseToolAsync("organize"); toolbar.Children.Add(tools);
            var search = BuildSearchControls(); Grid.SetColumn(search, 1); toolbar.Children.Add(search);
            Grid.SetColumn(bookmark, 2); toolbar.Children.Add(bookmark);
            toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); toolbar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            toolbar.SizeChanged += (_, _) => { var narrow = toolbar.ActualWidth < 700; Grid.SetColumn(search, narrow ? 0 : 1); Grid.SetColumnSpan(search, narrow ? 2 : 1); Grid.SetRow(search, narrow ? 1 : 0); };
            bookmark.Click += async (_, _) =>
            {
                if (!marks.Remove(pageIndex))
                {
                    if (marks.Count < 100) marks.Add(pageIndex);
                    else { notice.Text = "Maksimal 100 penanda dalam sesi ini."; return; }
                }
                UpdateControls(); await DrawThumbnailsAsync(generation);
            };
            Grid.SetRow(toolbar, 1); View.Children.Add(toolbar);
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) }); workspace.ColumnDefinitions.Add(new ColumnDefinition());
            var thumbnailsButton = HistoryButton("file-text", "Halaman", true); tools.Children.Add(thumbnailsButton);
            var rail = new Grid { BorderBrush = Brush(221, 228, 236), BorderThickness = new Thickness(0, 0, 1, 0) };
            rail.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); rail.RowDefinitions.Add(new RowDefinition());
            var tabs = new ResponsiveWrapPanel { Spacing = 2, Margin = new Thickness(8) };
            var pagesTab = HistoryButton("file-text", "Halaman", true); var marksTab = HistoryButton("favorite-star", "Penanda sesi");
            pagesTab.Click += async (_, _) => { bookmarksOnly = false; await DrawThumbnailsAsync(generation); };
            marksTab.Click += async (_, _) => { bookmarksOnly = true; await DrawThumbnailsAsync(generation); };
            tabs.Children.Add(pagesTab); tabs.Children.Add(marksTab); rail.Children.Add(tabs);
            var thumbnailScroll = new ScrollViewer { Content = thumbnails, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            Grid.SetRow(thumbnailScroll, 1); rail.Children.Add(thumbnailScroll); workspace.Children.Add(rail);
            var thumbnailOpen = false; rail.Background = Brush(255, 255, 255); Canvas.SetZIndex(rail, 2);
            void LayoutRail() { var compact = View.ActualWidth < 760; workspace.ColumnDefinitions[0].Width = new GridLength(compact ? 0 : 180); Grid.SetColumnSpan(rail, compact ? 2 : 1); rail.HorizontalAlignment = compact ? HorizontalAlignment.Left : HorizontalAlignment.Stretch; rail.Width = compact ? Math.Min(180, Math.Max(80, View.ActualWidth - 24)) : double.NaN; rail.Visibility = !compact || thumbnailOpen ? Visibility.Visible : Visibility.Collapsed; }
            thumbnailsButton.Click += (_, _) => { thumbnailOpen = !thumbnailOpen; LayoutRail(); if (thumbnailOpen) pagesTab.Focus(FocusState.Programmatic); };
            viewport = new ScrollViewer { Content = pageSurface, ZoomMode = ZoomMode.Enabled, MinZoomFactor = .1f, MaxZoomFactor = 5,
                HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalScrollMode = ScrollMode.Enabled, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            canvas.Children.Add(viewport);
            empty.Children.Add(HistoryIcon("file-text", 56)); empty.Children.Add(HistoryText("Baca PDF langsung di OTO", 24, true));
            empty.Children.Add(HistoryText("Pilih satu PDF, seret ke halaman ini, atau tempel dengan Ctrl+V. Dokumen tidak diunggah dan tidak diubah.", 14, muted: true));
            var emptyOpen = HistoryButton("plus", "Pilih PDF", true, true); emptyOpen.Click += async (_, _) => await PickAsync(); empty.Children.Add(emptyOpen); canvas.Children.Add(empty);
            Grid.SetColumn(canvas, 1); workspace.Children.Add(canvas); Grid.SetRow(workspace, 4); View.Children.Add(workspace);
            // Embedded previews show the tool's chosen source, not the reader's
            // document session. RefreshSession intentionally does not populate them.
            if (!previewOnly) { Grid.SetRow(sessionPicker, 2); View.Children.Add(sessionPicker); }
            var footer = new Grid { Padding = new Thickness(16, 10, 16, 10), ColumnSpacing = 12 };
            footer.ColumnDefinitions.Add(new ColumnDefinition()); footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); footer.ColumnDefinitions.Add(new ColumnDefinition());
            var safe = HistoryText("Mode baca · Sumber tidak diubah", 12, muted: true); footer.Children.Add(safe);
            var controls = new ResponsiveWrapPanel { Spacing = 6 };
            zoomControls = new ZoomController(viewport, "Zoom", .1, 5, () => viewport.ZoomFactor,
                scale => { if (document is null) return; fit = false; ApplyViewportZoom(viewport, scale); },
                FitPreview, () => document is not null && !busy);
            previous.Click += async (_, _) => await GoAsync(pageIndex - 1); next.Click += async (_, _) => await GoAsync(pageIndex + 1);
            AutomationProperties.SetName(pageInput, "Nomor halaman pembaca");
            pageInput.KeyDown += async (_, e) => { if (e.Key == VirtualKey.Enter) { e.Handled = true; await ReadPageInputAsync(); } };
            foreach (var control in new FrameworkElement[] { previous, pageInput, count, next, zoomControls.Toolbar }) controls.Children.Add(control);
            var controlsScroll = new ScrollViewer { Content = controls, HorizontalScrollMode = ScrollMode.Disabled, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 144 };
            Grid.SetColumn(controlsScroll, 1); footer.Children.Add(controlsScroll);
            var hint = HistoryText("Gulir antarhalaman · Cubit untuk zoom", 12, muted: true); hint.HorizontalAlignment = HorizontalAlignment.Right; Grid.SetColumn(hint, 2); footer.Children.Add(hint);
            var expand = HistoryButton("maximize2", "Layar penuh");
            expand.Click += (_, _) => owner.AppWindow.SetPresenter(owner.AppWindow.Presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
            var fullScreen = new MenuFlyoutItem { Text = "Layar penuh / jendela" }; fullScreen.Click += (_, _) => owner.AppWindow.SetPresenter(owner.AppWindow.Presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen); menu.Items.Add(fullScreen);
            Grid.SetRow(footer, 5); View.Children.Add(footer);
            notice.Margin = new Thickness(16, 0, 16, 8); AutomationProperties.SetLiveSetting(notice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
            Grid.SetRow(notice, 6); View.Children.Add(notice);
            BuildLanding(header, toolbar, footer);
            viewport.SizeChanged += (_, _) => { resizeTimer.Stop(); if (Visible && document is not null) resizeTimer.Start(); };
            resizeTimer.Tick += (_, _) =>
            {
                if (settingZoom) return;
                resizeTimer.Stop();
                if (!Visible || document is null) return;
                if (fit && Math.Abs(FitFactor() - viewport.ZoomFactor) > .0001) Fit(); else { UpdatePageExtent(viewport.ZoomFactor); RefreshScroll(); }
            };
            viewport.ViewChanged += (_, e) => OnReaderViewChanged(e);
            View.SizeChanged += (_, _) =>
            {
                var shortView = View.ActualHeight < 400; header.Padding = new Thickness(shortView ? 8 : 24, shortView ? 6 : 16, shortView ? 8 : 24, shortView ? 6 : 16); title.FontSize = shortView ? 16 : 18; metadata.Visibility = shortView ? Visibility.Collapsed : Visibility.Visible; ToolTipService.SetToolTip(title, title.Text + Environment.NewLine + metadata.Text); layoutHeader();
                controlsScroll.MaxHeight = View.ActualHeight < 450 ? 90 : 144; canvas.Padding = new Thickness(View.ActualWidth < 600 || View.ActualHeight < 450 ? 8 : 24);
                safe.Visibility = hint.Visibility = View.ActualWidth < 1080 ? Visibility.Collapsed : Visibility.Visible;
                var narrow = View.ActualWidth < 760;
                foreach (var button in new[] { focus, open, edit, thumbnailsButton }) if (button.Content is StackPanel labels) foreach (var label in labels.Children.OfType<TextBlock>()) label.Visibility = View.ActualWidth < 600 || shortView ? Visibility.Collapsed : Visibility.Visible;
                sessionPicker.Header = narrow ? null : "Dokumen sesi · pilih untuk membaca"; AutomationProperties.SetName(sessionPicker, "Dokumen sesi · pilih untuk membaca");
                LayoutRail(); thumbnailsButton.Visibility = narrow && !previewOnly ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetColumn(controlsScroll, View.ActualWidth < 760 ? 0 : 1); Grid.SetColumnSpan(controlsScroll, View.ActualWidth < 760 ? 3 : 1);
                controlsScroll.MaxWidth = Math.Max(120, View.ActualWidth - 32);
            };
            View.Unloaded += async (_, _) =>
            {
                if (!previewOnly) { if (!Visible) Deactivate(); return; }
                // ContentDialog can reparent its content while opening. Invalidate work
                // immediately, but release the document only after a lasting unload.
                generation++; settingZoom = false; resizeTimer.Stop(); ClearPageBitmaps(); thumbnails.Children.Clear();
                await Task.Delay(50);
                if (!View.IsLoaded) Release();
            };
            View.Loaded += (_, _) => Resume();
            View.AllowDrop = true;
            View.DragOver += (_, e) => { e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None; e.Handled = true; };
            View.Drop += async (_, e) => { e.Handled = true; try { if (e.DataView.Contains(StandardDataFormats.StorageItems)) await AcceptAsync(await e.DataView.GetStorageItemsAsync()); } catch (Exception error) { Failure("reader-drop", error); } };
            View.KeyDown += async (_, e) =>
            {
                if (e.Key == VirtualKey.Escape && thumbnailOpen) { thumbnailOpen = false; LayoutRail(); thumbnailsButton.Focus(FocusState.Programmatic); e.Handled = true; return; }
                if (e.Key == VirtualKey.Escape && searchPanel.Visibility == Visibility.Visible)
                { e.Handled = true; ClearSearch(); viewport.Focus(FocusState.Programmatic); return; }
                if (IsTextPasteTarget(FocusManager.GetFocusedElement(View.XamlRoot) as DependencyObject)) return;
                if (e.Key is VirtualKey.PageDown or VirtualKey.PageUp && document is not null)
                { e.Handled = true; await GoAsync(pageIndex + (e.Key == VirtualKey.PageDown ? 1 : -1)); }
                if (e.Key == VirtualKey.Escape && owner.AppWindow.Presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen)
                { e.Handled = true; owner.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default); }
            };
            if (previewOnly)
            {
                header.Visibility = toolbar.Visibility = Visibility.Collapsed;
                View.AllowDrop = false; emptyOpen.Visibility = Visibility.Collapsed;
                expand.Visibility = Visibility.Collapsed;
                footer.Children.Remove(safe); footer.Children.Remove(hint);
                footer.ColumnDefinitions[0].Width = footer.ColumnDefinitions[2].Width = new GridLength(0);
                footer.Padding = new Thickness(4, 8, 4, 8);
                canvas.Padding = new Thickness(8);
            }
            UpdateControls();
        }
        internal async Task AttachAsync(PdfDocument loaded, string input, long size, int initialPage = 0)
        {
            if (!Visible || loaded.PageCount is 0 or > 1000) return;
            var pages = MeasurePages(loaded);
            var candidate = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read);
            sourceLock?.Dispose(); sourceLock = candidate;
            suspended = false; document = loaded; path = input; pageIndex = 0; marks.Clear(); fit = true;
            InstallPages(pages); ClearSearch();
            title.Text = Path.GetFileName(input); metadata.Text = $"{loaded.PageCount} halaman · {HistoryFileSize(size)} · Offline";
            count.Text = $"/ {loaded.PageCount}"; UpdateControls();
            Rendering = GoAsync(initialPage); await Rendering;
        }
        internal async Task WaitLoadedAsync()
        {
            if (View.IsLoaded) return;
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void Loaded(object sender, RoutedEventArgs e) => completion.TrySetResult();
            View.Loaded += Loaded;
            try { await Task.WhenAny(completion.Task, Task.Delay(2000)); }
            finally { View.Loaded -= Loaded; }
        }

        private float FitFactor() => picture.Width > 0 && picture.Height > 0 ? Math.Clamp((float)(fitWidth ? Math.Max(1, viewport.ActualWidth - 8) / picture.Width : Math.Min(Math.Max(1, viewport.ActualWidth - 8) / picture.Width, Math.Max(1, viewport.ActualHeight - 8) / picture.Height)), .1f, 5) : 1;
        private void Fit()
        {
            if (document is null || viewport.ActualWidth <= 0) return;
            Rendering = GoAsync(pageIndex, clearNotice: false);
        }
        internal void FitPreview(bool width) { fitWidth = width; fit = true; Fit(); }
        private void Zoom(float factor) { if (document is null) return; fit = false; ApplyViewportZoom(viewport, Math.Clamp(viewport.ZoomFactor * factor, .1f, 5)); }
        private void Failure(string operation, Exception error) { LocalDiagnostics.Record(operation, error); notice.Text = "Dokumen belum dapat diproses. Periksa file dan izin akses. File asli tidak diubah."; }
        private void UpdateControls()
        {
            var ready = document is not null && !busy;
            if (document is null) { count.Text = ""; pageInput.Text = ""; }
            zoomControls?.Sync();
            sessionPicker.IsEnabled = !busy;
            edit.IsEnabled = organize.IsEnabled = bookmark.IsEnabled = more.IsEnabled = pageInput.IsEnabled = ready;
            previous.IsEnabled = ready && pageIndex > 0; next.IsEnabled = ready && pageIndex + 1 < document!.PageCount;
            bookmark.Background = marks.Contains(pageIndex) ? Brush(222, 236, 255) : Brush(255, 255, 255);
            ToolTipService.SetToolTip(bookmark, marks.Contains(pageIndex) ? "Hapus penanda halaman (sesi ini)" : "Tandai halaman (sesi ini)");
            empty.Visibility = document is null ? Visibility.Visible : Visibility.Collapsed;
            UpdateLanding();
            UpdateSearchControls();
        }

        internal async Task PickAsync()
        {
            if (busy || owner.messageOpen) return;
            try
            {
                var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".pdf");
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(owner));
                var file = await picker.PickSingleFileAsync(); if (file is not null && Visible) await OpenAsync(file.Path);
            }
            catch (Exception error) { Failure("reader-pick", error); }
        }
        internal async Task PasteAsync()
        {
            try { var data = Clipboard.GetContent(); if (data.Contains(StandardDataFormats.StorageItems)) await AcceptAsync(await data.GetStorageItemsAsync()); }
            catch (Exception error) { Failure("reader-paste", error); }
        }
        private async Task AcceptAsync(IReadOnlyList<IStorageItem> items)
        {
            if (previewOnly) return;
            if (items.Count != 1 || items[0] is not StorageFile file || !file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            { notice.Text = "Pilih satu file PDF untuk dibaca. Gunakan Gabung PDF untuk beberapa dokumen."; return; }
            await OpenAsync(file.Path);
        }
        internal async Task OpenAsync(string input, string previewPassword = "")
        {
            if (busy) return;
            if (path is not null && Path.GetFullPath(input).Equals(path, StringComparison.OrdinalIgnoreCase)) return;
            CancelSearch(); busy = true; UpdateControls(); var ticket = ++openGeneration; FileStream? candidate = null;
            try
            {
                var full = Path.GetFullPath(input);
                if (full.StartsWith(@"\\", StringComparison.Ordinal) || !Path.GetExtension(full).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                { notice.Text = "Pembaca menerima file PDF lokal saja."; return; }
                candidate = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (candidate.Length is <= 0 or > 104857600) { notice.Text = "Batas PDF: 100 MB. Dokumen sebelumnya tetap tersedia."; return; }
                notice.Text = "Membuka PDF…";
                // Embedded previews also call OpenAsync from XAML Loaded. Keep
                // synchronous Storage broker acquisition off that protected STA.
                var file = await Task.Run(async () => await StorageFile.GetFileFromPathAsync(full));
                PdfDocument loaded;
                try { loaded = previewPassword.Length == 0 ? await PdfDocument.LoadFromFileAsync(file) : await PdfDocument.LoadFromFileAsync(file, previewPassword); }
                catch (Exception error) when (error.HResult == unchecked((int)0x8007052B))
                {
                    if (!Visible || ticket != openGeneration || owner.messageOpen || previewOnly)
                    { notice.Text = "PDF terkunci. Isi kata sandi yang sah lalu muat ulang preview."; return; }
                    var secret = new PasswordBox { Header = "Kata sandi PDF", MaxLength = 127 };
                    owner.messageOpen = true;
                    try
                    {
                        var dialog = new ContentDialog { XamlRoot = View.XamlRoot, Title = "Buka PDF terlindungi", Content = secret, PrimaryButtonText = "Buka", CloseButtonText = "Batal" };
                        if (await dialog.ShowAsync() != ContentDialogResult.Primary) { notice.Text = "Pembukaan dibatalkan."; return; }
                        loaded = await PdfDocument.LoadFromFileAsync(file, secret.Password);
                    }
                    finally { secret.Password = ""; owner.messageOpen = false; }
                }
                if (loaded.PageCount is 0 or > 1000) { notice.Text = "Batas pembaca: 1–1000 halaman."; return; }
                if (ticket != openGeneration || !Visible) return;
                var pages = MeasurePages(loaded);
                sourceLock?.Dispose(); sourceLock = candidate; candidate = null;
                readerCloseNotice.Children.Clear(); explicitlyClosed = false; suspended = false; document = loaded; path = full; pageIndex = 0; marks.Clear(); bookmarksOnly = false; fit = true;
                InstallPages(pages);
                ClearSearch();
                title.Text = Path.GetFileName(full); ToolTipService.SetToolTip(title, title.Text);
                metadata.Text = $"{loaded.PageCount} halaman · {HistoryFileSize(sourceLock.Length)} · Offline"; count.Text = $"/ {loaded.PageCount}";
                if (!previewOnly) RememberReaderOpen(full, loaded.PageCount);
                UpdateControls(); View.UpdateLayout();
                Rendering = GoAsync(0); await Rendering;
            }
            catch (Exception error) { Failure("reader-open", error); notice.Text = "PDF tidak dapat dibuka: file mungkin rusak, terkunci kata sandi, atau tidak dapat diakses. Dokumen sebelumnya tetap tersedia."; }
            finally { candidate?.Dispose(); busy = false; UpdateControls(); if (!previewOnly && path is not null) { SetIdentity(path, owner.readerResults.Contains(path)); RefreshSession(false); } }
        }
        private async Task ReadPageInputAsync()
        {
            if (document is not null && int.TryParse(pageInput.Text, out var value) && value >= 1 && value <= document.PageCount) await GoAsync(value - 1);
            else { pageInput.Text = (pageIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture); notice.Text = "Nomor halaman di luar jangkauan."; }
        }
        private async Task GoAsync(int value, bool clearNotice = true)
        {
            if (document is null || !Visible) return;
            var ticket = ++generation;
            settingZoom = true;
            SelectPage(Math.Clamp(value, 0, pageSlots.Length - 1));
            if (clearNotice) notice.Text = "";
            var zoom = fit ? FitFactor() : viewport.ZoomFactor;
            if (fit) fittedZoom = zoom;
            try
            {
                UpdatePageExtent(zoom); viewport.UpdateLayout();
                var settled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                void Changed(object? sender, ScrollViewerViewChangedEventArgs e) { if (!e.IsIntermediate) settled.TrySetResult(); }
                viewport.ViewChanged += Changed;
                try
                {
                    var horizontal = Math.Max(0, (pageSurface.Width * zoom - viewport.ActualWidth) / 2);
                    if (viewport.ChangeView(horizontal, pageSlots[pageIndex].Top * zoom, zoom, true))
                        await Task.WhenAny(settled.Task, Task.Delay(250));
                }
                finally { viewport.ViewChanged -= Changed; }
            }
            finally { if (ticket == generation) settingZoom = false; }
            if (ticket != generation || !Visible) return;
            RefreshScroll(force: true); await Rendering;
        }
        private PdfDocument? thumbnailDocument;
        private string? thumbnailKey;
#if DEBUG
        private int thumbnailRenderRequests;
        private int thumbnailRebuilds;
#endif

        private void UpdateThumbnailSelection()
        {
            foreach (var button in thumbnails.Children.OfType<Button>())
                if (button.Tag is int index)
                {
                    button.BorderBrush = index == pageIndex ? Brush(8, 102, 232) : Brush(220, 227, 235);
                    button.BorderThickness = new Thickness(index == pageIndex ? 2 : 1);
                }
        }

        private async Task DrawThumbnailsAsync(int ticket)
        {
            if (document is null || ticket != generation || !Visible) return;
            var pdf = document;
            var start = pageIndex / 8 * 8;
            var indices = bookmarksOnly ? marks.Order().Take(100).ToArray() : Enumerable.Range(start, Math.Min(8, (int)pdf.PageCount - start)).ToArray();
            var key = (bookmarksOnly ? "marks:" : "pages:") + string.Join(',', indices);
            var thumbnailTicket = ++thumbnailGeneration;
            if (ReferenceEquals(thumbnailDocument, pdf) && thumbnailKey == key && thumbnails.Children.Count > 0)
            { UpdateThumbnailSelection(); return; }
            var replacement = new List<UIElement>();
            if (bookmarksOnly) replacement.Add(HistoryText(indices.Length == 0 ? "Belum ada penanda. Tekan bintang untuk menandai halaman." : "Penanda sesi", 12, muted: true));
            if (!bookmarksOnly && start > 0) { var before = HistoryButton("chevron-left", "8 sebelumnya", true); before.Click += async (_, _) => await GoAsync(start - 1); replacement.Add(before); }
            foreach (var index in indices)
            {
                if (ticket != generation || thumbnailTicket != thumbnailGeneration || !Visible) return;
                var image = new Image { Width = 116, Height = 155, Stretch = Stretch.Uniform };
                var content = new StackPanel { Spacing = 6 }; content.Children.Add(image); content.Children.Add(HistoryText((index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), 12));
                var button = new Button { Content = content, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center,
                    Background = Brush(255, 255, 255), BorderBrush = index == pageIndex ? Brush(8, 102, 232) : Brush(220, 227, 235), BorderThickness = new Thickness(index == pageIndex ? 2 : 1), Padding = new Thickness(8), CornerRadius = new CornerRadius(6) };
                button.Tag = index;
                AutomationProperties.SetName(button, $"Buka halaman {index + 1}"); button.Click += async (_, _) => await GoAsync(index); replacement.Add(button);
                try
                {
                    await RenderGate.WaitAsync();
                    try
                    {
                        if (ticket != generation || thumbnailTicket != thumbnailGeneration || !Visible) return;
                        using var page = pdf.GetPage((uint)index); using var stream = new InMemoryRandomAccessStream();
                        var ratio = page.Size.Height / page.Size.Width;
                        if (!double.IsFinite(ratio) || ratio is < .05 or > 20) continue;
#if DEBUG
                        thumbnailRenderRequests++;
#endif
                        await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = (uint)Math.Max(1, Math.Min(220, 220 / ratio)) });
                        stream.Seek(0); var bitmap = new BitmapImage(); await bitmap.SetSourceAsync(stream);
                        if (ticket == generation && thumbnailTicket == thumbnailGeneration && Visible) image.Source = bitmap;
                    }
                    finally { RenderGate.Release(); }
                }
                catch (Exception error) { LocalDiagnostics.Record("reader-thumbnail", error); }
            }
            if (!bookmarksOnly && start + 8 < pdf.PageCount) { var after = HistoryButton("chevron-right", "8 berikutnya", true); after.Click += async (_, _) => await GoAsync(start + 8); replacement.Add(after); }
            if (ticket != generation || thumbnailTicket != thumbnailGeneration || !Visible || !ReferenceEquals(pdf, document)) return;
            // Publish only complete replacement content; retain the previous rail during rendering.
#if DEBUG
            thumbnailRebuilds++;
#endif
            thumbnails.Children.Clear(); foreach (var child in replacement) thumbnails.Children.Add(child);
            thumbnailDocument = pdf; thumbnailKey = key; UpdateThumbnailSelection();
        }
        private async Task UseToolAsync(string id)
        {
            if (path is null || busy) return;
            if (id == "edit" && owner.liveEditor?.ReaderSource == path) { owner.ShowLiveEditor(); return; }
            if (!owner.homeFiles.Contains(path)) owner.homeFiles.Add(path);
            owner.chosenFiles.Clear(); owner.chosenFiles.Add(path);
            await owner.OpenWorkspaceToolAsync(ToolCatalog.All.First(t => t.Id == id));
        }
        private async Task OpenFolderAsync()
        {
            try { if (path is not null) await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)!)); }
            catch (Exception error) { Failure("reader-folder", error); }
        }
        internal async Task SaveCopyAsync()
        {
            if (path is null || busy) return;
            var original = path;
            busy = true; UpdateControls();
            try
            {
                var picker = new FolderPicker(); picker.FileTypeFilter.Add("*");
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(owner));
                var folder = await picker.PickSingleFolderAsync(); if (folder is null) return;
                await CopyAsync(original, folder);
                notice.Text = "Salinan tersimpan. File asli tidak ditimpa.";
            }
            catch (Exception error) { Failure("reader-copy", error); }
            finally { busy = false; UpdateControls(); }
        }
        private static async Task<StorageFile> CopyAsync(string original, StorageFolder folder) =>
            await (await StorageFile.GetFileFromPathAsync(original)).CopyAsync(folder, Path.GetFileNameWithoutExtension(original) + "_salinan.pdf", NameCollisionOption.GenerateUniqueName);
        private async Task RecycleAsync()
        {
            if (path is null || busy || owner.messageOpen) return;
            var original = path;
            owner.messageOpen = true;
            try
            {
                var dialog = new ContentDialog { XamlRoot = View.XamlRoot, Title = "Pindahkan file ke Recycle Bin?", Content = $"{Path.GetFileName(original)}\nIni menghapus file, bukan hanya halaman. Anda dapat memulihkannya dari Recycle Bin.", PrimaryButtonText = "Pindahkan", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close };
                if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
                // No permanent-delete fallback. Locked files fail safely.
                sourceLock?.Dispose(); sourceLock = null;
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(original, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
                Release(); title.Text = "Pembaca PDF"; metadata.Text = "Buka PDF lokal untuk mulai membaca"; count.Text = "/ 0";
                notice.Text = "File dipindahkan ke Recycle Bin."; UpdateControls();
            }
            catch (Exception error)
            {
                Failure("reader-recycle", error);
                try { if (File.Exists(original)) sourceLock = new FileStream(original, FileMode.Open, FileAccess.Read, FileShare.Read); else { Release(); UpdateControls(); } }
                catch (Exception lockError) when (lockError is IOException or UnauthorizedAccessException) { Release(); UpdateControls(); }
            }
            finally { owner.messageOpen = false; }
        }
        internal void Release() { ClearSearch(); generation++; openGeneration++; settingZoom = false; resizeTimer.Stop(); sourceLock?.Dispose(); sourceLock = null; document = null; path = null; ClearPageBitmaps(); pageSlots = []; pageSurface.Children.Clear(); thumbnails.Children.Clear(); marks.Clear(); UpdateControls(); }
        internal void Deactivate() { if (!suspended) { savedHorizontal = viewport.HorizontalOffset; savedVertical = viewport.VerticalOffset; savedZoom = viewport.ZoomFactor; } suspended = true; CancelSearch(); generation++; openGeneration++; settingZoom = false; resizeTimer.Stop(); ClearPageBitmaps(); thumbnails.Children.Clear(); }
        internal void Resume()
        {
            if (!Visible) return;
            if (document is null) { suspended = false; return; }
            if (!suspended) { RefreshScroll(force: true); return; }
            suspended = false; Rendering = RestoreScrollAsync();
        }
        private async Task RestoreScrollAsync()
        {
            var ticket = ++generation; settingZoom = true; renderedZoom = savedZoom;
            View.UpdateLayout(); UpdatePageExtent(savedZoom);
            var settled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void Changed(object? sender, ScrollViewerViewChangedEventArgs e) { if (!e.IsIntermediate) settled.TrySetResult(); }
            viewport.ViewChanged += Changed;
            try { if (viewport.ChangeView(savedHorizontal, savedVertical, savedZoom, true)) await Task.WhenAny(settled.Task, Task.Delay(250)); }
            finally { viewport.ViewChanged -= Changed; if (ticket == generation) settingZoom = false; }
            if (ticket != generation || suspended || !Visible) return;
            RefreshScroll(force: true); await Rendering;
        }
        public void Dispose() => Release();
    }
}
