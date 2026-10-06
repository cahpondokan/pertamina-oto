using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly List<string> homeFiles = [];
    private static readonly string[] HomeExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private TextBlock? homeFileStatus;
    private readonly Grid quickTools = new() { ColumnSpacing = 10, RowSpacing = 10 };

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["merge"] = "gabung gabungkan satukan menyatukan marge merge sambung",
        ["split"] = "pisah pisahkan pecah membagi", ["compress"] = "kompres kecilkan perkecil ukuran",
        ["rotate"] = "putar rotasi miring", ["images"] = "gambar foto jpg png ke pdf",
        ["remove"] = "hapus halaman", ["extract"] = "ambil ekstrak halaman", ["organize"] = "atur susun urut halaman",
        ["watermark"] = "watermark cap stempel tanda air", ["numbers"] = "nomor halaman penomoran",
        ["protect"] = "kunci sandi lindungi enkripsi", ["unlock"] = "buka kunci sandi",
        ["crop"] = "potong pangkas margin", ["ocr"] = "ocr baca teks scan", ["scan"] = "pindai scanner",
        ["summary"] = "ringkas ringkasan rangkum", ["translate"] = "terjemah terjemahkan bahasa",
        ["sign"] = "tanda tangan", ["compare"] = "banding bandingkan beda", ["repair"] = "perbaiki rusak",
        ["redact"] = "redaksi sensor", ["forms"] = "form formulir", ["edit"] = "edit sunting"
    };

    private static IEnumerable<ToolCard> FindTools(string query)
    {
        var words = query.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return ToolCatalog.All.Where(tool =>
        {
            var haystack = (tool.Name + " " + CatalogName(tool) + " " + tool.Group + " " + CategoryName(tool.Group) + " " + Aliases.GetValueOrDefault(tool.Id, "")).ToLowerInvariant();
            return words.All(word => haystack.Contains(word, StringComparison.Ordinal));
        });
    }

    private static TextBlock Copy(string text, double size = 14) => new()
    {
        Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Foreground = Brush(83, 102, 130)
    };

    private static Border LargeIcon(string glyph, bool purple = false) => new()
    {
        Width = 60, Height = 60, CornerRadius = new CornerRadius(16), HorizontalAlignment = HorizontalAlignment.Left,
        Background = purple ? Brush(237, 226, 255) : Brush(222, 237, 255),
        Child = ClearGlyph(glyph, purple)
    };

    private static UIElement ClearGlyph(string glyph, bool purple)
    {
        var path = glyph switch
        {
            "\uE74A" => "M2,2 L8,8 M3,8 L8,8 L8,3 M22,2 L16,8 M16,3 L16,8 L21,8 M2,22 L8,16 M3,16 L8,16 L8,21 M22,22 L16,16 M16,21 L16,16 L21,16",
            "\uE8A7" => "M9,8 C9,12 3,12 3,8 C3,4 9,4 9,8 M9,18 C9,22 3,22 3,18 C3,14 9,14 9,18 M8,10 L21,21 M8,16 L21,3",
            "\uE945" => "M12,2 C12,8 8,12 2,12 C8,12 12,16 12,22 C12,16 16,12 22,12 C16,12 12,8 12,2 Z",
            "\uE8D3" => "M8,16 L8,12 C8,10 10,10 10,8 L10,4 C10,1 14,1 14,4 L14,8 C14,10 16,10 16,12 L16,16 M5,16 L19,16 L21,21 L3,21 Z",
            _ => null
        };
        if (path is null) return new FontIcon { Glyph = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 32, Foreground = purple ? Brush(117, 65, 232) : Brush(15, 105, 232) };
        var color = purple ? "#7541E8" : "#0F69E8";
        return (UIElement)Microsoft.UI.Xaml.Markup.XamlReader.Load($"<Viewbox xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='32' Height='32'><Canvas Width='24' Height='24'><Path Data='{path}' Stroke='{color}' StrokeThickness='1.7' StrokeLineJoin='Round' StrokeStartLineCap='Round' StrokeEndLineCap='Round'/></Canvas></Viewbox>");
    }

    private static Button HomeButton(string label, string glyph, Action action, bool primary = false, bool purple = false)
    {
        var button = new Button { Content = IconLabel(glyph, label), HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center, MinHeight = 46, Padding = new Thickness(10),
            CornerRadius = new CornerRadius(10), Background = primary ? (purple ? Brush(116, 65, 231) : Brush(8, 105, 229)) : Brush(255, 255, 255),
            Foreground = primary ? Brush(255, 255, 255) : Brush(15, 105, 232), BorderBrush = Brush(161, 194, 241) };
        AutomationProperties.SetName(button, label); button.Click += (_, _) => action(); return button;
    }

    private static Border EntryPanel(string number, string title, string glyph, StackPanel content, int tone)
    {
        var panel = new StackPanel { Spacing = 18 };
        var heading = new Grid { ColumnSpacing = 14 };
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.Children.Add(LargeIcon(glyph, tone == 2));
        var labels = new StackPanel { Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        labels.Children.Add(new TextBlock { Text = number, FontSize = 12, Foreground = tone == 2 ? Brush(117, 65, 232) : Brush(15, 105, 232) });
        labels.Children.Add(new TextBlock { Text = title, FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        Grid.SetColumn(labels, 1); heading.Children.Add(labels); panel.Children.Add(heading);
        panel.Children.Add(content);
        return new Border { Child = panel, Padding = new Thickness(22), CornerRadius = new CornerRadius(18),
            Background = tone == 1 ? Brush(237, 246, 255) : tone == 2 ? Brush(248, 243, 255) : Brush(255, 255, 255),
            BorderBrush = tone == 2 ? Brush(230, 217, 253) : Brush(220, 230, 243), BorderThickness = new Thickness(1) };
    }

    private void BuildDesignedHome()
    {
        if (homeFiles.Count > 0) { BuildDocumentHome(); return; }
        BeginScreen("Mau mengerjakan apa hari ini?", "Mulai dari dokumen, temukan alat, atau ceritakan kebutuhan Anda.");
        foreach (var item in navigation) item.Background = Brush(7, 29, 61);
        navigation[0].Background = Brush(23, 104, 212);
        homeEntries.Children.Clear();
        var documents = new StackPanel { Spacing = 12 };
        var dropContent = new StackPanel { Spacing = 10, HorizontalAlignment = HorizontalAlignment.Center };
        dropContent.Children.Add(LargeIcon("\uE8A5"));
        dropContent.Children.Add(new TextBlock { Text = "Tarik file ke sini", FontSize = 17, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        dropContent.Children.Add(Copy("atau salin file lalu tekan Ctrl + V", 12));
        var drop = new Border { Child = dropContent, Padding = new Thickness(16), BorderBrush = Brush(133, 177, 237), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), AllowDrop = true };
        drop.DragOver += (_, e) => { e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None; };
        drop.Drop += async (_, e) => { var deferral = e.GetDeferral(); try { await ReceiveHomeFilesAsync(await e.DataView.GetStorageItemsAsync()); } catch (Exception) { HomeNotice("File tidak dapat dibaca. Coba Pilih file."); } finally { deferral.Complete(); } };
        documents.Children.Add(drop);
        var paste = HomeButton("Tempel dari clipboard", "\uE77F", async () => await PasteHomeAsync(), true);
        var shortcut = new KeyboardAccelerator { Key = VirtualKey.V, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) =>
        {
            if (homeEntries.Parent is null || FocusManager.GetFocusedElement(root.XamlRoot) is TextBox or PasswordBox) return;
            e.Handled = true; await PasteHomeAsync();
        };
        documents.Children.Add(paste); // Ctrl+V is handled by the window root.
        documents.Children.Add(HomeButton("Pilih file", "\uE8B7", async () =>
        {
            try
            {
                var picker = new FileOpenPicker(); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
                foreach (var extension in HomeExtensions) picker.FileTypeFilter.Add(extension);
                await ReceiveHomeFilesAsync(await picker.PickMultipleFilesAsync());
            }
            catch (Exception) { HomeNotice("Pemilih file tidak dapat dibuka."); }
        }));
        homeFileStatus = Copy(homeFiles.Count == 0 ? "PDF, JPG, PNG · Tidak diunggah" : $"{homeFiles.Count} file siap. Pilih alat di sebelah kanan.", 12);
        AutomationProperties.SetLiveSetting(homeFileStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        documents.Children.Add(homeFileStatus);
        documents.Children.Add(HomeButton("Undo dokumen", "\uE7A7", () => RestoreHome(false)));
        homeEntries.Children.Add(EntryPanel("01 · DOKUMEN", "Tempel dokumen", "\uE77F", documents, 1));

        var discovery = new StackPanel { Spacing = 14 };
        discovery.Children.Add(Copy("Tidak perlu hafal nama alat."));
        var search = new TextBox { PlaceholderText = "Misalnya: gabung pdf", MinHeight = 46, CornerRadius = new CornerRadius(10) };
        AutomationProperties.SetName(search, "Cari dengan kata Anda"); discovery.Children.Add(search);
        var chips = new ResponsiveWrapPanel { Spacing = 6 };
        foreach (var term in new[] { "gabung", "satukan", "merge" })
        {
            var chip = new Button { Content = term, CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 6, 10, 6) };
            chip.Click += (_, _) => search.Text = term; chips.Children.Add(chip);
        }
        discovery.Children.Add(chips);
        var results = new StackPanel { Spacing = 8 };
        void RefreshResults()
        {
            results.Children.Clear();
            var matches = FindTools(search.Text).Take(string.IsNullOrWhiteSpace(search.Text) ? 1 : 3).ToArray();
            foreach (var tool in matches)
            {
                var result = HomeButton(DisplayTool(tool), tool.Id == "merge" ? "\uE8C8" : tool.Glyph, () => OpenHomeTool(tool));
                ToolTipService.SetToolTip(result, tool.Available ? tool.Description : "Belum aktif: " + tool.Name);
                results.Children.Add(result);
            }
            if (matches.Length == 0) results.Children.Add(Copy("Belum ditemukan. Coba kata yang lebih singkat."));
        }
        search.TextChanged += (_, _) => RefreshResults(); RefreshResults();
        discovery.Children.Add(results); discovery.Children.Add(Copy("Juga kenali: kecilkan ukuran, putar, pisahkan", 12));
        discovery.Children.Add(HomeButton("Semua alat", "\uE80A", ShowCatalog));
        homeEntries.Children.Add(EntryPanel("02 · ALAT", "Cari dengan kata Anda", "\uE721", discovery, 0));

        var assistant = new StackPanel { Spacing = 16 };
        assistant.Children.Add(Copy("Jelaskan dengan bahasa Anda sendiri."));
        assistant.Children.Add(new TextBox { PlaceholderText = "Gabungkan dokumen ini menjadi satu PDF.", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 130, MaxLength = 500, IsReadOnly = true, Header = "Kebutuhan Anda (belum aktif)" });
        var review = HomeButton("Tinjau rencana", "\uE945", () => { }, true, true); review.IsEnabled = false;
        assistant.Children.Add(review); assistant.Children.Add(Copy("Konfirmasi sebelum diproses", 12));
        assistant.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning, Message = "Model belum terhubung" });
        homeEntries.Children.Add(EntryPanel("03 · ASSISTANT", "Ceritakan kebutuhan Anda", "\uE945", assistant, 2));
        body.Children.Add(homeEntries);
        body.Children.Add(new Border { Background = Brush(232, 243, 255), CornerRadius = new CornerRadius(12), Padding = new Thickness(16), Child = Copy("Dokumen baru diproses setelah Anda memilih alat dan menekan tombol proses. Gunakan salinan uji.", 13) });
        body.Children.Add(new TextBlock { Text = "Alat yang sering digunakan", FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        quickTools.Children.Clear();
        foreach (var id in new[] { "merge", "compress", "split", "rotate", "images", "watermark" })
        {
            var tool = ToolCatalog.All.First(t => t.Id == id);
            var content = new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Center };
            content.Children.Add(LargeIcon(id == "merge" ? "\uE8C8" : tool.Glyph, id is "merge" or "watermark"));
            content.Children.Add(new TextBlock { Text = DisplayTool(tool), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 13 });
            var button = new Button { Content = content, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center, Padding = new Thickness(12), CornerRadius = new CornerRadius(14), Background = Brush(255, 255, 255), MinHeight = 120 };
            AutomationProperties.SetName(button, DisplayTool(tool)); button.Click += (_, _) => OpenHomeTool(tool); quickTools.Children.Add(button);
        }
        body.Children.Add(quickTools); LayoutCards();
    }

    private static string DisplayTool(ToolCard tool) => tool.Id switch
    {
        "organize" => "Atur halaman PDF", "merge" => "Gabungkan PDF", "compress" => "Kompres PDF", "split" => "Pisahkan PDF", "rotate" => "Putar PDF", "images" => "Gambar ke PDF", "watermark" => "Watermark", _ => tool.Name
    };

    private void HomeNotice(string message) { if (homeFileStatus is not null) homeFileStatus.Text = message; }

    private async Task PasteHomeAsync()
    {
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { HomeNotice("Salin file dari File Explorer, bukan teks atau gambar screenshot, lalu tempel di sini."); return; }
            await ReceiveHomeFilesAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { HomeNotice("Clipboard tidak dapat dibaca. Gunakan Pilih file."); }
    }

    private async Task ReceiveHomeFilesAsync(IEnumerable<IStorageItem> items)
    {
        if (active is not null || importing) return;
        RememberHome();
        var rejected = 0;
        foreach (var item in items.Take(51))
        {
            if (item is not StorageFile file || !HomeExtensions.Contains(file.FileType, StringComparer.OrdinalIgnoreCase)
                || file.Path.StartsWith(@"\\", StringComparison.Ordinal) || string.IsNullOrEmpty(file.Path) || homeFiles.Count >= 50)
            { rejected++; continue; }
            var properties = await file.GetBasicPropertiesAsync();
            if (properties.Size > 100 * 1024 * 1024) { rejected++; continue; }
            if (!homeFiles.Contains(file.Path, StringComparer.OrdinalIgnoreCase)) { homeFiles.Add(file.Path); chosenFiles.Add(file.Path); }
        }
        if (organizerVisible) await AddOrganizerSourcesAsync(); else BuildDesignedHome();
        HomeNotice($"{homeFiles.Count} file siap. Pilih alat. {rejected} ditolak (format/lokasi/ukuran/batas 50 file). Tidak ada pemrosesan otomatis.");
    }

    private void OpenHomeTool(ToolCard tool) => _ = OpenWorkspaceToolAsync(tool);
}
