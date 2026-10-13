# AGENT D — ESCALATION, PROBLEM MANAGEMENT, KNOWLEDGE BASE & OPERATIONS QA REPORT

> **Task ID:** `DOC16-AGENT-D`  
> **Peran:** Senior Project Manager; Service Support Architect; Reliability Operations Architect; Problem Management Analyst; Knowledge Management Architect; Incident Readiness/Test Facilitator  
> **Status laporan:** `PRE-IMPLEMENTATION BASELINE` — rekomendasi specialist untuk integrasi; bukan master Incident Support Guide dan bukan bukti kesiapan operasi Produksi  
> **Baseline:** Documents 01–15 FINAL; 550 Feature canonical; 33 Primary PDF Tool Cards  
> **Status master yang disarankan:** `FINAL — PRE-IMPLEMENTATION INCIDENT SUPPORT GUIDE BASELINE`  
> **Aplikasi/RC:** `RC-TBD`  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`  
> **Tanggal analisis:** 1 September 2026

Laporan ini merancang ownership, escalation, Problem Management, Knowledge Base, operations QA, tabletop, dan handoff deployment secara ringan namun traceable. Laporan tidak membuat organisasi/nama unit Pertamina, tidak menetapkan SLA/response number resmi, tidak membuat kontrak vendor, tidak menulis Deployment Guide, tidak membuat Feature/role/permission/API/Audit Event baru, tidak mengedit master Document 16, dan tidak melakukan coding.

Baseline yang wajib dipertahankan:

- Issue Report, Incident, Defect, Problem, Known Error, Enterprise Audit, User History, dan Observability adalah record/lifecycle yang berbeda; tautkan dengan ID aman, jangan gabungkan enum atau payload;
- Support/Superadmin adalah profil permission scoped, bukan role formal atau wildcard;
- Incident ownership tidak memberi akses document content, secured-file content, OCR, Assistant prompt, visual signature, token, secret, raw storage, atau unrestricted log;
- containment harus paling tidak destruktif, dapat dipulihkan, dan sesuai permission/approval;
- evidence minimum memakai identifier, timestamp, build/config, safe logs/metrics, Audit reference, Job/Correlation ID, dan artifact digest yang diizinkan—bukan isi sensitif;
- Production tidak digunakan untuk serangan, malicious corpus, fault injection, load/stress, atau latihan destruktif;
- mandatory Audit/security/authorization tidak boleh dimatikan untuk memulihkan layanan;
- local-capable feature tetap dipertahankan saat server dependency gagal jika policy/capability/session mengizinkan;
- REST/persisted state tetap source of truth; SignalR hanya notifikasi dan recovery melalui REST;
- public/unapproved AI provider tidak boleh menjadi fallback;
- recovery/restore dinyatakan berhasil hanya setelah integrity, authorization, state, object, Audit, dan security smoke checks lulus;
- rollback/hotfix/migration/infrastructure/restore/config rollout selalu membutuhkan prosedur deployment yang disetujui di Document 17;
- target waktu, severity authority, organization mapping, on-call route, privacy/legal notification, RPO/RTO, evidence retention/hold, dan vendor contact tetap menunggu konfirmasi resmi.

---

# 1. Responsibility model

## 1.1 Prinsip ownership

1. Setiap Incident aktif mempunyai tepat satu **Incident Owner** pada satu waktu.
2. Incident Owner mengoordinasikan; fungsi tersebut tidak otomatis menjadi technical expert, approver, atau content reader.
3. Fungsi dapat dirangkap pada model satu developer, tetapi “hat” dan keputusan dicatat terpisah.
4. Codex dapat membantu analisis terbatasi, tetapi tidak menjadi owner, approver, security authority, atau evidence custodian.
5. Ownership tidak mengalahkan Role Matrix, segregation of duties, resource scope, reauthentication, atau environment boundary.
6. Setiap nama orang/unit/rotasi on-call harus diisi organisasi Pertamina yang berwenang.

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION
```

## 1.2 Fungsi konseptual

| Fungsi | Tanggung jawab | Tidak otomatis berwenang untuk |
|---|---|---|
| Reporter | Melaporkan gejala, dampak yang terlihat, waktu, dan safe evidence | Menentukan severity final, root cause, atau mengakses data lain |
| Support/Triage | Validasi/deduplikasi, klasifikasi awal, safe checks, Issue-to-Incident conversion, assignment | General document/job/storage/Audit access; containment teknis arbitrer |
| Incident Owner | Satu sumber koordinasi, scope, timeline, update, escalation, decision log, closure gate | Deploy, restore, policy override, security/legal decision tanpa approver |
| Technical Investigator | Diagnosis komponen, hipotesis, safe reproduction, proposed containment/recovery/fix | Content access otomatis, direct Production mutation, approval sendiri |
| Administrator | Safe control-plane read/change dalam `ADMIN_SCOPE` dan permission yang berlaku | User document content, generic Job control, Audit/Support access implisit |
| Security Reviewer | Security classification, containment/evidence guidance, control validation, notification input | Business risk acceptance atau legal conclusion tanpa authority resmi |
| Product/Business Owner | Menilai business impact, priority, workaround acceptability, Core Feature impact | Menurunkan security/privacy/Audit requirement atau mengesahkan deploy teknis |
| Auditor | Membaca evidence Audit yang disetujui dalam `AUDIT_SCOPE` | Mengubah Audit, Incident, config, Issue, atau resource content |
| Deployment/Operations Approver | Menyetujui rollout, rollback, hotfix, migration, restore, infra/config change | Mengabaikan QA/security/change evidence atau melakukan approval palsu |

## 1.3 Assignment dan transfer

Incident assignment minimum:

```text
Incident ID
Current Incident Owner function
Assigned principal reference
Assignment timestamp
Reason
Accepted timestamp
Previous owner/reference
Current severity proposal/authority
Environment/scope
Next checkpoint/update target reference
```

Transfer ownership hanya selesai setelah penerima menerima, current state/timeline/risk/next action/evidence location dijelaskan, dan Audit/record update yang diwajibkan tercatat. Mengirim pesan tanpa acceptance bukan transfer.

## 1.4 Lightweight RACI

| Aktivitas | Responsible | Accountable/decision | Consulted | Informed |
|---|---|---|---|---|
| Intake/validate/dedupe | Support/Triage | Incident Owner setelah declare | Reporter, Technical Investigator | Product/Business Owner bila material |
| Set/ubah severity | Incident Owner | Authority yang dikonfirmasi | Support, Technical, Security, Business | Stakeholder sesuai policy |
| Technical diagnosis | Technical Investigator | Incident Owner mengoordinasikan | Administrator/Security/Provider | Business Owner |
| Security containment | Technical + Administrator yang berizin | Security/Operations authority yang dikonfirmasi | Incident Owner, Auditor bila relevan | Required stakeholders |
| Business workaround | Product/Business Owner + Technical | Business authority | Security, Incident Owner | Reporter/user group sesuai policy |
| Deploy/rollback/restore | Approved deployment function | Deployment/Operations Approver | Technical, Security, Incident Owner | Business/Support/Auditor sesuai policy |
| Verify recovery | Technical + Support/Business verifier | Incident Owner | Security/Auditor bila relevan | Stakeholders |
| PIR/problem conversion | Incident Owner/Problem Owner function | Business/Security/Operations authority sesuai scope | Semua fungsi terkait | Knowledge owner |
| Closure | Incident Owner | Closure authority yang dikonfirmasi | Support, Technical, Security, Auditor as applicable | Reporter/stakeholders |

## 1.5 Incident record minimum

