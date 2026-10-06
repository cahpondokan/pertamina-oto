using System.Globalization;

namespace Pertamina.Oto.Desktop.App;

internal enum SplitMode { Custom, Fixed, AllPages, SelectedPages }
internal sealed record SplitRange(int First, int Last);

internal static class SplitPlan
{
    internal static int[] ParseSelection(string text, int count)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Pilih minimal satu halaman.");
        var result = new SortedSet<int>();
        foreach (var token in text.Replace('–', '-').Split(','))
        {
            var bounds = token.Trim().Split('-');
            if (bounds.Length is < 1 or > 2 || !int.TryParse(bounds[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var first))
                throw new ArgumentException("Gunakan format seperti 1, 3, 5–7.");
            var last = first;
            if (bounds.Length == 2 && !int.TryParse(bounds[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out last))
                throw new ArgumentException("Gunakan format seperti 1, 3, 5–7.");
            ValidateRange(first, last, count);
            for (var page = first; page <= last; page++) result.Add(page);
        }
        return result.ToArray();
    }

    private static void ValidateRange(int first, int last, int count)
    {
        if (count is < 1 or > 1000) throw new ArgumentException("PDF harus berisi 1–1000 halaman.");
        if (first < 1 || last > count) throw new ArgumentException($"Nomor halaman harus antara 1 dan {count}.");
        if (last < first) throw new ArgumentException("Halaman akhir harus sama atau lebih besar dari halaman awal.");
    }

    internal static int[][] Create(int count, SplitMode mode, IReadOnlyList<SplitRange> ranges, int step, string selection, bool combine)
    {
        ValidateRange(1, count, count);
        var groups = new List<int[]>();
        if (mode == SplitMode.Custom)
        {
            if (ranges.Count == 0) throw new ArgumentException("Tambahkan minimal satu rentang.");
            foreach (var range in ranges)
            {
                ValidateRange(range.First, range.Last, count);
                groups.Add(Enumerable.Range(range.First, range.Last - range.First + 1).ToArray());
            }
        }
        else if (mode == SplitMode.Fixed)
        {
            if (step < 1 || step > count) throw new ArgumentException($"Jumlah halaman per PDF harus antara 1 dan {count}.");
            for (var first = 1; first <= count; first += step) groups.Add(Enumerable.Range(first, Math.Min(step, count - first + 1)).ToArray());
        }
        else
        {
            var selected = mode == SplitMode.AllPages ? Enumerable.Range(1, count).ToArray() : ParseSelection(selection, count);
            groups.AddRange(selected.Select(page => new[] { page }));
        }
        if (groups.Sum(group => group.Length) > 1000) throw new ArgumentException("Total halaman hasil maksimal 1000. Kurangi rentang yang berulang.");
        return combine && mode is SplitMode.Custom or SplitMode.SelectedPages ? [groups.SelectMany(group => group).ToArray()] : groups.ToArray();
    }

    internal static string Describe(int[] pages)
    {
        var pieces = new List<string>();
        for (var i = 0; i < pages.Length; i++)
        {
            var first = pages[i]; var last = first;
            while (i + 1 < pages.Length && pages[i + 1] == last + 1) last = pages[++i];
            pieces.Add(first == last ? first.ToString(CultureInfo.InvariantCulture) : $"{first}–{last}");
        }
        return string.Join(", ", pieces);
    }
}
