using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Versioning;

namespace Pertamina.Oto.Contracts.Tests;

[TestClass]
public sealed class ContractVersionTests
{
    [TestMethod]
    public void Parse_preserves_an_authoritative_opaque_token()
    {
        var version = ContractVersion.Parse("contract-2026.09");

        Assert.AreEqual("contract-2026.09", version.Value);
        Assert.AreEqual("contract-2026.09", version.ToString());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(" v1")]
    [DataRow("v1 ")]
    public void TryParse_rejects_an_ambiguous_token(string? candidate)
    {
        var parsed = ContractVersion.TryParse(candidate, out var version);

        Assert.IsFalse(parsed);
        Assert.AreEqual(default, version);
    }

    [TestMethod]
    public void Equality_is_exact_and_case_sensitive()
    {
        var lowerCase = ContractVersion.Parse("v1");
        var upperCase = ContractVersion.Parse("V1");

        Assert.AreNotEqual(lowerCase, upperCase);
    }
}