```text
Incident ID; Title; Source Issue/Alert/Defect IDs
DetectedAt; ReportedAt; AcknowledgedAt; RestoredAt; ResolvedAt; ClosedAt
Environment; Build/Artifact/Config/Policy/Schema/Capability versions
Affected component/platform/Feature IDs; Core impact
Severity proposal/current/authority; status
Reporter; Incident Owner; involved functions
Business/user/security/data impact and scope
Symptoms; safe observations; safe Correlation/Job/Error references
Timeline and decision log
Containment; workaround; recovery; verification
Evidence manifest references; Audit references
Linked Defect/Problem/Known Error/Change/Deployment records
Communication records; target fields and breach/escalation state
Closure/PIR/KB/runbook/documentation status
```

Incident record tidak menyimpan password, token, secret, document/OCR/Assistant content, visual signature, raw malicious payload, unrestricted Issue attachment, storage key, raw SQL/stack/body, atau exploit detail sensitif.

## 1.6 Lifecycle konseptual

```text
REPORTED → ACKNOWLEDGED → TRIAGED → RESPONDING
→ MONITORING → RESOLVED → CLOSED

Disposition: DUPLICATE / NOT_AN_INCIDENT / MERGED
```

Lifecycle ini adalah baseline dokumentasi dan memerlukan persetujuan sebelum menjadi enum sistem. `RESOLVED` berarti layanan/outcome dipulihkan dan diverifikasi; `CLOSED` memerlukan closure quality gate. Issue statuses Document 15 dan defect statuses Document 12/13 tetap terpisah.

---

# 2. Escalation model

## 2.1 Prinsip

- Escalation adalah pemindahan perhatian, keahlian, authority, atau decision—bukan perluasan data access.
- Incident Owner tetap accountable sampai transfer eksplisit diterima.
- Beberapa jenis escalation dapat aktif bersamaan.
- Escalation mencatat trigger, requested decision, recipient function, safe evidence, target reference, acknowledgement, dan outcome.
- Escalation tidak mengizinkan shared credential, direct DB/storage change, unapproved provider, atau Production experiment.

## 2.2 Jenis escalation

| Jenis | Tujuan | Trigger umum | Fungsi penerima konseptual | Requested outcome |
|---|---|---|---|---|
| FUNCTIONAL | Validasi business workflow/expected outcome | Core behavior salah, user journey blocked, requirement ambigu | Product/Business Owner | Clarify impact/priority/workaround/expected behavior |
| TECHNICAL | Diagnosis atau fix lintas komponen | Repeated failure, crash, data/state mismatch, no safe workaround | Technical Investigator/engineering function | Root hypothesis, containment, fix/test plan |
| SECURITY | Kontrol/identity/data exposure/abuse | Cross-user exposure, secret leak, malicious file, Audit/security failure | Security Reviewer/approved security function | Classification, containment, evidence/notification guidance |
| INFRASTRUCTURE | Host/network/database/storage/capacity/identity platform | Production unavailable, storage/DB/SSO/queue resource failure | Approved Operations/Infrastructure function | Restore platform capacity/readiness using approved procedure |
| BUSINESS | Dampak layanan/priority/risk/workaround | Core outage, business deadline/operation affected | Product/Business Owner | Business decision, communication, workaround acceptance |
| VENDOR/PROVIDER | Approved dependency defect/availability | Provider outage/defect/license/support need | Approved provider-management function | Case/reference, workaround/fix estimate—no contract invention |
| MANAGEMENT | Authority, coordination, risk, resource | SEV-1/SEV-2, target exceeded, scope expanding, blocked decision | Confirmed management authority | Priority/resource/decision/communication authorization |

## 2.3 Escalation record

```text
Escalation ID
Incident ID
Type
Trigger and detected time
Current severity/scope/environment
Decision/help requested
Recipient function and accepted-by reference
Safe evidence references
Target policy reference and current status
Acknowledged/updated/resolved timestamps
Outcome/decision/conditions
Next escalation or fallback
```

## 2.4 L1/L2/L3 compatibility

Document 01 menyebut jalur L1/L2/L3. Gunakan sebagai tier konseptual, bukan nama organisasi:

- L1: intake, safe checks, knowledge use, routing;
- L2: application/operations investigation dan approved admin action;
- L3: specialist engineering/security/provider analysis dan permanent fix.

Mapping fungsi, jam layanan, kontak, on-call, dan batas authority:

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION
```

---

# 3. Escalation triggers

## 3.1 Trigger wajib

| Trigger | Escalation minimum | Catatan aman |
|---|---|---|
| Proposed/confirmed SEV-1 atau SEV-2 | MANAGEMENT + relevant FUNCTIONAL/TECHNICAL/SECURITY/INFRASTRUCTURE | Severity model masih `TARGET_TO_BE_APPROVED` |
| Suspected/confirmed data exposure | SECURITY + MANAGEMENT + BUSINESS per policy | Jangan browse untuk mengukur scope; preserve safe evidence |
| Repeated incident | TECHNICAL + Problem Management | Tautkan incident sebelumnya, jangan duplicate payload |
| No safe workaround | TECHNICAL + BUSINESS | Jangan membuat bypass permission/security |
| Core Feature outage | FUNCTIONAL + TECHNICAL; MANAGEMENT sesuai severity | Catat Feature ID/platform/processing route |
| Production unavailable | INFRASTRUCTURE + TECHNICAL + MANAGEMENT | Local capability may continue if safe; no false global outage claim |
| Recovery/restore failed | INFRASTRUCTURE + TECHNICAL + SECURITY as applicable | Stop repeat destructive attempts; preserve result/evidence |
| Scope expanding | Incident Owner escalates severity/management/security | Record old/new scope and evidence |
| Response target exceeded/at risk | MANAGEMENT/next support tier | Target remains policy-configurable |
| Vendor/provider dependency | VENDOR/PROVIDER + TECHNICAL | Approved contact only; no public fallback |
| Security control failure | SECURITY + MANAGEMENT | Authorization/Audit/quarantine/public-AI/secret control examples |
| Mandatory Audit not durable/integrity at risk | SECURITY + TECHNICAL + AUDIT owner | Privileged actions may fail closed; never disable Audit |
| Unauthorized privileged config/export | SECURITY + MANAGEMENT + Auditor as applicable | Do not use Audit visibility as content grant |
| Data loss/corruption/false success | TECHNICAL + SECURITY + BUSINESS | Protect source/current state; no cleanup before evidence decision |

## 3.2 Severity working model

Incident severity berbeda dari defect severity (`BLOCKER/CRITICAL/MAJOR/MINOR`) dan vulnerability severity (`Critical/High/Medium/Low`). Tidak ada automatic mapping.

| Working level | Impact posture | Candidate criteria | Target state |
|---|---|---|---|
| `SEV-1` | Enterprise/critical safety or business impact | Widespread Production unavailable; active cross-user/secret exposure; corruption/data loss; critical auth/Audit/security control failure; recovery cannot proceed safely | `TARGET_TO_BE_APPROVED` |
| `SEV-2` | Significant degradation/material scope | Core capability unavailable for material population; limited workaround; repeated cross-component failure; capacity/provider problem with broad impact | `TARGET_TO_BE_APPROVED` |
| `SEV-3` | Limited operational impact | Scoped users/component; safe workaround; no security/data-integrity impact; recovery path known | `TARGET_TO_BE_APPROVED` |
| `SEV-4` | Low-impact observation/request | Minimal impact, informational anomaly, documentation/operational improvement | `TARGET_TO_BE_APPROVED` |

Model ini hanya proposal. Definisi, authority, reassessment cadence, downgrade/upgrade, and communication:

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
```

## 3.3 Reassessment

