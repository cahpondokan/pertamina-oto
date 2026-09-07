# Agent 1 Wave 0H handoff

Objective: concrete no-host-mapping benign Sandbox observation for Agent 0.
Baseline: d472b85e179d715b9795773bcfda01dc628e9d42.
Branch: task/P2-A1-wave0h; fresh worktree agent-1-wave0h.
Files: benign-observation.wsb, Observe-Guest.ps1, Verify-Static.ps1,
RUNBOOK.md, HANDOFF.md, all under poc/wave0h/environment.
No old worktree, source/product graph, central record or historical gate changed.

Verification command:
`pwsh -NoProfile -File poc/wave0h/environment/Verify-Static.ps1`
Exit 0: STATIC=PASS; CONTROLS=8; HOST_MAPPINGS=0;
SCRIPT_BINDING=PASS; SCRIPT_SYNTAX=PASS; GUEST_EXECUTED=false.
Binding ignores Git LF/CRLF normalization only. Encoded source is fully reviewable.
Protected-path diff against HEAD empty, exit 0. No build/restore needed.
Validation parses only; observation script and Sandbox were not executed.

Security/privacy: zero host shares; redirections/network disabled in XML; output
allowlists counts, OS version and fixed status. No credentials, personal data,
document content, file writes, downloads, paid component or system changes.
Resource evidence: configured 4096 MB only; no measured performance or runtime
network/isolation claim. Timeouts and close procedure are concrete in RUNBOOK.

Recommendation: GO for Agent 0 audit and benign observation with this profile.
No repeated permission request required for already authorized Sandbox use.
Runtime controls/closure remain unobserved, and no PDF engine proof is claimed.
Candidate smoke requires the distinct audited plan from Agent 2/3; this no-mapping
profile intentionally does not transport binaries or documents. Capture only
guest summary via host-side screenshot and sanitized transcription; no share
needed. If guest boot/close fails or credentials/restart are requested, stop that
operation and report actual failure. No installation/restart authority inferred.

Integration order: Agent 0 integrates this directory with Agent 2/3 artifacts,
obtains independent lane review, revalidates config/source binding and records
integrated commit/digests, then acts as sole guest operator. This completes Agent 1
preparation acceptance, not Phase 2 or any PDF001 experiment exit gate.
