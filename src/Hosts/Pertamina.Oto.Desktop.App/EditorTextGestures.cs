using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        [LibraryImport("user32.dll")]
        private static partial uint GetDoubleClickTime();
        private string? lastTextTarget;
        private Point lastTextPoint;
        private long lastTextClick;
        private int textClickCount;

        private int CountTextClick(string target, Point point, long timestamp)
        {
            var close = Math.Abs(point.X - lastTextPoint.X) * viewport.ZoomFactor <= 6 && Math.Abs(point.Y - lastTextPoint.Y) * viewport.ZoomFactor <= 6;
            textClickCount = target == lastTextTarget && timestamp >= lastTextClick && timestamp - lastTextClick <= GetDoubleClickTime() && close ? textClickCount % 3 + 1 : 1;
            lastTextTarget = target; lastTextPoint = point; lastTextClick = timestamp;
            return textClickCount;
        }
        private bool TextObjectPressed(int index, Point point, long timestamp)
        {
            if (marks[index].Kind != "text") return false;
            if (CountTextClick($"{page}:{index}", point, timestamp) != 2) return false;
            dragging = null; resizeHandle = null;
            StartInlineText(index, point.X - marks[index].X);
            return true;
        }
        private static void SelectWordAt(TextBox editor, double localX)
        {
            if (editor.Text.Length == 0) return;
            editor.UpdateLayout(); var position = 0;
            for (var i = 0; i < editor.Text.Length; i++)
            {
                if (editor.GetRectFromCharacterIndex(i, false).X > localX) break;
                position = i;
            }
            var start = position; var end = position + 1;
            if (!char.IsWhiteSpace(editor.Text[position]))
            {
                while (start > 0 && !char.IsWhiteSpace(editor.Text[start - 1])) start--;
                while (end < editor.Text.Length && !char.IsWhiteSpace(editor.Text[end])) end++;
            }
            editor.Select(start, end - start);
        }
        private bool InlineTextPressed(TextBox editor, Point point, long timestamp)
        {
            var key = inlineOriginalRun is { } run ? $"original:{page}:{run.Id}" : $"{page}:{inlineIndex}";
            if (inlineText != editor || CountTextClick(key, point, timestamp) != 3) return false;
            owner.DispatcherQueue.TryEnqueue(() => { if (inlineText == editor) editor.SelectAll(); });
            return true;
        }
        private bool FontShortcut(VirtualKey key, bool control, bool shift)
        {
            if (!control || Working || !Visible) return false;
            var direction = key == (VirtualKey)221 && !shift || key == (VirtualKey)190 && shift ? 1
                : key == (VirtualKey)219 && !shift || key == (VirtualKey)188 && shift ? -1 : 0;
            if (direction == 0) return false;
            if (Typing() && inlineText is null) return false;
            if (mode == "Edit teks asli") { notice.Text = "Pintasan ukuran saat ini untuk teks tambahan. Ukuran font teks asli masih dipertahankan."; return true; }
            var index = inlineText is null ? chosen : inlineIndex;
            if (index < 0 || index >= marks.Count || marks[index].Kind != "text") return false;
            var mark = marks[index];
            double[] sizes = [1, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72];
            var size = shift ? direction > 0 ? sizes.FirstOrDefault(n => n > mark.Size, 72) : sizes.LastOrDefault(n => n < mark.Size, 1) : Math.Clamp(mark.Size + direction, 1, 72);
            if (size == mark.Size) return true;
            var measure = new TextBlock { Text = mark.Text, FontFamily = new FontFamily("Arial"), FontSize = size };
            measure.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var width = Math.Max(mark.Width, Math.Ceiling(measure.DesiredSize.Width + 8));
            var height = Math.Max(mark.Height, Math.Ceiling(measure.DesiredSize.Height + 4));
            if (width > sheet.Width - mark.X || height > sheet.Height - mark.Y)
            { notice.Text = "Font tidak diperbesar karena teks akan melewati halaman. Pindahkan kotak teks terlebih dahulu."; return true; }
            Snapshot(); marks[index] = mark with { Size = size, Width = width, Height = height };
            if (inlineText is not null)
            {
                inlineText.FontSize = size; inlineText.Width = width; inlineText.Height = Math.Max(24, height);
                updating = true;
                foreach (var number in properties.Children.OfType<NumberBox>())
                {
                    if (number.Header as string == "Ukuran teks (pt)") number.Value = size;
                    else if (number.Header as string == "Lebar (pt)") number.Value = width;
                    else if (number.Header as string == "Tinggi (pt)") number.Value = height;
                }
                updating = false;
            }
            else BuildProperties();
            Refresh(); notice.Text = $"Ukuran font: {size:0.#} pt · Ctrl+[ / Ctrl+] : 1 pt · Ctrl+Shift+< / > : bertahap";
            return true;
        }
    }
}