Reassess severity when scope, user/business impact, security/data impact, workaround, affected Features/platforms, recovery confidence, provider dependency, or target status changes. Downgrade tidak menghapus timeline, escalation, atau evidence; reason/authority wajib dicatat.

---

# 4. Response-target model

## 4.1 Field wajib

| Field | Arti | Nilai baseline |
|---|---|---|
| Acknowledgement target | Batas untuk memastikan laporan diterima dan owner/next action jelas | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |
| Triage target | Batas untuk validasi Incident, severity, scope, owner, dan initial path | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |
| Update target | Cadence/batas update komunikasi selama Incident aktif | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |
| Escalation target | Batas sebelum next tier/authority diaktifkan | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |
| Recovery target | Sasaran pemulihan layanan/outcome; bukan klaim RTO resmi sampai approved | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |
| RCA target | Sasaran root-cause/PIR/problem analysis setelah Incident eligible | `POLICY_CONFIGURABLE` / `TARGET_TO_BE_APPROVED` |

Seluruh angka resmi dan calendar/service-hour semantics:

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
```

## 4.2 Target policy record

```text
Policy ID/version
Severity and incident category
Environment/service window/calendar
Acknowledgement/Triage/Update/Escalation/Recovery/RCA targets
Pause/exception rules
Effective/expiry dates
Authority/owner
Escalation route
Measurement clock and timestamp source
```

## 4.3 Measurement rules

- Simpan actual timestamp; jangan hanya label “on time”.
- Ukur acknowledgement dari reported/detected time yang ditetapkan policy.
- Bedakan restore, resolve, close, and permanent fix.
- Jangan pause clock karena assignment transfer kecuali policy resmi menyatakannya.
- Missing/stale timestamp menghasilkan data quality gap, bukan default success.
- Target breach tidak otomatis berarti technical failure; tetap memicu escalation/review.
- RPO/RTO measured evidence dari tests/operations bukan SLA sampai disetujui.

## 4.4 Severity × target profile

| Severity | Priority posture | Target values | Update audience/route |
|---|---|---|---|
| SEV-1 | Highest approved incident priority | `TARGET_TO_BE_APPROVED` | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| SEV-2 | Expedited approved response | `TARGET_TO_BE_APPROVED` | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| SEV-3 | Standard operational response | `TARGET_TO_BE_APPROVED` | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| SEV-4 | Planned/knowledge/backlog handling | `TARGET_TO_BE_APPROVED` | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |

Tidak ada angka tersembunyi dalam contoh, template, metric, atau runbook.

---

# 5. Problem Management

## 5.1 Kapan Incident menjadi Problem

Incident dibuat/ditautkan ke Problem jika salah satu berlaku:

- berulang atau reopen dengan gejala/cause yang sama;
- root cause belum diperbaiki setelah service restored;
- workaround terus dipakai atau berisiko menjadi permanen;
- berdampak besar walau hanya terjadi sekali;
- melibatkan beberapa component/platform/provider;
- provider defect belum mempunyai permanent fix;
- menunjukkan design, capacity, authorization, Audit, monitoring, release, test, atau control weakness;
- corrective action lintas release memerlukan tracking terpisah.

Incident kecil dengan cause dan fix jelas tidak wajib menjadi Problem. Incident Owner mencatat reason `Problem required: YES/NO` pada closure.

## 5.2 Lifecycle

```text
OPEN
→ INVESTIGATING
→ KNOWN_ERROR
→ FIX_PLANNED
→ FIXED
→ CLOSED
```

Aturan:

- `OPEN`: Problem diterima, owner/scope ditetapkan;
- `INVESTIGATING`: evidence/hypothesis/root cause sedang diuji;
- `KNOWN_ERROR`: cause atau failure mechanism cukup dipahami dan workaround tercatat;
- `FIX_PLANNED`: permanent action, dependency, target version, verification disetujui;
- `FIXED`: permanent fix diterapkan pada scope target dan verification tersedia;
- `CLOSED`: linked incidents, residual risk, KB/runbook/test/docs, and actions reconciled.

Rollback dari state diperbolehkan jika evidence baru membatalkan root cause/fix. History tidak ditimpa.

## 5.3 Problem record

```text
Problem ID; Title; Owner function
Linked Incident/Defect/Known Error/Change IDs
Symptoms and pattern
Affected components/versions/platforms/Feature IDs/environments
Business/security/data impact
Evidence references and timeline summary
Hypotheses; root cause; contributing factors; confidence
Workaround and risk/expiry
Permanent fix strategy/dependencies/target version
Corrective/preventive actions
Status; last reviewed; next review
Verification; residual risk; closure authority
KB/runbook/test/documentation links
```

## 5.4 Lightweight review

- Satu Problem dapat mengelompokkan Incident berulang tanpa menyalin timeline penuh.
- Review dapat asinkron melalui record; rapat hanya bila decision/collaboration benar-benar diperlukan.
- Problem tidak menahan Incident closure setelah recovery dan closure gate terpenuhi, tetapi permanent action tetap tracked.
- Root cause harus didukung evidence; “human error” saja tidak cukup.

---

# 6. Known Error model

## 6.1 Record wajib

| Field | Aturan |
|---|---|
| KnownErrorId | ID unik dokumentasi/operations; bukan Feature ID |
| Symptoms | Observable, search-friendly, tanpa sensitive payload |
| Affected Versions | Exact app/component/provider/config versions |
| Affected Features | Feature ID canonical, tidak dibuat ulang |
| Cause | Validated failure mechanism dengan detail aman |
| Workaround | Safe, authorized, time-bounded; no security bypass |
| Risk | Business/security/data/recovery impact dan residual risk |
| Permanent Fix | Linked action/change/defect or `NOT_YET_DEFINED` |
| Target Version | Approved target atau `TARGET_TO_BE_APPROVED` |
| Status | `DRAFT`, `VALIDATED`, `FIX_PLANNED`, `FIXED`, `OUTDATED`, `CLOSED` sebagai documentation record proposal |
| Last Reviewed | Timestamp + reviewer function |

Status Known Error perlu dikonfirmasi sebelum menjadi enum sistem. Problem lifecycle `KNOWN_ERROR` tetap berbeda dari Known Error article status.

## 6.2 Safety rules

Known Error tidak boleh memuat:

- password, token, secret, key, connection string;
- document/OCR/Assistant/Issue content atau real signature;
- raw exploit, malicious PDF, payload/PoC attack, stack/SQL/body;
- real employee identity, unrestricted IP/hostname, object/storage reference;
- workaround berupa disabling authorization/Audit/quarantine/rate limits/public-AI block;
- direct Production DB/storage/host command.

Detail restricted security investigation disimpan pada approved evidence system dan direferensikan secara opaque.

## 6.3 Workaround gate

Workaround harus menyatakan scope, version, environment, permission, prerequisites, expected outcome, forbidden outcome, risk, expiry/review, rollback, and escalation. Workaround bukan permanent fix dan tidak boleh menurunkan acceptance/security requirement.

---

# 7. Post-Incident Review

## 7.1 Kapan PIR diperlukan

PIR direkomendasikan untuk SEV-1/SEV-2, security/data incident, Production outage, failed recovery/restore, repeated incident, broad Core impact, mandatory Audit failure, bad release/rollback, significant provider incident, atau management request. Exact trigger:

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
```

Incident kecil dapat memakai compact closure note bila tidak memenuhi trigger.

## 7.2 Template minimum

