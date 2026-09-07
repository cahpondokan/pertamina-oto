# Wave0I build-only disposition

2026-09-08, Agent6. Integrated input c9dec05a204e79a4420d556f691e4ea744df2ca2, clean at inspection. This addendum separates build preparation from engine execution; the original review's engine HOLD does not block this build-only route.

**BUILD_ONLY: source review ACCEPTABLE for root execution under existing authorization.** Run the reviewed `poc/wave0i/candidate/Build-Offline.ps1 -Mode Build -OutputDirectory <fresh-private-local-output>` from the integrated checkout using the existing locked SDK and package inventory. Root chooses a new private output within authorized artifact scope, verifies available disk/memory and absence of concurrent writers, retains any partial failure output, and records command, exit, diagnostics and produced manifest. No restore, download, MSBuild, package targets, analyzers, generators, or runner execution is included. This is technical review, not a substitute for any required human authority or separate governance review.

The script verifies 365 installed toolchain files, four package archives and twelve selected DLL members against its fixed lock before work. It copies verified bytes using exclusive creates, runs the existing pinned dotnet runtime plus csc directly with `/noconfig`, `/nostdlib+`, explicit references and a quoted response file, and clears the compiler environment. Package DLLs are metadata references, not invoked package code; the existing trusted SDK/runtime/compiler executes. Compiler metadata parsing remains an exposure, so this disposition is limited to these exact admitted bytes and fixed source. Nothing here approves running MergeSmoke/PDFsharp or processing PDF inputs on the host.

The internal compiler timeout is 60 seconds with tree kill; diagnostic reads have no independent hard size bound and post-kill cleanup is not verified by the script. For this fixed source/toolchain, root should supervise the build process, verify termination on timeout/error, and treat missing completion/manifest as failure. Do not claim hard memory/disk/process quotas. No changes to guest staging or observer are prerequisites for compilation; they remain prerequisites for subsequent engine work. A BUILT_NOT_EXECUTED receipt is the maximum successful build claim.

## Exact bindings and verification

- Build-Offline.ps1 SHA256 `8C455C2D44F916250B07445C85768236E4A071ABAC68FC12D460DD2CBD3EC032`.
- CandidateTools.ps1 SHA256 `41D6F20D5C6893D174E1119DFA0DD0538E0567516A40397F7DC4C8E7EAF45DF4`.
- inputs.lock.json SHA256 `3825D5ACC6A2692C6160B474EB3DDBA2FF4223E1D0FBDE6C32FFC83E7354AD27`.
- Wave0H MergeSmoke.cs SHA256 `BE56FFE0B1304319C9041F032F4591A52D35292A9ED9CCAA53F187126C38A507`.

Compared all prior bound A1/A2/A3 inputs with integrated files: no normalized text content differences; six raw-byte differences are CRLF/LF only. Re-read integrated compiler helper and source. Re-ran integrated Build-Offline.ps1 -Mode Verify: PASS, 365 files, four packages, twelve members; compilerExecuted=false and engineExecuted=false. No build was performed by Agent6. Earlier 8 candidate static tests remain applicable to unchanged normalized source.

## Runtime evidence update

Read docs/project-management/PHASE_02_WAVE_0I_SANDBOX_OBSERVATION.md. First-session exact timestamps imply 336.3605583 seconds, resolving the earlier rounded-second ambiguity; still over 300 seconds. Second session was stopped with exit 0 and empty list at 04:29:39.9646468, approximately 150.30 seconds after start. The documented exact BCL probe and ExitCode 0 support zero non-loopback interfaces at observation time. They do not prove attempted egress blocking or every redirection. Earlier missing query/stop capability is no longer a blocker. Engine staging, clean guest environment, qpdf/Python closure, observer export and bounded supervision remain separate technical work under existing permission.
