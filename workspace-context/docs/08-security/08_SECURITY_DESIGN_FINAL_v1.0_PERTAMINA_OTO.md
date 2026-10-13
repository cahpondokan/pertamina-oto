# 08 — DESAIN KEAMANAN
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Versi:** 1.0  
> **Tanggal:** 24 Agustus 2026  
> **Status:** FINAL — PRE-CODING BASELINE  
> **Keputusan keamanan:** 20/20 AUTO-LOCKED  
> **Baseline fitur canonical:** 550  
> **Prioritas penyampaian utama:** 33 Primary PDF Tool Cards

---

# 1. Tujuan Keamanan

PERTAMINA OTO wajib:

1. melakukan autentikasi hanya melalui batas identity Pertamina yang disetujui;
2. mengotorisasi setiap tindakan dan sumber daya pada batas server/aplikasi;
3. mempertahankan Offline-First dan Local-First yang memperhatikan capability tanpa menjadikan cached state sebagai kewenangan enterprise;
4. melindungi source, draft, result, cache, checkpoint, OCR, tanda tangan, lampiran Issue, dan isi secured-file sepanjang siklus hidupnya;
5. membatasi isi PDF, Office, image, HTML, OCR, clipboard, scanner, dan provider yang berbahaya/tidak tepercaya;
6. mencegah privilege escalation horizontal/vertikal serta memisahkan administrasi dari akses isi;
7. menjaga Assistant/AI tetap dalam application service typed canonical;
8. mencegah kebocoran password, token, secret, dokumen, OCR, clipboard, tanda tangan, dan screenshot;
9. tetap tersedia saat terjadi penyalahgunaan/tekanan sumber daya melalui antrean terbatas, limit, isolasi, dan degradasi aman;
10. menghasilkan bukti keamanan yang dapat diuji sejak desain hingga rilis, operasi, insiden, recovery, dan update.

Keamanan wajib mempertahankan alur bisnis dan pekerjaan lokal yang dapat digunakan. Setiap kontrol wajib memiliki risiko, fase implementasi, bukti, pemilik, dan perilaku kegagalan yang jelas. Security theater, kontrol tanpa batas, dan penghapusan fungsi bisnis secara tersembunyi dilarang.

---

# 2. Ruang Lingkup, Batas, dan Asumsi

## 2.1 Termasuk dalam Ruang Lingkup

- SSO Pertamina, session, token, logout, revocation, disabled account, dan otorisasi luring;
- model otorisasi berbasis permission/sadar objek dan reauthentication;
- Web, API, SignalR, Desktop, Mobile/Tablet, Local Worker, Server Worker, dan typed local IPC;
- keamanan SQL Server, SQLite, filesystem lokal, file/object storage aman, transfer, backup, dan restore;
- validasi file dan ancaman dokumen/conversion/OCR/HTML/PDF;
- Assistant, Content Intelligence, prompt injection, provider, dan minimisasi data;
- secret, encryption, key, logging, telemetry, Issue, batas Audit, dan kontrol penyalahgunaan;
- secure coding, dependency/supply chain, CI/CD, environment, deployment, testing, VA/PT, vulnerability, prinsip insiden, retention, dan gate rilis.

## 2.2 Di Luar Ruang Lingkup

- matriks akhir Role × Permission × Action — Document 09;
- desain akhir Audit Event × Field × Retention × Reporting — Document 10;
- penerimaan kinerja numerik — Document 12 setelah benchmark;
- playbook/kontak insiden operasional — Document 16;
- topologi deployment, RPO/RTO, DR/runbook terperinci — Document 17;
- penandatanganan dokumen certificate/PKI — scope produk tetap visual signature/paraf non-PKI;
- pemilihan vendor IAM, kriptografi, storage, scanning, AI, malware, atau keamanan baru.

## 2.3 Asumsi

- Documents 01–07 bersifat berwenang dan FINAL.
- Pertamina akan menyediakan/menyetujui standar identity enterprise, classification, cryptography, key/secret, infrastructure, dan operational yang diperlukan untuk Produksi.
- Endpoint/device dapat hilang atau dikompromikan; remote revocation luring tidak terjadi seketika.
- Malware dengan pengguna yang sama dan zero-day parser native tidak dapat dihilangkan sepenuhnya; isolasi, least privilege, minimisasi, monitoring, dan recovery mengurangi dampak.
- Properti keamanan tetap wajib meskipun produk/provider tepatnya ditunda.

---

# 3. Prinsip Keamanan

```text
Zero Trust at important boundaries
Least Privilege
Defense in Depth
Secure by Default
Deny by Default
Validate at Every Trust Boundary
Minimize Sensitive Data
Separation of Duties
Fail Securely
Audit Important Security Actions
Never Trust File Content
Never Trust AI Output
Preserve Source and Recoverability
Bound Resource Use
Evidence Before Release
```

Identity, claim, permission, Feature, versi contract, handler, resource state, policy, provider, type IPC/event, atau dependency keamanan yang tidak dikenal bersifat fail-closed. Capability lokal yang aman dapat tetap tersedia saat server gagal, tetapi sistem tidak pernah beralih ke akses anonim, otorisasi privileged stale, AI publik, SQL/storage langsung, atau validasi yang dikurangi.

---

# 4. Baseline dan Keputusan Berwenang

| Induk | Baseline keamanan yang dipertahankan |
|---|---|
| Document 01 | SSO, offline continuity, role boundaries, secure documents, issue/support, audit |
| Document 02 | `SR-SEC-001..023`, functional/API/performance/recovery/DevOps requirements |
| Document 03 | 550 canonical capabilities and 33 Primary PDF Tool Cards |
| Document 04 | Modular Monolith, Local Worker, REST/SignalR, Hybrid Storage, Assistant boundary |
| Document 05 | .NET/WinUI/MAUI/EF/SQLite, dependency/security scan, test baseline |
| Document 06 | Hybrid typed API, Pertamina auth, file/job/idempotency, REST authority, SignalR event, layered admission |
| Document 07 | module data ownership, SQL/object placement, local SQLite boundary, max-three versions, lifecycle controls |

Keputusan keamanan yang dikunci adalah `SEC-DEC-001..020` dalam `SECURITY_DECISION_REGISTER.md`.

---

# 5. Arsitektur Keamanan dan Batas Kepercayaan

```text
UNTRUSTED / PARTIALLY TRUSTED DEVICE
Browser / Desktop / Mobile / Tablet
files, camera, clipboard, OCR, IDs, Feature plans, local clock
        │
        │ approved SSO + HTTPS
        ▼
PERTAMINA IDENTITY BOUNDARY
authenticates subject; does not grant OTO resources
        │
        ▼
OTO APPLICATION SECURITY BOUNDARY
Reverse Proxy / Web / API / SignalR
authentication → session/account → permission → ownership/scope
→ policy/classification → state/version → consent → reauthentication
        │
        ├─────────────► module-owned SQL/object storage
        │
        └─────────────► persisted job/outbox
                              │
                              ▼
                     INTERNAL WORKLOAD BOUNDARY
                     queue / Worker / engine / adapter
                     separate workload identity
                              │
                              ▼
                     approved internal provider only

LOCAL DEVICE BOUNDARY
Desktop UI/application service
        │ authenticated typed/versioned local IPC
        ▼
isolated Local Worker
        │ job-scoped references only
        ▼
protected per-job temp and local engines
```

## 5.1 Aturan Kepercayaan

| Batas | Dipercaya untuk | Tidak pernah dipercaya untuk |
|---|---|---|
| Pertamina Identity | Subjek terautentikasi/claim yang disetujui dan tervalidasi | Ownership sumber daya OTO, permission Feature/isi, resource state |
| Client/UI | Intent pengguna dan presentation | Role, owner, policy, handler, storage path, hasil reauthentication |
| Profile luring | Capability lokal layak dalam grant terlindungi yang valid | Kewenangan server/admin/Support/Audit atau kebenaran revocation saat ini |
| API | Otorisasi/admission aplikasi | Kepercayaan buta pada token, ID, queue, provider, event, atau jaringan internal |
| SQL/object storage | Data dipersistenkan setelah otorisasi owner | Akses hanya berdasarkan kepemilikan key/path/ID |
| Worker/provider internal | Pekerjaan sesuai scope Job berdasarkan workload identity | Token pengguna yang diteruskan atau perintah klien arbitrer |
| SignalR | Notifikasi bahwa state REST mungkin berubah | Mutasi bisnis, grant otorisasi, state berwenang |
| UI Admin/Support/Auditor | UI untuk tindakan yang diotorisasi terpisah | Privileged trust zone atau akses isi otomatis |

---

# 6. Aset dan Klasifikasi Data

Aset terlindungi mencakup identity/session/permission; source/draft/result; upload/checkpoint/secured version; isi signature/password/OCR/scanner/clipboard/Issue; contract/policy/confirmation Feature; Job/idempotency/outbox/Audit; database/storage/IPC/Worker/provider; secret/key; build/artefak/SBOM; dan kapasitas availability.

Kelas kerja engineering berikut tidak menggantikan label resmi Pertamina.

| Kode | Kelas kerja | Contoh | Kontrol default |
|---|---|---|---|
| `I` | Operasional internal | Feature ID, status aman, version, metrik agregat | Akses internal, minimisasi, integritas |
| `C` | Metadata rahasia | Subject, nama/path file, Job/Correlation ID, referensi storage | Auth, object scope, encryption bila wajib, kueri/export/retention terkendali |
| `R` | Isi terbatas | Dokumen, OCR, pesan/screenshot Issue, tanda tangan, checkpoint isi | Batas isi kuat, local-first, transfer eksplisit, secure storage, least privilege |
| `X` | Kredensial/rahasia | Password PDF, access/refresh token, service secret, encryption key | Memory atau credential/key store yang disetujui; bukan persistensi/log biasa |
| `A` | Bukti audit terkendali | Peristiwa security/bisnis enterprise | Integritas berorientasi append, read/export/purge terbatas |