```text
PIR ID; Incident/Problem IDs; date/version
Apa yang terjadi
Dampak dan scope
Timeline
Detection
Response
Containment
Recovery dan verification
Root cause dan confidence
Contributing factors
Apa yang berjalan baik
Apa yang gagal/terlambat/membingungkan
Corrective actions
Preventive actions
Action owner/dependency/target/status/verification
Residual risk/acceptance reference
Communication/evidence/Audit references
Requirement/design/test/monitoring/KB/runbook/manual updates
Review functions dan approval/reference
```

## 7.3 Blameless standard

- Fokus pada system, process, context, control, tooling, documentation, workload, and decision conditions.
- Hindari atribusi “kelalaian individu” tanpa evidence dan formal HR/legal process.
- Nama orang diminimalkan; gunakan function/principal reference sesuai need-to-know.
- Catat keputusan yang masuk akal berdasarkan informasi saat itu, lalu identifikasi guard yang kurang.
- PIR bukan tempat menyalin raw evidence atau sensitive content.

## 7.4 Timeline quality

Gunakan UTC/source timezone yang jelas, bedakan occurred/received/detected/reported/acknowledged/contained/restored/resolved/closed, dan tandai estimated time. Jangan mengubah event lama untuk merapikan narasi; correction ditambahkan dengan reason/source.

---

# 8. Corrective-action tracking

## 8.1 Jenis action

- code fix;
- regression/security/recovery test;
- monitoring/alert improvement;
- permission/scope/SoD fix;
- typed configuration/policy fix;
- capacity/backpressure change;
- documentation/KB/runbook update;
- training/knowledge sharing;
- provider change proposal, bukan kontrak/selection otomatis;
- security control improvement;
- deployment/restore process improvement;
- architecture/requirement change request bila gap material.

## 8.2 Record

| Field | Aturan |
|---|---|
| Action ID | Stabil dan unik dalam PIR/Problem system |
| Owner | Function + assigned principal reference |
| Priority | Berdasarkan risk/impact; tidak disamakan otomatis dengan severity |
| Dependency | Feature/contract/provider/approval/change/test/deployment |
| Target | Version/date/status; `TARGET_TO_BE_APPROVED` bila belum sah |
| Status | `OPEN`, `IN_PROGRESS`, `BLOCKED`, `READY_FOR_VERIFICATION`, `VERIFIED`, `CLOSED` sebagai baseline workflow |
| Verification | Test/evidence/build/environment/reviewer/result |

## 8.3 Quality rules

- Satu action menghasilkan satu observable outcome.
- “Monitor” atau “lebih hati-hati” bukan action tanpa owner/verification.
- Code fix selalu memiliki affected tests dan controlled deployment handoff.
- Security/control action tidak closed hanya karena config changed; negative regression/evidence wajib.
- Documentation action menyebut exact manual/KB/runbook/screenshot/link/test.
- Cancel/defer memerlukan reason, risk, authority, review/expiry, and compensating control.
- Incident dapat CLOSED dengan action terbuka hanya jika recovery verified, risk governed, owner/target clear, dan closure authority menyetujui; action tetap visible.

## 8.4 Solo-developer board

Untuk model satu developer, satu board/list dapat menyimpan Incident, Defect, Problem, dan Action sebagai typed record/links. Jangan membuat formulir duplikat. Field wajib dan state separation tetap dipertahankan.

---

# 9. Metrics

## 9.1 Metric catalog

| Metric | Definisi minimum | Dimensi wajib | Target |
|---|---|---|---|
| Open incidents | Incident belum CLOSED pada snapshot | Severity, age band, environment, owner function | `TARGET_TO_BE_APPROVED` |
| Incidents by severity | Count per approved severity | Period, environment, service/component | `TARGET_TO_BE_APPROVED` |
| Time to acknowledge | AcknowledgedAt − approved start clock | Severity, channel, service window policy | `TARGET_TO_BE_APPROVED` |
| Time to restore | RestoredAt − approved start clock | Severity, component, environment, release | `TARGET_TO_BE_APPROVED` |
| Reopen rate | Reopened / closed eligible incidents | Period, component, cause category | `TARGET_TO_BE_APPROVED` |
| Repeat rate | Incidents linked to existing Problem/Known Error / eligible incidents | Period, component/provider/Feature | `TARGET_TO_BE_APPROVED` |
| Core Feature impact | Incidents affecting one or more 33 Core Tools | Feature ID, platform, processing mode, release | `TARGET_TO_BE_APPROVED` |
| Security incident count | Incidents classified security per approved policy | Severity, control/category, environment | `TARGET_TO_BE_APPROVED` |
| Incidents by component | Count using canonical application/component map | Component, environment, release | `TARGET_TO_BE_APPROVED` |
| Incidents after release | Incidents within approved post-release observation window | Build/digest, deployment/change ID | `TARGET_TO_BE_APPROVED` |
| Workaround duration | Workaround start → retired/permanent fix | Problem/Known Error, risk, owner | `TARGET_TO_BE_APPROVED` |
| Overdue corrective actions | Open actions past approved target | Priority, owner function, dependency | `TARGET_TO_BE_APPROVED` |
| Issue-to-incident conversion | Issues converted to Incident / eligible scoped Issues | Channel/category/build | `TARGET_TO_BE_APPROVED` |

## 9.2 Additional quality metrics

- target breach/escalation count;
- Incident without owner or evidence reference;
- severity changes with/without reason;
- closure gate failure/reopen reason;
- Problems without current Known Error/workaround/fix plan;
- KB/runbook usage and “resolved without escalation” count, without incentivizing unsafe under-escalation;
- tabletop gaps/actions and completion state;
- incidents with documentation/test/monitoring update;
- evidence/data-quality defects.

## 9.3 Guard against bad metrics

- No leaderboard or pressure to downgrade severity/avoid conversion.
- Always include period, environment, build/release, category, population, and data completeness.
- Median/percentile/average method requires approved analytics definition; do not compare unlike windows.
- Low incident count is not proof of reliability if detection/reporting is weak.
- Restore time is not permanent-fix time.
- Issue count and Incident count are not interchangeable.
- Metric dashboards contain safe aggregates only, no Issue/document/security payload.

---

# 10. Tabletop/drill strategy

## 10.1 Safety and format

Tabletop validates decisions, communication, evidence, and documentation. Controlled drill may exercise safe non-Production recovery behavior. Neither performs attack/fault/load against Production.

```text
Scenario
Objectives and scope
Participants/functions
Assumptions/injects
Expected decisions
Forbidden actions
Evidence
Observed gaps
Corrective actions
Owner/target/verification
```

