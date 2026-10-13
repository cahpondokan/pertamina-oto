# 10 — DESAIN JEJAK AUDIT
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Versi:** 1.0  
> **Tanggal:** 27 Agustus 2026  
> **Status:** FINAL — PRE-CODING BASELINE  
> **Keputusan audit:** 20/20 AUTO-LOCKED  
> **Baseline fitur canonical:** 550  
> **Prioritas penyampaian utama:** 33 Primary PDF Tool Cards

---

# 1. Tujuan

Dokumen ini menetapkan **hal yang dicatat PERTAMINA OTO sebagai bukti Enterprise Audit yang tepercaya serta cara bukti tersebut dibuat, disimpan, disinkronkan, dilindungi, dicari, diekspor, dipertahankan, dan diverifikasi**.

Desain wajib menyediakan akuntabilitas bisnis dan keamanan yang berguna dengan tetap hanya menyimpan metadata, kompatibel dengan Local-First, efisien bagi server, aman bagi privasi, dan dapat diuji.

---

# 2. Ruang Lingkup

## 2.1 Termasuk dalam Ruang Lingkup

- tujuan Enterprise Audit, kelayakan peristiwa, kategori, nama, versi, envelope, dan katalog;
- ketertelusuran aktor, asal, pelaksana, sumber daya, Feature, Job, Process, dan Correlation;
- peristiwa autentikasi, otorisasi, administrasi, konfigurasi, pemrosesan, file terlindungi, fitur berisiko tinggi, Support, akses Audit, Assistant/AI, dan Security;
- antrean lokal/luring, sinkronisasi, percobaan ulang, deduplikasi, pengurutan, kegagalan, dan pemulihan;
- klasifikasi privasi, pengecualian, masking, integritas, dan perilaku berorientasi append;
- referensi SQL/SQLite, pengindeksan, akses, pencarian, paginasi, ekspor, retensi, arsip, purge, kinerja, pemantauan, dan penerimaan.

## 2.2 Di Luar Ruang Lingkup

- mengubah Documents 01–09 FINAL tanpa revisi terkendali;
- kode implementasi aplikasi, database, atau API;
- memilih vendor IAM, SIEM, broker, arsip, WORM, database, atau vendor lain;
- membuat peran, Feature ID, layanan pemrosesan, atau endpoint peristiwa generik baru;
- menyimpan isi dokumen, Support, Assistant, atau AI dalam Audit;
- menetapkan sendiri retensi resmi, legal hold, RPO/RTO, kapasitas, atau ambang peringatan;
- menyusun Unit Test Document atau Test Plan secara lengkap.

## 2.3 Otoritas

Documents 01–09 FINAL tetap menjadi otoritas yang lebih tinggi. Keputusan dicatat dalam `AUDIT_TRAIL_DECISION_REGISTER.md`; bukti audit dicatat dalam `AUDIT_TRAIL_INTEGRATION_AUDIT.md`.

---

# 3. Prinsip Audit

1. **Tujuan sebelum pengumpulan.** Peristiwa atau field hanya ada untuk kebutuhan akuntabilitas, keamanan, kepatuhan, dukungan, atau siklus hidup yang disetujui.
2. **Hanya metadata.** Isi, kredensial, rahasia, dan diagnostik yang tidak aman dilarang.
3. **Typed dan versioned.** String peristiwa atau property bag arbitrer dilarang.
4. **Aktor nyata.** Kewenangan manusia, kanal asal, dan workload pelaksana tetap dibedakan.
5. **Berorientasi append.** Update/delete biasa dilarang; koreksi dibuat sebagai peristiwa baru yang saling tertaut.
6. **Durabel sebelum diandalkan.** Tindakan kritis tidak boleh commit atau mengungkapkan hasil tanpa intent bukti yang durabel.
7. **At-least-once dan aman terhadap duplikasi.** Percobaan ulang tidak boleh membuat peristiwa semantik kedua.
8. **Local-First.** Bukti luring hanya berupa metadata dan tidak memindahkan pemrosesan dokumen ke server.
9. **Otorisasi lebih dahulu.** Scope diterapkan sebelum count, kueri, detail, ekspor, atau pengambilan arsip.
10. **Pekerjaan dibatasi.** Jumlah peristiwa, field, batch, indeks, kueri, ekspor, dan job siklus hidup memiliki batas.
11. **Tanpa jaminan palsu.** Tidak ada klaim “exactly once”, pengurutan global, WORM, atau tahan-rusak secara kriptografis yang tidak didukung.
12. **Otoritas REST/database.** SignalR, logging, dan telemetry tidak pernah menjadi source of truth Audit.

---

# 4. Enterprise Audit vs Riwayat vs Log vs Issue

| Lapisan | Tujuan | Data umum | Model perubahan/kehilangan | Batas eksplisit |
|---|---|---|---|---|
| Enterprise Audit | Siapa melakukan apa, kapan, terhadap apa, dan dengan hasil apa | Bukti typed yang diminimalkan | Durabel, berorientasi append, mengikuti siklus hidup kebijakan | Tanpa payload dokumen, Support, atau prompt |
| Riwayat Pengguna | Aktivitas pribadi terbaru dan UX hasil | Nama, tautan hasil, dan status yang terlihat oleh pemilik | Dapat dikelola pengguna sesuai kebijakan | Penghapusan tidak pernah menghapus Audit |
| Observability Teknis | Health, error, kinerja, trace, dan peringatan | Identifier aman, agregat, dan reason code | Sampling/agregasi dapat berlaku | Bukan otoritas kepatuhan |
| Issue/Support | Masalah yang dilaporkan pengguna dan alur kerja dukungan | Pesan, screenshot/lampiran eksplisit, diagnostik aman | Izin dan retensi sesuai scope kasus | Bukan penyimpanan Audit umum atau log |

Correlation ID dapat menghubungkan keempat lapisan. Identifier tersebut tidak memberikan akses atau mengizinkan penyalinan isi terbatas antarlapisan.

---

# 5. Arsitektur Audit

```text
Application action
→ approved Event Catalog
→ typed AuditIntent + sensitive-field guard
→ owner transaction/outbox OR protected local audit_outbox
→ bounded dispatcher / authenticated batch ingestion
→ Audit-owned validator and context enrichment
→ idempotent append to audit.AuditEvents
→ permission-scoped query/export/lifecycle services
→ safe health metrics (not payload copies)
```

Batas konseptual:

| Komponen | Tanggung jawab |
|---|---|
| Audit Policy/Event Catalog | Kelayakan peristiwa, type/version, field, kelas transaksi, scope, dan kelas retensi |
| Audit Intent Factory | Membuat fakta typed yang aman pada batas use case aplikasi |
| Sensitive Field Guard | Menolak nama/type/value terlarang dan metadata yang terlalu besar |
| Owner Intent Collector | Mendaftarkan intent dalam transaksi/outbox modul pemilik |
| Offline Audit Outbox | Melindungi, mengurutkan, membuat batch, mencoba ulang, dan mengakui bukti lokal |
| Audit Ingestion Service | Mengautentikasi sumber, memvalidasi, memperkaya, mendeduplikasi, dan melakukan append |
| Audit Query Service | Proyeksi kueri/detail terstruktur sesuai scope |
| Audit Export Service | Ekspor asinkron terbatas dan siklus hidup artefak |
| Audit Lifecycle Worker | Retensi, hold, arsip, purge, dan bukti yang dikendalikan |

Kode Feature tidak mengetahui entity/table database Audit dan tidak pernah menjalankan SQL Audit secara langsung.

---

# 6. Batas Kepercayaan Audit

Konteks server/aplikasi tepercaya mencakup Principal ID internal yang terautentikasi, revisi policy/permission saat ini, environment, waktu penerimaan server, identitas/tujuan workload, serta kepemilikan/scope sumber daya yang berwenang.

Nilai dari klien diperlakukan sebagai klaim sampai tervalidasi:

- actor, role, permission, unit, owner, outcome, waktu server, kelas retensi, sumber, dan environment tidak pernah dipercaya secara langsung;
- waktu kejadian luring, urutan sumber, versi aplikasi/platform, identifier Feature/Process/Correlation, dan ukuran aman memerlukan validasi schema serta provenance;
- akses dokumen/storage hanya diselesaikan setelah otorisasi dan tidak pernah diturunkan dari kepemilikan referensi Audit;
- Event ID, Job ID, Correlation ID, cursor, referensi file, Issue ID, atau referensi ekspor bukan kewenangan.

Untuk bukti luring yang tertunda, sistem membedakan provenance grant/policy pada waktu kejadian dari konteks identity/policy saat diterima. Sistem tidak pernah memberi label role pada waktu penerimaan sebagai role historis tanpa bukti.

---

# 7. Model Pelaku

