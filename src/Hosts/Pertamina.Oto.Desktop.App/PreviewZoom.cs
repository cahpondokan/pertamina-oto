using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private double previewScale = 1;
    private event Action? PreviewScaleChanged;

    // Native ScrollViewer handles precision-touchpad pinch. At gesture completion
    // convert its visual zoom into card dimensions so columns wrap, not clip.
    private StackPanel ZoomablePreview(GridView grid)
    {
        var sizes = new Dictionary<FrameworkElement, (double Width, double Height)>();
        void Resize(DependencyObject node)
        {
            if (node is FrameworkElement element && (node is Image || node is StackPanel))
            {
                if (!sizes.TryGetValue(element, out var size))
                    sizes[element] = size = (element.Width, element.Height);
                if (!double.IsNaN(size.Width) && element.Width != size.Width * previewScale) element.Width = size.Width * previewScale;
                if (!double.IsNaN(size.Height) && element.Height != size.Height * previewScale) element.Height = size.Height * previewScale;
            }
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) Resize(VisualTreeHelper.GetChild(node, i));
        }
        var viewport = new ScrollViewer
        {
            Content = grid, MaxHeight = double.IsInfinity(grid.MaxHeight) ? 650 : grid.MaxHeight,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled
        };
        grid.MaxHeight = double.PositiveInfinity;
        ScrollViewer.SetVerticalScrollMode(grid, ScrollMode.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
        grid.Loaded += (_, _) => Resize(grid);
        grid.ContainerContentChanging += (_, e) =>
        {
            if (!e.InRecycleQueue) grid.DispatcherQueue.TryEnqueue(() => Resize(e.ItemContainer));
        };
        return PreviewZoomControls(viewport, () => Resize(grid));
    }

    private FrameworkElement? organizerZoomHost;
    private FrameworkElement OrganizerZoomHost()
    {
        return organizerZoomHost ??= PreviewZoomControls(pageGrid, () =>
        {
            CancelOrganizerDrag();
            ArrangeOrganizerCards(pageLayout, false);
        });
    }

    private StackPanel PreviewZoomControls(ScrollViewer viewport, Action reflow)
    {
        var panel = new StackPanel { Spacing = 8 };
        bool resetting = false;
        bool manipulating = false;
        var resetRetries = 0;
        var settleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        (FrameworkElement? Item, double Y) gestureAnchor = (null, 0);
        ZoomController? controller = null;
        (FrameworkElement? Item, double Y) Anchor()
        {
            IEnumerable<FrameworkElement> candidates = viewport.Content is GridView cards
                ? Enumerable.Range(0, cards.Items.Count).Select(i => cards.ContainerFromIndex(i)).OfType<FrameworkElement>()
                : Descendants((DependencyObject)viewport.Content).OfType<FrameworkElement>().Where(e => e is Image);
            foreach (var item in candidates)
            {
                var y = item.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y;
                if (y + item.ActualHeight * viewport.ZoomFactor >= 0) return (item, y);
            }
            return (null, 0);
        }
        void Refresh()
        {
            reflow();
            viewport.MinZoomFactor = (float)(.5 / previewScale); viewport.MaxZoomFactor = (float)(2 / previewScale);
            controller?.Sync();
        }
        void Apply(double scale)
        {
            if (active is not null || importing) return;
            var anchor = Anchor();
            previewScale = Math.Clamp(scale, .5, 2);
            if (Math.Abs(viewport.ZoomFactor - 1) > .0001)
            {
                resetting = true; resetRetries = 0; gestureAnchor = anchor;
                viewport.ChangeView(0, viewport.VerticalOffset / viewport.ZoomFactor, 1, true);
                settleTimer.Stop(); settleTimer.Start(); return;
            }
            PreviewScaleChanged?.Invoke(); Refresh(); viewport.UpdateLayout();
            if (anchor.Item is { IsLoaded: true } item)
            {
                var y = item.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y;
                viewport.ChangeView(null, Math.Max(0, viewport.VerticalOffset + y - anchor.Y), null, true);
            }
        }
        controller = new ZoomController(viewport, "Ukuran kartu", .5, 2,
            () => resetting ? previewScale : previewScale * viewport.ZoomFactor, Apply, ready: () => active is null && !importing);
        viewport.Loaded += (_, _) => { PreviewScaleChanged += Refresh; Refresh(); };
        viewport.Unloaded += (_, _) => { PreviewScaleChanged -= Refresh; settleTimer.Stop(); };
        viewport.ZoomMode = ZoomMode.Enabled; Refresh();
        void CompleteNativeZoom()
        {
            if (manipulating) return;
            if (resetting)
            {
                if (Math.Abs(viewport.ZoomFactor - 1) < .0001)
                {
                    resetting = false; PreviewScaleChanged?.Invoke(); Refresh(); viewport.UpdateLayout();
                    if (gestureAnchor.Item is { IsLoaded: true } item)
                    {
                        var y = item.TransformToVisual(viewport).TransformPoint(new Windows.Foundation.Point()).Y;
                        viewport.ChangeView(null, Math.Max(0, viewport.VerticalOffset + y - gestureAnchor.Y), null, true);
                    }
                    controller.Sync();
                }
                else if (resetRetries++ < 3)
                {
                    viewport.ChangeView(0, viewport.VerticalOffset / viewport.ZoomFactor, 1, true);
                    settleTimer.Start();
                }
                return;
            }
            if (Math.Abs(viewport.ZoomFactor - 1) < .0001) return;
            resetting = true;
            resetRetries = 0;
            gestureAnchor = Anchor();
            var relative = viewport.ZoomFactor;
            var offset = viewport.VerticalOffset / relative;
            previewScale = Math.Clamp(previewScale * relative, .5, 2);
            viewport.ChangeView(0, offset, 1, true);
            settleTimer.Stop(); settleTimer.Start();
        }
        settleTimer.Tick += (_, _) => { settleTimer.Stop(); CompleteNativeZoom(); };
        viewport.DirectManipulationStarted += (_, _) => { manipulating = true; settleTimer.Stop(); };
        viewport.DirectManipulationCompleted += (_, _) => { manipulating = false; CompleteNativeZoom(); };
        viewport.ViewChanged += (_, e) =>
        {
            controller.Sync(force: true);
            if (!e.IsIntermediate && !manipulating) CompleteNativeZoom();
            settleTimer.Stop(); if (!manipulating) settleTimer.Start();
        };
        ((UIElement)viewport.Content).AddHandler(UIElement.PointerWheelChangedEvent, new PointerEventHandler((_, e) =>
        {
            if (e.Handled) return;
            if ((Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & Windows.UI.Core.CoreVirtualKeyStates.Down) == 0) return;
            e.Handled = true;
            controller.Command(e.GetCurrentPoint(viewport).Properties.MouseWheelDelta > 0 ? 1 : -1);
        }), false);
        panel.Children.Add(controller.Toolbar); panel.Children.Add(viewport);
        return panel;
    }
}
