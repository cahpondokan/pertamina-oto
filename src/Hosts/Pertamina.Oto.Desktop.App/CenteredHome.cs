using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly ContentCenteredCatalogPanel centeredGrid = new() { Spacing = 12 };
    private TextBox? centeredSearch;
    private Button? centeredHomeNavigation;
    private TextBlock? centeredCount;
    private StackPanel? centeredEmpty;
    private string centeredCategory = "all";
    private string centeredQuery = "";
    private readonly Dictionary<string, ToggleButton> centeredFilters = [];
    private static string CenteredCategory(ToolCard t) => t.Id switch
    {
        "merge" or "split" or "organize" or "remove" or "extract" or "rotate" => "pages",
        "images" or "word-pdf" or "pdf-jpg" => "convert",
        "unlock" or "protect" => "security", _ => "edit"
    };
    private static (string Title, string Description) CenteredCopy(ToolCard t) => t.Id switch
    {
        "merge" => ("Gabungkan PDF", "Satukan beberapa dokumen"),
        "split" => ("Pisahkan PDF", "Pisahkan menjadi beberapa file"),
        "organize" => ("Atur halaman", "Susun ulang urutan halaman"),
        "compress" => ("Kompres PDF", "Kompresi lossless"),
        "remove" => ("Hapus halaman", "Hapus dari salinan hasil"),
        "extract" => ("Ambil halaman", "Simpan halaman terpilih"),
        "images" => ("Gambar ke PDF", "Ubah JPG atau PNG"),
        "word-pdf" => ("Word ke PDF", "Perlu Word lokal"),
        "pdf-jpg" => ("PDF ke JPG", "Simpan halaman sebagai gambar"),
        "rotate" => ("Putar halaman", "Ubah arah halaman"),
        "numbers" => ("Nomor halaman", "Tambahkan nomor halaman"),
        "watermark" => ("Watermark", "Tambahkan teks penanda"),
        "crop" => ("Potong halaman", "Atur area tampilan halaman"),
        "edit" => ("Edit PDF", "Edit teks yang didukung"),
        "unlock" => ("Buka kunci PDF", "Dengan kata sandi pemilik"),
        "protect" => ("Kunci PDF", "Tambahkan kata sandi"), _ => (CatalogName(t), t.Description)
    };
    private Grid BuildCenteredShell() => BuildIntegratedWindowHeader();
    private MenuFlyout BuildIntegratedAppMenu()
    {
        var flyout = new MenuFlyout();
        void Item(string name, Action open) => flyout.Items.Add(MenuCommand(name, () => { open(); return Task.CompletedTask; }));
        Item("Kembali", () => Travel(-1)); Item("Maju", () => Travel(1));
        Item("Riwayat", () => { if (active is null && !importing && !messageOpen) ShowHistory(); });
        Item("Dokumen sesi", () => { CaptureWorkspaceContext(); BuildDocumentHome(); }); Item("Pengaturan", ShowSettings); Item("Bantuan", ShowHelp); Item("Assistant", ShowAssistant); Item("Administrasi", ShowAdmin);
        // Keep existing file/edit/zoom commands and accelerators in a compact submenu.
        var commands = BuildDesktopMenu();
        var bar = commands.Children.OfType<MenuBar>().Single();
        foreach (var source in bar.Items.ToArray())
        {
            var sub = new MenuFlyoutSubItem { Text = source.Title };
            foreach (var item in source.Items.ToArray()) { source.Items.Remove(item); sub.Items.Add(item); }
            flyout.Items.Add(sub);
        }
        return flyout;
    }
    private async Task OpenCenteredReaderAsync()
    {
        if (active is not null || importing || messageOpen) return;
        ShowPdfReader(); await pdfReader!.PickAsync();
    }
    private void BuildCenteredHome()
    {
        BeginScreen("Mau mengerjakan apa hari ini?", ""); selected = null;
        body.Children.Clear(); body.Spacing = 0; root.Background = Brush(245,247,250);
        if (standardPageHeader is not null) standardPageHeader.Visibility = Visibility.Collapsed;
        var heading = new Grid { ColumnSpacing = 20, Margin = new Thickness(0,0,0,22) };
        heading.ColumnDefinitions.Add(new()); heading.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        var title = new StackPanel { Spacing = 6 };
        title.Children.Add(new TextBlock { Text = "Pilih alat, mulai bekerja", FontSize = 25, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(21,43,73), TextWrapping = TextWrapping.Wrap });
        title.Children.Add(new TextBlock { Text = "Alat PDF yang Anda perlukan, tanpa menu berulang.", FontSize = 13, Foreground = Brush(88,106,128), TextWrapping = TextWrapping.Wrap });
        heading.Children.Add(title);
        var open = HomeButton("Buka & baca PDF", "\uE8A5", async () => await OpenCenteredReaderAsync(), true); open.CornerRadius = new CornerRadius(6); open.MinHeight = 44; open.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(open,1); heading.Children.Add(open); ResponsiveLayout.Header(heading,title,open,700); body.Children.Add(heading);
        centeredSearch = new TextBox { Text = centeredQuery, PlaceholderText = "Cari alat PDF...", FontSize = 16, MinHeight = 46, CornerRadius = new CornerRadius(7), Background = Brush(255,255,255), BorderBrush = Brush(220,227,236) };
        AutomationProperties.SetName(centeredSearch,"Cari alat PDF");
        centeredSearch.TextChanged += (_,_) => { centeredQuery = centeredSearch.Text; RefreshCenteredCards(); };
        var searchRow = new Grid { ColumnSpacing=10, Background=Brush(255,255,255), BorderBrush=Brush(220,227,236),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(7),Padding=new Thickness(12,0,12,0) };
        searchRow.ColumnDefinitions.Add(new(){Width=GridLength.Auto});searchRow.ColumnDefinitions.Add(new());searchRow.Children.Add(HistoryIcon("search",19));centeredSearch.BorderThickness=new Thickness(0);centeredSearch.Background=new SolidColorBrush(Microsoft.UI.Colors.Transparent);centeredSearch.Padding=new Thickness(0,9,0,9);Grid.SetColumn(centeredSearch,1);searchRow.Children.Add(centeredSearch);body.Children.Add(searchRow);
        var filters = new ResponsiveWrapPanel { Spacing = 7, Margin = new Thickness(0,15,0,8) }; centeredFilters.Clear();
        foreach (var (id,label) in new[] { ("all","Semua (16)"), ("favorites","Favorit"), ("pages","Halaman"), ("convert","Konversi"), ("edit","Edit"), ("security","Keamanan") })
        {
            var chip = new ToggleButton { Content = label, MinHeight = 39, Padding = new Thickness(13,7,13,7), CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1) };
            chip.Template = CenteredToggleTemplate();
            AutomationProperties.SetName(chip,label); chip.Click += (_,_) => { centeredCategory = id; RefreshCenteredCards(); }; centeredFilters[id]=chip; filters.Children.Add(chip);
        }
        body.Children.Add(filters);
        var meta = new Grid { Margin = new Thickness(0,11,0,14) }; meta.ColumnDefinitions.Add(new()); meta.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        centeredCount = Copy("",12); AutomationProperties.SetLiveSetting(centeredCount,Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite); meta.Children.Add(centeredCount);
        var hint = Copy("Bintang menyimpan alat favorit",12); Grid.SetColumn(hint,1); meta.Children.Add(hint); ResponsiveLayout.Header(meta,centeredCount,hint,440); body.Children.Add(meta);
        body.Children.Add(centeredGrid);
        centeredEmpty = new StackPanel { Spacing = 8, Padding = new Thickness(20,48,20,48) }; body.Children.Add(centeredEmpty);
        body.Children.Add(new Border { Margin = new Thickness(0,24,0,0), Padding = new Thickness(0,14,0,0), BorderThickness = new Thickness(0,1,0,0), BorderBrush = Brush(220,227,236), Child = Copy("●  Alat lokal · Dokumen tetap di perangkat     |     PERTAMINA OTO · Pengembangan " + ApplicationVersion.Running().ProductVersion,12) });
        RefreshCenteredCards(); LayoutCards();
    }
    private void RefreshCenteredCards()
    {
        if (centeredCount is null || centeredEmpty is null) return;
        var order = new[] { "merge","split","organize","compress","remove","extract","images","word-pdf","pdf-jpg","rotate","numbers","watermark","crop","edit","unlock","protect" };
        var found = FindTools(centeredQuery).Where(t=>t.Available && (centeredCategory=="all" || (centeredCategory=="favorites" ? favorites.Contains(t.Id) : CenteredCategory(t)==centeredCategory))).OrderBy(t=>Array.IndexOf(order,t.Id)).ToArray();
        centeredGrid.Children.Clear(); foreach (var tool in found) centeredGrid.Children.Add(BuildCenteredCard(tool));
        centeredCount.Text = found.Length + " alat" + (centeredCategory=="favorites" ? " favorit" : " tersedia");
        foreach (var (id,chip) in centeredFilters) { chip.IsChecked=id==centeredCategory; chip.Background=id==centeredCategory ? Brush(238,246,255) : Brush(245,247,250); chip.BorderBrush=id==centeredCategory ? Brush(7,103,220) : Brush(245,247,250); chip.Foreground=id==centeredCategory ? Brush(7,103,220) : Brush(88,106,128); }
        if (centeredFilters.TryGetValue("favorites",out var favoriteFilter)) favoriteFilter.Content = "☆ Favorit ("+favorites.Count+")";
        centeredEmpty.Children.Clear(); centeredEmpty.Visibility=found.Length==0 ? Visibility.Visible : Visibility.Collapsed;
        if (found.Length==0) { centeredEmpty.Children.Add(HistoryText(centeredCategory=="favorites" && favorites.Count==0 ? "Belum ada alat favorit" : "Tidak ada alat yang cocok",17,true)); centeredEmpty.Children.Add(Copy(centeredCategory=="favorites" && favorites.Count==0 ? "Tandai bintang pada alat yang sering Anda gunakan." : "Coba kata lain atau tampilkan semua alat.")); var reset=HomeButton("Tampilkan semua alat","\uE80A",()=>{ centeredCategory="all"; centeredQuery=""; centeredSearch!.Text=""; RefreshCenteredCards(); }); centeredEmpty.Children.Add(reset); }
        LayoutCenteredCards();
    }
    private void LayoutCenteredCards()
    {
        centeredGrid.InvalidateMeasure();
    }
    private static ControlTemplate CenteredToggleTemplate() => (ControlTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load("""<ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" TargetType="ToggleButton"><Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="{TemplateBinding CornerRadius}"><ContentPresenter Content="{TemplateBinding Content}" Padding="{TemplateBinding Padding}" Foreground="{TemplateBinding Foreground}" HorizontalContentAlignment="Center" VerticalContentAlignment="Center"/></Border></ControlTemplate>""");
    private Grid BuildCenteredCard(ToolCard tool)
    {
        var copy = CenteredCopy(tool);
        var tile = new Grid { Height = 160, CornerRadius = new CornerRadius(7), Background = Brush(255,255,255), BorderBrush = Brush(220,227,236), BorderThickness = new Thickness(1), RenderTransform = new TranslateTransform() };
        var icon = CenteredIcon(tool.Id);
        icon.RenderTransform = new CompositeTransform { CenterX=17, CenterY=17 };
        var content = new StackPanel { Spacing = 11, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
        content.Children.Add(icon);
        var words = new StackPanel { Spacing = 3 }; words.Children.Add(new TextBlock { Text=copy.Title,FontSize=15,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold,TextAlignment=TextAlignment.Center,TextWrapping=TextWrapping.Wrap,Foreground=Brush(21,43,73) }); words.Children.Add(new TextBlock { Text=copy.Description,FontSize=12,TextAlignment=TextAlignment.Center,TextWrapping=TextWrapping.Wrap,Foreground=Brush(88,106,128) }); content.Children.Add(words);
        // Equal text padding plus equal star reservation on both sides keep text centered.
        // The panel measures this natural width, capping only at the current viewport.
        var launch = new Button { Content=content, HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Center,Padding=new Thickness(35,24,35,18),Background=new SolidColorBrush(Microsoft.UI.Colors.Transparent),BorderThickness=new Thickness(0),CornerRadius=new CornerRadius(6) };
        // Paint the full button surface transparently so padding and icon gaps receive clicks.
        launch.Template = (ControlTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load("""<ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" TargetType="Button"><Border Background="{TemplateBinding Background}" CornerRadius="{TemplateBinding CornerRadius}"><ContentPresenter Content="{TemplateBinding Content}" Padding="{TemplateBinding Padding}" HorizontalContentAlignment="Stretch" VerticalContentAlignment="Center"/></Border></ControlTemplate>""");
        AutomationProperties.SetName(launch,copy.Title); ToolTipService.SetToolTip(launch,tool.Description); launch.Click+=(_,_)=>OpenHomeTool(tool); tile.Children.Add(launch);
        var favorite = new ToggleButton { Content=favorites.Contains(tool.Id) ? "★" : "☆", IsChecked=favorites.Contains(tool.Id),Width=35,Height=35,MinWidth=35,MinHeight=35,FontSize=21,Padding=new Thickness(0),Margin=new Thickness(0,5,5,0),HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Background=new SolidColorBrush(Microsoft.UI.Colors.Transparent),BorderThickness=new Thickness(0),Foreground=favorites.Contains(tool.Id)?Brush(169,102,0):Brush(88,106,128) };
        favorite.Template = CenteredToggleTemplate();
        AutomationProperties.SetName(favorite,"Favorit "+copy.Title);
        void FavoriteChanged(object sender, RoutedEventArgs args) { if(favorite.IsChecked==true)favorites.Add(tool.Id);else favorites.Remove(tool.Id); SaveLibrary(); favorite.Content=favorite.IsChecked==true?"★":"☆";favorite.Foreground=favorite.IsChecked==true?Brush(169,102,0):Brush(88,106,128); if(centeredCategory=="favorites") { RefreshCenteredCards(); centeredFilters["favorites"].Focus(FocusState.Programmatic); } else { centeredFilters["favorites"].Content="☆ Favorit ("+favorites.Count+")"; centeredCount!.Text=libraryNotice.Length>0?libraryNotice:copy.Title+(favorite.IsChecked==true?" ditambahkan ke favorit":" dihapus dari favorit"); } }
        favorite.Checked += FavoriteChanged; favorite.Unchecked += FavoriteChanged; tile.Children.Add(favorite);
        void Highlight(bool on)
        {
            tile.Shadow = on ? new ThemeShadow() : null;
            tile.Translation = new System.Numerics.Vector3(0,0,on ? 8 : 0);
            foreach(var stroke in Descendants(icon).OfType<Microsoft.UI.Xaml.Shapes.Path>()) stroke.Stroke=on?Brush(7,103,220):Brush(21,43,73);
            tile.BorderBrush=on?Brush(7,103,220):Brush(220,227,236); tile.Background=on?Brush(247,251,255):Brush(255,255,255);
            var animations = new UISettings().AnimationsEnabled;
            var move=(TranslateTransform)tile.RenderTransform;
            if(!animations) { move.Y=0; return; }
            var storyboard=new Storyboard(); var lift=new DoubleAnimation { To=on?-3:0,Duration=new Duration(TimeSpan.FromMilliseconds(180)),EnableDependentAnimation=true,EasingFunction=new CubicEase { EasingMode=EasingMode.EaseOut } }; Storyboard.SetTarget(lift,move); Storyboard.SetTargetProperty(lift,"Y"); storyboard.Children.Add(lift); storyboard.Begin();
            var iconTransform = (CompositeTransform)icon.RenderTransform;
            var motion = new Storyboard();
            foreach (var (property,value) in new[] { ("TranslateY",on?-2d:0d), ("ScaleX",on?1.08:1), ("ScaleY",on?1.08:1) })
            {
                var animation = new DoubleAnimation { To=value, Duration=new Duration(TimeSpan.FromMilliseconds(180)), EnableDependentAnimation=true, EasingFunction=new CubicEase { EasingMode=EasingMode.EaseOut } };
                Storyboard.SetTarget(animation,iconTransform);Storyboard.SetTargetProperty(animation,property);motion.Children.Add(animation);
            }
            motion.Begin();
        }
        tile.PointerEntered+=(_,_)=>Highlight(true); tile.PointerExited+=(_,_)=>Highlight(false); tile.GotFocus+=(_,_)=>Highlight(true); tile.LostFocus+=(_,_)=>Highlight(false);
        return tile;
    }
    private static FrameworkElement CenteredIcon(string id)
    {
        if (id == "merge") return MergeBiblo(48);
        if (id == "organize") return OrganizerBiblo(48);
        var path = id switch
        {
            "merge" => "M8,3 L17,3 L21,7 L21,20 L8,20 Z M17,3 L17,7 L21,7 M5,7 L3,7 L3,23 L16,23 L16,22",
            "split" => "M12,22 L12,12 M12,12 C12,7 5,9 5,3 M2,6 L5,3 L8,6 M12,12 C12,7 19,9 19,3 M16,6 L19,3 L22,6",
            "organize" => "M3,3 L10,3 L10,10 L3,10 Z M14,3 L21,3 L21,10 L14,10 Z M3,14 L10,14 L10,21 L3,21 Z M14,14 L21,14 L21,21 L14,21 Z",
            "compress" => "M3,3 L9,9 M3,9 L9,9 L9,3 M21,21 L15,15 M21,15 L15,15 L15,21",
            "remove" => "M3,6 L21,6 M9,6 L9,3 L15,3 L15,6 M5,6 L6,21 L18,21 L19,6 M10,10 L10,17 M14,10 L14,17",
            "extract" => "M14,2 L6,2 L6,22 L18,22 L18,15 M14,2 L18,6 L14,6 Z M11,11 L23,11 M20,8 L23,11 L20,14",
            "images" => "M3,3 L21,3 L21,21 L3,21 Z M3,17 L9,11 L14,16 L17,13 L21,17 M14,7 A2,2 0 1 0 18,7 A2,2 0 1 0 14,7",
            "word-pdf" => "M14,2 L4,2 L4,22 L20,22 L20,8 L14,2 L14,8 L20,8 M7,12 L17,12 M12,12 L12,19 M9,19 L15,19",
            "pdf-jpg" => "M14,2 L4,2 L4,22 L20,22 L20,8 L14,2 L14,8 L20,8 M4,19 L10,13 L15,18 L18,15 L20,17",
            "rotate" => "M21,10 A9,9 0 1 0 21,16 M21,3 L21,10 L14,10",
            "numbers" => "M10,6 L21,6 M10,12 L21,12 M10,18 L21,18 M3,3 L5,3 L5,8 M3,10 L6,10 L6,12 L3,15 L6,15 M3,17 L6,17 L6,22 L3,22 M3,19 L6,19",
            "watermark" => "M12,3 C10,7 5,11 5,15 A7,7 0 0 0 19,15 C19,11 14,7 12,3 Z M9,15 C9,18 11,19 13,19",
            "crop" => "M6,3 L6,18 L21,18 M3,6 L18,6 L18,21",
            "edit" => "M12,3 L3,3 L3,21 L21,21 L21,12 M9,15 L10,10 L19,1 L23,5 L14,14 Z M16,4 L20,8",
            "unlock" => "M8,10 L8,6 A4,4 0 0 1 16,6 M5,10 L19,10 L19,22 L5,22 Z M12,15 L12,18",
            _ => "M8,10 L8,6 A4,4 0 0 1 16,6 L16,10 M5,10 L19,10 L19,22 L5,22 Z M12,15 L12,18"
        };
        return (FrameworkElement)Microsoft.UI.Xaml.Markup.XamlReader.Load($"<Viewbox xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='34' Height='34' HorizontalAlignment='Center'><Canvas Width='24' Height='24'><Path Data='{path}' Stroke='#152B49' StrokeThickness='1.65' StrokeLineJoin='Round' StrokeStartLineCap='Round' StrokeEndLineCap='Round'/></Canvas></Viewbox>");
    }
}

