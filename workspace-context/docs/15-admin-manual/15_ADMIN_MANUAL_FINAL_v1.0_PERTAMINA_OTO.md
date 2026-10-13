# 15 — ADMIN MANUAL
## PERTAMINA OTO — Office Tools Organizer

> **Versi manual:** FINAL v1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE  
> **Release readiness:** BELUM RELEASE_READY  
> **Aplikasi/RC:** `RC-TBD`  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards; Dokumen 01–14 FINAL

---

# 1. Informasi Dokumen

Manual ini adalah panduan administratif pra-implementasi. Struktur, batas kewenangan, prosedur konseptual, traceability, warning, dan release gate telah dikunci. Nama menu, contract yang belum tersedia, screenshot, angka policy, dan hasil UAT tetap menunggu bukti exact Release Candidate.

Dokumen pendukung:

- `ADMIN_MANUAL_DECISION_REGISTER.md`;
- `ADMIN_MANUAL_INTEGRATION_AUDIT.md`;
- `ADMIN_MANUAL_SCREENSHOT_REGISTER.md`.

---

# 2. Tujuan

Membantu Administrator, Support yang berizin, dan Auditor menjalankan tugas control-plane secara aman, mudah, dapat diuji, dan dapat diaudit tanpa memperoleh akses isi dokumen secara otomatis.

---

# 3. Target Pembaca

| Pembaca | Kebutuhan | Batas |
|---|---|---|
| Administrator | Account, permission, policy, Feature, limit, health | Hanya `ADMIN_SCOPE`; tanpa content access |
| Support/Superadmin profile | Issue triage, attachment eksplisit, note, export | Hanya `SUPPORT_SCOPE`; bukan role formal |
| Auditor | Query/detail/export Audit | Read-only dalam `AUDIT_SCOPE` |
| Reviewer/Trainer | Memvalidasi prosedur dan evidence | Tidak mendapat permission hanya karena membaca manual |

---

# 4. Scope

Termasuk: account/session, role/permission, Feature/policy/limit, 33 Core administration, Job/queue, Secured Files metadata, Support, Audit, monitoring, compatibility, configuration, backup-status boundary, troubleshooting, escalation trigger, screenshot, dan maintenance.

Tidak termasuk:

- penggunaan 33 tools oleh user — lihat Dokumen 14;
- severity/lifecycle/containment/recovery incident — lihat Dokumen 16;
- instalasi, deployment, migration, secret injection, dan infrastructure runbook — lihat Dokumen 17;
- direct SQL, source-code debugging, storage browsing, atau server command.

---

# 5. Prasyarat

```text
[ ] gunakan account pribadi, bukan shared account
[ ] koneksi tersedia
[ ] environment dan build tepat
[ ] permission dan delegated scope masih current
[ ] target, alasan, dampak, dan approval tersedia
[ ] SoD dan no self-elevation diperiksa
[ ] rollback yang benar-benar didukung diketahui
[ ] data yang ditampilkan tidak melewati scope
```

Jika salah satu prasyarat privileged mutation tidak jelas, berhenti sebelum menyimpan.

---

# 6. Konvensi

- `TBD_UI_LABEL` berarti nama UI belum dibuktikan.
- `WAITING_FOR_UI_VALIDATION` berarti prosedur belum diuji pada RC.
- `WAITING_FOR_API_CONTRACT` berarti action belum mempunyai contract resmi lengkap.
- `TARGET_TO_BE_APPROVED` berarti angka/nilai belum menjadi kebijakan resmi.
- `POLICY_CONFIGURABLE` berarti nilai berasal dari typed/versioned policy yang disetujui.
- Permission, Feature ID, Job ID, Correlation ID, status, endpoint, dan schema tidak diterjemahkan.

Template setiap prosedur:

```text
Tujuan | Actor/Permission | Platform | Prasyarat | Dampak
Langkah bernomor | Hasil | Audit | Warning
Rollback/no rollback | Jika gagal | Eskalasi | Traceability | Status UI/UAT
```

Semua prosedur baseline memerlukan koneksi. Label UI final: `WAITING_FOR_UI_VALIDATION`.

---

# 7. Batas Role Administrator

Role formal hanya `User`, `Administrator`, dan `Auditor`. Administrator mengelola control-plane serta metadata aman dalam `ADMIN_SCOPE`.

Administrator tidak otomatis menjadi User, Auditor, atau Support. Role bukan hasil otorisasi akhir. Setiap action tetap memerlukan:

```text
account/session
→ permission
→ delegated scope/ownership
→ policy dan resource state/version
→ platform/capability/connectivity
→ confirmation/reauthentication
→ ALLOW atau DENY
```

---

# 8. Administrative Access vs Content Access

```text
Administrative Access ≠ Document Content Access
Support Access        ≠ General User Document Access
Audit Access          ≠ Referenced Resource Content Access
Monitoring/DBA/Backup ≠ Business Content Permission
```

Administrator dapat melihat account/configuration/health metadata. Administrator tidak otomatis dapat membuka PDF user, mengunduh payload, membaca OCR/AI content, melihat password/token, mengambil visual signature, melihat clipboard, atau menelusuri object storage.

Tidak ada implicit impersonation, “view as user”, wildcard, atau break-glass content access pada baseline.

---

# 9. Pertamina SSO Overview

Pertamina Identity mengautentikasi pengguna. OTO mengelola eligibility aplikasi, application session, role, permission, scope, dan authorization.

OTO tidak:

- menyimpan atau mereset password Pertamina;
- mengganti MFA;
- melakukan enterprise account unlock/recovery;
- membuat identity provider paralel;
- menampilkan raw token/claim.

