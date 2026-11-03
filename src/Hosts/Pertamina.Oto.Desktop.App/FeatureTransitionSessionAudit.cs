#if DEBUG
using System.Security.Cryptography;
using Microsoft.UI.Xaml;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Test decisions exercise the real session methods; visual/keyboard dialogs have their own lane audit.
    private Func<IReadOnlyList<TransitionAction>, Task<string?>>? featureTransitionActionAudit;
    private Action? featureTransitionBeforeCommitAudit;
    private Action? featureTransitionAfterMutationAudit;

    internal async Task AuditFeatureTransitionSessionAsync(string directory)
    {
        // Agent 0 must invoke this only in a fresh, isolated DEBUG audit process after integration.
        if (FeatureTransitionBusy || liveEditor?.HasDocument == true || files.Count > 0 ||
            toolDrafts.Count > 0 || organizerSources.Count > 0)
            throw new InvalidOperationException("Session audit requires a fresh empty window.");
        Directory.CreateDirectory(directory);
        var report = new List<string>();
        void Check(bool condition, string name)
        {
            report.Add((condition ? "PASS " : "FAIL ") + name);
            if (!condition) throw new InvalidOperationException(name);
        }
        async Task Settle()
        {
            for (var i = 0; i < 100; i++)
            {
                root.UpdateLayout(); await Task.Delay(30);
                if (!FeatureTransitionBusy && i > 5) return;
            }
            throw new InvalidOperationException("Session audit did not become idle.");
        }
        void Choose(string? id) => featureTransitionActionAudit = actions =>
        {
            Check(id is null || actions.Any(a => a.Id == id), "requested session decision is offered");
            return Task.FromResult(id);
        };
        async Task ExpectFailure(Func<Task> operation, string label)
        {
            var failed = false;
            try { await operation(); }
            catch (IOException) { failed = true; }
            catch (System.Runtime.InteropServices.COMException error) when (error.HResult == unchecked((int)0x80048014)) { failed = true; }
            catch (InvalidOperationException) { failed = true; }
            Check(failed, label);
        }
        ToolCard Tool(string id) => ToolCatalog.All.Single(t => t.Id == id);
        var a = Path.Combine(directory, "synthetic-session-a.pdf");
        var b = Path.Combine(directory, "synthetic-session-b.pdf");
        var saved = Path.Combine(directory, "synthetic-composed-result.pdf");
        var corrupt = Path.Combine(directory, "synthetic-invalid.pdf");
        // Native text editing intentionally rejects fonts without an explicit supported encoding.
        // Use an admitted synthetic source; do not weaken that production safety check.
        WriteHistoryAuditFixture(a, editableText: true); WriteHistoryAuditFixture(b, 420, 595); WriteHistoryAuditFixture(saved);
        File.WriteAllText(corrupt, "Synthetic invalid PDF for transition failure testing.");
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        var initialHashes = new[] { Hash(a), Hash(b), Hash(saved), Hash(corrupt) };
        var restore = SnapshotFeatureTransitionMemory();
        try
        {
            Choose(null);
            var empty = CaptureFeatureTransitionOrigin();
            Check(empty.Files.Length == 0 && !empty.HasPendingChanges, "zero inputs produce an empty origin");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("merge"), []), "zero inputs may open a new empty destination");
            await OpenFeatureTransitionDestinationAsync(Tool("merge"), []);
            Check(emptyTool?.Id == "merge" && files.Count == 0, "zero inputs open the existing empty-state UI");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("merge"), [a, b]), "new multi-input destination can be prepared");
            await OpenFeatureTransitionDestinationAsync(Tool("merge"), [a, b]); await Settle();
            Check(files.SequenceEqual(new[] { a, b }), "multiple inputs preserve explicit order");
            outputName.Text = "synthetic-retained-merge.pdf"; mergeAngles[a] = 90;
            password.Password = "synthetic-memory-only";
            var source = CaptureFeatureTransitionOrigin();
            Check(source.ToolId == "merge" && source.HasPendingChanges && source.Files.SequenceEqual(new[] { a, b }),
                "multi-source rotation/output/password state is pending work");
            var sourceStamp = FeatureTransitionStamp("merge");
            Choose(null);
            Check(await ResolveFeatureTransitionOriginAsync(Tool("rotate"), source) is null && FeatureTransitionStamp("merge") == sourceStamp,
                "source cancel preserves complete draft and in-memory secret");
            Choose("original");
            Check((await ResolveFeatureTransitionOriginAsync(Tool("rotate"), source))!.SequenceEqual(new[] { a, b }),
                "source original decision returns originals without applying composition");
            Choose("review");
            Check(await ResolveFeatureTransitionOriginAsync(Tool("rotate"), source) is null && selected?.Id == "merge" &&
                status.Text.Contains("lagi", StringComparison.Ordinal), "source review stops transfer and explains explicit retry after save");
            Check(FeatureTransitionStamp("merge") == sourceStamp, "review does not process/save or alter pending source work");

            ShowHome(); await Settle();
            var retained = CaptureFeatureTransitionOrigin();
            Check(retained.ToolId == "merge" && retained.HasPendingChanges, "source context survives home/catalog navigation");
            homeFiles.Clear(); homeFiles.Add(b); chosenFiles.Clear(); chosenFiles.Add(b);
            var freshHome = CaptureFeatureTransitionOrigin();
            Check(freshHome.ToolId is null && freshHome.Files.SequenceEqual(new[] { b }), "fresh home selection overrides stale source context");
            Choose(null);
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("merge"), [b]) && FeatureTransitionStamp("merge") == sourceStamp,
                "dirty target cancellation retains draft/order/rotation/password");
            Choose("review");
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("merge"), [b]) && selected?.Id == "merge",
                "dirty target review resumes without replacement");
            await Settle();
            Check(FeatureTransitionStamp("merge") == sourceStamp, "resumed dirty target preserves exact state");
            Choose("replace");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("merge"), [b]), "explicit dirty-target discard grants a replacement decision");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("merge"), [a]), "decision cannot be replayed for different inputs");
            Check(FeatureTransitionStamp("merge") == sourceStamp, "mismatched decision preserves target");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("merge"), [b]), "replacement decision may be retried explicitly");
            featureTransitionAfterMutationAudit = () => throw new IOException("Synthetic post-mutation rollback.");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("merge"), [b]), "post-mutation fault propagates to routing");
            featureTransitionAfterMutationAudit = null;
            Check(FeatureTransitionStamp("merge") == sourceStamp && password.Password == "synthetic-memory-only",
                "rollback restores destination data and secret checkpoint");

            var clean = new FeatureTransitionOrigin("merge", "Gabungkan PDF", [a, b], false, saved);
            Choose("result");
            Check((await ResolveFeatureTransitionOriginAsync(Tool("rotate"), clean))!.SequenceEqual(new[] { saved }),
                "saved composed result is explicit and distinct from original inputs");
            Choose("original");
            Check((await ResolveFeatureTransitionOriginAsync(Tool("rotate"), clean))!.SequenceEqual(new[] { a, b }),
                "clean saved session can still choose original inputs");
            Choose(null);
            Check(await ResolveFeatureTransitionOriginAsync(Tool("rotate"), clean) is null, "saved-result selection may be cancelled");

            Check(await PrepareFeatureTransitionDestinationAsync(Tool("organize"), [a, b]), "organizer destination stages multiple files");
            await OpenFeatureTransitionDestinationAsync(Tool("organize"), [a, b]); await Settle();
            SaveLayout(); pageLayout.Reverse(); pageLayout[0] = pageLayout[0] with { Rotation = 90 }; chosenPages.Add(0);
            password.Password = "synthetic-organizer-secret";
            var organizerOrigin = CaptureFeatureTransitionOrigin(); var organizerStamp = FeatureTransitionStamp("organize");
            ShowHome(); await Settle();
            homeFiles.Add(saved); chosenFiles.Clear(); chosenFiles.Add(saved);
            Choose("resume");
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("organize"), [saved]), "resume existing organizer handles transition");
            await Settle();
            File.WriteAllText(Path.Combine(directory, "organizer-resume-state.json"), System.Text.Json.JsonSerializer.Serialize(new
            { Before = organizerStamp.State, After = FeatureTransitionStamp("organize").State,
              SameSecret = organizerStamp.Secret == FeatureTransitionStamp("organize").Secret,
              AddedIncoming = organizerSources.Contains(saved) }));
            Check(FeatureTransitionStamp("organize") == organizerStamp && !organizerSources.Contains(saved),
                "organizer resume never appends incoming home files; IDs/rotation/undo/selection/password retained");
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("organize"), []), "zero inputs resume organizer without replacement dialog");
            Choose("replace");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("organize"), [a, corrupt]), "organizer replacement decision is explicit");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("organize"), [a, corrupt]), "unreadable new organizer source fails before commit");
            Check(FeatureTransitionStamp("organize") == organizerStamp && organizerOrigin.HasPendingChanges,
                "failed organizer open retains complete old composition and undo history");

            Check(await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [a]), "single input prepares editor");
            await OpenFeatureTransitionDestinationAsync(Tool("edit"), [a]); await Settle();
            var oldEditor = liveEditor!;
            await oldEditor.PrepareFeatureTransitionAuditDraftAsync();
            var editorStamp = FeatureTransitionStamp("edit"); var inline = oldEditor.FeatureTransitionAuditInlineIdentity;
            Choose(null);
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [b]) &&
                FeatureTransitionStamp("edit") == editorStamp && oldEditor.FeatureTransitionAuditInlineIdentity == inline,
                "cancel keeps exact editor inline control, draft and undo");
            Choose("replace");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [corrupt]), "dirty editor replacement requires explicit decision");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("edit"), [corrupt]), "invalid editor candidate fails without replacing old editor");
            File.WriteAllText(Path.Combine(directory, "editor-failed-open-state.json"), System.Text.Json.JsonSerializer.Serialize(new
            { Before = editorStamp.State, After = FeatureTransitionStamp("edit").State, SameSecret = editorStamp.Secret == FeatureTransitionStamp("edit").Secret,
              SameEditor = liveEditor == oldEditor, SameInline = oldEditor.FeatureTransitionAuditInlineIdentity == inline }));
            Check(liveEditor == oldEditor && FeatureTransitionStamp("edit") == editorStamp && oldEditor.FeatureTransitionAuditInlineIdentity == inline,
                "failed editor open retains exact editor object, inline, undo and source lock");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [b]), "editor decision can be retried");
            featureTransitionBeforeCommitAudit = () => throw new IOException("Synthetic cancellation before commit.");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("edit"), [b]), "candidate succeeds but interrupted transaction does not commit");
            featureTransitionBeforeCommitAudit = null;
            Check(liveEditor == oldEditor && FeatureTransitionStamp("edit") == editorStamp && oldEditor.FeatureTransitionAuditInlineIdentity == inline,
                "interrupted candidate retains editor inline and undo exactly");

            await oldEditor.PrepareFeatureTransitionAuditOriginalDraftAsync();
            editorStamp = FeatureTransitionStamp("edit"); inline = oldEditor.FeatureTransitionAuditInlineIdentity;
            var editorOrigin = CaptureFeatureTransitionOrigin();
            Choose("original");
            Check(editorOrigin.ToolId == "edit" && editorOrigin.HasPendingChanges &&
                (await ResolveFeatureTransitionOriginAsync(Tool("rotate"), editorOrigin))!.SequenceEqual(new[] { a }),
                "editor source transfer distinguishes original from pending native-text edits");
            Choose(null);
            Check(!await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [b]) &&
                FeatureTransitionStamp("edit") == editorStamp && oldEditor.FeatureTransitionAuditInlineIdentity == inline,
                "cancel retains uncommitted original-text draft, source ID, inline and undo");
            Choose("replace");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [corrupt]), "original-text draft replacement still requires explicit discard");
            await ExpectFailure(() => OpenFeatureTransitionDestinationAsync(Tool("edit"), [corrupt]), "bad candidate cannot discard original-text draft");
            Check(FeatureTransitionStamp("edit") == editorStamp && oldEditor.FeatureTransitionAuditInlineIdentity == inline,
                "failed new operation preserves original-text source identity and draft exactly");

            var unavailable = Tool("ocr");
            Check(!await PrepareFeatureTransitionDestinationAsync(unavailable, [b]), "unavailable feature cannot prepare a session");
            await OpenFeatureTransitionDestinationAsync(unavailable, [b]);
            Check(liveEditor == oldEditor && FeatureTransitionStamp("edit") == editorStamp, "unavailable feature does not mutate editor state");
            importing = true; Check(FeatureTransitionBusy, "active import blocks transition"); importing = false;
            restartRequested = true; Check(FeatureTransitionBusy, "restart blocks transition"); restartRequested = false;
            using (var work = new CancellationTokenSource())
            { active = work; Check(FeatureTransitionBusy, "active processing blocks transition"); active = null; }
            oldEditor.SetFeatureTransitionAuditWorking(true); Check(FeatureTransitionBusy, "editor work blocks transition"); oldEditor.SetFeatureTransitionAuditWorking(false);
            Choose("replace");
            Check(await PrepareFeatureTransitionDestinationAsync(Tool("edit"), [b]), "successful editor replacement is explicitly approved");
            await OpenFeatureTransitionDestinationAsync(Tool("edit"), [b]); await Settle();
            Check(liveEditor != oldEditor && liveEditor is { TransitionReady: true, Dirty: false } replacement && replacement.TransitionSource == b,
                "validated candidate replaces old editor once, with a clean new draft");
            Check(initialHashes.SequenceEqual(new[] { Hash(a), Hash(b), Hash(saved), Hash(corrupt) }), "all synthetic source/result bytes unchanged");
            report.Add("MANUAL_REQUIRED native dialog keyboard/focus and end-to-end routing at 660 DIP; run companion UI/routing audits.");
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(directory, "failure.txt"), error.ToString());
            report.Add("FAIL " + error.GetType().Name); Environment.ExitCode = 1;
        }
        finally
        {
            featureTransitionActionAudit = null; featureTransitionBeforeCommitAudit = null; featureTransitionAfterMutationAudit = null;
            importing = false; restartRequested = false; active = null;
            liveEditor?.ReleaseFiles(); liveEditor = null;
            restore();
            await File.WriteAllLinesAsync(Path.Combine(directory, "session-audit.txt"), report);
        }
    }

    private sealed partial class LiveEditor
    {
        internal object? FeatureTransitionAuditInlineIdentity => inlineText;
        internal void SetFeatureTransitionAuditWorking(bool working) => busy = working;
        internal async Task PrepareFeatureTransitionAuditDraftAsync()
        {
            Add(new(0, "text", 40, 140, 260, 35, "SYNTHETIC UNSAVED", 16));
            StartInlineText(0); await Task.Delay(100);
            inlineText!.Text = "SYNTHETIC INLINE DRAFT"; inlineText.Select(2, 4);
            // WinUI TextChanged is delivered asynchronously. Freeze the expected
            // undo/model checkpoint only after the synthetic typing event is applied.
            await Task.Delay(150); owner.root.UpdateLayout();
        }
        internal async Task PrepareFeatureTransitionAuditOriginalDraftAsync()
        {
            EndInlineText(); await LoadOriginalRunsAsync();
            if (originalRuns.Length == 0) throw new InvalidOperationException("Synthetic text fixture did not expose original text: " + originalInspectionMessage);
            StartOriginalInline(originalRuns[0]); await Task.Delay(100);
            inlineText!.Text += " X"; inlineText.Select(1, 3);
            await Task.Delay(150); owner.root.UpdateLayout();
        }
    }
}
#endif
