using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly HashSet<string> favorites = [];
    private readonly List<(ToolCard Tool, string FileName)> completedActivities = [];
    private readonly TextBlock breadcrumb = new() { VerticalAlignment = VerticalAlignment.Center, Foreground = Brush(100, 116, 139) };
    private readonly Grid homeEntries = new() { ColumnSpacing = 16, RowSpacing = 16 };
    private readonly TextBox globalSearch = new() { PlaceholderText = "Cari alat atau kategori", MinWidth = 120 };

    private Grid BuildHeader()
    {
        var header = new Grid { Padding = new Thickness(20, 14, 20, 14), Background = Brush(255, 255, 255), ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        AutomationProperties.SetName(globalSearch, "Cari alat atau kategori");
        globalSearch.TextChanged += (_, _) => { if (active is not null) return; ShowCatalog(); FillCards(globalSearch.Text); };
        header.Children.Add(breadcrumb);
        var badge = new TextBlock { Text = "Offline · File tetap di perangkat", Foreground = Brush(17, 115, 82), FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(badge, 1); header.Children.Add(badge);
        breadcrumb.TextTrimming = TextTrimming.CharacterEllipsis; breadcrumb.MaxLines = 1;
        ResponsiveLayout.Header(header, breadcrumb, badge, 540);
        return header;
    }

    private static Border Section(string title, string detail, UIElement? action = null)
    {
        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        content.Children.Add(new TextBlock { Text = detail, FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = Brush(100, 116, 139) });
        if (action is not null) content.Children.Add(action);
        return new Border { Child = content, Padding = new Thickness(24), CornerRadius = new CornerRadius(16), Background = Brush(255, 255, 255), BorderBrush = Brush(223, 229, 238), BorderThickness = new Thickness(1) };
    }

    private static Border ToolIcon(ToolCard tool)
    {
        var color = tool.Group switch
        {
            "Organize PDF" => Brush(112, 71, 235), "Optimize PDF" => Brush(229, 72, 77),
            "Convert to PDF" => Brush(21, 153, 87), "Convert from PDF" => Brush(7, 149, 159),
            "PDF Security" => Brush(233, 118, 22), _ => Brush(23, 104, 212)
        };
        return new Border { Width = 44, Height = 44, CornerRadius = new CornerRadius(12), Background = Brush(237, 243, 251), HorizontalAlignment = HorizontalAlignment.Left,
            Child = new FontIcon { Glyph = tool.Glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), Foreground = color, FontSize = 24 } };
    }

    private void BeginScreen(string title, string description)
    {
        SaveToolDraft();
        organizerVisible = false;
        body.Children.Clear(); editable.Clear(); Heading(title, description);
    }

    private void ShowHome()
    {
        if (active is not null || importing || messageOpen) return;
        CaptureWorkspaceContext(); SaveToolDraft();
        BuildDesignedHome();
    }

    private void ShowFavorites()
    {
        if (active is not null) return;
        BeginScreen("Favorit", "Alat pilihan Anda, satu klik untuk mulai.");
        body.Children.Clear();
        foreach (var item in navigation) item.Background = AutomationProperties.GetName(item) == "Favorit" ? Brush(23, 104, 212) : Brush(7, 29, 61);
        var header = new Grid { ColumnSpacing = 20, RowSpacing = 16 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var heading = new StackPanel { Spacing = 6 };
        heading.Children.Add(HistoryText("Favorit", 28, true));
        heading.Children.Add(HistoryText("Alat pilihan Anda, satu klik untuk mulai.", 14, muted: true));
        header.Children.Add(heading);
        var add = HistoryButton("plus", "Tambah favorit", true, true);
        add.MinHeight = 44;
        add.Click += (_, _) => ShowCatalog();
        ToolTipService.SetToolTip(add, "Pilih bintang pada halaman Semua Alat untuk menambahkan favorit.");
        Grid.SetColumn(add, 1); header.Children.Add(add);
        header.SizeChanged += (_, _) =>
        {
            var narrow = header.ActualWidth < 530;
            Grid.SetColumn(add, narrow ? 0 : 1); Grid.SetRow(add, narrow ? 1 : 0);
            add.HorizontalAlignment = narrow ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        };
        body.Children.Add(new Border { Child = header, Padding = new Thickness(24), Background = Brush(255, 255, 255),
            CornerRadius = new CornerRadius(10), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) });
        var toolbar = new Grid { ColumnSpacing = 16 };
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var search = new TextBox { PlaceholderText = "Cari alat favorit…", MinHeight = 44 };
        AutomationProperties.SetName(search, "Cari alat favorit");
        var searchFrame = new Grid();
        search.Padding = new Thickness(38, 10, 12, 10);
        searchFrame.Children.Add(search);
        var searchIcon = HistoryIcon("search");
        searchIcon.HorizontalAlignment = HorizontalAlignment.Left; searchIcon.Margin = new Thickness(12, 0, 0, 0);
        searchFrame.Children.Add(searchIcon); toolbar.Children.Add(searchFrame);
        var count = HistoryText("", 13, muted: true); Grid.SetColumn(count, 1); toolbar.Children.Add(count);
        body.Children.Add(toolbar);
        var cards = new Grid { ColumnSpacing = 20, RowSpacing = 20 };
        body.Children.Add(cards);
        var empty = HistoryText("", 15, muted: true); empty.Margin = new Thickness(8, 20, 8, 20); body.Children.Add(empty);
        var tools = ToolCatalog.All.Where(t => favorites.Contains(t.Id)).ToArray();
        void Layout()
        {
            var visible = cards.Children.OfType<FrameworkElement>().Where(c => c.Visibility == Visibility.Visible).ToArray();
            var columns = Math.Clamp((int)((cards.ActualWidth + 20) / 280), 1, 3);
            cards.ColumnDefinitions.Clear(); cards.RowDefinitions.Clear();
            for (var i = 0; i < columns; i++) cards.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (var i = 0; i < (visible.Length + columns - 1) / columns; i++) cards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var i = 0; i < visible.Length; i++) { Grid.SetColumn(visible[i], i % columns); Grid.SetRow(visible[i], i / columns); }
        }
        foreach (var tool in tools)
        {
            var name = CatalogName(tool);
            var content = new Grid { RowSpacing = 16 };
            foreach (var height in new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto })
                content.RowDefinitions.Add(new RowDefinition { Height = height });
            var top = new Grid();
            var icon = tool.Id switch { "organize" => "favorite-organize", "images" => "favorite-images", "pdf-jpg" => "favorite-image", _ => "file-text" };
            top.Children.Add(new Border { Width = 52, Height = 52, CornerRadius = new CornerRadius(12),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = tool.Id == "images" ? Brush(231, 247, 238) : tool.Id == "pdf-jpg" ? Brush(229, 245, 252) : Brush(241, 235, 255),
                Child = HistoryIcon(icon, 28) });
            var star = HistoryButton("favorite-star", "Hapus " + name + " dari favorit");
            star.MinWidth = 44; star.MinHeight = 44; star.HorizontalAlignment = HorizontalAlignment.Right;
            ToolTipService.SetToolTip(star, "Hapus dari favorit — tidak menghapus alat atau file");
            star.Click += (_, _) =>
            {
                favorites.Remove(tool.Id); SaveLibrary(); ShowFavorites();
                var undo = HistoryButton("plus", "Urungkan penghapusan " + name, true);
                undo.Click += (_, _) => { favorites.Add(tool.Id); SaveLibrary(); ShowFavorites(); };
                body.Children.Insert(1, undo); undo.Focus(FocusState.Keyboard);
            };
            top.Children.Add(star); content.Children.Add(top);
            var title = HistoryText(name, 18, true); Grid.SetRow(title, 1); content.Children.Add(title);
            var description = HistoryText(tool.Id switch {
                "organize" => "Susun ulang, putar, atau hapus halaman PDF.",
                "images" => "Gabungkan gambar menjadi satu file PDF.",
                "pdf-jpg" => "Ubah halaman PDF menjadi gambar JPG.",
                _ => tool.Description }, 14, muted: true);
            Grid.SetRow(description, 2); content.Children.Add(description);
            var open = HistoryButton("favorite-arrow", tool.Available ? "Buka alat" : "Lihat keterangan", true);
            var openContent = (StackPanel)open.Content;
            var arrow = openContent.Children[0]; openContent.Children.RemoveAt(0); openContent.Children.Add(arrow);
            open.Foreground = Brush(8, 102, 232); open.MinHeight = 44;
            open.HorizontalAlignment = HorizontalAlignment.Left;
            AutomationProperties.SetName(open, "Buka " + name + (tool.Available ? "" : ", belum aktif"));
            open.Click += (_, _) => OpenHomeTool(tool);
            Grid.SetRow(open, 3); content.Children.Add(open);
            cards.Children.Add(new Border { Child = content, Tag = name, MinHeight = 252, Padding = new Thickness(24),
                Background = Brush(255, 255, 255), BorderBrush = Brush(221, 229, 241),
                BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10) });
        }
        void Filter()
        {
            var query = search.Text.Trim();
            foreach (var card in cards.Children.OfType<FrameworkElement>())
                card.Visibility = ((string)card.Tag).Contains(query, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
            var total = cards.Children.OfType<FrameworkElement>().Count(c => c.Visibility == Visibility.Visible);
            count.Text = query.Length == 0 ? $"{tools.Length} alat favorit" : $"{total} dari {tools.Length} alat";
            empty.Text = tools.Length == 0 ? "Belum ada favorit. Klik Tambah favorit, lalu pilih bintang pada alat yang Anda gunakan."
                : "Tidak ada alat yang cocok. Coba kata lain.";
            empty.Visibility = total == 0 ? Visibility.Visible : Visibility.Collapsed;
            Layout();
        }
        search.TextChanged += (_, _) => Filter();
        cards.SizeChanged += (_, _) => Layout();
        Filter();
        body.Children.Add(HistoryText(libraryNotice.Length == 0 ? "Favorit tersimpan otomatis di perangkat." : libraryNotice, 12, muted: true));
        body.Children.Add(HistoryText("Klik bintang untuk menghapus dari favorit.", 12, muted: true));
    }

    private void ShowActivity()
    {
        if (active is not null) return;
        BeginScreen("Aktivitas", "Pantau pekerjaan pada sesi ini.");
        var content = new StackPanel { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Stretch };
        var wrapper = BaselineSupportFrame();
        content.Children.Add(new Border { HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(12, 6, 12, 6), CornerRadius = new CornerRadius(8),
            Background = Brush(255, 255, 255), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1),
            Child = Copy("●  Tidak ada proses berjalan", 13) });
        var rows = new StackPanel();
        if (completedActivities.Count == 0)
        {
            var empty = new StackPanel { Spacing = 12, Padding = new Thickness(24), HorizontalAlignment = HorizontalAlignment.Stretch };
            empty.Children.Add(new FontIcon { Glyph = "\uE9D9", FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 28, Foreground = Brush(28, 100, 214), Margin = new Thickness(0, 4, 0, 0) });
            empty.Children.Add(new TextBlock { Text = "Belum ada aktivitas", FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextAlignment = TextAlignment.Center });
            var hint = Copy("Mulai mengolah dokumen. Hasil pekerjaan sesi ini akan muncul di sini.", 13);
            hint.TextAlignment = TextAlignment.Center; empty.Children.Add(hint);
            var start = ActionButton("Pilih alat", ShowCatalog);
            start.HorizontalAlignment = HorizontalAlignment.Center;
            start.Background = Brush(23, 104, 212); start.Foreground = Brush(255, 255, 255);
            start.Padding = new Thickness(24, 8, 24, 8);
            empty.Children.Add(start); rows.Children.Add(empty);
        }
        else
        {
            content.Children.Add(Copy($"{completedActivities.Count} pekerjaan selesai", 13));
            foreach (var item in completedActivities)
            {
                if (rows.Children.Count > 0) rows.Children.Add(new Border { Height = 1, Background = Brush(235, 239, 245) });
                var row = new Grid { MinHeight = 64, Padding = new Thickness(16, 10, 16, 10), ColumnSpacing = 14 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.Children.Add(new FontIcon { Glyph = item.Tool.Id == "merge" ? "\uE8C8" : item.Tool.Glyph,
                    FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 22, Foreground = Brush(28, 100, 214) });
                var label = new StackPanel { Spacing = 3, VerticalAlignment = VerticalAlignment.Center };
                label.Children.Add(new TextBlock { Text = CatalogName(item.Tool), FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
                var file = Copy(item.FileName, 12); file.TextWrapping = TextWrapping.NoWrap;
                file.TextTrimming = TextTrimming.CharacterEllipsis;
                ToolTipService.SetToolTip(file, item.FileName); label.Children.Add(file);
                Grid.SetColumn(label, 1); row.Children.Add(label);
                var done = new TextBlock { Text = "✓  Selesai", FontSize = 13, Foreground = Brush(17, 115, 82), VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(done, 2); row.Children.Add(done); rows.Children.Add(row);
            }
        }
        content.Children.Add(new Border { Child = rows, Background = Brush(255, 255, 255),
            CornerRadius = new CornerRadius(12), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) });
        content.Children.Add(Copy("Aktivitas ditampilkan untuk sesi aplikasi ini. Progres dan pembatalan tersedia di halaman alat.", 12));
        wrapper.Children.Add(content); body.Children.Add(wrapper);
    }

    private void ShowSecured()
    {
        if (active is not null) return;
        BeginScreen("File Diamankan", "Status penyimpanan dan cadangan dokumen Anda.");
        var content = new StackPanel { Spacing = 18 };
        var wrapper = BaselineSupportFrame();

        Border Notice(string text) => new()
        {
            Padding = new Thickness(12, 9, 12, 9), CornerRadius = new CornerRadius(8),
            Background = Brush(255, 247, 229), BorderBrush = Brush(245, 215, 153), BorderThickness = new Thickness(1),
            Child = new TextBlock { Text = text, FontSize = 13, Foreground = Brush(139, 82, 0), TextWrapping = TextWrapping.Wrap }
        };
        var badge = Notice("ⓘ  Penyimpanan akun belum tersedia");
        badge.HorizontalAlignment = HorizontalAlignment.Left; content.Children.Add(badge);
        var rows = new StackPanel();
        void AddStorageRow(string glyph, string title, string detail, FrameworkElement action, bool available)
        {
            var row = new Grid { Padding = new Thickness(18, 16, 18, 16), ColumnSpacing = 16, RowSpacing = 10 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row.Children.Add(new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(8),
                Background = available ? Brush(237, 244, 255) : Brush(242, 244, 247), VerticalAlignment = VerticalAlignment.Center,
                Child = new FontIcon { Glyph = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 23,
                    Foreground = available ? Brush(23, 104, 212) : Brush(112, 125, 146) } });
            var label = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
            label.Children.Add(new TextBlock { Text = title, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            label.Children.Add(Copy(detail, 13)); Grid.SetColumn(label, 1); row.Children.Add(label);
            action.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(action, 2); row.Children.Add(action);
            row.SizeChanged += (_, _) =>
            {
                var narrow = row.ActualWidth < 620;
                Grid.SetColumnSpan(label, narrow ? 2 : 1);
                Grid.SetColumn(action, narrow ? 1 : 2); Grid.SetRow(action, narrow ? 1 : 0);
                action.HorizontalAlignment = narrow ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            };
            rows.Children.Add(row);
        }
        var historyButton = ActionButton("Riwayat  →", ShowHistory, "Buka riwayat");
        historyButton.Foreground = Brush(23, 104, 212); historyButton.BorderBrush = Brush(23, 104, 212);
        historyButton.Background = Brush(255, 255, 255); historyButton.CornerRadius = new CornerRadius(6);
        historyButton.MinHeight = 36;
        AddStorageRow("\uE8B7", "File hasil di perangkat", "Disimpan di folder yang Anda pilih saat menyimpan hasil.", historyButton, true);
        rows.Children.Add(new Border { Height = 1, Margin = new Thickness(18, 0, 18, 0), Background = Brush(226, 232, 241) });
        AddStorageRow("\uE753", "Penyimpanan akun", "Sinkronisasi lintas perangkat belum tersedia.",
            new Border { Padding = new Thickness(10, 6, 10, 6), CornerRadius = new CornerRadius(6), Background = Brush(239, 242, 247), Child = Copy("Belum tersedia", 12) }, false);
        content.Children.Add(new Border { Child = rows, Background = Brush(255, 255, 255), CornerRadius = new CornerRadius(12),
            BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) });
        content.Children.Add(Notice("ⓘ  File lokal bukan cadangan otomatis. Tidak ada file yang diunggah dari halaman ini."));
        var guidance = new StackPanel { Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };
        guidance.Children.Add(new TextBlock { Text = "Cara menjaga salinan dokumen", FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        guidance.Children.Add(Copy("1.  Simpan hasil tanpa menimpa dokumen asli.\n2.  Salin dokumen penting ke media cadangan milik Anda.", 13));
        content.Children.Add(guidance);
        content.Children.Add(new Border { Height = 1, Background = Brush(221, 229, 241), Margin = new Thickness(0, 8, 0, 0) });
        content.Children.Add(Copy("Alat lokal yang sudah aktif tetap dapat digunakan secara offline.", 12));
        wrapper.Children.Add(content); body.Children.Add(wrapper);
    }

    private void ShowAssistant()
    {
        if (active is not null) return;
        BeginScreen("Assistant", "Temukan cara mengolah dokumen Anda.");
        var content = new StackPanel { Spacing = 16 };
        var wrapper = BaselineSupportFrame();
        content.Children.Add(new Border { HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(12, 6, 12, 6),
            CornerRadius = new CornerRadius(8), Background = Brush(255, 245, 222),
            Child = new TextBlock { Text = "ⓘ  AI lokal belum tersedia", Foreground = Brush(139, 82, 0), FontSize = 13 } });
        var composer = new StackPanel { Spacing = 12 };
        var title = IconLabel("\uE99A", "Apa yang ingin Anda kerjakan?");
        ((FontIcon)title.Children[0]).Foreground = Brush(112, 71, 235);
        composer.Children.Add(title);
        var prompt = new TextBox { PlaceholderText = "Perintah bebas belum tersedia. Model AI lokal belum terhubung.",
            IsEnabled = false, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 90 };
        AutomationProperties.SetName(prompt, "Perintah Assistant — AI lokal belum tersedia");
        composer.Children.Add(prompt);
        composer.Children.Add(new Button { Content = "Tinjau rencana", IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Right });
        composer.Children.Add(Copy("Belum ada model AI yang terhubung. Tidak ada dokumen dikirim ke AI.", 12));
        content.Children.Add(new Border { Child = composer, Padding = new Thickness(20), Background = Brush(255, 255, 255),
            CornerRadius = new CornerRadius(12), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) });
        content.Children.Add(new TextBlock { Text = "Gunakan alat langsung", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
        content.Children.Add(Copy("Pilihan ini membuka alat, bukan menjalankan AI.", 13));
        var rows = new StackPanel();
        foreach (var (id, description) in new[] {
            ("merge", "Satukan beberapa dokumen"), ("organize", "Susun urutan dan putar halaman"),
            ("compress", "Kompresi dasar tanpa menurunkan kualitas gambar; ukuran bisa tetap atau bertambah"),
            ("watermark", "Beri penanda pada dokumen") })
        {
            var tool = ToolCatalog.All.First(t => t.Id == id);
            if (rows.Children.Count > 0) rows.Children.Add(new Border { Height = 1, Background = Brush(235, 239, 245), Margin = new Thickness(16, 0, 16, 0) });
            var row = new Grid { ColumnSpacing = 14 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            row.Children.Add(ToolIcon(tool));
            var label = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
            label.Children.Add(new TextBlock { Text = CatalogName(tool), FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            label.Children.Add(Copy(description, 12)); Grid.SetColumn(label, 1); row.Children.Add(label);
            var arrow = Copy("›", 22); arrow.VerticalAlignment = VerticalAlignment.Center; Grid.SetColumn(arrow, 2); row.Children.Add(arrow);
            var open = new Button { Content = row, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Background = Brush(255, 255, 255), BorderThickness = new Thickness(0), Padding = new Thickness(16, 10, 16, 10), MinHeight = 64,
                IsEnabled = tool.Available };
            AutomationProperties.SetName(open, "Buka " + CatalogName(tool));
            ToolTipService.SetToolTip(open, "Buka alat; dokumen tidak diproses otomatis");
            open.Click += (_, _) => OpenHomeTool(tool); rows.Children.Add(open);
        }
        content.Children.Add(new Border { Child = rows, Background = Brush(255, 255, 255), CornerRadius = new CornerRadius(12), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1) });
        var all = ActionButton("Lihat semua alat →", ShowCatalog); all.Foreground = Brush(23, 104, 212);
        content.Children.Add(all);
        content.Children.Add(Copy("Alat lokal yang sudah aktif tetap dapat digunakan tanpa internet.", 12));
        wrapper.Children.Add(content); body.Children.Add(wrapper);
    }

    private Grid BaselineSupportFrame()
    {
        // 0.1.0 anchors the 850-DIP content to the page's left edge. A capped
        // stretch Grid centers itself instead, separating content from its heading.
        var wrapper = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        wrapper.ColumnDefinitions.Add(new ColumnDefinition());
        // Bound against the parent, not the child's desired 850-DIP width:
        // the latter can otherwise keep its own overflow alive on a narrow page.
        void ApplyWidth() => wrapper.Width = Math.Min(850, Math.Max(0, body.ActualWidth - body.Padding.Left - body.Padding.Right));
        void Resize(object sender, SizeChangedEventArgs e) => ApplyWidth();
        ApplyWidth();
        wrapper.Loaded += (_, _) => { body.SizeChanged += Resize; ApplyWidth(); };
        wrapper.Unloaded += (_, _) => body.SizeChanged -= Resize;
        return wrapper;
    }

    private void ShowSettings()
    {
        if (active is not null) return;
        BuildSettingsWorkspace();
    }

    private void ShowAdmin()
    {
        if (active is not null) return;
        BeginScreen("Administrasi", "Area organisasi. Tidak ada role administrator yang diasumsikan dari pengguna lokal.");
        body.Children.Add(Section("Identitas belum diverifikasi", "Manajemen pengguna, role, permission, audit enterprise, dan monitoring memerlukan layanan identitas serta otorisasi organisasi."));
        foreach (var title in new[] { "Pengguna dan role", "Audit organisasi", "Monitoring layanan" })
            body.Children.Add(Section(title, "Belum terhubung; tidak menampilkan data contoh sebagai data nyata.", new Button { Content = "Belum tersedia", IsEnabled = false }));
    }

    private void ShowIssueReport()
    {
        if (active is not null) return;
        BeginScreen("Laporkan masalah", "Buat ringkasan lokal untuk disalin ke percakapan proyek. Tidak ada pengiriman otomatis.");
        var title = new TextBox { Header = "Judul masalah", MaxLength = 120 };
        var detail = new TextBox { Header = "Langkah dan hasil yang Anda temukan", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 120, MaxLength = 4000 };
        var feedback = new TextBlock { TextWrapping = TextWrapping.Wrap };
        body.Children.Add(title); body.Children.Add(detail);
        body.Children.Add(Section("Tangkapan layar (opsional)", "Tekan Win + Shift + S secara manual, pilih area yang relevan, lalu tempel di percakapan proyek. Periksa dan tutupi nama, isi dokumen, serta kata sandi sebelum mengirim. Aplikasi tidak mengambil screenshot otomatis."));
        body.Children.Add(ActionButton("Salin ringkasan laporan", () =>
        {
            if (string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(detail.Text)) { feedback.Text = "Isi judul dan langkah masalah terlebih dahulu."; return; }
            try
            {
                var data = new DataPackage(); data.SetText("PERTAMINA OTO — laporan lokal\n" + title.Text.Trim() + "\n\n" + detail.Text.Trim()); Clipboard.SetContent(data);
                feedback.Text = "Ringkasan disalin. Periksa isinya sebelum menempel dan mengirim. Belum ada tiket yang dikirim.";
            }
            catch (Exception) { feedback.Text = "Clipboard tidak tersedia. Pilih dan salin teks laporan secara manual."; }
        }));
        body.Children.Add(feedback);
    }

    private void ShowPendingWorkspace(ToolCard tool)
    {
        body.Children.Add(Section("01  File sumber", "Area dokumen disiapkan. Pemilihan input dinonaktifkan sampai mesin fitur ini tersedia.", new Button { Content = "Pilih file — belum tersedia", IsEnabled = false }));
        var options = new StackPanel { Spacing = 12 };
        var choices = tool.Id switch
        {
            "ocr" => new[] { "Bahasa pengenalan", "Halaman yang diproses" },
            "scan" => new[] { "Perangkat scanner", "Resolusi dan warna" },
            "pdf-jpg" => new[] { "Halaman", "Resolusi gambar" },
            "pdfa" => new[] { "Profil PDF/A", "Hasil validasi konformitas" },
            "edit" => new[] { "Teks dan objek", "Halaman aktif" },
            "forms" => new[] { "Field formulir", "Nilai field" },
            "sign" => new[] { "Tanda tangan visual", "Halaman dan posisi" },
            "redact" => new[] { "Area redaksi permanen", "Pemeriksaan konten tersembunyi" },
            "compare" => new[] { "Dokumen pembanding", "Mode perbandingan" },
            "translate" => new[] { "Bahasa sumber", "Bahasa tujuan" },
            "summary" => new[] { "Cakupan halaman", "Panjang ringkasan" },
            _ => new[] { "Format hasil", "Cakupan halaman" }
        };
        foreach (var option in choices) options.Children.Add(new TextBox { Header = option, PlaceholderText = "Tersedia setelah integrasi mesin", IsEnabled = false });
        body.Children.Add(Section("02  Pengaturan", "Opsi belum dapat dijalankan atau disimpan.", options));
        body.Children.Add(Section("03  Hasil", "Belum ada hasil. Aplikasi tidak membuat dokumen atau progress simulasi.", new Button { Content = "Proses — belum tersedia", IsEnabled = false }));
    }
}
