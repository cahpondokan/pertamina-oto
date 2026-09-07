# Wave 0E Agent 1 verification

Date: 2026-09-07. Task: P2-W0E-A1-POSTRESTART-VERIFIER.
Baseline: 136060dc5f50eb6a39a774925c2909f63572597e.
Branch: task/P2-A1-harness. Initial worktree clean.

- `pwsh -NoProfile -File poc/environment/windows-sandbox/postrestart/Verify-PostRestart.ps1 -SelfTest`: exit 0; DISCOVERED=20 PASSED=20 FAILED=0; HOST_PROBES=0.
- Same script without mode: expected exit 2; no observation.
- Same script with both `-Observe -SelfTest`: expected exit 2; no observation.
- Mutation/download/write/launch command source scan: PASS.
- Diff outside new postrestart directory: empty, exit 0.

Twenty cases cover complete observations with all authority flags false, each of
six checks FAIL/UNKNOWN/untrusted text, missing fields and extra secret canary.
Two CLI negative cases prove default/conflicting modes do not enter observation.
Output contains only statuses and fixed schema fields. No exception/registry/path
values are exported. Self-tests never invoke Read-HostChecks.

No host observations, restart, Sandbox launch, system/security change, build,
restore, download, paid component, provider or document processing performed.
No build required for PowerShell/Markdown. No resource or OS-offline proof claimed.
Real cmdlet integration and post-restart checks remain unexecuted as required by
the task card; mocks prove evaluator behavior, not Windows API success.

Evidence is this report and console output in the assigned task. Agent 0 owns
final commit-bound evidence integration. Exact later unexecuted command and
limitations are in README.md. Stop before launch regardless of observed state.
