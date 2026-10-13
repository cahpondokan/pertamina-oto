# AGENT A — SQL SERVER REPORT

**Task ID:** DOC07-AGENT-A  
**Role:** SQL Server specialist — server-side database  
**Status:** ANALYSIS / RECOMMENDATION ONLY  
**Tanggal:** 24 Agustus 2026

Dokumen ini merupakan input untuk Document 07. Dokumen ini tidak mengubah master Document 07, tidak menetapkan nilai policy/retention baru, tidak memilih provider/vendor, dan tidak berisi production code.

## 1. Baseline dan executive recommendation

### 1.1 Source baseline yang digunakan

- Documents 01–05 FINAL.
- Document 06 working v0.5 beserta 9/9 API decision records yang berstatus LOCKED.
- MASTER_PROJECT_SUMMARY v2.8 yang menetapkan 550 canonical features dan Document 06 masih ACTIVE.

Catatan: artefak Document 06 berstatus FINAL belum tersedia. Rekomendasi database di bawah memakai seluruh keputusan API yang sudah locked, terutama Hybrid API Surface, Hybrid File Transfer, persisted idempotency, REST-authoritative job state, server authorization, dan layered admission control.

### 1.2 Rekomendasi utama

1. Gunakan **satu SQL Server database baseline** dengan schema terpisah menurut module owner. Jangan menaruh seluruh tabel pada dbo dan jangan membuat schema common/shared untuk menampung entity yang tidak jelas owner-nya.
2. Satu table hanya mempunyai satu write owner. Modul lain menyimpan public logical identifier dan berinteraksi melalui application contract/internal event; modul lain tidak menulis table secara langsung.
3. Gunakan FK penuh untuk relationship di dalam ownership boundary. Cross-module FK tidak menjadi baseline; gunakan logical references seperti PrincipalId, FeatureId, JobId, SecuredFileId, IssueId, dan CorrelationId.
4. Gunakan internal clustered bigint identity key untuk table ber-volume tinggi, ditambah public opaque ID yang unique untuk resource API. Canonical FeatureId tetap natural key pada feature registry.
5. File, screenshot, checkpoint payload, OCR text, summary, translation result, dan temporary/result content tidak disimpan sebagai SQL blob secara default. SQL hanya menyimpan metadata, lifecycle, checksum, size, ownership, dan internal object-storage reference.
6. Job, upload, idempotency, admission reservation, outbox, secured-file version, issue, policy, dan mutable aggregate memakai optimistic concurrency dengan SQL Server rowversion.
7. Append-only table seperti job attempt/history, audit event, issue status history, dan outbox event menggunakan unique sequence/event keys, bukan update-in-place.
8. Setiap module mempunyai EF Core DbContext dan migration history sendiri. Production host tidak menjalankan automatic migration saat startup.
9. Retention harus eksplisit per table/lifecycle melalui ExpiresAtUtc, RetainUntilUtc, DeletedAtUtc, atau PurgeAfterUtc sesuai kebutuhan. Exact duration tetap berasal dari approved policy, bukan ditetapkan di laporan ini.
10. CorrelationId harus dapat dicari lintas module tetapi bukan PK/FK/authorization token. JobId adalah public identity job dan tetap sama ketika retry/resume menambah attempt.

## 2. Proposed schemas

Tidak direkomendasikan membuat module table di dbo. dbo dapat tetap digunakan hanya untuk objek platform yang benar-benar diperlukan oleh DBA/deployment dan telah disetujui; EF migration history ditempatkan pada schema owner masing-masing.

| Schema | Module owner | Tanggung jawab | Data yang dilarang secara default |
|---|---|---|---|
| account | AccountAdministration.Account | Principal internal, external identity mapping, session/revocation metadata | Password, bearer token, refresh token, identity-provider secret |
| admin | AccountAdministration.Authorization | Role, permission, role-permission, principal-role assignment | Business document, unrestricted claim dump |
| configuration | AccountAdministration.Configuration / Platform Configuration | Canonical Feature Registry deployment snapshot, aliases, platform exposure, feature flags, policy versions, compatibility policy | Engine secret, arbitrary unversioned settings bag |
| workspace | Workspace | Selectively synced lightweight preferences, favorites, presets, optional sync change feed | Draft, undo/redo, cache, thumbnail, clipboard, local path, unselected source |
| jobs | Processing / Jobs | Server job, attempt, step, state history, input refs, artifacts, checkpoint metadata, upload lifecycle, processing idempotency, admission reservation, reliable event outbox | File bytes, password, token, raw document/OCR content, arbitrary command |
| secured | Secured Files | Amankan ke Akun file metadata, versions, account recycle lifecycle | File content as SQL blob, local filesystem path |
| history | History | Personal server/synced history projection | Enterprise audit evidence, local path, document content |
| audit | Audit | Append-only enterprise audit metadata, offline ingestion batch, export request metadata | Document/clipboard/screenshot/password/token/signature/full OCR content |
| support | Support / Issue Reporting | Issue, optional attachment metadata, status history, notes, relations, reliable status outbox | Attachment bytes in SQL, automatically captured document content |
| assistant | Assistant | Minimal server-side invocation/workflow/internal-AI metadata | Full prompt/chat, document content, password, token, clipboard, signature asset |
| intelligence | Content Intelligence | Server-side content-intelligence operation/stage metadata and policy/consent evidence reference | Prompt, OCR text, summary text, translation text, document bytes |

Schema configuration dapat dimiliki oleh submodule AccountAdministration/Platform Configuration tanpa membuat deployable/service baru. Schema jobs dan intelligence juga merupakan logical ownership boundaries di dalam modular monolith, bukan alasan menambah microservice.

## 3. Entity/table list dan ownership

### 3.1 account — owner: AccountAdministration.Account

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| account.Principals | Internal principal dan mapping ke approved Pertamina Identity | PrincipalPk bigint identity | Public PrincipalId unique; Issuer + ExternalSubject unique; minimal claim profile; RowVersion |
| account.Sessions | Server session/revocation metadata tanpa token | SessionPk bigint identity | SessionIdHash unique; FK PrincipalPk; expiry/revocation timestamps; RowVersion |

Recommended fields:

- Principals: PrincipalPk, PrincipalId, Issuer, ExternalSubject, DisplayName bila diizinkan, UnitCode bila claim contract menyediakannya, IsActive, CreatedAtUtc, UpdatedAtUtc, RowVersion.
- Sessions: SessionPk, SessionIdHash, PrincipalPk, IssuedAtUtc, ExpiresAtUtc, LastSeenAtUtc, RevokedAtUtc, RevocationReasonCode, CreatedAtUtc, RowVersion.

