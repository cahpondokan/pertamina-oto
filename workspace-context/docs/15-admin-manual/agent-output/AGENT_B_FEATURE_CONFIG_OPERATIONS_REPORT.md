# AGENT B — FEATURE, CONFIGURATION, LIMIT, JOB & STORAGE ADMINISTRATION

**Task ID:** `DOC15-AGENT-B`  
**Status:** Rekomendasi specialist untuk integrasi Document 15  
**Sumber berwenang:** Document 01–14 FINAL, terutama Document 03, 04, 05, 06, 07, 12, dan 14  
**Batas:** Prosedur administrasi control plane dan metadata operasional; bukan Deployment Guide, bukan direct database runbook, dan bukan kewenangan membuka isi dokumen.

## Baseline yang wajib dipertahankan

- Feature Registry memiliki **550 canonical feature records** dan **33 Primary PDF Tool Cards**. Admin tidak membuat Feature ID baru dan tidak mengubah arti record.
- Seluruh 33 Core Tool masih berstatus **Belum tersedia pada versi ini** pada User Manual FINAL sampai implementation, release exposure, dan UAT execution terbukti.
- Desktop adalah target full-feature utama; Web adalah kanal enterprise/browser; Phone dan Tablet adalah companion terkurasi. Exact exposure release tetap harus berasal dari registry/capability manifest dan bukti RC.
- Local-capable operation tidak dipindahkan ke server hanya karena admin mengubah konfigurasi. Dari baseline API: 19 Core cards local-only dan 14 mempunyai optional-server path; tidak ada Core card yang universal required-server.
- Draft, working state, undo/redo, cache, thumbnail, temp, clipboard, password, signature asset, dan checkpoint lokal tidak menjadi resource administrasi server.
- File hanya masuk Secured Files melalui tindakan eksplisit **Amankan ke Akun**. Maksimum tiga versi aktif/addressable adalah business invariant, bukan angka kuota yang boleh dinaikkan admin.
- Persisted job state adalah source of truth. SignalR hanya notifikasi; REST/current-state refresh memulihkan tampilan.
- Administrator tidak otomatis memperoleh akses isi dokumen, result, OCR, password, clipboard, visual signature, attachment Issue, object-storage key, atau storage browsing.
- Seluruh perubahan control plane harus daring, typed, terversi, melalui application/API resmi, memakai optimistic concurrency, reauthentication bila berdampak tinggi, dan menghasilkan Audit yang aman.

---

# 1. Feature Registry administration

## 1.1 Tujuan tampilan admin

Feature Registry Admin adalah tampilan baca atas descriptor yang dideploy, ditambah control state yang memang boleh dikelola. Registry bukan editor katalog dan bukan editor kode.

| Kelompok | Field minimum | Mode admin | Catatan |
|---|---|---|---|
| Identitas | `FeatureId`, name/display key, aliases | `VIEW_ONLY` | ID canonical, case-stable, tidak didaur ulang |
| Klasifikasi | `RecordType`, `UiExposure`, `BuildPriority`, priority/release, status katalog | `VIEW_ONLY` | Tidak mengubah scope atau meaning |
| Contract | input/output contract summary, `ContractVersion`, `ParameterSchemaVersion` | `VIEW_ONLY` | Perubahan memerlukan code/release, impact analysis, regression |
| Eksekusi | `ExecutionProfile`, `ProcessingMode`, `ServerDependency`, `OfflineSupport` | `VIEW_ONLY` | Admin tidak memilih engine/vendor |
| Resource | storage impact, network impact, concurrency risk | `VIEW_ONLY` | Menjadi input limit/capacity, bukan override bebas |
| Security | risk, destructive classification, permission family | `VIEW_ONLY` | Hard security semantics tidak dapat diedit |
| Platform | deployed maximum exposure Desktop/Web/Phone/Tablet | `VIEW_ONLY` baseline | Effective exposure dapat dipersempit melalui control terpisah |
| Dependency | dependency family, approved provider/capability availability | `VIEW_ONLY` health | Provider code aman; tanpa endpoint, secret, atau topology |
| Operasional | enabled state, feature flag, maintenance, effective availability reason | `EDITABLE` hanya pada control resmi | Tidak mengubah descriptor canonical |
| Limit/policy | assigned typed policy/limit profile dan effective version | `EDITABLE` sesuai permission | Hanya dalam hard bounds |
| Compatibility | App/API/contract/capability versions | Campuran | Minimum version dapat dikelola; contract/capability build tidak |

## 1.2 Prosedur melihat record

1. Buka **Administrasi > Feature Registry** pada environment yang benar.
2. Pastikan environment, client version, dan `CapabilityVersion` terlihat sebelum melakukan pencarian.
3. Cari dengan exact `FeatureId`; pencarian nama/alias hanya untuk discovery.
4. Verifikasi name, record type, UI exposure, execution profile, processing mode, server dependency, offline support, platform exposure, risk, release/status, `ParameterSchemaVersion`, dan provider availability.
5. Buka panel **Effective state** untuk melihat asal nilai: deployed default, environment, enterprise policy, feature flag, lalu runtime capability.
6. Jika status tidak tersedia, baca reason code aman dan dependency health; jangan menganggap flag `enabled` berarti fitur dapat dieksekusi.
7. Gunakan Correlation ID hanya untuk penelusuran diagnostik berwenang; identifier bukan bukti akses.

## 1.3 Guard perubahan

Admin **tidak boleh** mengubah melalui UI:

- `FeatureId`, name canonical/meaning, alias semantics;
- input/output contract atau `ParameterSchemaVersion`;
- risk, destructive classification, permission requirement;
- processing implementation, provider class/vendor, atau server dependency contract;
- Feature status menjadi released tanpa build, test, UAT, dan release evidence;
- deployed platform maximum menjadi capability yang belum diimplementasikan.

Jika descriptor salah, admin membuat Issue/change request yang menyebut Feature ID, environment, app/capability version, safe observed state, dan Correlation ID bila ada. Admin tidak memperbaikinya melalui database atau key/value bebas.

---

# 2. 33 Core Tool administration

## 2.1 Panel administrasi per card

Setiap Core Tool Card harus menampilkan:

```text
Primary Tool name + canonical Feature ID mapping
Release/current availability
Enabled/disabled control state
Platform exposure: deployed maximum + effective value
Local/server profile
Provider/engine capability state
Maintenance state
Current/Minimum/Contract/Capability version compatibility
Assigned limit profile + effective policy version
Safe error-rate/job-duration/queue/transfer health summary
Last configuration change + Audit correlation
```

Error/health summary hanya berisi aggregate safe metadata. Nama/path file, content, password, OCR text, signature asset, prompt, raw exception, storage key, dan provider secret dilarang.

## 2.2 Matriks 33 Core untuk admin

**Current release state seluruh baris:** `BELUM TERSEDIA` sampai exact RC/platform exposure/UAT evidence tersedia. Tabel berikut adalah posture contract, bukan tombol untuk mengaktifkan implementasi yang belum dirilis.

