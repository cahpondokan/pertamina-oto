# AGENT C — SUPPORT CONSOLE, AUDIT, MONITORING & OPERATIONAL SECURITY REPORT

**Task ID:** DOC15-AGENT-C  
**Status:** RECOMMENDATION — keputusan final tetap milik User dan Integrator Document 15  
**Tanggal analisis:** 2026-09-01  
**Ruang lingkup:** panduan Administrator/Support/Auditor untuk operasi aman; tanpa coding, perubahan master Document 15, atau Incident Guide lengkap

## Baseline

Laporan ini diturunkan dari Document 01–14 FINAL, terutama Document 08 Security Design, Document 09 Role Matrix, Document 10 Audit Trail, Document 13 UAT, Document 14 User Manual, dan kontrak In-App Issue Reporting.

Invarian yang tidak boleh diubah:

- Role formal tetap `User`, `Administrator`, dan `Auditor`. “Superadmin” adalah label UI untuk profil permission Support, bukan role keempat atau akses tanpa batas.
- Support Console menggunakan permission per tindakan dan exact issue scope. Issue ID, filename, Job ID, Correlation ID, Attachment ID, atau Export ID tidak memperluas akses.
- Administrator, Support, Auditor, monitoring operator, atau investigator tidak otomatis memperoleh document-content access.
- Auditor adalah controlled read-only. Export memerlukan permission terpisah.
- Observability, Enterprise Audit, User History, dan Support adalah data plane berbeda.
- Alert, log, Audit, export, internal note, dan evidence tidak boleh menjadi saluran password, token, secret, document/OCR content, signature, clipboard, atau raw diagnostic.
- Dokumen ini hanya menjelaskan kapan eskalasi diperlukan. Severity, incident lifecycle, containment, communication, evidence preservation, recovery, dan post-incident review berada di Document 16.
- Semua fitur administratif masih mengikuti status pra-implementasi Document 14 sampai implementation, release exposure, dan UAT execution selesai. Label, menu, screenshot, dan availability harus diverifikasi terhadap release candidate.

# 1. Support Console overview

Support Console dipakai untuk menangani laporan yang dikirim melalui PERTAMINA OTO. Support bekerja dalam queue/unit/region/assignment yang diizinkan dan hanya saat online.

## Permission minimum per tindakan

| Tindakan | Permission | Batas |
|---|---|---|
| Membuka daftar | `support.issue.list` | Safe list projection dalam approved scope |
| Membuka detail/diagnostics | `support.issue.read` | Exact scoped Issue; Issue content only |
| Membuka screenshot/lampiran | `support.attachment.read` | Exact child attachment yang sengaja dikirim user |
| Triage/status/category/priority | `support.issue.triage` | Typed allowed transition + current state/version |
| Membaca/menambah internal note | `support.note.read`, `support.note.create` | Exact Issue; restricted note only |
| Membuat/mengambil export | `support.export.create`, `support.export.read` | Bounded authorized scope; current authorization dan expiry |

Satu permission halaman yang luas tidak cukup. Setiap action harus memeriksa permission, scope, parent-child relation, current assignment, state/version, dan reauthentication bila policy mewajibkan.

## Membuka Support Console

1. Login menggunakan account pribadi yang berwenang.
2. Pilih **Support** atau **Issue Console** pada navigasi yang tersedia.
3. Pastikan environment dan queue/scope yang tampil benar.
4. Jika menu atau action tidak tersedia, jangan menggunakan direct link atau account lain untuk melewatinya.

> **Lokasi, label, dan screenshot final: PERLU VALIDASI UI pada release candidate.**

# 2. Issue list/detail

## 2.1 Membuka dan mencari Issue

1. Buka **Issue Console**.
2. Periksa environment, queue/unit/assignment, dan rentang tanggal.
3. Gunakan pencarian yang disediakan untuk Issue ID, safe Error Code, atau safe reference yang memang diizinkan UI.
4. Gunakan **Filter Status**, **Filter Priority**, dan **Filter Category** dari nilai yang tersedia pada dropdown resmi.
5. Buka exact Issue dari hasil yang berwenang.

Aturan pencarian:

- gunakan filter server-side dan scope saat ini;
- default list menggunakan cursor; jangan memuat seluruh Issue ke spreadsheet untuk mencari manual;
- jangan mencari document content, full OCR text, screenshot content, token, atau secret;
- “tidak ada hasil” tidak membuktikan bahwa Issue tidak ada di scope lain;
- nilai category dan priority **tidak dibuat oleh manual ini**; gunakan katalog typed/configured pada release.

## 2.2 Informasi detail yang dapat terlihat

Detail dapat mencakup, sesuai permission:

- Issue ID;
- pesan yang dikirim user;
- reporter reference yang aman;
- waktu dan environment;
- app version dan platform;
- Feature ID;
- safe Error Code;
- Job ID;
- Correlation ID;
- current status/category/priority;
- safe diagnostics;
- metadata screenshot/lampiran yang dikirim eksplisit;
- current assignment/version;
- internal notes yang diizinkan.

Feature ID, Job ID, dan Correlation ID membantu korelasi, tetapi tidak mengotorisasi pembukaan document, Job result, secured file, Audit event, atau storage object lain.

## 2.3 Melihat screenshot

1. Pastikan Issue berada dalam scope Anda.
2. Pastikan attachment merupakan exact child dari Issue.
3. Buka hanya bila screenshot diperlukan untuk triage.
4. Perlakukan screenshot sebagai restricted data.
5. Jangan menyalinnya ke Audit, monitoring, email, chat, note, atau evidence lain tanpa kebutuhan dan permission resmi.

Support hanya dapat melihat screenshot/lampiran yang sengaja dikirim reporter. Screenshot yang dibatalkan, belum dikirim, atau milik Issue lain tidak tersedia.

# 3. Triage workflow

Gunakan alur berikut tanpa mengubah state di luar transition yang disediakan sistem:

```text
Buka scoped Issue
→ periksa current version/status
→ baca pesan + safe diagnostics
→ buka screenshot hanya bila perlu dan berizin
→ cari duplicate yang berwenang
→ tetapkan category/priority dari katalog resmi
→ ubah status melalui typed transition
→ tambahkan internal note singkat bila perlu
→ minta informasi tambahan / tandai duplicate / bukan bug bila tepat
→ arahkan untuk perbaikan dan test bila diperlukan
→ verifikasi outcome
→ SELESAI
```

## Aturan triage

- Jangan mengubah Issue hanya dari list tanpa membuka current detail/version.
- Jangan menebak category/priority atau membuat nilai bebas.
- Jika update ditolak karena state/version berubah, refresh lalu nilai kembali; jangan overwrite perubahan agent lain.
- Duplicate harus menunjuk ke Issue canonical yang berwenang, bukan sekadar memiliki pesan mirip.
- **BUKAN_BUG** digunakan hanya setelah perilaku dibandingkan dengan requirement/manual/release behavior yang berlaku.
- **BUTUH_INFORMASI** hanya meminta informasi yang diperlukan; jangan meminta password, token, full document, full OCR, signature, atau unrelated screenshot.
- Support tidak mengubah code, production data, policy, permission, atau server configuration dari Issue Console.
- Jika gejala mengarah ke keamanan, integritas, kebocoran, atau outage material, hentikan triage biasa dan eskalasi sesuai Bagian 16.

# 4. Status/category/priority

## 4.1 Status internal canonical terbaru

Gunakan hanya status berikut:

| Status | Makna operasional yang disarankan |
|---|---|
| `BARU` | Laporan diterima dan belum selesai ditriage |
| `DIPERIKSA` | Support sedang memeriksa laporan dan bukti aman |
| `PERLU_PERBAIKAN` | Hasil triage menunjukkan perubahan/perbaikan diperlukan |
| `BUTUH_INFORMASI` | Informasi tambahan yang aman dan relevan diperlukan |
| `DUPLIKAT` | Issue telah ditautkan ke Issue canonical yang berwenang |
| `BUKAN_BUG` | Perilaku sesuai desain/requirement atau bukan defect aplikasi setelah pemeriksaan |
| `SEDANG_DIPERBAIKI` | Perbaikan sedang dikerjakan melalui workflow resmi |
| `READY_FOR_TEST` | Artefak perbaikan siap diuji |
| `TERVERIFIKASI` | Hasil perbaikan telah diverifikasi sesuai evidence yang berlaku |
| `SELESAI` | Siklus Support pada Issue telah selesai |

Prompt meminta “resolved/closed”. Baseline tidak memiliki enum `RESOLVED` atau `CLOSED`; gunakan `SELESAI`. Jangan membuat status baru. Exact transition graph, reopen rule, dan kondisi wajib setiap status harus divalidasi pada contract/UI release.

User hanya melihat proyeksi sederhana: **Dikirim**, **Sedang diperiksa**, **Sedang ditindaklanjuti**, atau **Selesai**. Jangan memaksa category, priority, note, assignment, atau state internal ke form pengguna.

## 4.2 Category dan priority

- Pilih hanya category/priority typed yang tersedia pada release.
- Gunakan definisi dan contoh pada katalog operasional yang disetujui.
- Jangan membuat category/priority melalui free text.
- Perubahan category dicatat sebagai triage event; perubahan priority mempunyai Audit event khusus.
- Bila nilai yang tepat tidak tersedia, jangan memilih nilai yang menyesatkan; dokumentasikan gap dan eskalasi ke pemilik taxonomy.

> **Katalog category, priority, transition, dan reopen rule: REQUIRES_PERTAMINA_STANDARD_CONFIRMATION.**

# 5. Internal notes

Internal note adalah catatan terbatas yang terikat pada exact Issue. Note bukan chat umum, log dump, document repository, atau Incident Record.

## Tulis note yang aman

Gunakan pola:

```text
Waktu/observasi singkat
Safe reference: Issue/Feature/Job/Correlation/Error Code
Pemeriksaan yang dilakukan
Hasil aman
Tindakan lanjutan/owner reference yang disetujui
```

Contoh aman:

> “2026-09-01 10:15 WIB — Error Code `RESULT_VALIDATION_FAILED` pada `FEAT-CONV-001`; Job/Correlation telah dicocokkan. Reproduksi pada release candidate masih menunggu evidence QA.”

Jangan menulis:

- password, token, cookie, secret, key, connection string;
- document content, full OCR/extracted text, signature image/template, clipboard;
- raw stack, SQL, request/response body, storage path/key, raw provider response;
- screenshot bytes atau salinan attachment;
- opini personal, tuduhan, atau data individu yang tidak relevan;
- detail containment/forensik yang menjadi Incident Record Document 16.

Note penting seperti hasil triage, keputusan bukan bug/duplicate, kebutuhan informasi, readiness test, dan verification harus singkat, faktual, dan traceable. Audit hanya menyimpan evidence bahwa note ditambahkan, bukan isi note.

# 6. Issue export

## 6.1 Export PDF satu Issue

1. Buka exact Issue yang berwenang.
2. Pilih **Export PDF** bila `support.export.create` tersedia.
3. Periksa scope dan field/lampiran yang akan disertakan.
4. Selesaikan reauthentication bila diminta.
5. Tunggu background export selesai.
6. Ambil export sebelum expiry dengan `support.export.read` dan authorization saat ini.

## 6.2 Export Excel rekap

1. Tetapkan query/filter Issue yang bounded.
2. Pastikan environment, date range, status/category/priority, dan scope benar.
3. Pilih **Export Excel**.
4. Periksa apakah export lengkap, dibatasi, atau gagal; truncation tidak boleh dianggap lengkap.
5. Simpan pada lokasi yang disetujui dan hapus sesuai retention policy.

## Batas export

- Export hanya memuat authorized fields dan exact attachment bila memang diizinkan.
- Export tidak memperluas scope dari query/detail.
- Retrieval diotorisasi ulang; memiliki Export ID bukan izin.
- Artifact bersifat privat, memiliki expiry, dan tidak memakai public durable link.
- PDF/Excel harus aman dari formula, macro, control character, dan external link abuse.
- Jangan mengirim export melalui channel yang belum disetujui.
- Issue export, completion, download, expiry, dan access denial dicatat sesuai Audit Catalog.