Participants, schedule, environment, stop conditions, and approval:

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION
```

## 10.2 Ten required exercises

| Scenario | Participants/functions | Expected decisions | Evidence | Gaps to capture | Corrective actions |
|---|---|---|---|---|---|
| 1. API outage | Support, Incident Owner, Technical, Administrator, Business, Operations Approver | Declare scope/severity; preserve local-capable path; online actions wait; no false success; escalate infra; deployment handoff if change needed | Health freshness, safe request/error/Correlation refs, affected Features, decision timeline | Detection, dependency map, user message, escalation/contact, local fallback clarity | Monitoring, error guidance, capacity/fix, KB/runbook, approved deploy task |
| 2. Worker failure | Support, Incident Owner, Technical, Administrator | Stop dependent admission if unsafe; validate lease/state; one active attempt; reject stale completion; source/result safe | Job/lease/state/version/Correlation refs, worker health, `REC-SYS-005/006` evidence | Missing safe cancel/reclaim visibility, checkpoint ambiguity | Worker fix, recovery regression, monitoring/runbook |
| 3. Queue overload | Support, Incident Owner, Technical, Administrator, Business | Apply bounded backpressure/admission/concurrency policy; no row deletion/mass retry; local route remains | Queue depth/age/drain/freshness, capacity/reason, affected Features | Threshold/owner/target not approved, retry storm risk | Capacity policy, load/stress test, alert/runbook |
| 4. Storage unavailable | Support, Technical, Administrator, Security as applicable | No final-result claim; protect source/old target; stop unsafe admission; reconcile orphan/metadata after recovery | Storage health, object/metadata state refs, digest/status, `REC-SYS-009..011` evidence | Restore/reconcile authority, cleanup safety, quota visibility | Storage fix, restore/reconcile test, monitoring/KB |
| 5. SSO outage | Support, Incident Owner, Administrator, Security, Business | No auth bypass/shared credential; valid local offline capability may continue; privileged/server action denied/waits; identity escalation | Auth health/safe error/time/build, offline policy reference | Official identity contact, comms, offline expiry/reauth behavior | Auth runbook, UX guidance, security test, organization mapping |
| 6. Cross-user exposure | Security, Incident Owner, Technical, Administrator, Auditor, Business/Management | Security escalation; least-destructive containment; revoke/limit access; preserve safe evidence; do not browse content; notification decision by authority | Authorization/Audit/access/Job/Correlation refs, actor/source/time/integrity manifest | Scope confidence, evidence handling, legal/privacy triggers, no content access path | AuthZ fix, permanent negative test, monitoring, PIR, policy/runbook |
| 7. Malicious PDF | Security, Technical, Incident Owner, Support | Isolate/quarantine exact artifact via approved control; no open/execute; preserve safe digest/reference; identify affected engine/build; no payload in record | File validation/security reason, opaque artifact ref, engine/SBOM/build, `Security.FileRejected/MalwareDetected` ref if applicable | Malicious corpus control, engine contact, evidence retention | Engine/dependency fix, malicious corpus regression, KB restricted guidance |
| 8. Audit pipeline failure | Incident Owner, Technical, Administrator, Security, Auditor | Protect durable intent; fail closed for privileged actions as designed; no disable/drop; inspect outbox/backlog/version; restore append path | Audit health, queue age/count, safe Event IDs/types, `REC-SYS-017..020` evidence | Degraded L2 policy, alert route, queue capacity, hold/restore authority | Audit fix, retry/recovery tests, monitoring/runbook/policy confirmation |
| 9. AI provider failure | Support, Technical, Security, Business | No public fallback; use approved local route only if valid; circuit/backpressure/maintenance; disclose limitation; protect selected content | Provider profile code/status, affected `FEAT-INTEL-*`, safe outcome/reason, no-transfer evidence | Provider contact/contract, classification, fallback availability | Provider adapter fix/proposal, test, KB, capability/maintenance policy |
| 10. Bad release/rollback need | Incident Owner, Technical, Security, Business, Deployment/Operations Approver | Stop/limit rollout; compare artifact/config/migration; choose approved rollback or forward fix; preserve evidence; verify after deploy | Build/commit/digest/SBOM/config/schema/change IDs, smoke/regression/recovery results | Approval route, rollback compatibility, migration reversibility, communication | Release gate fix, regression, deployment runbook update, PIR |

## 10.3 Exercise evidence

Evidence may include sanitized decision log, timeline, role/function roster, safe simulated alert/Correlation/Job IDs, checklist result, test result, screenshot with synthetic data, gaps/actions, and approval reference. Do not use real credentials, real documents, Production attacks, real employee identities, unrestricted topology, or sensitive exploit details.

## 10.4 Pass criteria

A tabletop/drill passes only if participants can identify owner, severity/escalation path, safe containment, forbidden actions, evidence boundary, recovery verification, deployment handoff, communication decision, closure/PIR/Problem trigger, and corrective actions without relying on undocumented verbal knowledge. Missing official route/target is a gap, not fabricated PASS.

---

# 11. Knowledge Base

## 11.1 Article structure

| Field | Aturan |
|---|---|
| KB ID | Stabil; bukan Feature/Incident ID |
| Title | Search-friendly symptom + component/outcome |
| Symptoms | Observable, localized terms dan safe error code |
| Affected Component | Canonical component/application area |
| Affected Version | Exact app/component/provider/config version |
| Safe Checks | Read-only/authorized checks; no raw content/secret |
| Workaround | Approved, scoped, risk/expiry noted |
| Resolution | Permanent/verified path or approved cross-reference |
| Escalation | Trigger + function/route reference; no invented name |
| Owner | Owner function + confirmed principal/group reference |
| Review Date | Last/next review according to policy |
| Status | `DRAFT`, `REVIEWED`, `APPROVED`, `OUTDATED`, `RETIRED` |

## 11.2 Article design

- One symptom/task per article.
- Title uses words users/support search: feature name, error code, “menunggu”, “tidak tersedia”, “permission denied”.
- Put safe checks before workaround.
- State affected versions, environment, and exceptions.
- Link canonical Feature IDs, Incident/Problem/Known Error, monitoring, and runbook references.
- Keep procedures short; link Document 17 for deployment change.
- Add forbidden actions and escalation threshold.
- Review after every material incident/fix/release.
- English translation is a separate set; identifiers remain unchanged.

## 11.3 Suggested taxonomy

```text
Authentication & Session
Authorization & Account
Core PDF Tools
API & Realtime
Worker & Queue
Storage & Secured Files
Audit Pipeline
Issue/Support
AI/Internal Provider
Release & Compatibility
Monitoring & Safe Errors
Recovery & Restore
```

## 11.4 Security boundary

Public/general KB contains only safe checks and user/support guidance. Restricted security/forensic details use approved access-controlled repository. Do not copy incident evidence into KB. Exploit details, malicious file, raw stack/SQL, provider endpoint, connection data, secrets, and document content are prohibited.

## 11.5 KB QA

Each article is tested by a reviewer who did not write it. Test must confirm searchability, version applicability, permission, safe checks, workaround outcome, forbidden action, escalation, link validity, and no sensitive content. An article with failed workaround becomes OUTDATED until corrected/revalidated.

---

# 12. Runbook Register

## 12.1 Field dan status

| Field | Aturan |
|---|---|
| Runbook ID | Unique documentation ID; not Feature ID |
| Incident Type | API/Worker/Queue/Storage/SSO/Security/File/Audit/Provider/Release etc. |
| Component | Canonical component/deployable/module |
| Severity Applicability | Approved severity range or `TARGET_TO_BE_APPROVED` |
| Owner Function | Conceptual function until organization mapping confirmed |
| Last Review | Timestamp/revision |
| Tested? | `NO`, `TABLETOP`, `CONTROLLED_DRILL`; no false Production test |
| Related Features | Canonical Feature IDs/family |
| Related Monitoring | Safe alerts/health/metrics references |
| Status | Lifecycle below |

```text
DRAFT
→ REVIEWED
→ TABLETOP_VALIDATED
→ APPROVED

