# Wave 0G-B independent security execution-freeze review

## Disposition

`BLOCKED_SECURITY`

Task `P2-W0G-D-A6-SECURITY-RECHECK`, run `P02-20260905-1434`, independently
re-reviewed the prior Wave 0G-B packet against exact integrated baseline
`d895b2bc5e9c6229de77959332b8a026b9d45736`.

This is a hash-bound blocker/readiness packet for governance review. It is not
an execution approval, Security Approver sign-off, PRE-012 approval, or
self-approval. All effective execution allowlists remain empty. No candidate,
sandbox, scanner, engine, restore, build, PDF corpus, download, or install was
run during this review, and no system setting was changed. Cost was zero.

The machine-readable authority boundary, source hashes, future eligibility
profiles, evidence plan, stop rules, and gap register are in
`security-review.v1.json`.

## GOV-C-001 controlled remediation

The recurring `GOV-C-001` failure was caused by treating append-oriented
coordination records as immutable material evidence. The repaired boundary is:

- 28 candidate, environment, corpus/oracle, prior-review, task-contract, and
  other material inputs retain exact byte-length/SHA-256 enforcement;
- all eight portfolio artifacts retain immutable commit, Git-blob, byte-length,
  and SHA-256 enforcement;
- `PROJECT_STATE.md`, `PHASE_DECISION_REGISTER.md`, and
  `PHASE_EXECUTION_LOG.md` retain reviewed byte/hash snapshots for traceability,
  but current content is re-read semantically on every verification.

Byte drift in those three declared mutable records is not itself security
clearance or failure. Verification fails if Phase 2 becomes `COMPLETE`, its
state is missing/ambiguous, candidate/Sandbox execution becomes permissive,
`PRE-012` becomes approved or loses its human-only boundary, or
`ZERO_ADDITIONAL_COST` ceases to be a hard stop. One harmless editorial-drift
fixture must pass; four permissive coordination mutations must fail. This fixes
the false recurring drift without clearing any actual security gap.

## Decisive findings

The freeze cannot advance as security-ready while these gaps remain:

| Rank | Gap | Security result | Required closure owner |
|---:|---|---|---|
| High | SEC-G-001 | The official PDFsharp issue 361 reports that malformed cyclic page trees terminate a PDFsharp 6.2.4 process through unbounded recursion/`StackOverflowException`. The exact frozen candidate is 6.2.4. | Security Approver + Technical Owner |
| High | SEC-G-002 | Official open qpdf issues 1739 and 1688 describe memory/resource-exhaustion paths. Exact 12.4.1 applicability is unresolved; the linearization-check arguments remain denied. | Security Approver + Technical Owner |
| High | SEC-G-003 | Package trust includes `RevocationStatusUnknown`, qpdf primary binaries have no embedded certificate table, and no approved fresh advisory/scanner disposition exists. | Security Approver |
| High | SEC-G-004 | The exact restore graph, .NET host, locked restore, harness build, launch binary, arguments, child processes, and runtime modules have not been observed. | Technical Owner + Agent 0 |
| High | SEC-G-005 | Guest boot, effective network denial, mapping isolation, process/module inventory, and DLL search behavior are unproven. | Security Approver + Agent 0 |
| High | SEC-G-006 | Numeric resource ceilings and bounded cancellation/crash/kill-tree/cleanup behavior are not frozen or evidenced. | Technical Owner + Security Approver |
| High | SEC-G-007 | Required malicious/edge corpus groups and render/content oracles are not materialized or approved. | Product Owner + UAT Approver + Technical Owner |
| High | SEC-G-008 | Evidence export mapping, minimization, redaction, retention, custody, and cleanup are unresolved. | Security Approver + Repository Integrator |
| High | SEC-G-011 | The conversion/OCR/device/AI proposals remain security-unadmitted: exact candidates/devices/providers/models, data/consent policy, limits, corpus/oracles, license, and per-PoC authority are unresolved. | Product + Technical + Security + Privacy/License/QA owners |
| Medium | SEC-G-009 | Post-restart host evidence is point-in-time and must be refreshed immediately before a later authorized launch. | Agent 0 |
| Low | SEC-G-010 | Public advisory/release pages are mutable observations rather than archived immutable artifacts. | Repository Integrator |

Official read-only references reviewed without acquiring artifacts:

- PDFsharp 6.2.4 release: <https://github.com/empira/PDFsharp/releases/tag/v6.2.4>
- PDFsharp 6.2.4 NuGet metadata: <https://www.nuget.org/packages/PDFsharp/6.2.4>
- PDFsharp issue 361: <https://github.com/empira/PDFsharp/issues/361>
- qpdf 12.4.1 release: <https://github.com/qpdf/qpdf/releases/tag/v12.4.1>
- qpdf release notes: <https://qpdf.readthedocs.io/en/stable/release-notes.html>
- qpdf issues 1739 and 1688: <https://github.com/qpdf/qpdf/issues/1739>, <https://github.com/qpdf/qpdf/issues/1688>

An open report is not treated as proof that every version/configuration is
affected. Conversely, release provenance, a lack of a matching advisory, or an
unreproduced report is not treated as proof of safety. The exact applicability
and containment decisions remain with the named human authorities.

## Cross-lane portfolio coverage

This review also covers the complete scoped outputs of both final sibling
commits without merging or mutating them. The static verifier resolves every
path from its commit, checks its Git blob identity/length, and recomputes its
SHA-256 from the immutable blob:

| Commit / packet | Path | Bytes | SHA-256 |
|---|---|---:|---|
| `df5430a14b432ed5ad18b0799512f4d1bd5d8dda` conversion/OCR | `README.md` | 4,633 | `51835471bdc7a67ca605a715e1b704ea4cfe7e1dd6b62910a09568b106f49c06` |
| same | `conversion-ocr-disposition.proposal.v1.json` | 34,299 | `266b3f0d630e9692ee387f9f47ee7c63f1118b505b2f394b945d6b243fc74be5` |
| same | `conversion-ocr-disposition.schema.v1.json` | 1,298 | `c7881ff24bd855f01a596a0d651aed97c9f5c995ae63c185ef1f6d8a5cbc3c3d` |
| same | `verify_static.py` | 8,925 | `e4480d33683ddbdd2c058619e889233d3c33a8ce8efd6574a0e4d43809146191` |
| `ee7f86735131302482af7524baf5422e081b73c9` device/AI | `README.md` | 4,195 | `10df7c9998d6eb368a57e86325382a7328ca1adeff570465dc6d2d8134451df7` |
| same | `device-ai-disposition.proposal.v1.json` | 46,223 | `3b17bb350feeb035bf6f2f3e83892a2fa4daef7bd61eb1bd4a69793d7844068b` |
| same | `device-ai-disposition.schema.v1.json` | 8,209 | `71026cc03359f482f67addc1e95f2aea6251058db023aa8062b4a8f5c31c2ad6` |
| same | `verify_static.py` | 8,572 | `14366935473b08117742f38d57d91b8082ecb229ca52e95cafc21437efbe63b8` |

Conversion/OCR coverage is exact for `POC-CONV-001`, `POC-CONV-002`,
`POC-OCR-001`, `POC-OCR-002`, `POC-CONV-003`, `POC-PDF-003`, and
`POC-CONV-004`. Its approval-isolation and macro/OLE/DDE/link,
formula-injection, HTML active-content/SSRF, OCR-content, independent PDF/A
oracle, and inert Markdown controls are necessary, but all seven PoCs remain
blocked and may not inherit `POC-PDF-001` authority.

Device/AI coverage is exact for `POC-SCAN-001`, `POC-INTEL-001`,
`POC-INTEL-002`, `POC-CONV-004`, and `POC-AI-LOCAL-001`. Its public-AI and
public-translation denial, no-silent-upload, offline-proof, consent,
untrusted-output, iOS-deferral, and zero-cost boundaries are necessary, but all
five PoCs remain blocked. The overlapping `POC-CONV-004` scope is consistent:
deterministic extraction first; OCR or AI fallback only after separate approval.

## Candidate and execution allowlist result

The review binds these acquired candidates by version, byte length, and SHA-256:

- PDFsharp 6.2.4 — `5647882339b327cbf6da9ea3f882efb335f8d602e6fd45323adf9e78da53cf67`
- Microsoft.Extensions.Logging.Abstractions 8.0.3 — `e4c498d5a13051b4577a148f1d8c3470167215c507e2392069b75dc61322bb74`
- System.Security.Cryptography.Pkcs 8.0.1 — `28c348909df243fe4ee962213e3c8021aaecbc8321929dba03a69bcb92a49de5`
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 — `51f2df1100245f10da54f0bb7e813f277155117777d4fbbab902214e27372606`
- qpdf 12.4.1 msvc64 ZIP — `3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6`