Protocol, claim allowlist, status mapping, reauth method, dan official help route berstatus `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

---

# 10. Login/Logout

**Actor:** seluruh principal berizin. **Permission:** sesuai session. **Audit:** `Auth.LoginSucceeded`, `Auth.LoginFailed`, dan logout/session event yang berlaku.

1. Buka aplikasi pada environment yang benar.
2. Pilih **`TBD_UI_LABEL: Login Pertamina`**.
3. Selesaikan autentikasi pada halaman Pertamina.
4. Periksa environment, profile, dan permission projection.
5. Setelah selesai, pilih **`TBD_UI_LABEL: Logout`**.
6. Tutup tab restricted dan export yang terbuka.

Logout mengakhiri session sesuai policy tetapi tidak menghapus source, draft, file, atau Audit. Masalah password/MFA diarahkan ke jalur Pertamina yang disetujui.

---

# 11. Reauthentication

**Actor:** principal yang melakukan action sensitif. **Permission:** permission action tetap wajib. **Risk:** HIGH-IMPACT. **Audit:** `Auth.ReauthenticationSucceeded` atau `Auth.ReauthenticationFailed`.

Reauthentication wajib untuk perubahan eligibility principal lain, revoke session lain, assign/revoke role/permission, dan high-impact security/classification/retention policy. Export atau perubahan broad dapat memerlukannya sesuai policy.

1. Periksa exact action, target, scope, version, dan dampak.
2. Pilih lanjut pada prompt **`TBD_UI_LABEL: Verifikasi Ulang`**.
3. Selesaikan autentikasi Pertamina.
4. Kembali ke aplikasi.
5. Periksa ulang permission, scope, policy, dan version.
6. Konfirmasi atau batalkan.

Reauthentication tidak menambah permission. Jika gagal, mutasi harus nol/atomic dan tetap fail-closed.

---

# 12. Admin Console Overview

**Status:** `WAITING_FOR_UI_VALIDATION`.

Area konseptual minimum:

| Area | Fungsi | Grant utama |
|---|---|---|
| Dashboard | Ringkasan environment, build, health | `monitoring.system.read` |
| User Management | Safe account/session metadata | `admin.user.list`, `admin.user.read`, `admin.user.update`, `admin.session.read`, `admin.session.revoke` |
| Role/Permission | Assignment dalam delegation | `admin.role.list`, `admin.role.read`, `admin.role.assign`, `admin.role.revoke`, `admin.permission.list`, `admin.permission.read`, `admin.permission.assign`, `admin.permission.revoke` |
| Feature Configuration | Exposure/flag/maintenance | `configuration.feature.read`, `configuration.feature.update` |
| Policy | Typed policy | `configuration.policy.read`, `configuration.policy.update` |
| Limits/Quota | Typed bounds | `configuration.limit.read`, `configuration.limit.update` |
| Jobs/Queue | Aggregate/read-only baseline | `monitoring.job.read` |
| Secured Files Metadata | Aggregate safe metadata | permission/contract resmi yang belum final |
| Issue/Support Console | Scoped Issue workflow | Permission tindakan Support yang tepat pada Bagian 35 |
| Audit | Read-only evidence | `audit.event.list`, `audit.event.read`, `audit.export.create`, `audit.export.read` |
| System Health | Aggregate operational health | `monitoring.system.read` |
| Configuration | Effective provenance/compatibility | Permission configuration yang tepat pada Bagian 61 |
| Help | Safe help/escalation reference | sesuai release |

Quick Start: login → periksa environment/build → buka System Health → periksa aggregate Job/queue → lakukan mutasi hanya jika kebutuhan, permission, target, dan rollback jelas → eskalasi bila jalur wajib unhealthy atau data tampak melewati scope.

---

# 13. Dashboard

Dashboard menampilkan environment, app/build/capability version, freshness, API/Worker/database/storage/queue/provider/Audit/Issue health, dan alert summary dalam bentuk agregat.

Dashboard tidak menampilkan document content, filename/path, raw user list, secret, connection string, provider endpoint, atau topology detail. Dashboard hijau tidak mengalahkan mandatory Audit/security failure.

Placeholder: `[SCREENSHOT-ADMIN-DASHBOARD-001]`.

---

# 14. User Search

**Actor/Permission:** Administrator — `admin.user.list`, `admin.user.read`. **Scope:** `ADMIN_SCOPE`. **Audit:** query/access evidence sesuai policy. **Status:** `WAITING_FOR_API_CONTRACT` dan `WAITING_FOR_UI_VALIDATION`.

1. Buka **`TBD_UI_LABEL: Administration › Accounts`**.
2. Masukkan identifier yang disetujui dan cukup spesifik.
3. Periksa scope indicator.
4. Pilih exact principal dari hasil scoped.
5. Cocokkan safe identity reference dan OTO status.

Jangan mencari berdasarkan isi file atau melakukan broad enumeration. Tidak ada hasil tidak membuktikan account tidak ada di scope lain.

---

# 15. Account Status

Empat lapisan wajib dipisahkan:

| Lapisan | Contoh | Arti |
|---|---|---|
| Identity Pertamina | status resmi upstream | Apakah identity dapat diautentikasi |
| OTO eligibility | `Active`, `Disabled` | Apakah principal boleh memakai OTO |
| Application session | `Active`, `Expired`, `Revoked` | Apakah session aplikasi berlaku |
| Effective access | `Allowed`, `Denied` + safe reason | Apakah exact action/resource boleh dilakukan |

Status `Unknown/Unavailable` bersifat fail-closed untuk mutasi. `Active` tidak berarti semua action diizinkan.

---

# 16. Account Activation/Deactivation

**Actor/Permission:** Administrator — `admin.user.update`. **Risk:** HIGH-IMPACT. **Reauth:** REQUIRED. **Audit:** `Administration.AccountEligibilityChanged`. **Status:** `WAITING_FOR_API_CONTRACT`.

1. Cari exact principal.
2. Periksa `ADMIN_SCOPE`, current eligibility, session, SoD, dan version.
3. Pilih **`TBD_UI_LABEL: Disable/Re-enable OTO Access`**.
4. Baca dampak terhadap login, session, queued work, dan offline grant.
5. Pilih reason code dan bounded reference.
6. Selesaikan reauthentication.
7. Konfirmasi exact target dan old/new state.
8. Simpan satu kali.
9. Baca ulang authoritative state.
10. Periksa mutation receipt; verifikasi Audit dilakukan reader yang berizin.

Disable tidak menghapus file atau Audit. Re-enable OTO bukan re-enable enterprise identity. Exact authority mengikuti `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

Rollback: inverse action hanya melalui review baru; session revoked tidak dapat “di-unrevoke”. Eskalasi bila status Pertamina tidak jelas, mutation ambiguous, atau user tetap memiliki privileged access setelah disable.

---

# 17. Role Assignment

**Actor/Permission:** Administrator — `admin.role.list`, `admin.role.read`, `admin.role.assign`, `admin.role.revoke`. **Reauth:** REQUIRED untuk mutation. **Audit:** `Administration.RoleAssigned` atau `Administration.RoleRevoked`. **Status:** `WAITING_FOR_API_CONTRACT`.

1. Cari exact principal dan buka **`TBD_UI_LABEL: Access › Roles`**.
2. Periksa current role, assignment source, scope, version, dan SoD.
3. Pilih hanya role canonical `User`, `Administrator`, atau `Auditor`.
4. Lihat delta permission dan deny/hard-policy effect.
5. Masukkan reason code/reference.
6. Pastikan tidak ada self-assignment, self-elevation, atau implicit `ALL`.
7. Selesaikan reauthentication.
8. Konfirmasi dan simpan dengan current version.
9. Verifikasi effective access dan receipt.

Rollback: revoke/assign balik adalah action baru yang memerlukan review. Role tidak menciptakan ownership atau content clearance.