OUTDATED → REVIEW/REPLACE
RETIRED
```

## 12.2 Initial register

All entries remain `DRAFT`; this report does not create executable runbooks.

| Runbook ID | Incident Type | Component | Severity Applicability | Owner Function | Last Review | Tested? | Related Features | Related Monitoring | Status |
|---|---|---|---|---|---|---|---|---|---|
| `RB-API-OUTAGE-001` | API outage | ASP.NET Core API | `TARGET_TO_BE_APPROVED` | Technical/Operations | Not reviewed | NO | Server-dependent canonical Features | API availability/error/latency | DRAFT |
| `RB-WORKER-FAIL-001` | Worker failure | Local/Server Worker | `TARGET_TO_BE_APPROVED` | Technical Investigator | Not reviewed | NO | `FEAT-PROG-*`, affected processing Features | Worker heartbeat/job lease/duration | DRAFT |
| `RB-QUEUE-OVERLOAD-001` | Queue overload | Jobs/Queue | `TARGET_TO_BE_APPROVED` | Technical/Administrator | Not reviewed | NO | Server/background Features | Depth/age/drain/retry/resource | DRAFT |
| `RB-STORAGE-UNAVAILABLE-001` | Storage unavailable | SQL/Object/Local storage as scoped | `TARGET_TO_BE_APPROVED` | Operations/Technical | Not reviewed | NO | `FEAT-BACKUP-*`, `FEAT-VER-*`, processing | Readiness/capacity/transfer/reconcile | DRAFT |
| `RB-SSO-OUTAGE-001` | SSO outage | Pertamina Identity integration | `TARGET_TO_BE_APPROVED` | Security/Operations | Not reviewed | NO | `FEAT-AUTH-*` | Auth availability/safe error | DRAFT |
| `RB-CROSS-USER-001` | Cross-user exposure | Authorization/resource boundary | `TARGET_TO_BE_APPROVED` | Security Reviewer | Not reviewed | NO | All protected resources | AuthZ deny/access/Audit/security alerts | DRAFT |
| `RB-MALICIOUS-PDF-001` | Malicious PDF | Validation/engine/Worker | `TARGET_TO_BE_APPROVED` | Security/Technical | Not reviewed | NO | PDF-consuming Feature families | File rejection/malware/engine health | DRAFT |
| `RB-AUDIT-PIPELINE-001` | Audit pipeline failure | Audit outbox/ingestion/storage | `TARGET_TO_BE_APPROVED` | Audit Technical/Security | Not reviewed | NO | `FEAT-AUDIT-*` | Outbox/backlog/ingestion/append/hold health | DRAFT |
| `RB-AI-PROVIDER-001` | AI provider failure | Approved intelligence provider | `TARGET_TO_BE_APPROVED` | Technical/Security | Not reviewed | NO | `FEAT-INTEL-001..002` | Provider/circuit/timeout/policy | DRAFT |
| `RB-BAD-RELEASE-001` | Bad release/rollback need | Release/config/schema | `TARGET_TO_BE_APPROVED` | Deployment/Operations Approver | Not reviewed | NO | Affected release Feature set | Version/health/error/deployment signals | DRAFT |

## 12.3 Runbook QA

Runbook is not APPROVED until role/scope, prerequisites, environment, safe checks, containment, forbidden actions, decision points, evidence, communication, rollback/forward-fix handoff, recovery verification, stop conditions, and closure references are reviewed and tested. Commands belong to Document 17 or controlled technical runbook, not this conceptual register.

---

# 13. Closure quality gate

## 13.1 Resolved vs Closed

- `RESOLVED`: service/business outcome restored or permanent safe outcome reached and verified.
- `CLOSED`: resolution plus evidence, communication, linked records, actions, Audit, documentation, and security review requirements complete.

## 13.2 Mandatory checklist

```text
[ ] recovery verified against authoritative state and business outcome
[ ] monitoring stable/fresh for approved observation condition
[ ] communication complete and recorded
[ ] linked Defect/Problem/Known Error/change records correct
[ ] evidence manifest complete, access-controlled, and retention/hold assigned
[ ] required Audit evidence complete or explicit approved exception/gap escalated
[ ] workaround state clear: active/retired/expiry/risk/owner
[ ] corrective/preventive action owners and targets assigned
[ ] KB/runbook/manual/test/monitoring updates completed or tracked
[ ] security review complete when security/data/control scope applies
[ ] no unresolved scope/ownership/restore/integrity ambiguity
[ ] closure reason, authority, and timestamp recorded
```

## 13.3 Forbidden closure

Do not close because alert disappeared, process restarted, one retry succeeded, user stopped reporting, dashboard is green but telemetry stale, workaround exists without risk/expiry, deploy completed without verification, or evidence/Audit is missing. Security/data incident cannot close without applicable Security Reviewer outcome.

## 13.4 Reopen

Reopen when symptoms recur within related scope, recovery validation fails, monitoring regresses, impact expands, linked fix is ineffective, new security/data evidence appears, or closure evidence was invalid. Preserve original closed/resolved timestamps and append reopen reason/timeline.

---

# 14. Deployment handoff

## 14.1 Boundary statement

If Incident requires rollback, hotfix deployment, database migration, infrastructure change, backup restore, or configuration rollout, Document 16 states exactly:

> **Approved Deployment Procedure Required.**

Execution details belong to `17_DEPLOYMENT_GUIDE.md` and approved environment-specific procedures.

## 14.2 Handoff package

```text
Incident/Problem/Defect/Action IDs
Reason, impact, severity, environment, scope
Requested change type: rollback/hotfix/migration/infra/restore/config rollout
Approved change/deployment reference
Source branch/commit/build/pipeline/artifact digest/SBOM
Configuration/schema/capability/provider versions
Test/security/recovery evidence and known risks
Backup/recovery point and compatibility evidence when relevant
Rollback/forward-fix decision criteria
Required approvers and segregation of duties
Verification/smoke/monitoring/communication plan
Evidence and Audit references
```

## 14.3 Gate

- No direct Production patch, SQL, object-storage edit, secret/config change, or manual binary replacement.
- Emergency process may be accelerated, not untracked.
- Artifact is immutable and built from controlled source; do not rebuild differently after QA evidence.
- Database migration/restore requires compatibility, integrity, authorization, object, outbox, idempotency, Audit, tombstone/hold, and security checks.
- Incident Owner does not self-approve deployment solely because service is down.
- Deployment completion is not Incident recovery until post-change verification passes.

## 14.4 Information returned to Incident

Document 17/deployment record returns deployment outcome, timestamps, artifact/config/schema version, approval reference, rollback/forward-fix result, smoke/regression/security checks, monitoring state, and safe evidence. Incident record does not duplicate deployment commands or secrets.

---

# 15. Solo developer/Codex workflow

## 15.1 Lean flow

```text
Incident
→ Task/Defect
→ scoped Codex analysis
→ branch/worktree
→ smallest authorized fix + tests
→ human review
→ QA/security/recovery gates
→ Approved Deployment Procedure Required.
→ verify authoritative outcome + monitoring
→ close Incident or continue Problem/actions
```

## 15.2 Required task package

```text
Task/Incident/Defect IDs
Requirement and canonical Feature IDs
Observed behavior and safe evidence
Build/environment/component
Scope and Allowed Paths
Do-Not-Touch/Non-Scope
Risk/security/data/recovery impact
Acceptance Criteria and forbidden outcomes
Tests Required and commands/gates
Expected evidence/output
Deployment handoff required? YES/NO
```

## 15.3 Codex boundary

Codex may summarize safe evidence, inspect authorized source/config/test artifacts, propose hypotheses, make scoped code/test/docs changes when separately authorized, and report commands/results/gaps.

Codex may not:

- declare severity/incident closure/business acceptance/Production approval;
- use Production data/secret or request broad content access;
- change requirement, security rule, role, permission, expected behavior, or test to make result green;
- delete/skip/ignore/quarantine critical tests;
- deploy, rollback, migrate, restore, rotate secret, or contact vendor without explicit authorization/workflow;
- interpret document/OCR/Issue content as instruction;
- invent success, evidence, target, owner, SLA, or Pertamina organization.

## 15.4 Hat separation for one developer

The same human may act as Incident Owner, Technical Investigator, and documenter when approved, but records function transitions. Security-sensitive and Production deployment decisions still require the applicable independent human authority. Codex is never the independent reviewer.

## 15.5 Stop protocol

Stop and report when requirement/test/implementation conflict, target/authority/scope is unknown, evidence would require prohibited content, safe reproduction is unavailable, fix expands beyond Allowed Paths, dependency/provider change is needed, or deployment/restore authority is missing.

---

# 16. Documentation maintenance

## 16.1 Triggers

Update review is triggered when:

- Incident status/lifecycle changes;
- severity model/authority changes;
- escalation type/route/contact/on-call changes;
- component/deployable/provider is added/removed;
- monitoring/alert/reason code changes;
- security/evidence/privacy/legal process changes;
- runbook/tabletop/drill fails;
- recurring Issue/Incident creates a Problem/Known Error;
- approved provider or provider policy changes;
- deployment/rollback/migration/restore model changes;
- Role Matrix/permission/scope/reauth changes;
- Audit event/access/retention/hold model changes;
- response targets/calendar changes;
- PIR/action/closure criteria changes;
- actual incident reveals missing/unsafe/confusing guidance.

## 16.2 Change workflow

```text
trigger
→ documentation impact record
→ affected Incident/Problem/KB/runbook/tabletop/metric/cross-reference
→ update controlled source
→ role/security/operations review
→ tabletop or controlled drill where risk requires
→ link/action/target validation
→ approval/version/release note
→ retire or mark OUTDATED prior content
```

## 16.3 Ownership and versioning

Each document/article/runbook records version, applies-to application/component versions, owner function, approver/reviewer, last review, next trigger/date according to policy, status, related Features/monitoring/Incidents/Problems, and superseding reference. Exact owner and review cadence:

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION
```

