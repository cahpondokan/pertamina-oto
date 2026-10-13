# AGENT B — Performance, Reliability & Recovery Report

**Task ID:** DOC12-AGENT-B  
**Scope:** performance, load, stress, concurrency, soak/stability, recovery, fault injection, dan validasi anggaran sumber daya untuk pemrosesan lokal serta server  
**Sumber yang diperiksa:** Document 01–11 FINAL, dengan fokus pada Document 04, 05, 06, 07, 10, 11, serta kebutuhan efisiensi server  
**Batas:** rekomendasi Test Plan; tanpa production coding, perubahan arsitektur, load-testing SaaS baru, angka SLA rekaan, atau perubahan master Document 12

Laporan ini menggunakan tiga jenis nilai yang tidak boleh dicampur:

| Jenis nilai | Arti | Cara diperoleh |
|---|---|---|
| **MEASURED_BASELINE** | Distribusi hasil aktual dari build, environment, corpus, route, concurrency, dan durasi yang disebutkan secara lengkap | Pengukuran berulang pada environment terkontrol |
| **TARGET_TO_BE_APPROVED** | Usulan ambang penerimaan yang belum menjadi komitmen Pertamina | Diajukan setelah baseline, risiko, UX, kapasitas, biaya, dan headroom ditinjau |
| **POLICY_LIMIT** | Batas keselamatan runtime yang diterapkan sistem, misalnya ukuran, kuota, concurrency, timeout, atau kapasitas antrean | Berasal dari typed policy/system safety yang disetujui; bukan target performa |

Jika target numerik belum disetujui, hasil tidak boleh dilabeli PASS hanya karena “terlihat cepat”. Hasil tetap dapat lulus/gagal terhadap invarian nonnumerik, misalnya tidak ada source corruption, duplicate logical job, silent failure, whole-file buffering, atau terminal-state regression.

## 1. Strategi performance test

### 1.1 Tujuan

Strategi harus menjawab enam pertanyaan:

1. Berapa biaya CPU, RAM, disk/temp, jaringan, SQL, koneksi, Worker, antrean, Audit, dan storage growth untuk satu workload serta campuran workload?
2. Bagaimana latency/throughput berubah saat concurrency dan volume bertambah?
3. Di titik mana bottleneck dan capacity knee muncul?
4. Apakah admission control/backpressure aktif sebelum resource masuk kondisi tidak aman?
5. Apakah crash, timeout, disconnect, restart, dan partial output pulih tanpa kehilangan sumber, duplikasi efek, atau false success?
6. Berapa capacity envelope environment kecil/pilot, dan scale trigger apa yang terukur tanpa menjadikannya desain permanen?

### 1.2 Lapisan suite

| Suite | Tujuan utama | Bukti yang dihasilkan |
|---|---|---|
| Performance | Biaya satu workflow dan jalur kritis pada beban terkendali | Latency/throughput/resource profile per Feature/workload |
| Load | Perilaku pada beban representatif hingga target kandidat | Capacity curve, queue stability, error/backpressure rate |
| Concurrency | Race dan kontensi pada resource/command yang sama atau berbeda | Uniqueness, fairness, lock/wait, state correctness |
| Stress | Perilaku melampaui target kandidat menuju batas keselamatan | Knee point, graceful degradation, safe rejection, recovery |
| Soak/Stability | Drift jangka panjang dan siklus cleanup/retry/reconnect | Leak/growth slope, orphan, pool/handle stability |
| Recovery/Resilience | Ketahanan terhadap kegagalan nyata lintas boundary | Measured recovery duration, data/effect integrity, backlog drain |
| Resource-budget validation | Kecocokan metadata Feature dengan biaya aktual | Processing route, server dependency, storage/network/concurrency class evidence |

Unit Test Document 11 tetap menjadi gate untuk business rule dan state decision. Suite ini menggunakan komponen nyata sesuai kebutuhan untuk membuktikan perilaku resource/infrastruktur yang tidak dapat dibuktikan fake.

### 1.3 Matriks eksekusi

Setiap skenario dicatat pada dimensi berikut:

| Dimensi | Nilai yang harus dinyatakan |
|---|---|
| Build | Commit/build ID, runtime/SDK/app version, configuration digest |
| Environment | Device/host profile, OS, CPU, RAM, disk/storage, network, SQL edition/configuration, topology |
| Route | LOCAL_IN_PROCESS, LOCAL_WORKER, SERVER_WORKER, atau approved internal provider |
| Feature/workload | Canonical Feature ID, execution profile, input shape, validation/security path |
| Corpus | Dataset version/digest, file count/size/structure/content class sintetis |
| Load | Arrival model, active users, request rate, concurrent jobs/streams, workload mix |
| State | Cold/warm cache, fresh/grown DB, queue state, storage free-space band |
| Duration | Warm-up, measurement window, drain/recovery window |
| Policy | Policy version, effective limits, retry/backoff, retention/cleanup settings |
| Instrumentation | Counter/trace/query collection configuration dan sampling |

Hasil yang tidak membawa manifest tersebut adalah observasi informal, bukan MEASURED_BASELINE.

### 1.4 Environment pengujian

- **Perangkat lokal minimum yang didukung:** membuktikan responsiveness, memory/temp safety, dan Worker isolation pada batas perangkat yang disetujui.
- **Perangkat lokal representatif:** mengukur pengalaman pilot yang paling umum.
- **Environment kecil/pilot sebagaimana dideploy:** Web/API/Worker/SQL/storage dapat berbagi physical server bila deployment aktual demikian; seluruh counter tetap dipisahkan per process/deployable.
- **Environment scale-reference bila tersedia:** menguji bahwa pemisahan deployable tidak mengubah business contract. Ini bukan kewajiban membeli infrastruktur baru.
- **Environment recovery terisolasi:** memiliki data sintetis, storage/temp test khusus, backup/restore test set, dan izin menghentikan process/service tanpa menyentuh Production.

Production data tidak digunakan. Corpus harus sintetis atau telah disanitasi dan disetujui.

Load generator dan collector berjalan pada host internal/self-hosted yang disetujui serta dipantau agar bukan bottleneck. Laporan ini tidak mewajibkan pembelian load-testing SaaS atau produk chaos baru; pemilihan tool konkret mengikuti dependency, security, lisensi, dan operasional governance yang sudah ada.

### 1.5 Invarian yang selalu menjadi gate

- operasi local-only tidak membuat upload atau API processing;
- file besar tidak di-buffer seluruhnya bila streaming berlaku;
- UI tetap hidup dan memberikan status; tidak ada silent wait;
- crash Local Worker tidak membunuh UI;
- persisted Job state adalah source of truth; SignalR bukan authority;
- 202 hanya setelah state durabel dan reservation berhasil;
- Job ID, Correlation ID, Idempotency Key, ownership, attempt, dan checkpoint tetap konsisten;
- source/target lama tidak rusak atau tertimpa diam-diam;
- partial/temp output tidak menjadi final;
- tidak ada duplicate logical job/upload/Audit/secured version;
- koneksi/transaksi SQL tidak ditahan selama file stream, engine/provider work, queue wait, SignalR, atau retry delay;
- mandatory Audit tidak dimatikan/disampling untuk meluluskan benchmark;
- password, token, secret, isi dokumen/OCR, signature, clipboard, prompt, atau payload Support tidak masuk telemetry/test artifact.

## 2. Metrik sumber daya

### 2.1 Katalog metrik

| Area | Metrik minimum | Dimensi/korelasi |
|---|---|---|
| CPU | Host dan process CPU utilization, saturation duration, processor queue bila tersedia, CPU time per Job/request | Deployable, Feature/workload, route, Job/Correlation |
| RAM/.NET | Working set, private bytes, committed memory, GC heap, allocation rate, LOH, Gen 0/1/2, GC pause, handle/thread count | UI, API, Worker, local Worker, job class |
| Disk | Read/write bytes dan operasi, latency, queue, throughput, free capacity | Workspace, temp, SQL data/log/tempdb, object storage mount |
| Temp storage | Peak bytes/job, concurrent peak, growth rate, oldest/orphan count/bytes, cleanup latency/throughput | Feature, attempt, lease, state |
| SQL I/O | Logical/physical reads, writes, CPU/duration per query, log bytes/flush, data/log/tempdb growth, wait/lock/deadlock | Query route, schema/module, plan, correlation-safe tag |
| DB connection | Active/idle/pooled, checkout wait, timeout, connection-open duration, pool exhaustion/recovery | Deployable dan endpoint/workload category |
| API | Request rate, concurrent requests, p50/p95/p99/max latency, timeout, status/error rate, 202 durable-accept latency | Route category, result class, build/environment |
| Network | Bytes/request/job, throughput, retry bytes, reconnect, upload/download/resume efficiency | Client-server/provider/storage boundary |
| Worker memory | Private/working set, GC/allocation, peak/job, post-job residual, recycle/crash count | Worker instance, engine/provider, attempt |
| Queue | Depth, oldest age, arrival/service/completion rate, wait duration, WAITING_RESOURCE count, retry backlog, fairness | Workload class, user/category, priority/policy |
| Job | Admission, queue wait, validation, execution, checkpoint, retry, finalization, end-to-end duration | Feature, input band, route, attempt/outcome |
| SignalR | Active connections/subscriptions, publish/delivery rate, latency, outbound buffer, disconnect/reconnect, duplicate/stale/gap, REST reconciliation | Hub/stream, job state version, client visibility |
| Audit | Intent latency, tambahan transaksi, events/action, row/bytes, ingestion latency/rate, duplicate/reject, backlog age/drain | Event class, local/server, workload |
| Storage growth | SQL data/log/index, Audit/history/job/issue metadata, upload/temp/checkpoint/result/secured object bytes, orphan/tombstone | Data class, policy version, hot/archive window |
| Cleanup/archive | Candidate/claim/delete rate, failure/retry, checkpoint age, resource usage, throughput versus growth | Lifecycle class dan foreground load |
| UI responsiveness | Input-to-response latency, main-thread stall, frame/render health bila tersedia, progress update delay, Activity Center recovery | Device, feature, local concurrency, UI state |

