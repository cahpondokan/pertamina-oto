# AGENT C — SECURITY, PRIVACY, COMPATIBILITY & CROSS-PLATFORM REPORT

**Task ID:** DOC12-AGENT-C  
**Status:** RECOMMENDATION — keputusan final tetap milik User dan Integrator Document 12  
**Tanggal analisis:** 2026-08-28  
**Output scope:** test plan; tanpa coding dan tanpa perubahan master Document 12

## Baseline dan cara membaca laporan

Laporan ini diturunkan dari Document 01–11 FINAL. Rujukan utama adalah arsitektur pada Document 04, kontrak teknis pada Document 05, API pada Document 06, Security Design pada Document 08, Role Matrix pada Document 09, Audit Trail pada Document 10, dan batas Unit Test/handoff pada Document 11.

Invarian yang tidak boleh diubah oleh test plan ini:

- REST/persisted state adalah source of truth; SignalR hanya event notification.
- Semua jalur UI, File-First, Tool-First, workflow, dan Assistant memakai Feature ID canonical serta official Feature execution flow.
- Autentikasi memakai Pertamina Identity; otorisasi deny-by-default berdasarkan permission, ownership/scope, policy, state, capability, compatibility, connectivity, confirmation, dan reauthentication.
- Administrator, Auditor, dan Support tidak memiliki implicit content access atau hierarchy otomatis.
- Local Worker terisolasi; IPC bertipe dan berversi; network/queue/worker tidak dipercaya hanya karena internal.
- Document/OCR/HTML/AI input dan output adalah untrusted input.
- Public AI endpoint dilarang; hanya local/internal provider yang telah disetujui.
- Test aktif dan VA/PT dilakukan pada environment yang diotorisasi, bukan Production, serta memakai data sintetis tanpa data sensitif nyata.

Prioritas: **P0** = release/security gate, **P1** = wajib sebelum area terkait dipromosikan, **P2** = hardening/regression. Nilai ukuran, waktu, rate, concurrency, dan versi platform harus berasal dari configuration/policy fixture, bukan angka hardcoded pada test.

# 1. Security-test strategy

## 1.1 Tujuan dan pendekatan

Strategi menggunakan defense-in-depth dan negative testing. Test tidak hanya membuktikan happy path, tetapi juga bahwa permintaan ditolak **sebelum** content resolution, storage access, provider invocation, atau side effect terjadi.

| Lapisan | Sasaran | Bukti minimum |
|---|---|---|
| Unit | Policy/permission evaluator, allowlist, bound, sanitizer, safe mapper, path rule, AI/tool rule | Keputusan deterministik; spy membuktikan dependency sensitif tidak terpanggil |
| Contract | OpenAPI, Feature/schema, IPC, event, error, forbidden-field, compatibility | Fixture versi dan schema; breaking diff terdeteksi; unknown input fail closed |
| Integration | Auth middleware, API–DB–object storage, audit/outbox, real serializer, local storage protection, worker boundary | Request/response dan state nyata pada environment terisolasi |
| Authorization | Cross-user, cross-role/profile, object ownership, list/count/cursor/range/export/subscription | Matriks allow/deny dan bukti tidak ada resource enumeration |
| Abuse/negative | Injection, malformed input, over-limit, replay, race, flood, file bomb, prompt injection | Safe rejection, bounded resource use, no unsafe retry, no sensitive error |
| E2E/UI | SSO→feature→job/result, secured-file, issue, offline/reconnect, platform UX | Journey lintas boundary dan evidence screenshot/log yang sudah disanitasi |
| Performance/DoS | Queue, pool, connection, stream, worker/provider, slow consumer, cleanup | SLO test environment, bounded degradation, recovery tanpa data corruption |
| Static/supply chain | Source, dependency, secret, configuration, artifact, SBOM | Scan record, triage, waiver terkontrol, remediation/retest |
| PoC/VA/PT | Kontrol OS/proxy/process/provider dan attack chain sistem | Rules of engagement, independent evidence, retest atas temuan |

Unit test Document 11 tetap dipakai untuk business/security decision logic. Real authentication middleware, wire serialization, real Named Pipe ACL, engine binary, browser, device, reverse proxy, resource containment, dan penetration test ditempatkan pada test lanjutan yang tepat; tidak dipaksakan menjadi unit test.

## 1.2 Test environment dan data

- Gunakan tenant/account/profile uji terpisah dengan set principal sintetis: User A, User B, Administrator terbatas, Auditor read-only, dan Support profile dengan permission eksplisit.
- Gunakan synthetic canary yang unik per channel agar sumber leakage dapat ditelusuri tanpa memakai password, token, dokumen, signature, atau issue milik orang nyata.
- Malicious-file corpus dibuat secara sintetis, berlisensi jelas, tersimpan pada zona test terisolasi, tidak pernah dibuka oleh aplikasi desktop umum, dan memiliki hash/manifest.
- Outbound network default-deny pada test file/HTML/AI; hanya endpoint sink/canary internal yang disetujui boleh dipakai untuk membuktikan tidak ada egress.
- Load/abuse test memiliki limit, stop condition, owner environment, observability, backup/recovery point, dan jadwal agar tidak mengganggu shared QA.
- Evidence harus menghindari raw request body, credential, full document/OCR/prompt, signature, path, dan screenshot sensitif.

## 1.3 Template test case minimum

Setiap case mencatat: Test ID, requirement/Feature ID bila relevan, priority, surface/platform, precondition, synthetic input, action, expected outward result, expected internal side effect/call count, privacy assertion, performance bound, evidence, dan cleanup. Case keamanan harus mempunyai negative oracle yang jelas; sekadar “tidak crash” tidak cukup.

## 1.4 Entry dan exit gate

Entry: contract/schema tersedia; environment diotorisasi; accounts/permissions/policies terversi; fixture dan canary tervalidasi; observability aman; rollback/cleanup siap. Exit minimum: seluruh P0 lulus, tidak ada leakage canary, Critical/High ditangani sesuai kebijakan, cross-user suite lulus, compatibility gate lulus, dan temuan yang diterima memiliki owner, alasan, compensating control, serta masa berlaku.

# 2. Authentication scenarios

