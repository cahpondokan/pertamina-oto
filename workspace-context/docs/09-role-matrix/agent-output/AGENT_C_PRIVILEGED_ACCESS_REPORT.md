# AGENT C — PRIVILEGED ACCESS REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC09-AGENT-C` |
| Fokus | User, Administrator, Support/Superadmin, Auditor, content separation, privileged access, dan separation of duties |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 24 Agustus 2026 |
| Decision owner | User + Integrator |

## Baseline dan cara membaca rekomendasi

Laporan ini diturunkan dari Document 01–08 FINAL, terutama Business Process section 7, 19–21, dan 26; Requirement `FR-ROLE-001..003`, `FR-HIST-001..003`, `FR-AUDIT-001..004`, `FR-SUP-001..007`, serta `SR-SEC-007`, `SR-SEC-019..020`; Feature Catalog `FEAT-AUTH-*`, `FEAT-ADMIN-*`, `FEAT-AUDIT-*`, `FEAT-HIST-*`, `FEAT-SUPPORT-*`, dan `FEAT-ASSIST-*`; API Final sections 9, 10.4–10.6, dan 18; Database Final sections 7.1, 7.5–7.6, 17–18; serta Security Design Final sections 8–12, 23, dan 30.

Prinsip yang tidak dibuka ulang:

- deny-by-default dan permission-based authorization;
- role hanya pengelompokan permission, bukan keputusan akses final;
- ownership, explicit scope, policy, classification, resource state, consent, dan freshness tetap diperiksa;
- `User`, `Administrator`, dan `Auditor` adalah baseline persona dari requirement;
- istilah `Superadmin` digunakan pada konteks Support Console, tetapi merupakan **Support permission bundle**, bukan God Mode atau role baru otomatis;
- administrative authority, support authority, audit authority, dan content authority saling terpisah;
- tidak ada hak implisit untuk impersonation, “view as user”, storage browsing, atau membuka isi file user;
- server surface untuk Administrator, Support, dan Auditor tidak tersedia secara offline;
- audit perusahaan terpisah dari personal history, observability, dan support issue;
- detail final event/field/integrity/retention Audit Trail tetap milik Document 10.

Label keputusan:

- **RECOMMENDED** — usulan Agent C yang konsisten dengan baseline, menunggu integrasi;
- **REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** — harus dikonfirmasi terhadap standar/kebijakan Pertamina;
- **FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** — bukan capability aktif pada scope saat ini.

---

## 1. User boundary

User berwenang memakai aplikasi untuk pekerjaannya sendiri dalam batas permission, ownership, enterprise policy, classification, platform capability, resource state, consent, dan limit yang berlaku. Kata “sendiri” selalu berarti resource yang owner-nya diturunkan dari authenticated principal; owner tidak boleh diterima dari body/query buatan client.

### 1.1 Hak dan batas per area

| Area | Hak User | Syarat wajib | Tidak tersirat |
|---|---|---|---|
| Menggunakan tools | Melihat tool yang exposed dan menjalankan canonical Feature ID yang diizinkan | Feature permission, typed parameters, input ownership, policy/classification, capability/engine/capacity, consent, dan confirmation bila berisiko | Menjalankan Feature ID tersembunyi, memilih engine/command/path/URL arbitrer, atau melewati limit |
| File lokal milik sendiri | Memilih, memproses, menyimpan, membuka, menghapus, dan memulihkan file/draft/result dalam app-managed boundary | OS permission, safe path resolution, file validation, dan kebijakan local workspace | Akses file user OS lain, folder di luar pilihan/allowlist, atau unrestricted filesystem |
| Job dan result sendiri | Melihat status/result; cancel/retry/resume bila state dan policy mengizinkan | Owner check pada setiap request dan reauthorization pada result/download/continuation | Mengakses job/result orang lain hanya karena mengetahui Job ID atau Correlation ID |
| Personal history | Membaca, mencari, memfilter, membuka hasil/lokasi, reprocess, dan menghapus item history miliknya | Owner scope; reprocess kembali melalui permission/policy saat ini | Menghapus enterprise audit, job evidence, secured file, atau data user lain |
| Secured files | Membuat melalui **Amankan ke Akun**, melihat metadata/content, menambah versi, restore, recycle, dan permanent delete miliknya | Owner, quota, storage/classification policy, version/state, confirmation, serta reauth untuk destructive action bila policy mewajibkan | Membaca secured file orang lain, bypass maksimum tiga versi, legal hold, atau retention policy |
| Issue sendiri | Membuat issue, melihat status/detail sederhana, dan mengirim screenshot/attachment secara eksplisit | Message minimum, attachment validation, preview, explicit Send, dan owner/reporter scope | Melihat internal notes, internal triage detail yang tidak user-facing, atau issue user lain |
| Preferences sendiri | Membaca/mengubah allowlisted preference dan favorite/preset metadata miliknya | Typed/versioned update; enterprise policy dan system safety tetap lebih tinggi | Mengubah enterprise policy, feature flag, limit, atau menyinkronkan path/content/secret |
| Assistant sendiri | Menggunakan Assistant untuk help, planning, dan eksekusi Feature resmi sesuai permission | Canonical Feature ID, typed parameters, ownership, permission/policy/capability, consent, confirmation, dan actual-result grounding | Direct SQL, shell, arbitrary API, unrestricted filesystem, permission bypass, atau akses conversation user lain |
| Profile/session sendiri | Melihat safe profile dan logout dari sesinya; mengelola opsi session yang secara eksplisit disediakan | Pertamina Identity dan current application session policy | Melihat token/refresh credential atau mengubah role/claim sendiri |

### 1.2 Guard User

1. UI yang menyembunyikan tombol bukan kontrol keamanan; API/application service tetap mengotorisasi setiap aksi.
2. Opaque ID, filename, local path, `fileRef`, Job ID, Correlation ID, cursor, link, dan possession atas result lama bukan bukti akses.
3. User dapat menghapus personal history dan Assistant conversation state miliknya tanpa menghapus enterprise audit.
4. File source, draft, result, secured file, issue attachment, dan audit adalah resource berbeda; aksi pada satu resource tidak boleh cascade diam-diam ke resource lain.
5. Setelah permission/policy berubah, retry, resume, reprocess, queued intent, download, dan restore harus dinilai ulang menggunakan state saat ini.
6. Offline hanya mengizinkan capability lokal yang tercantum dalam protected time-bounded grant. Upload, secured storage, issue submission, audit export, serta semua privileged server surface menunggu/menolak sampai online.

---

## 2. Administrator boundary

Administrator mengelola **control plane** aplikasi: account/application eligibility, permission assignment, configuration, enterprise policy, feature exposure, limit, dan safe system metadata. Administrator bukan custodian otomatis atas **data plane** dokumen user.

### 2.1 Kewenangan yang dapat diberikan secara granular

