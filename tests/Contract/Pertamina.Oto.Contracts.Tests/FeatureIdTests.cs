using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pertamina.Oto.Contracts.Identifiers;

namespace Pertamina.Oto.Contracts.Tests;

[TestClass]
public sealed class FeatureIdTests
{
    [TestMethod]
    public void Parse_preserves_a_canonical_catalog_identifier()
    {
        var featureId = FeatureId.Parse("FEAT-PDF-005");

        Assert.AreEqual("FEAT-PDF-005", featureId.Value);
        Assert.AreEqual("FEAT-PDF-005", featureId.ToString());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("FEAT-pdf-005")]
    [DataRow("FEAT-PDF-05")]
    [DataRow("FEAT-PDF-0005")]
    [DataRow("FEATURE-PDF-005")]
    [DataRow(" FEAT-PDF-005")]
    public void TryParse_rejects_noncanonical_identifiers(string? candidate)
    {
        var parsed = FeatureId.TryParse(candidate, out var featureId);

        Assert.IsFalse(parsed);
        Assert.AreEqual(default, featureId);
    }
}