Metrik dilaporkan sebagai distribusi dan tren, bukan rata-rata tunggal. p50/p95/p99 digunakan bila jumlah sampel memadai; untuk sampel kecil tampilkan seluruh hasil/minimum–maksimum dan tandai ketidakpastian.

### 2.2 Pengumpulan

- Gunakan Microsoft.Extensions.Logging/OpenTelemetry dan Application Insights/APM yang telah dikunci untuk metrik aplikasi aman.
- Gunakan counter runtime/OS bawaan yang disetujui untuk process, .NET GC, CPU, RAM, disk, network, thread, dan handle.
- Gunakan SQL Server Query Store, execution plan, wait/lock/deadlock, DMV/monitoring DBA, serta data/log/tempdb counters tanpa menangkap content atau parameter sensitif.
- Gunakan Job ID dan Correlation ID untuk korelasi terkontrol; jangan menaruh filename, path, document content, Actor ID luas, atau secret pada tag bermultiplikasi tinggi.
- Catat overhead instrumentation. Konfigurasi observability test harus menyerupai target environment dan tidak diubah antar-run tanpa mencatat baseline baru.

### 2.3 Derived indicators

Hitung dan laporkan:

    service rate - arrival rate
    queue drain rate setelah burst/outage
    cleanup throughput - sustained eligible growth
    bytes jaringan per logical operation
    peak dan residual Worker memory per completed Job
    SQL logical reads dan log bytes per operation
    Audit rows/bytes dan transaction delta per action
    temp peak versus concurrent Job mix
    error/retry/duplicate rate per accepted logical operation
    recovery duration dan uncommitted/duplicate effect count

Nilai negatif yang persisten pada drain/cleanup margin berarti backlog atau storage akan tumbuh tanpa batas pada workload tersebut.

## 3. Skenario workload lokal

### 3.1 Corpus bands

Gunakan band yang diturunkan dari corpus pilot dan policy, bukan angka rekaan:

| Band | Definisi |
|---|---|
| CONTROL | File kecil/sederhana untuk mengukur overhead tetap |
| TYPICAL | Distribusi representatif hasil inventaris pilot |
| LARGE_SUPPORTED | File/struktur besar namun masih di bawah batas yang disetujui |
| POLICY_BOUNDARY | Tepat pada limit efektif jumlah/byte/page/pixel/temp |
| OVER_LIMIT | Sedikit di atas limit; harus ditolak aman sebelum resource exhaustion |
| ADVERSARIAL_VALID | Syntactically valid tetapi mahal: banyak halaman/object/font/image, high compression, atau struktur kompleks |

Semua file membawa digest agar source immutability dan output identity dapat diverifikasi.

### 3.2 Matriks lokal

| ID | Workload | Variasi | Bukti utama |
|---|---|---|---|
| PERF-LOCAL-001 | PDF besar | TYPICAL, LARGE_SUPPORTED, POLICY_BOUNDARY, OVER_LIMIT; image-heavy dan vector-heavy | Streaming/memory profile, temp peak, UI responsiveness, source digest |
| PERF-LOCAL-002 | Banyak PDF/Merge | Jumlah input typical hingga boundary; ukuran campuran; urutan tetap | Throughput, file handle, temp, no whole-set RAM amplification, output order/validity |
| PERF-LOCAL-003 | Split/extract/organize/edit | Banyak operasi working-state, preview, undo/redo, finalisasi | UI stall, autosave/checkpoint cost, no source overwrite, bounded history/state growth |
| PERF-LOCAL-004 | Gambar besar | Pixel/dimensi/compression boundary, banyak gambar, convert/resize/compose | Decode peak memory, output/temp size, cancellation, image-bomb guard |
| PERF-LOCAL-005 | OCR PDF/gambar | Scanned pages, DPI/language/layout/table complexity, local engine route | CPU/RAM/job duration, checkpoint interval/cost, cancellation, UI isolation |
| PERF-LOCAL-006 | Conversion | Office/PDF/image/HTML local-capable route sesuai PoC | Worker memory spike/leak, temp/output growth, engine timeout/failure mapping |
| PERF-LOCAL-007 | Concurrent local jobs | Campuran ringan dan berat dari 1 hingga policy max dan satu di atas | Fairness, UI responsiveness, worker pool, queue/wait, safe rejection/wait |
| PERF-LOCAL-008 | Long workflow | Multi-step cumulative temp/storage/memory | Preflight accuracy, intermediate cleanup, no partial final |
| PERF-LOCAL-009 | Local audit outbox | L2/L3 event generation sambil workload berjalan | Intent latency, UI impact, SQLite growth, queue safety |
| PERF-LOCAL-010 | Local Worker recycle | Repeated heavy jobs dan recycle threshold yang dikonfigurasi | Residual memory trend, clean restart, no job/source loss |

### 3.3 Responsiveness dan isolation

- Jalankan scripted user interactions yang representatif selama job: membuka Activity Center, memilih file, navigasi, cancel, dan membaca status.
- Ukur input latency, longest main-thread stall, render/progress delay, serta waktu UI kembali responsif setelah pressure/crash.
- Pastikan pekerjaan berat tidak berjalan pada UI thread.
- Hentikan Local Worker pada titik running, checkpoint, dan output staging. UI harus tetap hidup, menampilkan failure/recovery yang benar, dan dapat memulai ulang Worker sesuai policy.
- Periksa bahwa UI tidak membuat progress palsu; unknown total tetap indeterminate.
- Setelah cancel/crash/retry, hitung file temp/orphan, file handle, thread, memory residual, dan sumber yang berubah. Source digest harus tetap identik.

Target responsiveness numerik berstatus TARGET_TO_BE_APPROVED setelah baseline pada perangkat minimum/representatif.

## 4. Skenario workload server

### 4.1 Keluarga workload

| ID | Keluarga | Skenario utama | Metrik/guard |
|---|---|---|---|
| PERF-SRV-001 | Authentication/capability/metadata | Login-support traffic sintetis yang disetujui, compatibility, Feature discovery, preferences/config reads | API/DB latency, connection, cache behavior; jangan membebani IdP nyata tanpa izin |
| PERF-SRV-002 | Upload | Concurrent one-shot streamed multipart dan resumable sessions, retry chunk, complete/deep validation | Network, temp, RAM, digest, storage I/O, session/expiry/cleanup |
| PERF-SRV-003 | Download | Concurrent manifest/HEAD/range/full/resume untuk Job result dan secured version | Throughput, storage read, API memory, connection lifetime, authorization cost |
| PERF-SRV-004 | Server Jobs | OCR/conversion/AI-S/internal-provider workload menurut route yang disetujui | Admission, queue, CPU/RAM, Worker memory, duration, checkpoint/retry |
| PERF-SRV-005 | Metadata queries | Job/activity, history, secured files/versions, configuration | p50/p95/p99, logical reads, plan/index, pagination |
| PERF-SRV-006 | Issue | Reporter create/list/detail, explicit attachment, Support list/triage/export | SQL/object I/O, query bounds, export queue, attachment cleanup |
| PERF-SRV-007 | Audit | Intent write, batch ingestion, scoped query, cursor, export, offline backlog drain | Transaction delta, rows/bytes, query plan, backlog, export contention |
| PERF-SRV-008 | Secured files | Create/add version/list/download/restore/delete, concurrent fourth-version race | Storage/SQL transaction, max-three invariant, tombstone/reconciliation |
| PERF-SRV-009 | SignalR | Many authenticated connections, progress bursts, disconnect/reconnect/gap | Hub CPU/RAM/buffer/network, delivery latency, REST fallback |
| PERF-SRV-010 | Cleanup/lifecycle | Expired upload/temp/result/export/outbox cleanup under foreground load | Throughput, lock/I/O, throttle, no active lease deletion |
| PERF-SRV-011 | Mixed pilot | Weighted mix dari hasil inventaris pilot | Capacity envelope dan dominant bottleneck |
| PERF-SRV-012 | Local-first negative load | Jalankan local-only workflows pada client yang online | Server processing/upload count tetap nol; hanya metadata/Audit yang memang diwajibkan |

### 4.2 Workload mix

Jangan mengarang persentase traffic. Bentuk workload mix dari:

