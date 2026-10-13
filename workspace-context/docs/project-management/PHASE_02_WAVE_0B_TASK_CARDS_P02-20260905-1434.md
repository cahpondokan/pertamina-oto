# PERTAMINA OTO — Phase 2 Wave 0B Review Task Cards

| Field | Value |
|---|---|
| Run ID | `P02-20260905-1434` |
| Checkpoint input | Integration branch `integration/P2-proof-capability` @ `42115b25c153717586f74a400a3cdc16c542f1af` |
| Candidate artifact | PDFsharp 6.2.4, 4,625,326 bytes, SHA-256 `5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67` |
| Cost boundary | `ZERO_ADDITIONAL_COST` |
| Processing authority | `NOT_AUTHORIZED`; review is static/read-only against the package and preparation artifacts |

## Agent 6 — `P2-W0B-A6-SECURITY-REVIEW`

- Worktree/branch: `worktrees/phase2-20260905/agent-6-security-review`; `task/P2-A6-security-review`.
- Objective: independent static security review of the exact package, harness, and synthetic corpus/oracle preparation.
- Allowed Paths: `poc/reviews/security/**` only. Candidate package and integrated sources are read-only inputs.
- Do Not Touch: all other tracked paths, raw package, product graph/code, central coordination files, system security settings.
- Required review: package integrity/signature limitations; archive/path/native/build/script inventory; managed-assembly indicators for P/Invoke, networking, process launch, dynamic loading, telemetry/activation, unsafe APIs, and active-content behavior without loading/executing assemblies; scanner availability/result; harness fail-closed boundaries; corpus/oracle abuse cases; zero-egress proof plan; threat-to-`PDF001-E00..E12` matrix; exact gaps and stop conditions.
- Verification: reproducible commands, artifact hash before/after, no assembly/provider execution, no new dependency/tool, no secrets/content, diff check.
- Output: `PASS_STATIC_WITH_LIMITATIONS`, `BLOCKED_SECURITY`, or `FAIL_SECURITY` recommendation; this is not a human engine approval.

## Agent 7 — `P2-W0B-A7-LICENSE-GOVERNANCE-REVIEW`

- Worktree/branch: `worktrees/phase2-20260905/agent-7-license-review`; `task/P2-A7-license-review`.
- Objective: independent governance/license/no-cost review of the exact package and preparation packet.
- Allowed Paths: `poc/reviews/governance/**` only. Candidate package and integrated sources are read-only inputs.
- Do Not Touch: all other tracked paths, raw package, product graph/code, central coordination files.
- Required review: exact identity/source/hash/signature; MIT text/reference/notice obligations; declared dependency ranges for all TFMs; native/bundled assets; no-cost boundary; redistribution/runtime/support/maintenance/removal limitations; unresolved exact transitive graph and SBOM; evidence completeness and binding; PRE/P2-D status recommendation; controlled next decision wording.
- Official-source metadata queries are allowed; no package/binary acquisition beyond the already approved artifact. Do not provide legal advice or approve a vendor.
- Verification: source links and check date, hash before/after, no product/package reference, no paid term, no secrets/content, diff check.
- Output: `INTAKE_GOVERNANCE_READY_WITH_LIMITATIONS`, `BLOCKED_LICENSE_COST`, or `FAIL_PROVENANCE` recommendation; only Agent 0 and the human authority decide next admission.

## Agent 0 checkpoint

Agent 0 waits for both reviews, rejects unsupported claims, integrates accepted review records, and determines whether a further free metadata/dependency intake is needed. Provider/document execution remains prohibited until the exact environment, dependency graph/SBOM, oracle, evidence policy, and execution authorization are frozen.
