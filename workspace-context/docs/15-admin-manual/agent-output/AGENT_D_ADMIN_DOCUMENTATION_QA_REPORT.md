# AGENT D — ADMIN DOCUMENTATION UX, SCREENSHOT, CHECKLIST & MAINTENANCE REPORT

> **Task ID:** `DOC15-AGENT-D`  
> **Peran:** Senior Technical Documentation Architect; Documentation Project Manager; Enterprise UX Content Designer; Accessibility Documentation Reviewer; Documentation QA Specialist; Senior Software Delivery Architect  
> **Status laporan:** `PRE-IMPLEMENTATION BASELINE` — rekomendasi specialist untuk integrasi; bukan master Admin Manual dan bukan manual release-ready  
> **Baseline:** Documents 01–14 FINAL; 550 Feature canonical; Role Matrix Document 09 dan Audit Trail Document 10 tetap berwenang  
> **Status master yang disarankan:** `FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE`  
> **Aplikasi/build:** `RC-TBD`  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`  
> **Tanggal analisis:** 1 September 2026

Laporan ini merancang Admin Manual yang task-oriented, aman, dapat dicari, dan dapat dirawat oleh satu developer + Codex. Laporan tidak membuat screenshot, tidak memakai data Produksi, tidak mengubah Role Matrix atau UI requirement, tidak membuat role/permission/Feature ID baru, tidak menulis Incident Guide/Deployment Guide, tidak mengedit master Document 15, dan tidak melakukan coding.

Baseline yang wajib dipertahankan:

- role formal hanya `User`, `Administrator`, dan `Auditor`;
- `Support/Superadmin` adalah profil permission terpisah, bukan role formal keempat atau wildcard;
- permission mengizinkan percobaan tindakan, tetapi keputusan akhir tetap memerlukan scope, policy, resource state/version, connectivity, capability, confirmation, dan reauthentication yang berlaku;
- UI hide/disable hanya membantu UX; server/application service tetap security boundary;
- Administrator tidak otomatis menjadi User, Auditor, atau Support dan tidak otomatis dapat membaca document content, secured-file content, Issue, atau Enterprise Audit;
- Auditor read-only dalam `AUDIT_SCOPE`; tidak ada permission manusia untuk mengubah atau menghapus source Audit event;
- Support hanya bekerja pada Issue/attachment/note/export dalam `SUPPORT_SCOPE` yang disetujui;
- seluruh tindakan administration, configuration, monitoring, Support, dan Audit hanya daring;
- Enterprise Audit, User History, Technical Log/Observability, dan Issue/Support adalah empat data plane terpisah;
- password, token, secret, document/OCR/Assistant content, visual signature, raw diagnostic, dan production connection information tidak boleh masuk manual evidence atau screenshot;
- Feature flag/policy tidak dapat mematikan hard security invariant, Audit wajib, atau larangan public AI;
- UAT specification Document 13 sudah FINAL, tetapi execution UAT belum direkam;
- User Manual Document 14 berstatus FINAL pre-implementation, belum release-ready, dan masih memakai `TBD_UI_LABEL` serta screenshot placeholder.

---

# 1. Manual information architecture

## 1.1 Prinsip

1. **Task-first.** Administrator mencari “Nonaktifkan kelayakan OTO” atau “Periksa antrean”, bukan nama controller, table, atau service.
2. **Tidak perlu dibaca berurutan.** Setiap prosedur memuat permission, target, dampak, rollback, Audit expectation, verification, dan escalation.
3. **Role/profile-aware.** Halaman menyatakan actor dan permission tepat; nama menu atau akses halaman tidak dianggap authority.
4. **Control-plane only.** Instruksi Administrator memakai metadata aman dan tidak membuka isi dokumen pengguna.
5. **Progressive disclosure.** Langkah utama singkat; detail policy, troubleshooting, dan referensi diletakkan setelah langkah.
6. **Version-bound.** Label, screenshot, dan hasil selalu terikat exact build/environment; `TBD_UI_LABEL` dipakai sampai RC membuktikannya.
7. **Search-friendly.** Heading menggunakan kata kerja + objek, misalnya “Mencabut session aplikasi”, bukan “Manajemen sesi”.
8. **Safe stop.** Jika permission, environment, target, current version, reason, atau rollback tidak jelas, prosedur berhenti sebelum mutasi.
9. **No false recovery.** Manual hanya menyebut rollback yang benar-benar didukung dan diuji pada exact RC.
10. **Boundary-safe.** Incident response, deployment, source code, SQL, dan user tool guide berada di dokumen lain.

## 1.2 Struktur yang disarankan

```text
15 ADMIN MANUAL
├── Tentang Manual Ini
│   ├── Versi, environment, dan audience
│   ├── Cara mencari prosedur
│   └── Arti warning/permission/audit label
├── Mulai Administrasi dalam 10 Menit
├── Account & Access
│   ├── Cari metadata account
│   ├── Aktifkan/nonaktifkan kelayakan OTO
│   ├── Lihat session aplikasi
│   ├── Cabut session aplikasi
│   ├── Lihat role/permission canonical
│   └── Assign/revoke dalam delegation scope
├── Feature & Policy
│   ├── Lihat policy typed
│   ├── Ubah policy yang disetujui
│   ├── Atur exposure/Feature flag yang sudah ada
│   ├── Atur limit dalam hard ceiling
│   ├── Atur maintenance/minimum version
│   └── Verifikasi versi dan rollback
├── Jobs & Storage
│   ├── Lihat metadata agregat job/queue/sync
│   ├── Lihat health storage
│   └── Kenali batas: no content/no arbitrary job control
├── Support
│   ├── Akses hanya dengan Support profile terpisah
│   ├── Triage/status/catatan Issue
│   ├── Buka explicit attachment secara aman
│   └── Buat/ambil bounded Support export
├── Audit
│   ├── Akses hanya dengan Audit grant terpisah atau role Auditor
│   ├── Query/filter/detail read-only
│   ├── Bounded export bila berizin
│   └── Batas: no edit/delete/content pivot
├── Monitoring
│   ├── System Health
│   ├── Job/queue/sync aggregate
│   ├── Error/storage/version aggregate
│   └── Correlation ID tracking tanpa content
├── Troubleshooting
│   ├── Permission denied
│   ├── Reauthentication required
│   ├── Conflict/stale version
│   ├── Job/queue waiting
│   └── Safe escalation
├── Security
│   ├── Environment dan target confirmation
│   ├── Sensitive-data boundary
│   ├── Evidence/screenshot handling
│   └── High-impact procedure checklist
├── FAQ
├── Glossary
├── Indeks Prosedur
├── Indeks Permission
└── Referensi Error/Message Code Aman
```

## 1.3 Peta kebutuhan pembaca

| Kebutuhan | Mulai di | Actor/grant | Hasil yang dicari |
|---|---|---|---|
| Pemeriksaan harian | Quick Start | Administrator | Health dan risiko operasional aman dipahami |
| Account tidak dapat memakai OTO | Account & Access | Administrator | Eligibility/session/permission diperiksa tanpa mengelola password |
| Feature tidak terlihat | Feature & Policy | Administrator | Exposure, policy, compatibility, dan permission dipisahkan |
| Antrean meningkat | Jobs & Storage/Monitoring | Administrator | Safe aggregate + Correlation ID untuk escalation |
| Issue penting | Support | Support profile terpisah | Triage Issue dalam exact scope |
| Verifikasi perubahan privileged | Audit | Auditor/Audit grant terpisah | Bukti read-only dalam `AUDIT_SCOPE` |
| Perubahan policy berisiko | Feature & Policy | Administrator + reauth | Typed/versioned mutation, verification, rollback yang didukung |
| Gejala incident | Troubleshooting | Actor yang berwenang | Stop, preserve safe evidence, cross-reference Document 16 |

## 1.4 Batas isi

| Ada dalam Admin Manual | Hanya cross-reference |
|---|---|
| Tugas control-plane, permission, target, impact, rollback, Audit expectation, verification | Cara memakai 33 Core Tools → Document 14 |
| Safe health/job/queue/storage metadata | Operational incident runbook dan escalation detail → Document 16 |
| Admin-facing Support/Audit boundaries | Install, deploy, migration, secret injection, topology → Document 17 |
| Penggunaan UI pada exact RC | Source code, API internals, database scripts → technical documentation |

---

# 2. Admin Quick Start

## 2.1 Mulai Administrasi dalam 10 Menit

### Sebelum mulai

```text
[ ] Gunakan account pribadi yang disetujui; jangan shared credential.
[ ] Pastikan environment benar dan bukan Produksi bila hanya berlatih.
[ ] Pastikan session aktif dan permission masih current.
[ ] Jangan lanjut jika UI menampilkan data di luar delegated scope.
```

### Langkah

1. Login melalui jalur Pertamina yang disetujui. Jangan memasukkan password ke catatan atau screenshot.
2. Pastikan header/environment indicator menunjukkan environment yang benar. Jika indicator belum final, berhenti dan verifikasi melalui jalur resmi.
3. Buka **`TBD_UI_LABEL: System Health`** jika memiliki `monitoring.system.read`. Periksa state, reason code aman, version, storage aggregate, dan timestamp data.
4. Periksa Issue penting hanya jika memiliki Support profile yang sesuai. Tanpa `support.issue.list`, gunakan jalur handoff Support; jangan mencoba direct link.
5. Buka **`TBD_UI_LABEL: Job & Queue Monitoring`** jika memiliki `monitoring.job.read`. Periksa aggregate waiting/failed/retrying dan Correlation ID aman. Monitoring tidak memberi hak cancel/retry job pengguna.
6. Kelola user hanya bila ada kebutuhan terverifikasi. Gunakan `admin.user.list/read`; perubahan eligibility memakai `admin.user.update`, exact target, reason, reauthentication, dan checklist sensitif.
7. Periksa perubahan policy/configuration melalui versi saat ini dan safe change summary. Untuk Audit verification, gunakan Auditor/Audit grant terpisah; Administrator tanpa grant tersebut meminta verifikasi kepada Auditor.
8. Jika UI menunjukkan critical/unhealthy state yang disetujui—termasuk `UNHEALTHY` untuk health pipeline Audit—ada dugaan unauthorized access/data loss/secret leak, atau langkah recovery tidak jelas, hentikan perubahan dan eskalasi ke proses Document 16.

### Hasil 10 menit

Administrator harus dapat menjawab tanpa membuka isi dokumen pengguna:

- environment dan build apa yang sedang dilihat;
- apakah health sehat, degraded, atau unhealthy menurut UI aktual;
- apakah ada aggregate job/queue/sync yang perlu perhatian;
- apakah ada perubahan account/configuration yang benar-benar diperlukan;
- permission apa yang akan dipakai;
- apakah langkah berikutnya merupakan administrasi biasa atau harus dieskalasikan.

## 2.2 Jalur aman jika akses tidak tersedia

| Kondisi | Tindakan |
|---|---|
| Menu tidak terlihat | Periksa release/platform/permission; jangan gunakan direct URL untuk bypass |
| `403` atau scope denied | Stop; verifikasi assignment/delegation melalui owner resmi |
| `401 REAUTHENTICATION_REQUIRED` | Reauthenticate melalui UI resmi; jangan meminta credential user lain |
| Data tampak stale | Refresh/relogin sesuai UI; jangan mengandalkan cache atau screenshot lama |
| Health data unavailable | Jangan menyatakan sistem sehat; catat safe time/Correlation ID dan eskalasi |
| Audit access tidak ada | Minta Auditor/Audit-granted reviewer melakukan verifikasi; jangan meminta database dump |
| Support access tidak ada | Handoff ke Support scope owner; jangan meminta Issue attachment dikirim di luar sistem |

---

# 3. Writing style

## 3.1 Aturan kalimat

- Gunakan kalimat pendek dan aktif.
- Satu numbered step berisi satu tindakan.
- Mulai heading dengan kata kerja: **Lihat**, **Ubah**, **Cabut**, **Verifikasi**, **Pulihkan**, **Eskalasi**.
- Tulis label UI **tebal** hanya setelah label dibuktikan oleh RC. Sebelum itu gunakan **`TBD_UI_LABEL: ...`**.
- Sebut permission canonical pada metadata prosedur atau callout, bukan berulang di setiap kalimat.
- Letakkan warning sebelum tindakan, bukan setelah akibat terjadi.
- Nyatakan dampak terhadap target, scope, user, session, rollout, job, atau compatibility.
- Bedakan **Batalkan sebelum simpan**, **rollback setelah perubahan**, dan **tidak dapat dibatalkan**.
- Nyatakan Audit expectation sebagai event/fakta yang diharapkan, bukan perintah menulis Audit manual.
- Gunakan escalation condition yang observable; jangan menulis “hubungi tim terkait” tanpa destination yang nantinya disetujui.
- Jangan menjelaskan SQL, storage key, secret injection, internal token, atau bypass.

## 3.2 Contoh

Gunakan:

> ⚠️ **Perhatian —** Menonaktifkan kelayakan OTO dapat menghentikan akses user dan membatalkan akses session sesuai policy. Pastikan target, reason, dan rollback sebelum memilih **`TBD_UI_LABEL: Nonaktifkan akun`**.

Jangan gunakan:

> Administrator selanjutnya dapat melaksanakan proses penonaktifan terhadap akun yang dipilih.

## 3.3 Istilah yang harus konsisten

| Gunakan | Jangan gunakan | Alasan |
|---|---|---|
| Kelayakan OTO / OTO eligibility | Hapus akun Pertamina | OTO tidak mengelola lifecycle identity Pertamina |
| Cabut session aplikasi | Reset password user | Password/MFA milik IdP |
| Assign permission dalam delegation scope | Beri akses penuh | Tidak ada wildcard/full access |
| Feature exposure/flag | Buat Feature baru | Admin hanya mengatur Feature canonical yang ada |
| Metadata agregat aman | Lihat file/job user | Monitoring tidak memberi content access |
| Profil Support/Superadmin | Role Superadmin | Support bukan role formal |
| Enterprise Audit | Riwayat/Log | Data plane berbeda |
| Reauthentication | Permission elevation | Reauth tidak menambah permission |
| Rollback yang didukung | Undo semua perubahan | Tidak semua perubahan reversible |
| Correlation ID | Kunci akses | Identifier bukan credential/authority |

---

# 4. Procedure template

## 4.1 Template wajib

```text
Judul: <Kata kerja + objek>
Tujuan:
Applies to: <manual version / app build / environment / platform>
Actor: <Administrator | Auditor | Support profile>
Permission:
Resource scope:
Risk: <NORMAL | ELEVATED | HIGH-IMPACT>
Online required: YES
Reauthentication: <NO | POLICY | REQUIRED>
Audit expectation:

Before
[ ] permission current
[ ] correct environment
[ ] exact target and current version
[ ] reason and expected impact
[ ] SoD/approval requirement, if applicable
[ ] supported rollback or explicit no-rollback statement

Steps
1. Open the verified RC screen.
2. Select the exact target.
3. Review current value/version/scope.
4. Enter/select only approved typed input.
5. Review impact and confirmation.
6. Reauthenticate when requested.
7. Submit once and wait for authoritative result.

Expected result:
Forbidden result:

After
[ ] authoritative result verified
[ ] expected Audit event verified by an authorized reader
[ ] affected user/stakeholder informed when required
[ ] evidence stored safely
[ ] issue/task/change record updated

Rollback:
Troubleshooting:
Escalation:
Requirement/Feature/permission references:
Screenshot IDs:
Last validated build/date/reviewer:
```

## 4.2 Katalog prosedur sensitif

| Prosedur | Actor dan permission | Risiko/dampak | Audit expectation | Rollback/verification minimum |
|---|---|---|---|---|
| Ubah kelayakan OTO | Administrator — `admin.user.update` | User kehilangan/mendapat akses aplikasi | `Administration.AccountEligibilityChanged` | Verifikasi target/state; re-enable hanya jika berwenang dan reason valid |
| Cabut session aplikasi | Administrator — `admin.session.revoke` | Session target berhenti | `Auth.SessionRevoked` atau `Administration.SessionRevoked` sesuai catalog final | Verifikasi exact session; user login ulang; tidak ada “unrevoke” session |
| Assign/revoke role | Administrator — `admin.role.assign/revoke` | Effective permission dapat berubah luas | `Administration.RoleAssigned/RoleRevoked` | Current assignment/version; inverse action hanya setelah review baru |
| Assign/revoke permission/profile | Administrator — `admin.permission.assign/revoke` | Akses granular/Support dapat berubah | `Administration.PermissionAssigned/PermissionRevoked` atau `SupportGrantChanged` | No self-elevation; verify effective projection dan scope |
| Ubah policy security/classification/retention | Administrator — `configuration.policy.update` | Dampak enterprise/high-impact | `Policy.SecurityPolicyChanged`, `ClassificationChanged`, atau `RetentionPolicyChanged` | Typed previous/new version; rollback hanya ke approved compatible version |
| Ubah Feature exposure/flag | Administrator — `configuration.feature.update` | Feature muncul/hilang pada population | `Configuration.FeatureExposureChanged` | Staged scope, expected population, rollback exposure value |
| Ubah limit/quota | Administrator — `configuration.limit.update` | Admission, job, storage dapat terpengaruh | `Configuration.LimitChanged` | Tetap dalam hard ceiling; verify health/backpressure |
| Ubah maintenance/minimum client version | Administrator — `configuration.compatibility.update` | Client dapat dibatasi/update-required | `Configuration.MaintenanceModeChanged` / `MinimumClientVersionChanged` | Staged validation; documented safe rollback; version compatibility check |
| Buat Audit export | Auditor/Audit grant — `audit.export.create` | Bulk evidence disclosure | `AuditAccess.ExportRequested/Completed` | Scope/date/field/as-of/expiry verified; no broad unbounded export |
| Ambil Audit export | Auditor/Audit grant — `audit.export.read` | Download evidence sensitif | `AuditAccess.ExportDownloaded` | Current authorization dan expiry; secure handling/disposal |
| Buka Issue attachment | Support profile — `support.attachment.read` | Explicit user-sent content dapat sensitif | `Support.AttachmentViewed` | Exact Issue-child membership; no local uncontrolled copy |
| Triage/status/note Issue | Support profile — `support.issue.triage`, `support.note.*` | Workflow/status dan restricted note berubah | `Support.IssueTriaged/StatusChanged/InternalNoteAdded` | Current version/state; typed transition; correction as new action |
| Buat/ambil Support export | Support profile — `support.export.create/read` | Bounded Issue disclosure | `Support.ExportRequested/Completed/Downloaded` | Exact filter/scope/expiry; safe storage/disposal |

Dokumentasi tidak boleh meminta admin menulis event Audit secara manual. Audit expectation digunakan untuk verification dan testing.

---

# 5. Warning/label standard

Emoji berikut hanya drafting aid. Manual final harus mengikuti branding/style Pertamina yang disetujui dan selalu mempertahankan teks label, bukan mengandalkan warna/ikon.

| Label | Kapan digunakan | Isi minimum |
|---|---|---|
| ℹ️ **Catatan** | Konteks/penjelasan non-blocking | Fakta singkat dan relevan |
| ⚠️ **Perhatian** | Ada dampak operasional atau risiko kesalahan | Target, dampak, cara mencegah |
| 🚨 **Tindakan berisiko** | Mutasi high-impact/destructive/bulk disclosure | Exact target/scope, reauth/approval, rollback atau no rollback |
| 🔐 **Memerlukan permission** | Tindakan hanya untuk grant tertentu | Permission canonical dan scope |
| 🔄 **Dapat dibatalkan** | Rollback terbukti tersedia | Batas waktu/state dan cara verifikasi |
| 📜 **Dicatat dalam audit** | Event wajib diharapkan | Fakta event, bukan payload sensitif |
| 🌐 **Memerlukan koneksi** | Seluruh privileged action baseline | Tidak ada offline/cached authority |
| 🛑 **Eskalasi diperlukan** | Incident, control failure, unknown target/scope, atau no safe recovery | Stop condition, safe evidence, cross-reference |

Aturan kombinasi:

- perubahan permission atau policy high-impact: `🚨 + 🔐 + 📜 + 🌐`;
- rollback terbukti: tambahkan `🔄`;
- incident/security signal: gunakan `🛑`, jangan menyisipkan runbook Document 16;
- label tidak menggantikan confirmation UI atau authorization check.

Contoh callout:

> 🚨 **Tindakan berisiko**  
> 🔐 Memerlukan `configuration.compatibility.update`.  
> 📜 Diharapkan menghasilkan `Configuration.MinimumClientVersionChanged`.  
> Perubahan dapat memblokir client lama. Pastikan rollout dan rollback versi sudah disetujui sebelum melanjutkan.

---

# 6. Navigation/findability

## 6.1 Komponen wajib

- daftar isi dengan link per tugas;
- Quick Links: Health, Account, Role/Permission, Feature/Policy, Job/Queue, Support, Audit, Troubleshooting;
- indeks prosedur berdasarkan kata kerja dan objek;
- indeks permission canonical;
- indeks Feature ID untuk `FEAT-ADMIN-*`, `FEAT-AUDIT-*`, `FEAT-MON-*`, dan `FEAT-SUPPORT-*` yang relevan;
- FAQ dengan pertanyaan dalam bahasa pengguna;
- glossary untuk istilah privileged;
- referensi error/message code aman yang ditarik dari catalog release, bukan dikarang;
- related procedures pada akhir halaman;
- cross-reference ke Documents 09, 10, 14, 16, dan 17;
- metadata `AppliesToAppVersion`, environment, platform, role/profile, permission, dan last validated build.

## 6.2 Indeks prosedur minimum

| Cari kata | Arahkan ke |
|---|---|
| login/session/account/eligibility | Account & Access |
| role/permission/grant/revoke | Account & Access → Role & Permission |
| feature tidak muncul/flag/exposure | Feature & Policy |
| limit/quota/retention/recycle | Feature & Policy → Typed Policy/Limit |
| maintenance/update/version | Feature & Policy → Compatibility |
| job/queue/sync/waiting | Jobs & Storage / Monitoring |
| storage/health/capacity | Monitoring → Storage Health |
| issue/support/attachment/triage | Support, hanya bila memiliki profile |
| audit/search/filter/export | Audit, hanya dengan Audit grant |
| denied/reauth/conflict/correlation | Troubleshooting |
| incident/data loss/unauthorized/secret | Stop + Document 16 |

## 6.3 Referensi error-code

Sebelum RC, gunakan pola:

```text
Display message: <actual localized message>
Message/Error Code: TBD_ERROR_CODE
Meaning: <approved safe meaning>
Administrator action: <safe next step>
Escalate when: <observable condition>
Evidence: Correlation ID / timestamp / build only as needed
```

Jangan menyalin raw exception, stack trace, SQL, storage path, token, atau request body. `401`, `403`, `404`, `409`, dan `412` dapat dijelaskan secara konseptual hanya setelah perilaku exact route/UI tervalidasi.

## 6.4 Findability test

Administrator baru harus dapat menemukan dalam maksimal dua pencarian/quick-link:

1. cara memeriksa health;
2. cara menonaktifkan eligibility dengan aman;
3. alasan permission belum berlaku;
4. batas akses Admin terhadap file pengguna;
5. cara menemukan Correlation ID;
6. kapan harus eskalasi.

Waktu numerik resmi selain target review internal ini harus dikonfirmasi jika akan menjadi acceptance/SLA Pertamina.

---

# 7. Screenshot register

## 7.1 Field wajib

| Field | Aturan |
|---|---|
| Screenshot ID | Stabil, unik, tidak memakai Feature ID baru |
| Chapter | Bab/prosedur pemilik |
| Role | Actor yang benar, bukan label menu |
| Permission | Permission canonical minimum |
| Environment | QA/UAT exact; bukan Production untuk dokumentasi biasa |
| Platform | Platform/surface exact |
| Language | Locale exact, `ID` primary |
| Build Version | Exact immutable RC/build; `RC-TBD` sebelum tersedia |
| Screen | Actual screen title pada RC |
| Purpose | Hal yang harus dipahami pembaca |
| Data Type | Dummy/synthetic data yang digunakan |
| Masking Required | `YES`, `NO`, atau `REVIEW`; security review tetap wajib |
| Status | Lifecycle terkendali |
| Owner | Documentation owner/reviewer yang ditetapkan |

## 7.2 Status

```text
PLANNED
→ CAPTURED
→ REVIEWED
→ SECURITY_REVIEWED
→ APPROVED

