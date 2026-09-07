# Wave 0I Agent 1 integration interface

Baseline f6fa411bdc5415099d6a74ea356a5faab224df1f. Fresh branch task/P2-A1-wave0i.
P2-W0H-EXEC-001 remains valid. Root alone operates after independent audit.
No new permission request; zero cost, no acquisition/restart/product change.

## Root host staging

Root creates a new run-specific directory with exactly input/ and export/ siblings.
Input contains only reviewed existing candidate runtime bytes from Agent 2,
synthetic fixtures from Agent 3, and environment scripts under environment/.
Do not map repository roots, user directories, drives or actual documents.
Export must be a new empty quarantine directory, never a sync/watched folder.
These paths are root's run-owned paths; helpers do not choose or create them.
Manifest stays OUTSIDE input: JSON {schema:1,files:[{path:'environment/Environment.psm1',sha256:'UPPERCASE_SHA256'}]}.
Supply one exact relative path/hash per file, no extras; no examples are defaults.

From integrated worktree:
1. `pwsh -NoProfile -File poc/wave0i/environment/Test-Staging.ps1 -InputRoot $inputRoot -ManifestPath $manifestPath`
2. `Import-Module ./poc/wave0i/environment/Environment.psm1`
3. `New-SmokeConfiguration -InputRoot $inputRoot -ExportRoot $exportRoot | Set-Content -LiteralPath $configPath -Encoding utf8`
4. Root hashes rendered config/manifest and launches after review; module never launches.

Root validates ACLs/ownership and host free resources immediately before use.
Checks reject reparse points and extra files but are not race-free handle security.
Keep staging immutable during the session and recheck digests afterward. Input is
mapped read-only C:\PocInput. Output is private C:\PocPrivate\CASE; mapping
C:\PocExport exposes only the dedicated empty export directory as writable.
This writable mapping is a deliberate proposal for machine-readable export,
requiring integrated security review; it does not expose pre-existing host data.
It is not write-only or hard containment: a compromised guest can write arbitrary
bytes there. Never auto-open/import contents. No logon auto-execution is configured.

## Root guest invocation

Use built-in Windows PowerShell, no pwsh installation needed:
`powershell.exe -NoProfile -File C:\PocInput\environment\Invoke-GuestChild.ps1 -CaseId AB -Executable C:\PocInput\runtime\dotnet.exe -ExecutableSha256 HASH_FROM_FROZEN_MANIFEST -Arguments 'C:\PocInput\app\MergeSmoke.dll result.pdf C:\PocInput\corpus\A.pdf C:\PocInput\corpus\B.pdf' -TimeoutSeconds 30`

Argument order matches baseline MergeSmoke (output first); reconcile asset paths with Agent 2 staging;
the wrapper accepts trusted root-reviewed arguments, not arbitrary user content.
qpdf can use the same wrapper with its frozen executable/hash and reviewed args.
Run AB/CAB/AA/AC sequentially, one child per case. Duplicate case directories are
rejected. No shell interpretation; UseShellExecute=false. All stdout remains in
guest console, not exported. Observer integration is separate: Agent 3 consumes
guest-private output and prepares its independent record; this wrapper does not
claim correctness or reopen. A missing result becomes FAILED.

## Timeout, export and close

Default child deadline30s, configurable1–60s; timeout invokes built-in taskkill
for that exact child PID with /T /F; wait5s for killer and5s for child. Failure to
confirm returns exit4 and tells root to close guest. Parent exit does not prove
all descendants stopped; taskkill is best-effort and PID races remain. No Job
Object, memory quota or disk quota is implemented. Reject hostile/unbounded input.
4096MB config is a smoke budget, not a PDF acceptance threshold. 1MiB output check
is post-write only. Root stops the exact guest via session-bound CLI (below), or UI, within five minutes total,
or immediately on timeout/error. Never kill shared host services or restart host.
Retain failure evidence; guest disposal is not application cleanup proof.

Wrapper exports CASE.json with fixed fields, uppercase digest, status/exit/duration;
correctness remains NOT_EVALUATED. Zero digest denotes no accepted output hash.
Root accepts ONLY four named JSON files (AB/CAB/AA/AC), each <=4096 bytes, regular
non-reparse files; reject unknown fields/statuses, missing/duplicate/empty records,
unexpected bytes/files and any content/path/credential fields. Quarantine all
other files without executing them. Independently validate observer records and
source/output digests before evidence promotion. Directory must remain private
until review and guest closure. No PDF bytes exported by this wrapper.

Current tests only exercise pure JSON schema rejection and syntax; no child,
engine, PDF generation/build/restore or Sandbox launch. Runtime staging, actual
timeout/tree behavior and machine export require root's controlled test. Security
review must decide whether best-effort supervision suffices for tiny benign smoke;
this implementation does not claim full POC-PDF-001 containment or exit PASS.

## CLI session control update from operator

Operator report 2026-09-08: launch04:19:36, visible guest04:20:45, no logon summary;
exact-session CLI stop04:25:13 followed by empty session list. This is reported
boot/close evidence only; effective configuration and logon execution unproven.
Existing per-user wsb package0.8.107.0 was found by root; no acquisition required.

Use the existing root-resolved executable as $wsb (do not persist user path):
`& $wsb start --config (Get-Content -LiteralPath $configPath -Raw) --raw`
Capture the returned session ID, validate GUID shape, bind to this run and never
select an arbitrary session from the list. Configure explicitly, never default start.
`& $wsb connect --id $sessionId`
`& $wsb exec --id $sessionId --command $reviewedGuestCommand --run-as ExistingLogin`
`& $wsb stop --id $sessionId`
`& $wsb list --raw`

Root binds stop in its supervisory finally/timeout path; confirm this ID absent
or stopped afterward. Do not stop unrelated sessions. If control CLI stalls,
UI-close remains fallback; never kill shared host services. ExistingLogin requires
an active connected guest; do not silently switch to System. Root should inspect
installed command help for argument handling before constructing guest commands.
The start config argument is XML TEXT, not a filename. Preserve the reviewed
Networking value Disable from configuration documentation; CLI documentation's
Disabled example differs and must not silently replace the frozen profile.

CLI exec reports exit code but documentation says it cannot retrieve process I/O.
Therefore it does not remove the JSON quarantine export requirement. Session stop
adds an outer containment response, not per-child Job Object or PDF correctness
proof. This lane has not run any CLI start/exec/stop command.

Source checked2026-09-08: https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-cli