Raw token, password, refresh token, dan unrestricted claim JSON tidak disimpan.

### 3.2 admin — owner: AccountAdministration.Authorization

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| admin.Roles | Role sebagai pengelompokan permission | RolePk bigint identity | RoleCode unique; active flag; RowVersion |
| admin.Permissions | Canonical permission definitions | PermissionPk bigint identity | PermissionCode unique; seeded/versioned |
| admin.RolePermissions | Many-to-many role dan permission | RolePermissionPk bigint identity atau composite | Unique RolePk + PermissionPk; same-owner FKs |
| admin.PrincipalRoles | Assignment principal ke role | PrincipalRolePk bigint identity | Unique PrincipalPk + RolePk; assigned/revoked timestamps; RowVersion |

Role tidak menjadi final authorization decision. API tetap melakukan permission, ownership, policy, dan application invariant checks. Exact permission catalog menunggu Role Matrix dan Security Design.

### 3.3 configuration — owner: Platform Configuration

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| configuration.FeatureDefinitions | Deployed snapshot 550 canonical feature contracts/metadata | FeatureId varchar key | Canonical immutable ID; contract fields tidak dapat diubah bebas oleh admin |
| configuration.FeatureAliases | Alias/discovery mapping ke canonical FeatureId | FeatureAliasPk bigint identity | FK within schema; unique normalized alias + locale |
| configuration.FeaturePlatformExposures | Desktop/Tablet/Phone/Web exposure | Composite FeatureId + PlatformCode | Exposure code check/reference; RowVersion bila mutable |
| configuration.FeatureFlags | Staged rollout/emergency disable metadata | FeatureFlagPk bigint identity | FlagKey unique per scope; optional FeatureId FK; lifecycle timestamps; RowVersion |
| configuration.PolicySets | Named scoped policy aggregate | PolicySetPk bigint identity | Public PolicySetId unique; ScopeType + ScopeId; status; RowVersion |
| configuration.PolicySetVersions | Immutable typed policy document versions | PolicySetVersionPk bigint identity | FK PolicySetPk; unique PolicySetPk + VersionNo; SchemaVersion; ContentHash |
| configuration.CompatibilityPolicies | Current/minimum client and API-major compatibility | CompatibilityPolicyPk bigint identity | Unique PlatformCode + ChannelCode + ApiMajor; RowVersion |

FeatureDefinitions minimum mengikuti Feature Catalog:

~~~text
FeatureId
RecordType
UiExposure
InputOutputContract
ExecutionProfile
ProcessingMode
ServerDependency
OfflineSupport
StorageImpact
NetworkImpact
ConcurrencyRisk
SecurityRisk
Destructive
Priority
Release
RequirementRefs
DependencyFamily
Status
ParameterSchemaVersion
BuildPriority
DesktopExposure
TabletExposure
PhoneExposure
ContractVersion
~~~

Aliases sebaiknya dinormalisasi pada child table, bukan disimpan sebagai comma-separated string. RequirementRefs yang perlu query dapat menggunakan child table tambahan configuration.FeatureRequirementRefs; jika hanya artifact/reference, dapat tetap sebagai versioned JSON kecil dengan ISJSON check. Pilihan final bergantung query requirement Document 07.

Stable feature contract berasal dari versioned code/catalog artifact. Database menyimpan deployed snapshot dan mutable enterprise override; database tidak boleh mengubah Merge PDF menjadi output Excel atau mengganti semantics contract.

### 3.4 workspace — owner: Workspace

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| workspace.UserPreferences | Lightweight selectively synced preference | UserPreferencePk bigint identity | OwnerPrincipalId unique; PreferenceSchemaVersion; small JSON/typed columns; RowVersion |
| workspace.FavoriteFeatures | Favorite canonical tools | Composite OwnerPrincipalId + FeatureId | Logical refs; no cross-module FK |
| workspace.Presets | Versioned feature/workflow preset metadata | PresetPk bigint identity | Public PresetId unique; owner + name uniqueness among active rows; schema versions; RowVersion |
| workspace.SyncChanges | Optional durable lightweight sync feed | SyncChangePk bigint identity | Unique OwnerPrincipalId + ChangeSequence; tombstone support; retention-sensitive |

SyncChanges hanya dibuat jika selective sync API memerlukan durable change feed. Table ini tidak boleh menjadi full workspace event store.

Preset definition boleh memakai versioned JSON karena schema ditentukan FeatureId/WorkflowSchemaVersion, tetapi:

- tidak menyimpan password atau secret;
- tidak menyimpan file content/local path;
- mempunyai ContentHash dan ISJSON check;
- menyimpan ParameterSchemaVersion atau WorkflowSchemaVersion;
- divalidasi ulang sebelum execution;
- migration/compatibility path mengikuti API contract.

### 3.5 jobs — owner: Processing / Jobs

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| jobs.JobStatuses | Seeded API-visible job states | StatusCode varchar key | Hanya locked states; migration-owned |
| jobs.UploadStatuses | Seeded upload lifecycle states | StatusCode varchar key | Hanya locked states; migration-owned |
| jobs.Jobs | Current persisted server job state | JobPk bigint identity | Public JobId unique; CorrelationId indexed; RowVersion; worker lease |
| jobs.JobAttempts | Append-only physical execution attempts untuk logical JobId | JobAttemptPk bigint identity | FK JobPk; unique JobPk + AttemptNo |
| jobs.JobSteps | Ordered canonical workflow/job steps | JobStepPk bigint identity | FK JobPk; unique JobPk + StepKey dan JobPk + StepOrder; RowVersion |
| jobs.JobStateHistory | Append-only state transition ledger | JobStateHistoryPk bigint identity | FK JobPk; unique JobPk + Sequence; immutable |
| jobs.JobInputs | Typed fileRef/input binding metadata | JobInputPk bigint identity | FK JobPk; unique JobPk + InputId; Kind + ExternalId; no path |
| jobs.JobArtifacts | Temporary/intermediate/result artifact metadata | JobArtifactPk bigint identity | Public ArtifactId unique; FK JobPk; object storage reference; expiry |
| jobs.JobCheckpoints | Verified checkpoint metadata | JobCheckpointPk bigint identity | FK JobPk/Attempt; unique JobPk + CheckpointSequence; optional artifact FK |
| jobs.UploadSessions | Resumable/one-shot upload lifecycle | UploadSessionPk bigint identity | Public UploadId unique; OwnerPrincipalId; committed offset; expiry; RowVersion |
| jobs.UploadParts | Idempotent received range/digest metadata | UploadPartPk bigint identity | FK UploadSessionPk; unique session + range; range/checksum checks |
| jobs.IdempotencyRecords | Persisted Idempotency-Key untuk job/upload processing commands | IdempotencyRecordPk bigint identity | Composite unique ScopeHash + OperationHash + KeyHash; expiry; RowVersion |
| jobs.AdmissionReservations | Persisted capacity/quota reservation before 202 | AdmissionReservationPk bigint identity | Public ReservationId unique; optional JobPk FK; expiry/release; RowVersion |
| jobs.OutboxMessages | Reliable post-commit event notification | OutboxMessagePk bigint identity | EventId unique; StreamId + Sequence unique; safe payload; retry metadata |

