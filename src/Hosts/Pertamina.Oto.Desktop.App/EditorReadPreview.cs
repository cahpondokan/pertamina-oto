using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class LiveEditor
    {
        private bool readingPreview;
        private bool ReadPreviewFocus()
        {
            if (transitionRetainInput || readingPreview) return true;
            return View.XamlRoot is not null && FocusManager.GetFocusedElement(View.XamlRoot) is Button button
                && AutomationProperties.GetName(button) is "Baca sumber" or "Lihat hasil" or "Preview hasil" or "Tutup dokumen";
        }

        internal Func<Task> PauseForReadPreview()
        {
            if (!Visible) return () => Task.CompletedTask;
            readingPreview = true;
            var input = inlineText; var start = input?.SelectionStart ?? 0; var length = input?.SelectionLength ?? 0;
            return async () =>
            {
                await Task.Yield();
                if (Visible && input is not null && inlineText == input && input.IsLoaded)
                {
                    input.Focus(FocusState.Programmatic); await Task.Yield(); input.Select(start, length);
                }
                await Task.Yield();
                readingPreview = false;
            };
        }
    }
}
