using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private TextBox? inlineText;
        private int inlineIndex = -1;
        private bool inlineSnapshot;

        private void PlaceObjectAt(Point position)
        {
            if (mode != "Tambah teks") return; // Shapes are created only by a completed drag.
            var isText = mode == "Tambah teks";
            var width = Math.Min(220, sheet.Width - position.X - 4);
            var height = Math.Min(isText ? 32 : 80, sheet.Height - position.Y - 4);
            if (width < 20 || height < 20 || marks.Count >= 100) return;
            Add(new(page, isText ? "text" : shape, position.X, position.Y, width, height, isText ? "Teks baru" : "", isText ? 14 : 2));
            if (isText) StartInlineText(chosen);
        }

        private void StartInlineText(int index, double? wordAt = null)
        {
            if (Working || index < 0 || index >= marks.Count || marks[index].Kind != "text") return;
            EndInlineText(); chosen = index; mode = "Pilih"; inlineIndex = index; inlineSnapshot = false;
            var mark = marks[index];
            var editor = new TextBox { Text = mark.Text, MaxLength = 200, AcceptsReturn = false, TextWrapping = TextWrapping.NoWrap,
                FontFamily = new FontFamily("Arial"), FontSize = mark.Size, Padding = new Thickness(0), MinWidth = 0, MinHeight = 0,
                Width = mark.Width, Height = Math.Max(24, mark.Height), BorderThickness = new Thickness(1), BorderBrush = Brush(8, 102, 232), Background = Brush(255, 255, 255) };
            AutomationProperties.SetName(editor, "Ketik teks pada halaman PDF"); inlineText = editor;
            editor.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((_, e) =>
            {
                if (!e.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed) return;
                if (InlineTextPressed(editor, e.GetCurrentPoint(sheet).Position, Environment.TickCount64)) e.Handled = true;
            }), true);
            editor.TextChanged += (_, _) =>
            {
                if (inlineText != editor || Working || inlineIndex < 0 || inlineIndex >= marks.Count || marks[inlineIndex].Text == editor.Text) return;
                if (!inlineSnapshot) { Snapshot(); inlineSnapshot = true; }
                marks[inlineIndex] = marks[inlineIndex] with { Text = editor.Text };
                // Refresh must retain this exact input control and keyboard focus.
                Refresh();
            };
            editor.KeyDown += (_, e) =>
            {
                if (e.Key is VirtualKey.Enter or VirtualKey.Escape) { e.Handled = true; EndInlineText(); BuildProperties(); Refresh(); viewport.Focus(FocusState.Programmatic); }
            };
            editor.LostFocus += (_, _) => owner.DispatcherQueue.TryEnqueue(() =>
            {
                if (ReadPreviewFocus()) return;
                if (inlineText == editor && (View.XamlRoot is null || (FocusManager.GetFocusedElement(View.XamlRoot) as TextBox) != editor))
                {
                    var index = inlineIndex; EndInlineText();
                    if (index >= 0 && index < marks.Count)
                    {
                        updating = true;
                        foreach (var input in properties.Children.OfType<TextBox>()) input.Text = marks[index].Text;
                        updating = false;
                    }
                    Refresh();
                }
            });
            Canvas.SetLeft(editor, mark.X); Canvas.SetTop(editor, mark.Y);
            BuildProperties(); Refresh();
            void FocusInput() { if (inlineText == editor) { editor.Focus(FocusState.Programmatic); if (wordAt is { } x) SelectWordAt(editor, x); else editor.SelectAll(); } }
            owner.DispatcherQueue.TryEnqueue(FocusInput);
            notice.Text = "Double-click: edit/pilih kata · Triple-click: pilih semua · Ctrl+[ / Ctrl+]: ukuran font · Enter: selesai.";
        }

        private void EndInlineText()
        {
            if (inlineText is not null && inlineOriginalRun is not null && originalInlineDraft is { } draft)
                originalInlineDraft = draft with { Text = inlineText.Text };
            var previous = inlineText; inlineText = null; inlineIndex = -1; inlineSnapshot = false;
            inlineOriginalRun = null;
            if (previous is not null) sheet.Children.Remove(previous);
        }
    }
}