1. expected pilot population dan business workflow;
2. execution profile Feature Catalog;
3. hasil usability/pilot;
4. measured telemetry aman bila tersedia;
5. worst credible combination, misalnya upload + OCR + download + Audit query + cleanup.

Gunakan model arrival-rate untuk API/background admission dan model concurrent-session untuk perjalanan user/SignalR. Laporkan keduanya secara terpisah.

### 4.3 Small-server/pilot

Pada server awal yang menggabungkan Web, API, Worker, SQL, dan storage:

- ukur resource per process serta total host;
- identifikasi kontensi CPU, memory, disk/temp, SQL data/log/tempdb, storage, network, dan pool koneksi;
- uji background Worker, Audit ingestion, dan cleanup bersamaan dengan metadata/API foreground;
- verifikasi concurrency diturunkan dan pekerjaan background di-throttle sebelum foreground tidak aman;
- hasilkan kapasitas pilot per workload mix, bottleneck pertama, dan scale trigger terukur;
- jangan menonaktifkan Feature resmi. Jika kapasitas belum cukup, gunakan queue, backpressure, concurrency policy, jadwal/throttle, atau rekomendasi pemisahan deployable yang sudah didukung arsitektur untuk keputusan Integrator.

## 5. Pengujian concurrency

| ID | Race/kontensi | Invarian |
|---|---|---|
| CONC-001 | Banyak command dengan Idempotency Key dan fingerprint sama | Satu logical effect/Job; replay resource yang sama |
| CONC-002 | Key sama, fingerprint berbeda | 409 conflict; tidak ada overwrite/effect kedua |
| CONC-003 | Multi-worker claim pada antrean yang sama | Satu lease/attempt aktif per Job; fairness dan bounded claim |
| CONC-004 | Lease expiry bersamaan dengan Worker lama kembali | State/version/lease guard mencegah dual completion |
| CONC-005 | Cancel berbarengan dengan complete/fail/checkpoint | Terminal state valid, tidak regresi, output hanya bila tervalidasi |
| CONC-006 | Retry/resume bersamaan | Satu attempt baru yang sah; logical Job ID tetap |
| CONC-007 | Upload chunk duplicate/overlap/out-of-order | Same digest di-ack; conflict aman; offset tidak rusak |
| CONC-008 | Download range paralel dan expiry/delete | Authorization/state diperiksa ulang; tidak ada byte setelah hak/retensi berakhir |
| CONC-009 | Versi secured keempat secara paralel | Maksimum tiga active/addressable; tombstone/version tetap konsisten |
| CONC-010 | Update policy/config dengan rowversion sama | Satu commit; stale writer mendapat precondition conflict |
| CONC-011 | Issue triage/status bersamaan | Rowversion/state transition valid; tidak ada last-write-wins diam-diam |
| CONC-012 | Audit Event/Batch replay paralel | Unique ID/fingerprint menyimpan sekali; conflict terlihat |
| CONC-013 | SignalR progress duplikat/reorder/gap/terminal delay | Client idempotent, gap REST reconcile, terminal tidak regresi |
| CONC-014 | Connection-pool pressure lintas API/Worker/cleanup/export | Bounded wait/backpressure; koneksi tidak ditahan saat external work |
| CONC-015 | Local jobs bersamaan dengan autosave/checkpoint/UI | Working state konsisten, UI responsif, file/temp tidak saling silang |
| CONC-016 | Offline backlog serentak setelah reconnect | Gradual release dengan backoff/jitter; tidak ada thundering herd/duplicate |

Uji concurrency menggunakan barrier terkontrol pada titik claim, commit, publish, acknowledgement, checkpoint, finalization, dan release. Jangan hanya menjalankan banyak request tanpa memastikan race window benar-benar tercapai.

## 6. Pengujian load

### 6.1 Tahapan load

Setiap workload utama menjalani urutan:

1. **Control:** satu user/operation untuk memvalidasi corpus, instrumentation, dan correctness.
2. **Measured baseline:** load rendah yang stabil pada environment bernama.
3. **Pilot/target candidate:** load representatif yang diusulkan dari kebutuhan pilot.
4. **Headroom exploration:** load bertahap di atas target kandidat tetapi masih di bawah POLICY_LIMIT.
5. **Mixed workload:** foreground metadata/query/transfer bercampur dengan jobs, SignalR, Audit, dan cleanup.
6. **Burst and drain:** lonjakan terbatas lalu kembali ke arrival normal untuk mengukur backlog drain.
7. **Recovery observation:** hentikan generator baru; ukur resource, queue, pool, temp, dan error kembali stabil.

Setiap tahap naik hanya setelah correctness guard tahap sebelumnya lulus. Jangan melanjutkan bila muncul source corruption, duplicate effect, mandatory Audit loss, unauthorized access, storage cross-job, atau host safety risk.

### 6.2 Kondisi steady state

Measurement window dimulai setelah:

- warm-up/JIT/initial connection dan cache state yang ditentukan selesai;
- arrival/completion rate, queue depth, CPU/RAM, connection, dan storage trend mencapai pola stabil atau jelas terus memburuk;
- data preconditioning dan background cleanup berada pada state yang didokumentasikan.

Jika queue, memory, connection wait, temp, atau log terus naik, workload belum steady. Hasilnya adalah finding kapasitas/growth, bukan baseline latency yang valid.

### 6.3 Load profiles

| Profile | Tujuan |
|---|---|
| Metadata-heavy | Capability, Job/History/Secured/Issue list, status, Activity Center |
| Transfer-heavy | Upload multipart/session, chunk retry/resume, range/full download |
| Compute-heavy | OCR, conversion, AI/internal-provider, complex PDF |
| Connection-heavy | SignalR connections/subscriptions plus REST status |
| Write-heavy | Job admission/progress terminal, Audit ingestion, Issue, secured version |
| Read/export-heavy | Audit query/export, Support export, broad bounded report |
| Recovery-heavy | Backlog reconnect, lease reclaim, retry/resume, cleanup |
| Pilot mixed | Workload mix berdasarkan data/estimasi pilot yang disetujui |

### 6.4 Bukti load

Untuk setiap step load, laporkan:

- offered, accepted, completed, rejected, cancelled, failed, retry, dan duplicate logical operation;
- arrival/service/completion rate serta queue depth/oldest age;
- end-to-end dan per-stage latency distribution;
- resource distribution/peak/trend;
- status 200/201/202/4xx/5xx dengan safe reason;
- fairness per user/workload category;
- output validity, source digest, idempotency/attempt counts;
- waktu drain dan resource kembali ke steady state.

Load test dianggap tidak sah bila generator menjadi bottleneck tanpa diketahui, timestamp tidak sinkron, data/corpus berubah antarrun, atau monitoring kehilangan sampel pada periode paling berat.

## 7. Pengujian stress

Stress test dilakukan hanya di environment non-Production yang terisolasi dan dapat dipulihkan. Naikkan satu dimensi utama per eksperimen agar bottleneck dapat diatribusikan.

| ID | Tekanan | Tujuan/expected behavior |
|---|---|---|
| STRESS-001 | Request rate metadata meningkat bertahap | Temukan knee API/DB; 429/503 aktif sebelum pool/SQL collapse |
| STRESS-002 | Concurrent expensive Jobs meningkat | Weighted quota/concurrency, WAITING_RESOURCE, Worker memory/CPU tetap dibatasi |
| STRESS-003 | Upload streams/session/temp meningkat | Hard byte/count/temp limits, streamed memory, safe 413/429/503 |
| STRESS-004 | Download/range concurrency meningkat | Storage/network bottleneck terlihat; API memory tidak tumbuh proporsional ukuran file |
| STRESS-005 | SignalR connections/progress burst meningkat | Buffer/subscription limit, coalescing, reconnect/fallback; no business-state loss |
| STRESS-006 | DB connections/queries meningkat | Bounded pool wait, no long-held connection, safe backpressure |
| STRESS-007 | Audit/offline backlog burst | Ingestion batch/backoff, duplicate-safe, foreground tidak starved |
| STRESS-008 | Storage/temp mendekati safety threshold | Preflight/INSUFFICIENT_STORAGE/409/WAITING_RESOURCE sebelum disk penuh |
| STRESS-009 | Internal provider slow/busy | Timeout/circuit/capacity guard; no public fallback; local deterministic tools tetap tersedia |
| STRESS-010 | Cleanup/archive/export bersamaan dengan foreground | Background throttle; foreground fairness; cleanup tetap dapat maju |
| STRESS-011 | Idempotency/retry storm | Satu logical effect per key, lookup/index tetap bounded, no hot loop |
| STRESS-012 | Mixed host contention pada server kecil | Identifikasi bottleneck pertama dan trigger pemisahan/scale yang terukur |

### 7.1 Stress stop conditions

Hentikan peningkatan load jika:

- host/storage test mendekati batas keselamatan yang disetujui;
- data integrity, isolation, authorization, atau source safety terancam;
- health UNHEALTHY dan tidak ada safe progress;
- telemetry penting hilang sehingga outcome tidak dapat dibuktikan;
- backpressure gagal dan resource menuju exhaustion tak terkendali.