#### jobs.Jobs recommended columns

~~~text
JobPk
JobId
OwnerPrincipalId
FeatureId
WorkflowSchemaVersion nullable
ParameterSchemaVersion nullable
ContractVersion
CorrelationId
StatusCode
WorkloadClass
QueueName
PriorityCode nullable
ProgressPercent nullable
CurrentStepCode nullable
CompletedCount nullable
RemainingCount nullable
ErrorCode nullable
CreatedAtUtc
QueuedAtUtc
StartedAtUtc nullable
CompletedAtUtc nullable
AvailableAtUtc
ExpiresAtUtc nullable
LeaseOwner nullable
LeaseExpiresAtUtc nullable
RowVersion
~~~

FeatureId, OwnerPrincipalId, dan CorrelationId adalah logical values, bukan cross-module FK. JobId tetap sama saat retry/resume; attempt history bertambah pada JobAttempts.

#### Locked job status seed

~~~text
QUEUED
RUNNING
WAITING_CONNECTION
WAITING_RESOURCE
RETRYING
COMPLETED
FAILED
CANCELLED
~~~

PAUSED tidak boleh ditambahkan tanpa controlled scope revision.

#### Locked upload status seed

~~~text
CREATED
UPLOADING
VALIDATING
READY
FAILED
ABORTED
EXPIRED
~~~

#### Idempotency record

Jangan menyimpan raw Idempotency-Key. Simpan hash key dan request fingerprint dengan algorithm/version metadata. Safe response snapshot harus dibatasi ukurannya dan tidak boleh membawa token, path, document content, password, raw error, atau secret.

Recommended unique boundary:

~~~text
ScopeHash
+ OperationHash
+ KeyHash
= one idempotency record
~~~

ScopeHash mewakili authenticated principal/tenant scope yang disetujui. OperationHash mewakili method + canonical route + contract version. RequestFingerprint disimpan terpisah untuk mendeteksi key reuse dengan payload berbeda.

#### Reliable event publication

State current dan JobStateHistory disimpan dalam transaksi module yang sama dengan OutboxMessages. SignalR publisher membaca outbox setelah commit. Event sequence monoton per JobId/stream, bukan global. Outbox payload hanya safe notification metadata.

### 3.6 secured — owner: Secured Files

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| secured.Files | Logical account-secured file aggregate | SecuredFilePk bigint identity | Public SecuredFileId unique; owner logical ID; current state; RowVersion |
| secured.FileVersions | Immutable secured content version metadata | FileVersionPk bigint identity | FK SecuredFilePk; unique file + VersionNo; storage object reference unique |
| secured.RecycleEntries | Account recycle/retention lifecycle | RecycleEntryPk bigint identity | FK SecuredFilePk; one active entry per file; purge eligibility; RowVersion |
| secured.IdempotencyRecords | Persisted idempotency untuk Amankan ke Akun/version commands | IdempotencyRecordPk bigint identity | Unique scoped key hash; request fingerprint; safe replay; RowVersion |
| secured.OutboxMessages | Durable secured-file lifecycle/storage cleanup events | OutboxMessagePk bigint identity | EventId unique; stream sequence unique; safe payload |

Recommended FileVersions metadata:

- VersionNo;
- MediaType;
- Extension;
- SizeBytes;
- ChecksumAlgorithm dan Checksum;
- ETag;
- internal StorageObjectKey;
- SourceJobId dan CorrelationId sebagai logical trace;
- CreatedByPrincipalId;
- CreatedAtUtc;
- RetainUntilUtc/PurgeAfterUtc bila policy membutuhkan.

Maximum tiga account-file versions adalah application invariant di dalam transaction Secured Files. Unique FilePk + VersionNo mencegah duplicate version number, tetapi count maksimal tiga tidak dapat dijamin dengan simple CHECK constraint. Rotation/purge harus dikendalikan aggregate service dan diuji concurrency.

File content berada di secure file/object storage. StorageObjectKey bersifat internal, tidak pernah menjadi API authority, dan tidak boleh ditulis log biasa.

### 3.7 history — owner: History

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| history.Entries | Personal server/synced history projection | HistoryEntryPk bigint identity | Public HistoryEntryId unique; owner + SourceEventId unique untuk idempotent projection |

Recommended fields:

- OwnerPrincipalId;
- FeatureId;
- JobId nullable;
- CorrelationId;
- safe display filename bila diizinkan;
- status;
- result reference kind/id, bukan local path;
- size dan duration;
- execution location LOCAL/SERVER bila disinkronkan;
- OccurredAtUtc, CreatedAtUtc;
- DeletedAtUtc/PurgeAfterUtc untuk personal deletion workflow.

History bukan audit. User deletion terhadap personal history tidak menghapus audit event. Local-only history tetap di SQLite kecuali selective sync requirement secara eksplisit mengizinkan metadata tertentu.

### 3.8 audit — owner: Audit

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| audit.IngestionBatches | Idempotent offline/batch audit ingestion | IngestionBatchPk bigint identity | Public BatchId unique per source/principal; counts/hash; received timestamp |
| audit.AuditEvents | Append-only enterprise audit metadata | AuditEventPk bigint identity | Public/source AuditEventId unique; immutable; high-value indexes |
| audit.ExportRequests | Audit export request/projection metadata | AuditExportRequestPk bigint identity | Public request ID unique; JobId logical; filter snapshot safe/versioned |

AuditEvents minimum:

~~~text
AuditEventPk
AuditEventId
IngestionBatchPk nullable
OccurredAtUtc
ReceivedAtUtc
ActorPrincipalId
RoleSnapshotCode nullable
UnitCode nullable
EventTypeCode
FeatureId nullable
ProcessId nullable
JobId nullable
CorrelationId
ExecutionLocation nullable
OutcomeStatus
FileSizeBytes nullable
ErrorCode nullable
PlatformCode nullable
AppVersion nullable
SafeMetadataJson nullable
~~~