Tipe aktor terkendali:

```text
USER
ADMINISTRATOR
AUDITOR
SUPPORT
SYSTEM
SERVICE
WORKER
```

Field aktor membedakan:

| Konsep | Arti |
|---|---|
| Actor | Subjek manusia/sistem terautentikasi yang bertanggung jawab |
| Origin | Kanal UI/API seperti WEB, DESKTOP, MOBILE, ASSISTANT, ADMIN, SUPPORT |
| Executor | Service/Worker yang melaksanakan pekerjaan |
| OnBehalfOf | Manusia pemrakarsa yang dipertahankan untuk eksekusi workload tertunda |
| Role/Unit Snapshot | Konteks kejadian minimal yang disetujui; bukan otorisasi saat ini |

Assistant tidak menjadi `ASSISTANT_ON_BEHALF_OF_USER` sebagai aktor utama. Pengguna sebenarnya tetap menjadi Actor dengan `Origin=ASSISTANT`.

---

# 8. Model Sumber Daya

Bukti sumber daya menggunakan tipe logis terkendali dan ID opaque:

```text
ACCOUNT
SESSION
ROLE_ASSIGNMENT
PERMISSION_ASSIGNMENT
POLICY
FEATURE_CONFIGURATION
LIMIT_CONFIGURATION
FEATURE_EXECUTION
JOB
SECURED_FILE
SECURED_FILE_VERSION
ISSUE
ISSUE_ATTACHMENT
AUDIT_EVENT
AUDIT_QUERY
AUDIT_EXPORT
ASSISTANT_WORKFLOW
AI_OPERATION
SECURITY_FINDING
WORKLOAD_OPERATION
```

Aturan:

- simpan `ResourceTypeCode + ResourceId` hanya ketika diperlukan;
- referensi parent/child diotorisasi secara terpisah;
- referensi File/SecuredFile/Issue/Attachment/Export merupakan field terbatas;
- nama file, path, storage key, URL, byte objek, dan isi bukan identitas sumber daya;
- referensi sumber daya tidak boleh digunakan sebagai token, tautan unduhan, atau locator storage langsung.

---

# 9. Taksonomi Peristiwa

| Kategori | Tujuan |
|---|---|
| `AUTH` | Sign-in, sign-out, session, dan reauthentication |
| `AUTHORIZATION` | Keputusan allow/deny istimewa dan keamanan lintas sumber daya |
| `ADMINISTRATION` | Account, role, permission, delegasi, dan kontrol administratif |
| `POLICY` | Perubahan security, klasifikasi, retensi, dan policy terkendali |
| `CONFIGURATION` | Perubahan exposure Feature, limit, pemeliharaan, dan kompatibilitas |
| `PROCESSING` | Feature/Job yang disetujui, diterima, dan mencapai hasil terminal |
| `SECURED_FILE` | Siklus hidup file/version yang diamankan ke akun |
| `HIGH_RISK_FEATURE` | Hasil Protect, Unlock, visual Sign, Redact, tindakan destruktif, share, atau overwrite |
| `SUPPORT` | Tindakan triage/access/status/note/export Issue tanpa isi |
| `AUDIT_ACCESS` | Akses sensitif untuk kueri, detail, ekspor, atau siklus hidup |
| `ASSISTANT` | Hanya batas orkestrasi/keamanan yang bermakna |
| `AI` | Policy dan hasil provider lokal/internal yang disetujui |
| `SECURITY` | Kontrol keamanan, penyalahgunaan, input berbahaya, serta penolakan route/policy |
| `RECOVERY` | Hasil restore, siklus hidup kunci, dan validasi terkendali |

---

# 10. Penamaan Peristiwa

Nama peristiwa adalah fakta PascalCase yang stabil:

```text
Category.PastTenseFact
```

Contoh:

```text
Auth.LoginSucceeded
Authorization.PrivilegedActionDenied
Administration.PermissionAssigned
Policy.SecurityPolicyChanged
Processing.ExecutionCompleted
SecuredFile.VersionCreated
AuditAccess.ExportDownloaded
Security.PublicAiRouteBlocked
```

Nama tersebut bukan label UI, teks terlokalisasi, nama route, nama method, table database, pesan error, atau judul Feature. Makna yang sudah dirilis tidak boleh diubah langsung.

---

# 11. Pembuatan Versi Peristiwa

- Setiap `EventType` memiliki integer `EventVersion` yang independen dari `contractVersion` API.
- Perubahan material pada field atau makna menaikkan Event Version.
- Reader tetap mendukung versi historis yang disetujui.
- Field tambahan opsional hanya diizinkan jika schema terdaftar menandainya sebagai opsional.
- Type/version peristiwa yang tidak dikenal tidak pernah dipaksa menjadi peristiwa generik atau dibuang.
- Peristiwa yang belum didukung tetap berada dalam state failed/outbox terlindungi dengan alasan aman dan peringatan.
- Ekspor mencantumkan type/version tersimpan dan menangani tampilan deprecated/tidak dikenal secara aman.

---

# 12. Amplop Peristiwa

| Field | Kebutuhan |
|---|---|
| `AuditEventId` | ID peristiwa logis yang stabil dan unik secara global |
| `ClientEventId` | Wajib untuk asal klien/luring; stabil pada setiap percobaan ulang |
| `EventType` / `EventVersion` | Kontrak typed yang terdaftar |
| `Category` / `AuditLevel` | Pemetaan katalog yang divalidasi server |
| `OccurredAtUtc` / `ReceivedAtUtc` | Waktu kejadian pada sumber dan waktu penerimaan server |
| `SourceEventSequence` | Pengurutan/rekonsiliasi per sumber yang opsional |
| `ActorPrincipalId` / `ActorType` | Subjek bertanggung jawab yang terverifikasi |
| `ActorRoleSnapshotCodes` / `ActorUnitSnapshotCode` | Snapshot kejadian minimal yang disetujui |
| `ActorDisplayRef` / `SessionIdRef` / `DeviceProfileRef` opsional | Referensi aman terbatas hanya jika tujuan bukti dan policy memerlukannya; bukan otoritas identity atau raw device ID |
| `Origin` / `ExecutorType` / `ExecutorIdentityId` / `OnBehalfOfPrincipalId` | Provenance kanal dan workload |
| `ActionCode` | Tindakan terkendali yang stabil |
| `ResourceTypeCode` / `ResourceId` / parent opsional | Target logis opaque |
| `FeatureId` | Feature ID canonical bila terkait |
| `ProcessId` / `JobId` / `CorrelationId` | Wajib bila berlaku |
| `PlatformCode` / `AppVersion` / `EnvironmentCode` | Konteks terkendali |
| `ExecutionLocation` / `ProcessingMode` | `LOCAL`, `SERVER`, atau konteks hybrid yang disetujui |
| `Outcome` | Enum hasil terkendali |
| `ReasonCode` / `ErrorCode` | Hanya kode aman yang stabil |
| `InputSizeBytes` / `OutputSizeBytes` atau bucket | Ukuran aman non-isi yang opsional |
| `PermissionCode` / `PolicyVersion` / `ResourceVersion` | Hanya jika bukti memerlukannya |
| `SafeMetadataSchemaVersion` / `SafeMetadata` | Objek tertutup, allowlisted, dan terbatas |

Policy field wajib:

- tindakan manusia memerlukan Actor;
- tindakan Feature memerlukan Feature ID canonical;
- pemrosesan server/background memerlukan Job ID ketika Job tersedia;
- tindakan server, privileged, Support, mutasi terlindungi, AI, berisiko tinggi, akses Audit, dan lintas komponen memerlukan Correlation ID;
- peristiwa kesehatan sistem tanpa sumber daya tetap berada di Observability, kecuali katalog menetapkannya sebagai bukti akuntabilitas/keamanan.

Hasil canonical:

```text
ACCEPTED | SUCCEEDED | FAILED | DENIED | CANCELLED | PARTIAL | EXPIRED | REVOKED
```

---

# 13. Peristiwa Autentikasi

| Peristiwa | Pemicu | Bukti aman minimum |
|---|---|---|
| `Auth.LoginSucceeded` | Kewenangan session/aplikasi diberikan | Actor, waktu, kanal/platform, referensi session aman bila wajib, revisi policy, correlation, outcome |
| `Auth.LoginFailed` | Kegagalan autentikasi yang bermakna | Referensi Actor bila diketahui secara aman, kanal/platform, alasan aman, correlation |
| `Auth.LogoutCompleted` | Session saat ini berakhir | Actor, tindakan/referensi session aman, kanal, outcome |
| `Auth.SessionRevoked` | Session dicabut/kedaluwarsa oleh policy/admin | Referensi Actor/target yang aman, initiator/executor, alasan, policy, outcome |
| `Auth.ReauthenticationSucceeded` | Fresh authentication sensitif selesai | Actor, kelas tindakan/sumber daya, policy, correlation |
| `Auth.ReauthenticationFailed` | Fresh authentication sensitif gagal | Actor, kelas tindakan/sumber daya, alasan aman, correlation |
| `Auth.OfflineGrantInvalidated` | Kewenangan luring terlindungi dibatalkan | Referensi Actor/device-profile yang aman, revisi/alasan, outcome |

