# AGENT C — SECURITY, QUERY & GOVERNANCE REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC06-AGENT-C` |
| Fokus | `API-DEC-006`, `API-DEC-007`, `API-DEC-009` |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 24 Agustus 2026 |
| Pemilik keputusan | User + Integrator |

## Catatan sumber dan batas laporan

Laporan ini diturunkan dari:

- `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`;
- `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md`;
- `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md`;
- `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md`;
- `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md`;
- `06_API_DOCUMENTATION_WORKING_v0.2_PERTAMINA_OTO.md`, yaitu versi working yang tersedia di workspace. Prompt menyebut v0.1, tetapi file v0.1 tidak tersedia.

Laporan ini tidak mengubah Role Matrix, tidak membuat role baru, tidak mengunci nilai capacity sebelum benchmark, dan tidak mengubah master Document 06. Kata **MUST**, **SHOULD**, dan **MAY** pada bagian rekomendasi menyatakan kekuatan usulan, bukan keputusan final.

Traceability utama: `FR-AUTH-001..005`, `FR-ROLE-001..003`, `FR-HIST-001..003`, `FR-AUDIT-001..004`, `FR-SUP-001..007`, `NFR-PERF-007..013`, `SR-SEC-001..023`, `IR-API-002..012`, `FEAT-AUTH-001..012`, `FEAT-HIST-013..015`, `FEAT-AUDIT-002..008`, `FEAT-SEC-008..009`, `FEAT-REC-014..015`, `FEAT-SUPPORT-001..008`, dan Solution Design bagian 11.4, 19, 22.6, 26, serta 27.

---

## 1. Pagination recommendation

### 1.1 Rekomendasi keputusan

Gunakan **satu List Query Standard dengan dua mode pagination**:

- `page` + `pageSize` untuk daftar metadata biasa yang kecil, relatif stabil, dan membutuhkan navigasi halaman;
- `cursor` + `pageSize` untuk daftar yang besar, terus bertambah, sensitif terhadap perubahan, atau berbiaya tinggi.

Satu endpoint MUST menetapkan satu mode utama. Client tidak boleh mengirim `page` dan `cursor` bersamaan. Mode tidak dipilih bebas oleh client karena karakteristik konsistensi dan biaya ditentukan oleh resource.

### 1.2 Nilai standar

| Aturan | Rekomendasi |
|---|---|
| Nomor halaman | 1-based; default `page=1` |
| Default umum | `pageSize=25` |
| Maximum umum | `pageSize=100` |
| History/job activity | default `25`, maximum `50`, memenuhi `FR-HIST-003` yang membatasi pemuatan awal maksimal 50 item |
| Nilai nol, negatif, non-integer, atau di atas maximum | `400 INVALID_PAGINATION`; tidak di-clamp diam-diam |
| Page melewati data terakhir | `200` dengan `items=[]` |
| Cursor habis | `nextCursor=null`, `hasNext=false` |
| Exact total | hanya untuk page-based list bila murah dan sudah dibatasi authorization; tidak wajib pada cursor list |

Maximum adalah batas kontrak API. Administrator MAY menurunkan effective maximum melalui policy, tetapi tidak boleh menaikkannya melewati system safety maximum tanpa controlled revision dan capacity evidence.

### 1.3 Profil per jenis daftar

| Jenis daftar | Mode | Default / maximum | Default stable order | Catatan |
|---|---|---:|---|---|
| Ordinary metadata/configuration list | Page | 25 / 100 | resource-defined field + unique ID | Gunakan hanya untuk set kecil/terbatas. Bila churn atau volume menjadi tinggi, endpoint direvisi secara versioned ke cursor. |
| Audit/event list | Cursor | 50 / 100 | `occurredAt desc, auditId desc` | Append-oriented; exact total tidak dikembalikan secara default. Wide export menjadi background job. |
| Issue list | Cursor | 25 / 100 | `createdAt desc, issueId desc` | Scope user sendiri atau support permission. Perubahan status tidak boleh mengubah default order. |
| Secured-file list | Cursor | 25 / 100 | `securedAt desc, securedFileId desc` | Ownership/permission diterapkan sebelum pagination; tidak ada content search. |
| Large/history/job list | Cursor | 25 / 50 | `completedAt desc, historyId/jobId desc` | Initial load tidak boleh melebihi 50. Arsip besar diekspor asynchronous bila diizinkan. |

Nama timestamp final mengikuti model resource, tetapi semantik dan tie-breaker unik MUST dipertahankan.

### 1.4 Cursor dan continuation behavior

Cursor MUST:

- opaque, integrity-protected, dan tidak berisi raw SQL, storage key, token, secret, atau data sensitif yang dapat dibaca client;
- terikat pada subject/caller scope, endpoint, filter, search, date range, sort, dan arah traversal;
- menggunakan keyset continuation dari stable sort, bukan offset tersembunyi;
- mempunyai expiry configurable;
- di-authorize ulang pada setiap request; cursor bukan bukti permission;
- tidak dapat digunakan untuk memperluas scope setelah role, permission, ownership, atau policy berubah.

Perilaku continuation:

- insert baru sebelum posisi cursor tidak menyebabkan item lama berulang;
- delete atau pencabutan akses dapat menyebabkan item tidak lagi muncul, tetapi tidak boleh membuka data lain;
- perubahan filter/search/sort/date range memulai query baru tanpa cursor lama;
- cursor rusak/tampered menghasilkan `400 INVALID_CURSOR`;
- cursor valid tetapi expired menghasilkan `410 CURSOR_EXPIRED` dan client memulai ulang dari awal;
- untuk laporan yang membutuhkan snapshot konsisten, gunakan asynchronous export/snapshot job, bukan memaksa live list menjadi transaction snapshot panjang.

Response envelope yang direkomendasikan:

```json
{
  "items": [],
  "pageInfo": {
    "mode": "cursor",
    "pageSize": 25,
    "hasNext": true,
    "nextCursor": "opaque-value"
  }
}
```

Untuk page mode, `pageInfo` berisi `mode`, `page`, `pageSize`, `hasNext`, dan `totalCount` hanya bila penghitungannya aman serta murah.

---

## 2. Filter/sort standard

### 2.1 Bentuk query

```http
GET /api/issues?pageSize=25&cursor=...&filter[status]=BARU&filter[status]=DIPERIKSA&sort=-createdAt,issueId&search=ERR-102&from=2026-08-01T00:00:00Z&to=2026-09-01T00:00:00Z
```

Standard parameter:

| Parameter | Semantik |
|---|---|
| `page` | nomor page 1-based untuk page mode |
| `pageSize` | jumlah item yang diminta, tunduk pada profile maximum |
| `cursor` | continuation opaque untuk cursor mode |
| `sort` | daftar field allowlisted; prefix `-` berarti descending |
| `filter[field]` | filter exact/enum/ID yang allowlisted |
| `search` | pencarian metadata pada field yang didokumentasikan per resource |
| `from`, `to` | rentang waktu RFC 3339 UTC; interval half-open `[from,to)` |

### 2.2 Aturan filter

- Semua field dan operator MUST menggunakan allowlist per endpoint. Arbitrary field path, raw expression, regex, SQL fragment, dan client-defined column name dilarang.
- Nilai berulang pada field yang sama berarti **OR**. Field berbeda berarti **AND**.
- Enum harus menggunakan canonical value. ID harus divalidasi formatnya sebelum query.
- Filter authorization seperti owner, unit, support scope, atau allowed resource set ditambahkan server-side dan tidak dapat dihapus/ditimpa client.
- Backend MUST memakai parameterized query/ORM expression yang aman. Client tidak pernah mengirim SQL.
- Jumlah filter, jumlah nilai per filter, panjang nilai, dan query execution time mempunyai system safety limit configurable.
- Invalid/unsupported filter menghasilkan `400 INVALID_FILTER`; field tersembunyi tidak perlu disebutkan secara rinci kepada client.

### 2.3 Aturan search

- `search` hanya mencari metadata yang dinyatakan aman pada kontrak endpoint.
- Search tidak berarti document full-text search, OCR text search, screenshot search, log search, atau storage scan.
- Wildcard/regex dari client tidak didukung. Normalization, collation, minimum/maximum length, dan prefix/contains behavior didokumentasikan per resource.
- Pencarian secured-file dibatasi ke metadata yang user memang boleh lihat. Isi file dan OCR tidak diindeks untuk endpoint list.
- Pencarian issue secara default dibatasi ke safe identifiers/metadata, misalnya issue ID, Error Code, Job ID, atau Correlation ID. Full-text pesan/screenshot memerlukan keputusan security dan permission terpisah.

### 2.4 Aturan sort dan stable ordering

- `sort` hanya menerima field yang allowlisted dan memiliki jalur query/index yang disetujui.
- Server selalu menambahkan unique tie-breaker walaupun client tidak mengirimkannya.
- Default sort tidak menggunakan field mutable seperti issue status atau priority.
- Unsupported sort menghasilkan `400 INVALID_SORT`.
- Null ordering ditetapkan per field dan konsisten; tidak bergantung pada perilaku database implicit.
- Authorization dan ownership filtering dilakukan sebelum item dan count dibentuk.

### 2.5 Date range

- `from` inclusive dan `to` exclusive dalam UTC, sehingga page/cursor antarperiode tidak overlap.
- Jika hanya `from` atau `to` dikirim, endpoint mengikuti bounded default/range policy yang didokumentasikan.
- Maximum date span berbeda menurut kategori dan bersifat configuration/policy-driven. Audit range yang terlalu lebar diarahkan ke export job.
- Timestamp invalid, `from >= to`, atau span melewati limit menghasilkan `400 INVALID_DATE_RANGE` dengan pesan aman.

### 2.6 Filter/search per resource

| Resource | Scope dan field rekomendasi |
|---|---|
| Ordinary metadata | hanya metadata bisnis yang memang ditampilkan; daftar final per endpoint |
| Audit/event | user/unit bila diizinkan, Feature ID, event type, status, platform, processing mode, Error Code, dan waktu; tidak mencari isi dokumen |
| Issue | status, category, priority, app version, platform, waktu, dan safe identifiers; reporter hanya untuk support permission |
| Secured file | type/MIME, status, secured time, version, dan nama metadata bila policy mengizinkan; owner selalu server-enforced |
| History/job | Feature ID, status, local/server mode, waktu, dan metadata file milik user; tidak mencari document content |