OUTDATED
→ tidak boleh tampil pada manual rilis aktif
```

Status `APPROVED` harus terikat Screenshot ID, digest/artifact reference, build, environment, platform, locale, review keamanan, owner, dan tanggal. Mockup/wireframe tidak boleh diberi status screenshot aktual.

## 7.3 Register awal

Tidak ada screenshot dibuat dalam laporan ini. Semua entry berstatus `PLANNED`, build `RC-TBD`, dan memakai data dummy/synthetic.

| Screenshot ID | Chapter | Role | Permission | Environment | Platform | Language | Build Version | Screen | Purpose | Data Type | Masking Required | Status | Owner |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `SS-ADM-QS-001-ID` | Quick Start | Administrator | `monitoring.system.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | System Health | Menunjukkan state/reason/timestamp aman | Dummy health aggregate | REVIEW | PLANNED | TBC |
| `SS-ADM-QS-002-ID` | Quick Start | Administrator | `monitoring.job.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Job & Queue Monitoring | Menunjukkan aggregate dan safe drill-down | Dummy jobs | YES | PLANNED | TBC |
| `SS-ADM-ACC-001-ID` | Account | Administrator | `admin.user.list` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | User list | Cari metadata account aman | Dummy users | YES | PLANNED | TBC |
| `SS-ADM-ACC-002-ID` | Account | Administrator | `admin.user.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | User detail | Meninjau eligibility tanpa credential/content | Dummy user | YES | PLANNED | TBC |
| `SS-ADM-ACC-003-ID` | Account | Administrator | `admin.user.update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Eligibility confirmation | Exact target, impact, reason, reauth | Dummy user | YES | PLANNED | TBC |
| `SS-ADM-SES-001-ID` | Session | Administrator | `admin.session.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Session list | Safe app-session metadata | Dummy sessions | YES | PLANNED | TBC |
| `SS-ADM-SES-002-ID` | Session | Administrator | `admin.session.revoke` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Revoke confirmation | Exact session/impact/reauth | Dummy session | YES | PLANNED | TBC |
| `SS-ADM-ROL-001-ID` | Role | Administrator | `admin.role.list/read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Role catalog | Tiga role formal dan assignment | Dummy principals | YES | PLANNED | TBC |
| `SS-ADM-ROL-002-ID` | Role | Administrator | `admin.role.assign/revoke` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Role change review | Scope, current version, SoD, confirmation | Dummy principal/role | YES | PLANNED | TBC |
| `SS-ADM-PER-001-ID` | Permission | Administrator | `admin.permission.list/read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Permission catalog | Canonical code dan scope | Dummy assignments | YES | PLANNED | TBC |
| `SS-ADM-PER-002-ID` | Permission | Administrator | `admin.permission.assign/revoke` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Permission change review | No self-elevation; exact scope | Dummy principal/grant | YES | PLANNED | TBC |
| `SS-ADM-POL-001-ID` | Policy | Administrator | `configuration.policy.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Policy list/detail | Typed values, scope, version | Dummy policy | YES | PLANNED | TBC |
| `SS-ADM-POL-002-ID` | Policy | Administrator | `configuration.policy.update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Policy change confirmation | Before/after safe digest, impact, reauth | Dummy policy | YES | PLANNED | TBC |
| `SS-ADM-FEA-001-ID` | Feature | Administrator | `configuration.feature.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Feature exposure | Existing canonical Feature/status | Dummy exposure | REVIEW | PLANNED | TBC |
| `SS-ADM-FEA-002-ID` | Feature | Administrator | `configuration.feature.update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Exposure/flag change | Scope, rollout, impact, rollback | Dummy Feature config | YES | PLANNED | TBC |
| `SS-ADM-LIM-001-ID` | Limit | Administrator | `configuration.limit.read/update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Limit change | Typed limit dan hard ceiling | Dummy limit | YES | PLANNED | TBC |
| `SS-ADM-CMP-001-ID` | Compatibility | Administrator | `configuration.compatibility.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Maintenance/version policy | Current minimum version/state | Dummy versions | REVIEW | PLANNED | TBC |
| `SS-ADM-CMP-002-ID` | Compatibility | Administrator | `configuration.compatibility.update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Compatibility change | Impact population dan rollback | Dummy versions | YES | PLANNED | TBC |
| `SS-ADM-MON-001-ID` | Monitoring | Administrator | `monitoring.system.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Storage/version/error health | Aggregate tanpa document metadata | Dummy telemetry | YES | PLANNED | TBC |
| `SS-ADM-MON-002-ID` | Monitoring | Administrator | `monitoring.job.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Correlation drill-down | Correlation tracking tanpa pivot content | Dummy Correlation ID | YES | PLANNED | TBC |
| `SS-ADM-AUD-001-ID` | Audit | Auditor | `audit.event.list` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Audit query/filter | Scoped authorization-first list | Dummy audit metadata | YES | PLANNED | TBC |
| `SS-ADM-AUD-002-ID` | Audit | Auditor | `audit.event.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Audit detail | Safe evidence; no content pivot | Dummy audit event | YES | PLANNED | TBC |
| `SS-ADM-AUD-003-ID` | Audit | Auditor/Audit grant | `audit.export.create/read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Audit export | Bounded filter, status, expiry | Dummy audit export | YES | PLANNED | TBC |
| `SS-ADM-SUP-001-ID` | Support | Support profile | `support.issue.list/read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Support Issue list/detail | Assigned scope dan safe diagnostics | Dummy Issue | YES | PLANNED | TBC |
| `SS-ADM-SUP-002-ID` | Support | Support profile | `support.attachment.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Attachment review | Explicit child attachment + privacy warning | Synthetic screenshot | YES | PLANNED | TBC |
| `SS-ADM-SUP-003-ID` | Support | Support profile | `support.issue.triage` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Triage/status | Typed transition dan current version | Dummy Issue | YES | PLANNED | TBC |
| `SS-ADM-SUP-004-ID` | Support | Support profile | `support.note.read/create` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Internal note | Restricted note boundary | Dummy note | YES | PLANNED | TBC |
| `SS-ADM-SUP-005-ID` | Support | Support profile | `support.export.create/read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Support export | Bounded scope/status/expiry | Dummy Issue export | YES | PLANNED | TBC |
| `SS-ADM-SEC-001-ID` | Security | Administrator | `configuration.policy.update` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Reauthentication prompt | Menjelaskan reauth bukan elevation | Dummy policy target | YES | PLANNED | TBC |
| `SS-ADM-ERR-001-ID` | Troubleshooting | Administrator | `monitoring.system.read` | `QA/UAT-TBD` | Web | ID | `RC-TBD` | Safe error/conflict | Message code, Correlation ID, next action | Dummy IDs | YES | PLANNED | TBC |