Token, cookie, kredensial, raw claim, payload identity lengkap, dan header otorisasi tidak pernah dicatat.

---

# 14. Peristiwa Otorisasi

| Peristiwa | Pemicu |
|---|---|
| `Authorization.PrivilegedActionGranted` | Grant privileged yang dikonfirmasi policy dan memerlukan bukti positif |
| `Authorization.PrivilegedActionDenied` | Permintaan sensitif/privileged ditolak |
| `Authorization.CrossResourceScopeDenied` | Probe atau penolakan scope lintas pengguna/sumber daya |
| `Authorization.PolicyDenied` | Penolakan policy enterprise/klasifikasi/provider/luring |
| `Authorization.ReauthenticationRequired` | Tindakan sensitif diblokir sampai fresh authentication selesai dan buktinya bermakna |

Jangan mengaudit setiap `403` biasa. Katalog menaikkan penolakan sensitif/privileged, ambang penyalahgunaan berulang, pola enumeration, dan hasil policy yang relevan bagi keamanan. Penolakan rutin dapat tetap menjadi sinyal Observability yang aman.

---

# 15. Peristiwa Administrasi

```text
Administration.AccountEligibilityChanged
Administration.RoleAssigned
Administration.RoleRevoked
Administration.PermissionAssigned
Administration.PermissionRevoked
Administration.SupportGrantChanged
Administration.SessionRevoked
Administration.DelegationChanged
```

Bukti wajib mencakup initiator, referensi target opaque, tindakan, kode scope/profile/version, kode atau versi sebelum/sesudah yang aman, outcome, correlation, serta hasil reauthentication/policy bila diwajibkan.

Raw claim, profil pekerja, kredensial, justifikasi bebas, data dokumen, dan nilai konfigurasi rahasia tidak disimpan.

---

# 16. Peristiwa Kebijakan dan Konfigurasi

```text
Policy.SecurityPolicyChanged
Policy.ClassificationChanged
Policy.RetentionPolicyChanged
Policy.LegalHoldChanged
Configuration.FeatureExposureChanged
Configuration.LimitChanged
Configuration.MaintenanceModeChanged
Configuration.MinimumClientVersionChanged
```

Simpan key/scope terkendali, versi atau digest sebelumnya/baru, nilai typed non-rahasia allowlisted yang opsional, actor/approver bila diwajibkan policy, waktu berlaku, outcome, dan correlation. Nilai rahasia serta JSON sebelum/sesudah tanpa batas dilarang.

---

# 17. Peristiwa Pemrosesan

Milestone baseline:

```text
Processing.ExecutionAccepted
Processing.ExecutionCompleted
Processing.ExecutionFailed
Processing.ExecutionCancelled
Processing.ExecutionRetryScheduled   (only materially meaningful retry)
```

Aturan:

- seluruh eksekusi 33 Primary PDF Tool membuat bukti L2 accepted/terminal yang aman;
- tindakan berisiko tinggi juga membuat bukti finalisasi/keamanan L3 yang berlaku;
- satu workflow dapat memuat beberapa langkah Feature canonical; setiap langkah memiliki peristiwa yang disetujui dan menggunakan Correlation ID workflow yang sama;
- persentase progress Job, token page/chunk/OCR, heartbeat antrean, notifikasi SignalR, preview, edit draft, dan tahap engine internal bukan peristiwa Enterprise Audit;
- kegagalan server mencatat Error Code aman yang stabil, bukan raw exception engine/provider;
- tindakan sekunder local-only tidak otomatis menjadi Audit pusat; Event Catalog menentukan L0/L1/L2/L3.

---

# 18. Peristiwa File Terlindungi

```text
SecuredFile.Created
SecuredFile.VersionCreated
SecuredFile.VersionRestored
SecuredFile.VersionReplaced
SecuredFile.RetrievalAuthorized
SecuredFile.RetrievalDenied
SecuredFile.DeletionRequested
SecuredFile.PermanentlyDeleted
```

Bukti menggunakan SecuredFile/Version ID opaque, actor, tindakan, version/count/state aman, versi policy/resource, outcome, Job/Correlation ID bila berlaku, dan hasil reauthentication.

Bukti tidak pernah menyimpan nama file, object key, password, isi, aset tanda tangan, lokasi storage, atau download token. “Amankan ke Akun” merupakan batas selective-sync eksplisit; sinkronisasi Audit tidak pernah mengunggah dokumen.

---

# 19. Peristiwa Fitur Berisiko Tinggi

| Tindakan canonical | Feature | Peristiwa |
|---|---|---|
| Protect PDF | `FEAT-PDF-061` | `HighRiskFeature.ProtectFinalized` |
| Unlock PDF | `FEAT-PDF-062` | `HighRiskFeature.UnlockFinalized` |
| Visual Sign PDF (non-PKI) | `FEAT-SIGN-013` | `HighRiskFeature.VisualSignFinalized` |
| Redact PDF | `FEAT-PDF-069` | `HighRiskFeature.RedactionFinalized` / `RedactionValidationFailed` |
| Penghapusan permanen | Feature/tindakan sumber daya yang berlaku | `HighRiskFeature.PermanentDeletionCompleted` |
| Overwrite/penggantian akhir | Feature/tindakan yang berlaku | `HighRiskFeature.FinalReplacementCompleted` |
| Share/ekspor sensitif | Scope yang disetujui dan berlaku | `HighRiskFeature.SensitiveDisclosureCompleted` |

Bukti hanya mencatat kode hasil confirmation/reauth/policy dan referensi sumber daya opaque. Password PDF, koordinat/teks redaction, gambar tanda tangan/paraf, isi halaman, nama/path file keluaran, dan rahasia penerima dilarang. Visual Sign tetap dinyatakan sebagai non-PKI.

---

# 20. Peristiwa Dukungan

```text
Support.IssueSubmitted
Support.AttachmentSubmitted
Support.IssueViewed
Support.AttachmentViewed
Support.IssueTriaged
Support.IssuePriorityChanged
Support.IssueStatusChanged
Support.InternalNoteAdded
Support.ExportRequested
Support.ExportCompleted
Support.ExportDownloaded
Support.AccessDenied
```

Audit menyimpan referensi opaque Issue/Attachment/Export, kode action/status/category, actor, outcome, waktu, Job/Correlation ID, serta platform/app/Error Code aman bila diperlukan. Pesan Issue, teks catatan internal, byte screenshot/lampiran, markup, nama file asli, dan referensi objek tetap berada pada lapisan Support yang dilindungi permission.

---

# 21. Peristiwa Auditor dan Akses Audit

```text
AuditAccess.QueryExecuted
AuditAccess.EventViewed
AuditAccess.ExportRequested
AuditAccess.ExportCompleted
AuditAccess.ExportDownloaded
AuditAccess.ExportExpired
AuditAccess.RetentionPolicyChanged
AuditAccess.LegalHoldChanged
AuditAccess.ArchiveActionCompleted
AuditAccess.PurgeActionCompleted
AuditAccess.PermissionChanged
AuditAccess.AccessDenied
```

Aturan audit terhadap Audit:

- catat satu session kueri sensitif yang disetujui, bukan setiap halaman cursor biasa;
- catat tampilan detail privileged hanya jika diwajibkan policy;
- gunakan satu siklus hidup Export Request, bukan peristiwa ekspor per baris;
- ingestion/persistensi internal tidak pernah mengaudit dirinya sendiri secara rekursif;
- bukti siklus hidup berada di luar kumpulan purge yang dicatatnya;
- simpan ringkasan filter/scope aman yang dinormalisasi beserta fingerprint, bukan raw SQL/kueri/isi atau payload baris hasil.

Auditor tidak dapat mengedit/menghapus bukti, mengubah retention/hold/policy, atau mengakses isi dokumen/Support/Assistant. Ekspor Audit tetap memerlukan permission terpisah.

---

# 22. Peristiwa Assistant dan AI

Default-nya adalah peristiwa Feature canonical yang mendasarinya dengan `Origin=ASSISTANT`, bukan duplikasi peristiwa Assistant.

Peristiwa Assistant yang bermakna dapat mencakup:

```text
Assistant.HighRiskStepConfirmed
Assistant.WorkflowValidationFailed
Assistant.PrivilegedActionBlocked
Assistant.AuditableFeatureDelegated   (only when needed beyond the Feature event)
```

