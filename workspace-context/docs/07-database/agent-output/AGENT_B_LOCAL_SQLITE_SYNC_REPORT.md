# AGENT B — LOCAL SQLITE & SELECTIVE SYNC REPORT

**Task ID:** `DOC07-AGENT-B`  
**Role:** SQLite + Local-First Specialist  
**Scope:** Desktop WinUI 3 dan Mobile/Tablet .NET MAUI  
**Status:** RECOMMENDATION — tidak mengubah Document 07 master dan bukan production schema/code

> **Baseline note:** Documents 01–05 tersedia sebagai FINAL. Workspace belum memuat Document 06 FINAL; artefak terbaru adalah `docs/06-api/06_API_DOCUMENTATION_WORKING.md` v0.5 dengan 9/9 API decisions LOCKED, didukung decision records dan integration audit. Laporan ini memakai contract locked tersebut tanpa menyatakan Document 06 sudah FINAL.

## 1. Recommended local data boundary

### 1.1 Tiga storage boundary

```text
Local Workspace filesystem
→ source reference, local result, cache, thumbnail, temp,
  draft/checkpoint artifact, optional protected signature asset

SQLite per local profile/device
→ metadata, state, operation log, history, queue, recovery,
  preferences, presets, sync state, secured-file metadata cache

Server SQL + Secure File/Object Storage
→ enterprise metadata, persisted server job state,
  audit, dan file/result yang dipilih melalui Amankan ke Akun
```

SQLite menyimpan metadata/state, bukan seluruh file pengguna. Database SQLite **tidak pernah disalin atau disinkronkan sebagai satu file** ke server, cloud drive, atau perangkat lain. User file/result tetap di filesystem; SQLite hanya memegang local opaque reference dan lifecycle metadata.

### 1.2 Sync classification wajib

| Data | SQLite/local storage | Outbound sync | Rule |
|---|---|---|---|
| Source file dan local source locator | Lokal | **Tidak** | Tidak disalin; tidak ditimpa otomatis |
| Draft, working state, operation log, undo/redo | Lokal | **Tidak** | Local-only tanpa exception baseline |
| Cache, thumbnail, temp | Lokal | **Tidak** | Evictable; bukan recycle-bin/account content |
| Checkpoint lokal | Lokal | **Tidak** | Hanya recovery lokal; server checkpoint punya lifecycle server sendiri |
| Local history dan recent files | Lokal | **Tidak** | Personal metadata; terpisah dari enterprise audit |
| Preferences, favorites, presets | Lokal pada scope tugas ini | **Tidak otomatis** | Dokumen lama mengizinkan metadata ringan “bila disetujui”, tetapi tugas ini membatasi sync ke Amankan ke Akun |
| Pending local/server intent | Lokal | Metadata command hanya jika user memang memulai server action | Tidak boleh memicu upload content tanpa connectivity/policy/consent recheck |
| Validated eligible file/result yang dipilih user | Lokal + explicit outbox | **Ya, hanya lewat Amankan ke Akun** | Baseline utama adalah final result; secure transfer + idempotency; server maksimum tiga versions |
| Secured-file list/version metadata cache | Lokal, server-derived | Pull/reconcile dari server | Cache, bukan authority; tidak membawa draft/source |
| Enterprise audit metadata | Local compliance outbox saat offline | **Ya, metadata-only** | Bukan workspace sync; tidak berisi document/password/token/clipboard/signature/OCR text |

Favorite/preset/language/view sync tidak diaktifkan oleh desain ini. Jika kelak disetujui, itu memerlukan controlled decision dan typed allowlist tersendiri; tidak boleh memanfaatkan generic workspace sync.

Temporary processing upload untuk feature Optional/Required adalah execution transfer sesuai Document 06, **bukan sync/backup**. Source tidak pernah auto-sync; processing upload tetap memerlukan resolver, policy, connectivity, permission, capacity, dan explicit consent.

### 1.3 Database ownership dan access pattern

- Gunakan satu logical schema bersama Desktop/Mobile, dengan platform adapter untuk file locator dan secure storage.
- Database dipisah per local OS profile + Pertamina subject profile. Account switch menutup database aktif dan membuka profile store berbeda; data antar-user tidak dicampur.
- Semua akses melalui repository/application abstraction seperti `ILocalWorkspaceStore`; business rule tidak boleh bergantung pada SQL statement atau SQLite API.
- Satu persistence writer per profile disarankan. UI dan Local Worker tidak menulis tabel yang sama secara bebas.
- Desktop Local Worker mengirim progress/checkpoint manifest melalui typed local IPC. Persistence owner melakukan transaction SQLite; worker tidak memperoleh arbitrary database access.
- Mobile background callback memakai writer yang sama/serialized queue dan transaksi singkat karena OS dapat suspend/terminate aplikasi.

## 2. Local SQLite entity design

### 2.1 Conventions

