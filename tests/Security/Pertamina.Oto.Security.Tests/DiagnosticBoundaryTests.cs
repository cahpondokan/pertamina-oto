using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Observability;
namespace Pertamina.Oto.Security.Tests;

[TestClass]
public sealed class DiagnosticBoundaryTests
{
    [TestMethod]
    public void Sensitive_canaries_cannot_enter_diagnostic_schema()
    {
        foreach (var candidate in new[] { @"C:\synthetic\document.pdf", "synthetic document content", "password=test-only", "Bearer synthetic-token", "provider stack trace", "OCR synthetic text" })
        {
            Assert.IsFalse(CorrelationId.TryParse(candidate, out _));
            var error = Assert.ThrowsExactly<FormatException>(() => CorrelationId.Parse(candidate));
            Assert.IsFalse(error.Message.Contains(candidate, StringComparison.Ordinal));
        }
        var constructor = typeof(SafeDiagnostic).GetConstructors().Single();
        CollectionAssert.AreEqual(new[] { typeof(CorrelationId), typeof(DiagnosticCode) }, constructor.GetParameters().Select(p => p.ParameterType).ToArray());
        var sink = typeof(ISafeDiagnosticSink).GetMethods().Single();
        Assert.AreEqual(typeof(SafeDiagnostic), sink.GetParameters().Single().ParameterType);
        var diagnostic = new SafeDiagnostic(CorrelationId.New(), DiagnosticCode.OperationFailed);
        Assert.AreEqual("code,correlationId,schemaVersion", string.Join(",", diagnostic.ToFields().Keys.Order(StringComparer.Ordinal)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SafeDiagnostic(CorrelationId.New(), (DiagnosticCode)999));
        Assert.ThrowsExactly<ArgumentException>(() => new SafeDiagnostic(default, DiagnosticCode.OperationFailed));
    }
    [TestMethod]
    public void Every_missing_denied_or_unknown_gate_denies_authorization()
    {
        var boundary = new DenyByDefaultAuthorizationBoundary();
        Assert.IsFalse(boundary.Evaluate(null).IsAllowed);
        Assert.IsFalse(default(AuthorizationDecision).IsAllowed);
        for (var index = 0; index < 8; index++)
            foreach (var denied in new[] { AuthorizationGate.Unknown, AuthorizationGate.Denied, (AuthorizationGate)999 })
            {
                var gates = Enumerable.Repeat(AuthorizationGate.Satisfied, 8).ToArray();
                gates[index] = denied;
                Assert.IsFalse(boundary.Evaluate(new(gates[0], gates[1], gates[2], gates[3], gates[4], gates[5], gates[6], gates[7])).IsAllowed);
            }
        Assert.IsTrue(boundary.Evaluate(new(AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied, AuthorizationGate.Satisfied)).IsAllowed);
    }
}
