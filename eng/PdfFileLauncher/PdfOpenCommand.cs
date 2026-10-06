using System.Diagnostics;
using Pertamina.Oto.Desktop.App;

namespace Pertamina.Oto.PdfOpener;

internal static class PdfOpenCommand
{
    internal static ProcessStartInfo Create(string installRoot, string[] arguments, Action<string>? stage = null)
    {
        if (arguments.Length != 1) throw new ArgumentException("Pilih satu file PDF.");
        var input = arguments[0];
        if (!Path.IsPathFullyQualified(input) || input.StartsWith(@"\\", StringComparison.Ordinal)
            || input.Contains('"') || input.AsSpan(2).Contains(':') || !Path.GetExtension(input).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Pilih file PDF lokal.");
        var path = Path.GetFullPath(input);
        stage?.Invoke("file");
        if (!File.Exists(path)) throw new FileNotFoundException("PDF tidak ditemukan.");
        stage?.Invoke("installation");
        var version = LocalAppUpdate.VersionAt(installRoot); LocalAppUpdate.ValidateVersion(version);
        var executable = LocalAppUpdate.SafePath(installRoot, "versions/" + version + "/Pertamina.Oto.Desktop.App.exe");
        if (!File.Exists(executable)) throw new FileNotFoundException("Aplikasi belum tersedia.");
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(executable)! };
        start.ArgumentList.Add("--open-reader"); start.ArgumentList.Add(path);
        return start;
    }
}
