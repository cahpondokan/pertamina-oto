# AGENT B — FILE TRANSFER & BACKGROUND JOB API REPORT

**Task ID:** `DOC06-AGENT-B`  
**Focus:** `API-DEC-004 — File Transfer Strategy`; `API-DEC-005 — Idempotency & Retry Contract`  
**Status:** RECOMMENDATION — belum mengunci atau mengubah Document 06 master

Catatan sumber: package menyediakan `06_API_DOCUMENTATION_WORKING_v0.2_PERTAMINA_OTO.md`; file v0.1 yang disebut pada prompt tidak tersedia. Laporan ini memakai v0.2 sebagai working baseline terbaru, setelah 01 v3.4, 02 v1.5, 03 v1.3, 04 v1.2, dan 05 v1.2.

## 1. Recommended contract

### 1.1 Rekomendasi keputusan

**API-DEC-004 — pilih Hybrid File Transfer Contract.**

- `Local-only`: tidak ada upload, upload session, atau server job.
- `Optional server`: transfer hanya setelah resolver memilih server, policy/capability mengizinkan, dan user menyetujui pengiriman isi file.
- `Required server`: gunakan secure transfer contract yang sama; server tetap mengulang permission, policy, limit, storage, dan capacity check.
- File kecil/menengah memakai **streamed multipart one-shot**.
- File besar, jaringan tidak stabil, atau biaya restart tinggi memakai **upload session + raw chunk streaming yang resumable**.
- Job hanya mereferensikan `uploadId` berstatus `READY`; body create-job tidak membawa file lagi.
- Server temporary input/result berbeda lifecycle dari file **Amankan ke Akun**. Hasil tidak menjadi dokumen permanen hanya karena pernah diproses server.

**API-DEC-005 — pilih Persisted Idempotency Record + Request Fingerprint.**

- `Idempotency-Key` wajib pada command `POST` yang dapat membuat transfer, job, attempt, atau efek mahal.
- Scope key: authenticated principal/tenant + HTTP method + canonical route + contract version.
- Record idempotency dan resource yang dibuat dipersist atomik sebelum response sukses/`202` dikirim.
- Retry transport dengan key dan fingerprint yang sama mengembalikan resource/response yang sama, termasuk `jobId` yang sama.
- Reuse key dengan fingerprint berbeda ditolak `409 IDEMPOTENCY_KEY_REUSED`.
- Automatic retry, `resume`, dan `retry` mempertahankan satu logical `jobId`; setiap percobaan dicatat sebagai `attemptNumber` baru. “Proses ulang” yang memang diminta user adalah command execute baru dengan key baru dan boleh menghasilkan Job ID baru.

### 1.2 Routing berdasarkan execution profile

| Kondisi | Transfer | Processing | Guard |
|---|---|---|---|
| `ServerDependency=None` / resolver memilih local | Tidak ada API file processing | Local application service atau Local Worker | Jangan membuat call server sebagai shortcut implementasi |
| `ServerDependency=Optional`, resolver memilih local | Tidak ada upload | Local | Fallback server tidak boleh diam-diam |
| `ServerDependency=Optional`, resolver memilih server | Preflight lalu one-shot atau session | Immediate atau server background job | Policy check + pemberitahuan/persetujuan user |
| `ServerDependency=Required` | Secure transfer contract | Immediate atau server background job | Koneksi, authz, policy, capacity, consent, dan validation wajib |
| `Amankan ke Akun` | Contract secured-file terpisah | Selective sync | Hanya hasil yang dipilih user; maksimum tiga versi sesuai baseline |

Client tidak mengirim parameter “pilih local/server”. Resolver memilih execution; server memverifikasi lagi agar client tidak dapat mem-bypass policy.

### 1.3 Upload API, job API, dan endpoint minimum

Gunakan `{apiBase}` sampai major-version routing dikunci.

| Endpoint | Tujuan |
|---|---|
| `POST {apiBase}/file-transfers/preflight` | Mengambil effective limit/mode dan menolak dini bila policy/capacity tidak mengizinkan transfer |
| `POST {apiBase}/uploads` | Streamed `multipart/form-data` one-shot untuk file kecil/menengah dan batch kecil terbatas |
| `POST {apiBase}/upload-sessions` | Membuat session resumable untuk satu file besar |
| `GET {apiBase}/upload-sessions/{uploadId}` | Mengambil status, `nextOffset`, expiry, dan validation state |
| `PUT {apiBase}/upload-sessions/{uploadId}/content` | Mengirim raw chunk dengan `Content-Range` dan content digest |
| `POST {apiBase}/upload-sessions/{uploadId}/complete` | Menutup session, memverifikasi ukuran/integritas, lalu memulai validasi yang diwajibkan |
| `DELETE {apiBase}/upload-sessions/{uploadId}` | Abort session secara idempotent dan jadwalkan cleanup |
| `POST {apiBase}/features/{featureId}/execute` | Immediate execution atau membuat background job dari input `uploadId` |
| `GET {apiBase}/jobs/{jobId}` | Persisted current state / REST recovery |
| `POST {apiBase}/jobs/{jobId}/cancel` | Meminta safe cooperative cancellation |
| `POST {apiBase}/jobs/{jobId}/retry` | Coba lagi dari tahap awal yang diperlukan, Job ID tetap |
| `POST {apiBase}/jobs/{jobId}/resume` | Lanjutkan dari checkpoint yang kompatibel, Job ID tetap |
| `GET {apiBase}/jobs/{jobId}/results` | Manifest hasil dan status retention |
| `HEAD/GET {apiBase}/jobs/{jobId}/results/{resultId}/content` | Metadata atau streaming download; `Range` untuk resume |