| Area | Recommendation |
|---|---|
| IDs | Opaque locally generated ID (`TEXT` UUID/ULID-equivalent); format bukan business contract |
| Time | UTC integer epoch milliseconds; timezone hanya untuk presentation |
| Boolean | `INTEGER` dengan `CHECK (value IN (0,1))` |
| Enum | Stable code + `CHECK` untuk invariant penting; unknown-safe migration policy |
| Concurrency | `row_version INTEGER` naik pada setiap mutation penting |
| Relationship | `PRAGMA foreign_keys=ON`; explicit delete behavior, tidak cascade untuk audit/recovery tanpa review |
| Payload | Typed/versioned JSON hanya untuk state kecil; bukan arbitrary command/document blob |
| File bytes | Tidak disimpan sebagai SQLite BLOB; gunakan `local_artifact` + app-managed filesystem |
| Paths | Simpan opaque locator/protected platform token; physical path bukan cross-device identity |
| Integrity | Hash/checksum artifact jika recovery memerlukannya; full source hash hanya on-demand |
| Index | Hanya query nyata; hindari duplicate index dan unbounded FTS/document-content index |

Jangan menaruh business logic kompleks di trigger. Gunakan database constraints untuk integritas dasar dan application service untuk transition/policy.

### 2.2 Profile, runtime, dan migration tables

| Table | Tujuan | Field minimum / constraint |
|---|---|---|
| `local_profile` | Isolasi data per Pertamina subject pada device | `profile_id PK`, protected/hashed subject + tenant binding, `status`, `created_at`, `last_opened_at`; unique subject/tenant binding |
| `offline_authorization_snapshot` | Narrow cached permission untuk fitur lokal selama offline session valid | `profile_id PK/FK`, `policy_version`, allowlisted capability snapshot, `issued_at`, `expires_at`, integrity marker; **tanpa bearer/refresh token** |
| `app_run` | Crash/unclean-shutdown detection | `run_id PK`, profile, app/schema version, `started_at`, heartbeat, `clean_shutdown_at`, termination marker |
| `schema_migration` | Riwayat migration durable | `migration_id PK`, from/to version, checksum, app version, applied time, status |
| `migration_journal` | Resume migration yang perlu file operation/chunking di luar satu transaction | step, cursor, artifact reference, started/completed/error; tidak menyimpan secret |

Raw identity token dan refresh token tetap pada approved OS secure storage, bukan SQLite.

### 2.3 Local file/workspace entities

| Table | Tujuan | Field minimum / index |
|---|---|---|
| `local_file_ref` | Referensi source/result lokal tanpa menyalin content | `file_ref_id PK`, profile, `locator_kind`, protected locator/token, protected display name, extension/MIME, size, last modified, platform file identity, optional quick/full fingerprint, availability state, last verified; index profile + last verified |
| `workspace` | Satu active editing/draft context | `workspace_id PK`, profile, canonical `feature_id`/workflow ID, contract/schema version, status, dirty flag, operation cursor, last snapshot sequence, created/updated/autosaved/closed time, `row_version` |
| `workspace_input` | Multi-file inputs dan roles | composite key workspace + input order/ID, `file_ref_id`, input role, captured size/modified/fingerprint, validation state; no file bytes |
| `draft_snapshot` | Autosave/compaction snapshot | `snapshot_id PK`, workspace, through operation sequence, typed state schema version, protected small state or `artifact_id`, checksum, created time, valid flag; unique workspace + sequence |
| `working_operation` | Append-only operation log untuk undo/redo | `operation_id PK`, workspace, unique monotonic sequence, operation code, typed payload schema, protected minimal forward/inverse data, created time; index workspace + sequence |
| `local_artifact` | Manifest seluruh app-managed file lokal | `artifact_id PK`, profile, owner kind/ID, kind (`DRAFT_STATE`, `CACHE`, `THUMBNAIL`, `TEMP`, `CHECKPOINT`, `INTERMEDIATE`, `LOCAL_RESULT`, `SIGNATURE_ASSET`), opaque relative locator, size, checksum, classification, state, expiry, last access, lease owner/until, delete attempts |

Rules:

- `working_operation` setelah current operation cursor adalah redo branch. Operasi baru setelah undo memangkas/menandai redo branch secara transactionally; tidak menyentuh source.
- Autosave menggunakan debounce dan menyimpan delta/snapshot, bukan full output setiap perubahan.
- Snapshot + operation cursor di-commit dalam satu transaction. Snapshot lama hanya boleh dihapus setelah snapshot pengganti tervalidasi.
- Source path/locator dapat berubah atau kehilangan permission. `local_file_ref` menyimpan identity evidence agar aplikasi mendeteksi `MISSING`, `MOVED`, `ACCESS_REVOKED`, atau `CHANGED`.
- Large/derived state berada pada protected local artifact. SQLite hanya menyimpan reference/integrity/lifecycle.
- `SIGNATURE_ASSET`, bila user memang memilih menyimpannya lokal, harus menjadi encrypted local artifact terpisah; bukan SQLite BLOB dan tidak pernah masuk sync outbox.

### 2.4 Job, checkpoint, queue, dan recovery entities

