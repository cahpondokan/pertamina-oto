# qpdf 12.4.1 acquisition checkpoint - BLOCKED

Task: P2-W0E-A3-QPDF-ACQUISITION-ONLY; run P02-20260905-1434.
Date: 2026-09-07. Branch: task/P2-A3-corpus-oracle.
Lane baseline: 74e93c16544103086ebf5e4d6b50dac23aadbd60.
Input checkpoint declared by Wave 0E: 38d5494050f62caea2427c546125791d53c73ca9.

## Authority and blocked action

Read AGENTS.md, Shared Agent Contract, Phase 2 prompt, Project State, Decision
Register, Wave 0E task card and qpdf acquisition packet. Register P2-W0-QPDF-ACQ-001
records approval for exact acquisition-only use under ZERO_ADDITIONAL_COST.
The older packet retains its historical NOT_AUTHORIZED label; current task/register
and delegation explicitly request the acquisition. No central record was changed.

Exact proposed asset: qpdf-12.4.1-msvc64.zip.
Exact URL: https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip
Expected SHA-256: 3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6.
Raw target reference: approved run evidence root / agent-3-qpdf-intake.

Automatic approval review rejected the acquisition command before process creation.
Reason: it classified the operation as downloading/storing a new binary dependency
before required admission, and said no trusted user message authorized this exact
acquisition. It explicitly prohibited bypass by workaround or indirect execution.
The rejection conflicts with the authorization represented by the delegation and
local register, but does not permit this agent to override the reviewer.

No retry through another tool, host or downloader was attempted. The rejected
command did not run; its acquisition script and output were not created by that
command. This report is the only task artifact created after rejection.

## Evidence status

| Check | Result |
|---|---|
| Worktree baseline/status | Expected branch; clean before task |
| Acquisition | BLOCKED_BY_APPROVAL_REVIEW before process creation |
| Local archive/source/redirect/time/size | NOT_OBSERVED |
| Downloaded SHA-256 | NOT_AVAILABLE; above is expected metadata only |
| Safe archive inventory | NOT_RUN |
| DLL/import/runtime dependency inventory | NOT_RUN |
| Exact license/notices/SBOM | NOT_INSPECTED |
| Signature/trust | NOT_VERIFIED |
| qpdf execution / PDF processing | NOT_RUN |
| Added cost / PATH / install / product reference / restart | NONE |

Prior Apache-2.0 metadata remains research, not verified license coverage for the
unacquired archive or bundled dependencies. No artifact admission, engine approval,
security clearance, offline proof or Phase 2 exit is claimed. No other component
was acquired. No raw binary or document is included in this tracked report.

## Required next decision and handoff

Status: BLOCKED_BY_APPROVAL_REVIEW. Required owner: Workspace User / trusted approval
channel, with Agent 0 coordinating admission evidence. Obtain direct confirmation
for this exact ZIP, official URL, expected hash and acquisition-only boundary;
then resubmit through the ordinary approval mechanism. Do not bypass the rejection.
Any paid requirement remains BLOCKED_BY_COST before commitment.

After approval, enforce source/redirect policy and hash before archive inspection;
stop on mismatches or unexpected dependencies/cost. Do not run qpdf or any archive
content. Record licenses/notices, imports, archive entry hashes and trust limits in
the exact raw evidence root, with sanitized tracked reports in this directory.

Agent 0 may integrate this blocker report only. No raw evidence has been produced
by this task, and all acquisition/static-inspection acceptance items remain open.