---

# 18. Permission Assignment

**Actor/Permission:** Administrator — `admin.permission.list`, `admin.permission.read`, `admin.permission.assign`. **Reauth:** REQUIRED. **Audit:** `Administration.PermissionAssigned` atau `Administration.SupportGrantChanged`. **Status:** `WAITING_FOR_API_CONTRACT`; persistence direct assignment memerlukan addendum bila dipakai.

1. Buka exact principal pada **`TBD_UI_LABEL: Access › Permissions`**.
2. Periksa permission dari role/profile dan assignment terpisah.
3. Pilih kode canonical; jangan mengetik wildcard atau kode baru.
4. Pilih scope terkecil yang disetujui.
5. Periksa sensitivity, online-only, ownership, SoD, expiry, dan content flag.
6. Preview effective-access delta.
7. Masukkan reason code/reference.
8. Selesaikan reauthentication.
9. Konfirmasi target, permission, scope, dan version.
10. Simpan dan verifikasi authoritative projection.

Menu yang terlihat bukan bukti permission. Direct assignment tidak boleh dioperasionalkan sebelum contract/persistence resmi tersedia.

---

# 19. Permission Revocation

**Actor/Permission:** Administrator — `admin.permission.revoke`. **Reauth:** REQUIRED. **Audit:** `Administration.PermissionRevoked` atau `Administration.SupportGrantChanged`.

1. Pilih exact assignment dan sumbernya.
2. Jika berasal dari role, kelola role assignment; jangan membuat deny ad hoc.
3. Periksa assignment lain yang masih memberi permission.
4. Periksa session, offline grant, Job, dan action sensitif yang terdampak.
5. Masukkan reason code/reference.
6. Selesaikan reauthentication.
7. Simpan dengan current version.
8. Verifikasi effective access.
9. Revoke session melalui action terpisah bila policy mewajibkan.

Revocation tidak menghapus file, Job, Issue, atau Audit. Cached UI bukan authority setelah server menolak.

---

# 20. Effective Permission

Effective permission adalah grant aktif setelah deny, SoD, scope, ownership, policy, resource state/version, platform/capability, connectivity, confirmation, dan reauthentication diterapkan.

```text
Role/profile/direct assignment
→ current permission set
→ deny + SoD + delegation
→ resource scope/ownership
→ policy/state/version/context
→ effective ALLOW/DENY
```

Jika access denied, periksa semua dimensi. Jangan menaikkan role atau mengubah owner sebagai jalan pintas. Safe explanation view tetap `WAITING_FOR_API_CONTRACT`.

---

# 21. Feature Configuration

**Actor/Permission:** `configuration.feature.read` dan `configuration.feature.update`. **Audit:** `Configuration.FeatureExposureChanged`. **Risk:** mutation dapat HIGH-IMPACT.

Feature Registry Admin memisahkan:

- read-only: Feature ID, meaning, contract/schema, risk, destructive flag, execution profile, server dependency, deployed platform maximum;
- editable bila resmi: enabled/disabled, scoped flag, effective exposure yang mempersempit maximum, maintenance, typed policy/limit profile.

Admin tidak membuat Feature ID, mengubah schema, memilih engine/vendor, mengubah risk, atau menandai release tanpa build/test/UAT evidence.

---

# 22. Feature Enable/Disable

**Actor/Permission:** `configuration.feature.update`. **Reauth:** policy/high-impact. **Audit:** `Configuration.FeatureExposureChanged`.

1. Cari exact Feature ID dan environment.
2. Periksa release status, deployed maximum, current effective state, dependency, capacity, dan active Job impact.
3. Pilih scope terkecil.
4. Masukkan reason code/reference dan window bila tersedia.
5. Preview old/new state, platform, workflow, Job, dan fallback impact.
6. Selesaikan reauthentication bila broad/high-impact.
7. Simpan dengan `If-Match`/row version.
8. Baca ulang effective state.
9. Verifikasi pada account/platform uji dan periksa Audit.

`Enabled` tidak berarti released, visible, authorized, compatible, atau healthy. Disable tidak mengubah Job state langsung. Jika transition behavior active Job belum defined, broad change diblok.

---

# 23. Feature Flags

Feature flag hanya untuk staged rollout, pilot, QA, scoped exposure, atau emergency disable.

Field minimum: Feature ID, environment, typed scope/platform, state, reason, effective window, schema/policy version, row version, owner/reviewer, dan last changed time.

Flag dilarang mengubah business meaning, schema, permission, security control, Local-First route, provider arbitrer, atau release evidence. Setiap flag mempunyai owner, review/expiry, dan exit decision.

---

# 24. Platform Exposure

Nilai exposure: `FULL`, `PRIMARY`, `SUPPORTED`, `LIMITED`, `NOT_EXPOSED`.

1. Pilih Feature ID, platform, environment, dan release channel.
2. Bandingkan effective exposure dengan deployed maximum/capability manifest.
3. Jangan menaikkan exposure tanpa implementation, platform test, UAT, documentation, dan RC evidence.
4. Preview File-First, Tool-First, Assistant-First, Help, localization, dan fallback.
5. Simpan typed/versioned change.
6. Verifikasi pada exact platform/build.

Exposure bukan permission dan tidak dapat memaksa capability yang tidak ada.

---

# 25. Policy Management

**Actor/Permission:** `configuration.policy.read` dan `configuration.policy.update`. **Reauth:** REQUIRED untuk security/classification/retention. **Audit:** `Policy.SecurityPolicyChanged`, `Policy.ClassificationChanged`, atau `Policy.RetentionPolicyChanged` sesuai jenis perubahan.

1. Pilih registered typed policy key.
2. Periksa environment, scope, schema/version, source, current value, dan hard bound.
3. Lihat affected Feature/platform/workload/provider.
4. Masukkan typed value, reason code, approved reference, dan effective time bila didukung.
5. Preview safe diff/digest.
6. Selesaikan reauthentication.
7. Simpan dengan `If-Match`; pada `409/412`, reload dan review ulang.
8. Verifikasi effective state, health, dan Audit.

Policy tidak dapat mematikan permission/ownership, file validation/quarantine, mandatory Audit, secret/content exclusion, public-AI block, no-silent-overwrite, Local-First, selective sync, maximum-three, atau hard safety ceiling.

---

# 26. Limit/Quota Management

**Actor/Permission:** `configuration.limit.read` dan `configuration.limit.update`. **Audit:** `Configuration.LimitChanged`. **Nilai:** `TARGET_TO_BE_APPROVED` atau `POLICY_CONFIGURABLE`.

Kategori: upload size, file/page count, concurrent Job, queue, secured storage, AI/OCR/conversion, retry, timeout, attachment/export.

