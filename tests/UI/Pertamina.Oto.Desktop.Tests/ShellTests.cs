using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Modules.Pdf.Contracts;
namespace Pertamina.Oto.Desktop.Tests;

[TestClass]
public sealed class ShellTests
{
    private static MergeCommand Command() => new([], [], Guid.NewGuid(), CorrelationId.New());
    private sealed class Fake(Func<MergeCommand, CancellationToken, Task<MergeOutcome>> execute) : IMergeService
    {
        public Task<MergeOutcome> ExecuteAsync(MergeCommand command, CancellationToken cancellationToken = default) => execute(command, cancellationToken);
    }
    [TestMethod]
    public async Task Entries_SameCommand_ConvergeWithoutDomainValidation()
    {
        var command = Command();
        var calls = new List<MergeCommand>();
        var shell = new MergeShell(new Fake((c, _) => { calls.Add(c); return Task.FromResult(MergeOutcome.Failure(MergeIssue.InvalidCommand, c.CorrelationId)); }));
        foreach (var entry in Enum.GetValues<EntryFlow>()) await shell.ExecuteAsync(entry, command);
        Assert.HasCount(2, calls);
        Assert.IsTrue(calls.All(c => ReferenceEquals(c, command)));
        Assert.AreEqual(MergeIssue.InvalidCommand, shell.State.Issue);
    }
    [TestMethod]
    public async Task Cancel_Request_DoesNotOverrideCommittedCompletion()
    {
        var pending = new TaskCompletionSource<MergeOutcome>();
        var command = Command();
        CancellationToken token = default;
        var shell = new MergeShell(new Fake((_, t) => { token = t; return pending.Task; }));
        var execution = shell.ExecuteAsync(EntryFlow.FileFirst, command);
        Assert.AreEqual(ShellPhase.Running, shell.State.Phase);
        Assert.IsNull(shell.State.Percentage);
        shell.Cancel();
        Assert.IsTrue(token.IsCancellationRequested);
        Assert.AreEqual(ShellPhase.Running, shell.State.Phase);
        Assert.IsFalse(shell.State.CanCancel);
        pending.SetResult(MergeOutcome.Completed(command.CorrelationId, Guid.NewGuid()));
        await execution;
        var terminal = shell.State;
        shell.Cancel();
        Assert.AreSame(terminal, shell.State);
        Assert.AreEqual(ShellPhase.Completed, terminal.Phase);
    }
    [TestMethod]
    public async Task ConcurrentStart_ActiveOperation_IsRejected()
    {
        var pending = new TaskCompletionSource<MergeOutcome>();
        var command = Command();
        var shell = new MergeShell(new Fake((_, _) => pending.Task));
        var execution = shell.ExecuteAsync(EntryFlow.ToolFirst, command);
        await Assert.ThrowsAsync<InvalidOperationException>(() => shell.ExecuteAsync(EntryFlow.FileFirst, command));
        pending.SetResult(MergeOutcome.Cancelled(command.CorrelationId));
        await execution;
        Assert.AreEqual(ShellPhase.Cancelled, shell.State.Phase);
    }
    [TestMethod]
    public async Task CancellationException_Requested_ProjectsCancelled()
    {
        var pending = new TaskCompletionSource<MergeOutcome>();
        var shell = new MergeShell(new Fake((_, _) => pending.Task));
        var execution = shell.ExecuteAsync(EntryFlow.ToolFirst, Command());
        shell.Cancel();
        pending.SetCanceled();
        await execution;
        Assert.AreEqual(ShellPhase.Cancelled, shell.State.Phase);
    }
    [TestMethod]
    public async Task Outcomes_FailureUnavailableAndForeignCorrelation_AreSafe()
    {
        foreach (var issue in new[] { MergeIssue.Unavailable, MergeIssue.ProcessingFailed })
        {
            var shell = new MergeShell(new Fake((c, _) => Task.FromResult(MergeOutcome.Failure(issue, c.CorrelationId))));
            await shell.ExecuteAsync(EntryFlow.ToolFirst, Command());
            Assert.AreEqual(issue == MergeIssue.Unavailable ? ShellPhase.Unavailable : ShellPhase.Failed, shell.State.Phase);
            Assert.IsNull(shell.State.ResultId);
        }
        var foreign = new MergeShell(new Fake((_, _) => Task.FromResult(MergeOutcome.Completed(CorrelationId.New(), Guid.NewGuid()))));
        await foreign.ExecuteAsync(EntryFlow.FileFirst, Command());
        Assert.AreEqual(ShellPhase.Failed, foreign.State.Phase);
        Assert.IsNull(foreign.State.ResultId);
    }
    [TestMethod]
    public async Task Exception_SensitiveMessage_IsNotExposed()
    {
        var shell = new MergeShell(new Fake((_, _) => throw new InvalidOperationException("SENSITIVE-CANARY")));
        await shell.ExecuteAsync(EntryFlow.FileFirst, Command());
        Assert.AreEqual(ShellPhase.Failed, shell.State.Phase);
        Assert.DoesNotContain("SENSITIVE-CANARY", shell.State.ToString());
    }
    [TestMethod]
    public void Semantics_KeyboardFocusNamesAndScale_AreDeterministic()
    {
        var commands = ShellSemantics.Commands;
        Assert.HasCount(3, commands.Select(c => c.Key).Distinct().ToArray());
        Assert.IsTrue(commands.Select(c => c.FocusOrder).SequenceEqual(Enumerable.Range(0, 3)));
        Assert.IsTrue(commands.All(c => !string.IsNullOrWhiteSpace(c.AccessibleName)));
        Assert.IsFalse(ShellSemantics.IsEnabled(ShellAction.Run, new(ShellPhase.Running)));
        Assert.IsTrue(ShellSemantics.IsEnabled(ShellAction.Cancel, new(ShellPhase.Running)));
        foreach (double scale in new[] { 1, 1.25, 1.5, 2 }) Assert.AreEqual(16 * scale, ShellSemantics.Scale(16, scale));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShellSemantics.Scale(16, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShellSemantics.Scale(16, 0));
    }
}