---

## 3. Authentication boundary

### 3.1 Trust flow

```text
Client
  ↓ HTTPS + approved interactive sign-in
Pertamina Identity
  ↓ access token / approved session assertion
API authentication
  ↓ validated principal
Permission + ownership + enterprise policy
  ↓ authorized application command/query
Application service
```

Setiap panah adalah trust boundary. UI state, hidden button, client claim cache, cursor, object ID, Feature ID, dan file metadata dari client bukan bukti authorization.

### 3.2 Pertamina Identity dan API token validation

API MUST memvalidasi sekurang-kurangnya:

- signature dengan key/trust material yang disetujui;
- allowed algorithm;
- issuer;
- audience/resource;
- `exp`, `nbf`, dan waktu dengan clock-skew terbatas;
- subject/account identifier;
- tenant/organization boundary bila Pertamina Identity menggunakannya;
- session/revocation state bila protocol dan policy mendukung;
- claim type/version yang diharapkan.

API tidak boleh mempercayai role/permission buatan client. Claims identity dipetakan ke account internal, lalu permission dan policy efektif diperoleh dari backend. Token tidak masuk URL, log, telemetry, issue diagnostics, atau error response.

### 3.3 Token acquisition dan refresh boundary

- Interactive login dan refresh adalah boundary antara client dan Pertamina Identity melalui library/flow yang disetujui.
- Native Desktop/Mobile SHOULD menggunakan authorization flow dengan PKCE dan OS secure storage.
- Web SHOULD menjaga token di server-side session/BFF-style boundary bila sesuai arsitektur Web MVC; browser-accessible storage tidak digunakan untuk menyimpan bearer/refresh token.
- API resource tidak menerima refresh token dan tidak bertindak sebagai identity provider.
- Client boleh mencoba refresh satu kali setelah authentication failure yang memang recoverable. Request mutating hanya boleh direplay bila aman menurut idempotency/retry contract `API-DEC-005`; tidak ada refresh/retry loop tanpa batas.

Detail protocol, issuer, claim mapping, dan refresh mechanism tetap mengikuti standar Pertamina Identity yang disetujui.

### 3.4 Offline session implications

- Offline session hanya mengizinkan fitur lokal selama cached session/claim/permission masih valid menurut policy.
- Cache permission harus dilindungi, time-bounded, dan deny-by-default bila rusak, expired, atau tidak dapat diverifikasi.
- Server-required action, secured-file access, role/permission administration, support attachment access, audit export, dan aksi yang membutuhkan authorization terkini ditunda sampai online.
- Revocation tidak selalu dapat diketahui saat perangkat offline; risikonya dibatasi dengan offline TTL, narrow local permission, dan sensitive action online check.
- Setelah reconnect, client refresh identity dan permission sebelum mengirim queued command. Queue offline tidak membawa hak akses permanen.
- Audit metadata offline dapat diantrikan secara aman sesuai baseline, tanpa token atau document content.

### 3.5 Sensitive reauthorization

Sensitive reauthorization/step-up SHOULD diminta untuk operasi yang ditetapkan policy, misalnya perubahan role/permission/security policy, penghapusan permanen server data, akses/export support yang sangat sensitif, atau tindakan lain dengan dampak tinggi. Reauthorization:

- membuktikan authentication recency/assurance;
- tidak menggantikan permission, ownership, atau policy check;
- dilakukan melalui Pertamina Identity, bukan dengan meminta password ke PERTAMINA OTO;
- menghasilkan token/session assurance yang tervalidasi API;
- tidak menyimpan password atau challenge response pada aplikasi.

Daftar aksi final harus diselaraskan dengan Security Design dan Role Matrix.

### 3.6 Service-to-service dan Worker/internal API trust

- API, Server Worker, storage adapter, dan approved internal provider memakai service identity terpisah dengan short-lived audience-scoped credential atau certificate/managed identity yang disetujui.
- Shared long-lived secret dan hardcoded secret dilarang. Secret/certificate reference berasal dari approved secret store/configuration boundary.
- Internal network location, queue membership, atau proses bernama `Worker` tidak otomatis dipercaya.
- End-user bearer/refresh token tidak diteruskan ke Worker atau AI/OCR/conversion provider.
- Job menyimpan safe subject reference, ownership, permission/policy version, Feature ID, dan approved storage reference. Dispatcher/Worker memverifikasi job state dan policy sebelum akses; perubahan permission/policy selama antrean dapat menggagalkan atau menahan job secara aman.
- Worker hanya memperoleh akses minimum untuk job tersebut dan tidak dapat melakukan arbitrary SQL, arbitrary storage traversal, atau arbitrary API call.
- Local Worker memakai versioned typed local IPC, tidak membuka public network endpoint, dan tetap menganggap file/document content sebagai untrusted input.
- Provider AI/OCR/conversion hanya provider local atau enterprise-internal yang approved. Tidak ada failover ke public AI endpoint.

---

## 4. Authorization boundary

Authorization MUST bersifat **deny-by-default, permission-based, object-aware, dan server-enforced**.

Urutan minimum setiap command/query:

