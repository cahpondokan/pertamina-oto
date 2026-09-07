# Windows Sandbox execution-freeze input

Task: `P2-W0G-A1-ENVIRONMENT-FREEZE`; run `P02-20260905-1434`.
Baseline: `4a78212e01d3daee9ae47f0473fb6186df0dccde`.

Status: **PREPARATION ONLY — STOP BEFORE LAUNCH**. This packet is a sanitized,
non-executing input to `PRE-009` and `PRE-010`; it is not `PRE-012` execution
authority. It adds no cost and acquires no component.

## What is frozen

- `environment-manifest.v1.json` binds the post-restart report/JSON, older host
  inventory, task/charter/security inputs, and the existing template/verifiers by
  SHA-256. It stores no user identity or resolved host path.
- Host post-restart readiness is observed: feature, launcher, Start registration,
  virtualization prerequisites, no pending restart, and static template controls.
- Host OS/build/architecture and .NET values come from the 2026-09-03 inventory.
  They are explicitly marked pre-restart and must be refreshed before any launch.
- The Microsoft-signed launcher is bound by basename, location class, size,
  version, SHA-256, signer, signature status, and AppID. Binary architecture was
  not observed and remains `UNOBSERVED`.
- Guest boot, guest OS/runtime, effective network denial, mapped-folder enforcement,
  redirections, host-boundary containment, resource behavior, close, and cleanup
  remain unproven because Sandbox was not launched.

## Control and mapping boundary

`control-checklist.v1.json` mirrors the exact current static template. The only
allowed host mapping is one unresolved, human-approved inert synthetic input token,
mapped to `C:\PocInput` as read-only. There is no output/evidence/temp mapping.
Drive/workspace/profile/network/sensitive/reparse/writable/unreviewed mappings are
denied. Any unlisted XML element, attribute, duplicate, or default is denied.
`MemoryInMB` remains absent until a numeric ceiling and revised template are
reviewed; `LogonCommand` remains denied.

## Later observation plan

`later-launch-observation-plan.v1.json` defines a future, separately authorized,
single benign environment observation with candidates, hostile material, and PDFs
absent. It orders hash revalidation, fresh host identity, inert mapping freeze,
supervised launch, guest/control checks, resource/network observation, close and
reconciliation, then sanitized independent review.

The plan intentionally leaves these blockers explicit: named human execution and
security reviewers; exact digest-bound authorization/expiry; least-privilege
identity; inert fixture authority; independent environment oracle; approved
host-side network collector and non-public canary; guest observation/export method;
sampling/resource/cancel/cleanup ceilings; and evidence access, retention, expiry,
disposal, classification mapping, owner, and reviewer. A writable evidence mapping
would be a different profile requiring a new hash freeze and security approval.

## Static verification only

Run from repository root:

```powershell
pwsh -NoProfile -File poc/environment/execution-freeze/Verify-ExecutionFreeze.ps1 -SelfTest -CoordinationRoot '<coordination-workspace>'
```

The verifier parses JSON, checks fixed controls/false authorities, verifies source
hashes when the coordination root is supplied, and rejects five in-memory negative
fixtures. It does not invoke host probes, start processes, launch Sandbox, render a
`.wsb`, install/restore/build/load a candidate, or open/process/create a PDF.

Expected marker:

```text
DISCOVERED=6 PASSED=6 FAILED=0; HOST_PROBES=0; PROCESS_LAUNCHES=0
FREEZE_STATIC_VALIDATION=PASS; LAUNCH_AUTHORIZED=false; PROCESSING_AUTHORIZED=false; NETWORK_PROVEN=false; COST=ZERO_ADDITIONAL_COST
```

Any source hash or frozen identity/control change invalidates this packet and
requires review. A complete freeze packet still must stop before human-only
`PRE-012`.