1. Pilih typed limit key.
2. Periksa unit, configured/effective value, hard ceiling, scope, version, dan headroom.
3. Masukkan nilai baru dalam approved range.
4. Tampilkan old/new value, dampak, reason, dan rollback.
5. Reauthenticate bila broad/high-impact.
6. Simpan dengan row version.
7. Verifikasi admission/backpressure/health dan Audit.

Manual tidak menetapkan angka resmi. Maksimum tiga secured versions bukan quota yang dapat dinaikkan.

---

# 27. 33 Core Tool Administration

Current state seluruh Core: **BELUM TERSEDIA** sampai exact RC, platform exposure, dan UAT evidence tersedia. Tabel ini adalah posture administrasi, bukan cara memakai tool.

| # | Core Tool | Canonical Feature ID | Posture | Fokus admin |
|---:|---|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | Local | Exposure, file/count/temp; server queue tidak relevan |
| 2 | Split PDF | `FEAT-PDF-006/007` | Local | Page/range limit, exposure |
| 3 | Remove pages | `FEAT-PDF-013` | Local | Exposure, source/finality guard |
| 4 | Extract pages | `FEAT-PDF-014` | Local | Page/output count |
| 5 | Organize PDF | `FEAT-PDF-010..016` | Local workflow | Member mapping dan platform subset |
| 6 | Scan to PDF | `FEAT-SCAN-014` | Local/device | Camera/scanner capability dan fallback |
| 7 | Compress PDF | `FEAT-PDF-023` | Local | Size/temp limit; no target-size promise |
| 8 | Repair PDF | `FEAT-PDF-025/026` | Local + optional server | Fallback/provider health |
| 9 | OCR PDF | `FEAT-OCR-004` | Local-first + optional server | Language, resource, provider quota |
| 10 | JPG to PDF | `FEAT-CONV-007` | Local | File/count/output limit |
| 11 | Word to PDF | `FEAT-CONV-001` | Optional server | Provider/version/queue/timeout |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | Optional server | Provider/version/queue/timeout |
| 13 | Excel to PDF | `FEAT-CONV-002` | Optional server | Sheet/page/provider health |
| 14 | HTML to PDF | `FEAT-CONV-017` | Optional server | Isolation/remote-resource policy/timeout |
| 15 | PDF to JPG | `FEAT-CONV-012` | Local | Page/output size |
| 16 | PDF to Word | `FEAT-CONV-004` | Optional server | Fidelity/provider/queue |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | Optional server | Fidelity/provider/queue |
| 18 | PDF to Excel | `FEAT-CONV-005` | Optional server | Table fidelity/output limit |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | Optional server | Profile/validator; no false conformance |
| 20 | Rotate PDF | `FEAT-PDF-012` | Local | Exposure/page limit |
| 21 | Add page numbers | `FEAT-PDF-037` | Local | Page/range/output limit |
| 22 | Add watermark | `FEAT-PDF-036` | Local | Resource limit; bukan DRM/redaction |
| 23 | Crop PDF | `FEAT-PDF-019` | Local | Exposure; bukan redaction |
| 24 | Edit PDF | `FEAT-PDF-031..041` | Local + optional subset | Exact released subset/fidelity |
| 25 | PDF Forms | `FEAT-PDF-053..060` | Local workflow | Member availability/flatten/platform |
| 26 | Unlock PDF | `FEAT-PDF-062` | Local | Valid password/authority; password invisible |
| 27 | Protect PDF | `FEAT-PDF-061` | Local | High-risk policy; password tidak disimpan |
| 28 | Sign PDF | `FEAT-SIGN-013` | Local visual non-PKI | Non-PKI label; asset tetap lokal |
| 29 | Redact PDF | `FEAT-PDF-069` | Local | True removal validation; bukan overlay |
| 30 | Compare PDF | `FEAT-PDF-071/072` | Local/Local Worker | Worker/resource/platform health |
| 31 | AI Summarizer | `FEAT-INTEL-001` | Local preferred + approved internal | Public AI blocked; quota/timeout/circuit |
| 32 | Translate PDF | `FEAT-INTEL-002` | Local preferred + approved internal | Language/provider/fidelity; no public endpoint |
| 33 | PDF to Markdown | `FEAT-CONV-036` | Local-first + optional OCR/layout | OCR dependency/output limit |

Setelah perubahan, verifikasi Feature ID mapping, release state, deployed maximum, provider/engine build, permission, policy, limit, compatibility, no server traffic untuk local-only, dan safe Audit event.

---

# 28. Provider/Capability Status

Admin hanya melihat approved provider/capability profile code, status, freshness, affected Feature, safe reason, compatibility, quota/circuit summary, dan maintenance state.

Admin tidak melihat endpoint, credential, topology, raw response, document content, atau memilih vendor. Status:

- available/healthy sesuai typed release schema;
- limited/degraded dengan safe reason;
- unavailable/maintenance;
- unknown/stale — jangan dianggap sehat.

Jika provider gagal, pertahankan local path yang sah. Jangan beralih ke public AI/provider.

---

# 29. Job Administration

**Actor/Permission:** Administrator — `monitoring.job.read`. **Baseline:** read-only aggregate/safe projection.

Status canonical:

```text
QUEUED
RUNNING
WAITING_CONNECTION
WAITING_RESOURCE
RETRYING
COMPLETED
FAILED
CANCELLED
```

Admin dapat mencari Job ID, Correlation ID, Feature ID, status, safe Error Code, duration, wait reason, processing mode, dan masked owner reference bila scope mengizinkan. Admin tidak melihat document content, filename/path, password, OCR text, signature, result bytes, checkpoint token, atau storage reference.

Job ID dan Correlation ID bukan permission. REST/persisted state selalu berwenang.

---

# 30. Queue Administration

Baseline menampilkan aggregate depth, oldest age, arrival/service/drain rate, retry, workload, capacity/resource, provider dependency, dan freshness.

Tindakan aman yang mungkin tersedia melalui typed policy:

- mempersempit admission mahal;
- menurunkan concurrency dalam approved range;
- mempertahankan local-capable route;
- menjalankan lifecycle cleanup resmi;
- memberi wait/retry guidance;
- eskalasi ke owner resmi.

Dilarang menghapus row, lease, priority, Job, atau queue melalui database; dilarang retry massal tanpa bound.

---

# 31. Retry/Cancel

Dokumen 06 menyediakan owner/permission-based Job cancel/retry/resume, tetapi Dokumen 09 tidak memberi Administrator generic cross-owner action. Karena itu baseline Admin **monitoring-only**.

Jika kelak action resmi dirilis:

