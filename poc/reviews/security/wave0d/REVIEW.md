# Wave 0D security packet review

Agent 6 / P2-W0D-A6-SECURITY-PACKET-REVIEW; run P02-20260905-1434; 2026-09-06 Asia/Jakarta.
Input HEAD 07b5621c8d5e26793e4d8658aef9f8e23b3a86ec verified; branch task/P2-A6-wave0d-review verified; initial worktree clean. ZERO_ADDITIONAL_COST. Recommendation: ACQUISITION_ONLY_PACKET_READY_WITH_SECURITY_CONDITIONS; processing remains BLOCKED_SECURITY. No human approval is supplied by this recommendation.

## Scope and authoritative inputs

Read AGENTS.md, Shared Agent Contract, Phase 2 prompt, Project State, Decision Register and Wave 0D task card fully. Reviewed frozen Sandbox template/validator/runbook, dependency closure plus four metadata records, qpdf/MuPDF research/manifest, previous security review and documented local Poppler observations. Current checkpoint takes precedence over stale summary row still naming d63155a; neither grants processing authority. Feature FEAT-PDF-005; P2-D01/P2-D02/P2-D03; PRE-004/PRE-005/PRE-006/PRE-007/PRE-008/PRE-009/PRE-010/PRE-011/PRE-012; TECH-DEC-005/TECH-DEC-006.

No acquisition, assembly/binary loading, document processing, restart, Sandbox launch, system/security mutation, product reference or paid component. Existing PowerShell was used only for text/XML/JSON validation. No current advisory or license clearance is claimed; candidate version/release facts below are attributed to frozen research, not newly fetched external facts.

## Exact next acquisition packet recommendation

Recommend human acquisition-only authorization for exactly these three official NuGet artifacts, followed by static quarantine inspection. Reuse PDFsharp 6.2.4 already held; do not redownload it. Proposed Windows x64/net10.0 graph selects net8.0 dependency groups. Pins are lower bounds from metadata, not a patch recommendation.

| Component | Exact proposed acquisition URL | Security restriction |
|---|---|---|
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | https://api.nuget.org/v3-flatcontainer/microsoft.extensions.logging.abstractions/8.0.3/microsoft.extensions.logging.abstractions.8.0.3.nupkg | Inspect logging generators/analyzers and buildTransitive targets before any restore/build. No source-generator execution permitted by acquisition. |
| System.Security.Cryptography.Pkcs 8.0.1 | https://api.nuget.org/v3-flatcontainer/system.security.cryptography.pkcs/8.0.1/system.security.cryptography.pkcs.8.0.1.nupkg | Inspect selected Windows runtime assembly, imports and crypto/OS dependencies. runtimes/win is not by itself proof of native code or proof of no native dependence. |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | https://api.nuget.org/v3-flatcontainer/microsoft.extensions.dependencyinjection.abstractions/8.0.2/microsoft.extensions.dependencyinjection.abstractions.8.0.2.nupkg | Inspect buildTransitive targets and selected assets; no implicit build import authorization. |

Bind each to its frozen catalog SHA512 assertion (verification.json records exact values), independently compute local SHA256/size, inspect archive paths/duplicate aliases/scripts/native resources/notices, verify signature/trust and vulnerability/scanner disposition. Upstream metadata hashes are assertions until checked; mismatch stops intake. No wildcard/latest, automatic dependency recursion, alternate feed, version substitution or extra package. Restore/build remains prohibited after acquisition until reviewed imports/analyzers, exact lock/assets/RID/SDK/runtime and human admission are frozen. A 4-node metadata graph is not deployment SBOM.

qpdf: recommend a SEPARATE optional acquisition-only line for ORACLE-QPDF-12.4.1, exact qpdf-12.4.1-msvc64.zip from https://github.com/qpdf/qpdf/releases/tag/v12.4.1, advertised SHA256 3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6. Agent 0 must freeze the exact asset download URL and permitted release/CDN redirect chain before dispatch; this report does not invent or approve that URL. Inspect bytes/DLL closure/signature/provenance/licenses/SBOM without running executable. qpdf structural checks do not close visual/content fidelity. Parser warnings/repair cannot be treated as clean PASS. Frozen CLI research warns encryption output can disclose recoverable passwords: raw stdout/stderr must not be ordinary evidence. No encrypted qpdf invocation before separate safe argument/output design. No silent fallback to another build.