| Area | Kewenangan Administrator yang direkomendasikan | Guard |
|---|---|---|
| Account administration | Membaca safe application-account metadata; mengaktifkan/menonaktifkan eligibility aplikasi sesuai kewenangan; memulai application-session revocation | Tidak mengelola password Pertamina, tidak meniru IdP, tidak melihat token; provisioning/disablement enterprise mengikuti standar Pertamina |
| Role dan permission | Membaca role/permission catalog dan mengelola assignment yang berada dalam delegable scope | Tidak boleh self-assign, memberi permission yang tidak dimilikinya untuk didelegasikan, mengubah canonical permission semantics dari UI, atau menghindari approval policy |
| Configuration | Membaca dan mengubah typed/versioned configuration yang diizinkan | `If-Match`/optimistic concurrency, validation, hard safety ceiling, audit, dan reauth untuk high-impact change |
| Enterprise policy | Mengelola policy set/version sesuai scope yang didelegasikan | System safety invariant tidak dapat dimatikan; retention/legal/classification change dapat memerlukan approval terpisah |
| Feature enable/disable | Mengubah exposure/rollout/emergency-disable flag | Tidak mengubah Feature ID, contract meaning, permission semantics, atau menjadikan handler tidak sah dapat dipanggil |
| Limits | Mengatur file, processing, storage, concurrency, dan policy limit dalam approved range | Nilai efektif tidak boleh melampaui hard system safety ceiling atau kapasitas provider/Worker |
| System metadata | Melihat version, compatibility, safe configuration, aggregate health, queue/job/sync/error/storage metrics | Default aggregate/minimized; tidak membuka filename/path/content/issue attachment/raw exception/storage key |
| Monitoring | Memantau kesehatan layanan dan menemukan gangguan berdasarkan safe code, Job ID, atau Correlation ID | Identifier hanya untuk korelasi; tidak menjadi pivot otomatis ke file/result/content |
| Audit access | Hanya bila permission Audit terpisah secara eksplisit diberikan | Nama role Administrator tidak otomatis memberi audit query/export; SoD dan self-review tetap berlaku |

### 2.2 Larangan eksplisit Administrator

Administrator tidak otomatis berhak:

- membaca, preview, mencari isi, atau mengunduh document/source/result/secured file user;
- membaca full OCR/extracted text, Assistant prompt/chat, clipboard, atau local workspace user;
- melihat password PDF, access/refresh token, cookie, service secret, encryption key, atau raw claim dump;
- melihat/mengunduh visual signature/paraf payload;
- membuka issue message, screenshot, attachment, atau internal note hanya karena berstatus Administrator;
- menggunakan storage key/raw path untuk membuka artifact;
- impersonate user, membuat delegated content link, atau “view as user”;
- menghapus/mengubah enterprise audit event atau issue status history;
- mengubah system safety invariant, canonical Feature contract, atau security ceiling melalui configuration UI;
- memberikan privilege kepada dirinya sendiri atau menyetujui perubahan privilege miliknya sendiri.

### 2.3 Account-administration interpretation

Karena identitas bersumber dari approved Pertamina Identity, “user management” dalam PERTAMINA OTO harus diartikan sebagai pengelolaan mapping/eligibility aplikasi, role/permission assignment, session metadata/revocation, dan safe profile projection sesuai standar. Pengelolaan password, MFA, account recovery, enterprise identity lifecycle, dan credential tetap berada pada Pertamina Identity kecuali ada konfirmasi standar yang eksplisit.

---

## 3. Support/Superadmin boundary

`Superadmin` pada layar produk berarti petugas Support yang memperoleh satu atau lebih permission Support. Istilah ini tidak membuat hierarchy di atas Administrator dan tidak memberikan wildcard permission.

### 3.1 Permission Support yang harus dipisah

| Capability | Akses yang diperlukan | Batas objek/data |
|---|---|---|
| Issue list | Membaca daftar issue sesuai organizational/assignment scope | Projection ringkas; tidak otomatis memuat attachment bytes atau internal note lengkap |
| Issue detail | Membaca message, reporter/time/version, category, priority, status, Feature ID/page, Error Code, Job ID, Correlation ID, dan safe diagnostic reference | Per-issue authorization; hanya field yang diperlukan untuk triage |
| Issue attachment | Preview/download screenshot/attachment yang sengaja dikirim user | Permission terpisah; case-bound; explicit-send evidence; validation dan retention check |
| Triage | Menetapkan category, priority, duplicate/not-bug/need-info, dan assignment/state yang diizinkan | Typed transition, optimistic concurrency, current-state policy, audit |
| Status | Mengubah internal status dan user-visible mapping sesuai workflow | Tidak dapat menghapus history transisi atau memalsukan actor/time |
| Internal notes | Membaca/menambah note terbatas untuk Support | Permission terpisah; note restricted, context-encoded, retention-sensitive, tidak tampil ke user kecuali workflow menentukan |
| Safe diagnostics | Melihat safe diagnostics yang dikaitkan dengan issue | Tidak ada document bytes/text, raw exception, credential, full OCR, clipboard, signature, raw path/storage key |
| Job/Correlation correlation | Melihat safe issue-linked job metadata yang diperlukan untuk triage | Job ID/Correlation ID tidak memberi akses result/input/content atau query bebas lintas user |
| PDF export | Meminta bounded export satu issue sesuai permission | Field dan attachment inclusion mengikuti policy; artifact sementara, access-controlled, expiry, dan audited |
| Excel export | Meminta bounded filtered issue register | Filter/row/field bound; formula/macro/external-link neutralization; tidak menjadi bulk content dump |

### 3.2 Data yang tidak dibutuhkan otomatis oleh Support

Support tidak otomatis memerlukan atau memperoleh:

- full user document, source, result, secured file, atau version content;
- full OCR/extracted text, Assistant conversation, atau prompt/provider payload;
- password, token, cookie, secret, key, atau raw claims;
- signature/paraf asset atau clipboard;
- seluruh local diagnostics/log window, crash dump, filesystem, database, storage bucket, atau backup;
- issue, note, atau attachment di luar permission dan organizational/assignment scope;
- Administrator policy/config/role-write permission;
- Auditor permission atau kemampuan mengubah/menghapus audit evidence.

### 3.3 Support scope

Daftar/detail issue harus dibatasi dengan scope yang diturunkan server, misalnya enterprise, unit/region, queue, assignment, atau incident team sesuai kebijakan yang disetujui. Filter client tidak membentuk scope. Exact support scope dan assignment model adalah **REQUIRES_ENTERPRISE_POLICY_CONFIRMATION**.

Jika dokumen user benar-benar dibutuhkan untuk investigasi, akses tersebut bukan bagian dari Support Console saat ini. Kemungkinan alur controlled access dibahas pada section 5 dan berstatus **FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION**.

---

## 4. Auditor boundary

Auditor adalah persona controlled-read untuk enterprise evidence. Default Auditor bukan Administrator, Support, user-content reviewer, atau operator database.

### 4.1 Hak Auditor