| No | Core Tool | Canonical Feature ID | Processing posture | Dependency/gate | Fokus pemeriksaan admin |
|---:|---|---|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | Local | Local PDF capability | Exposure; file/count/temp limit; local health tidak dinilai dari server queue |
| 2 | Split PDF | `FEAT-PDF-006 / 007` | Local | Local PDF capability | Page/range limit; platform exposure |
| 3 | Remove pages | `FEAT-PDF-013` | Local | Local PDF capability | Exposure; source/finality guard tetap code-owned |
| 4 | Extract pages | `FEAT-PDF-014` | Local | Local PDF capability | Page/output count limit |
| 5 | Organize PDF | `FEAT-PDF-010..016` | Local workflow | Bundled capabilities | Semua member mapping; subset platform; no partial semantic rewrite |
| 6 | Scan to PDF | `FEAT-SCAN-014` | Local/device | Physical scanner PoC; camera/device capability | Platform/device exposure; import fallback; permission device |
| 7 | Compress PDF | `FEAT-PDF-023` | Local | Local PDF capability | Size/temp limit; no target-size promise |
| 8 | Repair PDF | `FEAT-PDF-025 / 026` | Optional server untuk advanced fallback | Approved fallback capability | Provider health; fallback policy; failure summary |
| 9 | OCR PDF | `FEAT-OCR-004` | Local-first; optional server untuk complex/large | OCR PoC/provider | Language/corpus profile; CPU/RAM/temp; provider quota |
| 10 | JPG to PDF | `FEAT-CONV-007` | Local | Image/PDF capability | File/count/output limits |
| 11 | Word to PDF | `FEAT-CONV-001` | Optional server | Approved conversion provider; fidelity PoC | Provider/version; queue; timeout; policy exposure |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | Optional server | Approved conversion provider; fidelity PoC | Provider/version; queue; timeout |
| 13 | Excel to PDF | `FEAT-CONV-002` | Optional server | Approved conversion provider; fidelity PoC | Sheet/page/export limits; provider health |
| 14 | HTML to PDF | `FEAT-CONV-017` | Optional server | Approved isolated renderer; security/fidelity PoC | Provider isolation; remote-resource policy; timeout |
| 15 | PDF to JPG | `FEAT-CONV-012` | Local | Local PDF/image capability | Page/output-size limits |
| 16 | PDF to Word | `FEAT-CONV-004` | Optional server | Approved conversion provider; fidelity PoC | Provider/version; queue; failure rate |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | Optional server | Approved conversion provider; fidelity PoC | Provider/version; queue; failure rate |
| 18 | PDF to Excel | `FEAT-CONV-005` | Optional server | Approved conversion provider; table fidelity PoC | Provider/version; table/output limits |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | Optional server | Approved converter/validator; conformance PoC | Profile support; validator health; no false conformance |
| 20 | Rotate PDF | `FEAT-PDF-012` | Local | Local PDF capability | Exposure; page limit |
| 21 | Add page numbers | `FEAT-PDF-037` | Local | Local PDF capability | Page/range/output limits |
| 22 | Add watermark | `FEAT-PDF-036` | Local | Local PDF capability | Exposure; resource limits; not DRM/redaction |
| 23 | Crop PDF | `FEAT-PDF-019` | Local | Local PDF capability | Exposure; not redaction |
| 24 | Edit PDF | `FEAT-PDF-031..041` | Local baseline; optional server for existing-text subset | Existing-text editing PoC | Exact released subset; provider/version; limitation visibility |
| 25 | PDF Forms | `FEAT-PDF-053..060` | Local workflow baseline | Form capability; advanced subset may vary | Exact member availability; flatten policy; platform subset |
| 26 | Unlock PDF | `FEAT-PDF-062` | Local | Valid password/authority only | High-risk state; no password visibility; reauth policy |
| 27 | Protect PDF | `FEAT-PDF-061` | Local | Local PDF security capability | High-risk policy; password never stored |
| 28 | Sign PDF | `FEAT-SIGN-013` | Local visual non-PKI | Signature asset capability | Explicit non-PKI label; asset remains local/no auto-sync |
| 29 | Redact PDF | `FEAT-PDF-069` | Local | Approved true-removal capability | High-risk finalization; validation health; not overlay-only |
| 30 | Compare PDF | `FEAT-PDF-071 / 072` | Local/Local Worker | Local comparison capability | Worker health; duration/temp; platform exposure |
| 31 | AI Summarizer | `FEAT-INTEL-001` | Local preferred; optional approved internal provider | Intelligence provider PoC | Public AI prohibited; provider/quota/timeout/circuit; content policy |
| 32 | Translate PDF | `FEAT-INTEL-002` | Local preferred; optional approved internal provider | Translation provider/fidelity PoC | Provider/language availability; quota; no public endpoint |
| 33 | PDF to Markdown | `FEAT-CONV-036` | Deterministic local first; optional OCR/layout path | Extraction/OCR fidelity PoC | OCR/provider dependency only when needed; output-size limit |

## 2.3 Verifikasi card setelah perubahan

Untuk setiap perubahan availability, flag, exposure, provider policy, maintenance, compatibility, atau limit:

1. cocokkan exact Feature ID/card mapping;
2. pastikan release status bukan `Belum tersedia` bila hendak diekspos;
3. verifikasi platform berada dalam deployed maximum;
4. verifikasi provider/engine build telah approved dan sehat;
5. verifikasi permission, policy, limit, dan compatibility;
6. uji discovery melalui jalur UI/API resmi pada akun uji sesuai scope;
7. pastikan local-only card tidak menghasilkan processing upload/server traffic;
8. periksa safe Audit event dan effective `CapabilityVersion`;
9. bila hasil berbeda, rollback control state melalui aplikasi resmi atau eskalasi—jangan mengubah descriptor/database.

---

# 3. Feature enable/disable

## 3.1 Makna

`Enabled/Disabled` adalah keputusan availability operasional terhadap Feature yang sudah ada. Control ini tidak mengubah contract, release readiness, permission, risk, platform implementation, atau provider capability.

```text
Enabled
≠ released
≠ visible pada semua platform
≠ authorized untuk semua user
≠ provider sehat
≠ kapasitas tersedia
```

Disable hanya mempersempit availability. Enable hanya mengembalikan eligibility untuk dievaluasi oleh exposure, permission, policy, capability, resource, provider, connectivity, dan compatibility.

## 3.2 Prosedur

1. Cari exact Feature ID dan periksa environment.
2. Periksa deployed release/status dan current effective state.
3. Periksa dependent capability/provider, platform maximum, current jobs, dan user impact.
4. Tentukan scope perubahan yang paling sempit: satu Feature, environment, platform, atau approved audience.
5. Pilih enable/disable dan isi reason code, bounded ticket/reference, serta effective window bila tersedia.
6. Preview impact: cards/routes/platforms, local vs server path, active/new jobs, preset/workflow dependency, dan fallback.
7. Lakukan fresh reauthentication jika perubahan broad/high-impact.
8. Konfirmasi exact environment, Feature ID, old state, new state, dan scope.
9. Simpan melalui control plane dengan current ETag/row version.
10. Verifikasi effective state dari pembacaan baru; jangan mengandalkan snackbar saja.
11. Periksa Audit event dan Correlation ID.

## 3.3 Perilaku job yang sudah ada

