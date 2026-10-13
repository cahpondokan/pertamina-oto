# AGENT C — DATABASE PERFORMANCE REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC07-AGENT-C` |
| Fokus | Performance, indexing, concurrency, retention, cleanup, archive, dan server budget |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 24 Agustus 2026 |
| Decision owner | User + Integrator |

## Baseline, batas, dan catatan sumber

Laporan ini mengikuti baseline berikut:

- `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`;
- `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md`;
- `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md`;
- `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md`;
- `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md`;
- kontrak locked `API-DEC-001..009` pada `docs/06-api/06_API_DOCUMENTATION_WORKING.md` v0.5 dan decision records terkait.

Catatan governance: workspace belum mempunyai artifact bernama Document 06 FINAL; file terintegrasi masih berstatus `WORKING — DECISION PHASE`, walaupun 9/9 keputusan API sudah locked. Rekomendasi database di bawah menggunakan keputusan locked tersebut dan MUST diaudit ulang terhadap artifact 06 FINAL sebelum Document 07 dikunci.

Laporan ini:

- tidak mengubah master Document 07;
- tidak menetapkan physical table/column name final;
- tidak membuat kode atau SQL DDL;
- tidak menghapus fitur karena server awal kecil;
- tidak mengunci angka retention, pool, batch, timeout, concurrency, atau capacity sebelum benchmark/load test dan persetujuan user + Integrator.

Nama tabel dan kolom pada laporan adalah **logical design notation**. Integrator menyesuaikannya dengan naming convention final tanpa mengubah query shape dan invariant yang direkomendasikan.

Traceability utama: `FR-HIST-001..003`, `FR-AUDIT-001..004`, `FR-REC-001..006`, `FR-BACKUP-001..010`, `FR-VER-001..004`, `FR-SUP-001..007`, `NFR-PERF-001..013`, `NFR-REC-001..005`, `DR-DB-001..007`, `IR-API-003..012`, Solution Design bagian 8, 9, 11, 17, 19, 21, 22, 24, 27, dan 28, serta `API-DEC-004..009`.

---

## 1. Recommended indexes

### 1.1 Prinsip key dan clustered storage

Untuk tabel write-heavy seperti audit, jobs, job attempts, idempotency, uploads, dan issue activity:

- gunakan internal clustered key yang sempit dan insertion-friendly, misalnya sequential numeric surrogate;
- public/API ID tetap opaque dan mempunyai unique nonclustered index;
- public opaque ID bukan clustered key default bila pola pembentukannya menghasilkan random page split;
- external Pertamina subject/claim tidak diulang sebagai string panjang pada setiap tabel; resolve ke internal user/account key yang sempit, dengan unique mapping pada identity schema;
- sequential internal key tidak boleh menjadi authorization authority atau diekspos sebagai enumerable public ID;
- foreign key pada tabel besar MUST mempunyai supporting index bila digunakan untuk join, delete/retention validation, atau ownership query.

Timestamp bisnis dan timestamp penerimaan dipisahkan bila dibutuhkan. Audit offline sync, misalnya, perlu membedakan `OccurredAt` dari `ReceivedAt`; late arrival tidak boleh merusak urutan bisnis atau retention tanpa policy yang jelas.

### 1.2 Kandidat indeks per logical table

Notation:

- kolom sebelum `INCLUDE` adalah index key;
- `DESC` mengikuti default cursor order dari `API-DEC-006`;
- `WHERE` berarti filtered index untuk hot subset;
- seluruh kandidat harus diverifikasi dengan execution plan dan Query Store sebelum dikunci.