MuPDF 1.28.3: DEFER acquisition/build in the next minimal packet. Frozen research identifies source-only release; exact source digest/submodules/toolchain/features and build provenance remain unresolved. No substitute 1.28.1 binary, paid license, source checkout or auto-submodule acquisition. A free reviewed route can be considered later, with Agent 7 license disposition; subprocess separation is not an exemption from governance.

Local Poppler 26.07.0: INVENTORY_REVIEW_ONLY, no acquisition/copy/use approval. Central PHASE_EXECUTION_LOG documents pre-existing Codex-runtime pdfinfo/pdftoppm version observations, no PDFs processed, and unresolved bundled origin/license/lifecycle. Exact hashes are said to be in Agent 0 output but are not frozen in this input checkpoint. Agent 3 owns Wave 0D inventory; accept its committed hash/signature/dependency/license evidence before considering later internal-only guest use. Existing installation does not authorize copying a runtime tree, executing against documents or product redistribution. App-managed updates may change/remove paths/binaries; freeze and recheck exact bytes, no implicit PATH selection. Do not rely on main-host Poppler to bypass guest requirement. qpdf plus a separately admitted renderer remains necessary for complementary evidence; two tools from one engine are not independent votes.

## Sandbox static assessment

Template pins network/vGPU/audio/video/printer/clipboard Disable, ProtectedClient Enable; exactly one placeholder read-only host map; no logon command or writable export map. Validator prohibits DTD/external resolver, rejects duplicates/unexpected settings and validates exact controls. Re-run: 15/15 PASS, exit 0. This is PASS_STATIC_WITH_LIMITATIONS for this preparation template only. Placeholder/suffix/comment are governance markers, not execution locks. Effective guest controls unproven.

Memory ceiling omitted; processing profile must undergo reviewed revision, not simply add settings that current validator rejects. Dedicated immutable input root must be exact local path with ACL/reparse/alias/children checks; no repo/profile/drive/runtime-wide mapping. XML-escape final path, reject tokens, independently validate and hash rendered profile. Read-only mapping prevents intended guest writes but does not make host-side inputs immutable; bind bytes and prevent host changes during run.

There is intentionally no persistent evidence export. Do not enable network/clipboard to compensate. If approved later, a fresh empty dedicated writable quarantine map is a host write exposure; freeze ACL, names/types, storage ceiling, run ownership, retention and disposal. Guest sanitization alone is untrusted. After guest termination use a bounded host-side data-only validator; never execute/open exported scripts/binaries or auto-import results. Treat result PDFs as untrusted, with isolated oracle admission before opening. Preserve failed-run evidence and source/target digests. Guest disposal does not prove application cleanup or undo host writes. If safe export cannot be shown, processing stays blocked.

## Threat and gate matrix

| Threat / exact experiments | Required control/evidence | Current disposition |
|---|---|---|
| Supply-chain/build activation: PDF001-E00, PDF001-E11 | Exact artifact and selected assets; analyzer/targets inspection, immutable graph and runtime manifest; no unapproved loading | Metadata proposal only; PRE-004/PRE-005/PRE-006 open |
| Egress/child process: PDF001-E00, PDF001-E05, PDF001-E12 | Effective deny plus attempted/successful network and process-tree observation; no activation/telemetry exception | Template only; no runtime PASS |
| Wrong/lost/reordered/duplicate content: PDF001-E01, PDF001-E02, PDF001-E03, PDF001-E06 | Independent actual staging-byte structural/render/content/reopen oracle and negative controls, no repair-as-PASS | qpdf cannot replace renderer; PRE-008 open |
| Password/log/content leakage: PDF001-E04, PDF001-E05, PDF001-E06 | Ephemeral password, no argv/ordinary log/raw capture, safe per-run canaries and bounded typed evidence | Output handling not implemented/frozen |
| Traversal/source/finality: PDF001-E07 | File identity/handle containment, exact run roots, no silent overwrite; validated atomic publish | Path skeleton is advisory; no publisher proof |
| Cancel/crash/timeout/cleanup: PDF001-E08 | External supervisor, bounded process-tree stop, no late publish, reconcile before guest disposal | No implemented supervision evidence |
| Resource/concurrency: PDF001-E09, PDF001-E10 | Approved emergency safety ceilings and bands, per-run/aggregate sampling, race barriers and drain | PRE-009/PRE-010 pending; no fabricated thresholds |
| Evidence export/tampering: PDF001-E00, PDF001-E06, PDF001-E08 | Safe export channel, host-side schema/sanitization validation, hashes and immutable run binding | PRE-011 pending |
| Scope/fallback drift: PDF001-E11, PDF001-E12 | Exact binary+argument allowlist; approved local unavailable/fallback only | No product/provider execution allowed |