1. Periksa permission khusus, owner/scope, current state/version, idempotency, policy, dan impact.
2. Gunakan application/API resmi.
3. Reauthorize sebelum continuation.
4. Cancel bersifat cooperative dan menunggu terminal state.
5. Retry memakai bounded attempt dan logical Job ID yang sama.
6. Resume hanya dari compatible verified checkpoint.
7. Periksa Audit event yang berlaku.

Sampai permission/API cross-owner resmi ada: `WAITING_FOR_API_CONTRACT`; jangan tampilkan tombol action.

---

# 32. Secured Files Metadata

```text
SQL Server              → metadata
Secure file/object store→ payload
```

Baseline Administrator melihat aggregate quota, version count, storage status, error/orphan/cleanup count, timestamp, dan retention category yang aman. Detail per-file lintas owner memerlukan permission/scope/API terpisah yang belum tersedia.

Admin tidak membuka, mengunduh, mempreview, atau menelusuri payload/object key. Owner reference, size, dan filename adalah confidential metadata dan harus diminimalkan/masking.

---

# 33. Secured Version Management

Maksimum tiga versi aktif/addressable adalah invariant.

Admin dapat memantau aggregate:

- active version distribution;
- rotation/cleanup success;
- failed upload/finalization;
- metadata/object mismatch;
- orphan reconciliation status.

Admin tidak menghapus object atau mengubah current pointer melalui SQL/storage. Version count salah atau object mismatch adalah integrity signal dan harus dieskalasikan. Restore content tetap owner/action-specific, bukan admin convenience.

---

# 34. Storage Quota

Quota menampilkan configured/effective value, hard ceiling, usage aggregate, trend, freshness, scope, policy version, dan warning threshold source.

Nilai belum resmi berstatus `TARGET_TO_BE_APPROVED`. Perubahan mengikuti Bagian 26. Ketika mendekati batas, batasi admission content baru sebelum unsafe, jalankan cleanup lifecycle resmi, lindungi active Job/result, dan eskalasi. Jangan menghapus file user manual.

---

# 35. Issue/Support Console

Support/Superadmin adalah profile scoped, bukan role keempat.

| Action | Permission |
|---|---|
| List | `support.issue.list` |
| Detail | `support.issue.read` |
| Attachment eksplisit | `support.attachment.read` |
| Triage/status | `support.issue.triage` |
| Note | `support.note.read`, `support.note.create` |
| Export | `support.export.create`, `support.export.read` |

1. Login dengan account personal.
2. Buka **`TBD_UI_LABEL: Support/Issue Console`**.
3. Periksa environment, queue/unit/assignment, dan date range.
4. Cari/filter Issue dengan field typed yang tersedia.
5. Buka exact scoped Issue.

Support hanya melihat Issue message, safe diagnostics, dan exact attachment yang dikirim user sesuai permission. Tidak ada general document access atau “view as user”.

---

# 36. Issue Triage

**Actor/Permission:** Support — `support.issue.triage`. **Audit:** `Support.IssueTriaged` dan event typed terkait.

1. Buka exact Issue dan current version.
2. Baca pesan serta safe diagnostics.
3. Buka attachment hanya bila perlu dan berizin.
4. Cari duplicate dalam scope.
5. Pilih category/priority typed yang tersedia.
6. Pilih allowed status transition.
7. Tambahkan note singkat bila diperlukan.
8. Simpan dengan current version.
9. Verifikasi outcome.

Pada version conflict, refresh dan nilai ulang. Jangan overwrite. `DUPLIKAT`, `BUKAN_BUG`, dan `BUTUH_INFORMASI` memerlukan alasan operasional yang aman dan traceable.

---

# 37. Issue Status

Status internal canonical:

```text
BARU
DIPERIKSA
PERLU_PERBAIKAN
BUTUH_INFORMASI
DUPLIKAT
BUKAN_BUG
SEDANG_DIPERBAIKI
READY_FOR_TEST
TERVERIFIKASI
SELESAI
```