## 7.4 Caption dan alt text

Setiap screenshot `APPROVED` memerlukan:

- caption berisi task + screen + build, bukan “gambar di atas”;
- alt text yang menjelaskan state/control penting tanpa menyalin seluruh layar;
- urutan langkah yang tetap dapat dipahami tanpa gambar;
- highlight/callout yang tidak menutupi status, target, atau warning;
- no color-only instruction;
- locale label yang sama dengan gambar.

---

# 8. Screenshot security

## 8.1 Dilarang tampil

- password, passphrase, token, cookie, secret, key, connection string, authorization header;
- real user identity, email, employee data, claim, unit detail yang tidak diperlukan;
- confidential document, filename/path nyata, OCR/full text, thumbnail/preview content;
- real Issue message, real screenshot/attachment, internal note, raw diagnostic;
- real Audit restricted metadata atau export;
- visual signature/paraf nyata;
- production hostname, IP, URL, tenant, object key, storage path, database name, deployment connection information;
- raw exception, stack, SQL, request/response body, browser developer token, CLI output berisi secret;
- real Correlation/Job/Issue/Session/Export ID yang dapat ditautkan ke activity Produksi.

## 8.2 Data yang digunakan

```text
Dummy user          : USR-DEMO-001 / Pengguna Contoh
Dummy job           : JOB-DEMO-001
Dummy issue         : ISS-DEMO-001
Dummy correlation   : CORR-DEMO-001
Dummy feature       : canonical Feature ID + synthetic exposure
Dummy file metadata : contoh-admin.pdf, synthetic size/status only
Environment         : QA/UAT controlled, never Production by default
```

Dummy ID harus jelas tidak valid di Produksi dan tidak meniru identitas orang nyata.

## 8.3 Capture workflow

```text
approved exact RC + controlled QA/UAT account/data
→ reset state
→ capture only required region
→ inspect background/browser chrome/notification/clipboard
→ mask or recapture at source
→ content review
→ security/privacy review
→ accessibility/caption review
→ APPROVED artifact with digest
```

Cropping setelah capture tidak cukup jika file asli yang sensitif tetap tersimpan tanpa kontrol. Utamakan synthetic data dan safe source screen. Bila accidental sensitive capture terjadi, hentikan publikasi, batasi akses, hapus melalui approved retention/disposal process, dan gunakan jalur incident Document 16 bila memenuhi kriteria.

## 8.4 Penyimpanan

- simpan source dan approved rendition pada documentation/evidence repository yang access-controlled;
- gunakan classification/retention yang disetujui;
- jangan tempel ke chat, email, ticket, atau public link tanpa authority;
- screenshot OUTDATED tetap tunduk retention/disposal; status tidak menghapus kewajiban keamanan;
- artifact link tidak boleh menjadi durable public URL.

---

# 9. Procedure checklists

## 9.1 Checklist universal — Before

```text
[ ] Actor dan permission sesuai prosedur.
[ ] Session current; tindakan daring; reauth tersedia bila diperlukan.
[ ] Environment/build/policy version benar.
[ ] Exact target berada dalam delegated resource scope.
[ ] Current state/version/ETag telah diperiksa.
[ ] Reason dan expected impact terdokumentasi secara aman.
[ ] SoD/approval terpenuhi bila policy mewajibkan.
[ ] Rollback yang benar-benar didukung telah dipahami dan diuji.
[ ] Tidak ada password/token/content/secret dalam notes/evidence.
[ ] User/stakeholder notification plan tersedia bila relevan.
```

## 9.2 Checklist universal — After

```text
[ ] Authoritative result/state diverifikasi; tidak hanya toast/screenshot.
[ ] Target dan scope hasil sesuai; tidak ada collateral change.
[ ] Expected Audit event diperiksa oleh actor yang berizin.
[ ] Health/error/queue diperiksa bila perubahan berdampak operasional.
[ ] Affected user/stakeholder diberi tahu melalui kanal resmi bila diperlukan.
[ ] Evidence minimum disimpan aman dan ditautkan ke task/change reference.
[ ] Issue/change/task diperbarui tanpa secret/content.
[ ] Rollback dijalankan atau eskalasi dibuat bila verification gagal.
```

## 9.3 Account/session

```text
BEFORE: exact principal/session; scope; eligibility/current state; reauth; impact to active work.
AFTER : new state; session behavior; expected Audit; user notification; no credential handling.
```

Admin Manual tidak mengajarkan reset password, MFA recovery, atau enterprise identity lifecycle karena itu berada pada IdP/proses resmi Pertamina.

## 9.4 Role/permission/profile

```text
BEFORE: canonical role/permission; delegation scope; no self-elevation; SoD; effective-impact review.
AFTER : assignment projection refreshed; stale session behavior checked; Audit event; independent review where required.
```

Jangan gunakan wildcard, scope `ALL`, role hierarchy, atau label Superadmin sebagai grant.

## 9.5 Policy/Feature/limit/compatibility

```text
BEFORE: typed key; current version; exact scope; hard ceiling; rollout; client impact; safe rollback.
AFTER : version increment/state; affected population; monitoring; Audit event; compatibility; rollback decision.
```

Policy tidak dapat menonaktifkan permission/ownership check, validation/security control, Audit wajib, no-secret logging, Assistant boundary, public-AI prohibition, non-PKI scope, atau hard resource ceiling.

## 9.6 Audit/Support export

```text
BEFORE: separate export permission; exact query/scope/date/fields/as-of; classification; reauth; size/expiry.
AFTER : complete-vs-truncated status; manifest/digest; secure retrieval; Audit event; expiry/disposal.
```

Audit baseline export adalah UTF-8 CSV; fixed PDF opsional bila disetujui. XLSX bukan baseline Audit. Support export mengikuti bounded PDF/Excel requirement untuk Issue scope, bukan Audit.

## 9.7 Monitoring/troubleshooting