| Area | Hak yang dapat diberikan | Guard |
|---|---|---|
| Audit records | Membaca safe enterprise audit evidence sesuai permission dan scope | Append-oriented; no ordinary update/delete; content-prohibited fields tidak tersedia |
| Security events | Membaca approved security-event projection yang relevan | Hanya event/decision code dan metadata aman; bukan raw secret, document, dump, atau exploit detail sensitif |
| Administrative changes | Melihat perubahan account eligibility, session revocation, role/permission, policy/configuration, feature flag, limit, dan privileged action | Safe before/after reference/version/hash sesuai Document 10; tidak memberi write permission |
| Filtered search | Search/filter berdasarkan allowlisted field seperti actor, unit, time, event/feature, status/outcome, platform, Process/Job/Correlation ID | Permission/scope diterapkan sebelum count/query; date span, paging, dan export bound |
| Detail | Membuka detail safe audit event yang berada dalam scope | Tidak mengubah source event dan tidak melakukan content pivot |
| Export | Meminta bounded asynchronous audit export bila memang diperlukan | Permission terpisah, reauth untuk high-impact export, expiry, watermark/classification bila policy, dan audit atas export |

### 4.2 Larangan Auditor

Auditor secara default tidak dapat:

- mengubah/menghapus audit source event, ingestion evidence, atau administrative-change evidence;
- mengubah user data, account, role, permission, configuration, enterprise policy, limit, feature flag, issue, atau job;
- membaca document/result/secured-file/OCR/clipboard/signature/Assistant content;
- membaca issue message/screenshot/internal note kecuali permission Support terpisah secara eksplisit diberikan dan lolos SoD;
- menjalankan arbitrary query/SQL, mengakses raw storage/backup, atau memperluas scope melalui filter/export;
- menghapus personal history milik user lain atau mengubah retention/legal hold.

### 4.3 Audit export is not ordinary read

Export meningkatkan portability, volume, dan disclosure risk. Karena itu `audit.query`, `audit.detail`, dan `audit.export` harus menjadi permission berbeda. Export tidak boleh otomatis diberikan hanya karena Auditor dapat melihat layar audit.

---

## 5. Content-access separation

### 5.1 Rule utama

```text
Administrative authority  ≠ user-content authority
Support authority         ≠ source-document authority
Audit authority           ≠ document/issue-content authority
Monitoring authority      ≠ content authority
Identifier visibility     ≠ content authority
```

Content authorization harus menjadi keputusan terpisah dari kemampuan mengelola metadata. Tidak ada role hierarchy yang membuat permission control-plane mewarisi permission data-plane.

### 5.2 Default access per content class

| Content class | User | Administrator | Support/Superadmin | Auditor |
|---|---|---|---|---|
| Local source/draft/result | Owner pada device/profile sendiri | **NO** | **NO** | **NO** |
| Server job input/result | Owner/current authorized principal | **NO**; safe operational metadata only | **NO**; safe issue-linked metadata only | **NO**; safe evidence only |
| Secured file/version content | Owner/current explicit resource scope | **NO** | **NO** | **NO** |
| Full OCR/extracted text | Owner/tool execution scope | **NO** | **NO** | **NO** |
| Assistant prompt/chat/provider content | Owner/minimal local scope sesuai policy | **NO** | **NO** | **NO** |
| Visual signature/paraf payload | Owner pada explicit operation | **NO** | **NO** | **NO** |
| Clipboard | Current local user/process pada explicit action | **NO** | **NO** | **NO** |
| Issue message | Reporter pada issue sendiri | **NO** by admin role | Support detail permission pada assigned/scoped issue | **NO** by auditor role |
| Issue screenshot/attachment | Reporter pada issue sendiri | **NO** by admin role | Separate attachment permission pada explicitly sent item | **NO** by auditor role |
| Internal support note | Tidak default | **NO** by admin role | Support note permission | **NO** by auditor role |
| Enterprise audit metadata | Tidak default lintas user | Separate Audit permission only | Separate Audit permission only | Controlled query/detail permission |

### 5.3 Explicit controlled content-access possibility

Current baseline tidak menyediakan privileged document access. Bila investigasi di masa depan benar-benar membutuhkan file user, capability tersebut harus tetap:

**FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION**

Minimum decision gates sebelum capability dapat dibuat:

1. use case dan legal/business authority yang spesifik;
2. user request/consent atau enterprise legal basis yang disetujui;
3. pemilihan exact artifact, version, purpose, recipient, dan time window—bukan folder/account-wide access;
4. permission khusus yang tidak terkandung dalam Admin/Support/Auditor;
5. fresh reauthentication dan, bila policy mensyaratkan, independent approval;
6. read-only secure viewer atau controlled transfer; no unrestricted storage browsing;
7. classification/need-to-know check, expiry, revocation, dan no onward sharing;
8. complete safe evidence atas request, approval, access, download/export, expiry, dan denial;
9. user/owner notification bila kebijakan mengharuskan;
10. review Document 09, Document 10, Security, Legal/Compliance, dan Pertamina policy.

Reauthentication atau approval tidak boleh mengubah permission biasa menjadi akses tak terbatas.

---

## 6. Sensitive metadata access

Metadata dapat sensitif walaupun bukan file content. Proyeksi per persona harus berbeda dan minimum.

| Metadata | Sensitivitas/risiko | User | Administrator | Support | Auditor |
|---|---|---|---|---|---|
| Principal/account reference, unit/function | Identity/confidential | Own safe profile | Sesuai account-admin scope | Reporter reference minimum pada scoped issue | Approved actor/unit evidence dalam audit scope |
| Role/permission assignment | Privileged security metadata | Own effective capability summary bila UI perlu; no hidden policy internals | Read/manage sesuai delegation | Tidak default | Read administrative-change evidence, bukan assignment write |
| Filename/path | Dapat mengungkap isi/kegiatan | Own resource | Tidak default; aggregate/count preferred | Tidak default | Tidak default dalam audit |
| Job ID | Confidential correlation handle | Own job | Safe operational lookup bila permission | Issue-linked safe job metadata only | Bila tercatat pada approved event |
| Correlation ID | Confidential diagnostic handle | Own error/status | Safe operational search | Issue-linked correlation only | Approved evidence/search field |
| Error Code | Safe stable code | Own context | Aggregate/detail sesuai operations permission | Scoped issue | Approved evidence |
| Raw exception/stack/SQL/query | Dapat mengungkap secret/path/content/architecture | Never client-facing | Tidak melalui ordinary admin UI; sanitized diagnostics only | No | No |
| Storage key/raw object path | Direct-object pivot | No | No ordinary UI | No | No |
| Issue message | Restricted case content | Own issue | No by admin role | Scoped issue detail permission | No by auditor role |
| Issue screenshot/attachment | Restricted user-submitted content | Own issue | No by admin role | Separate scoped attachment permission | No by auditor role |
| Internal note | Restricted support content | No default | No by admin role | Separate note read/write | No by auditor role |
| Audit event metadata | Controlled evidence | Own event view only if product explicitly offers; not baseline | Only separate Audit permission | Only separate Audit permission | Controlled read/export |
| Configuration/policy/version | Security-sensitive control metadata | Effective filtered projection | Read/write separately | Effective behavior only if needed | Change evidence, not policy write |
| Password/token/cookie/secret/key | Credential/secret | No display | No display | No display | No display |
| Visual signature payload | Restricted content | Own explicit operation only | No | No | No |

