using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed class MainWindow : Window
{
    private readonly Grid root = new();
    private readonly StackPanel body = new() { Spacing = 20, Padding = new Thickness(32) };
    private readonly Grid cards = new() { ColumnSpacing = 14, RowSpacing = 14 };
    private readonly ObservableCollection<string> files = [];
    private readonly ListBox fileList = new() { MaxHeight = 180, MinHeight = 80, SelectionMode = SelectionMode.Single };
    private readonly TextBox pages = new() { Header = "Halaman (kosong = semua)", PlaceholderText = "Contoh: 1,3-5,2" };
    private readonly TextBox watermark = new() { Header = "Teks watermark", MaxLength = 200, PlaceholderText = "DRAFT" };
    private readonly PasswordBox password = new() { Header = "Kata sandi pemilik PDF (jika diperlukan)", MaxLength = 127 };
    private readonly PasswordBox newPassword = new() { Header = "Kata sandi pembuka baru (minimal 8 karakter)", MaxLength = 127 };
    private readonly ComboBox rotation = new() { Header = "Rotasi searah jarum jam", ItemsSource = new[] { 90, 180, 270 }, SelectedIndex = 0 };
    private readonly NumberBox margin = new() { Header = "Margin potong setiap sisi (pt)", Value = 20, Minimum = 1, Maximum = 500 };
    private readonly TextBox outputName = new() { Header = "Nama file hasil", Text = "hasil.pdf", MaxLength = 120 };
    private readonly TextBlock location = new() { Text = "Folder hasil belum dipilih", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly ProgressBar progress = new() { IsIndeterminate = true, Visibility = Visibility.Collapsed };
    private readonly Button run = new() { Content = "Proses dan simpan hasil", Padding = new Thickness(24, 12, 24, 12) };
    private readonly Button cancel = new() { Content = "Batalkan", IsEnabled = false };
    private readonly Button openResult = new() { Content = "Buka hasil", Visibility = Visibility.Collapsed };
    private readonly List<string> history = [];
    private readonly List<Button> navigation = [];
    private readonly List<FrameworkElement> editable = [];
    private ToolCard? selected;
    private CancellationTokenSource? active;
    private string? folder;
    private string? resultPath;
    private int columns;

    internal MainWindow()
    {
        Title = "PERTAMINA OTO — Desktop Offline · Development";
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1180, 850));
        root.Background = Brush(245, 247, 251);
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var sidebar = new StackPanel { Spacing = 12, Padding = new Thickness(16), Background = Brush(15, 32, 58) };
        sidebar.Children.Add(new TextBlock { Text = "OTO", FontSize = 32, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = Brush(255, 255, 255), Margin = new Thickness(8, 12, 0, 24) });
        var tools = Nav("\uE80F", "Alat utama", ShowCatalog);
        var recent = Nav("\uE81C", "Riwayat sesi", ShowHistory);
        var help = Nav("\uE897", "Bantuan", ShowHelp);
        sidebar.Children.Add(tools); sidebar.Children.Add(recent); sidebar.Children.Add(help);
        sidebar.Children.Add(new TextBlock { Text = "LOCAL • OFFLINE\nVersi pengembangan", TextWrapping = TextWrapping.Wrap,
            Foreground = Brush(169, 202, 231), Margin = new Thickness(8, 28, 0, 0), FontSize = 12 });
        root.Children.Add(sidebar);
        var scroll = new ScrollViewer { Content = body, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetColumn(scroll, 1); root.Children.Add(scroll);
        Content = root;
        fileList.ItemsSource = files;
        run.Background = Brush(22, 93, 222); run.Foreground = Brush(255, 255, 255);
        run.Click += async (_, _) => await ExecuteAsync();
        cancel.Click += (_, _) => active?.Cancel();
        openResult.Click += async (_, _) =>
        {
            if (resultPath is null) return;
            try { await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(resultPath)); }
            catch (Exception) { status.Text = "Tidak dapat membuka viewer. File hasil tetap tersimpan di folder pilihan Anda."; }
        };
        Closed += (_, _) => active?.Cancel();
        root.SizeChanged += (_, _) => LayoutCards();
        ShowCatalog();
    }

    private Button Nav(string glyph, string label, Action click)
    {
        var button = new Button { Content = IconLabel(glyph, label), HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Brush(25, 47, 78), Foreground = Brush(255, 255, 255), Padding = new Thickness(12) };
        AutomationProperties.SetName(button, label);
        button.Click += (_, _) => click(); navigation.Add(button); return button;
    }

    private static StackPanel IconLabel(string glyph, string label)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        panel.Children.Add(new FontIcon { Glyph = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 20 });
        panel.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
        return panel;
    }

    private void Heading(string title, string description)
    {
        body.Children.Add(new TextBlock { Text = title, FontSize = 30, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = description, FontSize = 14, Foreground = Brush(76, 91, 112), TextWrapping = TextWrapping.Wrap });
    }

    private void ShowCatalog()
    {
        if (active is not null) return;
        body.Children.Clear(); selected = null;
        Heading("Semua alat PDF Anda", "Berdasarkan 33 alat utama prototipe. Label menunjukkan fungsi yang benar-benar terhubung; belum berarti seluruh cakupan fitur selesai.");
        body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Informational,
            Title = "Development lokal — gunakan salinan uji", Message = "13 alat memiliki operasi dasar. Belum untuk dokumen sensitif/produksi. Processing tidak mengunggah dokumen." });
        var search = new TextBox { PlaceholderText = "Cari alat, misalnya merge, crop, atau watermark…" };
        AutomationProperties.SetName(search, "Cari alat PDF");
        search.TextChanged += (_, _) => FillCards(search.Text);
        body.Children.Add(search); body.Children.Add(cards); FillCards("");
    }

    private void FillCards(string search)
    {
        cards.Children.Clear();
        foreach (var tool in ToolCatalog.All.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || t.Group.Contains(search, StringComparison.OrdinalIgnoreCase)))
        {
            var panel = new StackPanel { Spacing = 10 };
            panel.Children.Add(new FontIcon { Glyph = tool.Glyph, FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 26, Foreground = Brush(22, 93, 222), HorizontalAlignment = HorizontalAlignment.Left });
            panel.Children.Add(new TextBlock { Text = tool.Name, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(new TextBlock { Text = tool.Group, FontSize = 12, Foreground = Brush(95, 109, 127) });
            panel.Children.Add(new TextBlock { Text = tool.Available ? "Operasi dasar tersedia" : "Belum aktif", FontSize = 12,
                Foreground = tool.Available ? Brush(17, 115, 82) : Brush(130, 92, 30) });
            var button = new Button { Content = panel, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(20), Background = Brush(255, 255, 255), MinHeight = 162 };
            AutomationProperties.SetName(button, tool.Name + (tool.Available ? ", operasi dasar tersedia" : ", belum aktif"));
            button.Click += (_, _) => ShowTool(tool); cards.Children.Add(button);
        }
        columns = 0; LayoutCards();
    }

    private void LayoutCards()
    {
        var count = root.ActualWidth >= 1120 ? 3 : root.ActualWidth >= 800 ? 2 : 1;
        if (count == columns) return;
        columns = count;
        cards.ColumnDefinitions.Clear(); cards.RowDefinitions.Clear();
        for (var i = 0; i < count; i++) cards.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < (cards.Children.Count + count - 1) / count; i++) cards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < cards.Children.Count; i++) { Grid.SetRow((FrameworkElement)cards.Children[i], i / count); Grid.SetColumn((FrameworkElement)cards.Children[i], i % count); }
    }

    private void ShowTool(ToolCard tool)
    {
        if (active is not null) return;
        body.Children.Clear(); editable.Clear(); selected = tool;
        if (run.Parent is Panel previousActions) previousActions.Children.Clear();
        Heading(tool.Name, tool.Description);
        if (!tool.Available)
        {
            body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning,
                Title = "Belum dapat diproses", Message = tool.Description });
            return;
        }
        body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning,
            Title = "Salinan uji saja", Message = "Versi awal: tata letak, form, bookmark, tautan, dan tanda tangan digital belum dijamin tetap sama. Crop tidak menghapus konten tersembunyi. Maksimal 100 MB / 1000 halaman." });
        files.Clear(); password.Password = ""; newPassword.Password = ""; pages.Text = "";
        outputName.Text = "hasil-" + tool.Id + (tool.Id == "split" ? ".zip" : ".pdf");
        status.Text = "Pilih file untuk memulai."; resultPath = null; openResult.Visibility = Visibility.Collapsed;
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        toolbar.Children.Add(ActionButton("Tambah file", async () => await PickFilesAsync()));
        toolbar.Children.Add(ActionButton("Hapus dari daftar", () => { if (fileList.SelectedIndex >= 0) files.RemoveAt(fileList.SelectedIndex); }));
        toolbar.Children.Add(ActionButton("↑", () => Move(-1), "Pindahkan file ke atas"));
        toolbar.Children.Add(ActionButton("↓", () => Move(1), "Pindahkan file ke bawah"));
        body.Children.Add(toolbar); body.Children.Add(fileList); editable.Add(fileList);
        if (tool.Id is not ("merge" or "images" or "protect" or "unlock" or "compress")) AddEditable(pages);
        if (tool.Id == "watermark") AddEditable(watermark);
        if (tool.Id == "rotate") AddEditable(rotation);
        if (tool.Id == "crop") AddEditable(margin);
        if (tool.Id != "images") AddEditable(password);
        if (tool.Id == "protect") AddEditable(newPassword);
        AddEditable(outputName);
        body.Children.Add(ActionButton("Pilih folder hasil (Save As)", async () => await PickFolderAsync()));
        body.Children.Add(location);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        actions.Children.Add(run); actions.Children.Add(cancel); actions.Children.Add(openResult);
        body.Children.Add(actions); body.Children.Add(progress); body.Children.Add(status);
        run.IsEnabled = true;
    }

    private void AddEditable(Control control) { body.Children.Add(control); editable.Add(control); }

    private Button ActionButton(string label, Action action, string? accessibleLabel = null)
    {
        var button = new Button { Content = label };
        AutomationProperties.SetName(button, accessibleLabel ?? label);
        button.Click += (_, _) => action(); editable.Add(button); return button;
    }

    private void Move(int delta)
    {
        var index = fileList.SelectedIndex;
        if (index < 0 || index + delta < 0 || index + delta >= files.Count) return;
        files.Move(index, index + delta); fileList.SelectedIndex = index + delta;
    }

    private async Task PickFilesAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            foreach (var extension in selected?.Id == "images" ? new[] { ".jpg", ".jpeg", ".png" } : new[] { ".pdf" })
                picker.FileTypeFilter.Add(extension);
            var picked = await picker.PickMultipleFilesAsync();
            if (picked.Count == 0) return;
            if (selected?.Id is not ("merge" or "images")) files.Clear();
            foreach (var file in picked)
            {
                if (files.Count >= 50) break;
                files.Add(file.Path);
                if (selected?.Id is not ("merge" or "images")) break;
            }
            status.Text = string.Create(CultureInfo.InvariantCulture, $"{files.Count} file dipilih. Urutan daftar dipakai saat proses.");
        }
        catch (Exception) { status.Text = "Pemilih file tidak dapat dibuka. Coba kembali."; }
    }

    private async Task PickFolderAsync()
    {
        try
        {
            var picker = new FolderPicker(); picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var chosen = await picker.PickSingleFolderAsync();
            if (chosen is null) return;
            folder = chosen.Path; location.Text = "Folder hasil: " + folder;
        }
        catch (Exception) { status.Text = "Pemilih folder tidak dapat dibuka. Coba kembali."; }
    }

    private async Task ExecuteAsync()
    {
        if (active is not null || selected is null) return;
        if (files.Count == 0) { status.Text = "Pilih file terlebih dahulu."; return; }
        if (folder is null) { status.Text = "Pilih folder hasil terlebih dahulu."; return; }
        var name = outputName.Text.Trim();
        var extension = selected.Id == "split" ? ".zip" : ".pdf";
        if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name != Path.GetFileName(name)
            || !name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        { status.Text = "Masukkan nama file yang valid dengan akhiran " + extension; return; }
        var target = Path.Combine(folder, name);
        var request = new PdfRequest(selected.Id, files.ToArray(), target, pages.Text, watermark.Text,
            (int)rotation.SelectedItem, margin.Value, password.Password, newPassword.Password);
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        foreach (var control in editable.OfType<Control>()) control.IsEnabled = false;
        foreach (var item in navigation) item.IsEnabled = false;
        run.IsEnabled = false; cancel.IsEnabled = true; progress.Visibility = Visibility.Visible;
        openResult.Visibility = Visibility.Collapsed; status.Text = "Memproses lokal… Anda dapat membatalkan.";
        try
        {
            var reply = await WorkerClient.RunAsync(request, cancellation.Token);
            status.Text = reply.Success
                ? string.Create(CultureInfo.InvariantCulture, $"{reply.Message}\n{reply.Pages} halaman • {reply.Bytes / 1024d:N1} KB\nTersimpan: {target}")
                : reply.Message;
            if (reply.Success)
            {
                resultPath = target; openResult.Visibility = Visibility.Visible;
                history.Insert(0, selected.Name + " — " + name + " — selesai lokal");
            }
        }
        finally
        {
            active = null; password.Password = ""; newPassword.Password = "";
            foreach (var control in editable.OfType<Control>()) control.IsEnabled = true;
            foreach (var item in navigation) item.IsEnabled = true;
            run.IsEnabled = true; cancel.IsEnabled = false; progress.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowHistory()
    {
        if (active is not null) return;
        body.Children.Clear(); Heading("Riwayat sesi", "Hanya sesi ini; kata sandi dan isi dokumen tidak dicatat. Riwayat persisten belum terhubung.");
        foreach (var item in history) body.Children.Add(new TextBlock { Text = item, TextWrapping = TextWrapping.Wrap });
        if (history.Count == 0) body.Children.Add(new TextBlock { Text = "Belum ada hasil pada sesi ini." });
    }

    private void ShowHelp()
    {
        if (active is not null) return;
        body.Children.Clear(); Heading("Bantuan", "Pilih alat → tambah file → atur opsi → pilih folder dan nama baru → proses → buka hasil.");
        body.Children.Add(new TextBlock { Text = "Untuk melaporkan masalah: tekan Win + Shift + S, pilih area layar, lalu tempel tangkapan layar di percakapan proyek. Hindari menampilkan isi dokumen sensitif atau kata sandi.\n\nAplikasi tidak menimpa sumber atau hasil existing. Split menyimpan ZIP. Organize menerima urutan seperti 3,1,1,2. Crop hanya mengubah area tampil, bukan menghapus data.\n\nIni build pengembangan, bukan rilis produksi. Operasi parser berjalan di proses terpisah dengan pembatalan dan timeout; pemisahan proses bukan sandbox keamanan.", TextWrapping = TextWrapping.Wrap });
    }

    private static SolidColorBrush Brush(byte red, byte green, byte blue) => new(ColorHelper.FromArgb(255, red, green, blue));
}