- Disable tidak boleh diam-diam mengubah status database atau membatalkan job aktif.
- Admission job baru dapat ditolak sesuai policy setelah perubahan efektif.
- Job aktif mengikuti policy transition yang disetujui: continue safely, withhold result, cooperative cancel, atau controlled failure. Exact behavior harus ditampilkan pada impact preview.
- Jika transition policy belum didefinisikan, perubahan broad harus diblok, bukan ditebak admin.

---

# 4. Feature flags

## 4.1 Kegunaan yang sah

Feature flag hanya untuk staged rollout, pilot, QA, scoped exposure, atau emergency disable. Flag tidak digunakan untuk:

- mengubah business meaning;
- mengganti input/output contract;
- melewati permission/security validation;
- memilih vendor/provider secara arbitrer;
- memindahkan proses local-only ke server;
- mengklaim Feature released sebelum gate selesai.

## 4.2 Data flag minimum

```text
FeatureId
Environment
Scope/audience/platform yang typed
Enabled state atau approved rollout mode
ReasonCode + bounded change reference
EffectiveFrom/Until bila digunakan
Policy/ConfigurationSchemaVersion
RowVersion/ETag
Owner/reviewer metadata
LastChangedAtUtc
```

Nilai rahasia, query audience bebas, arbitrary JSON, code expression, dan nama engine/provider mentah tidak boleh dimasukkan.

## 4.3 Prosedur minimum yang wajib

1. Cari Feature ID.
2. Periksa environment.
3. Periksa current status.
4. Periksa dependent capability.
5. Tentukan perubahan dan scope terkecil.
6. Isi alasan.
7. Preview impact.
8. Reauthenticate dan konfirmasi bila berdampak tinggi.
9. Simpan dengan optimistic concurrency.
10. Verifikasi melalui effective configuration/discovery.
11. Periksa Audit event.

Setiap flag mempunyai owner, tujuan, review/expiry, dan exit decision: promote melalui release terkontrol, keep dengan alasan, atau remove. Emergency flag tidak boleh menjadi konfigurasi permanen yang tidak terpantau.

---

# 5. Platform exposure

## 5.1 Nilai exposure

| Nilai | Arti administrasi |
|---|---|
| `FULL` | UX/capability terlengkap yang terbukti pada exact platform/build |
| `PRIMARY` | Diprioritaskan dan sesuai pada platform tersebut |
| `SUPPORTED` | Tersedia dengan capability normal yang dibuktikan |
| `LIMITED` | Hanya subset/parameter/use case yang terdokumentasi |
| `NOT_EXPOSED` | Tidak ditampilkan pada platform tersebut |

Exposure bukan permission dan bukan bukti bahwa runtime mampu mengeksekusi. Urutan akhir tetap:

```text
Deployed platform maximum
→ Administrative exposure/flag yang tidak melebihi maximum
→ Device capability
→ Permission
→ Enterprise policy/classification
→ Resource/storage
→ Engine/provider availability
→ Connectivity/compatibility
→ Execute / fallback / hide / explain
```

## 5.2 Prosedur perubahan exposure

1. Pilih exact Feature ID, platform, environment, dan release channel.
2. Bandingkan current effective exposure dengan deployed maximum dan release capability manifest.
3. Jangan menaikkan `NOT_EXPOSED/LIMITED` menjadi `SUPPORTED/FULL` tanpa implementation, platform test, UAT, documentation, dan RC evidence.
4. Preview dampak pada Tool-First, File-First, Assistant-First, search, preset/workflow, Help, dan localization.
5. Pastikan fallback/back path tersedia agar tidak ada dead-end.
6. Simpan perubahan typed/versioned dengan reason, ETag, confirmation, dan reauth bila broad.
7. Verifikasi pada exact real platform/build dan periksa `CapabilityVersion` serta Audit.

Current exact 33-Core exposure selain posture target masih `TBC`. Admin tidak boleh mengubah User Manual atau availability matrix berdasarkan konfigurasi saja; publication memerlukan release evidence.

---

# 6. Policy configuration

## 6.1 Kategori policy

| Kategori | Contoh | Admin action |
|---|---|---|
| Processing | server-processing permission, workload class, approved route | Update typed policy dalam scope |
| File transfer | upload purpose, size/count, consent/classification | Update typed policy dalam safety ceiling |
| Security/classification | provider allowance, high-risk confirmation/reauth, redaction/security profile | High-impact; fresh reauth dan review |
| Storage | secured quota, retention category, cleanup window | High-impact bila classification/retention |
| Job | admission, concurrency, timeout, retry class, checkpoint/recovery | Typed; tidak mengubah state langsung |
| Provider | approved capability profile, availability policy, fallback | Tidak mengisi endpoint/secret; provider registration deployment-owned |
| Compatibility | minimum supported client, maintenance | Staged validation; safe rollback |
| Issue/export | attachment/export limits and retention | Typed; content access tetap permission terpisah |

## 6.2 Prosedur update policy

1. Pilih policy key yang terdaftar; key/value arbitrer ditolak.
2. Verifikasi environment, scope, schema/version, current value, source, dan hard bounds.
3. Lihat affected Features/platforms/workload/provider dan active resource implications.
4. Masukkan typed value, reason code, approved reference, dan effective time bila didukung.
5. Preview diff versi/digest; raw unrestricted before/after JSON dilarang.
6. Lakukan reauthentication untuk security, classification, retention, atau broad-impact policy.
7. Simpan dengan `If-Match`; pada `412/409`, reload dan evaluasi ulang—jangan overwrite.
8. Verifikasi `GET effective configuration`/tampilan efektif, health, dan smoke evidence yang sesuai.
9. Periksa Audit event.

## 6.3 Hard invariants yang tidak dapat dinonaktifkan

- permission/ownership/resource-state checks;
- mandatory file validation/quarantine/security inspection;
- mandatory Audit intent dan sensitive-data exclusion;
- larangan password/token/content/signature/clipboard masuk log;
- larangan public AI dan arbitrary provider/URL/SQL/shell;
- no silent overwrite, Local-First, selective sync eksplisit;
- maksimum tiga active secured versions;
- hard safety ceiling file/job/resource;
- digital signature PKI tetap di luar scope.

---

# 7. Limit/quota administration

## 7.1 Status angka

Seluruh nilai numerik yang belum disetujui berstatus `TARGET_TO_BE_APPROVED`. Setelah benchmark, security/load review, dan approval, nilainya menjadi typed versioned `POLICY_CONFIGURABLE`. Hard safety ceiling tetap `VIEW_ONLY` dan tidak dapat dilonggarkan admin.

| Area | Limit yang harus terdokumentasi | Status saat belum disetujui |
|---|---|---|
| Upload | byte/file, byte/batch, file count, chunk bytes, concurrent streams/sessions, inactivity/expiry | `TARGET_TO_BE_APPROVED` |
| File/page/item | file count, page count, image/sheet/slide count, decompression/structure budget | `TARGET_TO_BE_APPROVED` |
| Concurrent job | per subject, workload category, provider, environment/global | `TARGET_TO_BE_APPROVED` |
| Queue | bounded depth/cost, oldest age, admission/drain threshold | `TARGET_TO_BE_APPROVED` |
| Timeout | API request, one-shot upload, chunk/session, job stage, provider, export/download | `TARGET_TO_BE_APPROVED` |
| Retry | max attempt, backoff, jitter band, retry window, circuit behavior | `TARGET_TO_BE_APPROVED` |
| Secured storage | byte/file/account quota, version size, account-recycle/retention | `TARGET_TO_BE_APPROVED`; version count fixed max 3 |
| Issue attachment | count, file/batch bytes, retention, export bounds | `TARGET_TO_BE_APPROVED` |
| OCR/conversion/AI | weighted cost, CPU/RAM/temp/output, context/call/concurrency/provider quota | `TARGET_TO_BE_APPROVED` |
| Export | row/page/file/byte/date-window, concurrent exports, artifact expiry | `TARGET_TO_BE_APPROVED` |