```text
BEFORE: correct environment/time range; safe aggregate permission; no content expectation.
AFTER : timestamp/freshness; reason code; Correlation ID; escalation decision; no raw payload copy.
```

Admin monitoring bukan Job-control permission. Manual tidak boleh menginstruksikan cancel/retry/download hasil user kecuali permission dan route baru kelak disetujui melalui controlled revision.

---

# 10. Documentation testing

## 10.1 Test record

```text
Documentation Test ID
Manual/Procedure Version
App Build/Digest; Environment; Platform; Locale
Actor/Profile; Permission Snapshot/Revision
Synthetic Dataset ID
Precondition
Steps executed exactly as written
Expected result / Forbidden result
Execution status: PLANNED | EXECUTED
Result when executed: PASS | FAIL | BLOCKED | NOT_APPLICABLE
Evidence IDs
Defect/Documentation Issue
Tester/Reviewer/Timestamp
```

`PLANNED` bukan hasil UAT. Skenario yang belum dijalankan tidak diberi `PASS`.

## 10.2 Matriks validasi

| Check | Pertanyaan | Reviewer/evidence | Gate |
|---|---|---|---|
| Role Matrix Check | Apakah actor, permission, scope, online, reauth, dan prohibited access cocok Document 09? | Security/authorization reviewer + trace | Mandatory |
| Security Check | Apakah langkah/evidence/screenshot bebas secret/content dan tidak mengajarkan bypass? | Security reviewer | Mandatory |
| UAT Cross-Check | Apakah hasil admin/support/audit cocok UAT Sections 29–34 dan recovery? | UAT evidence exact RC | Mandatory |
| UI Label Check | Apakah semua label/posisi cocok locale/build aktual? | UI resource + screenshot | Mandatory |
| Permission Check | Apakah allow/deny/wrong scope/stale/revoked/direct route telah diuji? | Authorization test evidence | Mandatory |
| Screenshot Check | Apakah image exact RC, dummy, captioned, alt text, security reviewed? | Screenshot register | Mandatory when used |
| Link Check | Apakah TOC, anchor, cross-reference, artifact link valid dan berizin? | Automated + manual link check | Mandatory |
| Task-Based Review | Dapatkah task selesai hanya dari satu procedure? | Observer notes | Mandatory |
| New Administrator Review | Dapatkah reviewer baru menyelesaikan tugas dasar tanpa instruksi verbal? | Independent execution | Mandatory |

## 10.3 Skenario documentation QA minimum

| ID | Task | Positive path | Negative/safety path |
|---|---|---|---|
| `DOCQA-ADM-001` | Quick Start | Health/job aggregate ditemukan | Missing permission tidak mendorong bypass |
| `DOCQA-ADM-002` | Ubah eligibility | Exact target/state berubah dan diverifikasi | Wrong scope/stale version/reauth missing berhenti |
| `DOCQA-ADM-003` | Cabut session | Exact session revoked | Credential tidak diminta; wrong session dicegah |
| `DOCQA-ADM-004` | Assign role/permission | Grant within delegation | Self-elevation/wildcard/out-of-scope ditolak |
| `DOCQA-ADM-005` | Ubah policy | Typed/versioned change + rollback | Hard security invariant tidak dapat dimatikan |
| `DOCQA-ADM-006` | Feature exposure | Existing Feature dan population tepat | Tidak membuat Feature ID atau mengubah semantics |
| `DOCQA-ADM-007` | Compatibility | Staged update dan impact jelas | Old/stale version conflict aman |
| `DOCQA-ADM-008` | Audit verification | Auditor menemukan expected event | Admin tanpa Audit grant diarahkan handoff, bukan bypass |
| `DOCQA-ADM-009` | Support triage | Exact Issue scope/status/note | Cross-Issue attachment/pivot ditolak |
| `DOCQA-ADM-010` | Troubleshoot queue | Safe aggregate/Correlation ID ditemukan | Admin tidak diberi undocumented job-control action |

## 10.4 New Administrator Review

Reviewer yang belum ikut menulis manual harus menjalankan pada controlled QA/UAT:

1. login dan memastikan environment;
2. melihat health;
3. menemukan user dummy;
4. menjelaskan permission dan dampak sebelum eligibility change;
5. melakukan satu approved reversible dummy change;
6. memverifikasi result dan handoff Audit;
7. menemukan prosedur troubleshooting dan escalation.

Tester tidak menerima instruksi lisan tambahan. Pertanyaan yang muncul dicatat sebagai documentation defect, bukan dijawab diam-diam lalu dinyatakan PASS.

## 10.5 Severity dokumentasi

| Severity | Contoh |
|---|---|
| BLOCKER | Manual tidak dapat dipakai untuk task mandatory atau menunjuk environment/target salah |
| CRITICAL | Instruksi dapat menyebabkan unauthorized access, secret/content leak, data loss, atau security control bypass |
| MAJOR | Permission/impact/rollback/Audit expectation salah atau langkah utama gagal |
| MINOR | Typo, caption, navigasi, atau wording tanpa mengubah outcome/safety |

Workflow defect tetap mengikuti Document 12/13; manual tidak membuat enum defect baru.

---

# 11. Role/permission validation

## 11.1 Urutan evaluasi

```text
Authenticated/current online session
→ action permission
→ subject type
→ delegated resource scope/ownership
→ policy/classification
→ resource state/version
→ environment/client/capability/compatibility
→ confirmation/consent
→ fresh reauthentication when required
→ application service execution
→ authoritative result + safe Audit evidence
```

Menu, cached page, direct URL, display role, Correlation ID, Job ID, Issue ID, atau screenshot tidak pernah memberikan permission.

## 11.2 Role/profile boundary

| Actor | Default purpose | Default allowed surface | Explicitly not implied |
|---|---|---|---|
| Administrator | Control-plane OTO dan safe operational metadata | Account/session, role/permission, typed configuration, safe monitoring dalam `ADMIN_SCOPE` | User document tools/content, Audit console/export, Support Issue/content |
| Auditor | Read-only Enterprise Audit evidence | `audit.event.list/read` dalam `AUDIT_SCOPE`; export separately assigned | Configuration/user/Issue mutation; document/Issue/Assistant content; Audit edit/delete |
| Support/Superadmin profile | Issue workflow dalam assignment/queue scope | `support.*` yang diberikan dalam `SUPPORT_SCOPE` | Role hierarchy, general admin/Audit, arbitrary user/job/file/storage content |
| User | Own office work | Document/workspace/job/history/secured/Issue own scope | Administration, Enterprise Audit, Support queue |

## 11.3 Permission index Administrator

| Task | Feature trace | Read permission | Mutation permission | Reauth | Audit |
|---|---|---|---|---|---|
| Account metadata/eligibility | `FEAT-ADMIN-001` | `admin.user.list/read` | `admin.user.update` | Required for mutation | Yes |
| Session metadata/revoke | `FEAT-AUTH-*`, `FEAT-ADMIN-001` | `admin.session.read` | `admin.session.revoke` | Required for revoke | Yes |
| Role catalog/assignment | `FEAT-ADMIN-002` | `admin.role.list/read` | `admin.role.assign/revoke` | Required | Yes |
| Permission/profile catalog/assignment | `FEAT-ADMIN-003` | `admin.permission.list/read` | `admin.permission.assign/revoke` | Required | Yes |
| Security/classification/retention policy | `FEAT-ADMIN-009/012/013` | `configuration.policy.read` | `configuration.policy.update` | Required/high-impact | Yes |
| Feature enable/flag | `FEAT-ADMIN-004/005` | `configuration.feature.read` | `configuration.feature.update` | High-impact policy | Yes |
| File/process/storage thresholds | `FEAT-ADMIN-006..011` | `configuration.limit.read` | `configuration.limit.update` | High-impact policy | Yes |
| Maintenance/minimum app version | `FEAT-ADMIN-014/015/022` | `configuration.compatibility.read` | `configuration.compatibility.update` | High-impact policy | Yes |
| Job/queue/sync monitoring | `FEAT-ADMIN-016..018`, `FEAT-MON-004..007` | `monitoring.job.read` | None in baseline | No default | Conditional |
| Error/server/storage/version health | `FEAT-ADMIN-019..022`, `FEAT-MON-001..017` | `monitoring.system.read` | None in baseline | No default | Conditional |

## 11.4 Audit permission index

| Task | Feature trace | Permission | Default actor | Boundary |
|---|---|---|---|---|
| List/search/filter | `FEAT-AUDIT-001..003` | `audit.event.list` | Auditor | Scope before count/query |
| Read detail | `FEAT-AUDIT-001` | `audit.event.read` | Auditor | Safe evidence only; no referenced content |
| Create export | `FEAT-AUDIT-004` | `audit.export.create` | Optional/separate | Bounded, policy/high-impact, async |
| Retrieve export | `FEAT-AUDIT-004` | `audit.export.read` | Optional/separate | Current auth, classification, expiry |
| Offline queue/batch sync/retry | `FEAT-AUDIT-005..007` | System/workload capability | Not a human grant | Do not document as Auditor/Admin write action |
| Retention/archive/purge | `FEAT-AUDIT-008` | Governed workload/policy | Not ordinary Auditor/Admin action | No UI delete; exact authority TBC |

Feature Catalog UI tag `AUDITOR/ADMIN` does not override Document 09. Administrator receives Audit read/export only through explicit separate grant and SoD policy.

