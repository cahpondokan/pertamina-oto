# Wave 0C static verification

Task: P2-W0C-A1-SANDBOX-CONFIG. Date: 2026-09-06.
Baseline: 25beadc0ec0cb01c0219959b5daa66544cccd0de.
Branch: task/P2-A1-harness; initial worktree clean.

Command: `pwsh -NoProfile -File poc/environment/windows-sandbox/Validate-Template.ps1 -SelfTest`
Exit 0: DISCOVERED=15 PASSED=15 FAILED=0.
One valid template plus fourteen negative mutations tested. Rejected network,
writable mapping, clipboard/default, logon command, duplicate settings, real host
path, weakened protected client, enabled audio/video/printer/vGPU, DTD, missing
read-only marker and unexpected attribute. XML external resolution is disabled.
This is a strict project template validator, not a Microsoft schema/runtime test.

Path/secret scan: PASS (no workspace/user host path or credential assignments).
Script mutation/download/launch command scan: PASS.
Protected-path diff against HEAD: empty, exit 0.
No build required: only XML template, PowerShell validator and Markdown.
No package/binary/tool download, Sandbox launch, provider execution, real document,
feature/security mutation, restart, cost or central-record change performed.

Limitations: configuration runtime support and effective controls unverified;
restart approval and post-restart host verification pending. No output host map,
no persistent evidence export implementation, no performance/resource measurement.
Memory ceiling and runtime execution profile require a reviewed later revision.
Network proof remains false; processing and launch authorization remain false.
Agent 0 must review only this task's new directory and bind integrated evidence.