## 7.2 Data yang ditampilkan

```text
LimitKey + typed unit
Environment/scope/workload/Feature profile
Configured value
Effective value
Effective source
Hard ceiling/floor summary
PolicyVersion + ConfigurationSchemaVersion
Measured baseline reference
Approval/change reference
EffectiveFrom/Until
Last changed actor/time
```

## 7.3 Prosedur perubahan

1. Buka category dan pastikan unit tidak ambigu.
2. Bandingkan measured baseline, current load, headroom, hard ceiling, dependent proxy/API/Worker/storage/provider limit, dan cleanup capacity.
3. Ubah hanya satu typed limit set yang disetujui atau gunakan bundled change yang memiliki impact preview.
4. Jangan menaikkan limit untuk menghilangkan `429/503` tanpa membuktikan bottleneck dan headroom.
5. Jangan menurunkan limit di bawah active reservation tanpa transition behavior yang disetujui.
6. Preview affected request/job, expected HTTP semantics, wait/reject message, dan rollback.
7. Reauthenticate bila broad/high-impact; simpan dengan ETag.
8. Pantau accepted/rejected rate, queue, resource, error, dan cleanup setelah perubahan.
9. Periksa Audit event `Configuration.LimitChanged`.

Effective limit selalu nilai paling ketat dari system safety, enterprise policy, environment capacity, subject scope, endpoint/workload, Worker/storage/database, dan approved provider capacity.

---

# 8. Job management

## 8.1 Batas kewenangan

Baseline role memberi Administrator `monitoring.job.read` untuk **safe aggregate job/queue/sync metadata**, bukan permission umum atas job milik pengguna. Endpoint Job pada Document 06 adalah owner/authorized scope dan cancel/retry/resume adalah tindakan pemilik yang diotorisasi ulang.

Karena itu prosedur berikut berlaku untuk tampilan metadata yang diizinkan. Tombol action lintas-owner tidak boleh muncul sampai permission, resource scope, API contract, Audit, SoD, dan UAT ditetapkan melalui revisi terkendali.

## 8.2 Pencarian

Admin yang berwenang dapat menggunakan filter aman berikut:

- exact Job ID;
- exact Correlation ID;
- Feature ID;
- status canonical;
- workload/processing mode (`LOCAL`/`SERVER`/approved hybrid projection);
- provider profile code aman;
- environment/platform/app version;
- time range;
- masked owner reference hanya bila permission/scope mengizinkan.

Search/list harus menerapkan authorization sebelum count/filter. Job ID dan Correlation ID tidak memperluas scope.

## 8.3 Detail aman

| Area | Boleh ditampilkan | Dilarang |
|---|---|---|
| Identity | Job ID, Correlation ID, Feature ID, masked owner ref | Raw identity/claims tanpa scope |
| State | status, attempt number, state version, wait reason, progress summary | Mengedit status/progress |
| Time | created/queued/started/completed, duration, queue age | Timestamp internal yang membocorkan topology bila tak perlu |
| Execution | processing mode, workload class, safe provider profile, platform/app version | Engine class, endpoint, credential, host topology |
| Recovery | checkpoint supported/available/stage/resume allowed/last saved | Raw checkpoint bytes/token/storage key |
| Cancellation | requested/canCancel, safe reason | Forced process/database mutation |
| Error | stable Error Code, retryable, resumeAvailable, safe guidance | Raw exception, stack, SQL, path, content |
| Result | available/partial/expired summary dan size category/bytes bila scoped | Download/open content melalui admin monitoring |

## 8.4 Interpretasi status

| Status | Arti operasional | Tindakan admin aman |
|---|---|---|
| `QUEUED` | Durable, menunggu dispatch/resource turn | Periksa queue class, age, admission, provider/resource; jangan ubah DB |
| `RUNNING` | Worker mempunyai execution lease | Periksa duration, heartbeat/lease health, resource trend |
| `WAITING_CONNECTION` | Koneksi/dependency dibutuhkan | Periksa dependency health; informasikan no-silent-wait |
| `WAITING_RESOURCE` | Resource/capacity belum aman | Periksa CPU/RAM/storage/DB/provider dan queue fairness |
| `RETRYING` | Retry allowlisted dan bounded sedang dijadwalkan/berjalan | Periksa attempt/backoff/reason; cegah retry storm |
| `COMPLETED` | Required output tervalidasi; outcome success/allowed partial | Periksa result availability/expiry; jangan ubah hasil |
| `FAILED` | Terminal failure aman | Periksa retryable/resumeAvailable dan Error Code |
| `CANCELLED` | Terminal cancellation telah persist | Tidak menghidupkan ulang status; retry harus command baru yang sah |

---

# 9. Queue management

## 9.1 Tampilan minimum

Queue dashboard menampilkan safe aggregate per environment dan workload class:

```text
admission state
queued/active/waiting/retrying count
oldest queue age
arrival/service/completion rate
accepted/rejected/cancelled/failed rate
worker busy/available aggregate
provider/storage/database dependency state
estimated drain trend bila evidence memadai
retry rate and attempt distribution
```

Queue harus persistent, bounded, fair, dan workload-separated. Tampilan admin tidak mengekspos raw topology, host name, connection detail, unrestricted tenant/user list, atau content.

## 9.2 Tindakan aman

- batasi admission baru memakai approved typed policy/feature flag/maintenance scope;
- turunkan concurrency dalam approved range ketika CPU/RAM/storage/DB/provider tertekan;
- pertahankan local-capable workflows dan jangan memindahkannya ke server;
- gunakan `Retry-After`, `WAITING_RESOURCE`, atau rejection aman sesuai kondisi;
- biarkan persisted queue/lease recovery melakukan reclaim; jangan memindahkan row manual;
- jalankan cleanup/reconciliation resmi yang bounded dan pressure-aware jika storage/temp menjadi penyebab;
- eskalasi ke owner Worker/DB/storage/provider bila health tidak pulih.

Admin tidak menghapus message/job, mengubah priority/status/lease, atau memaksa dequeue melalui database. Reprioritization lintas-user bukan baseline.

---

# 10. Retry/cancel

## 10.1 Cancel

- `QUEUED`: dapat menjadi `CANCELLED` segera melalui command resmi.
- `RUNNING`: cooperative cancellation; response dapat `202`, Worker berhenti pada safe point, kemudian state persisted menjadi `CANCELLED`.
- cancel ulang pada job cancelled mengembalikan state yang sama.
- unsafe cancellation menghasilkan `409 JOB_NOT_CANCELLABLE`.
- `COMPLETED`/finalized output tidak diubah menjadi cancelled.

Prosedur action yang berwenang:

1. reauthorize actor, owner/scope, current state, state version, permission, policy, dan compatibility;
2. tampilkan Job ID, Feature ID, current status, effect, serta apakah result/temp akan dipertahankan/dibersihkan;
3. minta confirmation; reauth bila policy mewajibkan;
4. gunakan command resmi dengan `Idempotency-Key` yang sama pada ambiguous retry;
5. baca persisted state sampai terminal; SignalR bukan bukti final;
6. verifikasi Audit evidence.

## 10.2 Retry

- Automatic retry hanya untuk transient allowlist, bounded attempts, exponential backoff + jitter.
- Manual retry membuat **attempt baru pada logical Job ID yang sama**; tidak membuat duplicate logical job.
- Permission Feature, input ownership, policy, provider, capacity, schema, result retention, dan classification dinilai kembali.
- Same idempotency key + same fingerprint mengembalikan job/resource yang sama; same key + different fingerprint menghasilkan `409`.
- Permanent validation/security failure tidak auto-retry.
- Retry tidak boleh hot loop dan tidak boleh mengubah failed partial/intermediate menjadi final.

## 10.3 Resume

Resume hanya bila checkpoint cocok dengan input identity, Feature/parameter schema, engine/provider version, stage, digest/integrity, dan expiry. Incompatible checkpoint menghasilkan `409 CHECKPOINT_NOT_RESUMABLE`; expired checkpoint menghasilkan `410 CHECKPOINT_EXPIRED`. Sistem tidak silent restart dari awal.

**Current authorization caveat:** admin cross-owner cancel/retry/resume belum mempunyai permission dan endpoint resmi pada baseline Document 06/09. Manual harus menampilkan monitoring read-only sampai controlled decision tersedia.

---

# 11. Secured Files metadata

## 11.1 Batas akses

Secured Files adalah resource milik user. Administrator tidak otomatis mendapat `secured.file.read`, `secured.version.read`, atau download permission milik user lain. `monitoring.system.read` dapat menampilkan storage aggregate yang aman; per-file cross-owner metadata memerlukan permission dan API contract terpisah yang belum ada.

Tidak ada tindakan admin yang otomatis memberi download/open content. Object key, physical path, delegated download token, encryption key reference, checksum penuh pada layar luas, dan byte content dilarang.

## 11.2 Metadata yang dapat ditampilkan bila scope resmi tersedia

| Field | Aturan tampilan |
|---|---|
| Secured File/Version ID | Opaque; full hanya pada exact authorized detail |
| Owner reference | Masked/minimized; tidak menjadi pivot ke content |
| Display name/extension/MIME | Confidential metadata; tampil hanya jika benar-benar diperlukan oleh permission |
| Size | Exact atau category sesuai projection/scope |
| Current state/upload status | Typed state; tidak dapat diedit admin |
| Storage status | Safe healthy/missing/reconciliation summary tanpa lokasi fisik |
| Version count/current version | Maksimum tiga active/addressable |
| Retention category/timestamps | Policy code/version; legal hold restricted |
| Quota | Subject/category aggregate yang diizinkan |
| Orphan/cleanup | Reconciliation state/count dan last attempt, tanpa storage key |
| Correlation/Job reference | Safe link untuk diagnostics; bukan content grant |

## 11.3 Prosedur operasional

1. Mulai dari aggregate storage health, quota, cleanup, dan orphan count.
2. Jika permission detail tersedia, cari exact opaque Secured File ID; jangan menggunakan filename untuk enumeration luas.
3. Verifikasi owner scope, active version count, total size, upload/storage state, retention/hold, recycle/tombstone, dan cleanup state.
4. Jika count active melebihi tiga atau current pointer tidak konsisten, tandai integrity incident dan gunakan official reconciliation workflow.
5. Jika metadata ada tetapi object hilang, jangan membuat object placeholder atau membuka version; eskalasi integrity/storage recovery.
6. Jika object diduga orphan, jangan delete manual; lifecycle worker harus memverifikasi lease/reference/tombstone/hold lalu cleanup secara idempotent.
7. Jika quota penuh, jelaskan limit dan tindakan user/policy yang sah; jangan menghapus versi/content tanpa lifecycle/retention authority.
8. Periksa safe Audit/monitoring result dari tindakan resmi.

## 11.4 Maksimum tiga versi

```text
Create validated version N
→ activate atomically
→ bila active count menjadi 4, tombstone oldest active version
→ commit current pointer + active count
→ post-commit cleanup/reconciliation
```

Failure purge fisik dapat menghasilkan cleanup pending, tetapi tidak boleh memulihkan akses ke versi yang sudah ditombstone. Angka tiga tidak dapat diubah menjadi empat melalui quota/configuration.

---

# 12. Version management

## 12.1 Empat versi compatibility

| Field | Pemilik | Admin behavior |
|---|---|---|
| `CurrentVersion` | Release/channel manifest | `VIEW_ONLY`; versi client terbaru yang disetujui |
| `MinimumSupportedVersion` | Typed compatibility policy | `EDITABLE` dengan scope, staged validation, reauth bila broad |
| `ContractVersion` | API/release contract | `VIEW_ONLY`; perubahan memerlukan deployment/compatibility path |
| `CapabilityVersion` | Registry/exposure/capability snapshot | `VIEW_ONLY`; berubah dari published snapshot, bukan input bebas |

## 12.2 Tindakan per status

| Status | Tindakan admin |
|---|---|
| `SUPPORTED` | Pertahankan operasi normal; pantau error/compatibility drift |
| `UPDATE_AVAILABLE` | Tampilkan notice non-blocking; operasi compatible tetap berjalan; verifikasi adoption dan support window |
| `UPDATE_REQUIRED` | Blokir server action yang incompatible/security-sensitive secara aman; local data/workspace tidak dirusak; pastikan alasan nyata dan route update disetujui |
| `UNSUPPORTED` | Fail closed pada kombinasi tidak dikenal/dihapus; jangan memaksa execute; arahkan update atau controlled rollback/support |

## 12.3 Mengubah minimum version

1. Verifikasi security/compatibility reason, affected platform/channel/API major, adoption data aman, dan tested oldest/current/newer matrix.
2. Pastikan long-running job, stored result, secured file, cursor, idempotency, checkpoint, EventVersion, dan mixed-version rollout mempunyai compatible path.
3. Preview affected client population dan local-data behavior.
4. Gunakan staged change, exact effective time, rollback, fresh reauth, dan `If-Match`.
5. Verifikasi `/api/compatibility` dari supported/below-minimum/current client fixture.
6. Periksa `Configuration.MinimumClientVersionChanged` Audit event.

Forced update hanya untuk kebutuhan security/compatibility nyata, bukan untuk menyederhanakan support operasional.

---

# 13. Configuration hierarchy

## 13.1 Evaluasi efektif

Hierarchy yang diminta admin adalah composition, bukan unrestricted last-write-wins:

```text
System Safety/Security Invariants (selalu menang)
→ Default deployed baseline
→ Environment boundary
→ Enterprise Policy
→ Feature Flag / exposure control
→ Runtime Capability and current resource/provider/connectivity check
→ Effective behavior
```

Layer berikutnya boleh mempersempit atau memilih opsi yang diizinkan layer sebelumnya, tetapi tidak dapat melampaui hard guard. Runtime Capability adalah fakta saat ini dan dapat menolak/menurunkan availability; ia tidak mengubah contract.

