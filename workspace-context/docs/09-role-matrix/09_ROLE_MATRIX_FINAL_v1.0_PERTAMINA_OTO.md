# 09 — MATRIKS PERAN
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Versi:** 1.0  
> **Tanggal:** 26 Agustus 2026  
> **Status:** FINAL — PRE-CODING BASELINE  
> **Keputusan peran:** 18/18 AUTO-LOCKED  
> **Peran formal:** 3  
> **Baseline fitur canonical:** 550  
> **Prioritas penyampaian utama:** 33 Primary PDF Tool Cards

---

# 1. Tujuan

Dokumen ini menetapkan **siapa yang boleh mencoba tindakan PERTAMINA OTO tertentu, terhadap sumber daya apa, serta berdasarkan policy dan konteks apa**. Dokumen ini menjadi baseline pre-coding berwenang untuk Role, Permission, Resource Scope, dan pengujian otorisasi.

Matriks wajib tetap:

- benar secara bisnis dan mudah dipahami;
- berbasis permission, deny-by-default, dan least-privilege;
- praktis untuk ASP.NET Core, WinUI 3, .NET MAUI, Worker, dan pengujian otomatis;
- kompatibel dengan Offline-First dan Local-First yang memperhatikan capability;
- konsisten pada UI, API, SignalR, Worker, dan Assistant;
- terpisah dari desain peristiwa Audit terperinci dalam Document 10.

---

# 2. Ruang Lingkup

## 2.1 Termasuk dalam Ruang Lingkup

- human role formal dan profil Support/Superadmin;
- kode permission canonical dan pemetaan role;
- gate policy, ownership, scope eksplisit, resource state, dan konteks;
- matriks tindakan, sumber daya, platform, sensitivitas, luring, dan reauthentication;
- pemetaan keluarga Feature dan 33 Core Tool;
- pemisahan identity manusia dan workload;
- enforcement UI/API/SignalR/Worker/Assistant;
- ketertelusuran dan pengujian penerimaan otorisasi.

## 2.2 Di Luar Ruang Lingkup

- memilih vendor IAM/SSO, profil protokol, atau metode MFA;
- menambahkan role formal baru atau akses privileged terhadap dokumen pengguna;
- menciptakan route API yang tidak ada dalam Document 06;
- mengubah persistensi Document 07 secara diam-diam;
- mengimplementasikan account root, wildcard Superadmin, impersonation, atau break-glass content;
- menetapkan field peristiwa Audit, integritas, retensi legal, alerting, atau integrasi SIEM;
- coding aplikasi.

## 2.3 Otoritas

Documents 01–08 FINAL tetap menjadi otoritas yang lebih tinggi. Delapan belas keputusan terintegrasi dicatat dalam `ROLE_PERMISSION_DECISION_REGISTER.md`; bukti integrasi berada dalam `ROLE_MATRIX_INTEGRATION_AUDIT.md`.

---

# 3. Model Peran

Role adalah kumpulan permission stabil yang umum diperlukan oleh suatu persona bisnis. Role bukan hasil otorisasi akhir, relasi kepemilikan, clearance klasifikasi data, resource scope, Feature flag, state daring/luring, atau hasil reauthentication.

```text
Formal role
→ effective permission set
→ policy restrictions
→ concrete resource ownership/scope
→ current context/state/reauthentication
→ ALLOW or DENY
```

Role formal hanya:

```text
User
Administrator
Auditor
```

Support/Superadmin adalah profil permission yang diberikan terpisah, bukan role formal.

---

# 4. Model Izin

Permission adalah satu kategori tindakan stabil terhadap satu sumber daya logis. Permission mengizinkan subjek untuk **mencoba** suatu tindakan. Permission tidak membuktikan ownership, scope, kelayakan policy, resource state, capability platform, consent, atau freshness autentikasi.

Setiap definisi permission memuat:

| Field | Aturan |
|---|---|
| Code | `domain.resource.action` yang stabil |
| Subject type | Manusia atau workload; tidak pernah keduanya kecuali dinyatakan eksplisit |
| Resource | Objek logis yang dilindungi oleh tindakan |
| Default scope | Scope own, reporter, admin, support, audit, atau workload job |
| Sensitivity | Normal, elevated, sensitive, destructive, atau bulk disclosure |
| Connectivity | Layak luring atau hanya daring |
| Reauthentication | Tidak ada, wajib, atau kondisional berdasarkan policy |
| Content access | YES/NO eksplisit; tidak pernah diimplikasikan oleh akses metadata |
| Feature trace | Kelompok/ID Feature canonical bila berlaku |
| Contract trace | Operasi API/aplikasi bila berlaku |
| Version/status | Active/deprecated/replacement; makna tidak pernah diubah langsung |

---

# 5. Model Kebijakan

Policy membatasi permission yang sudah ada. Policy tidak dapat menciptakan grant yang belum ada, mengubah makna Feature canonical, atau memperlemah hard security invariant.

Urutan prioritas:

```text
1. Hard system safety and security invariant
2. Enterprise security and classification policy
3. Resource ownership, state, retention, and legal hold
4. Organizational/delegation scope
5. Environment, platform, client, capability, and provider state
6. Operational safety, quota, capacity, and backpressure
7. User preference
```

Setiap kegagalan wajib atau deny menghentikan tindakan. Administrator tidak dapat menimpa lapisan keselamatan yang lebih tinggi.

---

# 6. Kepemilikan Sumber Daya

Ownership diturunkan dari `PrincipalId` internal yang terautentikasi dan metadata sumber daya berwenang, bukan dari owner, email, display name, role, scope, atau unit yang dikirim klien.

| Relasi | Sumber daya | Arti |
|---|---|---|
| `OWN` | Workspace, artefak lokal, job/result, history, secured file/version, preference | Principal saat ini adalah owner yang dipersistenkan |
| `REPORTER` | Issue dan lampiran yang terlihat reporter | Principal saat ini membuat Issue tersebut |
| `ADMIN_SCOPE` | Metadata account/session/configuration/health | Target berada dalam scope administratif terdelegasi; isi dikecualikan |
| `SUPPORT_SCOPE` | Issue/detail/attachment/note/export | Issue berada dalam scope antrean/assignment yang disetujui |
| `AUDIT_SCOPE` | Audit event/query/export | Bukti berada dalam scope unit/waktu/category/platform yang disetujui |
| `WORKLOAD_JOB_SCOPE` | Operasi Job/artefak/objek/provider | Workload memiliki lease/tujuan valid untuk Job tepat tersebut |
| `EXPLICIT_RESOURCE_GRANT` | Hanya untuk isi terdelegasi/tim pada masa depan | Tidak tersedia dalam scope saat ini |

Aturan:

- scope diterapkan sebelum count, filter, search, sort, cursor, atau projection;
- otorisasi parent dan keanggotaan child sama-sama diverifikasi;
- content/storage hanya diselesaikan setelah otorisasi khusus isi;
- Job ID, Correlation ID, `fileRef`, ETag, path, object key, link, cursor, atau group SignalR tidak pernah menjadi kewenangan;
- isi lintas pengguna ditolak secara default.

---

# 7. Otorisasi Berbasis Konteks

```text
ALLOW = AuthenticatedOrEligibleOfflineGrant
    AND AccountActive
    AND SessionOrGrantValid
    AND ActionPermission
    AND SubjectTypeAllowed
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND ClientCompatibilityAndCapability
    AND ConnectivityAllows
    AND RequiredConsentAndConfirmation
    AND FreshReauthenticationWhenRequired
```

Konteks mencakup session/revocation state saat ini, revisi permission/policy, daring/luring, platform, versi app/API/contract, capability, classification, resource state, consent, confirmation, reauthentication, environment, quota, capacity, serta lease/tujuan workload.

Input keamanan yang tidak dikenal, stale, hilang, tidak kompatibel, atau tidak tersedia bersifat fail-closed.

---

# 8. Peran Formal

| Role | Tujuan bisnis | Resource scope default | Batas eksplisit |
|---|---|---|---|
| `User` | Menggunakan office tool yang disetujui dan mengelola pekerjaan/sumber daya sendiri | `OWN`, `REPORTER` | Tanpa kewenangan administrasi, Support, sumber daya lintas pengguna, atau Enterprise Audit |
| `Administrator` | Mengelola control plane OTO dan metadata operasi aman | `ADMIN_SCOPE` | Tanpa kewenangan User, Auditor, Support, atau isi dokumen secara otomatis |
| `Auditor` | Membaca bukti Enterprise Audit aman yang disetujui | `AUDIT_SCOPE` | Tanpa mutasi configuration, resource, Issue, atau Audit; tanpa isi dokumen |

Kode role stabil dan tidak dilokalkan. Display name dapat dilokalkan. Hierarki role tidak tersedia.

---

# 9. Izin Pengguna

Role `User`, sesuai policy dan ownership, dapat:

- membaca profil sendiri yang aman dan mengelola preference allowlisted;
- menemukan dan menjalankan tool canonical yang disetujui untuk pengguna;
- mengelola workspace lokal, draft, result, recovery, dan Riwayat Pribadi sendiri;
- membaca/mengontrol job sendiri dan mengambil hasil sendiri;
- membuat/membaca/membuat version/restore/delete secured file sendiri;
- mengirim Issue, mengirim lampiran secara eksplisit, dan membaca status Issue sendiri;
- menggunakan Assistant sebagai antarmuka ke Feature berwenang yang sama;
- menerima notifikasi Activity sendiri yang aman;
- menggunakan tindakan lokal deterministik yang layak berdasarkan offline grant valid.