| Table | Tujuan | Field minimum / index |
|---|---|---|
| `local_job` | Authoritative state untuk local job dan local view dari pending execution | `job_id PK`, profile, job kind, feature/workflow ID, workspace, correlation ID, locked job status, safe progress/stage/counts, attempt number, cancel flag, retryable, next retry, error code, execution lease, started/completed, `row_version`; index profile + status + updated |
| `job_input` | Input identity per job | job + input ID, local file ref, captured version/fingerprint, input purpose, requires content transfer, validation state |
| `job_attempt` | Append-only attempt history | job + unique attempt number, trigger (`INITIAL`, `AUTO_RETRY`, `USER_RETRY`, `RESUME`), start/end, engine/provider/contract version, checkpoint, outcome, safe error code |
| `checkpoint` | Resume metadata untuk local job | `checkpoint_id PK`, job/workspace, `artifact_id`, stage, completed/total, input fingerprint, engine/provider/feature/schema versions, integrity hash, created/expiry, state, resume allowed |
| `pending_server_intent` | Typed pre-submission command ketika offline/blocked | `intent_id PK`, profile, command type, feature/schema/contract version, protected typed parameter state, idempotency-key reference + hash, request fingerprint, submission state/reason, consent state/version/time, attempt count/next retry, server resource ID if accepted |
| `pending_server_intent_input` | Local input refs untuk queued server intent | intent + input ID, local file ref/result artifact, captured fingerprint, upload required, upload ID if created |
| `server_job_cache` | Local cache untuk server job REST state/Activity Center | server job ID PK, profile, correlation/feature ID, persisted status/progress, server `state_version`/sequence, ETag, last REST reconciliation; explicitly marked `SERVER_AUTHORITATIVE` |
| `recovery_candidate` | Startup recovery decision | candidate ID, profile, resource kind/ID, detected reason, available actions, requires secret/consent/online flag, detected/resolved time, selected action |

Locked job states tetap:

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

Tidak ditambahkan state `PAUSED`. State `pending_server_intent` adalah submission lifecycle sebelum server Job ID ada, bukan pengganti job-state contract.

### 2.5 History, recent files, preferences, dan presets

| Table | Tujuan | Field minimum / index |
|---|---|---|
| `local_history` | Metadata-only personal history | history ID, profile, local/server job ID, Feature ID, protected display name, source/result file ref, status, occurred/completed time, size, duration, safe error code, deleted time; index `(profile_id, occurred_at DESC, history_id DESC)` dan feature/status |
| `recent_file` | Recent list tanpa menyalin file | profile + `file_ref_id` unique, last opened/used, open count, last Feature ID, pinned flag; index last used descending |
| `user_preference` | User/device overrides yang diizinkan | profile/device scope + preference key unique, typed value, value schema version, updated time; local-only |
| `favorite_tool` | Shortcut canonical Feature ID | profile + Feature ID unique, sort rank, created/updated; local-only |
| `preset` | Typed reusable feature/workflow parameters | preset ID, profile, name, canonical Feature/workflow ID, parameter/workflow schema version, protected typed payload, compatibility state, created/updated; local-only pada baseline ini |

Personal history delete hanya membuat row hilang/tombstoned dari UI sesuai retention; tidak menghapus enterprise audit. Initial history query maksimum 50 item dan memakai stable ordering/lazy load.

Default save location adalah device-specific preference. Jangan menyinkronkan path Desktop ke Mobile.

### 2.6 Selective-sync dan secured-file cache entities

| Table | Tujuan | Field minimum / index |
|---|---|---|
| `secured_transfer` | Outbox **khusus** aksi eksplisit Amankan ke Akun | transfer ID, profile, eligible validated file/final-result artifact ref, captured fingerprint, explicit action time, consent/policy version, idempotency key reference/hash, request fingerprint, upload ID, server secured-file/version ID, base version + ETag, status, attempt/next retry, error, timestamps |
| `secured_file_cache` | Server-derived local catalog cache | secured file ID PK, profile/owner binding, protected display name, extension/MIME, size, current version ID, version count, ETag, server state, retention metadata, server updated time, last reconciled time, optional local working-copy ref |
| `secured_file_version_cache` | Cache maksimum tiga server versions | version ID PK, secured file FK, ordinal/created time, size/MIME, safe checksum metadata if exposed, ETag, state; index secured file + created descending |
| `sync_cursor` | Incremental pull/reconciliation position | profile + stream kind unique, opaque protected cursor, cursor expiry, last sync, last success/error, server watermark/ETag |
| `sync_conflict` | Conflict evidence dan user resolution | conflict ID, profile, operation/resource IDs, type, captured local fingerprint/base version, current server version/ETag, detected time, permitted resolutions, selected resolution/time, status |
| `audit_outbox` | Required offline enterprise audit metadata | event ID PK, profile, process/job/correlation/Feature ID, status, timestamps, size category/bytes if allowed, local/server indicator, safe error/app version, attempt/next retry; **no content fields** |

`secured_file_cache` dan version cache adalah projection. Server REST/SQL state tetap authority. Client tidak menghapus versi server hanya karena cache melihat version keempat; server melakukan max-three rotation secara transactional, lalu client refresh.

