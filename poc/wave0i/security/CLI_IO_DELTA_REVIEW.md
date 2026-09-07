# CLI I/O delta review

2026-09-08. Root reports the preceding runtime attempt stalled awaiting inherited-pipe EOF despite no remaining wsb.exe process; exact session 6091f09b-ebfb-42b1-8749-d78b7c207536 stopped at 04:44:32 with empty list; interrupted supervisor ended CLI_FAILED. No actual noargs result or success is claimed. This demonstrates the previously documented I/O limitation; historical source GO is not runtime PASS.

Reviewed Invoke-Noargs.ps1 SHA256 `212C6656CC6A55FCAB568CE832EC1B324AAE259171C9F230DB34DF3F60E4B3C4`. Delta stops awaiting stderr EOF, accepts native exit 0 for connect/stop, and reads JSON lines only until one complete value. Strict callers still validate inventory/receipt. All GetResult calls are now preceded by completion/wait checks; trailing broker I/O and incomplete stderr are not certified absent. This is acceptable evidence scope for the fixed trusted CLI, subject to strict parsed receipts and exact cleanup.

**HOLD this intermediate revision:** 64 independent one-second read waits allow up to 64 seconds per JSON call. Start/exec/post-stop inventory can add 192 seconds to the nominal 240-second command/sleep schedule, reaching about 432 seconds before final checks. Replace this with one short aggregate JSON read deadline (for example 5 seconds with per-read remaining time). Keep the line/size limits as additional guards. This is a substantive session-budget defect, not an oracle/permission/package issue. No runtime operation was executed by Agent6.

Waiting for process exit before draining stdout can cause a bounded CLI timeout if the pipe fills; it must remain a failure with recovery rather than a successful result. Async stderr allocation and line allocation are not hard memory quotas. For this fixed tiny CLI protocol these are residual limitations, not authority to process arbitrary output. Source/config/closure are unchanged by this delta; no repeated package audit is needed.

## Focused correction and final disposition

Current Invoke-Noargs.ps1 SHA256 `CCBFD7D174009F75FAD3771B2D6FC98AD268FDB5D934D23F6F46E83B8B07851A` adds a Stopwatch check before each read and rejects elapsed time >=5 seconds. Each read waits at most one second, so aggregate waits can extend to about six seconds, plus local parse/scheduling overhead. The three relevant JSON reads now add about 18 seconds rather than 192 seconds; nominal call/sleep bounds total about 258 seconds, leaving operational cleanup margin before 300. This is not a hard real-time guarantee. AST syntax recheck passed.

**GO for the existing bounded noargs probe on this corrected exact revision**, with the same root operational deadline, exact-ID cleanup, no automatic retry after uncertain start, and strict receipt validation. This supersedes the intermediate HOLD above. Root reports an actual AST-extracted Invoke-Cli list --raw read-only test succeeded with empty inventory at 04:49:16; that is operator-reported evidence, not a guest test or reviewer execution. The source removes the concrete unbounded EOF wait, while incomplete stderr remains explicitly unproven. No package/engine/oracle scope expansion is approved.

Reviewer default Verify rerun also completed exit 0 on the corrected hash: VERIFIED_NOT_EXECUTED, 215 files and unchanged manifest/config/command hashes, one read-only mapping and no writable mappings. No Execute switch was used.
