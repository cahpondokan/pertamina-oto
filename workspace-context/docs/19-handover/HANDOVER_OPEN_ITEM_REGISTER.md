# HANDOVER OPEN ITEM REGISTER
## PERTAMINA OTO — Document 19

> Versi: FINAL v1.0
> Rule: tidak ada item disembunyikan untuk mempercepat sign-off

| Item ID | Type / Description | Affected scope / Impact / Risk | Workaround / Dependency | Target / Owner | Status / Accepted? / Reference |
|---|---|---|---|---|---|
| HO-RISK-001 | DOCUMENTATION_GAP — FR-FILE-006..009 unresolved | Requirement trace; invalid edge | No new ID; controlled D02/D03 revision | Before implementation trace freeze; Product/Requirement owner TBD | OPEN / NO |
| HO-RISK-002 | DOCUMENTATION_GAP — canonical path/archive ambiguity | Docs/automation may select wrong source | Current manifest; controlled canonical/archive policy | Baseline Freeze; Documentation Owner TBD | OPEN / NO |
| HO-RISK-003 | DOCUMENTATION_GAP — ContentIntelligence/Local Worker actual paths absent | Scaffold/scope ambiguity | Follow D18 logical map only | Repo bootstrap; Technical Owner TBD | OPEN / NO |
| HO-RISK-004 | TECHNICAL_DEBT — data-owner anchors not one-to-one with modules | Cross-write/god module risk | Preserve D07 one-writer boundary | Before DbContext/project lock; Tech/Data TBD | OPEN / NO |
| HO-RISK-005 | EXTERNAL_DEPENDENCY — official repository absent | Cannot clone/history/provenance | Documentation-only work | Before bootstrap acceptance; Repo Owner TBD | BLOCKED / NO |
| HO-RISK-006 | TECHNICAL_DEBT — no .NET solution/projects/source | Cannot build/test/package | Pre-implementation status | Repository Bootstrap; Technical Owner TBD | BLOCKED / NO |
| HO-RISK-007 | TECHNICAL_DEBT — SDK/feed/package pinning absent | Build/supply-chain drift | Do not publish build | Before first restore; DevOps TBD | OPEN / NO |
| HO-RISK-008 | TECHNICAL_DEBT — pipeline absent | No controlled artifact/gates | No manual release authority | Before shared build; Pipeline Owner TBD | BLOCKED / NO |
| HO-RISK-009 | SECURITY_FINDING — SBOM/license/security evidence absent | Supply-chain/legal/security risk | No package/provider approval | Before dependency admission; Security/License TBD | OPEN / NO |
| HO-RISK-010 | TECHNICAL_DEBT — Release Manifest/artifacts absent | No RC/promotion/rollback | Keep RC-TBD | Before QA/UAT; Release Owner TBD | BLOCKED / NO |
| HO-RISK-011 | TECHNICAL_DEBT — runtime OpenAPI/IPC/event contracts absent | Compatibility untestable | Use design only | First vertical integration; API/Contract owner TBD | OPEN / NO |
| HO-RISK-012 | TECHNICAL_DEBT — EF/SQLite migrations absent | DB lifecycle/recovery unproven | No DB deployment | Before DB integration; DB/Module owners TBD | BLOCKED / NO |
| HO-RISK-013 | TECHNICAL_DEBT — executable tests/corpus/results absent | Quality unknown | Specifications only | Before UAT entry; QA/Test TBD | BLOCKED / NO |
| HO-RISK-014 | POC_PENDING — edit existing PDF text | Fidelity/feature promise risk | Keep subset unavailable | PoC phase; Pdf/Product/Security TBD | NOT_STARTED / NO |
| HO-RISK-015 | POC_PENDING — Office/PDF/HTML/PDF-A conversion | Fidelity/license/resource risk | Keep gated | PoC phase; Conversion owner TBD | NOT_STARTED / NO |
| HO-RISK-016 | POC_PENDING — OCR/scanner/background removal | Accuracy/device/resource risk | Keep gated | PoC phase; Scanner/Image TBD | NOT_STARTED / NO |
| HO-RISK-017 | POC_PENDING — local/internal AI/summary/translate/Markdown | Data/accuracy/license/public-AI risk | No public fallback; keep unavailable | PoC phase; Intelligence/Security TBD | NOT_STARTED / NO |
| HO-RISK-018 | TECHNICAL_DEBT — UAT actual absent | Business acceptance unknown | D13 remains specification | Exact RC; Product/UAT TBD | NOT_RUN / NO |
| HO-RISK-019 | DOCUMENTATION_GAP — actual manual screenshots/labels absent | User/admin guidance may drift | Keep pre-implementation label | Before release manuals; Docs/Product TBD | OPEN / NO |
| HO-RISK-020 | PERTAMINA_CONFIRMATION_GAP — Production topology/SSO/network | No Production readiness | Only approved non-Prod work | Pre-Production; official owners TBD | REQUIRES_CONFIRMATION / NO |
| HO-RISK-021 | PERTAMINA_CONFIRMATION_GAP — signing/distribution | Cannot trust/update clients | No release package | Pre-Pilot/Production; Release/Security TBD | REQUIRES_CONFIRMATION / NO |
| HO-RISK-022 | PERTAMINA_CONFIRMATION_GAP — backup/DR/RPO/RTO | Recovery unknown | No Production claim | Pre-Production; Operations/Data TBD | REQUIRES_CONFIRMATION / NO |
| HO-RISK-023 | PERTAMINA_CONFIRMATION_GAP — owner/support/Service Desk | No accountable acceptance/escalation | Functional TBD labels | Before handover acceptance; Application Owner TBD | REQUIRES_CONFIRMATION / NO |
| HO-RISK-024 | TECHNICAL_DEBT — monitoring/alerts not provisioned | Blind/unactionable operations | Design only | Before Pilot; Observability Owner TBD | NOT_PROVISIONED / NO |
| HO-RISK-025 | TECHNICAL_DEBT — KT/teach-back not executed | Receiver competence unknown | KT plan only | Before transfer; Handover Owner TBD | PLANNED / NO |
| HO-RISK-026 | TECHNICAL_DEBT — runbook/drill/restore/rollback not executed | Operational recovery unknown | Tabletop design only | Before Pilot/Production; Ops TBD | NOT_RUN / NO |
| HO-RISK-027 | KNOWN_ISSUE — UI prototype can be confused with product | False RC/source/license/manual evidence | Label PROTOTYPE_ONLY; segregate | Baseline Freeze; Product/Docs TBD | OPEN / NO |
| HO-RISK-028 | PERFORMANCE_RISK — no measured budgets/targets | Server/client capacity unknown | Policy placeholders only | PoC/QA/Pilot; Performance/Ops TBD | OPEN / NO |
| HO-RISK-029 | SECURITY_FINDING — runtime secret/AuthZ/content controls untested | Disclosure/escalation risk | No Production access/provider | Security test phase; Security TBD | EVIDENCE_PENDING / NO |
| HO-RISK-030 | PERTAMINA_CONFIRMATION_GAP — CMDB/Application ID/classification/criticality/retention | Governance/archive/support incomplete | Markers remain visible | Before official handover; Business/Data/App TBD | REQUIRES_CONFIRMATION / NO |

## Closure rule

Item hanya DONE bila acceptance criteria, owner, evidence, reviewer, actual result, dependent updates, and closure reference tersedia. Risk acceptance memerlukan human authority; Codex/document tidak dapat mengubah Accepted menjadi YES.