```text
Authenticated principal
→ account/session eligibility
→ required permission
→ ownership/resource scope
→ enterprise policy/classification
→ feature/platform/provider availability
→ application invariant
→ execute or deny
```

Aturan:

- Role hanya pengelompokan permission; nama role bukan authorization check final.
- Role awal tetap `User`, `Administrator`, dan `Auditor`. Laporan ini tidak membuat role baru.
- Label **Superadmin/Support** pada source diperlakukan sebagai support surface yang membutuhkan permission khusus sampai Role Matrix menetapkan mapping resminya. Label tersebut bukan bypass dan bukan bukti akses.
- Permission diperiksa pada application boundary untuk list, detail, create, update, delete, download, attachment preview, export, retry, cancel, dan admin action.
- List filter bukan security control. Query selalu diawali dengan server-side accessible scope.
- Object-level authorization menggunakan owner/resource relation aktual; mengganti ID pada URL tidak boleh membuka resource lain.
- Administrator tidak otomatis boleh membaca file user, password PDF, token, clipboard, signature asset, atau OCR text lengkap.
- Support permission dapat memberi akses ke issue dan attachment yang user kirim eksplisit, tetapi tidak memberi akses ke dokumen user di luar attachment tersebut.
- Issue list, detail, attachment, triage, internal note, dan export SHOULD memiliki permission terpisah; exact permission names ditetapkan di Role Matrix, bukan Document 06.
- Storage key tidak dikirim sebagai authority. Download/preview menggunakan API authorization atau short-lived scoped access mechanism yang tidak memperluas permission.
- Setiap continuation request, export, retry, dan background execution di-authorize ulang pada boundary yang sesuai.
- Perubahan role/permission/policy dan sensitive support/admin access menghasilkan audit metadata aman.

---

## 5. 401/403 behavior

| Status | Digunakan ketika | Perilaku client |
|---:|---|---|
| `401 Unauthorized` | tidak ada credential yang dapat dipakai; token malformed, expired, revoked, wrong issuer/audience; session tidak valid; atau step-up authentication diwajibkan | login/refresh/reauth melalui Pertamina Identity; maksimal satu automatic recovery attempt |
| `403 Forbidden` | identity valid, tetapi permission, ownership, policy, classification, feature policy, atau current account state melarang aksi | jangan refresh berulang; tampilkan pesan akses ditolak yang aman |
| `404 Not Found` | resource sensitif tidak boleh diungkap keberadaannya kepada caller yang tidak mempunyai discovery access | perlakukan sebagai tidak ditemukan; kebijakan concealment harus konsisten per resource |

Rekomendasi error code:

- `AUTHENTICATION_REQUIRED` atau `TOKEN_INVALID` untuk 401 biasa;
- `REAUTHENTICATION_REQUIRED` untuk step-up yang aman diungkap;
- `ACCESS_DENIED` atau `POLICY_DENIED` untuk 403;
- `RESOURCE_NOT_FOUND` untuk concealment 404.

Contoh aman:

```http
HTTP/1.1 403 Forbidden
Content-Type: application/json
```

```json
{
  "code": "ACCESS_DENIED",
  "message": "Anda tidak memiliki akses untuk tindakan ini.",
  "correlationId": "corr_...",
  "details": null
}
```

401 SHOULD menyertakan `WWW-Authenticate` sesuai protocol tanpa issuer internals, validation detail, expected permission, token content, stack trace, object existence, atau security policy internals. 403 tidak menyebut permission tersembunyi atau daftar user yang berhak. Semua diagnostics rinci hanya di server-side secure telemetry dengan data minimization.

---

## 6. Rate-limit model

### 6.1 Prinsip model

Rate limit digunakan untuk dua tujuan yang sama penting:

1. membatasi abuse, enumeration, credential misuse, spam, dan automated retry storm;
2. menjaga API, SQL Server, storage, queue, Worker, serta provider AI/OCR/conversion tetap sehat pada server kecil.

Model yang direkomendasikan adalah **layered admission control** dengan limit configuration/policy-driven. Effective limit adalah nilai paling ketat dari system safety, enterprise policy, environment capacity, per-user scope, endpoint category, dan downstream provider capacity.

### 6.2 Partition key

- Request authenticated: stable subject/account ID + endpoint category. Jangan memakai nama/display name atau bearer token sebagai key.
- Request service-to-service: service identity + audience + operation category.
- Request pre-auth/invalid auth: network source/device/app signal sebagai control tambahan, bukan satu-satunya control karena corporate NAT dapat menggabungkan banyak user.
- IP/network limit tidak boleh menggantikan per-user limit dan tidak boleh dipercaya sebagai identity.

### 6.3 Endpoint category

| Kategori | Contoh | Bentuk control |
|---|---|---|
| Authentication/session | callback/session bootstrap, profile refresh | burst kecil, anti-loop, IdP-aware |
| Metadata read | list/detail history, issue, secured file, config | per-user token bucket + query-cost guard |
| Command/write | update setting, cancel/retry, issue triage | lower burst; idempotency alignment |
| Issue submission | pesan dan optional screenshot | spam limit per user + attachment quota |
| Upload/download | init/chunk/finalize atau stream sesuai API-DEC-004 | concurrent stream, bytes, file count, request size, bandwidth |
| Background job admission | feature execute, retry, export | request rate + weighted job quota + concurrent-job limit |
| Expensive processing | AI, OCR, Office/PDF conversion, large batch | separate capacity pool, weighted quota, global/provider cap |
| Admin/audit export | audit/issue export | permission + reauth bila policy + asynchronous quota |