Tidak ada generic “sync all tables” flag. Table local-only tidak mempunyai sync trigger/outbox mapping. Ini merupakan guard struktural terhadap full cloud workspace.

## 3. Sync-state model

### 3.1 Explicit Amankan ke Akun lifecycle

```text
No secured_transfer row
  ↓ user selects eligible validated file/final result + presses Amankan ke Akun
REQUESTED
  ↓ local identity/integrity + capacity + policy preflight
PREPARING
  ├─ offline ───────────────→ WAITING_CONNECTION
  ├─ auth/policy stale ─────→ WAITING_AUTHORIZATION
  ├─ result changed ────────→ CONFLICT
  └─ ready
UPLOADING
  ↓ upload READY
COMMITTING
  ├─ transient failure ─────→ RETRYING → UPLOADING/COMMITTING
  ├─ version conflict ──────→ CONFLICT
  ├─ permanent failure ─────→ FAILED
  ├─ user cancel ───────────→ CANCELLED
  └─ server secured resource/version committed
SYNCED
```

State di atas khusus `secured_transfer`, bukan job-state API; `PAUSED` tetap tidak ada.

### 3.2 Preconditions sebelum content upload

Sebelum setiap first upload/resume/commit:

1. File/result memenuhi eligibility policy, identity-nya stabil, dan result processing sudah final/tervalidasi.
2. User/profile ownership cocok.
3. Local result identity/fingerprint belum berubah sejak request.
4. Session online/auth valid; permission/policy terbaru mengizinkan.
5. Consent/action record masih berlaku untuk file, purpose, dan version yang sama.
6. Server storage/quota/admission preflight lulus.
7. `Idempotency-Key` dan request fingerprint tersedia.

Jika file berubah, consent/policy tidak dianggap otomatis berlaku pada bytes baru. State menjadi `CONFLICT` atau menunggu user re-confirmation; tidak ada silent upload.

### 3.3 Server version and cache reconciliation

- Create new secured file memakai idempotency key dan server ownership.
- Add version memakai base `versionId`/`ETag` dan conditional request.
- `409/412` membuat `sync_conflict`; tidak ada last-write-wins.
- Server menjaga maksimum tiga versi. Version keempat memicu server-side rotation sesuai policy dalam unit kerja yang konsisten.
- Pull catalog memakai API cursor/ETag, apply batch dalam satu local transaction, dan hanya advance `sync_cursor` setelah seluruh batch commit.
- Delete/tombstone server diterapkan ke cache; local working copy tidak dihapus diam-diam.
- Offline cache selalu menampilkan `lastReconciledAt`; sensitive server action menunggu online authorization.
- File secured yang dibuka menjadi local working copy baru. Edit copy tetap local-only sampai user menekan Amankan ke Akun lagi.

## 4. Offline queue and retry state

### 4.1 Queue separation

Gunakan tiga typed queue, bukan satu arbitrary JSON/API outbox:

1. `pending_server_intent` — user-initiated server command/job metadata.
2. `secured_transfer` — satu-satunya queue yang boleh membawa reference ke bytes hasil untuk Amankan ke Akun.
3. `audit_outbox` — compliance metadata-only dan dapat auto-flush sesuai locked audit requirement.

Queue dilarang menyimpan arbitrary URL, SQL, class/engine name, shell command, raw HTTP body, document bytes, password, clipboard, atau signature.

### 4.2 Pending server intent flow

```text
User starts feature while offline/server unavailable
→ persist typed intent + canonical Feature ID + schema version
→ status WAITING_CONNECTIVITY
→ network returns
→ refresh authentication/permission/policy/capability
→ revalidate local input identity
→ if content upload required, verify explicit consent
→ use locked upload contract
→ execute with same Idempotency-Key/fingerprint
→ persist returned server Job ID
→ reconcile via REST; SignalR only notification
```

Queued intent tidak otomatis mengunggah source. Bila consent tidak ada/stale, set submission reason `USER_ACTION_REQUIRED` dan tampilkan pilihan; jangan menebak.

### 4.3 Retry fields dan rules

Setiap retryable queue row minimum memuat:

```text
attempt_count
max_attempts_policy_version
last_attempt_at
next_attempt_at
last_error_code
error_class: TRANSIENT | PERMANENT | USER_ACTION | POLICY
idempotency_key_ref + idempotency_key_hash
request_fingerprint
lease_owner + lease_until
```

Rules:

- Exponential backoff + jitter, bounded by typed policy.
- Hormati server `Retry-After`.
- Claim row memakai transaction/lease agar dua worker tidak mengirim command yang sama.
- Crash setelah server accept tetapi sebelum local ack dipulihkan dengan key yang sama; server me-replay Job/resource ID yang sama.
- Retry hanya untuk connectivity, timeout, `429`, retryable `503`, dan allowlisted transient errors.
- Validation, unsupported format, authz denial, malware block, stale consent, version conflict, dan changed source tidak auto-retry.
- Setelah budget habis, state `FAILED`/`USER_ACTION_REQUIRED`; tidak loop tanpa akhir.
- Retry/resume server job mempertahankan logical Job ID dan append-only attempt history sesuai API-DEC-005.
- Mobile background execution boleh berhenti karena OS budget. Persist `next_attempt_at`; lanjutkan pada permitted wake/foreground tanpa membuat duplicate.