| ID | Skenario | Expected result | Priority/layer |
|---|---|---|---|
| `SEC-AUTH-001` | Request tanpa credential/session | `401`; tidak ada data, count, cursor, storage lookup, atau detail identity | P0 API integration |
| `SEC-AUTH-002` | Token expired/not-before belum valid | `401`; resource API tidak mencoba refresh dan tidak menerima refresh token | P0 API integration |
| `SEC-AUTH-003` | Signature, allowed algorithm, issuer, audience, token type, subject, atau claim contract salah | `401` dengan safe error; tidak mengungkap rule validasi spesifik yang membantu attacker | P0 contract/integration |
| `SEC-AUTH-004` | Session/token dicabut atau logout telah selesai | Credential lama tidak dapat dipakai; Hub/API/queued action gagal aman | P0 integration/E2E |
| `SEC-AUTH-005` | Account dinonaktifkan | Tidak dapat mengakses server action/result; outward `401` bila credential sudah tidak usable atau `403` bila identity valid tetapi account-state policy menolak, sesuai kontrak identity/API final | P0 integration |
| `SEC-AUTH-006` | Native Desktop/Mobile callback dengan PKCE verifier, state, nonce, redirect, atau transaction mismatch | Login ditolak, tidak membuat session, tidak memantulkan data sensitif | P0 E2E security |
| `SEC-AUTH-007` | Refresh request concurrent/replayed/rotated credential lama | Hanya alur yang sah berhasil; tidak terjadi session duplication/fixation; event aman tercatat | P0 identity integration |
| `SEC-AUTH-008` | Web session cookie hilang/diubah; session fixation; logout pada tab lain | Session tidak dipakai ulang; cookie attributes dan server-side session boundary berfungsi | P0 web integration |
| `SEC-AUTH-009` | Reauthentication dibutuhkan tetapi freshness/binding salah | `401` dengan safe code `REAUTHENTICATION_REQUIRED`; action tidak berjalan | P0 API/E2E |
| `SEC-AUTH-010` | Reauthentication valid tetapi permission/policy tetap tidak memenuhi | `403`; reauth tidak menjadi permission escalation | P0 authorization |
| `SEC-AUTH-011` | Offline grant valid, profile/device cocok, trusted time valid, local action eligible | Hanya local eligible action diizinkan; server call/upload = 0 | P0 local integration |
| `SEC-AUTH-012` | Offline grant tampered, expired, clock rollback, wrong profile/account/device, stale revision | Fail closed atau `ONLINE_REQUIRED`; tidak ada silent grace extension | P0 local security |
| `SEC-AUTH-013` | Admin/Support/Audit/secured/server-required/realtime action saat offline | Ditolak atau `ONLINE_REQUIRED`; cached role/claim tidak memberi privileged access | P0 E2E |
| `SEC-AUTH-014` | Reconnect dengan permission revoked/account disabled/client incompatible | Refresh identity/account/session/permission/policy/capability/compatibility terjadi sebelum dispatch; queued command yang tidak lagi sah tidak dikirim | P0 recovery/E2E |
| `SEC-AUTH-015` | SignalR reconnect memakai credential expired/revoked | Hub menolak/revalidasi; REST tidak membocorkan state; UI menunjukkan degraded/auth-required secara aman | P0 SignalR integration |
| `SEC-AUTH-016` | Account switch sementara queue profil lama ada | Queue lama tidak dikirim atas identity baru; storage/profile boundary tetap terpisah | P0 desktop/mobile |

Tambahan yang harus diverifikasi:

- Access/refresh token tidak berada pada URL, log, telemetry, audit, issue diagnostics, error, browser-accessible storage, atau test artifact.
- Desktop/Mobile menggunakan approved PKCE flow dan secure OS credential storage; Web mengikuti server-side session boundary sesuai arsitektur.
- Resource API bukan Identity Provider dan tidak menerima refresh token.
- Timeout/lock/unlock device, sleep/resume, multi-tab, network flapping, dan timezone/clock change tidak memperpanjang otorisasi secara diam-diam.

# 3. Authorization scenarios

Keputusan otorisasi harus mengevaluasi seluruh guard dari Document 09/11 dan berhenti sebelum content/storage resolution saat denial. Role label, opaque ID, FileRef, Job ID, Correlation ID, cursor, ETag, URL download, atau possession of event bukan bukti akses.

| ID | Principal dan tindakan | Expected result |
|---|---|---|
| `SEC-AZ-001` | User dengan permission dan own resource yang valid | Diizinkan hanya dalam policy, state, capability, compatibility, connectivity, consent/confirmation/reauth yang berlaku |
| `SEC-AZ-002` | User tanpa action permission | `403`; handler/provider/storage tidak terpanggil |
| `SEC-AZ-003` | User A memakai ID/FileRef/job/result/history/secured file/issue User B | Deny; gunakan `404` concealment bila discovery access tidak ada dan konsisten di semua method |
| `SEC-AZ-004` | User mengubah owner/actor/permission/role pada body atau mass assignment | Server-derived identity/scope menang; field client ditolak/diabaikan sesuai closed schema |
| `SEC-AZ-005` | Administrator membaca/mengunduh dokumen, OCR, signature, secured file, issue attachment user tanpa content permission khusus | Deny; hak admin konfigurasi/account tidak memberi content access |
| `SEC-AZ-006` | Administrator mencoba melemahkan hard safety ceiling, memberi diri sendiri akses melalui field tak dikenal, atau menghapus bukti perubahan | Deny; separation-of-duties dan audit intent tetap berlaku |
| `SEC-AZ-007` | Auditor mencari/membaca audit dalam assigned scope | Hanya safe metadata yang diizinkan; query permission dan scope diberlakukan sebelum count/cursor |
| `SEC-AZ-008` | Auditor mengubah/menghapus audit, policy, atau user data | Deny; Auditor default controlled read-only |
| `SEC-AZ-009` | Auditor export tanpa permission export atau di luar scope | Deny sebelum export job dibuat; tidak ada partial file |
| `SEC-AZ-010` | Support melihat issue/detail/screenshot/diagnostics yang user kirim eksplisit dan sesuai assignment/scope | Diizinkan hanya dengan permission terkait; action diaudit |
| `SEC-AZ-011` | Support mengakses full user document/OCR/signature/clipboard/unrelated enterprise audit | Deny; Support/Superadmin bukan God Mode dan bukan role hierarchy |
| `SEC-AZ-012` | Support memakai issue reference untuk pivot ke job/file/user lain | Hanya safe linked metadata yang allowlisted; setiap target diotorisasi ulang |
| `SEC-AZ-013` | Assistant session aktif tetapi Feature permission/ownership/policy hilang | Deny identik dengan UI/File-First; `assistant.session.use` bukan tool permission |
| `SEC-AZ-014` | Workflow multi-step: step pertama boleh, step berikutnya dilarang | Step terlarang tidak berjalan; tidak ada partial side effect yang dianggap success; recovery sesuai kontrak |
| `SEC-AZ-015` | Permission/policy dicabut antara preflight, upload, queue, execute, retry, result, download, range, export, atau subscription | Reauthorize pada setiap boundary; stale grant tidak cukup |
| `SEC-AZ-016` | Internal Worker/provider mencoba memakai end-user token atau meminta scope di luar workload identity | Ditolak; scoped service identity dan operation/resource binding wajib |
| `SEC-AZ-017` | List/filter/search dengan owner atau tenant lain, cursor lintas scope, atau page continuation setelah permission berubah | Scope diterapkan sebelum query/count; cursor ditolak/rebased aman; tidak ada enumeration |

Matriks authorization harus dijalankan langsung ke API dan bukan hanya melalui UI. Semua allow/deny diuji untuk Desktop, Web, Phone, Tablet jika surface terekspos. Feature yang tidak terekspos pada platform harus menghasilkan unavailable/unsupported explanation, bukan authorization bypass melalui direct request.

