using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Pertamina.Oto.LocalPdf;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private LiveEditor? liveEditor;
    internal void OpenEditorScreen() => ShowLiveEditor();

    private void ShowLiveEditor(bool forceWorkspace = false)
    {
        if (!forceWorkspace && liveEditor?.HasDocument != true)
        { ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "edit")); return; }
        BeginScreen("Edit PDF", "Editor lokal"); SetHistoryPageVisible(true);
        selected = ToolCatalog.All.First(t => t.Id == "edit"); emptyTool = null;
        LayoutCards();
        foreach (var item in navigation) item.Background = AutomationProperties.GetName(item) == "Semua Alat" ? Brush(23, 104, 212) : Brush(7, 29, 61);
        historyPage.Children.Clear(); historyPage.RowDefinitions.Clear(); liveEditor ??= new LiveEditor(this);
        historyPage.Children.Add(liveEditor.View);
    }

    private async Task OpenEditorDocumentAsync(string input)
    {
        ShowLiveEditor(true);
        await liveEditor!.OpenAsync(input);
        if (!liveEditor.HasDocument)
        {
            var message = liveEditor.OpenNotice;
            ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "edit"));
            if (emptyNotice is not null) emptyNotice.Text = message;
        }
    }

    // Draft stays in memory across navigation. Explicit replacement and app close warn before loss.
    private sealed partial class LiveEditor
    {
        internal bool HasDocument => pdf is not null;
        internal string OpenNotice => notice.Text;
        private readonly MainWindow owner;
        internal Grid View { get; } = new() { Padding = new Thickness(16), RowSpacing = 12, Background = Brush(245, 247, 251) };
        private readonly Grid workspace = new() { ColumnSpacing = 12 };
        private Canvas sheet = new() { Background = Brush(255, 255, 255), Width = 595, Height = 842 };
        private Image background = new() { IsHitTestVisible = false };
        private readonly ScrollViewer viewport;
        private readonly StackPanel properties = new() { Spacing = 12, Padding = new Thickness(16) };
        private readonly StackPanel thumbnails = new() { Spacing = 8, Padding = new Thickness(8) };
        private readonly TextBlock filename = HistoryText("Pilih PDF untuk mulai", 13, muted: true);
        private readonly TextBlock notice = HistoryText("Edit teks asli yang didukung, atau tambahkan teks, gambar, bentuk dan coretan. Sumber tetap aman.", 13, muted: true);
        private readonly TextBlock pageStatus = HistoryText("0 / 0");
        private readonly Button save = HistoryButton("file-text", "Simpan PDF baru", true, true);
        private readonly Button undo = HistoryButton("editor-undo", "Urungkan");
        private readonly Button redo = HistoryButton("editor-redo", "Ulangi");
        private readonly List<EditorMark> marks = [];
        private readonly Stack<EditorMark[]> past = [];
        private readonly Stack<EditorMark[]> future = [];
        private readonly Dictionary<string, Button> modes = [];
        private readonly List<Point> stroke = [];
        private PdfDocument? pdf;
        private FileStream? sourceLock;
        private readonly List<(string Path, FileStream Lock, BitmapImage Bitmap)> images = [];
        private EditorPageSize[] pageSizes = [];
        private bool pageReady;
        private bool picking;
        private readonly Button previous = HistoryButton("chevron-left", "Halaman sebelumnya");
        private readonly Button next = HistoryButton("chevron-right", "Halaman berikutnya");
        private readonly NumberBox pageInput = new() { Minimum = 1, Value = 1, Width = 76, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        private readonly Button resultButton = HistoryButton("file-text", "Lihat hasil", true);
        private string? lastResult;
        private string? path;
        private string mode = "Pilih";
        private string shape = "rectangle";
        private int page;
        private int chosen = -1;
        private bool rendering;
        private bool busy;
        private bool drawing;
        private bool updating;
        private int propertyGeneration;
        private EditorMark? dragging;
        private Point dragOrigin;
        private string savedState = "[]";
        private string State => System.Text.Json.JsonSerializer.Serialize(marks);
        internal bool Dirty => State != savedState || originalInlineDraft?.Changed == true;
        internal bool Working => busy || rendering || picking;
        internal bool Visible => View.IsLoaded && owner.historyPage.Visibility == Visibility.Visible && owner.historyPage.Children.Contains(View);

        internal LiveEditor(MainWindow owner)
        {
            this.owner = owner;
            filename.MaxLines = 1; filename.TextTrimming = TextTrimming.CharacterEllipsis;
            save.MinHeight = 44;
            foreach (var height in new[] { GridLength.Auto, GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto })
                View.RowDefinitions.Add(new RowDefinition { Height = height });
            var header = new Grid { ColumnSpacing = 12 };
            header.ColumnDefinitions.Add(new ColumnDefinition()); header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var title = new StackPanel { Spacing = 3 }; title.Children.Add(HistoryText("Edit PDF", 24, true)); title.Children.Add(filename); header.Children.Add(title);
            var actions = new ResponsiveWrapPanel { Spacing = 8 };
            var open = HistoryButton("plus", "Buka PDF", true); open.Click += async (_, _) => await PickAsync();
            var close = HistoryButton("x", "Tutup dokumen", true); close.Click += async (_, _) => await CloseAsync();
            resultButton.Visibility = Visibility.Collapsed;
            resultButton.Click += async (_, _) => { if (lastResult is not null && !Working) await owner.ShowReadPreviewDialogAsync(lastResult); };
            var readSource = HistoryButton("scan-eye", "Baca sumber", true);
            readSource.Click += async (_, _) =>
            {
                if (path is null || pdf is null || Working) return;
                await owner.ShowReadPreviewDialogAsync(path, pdf, page,
                    "Mode baca sumber asli. Perubahan draf belum ditampilkan di sini; draf tetap tersimpan di editor. Untuk membaca perubahan, simpan PDF baru lalu pilih Lihat hasil.");
            };
            actions.Children.Add(readSource);
            actions.Children.Add(open); actions.Children.Add(close); actions.Children.Add(resultButton); actions.Children.Add(save); Grid.SetColumn(actions, 1); header.Children.Add(actions); ResponsiveLayout.Header(header, title, actions, 1000); View.Children.Add(header);
            save.Click += async (_, _) => await SaveAsync();
            Grid.SetRow(notice, 1); View.Children.Add(notice);
            var toolbar = new ResponsiveWrapPanel { Spacing = 6 };
            foreach (var (label, icon) in new[] { ("Pilih", "editor-pointer"), ("Edit teks asli", "editor-text"), ("Tambah teks", "editor-text-add"), ("Gambar", "favorite-image"), ("Gambar bebas", "editor-pen"), ("Bentuk", "editor-shapes") })
            {
                var button = HistoryButton(icon, label, true); button.MinHeight = 44;
                button.Click += async (_, _) =>
                {
                    if (Working) return;
                    if (!await CommitOriginalInlineAsync()) return;
                    EndInlineText();
                    if (label == "Gambar") { await PickImageAsync(); return; }
                    mode = label; chosen = -1; BuildProperties(); Refresh();
                    if (label == "Edit teks asli")
                    {
                        busy = true; Refresh();
                        try { await LoadOriginalRunsAsync(); } finally { busy = false; BuildProperties(); Refresh(); }
                    }
                };
                toolbar.Children.Add(button); modes[label] = button;
            }
            undo.Click += (_, _) => Restore(false); redo.Click += (_, _) => Restore(true); toolbar.Children.Add(undo); toolbar.Children.Add(redo);
            var toolbarScroll = new ScrollViewer { Content = toolbar, HorizontalScrollMode = ScrollMode.Disabled, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 132 };
            Grid.SetRow(toolbarScroll, 2); View.Children.Add(toolbarScroll);
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(132) });
            workspace.ColumnDefinitions.Add(new ColumnDefinition()); workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
            var thumbScroll = new ScrollViewer { Content = thumbnails, Background = Brush(255, 255, 255) }; workspace.Children.Add(thumbScroll);
            viewport = new ScrollViewer { Content = editorSurface, IsTabStop = true, ZoomMode = ZoomMode.Enabled, MinZoomFactor = .1f, MaxZoomFactor = 5,
                Background = Brush(226, 232, 240), HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetColumn(viewport, 1); workspace.Children.Add(viewport);
            AutomationProperties.SetName(viewport, "Kanvas PDF · pilih objek lalu gunakan Delete atau tombol panah");
            var propertiesScroll = new ScrollViewer { Content = properties, Background = Brush(255, 255, 255) }; Grid.SetColumn(propertiesScroll, 2); workspace.Children.Add(propertiesScroll);
            Grid.SetRow(workspace, 3); View.Children.Add(workspace);
            var footer = new ResponsiveWrapPanel { Spacing = 8 };
            var thumbButton = HistoryButton("file-text", "Halaman", true); var thumbDrawer = new Flyout(); thumbButton.Flyout = thumbDrawer; footer.Children.Add(thumbButton);
            var drawerButton = HistoryButton("editor-shapes", "Pengaturan", true); var drawer = new Flyout(); drawerButton.Flyout = drawer; footer.Children.Add(drawerButton);
            previous.Click += async (_, _) => await RenderAsync(page - 1);
            next.Click += async (_, _) => await RenderAsync(page + 1);
            AutomationProperties.SetName(pageInput, "Nomor halaman");
            pageInput.ValueChanged += async (_, e) => { if (!updating && double.IsFinite(e.NewValue)) await RenderAsync((int)e.NewValue - 1); };
            footer.Children.Add(previous); footer.Children.Add(pageInput); footer.Children.Add(pageStatus); footer.Children.Add(next);
            editorZoom = new ZoomController(viewport, "Zoom", .1, 5, () => viewport.ZoomFactor,
                scale => ApplyViewportZoom(viewport, scale),
                width => { if (width) viewport.ChangeView(null, null, (float)Math.Clamp(Math.Max(1, viewport.ActualWidth - 32) / sheet.Width, .1, 5), true); else Fit(); }, () => pdf is not null && !Working);
            viewport.ViewChanged += (_, e) =>
            {
                editorZoomIntermediate = e.IsIntermediate && (editorZoomIntermediate || Math.Abs(editorObservedZoom - viewport.ZoomFactor) > .000001);
                editorObservedZoom = viewport.ZoomFactor;
                QueueEditorScroll();
            };
            footer.Children.Add(editorZoom.Toolbar);
            var outputLocation = owner.BuildOutputLocation(); outputLocation.MaxWidth = 480;
            footer.Children.Add(outputLocation);
            var footerScroll = new ScrollViewer { Content = footer, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 160 };
            Grid.SetRow(footerScroll, 4); View.Children.Add(footerScroll);
            var toolsDrawerButton = HistoryButton("editor-shapes", "Alat edit", true); var toolsDrawer = new Flyout(); toolsDrawerButton.Flyout = toolsDrawer; actions.Children.Insert(0, toolsDrawerButton);
            View.SizeChanged += (_, _) =>
            {
                var shortView = View.ActualHeight < 500;
                View.Padding = new Thickness(shortView ? 8 : 16); View.RowSpacing = shortView ? 6 : 12;
                footerScroll.MaxHeight = shortView ? 90 : 160; notice.MaxLines = shortView ? 2 : 0; ToolTipService.SetToolTip(notice, notice.Text);
                toolsDrawerButton.Visibility = shortView ? Visibility.Visible : Visibility.Collapsed;
                if (shortView && toolbarScroll.Parent == View) { View.Children.Remove(toolbarScroll); toolbarScroll.Width = Math.Max(120, Math.Min(480, View.ActualWidth - 64)); toolbarScroll.MaxHeight = Math.Max(100, View.ActualHeight - 120); toolsDrawer.Content = toolbarScroll; }
                else if (!shortView && toolbarScroll.Parent != View) { toolsDrawer.Hide(); toolsDrawer.Content = null; toolbarScroll.Width = double.NaN; toolbarScroll.MaxHeight = 132; View.Children.Add(toolbarScroll); }
                var narrow = View.ActualWidth < 1050;
                foreach (var button in modes.Values.Concat(new[] { readSource, open })) if (button.Content is StackPanel labels) foreach (var label in labels.Children.OfType<TextBlock>()) label.Visibility = View.ActualWidth < 650 ? Visibility.Collapsed : Visibility.Visible;
                workspace.ColumnDefinitions[0].Width = new GridLength(narrow ? 0 : 132); thumbButton.Visibility = narrow ? Visibility.Visible : Visibility.Collapsed;
                if (narrow && thumbScroll.Parent == workspace) { workspace.Children.Remove(thumbScroll); thumbScroll.Width = 160; thumbScroll.MaxHeight = Math.Max(80, View.ActualHeight - 120); thumbDrawer.Content = thumbScroll; }
                else if (!narrow && thumbScroll.Parent != workspace) { thumbDrawer.Hide(); thumbDrawer.Content = null; thumbScroll.Width = double.NaN; thumbScroll.MaxHeight = double.PositiveInfinity; workspace.Children.Add(thumbScroll); }
                workspace.ColumnDefinitions[2].Width = new GridLength(narrow ? 0 : 280); drawerButton.Visibility = narrow ? Visibility.Visible : Visibility.Collapsed;
                if (narrow && propertiesScroll.Parent == workspace) { workspace.Children.Remove(propertiesScroll); propertiesScroll.Width = Math.Min(280, Math.Max(100, View.ActualWidth - 64)); propertiesScroll.MaxHeight = Math.Max(80, Math.Min(480, View.ActualHeight - 120)); drawer.Content = propertiesScroll; }
                else if (!narrow && propertiesScroll.Parent != workspace) { drawer.Hide(); drawer.Content = null; propertiesScroll.Width = double.NaN; propertiesScroll.MaxHeight = double.PositiveInfinity; workspace.Children.Add(propertiesScroll); }
            };
            sheet.PointerPressed += Pressed; sheet.PointerMoved += Moved; sheet.PointerReleased += Released;
            // Route editor shortcuts before ScrollViewer/TextBox consume keys; typing guards retain native editing.
            View.PreviewKeyDown += ObjectKeyDown;
            sheet.PointerCanceled += (_, _) => CancelGesture();
            sheet.PointerCaptureLost += (_, _) => CancelGesture();
            View.AllowDrop = true;
            View.DragOver += (_, e) => e.AcceptedOperation = !Working && e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
            View.Drop += async (_, e) =>
            {
                var deferral = e.GetDeferral();
                try { await AcceptFilesAsync(await e.DataView.GetStorageItemsAsync()); }
                catch (Exception) { notice.Text = "Dokumen belum dapat dibuka dari seret dan lepas."; }
                finally { deferral.Complete(); }
            };
            View.Unloaded += (_, _) => { if (!transitionRetainInput) { EndInlineText(); CancelGesture(); } };
            View.Loaded += (_, _) => { if (originalInlineDraft is { } draft && inlineText is null) ShowOriginalInline(draft); else Refresh(); };
            BuildProperties(); Refresh();
        }

        private void Fit()
        {
            var zoom = (float)Math.Clamp(Math.Min(Math.Max(1, viewport.ActualWidth - 32) / sheet.Width, Math.Max(1, viewport.ActualHeight - 32) / sheet.Height), .1, 5);
            viewport.ChangeView(0, editorTops.Length > page ? editorTops[page] * zoom : 0, zoom, true);
        }
        private void Snapshot() { past.Push(marks.ToArray()); if (past.Count > 50) { var keep = past.Take(50).Reverse().ToArray(); past.Clear(); foreach (var item in keep) past.Push(item); } future.Clear(); }
        internal void Restore(bool forward)
        {
            if (Working) return;
            if (originalInlineDraft is not null) { CancelOriginalInline(); return; }
            EndInlineText();
            CancelGesture();
            var from = forward ? future : past; var to = forward ? past : future;
            if (from.Count == 0) return;
            var originalBefore = System.Text.Json.JsonSerializer.Serialize(marks.Where(m => m.Kind == "replace-text"));
            if (originalBefore != System.Text.Json.JsonSerializer.Serialize(from.Peek().Where(m => m.Kind == "replace-text"))) { _ = RestoreOriginalHistoryAsync(forward); return; }
            to.Push(marks.ToArray()); marks.Clear(); marks.AddRange(from.Pop()); chosen = -1; BuildProperties(); Refresh();
        }
        private void Add(EditorMark mark)
        {
            if (marks.Count >= 100) { notice.Text = "Maksimal 100 objek per dokumen."; return; }
            Snapshot(); marks.Add(mark); chosen = marks.Count - 1; mode = "Pilih"; BuildProperties(); Refresh();
        }

        internal async Task PickAsync()
        {
            if (Working || owner.messageOpen) return;
            picking = true;
            try
            {
                var picker = new FileOpenPicker(); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(owner)); picker.FileTypeFilter.Add(".pdf");
                var file = await picker.PickSingleFileAsync(); picking = false;
                if (file is not null) await OpenAsync(file.Path);
            }
            catch (Exception) { notice.Text = "Pemilih PDF tidak dapat dibuka. Coba kembali."; }
            finally { picking = false; }
        }

        internal async Task OpenAsync(string input)
        {
            try { input = System.IO.Path.GetFullPath(input); }
            catch (Exception) { notice.Text = "Lokasi PDF tidak valid."; return; }
            if (Working || owner.messageOpen || string.Equals(path, input, StringComparison.OrdinalIgnoreCase)) return;
            if (Dirty)
            {
                owner.messageOpen = true;
                try { if (await new ContentDialog { XamlRoot = owner.root.XamlRoot, Title = "Ganti dokumen?", Content = "Draf editor yang belum disimpan akan dibuang. File asli tetap aman.", PrimaryButtonText = "Buang draf dan buka", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close }.ShowAsync() != ContentDialogResult.Primary) return; }
                finally { owner.messageOpen = false; }
            }
            FileStream? candidateLock = null;
            busy = true; Refresh();
            try
            {
                if (!System.IO.Path.IsPathFullyQualified(input) || input.StartsWith(@"\\", StringComparison.Ordinal) || !System.IO.Path.GetExtension(input).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) throw new IOException();
                candidateLock = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (candidateLock.Length > 100L * 1024 * 1024) throw new IOException();
                using var preflight = new CancellationTokenSource(); owner.active = preflight;
                PdfReply inspection;
                try { inspection = await WorkerClient.RunAsync(new("edit-check", [input], System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf")), preflight.Token); }
                finally { owner.active = null; }
                if (!inspection.Success || inspection.PageSizes is null) { notice.Text = inspection.Message; return; }
                var loaded = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(input));
                if (loaded.PageCount is 0 or > 1000 || loaded.IsPasswordProtected) throw new IOException();
                if (inspection.PageSizes.Length != loaded.PageCount) throw new IOException();
                ReleaseFiles(); sourceLock = candidateLock; candidateLock = null; pdf = loaded; path = input; pageSizes = inspection.PageSizes;
                pageReady = false; background.Source = null; resultButton.Visibility = Visibility.Collapsed; lastResult = null;
                marks.Clear(); past.Clear(); future.Clear(); savedState = State; chosen = -1; page = 0;
                InstallEditorPages();
                filename.Text = System.IO.Path.GetFileName(input); notice.Text = "PDF siap · Pilih Edit teks asli atau tambahkan objek. Draf tetap tersedia saat berpindah menu.";
            }
            catch (Exception error) { LocalDiagnostics.Record("editor-open", error); notice.Text = "PDF belum dapat dibuka. Gunakan PDF lokal tanpa kata sandi, maksimal 100 MB / 1000 halaman. Draf lama tetap tersedia."; }
            finally { candidateLock?.Dispose(); busy = false; }
            await RenderAsync(page); BuildProperties(); Refresh();
        }

        private async Task RenderAsync(int index, bool finishInline = true, bool scrollToPage = true)
        {
            if (pdf is null || busy || rendering || index < 0 || index >= pdf.PageCount) return;
            if (finishInline && !await CommitOriginalInlineAsync()) return;
            EndInlineText(); lastTextTarget = null;
            CancelGesture();
            rendering = true; Refresh();
            try
            {
                using var pdfPage = pdf.GetPage((uint)index);
                if (pdfPage.Size.Width is <= 0 or > 3000 || pdfPage.Size.Height is <= 0 or > 3000) throw new IOException();
                using var stream = new InMemoryRandomAccessStream();
                var ratio = pdfPage.Size.Height / pdfPage.Size.Width;
                var renderWidth = (uint)Math.Max(1, Math.Min(Math.Min(1800, 1800 / ratio), Math.Sqrt(2_000_000d / ratio)));
                await pdfPage.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = renderWidth });
                var bitmap = new BitmapImage(); stream.Seek(0); await bitmap.SetSourceAsync(stream);
                ActivateEditorPage(index); page = index; chosen = -1;
                // Use authoritative PDF point dimensions from the worker, not inferred renderer units.
                sheet.Width = pageSizes[index].Width; sheet.Height = pageSizes[index].Height;
                background.Width = sheet.Width; background.Height = sheet.Height; background.Source = bitmap;
                pageReady = true;
                pageStatus.Text = $"/ {pdf.PageCount}";
                updating = true; pageInput.Maximum = pdf.PageCount; pageInput.Value = page + 1; updating = false;
                thumbnails.Children.Clear(); thumbnails.Children.Add(HistoryText("Halaman", 14, true));
                for (var number = Math.Max(0, page - 2); number < Math.Min((int)pdf.PageCount, page + 3); number++)
                {
                    var target = number; var button = HistoryButton("file-text", $"{number + 1}", true);
                    BitmapImage thumbnail;
                    if (number == page) thumbnail = bitmap;
                    else
                    {
                        using var thumbPage = pdf.GetPage((uint)number); using var thumbStream = new InMemoryRandomAccessStream();
                        await thumbPage.RenderToStreamAsync(thumbStream, new PdfPageRenderOptions { DestinationWidth = 120 });
                        thumbnail = new BitmapImage(); thumbStream.Seek(0); await thumbnail.SetSourceAsync(thumbStream);
                    }
                    var tile = new StackPanel { Spacing = 4 }; tile.Children.Add(new Image { Source = thumbnail, Width = 90, Height = 120, Stretch = Stretch.Uniform }); tile.Children.Add(HistoryText($"{number + 1}", 12)); button.Content = tile;
                    if (number == page) { button.BorderBrush = Brush(8, 102, 232); button.BorderThickness = new Thickness(2); }
                    button.Click += async (_, _) => await RenderAsync(target); thumbnails.Children.Add(button);
                }
                if (initialEditorFit) { initialEditorFit = false; Fit(); }
                if (scrollToPage) viewport.ChangeView(null, editorTops[index] * viewport.ZoomFactor, null, true);
                BuildProperties();
                if (mode == "Edit teks asli") { await LoadOriginalRunsAsync(); BuildProperties(); }
            }
            catch (Exception) { notice.Text = "Halaman tidak dapat dirender. Draf tidak dihapus."; }
            finally { rendering = false; Refresh(); QueueEditorScroll(); }
        }

        private void Pressed(object sender, PointerRoutedEventArgs args)
        {
            if (inlineText is not null)
            {
                var element = args.OriginalSource as DependencyObject;
                while (element is not null) { if (element == inlineText) return; element = VisualTreeHelper.GetParent(element); }
                if (inlineOriginalRun is not null) { _ = CommitOriginalInlineAsync(); args.Handled = true; return; }
                EndInlineText();
            }
            if (mode == "Pilih" && !Working && args.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed) { chosen = -1; viewport.Focus(FocusState.Programmatic); BuildProperties(); Refresh(); return; }
            if (!pageReady || pdf is null || Working || mode is "Pilih" or "Edit teks asli" || args.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Touch || !args.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed) return;
            var p = args.GetCurrentPoint(sheet).Position;
            if (mode == "Bentuk")
            {
                viewport.Focus(FocusState.Programmatic);
                if (sheet.CapturePointer(args.Pointer)) BeginShape(p);
                args.Handled = true; return;
            }
            if (mode == "Gambar bebas") { if (sheet.CapturePointer(args.Pointer)) BeginFreehand(p); args.Handled = true; return; }
            PlaceObjectAt(p); args.Handled = true;
        }
        private void Moved(object sender, PointerRoutedEventArgs args)
        {
            if (drawingShape) { UpdateShape(args.GetCurrentPoint(sheet).Position, KeyDown(Windows.System.VirtualKey.Shift)); args.Handled = true; return; }
            if (dragging is { } original && chosen >= 0)
            {
                var position = args.GetCurrentPoint(sheet).Position;
                if (resizeHandle is null && marks[chosen] == original && Math.Abs(position.X - dragOrigin.X) * viewport.ZoomFactor < 4 && Math.Abs(position.Y - dragOrigin.Y) * viewport.ZoomFactor < 4) return;
                marks[chosen] = resizeHandle is { } handle
                    ? ResizeObject(original, handle, position.X - dragOrigin.X, position.Y - dragOrigin.Y, original.Kind == "image" || KeyDown(Windows.System.VirtualKey.Shift), sheet.Width, sheet.Height)
                    : original with { X = Math.Clamp(original.X + position.X - dragOrigin.X, 0, sheet.Width - original.Width), Y = Math.Clamp(original.Y + position.Y - dragOrigin.Y, 0, sheet.Height - original.Height) };
                Refresh(); args.Handled = true; return;
            }
            if (!drawing) return;
            UpdateFreehand(args.GetCurrentPoint(sheet).Position); args.Handled = true;
        }
        private void Released(object sender, PointerRoutedEventArgs args)
        {
            if (drawingShape)
            {
                UpdateShape(args.GetCurrentPoint(sheet).Position, KeyDown(Windows.System.VirtualKey.Shift));
                FinishShape(); sheet.ReleasePointerCapture(args.Pointer); args.Handled = true; return;
            }
            if (dragging is { } original && chosen >= 0)
            {
                var moved = marks[chosen]; dragging = null; resizeHandle = null;
                if (moved != original) { lastTextTarget = null; marks[chosen] = original; Snapshot(); marks[chosen] = moved; }
                sheet.ReleasePointerCapture(args.Pointer); BuildProperties(); Refresh(); args.Handled = true; return;
            }
            if (!drawing) return;
            UpdateFreehand(args.GetCurrentPoint(sheet).Position); FinishFreehand();
            sheet.ReleasePointerCapture(args.Pointer); args.Handled = true;
        }

        private void CancelGesture()
        {
            drawingShape = false; shapePreview = null;
            if (dragging is { } original && chosen >= 0) marks[chosen] = original;
            dragging = null; resizeHandle = null; drawing = false; stroke.Clear(); Refresh();
        }

        private void Refresh(bool passive = false)
        {
            editorZoom?.Sync();
            if (passive || inlineText is null) sheet.Children.Clear();
            else foreach (var child in sheet.Children.Where(c => c != inlineText).ToArray()) sheet.Children.Remove(child);
            sheet.Children.Insert(0, background);
            if (pdf is null) { var empty = HistoryText("Buka PDF atau tempel file dengan Ctrl+V", 20, true); Canvas.SetLeft(empty, 40); Canvas.SetTop(empty, 80); sheet.Children.Add(empty); }
            for (var i = 0; i < marks.Count; i++)
            {
                var mark = marks[i]; if (mark.Page != page || mark.Kind == "replace-text") continue;
                if (!passive && inlineText is not null && inlineIndex == i) continue;
                var index = i; var color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, Convert.ToByte(mark.Color.Substring(1, 2), 16), Convert.ToByte(mark.Color.Substring(3, 2), 16), Convert.ToByte(mark.Color.Substring(5, 2), 16)));
                FrameworkElement visual = mark.Kind switch
                {
                    "text" => new TextBlock { Text = mark.Text, FontFamily = new FontFamily("Arial"), FontSize = mark.Size, Foreground = color, TextWrapping = TextWrapping.NoWrap },
                    "ellipse" => new Ellipse { Stroke = color, StrokeThickness = mark.Size },
                    "line" => ShapeLine(mark),
                    "pen" => new Polyline { Points = new PointCollection(), Stroke = color, StrokeThickness = mark.Size },
                    "image" => new Image { Source = images[mark.ImageIndex - 1].Bitmap, Stretch = Stretch.Fill },
                    _ => new Rectangle { Stroke = color, StrokeThickness = mark.Size }
                };
                if (visual is Polyline polyline) foreach (var p in mark.Points ?? []) polyline.Points.Add(new Point(p.X, p.Y));
                if (visual is Line lineVisual) { lineVisual.Stroke = color; lineVisual.StrokeThickness = mark.Size; }
                // A sized Shape clips its stroke at its geometry bounds. Canvas keeps the
                // original point coordinates without clipping or stretching the pen path.
                if (visual is Polyline penPath)
                {
                    var penCanvas = new Canvas(); penCanvas.Children.Add(penPath); visual = penCanvas;
                }
                visual.Width = mark.Width; visual.Height = mark.Height; visual.IsHitTestVisible = false;
                var container = new Grid { Width = mark.Width, Height = mark.Height, Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent) }; container.Children.Add(visual);
                if (!passive && i == chosen) container.Children.Add(new Border { BorderBrush = Brush(8, 102, 232), BorderThickness = new Thickness(1), IsHitTestVisible = false });
                container.IsHitTestVisible = !passive;
                container.PointerPressed += (_, e) =>
                {
                    if (mode != "Pilih" || Working || !e.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed || e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Touch) return;
                    if (TextObjectPressed(index, e.GetCurrentPoint(sheet).Position, Environment.TickCount64)) { e.Handled = true; return; }
                    EndInlineText();
                    chosen = index; dragOrigin = e.GetCurrentPoint(sheet).Position;
                    viewport.Focus(FocusState.Programmatic); resizeHandle = null;
                    if (sheet.CapturePointer(e.Pointer)) dragging = marks[index];
                    BuildProperties(); Refresh(); e.Handled = true;
                };
                Canvas.SetLeft(container, mark.X); Canvas.SetTop(container, mark.Y); sheet.Children.Add(container);
            }
            if (passive) return;
            RefreshInactiveDrafts();
            if (inlineText is null && chosen >= 0 && chosen < marks.Count && marks[chosen].Page == page && marks[chosen].Kind != "replace-text" && mode == "Pilih") AddResizeHandles(marks[chosen]);
            if (inlineText is not null && !sheet.Children.Contains(inlineText)) sheet.Children.Add(inlineText);
            if (inlineText is not null) Canvas.SetZIndex(inlineText, 1000);
            OriginalOverlays();
            DrawShapePreview();
            if (drawing) { var line = new Polyline { Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, Convert.ToByte(penColor.Substring(1, 2), 16), Convert.ToByte(penColor.Substring(3, 2), 16), Convert.ToByte(penColor.Substring(5, 2), 16))), StrokeThickness = penSize, IsHitTestVisible = false }; foreach (var p in stroke) line.Points.Add(p); sheet.Children.Add(line); }
            save.IsEnabled = pdf is not null && (marks.Count > 0 || originalInlineDraft?.Changed == true) && !Working; undo.IsEnabled = past.Count > 0 && !Working; redo.IsEnabled = future.Count > 0 && !Working;
            previous.IsEnabled = pdf is not null && page > 0 && !Working; next.IsEnabled = pdf is not null && page + 1 < pdf.PageCount && !Working; pageInput.IsEnabled = pdf is not null && !Working;
            foreach (var entry in modes) { entry.Value.IsEnabled = pageReady && !Working; entry.Value.Background = mode == entry.Key ? Brush(219, 234, 254) : Brush(255, 255, 255); AutomationProperties.SetItemStatus(entry.Value, mode == entry.Key ? "Alat aktif" : ""); }
            properties.IsHitTestVisible = !busy && !rendering;
            if (path is not null)
            {
                filename.Text = System.IO.Path.GetFileName(path) + (Dirty ? " · Belum disimpan" : lastResult is null ? " · Siap" : " · Tersimpan");
                ToolTipService.SetToolTip(filename, System.IO.Path.GetFileName(path));
            }
        }

        private void BuildProperties()
        {
            var generation = ++propertyGeneration;
            if (mode == "Edit teks asli") { updating = true; properties.Children.Clear(); OriginalProperties(); updating = false; return; }
            updating = true; properties.Children.Clear(); properties.Children.Add(HistoryText(mode, 20, true));
            var pageObjects = Enumerable.Range(0, marks.Count).Where(i => marks[i].Page == page && marks[i].Kind != "replace-text").ToArray();
            if (pageObjects.Length > 0)
            {
                var objects = new ComboBox { Header = "Objek pada halaman ini", HorizontalAlignment = HorizontalAlignment.Stretch,
                    ItemsSource = pageObjects.Select((i, n) => $"{n + 1}. " + (marks[i].Kind switch { "text" => "Teks tambahan", "image" => "Gambar", "pen" => "Coretan", "rectangle" => "Kotak", "ellipse" => "Lingkaran", _ => "Garis" })).ToArray(), SelectedIndex = Array.IndexOf(pageObjects, chosen) };
                objects.SelectionChanged += (_, _) =>
                {
                    var index = objects.SelectedIndex;
                    if (generation != propertyGeneration || updating || Working || index < 0 || index >= pageObjects.Length) return;
                    var target = pageObjects[index];
                    if (target == chosen) return; // Initial selection must not replace the inspector under its TextChanged events.
                    chosen = target; mode = "Pilih"; BuildProperties(); Refresh();
                };
                properties.Children.Add(objects);
            }
            if (chosen < 0 || chosen >= marks.Count)
            {
                properties.Children.Add(HistoryText(mode == "Pilih" ? "Klik objek, lalu tarik pegangan untuk mengubah ukuran. Delete: hapus · panah: geser · Shift+panah: 10 pt · Ctrl+D: duplikasi. Gunakan Edit teks asli untuk tulisan sumber." : mode == "Gambar bebas" ? "Tekan dan geser pada halaman untuk membuat coretan." : mode == "Bentuk" ? "Klik-tahan lalu tarik pada halaman untuk menggambar. Lepaskan untuk selesai. Shift: persegi atau lingkaran sempurna. Esc: batal." : "Klik halaman untuk menempatkan objek, lalu atur di panel ini.", 13, muted: true));
                if (mode == "Bentuk") { var types = new ComboBox { Header = "Bentuk", ItemsSource = new[] { "Kotak", "Lingkaran", "Garis" }, SelectedIndex = shape == "ellipse" ? 1 : shape == "line" ? 2 : 0 }; types.SelectionChanged += (_, _) => shape = types.SelectedIndex == 1 ? "ellipse" : types.SelectedIndex == 2 ? "line" : "rectangle"; properties.Children.Add(types); }
                if (mode == "Gambar bebas") FreehandProperties(generation);
                updating = false; return;
            }
            var mark = marks[chosen];
            void Change(Func<EditorMark, EditorMark> apply)
            {
                if (generation != propertyGeneration || updating || Working || chosen < 0 || chosen >= marks.Count) return;
                var changed = apply(marks[chosen]);
                changed = changed with { X = Math.Clamp(changed.X, 0, sheet.Width - changed.Width), Y = Math.Clamp(changed.Y, 0, sheet.Height - changed.Height) };
                changed = changed with { Width = Math.Clamp(changed.Width, 1, sheet.Width - changed.X), Height = Math.Clamp(changed.Height, 1, sheet.Height - changed.Y) };
                var before = marks[chosen];
                if (before.Points is not null && (changed.Width != before.Width || changed.Height != before.Height))
                    changed = changed with { Points = before.Points.Select(p => new EditorPoint(p.X * changed.Width / before.Width, p.Y * changed.Height / before.Height)).ToArray() };
                if (changed == marks[chosen]) return;
                Snapshot(); marks[chosen] = changed; Refresh();
            }
            if (mark.Kind == "text")
            {
                var onPage = HistoryButton("editor-text", "Ketik langsung di halaman", true);
                var textIndex = chosen; onPage.Click += (_, _) => StartInlineText(textIndex); properties.Children.Add(onPage);
                var text = new TextBox { Header = "Teks tambahan · Arial", Text = mark.Text, MaxLength = 200 };
                text.TextChanged += (_, _) => Change(m => m with { Text = text.Text }); properties.Children.Add(text);
                properties.Children.Add(HistoryText("Satu baris, huruf Latin dasar. Bukan pengganti teks asli.", 12, muted: true));
            }
            void Number(string label, double value, double minimum, double maximum, Func<EditorMark, double, EditorMark> apply)
            {
                var number = new NumberBox { Header = label, Value = value, Minimum = minimum, Maximum = maximum, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
                number.ValueChanged += (_, e) => { if (double.IsFinite(e.NewValue)) Change(m => apply(m, e.NewValue)); }; properties.Children.Add(number);
            }
            Number("Kiri (pt)", mark.X, 0, sheet.Width - mark.Width, (m, v) => m with { X = v });
            Number("Atas (pt)", mark.Y, 0, sheet.Height - mark.Height, (m, v) => m with { Y = v });
            if (mark.Kind != "pen")
            {
                Number("Lebar (pt)", mark.Width, 1, sheet.Width - mark.X, (m, v) => m with { Width = v });
                Number("Tinggi (pt)", mark.Height, 1, sheet.Height - mark.Y, (m, v) => m with { Height = v });
            }
            if (mark.Kind != "image") Number(mark.Kind == "text" ? "Ukuran teks (pt)" : "Ketebalan (pt)", mark.Size, 1, 72, (m, v) => m with { Size = v });
            var colors = new ComboBox { Header = "Warna", ItemsSource = new[] { "Hitam", "Biru", "Merah", "Hijau", "Jingga", "Ungu" }, SelectedIndex = Array.IndexOf(PenColors, mark.Color) };
            colors.SelectionChanged += (_, _) => { if (colors.SelectedIndex >= 0) Change(m => m with { Color = PenColors[colors.SelectedIndex] }); }; if (mark.Kind != "image") properties.Children.Add(colors);
            var remove = HistoryButton("x", "Hapus · Delete", true); remove.Click += (_, _) => DeleteSelected(); properties.Children.Add(remove);
            var duplicate = HistoryButton("file-text", "Duplikasi objek", true);
            duplicate.Click += (_, _) => DuplicateSelected();
            properties.Children.Add(duplicate);
            updating = false;
        }

        internal async Task SaveAsync(string? testDirectory = null, bool recordHistory = true)
        {
            string? completedTarget = null; string completedDetail = "";
            if (drawingShape || drawing || dragging is not null) return;
            if (!await CommitOriginalInlineAsync()) return;
            if (path is null || busy || rendering || marks.Count == 0 || owner.active is not null) return;
            EndInlineText();
            busy = true; Refresh();
            try
            {
                var destination = testDirectory ?? owner.folder ?? owner.defaultFolder;
                if (destination is null || !Directory.Exists(destination))
                {
                    if (testDirectory is not null) { notice.Text = "Folder hasil tidak tersedia. Draf tetap tersimpan di editor."; return; }
                    notice.Text = "Folder hasil belum tersedia. Pilih lokasi penyimpanan.";
                    var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(owner));
                    var directory = await picker.PickSingleFolderAsync();
                    if (directory is null) { notice.Text = "Penyimpanan dibatalkan. Draf tetap tersedia."; return; }
                    destination = directory.Path; owner.folder = destination; owner.location.Text = "Folder hasil: " + destination;
                }
                var target = AvailableOutputPath(destination, System.IO.Path.GetFileNameWithoutExtension(path) + "_edit.pdf");
                var request = new PdfRequest("edit", new[] { path }.Concat(images.Select(i => i.Path)).ToArray(), target, Marks: marks.ToArray());
                if (System.Text.Json.JsonSerializer.Serialize(request).Length > 60000) { notice.Text = "Draf terlalu besar. Kurangi coretan sebelum menyimpan."; return; }
                using var cancellation = new CancellationTokenSource(); owner.active = cancellation;
                if (testDirectory is null && recordHistory) ShowSaving(cancellation);
                var cancel = HistoryButton("x", "Batalkan penyimpanan", true); cancel.Click += (_, _) => cancellation.Cancel(); properties.Children.Add(cancel); properties.IsHitTestVisible = true;
                notice.Text = "Menyimpan PDF baru secara lokal…";
                try
                {
                    var reply = await WorkerClient.RunAsync(request, cancellation.Token);
                    notice.Text = reply.Success ? "Tersimpan: " + target : reply.Message;
                    if (reply.Success)
                    {
                        completedTarget = target; completedDetail = $"{reply.Pages} halaman · {reply.Bytes / 1024d:N1} KB";
                        savedState = State; lastResult = target; resultButton.Visibility = Visibility.Visible; owner.resultPath = target;
                        if (testDirectory is null && recordHistory)
                        {
                            owner.savedJobs.Insert(0, new(Guid.NewGuid(), DateTimeOffset.Now, "edit", [target], $"{reply.Pages} halaman · perubahan editor"));
                            if (owner.savedJobs.Count > 200) owner.savedJobs.RemoveRange(200, owner.savedJobs.Count - 200); owner.SaveLibrary();
                        }
                    }
                }
                finally { owner.active = null; properties.Children.Remove(cancel); }
            }
            catch (Exception) { notice.Text = "Belum berhasil menyimpan. Draf tetap tersedia; file asli tidak diubah."; }
            finally
            {
                busy = false; HideSaveScreen(); Refresh();
                if (completedTarget is not null && testDirectory is null && recordHistory) ShowSaved(completedTarget, completedDetail);
            }
        }

        internal void ReleaseFiles()
        {
            // Retire queued inspector callbacks before removing their object model.
            propertyGeneration++; chosen = -1;
            editorScrollTimer.Stop(); editorScrollPending = false;
            editorZoomIntermediate = false; editorRetainedRange = (-1, -1);
            editorObservedZoom = viewport.ZoomFactor;
            foreach (var image in editorImages) image.Source = null;
            originalInlineDraft = null;
            EndInlineText(); lastTextTarget = null;
            ClearOriginalPreview(); originalRuns = []; originalId = -1;
            sourceLock?.Dispose(); sourceLock = null;
            marks.Clear(); past.Clear(); future.Clear();
            foreach (var image in images) image.Lock.Dispose(); images.Clear();
        }

        internal async Task PasteAsync()
        {
            if (Working) return;
            HideSaveScreen();
            try
            {
                var data = Clipboard.GetContent();
                if (!data.Contains(StandardDataFormats.StorageItems)) { notice.Text = "Salin satu file PDF dari File Explorer."; return; }
                await AcceptFilesAsync(await data.GetStorageItemsAsync());
            }
            catch (Exception) { notice.Text = "Clipboard belum dapat dibaca."; }
        }
        private async Task AcceptFilesAsync(IEnumerable<IStorageItem> items)
        {
            var list = items.OfType<StorageFile>().ToArray();
            if (list.Length != 1) { notice.Text = "Editor menerima satu PDF. Pilih satu dokumen."; return; }
            if (string.Equals(path, list[0].Path, StringComparison.OrdinalIgnoreCase)) { notice.Text = "Dokumen ini sudah terbuka di editor."; return; }
            if (path is not null && !Dirty)
            {
                owner.messageOpen = true;
                try
                {
                    if (await new ContentDialog { XamlRoot = View.XamlRoot, Title = "Ganti dokumen?", Content = "Buka PDF dari clipboard? File asli dan hasil sebelumnya tetap tersimpan.", PrimaryButtonText = "Ganti dokumen", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close }.ShowAsync() != ContentDialogResult.Primary) return;
                }
                finally { owner.messageOpen = false; }
            }
            await OpenAsync(list[0].Path);
        }
        private async Task CloseAsync()
        {
            if (Working || owner.messageOpen) return;
            if (RestartHasSession)
            {
                var resumeInput = PauseForReadPreview();
                owner.messageOpen = true;
                try
                {
                    var gesture = drawing || drawingShape || dragging is not null;
                    var dialog = new ContentDialog { XamlRoot = owner.root.XamlRoot, Title = "Tutup dokumen?",
                        Content = gesture ? "Selesaikan gerakan untuk menyimpan, atau tutup tanpa menyimpan draf editor. File asli tetap tersedia." : "Draf editor belum disimpan. Simpan hasil atau tutup tanpa menyimpan. File asli dan hasil tersimpan tetap tersedia.",
                        PrimaryButtonText = "Simpan", IsPrimaryButtonEnabled = !gesture, SecondaryButtonText = "Tutup tanpa menyimpan",
                        CloseButtonText = "Kembali mengedit", DefaultButton = ContentDialogButton.Close };
                    ConfigureDocumentCloseDialog(dialog); var answer = await dialog.ShowAsync();
                    if (answer == ContentDialogResult.None) return;
                    if (answer == ContentDialogResult.Primary)
                    {
                        var previousResult = lastResult; await SaveAsync();
                        if (RestartHasSession || lastResult is null || lastResult == previousResult || !File.Exists(lastResult)) return;
                    }
                }
                finally { owner.messageOpen = false; await resumeInput(); }
            }
            if (Working) return;
            if (path is not null) owner.chosenFiles.Remove(path);
            owner.closedToolSessions.Add("edit"); owner.toolDrafts.Remove("edit");
            HideSaveScreen(); CancelGesture(); pdf = null; path = null; ReleaseFiles(); pageSizes = []; pageReady = false; background.Source = null; savedState = State;
            lastResult = null; resultButton.Visibility = Visibility.Collapsed;
            chosen = -1; thumbnails.Children.Clear(); filename.Text = "Pilih PDF untuk mulai"; pageStatus.Text = "0 / 0"; resultButton.Visibility = Visibility.Collapsed;
            notice.Text = "Dokumen ditutup. File di komputer tetap tersedia."; BuildProperties(); Refresh();
            owner.ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "edit"));
        }

        private async Task PickImageAsync(string? testImage = null)
        {
            if (!pageReady || Working || images.Count >= 20) { notice.Text = "Buka PDF terlebih dahulu. Maksimal 20 gambar per draf."; return; }
            picking = true; Refresh(); FileStream? fileLock = null;
            try
            {
                StorageFile? file;
                if (testImage is null)
                {
                    var picker = new FileOpenPicker(); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(owner));
                    foreach (var extension in new[] { ".png", ".jpg", ".jpeg" }) picker.FileTypeFilter.Add(extension);
                    file = await picker.PickSingleFileAsync();
                }
                else file = await StorageFile.GetFileFromPathAsync(testImage);
                if (file is null) return;
                if (file.Path.StartsWith(@"\\", StringComparison.Ordinal)) throw new IOException();
                fileLock = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fileLock.Length > 10 * 1024 * 1024 || fileLock.Length + (sourceLock?.Length ?? 0) + images.Sum(i => i.Lock.Length) > 100 * 1024 * 1024) throw new IOException();
                using var stream = await file.OpenReadAsync(); var decoder = await BitmapDecoder.CreateAsync(stream);
                if ((long)decoder.PixelWidth * decoder.PixelHeight > 20000000 || decoder.PixelWidth == 0 || decoder.PixelHeight == 0 || decoder.FrameCount != 1) throw new IOException();
                var bitmap = new BitmapImage { DecodePixelWidth = 1200 }; stream.Seek(0); await bitmap.SetSourceAsync(stream);
                images.Add((file.Path, fileLock, bitmap)); fileLock = null;
                var scale = Math.Min((sheet.Width - 40) / decoder.PixelWidth, Math.Min((sheet.Height - 40) / decoder.PixelHeight, 250d / decoder.PixelWidth));
                Add(new(page, "image", 20, 20, decoder.PixelWidth * scale, decoder.PixelHeight * scale, ImageIndex: images.Count));
            }
            catch (Exception) { notice.Text = "Gunakan JPG/PNG lokal maksimal 10 MB dan 20 megapiksel. Total dokumen dan gambar maksimal 100 MB."; }
            finally { fileLock?.Dispose(); picking = false; Refresh(); }
        }
    }
}