Audit event tidak mempunyai cascade FK ke account, jobs, secured, issue, atau feature tables. Audit harus tetap ada ketika source resource mengikuti retention/personal deletion. SafeMetadataJson wajib size-limited, versioned, ISJSON-checked, allowlisted, dan tidak menerima arbitrary object dump.

Required query indexes berasal dari requirement:

- ActorPrincipalId + OccurredAtUtc + AuditEventPk;
- FeatureId + OccurredAtUtc + AuditEventPk;
- OutcomeStatus + OccurredAtUtc + AuditEventPk;
- ProcessId;
- JobId;
- CorrelationId;
- ReceivedAtUtc untuk operational ingestion.

Partitioning/compression belum diputuskan sebelum SQL Server edition, volume, retention, dan performance evidence tersedia.

### 3.9 support — owner: Support / Issue Reporting

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| support.IssueStatuses | Seeded internal/user status mapping | StatusCode varchar key | Stable code; UserStatusCode mapping; migration-owned |
| support.IssuePriorities | Support-managed priority reference | PriorityCode varchar key | Exact catalog owner-approved later |
| support.IssueCategories | Support-managed category reference | CategoryCode varchar key | Exact catalog owner-approved later |
| support.Issues | Issue aggregate dan safe diagnostic context | IssuePk bigint identity | Public IssueId unique; reporter logical ID; RowVersion |
| support.IssueAttachments | Screenshot/attachment metadata | IssueAttachmentPk bigint identity | Public AttachmentId unique; FK IssuePk; object storage ref |
| support.IssueStatusHistory | Append-only issue transitions | IssueStatusHistoryPk bigint identity | FK IssuePk; unique IssuePk + Sequence |
| support.IssueNotes | Permission-gated internal notes | IssueNotePk bigint identity | FK IssuePk; author logical ID; retention-sensitive |
| support.IssueRelations | Duplicate/related issue link | IssueRelationPk bigint identity | Same-schema FKs; unique source + target + relation type |
| support.IdempotencyRecords | Persisted idempotency untuk issue submission/mutating support commands | IdempotencyRecordPk bigint identity | Unique scoped key hash; request fingerprint; safe replay; RowVersion |
| support.OutboxMessages | Reliable issue status notification | OutboxMessagePk bigint identity | EventId unique; stream sequence unique; safe payload |

Locked internal issue statuses:

~~~text
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
~~~

User-facing mapping tetap:

~~~text
Dikirim
Sedang diperiksa
Sedang ditindaklanjuti
Selesai
~~~

Issues recommended fields:

- ReporterPrincipalId;
- Message;
- StatusCode;
- CategoryCode dan PriorityCode nullable sampai triage;
- ActiveFeatureId/PageCode;
- AppVersion, PlatformCode, OsVersion;
- ErrorCode, JobId, CorrelationId;
- SafeDiagnosticsJson;
- CreatedAtUtc, UpdatedAtUtc, ClosedAtUtc;
- RowVersion.

Message dan internal note memang merupakan business content yang diperlukan Support, tetapi perlu access control, retention, encryption-at-rest policy, safe logging, dan no full-text indexing secara default. Screenshot bytes disimpan di secure object storage, bukan SQL. Attachment table hanya menyimpan ownership, media type, size, checksum, object key, created time, dan retention state.

### 3.10 assistant — owner: Assistant

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| assistant.InvocationMetadata | Minimal server-side Assistant invocation metadata | AssistantInvocationPk bigint identity | Public InvocationId unique; CorrelationId indexed; no prompt |
| assistant.WorkflowMetadata | Validated plan/execution summary | AssistantWorkflowPk bigint identity | Public WorkflowExecutionId unique; JobId logical; plan fingerprint |
| assistant.InternalAiCallMetadata | Approved internal-AI fallback telemetry/decision metadata | InternalAiCallPk bigint identity | Invocation FK; provider profile code; no request/response content |

Recommended metadata:

- OwnerPrincipalId;
- intent category, bukan raw utterance;
- resolved canonical FeatureId;
- WorkflowSchemaVersion/ParameterSchemaVersion;
- plan fingerprint dan step count, bukan unrestricted plan content;
- status/outcome/error code;
- JobId dan CorrelationId;
- local deterministic vs internal AI routing code;
- policy/consent decision reference bila relevant;
- duration/resource counters yang aman;
- CreatedAtUtc/CompletedAtUtc/ExpiresAtUtc.

Tidak direkomendasikan membuat assistant.Conversations atau assistant.Messages pada baseline server. Conversation state adalah local/minimal by default. Penambahan server chat history membutuhkan controlled privacy/security/business scope revision.

### 3.11 intelligence — owner: Content Intelligence

| Table | Purpose | Primary key | Important rules |
|---|---|---|---|
| intelligence.Operations | AI Summarizer/Translate/PDF-to-Markdown operation metadata | IntelligenceOperationPk bigint identity | Public OperationId unique; JobId/CorrelationId indexed; RowVersion |
| intelligence.OperationStages | Append-only preprocessing/provider/output stages | OperationStagePk bigint identity | FK operation; unique operation + StageSequence |

Recommended Operations metadata:

- FeatureId;
- OwnerPrincipalId;
- JobId;
- CorrelationId;
- execution location LOCAL/SERVER;
- provider profile/capability code after approval, never credential;
- source/target language codes where relevant;
- ParameterSchemaVersion dan ContractVersion;
- status/error code;
- policy/permission/data-minimization/consent outcome code;
- input/output byte counts and page counts if safe;
- result artifact logical ID;
- CreatedAtUtc/StartedAtUtc/CompletedAtUtc/ExpiresAtUtc;
- RowVersion.

Prompt, extracted text, OCR text, summary, translation, Markdown content, source document, and provider raw response tidak disimpan di these tables. Result content mengikuti jobs artifact atau secured file lifecycle dalam secure object storage.

## 4. Relationships dan ownership boundaries

### 4.1 FK yang direkomendasikan

Gunakan database FK untuk relationship dalam satu write-owner boundary:

- account.Sessions → account.Principals;
- admin.RolePermissions → admin.Roles dan admin.Permissions;
- admin.PrincipalRoles → account.Principals dan admin.Roles karena account/admin dimiliki satu AccountAdministration module;
- configuration child tables → configuration.FeatureDefinitions/PolicySets;
- jobs attempts/steps/history/inputs/artifacts/checkpoints/upload parts/reservations → parent table dalam jobs;
- secured.FileVersions/RecycleEntries → secured.Files;
- audit.AuditEvents → audit.IngestionBatches, nullable;
- support attachment/history/note/relation → support.Issues;
- assistant workflow/internal AI metadata → assistant invocation bila aggregate membutuhkannya;
- intelligence.OperationStages → intelligence.Operations.