Exact base route bergantung pada `API-DEC-003`; semantic resource di atas tidak bergantung pada pilihan routing tersebut.

## 2. Upload/download flow

### 2.1 Preflight dan max-size policy mechanism

Client memanggil preflight **setelah** resolver menyatakan server Optional/Required dan sebelum membaca/mengirim isi file. Request hanya membawa metadata minimum:

```json
{
  "featureId": "FEAT-OCR-004",
  "intendedUse": "PROCESSING_INPUT",
  "files": [
    {
      "clientFileId": "local_1",
      "fileName": "scan.pdf",
      "sizeBytes": 734003200,
      "declaredMediaType": "application/pdf"
    }
  ]
}
```

Response memberi effective policy, bukan keputusan user. Berikut pseudo-JSON; placeholder bersudut harus diganti integer hasil policy, bukan dikirim sebagai string:

```json
{
  "decision": "SERVER_TRANSFER_ALLOWED",
  "recommendedMode": "RESUMABLE_SESSION",
  "consentRequired": true,
  "limits": {
    "maxFileBytes": <effective-max-file-bytes>,
    "maxBatchBytes": <effective-max-batch-bytes>,
    "maxFileCount": <effective-max-file-count>,
    "maxChunkBytes": <effective-max-chunk-bytes>,
    "policyVersion": "policy_..."
  },
  "correlationId": "corr_..."
}
```

Implementasi wajib mengembalikan angka effective policy aktual. Limit berasal dari nilai paling ketat antara system safety ceiling, reverse proxy/API limit, enterprise policy, feature profile, role/classification, storage quota, dan capacity saat itu.

### 2.2 Kapan multipart cocok

Gunakan streamed `multipart/form-data` bila:

- satu file atau batch kecil terbatas;
- ukuran di bawah one-shot threshold;
- metadata sederhana perlu dikirim bersama file;
- koneksi cukup stabil dan mengulang seluruh request masih murah;
- validasi awal dapat selesai dalam request budget yang wajar.

Multipart **tetap harus di-stream** langsung ke secure temporary storage; framework tidak boleh melakukan whole-file buffering ke RAM. Batasi jumlah part, ukuran setiap part, total request, panjang header, dan nama field. Untuk mixed batch outcome, gunakan satu response manifest dengan status per file; jangan memakai `207 Multi-Status` sebagai baseline.

### 2.3 Kapan raw streaming lebih cocok

Gunakan raw `application/octet-stream` di dalam upload session bila:

- file besar atau durasi transfer panjang;
- jaringan dapat putus;
- restart dari byte nol mahal;
- gateway/request timeout one-shot berisiko terlampaui;
- byte offset, checksum chunk, dan resume perlu eksplisit.

Baseline disarankan **sequential chunks**, bukan parallel chunks, untuk menekan RAM, temporary fragments, locking, dan kompleksitas satu-developer. Contoh:

```http
PUT {apiBase}/upload-sessions/upl_123/content
Content-Type: application/octet-stream
Content-Length: 8388608
Content-Range: bytes 16777216-25165823/734003200
Content-Digest: sha-256=:base64digest:
```

Server hanya mengakui chunk setelah bytes dan metadata offset/checksum ter-commit. Response:

```http
204 No Content
Upload-Offset: 25165824
```

Retry range yang sudah ter-commit dengan digest sama mengembalikan acknowledgement yang sama. Offset salah, overlap, atau digest berbeda menghasilkan `409`; response memberi `expectedOffset` yang aman. Client mengambil `GET upload-session` sebelum resume. Raw direct-upload tanpa session tidak direkomendasikan untuk file besar karena tidak memberi recovery resource yang stabil.

### 2.4 Apakah upload session perlu

Ya, tetapi **conditional**, bukan untuk setiap file. Session wajib bila ukuran melewati resumable threshold, restart cost tinggi, koneksi tidak stabil, atau policy menetapkannya. Session tidak dibuat untuk operasi local-only dan tidak wajib untuk one-shot kecil.

Minimum upload state:

```text
CREATED → UPLOADING → VALIDATING → READY
                    ↘ FAILED
CREATED/UPLOADING/FAILED → ABORTED
non-final inactive state → EXPIRED
```

Session menyimpan `uploadId`, owner, feature/purpose, total expected bytes, committed offset/ranges, validation state, policy snapshot/version, created/last-activity/expiry time, dan opaque storage key. Physical path tidak pernah menjadi contract.