Candidate identity is not authority. The effective candidate, restore, build,
launch, runtime-process, runtime-module, and network-destination allowlists are
all empty. Six future profiles are documented only as
`BLOCKED_NOT_AUTHORIZED`: benign environment observation, PDFsharp bootstrap
restore, locked restore, build, run, and qpdf oracle. Each still needs exact
commands, arguments, paths, processes, hashes, modules, and its prerequisite
human decision before Agent 0 may create a later execution manifest.

`fix-qdf.exe`, `zlib-flate.exe`, undeclared child processes, writable-path
executables/modules, shells, interpreters, download tools, non-approved qpdf
arguments, and all network destinations are denied.

## Required later evidence plan

This plan is a requirement set, not permission to collect evidence now.

1. Process/module/DLL search: record the complete process tree, command lines,
   image hashes/signers, module absolute paths/hashes, .NET probing inputs,
   SafeDllSearchMode/KnownDLL/search paths, and unique unsigned DLL canaries in
   writable/current/input paths. Any undeclared child, module, search path,
   writable executable location, canary access, or telemetry gap stops the run.
2. Network/egress: prove effective guest network and firewall deny-default after
   boot; capture DNS/TCP/UDP/WinHTTP/WinINet plus routes/adapters/proxy state
   from pre-launch through teardown. Any query, socket, listener, destination,
   proxy use, or evidence gap stops the run. The qpdf DLL's `WS2_32` import makes
   observation mandatory even though no connection is expected.
3. Temp/resource/cancel/cleanup/canary: use a fresh guest-only run root; freeze
   numeric wall time, CPU, memory, handle, child-process, and output ceilings;
   test cooperative cancel, timeout kill-tree, crash, and forced teardown;
   hash before/after mapped-path inventories and source files; quarantine
   partial outputs; place non-PDF sentinels around every writable/forbidden
   boundary; destroy the disposable guest; prove no residue, process, handle,
   lock, mutation, or canary access outside the run root.
4. Evidence privacy: export only the approved minimal manifest/telemetry through
   a separately frozen write-only evidence path. No secrets, production
   documents, unnecessary document content, or unapproved identifiers may be
   retained.

The packet contains 18 fail-closed stop rules. The first security invariant
failure terminates the full process tree and the remaining experiments; a
failed run must never continue merely to increase coverage.

## Gate status and next decision

`PRE-003` through `PRE-011` remain `BLOCKED`. `PRE-012` remains
`HUMAN_ONLY_NOT_APPROVED`. Governance may review this blocker packet, but it
cannot use it to authorize execution. Closure requires the named owners to
resolve the high gaps, Agent 0 to bind a complete later manifest, and an
independent re-review after any candidate, source, advisory, host, sandbox,
command, or authority change.

## Static verification

From the repository worktree:

```powershell
python -B poc/reviews/security/execution-freeze/verify_static.py `
  --coordination-root 'D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO'
```

Expected and observed result at packet creation:

```text
SECURITY_FREEZE_STATIC_VERIFICATION=PASS
IMMUTABLE_BINDINGS=28;PEER_BLOBS=8;MUTABLE_COORDINATION=3;SNAPSHOT_MATCHES=3;NEGATIVE_FIXTURES=10;SAFE_DRIFT_FIXTURES=1;HIGH_GAPS=9;PRE003_011_BLOCKED=9
DISPOSITION=BLOCKED_SECURITY;EXECUTION_APPROVAL=false;SELF_APPROVAL=false;COST=ZERO_ADDITIONAL_COST
SCANNERS=0;SANDBOX_LAUNCHES=0;RESTORES=0;BUILDS=0;CANDIDATE_EXECUTIONS=0;PDF_FILES_OPENED=0
```

The verifier parses the packet; recomputes 28 immutable source and eight peer
Git-blob hashes/lengths; records whether three mutable coordination files still
match their reviewed snapshots; validates their current fail-closed semantics,
frozen identities, and authority boundary; accepts one safe byte-drift fixture;
and rejects ten unsafe mutations. It performs no dynamic security or candidate
test.