Isi turunan mewarisi kelas sumber tertinggi yang berlaku, kecuali aturan transformasi irreversible yang disetujui menyatakan lain. Classification berlaku pada nama file, metadata, thumbnail, preview, OCR, cache, index, diagnostics, backup, dan export—bukan hanya file utama.

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`: petakan kontrol kerja tersebut ke label resmi Pertamina serta aturan handling, sharing, retention, disposal, dan encryption sebelum Produksi.

---

# 7. Pelaku Ancaman dan Model Ancaman

## 7.1 Pelaku Ancaman

- penyerang jaringan tanpa autentikasi;
- pengguna terautentikasi yang mencoba user/role/resource lain;
- session browser/native atau device yang dicuri/dikompromikan;
- proses berbahaya pada device yang sama, malware, atau account OS berbeda;
- isi PDF/Office/image/archive/HTML/font/OCR/Issue berbahaya;
- engine, dependency, input build, Worker, atau provider yang dikompromikan/bermasalah;
- klien retry/replay/bot/flood;
- pengguna jujur yang memberi instruksi Assistant ambigu/destruktif;
- response AI/provider yang dimanipulasi atau berbahaya;
- operator dengan privilege berlebihan atau kesalahan konfigurasi.

## 7.2 Daftar Ancaman

| Ancaman | Pintu masuk utama | Dampak | Kontrol wajib |
|---|---|---|---|
| Forged/stolen identity | API/Hub/session callback | Impersonation | Strict validation, PKCE/state/nonce, protected credential, lifetime/revocation |
| IDOR/BFLA | IDs/cursor/fileRef/job/attachment/export | Cross-user/privileged access | Permission + owner/scope + policy/state on every operation |
| Self/role escalation | Admin/permission/UI fields | Vertical escalation | Server-derived permissions, reauth, concurrency, separate content permission |
| Offline stale privilege | Cached capability/clock rollback | Post-revocation local access | Absolute expiry, trusted-time evidence, local-only scope, reconnect validation |
| SQL/command injection | Query/DTO/filename/engine argument | Data/code execution | Parameterization, allowlist, no shell, fixed registered operation |
| Path traversal/reparse | File/archive/output/cleanup | Arbitrary read/write/delete | Random names, final-handle root check, no-follow/link rejection |
| XSS/formula injection | Issue/OCR/Markdown/AI/export | Session/data compromise | Context encoding, inert renderer, CSP, safe spreadsheet cells |
| CSRF/CORS/WebSocket origin | Cookie Web/Hub | Unauthorized browser action/read | Anti-forgery, SameSite, exact Origin, no state-changing GET |
| HTTP smuggling/host spoof | Proxy/API parsing | Auth/cache/request confusion | Governed proxy, aligned limits, reject ambiguous framing, trusted forwarded headers |
| Unsafe deserialization | API/IPC/event/checkpoint/provider | Type confusion/RCE/DoS | Sealed typed/versioned schemas, no type names, depth/size bounds |
| Malformed parser input | PDF/Office/image/OCR/HTML | RCE/crash/corruption | Quarantine, isolated patched Worker, resource bounds, output validation |
| Decompression/pixel/page bomb | Containers/images/PDF/OCR | CPU/RAM/disk exhaustion | Expanded-size/ratio/count/depth/page/pixel/time/output limits |
| SSRF/remote fetch | Web/HTML renderer/provider | Internal network/metadata/data theft | Remote deny under current API; future typed egress/redirect/DNS/IP controls |
| Job/upload/SignalR flood | API/queue/Hub/range | Availability loss | Layered quotas, reservations, bounded fair queues, rate/backpressure |
| Replay/race | Idempotency/chunks/version/delete/job lease | Duplicate/lost state | Scoped fingerprint, unique constraint, ETag/rowversion/lease/short transaction |
| Worker/IPC spoof | Named Pipe/queue/service identity | Arbitrary operation/data access | Authenticated identity/instance, closed contract, job scope, limits |
| Prompt/tool hijack | User/document/OCR/provider output | Policy/tool bypass/exfiltration | Instruction/data separation, canonical closed tools, per-step deterministic controls |
| AI/provider leakage | Internal provider transfer/log | Restricted content exposure | Approval, minimization, purpose/consent, egress, temporary lifecycle, no content logs |
| Secret/log leakage | Error/log/dump/issue/build | Credential/content compromise | Field allowlist, redaction, secret scan, dump policy, canary tests |
| Supply-chain compromise | Package/model/native/build/update | Broad code/data compromise | Approved source, pin/provenance, SBOM, scans, artifact integrity, emergency disable |
| Backup resurrection | SQL/object/local restore | Deleted/unauthorized data returns | Cross-store permission/tombstone/key/digest/version reconciliation |
| Misconfiguration | Origin/egress/policy/limits/logging | Boundary bypass | Typed safe bounds, review/audit/drift tests, same artifact promotion |

Risiko residual dan penerimaannya dicatat pada Bagian 30.

---

# 8. Identity dan SSO Pertamina

## 8.1 Batas Identitas

```text
Approved Pertamina Identity
→ validated assertion/token
→ issuer + subject maps to internal PrincipalId
→ account/session eligibility
→ current permissions and policy
→ object-aware authorization
```

PERTAMINA OTO dilarang:

- membuat penyimpanan username/password paralel;
- mengumpulkan, mem-proxy, mencatat, atau mempertahankan password Pertamina;
- meniru halaman login enterprise;
- mengimplementasikan password recovery/account unlock independen;
- mempercayai email/display name/employee number/principal dari klien sebagai identity stabil;
- memilih metode IdP/MFA baru tanpa tata kelola.

Protocol/profile, issuer, audience, redirect URI, token type, claim yang disetujui, stable subject, provisioning subject tidak dikenal, logout, dan mekanisme revocation tepat berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 8.2 Alur Autentikasi Daring

```text
1. Check compatibility and local session state
2. Start approved SSO when online session is absent
3. Pertamina authenticates user
4. Validate state/nonce/PKCE and callback binding
5. API validates signature/reference, algorithm, issuer, audience,
   token type, lifetime, not-before, subject, approved client/claims
6. Map issuer + subject to PrincipalId
7. Verify principal active and application session not revoked/expired
8. Resolve current permission/policy
9. Authorize every resource/action
10. Return only safe profile/capability projection
```

Subject Pertamina yang valid tetapi belum dikenal tidak di-auto-provision oleh klien secara diam-diam. Policy provisioning memerlukan konfirmasi enterprise/bisnis.

## 8.3 Akun Dinonaktifkan dan Dicabut

- autentikasi invalid/hilang/expired/revoked → `401` aman;
- identity valid tetapi principal OTO disabled/tidak layak → `403 ACCOUNT_ACCESS_DENIED` aman tanpa reason/account enumeration yang tidak perlu;
- revocation menghentikan akses API/Hub baru dan membatalkan session aplikasi/luring ketika diketahui;
- Job yang sudah diterima tidak lagi mengekspos/mengontrol hasil kepada pengguna yang revoked;
- Worker memeriksa ulang policy pada batas aman dan membatalkan atau mengarantina output bila continuation dilarang;
- risiko residual luring dibatasi oleh absolute expiry dan permission local-only.

---

# 9. Session, Token, Web, dan Logout

## 9.1 Penempatan Kredensial

| Artefak | Lokasi yang diizinkan | Lokasi/penggunaan yang dilarang |
|---|---|---|
| Authorization code/PKCE verifier | Memory, one transaction | Persistence/log/reuse |
| Native access token | Protected memory/approved credential boundary if persistence required | SQL/SQLite/config/log/URL/issue/backup |
| Native refresh credential, if issued | Approved OS credential store | API resource call/browser/ordinary DB |
| Web browser session | Opaque `Secure`, `HttpOnly`, narrowly scoped cookie | Browser bearer/refresh exposure |
| Server Web credentials | Approved server credential boundary | Page/JS/Web Storage/service-worker cache |
| Application session row | Hashed reference, status, expiry/revocation metadata | Raw token/cookie/refresh credential |
| Offline grant | Protected minimum capability projection | API/Hub credential or raw claim dump |
| Workload credential | Approved server workload identity boundary | End-user impersonation/client distribution |

Token tidak pernah masuk URL/query string. SignalR Web melakukan autentikasi menggunakan cookie session same-site yang terlindungi. SignalR native menggunakan transport header yang disetujui. Jika platform tidak dapat memenuhinya, realtime tetap tidak tersedia dan fallback REST mempertahankan fungsi sampai remediation disetujui.

## 9.2 Refresh

- refresh hanya melalui Pertamina Identity yang disetujui;
- koordinasi single-flight mencegah refresh storm;
- retry terbatas dengan backoff/jitter untuk kegagalan sementara allowlisted;
- invalid grant, sinyal replay/reuse, revocation, disabled account, atau interaction-required menghentikan silent refresh;
- terapkan kredensial baru secara atomik dan resolve ulang permission/policy OTO saat ini;
- jangan beralih ke mode luring kecuali offline grant terlindungi yang valid secara independen mengizinkan tindakan lokal.

## 9.3 Login Once

“Login sekali” berarti renewal diam-diam yang disetujui selama enterprise grant, session aplikasi, account, client, dan policy tetap layak. Hal ini bukan autentikasi tanpa batas atau bypass reauthentication.

## 9.4 Logout

Logout bersifat idempotent dan:

1. menghentikan Hub dan dispatch perintah;
2. mencabut/membatalkan session aplikasi yang dapat dijangkau;
3. membersihkan kredensial dari memory/client store yang disetujui;
4. membatalkan otorisasi luring;
5. membersihkan/merotasi session Web;
6. mengunci/melepas profile lokal dan membersihkan key/state dalam memory;
7. mencatat metadata Audit yang aman;
8. tidak menghapus source, draft, atau pekerjaan yang dikelola aplikasi secara diam-diam.

Pemusnahan key/penghapusan account/device mengikuti policy retention/security eksplisit yang disetujui, bukan logout biasa.

---

# 10. Keamanan Luring

## 10.1 Grant Terlindungi Minimum

Hanya simpan:

- binding profile/principal internal dan external subject yang disetujui;
- waktu issued/last-online/absolute-expiry;
- projection capability/permission yang layak luring;
- versi policy/capability/contract dan safety ceiling;
- pembatasan classification;
- binding profile/device bila didukung;
- bukti integrity/version dan trusted-time/rollback;
- marker disable/revoke terakhir yang diketahui.

Jangan menyimpan token, cookie, password, kredensial service, claim dump tanpa batas, isi dokumen, atau permission server privileged.

## 10.2 Kelayakan

| Capability | Perilaku luring |
|---|---|
| Eligible deterministic local Feature on user-selected local files | Allowed while grant/policy valid |
| Local draft/preview/undo/history/recovery | Allowed within protected profile/data policy |
| Upload/server processing/secured storage/cross-device/issue/audit export | Wait/deny until online authorization |
| Admin/Support/Auditor server surfaces | Denied offline |
| Reauthentication-required action | Denied until online fresh authentication |
| Assistant local orchestration | Only offline-eligible local Features with identical checks |

## 10.3 Waktu dan Reconnect

- absolute expiry yang dapat dikonfigurasi; tanpa sliding/renewal luring;
- gunakan trusted last-online time dan monotonic elapsed time jika tersedia;
- wall-clock rollback, reboot/sleep/timezone/DST tidak dapat memperpanjang grant;
- bila integritas waktu tidak pasti, tindakan terlindungi bersifat fail-closed;
- reconnect me-refresh identity/account/session/permission/policy/capability/compatibility sebelum perintah dalam antrean;
- setiap perintah dalam antrean diotorisasi ulang dengan ownership/state/consent/reauthentication saat ini;
- intent dalam antrean bukan grant otorisasi sebelumnya.

---

# 11. Otorisasi, Model Permission, dan Reauthentication

## 11.1 Formula Otorisasi

```text
Allow = Authenticated
    AND AccountActive
    AND SessionValid
    AND ActionOrFeaturePermission
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND ClientCompatibilityAndCapability
    AND RequiredConsent
    AND FreshReauthenticationWhenRequired
