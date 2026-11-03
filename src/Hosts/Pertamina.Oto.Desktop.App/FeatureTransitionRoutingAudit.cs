#if DEBUG
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Agent 0 wires this into a fresh DEBUG audit process after all lanes integrate.
    // This method creates only synthetic files in a unique evidence subdirectory.
    internal async Task AuditFeatureTransitionRoutingAsync(string directory)
    {
        Directory.CreateDirectory(directory);
        var report = new List<string>();
        var fixtures = Path.Combine(directory, "routing-fixtures-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtures);
        var held = new List<FileStream>();
        var started = System.Diagnostics.Stopwatch.StartNew();
        void Check(bool condition, string label)
        {
            report.Add((condition ? "PASS " : "FAIL ") + label);
            File.WriteAllLines(Path.Combine(directory, "routing-audit.txt"), report);
            if (!condition) throw new InvalidOperationException(label);
        }
        ToolCard Tool(string id) => ToolCatalog.All.Single(t => t.Id == id);
        async Task Settle() { root.UpdateLayout(); await Task.Delay(100); root.UpdateLayout(); }
        async Task Home(params string[] incoming)
        {
            ShowHome(); await Settle();
            // Isolated fixture reset: no save, file deletion, clipboard, or library write.
            toolDrafts.Clear(); closedToolSessions.UnionWith(ToolCatalog.All.Select(t => t.Id));
            selected = null; activeToolControls = null; files.Clear();
            homeFiles.Clear(); homeFiles.AddRange(incoming); chosenFiles.Clear(); chosenFiles.UnionWith(incoming);
            if (incoming.Length > 0) BuildDocumentHome();
            await Settle();
        }
        async Task<ContentDialog> Popup(Task operation, ContentDialog? previous = null)
        {
            for (var attempt = 0; attempt < 1000; attempt++)
            {
                var dialog = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
                    .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>()
                    .FirstOrDefault(candidate => !ReferenceEquals(candidate, previous));
                if (dialog is not null) { await Settle(); return dialog; }
                if (operation.IsCompleted) { await operation; throw new InvalidOperationException("Expected routing dialog did not open."); }
                await Task.Delay(25);
            }
            throw new TimeoutException("Routing dialog timeout.");
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        static ToggleButton[] DocumentChoices(ContentDialog dialog) => Descendants(dialog).OfType<ToggleButton>()
            .Where(c => c.Name.StartsWith("FeatureTransitionDocument", StringComparison.Ordinal)).OrderBy(c => c.Name, StringComparer.Ordinal).ToArray();
        static void Confirm(ContentDialog dialog) => Invoke(Descendants(dialog).OfType<Button>().Single(b => b.Name == "FeatureTransitionPrimary"));
        try
        {
            Check(!FeatureTransitionBusy && !messageOpen, "audit starts idle in a dedicated DEBUG process");
            var a = Path.Combine(fixtures, "a.pdf");
            var b = Path.Combine(fixtures, "b.pdf");
            var corrupt = Path.Combine(fixtures, "corrupt.pdf");
            var absent = Path.Combine(fixtures, "missing.pdf");
            var image = Path.Combine(fixtures, "image.png");
            var spoof = Path.Combine(fixtures, "png-renamed.jpg");
            var docx = Path.Combine(fixtures, "document.docx");
            WriteHistoryAuditFixture(a); WriteHistoryAuditFixture(b);
            await File.WriteAllTextAsync(corrupt, "synthetic invalid PDF");
            using (var file = new FileStream(image, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var stream = file.AsRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, 2, 2, 96, 96, new byte[16]);
                await encoder.FlushAsync();
            }
            File.Copy(image, spoof);
            using (var zip = ZipFile.Open(docx, ZipArchiveMode.Create))
            {
                foreach (var item in new[]
                {
                    ("[Content_Types].xml", "<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'/>"),
                    ("_rels/.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'/>"),
                    ("word/document.xml", "<w:document xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main'><w:body/></w:document>")
                })
                { using var writer = new StreamWriter(zip.CreateEntry(item.Item1).Open(), Encoding.UTF8); writer.Write(item.Item2); }
            }
            var hashes = new[] { a, b, image, docx }.ToDictionary(p => p, p => SHA256.HashData(File.ReadAllBytes(p)));
            foreach (var tool in ToolCatalog.All.Where(t => t.Available))
            {
                var correct = tool.Id == "images" ? image : tool.Id == "word-pdf" ? docx : a;
                Check(FeatureTransitionCompatible(tool, correct), "format admits correct source for " + tool.Id);
                Check(!FeatureTransitionCompatible(tool, tool.Id == "images" ? a : image), "format rejects incompatible source for " + tool.Id);
            }
            Check(!FeatureTransitionCompatible(Tool("ocr"), a), "unavailable feature never admits processing input");
            Check(FeatureTransitionAllowsMultiple(Tool("merge")) && FeatureTransitionAllowsMultiple(Tool("organize"))
                && FeatureTransitionAllowsMultiple(Tool("images")) && !FeatureTransitionAllowsMultiple(Tool("word-pdf"))
                && !FeatureTransitionAllowsMultiple(Tool("split")), "multi and single capacities match actual engines");
            var metadata = await InspectFeatureTransitionInputsAsync(Tool("merge"), [a, corrupt, absent, image]);
            Check(metadata[0].Problem is null && metadata[0].Pages == 3, "real PDF metadata reads three pages without processing");
            Check(metadata.Skip(1).All(item => item.Problem is not null), "corrupt missing and incompatible sources retain individual reasons");
            Check(metadata[1].Problem != metadata[2].Problem && metadata[2].Problem != metadata[3].Problem, "failure classes are not one generic rejection");
            var photoMetadata = await InspectFeatureTransitionInputsAsync(Tool("images"), [image, spoof]);
            Check(photoMetadata[0].Problem is null && photoMetadata[1].Problem is not null, "image codec metadata detects renamed PNG as JPG");
            var wordMetadata = await InspectFeatureTransitionInputsAsync(Tool("word-pdf"), [docx]);
            Check(wordMetadata[0].Problem is null && wordMetadata[0].Pages is null
                && wordMetadata[0].Detail.Contains("validasi lengkap", StringComparison.Ordinal), "DOCX structural preflight makes no Word layout or page-count promise");
            var duplicates = await InspectFeatureTransitionInputsAsync(Tool("merge"), [a, a]);
            Check(duplicates.Length == 2 && duplicates[1].Problem is not null, "duplicate source is disclosed instead of silently discarded");
            var network = await InspectFeatureTransitionInputsAsync(Tool("merge"), [@"\\127.0.0.1\unreachable\source.pdf"]);
            Check(network[0].Problem?.Contains("lokal", StringComparison.Ordinal) == true, "UNC source refused before storage/PDF access");
            using (var locking = new FileStream(a, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var locked = await InspectFeatureTransitionInputsAsync(Tool("merge"), [a]);
                Check(locked[0].Problem?.Contains("dikunci", StringComparison.Ordinal) == true, "exclusive file lock has actionable per-file rejection");
            }
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                var stopped = await InspectFeatureTransitionInputAsync(Tool("merge"), a, false, cancelled.Token);
                Check(stopped.Item.Problem is not null && stopped.Lease is null, "cancelled metadata does not return ready source or retain handle");
            }
            var large = Path.Combine(fixtures, "over-limit.pdf");
            using (var file = new FileStream(large, FileMode.CreateNew, FileAccess.Write)) file.SetLength(FeatureTransitionMaximumBytes + 1);
            var sizeResult = await InspectFeatureTransitionInputsAsync(Tool("merge"), [large]);
            Check(sizeResult[0].Problem?.Contains("100 MB", StringComparison.Ordinal) == true, "100 MB source boundary rejected before PDF load");
            File.Delete(large); // Reproducible, owned zero-filled fixture; preserve the check, not 100 MB per retry.
            var manyPages = Path.Combine(fixtures, "over-page-limit.pdf");
            WriteFeatureTransitionPageFixture(manyPages, 1001);
            var pageResult = await InspectFeatureTransitionInputsAsync(Tool("merge"), [manyPages]);
            Check(pageResult[0].Problem?.Contains("1000", StringComparison.Ordinal) == true, "real 1001 page source rejected");
            Check(FeatureTransitionSelectionProblem([metadata[0] with { Bytes = FeatureTransitionMaximumBytes }, metadata[0]]) is not null,
                "100 MB total selection boundary enforced");
            Check(FeatureTransitionSelectionProblem([metadata[0] with { Pages = 1000 }, metadata[0]]) is not null,
                "1000 page total selection boundary enforced");
            var encrypted = Path.Combine(fixtures, "encrypted.pdf");
            var protectedReply = await WorkerClient.RunAsync(new PdfRequest("protect", [a], encrypted, NewPassword: "Synthetic-routing-only-42"), CancellationToken.None);
            Check(protectedReply.Success, "existing worker creates synthetic password fixture");
            var lockedPdf = await InspectFeatureTransitionInputsAsync(Tool("edit"), [encrypted]);
            var splitProtected = await InspectFeatureTransitionInputsAsync(Tool("split"), [encrypted]);
            var unlockPdf = await InspectFeatureTransitionInputsAsync(Tool("unlock"), [encrypted]);
            File.WriteAllText(Path.Combine(directory, "encrypted-classification.json"), System.Text.Json.JsonSerializer.Serialize(new { lockedPdf, splitProtected, unlockPdf }));
            Check(lockedPdf[0].Problem?.Contains("sandi", StringComparison.Ordinal) == true, "password PDF refuses editor which does not support protected input");
            Check(splitProtected[0].Problem is null && splitProtected[0].Warning is not null && FeatureTransitionNeedsSelection(splitProtected),
                "existing password-capable destination retains explicit deferred-validation path");
            Check(unlockPdf[0].Problem is null && unlockPdf[0].Warning is not null && FeatureTransitionNeedsSelection(unlockPdf),
                "Unlock requires explicit selection with unverified page-count disclosure");

            var offered = new[] { new TransitionDocument(a, "same.pdf", "", "original", 0), new TransitionDocument(b, "same.pdf", "", "original", 1) };
            Check(TryResolveFeatureTransitionSelection(offered, [b], false, out var one) && one.SequenceEqual([b]), "single choice returns full original path despite duplicate names");
            Check(TryResolveFeatureTransitionSelection(offered, [b, a], true, out var ordered) && ordered.SequenceEqual([a, b]), "multiple choice preserves source order");
            Check(!TryResolveFeatureTransitionSelection(offered, [], false, out _)
                && !TryResolveFeatureTransitionSelection(offered, [a, b], false, out _)
                && !TryResolveFeatureTransitionSelection(offered, [a, a], true, out _)
                && !TryResolveFeatureTransitionSelection(offered, [absent], false, out _), "empty duplicate excess and injected selections are rejected");
            var ready = await InspectFeatureTransitionInputsAsync(Tool("merge"), [a], held);
            var writerDenied = false;
            try { using var writer = new FileStream(a, FileMode.Open, FileAccess.Write, FileShare.ReadWrite); }
            catch (IOException) { writerDenied = true; }
            Check(ready[0].Problem is null && held.Count == 1 && writerDenied, "validated source read lease denies replacement throughout destination decision");
            held.ForEach(handle => handle.Dispose()); held.Clear();
            using (var writer = new FileStream(a, FileMode.Open, FileAccess.Write, FileShare.Read))
                Check(writer.CanWrite, "read lease released on completion");

            await Home();
            await OpenWorkspaceToolAsync(Tool("compress")).WaitAsync(TimeSpan.FromSeconds(30));
            Check(emptyTool?.Id == "compress" && !messageOpen, "zero documents opens empty destination without selection dialog");
            await Home();
            var draft = new ToolDraft([a], "", "", "synthetic-retained.pdf", 0, 20);
            toolDrafts["rotate"] = draft; closedToolSessions.Remove("rotate");
            await OpenWorkspaceToolAsync(Tool("rotate")).WaitAsync(TimeSpan.FromSeconds(30));
            Check(selected?.Id == "rotate" && files.SequenceEqual([a]) && outputName.Text == draft.Output,
                "zero documents resumes destination draft rather than replacing it");
            await Home(a);
            await OpenWorkspaceToolAsync(Tool("compress")).WaitAsync(TimeSpan.FromSeconds(30));
            Check(selected?.Id == "compress" && files.SequenceEqual([a]) && !messageOpen, "one ready compatible document opens directly");
            await Home(a);
            var existing = new ToolDraft([b], "1", "", "keep-existing-draft.pdf", 1, 20);
            toolDrafts["rotate"] = existing; closedToolSessions.Remove("rotate");
            var conflictRoute = OpenWorkspaceToolAsync(Tool("rotate")); var conflictDialog = await Popup(conflictRoute);
            Check(DocumentChoices(conflictDialog).Length == 0 && ReferenceEquals(toolDrafts["rotate"], existing),
                "single ready source still requires destination conflict decision");
            conflictDialog.Hide(); await conflictRoute.WaitAsync(TimeSpan.FromSeconds(30));
            Check(ReferenceEquals(toolDrafts["rotate"], existing) && homeFiles.SequenceEqual([a]) && chosenFiles.SetEquals([a]),
                "destination conflict cancellation preserves draft and selected source");
            await Home(a, b);
            var beforeHome = homeFiles.ToArray(); var beforeChosen = chosenFiles.ToArray();
            var route = OpenWorkspaceToolAsync(Tool("split"));
            var radioDialog = await Popup(route);
            var radio = DocumentChoices(radioDialog);
            Check(radio.Length == 2 && radio.All(r => r is RadioButton), "multi to single displays radio choices");
            Check(radio.All(r => r.IsChecked != true), "multi to single requires explicit choice without arbitrary first selection");
            radioDialog.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(homeFiles.SequenceEqual(beforeHome) && chosenFiles.SetEquals(beforeChosen) && !toolDrafts.ContainsKey("split"),
                "cancel leaves home selection and unopened destination unchanged");
            route = OpenWorkspaceToolAsync(Tool("split")); radioDialog = await Popup(route);
            DocumentChoices(radioDialog)[1].IsChecked = true; Confirm(radioDialog);
            await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(selected?.Id == "split" && files.SequenceEqual([b]), "explicit second radio selection reaches destination");
            await Home(a, b);
            route = OpenWorkspaceToolAsync(Tool("merge")); var checks = await Popup(route);
            var checkChoices = DocumentChoices(checks);
            Check(checkChoices.Length == 2 && checkChoices.All(c => c is CheckBox), "multi to multi displays checkboxes");
            foreach (var choice in checkChoices) choice.IsChecked = false;
            checkChoices[1].IsChecked = true; Confirm(checks); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(selected?.Id == "merge" && files.SequenceEqual([b]), "multi destination receives exact chosen subset");
            await Home(a, image);
            route = OpenWorkspaceToolAsync(Tool("split")); var mixed = await Popup(route);
            var mixedChoices = DocumentChoices(mixed);
            Check(mixedChoices.Length == 2 && mixedChoices[0].IsEnabled && !mixedChoices[1].IsEnabled,
                "mixed input retains incompatible row with rejection instead of silent dropping");
            mixed.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            await Home(a, b);
            var protectedDraft = new ToolDraft([a], "", "", "retained-after-failure.pdf", 0, 20);
            toolDrafts["compress"] = protectedDraft; closedToolSessions.Remove("compress");
            route = OpenWorkspaceToolAsync(Tool("compress")); var beforeDelete = await Popup(route);
            var moved = Path.Combine(fixtures, "temporarily-moved.pdf");
            File.Move(b, moved); // Synthetic owned fixture only; restore immediately after the negative path.
            try
            {
                DocumentChoices(beforeDelete)[1].IsChecked = true; Confirm(beforeDelete);
                var failure = await Popup(route, beforeDelete);
                Check(DocumentChoices(failure).All(c => !c.IsEnabled) && ReferenceEquals(toolDrafts["compress"], protectedDraft),
                    "selected source disappearing before commit cannot reset destination");
                failure.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
                Check(homeFiles.SequenceEqual([a, b]) && chosenFiles.SetEquals([a, b]), "revalidation failure preserves source selection");
            }
            finally { File.Move(moved, b); }
            await Home(a, b);
            route = OpenWorkspaceToolAsync(Tool("split")); var firstDialog = await Popup(route);
            await OpenWorkspaceToolAsync(Tool("images"));
            firstDialog.Hide(); var refusal = await Popup(route, firstDialog);
            Check(!Descendants(refusal).OfType<ToggleButton>().Any(c => c.Name.StartsWith("FeatureTransitionDocument", StringComparison.Ordinal)),
                "overlapping navigation is refused through action dialog");
            refusal.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(!featureTransitionRoutingActive && !messageOpen && homeFiles.SequenceEqual([a, b]), "overlap guard releases without changing selection");
            await Home(a);
            using (var busy = new CancellationTokenSource())
            {
                active = busy;
                route = OpenWorkspaceToolAsync(Tool("split")); var busyDialog = await Popup(route);
                Check(homeFiles.SequenceEqual([a]) && !toolDrafts.ContainsKey("split"), "busy refusal precedes source and destination mutation");
                busyDialog.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30)); active = null;
            }
            await OpenWorkspaceToolAsync(Tool("ocr")).WaitAsync(TimeSpan.FromSeconds(30));
            Check(emptyTool?.Id == "ocr" && !toolDrafts.ContainsKey("ocr"), "unavailable tool stays on existing unavailable page");
            // Integrator's additional checks exercise the real reader button callback and
            // source dialogs, not only the isolated lane methods.
            await Home(b);
            await OpenPdfReaderAsync(a, processed: true); await Settle();
            var readerOrigin = CaptureFeatureTransitionOrigin();
            Check(readerOrigin.Files.SequenceEqual([a]) && readerOrigin.SavedResult == a,
                "visible reader origin overrides stale home choice and retains saved-result identity");
            var readerHome = homeFiles.ToArray(); var readerChosen = chosenFiles.ToArray();
            toolDrafts["rotate"] = new([b], "", "", "reader-retained.pdf", 0, 20); closedToolSessions.Remove("rotate");
            route = pdfReader!.AuditTransitionUseToolAsync("rotate"); var readerDialog = await Popup(route);
            readerDialog.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(pdfReader.Visible && homeFiles.SequenceEqual(readerHome) && chosenFiles.SetEquals(readerChosen)
                && toolDrafts["rotate"].Files.SequenceEqual([b]), "reader cancel leaves selection, reader and destination draft unchanged");
            CaptureWorkspaceContext(); ShowCatalog(); await Settle();
            readerOrigin = CaptureFeatureTransitionOrigin();
            Check(readerOrigin.Files.SequenceEqual([a]) && readerOrigin.SavedResult == a,
                "reader result identity survives Other tools catalog entry");
            await Home(a);
            await OpenWorkspaceToolAsync(Tool("edit")).WaitAsync(TimeSpan.FromSeconds(30)); await Settle();
            await liveEditor!.PrepareFeatureTransitionAuditDraftAsync();
            var exactEditor = liveEditor; var exactInline = liveEditor.FeatureTransitionAuditInlineIdentity;
            var exactState = FeatureTransitionStamp("edit");
            route = OpenWorkspaceToolAsync(Tool("split")); var sourceDialog = await Popup(route);
            File.WriteAllText(Path.Combine(directory, "editor-dialog-state.json"), System.Text.Json.JsonSerializer.Serialize(new
            { Before = exactState.State, After = FeatureTransitionStamp("edit").State,
              SameSecret = exactState.Secret == FeatureTransitionStamp("edit").Secret,
              SameEditor = liveEditor == exactEditor, SameInline = liveEditor.FeatureTransitionAuditInlineIdentity == exactInline }));
            Check(FeatureTransitionStamp("edit") == exactState && liveEditor.FeatureTransitionAuditInlineIdentity == exactInline,
                "native source dialog opening preserves inline text and undo checkpoint");
            sourceDialog.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(liveEditor == exactEditor && FeatureTransitionStamp("edit") == exactState
                && liveEditor.FeatureTransitionAuditInlineIdentity == exactInline, "native source cancellation restores exact editor inline state");
            toolDrafts["split"] = new([b], "1", "", "keep-target.zip", 0, 20); closedToolSessions.Remove("split");
            route = OpenWorkspaceToolAsync(Tool("split")); sourceDialog = await Popup(route);
            Descendants(sourceDialog).OfType<RadioButton>().Single(c => c.Name == "FeatureTransitionAction0").IsChecked = true;
            Confirm(sourceDialog); var nextDialog = await Popup(route, sourceDialog);
            Check(FeatureTransitionStamp("edit") == exactState, "editor retained across source and destination decision boundary");
            nextDialog.Hide(); await route.WaitAsync(TimeSpan.FromSeconds(30));
            Check(liveEditor == exactEditor && FeatureTransitionStamp("edit") == exactState
                && liveEditor.FeatureTransitionAuditInlineIdentity == exactInline && toolDrafts["split"].Files.SequenceEqual([b]),
                "second-stage cancellation retains both editor source and split target");
            Check(hashes.All(pair => pair.Value.SequenceEqual(SHA256.HashData(File.ReadAllBytes(pair.Key)))), "synthetic original hashes unchanged by all routing/readiness paths");
            report.Add("NOT_RUN permission-revocation ACL fixture and physical drive removal require Agent 0 manual/environment audit.");
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(directory, "failure.txt"), error.ToString());
            report.Add("FAIL audit stopped: " + error.GetType().Name); Environment.ExitCode = 1;
        }
        finally
        {
            held.ForEach(handle => handle.Dispose());
            active = null;
            foreach (var popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot))
                foreach (var dialog in Descendants(popup.Child).Prepend(popup.Child).OfType<ContentDialog>()) dialog.Hide();
            report.Add("ELAPSED_MS " + started.ElapsedMilliseconds);
            File.WriteAllLines(Path.Combine(directory, "routing-audit.txt"), report);
        }
    }

    private sealed partial class PdfReaderView
    {
        internal Task AuditTransitionUseToolAsync(string id) => UseToolAsync(id);
    }

    private static void WriteFeatureTransitionPageFixture(string path, int count)
    {
        var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "" };
        var references = new List<string>();
        for (var page = 0; page < count; page++)
        {
            references.Add((objects.Count + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) + " 0 R");
            objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] >>");
        }
        objects[1] = "<< /Type /Pages /Kids [" + string.Join(' ', references) + "] /Count " + count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " >>";
        var pdf = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(pdf.Length); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }
        var xref = pdf.Length;
        pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
        pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        File.WriteAllText(path, pdf.ToString(), Encoding.ASCII);
    }
}
#endif
