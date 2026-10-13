# TRACEABILITY COMPLETENESS REGISTER
## PERTAMINA OTO

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: FINAL AUDIT REGISTER — implementation/evidence overlay pending

---

# 1. Status Rantai

```text
Business Process → Requirement      : PARTIAL
Requirement → Feature               : PARTIAL — FR-FILE, UIR-UI, dan reverse-security gap
Feature identity/family → Module    : COMPLETE
33 Core → Feature/Test/UAT/Manual   : COMPLETE structural
Module → Component/Interface/Data   : PARTIAL — logical ownership exists; Admin/Audit/IPC/tree gaps open
Permission/Audit/Test design        : COMPLETE
Source/Artifact/Execution Evidence  : MISSING
Handover Evidence                   : MISSING
```

`COMPLETE` pada register ini berarti link dokumentasi tersedia, bukan implementasi selesai.
Scenario berikut adalah audit sample tingkat kapabilitas, bukan exact executable 550-row register. Range/prose seperti `FR-PROC-*` dan `Base` harus diekspansi menjadi ID/code canonical sebelum dipakai oleh CI atau task coding.

---

# 2. Scenario Trace Register

| Trace ID | Requirement → Feature | Core/Module | Interface/Data | Permission/Audit | Test/UAT/Manual/Runbook | Status |
|---|---|---|---|---|---|---|
| `TR-001` | `FR-PROC-001..005` → `FEAT-PDF-005` | Core 01 / Pdf | Local contract; optional IPC; Workspace/SQLite | `document.content.process`; L2 accepted/terminal | D11 Merge 50; D12 Merge 36; `UAT-CORE-001`; D14 Merge; Data Integrity | `PARTIAL` — code/artifact absent |
| `TR-002` | `FR-SCAN-*`, `FR-PROC-*` → `FEAT-OCR-004` | Core 09 / ScannerOcr | IPC or REST/Job; Workspace/Job Store | Base + Server when selected; L2 | OCR tests/UAT; malicious-doc/local-worker/queue runbook | `PARTIAL` — PoC/provider absent |
| `TR-003` | `FR-PROC-*`, `BR-GOV-006..009` → `FEAT-CONV-001` | Core 11 / Conversion | Local/IPC or optional REST/Worker | Base + Server when selected; L2 | Conversion tests/UAT/manual/runbook | `PARTIAL` |
| `TR-004` | Same conversion group → `FEAT-CONV-004` | Core 16 / Conversion | Local/IPC or optional REST/Worker | Base + Server when selected; L2 | PDF→Office tests/UAT/manual | `PARTIAL` |
| `TR-005` | `FR-PROC-*`, `SR-SEC-003` → `FEAT-PDF-061` | Core 27 / Pdf | Local; password memory-only; Workspace result | Base + `document.pdf.protect`; L2/L3 | Security tests; `UAT-CORE-027`; D14 | `PARTIAL` |
| `TR-006` | `FR-PROC-*`, `SR-SEC-003`, `SR-SEC-022` → `FEAT-PDF-062` | Core 26 / Pdf | Local; password memory-only | Base + `document.pdf.unlock`; L2/L3 | Negative tests; `UAT-CORE-026`; D14 | `PARTIAL` — D03 reverse link `SR-SEC-022` missing |
| `TR-007` | `FR-SIGN-001..003`, `BR-SIGN-001`, `SR-SEC-011..012` → `FEAT-SIGN-013` | Core 28 / SignatureAnnotation | Local service; protected asset; Workspace result | `document.signature.apply`; L2/L3 | Signature tests; `UAT-CORE-028`; D14 | `PARTIAL`; non-PKI correct |
| `TR-008` | `FR-PROC-*`, `SR-SEC-023` → `FEAT-PDF-069` | Core 29 / Pdf + Result Validation | Local; validated new result | `document.redaction.finalize`; L2/L3 | Forensic tests; `UAT-CORE-029`; Redaction runbook | `PARTIAL` — D03 reverse link `SR-SEC-023` missing |
| `TR-009` | `FR-INTEL-001`, `SR-SEC-021` → `FEAT-INTEL-001` | Core 31 / ContentIntelligence | Approved local/internal provider | Intelligence + optional Server; safe AI Audit | Quality/security/UAT; AI runbook | `PARTIAL` — provider/PoC absent |
| `TR-010` | `FR-INTEL-002`, `SR-SEC-021` → `FEAT-INTEL-002` | Core 32 / ContentIntelligence | Approved local/internal provider | Same Intelligence boundary | Translation tests/UAT/manual | `PARTIAL` |
| `TR-011` | `FR-INTEL-003`, `FR-PROC-*` → `FEAT-CONV-036` | Core 33 / Conversion/OCR | Deterministic local; complex route optional | Base + optional Server; L2 | Structure/OCR tests; `UAT-CORE-033` | `PARTIAL` |
| `TR-012` | `FR-BACKUP-*`, `FR-VER-*` → Backup/Version Features | N/A / Workspace-Secured | `/secured-files`; `secured.*` + object storage | Secured permissions; lifecycle Audit | Sync/storage/security tests; D14; Storage Failure | `PARTIAL` — runtime absent |
| `TR-013` | `FR-SUP-*`, `SR-SEC-019..020` → `FEAT-SUPPORT-*` | N/A / Support | Issue/attachment/export REST; `support.*` | Reporter/Support scoped permissions; Support events | Support tests/UAT; D14/D15; Issue runbook | `PARTIAL` |
| `TR-014` | `FR-ROLE-*`, `FR-AUTH-*` → `FEAT-ADMIN-003`, Auth Features | N/A / AccountAdministration | Typed Admin API required; `admin.*`/`identity.*` | assign/revoke + reauth; L3 | AuthZ negative/Admin UAT; SSO runbook | `MISSING` exact API contract |
| `TR-015` | `FR-AUDIT-*` → `FEAT-AUDIT-004` | N/A / Audit | Export REST/Job; `audit.*` + restricted object | Audit export scope; AuditAccess events | Audit tests/UAT; D15; Audit Failure | `PARTIAL` — binding/addendum |
| `TR-016` | `FR-REC-*`, `NFR-REC-*` → Recovery/Progress Features | Affected Core / Workspace | Typed local IPC; local job/checkpoint | Underlying Feature permission/Audit | IPC/recovery/fault tests; Local Worker runbook | `PARTIAL` — IPC artifact absent |
| `TR-017` | Recovery/API/performance requirements → affected Feature | Affected Core / Jobs | REST/queue/Worker/SignalR hint; `jobs.*` | Feature + Server scope; terminal evidence | API/job/load/recovery tests; queue/storage runbook | `PARTIAL` — runtime absent |
| `TR-018` | `IR-DEVOPS-*`, `NFR-ENV-*` → released set | N/A / Deployment | Azure DevOps/artifact/config/schema/evidence | Deployment authority; separate evidence plane | D12/D17; Bad Release/rollback | `MISSING` repository/artifact/environment |