User tidak dapat mengelola principal lain, role, permission, policy, enterprise configuration, Audit wajib, antrean Support, atau sumber daya pengguna lain.

---

# 10. Izin Administrator

Role `Administrator` dapat menerima kewenangan granular untuk:

- membuat daftar/membaca metadata account OTO dan kelayakan aplikasi yang aman;
- mengaktifkan/menonaktifkan kelayakan dan mencabut session aplikasi dalam scope terdelegasi;
- membaca definisi role/permission canonical dan assign/revoke entri yang disetujui;
- membaca/meng-update typed policy, exposure/flag Feature, limit, maintenance, compatibility/minimum version, retention, classification, dan security-profile configuration;
- membaca metadata agregat job, queue, sync, error, health, version, dan storage yang aman.

Guard wajib:

- otorisasi daring saat ini;
- tanpa self-assignment atau grant di luar kewenangan terdelegasi;
- evaluasi segregation of duties;
- fresh reauthentication untuk tindakan privileged/sistemik;
- optimistic concurrency dan versi policy saat ini;
- hard safety ceiling tidak dapat diperlemah;
- seluruh isi yang tercantum pada bagian 12 tetap dikecualikan.

Administrasi OTO tidak mengelola password Pertamina, MFA, enterprise account recovery, atau siklus hidup kredensial lain milik IdP.

---

# 11. Izin Auditor

Role `Auditor` menerima:

- `audit.event.list` dan `audit.event.read` dalam `AUDIT_SCOPE` yang ditetapkan;
- pencarian terfilter melalui field allowlisted tanpa perluasan scope;
- bukti read-only atas peristiwa authentication, processing, security, role, permission, configuration, dan privileged yang disetujui;
- ekspor Audit terbatas yang opsional dan diberikan terpisah.

Auditor tidak dapat:

- meng-update/delete bukti sumber Audit;
- mengubah retention, legal hold, policy, configuration, role, permission, Issue, job, atau data pengguna;
- menggunakan referensi peristiwa sebagai pivot ke isi document/result/secured-file/Issue;
- menggunakan ekspor untuk melampaui scope event/field/time/unit.

Ingestion Audit, antrean luring, sinkronisasi, dan retry adalah capability sistem, bukan permission write Auditor.

---

# 12. Capability Superadmin / Support

“Superadmin” adalah label UI untuk profil permission Support yang disetujui. Superadmin bukan role formal keempat dan bukan hierarki di atas Administrator.

Profil memisahkan:

| Capability | Permission | Batas isi |
|---|---|---|
| Koleksi/detail Issue | `support.issue.list`, `support.issue.read` | Issue assigned/sesuai scope dan diagnostik aman |
| Lampiran eksplisit | `support.attachment.read` | Lampiran tepat yang sengaja dikirim reporter |
| Triage/status | `support.issue.triage` | Transisi state typed yang diizinkan + versi saat ini |
| Catatan internal | `support.note.read`, `support.note.create` | Hanya catatan terbatas yang terikat Issue |
| Ekspor PDF/Excel | `support.export.create`, `support.export.read` | Field/lampiran berwenang dalam scope terbatas |

Support tidak menerima dokumen asli pengguna, input/result job yang tidak terkait, isi secured-file, OCR lengkap, percakapan Assistant, password, token, clipboard, aset tanda tangan, raw storage, log tanpa batas, atau “view as user” arbitrer.

Seluruh operasi Support hanya daring. Scope queue/unit/region/assignment tepat berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 13. Identity Service dan Internal

Identity Service/Worker bersifat non-manusia, spesifik environment, memiliki scope tujuan/audience, berumur pendek/dapat dirotasi, dan terbatas pada queue, Job, objek, modul, atau provider tepat.

Identity tersebut:

- tidak pernah menerima assignment `User`, `Administrator`, `Auditor`, atau Support;
- tidak pernah menggunakan session interaktif/luring;
- tidak pernah melakukan impersonation terhadap manusia;
- tidak pernah menerima access/refresh token end-user yang diteruskan;
- tidak dapat menjelajahi Job, storage, modul database, atau operasi provider yang tidak terkait.

Katalog permission manusia dan workload divalidasi agar subject type-nya tidak dapat dipertukarkan.

---

# 14. Permission Feature

Feature ID dan permission memiliki tanggung jawab berbeda:

| Kontrol | Tanggung jawab |
|---|---|
| Feature ID | Capability, contract/schema, handler, exposure, platform, status, risk, dan ketertelusuran yang stabil |
| Permission tindakan | Tindakan bisnis/keamanan yang boleh dicoba subjek |
| Policy | Pembatasan classification, provider, rollout, limit, transfer, dan keselamatan |
| Aturan sumber daya | Owner/reporter/scope eksplisit dan state saat ini |
| Capability | Ketersediaan engine/platform/provider yang disetujui |

Transformasi biasa menggunakan `document.content.process`. Eksekusi server juga menggunakan `document.server.execute`. Tindakan Protect, Unlock, visual Sign, Redact, Intelligence, share, overwrite, penyimpanan terlindungi, dan destruktif menambahkan permission khusus masing-masing.

Kontrol sistem, validasi, security scanning, cleanup, pengiriman Audit, pengumpulan monitoring, dan operasi Worker bukan grant eksekusi Feature untuk manusia.

---

# 15. Katalog Izin Tindakan

## 15.1 Izin Manusia

