using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oto.Poc.Harness;

internal static class RunIdentity
{
    public static string Create(string commit, string environmentDigest, string corpusDigest, string configDigest, int repetition)
    {
        if (commit.Length != 40 || !commit.All(Uri.IsHexDigit) || repetition < 1 ||
            new[] { environmentDigest, corpusDigest, configDigest }.Any(x => x.Length != 64 || !x.All(Uri.IsHexDigit)))
            throw new ArgumentException("INVALID_RUN_BINDING");
        var binding = string.Join('|', commit.ToUpperInvariant(), environmentDigest.ToUpperInvariant(),
            corpusDigest.ToUpperInvariant(), configDigest.ToUpperInvariant(), repetition.ToString(CultureInfo.InvariantCulture));
        return "RUN-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(binding)));
    }
}

internal sealed record EnvironmentManifest(int SchemaVersion, string OsVersion, string OsArchitecture,
    string ProcessArchitecture, string RuntimeVersion, int LogicalProcessorCount, string NetworkMode,
    bool NetworkDisabledVerified, bool ProcessingAuthorized, string ResourceStatus)
{
    public static EnvironmentManifest Collect() => new(1, Environment.OSVersion.Version.ToString(),
        RuntimeInformation.OSArchitecture.ToString(), RuntimeInformation.ProcessArchitecture.ToString(),
        Environment.Version.ToString(), Environment.ProcessorCount, "DISABLED_REQUIRED", false, false,
        "CPU_RAM_STORAGE_BASELINE_PENDING");
}

// Advisory validation only: intentionally no file mutation or recursive deletion API.
internal static class OwnedPath
{
    public static string Validate(string approvedRoot, string runId, string relativePath)
    {
        if (runId.Length != 68 || !runId.StartsWith("RUN-", StringComparison.Ordinal) || !runId[4..].All(Uri.IsHexDigit))
            throw new ArgumentException("INVALID_RUN_ID");
        if (!Path.IsPathFullyQualified(approvedRoot) || approvedRoot.StartsWith("\\\\", StringComparison.Ordinal) ||
            string.Equals(Path.GetPathRoot(approvedRoot), approvedRoot, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("INVALID_ROOT");
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) ||
            relativePath.Split('/', '\\').Any(x => x is "" or "." or ".." || x.EndsWith(' ') || x.EndsWith('.') ||
                x.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            throw new ArgumentException("INVALID_RELATIVE_PATH");
        var root = Path.GetFullPath(approvedRoot);
        if (string.Equals(root, Path.GetPathRoot(root), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("INVALID_ROOT");
        var runRoot = Path.Combine(root, runId);
        var target = Path.GetFullPath(Path.Combine(runRoot, relativePath));
        if (!target.StartsWith(runRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("PATH_ESCAPE");
        for (var cursor = target; cursor is not null; cursor = Path.GetDirectoryName(cursor))
        {
            if ((Directory.Exists(cursor) || File.Exists(cursor)) &&
                (File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("REPARSE_POINT_REJECTED");
        }
        return target;
    }
}

internal enum RunState { Queued, Running, Validating, Completed, Cancelled, TimedOut, Failed }
internal sealed record FailureRecord(RunState State, string CleanupStatus, bool PublicationAllowed);
internal sealed class Lifecycle
{
    public RunState State { get; private set; } = RunState.Queued;
    public bool PublicationAllowed => State == RunState.Completed;
    public void Start() { Require(RunState.Queued); State = RunState.Running; }
    public void BeginValidation() { Require(RunState.Running); State = RunState.Validating; }
    public void Complete(bool valid)
    {
        Require(RunState.Validating);
        State = valid ? RunState.Completed : RunState.Failed;
    }
    public FailureRecord Stop(RunState reason)
    {
        if (reason is not (RunState.Cancelled or RunState.TimedOut or RunState.Failed))
            throw new ArgumentException("INVALID_STOP_REASON");
        if (State is RunState.Queued or RunState.Running or RunState.Validating) State = reason;
        return new(State, "RECONCILIATION_REQUIRED", PublicationAllowed);
    }
    private void Require(RunState expected)
    {
        if (State != expected) throw new InvalidOperationException("INVALID_TRANSITION");
    }
}

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "manifest")
        {
            Console.WriteLine(JsonSerializer.Serialize(EnvironmentManifest.Collect()));
            return 0;
        }
        if (args.Length == 1 && args[0] == "self-test") return SelfTests.Run();
        Console.WriteLine("PREPARATION_ONLY: use manifest or self-test; provider execution unavailable.");
        return 2;
    }
}