# 7. Support privacy boundary

## Support dapat melihat

- informasi Issue yang diizinkan dalam exact scope;
- pesan yang dikirim reporter;
- exact screenshot/lampiran yang user kirim eksplisit;
- safe diagnostics;
- Issue-related Feature ID, safe Error Code, Job ID, Correlation ID, app version, platform, environment, dan status;
- internal note dan export sesuai permission terpisah.

## Support tidak dapat melihat secara otomatis

- password/passphrase;
- token, cookie, secret, key, atau credential;
- clipboard;
- visual signature/paraf/stamp payload atau template;
- full OCR/extracted/translated/Markdown text;
- source document, full unrelated document, Job input/result, atau secured-file content;
- Assistant conversation/prompt/provider request-response;
- raw path/storage key/log/stack/SQL/request body;
- unrelated Issue atau unrelated Enterprise Audit.

Support tidak memiliki “view as user”, unrestricted storage browsing, atau implicit break-glass content access. Jika content access luar biasa kelak diperlukan, itu memerlukan keputusan produk/security/legal terpisah; manual ini tidak membuat workflow tersebut.

# 8. Audit Console

Audit Console menyediakan controlled evidence untuk principal yang memiliki permission Audit eksplisit. Auditor adalah read-only; Administrator dan Support tidak mendapat akses Audit umum secara otomatis.

## Membuka Audit Console

1. Login dengan account pribadi yang memiliki `audit.event.list`.
2. Pilih **Audit** pada navigasi yang tersedia.
3. Periksa environment, time dimension, date range, dan scope.
4. Buka event detail hanya bila `audit.event.read` tersedia dan kebutuhan kerja jelas.

> **Lokasi, label, default filter, dan screenshot final: PERLU VALIDASI UI.**

## Batas data

List hanya menampilkan safe/masked projection. Detail tetap permission-controlled. Event ID, resource ID, Job ID, Correlation ID, menu visibility, access sebelumnya, atau masked value tidak memberikan content access. Audit Console tidak menyediakan document/Issue/Assistant full-text search.

# 9. Auditor workflow

```text
Tentukan tujuan pemeriksaan
→ pilih environment dan date range terbatas
→ pilih Actor/Event Category/Event Type/Feature/Outcome
→ tambahkan Resource atau Job/Correlation ID bila diperlukan
→ jalankan query dalam AUDIT_SCOPE
→ buka detail aman yang relevan
→ export hanya bila permission dan kebutuhan ada
→ catat reference, bukan payload sensitif
```

Auditor dapat:

- list/search/filter event pada `AUDIT_SCOPE`;
- membaca safe detail;
- membuat/mengambil export terbatas bila `audit.export.create/read` diberikan;
- memakai exact safe identifiers untuk korelasi.

Auditor tidak dapat:

- edit/delete source event;
- mengubah retention, legal hold, policy, configuration, role, permission, Issue, Job, atau user data;
- membuka document/result/secured file/Issue/Assistant content melalui Audit reference;
- memperluas event/field/time/unit/environment scope melalui export;
- menjalankan Audit action saat offline.

# 10. Audit search/export

## 10.1 Search dan filter

| Filter | Cara aman |
|---|---|
| Date range | Gunakan rentang UTC `[from,to)` yang bounded; pilih Occurred atau Received sesuai tujuan |
| Actor | Pilih exact authorized principal/directory reference; jangan melakukan broad email/name scan |
| EventType | Pilih nilai Event Catalog yang terlihat |
| Event Category | Gunakan kategori katalog yang disetujui |
| Feature ID | Gunakan Feature ID canonical tepat |
| Job ID | Masukkan exact Job ID; tidak memberi Job content access |
| Correlation ID | Gunakan exact value untuk mengikuti satu aktivitas; bukan kunci deduplikasi/permission |
| Outcome | Pilih controlled outcome code |
| Resource | Gunakan `ResourceType + opaque ResourceId` setelah authorization |
| Platform/Source | Pilih kode yang tersedia |
| Environment | Pilih environment yang diizinkan; jangan menggabungkan environment tanpa tujuan |

Tidak ada raw SQL, regex, arbitrary JSON path, unrestricted free-text, atau pencarian filename/document/OCR/Issue/prompt/provider content. Scope diterapkan sebelum count/query/filter/cursor. Query luas/lama harus dipersempit atau dijalankan sebagai export asinkron terbatas.

## 10.2 Export Audit

1. Jalankan query yang authorized dan bounded.
2. Pilih **Buat Export** hanya bila memiliki `audit.export.create`.
3. Periksa date/category/organization/environment scope dan field projection.
4. Selesaikan reauthentication bila diminta.
5. Tunggu export selesai.
6. Ambil artifact sebelum expiry dengan `audit.export.read` dan authorization saat ini.
7. Simpan, bagikan, dan hapus sesuai classification/retention policy.

Format baseline adalah **UTF-8 CSV** untuk baris terperinci. Fixed PDF bersifat opsional untuk template ringkasan yang disetujui. XLSX bukan baseline Audit dan memerlukan kebutuhan bisnis eksplisit. Raw JSON/database dump tidak tersedia bagi human user.

## 10.3 Audit-of-audit

Aktivitas Audit yang dapat menghasilkan evidence:

- sensitive query session (`AuditAccess.QueryExecuted`);
- privileged detail view bila policy mewajibkan (`AuditAccess.EventViewed`);
- export requested/completed/downloaded/expired;
- permission, retention, hold, archive, atau purge action oleh workload/role berwenang;
- access denial.

Sistem mencatat satu query session yang bermakna, bukan setiap cursor page. Evidence menyimpan normalized safe scope/filter fingerprint, bukan raw query, SQL, atau result rows.

# 11. Monitoring overview

Monitoring dipakai untuk melihat kesehatan teknis dan mendeteksi anomali. Monitoring bukan Enterprise Audit, User History, Support content, atau izin untuk membuka dokumen.

## Prinsip