Breaking point eksperimen bukan POLICY_LIMIT Production. Policy limit harus berada pada sisi aman dari knee point dengan headroom yang disetujui.

### 7.2 Kriteria stress

Stress suite lulus secara kualitatif bila:

- overload menghasilkan wait/reject yang eksplisit dan tepat;
- already accepted work tetap dapat direkonsiliasi;
- tidak ada duplicate/corrupt/cross-user effect;
- terminal state tidak regresi;
- output parsial tidak dipublikasikan;
- queue/resource kembali terkuras setelah pressure dilepas;
- health/metrics/alerts mengungkap bottleneck tanpa payload sensitif;
- safe local capability tetap tersedia bila server/internal provider tertekan.

Ambang error, saturation, recovery, dan headroom numerik tetap TARGET_TO_BE_APPROVED.

## 8. Pengujian soak/stability

Durasi soak harus cukup untuk mencakup beberapa siklus yang relevan: Job/checkpoint/lease, connection refresh, SignalR reconnect, upload/result expiry, outbox dispatch, retry/backoff, cleanup, log rotation/retention test, Worker recycle, dan bila diaktifkan archive/purge. Durasi tepat berstatus TARGET_TO_BE_APPROVED; jangan memilih angka tanpa mengaitkannya dengan siklus yang hendak diamati.

### 8.1 Workload soak

- gunakan mixed pilot load di bawah target kandidat;
- sisipkan burst periodik dan periode tenang untuk memeriksa drain;
- jalankan local heavy jobs berulang pada perangkat minimum/representatif;
- jalankan Worker recycle/restart terjadwal sesuai policy test;
- buat upload/session/result/temp/checkpoint yang eligible untuk cleanup;
- hasilkan Audit normal dan satu backlog reconnect yang bounded;
- lakukan SignalR disconnect/reconnect periodik tanpa mengulang command.

### 8.2 Drift yang dicari

| Area | Failure signature |
|---|---|
| Memory | Working/private/GC/LOH/handle/thread residual naik per siklus setelah beban setara selesai |
| Connection | Pool checkout wait/active connection terus naik atau tidak kembali |
| Queue | Oldest age/depth meningkat walau arrival jangka panjang tidak melebihi service rate |
| Temp/object | Orphan/bytes bertambah tanpa cleanup atau lease aktif |
| SQL | Data/log/tempdb/index growth tidak sesuai event/business volume; plan memburuk |
| Worker | Peak/residual meningkat antarrecycle; crash/retry rate membesar |
| SignalR | Buffer/reconnect/polling terus tumbuh; subscription bocor |
| Audit | Outbox/backlog/duplicate/reject naik tanpa penyelesaian; foreground transaction delta memburuk |
| Logging/APM | Volume/cardinality/cost tumbuh tidak proporsional dan menyebabkan pressure |
| UI | Responsiveness menurun setelah repeated jobs/sleep/recovery |

### 8.3 Akhir soak

Setelah load dihentikan:

- seluruh antrean yang dapat diproses harus drain;
- active connections, memory, handles, temp, dan Worker count kembali ke rentang baseline yang disetujui;
- cleanup menyelesaikan eligible artifact tanpa menyentuh lease/reference aktif;
- tidak ada Job nonterminal terlantar tanpa recovery reason;
- jumlah logical input, output, attempts, Audit, tombstone, dan object reconcile;
- source digest dan secured maximum-three invariant tetap benar.

Slope pertumbuhan residual dilaporkan walaupun belum ada target; tren positif persisten tidak boleh disembunyikan oleh rata-rata.

## 9. Pengujian backpressure

### 9.1 Matriks keputusan

| Kondisi | Job/request belum diterima | Job sudah diterima durabel | Expected user/system behavior |
|---|---|---|---|
| Per-user job quota/concurrency penuh | 429 + Retry-After | Existing Job lanjut/menunggu sesuai state | Penolakan aman, tidak membuat Job ganda |
| Endpoint rate limit | 429 + Retry-After | Tidak mengubah accepted state | Retry terbatas + jitter |
| Hard request/file/chunk/batch limit | 413 | Tidak berlaku pada resource yang sudah lolos kecuali integrity conflict | Batas jelas sebelum resource exhaustion |
| Storage quota/state conflict | 409 | WAITING_RESOURCE hanya bila recovery/space dapat menjadi tersedia dan policy mengizinkan | Tidak menulis partial final |
| Shared queue/Worker saturation unsafe to queue | 503, retry guidance bila aman | Accepted Job tetap persisted; dapat WAITING_RESOURCE | Tidak memberi 202 palsu |
| Resource sementara tidak tersedia | 503 sebelum acceptance | WAITING_RESOURCE setelah acceptance | Activity Center/status menunjukkan alasan aman |
| Koneksi dibutuhkan tetapi putus | Tidak menerima server-required command atau queue intent sesuai kontrak | WAITING_CONNECTION | Local-capable work tetap berjalan |
| Provider busy/circuit open | 503 bila belum accepted dan tidak aman queue | WAITING_RESOURCE atau bounded retry pada Job accepted | Tidak fallback ke public provider |
| SQL unavailable | Safe unavailable; tidak ada required mutation tanpa durable state/Audit intent | Dispatcher/Worker tidak membuat final tanpa persistence | Fail closed pada boundary wajib |
| Temp/storage pressure lokal | INSUFFICIENT_STORAGE sebelum engine bila preflight gagal | Pause/wait/fail aman menurut capability; source tetap | Warning/actionable, no silent delete |
| Audit queue penuh/unreadable | L3/privileged action diblokir sebelum mutasi | Evidence protected/recovery visible | Tidak evict evidence; L2 mengikuti policy resmi |
| Export/cleanup concurrency penuh | 429/503 atau queue bounded | WAITING_RESOURCE/queued sesuai Job contract | Foreground tidak starved |

### 9.2 Assertion wajib

- WAITING_RESOURCE dan WAITING_CONNECTION hanya muncul pada Job durabel, bukan sebagai respons palsu tanpa resource.
- Retry-After wajib pada 429; pada temporary 503 hanya diberikan bila retry memang aman.
- Retry-After tidak memuat queue depth, topology, provider detail, atau data user.
- State Job, Activity Center, dan message code memberi feedback; tidak ada silent wait/failure.
- Reservation dibuat atomik sebelum 202 dan dilepas saat reject, cancel, terminal, expiry, atau cleanup.
- Queue bounded, fair, persistent, dan dipisah per workload category; satu user/AI/OCR/export tidak memonopoli.
- Backlog release menggunakan exponential backoff + jitter dan gradual admission.
- Backpressure aktif sebelum DB pool, Worker memory, temp/storage, CPU, atau I/O masuk exhaustion tak terkendali.
- Menurunkan concurrency tidak mengubah Feature contract atau menghapus Feature.

### 9.3 Pengukuran

Catat load saat backpressure pertama aktif, resource saat aktivasi, decision latency, response/state distribution, queue wait, fairness, rejected-versus-accepted ratio, reservation leaks, drain time, dan recovery resource. Nilai aktivasi menjadi MEASURED_BASELINE; usulan threshold/headroom menjadi TARGET_TO_BE_APPROVED; konfigurasi runtime final menjadi POLICY_LIMIT.

## 10. Matriks recovery

