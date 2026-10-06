using System.Globalization;

namespace Pertamina.Oto.PdfOpener;

internal static class PdfOpenFailure
{
    internal static string Message(string stage, Exception error)
    {
        var advice = stage switch
        {
            "input" => "Pilih satu file PDF dengan lokasi lengkap di perangkat. Lokasi jaringan, URL, dan argumen tambahan belum didukung.",
            "file" => "File PDF tidak ditemukan atau lokasinya tidak dapat diakses. Periksa apakah file sudah dipindahkan, dihapus, atau belum tersedia di perangkat.",
            "installation" => "Instalasi PERTAMINA OTO tidak dapat dibaca atau aplikasi yang ditunjuk tidak tersedia. Coba buka shortcut desktop OTO untuk memeriksa instalasi.",
            _ => "Windows belum dapat menjalankan PERTAMINA OTO. Coba buka shortcut desktop OTO; simpan kode berikut untuk diagnosis."
        };
        return advice + "\n\nKode: PDF-" + stage.ToUpperInvariant() + " / " + error.HResult.ToString("X8", CultureInfo.InvariantCulture);
    }

    internal static void Record(string stage, Exception error)
    {
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PertaminaOTO", "Logs");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "pdf-opener.log");
            if (File.Exists(path) && new FileInfo(path).Length > 128 * 1024) return;
            // Do not record arguments, paths, exception messages, or document contents.
            File.AppendAllText(path, string.Join(" | ", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), stage,
                typeof(PdfOpenFailure).Assembly.ManifestModule.ModuleVersionId, error.GetType().Name,
                error.HResult.ToString("X8", CultureInfo.InvariantCulture)) + Environment.NewLine);
        }
        catch (Exception) { /* A logging failure must not hide the original failure. */ }
    }
}