`complete` mengembalikan `200` bila integrity/validation selesai cepat. Jika deep validation harus background, response `202` wajib membawa validation `jobId`, `correlationId`, status, dan `Location`; upload tetap `VALIDATING` sampai persisted validation job menyatakannya `READY` atau `FAILED`. Feature execution belum boleh dibuat selama upload belum `READY`.

### 2.5 MIME/type validation dan file besar

Jangan percaya extension, filename, atau client `Content-Type` sendiri. Pipeline:

1. Preflight: filename/extension allowlist, declared MIME, declared size, file count, permission/policy, capacity.
2. Saat stream: hard byte counter, bounded signature sniffing, magic/header check, chunk digest, total-size limit; hentikan segera saat limit dilampaui.
3. Setelah staging: MIME/signature consistency, parser/structural validation, corruption, password/encryption, page count, macro, embedded object/executable, decompressed-size ratio/limit.
4. Security inspection/malware scanning bila diwajibkan policy/feature.
5. Hanya status `READY` boleh direferensikan execute job.

Status validasi mengikuti baseline: `READY`, `PASSWORD_REQUIRED`, `WARNING`, `UNSUPPORTED`, `CORRUPTED`, `SECURITY_REVIEW`, `BLOCKED`, `INSUFFICIENT_STORAGE`. File di quarantine tidak dapat diunduh atau dipakai job. Password file dikirim hanya pada trust boundary yang disetujui saat diperlukan, bersifat ephemeral, dan tidak dipersist/log/audit/backup.

Untuk file besar, server stream ke storage sambil menghitung bytes/digest; jangan menahan file penuh di RAM. Deep parse, OCR, dan conversion tidak dijalankan di request upload. Capacity reservation dilakukan sebelum session dan dicek ulang saat complete/execute.

### 2.6 Download dan result retrieval

`GET /jobs/{jobId}/results` mengembalikan manifest, bukan bytes:

```json
{
  "jobId": "job_123",
  "outcome": "SUCCESS",
  "results": [
    {
      "resultId": "res_1",
      "fileName": "hasil.pdf",
      "mediaType": "application/pdf",
      "sizeBytes": 12582912,
      "etag": "\"...\"",
      "state": "AVAILABLE",
      "expiresAt": "2026-08-25T10:00:00Z",
      "contentUrl": "{apiBase}/jobs/job_123/results/res_1/content"
    }
  ],
  "correlationId": "corr_..."
}
```

Content endpoint:

- authorize owner/permission pada setiap request;
- stream storage-to-response tanpa whole-file buffering;
- kirim validated `Content-Type`, safe `Content-Disposition: attachment`, `Content-Length`, `ETag`, `Accept-Ranges: bytes`, dan `X-Content-Type-Options: nosniff`;
- dukung `HEAD`, `Range`, `If-Range`, `206 Partial Content`, dan `416` untuk resume;
- temporary result memakai private/no-store caching sesuai security policy;
- download tidak memberi instruksi untuk menimpa source lokal. Client tetap memakai temp → validate → Save As, kecuali user memilih overwrite secara eksplisit.

Jika result belum siap: `409 RESULT_NOT_READY`. Jika retention habis: persisted manifest menjadi `EXPIRED`, bytes dihapus, dan content endpoint mengembalikan `410 RESULT_EXPIRED`. User dapat menjalankan `resume/retry` hanya bila input/checkpoint masih tersedia; bila tidak, UI meminta upload/proses baru. API tidak otomatis memperpanjang retention atau menyimpan hasil permanen. **Amankan ke Akun** adalah aksi eksplisit terpisah yang memindahkan/menyalin hasil tervalidasi ke secured-file lifecycle.

## 3. Job lifecycle

### 3.1 Create dan response `202 Accepted`

```http
POST {apiBase}/features/FEAT-OCR-004/execute
Idempotency-Key: 7d2e...
X-Correlation-ID: corr_...
Content-Type: application/json
```

```json
{
  "contractVersion": "...",
  "parameterSchemaVersion": "...",
  "inputs": [{ "uploadId": "upl_123" }],
  "parameters": {}
}
```

Server mengulang authz, Feature ID/schema, upload ownership/state, policy, limit, engine, dan capacity check. Untuk background work, server harus:

1. persist logical job, initial attempt, input leases, idempotency record, dan dispatch marker secara konsisten;
2. baru mengembalikan `202 Accepted`;
3. worker mengambil job dari persisted state/dispatch mechanism;
4. publish SignalR hanya setelah state terbaru dipersist.

```http
202 Accepted
Location: {apiBase}/jobs/job_123
Retry-After: 2
```

```json
{
  "jobId": "job_123",
  "correlationId": "corr_...",
  "status": "QUEUED",
  "statusUrl": "{apiBase}/jobs/job_123",
  "resultUrl": "{apiBase}/jobs/job_123/results",
  "cancelUrl": "{apiBase}/jobs/job_123/cancel"
}
```

