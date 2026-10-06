using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Data.Pdf;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
#if DEBUG
    private Func<PdfReaderView, Task>? auditReadDialog;
#endif
    private async Task ShowReadPreviewDialogAsync(string input, PdfDocument? loaded = null, int page = 0, string? explanation = null)
    {
        if (messageOpen || active is not null || importing) return;
        var resumeEditing = liveEditor?.PauseForReadPreview();
        messageOpen = true;
        using var reader = new PdfReaderView(this, previewOnly: true);
        ContentDialog? dialog = null;
        var secret = new PasswordBox { Header = "Kata sandi PDF", MaxLength = 127 };
        reader.View.Height = Math.Clamp(root.ActualHeight - 260, 100, 600);
        async Task LoadPreview()
        {
            try
            {
                await reader.WaitLoadedAsync(); reader.View.UpdateLayout();
                if (loaded is null) await reader.OpenAsync(input);
                else await reader.AttachAsync(loaded, input, new FileInfo(input).Length, page);
#if DEBUG
                if (auditReadDialog is not null)
                {
                    try { await auditReadDialog(reader); }
                    finally { dialog?.Hide(); }
                }
#endif
            }
            catch (Exception error) { LocalDiagnostics.Record("read-preview", error); }
        }
        try
        {
            var content = new StackPanel { Spacing = 8 };
            if (explanation is not null) content.Children.Add(HistoryText(explanation, 12, muted: true));
            content.Children.Add(reader.View);
            if (loaded is null)
            {
                var access = new StackPanel { Spacing = 8 }; access.Children.Add(secret);
                var reload = HistoryButton("scan-eye", "Buka dengan kata sandi", true);
                reload.Click += async (_, _) =>
                {
                    var password = secret.Password; secret.Password = "";
                    await reader.OpenAsync(input, password);
                };
                access.Children.Add(reload);
                content.Children.Add(new Expander { Header = "PDF terkunci?", Content = access, HorizontalAlignment = HorizontalAlignment.Stretch });
            }
            dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = Path.GetFileName(input), Content = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled },
                CloseButtonText = "Kembali", DefaultButton = ContentDialogButton.Close };
            ResponsiveLayout.BoundDialog(dialog, (FrameworkElement)dialog.Content, root);
            dialog.Opened += async (_, _) => await LoadPreview();
            await dialog.ShowAsync();
        }
        catch (Exception error) { LocalDiagnostics.Record("read-preview-dialog", error); }
        finally { secret.Password = ""; messageOpen = false; if (resumeEditing is not null) await resumeEditing(); }
    }
}