Additional rules:

- authorization diterapkan sebelum count, filter, search, sort, cursor, detail, export, storage resolution, atau stream;
- list projection tidak memuat heavy/restricted field secara default;
- confidential identifiers tidak dimasukkan ke URL/log/export di luar kebutuhan dan policy;
- raw claim dump, debug bundle, crash dump, proxy log, dan database query capture mengikuti larangan content/log yang sama;
- export menggunakan allowlisted fields, bounded range/rows, safe filename, expiry, download reauthorization, dan formula/external-link neutralization;
- Support dan Auditor tidak boleh menggabungkan beberapa metadata aman untuk merekonstruksi content atau memperluas scope.

---

## 7. Cross-user access rules

### 7.1 Default

Semua cross-user access ditolak kecuali ada permission **dan** explicit server-derived scope untuk jenis resource tersebut. Nama role, organisasi yang sama, mengetahui ID, menerima link, atau pernah melihat event tidak cukup.

### 7.2 Authorized cross-user surfaces

| Persona | Cross-user surface yang dapat diotorisasi | Tidak boleh dipivot ke |
|---|---|---|
| Administrator | Safe account metadata, app eligibility, role/permission assignment, session revocation, aggregate operational metadata | File/result/content, issue attachment, Assistant content, credential |
| Support | Scoped issue list/detail, explicitly sent attachment, safe issue-linked diagnostic/job metadata, triage/note/export | Source/result/secured file, OCR, storage, other issues di luar scope |
| Auditor | Scoped safe audit/security/admin-change evidence dan bounded export | User content, issue content, configuration mutation, source event mutation |
| User | Tidak ada secara default; hanya own resources | Resource user lain |

### 7.3 Enforcement rules

1. Owner/scoped actor selalu diturunkan dari authenticated principal dan server-side assignment/policy.
2. Query/list di-scope sebelum count, filter, search, sort, paging, dan cursor creation.
3. Parent diotorisasi dan child relationship diverifikasi sebelum attachment/version/result/note diakses.
4. Setiap continuation, cursor, range request, download, export, retry, resume, restore, dan SignalR subscription diotorisasi ulang.
5. Permission/policy/assignment/resource-version change menginvalidasi akses lama dan link/export yang masih aktif sesuai policy.
6. Sensitive resource dapat menggunakan consistent concealed `404` untuk principal tanpa discovery permission.
7. “View as user”, arbitrary impersonation, delegated wildcard, direct SQL, raw storage browsing, dan backup browsing tidak tersedia.
8. Workload identity bukan human cross-user permission; Worker hanya melakukan operasi tujuan-spesifik pada job yang telah diotorisasi.
9. Temporary support/audit export tidak mewarisi lifetime resource asal; export memiliki ACL, expiry, dan download authorization sendiri.
10. Cross-user denied attempts, enumeration pattern, dan unusual export/access volume menghasilkan safe security signal.

---

## 8. Separation of duties

### 8.1 Minimum viable SoD

Sistem dapat mendukung separation of duties tanpa membuat role baru berlebihan melalui:

- permission read/write/export/attachment/content yang terpisah;
- scope delegation dan larangan self-assignment/self-approval;
- append-oriented audit/status history yang tidak dapat diubah oleh pelaku source action;
- typed/versioned policy dan optimistic concurrency;
- fresh reauthentication untuk high-impact action;
- approval kedua hanya pada action yang kebijakan Pertamina klasifikasikan kritis;
- periodic access review/recertification untuk privileged bundle;
- independent audit review atas perubahan privileged access;
- tidak ada wildcard `*`, role inheritance tanpa batas, atau hidden bypass.

### 8.2 Konflik kewenangan yang harus dicegah

| Konflik | Minimum enforcement | Status |
|---|---|---|
| Mengubah policy lalu menghapus evidence perubahan | Policy writer tidak memiliki audit-event update/delete; audit store append-oriented dan purge hanya lifecycle governed | **REQUIRED** |
| Memberi role/permission kepada diri sendiri | Target self-assignment/high-risk assignment ditolak atau membutuhkan independent approver sesuai policy | **RECOMMENDED**; exact approval **REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** |
| Mengubah permission dan menjadi satu-satunya reviewer perubahan sendiri | Auditor/reviewer independen atau periodic review; actor tidak dapat mengubah evidence | **RECOMMENDED** |
| Mengubah audit retention lalu menghapus periode yang sedang diperiksa | Legal hold/compliance bounds mengalahkan ordinary admin config; purge bukan ordinary admin action | **REQUIRED**; exact retention Document 10 |
| Support mengubah issue lalu menghapus status history/note evidence | Support mutation memakai append history; no ordinary delete/tamper permission | **REQUIRED** |
| Auditor memeriksa event lalu mengubah source user/policy/issue | Auditor bundle read-only; no source-plane write | **REQUIRED** |
| Requestor membuat sekaligus menyetujui bulk/high-risk export | Independent approval bila classification/volume policy mewajibkan | **REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** |
| Administrator memperoleh Auditor/Support bundle secara implisit | Bundle harus di-assign terpisah dan kombinasi dievaluasi | **REQUIRED** |
| Orang yang memegang support attachment access juga mengelola retention/purge attachment | Pisahkan access dan lifecycle/purge permission atau wajibkan independent evidence/review | **RECOMMENDED** |
| Break-glass user mereview/menghapus evidence penggunaan sendiri | Jika future, review independen dan immutable evidence wajib | **FUTURE** |

### 8.3 Kombinasi persona

Satu orang secara teknis mungkin mempunyai lebih dari satu permission bundle, tetapi union tidak boleh otomatis diterima untuk kombinasi sensitif. Exact incompatible-role/permission matrix, dual-control threshold, delegable scope, dan approval owner adalah **REQUIRES_ENTERPRISE_POLICY_CONFIRMATION**.

Rekomendasi default:

- Administrator tidak otomatis Auditor atau Support;
- Support tidak otomatis Administrator atau Auditor;
- Auditor tidak otomatis Administrator atau Support;
- account/permission administrator tidak dapat mengesahkan privilege miliknya sendiri;
- Auditor yang mereview perubahan tertentu tidak menjadi pelaku perubahan tersebut;
- developer/DBA/operations identity tidak dimasukkan ke persona bisnis ini dan tidak otomatis memperoleh business-content access.

---

## 9. Reauthentication needs