Delete behavior default adalah RESTRICT/NO ACTION. Cascade delete hanya dipertimbangkan untuk short-lived purely dependent rows yang telah dibuktikan tidak menghapus evidence atau external storage secara diam-diam. Material cleanup memakai explicit lifecycle process.

### 4.2 Cross-module logical relationships

| Logical identifier | Authoritative owner | Referencing schemas | Database FK? |
|---|---|---|---|
| PrincipalId | account | jobs, secured, history, audit, support, assistant, intelligence, workspace | Tidak |
| FeatureId | configuration/code contract | jobs, history, audit, support, assistant, intelligence, workspace | Tidak |
| JobId | jobs | secured, history, audit, support, assistant, intelligence | Tidak |
| CorrelationId | Request/application tracing | Semua relevant schemas | Tidak; indexed trace field |
| SecuredFileId | secured | jobs/history/workspace bila diperlukan | Tidak |
| IssueId | support | audit/jobs export metadata bila diperlukan | Tidak |
| ArtifactId/fileRef id | jobs atau secured sesuai kind | Other modules through application contract | Tidak |

Alasan tidak membuat cross-module FK:

- independent retention dan purge;
- mencegah module lain bergantung pada internal PK;
- menjaga audit saat source resource dihapus;
- menghindari migration coupling;
- mendukung future physical separation tanpa mengubah domain contract.

Konsekuensinya adalah referential integrity lintas module harus dijaga melalui application validation, idempotent event consumption, reconciliation/monitoring, dan immutable public IDs.

### 4.3 Tidak ada shared-table chaos

- Tidak ada global Entity, Status, Metadata, Attachment, Setting, Event, atau Outbox table yang ditulis semua module.
- Status/reference data dimiliki schema yang memakai state machine tersebut.
- Idempotency dan outbox menggunakan shared implementation pattern/library tetapi physical record table tetap dimiliki masing-masing resource module. Ini memungkinkan resource + idempotency/outbox commit dalam transaction owner yang sama.
- Setiap module dapat memakai reusable EF mapping/library, tetapi physical table tetap module-owned.
- Cross-module reporting tidak menulis source tables; gunakan API/application query, event-built projection, atau approved reporting export.
- Tidak ada direct SQL/view shortcut yang menjadikan module lain co-owner.

## 5. PK, FK, unique, status, timestamp, dan indexing rules

### 5.1 Primary key strategy

1. Table ber-volume tinggi memakai bigint IDENTITY sebagai internal clustered PK.
2. API resource memakai separate public opaque ID, misalnya JobId, UploadId, IssueId, SecuredFileId, PresetId, dan OperationId, dengan unique nonclustered index.
3. Canonical FeatureId dan stable status/reference codes dapat menjadi varchar natural PK karena kecil, immutable, dan memang contract identity.
4. Jangan mengekspos identity PK ke client.
5. Public ID tidak memuat user ID, tenant, engine, storage key, atau sequential database meaning.

Exact public-ID generator/encoding berada di implementation task, tetapi storage type dan maximum length harus konsisten dengan API contract. ASCII identifiers menggunakan bounded varchar; localized text menggunakan bounded nvarchar.

### 5.2 Foreign key strategy

- Required child relationship di dalam module memakai NOT NULL FK.
- Optional same-module relation memakai nullable FK.
- FK columns selalu di-index bila digunakan untuk join/delete validation.
- Cross-module references menggunakan bounded scalar public ID tanpa FK.
- Jangan membuat FK dari audit ke mutable source table.
- Jangan memakai cascade delete pada audit/history evidence, secured versions, issue history, attempts, or state history.

### 5.3 Unique constraints

Minimum uniqueness:

~~~text
account.Principals                 Issuer + ExternalSubject
account.Principals                 PrincipalId
account.Sessions                   SessionIdHash
admin.Roles                        RoleCode
admin.Permissions                  PermissionCode
admin.RolePermissions              RolePk + PermissionPk
admin.PrincipalRoles               PrincipalPk + RolePk
configuration.FeatureDefinitions   FeatureId
configuration.FeatureAliases       LocaleCode + NormalizedAlias
configuration.FeaturePlatformExposures FeatureId + PlatformCode
workspace.FavoriteFeatures         OwnerPrincipalId + FeatureId
workspace.Presets                  PresetId
jobs.Jobs                          JobId
jobs.JobAttempts                   JobPk + AttemptNo
jobs.JobSteps                      JobPk + StepKey
jobs.JobSteps                      JobPk + StepOrder
jobs.JobStateHistory               JobPk + Sequence
jobs.UploadSessions                UploadId
jobs.UploadParts                   UploadSessionPk + RangeStart + RangeEnd
jobs.IdempotencyRecords            ScopeHash + OperationHash + KeyHash
jobs.OutboxMessages                EventId
jobs.OutboxMessages                StreamId + Sequence
secured.Files                      SecuredFileId
secured.FileVersions               SecuredFilePk + VersionNo
secured.IdempotencyRecords         ScopeHash + OperationHash + KeyHash
secured.OutboxMessages             EventId
history.Entries                    HistoryEntryId
history.Entries                    OwnerPrincipalId + SourceEventId
audit.AuditEvents                  AuditEventId
audit.IngestionBatches             BatchId
support.Issues                     IssueId
support.IssueAttachments           AttachmentId
support.IssueStatusHistory         IssuePk + Sequence
support.IdempotencyRecords         ScopeHash + OperationHash + KeyHash
support.OutboxMessages             EventId
assistant.InvocationMetadata       InvocationId
intelligence.Operations            OperationId
~~~

Filtered unique indexes dapat digunakan untuk one-active-row invariant, misalnya one active recycle entry atau one current policy version, setelah exact lifecycle ditetapkan.

### 5.4 Status/reference data

Gunakan stable uppercase code, bukan localized display string dan bukan magic integer. Reference data yang API-visible/reportable di-seed melalui owned migrations dan tidak dapat ditambah dari admin UI tanpa corresponding contract/code change.

Gunakan reference table untuk:

- job status;
- upload status;
- issue status/user-status mapping;
- permission and role definitions;
- feature registry codes yang perlu query/governance.

Gunakan CHECK constraint untuk technical enum yang kecil dan hanya internal apabila tidak memerlukan metadata/reporting. Jangan membuat satu global Status table.

### 5.5 Timestamp standard

