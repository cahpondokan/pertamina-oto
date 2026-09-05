using Pertamina.Oto.Modules.Pdf.Contracts;
namespace Pertamina.Oto.Desktop;
public enum EntryFlow { FileFirst, ToolFirst }
public enum ShellPhase { Idle, Validation, Running, Cancelled, Completed, Failed, Unavailable }
public sealed record ShellState(ShellPhase Phase, MergeIssue Issue = MergeIssue.None, Guid? ResultId = null, bool CancellationRequested = false)
{
    // Application contract has no progress stream. Never invent percentage or ETA.
    public double? Percentage { get; }
    public bool IsIndeterminate => Phase is ShellPhase.Validation or ShellPhase.Running;
    public bool CanCancel => Phase == ShellPhase.Running && !CancellationRequested;
}
/// <summary>UI-thread adapter. The host must invoke and observe it on its UI synchronization context.</summary>
public sealed class MergeShell(IMergeService service)
{
    private readonly IMergeService service = service ?? throw new ArgumentNullException(nameof(service));
    private CancellationTokenSource? active;
    public ShellState State { get; private set; } = new(ShellPhase.Idle);
    public async Task ExecuteAsync(EntryFlow entry, MergeCommand command)
    {
        if (!Enum.IsDefined(entry)) throw new ArgumentOutOfRangeException(nameof(entry));
        ArgumentNullException.ThrowIfNull(command);
        if (active is not null) throw new InvalidOperationException("An operation is already active.");
        using var cancellation = new CancellationTokenSource();
        active = cancellation;
        State = new(ShellPhase.Validation);
        try
        {
            State = new(ShellPhase.Running);
            // Both entry flows pass the same immutable command. Domain validation stays in service.
            var outcome = await service.ExecuteAsync(command, cancellation.Token);
            State = outcome.CorrelationId != command.CorrelationId
                ? new(ShellPhase.Failed, MergeIssue.ProcessingFailed)
                : new(outcome.Status switch
                {
                    MergeStatus.Completed => ShellPhase.Completed,
                    MergeStatus.Cancelled => ShellPhase.Cancelled,
                    _ when outcome.Issue == MergeIssue.Unavailable => ShellPhase.Unavailable,
                    _ => ShellPhase.Failed
                }, outcome.Issue, outcome.ResultId);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        { State = new(ShellPhase.Cancelled); }
        catch (Exception)
        { State = new(ShellPhase.Failed, MergeIssue.ProcessingFailed); }
        finally { active = null; }
    }
    public void Cancel()
    {
        if (!State.CanCancel || active is null) return;
        State = State with { CancellationRequested = true };
        try { active.Cancel(); }
        catch (AggregateException) { /* A request cannot fabricate a terminal outcome. */ }
    }
}
