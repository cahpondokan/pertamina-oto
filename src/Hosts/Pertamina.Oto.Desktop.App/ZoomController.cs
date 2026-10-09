using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static void ApplyViewportZoom(ScrollViewer viewport, double scale)
    {
        var old = viewport.ZoomFactor;
        var x = (viewport.HorizontalOffset + viewport.ActualWidth / 2) / old;
        var y = (viewport.VerticalOffset + viewport.ActualHeight / 2) / old;
        viewport.ChangeView(Math.Max(0, x * scale - viewport.ActualWidth / 2),
            Math.Max(0, y * scale - viewport.ActualHeight / 2), (float)scale, true);
    }
    // A viewport owns its scale. Toolbars display it; menu commands never search for sliders.
    private sealed class ZoomController
    {
        private static readonly ConditionalWeakTable<DependencyObject, ZoomController> targets = new();
        private static WeakReference<ZoomController>? last;
        private static readonly string[] presets = ["25%", "50%", "75%", "100%", "125%", "150%", "200%", "300%", "400%", "500%"];
        private readonly ScrollViewer viewport;
        private readonly Func<double> read;
        private readonly Action<double> apply;
        private readonly Func<bool> ready;
        private readonly double minimum, maximum;
        internal StackPanel Toolbar { get; } = new() { Orientation = Orientation.Horizontal, Spacing = 6 };
        internal ComboBox Percent { get; } = new() { IsEditable = true, Width = 94, MinHeight = 36 };
        private readonly Button minus = HistoryButton("minus", "Perkecil tampilan");
        private readonly Button plus = HistoryButton("plus", "Perbesar tampilan");
        private bool syncing;
        private bool nativeIntermediate;
        private bool nativeManipulating;
        private readonly DispatcherTimer rasterTimer = new() { Interval = TimeSpan.FromMilliseconds(180) };
        private readonly DispatcherTimer percentTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };

        internal ZoomController(ScrollViewer viewport, string caption, double minimum, double maximum,
            Func<double> read, Action<double> apply, Action<bool>? fit = null, Func<bool>? ready = null)
        {
            this.viewport = viewport; this.minimum = minimum; this.maximum = maximum;
            this.read = read; this.apply = apply; this.ready = ready ?? (() => true);
            minus.MinWidth = plus.MinWidth = 36; minus.MinHeight = plus.MinHeight = 36;
            var captionLabel = HistoryText(caption, 12);

            Toolbar.Children.Add(captionLabel); Toolbar.Children.Add(minus);
            AutomationProperties.SetName(minus, caption == "Zoom" ? "Perkecil PDF" : "Perkecil kartu");
            AutomationProperties.SetName(plus, caption == "Zoom" ? "Perbesar PDF" : "Perbesar kartu");
            Percent.ItemsSource = presets
                .Where(s => TryPercent(s, minimum, maximum, out _)).ToArray();
            AutomationProperties.SetName(Percent, caption + " dalam persen");
            ToolTipService.SetToolTip(Percent, $"{minimum * 100:0}–{maximum * 100:0}% · Enter untuk terapkan · Escape untuk batalkan");
            Toolbar.Children.Add(Percent); Toolbar.Children.Add(plus);
            minus.Click += (_, _) => Command(-1); plus.Click += (_, _) => Command(1);
            Percent.SelectionChanged += (_, _) => { if (!syncing && Percent.SelectedItem is string s) { Submit(s); Percent.DispatcherQueue.TryEnqueue(() => Sync(force: true)); } };
            Percent.TextSubmitted += (_, e) =>
            {
                e.Handled = true; Submit(e.Text);
                RestorePercent();
            };
            Percent.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((_, e) =>
            {
                if (e.Key == VirtualKey.Escape) { e.Handled = true; RestorePercent(); }
            }), true);
            Percent.LostFocus += (_, _) => Sync(force: true);
            // An editable ComboBox creates its text presenter after the containing
            // toolbar loads. Synchronize again after that template is attached.
            Percent.Loaded += (_, _) => RestorePercent();
            percentTimer.Tick += (_, _) => { percentTimer.Stop(); Sync(force: true); };
            Percent.Unloaded += (_, _) => percentTimer.Stop();
            if (fit is null)
            {
                var reset = new Button { Content = "Reset", MinHeight = 36 };
                AutomationProperties.SetName(reset, "Reset ukuran kartu ke 100 persen");
                reset.Click += (_, _) => Command(0); Toolbar.Children.Add(reset);
            }
            else
            {
                var fitting = new Button { Content = "Sesuaikan", MinHeight = 36 };
                AutomationProperties.SetName(fitting, "Pilihan sesuaikan halaman atau lebar");
                var menu = new MenuFlyout();
                foreach (var width in new[] { false, true })
                {
                    var item = new MenuFlyoutItem { Text = width ? "Sesuaikan lebar" : "Sesuaikan halaman" };
                    item.Click += (_, _) => { if (!this.ready()) return; Activate(); fit(width); ToolTipService.SetToolTip(fitting, item.Text); };
                    menu.Items.Add(item);
                }
                fitting.Flyout = menu; Toolbar.Children.Add(fitting);
            }
            targets.Add(viewport, this); targets.Add(Toolbar, this);
            viewport.PointerPressed += (_, _) => Activate(); viewport.GotFocus += (_, _) => Activate();
            Toolbar.GotFocus += (_, _) => Activate(); Toolbar.PointerPressed += (_, _) => Activate();
            viewport.ViewChanged += (_, _) => Sync(force: true);
            rasterTimer.Tick += async (_, _) =>
            {
                rasterTimer.Stop(); var scale = read();
                if (nativeIntermediate || nativeManipulating) { rasterTimer.Start(); return; }
                try { await RefreshVisiblePreviewRastersAsync(viewport); }
                catch (Exception error) { LocalDiagnostics.Record("preview-zoom-render", error); }
                if (viewport.IsLoaded && Math.Abs(scale - read()) > .001) rasterTimer.Start();
            };
            viewport.DirectManipulationStarted += (_, _) => { nativeManipulating = true; rasterTimer.Stop(); };
            viewport.DirectManipulationCompleted += (_, _) => { nativeManipulating = false; rasterTimer.Start(); };
            viewport.ViewChanged += (_, e) => { nativeIntermediate = e.IsIntermediate; rasterTimer.Stop(); rasterTimer.Start(); };
            viewport.Unloaded += (_, _) => { rasterTimer.Stop(); nativeIntermediate = nativeManipulating = false; };
            viewport.Loaded += (_, _) => { Activate(); Sync(force: true); };
            Toolbar.Loaded += (_, _) => Sync(force: true);
            viewport.KeyDown += KeyDown; Toolbar.KeyDown += KeyDown;
            Sync(force: true);
        }

        internal static bool TryPercent(string? text, double min, double max, out double scale)
        {
            scale = 0; var number = text?.Trim().TrimEnd('%').Trim();
            if (!double.TryParse(number, NumberStyles.Float, CultureInfo.CurrentCulture, out var value)
                && !double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return false;
            scale = value / 100;
            return double.IsFinite(scale) && scale > 0 && scale >= min && scale <= max;
        }
        private void Submit(string text)
        {
            if (TryPercent(text, minimum, maximum, out var value) && ready()) { Activate(); apply(value); }
            else AutomationProperties.SetItemStatus(Percent, $"Masukkan {minimum * 100:0}–{maximum * 100:0} persen");
            Sync(force: true);
            rasterTimer.Stop(); rasterTimer.Start();
        }
        internal void Activate() => last = new(this);
        private void RestorePercent()
        {
            Percent.DispatcherQueue.TryEnqueue(() => Sync(force: true));
            percentTimer.Stop(); percentTimer.Start();
        }
        internal void Command(int direction)
        {
            if (!ready()) return;
            Activate(); apply(Math.Clamp(direction == 0 ? 1 : read() * (direction > 0 ? 1.15 : 1 / 1.15), minimum, maximum)); Sync(force: true);
            rasterTimer.Stop(); rasterTimer.Start();
        }
        internal void Sync(bool force = false)
        {
            var value = read(); var enabled = ready();
            foreach (var control in Toolbar.Children.OfType<Control>().Concat(Toolbar.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<Control>()))) control.IsEnabled = enabled;
            minus.IsEnabled = enabled && value > minimum + .0001; plus.IsEnabled = enabled && value < maximum - .0001;
            if (!force && Percent.XamlRoot is not null && FocusManager.GetFocusedElement(Percent.XamlRoot) is DependencyObject focus && IsWithin(focus, Percent)) return;
            var text = enabled ? $"{value * 100:0.##}%" : "—";
            syncing = true;
            // Preserve a real preset selection when possible. Clearing it on every
            // refresh can clear the native editable presenter after Text was set.
            Percent.SelectedItem = Percent.Items.Cast<string>().FirstOrDefault(item => item == text);
            Percent.Text = text;
            syncing = false;
        }
        private static bool IsWithin(DependencyObject node, DependencyObject ancestor)
        {
            for (DependencyObject? current = node; current is not null; current = VisualTreeHelper.GetParent(current)) if (current == ancestor) return true;
            return false;
        }
        private static bool Visible(DependencyObject node)
        {
            for (DependencyObject? current = node; current is not null; current = VisualTreeHelper.GetParent(current))
                if (current is UIElement { Visibility: Visibility.Collapsed }) return false;
            return true;
        }
        private void KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Handled || IsTextPasteTarget(e.OriginalSource as DependencyObject)) return;
            if ((Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & Windows.UI.Core.CoreVirtualKeyStates.Down) == 0) return;
            var direction = e.Key switch { VirtualKey.Add or (VirtualKey)187 => 1, VirtualKey.Subtract or (VirtualKey)189 => -1, VirtualKey.Number0 or VirtualKey.NumberPad0 => 0, _ => 2 };
            if (direction == 2) return; e.Handled = true; Command(direction);
        }
        internal static bool Route(DependencyObject? focus, int direction)
        {
            if (IsTextPasteTarget(focus)) return true;
            for (var node = focus; node is not null; node = VisualTreeHelper.GetParent(node))
                if (targets.TryGetValue(node, out var target)) { target.Command(direction); return true; }
            if (last?.TryGetTarget(out var previous) == true && previous.viewport.IsLoaded && Visible(previous.viewport))
            { previous.Command(direction); return true; }
            return false;
        }
    }
}