- Semua persisted business/technical time memakai UTC.
- Type baseline: datetime2(7), dengan suffix AtUtc.
- CreatedAtUtc/ReceivedAtUtc menggunakan server-side SYSUTCDATETIME default bila appropriate.
- Client-provided OccurredAtUtc disimpan terpisah dari server ReceivedAtUtc dan diperlakukan untrusted untuk ordering/security.
- UpdatedAtUtc diperbarui bersama successful aggregate write.
- StartedAtUtc, CompletedAtUtc, RevokedAtUtc, DeletedAtUtc, ExpiresAtUtc, RetainUntilUtc, dan PurgeAfterUtc nullable sesuai lifecycle.
- SQL Server rowversion bukan timestamp bisnis; jangan menamainya UpdatedAt.
- SignalR ordering memakai persisted StreamId + Sequence, bukan timestamp.

### 5.6 Index baseline

Index harus mengikuti query/API contract, bukan dibuat pada setiap column.

Recommended patterns:

- keyset cursor: scope/owner + stable sort timestamp DESC + unique PK DESC;
- jobs: OwnerPrincipalId + CreatedAtUtc + JobPk; StatusCode + AvailableAtUtc + JobPk; LeaseExpiresAtUtc; JobId; CorrelationId;
- uploads: OwnerPrincipalId + CreatedAtUtc; StatusCode + ExpiresAtUtc; UploadId;
- secured: OwnerPrincipalId + UpdatedAtUtc + SecuredFilePk; recycle purge timestamp;
- history: OwnerPrincipalId + OccurredAtUtc + HistoryEntryPk;
- audit: filters yang disebut pada section audit;
- support: StatusCode + UpdatedAtUtc + IssuePk; ReporterPrincipalId + CreatedAtUtc + IssuePk; JobId; CorrelationId;
- outbox: publication status + AvailableAtUtc + OutboxMessagePk;
- retention cleanup: ExpiresAtUtc/PurgeAfterUtc + PK.

Search/filter/sort fields hanya yang di-allowlist API. Jangan menyediakan arbitrary dynamic column/filter atau index berdasarkan client input. SQL query tidak memakai NOLOCK sebagai jalan pintas.

## 6. Concurrency recommendation

### 6.1 Database isolation

Rekomendasi baseline adalah mengaktifkan READ_COMMITTED_SNAPSHOT setelah QA/performance validation agar metadata read tidak memblokir normal write secara berlebihan. Keputusan database-level ini perlu dikonfirmasi pada Document 07/Deployment Guide berdasarkan SQL Server environment.

Gunakan explicit stronger isolation hanya pada narrow invariant:

- create/replay idempotency record;
- reserve/release constrained admission capacity;
- rotate secured file versions;
- allocate per-stream/per-job sequence bila tidak memakai atomic database mechanism.

Jangan menjadikan SERIALIZABLE sebagai default seluruh aplikasi.

### 6.2 Optimistic concurrency

Gunakan rowversion pada mutable aggregate roots:

- account.Principals/Sessions;
- admin role/assignment yang mutable;
- configuration FeatureFlags, PolicySets, CompatibilityPolicies;
- workspace UserPreferences/Presets;
- jobs Jobs, JobSteps, UploadSessions, IdempotencyRecords, AdmissionReservations;
- secured Files/RecycleEntries/IdempotencyRecords;
- support Issues/IdempotencyRecords;
- intelligence Operations.

EF Core memetakan rowversion sebagai concurrency token. Conflict harus menghasilkan controlled application result/409 bila relevant, bukan silent last-write-wins.

### 6.3 Append-only concurrency

Table berikut tidak perlu rowversion jika immutable:

- JobAttempts;
- JobStateHistory;
- JobCheckpoints setelah verified;
- UploadParts;
- PolicySetVersions setelah published;
- FileVersions;
- AuditEvents;
- IssueStatusHistory;
- OperationStages;
- OutboxMessages.

Gunakan unique sequence/event key dan handle duplicate insert idempotently.

### 6.4 Job claiming dan worker lease

Jobs current row menyimpan LeaseOwner dan LeaseExpiresAtUtc. Worker claim harus atomic dan memverifikasi StatusCode, AvailableAtUtc, lease state, dan RowVersion. Worker heartbeat memperpanjang lease melalui concurrency check. Expired lease dapat direcover sesuai retry policy tanpa membuat JobId baru.

Exact SQL locking hints/queue-claim statement ditentukan pada implementation/performance task. Laporan ini tidak merekomendasikan polling query yang membaca seluruh queue atau memakai unbounded in-memory queue.

### 6.5 Upload concurrency

- CommittedOffset dan RowVersion menjadi compare-and-swap boundary.
- UploadParts unique range + digest menjamin repeated identical chunk idempotent.
- Overlap dengan digest berbeda ditolak.
- Finalization hanya berhasil jika expected length/range/digest lengkap dan state valid.
- Hanya READY upload dapat dipakai sebagai JobInput.

### 6.6 Idempotency concurrency

- Coba insert scoped key record lebih dahulu.
- Unique constraint memilih satu winner.
- Duplicate membaca existing fingerprint/status/resource.
- Fingerprint sama melakukan replay; fingerprint berbeda menghasilkan conflict.
- Resource/job dan idempotency record dibuat konsisten sebelum 201/202 dikembalikan.
- Raw key tidak disimpan dan tidak dilog.
- Physical idempotency record berada pada resource owner schema: jobs untuk job/upload, secured untuk Amankan ke Akun/version, dan support untuk issue commands. Tidak ada cross-module writer ke satu global idempotency table.

### 6.7 Secured file version concurrency

Gunakan aggregate rowversion pada secured.Files dan transaction pendek:

1. verifikasi expected RowVersion;
2. tentukan next VersionNo;
3. insert immutable FileVersion;
4. rotate version tertua sesuai locked maximum-three rule;
5. update current-version metadata;
6. commit;
7. object-storage cleanup dilakukan melalui durable lifecycle/outbox, bukan cascade SQL.

### 6.8 Issue/configuration concurrency

Issue triage dan policy/config edit menggunakan RowVersion/If-Match equivalent. Conflict memberi user/admin data terbaru; jangan silent overwrite status/note/policy.

## 7. EF Core boundaries

### 7.1 DbContext per ownership boundary

| DbContext | Default schema | Allowed DbSets |
|---|---|---|
| AccountAdministrationDbContext | account, admin | account dan authorization tables yang dimiliki satu AccountAdministration module |
| ConfigurationDbContext | configuration | feature/configuration tables only |
| WorkspaceDbContext | workspace | workspace tables only |
| JobsDbContext | jobs | jobs/upload/idempotency/admission/outbox tables only |
| SecuredFilesDbContext | secured | secured file, version, recycle, idempotency, dan outbox tables only |
| HistoryDbContext | history | personal history projection only |
| AuditDbContext | audit | audit ingestion/event/export metadata only |
| SupportDbContext | support | issue, reference, attachment, idempotency, dan outbox tables only |
| AssistantDbContext | assistant | Assistant metadata only |
| ContentIntelligenceDbContext | intelligence | Content Intelligence metadata only |