- Gunakan dashboard/alert hanya sesuai permission dan environment.
- Mulai dari aggregate status, rate, duration, queue, dan safe reason code.
- Gunakan Feature ID, Job ID, Correlation ID, app/component version, dan provider profile code hanya bila diperlukan.
- Jangan menempelkan Issue message, screenshot, internal note, document/OCR/AI content, credential, raw error, atau storage path ke dashboard/alert.
- Green dashboard tidak mengalahkan failed mandatory Audit/security signal.
- `TIDAK DIKETAHUI` tidak boleh dianggap sehat.
- Threshold, window, deduplication, owner, on-call route, dan severity mapping berasal dari approved policy, bukan manual ini.

Observability minimum mencakup availability, API latency/error, Job duration, CPU/RAM/storage, database response/pool, transfer failure, Worker health, queue, SignalR, Audit pipeline, Issue service, provider/internal AI, deployed version, dan compatibility.

# 12. System-health status

## 12.1 Indikator operasional

| Indikator | Arti | Pedoman |
|---|---|---|
| **SEHAT** | Sinyal segar dan komponen bekerja dalam batas yang telah disetujui | Tetap monitor; tidak ada action korektif |
| **PERINGATAN** | Ada deviasi awal atau risiko meningkat, tetapi fungsi utama masih berjalan | Periksa trend/dependency; siapkan eskalasi bila memburuk |
| **TERBATAS** | Layanan degraded; sebagian capability, throughput, atau jalur pemulihan terganggu namun state masih aman | Gunakan guidance yang disetujui; hindari retry manual massal |
| **GAGAL** | Komponen tidak ready atau jalur wajib tidak dapat maju dengan aman | Hentikan action yang bergantung pada komponen; eskalasi |
| **TIDAK DIKETAHUI** | Telemetry hilang, stale, tidak lengkap, atau tidak dapat dipercaya | Jangan simpulkan sehat; validasi monitoring lalu eskalasi bila berlanjut |

Indikator tersebut adalah proyeksi dashboard. Ia tidak mengganti contract state seperti Audit `HEALTHY/DEGRADED/UNHEALTHY` atau API compatibility status.

## 12.2 Komponen dan pemeriksaan

