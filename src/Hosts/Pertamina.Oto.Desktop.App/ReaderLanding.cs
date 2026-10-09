using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class PdfReaderView
    {
        private ScrollViewer? landing;
        private FrameworkElement[] readerChrome = [];
        private readonly StackPanel recentLanding = new() { Spacing = 0 };
        private readonly StackPanel landingSessions = new() { Spacing = 6 };
        private readonly TextBlock landingDefaultStatus = HistoryText("", 12, muted: true);
        private readonly TextBlock defaultHeading = HistoryText("Jadikan OTO pembuka PDF utama", 18, true);
        private readonly TextBlock defaultSubtitle = HistoryText("Buka PDF langsung di OTO dengan klik dua kali dari File Explorer.", 14, muted: true);
        private Button? landingDefaultButton;
        private Button? landingOpen;
        private int defaultStatusGeneration;
#if DEBUG
        private PdfAssociation.DefaultStatus? landingDefaultTestStatus;
#endif

        private void BuildLanding(FrameworkElement header, FrameworkElement toolbar, FrameworkElement footer)
        {
            if (previewOnly) return;
            readerChrome = [header, toolbar, footer, workspace];
            var content = new StackPanel { Spacing = 18, MaxWidth = 1240, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(32, 24, 32, 24) };
            var heading = new Grid(); heading.ColumnDefinitions.Add(new()); heading.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            var names = new StackPanel { Spacing = 6 };
            names.Children.Add(HistoryText("Pembaca PDF", 30, true));
            names.Children.Add(HistoryText("Buka dan baca dokumen PDF langsung di OTO.", 16, muted: true)); heading.Children.Add(names);
            var offline = new Border { Child = HistoryText("●  Offline", 14), Padding = new Thickness(14, 7, 14, 7), CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1), BorderBrush = Brush(73, 168, 108), Background = Brush(241, 250, 245), VerticalAlignment = VerticalAlignment.Top };
            Grid.SetColumn(offline, 1); heading.Children.Add(offline); ResponsiveLayout.Header(heading, names, offline, 500); content.Children.Add(heading);
            var defaultBody = new StackPanel { Spacing = 10 };
            var defaultTop = new Grid { ColumnSpacing = 18 }; defaultTop.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); defaultTop.ColumnDefinitions.Add(new());
            defaultTop.Children.Add(new Border { Child = HistoryIcon("file-text", 34), Padding = new Thickness(14), CornerRadius = new CornerRadius(14), Background = Brush(231, 241, 255) });
            var defaultNames = new StackPanel { Spacing = 5 }; defaultNames.Children.Add(defaultHeading); defaultNames.Children.Add(defaultSubtitle); Grid.SetColumn(defaultNames, 1); defaultTop.Children.Add(defaultNames); defaultBody.Children.Add(defaultTop);
            landingDefaultButton = HistoryButton("external-link", "Atur sebagai default", true);
            landingDefaultButton.BorderBrush = Brush(8, 102, 232); landingDefaultButton.BorderThickness = new Thickness(1); landingDefaultButton.Foreground = Brush(8, 102, 232);
            landingDefaultButton.HorizontalAlignment = HorizontalAlignment.Right;
            landingDefaultButton.Click += async (_, _) => { await owner.ConfigureDefaultPdfAsync(); RefreshLandingDefault(); };
            var defaultActions = new StackPanel { Spacing = 6 }; defaultActions.Children.Add(landingDefaultButton); defaultActions.Children.Add(landingDefaultStatus);
            var defaultLayout = new Grid { ColumnSpacing = 18 }; defaultLayout.ColumnDefinitions.Add(new()); defaultLayout.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            defaultBody.Children.Remove(defaultTop); defaultLayout.Children.Add(defaultTop); Grid.SetColumn(defaultActions, 1); defaultLayout.Children.Add(defaultActions); defaultBody.Children.Add(defaultLayout);
            defaultLayout.RowDefinitions.Add(new() { Height = GridLength.Auto });
            defaultLayout.RowDefinitions.Add(new() { Height = GridLength.Auto });
            bool? defaultCompact = null;
            defaultLayout.SizeChanged += (_, _) =>
            {
                var compact = defaultLayout.ActualWidth < 850;
                if (compact == defaultCompact) return;
                defaultCompact = compact;
                Grid.SetColumn(defaultActions, compact ? 0 : 1); Grid.SetRow(defaultActions, compact ? 1 : 0);
                defaultActions.Margin = new Thickness(0, compact ? 12 : 0, 0, 0); defaultActions.MaxWidth = compact ? double.PositiveInfinity : 300;
                landingDefaultButton.HorizontalAlignment = compact ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            };
            AutomationProperties.SetLiveSetting(landingDefaultStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
            content.Children.Add(LandingCard(defaultBody, 18));
            var primary = new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Center };
            primary.Children.Add(new Border { Child = HistoryIcon("file-text", 48), Padding = new Thickness(16), CornerRadius = new CornerRadius(16), Background = Brush(231, 241, 255) });
            primary.Children.Add(HistoryText("Buka PDF untuk mulai membaca", 22, true));
            primary.Children.Add(HistoryText("Seret file PDF ke sini atau pilih dari perangkat Anda.", 14, muted: true));
            landingOpen = HistoryButton("folder", "Buka PDF", true, true); landingOpen.MinWidth = 170; landingOpen.MinHeight = 46;
            landingOpen.Click += async (_, _) => await PickAsync(); primary.Children.Add(landingOpen);
            primary.Children.Add(HistoryText("Ctrl + O   Buka file     |     Ctrl + V   Tempel file", 13, muted: true));
            primary.Children.Add(HistoryText("Dokumen tetap di perangkat Anda dan tidak diunggah.", 12, muted: true));
            foreach (var child in primary.Children.OfType<FrameworkElement>())
            {
                child.HorizontalAlignment = HorizontalAlignment.Center;
                if (child is TextBlock text) text.TextAlignment = TextAlignment.Center;
            }
            content.Children.Add(LandingCard(primary, 26));
            content.Children.Add(landingSessions);
            var recentHeader = new Grid(); recentHeader.ColumnDefinitions.Add(new()); recentHeader.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            recentHeader.Children.Add(HistoryText("Terakhir dibuka", 22, true));
            var history = HistoryButton("arrow-right", "Lihat riwayat", true); history.Click += (_, _) => owner.ShowHistory(); Grid.SetColumn(history, 1); recentHeader.Children.Add(history); ResponsiveLayout.Header(recentHeader, (FrameworkElement)recentHeader.Children[0], history, 500);
            content.Children.Add(recentHeader); content.Children.Add(LandingCard(recentLanding, 0));
            landing = new ScrollViewer { Content = content, Background = Brush(244, 248, 253), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            content.HorizontalAlignment = HorizontalAlignment.Center;
            landing.SizeChanged += (_, _) => content.Width = Math.Max(0, Math.Min(1240, landing.ActualWidth - 64));
            Grid.SetRowSpan(landing, 6); View.Children.Add(landing);
            owner.Activated += (_, e) => { if (e.WindowActivationState != WindowActivationState.Deactivated && landing.IsLoaded && !owner.configuringPdfDefault) RefreshLandingDefault(); };
            landing.Loaded += (_, _) => { RefreshLandingDefault(); RefreshLandingRecent(); };
        }

        private static Border LandingCard(UIElement child, double padding) => new() { Child = child, Padding = new Thickness(padding), CornerRadius = new CornerRadius(10), Background = Brush(255, 255, 255), BorderBrush = Brush(188, 210, 242), BorderThickness = new Thickness(1) };

        private void UpdateLanding()
        {
            if (landing is null) return;
            var emptyDocument = document is null;
            landing.Visibility = emptyDocument ? Visibility.Visible : Visibility.Collapsed;
            foreach (var element in readerChrome) element.Visibility = emptyDocument ? Visibility.Collapsed : Visibility.Visible;
            if (emptyDocument)
            {
                sessionPicker.Visibility = Visibility.Collapsed;
                RefreshLandingRecent();
            }
            landingOpen!.IsEnabled = !busy;
        }

        private void RefreshLandingDefault()
        {
            var request = ++defaultStatusGeneration;
#if DEBUG
            if (landingDefaultTestStatus is { } testStatus) { ApplyLandingDefault(testStatus); return; }
#endif
            _ = RefreshLandingDefaultAsync(request);
        }

        private async Task RefreshLandingDefaultAsync(int request)
        {
            // Shell association lookup may pump COM messages. Never perform it
            // inside a XAML Loaded/Activated/layout callback on the UI thread.
            var status = await Task.Run(PdfAssociation.QueryStatus);
            if (request != defaultStatusGeneration || landing?.IsLoaded != true) return;
            ApplyLandingDefault(status);
        }

        private void ApplyLandingDefault(PdfAssociation.DefaultStatus status)
        {
            var verified = status == PdfAssociation.DefaultStatus.Oto;
            defaultHeading.Text = verified ? "OTO adalah pembuka PDF default Anda." : "Jadikan OTO pembuka PDF utama";
            defaultSubtitle.Visibility = verified ? Visibility.Collapsed : Visibility.Visible;
            landingDefaultButton!.Visibility = verified ? Visibility.Collapsed : Visibility.Visible;
            landingDefaultButton.IsEnabled = !owner.configuringPdfDefault;
            landingDefaultStatus.Text = verified ? "" : owner.pdfDefaultActionMessage ?? (PortableProfile.IsPortable ? "Mode portable: pilih aplikasi terpasang melalui Pengaturan Windows." : status == PdfAssociation.DefaultStatus.Unknown ? "Status default belum dapat diverifikasi. Membuka Pengaturan Windows." : "Membuka Pengaturan Windows");
        }

        private void RememberReaderOpen(string input, uint pages)
        {
            owner.savedJobs.RemoveAll(j => j.Tool == "reader" && j.Paths.Length == 1 && j.Paths[0].Equals(input, StringComparison.OrdinalIgnoreCase));
            owner.savedJobs.Insert(0, new(Guid.NewGuid(), DateTimeOffset.Now, "reader", [input], $"{pages} halaman"));
            if (owner.savedJobs.Count > 200) owner.savedJobs.RemoveRange(200, owner.savedJobs.Count - 200);
            owner.SaveLibrary();
        }

        private void RefreshLandingRecent()
        {
            landingSessions.Children.Clear();
            var sessions = owner.homeFiles.Concat(owner.chosenFiles).Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToArray();
            if (sessions.Length > 0)
            {
                landingSessions.Children.Add(HistoryText("Dokumen sesi · pilih untuk membaca", 14, true));
                foreach (var input in sessions)
                {
                    var session = HistoryButton("file-text", Path.GetFileName(input), true);
                    session.Click += async (_, _) => await OpenAsync(input); landingSessions.Children.Add(session);
                }
            }
            recentLanding.Children.Clear();
            var entries = owner.savedJobs.OrderByDescending(j => j.When).SelectMany(j => j.Paths.Select(p => (Job: j, Path: p))).Where(e => Path.GetExtension(e.Path).Equals(".pdf", StringComparison.OrdinalIgnoreCase)).DistinctBy(e => e.Path, StringComparer.OrdinalIgnoreCase).Take(3).ToArray();
            if (entries.Length == 0) { recentLanding.Children.Add(new TextBlock { Text = "Belum ada PDF dalam riwayat lokal.", Margin = new Thickness(18), Foreground = Brush(77, 99, 128) }); return; }
            foreach (var entry in entries)
            {
                var image = new Image { Width = 38, Height = 48, Stretch = Stretch.Uniform };
                var row = new Grid { ColumnSpacing = 16 }; row.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new());
                row.Children.Add(image); var labels = new StackPanel { Spacing = 4 }; labels.Children.Add(HistoryText(Path.GetFileName(entry.Path), 15, true));
                var detail = HistoryText(entry.Job.When.ToLocalTime().ToString("dd MMM yyyy, HH:mm", System.Globalization.CultureInfo.CurrentCulture), 12, muted: true); labels.Children.Add(detail); Grid.SetColumn(labels, 1); row.Children.Add(labels);
                var button = new Button { Content = row, Padding = new Thickness(18, 10, 18, 10), HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch, Background = Brush(255, 255, 255), BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = Brush(228, 234, 243) };
                AutomationProperties.SetName(button, "Buka kembali " + Path.GetFileName(entry.Path));
                button.Click += async (_, _) => { if (!File.Exists(entry.Path)) { notice.Text = "File sudah dipindah atau tidak ditemukan. Pilih PDF dari lokasi barunya."; return; } await OpenAsync(entry.Path); };
                image.Loaded += async (_, _) =>
                {
                    if (!File.Exists(entry.Path)) { detail.Text += " · File tidak ditemukan"; return; }
                    try { var size = HistoryFileSize(new FileInfo(entry.Path).Length); var info = HistoryText(""); var pages = await RenderPreviewAsync(entry.Path, 0, image, info, 100); detail.Text = (pages > 0 ? $"{pages} halaman · " : "") + size + " · " + entry.Job.When.ToLocalTime().ToString("dd MMM yyyy, HH:mm", System.Globalization.CultureInfo.CurrentCulture); }
                    catch (Exception error) { LocalDiagnostics.Record("reader-recent", error); detail.Text = "Metadata belum dapat dibaca"; }
                };
                recentLanding.Children.Add(button);
            }
        }
    }
}
