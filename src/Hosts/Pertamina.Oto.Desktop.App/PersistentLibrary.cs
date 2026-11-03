using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed record SavedJob(Guid Id, DateTimeOffset When, string Tool, string[] Paths, string Detail);
internal sealed record SavedLibrary(int Version, string[] Favorites, SavedJob[] Jobs);

internal sealed partial class MainWindow
{
    private readonly List<SavedJob> savedJobs = [];
    private string libraryNotice = "";
    private bool libraryUnreadable;
    private static readonly string[] AllToolsFilter = ["Semua alat"];
    private static string LibraryPath => libraryTestPath ?? Path.Combine(PortableProfile.DataRoot, "library.json");
    private static string? libraryTestPath;

    internal void TestLibraryStorage()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "library-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory); libraryTestPath = Path.Combine(directory, "library.json");
        var report = new List<string>();
        void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); report.Add("PASS " + name); }
        try
        {
            favorites.Clear(); savedJobs.Clear(); libraryUnreadable = false;
            favorites.Add("merge");
            savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now, "merge", [Path.Combine(directory, "missing.pdf")], "2 halaman"));
            SaveLibrary();
            favorites.Clear(); savedJobs.Clear(); LoadLibrary();
            Check(favorites.Contains("merge") && savedJobs.Count == 1, "favorites and history reload from disk");
            Check(!File.Exists(savedJobs[0].Paths[0]), "missing file record retained");
            favorites.Remove("merge"); savedJobs.Clear(); SaveLibrary();
            favorites.Clear(); LoadLibrary();
            Check(favorites.Count == 0 && savedJobs.Count == 0, "removals survive reload");
            File.WriteAllText(LibraryPath, "invalid json"); LoadLibrary(); favorites.Add("merge"); SaveLibrary();
            Check(libraryUnreadable && File.ReadAllText(LibraryPath) == "invalid json", "corrupt store preserved without overwrite");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name); Environment.ExitCode = 1; }
        finally { libraryTestPath = null; File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "library-selftest.txt"), report); }
    }

    private void LoadLibrary()
    {
        try
        {
            if (!File.Exists(LibraryPath)) return;
            if (new FileInfo(LibraryPath).Length > 8 * 1024 * 1024) throw new IOException();
            var data = JsonSerializer.Deserialize<SavedLibrary>(File.ReadAllText(LibraryPath));
            if (data is null || data.Version != 1 || data.Favorites is null || data.Jobs is null) throw new IOException();
            favorites.UnionWith(data.Favorites.Where(id => ToolCatalog.All.Any(t => t.Id == id)));
            savedJobs.AddRange(data.Jobs.Where(j => j is not null && j.Paths is { Length: > 0 and <= 1000 } && j.Paths.All(p => !string.IsNullOrEmpty(p) && Path.IsPathFullyQualified(p) && !p.StartsWith(@"\\", StringComparison.Ordinal))).Take(200));
        }
        catch (Exception error) { libraryUnreadable = true; libraryNotice = "Data lokal tidak dapat dibaca. Data lama dipertahankan; perubahan sesi ini belum dapat disimpan."; LocalDiagnostics.Record("library-load", error); }
    }

    private void SaveLibrary()
    {
        if (libraryUnreadable) return;
        string? temp = null;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath)!);
            temp = LibraryPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new SavedLibrary(1, favorites.ToArray(), savedJobs.Take(200).ToArray())));
            File.Move(temp, LibraryPath, true); temp = null; libraryNotice = "";
            // Windows share one UI thread. Broadcast the committed snapshot so a
            // later save from another window cannot restore stale history/favorites.
            foreach (var window in readerWindows.Where(w => w != this))
            {
                window.savedJobs.Clear(); window.savedJobs.AddRange(savedJobs);
                window.favorites.Clear(); window.favorites.UnionWith(favorites);
                window.libraryUnreadable = false; window.libraryNotice = "";
            }
        }
        catch (Exception error) { libraryNotice = "Perubahan belum tersimpan di perangkat. Periksa ruang dan izin penyimpanan."; LocalDiagnostics.Record("library-save", error); }
        finally { if (temp is not null) { try { File.Delete(temp); } catch (Exception) { } } }
    }

    private void RememberResult(string[] paths, string detail)
    {
        savedJobs.Insert(0, new(Guid.NewGuid(), DateTimeOffset.Now, selected?.Id ?? "", paths.ToArray(), detail));
        if (savedJobs.Count > 200) savedJobs.RemoveRange(200, savedJobs.Count - 200);
        SaveLibrary();
    }

    private void BuildPersistentHistory() => BuildHistoryGallery();
}