| Komponen | Sinyal aman minimum | Contoh state terbatas/gagal |
|---|---|---|
| API | live/ready, latency, status/error rate, version | Ready `503`, error spike, latency sustained |
| Web | availability, response/error, auth flow dependency, deployed version | Page unavailable, callback/session dependency error |
| Worker | availability, job duration, crash/recycle, lease/attempt, resource use | No eligible Worker, repeated crash, stale completion |
| Database | readiness, response, connection pool wait/timeout, locks/deadlocks, log/storage | Not ready, pool exhaustion, persistent deadlock/storage issue |
| Secure storage | read/write readiness, transfer/digest failure, capacity, cleanup/expiry | Upload/finalization unavailable, pressure, digest mismatch |
| Queue | depth, oldest age, arrival/service/drain, retry, poison/version block, fairness | Backlog grows/no drain, oldest age rises, poison loop |
| SignalR | connections, publish/delivery, buffer, reconnect/gap, REST reconciliation | Reconnect storm, slow consumer, gap without recovery |
| Audit pipeline | intent/outbox, dispatcher, ingestion, query/export, backlog, integrity/hold | Mandatory intent not durable, protected queue unreadable/full |
| Issue service | submit/read/triage/attachment/export availability, latency, backlog/failure | User cannot submit, attachment errors, export backlog |
| Provider | approved profile health, timeout, quota/concurrency, circuit, output validation | Timeout/busy/circuit open, invalid output |
| AI/internal service | approved route, policy denial, capacity, invocation outcome | Internal service unavailable; public route remains blocked |
| Version compatibility | current/minimum/client version, `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, `UNSUPPORTED` | Widespread required update, unsupported client population |

API health hanya mengungkap minimal process-alive dan ready/not-ready. Production detail terbatas pada operations boundary; jangan menampilkan connection string, topology, dependency credential, storage path, atau raw queue depth kepada client/user biasa.

# 13. Alert guidance

Ambang tepat tidak di-hardcode. Setiap alert harus deduplicated, environment-aware, memiliki owner, dan tidak memperluas content access.

| Alert | Impact | Safe checks | Immediate safe action | Evidence | Escalation trigger |
|---|---|---|---|---|---|
| Repeated login failures | Account abuse, credential issue, atau IdP/config problem | Environment, time window, safe actor/session refs, source aggregate, outcome, app version | Pertahankan rate/session controls; jangan reset/unlock/grant access tanpa workflow resmi | Timestamp, environment, safe auth reason code, aggregate count, Correlation ID | Sustained/multi-account pattern, success setelah anomaly, privileged target, atau business access widespread gagal |
| Access-denied spike | Probe, stale permission/policy, deployment defect, atau legitimate bulk denial | Affected endpoint/Feature, safe actor/resource refs, policy/version change, deployment time | Jangan melonggarkan permission; validasi scope/config dan stop retry massal | Time, environment, Feature, safe denial code, count/rate, Correlation sample | Cross-user/privileged pattern, policy bypass suspicion, sharp unexplained rise, atau affected critical workflow |
| Malicious-file block spike | Attack campaign, bad upstream source, scanner/parser regression | File type/signature category, safe source aggregate, engine/scanner version, error code, crash/resource trend | Pertahankan block/quarantine; jangan membuka atau memindahkan samples manual | Timestamp, environment, Feature, safe file-rejection code, aggregate count, component version | Multiple users/sources, parser crash, containment pressure, or previously accepted format suddenly blocked |
| Queue overload | Job delay, starvation, resource exhaustion | Depth/oldest age, arrival/service/drain, workload category, Worker/DB/provider health | Biarkan admission/backpressure bekerja; hentikan manual retry storm; gunakan approved throttle if authorized | Queue state/age, environment, workload, Job/Correlation samples, safe status | Queue terus tumbuh/tidak drain, protected queue full, critical jobs starved, or data integrity uncertain |
| Storage pressure | Upload/result/export failure atau cleanup risk | Aggregate capacity, growth, cleanup/retention health, orphan/tombstone trend | Gunakan approved admission/cleanup controls; jangan menghapus file/database secara manual | Environment, storage class, safe capacity status, cleanup outcome, related Job IDs | Threshold breach, cleanup failure, mandatory path unavailable, or source/result integrity risk |
| Audit-write failure | Mandatory evidence tidak durabel; privileged mutation dapat fail closed | Intent/outbox/SQL readiness, safe reason, backlog/oldest age, version/integrity status | Jangan bypass/disable Audit; hentikan privileged action yang gagal aman | Timestamp, environment, Audit component/status, safe error, Correlation ID | **Segera** jika mandatory intent tidak durabel, evidence integrity/hold terancam, queue unreadable/full, or loss suspected |
| Worker unavailable | Job tidak dapat mulai/lanjut atau antrean tumbuh | Worker version/health, lease, crash/recycle trend, queue, resource pressure | Pertahankan queue/backpressure; gunakan approved restart/recycle runbook hanya bila berwenang | Environment, Worker profile/version, health, queue state, affected Job IDs | No compatible Worker, repeated crash, stale completion, or recovery fails |
| Provider timeout | OCR/conversion/AI dependency tertunda/gagal | Approved provider profile, timeout/rate/quota/circuit, affected Feature, network/Worker health | Biarkan bounded retry/circuit; jangan pindah ke public/unapproved provider | Provider reference, Feature, duration, safe outcome, Job/Correlation ID | Sustained outage, multiple Features affected, invalid output, data-governance concern, or no safe fallback |
| Database not ready | API/job/Audit/Issue operations gagal atau fail closed | Ready state, latency, pool wait/timeout, lock/deadlock/storage/log signals | Jangan menjalankan direct SQL; pertahankan service unavailable/fail-closed behavior | Environment, DB readiness, safe error, aggregate pool/latency, Correlation ID | Mandatory Audit/privileged path affected, persistent outage, restore/integrity concern, or pool exhaustion not recovering |
| Unsupported client spike | Banyak user tidak dapat menjalankan flow yang compatible | Platform/channel, current/minimum version, four compatibility statuses, deployment/config revision | Arahkan ke update resmi; jangan menurunkan minimum version tanpa approval | Version/platform aggregate, environment, compatibility status, app version | Widespread `UPDATE_REQUIRED/UNSUPPORTED`, wrong configuration, security-forced update failure, or rollout mismatch |
| Unusual export activity | Excessive disclosure, account compromise, atau misuse | Actor/session, export permission/reauth, normalized scope/filter, count/byte/rate, environment | Jangan membuka isi; cancel/block hanya melalui approved control dan authority; revalidate permission | Timestamp, actor safe ref, export ID, scope fingerprint, size/status, Correlation ID | Unknown actor, cross-scope attempt, repeated broad exports, high classification, download anomaly, or reauth mismatch |
| SignalR/reconnect spike | Delayed progress, reconnect storm, excess load | Connections, reconnect/gap, buffer/slow consumer, REST reconciliation, API health | Biarkan bounded reconnect; jangan mengulang execute command; gunakan approved temporary degradation | Environment, connection/reconnect aggregate, safe reason, affected Job samples | REST reconciliation gagal, cross-user event suspicion, sustained storm, or server resource starvation |
| Issue-service failure spike | User tidak dapat melapor atau Support tidak dapat triage | Submit/read/attachment/triage/export status, storage/API/DB dependency, backlog | Pertahankan message saat attachment gagal; jangan meminta user mengirim lewat contact tidak resmi | Environment, safe Issue/Error/Job/Correlation refs, outcome aggregate | Issue creation unavailable, message loss suspected, cross-scope exposure, or backlog not recovering |
| Public/unapproved AI route block | Misconfiguration atau attempted prohibited route | Feature, approved provider registry/version, policy-decision code, environment | Pertahankan block; jangan mengaktifkan public fallback | Timestamp, Feature, safe provider profile/code, policy outcome, Correlation ID | Any actual public transfer suspected, repeated config attempt, or approved route cannot be established safely |

Alert bukan Incident Record lengkap. Setelah escalation trigger tercapai, gunakan Document 16 dan jalur resmi yang telah ditetapkan organisasi.

# 14. Safe evidence

## Boleh dikumpulkan bila relevan dan berizin

- timestamp/time window;
- environment, application/component, dan app/deployed version;
- Feature ID;
- Job ID;
- Correlation ID;
- Issue/Event/Export ID opaque;
- safe Error/Reason/Event Code;
- status/outcome;
- duration dan bounded aggregate resource counters;
- provider profile/reference yang aman;
- queue state, oldest age, count/rate/byte aggregate;
- health status;
- normalized filter/scope fingerprint;
- screenshot yang user kirim dan investigator berwenang, bila benar-benar diperlukan.

## Jangan dikumpulkan pada note, alert, log, Audit, atau ordinary evidence

- password/passphrase;
- access/refresh token, cookie, authorization header;
- secret, private/encryption key, credential, connection string;
- full document/file, page content, preview, thumbnail, filename/path/storage key;
- full/sensitive OCR, extracted, redacted, translated, Markdown, atau generated text;
- clipboard;
- visual signature/paraf/stamp image/template;
- Assistant prompt/chat atau provider request-response/content;
- Issue message/internal-note/screenshot bytes di luar protected Support plane;
- raw exception, stack, SQL/query, request/response body, claim dump, packet, atau exploit payload.

Evidence minimum mencatat collector, time, source, environment, safe identifier, purpose, access, dan disposition sesuai policy. Jika evidence membutuhkan restricted data, gunakan workflow Document 16/official policy; jangan menyalinnya ke catatan biasa.

# 15. Operational security

1. **Gunakan account pribadi.** Jangan memakai generic/shared Support atau Auditor session.
2. **Least privilege.** Minta hanya permission dan scope yang diperlukan; jangan mengumpulkan role tambahan untuk kenyamanan.
3. **Jangan berbagi session.** Jangan meminjamkan browser, token, device session, screenshot login, atau hasil export kepada orang lain.
4. **Reauthentication.** Selesaikan sendiri ketika diminta untuk export/privileged/high-impact action. Reauthentication tidak mengganti permission.
5. **Periksa target.** Pastikan environment, Issue, actor, date range, scope, field, dan attachment sebelum view/update/export.
6. **Tangani export sebagai restricted.** Simpan pada lokasi resmi, jangan gunakan public link/email pribadi, batasi penerima, dan hapus sesuai expiry/retention.
7. **Tulis note aman.** Gunakan safe ID/code dan fakta; jangan paste content/log/raw error.
8. **Review screenshot.** Buka hanya exact submitted attachment; jangan salin ke plane lain tanpa dasar.
9. **Suspicious activity.** Jangan mencoba membuktikan sendiri dengan exploit atau membuka content; kumpulkan safe reference dan eskalasi.
10. **Logout.** Logout setelah selesai, terutama pada perangkat bersama. Tutup export dan tab restricted.
11. **Session expiry.** Login/reauth kembali; jangan mencoba mempertahankan stale session atau mengulang mutasi tanpa refresh.
12. **Restricted data.** Masking bukan pengganti permission. Identifier lengkap, message, note, screenshot, dan export tetap dibatasi.
13. **No direct access.** Jangan menjalankan direct SQL, storage browsing, Worker command, provider call, atau manual server change untuk triage.
14. **No control weakening.** Jangan mematikan Audit, alert, rate limit, validation, quarantine, public-AI block, atau hard resource ceiling.

# 16. Escalation boundary

Dokumen ini menjawab **kapan harus eskalasi**, bukan cara menangani incident.

## Eskalasi segera ketika

- ada indikasi cross-user/cross-scope access atau unauthorized privileged action;
- password, token, secret, document/OCR content, signature, atau restricted screenshot muncul pada log/Audit/alert/export yang tidak seharusnya;
- public/unapproved AI transfer diduga berhasil atau control route tidak dapat dibuktikan;
- mandatory Audit intent gagal menjadi durabel, evidence integrity/legal hold terancam, atau protected queue tidak dapat dibaca;
- source/result corruption, silent data loss, false success, duplicate destructive effect, atau unsafe restore terindikasi;
- repeated malicious-file block disertai parser/Worker crash atau resource starvation;
- database, secure storage, queue, Worker, API, Issue service, atau provider outage tidak pulih melalui mekanisme resmi;
- unusual broad/high-classification export atau actor/session yang tidak dikenal terdeteksi;
- permission/config/minimum-version berubah tanpa authority yang dapat diverifikasi;
- monitoring menjadi `TIDAK DIKETAHUI` untuk jalur kritis dan tidak segera pulih;
- dampak meluas lintas pengguna/environment atau detail aman yang tersedia tidak cukup untuk triage biasa.

## Handoff minimum ke jalur eskalasi resmi

- timestamp/time window;
- environment dan component/version;
- Issue/Alert/Event/Export safe reference;
- Feature ID, Job ID, Correlation ID;
- safe Error/Reason Code dan status;
- ringkasan impact tanpa content;
- tindakan aman yang sudah dilakukan;
- apakah evidence/screenshot restricted tersedia pada protected plane;
- nama owner/route hanya bila telah ditetapkan resmi.

Jangan membuat kontak, email, nomor telepon, on-call route, severity, atau authority baru. Nilai tersebut berasal dari Document 16 dan keputusan Pertamina.

Document 16 menjelaskan incident severity, lifecycle, containment, communication, evidence preservation/chain-of-custody, resolution/recovery, dan post-incident review. Jangan menduplikasi atau mengimprovisasi langkah tersebut di Issue/Internal Note.

# 17. Audit expectations

| Tindakan | Expected Audit evidence | Isi yang dilarang |
|---|---|---|
| Membuka Issue/detail | `Support.IssueViewed` bila berlaku | Issue message/screenshot bytes pada Audit |
| Membuka attachment | `Support.AttachmentViewed` | Attachment bytes/markup/name/object ref |
| Triage/category/duplicate/not-bug/need-info | `Support.IssueTriaged` dengan safe action/category/status code | Free-text triage payload |
| Mengubah priority | `Support.IssuePriorityChanged` | Alasan bebas/sensitive content |
| Mengubah status | `Support.IssueStatusChanged` | Issue content/internal discussion |
| Menambah internal note | `Support.InternalNoteAdded` | Note text |
| Membuat/menyelesaikan/mengambil Issue export | `Support.ExportRequested/Completed/Downloaded` | Export rows/attachment bytes |
| Support access denial | `Support.AccessDenied` bila security-relevant | Raw attempted payload |
| Sensitive Audit query/detail | `AuditAccess.QueryExecuted/EventViewed` sesuai policy | Raw query/result rows |
| Audit export lifecycle | `AuditAccess.ExportRequested/Completed/Downloaded/Expired` | Export data |
| Feature configuration change | `Configuration.FeatureExposureChanged` | Secret/unbounded before-after JSON |
| Permission assign/revoke | `Administration.PermissionAssigned/PermissionRevoked` | Raw claim/credential |
| Privileged access denial | `Authorization.PrivilegedActionDenied`, `AuditAccess.AccessDenied`, atau scoped event yang berlaku | Raw request/exploit detail |

Audit evidence menggunakan actor aktual, safe target opaque, action/status/category/outcome code, time, environment, Feature/Job/Correlation bila relevan, current policy/version, dan reauthentication result bila diwajibkan. Jangan mengaudit setiap page cursor atau setiap ordinary denial; simpan event yang bermakna sesuai catalog.

Administrator atau Support tidak otomatis dapat mencari Enterprise Audit untuk memeriksa event di atas. Mereka hanya melihat evidence yang diproyeksikan melalui permission Audit terpisah atau Issue-linked safe evidence yang memang diizinkan.

# 18. Screenshot requirements

## Screenshot dari user

- User memilih Screenshot, melihat preview, dan menekan Kirim secara eksplisit.
- Support hanya membuka exact submitted child attachment pada Issue berwenang.
- Screenshot tidak otomatis menjadi Audit, monitoring, alert, export, note, atau defect attachment lain.
- Screenshot target utamanya jendela PERTAMINA OTO bila platform mendukung.
- Markup sederhana tetap bagian dari protected attachment.

## Screenshot oleh Support untuk evidence

1. Ambil hanya jika evidence visual benar-benar diperlukan dan policy mengizinkan.
2. Batasi ke aplikasi/panel yang relevan.
3. Jangan sertakan content, token, email, notification, signature, raw path, atau data user lain.
4. Gunakan masking/redaction yang disetujui sebelum disimpan.
5. Simpan pada evidence location resmi dengan access/retention yang tepat.
6. Jangan menyalin screenshot Issue ke channel lain hanya untuk kemudahan.

Jika screenshot tidak dapat dibuat aman, simpan safe Issue/Job/Correlation/Error reference saja dan eskalasi. Exact screenshot control dan storage path resmi harus divalidasi pada release/policy.

# 19. Risks/conflicts

| Risiko/conflict | Dampak | Keputusan/aksi yang dibutuhkan |
|---|---|---|
| Category dan priority catalog belum terkunci pada dokumen final yang dibaca | Manual tidak dapat memberi daftar nilai | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; gunakan dropdown typed release, jangan invent |
| Status list terkunci tetapi transition graph, reopen, dan terminal rule belum dirinci | Agent dapat memilih transition yang salah | Validasi contract/UI; server hanya menerima allowed current-version transition |
| Prompt menyebut resolved/closed, tetapi status internal tidak memiliki enum tersebut | Risiko enum baru/inkonsisten | Gunakan `SELESAI`; jangan membuat `RESOLVED/CLOSED` |
| Exact Issue search fields selain contoh status/search/date belum lengkap | Manual dapat overpromise search | Publish allowlist per endpoint/UI dari release contract |
| Support queue/unit/region/assignment scope belum disahkan | Risiko overbroad access | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` dan UAT cross-scope |
| Monitoring thresholds/window/dedup/severity/owner/on-call route belum dikunci | Status/alert bisa noisy atau silent | Baseline lalu approval; jangan hardcode angka di manual |
| Dashboard/component mapping dan role visibility belum tersedia | Operator dapat melihat detail berlebih atau status tidak lengkap | Permission-aware dashboard design + privacy review + UAT |
| Health indicator UI berbeda dari contract internal | Risiko menyamakan TERBATAS dengan audit `DEGRADED` secara salah | Dokumentasikan mapping per component tanpa mengubah contract state |
| Provider/internal AI profile dan owner belum final | Alert/provider evidence tidak konsisten | Freeze approved registry/profile codes; public fallback tetap dilarang |
| Exact reauthentication triggers/freshness untuk export belum final | Export dapat blocked atau kurang terlindungi | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Issue export membutuhkan Excel, sedangkan Audit export baseline CSV | Format dapat tercampur | Pertahankan perbedaan: Support = PDF/Excel; Audit = UTF-8 CSV, optional approved PDF |
| Feature/admin/support UI masih pra-implementasi | Langkah/menu/screenshot belum dapat dipastikan | Validasi terhadap release candidate dan UAT; jangan publish placeholder |
| External contact/escalation route belum ditetapkan | Risiko kontak palsu | Document 16/Pertamina menetapkan; manual tidak membuat contact |
| Incident severity/lifecycle dapat tersalin ke Admin Manual | Duplikasi/conflict dengan Document 16 | Bagian 16 hanya trigger dan safe handoff; detail tetap Document 16 |
| Screenshot/Issue content mudah disalin ke note/Audit/alert | Privacy leakage | Protected Support plane, permission, minimization, screenshot review, canary test |
| Auditor/Admin/Support duty dapat digabung pada satu orang | SoD dan excessive privilege risk | Gunakan grant terpisah, time-bound bila perlu, review assignment dan Audit |
| `TIDAK DIKETAHUI` dapat diperlakukan sebagai `SEHAT` | Critical blind spot | Fail safe pada jalur wajib; validasi telemetry/monitoring dan eskalasi |

