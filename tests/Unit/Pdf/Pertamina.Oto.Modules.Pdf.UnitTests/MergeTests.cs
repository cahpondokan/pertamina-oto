using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Modules.Pdf.Application;
using Pertamina.Oto.Modules.Pdf.Contracts;
using Pertamina.Oto.Modules.Pdf.Domain;
namespace Pertamina.Oto.Modules.Pdf.UnitTests;

[TestClass]
public sealed class MergeTests
{
    private static readonly string[] SuccessCalls = ["preflight", "target", "stage", "validate", "publish", "discard"];
    private static readonly string[] TargetCalls = ["preflight", "target"];
    private static readonly string[] PreflightCalls = ["preflight"];
    private static InputOccurrence[] Inputs(int count) => Enumerable.Range(0, count)
        .Select(_ => new InputOccurrence(Guid.NewGuid(), Guid.NewGuid())).ToArray();
    private static MergeCommand Command(InputOccurrence[] inputs, Guid[]? order = null, Guid? target = null) =>
        new(inputs, order ?? inputs.Select(i => i.OccurrenceId), target ?? Guid.NewGuid(), CorrelationId.New());

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public async Task Execute_TooFewInputs_RejectsWithoutBoundaryCalls(int count)
    {
        var fake = new Boundary();
        var result = await fake.Service.ExecuteAsync(Command(Inputs(count)));
        Assert.AreEqual(MergeIssue.InvalidCommand, result.Issue);
        Assert.IsEmpty(fake.Calls);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(20)]
    public async Task Execute_ValidInputs_PreservesExactOrderAndFinality(int count)
    {
        var inputs = Inputs(count);
        var fake = new Boundary();
        var command = Command(inputs, inputs.Reverse().Select(i => i.OccurrenceId).ToArray());
        var result = await fake.Service.ExecuteAsync(command);
        Assert.AreEqual(MergeStatus.Completed, result.Status);
        Assert.AreEqual(fake.Result, result.ResultId);
        Assert.IsNotNull(fake.Plan);
        CollectionAssert.AreEqual(inputs.Reverse().ToArray(), fake.Plan.OrderedInputs.ToArray());
        CollectionAssert.AreEqual(SuccessCalls, fake.Calls);
        Assert.AreEqual(command.CorrelationId, result.CorrelationId);
    }

    [TestMethod]
    public async Task Execute_DuplicateFileOccurrences_DoesNotDeduplicate()
    {
        var inputs = Inputs(2);
        inputs[1] = inputs[1] with { FileId = inputs[0].FileId };
        var fake = new Boundary();
        await fake.Service.ExecuteAsync(Command(inputs));
        Assert.IsNotNull(fake.Plan);
        CollectionAssert.AreEqual(inputs, fake.Plan.OrderedInputs.ToArray());
    }

    [TestMethod]
    public void Plan_RemoveAndReorder_ChangesOnlyWorkingSelectionDeterministically()
    {
        var inputs = Inputs(3);
        var command = Command(inputs, [inputs[2].OccurrenceId, inputs[0].OccurrenceId]);
        var first = MergePlan.TryCreate(command, command.CorrelationId);
        var second = MergePlan.TryCreate(command, command.CorrelationId);
        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        CollectionAssert.AreEqual(new[] { inputs[2], inputs[0] }, first.OrderedInputs.ToArray());
        CollectionAssert.AreEqual(first.OrderedInputs.ToArray(), second.OrderedInputs.ToArray());
        Assert.AreEqual(3, command.Inputs.Count);
        Assert.IsNull(MergePlan.TryCreate(Command(inputs, [inputs[0].OccurrenceId]), CorrelationId.New()));
    }

    [TestMethod]
    [DataRow("unknown")]
    [DataRow("repeated")]
    [DataRow("duplicateIdentity")]
    [DataRow("emptyFile")]
    [DataRow("emptyTarget")]
    [DataRow("sourceTarget")]
    public async Task Execute_InvalidIdentity_RejectsBeforeProcessing(string condition)
    {
        var inputs = Inputs(2);
        var order = inputs.Select(i => i.OccurrenceId).ToArray();
        var target = Guid.NewGuid();
        if (condition == "unknown") order[1] = Guid.NewGuid();
        if (condition == "repeated") order[1] = order[0];
        if (condition == "duplicateIdentity") inputs[1] = inputs[0];
        if (condition == "emptyFile") inputs[0] = inputs[0] with { FileId = Guid.Empty };
        if (condition == "emptyTarget") target = Guid.Empty;
        if (condition == "sourceTarget") target = inputs[0].FileId;
        var fake = new Boundary();
        Assert.AreEqual(MergeIssue.InvalidCommand, (await fake.Service.ExecuteAsync(Command(inputs, order, target))).Issue);
        Assert.IsEmpty(fake.Calls);
    }