Peristiwa tata kelola AI dapat mencakup:

```text
AI.ProviderTransferAuthorized
AI.ProviderInvocationCompleted
AI.LocalIntelligenceCompleted
AI.PolicyDenied
Security.PublicAiRouteBlocked
```

Simpan Feature ID canonical, Actor pengguna sebenarnya, Origin, kelas route yang disetujui (`LOCAL` atau `INTERNAL_APPROVED`), kode profil provider terbatas yang opsional, policy/outcome, dan Correlation ID. Prompt, percakapan, system instruction, konteks dokumen/OCR, endpoint/account/credential provider, request/response, output model, dan payload AI publik dilarang.

---

# 23. Peristiwa Keamanan

```text
Security.TokenReplayDetected
Security.ResourceProbeDetected
Security.FileRejected
Security.MalwareDetected
Security.PathOrIpcViolationDetected
Security.PublicAiRouteBlocked
Security.WorkloadScopeDenied
Security.OutputValidationFailed
Security.AbuseThresholdExceeded
Security.KeyLifecycleChanged
Security.RestoreValidationFailed
```

Simpan kode event/reason/error yang stabil, actor/source bila diketahui secara aman, type/referensi opaque target, versi policy/control, outcome, environment, dan Correlation ID. Jangan pernah menyimpan payload serangan, file berbahaya, string exploit lengkap, token, kredensial, raw request, stack, SQL, paket jaringan, atau payload provider.

---

# 24. Audit Lokal dan Luring

Tingkat bukti:

| Tingkat | Arti | Sinkronisasi pusat |
|---|---|---|
| `L0` | Tidak memerlukan bukti akuntabilitas/keamanan | Tidak |
| `L1` | Riwayat pribadi atau bukti teknis lokal | Tanpa sinkronisasi Enterprise Audit |
| `L2` | Bukti bisnis enterprise | Ya, ketika konektivitas kembali |
| `L3` | Bukti privileged/security/berisiko tinggi | Ya; policy durability/kegagalan yang ketat |

Seluruh eksekusi 33 Core Tool mengikuti bukti milestone L2; hasil Core berisiko tinggi menambahkan L3. Edit lokal sekunder, preview, undo/redo, autosave, cache/thumbnail, navigasi, pengetikan pencarian, dan progress tidak otomatis menjadi Audit pusat.

Alur luring:

```text
eligible local action
→ validate typed safe AuditIntent
→ reserve/write protected audit_outbox metadata
→ finalize action only under its transaction class
→ keep row until durable server acknowledgement
→ reconnect with authenticated compatible client/policy
→ bounded idempotent batch sync
```

Outbox lokal dienkripsi/dilindungi, hanya berisi metadata, memiliki scope profile/device, dan terpisah dari perintah server tertunda, History, transfer terlindungi, sinkronisasi generik, cache, atau isi recovery.

---

# 25. Sinkronisasi dan Percobaan Ulang

- gunakan `POST /api/v1/audit-events/batches` yang sudah ada;
- autentikasi client/source dan validasi ulang versi contract/event;
- buat batch berdasarkan batas jumlah peristiwa dan byte terenkode;
- flush pada jumlah/byte/usia/reconnect/kesempatan background yang disetujui;
- lakukan retry dengan exponential backoff dan jitter yang terbatas;
- jangan menahan sumber daya database/jaringan selama jeda;
- berikan acknowledgement untuk hasil batch/peristiwa yang durabel;
- bersihkan baris lokal hanya setelah acknowledgement dan sesuai policy lokal yang disetujui;
- pertahankan peristiwa unsupported/poison secara aman tanpa hot loop;
- jangan pernah mengunggah payload dokumen/file/Issue/Assistant selama sinkronisasi Audit;
- bukti account yang revoked/disabled tetap terlindungi untuk route recovery terautentikasi yang disetujui; ingestion anonim tidak tersedia.

Siklus hidup lokal konseptual:

```text
PENDING_LOCAL → READY_TO_SYNC → IN_FLIGHT → ACKNOWLEDGED
                         ↘ RETRY_PENDING / VERSION_BLOCKED / POLICY_BLOCKED
```

Label tersebut dapat dipetakan ke field outbox yang sudah disetujui; database/table baru tidak diperlukan.

---

# 26. Deduplikasi

Kunci dan aturan deduplikasi:

| Input | Aturan |
|---|---|
| `AuditEventId` | Identitas logis yang stabil secara global |
| `ClientEventId + trusted source scope` | Identitas client/luring yang stabil |
| Batch `Idempotency-Key + request fingerprint` | Menggunakan kembali hasil batch logis yang sama |
| Server `EventFingerprint` | Memverifikasi kesetaraan semantik yang immutable |

```text
same ID + same fingerprint      → duplicate acknowledgement, one event
same ID + different fingerprint → conflict/security signal, no overwrite
new ID                           → validate and append
```

Correlation ID, Job ID, Feature ID, event type, outcome, dan timestamp tidak pernah menjadi kunci deduplikasi. Percobaan ulang dengan hasil bisnis baru yang material menggunakan event ID baru dan tautan eksplisit ke tindakan/peristiwa asli.

---

# 27. Pengurutan

- tidak ada klaim pengurutan peristiwa global;
- `SourceEventSequence` per sumber mendukung rekonsiliasi lokal;
- versi business/resource mendukung transisi state terurut jika tersedia;
- `OccurredAtUtc` tetap menjadi waktu bisnis pada sumber;
- `ReceivedAtUtc` tetap menjadi waktu ingestion server yang tepercaya;
- kedatangan terlambat mempertahankan kedua nilai beserta metadata delay/trust yang aman;
- urutan sumber yang hilang membuat sinyal gap/rekonsiliasi dan tidak pernah menciptakan peristiwa yang hilang;
- urutan tampilan default menggunakan `OccurredAtUtc DESC` ditambah tie-breaker baris/peristiwa Audit yang unik;
- investigasi berdasarkan waktu penerimaan menggunakan mode `ReceivedAtUtc` eksplisit.

---

# 28. Korelasi

Correlation membuat jejak tanpa menduplikasi isi:

```text
User Actor
→ Assistant Origin (optional)
→ canonical Feature / Process ID
→ API request
→ Job ID / Worker executor
→ result/security/support outcome
→ Audit evidence
```

Aturan:

- satu operasi logis menggunakan Correlation ID yang sama pada API, Worker, storage, log aman, notifikasi SignalR, diagnostik Issue, dan Audit;
- setiap peristiwa Audit mempertahankan Audit Event ID sendiri;
- Job ID tersedia untuk Job pemrosesan;
- Feature ID bersifat canonical dan typed;
- referensi Issue/SecuredFile/Export hanya disertakan ketika peristiwa memerlukannya;
- identifier tidak pernah memberikan akses data/isi dan di-masking pada tampilan luas.

---

# 29. Klasifikasi Data

| Kelas | Arti | Contoh |
|---|---|---|
| `SAFE` | Metadata yang disetujui dalam scope berwenang; bukan data publik | Kode event/category/outcome, waktu, Feature ID, keluarga platform, versi aplikasi |
| `MASKED` | Tampilan turunan/minimal secara default | Tampilan Principal, device/version umum, identifier pendek |
| `RESTRICTED` | Hanya disimpan untuk bukti yang beralasan dan diproyeksikan terpisah | Actor Principal ID, snapshot role/unit, Process/Job/Correlation/resource ID, profil provider aman, ukuran lengkap |
| `NOT_STORED` | Dilarang berada dalam Enterprise Audit | Isi, kredensial, rahasia, nama/path file secara default, raw diagnostics |

Masking tidak pernah menggantikan otorisasi. Identifier lengkap dapat diterima untuk pencarian tepat yang berwenang, sedangkan tampilan daftar/laporan tetap di-masking.

Label klasifikasi resmi Pertamina dan pemetaan penanganannya tetap berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 30. Pengecualian Data Sensitif

Data berikut berstatus `NOT_STORED` dalam kontrak Audit, outbox lokal/server, baris SQL, ekspor, log, trace, atau alert:

- password/passphrase, password PDF, token, cookie, secret, private/encryption key, dan connection string;
- byte dokumen/file, teks lengkap, isi halaman, thumbnail, preview, serta isi checkpoint/cache;
- nama file asli, path lokal/UNC/server, URL, bucket/share/object key, dan tautan unduhan durabel;
- teks OCR/extracted/redacted/translated/Markdown/generated secara lengkap;
- isi clipboard, gambar/template tanda tangan/paraf/stempel, serta koordinat/teks redaction;
- prompt/chat/system instruction/percakapan/konteks model/narasi tool Assistant;
- konteks dokumen AI/provider, request, response, endpoint/account/credential;
- pesan Issue, teks catatan internal, byte/markup/referensi objek screenshot/lampiran;
- raw exception, stack, teks SQL/query plan, body request/response, claim dump, authorization header, dan payload IPC;
- raw User-Agent, geolokasi lengkap, hostname/nama device/serial/IMEI/MAC/advertising ID;
- IP/header jaringan lengkap pada baseline;
- parameter Feature bebas dari pengguna, nama file keluaran, atau JSON sebelum/sesudah bebas.