## 5. Conflict detection and resolution

### 5.1 Conflict evidence

| Konflik | Deteksi | Safe resolution |
|---|---|---|
| Source/result berubah setelah draft/job/secure request | Platform file ID + size + modified time + optional quick/full fingerprint | Revalidate; pilih gunakan versi baru, buat request baru, atau cancel |
| Source dipindah/hilang/permission dicabut | File locator resolution gagal atau platform access token invalid | Locate ulang via user, keep draft reference, atau discard; jangan scan seluruh storage |
| Dua writer mengubah workspace | `row_version`/expected operation sequence mismatch | Reload current state; single-writer arbitration; jangan overwrite row |
| Secured version server berubah | ETag/base version mismatch (`409/412`) | Refresh; Save as new secured file, add new version setelah confirmation, download server copy terpisah, atau cancel |
| Secured file dihapus/retention berubah server | Pull tombstone/policy response | Mark cache server-deleted; jangan menghapus local copy otomatis |
| Parameter/preset schema berubah | Stored schema version tidak kompatibel | Migration/compatibility adapter; tandai `INCOMPATIBLE` dan minta edit, jangan menjalankan nilai lama diam-diam |
| Checkpoint engine/input tidak cocok | Engine/schema/input fingerprint mismatch | Resume ditolak; tawarkan retry dari safe stage/start bila user memilih |
| Account/profile mismatch | Current profile != row owner binding | Tutup resource dan switch profile; tidak merge database/data |

Timestamp saja tidak boleh menjadi conflict authority. Binary/PDF result tidak di-auto-merge. Silent overwrite dilarang untuk source, local result, secured version, dan cache projection.

### 5.2 Local optimistic concurrency

- Mutation workspace membawa expected `row_version` dan expected operation cursor.
- Job/checkpoint progress update memakai expected attempt + execution lease.
- Server cache apply membawa expected server ETag/state version.
- Conflict record menyimpan metadata evidence, bukan content.
- UI menawarkan resolusi yang tersedia; aplikasi tidak memilih destructive resolution untuk user.

## 6. Local cleanup and retention

### 6.1 Artifact cleanup policy

| Artifact | Cleanup rule |
|---|---|
| `TEMP` / invalid intermediate | Delete setelah failure/cancel/final validation, kecuali masih leased |
| Cache | LRU/quota eviction saat storage pressure/idle; aman diregenerasi |
| Thumbnail | On-demand; LRU/quota; tidak masuk recycle bin |
| Draft snapshot / operation | Pertahankan active/recoverable draft; compact hanya setelah replacement validated |
| Checkpoint | Pertahankan selama job/recovery window; hapus setelah terminal + recovery expiry/user discard |
| Local result | Bukan cache; jangan hapus otomatis hanya karena history dibersihkan. User/policy action harus jelas |
| Signature asset | User-controlled local asset; separate explicit clear/delete; never sync |
| Secured working copy | File lokal independen; server tombstone tidak menghapusnya otomatis |
| Personal history | Metadata retention configurable; delete tidak menghapus audit atau file secara implisit |
| Audit outbox | Delete hanya setelah server acknowledgement + retention policy; no content |

### 6.2 Safe cleanup algorithm

1. Query `local_artifact` by state/expiry/last-access; jangan scan seluruh device.
2. Skip active lease dan resources referenced active draft/job/checkpoint/transfer.
3. Transactionally mark `DELETE_PENDING` dengan cleanup owner/lease.
4. Resolve opaque relative path **hanya di app-managed root**; never delete arbitrary external source path.
5. Delete file; persist `DELETED` tombstone or retry metadata.
6. Remove metadata only after file outcome known and references cleared.
7. Reconcile orphan files only inside known app-managed directories using manifests; bounded batch.

Cleanup berjalan batch kecil, low priority, dan power/storage aware. Jangan menjalankan full `VACUUM` pada setiap startup. Gunakan WAL checkpoint/incremental vacuum/compaction pada idle window setelah benchmark.

Secure deletion pada flash/SSD tidak dapat dijamin hanya dengan overwrite. Protection utama adalah encryption-at-rest + key lifecycle; exact purge/WAL policy harus dikunci pada Security Design.

## 7. Crash recovery

### 7.1 Durability/write ordering

- Semua state transition penting memakai transaction.
- File artifact dibuat pada app-managed temp name, ditutup/flushed, divalidasi/checksum, lalu atomic rename pada filesystem yang sama; baru SQLite pointer/state di-commit.
- Jika DB commit harus mendahului external operation, gunakan explicit intent/outbox record lalu idempotent completion/reconciliation.
- Autosave operation + cursor + snapshot pointer di-commit konsisten.
- Checkpoint artifact harus complete + integrity-valid sebelum checkpoint row menjadi `AVAILABLE`.
- Progress sangat sering di-coalesce; persist pada stage boundary/interval aman agar tidak membebani disk, sementara checkpoint menentukan resume truth.