| Kode permission | Arti | Scope default | Daring | Reauth | Audit wajib |
|---|---|---|:---:|:---:|:---:|
| `account.profile.read` | Read own safe account profile | `OWN` | No | No | Conditional |
| `account.preference.read` | Read own allowlisted preferences | `OWN` | No | No | No |
| `account.preference.update` | Update own allowlisted preferences | `OWN` | Sync only | No | Conditional |
| `assistant.session.use` | Use the Assistant interface/orchestrator | `OWN` | No | No | Conditional |
| `document.content.process` | Execute ordinary approved transformation | Authorized input + own result | Route dependent | No by default | Yes |
| `document.server.execute` | Transfer content and execute approved server route | `OWN` | Yes | Policy | Yes |
| `document.pdf.protect` | Create a protected PDF result | `OWN` | No | Policy | Yes |
| `document.pdf.unlock` | Create an unlocked result with valid authority/password | `OWN` | No | Policy | Yes |
| `document.signature.apply` | Apply/finalize visual non-PKI signature/paraf | `OWN` | No | Policy | Yes |
| `document.redaction.finalize` | Finalize verified irreversible redaction | `OWN` | No | Policy | Yes |
| `document.intelligence.execute` | Use approved local/internal content intelligence | `OWN` | Provider dependent | Policy | Yes |
| `document.result.share` | Disclose selected result to an approved target | `OWN` | Target dependent | Policy | Yes |
| `local.file.overwrite` | Replace exact selected local file | `OWN` | No | No by default | Yes |
| `local.data.delete` | Delete own app-managed local data/recycle item | `OWN` | No | No by default | Conditional |
| `workspace.state.read` | Read own local workspace/draft/recovery | `OWN` | No | No | No |
| `workspace.state.update` | Update own local workspace/draft/recovery | `OWN` | No | No | Conditional |
| `workspace.state.delete` | Delete own app-managed workspace state | `OWN` | No | No by default | Conditional |
| `jobs.job.list` | List own authorized jobs | `OWN` | Yes for server | No | No |
| `jobs.job.read` | Read own job state/safe detail | `OWN` | Yes for server | No | Conditional |
| `jobs.job.cancel` | Cancel own cancellable job | `OWN` | Route dependent | No | Yes |
| `jobs.job.retry` | Retry own eligible job | `OWN` | Route dependent | No | Yes |
| `jobs.job.resume` | Resume own compatible checkpoint | `OWN` | Route dependent | No | Yes |
| `jobs.result.read` | Read own result metadata | `OWN` | Yes for server | No | Conditional |
| `jobs.result.download` | Retrieve own result bytes | `OWN` | Yes for server | Policy | Yes |
| `history.entry.list` | List own Personal History | `OWN` | Local/server | No | No |
| `history.entry.read` | Read own Personal History detail | `OWN` | Local/server | No | No |
| `history.entry.delete` | Remove own Personal History projection | `OWN` | Route dependent | No | Conditional |
| `history.entry.reprocess` | Reinvoke current approved Feature from own history | `OWN` | Route dependent | Per Feature | Yes |
| `secured.file.create` | Amankan ke Akun from explicit validated snapshot | `OWN` | Yes | Policy | Yes |
| `secured.file.list` | List own secured-file metadata | `OWN` | Yes | No | No |
| `secured.file.read` | Read own secured-file metadata | `OWN` | Yes | No | Conditional |
| `secured.file.delete` | Move own secured file to account recycle | `OWN` | Yes | No by default | Yes |
| `secured.file.restore` | Restore own secured file from recycle | `OWN` | Yes | Policy | Yes |
| `secured.file.purge` | Permanently delete eligible secured content | `OWN` | Yes | Required | Yes |
| `secured.version.create` | Add a version to own secured file | `OWN` | Yes | Policy | Yes |
| `secured.version.list` | List own secured versions | `OWN` | Yes | No | No |
| `secured.version.read` | Read own secured-version metadata | `OWN` | Yes | No | Conditional |
| `secured.version.download` | Download exact own secured version | `OWN` | Yes | Policy | Yes |
| `secured.version.restore` | Restore exact own secured version | `OWN` | Yes | Policy | Yes |
| `issue.report.create` | Submit own minimal Issue | `REPORTER` | Yes | No | Yes |
| `issue.report.list` | List own Issues | `REPORTER` | Yes | No | No |
| `issue.report.read` | Read own simple Issue status/detail | `REPORTER` | Yes | No | Conditional |
| `issue.attachment.create` | Explicitly send previewed own attachment | `REPORTER` | Yes | No | Yes |
| `issue.attachment.read` | Read exact attachment on own Issue | `REPORTER` | Yes | No | Conditional |
| `realtime.activity.connect` | Connect to own safe activity channel | `OWN` | Yes | No | No |
| `realtime.job.subscribe` | Subscribe to own authorized Job notification | `OWN` | Yes | No | No |
| `admin.user.list` | List safe account metadata | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.user.read` | Read safe account metadata | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.user.update` | Change OTO eligibility within scope | `ADMIN_SCOPE` | Yes | Required | Yes |
| `admin.session.read` | Read safe application-session metadata | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.session.revoke` | Revoke another application session | `ADMIN_SCOPE` | Yes | Required | Yes |
| `admin.role.list` | List canonical roles | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.role.read` | Read role definitions/assignments | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.role.assign` | Assign approved role within delegation | `ADMIN_SCOPE` | Yes | Required | Yes |
| `admin.role.revoke` | Revoke role within delegation | `ADMIN_SCOPE` | Yes | Required | Yes |
| `admin.permission.list` | List canonical permissions | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.permission.read` | Read permission definitions/assignments | `ADMIN_SCOPE` | Yes | No | Conditional |
| `admin.permission.assign` | Assign approved permission/profile | `ADMIN_SCOPE` | Yes | Required | Yes |
| `admin.permission.revoke` | Revoke approved permission/profile | `ADMIN_SCOPE` | Yes | Required | Yes |
| `configuration.policy.read` | Read safe typed policy metadata | `ADMIN_SCOPE` | Yes | No | Conditional |
| `configuration.policy.update` | Update approved typed policy | `ADMIN_SCOPE` | Yes | Required/high-impact | Yes |
| `configuration.feature.read` | Read Feature exposure/flag metadata | `ADMIN_SCOPE` | Yes | No | Conditional |
| `configuration.feature.update` | Update existing Feature exposure/flag | `ADMIN_SCOPE` | Yes | High-impact policy | Yes |
| `configuration.limit.read` | Read typed limits/quotas | `ADMIN_SCOPE` | Yes | No | Conditional |
| `configuration.limit.update` | Update limit within hard ceiling | `ADMIN_SCOPE` | Yes | High-impact policy | Yes |
| `configuration.compatibility.read` | Read maintenance/minimum-version policy | `ADMIN_SCOPE` | Yes | No | Conditional |
| `configuration.compatibility.update` | Update maintenance/minimum-version policy | `ADMIN_SCOPE` | Yes | High-impact policy | Yes |
| `monitoring.system.read` | Read safe health/error/storage/version aggregate | `ADMIN_SCOPE` | Yes | No | Conditional |
| `monitoring.job.read` | Read safe job/queue/sync aggregate | `ADMIN_SCOPE` | Yes | No | Conditional |
| `audit.event.list` | Query scoped safe Audit collection | `AUDIT_SCOPE` | Yes | No | Yes |
| `audit.event.read` | Read scoped safe Audit evidence | `AUDIT_SCOPE` | Yes | No | Yes |
| `audit.export.create` | Create bounded Audit export | `AUDIT_SCOPE` | Yes | Policy/high-impact | Yes |
| `audit.export.read` | Retrieve authorized expiring Audit export | `AUDIT_SCOPE` | Yes | Policy/high-impact | Yes |
| `support.issue.list` | List Issues in assigned Support scope | `SUPPORT_SCOPE` | Yes | No | Yes |
| `support.issue.read` | Read Issue message and safe diagnostics | `SUPPORT_SCOPE` | Yes | No | Yes |
| `support.attachment.read` | Read exact explicitly sent attachment | `SUPPORT_SCOPE` | Yes | Policy/high-impact | Yes |
| `support.issue.triage` | Perform typed triage/status transition | `SUPPORT_SCOPE` | Yes | Policy/high-impact | Yes |
| `support.note.read` | Read restricted internal Support note | `SUPPORT_SCOPE` | Yes | No | Yes |
| `support.note.create` | Add restricted internal Support note | `SUPPORT_SCOPE` | Yes | No | Yes |
| `support.export.create` | Create bounded PDF/Excel Support export | `SUPPORT_SCOPE` | Yes | Policy/high-impact | Yes |
| `support.export.read` | Retrieve authorized expiring Support export | `SUPPORT_SCOPE` | Yes | Policy/high-impact | Yes |

## 15.2 Izin Workload

| Kode permission | Subjek | Scope | Dapat diberikan kepada manusia |
|---|---|---|:---:|
| `jobs.worker.claim` | Server Worker | Approved queue/workload class | No |
| `jobs.worker.update` | Local/Server Worker | Exact leased Job/operation | No |
| `storage.object.read` | Storage adapter/Worker | Exact job/version object | No |
| `storage.object.write` | Storage adapter/Worker | Exact staging/final object | No |
| `audit.event.ingest` | Audit ingestion service | Validated batch/source scope | No |
| `provider.ai.invoke` | Approved internal provider adapter | Exact approved request/purpose | No |
| `realtime.event.publish` | Application/outbox publisher | Safe post-commit event | No |

Permission wildcard tidak tersedia.

---

# 16. Izin Akun

| Tindakan | Permission | Role/profile | Aturan sumber daya | Guard utama |
|---|---|---|---|---|
| Read own safe profile | `account.profile.read` | User | `OWN` | No token/raw claim |
| Read/update preference | `account.preference.read`, `account.preference.update` | User | `OWN` | Allowlisted typed fields; enterprise policy wins |
| Read account metadata | `admin.user.list`, `admin.user.read` | Administrator | `ADMIN_SCOPE` | Safe metadata only |
| Enable/disable OTO eligibility | `admin.user.update` | Administrator | `ADMIN_SCOPE` | Online, reauth, SoD, current version |
| Read application session metadata | `admin.session.read` | Administrator | `ADMIN_SCOPE` | No credential/cookie/token |
| Revoke another session | `admin.session.revoke` | Administrator | `ADMIN_SCOPE` | Online, reauth, idempotent, audited |

Pertamina Identity tetap berwenang atas password, MFA, pemulihan kredensial enterprise, dan siklus hidup identity eksternal.

---

# 17. Izin Workspace

Akses workspace/draft/preview/undo/recovery lokal menggunakan `workspace.state.read`, `workspace.state.update`, `workspace.state.delete`, dan binding profile `OWN`.

- Profile OS + profile subjek Pertamina + root yang dikelola aplikasi membentuk batas ownership.
- Pergantian account melepaskan profile sebelumnya; data dan grant tidak pernah digabungkan.
- File sumber tetap menjadi sumber daya pilihan pengguna dan tidak pernah di-overwrite secara default.
- Local Worker hanya menerima referensi tepat sesuai scope job melalui typed IPC terautentikasi.
- Penghapusan workspace tidak pernah menghapus Enterprise Audit, secured file, Job server, atau file sumber yang tidak terkait.

---

# 18. Izin File

| Tindakan | Permission | Tambahan wajib |
|---|---|---|
| Ordinary local transformation | `document.content.process` | Feature active, owned/authorized input, capability, policy, valid schema |
| Server transformation/upload | `document.content.process` + `document.server.execute` | Online authorization, notice/consent, classification, capacity |
| Share selected result | `document.result.share` | Exact target, classification, confirmation, current authorization |
| Overwrite exact source/target | `local.file.overwrite` | Producing Feature permission, identity/version check, confirmation, atomic publication |
| Local delete/recycle | `local.data.delete` | Exact own app-managed item, state, confirmation for permanent action |

File picker, kamera, scanner, gesture clipboard, dan akses file-provider platform merupakan pemeriksaan capability/consent OS. Semuanya tidak menggantikan permission bisnis OTO.

---

# 19. Permission File Terlindungi

Metadata dan isi dibedakan:

```text
secured.file.read       ≠ secured.version.download
secured.version.read    ≠ secured.version.download
secured.file.delete     ≠ secured.file.purge
```

Seluruh tindakan memerlukan ownership daring saat ini, policy storage/classification, state/version, quota, dan evaluasi retention/legal hold.

