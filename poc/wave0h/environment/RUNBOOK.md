# Wave 0H Agent 1 — concrete benign observation

Baseline d472b85e179d715b9795773bcfda01dc628e9d42.
Scope POC-PDF-001 / FEAT-PDF-005; PRE-006, PRE-009; PDF001-E00 preparation.
Current Wave 0H user permission covers Sandbox/engine; Agent 0 is sole launch
operator after audit. Historical false-authority validators are not used here.
ZERO_ADDITIONAL_COST. No restart, installation, host security change or new engine.

## Ready artifact and command

After integration/audit, Agent 0 runs from the integrated product worktree:
`Start-Process -FilePath (Join-Path $PWD 'poc/wave0h/environment/benign-observation.wsb')`

This is an intentional visible guest window for observation. Do not run during
Agent 1 preparation. Close any pre-existing guest first through its UI only if
owned by this task; otherwise stop to avoid interfering with another session.
The config requests 4096 MB, network/vGPU/all listed redirections disabled,
protected client enabled, zero host mappings, and a benign encoded logon command.
Observe-Guest.ps1 is its exact readable source; the validator checks byte equality.
Encoding transports the script without a mapped drive or clipboard. It does not
hide or confer trust on the command. No filesystem, network request, credential,
PDF/provider or package operation is in the script.

## Bounded operator procedure

1. Validate with `pwsh -NoProfile -File poc/wave0h/environment/Verify-Static.ps1`;
   record config/script SHA-256 and integrated commit. Check fresh launcher
   readiness with available read-only host observations; do not use old statuses
   as current evidence. Stop on missing launcher, credential or restart request.
2. Launch once. Allow 120 seconds from launch for visible guest startup. At timeout,
   close this Sandbox window via X and confirm its disposal prompt. Do not restart
   Windows or kill shared host virtualization services.
3. Once visible, allow 30 seconds for W0H_BENIGN_OBSERVATION_V1. Script queries BCL
   interface metadata only. No non-loopback interfaces is the desired observation;
   any interface or UNKNOWN requires review, not silent acceptance.
4. Record guest-only screenshot showing the fixed summary. Do not include other
   host windows, usernames, taskbar notifications or document content. Transcribe
   counts/versions verbatim and hash the screenshot in approved evidence storage.
   This host-side screenshot/manual record is the evidence export: no writable
   host share, clipboard, network upload or copy-back channel is needed.
5. Inspect disabled clipboard/printer/audio/video controls using guest-visible
   settings only if needed; do not probe with personal clipboard or real devices.
   Record unobserved controls as NOT_OBSERVED, not PASS.
6. Close the exact guest window after evidence capture; confirm disposal. Bound
   total session at five minutes. If UI close fails, report CONTAINMENT_CLOSE_FAILED
   and stop dependent processing; Agent 0 investigates this session without broad
   process kills. Verify window/session closure; capture end timestamp/outcome.

The 4096 MB and time values are technical smoke defaults for this one benign
observation, not canonical PDF performance thresholds or host disk quotas.
UI timeout/close is operator-enforced, not an implemented watchdog.

## Evidence and follow-on boundary

Record run/commit/config/script digests, UTC launch/summary/close timestamps,
counts/OS version, outcome and guest-only screenshot digest. No host path,
MAC/IP, adapter name, machine name, secret or document bytes in the report.
The script prints no such values and writes no files. Screenshot export does not
provide machine-readable PDF result artifacts or source/output digest validation.

This profile is runnable for benign environment observation only. It carries no
candidate/corpus bytes; candidate smoke needs Agent 0's audited separate transfer/
execution plan using Agent 2/3 inputs. Do not quietly add mappings to this profile.
A no-interface observation supports isolation but does not measure attempted
egress, effective policy for every redirect, engine behavior, crash cleanup, or
full charter exit. Config XML alone is never offline proof.

Primary reference (consulted during earlier environment preparation; recheck if
target Windows support changes):
https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file