Satu giant DbContext seluruh database tidak direkomendasikan. Satu module tidak menambahkan DbSet/navigation ke table module lain.

### 7.2 Mapping rules

- ToTable selalu menentukan table + owned schema.
- Bounded string length dan Unicode/ASCII choice ditetapkan eksplisit.
- Public typed IDs memakai value object + converter/comparer yang konsisten.
- RowVersion memakai concurrency token mapping.
- DeleteBehavior.Restrict/NoAction menjadi default.
- Required/optional relationship dinyatakan eksplisit.
- Unique/check/index constraints dibuat melalui migrations, tidak hanya application validation.
- JSON metadata memakai nvarchar(max) hanya jika benar-benar versioned/small, dengan ISJSON check dan size guard pada application contract.
- Decimal tidak digunakan untuk byte/count; gunakan bigint/int sesuai range.
- UTC timestamp semantics diuji pada serialization dan database mapping.
- Lazy loading proxy tidak digunakan untuk menembus module boundary.
- Query projection/read model tidak mengembalikan storage key, identity subject, token, secret, atau unrestricted JSON.

### 7.3 Cross-module interaction

- Cross-module command melalui application interface.
- Cross-module notification melalui internal event/outbox.
- Reference disimpan sebagai public typed ID scalar.
- Consumer memvalidasi ownership/permission lewat owner service saat current truth dibutuhkan.
- Tidak ada repository yang mengakses dua DbContext untuk menyembunyikan distributed ownership.
- Jika satu use case memerlukan consistency lintas module, gunakan explicit application orchestration, outbox/idempotent consumer, dan compensating lifecycle; jangan membuat cross-module write transaction sebagai default.

### 7.4 Database permissions

Rekomendasi:

- migration identity mempunyai DDL permission terkontrol;
- runtime identity tidak mempunyai schema-alter permission;
- runtime DML dibatasi ke schema yang diperlukan host/module sejauh topology memungkinkan;
- Worker memakai scoped service identity dan tidak menerima end-user token;
- read access ke audit/support/secured dibatasi role/permission surface;
- direct client SQL tetap dilarang.

## 8. Migration recommendation

### 8.1 Ownership

- Setiap DbContext mempunyai migration assembly dan migration history table sendiri pada schema owner, misalnya jobs.__EFMigrationsHistory. Multi-schema AccountAdministration memakai satu history table yang ditetapkan konsisten.
- Migration file dinamai dengan module prefix dan objective.
- Tidak ada migration module A yang mengubah table module B.
- account/admin memakai satu AccountAdministration migration unit karena satu module owner.

### 8.2 Deployment

1. Generate idempotent SQL migration script/bundle dari reviewed source.
2. Review SQL, lock/index impact, data movement, permission, dan rollback/forward-fix plan.
3. Jalankan di Development.
4. Jalankan integration/contract test.
5. Uji di QA dengan production-like row count/size.
6. Ambil backup/checkpoint deployment sesuai DBA policy.
7. Jalankan migration melalui approved pipeline identity.
8. Verifikasi schema version, reference seeds, constraints, indexes, health, dan application compatibility.
9. Baru deploy/enable application change sesuai expand-contract stage.

Production application startup tidak menjalankan Database.Migrate otomatis.

### 8.3 Expand-and-contract

Untuk compatible release:

1. **Expand:** tambah nullable/new table/new index tanpa menghapus contract lama.
2. Deploy code yang dapat membaca lama dan baru.
3. Backfill batch kecil, resumable, terobservasi.
4. Beralih write/read ke bentuk baru.
5. Verifikasi seluruh MinimumSupportedVersion/client contract.
6. **Contract:** hapus old field/table hanya pada migration/release terpisah setelah compatibility window.

Rename column langsung diperlakukan breaking; gunakan add-copy-switch-drop.

### 8.4 Large table changes

- Audit, job history, outbox, upload, dan attachment metadata dapat tumbuh cepat.
- Index build/rebuild, backfill, partitioning, dan compression harus berdasarkan SQL edition/capacity evidence.
- Data correction dan retention purge bukan schema migration reguler.
- Backfill memakai bounded batches dan progress checkpoint.
- Hindari long transaction yang menahan lock seluruh table.

### 8.5 Reference data migration

- Job/upload/issue status dan permission/feature codes di-seed secara explicit dan repeatable.
- Code rename/removal adalah contract change dan memerlukan migration/compatibility mapping.
- Admin UI tidak boleh menciptakan status API baru.
- Display localization berada pada application resources atau owned localization metadata, bukan mengganti stable code.

### 8.6 Rollback

Preferred response setelah migration Production adalah forward-fix untuk schema additive. Destructive rollback hanya jika telah diuji, data-preserving, dan disetujui. Setiap destructive migration memerlukan backup/restore evidence dan explicit approval.

## 9. Retention-sensitive tables

Exact period tidak ditetapkan di laporan ini. Semua nilai harus berasal dari typed policy, legal/security requirement, benchmark, storage capacity, dan approved owner.

| Table/group | Sensitivity | Required lifecycle fields/controls |
|---|---|---|
| account.Sessions | Security/session metadata | ExpiresAtUtc, RevokedAtUtc, purge after policy; no token |
| workspace.SyncChanges | Change-feed growth | ExpiresAtUtc/PurgeAfterUtc; cursor window documented |
| workspace.Presets | User metadata and potentially sensitive parameters | Soft delete/purge; schema migration; secret prohibition |
| jobs.UploadSessions/UploadParts | Temporary file-transfer metadata | ExpiresAtUtc, terminal cleanup, orphan reconciliation |
| jobs.JobArtifacts | Temporary/result storage pointer | ExpiresAtUtc, state, storage deletion outbox |
| jobs.JobCheckpoints | Temporary recovery metadata | Compatibility version, verification, expiry |
| module-owned IdempotencyRecords | Replay/security metadata | ExpiresAtUtc; hash only; safe snapshot size limit; no global multi-writer table |
| jobs.AdmissionReservations | Capacity correctness | Lease/expiry/release; stale reservation reconciliation |
| jobs.JobAttempts/StateHistory | Operational evidence | Retain per recovery/support policy; archive/purge independent of Job current row |
| jobs.OutboxMessages | Delivery reliability | PublishedAtUtc, attempt count, PurgeAfterUtc; poison handling |
| secured.FileVersions | User secured content metadata | Locked maximum three versions; retention and object cleanup |
| secured.RecycleEntries | Account delete lifecycle | DeletedAtUtc, PurgeAfterUtc, restore state |
| secured.OutboxMessages | Storage cleanup/version notification | PublishedAtUtc, PurgeAfterUtc, reconciliation |
| history.Entries | User-removable personal metadata | DeletedAtUtc/PurgeAfterUtc; audit remains separate |
| audit.AuditEvents/IngestionBatches | Enterprise/compliance evidence | Append-only, authorized retention purge, no user-delete cascade |
| audit.ExportRequests | Potential sensitive report artifact | Job/artifact expiry and access control |
| support.IssueAttachments | Screenshot may be sensitive | Object retention, explicit user-send evidence, purge coordination |
| support.Issues/Notes/History | User message and internal handling | Owner-defined retention, access controls, no arbitrary deletion |
| support.IdempotencyRecords | Issue command replay | ExpiresAtUtc, safe snapshot, hashed key |
| support.OutboxMessages | Realtime delivery metadata | Published/expiry/purge |
| assistant metadata | Intent/AI routing metadata | Minimal fields, ExpiresAtUtc, no conversation content |
| intelligence metadata | Sensitive processing trace | Minimal fields, policy-defined retention, no extracted/result text |