## 16.4 Documentation QA

- Search/link test;
- role/permission/security boundary review;
- safe evidence/privacy scan;
- tabletop decision test;
- runbook dry-read by a function not authoring it;
- exact version/environment applicability check;
- rollback/deployment cross-reference check;
- outdated/duplicate/conflicting guidance check;
- UAT/Test Plan/incident lessons traceability.

---

# 17. Pertamina confirmation gaps

## 17.1 Organization

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION

- names/groups for Reporter intake, L1/L2/L3, Incident Owner, Technical Investigator
- Administrator/Security/Product/Auditor/Deployment approver mapping
- on-call/service hours/roster and ownership transfer
- management escalation and communication audiences
- Service Desk/KB/runbook owners and publication platform
- vendor/provider management function and approved contact route
- PIR/Problem/closure authority and independent reviewer requirements
```

## 17.2 Standards/policy

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION

- incident severity definitions and authority
- acknowledgement/triage/update/escalation/recovery/RCA targets and calendars
- security/privacy/legal/regulatory notification triggers and authority
- evidence classification, chain of custody, retention, legal hold, disposal
- forensic tooling and restricted evidence repository
- containment, account/session revocation, key/secret rotation, destructive authority
- RPO/RTO, backup/restore/DR targets and exercise cadence
- monitoring/alert threshold, SOC/SIEM integration, health ownership
- risk acceptance, workaround expiry, corrective-action priority/target
- Production emergency change/hotfix/rollback/restore approval
- vendor/provider support contract, severity, evidence, and data-sharing boundaries
- tabletop/drill cadence, scope, environment, participants, and pass authority
- metric definitions, reporting audience, retention, and target approval
```

## 17.3 Product/implementation gaps

- No canonical Incident/Problem/Known Error API/entity/UI/Event Catalog is established by Documents 01–15; implementation requires controlled design/addenda and must not be invented in this report.
- Issue-to-Incident conversion fields/status/API are not finalized.
- Incident-specific Audit event names are not in Document 10; use existing safe Support/Security/Recovery/Audit evidence references until a controlled catalog decision exists.
- Exact component/application cartography is owned by future Document 18.
- Exact Deployment procedure is owned by Document 17.
- UAT execution and release candidate are not yet recorded.

These gaps do not block a pre-implementation documentation baseline, but they block operational `APPROVED`/Production-readiness claims.

---

# 18. Risks/conflicts

## 18.1 Risk register

| ID | Risk/conflict | Dampak | Control/handoff |
|---|---|---|---|
| `D16-D-001` | Incident severity not authoritative | Misclassification, wrong escalation/communication | Keep working model `TARGET_TO_BE_APPROVED`; confirm Pertamina policy |
| `D16-D-002` | Incident SEV, defect severity, and vulnerability severity can be conflated | False priority/closure/release decision | Separate fields and lifecycle; no automatic mapping |
| `D16-D-003` | Issue statuses Document 15 can be reused as Incident statuses | Broken traceability/user-facing confusion | Separate Issue/Incident records and linked IDs |
| `D16-D-004` | No Incident/Problem/KB/Runbook system contract | Teams may invent fields/API/database writes | Controlled design/addendum before implementation; documentation record only |
| `D16-D-005` | Existing Audit catalog lacks incident-specific event names | Free-form/duplicate/unsafe Audit payload | Do not invent EventType; link existing Support/Security/Recovery evidence; Integrator decision |
| `D16-D-006` | Response targets/on-call/calendar absent | SLA invention or inconsistent handling | All target fields policy-configurable/TBC; organization confirmation |
| `D16-D-007` | Support/Admin/Investigator mistaken for content access | Privacy/authorization breach | Role Matrix enforcement; exact Issue/resource scope; no automatic content access |
| `D16-D-008` | Evidence collection copies secret/content/exploit | Secondary disclosure | Safe manifest/opaque references; restricted evidence policy; security review |
| `D16-D-009` | Containment becomes destructive or untracked | Data loss, outage expansion, evidence loss | Least-destructive/recoverable control; approval; deployment handoff |
| `D16-D-010` | Service recovery mistaken for permanent fix/closure | Repeat incident, workaround becomes permanent | Separate restored/resolved/closed; Problem/action/KB tracking |
| `D16-D-011` | Solo developer self-approves security/Production change | SoD and quality failure | Function-hat record + required independent human approval; Codex not approver |
| `D16-D-012` | Runbook used without validation/version match | Unsafe/wrong action | Register lifecycle, exact applies-to version, tabletop/drill, OUTDATED block |
| `D16-D-013` | Tabletop becomes Production attack/fault test | Service/security impact | Tabletop or controlled non-Production drill only; explicit stop condition |
| `D16-D-014` | Vendor escalation shares restricted data or creates public fallback | Confidentiality/provider violation | Approved provider route/data-minimization; no contract invention/public AI |
| `D16-D-015` | Deployment details leak into Document 16 | Conflicting/uncontrolled operations | Exact boundary: “Approved Deployment Procedure Required.” → Document 17 |
| `D16-D-016` | Metric incentives cause severity downgrade/under-reporting | False green | Data-quality guard, no leaderboard, independent review |
| `D16-D-017` | UAT and RC not executed | Runbooks/recovery behavior unproven | Maintain pre-implementation status; validate on approved environment |

## 18.2 Source consistency audit

- Document 01 expects General Incident Guide, Post Go-Live Support, L1/L2/L3 escalation, Service Desk knowledge, error code, and business/technical ownership; organization mapping remains unspecified.
- Document 08 defines the security incident lifecycle and evidence principles but deliberately defers severity, contacts, chain of custody, exercises, retention/hold, and destructive authority to Document 16/Pertamina confirmation.
- Document 09 prevents incident functions from gaining implicit cross-user content or workload authority.
- Document 10 keeps Audit separate and forbids content/secret; an Incident ID is not yet a canonical Audit event/resource in the locked catalog.
- Document 12 provides non-Production recovery/fault-injection cases `REC-SYS-001..020`, defect severity, evidence, and measured-target rules; those are test inputs, not official incident SLA.
- Document 13 sends support/incident/recovery findings to Document 16 and does not claim execution UAT.
- Document 15 provides admin safe checks/escalation triggers but explicitly leaves severity, contact, containment, communication, and recovery details to Document 16.
- `NFR-HAND-001` requires source, build, deployment, manuals, test/UAT evidence, incident guide, cartography, knowledge sharing, and side-by-side maintenance; Document 16 supplies incident/KB/runbook readiness but not Deployment Guide content.

