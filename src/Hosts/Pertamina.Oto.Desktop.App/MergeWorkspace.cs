using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private GridView? mergeTiles;
    private readonly Dictionary<string, int> mergeAngles = new(StringComparer.OrdinalIgnoreCase);
    private bool syncingMerge;
    private readonly Dictionary<string, StackPanel> mergeCards = new(StringComparer.OrdinalIgnoreCase);
    private TextBlock? mergeSummary;
    private StackPanel? mergeInput;
    private Border? mergeProcessing;
    private readonly Dictionary<UIElement, Visibility> mergeProcessingVisibility = new();

    private void SetMergeProcessing(bool busy, string? destinationPath = null)
    {
        foreach (var item in mergeProcessingVisibility) item.Key.Visibility = item.Value;
        mergeProcessingVisibility.Clear();
        if (mergeCancelButton is not null) mergeCancelButton.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (mergeReadiness is not null) mergeReadiness.Visibility = busy ? Visibility.Collapsed : Visibility.Visible;
        if (mergeInput is not null) mergeInput.Visibility = busy ? Visibility.Collapsed : Visibility.Visible;
        if (mergeProcessing is not null) body.Children.Remove(mergeProcessing);
        mergeProcessing = null;
        if (!busy) return;
        foreach (var item in body.Children.Skip(2))
        {
            mergeProcessingVisibility[item] = item.Visibility;
            item.Visibility = Visibility.Collapsed;
        }
        var panel = new StackPanel { Spacing = 18, Padding = new Thickness(24), HorizontalAlignment = HorizontalAlignment.Stretch };
        var biblo = MergeBiblo(144);
        biblo.HorizontalAlignment = HorizontalAlignment.Center;
        panel.Children.Add(biblo);
        panel.Children.Add(new TextBlock { Text = "Sedang menggabungkan PDF…", FontSize = 26, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });
        var detail = Copy(mergeCloseSaveInProgress
            ? "Memproses dan menyimpan hasil di perangkat ini."
            : $"Memproses {files.Count} PDF di perangkat Anda. Persentase proses belum tersedia.", 16);
        detail.TextAlignment = TextAlignment.Center; panel.Children.Add(detail);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(detail, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        var indicator = new ProgressBar { IsIndeterminate = true };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(indicator, "Sedang menggabungkan PDF");
        panel.Children.Add(indicator);
        if (mergeCloseSaveInProgress)
        {
            var destinationName = destinationPath is null ? outputName.Text.Trim() : Path.GetFileName(destinationPath);
            var destinationFolder = destinationPath is null ? folder : Path.GetDirectoryName(destinationPath);
            var destination = new TextBlock { Text = destinationName + " · " + (destinationFolder ?? "Folder hasil belum dipilih"),
                FontSize = 14, MaxLines = 2, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Stretch, TextAlignment = TextAlignment.Center };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(destination,
                "Nama file hasil: " + destinationName + ". Folder hasil: " + (destinationFolder ?? "Belum dipilih"));
            ToolTipService.SetToolTip(destination, destination.Text);
            panel.Children.Add(destination);
        }
        var stop = new Button { Content = "Batalkan proses", MinHeight = 44, Padding = new Thickness(18, 10, 18, 10), HorizontalAlignment = HorizontalAlignment.Center };
        stop.SetBinding(Control.IsEnabledProperty, new Microsoft.UI.Xaml.Data.Binding { Source = cancel,
            Path = new PropertyPath("IsEnabled"), Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay });
        stop.Click += (_, _) => { active?.Cancel(); stop.IsEnabled = false; detail.Text = "Membatalkan proses…"; };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(stop, "Batalkan proses");
        stop.MinWidth = 44;
        panel.Children.Add(stop);
        mergeProcessing = new Border { Child = panel, Background = Brush(255, 255, 255), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(20) };
        if (mergeCloseSaveInProgress)
        {
            mergeProcessing.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            mergeProcessing.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
            foreach (var text in panel.Children.OfType<TextBlock>())
                text.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        }
        body.Children.Insert(2, mergeProcessing);
        body.UpdateLayout();
        standardPageScroll?.ChangeView(null, 0, null, true);
        mergeProcessing.Loaded += (_, _) => DispatcherQueue.TryEnqueue(() =>
        {
            if (mergeProcessing is null) return;
            body.UpdateLayout();
            stop.Focus(FocusState.Programmatic);
            standardPageScroll?.ChangeView(null, 0, null, true);
        });
    }

    private void ShowMergeResult(Pertamina.Oto.LocalPdf.PdfReply reply)
    {
        if (selected?.Id == "merge" && mergeCloseSaveInProgress) return;
        if (resultPath is not null) ShowCurrentResult([resultPath], $"{reply.Pages} halaman · {reply.Bytes / 1024d:N1} KB");
    }

    private void BuildMergeWorkspace(StackPanel inputArea)
    {
        inputArea.Children.Clear();
        mergeInput = inputArea;
        mergeCards.Clear();
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14 };
        heading.Children.Add(MergeBiblo(72));
        var headingText = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        headingText.Children.Add(new TextBlock { Text = "Gabungkan PDF", FontSize = 22,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        headingText.Children.Add(Copy("Susun dokumen sesuai urutan hasil gabungan.", 13));
        heading.Children.Add(headingText);
        inputArea.Children.Add(heading);
        inputArea.AllowDrop = true;
        inputArea.DragOver += (_, e) =>
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            { e.AcceptedOperation = active is null ? DataPackageOperation.Copy : DataPackageOperation.None; e.Handled = true; }
        };
        inputArea.Drop += async (_, e) =>
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            e.Handled = true; var deferral = e.GetDeferral();
            try { await AcceptMergeFilesAsync(await e.DataView.GetStorageItemsAsync()); }
            catch (Exception) { status.Text = "File tidak dapat dibaca. Coba pilih PDF kembali."; }
            finally { deferral.Complete(); }
        };
        var toolbar = new ResponsiveWrapPanel { Spacing = 6 };
        toolbar.Children.Add(ActionButton("＋ Tambah file", async () => await PickFilesAsync()));
        var paste = ActionButton("Tempel", async () => await PasteMergeAsync());
        var shortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.V, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) =>
        {
            if (mergeInput?.IsLoaded != true || selected?.Id != "merge" || active is not null) return;
            e.Handled = true; await PasteMergeAsync();
        };
        toolbar.Children.Add(paste); // Ctrl+V is handled by the window root.
        toolbar.Children.Add(ActionButton("Urut A–Z", () => SortMergeFiles()));
        inputArea.Children.Add(toolbar);
        mergeSummary = Copy("", 14); inputArea.Children.Add(mergeSummary);
        inputArea.Children.Add(Copy("Seret kartu untuk mengatur urutan · Jatuhkan PDF di area ini untuk menambah", 12));
        mergeTiles = new GridView { SelectionMode = ListViewSelectionMode.None, IsItemClickEnabled = false, MinHeight = 260, CanDragItems = true, CanReorderItems = true, AllowDrop = true };
        mergeTiles.DragItemsStarting += (_, e) => e.Cancel = active is not null;
        mergeTiles.DragItemsCompleted += (_, _) =>
        {
            if (active is not null || mergeTiles is null) return;
            var order = mergeTiles.Items.Cast<StackPanel>().Select(t => (string)t.Tag).ToArray();
            syncingMerge = true;
            try { for (var n = 0; n < order.Length; n++) files.Move(files.IndexOf(order[n]), n); }
            finally { syncingMerge = false; }
            RefreshMergeTiles();
        };
        editable.Add(mergeTiles); inputArea.Children.Add(ZoomablePreview(mergeTiles));
        mergeTiles.Loaded += (_, _) => RefreshMergeTiles();
        foreach (var control in new UIElement[] { outputName, password, run, cancel, openResult, progress, status })
            DetachOwned(toolOptions, control);
        toolOptions.Children.Clear();
        toolOptions.Children.Add(BuildMergeNameInput());
        toolOptions.Children.Add(BuildMergeOutputLocation());
        password.Visibility = Visibility.Visible;
        toolOptions.Children.Add(new Expander { Header = MergeLabel("\uE72E", "PDF dengan kata sandi"), Content = password, HorizontalAlignment = HorizontalAlignment.Stretch });
        toolOptions.Children.Add(new Border { Height = 1, Background = Brush(191, 216, 248) });
        toolOptions.Children.Add(BuildMergeReadiness());
        toolOptions.Children.Add(BuildMergeRunButton());
        toolOptions.Children.Add(BuildMergeCancelButton());
        toolOptions.Children.Add(progress);
        toolOptions.Children.Add(BuildMergeStatus());
        toolOptions.Children.Add(MergeLabel("\uE73E", "File asli tidak ditimpa.", true));
        toolOptions.Children.Add(BuildMergeOrganizerLink());
        run.Content = "Gabungkan PDF";
        run.HorizontalAlignment = HorizontalAlignment.Stretch;
        RefreshMergeTiles();
    }

    private void SortMergeFiles()
    {
        foreach (var path in files.OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase).ToArray())
            files.Move(files.IndexOf(path), files.Count - 1);
    }

    private async Task AcceptMergeFilesAsync(IEnumerable<IStorageItem> items)
    {
        if (active is not null || selected?.Id != "merge") return;
        var incoming = new List<string>();
        long total = 0;
        foreach (var path in files) total += new FileInfo(path).Length;
        foreach (var file in items.OfType<StorageFile>())
        {
            if (!file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(file.Path)
                || file.Path.StartsWith(@"\\", StringComparison.Ordinal)
                || files.Contains(file.Path, StringComparer.OrdinalIgnoreCase)
                || incoming.Contains(file.Path, StringComparer.OrdinalIgnoreCase)) continue;
            total += checked((long)(await file.GetBasicPropertiesAsync()).Size);
            if (files.Count + incoming.Count >= 50 || total > 100 * 1024 * 1024)
            { status.Text = "Maksimal 50 PDF dengan total 100 MB. File tambahan belum dimasukkan."; return; }
            incoming.Add(file.Path);
        }
        foreach (var path in incoming) files.Add(path);
        if (incoming.Count == 0) status.Text = "Tidak ada PDF baru. Pilih file PDF lokal yang belum ada dalam daftar.";
    }

    private void UpdateMergeReady()
    {
        run.IsEnabled = active is null && files.Count >= 2 && folder is not null;
        UpdateMergePanelReadiness();
    }

    private async Task PasteMergeAsync()
    {
        if (active is not null) return;
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { status.Text = "Salin file PDF dari File Explorer terlebih dahulu."; return; }
            await AcceptMergeFilesAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { status.Text = "Clipboard tidak dapat dibaca."; }
    }

    private void RefreshMergeTiles()
    {
        if (mergeTiles is null || selected?.Id != "merge" || syncingMerge) return;
        foreach (var path in mergeCards.Keys.Where(p => !files.Contains(p)).ToArray())
        {
            mergeTiles.Items.Remove(mergeCards[path]); mergeCards.Remove(path);
        }
        for (var i = 0; i < files.Count; i++)
        {
            var path = files[i];
            if (!mergeCards.TryGetValue(path, out var tile))
            {
                tile = CreateMergeCard(path, i);
                mergeCards.Add(path, tile);
            }
            var current = mergeTiles.Items.IndexOf(tile);
            if (current == i) continue;
            if (current >= 0) mergeTiles.Items.RemoveAt(current);
            mergeTiles.Items.Insert(i, tile);
        }
        if (mergeSummary is not null) mergeSummary.Text = $"{files.Count} dokumen · Urutan kiri ke kanan";
        UpdateMergeReady();
        status.Text = files.Count < 2 ? "Tambahkan satu PDF lagi untuk menggabungkan." : folder is null ? "Pilih lokasi hasil melalui ikon pengaturan." : "Siap digabungkan. File asli tidak berubah.";
    }

    private StackPanel CreateMergeCard(string path, int color)
    {
        var tile = new StackPanel { Tag = path, Width = 184, Spacing = 8, Padding = new Thickness(10),
            Background = Brush(255, 255, 255), CornerRadius = new CornerRadius(8) };
        var image = new Image { Height = 190, Stretch = Stretch.Uniform, RenderTransformOrigin = new Windows.Foundation.Point(.5, .5),
            RenderTransform = new RotateTransform { Angle = mergeAngles.GetValueOrDefault(path) } };
        var sheet = new Grid();
        sheet.Children.Add(image);
        var actions = CreateMergeHoverActions(tile,
            () => active is null,
            () =>
        {
            mergeAngles[path] = (mergeAngles.GetValueOrDefault(path) + 90) % 360;
            ((RotateTransform)image.RenderTransform).Angle = mergeAngles[path];
        }, () => RemoveToolInput(path),
            async () => await ShowReadPreviewDialogAsync(path,
                explanation: "Pratinjau sumber. Rotasi pada kartu diterapkan saat penggabungan; file asli tidak berubah."));
        sheet.Children.Add(actions);
        tile.Children.Add(new Border { Child = sheet, BorderBrush = OrganizerColor(color), BorderThickness = new Thickness(2),
            Padding = new Thickness(4), CornerRadius = new CornerRadius(6) });
        var name = Copy(Path.GetFileName(path), 12); name.MaxLines = 1; name.TextTrimming = TextTrimming.CharacterEllipsis;
        tile.Children.Add(name);
        ToolTipService.SetToolTip(name, Path.GetFileName(path));
        ToolTipService.SetToolTip(tile, Path.GetFileName(path));
        bool loaded = false;
        image.Loaded += async (_, _) =>
        {
            if (loaded) return;
            loaded = true;
            var info = Copy("", 12);
            var count = await RenderPreviewAsync(path, 0, image, info, 400, password.Password);
            string size;
            try { size = $"{new FileInfo(path).Length / 1024d:N0} KB"; } catch (IOException) { size = "Ukuran tidak tersedia"; }
            ToolTipService.SetToolTip(tile, $"{Path.GetFileName(path)}\n{count} halaman · {size}");
        };
        var left = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.Left, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        var right = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.Right, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        void MoveCard(int delta) { var index = files.IndexOf(path); if (active is null && index >= 0 && index + delta >= 0 && index + delta < files.Count) files.Move(index, index + delta); }
        left.Invoked += (_, e) => { e.Handled = true; MoveCard(-1); };
        right.Invoked += (_, e) => { e.Handled = true; MoveCard(1); };
        tile.KeyboardAccelerators.Add(left); tile.KeyboardAccelerators.Add(right);
        return tile;
    }
}