## 13.2 Matriks kontrol

| Layer | Contoh | View/Edit | Reauth | Audit | Deployment |
|---|---|---|---|---|---|
| System Safety/Security | ownership, validation, no secret log, hard ceiling, no public AI | `VIEW_ONLY` summary | N/A untuk admin UI | Perubahan code/policy khusus wajib | **YES**, controlled release |
| Default | canonical descriptor, default exposure/profile/schema | `VIEW_ONLY` | N/A | Release/change evidence | **YES** |
| Environment | approved endpoints/profile/capacity class/provider registration | `VIEW_ONLY` pada Admin UI | Ops process | Configuration/deployment evidence | **YES** atau approved environment operation; bukan edit UI bebas |
| Enterprise Policy | classification, transfer, retention, limit profile, provider allowance | `EDITABLE` typed dalam scope | **YES** untuk high-impact | **YES** | No untuk nilai policy yang memang runtime-configurable |
| Feature Flag | pilot/rollout/emergency disable/exposure scope | `EDITABLE` typed | Conditional; **YES** bila broad/high-impact | **YES** | No |
| Runtime Capability | device/engine/provider/storage/resource/connectivity health | `VIEW_ONLY`, derived | No | Monitoring; configuration change terpisah bila ada | Tidak untuk state health; provider/engine install/change mengikuti release/ops resmi |

User Preference berada di bawah enterprise/system controls dan tidak termasuk control plane admin. Device configuration/capability tidak boleh dipakai untuk memperluas enterprise authorization.

---

# 14. Capacity warnings

Semua warning threshold numerik tetap `TARGET_TO_BE_APPROVED`. Dashboard harus menunjukkan measured value, trend/window, effective threshold source, environment, workload, dan last refresh; warna saja tidak cukup.

| Gejala | Pemeriksaan | Tindakan aman | Eskalasi |
|---|---|---|---|
| Queue meningkat | arrival vs service, oldest age, workload fairness, admission/reject, provider/resource | Batasi admission mahal; turunkan concurrency bila host tertekan; pertahankan local route; publish wait/retry guidance | Worker/capacity owner bila backlog tidak drain |
| Worker sibuk | CPU/RAM/GC, active lease, duration, crash/recycle, workload mix | Kurangi concurrency dalam approved range; pisahkan workload; jangan tambah retry | Platform/Worker owner jika saturation/residual naik |
| Provider unavailable | approved provider health, timeout/circuit, affected Feature, fallback policy | Set affected server path unavailable/maintenance; gunakan approved local path bila capability; tidak ke public provider | Provider/service owner dan Security bila policy impact |
| Storage hampir penuh | temp/result/upload/secured growth, active leases, cleanup throughput, orphan | Hentikan admission content baru sebelum unsafe; jalankan bounded lifecycle cleanup; lindungi active job/result | Storage owner/DBA; incident bila object integrity terancam |
| Database pressure | latency/I/O/log/tempdb/pool/wait/deadlock, query category | Kurangi admission/concurrency; hentikan export/cleanup berat yang nonkritis secara terkendali; tetap short transactions | DBA/application owner; jangan ubah data langsung |
| Upload gagal meningkat | status 413/408/409/429/503, digest/offset, temp capacity, network/proxy alignment | Bedakan hard limit, client retry, session resume, shared saturation; cleanup failed/expired session resmi | API/storage/network owner berdasarkan evidence |
| Long-running job | expected workload/corpus, current step/progress, resource trend, checkpoint, provider | Jangan cancel hanya karena durasi; tampilkan no-silent-wait; cancel hanya jika requested/authorized/safe | Feature/provider owner bila melewati approved behavior |
| Retry rate tinggi | Error Code class, attempt/backoff, idempotency reuse, provider/capacity | Stop retry storm; buka circuit/backpressure melalui policy resmi; permanent errors fail once | Reliability/provider/security owner sesuai cause |

Safe local capabilities tetap tersedia ketika server/provider terganggu. Initial small server adalah environment pilot untuk mengukur bottleneck dan headroom, bukan alasan menghapus Feature.

---

# 15. Troubleshooting

| Gejala | Kemungkinan aman | Pemeriksaan | Pemulihan |
|---|---|---|---|
| Feature tidak terlihat | Not released, `NOT_EXPOSED`, disabled, flag, permission, policy, capability, compatibility | Feature ID, environment, release state, exposure chain, `CapabilityVersion`, current user scope | Koreksi control yang memang salah; jika implementation absent, tetap unavailable |
| Feature enabled tetapi tidak berjalan | Provider/resource/connectivity/schema/permission deny | Effective availability reason, health, policy, app/contract version | Pulihkan dependency atau narrow/disable dengan pesan jelas; jangan ganti semantics |
| Perubahan config ditolak `409/412` | State/ETag stale | Reload current version dan diff | Evaluasi ulang lalu submit versi baru; jangan force overwrite |
| Request `413` | Hard file/body/batch/chunk limit | Actual bytes/count dan effective limit source | Kurangi input/mode; admin hanya ubah policy setelah approval/headroom |
| Request `429` | Caller quota/rate/concurrency | Scope/category, reservation, `Retry-After` | Tunggu sesuai guidance atau sesuaikan approved quota; bukan shared outage |
| Request `503` | Shared queue/Worker/provider/readiness unsafe | Health aggregate dan affected workload | Graceful reject/wait; recovery owner; jangan menambah retry serentak |
| Job `WAITING_CONNECTION` | Network/provider needed | Dependency/compatibility/permission refresh | Pulihkan koneksi dan revalidate; job state tetap durable |
| Job `WAITING_RESOURCE` lama | Capacity/storage/DB/provider pressure | Queue age/fairness/resource/lease | Backpressure dan capacity action; no DB state edit |
| Job `RETRYING` berulang | Transient loop/provider busy | attempt budget, Error Code, backoff/circuit | Stop bounded attempts ketika limit; provider/reliability escalation |
| Cancel tidak selesai | Worker belum safe point atau lease issue | `cancellation.requested`, `canCancel`, worker/lease health | Tunggu persisted terminal atau recovery resmi; jangan kill/update row |
| Resume ditolak | Checkpoint incompatible/expired | Feature/schema/input/engine/stage/digest/expiry | Gunakan retry dari awal hanya setelah user/owner action yang jelas |
| Upload offset conflict | Stale/overlap/digest mismatch | Session current offset/digest/state | Query session, resend exact uncommitted range; conflicting data ditolak |
| Result expired | Retention selesai | Result manifest/state/expiry | `410 RESULT_EXPIRED`; rerun bila input/permission masih sah; tidak resurrect object |
| Secured version count salah | Concurrent version/cleanup/reconcile failure | Active count/current pointer/tombstone safe state | Integrity incident + official reconciliation; no manual delete |
| Storage object hilang | Integrity/storage failure | Metadata/object reconciliation summary | Withhold access; restore/reconcile via approved process |
| Audit event tidak ditemukan | Commit/outbox/ingestion delay atau failure | Owning transaction outcome, outbox health, Correlation ID, Audit health | Retry dispatcher/reconcile; privileged mutation gagal tertutup bila intent wajib tidak durable |
| Compatibility `UPDATE_REQUIRED` tak terduga | Wrong policy/channel/version parse | Current/minimum/contract/capability values dan change Audit | Correct typed policy dengan staged rollback; local data tetap aman |