No organization name, SLA value, vendor contract, Feature ID, role, permission, endpoint, database table, or Incident-specific Audit EventType is introduced.

---

# 19. Suggested Document 16 text

Teks berikut dapat diintegrasikan ke master Document 16 setelah review Integrator. Organization, targets, contacts, severity authority, and operational procedures remain confirmation/evidence-gated.

## 19.1 Status and purpose

> **Status:** `FINAL — PRE-IMPLEMENTATION INCIDENT SUPPORT GUIDE BASELINE`  
> **Operational readiness:** `BELUM OPERATIONS_READY`  
> **Applies to application:** `RC-TBD`

Guide ini menetapkan incident ownership, escalation, response-target fields, Problem Management, PIR, Knowledge Base, runbook governance, tabletop, closure, dan deployment handoff. Guide tidak memberikan permission, tidak membuka document content, tidak menetapkan SLA resmi, dan tidak menggantikan Deployment Guide.

## 19.2 Responsibility

Setiap Incident mempunyai satu Incident Owner. Reporter, Support/Triage, Technical Investigator, Administrator, Security Reviewer, Product/Business Owner, Auditor, dan Deployment/Operations Approver mempunyai fungsi terpisah. Satu orang dapat merangkap fungsi pada model solo developer jika hat/decision dicatat, tetapi security-sensitive dan Production deployment approval tetap memerlukan human authority yang berlaku. Codex bukan owner atau approver.

Organization mapping:

```text
REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION
```

## 19.3 Lifecycle and record

Lifecycle konseptual: `REPORTED → ACKNOWLEDGED → TRIAGED → RESPONDING → MONITORING → RESOLVED → CLOSED`. Issue, Defect, Problem, Known Error, Audit, and Observability remain separate linked records. Incident record stores safe metadata, timeline, decisions, impact, owner, evidence references, containment/workaround/recovery/verification, linked changes/actions, communication, and closure—not sensitive payload.

## 19.4 Escalation

Escalation types are FUNCTIONAL, TECHNICAL, SECURITY, INFRASTRUCTURE, BUSINESS, VENDOR/PROVIDER, and MANAGEMENT. Trigger on SEV-1/SEV-2 proposal, data exposure, repeated incident, no workaround, Core outage, Production unavailable, failed recovery, expanding scope, response target risk/breach, provider dependency, security control failure, mandatory Audit failure, or data loss/corruption.

Escalation requests expertise/authority; it does not expand data access. Incident Owner remains accountable until accepted transfer.

## 19.5 Severity and targets

`SEV-1..SEV-4` is a working incident-impact model only. Defect and vulnerability severity remain separate. Acknowledgement Target, Triage Target, Update Frequency, Escalation Target, Recovery Target, and RCA Target are `POLICY_CONFIGURABLE`, `TARGET_TO_BE_APPROVED`, and `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. Record actual timestamps and target-policy version; do not fabricate numbers.

## 19.6 Problem and Known Error

Create/link a Problem when an Incident repeats, root cause remains unfixed, workaround persists, impact is large, multiple components are involved, provider defect exists, or design/control weakness appears. Lifecycle: `OPEN → INVESTIGATING → KNOWN_ERROR → FIX_PLANNED → FIXED → CLOSED`.

Known Error records include KnownErrorId, Symptoms, Affected Versions/Features, Cause, Workaround, Risk, Permanent Fix, Target Version, Status, and Last Reviewed. Do not store secrets, user documents, raw exploit, or sensitive security detail.

## 19.7 PIR and corrective actions

PIR is blameless and records event, impact, timeline, detection, response, containment, recovery, root cause, contributing factors, what worked/failed, corrective/preventive actions, owner, dependency, target, status, verification, and document/test/runbook updates. Every action has Action ID, Owner, Priority, Dependency, Target, Status, and Verification.

## 19.8 Metrics

Measure open incidents, severity distribution, time to acknowledge/restore, reopen/repeat rate, Core impact, security incident count, component/release distribution, workaround duration, overdue actions, and Issue-to-Incident conversion. All targets require approval. Metrics include environment/build/window/data completeness and must not encourage under-reporting or severity downgrade.

## 19.9 Tabletop and drills

Maintain exercises for API outage, Worker failure, queue overload, storage unavailable, SSO outage, cross-user exposure, malicious PDF, Audit pipeline failure, AI provider failure, and bad release/rollback. Each records Scenario, Participants/Functions, Expected Decisions, Evidence, Gaps, and Corrective Actions. Use tabletop or controlled non-Production drill; never attack/fault/load Production.

## 19.10 Knowledge and runbooks

KB articles contain KB ID, Title, Symptoms, Affected Component/Version, Safe Checks, Workaround, Resolution, Escalation, Owner, Review Date, and Status. Articles are concise, searchable, versioned, reviewed, and free of secret/content/exploit details.

Runbook register contains Runbook ID, Incident Type, Component, Severity Applicability, Owner Function, Last Review, Tested?, Related Features, Related Monitoring, and Status. Statuses: `DRAFT`, `REVIEWED`, `TABLETOP_VALIDATED`, `APPROVED`, `OUTDATED`, `RETIRED`.

## 19.11 Closure

Before CLOSED: recovery verified, monitoring stable/fresh, communication complete, defect/problem linked, evidence and required Audit complete, workaround state clear, action owner assigned, documentation updated/tracked, and security review complete when applicable. Alert disappearance, restart, one retry, stale green dashboard, or deployment completion alone is insufficient.

## 19.12 Deployment boundary

For rollback, hotfix deployment, database migration, infrastructure change, backup restore, or configuration rollout:

> **Approved Deployment Procedure Required.**

See `17_DEPLOYMENT_GUIDE.md`. Incident Guide passes reason/scope, approved change, artifact/config/schema versions, evidence, risk, rollback/forward-fix criteria, approvers, and verification plan—never commands or secrets.

## 19.13 Solo developer and Codex

Use the lean flow Incident → Task/Defect → scoped Codex analysis → branch/worktree → fix + tests → human review → QA/security/recovery gates → approved deployment → verify → close or continue Problem/actions. Codex may support scoped analysis and changes but may not approve, deploy, change requirements/tests, access Production secrets/content, or invent evidence/targets/organizations.

## 19.14 Maintenance and readiness

Incident/severity/escalation/component/monitoring/security/runbook/recurring issue/provider/deployment/Role Matrix/Audit changes trigger controlled documentation review and appropriate tabletop/drill. Document 16 becomes operations-ready only after organization/target/policy confirmation, executable runbooks reviewed and tested, contacts/escalation valid, RC/recovery evidence available, KB/metrics/closure workflow established, and no critical readiness gap remains.

---

**Serah terima kepada Integrator:** confirm organization mapping, severity/target policy, notification/chain-of-custody/evidence/retention rules, on-call/escalation/provider routes, containment/destructive authority, RPO/RTO, emergency deployment approval, KB/runbook ownership, and tabletop cadence. Resolve Incident/Problem/Known Error data/API/Audit Event Catalog only through controlled downstream/addendum decisions; do not create them silently from this documentation report. Keep Document 17 as the sole owner of deployment execution details.