```

Availability/visibility pada sisi klien hanya untuk UX. Keputusan akhir berada dalam API/application service, dengan policy ASP.NET Core untuk pemeriksaan reusable dan resource authorization handler untuk ownership/scope/state objek.

## 11.2 Dimensi Permission

| Dimensi | Aturan |
|---|---|
| Feature execution | Canonical Feature ID + exposure/schema/capability/classification/policy |
| Own resource | Permission plus owner check; permission alone is insufficient |
| Content access | Separate from administration/support/audit/monitoring |
| Administration | Granular read/write with hard safety bounds and reauth for high impact |
| Support | List/detail/attachment/triage/note/status/export separated |
| Audit | Query/detail/export separated; no delete/content implication |
| Operations | Safe metadata only by default |
| Workload | Non-human audience/purpose-scoped service operations |

Role mengelompokkan permission. `User`, `Administrator`, dan `Auditor` tetap menjadi persona baseline. “Superadmin” adalah bundle permission Support, bukan role baru dengan kewenangan tanpa batas. Kode/pemetaan/delegasi/segregation/self-assignment permission tepat ditetapkan dalam Document 09.

## 11.3 Aturan Sumber Daya

- turunkan owner dari principal terautentikasi, bukan request body;
- terapkan scope daftar sebelum count/filter/search/sort/cursor;
- otorisasi parent dan verifikasi child menjadi anggotanya;
- lakukan otorisasi sebelum resolution storage/stream isi;
- otorisasi ulang setiap continuation, range, retry, resume, cancel, restore, export, dan subscription Hub;
- perlakukan public ID sebagai sesuatu yang dapat ditebak;
- gunakan `404` tersembunyi yang konsisten bila diwajibkan policy;
- perubahan permission/policy membawa version/invalidation; tindakan sensitif menggunakan state saat ini.

## 11.4 Autentikasi Ulang

Kategori kandidat mencakup penghapusan terlindungi permanen, perubahan role/permission/security, session revocation, ekspor/pengungkapan berdampak tinggi, akses isi luar biasa bila pernah disetujui, key/destructive recovery, dan transfer berklasifikasi tinggi yang ditetapkan policy.

Konteks berumur pendek, memiliki scope principal/session/client/action/resource/policy, dibatalkan oleh logout/revocation/disablement/perubahan permission, dan tidak pernah menjadi elevation global. Freshness yang hilang mengembalikan `401 REAUTHENTICATION_REQUIRED` sebelum mutasi parsial.

Matriks pemicu tepat, metode assurance, usia maksimum, dan perilaku retry/lock berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` serta menjadi input Document 09/10.

---

# 12. Batas Administratif, Support, Auditor, dan Isi

```text
Administrative authority ≠ Content access
Support authority        ≠ User-document authority
Audit authority          ≠ Document-content authority
Monitoring/DBA/Backup    ≠ Business content permission
```

| Surface | Dapat diotorisasi | Tidak pernah diimplikasikan |
|---|---|---|
| User | Own work/resources under current policy | Other user/admin/support/audit access |
| Administrator | Session/user/role/permission/feature/policy/config/limit/health metadata | User document/result/OCR/password/clipboard/signature/issue attachment |
| Support | Explicit issue message/attachment, triage/note/status/export | Source, unrelated job/result/secured file/storage browsing |
| Auditor | Approved safe audit evidence query/export | Audit delete, issue content, user document |
| Operations/Worker | Safe health/job execution under workload identity | Human impersonation, role management, unrestricted content browsing |

Tidak tersedia “view as user” implisit, impersonation arbitrer, penjelajahan storage, atau akses isi break-glass. Capability akses isi darurat pada masa depan memerlukan persetujuan produk/security/legal eksplisit, tujuan, reauthentication, batas waktu, bukti, dan desain Document 09/10.

---

# 13. Keamanan API dan Browser

## 13.1 Baseline API

- hanya menggunakan HTTPS yang disetujui; validasi sertifikat tidak boleh dilewati;
- autentikasi global menjadi fallback untuk API/MVC/Hub yang dilindungi;
- token, sesi, dan akun divalidasi secara ketat;
- DTO bertipe dan berversi menerapkan `additionalProperties=false` jika didukung kontrak;
- discriminator/enum bersifat eksplisit dan batas string/array/kedalaman/jumlah/waktu/rentang ditetapkan;
- JSON ganda atau ambigu, field/tipe/versi tidak dikenal, serta angka/waktu tidak valid harus gagal secara aman;
- gunakan EF/SQL berparameter; klien tidak boleh menentukan SQL/kolom/ekspresi dinamis;
- handler Feature canonical bersifat tertutup; tidak ada pemilihan engine/service/class/assembly/shell/path/URL/provider/perintah;
- error aman distandarkan dengan `code`, `message`, dan `correlationId`;
- respons tidak memuat stack/SQL/path/storage key/kebijakan internal/konten/secret;
- idempotency, ETag/rowversion, kuota/backpressure, streaming, dan respons tahan lama 202 mengikuti Dokumen 06.

## 13.2 Kontrol Browser

- lakukan pengodean keluaran kontekstual untuk laporan masalah, nama file, OCR, Markdown, AI, error, dan status;
- HTML mentah dinonaktifkan secara default; sanitizer allowlist hanya digunakan jika markup benar-benar diperlukan;
- CSP dibatasi, termasuk `default-src`, `object-src`, `base-uri`, `frame-ancestors`, dan sumber yang disetujui;
- cookie menggunakan `Secure`, `HttpOnly`, dan SameSite yang sempit;
- anti-forgery wajib untuk setiap mutasi berbasis cookie, termasuk logout, admin, dan sesi;
- GET/HEAD tidak boleh melakukan mutasi;
- origin CORS HTTPS harus tepat dan disetujui; credential wildcard/reflection dilarang;
- Origin WebSocket divalidasi secara terpisah;
- lindungi dari clickjacking, MIME-sniffing, pengalihan/navigasi tidak aman, dan header unduhan tidak aman;
- ekspor spreadsheet menetralkan awalan formula berbahaya serta tidak membuat macro/tautan eksternal.

## 13.3 Reverse Proxy dan Parsing HTTP

- hanya alamat proxy yang disetujui yang boleh memasok data host/protokol/klien terusan;
- validasi Host/target absolut sebelum membuat tautan, callback, atau pengalihan;
- batas proxy/API harus selaras untuk request line, jumlah/ukuran header, body, waktu, upload, dan protokol;
- tolak `Content-Length` ambigu/ganda, transfer coding, folding/whitespace tidak valid, dan framing yang tidak didukung;
- pengujian request smuggling harus melewati proxy/API setara Produksi, termasuk translasi protokol bila digunakan.

## 13.4 Error dan Kesehatan Layanan

`401`, `403`, `404` tersamar, `409`, `413`, `429`, `500`, dan `503` mengikuti Dokumen 06. Endpoint kesehatan hanya menampilkan status minimum; detail topologi/dependensi terbatas memerlukan permission operasi. Ketika dependensi keamanan tidak tersedia, sistem mengembalikan `503` yang aman, bukan akses anonim atau akses istimewa yang kedaluwarsa.

---

# 14. Keamanan SignalR

Hub `/hubs/activity` hanya untuk notifikasi.

1. autentikasi koneksi/negotiate menggunakan model principal yang sama dengan REST;
2. validasi akun, sesi, permission Hub, dan Origin browser;
3. tentukan grup pengguna di server; tolak user/role/group arbitrer;
4. otorisasi subscription Job opsional serta batasi subscription/koneksi;
5. publikasikan metadata aman hanya setelah state di-commit;
6. validasi ID/versi/stream/sequence event;
7. abaikan event ganda/kedaluwarsa; gap/tidak dikenal → rekonsiliasi REST;
8. state terminal yang telah tersimpan tidak boleh mundur;
9. batasi handshake/keepalive/idle/reconnect/pesan/buffer/slow consumer;
10. putuskan atau sambungkan ulang saat expiry/revocation/perubahan permission diketahui.

Hub tidak memiliki mutasi execute/upload/cancel/retry/resume/admin/issue. Kedatangan event, connection ID, grup, Job ID, dan Correlation ID bukan bukti otorisasi atau keberhasilan.

Transport credential:

```text
Web    → same-site protected HttpOnly server session cookie
Native → approved Authorization-header transport
Other  → REST fallback until a no-URL-token mechanism is approved
```

Access token tidak boleh muncul dalam URL, log proxy/WAF/aplikasi, telemetry, riwayat browser, diagnostik, atau referrer.

---

# 15. Keamanan Worker dan IPC

## 15.1 Worker Lokal

- menggunakan pengguna aktif yang dituju atau identitas lokal disetujui yang lebih ketat; bukan Administrator secara default;
- hanya menerima Feature ID canonical, parameter bertipe, referensi input/artefak lingkup job, deadline/budget/Correlation ID;
- tidak memiliki akses arbitrer ke SQLite/SQL, shell, process, filesystem, registry, secret lingkungan, jaringan, provider, atau application service;
- unique protected per-profile/job working root, random names, manifest, quota, path/reparse defenses;
- jaringan ditolak secara default; egress engine yang disetujui hanya untuk tujuan/destinasi yang tepat;
- CPU/RAM/time/temp/output/thread/handle/child-process limits;
- pembatalan/hard timeout menghentikan atau mendaur ulang proses secara aman;
- hasil parsial tidak pernah menjadi final; crash tidak boleh menghentikan UI; sumber tetap tidak berubah.