---

# 3. Orphan Register

| Orphan ID | Type | Node/edge | Dampak | Tindakan | Status |
|---|---|---|---|---|---|
| `ORPH-001` | Core Tool | 33 cards → Feature | Tidak ada Core orphan | Pertahankan automated mapping lint | `COMPLETE` |
| `ORPH-002` | Requirement | `FR-FILE-006..009` | False reverse trace | Controlled D02/D03 disposition | `CONFLICT` |
| `ORPH-002A` | UI Requirement | `UIR-UI-015..016` | Undefined canonical reference | Requirement Owner disposition + downstream revalidation | `CONFLICT` |
| `ORPH-003` | Feature overlay | 550 → all downstream edges | Release drift sulit dideteksi | Generated governed trace register | `PARTIAL` |
| `ORPH-004` | Priority | UI exposure → 33 Core | 82 UI-primary labels dapat dianggap Core | Add `BuildPriority/CoreCardMembership` overlay | `MISSING` executable |
| `ORPH-005` | API/Data | Admin permission/session → route/persistence | Endpoint ad hoc atau direct-assignment drift | Controlled D06/D07 addendum; revalidate D09/D15 | `MISSING` |
| `ORPH-006` | Recovery mapping | Local Activity + server `/jobs`/Issue/resources → client view | Aggregation/reconciliation dapat berbeda per client | Utamakan owner resources; controlled aggregate hanya bila kebutuhan terbukti | `PARTIAL` |
| `ORPH-007` | Audit contract/data | D10 events → D06 batch/D07 table | Replay/dedupe/search tidak testable | Compatible D06/D07 extension | `CONFLICT` |
| `ORPH-008` | Authorization | Export → result retrieval | IDOR/cross-scope risk | Bind parent/current scope | `PARTIAL` |
| `ORPH-009` | Local integration | Desktop → Local Worker | IPC safety/recovery tidak testable | PoC + typed contract | `MISSING` |
| `ORPH-010` | Internal event | Owner → consumer | Hidden coupling/duplicate projection | Controlled event catalog | `MISSING` |
| `ORPH-011` | Physical component | Logical component → source/artifact | Actual placement unresolved | Validate during bootstrap | `MISSING` actual |
| `ORPH-012` | Provider | Gated Feature → approved engine | Availability dapat diklaim salah | PoC/provider decision | `MISSING` evidence |
| `ORPH-013` | Deployment | Feature/component → artifact/env | Tidak dapat membuktikan shipped capability | Pipeline + Release Manifest | `MISSING` actual |
| `ORPH-014` | Operations | Alert → owner/runbook/drill | Alert tidak actionable | Assign owner, threshold, drill | `PARTIAL` |
| `ORPH-015` | Handover | Asset/access/KT → acceptance | False closure risk | D19 GATE-1..10 | `MISSING` actual |
| `ORPH-016` | Security reverse trace | `SR-SEC-022/023` → Unlock/Redact Feature | Control dapat hilang dari generated coverage | Controlled D03 trace alignment | `PARTIAL` |
| `ORPH-017` | Audit interface | `audit.event.read` → detail projection | Admin/Auditor detail UI bisa ad hoc | GET-by-id atau typed exact-detail decision | `MISSING` |
| `ORPH-018` | Architecture presentation | D04 locked boundaries → conceptual tree | ContentIntelligence/Local Worker tidak terlihat pada tree | Controlled D04 reconciliation/non-exhaustive note | `CONFLICT` non-fatal |

Tidak ditemukan Requirement/Feature baru yang boleh dibuat otomatis dari orphan register.

---

# 4. Target Generated Register pada Repository

Minimum field:

```text
TraceRowId
RequirementIds[]
FeatureId
CoreCardId / CoreCardMembership
BuildPriority
ModuleId / ComponentId / ApplicationService
InterfaceIds[]
DataOwner / DataStores[]
PermissionCodes[] / ScopeRule
AuditEventTypes[] / AuditLevel
ProcessingMode / ServerDependency
PlatformExposure
PoCDecisionId
UnitTestFamily / TestPlanFamily / UatIds[]
ManualRefs[] / RunbookIds[]
ArtifactTypes[] / ReleaseManifestRef
LifecycleStatus / TraceStatus
GapDecisionIds[] / EvidenceRefs[]
SourceVersions / LastValidatedUtc
```

CI harus menolak duplicate/unknown ID, invalid range, Core count selain 33, Feature count selain 550, local-only server side effect, missing owner edge, dan artifact/manifest mismatch.