Error UI menampilkan stable code, localizable message, Correlation ID, dan typed safe details. Stack, SQL, path, storage key, secret, provider detail, queue topology, dan content tidak ditampilkan.

---

# 16. Audit expectations

## 16.1 Peristiwa yang relevan

| Tindakan | Audit evidence |
|---|---|
| Feature enable/disable, exposure, scoped flag yang mengubah exposure | `Configuration.FeatureExposureChanged` menurut typed event mapping |
| Limit/quota change | `Configuration.LimitChanged` |
| Maintenance change | `Configuration.MaintenanceModeChanged` |
| Minimum client version change | `Configuration.MinimumClientVersionChanged` |
| Security policy | `Policy.SecurityPolicyChanged` |
| Classification policy | `Policy.ClassificationChanged` |
| Retention policy | `Policy.RetentionPolicyChanged` |
| Job accepted/completed/failed/cancelled | `Processing.ExecutionAccepted`, `Processing.ExecutionCompleted`, `Processing.ExecutionFailed`, `Processing.ExecutionCancelled` |
| Material retry scheduled | `Processing.ExecutionRetryScheduled` |
| Secured file/version lifecycle | Event `SecuredFile.*` yang sesuai action resmi |
| Reauthentication | `Auth.ReauthenticationSucceeded/Failed` atau required evidence yang sesuai |

Jika implementation memerlukan event flag yang berbeda dari `Configuration.FeatureExposureChanged`, event baru tidak boleh dibuat oleh manual; Event Catalog Document 10 harus direvisi secara terkendali terlebih dahulu.

## 16.2 Bukti aman minimum

```text
AuditEventId + EventType/EventVersion
OccurredAtUtc + ReceivedAtUtc
Actor/initiator dan target/scope opaque yang diizinkan
Action + FeatureId/resource type/id
Environment/platform/app version
Old/new configuration version atau digest yang aman
ReasonCode + bounded change/ticket reference
Outcome + safe ErrorCode
CorrelationId dan JobId bila berlaku
Reauthentication/policy decision code bila diwajibkan
```

Audit tidak menyimpan password, token, secret, document/OCR content, filename/path secara default, signature image, clipboard, prompt/chat, screenshot, object key, raw exception, SQL, atau unrestricted before/after JSON.

## 16.3 Verifikasi pascaperubahan

1. Pastikan authoritative mutation berhasil dan effective state terbaca.
2. Cari Audit dengan Correlation ID/event type dalam scope Auditor/Admin yang terpisah sesuai SoD.
3. Cocokkan actor, Feature/policy/limit key, scope, old/new version/digest, time, reason, outcome, dan reauth code.
4. Pastikan tidak ada sensitive value dan tidak ada duplicate logical event.
5. Jika Audit intent wajib tidak dapat dibuat durable, privileged/high-impact mutation harus fail closed.

---

# 17. Screenshot requirements

Belum ada screenshot aktual. Semua screenshot final hanya boleh dibuat dari exact approved RC menggunakan synthetic/dummy data, locale yang disebut, platform/build/environment yang tercatat, serta privacy/security review. Mockup tidak disebut screenshot.

| Placeholder | State yang harus ditangkap | Redaksi/guard |
|---|---|---|
| `[SCREENSHOT-ADMIN-FEATURE-LIST-001]` | Registry list, Feature ID, status, effective availability | Dummy data; no owner/content |
| `[SCREENSHOT-ADMIN-FEATURE-DETAIL-001]` | Read-only descriptor + effective state provenance | Provider code aman; no endpoint/topology |
| `[SCREENSHOT-ADMIN-FEATURE-IMPACT-001]` | Enable/disable preview dan confirmation | Exact environment/scope; reason synthetic |
| `[SCREENSHOT-ADMIN-FLAG-001]` | Feature flag scope/version/expiry | No arbitrary audience query |
| `[SCREENSHOT-ADMIN-EXPOSURE-001]` | Desktop/Web/Phone/Tablet deployed max vs effective | Tunjukkan `LIMITED/NOT_EXPOSED` dengan jelas |
| `[SCREENSHOT-ADMIN-LIMIT-001]` | Typed limit, unit, configured/effective/hard bound | Gunakan non-Production example; label bukan official value |
| `[SCREENSHOT-ADMIN-JOB-SEARCH-001]` | Search Job/Correlation/Feature/status | Mask owner; synthetic IDs |
| `[SCREENSHOT-ADMIN-JOB-DETAIL-001]` | Status/progress/wait/error/checkpoint safe summary | No filename/path/content/checkpoint token |
| `[SCREENSHOT-ADMIN-QUEUE-001]` | Aggregate depth/age/rate/resource trend | No host/topology/raw user list |
| `[SCREENSHOT-ADMIN-JOB-ACTION-001]` | Cancel/retry impact confirmation bila contract disetujui | Jangan ambil sebelum permission/API tersedia |
| `[SCREENSHOT-ADMIN-SECURED-001]` | Aggregate/permitted metadata, version count, storage/cleanup | No download/open/object key; masked owner |
| `[SCREENSHOT-ADMIN-VERSION-001]` | Current/minimum/contract/capability + status | Exact channel/platform; synthetic policy change |
| `[SCREENSHOT-ADMIN-CONFIG-001]` | Effective hierarchy/provenance | Hard guard terlihat `VIEW_ONLY` |
| `[SCREENSHOT-ADMIN-CAPACITY-001]` | Warning → checks → safe action | No sensitive telemetry/topology |
| `[SCREENSHOT-ADMIN-AUDIT-001]` | Safe configuration Audit event | Mask Actor/resource according to scope |

Setiap screenshot memerlukan caption, alt text, purpose, platform/build, locale, state prerequisite, sensitive-region checklist, owner, dan stale trigger. Screenshot lama menjadi `OUTDATED` ketika UI, Feature schema, compatibility, exposure, permission, atau policy berubah.

---

# 18. Risks/conflicts

