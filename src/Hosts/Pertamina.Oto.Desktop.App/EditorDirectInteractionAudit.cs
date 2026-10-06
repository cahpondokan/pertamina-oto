#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pertamina.Oto.LocalPdf;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private async Task AuditFreehandAsync(Action<bool, string> check, string directory, string source, Func<string, Task> capture)
        {
            mode = "Gambar bebas";
            BuildProperties();
            var palette = properties.Children.OfType<Microsoft.UI.Xaml.Controls.ComboBox>().First(c => (string)c.Header == "Warna goresan");
            palette.SelectedIndex = 2;
            var thickness = properties.Children.OfType<Microsoft.UI.Xaml.Controls.NumberBox>().First();
            thickness.Value = 12;
            check(penColor == "#DC2626" && penSize == 12, "pencil palette and thickness work before drawing");
            var history = past.Count;
            BeginFreehand(new Point(40, 380)); UpdateFreehand(new Point(90, 410)); UpdateFreehand(new Point(140, 380));
            FinishFreehand(); CancelGesture(); // Same capture-lost callback as ReleasePointerCapture.
            check(marks.Count == 1 && marks[0].Kind == "pen" && marks[0].Points!.Length == 3, "released freehand survives capture-lost callback");
            check(mode == "Gambar bebas" && past.Count == history + 1, "freehand stays active and creates one undo step");
            var pen = marks[0];
            check(pen.Color == "#DC2626" && pen.Size == 12, "committed pencil retains chosen color and thickness");
            check(pen.Points!.All(p => p.X >= pen.Size / 2 && p.Y >= pen.Size / 2 && p.X <= pen.Width - pen.Size / 2 && p.Y <= pen.Height - pen.Size / 2), "stroke bounds include full outer thickness");
            check(sheet.Children.OfType<Microsoft.UI.Xaml.Controls.Grid>().Any(g => g.Children.OfType<Microsoft.UI.Xaml.Controls.Canvas>().Any(c => c.Children.OfType<Microsoft.UI.Xaml.Shapes.Polyline>().Any(p => double.IsNaN(p.Width) && double.IsNaN(p.Height)))), "pen visual is not clipped by explicit shape dimensions");
            var exported = await WorkerClient.RunAsync(new("edit", [source], System.IO.Path.Combine(directory, "freehand-release.pdf"), Marks: marks.ToArray()), CancellationToken.None);
            check(exported.Success, "pointer-generated freehand saves to PDF");
            await capture("freehand-color-bounds");
            Restore(false); check(marks.Count == 0, "one undo removes committed freehand");
            BeginFreehand(new Point(40, 380)); FinishFreehand(); CancelGesture(); check(marks.Count == 0, "stationary freehand click adds no object");
            BeginFreehand(new Point(40, 380)); UpdateFreehand(new Point(50, 390)); CancelGesture(); check(marks.Count == 0 && !drawing, "cancel discards uncommitted stroke");
            BeginFreehand(new Point(40, 380));
            for (var i = 0; i < 700; i++) UpdateFreehand(new Point(50 + i % 200, 380 + i % 40));
            UpdateFreehand(new Point(260, 420));
            check(stroke.Count <= 300 && stroke[^1] == new Point(260, 420), "long freehand retains latest endpoint within point budget");
            FinishFreehand(); CancelGesture(); check(marks.Count == 1, "long freehand persists on release");
            Restore(false); check(marks.Count == 0, "long stroke undo restores empty draft");
            penColor = "#111827"; penSize = 2;
        }
        private async Task AuditOriginalInlineAsync(Action<bool, string> check, string directory, Func<string, Task> capture)
        {
            var run = originalRuns.First(r => r.Text == "PERTAMINA OTO - EDITOR TEST");
            mode = "Edit teks asli"; lastTextTarget = null;
            var point = new Point(run.X + 3, run.Y + 8);
            await OriginalTextPressedAsync(run, point, 10000);
            check(inlineText is null && originalId == run.Id, "first original-text click selects run");
            await OriginalTextPressedAsync(run, point, 10001); await Task.Delay(150);
            var input = inlineText!;
            check(input is not null && inlineOriginalRun?.Id == run.Id && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), input), "double click starts focused original-text input on page");
            check(input!.SelectedText == "Zqx", "double click selects original word under cursor");
            check(InlineTextPressed(input, point, 10002), "original triple click is recognized"); await Task.Delay(100);
            check(input.SelectedText == "Zqx 789", "original triple click selects entire current run");
            input.SelectedText = "INLINE ORIGINAL 456";
            await Task.Delay(100); // Native TextBox dispatches TextChanged after the selection replacement.
            check(Dirty && originalInlineDraft!.Text == "INLINE ORIGINAL 456", "direct original typing tracks unsaved draft");
            Refresh(); check(inlineText == input && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), input), "redraw preserves original text input focus");
            check(!HandleObjectKey(Windows.System.VirtualKey.Delete), "Delete during original typing stays native text deletion");
            await capture("original-inline-typing");
            check(await CommitOriginalInlineAsync() && marks.Any(m => m.Text == "INLINE ORIGINAL 456"), "direct original typing applies to real PDF preview");
            var committedState = State;
            StartOriginalInline(run); await Task.Delay(100); inlineText!.Text = "Ω"; await Task.Delay(100);
            check(!await CommitOriginalInlineAsync() && State == committedState && originalInlineDraft?.Text == "Ω" && inlineText?.Text == "Ω", "rejected inline original text retained without changing PDF draft");
            CancelOriginalInline(); check(originalInlineDraft is null && State == committedState, "Escape cancels only pending original typing");
            StartOriginalInline(run); await Task.Delay(100); inlineText!.Text = "INLINE NAV 456"; await Task.Delay(100);
            owner.ShowHome(); owner.ShowLiveEditor(); await Task.Delay(150);
            check(originalInlineDraft?.Text == "INLINE NAV 456" && Dirty, "navigation away and back retains original typing draft");
            check(await CommitOriginalInlineAsync(), "restored original typing draft applies");
            StartOriginalInline(run); await Task.Delay(100); inlineText!.Text = "INLINE SAVED 789";
            await SaveAsync(directory);
            check(!Dirty && originalInlineDraft is null && lastResult is not null, "Save commits pending original typing before export");
            var verify = await WorkerClient.RunAsync(new("edit-text-check", [lastResult!], System.IO.Path.Combine(directory, "verify-inline-original.pdf"), Rotation: 0), CancellationToken.None);
            check(verify.Success && verify.TextRuns is { } verified && verified.Any(r => r.Text == "INLINE SAVED 789") && !verified.Any(r => r.Text == run.Text), "saved original inline replacement reopens without old text");
        }
    }
}
#endif