Reauthentication membuktikan freshness identity untuk satu aksi; reauthentication **bukan** elevation, tidak menambah permission, dan tidak menggantikan ownership/policy/approval.

| Aksi | Persona | Rekomendasi freshness | Catatan |
|---|---|---|---|
| Ordinary permitted tool execution | User | Session valid; reauth hanya bila action/classification policy meminta | Confirmation tetap terpisah dari reauth |
| Permanent secured-file deletion | User | Fresh reauth ketika policy mewajibkan | Exact trigger/age: confirmation required |
| High-classification upload/share/export/transfer | User | Fresh reauth sesuai classification policy | Consent dan content policy tetap wajib |
| Role/permission assignment atau revocation | Administrator | Fresh reauth; self-assignment denied; independent approval bila policy | High-impact privileged action |
| Security/classification/retention policy change | Administrator | Fresh reauth; possible dual approval | Hard safety/legal hold tidak dapat dilewati |
| Account disable/enable atau broad session revocation | Administrator | Fresh reauth untuk high-impact scope | Target dan reason/purpose harus spesifik |
| Feature emergency disable/maintenance/minimum-version change | Administrator | Fresh reauth bila blast radius tinggi | Routine low-risk read tidak memerlukan |
| Limit/quota change | Administrator | Fresh reauth bila scope/blast radius melebihi threshold | Exact threshold configurable/policy-owned |
| Issue attachment download atau high-sensitivity view | Support | Current auth plus step-up berdasarkan classification/risk | Tidak harus meminta reauth untuk setiap routine list; risk-based/scoped |
| PDF/Excel bulk or high-impact issue export | Support | Fresh reauth; approval bila volume/classification policy | Download diotorisasi ulang |
| Audit bulk/high-impact export | Auditor | Fresh reauth; approval bila policy | Query/detail biasa menggunakan current session + permission |
| Exceptional cross-user content access | Separate future permission | Fresh reauth selalu + purpose + expiry + approval | **FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** |
| Break-glass activation | Future | Fresh strong enterprise authentication + independent approval where possible | **FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION** |

Fresh context harus short-lived dan terikat pada principal, session, client, action, resource/scope, dan policy version; invalid setelah logout, revocation, disablement, permission change, atau expiry. Missing freshness menghasilkan safe `401 REAUTHENTICATION_REQUIRED` sebelum partial mutation.

Exact assurance method, maximum age, retry/lock behavior, step-up mechanism, dan action matrix adalah **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION**.

---

## 10. Audit-required privileged actions

Bagian ini hanya menentukan **keluarga aksi yang memerlukan evidence**. Document 10 tetap menentukan katalog event final, field, integrity mechanism, retention/legal hold, reporting, dan purge authority.

| Actor/surface | Aksi yang wajib menghasilkan safe audit evidence | Evidence minimum secara konseptual | Dilarang masuk evidence |
|---|---|---|---|
| Administrator — account | Application-account eligibility change, disable/enable, session revocation, assignment scope change | Actor, target opaque ID, action/outcome, time, reason/purpose code bila required, policy/version, Correlation ID | Password, token, raw claims, content |
| Administrator — role/permission | Assign/revoke, delegation change, denied self-assignment, approval/denial | Actor, target, permission/bundle reference, before/after version/reference, approver bila ada, outcome | Credential atau user-content |
| Administrator — policy/config | Create/version/activate policy, security/classification/retention change, feature flag, limit, maintenance/minimum-version change | Actor, typed key/scope, safe before/after version/hash/reference, outcome, correlation | Secret value, document content, raw exception |
| Administrator — operations | Privileged diagnostic detail access dan high-impact operational action | Actor, scoped resource reference, purpose/action/outcome | Raw dump, file/path/storage key, content |
| Support — access | Issue detail read where policy requires, attachment view/download, safe diagnostics access | Actor, Issue/Attachment ID, access type, assignment/scope, time/outcome | Attachment bytes, issue text copy, token, document |
| Support — mutation | Category/priority/status/duplicate/not-bug/need-info/assignment change, internal note action | Actor, Issue ID, transition/action, version, time/outcome | Full note/message in audit |
| Support — export | PDF/Excel export request, generation, download/expiry/revocation | Actor, filter/scope reference, issue/count range, artifact ID, outcome | Export bytes/content |
| Auditor — access/export | Sensitive audit detail access, query/export request, export download, scope change | Actor, filter/scope reference, bounded count/time window, artifact ID, outcome | Raw restricted content |
| Content-access future | Request, approval, denial, open/view/download, expiry/revocation | Purpose, exact artifact/version, actor/approver, time window, outcome | Document bytes/text in audit |
| Break-glass future | Request/activation/use/command/access/termination/review | Actor, authority, purpose, start/end, actions, independent review | Credential/content payload |
| All privileged surfaces | Reauth success/failure, authorization denial, unusual enumeration/export, concurrency conflict | Safe action/resource code, actor, result, reason code, correlation | Secret, raw exploit payload, content |

Audit actor tidak boleh dapat mengubah atau menghapus evidence atas aksinya. Issue message/note/screenshot tetap berada di support plane, bukan disalin ke enterprise audit.

---

## 11. Break-glass assessment

### 11.1 Current decision

Current Document 01–08 tidak mendukung implicit break-glass, emergency content access, root account, unlimited superuser, arbitrary impersonation, atau storage browsing. Karena itu rekomendasi untuk Document 09 adalah:

```text
BREAK-GLASS STATUS:
FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION
```

### 11.2 Mengapa tidak dibuat sekarang

- belum ada confirmed enterprise use case, authority, IdP/MFA profile, approval chain, atau incident policy;
- current architecture sudah menyediakan ordinary governed controls untuk feature emergency disable, maintenance mode, minimum app version, session revocation, queue/backpressure, dan service recovery;
- menambah break-glass tanpa governance menciptakan bypass terbesar terhadap permission, content separation, dan audit independence;
- Support investigation tidak sama dengan emergency access dan tidak membenarkan God Mode.

### 11.3 Jika Pertamina kemudian mewajibkan

Capability future harus mempunyai identity khusus/non-shared, no permanent standing access where feasible, explicit incident/purpose, fresh enterprise authentication, shortest possible scope/time, independent approval where operationally possible, no wildcard, no automatic content access, complete immutable evidence, immediate alert, post-use review, credential revocation/rotation, dan periodic exercise. Exact offline contingency, approval-unavailable case, recovery authority, serta content access tetap harus diputuskan oleh Enterprise Security/Legal/Compliance dan Document 09/10.

Emergency feature disable yang dilakukan dengan ordinary `admin.feature.write` atau maintenance permission bukan break-glass selama masih mengikuti normal authentication, permission, scope, reauth, dan audit.

---

## 12. Risks

