using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Native design preview only. No document access or persistence. Never mark edit available here.
    private Grid? editorDesign;
    private StackPanel? editorProperties;
    private readonly Dictionary<string, Button> editorModeButtons = [];
    private Button? editorSave;

    internal void ShowPdfEditorDesignPreview()
    {
        BeginScreen("Edit PDF", "Pratinjau desain");
        SetHistoryPageVisible(true);
        foreach (var item in navigation) item.Background = AutomationProperties.GetName(item) == "Semua Alat" ? Brush(23, 104, 212) : Brush(7, 29, 61);
        sidebarCollapsed = true; LayoutCards();
        historyPage.Children.Clear(); editorModeButtons.Clear();
        var shell = new Grid { Padding = new Thickness(16), RowSpacing = 12, Background = Brush(245, 247, 251) };
        editorDesign = shell;
        foreach (var size in new[] { GridLength.Auto, GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto })
            shell.RowDefinitions.Add(new RowDefinition { Height = size });
        var header = new Grid { ColumnSpacing = 16 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new StackPanel { Spacing = 3 };
        title.Children.Add(HistoryText("Edit PDF", 24, true));
        title.Children.Add(HistoryText("Laporan inspeksi.pdf · Dokumen contoh", 13, muted: true));
        header.Children.Add(title);
        editorSave = HistoryButton("file-text", "Simpan PDF baru", true);
        editorSave.IsEnabled = false; editorSave.MinHeight = 44;
        ToolTipService.SetToolTip(editorSave, "Belum tersedia: mesin edit belum lolos pemeriksaan integrasi.");
        Grid.SetColumn(editorSave, 1); header.Children.Add(editorSave); shell.Children.Add(header);
        var warning = new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Informational,
            Title = "Pratinjau desain — bukan editor aktif", Message = "Pilihan alat dapat ditinjau. Dokumen tidak diubah dan belum dapat disimpan." };
        Grid.SetRow(warning, 1); shell.Children.Add(warning);
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        foreach (var (label, icon) in new[] { ("Pilih", "editor-pointer"), ("Edit teks asli", "editor-text"), ("Tambah teks", "editor-text-add"),
            ("Gambar", "favorite-image"), ("Gambar bebas", "editor-pen"), ("Bentuk", "editor-shapes") })
        {
            var button = HistoryButton(icon, label, true); button.MinHeight = 44;
            button.Click += (_, _) => SetEditorDesignMode(label);
            editorModeButtons.Add(label, button); toolbar.Children.Add(button);
        }
        foreach (var (label, icon) in new[] { ("Undo", "editor-undo"), ("Redo", "editor-redo") })
        {
            var button = HistoryButton(icon, label, true); button.IsEnabled = false; button.MinHeight = 44; toolbar.Children.Add(button);
        }
        var toolsScroll = new ScrollViewer { Content = toolbar, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(toolsScroll, 2); shell.Children.Add(toolsScroll);
        var workspace = new Grid { ColumnSpacing = 12 };
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(132) });
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        var thumbnails = new StackPanel { Spacing = 12, Padding = new Thickness(12) };
        thumbnails.Children.Add(HistoryText("Halaman", 14, true));
        var paper = new StackPanel { Spacing = 18, Width = 500, Padding = new Thickness(36), Background = Brush(255, 255, 255), MinHeight = 690 };
        var pageLabel = HistoryText("LAPORAN INSPEKSI", 22, true);
        for (var i = 1; i <= 3; i++)
        {
            var number = i;
            var thumb = new StackPanel { Spacing = 8 };
            var miniature = new StackPanel { Spacing = 5, Width = 68, Padding = new Thickness(8), Background = Brush(255, 255, 255), Height = 95 };
            for (var line = 0; line < 9; line++) miniature.Children.Add(new Border { Height = line == 0 ? 5 : 2, Background = Brush(182, 192, 206), Margin = new Thickness(0, 2, line % 3 * 8, 0) });
            thumb.Children.Add(miniature); thumb.Children.Add(HistoryText($"{i}", 12));
            var button = new Button { Content = thumb, HorizontalAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(8),
                Background = Brush(240, 246, 255), CornerRadius = new CornerRadius(6) };
            AutomationProperties.SetName(button, $"Halaman contoh {i}");
            button.Click += (_, _) => pageLabel.Text = $"LAPORAN INSPEKSI · {number}";
            thumbnails.Children.Add(button);
        }
        var thumbScroll = new ScrollViewer { Content = thumbnails, Background = Brush(255, 255, 255) }; workspace.Children.Add(thumbScroll);
        paper.Children.Add(HistoryText("PERTAMINA OTO", 16, true)); paper.Children.Add(pageLabel);
        paper.Children.Add(HistoryText("Peralatan Operasional · CONTOH", 14, muted: true));
        foreach (var line in new[] { "Lokasi                 Area inspeksi contoh", "Tanggal              18 September 2026", "Peralatan           Pompa transfer", "Kondisi               Baik" })
            paper.Children.Add(new Border { Padding = new Thickness(10), Background = Brush(235, 242, 250), Child = HistoryText(line, 13) });
        paper.Children.Add(HistoryText("Hasil inspeksi", 16, true));
        paper.Children.Add(new Border { BorderBrush = Brush(8, 102, 232), BorderThickness = new Thickness(1), Padding = new Thickness(8),
            Child = HistoryText("Kondisi peralatan baik dan siap digunakan.", 14) });
        paper.Children.Add(HistoryText("Dokumen ilustrasi untuk meninjau tata letak editor. Area ini bukan teks PDF yang sudah bisa diedit.", 13, muted: true));
        var viewport = new ScrollViewer { Content = paper, ZoomMode = ZoomMode.Enabled, MinZoomFactor = .4f, MaxZoomFactor = 3,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brush(226, 232, 240), Padding = new Thickness(20), HorizontalContentAlignment = HorizontalAlignment.Center };
        Grid.SetColumn(viewport, 1); workspace.Children.Add(viewport);
        editorProperties = new StackPanel { Spacing = 16, Padding = new Thickness(20) };
        var propertiesScroll = new ScrollViewer { Content = editorProperties, Background = Brush(255, 255, 255) };
        Grid.SetColumn(propertiesScroll, 2); workspace.Children.Add(propertiesScroll);
        Grid.SetRow(workspace, 3); shell.Children.Add(workspace);
        var footer = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var currentPage = 1;
        var pageStatus = HistoryText("1 / 3", 13, true);
        var previousPage = HistoryButton("chevron-left", "Halaman sebelumnya");
        var nextPage = HistoryButton("chevron-right", "Halaman berikutnya");
        void SetPage(int number)
        {
            currentPage = Math.Clamp(number, 1, 3);
            pageLabel.Text = $"LAPORAN INSPEKSI · {currentPage}";
            pageStatus.Text = $"{currentPage} / 3";
            previousPage.IsEnabled = currentPage > 1; nextPage.IsEnabled = currentPage < 3;
            for (var index = 1; index < thumbnails.Children.Count; index++)
                if (thumbnails.Children[index] is Button thumbnail)
                {
                    thumbnail.BorderThickness = new Thickness(index == currentPage ? 2 : 1);
                    thumbnail.BorderBrush = index == currentPage ? Brush(8, 102, 232) : Brush(218, 226, 237);
                    AutomationProperties.SetItemStatus(thumbnail, index == currentPage ? "Halaman aktif" : "");
                }
        }
        for (var index = 1; index < thumbnails.Children.Count; index++)
        {
            var number = index;
            if (thumbnails.Children[index] is Button thumbnail) thumbnail.Click += (_, _) => SetPage(number);
        }
        previousPage.Click += (_, _) => SetPage(currentPage - 1);
        nextPage.Click += (_, _) => SetPage(currentPage + 1);
        SetPage(1);
        footer.Children.Add(previousPage); footer.Children.Add(pageStatus); footer.Children.Add(nextPage);
        var smaller = HistoryButton("minus", "Perkecil"); var larger = HistoryButton("plus", "Perbesar");
        smaller.Click += (_, _) => viewport.ChangeView(null, null, Math.Max(.4f, viewport.ZoomFactor - .1f));
        larger.Click += (_, _) => viewport.ChangeView(null, null, Math.Min(3, viewport.ZoomFactor + .1f));
        var fit = HistoryButton("scan", "Sesuaikan", true); fit.Click += (_, _) => viewport.ChangeView(0, 0, (float)Math.Clamp((viewport.ActualWidth - 40) / 500, .4, 1));
        footer.Children.Add(smaller); footer.Children.Add(larger); footer.Children.Add(fit);
        var zoomStatus = HistoryText("100%", 12);
        viewport.ViewChanged += (_, _) => zoomStatus.Text = viewport.ZoomFactor.ToString("P0", System.Globalization.CultureInfo.CurrentCulture);
        footer.Children.Add(zoomStatus);
        footer.Children.Add(HistoryText("Cubit untuk zoom · Dokumen contoh", 12, muted: true));
        var footerScroll = new ScrollViewer { Content = footer, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(footerScroll, 4); shell.Children.Add(footerScroll);
        var showProperties = HistoryButton("editor-shapes", "Pengaturan alat", true);
        var propertyFlyout = new Flyout();
        showProperties.Flyout = propertyFlyout; footer.Children.Insert(0, showProperties);
        // Narrow preview prioritizes the inspector over the thumbnail rail. Real editor drawer remains pending.
        shell.SizeChanged += (_, _) =>
        {
            var narrow = shell.ActualWidth < 1050;
            workspace.ColumnDefinitions[0].Width = new GridLength(narrow ? 0 : 132);
            thumbScroll.Visibility = narrow ? Visibility.Collapsed : Visibility.Visible;
            var drawer = shell.ActualWidth < 1050;
            workspace.ColumnDefinitions[2].Width = new GridLength(drawer ? 0 : 280);
            showProperties.Visibility = drawer ? Visibility.Visible : Visibility.Collapsed;
            if (drawer && propertiesScroll.Parent == workspace)
            {
                workspace.Children.Remove(propertiesScroll);
                propertiesScroll.Width = 280; propertiesScroll.MaxHeight = 480;
                propertyFlyout.Content = propertiesScroll;
            }
            else if (!drawer && propertiesScroll.Parent != workspace)
            {
                propertyFlyout.Hide(); propertyFlyout.Content = null;
                propertiesScroll.Width = double.NaN; propertiesScroll.MaxHeight = double.PositiveInfinity;
                workspace.Children.Add(propertiesScroll);
            }
        };
        historyPage.Children.Add(shell); SetEditorDesignMode("Edit teks asli");
    }

    private void SetEditorDesignMode(string mode)
    {
        if (editorProperties is null) return;
        foreach (var item in editorModeButtons)
        {
            item.Value.Background = item.Key == mode ? Brush(219, 234, 254) : Brush(255, 255, 255);
            item.Value.BorderThickness = new Thickness(item.Key == mode ? 2 : 1);
            item.Value.BorderBrush = item.Key == mode ? Brush(8, 102, 232) : Brush(218, 226, 237);
            AutomationProperties.SetItemStatus(item.Value, item.Key == mode ? "Alat aktif" : "");
        }
        editorProperties.Children.Clear(); editorProperties.Children.Add(HistoryText(mode, 20, true));
        editorProperties.Children.Add(HistoryText("Contoh pengaturan · belum diterapkan ke PDF", 12, muted: true));
        void Choices(string label, params string[] values)
        {
            var box = new ComboBox { Header = label, ItemsSource = values, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch, MinHeight = 44 };
            AutomationProperties.SetName(box, label); editorProperties.Children.Add(box);
        }
        void Action(string label, string icon)
        {
            var button = HistoryButton(icon, label, true); button.MinHeight = 44; button.IsEnabled = false; editorProperties.Children.Add(button);
        }
        if (mode is "Edit teks asli" or "Tambah teks")
        {
            Choices("Font", "Arial", "Times New Roman", "Courier New"); Choices("Ukuran", "12 pt", "14 pt", "16 pt", "24 pt");
            Choices("Gaya", "Normal", "Tebal", "Miring", "Garis bawah"); Choices("Warna teks", "Hitam", "Biru", "Merah");
            Choices("Perataan", "Kiri", "Tengah", "Kanan");
            editorProperties.Children.Add(HistoryText(mode == "Edit teks asli" ? "Font yang tidak mendukung karakter harus ditolak. Tidak menggunakan kotak penutup." : "Klik halaman untuk menambahkan teks setelah mesin editor aktif.", 13, muted: true));
        }
        else if (mode == "Gambar")
        {
            Action("Pilih gambar", "favorite-image"); Choices("Proporsi", "Kunci proporsi", "Bebas"); Choices("Putaran", "0°", "90°", "180°", "270°"); Choices("Transparansi", "100%", "75%", "50%");
        }
        else if (mode is "Gambar bebas" or "Bentuk")
        {
            if (mode == "Bentuk") Choices("Jenis bentuk", "Kotak", "Lingkaran", "Garis", "Panah");
            Choices("Warna garis", "Hitam", "Biru", "Merah"); Choices("Ketebalan", "2 pt", "1 pt", "4 pt", "6 pt");
            if (mode == "Bentuk") Choices("Isi", "Tanpa isi", "Putih", "Biru muda");
            Choices("Transparansi", "100%", "75%", "50%");
        }
        else
        {
            editorProperties.Children.Add(HistoryText("Pilih objek tambahan untuk mengatur posisi dan ukurannya.", 14, muted: true));
            Choices("Putaran", "0°", "90°", "180°", "270°"); Action("Duplikasi", "file-text"); Action("Hapus objek", "x");
        }
    }
}