## 15.2 Named Pipe/IPC Lokal

- hanya mesin lokal; akses remote ditolak;
- restrictive current-user/app/package/process identity ACL where supported;
- trusted parent creates first/instance-bound endpoint;
- authenticated versioned handshake; pipe name/PID/randomness alone is insufficient;
- frame pesan memuat contract/version/type/length/request/Correlation ID/deadline;
- hard message/depth/count/rate/heartbeat limits;
- operasi menggunakan allowlist tertutup; versi/tipe/field tidak dikenal gagal secara aman;
- request bound to launched Worker instance/job; stale Worker cannot complete new job;
- progress/error Worker tidak dipercaya dan harus dinormalisasi;
- peer cacat atau membanjiri koneksi diputus/didaur ulang.

Malware pada pengguna yang sama tetap menjadi risiko residual. Identitas IPC, pengikatan instance, pewarisan handle, integritas kode, dan containment platform memerlukan PoC.

## 15.3 Worker Server dan Identitas Layanan

- workload identity terpisah dan terbatas audience/tujuan; token pengguna tidak diteruskan;
- queue/object/DB/provider/secret permissions separated and minimal;
- pesan queue hanya locator; Job berwenang yang tersimpan harus dimuat ulang;
- claim bounded jobs using state/version/lease and validate handler/schema/refs;
- tidak menahan koneksi/transaksi DB selama pekerjaan file/engine/provider;
- keluaran engine/provider divalidasi sebelum commit;
- crash/OOM recycles Worker and follows bounded attempt/checkpoint policy;
- pool per engine/provider mencegah starvation;
- konteks otorisasi admission job adalah metadata trace yang aman, bukan capability token.

---

# 16. Keamanan Database dan Database Lokal

## 16.1 SQL Server

- klien tidak pernah terhubung langsung;
- runtime, migration/DDL, backup/restore, and human administration identities are separated;
- no `db_owner`/shared human credential for runtime;
- module/schema least privilege and no direct cross-module write;
- parameterized queries, allowlisted query shapes, pagination, timeout, rowversion, uniqueness, idempotency, short transactions, outbox;
- certificate-validated encrypted connection; production trust-bypass disabled;
- approved at-rest/backup encryption, network segmentation/firewall, monitoring, separation of duties;
- lakukan otorisasi sebelum resolusi referensi storage;
- DB audit uses safe actor/service/action/object/outcome/correlation metadata only;
- pemulihan SQL belum selesai hingga rekonsiliasi object/digest/key/version/tombstone/retention/permission tuntas.

## 16.2 SQLite dan Artefak Lokal

Batas yang dilindungi mencakup:

```text
SQLite main DB
WAL / SHM
migration backup/journal
temporary DB and crash/diagnostic copy
draft/cache/thumbnail/checkpoint/temp/result/signature artifacts
local Assistant state and secured cache metadata
```

Aturan:

- satu batas profil/perangkat/subjek dengan ACL/permission terbatas;
- approved at-rest protection must cover DB, WAL/SHM, backups, migrations, crash recovery, supported Windows/mobile platforms, and integrity;
- key is in approved OS credential/key facility, never beside data/predictable ID;
- parameterized typed repositories and allowlisted query fields;
- one serialized writer; Worker only typed IPC;
- no document/signature bytes, password, access/refresh token, raw key, clipboard, or full OCR in ordinary SQLite;
- schema lebih baru yang tidak dikenal dan DB rusak tidak boleh memicu reset diam-diam;
- backup/restore validates profile binding, manifest, artifacts, and no source overwrite;
- local-only root excluded from automatic cloud/device backup.

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`: provider lintas platform, integrasi key OS, perlindungan WAL/temp/crash, rotasi/pemusnahan key, logout/pemulihan, pengecualian backup, dan perilaku saat perangkat hilang. Produksi tidak boleh memiliki fallback cleartext.

---

# 17. Keamanan Penyimpanan File, Temp, Cache, dan Checkpoint

## 17.1 Workspace Lokal

- root terlindungi per profil OS/perangkat dan subjek Pertamina;
- no shared Desktop/Downloads/Public/Global Temp/Camera Roll working state unless explicit export;
- sumber tetap di luar managed root; SQLite menyimpan referensi/fingerprint opaque;
- random cryptographic internal names, exclusive creation, safe Unicode display-name normalization;
- reject traversal, absolute/UNC/device/ADS, NUL/control, reserved names, trailing ambiguity, overlong/bidi-unsafe names;
- final opened object/handle must remain under canonical root;
- reject/fail safely on symlink/junction/reparse/mount escape;
- cleanup uses manifest-owned IDs only and never recursively deletes a user path;
- staged write → flush/close → validate/digest → atomic publish → SQLite pointer commit;
- hasil secara default dibuat baru/Save As; penggantian memerlukan pilihan eksplisit dan identitas file yang diharapkan;
- startup recovery reconciles manifest and residue; no silent discard/reset.

Penghapusan aman tidak dapat dijamin pada SSD/flash/copy-on-write/snapshot. Klaim produk hanya mencakup penghapusan referensi dan permintaan penghapusan OS. Kerahasiaan bergantung pada minimisasi, retensi singkat, enkripsi/siklus hidup key, backup terlindungi, dan kontrol akses.

## 17.2 Zona Penyimpanan

```text
upload quarantine
validated input
processing/checkpoint
job result
secured version
issue attachment
export staging
```

Setiap zona memiliki tujuan/permission terpisah. Promosi merupakan transisi state eksplisit yang telah divalidasi. Akses Worker ke quarantine tidak mencakup hak hapus/tulis secured version.

Objek menggunakan nama acak opaque dan metadata owner/purpose/type/size/digest/state/key reference/timestamp/retention. Raw path/share/bucket key/URL permanen bukan kontrak klien atau field log.

## 17.3 Temp/Cache/Checkpoint

| Artefak | Keamanan | Pembersihan/pemulihan |
|---|---|---|
| Local temp | Private operation root, random/exclusive, ACL, digest/limit | Atomic publish/cancel/expiry/orphan; verified reuse only |
| Upload chunks | Quarantine, owner/purpose/session/offset/digest/limit | Promote/abort/expire; lease-aware retryable purge |
| Processing intermediate | Isolated Worker, no network default, quotas | Terminal + short approved recovery window |
| Cache/thumbnail/preview | Same class as source, private, bounded, not auth truth | LRU/age/parent lifecycle; reauthorize before regeneration |
| Checkpoint | Job/generation/schema/engine/source/digest binding | Resume only if compatible/integrity-valid |
| OCR/temp image | Restricted, no log/index/public AI | Final/cancel/recovery expiry; rerun from source |
| Scanner/clipboard | Foreground explicit staging, bounded | Discard/import/cancel/short expiry |
| Signature working copy | Protected local non-PKI | Cancel/place/delete; never auto-sync |

“Sementara” adalah siklus hidup, bukan klasifikasi yang lebih rendah.

---

# 18. Keamanan Upload dan Download

## 18.1 Upload

Pemrosesan local-only tidak melakukan upload. Eksekusi server opsional/wajib dan transfer aman eksplisit tetap merupakan tujuan berbeda.

```text
authenticate/authorize/policy/capacity
→ bind owner/purpose/job/size/type/idempotency/correlation/expiry
→ stream to quarantine; count actual bytes
→ digest + MIME/signature/structure/resource/security inspection
→ READY only after conclusive validation
→ execute or secure under the authorized purpose
→ tombstone/cleanup failures and expiries
```

Resumable upload hanya menerima offset/range berwenang, menolak overlap yang bertentangan, meng-commit offset setelah penulisan tahan lama, dan selesai secara idempotent. Batasi byte aktual meskipun `Content-Length` salah/tidak ada. Validasi/pembersihan tidak pernah mengekspos raw upload key.

## 18.2 Download

Setiap HEAD/GET/range melakukan autentikasi baru serta otorisasi permission, ownership/purpose/version/state/retention sebelum resolusi storage. Gunakan `Content-Type` tervalidasi, `Content-Disposition` tersanitasi, `X-Content-Type-Options: nosniff`, private/no-store bila sesuai, ETag, Range/If-Range terbatas, dan rate limit untuk amplifikasi rentang kecil.

Klien mengunduh ke staging privat baru, memverifikasi length/digest/type/result, lalu menawarkan Save As. Sumber tidak pernah ditimpa otomatis. Hasil yang diketahui kedaluwarsa mengembalikan perilaku unavailable/expiry yang stabil dan opsi rerun eksplisit; objek belum kedaluwarsa yang hilang merupakan insiden integritas.

Delegated download berumur singkat hanya diizinkan bila disetujui: satu objek/aksi, audience sempit bila didukung, hanya TLS, non-listing, diterbitkan setelah otorisasi API, dan tidak masuk log/riwayat/analytics. URL mentah/permanen tanpa batas tetap dilarang.

---

# 19. Validasi File dan Keamanan Ancaman Dokumen

## 19.1 Pipeline Umum

1. preflight Feature/type/size/count/owner/purpose/policy/capacity/rate;
2. streaming actual bytes/digest/magic/session quota/timeout;
3. compare sanitized extension, declared MIME, signature, parser result;
4. structural parse under page/object/dimension/frame/entry/depth/ratio/expanded-size/output budgets;
5. approved malware/active-content inspection;
6. keep failed/inconclusive/timeout/password-required content non-addressable;
7. process in low-privilege isolated Worker;
8. revalidate generated output type/structure/size/count/digest/security property;
9. commit hasil hanya setelah tervalidasi.

## 19.2 Kontrol Format

| Format | Ancaman | Kontrol |
|---|---|---|
| PDF | malformed xref/objects/streams, JS/actions, embedded files, fonts/images, encrypted input | Bounded isolated parser, active-content policy, page/object/decompressed/output limits, ephemeral password, output validation |
| Office/OOXML | macro/VBA, OLE, DDE, external template/link, ZIP slip/bomb | No active execution/desktop automation baseline, link fetch off, container limits, isolated approved provider |
| Image | decoder/ICC/EXIF/font exploit, pixel/frame bomb, metadata leakage | Magic/decoder agreement, dimensions/pixels/frames/metadata limits, patched isolation, output validation |
| Archive | traversal/link/device entry, nesting/bomb, collision | Deny unless Feature needs it; canonical extraction root, no links, count/depth/ratio/total limits |
| OCR/scanner | adversarial image/PDF, excessive pages/resolution, malicious text | Same file/image gates, resource isolation, output classified restricted DATA |
| HTML/Markdown | script/local resource/remote content/raw HTML/links | Inert renderer, active content off, no local/custom scheme, CSP/sanitization, no auto-fetch |
| Clipboard/share intent | spoofed type/path/oversized delayed data | Explicit gesture, bounded private copy, actual-byte/file-identity validation |

Hasil pemindaian malware yang bersih mengurangi risiko, tetapi tidak membuktikan konten aman. Parser/scanner yang tidak tersedia, timeout, atau tidak dapat menentukan secara aman adalah state gagal/tidak konklusif, bukan keberhasilan.

---

# 20. Konversi, OCR, HTML/Web-to-PDF, dan Fitur Keamanan PDF

## 20.1 Engine Konversi

Semua converter berjalan di belakang Engine Adapter/Resolver dalam Worker terisolasi. Converter tidak boleh mewarisi credential filesystem/jaringan/DB/service yang luas, menjalankan macro/script/embedded executable/shell dokumen, atau menimpa sumber diam-diam. Dependensi harus di-patch, diinventarisasi, dipindai, ditelusuri melalui SBOM, dibatasi sumber dayanya, dan diisolasi dari crash.

Office↔PDF berisiko tinggi, PDF→Office, OCR kompleks, PDF/A, HTML/Web-to-PDF, editing lanjutan, integrasi scanner, dan AI lokal opsional tetap memerlukan gerbang kelayakan/PoC sesuai Dokumen 01–05.

## 20.2 OCR dan Scanner

- keluaran OCR adalah DATA terbatas/tidak dipercaya, bukan instruksi;
- local-first; public AI prohibited;
- page/pixel/time/output/concurrency limits and isolated engine;
- no log/telemetry/generic index/automatic command/path/tool use;
- camera/scanner permission only in foreground user flow;
- app-private staging, no automatic gallery save;
- strip location/device metadata when unnecessary;
- physical scanner helper is scoped and returned file revalidated;
- clipboard read only on explicit paste; no polling/history or silent OS clipboard clearing.

## 20.3 HTML dan Halaman Web ke PDF

Dalam API saat ini, eksekusi Feature generik tidak dapat menerima URL arbitrer. Karena itu:

- validated local/captured HTML artifact may be processed according to `FEAT-CONV-017/018` capability and PoC;
- server remote URL fetch is denied;
- AI/model/provider cannot supply a remote target;
- JavaScript/active content, local file/custom schemes, downloads, WebSocket, ambient credentials, local storage, uncontrolled navigation, and unrestricted network are off;
- renderer has CPU/RAM/time/nested-resource/output limits.

Jika remote Web fetch kelak diperlukan, dibutuhkan adendum/revisi terkendali Dokumen 06, kontrak remote-resource bertipe, dan PoC yang memblokir/memvalidasi ulang userinfo, host terenkode/ambigu, loopback, link-local, target metadata/internal/private, pengalihan, seluruh hasil DNS, rebinding, port/skema tidak didukung, ambient credential, dan konten tanpa batas. Hal ini tidak menghapus Feature; kontrak yang ada tetap dipertahankan secara aman.

## 20.4 Protect dan Unlock PDF

- password is masked and memory-only for the current operation;
- never in log, telemetry, DB, object metadata, audit, dump, diagnostic, chat, issue, checkpoint, autosave, backup, idempotency, queue, or event;
- minimize copies/lifetime; best-effort clear mutable buffers without promising managed-memory erasure;
- restart/resume asks again;
- Unlock requires valid password/authority and is not DRM/encryption bypass;
- Protect/Unlock creates and validates a new result; never silently overwrites source;
- repeated failures are rate-limited without logging password.

## 20.5 Tanda Tangan Visual/Paraf

Tanda tangan visual bersifat sensitif dan secara eksplisit NON-PKI. Ini bukan sertifikat, tanda tangan kriptografis, bukti identitas, atau mekanisme non-repudiation.

- canvas/upload/paste requires explicit action and preview;
- save for reuse is opt-in for this device/profile only;
- bytes are a protected random-name local artifact with minimal SQLite metadata;
- never auto-upload/sync, consumer-cloud backup, telemetry, diagnostics, audit payload, thumbnail cache, or issue screenshot;
- each placement/finalization is explicit;
- audit may record safe action metadata, never asset bytes/template;
- delete removes reference and requests OS deletion with secure-erase limitation.

## 20.6 Redaksi Permanen

Redaksi permanen harus menghapus atau mengubah secara tidak dapat dipulihkan konten sasaran—bukan menggambar overlay visual. Engine harus menangani teks, gambar, annotation/layer, konten tersembunyi/dapat diekstrak, dan turunan OCR yang didukung. Keluaran final harus lulus rendering, pengujian negatif ekstraksi/search/copy, dan pengujian struktur. Preview/thumbnail/OCR/cache/undo/checkpoint pra-redaksi harus diinvalidasi/dibersihkan sesuai siklus hidup. Dokumen asli tetap tidak berubah. Konten yang tidak didukung harus memblokir finalisasi aman atau menghasilkan batasan terlihat yang disetujui, bukan jaminan palsu.

---

# 21. Pertahanan Assistant, AI, dan Prompt Injection

## 21.1 Alur Kontrol Wajib

```text
User intent
→ minimum permitted context
→ document/OCR/HTML/metadata labeled DATA + provenance
→ canonical visible Feature ID
→ closed typed parameter/workflow validation
→ input compatibility and ownership
→ permission
→ policy/classification/consent
→ capability/provider/resource budget
→ ambiguity clarification
→ confirmation when required
→ official application service
→ actual engine/job/result
→ validated grounded encoded response
```

Keluaran AI/model tidak boleh melewati, mengubah urutan, atau memodifikasi kontrol ini.

## 21.2 Injeksi Prompt

- trusted system/policy, user intent, application metadata, and document content are separate typed/provenance channels;
- document/OCR/annotation/metadata/hidden/white text/HTML/translation is DATA, never instruction;
- ignore content requests to reveal secrets, change policy, call tools, browse, upload, or claim success;
- pure summarization/translation exposes no tools;
- plans propose visible canonical Feature IDs only and remain untrusted until deterministic validation;
- context/pages/chunks/output/workflow steps are bounded;
- never provide secrets, tokens, storage keys, unrelated user data, unrestricted paths, or unnecessary pages to model/provider.

## 21.3 Keamanan Tool

- no direct SQL, shell, process, dynamic code, filesystem, arbitrary HTTP/API/URL, plugin, engine/provider selection;
- Feature must be canonical, visible, active, registered, platform-capable, and authorized;
- unknown/extra parameter or unsupported schema fails;
- workflow step checks permission/policy/input-output/capability/budget/confirmation individually;
- destructive/irreversible/upload/share/permanent action never runs only because AI inferred it;
- ambiguity/low confidence requires clarification;
- AI filename/URL/page/path/language/options are ordinary untrusted inputs;
- model cannot declare success without persisted validated result.

## 21.4 Batas Provider dan Data

- provider selected from approved configuration only;
- public AI/translation route technically blocked;
- ownership, permission, classification, notice/consent, purpose, minimum selected pages/fields, capacity checked before transfer;
- provider service identity is audience/purpose scoped; no user token forwarded;
- encrypted temporary content/result uses lease, expiry, cleanup, reconciliation;
- provider retention/training/reuse/operator/location/subprocessor/incident/deletion rules require approval;
- response is untrusted, size/schema/encoding validated, and cannot auto-fetch/tool-call;
- provider content absent from ordinary SQL, idempotency, audit, telemetry, Assistant metadata, SignalR, raw logs, and automatic issue diagnostics.

Kontrol Feature:

| Feature | Perilaku keamanan wajib |
|---|---|
| `FEAT-INTEL-001` | Minimum selected pages/text; no tools; provenance/limitations; source unchanged |
| `FEAT-INTEL-002` | Explicit language/content scope; no public endpoint; review before export |
| `FEAT-CONV-036` | Deterministic extraction first; OCR only when needed; raw HTML/auto-fetch off |
| Assistant workflow | Closed bounded plan, per-step checks, clarification/confirmation, result grounding |

---

# 22. Manajemen Secret, Enkripsi, dan Manajemen Key

## 22.1 Aturan Secret

Password, access/refresh token, service secret, private/encryption key, connection credential, dan provider credential:

- never hardcoded or committed;
- never ordinary SQL/SQLite/object metadata;
- never package/artifact/SBOM/test fixture/log/telemetry/audit/issue/diagnostic;
- injected/retrieved at deployment/runtime through approved least-privilege environment-specific identity;
- absent secret causes safe deployment/startup failure, not weak fallback;
- exposure triggers revoke/rotate and usage/artifact/history review.

## 22.2 Enkripsi

| Batas | Properti wajib |
|---|---|
| Network | Approved TLS/ciphers and full certificate validation end to end |
| Local profile/SQLite/artifacts | Approved protection covering DB/WAL/SHM/temp/backups/migrations/crash and supported platforms |
| SQL Server | Encrypted connection plus approved at-rest/backup protection |
| Object storage | Private access, provider encryption, integrity metadata, enterprise-managed keys where required |
| Backup/export | Encryption + authenticity/integrity + separate key access |
| IPC | Peer/ACL/instance authentication and protected local boundary; pipe secrecy not relied upon |

Kriptografi khusus, embedded master key, atau fallback cleartext yang tidak terverifikasi dilarang. Enkripsi melengkapi—bukan menggantikan—otorisasi, validasi, isolasi, retensi, integritas, dan least privilege.

## 22.3 Siklus Hidup Key

- keys separated from data and stored in approved OS key facility/KMS/HSM/managed enterprise service;
- envelope encryption where applicable; store only opaque key/version reference and wrapped data key;
- unwrap limited to service identity/purpose;
- lifecycle covers generation, activation, rotation/rewrap, suspension, revocation, expiry, destruction, backup, DR, compromise;
- rotation cannot make authorized retained data unrecoverable;
- key administrators/storage/DB/developers/support do not automatically gain plaintext access;
- outage fails closed without queueing plaintext workaround;
- audit safe key operation metadata only;
- minimize plaintext/key memory lifetime and do not promise total managed-memory erase.

Algoritme, provider, custody, periode, escrow/recovery, dan break-glass yang disetujui secara tepat berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 23. Batas Logging, Privasi Telemetry, Issue, dan Audit

## 23.1 Field Operasional yang Diizinkan

```text
timestamp
environment/application/component/version
Feature ID / Job ID / Correlation ID
safe opaque principal/object reference when authorized
safe event/error/policy-decision code
status/outcome
duration and bounded resource counters
engine/provider profile code
queue/health aggregate
```

## 23.2 Field yang Dilarang

```text
password
access/refresh token or cookie
secret/private/encryption key
document/file bytes or full text
full OCR/extracted text
sensitive clipboard
visual signature payload/template
prompt/chat/provider request/response content
issue screenshot/attachment bytes
raw path/storage key/delegated credential
raw exception/SQL/claims dump
```

Filename/path/identity/storage reference/Correlation ID merupakan metadata rahasia: minimalkan, lindungi, batasi cardinality, dan batasi akses/ekspor/retensi. Normalisasi karakter kontrol untuk mencegah pemalsuan log. Crash dump, query capture, log proxy/akses, log pipeline, diagnostic bundle, dan detail kesehatan menggunakan batas konten yang sama.

## 23.3 Pemantauan

Pantau autentikasi/sesi/revocation, probe otorisasi/IDOR, pola status abnormal, file tidak valid dan pemicu batas sumber daya, kegagalan upload/digest, tekanan queue/retry/Worker/lease/sumber daya, akses secured/issue/support/admin/audit/config, anomali outbox/cleanup/backup, paparan scan/dependensi, penolakan kebijakan AI/public provider/schema tool, dan versi yang diterapkan.

Alert harus aman, terdeduplikasi, memahami severity/lingkungan, memiliki penanggung jawab, dan tidak memberi investigator akses konten yang lebih luas.

## 23.4 Pelaporan Masalah

- message/context schema is allowlisted and safe;
- never auto-attach document/log window/screenshot;
- screenshot targets app surface where possible, then preview/crop/redact/remove and explicit Send;
- local staging short-lived/protected; server object case-bound, validated, encrypted, retained, and support-permission scoped;
- attachment failure preserves issue text and offers retry/send-without;
- issue/notes are context-encoded; exports neutralize formulas/macros/external links;
- Job/Correlation ID enables safe correlation only, never content access.

## 23.5 Pemisahan Data Plane

```text
Observability    → technical health/security detection
Enterprise Audit→ controlled append-oriented evidence
User History    → personal convenience projection
Support         → explicit issue/triage content
```

Tidak ada plane yang menyalin konten terbatas dari plane lain secara diam-diam. Dokumen 10 menetapkan detail final event audit keamanan/bisnis, field, integritas, retensi legal, purge/hold, pelaporan, dan permission.

---

# 24. Rate Limit, Penyalahgunaan, dan Keamanan Sumber Daya

Batas efektif adalah nilai paling ketat dari keamanan sistem, kebijakan enterprise, subjek, endpoint/workload, lingkungan, Worker, storage/DB, dan kapasitas provider.

| Permukaan | Kontrol wajib |
|---|---|
| Auth/session/reauth | attempt/burst/session/revocation/refresh coordination; generic errors |
| API/query | body/header/depth/page/date/filter/search/time/DB connection limits |
| Upload/download | streams, bytes/file/batch/chunk/temp/range/slow-client limits |
| Job/Feature | weighted cost, per-user/category/global concurrency, atomic reservation, bounded queue |
| Retry/offline backlog | same idempotency, attempt budget, backoff/jitter, gradual release |
| SignalR | connect/negotiate/reconnect/subscription/message/outbound-buffer limits |
| Engine/OCR/conversion | per-engine CPU/RAM/time/temp/output/child-process pool |
| AI/provider | context/output/call/time/concurrency quota and circuit |
| Support/export | date/result/attachment/output limits and artifact expiry |
| Cleanup/archive | low-priority bounded batches, pressure-aware throttle |

Pengendalian tekanan beban (`backpressure`):

- `413` hard body/file/batch limit;
- `429` caller rate/quota/concurrency with safe `Retry-After`;
- `409` state/version/storage conflict;
- `503` shared unsafe saturation/security dependency unavailable;
- queue bersifat persisten, terbatas, adil, dan terpisah per workload;
- admission dilakukan sebelum pool DB/temp disk/Worker/provider kehabisan kapasitas;
- tidak ada fallback ke public AI/anonim/direct storage;
- capability lokal yang aman tetap dapat digunakan bila memungkinkan;
- fitur dapat menunggu/menurun secara aman, tetapi tidak dihapus hanya karena server awal berukuran kecil.

---

# 25. Keamanan Dependensi, Secure Coding, dan Supply Chain

## 25.1 Aturan Secure Coding

- validasi input bertipe pada setiap batas;
- tegakkan otorisasi objek di server;
- akses data harus berparameter dan dimiliki modul;
- perlakukan file/OCR/AI sebagai tidak dipercaya;
- gunakan streaming, batas sumber daya, dan retry terkendali;
- pertahankan sumber dan publikasikan keluaran tervalidasi secara atomik;
- hanya gunakan error/log yang aman;
- password hanya berada di memori;
- gunakan framework, kriptografi, dan key store yang disetujui;
- Assistant hanya memanggil service resmi canonical;
- pembatalan/checkpoint/idempotency/recovery harus eksplisit;
- kode terkait keamanan harus menambah pengujian negatif/regresi;
- warning tidak boleh disembunyikan tanpa alasan sempit, owner, expiry, dan persetujuan.

`unsafe`, P/Invoke/unmanaged interop, dynamic code/reflection, process/shell, akses jaringan/file/URL/SQL arbitrer, serialization tidak aman, dan validasi TLS yang dinonaktifkan ditolak secara default. Kebutuhan nyata memerlukan desain terbatas, persetujuan keamanan/dependensi, isolasi, pengujian terarah, dan fallback/batasan.

## 25.2 Tata Kelola Dependensi

- pin approved .NET SDK patch in `global.json` when repository starts;
- centralize package versions in `Directory.Packages.props`;
- no floating/silent project override/major-minor update;
- restore only approved sources; scan direct/transitive dependencies;
- dependency record: purpose, owner, alternatives, platforms, license, vulnerabilities, maintenance, resource/data/network behavior, compatibility, fallback/removal, PoC;
- include native library, engine, model, font, tool, image/base dependency when applicable;
- scheduled scanning finds newly disclosed issues in unchanged code;
- scanner must not upload proprietary source/content to unapproved public service.

## 25.3 SBOM dan Integritas Artefak

Setiap rilis mencatat component/version/source/supplier jika diketahui, hubungan langsung/transitif, lisensi, tautan commit/build/artefak, resolved graph, SBOM, digest artefak kuat, versi migration/config, dan versi aplikasi yang dipantau.

Signing/provenance attestation direkomendasikan hanya setelah persetujuan Pertamina. Ini merupakan keamanan build dan tidak mengubah ruang lingkup Sign PDF visual NON-PKI.

---

# 26. Keamanan CI/CD dan Pemisahan Lingkungan

## 26.1 Azure DevOps Git/PR

- branch dilindungi; direct push/force/history rewrite/delete rutin dilarang;
- short-lived scoped changes linked to requirement/Feature/defect/risk;
- PR, current pipeline, resolved discussions, traceability, human reviewer mandatory;
- author not sole security acceptance for security-sensitive change;
- bypass exceptional, logged, time-bounded, post-reviewed;
- perbaikan darurat adalah perubahan terkendali yang dipercepat, bukan patch langsung ke Produksi.

## 26.2 Lapisan Pipeline

```text
controlled restore
→ Release build
→ unit/integration/contract/security tests
→ secret/dependency/static/unsafe/source/config/API/deployment scans
→ conditional container/dynamic scan
→ package
→ SBOM + artifact digest/provenance evidence
→ controlled publication and promotion
```

Scan/tool wajib yang gagal tidak boleh lolos diam-diam. Kebijakan retry/fail, toolchain, lisensi, dan data residency yang tepat memerlukan konfirmasi Pertamina.

## 26.3 Build dan Promosi

- clean controlled agents, pinned SDK/packages/sources/tool versions;
- no undeclared workstation dependency/path;
- deterministic/reproducible settings where supported;
- generated artifacts produced by pipeline;
- artifact/migration/config/SBOM linked to commit/run and stored immutably/restricted;
- same binary promoted Dev → QA → Production; environment values injected separately;
- no manual in-place binary, database migration, or untracked config change.

## 26.4 Pemisahan Lingkungan

Dev, QA, dan Produksi memisahkan endpoint, credential, identitas layanan, konfigurasi, database/storage/queue/provider, telemetry, otorisasi deployment, dan jalur akses. Development menggunakan data sintetis. Data Produksi memerlukan masking/otorisasi yang disetujui dan tidak boleh disalin sembarangan. Produksi menonaktifkan debug, menerapkan least privilege dan pemantauan, hanya memakai artefak pipeline, serta tidak memiliki fallback public AI.

Identitas deployment terpisah dari identitas developer/runtime. Migration Produksi dikontrol pipeline, ditinjau, idempotent, diperiksa kompatibilitasnya, memiliki recovery point terverifikasi dan rencana forward-fix/rollback; migration saat startup aplikasi dilarang.

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`: branch/reviewer/code-owner/bypass policy, scan products, SBOM format, artifact signing, agent/image, host/network/IIS/cloud hardening, endpoint protection, access/masking, container applicability.

