# Agent6 Wave0I bounded security review

2026-09-08. Baseline f6fa411bdc5415099d6a74ea356a5faab224df1f. Source/static review only. Existing P2-W0H-EXEC-001 authorization continues; no renewed human permission is requested. Root remains sole operator. No compiler, PDF engine, generator, or Sandbox was launched by this review.

## Verdict

Preparation tests PASS; integrated engine execution remains technically HOLD pending the concrete interface fixes below. A lightweight no-map benign session is a separate profile that root can prepare and inspect under existing authority. This is not Phase2 completion, engine admission, effective isolation proof, or Production approval.

## Current-session evidence and bounded retry

Agent1 records the root report: launch 04:19:36, visible desktop 04:20:45, no logon summary, exact-session stop 04:25:13 and empty session list. This is reported boot/close evidence, not independently observed here. Timestamp subtraction is 337 seconds; root's stated 5m36 differs by one second. Both exceed the planned 300 seconds. Record the deviation, not a timing PASS. No engine was reported run. Reported free memory improved from the historical 615 MiB hold to 6088 MiB before and 4866 MiB after; these snapshots need refreshing at the next launch.

For a no-map, no-engine observation, 2048 MiB is a reasonable provisional lower resource budget, subject to fresh host headroom/commit measurements and a frozen explicit configuration. It is not a tested minimum or engine budget. A 4096 MiB guest against 4866 MiB available leaves only about 770 MiB arithmetic margin before other host costs; do not infer sufficient resource from that comparison alone. A1's generated 4096 MiB mapped profile is not the proposed no-map profile. Freeze and review the actual alternate XML separately; retain disabled networking and redirections. Do not change host security settings.

Prepare CLI discovery/help and command construction before starting the guest clock. Start using explicit XML text via `--config`, capture and validate the returned session ID, bind it to the run, and use that ID for connect/exec/stop. Use ExistingLogin only after a connected login; do not silently switch to System. Put exact-session stop in the outer timeout/finally route and reserve cleanup time before the five-minute boundary. Bound CLI calls too: a blocking start/exec/stop must not defeat the watchdog. Confirm that exact ID is absent or stopped; do not stop unrelated sessions or shared services. If a start times out without an ID, resolve that uncertainty before a retry.

Microsoft documents XML-string configuration, ID-specific control, and exit-code-only exec without process I/O retrieval. Therefore CLI control does not itself supply guest evidence export. Installed-version help remains a root preflight check. [Windows Sandbox CLI](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-cli).

## Concrete integration findings

Root follow-up during review reports a second explicit-config session started 04:27:09 with 5110 MiB free, session 67461cce-f69d-4142-9a38-ab4a14dc9788. ExistingLogin BCL interface-count probe returned ExitCode 0 at 04:29:17 (zero non-loopback interfaces; no network request). This is useful reported runtime observation and resolves the earlier inability to query the logged-in guest. It is not egress proof. Exact probe source/config/receipt and completed stop timing remain to be bound in root's forthcoming evidence; this review has not independently inspected those bytes. Do not preserve the earlier CLI-query limitation as a current blocker once that evidence is verified.

1. **Staging interface mismatch.** A1's example uses `C:\PocInput\runtime\dotnet.exe`; A2 emits `guest/dotnet/dotnet.exe`. A1 checks `schema:1`; A2 emits `schemaVersion:1`. Root must explicitly adapt the complete final payload manifest and invocation, retaining all member hashes, rejecting extra files, and rechecking the immutable staging. Do not rename a schema field and imply that qpdf/Python/environment/oracle bytes are covered by the candidate-only manifest.
2. **Guest environment contract missing.** A2 requires a clean guest environment for the pinned dotnet host. A1's ProcessStartInfo currently inherits environment variables. Apply the reviewed minimum SystemRoot/WINDIR/private TEMP/TMP and pinned DOTNET settings, clearing inherited injection/probing variables before invoking the candidate. A2's clean compiler environment does not fix the separate guest invocation.
3. **Observer staging and export are not connected.** A3 needs an already available compatible Python runtime, admitted qpdf executable/DLL closure, exact binding digest and pre-merge source ledger. A2's guest bundle covers dotnet/PDFsharp only. A1 exports a small per-case status/hash with correctness NOT_EVALUATED; it does not export/validate A3 result and raw-capture hashes. Implement a bounded allowlisted host consumer and explicit sanitized observer receipt route. Keep PDFs and raw errors quarantined; guest-writable JSON is not authenticated evidence. qpdf version/observation commands do not produce result.pdf and cannot simply reuse A1's merge-success wrapper.
4. **Supervision is best effort.** A1 taskkill tree cleanup and A3 direct-child kill/polling are concrete, but not Job Object quotas or hard process/disk/memory containment. They may be suitable only for the fixed tiny benign corpus with a working outer exact-session deadline and verified cleanup. Do not extend this conclusion to hostile/general documents. A3's two 15-second children plus cleanup/hashing need an explicit outer allowance; use separate bounded invocations if four cases cannot fit the session budget.
5. **Oracle remains bounded and unmeasured.** A3 correctly collects qpdf JSON itself for PDFsharp output, checks source/output/tool digests before and after, and rejects unsupported page/content/font structure. Its strict empty stream dictionary and exact catalog/page profile may reject real PDFsharp output; no runtime compatibility was measured. A rejection is not a correctness failure to bypass by copying expected markers. Structural matches do not prove visual fidelity, UI behavior, full canonical IDs, or general security scanning. Source hashes and the asserted merger identity require independently retained root ledger evidence.

## Verification actually run

From each corresponding fresh worktree:

- `& ./poc/wave0i/environment/Test-Static.ps1`: exit 0, discovered/passed 8, failed 0; no child/PDF.
- `& ./poc/wave0i/candidate/Test-Static.ps1`: exit 0, 8 PASS, compilerExecuted=false, engineExecuted=false, pdfGenerated=false. Includes read-only Verify of existing locked SDK/package/member bytes; no restore/build.
- `C:/Python314/python.exe -B -m unittest discover -s poc/wave0i/oracle -p test_observe.py -v`: exit 0, 28 tests PASS in 0.048 seconds. JSON/text temporary fixtures and mocked/guarded Popen only; no PDF or engine.

Exact source snapshots are in input-bindings.json. Timing is self-test timing only. No source changes were made in other lanes. Root owns integration, fresh resource evidence, final runtime staging/timeout/export verification, and central phase records. Remaining items are technical integration work under existing authorization, not a request for renewed permission.