`202` berarti request telah diterima dan state recovery sudah durable; bukan berarti proses berhasil. API tidak menahan koneksi sampai heavy work selesai. Proses yang benar-benar immediate boleh `200`/`201`; jika threshold terlewati, hasilnya harus `202`, bukan silent long wait.

### 3.2 State dan status resource

State minimum tetap:

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

Transition recommendation:

```text
QUEUED → RUNNING
QUEUED/RUNNING ↔ WAITING_RESOURCE
QUEUED/RUNNING ↔ WAITING_CONNECTION
RUNNING/WAITING_* → RETRYING → QUEUED/RUNNING
non-final → CANCELLED (setelah safe point)
RUNNING → COMPLETED | FAILED
FAILED → RETRYING hanya melalui retry/resume command yang valid
```

`GET /jobs/{jobId}` minimum:

```json
{
  "jobId": "job_123",
  "correlationId": "corr_...",
  "featureId": "FEAT-OCR-004",
  "status": "RUNNING",
  "outcome": null,
  "attemptNumber": 2,
  "progress": {
    "percentage": 62,
    "indeterminate": false,
    "currentStep": "OCR_PAGES",
    "currentItem": 620,
    "completedItems": 619,
    "totalItems": 1000,
    "etaSeconds": 180
  },
  "checkpoint": {
    "supported": true,
    "available": true,
    "lastSavedAt": "2026-08-24T10:00:00Z",
    "stage": "OCR_PAGES",
    "completedItems": 600,
    "totalItems": 1000,
    "resumeAllowed": true
  },
  "cancellation": {
    "requested": false,
    "canCancel": true
  },
  "stateVersion": 17,
  "startedAt": "...",
  "completedAt": null,
  "error": null
}
```

Raw checkpoint bytes, storage key, provider secret, dan internal resume token tidak diekspos. Client hanya melihat capability/action summary. Gunakan `ETag`/`If-None-Match` pada status polling untuk mengurangi network; SignalR event membawa minimal `jobId`, `correlationId`, dan `stateVersion`, lalu REST tetap menjadi recovery/source of truth.

### 3.3 Cancel

- `POST /jobs/{jobId}/cancel` memakai `Idempotency-Key`.
- Job `QUEUED` dapat dibatalkan sinkron dan mengembalikan `200` dengan state `CANCELLED`.
- Job `RUNNING` memakai cooperative cancellation: `202`, `cancellation.requested=true`, Job ID/Location tetap. Worker berhenti pada safe point, memvalidasi tidak ada final output palsu, lalu mempersist `CANCELLED`.
- Repeat cancel key yang sama mengembalikan response yang sama; cancel ulang pada job `CANCELLED` mengembalikan current state `200`.
- Jika tahap tidak aman dibatalkan: `409 JOB_NOT_CANCELLABLE` dengan alasan aman.
- `COMPLETED` atau finalization yang sudah commit tidak diubah diam-diam menjadi cancelled.

### 3.4 Completion, partial outcome, dan result

- `COMPLETED + outcome=SUCCESS`: semua output yang diwajibkan valid.
- Untuk operasi batch yang memang independen dan schema mengizinkan kelanjutan: `COMPLETED + outcome=PARTIAL_SUCCESS`, dengan status/error per item.
- Untuk operasi atomik seperti merge, kegagalan input/tahap menggagalkan job; intermediate file tidak diekspos sebagai final.
- API tidak menentukan sendiri `continueOnItemFailure`. Perilaku harus berasal dari semantics Feature ID/typed parameter schema dan pilihan user bila parameter itu memang diekspos.
- `FAILED` membawa safe `code`, `message`, `correlationId`, `retryable`, dan `resumeAvailable`; tanpa stack trace atau isi file.

## 4. Idempotency model

### 4.1 Command yang wajib memakai key

`Idempotency-Key` wajib untuk:

- one-shot upload;
- create upload session;
- complete upload session;
- feature execute/job create;
- cancel;
- retry;
- resume;
- penyimpanan result melalui **Amankan ke Akun**.

Chunk `PUT` memiliki idempotency natural dari `uploadId + byte range + digest`; header key tambahan tidak wajib. `GET`, `HEAD`, dan `DELETE upload-session` harus idempotent secara semantic.

### 4.2 Persisted record

Minimum record:

```text
Owner/Tenant Scope
IdempotencyKeyHash
Method + Canonical Route + ContractVersion
RequestFingerprint
State: IN_PROGRESS | COMPLETED | FAILED_BEFORE_COMMIT
ResourceType + ResourceId
OriginalStatusCode + SafeResponseSnapshot
OperationCorrelationId
CreatedAt + ExpiresAt
```

Key disimpan sebagai hash bila memungkinkan. Request fingerprint dibentuk dari canonical JSON/manifest, Feature ID, parameter schema version, referenced `uploadId` + immutable version, dan declared content digest bila ada. Untuk multipart one-shot, fingerprint memakai manifest + metadata + content digest; file besar sebaiknya memakai session agar retry tidak perlu mengirim body ulang.

### 4.3 Duplicate request handling

