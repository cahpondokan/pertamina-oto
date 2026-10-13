using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Point organizerGrabOffset;

    private void AttachOrganizerDrag(OrganizerCard card)
    {
        card.Root.PointerPressed += (_, e) =>
        {
            if (active is not null || importing || organizerHeld is not null || !e.GetCurrentPoint(card.Root).Properties.IsLeftButtonPressed) return;
            for (var node = e.OriginalSource as DependencyObject; node is not null && node != card.Root; node = VisualTreeHelper.GetParent(node))
                if (node is ButtonBase) return;
            card.Root.Focus(FocusState.Pointer);
            organizerPress = e.GetCurrentPoint(root).Position; organizerPointerPosition = organizerPress;
            organizerGrabOffset = e.GetCurrentPoint(card.Root).Position;
            if (!card.Root.CapturePointer(e.Pointer)) return;
            organizerHeld = card; organizerPointer = e.Pointer; e.Handled = true;
        };
        card.Root.PointerMoved += (_, e) =>
        {
            if (organizerHeld != card) return;
            var point = e.GetCurrentPoint(root).Position;
            if (organizerDrag is null && Math.Abs(point.X - organizerPress.X) + Math.Abs(point.Y - organizerPress.Y) < 8) return;
            if (organizerDrag is null) BeginOrganizerDrag(card);
            PreviewOrganizerDrag(point); e.Handled = true;
        };
        card.Root.PointerReleased += (_, e) =>
        {
            if (organizerHeld != card) return;
            if (organizerDrag is not null) PreviewOrganizerDrag(e.GetCurrentPoint(root).Position);
            FinishOrganizerDrag(organizerValidDrop); e.Handled = true;
        };
        card.Root.PointerCanceled += (_, _) => CancelOrganizerDrag();
        card.Root.PointerCaptureLost += (_, _) => { if (organizerHeld == card) CancelOrganizerDrag(); };
    }

    private void BeginOrganizerDrag(OrganizerCard card)
    {
        organizerDrag = new OrganizerDragOrder(pageLayout, card.Page.Id);
        var ghostImage = new Image { Source = card.Picture.Source, Width = 154 * previewScale, Height = 194 * previewScale, Stretch = Stretch.Uniform, RenderTransformOrigin = new Point(.5, .5), RenderTransform = new RotateTransform { Angle = card.Page.Rotation } };
        var ghostContent = new Grid(); ghostContent.Children.Add(ghostImage);
        if (card.Page.Source < 0) ghostContent.Children.Add(new Border { Width = 130, Height = 184, BorderBrush = Brush(205, 213, 225), BorderThickness = new Thickness(1), Background = Brush(255, 255, 255), RenderTransformOrigin = new Point(.5, .5), RenderTransform = new RotateTransform { Angle = card.Page.Rotation } });
        organizerGhost = new Border { Width = OrganizerCardWidth * previewScale, Height = OrganizerCardHeight * previewScale, Padding = new Thickness(12), Child = ghostContent, Opacity = .65,
            Background = Brush(255, 255, 255), BorderBrush = card.Frame.BorderBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(8), IsHitTestVisible = false };
        Canvas.SetZIndex(organizerGhost, 100); organizerBoard.Children.Add(organizerGhost);
        card.Root.Opacity = 0; organizerPlaceholder.Visibility = Visibility.Visible;
        foreach (var item in organizerCards.Values) RevealOrganizerActions(item);
        ArrangeOrganizerCards(pageLayout, false); run.IsEnabled = false; organizerScrollTimer.Start();
    }

    private void PreviewOrganizerDrag(Point rootPoint)
    {
        if (organizerDrag is null || organizerGhost is null) return;
        organizerPointerPosition = rootPoint;
        var point = root.TransformToVisual(organizerBoard).TransformPoint(rootPoint);
        Canvas.SetLeft(organizerGhost, point.X - organizerGrabOffset.X); Canvas.SetTop(organizerGhost, point.Y - organizerGrabOffset.Y);
        var viewport = root.TransformToVisual(pageGrid).TransformPoint(rootPoint);
        organizerValidDrop = viewport.X >= 0 && viewport.X <= pageGrid.ActualWidth && viewport.Y >= 0 && viewport.Y <= pageGrid.ActualHeight;
        if (!organizerValidDrop) { layoutStatus.Text = "Di luar area halaman · Lepaskan atau Escape untuk membatalkan"; return; }
        var column = Math.Clamp((int)Math.Floor((point.X - 14) / OrganizerPitchX), 0, organizerColumns - 1);
        var row = Math.Max(0, (int)Math.Floor(point.Y / OrganizerPitchY));
        var target = Math.Clamp(row * organizerColumns + column, 0, pageLayout.Count - 1);
        if (target != organizerDrag.Target) ArrangeOrganizerCards(organizerDrag.Preview(target), true);
        var placeholder = OrganizerSlot(target); Canvas.SetLeft(organizerPlaceholder, placeholder.X); Canvas.SetTop(organizerPlaceholder, placeholder.Y);
        layoutStatus.Text = $"Pindah ke urutan {target + 1} · Lepaskan untuk menetapkan · Escape untuk batal";
    }

    private void FinishOrganizerDrag(bool commit)
    {
        var card = organizerHeld; var transaction = organizerDrag; var pointer = organizerPointer;
        organizerHeld = null; organizerDrag = null; organizerPointer = null; organizerValidDrop = false; organizerScrollTimer.Stop();
        // Clear state before releasing capture: CaptureLost must not undo a completed drop.
        if (pointer is not null) card?.Root.ReleasePointerCapture(pointer);
        if (organizerGhost is not null) { organizerBoard.Children.Remove(organizerGhost); organizerGhost = null; }
        organizerPlaceholder.Visibility = Visibility.Collapsed;
        if (transaction is not null && commit)
        {
            var order = transaction.Preview(transaction.Target);
            if (!order.Select(p => p.Id).SequenceEqual(pageLayout.Select(p => p.Id)))
            {
                SaveLayout();
                // Mutate the model only. Existing visual children and bitmap objects stay attached.
                for (var i = 0; i < order.Length; i++) pageLayout[i] = order[i];
            }
            if (card is not null) { card.X = double.NaN; MoveOrganizerVisual(card, OrganizerSlot(transaction.Target), false); }
        }
        if (card is not null) { card.Root.Opacity = 1; RevealOrganizerActions(card); }
        if (transaction is not null) { ArrangeOrganizerCards(pageLayout, true); UpdateOrganizerSummary(); }
    }

    private void CancelOrganizerDrag()
    {
        if (organizerHeld is not null || organizerDrag is not null) FinishOrganizerDrag(false);
    }
}