| ID | Risiko | Dampak | Minimum control / treatment |
|---|---|---|---|
| `PA-RISK-001` | “Superadmin” ditafsirkan sebagai God Mode | Wildcard access, content disclosure, privilege escalation | Definisikan sebagai Support permission bundle; no inheritance/wildcard |
| `PA-RISK-002` | Administrator self-assign atau mengesahkan privilege sendiri | Account takeover internal dan policy bypass | Self-assignment denial, delegable scope, reauth, independent approval sesuai policy |
| `PA-RISK-003` | Administrator mengubah policy sekaligus menghapus evidence | Hilangnya akuntabilitas | Append-oriented audit; no audit update/delete; governed purge/legal hold |
| `PA-RISK-004` | Feature Catalog label `AUDITOR/ADMIN` dianggap implied audit access | SoD runtuh dan audit oversharing | Audit permission assigned separately; role name not authorization |
| `PA-RISK-005` | Support memakai Job/Correlation ID untuk membuka result/file | Excessive cross-user disclosure | Issue-linked safe projection; no content pivot; per-object reauthorization |
| `PA-RISK-006` | Screenshot issue berisi data sensitif | Disclosure kepada Support/export recipient | Preview/crop/redact/remove, explicit Send, case ACL, permission, retention |
| `PA-RISK-007` | Support list memuat message/thumbnail/note lengkap | Bulk disclosure | Minimal list projection; detail/attachment/note permissions separated |
| `PA-RISK-008` | PDF/Excel atau audit export menjadi bulk exfiltration path | Data leakage dan uncontrolled copies | Bounded export, field allowlist, reauth, approval by threshold, ACL/expiry/audit |
| `PA-RISK-009` | Internal notes berisi secret atau document copy | Restricted data persistence dan propagation | Guidance, validation/DLP policy if approved, restricted permission, no audit copy |
| `PA-RISK-010` | Safe metadata digabung untuk profiling/reidentification | Privacy leakage | Minimized projections, scope, purpose, retention, export limits, monitoring |
| `PA-RISK-011` | Offline cached privilege tetap digunakan setelah revocation | Stale privileged access | Admin/Support/Auditor denied offline; reconnect refresh before command |
| `PA-RISK-012` | Reauthentication dianggap elevation | Permission bypass | Reauth only proves freshness; all normal authorization remains mandatory |
| `PA-RISK-013` | Role/permission sprawl dan hidden inheritance | Least privilege sulit diaudit | Canonical permission catalog, versioned bundles, no wildcard, periodic review |
| `PA-RISK-014` | Auditor juga menjadi actor atas perubahan yang diaudit | Self-review dan conflict of interest | Independent reviewer assignment; immutable evidence; combination policy |
| `PA-RISK-015` | Ordinary admin retention menghapus audit/legal evidence | Compliance failure | Separate audit retention/purge authority; legal hold precedence; Document 10 |
| `PA-RISK-016` | Error/diagnostic UI membocorkan raw exception/path/query | Security-sensitive disclosure | Safe diagnostic schema, restricted projection, no raw dumps/content |
| `PA-RISK-017` | Issue attachment permission diwariskan dari issue list | Unexpected screenshot download | Separate attachment permission and per-download authorization |
| `PA-RISK-018` | Break-glass dibuat sebagai shared permanent root | Unattributed unrestricted access | Do not implement now; future enterprise-confirmed, individual, scoped, timed, audited |
| `PA-RISK-019` | Support/admin assignment scope salah konfigurasi | Cross-unit/cross-region exposure | Server-derived scope, preview/test, versioning, audit, access review |
| `PA-RISK-020` | Multiple persona union menghasilkan toxic combination | Escalation atau evidence tampering | Incompatible-permission policy, review/approval, deny self-management |

Residual risk tetap ada pada malicious insider dengan legitimate scoped access, user oversharing dalam screenshot/message, serta copied export setelah authorized download. Acceptance memerlukan owner, compensating control, review/expiry, dan Pertamina risk authority.

---

## 13. Conflicts

| ID | Sumber/ketegangan | Interpretasi yang direkomendasikan | Status keputusan |
|---|---|---|---|
| `PA-CONF-001` | `FR-ROLE-001` mengunci role awal User, Administrator, Auditor; Feature/Requirement memakai istilah Superadmin/Support | Jangan membuat role keempat otomatis. Implementasikan Superadmin sebagai named Support permission bundle; mapping ke role/persona diputuskan Document 09 | User + Integrator |
| `PA-CONF-002` | Feature Catalog `FEAT-AUDIT-*` menulis UI `AUDITOR/ADMIN`, sedangkan API/Security mensyaratkan Auditor permission terpisah | Administrator boleh audit hanya jika permission Audit secara eksplisit diberikan; tidak implied oleh role | User + Integrator; SoD policy confirmation |
| `PA-CONF-003` | Business Process menyebut Administrator “mengelola user dan role”, sedangkan identity authority berada pada Pertamina | Batasi OTO admin pada mapping/eligibility/session/role-permission aplikasi; password/MFA/enterprise lifecycle tetap di Pertamina Identity | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `PA-CONF-004` | Administrator mengatur retention, tetapi audit retention/purge harus tunduk compliance/legal hold | Pisahkan ordinary data-retention configuration dari audit retention/purge authority; final rule di Document 10 | Document 10 + Enterprise Compliance |
| `PA-CONF-005` | Admin job/error monitoring memerlukan diagnosa, tetapi tidak boleh membaca document content | Berikan aggregate/safe metadata dan issue-linked correlation; tidak ada input/result/content access | User + Integrator |
| `PA-CONF-006` | API mengizinkan job access untuk “authorized support scope”, tetapi exact safe job fields belum dikunci | Support hanya memperoleh issue-linked safe job projection; field/scope final perlu dikunci | Document 09/API addendum if needed |
| `PA-CONF-007` | `FR-SUP-007` menyebut detail satu issue “lengkap” ke PDF; screenshot/message dapat sensitif | “Lengkap” berarti seluruh field yang **diotorisasi** untuk export, bukan unrestricted source document/log. Attachment inclusion harus policy-driven | User + Integrator |
| `PA-CONF-008` | Organizational Support scope (enterprise/unit/region/assignment) belum ditentukan | Scope harus server-derived dan permission-gated; jangan default seluruh enterprise tanpa policy | `REQUIRES_ENTERPRISE_POLICY_CONFIRMATION` |
| `PA-CONF-009` | Reauthentication categories sudah ada, tetapi method/age/threshold belum ditentukan | Pertahankan triggers sebagai policy categories; jangan hardcode assurance/age | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `PA-CONF-010` | Requirement belum menyediakan emergency privileged access | Tidak implementasikan break-glass; label future | `FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION` |
| `PA-CONF-011` | Satu principal dapat memegang beberapa persona, tetapi toxic combinations belum didefinisikan | Gunakan permission bundle terpisah, self-assignment denial, dan policy incompatible combinations | `REQUIRES_ENTERPRISE_POLICY_CONFIRMATION` |
| `PA-CONF-012` | Support membutuhkan internal notes, tetapi retention/disclosure/search detail belum ditentukan | Treat note as restricted support content; permission and retention separate; final records rule tidak dibuat di report ini | Document 09/10 |
| `PA-CONF-013` | Auditor membutuhkan security events, tetapi Document 10 belum mendefinisikan event catalog | Document 09 hanya memberi read/export boundary; event/field/integrity/retention tetap Document 10 | Document 10 |
| `PA-CONF-014` | Controlled user-document access mungkin dibutuhkan untuk investigasi, tetapi tidak ada requirement | Jangan infer capability. Jika disetujui kelak, buat separate scoped workflow, bukan perluasan Support/Admin | `FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION` |