### 7.2 Startup recovery sequence

```text
1. Acquire single-profile writer lock
2. Open SQLite with foreign keys and approved journal settings
3. Validate schema version / run migration before job dispatch
4. Detect previous app_run without clean shutdown
5. Let SQLite recover WAL; run bounded quick integrity check
6. Expire stale execution/outbox/cleanup leases
7. Reconcile known app-managed temp/checkpoint manifests
8. Classify draft/job/transfer recovery candidates
9. For server Job ID: REST GET persisted state
10. For ambiguous submission: retry same Idempotency-Key
11. Validate source/result identity and checkpoint compatibility
12. Present Lanjutkan / Coba Lagi / Batalkan
```

Recovery behavior:

- `RUNNING` local job tanpa live worker lease menjadi recovery candidate; jangan menganggap completed.
- Local Worker pada IPC loss harus safe-stop/checkpoint sesuai contract; worker tidak menulis final source.
- Server job state dari REST menggantikan stale local cache. SignalR gap hanya memicu reconciliation.
- Partial/temp output tidak dianggap final.
- Bila PDF password/secret diperlukan lagi, recovery state `requires_secret=true` dan user diminta memasukkan ulang; secret tidak pernah dipulihkan dari DB.
- Jika database corrupt, jangan reset/delete otomatis. Buka recovery path dari last validated local backup/snapshot atau read-only diagnostics; laporkan safe error/correlation metadata.
- Kegagalan recovery tidak pernah menimpa source.

## 8. Schema migration strategy

### 8.1 Versioning

- Gunakan ordered forward migrations dengan stable `migration_id`, checksum, source/target schema, dan tested compatibility range.
- `PRAGMA user_version` boleh menjadi quick marker, tetapi `schema_migration` adalah audit teknis lokal yang lebih lengkap.
- Stored typed payload memiliki schema version sendiri; database schema version tidak menggantikan Feature/Parameter/Workflow/Configuration version.
- App tidak membuka DB dengan schema lebih baru yang tidak dipahami. Downgrade ditolak dengan safe update/recovery message.

### 8.2 Migration execution

1. Tutup/stop job dispatch dan acquire exclusive migration lease.
2. Preflight free storage, battery/power state bila mobile, dan compatibility.
3. Buat encrypted/local-only SQLite backup menggunakan approved backup mechanism; exclude dari cloud/device backup.
4. Jalankan DDL/data migration dalam transaction jika aman.
5. Large transform menggunakan bounded chunks + `migration_journal`, dengan resume idempotent.
6. Jalankan foreign-key check, schema validation, dan targeted data invariants.
7. Commit version dan baru aktifkan UI/job dispatch.
8. Pada failure, pertahankan original/backup; jangan membuat DB kosong atau menghapus draft secara otomatis.

### 8.3 Required migration tests

- Setiap supported prior schema → current.
- Empty, normal, maximum realistic local dataset.
- Active draft/undo log/checkpoint/pending intent/secured cache.
- Crash/power loss pada setiap migration phase.
- Low disk, corrupt row/artifact, expired token, account switch.
- App upgrade then unsupported downgrade.
- Migration performance Desktop dan representative phone/tablet.
- Verification bahwa migration tidak membuat local-only data menjadi syncable.

## 9. Device-specific considerations

### 9.1 Windows Desktop

- Primary full-feature dataset: banyak workspace, large batch, richer history, local worker, dan lebih banyak checkpoint.
- Gunakan app-private profile directory; external source tetap di lokasi user.
- File identity sebaiknya menggabungkan protected path + volume/file identity bila tersedia + metadata fingerprint agar rename/move/change terdeteksi.
- Tangani Unicode, long path, case-insensitive semantics, removable/network drive, dan offline share.
- Satu writer/persistence owner; Local Worker berkomunikasi melalui versioned Named Pipe/local IPC.
- WAL dapat cocok untuk responsive reads, tetapi journal/synchronous/busy-timeout/WAL checkpoint values harus dibenchmark dan mengikuti encryption/recovery policy.
- Batch cleanup dan compaction dijalankan saat idle, bukan menghambat UI.

### 9.2 Mobile/Tablet

- Database berada di app sandbox per device/profile; **jangan** taruh di shared documents/cloud-sync folder.
- DB, WAL/SHM, draft/cache/checkpoint/temp/signature directories wajib dikecualikan dari automatic OS/cloud backup. Jika target platform tidak menyediakan approved exclusion/control, itu menjadi release blocker untuk local-only guarantee.
- File reference memakai approved platform document handle/content URI/security-scoped bookmark/token; raw desktop path tidak portable.
- OS dapat suspend/kill app kapan saja: transaksi singkat, lifecycle checkpoint, durable outbox sebelum network, dan idempotent resume wajib.
- Background network/CPU tidak dijamin. Queue bertahan sampai permitted wake/foreground; UI menampilkan pending state.
- Storage lebih terbatas: cache/thumbnail quota lebih kecil, early storage preflight, aggressive safe LRU, dan no whole-device scan.
- Hormati metered/roaming/battery policy sebelum optional transfer; Required server action tetap memberi penjelasan dan tidak silent-upload.
- Camera scan/visual signature dapat menghasilkan sensitive local artifact. File tetap app-private, encrypted sesuai policy, dan tidak auto-sync.
- Tablet boleh menyimpan workspace lebih kaya daripada phone, tetapi schema/business invariant tetap sama.