# 4. File-security testing

## 4.1 Prinsip corpus aman

Gunakan generator/fixture sintetis dengan payload minimum untuk memicu guard. File bom tidak perlu diekspansi sampai merusak host: unit test menguji declared/observed bounds; integration test memakai bounded high-ratio sample; PoC containment memakai host khusus dengan CPU/RAM/temp/time/output/child-process limit dan emergency stop. Hash, expected parser path, dan disposal setiap sample dicatat.

| ID | Synthetic sample/attack | Expected result |
|---|---|---|
| `SEC-FILE-001` | PDF truncated, broken xref/object stream, recursive object, invalid page tree | Reject/quarantine/safe failure; source immutable; no parser stack/path in response |
| `SEC-FILE-002` | Body/file/page/pixel/dimension/count di atas policy dan hard ceiling | `413` atau typed validation result sebelum expensive processing; stream dibatasi |
| `SEC-FILE-003` | Declared MIME salah terhadap signature/structure | `415`/validation failure; tidak memilih engine hanya dari client MIME |
| `SEC-FILE-004` | Fake extension dan polyglot minimal | Pipeline signature+structure mendeteksi mismatch; tidak diteruskan ke unsafe handler |
| `SEC-FILE-005` | Filename `../`, absolute, UNC, device name, ADS, encoded separator, Unicode confusable | Nama dinormalisasi untuk display saja; storage memakai opaque server-generated ref; traversal ditolak |
| `SEC-FILE-006` | Symlink/reparse/TOCTOU swap pada workspace test | Path tetap berada pada approved root; identity/digest binding mendeteksi pergantian; no cross-job access |
| `SEC-FILE-007` | ZIP/Office nested archive dengan expansion ratio/depth/count ekstrem | Bounded inspection; abort/quarantine; no uncontrolled temp/storage growth |
| `SEC-FILE-008` | Office macro, OLE, DDE, external template/link, embedded object/action | Active content tidak dijalankan; external fetch disabled; conversion diisolasi; output divalidasi |
| `SEC-FILE-009` | HTML dengan script, iframe, `file:`, localhost/private/link-local URL, redirects, DNS rebinding fixture | Script/active content dan remote fetch mengikuti closed policy; SSRF/local-file access ditolak; egress sink menerima 0 request |
| `SEC-FILE-010` | Corrupted image, extreme dimensions, invalid ICC/EXIF, image polyglot | Decode dibatasi; safe failure; metadata/decoder error tidak bocor |
| `SEC-FILE-011` | Invalid OCR image/PDF, huge canvas, adversarial text/layout, control character | OCR input dibatasi; OCR output diperlakukan data; no instruction execution; timeout/resource result stabil |
| `SEC-FILE-012` | Embedded JavaScript/action/launch/URI/attachment dalam PDF | Tidak dieksekusi; dangerous action stripped/rejected sesuai feature policy |
| `SEC-FILE-013` | Password-protected/encrypted file dengan password salah dan retry flood | Password operation-memory-only; tidak log/persist/sync; retry/rate guard aktif |
| `SEC-FILE-014` | Same input di-submit paralel lalu cancel/crash/timeout | Source tidak berubah; partial/temp bukan final; cleanup/reconciliation idempotent |
| `SEC-FILE-015` | Provider menghasilkan wrong type/root/size/count, HTML/URL aktif, atau malformed result | Output validator menolak; provider success tidak langsung menjadi application success |

Assertions lintas case: tidak ada shell/SQL/arbitrary URL, tidak ada write di luar isolated working directory, child process terbatas, timeout/cancel efektif, temp dibersihkan sesuai lifecycle, crash containment bekerja, dan correlation tetap tersedia tanpa menaruh content sensitif.

# 5. Privacy testing

## 5.1 CanarySet sintetis

Buat marker unik untuk setiap kelas: password/passphrase; access/refresh token dan cookie; secret/private/encryption key; document bytes/full text; full OCR/translated/Markdown content; signature/paraf image; clipboard; prompt/chat/provider request-response; issue message/screenshot/attachment; raw path/storage key/URL; SQL/stack/claim dump. Marker bukan credential atau data real.

## 5.2 Permukaan yang harus diperiksa

| Surface | Expected privacy assertion |
|---|---|
| API response/ProblemDetails | Hanya stable safe code, user-safe message, correlation ID, dan allowlisted field; no stack/provider/path/content |
| Structured logs/scopes/exceptions | Canary sensitif tidak muncul pada message, parameter, scope, exception, sink, atau retained log |
| Telemetry/trace/metric/health | Tidak ada body, token, filename sensitif, OCR, prompt, arbitrary label cardinality, queue topology, atau dependency secret |
| Audit intent/outbox/event/export/alert | Safe metadata allowlist; User/Administrator/Auditor/Support scope benar; original filename diganti opaque FileId/SecuredFileId bila tidak diperlukan |
| Issue diagnostics | Hanya safe diagnostics; screenshot hanya setelah explicit capture→preview→send; deselect/cancel = tidak terkirim |
| SignalR/event | Tidak ada document/preview/OCR/password/token/secret/signature/prompt/stack/path/arbitrary display text |
| Idempotency/cache/checkpoint/history | Secret/password tidak masuk fingerprint/snapshot; temp/cache/draft/checkpoint tidak auto-sync |
| Local storage/crash dump/backup | Profile isolation dan approved protection mencakup artefak relevan; temp/sensitive lifecycle diuji |
| AI boundary | Minimum selected context; approved provider only; no unrelated content; egress/retention assertions sesuai provider contract |
| Test pipeline | Failure message, screenshot, report, attachment, CI log, coverage artifact, and scan evidence tidak memuat canary raw |

## 5.3 Teknik verifikasi

- Jalankan end-to-end canary scan pada response, DB test, log sink, traces, metrics, audit/outbox/export, event capture, issue package, filesystem sandbox, queue message, provider mock capture, dan generated test artifacts.
- Uji sanitizer failure: sistem fail closed; pattern redaction hanya defense-in-depth, bukan kontrol utama. Closed typed allowlist dan source-aware mapper adalah oracle.
- Uji metadata inference: filename, resource identifier, IP/device/network metadata, AI provider metadata, dan screenshot reference diklasifikasi SAFE/MASKED/RESTRICTED/NOT STORED sesuai Document 10.
- Verifikasi range/download/error denial tidak membocorkan existence, size, ETag, owner, filename, atau timing yang mudah dipakai enumeration.

# 6. Assistant/AI testing

Seluruh test memakai mock approved provider atau provider internal/local pada environment yang disetujui. Test tidak mengirim data ke public AI endpoint. Document/OCR/HTML/metadata/filename adalah **DATA**, bukan instruction.