Tidak ada konflik di atas yang memberi alasan untuk menghapus feature. Konflik diselesaikan melalui permission granularity, scope, reauth, audit evidence, dan keputusan enterprise—bukan God Mode.

---

## 14. Suggested Role Matrix entries

### 14.1 Legend

| Simbol | Arti |
|---|---|
| `OWN` | Hanya resource milik principal saat ini, tetap memerlukan action permission/policy |
| `SCOPED-R` | Read pada server-derived explicit scope |
| `SCOPED-W` | Mutation pada server-derived explicit scope dan workflow/state rule |
| `SEPARATE` | Tidak diwariskan dari persona; perlu permission bundle terpisah |
| `NO` | Tidak diizinkan pada persona/bundle tersebut |
| `FUTURE` | Tidak tersedia sekarang; memerlukan enterprise confirmation/change control |

### 14.2 Matrix usulan

Permission code di bawah adalah semantic proposal untuk Document 09, bukan perubahan Feature Catalog atau implementasi.

| Capability / suggested permission | User | Administrator | Support/Superadmin | Auditor | Scope/guard utama |
|---|---:|---:|---:|---:|---|
| Discover permitted features — `feature.discover` | `OWN` | `OWN` | `OWN` | `OWN` | Filter current permission/policy/platform |
| Execute canonical tool — `feature.execute` | `OWN` | `OWN` only if personally assigned | `OWN` only if personally assigned | `OWN` only if personally assigned | Persona privileged tidak otomatis memberi tool/content access |
| Own local file/workspace — `workspace.own.manage` | `OWN` | `OWN` | `OWN` | `OWN` | Per-profile/device boundary |
| Own job/result read — `job.own.read` | `OWN` | `OWN` | `OWN` | `OWN` | Other-user job requires separate safe metadata scope only |
| Own job cancel/retry/resume — `job.own.control` | `OWN` | `OWN` | `OWN` | `OWN` | Current permission/state/policy |
| Own result download — `result.own.download` | `OWN` | `OWN` | `OWN` | `OWN` | Reauthorize every stream/range |
| Personal history read/search — `history.own.read` | `OWN` | `OWN` | `OWN` | `OWN` | Audit remains separate |
| Personal history delete — `history.own.delete` | `OWN` | `OWN` | `OWN` | `OWN` | Never deletes audit/job/file |
| Secured file manage/read — `secured_file.own.manage` | `OWN` | `OWN` | `OWN` | `OWN` | Owner, quota, state/version/policy |
| Secured file permanent delete — `secured_file.own.delete_permanent` | `OWN` | `OWN` | `OWN` | `OWN` | Confirmation + reauth when policy |
| Own preference read/write — `preference.own.manage` | `OWN` | `OWN` | `OWN` | `OWN` | Allowlisted; enterprise policy wins |
| Use Assistant — `assistant.use` | `OWN` | `OWN` if assigned | `OWN` if assigned | `OWN` if assigned | No privilege bypass/content inheritance |
| Create issue — `issue.own.create` | `OWN` | `OWN` | `OWN` | `OWN` | Current user as reporter |
| Own issue read — `issue.own.read` | `OWN` | `OWN` | `OWN` | `OWN` | Simple reporter view |
| Send own issue attachment — `issue.own.attachment.send` | `OWN` | `OWN` | `OWN` | `OWN` | Preview + explicit Send |
| Account metadata read — `admin.account.read` | `NO` | `SCOPED-R` | `NO` | `NO`; change evidence via Audit | No credential/content |
| Account eligibility/session manage — `admin.account.manage` | `NO` | `SCOPED-W` | `NO` | `NO` | Reauth for high impact; IdP boundary |
| Role/permission catalog read — `admin.authorization.read` | `NO` | `SCOPED-R` | `NO` | `NO`; evidence via Audit | Canonical/versioned catalog |
| Role/permission assignment — `admin.authorization.assign` | `NO` | `SCOPED-W` | `NO` | `NO` | No self-assignment; delegation/approval |
| Policy read — `admin.policy.read` | Effective filtered only | `SCOPED-R` | Effective filtered only | Change evidence only | Security-sensitive metadata minimized |
| Policy write — `admin.policy.write` | `NO` | `SCOPED-W` | `NO` | `NO` | Typed/versioned, reauth, hard bounds |
| Feature flags read/write — `admin.feature.read` / `admin.feature.write` | Effective exposure only | `SCOPED-R/W` | Effective exposure only | Change evidence only | Cannot change feature meaning/safety |
| Limits read/write — `admin.limit.read` / `admin.limit.write` | Effective applicable limit | `SCOPED-R/W` | Effective applicable limit | Change evidence only | System safety ceiling wins |
| Safe system/health metadata — `admin.system_metadata.read` | Own/safe public status | `SCOPED-R` | Issue-linked safe subset | Approved evidence only | Aggregate/minimized; no content |
| Support issue list — `support.issue.list` | `NO` | `SEPARATE` | `SCOPED-R` | `NO` | Minimal projection; organizational/assignment scope |
| Support issue detail — `support.issue.detail` | `NO` | `SEPARATE` | `SCOPED-R` | `NO` | Message + safe context; per-issue auth |
| Support attachment read — `support.issue.attachment.read` | `NO` | `SEPARATE` | `SCOPED-R` | `NO` | Explicitly sent items only; no source document |
| Support triage/status — `support.issue.triage` | `NO` | `SEPARATE` | `SCOPED-W` | `NO` | Typed transition + `If-Match` + audit |
| Support notes — `support.issue.note.read/write` | `NO` | `SEPARATE` | `SCOPED-R/W` | `NO` | Restricted internal content |
| Support safe job metadata — `support.job.safe_metadata.read` | `NO` | `SEPARATE` | `SCOPED-R` | `NO` | Issue-linked; no input/result/content |
| Support PDF export — `support.issue.export.pdf` | `NO` | `SEPARATE` | `SCOPED-W` | `NO` | Bounded, authorized fields, expiry/audit |
| Support Excel export — `support.issue.export.excel` | `NO` | `SEPARATE` | `SCOPED-W` | `NO` | Bounded filters/rows, safe cells |
| Audit query — `audit.query` | `NO` | `SEPARATE` | `SEPARATE` | `SCOPED-R` | Scope before count/search/filter |
| Audit detail — `audit.detail` | `NO` | `SEPARATE` | `SEPARATE` | `SCOPED-R` | Safe evidence only; no content pivot |
| Audit export — `audit.export` | `NO` | `SEPARATE` | `SEPARATE` | `SCOPED-W` request only | Bounded async, reauth/high-risk approval |
| Audit source update/delete — none | `NO` | `NO` | `NO` | `NO` | No ordinary human permission |
| Other-user document/result/secured content | `NO` | `NO` | `NO` | `NO` | Explicit future workflow only |
| Password/token/cookie/secret/key | `NO` | `NO` | `NO` | `NO` | Never ordinary UI/DB/log/audit |
| Other-user signature/clipboard/OCR/Assistant content | `NO` | `NO` | `NO` | `NO` | No implicit permission |
| Controlled content access — future code TBD | `FUTURE` | `FUTURE` | `FUTURE` | `FUTURE` | Separate exact-artifact workflow, never role inheritance |
| Break-glass — future code TBD | `FUTURE` | `FUTURE` | `FUTURE` | `FUTURE` | Enterprise-confirmed only; no permanent wildcard |

