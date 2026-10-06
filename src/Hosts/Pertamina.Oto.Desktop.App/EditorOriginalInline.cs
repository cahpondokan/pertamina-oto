using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Foundation;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private sealed record OriginalInlineDraft(EditorTextRun Run, string Text, string Font, bool Fit, string InitialText, string InitialFont, bool InitialFit)
        {
            internal bool Changed => Text != InitialText || Font != InitialFont || Fit != InitialFit;
        }
        private OriginalInlineDraft? originalInlineDraft;
        private EditorTextRun? inlineOriginalRun;
        private Task<bool>? originalInlineCommit;

        private async Task OriginalTextPressedAsync(EditorTextRun run, Point point, long timestamp)
        {
            if (Working || string.IsNullOrWhiteSpace(run.Text)) return;
            if (originalInlineDraft is not null && !await CommitOriginalInlineAsync()) return;
            originalId = run.Id; chosen = -1;
            if (CountTextClick($"original:{page}:{run.Id}", point, timestamp) == 2) StartOriginalInline(run, point.X - run.X);
            else { viewport.Focus(FocusState.Programmatic); BuildProperties(); Refresh(); }
        }
        private void StartOriginalInline(EditorTextRun run, double? wordAt = null)
        {
            if (Working) return;
            if (originalInlineDraft is { } existing)
            {
                if (existing.Run.Id == run.Id) ShowOriginalInline(existing, wordAt);
                else notice.Text = "Selesaikan ketikan saat ini dengan Enter, atau batalkan dengan Esc.";
                return;
            }
            var current = marks.FirstOrDefault(m => m.Kind == "replace-text" && m.Page == page && m.SourceId == run.Id);
            var value = current?.Text ?? run.Text;
            var selectedFont = properties.Children.OfType<ComboBox>().FirstOrDefault(c => c.Header as string == "Font teks pengganti");
            var font = selectedFont is null ? current?.LocalFont ?? ""
                : selectedFont.SelectedIndex > 0 ? selectedFont.SelectedItem as string ?? "" : "";
            var fit = current?.FitText ?? false;
            ShowOriginalInline(new(run, value, font, fit, value, font, fit), wordAt);
        }
        private void ShowOriginalInline(OriginalInlineDraft draft, double? wordAt = null)
        {
            EndInlineText(); originalInlineDraft = draft; inlineOriginalRun = draft.Run;
            originalId = draft.Run.Id; chosen = -1; mode = "Edit teks asli";
            var input = new TextBox { Text = draft.Text, MaxLength = 200, AcceptsReturn = false, TextWrapping = TextWrapping.NoWrap,
                Width = Math.Min(Math.Max(32, draft.Run.Width), sheet.Width - draft.Run.X), Height = Math.Max(26, draft.Run.Height + 4),
                FontSize = draft.Run.Size, Padding = new Thickness(0), MinWidth = 0, MinHeight = 0,
                BorderBrush = Brush(8, 102, 232), BorderThickness = new Thickness(1), Background = Brush(255, 255, 255) };
            inlineText = input; ApplyInlineOriginalFont(draft.Font);
            AutomationProperties.SetName(input, "Edit teks asli langsung pada PDF");
            input.TextChanged += (_, _) =>
            {
                if (inlineText != input || originalInlineDraft is not { } pending || Working) return;
                originalInlineDraft = pending with { Text = input.Text };
                foreach (var field in properties.Children.OfType<TextBox>()) if (field.Text != input.Text) field.Text = input.Text;
                Refresh();
            };
            input.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((_, e) =>
            {
                if (e.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed && InlineTextPressed(input, e.GetCurrentPoint(sheet).Position, Environment.TickCount64)) e.Handled = true;
            }), true);
            input.KeyDown += async (_, e) =>
            {
                if (e.Key == VirtualKey.Enter) { e.Handled = true; await CommitOriginalInlineAsync(); }
                else if (e.Key == VirtualKey.Escape) { e.Handled = true; CancelOriginalInline(); }
            };
            input.LostFocus += (_, _) => owner.DispatcherQueue.TryEnqueue(async () =>
            {
                if (ReadPreviewFocus()) return;
                if (inlineText != input || !View.IsLoaded || Working) return;
                var focused = FocusManager.GetFocusedElement(View.XamlRoot) as DependencyObject;
                while (focused is not null) { if (focused == View) return; focused = VisualTreeHelper.GetParent(focused); }
                await CommitOriginalInlineAsync();
            });
            Canvas.SetLeft(input, draft.Run.X); Canvas.SetTop(input, draft.Run.Y);
            BuildProperties(); Refresh();
            owner.DispatcherQueue.TryEnqueue(() =>
            {
                if (inlineText != input) return;
                input.Focus(FocusState.Programmatic);
                if (wordAt is { } x) SelectWordAt(input, x); else input.SelectAll();
            });
            notice.Text = "Edit teks asli langsung · Double-click: kata · Triple-click: seluruh bagian · Enter: terapkan · Esc: batalkan. " + (draft.Font.Length > 0 ? "Font lokal: " + draft.Font : "Font PDF asli; karakter terbatas.");
        }
        private void ApplyInlineOriginalFont(string font)
        {
            if (inlineText is null) return;
            var family = font.Replace(" Bold", "", StringComparison.Ordinal).Replace(" Italic", "", StringComparison.Ordinal);
            inlineText.FontFamily = new FontFamily(family.Length == 0 ? "Arial" : family);
            inlineText.FontWeight = font.Contains("Bold", StringComparison.Ordinal) ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal;
            inlineText.FontStyle = font.Contains("Italic", StringComparison.Ordinal) ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
        }
        private async Task<bool> CommitOriginalInlineAsync()
        {
            if (originalInlineCommit is { } active) return await active;
            if (originalInlineDraft is null) return true;
            var operation = CommitOriginalInlineCoreAsync(); originalInlineCommit = operation;
            try { return await operation; }
            finally { originalInlineCommit = null; }
        }
        private async Task<bool> CommitOriginalInlineCoreAsync()
        {
            var draft = originalInlineDraft!;
            if (Working) return false;
            if (inlineOriginalRun is not null && inlineText is not null) draft = draft with { Text = inlineText.Text };
            originalInlineDraft = null; EndInlineText();
            if (!draft.Changed) { BuildProperties(); Refresh(); return true; }
            if (await ApplyOriginalTextAsync(draft.Run, draft.Text, draft.Font, draft.Fit)) return true;
            var error = notice.Text;
            // Never discard rejected input: restore exact draft so it can be corrected or cancelled.
            ShowOriginalInline(draft); notice.Text = error + " Draf ketikan tetap tersedia; perbaiki atau tekan Esc.";
            return false;
        }
        private void CancelOriginalInline()
        {
            originalInlineDraft = null; EndInlineText(); BuildProperties(); Refresh(); viewport.Focus(FocusState.Programmatic);
            notice.Text = "Ketikan terakhir dibatalkan. Perubahan yang sudah diterapkan tetap tersedia.";
        }
        private async Task ApplyOriginalFromInspectorAsync(EditorTextRun run, string text, string font, bool fit)
        {
            if (originalInlineDraft is { } draft && draft.Run.Id == run.Id)
            {
                originalInlineDraft = draft with { Text = text, Font = font, Fit = fit };
                if (inlineOriginalRun is not null && inlineText is not null && inlineText.Text != text) inlineText.Text = text;
                await CommitOriginalInlineAsync();
            }
            else await ApplyOriginalTextAsync(run, text, font, fit);
        }
    }
}
