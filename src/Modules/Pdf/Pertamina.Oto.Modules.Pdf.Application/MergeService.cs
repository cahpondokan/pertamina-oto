using Pertamina.Oto.Modules.Pdf.Contracts;
using Pertamina.Oto.Modules.Pdf.Domain;
namespace Pertamina.Oto.Modules.Pdf.Application;

public sealed class MergeService(IMergePreflight preflight, ILocalMergeProcessor processor,
    IMergePublication publication) : IMergeService
{
    public async Task<MergeOutcome> ExecuteAsync(MergeCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var correlation = command.CorrelationId == Guid.Empty ? Guid.NewGuid() : command.CorrelationId;
        Guid stage = default;
        var publishing = false;
        MergeOutcome outcome;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var plan = MergePlan.TryCreate(command, correlation);
            if (plan is null) return MergeOutcome.Failure(MergeIssue.InvalidCommand, correlation);
            var issue = await preflight.CheckAsync(plan, cancellationToken).ConfigureAwait(false);
            if (issue != MergeIssue.None) return Failure(issue, correlation);
            issue = await publication.CheckTargetAsync(plan, cancellationToken).ConfigureAwait(false);
            if (issue != MergeIssue.None) return Failure(issue, correlation);
            cancellationToken.ThrowIfCancellationRequested();
            stage = await processor.StageAsync(plan, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (stage == Guid.Empty ||
                !await publication.ValidateAsync(stage, plan, cancellationToken).ConfigureAwait(false))
                outcome = MergeOutcome.Failure(MergeIssue.ResultInvalid, correlation);
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                publishing = true;
                var receipt = await publication.PublishNewAsync(stage, plan, cancellationToken).ConfigureAwait(false);
                outcome = receipt.Status switch
                {
                    PublicationStatus.Published when receipt.ResultId != Guid.Empty =>
                        MergeOutcome.Completed(correlation, receipt.ResultId),
                    PublicationStatus.Conflict => MergeOutcome.Failure(MergeIssue.TargetConflict, correlation),
                    PublicationStatus.Failed => MergeOutcome.Failure(MergeIssue.PublicationFailed, correlation),
                    _ => MergeOutcome.Failure(MergeIssue.ReconciliationRequired, correlation)
                };
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested && !publishing)
        {
            outcome = MergeOutcome.Cancelled(correlation);
        }
        catch (Exception)
        {
            // No raw exception details leave the boundary. A possible commit is ambiguous,
            // including cancellation exceptions; reconcile rather than blindly retry.
            outcome = MergeOutcome.Failure(publishing ? MergeIssue.ReconciliationRequired : MergeIssue.ProcessingFailed, correlation);
        }
        if (stage != Guid.Empty)
        {
            try { await publication.DiscardStageAsync(stage, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception)
            {
                outcome = outcome.WithCleanupRequired();
            }
        }
        return outcome;
    }
    private static MergeOutcome Failure(MergeIssue issue, Guid correlation) =>
        MergeOutcome.Failure(Enum.IsDefined(issue) ? issue : MergeIssue.InputRejected, correlation);
}
