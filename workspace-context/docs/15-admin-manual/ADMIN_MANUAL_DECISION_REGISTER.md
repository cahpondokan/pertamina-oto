# REGISTER KEPUTUSAN ADMIN MANUAL
## PERTAMINA OTO — Office Tools Organizer

> **Dokumen:** 15 — Admin Manual  
> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — 40/40 AUTO-LOCKED  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards; Dokumen 01–14 FINAL

---

# 1. Aturan

Keputusan berikut hanya menormalkan dokumentasi dan prosedur administratif dari sumber berwenang. Register ini tidak membuat role, permission, Feature ID, endpoint, tabel, vendor, retensi, SLA, atau akses lintas pengguna baru.

---

# 2. Keputusan

| ID | Keputusan | Status |
|---|---|---|
| `ADM-AUTO-001` | Bahasa Indonesia sederhana dan task-oriented menjadi bahasa utama | **AUTO-LOCKED** |
| `ADM-AUTO-002` | Dokumen 15 difinalkan sebagai `FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE` | **AUTO-LOCKED** |
| `ADM-AUTO-003` | FINAL baseline tidak berarti aplikasi, UAT execution, atau manual telah `RELEASE_READY` | **AUTO-LOCKED** |
| `ADM-AUTO-004` | Label dan menu yang belum terbukti memakai `TBD_UI_LABEL` serta `WAITING_FOR_UI_VALIDATION` | **AUTO-LOCKED** |
| `ADM-AUTO-005` | Role formal tetap `User`, `Administrator`, dan `Auditor` | **AUTO-LOCKED** |
| `ADM-AUTO-006` | Support/Superadmin adalah profil permission terpisah, bukan role formal atau God Mode | **AUTO-LOCKED** |
| `ADM-AUTO-007` | Kewenangan administratif tidak pernah berarti akses isi dokumen | **AUTO-LOCKED** |
| `ADM-AUTO-008` | Auditor default read-only; Administrator/Support memerlukan grant Audit terpisah | **AUTO-LOCKED** |
| `ADM-AUTO-009` | Seluruh administrasi, configuration, monitoring, Support, dan Audit hanya daring | **AUTO-LOCKED** |
| `ADM-AUTO-010` | Mutasi sensitif memerlukan current authorization, exact scope, confirmation, optimistic concurrency, dan reauthentication sesuai Dokumen 09 | **AUTO-LOCKED** |
| `ADM-AUTO-011` | OTO tidak mereset password/MFA atau memulihkan identity Pertamina | **AUTO-LOCKED** |
| `ADM-AUTO-012` | Status identity Pertamina, OTO eligibility, application session, dan effective access ditampilkan terpisah | **AUTO-LOCKED** |
| `ADM-AUTO-013` | Least privilege, deny-by-default, no self-elevation, no wildcard, SoD, dan delegation scope wajib | **AUTO-LOCKED** |
| `ADM-AUTO-014` | Administrator hanya melihat metadata aman dalam `ADMIN_SCOPE` | **AUTO-LOCKED** |
| `ADM-AUTO-015` | Feature Registry descriptor canonical bersifat read-only; admin hanya mengubah control typed yang resmi | **AUTO-LOCKED** |
| `ADM-AUTO-016` | Evaluasi konfigurasi membedakan urutan komposisi dan prioritas pembatasan; hard safety selalu menang | **AUTO-LOCKED** |
| `ADM-AUTO-017` | Administrasi 33 Core hanya membahas availability, exposure, processing posture, dependency, limit, health, dan compatibility | **AUTO-LOCKED** |
| `ADM-AUTO-018` | Seluruh 33 Core tetap `BELUM TERSEDIA` sampai exact RC, exposure, dan UAT evidence tersedia | **AUTO-LOCKED** |
| `ADM-AUTO-019` | Konfigurasi tidak boleh memaksa Feature local-only memakai server | **AUTO-LOCKED** |
| `ADM-AUTO-020` | Angka limit, quota, timeout, retry, threshold, dan retention yang belum disetujui memakai `TARGET_TO_BE_APPROVED` atau `POLICY_CONFIGURABLE` | **AUTO-LOCKED** |
| `ADM-AUTO-021` | Maksimum tiga versi Secured Files adalah invariant dan tidak dapat dinaikkan admin | **AUTO-LOCKED** |
| `ADM-AUTO-022` | Persisted Job state/REST berwenang; SignalR hanya notifikasi dan REST menjadi recovery | **AUTO-LOCKED** |
| `ADM-AUTO-023` | Baseline Administrator hanya memonitor Job/queue; cancel/retry/resume lintas owner tidak dioperasionalkan tanpa permission/API resmi | **AUTO-LOCKED** |
| `ADM-AUTO-024` | Secured Files admin dimulai dari aggregate/safe metadata; tidak ada open/download payload atau storage browsing | **AUTO-LOCKED** |
| `ADM-AUTO-025` | Payload Secured Files tetap di secure file/object storage; SQL Server hanya metadata | **AUTO-LOCKED** |
| `ADM-AUTO-026` | Support bekerja per tindakan dan exact `SUPPORT_SCOPE`; identifier bukan grant | **AUTO-LOCKED** |
| `ADM-AUTO-027` | Status Support canonical memakai `SELESAI`; tidak membuat enum `RESOLVED` atau `CLOSED` | **AUTO-LOCKED** |
| `ADM-AUTO-028` | Internal note singkat, faktual, restricted, dan bebas secret/content/raw diagnostic | **AUTO-LOCKED** |
| `ADM-AUTO-029` | Issue export tetap PDF satu Issue dan Excel rekap; Audit export baseline UTF-8 CSV dengan PDF opsional yang disetujui | **AUTO-LOCKED** |
| `ADM-AUTO-030` | Enterprise Audit, User History, Technical Log/Observability, dan Issue/Support tetap empat data plane terpisah | **AUTO-LOCKED** |
| `ADM-AUTO-031` | Source Audit event tidak dapat diedit/dihapus oleh role manusia; lifecycle dijalankan workload governed | **AUTO-LOCKED** |
| `ADM-AUTO-032` | Monitoring hanya memakai safe aggregate metadata dan tidak menjadi content-access atau Job-control grant | **AUTO-LOCKED** |
| `ADM-AUTO-033` | Health memakai status `SEHAT`, `PERINGATAN`, `TERBATAS`, `TIDAK TERSEDIA`; badge `MEMERLUKAN TINDAKAN` terpisah | **AUTO-LOCKED** |
| `ADM-AUTO-034` | Telemetry hilang/stale tidak pernah dianggap sehat; tampilkan `TIDAK TERSEDIA` dan `MEMERLUKAN TINDAKAN` | **AUTO-LOCKED** |
| `ADM-AUTO-035` | Alert hanya memuat arti, dampak, safe check/action, larangan, dan trigger eskalasi; incident runbook tetap Dokumen 16 | **AUTO-LOCKED** |
| `ADM-AUTO-036` | Perubahan configuration hanya melalui Admin Console/API resmi; direct SQL/config/secret/source/package edit dilarang | **AUTO-LOCKED** |
| `ADM-AUTO-037` | Backup/restore dalam Admin Manual hanya status, request/authorization, impact, dan verification boundary | **AUTO-LOCKED** |
| `ADM-AUTO-038` | Screenshot hanya dari exact approved RC dengan data dummy/sanitized; placeholder tidak dianggap screenshot | **AUTO-LOCKED** |
| `ADM-AUTO-039` | Detail incident berada di Dokumen 16 dan deployment/infrastructure berada di Dokumen 17 | **AUTO-LOCKED** |
| `ADM-AUTO-040` | `RELEASE_READY` hanya setelah UI, contract, permission, screenshot, UAT, documentation test, owner, dan publication gate lulus pada exact RC | **AUTO-LOCKED** |

---

# 3. Input yang Menunggu Konfirmasi atau Implementasi

```text
REQUIRES_PERTAMINA_SSO_CONFIRMATION
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
WAITING_FOR_API_CONTRACT
WAITING_FOR_UI_VALIDATION
WAITING_FOR_SCREENSHOT
TARGET_TO_BE_APPROVED
```

Input tersebut mencakup exact SSO/profile/reauth behavior, delegation/SoD, Support/Audit scope, retention/hold, numeric limits/thresholds, alert/escalation route, Admin UI, management/session contracts, secured metadata scope, cross-owner Job action, screenshot, owner, dan publication route.

```text
Keputusan terkunci : 40 / 40 AUTO-LOCKED
Konflik material   : 0
User approval      : tidak diperlukan
Status Dokumen 15  : FINAL v1.0 — PRE-IMPLEMENTATION BASELINE
```
