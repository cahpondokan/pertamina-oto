using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool pastingDocument;

    private static bool IsTextPasteTarget(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is TextBox or RichEditBox or PasswordBox or NumberBox) return true;
            element = VisualTreeHelper.GetParent(element);
        }
        return false;
    }

    private async void GlobalPasteKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Handled || e.Key != VirtualKey.V ||
            (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) == 0) return;
        try
        {
            var containsFiles = Clipboard.GetContent().Contains(StandardDataFormats.StorageItems);
            // File paste takes precedence even inside search/text fields. Ordinary text
            // remains native, preserving selection, undo and rich text clipboard handling.
            if (!containsFiles && IsTextPasteTarget(FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject)) return;
            e.Handled = true;
            await PasteDocumentAnywhereAsync();
        }
        catch (Exception error)
        {
            e.Handled = true; LocalDiagnostics.Record("global-paste-key", error);
            await MenuMessageAsync("Clipboard belum tersedia", "Coba salin file kembali dari File Explorer, atau gunakan Buka / pilih dokumen.");
        }
    }

    private async Task PasteDocumentAnywhereAsync()
    {
        if (messageOpen) return; // Keep the active modal dialog in control.
        if (pastingDocument || active is not null || importing || liveEditor?.Working == true)
        { await MenuMessageAsync("Pekerjaan sedang berjalan", "Tunggu hingga selesai atau batalkan proses sebelum menempel dokumen baru."); return; }
        pastingDocument = true;
        try
        {
            if (!Clipboard.GetContent().Contains(StandardDataFormats.StorageItems))
            { await MenuMessageAsync("Tidak ada file untuk ditempel", "Salin dokumen dari File Explorer, lalu tekan Ctrl+V. Teks dapat ditempel langsung di kotak ketik."); return; }
            if (liveEditor?.Visible == true) { await liveEditor.PasteAsync(); return; }
            if (pdfReader?.Visible == true) { await pdfReader.PasteAsync(); return; }
            if (latestResultCard?.IsLoaded == true)
            {
                messageOpen = true;
                try
                {
                    if (await new ContentDialog { XamlRoot = root.XamlRoot, Title = "Mulai dengan dokumen baru?", Content = "Hasil sebelumnya tetap tersimpan. Dokumen dari clipboard akan dibuka di Beranda untuk memilih alat.", PrimaryButtonText = "Lanjutkan", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close }.ShowAsync() != ContentDialogResult.Primary) return;
                }
                finally { messageOpen = false; }
                ShowHome(); await PasteHomeAsync(); return;
            }
            if (emptyPanel?.IsLoaded == true && selected is { Available: false })
            { await MenuMessageAsync("Alat belum tersedia", "Pilih alat yang sudah aktif untuk mengolah dokumen."); return; }
            if (emptyPanel?.IsLoaded == true) { await PasteEmptyToolAsync(); return; }
            if (organizerVisible) { await PasteOrganizerAsync(); return; }
            if (toolWorkspace?.IsLoaded == true && selected is { Available: true })
            {
                if (selected.Id == "merge") await PasteMergeAsync();
                else if (selected.Id == "remove") await PasteRemoveFileAsync();
                else if (selected.Id == "extract") await PasteExtractFileAsync();
                else await PasteSourceAsync();
                return;
            }
            // Non-editor pages and completed jobs receive new documents on Home.
            // Existing saved results are not deleted.
            if (!homeEntries.IsLoaded) ShowHome();
            await PasteHomeAsync();
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("paste-document", error);
            await MenuMessageAsync("Dokumen belum dapat ditempel", "Coba salin file kembali dari File Explorer atau gunakan Buka / pilih dokumen.");
        }
        finally { pastingDocument = false; }
    }
}
