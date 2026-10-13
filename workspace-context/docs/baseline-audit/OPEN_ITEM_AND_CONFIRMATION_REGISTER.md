# OPEN ITEM AND CONFIRMATION REGISTER
## PERTAMINA OTO — Documentation Baseline Audit

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: ACTIVE  
> Aturan: open item bukan permission untuk mengarang nilai, owner, vendor, topology, atau approval.

---

# 1. Open Item Register

| ID | Kategori | Temuan | Severity | Status | Earliest block | Exit evidence |
|---|---|---|---|---|---|---|
| `BASE-OI-001` | Configuration | D01–D05 snapshot lama mempunyai filename/version sama dengan current tetapi hash berbeda | `MAJOR` | `OPEN` | Baseline acceptance | Superseded/archive marker + replacement hash |
| `BASE-OI-002` | Configuration | D07/D08 duplicate identik pada root dan folder canonical | `MAJOR` | `ACCEPTED_WITH_CONDITION` | Future edit | Human accepts canonical paths; archive/pointer policy |
| `BASE-OI-003` | Configuration | Master v3.4/v4.0/v4.1 referenced snapshots tidak tersedia | `MAJOR` | `OPEN` | Reproducible provenance | Restore or approved loss record |
| `BASE-OI-004` | Language | Prior 10/10 language PASS tidak reproducible | `MAJOR` | `OPEN` | Formal language acceptance | Hash-bound re-audit + reviewer |
| `BASE-OI-005` | Reference | Logical/physical filename dan historical reference mapping belum lengkap | `MINOR` | `OPEN` | Automation | Manifest mapping; 0 unresolved current ref |
| `BASE-OI-006` | Metadata | Owner/reviewer/approver/date/changelog/parent hash tidak seragam | `MINOR` | `DEFERRED` | Formal publication | Controlled metadata revision schema |
| `BASE-OI-007` | Requirement | `FR-FILE-006..009` tidak canonical; references tersebar pada D03/D08/D11/D12/D14/D18/D19 | `BLOCKER` | `OPEN` | Trace freeze/affected coding | Requirement Owner disposition + seluruh downstream revision/revalidation terkait |
| `BASE-OI-008` | Feature risk | HTML-to-PDF LOW vs MEDIUM | `MAJOR` | `OPEN` | HTML PoC/implementation | Canonical risk alignment; Medium control retained |
| `BASE-OI-009` | API/Data | Admin user/session/role/permission contract dan direct-assignment persistence mapping missing | `BLOCKER` | `OPEN` | Admin implementation | Reviewed D06/D07 addendum, D09/D15 revalidation, contract/auth tests |
| `BASE-OI-010` | API/Audit/Data | Audit batch/database per-event semantics, dedupe/index, dan Audit Detail projection incomplete | `BLOCKER` | `OPEN` | Offline Audit/Admin Audit implementation | Compatible D06/D07 contract/schema, D10 alignment, tests |
| `BASE-OI-011` | API | Mapping local Activity + server `/jobs`/Issue/resources belum eksplisit; aggregate baru belum terbukti perlu | `MAJOR` | `OPEN` | Client/Admin integration | Reconciliation contract memakai existing owner resources; aggregate baru hanya melalui controlled decision |
| `BASE-OI-012` | Authorization | Audit/Support export result binding incomplete | `MAJOR` | `OPEN` | Export implementation | Origin/current scope authorization tests |
| `BASE-OI-013` | IPC | Local Worker exact wire/auth/version contract absent | `BLOCKER` | `OPEN` | Local Worker implementation | IPC PoC + contract artifact + negative tests |
| `BASE-OI-014` | Integration | Internal event owner/schema/consumer catalog absent | `MAJOR` | `OPEN` | Async integration | Controlled event catalog |
| `BASE-OI-015` | Traceability | Generated exact 550-row implementation overlay absent | `MAJOR` | `OPEN` | Release trace gate | Generated register + CI validation |
| `BASE-OI-016` | Cartography | Actual source path/component/data-owner/artifact overlay absent | `IMPLEMENTATION_PENDING` | `OPEN` | Handover | Source-backed D18 actual overlay |
| `BASE-OI-017` | Repository | Official Git repo/project/access/owner absent | `BLOCKER` | `OPEN` | All formal coding | Approved repo reference + access + owner |
| `BASE-OI-018` | Toolchain | Exact SDK patch, package sources, initial dependency approval absent | `BLOCKER` | `OPEN` | Phase 0 | Approved values and clean restore |
| `BASE-OI-019` | DevOps | Branch/PR/pipeline authority and artifact storage absent | `IMPLEMENTATION_PENDING` | `OPEN` | Phase 0 exit/QA | Green PR pipeline + immutable reports |
| `BASE-OI-020` | PoC | Engine/provider/device/license/security decisions absent | `IMPLEMENTATION_PENDING` | `OPEN` | Affected adapter/Feature | Governed PoC disposition |
| `BASE-OI-021` | Testing | Executable Unit/Integration/Security/Performance evidence absent | `EVIDENCE_PENDING` | `OPEN` | QA/UAT | Results tied to build/env/corpus |
| `BASE-OI-022` | UAT | Actual UAT `NOT_RUN`; 0 sign-off | `EVIDENCE_PENDING` | `OPEN` | Pilot/Production | Exact-RC UAT + human sign-off |
| `BASE-OI-023` | Manual/UI | Actual labels/screenshots/release validation absent | `EVIDENCE_PENDING` | `OPEN` | UAT/release manual | Approved RC screenshots and documentation tests |
| `BASE-OI-024` | Production | Topology/network/Azure/server-separation decisions absent | `CONFIRMATION_REQUIRED` | `OPEN` | Production | Approved architecture/infrastructure record |
| `BASE-OI-025` | Identity/Security | SSO/IAM/MFA/reverse proxy/VA-PT details absent | `CONFIRMATION_REQUIRED` | `OPEN` | Integrated QA/Pilot | Approved security profile/evidence |
| `BASE-OI-026` | Operations | Monitoring, alert, Service Desk, L1/L2/L3, owner/contact/drill absent | `CONFIRMATION_REQUIRED` | `OPEN` | Pilot/operations | Active monitored service + exercised runbooks |
| `BASE-OI-027` | Recovery | Backup/restore/DR/RPO/RTO/retention evidence absent | `CONFIRMATION_REQUIRED` | `OPEN` | Production | Approved policy + successful exercise |
| `BASE-OI-028` | Handover | KT, access transfer, receiver, sign-off, closure absent | `EVIDENCE_PENDING` | `OPEN` | Handover/closure | D19 gates/evidence/signatures complete |
| `BASE-OI-029` | Pertamina process | CSAA dan update daftar L2 Service Desk belum dipetakan/dibuktikan | `CONFIRMATION_REQUIRED` | `OPEN` | Handover | Official function/template/update evidence |
| `BASE-OI-030` | Pertamina standard | Original Fax/RIC, WinUI/MAUI, Azure PaaS, server separation, dan makna label D01 `Selaras` belum mendapat konfirmasi resmi | `CONFIRMATION_REQUIRED` | `OPEN` | Irreversible architecture/infra decision atau false compliance claim | Official source, IT C&T response, dan compliance sign-off; sampai itu label D01 berarti keselarasan baseline internal, bukan sertifikasi |
| `BASE-OI-031` | UI Requirement | `UIR-UI-015..016` direferensikan D02, tetapi definisi canonical berhenti pada `UIR-UI-014` | `BLOCKER` | `OPEN` | Trace freeze/affected UI coding | Requirement Owner disposition + controlled D02 revision + downstream revalidation |
| `BASE-OI-032` | Security trace | Protect/Unlock/Sign/Redact requirement grouping dan reverse link `SR-SEC-022/023` belum seragam | `MAJOR` | `OPEN` | Affected security implementation | Controlled D03 trace correction; downstream controls tetap diberlakukan |
| `BASE-OI-033` | Architecture presentation | D04 mengunci ContentIntelligence/Local Worker tetapi conceptual tree tidak menampilkannya | `MAJOR` | `OPEN` | Repository tree approval | Controlled D04 tree reconciliation atau approved non-exhaustive interpretation; D18 tidak dianggap silent D04 revision |