    [TestMethod]
    [DataRow(MergeIssue.InputRejected)]
    [DataRow(MergeIssue.PasswordRequired)]
    [DataRow(MergeIssue.SourceChanged)]
    [DataRow(MergeIssue.Unavailable)]
    [DataRow(MergeIssue.InsufficientStorage)]
    [DataRow((MergeIssue)999)]
    public async Task Execute_PreflightDenied_NeverStagesOrPublishes(MergeIssue issue)
    {
        var fake = new Boundary { Preflight = issue };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)));
        Assert.AreEqual(MergeStatus.Failed, result.Status);
        Assert.IsNull(result.ResultId);
        CollectionAssert.AreEqual(PreflightCalls, fake.Calls);
    }

    [TestMethod]
    [DataRow(MergeIssue.TargetRejected)]
    [DataRow(MergeIssue.TargetConflict)]
    public async Task Execute_TargetDenied_PreservesSourceAndExistingTarget(MergeIssue issue)
    {
        var fake = new Boundary { Target = issue };
        Assert.AreEqual(issue, (await fake.Service.ExecuteAsync(Command(Inputs(2)))).Issue);
        CollectionAssert.AreEqual(TargetCalls, fake.Calls);
    }

    [TestMethod]
    public async Task Execute_PreCancelled_HasNoEffects()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var fake = new Boundary();
        Assert.AreEqual(MergeStatus.Cancelled, (await fake.Service.ExecuteAsync(Command(Inputs(2)), cancellation.Token)).Status);
        Assert.IsEmpty(fake.Calls);
    }

    [TestMethod]
    [DataRow("stage")]
    [DataRow("validate")]
    public async Task Execute_CancelAtSafePoint_DiscardsOnlyStage(string point)
    {
        using var cancellation = new CancellationTokenSource();
        var fake = new Boundary { OnCall = name => { if (name == point) cancellation.Cancel(); } };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)), cancellation.Token);
        Assert.AreEqual(MergeStatus.Cancelled, result.Status);
        Assert.IsNull(result.ResultId);
        Assert.DoesNotContain("publish", fake.Calls);
        Assert.Contains("discard", fake.Calls);
    }

    [TestMethod]
    public async Task Execute_CancelAfterCommit_RemainsCompleted()
    {
        using var cancellation = new CancellationTokenSource();
        var fake = new Boundary { OnCall = name => { if (name == "publish") cancellation.Cancel(); } };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)), cancellation.Token);
        Assert.AreEqual(MergeStatus.Completed, result.Status);
        cancellation.Cancel();
        Assert.AreEqual(fake.Result, result.ResultId);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Execute_InvalidStage_NeverPublishes(bool emptyStage)
    {
        var fake = new Boundary { Valid = false, EmptyStage = emptyStage };
        Assert.AreEqual(MergeIssue.ResultInvalid, (await fake.Service.ExecuteAsync(Command(Inputs(2)))).Issue);
        Assert.DoesNotContain("publish", fake.Calls);
    }

    [TestMethod]
    [DataRow(PublicationStatus.Conflict, MergeIssue.TargetConflict)]
    [DataRow(PublicationStatus.Failed, MergeIssue.PublicationFailed)]
    [DataRow(PublicationStatus.Indeterminate, MergeIssue.ReconciliationRequired)]
    [DataRow((PublicationStatus)999, MergeIssue.ReconciliationRequired)]
    public async Task Execute_NonFinalPublication_ExposesNoResult(PublicationStatus status, MergeIssue issue)
    {
        var fake = new Boundary { PublishStatus = status };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)));
        Assert.AreEqual(issue, result.Issue);
        Assert.IsNull(result.ResultId);
        Assert.Contains("discard", fake.Calls);
    }

    [TestMethod]
    [DataRow("stage", MergeIssue.ProcessingFailed)]
    [DataRow("validate", MergeIssue.ProcessingFailed)]
    [DataRow("publish", MergeIssue.ReconciliationRequired)]
    public async Task Execute_BoundaryException_ReturnsOnlySafeCategory(string point, MergeIssue issue)
    {
        var fake = new Boundary { OnCall = name => { if (name == point) throw new InvalidOperationException("SENSITIVE_CANARY"); } };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)));
        Assert.AreEqual(issue, result.Issue);
        Assert.IsNull(result.ResultId);
        Assert.DoesNotContain("SENSITIVE_CANARY", System.Text.Json.JsonSerializer.Serialize(result));
    }

    [TestMethod]
    public async Task Execute_EmptyPublicationReceipt_RequiresReconciliation()
    {
        var fake = new Boundary { Result = Guid.Empty };
        Assert.AreEqual(MergeIssue.ReconciliationRequired, (await fake.Service.ExecuteAsync(Command(Inputs(2)))).Issue);
    }

    [TestMethod]
    public async Task Execute_CorrelationMissing_GeneratesAndPropagatesOneIdentifier()
    {
        var inputs = Inputs(2);
        var fake = new Boundary();
        var result = await fake.Service.ExecuteAsync(new MergeCommand(inputs, inputs.Select(i => i.OccurrenceId), Guid.NewGuid()));
        Assert.IsTrue(result.CorrelationId.IsValid);
        Assert.IsNotNull(fake.Plan);
        Assert.AreEqual(result.CorrelationId, fake.Plan.CorrelationId);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Execute_CleanupFailure_PreservesTerminalStateAndRecordsIntent(bool cancel)
    {
        using var cancellation = new CancellationTokenSource();
        var fake = new Boundary
        {
            OnCall = name =>
        {
            if (cancel && name == "stage") cancellation.Cancel();
            if (name == "discard") throw new InvalidOperationException("SENSITIVE_CANARY");
        }
        };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)), cancellation.Token);
        Assert.AreEqual(cancel ? MergeStatus.Cancelled : MergeStatus.Completed, result.Status);
        Assert.IsTrue(result.CleanupRequired);
    }

    [TestMethod]
    public async Task Execute_CancellationExceptionDuringPublication_RequiresReconciliation()
    {
        using var cancellation = new CancellationTokenSource();
        var fake = new Boundary
        {
            OnCall = name =>
        {
            if (name == "publish") { cancellation.Cancel(); throw new OperationCanceledException(cancellation.Token); }
        }
        };
        var result = await fake.Service.ExecuteAsync(Command(Inputs(2)), cancellation.Token);
        Assert.AreEqual(MergeIssue.ReconciliationRequired, result.Issue);
        Assert.IsNull(result.ResultId);
    }
    private sealed class Boundary : IMergePreflight, ILocalMergeProcessor, IMergePublication
    {
        public List<string> Calls { get; } = [];
        public MergePlan? Plan { get; private set; }
        public MergeIssue Preflight { get; init; }
        public MergeIssue Target { get; init; }
        public bool Valid { get; init; } = true;
        public bool EmptyStage { get; init; }
        public Guid Result { get; init; } = Guid.NewGuid();
        public PublicationStatus PublishStatus { get; init; } = PublicationStatus.Published;
        public Action<string>? OnCall { get; init; }
        public MergeService Service => new(this, this, this);
        private void Call(string name) { Calls.Add(name); OnCall?.Invoke(name); }
        public Task<MergeIssue> CheckAsync(MergePlan plan, CancellationToken cancellationToken)
        { Plan = plan; Call("preflight"); return Task.FromResult(Preflight); }
        public Task<MergeIssue> CheckTargetAsync(MergePlan plan, CancellationToken cancellationToken)
        { Call("target"); return Task.FromResult(Target); }
        public Task<Guid> StageAsync(MergePlan plan, CancellationToken cancellationToken)
        { Call("stage"); return Task.FromResult(EmptyStage ? Guid.Empty : Guid.NewGuid()); }
        public Task<bool> ValidateAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken)
        { Call("validate"); return Task.FromResult(Valid); }
        public Task<PublicationReceipt> PublishNewAsync(Guid stageId, MergePlan plan, CancellationToken cancellationToken)
        { Call("publish"); return Task.FromResult(new PublicationReceipt(PublishStatus, Result)); }
        public Task DiscardStageAsync(Guid stageId, CancellationToken cancellationToken)
        { Assert.IsFalse(cancellationToken.IsCancellationRequested); Call("discard"); return Task.CompletedTask; }
    }
}
