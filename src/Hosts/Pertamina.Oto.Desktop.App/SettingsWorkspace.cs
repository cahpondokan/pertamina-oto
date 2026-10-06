using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private string? defaultFolder;
    private string preferenceNotice = "";
    private static string PreferenceFile
    {
        get
        {
#if DEBUG
            if (restartAuditPreference is not null) return restartAuditPreference;
#endif
            return Path.Combine(PortableProfile.DataRoot, "preferences.json");
        }
    }

    private void LoadFolderPreference()
    {
        try
        {
            if (!File.Exists(PreferenceFile))
            {
                var automatic = PortableProfile.IsPortable ? Path.Combine(PortableProfile.DataRoot, "Hasil")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pertamina OTO", "Hasil");
                Directory.CreateDirectory(automatic);
                defaultFolder = automatic; folder = automatic; location.Text = "Folder hasil: " + automatic;
                return;
            }
            var value = System.Text.Json.JsonSerializer.Deserialize<FolderPreference>(File.ReadAllText(PreferenceFile));
            if (value?.DefaultFolder is { } path && Path.IsPathFullyQualified(path) && !path.StartsWith(@"\\", StringComparison.Ordinal) && Directory.Exists(path))
            { defaultFolder = path; folder = path; location.Text = "Folder hasil: " + path; }
            else preferenceNotice = "Folder default tidak tersedia. Pilih folder kembali.";
        }
        catch (Exception) { preferenceNotice = "Preferensi folder tidak dapat dibaca. Pilih folder kembali."; }
    }

    private sealed record FolderPreference(string DefaultFolder);

    private Grid BuildOutputLocation()
    {
        var row = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 8, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var label = Copy("", 12);
        label.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Source = location, Path = new PropertyPath("Text"), Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay });
        row.Children.Add(label);
        var settings = new Button { Content = new FontIcon { Glyph = "\uE713", FontSize = 18 }, VerticalAlignment = VerticalAlignment.Center };
        ToolTipService.SetToolTip(settings, "Atur lokasi hasil");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(settings, "Atur lokasi hasil");
        var menu = new MenuFlyout();
        var session = new MenuFlyoutItem { Text = "Ganti folder untuk sesi ini" };
        session.Click += async (_, _) => await PickFolderAsync();
        var permanent = new MenuFlyoutItem { Text = "Ganti folder default semua alat" };
        permanent.Click += async (_, _) => { await ChooseDefaultFolderAsync(); status.Text = preferenceNotice; };
        var open = new MenuFlyoutItem { Text = "Buka folder hasil" };
        open.Click += async (_, _) =>
        {
            try
            {
                if (folder is null || !await Windows.System.Launcher.LaunchFolderAsync(await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folder)))
                    status.Text = "Folder belum tersedia. Pilih folder hasil.";
            }
            catch (Exception) { status.Text = "Folder tidak dapat dibuka. Periksa lokasi penyimpanan."; }
        };
        menu.Items.Add(session); menu.Items.Add(permanent); menu.Items.Add(new MenuFlyoutSeparator()); menu.Items.Add(open);
        settings.Flyout = menu; editable.Add(settings); Grid.SetColumn(settings, 1); row.Children.Add(settings);
        return row;
    }

    private async Task ChooseDefaultFolderAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker(); picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var chosen = await picker.PickSingleFolderAsync();
            if (chosen is null) return;
            var path = chosen.Path;
            if (path.StartsWith(@"\\", StringComparison.Ordinal)) { preferenceNotice = "Pilih folder lokal untuk default offline."; return; }
            folder = path; location.Text = "Folder hasil: " + path;
            Directory.CreateDirectory(Path.GetDirectoryName(PreferenceFile)!);
            var temp = PreferenceFile + ".tmp";
            File.WriteAllText(temp, System.Text.Json.JsonSerializer.Serialize(new FolderPreference(folder)));
            File.Move(temp, PreferenceFile, true);
            defaultFolder = folder; preferenceNotice = "Folder default tersimpan untuk semua alat dan pembukaan aplikasi berikutnya.";
        }
        catch (Exception) { preferenceNotice = "Folder berlaku untuk sesi ini, tetapi preferensi tidak berhasil disimpan."; }
    }

    private void BuildSettingsWorkspace()
    {
        BeginScreen("Pengaturan dasar", "Sesuaikan ruang kerja Anda.");
        var content = new StackPanel { Spacing = 14, MaxWidth = 850, HorizontalAlignment = HorizontalAlignment.Stretch };
        var wrapper = new Grid { MaxWidth = 850, HorizontalAlignment = HorizontalAlignment.Stretch };
        wrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        void Row(string glyph, string title, string detail, FrameworkElement action)
        {
            var row = new Grid { Padding = new Thickness(16), ColumnSpacing = 14, RowSpacing = 8 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row.Children.Add(new FontIcon { Glyph = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 23, Foreground = Brush(23, 104, 212) });
            var label = new StackPanel { Spacing = 4 };
            label.Children.Add(new TextBlock { Text = title, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            label.Children.Add(Copy(detail, 12)); Grid.SetColumn(label, 1); row.Children.Add(label);
            action.VerticalAlignment = VerticalAlignment.Center; Grid.SetColumn(action, 2); row.Children.Add(action);
            row.SizeChanged += (_, _) => { var narrow = row.ActualWidth < 620; Grid.SetColumnSpan(label, narrow ? 2 : 1); Grid.SetColumn(action, narrow ? 1 : 2); Grid.SetRow(action, narrow ? 1 : 0); };
            content.Children.Add(new Border { Child = row, Background = Brush(255, 255, 255), CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(1), BorderBrush = Brush(221, 229, 241) });
        }
        content.Children.Add(Copy("Umum", 15));
        Row("\uE777", "Update aplikasi", SafeRunningVersion() + (PortableProfile.IsPortable ? " · Portable: ekstrak ZIP baru ke folder terpisah." : " · Periksa versi di oto.ayani.id. Dokumen tetap lokal. Update lokal tersedia sebagai alternatif."),
            PortableProfile.IsPortable ? Copy("Update melalui ZIP", 13) : ActionButton("Periksa update", async () => await CheckLocalUpdateAsync()));
        AddOnlineUpdateSetting(Row, content);
        AddPdfDefaultSetting(Row, content);
        Row("\uE774", "Bahasa aplikasi", "Bahasa lain belum tersedia.", Copy("Bahasa Indonesia", 13));
        Row("\uE8B7", "Folder hasil default", defaultFolder ?? "Belum dipilih.", ActionButton(defaultFolder is null ? "Pilih folder" : "Ganti folder", async () => { await ChooseDefaultFolderAsync(); ShowSettings(); }));
        content.Children.Add(Copy("Dipakai semua alat. Pilih folder lain di halaman alat untuk mengganti lokasi sesi tanpa mengubah default tersimpan.", 12));
        if (preferenceNotice.Length > 0) content.Children.Add(Copy(preferenceNotice, 12));
        content.Children.Add(Copy("Privasi & kemudahan", 15));
        Row("\uE72E", "Pemrosesan lokal", "Dokumen tidak diunggah. Favorit serta nama dan lokasi hasil disimpan di perangkat.", Copy("Lokal", 13));
        content.Children.Add(new Expander { Header = "⌨  Navigasi keyboard", HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = Copy("Tab / Shift+Tab    Pindah kontrol\nEnter / Spasi       Aktifkan tombol", 13) });
        Row("\uE946", "Tentang PERTAMINA OTO", "Informasi versi, lisensi, dan lainnya.", BuildAboutSettingsButton());
        content.Children.Add(Copy("Gunakan salinan uji. Belum untuk dokumen sensitif atau produksi.", 12));
        content.Children.Add(Copy(PortableProfile.IsPortable ? "Mode portable menyimpan preferensi dan riwayat di folder Data. Tidak mendaftarkan pembuka PDF atau memasang update ke instalasi lama."
            : "Tombol Jadikan default mendaftarkan OTO sebagai pembuka PDF pada akun Windows ini. Windows tetap meminta Anda memilih aplikasi default.", 12));
        wrapper.Children.Add(content); body.Children.Add(wrapper);
    }
}