---

# 27. Pengujian Keamanan, PoC, dan VA/PT

## 27.1 Piramida Pengujian

| Lapisan | Bukti keamanan minimum |
|---|---|
| Unit | allowlist/bounds/redaction/path/policy/error/crypto-wrapper/prompt/tool rules |
| Integration | API-DB-object, SQLite-artifact, key adapter, outbox/audit, cleanup/tombstone |
| Contract | OpenAPI/Feature/IPC/event versions, handler/authz, compatibility, forbidden fields |
| Authorization | cross-user/profile/environment/module/role, cursor/export/range/download/support/admin |
| Abuse/Negative | injection, malformed/over-limit, retry/flood, prompt, file bomb, error leakage |
| E2E | SSO→tool→job/result, transfer, secured versions, issue send, delete, outage/reconnect |
| Static/Supply chain | secret/dependency/SAST/unsafe/config/deployment/SBOM/artifact |
| Performance/DoS | queues, pools, streams, malformed/large file, provider, cleanup, soak |
| VA/PT | independent/system-level attack validation before Production |

## 27.2 Rangkaian Pengujian Wajib

### Autentikasi dan Otorisasi

- invalid signature/algorithm/issuer/audience/type/lifetime/not-before/subject;
- PKCE/callback transaction/state/nonce/redirect mismatch;
- refresh concurrency/replay, logout, fixation, revocation, disabled account;
- offline tamper/expiry/rollback/account switch/reconnect;
- IDOR for upload/job/result/history/secured/version/issue/attachment/audit/export/config/admin/Hub;
- authorization before count/cursor/storage/range/export;
- self-elevation/mass assignment/content boundary;
- equivalent UI/File-First/Assistant/workflow authorization.