### 14.3 Assignment rules usulan

1. Role/bundle tidak mempunyai wildcard dan tidak otomatis saling mewarisi.
2. Baseline persona tetap `User`, `Administrator`, dan `Auditor`; Support/Superadmin adalah named permission bundle sampai diputuskan lain.
3. Read, write, attachment, export, audit, content, dan approval permission dipisahkan.
4. Assignment membawa scope, issuer/assigner, effective/expiry time bila sesuai, policy version, dan review state; exact data design ditentukan Integrator/Document 10.
5. Principal tidak dapat self-assign, memperluas delegation scope sendiri, atau menyetujui privileged assignment miliknya.
6. Privileged bundle tidak eligible offline.
7. System safety invariant dan audit immutability bukan permission yang dapat dicabut dari UI.
8. Removal/revocation menginvalidasi session/cursor/link/export/queued command sesuai policy dan memicu reauthorization.
9. Toxic combination dan dual-control threshold harus dapat dikonfigurasi melalui enterprise policy, tetapi tidak di-hardcode tanpa konfirmasi.
10. Semua role matrix entries tetap diperiksa bersama ownership, policy/classification, state/version, capability, consent, freshness, dan resource scope.

---

## 15. Suggested Document 09 text

Teks berikut dapat dipertimbangkan Integrator untuk Document 09.

### 15.1 Privileged-access principles

```text
PERTAMINA OTO uses deny-by-default, permission-based, object-aware access.
Role is a grouping of permissions and never the final authorization decision.

Allow = authenticated and active principal
    + valid session
    + action permission
    + resource ownership or explicit server-derived scope
    + enterprise policy and classification
    + resource state/version
    + required consent/confirmation
    + fresh reauthentication when required.

Administrative authority, Support authority, Audit authority, and
user-content authority are independent. No role grants wildcard access,
impersonation, arbitrary storage browsing, or direct SQL.
```

### 15.2 Persona boundary

```text
User
  May use permitted canonical tools and manage own files, jobs/results,
  history, secured files, issues, preferences, and Assistant state.
  Ownership is derived from the authenticated principal. Deleting personal
  history or Assistant conversation never deletes enterprise audit.

Administrator
  May manage application account eligibility, delegated role/permission
  assignments, typed policy/configuration, feature exposure, limits, and
  safe system metadata according to granular permission and scope.
  Administrator does not automatically receive user document/result/OCR,
  issue attachment, password/token, clipboard, signature, or Assistant
  content access. Self-assignment and self-approval are prohibited.

Support/Superadmin
  “Superadmin” is a Support permission bundle, not God Mode and not an
  automatic new role. Permissions for issue list, detail, explicitly sent
  attachment, triage/status, internal note, safe diagnostics, issue-linked
  safe job metadata, PDF export, and Excel export are separate. Support does
  not automatically access source documents, results, secured files, full
  OCR text, credentials, signature, clipboard, or unrelated user data.

Auditor
  Has controlled read access to approved audit/security/administrative-change
  evidence, filtered search, detail, and separate bounded export permission.
  Auditor cannot modify/delete source events, user data, issues, policy,
  configuration, roles, or permissions and receives no document-content
  authority.
```

### 15.3 Content and cross-user rule

```text
Cross-user access is denied unless both an explicit permission and a
server-derived resource scope exist. Opaque ID, Job ID, Correlation ID,
cursor, link, filename, or storage reference never proves access.

Issue screenshot/attachment access is limited to the exact item explicitly
sent by the reporter and requires separate Support attachment permission.
Job/Correlation IDs allow safe correlation only and cannot be pivoted to
input, result, secured file, OCR text, or storage content.

No current Admin, Support, or Auditor capability provides privileged access
to user documents. Any future investigation-content workflow is
FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION and requires exact artifact
scope, purpose, authority/consent, fresh reauthentication, expiry,
independent approval where policy requires, and complete safe evidence.
```

### 15.4 Separation of duties and reauthentication

```text
The actor who changes policy, role, permission, or issue state cannot alter
or delete the evidence of that action. Audit source events are append-oriented
and have no ordinary human update/delete permission. Personal history deletion
does not affect enterprise audit.

Administrator, Support, and Auditor bundles are assigned separately.
High-risk combinations, delegation scope, dual-approval thresholds, and
periodic access review follow confirmed Pertamina policy. No principal may
self-assign or self-approve privileged access.

Fresh reauthentication is action- and scope-bound and does not elevate
permission. Candidate triggers include role/permission/security-policy change,
session revocation, permanent secured-file deletion, high-impact disclosure
or export, and any future exceptional content access. Exact assurance method
and maximum age require Pertamina standard confirmation.
```

### 15.5 Audit and break-glass boundary

```text
Privileged account, authorization, policy/configuration, Support access and
mutation, attachment access, export, Auditor export, reauthentication, and
authorization-denial actions produce safe audit evidence. Audit never copies
document bytes/text, issue attachment bytes, password, token, clipboard,
signature, full OCR text, or secret. Document 10 owns the final event catalog,
fields, integrity, legal retention, hold, purge, and reporting rules.

PERTAMINA OTO has no current root account, unlimited Superadmin, implicit
break-glass, emergency content access, or arbitrary impersonation.
Break-glass status is:
FUTURE / REQUIRES_ENTERPRISE_POLICY_CONFIRMATION.
Normal emergency feature disable, maintenance mode, session revocation, and
service recovery remain governed Administrator/Operations actions and do not
create wildcard access.
```

---

## Closing recommendation

Document 09 sebaiknya mengunci **permission semantics dan separation rules terlebih dahulu**, lalu memetakan permission tersebut ke role/bundle sesuai keputusan user, Integrator, dan kebijakan Pertamina. Keputusan ini menjaga kemampuan sistem tetap lengkap tanpa menciptakan root role, Superadmin God Mode, atau privileged content bypass.

