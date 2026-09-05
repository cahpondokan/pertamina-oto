using System.Security.Cryptography;
using System.Text;

namespace Pertamina.Oto.Foundation.UnitTests;

[TestClass]
[TestCategory("Bootstrap")]
public sealed class EvidenceFingerprintSmokeTests
{
    private const string KnownSha256ForAbc =
        "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";

    [TestMethod]
    [TestProperty("TestId", "UT-BOOT-005-001")]
    [TestProperty("DecisionId", "TECH-DEC-013")]
    public void SyntheticEvidencePayload_HasKnownDeterministicSha256Fingerprint()
    {
        byte[] payload = Encoding.UTF8.GetBytes("abc");

        string actual = Convert.ToHexString(SHA256.HashData(payload));

        Assert.AreEqual(KnownSha256ForAbc, actual);
    }

    [TestMethod]
    [TestProperty("TestId", "UT-BOOT-005-002")]
    [TestProperty("DecisionId", "TECH-DEC-013")]
    public void ChangedSyntheticEvidencePayload_ChangesFingerprint()
    {
        byte[] baseline = Encoding.UTF8.GetBytes("bootstrap-evidence-v1");
        byte[] changed = Encoding.UTF8.GetBytes("bootstrap-evidence-v2");

        string baselineFingerprint = Convert.ToHexString(SHA256.HashData(baseline));
        string changedFingerprint = Convert.ToHexString(SHA256.HashData(changed));

        Assert.AreNotEqual(baselineFingerprint, changedFingerprint);
    }
}