| Risiko/konflik | Dampak | Rekomendasi |
|---|---|---|
| Seluruh 33 Core masih pre-implementation pada Document 14 | Admin guide dapat terlihat menjanjikan availability palsu | Pertahankan `BELUM TERSEDIA`; publish hanya dari RC manifest + UAT evidence |
| Exact Feature × platform release exposure belum tersedia | Admin dapat mengekspos dead-end/unsupported UI | Bekukan upward exposure sampai release capability manifest disetujui |
| `/api/v1/features` saat ini adalah discovery terfilter dan API admin registry detail belum eksplisit | Feature Registry Admin lengkap belum mempunyai contract publik yang jelas | Controlled Document 06 addendum untuk read-only safe admin projection; jangan direct DB |
| Baseline Admin hanya `monitoring.job.read` safe aggregate; tidak ada cross-owner cancel/retry permission/route | Prompt job action dapat bertentangan dengan Document 09 | Jadikan read-only; keputusan resmi diperlukan sebelum action lintas-owner |
| Admin per-file secured metadata cross-owner belum memiliki permission/API contract | Risiko privacy/IDOR dan pivot ke content | Mulai dari aggregate; definisikan field/scope/SoD terpisah bila business need disetujui |
| Queue/admin monitoring endpoint tidak tercantum di inventory Document 06 | UI dapat mendorong query/akses tidak resmi | Tambahkan read-only bounded projection melalui controlled API addendum |
| Exact limit, timeout, retry, retention, threshold, compatibility window belum disetujui | Angka manual dapat dianggap kebijakan resmi | Gunakan `TARGET_TO_BE_APPROVED`; isi hanya dari typed policy version |
| Provider health/maintenance schema belum distandarkan | Availability reason dapat tidak konsisten antar Feature | Freeze typed provider/capability health projection dan safe reason codes |
| Feature flag `FEAT-ADMIN-005` berprioritas Should/R2 | Manual dapat mendahului release capability | Tampilkan hanya bila feature flag surface benar-benar released; enable/disable baseline tetap terpisah |
| Hierarchy prompt menyederhanakan priority sementara Document 04 menempatkan System Safety paling atas | Risiko last-write-wins yang melemahkan keamanan | Dokumentasikan composition + most-restrictive-wins dan hard invariant di atas semua layer |
| Monitoring owner reference/filename/size adalah confidential metadata | Kebocoran lintas-user dari dashboard | Aggregate/masking/default deny; detail hanya permission/scope eksplisit |
| Disable/maintenance saat active job belum memiliki transition policy per Feature/workload | Job dapat dihentikan atau hasil dilepas tidak konsisten | Impact preview wajib; blok broad change sampai transition behavior disetujui |

Decision dependencies:

```text
BLOCKED_BY_DECISION: RELEASE_FEATURE_PLATFORM_EXPOSURE_MANIFEST
BLOCKED_BY_DECISION: CONTROLLED_API_CONTRACT_FOR_ADMIN_FEATURE_REGISTRY_AND_QUEUE_PROJECTION
BLOCKED_BY_DECISION: ADMIN_CROSS_OWNER_JOB_CANCEL_RETRY_RESUME_PERMISSION_AND_API
BLOCKED_BY_DECISION: ADMIN_SECURED_FILE_METADATA_SCOPE_AND_API
BLOCKED_BY_DECISION: PERTAMINA_NUMERIC_LIMIT_RETENTION_REAUTH_AND_COMPATIBILITY_POLICY
BLOCKED_BY_DECISION: TYPED_PROVIDER_HEALTH_MAINTENANCE_AND_EFFECTIVE_AVAILABILITY_SCHEMA
```

Label di atas adalah dependency kerja laporan, bukan Feature ID baru dan bukan keputusan arsitektur otomatis.

---

# 19. Suggested Document 15 text

## Feature, Configuration, Job, and Storage Operations

Administrator mengelola control plane dan metadata operasional PERTAMINA OTO melalui aplikasi/API resmi. Administrator tidak mengedit source/config server, database, queue row, job state, storage object, atau canonical Feature contract secara langsung. Kewenangan administrasi tidak memberikan akses isi dokumen pengguna.

Feature Registry menampilkan canonical Feature ID, name, record type, UI exposure, execution profile, processing mode, server dependency, offline support, platform exposure, risk, release/status, parameter schema version, dan provider availability. Field identitas, meaning, input/output contract, risk, destructive classification, permission, dan implementation bersifat read-only. Admin hanya dapat mengelola control typed yang disetujui: enabled/disabled, feature flag, exposure dalam deployed maximum, policy, limit, maintenance, dan minimum client version.

Availability efektif dihitung sebagai berikut:

```text
System Safety/Security
→ Default deployed baseline
→ Environment boundary
→ Enterprise Policy
→ Feature Flag/exposure
→ Runtime Capability, resource, provider, connectivity, compatibility
→ Effective behavior
```

Layer bawah tidak dapat melewati hard guard. `Enabled` tidak berarti released, visible, authorized, compatible, atau healthy. Feature flag hanya untuk staged rollout, pilot, QA, dan emergency disable; bukan untuk mengubah business meaning.

Setiap perubahan control plane mengikuti prosedur:

1. cari exact Feature/policy/limit key dan environment;
2. periksa current/effective state, version, dependency, capacity, dan active-resource impact;
3. pilih scope terkecil dan masukkan reason code/reference;
4. preview exact old/new value dan affected platform/workload;
5. reauthenticate bila high-impact;
6. simpan typed update dengan `If-Match`/row version;
7. baca ulang effective state dan lakukan verification yang sesuai;
8. periksa Audit event melalui Correlation ID.

Angka upload, file/page count, concurrency, queue, timeout, retry, secured storage, Issue attachment, OCR/conversion/AI, dan export tetap `TARGET_TO_BE_APPROVED` sampai benchmark dan approval. Setelah disetujui, angka disimpan sebagai `POLICY_CONFIGURABLE` typed/versioned value dalam hard safety ceiling. Maksimum tiga secured versions adalah invariant dan tidak dapat dinaikkan admin.

Job Monitoring menggunakan persisted state dengan status canonical:

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

Admin dapat melihat safe aggregate/job projection sesuai permission: Job ID, Correlation ID, Feature ID, masked owner reference bila diizinkan, processing mode, provider profile aman, duration, queue/wait reason, checkpoint summary, cancellation capability, dan safe Error Code. Admin tidak dapat mengubah result, owner, status database, document content, atau password. Cancel/retry/resume hanya melalui application/API resmi dan harus diotorisasi ulang terhadap actor, owner/scope, current state/version, permission, policy, compatibility, dan idempotency. Baseline saat ini belum memberi Administrator action lintas-owner; UI tetap read-only sampai contract resmi tersedia.

Queue dipantau sebagai aggregate bounded per workload. Ketika tekanan meningkat, tindakan aman adalah mempersempit admission, menurunkan concurrency dalam approved range, mempertahankan local capability, memberi wait/retry guidance, menjalankan cleanup lifecycle resmi, dan mengeskalasi berdasarkan CPU/RAM/storage/DB/provider evidence. Admin tidak menghapus queue/job atau mengubah lease/priority/state melalui database.

Secured Files Admin dimulai dari aggregate quota, storage health, version count, retention, orphan, dan cleanup. Detail per-file lintas-owner bukan default permission. Admin tidak otomatis diberi download/open content. Metadata/object mismatch diperlakukan sebagai integrity incident dan dipulihkan melalui reconciliation resmi; object tidak dihapus manual. Maksimum tiga versi aktif tetap berlaku setelah retry, restore, dan cleanup.

Compatibility menampilkan `CurrentVersion`, `MinimumSupportedVersion`, `ContractVersion`, dan `CapabilityVersion` dengan status `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, atau `UNSUPPORTED`. Current/Contract/Capability version adalah read-only release facts; Minimum Supported Version adalah typed policy yang memerlukan staged validation, impact preview, reauthentication bila broad, rollback plan, dan Audit. Forced update hanya untuk kebutuhan security/compatibility nyata dan tidak boleh merusak local workspace.

Semua dashboard dan Audit menggunakan metadata aman. Password, token, secret, isi dokumen/OCR, clipboard, signature asset, prompt/chat, screenshot, raw exception, SQL, filesystem path, storage key, dan unrestricted provider detail tidak dicatat atau ditampilkan. Screenshot manual hanya dibuat dari exact approved RC dengan dummy data, masking, caption, alt text, dan privacy/security review.