## 11.5 Support permission index

| Task | Feature trace | Permission | Content boundary |
|---|---|---|---|
| Issue list/detail | `FEAT-SUPPORT-005` | `support.issue.list/read` | Exact assigned Issue + safe diagnostics |
| Attachment view | `FEAT-SUPPORT-005` | `support.attachment.read` | Exact explicitly sent child only |
| Triage/status | `FEAT-SUPPORT-006` | `support.issue.triage` | Typed transition, current state/version |
| Internal note | `FEAT-SUPPORT-006` | `support.note.read/create` | Restricted Issue-bound note |
| PDF/Excel Support export | `FEAT-SUPPORT-007/008` | `support.export.create/read` | Bounded authorized Issue fields/attachments |

Support tidak mendapat Job permission umum. Job/Correlation/file/attachment/export identifier tidak memperluas Issue scope.

## 11.6 Validation checklist

Untuk setiap prosedur privileged, dokumentasi dan test harus mencakup:

```text
ALLOW
MISSING PERMISSION
WRONG ROLE / PROFILE
WRONG SCOPE / TARGET
STALE OR REVOKED PERMISSION
EXPIRED SESSION
DISABLED ACCOUNT
OFFLINE / CACHED PAGE
REAUTHENTICATION REQUIRED
STALE RESOURCE/POLICY VERSION
DIRECT LINK/API ATTEMPT
NO FORBIDDEN DATA LEAKAGE
```

---

# 12. FAQ

## Mengapa user tidak dapat login?

Periksa status account/eligibility OTO dan safe session metadata sesuai permission. Password, MFA, dan enterprise account recovery dikelola oleh Pertamina Identity, bukan Admin Manual OTO. Jangan meminta password user. Gunakan safe error code/Correlation ID dan eskalasi bila authentication service bermasalah.

## Mengapa fitur tidak muncul?

Ketersediaan dipengaruhi release exposure, platform/device capability, permission, policy, classification, provider, compatibility, resource, dan koneksi. Periksa Feature canonical dan exposure yang sudah ada. Jangan membuat Feature baru atau memaksa direct route.

## Mengapa permission belum berlaku?

Kemungkinan assignment belum current, revisi permission/policy berubah, session/cache stale, scope tidak cocok, account tidak aktif, atau SoD/policy menolak. Refresh melalui jalur resmi dan uji server-side result. Jangan mengedit claim/client cache.

## Mengapa job menunggu?

Gunakan `monitoring.job.read` untuk melihat aggregate/status/reason code aman seperti koneksi, resource, capacity, provider, atau backpressure. Administrator tidak otomatis dapat cancel/retry/read result job pengguna. Eskalasi jika state tidak maju atau health menunjukkan gangguan.

## Mengapa file tidak tersimpan ke akun?

**Amankan ke Akun** memerlukan explicit user action, online authorization, ownership, policy, quota, dan durable server state. Admin hanya melihat safe aggregate/storage health, bukan isi file atau upload token. User flow berada di Document 14.

## Mengapa hanya tiga versi?

Baseline menyimpan maksimal tiga versi terbaru untuk file yang diamankan. Versi keempat mengikuti retention policy. Exact lifecycle harus mengikuti policy resmi; jangan mengubah atau menjanjikan durasi yang belum disetujui.

## Mengapa admin tidak dapat membuka file user?

Administrator mengelola control-plane metadata, bukan document content. Role Admin tidak memberi `OWN`, download content, atau cross-user content grant. Baseline tidak menyediakan break-glass content access.

## Apa bedanya Administrator dan Support?

Administrator mengelola account eligibility, assignment, typed configuration, dan safe monitoring dalam `ADMIN_SCOPE`. Support/Superadmin adalah profil permission terpisah untuk exact Issue scope. Keduanya tidak otomatis saling mewarisi.

## Apa bedanya History, Audit, Log, dan Issue?

- History: aktivitas pribadi user dan dapat dikelola sesuai policy.
- Enterprise Audit: bukti typed/versioned berorientasi append dan tidak dapat diedit/dihapus biasa.
- Technical Log/Observability: health, error, performance, trace, dan dapat di-sample/aggregate.
- Issue: pesan, explicit screenshot/attachment, safe diagnostics, dan Support workflow.

Correlation ID dapat menghubungkan referensi aman, tetapi tidak memberikan akses atau memindahkan content antarlapisan.

## Kapan masalah harus dieskalasikan?

Eskalasi jika ada suspected unauthorized access/disclosure, secret/content leak, data loss/corruption, Audit integrity failure, critical/unhealthy state menurut UI yang disetujui, perubahan target/scope tidak jelas, repeated failure tanpa safe recovery, atau tindakan memerlukan authority yang tidak dimiliki. Untuk pipeline Audit, `UNHEALTHY` adalah state canonical Document 10. Detail operasional berada di Document 16.

## Bagaimana melihat Correlation ID?

Gunakan UI error/detail/monitoring yang sudah diotorisasi. Catat hanya ID, waktu, build, environment, dan safe context yang diperlukan. Correlation ID bukan credential dan tidak memberi akses ke Job, Issue, Audit, file, atau log lain.

---

# 13. Glossary

| Istilah | Definisi untuk Admin Manual |
|---|---|
| Administrator | Role formal untuk control-plane OTO dan safe operational metadata dalam `ADMIN_SCOPE` |
| Auditor | Role formal read-only untuk Enterprise Audit dalam `AUDIT_SCOPE` |
| Support/Superadmin | Profil permission Issue terpisah; bukan role formal atau wildcard |
| Permission | Kode tindakan yang boleh dicoba; bukan keputusan akhir |
| Policy | Pembatas grant berdasarkan safety, classification, scope, resource, dan context |
| Feature Flag | Kontrol rollout/exposure Feature canonical yang sudah ada |
| Feature ID | Identifier canonical capability; tidak diterjemahkan atau dibuat admin |
| Effective Permission | Hasil role/profile grant setelah policy, scope, state, dan context |
| Resource Ownership | Hubungan authority terhadap object; Admin scope metadata bukan ownership content |
| Reauthentication | Fresh authentication untuk tindakan tertentu; bukan elevation |
| Job | Pekerjaan pemrosesan dengan state persisten; ID bukan authority |
| Queue | Antrean bounded untuk pekerjaan; health/length tidak memberi content access |
| Correlation ID | Identifier pelacakan lintas komponen; bukan credential |
| Secured File | Hasil yang secara eksplisit diamankan ke account scope |
| Amankan ke Akun | Selective explicit transfer, bukan full workspace sync |
| Audit Trail | Enterprise evidence typed/versioned, metadata-only, append-oriented |
| User History | Projection aktivitas pribadi; bukan backup atau Enterprise Audit |
| Technical Log | Observability untuk health/error/trace; bukan compliance authority |
| Issue Report | Laporan user dengan pesan, optional explicit attachment, dan safe diagnostics |
| System Health | State/reason/timestamp agregat aman; bukan bukti tidak adanya incident |
| Provider | Engine/service yang disetujui dan dipilih capability/policy |
| Capability | Kemampuan runtime/platform/provider yang tersedia; bukan permission |
| Environment | Dev/QA/UAT/Prod lane yang terpisah dan harus terlihat jelas |
| Delegation Scope | Batas principal/unit/resource yang boleh dikelola Administrator |
| Audit Scope | Batas category/unit/time/environment/evidence yang dapat dibaca Auditor |
| Support Scope | Batas queue/assignment/Issue yang dapat ditangani Support |
| Resource Version / ETag | Versi current untuk mencegah stale mutation |
| Hard Security Invariant | Kontrol wajib yang tidak dapat dimatikan lewat policy/admin UI |
| Maintenance Mode | Typed compatibility/config state; bukan izin bypass security |
| Minimum App Version | Versi client minimum yang diizinkan oleh compatibility policy |
| Rollback | Perubahan balik yang didukung, berizin, dan diverifikasi; bukan asumsi Undo universal |
| Safe Metadata | Data minimum yang disetujui tanpa document content/secret/raw diagnostic |

---

# 14. Cross-reference model

## 14.1 Ownership

| Topik | Primary owner | Admin Manual treatment |
|---|---|---|
| Role, permission, scope, reauth | Document 09 | Ringkas untuk task; link ke source canonical |
| Audit event/access/export/retention | Document 10 | Procedure-facing explanation; tidak mengubah event/policy |
| UAT admin/auditor/support | Document 13 | Release validation dan evidence reference |
| User tool/account/secured flow | Document 14 | Link; jangan menulis ulang User Manual |
| Incident triage/escalation/recovery | Document 16 | Stop condition dan handoff saja |
| Deployment/config secret/topology | Document 17 | Link; jangan memberi deployment steps |
| API/database/source code | Documents 06/07/technical docs | Traceability saja; tidak menjadi admin procedure |

## 14.2 Cross-reference syntax

Gunakan link bermakna:

```text
Lihat “Document 09 — Permission Tampilan Audit” untuk grant dan scope canonical.
Lihat “Document 16 — <approved incident procedure>” jika UI menunjukkan critical/unhealthy state yang memenuhi escalation criteria.
```

Jangan gunakan “klik di sini”. Link harus menyebut tujuan dan tetap masuk akal jika dicetak. Setiap cross-reference mencatat document version yang divalidasi.