Field guard menolak nama/type/source terlarang, nesting/ukuran berlebihan, field schema yang tidak dikenal, serta nilai yang cocok dengan canary rahasia/isi.

---

# 31. Integritas

Kontrol integritas:

1. event type/version dan field schema yang terdaftar pada server;
2. identity, receipt time, environment, source/workload, category, level, dan kelas retensi yang diturunkan server;
3. identity workload append/query/lifecycle yang dibatasi;
4. constraint type/check/unique/relationship database;
5. ID stabil dan fingerprint immutable;
6. outbox pemilik transaksional dan outbox lokal terlindungi;
7. ingestion idempotent dan deteksi konflik;
8. visibilitas failed/poison tanpa logging raw payload;
9. validasi backup, restore, replay, uniqueness, permission, hold, dan append setelah restore;
10. manifest/digest arsip terverifikasi yang opsional jika arsip diaktifkan.

Document 10 tidak mengklaim WORM, non-repudiation kriptografis, penandatanganan PKI, blockchain, atau sifat tahan-rusak mutlak. Kemampuan tersebut memerlukan kebutuhan resmi eksplisit, pemilihan produk, dan revisi terkendali.

---

# 32. Prinsip Imutabilitas

- Jalur User, Administrator, Auditor, Support, service, dan aplikasi biasa tidak dapat meng-update/delete peristiwa sumber.
- Repository generik tidak mengekspos update/delete Audit.
- Koreksi, supersession, konteks terlambat, reversal, atau hasil siklus hidup merupakan peristiwa tertaut baru.
- Actor/time/outcome asli tetap utuh.
- Purge retensi merupakan siklus hidup policy dengan otorisasi terpisah yang dijalankan workload khusus, bukan edit peristiwa.
- Bukti tindakan purge/archive/hold tetap berada di luar kumpulan target terdampak dan bertahan sesuai policy sendiri.

---

# 33. Penyimpanan

Penyimpanan server menggunakan SQL Server yang sudah ada dan schema modul `audit`. Bukti luring lokal menggunakan penyimpanan metadata SQLite profile/device yang sudah ada.

Struktur server baseline:

```text
audit.AuditEvents
audit.IngestionBatches
audit.ExportRequests
```

Struktur siklus hidup kondisional seperti Archive Manifest, Retention Run, atau Legal Hold hanya diperkenalkan jika diwajibkan policy/scale yang disetujui dan melalui revisi terkendali Document 07. Database, event store, message broker, atau vendor baru tidak diperlukan.

Byte/artefak payload, bila diperlukan oleh ekspor/arsip yang disetujui, menggunakan object/file storage terenkripsi yang sudah disetujui dengan referensi internal opaque. Baris Audit tidak pernah memuat byte artefak.

---

# 34. Referensi Desain SQL

`audit.AuditEvents` mempertahankan field aman minimum dari Document 07 dan hanya menambahkan field typed yang dapat dicari melalui tata kelola schema terkendali:

```text
AuditEventPk / AuditEventId
IngestionBatchPk nullable
EventTypeCode / EventVersion / CategoryCode / AuditLevelCode
OccurredAtUtc / ReceivedAtUtc / SourceEventSequence
ActorPrincipalId / ActorTypeCode / approved role-unit snapshot codes
OriginCode / ExecutorTypeCode / ExecutorIdentityId / OnBehalfOfPrincipalId
ActionCode / OutcomeCode / ReasonCode / ErrorCode
ResourceTypeCode / ResourceId / optional ParentResourceId
FeatureId / ProcessId / JobId / CorrelationId
SourceCode / PlatformCode / AppVersion / EnvironmentCode
ExecutionLocationCode / ProcessingModeCode
safe size fields or category
PermissionCode / PolicyVersion / ResourceVersion where required
RetentionClassCode / RetentionPolicyVersion
SafeMetadataSchemaVersion / bounded SafeMetadataJson nullable
EventFingerprint
```

`audit.IngestionBatches` menyimpan identity source/batch, fingerprint/version request, konteks actor/source terautentikasi, count, waktu received/completed, outcome/status/alasan aman, dan versi concurrency—bukan raw request body.

`audit.ExportRequests` menyimpan requester/scope, filter dan field set versioned yang dinormalisasi, status/Job/Correlation ID, waktu as-of/created/completed/expiry, count/size aman, digest, dan referensi artefak opaque.

---

# 35. Pengindeksan

Kandidat awal yang sempit:

| Struktur | Tujuan utama |
|---|---|
| Clustered `AuditEventPk` | Append berurutan dan tie-breaker kueri unik |
| Unique `AuditEventId` | Idempotency dan rekonsiliasi langsung |
| Actor + waktu Occurred + row | Timeline actor sesuai scope |
| Feature + waktu Occurred + row | Timeline Feature; filtered saat nullable |
| Outcome + waktu Occurred + row | Investigasi outcome |
| Process ID + row | Jejak kebutuhan `DR-DB-006` |
| Source scope + Ingestion Batch ID unique | Idempotency batch |
| Status batch + received time + row | Recovery/cleanup |
| Requester ekspor + state + created time + row | Siklus hidup ekspor sesuai scope |

Indeks kondisional untuk category/type, resource, organizational scope, Job ID, Correlation ID, atau waktu Received memerlukan route selektif yang terukur dan review DBA.

Jangan mengindeks JSON arbitrer, content/message/note, path/storage key, raw diagnostics, setiap kombinasi filter, atau daftar include yang lebar. Setiap indeks tambahan mencatat pemilik kueri, selectivity, biaya write/storage, bukti plan, dan kriteria penghapusan.

---

# 36. Retensi

Retensi menggunakan katalog kelas versioned yang ringkas:

| Kelas | Tujuan penggunaan; durasi tetap mengikuti policy resmi |
|---|---|
| `SHORT` | Catatan siklus hidup operasional ingestion/export dengan kebutuhan bukti lebih rendah |
| `STANDARD` | Bukti rutin Feature/process dan siklus hidup sumber daya normal |
| `EXTENDED` | Bukti identity, authorization, privileged/admin/Support/akses Audit, tindakan destruktif, security, dan recovery luar biasa |
| `POLICY_DEFINED` | Bergantung pada klasifikasi, insiden, investigasi, regulasi, kontrak, atau hold |

Server menetapkan kelas dan versi policy dari Event Catalog; klien tidak dapat menurunkannya. Menghapus History, Job/result, tampilan Issue, percakapan Assistant, workspace, file sumber, atau sumber daya terlindungi tidak pernah menghapus Enterprise Audit.

Hal berikut tetap berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`:

- durasi per kelas;
- apakah retensi dimulai dari `OccurredAtUtc`, `ReceivedAtUtc`, atau deadline terhitung yang dikendalikan;
- guard untuk late arrival/replay;
- perilaku perubahan policy retroaktif;
- kelas bukti minimum dan aturan legal/investigation hold;
- interaksi siklus hidup salinan backup.

Purge Produksi dinonaktifkan sampai nilai tersebut disetujui dan dinyatakan dalam policy versioned.

---

# 37. Arsip

Arsip bersifat kondisional berdasarkan kewajiban resmi atau pertumbuhan hot table, backup window, latency kueri, atau kebutuhan kapasitas yang terukur. Arsip menggunakan storage terenkripsi yang sudah disetujui; dokumen ini tidak memilih vendor/produk arsip.

```text
select closed policy-eligible key range
→ recheck hold/policy
→ create versioned encrypted archive object
→ verify count, range, schema version, and digest
→ commit Archive Manifest
→ permit bounded active-table purge
→ retain/restore/purge archive under its own policy
```

Bukti manifest mencakup manifest ID opaque, rentang event-key/waktu yang disetujui, count, versi event/schema/retention-policy, waktu created/verified, algoritma/nilai digest, referensi storage opaque, lifecycle state, dan hold state. Arsip bukan backup; backup bukan arsip.

Pencarian/restore arsip diotorisasi secara terpisah, asinkron, terbatas, read-only bila memungkinkan, memiliki expiry, dideduplikasi terhadap ID aktif, dan diaudit.

---

# 38. Penghapusan Permanen Berdasarkan Kebijakan

- API biasa yang menghadap pengguna tidak menerima filter penghapusan Audit;
- workload retensi khusus dengan scope tujuan menjalankan policy;
- permission/policy/hold saat ini diperiksa ulang tepat sebelum penghapusan;
- bukti yang wajib diarsipkan tidak dihapus sampai artefak dan manifest di-commit serta diverifikasi;
- proses menggunakan rentang kecil terindeks, lease singkat, checkpoint, transaksi terbatas, retry, throttling, dan metrik aman;
- bukti yang held, belum acknowledged, failed, atau unresolved dilewati;
- tindakan mencatat versi policy, range/count, outcome, checkpoint, dan alasan aman tanpa payload yang dihapus;
- bukti purge berada di luar kumpulan target tepatnya;
- pengujian restore menerapkan ulang kontrol hold/purge sebelum data dapat dikueri.

---

# 39. Kontrol Akses

```text
ALLOW_AUDIT_ACCESS = authenticated and active principal
    AND online valid session
    AND explicit Audit/Support/Admin-transparency permission
    AND server-derived organizational/resource scope
    AND allowed event category and type
    AND allowed field projection
    AND permitted date/environment scope
    AND current policy/classification
    AND fresh reauthentication when required
