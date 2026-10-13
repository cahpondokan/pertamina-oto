using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private TextBlock? pdfDefaultStatus;
    private bool configuringPdfDefault;
    private bool pdfDefaultRefreshAttached;
    private string? pdfDefaultActionMessage;

    private void AddPdfDefaultSetting(Action<string, string, string, FrameworkElement> row, StackPanel content)
    {
        if (PortableProfile.IsPortable)
        {
            row("\uE8A5", "Pembuka PDF default", "Mode portable tidak mendaftarkan asosiasi file Windows.", Copy("Tidak tersedia", 13));
            return;
        }
        row("\uE8A5", "Pembuka PDF default", "Buka file .pdf dari File Explorer langsung di Pembaca PDF OTO. Pilihan default dikonfirmasi melalui Windows.",
            ActionButton("Jadikan default", async () => await ConfigureDefaultPdfAsync(), "Jadikan PERTAMINA OTO pembuka PDF default"));
        pdfDefaultStatus = Copy(PdfAssociation.Status(), 12);
        AutomationProperties.SetLiveSetting(pdfDefaultStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        content.Children.Add(pdfDefaultStatus);
        if (!pdfDefaultRefreshAttached)
        {
            Activated += (_, e) =>
            {
                if (e.WindowActivationState != WindowActivationState.Deactivated && !configuringPdfDefault && pdfDefaultStatus?.IsLoaded == true)
                    pdfDefaultStatus.Text = PdfAssociation.Status();
            };
            pdfDefaultRefreshAttached = true;
        }
    }

    private async Task ConfigureDefaultPdfAsync()
    {
        if (configuringPdfDefault || messageOpen) return;
        configuringPdfDefault = true;
        var registered = false;
        try
        {
            if (pdfDefaultStatus is not null) pdfDefaultStatus.Text = "Mendaftarkan pembuka PDF untuk akun Windows ini…";
            if (!PortableProfile.IsPortable)
            {
                try { await Task.Run(PdfAssociation.Register); registered = true; }
                catch (Exception error) { LocalDiagnostics.Record("pdf-default-registration", error); }
            }
            var opened = registered && await Launcher.LaunchUriAsync(PdfAssociation.SettingsUri);
            if (!opened) opened = await Launcher.LaunchUriAsync(new Uri("ms-settings:defaultapps"));
            pdfDefaultActionMessage = opened
                ? registered ? "Pilih OTO untuk .pdf di Pengaturan Windows. Pilihan default belum dikonfirmasi." : "Pengaturan Windows dibuka. Build ini belum terdaftar; pilih OTO terpasang jika tersedia."
                : "Pengaturan Windows belum dapat dibuka. Buka Aplikasi → Aplikasi default → .pdf secara manual.";
            if (pdfDefaultStatus is not null)
                pdfDefaultStatus.Text = opened
                    ? "Pada Windows, pilih .pdf → PERTAMINA OTO → Tetapkan default / Selalu. Jika halaman umum terbuka, cari PERTAMINA OTO atau .pdf."
                    : "OTO sudah terdaftar. Buka Pengaturan Windows → Aplikasi → Aplikasi default → .pdf → PERTAMINA OTO. Atau klik kanan PDF → Buka dengan → PERTAMINA OTO → Selalu.";
        }
        catch (Exception error)
        {
            pdfDefaultActionMessage = "Pengaturan atau pendaftaran belum selesai. Pilihan default Windows tetap tidak diubah otomatis.";
            LocalDiagnostics.Record("pdf-default-registration", error);
            if (pdfDefaultStatus is not null) pdfDefaultStatus.Text = registered
                ? "OTO sudah terdaftar, tetapi Pengaturan Windows belum terbuka. Buka Pengaturan Windows → Aplikasi → Aplikasi default → .pdf → PERTAMINA OTO."
                : "Pendaftaran belum selesai. Pastikan update terpasang dan buka aplikasi dari shortcut desktop, lalu coba lagi. Pilihan default Windows tetap tidak diubah otomatis.";
        }
        finally { configuringPdfDefault = false; }
    }
}