| Kondisi | Response |
|---|---|
| Key baru | Proses normal; buat satu resource |
| Key sama + fingerprint sama + sudah commit | Replay status/body/resource asli; boleh tambahkan `Idempotency-Replayed: true` |
| Key sama + fingerprint sama + masih berjalan dan resource sudah ada | Kembalikan `202` dan `Location` resource yang sama |
| Key sama + fingerprint sama + one-shot masih belum punya resource | `409 IDEMPOTENCY_REQUEST_IN_PROGRESS` + `Retry-After` |
| Key sama + fingerprint berbeda | `409 IDEMPOTENCY_KEY_REUSED`; jangan jalankan request kedua |
| Kegagalan sebelum durable commit | Setelah cleanup/marking konsisten, retry key yang sama boleh melanjutkan/restart satu operation; tidak boleh ada dua resource aktif |

Operation `correlationId` asli tetap stabil pada replay. Transport attempt boleh mempunyai internal trace/span baru, tetapi tidak mengganti correlation identity logical operation.

Content hash **bukan** pengganti user intent. Dua request dengan key berbeda dianggap dua aksi user yang berbeda dan tidak boleh digabung diam-diam hanya karena file/parameter sama. Storage dedup internal, bila kelak digunakan, tidak boleh melintasi security boundary atau mengubah ownership/lifecycle.

## 5. Retry/resume model

### 5.1 Tiga lapis recovery

1. **HTTP/transport retry** — ulangi request dengan key yang sama; hasil/resource tetap sama.
2. **Transfer resume** — session yang sama melanjutkan dari committed offset; download melanjutkan dengan `Range` + `If-Range`.
3. **Processing retry/resume** — logical Job ID tetap; attempt history bertambah dan diaudit.

### 5.2 Processing attempt

- Automatic retry hanya untuk error transient yang allowlisted, dengan bounded attempts, exponential backoff + jitter, dan `Retry-After` bila tersedia.
- Saat automatic retry, status `RETRYING`; Job ID tidak berubah.
- `POST /resume` memakai checkpoint terakhir yang valid dan kompatibel.
- `POST /retry` mengulang hanya stage yang perlu atau dari awal stage aman tanpa menghilangkan parameter/edit user.
- Setiap action memakai key baru untuk satu intent user; retransmission action yang sama memakai key yang sama sehingga tidak menambah attempt ganda.
- Attempt history bersifat append-only: attempt number, cause, checkpoint reference/version, engine/provider version, started/completed time, outcome, safe error code.
- Jangan retry validation, unsupported MIME, authz, policy block, malware block, atau deterministic corrupt input.
- Jangan fallback provider kecuali Engine Resolver/policy sudah menyetujuinya; fallback tidak boleh mengubah output semantics diam-diam.

### 5.3 Checkpoint contract

Checkpoint server wajib:

- commit atomik dengan stage progress/output fragment yang diakuinya;
- mempunyai engine/provider version, feature/parameter schema version, input identity/version, stage, dan integrity marker;
- hanya digunakan jika replay stage idempotent atau mempunyai compensation yang aman;
- dipertahankan hanya selama recovery retention/lease;
- diekspos ke client sebagai summary, bukan file/token internal.

Jika checkpoint tidak kompatibel atau sudah expired, `/resume` mengembalikan `409 CHECKPOINT_NOT_RESUMABLE` atau `410 CHECKPOINT_EXPIRED`. Response boleh menyatakan `retryFromStartAllowed=true`; server tidak otomatis memilih “Coba Lagi” untuk user.

### 5.4 Timeout

- Upload memiliki connect/header timeout, idle-body timeout, dan session inactivity expiry yang berbeda. Timeout chunk tidak menghapus session; client membaca committed offset lalu resume.
- Execute background tidak menunggu engine. Setelah durable acceptance, response `202`; client menggunakan SignalR + REST.
- Provider/stage timeout dibatasi per feature/policy. Job masuk `RETRYING`, `WAITING_RESOURCE`, atau `FAILED` sesuai klasifikasi; source/input tetap aman.
- Jika gateway timeout terjadi sebelum client mengetahui acceptance, client mengulang **key yang sama**. Server me-replay Job ID/resource bila sudah commit.
- Job tidak dibatalkan hanya karena client disconnect atau SignalR putus.

## 6. HTTP status recommendation