Tidak ada gap yang mengizinkan unlimited Support access, content browsing, direct SQL/storage, Audit disablement, secret logging, public AI fallback, atau kontak yang dibuat-buat.

# 20. Suggested Document 15 text

Berikut teks ringkas siap-integrasi yang dapat dipertimbangkan untuk Document 15.

---

## Support Console

Support/Superadmin adalah scoped permission profile, bukan role tanpa batas. Support Console memisahkan `support.issue.list`, `support.issue.read`, `support.attachment.read`, `support.issue.triage`, `support.note.read/create`, dan `support.export.create/read`. Seluruh action hanya daring, memeriksa exact queue/unit/assignment, parent-child relation, current state/version, permission, dan reauthentication bila policy mewajibkan.

Untuk menangani Issue, buka **Support/Issue Console** `[PERLU VALIDASI UI]`, periksa environment/scope/date range, lalu cari Issue menggunakan safe ID/code atau filter yang tersedia. Filter status, category, dan priority hanya memakai nilai typed pada release. Buka exact detail untuk melihat Issue message, safe diagnostics, Feature ID, safe Error Code, Job ID, Correlation ID, version/platform/environment, serta exact screenshot yang dikirim user bila permission ada. Identifier tidak memberi akses document, Job result, secured file, Audit, atau storage.