## Later control verification plan — not execution permission

After SEPARATE user-authorized restart: re-read actual feature state, OS/build/architecture, remaining reboot indicators, launcher presence/version/signature and prerequisites. No automatic feature repair/download if launcher absent. Validate compatible supported settings/entitlement at that time. Restart alone does not authorize Sandbox launch. Subsequent separate benign guest check must establish effective network/sharing/read-only-map controls, exact guest runtime/identity, observer health and approved resource limits before provider or hostile input.

Use explicit executable digest/path and typed argument allowlists, working directory and environment allowlists; no shell, arbitrary command, script, viewer, URI fetch or inherited broad credential. Child processes must be explicitly inventoried and contained. Supervisor owns timeout/cancel and waits for entire tree; no breakaway or false terminal success. Windows Sandbox boundary does not by itself implement per-job lifecycle. PDFsharp Quality/Cryptography network/process indicators from Wave 0B still require selected-call-path disposition; no absence-of-egress inference from intended Merge usage.

Observe attempted and successful network operations (including DNS) and child processes across candidate/oracle; offline configuration only blocks traffic, not proof of attempts. Calibrate observer with approved inert benign probes without public endpoints; missed events/inconclusive observation stop dependent tests. Admission metadata traffic is separate from processing egress.

Evidence records only opaque IDs, versions/digests/status/counts/timing; no document content, password, raw paths, dump or stdout excerpts. Per-run synthetic canaries test forbidden sinks, including crashes and verifier errors; content-bearing result is a separately controlled artifact. Bind config, candidate/oracle, corpus/expectations, build/commit, instrumentation and outcome; host validator must not trust guest PASS claims. Resource measurements stay NOT_RUN; safety ceilings, memory/temp/file/page/decompression/time/concurrency/cleanup and retention values remain explicit human decisions. Zero processing egress is canonical, not a new threshold.

## Remaining hard blockers and decision wording

Agent 0 can present: 'Authorize only static acquisition/inspection of Logging.Abstractions 8.0.3, Pkcs 8.0.1 and DependencyInjection.Abstractions 8.0.2 at the exact listed URLs, no additional cost, no install/restore/build/run. Stop on mismatch, unexpected asset/dependency or paid requirement. qpdf acquisition is a separate optional exact-asset decision after its URL/redirect/trust packet is frozen. Defer MuPDF source/build and Poppler use pending inventory/admission. This does not authorize restart, Sandbox launch, document processing or product integration.'

Human authority must separately resolve artifact/trust/scanner/security/license disposition, oracle selection, complete guest controls/runtime, corpus/expected digests, safety/measurement policy, evidence export/retention and execution freeze. Agent 6 cannot close PRE-012 or accept residual risk. Stop affected work on paid requirement, changed hash/graph/TFM/RID, unsupported setting, unresolved authority, egress/active content, source risk, missing observation, leakage, unbounded cleanup or export. Agent 0 coordinates; Workspace User temporary authority decides resumption. No engine/phase exit PASS.

## Verification / handoff

Commands executed: git status --short (clean); git rev-parse HEAD and branch --show-current (exact expected); pwsh -NoProfile -File poc/environment/windows-sandbox/Validate-Template.ps1 -SelfTest (exit 0, 15/15); pwsh -NoProfile -File poc/intake/dependency-closure/Build-Closure.ps1 -Verify (exit 0, four nodes/three edges/deterministic metadata). Both scripts were read before invocation; -Verify is read-only. No build/provider/scanner/runtime test performed. An exploratory rg lookup used a nonexistent docs path inside product worktree and exited 2; corrected central-doc lookup found the Poppler observation. No missing source was treated as PASS.

Input SHA256 bindings and exact metadata package hashes are in verification.json. Files changed: only this REVIEW.md and verification.json under poc/reviews/security/wave0d. Integrate after Agent 0 compares Agent 3 inventory and Agent 7 governance; this static review can be integrated before decisions, but neither approvals nor binary use follow automatically.
