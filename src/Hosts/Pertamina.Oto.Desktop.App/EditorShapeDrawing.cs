using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Pertamina.Oto.LocalPdf;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private bool drawingShape;
        private Point shapeOrigin;
        private EditorMark? shapePreview;

        private void BeginShape(Point point)
        {
            chosen = -1; shapeOrigin = ClampShapePoint(point); shapePreview = null; drawingShape = true;
            Refresh();
        }
        private Point ClampShapePoint(Point point) => new(Math.Clamp(point.X, 2, sheet.Width - 2), Math.Clamp(point.Y, 2, sheet.Height - 2));
        private void UpdateShape(Point point, bool constrain)
        {
            if (!drawingShape) return;
            var end = ClampShapePoint(point); var dx = end.X - shapeOrigin.X; var dy = end.Y - shapeOrigin.Y;
            if (constrain && shape != "line")
            {
                var side = Math.Min(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Min(dx < 0 ? shapeOrigin.X - 2 : sheet.Width - 2 - shapeOrigin.X, dy < 0 ? shapeOrigin.Y - 2 : sheet.Height - 2 - shapeOrigin.Y));
                end = new(shapeOrigin.X + (dx < 0 ? -side : side), shapeOrigin.Y + (dy < 0 ? -side : side));
                dx = end.X - shapeOrigin.X; dy = end.Y - shapeOrigin.Y;
            }
            // Ignore a click or incidental jitter. Preview is not part of the document/undo stack.
            if (shape == "line" ? Math.Sqrt(dx * dx + dy * dy) * viewport.ZoomFactor < 4 : Math.Min(Math.Abs(dx), Math.Abs(dy)) < 4)
            { shapePreview = null; Refresh(); return; }
            var x = Math.Clamp(Math.Min(shapeOrigin.X, end.X), 0, sheet.Width - 4);
            var y = Math.Clamp(Math.Min(shapeOrigin.Y, end.Y), 0, sheet.Height - 4);
            shapePreview = new(page, shape, x, y, Math.Max(4, Math.Abs(dx)), Math.Max(4, Math.Abs(dy)), Size: 2,
                Points: shape == "line" ? [new(shapeOrigin.X - x, shapeOrigin.Y - y), new(end.X - x, end.Y - y)] : null);
            Refresh();
        }
        private void FinishShape()
        {
            var completed = shapePreview; drawingShape = false; shapePreview = null;
            if (completed is not null) Add(completed); else Refresh();
        }
        private static Line ShapeLine(EditorMark mark) => mark.Points is { Length: 2 } points
            ? new Line { X1 = points[0].X, Y1 = points[0].Y, X2 = points[1].X, Y2 = points[1].Y }
            : new Line { X2 = mark.Width, Y2 = mark.Height };
        private void DrawShapePreview()
        {
            if (shapePreview is not { } preview) return;
            Shape visual = preview.Kind switch { "ellipse" => new Ellipse(), "line" => ShapeLine(preview), _ => new Rectangle() };
            visual.Width = preview.Width; visual.Height = preview.Height; visual.Stroke = Brush(8, 102, 232); visual.StrokeThickness = 2;
            visual.IsHitTestVisible = false; visual.Opacity = .75;
            Canvas.SetLeft(visual, preview.X); Canvas.SetTop(visual, preview.Y); sheet.Children.Add(visual);
        }
    }
}