| ID | Skenario | Expected result |
|---|---|---|
| `SEC-AI-001` | Direct prompt injection berbahasa Indonesia/English meminta bypass/system prompt/secret | Ditolak/diabaikan; no privileged context/tool access |
| `SEC-AI-002` | Indirect instruction dalam PDF, OCR, annotation, hidden text, metadata, filename, HTML, translation | Diperlakukan sebagai data; tidak mengubah plan/policy/tool selection |
| `SEC-AI-003` | Model mengirim invalid Feature ID, mengarang Feature ID, atau memakai deprecated/hidden/disabled Feature | Registry canonical menolak; tidak ada dynamic dispatch |
| `SEC-AI-004` | Parameter unknown/extra/wrong type/out-of-bound atau workflow cyclic/oversized | Typed schema/graph validation menolak sebelum execution |
| `SEC-AI-005` | Prompt meminta resource User B atau mengklaim permission/admin/support | Ownership/permission/policy check aktual menolak; prompt tidak menjadi authority |
| `SEC-AI-006` | Request destruktif ambigu | Assistant meminta klarifikasi; belum ada execution/confirmation token |
| `SEC-AI-007` | High-risk/destructive request mencoba melewati confirmation/reauth | Official confirmation/reauth binding wajib; no inferred consent |
| `SEC-AI-008` | Request meminta SQL, shell, filesystem, arbitrary HTTP/API, engine/vendor/class/path/URL | Closed tool/Feature allowlist menolak; provider tidak menerima capability tersebut |
| `SEC-AI-009` | Data leakage attempt: prompt mencoba exfiltrate unrelated document, OCR, signature, token, clipboard, issue, audit, atau AI sensitive payload | Minimum-context selector dan authorization menolak; provider capture tidak menerima canary |
| `SEC-AI-010` | Approved provider timeout/unavailable/malformed/oversized response | Safe typed failure/retry policy; no public/unapproved fallback; no false success |
| `SEC-AI-011` | Provider response berisi malicious Markdown/HTML/URL/control/tool JSON | Output dianggap untrusted, safely rendered/validated, bukan executable command |
| `SEC-AI-012` | Model menyatakan command/job sukses padahal service belum menyelesaikan | Response harus grounded pada actual validated result/persisted Job state; false success gagal test |
| `SEC-AI-013` | Satu workflow memiliki local step dan online-only step saat offline | Setiap step dinilai; tidak auto-upload/reroute; tampilkan online-required/clarification |
| `SEC-AI-014` | Public/unapproved provider ditambahkan melalui config atau response resolver | Reject; hard safety ceiling tidak dapat dilonggarkan Administrator |
| `SEC-AI-015` | Assistant-triggered execution menghasilkan Audit | Actor tetap User, Origin=`ASSISTANT`, Feature ID/Job/Correlation benar, prompt/content tidak diaudit |

Kesetaraan wajib: untuk input, resource, dan policy yang sama, Tool-First, File-First, workflow, dan Assistant-First menghasilkan authorization/execution decision yang sama. Assistant tidak mempunyai endpoint, engine, permission, confirmation, atau security bypass sendiri.

# 7. API security testing

| Area | Skenario minimum | Expected result |
|---|---|---|
| Authentication | Missing/invalid/expired credential | `401`; safe headers/body; no resource discovery |
| Authorization | Valid identity, wrong permission/owner/policy | `403` atau consistent concealed `404`; no downstream content access |
| Schema | Unknown/extra/ambiguous field, wrong discriminator/version/type, overlong header/query/body | Closed validation; deterministic `4xx`; no unsafe deserialization |
| Feature dispatch | Invalid/local-only/invented Feature ID, schema mismatch | Reject before handler/engine; no dynamic class/provider lookup |
| Injection | SQL, command, path, CRLF/header, URL, Markdown/HTML/XSS, CSV/formula payload | Data remains data; parameterized query/allowlist/encoding; no execution |
| Web session | CSRF where cookie auth applies; CORS/Origin/Host/forwarded-header/open-redirect/cache misconfiguration | Explicit allowed origins and trusted proxy behavior; state-changing request protected; no credentialed wildcard |
| Upload | Size/count, MIME/signature/structure, fake extension, session/chunk/order/hash/owner, resumable race | Validate before expensive work; owner/scope rechecked; `413`/`415`/safe conflict; no partial result exposure |
| Download | Wrong owner, guessed ID, expired link, range abuse, parallel range, result not ready | Reauthorize every request/range; bounded range; no size/name/content leakage |
| Query | Cursor tamper/expiry/cross-scope, page+cursor, non-allowlisted filter/sort/search, huge date range | Reject; authorization before count/cursor; stable ordering and bounded query |
| Idempotency | Same key+same fingerprint, same key+different fingerprint, concurrent replay, expired key | Same resource/Job replay; mismatch=`409`; exactly one accepted side effect; secret absent from fingerprint |
| Job state/race | Cancel/retry/resume/result concurrently; stale ETag/stateVersion | State machine/optimistic concurrency wins; no terminal regression or duplicate processing |
| Resource protection | Caller rate exceeded, hard upload bound, tenant quota/state conflict, shared saturation | `429`+valid `Retry-After`; `413`; `409`; `503`+safe guidance, respectively |
| Error | Engine/parser/provider/DB exception | Stable safe code; no stack, SQL, topology, physical path, credential, content, or raw vendor message |
| HTTP intermediaries | Conflicting length/transfer encodings, malformed request line/header, duplicate ambiguous headers through actual proxy/API | Consistent rejection at edge/app; no desynchronization; tested only in authorized environment |
| Health | Liveness/readiness under dependency failure | Minimal `live`; `ready` is `200`/`503`; no connection string, queue depth, topology, or secret |

API test wajib memeriksa middleware order: authentication → authorization/policy → validation/admission as designed → application service, dengan perhatian bahwa no-content-access denial terjadi sebelum resolver/storage/provider. OpenAPI contract tests memastikan breaking change, missing auth/security metadata, unbounded field, dan forbidden response field menjadi gate.

# 8. SignalR testing

| ID | Skenario | Expected result |
|---|---|---|
| `SEC-HUB-001` | Negotiate/connect tanpa credential, expired/revoked credential, atau origin tidak diizinkan | Connection/subscription ditolak tanpa detail sensitif |
| `SEC-HUB-002` | Client memasok User ID/tenant/group/issue/correlation milik pihak lain | Diabaikan/ditolak; group diturunkan server-side |
| `SEC-HUB-003` | User A subscribe Job User B | Deny; User A menerima 0 event User B |
| `SEC-HUB-004` | Event payload/privacy canary | Hanya safe typed envelope; forbidden-field test lulus |
| `SEC-HUB-005` | Duplicate `eventId` atau `sequence <= lastAppliedSequence` | Diabaikan idempotently; terminal state tidak mundur |
| `SEC-HUB-006` | Out-of-order dan `sequence > last + 1` | REST reconciliation; persisted state menang |
| `SEC-HUB-007` | Unknown event type atau unsupported event version | Safe telemetry + REST reconciliation; client tidak crash |
| `SEC-HUB-008` | Disconnect, sleep, network flap, server failover | Job tetap berjalan; bounded reconnect+jitter; REST reconcile; authorized resubscribe |
| `SEC-HUB-009` | Reconnect setelah client/server/capability version berubah | Compatibility dan auth refresh sebelum resubscribe; incompatible client tidak memakai stale event contract |
| `SEC-HUB-010` | Reconnect/retry mencoba mengirim ulang execute/cancel/download melalui Hub | Tidak ada business mutation; command count pada Hub = 0 |
| `SEC-HUB-011` | Connect/subscription/message flood, rapid reconnect, slow consumer, outbound buffer pressure | Rate/buffer/idle limits aktif; degrade/disconnect offender; server tetap sehat |
| `SEC-HUB-012` | Permission/ownership dicabut ketika connection masih hidup | Subscription/event delivery direvalidasi; REST access juga ditolak; no stale privilege |