| Siklus hidup | Permission | Konfirmasi | Reauth |
|---|---|:---:|:---:|
| Amankan ke Akun | `secured.file.create` | Yes | Policy |
| Add version | `secured.version.create` | Yes | Policy |
| List/read metadata | `secured.file.list`, `secured.file.read`, `secured.version.list`, `secured.version.read` | No | No default |
| Download content | `secured.version.download` | Policy | Policy |
| Restore version/file | `secured.version.restore`, `secured.file.restore` | Yes when replacing state | Policy |
| Move to Account Recycle | `secured.file.delete` | Yes | No default |
| Permanent deletion | `secured.file.purge` | Yes | Required |

Aturan maksimum tiga versi tetap tidak berubah.

---

# 20. Izin Job

| Tindakan | Permission | Sumber daya | Aturan |
|---|---|---|---|
| List/read | `jobs.job.list`, `jobs.job.read` | Own server/local job | Scope before query/count; current state |
| Cancel | `jobs.job.cancel` | Own cancellable job | Reauthorize current permission/policy/state |
| Retry | `jobs.job.retry` | Own eligible job | New attempt, current Feature permission, idempotency |
| Resume | `jobs.job.resume` | Own compatible checkpoint | Current Feature/capability/schema/policy |
| Read result metadata | `jobs.result.read` | Own result | Current retention/authorization |
| Download result | `jobs.result.download` | Own exact result | Authorize before storage/range/stream |

Support tidak menerima permission Job umum. Support hanya dapat menerima projection metadata Job aman yang tertaut Issue melalui otorisasi Support Issue.

Pekerjaan yang diterima setelah revocation mengikuti Document 08: kontrol pengguna/akses hasil segera berhenti ketika diketahui; policy aplikasi mengarahkan cancel/quarantine/finish-withhold secara aman; rilis hasil selalu diotorisasi ulang.

---

# 21. Izin Riwayat

`history.entry.list`, `history.entry.read`, `history.entry.delete`, dan `history.entry.reprocess` selalu menggunakan scope `OWN`.

- Riwayat Pribadi adalah projection pengguna, bukan Enterprise Audit.
- Menghapus entri tidak pernah menghapus Audit, bukti Job, objek hasil, secured file, atau data observability.
- Reprocess mengulangi pemeriksaan Feature, permission, input/resource, policy, capability, server route, consent, dan reauthentication saat ini.
- Riwayat lokal dapat digunakan luring dalam profile terlindungi; Riwayat server/account hanya daring.

---

# 22. Permission Laporan Issue

| Tindakan | Permission | Scope | Guard |
|---|---|---|---|
| Create Issue | `issue.report.create` | Current reporter | Minimal message + safe context |
| List/read own Issue | `issue.report.list`, `issue.report.read` | `REPORTER` | User-visible projection only |
| Send attachment | `issue.attachment.create` | Exact own Issue | Capture/preview/explicit Send + validation |
| Read own attachment | `issue.attachment.read` | Exact parent Issue | Parent-child validation before stream |

Mode luring dapat mempertahankan draft pesan/screenshot lokal. Submission, status server, dan transfer lampiran memerlukan otorisasi daring saat ini. Kegagalan lampiran tidak membuang pesan Issue secara diam-diam.

---

# 23. Permission Support Console

Support Console menggunakan pemeriksaan tingkat tindakan, bukan satu permission halaman yang luas.

| Surface/tindakan | Permission | Scope | Akses isi |
|---|---|---|:---:|
| Collection | `support.issue.list` | Approved queue/unit/assignment | Safe list projection |
| Detail/message/diagnostics | `support.issue.read` | Exact scoped Issue | Issue content only |
| Screenshot/attachment | `support.attachment.read` | Exact explicitly sent child | Yes, exact attachment only |
| Triage/status | `support.issue.triage` | Exact Issue + current state/version | No unrelated content |
| Internal note read/create | `support.note.read`, `support.note.create` | Exact Issue | Restricted note only |
| Export create/read | `support.export.create`, `support.export.read` | Bounded authorized query/Issue | Authorized fields and attachments only |

Nama file, Job ID, Correlation ID, attachment ID, atau export ID tidak memperluas scope. Ekspor dikontrol aksesnya, memiliki expiry, diotorisasi ulang saat diambil, dan dilindungi dari penyalahgunaan formula/macro/external link bila berlaku.

---

# 24. Izin Konfigurasi

Configuration dibagi menjadi permission read/update khusus sumber daya:

| Configuration | Read | Update | Hard guard |
|---|---|---|---|
| Policy/security/classification/retention | `configuration.policy.read` | `configuration.policy.update` | System safety, legal hold, approved scope |
| Feature exposure/flags | `configuration.feature.read` | `configuration.feature.update` | Existing Feature only; semantics unchanged |
| File/process/storage/concurrency limits | `configuration.limit.read` | `configuration.limit.update` | Immutable safety/capacity ceiling |
| Maintenance/minimum client/API compatibility | `configuration.compatibility.read` | `configuration.compatibility.update` | Typed version, staged validation, safe rollback |

Update dilakukan daring, typed/versioned, menggunakan optimistic concurrency/`If-Match`, memvalidasi scope delegasi, dan memerlukan reauthentication untuk kategori berdampak tinggi.

---

# 25. Izin Kebijakan

`configuration.policy.update` tidak mengotorisasi write key/value arbitrer. Hanya typed policy schema yang disetujui dan scope yang diizinkan yang dapat diubah.

Administrator tidak dapat menonaktifkan:

- pemeriksaan permission/ownership;
- kontrol validation/quarantine/security wajib;
- pembuatan Audit wajib;
- larangan logging secret/content;
- larangan akses langsung Assistant;
- larangan AI publik;
- scope produk non-PKI;
- hard ceiling file/job/resource.

Kewenangan retention/purge Audit bukan permission Auditor. Ownership dan siklus hidup kepatuhan tepat diselesaikan dalam Document 10.

---

# 26. Permission Tampilan Audit

| Tindakan | Permission | Pemetaan role | Aturan |
|---|---|---|---|
| List/search/filter | `audit.event.list` | Auditor default | Scope before count/query |
| Read detail | `audit.event.read` | Auditor default | Safe evidence projection only |
| Create export | `audit.export.create` | Auditor optional | Bounded scope; high-impact reauth policy |
| Retrieve export | `audit.export.read` | Auditor optional | Current authorization, expiry, classification |
| Update/delete source event | None | Prohibited for human roles | Governed lifecycle only |

Administrator atau Support hanya menerima permission tersebut melalui grant Audit eksplisit terpisah dan policy SoD. Referensi peristiwa tidak mengotorisasi dokumen dasar, hasil Job, secured file, lampiran Issue, password, token, tanda tangan, atau teks OCR.

---

# 27. Izin Assistant

Antarmuka Assistant memerlukan `assistant.session.use`, tetapi permission tersebut tidak memberikan tindakan tool.

```text
User intent
→ visible canonical Feature ID and typed parameters
→ permission mapping for that Feature/action
→ input/resource ownership
→ policy/classification/capability/provider/capacity
→ confirmation and reauthentication where required
→ official application service
→ actual validated result
```

Setiap langkah workflow diotorisasi secara independen pada saat eksekusi. Langkah, konfirmasi, atau hasil sukses sebelumnya tidak dapat mengotorisasi tindakan share, overwrite, Redact, transfer server, atau Amankan berikutnya.

Feature Admin/Support/Audit tidak diekspos melalui Assistant secara default. Assistant tidak dapat memanggil SQL langsung, shell, filesystem, API/URL/provider/engine arbitrer, mutasi SignalR, atau perintah Worker.

---

# 28. Permission Capability AI

| Capability | Permission | Gate tambahan |
|---|---|---|
| Local deterministic Assistant routing | `assistant.session.use` | Local policy/capability; no content server transfer |
| Approved local Intelligence | `document.content.process` + `document.intelligence.execute` | Feature/provider approval, classification, selected content |
| Enterprise-internal Intelligence | Above + `document.server.execute` | Online, data minimization, notice/consent, provider/capacity policy |
| Internal provider invocation | `provider.ai.invoke` | Workload identity, exact purpose/request, no human assignment |

Endpoint AI/terjemahan publik tetap dilarang secara teknis. Output prompt/document/OCR/HTML/model merupakan data tidak tepercaya dan tidak pernah menciptakan permission, confirmation, pemilihan provider, atau kewenangan tool.

---

# 29. Permission Core PDF Tool

Legenda: `Base` = `document.content.process`; `Server` = tambahan `document.server.execute`; seluruh eksekusi Core membuat metadata Audit proses yang aman. Kewenangan Administrator/Auditor/Support saja tidak memberikan permission tersebut; principal juga wajib memiliki permission tool User untuk sumber dayanya sendiri.