### 9.1 Purge principles

- Purge worker menggunakan bounded batches, deterministic eligibility, idempotency, metrics, dan audit where required.
- SQL metadata deletion dan object-storage deletion tidak dianggap atomic. Gunakan tombstone/state + durable cleanup command/outbox + reconciliation.
- Purge tidak mengikuti cascade lintas module.
- Audit retention dikelola oleh authorized policy dan tidak dapat dihapus oleh personal history delete.
- Legal/security hold bila kelak diperlukan harus menjadi explicit controlled requirement, bukan asumsi tersembunyi.
- Retention index dibuat pada ExpiresAtUtc/PurgeAfterUtc + PK.

## 10. Conflicts dan open risks

### 10.1 Conflicts found

1. **Document 06 status mismatch dengan task wording.** Task meminta membaca Documents 01–06 FINAL, tetapi workspace menetapkan Document 06 v0.5 sebagai WORKING/ACTIVE. Seluruh 9 API decisions telah locked dan dipakai sebagai database baseline; Document 06 final cross-document audit/finalization masih tercatat belum selesai.
2. Tidak ditemukan konflik fatal antara satu SQL Server, schema ownership per module, no direct client SQL, metadata-only storage, EF Core, Local-First, REST-authoritative jobs, persisted idempotency, upload lifecycle, dan SignalR recovery.

### 10.2 Open risks requiring downstream decision/evidence

| Risk/open item | Impact | Required owner/input |
|---|---|---|
| Exact SQL Server version/edition/topology belum ditetapkan | Online index, compression, partitioning, HA/DR capability dapat berbeda | Infrastructure/Deployment/DBA |
| Document 06 belum FINAL | Route/entity terminology masih dapat menerima editorial finalization | Document 06 owner |
| Security Design dan Role Matrix belum FINAL | Permission catalog, claim mapping, support/audit access belum final | Documents 08–09 |
| Exact retention/TTL values deferred | Purge capacity dan storage sizing belum final | Business owner, Security, DBA, typed policy |
| Identity issuer/subject/tenant/unit claim contract belum final | account.Principals columns/indexes dapat berubah | Approved Pertamina Identity + Security Design |
| Local-only history vs server history sync boundary perlu ditegaskan | Risiko menyinkronkan local path/history yang tidak diizinkan | Product/API/Privacy owner |
| Feature Registry code artifact vs database snapshot deployment process perlu dikunci | Risiko database menjadi mutable source of contract truth | Architecture/API/Configuration owner |
| Public ID format/maximum length belum final | Column length/index sizing | API contract implementation |
| Job queue workload classes/priority/fairness numeric policy belum final | jobs indexes/admission model tuning | Capacity benchmark/API-DEC-009 policy |
| Outbox retention, poison handling, and publisher topology belum final | Event lag/table growth | Processing/Support/Operations |
| Object storage consistency and orphan reconciliation belum detailed | File leak atau dangling metadata | Storage/Security/Operations |
| Assistant server metadata necessity must remain minimal | Privacy risk and duplication with audit | Assistant/Security/Privacy |
| Content Intelligence providers still PoC-gated | Provider/profile/usage metadata may change | Feasibility, Security, License review |
| Audit volume and retention unknown | Partitioning/index/storage plan cannot be locked | Audit owner + capacity test |
| Cross-module logical refs have no DB FK | Orphan reference possible after failures | Application validation + reconciliation monitoring |
| READ_COMMITTED_SNAPSHOT recommendation not yet approved | Blocking/concurrency behavior not final | DBA/performance test |

### 10.3 Decisions this report intentionally does not make

- exact business retention duration;
- exact rate/quota/concurrency numbers;
- exact role/permission catalog;
- exact identity provider protocol/claims;
- SQL Server edition/HA/DR topology;
- object-storage vendor;
- provider/model/vendor for PDF/OCR/Content Intelligence;
- whether optional sync change feed is required;
- production partition/compression strategy;
- new business status beyond locked sources.

## 11. Proposed Document 07 acceptance checks

Bagian tambahan ini bukan keputusan bisnis; ini adalah checklist untuk memverifikasi bahwa desain database final mempertahankan locks.

~~~text
[ ] Satu SQL Server baseline; schema owner jelas
[ ] Tidak ada module table tanpa write owner
[ ] Tidak ada client/direct SQL path
[ ] Tidak ada cross-module write
[ ] Cross-module FK diminimalkan dan dijustifikasi
[ ] File/screenshot/checkpoint/result content tidak menjadi SQL blob default
[ ] Local draft/cache/undo/thumbnail/path tidak tersinkron ke SQL
[ ] JobId tetap saat retry/resume; attempts append-only
[ ] CorrelationId terindeks dan traceable
[ ] Job state tidak mempunyai PAUSED
[ ] Upload state hanya memakai locked lifecycle
[ ] Idempotency unique scope + fingerprint persisten
[ ] SignalR event berasal dari persisted state/outbox
[ ] Assistant table tidak menyimpan prompt/chat/document content
[ ] Intelligence table tidak menyimpan OCR/summary/translation text
[ ] Audit append-only dan tidak ikut personal-history deletion
[ ] Secured file maximum-three version invariant diuji concurrency
[ ] Setiap mutable aggregate mempunyai RowVersion
[ ] Setiap DbContext hanya memetakan owned schema
[ ] Migration per module dan tidak otomatis saat Production startup
[ ] Retention/purge mempunyai explicit policy fields, indexes, dan reconciliation
~~~