Triage menggunakan status internal berikut dan tidak membuat status baru:

```text
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
```

`SELESAI` digunakan untuk outcome resolved/closed; `RESOLVED` dan `CLOSED` bukan enum tambahan. Exact transition/reopen rules serta category/priority catalog memerlukan confirmation. User hanya melihat Dikirim, Sedang diperiksa, Sedang ditindaklanjuti, atau Selesai.

Refresh Issue bila current version berubah. Duplicate harus menunjuk Issue canonical yang berwenang. Bukan Bug memerlukan pembandingan dengan requirement/manual/release behavior. Butuh Informasi hanya meminta informasi minimum yang aman. Internal note bersifat restricted, factual, singkat, dan menggunakan safe Issue/Feature/Job/Correlation/Error reference; jangan paste password/token/secret/content/OCR/signature/clipboard/raw error/path/provider payload.

Support dapat membuat PDF satu Issue dan Excel rekap filter jika permission export tersedia. Export terikat query/scope, berjalan bounded/background, memiliki expiry, diotorisasi ulang ketika diambil, dan hanya memuat authorized fields/attachments. Artifact privat, tidak memakai public link, dan diamankan dari formula/macro/external-link abuse.

## Support privacy

Support dapat melihat Issue yang diizinkan, exact screenshot yang user kirim, safe diagnostics, dan Issue-related context. Support tidak otomatis melihat password, token, secret, clipboard, visual-signature payload, full OCR/extracted/translated text, unrelated document/Job/secured-file content, Assistant conversation/provider content, raw storage/log/stack/SQL, unrelated Issue, atau unrelated Audit. Tidak ada “view as user” atau unrestricted storage browsing.

