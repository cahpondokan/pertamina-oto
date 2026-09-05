using Pertamina.Oto.Modules.Pdf.Contracts;
using Pertamina.Oto.Modules.Pdf.Domain;
namespace Pertamina.Oto.Modules.Pdf.Application;

// Trusted local preflight verifies permission/ownership, descriptor compatibility,
// input readiness/version and capacity. None is the only admission.
// Client-provided readiness booleans are not authorization.
public interface IMergePreflight
{
    Task<MergeIssue> CheckAsync(MergePlan plan, CancellationToken cancellationToken);
}

// Sources are read-only and bound to validated versions. Return only an unaddressable stage.
// No source mutation, final publication or network/fallback authority.
// If staging throws before returning an ID, the processor owns orphan cleanup/recovery.
public interface ILocalMergeProcessor
{
    Task<Guid> StageAsync(MergePlan plan, CancellationToken cancellationToken);
}
public enum PublicationStatus { Failed, Published, Conflict, Indeterminate }
public sealed record PublicationReceipt(PublicationStatus Status, Guid ResultId = default);

public interface IMergePublication
{
    // Resolve opaque target under policy; reject source aliases and existing targets.
    Task<MergeIssue> CheckTargetAsync(MergePlan plan, CancellationToken cancellationToken);
    Task<bool> ValidateAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken);
    // Atomically create NEW target, rechecking aliases/version/conflict at commit.
    // Return Published after commit even if cancellation arrives; unknown effect => Indeterminate.
    Task<PublicationReceipt> PublishNewAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken);
    // Stage-only cleanup, safe after publish. Never delete source or final result.
    Task DiscardStageAsync(Guid stageId, CancellationToken cancellationToken);
}
