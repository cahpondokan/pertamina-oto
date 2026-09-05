using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Modules.Pdf.Application;
using Pertamina.Oto.Modules.Pdf.Contracts;
using Pertamina.Oto.Modules.Pdf.Domain;
namespace Pertamina.Oto.Contracts.Tests;

[TestClass]
public sealed class MergeFailureBoundaryTests
{
    [TestMethod]
    public async Task Cancel_before_execution_never_stages_or_publishes()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var fake = new Boundary();
        var outcome = await new MergeService(fake, fake, fake).ExecuteAsync(Command(), cancellation.Token);
        Assert.AreEqual(MergeStatus.Cancelled, outcome.Status);
        Assert.AreEqual(0, fake.StageCalls);
        Assert.AreEqual(0, fake.PublishCalls);
    }
    [TestMethod]
    public async Task Invalid_output_and_conflicting_target_never_publish()
    {
        foreach (var conflict in new[] { true, false })
        {
            var fake = new Boundary { Conflict = conflict, Valid = false };
            var outcome = await new MergeService(fake, fake, fake).ExecuteAsync(Command());
            Assert.AreEqual(MergeStatus.Failed, outcome.Status);
            Assert.IsNull(outcome.ResultId);
            Assert.AreEqual(0, fake.PublishCalls);
            Assert.AreEqual(conflict ? 0 : 1, fake.StageCalls);
        }
    }
    [TestMethod]
    public async Task Cancellation_after_commit_preserves_final_result_and_correlation()
    {
        using var cancellation = new CancellationTokenSource();
        var fake = new Boundary { CommitCancellation = cancellation };
        var command = Command();
        var outcome = await new MergeService(fake, fake, fake).ExecuteAsync(command, cancellation.Token);
        Assert.AreEqual(MergeStatus.Completed, outcome.Status);
        Assert.AreEqual(fake.Result, outcome.ResultId);
        Assert.AreEqual(command.CorrelationId, outcome.CorrelationId);
        Assert.AreEqual(command.CorrelationId, fake.SeenCorrelation);
    }
    [TestMethod]
    public async Task Publication_exception_is_safe_reconciliation_without_final_reference()
    {
        var fake = new Boundary { ThrowOnPublish = true };
        var outcome = await new MergeService(fake, fake, fake).ExecuteAsync(Command());
        Assert.AreEqual(MergeIssue.ReconciliationRequired, outcome.Issue);
        Assert.IsNull(outcome.ResultId);
        Assert.AreEqual(1, fake.DiscardCalls);
    }
    private static MergeCommand Command()
    {
        var inputs = new[] { new InputOccurrence(Guid.NewGuid(), Guid.NewGuid()), new InputOccurrence(Guid.NewGuid(), Guid.NewGuid()) };
        return new(inputs, inputs.Select(i => i.OccurrenceId), Guid.NewGuid(), CorrelationId.New());
    }
    private sealed class Boundary : IMergePreflight, ILocalMergeProcessor, IMergePublication
    {
        public bool Conflict { get; init; }
        public bool Valid { get; init; } = true;
        public bool ThrowOnPublish { get; init; }
        public CancellationTokenSource? CommitCancellation { get; init; }
        public Guid Result { get; } = Guid.NewGuid();
        public int StageCalls { get; private set; }
        public int PublishCalls { get; private set; }
        public int DiscardCalls { get; private set; }
        public CorrelationId SeenCorrelation { get; private set; }
        public Task<MergeIssue> CheckAsync(MergePlan plan, CancellationToken cancellationToken) => Task.FromResult(MergeIssue.None);
        public Task<MergeIssue> CheckTargetAsync(MergePlan plan, CancellationToken cancellationToken) => Task.FromResult(Conflict ? MergeIssue.TargetConflict : MergeIssue.None);
        public Task<Guid> StageAsync(MergePlan plan, CancellationToken cancellationToken) { StageCalls++; SeenCorrelation = plan.CorrelationId; return Task.FromResult(Guid.NewGuid()); }
        public Task<bool> ValidateAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken) => Task.FromResult(Valid);
        public Task<PublicationReceipt> PublishNewAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken)
        {
            PublishCalls++;
            if (ThrowOnPublish) throw new InvalidOperationException("synthetic provider/path/content canary");
            CommitCancellation?.Cancel();
            return Task.FromResult(new PublicationReceipt(PublicationStatus.Published, Result));
        }
        public Task DiscardStageAsync(Guid stageId, CancellationToken cancellationToken) { DiscardCalls++; return Task.CompletedTask; }
    }
}