Exact numeric rate, burst, window, and reset values MUST berada pada typed configuration/policy dengan safe minimum/maximum. Perubahan policy harus tervalidasi, permission-gated, dan diaudit. Client tidak memilih limit class.

### 6.4 HTTP behavior

| Kondisi | Status/code rekomendasi |
|---|---|
| Per-user/per-category request window habis | `429 RATE_LIMITED` |
| Per-user concurrent job/stream penuh | `429 CONCURRENCY_LIMIT_REACHED` |
| Expensive quota window habis | `429 EXPENSIVE_QUOTA_EXCEEDED` |
| Request/file melewati hard body limit | `413 PAYLOAD_TOO_LARGE` |
| Account storage quota tidak cukup | `409 STORAGE_QUOTA_EXCEEDED` |
| Global queue/worker/provider capacity penuh dan request tidak aman untuk diantrikan | `503 CAPACITY_TEMPORARILY_UNAVAILABLE` |

Semua 429 MUST menyertakan `Retry-After` sebagai delta seconds atau HTTP-date yang benar. 503 temporary capacity SHOULD menyertakan `Retry-After`. Response tidak mengungkap global queue depth, jumlah user lain, worker count, provider credential, atau internal threshold.

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 30
Content-Type: application/json
```

```json
{
  "code": "RATE_LIMITED",
  "message": "Terlalu banyak permintaan. Coba lagi nanti.",
  "correlationId": "corr_...",
  "details": {
    "retryAfterSeconds": 30
  }
}
```

`details` hanya memuat safe retry guidance. Tidak ada stack trace atau security diagnostics.

---

## 7. Quota/concurrency model

### 7.1 Dimensi quota

| Resource | Dimensi minimum | Admission behavior |
|---|---|---|
| Request biasa | per subject, category, short window, burst | 429 saat habis |
| Expensive job | weighted units berdasarkan Feature ID, ukuran, halaman/item, execution profile, dan concurrency risk | reserve sebelum 202; 429 bila user quota habis |
| Concurrent job | per user, per expensive category, dan global worker pool | 202/QUEUED bila slot queue ada; 429 user limit; 503 global saturation |
| Upload | maximum file/request, file count, concurrent stream, bytes per window, temporary-storage budget | reject sebelum/selama stream; cleanup partial data |
| AI/OCR/conversion | per-user quota, global concurrency, provider capacity, timeout, circuit state | queue terbatas, approved fallback saja, atau 503 |
| Secured account storage | total bytes, file/version count, retention | 409 sebelum commit bila quota tidak cukup |
| Issue attachment | file count, per-file size, total report size, MIME/signature validation | message tetap dapat dikirim tanpa attachment hanya setelah user memilih; attachment tidak dijatuhkan diam-diam |
| Export | active export count, date span, output size, retention | background job dengan quota dan expiry |

### 7.2 Reservation dan accounting

- Admission preflight memperkirakan cost dari ukuran, page/item count, Feature ID, provider, dan cumulative multi-step workflow.
- Quota/concurrency slot di-reserve secara atomic sebelum API mengembalikan `202 Accepted`.
- Reservation mempunyai expiry dan dilepas pada reject, cancel, terminal failure, completion, atau orphan cleanup.
- Actual usage dapat direkonsiliasi setelah job, tetapi estimate tidak boleh memungkinkan negative quota atau unbounded overrun.
- Retry/resume tahap yang sama tidak boleh menggandakan job/quota bila idempotency contract menyatakan request yang sama; alignment final menunggu `API-DEC-005`.
- Queue fairness SHOULD mencegah satu user/batch besar memonopoli semua Worker. Metadata/auth/issue message traffic dipisahkan dari expensive processing capacity.
- Global capacity dihitung dari benchmark CPU, peak RAM, storage I/O, bandwidth, DB impact, engine behavior, dan provider limit dengan safety headroom.
- Limit AI, OCR, Office conversion, PDF conversion, dan general job tidak digabung menjadi satu angka jika resource profile berbeda.
- System safety maximum tidak dapat dimatikan dari admin UI. Enterprise policy hanya bekerja di dalam safe bounds.

### 7.3 Upload abuse protection

Upload control MUST mencakup:

- HTTPS, authentication, permission, dan explicit upload action/consent;
- streaming dan hard request/file limit;
- extension, MIME, file signature, structural validation, page/item count, decompressed-size/bomb check, dan malware/security inspection sesuai policy;
- concurrent-stream dan temporary-storage quota;
- bounded timeout serta slow-upload protection pada reverse proxy/API;
- cleanup partial/orphan file;
- storage reference yang scoped; tidak menerima physical path/storage key arbitrary dari client;
- file/document content tetap untrusted setelah lolos format validation.

---

## 8. Backpressure behavior

### 8.1 Admission sequence

```text
Request
→ authentication
→ permission/ownership/policy
→ schema + file/query validation
→ resource/capacity preflight
→ quota/concurrency reservation
→ immediate execution OR bounded queue
→ 200/201/202 OR controlled 4xx/503
```

### 8.2 Queue behavior

- Queue MUST bounded per workload class. Tidak ada unbounded in-memory queue.
- Job yang diterima mengembalikan `202`, `jobId`, `correlationId`, persisted status, dan resource/status location sesuai API baseline.
- State yang digunakan: `QUEUED`, `WAITING_RESOURCE`, `WAITING_CONNECTION`, `RUNNING`, `RETRYING`, dan terminal state yang sudah dikunci upstream.
- REST persisted job state tetap source of truth; SignalR hanya mempercepat event.
- Worker concurrency dapat diturunkan otomatis saat CPU, memory, storage, DB, atau provider pressure meningkat.
- Queue admission dapat ditutup sementara sebelum server crash. Request baru mendapat 503 jika tidak aman diantrikan.
- Retry memakai bounded exponential backoff + jitter, menghormati `Retry-After`, dan tidak berjalan tanpa batas.
- Offline queue disebarkan kembali secara bertahap setelah reconnect; client tidak boleh mengirim seluruh backlog sekaligus.
- Priority tidak didasarkan pada role baru. Fairness dan workload safety policy ditetapkan tanpa bypass permission.

### 8.3 Graceful degradation

Urutan degradasi:

1. capability local yang aman tetap berjalan; gangguan server/AI tidak menonaktifkan tool lokal;
2. expensive server job masuk bounded queue dan Activity Center menampilkan status menunggu;
3. Worker concurrency dikurangi dan admission job baru dibatasi;
4. approved alternate provider MAY digunakan hanya jika resolver, policy, permission, security, license, dan compatibility mengizinkan;
5. AI internal unavailable beralih ke deterministic/local routing atau memberi unavailable guidance; tidak pernah beralih ke public AI endpoint;
6. OCR/conversion yang tidak mempunyai approved fallback tetap menunggu atau gagal aman dengan retry guidance;
7. issue message tetap dapat dilaporkan saat attachment capacity bermasalah jika user secara eksplisit memilih kirim tanpa screenshot; screenshot tidak auto-upload dan tidak dihapus diam-diam dari laporan yang sudah disetujui;
8. export besar dialihkan ke background job atau diminta mempersempit date range;
9. client tidak menerima stack trace, provider detail, queue depth, atau infrastructure topology.

---

## 9. Security risks

| Risiko | Dampak | Control yang direkomendasikan |
|---|---|---|
| Broken object-level authorization/IDOR | file, issue, history, atau audit user lain terbuka | ownership + permission di application service; concealment 404; test negative per object |
| Administrator/Superadmin overreach | akses isi dokumen tanpa dasar | permission granular; no implicit file access; attachment hanya yang dikirim user; audit + reauth untuk akses sensitif |
| Stale offline permission | user tetap menjalankan aksi setelah revocation | offline TTL configurable; local-only narrow scope; online check untuk sensitive/server action; refresh saat reconnect |
| Token/secret leakage | account/service compromise | secure storage; no token in URL/log/diagnostics; short-lived scoped credential; no token forwarding ke Worker/provider |
| Query injection/dynamic SQL | data compromise | allowlisted fields/operators; parameterized ORM/query; no raw expressions; client never direct SQL |
| Expensive query abuse | DB/CPU exhaustion | page maximum; cursor; date-span/search/filter limits; index-aligned sort; timeout; async export |
| Cursor tampering/replay | scope bypass atau enumeration | opaque integrity-protected scope-bound cursor; expiry; authorize every continuation |
| Count/existence leakage | mengungkap resource tersembunyi | count setelah authorization; omit total on cursor; consistent 404 concealment |
| Upload/decompression bomb/malware | storage, memory, Worker exhaustion | streaming; compressed/decompressed limits; signature/structure/security inspection; temp quota; isolated Worker |
| File/document prompt injection | Assistant/provider menjalankan instruksi dokumen | treat content as untrusted; canonical tool/schema/policy validation; no arbitrary API/SQL/shell |
| Public AI fallback/data exfiltration | content keluar dari trust boundary | approved local/internal provider only; policy/permission/minimization; no public endpoint |
| Worker/internal service over-trust | lateral movement/privilege escalation | service identity, audience scope, job-scoped access, policy recheck, no network-location trust |
| Rate-limit bypass | distributed abuse atau header spoof | server-derived subject/service key; supplemental edge signals; no client-selected bucket |
| Corporate NAT false positive | banyak user sah terblokir | per-user primary partition; IP hanya secondary; calibrated burst |
| Queue starvation/retry storm | server kecil tidak sehat | bounded fair queue; separate workload pool; backoff+jitter; reservation expiry; circuit/capacity control |
| Screenshot privacy leakage | tampilan sensitif masuk support storage | explicit capture, preview, explicit send, validation/limit, permission, retention, secure storage |
| Unsafe error disclosure | attacker mengetahui policy/topology | safe envelope; correlation ID; no stack/claim/permission/provider/queue detail |
| Policy misconfiguration | limit terlalu tinggi atau akses terlalu luas | priority layers; typed validation; safe bounds; change audit; staged rollout/load test |

---

## 10. Open decisions

Semua item berikut tetap milik user + Integrator:

| No | Keputusan terbuka | Rekomendasi Agent C |
|---:|---|---|
| 1 | Lock `API-DEC-006` | setujui satu List Query Standard dengan page untuk bounded ordinary metadata dan cursor untuk audit/issue/secured-file/history/large list |
| 2 | Page profile final | setujui default umum 25, maximum umum 100, serta history/job 25/50 untuk memenuhi initial-load cap |
| 3 | Allowlist filter/search/sort per endpoint | dokumentasikan saat endpoint inventory dibuat; jangan membuka arbitrary field/operator |
| 4 | Cursor expiry dan kebutuhan snapshot | expiry configurable; live list keyset; snapshot hanya untuk export/job yang membutuhkan konsistensi |
| 5 | Pertamina Identity contract | konfirmasi protocol, issuer, audience, claim mapping, key discovery/rotation, revocation support, dan approved client library/flow |
| 6 | Offline session TTL dan sensitive-action list | tetapkan lewat Security Design/policy setelah risk review; server/admin/support-sensitive action harus online |
| 7 | Permission catalog/Role Matrix | tetapkan granular permission dan mapping User/Administrator/Auditor serta support capability di Document 09; jangan menganggap Superadmin sebagai bypass/role baru otomatis |
| 8 | Service-to-service credential technology | pilih managed/service identity atau certificate/short-lived token sesuai environment Pertamina; no shared hardcoded secret |
| 9 | Exact rate/burst/window values | tentukan per category dari pilot load test dan kapasitas reverse proxy/API/DB, bukan hardcode source |
| 10 | Exact job weight, concurrent-job, queue, upload, storage, export, AI/OCR/conversion cap | tetapkan per environment/provider setelah PoC, benchmark, license/security review, dan safety headroom |
| 11 | Global saturation semantics | rekomendasi: 429 untuk caller quota/concurrency, 503 untuk shared system/provider saturation, 413 untuk hard payload limit, 409 untuk storage quota |
| 12 | Rate-limit visibility | tampilkan hanya `Retry-After` dan safe code/message; metric rinci tetap internal kecuali policy mengizinkan standard RateLimit headers |
| 13 | Source version mismatch | task menyebut API Working v0.1, workspace hanya menyediakan v0.2; Integrator perlu memastikan v0.2 memang baseline yang akan digabung |

---

## 11. Suggested Document 06 text

Teks berikut siap dipertimbangkan Integrator. Statusnya tetap **PROPOSED**, bukan LOCKED.

### API-DEC-006 — Pagination / Filter / Sort Standard

**Status: PROPOSED — Agent C recommendation**

PERTAMINA OTO menggunakan satu List Query Standard dengan dua mode. Bounded ordinary metadata list menggunakan `page` dan `pageSize`; audit/event, issue, secured-file, history/job, dan large/changing list menggunakan opaque cursor dan `pageSize`. Satu endpoint tidak menerima page dan cursor bersamaan.

Default umum adalah `page=1`, `pageSize=25`, maximum `100`. History/job menggunakan default `25` dan maximum `50` agar pemuatan awal memenuhi `FR-HIST-003`. Nilai invalid atau melewati maximum menghasilkan `400 INVALID_PAGINATION` dan tidak di-clamp diam-diam.

Cursor menggunakan keyset pagination, integrity-protected, scope-bound terhadap subject/endpoint/filter/search/date/sort, mempunyai expiry configurable, dan tidak memuat SQL, token, secret, storage key, atau data sensitif. Setiap continuation di-authenticate dan di-authorize ulang. Cursor tampered menghasilkan `400 INVALID_CURSOR`; cursor expired menghasilkan `410 CURSOR_EXPIRED`. Exact total tidak wajib pada cursor list.

Setiap list memiliki stable default order dengan unique tie-breaker. Baseline: audit `occurredAt desc, auditId desc`; issue `createdAt desc, issueId desc`; secured file `securedAt desc, securedFileId desc`; history/job `completedAt desc, historyId/jobId desc`. Mutable status/priority tidak menjadi default ordering.

Query parameter standar adalah `sort`, `filter[field]`, `search`, `from`, dan `to`. Filter/sort/search field menggunakan allowlist per endpoint. Repeated value pada satu filter berarti OR dan field berbeda berarti AND. Date range memakai RFC 3339 UTC dengan semantik `[from,to)`. Arbitrary field path, regex, raw expression, dynamic SQL, document-content search, dan client-defined authorization filter dilarang. Authorization scope diterapkan server-side sebelum pagination dan count.

Wide audit/history/issue report yang membutuhkan snapshot atau exact total dibuat sebagai permission-gated asynchronous export job, bukan live query tanpa batas.

### API-DEC-007 — Client Authentication Boundary

**Status: PROPOSED — Agent C recommendation**

Trust flow:

```text
Client
→ Pertamina Identity
→ API authentication
→ permission + ownership + policy
→ application service
```

Semua komunikasi menggunakan HTTPS. API memvalidasi signature, allowed algorithm, issuer, audience, lifetime, subject, dan claim contract token Pertamina. Role/permission dari client tidak dipercaya sebagai authorization final; permission efektif dan policy dikendalikan backend.

Token acquisition/refresh dilakukan antara client dan Pertamina Identity melalui approved flow/library. API resource tidak menerima refresh token. Native client menggunakan secure OS storage dan approved PKCE flow; Web menjaga credential/token di server-side session boundary bila sesuai arsitektur dan tidak menyimpan bearer/refresh token pada browser-accessible storage.

Offline session hanya berlaku untuk local capability selama cached session/permission masih valid menurut configurable policy. Server-required dan sensitive action menunggu online authorization. Setelah reconnect, identity dan permission di-refresh sebelum queued command dikirim.

Authorization bersifat deny-by-default, permission-based, object-aware, dan diperiksa pada application boundary untuk setiap list/detail/command/download/attachment/export/retry. Role tidak menggantikan permission. Administrator tidak otomatis dapat membaca file user. Superadmin/Support adalah permission-gated support surface, bukan bypass atau role baru otomatis; akses issue attachment hanya mencakup attachment yang user kirim eksplisit.

Sensitive reauthorization membuktikan authentication recency/assurance dan tidak menggantikan permission/ownership/policy. Password tidak pernah diminta atau disimpan oleh PERTAMINA OTO.

API/Worker/internal provider memakai service identity terpisah dengan short-lived audience-scoped credential atau certificate/managed identity yang disetujui. End-user token tidak diteruskan ke Worker/provider. Internal network dan queue tidak otomatis dipercaya. Worker menerima job-scoped access, memverifikasi job/policy, dan tidak mempunyai arbitrary SQL/storage/API access. AI/OCR/conversion hanya memakai approved local/enterprise-internal provider; public AI endpoint dilarang.

401 digunakan bila tidak ada authentication yang dapat dipakai, token/session invalid/expired/revoked, atau step-up diwajibkan. 403 digunakan bila identity valid tetapi permission/ownership/policy menolak. Sensitive resource MAY menggunakan consistent 404 concealment. Error tetap memakai safe standard envelope, correlation ID, tanpa stack trace, token detail, required-permission detail, object existence, atau security internals.

### API-DEC-009 — Rate Limit / Quota / Backpressure Contract

**Status: PROPOSED — Agent C recommendation**

Rate limit adalah security control sekaligus server-health control. Effective limit adalah nilai paling ketat dari system safety, enterprise policy, environment capacity, per-user scope, endpoint category, global Worker capacity, dan downstream provider capacity. Exact angka disimpan pada typed configuration/policy dan dikunci setelah PoC/load test; bukan hardcoded tersebar di source.

Authenticated request dipartisi terutama berdasarkan stable subject ID + endpoint category. IP/network signal hanya control tambahan. Service request dipartisi berdasarkan service identity + audience + category. Kategori minimum: auth/session, metadata read, command/write, issue submission, upload/download, background job admission, expensive AI/OCR/conversion, dan admin/audit export.

Per-user request quota, weighted expensive-job quota, per-user/category/global concurrent-job limit, concurrent upload stream, file/request/temporary-storage limit, secured-storage quota, export quota, serta AI/OCR/conversion provider capacity diterapkan sebelum admission. Slot/quota di-reserve sebelum `202 Accepted`, mempunyai expiry, dan dilepas pada reject/cancel/terminal state/cleanup.

429 digunakan untuk caller rate/quota/concurrency dan MUST menyertakan `Retry-After`. 413 digunakan untuk hard payload limit, 409 untuk storage quota, dan 503 untuk shared queue/worker/provider saturation yang tidak aman diantrikan; temporary 503 SHOULD menyertakan `Retry-After`. Response tidak mengungkap queue depth, worker count, user lain, provider detail, atau threshold internal.

Queue per workload class harus bounded dan fair. Persisted REST state tetap source of truth; SignalR hanya event channel. Saat pressure meningkat, sistem mengurangi Worker concurrency, menahan job sebagai `WAITING_RESOURCE`, membatasi admission, dan menggunakan bounded exponential backoff + jitter. Offline backlog dilepas bertahap setelah reconnect.

Graceful degradation mempertahankan safe local capability, mengantrikan expensive server work secara terbatas, menggunakan approved provider fallback hanya bila policy mengizinkan, dan memberi unavailable/retry guidance bila tidak ada capacity. Gangguan AI internal tidak menonaktifkan deterministic/local tools dan tidak pernah menyebabkan fallback ke public AI endpoint. Screenshot issue tetap optional, explicit, previewed, validated, limited, dan tidak pernah auto-upload.

Acceptance minimum:

```text
[ ] Authorization/ownership diuji sebelum pagination dan count
[ ] Cursor tamper, expiry, permission change, dan continuation diuji
[ ] Query allowlist, stable ordering, maximum page size, date span, dan timeout diuji
[ ] 401/403/404 concealment tidak membocorkan security detail
[ ] Token/secret/document/password/OCR/screenshot content tidak masuk log/telemetry
[ ] Per-user rate, expensive quota, concurrent job, upload, dan global capacity diuji
[ ] 429/413/409/503 serta Retry-After konsisten
[ ] Queue bounded, retry bounded, reservation cleanup, dan graceful degradation diuji
[ ] Worker/service identity dan least-privilege job scope diuji
[ ] Tidak ada public AI endpoint atau fallback tidak disetujui
```
