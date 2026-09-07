# Exact assembled loader review

2026-09-08, Agent6. BUILD_ONLY disposition remains acceptable. Reviewed root's uncommitted operator files in integration after A1 fix c06c85b. **Loader execution HOLD for response-schema and start-uncertainty handling below; payload/config/source verification PASS.** No renewed human authority is required.

## Inputs and actual read-only verification

- Guest-Noargs.ps1 SHA256 `1A27F81C0BDDEFE965D396BCC9F67D451249D9E55A63A494E188FEE0FC411787`.
- Invoke-Noargs.ps1 SHA256 `7F2754E87948F0BF256982C38F625088CFF0FDB73929599100E0059368CD8C10`.
- Built manifest SHA256 `C5F2CE96F35B5790A4D4D862ACA457AD65411A4143AE90EB011502928B59543D`.
- Runner SHA256 `7EF30795356FE694D23C39DAB671119EDB07295CCA4D36B83911CDA6CEC0BB15`.
- Generated config SHA256 `2DD70287CAB5113D7F3E30008B9CC61734DFB19341C63E7D3FACC08392EB4CF7`.
- Encoded guest command SHA256 `57ABFF99F23465506FF800B0890D15231AA5E7B6FA2C30C51C1B78B5B36DF42B`.

Ran Invoke-Noargs.ps1 with no Execute switch: exit 0, VERIFIED_NOT_EXECUTED, all 215 staged files matched manifest, one read-only mapping and zero writable mappings. Read-only access initially denied by private ACL; scoped escalation succeeded. Get-Acl of the dedicated stage reports current user, SYSTEM and Administrators FullControl only. This inspects the root directory ACL; it is not a recursive security-descriptor attestation. No Sandbox, compiler, loader or PDF command was executed by Agent6.

Guest source checks zero non-loopback interfaces, clears child environment, uses fixed mapped dotnet/runner paths, supplies no application arguments, and explicitly propagates the child's exit code. Network failure codes 21/22 and timeout 23 are distinct from expected 2. Outer exact-session close is still required; direct child Kill is not a hard process-tree guarantee. Config disables networking and redirections and maps only the private stage read-only. A1's current module supplies the previously missing clean-environment helper and bundle manifest adapter; this loader uses its own explicit minimal environment and manifest directly.

## Concrete defects before Execute

1. Before/after CLI inventory handling reads `.WindowsSandboxEnvironments` without validating JSON shape or required collection. Missing property/null can appear empty, falsely admitting a launch or certifying session closure. Freeze the installed CLI response shape, reject malformed/null/missing inventory, validate each session ID and require an actual valid post-stop inventory. Similarly require an integer ExitCode exactly 2; avoid PowerShell string/numeric coercion as evidence validation.
2. If start times out or returns no usable ID, `$session` stays null and finally skips closure. Report explicit recovery-required with the uncertain start retained; root must resolve actual guest inventory/absence before retry. Do not select/stop an arbitrary session. Bound recovery and retain the start failure instead of treating it as evidence of no guest.

CLI WaitForExit calls have deadlines but redirected ReadToEndAsync/GetResult is not independently deadline/size bounded; size is checked after allocation. With the existing trusted CLI and fixed commands this is a residual supervision limitation, not hard containment. Root should retain the outer operational deadline/fallback and verify exact-session cleanup if command I/O stalls. No qpdf, Python, oracle or writable export is required to resolve these defects or run this noargs probe.

The above exact revision is not yet approved for Execute. A small source re-review of the response/recovery fix is sufficient; do not repeat build admission or unrelated historical audits. Result label remains NOARGS_EXIT_2 only, not PDF processing/fidelity or full engine closure evidence.