---

# 2. Confirmation Groups

| ID | Kelompok | Konfirmasi minimum | Status |
|---|---|---|---|
| `CONF-001` | Business Owner | Fungsi owner, VP approval, rollout scope | `OPEN` |
| `CONF-002` | Product/Application Owner | Scope, priority, release authority | `OPEN` |
| `CONF-003` | Repository | Azure DevOps org/project/repo/default branch | `OPEN` |
| `CONF-004` | Access | Developer/reviewer/service identities; grant/revoke | `OPEN` |
| `CONF-005` | Toolchain | .NET SDK patch, agent image, package feeds | `OPEN` |
| `CONF-006` | Dependency/License | Package/provider admission, license/cost reviewer | `OPEN` |
| `CONF-007` | Architecture Standard | WinUI 3, MAUI, server separation, Azure PaaS interpretation | `OPEN` |
| `CONF-008` | Pertamina Source | Fax SVP EIT dan Template RIC 2026 asli | `OPEN` |
| `CONF-009` | Identity | SSO protocol, claims, registration, provisioning, session/offline grant | `OPEN` |
| `CONF-010` | Reauthentication/MFA | Method, assurance/freshness, recovery | `OPEN` |
| `CONF-011` | Security | VA/PT, severity, waiver, risk acceptance, incident route | `OPEN` |
| `CONF-012` | Secret/Certificate | Custody, rotation, workload identity, signing | `OPEN` |
| `CONF-013` | Data Governance | Classification, retention, legal hold, deletion | `OPEN` |
| `CONF-014` | Provider/AI | Location, egress, training/reuse, retention, subprocessor, exit | `OPEN` |
| `CONF-015` | Infrastructure | Dev/QA/Pilot/Production host/topology/OS/runtime | `OPEN` |
| `CONF-016` | Network | DNS/TLS/proxy/port/firewall/private connectivity | `OPEN` |
| `CONF-017` | Database/Storage/Queue | Product/topology/owner/access/capacity | `OPEN` |
| `CONF-018` | Monitoring/APM | Backend/dashboard/owner/threshold/retention | `OPEN` |
| `CONF-019` | Backup/DR | RPO/RTO, topology, retention, restore/failover drill | `OPEN` |
| `CONF-020` | Packaging/Distribution | Desktop/Mobile signing/channel/rollout/withdrawal | `OPEN` |
| `CONF-021` | Support | Service Desk, L1/L2/L3, contact, hours, escalation | `OPEN` |
| `CONF-022` | Handover/CSAA | Receiver, authorization template, access transfer, sign-off | `OPEN` |
| `CONF-023` | KT/Operations | Participants, competency, side-by-side, observation | `OPEN` |
| `CONF-024` | Closure | Acceptance quorum, residual risk, project closure authority | `OPEN` |

---

# 3. Phase Rule

```text
BLOCKS CODING     : CONF-003..006 + owner/access + accepted manifest
BLOCKS POC        : corpus, candidate/license, security boundary, reviewer
BLOCKS QA         : artifact, environment, test data, scan policy, results
BLOCKS PILOT      : UAT readiness, signing, monitoring/support, rollback/restore
BLOCKS PRODUCTION : all Production/security/data/recovery/operations approvals
DOES NOT BLOCK CURRENT PLANNING:
  final Production topology, CMDB, Production SLA/RPO/RTO,
  final support roster, mobile distribution, closure signatures
```

Tidak ada open item yang boleh ditutup hanya karena dokumen perencanaan tersedia.
