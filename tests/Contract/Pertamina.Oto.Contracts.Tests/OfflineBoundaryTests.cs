using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Processing.Abstractions;
using Pertamina.Oto.Storage.Abstractions;
using Pertamina.Oto.Modules.Pdf.Contracts;
using Pertamina.Oto.Modules.Pdf.Domain;
namespace Pertamina.Oto.Contracts.Tests;
[TestClass]
public sealed class OfflineBoundaryTests
{
    [TestMethod]
    public void Every_terminal_state_rejects_every_subsequent_state()
    {
        foreach (var terminal in new[] { LocalJobState.Completed, LocalJobState.Cancelled, LocalJobState.Failed })
        foreach (var next in Enum.GetValues<LocalJobState>())
            Assert.IsFalse(LocalJobTransitions.CanTransition(terminal, next));
        Assert.IsFalse(LocalJobTransitions.CanTransition(LocalJobState.Queued, LocalJobState.Completed));
        Assert.IsTrue(LocalJobTransitions.CanTransition(LocalJobState.Running, LocalJobState.CancellationRequested));
    }
    [TestMethod]
    public void Local_dispatch_truth_table_has_no_network_fallback()
    {
        Assert.AreEqual(2, Enum.GetValues<LocalDispatchDecision>().Length);
        for (var bits = 0; bits < 8; bits++)
            Assert.AreEqual(bits == 7 ? LocalDispatchDecision.ExecuteLocally : LocalDispatchDecision.Unavailable,
                LocalDispatchPolicy.Evaluate((bits & 1) != 0, (bits & 2) != 0, (bits & 4) != 0));
    }
    [TestMethod]
    public void Publication_truth_table_requires_all_safety_facts()
    {
        foreach (var stage in Enum.GetValues<ArtifactStage>())
        for (var bits = 0; bits < 32; bits++)
        {
            bool authorized = (bits & 1) != 0, alias = (bits & 2) != 0, exists = (bits & 4) != 0,
                capacity = (bits & 8) != 0, cancel = (bits & 16) != 0;
            var actual = LocalPublicationPolicy.Evaluate(stage, authorized, alias, exists, capacity, cancel);
            Assert.AreEqual(stage == ArtifactStage.Validated && authorized && !alias && !exists && capacity && !cancel,
                actual == PublicationOutcome.Published);
        }
        foreach (var outcome in Enum.GetValues<PublicationOutcome>().Except([PublicationOutcome.Unknown, PublicationOutcome.Validated, PublicationOutcome.Published]))
            Assert.IsNull(PublicationResult.Rejected(outcome).FinalReference);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PublicationResult.Rejected(PublicationOutcome.Published));
    }
    [TestMethod]
    public void Shared_correlation_and_source_identity_survive_boundaries()
    {
        var id = CorrelationId.New();
        var inputs = new[] { new InputOccurrence(Guid.NewGuid(), Guid.NewGuid()), new InputOccurrence(Guid.NewGuid(), Guid.NewGuid()) };
        var command = new MergeCommand(inputs, inputs.Select(i => i.OccurrenceId), Guid.NewGuid(), id);
        var plan = MergePlan.TryCreate(command, id);
        Assert.IsNotNull(plan);
        Assert.AreEqual(id, plan.CorrelationId);
        Assert.AreEqual(id, MergeOutcome.Cancelled(id).CorrelationId);
        Assert.AreEqual(id, new LocalJobSnapshot(Guid.NewGuid(), id, LocalJobState.Queued, LocalFailureCode.None).CorrelationId);
        var overwrite = new MergeCommand(inputs, inputs.Select(i => i.OccurrenceId), inputs[0].FileId, id);
        Assert.IsNull(MergePlan.TryCreate(overwrite, id));
        Assert.ThrowsExactly<ArgumentException>(() => new LocalFileRef(Guid.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => MergeOutcome.Completed(id, Guid.Empty));
    }
}
