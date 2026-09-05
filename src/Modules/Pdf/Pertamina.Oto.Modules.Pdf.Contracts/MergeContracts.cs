using Pertamina.Oto.Contracts.Identifiers;

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
        Guid targetId, CorrelationId correlationId = default)
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
    public CorrelationId CorrelationId { get; }
}

public sealed class MergeOutcome
{
    private MergeOutcome(MergeStatus status, MergeIssue issue, CorrelationId correlationId, Guid? resultId, bool cleanupRequired = false)
    {
        if (!correlationId.IsValid)
        {
            throw new ArgumentException("A valid correlation identifier is required.", nameof(correlationId));
        }

        Status = status;
        Issue = issue;
        CorrelationId = correlationId;
        ResultId = resultId; CleanupRequired = cleanupRequired;
    }
    public MergeStatus Status { get; }
    public MergeIssue Issue { get; }
    public CorrelationId CorrelationId { get; }
    public Guid? ResultId { get; }
    public bool CleanupRequired { get; }
    public MergeOutcome WithCleanupRequired() => new(Status, Issue, CorrelationId, ResultId, true);
    public static MergeOutcome Failure(MergeIssue issue, CorrelationId correlationId)
    {
        if (issue == MergeIssue.None || !Enum.IsDefined(issue))
            throw new ArgumentOutOfRangeException(nameof(issue));
        return new(MergeStatus.Failed, issue, correlationId, null);
    }
    public static MergeOutcome Cancelled(CorrelationId correlationId) => new(MergeStatus.Cancelled, MergeIssue.None, correlationId, null);
    public static MergeOutcome Completed(CorrelationId correlationId, Guid resultId)
    {
        if (resultId == Guid.Empty) throw new ArgumentException("Result reference required.", nameof(resultId));
        return new(MergeStatus.Completed, MergeIssue.None, correlationId, resultId);
    }
}

public interface IMergeService
{
    Task<MergeOutcome> ExecuteAsync(MergeCommand command, CancellationToken cancellationToken = default);
}
