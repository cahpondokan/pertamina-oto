using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static void DetachOwned(UIElement tree, UIElement target)
    {
        if (tree is Panel panel)
        {
            if (panel.Children.Remove(target)) return;
            foreach (var child in panel.Children.ToArray()) DetachOwned(child, target);
        }
        else if (tree is Border border && border.Child is { } child)
        {
            if (child == target) border.Child = null; else DetachOwned(child, target);
        }
        else if (tree is ContentControl content && content.Content is UIElement element)
        {
            if (element == target) content.Content = null; else DetachOwned(element, target);
        }
    }

    private void DetachToolControls()
    {
        foreach (var control in new UIElement[] { pages, password, newPassword, outputName, watermark, margin, rotation, fileList, run, cancel, openResult, progress, status, location })
        {
            DetachOwned(toolWorkspace, control); DetachOwned(toolOptions, control);
        }
        DetachOwned(toolWorkspace, toolOptions);
    }
    private void CaptureWorkspaceContext()
    {
        if (selected is null || !toolWorkspace.IsLoaded) return;
        var current = organizerVisible
            ? pageLayout.Where(p => p.Source >= 0).Select(p => organizerSources[p.Source]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            : files.ToArray();
        foreach (var path in current) if (!homeFiles.Contains(path, StringComparer.OrdinalIgnoreCase)) homeFiles.Add(path);
        chosenFiles.Clear(); chosenFiles.UnionWith(current);
    }

    private async Task OpenWorkspaceToolAsync(ToolCard tool)
    {
        if (active is not null || importing || messageOpen) return;
        try
        {
            CaptureWorkspaceContext(); SaveToolDraft();
            if (!tool.Available) { ShowEmptyTool(tool); return; }
            var inputs = homeFiles.Where(chosenFiles.Contains).ToArray();
            var images = tool.Id == "images";
            bool Compatible(string p) => images ? new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(p).ToLowerInvariant()) : Path.GetExtension(p).Equals(tool.Id == "word-pdf" ? ".docx" : ".pdf", StringComparison.OrdinalIgnoreCase);
            if (inputs.Any(p => !Compatible(p)))
            {
                await MenuMessageAsync("Jenis dokumen tidak sesuai", images ? "Pilih JPG atau PNG di Beranda untuk alat ini. Dokumen Anda tetap tersedia." : "Pilih dokumen PDF di Beranda untuk alat ini. Dokumen Anda tetap tersedia.");
                return;
            }
            if (inputs.Length > 1 && tool.Id is not ("merge" or "images" or "organize"))
            {
                var choice = new ComboBox { ItemsSource = inputs.Select(Path.GetFileName).ToArray(), SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
                var content = new StackPanel { Spacing = 12 }; content.Children.Add(Copy("Alat ini memakai satu PDF. Pilih dokumen; file lain tetap tersedia di Beranda.")); content.Children.Add(choice);
                messageOpen = true;
                try
                {
                    var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Pilih dokumen", Content = content, PrimaryButtonText = "Gunakan dokumen", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close };
                    if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
                    inputs = [inputs[choice.SelectedIndex]];
                }
                finally { messageOpen = false; }
            }
            if (tool.Id == "organize") { ShowOrganizer(); return; }
            if (tool.Id == "edit")
            {
                if (inputs.Length == 1) await OpenEditorDocumentAsync(inputs[0]);
                else ShowLiveEditor();
                return;
            }
            openingEmptyInput = inputs.Length > 0;
            try
            {
                ShowTool(tool);
                if (inputs.Length > 0 && !files.SequenceEqual(inputs, StringComparer.OrdinalIgnoreCase))
                {
                    files.Clear(); foreach (var path in inputs) files.Add(path);
                }
                if (inputs.Length > 0) status.Text = $"{inputs.Length} dokumen dipakai. File asli tidak berubah.";
            }
            finally { openingEmptyInput = false; }
        }
        catch (Exception error) { LocalDiagnostics.Record("open-workspace-tool", error); await MenuMessageAsync("Alat belum dapat dibuka", "Dokumen di komputer tetap aman. Coba buka kembali dari Beranda."); }
    }
}