| Logical table | Indeks minimum/kandidat | Tujuan |
|---|---|---|
| `identity.UserAccount` | unique `(IssuerKey, ExternalSubject)`; unique `(PublicUserId)` | login mapping tanpa mengulang subject string panjang di child table |
| `history.ProcessHistory` | unique `(HistoryId)`; `(OwnerUserRowId, CompletedAt DESC, HistoryRowId DESC) INCLUDE (FeatureId, Status, FileName, FileSizeBytes, DurationMs, ExecutionMode)` | personal history cursor, proyeksi awal maksimal 50 |
| `history.ProcessHistory` conditional | `(OwnerUserRowId, FeatureId, CompletedAt DESC, HistoryRowId DESC) INCLUDE (Status)`; status-leading variant hanya jika workload membuktikan manfaat | filter Feature ID/status tanpa membuat indeks untuk setiap kombinasi |
| `audit.AuditEvent` | unique `(AuditEventId)`; `(OccurredAt DESC, AuditRowId DESC)` | global authorized time-range/cursor query |
| `audit.AuditEvent` | `(UserRowId, OccurredAt DESC, AuditRowId DESC)`; `(FeatureId, OccurredAt DESC, AuditRowId DESC) INCLUDE (Status)` | memenuhi query user/feature/time pada `DR-DB-006` |
| `audit.AuditEvent` | `(Status, OccurredAt DESC, AuditRowId DESC)`; `(ProcessId, OccurredAt DESC, AuditRowId DESC)` | status/time dan process trace; status index dikaji ulang jika selectivity sangat rendah |
| `jobs.Job` | unique `(JobId)`; `(OwnerUserRowId, CreatedAt DESC, JobRowId DESC) INCLUDE (FeatureId, Status, Progress, CurrentStepCode, CompletedAt)` | Activity Center/history job cursor |
| `jobs.Job` worker queue | `(WorkloadClass, AvailableAt, CreatedAt, JobRowId) INCLUDE (FeatureId, CostWeight, OwnerUserRowId)` filtered untuk queue-admissible state | dequeue yang bounded dan fair tanpa scan seluruh job table |
| `jobs.Job` lease recovery | `(LeaseExpiresAt, JobRowId) INCLUDE (LeaseOwner, Status)` filtered untuk `RUNNING` | reclaim/recovery Worker yang kehilangan lease |
| `jobs.JobAttempt` | unique `(JobRowId, AttemptNumber)`; `(JobRowId, StartedAt DESC, AttemptRowId DESC)` | append-only retry/resume attempt history |
| `jobs.IdempotencyRecord` | unique `(ScopeHash, KeyHash)` INCLUDE narrow fingerprint/resource/status fields; `(ExpiresAt, IdempotencyRowId)` | atomic replay lookup dan TTL cleanup |
| `jobs.QuotaReservation` | unique active reservation identity; `(OwnerUserRowId, WorkloadClass, ExpiresAt)` | atomic quota accounting dan expired reservation cleanup |
| `workspace.UploadSession` | unique `(UploadId)`; `(OwnerUserRowId, State, CreatedAt DESC, UploadRowId DESC)`; `(ExpiresAt, UploadRowId)` filtered untuk nonterminal/expired-cleanable state | ownership query, resume, dan orphan/expiry cleanup |
| `support.Issue` | unique `(IssueId)`; `(ReporterUserRowId, CreatedAt DESC, IssueRowId DESC) INCLUDE (PublicStatus, UpdatedAt)` | status issue milik user |
| `support.Issue` | `(Status, CreatedAt DESC, IssueRowId DESC) INCLUDE (Category, Priority, ReporterUserRowId, AppVersion, ErrorCode)` | Support Console default filter/cursor |
| `support.Issue` diagnostic lookup | filtered `(CorrelationId, CreatedAt DESC)` dan/atau `(JobId, CreatedAt DESC)` hanya bila lookup dipakai nyata | korelasi safe diagnostics tanpa mengindeks message |
| `support.IssueAttachment` | `(IssueRowId, AttachmentRowId)`; cleanup index pada retention/expiry field bila attachment retention dipisah | detail issue dan controlled attachment cleanup |
| `backup.SecuredFile` | unique `(SecuredFileId)`; `(OwnerUserRowId, SecuredAt DESC, SecuredFileRowId DESC) INCLUDE (DisplayName, MimeType, SizeBytes, CurrentVersionNumber, LifecycleStatus)` | secured-file cursor setelah ownership scope |
| `backup.SecuredFile` lifecycle | `(LifecycleStatus, RetentionUntil, SecuredFileRowId)` filtered untuk recycle/retention candidate | account recycle/retention worker tanpa full scan |
| `backup.SecuredFileVersion` | unique `(SecuredFileRowId, VersionNumber)`; `(SecuredFileRowId, CreatedAt DESC, VersionRowId DESC)` | maksimum tiga versi, version listing, dan rotation invariant |
| `platform.OutboxEvent` bila transactional outbox dipilih | unique `(EventId)`; `(OccurredAt, OutboxRowId)` filtered untuk unpublished state; `(PublishedAt, OutboxRowId)` untuk cleanup | persist-before-notify dan reliable audit/SignalR publication |
| `admin.Policy` / configuration | unique `(PolicyScope, PolicyKey)`; index pada effective/version state hanya jika query memerlukan | satu effective policy per scope dan optimistic update |

Catatan penting:

- `Message`, `InternalNote`, document content, OCR text, screenshot, token, password, storage object bytes, large JSON, dan `nvarchar(max)` tidak menjadi index key/include.
- Storage object tetap di secure file/object storage. SQL menyimpan metadata/reference, bukan file blob default.
- Storage key/reference yang sensitif tidak ikut covering list index. Ambil hanya setelah object-level authorization pada detail/download path.
- Candidate index pada status tidak otomatis bermanfaat karena cardinality rendah. Kombinasikan dengan time/range dan ukur selectivity.
- Index pada `CorrelationId`, `JobId`, atau Error Code hanya dibuat bila lookup frequency dan selectivity membenarkannya.

---

## 2. Composite-index strategy

### 2.1 Urutan key

Composite index mengikuti pola:

```text
server-enforced scope/equality
→ selective equality filter
→ range/timestamp
→ unique cursor tie-breaker
→ narrow projected columns as INCLUDE
```

Contoh logical shape:

```text
(OwnerUserRowId, CompletedAt DESC, HistoryRowId DESC)
```

Shape tersebut mendukung query:

```text
owner = current user
AND (CompletedAt, HistoryRowId) < cursor
ORDER BY CompletedAt DESC, HistoryRowId DESC
TAKE pageSize
```

Aturan:

- ownership/authorization predicate yang selalu ada ditempatkan sebelum cursor columns;
- equality columns sebelum range columns;
- timestamp diikuti unique tie-breaker agar keyset continuation stabil;
- `INCLUDE` hanya untuk kolom kecil yang benar-benar muncul pada list projection;
- mixed sort yang tidak mempunyai supporting index tidak dibuka bebas pada API;
- API sort/filter allowlist dan database index catalog MUST disusun bersama;
- wide report dengan kombinasi filter tidak umum menjadi asynchronous export, bukan alasan membuat seluruh kombinasi composite index;
- index prefix yang sudah tercakup tidak diduplikasi oleh index lain tanpa bukti;
- unique business invariant memakai unique constraint/index, bukan hanya application check.

### 2.2 Cursor dan page support

- Audit, issue, secured-file, history/job, dan large/changing list memakai keyset cursor; tidak memakai deep `OFFSET`.
- History/job default 25 dan maximum 50; maximum lain mengikuti `API-DEC-006`.
- Page/offset hanya untuk bounded ordinary metadata list.
- Exact `COUNT(*)` tidak menjadi default pada large cursor list.
- Authorization scope diterapkan sebelum cursor predicate, pagination, count, dan projection.
- Cursor field harus sama dengan leading ordered portion index setelah scope/filter columns.
- Late audit event tetap dapat muncul pada refresh baru; continuation lama tidak boleh bergantung pada timestamp saja tanpa unique tie-breaker.

