using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Storage.Abstractions;
using Pertamina.Oto.Processing.Abstractions;

namespace Pertamina.Oto.OfflineFoundation.Tests;

[TestClass]
public sealed class OfflineContractTests
{
    private static readonly Guid Id = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    [TestMethod]
    public void EmptyReferencesAreRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new LocalFileRef(Guid.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => new LocalArtifactRef(Guid.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => new LocalTargetRef(Guid.Empty));
    }
    [TestMethod]
    public void ReferenceAndMetadataSurfaceCannotCarryRawStringsOrBytes()
    {
        Type[] types = [typeof(LocalFileRef), typeof(LocalArtifactRef), typeof(LocalTargetRef),
            typeof(LocalHistoryEntry), typeof(LocalWorkspaceSnapshot), typeof(LocalJobSnapshot)];
        foreach (var type in types)
            foreach (var property in type.GetProperties())
            {
                Assert.AreNotEqual(typeof(string), property.PropertyType);
                Assert.AreNotEqual(typeof(byte[]), property.PropertyType);
                Assert.AreNotEqual(typeof(Uri), property.PropertyType);
            }
    }
    [TestMethod]
    public void AnOpaqueReferenceDoesNotAuthorizePublication() =>
        Assert.AreEqual(PublicationOutcome.AccessDenied, LocalPublicationPolicy.Evaluate(ArtifactStage.Validated, false, false, false, true, false));
    [TestMethod]
    public void SourceAliasIsDenied() =>
        Assert.AreEqual(PublicationOutcome.SourceConflict, LocalPublicationPolicy.Evaluate(ArtifactStage.Validated, true, true, false, true, false));
    [TestMethod]
    public void ExistingTargetIsDenied() =>
        Assert.AreEqual(PublicationOutcome.TargetExists, LocalPublicationPolicy.Evaluate(ArtifactStage.Validated, true, false, true, true, false));
    [TestMethod]
    public void OnlyValidatedStageCanPublish()
    {
        foreach (var stage in new[] { ArtifactStage.Staged, ArtifactStage.Published, ArtifactStage.Abandoned, (ArtifactStage)99 })
            Assert.AreEqual(PublicationOutcome.InvalidStage, LocalPublicationPolicy.Evaluate(stage, true, false, false, true, false));
    }
    [TestMethod]
    public void CapacityFailureBlocksPublication() =>
        Assert.AreEqual(PublicationOutcome.InsufficientStorage, LocalPublicationPolicy.Evaluate(ArtifactStage.Validated, true, false, false, false, false));
    [TestMethod]
    public void CancellationBeforeCommitBlocksPublication() =>
        Assert.AreEqual(PublicationOutcome.Cancelled, LocalPublicationPolicy.Evaluate(ArtifactStage.Validated, true, false, false, true, true));
    [TestMethod]
    public void FailureResultsCannotContainFinalReference()
    {
        foreach (var outcome in Enum.GetValues<PublicationOutcome>().Where(x => x is not (PublicationOutcome.Unknown or PublicationOutcome.Validated or PublicationOutcome.Published)))
            Assert.IsNull(PublicationResult.Rejected(outcome).FinalReference);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PublicationResult.Rejected(PublicationOutcome.Published));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PublicationResult.Rejected((PublicationOutcome)99));
        Assert.ThrowsExactly<ArgumentNullException>(() => PublicationResult.Published(null!));
    }
    [TestMethod]
    public void AllTerminalStatesRejectEveryLaterTransition()
    {
        foreach (var state in new[] { LocalJobState.Completed, LocalJobState.Cancelled, LocalJobState.Failed })
        {
            Assert.IsTrue(LocalJobTransitions.IsTerminal(state));
            foreach (var next in Enum.GetValues<LocalJobState>())
                Assert.IsFalse(LocalJobTransitions.CanTransition(state, next));
        }
    }
    [TestMethod]
    public void CancellationRequiresSafeAcknowledgement()
    {
        Assert.IsTrue(LocalJobTransitions.CanTransition(LocalJobState.Queued, LocalJobState.Cancelled));
        Assert.IsTrue(LocalJobTransitions.CanTransition(LocalJobState.Running, LocalJobState.CancellationRequested));
        Assert.IsFalse(LocalJobTransitions.IsTerminal(LocalJobState.CancellationRequested));
        Assert.IsFalse(LocalJobTransitions.CanTransition(LocalJobState.Running, LocalJobState.Cancelled));
        Assert.IsTrue(LocalJobTransitions.CanTransition(LocalJobState.CancellationRequested, LocalJobState.Cancelled));
        Assert.IsTrue(LocalJobTransitions.CanTransition(LocalJobState.CancellationRequested, LocalJobState.Completed));
    }
    [TestMethod]
    public void InvalidJobStateOrUnsafeFailureCombinationRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalJobSnapshot(Id, Id, (LocalJobState)99, LocalFailureCode.None));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalJobSnapshot(Id, Id, LocalJobState.Failed, LocalFailureCode.None));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalJobSnapshot(Id, Id, LocalJobState.Completed, LocalFailureCode.WorkerLost));
        Assert.ThrowsExactly<ArgumentException>(() => new LocalJobSnapshot(Id, Guid.Empty, LocalJobState.Queued, LocalFailureCode.None));
    }
    [TestMethod]
    public void MissingLocalCapabilityNeverCreatesRemoteRoute()
    {
        Assert.AreEqual(LocalDispatchDecision.ExecuteLocally, LocalDispatchPolicy.Evaluate(true, true, true));
        Assert.AreEqual(LocalDispatchDecision.Unavailable, LocalDispatchPolicy.Evaluate(false, true, true));
        Assert.AreEqual(LocalDispatchDecision.Unavailable, LocalDispatchPolicy.Evaluate(true, false, true));
        Assert.AreEqual(LocalDispatchDecision.Unavailable, LocalDispatchPolicy.Evaluate(true, true, false));
        Assert.HasCount(2, Enum.GetNames<LocalDispatchDecision>());
    }
    [TestMethod]
    public void HistoryPageIsBounded()
    {
        LocalMetadataPolicy.ValidatePageSize(1);
        LocalMetadataPolicy.ValidatePageSize(50);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => LocalMetadataPolicy.ValidatePageSize(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => LocalMetadataPolicy.ValidatePageSize(51));
    }
    [TestMethod]
    public async Task FakePublicationRequiresValidationAndPreservesSources()
    {
        var fake = new FakePublisher();
        var artifact = await fake.StageAsync(CancellationToken.None);
        var target = new LocalTargetRef(Id);
        LocalFileRef[] sources = [new(Id)];
        Assert.AreEqual(PublicationOutcome.InvalidStage, (await fake.PublishAsync(artifact, target, sources, CancellationToken.None)).Outcome);
        Assert.AreEqual(PublicationOutcome.Validated, await fake.ValidateAsync(artifact, CancellationToken.None));
        Assert.AreEqual(PublicationOutcome.Published, (await fake.PublishAsync(artifact, target, sources, CancellationToken.None)).Outcome);
        Assert.AreEqual(Id, sources[0].Value);

    }
    [TestMethod]
    public async Task FakeInjectedPublicationFailureLeavesNoFinalAndCleanupIntent()
    {
        var fake = new FakePublisher { FailCommit = true };
        var artifact = await fake.StageAsync(CancellationToken.None);
        await fake.ValidateAsync(artifact, CancellationToken.None);
        var result = await fake.PublishAsync(artifact, new(Id), [new(Id)], CancellationToken.None);
        Assert.AreEqual(PublicationOutcome.Failed, result.Outcome);
        Assert.IsNull(result.FinalReference);
        Assert.IsTrue(fake.CleanupPending);
        await fake.AbandonAsync(artifact, CancellationToken.None);
        Assert.IsFalse(fake.CleanupPending);
    }
    [TestMethod]
    public async Task FakeCancelledPublishCannotExposeFinal()
    {
        var fake = new FakePublisher();
        var artifact = await fake.StageAsync(CancellationToken.None);
        await fake.ValidateAsync(artifact, CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var result = await fake.PublishAsync(artifact, new(Id), [new(Id)], cancellation.Token);
        Assert.AreEqual(PublicationOutcome.Cancelled, result.Outcome);
        Assert.IsNull(result.FinalReference);
    }
    [TestMethod]
    public void BoundaryAssembliesHaveNoNetworkOrFilesystemProviderReference()
    {
        foreach (var assembly in new[] { typeof(LocalPublicationPolicy).Assembly, typeof(LocalDispatchPolicy).Assembly })
            foreach (var reference in assembly.GetReferencedAssemblies())
                Assert.IsTrue(reference.Name is "System.Runtime" or "System.Collections" or "Pertamina.Oto.Contracts", reference.Name);
    }
    // Contract double only: deliberately no filesystem, PDF bytes, database, worker or network implementation.
    private sealed class FakePublisher : ILocalArtifactPublication
    {
        private ArtifactStage stage;
        public bool FailCommit { get; init; }
        public bool CleanupPending { get; private set; }


        public ValueTask<LocalArtifactRef> StageAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stage = ArtifactStage.Staged;
            CleanupPending = true;
            return ValueTask.FromResult(new LocalArtifactRef(Id));
        }
        public ValueTask<PublicationOutcome> ValidateAsync(LocalArtifactRef artifact, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stage = ArtifactStage.Validated;
            return ValueTask.FromResult(PublicationOutcome.Validated);
        }
        public ValueTask<PublicationResult> PublishAsync(LocalArtifactRef artifact, LocalTargetRef target,
            IReadOnlyList<LocalFileRef> sources, CancellationToken cancellationToken)
        {
            var outcome = LocalPublicationPolicy.Evaluate(stage, true, false, false, true, cancellationToken.IsCancellationRequested);
            if (outcome != PublicationOutcome.Published) return ValueTask.FromResult(PublicationResult.Rejected(outcome));
            if (FailCommit) return ValueTask.FromResult(PublicationResult.Rejected(PublicationOutcome.Failed));
            stage = ArtifactStage.Published;
            CleanupPending = false;
            return ValueTask.FromResult(PublicationResult.Published(new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"))));
        }
        public ValueTask AbandonAsync(LocalArtifactRef artifact, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stage != ArtifactStage.Published) { stage = ArtifactStage.Abandoned; CleanupPending = false; }
            return ValueTask.CompletedTask;
        }
    }
}