Gunakan `SELESAI` untuk resolved/closed. Jangan membuat enum `RESOLVED` atau `CLOSED`. Exact transition/reopen rules dan category/priority catalog: `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 38. Internal Notes

**Actor/Permission:** `support.note.read` dan `support.note.create`. **Audit:** `Support.InternalNoteAdded` tanpa note text.

Format aman:

```text
waktu + observasi singkat
safe Issue/Feature/Job/Correlation/Error reference
pemeriksaan dan hasil aman
tindakan berikutnya/owner reference
```

Jangan menulis password, token, secret, document/OCR content, signature, clipboard, raw stack/SQL/body/path/provider response, attachment copy, atau detail forensik. Note bukan Incident Record.

---

# 39. Issue Export

**Actor/Permission:** `support.export.create` dan `support.export.read`. **Reauth:** policy/high-impact. **Audit:** `Support.ExportRequested`, `Support.ExportCompleted`, dan `Support.ExportDownloaded`.

1. Pilih exact Issue untuk PDF atau bounded filter untuk Excel.
2. Periksa scope, fields, attachments, date range, dan expected count/status.
3. Selesaikan reauthentication bila diminta.
4. Buat export satu kali.
5. Tunggu background Job selesai.
6. Ambil artifact sebelum expiry dengan current authorization.
7. Simpan/bagikan/hapus sesuai policy.

Export privat, data-minimized, tidak menggunakan public durable link, dan aman dari formula/macro/external-link abuse. Truncated/failed export tidak boleh dianggap lengkap.

---

# 40. Audit Search

**Actor/Permission:** Auditor atau grant terpisah — `audit.event.list`. **Scope:** `AUDIT_SCOPE`. **Mode:** read-only.

1. Buka **`TBD_UI_LABEL: Audit Console`**.
2. Pilih environment dan bounded `[from,to)` date range.
3. Pilih allowed filter: Actor, Event Category/Type, Feature ID, Outcome, Resource, Job ID, Correlation ID, Platform/Source.
4. Jalankan query.
5. Persempit jika terlalu luas.

Tidak ada raw SQL, regex, arbitrary JSON path, filename/document/OCR/Issue/prompt search. Scope diterapkan sebelum count/filter/cursor.

---

# 41. Audit Detail

**Permission:** `audit.event.read`.

Detail aman dapat mencakup Event ID/type/version, actor/source, time, action, resource opaque, outcome, Feature/Job/Correlation, environment/platform/version, policy/reason/error code yang aman.

Event reference bukan grant ke document, result, Secured File, Issue, atau Assistant content. Auditor tidak edit/delete event atau mengubah user/configuration/retention.

---

# 42. Audit Export

**Permission:** `audit.export.create` dan `audit.export.read`. **Reauth:** policy/high-impact. **Audit:** `AuditAccess.ExportRequested`, `AuditAccess.ExportCompleted`, `AuditAccess.ExportDownloaded`, dan `AuditAccess.ExportExpired`.

1. Jalankan query authorized dan bounded.
2. Periksa date/category/unit/environment scope serta field projection.
3. Buat export.
4. Tunggu background extraction.
5. Ambil sebelum expiry dengan current authorization.
6. Simpan/bagikan/dispose sesuai classification/retention.

Baseline detail: UTF-8 CSV. Fixed PDF opsional bila template disetujui. XLSX/raw JSON/database dump bukan baseline Audit. Query/export sendiri menghasilkan audit-of-audit yang aman.

---

# 43. Monitoring

**Permission:** `monitoring.system.read`, `monitoring.job.read`. Monitoring adalah safe aggregate technical health, bukan Enterprise Audit atau content plane.

Minimum:

- API availability/latency/error;
- Worker availability/job duration;
- database readiness/response/pool;
- storage readiness/capacity/transfer;
- queue depth/age/drain/retry;
- provider/internal AI availability;
- SignalR/reconnect;
- Audit pipeline;
- Issue service/backlog;
- deployed/compatible versions.

Semua nilai menampilkan timestamp/freshness, environment, safe reason, dan policy threshold source bila tersedia.

---

# 44. System Health

| Status | Arti |
|---|---|
| `SEHAT` | Sinyal segar dan dalam approved bounds |
| `PERINGATAN` | Deviasi awal; fungsi utama masih berjalan |
| `TERBATAS` | Capability/throughput/recovery sebagian terganggu tetapi state aman |
| `TIDAK TERSEDIA` | Komponen tidak ready atau telemetry tidak dapat dipercaya |
| `MEMERLUKAN TINDAKAN` | Badge terpisah: pemeriksaan/eskalasi dibutuhkan |

Telemetry hilang, stale, atau unknown ditampilkan `TIDAK TERSEDIA` + `MEMERLUKAN TINDAKAN`, bukan `SEHAT`. Mapping internal component state tidak diubah oleh label dashboard.

---

# 45. Alert Handling

Setiap alert memuat arti, dampak, safe checks, immediate safe action, forbidden action, evidence, dan escalation trigger.

| Alert | Pemeriksaan awal | Tindakan aman | Dilarang |
|---|---|---|---|
| Queue penuh | Depth/age/drain/resource/provider | Persempit admission; pertahankan local route | Retry massal/delete row |
| Storage mendekati batas | Growth/cleanup/orphan/active lease | Stop unsafe admission; bounded cleanup | Hapus file user manual |
| Worker tidak aktif | Heartbeat/crash/lease/version/resource | Hentikan dependent admission; eskalasi | Ubah Job state/kill sembarang |
| Provider gagal | Affected Feature/circuit/timeout/fallback | Maintenance affected path; local if valid | Public provider fallback |
| Audit queue tertunda | Intent/outbox/backlog/health | Lindungi privileged mutation; eskalasi | Disable Audit |
| Client unsupported | Current/minimum/contract/capability | Inform update; staged policy review | Paksa tanpa security/compat reason |
| Database not ready | Readiness/pool/latency/storage | Kurangi admission; eskalasi | Direct production data edit |

Severity, owner, threshold, on-call, dan containment detail berada di Dokumen 16/policy resmi.

---

# 46. Version Compatibility

Status: `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, `UNSUPPORTED`.

Admin melihat `CurrentVersion`, `MinimumSupportedVersion`, `ContractVersion`, dan `CapabilityVersion`.

- Current/Contract/Capability adalah release fact read-only.
- Minimum Supported Version adalah typed policy.
- Forced update hanya untuk kebutuhan security/compatibility nyata.
- Perubahan broad memerlukan impact preview, staged validation, reauthentication, rollback, Audit `Configuration.MinimumClientVersionChanged`, dan post-change monitoring.

---

# 47. Configuration Management

Composition view:

```text
Deployed Default
→ Environment Configuration
→ Enterprise Policy / approved Feature Flag
→ Runtime Capability and resource health
→ Effective value
```

Restriction priority dari Dokumen 04:

```text
System Safety/Security
→ Enterprise Policy
→ Environment Configuration
→ Device Configuration
→ User Preference
```

Layer bawah tidak mengalahkan layer pembatas di atas; most-restrictive safe result menang. Admin hanya mengubah typed exposed configuration melalui console/API resmi. Source code, production DB row, server config file, secret store, dan package version tidak diedit dari Admin Manual.

---

# 48. Environment Boundary

| Environment | Tujuan | Aturan |
|---|---|---|
| Development | Pengembangan | Synthetic data; bukan Production control |
| QA/Test | Integration/UAT/security test | Isolated account/config/data/evidence |
| Production | Layanan resmi | Approved artifact, authorization, no debug detail |

Data Production tidak disalin sembarangan ke Development. Perubahan Production harus melalui authorization dan application/control-plane resmi. Deployment command, topology, migration, dan secret handling berada di Dokumen 17.

---

# 49. Security Guidance

Administrator wajib:

- memakai account sendiri dan tidak berbagi session;
- menerapkan least privilege;
- memeriksa target/scope/version/dampak;
- menyelesaikan reauthentication sendiri;
- tidak memasukkan secret/content ke note, Issue, screenshot, Audit, atau export;
- menyimpan export restricted pada lokasi resmi;
- logout dari perangkat bersama;
- mengeskalasi suspicious access tanpa mencoba exploit;
- tidak menjalankan direct SQL/storage/Worker/provider call;
- tidak menonaktifkan Audit, validation, rate limit, quarantine, atau public-AI block.

Sign PDF tetap visual non-PKI. PKI/certificate tetap di luar scope.

---

# 50. Backup/Restore Overview

Admin Manual hanya menjelaskan:

- melihat status backup terakhir dan verification state;
- melihat status restore request/operation bila permission tersedia;
- memahami bahwa restore berdampak lintas SQL/object/version/tombstone/permission;
- memastikan maksimum tiga versi dan authorization tetap direkonsiliasi;
- meminta/menjalankan restore hanya melalui prosedur resmi dan permission yang disetujui.

Admin tidak menjalankan script database/object-storage dari manual ini. RPO/RTO, topology, command, recovery point, key, dan infrastructure restore berada di Dokumen 16/17 serta kebijakan Pertamina.

---

# 51. Troubleshooting