| ID | Kegagalan/titik injeksi | Perilaku wajib | Bukti yang diukur |
|---|---|---|---|
| REC-001 | SignalR disconnect saat Job running | Job tetap berjalan; realtime degraded; reconnect+jitter; REST reconcile; no command replay | Disconnect/reconnect/reconcile duration, duplicate/stale/gap, final-state correctness |
| REC-002 | Network disconnect saat upload | Committed chunk/offset/digest tetap; resume dari offset benar | Bytes retransmitted, resume duration, no duplicate/corrupt content |
| REC-003 | Network disconnect setelah server commit sebelum response/ack | Retry dengan Idempotency Key yang sama mengembalikan resource/Job yang sama | Duplicate effect count=0, replay latency |
| REC-004 | Network disconnect saat server-required Job | Accepted Job tetap persisted; client status pulih via REST | Queue/state continuity dan result authorization |
| REC-005 | API process restart sebelum durable acceptance | Tidak ada 202/half-resource; retry aman | Logical resources, reservation/orphan count |
| REC-006 | API restart setelah durable acceptance | Job tetap dapat ditemukan; dispatch/outbox lanjut | Recovery duration, state/revision continuity |
| REC-007 | Server Worker restart setelah claim | Lease expiry/reclaim; attempt history benar; one active executor | Reclaim duration, duplicate completion count |
| REC-008 | Local Worker crash | UI tetap hidup; Job failure/recovery candidate; source aman | UI responsiveness, Worker restart, source digest, temp/orphan |
| REC-009 | Desktop/application crash saat working/checkpoint | Startup mendeteksi state valid dan menawarkan Lanjutkan/Coba Lagi/Batalkan | Detection/recovery duration, draft/operation cursor integrity |
| REC-010 | Device restart/sleep/wake | SQLite/artifact/checkpoint konsisten; trusted time/offline policy tidak diperpanjang salah | Startup scan duration, state loss/corruption, queue isolation |
| REC-011 | SQL unavailable sebelum server mutation/Audit intent | Fail closed/safe unavailable; tidak ada mutation tanpa intent | Response, transaction/effect count, alert/health |
| REC-012 | SQL unavailable setelah owner outbox commit | Owner state/intent tetap; dispatcher retry setelah pulih | Backlog age/drain, duplicate event/job count |
| REC-013 | Object storage unavailable saat upload/finalization/download | Tidak ada final result palsu; metadata/tombstone/reconcile aman | Orphan/metadata mismatch, retry/recovery duration |
| REC-014 | Disk/temp test volume penuh atau write gagal | Preflight/INSUFFICIENT_STORAGE atau safe failure; no system-drive fill; source tetap | Free space guard, temp cleanup, source/target digest |
| REC-015 | Temp rename/publish gagal setelah output staging | Target lama tetap; staged output non-final; reconciliation intent | Half-final count=0, cleanup/retry |
| REC-016 | Internal provider timeout/busy | Bounded retry/circuit/backpressure; safe error; no public fallback | Timeout/retry duration, queue, provider concurrency |
| REC-017 | Engine transient exception | Retry hanya allowlisted dan bounded; attempt baru sah | Attempt count/backoff, source/output integrity |
| REC-018 | Engine permanent/security/validation exception | No automatic hot retry; failed safe; partial output unavailable | Retry count, final state/error sanitation |
| REC-019 | Crash tepat setelah checkpoint commit | Resume memakai checkpoint yang cocok | Resume start stage, reprocessed work, result equality |
| REC-020 | Checkpoint incompatible/corrupt/expired | 409/410 sesuai contract; tidak silent restart/resume | Decision, source safety, allowed recovery action |
| REC-021 | Cancel saat safe point dan saat non-cancellable | Cooperative cancel atau 409; terminal tidak berubah setelah complete | Cancel latency, resource release, output state |
| REC-022 | Offline Audit backlog setelah reconnect | Auth refresh lalu bounded idempotent batch sync | Drain rate, duplicate/conflict, occurred/received preservation |
| REC-023 | Audit append commit tetapi acknowledgement hilang | Replay disimpan sekali | Event uniqueness dan retry latency |
| REC-024 | Cleanup/archive/purge restart | Lanjut dari checkpoint; tidak menghapus sebelum verify/hold check | Checkpoint/recovery, source evidence, lock/I/O |
| REC-025 | Backup/restore test | Schema, migration, permissions, outbox/idempotency, SQL-object reconciliation, append capability valid | Measured restore duration, validation failures, orphan report |

### 10.1 Recovery evidence

Untuk setiap kasus catat:

- last durable state dan injection timestamp;
- expected versus actual state transition;
- measured detection, failover/restart, reconcile, resume, drain, dan user-visible recovery duration;
- committed effect lost, duplicated, corrupted, exposed, atau orphaned;
- attempt/lease/idempotency/checkpoint/outbox consistency;
- source/target/artifact digest;
- queue/temp/storage/connection/resource sebelum, selama, dan setelah pemulihan;
- safe error, health, alert, Job ID, dan Correlation ID;
- manual action yang dibutuhkan.

RPO/RTO resmi belum ditetapkan. Laporan hanya mencatat MEASURED_BASELINE recovery point/duration dan mengusulkan TARGET_TO_BE_APPROVED; tidak menyebutnya SLA.

## 11. Failure injection

### 11.1 Prinsip

Fault injection tidak membutuhkan produk/vendor chaos khusus. Gunakan dua level:

1. **Deterministic component fault:** boundary abstraction/test adapter mengembalikan timeout, transient/permanent exception, cancellation, partial response, atau ambiguous outcome pada titik yang dipilih.
2. **Real process/infrastructure interruption:** hentikan process test, putuskan jaringan test, hentikan endpoint SQL/storage test, atau buat volume test gagal secara terkendali.

Level pertama membuktikan cabang recovery dengan presisi; level kedua membuktikan persistence, IPC, OS, network, DB, dan deployment behavior nyata. Keduanya harus dibedakan dalam evidence.

### 11.2 Fault catalog

| Fault | Metode tanpa vendor khusus | Titik injeksi |
|---|---|---|
| Timeout | Test boundary menahan response sampai deadline/cancellation; endpoint test dapat dibuat lambat secara terkontrol | Sebelum acceptance, saat engine/provider, storage, SQL command, download |
| Network failure | Nonaktifkan koneksi/interface/rute pada client/host test atau hentikan endpoint test sesuai runbook | Upload chunk, response setelah commit, SignalR, provider, download |
| Process crash | Terminasi paksa process Local Worker, API, atau Server Worker di environment test | Setelah claim, sebelum/akhir checkpoint, saat stage output, setelah commit sebelum ack |
| Storage exception | Test storage adapter melempar transient/permanent exception; endpoint/volume test dibuat unavailable/read-only sesuai runbook | Put staging, finalize/promote, read/download, delete/cleanup |
| Database exception | Test repository/connection boundary melempar timeout/deadlock/transient; pada component environment hentikan SQL test atau gunakan skenario lock terkontrol | Begin/commit owner transaction, claim, progress, terminal, outbox publish |
| Engine exception | Adapter test mengembalikan transient/permanent/cancel/OOM-like safe result atau process engine dihentikan | Parse, render, OCR page batch, conversion, output validation |
| Partial output | Hentikan engine/storage setelah sejumlah byte/item atau sebelum digest/validation/final rename | Temp generation, multi-file batch, publish |
| Disk full/temp failure | Gunakan dedicated quota-limited test volume/path, bukan system/workspace root; cap/fail write pada threshold | Preflight, checkpoint, staging, finalization |
| Lost acknowledgement | Drop response setelah server commit | Upload chunk, command 202, Audit batch, outbox publish |
| Clock/time anomaly | Test clock mengubah elapsed/wall facts; device test menjalani sleep/wake/timezone sesuai runbook | Offline grant, lease, retry, expiry, checkpoint |

### 11.3 Injection points

Setiap owner transaction/pipeline penting minimal diuji:

    sebelum durable write
    selama transaction sebelum commit
    tepat setelah commit sebelum outbox/response acknowledgement
    setelah external staging sebelum metadata finalization
    setelah metadata finalization sebelum artifact promotion/response
    setelah checkpoint write sebelum progress notification
    saat lease habis dan Worker lama kembali
    saat cleanup external delete berhasil tetapi metadata finalize belum selesai

Titik yang sama diuji dengan Idempotency Key, Job ID, Correlation ID, attempt, lease, dan digest yang dapat direkonsiliasi.

### 11.4 Guard keselamatan

- Fault test hanya pada environment non-Production dan data sintetis.
- Target process/endpoint/path/volume diselesaikan dan dicatat eksplisit sebelum tindakan.
- Disk-full test memakai volume/directory test yang dikota, bukan mengisi system drive atau workspace luas.
- Ada precheck free space, backup/recovery point yang relevan, stop condition, dan cleanup/reconciliation setelah run.
- Jangan mematikan monitoring yang diperlukan untuk membuktikan failure.
- Jangan menyimpan credential/content pada script, output, dump, atau test artifact.
- Setiap injection memiliki owner, blast radius, rollback/recovery command, timeout pengaman, dan approval environment.
- Test runner tidak boleh menganggap exception yang diinjeksi sebagai PASS sebelum state/data invariant diverifikasi.

## 12. Kinerja database

### 12.1 Dataset states

Uji database pada:

- empty/control;
- pilot-representative;
- grown hot data sesuai retention kandidat;
- history/Audit/Job/Issue/secured metadata dengan skew realistis;
- backlog/outbox/idempotency/expired lifecycle candidates;
- archive/cleanup state hanya bila feature lifecycle tersebut diaktifkan.

Ukuran dataset adalah MEASURED_BASELINE atau TARGET_TO_BE_APPROVED, bukan angka yang diasumsikan.

### 12.2 Query matrix

| ID | Query/path | Pemeriksaan |
|---|---|---|
| DB-PERF-001 | Job/Activity Center owner cursor | Keyset seek, narrow projection, max page, no deep offset/count |
| DB-PERF-002 | History owner cursor | Scope first, stable time+tie-breaker, bounded reads |
| DB-PERF-003 | Secured file/version list | Owner index, max-three access, no payload/storage key projection |
| DB-PERF-004 | Issue reporter/support list | Scope/status/category/selectivity, message/note excluded |
| DB-PERF-005 | Audit actor/Feature/outcome/Process cursor | Narrow approved index, scope before count, no JSON scan |
| DB-PERF-006 | Job/Correlation/exact opaque ID lookup | Selective lookup dan authorization path |
| DB-PERF-007 | Worker queue claim | Filtered hot subset, atomic bounded claim, no duplicate |
| DB-PERF-008 | Lease recovery | Expiry index, bounded reclaim |
| DB-PERF-009 | Idempotency lookup/storm | Unique scoped key hash, collision/replay bounded |
| DB-PERF-010 | Upload session/resume/expiry | Owner/state/time/offset lookup dan cleanup |
| DB-PERF-011 | Cleanup candidate | State+expiry ordered batch; active lease/hold skipped |
| DB-PERF-012 | Broad export | Async bounded snapshot; tidak memblokir interactive query |