Test harus membuktikan Hub hanya `/hubs/activity`, tidak ada arbitrary group atau per-tool hub, dan correctness job tidak bergantung pada event delivery. Jika realtime tetap unavailable, bounded visibility-aware REST polling boleh memulihkan status tanpa membuat polling storm.

# 9. Worker/IPC testing

## 9.1 Contract dan negative tests

- Handshake valid dengan ContractVersion/CapabilityVersion/worker instance yang cocok → operation dapat dipertimbangkan.
- Unknown operation, Feature ID invalid, message type/discriminator tidak dikenal, extra field, schema/version mismatch, oversized/truncated frame → fail closed; no dynamic dispatch/deserialization.
- Correlation ID/Job ID/request ID/deadline/cancellation tidak cocok atau stale → reject; tidak menempel ke job lain.
- Impersonated/cross-user/remote/same-user rogue client, stale worker instance, wrong application package/process identity → IPC connection/operation ditolak sesuai boundary platform.
- Operation allowlist mencoba shell, SQL, arbitrary filesystem/path, network/URL, provider, token, atau child process tidak diizinkan → reject sebelum adapter invocation.
- Worker output salah type/root/size/count atau mengandung unsafe active content → output validator menolak; no success publication.
- Cancel, timeout, crash, OOM/resource wait, restart/recycle → typed status stabil; source tetap; partial/temp non-final; recovery/checkpoint hanya bila binding valid.
- Malformed/flooding peer → bounded read/message/queue, disconnect offender, safe telemetry, no unbounded allocation/hot loop.
- End-user access/refresh token tidak pernah dikirim ke Local/Server Worker atau provider.

## 9.2 Pemisahan lapisan bukti

| Bukti | Lapisan |
|---|---|
| Validator, operation allowlist, message/version/state/error mapping, correlation propagation | Unit/contract dengan fake transport |
| Real serializer/framing, cancellation/deadline, crash/restart, temp isolation | Integration dengan real worker process terisolasi |
| Named Pipe ACL/current-user/app/package/process identity, remote rejection, rogue/stale instance | Security PoC pada Windows target |
| Server workload identity, queue authorization, instance/job binding, provider/storage scope | Server integration/security test |
| CPU/RAM/time/temp/output/child-process containment dan recycle | Performance/containment PoC |

Unit test tidak boleh diklaim membuktikan OS ACL atau containment proses nyata. PoC yang gagal menghasilkan controlled limitation/alternative decision, bukan silent acceptance atau penghapusan fitur.

# 10. Compatibility testing

Compatibility diuji terhadap matriks yang terversi per platform/channel. Exact client, OS, browser, runtime, engine, dan device versions merupakan configuration test data yang disetujui; tidak di-hardcode ke business assertions.

| Status API | Baseline expected behavior | Test minimum |
|---|---|---|
| `SUPPORTED` | Client beroperasi normal sesuai exposure/capability/policy | Current and supported older client; allowed Feature contracts; no update blocker |
| `UPDATE_AVAILABLE` | Operasi yang masih kompatibel tetap berjalan; notice update non-blocking dan accessible | CurrentVersion lebih baru tetapi client ≥ MinimumSupportedVersion; no forced denial without real requirement |
| `UPDATE_REQUIRED` | Incompatible/security-sensitive server interaction diblok secara aman dan user diarahkan update; local data tidak dirusak | Client di bawah required gate; no stale server action/queued dispatch; recovery guidance safe |
| `UNSUPPORTED` | Unsupported client/API major/contract/platform combination fail closed tanpa crash atau data loss | Unknown major, removed contract, unsupported channel/platform; no silent downgrade |

Forced update hanya lulus bila fixture menyatakan security/compatibility requirement nyata. Test harus mencegah Administrator/feature flag secara arbitrer mengubah hard compatibility/safety boundary.

## 10.1 Matriks compatibility minimum

- N server vs current client; N server vs oldest supported client; N server vs client below minimum; newer client vs older supported server where release topology permits.
- Platform/channel-specific `CurrentVersion` dan `MinimumSupportedVersion`; salah platform header tidak boleh memperoleh capability client lain.
- OS/runtime/browser/engine/provider version supported, deprecated, missing, incompatible, dan partially capable.
- Existing secured file/history/job/result created by older supported version tetap dapat dibaca/ditangani sesuai migration path.
- Feature exposure tersedia tetapi device capability/engine/storage/connectivity tidak memenuhi → explain/fallback/hide sesuai canonical resolver; bukan crash.
- Upgrade/downgrade boundary, rollback deployment, mixed-version node, cached registry, stale capability, and long-running job across rollout.
- Accessibility/localization/timezone/locale tidak mengubah security/compatibility decision.

# 11. Versioning tests

| Versioned contract | Skenario wajib | Expected result |
|---|---|---|
| API major `/api/v1` | Supported major, unknown/newer/removed major | Supported binds correctly; unknown fails explicitly; no heuristic downgrade |
| Client app/version | Equal/current, oldest supported, below minimum, newer-than-server | Empat status compatibility tepat per platform/channel |
| `ContractVersion` | Supported minor, unknown field, required field missing, incompatible major | Additive behavior tetap kompatibel; incompatible contract ditolak dengan safe status |
| Feature parameter schema | Old/new schema, Feature ID/schema mismatch, unknown parameter | Typed validation; no handler/provider invocation on mismatch |
| Workflow/configuration schema | Old graph migration, unknown node/type, cyclic/oversized plan | Compatibility/migration path atau safe rejection; no unsafe partial execution |
| `CapabilityVersion` | Stale cache, server registry change, provider disabled, reconnect | Refresh/re-evaluate; stale capability tidak dipakai untuk dispatch |
| `MinimumSupportedVersion` | Threshold below/equal/above and platform-specific | Boundary tepat; equal tidak salah ditolak; update state konsisten |
| SignalR `eventVersion` | Supported, unknown, newer, stale event after deploy | Unknown/newer → safe telemetry + REST reconcile; no crash/state regression |
| IPC/engine contract | Client–Worker compatible/incompatible, stale worker, provider capability/version change | Handshake rejects mismatch; resolver re-evaluates; no dynamic fallback bypass |
| Audit `EventVersion` | Old/current/new event mapper and export reader | Known versions readable; unsupported/poison retained safely; no silent misinterpretation |
| Checkpoint/result version | Old/incompatible Feature/schema/input/engine/stage/digest/expiry | Resume only on full binding; otherwise safe restart/failure; partial not final |
| Cursor/idempotency | Cursor created before scope/schema change; fingerprint across compatible client | Cursor scope/version verified; replay semantics stable; secret excluded |

