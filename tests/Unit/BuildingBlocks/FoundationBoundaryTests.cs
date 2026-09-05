using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Observability;
using Pertamina.Oto.Security;

namespace Pertamina.Oto.BuildingBlocks.UnitTests;

[TestClass]
public sealed class FoundationBoundaryTests
{
    private static readonly string[] DiagnosticKeys = ["schemaVersion", "correlationId", "code"];

    [TestMethod]
    public void Correlation_RoundTrip_PreservesOpaqueIdentity()
    {
        var id = CorrelationId.New();
        Assert.IsTrue(id.IsValid);
        Assert.AreEqual(id, CorrelationId.Parse(id.ToString()));
        Assert.AreNotEqual(id, CorrelationId.New());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("00000000000000000000000000000000")]
    [DataRow("synthetic-document-content")]
    [DataRow("C:\\synthetic\\document.pdf")]
    [DataRow("Bearer synthetic-canary")]
    [DataRow("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [DataRow("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n")]
    public void Correlation_InvalidInput_RejectsWithoutEcho(string? candidate)
    {
        Assert.IsFalse(CorrelationId.TryParse(candidate, out var result));
        Assert.IsFalse(result.IsValid);
        var error = Assert.ThrowsExactly<FormatException>(() => CorrelationId.Parse(candidate));
        Assert.AreEqual("Invalid correlation identifier.", error.Message);
    }

    [TestMethod]
    public void Authorization_MissingContextOrDecision_Denies()
    {
        Assert.IsFalse(default(AuthorizationDecision).IsAllowed);
        Assert.IsFalse(new DenyByDefaultAuthorizationBoundary().Evaluate(null).IsAllowed);
    }

    [TestMethod]
    public void Authorization_EveryGateMustBeExplicitlySatisfied_DeniesOtherwise()
    {
        var boundary = new DenyByDefaultAuthorizationBoundary();
        for (int gate = 0; gate < 8; gate++)
        {
            foreach (var failure in new[] { AuthorizationGate.Unknown, AuthorizationGate.Denied, (AuthorizationGate)999 })
            {
                var gates = Enumerable.Repeat(AuthorizationGate.Satisfied, 8).ToArray();
                gates[gate] = failure;
                var context = new AuthorizationContext(gates[0], gates[1], gates[2], gates[3], gates[4], gates[5], gates[6], gates[7]);
                Assert.IsFalse(boundary.Evaluate(context).IsAllowed);
            }
        }
    }

    [TestMethod]
    public void Authorization_AllTrustedGatesSatisfied_Allows()
    {
        var gate = AuthorizationGate.Satisfied;
        Assert.IsTrue(new DenyByDefaultAuthorizationBoundary().Evaluate(
            new AuthorizationContext(gate, gate, gate, gate, gate, gate, gate, gate)).IsAllowed);
    }

    [TestMethod]
    public void Diagnostic_InvalidIdentifierOrCode_Rejects()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new SafeDiagnostic(default, DiagnosticCode.AccessDenied));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SafeDiagnostic(CorrelationId.New(), (DiagnosticCode)0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SafeDiagnostic(CorrelationId.New(), (DiagnosticCode)999));
    }

    [TestMethod]
    public void Diagnostic_Projection_IsClosedAndImmutable()
    {
        var id = CorrelationId.New();
        foreach (var code in Enum.GetValues<DiagnosticCode>())
        {
            var fields = new SafeDiagnostic(id, code).ToFields();
            CollectionAssert.AreEquivalent(DiagnosticKeys, fields.Keys.ToArray());
            Assert.AreEqual("1", fields["schemaVersion"]);
            Assert.AreEqual(id.ToString(), fields["correlationId"]);
            Assert.AreEqual(code.ToString(), fields["code"]);
            Assert.ThrowsExactly<NotSupportedException>(() => ((IDictionary<string, string>)fields).Add("path", "synthetic"));
        }
    }

    [TestMethod]
    public void Diagnostic_PublicInputSurface_HasNoFreeTextOrException()
    {
        var parameters = typeof(SafeDiagnostic).GetConstructors().SelectMany(c => c.GetParameters()).ToArray();
        Assert.AreEqual(2, parameters.Length);
        Assert.IsTrue(parameters.All(p => p.ParameterType == typeof(CorrelationId) || p.ParameterType == typeof(DiagnosticCode)));
        Assert.IsTrue(typeof(SafeDiagnostic).GetProperties().All(p => p.SetMethod is null));
    }
}