| Gejala | Pemeriksaan aman | Tindakan admin | Eskalasi |
|---|---|---|---|
| User tidak dapat login | Bedakan SSO, eligibility, session | Re-enable OTO hanya bila resmi; relogin | Password/MFA/identity issue |
| Feature tidak terlihat | Release/exposure/permission/policy/capability/version | Koreksi control yang benar | Implementation/exposure absent |
| Permission belum berlaku | Assignment source/scope/version/session | Refresh/relogin; verify effective | Invalidation/SoD unclear |
| Account disabled/revoked | Layer status exact | Pertahankan deny; official process | Upstream status unclear |
| Job menunggu lama | Status/reason/queue/provider/resource | No-silent-wait; bounded capacity action | Backlog tidak drain |
| Queue penuh | Depth/age/drain/headroom | Narrow admission/concurrency | Capacity/Worker/DB owner |
| Provider unavailable | Profile/circuit/affected Feature | Maintenance affected path; local if valid | Provider/security owner |
| Upload gagal | 413/408/409/429/503, temp/storage/network | Correct input/policy; official resume | Repeated integrity/capacity failure |
| Secured file gagal | Transfer/version/quota/object metadata | Retry official flow; no manual object edit | Mismatch/orphan/integrity |
| Issue tidak masuk | Submission/idempotency/service/outbox | Retry same logical action if safe | Issue service failure |
| Audit event terlambat | Owning commit/outbox/ingestion/health | Check receipt; authorized Audit review | Mandatory intent not durable |
| Monitoring warning | Freshness/trend/dependency/reason | Safe action from alert | Critical/unknown persists |
| Client unsupported | Four version values | Inform update/review policy | Unexpected broad block |
| Realtime putus | REST current state | Refresh/reconnect | REST also unavailable |

Evidence minimum: time, environment, component/version, safe Issue/Alert/Event/Feature/Job/Correlation/Error/status reference, impact, dan tindakan aman. Jangan lampirkan content/secret/raw diagnostic.

---

# 52. Escalation

Eskalasi bila:

- cross-user/cross-scope atau unauthorized privileged access terindikasi;
- secret/document/OCR/signature/restricted screenshot bocor;
- public/unapproved AI route diduga berhasil;
- mandatory Audit tidak durable atau integrity/hold terancam;
- corruption, silent loss, false success, atau duplicate destructive effect terjadi;
- API/Worker/database/storage/queue/provider/Issue service tidak pulih;
- broad export/config/permission berubah tanpa authority;
- monitoring jalur kritis unavailable/stale;
- safe recovery tidak jelas atau dampak meluas.

Handoff hanya membawa safe evidence. Severity, contact, on-call route, containment, communication, forensic handling, dan recovery procedure ditetapkan Dokumen 16; manual ini tidak mengarangnya.

---

# 53. Common Error Codes

| Code/status | Arti aman | Tindakan |
|---|---|---|
| `401` / `REAUTHENTICATION_REQUIRED` | Authentication/freshness tidak cukup | Login/reauth resmi; jangan meminta credential |
| `403` | Permission/scope/policy menolak | Periksa effective access; jangan bypass |
| `404` | Tidak ada atau concealed | Periksa scope/ID tanpa enumeration |
| `409` | State/resource/quota conflict | Refresh state; jangan force overwrite |
| `412` | ETag/version stale | Reload diff lalu submit ulang bila masih tepat |
| `413` | Hard size/count limit | Kurangi input; ubah policy hanya dengan approval |
| `429` | Caller quota/rate/concurrency | Ikuti `Retry-After`; hindari retry storm |
| `503` | Shared readiness/capacity unsafe | Tunggu/eskalasi; pertahankan local capability |
| `ONLINE_REQUIRED` | Action tidak boleh offline | Sambungkan lalu refresh authorization |
| `JOB_NOT_CANCELLABLE` | Job tidak aman untuk cancel | Tunggu current state/safe point |
| `RESULT_EXPIRED` / `410` | Result melewati lifecycle | Rerun bila input/permission masih sah |

Exact release Error Catalog tetap harus divalidasi. UI hanya menampilkan stable code, pesan lokal, safe details, dan Correlation ID—tanpa stack/SQL/path/secret.

---

# 54. FAQ

**Mengapa Administrator tidak dapat membuka file user?** Karena administrative access berbeda dari content access.

**Mengapa menu terlihat tetapi action ditolak?** UI bukan authority; permission, scope, policy, state, version, dan reauth masih diperiksa server.

**Mengapa permission belum berlaku?** Session/projection mungkin stale atau deny/scope/SoD masih membatasi. Refresh secara resmi; jangan memberi role lebih tinggi.

**Apakah Superadmin role tertinggi?** Tidak. Ia label profile Support yang scoped.

**Mengapa Admin tidak melihat Audit?** Audit membutuhkan grant terpisah atau role Auditor.

**Bolehkah Admin cancel Job user?** Tidak pada baseline. Monitoring-only sampai permission/API cross-owner resmi tersedia.

**Mengapa hanya tiga secured versions?** Itu invariant bisnis, bukan quota yang dapat dinaikkan.

**Apa beda History, Audit, Log, dan Issue?** History untuk user; Audit untuk bukti enterprise; Log untuk health teknis; Issue untuk Support.

**Kapan eskalasi?** Saat ada security/integrity/content leak, mandatory control failure, outage berlanjut, atau safe recovery tidak jelas.

---

# 55. Glossary

| Istilah | Arti |
|---|---|
| `ADMIN_SCOPE` | Metadata/control plane dalam delegasi Administrator |
| `SUPPORT_SCOPE` | Exact queue/unit/assignment/Issue yang disetujui |
| `AUDIT_SCOPE` | Evidence unit/waktu/category/platform yang disetujui |
| Effective permission | Permission setelah deny/scope/policy/context |
| Reauthentication | Pembuktian identity fresh; bukan elevation |
| OTO eligibility | Kelayakan memakai aplikasi, bukan status enterprise identity |
| Control plane | Account/configuration/policy/metadata operasional |
| Safe metadata | Data minimum tanpa content/secret/raw diagnostic |
| Feature exposure | Visibilitas/capability dalam deployed maximum |
| Hard ceiling | Batas keselamatan yang tidak dapat dilewati admin |
| Secured File | File yang dipilih eksplisit melalui Amankan ke Akun |
| Correlation ID | Benang aktivitas; bukan kunci akses |
| Audit receipt | Bukti aman bahwa mutation diterima/hasil tersedia |
| Release Candidate | Build kandidat yang hampir siap rilis dan dapat divalidasi |

---

# 56. Screenshot Register

Register penuh: `ADMIN_MANUAL_SCREENSHOT_REGISTER.md`.

Placeholder minimum:

```text
[SCREENSHOT-ADMIN-DASHBOARD-001]
[SCREENSHOT-USER-MANAGEMENT-001]
[SCREENSHOT-PERMISSION-001]
[SCREENSHOT-FEATURE-CONFIG-001]
[SCREENSHOT-JOB-QUEUE-001]
[SCREENSHOT-SUPPORT-CONSOLE-001]
[SCREENSHOT-AUDIT-SEARCH-001]
[SCREENSHOT-SYSTEM-HEALTH-001]
```