### 2.3 Filter dan sort support

- Default order mendapat index prioritas tertinggi.
- Filter umum mendapat composite variant setelah telemetry membuktikan frekuensinya.
- Low-cardinality status tidak diindeks sendirian pada large table.
- Secondary sort yang jarang dipakai dapat dijalankan hanya pada bounded result setelah selective scope/date filter, atau dipindahkan ke async export.
- `ORDER BY` arbitrary, dynamic column, regex, dan raw expression tetap dilarang sesuai Document 06.

---

## 3. Avoid over-indexing

Setiap nonclustered index menambah storage, buffer-pool pressure, transaction-log volume, insert/update/delete cost, lock duration, backup size, dan cleanup cost. Audit/job tables adalah area paling rentan terhadap write amplification.

### 3.1 Index governance

Setiap index candidate harus mempunyai:

```text
Owner query / endpoint / worker
Authorization scope
Filter + sort pattern
Expected cardinality/selectivity
Projection
Read frequency
Write cost
Retention/cleanup impact
Execution-plan evidence
Review date
```

Jangan membuat index karena satu kolom "terlihat penting". `DR-DB-006` dipenuhi dengan composite query indexes untuk user, time, feature, status, dan Process ID; tidak harus satu index tunggal untuk setiap kolom jika satu composite shape memberi dukungan nyata.

### 3.2 Larangan dan guard

- Tidak ada index untuk setiap kombinasi filter.
- Tidak ada duplicate/near-duplicate index dengan prefix dan INCLUDE yang sama.
- Tidak ada global fill factor non-default tanpa measured page-split evidence.
- Tidak ada scheduled rebuild semua index setiap malam. Maintenance berdasarkan page count, fragmentation, write load, maintenance window, dan actual benefit.
- Tidak ada index besar pada free text, payload JSON, message, diagnostics blob, atau external storage key untuk mempercepat list.
- Tidak ada unused-index removal hanya dari satu snapshot singkat; perhatikan seasonal/month-end/audit workload dan restart reset DMV.
- Jangan memakai "missing index" recommendation secara otomatis; rekomendasi tersebut tidak memahami write cost, overlap, filtered index, retention, dan authorization model.
- Statistics harus sehat. Auto statistics, sampled/fullscan policy, dan incremental statistics untuk partitioned table ditetapkan DBA berdasarkan table size serta change rate.
- Query Store digunakan untuk plan regression, top resource queries, dan validasi index; bukan merekam sensitive parameter/content.
- Parameter-sensitive plan untuk filter audit/issue perlu diuji. Query shape dapat dipisah secara controlled bila satu generic plan tidak stabil.

### 3.3 Optional advanced storage

- Row/page compression MAY digunakan pada cold audit/history setelah CPU-versus-I/O benchmark.
- Partitioning MAY digunakan untuk large append/retention table setelah volume, late-arrival policy, archive boundary, operational maturity, dan edition/feature support terbukti.
- Columnstore MAY dipertimbangkan untuk cold analytic/archive workload, bukan operational queue/job table dan bukan default hot audit table.
- Read replica, separate reporting database, atau external message broker bukan baseline; tambahkan hanya jika measurement membuktikan kebutuhan.

---

## 4. Query-pattern mapping

### 4.1 Primary query map

| Domain | Query pattern | Index shape | Performance guard |
|---|---|---|---|
| Personal history | current owner + optional feature/status + cursor by completion time | owner + optional selective equality + completed time + row ID | 25/50; metadata projection; no exact total default |
| Audit list | authorized scope + date range + optional user/unit/feature/status/platform + cursor | scope/filter + occurred time + row ID | date-span policy; max 100; no content; async export for wide range |
| Process trace | exact Process ID ordered by event time | process ID + occurred time + row ID | exact/prefix ID only; bounded events per process |
| Job lookup | exact opaque Job ID + ownership/permission | unique Job ID, then authorization join/check | no internal row ID lookup from client |
| Activity Center | current owner + recent jobs + cursor | owner + created time + row ID | 25/50; narrow progress projection |
| Worker dequeue | admissible state + workload class + available time | filtered workload + available time + creation/row ID | atomic lease; bounded fetch; fairness/backpressure |
| Lease recovery | running + expired lease | filtered lease expiry + row ID | single/bounded recovery worker; no duplicate execution |
| Idempotency | exact scope hash + key hash | unique scope/key hash | same fingerprint replays; mismatch conflict |
| Issue user list | reporter + created cursor | reporter + created time + row ID | only own issue; simplified status projection |
| Support issue list | permission scope + status/category/priority/date + cursor | status/selective filter + created time + row ID | no message/screenshot in list; async export |
| Diagnostic issue lookup | exact Correlation ID/Job ID/Error Code if supported | filtered identifier index | safe identifiers only; no log text search |
| Secured-file list | owner/permission + secured cursor + optional safe metadata filter | owner + secured time + row ID | no content/OCR search; object access separate |
| File versions | exact secured-file parent ordered by version/time | file row ID + version/time | maximum three versions; unique version number |
| Retention cleanup | lifecycle/terminal state + expiry before cutoff | state + expiry + row ID | small batches, checkpoint, throttle, no request-thread cascade |
| Outbox publication | unpublished ordered events | filtered occurred time + row ID | short claim transaction; publish after commit; idempotent consumer |

### 4.2 Search recommendation

Search MUST mengikuti `API-DEC-006`:

- exact search untuk opaque IDs, Process ID, Job ID, Issue ID, Correlation ID, dan canonical Feature ID;
- prefix/normalized metadata search hanya pada allowlisted field dan selalu setelah ownership/date/selective scope;
- filename search tidak boleh menjadi storage scan dan tidak mencari document content;
- issue `Message` dan `InternalNote` tidak diindeks sebagai B-tree dan bukan default API list search;
- leading-wildcard `LIKE '%term%'` pada large table dilarang sebagai primary query path;
- SQL Full-Text Search hanya dapat ditambahkan setelah explicit requirement, Security Design review, permission-aware search design, storage/capacity test, dan keputusan user + Integrator. Document 06 saat ini melarang document-content search;
- search result tetap keyset-paginated dan permission-scoped.

### 4.3 EF Core/query behavior

- Gunakan projection langsung ke list contract; jangan materialize entity graph penuh.
- Read-only list menggunakan no-tracking behavior.
- Hindari N+1 query untuk owner, attachment count, version, dan status labels.
- Large `IN` list, multiple collection include, client-side evaluation, dan unbounded navigation loading dilarang.
- Split query hanya dipakai setelah mengukur trade-off round-trip versus cartesian explosion.
- Cancellation dan bounded command timeout diteruskan ke database operation.
- Database query tidak mengambil storage object/file bytes.

---

## 5. Optimistic concurrency

### 5.1 Aggregate yang memerlukan concurrency token

Gunakan SQL Server `rowversion` atau equivalent database-managed concurrency token untuk mutable aggregate:

- `jobs.Job` current state/progress/lease;
- `workspace.UploadSession` state/committed offset;
- `support.Issue` triage/status/category/priority/internal-note metadata;
- `backup.SecuredFile` lifecycle/current version pointer;
- `admin.Policy` dan security/configuration record;
- quota/accounting record bila memakai mutable counter.

Append-only record seperti `audit.AuditEvent`, `jobs.JobAttempt`, dan immutable secured-file version tidak memerlukan general update concurrency token. Perubahan dilakukan dengan insert baru atau controlled lifecycle operation.

### 5.2 State-transition guard

- Update job/upload/issue/file MUST membawa expected version dan expected current state.
- Terminal job state tidak boleh kembali ke nonterminal state tanpa explicit recovery contract.
- Worker completion harus cocok dengan active lease owner/token dan unexpired/accepted lease.
- Upload offset update harus compare expected committed offset; chunk replay yang sama tetap idempotent.
- Secured-file version number mempunyai unique invariant per parent.
- Idempotency key mempunyai unique scope invariant; read-then-insert tanpa unique constraint tidak cukup.
- Quota reservation/counter memakai atomic update atau unique reservation, bukan read-then-write yang race-prone.
- Concurrency conflict tidak diatasi dengan silent last-write-wins pada admin, issue triage, secured-file lifecycle, atau job state.
- API conflict semantics harus diselaraskan Integrator dengan contract final, misalnya version token/ETag dan safe `409`/`412` behavior; laporan ini tidak membuka keputusan API baru.

### 5.3 Pessimistic lock yang diperbolehkan

Pessimistic locking hanya untuk invariant sempit dan transaksi sangat singkat:

- claim/dequeue satu batch job;
- reserve/release quota;
- rotate maksimum tiga secured-file version;
- claim cleanup/outbox batch;
- transition upload/session state yang harus tunggal.

Tidak ada database lock atau transaction yang dipertahankan selama file upload/download, OCR, conversion, AI call, antivirus scan, object-storage operation, SignalR delivery, atau seluruh lifetime background job.

---

## 6. Transaction recommendations

### 6.1 Transaction boundaries

| Use case | Satu short transaction mencakup | Di luar transaction |
|---|---|---|
| Job admission | idempotency record, job row, initial state, quota reservation, optional outbox/audit intent | upload validation yang sudah selesai, Worker execution, provider call |
| Worker dequeue | select/claim bounded jobs, lease owner/expiry, state/version update | seluruh OCR/conversion/AI/PDF processing |
| Job progress | coalesced persisted progress/current state + optional outbox notification | high-frequency UI calculation dan SignalR transport |
| Job completion | verified output reference, terminal job state, attempt result, reservation release, outbox/audit intent | output generation dan object-storage upload/validation |
| Upload finalize | validated staging reference, size/digest metadata, READY state/version | streaming, malware scan, deep file inspection |
| Secured-file save/version | metadata, owner, current version pointer, max-three-version invariant, retention action record | object bytes transfer/encryption and later physical cleanup |
| Issue submit | issue metadata + attachment metadata reference + audit/outbox intent | screenshot capture, streaming, scan, storage write |
| Issue triage/admin change | expected rowversion update + mandatory audit/outbox event | export generation and notification delivery |
| Cleanup/archive claim | claim bounded candidate set + checkpoint | external archive/object deletion; finalize in follow-up transaction |

### 6.2 External storage consistency

SQL Server dan secure file/object storage tidak memakai long distributed transaction. Gunakan stateful, recoverable workflow:

1. simpan object pada staging/quarantine dengan scoped reference;
2. validasi dan scan;
3. commit metadata/reference pada short database transaction;
4. promote/finalize object bila diperlukan;
5. cleanup worker merekonsiliasi orphan object atau metadata bila salah satu sisi gagal.

Deletion juga dua tahap: mark/claim sesuai policy, delete object secara idempotent, lalu finalize/tombstone metadata. Jangan menghapus metadata authority lebih dahulu sehingga object orphan tidak dapat direkonsiliasi.

### 6.3 Audit dan event atomicity

- Perubahan permission/configuration/issue triage/job terminal yang wajib diaudit tidak boleh memakai unreliable dual write.
- Bila audit module berbeda, gunakan transactional outbox pada owner module; publish setelah commit dan insert audit event secara idempotent.
- SignalR event hanya dipublikasikan setelah persisted state commit sesuai `API-DEC-008`.
- Outbox consumer at-least-once; `EventId` unique mencegah duplicate audit/event effect.
- App request tidak menunggu long audit export atau archive.