| # | Primary Tool | Pemetaan canonical | Permission wajib | Luring/lokal | Gate khusus | Audit |
|---:|---|---|---|---|---|:---:|
| 1 | Merge PDF | `FEAT-PDF-005` | Base | Yes | Authorized inputs | YES |
| 2 | Split PDF | `FEAT-PDF-006`, `FEAT-PDF-007` | Base | Yes | Range validation | YES |
| 3 | Remove pages | `FEAT-PDF-013` | Base | Yes | Working copy; source preserved | YES |
| 4 | Extract pages | `FEAT-PDF-014` | Base | Yes | Selected pages/new result | YES |
| 5 | Organize PDF | `FEAT-PDF-010..016` | Base | Yes | Per-step resource/state checks | YES |
| 6 | Scan to PDF | `FEAT-SCAN-014` | Base | Capability-dependent | OS camera/scanner consent; scanner PoC | YES |
| 7 | Compress PDF | `FEAT-PDF-023` | Base | Yes | Resource budgets | YES |
| 8 | Repair PDF | `FEAT-PDF-025`, `FEAT-PDF-026` | Base; Server for fallback | Basic local | Quarantine/parser isolation | YES |
| 9 | OCR PDF | `FEAT-OCR-004` | Base; Server for approved fallback | Local first | OCR derivative/classification | YES |
| 10 | JPG to PDF | `FEAT-CONV-007` | Base | Yes | Image validation | YES |
| 11 | Word to PDF | `FEAT-CONV-001` | Base; Server when selected | Capability-dependent | Office active-content control/PoC | YES |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | Base; Server when selected | Capability-dependent | Office active-content control/PoC | YES |
| 13 | Excel to PDF | `FEAT-CONV-002` | Base; Server when selected | Capability-dependent | Formula/external-link control/PoC | YES |
| 14 | HTML to PDF | `FEAT-CONV-017` | Base; Server when selected | Capability-dependent | Validated captured/local HTML; no remote URL baseline | YES |
| 15 | PDF to JPG | `FEAT-CONV-012` | Base | Yes | Selected pages/output validation | YES |
| 16 | PDF to Word | `FEAT-CONV-004` | Base; Server when selected | Capability-dependent | Feasibility gate | YES |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | Base; Server when selected | Capability-dependent | Feasibility gate | YES |
| 18 | PDF to Excel | `FEAT-CONV-005` | Base; Server when selected | Capability-dependent | Structured-data/classification gate | YES |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | Base; Server when selected | Capability-dependent | Conformance validation/PoC | YES |
| 20 | Rotate PDF | `FEAT-PDF-012` | Base | Yes | Working copy | YES |
| 21 | Add page numbers | `FEAT-PDF-037` | Base | Yes | Working copy | YES |
| 22 | Add watermark | `FEAT-PDF-036` | Base | Yes | Authorized watermark asset | YES |
| 23 | Crop PDF | `FEAT-PDF-019` | Base | Yes | Hidden/out-of-crop awareness | YES |
| 24 | Edit PDF | `FEAT-PDF-031..041` | Base; Server for approved subset | Local baseline | Existing-text subset PoC | YES |
| 25 | PDF Forms | `FEAT-PDF-053..060` | Base | Yes | Field-data/classification; flatten confirmation | YES |
| 26 | Unlock PDF | `FEAT-PDF-062` | Base + `document.pdf.unlock` | Yes | Valid password/authority; confirmation | YES |
| 27 | Protect PDF | `FEAT-PDF-061` | Base + `document.pdf.protect` | Yes | Confirmation; password never persisted | YES |
| 28 | Sign PDF | `FEAT-SIGN-013` | Base + `document.signature.apply` | Yes | Visual non-PKI; final confirmation | YES |
| 29 | Redact PDF | `FEAT-PDF-069` | Base + `document.redaction.finalize` | Yes | Preview, final confirmation, removal validation | YES |
| 30 | Compare PDF | `FEAT-PDF-071`, `FEAT-PDF-072` | Base | Yes | Both inputs authorized | YES |
| 31 | AI Summarizer | `FEAT-INTEL-001` | Base + `document.intelligence.execute`; Server for internal provider | Approved local only offline | Provider/classification/minimization/consent | YES |
| 32 | Translate PDF | `FEAT-INTEL-002` | Base + `document.intelligence.execute`; Server for internal provider | Approved local only offline | No public endpoint | YES |
| 33 | PDF to Markdown | `FEAT-CONV-036` | Base; Server for approved complex route | Deterministic local first | Extracted output remains untrusted | YES |

Permission tidak pernah menimpa status rilis, Feature flag, exposure platform, kompatibilitas schema, feasibility gate, persetujuan provider, atau capability engine.

---

# 30. Tindakan PDF Sensitif terhadap Keamanan

| Tindakan | Permission | Konfirmasi | Reauth default | Daring | Security invariant |
|---|---|:---:|---|---|---|
| Protect PDF | `document.pdf.protect` | Required before final result | Policy-conditional | No locally | Password never persisted/logged |
| Unlock PDF | `document.pdf.unlock` | Required | Policy-conditional | No locally | Valid password/authority; no bypass |
| Visual Sign | `document.signature.apply` | Required before finalization | Policy-conditional | No locally | Non-PKI; signature asset stays protected/local by default |
| Redact | `document.redaction.finalize` | Required immediately before finalization | Policy-conditional | No locally | Remove/flatten and verify, never overlay-only |
| Intelligence | `document.intelligence.execute` | Required where content transfer/provider risk applies | Policy-conditional | Provider-dependent | Approved local/internal provider only |
| Share | `document.result.share` | Required | Policy-conditional | Target-dependent | Exact target and classification |
| Overwrite | `local.file.overwrite` | Required | No by default | No | Exact file identity; atomic/recoverable publication |

Reauthentication tidak menggantikan confirmation, password/kewenangan, ownership, policy, atau validasi output.

---

# 31. Perilaku Lokal dan Luring

Otorisasi luring menggunakan satu grant minimum terlindungi yang hanya memuat binding subject/profile, projection permission/capability yang layak luring, ceiling policy/classification, revisi contract, issued/last-online/absolute-expiry, bukti integrity/time, binding device bila didukung, dan marker disable/revoke terakhir yang diketahui.

Grant tidak memuat token, cookie, password, kredensial service, claim tanpa batas, permission server/Admin/Support/Audit, atau isi dokumen.

Diizinkan luring jika seluruh pemeriksaan lulus:

- Feature pengguna lokal deterministik yang layak;
- workspace lokal, draft, preview, undo, result, history, dan recovery sendiri;
- Protect/Unlock/Redact/visual Sign lokal milik sendiri sesuai policy tindakan dan confirmation;
- orkestrasi Assistant hanya jika setiap langkah layak luring;
- pembuatan outbox metadata Audit wajib yang aman oleh aplikasi.

Aturan:

- absolute expiry; tanpa sliding renewal saat luring;
- clock rollback atau trusted time yang tidak pasti bersifat fail-closed;
- grant lokal tidak dapat mengotorisasi upload/server/resource API;
- pergantian account/profile tidak pernah menggunakan ulang grant;
- reconnect me-refresh identity, account, session, permission, policy, capability, dan compatibility sebelum perintah tertunda;
- intent dalam antrean bukan otorisasi sebelumnya.

Cached page web/aset service-worker tidak memberikan kewenangan bisnis luring dalam arsitektur saat ini.

---

# 32. Permission Sensitif yang Hanya Daring

Hal berikut selalu daring:

- `document.server.execute`;
- seluruh permission secured-file dan secured-version;
- seluruh permission Issue server;
- seluruh permission administration, configuration, monitoring, Support, dan Audit;
- seluruh permission realtime;
- setiap tindakan yang memerlukan reauthentication;
- seluruh permission workload.

Saat luring, UI mengembalikan `ONLINE_REQUIRED`, mempertahankan source/draft/intent lokal yang aman bila diizinkan, tidak menghasilkan side effect server, dan tidak pernah mengubah route eksekusi secara diam-diam.

---

# 33. Kasus Reauthentication

Fresh reauthentication wajib untuk:

- `admin.role.assign` and `admin.role.revoke`;
- `admin.permission.assign` and `admin.permission.revoke`;
- `admin.user.update` when changing another principal's eligibility;
- `admin.session.revoke` for another session;
- high-impact `configuration.policy.update` affecting security/classification/retention;
- `secured.file.purge`;
- key recovery/destructive recovery;
- any future exceptional user-content access, if formally approved.

Kondisional berdasarkan policy hingga wajib:

- high-impact `audit.export.create`, `audit.export.read`, `support.export.create`, or `support.export.read`;
- broad Feature, maintenance, minimum-version, or limit changes;
- high-classification upload, secured transfer, download, share, or disclosure;
- broad/high-classification Support attachment access.

Konteks reauthentication berumur pendek dan terikat pada principal, session, client, tindakan, resource/scope, revisi policy/permission, dan assurance. Logout, disablement, revocation, perubahan assignment/policy/resource-version, mismatch, atau expiry membatalkannya.

Freshness yang hilang menghasilkan `401 REAUTHENTICATION_REQUIRED` yang aman sebelum mutasi. Identity fresh yang valid tanpa permission tetap menerima `403`/`404` tersembunyi.

Metode tepat, usia maksimum, penggunaan terbatas, perilaku retry/lock, dan ambang berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 34. Enforcement API

```text
HTTPS / approved proxy
→ authenticate client/session/account
→ endpoint permission policy
→ load minimum resource authorization metadata
→ owner or explicit scope handler
→ policy/classification/state/version/context/reauth
→ owning application service
→ authorized query/mutation/content stream
→ safe evidence after authoritative outcome
```

Kebutuhan:

- fallback policy terautentikasi pada surface terlindungi;
- principal, role, permission, owner, dan scope diturunkan server;
- application service tetap menjadi batas otorisasi server akhir;
- scope daftar diterapkan sebelum count/filter/cursor;
- pemeriksaan parent-child dilakukan sebelum isi attachment/version/result;
- continuation, retry, resume, cancel, restore, export, range, pengambilan hasil, dan download diotorisasi ulang;
- request API langsung memperoleh hasil yang sama dengan UI/Assistant;
- object state tanpa otorisasi tidak bocor melalui `409`/`412`;
- `401`, `403`, atau `404` tersembunyi yang konsisten dan aman mengikuti Document 06.

Implementasi `FEAT-ADMIN-001..003` dan `FEAT-AUTH-007` untuk pengguna lain memerlukan addendum contract Document 06 yang terkendali. Route baru tidak dibuat di sini.

---

# 35. Enforcement SignalR

`/hubs/activity` tetap terautentikasi dan hanya untuk notifikasi.

- validasi identity/session/account/client saat koneksi;
- turunkan group pengguna pada sisi server;
- otorisasi setiap subscription opsional per job menggunakan ownership/permission/state saat ini;
- jangan menerima user, role, group, unit, owner, atau scope yang dikirim klien;
- publish hanya metadata post-commit yang aman;
- jangan mengekspos mutasi bisnis;
- batasi koneksi, subscription, buffer, dan slow consumer;
- hentikan/validasi ulang saat expiry, logout, disablement, revocation, atau perubahan permission yang relevan;
- REST merekonsiliasi state berwenang setelah reconnect.

Kepemilikan event, connection ID, nama group, Job ID, Issue ID, atau Correlation ID tidak pernah memberikan akses.

---

# 36. Enforcement Worker

Otorisasi bisnis manusia diselesaikan oleh shared/API application service sebelum eksekusi diterima. Pesan queue atau request IPC merupakan locator/closed contract, bukan capability token manusia.

Worker wajib:

- mengautentikasi workload/instance dan audience/purpose;
- memuat ulang Job/operasi lokal berwenang;
- memvalidasi versi contract, Feature ID canonical, handler terdaftar, typed schema, referensi sumber daya, profile/environment, state/version/lease, deadline, dan resource budget;
- menolak path, URL, provider, engine, SQL, shell, process, assembly, atau operasi arbitrer;
- tetap dalam scope Job/object/queue/path tepat;
- memvalidasi output sebelum promosi akhir;
- menghormati cancel/quarantine/policy-stop pada batas aman;
- hanya mengeluarkan status aman yang dinormalisasi.

Worker tidak pernah mengevaluasi pemetaan role `User`, `Administrator`, `Auditor`, atau Support dan tidak pernah menerima token end-user.

---

# 37. Perilaku UI

UI Web/Desktop/Mobile menerima projection action/capability versioned yang aman dan dapat:

- menampilkan, menyembunyikan, atau menonaktifkan tindakan;
- menjelaskan state denied, offline, online-required, reauthentication-required, unsupported, unavailable, atau update-required;
- meminta confirmation/reauthentication yang tepat;
- menghindari panggilan permission/API per tombol.

UI dilarang:

- mengotorisasi sumber daya server;
- menggunakan `if role == ...` sebagai pemeriksaan akhir;
- mengungkap keberadaan Feature/resource tersembunyi atau count terbatas;
- mempercayai visibilitas cache setelah keputusan server gagal;
- memperlakukan route/menu/page langsung sebagai permission;
- mengirim role, permission, owner, scope, override classification, atau hasil reauth sebagai data tepercaya.

Layout responsif dan capability device tidak mengimplikasikan exposure Feature atau otorisasi yang sama pada setiap platform.

---

# 38. Konvensi Penamaan Izin

Grammar canonical:

```text
^[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*$
```

Format:

```text
domain.resource.action
```

Tindakan terkendali mencakup `list`, `read`, `create`, `update`, `delete`, `purge`, `execute`, `cancel`, `retry`, `resume`, `download`, `restore`, `assign`, `revoke`, `triage`, `connect`, `subscribe`, `claim`, `ingest`, `invoke`, dan `publish`.

Jangan mengenkode role, `own`, `all`, `any`, `superadmin`, environment, unit, resource ID, status Feature, online/offline, classification, atau `bypass` dalam kode permission. Ownership dan konteks tetap terpisah.

---

# 39. Aturan Pewarisan Izin

1. Role formal tidak mewarisi role lain.
2. Principal dapat memiliki beberapa role/profile yang disetujui; permission efektif digabungkan, lalu dibatasi oleh SoD, policy, resource scope, dan konteks.
3. Deny/hard policy mengalahkan grant.
4. Administrator tidak mengimplikasikan User, Auditor, atau Support.
5. Auditor tidak mengimplikasikan User, Administrator, atau Support.
6. Profil Support tidak mengimplikasikan role formal atau permission isi di luar scope Issue tepatnya.
7. Permission manusia tidak pernah mengimplikasikan permission workload, dan permission workload tidak mengimplikasikan kewenangan manusia.
8. Reauthentication, confirmation, visibilitas UI, exposure Feature, atau permission OS tidak pernah menciptakan permission bisnis.
9. Grant Support sesuai scope khusus principal, bila disetujui, tidak mengubah bundle role canonical.
10. Wildcard dan scope `ALL` implisit dilarang.

---

# 40. Aturan Tolak secara Default

Tolak jika salah satu hal berikut hilang, tidak dikenal, inactive, stale, expired, mismatch, atau dilarang:

- identity, account, session, atau offline grant;
- permission atau subject type yang diizinkan;
- Feature/handler/schema/exposure canonical;
- ownership, relasi parent-child, atau scope eksplisit;
- resource state/version/retention/kelayakan legal hold saat ini;
- enterprise/classification policy;
- client/platform/capability/provider/compatibility;
- kewenangan daring, consent, confirmation, atau reauthentication;
- workload identity/lease/purpose;
- dependency keamanan wajib.

Kegagalan authorization-store atau policy-service tidak dapat menghasilkan stale privileged allow. Permission yang tidak dikenal menggagalkan validasi deployment atau otorisasi runtime secara aman.

---

# 41. Matriks Role × Permission

Legenda:

```text
D = default role grant
O = optional/separately assigned grant
P = prohibited/not granted by this role baseline
S = Support-profile grant, not a formal-role grant
N = non-human/system only
```

`P` tidak mencegah principal memiliki role formal lain yang disetujui secara terpisah; kode tersebut berarti role pada kolom itu tidak pernah mengimplikasikan permission.

| Keluarga permission | User | Administrator | Auditor | Profil Support | Scope default |
|---|:---:|:---:|:---:|:---:|---|
| Own account/profile/preferences | D | P | P | P | `OWN` |
| Assistant interface | D | P | P | P | `OWN` |
| Ordinary document processing | D | P | P | P | Authorized input + own result |
| Server processing modifier | D, policy-gated | P | P | P | `OWN` + online transfer policy |
| Protect/Unlock/Sign/Redact | D, policy-gated | P | P | P | `OWN` |
| Approved Intelligence | D, policy/provider-gated | P | P | P | `OWN` |
| Share/overwrite/local delete | D, policy-gated | P | P | P | `OWN` |
| Own workspace | D | P | P | P | `OWN` |
| Own jobs/results | D | P | P | P | `OWN` |
| Own Personal History | D | P | P | P | `OWN` |
| Own secured files/versions | D | P | P | P | `OWN` |
| Own Issue/attachment | D | P | P | P | `REPORTER` |
| Own realtime activity/job | D | P | P | P | `OWN` |
| Admin account/session read | P | D | P | P | `ADMIN_SCOPE` |
| Admin account/session mutation | P | D, sensitive | P | P | `ADMIN_SCOPE` |
| Role/permission catalog read | P | D | P | P | `ADMIN_SCOPE` |
| Role/permission assignment | P | D, sensitive | P | P | `ADMIN_SCOPE` |
| Configuration read/update | P | D; update risk-gated | P | P | `ADMIN_SCOPE` |
| Monitoring safe metadata | P | D | P | P | `ADMIN_SCOPE` |
| Audit event list/read | P | O, SoD-gated | D | P | `AUDIT_SCOPE` |
| Audit export create/read | P | O, SoD-gated | O | P | `AUDIT_SCOPE` |
| Support Issue list/read | P | O | P | S | `SUPPORT_SCOPE` |
| Support attachment | P | O | P | S, separate | `SUPPORT_SCOPE` |
| Support triage/note | P | O | P | S, separate | `SUPPORT_SCOPE` |
| Support export | P | O | P | S, separate | `SUPPORT_SCOPE` |
| Audit update/delete | P | P | P | P | None |
| Other-user document/result/secured content | P | P | P | P | None |
| Password/token/secret/signature/clipboard/full OCR | P | P | P | P | None |
| Workload operations | N | N | N | N | `WORKLOAD_JOB_SCOPE` |

Administrator yang memiliki grant Audit atau Support opsional tidak mengubah grant tersebut menjadi default role. Policy SoD dan independent review tetap berlaku.

---

# 42. Matriks Permission × Sumber Daya

