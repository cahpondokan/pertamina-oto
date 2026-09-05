namespace Pertamina.Oto.Modules.Pdf.Contracts;

// Module-local semantic contract, not a physical-path authority or public wire schema.
public sealed record InputOccurrence(Guid OccurrenceId, Guid FileId);
public enum MergeStatus { Failed, Cancelled, Completed }
public enum MergeIssue
{
    None, InvalidCommand, InputRejected, PasswordRequired, SourceChanged,
    Unavailable, InsufficientStorage, TargetRejected, TargetConflict,
    ProcessingFailed, ResultInvalid, PublicationFailed, ReconciliationRequired
}

public sealed class MergeCommand
{
    public MergeCommand(IEnumerable<InputOccurrence> inputs, IEnumerable<Guid> orderedOccurrenceIds,
        Guid targetId, Guid correlationId = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(orderedOccurrenceIds);
        Inputs = Array.AsReadOnly(inputs.ToArray());
        OrderedOccurrenceIds = Array.AsReadOnly(orderedOccurrenceIds.ToArray());
        TargetId = targetId;
        CorrelationId = correlationId;
    }
    public IReadOnlyList<InputOccurrence> Inputs { get; }
    public IReadOnlyList<Guid> OrderedOccurrenceIds { get; }
    public Guid TargetId { get; }
    public Guid CorrelationId { get; }
}

public sealed class MergeOutcome
{
    private MergeOutcome(MergeStatus status, MergeIssue issue, Guid correlationId, Guid? resultId, bool cleanupRequired = false)
    {
        Status = status;
        Issue = issue;
        CorrelationId = correlationId;
        ResultId = resultId; CleanupRequired = cleanupRequired;
    }
    public MergeStatus Status { get; }
    public MergeIssue Issue { get; }
    public Guid CorrelationId { get; }
    public Guid? ResultId { get; }
    public bool CleanupRequired { get; }
    public MergeOutcome WithCleanupRequired() => new(Status, Issue, CorrelationId, ResultId, true);
    public static MergeOutcome Failure(MergeIssue issue, Guid correlationId)
    {
        if (issue == MergeIssue.None || !Enum.IsDefined(issue))
            throw new ArgumentOutOfRangeException(nameof(issue));
        return new(MergeStatus.Failed, issue, correlationId, null);
    }
    public static MergeOutcome Cancelled(Guid correlationId) => new(MergeStatus.Cancelled, MergeIssue.None, correlationId, null);
    public static MergeOutcome Completed(Guid correlationId, Guid resultId)
    {
        if (resultId == Guid.Empty) throw new ArgumentException("Result reference required.", nameof(resultId));
        return new(MergeStatus.Completed, MergeIssue.None, correlationId, resultId);
    }
}

public interface IMergeService
{
    Task<MergeOutcome> ExecuteAsync(MergeCommand command, CancellationToken cancellationToken = default);
}
