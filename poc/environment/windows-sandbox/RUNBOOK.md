# Windows Sandbox preparation — P2-W0C-A1-SANDBOX-CONFIG

Status: PREPARATION_ONLY. ZERO_ADDITIONAL_COST.
**NO_RESTART_WITHOUT_SEPARATE_APPROVAL. NOT_AUTHORIZED_TO_LAUNCH.**
Authority: P2-W0-ENV-001, P2-W0-ENV-002. Trace: PRE-006, PRE-009,
PRE-010, PRE-011, PRE-012; PDF001-E00, PDF001-E05, PDF001-E08.
No PDF experiment is proven by this configuration.

## Current gate and validation

The task card reports feature Enabled, launcher absent, reboot indicators present.
These are inherited observations, not a fresh machine audit by this task.
Do not restart, launch Sandbox, change features/security settings or install tools.
A missing/unsupported control stops preparation for execution; never remove a
control or silently use defaults to get a guest to start.

Run only: `pwsh -NoProfile -File poc/environment/windows-sandbox/Validate-Template.ps1 -SelfTest`.
This reads XML and tests rejection in memory. It does not launch anything, write
a rendered configuration, inspect host settings, or establish execution authority.
The template suffix avoids normal .wsb association. A user could still manually
launch it; the marker and placeholder are governance guards, not an OS security
boundary. Static PASS proves this exact template profile only.

## Proposed profile

One placeholder host input mapping is read-only; guest target is C:\PocInput.
Network, vGPU, clipboard, printer, microphone and webcam sharing are disabled.
Protected client is enabled. There is no logon command, output host mapping,
automatic processing, or hard-coded host path. Memory allocation remains a
pending approved safety ceiling; omission is not a resource-limit approval.
The eventual runtime profile must freeze memory and temp/output budgets before
processing. This template validator deliberately rejects extra settings; a later
reviewed revision must validate the approved runtime profile.

## Prelaunch review — later authorized task only

1. Obtain separate explicit user restart approval if reboot is still required.
   Stop at this gate; no scheduled restart or restart command is included here.
2. After the user-approved restart, record fresh OS build/edition/architecture,
   feature state, launcher presence/version and remaining reboot indicators.
   Read-only checks requiring elevation must follow the authorized operator flow;
   credential prompts stop the agent. If launcher remains absent, return BLOCKED.
3. Record installed guest prerequisites and supported settings against Microsoft
   documentation and local policy. Do not download missing components. Confirm
   existing Windows entitlement meets zero-additional-cost; no procurement assumed.
4. Approve exact input root and immutable digest manifest. Use a dedicated
   synthetic-only copy, not the repository, drive root, user profile, network
   share, secret directory or operational document folder. Validate absolute local
   path, ACLs, absence of reparse points/aliases and source immutability. Review
   children too; do not assume a root-only link check is sufficient.
5. Freeze corpus, tools/dependencies, independent oracle, instrumentation,
   least-privilege execution identity, budgets and execution authorization.
   Sandbox availability alone does not satisfy PRE-001 through PRE-012.
6. A later authorized renderer must XML-escape the approved path, reject unresolved
   tokens and hash the final .wsb plus manifests. Independently compare controls
   and mapping inventory with approval. Never double-click the template now.
7. With separate launch authority, verify controls in a benign guest before any
   provider/hostile suite. Record actual network state and effective policy plus
   observation of attempted egress. No public endpoint is needed. Config text
   alone is not offline proof. Record read-only mapping behavior and disabled
   redirections. Unsupported settings or inconclusive observation stop the run.

## Evidence export design — pending review

This profile exposes no writable host mapping and disables clipboard export.
Therefore persistent guest evidence export is deliberately unavailable.
For a later approved benign environment check, a sanitized manual observation
record can capture only control states, versions and manifest digests; it cannot
replace machine measurements or result artifacts.

A processing profile needs a separate reviewed export mechanism. Proposed option:
a fresh, empty, run-specific host quarantine directory as the only writable mapping.
This is a host-write exposure, not a write-only channel. It must not contain
valuable files, sit in a watched/imported folder, or permit automatic execution.
Approve ACLs, byte/file-count ceilings, filenames/extensions, retention and disposal.
Treat every exported byte as untrusted; never open generated scripts/binaries.
After guest termination, independent review validates allowlisted metadata schema,
sanitization and hashes before copying accepted evidence into its restricted root.
Do not export document content, passwords, raw paths, dumps or environment variables.
No writable mapping or export implementation is included in this template.
If that exposure is unacceptable, keep processing blocked pending another approved
zero-cost design; do not enable clipboard/network as a workaround.

## Recovery and stop records

Before closing a future guest, reconcile run-owned temp/output and preserve approved
sanitized evidence. Sandbox disposal is not proof of application cleanup, nor does
it undo writes through a writable host mapping. Record cancel/timeout/crash state,
source and target digests, orphan inventory and unresolved reconciliation.
No deletion, shutdown/restart, feature enablement or security mutation script is
included. A reset cannot erase failed-run evidence or justify a PASS.

## Primary references checked 2026-09-06

[Microsoft configuration options](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file)
documents these XML controls, read-only mappings and persistence of mapped writes.
[Microsoft overview](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/)
provides the Windows Sandbox product context. Local compatibility remains untested.