SignalR reconnect after version change harus menjalankan urutan: auth/account/session refresh → compatibility/capability refresh → REST authoritative reconcile → authorized resubscribe. Reconnect tidak mengirim ulang execute command. Contract test pipeline perlu mendeteksi breaking OpenAPI/schema/event/IPC changes dan menuntut migration/compatibility path sebelum release.

# 12. Desktop/Web/Mobile/Tablet matrix

Matriks di bawah menetapkan behavior, bukan daftar versi perangkat. Versi OS/browser/device yang benar harus ditetapkan dalam supported-platform configuration dan test inventory.

| Area | Windows Desktop | Responsive Web | Phone | Tablet |
|---|---|---|---|---|
| Product posture | Primary full-feature client | Account/history/sync/admin/audit/support/browser-safe tools | Curated quick/practical feature set | Curated, lebih kaya dari phone |
| Layout/overflow | Window min/default/max, DPI scaling, multi-panel, long localized text; no clipped security prompt | Breakpoints, zoom, narrow/wide viewport; no horizontal overflow except intentional data grid | Small portrait/landscape, safe-area, keyboard-open state; no hidden action | Portrait/landscape, split-view where supported, touch targets; no overlay collision |
| Navigation | Keyboard/mouse, focus order, back/close, deep-link validation | Browser back/forward/refresh, route guard, multi-tab session | Touch/back gesture, resume, share/deep-link validation | Touch/stylus, optional keyboard/mouse, orientation transition |
| Capability detection | Local Worker/engine/file system facts; stale capability refresh | Browser and server capability; never assume local Worker/path | Camera/share/storage permission and curated features | Camera/stylus/share and curated features |
| Unavailable feature | Explain platform/capability/policy/resource/connectivity reason without revealing sensitive detail | Explain browser-safe/server-required limitation | Explain why desktop/server action is unavailable; no dead control | Same, adapted to available capability |
| Input | File picker, drag/drop, mouse, keyboard shortcuts; validate all paths | Browser picker/drop where allowed; no raw local path trust | System picker/camera/share input; revoke permission handling | Picker/camera/share/stylus; optional hardware keyboard |
| Execution | LOCAL uses in-process/Local Worker; server only per resolver/consent | Server/browser-safe path per policy; no implicit local engine | Curated local/server route based on capability; no auto-upload | Curated route based on capability; no auto-upload |
| Offline | Eligible local action only; protected grant; no privileged action | Only behavior explicitly supported; no assumed business offline | Eligible curated local action only; queue profile-isolated | Same; orientation/resume must not trigger upload |
| Reconnect | Refresh identity/policy/capability/compatibility before queue; REST reconcile | Refresh route/session and Hub; multi-tab consistency | Resume/network switch; no duplicate share/upload | Resume/orientation/network switch; no duplicate execution |
| File/result UX | Large file/batch/progress/cancel/recovery | Bounded upload/download, tab close/reload recovery | Small/curated inputs, share/export result | Curated/bigger view, stylus/touch result review |
| Security UX | Reauth/confirmation focus-safe; password never persisted; clipboard controlled | CSRF/CORS/session/XSS protections; no token in browser-accessible storage | Secure credential storage; screen/background/resume handling | Same plus multi-window/split-view exposure checks |
| Accessibility | Keyboard-only, focus indicator, screen reader labels, high contrast, DPI | Semantic navigation, zoom/reflow, focus restoration | Screen reader, dynamic text, touch target, rotation | Screen reader, dynamic text, touch/stylus target |

## 12.1 Cross-platform scenario set

1. Same Feature ID/input/policy produces equivalent validation and permission decision across exposed platforms.
2. A platform-hidden Feature cannot be invoked by deep link, crafted route, Assistant, or direct API without normal authorization/capability checks.
3. Orientation, resize, zoom, virtual keyboard, long safe error text, and translated labels never hide confirmation, cancel, privacy choice, or issue screenshot preview.
4. Pointer, touch, keyboard, stylus, drag/drop, share target, and file picker all pass through the same file/type/path/security validation.
5. Permission denial, unavailable capability, offline, update required, provider unavailable, rate limit, and resource wait are distinguishable and actionable without exposing sensitive system detail.
6. App sleep/background/terminate/resume does not duplicate upload/execution, reuse stale permission, leak content in preview, or lose persisted job truth.
7. Test automation may cover viewport/navigation/contract behavior; representative approved real devices remain necessary for camera/share/picker, OS credential store, orientation, accessibility, and lifecycle evidence.

# 13. Security scanning