Untuk setiap query ukur execution plan, seek/scan, estimated-versus-actual rows, logical/physical reads, CPU, duration, memory grant/spill, wait, lock/block/deadlock, tempdb, returned rows/bytes, dan plan stability pada parameter distribution yang relevan.

### 12.3 Write dan transaction paths

Uji:

- atomic Job admission: idempotency, Job, attempt, reservation, outbox;
- multi-worker claim/lease/heartbeat/progress coalescing/terminal;
- upload finalize dan secured version max-three;
- Issue submit/triage dan Audit intent;
- Audit batch ingestion dan duplicate/conflict;
- outbox dispatch/reclaim/cleanup;
- lifecycle small-batch claim/finalize.

Assertion:

- transaksi singkat dan tidak melintasi external work;
- unique/rowversion/lease menangani race, bukan read-before-write;
- deadlock/transient retry bounded serta idempotent;
- tidak ada NOLOCK shortcut;
- progress write tidak menjadi storm;
- append/internal key/index tidak menghasilkan write amplification tak terukur.

### 12.4 Koneksi dan pool

- Tetapkan total connection budget lintas Web/API/Worker/export/cleanup dari konfigurasi environment.
- Naikkan concurrency hingga target kandidat dan pressure zone; ukur active connection, checkout wait, command timeout, pool exhaustion, serta recovery.
- Suntik slow SQL dan external engine/storage delay; pastikan connection dilepas sebelum delay external.
- Periksa cancellation benar-benar menghentikan/menutup command dan mengembalikan connection.
- Verifikasi retry tidak membuka koneksi selama backoff.

Nilai pool final adalah POLICY_LIMIT yang harus selaras dengan SQL capacity, bukan sekadar default framework.

### 12.5 Maintenance, cleanup, backup, restore

- Jalankan index/statistics maintenance yang diusulkan hanya setelah pengukuran; jangan memakai rebuild-all sebagai baseline.
- Jalankan cleanup/archive/export di bawah foreground load untuk mengukur throttle dan starvation.
- Ukur log growth dan recovery setelah bounded purge/cleanup.
- Uji backup dan restore lengkap beserta schema/migration, owner permission, outbox/idempotency, serta SQL-object reconciliation.
- Jika READ_COMMITTED_SNAPSHOT dipertimbangkan, bandingkan blocking versus tempdb/version-store cost pada representative load dan minta review DBA; jangan mengaktifkannya hanya dari asumsi.

### 12.6 Database pass invariants

- primary cursor query menggunakan plan/index pendukung pada data representatif;
- scope otorisasi diterapkan sebelum count/projection;
- tidak ada unbounded Include, N+1, client evaluation, arbitrary sort/search, atau large IN;
- no long transaction/connection across external work;
- duplicate logical object/event tetap nol;
- blocking/deadlock recovery berada pada TARGET_TO_BE_APPROVED setelah disetujui;
- cleanup throughput melebihi sustained eligible growth;
- Query Store/trace/test artifact tidak mengandung restricted content.

## 13. Dampak Audit dan logging

### 13.1 Audit overhead

Mandatory Audit tetap aktif pada seluruh release-pass benchmark. Ukur:

- waktu validasi/mapping Audit intent;
- tambahan durasi transaksi owner untuk satu outbox row;
- latency accepted/terminal action dengan intent wajib;
- rows dan encoded bytes per action;
- log bytes/index write per event;
- dispatcher/ingestion batch throughput dan latency;
- duplicate/conflict/reject rate;
- local SQLite outbox latency/growth;
- backlog oldest age dan drain setelah outage/burst;
- query/export/archive/purge resource cost;
- dampak pada representative Merge/OCR/conversion throughput dan UI.

Diagnostic microbenchmark boleh mengisolasi mapper/outbox cost pada lab, tetapi hasil tanpa mandatory Audit tidak boleh menjadi release PASS, target user, atau alasan menonaktifkan Audit.

### 13.2 Event-volume guard

- Satu owner outbox/event untuk milestone accepted/terminal/security yang disetujui.
- Jangan membuat Enterprise Audit per progress, page, chunk, heartbeat, preview, reorder, atau navigation.
- Uji 33 Core tools menghasilkan L2 accepted/terminal; high-risk menambah L3 sesuai Document 10.
- Uji replay yang sama menghasilkan satu event; conflict terlihat tanpa overwrite.
- Uji offline backlog sync tidak menghambat UI dan menggunakan bounded batches/backoff.

Hitung events per logical action dan bandingkan dengan Event Catalog. Kenaikan tanpa perubahan catalog adalah regression.

### 13.3 Logging/APM

Ukur:

- log/trace/metric events per request/Job dan bytes/time;
- serialization/export CPU dan allocation;
- buffer/drop/backpressure/error pada telemetry pipeline;
- cardinality Feature/Job/Correlation/error tags;
- APM/network overhead dan perilaku saat backend observability lambat/unavailable;
- log rotation/retention test growth jika ada local log.

Observability failure tidak boleh memblokir safe local work atau menyebabkan recursive failure storm, tetapi mandatory Audit durability tetap mengikuti kelas L2/L3. Canary Document 11 harus tetap absen pada log, trace, metric, alert, query capture, dump, dan performance artifact.

### 13.4 Perbandingan

Bandingkan build terhadap baseline terakhir yang kompatibel dengan:

- workload/corpus/environment/policy/instrumentation yang sama;
- per-stage latency dan resource per operation;
- Audit rows/bytes/action;
- log/APM volume/action;
- queue/backlog/drain;
- DB reads/writes/log bytes.

Perbedaan konfigurasi atau catalog harus dilaporkan sebagai faktor, bukan dianggap regresi/improvement murni.

## 14. Metodologi baseline

### 14.1 Prosedur run

1. Tentukan pertanyaan dan invarian skenario.
2. Bekukan build/configuration/policy/version dan dokumentasikan topology.
3. Inventaris CPU/RAM/disk/network/SQL/storage/device serta background services.
4. Pilih corpus versioned dan simpan manifest/digest, bukan isi sensitif pada laporan.
5. Precondition database, cache, queue, temp, dan storage ke state yang ditentukan.
6. Sinkronkan clock test dan aktifkan korelasi aman.
7. Verifikasi generator/harness mampu menghasilkan load di atas kebutuhan tanpa menjadi bottleneck.
8. Jalankan control/smoke untuk correctness dan counter completeness.
9. Jalankan warm-up sesuai state yang dituju.
10. Jalankan measurement dengan arrival/concurrency/duration manifest yang tetap.
11. Verifikasi output/source/idempotency/Audit/state invariant sambil load berjalan.
12. Hentikan load baru dan ukur drain/recovery.
13. Ulangi sampai variansi/confidence yang disetujui tercapai; jangan memilih run terbaik.
14. Simpan raw aggregate aman, summary, query plans aman, failure timeline, dan environment manifest.
15. Bandingkan dengan baseline kompatibel dan ajukan target/policy decision.

Jumlah pengulangan dan confidence threshold adalah TARGET_TO_BE_APPROVED berdasarkan variansi aktual; angka tersebut tidak diarang di laporan ini.

### 14.2 Cold, warm, dan grown state

Laporkan terpisah:

- cold start/process/JIT/connection/cache;
- warm steady state;
- restart/recycle;
- grown DB/storage/queue;
- post-failure dan post-soak.

Jangan mencampur cold dan warm samples dalam satu percentile tanpa label.

### 14.3 Statistik

- tampilkan sample count, measurement duration, offered/accepted/completed count;
- gunakan percentile untuk latency/duration dan peak+slope untuk resource/growth;
- tampilkan error/retry/reject/duplicate sebagai rate dan absolute count;
- gunakan confidence interval/variability bila jumlah sampel memadai;
- bedakan coordinated omission pada generator; offered load dan completion harus keduanya dicatat;
- tandai censored operation yang belum selesai saat window berakhir;
- sertakan longest tail/outlier dengan Correlation ID aman untuk analisis.

### 14.4 Performance budget sheet

Setiap Feature/workload yang diuji memiliki:

| Field | Isi |
|---|---|
| Feature/Workload ID | Canonical Feature ID atau infrastructure workload |
| Execution profile | L1/H1/L-SYS/SYS-H/E1/SEC1/AI-H/AI-S sesuai catalog |
| Route/environment | Local/Local Worker/Server/Provider dan named environment |
| Input/corpus band | CONTROL/TYPICAL/LARGE_SUPPORTED/POLICY_BOUNDARY |
| Concurrency/load | User/request/job/stream mix |
| Metric | Latency, throughput, CPU, RAM, temp, SQL, network, queue, growth, recovery |
| MEASURED_BASELINE | Observed distribution + run ID |
| TARGET_TO_BE_APPROVED | Proposed acceptance + rationale/owner/status |
| POLICY_LIMIT | Enforced bound + policy version |
| Headroom | Distance target kandidat ke measured knee/safety limit |
| Result | PASS/FAIL/INCONCLUSIVE/BLOCKED_PENDING_TARGET_APPROVAL |
| Finding/action | Bottleneck, regression, scale/throttle/config recommendation |

### 14.5 Rebaseline

