# CROSS-DOCUMENT CONSISTENCY MATRIX
## PERTAMINA OTO — Baseline 01–19

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: FINAL AUDIT REGISTER — bukan source of truth baru

Legenda: `COMPLETE`, `PARTIAL`, `MISSING`, `CONFLICT`, `NOT_APPLICABLE`, `CONFIRMATION_REQUIRED`.

| ID | Area | Authority utama | Downstream utama | Status | Temuan/gap | Disposisi |
|---|---|---|---|---|---|---|
| `XDC-001` | Business Process | D01 | D02–D19 | `COMPLETE` | Actual owner/approval/rollout evidence pending | Pertahankan D01; confirmation register |
| `XDC-002` | Requirement | D02 | D03–D19 | `PARTIAL` | `FR-FILE-006..009` tidak mempunyai heading canonical | Controlled D02/D03 revision; no invented ID |
| `XDC-002A` | UI Requirement | D02 | D03–D19 | `CONFLICT` | `UIR-UI-015..016` direferensikan, tetapi definisi berhenti pada `UIR-UI-014` | Requirement Owner disposition; controlled revision dan downstream revalidation |
| `XDC-003` | Feature identity/count | D03 | D04–D19 | `COMPLETE` | 550 unique; 0 duplicate; 0 active 546 | D03 tetap authority |
| `XDC-004` | 33 Core priority | D03 | D06/D09/D11–D18 | `COMPLETE` structural / `PARTIAL` executable | `BuildPriority/CoreCardMembership` belum generated | Generated overlay pada bootstrap |
| `XDC-005` | HTML-to-PDF risk | D03 + D08 | D06/D12/D18 | `CONFLICT` | D03 LOW; downstream MEDIUM | Terapkan MEDIUM control; controlled canonical alignment |
| `XDC-006` | Architecture | D04 | D05–D19 | `PARTIAL` downstream trace | 18/18 locks ada; internal event, Activity, IPC, dan physical owner belum executable | Buat 18-row implementation cross-check; no architecture invention |
| `XDC-007` | Technology | D05 | D11/D12/D17/D18 | `PARTIAL` implementation | Exact SDK/package/provider absent | Phase-0 confirmation dan PoC |
| `XDC-008` | Hybrid API surface | D06 | D07–D19 | `COMPLETE` baseline | Runtime OpenAPI artifact absent | Generate/lint/contract test pada implementation |
| `XDC-009` | Admin API/persistence | D09/D15 need; D06/D07 authority | D15/D18 | `MISSING` | User/session/role/permission route belum typed; direct permission assignment belum punya persistence mapping | Controlled D06/D07 addendum; revalidate D09/D15 |
| `XDC-010` | Activity/monitoring API | D06/D15/D18 | Client/Admin | `PARTIAL` | `/jobs` dan owner resources ada; cara menggabungkan server REST dengan local IPC/Issue belum eksplisit | Utamakan existing `/jobs`/resource API; buat aggregate baru hanya bila kebutuhan terbukti |
| `XDC-011` | Audit batch/database contract | D06 + D07 + D10 | D11/D12 | `CONFLICT` detail | Per-event type/version/ID/sequence/provenance/ack, dedupe, dan index semantics kurang | Backward-compatible D06/D07 addendum dan D10 alignment |
| `XDC-011A` | Audit Detail | D09/D15 need; D06 authority | Admin/Auditor UI | `MISSING` | `audit.event.read` tersedia, tetapi GET-by-id atau exact-detail projection belum eksplisit | Controlled typed projection decision + authorization tests |
| `XDC-012` | Export result authorization | D06/D09/D10 | D15/D18 | `PARTIAL` | Parent/resource binding belum lengkap | Bind result ke origin/current scope |
| `XDC-013` | Database ownership | D07 | D08–D19 | `COMPLETE` logical | Actual DbContext/migrations/instance absent | Phase 0–3 implementation evidence |
| `XDC-014` | `secured.*` naming | D07 | D04/D06/D17/D18 | `COMPLETE` | D04 lama memakai contoh `backup.*` | D07 `secured.*` authoritative; no new schema |
| `XDC-015` | Security boundary | D08 | D09–D19 | `COMPLETE` design | SSO/profile/VA-PT/runtime proof pending | Pertamina confirmation + executable evidence |
| `XDC-016` | Role/Permission | D09 | D10–D19 | `COMPLETE` design | Exact IAM mapping/Admin contracts pending | No God Mode; controlled API/IAM mapping |
| `XDC-017` | Audit vs History/Log/Issue | D10 | D11–D19 | `COMPLETE` | Retention and degraded L2 policy pending | Maintain separate planes; confirm policy |
| `XDC-018` | Job lifecycle | D06/D07 | D10–D18 | `COMPLETE` | Runtime unavailable | `WAITING_CONNECTION/RESOURCE`; no `PAUSED` |
| `XDC-019` | Local Worker IPC | D04/D05/D08/D18 | D11/D12/D17 | `MISSING` executable | Exact wire/auth/version/compatibility absent | PoC + typed contract artifact |
| `XDC-020` | Unit Test | D11 | D12/D13/D19 | `COMPLETE` specification / `MISSING` results | 50 Merge cases belum executable | Implement pada Phase 3; zero-test fails |
| `XDC-021` | Test Plan | D12 | D13/D17/D19 | `COMPLETE` design / `MISSING` results | Environment/corpus/targets absent | QA gate; no fabricated PASS |
| `XDC-022` | UAT | D13 | D14–D19 | `COMPLETE` specification / `MISSING` execution | Actual `NOT_RUN`; 0 sign-off | Exact RC + human UAT later |
| `XDC-023` | User Manual | D14 | D19 | `PARTIAL` release readiness | TBD labels; 0 actual screenshot | Release edition only from exact RC |
| `XDC-024` | Admin Manual | D15 | D16/D19 | `PARTIAL` release readiness | UI/contract/SSO/monitoring absent | Keep pre-implementation status |
| `XDC-025` | Incident/Support | D16 | D17/D19 | `PARTIAL` operations | Owner/contact/SLA/drill absent | Confirm organization; execute drills |
| `XDC-026` | Deployment | D17 | D18/D19 | `COMPLETE` design / `MISSING` evidence | Repo/pipeline/artifact/env/restore absent | Build-once/promotion evidence later |
| `XDC-027` | Cartography | D18 | D19 | `PARTIAL` | Actual path/artifact/owner/CMDB missing | Conceptual source + later actual overlay |
| `XDC-028` | Handover | D19 | Master | `MISSING` actual | GATE-1..10 pending; KT/access/sign-off absent | Preserve `NOT_HANDOVER_READY` |
| `XDC-029` | Language standard | ID-1.0 + Master | D01–D19 | `PARTIAL` | Mixed narrative; prior 10/10 PASS not reproducible | Controlled language re-audit |
| `XDC-030` | Configuration control | Baseline Manifest | D01–D19/Master | `PARTIAL` | Divergent snapshots, duplicate paths, missing Master snapshots | Human acceptance + archive policy |
| `XDC-031` | Pertamina standard | Supplied PDF + official sources TBC | D01/D04/D05/D08/D17/D19 | `CONFIRMATION_REQUIRED` | Original Fax/RIC, WinUI, server split, Azure PaaS, CSAA/L2 | Obtain official confirmation |
| `XDC-032` | Project status | D19 + Master | Semua | `COMPLETE` truthful | Implementation/Production/handover not complete | Do not claim project complete |
| `XDC-033` | PDF security reverse trace | D02/D03 | D08/D11–D18 | `PARTIAL` | Unlock/Redact Feature belum menautkan `SR-SEC-022/023`; Protect/Sign grouping perlu dipisah | Controlled D03 trace revision; pertahankan downstream security controls |
| `XDC-034` | D04 conceptual tree | D04 | D18/repository bootstrap | `CONFLICT` presentation | ContentIntelligence dan Local Worker locked, tetapi tidak tampil pada tree D04 | Controlled D04 tree reconciliation atau nyatakan tree non-exhaustive |

---

# Invariant Lint

| Invariant | Hasil |
|---|---|
| 550 Feature canonical | PASS |
| 33 Primary PDF Tool Cards | PASS |
| Merge PDF first vertical slice | PASS |
| Local-only tool tidak dipaksa ke server | PASS desain |
| REST authority / SignalR notification | PASS |
| `uploadId` dan `fileRef` terpisah | PASS |
| `PAUSED` tidak menjadi Job state | PASS |
| Admin bukan God Mode | PASS |
| Support tidak otomatis membaca content | PASS |
| Auditor no write | PASS |
| Assistant no independent permission | PASS |
| Public AI dilarang | PASS |
| Sign PDF non-PKI | PASS |
| Max 3 secured versions | PASS |
| Source no silent overwrite | PASS |
| Screenshot Issue explicit | PASS |
| Content excluded from Audit/telemetry | PASS desain |
| Hostinger bukan Production | PASS |
| UAT/handover/closure belum selesai | PASS truthful status |

Matriks ini harus digenerate/ditautkan ke source dan release evidence setelah repository tersedia; pemeliharaan manual tidak boleh menjadi authority baru.