## 14.3 Handoff rules

- temuan penggunaan aplikasi biasa → backlog Document 14;
- temuan admin procedure/permission label → Document 15 owner;
- incident, on-call, evidence preservation, operational recovery → Document 16;
- deploy, environment provisioning, secret injection, migration → Document 17;
- role/permission semantic conflict → Integrator + controlled Document 09 revision, bukan keputusan writer;
- Audit event/retention conflict → Integrator + controlled Document 10 revision;
- UI mismatch tanpa architecture change → UI/documentation defect dan screenshot OUTDATED.

---

# 15. Release readiness

## 15.1 Gate

Manual Admin hanya release-ready jika seluruh item berikut lulus pada exact RC:

```text
[ ] menu/navigation final
[ ] role/profile/permission/delegation mapping final
[ ] all TBD_UI_LABEL replaced with exact localized labels
[ ] all released procedures task-tested without verbal help
[ ] sensitive procedures pass allow/deny/wrong-scope/stale/reauth tests
[ ] screenshots APPROVED and tied to exact build/locale/platform
[ ] dummy data and original captures pass security/privacy review
[ ] warnings, impact, rollback, Audit expectations approved
[ ] UAT Administrator/Auditor/Support execution evidence reconciled
[ ] links, TOC, search, permission index, and error reference valid
[ ] known limitations and escalation cross-reference current
[ ] application/manual/build/environment versions recorded
[ ] document owner, reviewer, maintenance route, and publication path recorded
[ ] no documentation Blocker/Critical
```

## 15.2 Current assessment

| Area | Current status | Evidence/gap |
|---|---|---|
| Documents 01–14 | BASELINED | FINAL sources tersedia |
| Role/permission semantics | BASELINED | Document 09 FINAL; Pertamina-specific scope/delegation details pending |
| Audit semantics | BASELINED | Document 10 FINAL; retention/hold/operational values pending |
| UI/menu/labels | NOT READY | No implementation/RC; use `TBD_UI_LABEL` |
| Procedures | DRAFT BASELINE | Belum dieksekusi pada actual UI |
| Screenshots | NOT READY | 0 actual; register `PLANNED` only |
| Security warnings | BASELINED | Harus direview terhadap actual flow |
| UAT execution | NOT READY | Document 13 specification FINAL; execution belum direkam |
| Documentation test | NOT RUN | Memerlukan RC, accounts, permissions, dataset, reviewers |
| Owner/contact/publication | TBC | Memerlukan keputusan resmi |

## 15.3 Status yang benar

```text
FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE
Release readiness: BELUM RELEASE_READY
Validated application build: none / RC-TBD
Actual screenshots: 0
```

`FINAL` berarti struktur/governance baseline dapat dikunci, bukan aplikasi/manual telah lulus UAT atau siap Produksi.

---

# 16. Maintenance triggers

Pembaruan Admin Manual wajib dinilai ketika:

| Trigger | Minimum impact review |
|---|---|
| Admin UI/menu/label berubah | Steps, search terms, screenshots, accessibility text |
| Role/permission/profile berubah | Actor, permission index, negative tests, SoD |
| Delegation/resource scope berubah | Target selection, cross-scope warning, screenshots |
| Policy schema berubah | Typed field, impact, rollback, Audit event |
| Feature Registry/exposure berubah | Feature index, availability, flag procedure |
| Limit/quota/threshold berubah | Wording, validation, warning; no hard-coded obsolete number |
| Job/queue/sync state berubah | Monitoring/troubleshooting/action availability |
| Audit event/schema/access/export berubah | Audit expectation, glossary, procedure validation |
| Issue workflow/status/export berubah | Support sections and Document 16 handoff |
| Monitoring/health/alert berubah | Quick Start, reason codes, escalation trigger |
| Version compatibility berubah | Maintenance/minimum version procedure and rollback |
| Security warning/reauth changes | Callouts, checklists, screenshots, negative tests |
| Escalation route/on-call changes | Cross-reference, owner, contact—not incident content |
| Platform exposure/localization changes | Platform/locale screenshots and labels |
| UAT defect/known issue found | Procedure correction, limitation, retest |
| Release build changes after capture | Screenshot becomes OUTDATED until recaptured/reapproved |

## 16.1 Change workflow

```text
change detected
→ documentation impact record
→ identify affected task/permission/screenshot/link/test
→ update source content
→ role/security/UI review
→ recapture only affected screenshots
→ task-based documentation test
→ UAT/known-issue reconciliation
→ approval + version/release-note update
```

## 16.2 Periodic review

Schedule/frequency resmi tidak dibuat di laporan ini. Content owner harus menetapkan review berbasis release dan policy resmi. Perubahan high-impact tidak menunggu periodic review.

---

# 17. Versioning

## 17.1 Metadata

```text
AdminManualVersion: 1.0
Status: PRE_IMPLEMENTATION_ADMIN_MANUAL_BASELINE
AppliesToAppVersion: RC-TBD
ValidatedBuildDigest: none
Environment: TBC
Platforms: Web primary privileged surface; exact release matrix TBC
Locales: ID source; EN separate translation set pending validation
RoleMatrixVersion: Document 09 FINAL v1.0
AuditDesignVersion: Document 10 FINAL v1.0
UATVersion: Document 13 FINAL v1.0; execution not recorded
UserManualVersion: Document 14 FINAL v1.0 pre-implementation
ContentOwner/Reviewer: TBC
```

## 17.2 Lifecycle

```text
DRAFT_CONTENT
→ WAITING_FOR_UI
→ WAITING_FOR_SCREENSHOT
→ SECURITY_REVIEWED
→ UAT_VALIDATED
→ RELEASE_READY
→ SUPERSEDED / RETIRED
```

Lifecycle content berbeda dari screenshot lifecycle. `FINAL — PRE-IMPLEMENTATION` berada sebelum `RELEASE_READY`.

## 17.3 Change classification

| Change | Manual version impact |
|---|---|
| Typo tanpa behavior change | Patch |
| Label/step/screenshot/permission clarification compatible | Minor/patch sesuai policy |
| Role/permission meaning, procedure outcome, security boundary, or information architecture change | Controlled major/minor decision; impact review mandatory |
| App major version incompatible | Separate applicable manual baseline |

Exact scheme, approval, repository, publication, and retention mengikuti documentation governance yang disetujui; laporan tidak mengarang standar Pertamina.

## 17.4 Translation versioning

Bahasa Indonesia adalah source/fallback. English menjadi translation set terpisah yang mengacu pada source revision. Permission codes, Feature IDs, Job/Correlation/Issue/Error/event/state codes tidak diterjemahkan. UI labels harus berasal dari resource exact locale/build, bukan terjemahan manual bebas.

---

# 18. Risks/gaps

## 18.1 Risk register

| ID | Risk/gap | Dampak | Tindakan/owner |
|---|---|---|---|
| `D15-D-001` | Actual Admin UI/menu/label belum tersedia | Langkah/screenshot dapat salah | Pertahankan `TBD_UI_LABEL`; validate exact RC — UI/Integrator |
| `D15-D-002` | UAT execution belum direkam | Manual belum terbukti usable/safe | Jalankan UAT admin/auditor/support dan documentation QA — UAT owner |
| `D15-D-003` | Exact Support/Admin/Audit organizational scope dan delegation belum disetujui | Assignment/scope procedure tidak dapat final | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Security/Business |
| `D15-D-004` | Addendum API untuk management/session-revocation belum selesai sebelum implementation | Procedure `FEAT-ADMIN-001..003` dapat tidak punya contract aktual | Controlled Document 06 addendum — API/Integrator |
| `D15-D-005` | Feature Catalog menandai `FEAT-AUDIT-*` sebagai UI `AUDITOR/ADMIN`, sedangkan Role Matrix memberi Admin hanya optional separate Audit grant | Manual dapat menyiratkan implicit Audit access | Document 09 tetap authority; tulis grant terpisah/SoD; Integrator pertahankan normalisasi |
| `D15-D-006` | `FEAT-AUDIT-005..008` terlihat sebagai UI capability, tetapi queue/sync/retry/lifecycle adalah system/workload path | Auditor/Admin dapat dikira memiliki Audit write/retention mutation | Jangan jadikan human procedure; controlled UI/capability clarification — Integrator |
| `D15-D-007` | Retention duration, clock, hold authority, purge, archive, RPO/RTO belum disetujui | Retention procedure berisiko salah/compliance claim | Produksi purge tetap disabled; confirm policy — Pertamina authority |
| `D15-D-008` | Reauth method/freshness/attempt policy belum final | Steps dan screenshot belum dapat disahkan | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — IAM/Security |
| `D15-D-009` | Exact alert route/on-call/escalation belum tersedia | Troubleshooting dapat berakhir dead-end | Document 16 menentukan operasional route; manual hanya cross-reference |
| `D15-D-010` | Admin dapat salah dianggap mempunyai user document access | Privacy/authorization breach | Repeated boundary, negative tests, no content screenshots — Security/Docs |
| `D15-D-011` | Monitoring data dapat dianggap source of truth atau Job-control grant | False health/unsafe action | Show freshness/reason; REST/state authority; no undocumented mutation |
| `D15-D-012` | Screenshot privileged surface berisiko identity/Issue/Audit/prod config leak | Sensitive disclosure | Synthetic QA/UAT only + security review + controlled storage |
| `D15-D-013` | Rollback tidak selalu tersedia | Manual dapat menjanjikan recovery palsu | Setiap procedure menyatakan supported rollback/no rollback; test exact RC |
| `D15-D-014` | Owner/publication/search/offline format belum ditetapkan | Manual cepat usang/tidak ditemukan | Confirm documentation governance and owner |
| `D15-D-015` | Admin Manual dapat mengambil alih Incident/Deployment Guide | Duplikasi dan prosedur konflik | Gunakan stop condition + stable cross-reference only |