| Status | Penggunaan |
|---:|---|
| `200 OK` | Status/manifest; immediate result; action idempotent yang sudah selesai |
| `201 Created` | Upload/session/resource berhasil dibuat; sertakan `Location` |
| `202 Accepted` | Background job, deep upload validation, cooperative cancel, retry, atau resume sudah durable tetapi belum selesai; sertakan Job ID, Correlation ID, status, `Location`, dan bila berguna `Retry-After` |
| `204 No Content` | Chunk ter-commit atau abort/delete idempotent tanpa body |
| `206 Partial Content` | Download range/resume |
| `304 Not Modified` | Conditional job-status polling dengan ETag |
| `400 Bad Request` | JSON/header/range malformed, field wajib hilang |
| `401 Unauthorized` | Authentication tidak ada/tidak valid |
| `403 Forbidden` | Permission, classification, atau enterprise policy melarang aksi |
| `404 Not Found` | Resource tidak ada atau disembunyikan dari principal yang tidak berhak |
| `408 Request Timeout` | Request/upload one-shot tidak selesai dalam request timeout |
| `409 Conflict` | State conflict, wrong upload offset, result belum siap, checkpoint tidak resumable, job tidak cancellable, atau idempotency conflict |
| `410 Gone` | Upload session/result/checkpoint diketahui owner tetapi sudah expired/dihapus sesuai retention |
| `412 Precondition Failed` | `If-Match`/ETag atau immutable version tidak lagi cocok |
| `413 Content Too Large` | File, chunk, batch, atau request melampaui effective byte limit |
| `415 Unsupported Media Type` | Media type/signature tidak didukung atau tidak konsisten |
| `416 Range Not Satisfiable` | Download range di luar file |
| `422 Unprocessable Content` | Payload syntactically valid tetapi file/structure/parameter validation gagal |
| `429 Too Many Requests` | Per-user/tenant rate, quota, session, atau concurrent-job limit; sertakan `Retry-After` bila transient |
| `500 Internal Server Error` | Error tak terduga dengan standard safe envelope |
| `502 Bad Gateway` | Dependency/provider upstream gagal secara immediate dan request belum menjadi background job |
| `503 Service Unavailable` | Global saturation/readiness/capacity sementara; sertakan `Retry-After` |
| `504 Gateway Timeout` | Upstream timed out sebelum durable background acceptance |
| `507 Insufficient Storage` | Server temp/object storage tidak cukup setelah safe preflight; gunakan bila status ini disetujui pada standard API final |

Semua error memakai envelope `code`, `message`, `correlationId`, dan optional safe structured `details`; tanpa stack trace.

## 7. Security controls

- HTTPS wajib; server-stored payload encrypted at rest sesuai policy.
- Authz dilakukan pada create, chunk, complete, execute, status, cancel/retry/resume, manifest, dan setiap content download—not only pada UI.
- `uploadId`, `jobId`, dan `resultId` opaque/unpredictable; ownership/permission tetap sumber otorisasi, bukan kerahasiaan ID.
- Storage key dibuat server, tidak berasal dari filename/path user. Sanitasi filename untuk display/`Content-Disposition`; blok path traversal, alternate stream, control character, dan header injection.
- Treat file content as untrusted: allowlist, magic signature, structural/decompression controls, malware/security inspection, quarantine.
- Temporary inputs/results diisolasi per owner/tenant/job purpose dan tidak dapat di-list/download lintas permission.
- Short-lived download ticket/direct object URL hanya boleh dipakai bila topology membutuhkannya; harus scoped ke satu result, expiry singkat, HTTPS, dan diterbitkan setelah authz. Baseline API streaming tetap aman tanpa permanent URL.
- User harus melihat pemberitahuan dan melakukan aksi eksplisit sebelum upload. Simpan metadata consent/purpose secukupnya; jangan menyimpan isi dokumen sebagai audit evidence.
- Jangan log/telemetry body, file bytes, full OCR text, password, token, signature asset, clipboard, raw checkpoint, atau storage key. Filename/classification juga diminimalkan pada operational telemetry.
- Password file bersifat ephemeral dan tidak masuk idempotency snapshot.
- Temp/result response memakai safe content headers dan tidak inline-render file aktif secara default.
- Client source tidak pernah ditimpa otomatis; server response tidak dapat mengubah local source path.

Exact authentication scheme/claim/download-ticket boundary menunggu `API-DEC-007`.

## 8. Server-resource controls

1. **Reject early:** preflight memeriksa byte limit, storage reservation/quota, queue/concurrency, feature permission, dan service readiness sebelum transfer besar.
2. **Stream end-to-end:** request → bounded buffer → temp/object storage; tidak ada whole-file RAM load.
3. **Bounded sessions:** limit active sessions per user/tenant, chunk size, file count, batch bytes, idle lifetime, dan upload bandwidth.
4. **Sequential chunk baseline:** mencegah parallel fragment/storage explosion; bounded parallelism baru setelah benchmark.
5. **Capacity-weighted jobs:** concurrency dibatasi berdasarkan `ConcurrencyRisk`, CPU/RAM/storage/network weight, bukan hanya jumlah job.
6. **Bounded queue:** jangan menerima job tanpa batas. Sebelum job dibuat, gunakan `429/503/507`; setelah job durable, gunakan `WAITING_RESOURCE` dan biarkan user cancel.
7. **Leases/reservations:** upload/result/checkpoint yang sedang dipakai job mempunyai lease; cleanup tidak boleh menghapus resource aktif.
8. **Backoff:** client polling menghormati `Retry-After`, ETag, exponential backoff, dan SignalR; hindari polling rapat.
9. **Low-cost cleanup:** batch kecil, low priority, idempotent; object-storage lifecycle menjadi defense-in-depth, bukan satu-satunya source of truth.
10. **Metrics metadata-only:** bytes transferred, duration, failure code, queue length, worker utilization, cleanup lag, retry count; tanpa content.