Actual screenshot: 0. Jangan memakai mockup sebagai screenshot. Seluruh capture memakai dummy/sanitized non-Production data dan exact approved RC.

---

# 57. Known Limitations

- Admin UI/menu/labels belum tersedia.
- Management/session/effective-access contract belum lengkap.
- Exact direct permission/Support grant persistence belum final.
- Admin cross-owner Job cancel/retry/resume belum diizinkan.
- Per-file Secured Files metadata lintas owner belum diizinkan.
- Platform release exposure 33 Core belum tersedia.
- Numeric limit/threshold/timeout/retention/RPO/RTO belum disetujui.
- SSO mapping, reauth freshness, delegation/SoD, Support/Audit scope belum dikonfirmasi.
- Category/priority/transition/reopen Issue belum final.
- Alert owner/contact/escalation route belum ditetapkan.
- Actual screenshot dan documentation test belum ada.
- UAT specification FINAL, tetapi execution belum direkam.

---

# 58. Document Maintenance

Trigger revisi: UI/label, role/permission/profile, scope/delegation, API/schema, policy/limit, Feature exposure, Job state/action, Audit catalog/export, Issue workflow, monitoring/health, compatibility, security/reauth, escalation route, localization, UAT defect, atau build baru.

```text
change detected
→ impact record
→ update affected procedure/permission/screenshot/link/test
→ role/security/UI review
→ task-based documentation test
→ UAT/known-issue reconciliation
→ approval + version/release note
```

Lifecycle: `DRAFT_CONTENT → WAITING_FOR_UI → WAITING_FOR_SCREENSHOT → SECURITY_REVIEWED → UAT_VALIDATED → RELEASE_READY → SUPERSEDED/RETIRED`.

---

# 59. Requirement Traceability

| Area | Requirement |
|---|---|
| SSO/account/session | `FR-AUTH-001..005` |
| Role/permission | `FR-ROLE-001..003` |
| Secured Files/version | `FR-BACKUP-001..010`, `FR-VER-001..004` |
| Audit | `FR-AUDIT-001..004` |
| Support | `FR-SUP-001..007` |
| Security/privacy | `SR-SEC-001..023` |
| API/admin/support | `IR-API-001..012` |
| Monitoring | `NFR-MON-001..004` |
| Performance/limits | `NFR-PERF-006..013` |
| Recovery | `FR-REC-001..006`, `NFR-REC-001..005` |

---

# 60. Feature Traceability

| Area | Feature |
|---|---|
| Account/session | `FEAT-AUTH-001..012` |
| Administration | `FEAT-ADMIN-001..022` |
| Audit | `FEAT-AUDIT-001..008` |
| Monitoring | `FEAT-MON-001..017` |
| Support | `FEAT-SUPPORT-001..008` |
| Secured/version | `FEAT-BACKUP-001..013`, `FEAT-VER-001..006` |
| Job/progress/recovery | `FEAT-PROG-001..009`, `FEAT-REC-001..015` |
| 33 Core | Dokumen 03/13 dan Bagian 27 |

Tidak ada Feature ID baru. Baseline tetap 550.

---

# 61. Permission Traceability

| Domain | Permission canonical |
|---|---|
| Account/session | `admin.user.list`, `admin.user.read`, `admin.user.update`, `admin.session.read`, `admin.session.revoke` |
| Role | `admin.role.list`, `admin.role.read`, `admin.role.assign`, `admin.role.revoke` |
| Permission | `admin.permission.list`, `admin.permission.read`, `admin.permission.assign`, `admin.permission.revoke` |
| Configuration | `configuration.policy.read`, `configuration.policy.update`, `configuration.feature.read`, `configuration.feature.update`, `configuration.limit.read`, `configuration.limit.update`, `configuration.compatibility.read`, `configuration.compatibility.update` |
| Monitoring | `monitoring.system.read`, `monitoring.job.read` |
| Support | `support.issue.list`, `support.issue.read`, `support.issue.triage`, `support.attachment.read`, `support.note.read`, `support.note.create`, `support.export.create`, `support.export.read` |
| Audit | `audit.event.list`, `audit.event.read`, `audit.export.create`, `audit.export.read` |

Job/secured content cross-owner permission tidak dibuat. Permission manusia tidak mengimplikasikan workload permission.

---

# 62. Audit Traceability

| Tindakan | Expected event family |
|---|---|
| Eligibility/role/permission/session | `Administration.*` |
| Policy/security/classification/retention | `Policy.*Changed` |
| Feature/limit/maintenance/minimum version | `Configuration.*Changed` |
| Job accepted/terminal/retry | `Processing.*` |
| Secured lifecycle | `SecuredFile.*` |
| Support view/triage/note/export | `Support.*` |
| Audit query/detail/export | `AuditAccess.*` |
| Reauthentication | `Auth.Reauthentication*` |

Event menggunakan actor, target opaque, action, scope/version, outcome, reason/error code aman, time, environment, Feature/Job/Correlation bila relevan. Event tidak menyimpan password/token/content/OCR/signature/clipboard/note/screenshot bytes/raw diagnostics.

---

# 63. Admin Acceptance Checklist

## Baseline documentation

```text
[x] Documents 01–14 and 4/4 specialist reports audited
[x] 63/63 required topics
[x] 40/40 documentation decisions AUTO-LOCKED
[x] 33/33 Core administration mapped
[x] role/permission/security/Audit boundaries preserved
[x] Account/Feature/Job/Secured/Support/Audit/Monitoring covered
[x] no God Mode/content elevation/direct SQL/storage/config
[x] no invented Feature/permission/endpoint/table/value
[x] Bahasa Indonesia simple and task-oriented
[x] screenshot placeholders only; actual screenshot 0
[x] Document 16/17 boundary preserved
```

## Release validation gate

```text
[ ] exact RC/version/build/digest/environment recorded
[ ] management/API contracts released and versioned
[ ] role/profile/permission/delegation mapping approved
[ ] all TBD_UI_LABEL replaced
[ ] all released procedures task-tested without verbal help
[ ] allow/deny/wrong-scope/stale/offline/reauth tests pass
[ ] 33 Core administration views verified
[ ] screenshots APPROVED and tied to exact RC
[ ] UAT Administrator/Auditor/Support execution reconciled
[ ] links/error/help/escalation references valid
[ ] owner/reviewer/publication path confirmed
[ ] no documentation Blocker/Critical
[ ] status changed to RELEASE_READY only after all above
```

```text
Dokumen 15 baseline : FINAL v1.0
Release-ready       : BELUM
Required topics     : 63 / 63
Core administration: 33 / 33
Decisions           : 40 / 40 AUTO-LOCKED
Actual screenshots  : 0
Next document       : 16_INCIDENT_SUPPORT_GUIDE.md
```