Baseline baru diperlukan bila terjadi perubahan material pada engine/provider/version, runtime, OS, database schema/index/query, Feature/schema, compression/encryption, hardware/topology, storage/network, policy limit, Audit catalog, observability, corpus, atau deployment configuration. Baseline lama tetap disimpan sebagai histori dan tidak ditimpa.

## 15. Pendekatan pass/fail

### 15.1 Status hasil

| Status | Definisi |
|---|---|
| PASS | Seluruh invarian berlaku dan seluruh target numerik yang sudah disetujui terpenuhi pada run valid |
| FAIL | Minimal satu invarian rusak atau target numerik resmi tidak terpenuhi |
| INCONCLUSIVE | Run/counter/generator/environment/data tidak valid atau bukti tidak cukup |
| BLOCKED_PENDING_TARGET_APPROVAL | Baseline valid tersedia tetapi target numerik resmi belum disetujui |
| BLOCKED_ENVIRONMENT | Prasyarat environment/provider/engine/permission/instrumentation resmi belum tersedia |

BLOCKED bukan PASS terselubung. FAIL tidak boleh diubah menjadi PASS dengan menurunkan instrumentation, menonaktifkan Audit/security, memperkecil corpus tanpa alasan, atau menghapus Feature.

### 15.2 Gate nonnumerik langsung

P0 immediate fail:

- source/target corruption atau silent overwrite;
- unauthorized/cross-user content access;
- duplicate logical Job/upload/Audit/secured version/effect;
- 202 sebelum durable acceptance;
- final state/output sebelum validation;
- terminal-state regression;
- Worker crash membunuh UI;
- SignalR menjadi source of truth atau disconnect membatalkan Job;
- whole-file memory load pada jalur yang wajib streaming;
- connection/transaction ditahan selama external/engine wait;
- mandatory Audit hilang/dinonaktifkan;
- sensitive-data canary bocor;
- unbounded queue/retry/hot loop;
- server overload menyebabkan silent failure atau public-provider fallback;
- cleanup menghapus leased/held/active resource.

### 15.3 Gate numerik

Setelah target disetujui, pass/fail dinilai per environment/workload pada:

- UI/API/Job/queue/SignalR latency distribution;
- throughput dan accepted/completed rate;
- CPU/RAM/Worker memory/GC;
- disk/temp/storage/network;
- SQL I/O/log/tempdb/query/connection/blocking;
- queue depth/age/drain/fairness;
- Audit write/ingestion/query/export impact;
- storage/log/index growth dan cleanup margin;
- recovery detection/restart/reconcile/resume/drain duration;
- soak residual slope.

Satu percentile atau rata-rata tidak cukup. Gate harus menyebut measurement window, sample minimum/confidence, allowed error/retry/reject classes, serta apakah target berlaku pada TYPICAL, LARGE_SUPPORTED, target load, atau boundary.

### 15.4 Aturan suite

| Suite | Kelulusan minimum |
|---|---|
| Performance | Baseline repeatable; output benar; target approved terpenuhi; tidak ada resource anomaly |
| Load | Target workload stabil; queue/growth bounded; backpressure belum prematur; correctness lulus |
| Concurrency | Seluruh invariant state/uniqueness/ownership lulus pada race yang benar-benar terjadi |
| Stress | Safe wait/reject sebelum exhaustion; accepted state konsisten; pulih setelah load turun |
| Soak | Tidak ada leak/drift/orphan/backlog tak terbatas; cleanup/drain efektif |
| Backpressure | Status/WAITING state/Retry-After tepat, visible, fair, reservation bersih |
| Recovery | Tidak ada committed effect hilang/duplikat/corrupt; state/source/output pulih sesuai contract dan target |
| Database | Plan/query/transaction/pool/cleanup memenuhi target; scope dan keamanan tetap benar |
| Audit/logging | Mandatory evidence lengkap, bounded, dan aman; overhead memenuhi target; no sensitive leakage |

### 15.5 Approval target dan keputusan kapasitas

Target disetujui oleh pemilik bisnis/UX, Architecture, Infrastructure/Operations, DBA, Security, dan owner workload yang relevan. Approval mencatat environment, workload, percentile/window, headroom, policy version, tanggal, dan pemilik perubahan.

Jika server kecil/pilot tidak memenuhi target:

1. verifikasi test/corpus/configuration dan bottleneck;
2. optimalkan query/projection/batching/streaming/concurrency dalam arsitektur terkunci;
3. sesuaikan POLICY_LIMIT/backpressure hanya dalam hard safety dan setelah approval;
4. ukur ulang;
5. bila perlu gunakan opsi scale-up/pemisahan deployable yang sudah didukung architecture;
6. catat APPROVED_WITH_LIMITATION atau BLOCKED_ALTERNATIVE_REQUIRED/DEFERRED sesuai governance.

Jangan menghapus Feature resmi hanya karena kapasitas environment awal.

## 16. Risiko

| Risiko/keputusan terbuka | Dampak | Tindak lanjut |
|---|---|---|
| SLA/target latency, throughput, concurrency, recovery, dan availability Pertamina belum resmi | Pass/fail numerik tidak dapat dikunci | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** setelah MEASURED_BASELINE |
| Hardware/topology/pool/storage/network environment pilot belum dibekukan | Baseline tidak dapat dibandingkan | Buat environment manifest dan configuration freeze |
| Populasi, workload mix, arrival pattern, dan corpus pilot belum terukur | Test dapat terlalu ringan/berat | Inventaris bisnis/pilot dan versioned synthetic corpus |
| Engine/provider final beberapa conversion/OCR/AI masih PoC | Resource profile dapat berubah material | Baseline per approved engine/version; rebaseline setelah keputusan |
| Internal AI/provider capacity/timeout/retention belum final | Test provider busy/fallback belum lengkap | Approval provider dan controlled test endpoint; public fallback tetap dilarang |
| RPO/RTO dan restore objective belum disetujui | Recovery hanya menghasilkan measured duration | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** dan DBA/Operations approval |
| Reauthentication/SSO eksternal dapat menjadi bottleneck di luar kontrol OTO | Load test dapat membebani identity service secara tidak sah | Gunakan approved stub/test tenant/rate; real IdP load hanya dengan izin |
| Retention/hold/archive duration belum final | Storage-growth/cleanup horizon belum final | Gunakan scenario bands dan re-run setelah policy Document 10 disetujui |
| Fault-injection seams/runbook belum tersedia | Beberapa race sulit dibuat repeatable | Definisikan test-only controlled hooks/boundaries tanpa production bypass |
| Server awal menggabungkan banyak deployable | Noisy-neighbor menyulitkan atribusi | Counter per process + host, isolated runs, lalu mixed contention |
| APM/telemetry sampling/overhead belum dikalibrasi | Tail/error dapat hilang atau overhead menyesatkan | Ukur instrumentation loss/overhead dan freeze config |
| Generator/load runner menjadi bottleneck | Throughput server terlihat lebih rendah/latency bias | Monitor generator dan gunakan distributed/internal runner bila disetujui; tanpa SaaS baru |
| Test data terlalu sederhana | Parser/memory/query path terburuk tidak muncul | Corpus structurally diverse, adversarial-valid, dan policy-boundary |
| Performance tuning dapat merusak security/correctness | False improvement | P0 correctness/security/Audit gate dijalankan pada setiap benchmark |
| Disk-full/process-stop test dapat merusak host test bila salah scope | Blast radius operasional | Dedicated quota-limited volume, resolved target, stop condition, recovery runbook |
| Mobile/desktop device fragmentation | Satu baseline tidak mewakili semua | Minimum dan representative supported device profiles |
| Background cleanup/export/Audit dapat starvation atau menutupi foreground | Mixed load tidak representatif bila diabaikan | Uji workload bersama dan ukur fairness/progress |
| Numeric policy terlalu dekat dengan measured knee | Instabilitas saat burst/variance | Tetapkan safety headroom melalui approval, bukan nilai maksimum benchmark |

## 17. Suggested Document 12 text

Teks berikut siap dipertimbangkan untuk integrasi ke Document 12.

### 17.1 Model nilai dan governance

Document 12 SHALL membedakan MEASURED_BASELINE, TARGET_TO_BE_APPROVED, dan POLICY_LIMIT.

- MEASURED_BASELINE SHALL selalu terikat pada build, environment, topology, corpus, route, workload, concurrency, state, policy, instrumentation, dan measurement window.
- TARGET_TO_BE_APPROVED SHALL merupakan ambang penerimaan usulan yang baru menjadi gate numerik setelah persetujuan Pertamina.
- POLICY_LIMIT SHALL merupakan hard/typed runtime safety ceiling dan SHALL NOT diperlakukan sebagai target performa.

Tidak adanya target resmi SHALL menghasilkan BLOCKED_PENDING_TARGET_APPROVAL untuk gate numerik, bukan PASS. Invarian security, integrity, Local-First, source safety, idempotency, durable acceptance, dan no-sensitive-data SHALL tetap dapat PASS/FAIL langsung.

### 17.2 Strategi suite

Test Plan SHALL mencakup:

1. performance test untuk biaya satu operation/workflow;
2. load test untuk capacity curve pada beban representatif;
3. concurrency test untuk race/state/lease/idempotency;
4. stress test untuk knee point dan graceful degradation;
5. soak/stability test untuk leak/drift/growth;
6. recovery/resilience test untuk crash/restart/disconnect/reconcile;
7. resource-budget validation untuk setiap execution profile dan workload.

Setiap suite SHALL memvalidasi correctness dan security bersamaan dengan resource metrics. Unit Test hijau SHALL NOT menggantikan bukti performance/infrastructure nyata.

### 17.3 Metrik wajib

Pengukuran SHALL mencakup CPU, process/.NET memory dan GC, disk dan temp storage, SQL logical/physical I/O dan log/tempdb, database connection/pool, API request rate dan p50/p95/p99 latency, network bytes/throughput/retry, Worker peak/residual memory, queue depth/oldest age/arrival/service/drain, Job stage/end-to-end duration, SignalR connection/delivery/reconnect/gap, Audit intent/ingestion/query impact, storage/index/log growth, cleanup throughput, serta UI responsiveness.

Metrik SHALL dikorelasikan menggunakan identifier aman seperti Feature ID, Job ID, dan Correlation ID. Tag/log/report SHALL NOT memuat document/OCR content, password, token, secret, signature, clipboard, prompt/provider payload, screenshot/Issue content, raw path/storage key, atau SQL/exception sensitif.

### 17.4 Workload lokal

Local performance SHALL diuji pada minimum dan representative supported devices menggunakan corpus CONTROL, TYPICAL, LARGE_SUPPORTED, POLICY_BOUNDARY, OVER_LIMIT, dan ADVERSARIAL_VALID.

Skenario SHALL mencakup PDF besar, banyak PDF, gambar besar, OCR, conversion, edit/working-state, multi-step cumulative temp, concurrent local jobs, Audit outbox, dan Local Worker recycle/crash. Pengujian SHALL mengukur UI input/stall/progress responsiveness, process/Worker CPU/RAM/GC/handle, temp/disk, duration, queue, checkpoint, cancellation, dan cleanup.

Local-only execution SHALL membuat nol upload dan nol API processing. Heavy local work SHALL menggunakan Local Worker bila resolver menentukan demikian. Local Worker crash SHALL NOT menjatuhkan UI. Source digest SHALL tetap sama setelah success, cancel, failure, crash, retry, atau recovery.

### 17.5 Workload server dan pilot

Server tests SHALL mencakup concurrent users, streamed/resumable upload, range/full download, background Jobs, metadata/Job/History/Secured/Issue queries, Issue attachment/export, Audit write/query/export/backlog, secured version race, SignalR, cleanup, dan approved internal-provider requests.

Workload mix SHALL berasal dari inventaris/pilot/telemetry aman dan execution profiles, bukan persentase rekaan. Local-only workflows pada client online SHALL diuji sebagai negative server-load case.

Environment kecil/pilot SHALL diuji sesuai deployment aktual. Counter SHALL dipisah per deployable/process dan total host. Hasil SHALL menghasilkan capacity envelope, bottleneck pertama, headroom, dan scale trigger. Kapasitas kecil SHALL ditangani melalui batching, queue, backpressure, concurrency, throttle, cleanup, atau scale/pemisahan yang sudah didukung; Feature resmi SHALL NOT dihapus.

### 17.6 Load, stress, dan soak

Load test SHALL menjalani control, measured baseline, pilot/target candidate, headroom exploration, mixed workload, burst/drain, dan recovery observation. Measurement SHALL dimulai setelah state warm-up/precondition yang dinyatakan dan pola resource/queue dapat diinterpretasikan.

Stress test SHALL menaikkan satu dimensi utama per eksperimen dan SHALL berhenti sebelum membahayakan host/data test. PASS stress berarti overload menghasilkan wait/reject aman, accepted state tetap konsisten, no duplicate/corruption, resource/queue pulih, dan health/telemetry menjelaskan pressure. Breaking point test SHALL NOT langsung menjadi POLICY_LIMIT.

Soak duration SHALL mencakup beberapa siklus Job/checkpoint/lease, retry/backoff, connection/SignalR reconnect, upload/result expiry, outbox, cleanup, Worker recycle, dan lifecycle lain yang diuji. Exact duration SHALL menjadi TARGET_TO_BE_APPROVED. Memory/handle/connection/queue/temp/SQL/log/index/Audit growth slope SHALL dilaporkan; tren residual positif tidak boleh disembunyikan oleh rata-rata.

### 17.7 Backpressure

Admission control SHALL memakai batas paling ketat dari system safety, enterprise policy, environment, subject, endpoint/workload, Worker/storage/database, dan provider.

- caller rate/quota/concurrency SHALL menghasilkan 429 dengan Retry-After;
- hard payload/file/chunk/batch limit SHALL menghasilkan 413;
- storage/state/version conflict SHALL menghasilkan 409;
- shared saturation yang tidak aman untuk queue SHALL menghasilkan 503;
- Job yang telah diterima secara durabel MAY menggunakan WAITING_RESOURCE atau WAITING_CONNECTION sesuai kondisi;
- 202 SHALL hanya dihasilkan setelah Job, attempt, lease/input, idempotency, reservation, dan dispatch marker menjadi durabel.

Backpressure SHALL aktif sebelum CPU, RAM, DB pool, I/O, temp/storage, queue, atau provider exhaustion. State wait/reject SHALL terlihat pada status/Activity Center dan tidak silent. Reservation SHALL dilepas pada reject/cancel/terminal/expiry/cleanup. Public-provider fallback SHALL tetap dilarang.

### 17.8 Recovery dan fault injection

Recovery tests SHALL mencakup SignalR/network disconnect, API/Worker/application/device restart, SQL/storage unavailable, dedicated test-volume full/temp failure, provider timeout/busy, engine transient/permanent exception, lost acknowledgement, partial output, cancel, retry, resume, checkpoint incompatible/expired, Audit backlog/replay, cleanup/archive restart, serta backup/restore.

Fault injection SHALL mencakup titik sebelum durable write, sebelum commit, setelah commit sebelum acknowledgement, setelah external staging, sebelum/selesai finalization, setelah checkpoint, lease expiry, dan cleanup external-delete ambiguity. Idempotency Key, Job ID, Correlation ID, attempt, lease, state version, checkpoint, outbox, dan digest SHALL direkonsiliasi.

Fault tests SHALL berjalan pada environment non-Production dengan data sintetis. Disk-full test SHALL memakai dedicated quota-limited test volume dan SHALL NOT mengisi system drive/workspace root. Setiap run SHALL memiliki resolved target, blast radius, stop condition, recovery/cleanup runbook, dan telemetry.

### 17.9 Database, Audit, dan logging

Database performance SHALL menguji scoped keyset queries, Job worker claim/lease recovery, idempotency storm, upload resume/expiry, secured max-three race, Audit ingestion/query/export, outbox, cleanup under load, connection-pool pressure, deadlock/transient retry, log/tempdb growth, backup, dan restore.

Primary cursor queries SHALL menggunakan supporting seek/index pada data representatif. Scope SHALL mendahului count/projection. Connections/transactions SHALL NOT melintasi file streaming, engine/provider work, queue wait, SignalR, atau retry delay. Cleanup throughput SHALL melebihi sustained eligible growth.

Mandatory Audit SHALL tetap aktif pada release-pass benchmarks. Ukur intent latency, owner-transaction delta, events/rows/bytes per action, SQL/log/index growth, ingestion/backlog/drain, query/export/archive cost, dan representative user-job impact. Progress/page/chunk/heartbeat SHALL NOT menciptakan Audit write storm.

Logging/APM overhead, volume, cardinality, buffering/drop, dan backend-unavailable behavior SHALL diukur. Canary sensitive-data Document 11 SHALL tetap absent dari seluruh performance/recovery artifacts.

### 17.10 Baseline dan pass/fail

Setiap run SHALL menyimpan environment/configuration manifest, corpus digest/version, policy/limit version, load model, warm/cold/grown state, sample count/window, offered/accepted/completed/rejected operations, percentile/resource/growth/recovery metrics, output/source/idempotency/Audit verification, dan failure timeline.

Run SHALL diulang sampai variability/confidence rule yang disetujui terpenuhi dan SHALL NOT memilih hasil terbaik. Cold, warm, restart, grown, post-failure, dan post-soak samples SHALL dilaporkan terpisah.

Status SHALL menggunakan PASS, FAIL, INCONCLUSIVE, BLOCKED_PENDING_TARGET_APPROVAL, atau BLOCKED_ENVIRONMENT. Source corruption, unauthorized access, duplicate effect, premature 202/final output, terminal regression, Worker-to-UI crash propagation, unbounded queue/retry, whole-file buffering pada jalur streaming, mandatory Audit loss, sensitive-data leakage, dan cleanup atas active/held resource SHALL menjadi immediate failure.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** target latency/throughput/concurrency/resource/recovery/availability, percentile/window/confidence, pilot workload/corpus, environment specification, RPO/RTO, policy limits/headroom, serta approval owner. Tidak ada nilai tersebut yang boleh disebut SLA sebelum disetujui resmi.