## Audit Console

Auditor mempunyai controlled read-only access melalui `audit.event.list/read` dalam `AUDIT_SCOPE`; export memerlukan `audit.export.create/read` terpisah. Administrator/Support tidak mendapat Audit access otomatis.

Audit search memakai bounded date range, Actor, Event Category/Type, Feature ID, Outcome, ResourceType+opaque ID, Process/Job/Correlation ID, Platform/Source, dan Environment. Scope diterapkan sebelum count/query/filter/cursor. Tidak ada raw SQL, regex, unrestricted free-text, filename, document/OCR/Issue/prompt/provider-content search.

Audit export menggunakan authorized normalized query, explicit field/scope/as-of boundary, current authorization/reauth, bounded background extraction, private encrypted artifact, manifest/digest/expiry, dan audited download/cleanup. UTF-8 CSV adalah baseline detail; fixed PDF opsional jika disetujui; XLSX bukan baseline Audit. Audit query/detail/export sendiri menghasilkan audit-of-audit yang bermakna tanpa menyimpan raw query/result payload.

Auditor tidak edit/delete event, mengubah retention/hold/policy/configuration/user data, atau memakai Audit reference untuk membuka document/result/secured file/Issue/Assistant content.

## Monitoring dan system health

Monitoring menampilkan aggregate technical health dan security detection; bukan Enterprise Audit atau content-access path. Data aman meliputi timestamp, environment/component/version, Feature/Job/Correlation, safe outcome/error code, status, duration/resource counters, provider profile, dan aggregate queue/health.

Gunakan indikator:

- **SEHAT:** sinyal segar dan layanan bekerja dalam approved bounds.
- **PERINGATAN:** deviasi awal/risk meningkat, fungsi utama masih berjalan.
- **TERBATAS:** degraded; sebagian capability/throughput/recovery terganggu tetapi state aman.
- **GAGAL:** komponen tidak ready atau jalur wajib tidak dapat maju dengan aman.
- **TIDAK DIKETAHUI:** telemetry hilang/stale/tidak dapat dipercaya; tidak boleh dianggap sehat.

Pantau API, Web, Worker, database, secure storage, queue, SignalR, Audit pipeline, Issue service, approved provider, internal AI service, dan version compatibility. Sinyal minimum mencakup availability/readiness, latency/error, resource/pool/queue/oldest-age/drain, crash/recycle/reconnect, transfer/digest, Audit intent/ingestion/backlog, export, provider timeout/quota/circuit, dan `SUPPORTED/UPDATE_AVAILABLE/UPDATE_REQUIRED/UNSUPPORTED` distribution. Detail Production tetap restricted dan tidak mengungkap secret, connection string, path, credential, topology, atau content.

## Alert dan safe evidence

Alert minimum mencakup repeated login failures, access-denied spike, malicious-file block spike, queue overload, storage pressure, Audit-write failure, Worker unavailable, provider timeout, database not ready, unsupported-client spike, unusual export activity, SignalR/reconnect spike, Issue-service failure, dan public/unapproved AI route block. Setiap alert memuat Impact, Safe Checks, Immediate Safe Action, Evidence, dan Escalation Trigger; threshold/owner/route/severity ditentukan policy Pertamina.

Safe evidence dapat memuat timestamp, environment/component/version, Feature ID, Job ID, Correlation ID, opaque Issue/Event/Export ID, safe Error/Reason/Event Code, status/outcome, provider reference, queue/health aggregate, normalized scope fingerprint, dan approved screenshot bila perlu. Jangan kumpulkan password, token/cookie, secret/key, full document, sensitive OCR, signature, clipboard, Assistant/provider content, raw stack/SQL/body/path/storage key, atau exploit payload.

## Operational security dan escalation

Gunakan account pribadi dan least privilege; jangan berbagi session. Periksa environment/scope/target sebelum action. Selesaikan reauthentication sendiri. Perlakukan export/internal note/screenshot sebagai restricted. Logout setelah selesai. Jangan melakukan direct SQL/storage browsing/Worker/provider call, melemahkan controls, atau mencoba exploit untuk membuktikan anomaly.

Eskalasi ketika ada cross-user/privileged access, forbidden-data leakage, suspected public-AI transfer, mandatory Audit-write/integrity failure, corruption/silent loss/false success, malicious-file crash/starvation, critical component outage yang tidak pulih, unusual export, unauthorized config/permission change, atau blind spot `TIDAK DIKETAHUI` pada jalur kritis. Handoff hanya membawa safe time/environment/component/version/Issue/Alert/Event/Feature/Job/Correlation/Error/status/impact references.

Document 16 menetapkan severity, incident lifecycle, containment, communication, evidence preservation, resolution/recovery, dan post-incident review. Document 15 tidak membuat kontak, on-call route, severity, atau langkah containment sendiri.

## Audit expectations dan screenshot

Issue view/attachment/triage/priority/status/note/export, Audit query/detail/export, Feature configuration, permission change, dan privileged denial menghasilkan event Catalog yang sesuai. Audit menyimpan actor, opaque target, safe action/status/category/outcome, time/environment, Feature/Job/Correlation, policy/version, dan reauth result; tidak menyimpan Issue message, internal-note text, screenshot bytes, document/OCR/signature/credential/raw diagnostic.

Screenshot user hanya tersedia setelah preview dan explicit Send. Support membuka exact submitted child attachment dalam scope. Jangan memindahkan screenshot ke Audit, monitoring, alert, note, email, chat, atau evidence lain tanpa kebutuhan, permission, dan handling resmi.

---

**Keputusan final tetap milik User dan Integrator Document 15.**