Backpressure error minimum memberi safe `limitScope`, `retryAfterSeconds`, dan stable error code. Jangan mengekspos topology/server detail. Exact quota, thresholds, dan header standardisasi menunggu `API-DEC-009` dan capacity test.

## 9. Failure/recovery cases

| Kasus | Contract recovery |
|---|---|
| Resolver memilih local | Tidak upload; proses local tetap berjalan |
| Network putus sebelum upload | Tidak ada server state; user/client dapat lanjut local atau menunggu sesuai capability |
| One-shot putus | Retry key yang sama; bila transfer mahal, beralih hanya melalui flow/session baru yang terlihat, bukan keputusan diam-diam |
| Chunk putus | `GET upload-session` → resume dari committed offset; chunk duplicate digest sama di-ack |
| Session expired | `410 UPLOAD_SESSION_EXPIRED`; buat session/upload baru setelah user tetap mengizinkan transfer |
| MIME/signature mismatch | `415`; quarantine/delete temp sesuai policy; job tidak dibuat |
| Structural validation gagal | `422` + per-file validation status; file lain dalam batch tetap independen |
| Malware/security block | `422/403` stable code; quarantine access-restricted; no automatic retry |
| Storage penuh sebelum accept | `507` atau `503` + `Retry-After`; jangan menerima bytes/job |
| Queue penuh sebelum job create | `429` untuk scoped quota atau `503` global; same key aman untuk retry |
| Resource habis setelah `202` | Job `WAITING_RESOURCE`; progress/status jelas; user dapat cancel |
| Client/SignalR disconnect setelah `202` | Job lanjut; reconnect lalu REST GET current state |
| Worker crash | Persisted state/checkpoint digunakan; bounded automatic retry dengan Job ID sama |
| Provider timeout/transient | `RETRYING` dengan backoff; `FAILED` setelah budget habis |
| Invalid deterministic input | `FAILED`, `retryable=false`; jangan loop retry |
| User cancel saat running | `202` cancel requested → safe point → `CANCELLED`; temp non-final dibersihkan |
| Cancel terlambat/tidak aman | `409`; status asli tetap source of truth |
| Checkpoint corrupt/incompatible | Resume ditolak; tawarkan retry bila input masih ada, tanpa silent restart |
| Partial batch failure | Per-item outcome; `PARTIAL_SUCCESS` hanya untuk feature non-atomic yang mengizinkan |
| Result download putus | `Range` + `If-Range`; ETag memastikan bytes berasal dari versi sama |
| Result expired | Manifest `EXPIRED`, content `410`; reprocess/re-upload bila recovery artifacts juga habis |
| Cleanup crash | Idempotent sweeper melanjutkan dari persisted lifecycle state; orphan reconciliation |

### Cleanup/retention lifecycle

- Incomplete upload: expiry setelah inactivity; aborted/failed temp dihapus secepat aman.
- `READY` upload: leased sampai job terminal dan recovery window berakhir.
- Intermediate: dihapus setelah validation/finalization atau terminal failure/cancel, kecuali checkpoint masih leased untuk recovery.
- Result: tersedia hanya selama download retention; expiry timestamp selalu terlihat sebelum download.
- Setelah expiry: bytes dihapus, minimal job/result metadata dan audit yang diwajibkan dapat tetap ada dengan state `EXPIRED`; content tidak dipertahankan tanpa alasan.
- **Amankan ke Akun**: copy/promote hasil tervalidasi hanya atas aksi user ke lifecycle secured file terpisah; maksimum tiga versi. Mengamankan hasil tidak mengubah draft/cache/temp menjadi cloud workspace.
- Cleanup memakai lifecycle state + lease/ref-count, tombstone, retry, reconciliation, dan safe delete. Jangan menghapus blob berdasarkan umur saja bila masih direferensikan job aktif.

## 10. Open decisions

Nilai berikut sengaja tidak di-hardcode karena baseline menyatakan perlu PoC/benchmark/security/capacity test:

- max file/batch/chunk bytes dan one-shot/resumable threshold;
- maksimum file per multipart batch;
- concurrent upload session/job per user/tenant/feature;
- queue capacity dan CPU/RAM/storage/network weights;
- connect/header/idle/provider/job timeouts;
- automatic retry count/backoff ceiling;
- upload inactivity TTL, checkpoint recovery window, result retention, idempotency retention;
- final malware scanner, secure file/object storage implementation, dan download-ticket topology.

Dependency keputusan:

`BLOCKED_BY_DECISION: API-DEC-003` — exact `{apiBase}` dan major-version route.

`BLOCKED_BY_DECISION: API-DEC-007` — exact authentication scheme, claims, delegated access, serta download-ticket authorization boundary.

`BLOCKED_BY_DECISION: API-DEC-008` — exact SignalR event envelope, event ordering/sequence, reconnect cursor, dan replay behavior. REST job resource/stateVersion di laporan ini tetap dapat dikunci lebih dahulu.

