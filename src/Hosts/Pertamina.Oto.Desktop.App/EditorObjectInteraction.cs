using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private string? resizeHandle;
        private sealed class HandleBorder : UserControl
        {
            internal HandleBorder(InputSystemCursorShape cursor) => ProtectedCursor = InputSystemCursor.Create(cursor);
        }
        private static bool KeyDown(VirtualKey key) => (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key) & CoreVirtualKeyStates.Down) != 0;
        internal bool Typing()
        {
            var element = FocusManager.GetFocusedElement(View.XamlRoot) as DependencyObject;
            while (element is not null && element != View)
            {
                if (element is TextBox or RichEditBox or NumberBox or ComboBox) return true;
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
        private void ObjectKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Handled || saveScreen is not null) return;
            if (FontShortcut(e.Key, KeyDown(VirtualKey.Control), KeyDown(VirtualKey.Shift))) { e.Handled = true; return; }
            if (HandleObjectKey(e.Key, KeyDown(VirtualKey.Control), KeyDown(VirtualKey.Shift))) e.Handled = true;
        }
        private bool HandleObjectKey(VirtualKey key, bool control = false, bool shift = false)
        {
            if (Working || saveScreen is not null || Typing() || !Visible) return false;
            if (key == VirtualKey.Escape) { CancelGesture(); chosen = -1; mode = "Pilih"; BuildProperties(); Refresh(); return true; }
            if (key == VirtualKey.Delete && mode == "Edit teks asli" && originalRuns.FirstOrDefault(r => r.Id == originalId) is { } original)
            { _ = ApplyOriginalTextAsync(original, ""); return true; }
            if (key is VirtualKey.F2 or VirtualKey.Enter && mode == "Edit teks asli" && originalRuns.FirstOrDefault(r => r.Id == originalId) is { } selectedOriginal)
            { StartOriginalInline(selectedOriginal); return true; }
            if (chosen < 0 || chosen >= marks.Count || marks[chosen].Kind == "replace-text") return false;
            if (key is VirtualKey.F2 or VirtualKey.Enter && marks[chosen].Kind == "text") { StartInlineText(chosen); return true; }
            if (key == VirtualKey.Delete) { DeleteSelected(); return true; }
            else if (key == VirtualKey.D && control) { DuplicateSelected(); return true; }
            else if (key is VirtualKey.Left or VirtualKey.Right or VirtualKey.Up or VirtualKey.Down)
            {
                var amount = shift ? 10 : 1;
                var m = marks[chosen]; var dx = key == VirtualKey.Left ? -amount : key == VirtualKey.Right ? amount : 0;
                var dy = key == VirtualKey.Up ? -amount : key == VirtualKey.Down ? amount : 0;
                var moved = m with { X = Math.Clamp(m.X + dx, 0, sheet.Width - m.Width), Y = Math.Clamp(m.Y + dy, 0, sheet.Height - m.Height) };
                if (moved != m) { Snapshot(); marks[chosen] = moved; BuildProperties(); Refresh(); }
                return true;
            }
            return false;
        }
        private void DeleteSelected()
        {
            if (Working || chosen < 0 || chosen >= marks.Count || marks[chosen].Kind == "replace-text") return;
            CancelGesture();
            Snapshot(); marks.RemoveAt(chosen); chosen = -1; BuildProperties(); Refresh();
        }
        private void DuplicateSelected()
        {
            if (Working || chosen < 0 || chosen >= marks.Count || marks[chosen].Kind == "replace-text") return;
            CancelGesture();
            var m = marks[chosen]; Add(m with { X = Math.Min(m.X + 12, sheet.Width - m.Width), Y = Math.Min(m.Y + 12, sheet.Height - m.Height) });
        }
        private void AddResizeHandles(EditorMark mark)
        {
            foreach (var (name, fx, fy, cursor) in new[] {
                ("nw", 0d, 0d, InputSystemCursorShape.SizeNorthwestSoutheast), ("n", .5, 0d, InputSystemCursorShape.SizeNorthSouth),
                ("ne", 1d, 0d, InputSystemCursorShape.SizeNortheastSouthwest), ("e", 1d, .5, InputSystemCursorShape.SizeWestEast),
                ("se", 1d, 1d, InputSystemCursorShape.SizeNorthwestSoutheast), ("s", .5, 1d, InputSystemCursorShape.SizeNorthSouth),
                ("sw", 0d, 1d, InputSystemCursorShape.SizeNortheastSouthwest), ("w", 0d, .5, InputSystemCursorShape.SizeWestEast) })
            {
                var size = 20 / viewport.ZoomFactor;
                var target = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent) };
                target.Children.Add(new Border { Width = 7 / viewport.ZoomFactor, Height = 7 / viewport.ZoomFactor, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    Background = Brush(255, 255, 255), BorderBrush = Brush(8, 102, 232), BorderThickness = new Thickness(1 / viewport.ZoomFactor), CornerRadius = new CornerRadius(1) });
                var handle = new HandleBorder(cursor) { Width = size, Height = size, Content = target };
                ToolTipService.SetToolTip(handle, "Tarik untuk mengubah ukuran · Shift: jaga proporsi");
                AutomationProperties.SetName(handle, "Pegangan ukuran " + name);
                Canvas.SetLeft(handle, mark.X + mark.Width * fx - size / 2); Canvas.SetTop(handle, mark.Y + mark.Height * fy - size / 2);
                handle.PointerPressed += (_, e) =>
                {
                    if (Working || !e.GetCurrentPoint(sheet).Properties.IsLeftButtonPressed) return;
                    viewport.Focus(FocusState.Programmatic); dragOrigin = e.GetCurrentPoint(sheet).Position;
                    if (sheet.CapturePointer(e.Pointer)) { dragging = marks[chosen]; resizeHandle = name; }
                    e.Handled = true;
                };
                sheet.Children.Add(handle);
            }
        }
        private static EditorMark ResizeObject(EditorMark m, string handle, double dx, double dy, bool ratio, double pageWidth, double pageHeight)
        {
            var min = Math.Max(8, m.Kind is "rectangle" or "ellipse" ? m.Size + 1 : 8);
            var minWidth = Math.Min(min, m.Width); var minHeight = Math.Min(min, m.Height);
            var left = handle.Contains('w') ? Math.Clamp(m.X + dx, 0, m.X + m.Width - minWidth) : m.X;
            var right = handle.Contains('e') ? Math.Clamp(m.X + m.Width + dx, m.X + minWidth, pageWidth) : m.X + m.Width;
            var top = handle.Contains('n') ? Math.Clamp(m.Y + dy, 0, m.Y + m.Height - minHeight) : m.Y;
            var bottom = handle.Contains('s') ? Math.Clamp(m.Y + m.Height + dy, m.Y + minHeight, pageHeight) : m.Y + m.Height;
            if (ratio && handle.Length == 2)
            {
                var scale = Math.Max((right - left) / m.Width, (bottom - top) / m.Height);
                var maxW = handle.Contains('w') ? m.X + m.Width : pageWidth - m.X;
                var maxH = handle.Contains('n') ? m.Y + m.Height : pageHeight - m.Y;
                scale = Math.Min(scale, Math.Min(maxW / m.Width, maxH / m.Height));
                var w = m.Width * scale; var h = m.Height * scale;
                if (handle.Contains('w')) left = right - w; else right = left + w;
                if (handle.Contains('n')) top = bottom - h; else bottom = top + h;
            }
            var width = right - left; var height = bottom - top;
            return m with { X = left, Y = top, Width = width, Height = height,
                Points = m.Points?.Select(p => new EditorPoint(p.X * width / m.Width, p.Y * height / m.Height)).ToArray() };
        }
    }
}
