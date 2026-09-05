namespace Pertamina.Oto.Processing.Abstractions;

public enum LocalJobState { Queued, WaitingResource, Running, CancellationRequested, Completed, Cancelled, Failed }
public enum LocalFailureCode { None, Unavailable, InsufficientStorage, ValidationFailed, PublicationFailed, WorkerLost }
public enum LocalDispatchDecision { Unavailable, ExecuteLocally }
/// <summary>LOCAL has no remote route. Connectivity cannot promote it to server processing.</summary>
public static class LocalDispatchPolicy
{
    public static LocalDispatchDecision Evaluate(bool capabilityAvailable, bool policyAllows, bool resourcesAvailable) =>
        capabilityAvailable && policyAllows && resourcesAvailable
            ? LocalDispatchDecision.ExecuteLocally : LocalDispatchDecision.Unavailable;
}
/// <summary>Pure transition rule. Adapters serialize compare-and-swap updates. Completion requires publication;
/// cancellation acknowledgement requires safe stop. Lost worker lease requires recovery, not inferred success.
/// Retry uses a new attempt, never revives a terminal attempt.</summary>
public static class LocalJobTransitions
{
    public static bool IsTerminal(LocalJobState state) =>
        state is LocalJobState.Completed or LocalJobState.Cancelled or LocalJobState.Failed;
    public static bool CanTransition(LocalJobState current, LocalJobState next) => (current, next) switch
    {
        (LocalJobState.Queued, LocalJobState.Running or LocalJobState.WaitingResource or LocalJobState.Cancelled or LocalJobState.Failed) => true,
        (LocalJobState.WaitingResource, LocalJobState.Queued or LocalJobState.Cancelled or LocalJobState.Failed) => true,
        (LocalJobState.Running, LocalJobState.CancellationRequested or LocalJobState.Completed or LocalJobState.Failed) => true,
        // Confirmed publication wins over a cancellation that arrived after commit.
        (LocalJobState.CancellationRequested, LocalJobState.Cancelled or LocalJobState.Completed or LocalJobState.Failed) => true,
        _ => false
    };
}
/// <summary>No raw error text or provider/transport details.</summary>
public sealed record LocalJobSnapshot
{
    public LocalJobSnapshot(Guid jobId, Guid correlationId, LocalJobState state, LocalFailureCode failure)
    {
        if (jobId == Guid.Empty) throw new ArgumentException("Empty job ID.", nameof(jobId));
        if (correlationId == Guid.Empty) throw new ArgumentException("Empty correlation ID.", nameof(correlationId));
        if (!Enum.IsDefined(state)) throw new ArgumentOutOfRangeException(nameof(state));
        if (!Enum.IsDefined(failure) || (state == LocalJobState.Failed) != (failure != LocalFailureCode.None))
            throw new ArgumentOutOfRangeException(nameof(failure));
        JobId = jobId; CorrelationId = correlationId; State = state; Failure = failure;
    }
    public Guid JobId { get; }
    public Guid CorrelationId { get; }
    public LocalJobState State { get; }
    public LocalFailureCode Failure { get; }
}
/// <summary>Local boundary only, not a server Job API or IPC wire/security contract.
/// Authorize profile ownership independently; serialize transitions and ignore stale events.
/// Cancellation is intent, not terminal acknowledgement. Worker loss never triggers network fallback.</summary>
public interface ILocalJobControl
{
    ValueTask<LocalJobSnapshot> GetStatusAsync(Guid jobId, CancellationToken cancellationToken);
    ValueTask<LocalJobSnapshot> RequestCancellationAsync(Guid jobId, CancellationToken cancellationToken);
}
