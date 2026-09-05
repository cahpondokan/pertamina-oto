namespace Pertamina.Oto.Storage.Abstractions;

public enum ArtifactStage { Staged, Validated, Published, Abandoned }
public enum PublicationOutcome
{
    Unknown, Validated, Published, InvalidStage, AccessDenied, SourceConflict, TargetExists,
    InsufficientStorage, ValidationFailed, Cancelled, Failed, ReconciliationRequired
}
/// <summary>Adapter-verified facts only. Repeat checks inside atomic no-replace operation to avoid races.</summary>
public static class LocalPublicationPolicy
{
    public static PublicationOutcome Evaluate(ArtifactStage stage, bool authorized,
        bool sourceAlias, bool targetExists, bool sufficientStorage, bool cancellationRequested)
    {
        if (!authorized) return PublicationOutcome.AccessDenied;
        if (sourceAlias) return PublicationOutcome.SourceConflict;
        if (targetExists) return PublicationOutcome.TargetExists;
        if (stage != ArtifactStage.Validated) return PublicationOutcome.InvalidStage;
        if (!sufficientStorage) return PublicationOutcome.InsufficientStorage;
        if (cancellationRequested) return PublicationOutcome.Cancelled;
        return PublicationOutcome.Published;
    }
}
/// <summary>Only confirmed publication can carry a final reference.</summary>
public sealed record PublicationResult
{
    private PublicationResult(PublicationOutcome outcome, LocalFileRef? finalReference)
    {
        Outcome = outcome;
        FinalReference = finalReference;
    }
    public PublicationOutcome Outcome { get; }
    public LocalFileRef? FinalReference { get; }
    public static PublicationResult Published(LocalFileRef finalReference) =>
        new(PublicationOutcome.Published, finalReference ?? throw new ArgumentNullException(nameof(finalReference)));
    public static PublicationResult Rejected(PublicationOutcome outcome)
    {
        if (!Enum.IsDefined(outcome) || outcome is PublicationOutcome.Unknown or PublicationOutcome.Validated or PublicationOutcome.Published)
            throw new ArgumentOutOfRangeException(nameof(outcome));
        return new(outcome, null);
    }
}
/// <summary>Local-only, profile-scoped. No remote fallback.
/// Stage creates a new managed temporary artifact. Validate closes/flushes, validates structure and digest,
/// and binds validation to immutable staged bytes. Publish rechecks authorization, source aliases (including
/// links), capacity and target absence, then atomically publishes with no-replace in the same filesystem
/// and commits metadata. References are not authority. There is no overwrite mode.
/// Cancel before commit leaves no final result; after commit success wins. Uncertain commit/pointer failure
/// returns ReconciliationRequired and retains durable reconciliation/cleanup intent. No partial final output.
/// Adapter exceptions must be mapped to safe outcomes; never mutate a source or silently infer success.</summary>
public interface ILocalArtifactPublication
{
    ValueTask<LocalArtifactRef> StageAsync(CancellationToken cancellationToken);
    ValueTask<PublicationOutcome> ValidateAsync(LocalArtifactRef artifact, CancellationToken cancellationToken);
    ValueTask<PublicationResult> PublishAsync(LocalArtifactRef artifact, LocalTargetRef target,
        IReadOnlyList<LocalFileRef> sources, CancellationToken cancellationToken);
    /// <summary>Idempotent cleanup of non-final managed artifacts only; never sources or published files.</summary>
    ValueTask AbandonAsync(LocalArtifactRef artifact, CancellationToken cancellationToken);
}