Tidak ada product/vendor scanner yang ditetapkan oleh laporan ini. Semua nama produk, standard configuration, severity mapping, waiver authority, dan evidence retention adalah `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

| Scan/activity | Scope | Cadence/gate rekomendasi | Evidence |
|---|---|---|---|
| Static application security analysis | .NET/client/server/worker code and generated code policy | PR/CI dan release; changed code plus periodic full scan | Versioned result, triage, owner, remediation/waiver |
| Compiler/analyzer/secure coding rules | Nullability, unsafe APIs, deserialization, path/process/network/crypto misuse | Build gate sesuai severity | Build log aman dan rule baseline |
| Dependency vulnerability + license | Direct/transitive packages, runtime, engine/converter/provider binaries | PR/restore/build, scheduled refresh, pre-release | Dependency graph, advisory, license disposition |
| Secret scan | Source, history according to approved scope, config/template, build artifact | Pre-commit/PR/CI/release | Finding without exposing secret; rotation/removal evidence |
| SBOM and artifact integrity | Application, Worker, server, images/package where applicable | Release candidate | SBOM format/signature/provenance per Pertamina confirmation |
| Configuration/deployment vulnerability scan | TLS, headers, CORS/CSP, proxy, identity, storage, queue, logging, environment separation | QA/Pilot/pre-release and material change | Environment-specific sanitized baseline |
| DAST/API security | Web/API auth/authz/schema/upload/download/query/error/rate | Authorized QA; release and material attack-surface change | Scan config, accounts/scope, result, manual validation |
| Container/host/IaC scan | Hanya bila deployment artefact tersebut digunakan | Build/deploy gate sesuai platform | Applicable-scope record; no invented control |
| Malicious corpus/fuzz | Parser/converter/IPC/serialization/HTML/OCR boundary | Scheduled regression and engine/provider upgrade | Corpus manifest/hash, crash/resource/leak result |
| Privacy canary scan | Logs/telemetry/audit/events/issues/artifacts/provider capture | Per security regression and release | Zero forbidden canary or documented fixed finding |

Automated scan bukan pengganti threat-model test, authorization matrix, manual review, PoC containment, atau VA/PT. False positive harus ditriase; suppression/waiver memiliki alasan, approver berwenang, scope, compensating control, expiry, dan revalidation. Temuan Critical/High mengikuti remediation dan retest atau formal exception yang dibatasi waktu sesuai Security Design.

# 14. VA/PT readiness

## 14.1 Scope minimum

Web, API, Pertamina Identity integration, authorization/ownership, file transfer/storage/result, job/idempotency/race, SignalR, secured-file/version, issue/support, admin/config/policy, audit/export, Desktop local storage, IPC/Local Worker, server Worker/workload identity, HTML/Web-to-PDF, document engines/converters/OCR, Assistant/approved provider boundary, deployment/config/secret/observability.

## 14.2 Readiness checklist

- Written authorization, environment, dates, source IP, named testers, scope/exclusions, contact, escalation, and rules of engagement disetujui.
- Gunakan pre-production yang representatif; active exploit/DoS pada Production dilarang kecuali ada otorisasi khusus terpisah yang eksplisit.
- Accounts/permissions tersedia untuk User A/B, limited Administrator, read-only Auditor, scoped Support, dan scoped workload identity; tidak ada unlimited root profile.
- Synthetic data/canary dan malicious corpus tersedia; tidak ada real sensitive data.
- API/OpenAPI, Feature registry/schema, event/IPC contract, architecture/trust boundary, role/permission matrix, test endpoints, and supported version matrix tersedia kepada tester secara terkontrol.
- Rate/load/large-file/race/request-smuggling/slow-consumer test mempunyai safety ceiling, stop condition, recovery owner, dan monitoring real time.
- Outbound canary sink internal disiapkan untuk SSRF/HTML/provider exfiltration test tanpa internet/public AI.
- Logging/evidence capture aman dan cukup untuk Correlation ID/Job ID tanpa token/body/content; clock synchronization and retention disepakati.
- Backup/recovery point, cleanup/tombstone test plan, data reset, and post-test secret/account revocation disiapkan.
- Incident/escalation path jelas jika tester menemukan active credential, cross-user content, remote execution, persistence, atau service instability.
- Temuan mempunyai severity/risk owner, reproducible safe steps, impacted contract/boundary, remediation target, retest status, dan formal exception expiry bila ada.

## 14.3 Release condition

VA/PT wajib sebelum Production dan setelah perubahan material pada attack surface/batas keamanan sesuai persetujuan. Critical/High memerlukan perbaikan dan retest, atau formal time-bound exception oleh authority Pertamina. Provider, metode, cadence, independensi tester, retensi bukti, dan otoritas risk acceptance tetap memerlukan konfirmasi Pertamina.

# 15. Risks/gaps

| Risiko/gap | Dampak pada test plan | Keputusan/aksi yang dibutuhkan |
|---|---|---|
| Official Pertamina Identity test tenant, MFA/reauth method, token/session/revocation semantics belum dikonfirmasi di laporan ini | Expected 401/403 dan session lifecycle tidak dapat dibuktikan penuh | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` dan non-production identity integration |
| Exact Windows/OS/browser/phone/tablet versions dan device inventory belum dikunci | Matriks compatibility tidak dapat diberi angka/versi konkret | Integrator menetapkan supported-platform matrix, ownership, dan refresh cadence |
| Behavioral distinction `UPDATE_REQUIRED` vs `UNSUPPORTED`, khususnya akses local recovery/read-only, belum dirinci dalam sumber | Risiko inconsistent UX/fail-open/fail-closed antar client | Integrator mengunci action matrix tanpa merusak local data; forced update hanya untuk kebutuhan nyata |
| Security scanner/vendor, rule pack, severity mapping, waiver authority, SBOM/signing format belum diketahui | Tool/cadence/gate tidak dapat dinyatakan sebagai standard Pertamina | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; laporan ini tetap vendor-neutral |
| Real Named Pipe ACL, process identity, resource/job binding, CPU/RAM/temp/child-process containment belum dapat dibuktikan oleh unit test | Risiko Worker impersonation/escape/resource starvation | PoC Windows dan integration test proses nyata sebelum release fitur terkait |
| Actual reverse proxy/IIS/load balancer parsing topology belum dikunci | Request smuggling/forwarded header/Host tests mungkin tidak representatif | Uji end-to-end pada topology QA yang sama dengan deployment target |
| Approved AI/Content Intelligence provider, egress, retention, model/version behavior belum final | AI minimization/privacy/provider-failure tests belum lengkap | Provider-specific security PoC setelah approval; public fallback tetap dilarang |
| Engine/parser/converter versions dan safe malicious corpus management belum dikunci | Parser exploit/regression tidak dapat di-cover stabil | Corpus governance, isolated host, upgrade gate, hash/manifest, and incident procedure |
| Rate/quota/resource exact values bersifat policy/config | Test hardcoded akan cepat usang | Parameterized below/equal/above fixtures; hard ceiling diuji terpisah |
| Shared QA dapat terdampak load, bomb, race, SignalR flood, atau DAST | False outage dan risiko data lintas tim | Dedicated capacity window/environment, stop condition, observability, cleanup |
| E2E evidence dan scan report sendiri dapat membocorkan canary/sensitive context | Test pipeline menjadi exfiltration channel | Evidence minimization, safe mapper, access control, retention, canary scan |
| Real device picker/share/camera/secure storage/background behavior tidak sepenuhnya terwakili emulator | Compatibility/security defect lolos | Representative approved physical-device run sebelum supported claim |
| Administrator/Auditor/Support permission bundle dapat berubah | Role-name assertions menghasilkan false confidence | Test berdasarkan permission fixture dan resource scope; role label bukan oracle |
| Offline revocation memiliki inherent delay sampai expiry/reconnect | Residual stale privilege pada local eligible action | Test absolute expiry/trusted time/narrow scope/reconnect revalidation; document residual risk |
| Document/HTML/OCR/AI output sangat bervariasi | Corpus tidak pernah lengkap | Risk-based mutation/fuzz, regression from sanitized incidents, containment, and safe failure |
| VA/PT provider/method/cadence/evidence retention belum dipilih | Readiness bisa selesai tetapi assessment belum terlaksana | Pertamina menentukan authority dan engagement; wajib sebelum Production |

Tidak ada konflik yang membenarkan penghapusan fitur. Risiko server kecil ditangani melalui bounded input, batching, pagination, admission control, queue, backpressure, concurrency limit, async job, cancellation, cleanup, dan graceful degradation.

# 16. Suggested Document 12 text

Berikut teks ringkas yang dapat dipertimbangkan Integrator untuk Document 12:

---

## Security, Privacy, Compatibility, and Cross-Platform Test Baseline

