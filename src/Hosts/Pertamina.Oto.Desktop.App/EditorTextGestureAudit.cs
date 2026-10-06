#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private async Task AuditTextGesturesAsync(Action<bool, string> check)
        {
            EndInlineText(); viewport.Focus(FocusState.Programmatic);
            var point = new Point(marks[0].X + 4, marks[0].Y + 8);
            lastTextTarget = null;
            check(!TextObjectPressed(0, point, 1000) && inlineText is null, "first text click does not enter typing");
            check(TextObjectPressed(0, point, 1001), "second text click enters inline editing");
            await Task.Delay(100);
            var input = inlineText!;
            check(input.SelectedText == "EDITOR" && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), input), "double click focuses inline input and selects word");
            check(InlineTextPressed(input, point, 1002), "third text click routed to select all");
            await Task.Delay(100);
            check(input.SelectedText == "EDITOR NEW TEXT UPDATED", "triple click selects entire added text");
            check(CountTextClick("different-object", point, 1003) == 1, "different object resets multi-click count");
            check(CountTextClick("different-object", new Point(point.X + 100, point.Y), 1004) == 1, "distant click resets multi-click count");
            check(CountTextClick("different-object", point, 1004 + GetDoubleClickTime() + 1) == 1, "expired Windows double-click interval resets count");
            check(FontShortcut((VirtualKey)221, true, false) && marks[0].Size == 15, "Ctrl+] increases font one point");
            check(FontShortcut((VirtualKey)219, true, false) && marks[0].Size == 14, "Ctrl+[ decreases font one point");
            check(FontShortcut((VirtualKey)190, true, true) && marks[0].Size == 16, "Ctrl+Shift+> advances font preset");
            check(FontShortcut((VirtualKey)188, true, true) && marks[0].Size == 14, "Ctrl+Shift+< reverses font preset");
            check(inlineText == input && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), input) && input.SelectedText == input.Text, "font shortcuts preserve input focus and text selection");
            check(!FontShortcut((VirtualKey)221, false, false), "unmodified punctuation is not a font shortcut");
            EndInlineText(); viewport.Focus(FocusState.Programmatic);
            check(HandleObjectKey(VirtualKey.F2), "F2 enters editing for selected text"); await Task.Delay(100);
            check(inlineText is not null && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), inlineText), "F2 places keyboard focus in text");
            EndInlineText(); viewport.Focus(FocusState.Programmatic);
            check(HandleObjectKey(VirtualKey.Enter), "Enter enters editing for selected text"); await Task.Delay(100);
            FontShortcut((VirtualKey)190, true, true); FontShortcut((VirtualKey)190, true, true);
            check(marks[0].Size == 18 && inlineText!.FontSize == 18, "font change reflected in canvas input before PDF export");
        }
    }
}
#endif
