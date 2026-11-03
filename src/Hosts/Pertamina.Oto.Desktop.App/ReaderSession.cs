using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool readerFocus;
    private readonly HashSet<string> readerResults = new(StringComparer.OrdinalIgnoreCase);
    private sealed partial class LiveEditor
    {
        internal string? ReaderSource => path;
    }
    private sealed partial class PdfReaderView
    {
        internal string? ReaderSource => path;
        private readonly ComboBox sessionPicker = new() { Header = "Dokumen sesi · pilih untuk membaca", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(16, 4, 16, 8) };
        private bool refreshingSession;
        private bool sessionWired;
        private double savedHorizontal, savedVertical;
        private float savedZoom = 1;
        private bool suspended;
        private bool restoringPosition;
        private bool explicitlyClosed;
        private readonly StackPanel readerCloseNotice = new() { Spacing = 8 };

        internal void CloseReaderDocument()
        {
            if (!previewOnly && owner.FindReaderTab(this) is not null) { owner.CloseReaderTab(this); return; }
            ResetReaderDocument();
        }

        internal void ResetReaderDocument()
        {
            // A reader close ends only this read session, never a tool/editor draft.
            explicitlyClosed = true; Release(); savedHorizontal = savedVertical = 0; savedZoom = 1;
            title.Text = "Pembaca PDF"; metadata.Text = "Dokumen ditutup · pilih PDF untuk membaca"; count.Text = "/ 0";
            RefreshSession(false);
            var pending = owner.RestartPendingWork();
            notice.Text = pending.Count == 0 ? "Dokumen ditutup. File di komputer tetap tersedia." :
                "Pembaca ditutup. Draf alat/editor terpisah masih belum disimpan; tinjau pekerjaan tersebut sebelum mulai ulang.";
            readerCloseNotice.Children.Clear(); readerCloseNotice.Children.Add(HistoryText(notice.Text, 14, muted: true));
            if (pending.Count > 0)
            {
                var review = HistoryButton("file-text", "Tinjau pekerjaan belum disimpan", true);
                review.Click += (_, _) => { var latest = owner.RestartPendingWork(); if (latest.Count > 0) latest[0].Review(); };
                readerCloseNotice.Children.Add(review);
            }
            if (landing?.Content is StackPanel content && !content.Children.Contains(readerCloseNotice)) content.Children.Insert(1, readerCloseNotice);
        }

        internal void SetIdentity(string input, bool processed)
        {
            if (path is null || !Path.GetFullPath(input).Equals(path, StringComparison.OrdinalIgnoreCase)) return;
            if (processed) owner.readerResults.Add(path);
            var identity = owner.readerResults.Contains(path) ? "Hasil pengolahan" : "Sumber asli";
            metadata.Text = $"{document!.PageCount} halaman · {identity} · Offline";
            if (notice.Text.StartsWith("Pembaca menampilkan sumber asli.", StringComparison.Ordinal)) notice.Text = "";
            if (owner.liveEditor?.ReaderSource == path && !owner.readerResults.Contains(path))
                notice.Text = "Pembaca menampilkan sumber asli. Draft editor tetap tersimpan dalam sesi; simpan edit untuk membaca hasilnya.";
        }

        internal void RefreshSession(bool autoOpen = true)
        {
            if (previewOnly) return;
            var files = owner.homeFiles.Concat(owner.chosenFiles).Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (path is not null && !files.Contains(path, StringComparer.OrdinalIgnoreCase)) files.Insert(0, path);
            refreshingSession = true;
            sessionPicker.Items.Clear();
            foreach (var file in files)
            {
                var duplicate = files.Count(p => Path.GetFileName(p).Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase)) > 1;
                var item = new ComboBoxItem { Content = duplicate ? $"{Path.GetFileName(file)} · {Path.GetDirectoryName(file)}" : Path.GetFileName(file), Tag = file };
                ToolTipService.SetToolTip(item, file); sessionPicker.Items.Add(item);
            }
            sessionPicker.SelectedIndex = path is null ? -1 : files.FindIndex(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
            // The empty reader already lists sessions on its landing page. Keep
            // the document toolbar collapsed when refreshing a cached reader.
            sessionPicker.Visibility = Visibility.Collapsed; // Document tabs own switching; landing still offers session inputs.
            refreshingSession = false;
            if (!sessionWired)
            {
                sessionWired = true;
                sessionPicker.SelectionChanged += async (_, _) =>
                {
                    if (refreshingSession || sessionPicker.SelectedItem is not ComboBoxItem { Tag: string input }) return;
                    await owner.OpenPdfReaderAsync(input, owner.readerResults.Contains(input));
                };
            }
            // Existing manual reading always wins. Ambiguous sessions require a choice.
            var preferred = owner.chosenFiles.Where(p => files.Contains(p, StringComparer.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var candidate = preferred.Length == 1 ? preferred[0] : preferred.Length == 0 && files.Count == 1 ? files[0] : null;
            if (autoOpen && !explicitlyClosed && path is null && candidate is not null)
                owner.DispatcherQueue.TryEnqueue(async () =>
                {
                    await WaitLoadedAsync();
                    if (explicitlyClosed || path is not null || !Visible) return;
                    await owner.OpenPdfReaderAsync(candidate, owner.readerResults.Contains(candidate));
                });
        }
    }
}
