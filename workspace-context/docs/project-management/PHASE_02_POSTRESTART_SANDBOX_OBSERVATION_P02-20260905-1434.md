# PERTAMINA OTO — Phase 2 Post-Restart Windows Sandbox Observation

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Observation | 2026-09-07 20:20:57 +07:00 |
| Last boot | 2026-09-07 15:06:05 +07:00 |
| Result | `OBSERVATIONS_COMPLETE_REVIEW_REQUIRED` |
| Cost | `ZERO_ADDITIONAL_COST` |
| Sandbox launch | `NOT_RUN` |
| qpdf/PDFsharp execution, restore/build, PDF processing | `NOT_RUN` |

## Outcome

The separately approved Windows restart completed. Agent 0 then ran only the
prepared read-only post-restart observer and non-mutating system cross-checks.
Windows Sandbox was not launched. No component was installed, restored, built,
loaded, or used to process a PDF.

All six reconciled host-readiness observations are PASS:

| Check | Evidence | Result |
|---|---|---|
| Feature enabled | `Win32_OptionalFeature`: `Containers-DisposableClientVM`, caption `Windows Sandbox`, `InstallState=1` | PASS |
| Launcher present | `C:\Windows\System32\WindowsSandbox.exe`, 1,078,736 bytes | PASS |
| Start entry present | `Windows Sandbox`, AppID `Microsoft.Windows.Containers.Sandbox` | PASS |
| Virtualization observable | `HypervisorPresent=true`; `vmcompute` and `hns` running | PASS |
| No pending restart | verifier registry checks | PASS |
| Template controls | static validator | PASS |

The launcher is version `10.0.26100.8875`, SHA-256
`7C0095F52A1FDED07D90171A005BB495440C92DA099C064E0ED9AD08656AC3FB`,
with a valid Microsoft Windows Authenticode signature.

## Verifier limitation and reconciliation

The prepared verifier itself returned `BLOCKED`: its optional-feature cmdlet could
not read state without administrative elevation, and its Start-app regex looked for
`WindowsSandbox`, while the actual AppID is `Microsoft.Windows.Containers.Sandbox`.
It correctly failed closed and kept all launch/processing/network authority flags
false. Separate read-only CIM and Start-app queries closed these two observations;
the script was not modified in this read-only task.

## Safety boundary

- No `WindowsSandbox`, `qpdf`, `fix-qdf`, or `zlib-flate` process was running at the
  final check.
- No Sandbox launch, qpdf/PDFsharp execution, package restore/build, PDF processing,
  installation, download, or paid service occurred.
- Static template controls do not prove effective guest boot, host-folder boundary,
  or network denial. `NetworkDisabledProven` remains false until a separately
  authorized, supervised launch/containment experiment.
- The one-time restart approval is fulfilled and consumed; it does not authorize
  another restart or component execution.

## Phase disposition

The environment observation gate has advanced, but Phase 2 remains blocked pending
security/provenance/SBOM/license-notice closure, frozen corpus and independent
oracles, numeric resource limits, evidence policy, effective guest/no-network proof,
and a later separate exact restore/build/execution authorization. Phase 3 remains
`BLOCKED_BY_SEQUENCE`; program progress remains 2/12 phases = 16.67%.
