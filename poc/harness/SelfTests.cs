using System.Text.Json;

namespace Oto.Poc.Harness;

internal static class SelfTests
{
    public static int Run()
    {
        var commit = new string('a', 40);
        var digest = new string('b', 64);
        string Id(int repeat) => RunIdentity.Create(commit, digest, digest, digest, repeat);
        var root = Path.Combine(Path.GetTempPath(), "oto-poc-validation-only");
        var tests = new (string Id, Action Body)[]
        {
            ("H01", () => Check(Id(1) == Id(1))),
            ("H02", () => Check(Id(1) != Id(2))),
            ("H03", () => Reject(() => RunIdentity.Create("bad", digest, digest, digest, 1))),
            ("H04", () => Reject(() => Id(0))),
            ("H05", () => Check(OwnedPath.Validate(root, Id(1), "temp/result.bin").Contains(Id(1), StringComparison.Ordinal))),
            ("H06", () => Reject(() => OwnedPath.Validate(root, Id(1), "../escape"))),
            ("H07", () => Reject(() => OwnedPath.Validate(root, Id(1), Path.GetFullPath(root)))),
            ("H08", () => Reject(() => OwnedPath.Validate(Path.GetPathRoot(root)!, Id(1), "x"))),
            ("H09", () => Reject(() => OwnedPath.Validate(root, "../bad", "x"))),
            ("H10", () => Reject(() => OwnedPath.Validate(root, Id(1), "x./y"))),
            ("H11", () => { var r = new Lifecycle(); r.Stop(RunState.Cancelled); Reject(r.Start); Check(!r.PublicationAllowed); }),
            ("H12", () => { var r = new Lifecycle(); r.Start(); r.Stop(RunState.TimedOut); Reject(r.BeginValidation); Check(!r.PublicationAllowed); }),
            ("H13", () => { var r = new Lifecycle(); r.Start(); r.BeginValidation(); r.Complete(false); Check(!r.PublicationAllowed); }),
            ("H14", () => { var r = new Lifecycle(); Reject(() => r.Complete(true)); }),
            ("H15", () => { var r = new Lifecycle(); r.Start(); r.BeginValidation(); r.Complete(true); r.Stop(RunState.Cancelled); Check(r.State == RunState.Completed); }),
            ("H16", () => { var r = new Lifecycle(); r.Start(); var f = r.Stop(RunState.Failed); Check(f.CleanupStatus == "RECONCILIATION_REQUIRED" && !f.PublicationAllowed); }),
            ("H17", () => { var m = EnvironmentManifest.Collect(); Check(!m.NetworkDisabledVerified && !m.ProcessingAuthorized); }),
            ("H18", () => { var json = JsonSerializer.Serialize(EnvironmentManifest.Collect()); Check(!json.Contains(root, StringComparison.Ordinal) && !json.Contains("UserName", StringComparison.Ordinal) && !json.Contains("MachineName", StringComparison.Ordinal)); }),
            ("H19", () => Reject(() => OwnedPath.Validate(root, Id(1), "x:stream"))),
            ("H20", () => { var r = new Lifecycle(); r.Start(); r.BeginValidation(); r.Stop(RunState.Cancelled); Reject(() => r.Complete(true)); }),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            try { test.Body(); Console.WriteLine(test.Id + " PASS"); }
            catch (Exception) { failures++; Console.WriteLine(test.Id + " FAIL"); }
        }
        Console.WriteLine($"DISCOVERED={tests.Length} PASSED={tests.Length - failures} FAILED={failures}");
        return tests.Length > 0 && failures == 0 ? 0 : 1;
    }
    private static void Check(bool value) { if (!value) throw new InvalidOperationException("ASSERTION_FAILED"); }
    private static void Reject(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("EXPECTED_REJECTION");
    }
}
