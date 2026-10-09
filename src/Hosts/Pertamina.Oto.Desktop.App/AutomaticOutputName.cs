using System.Globalization;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly Dictionary<string, string> automaticOutputNames = [];

    private void UpdateAutomaticOutputName()
    {
        if (selected is null) return;
        var suffix = selected.Id switch { "word-pdf" => "word", "merge" => "gabungan", "remove" => "hapus-halaman", "extract" => "ambil-halaman", "organize" => "susun-ulang", _ => null };
        if (suffix is null) return;
        var current = outputName.Text.Trim();
        if (current.Length > 0 && current != "hasil-" + selected.Id + ".pdf"
            && current != automaticOutputNames.GetValueOrDefault(selected.Id)) return;
        var source = selected.Id == "organize"
            ? pageLayout.Where(p => p.Source >= 0).Select(p => organizerSources[p.Source]).FirstOrDefault()
            : files.FirstOrDefault();
        if (source is null) return;
        var stem = Path.GetFileNameWithoutExtension(source);
        if (stem.Length > 100) stem = stem[..100];
        var name = stem + "_" + suffix + ".pdf";
        automaticOutputNames[selected.Id] = name;
        outputName.Text = name;
    }

    private static string AvailableOutputPath(string directory, string name)
    {
        var stem = Path.GetFileNameWithoutExtension(name);
        var extension = Path.GetExtension(name);
        var candidate = Path.Combine(directory, name);
        for (var number = 2; File.Exists(candidate) || Directory.Exists(candidate); number++)
        {
            candidate = Path.Combine(directory, stem + " (" + number.ToString(CultureInfo.InvariantCulture) + ")" + extension);
        }
        // Publication still uses the worker's no-overwrite check to prevent a race.
        return candidate;
    }
}

