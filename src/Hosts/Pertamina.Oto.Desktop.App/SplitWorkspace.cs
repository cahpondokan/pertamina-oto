using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private SplitMode splitMode;
    private readonly List<SplitRange> splitRanges = [];
    private int splitStep = 1;
    private string splitSelection = "";
    private bool splitCombine;
    private int splitCount;
    private string? splitSource;
    private int splitGeneration;
    private int[][] splitGroups = [];
    private StackPanel splitFields = new();
    private TextBlock splitSummary = new();
    private TextBlock splitError = new();
    private TextBlock splitFile = new();
    private Button splitRun = new();
    private Button splitCancel = new();
    private ProgressBar splitProgress = new();
    private StackPanel splitResults = new();
    private PasswordBox splitPassword = new();
    private ContentControl splitInputControls = new();
    private ContentControl splitOptionControls = new();
    private TextBox? splitSelectionBox;
    private readonly List<(CheckBox Check, Border Border, TextBlock Group)> splitTiles = [];
    private bool splitUpdating;
    private readonly List<ToggleButton> splitTabs = [];
    private Button splitFolderButton = new();

    private static Border SplitFrame(UIElement content) => new() { Child = content, Padding = new Thickness(16),
        Background = Brush(255, 255, 255), CornerRadius = new CornerRadius(12), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) };

    internal async Task OpenSplitFileAsync(string path)
    {
        openingEmptyInput = true;
        try
        {
            ShowTool(ToolCatalog.All.First(tool => tool.Id == "split"));
            files.Clear(); files.Add(Path.GetFullPath(path));
            await RefreshSplitSourceAsync();
        }
        finally { openingEmptyInput = false; }
    }

    private void BuildSplitWorkspace(StackPanel inputArea)
    {
        splitPreviewKey = "";
        toolOptions.Visibility = Visibility.Visible;
        inputArea.Children.Clear();
        // Detach the generic actions; this workspace has a dedicated batch executor.
        if (run.Parent is Panel old) old.Children.Clear();
        toolOptions.Children.Clear();
        splitResults = new StackPanel { Spacing = 8 };
        var input = new StackPanel { Spacing = 12 };
        splitFile = Copy("Pilih PDF untuk memulai.", 15); input.Children.Add(splitFile);
        var replace = new Button { Content = "Ganti PDF", HorizontalAlignment = HorizontalAlignment.Right };
        replace.Click += async (_, _) => await PickFilesAsync(); input.Children.Add(replace);
        sourcePreviewHost = new StackPanel { Spacing = 10 };
        input.Children.Add(sourcePreviewHost);
        splitInputControls = new ContentControl { Content = input, HorizontalContentAlignment = HorizontalAlignment.Stretch };
        inputArea.Children.Add(splitInputControls); inputArea.Children.Add(splitResults);
        sourcePreviewHost.Loaded += (_, _) => _ = RefreshSplitSourceAsync();

        var options = new StackPanel { Spacing = 12 };
        var tabs = new Grid { ColumnSpacing = 6 };
        tabs.ColumnDefinitions.Add(new ColumnDefinition()); tabs.ColumnDefinitions.Add(new ColumnDefinition()); tabs.ColumnDefinitions.Add(new ColumnDefinition());
        splitTabs.Clear();
        foreach (var (label, index) in new[] { ("Rentang", 0), ("Halaman", 1), ("Ukuran", 2) })
        {
            var visual = new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
            visual.Children.Add(new FontIcon { Glyph = index == 0 ? "\uE8A5" : index == 1 ? "\uE8B3" : "\uE9D9", FontSize = 26 });
            // Inherit checked/disabled foreground from the native toggle state.
            visual.Children.Add(new TextBlock { Text = label, FontSize = 13, TextWrapping = TextWrapping.Wrap });
            if (index == 2) visual.Children.Add(Copy("Belum tersedia", 10));
            var tab = new ToggleButton { Content = visual, HorizontalAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(5, 12, 5, 12), IsEnabled = index != 2 };
            if (index == 2) ToolTipService.SetToolTip(tab, "Belum tersedia: pembagian berdasarkan batas KB/MB. Bukan kompresi.");
            tab.Click += (_, _) => { splitMode = index == 0 ? SplitMode.Custom : SplitMode.AllPages; splitCombine = false; BuildSplitFields(); };
            Grid.SetColumn(tab, index); tabs.Children.Add(tab); splitTabs.Add(tab);
        }
        options.Children.Add(tabs);
        splitFields = new StackPanel { Spacing = 10 }; options.Children.Add(splitFields);
        splitError = Copy("", 12); splitError.Foreground = Brush(180, 30, 45); options.Children.Add(splitError);
        splitSummary = Copy("", 13);
        splitPassword = new PasswordBox { Header = "Kata sandi pemilik (jika diperlukan)", MaxLength = 127 };
        var locked = new StackPanel { Spacing = 8 }; locked.Children.Add(splitPassword);
        var reload = new Button { Content = "Buka ulang PDF" };
        reload.Click += async (_, _) => await RefreshSplitSourceAsync(); locked.Children.Add(reload);
        options.Children.Add(new Expander { Header = "PDF dengan kata sandi", Content = locked, HorizontalAlignment = HorizontalAlignment.Stretch });
        options.Children.Add(new Expander { Header = "Batasan versi pengembangan", HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = Copy("Gunakan salinan uji. Maksimal 100 MB dan 1000 halaman hasil. Form, bookmark, tautan, dan tanda tangan digital perlu diperiksa kembali pada hasil.", 12) });
        splitOptionControls = new ContentControl { Content = options, HorizontalContentAlignment = HorizontalAlignment.Stretch };
        var optionsScroll = new ScrollViewer { Content = splitOptionControls, MaxHeight = 290, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        inputArea.SizeChanged += (_, _) => optionsScroll.MaxHeight = toolWorkspace.ActualWidth < 780
            ? double.PositiveInfinity : Math.Clamp(root.ActualHeight - 510, 320, 420);
        toolOptions.Children.Add(optionsScroll);
        toolOptions.Children.Add(new Border { Child = splitSummary, Padding = new Thickness(10), Background = Brush(225, 241, 255), CornerRadius = new CornerRadius(8) });
        var folderRow = new StackPanel { Spacing = 4 };
        var folderLabel = Copy("", 12);
        folderLabel.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Source = location, Path = new PropertyPath("Text") });
        folderRow.Children.Add(folderLabel);
        var changeFolder = new Button { Content = "Ubah folder hasil" };
        splitFolderButton = changeFolder;
        changeFolder.Click += async (_, _) => { await PickFolderAsync(); UpdateSplitPlan(); };
        folderRow.Children.Add(changeFolder); toolOptions.Children.Add(folderRow);
        splitRun = new Button { Content = "Pisahkan PDF", HorizontalAlignment = HorizontalAlignment.Stretch, MinHeight = 52,
            Background = Brush(22, 93, 222), Foreground = Brush(255, 255, 255), FontSize = 17, CornerRadius = new CornerRadius(8) };
        splitRun.Click += async (_, _) => await ExecuteSplitAsync();
        splitCancel = new Button { Content = "Batalkan", Visibility = Visibility.Collapsed, HorizontalAlignment = HorizontalAlignment.Stretch };
        splitCancel.Click += (_, _) => active?.Cancel();
        splitProgress = new ProgressBar { Visibility = Visibility.Collapsed, Minimum = 0, Maximum = 100 };
        toolOptions.Children.Add(splitRun); toolOptions.Children.Add(splitCancel); toolOptions.Children.Add(splitProgress); toolOptions.Children.Add(status);
        toolOptions.Children.Add(Copy("Lokal · Tanpa upload · File asli tetap aman", 12));
        BuildSplitFields();
    }

    private void BuildSplitFields()
    {
        splitFields.Children.Clear(); splitSelectionBox = null;
        var ranges = splitMode is SplitMode.Custom or SplitMode.Fixed;
        for (var i = 0; i < splitTabs.Count; i++) splitTabs[i].IsChecked = i < 2 && ranges == (i == 0);
        var choices = new Grid { ColumnSpacing = 6 };
        choices.ColumnDefinitions.Add(new ColumnDefinition()); choices.ColumnDefinitions.Add(new ColumnDefinition());
        var labels = ranges ? new[] { "Rentang sendiri", "Setiap N halaman" } : new[] { "Semua halaman", "Pilih halaman" };
        for (var j = 0; j < 2; j++)
        {
            var index = j;
            var choice = new ToggleButton { Content = new TextBlock { Text = labels[j], TextWrapping = TextWrapping.Wrap },
                HorizontalAlignment = HorizontalAlignment.Stretch, IsChecked = j == (splitMode is SplitMode.Fixed or SplitMode.SelectedPages ? 1 : 0) };
            choice.Click += (_, _) =>
            {
                splitMode = ranges ? (index == 0 ? SplitMode.Custom : SplitMode.Fixed) : (index == 0 ? SplitMode.AllPages : SplitMode.SelectedPages);
                splitCombine = false; BuildSplitFields();
            };
            Grid.SetColumn(choice, j); choices.Children.Add(choice);
        }
        splitFields.Children.Add(choices);
        if (splitMode == SplitMode.Custom)
        {
            for (var i = 0; i < splitRanges.Count; i++)
            {
                var index = i; var range = splitRanges[i];
                var row = new StackPanel { Spacing = 6 }; row.Children.Add(Copy($"Rentang {i + 1}", 13));
                var from = new NumberBox { Header = "Dari halaman", Value = range.First, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
                var to = new NumberBox { Header = "Sampai halaman", Value = range.Last, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
                void Changed() { splitRanges[index] = new(SplitInteger(from.Value), SplitInteger(to.Value)); UpdateSplitPlan(); }
                from.ValueChanged += (_, _) => Changed(); to.ValueChanged += (_, _) => Changed();
                var bounds = new Grid { ColumnSpacing = 8 }; bounds.ColumnDefinitions.Add(new ColumnDefinition()); bounds.ColumnDefinitions.Add(new ColumnDefinition());
                bounds.Children.Add(from); Grid.SetColumn(to, 1); bounds.Children.Add(to); row.Children.Add(bounds);
                var remove = new Button { Content = "Hapus rentang", IsEnabled = splitRanges.Count > 1 };
                remove.Click += (_, _) => { splitRanges.RemoveAt(index); BuildSplitFields(); }; row.Children.Add(remove);
                splitFields.Children.Add(new Border { Child = row, Padding = new Thickness(10), Background = Brush(242, 245, 250), CornerRadius = new CornerRadius(8) });
            }
            var add = new Button { Content = "+ Tambah Rentang", IsEnabled = splitRanges.Count < 1000 };
            add.Click += (_, _) => { var first = Math.Min(splitCount, splitRanges.LastOrDefault()?.Last + 1 ?? 1); splitRanges.Add(new(first, splitCount)); BuildSplitFields(); };
            splitFields.Children.Add(add);
        }
        if (splitMode == SplitMode.Fixed)
        {
            var step = new NumberBox { Header = "Jumlah halaman per PDF", Value = splitStep, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
            step.ValueChanged += (_, _) => { splitStep = SplitInteger(step.Value); UpdateSplitPlan(); }; splitFields.Children.Add(step);
        }
        if (splitMode == SplitMode.SelectedPages)
        {
            splitSelectionBox = new TextBox { Header = "Halaman yang dipilih", PlaceholderText = "Contoh: 1, 3, 5–7", Text = splitSelection };
            splitSelectionBox.TextChanged += (_, _) => { splitSelection = splitSelectionBox.Text; UpdateSplitPlan(); };
            splitFields.Children.Add(splitSelectionBox); splitFields.Children.Add(Copy("Klik centang pada preview atau ketik nomor halaman. Urutan hasil mengikuti dokumen.", 12));
        }
        if (splitMode is SplitMode.Custom or SplitMode.SelectedPages)
        {
            var combine = new CheckBox { Content = new TextBlock { Text = splitMode == SplitMode.Custom ? "Jadikan semua rentang satu PDF" : "Jadikan halaman terpilih satu PDF", TextWrapping = TextWrapping.Wrap }, IsChecked = splitCombine };
            combine.Click += (_, _) => { splitCombine = combine.IsChecked == true; UpdateSplitPlan(); }; splitFields.Children.Add(combine);
        }
        UpdateSplitPlan();
    }

    private static int SplitInteger(double value) => double.IsFinite(value) && value >= 1 && value <= 1000 && value == Math.Truncate(value) ? (int)value : 0;

    private void UpdateSplitPlan()
    {
        splitGroups = []; splitError.Text = "";
        try
        {
            if (splitCount == 0) throw new ArgumentException("Pilih PDF yang dapat dibuka untuk melanjutkan.");
            splitGroups = SplitPlan.Create(splitCount, splitMode, splitRanges, splitStep, splitSelection, splitCombine);
            splitSummary.Text = $"{splitGroups.Length} PDF akan dibuat · {splitGroups.Sum(group => group.Length)} halaman\n" +
                string.Join("\n", splitGroups.Take(3).Select((group, i) => $"Bagian {i + 1}: {SplitPlan.Describe(group)}")) +
                (splitGroups.Length > 3 ? $"\n… dan {splitGroups.Length - 3} bagian lainnya." : "");
            if (folder is null) splitError.Text = "Pilih folder hasil terlebih dahulu.";
        }
        catch (ArgumentException error) { splitError.Text = error.Message; splitSummary.Text = "Perbaiki pilihan untuk melihat jumlah hasil."; }
        splitRun.Content = splitGroups.Length == 0 ? "Pisahkan PDF" : $"Pisahkan • {splitGroups.Length} PDF";
        splitRun.IsEnabled = active is null && splitGroups.Length > 0 && folder is not null;
        RefreshSplitDesign();
        splitUpdating = true;
        try
        {
            for (var i = 0; i < splitTiles.Count; i++)
            {
                var tile = splitTiles[i]; var page = i + 1;
                var groups = splitGroups.Select((group, index) => (group, index)).Where(item => item.group.Contains(page)).Select(item => item.index + 1).ToArray();
                tile.Check.IsChecked = groups.Length > 0;
                tile.Check.IsEnabled = active is null;
                tile.Check.IsHitTestVisible = splitMode == SplitMode.SelectedPages;
                tile.Check.IsTabStop = splitMode == SplitMode.SelectedPages;
                tile.Border.BorderBrush = Brush(226, 231, 240);
                tile.Group.Text = groups.Length > 0 ? "Bagian " + string.Join(", ", groups.Take(5)) + (groups.Length > 5 ? "…" : "") : "Tidak diambil";
            }
        }
        finally { splitUpdating = false; }
    }

    private async Task RefreshSplitSourceAsync()
    {
        if (selected?.Id != "split" || sourcePreviewHost is null || active is not null) return;
        var generation = ++splitGeneration; var host = sourcePreviewHost;
        var source = files.FirstOrDefault(); var secret = splitPassword.Password;
        splitPreviewKey = ""; splitPreviewBitmaps.Clear();
        splitCount = 0; splitTiles.Clear(); host.Children.Clear(); splitResults.Children.Clear(); UpdateSplitPlan();
        if (source is null) { splitFile.Text = "Pilih PDF untuk memulai."; return; }
        splitFile.Text = Path.GetFileName(source) + " · Memuat halaman…";
        try
        {
            if (source.StartsWith(@"\\", StringComparison.Ordinal)) throw new IOException();
            var (file, properties) = await OpenPreviewStorageAsync(source);
            if (properties.Size is 0 or > 104857600) throw new IOException();
            var pdf = await PdfDocument.LoadFromFileAsync(file, secret);
            if (generation != splitGeneration || selected?.Id != "split" || host != sourcePreviewHost) return;
            if (pdf.PageCount is 0 or > 1000) throw new IOException();
            splitCount = (int)pdf.PageCount;
            if (source != splitSource)
            {
                splitSource = source; splitMode = SplitMode.Custom; splitCombine = false; splitSelection = ""; splitStep = Math.Min(3, splitCount);
                splitRanges.Clear(); splitRanges.Add(new(1, splitCount));
            }
            splitFile.Text = $"{Path.GetFileName(source)} · {splitCount} halaman";
            splitPreviewSecret = secret;
            BuildSplitFields(); status.Text = "Atur pilihan, periksa preview, lalu pisahkan.";
        }
        catch (Exception)
        {
            if (generation != splitGeneration || selected?.Id != "split") return;
            splitFile.Text = Path.GetFileName(source); splitCount = 0; UpdateSplitPlan();
            status.Text = "PDF tidak dapat dibuka. Periksa kata sandi, kondisi file, dan batas 100 MB / 1000 halaman.";
        }
    }

    private async Task ExecuteSplitAsync()
    {
        UpdateSplitPlan();
        if (!splitRun.IsEnabled || splitSource is null || folder is null || active is not null) return;
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        splitRun.IsEnabled = false; splitInputControls.IsEnabled = false; splitOptionControls.IsEnabled = false; splitFolderButton.IsEnabled = false;
        globalSearch.IsEnabled = false; foreach (var item in navigation) item.IsEnabled = false;
        splitCancel.Visibility = Visibility.Visible; splitProgress.Visibility = Visibility.Visible; splitResults.Children.Clear();
        var saved = false;
        try
        {
            var result = await SplitExport.RunAsync(splitSource, folder, splitPassword.Password, splitGroups, cancellation.Token, (done, total) =>
            {
                splitProgress.Value = done * 100d / total;
                status.Text = done == total ? "Menyelesaikan hasil…" : $"Memproses PDF {done + 1} dari {total} · {done} selesai";
            });
            status.Text = result.Message + (result.CleanupPending ? " File sementara mungkin tertinggal di folder hasil." : "");
            if (result.Success)
            {
                saved = true;
                MarkRestartToolSaved("split");
                ShowCurrentResult(result.Files, $"{result.Files.Length} PDF · {splitGroups.Sum(g => g.Length)} halaman");
                resultPath = result.Files[0]; history.Insert(0, $"Pisahkan PDF — {result.Files.Length} PDF — selesai lokal");
                if (selected is not null) completedActivities.Insert(0, (selected, Path.GetFileName(result.Files[0])));
            }
        }
        finally
        {
            active = null; if (saved) splitPassword.Password = ""; splitInputControls.IsEnabled = true; splitOptionControls.IsEnabled = true; splitFolderButton.IsEnabled = true;
            globalSearch.IsEnabled = true; foreach (var item in navigation) item.IsEnabled = true;
            splitCancel.Visibility = Visibility.Collapsed; splitProgress.Visibility = Visibility.Collapsed; UpdateSplitPlan();
        }
    }
}
