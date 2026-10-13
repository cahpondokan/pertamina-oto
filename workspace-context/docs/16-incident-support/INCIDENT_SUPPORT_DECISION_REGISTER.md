# REGISTER KEPUTUSAN INCIDENT & SUPPORT
## PERTAMINA OTO — Office Tools Organizer

> **Dokumen:** 16 — Incident Support Guide  
> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — 45/45 AUTO-LOCKED  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards; Dokumen 01–15 FINAL

---

# 1. Aturan

Keputusan berikut menormalkan sumber berwenang dan empat laporan specialist. Register ini tidak membuat organisasi, SLA, role, permission, Feature ID, API, tabel, Audit EventType, vendor, akses isi, atau tindakan Produksi baru.

# 2. Keputusan

| ID | Keputusan | Status |
|---|---|---|
| `INC-AUTO-001` | Bahasa utama adalah Bahasa Indonesia `ID-1.0`; identifier teknis canonical tidak diterjemahkan | **AUTO-LOCKED** |
| `INC-AUTO-002` | Dokumen 16 difinalkan sebagai baseline pra-implementasi, bukan bukti kesiapan Produksi | **AUTO-LOCKED** |
| `INC-AUTO-003` | Baseline fitur tetap 550; tidak ada Feature ID baru | **AUTO-LOCKED** |
| `INC-AUTO-004` | Issue, Incident, Defect, Problem, Service Request, dan Security Incident adalah konsep/record berbeda | **AUTO-LOCKED** |
| `INC-AUTO-005` | Lifecycle utama Incident adalah `DETECTED → ACKNOWLEDGED → TRIAGE → IN_PROGRESS → MONITORING → RESOLVED → CLOSED` | **AUTO-LOCKED** |
| `INC-AUTO-006` | Status opsional hanya `WAITING_INFORMATION`, `WAITING_VENDOR`, dan `REOPENED` | **AUTO-LOCKED** |
| `INC-AUTO-007` | Status Incident tidak mengubah status Issue, Job, Defect, atau Problem secara otomatis | **AUTO-LOCKED** |
| `INC-AUTO-008` | `SEV-1..SEV-4` hanya model kerja provisional sampai disahkan Pertamina | **AUTO-LOCKED** |
| `INC-AUTO-009` | Severity, priority, dan target waktu adalah field berbeda; tidak ada pemetaan otomatis | **AUTO-LOCKED** |
| `INC-AUTO-010` | Tidak ada angka SLA/OLA, cadence, threshold, RPO, atau RTO yang dikarang | **AUTO-LOCKED** |
| `INC-AUTO-011` | Setiap Incident aktif mempunyai satu Incident Owner; fungsi ownership bukan role/permission baru | **AUTO-LOCKED** |
| `INC-AUTO-012` | Transfer ownership selesai hanya setelah penerima mengakui handoff | **AUTO-LOCKED** |
| `INC-AUTO-013` | Incident ID, Job ID, Correlation ID, Issue ID, dan Evidence ID bukan authorization grant | **AUTO-LOCKED** |
| `INC-AUTO-014` | User tetap memakai intake Issue yang sederhana; Support melakukan enrichment dan klasifikasi | **AUTO-LOCKED** |
| `INC-AUTO-015` | Issue menjadi Incident hanya setelah dampak/risiko kredibel memerlukan koordinasi | **AUTO-LOCKED** |
| `INC-AUTO-016` | Evidence umum memakai metadata aman dan opaque reference; payload sensitif dilarang | **AUTO-LOCKED** |
| `INC-AUTO-017` | Fakta, hipotesis, dan unknown dicatat terpisah; unknown tidak dianggap aman | **AUTO-LOCKED** |
| `INC-AUTO-018` | Support package memakai `FEAT-SUPPORT-001` dan tidak memuat content/secret/raw diagnostic | **AUTO-LOCKED** |
| `INC-AUTO-019` | Persisted state/REST tetap source of truth; SignalR hanya notifikasi | **AUTO-LOCKED** |
| `INC-AUTO-020` | Putusnya SignalR dipulihkan dengan reconnect dan REST reconciliation | **AUTO-LOCKED** |
| `INC-AUTO-021` | Gangguan server tidak mematikan capability lokal yang masih aman dan diizinkan | **AUTO-LOCKED** |
| `INC-AUTO-022` | Provider gagal tidak mengizinkan public/unapproved AI fallback | **AUTO-LOCKED** |
| `INC-AUTO-023` | Containment paling sempit, reversible, authorized, time-bound, dan monitored | **AUTO-LOCKED** |
| `INC-AUTO-024` | Direct SQL/storage edit, shared credential, atau perubahan Produksi ad hoc dilarang | **AUTO-LOCKED** |
| `INC-AUTO-025` | Recovery hanya melalui application/control-plane/deployment path resmi | **AUTO-LOCKED** |
| `INC-AUTO-026` | Service up bukan bukti recovery; state, result, security, monitoring, queue, dan Audit harus divalidasi | **AUTO-LOCKED** |
| `INC-AUTO-027` | `RESOLVED` berbeda dari `CLOSED`; closure membutuhkan quality gate | **AUTO-LOCKED** |
| `INC-AUTO-028` | Reopen menambah reason/timeline dan menilai ulang impact, severity, priority, serta owner | **AUTO-LOCKED** |
| `INC-AUTO-029` | Security Incident mengikuti proses restricted; Incident/Issue umum bukan evidence store forensik | **AUTO-LOCKED** |
| `INC-AUTO-030` | Tidak ada implicit break-glass, impersonation, atau investigator content access | **AUTO-LOCKED** |
| `INC-AUTO-031` | Mandatory Audit tidak dimatikan; privileged action yang memerlukan durable intent tetap fail closed | **AUTO-LOCKED** |
| `INC-AUTO-032` | Incident berulang/material ditautkan ke Problem tanpa menunda restoration | **AUTO-LOCKED** |
| `INC-AUTO-033` | Known Error memuat safe symptom, cause/confidence, workaround, risk, scope, expiry, dan fix reference | **AUTO-LOCKED** |
| `INC-AUTO-034` | PIR bersifat blameless dan memisahkan system/process factor dari atribusi personal | **AUTO-LOCKED** |
| `INC-AUTO-035` | Corrective action harus mempunyai owner, outcome, status, evidence, dan verification | **AUTO-LOCKED** |
| `INC-AUTO-036` | Komunikasi memakai fakta minimum, dampak, tindakan aman, unknown, dan checkpoint tanpa janji palsu | **AUTO-LOCKED** |
| `INC-AUTO-037` | Security communication dibatasi need-to-know dan memakai holding statement yang disetujui | **AUTO-LOCKED** |
| `INC-AUTO-038` | Escalation memindahkan perhatian/keahlian/authority, bukan memperluas akses data | **AUTO-LOCKED** |
| `INC-AUTO-039` | Vendor/provider escalation hanya melalui route resmi dan data minimum | **AUTO-LOCKED** |
| `INC-AUTO-040` | Metrics tidak boleh mendorong penurunan severity, under-reporting, atau closure prematur | **AUTO-LOCKED** |
| `INC-AUTO-041` | KB umum hanya berisi safe checks; evidence restricted tidak disalin ke KB | **AUTO-LOCKED** |
| `INC-AUTO-042` | Runbook harus versioned, scoped, memiliki forbidden actions, stop conditions, dan validation gate | **AUTO-LOCKED** |
| `INC-AUTO-043` | Tabletop/controlled drill tidak melakukan attack, fault, load, atau destructive action pada Produksi | **AUTO-LOCKED** |
| `INC-AUTO-044` | Rollback, hotfix, migration, restore, infra, dan config rollout selalu diserahkan ke Dokumen 17 | **AUTO-LOCKED** |
| `INC-AUTO-045` | Codex hanya membantu analisis/perubahan terotorisasi; bukan owner, approver, security authority, atau deployer | **AUTO-LOCKED** |

# 3. Keputusan yang Tetap Memerlukan Pertamina

- organisasi, nama fungsi nyata, L1/L2/L3, on-call, contact, dan service hours;
- taxonomy/authority severity dan priority;
- acknowledgement/update/escalation/recovery/RCA target;
- Security/Privacy/Legal/Regulatory process dan komunikasi eksternal;
- evidence repository, chain-of-custody, retention, hold, dan disposal;
- containment/destructive/recovery authority, RPO/RTO, serta vendor route;
- system of record, API, persistence, permission, dan Audit EventType Incident.

Semua tetap berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`, `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION`, atau `REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`.
