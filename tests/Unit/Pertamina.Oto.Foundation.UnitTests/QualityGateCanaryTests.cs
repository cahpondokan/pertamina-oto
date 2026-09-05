namespace Pertamina.Oto.Foundation.UnitTests;

[TestClass]
[TestCategory("Bootstrap")]
public sealed class QualityGateCanaryTests
{
    internal const string CanaryEnvironmentVariable = "OTO_CONTROLLED_NEGATIVE_CANARY";

    [TestMethod]
    [TestCategory("NegativeCanary")]
    [TestProperty("TestId", "UT-BOOT-006-001")]
    [TestProperty("DecisionId", "TECH-DEC-014")]
    public void NormalExecution_DoesNotEnableControlledNegativeCanary()
    {
        string? canaryValue = Environment.GetEnvironmentVariable(CanaryEnvironmentVariable);

        Assert.IsTrue(
            string.IsNullOrEmpty(canaryValue),
            "Controlled negative canary was enabled; this intentional failure must block the gate.");
    }
}