### File, Worker, dan IPC

- malicious PDF/Office/image/archive/HTML/OCR corpus, fuzz/mutation where supported;
- MIME/signature/structure/polyglot/truncation/macro/OLE/link/action/bomb limits;
- traversal/UNC/device/ADS/symlink/reparse/TOCTOU/cross-job access;
- source immutability/output validation/quarantine/cancel/crash/OOM/recycle/cleanup;
- IPC cross-user/remote/same-user rogue/stale/version/malformed/flood/job mismatch;
- Worker cannot access arbitrary DB/shell/path/network/provider/token;
- CPU/RAM/time/temp/output/child-process containment.

### Web/API/SignalR

- SQLi/command/path/CRLF/XSS/Markdown/formula/URL injection;
- CSRF/CORS/CSP/Origin/cookie/open redirect/Host/forwarded header/cache;
- request smuggling through actual proxy/API;
- body/header/query/range/rate/quota/idempotency/race;
- Hub credential/origin/group/subscription/flood/slow consumer;
- duplicate/stale/reordered/gap/unknown event and REST recovery;
- verify Hub has no business mutation.

### AI dan Privasi

- Indonesian/English direct and indirect prompt injection;
- malicious PDF/OCR/metadata/annotation/hidden/HTML/translation/filename instructions;
- invented/deprecated/hidden Feature, extra parameter, cyclic/oversized plan, confirmation bypass;
- no SQL/shell/filesystem/arbitrary HTTP/public AI/other-user/secret access;
- selected-page/field minimization and provider egress/retention behavior;
- malicious provider HTML/Markdown/URL/tool JSON/control/oversize;
- no false success before actual result;
- forbidden content absent across DB/log/audit/telemetry/event/idempotency/issue.

## 27.3 PoC Wajib

Rilis fitur berisiko tinggi memerlukan PoC yang sesuai:

- document/converter/parser containment and malicious corpus;
- HTML/Web-to-PDF renderer/SSRF/egress;
- Named Pipe/Worker identity/instance/resource containment;
- SignalR credential/origin/revocation/load/REST recovery;
- Content Intelligence provider, injection, minimization, egress, retention, limits;
- SQLite/WAL/SHM/artifact/backup-exclusion/key/logout/recovery.

Kegagalan menghasilkan status terkendali `BLOCKED_ALTERNATIVE_REQUIRED`, `APPROVED_WITH_LIMITATION`, atau alternatif/fase yang disetujui—bukan penerimaan risiko diam-diam atau penghapusan fitur.

## 27.4 VA/PT

VA/PT wajib sebelum Produksi dan setelah perubahan material pada attack surface/batas keamanan sesuai persetujuan. Cakupannya meliputi Web/API/auth/authz/files/storage/jobs/SignalR/secured/support/admin/audit/config, penyimpanan lokal/IPC/Worker, batas provider ketika aktif, deployment/config/secret/observability.

Temuan Critical/High memerlukan perbaikan dan retest atau penanganan risiko pengecualian yang disetujui formal dan memiliki masa berlaku. Provider, metode, cadence, aturan, retensi bukti, data uji, independensi, dan otoritas penerimaan memerlukan konfirmasi Pertamina.

---

# 28. Keamanan Backup dan Restore

| Penyimpanan | Perlindungan wajib |
|---|---|
| SQL Server | DB/log according to recovery model, encryption, least privilege, integrity, retention, monitoring, verified recovery point |
| Object storage | Encrypted payload and metadata link by opaque ref/version/digest/key/tombstone/retention/hold |
| SQLite/local artifacts | Profile-scoped protected DB + required artifacts + authenticated integrity manifest; backup exclusion; no source overwrite |
| Pipeline/artifact/config | Controlled access, integrity metadata, retention, reproducible release evidence, no secret embedding |

Pembaca backup/DBA/operator tidak otomatis memperoleh permission bisnis untuk melihat dokumen. Key, backup, restore staging, katalog, dan log dibatasi per lingkungan serta diaudit aksesnya.

Restore dinyatakan berhasil hanya setelah memverifikasi:

- authorized key/credential and backup integrity;
- schemas/migrations/constraints/module permissions/app compatibility;
- object existence/version/digest/key/tombstone/retention/legal hold;
- outbox/idempotency/job/lease/audit and no duplicate logical effects;
- maximum three active/addressable secured versions;
- local profile/SQLite/artifact manifest/account isolation/no source overwrite;
- deleted/tombstoned/expired data does not become addressable;
- safe audit/monitoring and post-restore security smoke test;
- measured recovery evidence against approved targets.

RPO/RTO, topologi, jadwal, HA/DR, salinan immutable/offline, lokasi geografis, key escrow, role operator, dan cadence latihan menjadi cakupan Dokumen 17/DBA/konfirmasi Pertamina.

---

# 29. Manajemen Kerentanan dan Prinsip Insiden Keamanan

## 29.1 Siklus Hidup Kerentanan

```text
detect/receive
→ validate/deduplicate
→ classify asset/data/exposure/severity
→ assign owner
→ contain if necessary
→ scoped remediation PR
→ pipeline + security regression
→ QA/security retest
→ controlled release/monitor
→ close with evidence or approved expiring exception
```

Severity kerja adalah Critical/High/Medium/Low. Critical/High memblokir rilis kecuali ada penerimaan pengecualian yang disetujui, terbatas, dikompensasi, dipantau, dan memiliki masa berlaku. Scoring resmi, SLA, penyesuaian exploitability, kanal disclosure, otoritas risiko, dan expiry waiver memerlukan konfirmasi.

Update darurat tetap ditelusuri, dibatasi sempit, dipindai/diuji, dibangun sebagai artefak immutable baru, disetujui melalui tata kelola dipercepat, diterapkan melalui pipeline, siap rollback/forward-fix, dipantau, dan ditinjau setelahnya. Patch langsung atau perubahan secret/config tanpa dokumentasi dilarang.

Dependency compromise freezes affected source/version, preserves package/hash/build/SBOM evidence, identifies impacted artifacts, rotates plausible exposed credentials, replaces/pins verified safe dependency, rebuilds from clean inputs, retests/redeploys, and monitors.

## 29.2 Prinsip Insiden

```text
Prepare → Detect/Validate → Triage → Contain
→ Preserve Evidence → Eradicate/Recover → Learn/Update
```

- gunakan containment paling tidak destruktif dan dapat dipulihkan;
- investigator tidak memperoleh akses konten otomatis;
- preserve safe synchronized logs/audit/config/pipeline/SBOM/digest/dependency/access/Job/Correlation evidence;
- record collector/time/source/integrity/access/transfer/disposition;
- do not put password/token/secret/document/OCR/signature into incident record by default;
- rotate compromised material, rebuild from controlled source, reconcile restore, retest, deploy verified artifact, monitor;
- perbarui requirement/kontrol/test/dependensi/desain/runbook.

Kontak operasional, severity, pemicu notifikasi/legal/privasi, chain-of-custody, tool forensik, retention/hold, latihan, dan otoritas destruktif menjadi cakupan Dokumen 16 dan konfirmasi Pertamina.

---

# 30. Retensi, Penghapusan, Risiko Residual, dan Konfirmasi

## 30.1 Aturan Retensi Keamanan

- exact durations are typed governed policy, never guessed/hardcoded;
- retain only for user operation/recovery, security investigation, audit obligation, or legal hold;
- temp/cache/preview short and bounded;
- active draft/checkpoint not deleted under emergency pressure;
- password only operation memory;
- upload/job/result/issue/export each has explicit expiry and tombstone/cleanup;
- max three active/addressable secured versions;
- backups expire under approved recovery/legal policy;
- cleanup is lease/hold aware, idempotent, bounded, observable, and never follows user path/link;
- deletion/tombstone prevents access immediately; physical purge may retry;
- no absolute secure-erase claim.

## 30.2 Risiko Residual

| Risiko | Kontrol kompensasi/penerimaan wajib |
|---|---|
| Parser/codec zero-day | Isolation, low privilege, patch/SBOM, kill/recycle, monitoring, engine approval |
| Same-user malware | Endpoint/device assumption, app/package identity PoC, minimize local secrets/privilege |
| Approved internal AI sees selected content | Classification, consent, minimization, provider governance, temporary lifecycle |
| AI/OCR/translation error | Label/provenance, preview/user review, no authoritative replacement |
| Remote content mutability | Remote fetch denied currently; future digest/provenance/egress bounds |
| Offline revocation delay | Narrow absolute-expiry local-only capability and reconnect validation |
| User overshares issue screenshot | Preview/warning/crop/redact, explicit send, case ACL/retention |
| Flash/SSD secure erase limitation | Encryption/key lifecycle, minimization, no false claim |
| Redaction engine limitation | PoC/forensic tests, visible limitation, block unsafe finalization |
| Supply-chain/zero-day | Controlled source, pin/SBOM/scans, monitoring, emergency disable/update |
| Extreme distributed DoS | Edge/infrastructure capacity, layered quotas, degradation, incident response |
| Misconfiguration | Typed bounds, review/audit/drift/security tests, staged rollout |

Setiap risiko residual yang diterima mencatat owner, feature/lingkungan, bukti, kontrol kompensasi, otoritas penerimaan, expiry/review, dan remediasi. “Internal” bukan alasan penerimaan.