Pengujian keamanan menggunakan lapisan Unit, Contract, Integration, Authorization, Abuse/Negative, E2E, Performance/DoS, Static/Supply-Chain, PoC, dan VA/PT. Unit Test membuktikan decision logic; real identity middleware, HTTP/SignalR wire, OS credential store, Named Pipe ACL, Worker process containment, reverse proxy, engine/provider binary, browser, dan perangkat nyata dibuktikan pada lapisan integrasi/PoC/compatibility yang sesuai.

Semua security test aktif dilakukan hanya pada environment yang diotorisasi dan menggunakan synthetic accounts, canary, serta malicious-file corpus yang aman. Exploit Production dan real sensitive data dilarang. Load, flood, file-bomb, request-smuggling, fuzz, dan DAST memerlukan scope, safety ceiling, stop condition, observability, recovery owner, dan cleanup.

### Authentication and Authorization

Test wajib mencakup unauthenticated, invalid/expired/revoked credential, disabled account, logout, refresh replay/concurrency, PKCE/state/nonce/redirect mismatch, reauthentication, offline grant tamper/expiry/clock rollback/account switch, serta reconnect validation. Setelah reconnect, identity, account, session, permission, policy, capability, dan compatibility di-refresh sebelum queued command atau authorized resubscription.

Authorization diuji secara langsung pada API untuk wrong permission, wrong owner, stale permission, policy/classification deny, list/count/cursor/range/download/export/subscription, and cross-user object references. Administrator tidak otomatis dapat membaca document content. Auditor default controlled read-only dan tidak dapat mengubah Audit, policy, atau user data. Support hanya dapat mengakses issue, explicit attachment/screenshot, dan safe diagnostics sesuai permission/scope; Support/Superadmin bukan God Mode. Denial harus terjadi sebelum content/storage/provider resolution.

### File, Worker, API, SignalR, and AI Security

Malicious-file suite mencakup malformed/truncated PDF, oversized input, wrong MIME, fake extension/polyglot, traversal/UNC/device/ADS/symlink/reparse/TOCTOU, archive/decompression bomb, malicious Office macro/OLE/DDE/link, malicious HTML/remote fetch/SSRF, corrupted image, invalid OCR input, parser crash, output validation, and cleanup. Source harus immutable; partial/temp bukan final; CPU/RAM/time/temp/output/child-process dibatasi.

API suite mencakup `401`, `403`, concealed `404`, closed schema validation, injection, CSRF/CORS/CSP/Origin where applicable, upload/download ownership, cursor scope, idempotency replay/conflict, race, rate/quota/backpressure, request smuggling through the actual authorized proxy, and safe error/health response. `429` dilengkapi valid `Retry-After`; hard size, state/quota conflict, dan shared saturation dibedakan sebagai `413`, `409`, dan `503` sesuai contract.

SignalR diuji untuk authentication/origin, server-derived group, cross-user leakage, permission revocation, forbidden payload, duplicate/stale/reordered/gap/unknown events, slow consumer/flood, outage/reconnect, and REST reconciliation. Hub tidak boleh melakukan business mutation dan reconnect tidak mengirim ulang execute command.

Worker/IPC diuji untuk typed/versioned handshake, identity/ACL, closed operation allowlist, schema/Feature/job/instance/correlation/deadline validation, malformed/flood input, arbitrary path/network/SQL/shell denial, crash/recycle/resource containment, safe error, and output validation. End-user token tidak diteruskan ke Worker/provider.

Assistant/AI suite mencakup direct/indirect prompt injection, document/OCR/HTML/metadata/filename instruction, invented Feature ID, invalid/extra parameter, permission/ownership bypass, ambiguous destructive action, confirmation/reauth bypass, data exfiltration, provider failure, malicious response, hallucinated command/success, and public-provider rejection. Assistant selalu memakai canonical Feature ID, typed parameter, official execution service, normal authorization/policy/capability/confirmation, minimum required context, approved local/internal provider, and actual validated result. AI input/output diperlakukan untrusted.

### Privacy

Synthetic CanarySet untuk password, token/cookie, secret/key, document content, full OCR/translation/Markdown, signature, clipboard, prompt/provider payload, issue screenshot/attachment, raw path/storage key/URL, SQL/stack/claim dump harus tidak muncul pada API error, log, exception, trace, metric, health, Audit/outbox/export, SignalR, idempotency/cache/checkpoint/history, issue diagnostics, provider capture, filesystem artifact, atau test report. Screenshot issue hanya dikirim setelah explicit capture, preview, dan send. Closed typed allowlist adalah kontrol utama; sanitizer failure harus fail closed.

### Compatibility and Platforms

Compatibility endpoint dan clients diuji untuk `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, dan `UNSUPPORTED` menggunakan `CurrentVersion`, `MinimumSupportedVersion`, `ContractVersion`, dan `CapabilityVersion` per platform/channel. Forced update hanya digunakan untuk kebutuhan security/compatibility nyata. Test mencakup oldest supported client, below-minimum client, newer server/client where applicable, stale capability, long-running job during rollout, API/Feature/workflow/configuration/engine/event/IPC/checkpoint versions, dan SignalR reconnect after version change. Breaking change memerlukan migration/compatibility path.

Windows Desktop, Responsive Web, Phone, dan Tablet diuji untuk layout/overflow, navigation, zoom/orientation/lifecycle, capability detection, unavailable-feature explanation, touch/mouse/keyboard/stylus, picker/drop/camera/share input, offline/reconnect, accessibility, dan security UX. Feature yang hidden/unavailable pada platform tidak boleh dapat bypass melalui route, deep link, Assistant, atau direct API. Representative physical devices tetap diperlukan untuk OS credential store, picker/share/camera, background/resume, orientation, dan accessibility claims.

### Scanning and VA/PT

Security pipeline mencakup static analysis, secure-code analyzers, dependency vulnerability/license scan, secret scan, SBOM/artifact integrity, configuration/deployment scan, authorized DAST/API security test, parser/IPC fuzz/malicious corpus, and privacy-canary scan. Product/vendor, rule set, severity mapping, waiver authority, SBOM/signing format, cadence, dan evidence retention adalah `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

VA/PT wajib sebelum Production dan setelah material attack-surface/security-boundary changes sesuai persetujuan. Scope mencakup Web/API/auth/authz/files/storage/jobs/SignalR/secured/support/admin/audit/config, local storage/IPC/Worker, document/HTML/OCR engines, approved AI/provider boundary, deployment/config/secret/observability. Critical/High memerlukan remediation dan retest atau formal time-bound exception oleh authority yang disetujui.

### Acceptance

Security test dinyatakan lulus jika seluruh P0 lulus, tidak ada cross-user/role/profile leakage, tidak ada forbidden canary pada seluruh sink, official Feature execution tidak dapat dibypass, unauthorized request berhenti sebelum sensitive dependency, compatibility dan cross-platform gates lulus, server tetap bounded saat abuse, recovery tidak merusak data, dan seluruh Critical/High telah diremediasi/retest atau memiliki formal exception yang masih berlaku.

---

**Keputusan tetap milik User dan Integrator Document 12.**