```

| Konsumen | Akses baseline |
|---|---|
| User | Riwayat Pribadi; tanpa Enterprise Audit Console. Transparansi keamanan account sendiri yang opsional memerlukan keputusan UI/kebutuhan eksplisit pada masa depan |
| Administrator | Tanpa akses Audit implisit. Dapat menerima bukti control-plane dengan scope yang diberikan terpisah; write Admin tidak mengimplikasikan read/export Audit |
| Auditor | `audit.event.list/read` dalam `AUDIT_SCOPE`; ekspor terbatas opsional yang terpisah |
| Support | Hanya bukti aman tertaut Issue melalui otorisasi Support; tanpa kueri/ekspor Audit umum |
| Workload siklus hidup Audit | Operasi ingestion/export/retention/archive dengan scope tujuan tepat; bukan human role |

Cursor, Event ID, resource ID, label role, menu, akses sebelumnya, kepemilikan ekspor, atau nilai masked tidak pernah memberikan akses. Otorisasi diulang untuk setiap kelanjutan cursor, detail, pembuatan/publikasi/unduhan ekspor, pengambilan arsip, dan tindakan siklus hidup.

---

# 40. Pencarian

Pencarian Enterprise Audit bersifat terstruktur dan terikat tujuan. Pencarian mendukung investigasi atas waktu, actor, category/type peristiwa, Feature canonical, outcome, sumber daya opaque, Process/Job/Correlation, platform/source, dan environment yang diketahui dalam scope saat ini.

Aturan:

- scope diterapkan sebelum count/search/filter/sort/cursor;
- identifier tepat diotorisasi sebelum lookup;
- default menggunakan rentang UTC terbaru dan terbatas;
- hasil luas tidak dimuat lalu difilter di memory;
- tidak ada pencarian full-text arbitrer “cari semuanya”;
- tidak ada pencarian nama file atau payload document/OCR/Issue/prompt/provider karena isi tersebut tidak disimpan;
- kueri luas/lama/arsip menjadi ekspor asinkron terbatas atau memerlukan filter lebih sempit;
- “tidak ada kecocokan berwenang” tidak mengungkap count di luar scope.

---

# 41. Penyaringan

| Filter | Aturan |
|---|---|
| Rentang tanggal | Wajib berupa UTC `[from,to)` terbatas; dimensi Occurred atau Received harus eksplisit |
| Actor | Principal ID internal tepat/directory selector dalam scope; tanpa pemindaian email/nama luas |
| Event Category / Type | Nilai katalog terlihat yang terkendali |
| Feature | Feature ID canonical tepat |
| Outcome | Kode outcome terkendali |
| Resource | `ResourceType + opaque ResourceId` tepat setelah otorisasi |
| Process / Job / Correlation | Identifier terbatas yang tepat; tanpa pivot isi |
| Platform / Source / Environment | Kode terkendali; environment dapat terikat server |

Filter otorisasi buatan klien, sort/expression arbitrer, regex, dynamic JSON path, SQL, free text tanpa batas, dan pencarian leading-wildcard pada table besar tidak didukung.

Jika filter yang sering digunakan memerlukan first-class field yang belum ada dalam Document 07, field tersebut dipromosikan melalui tata kelola schema terkendali, bukan disembunyikan dalam `SafeMetadataJson` tanpa batas.

---

# 42. Paginasi

- gunakan keyset cursor opaque yang dilindungi integritas;
- ukuran halaman default 50 dan maksimum 100, konsisten dengan Document 06;
- urutan default stabil: `OccurredAtUtc DESC, AuditEventPk DESC`;
- cursor mengikat principal, revisi permission/scope/policy, filter, dimensi waktu, sort, projection, dan expiry;
- setiap kelanjutan melakukan otorisasi ulang terhadap akses saat ini;
- perubahan scope/policy/permission membatalkan atau menolak stale cursor secara aman;
- deep OFFSET paging dan exact count tanpa batas dilarang;
- projection daftar mengecualikan `SafeMetadata`; detail hanya memuatnya setelah otorisasi terpisah.

---

# 43. Ekspor

Satu alur asinkron terkendali berlaku untuk ekspor Enterprise Audit:

```text
authorized normalized query
→ separate export permission
→ current scope/field set/reauth/admission checks
→ durable ExportRequest + Audit intent
→ bounded background extraction at declared as-of boundary
→ safe private artifact + manifest/digest/expiry
→ current authorization at publication/download
→ audited download/expiry/cleanup
```

Kontrol:

- scope tanggal/category/organization/environment dan field projection yang eksplisit;
- idempotency/request fingerprint;
- batas row/byte/date/runtime/concurrency/rate/storage serta backpressure;
- truncation diam-diam tidak boleh disajikan sebagai hasil lengkap;
- masking sama atau lebih ketat daripada projection interaktif;
- artefak privat terenkripsi, ID opaque, tanpa URL publik/durabel atau lampiran email;
- manifest mencatat request, ringkasan filter/scope yang dinormalisasi, versi field/schema, makna UTC/as-of, count, digest, klasifikasi, dan expiry;
- formula spreadsheet, control character, bidi, dan newline dinetralkan/di-escape;
- tanpa field document/Support/Assistant/credential/raw diagnostic/storage.

Format baseline:

| Format | Penggunaan |
|---|---|
| UTF-8 CSV | Baseline ekspor baris terperinci |
| Fixed PDF | Template ringkasan manajemen/kepatuhan opsional yang disetujui |
| XLSX | Bukan baseline; memerlukan kebutuhan bisnis eksplisit |
| Raw JSON/database dump | Tidak tersedia bagi pengguna biasa |

Template laporan minimum adalah Perubahan Privileged; Autentikasi & Keamanan; Perubahan Policy & Configuration; Akses Sensitif yang Ditolak; Tindakan Feature Berisiko Tinggi; Aktivitas Support; serta Akses/Ekspor/Siklus Hidup Audit. Semuanya merupakan definisi saved-query/laporan sesuai scope, bukan kebutuhan dashboard baru.

---

# 44. Kinerja

Aturan write path:

- buat intent yang sempit dari ID/kode yang sudah tersedia dalam konteks aplikasi;
- jangan pernah men-serialize isi entity/request/file/OCR/exception/provider;
- tempatkan pekerjaan PDF engine, file/object storage, jaringan, AI/provider, dan ekspor di luar transaksi database;
- tulis satu baris owner outbox per tindakan accepted/terminal/security yang disetujui, bukan per progress/page/chunk;
- gunakan batch dispatcher/ingestion terbatas, reuse koneksi, lease singkat, dan backoff;
- lepaskan koneksi database sebelum delay/rendering/pekerjaan berat;
- gunakan unique constraint, bukan race read-before-insert;
- batasi ukuran/kedalaman metadata dan jumlah byte batch;
- gunakan keyset query dan projection sempit;
- throttle archive/purge/export ketika ada foreground load;
- Audit wajib tidak dapat dinonaktifkan atau di-sampling diam-diam untuk mencapai target.

Ukur latency Audit intent, tambahan waktu transaksi, row/byte per tindakan, pertumbuhan SQL/log/index, usia/laju backlog, latency ingestion, laju duplicate/reject, p50/p95/p99 kueri, durasi/ukuran ekspor, throughput archive/purge, waktu restore, CPU/RAM/I/O/jaringan/koneksi, dan throughput user-job yang representatif.

Target numerik ditetapkan dalam Document 12 dan standar Pertamina yang berlaku.

---

# 45. Kegagalan dan Pemulihan

| Kegagalan | Perilaku wajib |
|---|---|
| Validasi/write intent kritis gagal sebelum commit | Rollback mutation/session grant/penerimaan ekspor; error/Correlation ID aman; alert |
| Projection gagal setelah commit owner/outbox | Pertahankan intent secara durabel; retry terbatas; business state tetap; tanpa duplikasi |
| SQL tidak tersedia | Tidak ada mutasi server/pengungkapan privileged wajib tanpa intent durabel; response unavailable yang aman |
| SQLite/outbox lokal tidak tersedia | Pertahankan sumber/pekerjaan saat ini; blokir tindakan L3/privileged baru yang wajib diaudit sebelum mutasi; perilaku degraded L2 biasa memerlukan policy yang disetujui |
| Antrean lokal penuh | Coba sinkronisasi/cleanup terbatas; jangan mengeluarkan bukti yang belum acknowledged; cadangkan ruang terminal/kritis; terapkan policy block/degraded tingkat tindakan yang disetujui |
| Duplicate/replay | Kembalikan/gunakan ulang hasil logis; simpan sekali |
| ID sama/data berbeda | Sinyal conflict/security; tanpa overwrite |
| Sinkronisasi luring tertunda | Pertahankan waktu occurred/received, urutan sumber, versi, dan trust provenance |
| Crash sebelum commit transaksi owner | Mutasi maupun intent tidak terlihat |
| Crash setelah commit transaksi owner | Dispatcher startup melanjutkan intent durabel |
| Crash setelah append sebelum acknowledgement | Replay melakukan deduplikasi |
| Worker crash | Recovery lease/attempt; peristiwa terminal hanya dengan terminal state terverifikasi |
| Versi unsupported/poison | Pertahankan kegagalan terlindungi yang terlihat; tanpa hot loop/drop; deploy reader/migrasi kompatibel |
| Pembuatan ekspor gagal | Failed state terkendali; peristiwa kegagalan aman; artefak tidak lengkap tidak tersedia/dibersihkan |
| Archive/purge gagal | Pertahankan bukti sumber; lanjutkan dari checkpoint; jangan menghapus sebelum pemeriksaan archive/hold terverifikasi |
| Restore | Validasi schema, uniqueness, idempotency, outbox, permission, retention/hold, dan append setelah restore |

Batas fail-closed:

```text
role/permission/account eligibility and other-session revocation
high-impact security/classification/retention policy change
permanent secured-content deletion and destructive/key recovery
successful session grant when required intent cannot persist
privileged Audit/Support export or disclosure without durable access intent
```

Pemrosesan lokal/server rutin hanya boleh selesai setelah intent wajib menjadi durabel; central projection kemudian dapat mencoba ulang secara asinkron.

---

# 46. Pemantauan

Pemantauan pipeline Audit menggunakan Application Insights/OpenTelemetry dan structured logging yang disetujui hanya sebagai Observability.

Data pemantauan yang diizinkan:

```text
Event ID/type/version
source module/environment
safe outcome/reason code
Correlation ID
count, duration, rate, byte size, queue age
```

Data pemantauan yang dilarang mencakup seluruh baris intent/request/event/export Audit; isi dokumen/Support/Assistant/AI; kredensial/rahasia; raw exception/SQL; referensi storage fisik; dan nilai metadata sensitif.

Sinyal minimum:

| Area | Sinyal |
|---|---|
| Intent/outbox | kegagalan write, count pending/failed, usia tertua, retry, recovery lease |
| Dispatcher | throughput, ukuran batch, stale lease, poison/version block, tanpa progress ke depan |
| Ingestion | count dan latency accepted, duplicate, conflict, rejected/unsupported |
| Luring | count/byte/capacity/usia tertua antrean, kegagalan decrypt/read, delay sinkronisasi |
| Kueri/ekspor | penolakan otorisasi, latency, guard kueri luas, backlog/kegagalan/ukuran ekspor |
| Retensi/arsip | count eligible/held/archived/purged, usia checkpoint, kegagalan manifest/verifikasi |
| Perilaku | baseline event rate aman dan lonjakan/penurunan tidak biasa menurut type/outcome/environment |

Alert dideduplikasi, memperhatikan environment, diklasifikasikan menurut severity, dan diarahkan kepada pemilik yang bertanggung jawab setelah konfirmasi operasional resmi.

---

# 47. Kesehatan Audit

State kesehatan:

| State | Arti |
|---|---|
| `HEALTHY` | Jalur intent wajib, dispatch, ingestion, kueri/ekspor, dan siklus hidup beroperasi dalam batas terkonfirmasi |
| `DEGRADED` | Bukti durabel tetap aman, tetapi layanan backlog, latency, version, ekspor, atau arsip terganggu |
| `UNHEALTHY` | Intent wajib tidak dapat dibuat durabel, bukti tidak dapat maju secara aman, integrity/hold terancam, atau antrean terlindungi tidak dapat dibaca/penuh |

Health hanya mengekspos reason code aman dan count agregat. UI/metrik hijau tidak boleh menyembunyikan kegagalan jalur Audit wajib. Health check tidak pernah membaca atau mengeluarkan payload dokumen/Audit dan tidak membuat peristiwa Audit rekursif.

Ambang tepat, route alert, pemilik on-call, integrasi SIEM, dan recovery objective berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 48. Ketertelusuran Kebutuhan

| Kebutuhan | Kontrol Document 10 |
|---|---|
| `FR-AUDIT-001` | Typed envelope includes Process/User/role/time/Feature/outcome/size/location/Error/App Version |
| `FR-AUDIT-002` | Closed forbidden-field list and field guard |
| `FR-AUDIT-003` | Protected local outbox, automatic authenticated batch sync, retry/dedupe |
| `FR-AUDIT-004` | Append-oriented path, no ordinary delete, policy/authority/hold lifecycle |
| `NFR-PERF-005` | Durable asynchronous projection, batching, no UI/network wait after required local intent |
| `DR-DB-006` | Actor/time, Feature/time, outcome/time, and Process ID index baseline |
| `FR-HIST-001..003` | Personal History remains separate; deletion cannot cascade to Audit |
| `FR-AUTH-001..005`, `FR-ROLE-001..003` | Authentication/session/role/permission evidence and Document 09 access model |
| `FR-SUP-001..007` | Issue-linked safe support action/access/export evidence without issue content |
| `FR-ASSIST-001..015`, `FR-INTEL-001..003` | Actual Actor + Assistant Origin + canonical Feature; local/internal AI route only |
| `SR-SEC-001..023` | Minimized payload, authorization, secret/content exclusion, abuse/security evidence, recovery tests |
| `IR-API-001..012` | Typed/versioned API, idempotency, auth, rate/backpressure, REST authority |

---

# 49. Ketertelusuran Fitur

Baseline canonical tetap 550. Tidak ada Feature ID yang ditambahkan, diganti nama, atau dihapus.

| Kelompok/tindakan Feature | Pemetaan Audit |
|---|---|
| `FEAT-AUDIT-001` | Enterprise Event Catalog and append evidence |
| `FEAT-AUDIT-002..003` | Scoped structured search/filter |
| `FEAT-AUDIT-004` | Bounded async export lifecycle |
| `FEAT-AUDIT-005..007` | Local outbox, batch sync, retry; system capability, not Auditor write permission |
| `FEAT-AUDIT-008` | Governed retention lifecycle; no Auditor mutation |
| 33 Primary PDF Tool Cards | Safe accepted/terminal L2 evidence for every execution |
| `FEAT-PDF-061` | Protect finalization L3 |
| `FEAT-PDF-062` | Unlock finalization L3 |
| `FEAT-PDF-069` | Redaction finalization/validation L3 |
| `FEAT-SIGN-013` | Visual non-PKI signing finalization L3 |
| `FEAT-INTEL-001..002` | Approved route/policy/outcome; no content |
| `FEAT-ASSIST-001..030` | Underlying Feature event with Assistant Origin; special events only at meaningful boundaries |
| `FEAT-SUPPORT-001..008` | Safe Support action/access/export metadata; content remains Support plane |

Kelayakan Feature lokal sekunder dikendalikan Event Catalog. Card UI tidak menetapkan peristiwa atau API baru.

---

# 50. Ketertelusuran Peran

| Role/profile | Hak Audit dan batas bukti |
|---|---|
| User | Menghasilkan bukti yang disetujui; tanpa Enterprise Audit Console; hanya mengelola Riwayat Pribadi |
| Administrator | Menghasilkan bukti control-plane privileged; tanpa read/export Audit atau isi dokumen secara implisit |
| Auditor | `audit.event.list/read` dalam `AUDIT_SCOPE`; `audit.export.create/read` terpisah dan opsional; tanpa mutasi |
| Profil Support | Menghasilkan bukti Support; hanya konteks aman tertaut Issue; tanpa pivot Audit/isi umum |
| Workload | Operasi ingestion/dispatch/export/siklus hidup dengan scope tujuan; tanpa pewarisan human role/permission |
| Assistant | Tanpa role/elevation; permission dan scope pengguna sebenarnya dievaluasi pada setiap langkah dasar |

Permission, ownership/scope, policy, state, context, confirmation, dan reauthentication pada Document 09 tetap berwenang. Visibilitas Audit tidak pernah mengimplikasikan akses terhadap sumber daya/isi yang diaudit.

---

# 51. Ketertelusuran Keamanan

| Baseline keamanan | Implementasi Audit |
|---|---|
| Tolak secara default | Gate permission/scope/category/field/time/environment sebelum kueri/detail/ekspor |
| Autentikasi Pertamina | Server memverifikasi actor/source; Audit tidak mempercayai identity/role dari klien |
| Pemisahan isi | Tanpa payload dokumen pengguna, OCR, Issue, tanda tangan, clipboard, atau Assistant/AI |
| Rahasia | Dilarang dalam event, outbox, SQL, ekspor, log, trace, dan alert |
| Keamanan file | Hanya ID sumber daya opaque; tanpa storage key/path/download token |
| Otoritas REST | SignalR tetap hanya untuk notifikasi dan tidak dapat membuat/mengubah bukti |
| Isolasi Worker | Identity workload, tujuan, lease, executor, dan Actor pemrakarsa terpisah |
| Batas Assistant | Registry/application service canonical; otorisasi dan Audit sama dengan penggunaan langsung |
| Larangan AI publik | `Security.PublicAiRouteBlocked`; tanpa payload provider publik |
| Secure SDLC | Validator schema, canary field terlarang, uji auth negatif, serta kesiapan recovery dan VA/PT |

---

# 52. Ketertelusuran API

Surface Document 06 yang ada tetap berlaku:

```text
POST /api/v1/audit-events/batches
GET  /api/v1/audit-events
POST /api/v1/audit-events/exports
```

Aturan:

- tidak ada endpoint per Feature/card UI;
- state bukti/ekspor yang dipersistenkan melalui REST adalah otoritas;
- SignalR dapat memberi tahu perubahan state, tetapi tidak pernah membawa payload atau perintah Audit yang berwenang;
- daftar menggunakan batas cursor opaque Document 06;
- batch menggunakan autentikasi, `Idempotency-Key`, versi contract/event, ID stabil, batas, dan hasil acknowledgement/conflict per peristiwa;
- otorisasi kueri/ekspor mendahului count/filter/projection;
- problem response/logging tidak pernah menggemakan payload peristiwa;
- OpenAPI/contract diff dan typed contract test tetap wajib.

Serah terima terkendali Document 06 sebelum implementasi: perluas kontrak batch yang ada secara kompatibel dengan `eventType`, `eventVersion`, Audit/client Event ID, source sequence/trust provenance, dan semantik accept/duplicate/reject/conflict per peristiwa. Dokumen ini tidak mengotorisasi perubahan route/API major.

---

# 53. Ketertelusuran Database

Document 07 tetap berwenang untuk:

- satu SQL Server dengan schema milik modul;
- `audit.AuditEvents`, `audit.IngestionBatches`, dan `audit.ExportRequests`;
- `audit_outbox` lokal terenkripsi yang hanya berisi metadata;
- perilaku berorientasi append/tanpa update-delete biasa;
- outbox transaksional milik domain dan referensi logis lintas modul;
- internal key sempit ditambah public ID opaque;
- indeks berbasis bukti, siklus hidup terbatas, arsip terenkripsi terverifikasi, dan pengujian restore;
- pemisahan Riwayat Pribadi, Enterprise Audit, Support, dan Observability.

Serah terima terkendali Document 07 sebelum implementasi:

- promosikan field Event Category, Resource Type/ID, Source, Environment, Event Version, Actor/Origin/Executor, retention class/policy, dan fingerprint yang disetujui serta rutin dicari jika belum tersedia;
- tambahkan outbox owner-module hanya jika pemilik yang disetujui belum memilikinya;
- tambahkan struktur Archive Manifest/Retention Run/Hold hanya setelah persetujuan policy/scale;
- review definisi indeks terhadap predicate otorisasi aktual dan bukti Query Store.

Database kedua, outbox global tanpa pemilik, write Feature langsung lintas schema, dan pencarian JSON arbitrer tidak diotorisasi.

---

# 54. Kriteria Penerimaan

## 54.1 Fungsional dan Kontrak

- Keputusan 20/20 diimplementasikan tanpa jalur peristiwa alternatif.
- Seluruh 33 Core Tool menghasilkan bukti accepted/terminal aman yang disetujui.
- Setiap peristiwa yang dirilis dipetakan ke satu record type/version/schema/catalog.
- Field Actor, Origin, Executor, target, outcome, Feature/Process/Job/Correlation yang sebenarnya benar bila diwajibkan.
- Jalur langsung UI/API/Assistant/Worker menghasilkan bukti berwenang yang ekuivalen.
- Snapshot OpenAPI/contract, diff, dan uji kompatibilitas lulus.

## 54.2 Keamanan dan Privasi

- pemeriksaan permission/scope/field/category/time/environment dilakukan sebelum count/query/detail/export;
- kasus negatif User/Admin/Auditor/Support/workload lulus;
- canary field terlarang tidak ditemukan dalam request, outbox, database, ekspor, log, trace, metrik, alert, arsip, dan bukti uji backup;
- Assistant tidak dapat melakukan elevation, menciptakan Feature, atau menyembunyikan Actor sebenarnya;
- akses isi tidak diturunkan dari ID Audit;
- grant database append/update/delete dan jalur aplikasi sesuai desain.

## 54.3 Luring, Integritas, dan Pemulihan

- peristiwa luring dipersistenkan sebelum finalisasi tindakan wajib sesuai kelas transaksinya;
- sinkronisasi reconnect terbatas, terautentikasi, idempotent, aman dari duplikasi, dan mempertahankan waktu occurred/received;
- replay yang sama disimpan sekali; replay berkonflik ditolak/diberi alert;
- skenario crash sebelum/setelah commit/append/ack pulih tanpa kehilangan/duplikasi bukti semantik;
- perilaku queue-full/corrupt/version-blocked terlihat dan tidak pernah mengeluarkan bukti wajib secara diam-diam;
- backup/restore mempertahankan schema, uniqueness, permission, outbox, hold/policy, dan kemampuan append.

## 54.4 Kinerja dan Operasi

- workload Merge/OCR/conversion representatif menunjukkan overhead Audit yang terbatas;
- pekerjaan berat engine/file/provider tetap di luar transaksi/koneksi DB;
- backlog terkuras setelah outage/burst yang diuji tanpa retry storm;
- query plan tetap terbatas pada volume representatif dan scope mendahului count;
- export/archive/purge terbatas, dapat dilanjutkan, di-throttle, dan tidak menyebabkan starvation;
- health state/alert mengungkap kegagalan write, backlog, poison version, kapasitas antrean, ekspor, arsip, dan risiko hold tanpa kebocoran payload;
- Audit wajib tidak dapat dinonaktifkan untuk meluluskan benchmark.

Ambang lulus numerik diselesaikan dalam Document 12 dan policy resmi.

---

# 55. Konfirmasi dan Serah Terima Terkendali

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION

- retention duration by class
- authoritative retention clock for offline/late evidence
- legal/investigation hold authority and approval workflow
- organization/unit/region Audit scope vocabulary and grant authority
- official classification, masking, and exceptional IP/device policy
- ordinary-L2 degraded behavior when the protected Audit queue is unavailable/full
- query/export/offline-queue/batch/archive numeric limits
- RPO/RTO, SIEM/alert route, and on-call ownership
- archive technology only if measured/policy need exists
```

Prasyarat implementasi:

1. addendum batch-contract Audit Document 06 yang terkendali;
2. addendum typed-field/outbox/lifecycle Document 07 yang terkendali hanya jika belum ada;
3. benchmark dan ambang penerimaan numerik Document 12;
4. konfirmasi policy resmi sebelum konfigurasi purge/hold/archive Produksi.

Butir tersebut tidak membuka kembali desain logis atau mengotorisasi edit diam-diam terhadap Documents 01–09.

---

# 56. Audit Akhir dan Status

```text
Specialist reports integrated : 4 / 4
Parent FINAL documents audited: 9 / 9
Audit decisions               : 20 / 20 AUTO-LOCKED
Required areas                : 54 / 54
Canonical features            : 550 / 550
Primary PDF Tool Cards        : 33 / 33
Resolved conflicts            : 20
Material contradiction        : 0
Sensitive payload added       : 0
New role/vendor/database      : 0
User approval required        : 0
Document status               : FINAL v1.0
```

Dokumen berikutnya:

```text
11_UNIT_TEST_DOCUMENT.md
```