## 30.3 Konfirmasi Pertamina

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
IAM protocol/claims/provisioning/lifetimes/revocation
official classification and handling matrix
Role/Permission/separation-of-duty and reauthentication matrix
crypto/key/secret/local SQLite/object storage standards and products
engine/malware/internal-AI provider governance
secure coding/analyzer/branch/scan/SBOM/provenance standards
host/network/cloud/container/environment hardening
audit catalog/integrity/legal retention
vulnerability/VA-PT/risk-exception governance
alert/SOC/SIEM and incident/evidence policy
backup/DR/RPO/RTO/key recovery
numeric security/performance/rate/retention thresholds
```

Konfirmasi ini menjadi gerbang capability Produksi yang terdampak. Konfirmasi tidak melemahkan secure default atau memblokir model pre-coding final.

---

# 31. Kesiapan Implementasi dan Manajemen Proyek

| Fase | Ruang lingkup | Dependensi/gerbang |
|---|---|---|
| `P0 FOUNDATION` | SSO integration contract, deny-by-default policy framework, safe errors/logging, secret boundary, dependency/CI baseline, protected profiles, threat/test harness | Required before user feature coding at scale |
| `P1 CORE SECURITY` | Object authorization, session/offline/logout, local storage, file staging/validation, Worker/IPC isolation, rate/backpressure, database/object permissions | Dependency for 33 Primary PDF Tool Cards |
| `P2 FEATURE SECURITY` | Per-format engines, password/signature/redaction/OCR/scanner/clipboard/security-specific tests | Implement with each Feature/PoC |
| `P3 INTEGRATION` | Upload/download/jobs/secured versions/SignalR/issue/internal provider/cross-store restore | Contract and environment integration evidence |
| `P4 HARDENING` | malicious corpora/fuzz, DAST/VA/PT, abuse/soak, drift, incident/restore drills, final confirmation/acceptance | Before Production and material releases |

Untuk setiap kontrol, perencanaan mencatat risiko bila kontrol tidak ada, fase implementasi, dependensi Core, bukti otomatis/manual, kebutuhan PoC, konfirmasi Pertamina, dan dampak pada pengembangan paralel. Kontrol P0/P1 yang belum tersedia memblokir coding/rilis yang bergantung padanya, bukan pekerjaan dokumentasi aman yang tidak terkait.

---

# 32. Ketertelusuran Kebutuhan

| Area | Cakupan requirement | Hasil keamanan |
|---|---|---|
| Identity/session/offline | `FR-AUTH-001..005`, `FR-ROLE-001..003`, `SR-SEC-001..003` | Approved SSO, protected sessions, narrow offline grant |
| Files/storage | `FR-FILE-001..006,010..015`, `FR-STOR-001..006`, `SR-SEC-003..007` | Untrusted validation, source preservation, protected local/server storage |
| Processing/server budget | `FR-PROC-001..005`, `NFR-PERF-001..013` | Resolver, explicit transfer, isolation, bounds/backpressure |
| Selective secured storage | `FR-BACKUP-001..010`, `FR-VER-001..004` | Explicit Amankan, encrypted object, max-three versions |
| Recovery/deletion | `FR-REC-001..006`, `FR-TRASH-001..009`, `NFR-REC-001..005` | Checkpoint integrity, tombstone, source safety, restore |
| Assistant/AI | `FR-ASSIST-001..015`, `SR-SEC-013..018` | Closed canonical orchestration, prompt/data separation, no secret/content default |
| Intelligence | `FR-INTEL-001..003`, `SR-SEC-021` | Approved local/internal provider, minimization, no public endpoint |
| Support | `FR-SUP-001..007`, `SR-SEC-019..020` | Explicit screenshot, safe diagnostics, permission-scoped support |
| PDF/signature/redaction | `FR-SIGN-001..003`, `SR-SEC-011..012,022..023` | Non-PKI asset, authorized Unlock, real redaction |
| API | `IR-API-001..012` | Auth, safe upload/job/error/telemetry/issue contracts |
| DevOps/environment | `IR-DEVOPS-001..004`, `NFR-ENV-001..004` | Controlled PR/pipeline/artifact/environment promotion |
| Monitoring | `NFR-MON-001..004` | Safe observability and alert boundary |
| Governance | `BR-GOV-001..009` | Traceability, dependency approval, PoC, Primary Feature priority |

---

# 33. Ketertelusuran Fitur

| Grup Feature | Tanggung jawab keamanan |
|---|---|
| 33 Primary PDF Tool Cards | Shared P0/P1 security plus per-engine Feature gate; no security bypass per card |
| `FEAT-AUTH-001..012` | SSO/session/account/permission boundary |
| `FEAT-VALID-001..018` | Layered file validation/quarantine/status |
| `FEAT-SEC-001..023` | Security inspection/policy/safe error controls |
| `FEAT-STOR-001..016` | Local protected workspace/temp/cache/artifact lifecycle |
| `FEAT-BACKUP-001..013`, `FEAT-VER-001..006` | Explicit secured storage, restore, max-three versions |
| `FEAT-REC-001..015`, `FEAT-PROG-001..008` | Job/checkpoint/recovery/REST authority/safe notifications |
| `FEAT-AUDIT-001..008`, `FEAT-MON-001..017` | Safe evidence/observability separation |
| `FEAT-SUPPORT-001..008` | Explicit issue content/attachment/triage/export security |
| `FEAT-ASSIST-001..030` | Closed orchestration, clarification, confirmation, result grounding |
| `FEAT-INTEL-001..002`, `FEAT-CONV-036` | Restricted content/provider/AI-output controls |
| `FEAT-CONV-017..018` | HTML/Web renderer isolation and remote-fetch gate |
| `FEAT-OCR-004` | Isolated bounded OCR and DATA-only output |
| `FEAT-PDF-062`, `FEAT-PDF-069` | Authorized Unlock/password memory and irreversible redaction |
| `FEAT-SIGN-001..013` | Sensitive local visual non-PKI asset |

Semua ID tersedia dalam Feature Catalog v1.3. Tidak ada Feature ID yang dibuat atau dihapus. Risiko menentukan PoC/bukti/batasan, bukan penghapusan fitur otomatis.

---

# 34. Ketertelusuran API

| Keluarga API | Pemeriksaan keamanan |
|---|---|
| Compatibility/health | Explicit minimal anonymous/operations scope; no sensitive topology |
| Feature/workflow execution | Canonical handler, typed schema, authz, ownership, policy/classification/consent/capability/capacity/output |
| Preflight/upload/session | Auth/purpose/limits/quarantine/digest/offset/state/idempotency/cleanup |
| Jobs/results | Owner/scope/state/lease/retry/checkpoint; fresh authorization for control/retrieval |
| Account/preferences/history | Current principal/owner; protected metadata; history delete not audit |
| Secured files/versions | Owner/policy/ETag/reauth for high-risk deletion; encrypted object; max-three |
| Issues/support/export | Reporter/support permission separation, explicit attachment, safe content/export |
| Audit/admin/config/limits | Granular permission, scope, reauth for high impact, rowversion, safe evidence |
| SignalR Hub | Same principal, server groups, safe event, no command, REST recovery |

Endpoint `/api/v1/features/{featureId}/execute` yang ada tidak menerima URL/provider/path/engine/command arbitrer. Karena itu, remote fetch `FEAT-CONV-018` tetap tidak tersedia dalam kontrak saat ini sampai ada revisi/adendum terkendali.

---

# 35. Ketertelusuran Database

| Batas database/storage | Aturan keamanan |
|---|---|
| `identity`/`admin` | Hashed session metadata, current principal/permission, no token/password, least privilege |
| `configuration`/`workspace` | Typed/versioned policy/preferences; admin cannot change Feature meaning or weaken hard safety |
| `jobs` | Owner, Feature, state/lease/idempotency/outbox; no content/secret; Worker job scope |
| `secured` | Owner/version/tombstone/retention/object ref; max-three active/addressable; object auth |
| `history` | Personal projection/owner; delete separated from Audit |
| `audit` | Safe append-oriented metadata, restricted query/export; no content |
| `support` | Restricted issue message/note and safe metadata; attachment/export object storage |
| `assistant`/`intelligence` | Minimal operational metadata only; no prompt/document/provider content |
| SQLite | Per-profile local state; protected DB/WAL/SHM/artifacts; no token/password/document BLOB |
| Object storage | Purpose zones, encryption, digest, opaque ref, lease/retention/tombstone/reconciliation |

Identitas database runtime tidak dapat melakukan administrasi schema atau baca/tulis lintas modul tanpa otorisasi. Otorisasi dilakukan sebelum resolusi referensi. Restore memvalidasi ulang permission aktif dan state penghapusan.

---

# 36. Kriteria Penerimaan Keamanan

## 36.1 Identitas dan Otorisasi

```text
[x] Pertamina Identity and OTO authorization authority separated
[x] Native PKCE and Web server-side protected session defined
[x] No token in URL/DB/log/telemetry/issue
[x] Logout/revocation/disabled/offline/reconnect model defined
[x] Deny-by-default permission + ownership + policy + state formula defined
[x] Admin/Support/Auditor/content permissions separated
[x] Scoped sensitive reauthentication defined
```

## 36.2 Data, File, dan Local-First

```text
[x] Per-profile SQLite/WAL/artifact protection boundary defined
[x] Source/draft/cache/checkpoint/signature remain local unless explicit allowed action
[x] Path/reparse/random-name/atomic-output/source-safety controls defined
[x] Upload/download/quarantine/validation/output controls defined
[x] SQL/object/temp/secured/version/cleanup boundaries defined
[x] Max three active/addressable secured versions preserved
[x] No false secure-delete claim
```

## 36.3 Aplikasi, Worker, dan AI

```text
[x] API/browser/proxy/XSS/CSRF/CORS/smuggling controls defined
[x] SignalR authenticated notification-only and no-token-URL resolution defined
[x] Worker/IPC least privilege, authentication, isolation, limits, recovery defined
[x] PDF/Office/image/OCR/HTML threat controls defined
[x] Password, visual non-PKI signature, and real redaction controls defined
[x] Assistant canonical closed orchestration and prompt-injection controls defined
[x] Approved local/internal provider only; public AI unreachable
[x] Remote URL fetch not inferred into current API
```

## 36.4 Operasi dan Bukti

```text
[x] Secret/encryption/key principles defined without vendor invention
[x] Safe logging/telemetry/issue/audit boundaries defined
[x] Layered abuse/resource/backpressure controls defined
[x] Secure coding/dependency/SBOM/artifact/CI/CD/environment gates defined
[x] Security test pyramid, malicious corpus, PoC, VA/PT gates defined
[x] Backup/restore, vulnerability, incident, retention, residual-risk rules defined
[x] Pertamina confirmation gaps explicit
[x] Requirement, Feature, API, and Database traceability included
```

---

# 37. Audit Akhir dan Status

```text
Specialist reports integrated : 4 / 4
Parent FINAL documents audited: 7 / 7
Security decisions            : 20 / 20 AUTO-LOCKED
Required security areas       : 59 / 59
Material contradiction        : 0
User approval required        : 0
Canonical features            : 550
Primary PDF cards             : 33
Document status               : FINAL v1.0
```

Dokumen berikutnya:

```text
09_ROLE_MATRIX.md
```
