# Post-restart verifier — P2-W0E-A1-POSTRESTART-VERIFIER

Preparation only; ZERO_ADDITIONAL_COST. NO_RESTART_WITHOUT_SEPARATE_APPROVAL.
Baseline 136060dc5f50eb6a39a774925c2909f63572597e; task/P2-A1-harness.
Trace: PRE-006, PRE-009, PRE-012; P2-W0-ENV-002; P2-W0-RESTART-001
(proposed, not approved). No provider, product graph, existing template or central
record is changed.

Self-test command (mock observations only):
`pwsh -NoProfile -File poc/environment/windows-sandbox/postrestart/Verify-PostRestart.ps1 -SelfTest`

Exact later command, NOT EXECUTED in this task:
`pwsh -NoProfile -File poc/environment/windows-sandbox/postrestart/Verify-PostRestart.ps1 -Observe`

Use that command only in a later authorized post-restart verification task. First
obtain separate restart approval; this script neither performs nor proves a restart.
No elevation is attempted. If an observation needs privileges or a missing module,
it remains UNKNOWN. Do not install, repair features, change security or launch
Sandbox to resolve a check. Stop and report to Agent 0.

The observer reads optional feature state; conventional system launcher presence;
Start app registration whose AppID contains WindowsSandbox; CIM hypervisor/CPU
virtualization indicators; three categories of pending-restart indicators; and the
existing static template validator. Queries run sequentially, not as candidate
processes. No document processing, package/tool acquisition or fee is involved.
Windows built-in observation cmdlets may be unavailable in PowerShell 7; that
produces UNKNOWN, not a silent pass or an automatic compatibility/elevation action.

Result schema allowlists PASS/FAIL/UNKNOWN for six checks. Paths, Start app names,
registry data, exception text, host/user names and arbitrary fixture fields never
enter output. All authority flags and network proof remain false, including when
all checks pass. Exit 0 means observations complete for REVIEW; exit 3 means blocked;
exit 2 means missing/conflicting mode. Default invocation makes no host queries.

Limitations: launcher existence does not verify its signature or successful boot;
Start app naming may vary and absence stays UNKNOWN; a hypervisor flag is only
observable prerequisite evidence, not complete hardware/guest compatibility.
Pending-reboot keys are indicators, not an exhaustive Windows restart detector.
Template validation is static, not effective network/security enforcement.
No timing/resource threshold is invented. CIM/feature queries may take time; no
watchdog or elevated helper is implemented. This task runs only mocks; actual host
probe coverage and actual post-restart environment remain unverified.

Agent 0 should review and integrate this directory, rerun self-tests, and retain
the new commit binding. Later sanitized JSON can be saved by Agent 0 into its
approved run evidence directory; this verifier itself writes no evidence file.
Stop before any Sandbox launch regardless of result. Remaining gates include
restart authorization, post-restart observation, executable trust, actual guest
controls, dependency/oracle/corpus freeze and separate processing authorization.