| Keluarga permission | Sumber daya | Relasi | Lintas pengguna | Isi | Guard parent/state |
|---|---|---|:---:|:---:|---|
| Document/local/workspace permissions | Selected local input/workspace/result | `OWN`/authorized local selection | No | Yes | Profile/path/file identity/state |
| Job permissions | Job | `OWN` | No | Metadata only | Current Job state/version |
| Result permissions | Result | `OWN` | No | Download permission only | Parent Job + result state/retention |
| Personal History permissions | Personal History | `OWN` | No | Safe personal projection | Entry state; current Feature on reprocess |
| Secured-file permissions | Secured file | `OWN` | No baseline | Metadata; content through version | Recycle/retention/legal hold/ETag |
| Secured-version permissions | Secured version | `OWN` | No baseline | Download permission only | Parent file + version membership/state |
| Reporter Issue permissions | Issue | `REPORTER` | No | Reporter projection | Issue state |
| Reporter attachment permissions | Issue attachment | `REPORTER` | No | Exact attachment | Authorized parent + explicit-send relation |
| Administration permissions | Control-plane metadata | `ADMIN_SCOPE` | Yes, metadata only | No user content | Delegation, target, version, SoD |
| Configuration permissions | Typed configuration | `ADMIN_SCOPE` | Yes, metadata only | No user content | Scope, schema, version, hard bounds |
| Monitoring permissions | Safe aggregate telemetry | `ADMIN_SCOPE` | Aggregate only | No | Field allowlist, environment scope |
| Audit-event permissions | Enterprise evidence | `AUDIT_SCOPE` | Yes, evidence only | No referenced resource content | Scope before count/filter/detail |
| Audit-export permissions | Bounded export artifact | `AUDIT_SCOPE` | Yes, evidence only | Authorized safe evidence | Source scope, expiry, current permission |
| Support-Issue permissions | Support Issue | `SUPPORT_SCOPE` | Yes, Issue only | Message/safe diagnostic | Assignment/queue + state/version |
| Support-attachment permission | Explicit Issue attachment | `SUPPORT_SCOPE` | Yes, exact attachment | Yes, exact attachment | Parent Issue + explicit-send membership |
| Support-note permissions | Internal note | `SUPPORT_SCOPE` | Yes, Issue-bound | Restricted note | Parent Issue + note visibility |
| Support-export permissions | Bounded Support export | `SUPPORT_SCOPE` | Yes, scoped Issue data | Authorized fields/attachments | Filter scope, expiry, current permission |
| Workload permissions | Job/object/provider/event | `WORKLOAD_JOB_SCOPE` | Technical only | Exact required payload | Identity, lease, purpose, environment |

Relasi `ALL` tanpa batas tidak valid.

---

# 43. Matriks Permission × Platform

Legenda: `L` layak lokal, `O` kewenangan daring, `C` bergantung pada capability/policy, `X` tidak tersedia pada arsitektur saat ini.

| Keluarga permission/tindakan | Windows Desktop | Mobile/Tablet | Web | Server/Worker |
|---|---|---|---|---|
| Ordinary local document processing | `L/C` | `L/C` curated | Online browser execution only | `O/C` when selected |
| Protect/Unlock/Sign/Redact local | `L/C` | `L/C` curated | Online browser execution only | Only approved route |
| Server processing/content transfer | `O` | `O` | `O` | Workload executes accepted Job |
| Local workspace/history/recovery | `L` | `L/C` | No offline business authority | N/A |
| Local overwrite/delete/share | `L/C` | `L/C` | Target/capability dependent, online | N/A |
| Secured files/versions | `O` | `O` | `O` | Scoped storage workload |
| Issue/report attachment | Draft local; submit `O` | Draft local; submit `O` | `O` | Support/API workload |
| Administrator/configuration/monitoring | `O` | `O` if exposed | `O` | N/A for human; service metadata only |
| Support Console | `O` | `O` if explicitly exposed | `O` | Export workload only |
| Auditor query/export | `O` | `O` if explicitly exposed | `O` | Export workload only |
| Assistant local routing | `L/C` | `L/C` | No offline authority | Internal provider `O/C` |
| SignalR | `O` | `O` | `O` | Safe publisher workload |
| Workload permissions | X for human app | X for human app | X for browser | Exact workload only |

Ketersediaan platform tidak pernah memperluas role atau permission.

---

# 44. Matriks Tindakan Sensitif

| Tindakan | Permission | Daring | Konfirmasi | Reauth | SoD/persetujuan | Audit wajib |
|---|---|:---:|:---:|---|---|:---:|
| Ordinary own local process | `document.content.process` | No | At final risky/output boundary only | No default | No | YES |
| Server process/upload | `document.server.execute` | Yes | Explicit transfer/notice | Policy | Classification/provider policy | YES |
| Protect PDF | `document.pdf.protect` | No locally | Yes | Policy | No default | YES |
| Unlock PDF | `document.pdf.unlock` | No locally | Yes | Policy | Valid password/authority | YES |
| Visual Sign | `document.signature.apply` | No locally | Yes | Policy | Non-PKI only | YES |
| Final Redact | `document.redaction.finalize` | No locally | Yes | Policy | Output assurance | YES |
| Share result | `document.result.share` | Target-dependent | Yes | Classification policy | Target policy | YES |
| Overwrite local file | `local.file.overwrite` | No | Yes | No default | Exact-file check | YES |
| Permanent local deletion | `local.data.delete` | No | Yes | No default | Policy | CONDITIONAL |
| Amankan/add secured version | `secured.file.create` / `secured.version.create` | Yes | Yes | Classification policy | Quota/version policy | YES |
| Permanent secured deletion | `secured.file.purge` | Yes | Yes | Required | Retention/legal hold | YES |
| User eligibility update | `admin.user.update` | Yes | Exact target/effect | Required | Delegation/SoD | YES |
| Role/permission assign/revoke | `admin.role.assign`, `admin.role.revoke`, `admin.permission.assign`, `admin.permission.revoke` | Yes | Exact target/grant | Required | No self-elevation; delegation/SoD | YES |
| Other-session revoke | `admin.session.revoke` | Yes | Exact target | Required | Delegation scope | YES |
| Security/classification/retention update | `configuration.policy.update` | Yes | Exact scope/change | Required/high-impact | Approval/hard bounds | YES |
| Broad Feature/limit/compatibility update | `configuration.feature.update`, `configuration.limit.update`, `configuration.compatibility.update` | Yes | Exact scope/change | Policy/high-impact | Rollout/SoD policy | YES |
| Audit export | `audit.export.create`, `audit.export.read` | Yes | Export scope | Policy/high-impact | Bounded scope | YES |
| Support attachment/export | `support.attachment.read`, `support.export.create`, `support.export.read` | Yes | Export/disclosure as applicable | Policy/high-impact | Issue scope | YES |
| Future exceptional content access | No current permission | N/A | N/A | Always if approved | Product/Security/Legal approval | YES |

---

# 45. Ketertelusuran Kebutuhan

| Kebutuhan | Cakupan Matriks Peran |
|---|---|
| `FR-AUTH-001..005` | Pertamina Identity, session/offline boundary, logout/revocation, online-sensitive rules |
| `FR-ROLE-001..003` | Three formal roles; permission-based and backend-controlled authorization |
| `FR-FILE-001..005,010..015` | Authorized input, validation/security control, password and resource boundary |
| `FR-PROC-001..005` | Local/server permission separation and online dependency |
| `FR-STOR-001..006` | Own local workspace and storage/capacity policy |
| `FR-BACKUP-001..010` | Own secured file/version lifecycle and explicit transfer |
| `FR-HIST-001..003` | Own Personal History read/delete/reprocess |
| `FR-AUDIT-001..004` | Scoped read/export; system ingestion; no ordinary delete |
| `FR-TRASH-001..009` | Own local/account recycle rules and admin policy separation |
| `FR-REC-001..006` | Current authorization on retry/resume/recovery |
| `FR-VER-001..004` | Own version lifecycle and maximum-three rule |
| `FR-SIGN-001..003`, `BR-SIGN-001` | Visual non-PKI signature permission and content boundary |
| `FR-ASSIST-001..015` | Assistant interface, parity, per-step authorization, offline boundary |
| `FR-SUP-001..007` | Reporter and separated Support permissions |
| `FR-INTEL-001..003` | Approved local/internal intelligence and transfer/provider gates |
| `SR-SEC-001..023` | Least privilege, content separation, Assistant, Support, Unlock/Redact controls |
| `IR-API-001..012` | Server authorization, Issue/Support, safe API enforcement, no invented routes |
| `NFR-MON-001..004` | Safe monitoring/issue diagnostics permissions |

---

# 46. Ketertelusuran Fitur

Seluruh 550 record canonical tercakup tanpa membuat 550 permission:

