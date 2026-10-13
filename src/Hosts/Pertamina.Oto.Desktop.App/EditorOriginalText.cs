using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private EditorTextRun[] originalRuns = [];
        private string originalInspectionMessage = "";
        private int originalId = -1;
        private string[] localTextFonts = [];
        private IRandomAccessStream? originalPreviewStream;
        private string? originalPreviewFile;

        private async Task LoadOriginalRunsAsync()
        {
            originalRuns = []; originalId = -1; originalInspectionMessage = "";
            if (path is null) return;
            var reply = await WorkerClient.RunAsync(new("edit-text-check", [path], System.IO.Path.Combine(System.IO.Path.GetTempPath(), "oto-inspect-" + Guid.NewGuid().ToString("N") + ".pdf"), Rotation: page), CancellationToken.None);
            originalRuns = reply.TextRuns ?? []; localTextFonts = reply.LocalFonts ?? []; notice.Text = reply.Message; originalInspectionMessage = reply.Message;
        }

        private void OriginalProperties()
        {
            var generation = propertyGeneration;
            properties.Children.Add(HistoryText("Edit teks asli", 20, true));
            properties.Children.Add(HistoryText("Double-click teks untuk mengetik langsung. Triple-click memilih seluruh bagian. Enter menerapkan; Esc membatalkan ketikan. Anda juga bisa memakai panel ini.", 13, muted: true));
            if (originalRuns.Length == 0)
            {
                properties.Children.Add(HistoryText(originalInspectionMessage.Length > 0 ? originalInspectionMessage : "Pemeriksaan teks belum tersedia.", 13, muted: true)); return;
            }
            var list = new ComboBox { Header = "Teks pada halaman", HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = originalRuns.Select(r => r.Text).ToArray(), SelectedIndex = Array.FindIndex(originalRuns, r => r.Id == originalId) };
            list.SelectionChanged += async (_, _) => { if (Working || updating || generation != propertyGeneration || list.SelectedIndex < 0) return; var target = originalRuns[list.SelectedIndex].Id; if (!await CommitOriginalInlineAsync()) return; originalId = target; BuildProperties(); Refresh(); };
            properties.Children.Add(list);
            var run = originalRuns.FirstOrDefault(r => r.Id == originalId); if (run is null) return;
            var current = marks.FirstOrDefault(m => m.Kind == "replace-text" && m.Page == page && m.SourceId == run.Id);
            var pending = originalInlineDraft?.Run.Id == run.Id ? originalInlineDraft : null;
            var fontChoices = localTextFonts.Prepend("Pertahankan font dalam PDF").ToArray();
            var fontChoice = new ComboBox { Header = "Font teks pengganti", HorizontalAlignment = HorizontalAlignment.Stretch, ItemsSource = fontChoices,
                SelectedIndex = Math.Max(0, Array.IndexOf(fontChoices, pending?.Font ?? current?.LocalFont ?? "")) };
            properties.Children.Add(fontChoice);
            properties.Children.Add(HistoryText(run.MatchingLocalFont.Length > 0
                ? $"Untuk mengetik huruf baru, pilih font lokal yang cocok: {run.MatchingLocalFont}."
                : "Font asli lengkap belum ditemukan di daftar lokal. Pilih font pengganti secara eksplisit jika diperlukan.", 12, muted: true));
            var text = new TextBox { Header = "Teks pengganti", Text = pending?.Text ?? current?.Text ?? run.Text, MaxLength = 200 };
            string SelectedFont() => fontChoice.SelectedIndex > 0 ? fontChoices[fontChoice.SelectedIndex] : "";
            var fitText = new CheckBox { Content = new TextBlock { Text = "Kecilkan font agar muat\n(minimal 6 pt)", TextWrapping = TextWrapping.Wrap }, IsChecked = pending?.Fit ?? current?.FitText ?? false, IsEnabled = fontChoice.SelectedIndex > 0 };
            void SyncDraft()
            {
                if (updating || generation != propertyGeneration || originalInlineDraft is not { } draft || draft.Run.Id != run.Id) return;
                originalInlineDraft = draft with { Text = text.Text, Font = SelectedFont(), Fit = fitText.IsEnabled && fitText.IsChecked == true };
                if (inlineOriginalRun is not null && inlineText is not null) { if (inlineText.Text != text.Text) inlineText.Text = text.Text; ApplyInlineOriginalFont(SelectedFont()); }
                Refresh();
            }
            fontChoice.SelectionChanged += (_, _) => { if (generation != propertyGeneration) return; fitText.IsEnabled = fontChoice.SelectedIndex > 0; SyncDraft(); };
            text.TextChanged += (_, _) => SyncDraft(); fitText.Checked += (_, _) => SyncDraft(); fitText.Unchecked += (_, _) => SyncDraft();
            text.KeyDown += async (_, e) => { if (e.Key == Windows.System.VirtualKey.Enter && !Working) { e.Handled = true; await ApplyOriginalFromInspectorAsync(run, text.Text, SelectedFont(), fitText.IsEnabled && fitText.IsChecked == true); } };
            properties.Children.Add(text);
            properties.Children.Add(fitText);
            properties.Children.Add(HistoryText($"Font asli: {run.Font.Split('+')[^1]}\nUkuran: {run.Size:0.#} pt", 12, muted: true));
            properties.Children.Add(HistoryText("Font PDF: karakter yang tersedia saja. Font lokal: huruf Latin dasar lengkap. Ukuran dan posisi tetap; pengganti harus muat. Kosongkan untuk menghapus teks, bukan redaksi data sensitif.", 12, muted: true));
            var apply = HistoryButton("editor-text", "Terapkan teks", true, true);
            apply.Click += async (_, _) => await ApplyOriginalFromInspectorAsync(run, text.Text, SelectedFont(), fitText.IsEnabled && fitText.IsChecked == true);
            properties.Children.Add(apply);
            var reset = HistoryButton("editor-undo", "Pulihkan teks sumber", true); reset.IsEnabled = current is not null;
            reset.Click += async (_, _) => { CancelOriginalInline(); await ApplyOriginalTextAsync(run, run.Text); };
            properties.Children.Add(reset);
        }

        private void OriginalOverlays()
        {
            if (mode != "Edit teks asli") return;
            foreach (var run in originalRuns)
            {
                if (inlineOriginalRun?.Id == run.Id) continue;
                var selected = originalId == run.Id;
                var border = new Border { Width = run.Width, Height = run.Height,
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(selected ? (byte)35 : (byte)8, 8, 102, 232)),
                    BorderBrush = Brush(8, 102, 232), BorderThickness = new Thickness(selected ? 2 : 1) };
                var target = new HandleBorder(Microsoft.UI.Input.InputSystemCursorShape.IBeam) { Width = run.Width, Height = run.Height, Content = border };
                ToolTipService.SetToolTip(target, "Double-click untuk mengedit teks langsung");
                target.PointerPressed += async (_, e) => { if (Working || !e.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed) return; e.Handled = true; await OriginalTextPressedAsync(run, e.GetCurrentPoint(sheet).Position, Environment.TickCount64); };
                Canvas.SetLeft(target, run.X); Canvas.SetTop(target, run.Y); sheet.Children.Add(target);
            }
        }

        private async Task<bool> ApplyOriginalTextAsync(EditorTextRun run, string value, string localFont = "", bool fitText = false)
        {
            if (Working || path is null) return false;
            var replacement = new EditorMark(page, "replace-text", run.X, run.Y, run.Width, run.Height, value, run.Size, SourceId: run.Id, OriginalText: run.Text, LocalFont: localFont, FitText: fitText);
            var draft = marks.Where(m => !(m.Kind == "replace-text" && m.Page == page && m.SourceId == run.Id)).ToList();
            if (value != run.Text || localFont.Length > 0) draft.Add(replacement);
            if (draft.Count > 100) { notice.Text = "Maksimal 100 perubahan/objek."; return false; }
            if (!await BuildOriginalPreviewAsync(draft.ToArray())) return false;
            Snapshot(); marks.Clear(); marks.AddRange(draft);
            await RenderAsync(page, false, false); originalId = run.Id; BuildProperties(); Refresh();
            notice.Text = "Teks asli diperbarui pada preview. Simpan PDF baru untuk menyimpan hasilnya.";
            return true;
        }

        private async Task RestoreOriginalHistoryAsync(bool forward)
        {
            var from = forward ? future : past; var to = forward ? past : future;
            if (from.Count == 0) return;
            var candidate = from.Peek();
            if (!await BuildOriginalPreviewAsync(candidate)) return;
            to.Push(marks.ToArray()); from.Pop(); marks.Clear(); marks.AddRange(candidate); chosen = -1;
            await RenderAsync(page, true, false); BuildProperties(); Refresh();
        }

        private async Task<bool> BuildOriginalPreviewAsync(EditorMark[] draft)
        {
            if (path is null) return false;
            busy = true; Refresh(); string? temporary = null; IRandomAccessStream? stream = null;
            try
            {
                var replacements = draft.Where(m => m.Kind == "replace-text").ToArray();
                PdfDocument loaded;
                if (replacements.Length == 0) loaded = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(path));
                else
                {
                    temporary = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "oto-editor-preview-" + Guid.NewGuid().ToString("N") + ".pdf");
                    using var cancellation = new CancellationTokenSource(); owner.active = cancellation;
                    PdfReply reply;
                    try { reply = await WorkerClient.RunAsync(new("edit", [path], temporary, Marks: replacements), cancellation.Token); }
                    finally { owner.active = null; }
                    if (!reply.Success) { notice.Text = reply.Message; return false; }
                    stream = await (await StorageFile.GetFileFromPathAsync(temporary)).OpenReadAsync();
                    loaded = await PdfDocument.LoadFromStreamAsync(stream);
                }
                pdf = loaded; ClearOriginalPreview(); originalPreviewFile = temporary; originalPreviewStream = stream; temporary = null; stream = null;
                foreach (var image in editorImages) image.Source = null;
                return true;
            }
            catch (Exception) { notice.Text = "Preview teks belum dapat diperbarui. Perubahan tidak diterapkan; sumber tetap aman."; return false; }
            finally
            {
                stream?.Dispose(); DeletePreview(temporary); busy = false; Refresh();
            }
        }
        private static void DeletePreview(string? file)
        {
            if (file is null) return;
            try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        private void ClearOriginalPreview()
        {
            originalPreviewStream?.Dispose(); originalPreviewStream = null;
            DeletePreview(originalPreviewFile); originalPreviewFile = null;
        }
    }
}
