#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Authored for Agent 0's isolated DEBUG process after all lanes are integrated.
    // Uses synthetic path strings only; does not open, write or process any PDF.
    internal async Task AuditFeatureTransitionDialogAsync(string directory)
    {
        Directory.CreateDirectory(directory);
        var report = new List<string>();
        var reportPath = Path.Combine(directory, "feature-transition-dialog-audit.txt");
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label); File.WriteAllLines(reportPath, report);
            if (!ok) throw new InvalidOperationException(label);
        }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        {
            yield return node;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child;
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        static void Select(TransitionChoiceView row) =>
            ((ISelectionItemProvider)new RadioButtonAutomationPeer((RadioButton)row.Choice).GetPattern(PatternInterface.SelectionItem)).Select();
        static void Toggle(CheckBox box) => ((IToggleProvider)new CheckBoxAutomationPeer(box).GetPattern(PatternInterface.Toggle)).Toggle();
        async Task Settle() { await Task.Delay(180); root.UpdateLayout(); }
        TransitionDialogView? opened = null;
        Task? pending = null;
        async Task<TransitionDialogView> Current(Task task)
        {
            pending = task;
            for (var i = 0; i < 100; i++)
            {
                if (task.IsFaulted) await task;
                if (opened is not null) { await Settle(); return opened; }
                await Task.Delay(25);
            }
            throw new InvalidOperationException("Feature transition dialog did not open.");
        }
        async Task Capture(TransitionDialogView view, string name)
        {
            await Settle();
            var panel = Walk(view.Dialog).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
            Check(panel.ActualWidth <= root.ActualWidth + 1 && panel.ActualHeight <= root.ActualHeight + 1,
                name + ": native modal stays inside the viewport");
            foreach (var button in new[] { view.Primary, view.Cancel, view.Close })
            {
                var bounds = button.TransformToVisual(panel).TransformBounds(new Windows.Foundation.Rect(0, 0, button.ActualWidth, button.ActualHeight));
                Check(button.ActualHeight >= 44 && button.ActualWidth >= 44 && bounds.Left >= -1 && bounds.Right <= panel.ActualWidth + 1
                    && bounds.Top >= -1 && bounds.Bottom <= panel.ActualHeight + 1, name + ": " + button.Name + " visible hit target");
                Check(AutomationProperties.GetName(button).Length > 0 && button.UseSystemFocusVisuals && button.IsTabStop,
                    name + ": " + button.Name + " named keyboard control");
            }
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(panel);
            var file = await (await StorageFolder.GetFolderFromPathAsync(Path.GetFullPath(directory)))
                .CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth,
                (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        var previousHook = auditFeatureTransitionOpened;
        var previousSize = AppWindow.Size;
        var previousInputs = files.ToArray(); var previousHome = homeFiles.ToArray(); var previousChosen = chosenFiles.ToArray();
        var previousPages = pageLayout.ToArray(); var previousSources = organizerSources.ToArray();
        var previousUndo = layoutUndo.Count; var previousRedo = layoutRedo.Count;
        var previousSelected = selected; var previousName = outputName.Text;
        var opener = Walk(root).OfType<Button>().FirstOrDefault(b => b.IsLoaded && b.IsEnabled && b.Visibility == Visibility.Visible
            && b.Focus(FocusState.Programmatic));
        var tool = ToolCatalog.All.Single(t => t.Id == "edit");
        var merge = ToolCatalog.All.Single(t => t.Id == "merge");
        var synthetic = Path.GetFullPath(Path.Combine(directory, "synthetic-paths"));
        TransitionDocument[] documents =
        [
            new(Path.Combine(synthetic, "first.pdf"), "Dokumen pertama.pdf", "PDF · 1 halaman", "A", 0),
            new(Path.Combine(synthetic, "second.pdf"), "Dokumen kedua.pdf", "PDF · 3 halaman", "B", 1),
            new(Path.Combine(synthetic, "third.pdf"), "Dokumen ketiga.pdf", "PDF · 12 halaman", "C", 2)
        ];
        try
        {
            Check(!messageOpen, "audit starts without a competing modal");
            auditFeatureTransitionOpened = view => opened = view;
            foreach (var feature in ToolCatalog.All.Where(t => t.Available))
                Check(File.Exists(Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", TransitionBibloAsset(feature.Id))),
                    "packaged existing Biblo for " + feature.Id);
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            foreach (var width in new[] { 1080, 660 })
            {
                var scale = root.XamlRoot.RasterizationScale;
                AppWindow.Resize(new Windows.Graphics.SizeInt32((int)Math.Ceiling(width * scale), (int)Math.Ceiling(850 * scale)));
                await Settle(); opener?.Focus(FocusState.Programmatic); opened = null;
                var task = ChooseTransitionDocumentsAsync(tool, documents, "Dari Atur halaman PDF",
                    "Yang dibuka adalah file sumber, bukan hasil susunan halaman.", false);
                var view = await Current(task);
                var focused = FocusManager.GetFocusedElement(root.XamlRoot);
                File.WriteAllText(Path.Combine(directory, "focus-" + width + ".txt"),
                    (focused?.GetType().Name ?? "null") + " " + (focused as FrameworkElement)?.Name);
                Check(messageOpen && (ReferenceEquals(focused, view.Cancel) || ReferenceEquals(focused, view.Close)),
                    width + ": modal guard and safe dismiss focus");
                Check(view.Dialog.DefaultButton == ContentDialogButton.None && view.Dialog.PrimaryButtonText.Length == 0
                    && view.Dialog.CloseButtonText.Length == 0 && view.Dialog.SecondaryButtonText.Length == 0,
                    width + ": no automatic primary or duplicate footer");
                Check(view.Choices.Count == 3 && view.Choices.All(r => r.Choice is RadioButton)
                    && view.Choices.All(r => r.Choice.IsChecked != true) && !view.Primary.IsEnabled, width + ": single document choice requires explicit selection");
                Check(view.Biblo.Source is BitmapImage { PixelWidth: > 0 } image && image.UriSource.LocalPath.EndsWith("018-sunting-pdf.png", StringComparison.Ordinal),
                    width + ": destination Biblo decoded");
                Check(view.Primary.Content is TextBlock && !Walk(view.Primary).OfType<Image>().Any(), width + ": text-only primary");
                var primaryColor = ((SolidColorBrush)view.Primary.Resources["ButtonBackground"]).Color;
                var cancelColor = ((SolidColorBrush)view.Cancel.Resources["ButtonBackground"]).Color;
                Check(view.Palette.HighContrast || primaryColor != cancelColor, width + ": explicit blue primary and neutral cancel");
                Check(view.Primary.Foreground is SolidColorBrush primaryText && primaryText.Color == view.Palette.OnBlue.Color
                    && view.Cancel.Foreground is SolidColorBrush cancelText && cancelText.Color == view.Palette.Ink.Color,
                    width + ": rendered button foregrounds resolve the explicit resources");
                Check(view.Search?.Visibility == Visibility.Collapsed, width + ": small selection stays compact");
                Check((await ChooseTransitionDocumentsAsync(tool, documents, "", "", false)) is null && messageOpen,
                    width + ": reentrant call leaves the displayed modal guard intact");
                Select(view.Choices[1]); await Settle();
                Check(view.Choices[1].Choice.IsChecked == true && view.Choices.Count(r => r.Choice.IsChecked == true) == 1,
                    width + ": radio callback maintains cardinality");
                await Capture(view, "documents-" + width);
                Invoke(view.Primary); var result = await task;
                Check(result is { Length: 1 } && result[0] == documents[1].Path && !messageOpen,
                    width + ": confirmation returns original full path after guard release");
            }

            var many = Enumerable.Range(0, 30).Select(i => new TransitionDocument(
                Path.Combine(synthetic, "folder-" + i, "shared.pdf"),
                i < 2 ? "Same name.pdf" : "Dokumen " + i + " dengan nama yang cukup panjang untuk menguji pembungkusan.pdf",
                "PDF · " + (i + 1) + " halaman", OrganizerSourceCode(i), i,
                i == 2 ? "File tidak ditemukan." : i == 3 ? "Jenis dokumen tidak didukung." : null)).ToArray();
            opened = null;
            var multiple = ChooseTransitionDocumentsAsync(merge, many, "Dari Beranda", "Pilih file sumber yang ingin digunakan.", true);
            var multi = await Current(multiple);
            Check(multi.Choices.All(r => r.Choice is CheckBox) && multi.Choices.Count(r => r.Choice.IsChecked == true) == 28,
                "multiple: compatible documents selected; problematic documents disabled");
            Check(multi.Choices[0].PathHint is not null && multi.Choices[1].PathHint is not null
                && multi.Choices[0].PathHint!.Text != multi.Choices[1].PathHint!.Text, "duplicate names retain distinct visible paths");
            Check(multi.Choices[26].BadgeText?.Text == "AA", "source badge uses stable organizer source index above Z");
            Check(multi.Search?.Visibility == Visibility.Visible && multi.Scroll.ScrollableHeight > 0, "many documents have search and native scroll");
            multi.Search!.Text = "folder-29"; await Settle();
            Check(multi.Choices.Count(r => r.Frame.Visibility == Visibility.Visible) == 1
                && multi.Summary.Text.Contains("27 di luar pencarian", StringComparison.Ordinal), "filter discloses retained hidden selections");
            Toggle((CheckBox)multi.Choices[29].Choice); await Settle();
            Check(multi.Summary.Text.StartsWith("27 dokumen", StringComparison.Ordinal), "checkbox callback changes only requested choice");
            Toggle(multi.SelectVisible!); await Settle();
            Check(multi.Choices[29].Choice.IsChecked == true && multi.Summary.Text.StartsWith("28 dokumen", StringComparison.Ordinal),
                "select-visible automation includes only the filtered eligible document");
            Toggle(multi.SelectVisible!); await Settle();
            Check(multi.Choices[29].Choice.IsChecked == false && multi.Summary.Text.StartsWith("27 dokumen", StringComparison.Ordinal),
                "deselect-visible automation retains hidden selections");
            multi.Search.Text = "no-match-query"; await Settle();
            Check(Walk(multi.Dialog).OfType<TextBlock>().Single(t => t.Name == "FeatureTransitionEmpty").Visibility == Visibility.Visible,
                "no-match search has visible recovery guidance");
            multi.Search.Text = ""; await Settle();
            await Capture(multi, "many-documents-660");
            multi.Scroll.ChangeView(null, multi.Scroll.ScrollableHeight, null); await Capture(multi, "many-documents-bottom-660");
            Invoke(multi.Primary); var manyResult = await multiple;
            Check(manyResult is { Length: 27 } && manyResult.SequenceEqual(many.Where((_, i) => i is not (2 or 3 or 29)).Select(d => d.Path)),
                "multiple returns original paths in input order, with no problem or unchecked path");

            foreach (var dismissal in new[] { "cancel", "close", "escape", "native" })
            {
                opener?.Focus(FocusState.Programmatic); opened = null;
                var task = ChooseTransitionDocumentsAsync(tool, documents, "Dari Beranda", "", false);
                var view = await Current(task);
                if (dismissal == "cancel") Invoke(view.Cancel);
                else if (dismissal == "close") Invoke(view.Close);
                else if (dismissal == "native") view.Dialog.Hide();
                else
                {
                    Select(view.Choices[0]); await Settle();
                    Check(view.KeyDown!(VirtualKey.Escape) && !task.IsCompleted && messageOpen, "Escape waits for key release");
                    Invoke(view.Primary); // Escape must win even over a concurrent confirmation callback.
                    Check(view.KeyUp!(VirtualKey.Escape), "Escape release is consumed");
                }
                Check(await task is null && !messageOpen, dismissal + ": null result and guard released");
                Check(opener is null || ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), opener), dismissal + ": previous focus restored");
            }

            foreach (var candidates in new[] { Array.Empty<TransitionDocument>(), documents.Select(d => d with { Problem = "Filenya tidak tersedia." }).ToArray() })
            {
                opened = null;
                var task = ChooseTransitionDocumentsAsync(tool, candidates, "Dari Beranda", "", false);
                var view = await Current(task);
                Check(!view.Primary.IsEnabled && view.Choices.All(r => r.Choice.IsChecked != true), "zero valid documents cannot be confirmed");
                Invoke(view.Cancel); Check(await task is null, "zero valid documents remains cancellable");
            }

            TransitionAction[] actions =
            [
                new("original", "Buka file sumber", "Gunakan dokumen asli tanpa hasil susunan halaman."),
                new("save", "Tinjau dan simpan hasil", "Kembali ke fitur asal untuk menyimpan susunan sebagai PDF baru."),
                new("replace", "Ganti draft tujuan", "Draft yang terbuka di fitur tujuan akan diganti.", true)
            ];
            opened = null;
            var actionTask = ChooseTransitionActionAsync(tool, "Buka di Edit PDF", "Pilih dokumen atau pekerjaan yang ingin dilanjutkan.", actions,
                "Hasil susunan halaman belum disimpan.");
            var actionView = await Current(actionTask);
            Check(!actionView.Primary.IsEnabled && actionView.Choices.All(r => r.Choice.IsChecked != true), "actions require explicit choice");
            Select(actionView.Choices[2]); await Settle();
            Check(!actionView.Primary.IsEnabled && actionView.Acknowledgment?.Visibility == Visibility.Visible, "destructive action requires separate acknowledgment");
            var consentBounds = actionView.Acknowledgment!.TransformToVisual(actionView.Scroll)
                .TransformBounds(new Windows.Foundation.Rect(0, 0, actionView.Acknowledgment.ActualWidth, actionView.Acknowledgment.ActualHeight));
            Check(consentBounds.Top >= -1 && consentBounds.Bottom <= actionView.Scroll.ActualHeight + 1,
                "required destructive consent is revealed inside the narrow scroll viewport");
            Toggle(actionView.Acknowledgment!); Check(actionView.Primary.IsEnabled, "acknowledgment enables explicit destructive confirmation");
            actionView.PrimeOpeningKey!(VirtualKey.Enter);
            Check(actionView.KeyDown!(VirtualKey.Enter), "opening Enter is suppressed");
            Invoke(actionView.Primary); Check(!actionTask.IsCompleted && messageOpen, "opening-key carryover cannot accept destructive action");
            Check(actionView.KeyUp!(VirtualKey.Enter) && !actionTask.IsCompleted, "opening Enter release does not accept selection");
            Select(actionView.Choices[0]); Select(actionView.Choices[2]); await Settle();
            Check(!actionView.Primary.IsEnabled && actionView.Acknowledgment!.IsChecked != true, "changing action resets acknowledgment");
            await Capture(actionView, "draft-replacement-660");
            Toggle(actionView.Acknowledgment!);
            Invoke(actionView.Primary);
            Check(await actionTask == "replace" && !messageOpen, "destructive choice returns only intent and releases guard");

            opened = null;
            var safeTask = ChooseTransitionActionAsync(tool, "Buka di Edit PDF", "Pilih sumber atau hasil yang sudah disimpan.", actions[..2]);
            var safeView = await Current(safeTask);
            Select(safeView.Choices[1]);
            // Palette simulation exercises authored high-contrast branches without changing Windows settings.
            safeView.Palette.Update(true); safeView.Refresh?.Invoke();
            Check(safeView.Palette.Surface.Color != safeView.Palette.Ink.Color, "simulated high contrast separates surface and text");
            await Capture(safeView, "action-palette-high-contrast-660");
            Invoke(safeView.Primary); Check(await safeTask == "save" && !messageOpen, "safe action callback returns exact opaque identifier");

            opened = null;
            var busyTask = ChooseTransitionActionAsync(tool, "Proses masih berjalan", "Tunggu proses selesai sebelum berpindah fitur.",
                [new("stay", "Kembali ke proses", "Lanjutkan pekerjaan yang sedang berjalan.")]);
            var busyView = await Current(busyTask); await Capture(busyView, "busy-660");
            Invoke(busyView.Cancel); Check(await busyTask is null && !messageOpen, "busy action modal safely cancels");

            var buildFailed = false;
            try { await ChooseTransitionDocumentsAsync(tool, null!, "", "", false); }
            catch (ArgumentNullException) { buildFailed = true; }
            Check(buildFailed && !messageOpen, "construction failure releases guard");
            Check(files.SequenceEqual(previousInputs) && homeFiles.SequenceEqual(previousHome) && chosenFiles.SetEquals(previousChosen)
                && pageLayout.SequenceEqual(previousPages) && organizerSources.SequenceEqual(previousSources)
                && layoutUndo.Count == previousUndo && layoutRedo.Count == previousRedo && ReferenceEquals(selected, previousSelected)
                && outputName.Text == previousName, "all dialog paths preserve source and destination session state");
            report.Add("LIMIT Authored callback simulations are not physical-key, screen-reader, OS high-contrast-toggle, alternate text-scale or multiple-monitor UAT.");
            report.Add("LIMIT This audit only covers UI intent; routing/session integration and file validation require their own audits.");
        }
        catch (Exception error)
        {
            report.Add("FAIL audit terminated: " + error.GetType().Name); Environment.ExitCode = 1;
            throw;
        }
        finally
        {
            opened?.Dialog.Hide();
            if (pending is not null) try { await pending; } catch (Exception) { /* Failure is recorded above. */ }
            auditFeatureTransitionOpened = previousHook;
            AppWindow.Resize(previousSize);
            File.WriteAllLines(reportPath, report);
        }
    }
}
#endif
