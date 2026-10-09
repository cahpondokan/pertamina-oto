using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private PdfReaderView? historyReadingView;
    private int historyReaderRequest;
    private Task? historyReaderRender;
    private ScrollViewer? historyReaderViewport;
    private Image? historyReaderImage;
    private Func<int, Task>? historyGoToPage;

    private async Task OpenInternalPdfAsync(string path)
    {
        if (active is not null || importing || messageOpen) return;
        var navigationVersion = historyNavigationGeneration;
        var request = ++historyReaderRequest;
        try
        {
            if (path.StartsWith(@"\\", StringComparison.Ordinal) || !Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            { SetHistoryNotice("Preview internal menerima PDF lokal. Gunakan Buka folder untuk format gambar atau format lain."); return; }
            var file = await StorageFile.GetFileFromPathAsync(path);
            var size = (await file.GetBasicPropertiesAsync()).Size;
            if (request != historyReaderRequest || navigationVersion != historyNavigationGeneration) return;
            if (size > 100 * 1024 * 1024)
            { SetHistoryNotice("PDF melebihi batas pembaca saat ini: 100 MB. File asli tidak diubah."); return; }
            PdfDocument pdf;
            try { pdf = await PdfDocument.LoadFromFileAsync(file); }
            catch (Exception error) when (error.HResult == unchecked((int)0x8007052B))
            {
                if (navigationVersion != historyNavigationGeneration || request != historyReaderRequest) return;
                var secret = new PasswordBox { Header = "Kata sandi PDF", MaxLength = 127 };
                messageOpen = true;
                try
                {
                    var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "PDF dilindungi kata sandi", Content = secret,
                        PrimaryButtonText = "Buka", CloseButtonText = "Batal" };
                    if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
                    pdf = await PdfDocument.LoadFromFileAsync(file, secret.Password);
                }
                finally { secret.Password = ""; messageOpen = false; }
            }
            if (pdf.PageCount is 0 or > 1000) throw new IOException();
            if (navigationVersion != historyNavigationGeneration || request != historyReaderRequest || historyPage.Visibility != Visibility.Visible) return;
            ShowInternalPdf(pdf, path, (long)size);
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("internal-pdf-open", error);
            if (navigationVersion == historyNavigationGeneration && request == historyReaderRequest)
                SetHistoryNotice("PDF tidak dapat dibuka. File mungkin dipindahkan, rusak, kata sandi tidak sesuai, atau melebihi 1000 halaman. File asli tetap aman.");
        }
    }

    private void ShowInternalPdf(PdfDocument pdf, string path, long size)
    {
        if (historyPreview is null || historyPreviewFrame is null) return;
        disposeHistoryReader?.Invoke();
        var target = historyPreview;
        target.Children.Clear(); target.RowDefinitions.Clear();
        target.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        target.RowDefinitions.Add(new RowDefinition());
        target.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        historySelectedPath = path;
        historyPreviewFrame.Visibility = Visibility.Visible;
        resizeHistory?.Invoke(); UpdateHistorySelection(); SetHistoryNotice("");

        var heading = new Grid { Padding = new Thickness(14, 10, 14, 10), ColumnSpacing = 10, MinHeight = 60,
            Background = Brush(255, 255, 255), BorderBrush = Brush(221, 228, 236), BorderThickness = new Thickness(0, 0, 0, 1) };
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new ColumnDefinition());
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var fileIcon = HistoryIcon("file-text", 22); heading.Children.Add(fileIcon);
        var filename = HistoryText(Path.GetFileName(path), 13, true); filename.MaxLines = 1; filename.TextTrimming = TextTrimming.CharacterEllipsis;
        ToolTipService.SetToolTip(filename, Path.GetFileName(path));
        var titles = new StackPanel { Spacing = 3, VerticalAlignment = VerticalAlignment.Center };
        titles.Children.Add(filename); titles.Children.Add(HistoryText($"{pdf.PageCount} halaman · {HistoryFileSize(size)}", 11, muted: true));
        Grid.SetColumn(titles, 1); heading.Children.Add(titles);
        var actions = new ResponsiveWrapPanel { Spacing = 2 };
        var toolsButton = HistoryButton("wrench", "Gunakan alat", true);
        toolsButton.Click += (_, _) =>
        {
            if (active is not null || importing || messageOpen) return;
            if (!homeFiles.Contains(path)) homeFiles.Add(path);
            chosenFiles.Clear(); chosenFiles.Add(path); ShowCatalog();
        };
        var folderButton = HistoryButton("folder", "Buka folder hasil");
        folderButton.Click += async (_, _) => await OpenHistoryFolderAsync(path);
        var expand = HistoryButton("maximize2", "Perluas preview");
        expand.Click += (_, _) =>
        {
            historyExpanded = !historyExpanded; resizeHistory?.Invoke();
            ((StackPanel)expand.Content).Children.Clear();
            ((StackPanel)expand.Content).Children.Add(HistoryIcon(historyExpanded ? "minimize2" : "maximize2"));
            AutomationProperties.SetName(expand, historyExpanded ? "Tampilkan daftar dokumen" : "Perluas preview");
        };
        var close = HistoryButton("x", "Tutup preview"); close.Click += (_, _) => CloseHistoryPreview();
        var reader = HistoryButton("file-text", "Buka di Pembaca PDF");
        reader.Click += async (_, _) => await OpenPdfReaderAsync(path);
        actions.Children.Add(toolsButton); actions.Children.Add(new Border { Width = 1, Height = 20, Background = Brush(220, 227, 235), Margin = new Thickness(4, 0, 4, 0) });
        actions.Children.Add(reader); actions.Children.Add(folderButton); actions.Children.Add(expand); actions.Children.Add(close);
        Grid.SetColumn(actions, 2); heading.Children.Add(actions); ResponsiveLayout.Header(heading, titles, actions, 440); target.Children.Add(heading);
        heading.SizeChanged += (_, _) =>
        {
            var label = ((StackPanel)toolsButton.Content).Children.OfType<TextBlock>().First();
            label.Visibility = heading.ActualWidth < 620 ? Visibility.Collapsed : Visibility.Visible;
            fileIcon.Visibility = heading.ActualWidth < 400 ? Visibility.Collapsed : Visibility.Visible;
        };

        var sharedReader = new PdfReaderView(this, previewOnly: true);
        historyReadingView = sharedReader;
        historyReaderViewport = sharedReader.ReadingViewport;
        sharedReader.PageSelected = image => historyReaderImage = image;
        Grid.SetRow(sharedReader.View, 1); target.Children.Add(sharedReader.View);
        historyGoToPage = sharedReader.NavigateAsync;
        disposeHistoryReader = () =>
        {
            sharedReader.Dispose(); historyReadingView = null;
            historyGoToPage = null; historyReaderRender = null;
            historyReaderImage = null; historyReaderViewport = null;
        };
        async Task Load()
        {
            await sharedReader.WaitLoadedAsync();
            if (historyReadingView != sharedReader) return;
            await sharedReader.AttachAsync(pdf, path, size);
        }
        historyReaderRender = Load();
    }
}
