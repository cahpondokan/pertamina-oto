using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Modules.Pdf.Contracts;
namespace Pertamina.Oto.Modules.Pdf.ContractTests;

[TestClass]
public sealed class MergeContractTests
{
    [TestMethod]
    public void Command_MutableCallerCollections_AreSnapshotted()
    {
        var inputs = new[] { new InputOccurrence(Guid.NewGuid(), Guid.NewGuid()) };
        var original = inputs[0];
        var order = new[] { original.OccurrenceId };
        var command = new MergeCommand(inputs, order, Guid.NewGuid());
        inputs[0] = new(Guid.NewGuid(), Guid.NewGuid());
        order[0] = Guid.NewGuid();
        Assert.AreEqual(original, command.Inputs[0]);
        Assert.AreEqual(original.OccurrenceId, command.OrderedOccurrenceIds[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<Guid>)command.OrderedOccurrenceIds)[0] = Guid.NewGuid());
    }

    [TestMethod]
    public void Outcome_NonFinalStates_ContainNoResultOrMutableState()
    {
        var correlation = Guid.NewGuid();
        Assert.IsNull(MergeOutcome.Cancelled(correlation).ResultId);
        Assert.IsNull(MergeOutcome.Failure(MergeIssue.ProcessingFailed, correlation).ResultId);
        Assert.IsTrue(typeof(MergeOutcome).GetProperties().All(p => p.SetMethod is null));
        Assert.Throws<ArgumentException>(() => MergeOutcome.Completed(correlation, Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => MergeOutcome.Failure(MergeIssue.None, correlation));
    }

    [TestMethod]
    public void Contracts_Reflection_ExposeNoRawPathContentOrServerRoute()
    {
        var properties = new[] { typeof(MergeCommand), typeof(InputOccurrence), typeof(MergeOutcome) }
            .SelectMany(t => t.GetProperties()).ToArray();
        Assert.IsFalse(properties.Any(p => p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[])));
        Assert.IsFalse(properties.Any(p => p.Name.Contains("Job", StringComparison.Ordinal) ||
            p.Name.Contains("Upload", StringComparison.Ordinal) || p.Name.Contains("Server", StringComparison.Ordinal)));
    }
}