### 6.4 Isolation dan deadlock

- `READ_COMMITTED_SNAPSHOT` adalah kandidat kuat untuk mengurangi reader-writer blocking pada list/audit/history, tetapi hanya dikunci setelah DBA review, `tempdb` sizing, compatibility test, dan load test.
- Queue claim menggunakan atomic update/claim pattern atau short locking read dengan skip-locked/read-past semantics yang disetujui; exact SQL implementation dipilih pada coding phase.
- `SERIALIZABLE` hanya untuk invariant sempit bila unique constraint/atomic update tidak cukup.
- Selalu akses aggregate/table dalam order yang konsisten untuk mengurangi deadlock.
- Deadlock/transient retry bounded, memakai backoff+jitter, dan hanya pada command yang idempotent atau transaction-safe.
- Tidak ada blind retry seluruh workflow eksternal.

### 6.5 Connection usage

- Gunakan connection pooling per deployable, tetapi total pool budget dihitung dari seluruh API replica, Web, Server Worker, migration/admin process, dan monitoring connection.
- Pool maximum tidak ditetapkan per service secara terpisah tanpa memastikan total potential connection berada di bawah safe SQL Server capacity.
- `DbContext`/connection berumur pendek per database operation atau unit of work; bukan singleton dan bukan lifetime job.
- Jangan menahan connection selama response streaming, file I/O, queue wait, provider call, CPU processing, user think-time, SignalR connection, atau retry delay.
- Worker concurrency harus lebih kecil/sama dengan database operation capacity; backpressure terjadi sebelum pool exhaustion.
- Async database I/O dan narrow projection digunakan untuk mengurangi thread/connection occupancy.
- Bounded command timeout berbeda menurut kategori query; export besar menjadi background workload, bukan menaikkan timeout semua endpoint.
- Connection resiliency hanya untuk transient allowlist dan transaction-aware retry.
- Health/readiness mendeteksi pool/DB pressure tanpa membuka detail sensitif.
- Migration dan heavy index maintenance tidak berjalan bersamaan dengan peak processing tanpa change window.

---

## 7. Retention/archive strategy

### 7.1 Prinsip umum

- Retention adalah typed enterprise policy dengan system safety bounds, permission, audit perubahan, dan legal-hold override.
- Retention dihitung per data class, bukan satu global TTL.
- Data tidak dihapus hanya karena server kecil; data dipaginasi, dibatasi, diarsipkan, dibersihkan bertahap, atau dipindahkan ke approved cold storage sesuai policy.
- Archive tetap encrypted, access-controlled, auditable, dan mempunyai integrity manifest berisi range, count, checksum/digest, schema version, waktu, serta archive location reference.
- Archive/purge worker idempotent, checkpointed, bounded, throttled, observable, dan dapat dilanjutkan.
- Purge memakai ordered small batches berdasarkan retention key + row ID. Hindari satu massive `DELETE` yang memenuhi transaction log dan memblokir hot table.
- Bila partition switching akhirnya dipilih, semua index/constraint, late-arrival policy, archive restore, dan operational runbook harus partition-aligned.

### 7.2 Retention matrix

| Data class | Hot/operational retention | Archive/purge guard |
|---|---|---|
| Enterprise audit | nilai ditetapkan compliance/Document 10; append-only | archive sebelum purge bila policy mewajibkan; legal hold; privileged audited operation; user-history delete tidak memengaruhi audit |
| Personal/server history | policy user/business; pisahkan dari local SQLite history | user delete/retention hanya history; tidak cascade ke audit/job evidence; metadata-only |
| Job current metadata | sampai terminal + operational support/retry window | nonterminal tidak dipurge; minimal audit trace dipertahankan sesuai audit policy |
| Job attempts/progress events | lebih pendek dari job summary bila support tidak membutuhkan detail lama | compact/purge setelah terminal dan support window; current persisted state tetap source of truth |
| Idempotency records | sekurang-kurangnya mencakup client retry/replay window dan resource lifecycle yang relevan | expiry indexed; jangan purge saat request/job masih dapat replay/resume |
| Upload sessions/temp metadata | short operational TTL setelah terminal/expired | jangan hapus data yang masih dibutuhkan resume/validation; reconcile partial object |
| Unsecured temporary result/checkpoint | sampai completion/recovery window | hanya explicit **Amankan ke Akun** membuat durable secured copy; orphan cleanup wajib |
| Secured file metadata | mengikuti ownership/account recycle/retention policy | tidak dipurge oleh generic temp cleanup; object-level authorization; max tiga versi |
| Secured-file versions | maksimum tiga versi terakhir | versi keempat memicu oldest-version action sesuai `FR-VER-002`; metadata/object cleanup recoverable dan policy-aware |
| Issue metadata | support/incident retention policy | legal/support hold, status workflow, export trace, dan permission |
| Issue attachment | policy dapat berbeda dari issue metadata | object storage, explicit user send, access audit, secure purge, attachment failure tidak menghilangkan issue message |
| Outbox/realtime delivery rows | short operational retention setelah published/reconciled | unpublished/failed event tidak dipurge; archive tidak menggantikan persisted job/resource state |
| Application log/telemetry | Application Insights/APM policy, bukan operational SQL default | sampling/data minimization; tidak berisi document/password/token/OCR content |

### 7.3 Hot/cold access

- Live UI tidak otomatis `UNION` seluruh hot + archive untuk setiap page.
- Recent online data menggunakan hot indexes.
- Older authorized range menggunakan explicit archive query/export workflow.
- Wide audit/issue/history export tetap asynchronous, rate-limited, dan mempunyai output retention.
- Archive restore/read mempunyai contract dan runbook; archive bukan tempat pembuangan data tanpa kemampuan verifikasi.