| Kelompok Feature | Jumlah | Pemetaan permission utama |
|---|---:|---|
| `FEAT-PDF-001..075` | 75 | `document.content.process`; sensitive exceptions in sections 29–30 |
| `FEAT-CONV-001..036` | 36 | `document.content.process`; server modifier when selected |
| `FEAT-INTEL-001..002` | 2 | Processing + `document.intelligence.execute`; server modifier for internal provider |
| `FEAT-IMG-001..048` | 48 | `document.content.process`; capability/policy |
| `FEAT-SCAN-001..018` | 18 | `document.content.process`; device consent/capability |
| `FEAT-OCR-001..013` | 13 | `document.content.process`; server modifier for approved route |
| `FEAT-SIGN-001..013` | 13 | Processing + `document.signature.apply` for final application |
| `FEAT-ANN-001..014` | 14 | `document.content.process` |
| `FEAT-FILE-001..010` | 10 | Processing/local file action; output boundary permissions where applicable |
| `FEAT-PROD-001..018` | 18 | Processing/output; `document.result.share` for share |
| `FEAT-INPUT-001..021` | 21 | OS/user input capability; called Feature permission at execution |
| `FEAT-UX-001..018` | 18 | UX/capability projection; no standalone authorization grant |
| `FEAT-EDIT-001..012` | 12 | Workspace/process; `local.file.overwrite` for `FEAT-EDIT-012` |
| `FEAT-FLOW-001..010` | 10 | Same permission per canonical workflow step |
| `FEAT-PROC-001..015` | 15 | Application/workload processing control; not human bypass |
| `FEAT-VALID-001..018` | 18 | Mandatory system validation control |
| `FEAT-SEC-001..023` | 23 | Mandatory security control; non-human |
| `FEAT-STOR-001..016` | 16 | Own workspace/local storage policy; system cleanup controls |
| `FEAT-BACKUP-001..016` | 16 | Secured-file, secured-version, preference, or policy permission as applicable |
| `FEAT-HIST-001..015` | 15 | Personal History permissions with `OWN` scope |
| `FEAT-TRASH-001..012` | 12 | `local.data.delete`, secured lifecycle, or admin policy by resource |
| `FEAT-REC-001..015` | 15 | Workspace/job current authorization and recovery state |
| `FEAT-VER-001..006` | 6 | Secured/workspace version permissions by location |
| `FEAT-AUTH-001..012` | 12 | Authentication/session/current profile controls; not generic tool execution |
| `FEAT-ADMIN-001..022` | 22 | Administration, configuration, or monitoring permissions |
| `FEAT-AUDIT-001..008` | 8 | System ingestion/lifecycle or Audit-event/Audit-export permissions |
| `FEAT-PROG-001..009` | 9 | Own Job/activity actions or system notification; no new role grant |
| `FEAT-MON-001..017` | 17 | Safe monitoring permissions or observability workload |
| `FEAT-ASSIST-001..030` | 30 | `assistant.session.use`; underlying action permission per step |
| `FEAT-SUPPORT-001..008` | 8 | Reporter Issue or separate Support permissions |

Verifikasi jumlah:

```text
Canonical Features mapped : 550 / 550
New Feature IDs           : 0
Removed Feature IDs       : 0
Duplicate processing API  : 0
```

---

# 47. Ketertelusuran Keamanan

| Baseline keamanan | Aturan Matriks Peran |
|---|---|
| `SEC-DEC-001` | Deny-by-default and trust-boundary checks |
| `SEC-DEC-002..003` | Pertamina authentication, OTO authorization, secure Web/native session |
| `SEC-DEC-004` | Narrow absolute-expiry offline grant |
| `SEC-DEC-005..007` | Object authorization, reauth, privileged/content separation |
| `SEC-DEC-008..010` | Protected local data, untrusted files, transfer/storage controls |
| `SEC-DEC-011` | Workload/IPC identity separated from human permission |
| `SEC-DEC-012..013` | API/Web edge and remote-content restrictions preserved |
| `SEC-DEC-014..015` | Assistant/AI closed permission path and approved provider only |
| `SEC-DEC-016..018` | Secret/log/privacy and abuse controls cannot be admin-disabled |
| `SEC-DEC-019..020` | SDLC/test/release evidence requirements |

Bukti wajib:

- User A tidak dapat membaca secured file User B.
- Administrator tidak dapat membaca isi dokumen User A hanya berdasarkan role.
- Auditor tidak dapat melakukan mutasi/delete bukti Audit.
- Support tidak dapat mengakses password, token, tanda tangan, clipboard, OCR lengkap, atau isi yang tidak terkait.
- Assistant tidak dapat melewati permission, ownership, confirmation, reauthentication, platform, atau policy luring.

---

# 48. Kriteria Penerimaan Pengujian

## 48.1 Matriks Skenario

Setiap keluarga permission yang berlaku wajib menguji:

```text
ALLOW
MISSING PERMISSION
WRONG OWNER / WRONG SCOPE
WRONG ROLE / PROFILE
EXPIRED SESSION / OFFLINE GRANT
DISABLED ACCOUNT
REVOKED SESSION / ROLE / PERMISSION
STALE PERMISSION OR POLICY REVISION
OFFLINE
ONLINE
REAUTHENTICATION REQUIRED
DIRECT API ATTEMPT
ASSISTANT ATTEMPT
SIGNALR SUBSCRIPTION ATTEMPT
WORKER CONTRACT ATTEMPT
NO FORBIDDEN DATA LEAKAGE
```

## 48.2 Lapisan yang Wajib

| Lapisan | Penerimaan |
|---|---|
| Unit | Permission, owner/scope, policy, state/version, context, reauth, offline decision |
| Integration | Current backend permission snapshot, scoped repository query, parent-child, invalidation, no content before authorization |
| API contract | Direct route, safe 401/403/404/409/412, authorization before count/range/export/storage |
| Web/Desktop/Mobile E2E | UI projection and real application authorization remain consistent |
| SignalR | Cross-user groups/subscriptions denied; reconnect/revocation revalidated |
| Worker/IPC | Forged identity/job/schema/ref/path/operation/lease rejected |
| Assistant parity | Same result as ordinary UI for every step, including denial/online/reauth |
| Negative security | IDOR, self-elevation, stale cache, toxic grant, content leakage, arbitrary command/provider denied |
| Regression | Every fixed authorization defect becomes a permanent test |

## 48.3 Kasus Negatif Wajib

1. User A menebak Job/result/history/secured/version/Issue ID User B.
2. Administrator memanggil route document/result/OCR/secured-content.
3. Auditor mencoba Audit `PUT/PATCH/DELETE` atau memperluas scope ekspor.
4. Support membaca Issue/attachment/job/file/result/storage yang tidak terkait.
5. Klien memalsukan principal, role, permission, owner, scope, classification, reauth, Feature ID, cursor, atau ETag.
6. Principal melakukan self-assignment atau grant di luar delegasi.
7. Offline grant expired/tampered/copied digunakan ulang pada profile/device lain.
8. Perintah dalam antrean di-dispatch sebelum refresh otorisasi reconnect.
9. SignalR melakukan subscription ke group/Job pengguna lain atau memicu mutasi.
10. Worker menerima contract queue/IPC palsu, operation/path/URL/provider arbitrer, atau token end-user.
11. Assistant memanggil langkah hidden/unauthorized/cross-user/sensitive atau menyimpulkan confirmation.
12. List/count/range/export tanpa otorisasi membocorkan keberadaan, metadata, timing, atau isi.

## 48.4 Penerimaan Kinerja-Keamanan

- satu projection action/capability aman per halaman/konteks, bukan pemeriksaan jaringan per tombol;
- satu snapshot permission efektif yang memperhatikan revisi per request/operasi;
- scope diterapkan dalam kueri database sebelum count/projection;
- metadata otorisasi minimum dimuat sebelum isi;
- cache terbatas yang memperhatikan revisi dengan mode kegagalan aman;
- storm permission/revocation/reconnect/SignalR tetap aman;
- budget numerik di-benchmark dan diselesaikan dalam Document 12, bukan dibuat di sini.

---

# 49. Serah Terima Terkendali

Hal yang berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`:

- approved IAM claim/group mapping and provisioning;
- exact Support/Admin/Audit organizational scope types;
- Support permission assignment source and, if needed, controlled persistence addendum;
- delegation ceilings, incompatible role/profile combinations, dual approval, and recertification;
- reauthentication method, age, attempt policy, and classification thresholds;
- offline grant lifetime and revocation propagation objective;
- permission/policy revision and cache invalidation contract details;
- future emergency administrative recovery; no content break-glass exists now.

Serah terima dokumen terkendali:

| Serah terima | Alasan |
|---|---|
| Document 06 addendum | Missing management/session-revocation contracts before `FEAT-ADMIN-001..003` and other-user `FEAT-AUTH-007` coding |
| Document 07 addendum, only if needed | Scoped principal Support grants if no approved Pertamina mapping exists |
| Document 10 | Audit-required action event names, fields, integrity, retention, legal hold, export, alert, and SIEM |
| Document 11/12 | Unit/integration/test-plan implementation and numeric acceptance thresholds |

Tidak ada serah terima yang membuka kembali Matriks Peran logis atau mengotorisasi fallback yang lebih lemah.

---

# 50. Audit Akhir dan Status

```text
Specialist reports integrated : 4 / 4
Parent FINAL documents audited: 8 / 8
Role decisions                : 18 / 18 AUTO-LOCKED
Formal roles                  : 3
Support profile               : permission-based, not a role
Canonical permission codes    : 88
Required areas                : 48 / 48
Canonical features            : 550 / 550
Primary PDF Tool Cards        : 33 / 33
Resolved conflicts            : 14
Material contradiction        : 0
New formal role               : 0
Implicit privileged content   : 0
User approval required        : 0
Document status               : FINAL v1.0
```

Dokumen berikutnya:

```text
10_AUDIT_TRAIL_DESIGN.md
```