### 9.3 Cross-device rule

Local database dan local IDs tidak menjadi replication source. Cross-device identity hanya memakai server `securedFileId`/`versionId` setelah explicit Amankan ke Akun. A local working copy pada device baru mendapat `local_file_ref` dan workspace ID baru.

## 10. Privacy and security fields

### 10.1 Never store in SQLite

- document/file bytes;
- PDF password atau encryption password;
- access/refresh token, client secret, credential;
- raw clipboard history/content;
- visual signature image/biometric trace as BLOB;
- full OCR text/document extraction unless an explicitly approved local feature artifact is strictly required;
- server storage key, arbitrary download URL, raw provider response;
- public AI prompt/content cache;
- stack trace/raw engine error in user-state tables.

Jika derived document text/annotation wajib dipertahankan untuk local draft, simpan sebagai classified encrypted local artifact dengan explicit lifecycle; jangan masukkan ke telemetry/sync/outbox.

### 10.2 Protected/sensitive metadata

| Field | Protection |
|---|---|
| Subject/tenant binding | Opaque local profile + protected/hashed identifier; no token |
| Full file path/display name | Treat as sensitive metadata; protected DB/field or opaque platform locator; never telemetry by default |
| Operation/preset payload | Typed, minimized, encrypted/protected; reject secret fields |
| Idempotency key | Preserve to retry exact command using secure reference/field protection; store hash for lookup |
| Offline permission snapshot | Allowlisted, time-bound, integrity-protected; no bearer token; sensitive server action blocked offline |
| Error | Stable safe code only; raw exception stays in protected diagnostics with redaction policy |
| Correlation/Job ID | Allowed operational identifiers, but still scoped by profile and not authorization proof |
| Secured-file cache | Owner/profile-scoped, encrypted at rest per policy; server remains authority |
| Audit outbox | Metadata allowlist only; encrypted locally; delete after ack/retention rule |

### 10.3 Encryption/key and OS backup boundary

- SQLite core selection does not by itself settle encryption. Exact approved encryption provider/key management belongs to Security Design and dependency approval.
- Minimum baseline: app-private OS directory, device encryption where mandated, OS secure storage for keys/tokens, profile separation, least privilege, and encrypted sensitive cache/artifact.
- Key tidak disimpan berdampingan secara plaintext dengan database. Account logout locks/unmounts profile; deletion/key destruction behavior mengikuti retention/legal/security policy, bukan asumsi aplikasi.
- WAL, SHM, migration backup, crash dump, temp, dan diagnostic copy termasuk security boundary yang sama dengan main DB.
- Automatic OS/cloud backup dapat menjadi hidden sync; harus dinonaktifkan/dibatasi untuk seluruh local-only store sesuai approved platform mechanism.
- Administrator tidak otomatis dapat membaca local document metadata/content hanya karena role admin.

### 10.4 Data minimization and deletion

- Hanya simpan field yang dipakai recovery/UX/audit contract.
- Full hash source dihitung hanya bila integrity/conflict memerlukannya.
- History search tidak membuat unencrypted document-content index.
- User dapat menghapus personal history/conversation state tanpa menghapus mandatory enterprise audit.
- File/source delete dan metadata delete adalah aksi berbeda; jangan cascade diam-diam.
- Telemetry hanya count/duration/status/error/schema/migration health yang aman; tanpa document, filename/path bila tidak wajib, OCR, password, clipboard, atau signature.

## 11. Acceptance and architecture checks

Minimum tests untuk Document 07/implementation planning:

```text
[ ] No local-only table has sync trigger/outbox mapping
[ ] SQLite DB/WAL/backup is not placed in a cloud-synced folder
[ ] Source/draft/undo/cache/thumbnail/temp/checkpoint never enters secured_transfer
[ ] Only explicit eligible validated file/result creates secured_transfer
[ ] Audit outbox contains only approved metadata fields
[ ] Server maximum-three-version behavior is server-authoritative
[ ] Retry after ambiguous network outcome reuses Idempotency-Key
[ ] SignalR duplicate/gap cannot overwrite newer REST state
[ ] Source change and ETag conflict never use silent last-write-wins
[ ] Undo/redo, autosave, checkpoint, and recovery survive crash tests
[ ] Password/token/clipboard/signature/document bytes are absent from SQLite
[ ] Cleanup never deletes an arbitrary external source path
[ ] Account switch cannot expose another profile database/cache
[ ] Every supported schema migration survives failure/low-disk tests
[ ] Desktop Local Worker does not bypass local persistence abstraction
[ ] Mobile suspension resumes queue without duplicate job/transfer
```

Traceability utama:

- Business Process sections 8, 14–16, 19–22.
- `FR-AUTH-003..005`; `FR-FILE-005`; `FR-PROC-001..005`; `FR-STOR-001..006`; `FR-BACKUP-001..010`; `FR-HIST-001..003`; `FR-AUDIT-001..004`; `FR-REC-001..006`; `FR-UI-020..021`; `FR-VER-001..004`; `FR-ASSIST-009,011,012,015`.
- `NFR-PERF-004..010`; `NFR-REC-001..005`; `SR-SEC-001..021`.
- `FEAT-EDIT-001..012`; `FEAT-PROC-001..015`; `FEAT-STOR-001..016`; `FEAT-BACKUP-001..013`; `FEAT-HIST-001..015`; `FEAT-REC-001..015`; `FEAT-VER-001..006`; `FEAT-UX-017..018`.
- Solution Design sections 8–9, 11–13, 15–19, 21, 24, 26–28.
- `TECH-DEC-010`, `TECH-DEC-011`, `TECH-DEC-012`.
- `API-AUTO-004..007`; `API-DEC-004`, `API-DEC-005`, `API-DEC-007`, `API-DEC-008`, `API-DEC-009`.

## 12. Open dependencies and suggested Document 07 text

### 12.1 Open dependencies

1. **Document 06 finalization:** exact route/DTO inventory belum ada dalam file FINAL. SQLite design hanya menyimpan opaque server IDs, ETag/cursor/state version dan tidak bergantung pada physical endpoint name.
2. **Document 08 Security Design:** exact SQLite encryption provider, field encryption, key rotation/destruction, secure erase/WAL policy, crash dump, and OS-backup exclusion.
3. **Approved SQLite package/provider:** TECH-DEC-005 mewajibkan dependency/license/security/compatibility review; laporan ini tidak memilih package produksi.
4. **Retention/quotas:** exact draft, history, cache, checkpoint, outbox, local backup, and secured metadata cache retention membutuhkan policy + storage/performance test.
5. **Mobile target OS policies:** exact background scheduler, file-handle persistence, and backup-exclusion APIs ditentukan per approved target/runtime.
6. **Lightweight preference/preset sync:** dinonaktifkan pada scope ini karena user mengunci sync hanya untuk Amankan ke Akun. Aktivasi kelak memerlukan controlled decision; tidak derived otomatis dari older “bila disetujui” wording.

### 12.2 Suggested Document 07 text

**Local SQLite Boundary — LOCK RECOMMENDATION**

Desktop dan Mobile/Tablet menggunakan SQLite per local profile/device untuk metadata dan state lokal: source reference, workspace/draft, operation-based undo/redo, autosave snapshot, local job/attempt, checkpoint/recovery metadata, personal history, recent files, preferences, favorites, presets, pending server intent, selective-sync state, dan secured-file metadata cache. User file/result dan large local artifact tetap berada di Local Workspace filesystem; SQLite tidak menyimpan file bytes sebagai BLOB dan tidak menggantikan SQL Server.

Draft, working state, undo/redo, cache, thumbnail, temp, checkpoint, local history, recent file, preference, dan preset tidak mempunyai automatic sync mapping. SQLite database/WAL/backup juga tidak boleh berada pada cloud-synced atau automatic OS backup path. Only an eligible validated file/final result explicitly selected through **Amankan ke Akun** creates a `secured_transfer` outbox record. Enterprise audit metadata uses a separate metadata-only compliance outbox and is not workspace sync. Temporary server processing upload remains a separate consent/policy-gated execution transfer, not workspace sync.

Local persistence menggunakan repository abstraction, versioned typed state, foreign-key constraints, transaction, optimistic concurrency, single-writer coordination, artifact manifest/lease, crash marker, and forward tested schema migrations. Local Worker communicates through typed IPC and does not bypass persistence/application boundaries.

Offline server intent stores canonical Feature ID, contract/schema version, local input references, consent state, request fingerprint, and reusable idempotency key. Reconnect rechecks authentication, permission, policy, capability, content identity, and consent before upload/execute. Ambiguous network retry uses the same idempotency key so it cannot create duplicate server jobs or secured copies.

Secured-file metadata cache is server-derived and non-authoritative. Version mutation uses server ID + ETag/base version; conflict never uses silent last-write-wins. The server enforces maximum three secured versions transactionally. A file opened from secured storage becomes a new local working copy and is not uploaded again until the user explicitly selects Amankan ke Akun.

Crash recovery detects unclean app runs, stale leases, incomplete artifacts, active drafts, local jobs, pending intents, and secured transfers. It validates source/checkpoint identity, reconciles known server Job IDs through REST, and presents **Lanjutkan**, **Coba Lagi**, or **Batalkan**. Partial/temp output is never final and source is never overwritten automatically.

Sensitive local data follows Security Design. Password, token, clipboard history, signature bytes, document bytes, and full OCR text are not stored in SQLite. Paths/names, typed operation state, idempotency material, permission snapshot, secured metadata, WAL/SHM, migration backups, and local artifacts receive approved at-rest protection and retention controls.