## 18.2 Inconsistency audit

- **Role naming:** Document 02/03 memakai label Superadmin pada Support Console; Document 09 menormalkan sebagai Support permission profile. Admin Manual wajib mengikuti Document 09 dan menjelaskan label UI tersebut.
- **Audit UI tag:** `AUDITOR/ADMIN` pada Feature Catalog adalah exposure hint, bukan default role grant. Permission table Document 09 tetap berwenang.
- **Audit lifecycle:** Offline queue/batch/retry/retention adalah capability sistem. Auditor read-only dan tidak memperoleh write/delete.
- **Admin vs Audit verification:** Prompt Quick Start menyebut “gunakan audit untuk verifikasi”; baseline mengharuskan Audit grant terpisah atau handoff kepada Auditor.
- **Admin vs Support Issue:** Prompt Quick Start menyebut memeriksa issue penting; baseline mengharuskan Support permission terpisah dan exact scope.
- **Job management:** Admin mempunyai monitoring aggregate, bukan generic job cancel/retry/download permission.
- **Status dokumen:** Document 14 dan UAT belum release-executed. Document 15 harus `FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE`, bukan release-ready.

Tidak ada Feature ID, role, permission, endpoint, database, vendor, atau UI requirement baru dibuat dalam laporan ini. Temuan semantic conflict harus dilaporkan ke Integrator dan tidak diselesaikan dengan mengubah source document secara diam-diam.

---

# 19. Suggested Document 15 text

Teks berikut dapat diintegrasikan ke master Document 15 setelah review Integrator. Label, screenshot, exact platform, owner, dan release values tetap menunggu RC/confirmation.

## 19.1 Status dan tujuan

> **Status:** `FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE`  
> **Release readiness:** `BELUM RELEASE_READY`  
> **Applies to application:** `RC-TBD`

Admin Manual membantu actor privileged melakukan tugas control-plane PERTAMINA OTO secara aman, singkat, dan dapat diverifikasi. Manual ini tidak memberikan permission. Setiap tindakan tetap diperiksa oleh authentication, permission, delegated scope, policy, resource state/version, compatibility, confirmation, dan reauthentication yang berlaku.

## 19.2 Batas role

Role formal adalah User, Administrator, dan Auditor. Support/Superadmin adalah profil permission terpisah. Administrator tidak otomatis menjadi Auditor atau Support dan tidak otomatis dapat membuka isi dokumen, secured file, hasil job, Issue, atau Enterprise Audit pengguna lain. Auditor read-only. Support hanya mengakses Issue dan attachment yang secara eksplisit berada dalam scope-nya.

## 19.3 Mulai Administrasi dalam 10 Menit

1. Login dengan account pribadi yang disetujui.
2. Pastikan environment dan build yang benar.
3. Buka **`TBD_UI_LABEL: System Health`** jika memiliki `monitoring.system.read`.
4. Periksa Issue penting hanya jika memiliki Support profile yang sesuai.
5. Buka **`TBD_UI_LABEL: Job & Queue Monitoring`** jika memiliki `monitoring.job.read`.
6. Kelola account hanya jika ada kebutuhan dan scope yang terverifikasi.
7. Periksa current policy/configuration version sebelum perubahan.
8. Minta Auditor atau pemegang Audit grant terpisah memverifikasi event perubahan bila Anda tidak memiliki `audit.event.read`.
9. Hentikan perubahan dan gunakan Document 16 jika ada incident, data loss, unauthorized access, secret leak, atau critical/unhealthy state menurut UI yang disetujui.

## 19.4 Standard prosedur

Setiap prosedur menyatakan tujuan, actor, permission, resource scope, environment, risk, connectivity, reauthentication, Audit expectation, prasyarat, numbered steps, expected/forbidden result, verification, rollback, troubleshooting, escalation, traceability, dan last validated build.

Sebelum tindakan sensitif, pastikan permission, environment, exact target, current version, reason, impact, SoD/approval, serta rollback. Setelah tindakan, verifikasi authoritative result, Audit event oleh reader yang berizin, notification, safe evidence, dan issue/task/change record.

## 19.5 Warning

Gunakan label teks **Catatan**, **Perhatian**, **Tindakan berisiko**, **Memerlukan permission**, **Dapat dibatalkan**, **Dicatat dalam audit**, **Memerlukan koneksi**, dan **Eskalasi diperlukan**. Emoji hanya drafting aid dan tidak boleh menjadi satu-satunya pembeda.

## 19.6 Account dan access

Administrator dapat membaca metadata account/session yang aman dan mengubah eligibility atau mencabut session dalam delegated scope. Mutasi memerlukan exact target, current state/version, reauthentication, SoD sesuai policy, confirmation, dan Audit. OTO tidak mengelola password, MFA, atau enterprise identity recovery.

Role/permission assignment memakai kode canonical, tanpa wildcard, self-elevation, role hierarchy, atau implicit `ALL`. Reauthentication tidak menambah permission.

## 19.7 Feature, policy, dan compatibility

Administrator hanya dapat mengubah typed policy, existing Feature exposure/flag, limit dalam hard ceiling, maintenance, dan minimum client version sesuai permission. Admin tidak dapat membuat Feature baru, mengubah makna Feature ID, atau mematikan hard security invariant, Audit wajib, no-secret logging, Assistant boundary, public-AI prohibition, atau non-PKI Sign scope.

Perubahan high-impact memerlukan version check, impact review, staged scope, safe rollback yang terbukti, reauthentication, Audit, dan post-change monitoring.

## 19.8 Jobs, storage, dan monitoring

`monitoring.system.read` dan `monitoring.job.read` hanya memberi safe aggregate metadata dalam `ADMIN_SCOPE`. Monitoring tidak mengotorisasi akses document content, result download, secured-file content, atau generic cancel/retry job pengguna. Correlation ID membantu pelacakan tetapi bukan credential atau authority.

## 19.9 Support

Support Console hanya tersedia bagi profil permission Support yang disetujui. Issue list/detail, explicit attachment, triage, restricted note, dan bounded export diotorisasi per tindakan dan exact Issue scope. Support tidak menerima general access ke user document, secured file, OCR, Assistant conversation, password, token, clipboard, signature, raw storage, atau unrestricted log.

## 19.10 Audit

Enterprise Audit terpisah dari User History, Observability, dan Issue. Auditor memakai `audit.event.list/read` secara read-only dalam `AUDIT_SCOPE`; export memerlukan permission terpisah. Administrator hanya mendapat akses melalui separate Audit grant dan SoD. Tidak ada human permission untuk edit/delete source Audit event. Queue, sync, retry, retention, archive, dan purge adalah governed system/workload capability, bukan Auditor write action.

## 19.11 Screenshot dan evidence

Screenshot hanya dari exact RC pada controlled QA/UAT dengan dummy user, job, Issue, Correlation ID, dan file metadata. Password, token, secret, real identity/content/Issue/Audit/signature, serta production connection information dilarang. Screenshot wajib melalui `PLANNED → CAPTURED → REVIEWED → SECURITY_REVIEWED → APPROVED`; `OUTDATED` tidak boleh tampil dalam manual aktif.

## 19.12 Documentation test

Manual diuji melalui Role Matrix Check, Security Check, UAT Cross-Check, UI Label Check, Permission Check, Screenshot Check, Link Check, Task-Based Review, dan New Administrator Review. Administrator baru harus dapat menjalankan prosedur dasar tanpa instruksi verbal. Missing permission, wrong scope, stale/revoked state, offline, reauthentication, direct route, dan no-data-leakage wajib diuji.

## 19.13 Release gate

Admin Manual baru `RELEASE_READY` setelah menu/permission final, seluruh langkah diuji, screenshot disetujui, dummy data aman, warnings disetujui, UAT feedback diselesaikan, links valid, limitations current, build version tercatat, serta owner/reviewer/publication route dikonfirmasi. Sampai itu, status tetap `FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE`.

## 19.14 Maintenance

Admin UI, role/permission, policy, Feature Registry, limit, job state, Audit event, Issue workflow, monitoring, compatibility, security warning, atau escalation route memicu documentation impact review. Perubahan memperbarui hanya prosedur/screenshot/test yang terdampak, tetapi selalu menguji ulang permission dan security boundary.

---

**Serah terima kepada Integrator:** pertahankan Document 09 sebagai authority saat tag UI Feature Catalog tampak lebih luas; konfirmasikan delegation/Support/Audit scope, SoD, reauthentication, retention/hold, alert/escalation route, actual UI labels, platform exposure, documentation owner, screenshot storage/retention, dan publication/version policy. Sebelum implementasi prosedur `FEAT-ADMIN-001..003` serta session revocation, selesaikan controlled API addendum yang sudah dicatat Document 09. Jangan mengubah role, permission, Feature ID, Audit lifecycle, atau manual status hanya untuk menyesuaikan UI draft.
