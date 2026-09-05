# P2-W0A-A1-HARNESS verification

Baseline: 5769ccce917d03905d07a1440ee527cb653c9cc4.
Branch: task/P2-A1-harness. Date: 2026-09-05.
Scope: provider-neutral preparation only; zero additional cost.

Commands run from assigned worktree (paths below are relative for sanitization):

| Command/check | Result |
|---|---|
| dotnet --version | 10.0.400; exit 0 |
| dotnet restore poc/harness/Harness.csproj --locked-mode --configfile eng/poc/NuGet.Offline.Config | PASS, exit 0, no package sources; empty net10.0 lock graph |
| dotnet build poc/harness/Harness.csproj --no-restore -c Release | PASS, exit 0; 0 warnings/errors |
| dotnet poc/harness/bin/Release/net10.0/Harness.dll self-test | PASS, exit 0; DISCOVERED=20 PASSED=20 FAILED=0 |
| dotnet poc/harness/bin/Release/net10.0/Harness.dll manifest | PASS, exit 0; allowlisted JSON only |
| dotnet poc/harness/bin/Release/net10.0/Harness.dll execute-provider | Expected rejection, exit 2 |
| PackageReference/ProjectReference inspection | None |
| Network/process-launch/delete API and identity collection source scan | None in implementation |
| Protected product source/config/solution diff | Empty, exit 0 |
| git diff --check | PASS, exit 0 |

Manifest observation: OS 10.0.26200.0, X64 OS/process, runtime 10.0.11,
16 logical processors. NetworkMode=DISABLED_REQUIRED,
NetworkDisabledVerified=false, ProcessingAuthorized=false.
CPU/RAM/storage measurement remains pending; no performance pass claim.

Self-tests include invalid digest/sequence, deterministic identity, path escape,
absolute/root paths, malformed run identity, trailing-dot/alternate-stream names,
queued/running/validation cancellation, timeout, failure, invalid result,
premature/late completion, terminal state preservation and evidence allowlisting.
No files or child processes are created by self-tests. Existing reparse ancestor
validation is implemented but a real junction/symlink test remains unexecuted.

No document bytes, passwords, production data, machine/user names or absolute
paths are stored in this evidence. No package/tool install or candidate execution.
The build uses local SDK and empty sources; this is not OS-network-disabled proof.
Initial sandbox filesystem denial was resolved by scoped command escalation;
no ACL/global Git config or system setting was changed.

Limitations: no actual watchdog/job object/cleanup publisher; design documented
in poc/harness/README.md. No hostile suite, full environment manifest, independent
oracle, candidate admission, or Phase 2/engine exit claim. Agent 0 must audit and
bind integrated evidence to its final commit before any later processing.