---

## 8. Audit growth control

### 8.1 Data shape dan ingestion

- Audit menyimpan metadata minimum yang diwajibkan; tidak menyimpan document content, password, token, clipboard, screenshot, full OCR text, signature asset, prompt/chat penuh, atau raw provider response.
- Bedakan `AuditEventId`, `ProcessId`, `JobId`, dan `CorrelationId`; masing-masing bukan pengganti yang lain.
- Setiap offline audit event mempunyai stable unique event ID agar batch sync/retry tidak menggandakan row.
- Simpan `OccurredAt` dan `ReceivedAt` untuk late/offline event analysis; policy menentukan timestamp mana yang mengontrol retention.
- Batch audit sync bounded; satu batch memakai short insert transaction dan backpressure agar tidak menghambat UI/API.
- Audit append-only. Koreksi menggunakan compensating/new event, bukan update history diam-diam.
- User personal-history delete tidak melakukan cascade ke enterprise audit.

### 8.2 Write/read balance

- Clustered insert key tetap sequential untuk mengurangi page split meskipun `OccurredAt` datang terlambat.
- Initial nonclustered index set harus minimal tetapi memenuhi `DR-DB-006`; ukur write amplification setiap tambahan index.
- Global time, user/time, feature/time, status/time, dan Process ID paths diuji dengan volume produksi proyeksi.
- Dashboard/count tidak menjalankan repeated full-table count. Gunakan bounded range, cached/summary metric yang dapat direkonsiliasi, atau async report bila requirement mengizinkan.
- Audit export membaca snapshot/range secara asynchronous dan tidak memblokir ingestion.
- Hot audit table dipisahkan secara lifecycle dari archive. Partitioning/compression baru diterapkan setelah threshold dan operational evidence.

### 8.3 Capacity model audit

Integrator perlu menghitung:

```text
audit rows/day
= auth events
+ feature/process events
+ job state/terminal events
+ role/permission/config changes
+ issue/support changes
+ export/security events

hot audit footprint
= row bytes × retained hot rows
+ clustered/nonclustered index bytes
+ transaction-log/backup overhead
+ safety headroom
```

Jangan mencatat setiap progress tick sebagai audit event. Progress state dan audit event mempunyai tujuan berbeda; event catalog final berada di Document 10.

---

## 9. History growth control

- Pisahkan local history metadata pada SQLite (`TECH-DEC-011`) dari enterprise/server history pada SQL Server. Jangan menyinkronkan seluruh local history secara otomatis tanpa requirement.
- Server history hanya menyimpan metadata yang dibutuhkan API/account/server-job behavior: nama/display metadata yang diizinkan, Feature ID, waktu, status, result reference, ukuran, durasi, dan execution mode.
- Full local path tidak disinkronkan. Gunakan opaque result/secured-file reference untuk server resource.
- Default cursor owner + completion time + unique ID; maximum 50 sesuai `FR-HIST-003` dan `API-DEC-006`.
- Personal history delete tidak menghapus enterprise audit, secured file, atau job evidence yang mempunyai retention terpisah.
- Tidak ada auto-include attachment, document content, OCR output, workflow payload besar, atau full error detail pada history list.
- Duplicate server history akibat retry dicegah oleh stable logical process/job identity atau unique business constraint.
- Cleanup history berdasarkan owner/time/retention memakai batch. Satu user dengan volume tinggi tidak boleh memblokir cleanup user lain.
- Old history export/search mengikuti async/date-range policy; deep offset dan exact global count dihindari.
- History table tidak menjadi pengganti observability log, audit trail, job attempt log, atau secured-file catalog.

---

## 10. Job cleanup

### 10.1 Lifecycle guard

Cleanup job MUST membedakan:

```text
active job
terminal job summary
job attempt
progress/event detail
idempotency record
quota reservation
checkpoint
temporary input/output
secured result
audit evidence
```

Aturan:

- `QUEUED`, `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, dan `RETRYING` tidak dipurge sebagai terminal cleanup.
- Job dengan expired lease direcovery/reconciled dahulu; tidak langsung dianggap selesai atau dihapus.
- Terminal job summary dipertahankan selama operational support/retry window yang disetujui.
- Attempt/progress detail MAY mempunyai retention lebih pendek setelah terminal summary aman.
- Idempotency record tidak dipurge sebelum replay window berakhir dan resource/job tidak lagi membutuhkan identity replay.
- Expired quota reservation dilepas secara idempotent; accounting direkonsiliasi.
- Checkpoint/temp input/output dibersihkan hanya setelah tidak dibutuhkan resume/retry dan tidak mempunyai active reference.
- Hasil yang sudah menjadi secured file tidak ikut generic job-temp cleanup.
- Minimal audit metadata tetap mengikuti audit retention walaupun operational job detail dipurge.
- Delete tidak memakai synchronous cascade besar pada request/Worker completion transaction.

### 10.2 Cleanup execution

- Gunakan expiry/terminal-time + row-ID index.
- Claim batch kecil, commit, hapus/reconcile external object, lalu finalize metadata.
- Satu logical cleanup partition tidak diproses ganda; gunakan short lease/claim.
- Batch size, delay, concurrency, dan maintenance window berasal dari policy/capacity evidence.
- Cleanup menurunkan concurrency saat DB log, storage I/O, CPU, atau user workload pressure tinggi.
- Progress/checkpoint cleanup dipersist agar dapat resume setelah crash.
- Orphan scanner membatasi scope/prefix/time; tidak memindai seluruh object storage tanpa batas.
- Metrics minimum: candidate count, processed, skipped-active, failed, retry, object-orphan, metadata-orphan, duration, rows/log bytes, dan oldest pending age—tanpa content atau storage secret.

---

## 11. Capacity risks and server budget

### 11.1 Server-efficiency analysis

| Dimensi | Risiko utama | Rekomendasi | Metric minimum |
|---|---|---|---|
| CPU | table scan, sort/hash besar, excessive index maintenance, full-text/free-text search | keyset + allowlist + aligned index; async export; measured maintenance; local processing tetap diprioritaskan | DB CPU, top query CPU, compile/recompile, Worker CPU |
| RAM | large memory grant, wide projection, entity graph, buffer-pool pressure, concurrent export | narrow projection, page limit, no blob, bounded query, workload separation | memory grants, spills, buffer cache, process/SQL memory |
| Storage | audit/history/job/index growth, temp/checkpoint, attachments, backup, transaction log | metadata-only SQL; object storage; retention/archive; pre-size files; fixed controlled autogrowth | data/index/log/tempdb size and growth, backup size |
| DB I/O | write amplification, random GUID insert, key lookup storm, massive delete/rebuild | sequential internal cluster key, minimal covering index, batching, filtered hot index, archive | logical/physical reads, writes, IOPS, latency, page split |
| Network | oversized list/export, N+1, blob through metadata query | pagination, projection, compression where approved, streaming, async export, no file blob in SQL | DB-to-app bytes, round trips, upload/download throughput |
| Concurrency | lock/block/deadlock, duplicate job, pool exhaustion, Worker stampede | optimistic token, short claim/lease, RCSI candidate, bounded Worker/pool, backpressure | blocking time, deadlock, active connections, pool wait, queue depth |
| Growth | late audit sync, high job attempts, issue spike, secured versions | rows/day model, hot/cold tiers, max-three-version invariant, retention dashboard | rows/day, bytes/day, oldest hot row, forecast exhaustion date |
| Logging | application log in DB, transaction-log spike from cleanup/index work/progress tick | APM outside operational SQL; coalesced progress; batch purge; log backup/recovery plan | log generation MB/s, log used %, VLF/backup health, progress write rate |
| Cleanup | backlog, long delete locks, external storage orphan, cleanup competing with users | bounded resumable worker, state/expiry index, throttle, reconciliation | backlog age/count, batch duration, failure/retry, log/I/O impact |

### 11.2 Specific capacity risks

| Risk | Severity | Control |
|---|---|---|
| Audit ingestion + many nonclustered indexes | High | minimal evidence-based index set, sequential cluster key, batch sync, hot/archive lifecycle |
| Progress update storm | High | persist meaningful/coalesced progress, not every UI tick; short state transaction; measure log writes |
| Queue scan without filtered index | High | workload/available-time filtered index + atomic bounded claim |
| Long job holds DB connection/transaction | Critical | connection only around short state operations; lease, not open transaction |
| Deep offset or unbounded count/export | High | keyset cursor, page cap, async snapshot/export |
| Free-text/leading-wildcard search | High | identifier/prefix allowlist; explicit approved search architecture only |
| Massive retention delete | High | archive + small ordered batch/partition operation after evidence |
| Random wide clustered key | Medium/High | narrow insertion-friendly internal key + opaque public unique ID |
| Connection pools sized independently | High | total cross-deployable connection budget and admission control |
| RCSI enabled without `tempdb` capacity | High | DBA review and load test before lock |
| File/screenshot stored as SQL blob | High | secure object storage; SQL metadata/reference only |
| Secured version rotation DB/object inconsistency | High | transactional metadata invariant + recoverable external cleanup |
| Cleanup/index maintenance overlaps peak | Medium/High | workload-aware scheduling/throttle and monitoring |
| Parameter-sensitive audit plan | Medium | Query Store, representative cardinalities, controlled query shapes |
| Under-sized data/log/tempdb autogrowth | High | capacity forecast, pre-sizing, controlled fixed growth, alerting |

### 11.3 Capacity worksheet yang wajib sebelum lock angka

Untuk setiap environment, ukur:

```text
active users / peak users
API request rate by category
jobs/day by workload class
peak concurrent jobs
audit events/job and audit events/day
history rows/day
issues and attachments/day
secured files/versions/day
average and p95 row width
hot retention days
archive growth
index-to-data ratio
transaction-log generation
backup/restore window
SQL CPU/RAM/storage/IOPS/network budget
total connection budget
cleanup throughput versus daily growth
```

Exact angka menjadi typed configuration/capacity policy setelah PoC/load test. Keterbatasan server awal menghasilkan pagination, batching, quota, queue, cleanup, backpressure, dan async behavior—bukan penghapusan fitur.

---

## 12. Load-test database scenarios

### 12.1 Dataset dan method

- Gunakan synthetic/masked data; tidak memakai Production content tanpa masking dan approval.
- Uji empty/small, forecast hot volume, retained-volume boundary, dan approved safety-headroom volume.
- Distribusi data harus realistis: beberapa heavy user, skewed status, late offline audit, large date range, failed/retried job, serta issue/secured-file ownership.
- Warm-cache dan cold-cache test dibedakan.
- Capture execution plan, Query Store, waits, locks, deadlocks, memory grant/spill, I/O, log, tempdb, connection pool, and cleanup backlog.
- Jalankan steady-state, burst, soak, failure/recovery, dan maintenance-overlap test.

### 12.2 Scenario matrix

| No | Scenario | Workload | Verification |
|---:|---|---|---|
| 1 | History cursor | concurrent users membuka page 1 lalu continuation dengan feature/status filter | index seek/keyset, no deep offset, p95/p99 stable, max 50, ownership correct |
| 2 | Audit ingest + read | continuous online/offline batch insert bersamaan dengan user/feature/status/process/date queries | ingestion tidak starved; no duplicate event; late arrival correct; acceptable write amplification |
| 3 | Audit wide export | authorized multi-period export saat normal traffic | async, bounded read batches, no API timeout, no ingestion blocking, output retention applied |
| 4 | Job admission/idempotency storm | repeated identical and conflicting idempotency keys under concurrency | exactly one logical resource, conflict correct, no duplicate reservation/job |
| 5 | Multi-Worker dequeue | several Worker claim same workload queue | no duplicate execution, fairness, lease correctness, bounded lock waits |
| 6 | Worker crash/lease expiry | Worker berhenti setelah claim dan sebelum/after progress commit | safe reclaim, expected state/version, no terminal regression |
| 7 | Progress update pressure | many active jobs update progress and emit persisted notification | coalesced write rate, transaction log stable, Activity Center current state correct |
| 8 | Upload/session lifecycle | concurrent streamed/resumable sessions, offset replay, expiry, abort, cleanup | no DB connection held during stream, offset concurrency safe, orphan cleanup bounded |
| 9 | Issue spike | user submits issue plus optional attachment; support filters/triage concurrently | message survives attachment failure, cursor/filter index works, rowversion conflict detected |
| 10 | Secured-file/version rotation | list/download authorization plus concurrent fourth-version creation | max-three invariant, no lost current pointer, recoverable object cleanup, no cross-owner access |
| 11 | Retention cleanup under load | audit/history/job/idempotency/upload batches run during representative traffic | bounded log/locks/I/O, resumable checkpoint, backpressure reduces cleanup first |
| 12 | Archive boundary | hot-to-archive move, integrity manifest, authorized old-range retrieval | row/count/checksum reconciles, legal hold honored, live query unaffected |
| 13 | Connection-pool pressure | API + Worker + export + cleanup compete for connections | no pool exhaustion cascade, admission/backpressure activates, connections released promptly |
| 14 | Deadlock/transient fault | inject deadlock, SQL restart/network transient, timeout | bounded safe retry, no duplicate job/audit/file version, clear recovery state |
| 15 | Worst allowed filter/sort | each API allowlisted combination at skewed cardinality | stable plan or controlled alternate shape; no arbitrary full scan/sort spill |
| 16 | Capacity soak/growth | sustained forecast load sampai steady-state cache/log/cleanup behavior terlihat | cleanup throughput exceeds growth, no unbounded queue/log/tempdb/storage trend |

### 12.3 Metrics dan acceptance gate

Target numerik ditetapkan Test Plan setelah benchmark, tetapi Document 07 SHOULD mewajibkan gate berikut:

```text
[ ] Primary cursor query memakai supporting index dan tidak full-scan hot table
[ ] Authorization predicate diterapkan sebelum pagination/count
[ ] No duplicate logical job, audit event, upload commit, atau secured-file version
[ ] No long transaction/connection across processing or external I/O
[ ] Blocking/deadlock berada dalam approved budget dan recovery benar
[ ] Transaction log, tempdb, data, index, dan backup growth berada dalam forecast
[ ] Cleanup/archive throughput lebih besar dari sustained data growth
[ ] 429/503 backpressure aktif sebelum DB connection/CPU/IO exhaustion
[ ] Audit/history/issue/secured-file export tidak mengganggu operational workload
[ ] No document/password/token/OCR/screenshot content pada DB log/telemetry/query capture
[ ] Feature tetap tersedia melalui bounded/async behavior; tidak dihapus karena server kecil
```

---

## Open decisions for User + Integrator

| No | Keputusan terbuka | Rekomendasi Agent C |
|---:|---|---|
| 1 | Document 06 authoritative artifact | finalkan/cantumkan artifact 06 FINAL lalu audit ulang seluruh query/retention contract sebelum Document 07 lock |
| 2 | Logical/physical table names dan key types | gunakan module-owned schemas; narrow sequential internal cluster key + opaque public unique ID untuk high-write tables |
| 3 | Pertamina identity key mapping | simpan unique issuer/subject mapping dan gunakan narrow internal user FK pada large tables |
| 4 | Exact API filter/sort allowlist | susun route/query inventory bersama index catalog; jangan membuka kombinasi yang tidak didukung |
| 5 | Hot retention, archive retention, legal hold | tetapkan per data class dengan owner/compliance; tidak ada satu TTL global |
| 6 | Audit retention timestamp | putuskan `OccurredAt` versus `ReceivedAt` untuk late/offline event, termasuk maximum accepted lateness |
| 7 | Audit partitioning/compression/archive platform | tunda sampai volume, operational capability, restore path, dan benchmark membuktikan manfaat |
| 8 | Job progress persistence cadence | ukur trade-off realtime/recovery versus log/write pressure; jangan persist setiap UI tick |
| 9 | RCSI/isolation policy | rekomendasi evaluasi RCSI dengan DBA dan `tempdb` load test; lock hanya setelah evidence |
| 10 | Pool size, timeout, Worker DB concurrency | hitung sebagai satu total SQL Server connection/capacity budget per environment |
| 11 | Cleanup batch/concurrency/window | typed policy hasil load test; cleanup wajib resumable dan menurunkan load saat pressure |
| 12 | Recovery model, backup, restore, data/log/tempdb sizing | tetapkan bersama Deployment/DR Guide dan DBA; sertakan transaction-log growth test |
| 13 | Full-text/search expansion | tidak menjadi baseline; memerlukan requirement, permission/security design, dan capacity evidence baru |
| 14 | Numerical performance acceptance | ditetapkan pada Test Plan dari SLA dan capacity benchmark, bukan hardcode pada source |

Semua rekomendasi ini bersifat input untuk Integrator. Keputusan akhir tetap milik user + Integrator.