`BLOCKED_BY_DECISION: API-DEC-009` — exact quota/rate/concurrency values, standardized backpressure headers, dan limit scope. Rekomendasi status `429/503/507` tetap dapat menjadi baseline.

Tidak blocked oleh API-DEC-002: semantics ini dapat didokumentasikan pada OpenAPI, baik strategi contract akhirnya governed hybrid maupun opsi lain. Source package discrepancy v0.1/v0.2 juga tidak mengubah rekomendasi; v0.2 adalah working file yang tersedia dan lebih baru.

## 11. Suggested Document 06 text

### API-DEC-004 — File Transfer Strategy

**Status:** RECOMMENDED — Hybrid File Transfer Contract

File local-only tidak di-upload. Untuk `ServerDependency=Optional`, upload hanya dilakukan jika Engine Resolver memilih server, policy/capability mengizinkan, dan user menyetujui transfer. Untuk `Required`, client memakai secure transfer contract dan server mengulang authz, validation, limit, storage, serta capacity check.

File kecil/menengah menggunakan streamed `multipart/form-data` one-shot. Multipart tidak boleh di-buffer penuh ke memory. File besar, koneksi tidak stabil, atau restart cost tinggi menggunakan upload session resumable dengan raw chunk `application/octet-stream`, `Content-Range`, content digest, persisted committed offset, expiry, dan abort. Baseline chunk bersifat sequential dan bounded.

Upload memakai allowlist, extension/MIME/signature consistency, byte limit saat stream, structural/decompression validation, dan security inspection bila diwajibkan. Hanya `uploadId` berstatus `READY` dapat dipakai `POST /features/{featureId}/execute`.

Download hasil menggunakan result manifest dan authorized streaming content endpoint. `Range`, `ETag`, dan `If-Range` mendukung resume. Result processing adalah temporary/retained terbatas; expiry menghasilkan `410 RESULT_EXPIRED`. Hanya aksi eksplisit **Amankan ke Akun** memindahkan/menyalin hasil tervalidasi ke secured account storage. Source lokal tidak ditimpa otomatis.

Server temp lifecycle menggunakan persisted state, expiry, lease/ref-count, idempotent cleanup, dan orphan reconciliation. Input, intermediate, checkpoint, dan result mempunyai retention berbeda. Exact limit/TTL ditetapkan lewat policy dan capacity/security test, bukan hardcode contract.

**Traceability:** `FR-FILE-010..015`; `FR-PROC-001..005`; `FR-STOR-001..006`; `FR-BACKUP-001..010`; `FR-REC-001..006`; `NFR-PERF-003,007..010`; `SR-SEC-001..008`; `IR-API-003..008`; `FEAT-VALID-001..018`; `FEAT-STOR-011..016`; `FEAT-REC-008..009`; `FEAT-SEC-004..012`; Solution Design sections 9, 11–13, 17, 24, 26–28.

### API-DEC-005 — Idempotency & Retry Contract

**Status:** RECOMMENDED — Persisted Idempotency Record + Request Fingerprint

Mutating command yang dapat menciptakan upload, session, job, attempt, cancel, resume, retry, atau secured copy wajib memakai `Idempotency-Key`. Scope key adalah authenticated principal/tenant + method + canonical route + contract version. Server menyimpan request fingerprint, operation correlation ID, resource ID, dan safe response snapshot.

Key/fingerprint sama mengembalikan resource dan response yang sama. Create-job retry tidak membuat Job ID ganda. Key sama dengan fingerprint berbeda menghasilkan `409 IDEMPOTENCY_KEY_REUSED`. Key berbeda adalah intent user baru dan tidak digabung hanya berdasarkan content hash.

Background create mengembalikan `202 Accepted` hanya setelah job/idempotency/input lease/dispatch state durable. Response memuat `jobId`, `correlationId`, status, dan `Location`. Persisted job state adalah source of truth; SignalR hanya notification dan REST memulihkan state setelah reconnect.

Automatic retry, explicit retry, dan resume mempertahankan logical Job ID serta menambah append-only attempt history. Resume hanya memakai checkpoint yang integrity/version-compatible. Jika resume tidak tersedia, API tidak diam-diam retry dari awal; user mendapat pilihan `Lanjutkan`, `Coba Lagi`, atau `Batalkan`. Retry dibatasi, hanya untuk error transient allowlisted, dan memakai exponential backoff + jitter.

Job status minimum adalah `QUEUED`, `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, dan `CANCELLED`. Status memperlihatkan safe progress, attempt, cancellation, checkpoint summary, error code, timestamps, dan monotonic `stateVersion`; raw checkpoint/internal token tidak diekspos.

**Traceability:** `FR-REC-001..006`; `UIR-UI-003..004`; `NFR-REC-001..005`; `NFR-PERF-009..010`; `IR-API-004..008`; `FEAT-REC-001..015`; `FEAT-PROG-001..008`; `FEAT-MON-004..007,013`; Solution Design sections 11–13, 21, 24, 27–28; `API-AUTO-001..004`.
