using Windows.Foundation;
using Pertamina.Oto.LocalPdf;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private string penColor = "#111827";
        private double penSize = 2;
        private Point StrokePoint(Point p)
        {
            var inset = penSize / 2 + 1;
            return new(Math.Clamp(p.X, inset, sheet.Width - inset), Math.Clamp(p.Y, inset, sheet.Height - inset));
        }
        private void FreehandProperties(int generation)
        {
            var colors = new Microsoft.UI.Xaml.Controls.ComboBox { Header = "Warna goresan", ItemsSource = new[] { "Hitam", "Biru", "Merah", "Hijau", "Jingga", "Ungu" }, SelectedIndex = Array.IndexOf(PenColors, penColor), HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch };
            colors.SelectionChanged += (_, _) => { if (generation == propertyGeneration && !Working && !drawing && colors.SelectedIndex >= 0) penColor = PenColors[colors.SelectedIndex]; };
            properties.Children.Add(colors);
            var width = new Microsoft.UI.Xaml.Controls.NumberBox { Header = "Ketebalan goresan (pt)", Minimum = 1, Maximum = 24, Value = penSize, SpinButtonPlacementMode = Microsoft.UI.Xaml.Controls.NumberBoxSpinButtonPlacementMode.Compact };
            width.ValueChanged += (_, e) => { if (generation == propertyGeneration && !Working && !drawing && double.IsFinite(e.NewValue)) penSize = Math.Clamp(e.NewValue, 1, 24); };
            properties.Children.Add(width);
            properties.Children.Add(HistoryText("Pengaturan berlaku untuk coretan berikutnya. Untuk mengubah coretan lama, gunakan Pilih lalu klik coretannya.", 12, muted: true));
        }
        private static readonly string[] PenColors = ["#111827", "#1668D4", "#DC2626", "#15803D", "#EA580C", "#7C3AED"];
        private void BeginFreehand(Point p) { stroke.Clear(); stroke.Add(StrokePoint(p)); drawing = true; }
        private void UpdateFreehand(Point p)
        {
            if (!drawing) return;
            p = StrokePoint(p);
            if (stroke.Count > 0 && Math.Abs(p.X - stroke[^1].X) + Math.Abs(p.Y - stroke[^1].Y) < .25) return;
            if (stroke.Count >= 300)
            {
                var simplified = stroke.Where((_, i) => i % 2 == 0 || i == stroke.Count - 1).ToArray();
                stroke.Clear(); stroke.AddRange(simplified);
            }
            stroke.Add(p); Refresh();
        }
        private void FinishFreehand()
        {
            if (!drawing) return;
            var points = stroke.ToArray(); drawing = false; stroke.Clear();
            // Commit BEFORE ReleasePointerCapture: capture-lost can synchronously cancel the gesture.
            if (points.Length > 1)
            {
                var padding = penSize / 2 + 1;
                var x = points.Min(p => p.X) - padding; var y = points.Min(p => p.Y) - padding;
                var w = points.Max(p => p.X) - x + padding; var h = points.Max(p => p.Y) - y + padding;
                Add(new(page, "pen", x, y, w, h, Size: penSize, Color: penColor, Points: points.Select(p => new EditorPoint(p.X - x, p.Y - y)).ToArray()));
                mode = "Gambar bebas"; chosen = -1; BuildProperties();
            }
            Refresh();
        }
    }
}
